using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Events;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Gensearch.Helpers;
using SQLite;

namespace Gensearch.Scrapers
{
    public class Weapons
    {
        SQLiteAsyncConnection db = GenSearch.db;
        Regex intsOnly = new Regex(@"[^\d\+-]"); 

        /// <summary>
        /// Iterates through all the weapon URLS on a page and feeds each one into the database.
        /// </summary>
        /// <param name="addr">The address to pull from.</param>
        public async Task GetWeapons(string addr) {
            int throttle = 3;
            string[] special_weapons = new string[] {"/gunlance", "/chargeblade", "/switchaxe", "/lightbowgun", "/heavybowgun", "/bow", "/huntinghorn"};
            await db.CreateTablesAsync<SwordValues, SharpnessValue, ElementDamage, CraftItem, HuntingHorn>();
            try {
                List<Task> tasks = new List<Task>();
                var config = Configuration.Default.WithDefaultLoader().WithJavaScript().WithCss();
                var context = BrowsingContext.New(config);
                var page = await context.OpenAsync(addr);
                int page_length = Convert.ToInt32(page.ExecuteScript("window[\"mhgen\"][\"weapons\"].length"));
                if (!special_weapons.Any(w => addr.Contains(w))) {
                    for (int i = 0; i < page_length; i++) {
                        string address = (string) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i.ToString()}].url");
                        tasks.Add(GetGenericSword(address));

                        if (tasks.Count == throttle) {
                            Task completed = await Task.WhenAny(tasks);
                            tasks.Remove(completed);
                        }
                    }
                    await Task.WhenAll(tasks);
                }
                else if (addr.Contains("/huntinghorn")) {
                    for (int i = 0; i < page_length; i++) {
                        string address = (string) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i.ToString()}].url");
                        int[] notes = new int[] {
                            Convert.ToInt32((double) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i}][\"levels\"][0][\"hhnotes\"][0][\"color_1\"]")),
                            Convert.ToInt32((double) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i}][\"levels\"][0][\"hhnotes\"][0][\"color_2\"]")),
                            Convert.ToInt32((double) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i}][\"levels\"][0][\"hhnotes\"][0][\"color_3\"]")),
                        };
                        tasks.Add(GetHuntingHorn(address, notes));

                        if (tasks.Count == throttle) {
                            Task completed = await Task.WhenAny(tasks);
                            tasks.Remove(completed);
                        }
                    }
                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Error getting page. Pausing for sixty seconds.");
                Thread.Sleep(60000);
                
            }
        }

        /// <summary>
        /// Retrieves weapon information for greatswords, longswords, sword and shields, hammers, lances, and insect glaives.
        /// <para>The following weapons are excluded because of special characteristics:</para>
        ///     <para/>
        ///     <para/>Switch Axe / Charge Blade - Phials
        ///     <para/>LBG / HBG / Bow - Ammo
        ///     <para/>Gunlance - Shells
        ///     <para/>Hunting Horn - Songs
        /// </summary>
        /// <param name="address">The URL of the weapon.</param>
        /// <para><see cref="GetHuntingHorn(string, int[])"></see> if you wish to gather information on hunting horns.</para>
        public async Task GetGenericSword(string address) {
            try {
                var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
                var context = BrowsingContext.New(config);
                var page = await context.OpenAsync(address);
                string setname = page.QuerySelector("[itemprop=\"name\"]").TextContent.Split("/")[0].Trim();
                ConsoleWriters.StartingPageMessage($"Started work on the {setname} series. ({address})");

                var crafting_table = page.QuerySelectorAll(".table")[1].QuerySelector("tbody");
                int current_wpn_index = 0;
                foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr")) {

                    SwordValues sv = await GetSwordAttributes(page, tr, crafting_table, current_wpn_index);
                    List<SharpnessValue> sharpvalues = GetSharpness(page, tr);
                    await db.InsertAllAsync(sharpvalues);
                    sv.sharp_0_id = sharpvalues[0].sharp_id;
                    sv.sharp_1_id = sharpvalues[1].sharp_id;
                    sv.sharp_2_id = sharpvalues[2].sharp_id;
                    sv.sword_set_name = setname;

                    if (address.Contains("/greatsword/")) { sv.sword_class = "Great Sword"; }
                    else if (address.Contains("/longsword/")) { sv.sword_class = "Long Sword"; }
                    else if (address.Contains("/swordshield/")) { sv.sword_class = "Sword & Shield"; }
                    else if (address.Contains("/hammer/")) { sv.sword_class = "Hammer"; }
                    else if (address.Contains("/lance/")) { sv.sword_class = "Great Sword"; }
                    else if (address.Contains("/insectglaive/")) {sv.sword_class = "Insect Glaive"; }
                    else if (address.Contains("/dualblades/")) {sv.sword_class = "Dual Blades";}
                    await db.InsertAsync(sv);

                    List<CraftItem> craftitems = GetCraftItems(crafting_table.Children[current_wpn_index]);
                    foreach (CraftItem item in craftitems) {
                        item.creation_id = sv.sword_id;
                    }
                    foreach (ElementDamage element in sv.element) {
                        element.weapon_id = sv.sword_id;
                    }
                    await db.InsertAllAsync(sv.element);
                    await db.InsertAllAsync(craftitems);
                    current_wpn_index++;
                }
                ConsoleWriters.CompletionMessage($"Finished with the {setname} series!");
            }
            catch (Exception ex) {
                ConsoleWriters.ErrorMessage(ex.ToString());
                await GetGenericSword(address);
            }
        }

        /// <summary>
        /// Retrieves weapon information for hunting horns.
        /// </summary>
        /// <param name="address">The URL of the weapon.</param>
        /// <param name="notes">An array of ints corresponding to the horn's note values.</param>
        /// <returns></returns>
        public async Task GetHuntingHorn(string address, int[] notes) {
            try {
                var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
                var context = BrowsingContext.New(config);
                var page = await context.OpenAsync(address);
                string setname = page.QuerySelector("[itemprop=\"name\"]").TextContent.Split("/")[0].Trim();
                ConsoleWriters.StartingPageMessage($"Started work on the {setname} series. ({address})");
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

                var crafting_table = page.QuerySelectorAll(".table")[1].QuerySelector("tbody");
                int current_wpn_index = 0;
                foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr")) {
                    SwordValues sv = await GetSwordAttributes(page, tr, crafting_table, current_wpn_index);
                    List<SharpnessValue> sharpvalues = GetSharpness(page, tr);
                    await db.InsertAllAsync(sharpvalues);
                    sv.sharp_0_id = sharpvalues[0].sharp_id;
                    sv.sharp_1_id = sharpvalues[1].sharp_id;
                    sv.sharp_2_id = sharpvalues[2].sharp_id;
                    sv.sword_set_name = setname;
                    sv.sword_class = "Hunting Horn";
                    await db.InsertAsync(sv);

                    List<CraftItem> craftitems = GetCraftItems(crafting_table.Children[current_wpn_index]);
                    foreach (CraftItem item in craftitems) {
                        item.creation_id = sv.sword_id;
                    }
                    foreach (ElementDamage element in sv.element) {
                        element.weapon_id = sv.sword_id;
                    }
                    await db.InsertAllAsync(sv.element);
                    await db.InsertAllAsync(craftitems);

                    HuntingHorn horn = new HuntingHorn() {
                        sword_id = sv.sword_id,
                        notes = notestring
                    };

                    await db.InsertAsync(horn);

                    current_wpn_index++;
                }
                ConsoleWriters.CompletionMessage($"Finished with the {setname} series!");
            }
            catch (Exception ex) {
                ConsoleWriters.ErrorMessage(ex.ToString());
                await GetHuntingHorn(address, notes);
            }
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
                upgrades_into = String.Join('\n', upgradeinfo[0].QuerySelectorAll("a").Select(a => a.TextContent.Trim()));
            }
            List<ElementDamage> elements = new List<ElementDamage>();
            int affinity = 0;
            foreach (var small in wrapper.Children[2].QuerySelectorAll("small")) {
                if (small.TextContent.Any(char.IsLetter)) {
                    elements.Add(GetElement(small));
                }
                else {
                    affinity = Convert.ToInt32(intsOnly.Replace(small.TextContent.Trim(), ""));
                }
            }
            int price = Convert.ToInt32(upgradeinfo[1].TextContent.Replace("z", ""));
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
        /// <param name="page">The IDocument holding the page information.</param>
        /// <param name="wrapper">The table row element holding the weapon information.</param>
        /// <returns>
        /// <para>Returns a list of SharpnessValue objects.</para>
        /// <para>Index zero is the base weapon sharpness, index one is the sharpness with the skill Sharpness+1, and index two
        /// is the sharpness with Sharpness+2.</para>
        /// </returns>
        public List<SharpnessValue> GetSharpness(IDocument page, IElement wrapper) {
            List<SharpnessValue> values = new List<SharpnessValue>();
            var sharpvalues = wrapper.Children[3].QuerySelectorAll("div");
            for (int i = 0; i <= 2; i++) {
                var spans = sharpvalues[i].QuerySelectorAll("span");
                int red_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[0]).Width, "")) * 5;
                int orange_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[1]).Width, "")) * 5;
                int yellow_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[2]).Width, "")) * 5;
                int green_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[3]).Width, "")) * 5;
                int blue_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[4]).Width, "")) * 5;
                int white_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[5]).Width, "")) * 5;
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
        /// Gets the special elements related to the weapon. May be used for both Blademaster and Gunner weapons.
        /// <para>The term "elements" includes literal elemental qualitites (such as 28 Poison), status damage
        /// (such as 14 Para), and defense that the weapon adds on top of your armor (such as 20 Def).</para>
        /// </summary>
        /// <param name="small">The <c>small</c> element containing the element information.</param>
        /// <returns>
        /// Returns an <c>ElementDamage</c> object with the element information.
        /// </returns>
        public ElementDamage GetElement(IElement small) {
            int elem_amount = Convert.ToInt32(intsOnly.Replace(small.TextContent.Trim(), ""));
            string elem_type = small.TextContent.Trim().Split(" ").Last();
            return new ElementDamage() {
                elem_type = elem_type,
                elem_amount = elem_amount,
            };
        }

        public List<CraftItem> GetCraftItems(IElement wrapper) {
            var tds = wrapper.QuerySelectorAll("td");
            List<CraftItem> items = new List<CraftItem>();
            var upgrade_children = tds[3].Children;
            var create_children = tds[2].Children;

            bool is_scrap = false;
            foreach (var child in create_children) {
                if (child.NodeName == "DIV" && child.TextContent == "Scraps") {
                    is_scrap = true;
                }
                else if (child.NodeName == "A") {
                    string item_name = child.TextContent.Trim();
                    int quantity;
                    string unlocks_creation = "no";
                    if (child.GetAttribute("data-toggle") != null) {
                        quantity = Convert.ToInt32(intsOnly.Replace(child.TextContent, ""));
                    }
                    else {
                        quantity = Convert.ToInt32(intsOnly.Replace(child.NextSibling.TextContent, ""));
                        if (child.NextSibling.TextContent.Contains("Unlock")) {
                            unlocks_creation = "yes";
                        }
                    }
                    
                    items.Add(new CraftItem() {
                        item_name = item_name,
                        quantity = quantity,
                        is_scrap = is_scrap ? "yes" : "no",
                        unlocks_creation = unlocks_creation,
                        usage = is_scrap ? "byproduct" : "create"
                    });
                }
            }
            is_scrap = false;

            foreach (var child in upgrade_children) {
                if (child.NodeName == "DIV" && child.TextContent == "Scraps") {
                    is_scrap = true;
                }
                else if (child.NodeName == "A") {
                    string item_name = child.TextContent.Trim();
                    int quantity;
                    if (child.GetAttribute("data-toggle") != null) {
                        quantity = Convert.ToInt32(intsOnly.Replace(child.TextContent, ""));
                    }
                    else {
                        quantity = Convert.ToInt32(intsOnly.Replace(child.NextSibling.TextContent, ""));
                    }
                    
                    items.Add(new CraftItem() {
                        item_name = item_name,
                        quantity = quantity,
                        is_scrap = is_scrap ? "yes" : "no",
                        unlocks_creation = "no",
                        usage = is_scrap ? "byproduct" : "upgrade"
                    });
                }
            }
            
            return items; 
        }
    }
}