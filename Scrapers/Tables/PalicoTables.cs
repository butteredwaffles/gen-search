using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Gensearch.Scrapers.Tables
{
    [Table("PalicoWeapons")]
    public class PalicoWeapon {
        [PrimaryKey, AutoIncrement]
        public int pw_id {get; set;}
        public string pw_name {get; set;}
        public string pw_description {get; set;}
        public int pw_rarity {get; set;}
        public int pw_price {get; set;}

        // close range, balanced, boomerang
        public string pw_type {get; set;}
        public int pw_damage {get; set;}
        // cutting or blunt
        public string pw_damage_type {get; set;}
        public int pw_affinity {get; set;}
        // Palico sharpness is thankfully different from hunter sharpness, so it can be stored in a single string
        public string pw_sharpness {get; set;}
        public int pw_boomerang_damage {get; set;}
        public int pw_boomerang_affinity {get; set;}

        public int pw_defense {get; set;}
    }

    [Table("PalicoArmor")]
    public class PalicoArmor {
        [PrimaryKey, AutoIncrement]
        public int pa_id {get; set;}
        public string pa_name {get; set;}
        public string pa_description {get; set;}
        public int pa_rarity {get; set;}
        public int pa_price {get; set;}

        public int pa_defense {get; set;}
        public int pa_fire {get; set;}
        public int pa_water {get; set;}
        public int pa_thunder {get; set;}
        public int pa_ice {get; set;}
        public int pa_dragon {get; set;}
    }

    [Table("PalicoSkills")]
    public class PalicoSkill {
        [PrimaryKey, AutoIncrement]
        public int ps_id {get; set;}
        public string ps_name {get; set;}
        // Defensive, offensive, or passive
        public string ps_type {get; set;}
        public string ps_description {get; set;}
        public int ps_memory_req {get; set;}
        public int ps_learn_level {get; set;}
    }

    [Table("PalicoCraftItems")]
    public class PalicoCraftItems {
        [PrimaryKey, AutoIncrement]
        public int pc_id {get; set;}
        // The name of the item being crafted
        public string palico_item {get; set;}
        [ForeignKey(typeof(Item))]
        public int item_id {get; set;}
        public int quantity {get; set;}
    }



}