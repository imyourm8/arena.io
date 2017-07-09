using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;

namespace Box2CS
{
	public sealed class CircleShape : Shape, ICompare<CircleShape>
	{
		cb2circleshapeportable _internalCircleShape;

		internal cb2circleshapeportable InternalCircleShape
		{
			get { return _internalCircleShape; }
		}

		StructToPtrMarshaller _internalstruct;
		internal override IntPtr Lock()
		{
			_internalCircleShape.m_shape = base.InternalShape;
			_internalstruct = new StructToPtrMarshaller(_internalCircleShape);
			return _internalstruct.Pointer;
		}

		internal override void Unlock()
		{
			_internalstruct.Free();
			_internalstruct = null;
		}

		internal CircleShape(cb2circleshapeportable portableShape)
		{
			_internalCircleShape = portableShape;
			InternalShape = portableShape.m_shape;
		}

		public CircleShape(Vec2 position, float radius = 0.0f)
		{
			_internalCircleShape = new cb2circleshapeportable();
			_internalCircleShape.m_shape = base.InternalShape;
			InternalShape.m_type = ShapeType.Circle;
			InternalShape.m_radius = radius;
			_internalCircleShape.m_p = position;
		}

		public CircleShape() :
			this(Vec2.Empty, 0)
		{
		}

		public CircleShape(float radius) :
			this(Vec2.Empty, radius)
		{
		}

		public override Shape Clone()
		{
			CircleShape shape = new CircleShape();
			shape.Position = Position;
			shape.Radius = Radius;
			return shape;
		}

		public override void ComputeAABB(out AABB aabb, Transform xf)
		{
			Vec2 p = xf.Position + (xf.R * Position);
			aabb = new AABB(new Vec2(p.X - Radius, p.Y - Radius),
				new Vec2(p.X + Radius, p.Y + Radius));
		}

		public override void ComputeMass(out MassData massData, float density)
		{
			var mass = density * Math.PI * Radius * Radius;
			massData = new MassData((float)mass,
				Position,
				(float)mass * (0.5f * Radius * Radius + Position.Dot(Position)));
		}

		/// <summary>
		/// Get or set the local center of the circle.
		/// </summary>
		public Vec2 Position
		{
			get { return _internalCircleShape.m_p; }
			set { _internalCircleShape.m_p = value; }
		}

		public override bool TestPoint(Transform xf, Vec2 p)
		{
			Vec2 center = xf.Position + (xf.R * Position);
			Vec2 d = p - center;
			return d.Dot(d) <= Radius * Radius;
		}

		public bool CompareWith(CircleShape shape)
		{
			return (Radius == shape.Radius &&
				Position == shape.Position);
		}

		public override string ToString()
		{
			return base.ToString() + " Radius="+Radius.ToString() + " Position=" + Position.ToString();
		}

		public static CircleShape Parse(string p)
		{
			SimpleParser parser = new SimpleParser(p, true);

			return new CircleShape(Vec2.Parse(parser.ValueFromKey("Position")), float.Parse(parser.ValueFromKey("Radius")));
		}
	}
}
