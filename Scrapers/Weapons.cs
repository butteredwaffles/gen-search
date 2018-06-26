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
        public async Task GetWeapons(string addr) {
            int throttle = 3;
            string[] special_weapons = new string[] {"/dualblades", "/gunlance", "/chargeblade", "/switchaxe", "/lightbowgun", "/heavybowgun", "/bow", "/huntinghorn"};
            await db.CreateTablesAsync<SwordValues, SharpnessValue, ElementDamage, CraftItem>();
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
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Error getting page. Pausing for sixty seconds.");
                Thread.Sleep(60000);
                
            }
        }

        /// <summary>
        /// Gets greatswords, longswords, sword and shields, hammers, lances, and insect glaives.
        /// The following weapons are excluded because of special characteristics:
        ///     Switch Axe / Charge Blade - Phials
        ///     LBG / HBG / Bow - Ammo
        ///     Gunlance - Shells
        ///     Dual Blades - Dual Elements
        ///     Hunting Horn - Songs
        /// </summary>
        /// <param name="address">The URL of the weapon.</param>
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
                    if (sv.element != null) {
                        await db.InsertAsync(sv.element);
                        sv.elem_id = sv.element.elem_id;
                    }
                    else {
                        // Ints are non-nullable so setting it to a value that's impossible
                        sv.elem_id = -1;
                    }

                    if (address.Contains("/greatsword/")) { sv.sword_class = "Great Sword"; }
                    else if (address.Contains("/longsword/")) { sv.sword_class = "Long Sword"; }
                    else if (address.Contains("/swordshield/")) { sv.sword_class = "Sword & Shield"; }
                    else if (address.Contains("/hammer/")) { sv.sword_class = "Hammer"; }
                    else if (address.Contains("/lance/")) { sv.sword_class = "Great Sword"; }
                    else if (address.Contains("/insectglaive/")) {sv.sword_class = "Insect Glaive"; }
                    await db.InsertAsync(sv);

                    List<CraftItem> craftitems = GetCraftItems(crafting_table.Children[current_wpn_index]);
                    foreach (CraftItem item in craftitems) {
                        item.creation_id = sv.sword_id;
                    }
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
            ElementDamage element = null;
            int affinity = 0;
            foreach (var small in wrapper.Children[2].QuerySelectorAll("small")) {
                if (small.TextContent.Any(char.IsLetter)) {
                    element = GetElement(wrapper);
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
                element = element,
                affinity = affinity,
                monster_id = monsterid,
            };
        }

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

        public ElementDamage GetElement(IElement wrapper) {
            var div = wrapper.Children[2].QuerySelector("div");
            int elem_amount = Convert.ToInt32(intsOnly.Replace(div.TextContent.Trim(), ""));
            string elem_type = div.TextContent.Trim().Split(" ").Last();
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