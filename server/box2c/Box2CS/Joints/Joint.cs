using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Box2CS
{
	public enum JointType
	{
		Unknown,
		Revolute,
		Prismatic,
		Distance,
		Pulley,
		Mouse,
		Gear,
		Line,
		Weld,
		Friction,
	};

	[StructLayout(LayoutKind.Sequential)]
	public abstract class JointDef : IFixedSize
	{
		JointType _type;
		IntPtr _userData;
		IntPtr _bodyA;
		IntPtr _bodyB;
		[MarshalAs(UnmanagedType.U1)]
		bool _collideConnected;

		int IFixedSize.FixedSize()
		{
			return Marshal.SizeOf(GetType());
		}

		void IFixedSize.Lock() { }
		void IFixedSize.Unlock() { }

		public JointDef()
		{
			if (GetType().StructLayoutAttribute.Value != LayoutKind.Sequential)
				throw new TypeLoadException("Derived joint type \""+ GetType().Name+"\" does not have StructLayout set to sequential.\n\nDerived joint defs MUST use StructLayout(LayoutKind.Sequential).");

			_type = JointType.Unknown;
			_userData = IntPtr.Zero;
			_bodyA = IntPtr.Zero;
			_bodyB = IntPtr.Zero;
			_collideConnected = false;
		}

		[Category("Main")]
		public JointType JointType
		{
			get { return _type; }
			protected set { _type = value; }
		}

		[Category("Main")]
		public object UserData
		{
			get
			{
				return UserDataStorage.JointStorage.ObjectFromHandle(UserDataStorage.IntPtrToHandle(_userData));
			}

			set
			{
				var ptr = UserDataStorage.IntPtrToHandle(_userData);

				if (ptr != 0)
					UserDataStorage.JointStorage.UnpinObject(ptr);

				if (value != null)
				{
					var handle = UserDataStorage.JointStorage.PinDataToHandle(value);
					_userData = UserDataStorage.HandleToIntPtr(handle);
				}
				else
					_userData = IntPtr.Zero;
			}
		}

		[Browsable(false)]
		public Body BodyA
		{
			get { return Body.FromPtr(_bodyA); }
			set { _bodyA = value.BodyPtr; }
		}

		[Browsable(false)]
		public Body BodyB
		{
			get { return Body.FromPtr(_bodyB); }
			set { _bodyB = value.BodyPtr; }
		}

		[Category("Main")]
		public bool CollideConnected
		{
			get { return _collideConnected; }
			set { _collideConnected = value; }
		}
	}

	public abstract class Joint
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern int b2joint_gettype(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2joint_getbodya(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2joint_getbodyb(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2joint_getanchora(IntPtr joint, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2joint_getanchorb(IntPtr joint, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2joint_getreactionforce(IntPtr joint, float inv_dt, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2joint_getreactiontorque(IntPtr joint, float inv_dt);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2joint_getnext(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2joint_getuserdata(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2joint_setuserdata(IntPtr joint, IntPtr data);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2joint_getisactive(IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern bool b2joint_getcollideconnected(IntPtr joint);
		}

		IntPtr _jointPtr;

		internal IntPtr JointPtr
		{
			get { return _jointPtr; }
		}

		internal Joint(IntPtr ptr)
		{
			_jointPtr = ptr;
		}

		internal static Joint FromPtr(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				throw new ArgumentNullException("Invalid joint ptr (locked world?)");

			switch ((JointType)NativeMethods.b2joint_gettype(ptr))
			{
			case JointType.Distance:
				return new DistanceJoint(ptr);
			case JointType.Friction:
				return new FrictionJoint(ptr);
			case JointType.Gear:
				return new GearJoint(ptr);
			case JointType.Line:
				return new LineJoint(ptr);
			case JointType.Mouse:
				return new MouseJoint(ptr);
			case JointType.Prismatic:
				return new PrismaticJoint(ptr);
			case JointType.Pulley:
				return new PulleyJoint(ptr);
			case JointType.Revolute:
				return new RevoluteJoint(ptr);
			case JointType.Weld:
				return new WeldJoint(ptr);
			}

			return null;
		}

		public Vec2 GetReactionForce(float invDt)
		{
			Vec2 temp;
			NativeMethods.b2joint_getreactionforce(_jointPtr, invDt, out temp);
			return temp;
		}

		public float GetReactionTorque(float invDt)
		{
			return NativeMethods.b2joint_getreactiontorque(_jointPtr, invDt);
		}

		public bool CollideConnected
		{
			get { return NativeMethods.b2joint_getcollideconnected(_jointPtr); }
		}

		public JointType JointType
		{
			get { return (JointType)NativeMethods.b2joint_gettype(_jointPtr); }
		}

		public Body BodyA
		{
			get { return Body.FromPtr(NativeMethods.b2joint_getbodya(_jointPtr)); }
		}

		public Body BodyB
		{
			get { return Body.FromPtr(NativeMethods.b2joint_getbodyb(_jointPtr)); }
		}

		public Vec2 AnchorA
		{
			get { Vec2 temp; NativeMethods.b2joint_getanchora(_jointPtr, out temp); return temp; }
		}

		public Vec2 AnchorB
		{
			get { Vec2 temp; NativeMethods.b2joint_getanchorb(_jointPtr, out temp); return temp; }
		}

		public Joint Next
		{
			get { return Joint.FromPtr(NativeMethods.b2joint_getnext(_jointPtr)); }
		}

		public bool IsActive
		{
			get { return NativeMethods.b2joint_getisactive(_jointPtr); }
		}

		public object UserData
		{
			get
			{
				return UserDataStorage.JointStorage.ObjectFromHandle(UserDataStorage.IntPtrToHandle(NativeMethods.b2joint_getuserdata(_jointPtr)));
			}

			set
			{
				var ptr = UserDataStorage.IntPtrToHandle(NativeMethods.b2joint_getuserdata(_jointPtr));

				if (ptr != 0)
					UserDataStorage.JointStorage.UnpinObject(ptr);

				if (value != null)
				{
					var handle = UserDataStorage.JointStorage.PinDataToHandle(value);
					NativeMethods.b2joint_setuserdata(_jointPtr, UserDataStorage.HandleToIntPtr(handle));
				}
				else
					NativeMethods.b2joint_setuserdata(_jointPtr, IntPtr.Zero);
			}
		}

		public static bool operator ==(Joint l, Joint r)
		{
			if ((object)l == null && (object)r == null)
				return true;
			else if ((object)l == null && (object)r != null ||
				(object)l != null && (object)r == null)
				return false;

			return l._jointPtr == r._jointPtr;
		}

		public static bool operator !=(Joint l, Joint r)
		{
			return !(l == r);
		}

		public override bool Equals(object obj)
		{
			if (obj is Joint)
				return (obj as Joint) == this;

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return _jointPtr.GetHashCode();
		}
	}
}
