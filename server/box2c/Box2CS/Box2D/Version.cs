using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Box2DVersion
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2version_get(out Box2DVersion version);
		}

		static Box2DVersion GetBox2DVersion()
		{
			Box2DVersion ver;
			NativeMethods.b2version_get(out ver);
			return ver;
		}

		public static Box2DVersion Version = GetBox2DVersion();

		int _major, _minor, _revision;

		public int Major
		{
			get { return _major; }
		}

		public int Minor
		{
			get { return _minor; }
		}

		public int Revision
		{
			get { return _revision; }
		}

		public override string ToString()
		{
			return Major.ToString() + "." + Minor.ToString() + "." + Revision.ToString();
		}
	}
}
