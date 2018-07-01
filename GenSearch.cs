using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using SQLite;
using Gensearch.Scrapers;
using Gensearch.Helpers;

namespace Gensearch
{
    class GenSearch
    {
        public static SQLiteAsyncConnection db = new SQLiteAsyncConnection("data/mhgen.db");
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TimeSpan timeSpan = new TimeSpan();
            
            if (args.Contains("--items") || args.Contains("--all") || args.Length == 0) {
                var itemManager = new Items();
                itemManager.GetItemList().Wait();
                timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                Console.WriteLine("\n\nFinished with items! Took " + timeSpan.ToString("c") + ".\n\n");
                stopwatch.Reset();
                stopwatch.Start();
            }
            
            if (args.Contains("--monsters") || args.Contains("--all") || args.Length == 0) {
                var monManager = new Monsters();
                Console.WriteLine("Starting monster data retrieval.");
                monManager.GetMonsters().Wait();
                timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                Console.WriteLine("Done with monsters! Took " + timeSpan.ToString("c") + ".\n\n");
                stopwatch.Reset();
                stopwatch.Start();
            }
            
            if (args.Contains("--quests") || args.Contains("--all") || args.Length == 0) {
                var questManager = new Quests();
                Console.WriteLine("Starting quest data retrieval.");
                questManager.GetQuests("http://mhgen.kiranico.com/quest/village").Wait();
                questManager.GetQuests("http://mhgen.kiranico.com/quest/guild").Wait();
                questManager.GetQuests("http://mhgen.kiranico.com/quest/arena").Wait();
                questManager.GetQuests("http://mhgen.kiranico.com/quest/training").Wait();
                questManager.GetQuests("http://mhgen.kiranico.com/quest/special-permit").Wait();
                timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                Console.WriteLine("Done with all quests! Took " + timeSpan.ToString("c") + ".\n\n");
                stopwatch.Reset();
                stopwatch.Start();
            }

            if (args.Contains("--weapons") || args.Contains("--all") || args.Length == 0) {
                Stopwatch indiv_weapon_watch = new Stopwatch();
                var weaponManager = new Weapons();
                string[] weaponurls = new string[] {
                    "heavybowgun",
                    "lightbowgun",
                    "bow", 
                    "gunlance", 
                    "chargeblade", 
                    "switchaxe", 
                    "huntinghorn", 
                    "dualblades", 
                    "greatsword", 
                    "longsword", 
                    "swordshield", 
                    "hammer", 
                    "lance", 
                    "insectglaive"
                };
                indiv_weapon_watch.Start();
                foreach (string category in weaponurls) {
                    ConsoleWriters.InfoMessage($"Starting {category} retrieval...");
                    weaponManager.GetWeapons($"http://mhgen.kiranico.com/{category}").Wait();
                    timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(indiv_weapon_watch.Elapsed.TotalSeconds));
                    ConsoleWriters.InfoMessage($"Done with {category}s! Took {timeSpan.ToString("c")}.\n\n");
                    indiv_weapon_watch.Restart();
                }
                indiv_weapon_watch.Stop();
                timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                ConsoleWriters.InfoMessage("Done with all weapons! Took " + timeSpan.ToString("c") + ".\n\n");
                stopwatch.Restart();
            }

            if (args.Contains("--skills") || args.Contains("--all") || args.Length == 0) {
                var skillManager = new Skills();
                ConsoleWriters.InfoMessage("Starting skill retrieval...\n\n");
                skillManager.GetSkills("http://mhgen.kiranico.com/skill").Wait();
                timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                ConsoleWriters.InfoMessage("Done with all skills! Took " + timeSpan.ToString("c") + ".\n\n");
                stopwatch.Restart();
            }

            if (args.Contains("--arts") || args.Contains("--all") || args.Length == 0) {
                var artManager = new HunterArts();
                ConsoleWriters.InfoMessage("Starting hunter art retrieval...\n\n");
                artManager.GetArts("http://mhgen.kiranico.com/hunter-art").Wait();
                timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                ConsoleWriters.InfoMessage("Done with all hunter arts! Took " + timeSpan.ToString("c") + ".\n\n");
                stopwatch.Restart();
            }

            if (args.Contains("--decorations") || args.Contains("--all") || args.Length == 0) {
                var decoManager = new Decorations();
                ConsoleWriters.InfoMessage("Starting decoration retrieval...\n\n");
                decoManager.GetDecorations("http://mhgen.kiranico.com/decoration").Wait();
                timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                ConsoleWriters.InfoMessage("Done with all decorations! Took " + timeSpan.ToString("c") + ".\n\n");
                stopwatch.Restart();
            }

            if (args.Contains("--armor") || args.Contains("--all") || args.Length == 0) {
                var armor = new Armors();
                ConsoleWriters.InfoMessage("Starting armor retrieval...\n\n");
                for (int i = 1; i < 9; i++) {
                    armor.GetArmors($"http://mhgen.kiranico.com/armor?rare={i}").Wait();
                }
                timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                ConsoleWriters.InfoMessage("Done with all armors! Took " + timeSpan.ToString("c") + ".\n\n");
                stopwatch.Restart();
            }
            stopwatch.Stop();
        }
    }
}