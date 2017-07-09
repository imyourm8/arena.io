// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the BOX2C_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// BOX2C_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef BOX2C_EXPORTS
#define BOX2C_API
#else
#define BOX2C_API __declspec(dllimport)
#endif

#include <Box2D\Box2D.h>

typedef b2World cb2world;
typedef b2Body cb2body;
typedef b2Joint cb2joint;
typedef b2Fixture cb2fixture;
typedef b2Contact cb2contact;
typedef b2Vec2 cb2vec2;
typedef b2Manifold cb2manifold;
typedef b2ContactImpulse cb2contactimpulse;
typedef b2Filter cb2filter;
typedef b2RayCastOutput cb2raycastoutput;
typedef b2RayCastInput cb2raycastinput;
typedef b2MassData cb2massdata;
typedef b2AABB cb2aabb;
typedef b2JointEdge cb2jointedge;
typedef b2ContactEdge cb2contactedge;
typedef b2WorldManifold cb2worldmanifold;
typedef b2Transform cb2transform;
typedef b2GearJoint cb2gearjoint;
typedef b2DistanceJoint cb2distancejoint;
typedef b2FrictionJoint cb2frictionjoint;
typedef b2PulleyJoint cb2pulleyjoint;
typedef b2MouseJoint cb2mousejoint;
typedef b2LineJoint cb2linejoint;
typedef b2RevoluteJoint cb2revolutejoint;
typedef b2PrismaticJoint cb2prismaticjoint;
typedef b2Version cb2version;
typedef b2ClipVertex cb2clipvertex;

#define QUICK_GET_SETTER(getname,setname,ptrtype,type,member) \
	type getname (ptrtype *me) \
	{ \
		return me->member; \
	} \
		void setname (ptrtype *me, type val) \
	{ \
		me->member = val; \
	}

#define QUICK_GET_SETTER_POINTER(getname,setname,ptrtype,type,member) \
	void getname (ptrtype *me, type *outType) \
	{ \
		*outType = me->member; \
	} \
		void setname (ptrtype *me, type val) \
	{ \
		me->member = val; \
	}

#define QUICK_GETTER(getname,ptrtype,type,member) \
	type getname (ptrtype *me) \
	{ \
		return me->member; \
	} 

#define QUICK_GETTER_PTR(getname,ptrtype,type,member) \
	void getname (ptrtype *me, type *outVar) \
	{ \
		*outVar = me->member; \
	} 

#define QUICK_GET_SETTER_FUNC(getname,setname,ptrtype,type,getmember, setmember) \
	type getname (ptrtype *me) \
	{ \
		return me->getmember(); \
	} \
		void setname (ptrtype *me, type val) \
	{ \
		me->setmember(val); \
	}

#define QUICK_GET_SETTER_FUNC_PTR(getname,setname,ptrtype,type,getmember, setmember) \
	void getname (ptrtype *me, type *outPtr) \
	{ \
		*outPtr = me->getmember(); \
	} \
		void setname (ptrtype *me, type val) \
	{ \
		me->setmember(val); \
	}

#define QUICK_SETTER_FUNC(setname,ptrtype,type, setmember) \
	void setname (ptrtype *me, type val) \
	{ \
		me->setmember(val); \
	}

#define QUICK_CONSTRUCTOR_DESTROYER(constructor, destructor, type) \
	type *constructor () \
	{ \
		return new type; \
	} \
		void destructor (type *ptr) \
	{ \
		delete ptr; \
	}

// These are "portable" versions of the types they represent.
// The original types have virtual destructors and inheritance-issues,
// and aren't exactly cross-platform/cross-language in their usages.
struct cb2shapeportable
{
	b2Shape::Type	m_type;
	float32			m_radius;
};

struct cb2circleshapeportable
{
	cb2shapeportable m_shape;

	b2Vec2 m_p;
};

struct cb2polygonshapeportable
{
	cb2shapeportable m_shape;

	b2Vec2 m_centroid;
	b2Vec2 m_vertices[b2_maxPolygonVertices];
	b2Vec2 m_normals[b2_maxPolygonVertices];
	int32 m_vertexCount;
};

struct cb2fixturedefportable
{
	const cb2shapeportable* shape;
	void* userData;
	float32 friction;
	float32 restitution;
	float32 density;
	bool isSensor;
	b2Filter filter;
};