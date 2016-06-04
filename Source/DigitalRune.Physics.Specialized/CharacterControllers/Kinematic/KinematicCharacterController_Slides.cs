// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Settings;


namespace DigitalRune.Physics.Specialized
{
  public partial class KinematicCharacterController
  {
    // In this file: Methods for sliding.

    // Sliding moves the body directly (not using the Simulation). The slide methods are:
    //   ResolvePenetrations ... slides out of invalid contacts.
    //   Fly ... slides in any direction without gravity. 
    //   Slide ... slides along the ground. 
    //   StepUp ... moves up onto a step.
    //   StepDown ... tries to move down to ground where the CC can stand.
    //
    // While sliding, the character controller does not push other bodies.
    // CollectObstacles() must be called manually before a slide method is called (except for 
    // ResolvePenetrations()).
    //
    // Note: Finding a target position using linear programming (see Fly(), Slide()):
    // From all found contacts we create planes that describe the convex polyhedral space in which 
    // the character can move. We search for the target position using simple relaxation (iterative 
    // correction).
    //
    // Note: If the convex polyhedron of the bounding planes has sharp edges, it can happen that the 
    // relaxation converges very slowly. For example, if the iteration limit is 4 and the character 
    // runs into a wedge-like corner, the 4 iterations will not find a solution. 
    // Solutions: Use more iterations, a different LP solving method or stuff that speeds up 
    // convergence, ...
    //
    // Note: How to move out of a plane?
    // If a point is inside the halfspace of a plane, where the normal penetration depth to the 
    // plane is d, the following formula computes a correction vector. The correction vector moves 
    // the point from inside the plane halfspace in a given direction (normalized!) towards the 
    // plane surface.
    //   Vector3F correction = d / Vector3F.Dot(direction, plane.Normal) * direction;
    // Derivation: 
    // d is penetration depth in the direction of the plane normal. d * plane.Normal and correction
    // vector form a triangle with a right angle. The cosine of the angle between the normal and 
    // the correction vector is equal to Vector3F.Dot(direction, plane.Normal). Finally, apply the 
    // cosine definition formula.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the number of slide iterations.
    /// </summary>
    /// <value>The number of slide iterations in the range [1, ∞[. The default value is 4.</value>
    /// <remarks>
    /// The character controller will slide from contact to the next contact until it finds a 
    /// position near the target position. <see cref="NumberOfSlideIterations"/> is the maximal
    /// iteration limit. Setting this property to a lower value can make the movement less smooth
    /// and the character controller could stop at small obstacles and steps.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public int NumberOfSlideIterations
    {
      get { return _numberOfSlideIterations; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", "The number of slide iterations must be greater than 0.");

        _numberOfSlideIterations = value;
      }
    }
    private int _numberOfSlideIterations = 4;


    /// <summary>
    /// Gets or sets the number of solver iterations.
    /// </summary>
    /// <value>The number of solver iterations in the range [1, ∞[. The default value is 4.</value>
    /// <remarks>
    /// <para>
    /// In each slide movement the character controller gathers bounding planes that form a convex
    /// space in which it may move. An iterative solver is used to find a valid position in this
    /// convex space that is nearest to the target position.
    /// </para>
    /// <para>
    /// <see cref="NumberOfSolverIterations"/> is the maximal iteration limit. Setting this 
    /// property to a lower value can make the movement less smooth and the character controller 
    /// could stop at small obstacles and steps.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public int NumberOfSolverIterations
    {
      get { return _numberOfSolverIterations; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", "The number of solver iterations must be greater than 0.");

        _numberOfSolverIterations = value;
      }
    }
    private int _numberOfSolverIterations = 4;
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Tries to move the character to the nearest position where it does not penetrate other
    /// objects.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the character is in or moved to a non-penetrating position;
    /// otherwise, <see langword="false"/> if the penetrations could not be resolved.
    /// </returns>
    /// <remarks>
    /// This method does nothing if the character controller is disabled. For deep interpenetrations
    /// this method might not find a solution. Penetrations are only removed up to the 
    /// penetration depth (see <see cref="ConstraintSettings.AllowedPenetration"/>).
    /// </remarks>
    public bool ResolvePenetrations()
    {
      if (!Enabled)
        return false;

      // TODO: Improve ResolvePentrations().
      // If we cannot resolve from penetrations, we could try to randomly move into any
      // direction and try again...

      // Store the initial position (in case of rollback).
      Vector3F startPosition = Position;

      // Gather all obstacles in a small range and determine all contacts.
      CollectObstacles(2 * Width);
      UpdateContacts();

      // Abort if there are no unallowed penetrations.
      bool hasUnallowedContact = HasUnallowedContact(Vector3F.Zero);
      if (!hasUnallowedContact)
      {
        // Nothing to do.
        return true;
      }

      // From all contacts we form a convex boundary. The capsule must stay within this boundary.
      _bounds.Clear();

      int iterationCount = 0;
      do
      {
        iterationCount++;

        // Add new boundary planes for the current position.
        AddBounds(Position);

        // Find a valid position within the convex space formed. In a relaxation process resolve 
        // penetrations by moving straight out of each plane.
        int solverIterationCount = 0;
        bool shouldRestart;
        do
        {
          solverIterationCount++;
          shouldRestart = false;

          // Sequentially correct the penetration of each boundary plane.
          int numberOfBounds = _bounds.Count;
          for (int i = 0; i < numberOfBounds; i++)
          {
            Plane plane = _bounds[i];

            // Get normal distance (minus allowed penetration). Negative distance = penetration.
            float distance = GeometryHelper.GetDistance(plane, Position) + AllowedPenetration;
            if (Numeric.IsLess(distance, 0))
            {
              // We simply recover from penetration by moving along the normal direction.
              Position += plane.Normal * (-distance);

              // Target position has changed. Relaxation has to continue.
              shouldRestart = true;
            }
          }
        } while (shouldRestart && solverIterationCount < NumberOfSolverIterations);

        // Find contacts at new position.
        UpdateContacts();

        // Give up if the iteration limit was reached.
        if (solverIterationCount >= NumberOfSolverIterations)
          break;

        // Check for forbidden contacts at the new position.
        hasUnallowedContact = HasUnallowedContact(Vector3F.Zero);
      } while (hasUnallowedContact && iterationCount < NumberOfSlideIterations);

      // Rollback if there was a numerical problem or if we didn't remove all penetrations.
      if (Position.IsNaN || hasUnallowedContact)
      {
        Position = startPosition;

        // Recompute contacts at original position.
        UpdateContacts();

        return false;
      }

      return true;
    }


    /// <summary>
    /// Flies to a new position.
    /// </summary>
    /// <remarks>
    /// When flying the character will smoothly slide along all slopes it encounters. The 
    /// <see cref="SlopeLimit"/> is not applied.
    /// </remarks>
    private void Fly()
    {
      // Compute the desired movement.
      Vector3F desiredMovement = _desiredPosition - _oldPosition;
      if (desiredMovement.IsNumericallyZero)
      {
        // Nothing to do.
        return;
      }

      // From all contacts we form a convex boundary. The capsule must stay within this boundary.
      _bounds.Clear();

      // Store initial position and contacts (in case of rollback).
      Vector3F startPosition = Position;
      BackupContacts();

      bool hasUnallowedContacts = true;  // Assume that we do not find a valid position.
      int iterationCount = 0;
      do
      {
        iterationCount++;

        // Add new boundary planes for the current position.
        AddBounds(Position);

        // The next solver iterations will try to find a valid current movement.
        // Start with the desired movement.
        Vector3F currentMovement = desiredMovement;

        bool targetPositionFound;
        int solverIterationCount = 0;
        do
        {
          solverIterationCount++;
          targetPositionFound = true;

          // Sequentially correct the penetration of each boundary plane.
          int numberOfBounds = _bounds.Count;
          for (int i = 0; i < numberOfBounds; i++)
          {
            Plane plane = _bounds[i];

            // Ignore the plane if its normal points into the same direction as the current 
            // movement direction.
            if (Numeric.IsGreaterOrEqual(Vector3F.Dot(plane.Normal, currentMovement), 0))
              continue;

            // Get normal distance (minus allowed penetration). Negative distance = penetration.
            float distance = GeometryHelper.GetDistance(plane, startPosition + currentMovement) + AllowedPenetration;
            if (Numeric.IsLess(distance, 0))
            {
              // We simply recover from penetration by moving into the plane normal direction.
              // This creates a nice sliding movement.
              Vector3F correction = plane.Normal * (-distance);
              currentMovement += correction;

              // Target position has changed. Relaxation has to continue.
              targetPositionFound = false;
            }
          }
        } while (!targetPositionFound && solverIterationCount < NumberOfSolverIterations);

        // Abort if the relaxation couldn't find an allowed position, or we would move backwards.
        if (solverIterationCount >= NumberOfSolverIterations
            || Numeric.IsLessOrEqual(Vector3F.Dot(currentMovement, desiredMovement), 0))
        {
          break;
        }

        // We have a new position which we can test.
        Position = startPosition + currentMovement;

        // Find contacts at new position.
        UpdateContacts();

        // Check for forbidden contacts at the new position. (Contacts that point against our 
        // current movement direction. If we don't find any, we are done.)
        hasUnallowedContacts = HasUnallowedContact(currentMovement);
      } while (hasUnallowedContacts && iterationCount < NumberOfSlideIterations);

      // Rollback if there was a numerical problem or if we didn't remove all penetrations.
      if (Position.IsNaN || hasUnallowedContacts)
      {
        // No improvement --> Rollback.
        Position = startPosition;
        RollbackContacts();
      }
    }


    /// <summary>
    /// Slides to a new position.
    /// </summary>
    /// <param name="stopAtObstacle">
    /// if set to <see langword="true"/> the slide stops at the first obstacle that is no
    /// ground plane.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the movement was finished.
    /// <see langword="false"/> if the movement was stopped at an obstacle.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private bool Slide(bool stopAtObstacle)
    {
      // Compute the desired movement.
      Vector3F desiredMovement = _desiredPosition - Position;
      if (desiredMovement.IsNumericallyZero)
      {
        // Nothing to do.
        return true;
      }

      Vector3F desiredMovementDirection = desiredMovement.Normalized;
      Vector3F desiredHorizontalMovementDirection = desiredMovementDirection - Vector3F.Dot(desiredMovementDirection, UpVector) * UpVector;

      // From all contacts we form a convex boundary. The capsule must stay within this boundary.
      _bounds.Clear();

      // Store initial position and contacts (in case of rollback).
      Vector3F startPosition = Position;
      BackupContacts();

      bool blocked = false;   // true if a steep plane was hit.
      bool commit = false;    // Assume that we do not find a valid position.
      bool startedOnGround = HasGroundContact;
      int iterationCount = 0;
      bool noSlide = false;
      bool onlyLateral = false;
      do
      {
        iterationCount++;

        // Add new boundary planes for the current position.
        AddBounds(Position);

        // The next solver iterations will try to find a valid current movement.
        // Start with the desired movement.
        Vector3F currentMovement = desiredMovement;

        bool targetPositionFound;
        int solverIterationCount = 0;
        do
        {
          solverIterationCount++;
          targetPositionFound = true;

          // Sequentially correct the penetration of each boundary plane.
          int numberOfBounds = _bounds.Count;
          for (int i = 0; i < numberOfBounds; i++)
          {
            Plane plane = _bounds[i];

            // Ignore the plane if its normal points into the same direction as the current 
            // movement direction.
            if (Numeric.IsGreaterOrEqual(Vector3F.Dot(plane.Normal, currentMovement), 0))
              continue;

            // Get normal distance (minus allowed penetration). Negative distance = penetration.
            float distance = GeometryHelper.GetDistance(plane, startPosition + currentMovement) + AllowedPenetration;
            if (Numeric.IsLess(distance, 0))
            {
              // Slide along the wall: We simply recover from penetration by moving into the 
              // plane normal direction.
              Vector3F correction = plane.Normal * (-distance);

              if (!IsAllowedSlope(plane.Normal))
              {
                // Slope is above slope limit.

                if (stopAtObstacle)
                {
                  // We should stop at obstacle: Move back until there is no penetration.
                  correction = -distance / Vector3F.Dot(desiredMovementDirection, plane.Normal)
                               * desiredMovementDirection;
                  blocked = true;
                }
                else if (noSlide)
                {
                  // Cut lateral movement.
                  correction = -distance / Vector3F.Dot(desiredHorizontalMovementDirection, plane.Normal)
                               * desiredHorizontalMovementDirection;
                }
                else if (onlyLateral || Vector3F.Dot(correction, UpVector) > 0)
                {
                  // Slide laterally. (Don't slide "up" forbidden slopes.)
                  Vector3F correctionDirection = plane.Normal - Vector3F.Dot(plane.Normal, UpVector) * UpVector;
                  if (correctionDirection.TryNormalize())
                  {
                    correction = -distance / Vector3F.Dot(correctionDirection, plane.Normal) 
                                 * correctionDirection;
                  }
                }
              }

              currentMovement += correction;

              if (currentMovement.Length > desiredMovement.Length)
                currentMovement.Length = desiredMovement.Length;

              // Target position has changed. Relaxation has to continue.
              targetPositionFound = false;
            }
          }
        } while (!targetPositionFound && solverIterationCount < NumberOfSolverIterations);

        if (solverIterationCount >= NumberOfSolverIterations)
        {
          // Relaxation didn't converge to a solution. This usually happens in V slopes. 

          // Possible problem when CC is standing on V:
          // At the beginning only one side is touched --> The user cannot jump form this position.
          // Gravity pulls down, but the correction is only horizontal because of the
          // steep slopes. And because the correction never moves up into a valid position
          // no valid solution can be found. The problem is that the character stops at the
          // current position and will never get a second contact. 
          // --> Detect this case and allow up correction.

          // Solution 1: Try once more with half the desired movement.
          // Abort when desired movement falls under arbitrary limit. 
          //desiredMovement *= 0.5f;
          //if (desiredMovement.LengthSquared < (AllowedPenetration * AllowedPenetration / 4))
          //  break;

          // Solution 2: Try again with different sliding strategies. 
          if (!onlyLateral && !desiredHorizontalMovementDirection.IsNumericallyZero)
          {
            // Allow only lateral slides. This helps when the head scratches against a slope.
            onlyLateral = true;
          }
          else if (!noSlide && !desiredHorizontalMovementDirection.IsNumericallyZero)
          {
            // If onlyLateral does not help, then we do not slide laterally and only cut the
            // horizontal movement. This helps in a corner where to walls meet at a small angle.
            noSlide = true;
          }
          else if (Numeric.IsLess(Vector3F.Dot(desiredMovementDirection, UpVector), 0) 
                   && desiredHorizontalMovementDirection.IsNumericallyZero)
          {
            // The solutions above didn't get us anywhere and down movement didn't get us anywhere.
            // That means we are standing on ground and allow the user to jump in the next frame.
            commit = true;
            _hasGroundContact = true;
            break;
          }
          else
          {
            break;
          }
        }

        // We have a new position which we can test.
        Position = startPosition + currentMovement;

        // Find contacts at new position.
        UpdateContacts();

        if (startedOnGround && Vector3F.Dot(currentMovement, desiredHorizontalMovementDirection) < -AllowedPenetration)
        {
          // Abort if we started on the ground and would move backward. Backward corrections are 
          // ok if we started airborne.
          blocked = true;
          break;
        }

        // Check for forbidden contacts at the new position. (Contacts that point against our 
        // current movement direction. If we don't find any, we are done.)
        commit = !HasUnallowedContact(currentMovement);

        if (commit                                                                  // Movement finished.
            && startedOnGround                                                      // We started on standing on ground.
            && Numeric.IsZero(Vector3F.Dot(desiredMovement.Orthonormal1, UpVector)) // The desired movement is entirely "down".
            && Vector3F.Dot(currentMovement, UpVector) <= 0)                        // The resulting movement has a "down" component.
        {
          // Abort. We don't want to slide down when desiredMovement was straight down (gravity!).
          // Without this check, the character will slide down on an inclined plane.
          commit = false;
          break;
        }
      } while (!commit && iterationCount < NumberOfSlideIterations);

      // Rollback if there was a numerical problem or if we didn't remove all penetrations.
      if (Position.IsNaN || !commit)
      {
        // No improvement --> roll back.
        Position = startPosition;
        RollbackContacts();
      }

      return !blocked;
    }


    /// <summary>
    /// Tries to step up onto an obstacle.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a step was made; otherwise <see langword="false"/> if the
    /// <see cref="Position"/> was not changed.
    /// </returns>
    /// <remarks>
    /// This method does not change the position if no up-step can be made.
    /// </remarks>
    private bool StepUp()
    {
      Vector3F startPosition = Position;
      Vector3F desiredMovement = _desiredPosition - startPosition;

      // Compute forward direction (= movement normal to the up direction). 
      // Abort if the movement is not forward directed.
      Vector3F forward = desiredMovement - Vector3F.ProjectTo(desiredMovement, UpVector);
      if (!forward.TryNormalize())
        return false;

      // Store contacts for a possible rollback.
      BackupContacts();

      // Test if there is enough room if we step up and forward. There must be at least room
      // for half the capsule.
      Position = Position + UpVector * StepHeight + forward * (Width / 2 - 2 * AllowedPenetration);
      UpdateContacts();
      if (!HasUnallowedContact(Vector3F.Zero))
      {
        // There is enough room! :-)

        // Now check if there is something where we can stand on (within slope limit).
        bool hasGroundContact = StepDown(true);
        if (hasGroundContact)
        {
          // There is enough room and there is ground to stand on. :-)
          return true;
        }
      }

      // Rollback movement.
      Position = startPosition;
      RollbackContacts();

      return false;
    }


    /// <summary>
    /// Tries to move the <see cref="StepHeight"/> downwards.
    /// </summary>
    /// <param name="onlyOntoAllowedSlopes">
    /// If set to <see langword="true"/>, the character will only step down if it touches an
    /// allowed slope at the end of the step. If set to <see langword="false"/>, the character
    /// will step down as long as it touches anything on the capsule bottom.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a valid ground was found.
    /// </returns>
    /// <remarks>
    /// The step ends on the first contact. If there is no contact found within the step height,
    /// this method does nothing.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private bool StepDown(bool onlyOntoAllowedSlopes)
    {
      if (HasGroundContact)
      {
        // Nothing to do.
        return true;
      }

      // We try a downward movement with the StepHeight.
      if (Numeric.IsZero(StepHeight))
        return true;

      Vector3F desiredMovement = -UpVector * StepHeight;

      // This method searches for valid ground contacts using binary search (bisecting the 
      // step height). Binary search is only performed if a down-step with full height touches
      // anything.
      bool bisect = false;
      Vector3F startPosition = Position;
      Vector3F safeMovement = new Vector3F();
      Vector3F currentMovement = desiredMovement;

      // From all contacts we form a convex boundary. The capsule must stay within this boundary.
      _bounds.Clear();

      // Store contacts for a possible rollback.
      BackupContacts();

      bool hasUnallowedContacts = false;
      bool hasBottomContact = false;
      bool foundAllowedSlope = false;
      int iterationCount = 0;
      do
      {
        iterationCount++;

        // Add new boundary planes for the current position.
        AddBounds(Position);

        int solverIterationCount = 0;
        bool targetPositionFound;
        hasBottomContact = false;
        do
        {
          solverIterationCount++;
          targetPositionFound = true;

          int numberOfBounds = _bounds.Count;
          for (int i = 0; i < numberOfBounds; i++)
          {
            Plane plane = _bounds[i];

            // Ignore the plane if its normal points downwards.
            if (Numeric.IsGreaterOrEqual(Vector3F.Dot(plane.Normal, -UpVector), 0, 0.001f))
              continue;

            // Get normal distance (minus allowed penetration). Negative distance = penetration.
            float distance = GeometryHelper.GetDistance(plane, startPosition + currentMovement) + AllowedPenetration;

            if (distance < AllowedPenetration)
            {
              // Remember that we have found at least one valid contact.
              hasBottomContact = true;
              foundAllowedSlope = foundAllowedSlope || IsAllowedSlope(plane.Normal);
            }

            if (Numeric.IsLess(distance, 0))
            {
              // We correct the position upwards. We do not slide.
              Vector3F correction = (-distance) / Vector3F.Dot(UpVector, plane.Normal) * UpVector;
              currentMovement += correction;

              // Target position has changed. Relaxation has to continue.
              targetPositionFound = false;
            }
          }
        } while (solverIterationCount < NumberOfSolverIterations && !targetPositionFound);

        bool makingProgress = Numeric.IsGreater(
          Vector3F.Dot(currentMovement, -UpVector), // The downward distance we have found in this iteration.
          Vector3F.Dot(safeMovement, -UpVector));   // We can move at least this distance downwards.

        if (iterationCount == 1 && (!targetPositionFound || !makingProgress))
        {
          // First iteration. No movement possible with contacts of start position.
          break;
        }

        bool noPositionChange = Vector3F.AreNumericallyEqual(startPosition + currentMovement, Position);

        if (iterationCount > 1 && noPositionChange)    // noPositionChange is always true in the first iteration.
        {
          if (!bisect)
          {
            // Solver couldn't change position and we are not using binary search.
            break;
          }

          // Try again with bisected distance.
          _bounds.Clear();
          currentMovement = (safeMovement + desiredMovement) / 2;
        }

        if (!targetPositionFound || !makingProgress)
        {
          // With large step heights we can get deep interpenetrations with horizontal
          // contact normals. Horizontal normals can create very high upward corrections.

          // We cannot use these normal.
          _bounds.Clear();
          hasBottomContact = false;

          // Try again with bisected distance.
          currentMovement = (safeMovement + desiredMovement) / 2;
        }

        Position = startPosition + currentMovement;
        UpdateContacts();

        hasUnallowedContacts = HasUnallowedContact(Vector3F.Zero);

        // Bilinear search is only done if any check finds contacts.
        bisect = bisect || hasUnallowedContacts;

        // If we have unallowed contacts, we reduce the desiredMovement. 
        // Otherwise, we can extend safeMovement.
        if (hasUnallowedContacts)
          desiredMovement = currentMovement;
        else
          safeMovement = currentMovement;

      } while ((hasUnallowedContacts || !hasBottomContact || (bisect && _contacts.Count == 0)) && iterationCount < NumberOfSlideIterations);

      if (Position.IsNaN || hasUnallowedContacts || !hasBottomContact || (onlyOntoAllowedSlopes && !foundAllowedSlope))
      {
        // No step down. --> Rollback.
        Position = startPosition;
        RollbackContacts();
        return false;
      }

      return true;
    }
    #endregion
  }
}
