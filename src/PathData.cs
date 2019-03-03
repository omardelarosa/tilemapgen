using System;
using System.Collections.Generic;

public class PathData<T> where T : INode<Vector2Int>
{
    public Dictionary<Guid, int> distances;
    public Dictionary<Guid, Guid> paths;
    public PathData()
    {
        distances = new Dictionary<Guid, int>();
        paths = new Dictionary<Guid, Guid>();
    }

    // TODO: use priority queue or minheap instead
    public Guid GetGuidOfMinInSet(HashSet<T> s)
    {
        int minVal = int.MaxValue;
        Guid minKey = new Guid();
        foreach (T n in s)
        {
            Guid k = n.GetGuid();
            int val = distances[k];
            if (val < minVal)
            {
                minVal = val;
                minKey = k;
            }
        }
        return minKey;
    }

    public bool HasPathTo(Guid nid)
    {
        return distances[nid] < int.MaxValue;
    }
}
