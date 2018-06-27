using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Gensearch
{
    [Table("Items")]
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [Unique]
        public string item_name {get; set;}
        public string description {get; set;}
        public int rarity {get; set;}
        public int max_stack {get; set;}
        public int sell_price {get; set;}
        public string combinations {get; set;}
    }

    [Table("Monsters")]
    public class Monster {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [Unique]
        public string mon_name {get; set;}
        public int base_hp {get; set;}
        public double base_size {get; set;}
        public double small_size {get; set;}
        public double silver_size {get; set;}
        public double king_size {get; set;}
    }

    [Table("MonsterParts")]
    public class MonsterPart {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        public string part_name {get; set;}
        public int stagger_value {get; set;}
        public string extract_color {get; set;}

        [ForeignKey(typeof(Monster))]
        public int monsterid {get; set;}
        [OneToOne]
        public Monster monster {get; set;}
    }

    [Table("MonsterDrops")]
    public class MonsterDrop {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [ForeignKey(typeof(Item))]
        public int itemid {get; set;}
        [ForeignKey(typeof(Monster))]
        public int monsterid {get; set;}
        [ForeignKey(typeof(MonsterPart))]
        public int sourceid {get; set;}
        public string rank {get; set;}
        public int drop_chance {get; set;}
        public int quantity {get; set;}

        [OneToOne]
        public Item item {get; set;}
        [OneToOne]
        public Monster monster {get; set;}
        [OneToOne]
        public MonsterPart source {get; set;}
    }

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

    [Table("SwordValues")]
    public class SwordValues {
        [PrimaryKey, AutoIncrement]
        public int sword_id {get; set;}
        public string sword_class {get; set;}
        [Unique, NotNull]
        public string sword_name {get; set;}
        public string sword_set_name {get; set;}
        [ForeignKey(typeof(Monster))]
        public int monster_id {get; set;}
        [NotNull]
        public int raw_dmg {get; set;}
        public int affinity {get; set;}
        [ForeignKey(typeof(SharpnessValue))]
        public int sharp_0_id {get; set;}
        [ForeignKey(typeof(SharpnessValue))]
        public int sharp_1_id {get; set;}
        [ForeignKey(typeof(SharpnessValue))]
        public int sharp_2_id {get; set;}
        [NotNull]
        public int slots {get; set;}
        [NotNull]
        public int rarity {get; set;}
        // Going by names instead of values because odds are the weapon it upgrades into will not be in the database yet
        public string upgrades_into {get; set;}
        [NotNull]
        public int price {get; set;}

        [OneToOne]
        public SharpnessValue sharpness_0 {get; set;}
        [OneToOne]
        public SharpnessValue sharpness_1 {get; set;}
        [OneToOne]
        public SharpnessValue sharpness_2 {get; set;}
        [OneToOne]
        public Monster monster {get; set;}

        [Ignore]
        public List<ElementDamage> element {get; set;}
    }

    [Table("HuntingHorns")]
    public class HuntingHorn {
        [PrimaryKey, AutoIncrement]
        public int hh_id {get; set;}
        [ForeignKey(typeof(SwordValues)), NotNull]
        public int sword_id {get; set;}
        public string notes {get; set;}
    }

    [Table("PhialAndShellWeapons")]
    public class PhialOrShellWeapon {
        [PrimaryKey, AutoIncrement]
        public int pw_id {get; set;}
        [ForeignKey(typeof(SwordValues)), NotNull]
        public int sword_id {get; set;}
        public string phial_or_shell_type {get; set;}
    }

    [Table("ElementDamages")]
    public class ElementDamage {
        [PrimaryKey, AutoIncrement]
        public int elem_id {get; set;}
        [NotNull]
        public int weapon_id {get; set;}
        [NotNull]
        public string elem_type {get; set;} // fire, water, ice, etc.
        [NotNull]
        public int elem_amount {get; set;}
    }

    [Table("SharpnessValues")]
    public class SharpnessValue {
        [PrimaryKey, AutoIncrement]
        public int sharp_id {get; set;}
        [NotNull]
        public int handicraft_modifier {get; set;}

        // Values are tiny, small, medium, large
        public int red_sharpness_length {get; set;}
        public int orange_sharpness_length {get; set;}
        public int yellow_sharpness_length {get; set;}
        public int green_sharpness_length {get; set;}
        public int blue_sharpness_length {get; set;}
        public int white_sharpness_length {get; set;}
    }

    [Table("CraftItems")]
    public class CraftItem {
        [PrimaryKey, AutoIncrement]
        public int craft_id {get; set;}
        // Because it can also be from a selected group of items such as "Ore" or "Insect", using names
        public string item_name {get; set;}
        [NotNull]
        public string creation_type {get; set;}
        [NotNull]
        public int creation_id {get; set;}
        [NotNull]
        public int quantity {get; set;}
        [NotNull]
        public string unlocks_creation {get; set;} // either no or yes
        public string is_scrap {get; set;} // if a scrap, it's a byproduct
        public string usage {get; set;} // create or upgrade

    }

    [Table("Bows")]
    public class Bow {
        [PrimaryKey, AutoIncrement]
        public int bow_id {get; set;}
        [NotNull]
        public string bow_name {get; set;}
        [NotNull]
        public int bow_damage {get; set;}
        [NotNull]
        public string arc_type {get; set;}

        public string level_one_charge {get; set;}
        public string level_two_charge {get; set;}
        public string level_three_charge {get; set;}
        public string level_four_charge {get; set;}

        public string supported_coatings {get; set;}
        [NotNull]
        public int slots {get; set;}
        [NotNull]
        public int rarity {get; set;}

    }
}