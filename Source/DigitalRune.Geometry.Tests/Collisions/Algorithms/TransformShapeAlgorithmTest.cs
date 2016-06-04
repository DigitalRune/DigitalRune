using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class TransformShapeAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestException()
    {
      new TransformedShapeAlgorithm(new CollisionDetection()).GetClosestPoints(new CollisionObject(), new CollisionObject());
    }


    [Test]
    public void ComputeCollision()
    {
      CollisionObject plane = new CollisionObject
      {
        GeometricObject = new GeometricObject
         {
           Pose = Pose.Identity,
           Shape = new PlaneShape(new Vector3F(0, 1, 0), 0),
         },
      };
      CollisionObject transformShape = new CollisionObject
      {
        GeometricObject = new GeometricObject
        {
          Pose = Pose.Identity,
          Shape = new TransformedShape
          {
            Child = new GeometricObject
             {
                Pose = new Pose(new Vector3F(1, 2, 0)),
                Shape = new SphereShape(1),
             },
          },
        },
      };

      TransformedShapeAlgorithm algo = new TransformedShapeAlgorithm(new CollisionDetection());

      ContactSet set = algo.GetClosestPoints(plane, transformShape);
      if (set.ObjectA != plane)
        set = set.Swapped;
      Assert.AreEqual(1, set.Count);
      Assert.AreEqual(new Vector3F(1, 0, 0), set[0].PositionAWorld);
      Assert.AreEqual(new Vector3F(1, 1, 0), set[0].PositionBWorld);

      Assert.AreEqual(false, algo.HaveContact(plane, transformShape));

      ((GeometricObject)transformShape.GeometricObject).Pose = new Pose(new Vector3F(-1, -5, 4));
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(1, set.Count);
      Assert.AreEqual(new Vector3F(0, 0, 4), set[0].PositionAWorld);
      Assert.AreEqual(new Vector3F(0, -4, 4), set[0].PositionBWorld);

    }
  }
}
