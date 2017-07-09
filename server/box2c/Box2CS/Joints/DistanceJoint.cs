using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class DistanceJointDef : JointDef
	{
		Vec2 _localAnchorA;
		Vec2 _localAnchorB;
		float _length;
		float _frequencyHz;
		float _dampingRatio;

		public DistanceJointDef()
		{
			JointType = JointType.Distance;
			_localAnchorA = Vec2.Empty;
			_localAnchorB = Vec2.Empty;
			_length = 1.0f;
			_frequencyHz = 0.0f;
			_dampingRatio = 0.0f;
		}

		/// Initialize the bodies, anchors, and length using the world
		/// anchors.
		public void Initialize(Body bodyA, Body bodyB,
						Vec2 anchorA, Vec2 anchorB)
		{
			BodyA = bodyA;
			BodyB = bodyB;
			_localAnchorA = bodyA.GetLocalPoint(anchorA);
			_localAnchorB = bodyB.GetLocalPoint(anchorB);
			Vec2 d = anchorB - anchorA;
			_length = d.Length();
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

		public float Length
		{
			get { return _length; }
			set { _length = value; }
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

	public sealed class DistanceJoint : Joint
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2distancejoint_setlength(IntPtr joint, float data);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2distancejoint_getlength(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2distancejoint_setfrequency(IntPtr joint, float data);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2distancejoint_getfrequency(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2distancejoint_setdampingratio(IntPtr joint, float data);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2distancejoint_getdampingratio(IntPtr joint);
		}

		public DistanceJoint(IntPtr ptr) :
			base(ptr)
		{
		}

		public float Length
		{
			get { return NativeMethods.b2distancejoint_getlength(JointPtr); }
			set { NativeMethods.b2distancejoint_setlength(JointPtr, value); }
		}

		public float Frequency
		{
			get { return NativeMethods.b2distancejoint_getfrequency(JointPtr); }
			set { NativeMethods.b2distancejoint_setfrequency(JointPtr, value); }
		}

		public float DampingRatio
		{
			get { return NativeMethods.b2distancejoint_getdampingratio(JointPtr); }
			set { NativeMethods.b2distancejoint_setdampingratio(JointPtr, value); }
		}
	}
}
