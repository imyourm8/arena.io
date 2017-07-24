using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.helpers
{
    class PolygonContours : List<Contour>
    {
        public PolygonContours(int capacity)
            : base(capacity)
        { }

        public PolygonContours()
            : base()
        { }
    }
}
