// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.ForceEffects;
using DigitalRune.Physics.Materials;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Applies gravity and push forces to bodies touched by the <see cref="KinematicCharacterController"/> 
  /// and handles traction when standing on moving platforms.
  /// </summary>
  internal class CharacterControllerForceEffect : ForceEffect
  {
    private readonly KinematicCharacterController _cc;


    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterControllerForceEffect"/> class.
    /// </summary>
    /// <param name="characterController">The character controller.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="characterController" /> is <see langword="null"/>.
    /// </exception>
    public CharacterControllerForceEffect(KinematicCharacterController characterController)
    {
      if (characterController == null)
        throw new ArgumentNullException("characterController");

      _cc = characterController;
    }


    protected override void OnApply()
    {
      Vector3F up = _cc.UpVector;
      float height = _cc.Height;                     // The height of the capsule (including the spherical caps).
      float radius = _cc.Width / 2;                  // The radius of the capsule.
      float bottomOfCylinder = -height / 2 + radius; // The bottom position of the cylindric part.

      float deltaTime = Simulation.Settings.Timing.FixedTimeStep;

      // A CC standing on another body should apply following impulse to the body:
      var gravityImpulse = -up * (_cc.Gravity * _cc.Body.MassFrame.Mass * deltaTime);

      // We also count the number of contacts on the bottom spherical cap and compute the 
      // sum of the velocities of the contacts.
      var numberOfGroundContacts = 0;
      var groundVelocity = Vector3F.Zero;

      // Loop over all contact constraints in the simulation to get all contacts of the CC. 
      // --> Apply gravity impulses on ground contacts. 
      // --> Get velocities of ground contacts.
      // --> Apply push impulses on other contacts.
      for (int i = 0; i < Simulation.ContactConstraints.Count; i++)
      {
        var contactConstraint = Simulation.ContactConstraints[i];

        // Skip contacts where the CC is not involved.
        if (contactConstraint.BodyA != _cc.Body && contactConstraint.BodyB != _cc.Body)
          continue;

        Contact contact = contactConstraint.Contact;

        // The CC should be object A, but object A and B can be swapped in the contact.
        bool swapped = (contactConstraint.BodyB == _cc.Body);
        var touchedBody = swapped ? contactConstraint.BodyA : contactConstraint.BodyB;

        // The contact position and normal.
        var position = swapped ? contact.PositionBLocal : contact.PositionALocal;
        var normal = swapped ? -contact.Normal : contact.Normal; // The normal points from CC to touchedBody.

        if (Vector3F.Dot(position, up) < bottomOfCylinder)
        {
          // ----- The contact is on the bottom spherical cap.
          // Apply gravity impulse on dynamic bodies.
          if (touchedBody.MotionType == MotionType.Dynamic)
            touchedBody.ApplyImpulse(gravityImpulse, contact.Position);

          // If the contact represents a slope where the CC can stand on, then the CC should
          // move with this contact.
          if (_cc.IsAllowedSlope(-normal))
          {
            numberOfGroundContacts++;

            // Get velocity of contact point.
            var velocity = touchedBody.GetVelocityOfWorldPoint(contact.Position);
            groundVelocity += velocity;

            // Add SurfaceMotion which can be defined in the material of a body.
            MaterialProperties materialProperties = touchedBody.Material.GetProperties(
              touchedBody,
              swapped ? contact.PositionALocal : contact.PositionBLocal,
              swapped ? contact.FeatureA : contact.FeatureB);

            if (materialProperties.SupportsSurfaceMotion)
            {
              var surfaceVelocity = touchedBody.Pose.ToWorldDirection(materialProperties.SurfaceMotion);
              groundVelocity += surfaceVelocity;
            }
          }
        }
        else
        {
          // ----- The contact is not on the bottom sphere. 
          // Apply push impulse to dynamic bodies.
          if (touchedBody.MotionType != MotionType.Dynamic)
            continue;

          // There are different methods to push bodies. We could apply the maximal impulse
          // to kick bodies out of the way. Here, we apply an impulse that changes the velocity
          // of the body to the current velocity of the CC. This is more like a smooth pushing
          // and not a brutal kicking.

          // Velocity of touchedBody at the contact.
          Vector3F velocity = touchedBody.GetVelocityOfWorldPoint(contact.Position);

          // Relative velocity between CC and touchedBody.
          Vector3F relativeVelocity = _cc._lastDesiredVelocity - velocity;

          // Relative velocity in normal direction.
          float collisionVelocity = Vector3F.Dot(relativeVelocity, normal);
          if (collisionVelocity > 0)
          {
            // CC and body are colliding (not separating).

            // Compute an impulse that changes the velocity of the contact from velocity to
            // _cc._lastDesiredVelocity.
            var matrixK = touchedBody.ComputeKMatrix(contact.Position);
            Vector3F impulse = matrixK.Inverse * relativeVelocity;

            // Limit the impulse by the max force of the CC.
            float impulseMagnitude = impulse.Length;
            float maxImpulseMagnitude = _cc.PushForce * deltaTime;
            if (impulseMagnitude > maxImpulseMagnitude && impulseMagnitude > 0)
              impulse.Length = maxImpulseMagnitude;

            touchedBody.ApplyImpulse(impulse, contact.Position);
          }
        }
      }

      // Traction: The CC should move when it stands on a moving platform. 
      // Set the velocity of the CC's body to the average ground velocity and let the
      // Simulation move the body for us.
      groundVelocity /= numberOfGroundContacts;
      _cc.Body.LinearVelocity = groundVelocity;
    }
  }
}
