using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class CombinedCollisionAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException()
    {
      CollisionDetection cd = new CollisionDetection();
      new CombinedCollisionAlgorithm(cd, null, new BoxBoxAlgorithm(cd));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException2()
    {
      CollisionDetection cd = new CollisionDetection();
      new CombinedCollisionAlgorithm(cd, new BoxBoxAlgorithm(cd), null);
    }


    [Test]
    public void ComputeCollision()
    {
      CollisionDetection cd = new CollisionDetection();

      CombinedCollisionAlgorithm cca = new CombinedCollisionAlgorithm(cd, new SphereSphereAlgorithm(cd), new SphereSphereAlgorithm(cd));

      CollisionObject a = new CollisionObject(new GeometricObject
            {
              Shape = new SphereShape(1),
            });

      CollisionObject b = new CollisionObject(new GeometricObject
            {
              Shape = new SphereShape(1),
              Pose = new Pose(new Vector3F(3, 0, 0)),
            });

      ContactSet set = cca.GetClosestPoints(a, b);
      Assert.AreEqual(1, set.Count);

      set = cca.GetContacts(a, b);
      Assert.IsTrue(set == null || set.Count == 0);

      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(2, 0, 0));

      set = cca.GetClosestPoints(a, b);
      Assert.AreEqual(1, set.Count);

      set = cca.GetContacts(a, b);
      Assert.AreEqual(1, set.Count);
    }


    [Test]
    public void ComputeCollision2()
    {
      // Use TestAlgo to test if GetClosestPoint returns only 1 contact. 
      CollisionDetection cd = new CollisionDetection();
      CombinedCollisionAlgorithm cca = new CombinedCollisionAlgorithm(cd, new TestAlgo(cd), new TestAlgo(cd));

      CollisionObject a = new CollisionObject();
      CollisionObject b = new CollisionObject();

      ContactSet set = cca.GetClosestPoints(a, b);
      // Test algo returns 2 contacts. One is filtered out because a closest-point query should return only 1 contact.
      Assert.AreEqual(1, set.Count);
      Assert.AreEqual(1.2f, set[0].PenetrationDepth);
    }


    [Test]
    public void TouchingButNotTouching()
    {
      // Special case: GJK reports contact, MPR cannot find the contact.
      // This happens for perfectly touching quadrics.

      CollisionDetection cd = new CollisionDetection();
      CombinedCollisionAlgorithm cca = new CombinedCollisionAlgorithm(cd, new Gjk(cd), new MinkowskiPortalRefinement(cd));

      CollisionObject a = new CollisionObject();
      ((GeometricObject)a.GeometricObject).Shape = new ConeShape(2, 2);

      CollisionObject b = new CollisionObject();
      ((GeometricObject)b.GeometricObject).Shape = new CircleShape(2);
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(4, 0, 0));

      Assert.AreEqual(false, cca.HaveContact(a, b));
      Assert.AreEqual(0, cca.GetContacts(a, b).Count);
      Assert.IsTrue(cca.GetClosestPoints(a, b)[0].PenetrationDepth < 0);
    }


    private class TestAlgo : CollisionAlgorithm
    {
      public TestAlgo(CollisionDetection cd)
        : base(cd)
      {
      }

      public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
      {
        contactSet.Clear();
        contactSet.Add(ContactHelper.CreateContact(contactSet, new Vector3F(1, 2, 3), Vector3F.UnitZ, 1, false));

        if (type == CollisionQueryType.Contacts)
          contactSet.Add(ContactHelper.CreateContact(contactSet, new Vector3F(2, 2, 3), Vector3F.UnitZ, 1.2f, false));

        contactSet.HaveContact = true;
      }
    }
  }
}
