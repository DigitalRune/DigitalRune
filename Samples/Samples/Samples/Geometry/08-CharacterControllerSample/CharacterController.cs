using System;
using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Geometry
{
  //----------------------------------------------------------------------------
  // Important: 
  // This character controller is an educational example. The DigitalRune.Physics
  // library contains a more advanced, faster and more stable character controller
  // implementation.
  //----------------------------------------------------------------------------


  // A basic character controller.
  // 
  // A character is modeled as an upright capsule.
  // This class handles collisions. The character handles:
  // - Sliding along obstacles
  // - Stepping up and down
  // - Jumping
  // - Step height is limited.
  // - The slope angle that can be climbed is limited. 
  //
  // How a new position is computed:
  // Finding a new position is difficult because the character controller should not penetrate
  // obstacles. Additionally, the character controller should slide along walls and not stop
  // at the first contact.
  // Following method is used. The character controller capsule is positioned at the desired 
  // position and we compute contact with other objects. For each contact point we check
  // the contact position, penetration depth and contact normal. Each contact limits the possible
  // movement of the character and for each contact point we store a bounding plane that represents 
  // this movement limit. 
  // When a desired position penetrates a bounding plane, we project the position back onto the
  // the surface of this plane and check for collision at this positions. For all new contact points
  // we add more bounding planes and restart the process.
  // In other words: In a loop we test several positions and collecting planes that define the
  // convex space in which we are allowed to move. In an inner loop we try to find a position in 
  // this convex space that is close to the desired position.
  //
  // Note on updating collision information:
  // Collision information has to be computed for temporal positions of the character controller.
  // Normally, we can call collisionDomain.Update() to compute new collision information for 
  // all collision objects. The collisionDomain will see what objects have moved and will compute 
  // new collision info only where necessary. Here, we know that only the character controller has 
  // moved, so we call collisionDomain.Update(character controller) which is even faster.
  //
  // The character controller is already pretty capable but far from perfect.
  // TODOs:
  // - Make character slower when climbing slopes.
  // - Reduce maneuverability while airborne.
  // - Character jitters when running into some corners.
  // - Movement is not smooth when running down steep slope while sliding along a wall.
  // - Smooth movement (stepping up stairs is too snappy).
  // - ...
  public class CharacterController
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    // The dimensions of the character controller capsule (in meter).
    private const float Height = 1.8f;
    private const float Width = 1;

    // The max. number of iterations of the outer loops, where new positions are tested.
    private const int IterationLimit = 2;

    // The max. number of iterations for finding a position in the permitted convex space.
    private const int SolverIterationLimit = 5;

    // We allow small penetrations between capsule and other objects. The reason is: If we do 
    // not allow penetrations then at the end of the character controller movement the capsule
    // does not touch anything. This is bad because then we do not know if the character 
    // is standing on the ground or if it is in the air.
    // So it is better to allow small penetrations so that ground and wall contacts are always
    // visible for the program.
    private const float AllowedPenetration = 0.01f;

    // The character can move up inclined planes. If the inclination is higher than this value
    // the character will not move up.
    private static readonly float SlopeLimit = MathHelper.ToRadians(45);

    // Height limit for stepping up/down.
    // Up steps: The character automatically tries to move up low obstacles/steps. To move up onto 
    // a step it is necessary that the obstacle is not higher than this value and that there is 
    // enough space for the character to stand on. 
    // Down steps: If the character loses contact with the ground it tries to step down onto solid
    // ground. If it cannot find ground within the step height, it  will simply fall in a ballistic
    // curve (defined by gravity). Example, where this is useful: If the character moves 
    // horizontally on an inclined plane, it will always touch the plane. But, if the step height is 
    // set to <c>0</c>, the character will not try to step down and instead will "fall" down the
    // plane on short ballistic curves.    
    private const float StepHeight = 0.3f;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The collision domain that computes all collisions of the character controller.
    private readonly CollisionDomain _collisionDomain;

    // Current velocity from gravity.
    private Vector3F _gravityVelocity;

    // Current velocity from jumping.
    private Vector3F _jumpVelocity;

    // The last valid position (set at the beginning of Move()).
    private Vector3F _oldPosition;

    // The desired target position (set in Move()).
    private Vector3F _desiredPosition;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    // The geometric object of the character.
    public GeometricObject GeometricObject { get; private set; }


    // The collision object used for collision detection. 
    public CollisionObject CollisionObject { get; private set; }


    // The bottom position (the lowest point on the capsule).
    public Vector3F Position
    {
      get
      {
        return GeometricObject.Pose.Position - new Vector3F(0, Height / 2, 0);
      }
      set
      {
        Pose oldPose = GeometricObject.Pose;
        Pose newPose = new Pose(value + new Vector3F(0, Height / 2, 0), oldPose.Orientation);
        GeometricObject.Pose = newPose;

        // Note: GeometricObject.Pose is a struct. That means we cannot simply set
        //   GeometricObject.Pose.Position = value + new Vector3F(0, Height / 2, 0);
        // We need to set the whole struct.
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public CharacterController(CollisionDomain domain)
    {
      _collisionDomain = domain;

      // Create a game object for the character controller.
      GeometricObject = new GeometricObject(
        new CapsuleShape(Width / 2, Height),
        new Pose(new Vector3F(0, Height / 2, 0)));

      // Create a collision object for the game object and add it to the collision domain.
      CollisionObject = new CollisionObject(GeometricObject);
      _collisionDomain.CollisionObjects.Add(CollisionObject);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Move the character to the new desired position, sliding along obstacles and stepping 
    // automatically up and down. Gravity is applied.
    public void Move(Vector3F desiredPosition,
                     float deltaTime,     // The size of the time step in seconds.
                     bool jump)           // True if character should jump.
    {
      // Remember the start position.
      _oldPosition = Position;

      // Desired movement vector:
      Vector3F desiredMovement = desiredPosition - Position;

      if (HasGroundContact())
      {
        // The character starts on the ground.

        // Reset velocity from gravity.
        _gravityVelocity = Vector3F.Zero;

        // Add jump velocity or reset jump velocity.
        if (jump)
          _jumpVelocity = new Vector3F(0, 4, 0);
        else
          _jumpVelocity = Vector3F.Zero;
      }

      // Add up movement to desired movement.
      desiredMovement += _jumpVelocity * deltaTime;

      // Add down movement due to gravity.
      _gravityVelocity += new Vector3F(0, -9.81f, 0) * deltaTime;
      desiredMovement += _gravityVelocity * deltaTime;

      // Compute the total desired position.
      _desiredPosition = _oldPosition + desiredMovement;

      bool isJumping = _jumpVelocity != Vector3F.Zero;

      // Try to slide to desired position. 
      // If we are jumping we do not stop at the first obstacle. If we are not jumping 
      // Slide() should stop at the first obstacle because we can try to step up.
      if (!Slide(!isJumping))
      {
        // Try to step up the allowed step height.
        bool stepped = StepUp();

        // If we could not step up, continue sliding.
        if (!stepped)
          Slide(false);
      }

      // If we are not jumping and do not touch the ground, try a down step.
      if (!isJumping && !HasGroundContact())
        StepDown();

      // Limit amount of movement to the length of the desired movement.
      // (Position corrections could have added additional movement.)
      Vector3F actualMovement = Position - _oldPosition;
      float desiredMovementLength = (_desiredPosition - _oldPosition).Length;
      if (actualMovement.Length > desiredMovementLength)
      {
        // Correct length of movement.
        Position = _oldPosition + actualMovement.Normalized * desiredMovementLength;

        // Update collision detection info for new corrected Position.
        _collisionDomain.Update(CollisionObject);
      }
    }


    // Slides the character controller to a new valid position. If stopAtObstacle is set,
    // the slide ends at the first obstacle; otherwise the slide continues as far as possible.
    // True is returned if the slide was stopped at an obstacle.
    private bool Slide(bool stopAtObstacle)
    {
      // We try to move to the _desiredPosition. 
      Vector3F desiredMovement = _desiredPosition - Position;

      // Abort if the movement length is zero.
      if (desiredMovement.IsNumericallyZero)
        return true;

      Vector3F desiredMovementDirection = desiredMovement.Normalized;

      // All bounding planes are collected in this list.
      List<Plane> bounds = new List<Plane>();

      bool startedOnGround = HasGroundContact();

      // Loop until we have found an allowed position or until iteration limit is exceeded.
      Vector3F startPosition = Position;
      bool blocked = false;                    // Flag: true if a steep plane was hit.
      bool targetPositionIsValid = false;
      for (int i = 0; i < IterationLimit && !targetPositionIsValid; i++)
      {
        // Add all bounding planes of current position.
        AddBounds(bounds, Position);

        // In this loop: We correct currentMovement until the movement is within the allowed space.
        Vector3F currentMovement = desiredMovement;
        for (int j = 0; j < SolverIterationLimit && !targetPositionIsValid; j++)
        {
          // Assume the current position (= startPosition + currentMovement) is valid.
          targetPositionIsValid = true;

          // Iterate over all bounding planes and correct penetrations.
          foreach (Plane plane in bounds)
          {
            // Ignore the plane if we are moving away from it.
            if (Numeric.IsGreaterOrEqual(Vector3F.Dot(plane.Normal, currentMovement), 0))
              continue;

            // Get distance from plane.
            float distance = GeometryHelper.GetDistance(plane, startPosition + currentMovement);
            if (Numeric.IsLess(distance, 0))
            {
              // We are in the forbidden space.

              // Slide along the wall: We simply recover from penetration by moving into the plane 
              // normal direction.
              Vector3F correction = plane.Normal * -distance;

              if (!IsAllowedSlope(plane.Normal))
              {
                // Slope is above slope limit. We have to correct the correction.

                if (stopAtObstacle)
                {
                  // We should stop at obstacle: Move back until no penetration.
                  correction = (-distance) / Vector3F.Dot(desiredMovementDirection, plane.Normal) * desiredMovementDirection;
                  blocked = true;
                }
                else
                {
                  // Slide laterally (don't slide "up" forbidden slopes).
                  Vector3F correctionDirection = new Vector3F(plane.Normal.X, 0, plane.Normal.Z);
                  if (correctionDirection.TryNormalize())
                    correction = (-distance) / Vector3F.Dot(correctionDirection, plane.Normal) * correctionDirection;
                }
              }

              // Apply correction.
              currentMovement += correction;

              // We have to check the new currentMovement.
              targetPositionIsValid = false;
            }
          }
        }

        if (!targetPositionIsValid)
        {
          // The previous loop has stopped exceeded the iteration limit without finding a valid
          // target position. Abort!
          break;
        }

        // Update position and detect collisions.
        Position = startPosition + currentMovement;
        _collisionDomain.Update(CollisionObject);

        targetPositionIsValid = !HasUnallowedContact(currentMovement);

        if (targetPositionIsValid                                                      // Target position is valid.
           && startedOnGround                                                          // We started standing on ground.
           && Numeric.IsZero(desiredMovement.X) && Numeric.IsZero(desiredMovement.Z)   // The desired movement is vertical
           && currentMovement.Y <= 0)                                                  // The resulting movement has a "down" component.
        {
          // Abort. We don't want to slide down when desiredMovement was straight down (gravity!).
          // Without this check, the character will slide down on an inclined plane instead of
          // standing still.
          targetPositionIsValid = false;
          break;
        }
      }

      if (!targetPositionIsValid
          || Numeric.IsNaN(Position.Length))   // Position contains NaN!? We have messed it up :-(
      {
        // No valid position found --> Reset position.
        Position = startPosition;
        _collisionDomain.Update(CollisionObject);
      }

      return !blocked;
    }


    // Tries to step up onto an obstacle. 
    // This method does not change the position if no up-step can be made.
    // If a step-up was performed true is returned, otherwise false.
    private bool StepUp()
    {
      Vector3F startPosition = Position;
      Vector3F desiredMovement = _desiredPosition - startPosition;

      // Compute forward direction (movement direction normal to the up direction). 
      // Abort if the movement is not forward directed.
      Vector3F forward = new Vector3F(desiredMovement.X, 0, desiredMovement.Z);
      if (!forward.TryNormalize())
        return false;

      // Test if there is enough room if we step up and forward. There must be at least room
      // for half the capsule.
      Position = Position + new Vector3F(0, StepHeight, 0) + forward * (Width / 2 - AllowedPenetration);

      // Update collision info.
      _collisionDomain.Update(CollisionObject);

      if (HasUnallowedContact(Vector3F.Zero))
      {
        // Not enough room :-(. Undo movement.
        Position = startPosition;
        _collisionDomain.Update(CollisionObject);
        return false;
      }

      return true;
    }


    // Tries to move down the StepHeight. If a down step leads to ground contact, this is the
    // new character position. If no ground contact is found, this method does not change
    // the character position.
    private void StepDown()
    {
      // We try a downward movement with the StepHeight.
      Vector3F startPosition = Position;
      Vector3F desiredMovement = new Vector3F(0, -StepHeight, 0);

      // All bounding planes are collected in this list.
      List<Plane> bounds = new List<Plane>();

      // Loop until we have found an allowed position or until iteration limit is exceeded.
      bool targetPositionIsValid = false;
      for (int i = 0; i < IterationLimit && !targetPositionIsValid; i++)
      {
        // Add all bounding planes of current position.
        AddBounds(bounds, Position);

        // In this loop: We correct currentMovement until the movement is within the allowed space.
        Vector3F currentMovement = desiredMovement;
        for (int j = 0; j < SolverIterationLimit && !targetPositionIsValid; j++)
        {
          // Assume the current position (= startPosition + currentMovement) is valid.
          targetPositionIsValid = true;

          // Iterate over all bounding planes and correct penetrations.
          foreach (Plane plane in bounds)
          {
            // Get distance from plane.
            float distance = GeometryHelper.GetDistance(plane, startPosition + currentMovement);
            if (Numeric.IsLess(distance, 0))
            {
              // We are in the forbidden space.

              // Correct the position upwards. Do not slide.
              Vector3F correction = (-distance) / Vector3F.Dot(Vector3F.UnitY, plane.Normal) * Vector3F.UnitY;
              currentMovement += correction;

              // We have to check the new currentMovement.
              targetPositionIsValid = false;
            }
          }
        }

        // Abort if the iteration limit was exceeded (no valid target position found) 
        // or if the current movement is not positive.
        if (!targetPositionIsValid || Numeric.IsLessOrEqual(Vector3F.Dot(currentMovement, desiredMovement), 0))
        {
          targetPositionIsValid = false;
          break;
        }

        // Update position and detect collisions.
        Position = startPosition + currentMovement;
        _collisionDomain.Update(CollisionObject);

        targetPositionIsValid = !HasUnallowedContact(Vector3F.Zero);
      }

      if (!targetPositionIsValid
          || Numeric.IsNaN(Position.Length)) // Position contains NaN!? We have messed it up :-(
      {
        // No valid position found --> Reset position.
        Position = startPosition;
        _collisionDomain.Update(CollisionObject);
      }
    }


    // Fills bounds with all current planes that limit the movement of the character
    // at the given position. 
    // Note: All planes are relative to the given position - usually the bottom of the
    // character controller.
    private void AddBounds(List<Plane> bounds, Vector3F position)
    {
      // Get contact sets from domain and add a plane for each contact.
      foreach (ContactSet contactSet in _collisionDomain.GetContacts(CollisionObject))
      {
        foreach (Contact contact in contactSet)
        {
          // Get the contact normal vector pointing to the character controller. 
          Vector3F normal = (contactSet.ObjectB == CollisionObject) ? contact.Normal : -contact.Normal;

          // The penetration depth measures how much the character controller penetrates the 
          // obstacle.
          float penetration = contact.PenetrationDepth;

          // We allow a bit of penetration. For numerical stability, we use only 90% of 
          // the allowed penetration. 
          penetration -= AllowedPenetration * 0.9f;

          // Add a plane that represents this movement limit.
          bounds.Add(new Plane(normal, position + normal * penetration));
        }
      }
    }


    // Returns true if the character stands on the ground.
    private bool HasGroundContact()
    {
      // Iterate over all contact sets. For each contact set we check all contacts.
      foreach (ContactSet set in _collisionDomain.GetContacts(CollisionObject))
      {
        foreach (Contact contact in set)
        {
          // Get the contact normal vector pointing to the character controller. 
          Vector3F normal = (set.ObjectB == CollisionObject) ? contact.Normal : -contact.Normal;

          // If the contact position height is on the lower cap of the capsule 
          // and if the slope of the contact is allowed, we have ground contact.
          if (contact.Position.Y < Position.Y + Width / 2 && IsAllowedSlope(normal))
            return true;
        }
      }

      return false;
    }


    // Returns true if there are any contacts that are not allowed for the given movement vector.
    private bool HasUnallowedContact(Vector3F movement)
    {
      // Iterate over all contact sets. For each contact set we check all contacts.
      foreach (ContactSet contactSet in _collisionDomain.GetContacts(CollisionObject))
      {
        foreach (Contact contact in contactSet)
        {
          // Get the contact normal vector pointing to the character controller. 
          Vector3F normal = (contactSet.ObjectB == CollisionObject) ? contact.Normal : -contact.Normal;

          // It is ok if the normal vector of a contact points into the movement direction because
          // in this case we move away from the obstacle.
          // If the normal vector points against the movement direction and the penetration depth
          // is larger than the AllowedPenetration, we have an unallowed contact.
          if ((movement == Vector3F.Zero || Numeric.IsLess(Vector3F.Dot(normal, movement), 0))
              && contact.PenetrationDepth > AllowedPenetration)
          {
            return true;
          }
        }
      }
      return false;
    }


    // Returns true if 'normal' is a plane normal of a plane where we can stand on.
    private bool IsAllowedSlope(Vector3F normal)
    {
      // If dot product of normal and up-vector is greater than the
      // cosine of the max slope angle, then we can stand on the plane.
      return Vector3F.Dot(normal, Vector3F.UnitY) >= (float)Math.Cos(SlopeLimit);
    }
    #endregion
  }
}
