// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Modifies a <see cref="SkeletonPose"/> using inverse kinematics (IK).
  /// </summary>
  /// <remarks>
  /// <para>
  /// An inverse kinematics (IK) solver transforms the bones of a skeleton in order to achieve
  /// a desired pose. For instance, a <see cref="LookAtIKSolver"/> rotates a bone (e.g. a head)
  /// to look into a desired direction. A <see cref="TwoJointIKSolver"/> can be used to 
  /// bend/stretch a leg so that it touches the ground, or to make an arm reach a certain target.
  /// </para>
  /// <para>
  /// An <see cref="IKSolver"/> instances modifies a <see cref="SkeletonPose"/> instance. And the
  /// goal is to point to or reach the <see cref="Target"/> position. The solver works in model 
  /// space - not in world space. Therefore, <see cref="Target"/> and other positions and 
  /// orientations must be specified in model space.
  /// </para>
  /// <para>
  /// If <see cref="Solve"/> is called and <see cref="MaxAngularVelocity"/> is 
  /// <see cref="float.PositiveInfinity"/> (default), the <see cref="SkeletonPose"/> is changed
  /// instantly. To avoid this instantaneous change, a <see cref="MaxAngularVelocity"/> limit can 
  /// be set, then the bones will rotate slowly to the target pose over several <see cref="Solve"/> 
  /// calls. <see cref="MaxAngularVelocity"/> defines the maximal rotation velocity for each bone.
  /// (However, limiting <see cref="MaxAngularVelocity"/> is not ideal if the model is also 
  /// animated, because the animation might reset the previous IK pose.)
  /// </para>
  /// <para>
  /// The solver also has a <see cref="Weight"/> parameter. If the weight is 0, the solver is 
  /// disabled. If the weight is 1, the solver tries its best to make the skeleton reach the target. 
  /// A weight less than 1 can be used to blend the target skeleton pose with the unmodified 
  /// skeleton pose.
  /// </para>
  /// <para>
  /// Note that the <see cref="IKSolver"/> implements <see cref="IAnimatableObject"/> which means
  /// that it has properties which can be animated: The only property that can be animated is the 
  /// <see cref="Weight"/>, the other properties are not animatable. Animating the weight can be 
  /// useful to fade an IK pose in or out.
  /// </para>
  /// </remarks>
  public abstract class IKSolver : IAnimatableObject
  {
    // TODO: We could add a more general IK Solver class:
    // We could create an IK solver that manages several chains concurrently. For bones
    // that are affected by more than one chain use a weight of 1 / numberOfChainsContainingBone
    // to mix the influences of the different IK goals.
    // Add support for limits.
    // Monitor limits. When the goal is equally distant from two limits, then bias the 
    // goal towards one limit (like a hysteresis) to minimize flipping. (see Game Prog. Gems 3).
    // Limit the angular velocities.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the skeleton pose.
    /// </summary>
    /// <value>The skeleton pose.</value>
    public SkeletonPose SkeletonPose
    {
      get { return _skeletonPose; }
      set
      {
        if (_skeletonPose != value)
        {
          _skeletonPose = value;
          OnInvalidate();
        }
      }
    }
    private SkeletonPose _skeletonPose;


    /// <summary>
    /// Gets or sets the target position in model space.
    /// </summary>
    /// <value>The target position in model space.</value>
    public Vector3F Target { get; set; }


    /// <summary>
    /// Gets or sets the weight.
    /// </summary>
    /// <value>The weight. The default is 1.</value>
    /// <remarks>
    /// <para>
    /// The solver has a <see cref="Weight"/> parameter. If the weight is 0, the solver is disabled.
    /// If the weight is 1, the solver tries its best to make the skeleton reach the target. A 
    /// weight less than 1 can be used to blend the target skeleton pose with the unmodified 
    /// skeleton pose.
    /// </para>
    /// </remarks>
    public float Weight
    {
      get { return _weightProperty.Value; }
      set { _weightProperty.Value = value; }
    }
    private readonly AnimatableProperty<float> _weightProperty;


    // per bone. The tip bone in a long chain can rotate faster. but its rotation relative to its
    // parent is limited by this value.
    // Must be positive. Locked when <= 0, no limit when float.PositiveInfinity.
    /// <summary>
    /// Gets or sets the maximal angular velocity per bone.
    /// </summary>
    /// <value>
    /// The maximal angular velocity. The default is <see cref="float.PositiveInfinity"/> 
    /// (= no limit).
    /// </value>
    /// <remarks>
    /// If <see cref="Solve"/> is called and <see cref="MaxAngularVelocity"/> is 
    /// <see cref="float.PositiveInfinity"/> (default), the <see cref="SkeletonPose"/> is changed
    /// instantly. To avoid this instantaneous change, a <see cref="MaxAngularVelocity"/> limit can 
    /// be set, then the bones will rotate slowly to the target pose over several 
    /// <see cref="Solve"/> calls. <see cref="MaxAngularVelocity"/> defines the maximal rotation 
    /// velocity for each bone.
    /// </remarks>
    public float MaxAngularVelocity { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="IKSolver"/> class.
    /// </summary>
    protected IKSolver()
    {
      MaxAngularVelocity = float.PositiveInfinity;

      _weightProperty = new AnimatableProperty<float>();
      _weightProperty.Value = 1.0f;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Modifies the <see cref="SkeletonPose"/> to reach the <see cref="Target"/> position.
    /// </summary>
    /// <param name="deltaTime">The current time step (in seconds).</param>
    public void Solve(float deltaTime)
    {
      if (_skeletonPose == null)
        return;

      if (Numeric.IsZero(Weight))
        return;

      if (MaxAngularVelocity * deltaTime < Numeric.EpsilonF)
        return;

      OnSolve(deltaTime);
    }


    /// <summary>
    /// Called when <see cref="IKSolver.Solve"/> is called.
    /// </summary>
    /// <param name="deltaTime">The current time step (in seconds).</param>
    /// <remarks>
    /// <strong>Notes to Inheritors: </strong><br/>
    /// This method must be implemented and perform the IK computations. When this method is called, 
    /// it is guaranteed that <see cref="IKSolver.SkeletonPose"/> is not <see langword="null"/>, and
    /// the <see cref="IKSolver.Weight"/> is not 0.
    /// </remarks>
    protected abstract void OnSolve(float deltaTime);


    /// <summary>
    /// Called when the <see cref="SkeletonPose"/> was exchanged.
    /// </summary>
    protected virtual void OnInvalidate()
    {
    }


    /// <summary>
    /// Determines whether the resulting bone transforms need to be interpolated with the original 
    /// bone transforms.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the resulting bone transforms need to be interpolated with the 
    /// original bone transforms; otherwise, <see langword="false"/>.
    /// </returns>
    internal bool RequiresBlending()
    {
      return !Numeric.AreEqual(Weight, 1);
    }


    /// <summary>
    /// Applies the weight to a given bone by blending the bone transforms.
    /// </summary>
    /// <param name="originalTransform">
    /// In: The original bone transform.
    /// </param>
    /// <param name="targetTransform">
    /// In: The target bone transform.<br/>
    /// Out: The blended bone transform.
    /// </param>
    internal void BlendBoneTransform(ref SrtTransform originalTransform, ref SrtTransform targetTransform)
    {
      targetTransform = SrtTransform.Interpolate(originalTransform, targetTransform, Weight);
    }


    /// <summary>
    /// Determines whether the rotations need to be limited.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last time step.</param>
    /// <param name="maxRotationAngle">The max rotation angle.</param>
    /// <returns>
    /// <see langword="true"/> if the max velocity defines a finite useful limit and rotations need
    /// to be limited; otherwise, <see langword="false"/>.
    /// </returns>
    internal bool RequiresLimiting(float deltaTime, out float maxRotationAngle)
    {
      if (Numeric.IsPositiveFinite(MaxAngularVelocity))
      {
        maxRotationAngle = MaxAngularVelocity * deltaTime;
        return maxRotationAngle < ConstantsF.Pi;
      }

      maxRotationAngle = ConstantsF.Pi;
      return false;
    }


    /// <summary>
    /// Applies max velocity limit to the given bone transform.
    /// </summary>
    /// <param name="originalTransform">
    /// In: The original bone transform.
    /// </param>
    /// <param name="targetTransform">
    /// In: The target bone transform.<br/>
    /// Out: The limited bone transform.
    /// </param>
    /// <param name="maxRotationAngle">The max rotation angle.</param>
    internal void LimitBoneTransform(ref SrtTransform originalTransform, ref SrtTransform targetTransform, float maxRotationAngle)
    {
      if (maxRotationAngle < ConstantsF.Pi)
      {
        // Compute relative rotation.
        var rotationChange = targetTransform.Rotation * originalTransform.Rotation.Conjugated;

        // Make sure we rotate around the shortest arc.
        if (QuaternionF.Dot(originalTransform.Rotation, targetTransform.Rotation) < 0)
          rotationChange = -rotationChange;

        if (rotationChange.Angle > maxRotationAngle && !rotationChange.V.IsNumericallyZero)
        {
          // ReSharper disable EmptyGeneralCatchClause
          try
          {
            // Limit rotation.
            rotationChange.Angle = maxRotationAngle;
            targetTransform.Rotation = rotationChange * originalTransform.Rotation;
          }
          catch
          {
            // rotationChange.Angle = xxx. Can cause DivideByZeroException or similar.
            // The !rotationChange.V.IsNumericallyZero should avoid this. But just to go sure.
          }
          // ReSharper restore EmptyGeneralCatchClause
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region IAnimatableObject
    //--------------------------------------------------------------

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <value>
    /// Not implemented. Always returns <see cref="String.Empty"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    string INamedObject.Name
    {
      get { return string.Empty; }
    }


    /// <summary>
    /// Gets the properties which are currently being animated.
    /// </summary>
    /// <returns>
    /// The properties which are currently being animated.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IEnumerable<IAnimatableProperty> IAnimatableObject.GetAnimatedProperties()
    {
      if (((IAnimatableProperty<float>)_weightProperty).IsAnimated)
        yield return _weightProperty;
    }


    /// <summary>
    /// Gets the property with given name and type which can be animated.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>
    /// The <see cref="IAnimatableProperty"/> that has the given name and type; otherwise, 
    /// <see langword="null"/> if the object does not have an property with this name or type.
    /// </returns>
    /// <remarks>
    /// The <see cref="Weight"/> property of an <see cref="IKSolver"/> can be animated. The 
    /// property is identified using the string "Weight".
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IAnimatableProperty<T> IAnimatableObject.GetAnimatableProperty<T>(string name)
    {
      if (name == "Weight")
        return _weightProperty as IAnimatableProperty<T>;

      return null;
    }
    #endregion
  }
}
