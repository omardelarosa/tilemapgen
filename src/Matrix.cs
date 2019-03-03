using System;
using System.Collections.Generic;

public class Matrix
{
    public const string PADDING = " ";
    public const int BARRIER_PERCENTAGE = 20;
    public int rebuilds = 0;
    public const int MAX_REBUILDS = 5;

    List<List<Tile>> tilematrix;
    List<List<Node>> nodematrix;
    List<Vector2Int> positions;
    List<Vector2Int> borderPositions;
    List<Vector2Int> exitPositions;
    PathData<Node> pathData;
    private Vector2Int size;
    private int numDoors;
    private int _doorsPlaced = 0;
    private Node nullNode;
    private Vector2Int nullPos;
    Dictionary<Tile, Graph<Node>> graphs;
    Random r;
    public Matrix(Vector2Int size)
    {
        OnInit(size);
        rebuilds = 0;
        while (exitPositions.Count < 2 && rebuilds <= MAX_REBUILDS)
        {
            OnInit(size);
            rebuilds += 1;
        }

        if (rebuilds == MAX_REBUILDS)
        {
            Console.WriteLine("Unable to build valid map after " + MAX_REBUILDS + " rebuilds.");
        }

        bool hasPath = BuildPath();

        if (hasPath)
        {
            Console.WriteLine("Has a path!");
            Vector2Int target = exitPositions[1];
            var n = NGet(target.x, target.y);
            var nid = n.GetGuid();
            var path = pathData.GetPathGuidsFrom(nid);
            foreach (Guid nidOnPath in path)
            {
                var nodeInPath = graphs[Tile.Space].Find(nidOnPath);
                var pos = nodeInPath.Read();
                MSet(pos.x, pos.y, Tile.Path);
            }
        }
        else
        {
            Console.WriteLine("No path found!");
        }
    }

    bool BuildPath()
    {
        if (exitPositions.Count < 2)
        {
            return false;
        }
        Vector2Int source = exitPositions[0];
        Vector2Int target = exitPositions[1];
        var graph = graphs[Tile.Space];
        var sourceNode = NGet(source.x, source.y);
        var targetNode = NGet(target.x, target.y);
        var pd = graph.Dijkstra(sourceNode, targetNode);
        bool hasPath = pd.HasPathTo(targetNode.GetGuid());
        pathData = pd;
        if (hasPath)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void HighlightPath()
    {

    }

    void Reset()
    {
        r = new Random();
        // Initialize containers, static values
        positions = new List<Vector2Int>();
        borderPositions = new List<Vector2Int>();
        exitPositions = new List<Vector2Int>();
        _doorsPlaced = 0;
    }

    void OnInit(Vector2Int size)
    {
        r = new Random();
        // Initialize containers, static values
        Reset();
        numDoors = r.Next(2, 3); // ensures 2 doors. // or 1 to 4 doors
        nullPos = Vector2Int.NULL_POSITION;
        nullNode = Node.NULL_NODE;
        // Populate Matrix
        BuildEmptyMatrix(size);
        BuildEmptyNodeMatrix(size);
        FillMatrix();
        graphs = MakeGraphs(); // Make graph of tile relationships
        SetExitPositions();
        Console.WriteLine("Dims: " + tilematrix.Count + "rows x " + tilematrix[0].Count + "cols");
        Console.WriteLine("Exits: " + exitPositions.Count);
    }

    public Tile MGet(int x, int y)
    {
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
        {
            return Tile.OutOfBoundsTile;
        }
        else
        {
            return tilematrix[y][x];
        }
    }

    public bool MSet(int x, int y, Tile t)
    {
        bool isValidTile = MGet(x, y) != Tile.OutOfBoundsTile;
        if (isValidTile)
        {
            tilematrix[y][x] = t;
            return true;
        }
        else
        {
            return false;
        }
    }

    public Node NGet(int x, int y)
    {
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
        {
            return nullNode;
        }
        else
        {
            return nodematrix[y][x];
        }
    }

    public bool NSet(int x, int y, Node n)
    {
        bool isValidTile = MGet(x, y) != Tile.NullTile;
        if (isValidTile)
        {
            nodematrix[y][x] = n;
            return true;
        }
        else
        {
            return false;
        }
    }

    void SetExitPositions()
    {
        // No doors.
        if (!graphs.ContainsKey(Tile.Door))
        {
            exitPositions = new List<Vector2Int>();
            return;
        }
        // Determine paths
        List<Node> doors = graphs[Tile.Door].nodesList;
        exitPositions = new List<Vector2Int>();
        foreach (Node d in doors)
        {
            Node adjacentNodeToDoor = GetDoorNeighborSpaces(d);

            if (adjacentNodeToDoor != null)
            {
                Vector2Int nPos = adjacentNodeToDoor.Read();
                MSet(nPos.x, nPos.y, Tile.Path); // Mark as path
                exitPositions.Add(nPos);
            }
            else
            {
                Console.WriteLine("Blocked door at: " + d.Read());
            }
        }
    }

    void BuildEmptyNodeMatrix(Vector2Int s)
    {
        int x = s.x;
        int y = s.y;
        size = s;
        nodematrix = new List<List<Node>>();
        // Rows
        for (int i = 0; i < y; i++)
        {
            List<Node> row = new List<Node>();
            // Cols
            for (int j = 0; j < x; j++)
            {
                row.Add(nullNode);
            }
            nodematrix.Add(row);
        }
    }
    void BuildEmptyMatrix(Vector2Int s)
    {
        int x = s.x;
        int y = s.y;
        size = s;
        tilematrix = new List<List<Tile>>();
        // Rows
        for (int i = 0; i < y; i++)
        {
            List<Tile> row = new List<Tile>();
            // Cols
            for (int j = 0; j < x; j++)
            {
                row.Add(Tile.NullTile);
                Vector2Int pos = new Vector2Int(j, i);
                positions.Add(pos);

                // Use this to add doors
                if (IsBorder(pos))
                {
                    borderPositions.Add(pos);
                }
            }
            tilematrix.Add(row);
        }
    }
    public Dictionary<Guid, List<Guid>> BuildAdjacencyList(List<Node> nodes, Tile t)
    {
        var edges = new Dictionary<Guid, List<Guid>>();
        foreach (Node n in nodes)
        {
            if (n != null)
            {
                var neighbors = GetNeighborGuids(n, t);
                edges.Add(n.GetGuid(), neighbors);
            }
        }
        return edges;
    }

    Node GetDoorNeighborSpaces(Node door)
    {
        var neighbors = GetNeighborGuids(door, Tile.Space);
        if (neighbors.Count == 0)
        {
            // Blocked in door
            return null;
        }
        else
        {
            Guid nid = neighbors[0]; // get first neighbor
            var node = graphs[Tile.Space].Find(nid);
            return node;
        }
    }

    List<Guid> GetNeighborGuids(Node node, Tile t)
    {
        List<Guid> neighbors = new List<Guid>();
        List<Vector2Int> neighborPositions = new List<Vector2Int>();
        Vector2Int pos = node.Read();
        // Check above
        neighborPositions.Add(new Vector2Int(pos.x - 1, pos.y)); // left
        neighborPositions.Add(new Vector2Int(pos.x + 1, pos.y)); // right
        neighborPositions.Add(new Vector2Int(pos.x, pos.y - 1)); // up
        neighborPositions.Add(new Vector2Int(pos.x, pos.y + 1)); // down

        foreach (Vector2Int nPos in neighborPositions)
        {
            // Check if neighbor is of same tiletype
            if (MGet(nPos.x, nPos.y) == t)
            {
                var n = NGet(nPos.x, nPos.y);
                if (n != null)
                {
                    neighbors.Add(n.GetGuid());
                }
                else
                {
                    Console.WriteLine("NO NODE AT POSITION: " + nPos);
                }
                // Adds GUID for node to neighbors

            }
        }
        return neighbors;
    }

    public Dictionary<Tile, Graph<Node>> MakeGraphs()
    {
        IDictionary<Tile, List<Node>> nodes = new Dictionary<Tile, List<Node>>();
        IDictionary<Tile, Dictionary<Guid, List<Guid>>> edges = new Dictionary<Tile, Dictionary<Guid, List<Guid>>>();

        // Making NullTile
        nodes.Add(Tile.NullTile, new List<Node>());
        // Making DoorTile
        nodes.Add(Tile.Door, new List<Node>());

        foreach (Vector2Int p in positions)
        {
            Tile t = MGet(p.x, p.y);
            Node n = new Node(p);
            if (!nodes.ContainsKey(t))
            {
                nodes.Add(t, new List<Node>()); // create nodelist
            }
            var list = nodes[t];
            if (list == null)
            {
                nodes[t] = new List<Node>();
                list = nodes[t];
            }
            NSet(p.x, p.y, n); // Adds node to node matrix
            list.Add(n);
        }

        Dictionary<Tile, Graph<Node>> graphs = new Dictionary<Tile, Graph<Node>>();

        foreach (Tile t in nodes.Keys)
        {
            var nList = nodes[t];
            if (t != Tile.NullTile)
            {
                var eDict = BuildAdjacencyList(nList, t);
                edges.Add(t, eDict);
                Graph<Node> g = new Graph<Node>(nList, eDict);
                graphs.Add(t, g);
            }
            else
            {
                if (nList != null && nList.Count > 0)
                {
                    Console.WriteLine("Warning: " + nList.Count + " null tiles exist!");
                }
            }

        }

        return graphs;
        // TODO: make graph for all parts.
    }

    void FillMatrix()
    {
        // Adds barriers
        foreach (Vector2Int pos in positions)
        {
            FillAtPosition(pos);
        }

        // Adds doors to border positions
        AddDoors();
    }

    // TODO: generalize rules based on position better?
    void FillAtPosition(Vector2Int pos)
    {
        if (IsBorder(pos))
        {
            MSet(pos.x, pos.y, Tile.Barrier);
        }
        else if (IsRandomBarrier())
        {
            MSet(pos.x, pos.y, Tile.Barrier);
        }
        else
        {
            MSet(pos.x, pos.y, Tile.Space);
        }
    }

    void AddDoors()
    {
        int numBorderPositions = borderPositions.Count;

        while (_doorsPlaced < numDoors)
        {
            int rIdx = r.Next(0, numBorderPositions);
            Vector2Int randomPos = borderPositions[rIdx];
            Tile t = MGet(randomPos.x, randomPos.y);
            if (t != Tile.Door)
            {
                MSet(randomPos.x, randomPos.y, Tile.Door);
                _doorsPlaced += 1;
            }
        }

        // TODO: take out debug logic
        Console.WriteLine("Doors: " + numDoors + " DoorsPlace: " + _doorsPlaced);
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
        int rInt = r.Next(0, 101);
        if (rInt <= BARRIER_PERCENTAGE)
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
            case Tile.Door:
                return "D";
            case Tile.Path:
                return ".";
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
        for (int i = 0; i < y; i++)
        {
            string line = "";
            for (int j = 0; j < x; j++)
            {
                line = line + TileToString(MGet(j, i)) + PADDING;
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
            Graph<Node> g = m.graphs[t];
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