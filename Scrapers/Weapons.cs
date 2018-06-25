using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Gensearch.Helpers;
using SQLite;

namespace Gensearch.Scrapers
{
    public class Weapons
    {
        Regex intsOnly = new Regex(@"[^\d\+-]"); 
        public async Task GetWeapons(string addr) {
            int throttle = 3;
            try {
                List<Task> tasks = new List<Task>();
                var config = Configuration.Default.WithDefaultLoader().WithJavaScript().WithCss();
                var context = BrowsingContext.New(config);
                var page = await context.OpenAsync(addr);
                int page_length = Convert.ToInt32(page.ExecuteScript("window[\"mhgen\"][\"weapons\"].length"));
                for (int i = 0; i < page_length; i++) {
                    string address = (string) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i.ToString()}].url");
                    tasks.Add(GetGreatSword(address));

                    if (tasks.Count == throttle) {
                        Task completed = await Task.WhenAny(tasks);
                        tasks.Remove(completed);
                    }
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Error getting page. Pausing for sixty seconds.");
                Thread.Sleep(60000);
                
            }
        }

        public async Task GetGreatSword(string address) {
            try {
                var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
                var context = BrowsingContext.New(config);
                var page = await context.OpenAsync(address);
                SQLiteAsyncConnection db = new SQLiteAsyncConnection("data/mhgen.db");
                await db.CreateTablesAsync<GreatSword, SharpnessValue, ElementDamage>();

                string setname = page.QuerySelector("[itemprop=\"name\"]").TextContent.Split("/")[0].Trim();
                ConsoleWriters.StartingPageMessage($"Started work on the {setname} series. ({address})");

                var crafting_table = page.QuerySelectorAll(".table")[1].QuerySelector("tbody");
                int current_wpn_index = 0;
                foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr")) {
                    string weapon_name = tr.FirstElementChild.TextContent;
                    int weapon_damage = Convert.ToInt32(tr.Children[1].TextContent);
                    var specialinfo = tr.Children[2];
                    ElementDamage element = null;
                    int affinity = 0;
                    foreach (var small in specialinfo.QuerySelectorAll("small")) {
                        if (small.TextContent.Any(char.IsLetter)) {
                            element = GetElement(tr);
                            await db.InsertAsync(element);
                        }
                        else {
                            affinity = Convert.ToInt32(intsOnly.Replace(small.TextContent.Trim(), ""));
                        }
                    }
                    List<SharpnessValue> sharpvalues = GetSharpness(page, tr);
                    await db.InsertAllAsync(sharpvalues);
                    var techinfo = tr.Children[5];
                    int slots = techinfo.FirstElementChild.TextContent.Count(c => c == '◯');
                    int rarity = Convert.ToInt32(intsOnly.Replace(techinfo.Children[1].TextContent.Trim(), ""));
                    string upgrades_into = "none";
                    var upgradeinfo = crafting_table.Children[current_wpn_index].QuerySelectorAll("td");
                    if (upgradeinfo[0].QuerySelector(".font-weight-bold") != null) {
                        upgrades_into = String.Join('\n', upgradeinfo[0].QuerySelectorAll("a").Select(a => a.TextContent.Trim()));
                    }
                    int price = Convert.ToInt32(upgradeinfo[1].TextContent.Replace("z", ""));
                    GreatSword gs = new GreatSword() {
                        gs_name = weapon_name,
                        gs_set_name = setname,
                        raw_dmg = weapon_damage,
                        affinity = affinity,
                        sharp_0_id = sharpvalues[0].sharp_id,
                        sharp_1_id = sharpvalues[1].sharp_id,
                        sharp_2_id = sharpvalues[2].sharp_id,
                        slots = slots,
                        rarity = rarity,
                        upgrades_into = upgrades_into,
                        price = price
                    };
                    if (element != null) {
                        gs.elem_id = element.elem_id;
                    }
                    else {
                        // Ints are non-nullable so setting it to a value that's impossible
                        gs.elem_id = -1;
                    }
                    await db.InsertAsync(gs);
                    current_wpn_index++;
                }
                ConsoleWriters.CompletionMessage($"Finished with the {setname} series!");
            }
            catch (Exception ex) {
                ConsoleWriters.ErrorMessage(ex.ToString());
            }
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
    }
}