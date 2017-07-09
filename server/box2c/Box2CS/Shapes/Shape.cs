using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;

namespace Box2CS
{
	public enum ShapeType
	{
		Unknown = -1,

		Circle,
		Polygon,
		Count,
	};

	[StructLayout(LayoutKind.Sequential)]
	internal class cb2shapeportable
	{
		[MarshalAs(UnmanagedType.I4)]
		public ShapeType m_type;
		public float m_radius;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class cb2circleshapeportable : IFixedSize
	{
		public cb2shapeportable m_shape = new cb2shapeportable();
		public Vec2 m_p;

		int IFixedSize.FixedSize()
		{
			return Marshal.SizeOf(typeof(cb2circleshapeportable));
		}

		void IFixedSize.Lock()
		{
		}

		void IFixedSize.Unlock()
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class cb2polygonshapeportable : IFixedSize
	{
		[MarshalAs(UnmanagedType.Struct)]
		public cb2shapeportable m_shape = new cb2shapeportable();

		public Vec2 m_centroid;
	
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=Box2DSettings.b2_maxPolygonVertices, ArraySubType=UnmanagedType.Struct)]
		public Vec2[] m_vertices = new Vec2[Box2DSettings.b2_maxPolygonVertices];
	
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = Box2DSettings.b2_maxPolygonVertices, ArraySubType = UnmanagedType.Struct)]
		public Vec2[] m_normals = new Vec2[Box2DSettings.b2_maxPolygonVertices];

		public int m_vertexCount;

		int IFixedSize.FixedSize()
		{
			return Marshal.SizeOf(typeof(cb2polygonshapeportable));
		}

		void IFixedSize.Lock()
		{
		}

		void IFixedSize.Unlock()
		{
		}
	}

	public abstract class Shape : ICloneable, ICompare<Shape>
	{
		cb2shapeportable _internalShape;

		internal cb2shapeportable InternalShape
		{
			get { return _internalShape; }
			set { _internalShape = value; }
		}

		public Shape()
		{
			_internalShape = new cb2shapeportable();
		}

		public abstract Shape Clone();

		internal abstract IntPtr Lock();
		internal abstract void Unlock();

		public abstract void ComputeAABB(out AABB aabb, Transform xf);
		public abstract void ComputeMass(out MassData massData, float density);
		public abstract bool TestPoint(Transform xf, Vec2 p);

		object ICloneable.Clone()
		{
			return Clone();
		}

		[Browsable(false)]
		public ShapeType ShapeType
		{
			get { return _internalShape.m_type; }
			internal set { _internalShape.m_type = value; }
		}

		/// <summary>
		/// The radius of this shape.
		/// </summary>
		public float Radius
		{
			get { return _internalShape.m_radius; }
			set { _internalShape.m_radius = value; }
		}

		public bool CompareWith(Shape shape)
		{
			if (shape is PolygonShape && this is PolygonShape)
				return ((PolygonShape)this).CompareWith((PolygonShape)shape);
			else if (shape is CircleShape && this is CircleShape)
				return ((CircleShape)this).CompareWith((CircleShape)shape);

			return false;
		}

		public override string ToString()
		{
			return "ShapeType="+ShapeType.ToString();
		}
	}
}
