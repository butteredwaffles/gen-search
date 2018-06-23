using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using SQLite;
using SQLiteNetExtensionsAsync;

namespace Gensearch
{
    public class Quests
    {
        public async Task GetQuests(string addr) {
            int throttle = 3;
            try {
                var config = Configuration.Default.WithDefaultLoader();
                var context = BrowsingContext.New(config);
                var page = await context.OpenAsync(addr);
                List<Task> tasks = new List<Task>();
                if (!addr.Contains("event")) { 
                    foreach (var table in page.QuerySelectorAll(".table")) {
                        var rows = table.QuerySelectorAll("tr").Skip(1).ToArray();
                        for (int i = 0; i < rows.Length; i += 2) {
                            var qinfo_tds = rows[i].QuerySelectorAll("td");
                            // Have to get the key and prowler data off of the search page, as it's not available on the individual pages
                            // Could make unstable value more filtered, but /shrug
                            bool isKey = qinfo_tds[0].FirstElementChild != null;
                            bool isUnstable = rows[i].TextContent.Contains("UNSTABLE");
                            string address = qinfo_tds[1].FirstElementChild.GetAttribute("href");
                            bool isProwler = qinfo_tds[2].FirstElementChild != null;
                            tasks.Add(GetQuest(isKey, isUnstable, isProwler, address));

                            if (tasks.Count == throttle) {
                                Task completed = await Task.WhenAny(tasks);
                                tasks.Remove(completed);
                            }
                        }
                    }
                }
                else {
                    // Event quests do not have a 'key' value, so have to handle it slightly differently
                    // In addition, there is only one table, so no need for an extra loop
                    var rows = page.QuerySelector(".table").QuerySelectorAll("tr").Skip(1).ToArray();
                    for (int i = 0; i < rows.Length; i+= 2) {
                        var tds = rows[i].QuerySelectorAll("td");
                        bool isProwler = tds[1].FirstElementChild != null;
                        bool isUnstable = rows[i].TextContent.Contains("UNSTABLE");
                        string address = tds[0].FirstElementChild.GetAttribute("href");
                        tasks.Add(GetQuest(false, isUnstable, isProwler, address));

                        if (tasks.Count == throttle) {
                            Task completed = await Task.WhenAny(tasks);
                            tasks.Remove(completed);
                        }
                    }
                }
                await Task.WhenAll(tasks);
            }
            catch {
                Console.WriteLine("Error getting page. Pausing for sixty seconds.");
                Thread.Sleep(60000);
                await GetQuests(addr);
            }
        }

        public async Task GetQuest(bool isKey, bool isUnstable, bool isProwler, string address) {
            try {
                var page = await BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(address);
                var db = new SQLiteAsyncConnection("data/mhgen.db");
                await db.CreateTablesAsync<Quest, Goal, QuestMonster, QuestBoxItem, QuestUnlock>();


                Regex intsOnly = new Regex(@"[^\d]");

                var general_qdata = page.QuerySelectorAll(".lead");
                string questName = page.QuerySelector("[itemprop=\"name\"]").TextContent.Trim();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Getting data for the quest '" + questName + "'.");
                Console.ResetColor();
                string questDescription = page.QuerySelector("[itemprop=\"description\"]").TextContent;
                int timeLimit = Convert.ToInt32(intsOnly.Replace(general_qdata[2].TextContent, ""));
                int contractFee = Convert.ToInt32(intsOnly.Replace(general_qdata[3].TextContent, ""));
                Goal goal = await GetGoal(general_qdata[0], db);
                Goal subgoal = await GetGoal(general_qdata[1], db);
                var is_none = await db.QueryAsync<Goal>("select * from QuestGoals where goal_description = ?", "None");
                await db.InsertAsync(goal);
                if (subgoal.goal_description != "None"){
                    await db.InsertAsync(subgoal);
                }
                else if (subgoal.goal_description == "None" && is_none.Count == 0) {
                    await db.InsertAsync(subgoal);
                }
                

                // Only need the type from the main goal.
                string questType = goal.goal_description.Split(' ')[0];
                Quest quest = new Quest() {
                    quest_name = questName,
                    quest_description = questDescription,
                    quest_type = questType,
                    isKey = isKey ? "yes" : "no",
                    isProwler = isProwler ? "yes" : "no",
                    isUnstable = isUnstable ? "yes" : "no",
                    timeLimit = timeLimit,
                    contractFee = contractFee,
                    goalid = goal.id,
                    subgoalid = subgoal.id
                    };
                await db.InsertAsync(quest);

                // Quest Boxes
                foreach (var box in page.QuerySelectorAll(".card-header")) {
                    string box_type = box.TextContent.Trim();
                    await GetBox(box.NextElementSibling, box_type, quest.id, db);
                }

                var mon_table = page.QuerySelectorAll("h3")[1].NextElementSibling;
                await GetQuestMonsters(mon_table, db, quest.id);

                foreach (var alert in page.QuerySelectorAll(".alert-info")) {
                    await GetQuestUnlocks(alert, quest.id, db);
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Finished with the quest '" + questName + "'!");
                Console.ResetColor();
            }
            catch (Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                if (ex is SQLiteException) {
                    Console.WriteLine(String.Format("The quest ({0}) is already present in the database!", address));
                    Console.ResetColor();
                    return;
                }
                // Some quests just don't like to load. In this case, have to loop them through lots of times until they decide to...
                // As far as I can tell, there's no way to really fix this except by brute force.
                Console.WriteLine("Error on address " + address + ". Retrying in three seconds.");
                Console.ResetColor();
                Thread.Sleep(3000);
                await GetQuest(isKey, isUnstable, isProwler, address);
            }
        }

        public async Task<Goal> GetGoal(IElement wrapper, SQLiteAsyncConnection db) {
            Regex rewardRegex = new Regex(@"\d*");
            string goalDescription = wrapper.TextContent;
            var no_goal = await db.QueryAsync<Goal>("select * from QuestGoals where goal_description = ?", "None");
            if (goalDescription == "None" && no_goal.Count == 0) {
                return new Goal() {
                    zenny_reward = 0,
                    hrp_reward = 0,
                    wycadpts_reward = 0,
                    goal_description = goalDescription
                };
            }
            else if (goalDescription == "None" && no_goal.Count >= 1) {
                return no_goal[0];
            }
            var ok = rewardRegex.Matches(wrapper.NextElementSibling.TextContent.Trim()).ToArray();
            int[] goalInfo = ok.Select(m => m.Value).Where(m => !string.IsNullOrEmpty(m)).Select(m => Convert.ToInt32(m)).ToArray();
            int z_reward = goalInfo[0];
            int hrp_reward = goalInfo[1];
            int wy_reward = goalInfo[2];
            return new Goal() {
                zenny_reward = z_reward,
                hrp_reward = hrp_reward,
                wycadpts_reward = wy_reward,
                goal_description = goalDescription
            };
        }

        public async Task GetBox(IElement wrapper, string boxname, int quest_id, SQLiteAsyncConnection db) {
            Regex intsOnly = new Regex(@"[^\d]");
            if (boxname != "Supplies") {
                foreach (var row in wrapper.QuerySelectorAll("tr")) {
                    var tds = row.QuerySelectorAll("td");
                    var iteminfo = tds[0].FirstElementChild.TextContent.Trim();
                    string itemname = "";
                    if (iteminfo.Any(char.IsDigit)) {String.Join(" ", iteminfo.Split(' ').SkipLast(1)).Trim(); }
                    else { itemname = iteminfo; }
                    var itemdata = await db.QueryAsync<Item>("select * from Items where item_name = ?", itemname);
                    int quantity = 1;
                    if (intsOnly.Replace(iteminfo, "") != "") {
                        quantity = Convert.ToInt32(intsOnly.Replace(iteminfo, ""));
                    }
                    int appearchance = Convert.ToInt32(intsOnly.Replace(tds[1].TextContent, ""));
                    await db.InsertAsync(new QuestBoxItem() {
                        box_type = boxname,
                        questid = quest_id,
                        itemid = itemdata[0].id,
                        quantity = quantity,
                        appear_chance = appearchance
                    });
                }
            }
            else {
                foreach (var row in wrapper.QuerySelectorAll("tr")) {
                    var tds = row.QuerySelectorAll("td");
                    string itemname = tds[1].TextContent.Trim();
                    int quantity = Convert.ToInt32(intsOnly.Replace(tds[2].TextContent, ""));
                    await db.InsertAsync(new QuestBoxItem() {
                        box_type = boxname,
                        questid = quest_id,
                        itemid = (await db.QueryAsync<Item>("select * from Items where item_name = ?", itemname))[0].id,
                        quantity = quantity,
                    });
                }
            }
        }

        public async Task GetQuestMonsters(IElement wrapper, SQLiteAsyncConnection db, int quest_id) {
            // Sometimes there are no monsters that can appear in a quest
            if (wrapper == null) {return;}

            Regex intsOnly = new Regex(@"[^\d]");
            Regex decimalsOnly = new Regex(@"[^\d.]");
            foreach (var row in wrapper.QuerySelectorAll("tbody tr")) {
                var tds = row.QuerySelectorAll("td");
                string monname = tds[0].TextContent.Trim();
                int monid = (await db.QueryAsync<Monster>("select * from Monsters where mon_name = ?", monname))[0].id;
                string isSpecial = "";
                if (tds[1].TextContent.Contains("INTRUDER") || tds[1].TextContent.Contains("HYPER")) {
                    isSpecial = tds[1].TextContent.Trim().ToLower();
                }
                else {
                    isSpecial = "no";
                }
                int amount = Convert.ToInt32(tds[2].TextContent);
                int hp = Convert.ToInt32(intsOnly.Replace(tds[3].TextContent, ""));
                double stag = Convert.ToDouble(decimalsOnly.Replace(tds[4].TextContent, ""));
                double atk = Convert.ToDouble(decimalsOnly.Replace(tds[5].TextContent, ""));
                double def = Convert.ToDouble(decimalsOnly.Replace(tds[6].TextContent, ""));
                double exh = Convert.ToDouble(decimalsOnly.Replace(tds[7].TextContent, ""));
                double diz = Convert.ToDouble(decimalsOnly.Replace(tds[8].TextContent, ""));
                double mnt = Convert.ToDouble(decimalsOnly.Replace(tds[4].TextContent, ""));
                await db.InsertAsync(new QuestMonster() {
                    questid = quest_id,
                    monsterid = monid,
                    amount = amount,
                    isSpecial = isSpecial,
                    mon_hp = hp,
                    stag_multiplier = stag,
                    atk_multiplier = atk,
                    def_multiplier = def,
                    exh_multiplier = exh,
                    diz_multiplier = diz,
                    mnt_multiplier = mnt
                });
            }
        }

        public async Task GetQuestUnlocks(IElement wrapper, int quest_id, SQLiteAsyncConnection db) {
            // Unlock_type is always first div, so just use regular queryselector
            try {
                string unlock_type = wrapper.QuerySelector("div").TextContent.Contains("completing") ? "prerequisite" : "unlocks";
                foreach (var quest in wrapper.QuerySelectorAll("div").Skip(1)) {
                    await db.InsertAsync(new QuestUnlock() {
                        unlock_type = unlock_type,
                        questid = quest_id,
                    });
                }
            }
            catch (NullReferenceException ex) {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Errored on quest id " + quest_id.ToString() + ".");
            }
        }       
    }
}