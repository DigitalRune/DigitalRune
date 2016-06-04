using System;
using DigitalRune.Geometry.Collisions.Algorithms;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Tests
{
  [TestFixture]
  public class CollisionDetectionTest
  {
    CollisionDetection _collisionDetection;
    CollisionObject _objectA;
    CollisionObject _objectB;
    CollisionObject _objectC;

    [SetUp]
    public void SetUp()
    {
      _collisionDetection = new CollisionDetection();

      // Create objects. Object A and B touch.
      _objectA = new CollisionObject(new GeometricObject
      {
        Shape = new SphereShape(1),
        Pose = Pose.Identity,
      });
      _objectB = new CollisionObject(new GeometricObject
      {
        Shape = new SphereShape(1),
        Pose = new Pose(new Vector3F(2, 0, 0), QuaternionF.Identity),
      });
      _objectC = new CollisionObject(new GeometricObject
      {
        Shape = new SphereShape(1),
        Pose = new Pose(new Vector3F(10, 0, 0), QuaternionF.Identity),
      });
    }


    [Test]
    public void ContactPositionTolerance()
    {
      CollisionDetection cd = new CollisionDetection();
      cd.ContactPositionTolerance = 1f;
      Assert.AreEqual(1f, cd.ContactPositionTolerance);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ContactPositionToleranceException()
    {
      new CollisionDetection().ContactPositionTolerance = -0.1f;
    }


    [Test]
    public void Epsilon()
    {
      CollisionDetection cd = new CollisionDetection();
      cd.Epsilon = 1f;
      Assert.AreEqual(1f, cd.Epsilon);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void EpsilonException()
    {
      new CollisionDetection().Epsilon = -0.1f;
    }


    [Test]
    public void HaveAabbContact()
    {
      Assert.IsTrue(_collisionDetection.HaveAabbContact(_objectA, _objectA));
      Assert.IsTrue(_collisionDetection.HaveAabbContact(_objectA, _objectB));
      Assert.IsFalse(_collisionDetection.HaveAabbContact(_objectA, _objectC));

      _collisionDetection.CollisionFilter = new CollisionFilter();
      ((CollisionFilter) _collisionDetection.CollisionFilter).Set(_objectA, _objectB, false);
      Assert.IsFalse(_collisionDetection.HaveAabbContact(_objectA, _objectB));

      ((CollisionFilter) _collisionDetection.CollisionFilter).Set(_objectA, _objectB, true);
      Assert.IsTrue(_collisionDetection.HaveAabbContact(_objectA, _objectB));

      _collisionDetection.CollisionFilter = null;
      Assert.IsTrue(_collisionDetection.HaveAabbContact(_objectA, _objectB));

      _objectA.Enabled = false;
      Assert.IsFalse(_collisionDetection.HaveAabbContact(_objectA, _objectB));

      _objectA.Enabled = true;
      _objectB.Enabled = false;
      Assert.IsFalse(_collisionDetection.HaveAabbContact(_objectA, _objectB));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void HaveAabbContactException()
    {
      Assert.IsFalse(_collisionDetection.HaveAabbContact(null, _objectA));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void HaveAabbContactException2()
    {
      Assert.IsFalse(_collisionDetection.HaveAabbContact(_objectA, null));
    }


    [Test]
    public void HaveContact()
    {
      Assert.IsTrue(_collisionDetection.HaveContact(_objectA, _objectB));
      Assert.IsFalse(_collisionDetection.HaveContact(_objectA, _objectC));
    }

    [Test]
    public void GetContacts()
    {
      Assert.AreEqual(1, _collisionDetection.GetContacts(_objectA, _objectB).Count);
      Assert.AreEqual(_objectA, _collisionDetection.GetContacts(_objectA, _objectB).ObjectA);
      Assert.AreEqual(_objectB, _collisionDetection.GetContacts(_objectA, _objectB).ObjectB);

      Assert.AreEqual(null, _collisionDetection.GetContacts(_objectA, _objectC));

      _collisionDetection.CollisionFilter = new CollisionFilter();
      ((CollisionFilter) _collisionDetection.CollisionFilter).Set(_objectA, _objectB, false);
      Assert.AreEqual(null, _collisionDetection.GetContacts(_objectA, _objectB));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetContactsException()
    {
      _collisionDetection.GetContacts(null, _objectB);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetContactsException2()
    {
      _collisionDetection.GetContacts(_objectA, null);
    }


    [Test]
    public void GetClosestPoints()
    {
      Assert.AreEqual(1, _collisionDetection.GetClosestPoints(_objectA, _objectB).Count);
      Assert.AreEqual(_objectA, _collisionDetection.GetClosestPoints(_objectA, _objectB).ObjectA);
      Assert.AreEqual(_objectB, _collisionDetection.GetClosestPoints(_objectA, _objectB).ObjectB);

      Assert.AreEqual(1, _collisionDetection.GetClosestPoints(_objectA, _objectC).Count);
      Assert.AreEqual(_objectA, _collisionDetection.GetClosestPoints(_objectA, _objectC).ObjectA);
      Assert.AreEqual(_objectC, _collisionDetection.GetClosestPoints(_objectA, _objectC).ObjectB);

      _collisionDetection.CollisionFilter = new CollisionFilter();
      ((CollisionFilter) _collisionDetection.CollisionFilter).Set(_objectA, _objectB, false);
      Assert.AreEqual(1, _collisionDetection.GetClosestPoints(_objectA, _objectB).Count);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetClosestPointsException()
    {
      _collisionDetection.GetClosestPoints(null, _objectB);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetClosestPointsException2()
    {
      _collisionDetection.GetClosestPoints(_objectA, null);
    }


    [Test]
    public void UpdateContacts()
    {
      ContactSet set = ContactSet.Create(_objectA, _objectB);
      _collisionDetection.UpdateContacts(set, 0);  
      Assert.AreEqual(1, set.Count);
      Assert.AreEqual(_objectA, set.ObjectA);
      Assert.AreEqual(_objectB, set.ObjectB);

      set = ContactSet.Create(_objectA, _objectC);
      set.Add(ContactHelper.CreateContact(set, new Vector3F(1, 0, 0), new Vector3F(1, 0, 0), 0, false));
      _collisionDetection.UpdateContacts(set, 0);
      Assert.AreEqual(0, set.Count);
      Assert.AreEqual(_objectA, set.ObjectA);
      Assert.AreEqual(_objectC, set.ObjectB);

      _collisionDetection.CollisionFilter = new CollisionFilter();
      ((CollisionFilter) _collisionDetection.CollisionFilter).Set(_objectA, _objectB, false);
      set = ContactSet.Create(_objectA, _objectB);
      _collisionDetection.UpdateContacts(set, 0);  
      Assert.AreEqual(0, set.Count);
      Assert.AreEqual(_objectA, set.ObjectA);
      Assert.AreEqual(_objectB, set.ObjectB);
    }


    [Test]
    public void UpdateClosestPoints()
    {
      ContactSet set = ContactSet.Create(_objectA, _objectB);
      _collisionDetection.UpdateClosestPoints(set, 0);
      Assert.AreEqual(1, set.Count);
      Assert.AreEqual(_objectA, set.ObjectA);
      Assert.AreEqual(_objectB, set.ObjectB);

      set = ContactSet.Create(_objectA, _objectC);
      set.Add(ContactHelper.CreateContact(set, new Vector3F(1, 0, 0), new Vector3F(1, 0, 0), -10, false));
      _collisionDetection.UpdateClosestPoints(set, 0);
      Assert.AreEqual(1, set.Count);
      Assert.AreEqual(_objectA, set.ObjectA);
      Assert.AreEqual(_objectC, set.ObjectB);

      _collisionDetection.CollisionFilter = new CollisionFilter();
      ((CollisionFilter) _collisionDetection.CollisionFilter).Set(_objectA, _objectB, false);
      set = ContactSet.Create(_objectA, _objectB);
      _collisionDetection.UpdateClosestPoints(set, 0);
      Assert.AreEqual(1, set.Count);
      Assert.AreEqual(_objectA, set.ObjectA);
      Assert.AreEqual(_objectB, set.ObjectB);
    }
  }
}
