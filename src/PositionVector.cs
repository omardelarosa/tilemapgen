using UnityEngine;
public class PositionVector
{
    public int x;
    public int y;
    public Vector3Int vec;
    public static PositionVector NULL_POSITION = new PositionVector(-1, -1);
    public string key;
    public PositionVector(int _x, int _y)
    {
        x = _x;
        y = _y;
        key = x + "_" + y;
        vec = new Vector3Int(x, y, 0); // TODO: just make this a vector
    }

    override public string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}