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

        public async Task GetPalicoArmors(string address) {
            var page = await context.OpenAsync(address);
            await db.CreateTablesAsync<PalicoArmor, PalicoCraftItem>();

            int throttle = 7;
            List<Task<PalicoArmor>> tasks = new List<Task<PalicoArmor>>();
            List<PalicoArmor> armors = new List<PalicoArmor>();
            foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr").Skip(1)) {
                tasks.Add(GetPalicoArmor(tr.FirstElementChild.FirstElementChild.GetAttribute("href")));
                if (tasks.Count() == throttle) {
                    Task<PalicoArmor> completed = await Task.WhenAny(tasks);
                    armors.Add(completed.Result);
                    tasks.Remove(completed);
                }
            }
            armors.Concat(await Task.WhenAll(tasks));
            await db.InsertAllAsync(armors);
            List<PalicoCraftItem> all_craft_items = new List<PalicoCraftItem>();
            foreach (var list in armors.Select(a => a.craft_items)) {
                all_craft_items.AddRange(list);
            }
            await db.InsertAllAsync(all_craft_items);
        }

        public async Task<PalicoArmor> GetPalicoArmor(string address) {
            var page = await context.OpenAsync(address);

            var basicinfo = page.QuerySelector("[itemprop=\"gameItem\"]");
            string name = basicinfo.FirstElementChild.TextContent;
            string description = basicinfo.Children[1].TextContent;
            ConsoleWriters.StartingPageMessage($"Started on the {name} palico armor.");
            var leads = page.QuerySelectorAll(".lead");
            int rarity = leads[0].TextContent.ToInt();
            int price = leads[1].TextContent.ToInt();
            int[] defense_info = page.QuerySelector(".table").Children[1].QuerySelectorAll("td").Select(d => d.TextContent.ToInt()).ToArray();
            PalicoArmor armor = new PalicoArmor() {
                pa_name = name,
                pa_description = description,
                pa_rarity = rarity,
                pa_price = price,
                pa_defense = defense_info[0],
                pa_fire = defense_info[1],
                pa_water = defense_info[2],
                pa_thunder = defense_info[3],
                pa_ice = defense_info[4],
                pa_dragon = defense_info[5],
                craft_items = new List<PalicoCraftItem>()
            };
            var craft_table = page.QuerySelector(".table-sm").FirstElementChild.QuerySelectorAll("td");
            for (int i = 0; i < craft_table.Length; i += 2) {
                int item_id = (await Items.GetItemFromDB(craft_table[i].TextContent)).id;
                int quantity = craft_table[i+1].TextContent.ToInt();
                string armor_name = armor.pa_name;
                armor.craft_items.Add(new PalicoCraftItem() {
                    palico_item = armor_name,
                    item_id = item_id,
                    quantity = quantity
                });
            }
            ConsoleWriters.CompletionMessage($"Finished adding the {name} palico armor!");
            return armor;
        }
    }
}