using System;
using DigitalRune.Geometry.Collisions.Algorithms;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Tests
{
  [TestFixture]
  public class ContactSetTest
  {
    [Test]
    public void Swapped()
    {
      CollisionObject a = new CollisionObject();
      CollisionObject b = new CollisionObject();

      ContactSet set = ContactSet.Create(a, b);
      set.Add(ContactHelper.CreateContact(set, new Vector3F(1, 2, 3), new Vector3F(0, 0, 1), 10, false));
      set.Add(ContactHelper.CreateContact(set, new Vector3F(4, 5, 6), new Vector3F(0, 0, 1), 10, false));
      set.Add(ContactHelper.CreateContact(set, new Vector3F(7, 8, 9), new Vector3F(0, 0, 1), 10, false));

      ContactSet swapped = set.Swapped;
      Assert.AreEqual(set.ObjectA, swapped.ObjectB);
      Assert.AreEqual(set.ObjectB, swapped.ObjectA);
      Assert.AreEqual(set[0].PositionALocal, swapped[0].PositionBLocal);
      Assert.AreEqual(set[1].Normal, -swapped[1].Normal);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException()
    {
      ContactSet.Create(null, new CollisionObject());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException2()
    {
      ContactSet.Create(new CollisionObject(), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException3()
    {
      CollisionObject a = new CollisionObject();
      ContactSet.Create(a, a);
    }


    //[Test]
    //public void ToStringTest()
    //{
    //  CollisionObject a = new CollisionObject { Name = "a" };
    //  CollisionObject b = new CollisionObject { Name = "b" };

    //  ContactSet set = ContactSet.Create(a, b);
    //  set.Add(ContactHelper.CreateContact(set, new Vector3F(1, 2, 3), new Vector3F(0, 0, 1), 10, false));
    //  set.Add(ContactHelper.CreateContact(set, new Vector3F(4, 5, 6), new Vector3F(0, 0, 1), 10, false));
    //  set.Add(ContactHelper.CreateContact(set, new Vector3F(7, 8, 9), new Vector3F(0, 0, 1), 10, false));

    //  Assert.AreEqual("ContactSet{ObjectA=\"a\", ObjectB=\"b\", Count=3}", set.ToString());
    //}
  }
}
