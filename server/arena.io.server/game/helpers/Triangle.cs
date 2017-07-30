using System;
using System.Collections.Generic;
using System.Diagnostics;

using shared.helpers;

namespace arena.helpers
{
    class Triangle
    {
        private Vector2[] points = new Vector2[3];
        // Summary:
        //     Add 2d point using index
        //     index have to be in [0..2] range
        public void Add(Vector2 point, int index)
        {
            points[index] = point;
        }

        public Vector2 GetRandomPointInside(Random rand)
        {
            // thanks to this post https://stackoverflow.com/a/19654424/1223007
            double r1 = rand.NextDouble();
            double r2 = rand.NextDouble();
            double x = (1 - Math.Sqrt(r1)) * points[0].x + (Math.Sqrt(r1) * (1 - r2)) * points[1].x + (Math.Sqrt(r1) * r2) * points[2].x;
            double y = (1 - Math.Sqrt(r1)) * points[0].y + (Math.Sqrt(r1) * (1 - r2)) * points[1].y + (Math.Sqrt(r1) * r2) * points[2].y;
            return new Vector2(x, y);
        }
    }
}
