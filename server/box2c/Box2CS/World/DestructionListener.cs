using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	public abstract class DestructionListener : IDisposable
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr cb2destructionlistener_create(cb2destructionlistener functions);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void cb2destructionlistener_destroy(IntPtr listener);
		}

		IntPtr _listener;
		cb2destructionlistener functions;

		internal IntPtr Listener
		{
			get { return _listener; }
		}

		public DestructionListener()
		{
			functions = new cb2destructionlistener();
			functions.SayGoodbyeFixture = SayGoodbyeFixture;
			functions.SayGoodbyeJoint = SayGoodbyeJoint;

			_listener = NativeMethods.cb2destructionlistener_create(functions);
		}

		void SayGoodbyeFixture(IntPtr ptr)
		{
			SayGoodbye(Fixture.FromPtr(ptr));
		}

		void SayGoodbyeJoint(IntPtr ptr)
		{
			SayGoodbye(Joint.FromPtr(ptr));
		}

		public abstract void SayGoodbye(Fixture fixture);
		public abstract void SayGoodbye(Joint joint);

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

				NativeMethods.cb2destructionlistener_destroy(_listener);

				disposed = true;
			}
		}

		~DestructionListener()
		{
			Dispose(false);
		}
		#endregion
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SayGoodbye(IntPtr type);

	[StructLayout(LayoutKind.Sequential)]
	internal struct cb2destructionlistener
	{
		public SayGoodbye SayGoodbyeJoint;
		public SayGoodbye SayGoodbyeFixture;
	}
}
