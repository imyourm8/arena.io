// Box2C.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Box2C.h"

cb2world *b2world_constructor (cb2vec2 gravity, bool doSleep)
{
	return new b2World(gravity, doSleep);
}

void b2world_destroy (cb2world *world)
{
	delete world;
}

struct cb2destructionlistener
{
	typedef void (*saygoodbye_joint) (cb2joint *joint);
	typedef void (*saygoodbye_fixture) (cb2fixture *joint);

	saygoodbye_joint saygoodbye_joint_callback;
	saygoodbye_fixture saygoodbye_fixture_callback;
};

class cb2DestructionListenerWrapper : public b2DestructionListener
{
public:
	cb2DestructionListenerWrapper (cb2destructionlistener listener) :
	  listener(listener)
	  {
	  }

	cb2destructionlistener listener;

	void SayGoodbye(b2Fixture* fixture)
	{
		listener.saygoodbye_fixture_callback(fixture);
	}

	void SayGoodbye(b2Joint* joint)
	{
		listener.saygoodbye_joint_callback(joint);
	}
};

cb2DestructionListenerWrapper *cb2destructionlistener_create (cb2destructionlistener functions)
{
	return new cb2DestructionListenerWrapper(functions);
}

void cb2destructionlistener_destroy (cb2DestructionListenerWrapper *listener)
{
	delete listener;
}

//SetDestructionListener
void b2world_setdestructionlistener (cb2world *world, cb2DestructionListenerWrapper *listener)
{
	world->SetDestructionListener(listener);
}

//SetContactFilter
struct cb2contactfilter
{
	typedef bool (*shouldcollide) (cb2fixture *fixtureA, cb2fixture *fixtureB);

	shouldcollide shouldcollide_callback;
};

class cb2ContactFilterWrapper : public b2ContactFilter
{
public:
	cb2ContactFilterWrapper (cb2contactfilter listener) :
	  listener(listener)
	  {
	  }

	cb2contactfilter listener;

	bool ShouldCollide(b2Fixture* fixtureA, b2Fixture* fixtureB)
	{
		return listener.shouldcollide_callback(fixtureA, fixtureB);
	}
};

cb2ContactFilterWrapper *cb2contactfilter_create (cb2contactfilter functions)
{
	return new cb2ContactFilterWrapper(functions);
}

void cb2contactfilter_destroy (cb2ContactFilterWrapper *listener)
{
	delete listener;
}

void b2world_setcontactfilter (cb2world *world, cb2ContactFilterWrapper *listener)
{
	world->SetContactFilter(listener);
}

//SetContactListener
struct cb2contactlistener
{
	typedef void (*beginendcontact) (cb2contact *contact);
	typedef void (*presolve) (cb2contact *contact, cb2manifold oldManifold);
	typedef void (*postsolve) (cb2contact *contact, cb2contactimpulse impulse);

	beginendcontact begincontact_callback;
	beginendcontact endcontact_callback;
	presolve presolve_callback;
	postsolve postsolve_callback;
};

class cb2ContactListenerWrapper : public b2ContactListener
{
public:
	cb2ContactListenerWrapper (cb2contactlistener listener) :
	  listener(listener)
	  {
	  }

	  cb2contactlistener listener;

	  void BeginContact(b2Contact* contact)
	  {
		  listener.begincontact_callback(contact);
	  }

	  void EndContact(b2Contact* contact) 
	  {
		  listener.endcontact_callback(contact);
	  }

	  void PreSolve(b2Contact* contact, const b2Manifold* oldManifold)
	  {
		  listener.presolve_callback(contact, *oldManifold);
	  }

	  void PostSolve(b2Contact* contact, const b2ContactImpulse* impulse)
	  {
		  listener.postsolve_callback(contact, *impulse);
	  }
};

cb2ContactListenerWrapper *cb2contactlistener_create (cb2contactlistener functions)
{
	return new cb2ContactListenerWrapper(functions);
}

void cb2contactlistener_destroy (cb2ContactListenerWrapper *listener)
{
	delete listener;
}

void b2world_setcontactlistener (cb2world *world, cb2ContactListenerWrapper *listener)
{
	world->SetContactListener(listener);
}

//SetDebugDraw
struct cb2debugdraw
{
	typedef void (*drawpolygon) (const b2Vec2* vertices, int vertexCount, b2Color color);
	typedef void (*drawcircle) (b2Vec2 center, float radius, b2Color color);
	typedef void (*drawsolidcircle) (b2Vec2 center, float radius, b2Vec2 axis, b2Color color);
	typedef void (*drawsegment) (b2Vec2 p1, b2Vec2 p2, b2Color color);
	typedef void (*drawtransform) (b2Transform xf);

	drawpolygon drawpolygon_callback;
	drawpolygon drawsolidpolygon_callback;
	drawcircle drawcircle_callback;
	drawsolidcircle drawsolidcircle_callback;
	drawsegment drawsegment_callback;
	drawtransform drawtransform_callback;
};

class cb2DebugDrawWrapper : public b2DebugDraw
{
public:
	cb2DebugDrawWrapper (cb2debugdraw listenerPtr) :
	  listener(listenerPtr)
	{
	}

	cb2debugdraw listener;

	virtual void DrawPolygon(const b2Vec2* vertices, int32 vertexCount, const b2Color& color)
	{
		if (listener.drawpolygon_callback)
			listener.drawpolygon_callback(vertices, vertexCount, color);
	}

	virtual void DrawSolidPolygon(const b2Vec2* vertices, int32 vertexCount, const b2Color& color)
	{
		if (listener.drawsolidpolygon_callback)
			listener.drawsolidpolygon_callback(vertices, vertexCount, color);
	}

	virtual void DrawCircle(const b2Vec2& center, float32 radius, const b2Color& color)
	{
		if (listener.drawcircle_callback)
			listener.drawcircle_callback(center, radius, color);
	}

	virtual void DrawSolidCircle(const b2Vec2& center, float32 radius, const b2Vec2& axis, const b2Color& color)
	{
		if (listener.drawsolidcircle_callback)
			listener.drawsolidcircle_callback(center, radius, axis, color);
	}

	virtual void DrawSegment(const b2Vec2& p1, const b2Vec2& p2, const b2Color& color)
	{
		if (listener.drawsegment_callback)
			listener.drawsegment_callback(p1, p2, color);
	}

	virtual void DrawTransform(const b2Transform& xf)
	{
		if (listener.drawtransform_callback)
			listener.drawtransform_callback(xf);
	}
};

cb2DebugDrawWrapper *cb2debugdraw_create (cb2debugdraw functions)
{
	return new cb2DebugDrawWrapper(functions);
}

uint32 cb2debugdraw_getflags (cb2DebugDrawWrapper *listener)
{
	return listener->GetFlags();
}

void cb2debugdraw_setflags (cb2DebugDrawWrapper *listener, uint32 flags)
{
	listener->SetFlags(flags);
}

void cb2debugdraw_destroy (cb2DebugDrawWrapper *listener)
{
	delete listener;
}

void b2world_setdebugdraw (cb2world *world, cb2DebugDrawWrapper *listener)
{
	world->SetDebugDraw(listener);
}

cb2body *b2world_createbody (cb2world *world, b2BodyDef *def)
{
	return world->CreateBody (def);
}

void b2world_destroybody (cb2world *world, cb2body *body)
{
	world->DestroyBody(body);
}

cb2joint *b2world_createjoint (cb2world *world, b2JointDef *def)
{
	return world->CreateJoint(def);
}

void b2world_destroyjoint (cb2world *world, cb2joint *joint)
{
	world->DestroyJoint(joint);
}

/*
long _elapsed, _min = LONG_MAX, _max = LONG_MIN;
#include <Windows.h>
#include <iostream>
#include <mmsystem.h>

LARGE_INTEGER timerFreq_;
LARGE_INTEGER counterAtStart_;

void startTime()
{
  QueryPerformanceFrequency(&timerFreq_);
  QueryPerformanceCounter(&counterAtStart_);
  //cout<<"timerFreq_ = "<<timerFreq_.QuadPart<<endl;
  //cout<<"counterAtStart_ = "<<counterAtStart_.QuadPart<<endl;
  TIMECAPS ptc;
  UINT cbtc = 8;
  MMRESULT result = timeGetDevCaps(&ptc, cbtc);
  if (result == TIMERR_NOERROR)
  {
    //cout<<"Minimum resolution = "<<ptc.wPeriodMin<<endl;
    //cout<<"Maximum resolution = "<<ptc.wPeriodMax<<endl;
  }
  else
  {
    //cout<<"result = TIMER ERROR"<<endl;
  }
}

unsigned int calculateElapsedTime()
{
  if (timerFreq_.QuadPart == 0)
  {
    return -1;
  }
  else
  {
    LARGE_INTEGER c;
    QueryPerformanceCounter(&c);
    return static_cast<unsigned int>( (c.QuadPart - counterAtStart_.QuadPart) * 1000 / timerFreq_.QuadPart );
  }
}
*/

void b2world_step (cb2world *world, float timeStep, int velocityIterations, int positionIterations)
{
	//startTime();
	world->Step(timeStep, velocityIterations, positionIterations);
	/*_elapsed = calculateElapsedTime();

	if (_elapsed < _min)
		_min = _elapsed;
	if (_elapsed > _max)
		_max = _elapsed;*/
}
/*
long b2world_getelapsed ()
{
	return _elapsed;
}

long b2world_getmin ()
{
	return _min;
}

long b2world_getmax ()
{
	return _max;
}
*/

void b2world_clearforces (cb2world *world)
{
	world->ClearForces();
}

void b2world_drawdebugdata (cb2world *world)
{
	world->DrawDebugData();
}

// QueryAABB

// RayCast

cb2body *b2world_getbodylist (cb2world *world)
{
	return world->GetBodyList();
}

cb2joint *b2world_getjointlist (cb2world *world)
{
	return world->GetJointList();
}

cb2contact *b2world_getcontactlist (cb2world *world)
{
	return world->GetContactList();
}

void b2world_setwarmstarting (cb2world *world, bool flag)
{
	world->SetWarmStarting(flag);
}

void b2world_setcontinuousphysics (cb2world *world, bool flag)
{
	world->SetContinuousPhysics(flag);
}

int b2world_getproxycount (cb2world *world)
{
	return world->GetProxyCount();
}

int b2world_getbodycount (cb2world *world)
{
	return world->GetBodyCount();
}

int b2world_getjointcount (cb2world *world)
{
	return world->GetJointCount();
}

int b2world_getcontactcount (cb2world *world)
{
	return world->GetContactCount();
}

void b2world_setgravity (cb2world *world, cb2vec2 gravity)
{
	world->SetGravity(gravity);
}

cb2vec2 b2world_getgravity (cb2world *world)
{
	return world->GetGravity();
}

bool b2world_islocked (cb2world *world)
{
	return world->IsLocked();
}

void b2world_setautoclearforces (cb2world *world, bool flag)
{
	world->SetAutoClearForces(flag);
}

bool b2world_getautoclearforces (cb2world *world)
{
	return world->GetAutoClearForces();
}

struct cb2querycallback
{
	typedef bool (*reportfixture) (b2Fixture* fixture);

	reportfixture reportfixture_callback;
};

class cb2QueryCallbackWrapper : public b2QueryCallback
{
public:
	cb2QueryCallbackWrapper (cb2querycallback listener) :
	  listener(listener)
	  {
	  }

	cb2querycallback listener;

	bool ReportFixture(b2Fixture* fixture)
	{
		return listener.reportfixture_callback(fixture);
	}
};

cb2QueryCallbackWrapper *cb2querycallback_create (cb2querycallback functions)
{
	return new cb2QueryCallbackWrapper(functions);
}

void cb2querycallback_destroy (cb2QueryCallbackWrapper *listener)
{
	delete listener;
}

struct cb2raycastcallback
{
	typedef float (*reportfixture) (cb2fixture* fixture, cb2vec2 point,
									cb2vec2 normal, float fraction);

	reportfixture reportfixture_callback;
};

class cb2RayCastCallbackWrapper : public b2RayCastCallback
{
public:
	cb2RayCastCallbackWrapper (cb2raycastcallback listener) :
	  listener(listener)
	  {
	  }

	cb2raycastcallback listener;

	float ReportFixture(	b2Fixture* fixture, const b2Vec2& point,
									const b2Vec2& normal, float fraction)
	{
		return listener.reportfixture_callback(fixture, point, normal, fraction);
	}
};

cb2RayCastCallbackWrapper *cb2raycastcallback_create (cb2raycastcallback functions)
{
	return new cb2RayCastCallbackWrapper(functions);
}

void cb2raycastcallback_destroy (cb2RayCastCallbackWrapper *listener)
{
	delete listener;
}

void b2world_queryaabb (cb2world *world, cb2QueryCallbackWrapper *callback, cb2aabb aabb)
{
	world->QueryAABB(callback, aabb);
}

void b2world_raycast (cb2world *world, cb2RayCastCallbackWrapper *callback, cb2vec2 point1, cb2vec2 point2)
{
	world->RayCast(callback, point1, point2);
}

int b2fixture_gettype (cb2fixture *fixture)
{
	return fixture->GetType();
}

void b2fixture_getshape (cb2fixture *fixture, cb2shapeportable *shape)
{
	switch (fixture->GetShape()->m_type)
	{
	case b2Shape::e_circle:
		{
			cb2circleshapeportable *circle = (cb2circleshapeportable*)shape;
			b2CircleShape *circleIn = (b2CircleShape*)fixture->GetShape();
			circle->m_p = circleIn->m_p;
			circle->m_shape.m_radius = circleIn->m_radius;
			circle->m_shape.m_type = circleIn->m_type;
		}
		break;
	case b2Shape::e_polygon:
		{
			cb2polygonshapeportable *poly = (cb2polygonshapeportable*)shape;
			b2PolygonShape *polyIn = (b2PolygonShape*)fixture->GetShape();
			poly->m_shape.m_radius = polyIn->m_radius;
			poly->m_shape.m_type = polyIn->m_type;
			poly->m_centroid = polyIn->m_centroid;
			poly->m_vertexCount = polyIn->m_vertexCount;
			memcpy (poly->m_vertices, polyIn->m_vertices, sizeof(polyIn->m_vertices));
			memcpy (poly->m_normals, polyIn->m_normals, sizeof(polyIn->m_normals));
		}
		break;
	}
}

void b2fixture_setsensor (cb2fixture *fixture, bool val)
{
	fixture->SetSensor(val);
}

bool b2fixture_getsensor (cb2fixture *fixture)
{
	return fixture->IsSensor();
}

void b2fixture_setfilterdata (cb2fixture *fixture, cb2filter filter)
{
	fixture->SetFilterData(filter);
}

void b2fixture_getfilterdata (cb2fixture *fixture, cb2filter *outFilter)
{
	*outFilter = fixture->GetFilterData();
}

cb2body *b2fixture_getbody (cb2fixture *fixture)
{
	return fixture->GetBody();
}

cb2fixture *b2fixture_getnext (cb2fixture *fixture)
{
	return fixture->GetNext();
}

void b2fixture_setuserdata (cb2fixture *fixture, void *data)
{
	fixture->SetUserData(data);
}

void *b2fixture_getuserdata (cb2fixture *fixture)
{
	return fixture->GetUserData();
}

bool b2fixture_testpoint (cb2fixture *fixture, cb2vec2 point)
{
	return fixture->TestPoint(point);
}

bool b2fixture_raycast (cb2fixture *fixture, cb2raycastoutput *output, cb2raycastinput input)
{
	return fixture->RayCast(output, input);
}

void b2fixture_getmassdata (cb2fixture *fixture, cb2massdata *data)
{
	fixture->GetMassData(data);
}

float b2fixture_getdensity (cb2fixture *fixture)
{
	return fixture->GetDensity();
}

void b2fixture_setdensity (cb2fixture *fixture, float val)
{
	fixture->SetDensity(val);
}

float b2fixture_getfriction (cb2fixture *fixture)
{
	return fixture->GetFriction();
}

void b2fixture_setfriction (cb2fixture *fixture, float val)
{
	fixture->SetFriction(val);
}

float b2fixture_getrestitution (cb2fixture *fixture)
{
	return fixture->GetRestitution();
}

void b2fixture_setrestitution (cb2fixture *fixture, float val)
{
	fixture->SetRestitution(val);
}

void b2fixture_getaabb (cb2fixture *fixture, cb2aabb *outAABB)
{
	*outAABB = fixture->GetAABB();
}

b2CircleShape CircleShapeFromPortableCircleShape (cb2circleshapeportable *portable)
{
	b2CircleShape circleShape;
	circleShape.m_radius = portable->m_shape.m_radius;
	circleShape.m_type = portable->m_shape.m_type;
	circleShape.m_p = portable->m_p;
	return circleShape;
}

b2PolygonShape PolygonShapeFromPortableCircleShape (cb2polygonshapeportable *portable)
{
	b2PolygonShape polyShape;
	polyShape.m_radius = portable->m_shape.m_radius;
	polyShape.m_type = portable->m_shape.m_type;
	polyShape.m_centroid = portable->m_centroid;
	polyShape.m_vertexCount = portable->m_vertexCount;
	memcpy (polyShape.m_normals, portable->m_normals, sizeof(polyShape.m_normals));
	memcpy (polyShape.m_vertices, portable->m_vertices, sizeof(polyShape.m_vertices));
	return polyShape;
}

// be sure to free this
b2Shape *ShapeFromPortableShape (cb2shapeportable *portable)
{
	b2Shape *newShape;

	switch (portable->m_type)
	{
	case b2Shape::e_circle:
		{
			b2CircleShape ncircle = CircleShapeFromPortableCircleShape((cb2circleshapeportable*)portable);
			newShape = new b2CircleShape;
			memcpy (newShape, &ncircle, sizeof(ncircle));
		}
		break;
	case b2Shape::e_polygon:
		{
			b2PolygonShape npoly = PolygonShapeFromPortableCircleShape((cb2polygonshapeportable*)portable);
			newShape = new b2PolygonShape;
			memcpy (newShape, &npoly, sizeof(npoly));
		}
		break;
	}

	return newShape;
}

cb2fixture *b2body_createfixture (cb2body *body, cb2fixturedefportable *fixtureDef)
{
	b2FixtureDef tempDef;
	tempDef.userData = fixtureDef->userData;
	tempDef.friction = fixtureDef->friction;
	tempDef.restitution = fixtureDef->restitution;
	tempDef.density = fixtureDef->density;
	tempDef.isSensor = fixtureDef->isSensor;
	tempDef.filter = fixtureDef->filter;

	switch (fixtureDef->shape->m_type)
	{
	case b2Shape::e_circle:
		{
			b2CircleShape circleShape = CircleShapeFromPortableCircleShape((cb2circleshapeportable*)fixtureDef->shape);
			tempDef.shape = &circleShape;
			return body->CreateFixture(&tempDef);
		}
	case b2Shape::e_polygon:
		{
			b2PolygonShape polyShape = PolygonShapeFromPortableCircleShape((cb2polygonshapeportable*)fixtureDef->shape);
			tempDef.shape = &polyShape;
			return body->CreateFixture(&tempDef);
		}
	default:
		return NULL;
	}
}

void b2body_gettransform (cb2body *body, cb2transform *trans)
{
	*trans = body->GetTransform();
}

cb2fixture *b2body_createfixturefromshape (cb2body *body, cb2shapeportable *shape, float density)
{
	switch (shape->m_type)
	{
	case b2Shape::e_circle:
		{
			b2CircleShape circleShape;
			circleShape.m_radius = shape->m_radius;
			circleShape.m_type = shape->m_type;
			circleShape.m_p = ((cb2circleshapeportable*)shape)->m_p;
			return body->CreateFixture(&circleShape, density);
		}
	case b2Shape::e_polygon:
		{
			b2PolygonShape polyShape;
			polyShape.m_radius = shape->m_radius;
			polyShape.m_type = shape->m_type;
			cb2polygonshapeportable *poly = ((cb2polygonshapeportable*)shape);
			polyShape.m_centroid = poly->m_centroid;
			polyShape.m_vertexCount = poly->m_vertexCount;
			memcpy (polyShape.m_normals, poly->m_normals, sizeof(polyShape.m_normals));
			memcpy (polyShape.m_vertices, poly->m_vertices, sizeof(polyShape.m_vertices));

			return body->CreateFixture(&polyShape, density);
		}
	default:
		return NULL;
	}
}

cb2body *b2body_getnext (cb2body *body)
{
	return body->GetNext();
}

void b2body_destroyfixture (cb2body *body, cb2fixture *fixture)
{
	body->DestroyFixture(fixture);
}

void b2body_settransform (cb2body *body, cb2vec2 pos, float angle)
{
	body->SetTransform(pos, angle);
}

void b2body_getposition (cb2body *body, cb2vec2 *outVec)
{
	*outVec = body->GetPosition();
}

float b2body_getangle (cb2body *body)
{
	return body->GetAngle();
}

void b2body_getworldcenter (cb2body *body, cb2vec2 *outVec)
{
	*outVec = body->GetWorldCenter();
}

void b2body_getlocalcenter (cb2body *body, cb2vec2 *outVec)
{
	*outVec = body->GetLocalCenter();
}

void b2body_setlinearvelocity (cb2body *body, cb2vec2 v)
{
	body->SetLinearVelocity(v);
}

void b2body_getlinearvelocity (cb2body *body, cb2vec2 *outVec)
{
	*outVec = body->GetLinearVelocity();
}

void b2body_setangularvelocity (cb2body *body, float v)
{
	body->SetAngularVelocity(v);
}

float b2body_getangularvelocity (cb2body *body)
{
	return body->GetAngularVelocity();
}

void b2body_applyforce (cb2body *body, cb2vec2 force, cb2vec2 point)
{
	body->ApplyForce(force, point);
}

void b2body_applytorque (cb2body *body, float torque)
{
	body->ApplyTorque(torque);
}

void b2body_applylinearimpulse (cb2body *body, cb2vec2 force, cb2vec2 point)
{
	body->ApplyLinearImpulse(force, point);
}

void b2body_applyangularimpulse (cb2body *body, float impulse)
{
	body->ApplyAngularImpulse(impulse);
}

float b2body_getmass (cb2body *body)
{
	return body->GetMass();
}

float b2body_getinertia (cb2body *body)
{
	return body->GetInertia();
}

void b2body_getmassdata (cb2body *body, cb2massdata *data)
{
	body->GetMassData(data);
}

void b2body_setmassdata (cb2body *body, cb2massdata *data)
{
	body->SetMassData(data);
}

void b2body_resetmassdata (cb2body *body)
{
	body->ResetMassData();
}

void b2body_getworldpoint (cb2body *body, cb2vec2 localPoint, cb2vec2 *outPoint)
{
	*outPoint = body->GetWorldPoint(localPoint);
}

void b2body_getworldvector (cb2body *body, cb2vec2 localPoint, cb2vec2 *outPos)
{
	*outPos = body->GetWorldVector(localPoint);
}

void b2body_getlocalpoint (cb2body *body, cb2vec2 localPoint, cb2vec2 *outPos)
{
	*outPos = body->GetLocalPoint(localPoint);
}

void b2body_getlocalvector (cb2body *body, cb2vec2 localPoint, cb2vec2 *outPos)
{
	*outPos = body->GetLocalVector(localPoint);
}

void b2body_getlinearvelocityfromworldvector (cb2body *body, cb2vec2 localPoint, cb2vec2 *outPos)
{
	*outPos = body->GetLinearVelocityFromWorldPoint(localPoint);
}

void b2body_getlinearvelocityfromlocalvector (cb2body *body, cb2vec2 localPoint, cb2vec2 *outPos)
{
	*outPos = body->GetLinearVelocityFromLocalPoint(localPoint);
}

float b2body_getlineardamping (cb2body *body)
{
	return body->GetLinearDamping();
}

void b2body_setlineardamping (cb2body *body, float damping)
{
	body->SetLinearDamping(damping);
}

float b2body_getangulardamping (cb2body *body)
{
	return body->GetAngularDamping();
}

void b2body_setangulardamping (cb2body *body, float damping)
{
	return body->SetAngularDamping(damping);
}

void b2body_settype (cb2body *body, int type)
{
	body->SetType((b2BodyType)type);
}

int b2body_gettype (cb2body *body)
{
	return body->GetType();
}

void b2body_setbullet (cb2body *body, bool bullet)
{
	body->SetBullet (bullet);
}

bool b2body_getbullet (cb2body *body)
{
	return body->IsBullet();
}

void b2body_setissleepingallowed (cb2body *body, bool bullet)
{
	body->SetSleepingAllowed(bullet);
}

bool b2body_getissleepingallowed (cb2body *body)
{
	return body->IsSleepingAllowed();
}

void b2body_setawake (cb2body *body, bool bullet)
{
	body->SetAwake(bullet);
}

bool b2body_getawake (cb2body *body)
{
	return body->IsAwake();
}

void b2body_setactive (cb2body *body, bool bullet)
{
	body->SetActive(bullet);
}

bool b2body_getactive (cb2body *body)
{
	return body->IsActive();
}

void b2body_setfixedrotation (cb2body *body, bool bullet)
{
	body->SetFixedRotation(bullet);
}

bool b2body_getfixedrotation (cb2body *body)
{
	return body->IsFixedRotation();
}

cb2fixture *b2body_getfixturelist (cb2body *body)
{
	return body->GetFixtureList();
}

cb2jointedge *b2body_getjointlist (cb2body *body)
{
	return body->GetJointList();
}

cb2contactedge *b2body_getcontactlist (cb2body *body)
{
	return body->GetContactList();
}

void *b2body_getuserdata (cb2body *body)
{
	return body->GetUserData();
}

void b2body_setuserdata (cb2body *body, void *data)
{
	body->SetUserData(data);
}

cb2world *b2body_getworld (cb2body *body)
{
	return body->GetWorld();
}


cb2body *b2jointedge_getother (cb2jointedge *edge)
{
	return edge->other;
}

cb2joint *b2jointedge_getjoint (cb2jointedge *edge)
{
	return edge->joint;
}

cb2jointedge *b2jointedge_getprev (cb2jointedge *edge)
{
	return edge->prev;
}

cb2jointedge *b2jointedge_getnext (cb2jointedge *edge)
{
	return edge->next;
}

cb2body *b2contactedge_getother (cb2contactedge *edge)
{
	return edge->other;
}

cb2contact *b2contactedge_getcontact (cb2contactedge *edge)
{
	return edge->contact;
}

cb2contactedge *b2contactedge_getprev (cb2contactedge *edge)
{
	return edge->prev;
}

cb2contactedge *b2contactedge_getnext (cb2contactedge *edge)
{
	return edge->next;
}


void b2contact_getmanifold (cb2contact *contact, cb2manifold *manifold)
{
	*manifold = *contact->GetManifold();
}

void b2contact_getworldmanifold (cb2contact *contact, cb2worldmanifold *worldManifold)
{
	contact->GetWorldManifold(worldManifold);
}

bool b2contact_istouching (cb2contact *contact)
{
	return contact->IsTouching();
}

void b2contact_setenabled (cb2contact *contact, bool flag)
{
	contact->SetEnabled(flag);
}

bool b2contact_getenabled (cb2contact *contact)
{
	return contact->IsEnabled();
}

cb2contact *b2contact_getnext (cb2contact *contact)
{
	return contact->GetNext();
}

cb2fixture *b2contact_getfixturea (cb2contact *contact)
{
	return contact->GetFixtureA();
}

cb2fixture *b2contact_getfixtureb (cb2contact *contact)
{
	return contact->GetFixtureB();
}

void b2contact_evaluate (cb2contact *contact, cb2manifold *outManifold, cb2transform xfA, cb2transform xfB)
{
	contact->Evaluate(outManifold, xfA, xfB);
}

QUICK_GETTER(
	b2joint_gettype,
	cb2joint,
	int,
	GetType());

QUICK_GETTER(
	b2joint_getbodya,
	cb2joint,
	cb2body*,
	GetBodyA());

QUICK_GETTER(
	b2joint_getbodyb,
	cb2joint,
	cb2body*,
	GetBodyB());

QUICK_GETTER_PTR(
	b2joint_getanchora,
	cb2joint,
	cb2vec2,
	GetAnchorA());

QUICK_GETTER_PTR(
	b2joint_getanchorb,
	cb2joint,
	cb2vec2,
	GetAnchorB());

void b2joint_getreactionforce (cb2joint *joint, float inv_dt, cb2vec2 *outVar)
{
	*outVar = joint->GetReactionForce(inv_dt);
}

float b2joint_getreactiontorque (cb2joint *joint, float inv_dt)
{
	return joint->GetReactionTorque(inv_dt);
}

// This is a workaround to get m_collideConnected from b2Joint.
class b2stupid
{
public:
	virtual ~b2stupid() {}

	b2JointType m_type;
	b2Joint* m_prev;
	b2Joint* m_next;
	b2JointEdge m_edgeA;
	b2JointEdge m_edgeB;
	b2Body* m_bodyA;
	b2Body* m_bodyB;

	bool m_islandFlag;
	bool m_collideConnected;

	void* m_userData;

	b2Vec2 m_localCenterA, m_localCenterB;
	float32 m_invMassA, m_invIA;
	float32 m_invMassB, m_invIB;
};

bool b2joint_getcollideconnected (cb2joint *joint)
{
	b2stupid *stupid = reinterpret_cast<b2stupid*>(joint);
	return stupid->m_collideConnected;
}

QUICK_GETTER(
	b2joint_getnext,
	cb2joint,
	cb2joint*,
	GetNext());

QUICK_GET_SETTER_FUNC(
	b2joint_getuserdata,
	b2joint_setuserdata,
	cb2joint,
	void*,
	GetUserData,
	SetUserData);

QUICK_GETTER(
	b2joint_getisactive,
	cb2joint,
	bool,
	IsActive());


QUICK_GET_SETTER_FUNC(
	b2gearjoint_getratio,
	b2gearjoint_setratio,
	cb2gearjoint,
	float,
	GetRatio,
	SetRatio);

// FIXME. Another hack.
class b2stupidgear : public b2Joint
{
public:
	b2Body* m_ground1;
	b2Body* m_ground2;

	// One of these is NULL.
	b2RevoluteJoint* m_revolute1;
	b2PrismaticJoint* m_prismatic1;

	// One of these is NULL.
	b2RevoluteJoint* m_revolute2;
	b2PrismaticJoint* m_prismatic2;

	b2Vec2 m_groundAnchor1;
	b2Vec2 m_groundAnchor2;

	b2Vec2 m_localAnchor1;
	b2Vec2 m_localAnchor2;

	b2Jacobian m_J;

	float32 m_constant;
	float32 m_ratio;

	// Effective mass
	float32 m_mass;

	// Impulse for accumulation/warm starting.
	float32 m_impulse;
};

cb2joint *b2gearjoint_getjointa (cb2gearjoint *joint)
{
	b2stupidgear *stupid = reinterpret_cast<b2stupidgear*>(joint);
	return (stupid->m_prismatic1 == NULL) ? (cb2joint*)stupid->m_revolute1 : (cb2joint*)stupid->m_prismatic1;
}

cb2joint *b2gearjoint_getjointb (cb2gearjoint *joint)
{
	b2stupidgear *stupid = reinterpret_cast<b2stupidgear*>(joint);
	return (stupid->m_prismatic2 == NULL) ? (cb2joint*)stupid->m_revolute2 : (cb2joint*)stupid->m_prismatic2;
}

QUICK_GET_SETTER_FUNC(
	b2distancejoint_getlength,
	b2distancejoint_setlength,
	cb2distancejoint,
	float,
	GetLength,
	SetLength);

QUICK_GET_SETTER_FUNC(
	b2distancejoint_getfrequency,
	b2distancejoint_setfrequency,
	cb2distancejoint,
	float,
	GetFrequency,
	SetFrequency);

QUICK_GET_SETTER_FUNC(
	b2distancejoint_getdampingratio,
	b2distancejoint_setdampingratio,
	cb2distancejoint,
	float,
	GetDampingRatio,
	SetDampingRatio);


QUICK_GET_SETTER_FUNC(
	b2frictionjoint_getmaxforce,
	b2frictionjoint_setmaxforce,
	cb2frictionjoint,
	float,
	GetMaxForce,
	SetMaxForce);

QUICK_GET_SETTER_FUNC(
	b2frictionjoint_getmaxtorque,
	b2frictionjoint_setmaxtorque,
	cb2frictionjoint,
	float,
	GetMaxTorque,
	SetMaxTorque);


QUICK_GETTER(
	b2pulleyjoint_getlength1,
	cb2pulleyjoint,
	float,
	GetLength1());

QUICK_GETTER(
	b2pulleyjoint_getlength2,
	cb2pulleyjoint,
	float,
	GetLength2());

QUICK_GETTER(
	b2pulleyjoint_getratio,
	cb2pulleyjoint,
	float,
	GetRatio());


QUICK_GET_SETTER_FUNC_PTR(
	b2mousejoint_gettarget,
	b2mousejoint_settarget,
	cb2mousejoint,
	cb2vec2,
	GetTarget,
	SetTarget);

QUICK_GET_SETTER_FUNC(
	b2mousejoint_getmaxforce,
	b2mousejoint_setmaxforce,
	cb2mousejoint,
	float,
	GetMaxForce,
	SetMaxForce);

QUICK_GET_SETTER_FUNC(
	b2mousejoint_getfrequency,
	b2mousejoint_setfrequency,
	cb2mousejoint,
	float,
	GetFrequency,
	SetFrequency);

QUICK_GET_SETTER_FUNC(
	b2mousejoint_getdampingratio,
	b2mousejoint_setdampingratio,
	cb2mousejoint,
	float,
	GetDampingRatio,
	SetDampingRatio);


QUICK_GETTER(
	b2linejoint_getjointtranslation,
	cb2linejoint,
	float,
	GetJointTranslation());

QUICK_GETTER(
	b2linejoint_getjointspeed,
	cb2linejoint,
	float,
	GetJointSpeed());

QUICK_GET_SETTER_FUNC(
	b2linejoint_getenablelimit,
	b2linejoint_setenablelimit,
	cb2linejoint,
	bool,
	IsLimitEnabled,
	EnableLimit);

QUICK_GETTER(
	b2linejoint_getlowerlimit,
	cb2linejoint,
	float,
	GetLowerLimit());

QUICK_GETTER(
	b2linejoint_getupperlimit,
	cb2linejoint,
	float,
	GetUpperLimit());

void b2linejoint_setlimits (cb2linejoint *joint, float lower, float upper)
{
	joint->SetLimits(lower, upper);
}

QUICK_GET_SETTER_FUNC(
	b2linejoint_getenablemotor,
	b2linejoint_setenablemotor,
	cb2linejoint,
	bool,
	IsMotorEnabled,
	EnableMotor);

QUICK_GET_SETTER_FUNC(
	b2linejoint_getmotorspeed,
	b2linejoint_setmotorspeed,
	cb2linejoint,
	float,
	GetMotorSpeed,
	SetMotorSpeed);

QUICK_SETTER_FUNC(
	b2linejoint_setmaxmotorforce,
	cb2linejoint,
	float,
	SetMaxMotorForce);

QUICK_GETTER(
	b2linejoint_getmotorforce,
	cb2linejoint,
	float,
	GetMotorForce());


QUICK_GETTER(
	b2revolutejoint_getjointangle,
	cb2revolutejoint,
	float,
	GetJointAngle());

QUICK_GETTER(
	b2revolutejoint_getjointspeed,
	cb2revolutejoint,
	float,
	GetJointSpeed());

QUICK_GET_SETTER_FUNC(
	b2revolutejoint_getenablelimit,
	b2revolutejoint_setenablelimit,
	cb2revolutejoint,
	bool,
	IsLimitEnabled,
	EnableLimit);

QUICK_GETTER(
	b2revolutejoint_getlowerlimit,
	cb2revolutejoint,
	float,
	GetLowerLimit());

QUICK_GETTER(
	b2revolutejoint_getupperlimit,
	cb2revolutejoint,
	float,
	GetUpperLimit());

void b2revolutejoint_setlimits (cb2revolutejoint *joint, float lower, float upper)
{
	joint->SetLimits(lower, upper);
}

QUICK_GET_SETTER_FUNC(
	b2revolutejoint_getenablemotor,
	b2revolutejoint_setenablemotor,
	cb2revolutejoint,
	bool,
	IsMotorEnabled,
	EnableMotor);

QUICK_GET_SETTER_FUNC(
	b2revolutejoint_getmotorspeed,
	b2revolutejoint_setmotorspeed,
	cb2revolutejoint,
	float,
	GetMotorSpeed,
	SetMotorSpeed);

QUICK_SETTER_FUNC(
	b2revolutejoint_setmaxmotortorque,
	cb2revolutejoint,
	float,
	SetMaxMotorTorque);

QUICK_GETTER(
	b2revolutejoint_getmotortorque,
	cb2revolutejoint,
	float,
	GetMotorTorque());


QUICK_GETTER(
	b2prismaticjoint_getjointtranslation,
	cb2prismaticjoint,
	float,
	GetJointTranslation());

QUICK_GETTER(
	b2prismaticjoint_getjointspeed,
	cb2prismaticjoint,
	float,
	GetJointSpeed());

/*QUICK_GET_SETTER_FUNC(
	b2prismaticjoint_getenablelimit,
	b2prismaticjoint_setenablelimit,
	cb2prismaticjoint,
	bool,
	IsLimitEnabled,
	EnableLimit);*/

bool b2prismaticjoint_getenablelimit (cb2prismaticjoint *j)
{
	return j->IsLimitEnabled();
}

void b2prismaticjoint_setenablelimit (cb2prismaticjoint *j, bool v)
{
	j->EnableLimit(v);
}

QUICK_GETTER(
	b2prismaticjoint_getlowerlimit,
	cb2prismaticjoint,
	float,
	GetLowerLimit());

QUICK_GETTER(
	b2prismaticjoint_getupperlimit,
	cb2prismaticjoint,
	float,
	GetUpperLimit());

void b2prismaticjoint_setlimits (cb2prismaticjoint *joint, float lower, float upper)
{
	joint->SetLimits(lower, upper);
}

QUICK_GET_SETTER_FUNC(
	b2prismaticjoint_getenablemotor,
	b2prismaticjoint_setenablemotor,
	cb2prismaticjoint,
	bool,
	IsMotorEnabled,
	EnableMotor);

QUICK_GET_SETTER_FUNC(
	b2prismaticjoint_getmotorspeed,
	b2prismaticjoint_setmotorspeed,
	cb2prismaticjoint,
	float,
	GetMotorSpeed,
	SetMotorSpeed);

QUICK_SETTER_FUNC(
	b2prismaticjoint_setmaxmotorforce,
	cb2prismaticjoint,
	float,
	SetMaxMotorForce);

QUICK_GETTER(
	b2prismaticjoint_getmotorforce,
	cb2prismaticjoint,
	float,
	GetMotorForce());

void b2version_get (cb2version *outVersion)
{
	*outVersion = b2_version;
}


// Globals
void cb2_collidecircles (cb2manifold *manifold, cb2circleshapeportable *circle1, cb2transform xf1, cb2circleshapeportable *circle2, cb2transform xf2)
{
	b2CircleShape	sh1 = CircleShapeFromPortableCircleShape(circle1),
					sh2 = CircleShapeFromPortableCircleShape(circle2);

	b2CollideCircles(manifold, &sh1, xf1, &sh2, xf2);
}

void cb2_collidepolygonandcircle (cb2manifold *manifold, cb2polygonshapeportable *polygon, cb2transform xf1, cb2circleshapeportable *circle, cb2transform xf2)
{
	b2PolygonShape	sh1 = PolygonShapeFromPortableCircleShape(polygon);
	b2CircleShape	sh2 = CircleShapeFromPortableCircleShape(circle);

	b2CollidePolygonAndCircle(manifold, &sh1, xf1, &sh2, xf2);
}

void cb2_collidepolygons (cb2manifold *manifold, cb2polygonshapeportable *polygon1, cb2transform xf1, cb2polygonshapeportable *polygon2, cb2transform xf2)
{
	b2PolygonShape	sh1 = PolygonShapeFromPortableCircleShape(polygon1);
	b2PolygonShape	sh2 = PolygonShapeFromPortableCircleShape(polygon2);

	b2CollidePolygons(manifold, &sh1, xf1, &sh2, xf2);
}

int cb2_clipsegmenttoline(cb2clipvertex *vOut, cb2clipvertex vIn[2], cb2vec2 normal, float offset)
{
	return b2ClipSegmentToLine(vOut, vIn, normal, offset);
}

bool cb2_testoverlap (cb2shapeportable *shapeA, cb2shapeportable *shapeB, cb2transform xfA, cb2transform xfB)
{
	b2Shape *shapeAReal = ShapeFromPortableShape(shapeA);
	b2Shape *shapeBReal = ShapeFromPortableShape(shapeB);

	bool rV = b2TestOverlap(shapeAReal, shapeBReal, xfA, xfB);

	delete shapeAReal;
	delete shapeBReal;

	return rV;
}