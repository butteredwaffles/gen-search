using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using Gensearch.Helpers;
using ShellProgressBar;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;

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
            await db.CreateTablesAsync<Armor, ArmorSkill>();
            
            List<Task> tasks = new List<Task>();
            var options = new ProgressBarOptions { ProgressBarOnBottom = true };
            using (var progress = new ProgressBar(rows.Length, $"Starting armor retrieval. ({address})", options)) {
                foreach (var row in rows) {
                    string addr = ((IHtmlAnchorElement) row.NextElementSibling.FirstElementChild.FirstElementChild).Href;
                    tasks.Add(GetArmor(addr));

                    if (tasks.Count == throttle) {
                        Task completed = await Task.WhenAny(tasks);
                        tasks.Remove(completed);
                    }
                }
            }
            await Task.WhenAll(tasks);
        }

        public async Task GetArmor(string address) {
            IConfiguration config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
            IBrowsingContext context = BrowsingContext.New(config);
            var page = await context.OpenAsync(address);
            
            SetInfo setinfo = GetSetInfo(page);
            List<Armor> armor_pieces = new List<Armor>();
            var skill_table = page.QuerySelectorAll(".table")[1].Children[1].Children.ToArray();
            // Skip the last tr, because that is the total
            foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tbody tr").SkipLast(1)) {
                DefenseInfo definfo = GetArmorPieceDefenseInfo(tr);
                int tr_index = tr.ParentElement.Children.Index(tr);
                ArmorSkillInfo asi = GetArmorSkills(skill_table[tr_index]);
                Armor piece = new Armor() {
                    armor_name = definfo.armor_name,
                    armor_set = setinfo.armor_set,
                    armor_description = setinfo.piece_descriptions[tr_index],
                    rarity = setinfo.rarity,
                    max_upgrade = setinfo.max_upgrade,
                    monster_id = setinfo.monster_id,
                    is_blademaster = setinfo.is_blademaster,
                    is_gunner = setinfo.is_gunner,
                    is_male = setinfo.is_male,
                    is_female = setinfo.is_female,
                    min_armor_defense = definfo.min_defense,
                    max_armor_defense = definfo.max_defense,
                    fire_def = definfo.fire_defense,
                    water_def = definfo.water_defense,
                    thunder_def = definfo.thunder_defense,
                    ice_def = definfo.ice_defense,
                    dragon_def = definfo.dragon_def,
                    slots = asi.slots,
                    skills = asi.skills
                };

                armor_pieces.Add(piece);
            }
            await db.InsertAllWithChildrenAsync(armor_pieces);
        }

        public SetInfo GetSetInfo(IDocument page) {
            var divs = page.QuerySelectorAll(".lead");
            int rarity = divs[0].TextContent.ToInt();
            int max_upgrade = divs[1].TextContent.ToInt();
            List<bool> genderweaponchecks = new List<bool>();
            foreach (int index in new int[] {2, 3, 4, 5}) {
                 genderweaponchecks.Add(divs[index].FirstElementChild.ClassList.Contains("octicon-check"));
            }
            string set_name = page.QuerySelector("[itemprop=\"name\"]").TextContent.Replace("Armor Set", "").Trim();
            int monster_id = -1;
            string[] piece_descriptions;
            if (divs[6].FirstElementChild.TagName == "A") {
                monster_id = Monsters.GetMonsterFromDB(divs[6].FirstElementChild.TextContent).Result.id;
                piece_descriptions = divs.Skip(7).Select(d => d.NextElementSibling.TextContent).ToArray();
            }
            else {
                piece_descriptions = divs.Skip(6).Select(d => d.NextElementSibling.TextContent).ToArray();
            }
            return new SetInfo {
                armor_set = set_name,
                rarity = rarity,
                max_upgrade = max_upgrade,
                is_blademaster = genderweaponchecks[0],
                is_gunner = genderweaponchecks[1],
                is_male = genderweaponchecks[2],
                is_female = genderweaponchecks[3],
                monster_id = monster_id,
                piece_descriptions = piece_descriptions
            };
        }

        public ArmorSkillInfo GetArmorSkills(IElement tr) {
            var tds = tr.QuerySelectorAll("td");
            string name = tds[0].TextContent;
            int slots = tds[1].TextContent.Count(c => c == '◯');
            List<ArmorSkill> skills = new List<ArmorSkill>();

            foreach (var small in tds[2].QuerySelectorAll("small")) {
                string skill_name = String.Join(' ', small.TextContent.Trim().Split(' ').SkipLast(1));
                int skill_id = Skills.GetSkillFromDB(skill_name).Result.skill_id;
                int skill_value = small.TextContent.ToInt();
                skills.Add(new ArmorSkill() {
                    skill_id = skill_id,
                    skill_quantity = skill_value
                });
            }
            return new ArmorSkillInfo() {
                armor_name = name,
                skills = skills,
                slots = slots
            };
        }

        public DefenseInfo GetArmorPieceDefenseInfo(IElement tr) {
            DefenseInfo def_info = new DefenseInfo();
            string[] defenses = tr.Children[1].TextContent.Split('-');
            
            return new DefenseInfo() {
                armor_name = tr.FirstElementChild.TextContent,
                min_defense = defenses[0].ToInt(),
                max_defense = defenses[1].ToInt(),
                fire_defense = tr.Children[2].TextContent.ToInt(),
                water_defense = tr.Children[3].TextContent.ToInt(),
                thunder_defense = tr.Children[4].TextContent.ToInt(),
                ice_defense = tr.Children[5].TextContent.ToInt(),
                dragon_def = tr.Children[6].TextContent.ToInt(),
            }; 
        }

        public class SetInfo {
            public string armor_set {get; set;}
            public int rarity {get; set;}
            public int max_upgrade {get; set;}
            public bool is_blademaster {get; set;}
            public bool is_gunner {get; set;}
            public bool is_male {get; set;}
            public bool is_female {get; set;}
            public int monster_id {get; set;}
            public string[] piece_descriptions {get; set;}
        }

        public class DefenseInfo {
            public string armor_name {get; set;}
            public int min_defense {get; set;}
            public int max_defense {get; set;}
            public int fire_defense {get; set;}
            public int water_defense {get; set;}
            public int thunder_defense {get; set;}
            public int ice_defense {get; set;}
            public int dragon_def {get; set;}
        }

        public class ArmorSkillInfo {
            public string armor_name {get; set;}
            public int slots {get; set;}
            public List<ArmorSkill> skills {get; set;}
        }
    }
}