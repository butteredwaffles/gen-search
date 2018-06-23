using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using DeepEqual.Syntax;
using SQLite;
using Xunit;

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
            List<MonsterPart> part_data = monsterManager.GetParts(page, 4);
            part_values.ShouldDeepEqual(part_data);
        }
    }
}