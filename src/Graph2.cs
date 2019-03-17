using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Utility Graph class with flexible data types
 */
public class Graph2<DType>
{
    // public delegate T GraphIterator<T>(T val);
    public List<int> nodes;
    public Dictionary<int, List<int>> edges;
    public Dictionary<int, DType> data = new Dictionary<int, DType>();
    public Graph2(List<int> _nodes, Dictionary<int, List<int>> _edges)
    {
        nodes = _nodes;
        edges = _edges;
    }

    // Use BFS to check for reachability between two nodes
    public bool IsReachable(int startId, int endId)
    {
        HashSet<int> visited = new HashSet<int>();
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(startId);
        visited.Add(startId);
        int counter = 0;
        while (queue.Count > 0)
        {
            int n = queue.Dequeue();
            if (n == endId)
            {
                return true;
            }

            var neighbors = GetEdges(n);
            if (neighbors != null)
            {
                foreach (int i in neighbors)
                {
                    if (!visited.Contains(i))
                    {
                        queue.Enqueue(i);
                        visited.Add(i);
                        counter += 1;
                    }
                }
            }
        }
        return false;
    }

    // Retrieves data for node
    public DType GetData(int nodeId)
    {
        if (data.ContainsKey(nodeId))
        {
            return data[nodeId];
        }
        else
        {
            return default(DType);
        }
    }

    public List<int> GetEdges(int nodeId)
    {
        if (edges.ContainsKey(nodeId))
        {
            return edges[nodeId];
        }
        else
        {
            return null;
        }
    }

    // Sets data for node
    public void SetData(int nodeId, DType val)
    {
        data.Add(nodeId, val);
    }

    public string StringifyNode(int n)
    {
        return ("id(" + n + ")<" + GetData(n) + ">: [" + GetEdges(n) + "]");
    }

    // // Iterates over nodes with side effects
    // void ForEach(GraphIterator<DType> gh)
    // {
    //     foreach (int n in nodes)
    //     {
    //         gh(GetData(n));
    //     }
    // }

    public override string ToString()
    {
        string str = "";
        foreach (int n in nodes)
        {
            // Note: DType must have a ToString
            str += StringifyNode(n);
        }
        return str;
    }
}
