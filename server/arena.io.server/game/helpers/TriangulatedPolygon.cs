using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.helpers;

namespace arena.helpers
{
    class TriangulatedPolygon : List<Triangle>
    {
        public TriangulatedPolygon(int capacity)
            : base(capacity)
        { }

        public TriangulatedPolygon()
            : base()
        { }

        public Vector2 GetRandomPoint(Random rand)
        {
            // select random triangle first
            int triangleIndex = rand.Next(Count);
            return this[triangleIndex].GetRandomPointInside(rand);
        }
    }
}
