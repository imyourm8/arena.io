using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class RevoluteJointDef : JointDef
	{
		Vec2 _localAnchorA;
		Vec2 _localAnchorB;
		float _referenceAngle;
		[MarshalAs(UnmanagedType.U1)]
		bool _enableLimit;
		float _lowerAngle;
		float _upperAngle;
		[MarshalAs(UnmanagedType.U1)]
		bool _enableMotor;
		float _motorSpeed;
		float _maxMotorTorque;

		public RevoluteJointDef()
		{
			JointType = JointType.Revolute;
			_localAnchorA = Vec2.Empty;
			_localAnchorB = Vec2.Empty;
			_referenceAngle = 0.0f;
			_lowerAngle = 0.0f;
			_upperAngle = 0.0f;
			_maxMotorTorque = 0.0f;
			_motorSpeed = 0.0f;
			_enableLimit = false;
			_enableMotor = false;
		}

		public void Initialize(Body bodyA, Body bodyB, Vec2 anchor)
		{
			BodyA = bodyA;
			BodyB = bodyB;
			_localAnchorA = bodyA.GetLocalPoint(anchor);
			_localAnchorB = bodyB.GetLocalPoint(anchor);
			_referenceAngle = bodyB.Angle - bodyA.Angle;
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

		public float LowerAngle
		{
			get { return _lowerAngle; }
			set { _lowerAngle = value; }
		}

		public float UpperAngle
		{
			get { return _upperAngle; }
			set { _upperAngle = value; }
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

		public float MaxMotorTorque
		{
			get { return _maxMotorTorque; }
			set { _maxMotorTorque = value; }
		}
	}

	public sealed class RevoluteJoint : Joint
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2revolutejoint_getjointangle(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2revolutejoint_getjointspeed(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2revolutejoint_getenablelimit(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2revolutejoint_setenablelimit(IntPtr joint, [MarshalAs(UnmanagedType.U1)] bool val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2revolutejoint_getlowerlimit(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2revolutejoint_getupperlimit(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2revolutejoint_setlimits(IntPtr joint, float lower, float upper);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2revolutejoint_getenablemotor(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2revolutejoint_setenablemotor(IntPtr joint, [MarshalAs(UnmanagedType.U1)] bool val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2revolutejoint_getmotorspeed(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2revolutejoint_setmotorspeed(IntPtr joint, float val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2revolutejoint_setmaxmotortorque(IntPtr joint, float val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2revolutejoint_getmotortorque(IntPtr joint);
		}

		public RevoluteJoint(IntPtr ptr) :
			base(ptr)
		{
		}

		public float JointAngle
		{
			get { return NativeMethods.b2revolutejoint_getjointangle(JointPtr); }
		}

		public float JointSpeed
		{
			get { return NativeMethods.b2revolutejoint_getjointspeed(JointPtr); }
		}

		public bool IsLimitEnabled
		{
			get { return NativeMethods.b2revolutejoint_getenablelimit(JointPtr); }
			set { NativeMethods.b2revolutejoint_setenablelimit(JointPtr, value); }
		}

		public void SetLimits(float lower, float upper)
		{
			if (LowerLimit != lower || UpperLimit != upper)
				NativeMethods.b2revolutejoint_setlimits(JointPtr, lower, upper);
		}

		public float LowerLimit
		{
			get { return NativeMethods.b2revolutejoint_getlowerlimit(JointPtr); }
			set { SetLimits(value, UpperLimit); }
		}

		public float UpperLimit
		{
			get { return NativeMethods.b2revolutejoint_getupperlimit(JointPtr); }
			set { SetLimits(LowerLimit, value); }
		}

		public bool IsMotorEnabled
		{
			get { return NativeMethods.b2revolutejoint_getenablemotor(JointPtr); }
			set { NativeMethods.b2revolutejoint_setenablemotor(JointPtr, value); }
		}

		public float MotorSpeed
		{
			get { return NativeMethods.b2revolutejoint_getmotorspeed(JointPtr); }
			set { NativeMethods.b2revolutejoint_setmotorspeed(JointPtr, value); }
		}

		public float MaxMotorTorque
		{
			set { NativeMethods.b2revolutejoint_setmaxmotortorque(JointPtr, value); }
		}

		public float MotorTorque
		{
			get { return NativeMethods.b2revolutejoint_getmotortorque(JointPtr); }
		}
	}
}
