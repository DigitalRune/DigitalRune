// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Applies an explosion force for a short duration.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The explosion starts immediately after it is added to a <see cref="Simulation"/>. When the
  /// effect has ended (see <see cref="Duration"/>) it removes itself automatically from the 
  /// <see cref="Simulation.ForceEffects"/> collection of the <see cref="Simulation"/>.
  /// </para> 
  /// <para>
  /// <strong>Explosion Model:</strong>
  /// The effect applies a force that pushes rigid bodies away from the center of the explosion 
  /// (see <see cref="Position"/>). The effect is only applied to the rigid body's center of mass -
  /// size, shape and orientation of the rigid body is ignored. The force effect has a linear 
  /// falloff, i.e. a rigid body in <i>n</i> units distance experiences only 1/<i>n</i> of the 
  /// force.
  /// </para>
  /// </remarks>
  public class Explosion : ForceField
  {
    // TODO: Make auto-removal optional.
    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the duration of the explosion.
    /// </summary>
    /// <value>The duration of the explosion. The default is 0.03 s.</value>
    public TimeSpan Duration { get; set; }


    /// <summary>
    /// Gets or sets the simulation time when the effect ends.
    /// </summary>
    /// <value>The end time.</value>
    private TimeSpan EndTime { get; set; }


    /// <summary>
    /// Gets or sets the force magnitude of the explosion.
    /// </summary>
    /// <value>The force. The default is <c>100000</c>.</value>
    public float Force { get; set; }


    /// <summary>
    /// Gets or sets the position of the center of the explosion.
    /// </summary>
    /// <value>The position of the explosion center. The default is <c>(0, 0, 0)</c>.</value>
    public Vector3F Position { get; set; }


    /// <summary>
    /// Gets or sets the explosion radius.
    /// </summary>
    /// <value>The explosion radius. The default value is 5.</value>
    /// <remarks>
    /// The explosion force will fall off from the explosion center (<see cref="Position"/>) to this 
    /// radius where it reaches 0. Bodies outside this radius are not affected.
    /// </remarks>
    public float Radius { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Explosion"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Explosion"/> class.
    /// </summary>
    /// <remarks>
    /// The property <see cref="ForceField.AreaOfEffect"/> is initialized with a new instance of
    /// <see cref="GlobalAreaOfEffect"/>.
    /// </remarks>
    public Explosion()
    {
      Initialize();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Explosion"/> class.
    /// </summary>
    /// <param name="areaOfEffect">The area of effect.</param>
    public Explosion(IAreaOfEffect areaOfEffect)
      : base(areaOfEffect)
    {
      Initialize();
    }


    private void Initialize()
    {
      Duration = new TimeSpan(0, 0, 0, 0, 30); // 30 ms
      Force = 100000;
      EndTime = TimeSpan.MinValue;
      Radius = 5;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when this force effect is added to a simulation.
    /// </summary>
    /// <remarks>
    /// The simulation to which the force effect is added is set in the property
    /// <see cref="Simulation"/>.
    /// </remarks>
    protected override void OnAddToSimulation()
    {
      base.OnAddToSimulation();

      if (EndTime == TimeSpan.MinValue)
        EndTime = Simulation.Time + Duration;
    }


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
      if (EndTime <= Simulation.Time)
      {
        // The effect has ended. Remove from the force effects.
        Simulation.ForceEffects.Remove(this);
        return;
      }

      AreaOfEffect.Apply(this);
    }


    /// <inheritdoc/>
    public override void Apply(RigidBody body)
    {
      if (body == null)
        throw new ArgumentNullException("body", "Rigid body in area of effect must not be null.");

      // Calculate distance to explosion center.
      Vector3F explosionToBody = body.PoseCenterOfMass.Position - Position;
      float distanceSquared = explosionToBody.LengthSquared;

      float radiusSquared = Radius * Radius;
      if (distanceSquared > radiusSquared)
        return;

      // Using a falloff curve of: 1 - (d/r)^a with a = 2;
      float attenuation = 1 - distanceSquared / radiusSquared; 

      if (!explosionToBody.TryNormalize())
        explosionToBody = Vector3F.UnitY;

      // Apply force in direction of distance. Force fades off with the distance.
      var force = Force * attenuation * explosionToBody;
      AddForce(body, force);
    }
    #endregion
  }
}
