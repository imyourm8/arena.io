using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class FrictionJointDef : JointDef
	{
		Vec2 _localAnchorA;
		Vec2 _localAnchorB;
		float _maxForce;
		float _maxTorque;

		public FrictionJointDef()
		{
			JointType = JointType.Friction;
			_localAnchorA = Vec2.Empty;
			_localAnchorB = Vec2.Empty;
			_maxForce = 0.0f;
			_maxTorque = 0.0f;
		}

		public void Initialize(Body bodyA, Body bodyB, Vec2 anchor)
		{
			BodyA = bodyA;
			BodyB = bodyB;
			_localAnchorA = bodyA.GetLocalPoint(anchor);
			_localAnchorB = bodyB.GetLocalPoint(anchor);
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

		public float MaxForce
		{
			get { return _maxForce; }
			set { _maxForce = value; }
		}

		public float MaxTorque
		{
			get { return _maxTorque; }
			set { _maxTorque = value; }
		}
	}

	public sealed class FrictionJoint : Joint
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2frictionjoint_getmaxforce(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2frictionjoint_setmaxforce(IntPtr joint, float data);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2frictionjoint_getmaxtorque(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2frictionjoint_setmaxtorque(IntPtr joint, float data);
		}

		public FrictionJoint(IntPtr ptr) :
			base(ptr)
		{
		}

		public float MaxForce
		{
			get { return NativeMethods.b2frictionjoint_getmaxforce(JointPtr); }
			set { NativeMethods.b2frictionjoint_setmaxforce(JointPtr, value); }
		}

		public float MaxTorque
		{
			get { return NativeMethods.b2frictionjoint_getmaxtorque(JointPtr); }
			set { NativeMethods.b2frictionjoint_setmaxtorque(JointPtr, value); }
		}
	}
}
