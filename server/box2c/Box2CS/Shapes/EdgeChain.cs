using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Box2CS
{
	// Box2CS exclusive
	public class EdgeChain
	{
		List<Vec2> _vecs = new List<Vec2>();

		public IList<Vec2> Vecs
		{
			get { return _vecs; }
		}

		public EdgeChain()
		{
		}

		public EdgeChain(params Vec2[] vecs)
		{
			_vecs = vecs.ToList<Vec2>();
		}

		public Body[] GenerateBodies(World world, Vec2 basePosition, FixtureDef def)
		{
			if (_vecs.Count <= 1)
				throw new ArgumentOutOfRangeException("Vecs");

			Body[] bodies = new Body[_vecs.Count - 1];

			for (int i = 0; i < _vecs.Count - 1; ++i)
			{
				PolygonShape edge = new PolygonShape(_vecs[i], _vecs[i + 1]);

				BodyDef bd = new BodyDef();
				bd.Position = basePosition;

				bodies[i] = world.CreateBody(bd);
				bodies[i].CreateFixture(new FixtureDef(edge, 0, def.Restitution, def.Friction, false, def.UserData));
			}

			return bodies;
		}
	}
}
