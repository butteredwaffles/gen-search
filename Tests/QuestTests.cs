using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AngleSharp;
using DeepEqual.Syntax;
using SQLite;
using Xunit;

namespace Gensearch.Tests
{
    public class QuestTests
    {
        private Quests questManager = new Quests();
        private SQLiteAsyncConnection db = new SQLiteAsyncConnection(@"data\mhgen.db");
        private string testAddress = "http://mhgen.kiranico.com/quest/guild/0.1-flames-of-savagery-at-usj";

        [Fact]
        public async Task GoalTest() {
            var page = await BrowsingContext.New(Configuration.Default.WithDefaultLoader())
            .OpenAsync(testAddress);
            var goal_element = page.QuerySelector(".lead"); 

            Goal goal_values = new Goal() {
                zenny_reward = 9600,
                hrp_reward = 500,
                wycadpts_reward = 960,
                goal_description = "Hunt a Glavenus"
            };
            
            Goal goal_data = await questManager.GetGoal(goal_element, db);
            goal_values.ShouldDeepEqual(goal_data);
        }

        [Fact]
        public async Task BoxTest() {
            var page = await BrowsingContext.New(Configuration.Default.WithDefaultLoader())
            .OpenAsync(testAddress);
            var box_element = page.QuerySelectorAll(".card-header")[2].NextElementSibling;
            int itemid = Convert.ToInt32((await db.Table<Item>().Where(n => n.item_name == "Studio Pass").FirstAsync()).id);

            List<QuestBoxItem> items_values = new List<QuestBoxItem>() {
                new QuestBoxItem() {box_type = "Main Reward C", questid = 0, itemid = 1880, quantity = 2, appear_chance = 50},
                new QuestBoxItem() {box_type = "Main Reward C", questid = 0, itemid = 1880, quantity = 1, appear_chance = 40},
                new QuestBoxItem() {box_type = "Main Reward C", questid = 0, itemid = 1880, quantity = 3, appear_chance = 10}           
            };
            
            List<QuestBoxItem> items_data = await questManager.GetBox(box_element, "Main Reward C", 0, db);
            items_values.ShouldDeepEqual(items_data);
        }

        [Fact]
        public async Task MonTest() {
            var page = await BrowsingContext.New(Configuration.Default.WithDefaultLoader())
            .OpenAsync(testAddress);
            var box_element = page.QuerySelectorAll("h3")[1].NextElementSibling;
            int glavid = Convert.ToInt32((await db.Table<Monster>().Where(n => n.mon_name == "Glavenus").FirstAsync()).id);
            int maccid = Convert.ToInt32((await db.Table<Monster>().Where(n => n.mon_name == "Great Maccao").FirstAsync()).id);

            List<QuestMonster> monster_values = new List<QuestMonster>() {
                // glavenus
                new QuestMonster() {questid = 0, monsterid = glavid, amount = 1, isSpecial = "no", mon_hp = 4928, 
                stag_multiplier = 1.5, atk_multiplier = 2.3, def_multiplier = .85, exh_multiplier = 1.5, 
                diz_multiplier = 1.2, mnt_multiplier = 1.6},
                // great maccao
                new QuestMonster() {questid = 0, monsterid = maccid, amount = 1, isSpecial = "intruder", mon_hp = 1940,
                stag_multiplier = 1.3, atk_multiplier = 2.3, def_multiplier = .85, exh_multiplier = 1.5,
                diz_multiplier = 1.2, mnt_multiplier = 1.6},
            };
            List<QuestMonster> monster_data = await questManager.GetQuestMonsters(box_element, db, 0);
            monster_values.ShouldDeepEqual(monster_data);
        }
    }
}