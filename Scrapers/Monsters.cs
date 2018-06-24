using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;

namespace Gensearch.Scrapers
{
    public class Monsters
    {
        public async Task GetMonsters() {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync("http://mhgen.kiranico.com");
            var rows = page.QuerySelectorAll(".table")[6].QuerySelectorAll("td a").OfType<IHtmlAnchorElement>().ToArray();
            int throttle = 3;
            
            List<Task> tasks = new List<Task>();
            foreach (var row in rows) {
                tasks.Add(GetMonster(row.Href));
                
                if (tasks.Count == throttle) {
                    Task completed = await Task.WhenAny(tasks);
                    tasks.Remove(completed);
                }
            }
            await Task.WhenAll(tasks);
        }

        public async Task GetMonster(string address) {
            try {
                var page = await BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(address);
                var db = new SQLiteAsyncConnection("data/mhgen.db");
                await db.CreateTablesAsync<Monster, MonsterDrop, MonsterPart>();
                // Adds basic monster info into the database.
                var intvalues = page.QuerySelectorAll(".lead");
                Regex decimalsOnly = new Regex(@"[^\d.]"); 

                string name = page.QuerySelector("[itemprop=\"name\"]").TextContent;
                int base_hp = 0;
                double base_size = 0.0;
                double small_size = 0.0;
                double silver_size = 0.0;
                double king_size = 0.0;

                try {
                    base_hp = Convert.ToInt32(decimalsOnly.Replace(intvalues[0].TextContent, ""));
                    base_size = Convert.ToDouble(decimalsOnly.Replace(intvalues[1].TextContent, ""));
                    small_size = Convert.ToDouble(decimalsOnly.Replace(intvalues[2].TextContent, ""));
                    silver_size = Convert.ToDouble(decimalsOnly.Replace(intvalues[3].TextContent, ""));
                    king_size = Convert.ToDouble(decimalsOnly.Replace(intvalues[4].TextContent, ""));
                }
                catch (Exception) {}

                Monster monster = new Monster() {
                    mon_name = name,
                    base_hp = base_hp,
                    base_size = base_size,
                    small_size = small_size,
                    silver_size = silver_size,
                    king_size = king_size,
                };

                try {
                    await db.InsertAsync(monster);
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(monster.mon_name + " has been added to the database.");
                    Console.ResetColor();
                }
                catch (SQLiteException) {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(monster.mon_name + " is already in the database!");
                    Console.ResetColor();
                }

                await db.InsertAllAsync(GetParts(page, monster.id));
                await db.InsertAllAsync(await GetDrops(page, monster.id, db));
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Finished with " + monster.mon_name + "!");
                Console.ResetColor();
            }
            catch (Exception) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error on address " + address + ". Retrying.");
                Console.ResetColor();
                await GetMonster(address);
            }
        }

        public List<MonsterPart> GetParts(IDocument page, int monster) {
            List<MonsterPart> parts = new List<MonsterPart>();
            var part_rows = page.QuerySelectorAll(".col-lg-5 tbody tr");
                foreach (var row in part_rows) {
                    var tds = row.QuerySelectorAll("td");
                    MonsterPart part = new MonsterPart() {
                        part_name = tds[0].TextContent,
                        stagger_value = Convert.ToInt32(tds[1].TextContent),
                        monsterid = monster
                    };
                    if (tds[3].TextContent.Contains("R")) { part.extract_color = "red"; }
                    else if (tds[3].TextContent.Contains("O")) { part.extract_color = "orange"; }
                    else { part.extract_color = "white"; }

                    try {
                        parts.Add(part);
                    }
                    catch (SQLiteException ex) {
                        Console.WriteLine(ex.ToString());
                    }
                }
            return parts;
        }

        public async Task<List<MonsterDrop>> GetDrops(IDocument page, int monster, SQLiteAsyncConnection db) {
            List<MonsterDrop> drops = new List<MonsterDrop>();
            var table_wrappers = page.QuerySelectorAll(".col-lg-6");
            foreach (var wrapper in table_wrappers) {
                string rank = wrapper.QuerySelector("h5").TextContent;
                string currentSource = "";
                foreach (var tr in wrapper.QuerySelectorAll("tr").Where(t => t.ClassName != "table-active")) {
                    if (tr.FirstElementChild.ClassName == "vertical-align") {
                        currentSource = tr.FirstElementChild.TextContent.Trim();
                    }
                    else {
                        var tds = tr.QuerySelectorAll("td");
                        var drop_item = await db.QueryAsync<Item>("select * from Items where item_name = ?", 
                        ((IHtmlAnchorElement) tds[1].FirstElementChild).TextContent);
                        MonsterPart part = new MonsterPart();
                        // Make sure it's not adding the same carve/capture multiple time.
                        var tempp = await db.QueryAsync<MonsterPart>("SELECT * FROM MonsterParts WHERE monsterid = ?", monster);
                        foreach (var monpart in tempp) {
                            if (currentSource == monpart.part_name) {
                                part = monpart;
                                break;
                            }
                        }
                        if (part.part_name == null) {
                            part.monsterid = monster;
                            part.part_name = currentSource;
                            await db.InsertAsync(part);
                        }
                        Regex intsOnly = new Regex(@"[^\d]"); 
                        var part_wrap = await db.QueryAsync<MonsterPart>("select * from MonsterParts where id = ?", part.id);
                        string quantity = "";
                        if (tds[1].TextContent.Any(char.IsDigit)) {
                            quantity = intsOnly.Replace(tds[1].TextContent.Trim(), "");
                        }
                        else {
                            quantity = "1";
                        }

                        MonsterDrop drop = new MonsterDrop() {
                            itemid = drop_item[0].id,
                            monsterid = monster,
                            sourceid = part_wrap[0].id,
                            rank = rank,
                            drop_chance = Convert.ToInt32(intsOnly.Replace(tds[2].TextContent.Trim(), "")),
                            quantity = Convert.ToInt32(intsOnly.Replace(quantity, ""))
                        };
                        drops.Add(drop);
                    }
                }
            }
            return drops;
        }
    }
}