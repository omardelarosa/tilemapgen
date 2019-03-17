using System;
using System.Collections.Generic;
using UnityEngine;

public class PGCMap
{
    public const string PADDING = " ";
    public static int TILE_DEATH_LIMIT = 2;
    public static int TILE_BIRTH_LIMIT = 3;
    public static int MAX_SIMULATIONS_OF_TILEGEN = 20;
    public static int BARRIER_PERCENTAGE = 50;
    public int rebuilds = 0;
    public const int MAX_REBUILDS = 5;

    public List<List<ETile>> tilemap;
    public bool hasPath;
    public List<PositionVector> positions;
    public List<PositionVector> borderPositions;
    public List<Vector2Int> exitPositions;
    public List<Vector2Int> doorPositions;
    public List<Vector2Int> validEntityPositions;
    private PositionVector size;
    private List<Vector2Int> _barriersMemo = new List<Vector2Int>();
    private int numDoors;
    private int _doorsPlaced = 0;

    // TileAutomata Values
    private int tileDeathLimit;
    private int tileBirthLimit;
    private int maxIterations;
    private int barrierFrequency;
    private System.Random random;

    private PositionVector nullPos;
    private int[,] nidsByPos;
    Dictionary<ETile, Graph2<Vector3Int>> graphsByTileType;
    Dictionary<string, List<Vector2Int>> _neighborsMemo;
    bool hasInitializedRNG = false;
    int BARRIER_TILE = 1;
    int SPACE_TILE = 0;

    public PGCMap(PositionVector size)
    {
        // Simulation parameters
        tileDeathLimit = TILE_DEATH_LIMIT;
        tileBirthLimit = TILE_BIRTH_LIMIT;
        maxIterations = MAX_SIMULATIONS_OF_TILEGEN;
        barrierFrequency = BARRIER_PERCENTAGE;

        OnInit(size);
    }

    Vector2Int GeneratePathFrom(Vector2Int pos, int counter, int max, int minRadius)
    {
        // Go in random direction until a wall is encountered, place a door when wall is found.

        // No movement by default
        Vector2Int nextPos = pos;
        // Base case:  Stop looping
        if (counter >= max)
        {
            Console.WriteLine("Counter " + counter);
            Console.WriteLine("Maximum path drawn! Ensure this is not an error.");
            return pos;
        }
        var wallNeighbors = GetNeighborPositionsOfType(pos, ETile.Wall);
        // Case 1: Reached wall
        if (counter > minRadius && wallNeighbors.Count > 0)
        {
            nextPos = wallNeighbors[0];
            counter = maxIterations; // Terminate
        }
        // Case 2: no walls
        else
        {
            var neighboringBarriers = GetNeighborPositionsOfType(pos, ETile.Barrier);
            // Case 2A: No barriers nearby, keep going
            if (neighboringBarriers.Count == 0)
            {
                // Get space neighbors, go in random direction, recurse
                var neighbors = GetNeighborPositions(pos);
                int randIdx = GetRandInt(0, neighbors.Count);
                nextPos = neighbors[randIdx];
            }
            else
            {
                // Remove barrier, set its former position to loop
                int randIdx = GetRandInt(0, neighboringBarriers.Count);
                nextPos = neighboringBarriers[randIdx];
                MSet(nextPos.x, nextPos.y, ETile.Space, tilemap);
            }
        }

        // Draw Path
        MSet(pos.x, pos.y, ETile.Path, tilemap);
        // Recurse
        return GeneratePathFrom(nextPos, counter + 1, max, minRadius);
    }

    void Reset()
    {
        GetRandInt(0, 10); // Initializes RNG state
        // Initialize containers, static values
        positions = new List<PositionVector>();
        borderPositions = new List<PositionVector>();
        exitPositions = new List<Vector2Int>();
        doorPositions = new List<Vector2Int>();
        validEntityPositions = new List<Vector2Int>();
        _doorsPlaced = 0;
    }

    public int GetRandInt(int low, int high)
    {
        if (!hasInitializedRNG)
        {
            random = new System.Random();
            // int seed = (int)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            // Console.WriteLine("Random Seed: " + seed);
            // UnityEngine.Random.InitState(seed); // TODO: make this seed configurable
            hasInitializedRNG = true;
        }
        return random.Next(low, high);
    }

    void OnInit(PositionVector size)
    {
        // Initialize containers, static values
        Reset();
        numDoors = GetRandInt(2, 3); // ensures 2 doors. // or 1 to 4 doors
        nullPos = PositionVector.NULL_POSITION;
        // Populate PGCMap
        tilemap = BuildEmptyPGCMap(size);
        FillPGCMap();
        SetExitPositions();

        // Build Graphs of TileTypes
        graphsByTileType = BuildAllGraphs();
    }

    Dictionary<ETile, Graph2<Vector3Int>> BuildAllGraphs()
    {
        // Build Graphs of TileTypes
        Dictionary<ETile, Graph2<Vector3Int>> _graphsByTileType = new Dictionary<ETile, Graph2<Vector3Int>>();

        foreach (ETile et in Enum.GetValues(typeof(ETile)))
        {
            var graph = MakeGraphFromPositionsListForTileType(positions, et);
            _graphsByTileType.Add(et, graph);
        }

        return _graphsByTileType;
    }

    public ETile MGet(int x, int y, List<List<ETile>> _tilemap)
    {
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
        {
            return ETile.OutOfBoundsETile;
        }
        else
        {
            return _tilemap[y][x];
        }
    }

    public bool MSet(int x, int y, ETile t, List<List<ETile>> _tilemap)
    {
        bool isValidETile = MGet(x, y, _tilemap) != ETile.OutOfBoundsETile;
        if (isValidETile)
        {
            _tilemap[y][x] = t;
            return true;
        }
        else
        {
            return false;
        }
    }

    void SetExitPositions()
    {
        // Determine paths
        exitPositions = new List<Vector2Int>();
        foreach (var p in doorPositions)
        {
            List<Vector2Int> doorNeighbors = GetNeighborPositionsOfType(p, ETile.Path);
            if (doorNeighbors.Count > 0)
            {
                Vector2Int nPos = doorNeighbors[0];
                MSet(nPos.x, nPos.y, ETile.Space, tilemap); // Mark as path
                exitPositions.Add(nPos);
                validEntityPositions.Remove(nPos);
            }
            else
            {
                Console.WriteLine("Blocked door at: " + p);
            }
        }
    }
    List<List<ETile>> BuildEmptyPGCMap(PositionVector s)
    {
        int x = s.x;
        int y = s.y;
        size = s;
        var tilemap = new List<List<ETile>>();
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
            tilemap.Add(row);
        }
        return tilemap;
    }
    List<Vector2Int> GetDoorNeighborSpaces(Vector2Int doorPos)
    {
        var neighbors = GetNeighborPositions(doorPos);
        List<Vector2Int> spaceNeighbors = new List<Vector2Int>();
        foreach (var n in neighbors)
        {
            ETile nt = MGet(n.x, n.y, tilemap);
            if (nt == ETile.Space)
            {
                spaceNeighbors.Add(n);
            }
        }
        return spaceNeighbors;
    }
    List<Vector2Int> GetNeighborPositions(Vector2Int pos)
    {
        // TODO: get rid of this interface
        return GetNeighborPositions(new PositionVector(pos.x, pos.y));
    }

    List<Vector2Int> GetNeighborPositions(PositionVector pos)
    {
        // Create memoization container
        if (_neighborsMemo == null)
        {
            _neighborsMemo = new Dictionary<string, List<Vector2Int>>();
        }
        if (_neighborsMemo.ContainsKey(pos.key))
        {
            return _neighborsMemo[pos.key];
        }

        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1); // Makes a box.
        List<Vector2Int> neighborPositions = new List<Vector2Int>();
        // Check all adjacent positions
        foreach (var bi in bounds.allPositionsWithin)
        {
            // Exclude self
            if (bi.x != 0 && bi.y != 0)
            {
                int xp = pos.x + bi.x;
                int yp = pos.y + bi.y;
                // Exclude outer neighbors
                if (xp >= 0 && xp < size.x && yp >= 0 && yp < size.y)
                {
                    neighborPositions.Add(new Vector2Int(pos.x + bi.x, pos.y + bi.y));
                }
            }
        }
        // Memoize neighbors to avoid list creation
        _neighborsMemo.Add(pos.key, neighborPositions);

        return neighborPositions;
    }

    List<Vector2Int> GetNeighborPositionsOfType(Vector2Int pos, ETile et)
    {
        var neighbors = GetNeighborPositions(pos);
        // No neighbors
        if (neighbors == null)
        {
            return null;
        }

        // Filter neighbors
        List<Vector2Int> filteredNeighbors = new List<Vector2Int>();
        foreach (var n in neighbors)
        {
            ETile tile = MGet(n.x, n.y, tilemap);
            if (tile == et)
            {
                filteredNeighbors.Add(n);
            }
        }

        return filteredNeighbors;
    }

    public Graph2<Vector3Int> MakeGraphFromPositionsListForTileType(List<PositionVector> _positions, ETile tileType)
    {
        List<int> nodes = new List<int>();
        Dictionary<int, List<int>> edges = new Dictionary<int, List<int>>();
        int ids = 0;
        Graph2<Vector3Int> _graph = new Graph2<Vector3Int>(nodes, edges);

        // This only needs to be made once
        nidsByPos = new int[size.x, size.y];

        // Make nodes
        foreach (PositionVector p in _positions)
        {
            nodes.Add(ids); // Adds node to graph
            _graph.SetData(ids, p.vec); // Adds vector data to node
            nidsByPos[p.x, p.y] = ids;
            ids += 1;
        }

        // Make edges based on tile type
        foreach (PositionVector p in _positions)
        {
            var neighbors = GetNeighborPositions(p); // TODO: support neighbor filtering by type
            int pid = nidsByPos[p.x, p.y];
            List<int> nids = new List<int>();
            foreach (var n in neighbors)
            {
                int nid = 0;
                try
                {
                    nid = nidsByPos[n.x, n.y];
                }
                catch (System.Exception ex)
                {
                    // Detects invalid positions
                    Console.WriteLine("INVALID POS: " + n);
                    throw ex;
                }

                ETile tileAtNode = MGet(n.x, n.y, tilemap);
                if (tileAtNode == tileType)
                {
                    nids.Add(nid);
                }
            }
            edges.Add(pid, nids);
        }

        return _graph;
    }

    int[,] MakeRandomTilemapFromPositions(List<PositionVector> _positions)
    {
        int[,] _tilemap = new int[size.x, size.y];
        foreach (PositionVector pos in _positions)
        {
            if (GetRandInt(0, 101) <= barrierFrequency)
            {
                // Set to barrier tile
                _tilemap[pos.x, pos.y] = BARRIER_TILE;
            }
            else
            {
                // Set to spaces
                _tilemap[pos.x, pos.y] = SPACE_TILE;
            }
        }
        return _tilemap;
    }

    int[,] MakeEmptyTilemapFromPositions(List<PositionVector> _positions)
    {
        int[,] _tilemap = new int[size.x, size.y];
        foreach (PositionVector pos in _positions)
        {
            // Set to spaces
            _tilemap[pos.x, pos.y] = SPACE_TILE;
        }
        return _tilemap;
    }

    int[,] CloneTilemapFromPositions(int[,] tilemap, List<PositionVector> _positions)
    {
        int[,] _tilemap = new int[size.x, size.y];
        foreach (PositionVector pos in _positions)
        {
            // Set to spaces
            _tilemap[pos.x, pos.y] = tilemap[pos.x, pos.y];
        }
        return _tilemap;
    }

    void CopyTilemapFromPositions(int[,] sourceTilemap, int[,] targetTilemap, List<PositionVector> _positions)
    {
        foreach (PositionVector pos in _positions)
        {
            // Transfer
            targetTilemap[pos.x, pos.y] = sourceTilemap[pos.x, pos.y];
        }
    }

    void ClearTilemapFromPositions(int[,] _tilemap, List<PositionVector> _positions)
    {
        foreach (PositionVector pos in _positions)
        {
            // Set to spaces
            _tilemap[pos.x, pos.y] = SPACE_TILE;
        }
    }

    // Runs a Conway's Game of Life-style simulation for tile PGC
    int[,] RunSimulation(int[,] _lastTilemap)
    {
        // Fill Spaces based on Simple Tile Automata
        int BARRIER_TILE = 1;
        int SPACE_TILE = 0;
        int[,] lastTilemap = CloneTilemapFromPositions(_lastTilemap, positions);
        int[,] currentTilemap = MakeEmptyTilemapFromPositions(positions);
        var totalBarriers = 0;

        for (int i = 0; i < maxIterations; i++)
        {
            if (i > 0)
            {
                // Swaps tiles between both maps, preventing tile map creation at each step
                CopyTilemapFromPositions(currentTilemap, lastTilemap, positions);
                ClearTilemapFromPositions(currentTilemap, positions);
            }

            // Adds barriers
            foreach (PositionVector pos in positions)
            {
                var neighbors = GetNeighborPositions(pos);
                int lastTile = lastTilemap[pos.x, pos.y];
                int nextTile = lastTile;
                // Count living neighbors
                int numLivingNeighbors = 0;

                foreach (var n in neighbors)
                {
                    // Ignore out of bounds neighbors
                    if (n.x < 0 || n.y < 0 || n.x >= size.x || n.y >= size.y)
                    {
                        // Treat out of bounds as a barrier
                        numLivingNeighbors = BARRIER_TILE;
                        numLivingNeighbors += 1;
                        continue;
                    }
                    int neighborTile = lastTilemap[n.x, n.y];
                    if (neighborTile == 1)
                    {
                        numLivingNeighbors += 1;
                    }
                }

                // Case 1
                if (lastTile == BARRIER_TILE)
                {
                    // Should be dead on next map, too many neighbors
                    if (numLivingNeighbors < tileDeathLimit)
                    {
                        nextTile = SPACE_TILE;
                    }
                    else
                    {
                        nextTile = BARRIER_TILE;
                    }
                }

                // Case 2:
                if (lastTile == SPACE_TILE)
                {
                    // Should be born on next map because too few neighbors
                    if (numLivingNeighbors < tileBirthLimit)
                    {
                        nextTile = BARRIER_TILE;
                    }
                    else
                    {
                        nextTile = SPACE_TILE;
                    }
                }

                // Keep track of population
                if (nextTile == BARRIER_TILE)
                {
                    totalBarriers += 1;
                }
                else
                {
                    totalBarriers -= 1;
                }
                currentTilemap[pos.x, pos.y] = nextTile;
            }
        }

        return currentTilemap;
    }
    void FillPGCMap()
    {
        int[,] randomTiles = MakeRandomTilemapFromPositions(positions);
        int[,] spaces = RunSimulation(randomTiles);
        // Populate map based on simulation results
        foreach (var pos in positions)
        {
            int spaceVal = spaces[pos.x, pos.y];
            if (spaceVal == 0)
            {
                MSet(pos.x, pos.y, ETile.Space, tilemap);
            }
            else
            {
                MSet(pos.x, pos.y, ETile.Barrier, tilemap);
                _barriersMemo.Add(new Vector2Int(pos.x, pos.y));
            }
        }
        // Reset tilemap var using new tilemap

        // Adds doors to border positions
        AddWalls(tilemap);
        AddDoors(tilemap);
    }

    // TODO: generalize rules based on position better?
    void FillAtPosition(PositionVector pos, List<List<ETile>> tilemap)
    {
        // Count neighbors

        if (IsBorder(pos))
        {
            MSet(pos.x, pos.y, ETile.Wall, tilemap);
        }
        else if (IsRandomBarrier(pos))
        {
            MSet(pos.x, pos.y, ETile.Barrier, tilemap);
        }
        else
        {
            // TODO: put this somewhere else
            validEntityPositions.Add(new Vector2Int(pos.x, pos.y));
            MSet(pos.x, pos.y, ETile.Space, tilemap);
        }
    }

    void AddDoors(List<List<ETile>> tilemap)
    {
        int numBorderPositions = borderPositions.Count;

        int randBorderPosIdx = GetRandInt(0, numBorderPositions);

        PositionVector startDoorPos = borderPositions[randBorderPosIdx];

        // Set start position to door tile
        MSet(startDoorPos.x, startDoorPos.y, ETile.Door, tilemap);
        int maxPath = size.x * size.y;
        Console.WriteLine("Generate path of max size: " + maxPath);
        Vector2Int startDoorPosVector = new Vector2Int(startDoorPos.x, startDoorPos.y);
        Vector2Int exitDoor = GeneratePathFrom(startDoorPosVector, 0, maxPath, 10);

        // Set exit position to door tile
        MSet(exitDoor.x, exitDoor.y, ETile.Door, tilemap);

        if (Vector2Int.Equals(startDoorPos, exitDoor))
        {
            throw new System.Exception("No path made!");
        }
        else
        {
            doorPositions.Add(startDoorPosVector);
            doorPositions.Add(exitDoor);
            Console.WriteLine("Doors added: " + startDoorPosVector + ", " + exitDoor);
        }
    }

    void AddWalls(List<List<ETile>> tilemap)
    {
        Console.WriteLine("Adding walls " + borderPositions.Count);
        foreach (var pos in borderPositions)
        {
            if (IsBorder(pos))
            {
                MSet(pos.x, pos.y, ETile.Wall, tilemap);
            }
        }
    }

    bool IsBorder(PositionVector pos)
    {
        if (pos.x == 0 || pos.x == size.x - 1 || pos.y == 0 || pos.y == size.y - 1)
        {
            return true;
        }
        return false;
    }

    bool IsRandomBarrier(PositionVector pos)
    {
        // TODO: enable a safe mode?
        // // Avoids edges to make levels not impossible. // TODO: make this unnecessary
        // if (pos.x == 1 || pos.x == size.x - 2 || pos.y == 1 || pos.y == size.y - 2)
        // {
        //     return false;
        // }

        int rInt = GetRandInt(0, 101);
        if (rInt <= barrierFrequency)
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
    public string StringifyTilemap(List<List<ETile>> _tilemap)
    {
        int x = size.x;
        int y = size.y;

        string str = "";
        for (int i = 0; i < y; i++)
        {
            string line = "";
            for (int j = 0; j < x; j++)
            {
                line = line + ETileToString(MGet(j, i, _tilemap)) + PADDING;
            }
            str = str + line + "\n";
        }
        return str;
    }

    public string StringifyTilemap(int[,] _tilemap)
    {
        int x = size.x;
        int y = size.y;

        string str = "";
        for (int i = 0; i < y; i++)
        {
            string line = "";
            for (int j = 0; j < x; j++)
            {
                line = line + _tilemap[j, i];
            }
            str = str + line + "\n";
        }
        return str;
    }

    public override string ToString()
    {
        return StringifyTilemap(tilemap);
    }

    public static void PrintPGCMap(PGCMap m)
    {
        Console.WriteLine(m.ToString());
    }
    public void PrintPGCMap()
    {
        PGCMap.PrintPGCMap(this);
    }
}