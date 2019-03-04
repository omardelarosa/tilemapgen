using System;
using System.Collections.Generic;
public interface INode<T> where T : PositionVector
{
    Guid GetGuid();
    PositionVector Read();
    INode<T> GetNullNode();
    bool IsNull();
}