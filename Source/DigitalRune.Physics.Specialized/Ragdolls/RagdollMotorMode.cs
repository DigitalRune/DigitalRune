// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Animation.Character;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Defines the type of <see cref="RagdollMotor"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A ragdoll motor can either directly set the velocities of the controlled ragdoll bodies or it
  /// can use <see cref="Constraint"/>s to influence the bodies. 
  /// </para>
  /// <para>
  /// <strong>Velocity Motors:</strong><br/>
  /// Velocity motors directly set the velocities (<see cref="RigidBody.LinearVelocity"/> and 
  /// <see cref="RigidBody.AngularVelocity"/>) of the controlled rigid bodies. If velocity motors
  /// are used, <see cref="Ragdoll.DriveToPose(SkeletonPose,float)"/> must be called in each frame. 
  /// Velocity motors drive the ragdoll to an absolute world space pose. This means that velocity 
  /// motors cannot be used if different ragdoll movements should be blended (e.g. a hurled ragdoll 
  /// moves its limbs into a defensive pose). Forces acting on the ragdoll and collisions will also 
  /// have little impact. Velocity motors can be used to drive the root bone of a ragdoll.
  /// </para>
  /// <para>
  /// <strong>Constraint Motors:</strong><br/>
  /// Constraint motors use <see cref="Constraint"/>s to control the movement of the ragdoll bodies.
  /// It is only necessary to call <see cref="Ragdoll.DriveToPose(SkeletonPose,float)"/> if the 
  /// target skeleton pose has changed. Constraints can be used to blend key-frame animation and 
  /// physically-based animation (reaction to collisions). A constraint motor acts similar to a 
  /// damped spring that connects a bone with its parent bone. Constraint motors cannot be used to 
  /// drive the root bone of a ragdoll.
  /// </para>
  /// </remarks>
  public enum RagdollMotorMode
  {
    /// <summary>
    /// A velocity motor directly sets the linear and angular velocity of controlled ragdoll bodies. 
    /// </summary>
    Velocity,

    /// <summary>
    /// Constraint motors use <see cref="Constraint"/>s to influence the controlled ragdoll bodies.
    /// </summary>
    Constraint,
  }
}
