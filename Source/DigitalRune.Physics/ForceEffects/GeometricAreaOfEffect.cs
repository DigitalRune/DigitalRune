// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Collisions;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Applies a force field effect to all rigid bodies that touch a certain 
  /// <see cref="CollisionObject"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This <see cref="IAreaOfEffect"/> uses the collision detection to define which objects are in
  /// the area of effect. Per default no <see cref="CollisionObject"/> is set, which means that no
  /// objects are in the area of effect. A <see cref="CollisionObject"/> must be created by the user
  /// and set in the property <see cref="CollisionObject"/>. The properties of the collision object
  /// must be set appropriately and the collision object must be added to the 
  /// <see cref="Simulation.CollisionDomain"/> of the <see cref="Simulation"/> by the user.
  /// </para>
  /// <para>
  /// <strong>Performance Tip:</strong> If the <see cref="CollisionObject"/> is not used for other 
  /// purposes it is good for performance if the 
  /// <see cref="Geometry.Collisions.CollisionObject.Type"/> is set to 
  /// <see cref="CollisionObjectType.Trigger"/>.
  /// </para>
  /// <para>
  /// <strong>Collision Filtering:</strong> As with all other collision objects the standard
  /// collision filtering (see <see cref="CollisionDetection.CollisionFilter"/>) can be used to 
  /// define which objects can collide with the <see cref="CollisionObject"/>.
  /// </para>
  /// </remarks>
  public class GeometricAreaOfEffect : IAreaOfEffect
  {
    /// <summary>
    /// Gets or sets the collision object that defines the area of effect.
    /// </summary>
    /// <value>The collision object. The default is <see langword="null"/>.</value>
    /// <remarks>
    /// <para>
    /// This <see cref="IAreaOfEffect"/> uses the collision detection to define which objects are
    /// in the area of effect. Per default no <see cref="CollisionObject"/> is set, which means
    /// that no objects are in the area of effect. A <see cref="CollisionObject"/> must be created
    /// by the user and set in the property <see cref="CollisionObject"/>. The properties of the
    /// collision object must be set appropriately and the collision object must be added to the 
    /// <see cref="Simulation.CollisionDomain"/> of the <see cref="Simulation"/> by the user.
    /// </para>
    /// <para>
    /// <strong>Performance Tip:</strong> If the <see cref="CollisionObject"/> is not used for other 
    /// purposes it is good for performance if the 
    /// <see cref="Geometry.Collisions.CollisionObject.Type"/> is set to 
    /// <see cref="CollisionObjectType.Trigger"/>.
    /// </para>
    /// <para>
    /// <strong>Collision Filtering:</strong> As with all other collision objects the standard
    /// collision filtering (see <see cref="CollisionDetection.CollisionFilter"/>) can be used to 
    /// define which objects can collide with the <see cref="CollisionObject"/>.
    /// </para>
    /// </remarks>
    public CollisionObject CollisionObject { get; set; }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="GeometricAreaOfEffect"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="GeometricAreaOfEffect"/> class.
    /// </summary>
    public GeometricAreaOfEffect()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GeometricAreaOfEffect"/> class.
    /// </summary>
    /// <param name="collisionObject">The collision object.</param>
    public GeometricAreaOfEffect(CollisionObject collisionObject)
    {
      CollisionObject = collisionObject;
    }


    /// <inheritdoc/>
    public void Apply(ForceField forceField)
    {
      if (forceField == null)
        throw new ArgumentNullException("forceField");
      
      if (forceField.Simulation == null)
        return;

      if (CollisionObject != null && CollisionObject.Domain == forceField.Simulation.CollisionDomain)
      {
        // Get objects touching the CollisionObject.
        var affectedObjects = forceField.Simulation.CollisionDomain.GetContactObjects(CollisionObject);
        foreach (CollisionObject affectedObject in affectedObjects)
        {
          RigidBody body = affectedObject.GeometricObject as RigidBody;
          if (body != null && body.MotionType == MotionType.Dynamic)
          {
            forceField.Apply(body);
          }
        }
      }
    }
  }
}
