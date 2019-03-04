
public class PositionVector
{
    public int x;
    public int y;
    public static PositionVector NULL_POSITION = new PositionVector(-1, -1);
    public PositionVector(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    override public string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}