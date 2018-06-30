using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Gensearch.Scrapers
{
    [Table("Quests")]
    public class Quest {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [NotNull]
        public string quest_name {get; set;}
        [NotNull]
        public string quest_type {get; set;} // hunt, gather, capture, slay, survive
        [NotNull]
        public string quest_description {get; set;}
        public string isKey {get; set;}
        public string isProwler {get; set;}
        public string isUnstable {get; set;}
        public int timeLimit {get; set;}
        public int contractFee {get; set;}

        [ForeignKey(typeof(Goal))]
        public int goalid {get; set;}
        [ForeignKey(typeof(Goal))]
        public int subgoalid {get; set;}

        [OneToOne]
        public Goal goal {get; set;}
        [OneToOne]
        public Goal subgoal {get; set;}
    }

    [Table("QuestGoals")]
    public class Goal {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        public int zenny_reward {get; set;}
        public int hrp_reward {get; set;}
        public int wycadpts_reward {get; set;}
        public string goal_description {get; set;}
    }

    [Table("QuestMonsters")]
    public class QuestMonster {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [ForeignKey(typeof(Quest))]
        public int questid {get; set;}
        [ForeignKey(typeof(Monster))]
        public int monsterid {get; set;}
        public int amount {get; set;}
        public string isSpecial {get; set;}
        public int mon_hp {get; set;}
        public double stag_multiplier {get; set;}
        public double atk_multiplier {get; set;}
        public double def_multiplier {get; set;}
        public double exh_multiplier {get; set;}
        public double diz_multiplier {get; set;}
        public double mnt_multiplier {get; set;}

        [OneToOne]
        public Monster monster {get; set;}
        [OneToOne]
        public Quest quest {get; set;}
    }

    [Table("QuestBoxItems")]
    public class QuestBoxItem {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [NotNull]
        public string box_type {get; set;} // main reward A, main reward B, supplies, etc...
        [ForeignKey(typeof(Quest)), NotNull]
        public int questid {get; set;}
        [ForeignKey(typeof(Item)), NotNull]
        public int itemid {get; set;}
        public int quantity {get; set;}
        public double appear_chance {get; set;}

        [OneToOne]
        public Quest quest {get; set;}
        [OneToOne]
        public Item item {get; set;}
    }

    [Table("QuestUnlocks")]
    public class QuestUnlock {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [NotNull]
        public string unlock_type {get; set;} // whether this quest is a prereq or unlocks another one
        [ForeignKey(typeof(Quest)), NotNull]
        public int questid {get; set;}

        [OneToOne]
        public Quest quest {get; set;}
    }
}