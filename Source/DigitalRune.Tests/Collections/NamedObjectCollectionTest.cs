using System;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class NamedObjectCollectionTest
  {
    private class NamedTestObject : INamedObject
    {
      private readonly string _name;
      private readonly int _value;

      public string Name
      {
        get { return _name; }
      }

      public int Value
      {
        get { return _value; }
      }

      public NamedTestObject(string name, int value)
      {
        _name = name;
        _value = value;
      }
    }

    private class DerivedNamedTestObject : NamedTestObject
    {
      public DerivedNamedTestObject(string name, int value)
        : base(name, value)
      {
      }
    }


    [Test]
    public void TestConstructorWithComparer()
    {
      var collection = new NamedObjectCollection<NamedTestObject>(StringComparer.InvariantCultureIgnoreCase);
      collection.Add(new NamedTestObject("foo1", 1));
      Assert.IsTrue(collection.Contains("FOO1"));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddRangeShouldThrowWhenNull()
    {
      NamedObjectCollection<NamedTestObject> collection = new NamedObjectCollection<NamedTestObject>();
      collection.AddRange(null);
    }


    [Test]
    public void AddRange()
    {
      NamedObjectCollection<NamedTestObject> collection = new NamedObjectCollection<NamedTestObject>();
      collection.AddRange(new[]
      {
        new NamedTestObject("foo1", 1),
        new NamedTestObject("foo2", 2)
      });

      Assert.AreEqual(1, collection["foo1"].Value);
      Assert.AreEqual(2, collection["foo2"].Value);
      Assert.AreEqual(1, collection[0].Value);
      Assert.AreEqual(2, collection[1].Value);
    }


    [Test]
    public void BasicsTest()
    {
      NamedObjectCollection<NamedTestObject> collection = new NamedObjectCollection<NamedTestObject>();
      collection.Add(new NamedTestObject("foo1", 1));
      collection.Add(new NamedTestObject("foo2", 2));

      Assert.AreEqual(1, collection["foo1"].Value);
      Assert.AreEqual(2, collection["foo2"].Value);
      Assert.AreEqual(1, collection[0].Value);
      Assert.AreEqual(2, collection[1].Value);
    }


    [Test]
    public void GetEnumerator()
    {
      NamedObjectCollection<NamedTestObject> collection = new NamedObjectCollection<NamedTestObject>();
      collection.Add(new NamedTestObject("foo1", 1));
      collection.Add(new NamedTestObject("foo2", 2));

      int i = 0;
      foreach(var item in collection)
      {
        i++;
        Assert.AreEqual(i, item.Value);
      }
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ExceptionTest()
    {
      NamedObjectCollection<NamedTestObject> collection = new NamedObjectCollection<NamedTestObject>();

      collection.Add(new NamedTestObject("foo1", 1));
      collection.Add(new NamedTestObject("foo1", 2));
    }


    [Test]
    public void TryGetValueTest()
    {
      NamedObjectCollection<NamedTestObject> collection = new NamedObjectCollection<NamedTestObject>();
      collection.Add(new NamedTestObject("foo1", 1));
      collection.Add(new NamedTestObject("foo2", 2));

      NamedTestObject namedObject;
      Assert.IsTrue(collection.TryGet("foo2", out namedObject));
      Assert.AreEqual(2, namedObject.Value);

      Assert.IsFalse(collection.TryGet("not existing", out namedObject));
      Assert.IsNull(namedObject);

      // Test with other comparer and without internal Dictionary.
      collection = new NamedObjectCollection<NamedTestObject>(StringComparer.InvariantCultureIgnoreCase, 5);
      Assert.IsFalse(collection.TryGet("notExisting", out namedObject));

      collection.Add(new NamedTestObject("foo1", 1));
      collection.Add(new NamedTestObject("foo2", 2));
      Assert.IsTrue(collection.TryGet("foo2", out namedObject));
      Assert.AreEqual(2, namedObject.Value);
      Assert.IsFalse(collection.TryGet("notExisting", out namedObject));
    }


    [Test]
    public void GenericTryGetValueTest()
    {
      NamedObjectCollection<NamedTestObject> collection = new NamedObjectCollection<NamedTestObject>();
      collection.Add(new NamedTestObject("foo1", 1));
      collection.Add(new NamedTestObject("foo2", 2));
      collection.Add(new DerivedNamedTestObject("foo3", 3));

      NamedTestObject namedObject;
      Assert.IsTrue(collection.TryGet<NamedTestObject>("foo2", out namedObject));
      Assert.AreEqual(2, namedObject.Value);

      Assert.IsTrue(collection.TryGet<NamedTestObject>("foo3", out namedObject));
      Assert.AreEqual(3, namedObject.Value);

      DerivedNamedTestObject derivedObject;
      Assert.IsFalse(collection.TryGet<DerivedNamedTestObject>("not existing", out derivedObject));
      Assert.IsNull(derivedObject);

      Assert.IsFalse(collection.TryGet<DerivedNamedTestObject>("foo2", out derivedObject));
      Assert.IsNull(derivedObject);

      Assert.IsTrue(collection.TryGet<DerivedNamedTestObject>("foo3", out derivedObject));
      Assert.AreEqual(3, derivedObject.Value);


      // Test with other comparer and without internal Dictionary.
      collection = new NamedObjectCollection<NamedTestObject>(StringComparer.InvariantCultureIgnoreCase, 5);
      collection.Add(new NamedTestObject("foo1", 1));
      collection.Add(new NamedTestObject("foo2", 2));
      collection.Add(new DerivedNamedTestObject("foo3", 3));

      Assert.IsFalse(collection.TryGet<DerivedNamedTestObject>("not existing", out derivedObject));
      Assert.IsNull(derivedObject);

      Assert.IsFalse(collection.TryGet<DerivedNamedTestObject>("foo2", out derivedObject));
      Assert.IsNull(derivedObject);

      Assert.IsTrue(collection.TryGet<DerivedNamedTestObject>("foo3", out derivedObject));
      Assert.AreEqual(3, derivedObject.Value);
    }


    [Test]
    public void Move()
    {
      NamedObjectCollection<NamedTestObject> collection = new NamedObjectCollection<NamedTestObject>();
      collection.Add(new NamedTestObject("foo1", 1));
      collection.Add(new NamedTestObject("foo2", 2));
      collection.Add(new DerivedNamedTestObject("foo3", 3));

      Assert.Throws(typeof(ArgumentOutOfRangeException), () => collection.Move(-1, 1));
      Assert.Throws(typeof(ArgumentOutOfRangeException), () => collection.Move(1, -1));
      Assert.Throws(typeof(ArgumentOutOfRangeException), () => collection.Move(3, 1));
      Assert.Throws(typeof(ArgumentOutOfRangeException), () => collection.Move(1, 3));

      collection.Move(1, 1);
      Assert.AreEqual(2, collection[1].Value);

      collection.Move(0, 2);
      Assert.AreEqual(2, collection[0].Value);
      Assert.AreEqual(3, collection[1].Value);
      Assert.AreEqual(1, collection[2].Value);
    }
  }
}
