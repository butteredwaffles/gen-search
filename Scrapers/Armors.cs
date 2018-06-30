using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using Gensearch.Helpers;
using SQLite;

namespace Gensearch.Scrapers
{
    public class Armors
    {
        static SQLiteAsyncConnection db = GenSearch.db;
        public async Task GetArmors(string address) {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync(address);
            var rows = page.QuerySelector(".table").QuerySelectorAll("tr.table-active");
            int throttle = 3;
            
            List<Task> tasks = new List<Task>();
            foreach (var row in rows) {
                string addr = ((IHtmlAnchorElement) row.NextElementSibling.FirstElementChild.FirstElementChild).Href;
                tasks.Add(GetArmor(addr));

                if (tasks.Count == throttle) {
                    Task completed = await Task.WhenAny(tasks);
                    tasks.Remove(completed);
                }
            }
            await Task.WhenAll(tasks);
        }

        public async Task GetArmor(string address) {
            IConfiguration config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
            IBrowsingContext context = BrowsingContext.New(config);
            var page = await context.OpenAsync(address);
            
            Hashtable setinfo = GetSetInfo(page);
        }

        public Hashtable GetSetInfo(IDocument page) {
            var divs = page.QuerySelectorAll(".lead");
            int rarity = divs[0].TextContent.ToInt();
            int max_upgrade = divs[1].TextContent.ToInt();
            List<bool> genderweaponchecks = new List<bool>();
            foreach (int index in new int[] {2, 3, 4, 5}) {
                 genderweaponchecks.Add(divs[index].FirstElementChild.ClassList.Contains("octicon-check"));
            }
            string set_name = page.QuerySelector("[itemprop=\"name\"]").TextContent.Replace("[Blademaster]", "").Replace("[Gunner]", "").Trim();
            int monster_id = -1;
            string[] piece_descriptions;
            if (divs[6].FirstElementChild.TagName == "A") {
                monster_id = Monsters.GetMonsterFromDB(divs[6].FirstElementChild.TextContent).Result.id;
                piece_descriptions = divs.Skip(7).Select(d => d.NextElementSibling.TextContent).ToArray();
            }
            else {
                piece_descriptions = divs.Skip(6).Select(d => d.NextElementSibling.TextContent).ToArray();
            }
            return new Hashtable() {
                {"armor_set", set_name},
                {"rarity", rarity},
                {"max_upgrade", max_upgrade},
                {"is_blademaster", genderweaponchecks[0]},
                {"is_gunner", genderweaponchecks[1]},
                {"is_male", genderweaponchecks[2]},
                {"is_female", genderweaponchecks[3]},
                {"monster_id", monster_id},
                {"piece_descriptions", piece_descriptions}
            };
        }
    }
}