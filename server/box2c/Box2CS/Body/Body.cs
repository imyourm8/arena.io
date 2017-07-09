using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Box2CS
{
	public enum BodyType
	{
		Static = 0,
		Kinematic,
		Dynamic,
	};

	[StructLayout(LayoutKind.Sequential)]
	public sealed class BodyDef : IFixedSize, ICloneable
	{
		int IFixedSize.FixedSize()
		{
			return Marshal.SizeOf(typeof(BodyDef));
		}

		void IFixedSize.Lock()
		{
		}

		void IFixedSize.Unlock()
		{
		}

		[MarshalAs(UnmanagedType.I4)]
		BodyType _type;

		Vec2 _position;
		float _angle;
		Vec2 _linearVelocity;
		float _angularVelocity;
		float _linearDamping;
		float _angularDamping;

		[MarshalAs(UnmanagedType.U1)]
		bool _allowSleep;

		[MarshalAs(UnmanagedType.U1)]
		bool _awake;

		[MarshalAs(UnmanagedType.U1)]
		bool _fixedRotation;

		[MarshalAs(UnmanagedType.U1)]
		bool _bullet;

		[MarshalAs(UnmanagedType.U1)]
		bool _active;

		IntPtr _userData;
		float _inertiaScale;

		public BodyDef(
			BodyType bodyType,
			Vec2 position,
			float angle,
			Vec2 linearVelocity,
			float angularVelocity = 0.0f,
			float linearDamping = 0.0f,
			float angularDamping = 0.0f,
			bool bullet = false,
			bool active = true,
			bool fixedRotation = false,
			bool allowSleep = true,
			bool awake = true,
			float inertiaScale = 1.0f,
			object userData = null)
		{
			UserData = userData;
			_position = position;
			_angle = angle;
			_linearVelocity = linearVelocity;
			_angularVelocity = angularVelocity;
			_linearDamping = linearDamping;
			_angularDamping = angularDamping;
			_allowSleep = allowSleep;
			_awake = awake;
			_fixedRotation = fixedRotation;
			_bullet = bullet;
			_type = bodyType;
			_active = active;
			_inertiaScale = inertiaScale;
		}

		public BodyDef(
			BodyType bodyType,
			Vec2 position,
			float angle = 0,
			bool bullet = false,
			float angularVelocity = 0.0f,
			float linearDamping = 0.0f,
			float angularDamping = 0.0f,
			bool active = true,
			bool fixedRotation = false,
			bool allowSleep = true,
			bool awake = true,
			float inertiaScale = 1.0f,
			object userData = null) :
			this(bodyType, position, angle, Vec2.Empty, angularVelocity, linearDamping, angularDamping, bullet, active, fixedRotation, allowSleep,
			awake, inertiaScale, userData)
		{
		}

		public BodyDef(
			BodyType bodyType,
			Vec2 position,
			float angle,
			Vec2 linearVelocity,
			bool bullet = false,
			bool active = true,
			bool fixedRotation = false,
			bool allowSleep = true,
			bool awake = true,
			float inertiaScale = 1.0f,
			object userData = null) :
			this(bodyType, position, angle, linearVelocity, 0.0f, 0.0f, 0.0f, bullet, active, fixedRotation, allowSleep,
			awake, inertiaScale, userData)
		{
		}

		public BodyDef() :
			this(BodyType.Static, Vec2.Empty, 0.0f, Vec2.Empty, 0.0f, 0.0f, 0.0f, false, true, false, true, true, 1.0f, null)
		{
		}

		/// <summary>
		/// User-specific and application-specific data.
		/// </summary>
		public object UserData
		{
			get { return UserDataStorage.BodyStorage.ObjectFromHandle(UserDataStorage.IntPtrToHandle(_userData)); }

			set
			{
				var ptr = UserDataStorage.IntPtrToHandle(_userData);

				if (ptr != 0)
					UserDataStorage.BodyStorage.UnpinObject(ptr);

				if (value != null)
				{
					var handle = UserDataStorage.BodyStorage.PinDataToHandle(value);
					_userData = UserDataStorage.HandleToIntPtr(handle);
				}
				else
					_userData = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Get or set the position of the body in the world.
		/// </summary>
		public Vec2 Position
		{
			get { return _position; }
			set { _position = value; }
		}

		/// <summary>
		/// Get or set the angle of the body.
		/// </summary>
		public float Angle
		{
			get { return _angle; }
			set { _angle = value; }
		}

		/// <summary>
		/// Get or set the linear velocity of the body.
		/// </summary>
		public Vec2 LinearVelocity
		{
			get { return _linearVelocity; }
			set { _linearVelocity = value; }
		}

		/// <summary>
		/// Get or set the angular velocity of the body.
		/// </summary>
		public float AngularVelocity
		{
			get { return _angularVelocity; }
			set { _angularVelocity = value; }
		}

		/// <summary>
		/// Get or set this body's resistance to linear movement.
		/// </summary>
		public float LinearDamping
		{
			get { return _linearDamping; }
			set { _linearDamping = value; }
		}

		/// <summary>
		/// Get or set this body's resistance to angular movement.
		/// </summary>
		public float AngularDamping
		{
			get { return _angularDamping; }
			set { _angularDamping = value; }
		}

		/// <summary>
		/// Get or set whether this body is allowed to sleep or not.
		/// </summary>
		public bool AllowSleep
		{
			get { return _allowSleep; }
			set { _allowSleep = value; }
		}

		/// <summary>
		/// Get or set whether this body will begin awake or not.
		/// </summary>
		public bool Awake
		{
			get { return _awake; }
			set { _awake = value; }
		}

		/// <summary>
		/// Get or set whether this body can rotate or not.
		/// </summary>
		public bool FixedRotation
		{
			get { return _fixedRotation; }
			set { _fixedRotation = value; }
		}

		/// <summary>
		/// Get or set whether this body will use continuous collision detection or not.
		/// </summary>
		public bool Bullet
		{
			get { return _bullet; }
			set { _bullet = value; }
		}

		/// <summary>
		/// Get or set the type of this body.
		/// </summary>e
		public BodyType BodyType
		{
			get { return _type; }
			set { _type = value; }
		}

		/// <summary>
		/// Get or set whether this body will be active or not.
		/// </summary>
		public bool Active
		{
			get { return _active; }  
			set { _active = value; }
		}

		/// <summary>
		/// Get or set the inertia scale of the body.
		/// </summary>
		public float InertiaScale
		{
			get { return _inertiaScale; }
			set { _inertiaScale = value; }
		}

		/// <summary>
		/// Compute the mass for this body from an array of fixtures.
		/// </summary>
		/// <param name="fixtures">The array of fixtures.</param>
		/// <returns>The calculated mass data.</returns>
		public MassData ComputeMass(IEnumerable<FixtureDef> fixtures)
		{
			// Compute mass data from shapes. Each shape has its own density.
			float m_mass = 0.0f;
			float m_I = 0.0f;

			// Static and kinematic bodies have zero mass.
			if (BodyType != Box2CS.BodyType.Dynamic)
				return MassData.Empty;

			// Accumulate mass over all fixtures.
			Vec2 center = Vec2.Empty;
			
			foreach (var f in fixtures)
			{
				if (f.Density == 0.0f)
					continue;

				MassData massData;
				f.Shape.ComputeMass(out massData, f.Density);
				m_mass += massData.Mass;
				center += massData.Mass * massData.Center;
				m_I += massData.Inertia;
			}

			// Compute center of mass.
			if (m_mass > 0.0f)
				center *= (1.0f / m_mass);
			else
				// Force all dynamic bodies to have a positive mass.
				m_mass = 1.0f;

			if (m_I > 0.0f && !FixedRotation)
			{
				// Center the inertia about the center of mass.
				m_I -= m_mass * center.Dot(center);

				if (!(m_I > 0.0f))
					throw new Exception("Negative inertia!");
			}
			else
				m_I = 0.0f;

			return new MassData(m_mass, center, m_I);
		}

		public object Clone()
		{
			BodyDef def = new BodyDef(BodyType, Position, Angle, LinearVelocity, AngularVelocity, LinearDamping, AngularDamping, Bullet, Active, FixedRotation, AllowSleep, Awake, InertiaScale, UserData);
			return def;
		}
	}

	public sealed class Body
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2body_createfixture(IntPtr body, IntPtr fixtureDef);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2body_createfixturefromshape(IntPtr body, IntPtr shape, float density);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_destroyfixture(IntPtr body, IntPtr fixture);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_settransform(IntPtr body, Vec2 position, float angle);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_gettransform(IntPtr body, out Transform transform);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_getposition(IntPtr body, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2body_getangle(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_getworldcenter(IntPtr body, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_getlocalcenter(IntPtr body, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_setlinearvelocity(IntPtr body, Vec2 vec);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_getlinearvelocity(IntPtr body, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_setangularvelocity(IntPtr body, float vec);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2body_getangularvelocity(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_applyforce(IntPtr body, Vec2 force, Vec2 point);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_applytorque(IntPtr body, float torque);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_applylinearimpulse(IntPtr body, Vec2 force, Vec2 point);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_applyangularimpulse(IntPtr body, float torque);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2body_getmass(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2body_getinertia(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_getmassdata(IntPtr body, out MassData data);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_setmassdata(IntPtr body, out MassData data);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_resetmassdata(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_getworldpoint(IntPtr body, Vec2 localPoint, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_getworldvector(IntPtr body, Vec2 localPoint, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_getlocalpoint(IntPtr body, Vec2 localPoint, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_getlocalvector(IntPtr body, Vec2 localPoint, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_getlinearvelocityfromworldvector(IntPtr body, Vec2 localPoint, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_getlinearvelocityfromlocalvector(IntPtr body, Vec2 localPoint, out Vec2 outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_setlineardamping(IntPtr body, float vec);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2body_getlineardamping(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_setangulardamping(IntPtr body, float vec);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2body_getangulardamping(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_settype(IntPtr body, int vec);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern int b2body_gettype(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_setbullet(IntPtr body, [MarshalAs(UnmanagedType.U1)] bool vec);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2body_getbullet(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_setissleepingallowed(IntPtr body, [MarshalAs(UnmanagedType.U1)] bool vec);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2body_getissleepingallowed(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_setawake(IntPtr body, [MarshalAs(UnmanagedType.U1)] bool vec);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2body_getawake(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_setactive(IntPtr body, [MarshalAs(UnmanagedType.U1)] bool vec);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2body_getactive(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2body_setfixedrotation(IntPtr body, [MarshalAs(UnmanagedType.U1)] bool vec);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2body_getfixedrotation(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2body_getfixturelist(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2body_getjointlist(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2body_getcontactlist(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2body_getnext(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2body_getuserdata(IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2body_setuserdata(IntPtr body, IntPtr data);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2body_getworld(IntPtr body);
		}

		IntPtr _bodyPtr;

		internal Body(IntPtr ptr)
		{
			_bodyPtr = ptr;
		}

		internal IntPtr BodyPtr
		{
			get { return _bodyPtr; }
		}

		internal static Body FromPtr(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return null;

			return new Body(ptr);
		}

		/// <summary>
		/// Get the transform of this body.
		/// </summary>
		public Transform Transform
		{
			get { Transform temp; NativeMethods.b2body_gettransform(_bodyPtr, out temp); return temp; }
		}

		/// <summary>
		/// Get or set the position of this body.
		/// </summary>
		public Vec2 Position
		{
			get { Vec2 temp; NativeMethods.b2body_getposition(_bodyPtr, out temp); return temp; }
			set { SetTransform(value, Angle); }
		}

		/// <summary>
		/// Get or set the angle of this body.
		/// </summary>
		public float Angle
		{
			get { return NativeMethods.b2body_getangle(_bodyPtr); }
			set { SetTransform(Position, value); }
		}

		/// <summary>
		/// Get the center of the body in world coordinates.
		/// </summary>
		public Vec2 WorldCenter
		{
			get { Vec2 temp; NativeMethods.b2body_getworldcenter(_bodyPtr, out temp); return temp; }
		}

		/// <summary>
		/// Get the center of the body in local coordinates.
		/// </summary>
		public Vec2 LocalCenter
		{
			get { Vec2 temp; NativeMethods.b2body_getlocalcenter(_bodyPtr, out temp); return temp; }
		}

		/// <summary>
		/// Get or set the linear velocity of this body.
		/// </summary>
		public Vec2 LinearVelocity
		{
			get { Vec2 temp; NativeMethods.b2body_getlinearvelocity(_bodyPtr, out temp); return temp; }
			set { NativeMethods.b2body_setlinearvelocity(_bodyPtr, value); }
		}

		/// <summary>
		/// Get or set the angular velocity of this body.
		/// </summary>
		public float AngularVelocity
		{
			get { return NativeMethods.b2body_getangularvelocity(_bodyPtr); }
			set { NativeMethods.b2body_setangularvelocity(_bodyPtr, value); }
		}

		/// <summary>
		/// Get the next body in the world's body list.
		/// </summary>
		public Body Next
		{
			get { return Body.FromPtr(NativeMethods.b2body_getnext(_bodyPtr)); }
		}

		/// <summary>
		/// Get or set the mass of this body.
		/// </summary>
		public float Mass
		{
			get { return NativeMethods.b2body_getmass(_bodyPtr); }
			set { var x = MassData; MassData = new MassData(value, x.Value.Center, x.Value.Inertia); }
		}

		/// <summary>
		/// Get or set the rotational inertia of this body.
		/// </summary>
		public float Inertia
		{
			get { return NativeMethods.b2body_getinertia(_bodyPtr); }
			set { var x = MassData; MassData = new MassData(x.Value.Mass, x.Value.Center, value); }
		}

		/// <summary>
		/// Get or set the center of gravity of this body.
		/// </summary>
		public Vec2 CenterOfGravity
		{
			get { return MassData.Value.Center; }
			set { var x = MassData; MassData = new MassData(x.Value.Mass, value, x.Value.Inertia); }
		}

		/// <summary>
		/// Get the inverted mass of this body.
		/// </summary>
		public float InvMass
		{
			get { return 1.0f / Mass; }
		}

		/// <summary>
		/// Get the inverted inertia of this body.
		/// </summary>
		public float InvInertia
		{
			get { return 1.0f / Inertia; }
		}

		/// <summary>
		/// Get or set the MassData of this body.
		/// </summary>
		/// <remarks>This property is nullable. Setting the value to null will cause the body to reset its mass to default.</remarks>
		public MassData? MassData
		{
			get { MassData returnVal = new MassData(); NativeMethods.b2body_getmassdata(_bodyPtr, out returnVal); return returnVal; }
			set
			{
				if (value == null)
				{
					NativeMethods.b2body_resetmassdata(_bodyPtr);
					return;
				}

				MassData mb = value.Value;
				NativeMethods.b2body_setmassdata(_bodyPtr, out mb);
			}
		}

		/// <summary>
		/// Get or set the linear damping of this body.
		/// </summary>
		public float LinearDamping
		{
			get { return NativeMethods.b2body_getlineardamping(_bodyPtr); }
			set { NativeMethods.b2body_setlineardamping(_bodyPtr, value); }
		}

		/// <summary>
		/// Get or set the angular damping of this body.
		/// </summary>
		public float AngularDamping
		{
			get { return NativeMethods.b2body_getangulardamping(_bodyPtr); }
			set { NativeMethods.b2body_setangulardamping(_bodyPtr, value); }
		}

		/// <summary>
		/// Get or set the bodytype of this body.
		/// </summary>
		public BodyType BodyType
		{
			get { return (BodyType)NativeMethods.b2body_gettype(_bodyPtr); }
			set { NativeMethods.b2body_settype(_bodyPtr, (int)value); }
		}

		/// <summary>
		/// Get or set whether this body is a bullet (continuous collision).
		/// </summary>
		public bool IsBullet
		{
			get { return NativeMethods.b2body_getbullet(_bodyPtr); }
			set { NativeMethods.b2body_setbullet(_bodyPtr, value); }
		}

		/// <summary>
		/// Get or set whether this body is allowed to sleep.
		/// </summary>
		public bool IsSleepingAllowed
		{
			get { return NativeMethods.b2body_getissleepingallowed(_bodyPtr); }
			set { NativeMethods.b2body_setissleepingallowed(_bodyPtr, value); }
		}

		/// <summary>
		/// Get or set whether this body is currently awake.
		/// </summary>
		public bool IsAwake
		{
			get { return NativeMethods.b2body_getawake(_bodyPtr); }
			set { NativeMethods.b2body_setawake(_bodyPtr, value); }
		}

		/// <summary>
		/// Get or set whether this body is active or not.
		/// </summary>
		public bool IsActive
		{
			get { return NativeMethods.b2body_getactive(_bodyPtr); }
			set { NativeMethods.b2body_setactive(_bodyPtr, value); }
		}

		/// <summary>
		/// Get or set whether this body is allowed to rotate or not.
		/// </summary>
		public bool IsFixedRotation
		{
			get { return NativeMethods.b2body_getfixedrotation(_bodyPtr); }
			set { NativeMethods.b2body_setfixedrotation(_bodyPtr, value); }
		}

		/// <summary>
		/// Get the first fixture in the list of fixtures for this body.
		/// </summary>
		public Fixture FixtureList
		{
			get { return Fixture.FromPtr(NativeMethods.b2body_getfixturelist(_bodyPtr)); }
		}

		/// <summary>
		/// Get the enumerated list of fixtures.
		/// </summary>
		public IEnumerable<Fixture> Fixtures
		{
			get
			{
				for (Fixture? fixture = FixtureList; fixture != null; fixture = fixture.Value.Next)
					yield return fixture.Value;
			}
		}

		/// <summary>
		/// Get the first joint in the list of joint edges for this body.
		/// </summary>
		public JointEdge JointList
		{
			get { return JointEdge.FromPtr(NativeMethods.b2body_getjointlist(_bodyPtr)); }
		}

		/// <summary>
		/// Get the enumerated list of joints.
		/// </summary>
		public IEnumerable<JointEdge> Joints
		{
			get
			{
				for (var jointEdge = JointList; jointEdge != null; jointEdge = jointEdge.Next)
					yield return jointEdge;
			}
		}

		/// <summary>
		/// Get the first contact in the list of contact edges for this body.
		/// </summary>
		public ContactEdge ContactList
		{
			get { return ContactEdge.FromPtr(NativeMethods.b2body_getcontactlist(_bodyPtr)); }
		}

		/// <summary>
		/// Get the enumerated list of contacts.
		/// </summary>
		public IEnumerable<ContactEdge> Contacts
		{
			get
			{
				for (var contactEdge = ContactList; contactEdge != null; contactEdge = contactEdge.Next)
					yield return contactEdge;
			}
		}

		/// <summary>
		/// Application-specific and user-specific data for this body.
		/// </summary>
		public object UserData
		{
			get
			{
				return UserDataStorage.BodyStorage.ObjectFromHandle(UserDataStorage.IntPtrToHandle(NativeMethods.b2body_getuserdata(_bodyPtr)));
			}

			set
			{
				var ptr = UserDataStorage.IntPtrToHandle(NativeMethods.b2body_getuserdata(_bodyPtr));

				if (ptr != 0)
					UserDataStorage.BodyStorage.UnpinObject(ptr);

				if (value != null)
				{
					var handle = UserDataStorage.BodyStorage.PinDataToHandle(value);
					NativeMethods.b2body_setuserdata(_bodyPtr, UserDataStorage.HandleToIntPtr(handle));
				}
				else
					NativeMethods.b2body_setuserdata(_bodyPtr, IntPtr.Zero);
			}
		}

		/// <summary>
		/// Get the world this body is a part of.
		/// </summary>
		public World World
		{
			get { return World.FromPtr(NativeMethods.b2body_getworld(_bodyPtr)); }
		}

		/// <summary>
		/// Attach a fixture to this shape.
		/// </summary>
		/// <param name="def">The fixture definition to add.</param>
		/// <returns>The created Fixture object.</returns>
		public Fixture CreateFixture(FixtureDef def)
		{
			def.SetShape(def.Shape.Lock());
			Fixture fixture;
			using (var structPtr = new StructToPtrMarshaller(def.Internal))
				fixture = Fixture.FromPtr(NativeMethods.b2body_createfixture(_bodyPtr, structPtr.Pointer));
			def.Shape.Unlock();
			return fixture;
		}

		/// <summary>
		/// Attach a fixture to this shape.
		/// </summary>
		/// <param name="shape">The shape to add.</param>
		/// <param name="density">The density of the fixture.</param>
		/// <returns>The created Fixture object.</returns>
		public Fixture CreateFixture(Shape shape, float density)
		{
			var ptr = shape.Lock();
			var fix = Fixture.FromPtr(NativeMethods.b2body_createfixturefromshape(_bodyPtr, ptr, density));
			shape.Unlock();
			return fix;
		}

		/// <summary>
		/// Destroy a Fixture object attached to this body.
		/// </summary>
		/// <param name="fixture">The fixture to destroy.</param>
		public void DestroyFixture(Fixture fixture)
		{
			NativeMethods.b2body_destroyfixture(_bodyPtr, fixture.FixturePtr);
		}

		/// <summary>
		/// Set the transform of this body.
		/// </summary>
		/// <param name="position">The new position of the body.</param>
		/// <param name="angle">The new angle of the body</param>
		public void SetTransform(Vec2 position, float angle)
		{
			NativeMethods.b2body_settransform(_bodyPtr, position, angle);
		}

		/// <summary>
		/// Apply a force to this body.
		/// </summary>
		/// <param name="force">The force direction/magnitude to apply.</param>
		/// <param name="point">The local point the force originates from.</param>
		public void ApplyForce(Vec2 force, Vec2 point)
		{
			NativeMethods.b2body_applyforce(_bodyPtr, force, point);
		}

		/// <summary>
		/// Apply torque to this body.
		/// </summary>
		/// <param name="torque">The amount of torque to apply.</param>
		public void ApplyTorque(float torque)
		{
			NativeMethods.b2body_applytorque(_bodyPtr, torque);
		}

		/// <summary>
		/// Apply a linear impulse to this body.
		/// </summary>
		/// <param name="force">The force direction/magnitude to apply.</param>
		/// <param name="point">The local point the force originates from.</param>
		public void ApplyLinearImpulse(Vec2 force, Vec2 point)
		{
			NativeMethods.b2body_applylinearimpulse(_bodyPtr, force, point);
		}

		/// <summary>
		/// Apply an angular impulse to this body.
		/// </summary>
		/// <param name="impulse">The impulse to apply.</param>
		public void ApplyAngularImpulse(float impulse)
		{
			NativeMethods.b2body_applyangularimpulse(_bodyPtr, impulse);
		}

		/// <summary>
		/// Get a world point from a local point on the body.
		/// </summary>
		/// <param name="localPoint">The local point.</param>
		/// <returns>The point in world coordinates.</returns>
		public Vec2 GetWorldPoint(Vec2 localPoint)
		{
			Vec2 temp;
			NativeMethods.b2body_getworldpoint(_bodyPtr, localPoint, out temp);
			return temp;
		}

		/// <summary>
		/// Get a world vector from a local point.
		/// </summary>
		/// <param name="localPoint">The local point.</param>
		/// <returns>The vector in world coordinates.</returns>
		public Vec2 GetWorldVector(Vec2 localPoint)
		{
			Vec2 temp;
			NativeMethods.b2body_getworldvector(_bodyPtr, localPoint, out temp);
			return temp;
		}

		/// <summary>
		/// Get a local point from a world point.
		/// </summary>
		/// <param name="worldPoint">The world point.</param>
		/// <returns>The point in local coordinates.</returns>
		public Vec2 GetLocalPoint(Vec2 worldPoint)
		{
			Vec2 temp;
			NativeMethods.b2body_getlocalpoint(_bodyPtr, worldPoint, out temp);
			return temp;
		}

		/// <summary>
		/// Get a local vector from a world point.
		/// </summary>
		/// <param name="worldPoint">The world point.</param>
		/// <returns>The vector in local coordinates.</returns>
		public Vec2 GetLocalVector(Vec2 worldPoint)
		{
			Vec2 temp;
			NativeMethods.b2body_getlocalvector(_bodyPtr, worldPoint, out temp);
			return temp;
		}

		/// <summary>
		/// Get linear velocity from a world vector.
		/// </summary>
		/// <param name="worldVector">The world vector</param>
		/// <returns>The linear velocity.</returns>
		public Vec2 GetLinearVelocityFromWorldVector(Vec2 worldVector)
		{
			Vec2 temp;
			NativeMethods.b2body_getlinearvelocityfromworldvector(_bodyPtr, worldVector, out temp);
			return temp;
		}

		/// <summary>
		/// Get linear velocity from a local vector.
		/// </summary>
		/// <param name="localVector">The local vector.</param>
		/// <returns>The linear velocity.</returns>
		public Vec2 GetLinearVelocityFromLocalVector(Vec2 localVector)
		{
			Vec2 temp;
			NativeMethods.b2body_getlinearvelocityfromlocalvector(_bodyPtr, localVector, out temp);
			return temp;
		}

		public static bool operator ==(Body l, Body r)
		{
			if ((object)l == null && (object)r == null)
				return true;
			else if ((object)l == null && (object)r != null ||
				(object)l != null && (object)r == null)
				return false;

			return l.BodyPtr == r.BodyPtr;
		}

		public static bool operator !=(Body l, Body r)
		{
			return !(l == r);
		}

		public override bool Equals(object obj)
		{
			if (obj is Body)
				return (obj as Body) == this;

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return BodyPtr.GetHashCode();
		}
	}
}
