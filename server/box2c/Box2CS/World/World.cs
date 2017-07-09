using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Box2CS
{
	public sealed class World : IDisposable
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2world_constructor(Vec2 gravity, [MarshalAs(UnmanagedType.U1)] bool doSleep);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_destroy(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_setautoclearforces(IntPtr world, [MarshalAs(UnmanagedType.U1)] bool flag);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2world_getautoclearforces(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2world_islocked(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_setgravity(IntPtr world, Vec2 gravity);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern Vec2 b2world_getgravity(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern int b2world_getbodycount(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern int b2world_getcontactcount(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern int b2world_getproxycount(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern int b2world_getjointcount(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2world_getbodylist(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2world_getjointlist(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2world_getcontactlist(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_setcontinuousphysics(IntPtr world, [MarshalAs(UnmanagedType.U1)] bool flag);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_setwarmstarting(IntPtr world, [MarshalAs(UnmanagedType.U1)] bool flag);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_step(IntPtr world, float hz, int velocityIterations, int positionIteations);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_setdebugdraw(IntPtr world, IntPtr listener);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_drawdebugdata(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_clearforces(IntPtr world);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_setcontactlistener(IntPtr world, IntPtr listener);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_setcontactfilter(IntPtr world, IntPtr listener);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_setdestructionlistener(IntPtr world, IntPtr listener);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2world_createbody(IntPtr world, IntPtr bodyDef);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2world_destroybody(IntPtr world, IntPtr body);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2world_createjoint(IntPtr world, IntPtr jointDef);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2world_destroyjoint(IntPtr world, IntPtr joint);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_queryaabb(IntPtr world, IntPtr callback, AABB aabb);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2world_raycast(IntPtr world, IntPtr callback, Vec2 point1, Vec2 point2);
		}

		IntPtr _worldPtr;

		public static World FromPtr(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return null;

			return new World(ptr);
		}

		internal World(IntPtr ptr)
		{
			_worldPtr = ptr;
			shouldDispose = false;
		}

		public World(Vec2 gravity, bool doSleep)
		{
			_worldPtr = NativeMethods.b2world_constructor(gravity, doSleep);
			shouldDispose = true;
		}

		public void Step(float hz, int velocityIterations, int positionIterations)
		{
			NativeMethods.b2world_step(_worldPtr, hz, velocityIterations, positionIterations);
		}

		public void ClearForces()
		{
			NativeMethods.b2world_clearforces(_worldPtr);
		}

		public Body CreateBody(BodyDef def)
		{
			using (var structPtr = new StructToPtrMarshaller(def))
				return Body.FromPtr(NativeMethods.b2world_createbody(_worldPtr, structPtr.Pointer));
		}

		public void DestroyBody(Body body)
		{
			NativeMethods.b2world_destroybody(_worldPtr, body.BodyPtr);
		}

		public Joint CreateJoint(JointDef def)
		{
			using (var ptr = new StructToPtrMarshaller(def))
				return Joint.FromPtr(NativeMethods.b2world_createjoint(_worldPtr, ptr.Pointer));
		}

		public void DestroyJoint(Joint joint)
		{
			NativeMethods.b2world_destroyjoint(_worldPtr, joint.JointPtr);
		}

		public bool AutoClearForces
		{
			get
			{
				return NativeMethods.b2world_getautoclearforces(_worldPtr);
			}

			set
			{
				NativeMethods.b2world_setautoclearforces(_worldPtr, value);
			}
		}

		DebugDraw _debugDraw;
		public DebugDraw DebugDraw
		{
			set
			{
				_debugDraw = value;
				NativeMethods.b2world_setdebugdraw(_worldPtr, value.Listener);
			}

			get
			{
				return _debugDraw;
			}
		}

		public void DrawDebugData()
		{
			NativeMethods.b2world_drawdebugdata(_worldPtr);
		}

		ContactFilter _filter;
		public ContactFilter ContactFilter
		{
			set
			{
				_filter = value;
				NativeMethods.b2world_setcontactfilter(_worldPtr, value.Listener);
			}

			get
			{
				return _filter;
			}

		}

		ContactListener _contactListener;
		public ContactListener ContactListener
		{
			set
			{
				_contactListener = value;
				NativeMethods.b2world_setcontactlistener(_worldPtr, value.Listener);
			}

			get
			{
				return _contactListener;
			}
		}

		DestructionListener _destructionListener;
		public DestructionListener DestructionListener
		{
			set
			{
				_destructionListener = value;
				NativeMethods.b2world_setdestructionlistener(_worldPtr, value.Listener);
			}

			get
			{
				return _destructionListener;
			}
		}

		public Body BodyList
		{
			get
			{
				return Body.FromPtr(NativeMethods.b2world_getbodylist(_worldPtr));
			}
		}

		public IEnumerable<Body> Bodies
		{
			get
			{
				for (var body = BodyList; body != null; body = body.Next)
					yield return body;
			}
		}

		public Joint JointList
		{
			get
			{
				return Joint.FromPtr(NativeMethods.b2world_getjointlist(_worldPtr));
			}
		}

		public IEnumerable<Joint> Joints
		{
			get
			{
				for (var joint = JointList; joint != null; joint = joint.Next)
					yield return joint;
			}
		}

		public Contact ContactList
		{
			get
			{
				return Contact.FromPtr(NativeMethods.b2world_getcontactlist(_worldPtr));
			}
		}

		public IEnumerable<Contact> Contacts
		{
			get
			{
				for (var contact = ContactList; contact != null; contact = contact.Next)
					yield return contact;
			}
		}

		public bool ContinuousPhysics
		{
			set
			{
				NativeMethods.b2world_setcontinuousphysics(_worldPtr, value);
			}
		}

		public bool WarmStarting
		{
			set
			{
				NativeMethods.b2world_setwarmstarting(_worldPtr, value);
			}
		}

		public bool IsLocked
		{
			get
			{
				return NativeMethods.b2world_islocked(_worldPtr);
			}
		}

		public Vec2 Gravity
		{
			get
			{
				return NativeMethods.b2world_getgravity(_worldPtr);
			}

			set
			{
				NativeMethods.b2world_setgravity(_worldPtr, value);
			}
		}

		public int BodyCount
		{
			get
			{
				return NativeMethods.b2world_getbodycount(_worldPtr);
			}
		}

		public int ContactCount
		{
			get
			{
				return NativeMethods.b2world_getcontactcount(_worldPtr);
			}
		}

		public int JointCount
		{
			get
			{
				return NativeMethods.b2world_getjointcount(_worldPtr);
			}
		}

		public int ProxyCount
		{
			get
			{
				return NativeMethods.b2world_getproxycount(_worldPtr);
			}
		}

		public delegate bool QueryCallbackDelegate(Fixture fixture);

		public void QueryAABB(QueryCallback callback, AABB aabb)
		{
			NativeMethods.b2world_queryaabb(_worldPtr, callback.Listener, aabb);
		}

		class QueryCallbackDelegateWrapper : QueryCallback
		{
			QueryCallbackDelegate _delegate;

			public QueryCallbackDelegateWrapper(QueryCallbackDelegate deleg)
			{
				_delegate = deleg;
			}

			public override bool ReportFixture(Fixture fixture)
			{
				return _delegate(fixture);
			}
		}

		public void QueryAABB(QueryCallbackDelegate callback, AABB aabb)
		{
			using (QueryCallbackDelegateWrapper wrapper = new QueryCallbackDelegateWrapper(callback))
				QueryAABB(wrapper, aabb);
		}

		public delegate float RayCastCallbackDelegate(Fixture fixture, Vec2 point,
									Vec2 normal, float fraction);
		
		class RayCastCallbackDelegateWrapper : RayCastCallback
		{
			RayCastCallbackDelegate _delegate;

			public RayCastCallbackDelegateWrapper(RayCastCallbackDelegate deleg)
			{
				_delegate = deleg;
			}

			public override float  ReportFixture(Fixture fixture, Vec2 point, Vec2 normal, float fraction)
			{
 				return _delegate(fixture, point, normal, fraction);
			}
		}

		public void RayCast(RayCastCallbackDelegate callback, Vec2 point1, Vec2 point2)
		{
			using (RayCastCallbackDelegateWrapper wrapper = new RayCastCallbackDelegateWrapper(callback))
				RayCast(wrapper, point1, point2);
		}
		
		public void RayCast(RayCastCallback callback, Vec2 point1, Vec2 point2)
		{
			NativeMethods.b2world_raycast(_worldPtr, callback.Listener, point1, point2);
		}

		#region IDisposable
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool disposed, shouldDispose;

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
				}

				if (shouldDispose)
					NativeMethods.b2world_destroy(_worldPtr);

				disposed = true;
			}
		}

		~World()
		{
			Dispose(false);
		}
		#endregion
	}
}
