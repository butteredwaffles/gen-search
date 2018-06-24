using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using DeepEqual.Syntax;
using SQLite;
using Xunit;
using Gensearch.Scrapers;

namespace Gensearch.Tests
{
    public class MonsterTests
    {
        private Monsters monsterManager = new Monsters();
        private SQLiteAsyncConnection db = new SQLiteAsyncConnection(@"data\mhgen.db");
        private string testAddress = "http://mhgen.kiranico.com/monster/seltas";

        [Fact]
        public async Task PartsTest() {
            var page = await BrowsingContext.New(Configuration.Default.WithDefaultLoader())
            .OpenAsync(testAddress);

            int seltasid = Convert.ToInt32((await db.Table<Monster>().Where(n => n.mon_name == "Seltas").FirstAsync()).id);
            List<MonsterPart> part_values = new List<MonsterPart>() {
                new MonsterPart() {part_name = "Head", stagger_value = 180, monsterid = seltasid, extract_color = "red"},
                new MonsterPart() {part_name = "Feet", stagger_value = 60, monsterid = seltasid, extract_color = "white"},
                new MonsterPart() {part_name = "Stomach", stagger_value = 100, monsterid = seltasid, extract_color = "orange"},
            };
            List<MonsterPart> part_data = monsterManager.GetParts(page, seltasid);
            part_values.ShouldDeepEqual(part_data);
        }

        [Fact]
        public async Task DropsTest() {
            var page = await BrowsingContext.New(Configuration.Default.WithDefaultLoader())
            .OpenAsync(testAddress);
            int seltasid = Convert.ToInt32((await db.Table<Monster>().Where(n => n.mon_name == "Seltas").FirstAsync()).id);
            var seltpartsids = await db.Table<MonsterPart>().Where(p => p.monsterid == seltasid).ToListAsync();

            List<MonsterDrop> drop_values = new List<MonsterDrop>() {
                new MonsterDrop() {itemid = Convert.ToInt32((await db.Table<Item>().Where(n => n.item_name == "Seltas Carapace").FirstAsync()).id),
                monsterid = seltasid, sourceid = seltpartsids.Where(p => p.part_name == "Body Carve").First().id,
                rank = "Low", drop_chance = 60, quantity = 1},
                new MonsterDrop() {itemid = Convert.ToInt32((await db.Table<Item>().Where(n => n.item_name == "Monster Broth").FirstAsync()).id),
                monsterid = seltasid, sourceid = seltpartsids.Where(p => p.part_name == "Capture").First().id,
                rank = "High", drop_chance = 5, quantity = 2},
            };
            List<MonsterDrop> drop_data = await monsterManager.GetDrops(page, seltasid, db);
            Assert.True(drop_values.Where(d => d.itemid == drop_data[0].itemid).Count() == 1 &&
                        drop_values.Where(d => d.itemid == drop_values[1].itemid).Count() == 1);
        }
    }
}