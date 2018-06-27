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
        public BladeWeapons bw = new BladeWeapons(); 

        /// <summary>
        /// Iterates through all the weapon URLS on a page and feeds each one into the database.
        /// </summary>
        /// <param name="addr">The address to pull from.</param>
        public async Task GetWeapons(string addr) {
            int throttle = 3;
            string[] special_weapons = new string[] {"/gunlance", "/chargeblade", "/switchaxe", "/lightbowgun", "/heavybowgun", "/bow", "/huntinghorn"};
            await db.CreateTablesAsync<SwordValues, SharpnessValue, ElementDamage, CraftItem, HuntingHorn>();
            await db.CreateTableAsync<PhialOrShellWeapon>();

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
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Error getting page. Pausing for sixty seconds.");
                Thread.Sleep(60000);
                
            }
        }
    }
}