using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void BeginEndContactDelegate(IntPtr contact);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void PreSolveDelegate(IntPtr contact, Manifold oldManifold);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void PostSolveDelegate(IntPtr contact, ContactImpulse impulse);

	[StructLayout(LayoutKind.Sequential)]
	internal struct cb2contactlistener
	{
		public BeginEndContactDelegate BeginContact;
		public BeginEndContactDelegate EndContact;
		public PreSolveDelegate PreSolve;
		public PostSolveDelegate PostSolve;
	}

	/// Contact impulses for reporting. Impulses are used instead of forces because
	/// sub-step forces may approach infinity for rigid body collisions. These
	/// match up one-to-one with the contact points in Manifold.
	[StructLayout(LayoutKind.Sequential)]
	public struct ContactImpulse
	{
		float _normalImpulse0, _normalImpulse1;

		public float GetNormalImpulse(int index)
		{
			if (index == 0)
				return _normalImpulse0;
			else
				return _normalImpulse1;
		}

		float _tangentImpulse0, _tangentImpulse1;

		public float GetTangentImpulse(int index)
		{
			if (index == 0)
				return _tangentImpulse0;
			else
				return _tangentImpulse1;
		}

	};

	public abstract class ContactListener : IDisposable
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr cb2contactlistener_create(cb2contactlistener functions);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void cb2contactlistener_destroy(IntPtr listener);
		}

		IntPtr _listener;
		cb2contactlistener functions;

		internal IntPtr Listener
		{
			get { return _listener; }
		}

		public ContactListener()
		{
			functions = new cb2contactlistener();
			functions.BeginContact = BeginContactInternal;
			functions.EndContact = EndContactInternal;
			functions.PreSolve = PreSolveInternal;
			functions.PostSolve = PostSolveInternal;

			_listener = NativeMethods.cb2contactlistener_create(functions);
		}

		void BeginContactInternal(IntPtr contact)
		{
			BeginContact(Contact.FromPtr(contact));
		}

		void EndContactInternal(IntPtr contact)
		{
			EndContact(Contact.FromPtr(contact));	
		}

		void PreSolveInternal(IntPtr contact, Manifold oldManifold)
		{
			PreSolve(Contact.FromPtr(contact), oldManifold);
		}

		void PostSolveInternal(IntPtr contact, ContactImpulse impulse)
		{
			PostSolve(Contact.FromPtr(contact), impulse);
		}

		public abstract void BeginContact(Contact contact);
		public abstract void EndContact(Contact contact);
		public abstract void PreSolve(Contact contact, Manifold oldManifold);
		public abstract void PostSolve(Contact contact, ContactImpulse impulse);

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

				NativeMethods.cb2contactlistener_destroy(_listener);

				disposed = true;
			}
		}

		~ContactListener()
		{
			Dispose(false);
		}
		#endregion
	}
}
