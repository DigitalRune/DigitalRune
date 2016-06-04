// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// This collision algorithm combines 2 other algorithms: One is used for penetrating objects, the
  /// other is used for closest-point queries of separated objects.
  /// </summary>
  /// <remarks>
  /// Some algorithms, like the GJK, do not compute valid closest points for penetrating objects.
  /// Other algorithms, like the MPR, cannot compute closest points for separated objects. This 
  /// class can combine different algorithms like GJK+MPR to get an algorithm that can handle all
  /// cases.
  /// </remarks>
  public class CombinedCollisionAlgorithm : CollisionAlgorithm
  {
    private readonly CollisionAlgorithm _closestPointsAlgorithm;
    private readonly CollisionAlgorithm _contactAlgorithm;


    /// <summary>
    /// Initializes a new instance of the <see cref="CombinedCollisionAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    /// <param name="closestPointsAlgorithm">The closest points algorithm.</param>
    /// <param name="contactAlgorithm">The contact algorithm.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="closestPointsAlgorithm"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="contactAlgorithm"/> is <see langword="null"/>.
    /// </exception>
    public CombinedCollisionAlgorithm(CollisionDetection collisionDetection, CollisionAlgorithm closestPointsAlgorithm, CollisionAlgorithm contactAlgorithm)
      : base(collisionDetection)
    {
      if (closestPointsAlgorithm == null)
        throw new ArgumentNullException("closestPointsAlgorithm");
      if (contactAlgorithm == null)
        throw new ArgumentNullException("contactAlgorithm");

      _closestPointsAlgorithm = closestPointsAlgorithm;
      _contactAlgorithm = contactAlgorithm;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      if (type == CollisionQueryType.ClosestPoints)
      {
        // Closest point queries.
        _closestPointsAlgorithm.ComputeCollision(contactSet, type);
        if (contactSet.HaveContact)
        {
          // Penetration. 

          // Remember the closest point info in case we run into troubles.
          // Get the last contact. We assume that this is the newest. In most cases there will 
          // only be one contact in the contact set.
          Contact fallbackContact = (contactSet.Count > 0) 
                                    ? contactSet[contactSet.Count - 1]
                                    : null;

          // Call the contact query algorithm.
          _contactAlgorithm.ComputeCollision(contactSet, CollisionQueryType.Contacts);

          if (!contactSet.HaveContact)
          {
            // Problem!
            // The closest-point algorithm reported contact. The contact algorithm didn't find a contact.
            // This can happen, for example, because of numerical inaccuracies in GJK vs. MPR.
            // Keep the result of the closest-point computation, but decrease the penetration depth
            // to indicate separation.
            if (fallbackContact != null)
            {
              Debug.Assert(fallbackContact.PenetrationDepth == 0);
              fallbackContact.PenetrationDepth = -Math.Min(10 * Numeric.EpsilonF, CollisionDetection.Epsilon);

              foreach (var contact in contactSet)
                if (contact != fallbackContact)
                  contact.Recycle();

              contactSet.Clear();
              contactSet.Add(fallbackContact);
            }

            contactSet.HaveContact = false;
          }
        }
      }
      else
      {
        // Boolean or contact queries.
        _contactAlgorithm.ComputeCollision(contactSet, type);
      }
    }


    /// <inheritdoc/>
    public override float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      // Call GetTimeOfImpact of _contactAlgorithm. The _closestPointsAlgorithm is 
      // GJK most of the time and the _contactAlgorithm is more specialized in most 
      // cases. If this is not the case, we have to add a flag which the user can set.
      return _contactAlgorithm.GetTimeOfImpact(objectA, targetPoseA, objectB, targetPoseB, allowedPenetration);
    }
  }
}
