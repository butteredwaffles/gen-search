using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using DeepEqual.Syntax;
using Gensearch.Scrapers;
using Xunit;

namespace Gensearch.Tests
{
    public class BladeWeaponsTests
    {
        private BladeWeapons weaponManager = new BladeWeapons();
        private static IConfiguration config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
        private static IBrowsingContext context = BrowsingContext.New(config);

        [Fact]
        public async Task SharpnessTest() {
            var page = await context.OpenAsync("http://mhgen.kiranico.com/greatsword/sentoryo-calamity");

            List<SharpnessValue> sharpness_data = weaponManager.GetSharpness(page.QuerySelector(".table").QuerySelector("tr"));
            SharpnessValue sharpness_value = new SharpnessValue() {
                red_sharpness_length = 80,
                orange_sharpness_length = 40,
                yellow_sharpness_length = 110,
                green_sharpness_length = 100,
                blue_sharpness_length = 20,
                white_sharpness_length = 0
            };
            sharpness_value.ShouldDeepEqual(sharpness_data[0]);
        }

        [Theory]
        [ClassData(typeof(SwordAttributeTestData))]
        public async Task SwordAttributesTest(string address, int index, SwordValues expected) {
            var page = await context.OpenAsync(address);
            var trs = page.QuerySelector(".table").QuerySelectorAll("tr");
            var crafting = page.QuerySelectorAll("table")[1].QuerySelector("tbody");

            SwordValues received = weaponManager.GetSwordAttributes(page, trs[index], crafting, index).Result;
            expected.ShouldDeepEqual(received);
        }

        public class SwordAttributeTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator() {
                yield return new object[] {"http://mhgen.kiranico.com/huntinghorn/larinoth-ocarina", 0, new SwordValues() {
                    sword_name = "Larinoth Ocarina 1",
                    raw_dmg = 70,
                    slots = 2,
                    rarity = 1,
                    upgrades_into = "none",
                    price = 1000,
                    element = new List<ElementDamage>(),
                    affinity = 0,
                    monster_id = Monsters.GetMonsterFromDB("Larinoth").Result.id,
                }};
                yield return new object[] {"http://mhgen.kiranico.com/hammer/striped-striker", 0, new SwordValues() {
                    sword_name = "Striped Striker 1",
                    raw_dmg = 140,
                    slots = 0,
                    rarity = 3,
                    upgrades_into = "none",
                    price = 4000,
                    element = new List<ElementDamage>(),
                    affinity = -25,
                    monster_id = Monsters.GetMonsterFromDB("Tigrex").Result.id,
                }};
                yield return new object[] {"http://mhgen.kiranico.com/insectglaive/heavens-rath-glaive", 0, new SwordValues() {
                    sword_name = "Heaven's Rath Glaive 1",
                    raw_dmg = 170,
                    slots = 0,
                    rarity = 7,
                    upgrades_into = "none",
                    price = 20000,
                    element = new List<ElementDamage>() {
                        new ElementDamage() {elem_type = "Fire", elem_amount = 22}
                    },
                    affinity = 5,
                    monster_id = -1
                }};
                yield return new object[] {"http://mhgen.kiranico.com/gunlance/red-devil-gunlance", 3, new SwordValues() {
                    sword_name = "Red Devil Gunlance 4",
                    raw_dmg = 170,
                    slots = 0,
                    rarity = 2,
                    upgrades_into = "Blue Devil Uberlance 1",
                    price = 22100,
                    element = new List<ElementDamage>(),
                    affinity = -20,
                    monster_id = Monsters.GetMonsterFromDB("Tetsucabra").Result.id,
                }};
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}