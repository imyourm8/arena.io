using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class PrismaticJointDef : JointDef
	{
		Vec2 _localAnchorA;
		Vec2 _localAnchorB;
		Vec2 _localAxis1;
		float _referenceAngle;
		[MarshalAs(UnmanagedType.U1)]
		bool _enableLimit;
		float _lowerTranslation;
		float _upperTranslation;
		[MarshalAs(UnmanagedType.U1)]
		bool _enableMotor;
		float _maxMotorForce;
		float _motorSpeed;

		public PrismaticJointDef()
		{
			JointType = JointType.Prismatic;
			_localAnchorA = Vec2.Empty;
			_localAnchorB = Vec2.Empty;
			_localAxis1 = new Vec2(1.0f, 0.0f);
			_referenceAngle = 0.0f;
			_enableLimit = false;
			_lowerTranslation = 0.0f;
			_upperTranslation = 0.0f;
			_enableMotor = false;
			_maxMotorForce = 0.0f;
			_motorSpeed = 0.0f;
		}

		public void Initialize(Body bodyA, Body bodyB, Vec2 anchor, Vec2 axis)
		{
			BodyA = bodyA;
			BodyB = bodyB;
			_localAnchorA = bodyA.GetLocalPoint(anchor);
			_localAnchorB = bodyB.GetLocalPoint(anchor);
			_localAxis1 = bodyA.GetLocalVector(axis);
			_referenceAngle = bodyB.Angle - bodyA.Angle;
		}

		public Vec2 LocalAxis
		{
			get { return _localAxis1; }
			set { _localAxis1 = value; }
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

		public float ReferenceAngle
		{
			get { return _referenceAngle; }
			set { _referenceAngle = value; }
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

		public float MotorSpeed
		{
			get { return _motorSpeed; }
			set { _motorSpeed = value; }
		}

		public float MaxMotorForce
		{
			get { return _maxMotorForce; }
			set { _maxMotorForce = value; }
		}
	}

	public sealed class PrismaticJoint : Joint
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2prismaticjoint_getjointtranslation(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2prismaticjoint_getjointspeed(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2prismaticjoint_getenablelimit(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2prismaticjoint_setenablelimit(IntPtr joint, [MarshalAs(UnmanagedType.U1)] bool val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2prismaticjoint_getlowerlimit(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2prismaticjoint_getupperlimit(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2prismaticjoint_setlimits(IntPtr joint, float lower, float upper);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2prismaticjoint_getenablemotor(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2prismaticjoint_setenablemotor(IntPtr joint, [MarshalAs(UnmanagedType.U1)] bool val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2prismaticjoint_getmotorspeed(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2prismaticjoint_setmotorspeed(IntPtr joint, float val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2prismaticjoint_setmaxmotorforce(IntPtr joint, float val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2prismaticjoint_getmotorforce(IntPtr joint);
		}

		public PrismaticJoint(IntPtr ptr) :
			base(ptr)
		{
		}

		public float JointTranslation
		{
			get { return NativeMethods.b2prismaticjoint_getjointtranslation(JointPtr); }
		}

		public float JointSpeed
		{
			get { return NativeMethods.b2prismaticjoint_getjointspeed(JointPtr); }
		}

		public bool IsLimitEnabled
		{
			get { return NativeMethods.b2prismaticjoint_getenablelimit(JointPtr); }
			set { NativeMethods.b2prismaticjoint_setenablelimit(JointPtr, value); }
		}

		public void SetLimits(float lower, float upper)
		{
			LowerLimit = lower;
			UpperLimit = upper;
		}

		public float LowerLimit
		{
			get { return NativeMethods.b2prismaticjoint_getlowerlimit(JointPtr); }
			set { SetLimits(value, UpperLimit); }
		}

		public float UpperLimit
		{
			get { return NativeMethods.b2prismaticjoint_getupperlimit(JointPtr); }
			set { SetLimits(LowerLimit, value); }
		}

		public bool IsMotorEnabled
		{
			get { return NativeMethods.b2prismaticjoint_getenablemotor(JointPtr); }
			set { NativeMethods.b2prismaticjoint_setenablemotor(JointPtr, value); }
		}

		public float MotorSpeed
		{
			get { return NativeMethods.b2prismaticjoint_getmotorspeed(JointPtr); }
			set { NativeMethods.b2prismaticjoint_setmotorspeed(JointPtr, value); }
		}

		public float MaxMotorForce
		{
			set { NativeMethods.b2prismaticjoint_setmaxmotorforce(JointPtr, value); }
		}

		public float MotorForce
		{
			get { return NativeMethods.b2prismaticjoint_getmotorforce(JointPtr); }
		}
	}
}
