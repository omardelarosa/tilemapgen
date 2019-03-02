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
            Vector2Int size = new Vector2Int(16, 16);
            Matrix m = new Matrix(size);
            m.PrintMatrix();

            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
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
    }

    class Matrix
    {
        public const string PADDING = " ";
        List<List<Tile>> tilematrix;
        private Vector2Int size;
        public Matrix(Vector2Int size)
        {
            BuildEmptyMatrix(size);
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
                }
                tilematrix.Add(row);
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

        public string ToString()
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