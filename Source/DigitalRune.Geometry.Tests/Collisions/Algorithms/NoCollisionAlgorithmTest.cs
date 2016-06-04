using DigitalRune.Geometry.Shapes;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class NoCollisionAlgorithmTest
  {
    [Test]
    public void TestMethods()
    {
      NoCollisionAlgorithm algo = new NoCollisionAlgorithm(new CollisionDetection());

      CollisionObject a = new CollisionObject { GeometricObject = new GeometricObject { Shape = new SphereShape(1) } };
      CollisionObject b = new CollisionObject { GeometricObject = new GeometricObject { Shape = new SphereShape(2) } };

      Assert.AreEqual(a, algo.GetClosestPoints(a, b).ObjectA);
      Assert.AreEqual(b, algo.GetClosestPoints(a, b).ObjectB);
      Assert.AreEqual(0, algo.GetClosestPoints(a, b).Count);

      Assert.AreEqual(a, algo.GetContacts(a, b).ObjectA);
      Assert.AreEqual(b, algo.GetContacts(a, b).ObjectB);
      Assert.AreEqual(0, algo.GetContacts(a, b).Count);

      Assert.AreEqual(false, algo.HaveContact(a, b));

      ContactSet cs = ContactSet.Create(a, b);
      algo.UpdateClosestPoints(cs, 0);
      Assert.AreEqual(a, cs.ObjectA);
      Assert.AreEqual(b, cs.ObjectB);
      Assert.AreEqual(0, cs.Count);

      cs = ContactSet.Create(a, b);
      cs.Add(Contact.Create());
      algo.UpdateContacts(cs, 0);
      Assert.AreEqual(a, cs.ObjectA);
      Assert.AreEqual(b, cs.ObjectB);
      Assert.AreEqual(0, cs.Count);
    }    
  }
}
