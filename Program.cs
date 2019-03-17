﻿using System;

namespace TilemapGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("---------------------------");
            Console.WriteLine("-- Map Generator: v0.0.1 --");
            Console.WriteLine("---------------------------");

            PositionVector size = new PositionVector(64, 32);
            PGCMap.MAX_SIMULATIONS_OF_TILEGEN = 300;
            PGCMap.TILE_DEATH_LIMIT = 3;
            PGCMap.TILE_BIRTH_LIMIT = 2;
            PGCMap.BARRIER_PERCENTAGE = 20;
            PGCMap m = new PGCMap(size);
            m.PrintPGCMap();

            ConsoleKeyInfo cki = Program.Prompt();
            while (cki.Key == ConsoleKey.Enter)
            {
                m = new PGCMap(size);
                m.PrintPGCMap();
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
