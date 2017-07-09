using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public struct RayCastInput
	{
		public Vec2 Point1
		{
			get;
			set;
		}

		public Vec2 Point3
		{
			get;
			set;
		}

		public float MaxFraction
		{
			get;
			set;
		}
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct RayCastOutput
	{
		public Vec2 Normal
		{
			get;
			set;
		}

		public float Fraction
		{
			get;
			set;
		}
	};
}
