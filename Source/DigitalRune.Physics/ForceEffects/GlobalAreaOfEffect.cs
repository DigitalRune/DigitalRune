// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Applies a force field effect to all <see cref="Simulation.RigidBodies"/> of the 
  /// <see cref="Simulation"/>. (An optional predicate can be used to exclude certain objects.)
  /// </summary>
  public class GlobalAreaOfEffect : IAreaOfEffect 
  {
    /// <summary>
    /// Gets or sets the predicate that can be used to exclude rigid bodies from the area of effect.
    /// </summary>
    /// <value>
    /// The predicate that determines whether a rigid body is excluded from the area of effect. The 
    /// default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// If this predicate is <see langword="null"/>, all rigid bodies of the simulation are in the
    /// area of effect.
    /// </remarks>
    public Predicate<RigidBody> Exclude { get; set; }


    /// <inheritdoc/>
    public void Apply(ForceField forceField)
    {
      if (forceField == null)
        throw new ArgumentNullException("forceField");

      var simulation = forceField.Simulation;
      if (simulation != null)
      {
        int numberOfRigidBodies = simulation.RigidBodies.Count;
        if (Exclude != null)
        {
          for (int i = 0; i < numberOfRigidBodies; i++)
          {
            var body = simulation.RigidBodies[i];
            if (body.MotionType == MotionType.Dynamic && !Exclude(body))
            {
              forceField.Apply(body);
            }
          }
        }
        else
        {
          for (int i = 0; i < numberOfRigidBodies; i++)
          {
            var body = simulation.RigidBodies[i];
            if (body.MotionType == MotionType.Dynamic)
            {
              forceField.Apply(body);
            }
          }
        }
      }
    }
  }
}
