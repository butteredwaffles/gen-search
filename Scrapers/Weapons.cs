using System;
using System.Collections.Generic;
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
        Regex intsOnly = new Regex(@"[^\d]"); 
        public async Task GetWeapons(string addr) {
            int throttle = 6;
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
            catch {
                Console.WriteLine("Error getting page. Pausing for sixty seconds.");
                Thread.Sleep(60000);
                
            }
        }

        public async Task GetGreatSword(string address) {
            var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync(address);
            SQLiteAsyncConnection db = new SQLiteAsyncConnection("data/mhgen.db");
            await db.CreateTablesAsync<GreatSword, SharpnessValue>();

            string setname = page.QuerySelector("[itemprop=\"name\"]").TextContent.Split("/")[0].Trim();
            ConsoleWriters.StartingPageMessage(setname, address);       

            List<SharpnessValue> sharpvalues = new List<SharpnessValue>();
            foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr")) {
                sharpvalues.AddRange(GetSharpness(page, tr, 0));
            }
            await db.InsertAllAsync(sharpvalues);
        }

        public List<SharpnessValue> GetSharpness(IDocument page, IElement wrapper, int weaponid) {
            List<SharpnessValue> values = new List<SharpnessValue>();
            var sharpvalues = wrapper.QuerySelectorAll("td")[3];
            foreach (var div in sharpvalues.QuerySelectorAll("div")) {
                var spans = div.QuerySelectorAll("span");
                int red_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[0]).Width, "")) * 5;
                int orange_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[1]).Width, "")) * 5;
                int yellow_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[2]).Width, "")) * 5;
                int green_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[3]).Width, "")) * 5;
                int blue_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[4]).Width, "")) * 5;
                int white_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[5]).Width, "")) * 5;
                values.Add(new SharpnessValue() {
                    weapon_id = weaponid,
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
    }
}