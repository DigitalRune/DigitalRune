// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// Helper methods for managing collision contacts.
  /// </summary>
  public static class ContactHelper
  {
    /// <overloads>
    /// <summary>
    /// Creates a new <see cref="Contact"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates a new <see cref="Contact"/> for the given pair of collision objects.
    /// </summary>
    /// <param name="objectA">The first collision object. (Must not be <see langword="null"/>.)</param>
    /// <param name="objectB">The second collision object. (Must not be <see langword="null"/>.)</param>
    /// <param name="position">The contact position.</param>
    /// <param name="normal">The normal vector. Needs to be normalized.</param>
    /// <param name="penetrationDepth">The penetration depth.</param>
    /// <param name="isRayHit">
    /// If set to <see langword="true"/> the contact is a hit by a ray (see 
    /// <see cref="Contact.IsRayHit"/>).
    /// </param>
    /// <returns>A new <see cref="Contact"/>.</returns>
    /// <remarks>
    /// This method copies the given information into a new contact and initializes the local 
    /// contact positions (<see cref="Contact.PositionALocal"/> and 
    /// <see cref="Contact.PositionBLocal"/>). <paramref name="objectA"/> and 
    /// <paramref name="objectB"/> are only used to compute the local contact positions.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Contact CreateContact(CollisionObject objectA,
                                        CollisionObject objectB,
                                        Vector3F position,
                                        Vector3F normal,
                                        float penetrationDepth,
                                        bool isRayHit)
    {
      Debug.Assert(objectA != null);
      Debug.Assert(objectB != null);

      Contact contact = Contact.Create();
      contact.Position = position;
      contact.Normal = normal;
      contact.PenetrationDepth = penetrationDepth;
      contact.IsRayHit = isRayHit;

      if (isRayHit)
      {
        contact.PositionALocal = objectA.GeometricObject.Pose.ToLocalPosition(position);
        contact.PositionBLocal = objectB.GeometricObject.Pose.ToLocalPosition(position);
      }
      else
      {
        //Vector3F halfPenetration = normal * (penetrationDepth / 2);
        //contact.PositionALocal = objectA.GeometricObject.Pose.ToLocalPosition(position + halfPenetration);
        //contact.PositionBLocal = objectB.GeometricObject.Pose.ToLocalPosition(position - halfPenetration);

        // ----- Optimized version:
        float halfPenetration = penetrationDepth / 2;
        Vector3F halfPenetrationVector;
        halfPenetrationVector.X = normal.X * halfPenetration;
        halfPenetrationVector.Y = normal.Y * halfPenetration;
        halfPenetrationVector.Z = normal.Z * halfPenetration;

        var poseA = objectA.GeometricObject.Pose;
        Vector3F diffA;
        diffA.X = position.X + halfPenetrationVector.X - poseA.Position.X;
        diffA.Y = position.Y + halfPenetrationVector.Y - poseA.Position.Y;
        diffA.Z = position.Z + halfPenetrationVector.Z - poseA.Position.Z;
        Vector3F positionALocal;
        positionALocal.X = poseA.Orientation.M00 * diffA.X + poseA.Orientation.M10 * diffA.Y + poseA.Orientation.M20 * diffA.Z;
        positionALocal.Y = poseA.Orientation.M01 * diffA.X + poseA.Orientation.M11 * diffA.Y + poseA.Orientation.M21 * diffA.Z;
        positionALocal.Z = poseA.Orientation.M02 * diffA.X + poseA.Orientation.M12 * diffA.Y + poseA.Orientation.M22 * diffA.Z;
        contact.PositionALocal = positionALocal;

        var poseB = objectB.GeometricObject.Pose;
        Vector3F diffB;
        diffB.X = position.X - halfPenetrationVector.X - poseB.Position.X;
        diffB.Y = position.Y - halfPenetrationVector.Y - poseB.Position.Y;
        diffB.Z = position.Z - halfPenetrationVector.Z - poseB.Position.Z;
        Vector3F positionBLocal;
        positionBLocal.X = poseB.Orientation.M00 * diffB.X + poseB.Orientation.M10 * diffB.Y + poseB.Orientation.M20 * diffB.Z;
        positionBLocal.Y = poseB.Orientation.M01 * diffB.X + poseB.Orientation.M11 * diffB.Y + poseB.Orientation.M21 * diffB.Z;
        positionBLocal.Z = poseB.Orientation.M02 * diffB.X + poseB.Orientation.M12 * diffB.Y + poseB.Orientation.M22 * diffB.Z;
        contact.PositionBLocal = positionBLocal;
      }

      return contact;
    }


    /// <summary>
    /// Creates a new <see cref="Contact"/> for the given contact set.
    /// </summary>
    /// <param name="contactSet">
    /// The contact set for the colliding objects. (Must not be <see langword="null"/>.)
    /// </param>
    /// <param name="position">The contact position.</param>
    /// <param name="normal">The normal vector. Needs to be normalized.</param>
    /// <param name="penetrationDepth">The penetration depth.</param>
    /// <param name="isRayHit">
    /// If set to <see langword="true"/> the contact is a hit by a ray (see 
    /// <see cref="Contact.IsRayHit"/>).
    /// </param>
    /// <returns>A new <see cref="Contact"/>.</returns>
    /// <remarks>
    /// This method copies the given information into a new contact and initializes the local 
    /// contact positions (<see cref="Contact.PositionALocal"/> and 
    /// <see cref="Contact.PositionBLocal"/>). The <paramref name="contactSet"/> is only required 
    /// to compute the local contact positions.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Contact CreateContact(ContactSet contactSet,
                                        Vector3F position,
                                        Vector3F normal,
                                        float penetrationDepth,
                                        bool isRayHit)
    {
      Debug.Assert(contactSet != null);

      return CreateContact(contactSet.ObjectA, contactSet.ObjectB, position, normal, penetrationDepth, isRayHit);
    }


    /// <overloads>
    /// <summary>
    /// Merges new contacts into a contact set.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Merges a new contact into the given contact set.
    /// </summary>
    /// <param name="contactSet">The contact set. (Must not be <see langword="null"/>.)</param>
    /// <param name="newContact">The contact. (Must not be <see langword="null"/>.)</param>
    /// <param name="type">The type of collision query.</param>
    /// <param name="contactPositionTolerance">The contact position tolerance.</param>
    /// <remarks>
    /// <para>
    /// This method adds the given contact by merging it with an existing contact or by simply
    /// adding it to <paramref name="contactSet"/>. A contact is merged with an existing one if the
    /// difference of the contact positions is less than the 
    /// <paramref name="contactPositionTolerance"/> and the contact features are identical. Contacts
    /// of ray casts are always merged with existing contacts of the same features 
    /// (<paramref name="contactPositionTolerance"/> is not checked in this case).
    /// </para>
    /// <para>
    /// This method must not be called for Boolean ("HaveContact") queries since boolean queries do 
    /// not normally change contact information.
    /// </para>
    /// <para>
    /// For shapes where contacts are computed for child shapes, the feature 
    /// (<see cref="Contact.FeatureA"/> or <see cref="Contact.FeatureB"/>) must be set correctly in 
    /// <paramref name="newContact"/>. For example, this is necessary for 
    /// <see cref="CompositeShape"/>, 
    /// <see cref="TriangleMeshShape"/>, <see cref="HeightField"/> and similar shapes.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The <paramref name="newContact"/> will be recycled and cannot
    /// be accessed after this method call.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="contactSet"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void Merge(ContactSet contactSet, Contact newContact, CollisionQueryType type, float contactPositionTolerance)
    {
      Debug.Assert(contactSet != null);
      Debug.Assert(newContact != null);

      Debug.Assert(contactPositionTolerance >= 0, "The contact position tolerance must be greater than or equal to 0");
      Debug.Assert(type == CollisionQueryType.ClosestPoints || type == CollisionQueryType.Contacts);

      if (type == CollisionQueryType.Contacts && newContact.PenetrationDepth < 0)
        return; // Do not merge separated contacts.

      // ----- Simplest case: Contact set is empty.
      if (contactSet.Count == 0)
      {
        // Simply add the new contact.
        contactSet.Add(newContact);
        return;
      }

      // ----- Try to merge with nearest old contact.
      bool merged = TryMergeWithNearestContact(contactSet, newContact, contactPositionTolerance, true);
      if (merged)
      {
        newContact.Recycle();
        return;
      }

      // ----- Default: Add the new contact.
      contactSet.Add(newContact);
    }


    /// <summary>
    /// Merges a new set of contacts into the given contact set.
    /// </summary>
    /// <param name="target">The target contact. (Must not be <see langword="null"/>.)</param>
    /// <param name="newContacts">
    /// The contact set to merge. (Must not be <see langword="null"/>.)
    /// </param>
    /// <param name="type">The type of collision query.</param>
    /// <param name="contactPositionTolerance">The contact position tolerance.</param>
    /// <remarks>
    /// <para>
    /// This method calls <see cref="Merge(ContactSet, Contact, CollisionQueryType, float)"/> for 
    /// all contacts in <paramref name="newContacts"/>.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The <see cref="Contact"/>s in <paramref name="newContacts"/> 
    /// will be recycled and cannot be used after this method call.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void Merge(ContactSet target, ContactSet newContacts, CollisionQueryType type, float contactPositionTolerance)
    {
      Debug.Assert(target != null);
      Debug.Assert(newContacts != null);
      Debug.Assert(contactPositionTolerance >= 0, "The contact position tolerance must be greater than or equal to 0");
      
      int numberOfNewContacts = newContacts.Count;
      for (int i = 0; i < numberOfNewContacts; i++)
        Merge(target, newContacts[i], type, contactPositionTolerance);

      newContacts.Clear();
    }


    /// <summary>
    /// Tries to the merge the contact with the nearest contact in the given 
    /// <see cref="ContactSet"/>.
    /// </summary>
    /// <param name="contactSet">The contact set. (Must not be <see langword="null"/>.)</param>
    /// <param name="contact">The contact. (Must not be <see langword="null"/>.)</param>
    /// <param name="contactPositionTolerance">The contact position tolerance.</param>
    /// <param name="updateMerged">
    /// If set to <see langword="true"/> the merged contact is updated with the data of 
    /// <paramref name="contact"/>. If set to <see langword="false"/> the merged contact keeps the
    /// data of the old contact.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the contact was merged successfully; otherwise
    /// <see langword="false"/>.
    /// </returns>
    private static bool TryMergeWithNearestContact(ContactSet contactSet, Contact contact, float contactPositionTolerance, bool updateMerged)
    {
      Debug.Assert(contactPositionTolerance >= 0, "The contact position tolerance must be greater than or equal to 0");
      Debug.Assert(contactSet.Count > 0, "Cannot merge nearest contact to empty contact set.");

      // Find the cached contact that is closest.
      int nearestContactIndex = -1;

      // Near contact must be within contact position tolerance.
      float minDistance = contactPositionTolerance + Numeric.EpsilonF;
      float minDistanceSquared = minDistance * minDistance;

      int numberOfContacts = contactSet.Count;
      for (int i = 0; i < numberOfContacts; i++)
      {
        Contact otherContact = contactSet[i];

        // Do not merge contacts with different features because user should be notified by 
        // a new contact that a new feature is touched.
        if (contact.FeatureA == otherContact.FeatureA && contact.FeatureB == otherContact.FeatureB)
        {
          // Check position difference.
          float distanceSquared = (otherContact.Position - contact.Position).LengthSquared;
          if (distanceSquared < minDistanceSquared)
          {
            minDistanceSquared = distanceSquared;
            nearestContactIndex = i;
          }
        }
      }

      if (nearestContactIndex >= 0)
      {
        if (updateMerged)
        {
          // Merge with an existing contact.
          // We take the geometry of the new contact and keep the other data of the old contact.
          // We also keep the old contact, so that references to this contact stay valid.
          Contact nearestContact = contactSet[nearestContactIndex];
          nearestContact.IsRayHit = contact.IsRayHit;
          nearestContact.PositionALocal = contact.PositionALocal;
          nearestContact.PositionBLocal = contact.PositionBLocal;
          nearestContact.Normal = contact.Normal;
          nearestContact.PenetrationDepth = contact.PenetrationDepth;
          nearestContact.Position = contact.Position;
        }

        return true;
      }

      return false;
    }


    /// <summary>
    /// Reduces the number of contacts in the contact set to 1 contact.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <remarks>
    /// The contact with the biggest penetration depth is kept.
    /// </remarks>
    internal static void ReduceClosestPoints(ContactSet contactSet)
    {
      // Reduce to 1 contact.
      int numberOfContacts = contactSet.Count;
      if (numberOfContacts > 1)
      {
        // Keep the contact with the deepest penetration depth.
        Contact bestContact = contactSet[0];
        for (int i = 1; i < numberOfContacts; i++)
        {
          if (contactSet[i].PenetrationDepth > bestContact.PenetrationDepth)
          {
            bestContact = contactSet[i];
          }
        }

        // Throw away other contacts.
        foreach (var contact in contactSet)
          if (contact != bestContact)
            contact.Recycle();

        contactSet.Clear();
        contactSet.Add(bestContact);
      }

      Debug.Assert(contactSet.Count == 0 || contactSet.Count == 1);

      // If we HaveContact but the contact shows a separation, delete contact.
      // This can happen for TriangleMesh vs. TriangleMesh because the triangle mesh algorithm
      // filters contacts if they have a bad normal. It can happen that all contacts are filtered
      // and only a separated contact remains in the contact set.
      if (contactSet.HaveContact && contactSet.Count > 0 && contactSet[0].PenetrationDepth < 0)
      {
        contactSet[0].Recycle();
        contactSet.Clear();
      }
    }


    /// <summary>
    /// Reduces the number of contacts in the contact set to 1 contact.
    /// </summary>
    /// <param name="contactSet">
    /// The contact set. One shape in the contact set must be a <see cref="RayShape"/>!
    /// </param>
    /// <remarks>
    /// The best ray hit is kept. 
    /// </remarks>
    internal static void ReduceRayHits(ContactSet contactSet)
    {
      Debug.Assert(
        contactSet.ObjectA.GeometricObject.Shape is RayShape
        || contactSet.ObjectB.GeometricObject.Shape is RayShape, 
        "ReduceRayHits was called for a contact set without a ray.");

      // For separated contacts keep the contact with the smallest separation.
      // If we have contact, keep the contact with the SMALLEST penetration (= shortest ray length)
      // and remove all invalid contacts (with separation).
      bool haveContact = contactSet.HaveContact;
      float bestPenetrationDepth = haveContact ? float.PositiveInfinity : float.NegativeInfinity;
      Contact bestContact = null;
      int numberOfContacts = contactSet.Count;
      for (int i = 0; i < numberOfContacts; i++)
      {
        Contact contact = contactSet[i];
        float penetrationDepth = contact.PenetrationDepth;

        if (haveContact)
        {
          // Search for positive and smallest penetration depth.
          if (penetrationDepth >= 0 && penetrationDepth < bestPenetrationDepth)
          {
            bestContact = contact;
            bestPenetrationDepth = penetrationDepth;
          }
        }
        else
        {
          // Search for negative and largest penetration depth (Separation!)
          Debug.Assert(penetrationDepth < 0, "HaveContact is false, but contact shows penetration.");
          if (penetrationDepth > bestPenetrationDepth)
          {
            bestContact = contact;
            bestPenetrationDepth = penetrationDepth;
          }
        }
      }

      // Keep best contact.
      // Note: In some situations HaveContact is true, but the contact set does not contains any
      // contacts with penetration. This happen, for example, when testing a ray inside a triangle
      // mesh. The TriangleMeshAlgorithm automatically filters contacts with bad normals.
      // When HaveContact is false, we should always have a contact (closest point).

      // Throw away other contacts.
      foreach (var contact in contactSet)
        if (contact != bestContact)
          contact.Recycle();

      contactSet.Clear();
      if (bestContact != null)
        contactSet.Add(bestContact);
    }


    /// <summary>
    /// Removes contacts where the contact normal points into an invalid direction.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <param name="normal">The desired normal vector.</param>
    /// <param name="minDotProduct">
    /// The minimal allowed dot product of the contact normal and <paramref name="normal"/>.
    /// </param>
    /// <remarks>
    /// For each contact this method computes the dot product of the contact normal and the given 
    /// normal. If the dot product is less than <paramref name="minDotProduct"/> the contact is 
    /// removed.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void RemoveBadContacts(ContactSet contactSet, Vector3F normal, float minDotProduct)
    {
      for (int i = contactSet.Count - 1; i >= 0; i--)
      {
        Contact contact = contactSet[i];
        Vector3F contactNormal = contact.Normal;

        // float dot = Vector3F.Dot(contactNormal, normal);
        
        // ----- Optimized version:
        float dot = contactNormal.X * normal.X + contactNormal.Y * normal.Y + contactNormal.Z * normal.Z;

        if (dot < minDotProduct)
        {
          contactSet.RemoveAt(i);
          contact.Recycle();
        }
      }
    }


    /// <summary>
    /// Removes separated contacts.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    internal static void RemoveSeparatedContacts(ContactSet contactSet)
    {
      for (int i = contactSet.Count - 1; i >= 0; i--)
      {
        Contact contact = contactSet[i];
        if (contact.PenetrationDepth < 0)
        {
          contactSet.RemoveAt(i);
          contact.Recycle();
        }
      }
    }


    /// <summary>
    /// Updates the contact geometry of a contact set.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <param name="deltaTime">
    /// The time step in seconds. (The simulation time that has elapsed since the last time that an 
    /// Update-method was called.)
    /// </param>
    /// <param name="contactPositionTolerance">The contact position tolerance.</param>
    /// <remarks>
    /// <para>
    /// The objects can still move relative to each other. This method updates the contact 
    /// information if the objects have moved. The <see cref="Contact.Lifetime"/> of persistent 
    /// contacts is increased. Invalid contacts are removed. Closest point pairs will be removed if 
    /// they start touching. Penetrating or touching contacts are removed if the objects have moved 
    /// more than <paramref name="contactPositionTolerance"/> or the contacts have separated.
    /// </para>
    /// <para>
    /// Note: Only the info of the cached contacts is updated. New contacts are not discovered in
    /// this method.
    /// </para>
    /// </remarks>
    internal static void UpdateContacts(ContactSet contactSet, float deltaTime, float contactPositionTolerance)
    {
      Debug.Assert(contactPositionTolerance >= 0, "The contact position tolerance must be greater than or equal to 0");

      int numberOfContacts = contactSet.Count;
      if (numberOfContacts == 0)
        return;

      if (contactSet.ObjectA.Changed || contactSet.ObjectB.Changed)
      {
        // Objects have moved.
        for (int i = numberOfContacts - 1; i >= 0; i--)   // Reverse loop, because contacts might be removed.
        {
          Contact contact = contactSet[i];

          // Increase Lifetime. 
          contact.Lifetime += deltaTime;

          // Update all contacts and remove invalid contacts.      
          bool shouldRemove = UpdateContact(contactSet, contact, contactPositionTolerance);
          if (shouldRemove)
          {
            contactSet.RemoveAt(i);
            contact.Recycle();
          }
        }
      }
      else
      {
        // Nothing has moved. 
        for (int i = 0; i < numberOfContacts; i++)
        {
          // Increase Lifetime. 
          contactSet[i].Lifetime += deltaTime;
        }
      }
    }


    /// <summary>
    /// Updates the contact geometry for a single contact.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <param name="contact">The contact to be updated.</param>
    /// <param name="contactPositionTolerance">The contact position tolerance.</param>
    /// <returns>
    /// <see langword="true"/> if the contact is invalid and should be removed.
    /// </returns>
    private static bool UpdateContact(ContactSet contactSet, Contact contact, float contactPositionTolerance)
    {
      Pose poseA = contactSet.ObjectA.GeometricObject.Pose;
      Pose poseB = contactSet.ObjectB.GeometricObject.Pose;

      // Get local positions in world space.
      //Vector3F positionA = poseA.ToWorldPosition(contact.PositionALocal);
      //Vector3F positionB = poseB.ToWorldPosition(contact.PositionBLocal);

      // ----- Optimized version:
      Vector3F positionALocal = contact.PositionALocal;
      Vector3F positionA;
      positionA.X = poseA.Orientation.M00 * positionALocal.X + poseA.Orientation.M01 * positionALocal.Y + poseA.Orientation.M02 * positionALocal.Z + poseA.Position.X;
      positionA.Y = poseA.Orientation.M10 * positionALocal.X + poseA.Orientation.M11 * positionALocal.Y + poseA.Orientation.M12 * positionALocal.Z + poseA.Position.Y;
      positionA.Z = poseA.Orientation.M20 * positionALocal.X + poseA.Orientation.M21 * positionALocal.Y + poseA.Orientation.M22 * positionALocal.Z + poseA.Position.Z;
      Vector3F positionBLocal = contact.PositionBLocal;
      Vector3F positionB;
      positionB.X = poseB.Orientation.M00 * positionBLocal.X + poseB.Orientation.M01 * positionBLocal.Y + poseB.Orientation.M02 * positionBLocal.Z + poseB.Position.X;
      positionB.Y = poseB.Orientation.M10 * positionBLocal.X + poseB.Orientation.M11 * positionBLocal.Y + poseB.Orientation.M12 * positionBLocal.Z + poseB.Position.Y;
      positionB.Z = poseB.Orientation.M20 * positionBLocal.X + poseB.Orientation.M21 * positionBLocal.Y + poseB.Orientation.M22 * positionBLocal.Z + poseB.Position.Z;

      // Update Position.
      contact.Position = (positionA + positionB) / 2;

      // Update contacts and closest points differently:
      if (contact.PenetrationDepth >= 0)
      {
        // ----- Contact.
        Vector3F bToA = positionA - positionB;  // Vector from contact on A to contact on B
        if (!contact.IsRayHit)
        {
          // ----- Normal contact.
          // Update penetration depth: Difference of world position projected onto normal.
          //contact.PenetrationDepth = Vector3F.Dot(bToA, contact.Normal);

          // ----- Optimized version:
          Vector3F contactNormal = contact.Normal;
          contact.PenetrationDepth = bToA.X * contactNormal.X + bToA.Y * contactNormal.Y + bToA.Z * contactNormal.Z;
        }
        else
        {
          // ----- Ray hit.
          // Update penetration depth: Contact position to ray origin projected onto ray direction.

          // Get ray. Only one shape is a ray because ray vs. ray do normally not collide.
          RayShape ray = contactSet.ObjectA.GeometricObject.Shape as RayShape;
          float rayScale = contactSet.ObjectA.GeometricObject.Scale.X;   // Non-uniformly scaled rays are not support, so we only need Scale.X!
          Vector3F hitPositionLocal;  // Hit position in local space of ray.
          if (ray != null)
          {
            hitPositionLocal = poseA.ToLocalPosition(contact.Position);
          }
          else
          {
            // The other object must be the ray.
            ray = contactSet.ObjectB.GeometricObject.Shape as RayShape;
            rayScale = contactSet.ObjectB.GeometricObject.Scale.X;   // Non-uniformly scaled rays are not support, so we only need Scale.X!
            hitPositionLocal = poseB.ToLocalPosition(contact.Position);
          }

          // Now, we have found the ray, unless there is a composite shape with a child ray - which
          // is not supported.
          if (ray != null)
          {
            contact.PenetrationDepth = Vector3F.Dot(hitPositionLocal - ray.Origin * rayScale, ray.Direction);

            // If the new penetration depth is negative or greater than the ray length, 
            // the objects have separated along the ray direction.
            if (contact.PenetrationDepth < 0 || contact.PenetrationDepth > ray.Length * rayScale)
              return true;
          }
        }

        // Remove points with negative penetration depth.
        if (contact.PenetrationDepth < 0)
          return true;

        // Check drift. 
        float driftSquared;
        if (contact.IsRayHit)
        {
          // For ray casts: Remove contact if movement in any direction is too large.
          driftSquared = bToA.LengthSquared;
        }
        else
        {
          // For contacts: Remove contacts if horizontal movement (perpendicular to contact normal) 
          // is too large.
          driftSquared = (bToA - contact.Normal * contact.PenetrationDepth).LengthSquared;
        }

        // Remove contact if drift is too large.
        return driftSquared > contactPositionTolerance * contactPositionTolerance;
      }
      else
      {
        // ----- Closest point pair.

        // Update distance. Since we do not check the geometric objects, the new distance
        // could be a separation or a penetration. We assume it is a separation and
        // use a "-" sign.
        // We have no problem if we are wrong and this is actually a penetration because this 
        // contact is automatically updated or removed when new contacts are computed in
        // the narrow phase.
        Vector3F aToB = positionB - positionA;  // Vector from contact on A to contact on B
        contact.PenetrationDepth = -aToB.Length;

        // If points moved into contact, remove this pair, because we don't have a valid
        // contact normal.
        if (Numeric.IsZero(contact.PenetrationDepth))
          return true;

        // Update normal.
        contact.Normal = aToB.Normalized;

        return false;
      }
    }


    /// <summary>
    /// Creates a <see cref="ContactSetCollection"/> from an <see cref="IEnumerable{ContactSet}"/>.
    /// </summary>
    /// <param name="contactSets">The contact sets.</param>
    /// <returns>
    /// A <see cref="ContactSetCollection"/> that contains the elements from the input sequence.
    /// </returns>
    public static ContactSetCollection ToContactSetCollection(this IEnumerable<ContactSet> contactSets)
    {
      ContactSetCollection collection = contactSets as ContactSetCollection;
      if (collection != null)
        return collection;

      return new ContactSetCollection(contactSets);
    }


    /// <summary>
    /// Performs more collision tests while slightly rotating one collision object.
    /// </summary>
    /// <param name="collisionDetection">The collision detection.</param>
    /// <param name="contactSet">
    /// The contact set; must contain at least 1 <see cref="Contact"/>.
    /// </param>
    /// <param name="perturbB">
    /// if set to <see langword="true"/> collision object B will be rotated; otherwise collision 
    /// object A will be rotated.
    /// </param>
    /// <param name="testMethod">The test method that is called to compute contacts.</param>
    /// <remarks>
    /// This method rotates one object 3 times and calls contact computation for the new
    /// orientations. It is recommended to call this method only when the contact set has 1 new
    /// contact.
    /// </remarks>
    internal static void TestWithPerturbations(CollisionDetection collisionDetection, ContactSet contactSet, bool perturbB, Action<ContactSet> testMethod)
    {
      Debug.Assert(contactSet != null);
      Debug.Assert(contactSet.Count > 0 && contactSet.HaveContact || !contactSet.IsPerturbationTestAllowed);
      Debug.Assert(testMethod != null);

      // Make this test only if there is 1 contact.
      // If there are 0 contacts, we assume that the contact pair is separated.
      // If there are more than 3 contacts, then we already have a lot of contacts to work with, no
      // need to search for more.
      if (!contactSet.HaveContact || contactSet.Count == 0 || contactSet.Count >= 4 || !contactSet.IsPerturbationTestAllowed)
        return;

      // Get data of object that will be rotated.
      var collisionObject = (perturbB) ? contactSet.ObjectB : contactSet.ObjectA;
      var geometricObject = collisionObject.GeometricObject;
      var pose = geometricObject.Pose;

      // Get normal, pointing to the test object.
      var normal = contactSet[0].Normal;
      if (!perturbB)
        normal = -normal;

      var contactPosition = contactSet[0].Position;
      var centerToContact = contactPosition - pose.Position;

      // Compute a perturbation angle proportional to the dimension of the object.
      var radius = geometricObject.Aabb.Extent.Length;
      var angle = collisionDetection.ContactPositionTolerance / radius;

      // axis1 is in the contact tangent plane, orthogonal to normal.
      var axis1 = Vector3F.Cross(normal, centerToContact);

      // If axis1 is zero then normal and centerToContact are collinear. This happens 
      // for example for spheres or cone tips against flat faces. In these cases we assume
      // that there will be max. 1 contact.
      if (axis1.IsNumericallyZero)
        return;

      var axis1Local = pose.ToLocalDirection(axis1);
      var rotation = Matrix33F.CreateRotation(axis1Local, -angle);

      // Use temporary test objects.
      var testGeometricObject = TestGeometricObject.Create();
      testGeometricObject.Shape = geometricObject.Shape;
      testGeometricObject.Scale = geometricObject.Scale;
      testGeometricObject.Pose = new Pose(pose.Position, pose.Orientation * rotation);

      var testCollisionObject = ResourcePools.TestCollisionObjects.Obtain();
      testCollisionObject.SetInternal(collisionObject, testGeometricObject);

      var testContactSet = perturbB ? ContactSet.Create(contactSet.ObjectA, testCollisionObject) 
                                    : ContactSet.Create(testCollisionObject, contactSet.ObjectB);
      testContactSet.IsPerturbationTestAllowed = false;             // Avoid recursive perturbation tests!
      testContactSet.PreferredNormal = contactSet.PreferredNormal;

      // Compute next contacts.
      testMethod(testContactSet);

      if (testContactSet.Count > 0)
      {
        // axis2 is in the contact tangent plane, orthogonal to axis1.
        var axis2 = Vector3F.Cross(axis1, normal);
        var axis2Local = pose.ToLocalDirection(axis2);

        var rotation2 = Matrix33F.CreateRotation(axis2Local, -angle);
        testGeometricObject.Pose = new Pose(pose.Position, pose.Orientation * rotation2);

        // Compute next contacts.
        testMethod(testContactSet);

        // Invert rotation2.
        rotation2.Transpose();
        testGeometricObject.Pose = new Pose(pose.Position, pose.Orientation * rotation2);

        // Compute next contacts.
        testMethod(testContactSet);
      }

      // Set HaveContact. It is reset when a perturbation separates the objects.
      testContactSet.HaveContact = true;
      
      // TODO: Test if we need this:
      // The contact world positions are not really correct because one object was rotated.
      // UpdateContacts recomputes the world positions from the local positions.
      UpdateContacts(testContactSet, 0, collisionDetection.ContactPositionTolerance);

      // Merge contacts of testContactSet into contact set, but do not change existing contacts.
      foreach (var contact in testContactSet)
      {
        // We call TryMerge() to see if the contact is similar to an existing contact.
        bool exists = TryMergeWithNearestContact(
          contactSet, 
          contact, 
          collisionDetection.ContactPositionTolerance,
          false);   // The existing contact must no be changed!

        if (exists)
        {
          // We can throw away the new contact because a similar is already in the contact set.
          contact.Recycle();
        }
        else
        {
          // Add new contact.
          contactSet.Add(contact);
        }
      }

      // Recycle temporary objects.
      testContactSet.Recycle();
      ResourcePools.TestCollisionObjects.Recycle(testCollisionObject);
      testGeometricObject.Recycle();
    }
  }
}
