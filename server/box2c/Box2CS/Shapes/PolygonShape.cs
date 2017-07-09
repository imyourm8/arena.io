using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Box2CS
{
	public sealed class PolygonShape : Shape, ICompare<PolygonShape>
	{
		cb2polygonshapeportable _internalPolyShape;

		internal cb2polygonshapeportable InternalPolyShape
		{
			get { return _internalPolyShape; }
		}

		StructToPtrMarshaller _internalstruct;
		internal override IntPtr Lock()
		{
			_internalPolyShape.m_shape = base.InternalShape;
			_internalstruct = new StructToPtrMarshaller(_internalPolyShape);
			return _internalstruct.Pointer;
		}

		internal override void Unlock()
		{
			_internalstruct.Free();
			_internalstruct = null;
		}

		public PolygonShape()
		{
			_internalPolyShape = new cb2polygonshapeportable();
			InternalShape.m_radius = Box2DSettings.b2_polygonRadius;
			_internalPolyShape.m_shape = base.InternalShape;
			InternalShape.m_type = ShapeType.Polygon;
		}

		bool _autoReverse;

		[Browsable(false)]
		public bool AutoReverse
		{
			get { return _autoReverse; }
			set { _autoReverse = value; }
		}

		public PolygonShape(Vec2[] vertices, Vec2 centroid, bool autoReverse = false) :
			this()
		{
			_autoReverse = autoReverse;
			if (vertices.Length == 2)
				SetAsEdge(vertices[0], vertices[1]);
			else
				Vertices = vertices;
			Centroid = centroid;
		}

		public PolygonShape(params Vec2[] vertices) :
			this(false, vertices)
		{
		}

		public PolygonShape(bool autoReverse, params Vec2[] vertices) :
			this(vertices, Vec2.Empty, autoReverse)
		{
		}

		public PolygonShape(float width, float height, Vec2 centroid, float angle = 0.0f) :
			this()
		{
			SetAsBox(width, height, centroid, angle);
		}

		public PolygonShape(float width, float height, float angle = 0.0f) :
			this(width, height, Vec2.Empty, angle)
		{
		}

		internal PolygonShape(cb2polygonshapeportable portableShape)
		{
			_internalPolyShape = portableShape;
			InternalShape = portableShape.m_shape;
		}

		public override Shape Clone()
		{
			PolygonShape shape = new PolygonShape();
			shape.Centroid = Centroid;
			shape.Normals = Normals;
			shape.Radius = Radius;
			shape.Vertices = Vertices;
			return shape;
		}

		public void SetAsBox(float hx, float hy)
		{
			SetAsBox(hx, hy, Vec2.Empty, 0);
		}

		internal bool _autoSet = true;
		public void SetAsBox(float hx, float hy, Vec2 center, float angle)
		{
			var tempVertices = new Vec2[]
			{
				new Vec2(-hx, -hy),
				new Vec2(hx, -hy),
				new Vec2(hx, hy),
				new Vec2(-hx, hy)
			};

			var tempNormals = new Vec2[]
			{
				new Vec2(0.0f, -1.0f),
				new Vec2(1.0f, 0.0f),
				new Vec2(0.0f, 1.0f),
				new Vec2(-1.0f, 0.0f)
			};

			Transform xf = new Transform();
			xf.Position = center;
			xf.R = new Mat22(angle);

			// Transform vertices and normals.
			for (int i = 0; i < tempVertices.Length; ++i)
			{
				tempVertices[i] = xf * tempVertices[i];
				tempNormals[i] = xf.R * tempNormals[i];
			}

			Vertices = tempVertices;
			Normals = tempNormals;

			Centroid = center;
		}

		public void SetAsEdge(Vec2 v1, Vec2 v2)
		{
			Vertices = new Vec2[]
			{
				v1,
				v2
			};
			Centroid = 0.5f * (v1 + v2);

			var tempNormal = (v2 - v1).Cross(1.0f).Normalized();
			Normals = new Vec2[]
			{
				tempNormal,
				-tempNormal
			};
		}

		/// <summary>
		/// Get or set the local center point of this shape
		/// </summary>
		public Vec2 Centroid
		{
			get { return _internalPolyShape.m_centroid; }
			set { _internalPolyShape.m_centroid = value; }
		}

		[Browsable(false)]
		public int VertexCount
		{
			get { return _internalPolyShape.m_vertexCount; }
			set { _internalPolyShape.m_vertexCount = value; }
		}

		public static void ComputeCentroid(out Vec2 centroid, System.Collections.Generic.IList<Vec2> vertices)
		{
			if (!(vertices.Count >= 2))
				throw new ArgumentOutOfRangeException("vertices");

			centroid = Vec2.Empty;
			float area = 0.0f;

			if (vertices.Count == 2)
			{
				centroid = 0.5f * (vertices[0] + vertices[1]);
				return;
			}

			// pRef is the reference point for forming triangles.
			// It's location doesn't change the result (except for rounding error).
			Vec2  pRef = Vec2.Empty;

			const float inv3 = 1.0f / 3.0f;

			for (int i = 0; i < vertices.Count; ++i)
			{
				// Triangle vertices.
				Vec2 p1 = pRef;
				Vec2 p2 = vertices[i];
				Vec2 p3 = i + 1 < vertices.Count ? vertices[i + 1] : vertices[0];

				Vec2 e1 = p2 - p1;
				Vec2 e2 = p3 - p1;

				float D = e1.Cross(e2);

				float triangleArea = 0.5f * D;
				area += triangleArea;

				// Area weighted centroid
				centroid += triangleArea * inv3 * (p1 + p2 + p3);
			}

			// Centroid
			if (!(area > float.Epsilon))
				throw new Exception("Area of polygon is too small");

			centroid *= 1.0f / area;
		}

		void Set(Vec2[] verts)
		{
			Vec2[] normals = new Vec2[verts.Length];

			if (!(2 <= verts.Length && verts.Length <= Box2DSettings.b2_maxPolygonVertices))
				throw new ArgumentOutOfRangeException("verts", "Vertice count is " + ((2 <= verts.Length) ? "less than 2" : "greater than "+Box2DSettings.b2_maxPolygonVertices.ToString()));

			if (_autoReverse)
			// Ensure the polygon is convex and the interior
			// is to the left of each edge.
			{
				bool _reversed = false;
				for (int i = 0; i < verts.Length; ++i)
				{
					int i1 = i;
					int i2 = i + 1 < verts.Length ? i + 1 : 0;
					Vec2 edge = verts[i2] - verts[i1];

					for (int j = 0; j < verts.Length; ++j)
					{
						// Don't check vertices on the current edge.
						if (j == i1 || j == i2)
						{
							continue;
						}

						Vec2 r = verts[j] - verts[i1];

						// Your polygon is non-convex (it has an indentation) or
						// has colinear edges.
						float s = edge.Cross(r);
						if (!(s > 0.0f))
						{
							_reversed = true;
							verts = ReverseOrder(verts);
							break;
						}
					}

					if (_reversed)
						break;
				}
			}

			// Compute normals. Ensure the edges have non-zero length.
			for (int i = 0; i < verts.Length; ++i)
			{
				int i1 = i;
				int i2 = i + 1 < verts.Length ? i + 1 : 0;
				Vec2 edge = verts[i2] - verts[i1];

				if (!(edge.LengthSquared() > float.Epsilon * float.Epsilon))
					throw new Exception("Edge has a close-to-zero length (vertices too close?)");

				normals[i] = edge.Cross(1.0f);
				normals[i].Normalize();
			}

			// Compute the polygon centroid.
			ComputeCentroid(out _internalPolyShape.m_centroid, verts);

			for (int i = 0; i < verts.Length; ++i)
			{
				_internalPolyShape.m_vertices[i] = verts[i];
				_internalPolyShape.m_normals[i] = normals[i];
			}

			VertexCount = verts.Length;
		}

		/// <summary>
		/// Get or set the vertices that make up this shape.
		/// </summary>
		public Vec2[] Vertices
		{
			get
			{
				Vec2[] verts = new Vec2[VertexCount];

				for (int i = 0; i < verts.Length; ++i)
					verts[i] = _internalPolyShape.m_vertices[i];

				return verts;
			}

			set
			{
				if (value.Length > 8)
					throw new IndexOutOfRangeException("value");

				if (_autoSet)
					Set(value);
			}
		}

		[Browsable(false)]
		public Vec2[] Normals
		{
			get
			{
				Vec2[] verts = new Vec2[VertexCount];

				for (int i = 0; i < verts.Length; ++i)
					verts[i] = _internalPolyShape.m_normals[i];

				return verts;
			}

			set
			{
				if (value.Length > 8)
					throw new IndexOutOfRangeException("value");

				for (int i = 0; i < value.Length; ++i)
					_internalPolyShape.m_normals[i] = value[i];

				_internalPolyShape.m_vertexCount = value.Length;
			}
		}

		public Vec2[] ReverseOrder(Vec2[] verts)
		{
			var list = new System.Collections.Generic.List<Vec2>(verts);
			list.Reverse();
			return list.ToArray();
		}

		public override void ComputeAABB(out AABB aabb, Transform xf)
		{
			Vec2 lower = xf * Vertices[0];
			Vec2 upper = lower;

			for (int i = 1; i < VertexCount; ++i)
			{
				Vec2 v = xf * Vertices[i];
				lower = Vec2.Min(lower, v);
				upper = Vec2.Max(upper, v);
			}

			Vec2 r = new Vec2(Radius, Radius);
			aabb = new AABB(lower - r,
							upper + r);
		}

		public override void ComputeMass(out MassData massData, float density)
		{
			if (VertexCount < 2)
				throw new ArgumentOutOfRangeException("Vertice count less than 2");

			// A line segment has zero mass.
			if (VertexCount == 2)
			{
				massData = new MassData(0.0f,
										0.5f * (Vertices[0] + Vertices[1]),
										0.0f);
				return;
			}

			Vec2 center = Vec2.Empty;
			float area = 0.0f;
			float I = 0.0f;

			// pRef is the reference point for forming triangles.
			// It's location doesn't change the result (except for rounding error).
			Vec2 pRef = Vec2.Empty;
		#if NO
			// This code would put the reference point inside the polygon.
			for (int i = 0; i < VertexCount; ++i)
			{
				pRef += Vertices[i];
			}
			pRef *= 1.0f / count;
		#endif

			const float k_inv3 = 1.0f / 3.0f;

			for (int i = 0; i < VertexCount; ++i)
			{
				// Triangle vertices.
				Vec2 p1 = pRef;
				Vec2 p2 = Vertices[i];
				Vec2 p3 = i + 1 < VertexCount ? Vertices[i+1] : Vertices[0];

				Vec2 e1 = p2 - p1;
				Vec2 e2 = p3 - p1;

				float D = e1.Cross(e2);

				float triangleArea = 0.5f * D;
				area += triangleArea;

				// Area weighted centroid
				center += triangleArea * k_inv3 * (p1 + p2 + p3);

				float px = p1.X, py = p1.Y;
				float ex1 = e1.X, ey1 = e1.Y;
				float ex2 = e2.X, ey2 = e2.Y;

				float intx2 = k_inv3 * (0.25f * (ex1*ex1 + ex2*ex1 + ex2*ex2) + (px*ex1 + px*ex2)) + 0.5f*px*px;
				float inty2 = k_inv3 * (0.25f * (ey1*ey1 + ey2*ey1 + ey2*ey2) + (py*ey1 + py*ey2)) + 0.5f*py*py;

				I += D * (intx2 + inty2);
			}

			if (!(area > float.Epsilon))
				throw new Exception("Area is too small");

			massData = new MassData(density * area,
				center * (1.0f / area),
				density * I);
		}

		public override bool TestPoint(Transform xf, Vec2 p)
		{
			Vec2 pLocal = xf.R.MulT(p - xf.Position);

			for (int i = 0; i < VertexCount; ++i)
			{
				float dot = Normals[i].Dot(pLocal - Vertices[i]);

				if (dot > 0.0f)
					return false;
			}

			return true;
		}

		public bool CompareWith(PolygonShape shape)
		{
			return (this.Radius == shape.Radius && 
				this.Centroid == shape.Centroid &&
				this.Vertices == shape.Vertices &&
				this.Normals == shape.Normals &&
				this.VertexCount == shape.VertexCount);
		}

		public override string ToString()
		{
			string str = base.ToString() + " Centroid=" + Centroid.ToString() + " Vertices={";

			bool _started = false;
			foreach (var v in Vertices)
			{
				if (!_started)
					_started = true;
				else
					str += ' ';

				str += '{'+v.ToString()+'}';
			}

			str += '}';
			return str;
		}

		public static PolygonShape Parse(string p)
		{
			SimpleParser parser = new SimpleParser(p, true);
			Vec2[] vertices = SimpleArrayParser.GenerateArray<Vec2>((parser.ValueFromKey("vertices")), delegate(string input) { return Vec2.Parse(input); });

			return new PolygonShape(vertices, Vec2.Parse(parser.ValueFromKey("Centroid")));
		}
	}
}
