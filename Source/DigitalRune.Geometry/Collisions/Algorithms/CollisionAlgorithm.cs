// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// A collision algorithm computes contact information or closest-point information for 
  /// collision objects with certain shapes.
  /// </summary>
  /// <remarks>
  /// No collision filtering is used. 
  /// </remarks>
  public abstract class CollisionAlgorithm
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the collision detection service.
    /// </summary>
    /// <value>The collision detection service.</value>
    public CollisionDetection CollisionDetection { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collisionDetection"/> is <see langword="null"/>.
    /// </exception>
    protected CollisionAlgorithm(CollisionDetection collisionDetection)
    {
      if (collisionDetection == null)
        throw new ArgumentNullException("collisionDetection");

      CollisionDetection = collisionDetection;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the closest points of two collision objects.
    /// </summary>
    /// <param name="objectA">The collision object A.</param>
    /// <param name="objectB">The collision object B.</param>
    /// <returns>The contact set with the closest points.</returns>
    /// <remarks>
    /// Note that it is possible that two collision objects have "no closest points", for example
    /// if one collision object has an <see cref="EmptyShape"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    public ContactSet GetClosestPoints(CollisionObject objectA, CollisionObject objectB)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      // Create new contact set.

      ContactSet contactSet = ContactSet.Create(objectA, objectB);

      // Compute new closest points.
      ComputeCollision(contactSet, CollisionQueryType.ClosestPoints);

      // Reduce to 1 contact.
      if (objectA.IsRay || objectB.IsRay)
        ContactHelper.ReduceRayHits(contactSet);
      else
        ContactHelper.ReduceClosestPoints(contactSet);

      CheckResult(contactSet, true);

      return contactSet;
    }


    /// <summary>
    /// Gets the contact information of two possibly touching collision objects.
    /// </summary>
    /// <param name="objectA">The collision object A.</param>
    /// <param name="objectB">The collision object B.</param>
    /// <returns>The contact set.</returns>
    /// <remarks>
    /// The returned <see cref="ContactSet"/> will be empty if the objects are not in contact and 
    /// <see cref="ContactSet"/>.<see cref="ContactSet.HaveContact"/> will be 
    /// <see langword="false"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    public ContactSet GetContacts(CollisionObject objectA, CollisionObject objectB)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      // We do not compute the detailed contact information for triggers.
      bool ignoreContactInfo = (objectA.Type == CollisionObjectType.Trigger 
                                || objectB.Type == CollisionObjectType.Trigger);

      // We have to make the full contact query if one object is a ray with StopsAtFirstHit because 
      // we need the PenetrationDepth for sorting the hits.
      if (objectA.IsRayThatStopsAtFirstHit || objectB.IsRayThatStopsAtFirstHit)
        ignoreContactInfo = false;

      // Create new contact set.
      ContactSet contactSet = ContactSet.Create(objectA, objectB);

      if (ignoreContactInfo)
      {
        // Make only boolean check.
        ComputeCollision(contactSet, CollisionQueryType.Boolean);
      }
      else
      {
        // Compute new contact info.
        ComputeCollision(contactSet, CollisionQueryType.Contacts);
      }

      // Reduce ray cast results to 1 contact.
      if (objectA.IsRay || objectB.IsRay)
        ContactHelper.ReduceRayHits(contactSet);

      // Call contact filter.
      if (CollisionDetection.ContactFilter != null)
        CollisionDetection.ContactFilter.Filter(contactSet);

      CheckResult(contactSet, false);

      return contactSet;
    }


    /// <summary>
    /// Gets the time of impact between two moving objects.
    /// </summary>
    /// <param name="objectA">The object A.</param>
    /// <param name="targetPoseA">The target pose of A.</param>
    /// <param name="objectB">The object B.</param>
    /// <param name="targetPoseB">The target pose of B.</param>
    /// <param name="allowedPenetration">
    /// The allowed penetration. A positive allowed penetration value makes sure that the objects 
    /// have a measurable contact at the time of impact.
    /// </param>
    /// <returns>The time of impact in the range [0, 1].</returns>
    /// <remarks>
    /// <para>
    /// Both objects are moved from their current pose (time = 0) to the given target pose (time =
    /// 1). If they collide during this movement the first time of impact is returned. A time of
    /// impact of 1 can mean that the objects do not collide or they collide at their target
    /// positions.
    /// </para>
    /// <para>
    /// The result is undefined if the objects are already in contact at their start poses.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The base implementation already computes the time of
    /// impact for convex shapes. For other shapes it returns 1. Optimized versions should be
    /// implemented in derived classes.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> or <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    public virtual float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      // Handle convex shapes here, because this is so common.
      if (objectA.GeometricObject.Shape is ConvexShape && objectB.GeometricObject.Shape is ConvexShape)
      {
        switch (CollisionDetection.ContinuousCollisionDetectionMode)
        {
          case ContinuousCollisionDetectionMode.Full:
            return CcdHelper.GetTimeOfImpactCA(objectA, targetPoseA, objectB, targetPoseB, allowedPenetration, CollisionDetection);
          default:
            return CcdHelper.GetTimeOfImpactLinearSweep(objectA, targetPoseA, objectB, targetPoseB, allowedPenetration);
        }
      }

      return 1;
    }


    /// <summary>
    /// Determines whether two collision objects are in contact.
    /// </summary>
    /// <param name="objectA">The collision object A.</param>
    /// <param name="objectB">The collision object B.</param>
    /// <returns>
    /// <see langword="true"/> if the objects are touching (or penetrating); otherwise 
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    public bool HaveContact(CollisionObject objectA, CollisionObject objectB)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      ContactSet contactSet = ContactSet.Create(objectA, objectB);
      ComputeCollision(contactSet, CollisionQueryType.Boolean);
      CheckResult(contactSet, false);

      bool haveContact = contactSet.HaveContact;
      contactSet.Recycle();
      return haveContact;
    }


    /// <summary>
    /// Performs a collision query to update the closest-point information in the contact set.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <param name="deltaTime">
    /// The time step size in seconds. (The elapsed simulation time since the contact set was
    /// updated the last time.)
    /// </param>
    /// <remarks>
    /// This method updates closest-point information stored in the given contact set. This method
    /// is usually faster than <see cref="GetClosestPoints"/> because the information in 
    /// <paramref name="contactSet"/> is reused and updated.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="contactSet"/> is <see langword="null"/>.
    /// </exception>
    public void UpdateClosestPoints(ContactSet contactSet, float deltaTime)
    {
      if (contactSet == null)
        throw new ArgumentNullException("contactSet");

      // Update cached information. The last closest-point pair is removed if 
      // a penetration is removed. 
      ContactHelper.UpdateContacts(contactSet, deltaTime, CollisionDetection.ContactPositionTolerance);

      ComputeCollision(contactSet, CollisionQueryType.ClosestPoints);

      // Reduce to 1 contact.
      if (contactSet.ObjectA.IsRay || contactSet.ObjectB.IsRay)
        ContactHelper.ReduceRayHits(contactSet);
      else
        ContactHelper.ReduceClosestPoints(contactSet);

      CheckResult(contactSet, true);
      contactSet.IsValid = true;
    }


    /// <summary>
    /// Performs a collision query to update the contact information in the contact set.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <param name="deltaTime">
    /// The time step size in seconds. (The elapsed simulation time since the contact set was
    /// updated the last time.)
    /// </param>
    /// <remarks>
    /// <para>
    /// This method updates contact information stored in the given contact set. This method is 
    /// usually faster than <see cref="GetContacts"/> because the information in 
    /// <paramref name="contactSet"/> is reused and updated.
    /// </para>
    /// <para>
    /// The life time counter of persistent contacts is increased.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="contactSet"/> is <see langword="null"/>.
    /// </exception>
    public void UpdateContacts(ContactSet contactSet, float deltaTime)
    {
      if (contactSet == null)
        throw new ArgumentNullException("contactSet");

      // Remove separated contacts - the contact set could contain separated
      // closest points of a GetClosestPoints query.
      ContactHelper.RemoveSeparatedContacts(contactSet);

      // Update cached information.
      ContactHelper.UpdateContacts(contactSet, deltaTime, CollisionDetection.ContactPositionTolerance);

      // We do not compute the detailed contact information for triggers.
      bool ignoreContactInfo = (contactSet.ObjectA.Type == CollisionObjectType.Trigger 
                                || contactSet.ObjectB.Type == CollisionObjectType.Trigger);

      // We have to make the full contact query if one object is a ray with StopsAtFirstHit because 
      // we need the PenetrationDepth for sorting the hits.
      if (contactSet.ObjectA.IsRayThatStopsAtFirstHit || contactSet.ObjectB.IsRayThatStopsAtFirstHit)
        ignoreContactInfo = false;

      if (ignoreContactInfo)
      {
        // Check cached flag. If necessary, make only boolean check.
        if (contactSet.HaveContact == false || contactSet.Count == 0)
          ComputeCollision(contactSet, CollisionQueryType.Boolean);
      }
      else
      {
        // Compute new contact info.
        ComputeCollision(contactSet, CollisionQueryType.Contacts);
      }
      
      if (contactSet.HaveContact == false)
      {
        // No contact: Remove old contacts if objects do not touch.
        foreach (var contact in contactSet)
          contact.Recycle();

        contactSet.Clear();
      }
      else
      {
        // Reduce ray cast results to 1 contact.
        if (contactSet.ObjectA.IsRay || contactSet.ObjectB.IsRay)
          ContactHelper.ReduceRayHits(contactSet);

        // We have contact: Call contact filter.
        if (CollisionDetection.ContactFilter != null)
          CollisionDetection.ContactFilter.Filter(contactSet);
      }

      CheckResult(contactSet, false);
      contactSet.IsValid = true;
    }


    /// <summary>
    /// Computes the collision. - This method should only be used by 
    /// <see cref="CollisionAlgorithm"/> instances!
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <param name="type">The type of collision query.</param>
    /// <remarks>
    /// <para>
    /// This method does the real work. It is called from the other public methods of a 
    /// <see cref="CollisionAlgorithm"/>. Also, if one <see cref="CollisionAlgorithm"/> uses another
    /// <see cref="CollisionAlgorithm"/> internally, this method should be called directly instead
    /// of <see cref="CollisionAlgorithm.UpdateClosestPoints"/> or 
    /// <see cref="CollisionAlgorithm.UpdateContacts"/>.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> This is the central method which has to be implemented
    /// in derived classes. <paramref name="contactSet"/> is never <see langword="null"/>. This
    /// method has to add new contact/closest-point info with 
    /// <see cref="ContactHelper.Merge(ContactSet,Contact,CollisionQueryType,float)"/>. It is not
    /// necessary to remove old contacts. At the beginning of the method 
    /// <see cref="ContactSet.HaveContact"/> in <paramref name="contactSet"/> indicates the result
    /// of the last narrow phase algorithm that was run on <paramref name="contactSet"/>. This
    /// method must set <see cref="ContactSet.HaveContact"/> to <see langword="false"/> if it 
    /// doesn't find a contact or to <see langword="true"/> if it finds a contact.
    /// </para>
    /// </remarks>
    public abstract void ComputeCollision(ContactSet contactSet, CollisionQueryType type);


    /// <summary>
    /// Checks whether a contact query result is valid.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <param name="isClosestPointResult">
    /// <see langword="true"/> if the query was a closest-point query; otherwise, 
    /// <see langword="false"/>.
    /// </param>
    /// <exception cref="GeometryException">The contact query result is invalid.</exception>
    [Conditional("DEBUG")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static void CheckResult(ContactSet contactSet, bool isClosestPointResult)
    {
      if (isClosestPointResult && contactSet.Count > 1)
        throw new GeometryException("Closest point query result contains more than one contact.");

      if (contactSet.HaveContact)
      {
        if (contactSet.Any(contact => contact.PenetrationDepth < 0))
          throw new GeometryException("HaveContact is true but the contactSet contains a separated contact.");

        if (contactSet.ObjectA.GeometricObject.Shape is RayShape || contactSet.ObjectB.GeometricObject.Shape is RayShape)
        {
          if (contactSet.Count > 1)
            throw new GeometryException("Ray casts must not contain more than 1 contact.");

          if (contactSet.Any(contact => !contact.IsRayHit))
            throw new GeometryException("IsRayHit is not set for a ray hit.");
        }
      }

      if (isClosestPointResult == false && contactSet.HaveContact == false && contactSet.Count > 0)
        throw new GeometryException("A contact query returned contact details for a separated pair.");

      if (contactSet.HaveContact == false && contactSet.Any(contact => contact.PenetrationDepth >= 0))
        throw new GeometryException("HaveContact is false but the contactSet contains a touching/penetrating contact.");
    }
    #endregion
  }
}
