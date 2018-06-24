using System;
using System.Threading.Tasks;
using DeepEqual.Syntax;
using Gensearch.Scrapers;
using Xunit;

namespace Gensearch.Tests
{
    public class ItemTests
    {
        private Items itemManager = new Items();

        /// <summary>
        /// Tests against a normal item without extra symbols.
        /// </summary>
        [Fact]
        public async Task PlainItemTest() {
            string dash_url = "http://mhgen.kiranico.com/item/mega-dash-juice";
            Item dash_values = new Item() {
                item_name = "Mega Dash Juice",
                description = "Lets you run without tiring for longer than regular Dash Juice does.",
                rarity = 3,
                max_stack = 5,
                sell_price = 205,
                combinations = "Dash Extract + Well-done Steak"
            };
            Item dash_data = await this.itemManager.GetItem(dash_url);
            dash_values.ShouldDeepEqual(dash_data);    
        }

        /// <summary>
        /// Tests against an item with symbols in the name.
        /// </summary>
        [Fact]
        public async Task SymbolItemTest() {
            string bomb_url = "http://mhgen.kiranico.com/item/barrel-bomb-l+";
            Item bomb_values = new Item() {
                item_name = "Barrel Bomb L+",
                description = "Upgraded Large Barrel Bomb. Effective against large monsters.",
                rarity = 4,
                max_stack = 2,
                sell_price = 80,
                combinations = "Scatterfish + Barrel Bomb L"
            };
            Item bomb_data = await this.itemManager.GetItem(bomb_url);
            bomb_values.ShouldDeepEqual(bomb_data);
        }

    }
}