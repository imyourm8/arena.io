using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	public sealed class JointEdge
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2jointedge_getother(IntPtr jointEdge);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2jointedge_getjoint(IntPtr jointEdge);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2jointedge_getprev(IntPtr jointEdge);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2jointedge_getnext(IntPtr jointEdge);
		}

		IntPtr _jointEdgePtr;

		internal JointEdge(IntPtr ptr)
		{
			_jointEdgePtr = ptr;
		}

		internal static JointEdge FromPtr(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return null;

			return new JointEdge(ptr);
		}

		public Body Body
		{
			get { return Body.FromPtr(NativeMethods.b2jointedge_getother(_jointEdgePtr)); }
		}

		public Joint Joint
		{
			get { return Joint.FromPtr(NativeMethods.b2jointedge_getjoint(_jointEdgePtr)); }
		}

		public JointEdge Next
		{
			get { return JointEdge.FromPtr(NativeMethods.b2jointedge_getnext(_jointEdgePtr)); }
		}

		public JointEdge Prev
		{
			get { return JointEdge.FromPtr(NativeMethods.b2jointedge_getprev(_jointEdgePtr)); }
		}

		public static bool operator ==(JointEdge l, JointEdge r)
		{
			if ((object)l == null && (object)r == null)
				return true;
			else if ((object)l == null && (object)r != null ||
				(object)l != null && (object)r == null)
				return false;

			return l._jointEdgePtr == r._jointEdgePtr;
		}

		public static bool operator !=(JointEdge l, JointEdge r)
		{
			return !(l == r);
		}

		public override bool Equals(object obj)
		{
			if (obj is JointEdge)
				return (obj as JointEdge) == this;

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return _jointEdgePtr.GetHashCode();
		}
	}
}
