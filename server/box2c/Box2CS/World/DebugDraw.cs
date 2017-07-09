using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawPolygon([MarshalAs(UnmanagedType.LPArray, SizeConst=Box2DSettings.b2_maxPolygonVertices)]Vec2[] vertices, int vertexCount, ColorF color);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawCircle(Vec2 center, float radius, ColorF color);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawSolidCircle(Vec2 center, float radius, Vec2 axis, ColorF color);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawSegment(Vec2 p1, Vec2 p2, ColorF color);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawTransform(Transform xf);

	[StructLayout(LayoutKind.Sequential)]
	internal struct cb2debugdraw
	{
		public DrawPolygon DrawPolygonCallback,
					DrawSolidPolygonCallback;
		public DrawCircle DrawCircleCallback;
		public DrawSolidCircle DrawSolidCircleCallback;
		public DrawSegment DrawSegmentCallback;
		public DrawTransform DrawTransformCallback;
	}

	[Flags]
	public enum DebugFlags
	{
		Shapes				= 0x0001, ///< draw shapes
		Joints				= 0x0002, ///< draw joint connections
		AABBs				= 0x0004, ///< draw axis aligned bounding boxes
		Pairs				= 0x0008, ///< draw broad-phase pairs
		CenterOfMasses		= 0x0010, ///< draw center of mass frame
	};

	public abstract class DebugDraw : IDisposable
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr cb2debugdraw_create(cb2debugdraw functions);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern uint cb2debugdraw_getflags(IntPtr wrapper);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void cb2debugdraw_setflags(IntPtr wrapper, uint flags);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void cb2debugdraw_destroy(IntPtr wrapper);
		}


		IntPtr _debugDraw;
		internal cb2debugdraw funcs;

		public DebugDraw()
		{
			funcs = new cb2debugdraw();
			funcs.DrawPolygonCallback = DrawPolygon;
			funcs.DrawSolidPolygonCallback = DrawSolidPolygon;
			funcs.DrawCircleCallback = DrawCircle;
			funcs.DrawSolidCircleCallback = DrawSolidCircle;
			funcs.DrawSegmentCallback = DrawSegment;
			funcs.DrawTransformCallback = DrawTransform;

			_debugDraw = NativeMethods.cb2debugdraw_create(funcs);
		}

		public DebugFlags Flags
		{
			get { return (DebugFlags)NativeMethods.cb2debugdraw_getflags(_debugDraw); }
			set { NativeMethods.cb2debugdraw_setflags(_debugDraw, (uint)value); }
		}

		internal IntPtr Listener
		{
			get { return _debugDraw; }
		}

		public abstract void DrawPolygon(Vec2[] vertices, int vertexCount, ColorF color);
		public abstract void DrawSolidPolygon(Vec2[] vertices, int vertexCount, ColorF color);
		public abstract void DrawCircle(Vec2 center, float radius, ColorF color);
		public abstract void DrawSolidCircle(Vec2 center, float radius, Vec2 axis, ColorF color);
		public abstract void DrawSegment(Vec2 p1, Vec2 p2, ColorF color);
		public abstract void DrawTransform(Transform xf);

		#region IDisposable
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool disposed;

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
				}

				NativeMethods.cb2debugdraw_destroy(_debugDraw);

				disposed = true;
			}
		}

		~DebugDraw()
		{
			Dispose(false);
		}
		#endregion
	}
}
