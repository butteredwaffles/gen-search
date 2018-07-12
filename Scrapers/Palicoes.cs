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
            armor.craft_items = GetPalicoCrafts(craft_table, armor.pa_name, "armor");
            ConsoleWriters.CompletionMessage($"Finished adding the {name} palico armor!");
            return armor;
        }

        public List<PalicoCraftItem> GetPalicoCrafts(IHtmlCollection<IElement> tds, string palico_item, string type) {
            List<PalicoCraftItem> items = new List<PalicoCraftItem>();
            for (int i = 0; i < tds.Length; i += 2) {
                int item_id = Items.GetItemFromDB(tds[i].TextContent).Result.id;
                int quantity = tds[i+1].TextContent.ToInt();
                items.Add(new PalicoCraftItem() {
                    palico_item = palico_item,
                    item_id = item_id,
                    quantity = quantity,
                    type = type
                });
            }
            return items;
        }

        public async Task GetPalicoWeapons(string address) {
            var page = await context.OpenAsync(address);
            await db.CreateTablesAsync<PalicoWeapon, PalicoCraftItem>();

            int throttle = 7;
            List<Task<PalicoWeapon>> tasks = new List<Task<PalicoWeapon>>();
            List<PalicoWeapon> weapons = new List<PalicoWeapon>();
            foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr").Skip(1)) {
                string wpn_address = tr.QuerySelector("a").GetAttribute("href");
                string wpn_type = tr.QuerySelector("small").TextContent;
                string wpn_damage_type = tr.QuerySelector("div").TextContent.Split(' ')[1].Replace("(", "").Replace(")", "");

                tasks.Add(GetPalicoWeapon(wpn_address, wpn_type, wpn_damage_type));
                if (tasks.Count() == throttle) {
                    Task<PalicoWeapon> completed = await Task.WhenAny(tasks);
                    weapons.Add(completed.Result);
                    tasks.Remove(completed);
                }
            }
            weapons.Concat(await Task.WhenAll(tasks));
            await db.InsertAllAsync(weapons);
            List<PalicoCraftItem> all_craft_items = new List<PalicoCraftItem>();
            foreach (var list in weapons.Select(w => w.craft_items)) {
                all_craft_items.AddRange(list);
            }
            await db.InsertAllAsync(all_craft_items);
        }

        public async Task<PalicoWeapon> GetPalicoWeapon(string address, string weapon_type, string weapon_damage_type) {
            var page = await context.OpenAsync(address);
            var basicinfo = page.QuerySelector("[itemprop=\"gameItem\"]");
            string name = basicinfo.FirstElementChild.TextContent;
            string description = basicinfo.Children[1].TextContent;
            ConsoleWriters.StartingPageMessage($"Started on the {name} palico weapon.");
            var leads = page.QuerySelectorAll(".lead");
            int rarity = leads[0].TextContent.ToInt();
            int price = leads[1].TextContent.ToInt();

            var damage_tr = page.QuerySelector(".table").QuerySelectorAll("tr")[1];

            var melee_data = damage_tr.FirstElementChild.QuerySelectorAll("div");
            int melee_dmg = melee_data[0].TextContent.ToInt();
            int melee_affinity = melee_data[1].TextContent.ToInt();
            string sharpness = damage_tr.QuerySelector("span").ClassName;
            string melee_element = "none";
            int melee_elem_amount = 0;
            if (damage_tr.Children[2].TextContent.Replace(" ", "") != "") {
                melee_element = damage_tr.Children[2].TextContent.Trim().Split(' ')[1];
                melee_elem_amount = damage_tr.Children[2].TextContent.ToInt();
            }

            var boom_data = damage_tr.Children[3].QuerySelectorAll("div");
            int boom_dmg = boom_data[0].TextContent.ToInt();
            int boom_affinity = boom_data[1].TextContent.ToInt();
            string boom_element = "none";
            int boom_elem_amount = 0;
            if (damage_tr.Children[4].TextContent.Replace(" ", "") != "") {
                boom_element = damage_tr.Children[4].TextContent.Trim().Split(' ')[1];
                boom_elem_amount = damage_tr.Children[4].TextContent.ToInt();
            }
            int defense = damage_tr.LastElementChild.TextContent.ToInt();
            
            PalicoWeapon weapon = new PalicoWeapon() {
                pw_name = name,
                pw_description = description,
                pw_affinity = melee_affinity,
                pw_boomerang_affinity = boom_affinity,
                pw_boomerang_damage = boom_dmg,
                pw_boomerang_element = boom_element,
                pw_boomerang_element_amt = boom_elem_amount,
                pw_damage = melee_dmg,
                pw_damage_type = weapon_damage_type,
                pw_defense = defense,
                pw_element = melee_element,
                pw_element_amt = melee_elem_amount,
                pw_price = price,
                pw_rarity = rarity,
                pw_sharpness = sharpness,
                pw_type = weapon_type
            };

            var craft_table = page.QuerySelector(".table-sm").FirstElementChild.QuerySelectorAll("td");
            weapon.craft_items = GetPalicoCrafts(craft_table, name, "weapon");
            ConsoleWriters.CompletionMessage($"Finished adding the {name} palico weapon!");
            return weapon;
        }
    }
}