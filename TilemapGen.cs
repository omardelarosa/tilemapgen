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

    class Node<T>
    {
        public Guid id;
        T data;
        public Node(T d)
        {
            id = Guid.NewGuid();
            data = d;
        }

        Guid GetGuid()
        {
            return id;
        }

        T Read()
        {
            return data;
        }

        public override string ToString()
        {
            if (data != null)
            {
                return "<Node id:" + id + " data:" + data.ToString() + ">";
            }
            return "<Node id:" + id + " data:NULL>";
        }
    }

    class Graph<T>
    {
        public List<T> nodesList;
        public Dictionary<Guid, T> nodesMap;
        public Dictionary<Guid, List<Guid>> edges;
        public Graph(List<T> nodes, Dictionary<Guid, List<Guid>> _edges)
        {
            nodesList = nodes;
            edges = _edges;
            // nodesMap = new Dictionary<Guid, T>();
            // foreach (T n in nodesList)
            // {
            //     if (n != null)
            //     {
            //         nodesMap.Add(n.GetGuid(), n);
            //     }

            // }
        }

        public void PrintNodes()
        {
            foreach (T n in nodesList)
            {
                Console.WriteLine(n);
            }
        }

        public override string ToString()
        {
            return nodesMap.ToString();
        }
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
                // m.PrintGraphs();

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
        Dictionary<Tile, Graph<Node<Vector2Int>>> graphs;
        Random r;
        public Matrix(Vector2Int size)
        {
            r = new Random();
            positions = new List<Vector2Int>();
            BuildEmptyMatrix(size);
            graphs = MakeGraphs();
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

        public Dictionary<Tile, Graph<Node<Vector2Int>>> MakeGraphs()
        {
            IDictionary<Tile, List<Node<Vector2Int>>> nodes = new Dictionary<Tile, List<Node<Vector2Int>>>();

            // Making NullTile
            nodes.Add(Tile.NullTile, null);

            foreach (Vector2Int p in positions)
            {
                Tile t = tilematrix[p.x][p.y];
                Node<Vector2Int> n = new Node<Vector2Int>(p);
                var list = nodes[t];
                if (list == null)
                {
                    nodes[t] = new List<Node<Vector2Int>>();
                    list = nodes[t];
                }
                list.Add(n);
            }

            Dictionary<Tile, Graph<Node<Vector2Int>>> graphs = new Dictionary<Tile, Graph<Node<Vector2Int>>>();

            // Making null tile
            // graphs[Tile.NullTile] = null;

            foreach (Tile t in nodes.Keys)
            {
                var edges = new Dictionary<Guid, List<Guid>>();
                Graph<Node<Vector2Int>> g = new Graph<Node<Vector2Int>>(nodes[t], edges);
                graphs.Add(t, g);
            }

            return graphs;
            // TODO: make graph for all parts.
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

        public static void PrintGraphs(Matrix m)
        {
            foreach (Tile t in m.graphs.Keys)
            {
                Graph<Node<Vector2Int>> g = m.graphs[t];
                if (g != null)
                {
                    g.PrintNodes();
                }
            }
        }

        public void PrintGraphs()
        {
            Matrix.PrintGraphs(this);
        }


    }
}