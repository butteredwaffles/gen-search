using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using DeepEqual.Syntax;
using Gensearch.Scrapers;
using Xunit;

namespace Gensearch.Tests
{
    public class WeaponsTests
    {
        private Weapons weaponManager = new Weapons();

        [Fact]
        public async Task SharpnessTest() {
            var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
            var context = BrowsingContext.New(config);
            var page = await context.OpenAsync("http://mhgen.kiranico.com/greatsword/sentoryo-calamity");

            List<SharpnessValue> sharpness_data = weaponManager.GetSharpness(page, page.QuerySelector(".table").QuerySelector("tr"), 0);
            SharpnessValue sharpness_value = new SharpnessValue() {
                weapon_id = 0,
                red_sharpness_length = 80,
                orange_sharpness_length = 40,
                yellow_sharpness_length = 110,
                green_sharpness_length = 100,
                blue_sharpness_length = 20,
                white_sharpness_length = 0
            };
            sharpness_value.ShouldDeepEqual(sharpness_data[0]);
        }
    }
}