using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using ShellProgressBar;
using SQLite;

namespace Gensearch.Scrapers
{
    public class HunterArts
    {
        private SQLiteAsyncConnection db = GenSearch.db;

        public async Task GetArts(string address) {
            var config = Configuration.Default.WithDefaultLoader().WithJavaScript().WithCss();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync(address);
            await db.CreateTablesAsync<HunterArt, HunterArtUnlock>();
            var trs = page.QuerySelector(".table").QuerySelectorAll("tr").Skip(1);
            var options = new ProgressBarOptions
            {
                ProgressBarOnBottom = true
            };

            using (var progress = new ProgressBar(trs.Count(), "Starting hunter art retrieval.", options)) {
                List<Task> tasks = new List<Task>();
                foreach (var tr in trs) {
                    HunterArt art = GetArt(tr);
                    await db.InsertAsync(art);
                    List<HunterArtUnlock> unlocks = GetArtUnlockQuests(tr, art.art_id);
                    await db.InsertAllAsync(unlocks);
                    progress.Tick($"Inserted the hunter art {art.art_name}.");
                }
                progress.Message = "Done!";
            }
        }

        public HunterArt GetArt(IElement wrapper) {
            var tds = wrapper.QuerySelectorAll("td");
            string art_name = tds[0].TextContent;
            int gauge_size = Convert.ToInt32(tds[1].TextContent);
            string description = tds[2].TextContent;
            return new HunterArt() {
                art_name = art_name,
                art_gauge = gauge_size,
                art_description = description
            };
        }

        public List<HunterArtUnlock> GetArtUnlockQuests(IElement wrapper, int art_id) {
            List<HunterArtUnlock> unlocks = new List<HunterArtUnlock>();
            foreach (var a in wrapper.QuerySelectorAll("a")) {
                Quest quest = Quests.GetQuestFromDB(a.TextContent).Result;
                unlocks.Add(new HunterArtUnlock() {
                    quest_id = quest.id,
                    art_id = art_id
                });
            }
            return unlocks;
        }
    }
}