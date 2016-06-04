using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Tests
{
  [TestFixture]
  public class ContactSetCollectionTest
  {
    CollisionObject _a;
    CollisionObject _b;
    CollisionObject _c;
    CollisionObject _d;
    CollisionObject _e;

    [SetUp]
    public void SetUp()
    {
      _a = new CollisionObject();
      _b = new CollisionObject();
      _c = new CollisionObject();
      _d = new CollisionObject();
      _e = new CollisionObject();
    }


    [Test]
    public void Add()
    {
      ContactSetCollection csc = new ContactSetCollection();
      Assert.AreEqual(0, csc.Count);
      csc.Add(ContactSet.Create(_a, _b));
      Assert.AreEqual(1, csc.Count);
      csc.Add(ContactSet.Create(_a, _c));
      Assert.AreEqual(2, csc.Count);
      csc.Add(ContactSet.Create(_d, _a));
      Assert.AreEqual(3, csc.Count);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddException()
    {
      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(ContactSet.Create(_a, _b));
      csc.Add(ContactSet.Create(_a, _b));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddException2()
    {
      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(ContactSet.Create(_a, _b));
      csc.Add(ContactSet.Create(_b, _a));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddException3()
    {
      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddException4()
    {
      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(ContactSet.Create(_a, _b));
      csc.Add(ContactSet.Create(_b, _a));
    }


    [Test]
    public void Clear()
    {
      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(ContactSet.Create(_a, _b));
      csc.Add(ContactSet.Create(_a, _c));
      csc.Add(ContactSet.Create(_d, _a));

      Assert.AreEqual(3, csc.Count);
      csc.Clear();
      Assert.AreEqual(0, csc.Count);
      Assert.IsFalse(csc.Contains(_a, _b));
    }


    [Test]
    public void Contains()
    {
      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(ContactSet.Create(_a, _b));
      csc.Add(ContactSet.Create(_a, _c));
      csc.Add(ContactSet.Create(_d, _a));

      Assert.AreEqual(3, csc.Count);
      Assert.IsTrue(csc.Contains(_a, _b));
      Assert.IsTrue(csc.Contains(_b, _a));
      Assert.IsTrue(csc.Contains(_a, _d));
      Assert.IsFalse(csc.Contains(_a, _a));
      Assert.IsFalse(csc.Contains(_b, _c));
      Assert.IsFalse(csc.Contains(null, null));
      Assert.IsFalse(csc.Contains(_a, null));
      Assert.IsFalse(csc.Contains(null, _a));

      // Same object but other contact set instances.
      Assert.IsFalse(csc.Contains(ContactSet.Create(_a, _b)));
      Assert.IsFalse(csc.Contains(ContactSet.Create(_b, _c)));
    }


    [Test]
    public void ContainsNull()
    {
      Assert.AreEqual(false, new ContactSetCollection().Contains((ContactSet)null));
      Assert.AreEqual(false, new ContactSetCollection().Contains((CollisionObject)null));
    }


    [Test]
    public void CopyTo()
    {
      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(ContactSet.Create(_a, _b));
      csc.Add(ContactSet.Create(_a, _c));
      csc.Add(ContactSet.Create(_d, _a));
      csc.Add(ContactSet.Create(_b, _c));
      csc.Add(ContactSet.Create(_c, _d));

      var array = new ContactSet[5];
      csc.CopyTo(array, 0);
      Assert.IsNotNull(array[0]);
      Assert.IsNotNull(array[1]);
      Assert.IsNotNull(array[2]);
      Assert.IsNotNull(array[3]);
      Assert.IsNotNull(array[4]);

      array = new ContactSet[6];
      csc.CopyTo(array, 1);
      Assert.IsNull(array[0]);
      Assert.IsNotNull(array[1]);
      Assert.IsNotNull(array[2]);
      Assert.IsNotNull(array[3]);
      Assert.IsNotNull(array[4]);
      Assert.IsNotNull(array[5]);
    }


    [Test]
    public void GetContactSet()
    {
      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(ContactSet.Create(_a, _b));
      csc.Add(ContactSet.Create(_a, _c));
      csc.Add(ContactSet.Create(_d, _a));
      csc.Add(ContactSet.Create(_b, _c));
      csc.Add(ContactSet.Create(_c, _d));

      Assert.AreEqual(_a, csc.GetContacts(_a, _b).ObjectA);
      Assert.AreEqual(_b, csc.GetContacts(_b, _a).ObjectB);
      Assert.AreEqual(_a, csc.GetContacts(_a, _d).ObjectB);
      Assert.AreEqual(null, csc.GetContacts(_b, _e));
    }


    [Test]    
    public void GetContactSetNullArgument()
    {
      Assert.AreEqual(null, new ContactSetCollection().GetContacts(_a, null));
      Assert.AreEqual(null, new ContactSetCollection().GetContacts(null, _a));
      Assert.AreEqual(null, new ContactSetCollection().GetContacts(null, null));
    }


    [Test]
    public void GetContactSets()
    {
      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(ContactSet.Create(_a, _b));
      csc.Add(ContactSet.Create(_a, _c));
      csc.Add(ContactSet.Create(_d, _a));
      csc.Add(ContactSet.Create(_b, _c));
      csc.Add(ContactSet.Create(_c, _d));

      Assert.AreEqual(0, csc.GetContacts(_e).Count());
      Assert.AreEqual(3, csc.GetContacts(_a).Count());
      Assert.AreEqual(2, csc.GetContacts(_b).Count());
      Assert.AreEqual(true, csc.GetContacts(_b).ToContactSetCollection().Contains(_b, _c));
      Assert.AreEqual(true, csc.GetContacts(_b).ToContactSetCollection().Contains(_b, _a));
    }


    [Test]
    public void GetContactSetsNullArgument()
    {
      Assert.AreEqual(0, new ContactSetCollection().GetContacts(null).Count());
    }


    [Test]
    public void IsReadOnly()
    {
      Assert.IsFalse(((ICollection<ContactSet>)new ContactSetCollection()).IsReadOnly);
    }


    [Test]
    public void Remove()
    {
      // Remove with null.
      Assert.IsFalse(new ContactSetCollection().Remove((ContactSet) null));
      Assert.IsFalse(new ContactSetCollection().Remove((CollisionObject) null));
      Assert.IsNull(new ContactSetCollection().Remove(null, _a));
      Assert.IsNull(new ContactSetCollection().Remove(_a, (CollisionObject)null));

      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(ContactSet.Create(_a, _b));
      csc.Add(ContactSet.Create(_a, _c));
      csc.Add(ContactSet.Create(_d, _a));
      csc.Add(ContactSet.Create(_b, _c));
      csc.Add(ContactSet.Create(_c, _d));
      Assert.AreEqual(5, csc.Count);
      Assert.IsNotNull(csc.Remove(_a, _d));
      Assert.IsFalse(csc.Contains(_a, _d));
      Assert.AreEqual(4, csc.Count);
      Assert.IsNull(csc.Remove(_a, _d));
      Assert.AreEqual(4, csc.Count);
      Assert.IsTrue(csc.Remove(csc.GetContacts(_c, _b)));
      Assert.IsFalse(csc.Contains(_b, _c));
      Assert.AreEqual(3, csc.Count);
      Assert.IsFalse(csc.Remove(ContactSet.Create(_a, _e)));
      Assert.AreEqual(3, csc.Count);
      Assert.IsTrue(csc.Remove(_a));
      Assert.AreEqual(1, csc.Count);
      Assert.IsFalse(csc.Remove(_b));
      Assert.AreEqual(1, csc.Count);
    }


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("ContactSetCollection { Count = 0 }", new ContactSetCollection().ToString());

      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(ContactSet.Create(_a, _b));
      csc.Add(ContactSet.Create(_a, _c));
      csc.Add(ContactSet.Create(_d, _a));
      csc.Add(ContactSet.Create(_b, _c));
      csc.Add(ContactSet.Create(_c, _d));

      Assert.AreEqual("ContactSetCollection { Count = 5 }", csc.ToString());
    }


    [Test]
    public void TestEnumerator()
    {
      ContactSetCollection csc = new ContactSetCollection();
      csc.Add(ContactSet.Create(_a, _b));
      csc.Add(ContactSet.Create(_a, _c));
      csc.Add(ContactSet.Create(_d, _a));
      csc.Add(ContactSet.Create(_b, _c));
      csc.Add(ContactSet.Create(_c, _d));
      
      IEnumerator enumerator = ((IEnumerable)csc).GetEnumerator();
      IEnumerator<ContactSet> genericEnumerator = csc.GetEnumerator();
      for (int i = 0; i < csc.Count; i++)
      {
        Assert.IsTrue(enumerator.MoveNext());
        Assert.IsTrue(genericEnumerator.MoveNext());
      }

      Assert.IsFalse(enumerator.MoveNext());
      Assert.IsFalse(genericEnumerator.MoveNext());
    }
  }
}
