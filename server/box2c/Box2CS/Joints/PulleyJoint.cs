using System;
using System.Runtime.InteropServices;

namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class PulleyJointDef : JointDef
	{
		const float b2_minPulleyLength = 2.0f;
		
		Vec2 _groundAnchorA;
		Vec2 _groundAnchorB;
		Vec2 _localAnchorA;
		Vec2 _localAnchorB;
		float _lengthA;
		float _maxLengthA;
		float _lengthB;
		float _maxLengthB;
		float _ratio;

		public PulleyJointDef()
		{
			JointType = JointType.Pulley;
			_groundAnchorA = new Vec2(-1.0f, 1.0f);
			_groundAnchorB = new Vec2(1.0f, 1.0f);
			_localAnchorA = new Vec2(-1.0f, 0.0f);
			_localAnchorB = new Vec2(1.0f, 0.0f);
			_lengthA = 0.0f;
			_maxLengthA = 0.0f;
			_lengthB = 0.0f;
			_maxLengthB = 0.0f;
			_ratio = 1.0f;
			CollideConnected = true;
		}

		/// Initialize the bodies, anchors, lengths, max lengths, and ratio using the world anchors.
		public void Initialize(Body bodyA, Body bodyB,
						Vec2 groundAnchorA, Vec2 groundAnchorB,
						Vec2 anchorA, Vec2 anchorB,
						float ratio)
		{
			BodyA = bodyA;
			BodyB = bodyB;
			_groundAnchorA = groundAnchorA;
			_groundAnchorB = groundAnchorB;
			_localAnchorA = bodyA.GetLocalPoint(anchorA);
			_localAnchorB = bodyB.GetLocalPoint(anchorB);
			Vec2 d1 = anchorA - groundAnchorA;
			_lengthA = d1.Length();
			Vec2 d2 = anchorB - groundAnchorB;
			_lengthB = d2.Length();
			_ratio = ratio;
			
			if (!(ratio > float.Epsilon))
				throw new ArgumentException("Ratio is too small");

			float C = _lengthA + ratio * _lengthB;
			_maxLengthA = C - ratio * b2_minPulleyLength;
			_maxLengthB = (C - b2_minPulleyLength) / ratio;
		}

		public Vec2 GroundAnchorA
		{
			get { return _groundAnchorA; }
			set { _groundAnchorA = value; }
		}

		public Vec2 GroundAnchorB
		{
			get { return _groundAnchorB; }
			set { _groundAnchorB = value; }
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

		public float LengthA
		{
			get { return _lengthA; }
			set { _lengthA = value; }
		}

		public float LengthB
		{
			get { return _lengthB; }
			set { _lengthB = value; }
		}

		public float MaxLengthA
		{
			get { return _maxLengthA; }
			set { _maxLengthA = value; }
		}

		public float MaxLengthB
		{
			get { return _maxLengthB; }
			set { _maxLengthB = value; }
		}

		public float Ratio
		{
			get { return _ratio; }
			set { _ratio = value; }
		}
	}

	public sealed class PulleyJoint : Joint
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2pulleyjoint_getlength1(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2pulleyjoint_getlength2(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2pulleyjoint_getratio(IntPtr joint);
		}

		public PulleyJoint(IntPtr ptr) :
			base(ptr)
		{
		}

		public float LengthA
		{
			get { return NativeMethods.b2pulleyjoint_getlength1(JointPtr); }
		}

		public float LengthB
		{
			get { return NativeMethods.b2pulleyjoint_getlength2(JointPtr); }
		}

		public float Ratio
		{
			get { return NativeMethods.b2pulleyjoint_getratio(JointPtr); }
		}
	}
}
