using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Gensearch.Scrapers
{
    [Table("Armors")]
    public class Armor {
        [PrimaryKey, AutoIncrement]
        public int armor_id {get; set;}
        public string armor_set {get; set;}
        public string armor_name {get; set;}
        public string armor_description {get; set;}

        public int min_armor_defense {get; set;}
        public int max_armor_defense {get; set;}
        public int fire_def {get; set;}
        public int water_def {get; set;}
        public int thunder_def {get; set;}
        public int ice_def {get; set;}
        public int dragon_def {get; set;}

        public int slots {get; set;}
        public int rarity {get; set;}
        public int max_upgrade {get; set;}
        [ForeignKey(typeof(Monster))]
        public int monster_id {get; set;}

        public bool is_blademaster {get; set;}
        public bool is_gunner {get; set;}
        public bool is_male {get; set;}
        public bool is_female {get; set;}

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<ArmorSkill> skills {get; set;}
    }

    [Table("ArmorSkills")]
    public class ArmorSkill {
        [PrimaryKey, AutoIncrement]
        public int as_id {get; set;}
        [ForeignKey(typeof(Armor))]
        public int armor_id {get; set;}
        [ForeignKey(typeof(Skill))]
        public int skill_id {get; set;}
        public int skill_quantity {get; set;}
    }
}