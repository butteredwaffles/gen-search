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

    [Table("PalicoWeaponCraftItems")]
    public class PalicoWeaponCraftItem {
        [PrimaryKey, AutoIncrement]
        public int pwc_id {get; set;}
        [ForeignKey(typeof(PalicoWeapon))]
        public int pw_id {get; set;}
        [ForeignKey(typeof(Item))]
        public int item_id {get; set;}
        public int quantity {get; set;}
    }


}