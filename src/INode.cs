using System;
using System.Collections.Generic;
public interface INode<T> where T : Vector2Int
{
    Guid GetGuid();
    Vector2Int Read();
    INode<T> GetNullNode();
    bool IsNull();
}