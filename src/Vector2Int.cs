
public class Vector2Int
{
    public int x;
    public int y;
    public static Vector2Int NULL_POSITION = new Vector2Int(-1, -1);
    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    override public string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}