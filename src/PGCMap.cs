using System;
using System.Collections.Generic;

public class PGCMap
{
    public const string PADDING = " ";
    public const int BARRIER_PERCENTAGE = 30;
    public int rebuilds = 0;
    public const int MAX_REBUILDS = 5;

    List<List<ETile>> ETilePGCMap;
    List<List<Node>> nodePGCMap;
    List<PositionVector> positions;
    List<PositionVector> borderPositions;
    List<PositionVector> exitPositions;
    PathData<Node> pathData;
    private PositionVector size;
    private int numDoors;
    private int _doorsPlaced = 0;
    private Node nullNode;
    private PositionVector nullPos;
    Dictionary<ETile, Graph<Node>> graphs;
    Random r;
    public PGCMap(PositionVector size)
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
            PositionVector target = exitPositions[1];
            var n = NGet(target.x, target.y);
            var nid = n.GetGuid();
            var path = pathData.GetPathGuidsFrom(nid);
            foreach (Guid nidOnPath in path)
            {
                var nodeInPath = graphs[ETile.Space].Find(nidOnPath);
                var pos = nodeInPath.Read();
                MSet(pos.x, pos.y, ETile.Path);
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
        PositionVector source = exitPositions[0];
        PositionVector target = exitPositions[1];
        var graph = graphs[ETile.Space];
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
        positions = new List<PositionVector>();
        borderPositions = new List<PositionVector>();
        exitPositions = new List<PositionVector>();
        _doorsPlaced = 0;
    }

    void OnInit(PositionVector size)
    {
        r = new Random();
        // Initialize containers, static values
        Reset();
        numDoors = r.Next(2, 3); // ensures 2 doors. // or 1 to 4 doors
        nullPos = PositionVector.NULL_POSITION;
        nullNode = Node.NULL_NODE;
        // Populate PGCMap
        BuildEmptyPGCMap(size);
        BuildEmptyNodePGCMap(size);
        FillPGCMap();
        graphs = MakeGraphs(); // Make graph of ETile relationships
        SetExitPositions();
        Console.WriteLine("Dims: " + ETilePGCMap.Count + "rows x " + ETilePGCMap[0].Count + "cols");
        Console.WriteLine("Exits: " + exitPositions.Count);
    }

    public ETile MGet(int x, int y)
    {
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
        {
            return ETile.OutOfBoundsETile;
        }
        else
        {
            return ETilePGCMap[y][x];
        }
    }

    public bool MSet(int x, int y, ETile t)
    {
        bool isValidETile = MGet(x, y) != ETile.OutOfBoundsETile;
        if (isValidETile)
        {
            ETilePGCMap[y][x] = t;
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
            return nodePGCMap[y][x];
        }
    }

    public bool NSet(int x, int y, Node n)
    {
        bool isValidETile = MGet(x, y) != ETile.NullETile;
        if (isValidETile)
        {
            nodePGCMap[y][x] = n;
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
        if (!graphs.ContainsKey(ETile.Door))
        {
            exitPositions = new List<PositionVector>();
            return;
        }
        // Determine paths
        List<Node> doors = graphs[ETile.Door].nodesList;
        exitPositions = new List<PositionVector>();
        foreach (Node d in doors)
        {
            Node adjacentNodeToDoor = GetDoorNeighborSpaces(d);

            if (adjacentNodeToDoor != null)
            {
                PositionVector nPos = adjacentNodeToDoor.Read();
                MSet(nPos.x, nPos.y, ETile.Path); // Mark as path
                exitPositions.Add(nPos);
            }
            else
            {
                Console.WriteLine("Blocked door at: " + d.Read());
            }
        }
    }

    void BuildEmptyNodePGCMap(PositionVector s)
    {
        int x = s.x;
        int y = s.y;
        size = s;
        nodePGCMap = new List<List<Node>>();
        // Rows
        for (int i = 0; i < y; i++)
        {
            List<Node> row = new List<Node>();
            // Cols
            for (int j = 0; j < x; j++)
            {
                row.Add(nullNode);
            }
            nodePGCMap.Add(row);
        }
    }
    void BuildEmptyPGCMap(PositionVector s)
    {
        int x = s.x;
        int y = s.y;
        size = s;
        ETilePGCMap = new List<List<ETile>>();
        // Rows
        for (int i = 0; i < y; i++)
        {
            List<ETile> row = new List<ETile>();
            // Cols
            for (int j = 0; j < x; j++)
            {
                row.Add(ETile.NullETile);
                PositionVector pos = new PositionVector(j, i);
                positions.Add(pos);

                // Use this to add doors
                if (IsBorder(pos))
                {
                    borderPositions.Add(pos);
                }
            }
            ETilePGCMap.Add(row);
        }
    }
    public Dictionary<Guid, List<Guid>> BuildAdjacencyList(List<Node> nodes, ETile t)
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
        var neighbors = GetNeighborGuids(door, ETile.Space);
        if (neighbors.Count == 0)
        {
            // Blocked in door
            return null;
        }
        else
        {
            Guid nid = neighbors[0]; // get first neighbor
            var node = graphs[ETile.Space].Find(nid);
            return node;
        }
    }

    List<Guid> GetNeighborGuids(Node node, ETile t)
    {
        List<Guid> neighbors = new List<Guid>();
        List<PositionVector> neighborPositions = new List<PositionVector>();
        PositionVector pos = node.Read();
        // Check above
        neighborPositions.Add(new PositionVector(pos.x - 1, pos.y)); // left
        neighborPositions.Add(new PositionVector(pos.x + 1, pos.y)); // right
        neighborPositions.Add(new PositionVector(pos.x, pos.y - 1)); // up
        neighborPositions.Add(new PositionVector(pos.x, pos.y + 1)); // down

        foreach (PositionVector nPos in neighborPositions)
        {
            // Check if neighbor is of same ETiletype
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

    public Dictionary<ETile, Graph<Node>> MakeGraphs()
    {
        IDictionary<ETile, List<Node>> nodes = new Dictionary<ETile, List<Node>>();
        IDictionary<ETile, Dictionary<Guid, List<Guid>>> edges = new Dictionary<ETile, Dictionary<Guid, List<Guid>>>();

        // Making NullETile
        nodes.Add(ETile.NullETile, new List<Node>());
        // Making DoorETile
        nodes.Add(ETile.Door, new List<Node>());

        foreach (PositionVector p in positions)
        {
            ETile t = MGet(p.x, p.y);
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
            NSet(p.x, p.y, n); // Adds node to node PGCMap
            list.Add(n);
        }

        Dictionary<ETile, Graph<Node>> graphs = new Dictionary<ETile, Graph<Node>>();

        foreach (ETile t in nodes.Keys)
        {
            var nList = nodes[t];
            if (t != ETile.NullETile)
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
                    Console.WriteLine("Warning: " + nList.Count + " null ETiles exist!");
                }
            }

        }

        return graphs;
        // TODO: make graph for all parts.
    }

    void FillPGCMap()
    {
        // Adds barriers
        foreach (PositionVector pos in positions)
        {
            FillAtPosition(pos);
        }

        // Adds doors to border positions
        AddDoors();
    }

    // TODO: generalize rules based on position better?
    void FillAtPosition(PositionVector pos)
    {
        if (IsBorder(pos))
        {
            MSet(pos.x, pos.y, ETile.Wall);
        }
        else if (IsRandomBarrier())
        {
            MSet(pos.x, pos.y, ETile.Barrier);
        }
        else
        {
            MSet(pos.x, pos.y, ETile.Space);
        }
    }

    void AddDoors()
    {
        int numBorderPositions = borderPositions.Count;

        while (_doorsPlaced < numDoors)
        {
            int rIdx = r.Next(0, numBorderPositions);
            PositionVector randomPos = borderPositions[rIdx];
            ETile t = MGet(randomPos.x, randomPos.y);
            if (t != ETile.Door)
            {
                MSet(randomPos.x, randomPos.y, ETile.Door);
                _doorsPlaced += 1;
            }
        }

        // TODO: take out debug logic
        Console.WriteLine("Doors: " + numDoors + " DoorsPlace: " + _doorsPlaced);
    }

    bool IsBorder(PositionVector pos)
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

    string ETileToString(ETile t)
    {
        switch (t)
        {
            case ETile.Wall:
                return "W";
            case ETile.Barrier:
                return "B";
            case ETile.Space:
                return " ";
            case ETile.Door:
                return "D";
            case ETile.Path:
                return ".";
            case ETile.NullETile:
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
                line = line + ETileToString(MGet(j, i)) + PADDING;
            }
            str = str + line + "\n";
        }
        return str;
    }

    public static void PrintPGCMap(PGCMap m)
    {
        Console.WriteLine(m.ToString());
    }
    public void PrintPGCMap()
    {
        PGCMap.PrintPGCMap(this);
    }

    public static void PrintGraphs(PGCMap m)
    {
        foreach (ETile t in m.graphs.Keys)
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
        PGCMap.PrintGraphs(this);
    }


}