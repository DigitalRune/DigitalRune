// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Provides constraint-related helper methods.
  /// </summary>
  public static class ConstraintHelper
  {
    /// <summary>
    /// Computes the error reduction parameter for a given spring and damping constant.
    /// </summary>
    /// <param name="deltaTime">The time step size.</param>
    /// <param name="springConstant">The spring constant.</param>
    /// <param name="dampingConstant">The damping constant.</param>
    /// <returns>
    /// The error reduction parameter that lets the constraint behave like a damped spring with the
    /// given parameters.
    /// </returns>
    public static float ComputeErrorReduction(float deltaTime, float springConstant, float dampingConstant)
    {
      // The returned value must be divided by dt, so the multiplication with dt could be optimized.
      float denominator = (deltaTime * springConstant + dampingConstant);

      // Return a save value.
      if (Numeric.IsZero(denominator) || Numeric.IsNaN(denominator))
        return 0;

      return deltaTime * springConstant / denominator;
    }


    /// <summary>
    /// Computes the softness parameter for a given spring and damping constant.
    /// </summary>
    /// <param name="deltaTime">The time step size.</param>
    /// <param name="springConstant">The spring constant.</param>
    /// <param name="dampingConstant">The damping constant.</param>
    /// <returns>
    /// The softness parameter that lets the constraint behave like a damped spring with the given
    /// parameters.
    /// </returns>
    public static float ComputeSoftness(float deltaTime, float springConstant, float dampingConstant)
    {
      // The returned value must be divided by dt, so the multiplication with dt could be optimized.
      float denominator = (deltaTime * springConstant + dampingConstant);

      // Return a save value.
      if (Numeric.IsZero(denominator) || Numeric.IsNaN(denominator))
        return 1;

      // The returned value is the CFM value that must be divided by dt as usual.
      return 1 / denominator;
    }


    /// <summary>
    /// Computes the spring constant from error reduction and softness parameters.
    /// </summary>
    /// <param name="deltaTime">The time step size.</param>
    /// <param name="errorReduction">The error reduction parameter.</param>
    /// <param name="softness">The softness parameter.</param>
    /// <returns>The spring constant.</returns>
    public static float ComputeSpringConstant(float deltaTime, float errorReduction, float softness)
    {
      return errorReduction / (softness * deltaTime);
    }


    /// <summary>
    /// Computes the damping constant from error reduction and softness parameters.
    /// </summary>
    /// <param name="deltaTime">The time step size.</param>
    /// <param name="errorReduction">The error reduction parameter.</param>
    /// <param name="softness">The softness parameter.</param>
    /// <returns>The damping constant.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    public static float ComputeDampingConstant(float deltaTime, float errorReduction, float softness)
    {
      return 1 / softness - errorReduction / softness;
    }


    /// <summary>
    /// Computes the K matrix needed by sequential impulse-based methods.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="positionWorld">The constraint anchor position in world space.</param>
    /// <returns>The K matrix.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="body"/> is <see langword="null"/>.
    /// </exception>
    public static Matrix33F ComputeKMatrix(this RigidBody body, Vector3F positionWorld)
    {
      if (body == null)
        throw new ArgumentNullException("body");

      if (body.MotionType != MotionType.Dynamic)
        return Matrix33F.Zero;

      Vector3F radiusVector = positionWorld - body.PoseCenterOfMass.Position;
      Matrix33F skewR = radiusVector.ToCrossProductMatrix();
      Matrix33F massMatrixInverse = Matrix33F.CreateScale(body.MassInverse);
      Matrix33F kMatrix = massMatrixInverse - skewR * body.InertiaInverseWorld * skewR;
      return kMatrix;
    }


    /// <summary>
    /// Applies an impulse so that the velocity of point on the body is changed.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="positionWorld">The position on the body in world space.</param>
    /// <param name="velocityWorld">The target velocity of the point in world space.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="body"/> is <see langword="null"/>.
    /// </exception>
    public static void SetVelocityOfWorldPoint(this RigidBody body, Vector3F positionWorld, Vector3F velocityWorld)
    {
      if (body == null)
        throw new ArgumentNullException("body");

      Vector3F oldVelocityWorld = body.GetVelocityOfWorldPoint(positionWorld);
      var matrixK = ComputeKMatrix(body, positionWorld);
      var constraintImpulse = matrixK.Inverse * ((velocityWorld - oldVelocityWorld));
      body.ApplyImpulse(constraintImpulse, positionWorld);
    }


    /// <summary>
    /// Gets the Euler angles for the given rotation.
    /// </summary>
    /// <param name="rotation">The rotation.</param>
    /// <returns>
    /// A vector with the three Euler angles in radians: (angle0, angle1, angle2).
    /// </returns>
    /// <remarks>
    /// <para>
    /// The Euler angles are computed for following order of rotations: The first rotations is about
    /// the x-axis. The second rotation is about the rotated y-axis after the first rotation. The
    /// last rotation is about the final z-axis.
    /// </para>
    /// <para>
    /// The Euler angles are unique if the second angle is less than +/- 90°. The limits for the
    /// rotation angles are [-180°, 180°] for the first and the third angle. And the limit for the
    /// second angle is [-90°, 90°].
    /// </para>
    /// </remarks>
    public static Vector3F GetEulerAngles(Matrix33F rotation)
    {
      // See book Geometric Tools, "Factoring Rotation Matrices as RxRyRz", pp. 848.
      Vector3F result = new Vector3F();

      float sinY = rotation.M02;
      if (sinY < 1.0f)
      {
        if (sinY > -1.0f)
        {
          result.X = (float)Math.Atan2(-rotation.M12, rotation.M22);
          result.Y = (float)Math.Asin(sinY);
          result.Z = (float)Math.Atan2(-rotation.M01, rotation.M00);
        }
        else
        {
          // Not a unique solution (thetaX - thetaZ is constant).
          result.X = -(float)Math.Atan2(rotation.M10, rotation.M11);
          result.Y = -ConstantsF.PiOver2;
          result.Z = 0;
        }
      }
      else
      {
        // Not a unique solution (thetaX + thetaZ is constant).
        result.X = (float)Math.Atan2(rotation.M10, rotation.M11);
        result.Y = ConstantsF.PiOver2;
        result.Z = 0;
      }

      return result;
    }
  }
}
