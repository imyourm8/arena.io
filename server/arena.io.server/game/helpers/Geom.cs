using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.helpers
{
    struct Area
    {
        public float minX, maxX, minY, maxY;
        public Area(float minX_, float maxX_, float minY_, float maxY_)
        {
            minX = minX_;
            maxX = maxX_;
            minY = minY_;
            maxY = maxY_;
        }
        public float Width
        { get { return maxX - minX; } }

        public float Height
        { get { return maxY - minY; } }

        public bool IsInside(float x, float y)
        {
            return minX <= x && x <= maxX && minY <= y && y <= maxY;
        }

        public void RandomPoint(out float x, out float y)
        {
            x = minX + helpers.MathHelper.Range(0, Width);
            y = minY + helpers.MathHelper.Range(0, Height);
        }
    }
}
