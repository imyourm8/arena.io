using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Reflection;

namespace Box2CS
{
	[StructLayout(LayoutKind.Sequential)]
	public class FilterData
	{
		public const ushort DefaultCategoryBits = 0x0001;
		public const ushort DefaultMaskBits = 0xFFFF;

		ushort _categoryBits = DefaultCategoryBits, 
			_maskBits = DefaultMaskBits;
		short _groupIndex = 0;

		public static readonly FilterData Default = new FilterData();

		/// <summary>
		/// The collision category bits. Normally you would just set one bit.
		/// </summary>
		public ushort CategoryBits
		{
			get { return _categoryBits; }
			set { _categoryBits = value; }
		}

		/// <summary>
		/// The collision mask bits. This states the categories that this shape would accept for collision.
		/// </summary>
		public ushort MaskBits
		{
			get { return _maskBits; }
			set { _maskBits = value; }
		}

		/// <summary>
		/// Collision groups allow a certain group of objects to never collide (negative) or always collide (positive).
		/// Zero means no collision group. Non-zero group filtering always wins against the mask bits.
		/// </summary>
		public short GroupIndex
		{
			get { return _groupIndex; }
			set { _groupIndex = value; }
		}

		/// <summary>
		/// Default constructor. Initializes this FilterData to FilterData.Default.
		/// </summary>
		public FilterData()
		{
		}

		/// <summary>
		/// Constructor. Initializes this FilterData with the data provided.
		/// </summary>
		/// <param name="categoryBits">The category bits.</param>
		/// <param name="maskBits">The mask bits.</param>
		/// <param name="groupIndex">The group index.</param>
		public FilterData(ushort categoryBits, ushort maskBits, short groupIndex)
		{
			CategoryBits = categoryBits;
			MaskBits = maskBits;
			GroupIndex = groupIndex;
		}

		public static bool operator ==(FilterData l, FilterData r)
		{
			if ((object)l == null && (object)r == null)
				return true;
			else if ((object)l == null && (object)r != null ||
				(object)l != null && (object)r == null)
				return false;

			return (l.CategoryBits == r.CategoryBits && l.MaskBits == r.MaskBits && l.GroupIndex == r.GroupIndex);
		}

		public static bool operator !=(FilterData l, FilterData r)
		{
			return !(l == r);
		}

		public override bool Equals(object obj)
		{
			if (obj is FilterData)
				return (obj as FilterData) == this;

			return base.Equals(obj);
		}

		/// <summary>
		/// Convert this FilterData to a human-readable format.
		/// </summary>
		/// <returns>The formatted string</returns>
		public override string ToString()
		{
			return "CategoryBits="+CategoryBits.ToString()+" MaskBits="+MaskBits.ToString()+" GroupIndex="+GroupIndex.ToString();
		}

		public override int GetHashCode()
		{
			return CategoryBits.GetHashCode() + MaskBits.GetHashCode() + GroupIndex.GetHashCode();
		}

		/// <summary>
		/// Parse a FilterData out from a string.
		/// </summary>
		/// <param name="value">The string to parse from.</param>
		/// <returns>The new FilterData.</returns>
		public static FilterData Parse(string value)
		{
			SimpleParser parser = new SimpleParser(value, true);
			return new FilterData(ushort.Parse(parser.ValueFromKey("CategoryBits")), ushort.Parse(parser.ValueFromKey("MaskBits")), short.Parse(parser.ValueFromKey("GroupIndex")));
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MassData
	{
		public readonly static MassData Empty = new MassData(0, Vec2.Empty, 0);

		/// <summary>
		/// The mass of the body.
		/// </summary>
		public float Mass
		{
			get;
			set;
		}

		/// <summary>
		/// The center of gravity for the body.
		/// </summary>
		public Vec2 Center
		{
			get;
			set;
		}

		/// <summary>
		/// Rotational inertia for the body
		/// </summary>
		public float Inertia
		{
			get;
			set;
		}

		/// <summary>
		/// Initialize a MassData structure from the data provided.
		/// </summary>
		/// <param name="mass">Body mass</param>
		/// <param name="center">Body center of gravity</param>
		/// <param name="inertia">Body inertia</param>
		public MassData(float mass, Vec2 center, float inertia) :
			this()
		{
			Mass = mass;
			Center = center;
			Inertia = inertia;
		}

		public static bool operator== (MassData left, MassData right)
		{
			return (left.Mass == right.Mass && left.Center == right.Center && left.Inertia == right.Inertia);
		}

		public static bool operator!=(MassData left, MassData right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (obj is MassData)
				return this == (MassData)obj;

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Mass.GetHashCode() + Center.GetHashCode() + Inertia.GetHashCode();
		}

		/// <summary>
		/// Parse a MassData from a string.
		/// </summary>
		/// <param name="value">The string to parse.</param>
		/// <returns>The new MassData.</returns>
		public static MassData Parse(string value)
		{
			SimpleParser parser = new SimpleParser(value, true);
			return new MassData(float.Parse(parser.ValueFromKey("Mass")), Vec2.Parse(parser.ValueFromKey("Center")), float.Parse(parser.ValueFromKey("Inertia")));
		}

		/// <summary>
		/// Convert this MassData to a human-readable string.
		/// </summary>
		/// <returns>The formatted string</returns>
		public override string ToString()
		{
			return "Mass="+Mass.ToString()+" Center="+Center.ToString()+" Inertia="+Inertia.ToString();
		}
	};

	internal struct FixtureDefInternal : IFixedSize
	{
		int IFixedSize.FixedSize()
		{
			return Marshal.SizeOf(typeof(FixtureDefInternal));
		}

		void IFixedSize.Lock()
		{
		}

		void IFixedSize.Unlock()
		{
		}

		public IntPtr _shape;
		public IntPtr _userData;
		public float _friction;
		public float _restitution;
		public float _density;
		[MarshalAs(UnmanagedType.U1)]
		public bool _isSensor;
		public FilterData _filter;
	}

	public sealed class FixtureDef : ICompare<FixtureDef>, ICloneable
	{
		FixtureDefInternal _internalFixture = new FixtureDefInternal();
		// private, unrelated data
		Shape _realShape;

		internal FixtureDefInternal Internal
		{
			get { return _internalFixture; }
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Clone this FixtureDef.
		/// </summary>
		/// <returns></returns>
		public FixtureDef Clone()
		{
			return new FixtureDef(Shape.Clone(), Density, Restitution, Friction, Filter, IsSensor, UserData);
		}

		/// <summary>
		/// User-specific and application-specific data.
		/// </summary>
		public object UserData
		{
			get { return UserDataStorage.FixtureStorage.ObjectFromHandle(UserDataStorage.IntPtrToHandle(_internalFixture._userData)); }

			set
			{
				var ptr = UserDataStorage.IntPtrToHandle(_internalFixture._userData);

				if (ptr != 0)
					UserDataStorage.FixtureStorage.UnpinObject(ptr);

				if (value != null)
				{
					var handle = UserDataStorage.FixtureStorage.PinDataToHandle(value);
					_internalFixture._userData = UserDataStorage.HandleToIntPtr(handle);
				}
				else
					_internalFixture._userData = IntPtr.Zero;
			}
		}

		/// <summary>
		/// The shape connected to this fixture.
		/// </summary>
		public Shape Shape
		{
			get { return _realShape; }
			set { _realShape = value; }
		}

		/// <summary>
		/// The friction of this fixture.
		/// </summary>
		public float Friction
		{
			get { return _internalFixture._friction; }
			set { _internalFixture._friction = value; }
		}

		/// <summary>
		/// The restitution of this fixture.
		/// </summary>
		public float Restitution
		{
			get { return _internalFixture._restitution; }
			set { _internalFixture._restitution = value; }
		}

		/// <summary>
		/// The density of this fixture.
		/// </summary>
		public float Density
		{
			get { return _internalFixture._density; }
			set { _internalFixture._density = value; }
		}

		/// <summary>
		/// True if this fixture is a sensor shape (doesn't generate contacts).
		/// </summary>
		public bool IsSensor
		{
			get { return _internalFixture._isSensor; }
			set { _internalFixture._isSensor = value; }
		}

		/// <summary>
		/// The filtering data for this fixture.
		/// </summary>
		public FilterData Filter
		{
			get { return _internalFixture._filter; }
			set { _internalFixture._filter = value; }
		}

		internal void SetShape(IntPtr intPtr)
		{
			_internalFixture._shape = intPtr;
		}

		public FixtureDef() :
			this(null)
		{
		}

		public FixtureDef(Shape shape, float density, float restitution, float friction, FilterData filter, bool isSensor = false, object userData = null)
		{
			_realShape = shape;
			UserData = userData;
			_internalFixture._friction = friction;
			_internalFixture._restitution = restitution;
			_internalFixture._density = density;
			Filter = filter;
			_internalFixture._isSensor = isSensor;
		}

		public FixtureDef(Shape shape, float density = 0.0f, float restitution = 0.0f, float friction = 0.2f, bool isSensor = false, object userData = null) :
			this(shape, density, restitution, friction, new FilterData(), isSensor, userData)
		{
		}

		/// <summary>
		/// Compare this FixtureDef with another.
		/// </summary>
		/// <param name="fixture">The fixture to compare to.</param>
		/// <returns>True if they equal; false if not.</returns>
		public bool CompareWith(FixtureDef fixture)
		{
			return (this.Density == fixture.Density &&
				this.Filter == fixture.Filter &&
				this.Friction == fixture.Friction &&
				this.IsSensor == fixture.IsSensor &&
				this.Restitution == fixture.Restitution && 
				this.Shape.CompareWith(fixture.Shape) &&
				this.UserData == fixture.UserData);
		}
	}

	public struct Fixture
	{
		static class NativeMethods
		{
			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern int b2fixture_gettype(IntPtr fixture);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2fixture_getshape(IntPtr fixture, IntPtr shape);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2fixture_setsensor(IntPtr fixture, [MarshalAs(UnmanagedType.U1)] bool val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2fixture_getsensor(IntPtr fixture);

			[StructLayout(LayoutKind.Sequential)]
			public struct FilterDataInternal
			{
				FilterData _filterData;

				public FilterDataInternal(FilterData data)
				{
					_filterData = data;
				}
			}

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2fixture_setfilterdata(IntPtr fixture, FilterDataInternal val);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2fixture_getfilterdata(IntPtr fixture, [Out] [In] FilterData filterData);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2fixture_getbody(IntPtr fixture);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2fixture_getnext(IntPtr fixture);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2fixture_testpoint(IntPtr fixture, Vec2 point);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool b2fixture_raycast(IntPtr fixture, out RayCastOutput output, RayCastInput input);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2fixture_getmassdata(IntPtr fixture, out MassData data);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2fixture_getdensity(IntPtr fixture);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2fixture_setdensity(IntPtr fixture, float density);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2fixture_getfriction(IntPtr fixture);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2fixture_setfriction(IntPtr fixture, float density);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern float b2fixture_getrestitution(IntPtr fixture);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2fixture_setrestitution(IntPtr fixture, float density);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2fixture_getaabb(IntPtr fixture, out AABB outPtr);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern void b2fixture_setuserdata(IntPtr fixture, IntPtr data);

			[DllImport(Box2DSettings.Box2CDLLName, CallingConvention=CallingConvention.Cdecl)]
			public static extern IntPtr b2fixture_getuserdata(IntPtr fixture);
		}

		IntPtr _fixturePtr;

		internal IntPtr FixturePtr
		{
			get { return _fixturePtr; }
			set { _fixturePtr = value; }
		}

		internal Fixture(IntPtr ptr)
		{
			_fixturePtr = ptr;
		}

		internal static Fixture FromPtr(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				throw new Exception();

			return new Fixture(ptr);
		}

		internal static Fixture? FromPtrNoCrash(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return null;

			return new Fixture(ptr);
		}

		/// <summary>
		/// The type of Shape attached to this fixture.
		/// </summary>
		public ShapeType ShapeType
		{
			get { return (ShapeType)NativeMethods.b2fixture_gettype(_fixturePtr); }
		}

		/// <summary>
		/// Get or set the shape attached to this fixture.
		/// </summary>
		public Shape Shape
		{
			get
			{
				switch (ShapeType)
				{
				case ShapeType.Circle:
					{
						cb2circleshapeportable shape = new cb2circleshapeportable();

						using (var ptr = new StructToPtrMarshaller(shape))
						{
							NativeMethods.b2fixture_getshape(_fixturePtr, ptr.Pointer);
							shape = (cb2circleshapeportable)ptr.GetValue(typeof(cb2circleshapeportable));
						}

						return new CircleShape(shape);
					}
				case ShapeType.Polygon:
					{
						cb2polygonshapeportable shape = new cb2polygonshapeportable();

						using (var ptr = new StructToPtrMarshaller(shape))
						{
							NativeMethods.b2fixture_getshape(_fixturePtr, ptr.Pointer);
							shape = (cb2polygonshapeportable)ptr.GetValue(typeof(cb2polygonshapeportable));
						}

						return new PolygonShape(shape);
					}
				}

				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// User-specific and application-specific data.
		/// </summary>
		public object UserData
		{
			get { return UserDataStorage.FixtureStorage.ObjectFromHandle(UserDataStorage.IntPtrToHandle(NativeMethods.b2fixture_getuserdata(_fixturePtr))); }

			set
			{
				var ptr = UserDataStorage.IntPtrToHandle(NativeMethods.b2fixture_getuserdata(_fixturePtr));

				if (ptr != 0)
					UserDataStorage.FixtureStorage.UnpinObject(ptr);

				if (value != null)
				{
					var handle = UserDataStorage.FixtureStorage.PinDataToHandle(value);
					NativeMethods.b2fixture_setuserdata(_fixturePtr, UserDataStorage.HandleToIntPtr(handle));
				}
				else
					NativeMethods.b2fixture_setuserdata(_fixturePtr, IntPtr.Zero);
			}
		}

		/// <summary>
		/// Get or set whether this fixture is a sensor shape or not.
		/// </summary>
		public bool IsSensor
		{
			get { return NativeMethods.b2fixture_getsensor(_fixturePtr); }
			set { NativeMethods.b2fixture_setsensor(_fixturePtr, value); }
		}

		/// <summary>
		/// Get or set the filter data associated with this fixture.
		/// </summary>
		public FilterData FilterData
		{
			get { FilterData temp = new FilterData(); NativeMethods.b2fixture_getfilterdata(_fixturePtr, temp); return temp; }
			set { NativeMethods.b2fixture_setfilterdata(_fixturePtr, new NativeMethods.FilterDataInternal(value)); }
		}

		/// <summary>
		/// Get the body this fixture is attached to.
		/// </summary>
		public Body Body
		{
			get { return Body.FromPtr(NativeMethods.b2fixture_getbody(_fixturePtr)); }
		}

		/// <summary>
		/// Get the next fixture in the attached body's fixture list.
		/// </summary>
		public Fixture? Next
		{
			get { return Fixture.FromPtrNoCrash(NativeMethods.b2fixture_getnext(_fixturePtr)); }
		}

		/// <summary>
		/// Test if the point is inside the fixture.
		/// </summary>
		/// <param name="Point">Point to test</param>
		/// <returns>True if inside, false if not.</returns>
		public bool TestPoint(Vec2 Point)
		{
			return NativeMethods.b2fixture_testpoint(_fixturePtr, Point);
		}

		/// <summary>
		/// Perform a raycast against this fixture.
		/// </summary>
		/// <param name="Output">The output data</param>
		/// <param name="Input">The input data</param>
		/// <returns>True if hit, false if not.</returns>
		public bool RayCast(out RayCastOutput Output, RayCastInput Input)
		{
			Output = new RayCastOutput();
			var returnVal = NativeMethods.b2fixture_raycast(_fixturePtr, out Output, Input);
			return returnVal;
		}

		/// <summary>
		/// Get the mass data for this fixture.
		/// </summary>
		public MassData MassData
		{
			get { MassData returnVal = new MassData(); NativeMethods.b2fixture_getmassdata(_fixturePtr, out returnVal); return returnVal; }
		}

		/// <summary>
		/// Get or set the density of this fixture.
		/// </summary>
		public float Density
		{
			get { return NativeMethods.b2fixture_getdensity(_fixturePtr); }
			set { NativeMethods.b2fixture_setdensity(_fixturePtr, value); }
		}

		/// <summary>
		/// Get or set the friction of this fixture.
		/// </summary>
		public float Friction
		{
			get { return NativeMethods.b2fixture_getfriction(_fixturePtr); }
			set { NativeMethods.b2fixture_setfriction(_fixturePtr, value); }
		}

		/// <summary>
		/// Get or set the restitution of this fixture.
		/// </summary>
		public float Restitution
		{
			get { return NativeMethods.b2fixture_getrestitution(_fixturePtr); }
			set { NativeMethods.b2fixture_setrestitution(_fixturePtr, value); }
		}

		/// <summary>
		/// Get the AABB of this fixture.
		/// </summary>
		public AABB AABB
		{
			get { AABB temp; NativeMethods.b2fixture_getaabb(_fixturePtr, out temp); return temp; }
		}

		public static bool operator ==(Fixture l, Fixture r)
		{
			if ((object)l == null && (object)r == null)
				return true;
			else if ((object)l == null && (object)r != null ||
				(object)l != null && (object)r == null)
				return false;

			return (l.FixturePtr == r.FixturePtr);
		}

		public static bool operator !=(Fixture l, Fixture r)
		{
			return !(l == r);
		}

		public override bool Equals(object obj)
		{
			if (obj is Fixture)
				return ((Fixture)obj) == this;

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return FixturePtr.GetHashCode();
		}
	}
}
