using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Gensearch.Helpers;
using SQLite;

namespace Gensearch.Scrapers
{
    public class BladeWeapons
    {
        SQLiteAsyncConnection db = GenSearch.db;
        string[] spblade_weapons = {"switchaxe", "chargeblade", "gunlance"};

        /// <summary>
        /// Retrieves weapon information for all blademaster weapons.
        /// </summary>
        /// <param name="address">The URL of the weapon.</param>
        /// <param name="notes">If the weapon is a hunting horn, this is the int array containing the note information.</param>
        public async Task GetBlademasterWeapon(string address, int[] notes = null) {
            try {
                var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
                var context = BrowsingContext.New(config);
                var page = await context.OpenAsync(address);
                string[] flav = Weapons.GetFlavorText(page);

                var crafting_table = page.QuerySelectorAll(".table")[1].QuerySelector("tbody");
                int current_wpn_index = 0;
                foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr")) {

                    SwordValues sv = await GetSwordAttributes(page, tr, crafting_table, current_wpn_index);
                    List<SharpnessValue> sharpvalues = GetSharpness(tr);
                    await db.InsertAllAsync(sharpvalues);
                    sv.sharp_0_id = sharpvalues[0].sharp_id;
                    sv.sharp_1_id = sharpvalues[1].sharp_id;
                    sv.sharp_2_id = sharpvalues[2].sharp_id;
                    sv.sword_set_name = flav[0];
                    sv.description = sv.sword_name.Contains(flav[0]) ? flav[2] : flav[3];
                    bool already_inserted = false;
                    if (address.Contains("/greatsword/")) { sv.sword_class = "Great Sword"; }
                    else if (address.Contains("/longsword/")) { sv.sword_class = "Long Sword"; }
                    else if (address.Contains("/swordshield/")) { sv.sword_class = "Sword & Shield"; }
                    else if (address.Contains("/hammer/")) { sv.sword_class = "Hammer"; }
                    else if (address.Contains("/lance/")) { sv.sword_class = "Great Sword"; }
                    else if (address.Contains("/insectglaive/")) {sv.sword_class = "Insect Glaive"; }
                    else if (address.Contains("/dualblades/")) {sv.sword_class = "Dual Blades";}
                    else if (spblade_weapons.Any(b => address.Contains(b))) {
                        if (address.Contains("/chargeblade/")) { sv.sword_class = "Charge Blade"; }
                        else if (address.Contains("/switchaxe/")) { sv.sword_class = "Switch Axe"; }
                        else if (address.Contains("/gunlance/")) { sv.sword_class = "Gunlance"; }
                        await db.InsertAsync(sv);
                        already_inserted = true;

                        string phialtype = GetPhialType(tr, sv.sword_class);
                        PhialOrShellWeapon weapon = new PhialOrShellWeapon() {
                            sword_id = sv.sword_id,
                            phial_or_shell_type = phialtype
                        };
                        await db.InsertAsync(weapon);
                    }
                    else if (address.Contains("/huntinghorn/") && notes != null) {
                        sv.sword_class = "Hunting Horn";
                        await db.InsertAsync(sv);
                        already_inserted = true;

                        HuntingHorn horn = GetHuntingHorn(notes, sv.sword_id);
                        await db.InsertAsync(horn);
                    }
                    if (!already_inserted) { 
                        try {
                            await db.InsertAsync(sv);
                        }
                        catch (SQLiteException){
                            ConsoleWriters.ErrorMessage($"{sv.sword_name} is already in the database!");
                        }
                    }
                    

                    List<CraftItem> craftitems = Weapons.GetCraftItems(crafting_table.Children[current_wpn_index]);
                    foreach (CraftItem item in craftitems) {
                        item.creation_id = sv.sword_id;
                        item.creation_type = "Blademaster";
                    }
                    foreach (ElementDamage element in sv.element) {
                        element.weapon_id = sv.sword_id;
                    }
                    await db.InsertAllAsync(sv.element);
                    await db.InsertAllAsync(craftitems);
                    current_wpn_index++;
                }
            }
            catch (Exception ex) {
                ConsoleWriters.ErrorMessage(ex.ToString());
                await GetBlademasterWeapon(address, notes);
            }
        }

        /// <summary>
        /// Retrieves note information for hunting horns.
        /// </summary>
        /// <param name="notes">An array of ints corresponding to the horn's note values.</param>
        /// <param name="sword_id">The ID of the hunting horn in the database.</param>
        /// <returns>Returns a hunting horn object.</returns>
        public HuntingHorn GetHuntingHorn(int[] notes, int sword_id) {
            string notestring = "";
            foreach (int note in notes) {
                switch(note) {
                    case 1:
                        notestring += "white ";
                        break;
                    case 2:
                        notestring += "purple ";
                        break;
                    case 3:
                        notestring += "red ";
                        break;
                    case 4:
                        notestring += "blue ";
                        break;
                    case 5:
                        notestring += "green ";
                        break;
                    case 6:
                        notestring += "yellow ";
                        break;
                    case 7:
                        notestring += "light_blue ";
                        break;
                }
            }
            notestring = notestring.Trim().Replace(" ", ", ");
            HuntingHorn horn = new HuntingHorn() {
                sword_id = sword_id,
                notes = notestring
            };
            return horn;
        }


        /// <summary>
        /// Gets general weapon information. Blademaster only.
        /// </summary>
        /// <param name="page">The IDocument holding the page information.</param>
        /// <param name="wrapper">The table row element holding the weapon information.</param>
        /// <param name="crafting">The table containing information on price, upgrades, and items.</param>
        /// <param name="current_index">The index of the wrapper element in its parent table.</param>
        /// <returns>Returns a SwordValues object containing the retrieved information.</returns>
        public async Task<SwordValues> GetSwordAttributes(IDocument page, IElement wrapper, IElement crafting, int current_index) {
            string weapon_name = wrapper.FirstElementChild.TextContent;
            int weapon_damage = Convert.ToInt32(wrapper.Children[1].TextContent);
            var techinfo = wrapper.Children[5];
            int slots = techinfo.FirstElementChild.TextContent.Count(c => c == '◯');
            int rarity = Convert.ToInt32(techinfo.Children[1].TextContent.Trim().Replace("RARE", ""));
            string upgrades_into = "none";
            var upgradeinfo = crafting.Children[current_index].QuerySelectorAll("td");
            if (upgradeinfo[0].QuerySelector(".font-weight-bold") != null) {
                upgrades_into = String.Join(" & ", upgradeinfo[0].QuerySelectorAll("a").Select(a => a.TextContent.Trim()));
            }
            List<ElementDamage> elements = new List<ElementDamage>();
            int affinity = 0;
            foreach (var small in wrapper.Children[2].QuerySelectorAll("small")) {
                if (small.TextContent.Any(char.IsLetter)) {
                    elements.Add(Weapons.GetElement(small));
                }
                else {
                    affinity = small.TextContent.Trim().ToInt();
                }
            }
            int price = upgradeinfo[1].TextContent.Replace("z", "").ToInt();
            int monsterid = -1;
            if (page.QuerySelectorAll(".lead").Count() == 3) {
                monsterid = (await Monsters.GetMonsterFromDB(page.QuerySelectorAll(".lead")[2].TextContent.Trim())).id;
            }
            return new SwordValues() {
                sword_name = weapon_name,
                raw_dmg = weapon_damage,
                slots = slots,
                rarity = rarity,
                upgrades_into = upgrades_into,
                price = price,
                element = elements,
                affinity = affinity,
                monster_id = monsterid,
            };
        }

        /// <summary>
        /// Retrieves weapon sharpness information. Blademaster only.
        /// </summary>
        /// <param name="wrapper">The table row element holding the weapon information.</param>
        /// <returns>
        /// <para>Returns a list of SharpnessValue objects.</para>
        /// <para>Index zero is the base weapon sharpness, index one is the sharpness with the skill Sharpness+1, and index two
        /// is the sharpness with Sharpness+2.</para>
        /// </returns>
        public List<SharpnessValue> GetSharpness(IElement wrapper) {
            List<SharpnessValue> values = new List<SharpnessValue>();
            var sharpvalues = wrapper.Children[3].QuerySelectorAll("div");
            for (int i = 0; i <= 2; i++) {
                var spans = sharpvalues[i].QuerySelectorAll("span");
                int red_sharpness = spans[0].Style.Width.ToInt() * 5;
                int orange_sharpness = spans[1].Style.Width.ToInt() * 5;
                int yellow_sharpness = spans[2].Style.Width.ToInt() * 5;
                int green_sharpness = spans[3].Style.Width.ToInt() * 5;
                int blue_sharpness = spans[4].Style.Width.ToInt() * 5;
                int white_sharpness = spans[5].Style.Width.ToInt() * 5;
                values.Add(new SharpnessValue() {
                    handicraft_modifier = i,
                    red_sharpness_length = red_sharpness,
                    orange_sharpness_length = orange_sharpness,
                    yellow_sharpness_length = yellow_sharpness,
                    green_sharpness_length = green_sharpness,
                    blue_sharpness_length = blue_sharpness,
                    white_sharpness_length = white_sharpness
                });
            }
            return values;
        }

        /// <summary>
        /// Get the phial and/or shell name from a weapon. For charge blades, switch axes, and gunlances.
        /// </summary>
        /// <param name="wrapper">The table row element containing the weapon information.</param>
        /// <param name="type">The type of weapon.</param>
        /// <returns>Returns a string with the phial type.</returns>
        public string GetPhialType(IElement wrapper, string type) {
            var phialinfo = wrapper.Children[4].TextContent;
            if (type == "Gunlance") {return phialinfo.Trim(); }
            else { return phialinfo.Split(':')[1].Trim(); }
        }
    }
}