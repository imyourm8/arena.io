using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using arena.battle.map;
using arena.helpers;

using ClipperLib;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

using Box2CS;

namespace arena.battle
{
    class NavigationLayer
    {
        public int TileWidth
        { get; private set; }

        public int TileHeight
        { get; private set; }

        public Contour OuterBorder
        { get; private set; }

        public PolygonContours Obstacles
        { get; private set; }

        public World World
        { get; private set; }

        public void Load(JToken data)
        {
            var properties = data.SelectToken(TiledParser.PropertiesToken); 
            TileHeight = properties.SelectToken("HashHeight").Value<int>();
            TileWidth = properties.SelectToken("HashWidth").Value<int>();

            Obstacles = new PolygonContours(20);

            var objs = data.SelectToken(TiledParser.ObjectToken);
            foreach (var obj in objs)
            {
                Contour contour = TiledParser.ParseObject(obj);
                JToken nameToken = obj.SelectToken("name");
                if (nameToken != null && nameToken.Value<string>() == "OuterBorder")
                {
                    OuterBorder = contour;
                }
                else
                {
                    Obstacles.Add(contour);
                }
            }

            World = new World(Vec2.Empty, true);
            CreateWallsAroundWorld();
        }

        private void CreateWallsAroundWorld()
        {
            var bodyDef = new BodyDef();
            var body = World.CreateBody(bodyDef);

            var edgeShape = new PolygonShape(); 
            var edge = new FixtureDef(edgeShape);
            edge.Density = 0.0f; //solid body
            edge.Filter.CategoryBits = (ushort)PhysicsDefs.Category.WALLS;
            edge.Filter.MaskBits =
                (ushort)PhysicsDefs.Category.EXP_BLOCK | (ushort)PhysicsDefs.Category.PLAYER | (ushort)PhysicsDefs.Category.MOB;

            int count = OuterBorder.Count;
            for (int i = 0; i < count; ++i)
            {
                edgeShape.Vertices = new Vec2[] { 
                    OuterBorder[i],
                    OuterBorder[(i+1)%count]
                };

                body.CreateFixture(edge);
            }
        }
    }
}
