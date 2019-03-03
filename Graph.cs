using System;
using System.Collections.Generic;

public class Graph<T>
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
