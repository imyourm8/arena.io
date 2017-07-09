using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class MouseJointDef : JointDef
	{
		Vec2 _target;
		float _maxForce;
		float _frequencyHz;
		float _dampingRatio;

		public MouseJointDef()
		{
			JointType = JointType.Mouse;
			_target = Vec2.Empty;
			_maxForce = 0.0f;
			_frequencyHz = 5.0f;
			_dampingRatio = 0.7f;
		}

		public Vec2 Target
		{
			get { return _target; }
			set { _target = value; }
		}

		public float MaxForce
		{
			get { return _maxForce; }
			set { _maxForce = value; }
		}

		public float FrequencyHz
		{
			get { return _frequencyHz; }
			set { _frequencyHz = value; }
		}

		public float DampingRatio
		{
			get { return _dampingRatio; }
			set { _dampingRatio = value; }
		}
	}

	public sealed class MouseJoint : Joint
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2mousejoint_gettarget(IntPtr joint, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2mousejoint_settarget(IntPtr joint, Vec2 val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2mousejoint_getmaxforce(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2mousejoint_setmaxforce(IntPtr joint, float val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2mousejoint_getfrequency(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2mousejoint_setfrequency(IntPtr joint, float val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2mousejoint_getdampingratio(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2mousejoint_setdampingratio(IntPtr joint, float val);
		}

		public MouseJoint(IntPtr ptr) :
			base(ptr)
		{
		}

		public Vec2 Target
		{
			get { Vec2 temp; NativeMethods.b2mousejoint_gettarget(JointPtr, out temp); return temp; }
			set { NativeMethods.b2mousejoint_settarget(JointPtr, value); }
		}

		public float MaxForce
		{
			get { return NativeMethods.b2mousejoint_getmaxforce(JointPtr); }
			set { NativeMethods.b2mousejoint_setmaxforce(JointPtr, value); }
		}

		public float Frequency
		{
			get { return NativeMethods.b2mousejoint_getfrequency(JointPtr); }
			set { NativeMethods.b2mousejoint_setfrequency(JointPtr, value); }
		}

		public float DampingRatio
		{
			get { return NativeMethods.b2mousejoint_getdampingratio(JointPtr); }
			set { NativeMethods.b2mousejoint_setdampingratio(JointPtr, value); }
		}
	}
}
