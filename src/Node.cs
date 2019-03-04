using System;
using System.Collections.Generic;
public class Node : INode<PositionVector>
{
    public Guid id;
    PositionVector data;
    public static Node NULL_NODE = new Node(PositionVector.NULL_POSITION);

    public Node(PositionVector d)
    {
        id = Guid.NewGuid();
        data = d;
    }

    public Guid GetGuid()
    {
        return id;
    }

    public PositionVector Read()
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

    public INode<PositionVector> GetNullNode()
    {
        return Node.NULL_NODE;
    }

    public bool IsNull()
    {
        return this.id == Node.NULL_NODE.id;
    }
}