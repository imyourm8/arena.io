using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;

namespace Box2CS
{
	public static class b2Math
	{
		public static bool IsValid(float x)
		{
			if (float.IsNaN(x) || float.IsInfinity(x))
				return false;

			return true;
		}

		public static float b2Clamp(float a, float low, float high)
		{
			return Math.Max(low, Math.Min(a, high));
		}

		public static int b2Clamp(int a, int low, int high)
		{
			return Math.Max(low, Math.Min(a, high));
		}

		public static Vec2 b2Cross(float s, Vec2 a)
		{
			return new Vec2(-s * a.Y, s * a.X);
		}
	
		const double Deg2RadConst = Math.PI / 180.0;
		const double Rad2DegConst = 180.0 / Math.PI;

		public static double Rad2Deg(double angle)
		{
			return angle * Rad2DegConst;
		}

		public static double Deg2Rad(double angle)
		{
			return angle * Deg2RadConst;
		}

		public static float Rad2Deg(float angle)
		{
			return (float)Rad2Deg((double)angle);
		}

		public static float Deg2Rad(float angle)
		{
			return (float)Deg2Rad((double)angle);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Vec2
	{
		public static Vec2 Min(Vec2 a, Vec2 b)
		{
			return new Vec2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
		}

		public static Vec2 Max(Vec2 a, Vec2 b)
		{
			return new Vec2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
		}

		public static Vec2 Empty = new Vec2(0, 0);

		/// <summary>
		/// The value of this vector in the X axis (typically left and right).
		/// </summary>
		public float X
		{
			get;
			set;
		}

		/// <summary>
		/// The value of this vector in the Y axis (typically up and down).
		/// </summary>
		public float Y
		{
			get;
			set;
		}

		public Vec2(float x, float y) :
			this()
		{
			X = x;
			Y = y;
		}

		public static Vec2 operator-(Vec2 l, Vec2 r)
		{
			return new Vec2(l.X - r.X, l.Y - r.Y);
		}

		public static Vec2 operator+(Vec2 l, Vec2 r)
		{
			return new Vec2(l.X + r.X, l.Y + r.Y);
		}

		public static Vec2 operator *(Vec2 l, float v)
		{
			return new Vec2(l.X * v, l.Y * v);
		}

		public static Vec2 operator /(Vec2 l, float v)
		{
			return new Vec2(l.X / v, l.Y / v);
		}

		public static Vec2 operator *(Vec2 l, Vec2 v)
		{
			return new Vec2(l.X * v.X, l.Y * v.Y);
		}

		public static Vec2 operator*(float v, Vec2 l)
		{
			return l * v;
		}

		public static Vec2 operator-(Vec2 l)
		{
			return new Vec2(-l.X, -l.Y);
		}

		public static bool operator==(Vec2 l, Vec2 r)
		{
			return (l.X == r.X && l.Y == r.Y);
		}

		public override bool Equals(object obj)
		{
			if (obj is Vec2)
				return ((Vec2)obj) == this;

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator !=(Vec2 l, Vec2 r)
		{
			return !(l == r);
		}

		/// Perform the dot product on two vectors.
		public float Dot(Vec2 b)
		{
			return X * b.X + Y * b.Y;
		}

		/// Perform the cross product on two vectors. In 2D this produces a scalar.
		public float Cross(Vec2 b)
		{
			return X * b.Y - Y * b.X;
		}

		/// Perform the cross product on a vector and a scalar. In 2D this produces
		/// a vector.
		public Vec2 Cross(float s)
		{
			return new Vec2(s * Y, -s * X);
		}

		/// Perform the cross product on a scalar and a vector. In 2D this produces
		/// a vector.
		public static Vec2 b2Cross(float s, Vec2 a)
		{
			return new Vec2(-s * a.Y, s * a.X);
		}

		public bool IsValid()
		{
			return b2Math.IsValid(X) && b2Math.IsValid(Y);
		}

		public float Length()
		{
			return (float)Math.Sqrt(X * X + Y * Y);
		}

		public float Distance(Vec2 v)
		{
			var sub = (this - v);
			return sub.Dot(sub);
		}

		public float Normalize()
		{
			float length = Length();

			if (length < float.Epsilon)
				return 0.0f;

			float invLength = 1.0f / length;
			X *= invLength;
			Y *= invLength;

			return length;
		}

		public Vec2 Normalized()
		{
			var newVec = this;
			newVec.Normalize();
			return newVec;
		}

		public float LengthSquared()
		{
			return X * X + Y * Y;
		}

		public static Vec2 Parse(string p)
		{
			var parsed = p.Split(' ', ',');
			return new Vec2(float.Parse(parsed[0]), float.Parse(parsed[1]));
		}

		public override string ToString()
		{
			return X.ToString() + "," + Y.ToString();
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Mat22
	{
		public static Mat22 Identity = new Mat22(new Vec2(1.0f, 0.0f), new Vec2(0.0f, 1.0f));

		public Vec2 Col1
		{
			get;
			set;
		}

		public Vec2 Col2
		{
			get;
			set;
		}

		public static Vec2 operator*(Mat22 A, Vec2 v)
		{
			return new Vec2(A.Col1.X * v.X + A.Col2.X * v.Y, A.Col1.Y * v.X + A.Col2.Y * v.Y);
		}

		public Vec2 MulT(Vec2 v)
		{
			return new Vec2(v.Dot(Col1), v.Dot(Col2));
		}

		public Mat22 MulT(Mat22 m)
		{
			Vec2 c1 = new Vec2(Col1.Dot(m.Col1), Col2.Dot(m.Col1));
			Vec2 c2 = new Vec2(Col1.Dot(m.Col2), Col2.Dot(m.Col2));
			return new Mat22(c1, c2);
		}

		public Mat22(Vec2 ncol1, Vec2 ncol2) :
			this()
		{
			Col1 = ncol1;
			Col2 = ncol2;
		}

		public Mat22(float angle) :
			this()
		{
			float c = (float)Math.Cos(angle), s = (float)Math.Sin(angle);
			Col1 = new Vec2(c, s);
			Col2 = new Vec2(-s, c);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Transform
	{
		public static Transform Identity = new Transform(Vec2.Empty, Mat22.Identity);

		public Vec2 Position
		{
			get;
			set;
		}

		public Mat22 R
		{
			get;
			set;
		}

		public static Vec2 operator*(Transform T, Vec2 v)
		{
			float x = T.Position.X + T.R.Col1.X * v.X + T.R.Col2.X * v.Y;
			float y = T.Position.Y + T.R.Col1.Y * v.X + T.R.Col2.Y * v.Y;

			return new Vec2(x, y);
		}

		public Vec2 MulT(Vec2 v)
		{
			return R.MulT(v - Position);
		}

		public Transform(Vec2 position, Mat22 r) :
			this()
		{
			Position = position;
			R = r;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ColorF
	{
		public float R
		{
			get;
			set;
		}

		public float G
		{
			get;
			set;
		}

		public float B
		{
			get;
			set;
		}

		public float A
		{
			get;
			set;
		}

		public ColorF(float r, float g, float b, float a) :
			this()
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public ColorF(float r, float g, float b) :
			this(r, g, b, 1)
		{
		}

		public ColorF(System.Drawing.Color color) :
			this(
				I2F(color.R, 255),
				I2F(color.G, 255),
				I2F(color.B, 255),
				I2F(color.A, 255)
			)
		{
		}

		static int F2I(float f, int multiplier)
		{
			return (int)(f * multiplier);
		}

		static float I2F(int i, int multiplier)
		{
			return (float)i / (float)multiplier;
		}

		public System.Drawing.Color ToGDIColor()
		{
			return System.Drawing.Color.FromArgb(
				F2I(R, 255),
				F2I(G, 255),
				F2I(B, 255),
				F2I(A, 255));
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct AABB
	{
		public Vec2 LowerBound
		{
			get;
			set;
		}

		public Vec2 UpperBound
		{
			get;
			set;
		}

		public AABB(Vec2 lowerBound, Vec2 upperBound) :
			this()
		{
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}

		/// Verify that the bounds are sorted.
		public bool IsValid()
		{
			Vec2 d = UpperBound - LowerBound;
			bool valid = d.X >= 0.0f && d.Y >= 0.0f;
			valid = valid && LowerBound.IsValid() && UpperBound.IsValid();
			return valid;
		}

		/// Get the center of the AABB.
		public Vec2 GetCenter()
		{
			return 0.5f * (LowerBound + UpperBound);
		}

		/// Get the extents of the AABB (half-widths).
		public Vec2 GetExtents()
		{
			return 0.5f * (UpperBound - LowerBound);
		}

		/// Combine two AABBs into this one.
		public void Combine(AABB aabb1, AABB aabb2)
		{
			LowerBound = Vec2.Min(aabb1.LowerBound, aabb2.LowerBound);
			UpperBound = Vec2.Min(aabb1.UpperBound, aabb2.UpperBound);
		}

		/// Does this aabb contain the provided AABB.
		public bool Contains(AABB aabb)
		{
			bool result = true;
			result = result && LowerBound.X <= aabb.LowerBound.X;
			result = result && LowerBound.Y <= aabb.LowerBound.Y;
			result = result && aabb.UpperBound.X <= UpperBound.X;
			result = result && aabb.UpperBound.Y <= UpperBound.Y;
			return result;
		}

		/// Does this aabb contain the provided AABB.
		public bool Contains(Vec2 pt)
		{
			bool result = true;
			result = result && LowerBound.X <= pt.X;
			result = result && LowerBound.Y <= pt.Y;
			result = result && pt.X <= UpperBound.X;
			result = result && pt.Y <= UpperBound.Y;
			return result;
		}

		public static bool TestOverlap(AABB a, AABB b)
		{
			var d1 = b.LowerBound - a.UpperBound;
			var d2 = a.LowerBound - b.UpperBound;

			if (d1.X > 0.0f || d1.Y > 0.0f)
				return false;

			if (d2.X > 0.0f || d2.Y > 0.0f)
				return false;

			return true;
		}

		public bool Overlaps(AABB b)
		{
			return TestOverlap(this, b);
		}
	};
}
