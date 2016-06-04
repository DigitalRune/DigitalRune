// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Applies a force effect to all bodies in the <see cref="AreaOfEffect"/> individually.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A force field is a specialized force effect that applies a force to all bodies that are in
  /// the <see cref="AreaOfEffect"/>. The area of effect can be defined using collision detection,
  /// simple lists, or other means. Each body in the area of effect is treated individually.
  /// </para>
  /// <para>
  /// Derived classes must implement the method <see cref="Apply(RigidBody)"/>.
  /// </para>
  /// </remarks>
  public abstract class ForceField : ForceEffect
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the area of effect.
    /// </summary>
    /// <value>The area of effect. (Can be <see langword="null"/>.)</value>
    /// <remarks>
    /// The <see cref="IAreaOfEffect"/> object defines on which object the force field effect is
    /// applied. When the property is <see langword="null"/>, nothing is affected by the force 
    /// effect.
    /// </remarks>
    public IAreaOfEffect AreaOfEffect { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ForceField"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ForceField"/> class.
    /// </summary>
    /// <remarks>
    /// The property <see cref="AreaOfEffect"/> is initialized with a new instance of
    /// <see cref="GlobalAreaOfEffect"/>.
    /// </remarks>
    protected ForceField()
      : this(new GlobalAreaOfEffect())
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ForceField"/> class.
    /// </summary>
    /// <param name="areaOfEffect">The area of effect.</param>
    protected ForceField(IAreaOfEffect areaOfEffect)
    {
      AreaOfEffect = areaOfEffect;
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
    /// <strong>Notes to Inheritors:</strong>
    /// This method must be implemented in derived classes. This method is only called after the
    /// force effect was added to a simulation and <see cref="ForceEffect.OnAddToSimulation"/> was 
    /// called.
    /// </para>
    /// <para>
    /// This method uses the <see cref="IAreaOfEffect"/> to call <see cref="Apply"/> for each rigid 
    /// body in the area of effect.
    /// </para>
    /// </remarks>
    protected override void OnApply()
    {
      if (AreaOfEffect != null)
        AreaOfEffect.Apply(this);
    }


    /// <summary>
    /// Applies the force effect to the specified body.
    /// </summary>
    /// <param name="body">The rigid body.</param>
    /// <remarks>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> This method is responsible for applying the forces of
    /// the effect to a rigid body. To apply a force the methods 
    /// <see cref="ForceEffect.AddForce(RigidBody, Vector3F, Vector3F)"/>,
    /// <see cref="ForceEffect.AddForce(RigidBody, Vector3F)"/> and/or
    /// <see cref="ForceEffect.AddTorque(RigidBody, Vector3F)"/> of the <see cref="ForceEffect"/>
    /// base class must be used. Do not use the <strong>AddForce</strong>/<strong>AddTorque</strong>
    /// methods of the <see cref="RigidBody"/> class.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="body"/> is <see langword="null"/>.
    /// </exception>
    public abstract void Apply(RigidBody body);
    #endregion
  }
}
