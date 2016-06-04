// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// A dummy collision algorithm which always reports NO collision and NO closest points.
  /// </summary>
  public class NoCollisionAlgorithm : CollisionAlgorithm
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="NoCollisionAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public NoCollisionAlgorithm(CollisionDetection collisionDetection) 
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      contactSet.HaveContact = false;
    }


    /// <inheritdoc/>
    public override float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      return 1;
    }
  }
}
