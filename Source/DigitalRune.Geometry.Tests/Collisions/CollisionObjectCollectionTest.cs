using System;
using DigitalRune.Geometry.Shapes;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Tests
{
  [TestFixture]
  public class CollisionObjectCollectionTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetShouldThrowWhenNull()
    {
      new CollisionObjectCollection().Get(null);
    }


    [Test]
    public void LookupWithoutTable()
    {
      IGeometricObject geometricObjectA = new GeometricObject();
      IGeometricObject geometricObjectB = new GeometricObject();
      IGeometricObject geometricObjectC = new GeometricObject();
      CollisionObject collisionObjectA = new CollisionObject(geometricObjectA);
      CollisionObject collisionObjectB = new CollisionObject(geometricObjectB);
      CollisionObject collisionObjectC = new CollisionObject(geometricObjectC);

      var collisionObjectCollection = new CollisionObjectCollection();

      Assert.IsFalse(collisionObjectCollection.EnableLookupTable);
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectA));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectB));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectC));

      collisionObjectCollection.Add(collisionObjectA);
      collisionObjectCollection.Add(collisionObjectB);
      collisionObjectCollection.Add(collisionObjectC);

      Assert.AreEqual(collisionObjectA, collisionObjectCollection.Get(geometricObjectA));
      Assert.AreEqual(collisionObjectB, collisionObjectCollection.Get(geometricObjectB));
      Assert.AreEqual(collisionObjectC, collisionObjectCollection.Get(geometricObjectC));

      collisionObjectCollection.Remove(collisionObjectB);
      Assert.AreEqual(collisionObjectA, collisionObjectCollection.Get(geometricObjectA));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectB));
      Assert.AreEqual(collisionObjectC, collisionObjectCollection.Get(geometricObjectC));

      collisionObjectCollection.Clear();
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectA));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectB));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectC));

      collisionObjectCollection.Add(collisionObjectA);
      collisionObjectCollection[0] = collisionObjectB;
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectA));
      Assert.AreEqual(collisionObjectB, collisionObjectCollection.Get(geometricObjectB));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectC));
    }



    [Test]
    public void LookupWithTable()
    {
      IGeometricObject geometricObjectA = new GeometricObject();
      IGeometricObject geometricObjectB = new GeometricObject();
      IGeometricObject geometricObjectC = new GeometricObject();
      CollisionObject collisionObjectA = new CollisionObject(geometricObjectA);
      CollisionObject collisionObjectB = new CollisionObject(geometricObjectB);
      CollisionObject collisionObjectC = new CollisionObject(geometricObjectC);

      var collisionObjectCollection = new CollisionObjectCollection { EnableLookupTable = true };

      Assert.IsTrue(collisionObjectCollection.EnableLookupTable);
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectA));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectB));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectC));

      collisionObjectCollection.Add(collisionObjectA);
      collisionObjectCollection.Add(collisionObjectB);
      collisionObjectCollection.Add(collisionObjectC);

      Assert.AreEqual(collisionObjectA, collisionObjectCollection.Get(geometricObjectA));
      Assert.AreEqual(collisionObjectB, collisionObjectCollection.Get(geometricObjectB));
      Assert.AreEqual(collisionObjectC, collisionObjectCollection.Get(geometricObjectC));

      collisionObjectCollection.Remove(collisionObjectB);
      Assert.AreEqual(collisionObjectA, collisionObjectCollection.Get(geometricObjectA));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectB));
      Assert.AreEqual(collisionObjectC, collisionObjectCollection.Get(geometricObjectC));

      collisionObjectCollection.Clear();
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectA));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectB));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectC));

      collisionObjectCollection.Add(collisionObjectA);
      collisionObjectCollection[0] = collisionObjectB;
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectA));
      Assert.AreEqual(collisionObjectB, collisionObjectCollection.Get(geometricObjectB));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectC));
    }


    [Test]
    public void DelayedActivationOfLookupTable()
    {
      IGeometricObject geometricObjectA = new GeometricObject();
      IGeometricObject geometricObjectB = new GeometricObject();
      IGeometricObject geometricObjectC = new GeometricObject();
      CollisionObject collisionObjectA = new CollisionObject(geometricObjectA);
      CollisionObject collisionObjectB = new CollisionObject(geometricObjectB);
      CollisionObject collisionObjectC = new CollisionObject(geometricObjectC);

      var collisionObjectCollection = new CollisionObjectCollection();
      Assert.IsFalse(collisionObjectCollection.EnableLookupTable);

      collisionObjectCollection.EnableLookupTable = false; // Set to false again, just for code coverage.

      collisionObjectCollection.Add(collisionObjectA);

      collisionObjectCollection.EnableLookupTable = true;
      collisionObjectCollection.Add(collisionObjectB);
      collisionObjectCollection.Add(collisionObjectC);

      Assert.IsTrue(collisionObjectCollection.EnableLookupTable);
      Assert.AreEqual(collisionObjectA, collisionObjectCollection.Get(geometricObjectA));
      Assert.AreEqual(collisionObjectB, collisionObjectCollection.Get(geometricObjectB));
      Assert.AreEqual(collisionObjectC, collisionObjectCollection.Get(geometricObjectC));

      collisionObjectCollection.Remove(collisionObjectB);
      Assert.AreEqual(collisionObjectA, collisionObjectCollection.Get(geometricObjectA));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectB));
      Assert.AreEqual(collisionObjectC, collisionObjectCollection.Get(geometricObjectC));

      collisionObjectCollection.EnableLookupTable = false;
      Assert.AreEqual(collisionObjectA, collisionObjectCollection.Get(geometricObjectA));
      Assert.IsNull(collisionObjectCollection.Get(geometricObjectB));
      Assert.AreEqual(collisionObjectC, collisionObjectCollection.Get(geometricObjectC));
    }
  }
}
