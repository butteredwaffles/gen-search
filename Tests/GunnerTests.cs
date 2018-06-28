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
    public class GunnerTests
    {
        private GunnerWeapons weaponManager = new GunnerWeapons();
        private static IConfiguration config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
        private static IBrowsingContext context = BrowsingContext.New(config);

        /// <summary>
        /// Assures that <c>GunnerWeapons.GetBowShots</c> returns the correct values.
        /// </summary>
        /// <param name="address">The URL of the weapon.</param>
        /// <param name="index">The index of the weapon in its hierachy.</param>
        /// <param name="expected">The value the test is exoected to return.</param>
        [Theory]
        [ClassData(typeof(BowShotsTestData))]
        public async Task BowShotsTest(string address, int index, string[] expected) {
            var page = await context.OpenAsync(address);
            var trs = page.QuerySelector(".table").QuerySelectorAll("tr");

            string[] received = weaponManager.GetBowShots(trs[index]);
            received.ShouldDeepEqual(expected);
        }

        public class BowShotsTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator() {
                yield return new object[] {"http://mhgen.kiranico.com/bow/heat-haze", 0, new string[] {"Arc Shot: Blast", "Pierce Lv1", "Pierce Lv2", "Rapid Lv3", "Heavy Lv 3"}};
                yield return new object[] {"http://mhgen.kiranico.com/bow/usurpers-rumble", 3, new string[] {"Arc Shot: Wide", "Rapid Lv2", "Rapid Lv3", "Pierce Lv4", "Rapid Lv4"}};
                yield return new object[] {"http://mhgen.kiranico.com/bow/alatreon-bow", 2, new string[] {"Arc Shot: Blast", "Spread Lv2", "Pierce Lv4", "Pierce Lv5", "Rapid Lv3"}};
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}