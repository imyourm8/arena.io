using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool QueryReportFixture(IntPtr fixture);

	[StructLayout(LayoutKind.Sequential)]
	internal struct cb2querycallback
	{
		public QueryReportFixture ReportFixture;
	}

	public abstract class QueryCallback : IDisposable
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr cb2querycallback_create(cb2querycallback functions);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void cb2querycallback_destroy(IntPtr listener);
		}

		IntPtr _listener;
		cb2querycallback functions;

		internal IntPtr Listener
		{
			get { return _listener; }
		}

		public QueryCallback()
		{
			functions = new cb2querycallback();
			functions.ReportFixture = ReportFixtureInternal;

			_listener = NativeMethods.cb2querycallback_create(functions);
		}

		bool ReportFixtureInternal(IntPtr fixture)
		{
			return ReportFixture(Fixture.FromPtr(fixture));
		}

		public abstract bool ReportFixture(Fixture fixture);

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

				NativeMethods.cb2querycallback_destroy(_listener);

				disposed = true;
			}
		}

		~QueryCallback()
		{
			Dispose(false);
		}
		#endregion
	}
}
