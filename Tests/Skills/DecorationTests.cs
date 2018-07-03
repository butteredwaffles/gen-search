using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using DeepEqual.Syntax;
using Gensearch.Scrapers;
using SQLite;
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

        [Theory]
        [ClassData(typeof(CombinationTestData))]
        public async Task DecorationCombinationTest(int index, List<DecorationCombination> expected) {
            var page = await context.OpenAsync("http://mhgen.kiranico.com/decoration");
            var trs = page.QuerySelector(".table").QuerySelectorAll("tr");

            List<DecorationCombination> actual = await decoManager.GetDecorationCombinations(trs[index], 0);
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

        public class CombinationTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator() {
                yield return new object[] {0, new List<DecorationCombination>() {
                    new DecorationCombination() {
                        deco_id = 0,
                        item_1_id = Items.GetItemFromDB("Aquaglow Jewel").Result.id,
                        item_1_quantity = 1,
                        item_2_id = Items.GetItemFromDB("Ioprey Fang").Result.id,
                        item_2_quantity = 2,
                        item_3_id = -1,
                        item_3_quantity = 0
                    }
                }};
                yield return new object[] {1, new List<DecorationCombination>() {
                    new DecorationCombination() {
                        deco_id = 0,
                        item_1_id = Items.GetItemFromDB("Sunspire Jewel").Result.id,
                        item_1_quantity = 1,
                        item_2_id = Items.GetItemFromDB("Screamer Sac").Result.id,
                        item_2_quantity = 2,
                        item_3_id = Items.GetItemFromDB("Iodrome Hide").Result.id,
                        item_3_quantity = 2
                    },
                    new DecorationCombination() {
                        deco_id = 0,
                        item_1_id = Items.GetItemFromDB("Bloodrun Jewel").Result.id,
                        item_1_quantity = 1,
                        item_2_id = Items.GetItemFromDB("Toxin Sac").Result.id,
                        item_2_quantity = 1,
                        item_3_id = -1,
                        item_3_quantity = 0
                    }
                }};
                yield return new object[] {90, new List<DecorationCombination>() {
                    new DecorationCombination() {
                        deco_id = 0,
                        item_1_id = Items.GetItemFromDB("Sunspire Jewel").Result.id,
                        item_1_quantity = 1,
                        item_2_id = Items.GetItemFromDB("Blue Cutthroat").Result.id,
                        item_2_quantity = 2,
                        item_3_id = Items.GetItemFromDB("Monster Bone+").Result.id,
                        item_3_quantity = 2
                    },
                    new DecorationCombination() {
                        deco_id = 0,
                        item_1_id = Items.GetItemFromDB("Bloodrun Jewel").Result.id,
                        item_1_quantity = 1,
                        item_2_id = Items.GetItemFromDB("Avian Finebone").Result.id,
                        item_2_quantity = 1,
                        item_3_id = Items.GetItemFromDB("Stoutbone").Result.id,
                        item_3_quantity = 1
                    }
                }};
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}