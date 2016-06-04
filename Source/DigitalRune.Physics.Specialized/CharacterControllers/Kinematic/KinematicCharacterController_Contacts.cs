// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Settings;


namespace DigitalRune.Physics.Specialized
{
  public partial class KinematicCharacterController
  {
    // In this file: All members related to collision detection, contacts and bounding planes.

    // During sliding the CC finds other collision objects in a certain range - see
    // CollectObstacles(). UpdateContacts() runs collision detection between the CC and
    // the obstacles and stores CCContacts in a list. AddBounds() converts the contacts 
    // to boundary planes of the allowed move space.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private struct CCContact
    {
      public Vector3F Position;       // Local position on the capsule.
      public Vector3F Normal;         // Normal pointing to capsule.
      public float PenetrationDepth;
    }
    #endregion


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// Contacts at the bottom cap of the capsule are considered "opposite" if the angle between the
    /// contacts is greater than a given angle. (In <see cref="HasGroundContact"/> we compare the
    /// dot-product with the cosine of this angle.)
    /// </summary>
    private static readonly float OppositeContactLimit = (float)Math.Cos(MathHelper.ToRadians(120));
    // Note: 3 contacts distributed in 120° around the cap is the case with the smallest possible 
    // angle between any two contacts.
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Result of CollectObstacles(). In each ContactSet the CC is the first object.
    private readonly List<ContactSet> _obstacles = new List<ContactSet>();

    // Result of UpdateContacts(). 
    private readonly List<CCContact> _contacts = new List<CCContact>();

    // Result of AddBounds().
    private readonly List<Plane> _bounds = new List<Plane>();

    // HasGroundContact is evaluated lazy and stored in this field. _hasGroundContact is
    // set to null in UpdateContacts().
    private bool? _hasGroundContact;
    private readonly List<Vector3F> _bottomContacts = new List<Vector3F>();

    // For BackupContacts()/RollbackContacts():
    private readonly List<CCContact> _backupContacts = new List<CCContact>();
    private bool? _backupHasGroundContact;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    private float AllowedPenetration
    {
      // Note: Large AllowedPenetration
      // If the allowed penetration is large, following happens after a jump: The character lands with
      // no penetration then it slowly sinks into the ground until full allowed penetration. 
      // Cause: The gravity velocity is reset at the first contact. After the landing 
      // StepDown will move the character down but the down movement is always limited to 
      // the desired movement length which is gravity * deltaTime². No need to fix this as long
      // reasonable AllowedPenetrations are used.

      get { return Simulation.Settings.Constraints.AllowedPenetration; }
    }


    private CollisionDetection CollisionDetection
    {
      get { return Simulation.CollisionDomain.CollisionDetection; }
    }


    /// <summary>
    /// Gets a value indicating whether this character has ground contact.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if character has ground contact; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// If this value is <see langword="true"/>, the character stands on a plane with allowed
    /// inclination or on several contacts that give enough support (e.g. pinched between 2 or
    /// more contacts). 
    /// </remarks>
    public bool HasGroundContact
    {
      // TODO: Maybe we should check the contact normal and not the height on the bottom cap.
      // Could be better for deep interpenetrations.
      get
      {
        if (_hasGroundContact.HasValue)
          return _hasGroundContact.Value;

        // We have ground contact if the contact position is on the bottom spherical cap.
        // The slopeLimit defines which slopes we can stand on. Using some trigonometry:
        float bottom = -Height / 2;                   // The bottom position of capsule.
        float capRadius = Width / 2;                  // The radius of the capsule caps.
        float bottomOfCylinder = bottom + capRadius;  // The bottom position of the cylindric part.

        float allowedRange = capRadius - capRadius * _cosSlopeLimit; // The allowed "height" of a contact on the bottom sphere.
        Debug.Assert(0 <= allowedRange && allowedRange <= capRadius);

        // A contact is considered a ground contact if it is within the allowed range.
        float groundContactLimit = bottom + allowedRange;

        _bottomContacts.Clear();
        int numberOfContacts = _contacts.Count;
        for (int i = 0; i < numberOfContacts; i++)
        {
          var contact = _contacts[i];

          // We have ground contact if the contact position is on the bottom spherical cap
          // near the bottom.
          if (contact.Position.Y <= groundContactLimit)
          {
            _hasGroundContact = true;
            return true;
          }

          // Store the contacts at the bottom cap of the capsule.
          if (contact.Position.Y <= bottomOfCylinder)
            _bottomContacts.Add(contact.Position);
        }

        // If we get here we do not have a ground contact, but we might have several contacts on
        // the bottom cap of the capsule. The capsule might be pinched between slopes.
        int numberOfBottomContacts = _bottomContacts.Count;
        if (numberOfBottomContacts < 2)
        {
          _hasGroundContact = false;
          return false;
        }

        // Make contacts relative to center of the bottom cap.
        Vector3F bottomCenter = new Vector3F(0, bottomOfCylinder, 0);
        for (int i = 0; i < numberOfBottomContacts; i++)
          _bottomContacts[i] = (_bottomContacts[i] - bottomCenter).Normalized;

        // Check if we have opposite contacts:
        // Contacts are opposite if the angle between the contacts is greater than a given limit.
        for (int i = 0; i < numberOfBottomContacts; i++)
        {
          Vector3F contactI = _bottomContacts[i];
          for (int j = i + 1; j < numberOfBottomContacts; j++)
          {
            Vector3F contactJ = _bottomContacts[j];
            if (Vector3F.Dot(contactI, contactJ) <= OppositeContactLimit)
            {
              // Contact i and j are on opposite sides.
              _hasGroundContact = true;
              return true;
            }
          }
        }

        _hasGroundContact = false;
        return false;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Stores a copy of the contacts.
    /// </summary>
    private void BackupContacts()
    {
      _backupContacts.Clear();

      // (Note: We could use _backupContacts.AddRange(_contacts) instead of the foreach-loop.
      // But AddRange() creates garbage on the managed heap!)
      foreach (var contact in _contacts)
        _backupContacts.Add(contact);
      
      _backupHasGroundContact = _hasGroundContact;
    }


    /// <summary>
    /// Restores the contacts to the last copy made with <see cref="BackupContacts"/>.
    /// </summary>
    private void RollbackContacts()
    {
      _contacts.Clear();

      // (Note: We could use _contacts.AddRange(_backupContacts) instead of the foreach-loop.
      // But AddRange() creates garbage on the managed heap!)
      foreach (var contact in _backupContacts)
        _contacts.Add(contact);

      _hasGroundContact = _backupHasGroundContact;
    }


    /// <summary>
    /// Searches for possible obstacles in the movement radius. A contact set is created 
    /// for each found object.
    /// </summary>
    /// <param name="radius">The radius.</param>
    private void CollectObstacles(float radius)
    {
      // Recycle old contacts and contact sets.
      foreach (var contactSet in _obstacles)
        contactSet.Recycle(true);

      _obstacles.Clear();

      // In "ghost mode" we can ignore obstacles.
      if (Body.CollisionObject.Enabled == false)
        return;

      // Compute AABB and extend it by the movement radius.
      Aabb aabb = Body.Aabb;
      aabb.Minimum = aabb.Minimum - new Vector3F(radius);
      aabb.Maximum = aabb.Maximum + new Vector3F(radius);

      // Use broad-phase to get obstacles and create contact sets.
      IEnumerable<CollisionObject> overlaps = Simulation.CollisionDomain.BroadPhase.GetOverlaps(aabb);
      foreach (var collisionObject in overlaps)
      {
        if (collisionObject != Body.CollisionObject) // Ignore self-overlap.
        {
          var contactSet = ContactSet.Create(Body.CollisionObject, collisionObject);
          _obstacles.Add(contactSet);
        }
      }
    }


    /// <summary>
    /// Updates the contact sets with the obstacles and stores the contacts.
    /// </summary>
    private void UpdateContacts()
    {
      _contacts.Clear();
      _hasGroundContact = null;

      int numberOfObstacles = _obstacles.Count;
      for (int i = 0; i < numberOfObstacles; i++)
      {
        var contactSet = _obstacles[i];

        // Let the collision detection compute the contact information (positions, normals, 
        // penetration depths, etc.).
        CollisionDetection.UpdateContacts(contactSet, 0);

        int numberOfContacts = contactSet.Count;
        for (int j = 0; j < numberOfContacts; j++)
        {
          var contact = contactSet[j];
          _contacts.Add(new CCContact
            {
              Position = contact.PositionALocal,  // Position in local space of character.
              Normal = -contact.Normal,           // Normal that points to character.
              PenetrationDepth = contact.PenetrationDepth,
            });
        }
      }
    }


    /// <summary>
    /// Stores all current boundary planes of the character in a list.
    /// </summary>
    /// <param name="position">The current character position.</param>
    /// <returns>The number of new boundary planes added.</returns>
    /// <remarks>
    /// Duplicate planes are not added.
    /// </remarks>
    private int AddBounds(Vector3F position)
    {
      int oldNumberOfBounds = _bounds.Count;

      // Add a plane for each contact.
      int numberOfContacts = _contacts.Count;
      for (int i = 0; i < numberOfContacts; i++)
      {
        var contact = _contacts[i];
        Vector3F normal = contact.Normal;
        float penetrationDepth = contact.PenetrationDepth;
        Plane plane = new Plane(normal, position + normal * penetrationDepth);

        // Check if a similar plane exists. 
        bool planeIsNew = true;

        int numberOfBounds = _bounds.Count;
        for (int j = 0; j < numberOfBounds; j++)
        {
          Plane existingPlane = _bounds[j];
          if (Vector3F.AreNumericallyEqual(existingPlane.Normal, plane.Normal, CollisionDetection.Epsilon)
              && Numeric.AreEqual(existingPlane.DistanceFromOrigin, plane.DistanceFromOrigin, CollisionDetection.Epsilon))
          {
            planeIsNew = false;
          }
        }

        if (planeIsNew)
        {
          // Add new plane.
          // Treat planes which block movement first. Because "blocking" constraints should have 
          // higher priority. If we do not sort the planes, then following could happen in the 
          // simplex solver: The character is bound by a ground plane and a blocking plane. First 
          // the character moves up to stand on the ground plane and not inside, then the character 
          // moves back to be out of the blocking plane. --> Now the character is in the air! 
          // Therefore, treat blocking planes first because they create only a horizontal correction
          // and thus are "safe".
          if (IsAllowedSlope(plane.Normal))
          {
            // Append at end of list.
            _bounds.Add(plane);
          }
          else
          {
            // "Blocking" plane: Insert at start of list.
            _bounds.Insert(0, plane);
          }
        }
      }

      return _bounds.Count - oldNumberOfBounds;
    }


    /// <summary>
    /// Determines whether the specified current movement has a forbidden contact.
    /// </summary>
    /// <param name="currentMovement">The current movement.</param>
    /// <returns>
    /// <see langword="true"/> if the specified current movement has a forbidden contact; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// If the penetration depth of a contact is within the 
    /// <see cref="ConstraintSettings.AllowedPenetration"/> depth or if the character controller is 
    /// moving out of a contact, then this contact is "allowed". This method reports 
    /// <see langword="true"/> if there are "forbidden" contacts, which are contacts where the
    /// contact normal points against the movement direction and where the penetration depth is
    /// above the allowed limit.
    /// </remarks>
    private bool HasUnallowedContact(Vector3F currentMovement)
    {
      bool noMovement = (currentMovement == Vector3F.Zero);
      float maxPenetrationDepth = AllowedPenetration + CollisionDetection.Epsilon;

      int numberOfContacts = _contacts.Count;
      for (int i = 0; i < numberOfContacts; i++)
      {
        var contact = _contacts[i];
        if ((noMovement || Numeric.IsLess(Vector3F.Dot(contact.Normal, currentMovement), 0))
            && contact.PenetrationDepth > maxPenetrationDepth)
        {
          return true;
        }
      }

      // No forbidden contacts.
      return false;
    }


    /// <summary>
    /// Determines whether the given normal belongs to a plane where the character can stand on.
    /// </summary>
    /// <param name="normal">The normal.</param>
    /// <returns>
    /// <see langword="true"/> if the slope is in the allowed range; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    internal bool IsAllowedSlope(Vector3F normal)
    {
      return Vector3F.Dot(normal, UpVector) >= _cosSlopeLimit;
    }
    #endregion
  }
}
