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
            int x = (int)Math.Round(obj["x"].Value<float>());
            int y = (int)Math.Round(obj["y"].Value<float>());

            Path path = new Path(15);
            JToken polygon = obj.SelectToken("polygon");
            if (polygon != null)
            {
                foreach (JToken pointToken in polygon)
                {
                    float pointX = pointToken["x"].Value<float>();
                    float pointY = pointToken["y"].Value<float>();
                    path.Add(new IntPoint(pointX + x, pointY + y));
                }
            }
            else
            {
                int w = (int)Math.Round(obj["width"].Value<float>());
                int h = (int)Math.Round(obj["height"].Value<float>());

                path.Add(new IntPoint(x, y));
                path.Add(new IntPoint(x + w, y));
                path.Add(new IntPoint(x + w, y + h));
                path.Add(new IntPoint(x, y + h));
            }
            return path;
        }
    }
}
