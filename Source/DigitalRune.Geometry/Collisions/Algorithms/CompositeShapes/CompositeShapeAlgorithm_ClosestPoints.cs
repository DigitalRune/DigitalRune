// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


using System;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  public partial class CompositeShapeAlgorithm
  {
    // Provides callbacks for ISupportClosestPointQueries.
    private sealed class ClosestPointCallback
    {
      public readonly Func<int, float> HandleItem;
      public readonly Func<int, int, float> HandlePair;

      public CompositeShapeAlgorithm CollisionAlgorithm;
      public bool Swapped;
      public ContactSet ContactSet;


      #region ----- Test Objects -----

      // Test objects which are re-used to avoid unnecessary memory allocations.
      public ContactSet TestContactSet;
      public CollisionObject TestCollisionObjectA;
      public TestGeometricObject TestGeometricObjectA;

      // Only needed for Partition vs. Partition.
      public CollisionObject TestCollisionObjectB;
      public TestGeometricObject TestGeometricObjectB;
      #endregion


      public ClosestPointCallback()
      {
        // Cache delegates.
        HandleItem = HandleClosestPoint;
        HandlePair = HandleClosestPoint;
      }


      private float HandleClosestPoint(int childIndex)
      {
        CollisionAlgorithm.AddChildContacts(
          ContactSet,
          Swapped,
          childIndex,
          CollisionQueryType.ClosestPoints,
          TestContactSet,
          TestCollisionObjectA,
          TestGeometricObjectA);

        return GetClosestPointDistanceSquared();
      }


      private float HandleClosestPoint(int childIndexA, int childIndexB)
      {
        // Compute contacts between other object and BVH leaf.
        CollisionAlgorithm.AddChildChildContacts(
          ContactSet,
          childIndexA,
          childIndexB,
          CollisionQueryType.ClosestPoints,
          TestContactSet,
          TestCollisionObjectA,
          TestGeometricObjectA,
          TestCollisionObjectB,
          TestGeometricObjectB);

        return GetClosestPointDistanceSquared();
      }


      private float GetClosestPointDistanceSquared()
      {
        if (ContactSet.HaveContact)
          return 0;
        
        if (ContactSet.Count > 0)
        {
          // Note: Contact set can contain several contacts. We assume that the new contact is the last 
          // in the collection.
          float newPenetrationDepth = ContactSet[ContactSet.Count - 1].PenetrationDepth;
          float childDistanceSquared = newPenetrationDepth * newPenetrationDepth;
          return childDistanceSquared;
        }

        // No info, return safe value.
        return float.PositiveInfinity;
      }
    }
  }
}
