// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Constraints;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Connects to rigid bodies with a damped spring.
  /// </summary>
  /// <remarks>
  /// A better, more stable way to model a damped spring is to use joints instead of this force
  /// effect. You can model damped spring by using a <see cref="PositionMotor"/>. Especially for
  /// high spring constants (stiff springs) joints are more stable.
  /// </remarks>
  public class DampedSpring : ForceEffect
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the first rigid body.
    /// </summary>
    /// <value>The first rigid body.</value>
    public RigidBody BodyA { get; set; }


    /// <summary>
    /// Gets or sets the second rigid body.
    /// </summary>
    /// <value>The second rigid body.</value>
    public RigidBody BodyB { get; set; }


    /// <summary>
    /// Gets or sets the damping constant.
    /// </summary>
    /// <value>The damping constant. The default value is <c>1.6</c>.</value>
    public float DampingConstant { get; set; }


    /// <summary>
    /// Gets or sets the rest length of the spring.
    /// </summary>
    /// <value>The length. The default value is <c>0</c>.</value>
    /// <remarks>
    /// If the current length of the spring is equal to this value, then the spring does not apply
    /// forces to the attached bodies.
    /// </remarks>
    public float Length { get; set; }


    /// <summary>
    /// Gets or sets the position where the spring is attached to the first body (in local space of
    /// the first body).
    /// </summary>
    /// <value>The attachment position for the first body in local space.</value>
    public Vector3F AttachmentPositionALocal { get; set; }


    /// <summary>
    /// Gets or sets the position where the spring is attached to the second body (in local space of
    /// the second body).
    /// </summary>
    /// <value>The attachment position for the second body in local space.</value>
    public Vector3F AttachmentPositionBLocal { get; set; }


    /// <summary>
    /// Gets or sets the spring constant.
    /// </summary>
    /// <value>The spring constant. The default value is <c>2.0</c>.</value>
    public float SpringConstant { get; set; }


    #region ----- Optional new properties -----
    //        /// <summary>
    //        /// Not implemented yet.
    //        /// </summary>
    //        public float MaxCompressForce
    //        {
    //            set { throw new NotImplementedException(); }
    //        }
    //
    //        /// <summary>
    //        /// Not implemented yet.
    //        /// </summary>
    //        public float MaxCompressVelocity
    //        {
    //            set { throw new NotImplementedException(); }
    //        }
    //
    //        /// <summary>
    //        /// Not implemented yet.
    //        /// </summary>
    //        public float MaxStretchForce
    //        {
    //            set { throw new NotImplementedException(); }
    //        }
    //
    //        /// <summary>
    //        /// Not implemented yet.
    //        /// </summary>
    //        public float MaxStretchVelocity
    //        {
    //            set { throw new NotImplementedException(); }
    //        }
    #endregion

    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DampedSpring"/> class.
    /// </summary>
    public DampedSpring()
    {
      DampingConstant = 1.6f;
      SpringConstant = 2;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when the simulation wants this force effect to apply forces to rigid bodies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method must be implemented in derived classes. This method is only called after the
    /// force effect was added to a simulation and <see cref="ForceEffect.OnAddToSimulation"/> was 
    /// called.
    /// </para>
    /// <para>
    /// This method is responsible for applying the forces of the effect to the rigid bodies. To
    /// apply a force the methods <see cref="ForceEffect.AddForce(RigidBody, Vector3F, Vector3F)"/>,
    /// <see cref="ForceEffect.AddForce(RigidBody, Vector3F)"/> and/or
    /// <see cref="ForceEffect.AddTorque(RigidBody, Vector3F)"/> of the <see cref="ForceEffect"/>
    /// base class must be used. Do not use the <strong>AddForce</strong>/<strong>AddTorque</strong>
    /// methods of the <see cref="RigidBody"/> class.
    /// </para>
    /// </remarks>
    protected override void OnApply()
    {
      Vector3F worldPosA = (BodyA != null) ? BodyA.Pose.ToWorldPosition(AttachmentPositionALocal) : AttachmentPositionALocal;
      Vector3F velA = (BodyA != null) ? BodyA.GetVelocityOfLocalPoint(AttachmentPositionALocal) : Vector3F.Zero;
      Vector3F worldPosB = (BodyB != null) ? BodyB.Pose.ToWorldPosition(AttachmentPositionBLocal) : AttachmentPositionBLocal;
      Vector3F velB = (BodyB != null) ? BodyB.GetVelocityOfLocalPoint(AttachmentPositionBLocal) : Vector3F.Zero;

      // Compute spring force.
      Vector3F springVectorAToB = worldPosB - worldPosA;
      float currentLength = springVectorAToB.Length;

      if (!springVectorAToB.TryNormalize())
        springVectorAToB = Vector3F.UnitY;

      Vector3F force = SpringConstant * (currentLength - Length) * springVectorAToB;

      // Compute damping force.
      Vector3F velRel = velA - velB;
      force += -DampingConstant * Vector3F.Dot(velRel, springVectorAToB) * springVectorAToB;

      // Not needed anymore. Simulation.EvaluateForce automatically wakes up rigid bodies for big 
      // force changes.
      //if (BodyA != null && BodyB != null)
      //{
      //  // Make sure both are awake or sleep simultaneously.
      //  if (BodyA.IsSleeping && !BodyB.IsSleeping)
      //    BodyA.DeferSleep(Simulation.Settings.Timing.FixedTimeStep);
      //  if (!BodyA.IsSleeping && BodyB.IsSleeping)
      //    BodyB.DeferSleep(Simulation.Settings.Timing.FixedTimeStep);
      //}

      if (BodyA != null && BodyA.MotionType == MotionType.Dynamic)
        AddForce(BodyA, force, worldPosA);
      if (BodyB != null && BodyB.MotionType == MotionType.Dynamic)
        AddForce(BodyB, -force, worldPosB);
    }
    #endregion
  }
}
