using System;
using System.Collections;
using System.Collections.Generic;

namespace TilemapGen
{
    enum Tile
    {
        Barrier,
        Space,
        NullTile
    }

    class CLI
    {
        static void Main()
        {
            Console.WriteLine("MAP GENERATOR");

            ConsoleKeyInfo cki = CLI.Prompt();
            while (cki.Key == ConsoleKey.Enter)
            {
                Vector2Int size = new Vector2Int(16, 16);
                Matrix m = new Matrix(size);
                m.PrintMatrix();
                cki = CLI.Prompt();
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

    // TODO: use a library for vectors
    class Vector2Int
    {
        public int x;
        public int y;
        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        override public string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }

    class Matrix
    {
        public const string PADDING = " ";
        List<List<Tile>> tilematrix;
        List<Vector2Int> positions;
        private Vector2Int size;
        Random r;
        public Matrix(Vector2Int size)
        {
            r = new Random();
            positions = new List<Vector2Int>();
            BuildEmptyMatrix(size);
            FillMatrix();
        }

        void BuildEmptyMatrix(Vector2Int s)
        {
            int x = s.x;
            int y = s.y;
            size = s;
            tilematrix = new List<List<Tile>>();
            for (int i = 0; i < x; i++)
            {
                List<Tile> row = new List<Tile>();
                for (int j = 0; j < y; j++)
                {
                    row.Add(Tile.NullTile);
                    Vector2Int pos = new Vector2Int(i, j);
                    positions.Add(pos);
                }
                tilematrix.Add(row);
            }
        }

        void FillMatrix()
        {
            foreach (Vector2Int pos in positions)
            {
                FillAtPosition(pos);
            }
        }

        // TODO: generalize rules based on position better?
        void FillAtPosition(Vector2Int pos)
        {
            if (IsBorder(pos) || IsRandomBarrier())
            {
                tilematrix[pos.x][pos.y] = Tile.Barrier;
            }
            else
            {
                tilematrix[pos.x][pos.y] = Tile.Space;
            }
        }

        bool IsBorder(Vector2Int pos)
        {
            if (pos.x == 0 || pos.x == size.x - 1 || pos.y == 0 || pos.y == size.y - 1)
            {
                return true;
            }
            return false;
        }

        bool IsRandomBarrier()
        {
            int rInt = r.Next(0, 10);
            if (rInt < 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        string TileToString(Tile t)
        {
            switch (t)
            {
                case Tile.Barrier:
                    return "X";
                case Tile.Space:
                    return " ";
                case Tile.NullTile:
                    return "N";
                default:
                    return "?";
            }
        }

        public override string ToString()
        {
            int x = size.x;
            int y = size.y;

            string str = "";
            for (int i = 0; i < x; i++)
            {
                string line = "";
                for (int j = 0; j < y; j++)
                {
                    line = line + TileToString(tilematrix[i][j]) + PADDING;
                }
                str = str + line + "\n";
            }
            return str;
        }

        public static void PrintMatrix(Matrix m)
        {
            Console.WriteLine(m.ToString());
        }

        public void PrintMatrix()
        {
            Matrix.PrintMatrix(this);
        }
    }
}