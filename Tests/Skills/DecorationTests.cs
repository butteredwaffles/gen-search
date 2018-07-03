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
    public class DecorationTests
    {
        private Decorations decoManager = new Decorations();
        private static IConfiguration config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
        private static IBrowsingContext context = BrowsingContext.New(config);

        [Theory]
        [ClassData(typeof(DecorationTestData))]
        public async Task DecorationTest(int index, Decoration expected) {
            var page = await context.OpenAsync("http://mhgen.kiranico.com/decoration");
            var trs = page.QuerySelector(".table").QuerySelectorAll("tr");

            Decoration actual = await decoManager.GetDecoration(trs[index]);
            actual.ShouldDeepEqual(expected);
        }

        public class DecorationTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator() {
                yield return new object[] {0, new Decoration() {
                    deco_name = "Antidote Jwl 1",
                    deco_slot_requirement = 1,
                    positive_skill_tree = "Poison",
                    positive_skill_effect = 1,
                    negative_skill_tree = "Stun",
                    negative_skill_effect = -1 
                }};
                yield return new object[] {36, new Decoration() {
                    deco_name = "Nul-Water Jwl 1",
                    deco_slot_requirement = 1,
                    positive_skill_tree = "Water Res",
                    positive_skill_effect = 2,
                    negative_skill_tree = "none",
                    negative_skill_effect = 0
                }};
                yield return new object[] {111, new Decoration() {
                    deco_name = "Capacity Jwl 3",
                    deco_slot_requirement = 3,
                    positive_skill_tree = "Loading",
                    positive_skill_effect = 2,
                    negative_skill_tree = "Reload Spd",
                    negative_skill_effect = -2
                }};
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}