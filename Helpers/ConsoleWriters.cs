using System;

namespace Gensearch.Helpers
{
    public static class ConsoleWriters
    {
        public static void StartingPageMessage(string name, string address) {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Started work on {name} ({address}).");
            Console.ResetColor();
        }

        public static void InsertionMessage(string message) {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}