using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using DeepEqual.Syntax;
using Gensearch.Scrapers;
using Xunit;

namespace Gensearch.Tests
{
    public class ArmorTests
    {
        private Armors armors = new Armors();
        private static IConfiguration config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
        private static IBrowsingContext context = BrowsingContext.New(config);


        [Theory]
        [ClassData(typeof(SetInfoTestData))]
        public async Task SetInfoTest(string address, Armors.SetInfo expected) {
            var page = await context.OpenAsync(address);

            Armors.SetInfo actual = armors.GetSetInfo(page);
            actual.ShouldDeepEqual(expected);
        }

        public class SetInfoTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator() {
                yield return new object[] {"http://mhgen.kiranico.com/armor/428-derring-s", new Armors.SetInfo() {
                    armor_set = "[Blademaster] Derring S",
                    rarity = 6,
                    max_upgrade = 7,
                    is_blademaster = true,
                    is_gunner = false,
                    is_male = true,
                    is_female = true,
                    monster_id = -1,
                    piece_descriptions = new string[] {
                        "Head armor created for the Caravan. It crowns a proud hunter indeed.",
                        "Chest armor created for the Caravan and imbued with hopes for a safe hunt.",
                        "Armguards created for the Caravan to fit the hands of each wearer just right.",
                        "Waist armor created for the Caravan. Nothing special, but does what it's supposed to.",
                        "Leg armor created for the Caravan. Might outlast your legs on long caravan treks."
                    }
                }};
                yield return new object[] {"http://mhgen.kiranico.com/armor/435-damascus-r", new Armors.SetInfo() {
                    armor_set = "[Gunner] Damascus R",
                    rarity = 6,
                    max_upgrade = 7,
                    is_blademaster = false,
                    is_gunner = true,
                    is_male = true,
                    is_female = true,
                    monster_id = -1,
                    piece_descriptions = Enumerable.Repeat("Crafted using Hyper materials and special techniques to grant new capabilities.", 5).ToArray()
                }};
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}