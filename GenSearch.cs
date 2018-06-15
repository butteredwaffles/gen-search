using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Gensearch
{
    class GenSearch
    {
        static void Main(string[] args)
        {
            // var itemManager = new Items();
            // itemManager.GetItemList().Wait();
            // Console.WriteLine("\n\nFinished with items!\n\n");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var monManager = new Monsters();

            Console.WriteLine("Starting monster data retrieval.");
            monManager.GetMonsters().Wait();
            stopwatch.Stop();
            TimeSpan timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
            Console.WriteLine("Done with monsters! Took " + timeSpan.ToString("c") + ".");
        }
    }
}
