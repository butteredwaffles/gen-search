using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom.Html;

namespace Gensearch
{
    public class Monsters
    {
        
        List<string> woundable_parts = new List<string>();

        public async Task GetMonsters() {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync("http://mhgen.kiranico.com");
            var rows = page.QuerySelectorAll(".table")[6].QuerySelectorAll("td a").OfType<IHtmlAnchorElement>().ToArray();
            
            List<Task> tasks = new List<Task>();
            foreach (var row in rows) {
                tasks.Add(GetMonster(row.Href));
            }
            await Task.WhenAll(tasks);
        }

        public async Task GetMonster(string address) {

            // Simply used to determine the names of breakable parts. Delete later.
            var page = await BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(address);
            var found_wounds = page.QuerySelectorAll("td.vertical-align")
                .Where(a => a.TextContent.Contains("Wound") || a.TextContent.Contains("Tail"))
                .Select(a => a.TextContent).ToArray();
            foreach (string wound in found_wounds) {
                if (!this.woundable_parts.Contains(wound)) {
                    this.woundable_parts.Add(wound);
                    Console.WriteLine(wound);
                }
            }
        }
    }
}