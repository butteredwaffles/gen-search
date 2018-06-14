using System;
using System.Threading;

namespace Gensearch
{
    class GenSearch
    {
        static void Main(string[] args)
        {
            var itemManager = new Items();
            // itemManager.GetItemList().Wait();
            // Console.WriteLine("Finished!");
            var monManager = new Monsters();
            monManager.GetMonsters().Wait();
            Console.WriteLine("Done!");
        }
    }
}
