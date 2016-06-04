// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Settings;


namespace DigitalRune.Physics
{
  public partial class RigidBody
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The time of impact. Used during motion clamping.
    /// </summary>
    internal float TimeOfImpact;  // The property is stored as a field because it needs to be 
    // synchronized using and Interlocked.CompareExchange.
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether Continuous Collision Detection (CCD) is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if CCD is enabled; otherwise, <see langword="false"/>.
    /// The default is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// See also <see cref="MotionSettings"/>. CCD can be used for small fast moving objects to
    /// detect all collisions, e.g. for bullets. The simulation will only perform CCD for pairs
    /// of rigid bodies where <see cref="CcdEnabled"/> is set for one of the bodies. If CCD is not
    /// enabled for this body, it can happen that this body moves through other bodies and the 
    /// collision is not detected - this problem only occurs for small/thin objects and fast moving 
    /// objects.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public bool CcdEnabled { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether CCD should be performed for this body in the 
    /// current time step.
    /// </summary>
    internal bool IsCcdActive { get; set; }  // If this flag is set, the AABB is the temporal AABB and CCD tests are active.


    /// <summary>
    /// Gets or sets the linear velocity of this body in world space.
    /// </summary>
    /// <value>The linear velocity in world space.</value>
    public Vector3F LinearVelocity
    {
      get { return _linearVelocity; }
      set
      {
        if (!value.IsNaN)
        {
          _linearVelocity = value;

          if (IsSleeping)
          {
            var velocitySquared = _linearVelocity.LengthSquared;
            if (MotionType == MotionType.Dynamic)
            {
              if (Simulation == null || velocitySquared > Simulation.Settings.Sleeping.LinearVelocityThresholdSquared)
                WakeUp();
            }
            else
            {
              if (velocitySquared > Numeric.EpsilonFSquared)
                WakeUp();
            }
          }
        }
        else
        {
          _linearVelocity = Vector3F.Zero;
        }
      }
    }
    internal Vector3F _linearVelocity;


    /// <summary>
    /// Gets or sets the angular velocity about the center of mass in world space.
    /// </summary>
    /// <value>The angular velocity in world space.</value>
    public Vector3F AngularVelocity
    {
      get { return _angularVelocity; }
      set
      {
        if (!value.IsNaN)
        {
          _angularVelocity = value;

          if (IsSleeping)
          {
            var velocitySquared = _angularVelocity.LengthSquared;
            if (MotionType == MotionType.Dynamic)
            {
              if (Simulation == null || velocitySquared > Simulation.Settings.Sleeping.AngularVelocityThresholdSquared)
                WakeUp();
            }
            else
            {
              if (velocitySquared > Numeric.EpsilonFSquared)
                WakeUp();
            }
          }
        }
        else
        {
          _angularVelocity = Vector3F.Zero;
        }
      }
    }
    internal Vector3F _angularVelocity;


    // Don't use this because if a value of the inertia tensor is infinite this can create
    // NaN. If we use InertiaInverse we already have valid values.
    //public Vector3F AngularMomentumWorld
    //{
    //  get 
    //  { 
    //    var result = InertiaWorld * AngularVelocity;
    //    return result;
    //  }
    //  set { AngularVelocity = InertiaInverseWorld * value; }
    //}


    /// <summary>
    /// Gets or sets the linear correction velocity.
    /// </summary>
    /// <value>The linear correction velocity.</value>
    /// <remarks>
    /// This is used for Split Impulses. Also known as push impulses, flash impulses, first order
    /// world impulses, etc. This velocity is set to 0 at the end of each time step.
    /// </remarks>
    internal Vector3F LinearCorrectionVelocity;  // A.k.a. bias velocity.


    /// <summary>
    /// Gets or sets the angular correction velocity.
    /// </summary>
    /// <value>The angular correction velocity.</value>
    /// <remarks>
    /// This is used for Split Impulses. Also known as push impulses, flash impulses, first order
    /// world impulses, etc. This velocity is set to 0 at the end of each time step.
    /// </remarks>
    internal Vector3F AngularCorrectionVelocity;


    /// <summary>
    /// Gets the rotational kinetic energy.
    /// </summary>
    /// <value>The rotational kinetic energy.</value>
    public float RotationalEnergy
    {
      get
      {
        Vector3F ω = AngularVelocity;
        Matrix33F inertia = InertiaWorld;

        // Rotational engergy Erot = 1/2 * ω^T * I * ω = 1/2 * ω ∙ angularMomentumWorld
        Vector3F angularMomentumWorld = inertia * ω;
        float energy = 1.0f / 2.0f * Vector3F.Dot(ω, angularMomentumWorld);
        return energy;
      }
    }


    /// <summary>
    /// Gets the translational kinetic energy.
    /// </summary>
    /// <value>The translational kinetic energy.</value>
    public float TranslationalEnergy
    {
      get
      {
        float velocity = LinearVelocity.Length;
        return 1.0f / 2.0f * MassFrame.Mass * velocity * velocity;
      }
    }


    /// <summary>
    /// Gets the kinetic energy.
    /// </summary>
    /// <value>The kinetic energy.</value>
    /// <remarks>
    /// The kinetic energy is the sum of the <see cref="TranslationalEnergy"/> and the
    /// <see cref="RotationalEnergy"/>.
    /// </remarks>
    public float KineticEnergy
    {
      get { return TranslationalEnergy + RotationalEnergy; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Applies an impulse at a given position.
    /// </summary>
    /// <param name="impulseWorld">The impulse in world space.</param>
    /// <param name="positionWorld">
    /// The position where the impulse is applied in world space.
    /// </param>
    public void ApplyImpulse(Vector3F impulseWorld, Vector3F positionWorld)
    {
      if (MotionType != MotionType.Dynamic)
        return;

      if (IsSleeping)
        WakeUp();

      Vector3F radius = positionWorld - PoseCenterOfMass.Position;
      LinearVelocity += MassInverse * impulseWorld;

      //AngularMomentumWorld += Vector3F.Cross(radius, impulseWorld);
      AngularVelocity += InertiaInverseWorld * Vector3F.Cross(radius, impulseWorld);
    }


    /// <summary>
    /// Applies a linear impulse at the center of mass
    /// </summary>
    /// <param name="impulseWorld">The impulse in world space.</param>
    public void ApplyLinearImpulse(Vector3F impulseWorld)
    {
      if (MotionType != MotionType.Dynamic)
        return;

      if (IsSleeping)
        WakeUp();

      LinearVelocity += MassInverse * impulseWorld;
    }


    /// <summary>
    /// Applies an angular impulse at the center of mass
    /// </summary>
    /// <param name="impulseWorld">The impulse in world space.</param>
    public void ApplyAngularImpulse(Vector3F impulseWorld)
    {
      if (MotionType != MotionType.Dynamic)
        return;

      if (IsSleeping)
        WakeUp();

      AngularVelocity += InertiaInverseWorld * impulseWorld;
    }


    /// <summary>
    /// Applies a correction impulse at a given position.
    /// </summary>
    /// <param name="impulseWorld">The impulse in world space.</param>
    /// <param name="positionWorld">
    /// The position where the impulse is applied in world space.
    /// </param>
    /// <remarks>
    /// Correction impulses are also known as push impulses, flash impulses, first order world 
    /// impulses etc. This is similar to <see cref="ApplyImpulse"/> but only the correction
    /// velocities (<see cref="LinearCorrectionVelocity"/> and 
    /// <see cref="AngularCorrectionVelocity"/>) are changed.
    /// </remarks>
    internal void ApplyCorrectionImpulse(Vector3F impulseWorld, Vector3F positionWorld)
    {
      if (MotionType != MotionType.Dynamic)
        return;

      Vector3F radius = positionWorld - PoseCenterOfMass.Position;
      LinearCorrectionVelocity += MassInverse * impulseWorld;
      AngularCorrectionVelocity += InertiaInverseWorld * Vector3F.Cross(radius, impulseWorld);
    }


    /// <summary>
    /// Gets the velocity of a point on the rigid body.
    /// </summary>
    /// <param name="positionWorld">The position of the point in world space.</param>
    /// <returns>
    /// The velocity of the point on the rigid body in world space.
    /// </returns>
    /// <remarks>
    /// This method computes the velocity of a point that is fixed on the moving body.
    /// </remarks>
    public Vector3F GetVelocityOfWorldPoint(Vector3F positionWorld)
    {
      return LinearVelocity + Vector3F.Cross(AngularVelocity, positionWorld - PoseCenterOfMass.Position);
    }


    /// <summary>
    /// Gets the velocity of a point on the rigid body.
    /// </summary>
    /// <param name="positionLocal">
    /// The position of the point in the local space of the rigid body.
    /// </param>
    /// <returns>
    /// The velocity of the point on the rigid body in world space.
    /// </returns>
    /// <remarks>
    /// This method computes the velocity of a point that is fixed on the moving body.
    /// </remarks>
    public virtual Vector3F GetVelocityOfLocalPoint(Vector3F positionLocal)
    {
      return GetVelocityOfWorldPoint(Pose.ToWorldPosition(positionLocal));
    }


    /// <summary>
    /// Updates the velocities using numerical integration.
    /// </summary>
    /// <param name="deltaTime">The time step.</param>
    internal void UpdateVelocity(float deltaTime)
    {
      // Update sleeping first.
      UpdateSleeping(deltaTime);

      // Static and kinematic body velocities are not influenced by forces.
      if (MotionType != MotionType.Dynamic)
        return;

      if (IsSleeping)
        return;

      // Derivative of linear velocity: v' = a = F / m
      //Vector3F linearAcceleration = AccumulatedForce * MassInverse;
      //LinearVelocity += deltaTime * linearAcceleration;

      // ----- Optimized version:
      Vector3F newLinearVelocity;
      newLinearVelocity.X = _linearVelocity.X + deltaTime * (AccumulatedForce.X * _massInverse);
      newLinearVelocity.Y = _linearVelocity.Y + deltaTime * (AccumulatedForce.Y * _massInverse);
      newLinearVelocity.Z = _linearVelocity.Z + deltaTime * (AccumulatedForce.Z * _massInverse);
      LinearVelocity = newLinearVelocity;

      // Derivative of angular momentum: Iω =  τ - coriolisTerm
      // The coriolis term is ignored because it can lead to instability.
      //OLD: AngularMomentumWorld += deltaTime * AccumulatedTorqueWorld; 
      //AngularVelocity += InertiaInverseWorld * deltaTime * AccumulatedTorque;

      // ----- Optimized version:
      Vector3F newAngularVelocity;
      newAngularVelocity.X = _angularVelocity.X + deltaTime * (_inertiaInverseWorld.M00 * AccumulatedTorque.X + _inertiaInverseWorld.M01 * AccumulatedTorque.Y + _inertiaInverseWorld.M02 * AccumulatedTorque.Z);
      newAngularVelocity.Y = _angularVelocity.Y + deltaTime * (_inertiaInverseWorld.M10 * AccumulatedTorque.X + _inertiaInverseWorld.M11 * AccumulatedTorque.Y + _inertiaInverseWorld.M12 * AccumulatedTorque.Z);
      newAngularVelocity.Z = _angularVelocity.Z + deltaTime * (_inertiaInverseWorld.M20 * AccumulatedTorque.X + _inertiaInverseWorld.M21 * AccumulatedTorque.Y + _inertiaInverseWorld.M22 * AccumulatedTorque.Z);
      AngularVelocity = newAngularVelocity;

      // TODO: Keep coriolis term.
      // Idea from XenoCollide forum (by Erin Catto): Use higher order integration for 
      // the coriolis term (e.g. Runge-Kutta). Use explicit integration for the rest.
    }


    /// <summary>
    /// Updates the pose using numerical integration.
    /// </summary>
    /// <param name="deltaTime">The time step.</param>
    internal void UpdatePose(float deltaTime)
    {
      // Static bodies do not move.
      if (MotionType == MotionType.Static)
        return;

      if (IsSleeping)
      {
        // Previously in this time step the sleeping could still be deferred. Therefore the 
        // velocities have not been reset. At this point the rigid body is definitely sleeping.

        // Reset the velocities...
        _linearVelocity = Vector3F.Zero;
        _angularVelocity = Vector3F.Zero;

        // ...and exit.
        return;
      }

      // Clamp velocities before we apply them.
      if (_linearVelocity.IsNaN)
      {
        _linearVelocity = Vector3F.Zero;
      }
      else if (_linearVelocity.LengthSquared > Simulation.Settings.Motion.MaxLinearVelocitySquared)
      {
        _linearVelocity.Length = Simulation.Settings.Motion.MaxLinearVelocity;
      }

      if (_angularVelocity.IsNaN)
      {
        _angularVelocity = Vector3F.Zero;
      }
      else if (_angularVelocity.LengthSquared > Simulation.Settings.Motion.MaxAngularVelocitySquared)
      {
        _angularVelocity.Length = Simulation.Settings.Motion.MaxAngularVelocity;
      }

      // Important: Use the center-of-mass pose!
      var x = PoseCenterOfMass.Position;
      var q = QuaternionF.CreateRotation(PoseCenterOfMass.Orientation);

      // Derivative of position: velocity
      // Derivative of orientation: q' = 1/2 * (0, ω) * q
      var xDerivative = LinearVelocity + LinearCorrectionVelocity;
      var qDerivative = 0.5f * new QuaternionF(0, AngularVelocity + AngularCorrectionVelocity) * q;
      Pose targetPoseCOM = new Pose(x + deltaTime * xDerivative, (q + deltaTime * qDerivative).Normalized);

      if (CcdEnabled
          && Simulation.Settings.Motion.CcdEnabled
          && LinearVelocity.LengthSquared > Simulation.Settings.Motion.CcdVelocityThresholdSquared)
      {
        // Continuous collision detection.
        IsCcdActive = true;
        TimeOfImpact = 1;
        Simulation.CcdRequested = true;
        TargetPose = targetPoseCOM * MassFrame.Pose.Inverse;

        // Trigger PoseChanged event. The pose has not changed but the AABB will be set to the 
        // temporal AABB during motion clamping.
        _aabbIsValid = false;
        OnPoseChanged(EventArgs.Empty);
      }
      else
      {
        IsCcdActive = false;
        PoseCenterOfMass = targetPoseCOM;
      }

      LinearCorrectionVelocity = Vector3F.Zero;
      AngularCorrectionVelocity = Vector3F.Zero;
    }
    #endregion
  }
}
