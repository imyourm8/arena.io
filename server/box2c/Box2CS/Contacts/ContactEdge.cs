using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	public sealed class ContactEdge
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2contactedge_getother(IntPtr contactEdge);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2contactedge_getcontact(IntPtr contactEdge);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2contactedge_getprev(IntPtr contactEdge);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2contactedge_getnext(IntPtr contactEdge);
		}

		IntPtr _contactEdgePtr;

		internal ContactEdge(IntPtr ptr)
		{
			_contactEdgePtr = ptr;
		}

		internal static ContactEdge FromPtr(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return null;

			return new ContactEdge(ptr);
		}

		public Body Body
		{
			get { return Body.FromPtr(NativeMethods.b2contactedge_getother(_contactEdgePtr)); }
		}

		public Contact Contact
		{
			get { return Contact.FromPtr(NativeMethods.b2contactedge_getcontact(_contactEdgePtr)); }
		}

		public ContactEdge Next
		{
			get { return ContactEdge.FromPtr(NativeMethods.b2contactedge_getnext(_contactEdgePtr)); }
		}

		public ContactEdge Prev
		{
			get { return ContactEdge.FromPtr(NativeMethods.b2contactedge_getprev(_contactEdgePtr)); }
		}

		public static bool operator ==(ContactEdge l, ContactEdge r)
		{
			if ((object)l == null && (object)r == null)
				return true;
			else if ((object)l == null && (object)r != null ||
				(object)l != null && (object)r == null)
				return false;

			return l._contactEdgePtr == r._contactEdgePtr;
		}

		public static bool operator !=(ContactEdge l, ContactEdge r)
		{
			return !(l == r);
		}

		public override bool Equals(object obj)
		{
			if (obj is ContactEdge)
				return (obj as ContactEdge) == this;

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return _contactEdgePtr.GetHashCode();
		}
	}
}
