using System;
using DigitalRune.Collections;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Tests
{
  [TestFixture]
  public class CollisionFilterTest
  {
    [Test]
    public void ConstructorTest()
    {
      CollisionFilter filter = new CollisionFilter();
      filter.Get(0); // No exception.
      filter.Get(31); // No exception.
      Assert.AreEqual(32, filter.MaxNumberOfGroups);

      CollisionFilter filter400 = new CollisionFilter(400);
      filter400.Get(0); // No exception.
      filter400.Get(399); // No exception.
      Assert.AreEqual(400, filter400.MaxNumberOfGroups);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException()
    {
      new CollisionFilter(-1);
    }


    [Test]
    public void Test1()
    {
      CollisionObject object1A = new CollisionObject();
      object1A.CollisionGroup = 1;
      CollisionObject object1B = new CollisionObject();
      object1B.CollisionGroup = 1;

      CollisionObject object2A = new CollisionObject();
      object2A.CollisionGroup = 2;
      CollisionObject object2B = new CollisionObject();
      object2B.CollisionGroup = 2;

      CollisionFilter filter = new CollisionFilter();

      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object2B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object2B)));

      filter.Set(2, false);
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object1B)));
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object2A, object2B)));
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object2A, object1B)));
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object1A, object2B)));

      filter.Set(1, false);
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object1A, object1B)));
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object2A, object2B)));
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object2A, object1B)));
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object1A, object2B)));

      filter.Set(2, true);
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object1A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object2B)));
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object2A, object1B)));
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object1A, object2B)));

      filter.Set(1, true);
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object2B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object2B)));

      filter.Set(1, 1, false);
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object1A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object2B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object2B)));

      filter.Set(2, 1, false);
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object1A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object2B)));
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object2A, object1B)));
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object1A, object2B)));

      filter.Set(1, 1, true);
      filter.Set(1, 2, true);
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object2B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object2B)));

      filter.Set(object1A, object2B, false);
      filter.Set(object1B, object1A, false);
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object1A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object2B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object1B)));
      Assert.IsFalse(filter.Filter(new Pair<CollisionObject>(object1A, object2B)));

      filter.Set(object1A, object2B, true);
      filter.Set(object1B, object1A, true);
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object2B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object2B)));

      filter.Set(object1B, object1A, false);
      filter.Reset();
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object2B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object2A, object1B)));
      Assert.IsTrue(filter.Filter(new Pair<CollisionObject>(object1A, object2B)));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetException()
    {
      new CollisionFilter().Get(-1);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetException2()
    {
      new CollisionFilter().Get(32);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetException3()
    {
      new CollisionFilter().Get(-1, 0);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetException4()
    {
      new CollisionFilter().Get(32, 0);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetException5()
    {
      new CollisionFilter().Get(0, -1);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetException6()
    {
      new CollisionFilter(33).Get(0, 33);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetException()
    {
      new CollisionFilter().Set(-1, false);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetException2()
    {
      new CollisionFilter().Set(32, false);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetException3()
    {
      new CollisionFilter().Set(-1, 0, false);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetException4()
    {
      new CollisionFilter().Set(32, 0, false);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetException5()
    {
      new CollisionFilter().Set(0, -1, false);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetException6()
    {
      new CollisionFilter().Set(0, 32, false);
    }
  }
}
