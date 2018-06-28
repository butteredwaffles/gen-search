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
        static Regex intsOnly = new Regex(@"[^\d\+-]");
        public BladeWeapons bw = new BladeWeapons();
        public GunnerWeapons gw = new GunnerWeapons();

        /// <summary>
        /// Iterates through all the weapon URLS on a page and feeds each one into the database.
        /// </summary>
        /// <param name="addr">The address to pull from.</param>
        public async Task GetWeapons(string addr) {
            int throttle = 5;
            string[] special_weapons = new string[] {"/gunlance", "/chargeblade", "/switchaxe", "/lightbowgun", "/heavybowgun", "/bow", "/huntinghorn"};
            await db.CreateTablesAsync<SwordValues, SharpnessValue, ElementDamage, CraftItem, HuntingHorn>();
            await db.CreateTablesAsync<PhialOrShellWeapon, Bow>();
            await db.CreateTablesAsync<Bowgun, BowgunAmmo, InternalBowgunAmmo, SpecialBowgunAmmo>();

            try {
                List<Task> tasks = new List<Task>();
                var config = Configuration.Default.WithDefaultLoader().WithJavaScript().WithCss();
                var context = BrowsingContext.New(config);
                var page = await context.OpenAsync(addr);
                int page_length = Convert.ToInt32(page.ExecuteScript("window[\"mhgen\"][\"weapons\"].length"));
                string address;
                if (!special_weapons.Any(w => addr.Contains(w))) {
                    for (int i = 0; i < page_length; i++) {
                        address = (string) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i.ToString()}].url");
                        tasks.Add(bw.GetGenericSword(address));

                        if (tasks.Count == throttle) {
                            Task completed = await Task.WhenAny(tasks);
                            tasks.Remove(completed);
                        }
                    }
                    await Task.WhenAll(tasks);
                }
                else if (addr.Contains("/huntinghorn")) {
                    for (int i = 0; i < page_length; i++) {
                        address = (string) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i.ToString()}].url");
                        int[] notes = new int[] {
                            Convert.ToInt32((double) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i}][\"levels\"][0][\"hhnotes\"][0][\"color_1\"]")),
                            Convert.ToInt32((double) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i}][\"levels\"][0][\"hhnotes\"][0][\"color_2\"]")),
                            Convert.ToInt32((double) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i}][\"levels\"][0][\"hhnotes\"][0][\"color_3\"]")),
                        };
                        tasks.Add(bw.GetHuntingHorn(address, notes));

                        if (tasks.Count == throttle) {
                            Task completed = await Task.WhenAny(tasks);
                            tasks.Remove(completed);
                        }
                    }
                    await Task.WhenAll(tasks);
                }
                else if (addr.Contains("/switchaxe") || addr.Contains("/chargeblade") || addr.Contains("/gunlance")) {
                    for (int i = 0; i < page_length; i++) {
                        address = (string) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i.ToString()}].url");
                        tasks.Add(bw.GetPhialAndShellWeapons(address));

                        if (tasks.Count == throttle) {
                            Task completed = await Task.WhenAny(tasks);
                            tasks.Remove(completed);
                        }
                    }
                    await Task.WhenAll(tasks);
                }
                else if (addr.Contains("/bow")) {
                    for (int i = 0; i < page_length; i++) {
                        address = (string) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i.ToString()}].url");
                        tasks.Add(gw.GetBow(address));

                        if (tasks.Count == throttle) {
                            Task completed = await Task.WhenAny(tasks);
                            tasks.Remove(completed);
                        }
                    }
                    await Task.WhenAll(tasks);
                }
                else {
                    for (int i = 0; i < page_length; i++) {
                        address = (string) page.ExecuteScript($"window[\"mhgen\"][\"weapons\"][{i.ToString()}].url");
                        tasks.Add(gw.GetBowgun(address));

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
        /// Gets the special elements related to the weapon. May be used for both Blademaster and Bow weapons.
        /// <para>The term "elements" includes literal elemental qualitites (such as 28 Poison), status damage
        /// (such as 14 Para), and defense that the weapon adds on top of your armor (such as 20 Def).</para>
        /// </summary>
        /// <param name="small">The <c>small</c> element containing the element information.</param>
        /// <returns>
        /// Returns an <c>ElementDamage</c> object with the element information.
        /// </returns>
        public static ElementDamage GetElement(IElement small) {
            int elem_amount = Convert.ToInt32(intsOnly.Replace(small.TextContent.Trim(), ""));
            string elem_type = small.TextContent.Trim().Split(" ").Last();
            return new ElementDamage() {
                elem_type = elem_type,
                elem_amount = elem_amount,
            };
        }

        public static List<CraftItem> GetCraftItems(IElement wrapper) {
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