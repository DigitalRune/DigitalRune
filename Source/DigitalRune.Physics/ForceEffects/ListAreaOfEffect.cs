// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Applies a force field effect to all objects in a given list.
  /// </summary>
  public class ListAreaOfEffect : IAreaOfEffect
  {
    /// <summary>
    /// Gets or sets the list of rigid bodies that are affected by the force effect.
    /// </summary>
    /// <value>
    /// The rigid bodies in the area of effect. The default value is an empty 
    /// <see cref="List{RigidBody}"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public IList<RigidBody> RigidBodies { get; set; }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ListAreaOfEffect"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ListAreaOfEffect"/> class.
    /// </summary>
    public ListAreaOfEffect()
    {
      RigidBodies = new List<RigidBody>();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ListAreaOfEffect"/> class.
    /// </summary>
    /// <param name="rigidBodies">
    /// The list of rigid bodies. The property <see cref="RigidBodies"/> is set to this list. 
    /// The list is not copied.
    /// </param>
    public ListAreaOfEffect(IList<RigidBody> rigidBodies)
    {
      RigidBodies = rigidBodies;
    }


    /// <inheritdoc/>
    public void Apply(ForceField forceField)
    {
      if (forceField == null)
        throw new ArgumentNullException("forceField");

      if (RigidBodies != null)
      {
        int numberOfRigidBodies = RigidBodies.Count;
        for (int index = 0; index < numberOfRigidBodies; index++)
        {
          var body = RigidBodies[index];
          if (body != null && body.MotionType == MotionType.Dynamic)
          {
            forceField.Apply(body);
          }
        }
      }
    }
  }
}
