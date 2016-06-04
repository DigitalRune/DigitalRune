using System;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class CollisionAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException()
    {
      new SphereSphereAlgorithm(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetClosestPointsException0()
    {
      new NoCollisionAlgorithm(new CollisionDetection()).GetClosestPoints(null, new CollisionObject());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetClosestPointsException1()
    {
      new NoCollisionAlgorithm(new CollisionDetection()).GetClosestPoints(new CollisionObject(), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetContactsException0()
    {
      new NoCollisionAlgorithm(new CollisionDetection()).GetContacts(null, new CollisionObject());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetContactsException1()
    {
      new NoCollisionAlgorithm(new CollisionDetection()).GetContacts(new CollisionObject(), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void HaveContactException0()
    {
      new NoCollisionAlgorithm(new CollisionDetection()).HaveContact(new CollisionObject(), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void HaveContactException1()
    {
      new NoCollisionAlgorithm(new CollisionDetection()).HaveContact(null, new CollisionObject());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void UpdateContactsException()
    {
      new NoCollisionAlgorithm(new CollisionDetection()).UpdateContacts(null, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void UpdateClosestPointsException()
    {
      new NoCollisionAlgorithm(new CollisionDetection()).UpdateClosestPoints(null, 0);
    }
  }
}
