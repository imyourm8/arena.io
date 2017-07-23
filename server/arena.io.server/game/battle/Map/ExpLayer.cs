using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        {
        }

        public int MaxBlocks
        { get; set; }

        public int TileWidth
        { get; set; }

        public int TileHeight
        { get; set; }

        public helpers.Area Area
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
                        helpers.Parsing.ParseEnum<proto_game.ExpBlocks>(prop.Name),
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
            foreach (var solution in solutions)
            {
                Tess tess = new Tess();
                var solutionPaths = solution.Value;
                foreach (var path in solutionPaths)
                { 
                    
                }
            }
        }
    }
}
