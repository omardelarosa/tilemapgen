using System;
using System.Collections.Generic;
public class Node<T> : INode<Node<T>>
{
    public Guid id;
    T data;
    public Node(T d)
    {
        id = Guid.NewGuid();
        data = d;
    }

    public Guid GetGuid()
    {
        return id;
    }

    public T Read()
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