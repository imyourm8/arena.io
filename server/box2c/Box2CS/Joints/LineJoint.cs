using System;
using System.Runtime.InteropServices;
namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class LineJointDef : JointDef
	{
		Vec2 _localAnchorA;
		Vec2 _localAnchorB;
		Vec2 _localAxisA;
		[MarshalAs(UnmanagedType.U1)]
		bool _enableLimit;
		float _lowerTranslation;
		float _upperTranslation;
		[MarshalAs(UnmanagedType.U1)]
		bool _enableMotor;
		float _maxMotorForce;
		float _motorSpeed;

		public LineJointDef()
		{
			JointType = JointType.Line;
			_localAnchorA = Vec2.Empty;
			_localAnchorB = Vec2.Empty;
			_localAxisA = new Vec2(1.0f, 0.0f);
			_enableLimit = false;
			_lowerTranslation = 0.0f;
			_upperTranslation = 0.0f;
			_enableMotor = false;
			_maxMotorForce = 0.0f;
			_motorSpeed = 0.0f;
		}

		/// Initialize the bodies, anchors, axis, and reference angle using the world
		/// anchor and world axis.
		public void Initialize(Body bodyA, Body bodyB, Vec2 anchor, Vec2 axis)
		{
			BodyA = bodyA;
			BodyB = bodyB;
			_localAnchorA = bodyA.GetLocalPoint(anchor);
			_localAnchorB = bodyB.GetLocalPoint(anchor);
			_localAxisA = bodyA.GetLocalVector(axis);
		}

		public Vec2 LocalAnchorA
		{
			get { return _localAnchorA; }
			set { _localAnchorA = value; }
		}

		public Vec2 LocalAnchorB
		{
			get { return _localAnchorB; }
			set { _localAnchorB = value; }
		}

		public Vec2 LocalAxisA
		{
			get { return _localAxisA; }
			set { _localAxisA = value; }
		}

		public bool EnableLimit
		{
			get { return _enableLimit; }
			set { _enableLimit = value; }
		}

		public float LowerTranslation
		{
			get { return _lowerTranslation; }
			set { _lowerTranslation = value; }
		}

		public float UpperTranslation
		{
			get { return _upperTranslation; }
			set { _upperTranslation = value; }
		}

		public bool EnableMotor
		{
			get { return _enableMotor; }
			set { _enableMotor = value; }
		}

		public float MaxMotorForce
		{
			get { return _maxMotorForce; }
			set { _maxMotorForce = value; }
		}

		public float MotorSpeed
		{
			get { return _motorSpeed; }
			set { _motorSpeed = value; }
		}
	}

	public sealed class LineJoint : Joint
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2linejoint_getjointtranslation(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2linejoint_getjointspeed(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2linejoint_getenablelimit(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2linejoint_setenablelimit(IntPtr joint, [MarshalAs(UnmanagedType.U1)] bool val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2linejoint_getlowerlimit(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2linejoint_getupperlimit(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2linejoint_setlimits(IntPtr joint, float lower, float upper);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2linejoint_getenablemotor(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2linejoint_setenablemotor(IntPtr joint, [MarshalAs(UnmanagedType.U1)] bool val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2linejoint_getmotorspeed(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2linejoint_setmotorspeed(IntPtr joint, float val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2linejoint_setmaxmotorforce(IntPtr joint, float val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2linejoint_getmotorforce(IntPtr joint);
		}

		public LineJoint(IntPtr ptr) :
			base(ptr)
		{
		}

		public float JointTranslation
		{
			get { return NativeMethods.b2linejoint_getjointtranslation(JointPtr); }
		}

		public float JointSpeed
		{
			get { return NativeMethods.b2linejoint_getjointspeed(JointPtr); }
		}

		public bool IsLimitEnabled
		{
			get { return NativeMethods.b2linejoint_getenablelimit(JointPtr); }
			set { NativeMethods.b2linejoint_setenablelimit(JointPtr, value); }
		}

		public void SetLimits(float lower, float upper)
		{
			LowerLimit = lower;
			UpperLimit = upper;
		}

		public float LowerLimit
		{
			get { return NativeMethods.b2linejoint_getlowerlimit(JointPtr); }
			set { SetLimits(value, UpperLimit); }
		}

		public float UpperLimit
		{
			get { return NativeMethods.b2linejoint_getupperlimit(JointPtr); }
			set { SetLimits(LowerLimit, value); }
		}

		public bool IsMotorEnabled
		{
			get { return NativeMethods.b2linejoint_getenablemotor(JointPtr); }
			set { NativeMethods.b2linejoint_setenablemotor(JointPtr, value); }
		}

		public float MotorSpeed
		{
			get { return NativeMethods.b2linejoint_getmotorspeed(JointPtr); }
			set { NativeMethods.b2linejoint_setmotorspeed(JointPtr, value); }
		}

		public float MaxMotorForce
		{
			set { NativeMethods.b2linejoint_setmaxmotorforce(JointPtr, value); }
		}

		public float MotorForce
		{
			get { return NativeMethods.b2linejoint_getmotorforce(JointPtr); }
		}
	}
}
