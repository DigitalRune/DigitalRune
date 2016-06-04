// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Wraps a 1-dimensional constraint (a row in the constraint solver matrix).
  /// </summary>
  internal sealed class Constraint1D
  {
    // Notes:
    // We use following conventions:
    // The same impulse (same sign!) is applied on A and B. 
    // If the constraint axis points from A to B:
    // A positive impulse creates a positive constraint velocity.
    // A positive relative velocity means A and B are separating.
    // A negative relative velocity means A and B are getting closer.
    //
    // Constraint Force Mixing (CFM)/Softness:
    // Softness is added to the diagonal of JWJT and is also considered when the impulse
    // is computed with
    //   impulse = JWJTInverse * (TargetRelativeVelocity - relativeVelocity - Softness * ConstraintImpulse);
    // This is derived in http://bulletphysics.org/Bullet/phpBB3/viewtopic.php?f=4&t=1354&hilit=cfm.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The Jacobian matrices J.
    public Vector3F JLinA;
    public Vector3F JAngA;
    public Vector3F JLinB;
    public Vector3F JAngB;

    // The terms: M^-1 * J^T. 
    // M^-1 is often written as W.
    public Vector3F WJTLinA;
    public Vector3F WJTAngA;
    public Vector3F WJTLinB;
    public Vector3F WJTAngB;

    // The term: (J * M^-1 * J^T)^-1. This is equal to the collision matrix K^-1.
    public float JWJTInverse;

    // The target relative velocity which is usually: 0 + error reduction + bounce velocity + motor velocities.
    public float TargetRelativeVelocity;

    // This softness is already divided by dt!
    public float Softness;

    // The total constraint impulse (the sum of all sequential impulses per time step).
    public float ConstraintImpulse;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes the 1-dimensional constraint.
    /// </summary>
    public void Prepare(RigidBody bodyA, RigidBody bodyB, Vector3F jLinA, Vector3F jAngA, Vector3F jLinB, Vector3F jAngB)
    {
      JLinA = jLinA;
      JAngA = jAngA;
      JLinB = jLinB;
      JAngB = jAngB;

      //WJTLinA = bodyA.MassInverse * jLinA;
      //WJTAngA = bodyA.InertiaInverseWorld * jAngA;
      //WJTLinB = bodyB.MassInverse * jLinB;
      //WJTAngB = bodyB.InertiaInverseWorld * jAngB;
      // ----- Optimized version:
      WJTLinA.X = bodyA.MassInverse * jLinA.X;
      WJTLinA.Y = bodyA.MassInverse * jLinA.Y;
      WJTLinA.Z = bodyA.MassInverse * jLinA.Z;
      Matrix33F matrix = bodyA.InertiaInverseWorld;
      WJTAngA.X = matrix.M00 * jAngA.X + matrix.M01 * jAngA.Y + matrix.M02 * jAngA.Z;
      WJTAngA.Y = matrix.M10 * jAngA.X + matrix.M11 * jAngA.Y + matrix.M12 * jAngA.Z;
      WJTAngA.Z = matrix.M20 * jAngA.X + matrix.M21 * jAngA.Y + matrix.M22 * jAngA.Z;
      WJTLinB.X = bodyB.MassInverse * jLinB.X;
      WJTLinB.Y = bodyB.MassInverse * jLinB.Y;
      WJTLinB.Z = bodyB.MassInverse * jLinB.Z;
      matrix = bodyB.InertiaInverseWorld;
      WJTAngB.X = matrix.M00 * jAngB.X + matrix.M01 * jAngB.Y + matrix.M02 * jAngB.Z;
      WJTAngB.Y = matrix.M10 * jAngB.X + matrix.M11 * jAngB.Y + matrix.M12 * jAngB.Z;
      WJTAngB.Z = matrix.M20 * jAngB.X + matrix.M21 * jAngB.Y + matrix.M22 * jAngB.Z;

      //float JWJT = Vector3F.Dot(jLinA, WJTLinA) + Vector3F.Dot(jAngA, WJTAngA)
      //             + Vector3F.Dot(jLinB, WJTLinB) + Vector3F.Dot(jAngB, WJTAngB);
      // ----- Optimized version:
      float JWJT = jLinA.X * WJTLinA.X + jLinA.Y * WJTLinA.Y + jLinA.Z * WJTLinA.Z
                   + jAngA.X * WJTAngA.X + jAngA.Y * WJTAngA.Y + jAngA.Z * WJTAngA.Z
                   + jLinB.X * WJTLinB.X + jLinB.Y * WJTLinB.Y + jLinB.Z * WJTLinB.Z
                   + jAngB.X * WJTAngB.X + jAngB.Y * WJTAngB.Y + jAngB.Z * WJTAngB.Z;

      JWJT += Softness;
      JWJTInverse = 1 / JWJT;
    }


    /// <summary>
    /// Applies the given constraint impulse.
    /// </summary>
    private void ApplyImpulse(RigidBody bodyA, RigidBody bodyB, float impulse)
    {
      // u2 = u1 + M^-1 * J^T * lambda
      //bodyA._linearVelocity += WJTLinA * impulse;
      //bodyA._angularVelocity += WJTAngA * impulse;
      //bodyB._linearVelocity += WJTLinB * impulse;
      //bodyB._angularVelocity += WJTAngB * impulse;

      // ----- Optimized version:
      bodyA._linearVelocity.X += WJTLinA.X * impulse;
      bodyA._linearVelocity.Y += WJTLinA.Y * impulse;
      bodyA._linearVelocity.Z += WJTLinA.Z * impulse;
      bodyA._angularVelocity.X += WJTAngA.X * impulse;
      bodyA._angularVelocity.Y += WJTAngA.Y * impulse;
      bodyA._angularVelocity.Z += WJTAngA.Z * impulse;

      bodyB._linearVelocity.X += WJTLinB.X * impulse;
      bodyB._linearVelocity.Y += WJTLinB.Y * impulse;
      bodyB._linearVelocity.Z += WJTLinB.Z * impulse;
      bodyB._angularVelocity.X += WJTAngB.X * impulse;
      bodyB._angularVelocity.Y += WJTAngB.Y * impulse;
      bodyB._angularVelocity.Z += WJTAngB.Z * impulse;
    }


    /// <summary>
    /// Applies the given constraint impulse.
    /// </summary>
    private void ApplyCorrectionImpulse(RigidBody bodyA, RigidBody bodyB, float impulse)
    {
      // u2 = u1 + M^-1 * J^T * lambda
      //bodyA.LinearCorrectionVelocity += WJTLinA * impulse;
      //bodyA.AngularCorrectionVelocity += WJTAngA * impulse;
      //bodyB.LinearCorrectionVelocity += WJTLinB * impulse;
      //bodyB.AngularCorrectionVelocity += WJTAngB * impulse;

      // ----- Optimized version:
      bodyA.LinearCorrectionVelocity.X += WJTLinA.X * impulse;
      bodyA.LinearCorrectionVelocity.Y += WJTLinA.Y * impulse;
      bodyA.LinearCorrectionVelocity.Z += WJTLinA.Z * impulse;
      bodyA.AngularCorrectionVelocity.X += WJTAngA.X * impulse;
      bodyA.AngularCorrectionVelocity.Y += WJTAngA.Y * impulse;
      bodyA.AngularCorrectionVelocity.Z += WJTAngA.Z * impulse;

      bodyB.LinearCorrectionVelocity.X += WJTLinB.X * impulse;
      bodyB.LinearCorrectionVelocity.Y += WJTLinB.Y * impulse;
      bodyB.LinearCorrectionVelocity.Z += WJTLinB.Z * impulse;
      bodyB.AngularCorrectionVelocity.X += WJTAngB.X * impulse;
      bodyB.AngularCorrectionVelocity.Y += WJTAngB.Y * impulse;
      bodyB.AngularCorrectionVelocity.Z += WJTAngB.Z * impulse;
    }


    /// <summary>
    /// Gets the relative constraint velocity (ignoring error correction velocities).
    /// </summary>
    public float GetRelativeVelocity(RigidBody bodyA, RigidBody bodyB)
    {
      // relative velocity = J * u.
      //return Vector3F.Dot(JLinA, bodyA._linearVelocity)
      //       + Vector3F.Dot(JAngA, bodyA._angularVelocity)
      //       + Vector3F.Dot(JLinB, bodyB._linearVelocity)
      //       + Vector3F.Dot(JAngB, bodyB._angularVelocity);

      // ----- Optimized version:
      Vector3F linearVelocityA = bodyA._linearVelocity;
      Vector3F angularVelocityA = bodyA._angularVelocity;
      Vector3F linearVelocityB = bodyB._linearVelocity;
      Vector3F angularVelocityB = bodyB._angularVelocity;
      return JLinA.X * linearVelocityA.X + JLinA.Y * linearVelocityA.Y + JLinA.Z * linearVelocityA.Z
             + JAngA.X * angularVelocityA.X + JAngA.Y * angularVelocityA.Y + JAngA.Z * angularVelocityA.Z
             + JLinB.X * linearVelocityB.X + JLinB.Y * linearVelocityB.Y + JLinB.Z * linearVelocityB.Z
             + JAngB.X * angularVelocityB.X + JAngB.Y * angularVelocityB.Y + JAngB.Z * angularVelocityB.Z;
    }


    /// <summary>
    /// Gets the relative constraint velocity using only the error correction velocities.
    /// </summary>
    public float GetRelativeCorrectionVelocity(RigidBody bodyA, RigidBody bodyB)
    {
      // relative velocity = J * u.
      //return Vector3F.Dot(JLinA, bodyA.LinearCorrectionVelocity)
      //       + Vector3F.Dot(JAngA, bodyA.AngularCorrectionVelocity)
      //       + Vector3F.Dot(JLinB, bodyB.LinearCorrectionVelocity)
      //       + Vector3F.Dot(JAngB, bodyB.AngularCorrectionVelocity);
      
      // ----- Optimized version:
      Vector3F linearVelocityA = bodyA.LinearCorrectionVelocity;
      Vector3F angularVelocityA = bodyA.AngularCorrectionVelocity;
      Vector3F linearVelocityB = bodyB.LinearCorrectionVelocity;
      Vector3F angularVelocityB = bodyB.AngularCorrectionVelocity;
      return JLinA.X * linearVelocityA.X + JLinA.Y * linearVelocityA.Y + JLinA.Z * linearVelocityA.Z
             + JAngA.X * angularVelocityA.X + JAngA.Y * angularVelocityA.Y + JAngA.Z * angularVelocityA.Z
             + JLinB.X * linearVelocityB.X + JLinB.Y * linearVelocityB.Y + JLinB.Z * linearVelocityB.Z
             + JAngB.X * angularVelocityB.X + JAngB.Y * angularVelocityB.Y + JAngB.Z * angularVelocityB.Z;
    }


    /// <summary>
    /// Applies the current constraint impulse.
    /// </summary>
    public void Warmstart(RigidBody bodyA, RigidBody bodyB)
    {
      ApplyImpulse(bodyA, bodyB, ConstraintImpulse);
    }


    /// <summary>
    /// Satisfies a general constraint.
    /// </summary>
    public float SatisfyConstraint(RigidBody bodyA, RigidBody bodyB, float relativeVelocity, float minImpulseLimit, float maxImpulseLimit)
    {
      float impulse = JWJTInverse * (TargetRelativeVelocity - relativeVelocity - Softness * ConstraintImpulse);
      var oldCachedNormalImpulse = ConstraintImpulse;
      ConstraintImpulse = MathHelper.Clamp(ConstraintImpulse + impulse, minImpulseLimit, maxImpulseLimit);
      impulse = ConstraintImpulse - oldCachedNormalImpulse;

      ApplyImpulse(bodyA, bodyB, impulse);

      return impulse;
    }


    /// <summary>
    /// Satisfies an inequality constraint.
    /// </summary>
    public float SatisfyInequalityConstraint(RigidBody bodyA, RigidBody bodyB, float relativeVelocity, float minImpulseLimit)
    {
      // Accumulate/Clamp total impulse
      // Total impulse must be at least as big as the impulse required for restitution.
      // In this iteration we have to apply the missing impulse to get the current total impulse.
      float impulse = JWJTInverse * (TargetRelativeVelocity - relativeVelocity - Softness * ConstraintImpulse);
      var oldCachedNormalImpulse = ConstraintImpulse;
      ConstraintImpulse = Math.Max(minImpulseLimit, ConstraintImpulse + impulse);
      impulse = ConstraintImpulse - oldCachedNormalImpulse;

      ApplyImpulse(bodyA, bodyB, impulse);

      return impulse;
    }


    /// <summary>
    /// Satisfies a contact constraint.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    public float SatisfyContactConstraint(RigidBody bodyA, RigidBody bodyB, float relativeVelocity)
    {
      // Optimized version of SatisfyInequalityConstraint
      float impulse = JWJTInverse * (TargetRelativeVelocity - relativeVelocity);  // No softness!
      var oldCachedNormalImpulse = ConstraintImpulse;
      ConstraintImpulse = ConstraintImpulse + impulse;
      if (ConstraintImpulse < 0)
        ConstraintImpulse = 0;
      impulse = ConstraintImpulse - oldCachedNormalImpulse;

      // Optimized: Moved to ContactConstraint class:
      // Apply constraint impulse. 
      //ApplyImpulse(bodyA, bodyB, impulse);

      return impulse;
    }


    /// <summary>
    /// Satisfies an inequality constraint using only error correction velocities. This method
    /// applies a Split Impulse.
    /// </summary>
    public void CorrectErrors(RigidBody bodyA, RigidBody bodyB, float targetRelativeVelocity, ref float accumulatedImpulse)
    {
      // Similar to SatisfyInequalityConstraint, but instead a split impulse for contacts is applied.
      float relativeVelocity = GetRelativeCorrectionVelocity(bodyA, bodyB);
      float impulse = JWJTInverse * (targetRelativeVelocity - relativeVelocity);
      var oldCachedNormalImpulse = accumulatedImpulse;
      accumulatedImpulse = Math.Max(0, accumulatedImpulse + impulse);
      impulse = accumulatedImpulse - oldCachedNormalImpulse;

      ApplyCorrectionImpulse(bodyA, bodyB, impulse);
    }


    /// <summary>
    /// Satisfies a friction constraint.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    public float SatisfyFrictionConstraint(RigidBody bodyA, RigidBody bodyB, float relativeVelocity,
                                           float staticFrictionLimit, float normalImpulse, float dynamicFriction)
    {
      // Constraint impulse for target velocity = 0:
      float impulse = JWJTInverse * (/*0*/ - relativeVelocity /*- Softness * ConstraintImpulse*/);   // No softness

      // Clamp friction to static friction limit. If limit is reached we apply dynamic friction.
      float oldTotalFrictionImpulse = ConstraintImpulse;
      ConstraintImpulse = ConstraintImpulse + impulse;
      if (ConstraintImpulse > staticFrictionLimit)
        ConstraintImpulse = normalImpulse * dynamicFriction;
      else if (ConstraintImpulse < -staticFrictionLimit)
        ConstraintImpulse = -normalImpulse * dynamicFriction;

      impulse = ConstraintImpulse - oldTotalFrictionImpulse;

      // Optimized: Moved to ContactConstraint class:
      // Apply constraint impulse. 
      //ApplyImpulse(bodyA, bodyB, impulse);

      return impulse;
    }
    #endregion
  }
}
