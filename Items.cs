using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom.Html;
using SQLite;

namespace Gensearch
{
    class Items
    {
        public string itaddress = "http://mhgen.kiranico.com/item";

        public async Task GetItemList() {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync(itaddress);
            var rows = page.QuerySelector(".table").QuerySelectorAll("td a").OfType<IHtmlAnchorElement>().ToArray();
            var db = new SQLiteAsyncConnection("data/mhgen.db");
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

        public async Task<string[]> GetItem(string address) {
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
                return new string[6] {
                    name, // name
                    page.QuerySelector("p[itemprop=\"description\"]").TextContent, // description
                    itemIntData[0].TextContent, // rarity
                    itemIntData[1].TextContent, // stack size
                    itemIntData[2].TextContent, // sell price
                    combination
                }.ToArray();
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error on page " + address + ". Retrying.");
                Console.ResetColor();
                return await GetItem(address);
            }
        }

        public async Task UpdateItemDatabase(Task<string[]> task) {
            string[] data = await task;
            var db = new SQLiteAsyncConnection("data/mhgen.db");
            await db.CreateTableAsync<Item>();
            try {
                await db.InsertAsync (new Item() {
                    item_name = data[0],
                    description = data[1],
                    rarity = Convert.ToInt32(data[2]),
                    max_stack = Convert.ToInt32(data[3]),
                    sell_price = Convert.ToInt32(data[4].Remove(data[4].Length - 1)),
                    combinations = data[5]
                });
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(String.Format ("Inserted {0} into the item database!", data[0]));
                Console.ResetColor();
            } catch (SQLiteException) { Console.WriteLine (data[0] + " is already in the item database."); }
        }

    }
}
