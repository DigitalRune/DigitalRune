// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.ForceEffects;


namespace DigitalRune.Physics.Specialized
{
  // This class implements a force effect and handles the Simulation.SubTimeStepFinished
  // event. ForceEffect.OnApply() is called in the physics simulation after the
  // collision detection. We use this method to update the contact info of vehicle wheels.
  // When the SubTimeStepFinished event occurs, we update the wheel rotations.
  // All vehicles of one simulation use the same instance of this class.
  internal class ConstraintVehicleHandler : ForceEffect
  {
    // TODO: 
    // - If all vehicles are removed and a new one is added, a new instance of this
    //   class is created (unnecessary garbage)...


    private readonly List<ConstraintVehicle> _vehicles = new List<ConstraintVehicle>();


    // Creates or gets the ConstraintVehicleHandler instance and adds a vehicle.
    internal static void Add(ConstraintVehicle vehicle)
    {
      var simulation = vehicle.Simulation;
      var handler = GetHandler(simulation);
      if (handler == null)
      {
        // The first vehicle: Add a new instance of this class to the simulation.
        handler = new ConstraintVehicleHandler();
        simulation.ForceEffects.Add(handler);
        simulation.SubTimeStepFinished += handler.OnSubTimeStepFinished;
      }

      handler._vehicles.Add(vehicle);
    }


    // Removes a vehicle. When the last vehicle is removed, the ConstraintVehicleHandler
    // instance is removed too.
    internal static void Remove(ConstraintVehicle vehicle)
    {
      Simulation simulation = vehicle.Simulation;
      ConstraintVehicleHandler handler = GetHandler(simulation);
      if (handler != null)
      {
        handler._vehicles.Remove(vehicle);

        // The last vehicle was removed.
        if (handler._vehicles.Count == 0)
        {
          simulation.ForceEffects.Remove(handler);
          simulation.SubTimeStepFinished -= handler.OnSubTimeStepFinished;
        }
      }
    }


    // Searches for an ConstraintVehicleHandler instance in the Simulation.ForceEffects.
    private static ConstraintVehicleHandler GetHandler(Simulation simulation)
    {
      foreach (var forceEffect in simulation.ForceEffects)
      {
        ConstraintVehicleHandler handler = forceEffect as ConstraintVehicleHandler;
        if (handler != null)
          return handler;
      }

      return null;
    }


    // Called when the forces are updated (after the collision detection).
    protected override void OnApply()
    {
      // Loop over all contact sets and update all wheels.
      // (Note: In the future we will add internal acceleration structures for faster contact
      // set queries, then it will be faster to call 
      // Simulation.CollisionDomain.GetContactSet(wheel.CollisionObject)
      // for each wheel instead of looping over all contact sets.
      foreach (var contactSet in Simulation.CollisionDomain.ContactSets)
      {
        var wheel = contactSet.ObjectA.GeometricObject as ConstraintWheel;
        if (wheel == null)
          wheel = contactSet.ObjectB.GeometricObject as ConstraintWheel;

        if (wheel != null && wheel.Tag == 0)
          UpdateWheelContactInfo(wheel, contactSet);
      }

      // Update all other wheels which do not touch the ground.
      foreach (var vehicle in _vehicles)
      {
        foreach (var wheel in vehicle.Wheels)
        {
          if (wheel.Tag == 0)
            UpdateWheelContactInfo(wheel, null);
        }
      }
    }


    private static void UpdateWheelContactInfo(ConstraintWheel wheel, ContactSet contactSet)
    {
      if (contactSet != null && contactSet.HaveContact && contactSet.Count > 0)
      {
        // ----- Ray has contact.
        var contact = contactSet[0];

        wheel.HasGroundContact = true;
        wheel.GroundPosition = contact.Position;
        if (wheel.CollisionObject == contactSet.ObjectA)
        {
          wheel.GroundNormal = -contact.Normal;
          wheel.TouchedBody = contactSet.ObjectB.GeometricObject as RigidBody;
        }
        else
        {
          wheel.GroundNormal = contact.Normal;
          wheel.TouchedBody = contactSet.ObjectA.GeometricObject as RigidBody;
        }

        // If the ray is nearly parallel to the ground, then the contact is not
        // useful and we ignore it.
        Vector3F up = wheel.Vehicle.Chassis.Pose.Orientation.GetColumn(1);
        float normalDotUp = Vector3F.Dot(wheel.GroundNormal, up);
        if (Numeric.IsGreater(normalDotUp, 0))
        {
          wheel.Tag = 1; // Tag = 1 means that this wheel has a useful ground contact.

          float hitDistance = contact.PenetrationDepth;
          wheel.SuspensionLength = Math.Max(hitDistance - wheel.Radius, wheel.MinSuspensionLength);

          wheel.Constraint.BodyB = wheel.TouchedBody ?? wheel.Vehicle.Simulation.World;
          wheel.Constraint.Enabled = true;

          wheel.GroundRight = wheel.Vehicle.Chassis.Pose.ToWorldDirection(Matrix33F.CreateRotationY(wheel.SteeringAngle) * Vector3F.UnitX);
          wheel.GroundForward = Vector3F.Cross(wheel.GroundNormal, wheel.GroundRight).Normalized;
        }
      }
      else
      {
        // -----  The wheel is in the air.          
        wheel.Constraint.Enabled = false;
        wheel.Constraint.BodyB = wheel.Vehicle.Simulation.World;
        wheel.HasGroundContact = false;
        wheel.TouchedBody = null;
        wheel.SuspensionLength = wheel.SuspensionRestLength;
      }
    }



    private void OnSubTimeStepFinished(object sender, EventArgs eventArgs)
    {
      float deltaTime = Simulation.Settings.Timing.FixedTimeStep;

      foreach (var vehicle in _vehicles)
        foreach (var wheel in vehicle.Wheels)
          UpdateWheelVelocity(deltaTime, vehicle, wheel);
    }


    private static void UpdateWheelVelocity(float deltaTime, ConstraintVehicle vehicle, ConstraintWheel wheel)
    {
      if (wheel.Tag == 1)
      {
        wheel.Tag = 0;

        if (Numeric.IsZero(wheel.BrakeForce))
        {
          // We set the angular velocity, so that the wheel matches the moving underground.
          Vector3F relativeContactVelocity = vehicle.Chassis.GetVelocityOfWorldPoint(wheel.GroundPosition)
                                             - wheel.TouchedBody.GetVelocityOfWorldPoint(wheel.GroundPosition);
          float forwardVelocity = Vector3F.Dot(relativeContactVelocity, wheel.GroundForward);
          wheel.AngularVelocity = forwardVelocity / wheel.Radius;
          wheel.RotationAngle += wheel.AngularVelocity * deltaTime;
        }
        else
        {
          // Braking wheels do not rotate.
          wheel.AngularVelocity = 0;
        }
      }
      else
      {
        // To keep it simple: The wheel continues spinning in the same direction.
        // Damp angular velocity and update rotation angle.
        wheel.AngularVelocity *= 0.99f;
        wheel.RotationAngle += wheel.AngularVelocity * deltaTime;
      }
    }
  }
}
