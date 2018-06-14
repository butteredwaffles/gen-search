using System;
using SQLite;

namespace Gensearch
{
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
}