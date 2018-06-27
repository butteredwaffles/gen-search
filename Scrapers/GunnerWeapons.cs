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
    public class GunnerWeapons
    {
        SQLiteAsyncConnection db = GenSearch.db;
        Regex intsOnly = new Regex(@"[^\d\+-]");

        public async Task GetBow(string address) {
            var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync(address);
            string setname = page.QuerySelector("[itemprop=\"name\"]").TextContent.Split("/")[0].Trim();
            ConsoleWriters.StartingPageMessage($"Started work on the {setname} series. ({address})");

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
                var techinfo = tr.Children[5];
                int slots = techinfo.FirstElementChild.TextContent.Count(c => c == '◯');
                int rarity = Convert.ToInt32(techinfo.Children[1].TextContent.Trim().Replace("RARE", ""));
                
                Bow bow = new Bow() {
                    bow_name = weapon_name,
                    bow_damage = weapon_damage,
                    arc_type = bow_shots[0].Split(":")[1].Trim().ToLower(),
                    level_one_charge = bow_shots[1],
                    level_two_charge = bow_shots[2],
                    level_three_charge = bow_shots[3],
                    level_four_charge = bow_shots[4],
                    supported_coatings = supported_coatings,
                    slots = slots,
                    rarity = rarity
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
            ConsoleWriters.CompletionMessage($"Finished with the {setname} series!");
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
    }
}