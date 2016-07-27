using System.Collections.Generic;

public class MapNavigation
{
    public struct Point
    {
        public float x, y;
        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Line
    {
        public Point begin, end;
        public Line(float x, float y, float x2, float y2)
        {
            begin = new Point(x, y);
            end = new Point(x2, y2);
        }
    }

    private List<Line> lines_ = new List<Line>();
    public MapNavigation()
    { }

    public void Add(float x, float y, float x2, float y2)
    {
        lines_.Add(new Line(x, y, x2, y2));
    }
}
