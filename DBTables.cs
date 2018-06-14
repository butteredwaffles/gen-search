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
        [Unique, Indexed]
        public string name {get; set;}
        public string description {get; set;}
        public int rarity {get; set;}
        public int max_stack {get; set;}
        public int sell_price {get; set;}
        public string combinations {get; set;}

        [ManyToMany(typeof(MonsterDrop))]
        public List<Monster> drops_from {get; set;}
    }

    [Table("Monsters")]
    public class Monster {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [Unique, Indexed]
        public string name {get; set;}
        public int base_hp {get; set;}

        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_carve {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_shiny {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_capture {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_head {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_claws {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_tail {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_shell {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_fin {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_back {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_leg {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_wings {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_sponge {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_uvula {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_body {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_shell_1 {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_shell_2 {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_ears {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_chest {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_trunk {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_feelers {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_wingarms {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_claw_right {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_claw_left {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_tentacle_right {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_wound_tentacle_left {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_tail_carve {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> lr_tail_mine {get; set;}

        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_carve {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_tailcarve {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_shiny {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_capture {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_head {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_claws {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_tail {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_shell {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_fin {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_back {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_leg {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_wings {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_sponge {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_uvula {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_body {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_shell_1 {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_shell_2 {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_ears {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_chest {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_trunk {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_feelers {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_wingarms {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_claw_right {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_claw_left {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_tentacle_right {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_wound_tentacle_left {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_tail_carve {get; set;}
        [ManyToMany(typeof(MonsterDrop))]
        public List<Item> hr_tail_mine {get; set;}
        
        
    }

    public class MonsterDrop {
        [ForeignKey(typeof(Monster))]
        public int monid {get; set;}
        [ForeignKey(typeof(Item))]
        public int itemid {get; set;}
        public int dropChance {get; set;}
    }
}