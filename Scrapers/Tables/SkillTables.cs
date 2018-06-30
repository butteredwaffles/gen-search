using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Gensearch.Scrapers
{
    [Table("Skills")]
    public class Skill {
        [PrimaryKey, AutoIncrement]
        public int skill_id {get; set;}
        public string skill_tree {get; set;}
        public string skill_name {get; set;}
        public int skill_value {get; set;}
        public string skill_description {get; set;}
    }

    [Table("HunterArts")]
    public class HunterArt {
        [PrimaryKey, AutoIncrement]
        public int art_id {get; set;}
        public string art_name {get; set;}
        public int art_gauge {get; set;}
        public string art_description {get; set;}
    }

    [Table("HunterArtUnlocks")]
    public class HunterArtUnlock {
        [PrimaryKey, AutoIncrement]
        public int unlock_id {get; set;}
        [ForeignKey(typeof(HunterArt))]
        public int art_id {get; set;}
        [ForeignKey(typeof(Quest))]
        public int quest_id {get; set;}
    }

    [Table("Decorations")]
    public class Decoration {
        [PrimaryKey, AutoIncrement]
        public int deco_id {get; set;}
        public string deco_name {get; set;}
        public int deco_slot_requirement {get; set;}
        public string positive_skill_tree {get; set;}
        public int positive_skill_effect {get; set;}
        public string negative_skill_tree {get; set;}
        public int negative_skill_effect {get; set;}
    }

    [Table("DecorationCombinations")]
    public class DecorationCombination {
        [PrimaryKey, AutoIncrement]
        public int deco_comb_id {get; set;}
        [ForeignKey(typeof(Decoration))]
        public int deco_id {get; set;}
        [ForeignKey(typeof(Item))]
        public int item_1_id {get; set;}
        public int item_1_quantity {get; set;}
        [ForeignKey(typeof(Item))]
        public int item_2_id {get; set;}
        public int item_2_quantity {get; set;}
        [ForeignKey(typeof(Item))]
        public int item_3_id {get; set;}
        public int item_3_quantity {get; set;}
    }
}