using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Gensearch.Helpers;
using Gensearch.Scrapers.Tables;
using SQLite;

namespace Gensearch.Scrapers
{
    public class Palicoes
    {
        static private SQLiteAsyncConnection db = GenSearch.db;
        static private IConfiguration config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
        static private IBrowsingContext context = BrowsingContext.New(config);

        public async Task GetPalicoSkills(string address) {
            var page = await context.OpenAsync(address);
            await db.CreateTableAsync<PalicoSkill>();
            List<PalicoSkill> skills = new List<PalicoSkill>();
            foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr").Skip(1)) {
                skills.Add(GetPalicoSkill(tr));
            }
            await db.InsertAllAsync(skills);
            
        }

        public PalicoSkill GetPalicoSkill(IElement tr) {
            string name = tr.Children[0].TextContent;
            ConsoleWriters.StartingPageMessage($"Retrieving the palico skill {name}.");
            string type = "";
            switch (tr.Children[1].FirstElementChild.ClassList[2]) {
                case "text-danger":
                    type = "offensive";
                    break;
                case "text-info":
                    type = "defensive";
                    break;
                case "text-warning":
                    type = "passive";
                    break;
            }
            int memory = 0;
            foreach (var span in tr.Children[1].Children) {
                memory += 1;
            }
            int level = tr.Children[2].TextContent.ToInt();
            string description = tr.Children[3].TextContent;
            ConsoleWriters.CompletionMessage($"Finished with the palico skill {name}!");
            return new PalicoSkill() {
                ps_description = description,
                ps_learn_level = level,
                ps_memory_req = memory,
                ps_name = name,
                ps_type = type
            };
        }
    }
}