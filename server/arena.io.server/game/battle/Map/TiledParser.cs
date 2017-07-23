using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClipperLib;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace arena.battle.map
{
    static class TiledParser
    {
        static public readonly string ObjectToken = "objects";
        static public readonly string PropertiesToken = "properties";

        static public Path ParseObject(JToken obj)
        {
            int x = (int)Math.Round(obj.SelectToken("x").Value<float>());
            int y = (int)Math.Round(obj.SelectToken("y").Value<float>());

            Path path = null;
            JToken polygon = obj.SelectToken("polygon");
            if (polygon != null)
            {
                string[] pointTokens = polygon.SelectToken("points").Value<string>().Split(' ');
                path = new Path(pointTokens.Length);
                foreach (string pointToken in pointTokens)
                {
                    float point = float.Parse(pointToken);
                    path.Add(new IntPoint(point + x, point + y));
                }
            }
            else
            {
                int w = (int)Math.Round(obj.SelectToken("width").Value<float>());
                int h = (int)Math.Round(obj.SelectToken("height").Value<float>());

                path = new Path(4);
                path.Add(new IntPoint(x, y));
                path.Add(new IntPoint(x + w, y));
                path.Add(new IntPoint(x + w, y + h));
                path.Add(new IntPoint(x, y + h));
            }
            return path;
        }
    }
}
