using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Gensearch.Helpers;
using ShellProgressBar;
using SQLite;

namespace Gensearch.Scrapers
{
    public class Decorations
    {
        private SQLiteAsyncConnection db = GenSearch.db;

        public async Task GetDecorations(string address) {
            var config = Configuration.Default.WithDefaultLoader().WithJavaScript().WithCss();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync(address);
            await db.CreateTablesAsync<Decoration, DecorationCombination>();

            var trs = page.QuerySelector(".table").QuerySelectorAll("tr");
            List<Task> tasks = new List<Task>();
            var options = new ProgressBarOptions { ProgressBarOnBottom = true };
            using (var progress = new ProgressBar(trs.Length, "Starting decoration retrieval.", options)) {
                foreach (var tr in trs) {
                    Decoration deco = await GetDecoration(tr);
                    await db.InsertAsync(deco);
                    List<DecorationCombination> combos = await GetDecorationCombinations(tr, deco.deco_id);
                    await db.InsertAllAsync(combos);
                    progress.Tick($"Inserted {deco.deco_name} into the database.");
                }
                progress.Message = "Done!";
            }
        }

        public async Task<Decoration> GetDecoration(IElement wrapper) {
            var tds = wrapper.QuerySelectorAll("td");
            string name = tds[0].FirstElementChild.TextContent;
            int slot_req = tds[1].TextContent.Count(c => c == '◯');
            string positive_skill = (await Skills.GetSkillFromDB(tds[2].TextContent)).skill_tree;
            int positive_skill_effect = Convert.ToInt32(tds[3].TextContent);
            string negative_skill = "none";
            int negative_skill_effect = 0;
            if (tds[4].TextContent != "") {
                negative_skill = (await Skills.GetSkillFromDB(tds[4].TextContent)).skill_tree;
                negative_skill_effect = Convert.ToInt32(tds[5].TextContent);
            }
            return new Decoration() {
                deco_name = name,
                deco_slot_requirement = slot_req,
                positive_skill_tree = positive_skill,
                positive_skill_effect = positive_skill_effect,
                negative_skill_tree = negative_skill,
                negative_skill_effect = negative_skill_effect
            };
        }

        public async Task<List<DecorationCombination>> GetDecorationCombinations(IElement wrapper, int deco_id) {
            var divs = wrapper.Children[6].QuerySelectorAll("div");
            List<DecorationCombination> combinations = new List<DecorationCombination>();
            switch(divs.Length) {
                case 2:
                    combinations.Add(new DecorationCombination() {
                        deco_id = deco_id,
                        item_1_id = (await Items.GetItemFromDB(divs[0].FirstElementChild.TextContent)).id,
                        item_1_quantity = Convert.ToInt32(divs[0].TextContent.ToInt()),
                        item_2_id = (await Items.GetItemFromDB(divs[1].FirstElementChild.TextContent)).id,
                        item_2_quantity = Convert.ToInt32(divs[1].TextContent.ToInt()),
                        item_3_id = -1,
                        item_3_quantity = 0
                    });
                    break;
                case 3:
                    combinations.Add(new DecorationCombination() {
                        deco_id = deco_id,
                        item_1_id = (await Items.GetItemFromDB(divs[0].FirstElementChild.TextContent)).id,
                        item_1_quantity = Convert.ToInt32(divs[0].TextContent.ToInt()),
                        item_2_id = (await Items.GetItemFromDB(divs[1].FirstElementChild.TextContent)).id,
                        item_2_quantity = Convert.ToInt32(divs[1].TextContent.ToInt()),
                        item_3_id = (await Items.GetItemFromDB(divs[2].FirstElementChild.TextContent)).id,
                        item_3_quantity = Convert.ToInt32(divs[2].TextContent.ToInt()),
                    });
                    break;
                case 5:
                    combinations.Add(new DecorationCombination() {
                        deco_id = deco_id,
                        item_1_id = (await Items.GetItemFromDB(divs[0].FirstElementChild.TextContent)).id,
                        item_1_quantity = Convert.ToInt32(divs[0].TextContent.ToInt()),
                        item_2_id = (await Items.GetItemFromDB(divs[1].FirstElementChild.TextContent)).id,
                        item_2_quantity = Convert.ToInt32(divs[1].TextContent.ToInt()),
                        item_3_id = (await Items.GetItemFromDB(divs[2].FirstElementChild.TextContent)).id,
                        item_3_quantity = Convert.ToInt32(divs[2].TextContent.ToInt()),
                    });
                    combinations.Add(new DecorationCombination() {
                        deco_id = deco_id,
                        item_1_id = (await Items.GetItemFromDB(divs[3].FirstElementChild.TextContent)).id,
                        item_1_quantity = Convert.ToInt32(divs[3].TextContent.ToInt()),
                        item_2_id = (await Items.GetItemFromDB(divs[4].FirstElementChild.TextContent)).id,
                        item_2_quantity = Convert.ToInt32(divs[4].TextContent.ToInt()),
                        item_3_id = -1,
                        item_3_quantity = 0,
                    });
                    break;
                case 6:
                    combinations.Add(new DecorationCombination() {
                        deco_id = deco_id,
                        item_1_id = (await Items.GetItemFromDB(divs[0].FirstElementChild.TextContent)).id,
                        item_1_quantity = Convert.ToInt32(divs[0].TextContent.ToInt()),
                        item_2_id = (await Items.GetItemFromDB(divs[1].FirstElementChild.TextContent)).id,
                        item_2_quantity = Convert.ToInt32(divs[1].TextContent.ToInt()),
                        item_3_id = (await Items.GetItemFromDB(divs[2].FirstElementChild.TextContent)).id,
                        item_3_quantity = Convert.ToInt32(divs[2].TextContent.ToInt()),
                    });
                    combinations.Add(new DecorationCombination() {
                        deco_id = deco_id,
                        item_1_id = (await Items.GetItemFromDB(divs[3].FirstElementChild.TextContent)).id,
                        item_1_quantity = Convert.ToInt32(divs[3].TextContent.ToInt()),
                        item_2_id = (await Items.GetItemFromDB(divs[4].FirstElementChild.TextContent)).id,
                        item_2_quantity = Convert.ToInt32(divs[4].TextContent.ToInt()),
                        item_3_id = (await Items.GetItemFromDB(divs[5].FirstElementChild.TextContent)).id,
                        item_3_quantity = Convert.ToInt32(divs[5].TextContent.ToInt()),
                    });
                    break;
            }
            return combinations;
        }
    }
}