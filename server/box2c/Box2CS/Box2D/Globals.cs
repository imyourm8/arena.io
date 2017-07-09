using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	public struct ClipVertex
	{
		public Vec2 V
		{
			get;
			set;
		}

		public ContactID ID
		{
			get;
			set;
		}

		public ClipVertex(Vec2 v, ContactID id) :
			this()
		{
			V = v;
			ID = id;
		}
	};

	public static class Box2D
	{
		class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void cb2_collidecircles(out Manifold manifold, IntPtr circle1, Transform xf1, IntPtr circle2, Transform xf2);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void cb2_collidepolygonandcircle(out Manifold manifold, IntPtr polygon, Transform xf1, IntPtr circle, Transform xf2);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void cb2_collidepolygons(out Manifold manifold, IntPtr polygon1, Transform xf1, IntPtr polygon2, Transform xf2);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern int cb2_clipsegmenttoline([MarshalAs(UnmanagedType.LPArray, SizeConst=2)]out ClipVertex[] vOut, ClipVertex[] vIn, Vec2 normal, float offset);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool cb2_testoverlap(IntPtr shapeA, IntPtr shapeB, Transform xfA, Transform xfB);
		}

		public static void CollideCircles(out Manifold manifold, CircleShape circle1, Transform xf1, CircleShape circle2, Transform xf2)
		{
			var circle1Lock = circle1.Lock();
			var circle2Lock = circle2.Lock();

			NativeMethods.cb2_collidecircles(out manifold, circle1Lock, xf1, circle2Lock, xf2);

			circle1.Unlock();
			circle2.Unlock();
		}

		public static void CollidePolygonAndCircle(out Manifold manifold, PolygonShape polygon, Transform xf1, CircleShape circle, Transform xf2)
		{
			var polyLock = polygon.Lock();
			var circleLock = circle.Lock();

			NativeMethods.cb2_collidepolygonandcircle(out manifold, polyLock, xf1, circleLock, xf2);

			polygon.Unlock();
			circle.Unlock();
		}

		public static void CollidePolygons(out Manifold manifold, PolygonShape poly1, Transform xf1, PolygonShape poly2, Transform xf2)
		{
			var poly1Lock = poly1.Lock();
			var poly2Lock = poly2.Lock();

			NativeMethods.cb2_collidepolygons(out manifold, poly1Lock, xf1, poly2Lock, xf2);

			poly1.Unlock();
			poly2.Unlock();
		}

		public static int ClipSegmentToLine(out ClipVertex[] vOut, ClipVertex[] vIn, Vec2 normal, float offset)
		{
			return NativeMethods.cb2_clipsegmenttoline(out vOut, vIn, normal, offset);
		}

		public static bool TestOverlap(Shape shapeA, Shape shapeB, Transform xfA, Transform xfB)
		{
			var sh1Lock = shapeA.Lock();
			var sh2Lock = shapeB.Lock();

			bool rV = NativeMethods.cb2_testoverlap(sh1Lock, sh2Lock, xfA, xfB);

			shapeA.Unlock();
			shapeB.Unlock();

			return rV;
		}
	}
}
