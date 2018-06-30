using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom.Html;
using SQLite;

namespace Gensearch.Scrapers
{
    class Items
    {
        public string itaddress = "http://mhgen.kiranico.com/item";
        private static SQLiteAsyncConnection db = GenSearch.db;

        public static async Task<Item> GetItemFromDB(string name) {
            var items = await db.Table<Item>().Where(i => i.item_name == name).ToArrayAsync();
            return items[0];
        }

        public async Task GetItemList() {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync(itaddress);
            var rows = page.QuerySelector(".table").QuerySelectorAll("td a").OfType<IHtmlAnchorElement>().ToArray();
            await db.CreateTableAsync<Item>();
            List<Task> tasks = new List<Task>();
            foreach (IHtmlAnchorElement item in rows) {
                var it = await db.QueryAsync<Item>("select * from Items where item_name = ?", item.TextContent);
                if (it.Count == 0) {
                    tasks.Add(UpdateItemDatabase(GetItem(item.Href)));
                }
                else {
                    Console.WriteLine(item.TextContent + " is already in the item database, so skipping the request..");
                }
            }
            await Task.WhenAll(tasks.ToArray());
        }

        public async Task<Item> GetItem(string address) {
            try {
                var page = await BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(address);
                string name = page.QuerySelector("h3[itemprop=\"name\"]").TextContent;
                var itemIntData = page.QuerySelectorAll("div.lead");
                string combination;
                var combinationDiv = page.QuerySelector(".col-lg-12 div");
                if (combinationDiv != null) {
                    var comboitems = combinationDiv.QuerySelectorAll("a").OfType<IHtmlAnchorElement>().ToArray();
                    try {
                        combination = comboitems[0].TextContent + " + " + comboitems[1].TextContent;
                    }
                    catch (IndexOutOfRangeException) {
                        combination = "None";
                    }
                }
                else {
                    combination = "None";
                }
                Console.WriteLine("Retrieved " + name.Trim() + ".");
                return new Item() {
                    item_name = name, // name
                    description = page.QuerySelector("p[itemprop=\"description\"]").TextContent,
                    rarity = Convert.ToInt32(itemIntData[0].TextContent),
                    max_stack = Convert.ToInt32(itemIntData[1].TextContent),
                    sell_price = Convert.ToInt32(itemIntData[2].TextContent.Replace("z", "")),
                    combinations = combination
                };
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error on page " + address + ". Retrying.");
                Console.ResetColor();
                return await GetItem(address);
            }
        }

        public async Task UpdateItemDatabase(Task<Item> task) {
            Item item = await task;
            var db = new SQLiteAsyncConnection("data/mhgen.db");
            await db.CreateTableAsync<Item>();
            try {
                await db.InsertAsync(item);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(String.Format ("Inserted {0} into the item database!", item.item_name));
                Console.ResetColor();
            } catch (SQLiteException) { Console.WriteLine (item.item_name + " is already in the item database."); }
        }

    }
}
