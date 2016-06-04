// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Provides helper methods for vehicle simulation.
  /// </summary>
  public static class VehicleHelper
  {
    /// <summary>
    /// Sets the steering angles for a standard 4 wheel car.
    /// </summary>
    /// <param name="steeringAngle">The steering angle.</param>
    /// <param name="frontLeft">The front left wheel.</param>
    /// <param name="frontRight">The front right wheel.</param>
    /// <param name="backLeft">The back left wheel.</param>
    /// <param name="backRight">The back right wheel.</param>
    /// <remarks>
    /// In a real car, the steerable front wheels do not always have the same steering angle. Have a
    /// look at http://www.asawicki.info/Mirror/Car%20Physics%20for%20Games/Car%20Physics%20for%20Games.html
    /// (section "Curves") for an explanation. The steering angle defines the angle of the inner
    /// wheel. The outer wheel is adapted. This works only for 4 wheels in a normal car setup.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="frontLeft"/>, <paramref name="frontRight"/>, <paramref name="backLeft"/>, or
    /// <paramref name="backRight"/> is <see langword="null"/>.
    /// </exception>
    public static void SetCarSteeringAngle(float steeringAngle, ConstraintWheel frontLeft, ConstraintWheel frontRight, ConstraintWheel backLeft, ConstraintWheel backRight)
    {
      if (frontLeft == null)
        throw new ArgumentNullException("frontLeft");
      if (frontRight == null)
        throw new ArgumentNullException("frontRight");
      if (backLeft == null)
        throw new ArgumentNullException("backLeft");
      if (backRight == null)
        throw new ArgumentNullException("backRight");

      backLeft.SteeringAngle = 0;
      backRight.SteeringAngle = 0;

      if (Numeric.IsZero(steeringAngle))
      {
        frontLeft.SteeringAngle = 0;
        frontRight.SteeringAngle = 0;
        return;
      }

      ConstraintWheel inner, outer;
      if (steeringAngle > 0)
      {
        inner = frontLeft;
        outer = frontRight;
      }
      else
      {
        inner = frontRight;
        outer = frontLeft;
      }

      inner.SteeringAngle = steeringAngle;

      float backToFront = backLeft.Offset.Z - frontLeft.Offset.Z;
      float rightToLeft = frontRight.Offset.X - frontLeft.Offset.X;

      float innerAngle = Math.Abs(steeringAngle);
      float outerAngle = (float)Math.Atan2(backToFront, backToFront / Math.Tan(innerAngle) + rightToLeft);

      outer.SteeringAngle = Math.Sign(steeringAngle) * outerAngle;
    }
  }
}
