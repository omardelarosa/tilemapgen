using System;
using System.Collections.Generic;

public class Graph<T> where T : INode<PositionVector>
{
    public List<T> nodesList;
    public Dictionary<Guid, T> nodesMap;
    public Dictionary<Guid, List<Guid>> edges;

    // TODO: memoize dijkstra
    // public Dictionary<Guid, Dictionary<Guid, int>> distBySource; // keep track of dist
    // public Dictionary<Guid, Dictionary<Guid, Guid>> prevBySource; // keep track of paths for a given source

    public Graph(List<T> nodes, Dictionary<Guid, List<Guid>> _edges)
    {
        // Null graph
        if (nodes == null)
        {
            nodesList = new List<T>();
            edges = _edges;
        }
        else
        {
            nodesList = nodes;
            edges = _edges;
            nodesMap = new Dictionary<Guid, T>();
            foreach (T n in nodesList)
            {
                if (n != null)
                {
                    nodesMap.Add(n.GetGuid(), n);
                }
            }
        }

    }

    public int Length(Guid u, Guid v)
    {
        T n1 = Find(u);
        T n2 = Find(v);
        if (n1.IsNull() || n2.IsNull())
        {
            // TODO: error?
            return int.MaxValue;
        }
        else
        {
            PositionVector d1 = n1.Read();
            PositionVector d2 = n2.Read();
            var dist = Math.Sqrt((d1.y - d2.y) / (float)(d1.x - d2.x));
            // TODO: make this distance different based on position
            return (int)Math.Round(dist);
        }

    }

    public PathData<T> Dijkstra(T source, T target)
    {
        PathData<T> pd = new PathData<T>();
        HashSet<T> set = new HashSet<T>();

        // Initialize all routes
        foreach (T n in nodesList)
        {
            Guid id = n.GetGuid();
            pd.distances.Add(id, int.MaxValue);
            // pd.paths[id] = Node.NULL_GUID;
            set.Add(n);
        }

        // Set start to min
        pd.distances[source.GetGuid()] = 0;

        while (set.Count > 0)
        {
            Guid uid = pd.GetGuidOfMinInSet(set);
            set.Remove(Find(uid));

            // No edges containing uid case
            if (!edges.ContainsKey(uid))
            {
                break;
            }
            List<Guid> neighbors = edges[uid];

            // No neighbors case.
            if (neighbors.Count == 0)
            {
                break;
            }

            foreach (Guid nid in neighbors)
            {
                T n = Find(nid);
                if (n.IsNull())
                {
                    throw new InvalidOperationException("Node with id: " + nid + " not found during path data construction.");
                }
                Guid vid = n.GetGuid();
                int alt = pd.distances[uid] + Length(vid, uid);
                if (alt < pd.distances[vid])
                {
                    pd.distances[vid] = alt;
                    pd.paths.Add(vid, uid);
                }
            }
        }

        return pd;
    }
    public T Find(Guid id)
    {
        if (nodesMap.ContainsKey(id))
        {
            return nodesMap[id];
        }
        else
        {
            return default(T);
        }
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
