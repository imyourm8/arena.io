using System;
using System.Collections.Generic;

namespace Box2CS
{
	public static class Box2DSettings
	{
		/// The maximum number of contact points between two convex shapes.
		public const int b2_maxManifoldPoints	= 2;

		/// The maximum number of vertices on a convex polygon.
		public const int b2_maxPolygonVertices	= 8;

		/// This is used to fatten AABBs in the dynamic tree. This allows proxies
		/// to move by a small amount without triggering a tree adjustment.
		/// This is in meters.
		public const float b2_aabbExtension		= 0.1f;

		/// This is used to fatten AABBs in the dynamic tree. This is used to predict
		/// the future position based on the current displacement.
		/// This is a dimensionless multiplier.
		public const float b2_aabbMultiplier		= 2.0f;

		/// A small length used as a collision and constraint tolerance. Usually it is
		/// chosen to be numerically significant, but visually insignificant.
		public const float b2_linearSlop			= 0.005f;

		/// A small angle used as a collision and constraint tolerance. Usually it is
		/// chosen to be numerically significant, but visually insignificant.
		public const float b2_angularSlop			= (float)(2.0 / 180.0 * Math.PI);

		/// The radius of the polygon/edge shape skin. This should not be modified. Making
		/// this smaller means polygons will have an insufficient buffer for continuous collision.
		/// Making it larger may create artifacts for vertex collision.
		public const float b2_polygonRadius		= (2.0f * b2_linearSlop);


		// Dynamics

		/// Maximum number of contacts to be handled to solve a TOI impact.
		public const int b2_maxTOIContacts			= 32;

		/// A velocity threshold for elastic collisions. Any collision with a relative linear
		/// velocity below this threshold will be treated as inelastic.
		public const float b2_velocityThreshold		= 1.0f;

		/// The maximum linear position correction used when solving constraints. This helps to
		/// prevent overshoot.
		public const float b2_maxLinearCorrection		= 0.2f;

		/// The maximum angular position correction used when solving constraints. This helps to
		/// prevent overshoot.
		public const float b2_maxAngularCorrection		= (float)(8.0 / 180.0 * Math.PI);

		/// The maximum linear velocity of a body. This limit is very large and is used
		/// to prevent numerical problems. You shouldn't need to adjust this.
		public const float b2_maxTranslation			= 2.0f;
		public const float b2_maxTranslationSquared	= (b2_maxTranslation * b2_maxTranslation);

		/// The maximum angular velocity of a body. This limit is very large and is used
		/// to prevent numerical problems. You shouldn't need to adjust this.
		public const float b2_maxRotation				= (float)(0.5 * Math.PI);
		public const float b2_maxRotationSquared		= (b2_maxRotation * b2_maxRotation);

		/// This scale factor controls how fast overlap is resolved. Ideally this would be 1 so
		/// that overlap is removed in one time step. However using values close to 1 often lead
		/// to overshoot.
		public const float b2_contactBaumgarte			= 0.2f;

		// Sleep

		/// The time that a body must be still before it will go to sleep.
		public const float b2_timeToSleep				= 0.5f;

		/// A body cannot sleep if its linear velocity is above this tolerance.
		public const float b2_linearSleepTolerance		= 0.01f;

		/// A body cannot sleep if its angular velocity is above this tolerance.
		public const float b2_angularSleepTolerance = (float)(2.0 / 180.0 * Math.PI);

		public const string Box2CDLLName = "box2c.dll";
	}
}
