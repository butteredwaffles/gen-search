using System;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using ShellProgressBar;
using SQLite;

namespace Gensearch.Scrapers
{
    public class Skills
    {
        private static SQLiteAsyncConnection db = GenSearch.db;

        public static async Task<Skill> GetSkillFromDB(string name) {
            var skills = await db.Table<Skill>().Where(s => s.skill_tree == name).ToArrayAsync();
            return skills[0];
        }

        public async Task GetSkills(string address) {
            var config = Configuration.Default.WithDefaultLoader().WithJavaScript().WithCss();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync(address);
            await db.CreateTableAsync<Skill>();

            var trs = page.QuerySelector(".table").QuerySelectorAll("tr");
            string current_skill = "";

            var options = new ProgressBarOptions { ProgressBarOnBottom = true };
            using (var progress = new ProgressBar(trs.Length, "Starting skill retrieval.", options)) {
                foreach (var tr in trs) {
                    if (tr.QuerySelector("a") != null) {
                        current_skill = tr.QuerySelector("a").TextContent;
                        progress.Tick($"Working on the {current_skill} skill tree.");
                        continue;
                    }
                    await db.InsertAsync(GetSkill(tr, current_skill));
                    progress.Tick();
                }
                progress.Message = "Done!";
            }
        }

        public Skill GetSkill(IElement wrapper, string current_skill) {
            var tds = wrapper.QuerySelectorAll("td");
            string skill_name = tds[0].TextContent;
            int skill_value = Convert.ToInt32(tds[1].TextContent);
            string description = tds[2].TextContent;
            return new Skill() {
                skill_tree = current_skill,
                skill_name = skill_name,
                skill_value = skill_value,
                skill_description = description
            };
        }
    }
}