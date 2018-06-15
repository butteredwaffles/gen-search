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
        public string name {get; set;}
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
        public string name {get; set;}
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
        public string name {get; set;}
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
        public Monster Monster {get; set;}
        [OneToOne]
        public MonsterPart source {get; set;}
    }

}