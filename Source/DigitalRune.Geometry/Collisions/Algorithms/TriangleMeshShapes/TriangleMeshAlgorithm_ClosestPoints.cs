// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  public partial class TriangleMeshAlgorithm
  {
    // Provides callbacks for ISupportClosestPointQueries.
    private sealed class ClosestPointCallback
    {
      public readonly Func<int, float> HandleItem;
      public readonly Func<int, int, float> HandlePair;

      public TriangleMeshAlgorithm CollisionAlgorithm;
      public bool Swapped;
      public ContactSet ContactSet;


      #region ----- Test Objects -----
    
      // Test objects which are re-used to avoid unnecessary memory allocations.
      public ContactSet TestContactSet;
      public CollisionObject TestCollisionObjectA;
      public TestGeometricObject TestGeometricObjectA;
      public TriangleShape TestTriangleA;
      
      // Only needed for Partition vs. Partition.
      public CollisionObject TestCollisionObjectB;
      public TestGeometricObject TestGeometricObjectB;
      public TriangleShape TestTriangleB;
      #endregion


      public ClosestPointCallback()
      {
        // Cache delegates.
        HandleItem = HandleClosestPoint;
        HandlePair = HandleClosestPoint;
      }


      private float HandleClosestPoint(int triangleIndex)
      {
        CollisionAlgorithm.AddTriangleContacts(
          ContactSet, 
          Swapped, 
          triangleIndex,
          CollisionQueryType.ClosestPoints,
          TestContactSet,
          TestCollisionObjectA,
          TestGeometricObjectA,
          TestTriangleA);

        return GetClosestPointDistanceSquared();
      }


      private float HandleClosestPoint(int triangleIndexA, int triangleIndexB)
      {
        CollisionAlgorithm.AddTriangleTriangleContacts(
          ContactSet, 
          triangleIndexA,
          triangleIndexB, 
          CollisionQueryType.ClosestPoints,
          TestContactSet,
          TestCollisionObjectA,
          TestGeometricObjectA,
          TestTriangleA,
          TestCollisionObjectB,
          TestGeometricObjectB, 
          TestTriangleB);

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
