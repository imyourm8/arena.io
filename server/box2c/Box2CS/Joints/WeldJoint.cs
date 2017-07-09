using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class WeldJointDef : JointDef
	{
		Vec2 _localAnchorA;
		Vec2 _localAnchorB;
		float _referenceAngle;

		public WeldJointDef()
		{
			JointType = JointType.Weld;
			_localAnchorA = Vec2.Empty;
			_localAnchorB = Vec2.Empty;
			_referenceAngle = 0.0f;
		}

		/// Initialize the bodies, anchors, and reference angle using a world
		/// anchor point.
		public void Initialize(Body body1, Body body2, Vec2 anchor)
		{
			BodyA = body1;
			BodyB = body2;
			_localAnchorA = BodyA.GetLocalPoint(anchor);
			_localAnchorB = BodyB.GetLocalPoint(anchor);
			_referenceAngle = BodyB.Angle - BodyA.Angle;
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
	};

	public sealed class WeldJoint : Joint
	{
		public WeldJoint(IntPtr ptr) :
			base(ptr)
		{
		}
	}
}
