using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float RayCastReportFixture (IntPtr fixture, Vec2 point,
									Vec2 normal, float fraction);

	[StructLayout(LayoutKind.Sequential)]
	internal struct cb2raycastcallback
	{
		public RayCastReportFixture ReportFixture;
	}

	public abstract class RayCastCallback : IDisposable
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr cb2raycastcallback_create(cb2raycastcallback functions);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void cb2raycastcallback_destroy(IntPtr listener);
		}

		IntPtr _listener;
		cb2raycastcallback functions;

		internal IntPtr Listener
		{
			get { return _listener; }
		}

		public RayCastCallback()
		{
			functions = new cb2raycastcallback();
			functions.ReportFixture = ReportFixtureInternal;

			_listener = NativeMethods.cb2raycastcallback_create(functions);
		}

		float ReportFixtureInternal(IntPtr fixture, Vec2 point,
									Vec2 normal, float fraction)
		{
			return ReportFixture(Fixture.FromPtr(fixture), point, normal, fraction);
		}

		public abstract float ReportFixture(Fixture fixture, Vec2 point,
									Vec2 normal, float fraction);

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

				NativeMethods.cb2raycastcallback_destroy(_listener);

				disposed = true;
			}
		}

		~RayCastCallback()
		{
			Dispose(false);
		}
		#endregion
	}
}
