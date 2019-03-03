using System;
using System.Collections.Generic;
public class Node : INode<Vector2Int>
{
    public Guid id;
    Vector2Int data;
    public static Node NULL_NODE = new Node(Vector2Int.NULL_POSITION);

    public Node(Vector2Int d)
    {
        id = Guid.NewGuid();
        data = d;
    }

    public Guid GetGuid()
    {
        return id;
    }

    public Vector2Int Read()
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

    public INode<Vector2Int> GetNullNode()
    {
        return Node.NULL_NODE;
    }

    public bool IsNull()
    {
        return this.id == Node.NULL_NODE.id;
    }
}