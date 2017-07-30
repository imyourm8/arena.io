using System;
using System.Collections.Generic;

using shared.helpers;

using ClipperLib;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;

namespace arena.helpers
{
    class Contour : List<Vector2>
    {
        public Contour()
            : base()
        { }

        public Contour(int capacity)
            : base(capacity)
        { }

        public static implicit operator Contour(Path path)
        {
            Contour contour = new Contour(path.Count);
            foreach (var point in path)
            {
                contour.Add(new Vector2(point.X, point.Y));
            }
            return contour;
        }
    }
}
