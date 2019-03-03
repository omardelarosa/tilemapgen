using System;

namespace TilemapGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("---------------------------");
            Console.WriteLine("-- Map Generator: v0.0.1 --");
            Console.WriteLine("---------------------------");
            ConsoleKeyInfo cki = Program.Prompt();
            while (cki.Key == ConsoleKey.Enter)
            {
                Vector2Int size = new Vector2Int(16, 16);
                Matrix m = new Matrix(size);
                m.PrintMatrix();
                // m.PrintGraphs();
                cki = Program.Prompt();
            }
        }

        static ConsoleKeyInfo Prompt()
        {
            ConsoleKeyInfo cki = Console.ReadKey();
            // Keep the console window open in debug mode.
            Console.WriteLine("Press ENTER to generate a map.  Anything else to quit."); ;
            return cki;
        }
    }
}
