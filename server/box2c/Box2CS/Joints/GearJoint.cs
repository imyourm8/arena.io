using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class GearJointDef : JointDef
	{
		IntPtr _joint1;
		IntPtr _joint2;
		float _ratio;

		public GearJointDef ()
		{
			JointType = JointType.Gear;
			_joint1 = IntPtr.Zero;
			_joint2 = IntPtr.Zero;
			_ratio = 1.0f;
		}

		public Joint JointA
		{
			get { return Joint.FromPtr(_joint1); }
			set { _joint1 = value.JointPtr; }
		}

		public Joint JointB
		{
			get { return Joint.FromPtr(_joint2); }
			set { _joint2 = value.JointPtr; }
		}

		public float Ratio
		{
			get { return _ratio; }
			set { _ratio = value; }
		}
	}

	public sealed class GearJoint : Joint
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2gearjoint_setratio(IntPtr joint, float data);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2gearjoint_getratio(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2gearjoint_getjointa(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2gearjoint_getjointb(IntPtr joint);
		}

		public GearJoint(IntPtr ptr) :
			base(ptr)
		{
		}

		public Joint JointA
		{
			get { return Joint.FromPtr(NativeMethods.b2gearjoint_getjointa(JointPtr)); }
		}

		public Joint JointB
		{
			get { return Joint.FromPtr(NativeMethods.b2gearjoint_getjointb(JointPtr)); }
		}

		public float Ratio
		{
			get { return NativeMethods.b2gearjoint_getratio(JointPtr); }
			set { NativeMethods.b2gearjoint_setratio(JointPtr, value); }
		}
	}
}
