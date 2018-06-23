using System;
using System.Collections.Generic;
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
        private SQLiteAsyncConnection db = new SQLiteAsyncConnection(@"C:\Users\jdonu\Desktop\Gensearch\data\mhgen.db");
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

            List<QuestBoxItem> items_values = new List<QuestBoxItem>() {
                new QuestBoxItem() {box_type = "Main Reward C", questid = 0, itemid = 1880, quantity = 2, appear_chance = 50},
                new QuestBoxItem() {box_type = "Main Reward C", questid = 0, itemid = 1880, quantity = 1, appear_chance = 40},
                new QuestBoxItem() {box_type = "Main Reward C", questid = 0, itemid = 1880, quantity = 3, appear_chance = 10}           
            };
            
            List<QuestBoxItem> items_data = await questManager.GetBox(box_element, "Main Reward C", 0, db);
            items_values.ShouldDeepEqual(items_data);
        }
    }
}