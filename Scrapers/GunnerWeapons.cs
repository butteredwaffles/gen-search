using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Gensearch.Helpers;
using SQLite;

namespace Gensearch.Scrapers
{
    public class GunnerWeapons
    {
        SQLiteAsyncConnection db = GenSearch.db;
        Regex intsOnly = new Regex(@"[^\d\+-]");

        public async Task GetBow(string address) {
            var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync(address);
            string[] flav = Weapons.GetFlavorText(page);

            var crafting_table = page.QuerySelectorAll(".table")[1].QuerySelector("tbody");
            int current_wpn_index = 0;
            foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr")) {
                string weapon_name = tr.FirstElementChild.TextContent;
                int weapon_damage = Convert.ToInt32(tr.Children[1].TextContent);

                List<ElementDamage> elements = new List<ElementDamage>();
                int affinity = 0;
                foreach (var small in tr.Children[2].QuerySelectorAll("small")) {
                    if (small.TextContent.Any(char.IsLetter)) {
                        elements.Add(Weapons.GetElement(small));
                    }
                    else {
                        affinity = Convert.ToInt32(intsOnly.Replace(small.TextContent.Trim(), ""));
                    }
                }

                string[] bow_shots = GetBowShots(tr);
                string supported_coatings = GetBowCoatings(tr);
                int slots = tr.Children[5].FirstElementChild.TextContent.Count(c => c == '◯');

                int monsterid = -1;
                if (page.QuerySelectorAll(".lead").Count() == 3) {
                    monsterid = (await Monsters.GetMonsterFromDB(page.QuerySelectorAll(".lead")[2].TextContent.Trim())).id;
                }
                
                Bow bow = new Bow() {
                    monster_id = monsterid,
                    bow_name = weapon_name,
                    bow_damage = weapon_damage,
                    affinity = affinity,
                    arc_type = bow_shots[0].Split(":")[1].Trim(),
                    level_one_charge = bow_shots[1],
                    level_two_charge = bow_shots[2],
                    level_three_charge = bow_shots[3],
                    level_four_charge = bow_shots[4],
                    supported_coatings = supported_coatings,
                    slots = slots,
                    rarity = Convert.ToInt32(flav[4]),
                    description = weapon_name.Contains(flav[0]) ? flav[2] : flav[3]
                };
                await db.InsertAsync(bow);

                List<CraftItem> craftitems = Weapons.GetCraftItems(crafting_table.Children[current_wpn_index]);
                foreach (CraftItem item in craftitems) {
                    item.creation_id = bow.bow_id;
                    item.creation_type = "Bow";
                }
                foreach (ElementDamage element in elements) {
                    element.weapon_id = bow.bow_id;
                }
                await db.InsertAllAsync(craftitems);
                current_wpn_index++;
            }
        }

        public string[] GetBowShots(IElement wrapper) {
            return wrapper.QuerySelectorAll("td")[3].QuerySelectorAll("div").Select(d => d.TextContent).ToArray();
        }

        public string GetBowCoatings(IElement wrapper) {
            var td = wrapper.QuerySelectorAll("td")[4];
            string supported_coatings = "";
            foreach (var span in td.GetElementsByClassName("text-primary")) {
                supported_coatings += span.TextContent + " ";
            }
            return supported_coatings.Trim().Replace(" ", ", ");
        }
        
        public async Task GetBowgun(string address) {
            var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync(address);
            string[] flav = Weapons.GetFlavorText(page);

            var crafting_table = page.QuerySelectorAll(".table")[1].QuerySelector("tbody");
            int current_wpn_index = 0;
            foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr")) {
                string weapon_name = tr.FirstElementChild.TextContent;
                int weapon_damage = Convert.ToInt32(tr.Children[1].TextContent);

                List<ElementDamage> elements = new List<ElementDamage>();
                int affinity = 0;
                foreach (var small in tr.Children[2].QuerySelectorAll("small").Skip(1)) {
                    if (small.TextContent.Any(char.IsLetter)) {
                        elements.Add(Weapons.GetElement(small));
                    }
                    else {
                        affinity = Convert.ToInt32(intsOnly.Replace(small.TextContent.Trim(), ""));
                    }
                }
                
                string[] gun_characteristics = GetBowgunInformation(tr);
                int slots = tr.Children[5].FirstElementChild.TextContent.Count(c => c == '◯');

                int monsterid = -1;
                if (page.QuerySelectorAll(".lead").Count() == 3) {
                    monsterid = (await Monsters.GetMonsterFromDB(page.QuerySelectorAll(".lead")[2].TextContent.Trim())).id;
                }
                
                Bowgun bowgun = new Bowgun() {
                    monster_id = monsterid,
                    bg_name = weapon_name,
                    bg_damage = weapon_damage,
                    affinity = affinity,
                    slots = slots,
                    rarity = Convert.ToInt32(flav[4]),
                    reload_speed = gun_characteristics[0],
                    recoil = gun_characteristics[1],
                    deviation = gun_characteristics[2],
                    description = weapon_name.Contains(flav[0]) ? flav[2] : flav[3]
                };
                await db.InsertAsync(bowgun);

                BowgunAmmo ba = GetBowgunAmmo(tr);
                ba.bowgun_id = bowgun.bg_id;
                await db.InsertAsync(ba);

                List<InternalBowgunAmmo> iba = GetInternalAmmo(tr);
                foreach (InternalBowgunAmmo ammo in iba) {
                    ammo.bowgun_id = bowgun.bg_id;
                }
                await db.InsertAllAsync(iba);

                List<SpecialBowgunAmmo> sba = GetSpecialAmmo(tr);
                foreach (SpecialBowgunAmmo ammo in sba) {
                    ammo.bowgun_id = bowgun.bg_id;
                }
                await db.InsertAllAsync(sba);

                List<CraftItem> craftitems = Weapons.GetCraftItems(crafting_table.Children[current_wpn_index]);
                foreach (CraftItem item in craftitems) {
                    item.creation_id = bowgun.bg_id;
                    item.creation_type = "Bowgun";
                }
                foreach (ElementDamage element in elements) {
                    element.weapon_id = bowgun.bg_id;
                }
                await db.InsertAllAsync(craftitems);
                current_wpn_index++;
            }
        }

        public string[] GetBowgunInformation(IElement wrapper) {
            return wrapper.Children[2].FirstElementChild.QuerySelectorAll("div").Select(d => d.TextContent.Split(":")[1].Trim()).ToArray();
        }

        public BowgunAmmo GetBowgunAmmo(IElement wrapper) {
            BowgunAmmo ba = new BowgunAmmo();
            // Skipping two because the first two properties are just ids.
            List<PropertyInfo> pi = typeof(BowgunAmmo).GetProperties().Skip(2).ToList();

            // Skipping one because the first span is simply the levels.
            var spans = wrapper.Children[3].QuerySelectorAll("span").Skip(1).ToArray();
            for (int i = 0; i < spans.Count(); i++) {
                string value = spans[i].TextContent;
                // In this case, skill_shots are shots that can be equipped for the bowgun only if the corresponding skill is activated.
                string[] skill_shots = {"normal", "pierce", "pellet", "crag", "clust"};
                if (spans[i].ClassName == "text-muted" && skill_shots.Any(s => pi[i].Name.Contains(s))) {
                    value += " (needs skill)";
                }
                pi[i].SetValue(ba, value);
            }
            return ba;
        }

        public List<InternalBowgunAmmo> GetInternalAmmo(IElement wrapper) {
            var small = wrapper.Children[4].FirstElementChild;
            List<InternalBowgunAmmo> iba = new List<InternalBowgunAmmo>();
            foreach (var div in small.QuerySelectorAll("div").Skip(1)) {
                string[] iba_text = div.TextContent.Split("/");
                iba.Add(new InternalBowgunAmmo() {
                    ammo_name = iba_text[0].Trim(),
                    total_ammo = Convert.ToInt32(iba_text[1].Trim()),
                    load_amt = Convert.ToInt32(iba_text[2].Trim())
                });
            }
            return iba;
        }

        public List<SpecialBowgunAmmo> GetSpecialAmmo(IElement wrapper) {
            var small = wrapper.Children[4].Children[1];
            List<SpecialBowgunAmmo> sba = new List<SpecialBowgunAmmo>();
            string type = small.FirstElementChild.TextContent.Split("/")[0].Trim();
            foreach (var div in small.QuerySelectorAll("div").Skip(1)) {
                string[] sba_text = div.TextContent.Split("/");
                SpecialBowgunAmmo ammo = new SpecialBowgunAmmo() {
                    ammo_name = sba_text[0].Trim(),
                    ammo_type = type,
                    shots = Convert.ToInt32(sba_text[1].Trim())
                };
                
                if (type == "Rapid Fire") {
                    ammo.multiplier = Convert.ToInt32(sba_text[2].Trim());
                    ammo.wait = sba_text[3].Trim();
                }

                sba.Add(ammo);
            }
            return sba;
        }
    }
}