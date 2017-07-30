using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.helpers;
using arena.helpers;
using arena.battle.map;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ClipperLib;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

using LibTessDotNet;

namespace arena.battle
{
    class ExpLayer
    {
        private List<ExpArea> areas_;

        public ExpLayer()
        {}

        public int MaxBlocks
        { get; set; }

        public int TileWidth
        { get; set; }

        public int TileHeight
        { get; set; }

        public Area Area
        { get; set; }

        public void Load(JToken data)
        { 
            var properties = data.SelectToken(TiledParser.PropertiesToken);
            TileHeight = properties.SelectToken("Height").Value<int>();
            TileWidth = properties.SelectToken("Width").Value<int>();
            MaxBlocks = properties.SelectToken("MaxBlocks").Value<int>();

            List<ExpArea> expAreas = new List<ExpArea>(10);
            Dictionary<ExpArea, Path> paths = new Dictionary<ExpArea, Path>();
            foreach (var obj in data.SelectToken(TiledParser.ObjectToken))
            {
                var expArea = new ExpArea(); 
                paths.Add(expArea, TiledParser.ParseObject(obj));
                expArea.Priority = obj.SelectToken("name").Value<int>();

                var probabilities = new Dictionary<proto_game.ExpBlocks, float>();
                properties = obj.SelectToken(TiledParser.PropertiesToken);
                foreach (JProperty prop in properties)
                {
                    probabilities.Add(
                        Parsing.ParseEnum<proto_game.ExpBlocks>(prop.Name),
                        prop.Value.Value<float>()
                    );
                }

                expArea.Probabilities = probabilities;
                expAreas.Add(expArea);
            }

            expAreas.Sort((ExpArea e1, ExpArea e2)=>
            {
                return e1.Priority.CompareTo(e2.Priority);
            });

            Dictionary<ExpArea, Paths> solutions = new Dictionary<ExpArea,Paths>();
            Clipper clipper = new Clipper();
            int count = expAreas.Count;
            /* For every polygon set, try to cut holes using layers with higher priority (higher layer index)
             * 
             * Polygon with priority 1 will be intersected with polygons 2 and 3
             * Polygon with priority 2 will be insterscted with polygon 3 
             * Polygon with priority 3 will remain untouched
             * 
             * Priority index
             * 1     2      3
             * |
             * |     |
             * | <-- |  <-- |
             * |     |
             * |
             * 
             * As a result
             * 1     2      3
             * |     
             *       |
             *             |
             *       |
             * |
             * */
            for (int i = 0; i < count-1; ++i)
            {
                clipper.AddPath(paths[expAreas[i]], PolyType.ptSubject, true);
                for (int j = i + 1; j < count; j++)
                {
                    clipper.AddPath(paths[expAreas[j]], PolyType.ptClip, true);
                }

                Paths solution = new Paths();
                clipper.Execute(ClipType.ctDifference, solution, PolyFillType.pftEvenOdd);
                solutions.Add(expAreas[i], solution);
                clipper.Reset();
            }

            areas_ = expAreas; 

            // triangulate areas to generate random point inside
            int sizeOfTesselation = 3; // split polygon on triangles (3 points)
            Tess tess = new Tess();
            tess.UsePooling = true;
            foreach (var solution in solutions) // for every solution
            {
                var solutionPaths = solution.Value;
                var polygonContours = new PolygonContours(solutionPaths.Count);
                foreach (var path in solutionPaths) // go around every polygon inside
                {
                    var contour = new ContourVertex[path.Count];
                    var polygonContour = new Contour(path.Count);
                    for(int i = 0; i < path.Count; ++i)
                    {
                        IntPoint point = path[i];
                        contour[i].Position = new Vec3{X=point.X, Y=point.Y, Z=0};
                        polygonContour.Add(new Vector2(point.X, point.Y));
                    }
                    tess.AddContour(contour, ContourOrientation.Clockwise); // add this polygon as a contour
                }
                tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, sizeOfTesselation); // triangulate
                var area = solution.Key;
                area.Area = new TriangulatedPolygon(tess.ElementCount); // convert it to game polygon format
                for (int i = 0; i < tess.ElementCount; ++i)
                {
                    var triangle = new Triangle();
                    for (int j = 0; j < sizeOfTesselation; ++j)
                    {
                        int index = tess.Elements[i * sizeOfTesselation + j];
                        if (index == -1)
                            continue;
                        Vec3 trianglePoint = tess.Vertices[index].Position;
                        triangle.Add(new Vector2(trianglePoint.X, trianglePoint.Y), j); 
                    }
                    area.Area.Add(triangle);
                }
            }
        }
    }
}
