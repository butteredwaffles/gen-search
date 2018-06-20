using System;
using System.Threading.Tasks;
using Xunit;

namespace Gensearch.Tests
{
    public class Item_Tests
    {
        private Items itemManager = new Items();

        /// <summary>
        /// Tests against a normal item without extra symbols.
        /// </summary>
        [Fact]
        public async Task PlainItemTest() {
            string dash_url = "http://mhgen.kiranico.com/item/mega-dash-juice";
            string[] dash_values = new string[] {
                "Mega Dash Juice",
                "Lets you run without tiring for longer than regular Dash Juice does.",
                "3",
                "5",
                "205z",
                "Dash Extract + Well-done Steak"
            };
            string[] dash_data = await this.itemManager.GetItem(dash_url);
            Assert.Equal(dash_values, dash_data);    
        }

        /// <summary>
        /// Tests against an item with symbols in the name.
        /// </summary>
        [Fact]
        public async Task SymbolItemTest() {
            string bomb_url = "http://mhgen.kiranico.com/item/barrel-bomb-l+";
            string[] bomb_values = new string[] {
                "Barrel Bomb L+",
                "Upgraded Large Barrel Bomb. Effective against large monsters.",
                "4",
                "2",
                "80z",
                "Scatterfish + Barrel Bomb L"
            };
            string[] bomb_data = await this.itemManager.GetItem(bomb_url);
            Assert.Equal(bomb_values, bomb_values);
        }

    }
}