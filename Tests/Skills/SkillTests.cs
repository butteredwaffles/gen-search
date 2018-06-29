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
    public class SkillTests
    {
        private Skills skillManager = new Skills();
        private static IConfiguration config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
        private static IBrowsingContext context = BrowsingContext.New(config);

        [Theory]
        [ClassData(typeof(SkillTestData))]
        public async Task SkillTest(int index, string skill_tree, Skill expected) {
            var page = await context.OpenAsync("http://mhgen.kiranico.com/skill");
            var trs = page.QuerySelector(".table").QuerySelectorAll("tr");

            Skill received = skillManager.GetSkill(trs[index], skill_tree);
            received.ShouldDeepEqual(expected);
        }

        public class SkillTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator() {
                yield return new object[] {2, "Poison", new Skill() {
                    skill_name = "Double Poison",
                    skill_tree = "Poison",
                    skill_value = -10,
                    skill_description = "Doubles the damage received from Poison."
                }};
                yield return new object[] {11, "Stun", new Skill() {
                    skill_name = "Halve Stun",
                    skill_tree = "Stun",
                    skill_value = 10,
                    skill_description = "Reduces the likelihood of being Stunned by 50%."
                }};
                yield return new object[] {14, "Hearing", new Skill() {
                    skill_name = "HG Earplugs",
                    skill_tree = "Hearing",
                    skill_value = 15,
                    skill_description = "Negates the effects of all large monsters' roars."
                }};
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}