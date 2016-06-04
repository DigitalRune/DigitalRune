using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  internal class TestObject
  {
    public String Value { get; set; }
  }


  [TestFixture]
  public class WeakCollectionTest
  {
    private object Object1;
    private object Object2;
    private object Object3;
    private object Object4;
    private object Object5;
    private WeakCollection<object> WeakCollection { get; set; }


    [SetUp]
    public void SetUp()
    {
      Object1 = new object();
      Object2 = new object();
      Object3 = new object();
      Object4 = new object();
      Object5 = new object();
      WeakCollection = new WeakCollection<object>
      {
        Object1,
        Object2,
        Object3,
        Object4,
        Object5
      };
    }


    [Test]
    public void FinalizeWeakCollection()
    {
      Object2 = null;
      GC.Collect();

      WeakCollection = null;
      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();
    }


    [Test]
    public void AllObjectShouldBeAlive()
    {
      GC.Collect();

      Assert.AreEqual(5, WeakCollection.Count);
      Assert.IsTrue(WeakCollection.Contains(Object1));
      Assert.IsTrue(WeakCollection.Contains(Object2));
      Assert.IsTrue(WeakCollection.Contains(Object3));
      Assert.IsTrue(WeakCollection.Contains(Object4));
      Assert.IsTrue(WeakCollection.Contains(Object5));
    }


    [Test]
    public void SomeObjectsShouldBeCollected()
    {
      Object3 = null;
      Object5 = null;
      GC.Collect();

      Assert.AreEqual(3, WeakCollection.Count);
      Assert.IsTrue(WeakCollection.Contains(Object1));
      Assert.IsTrue(WeakCollection.Contains(Object2));
      Assert.IsTrue(WeakCollection.Contains(Object4));
    }


    [Test]
    public void IsReadOnly()
    {
      Assert.IsFalse(((ICollection<object>)WeakCollection).IsReadOnly);
    }


    [Test]
    public void SyncRoot()
    {
      Assert.IsNotNull(((ICollection)WeakCollection).SyncRoot);
    }


    [Test]
    public void IsSynchronized()
    {
      Assert.IsFalse(((ICollection)WeakCollection).IsSynchronized);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddShouldThrowWhenItemIsNull()
    {
      WeakCollection.Add(null);
    }


    [Test]
    public void Remove()
    {
      WeakCollection.Remove(Object4);

      Assert.AreEqual(4, WeakCollection.Count);
      Assert.IsTrue(WeakCollection.Contains(Object1));
      Assert.IsTrue(WeakCollection.Contains(Object2));
      Assert.IsTrue(WeakCollection.Contains(Object3));
      Assert.IsFalse(WeakCollection.Contains(Object4));
      Assert.IsTrue(WeakCollection.Contains(Object5));
    }


    [Test]
    public void RemoveNull()
    {
      Object3 = null;
      Object5 = null;
      GC.Collect();

      bool result = WeakCollection.Remove(null);
      Assert.IsFalse(result);
      Assert.AreEqual(3, WeakCollection.Count);
    }


    [Test]
    public void RemoveUnknown()
    {
      Object3 = null;
      Object5 = null;
      GC.Collect();

      bool result = WeakCollection.Remove(new object());
      Assert.IsFalse(result);
      Assert.AreEqual(3, WeakCollection.Count);
    }


    [Test]
    public void Clear()
    {
      Object3 = null;
      Object5 = null;
      GC.Collect();

      WeakCollection.Clear();
      Assert.AreEqual(0, WeakCollection.Count);
    }


    [Test]
    public void CopyTo()
    {
      Object3 = null;
      Object5 = null;
      GC.Collect();

      object[] array = new object[3];
      ((ICollection)WeakCollection).CopyTo(array, 0);
      Assert.AreSame(Object1, array[0]);
      Assert.AreSame(Object2, array[1]);
      Assert.AreSame(Object4, array[2]);
    }


    [Test]
    public void CopyTo2()
    {
      Object3 = null;
      Object5 = null;
      GC.Collect();

      object[] array = new object[3];
      ((ICollection<object>)WeakCollection).CopyTo(array, 0);
      Assert.AreSame(Object1, array[0]);
      Assert.AreSame(Object2, array[1]);
      Assert.AreSame(Object4, array[2]);
    }


    [Test]
    public void GetEnumeratorOfT()
    {
      Object3 = null;
      Object5 = null;
      GC.Collect();

      IEnumerator<object> enumerator = WeakCollection.GetEnumerator();
      enumerator.MoveNext();
      Assert.AreSame(Object1, enumerator.Current);
      enumerator.MoveNext();
      Assert.AreSame(Object2, enumerator.Current);
      enumerator.MoveNext();
      Assert.AreSame(Object4, enumerator.Current);
      Assert.IsFalse(enumerator.MoveNext());
    }


    [Test]
    public void GetEnumerator()
    {
      Object3 = null;
      Object5 = null;
      GC.Collect();

      IEnumerator enumerator = ((IEnumerable)WeakCollection).GetEnumerator();
      enumerator.MoveNext();
      Assert.AreSame(Object1, enumerator.Current);
      enumerator.MoveNext();
      Assert.AreSame(Object2, enumerator.Current);
      enumerator.MoveNext();
      Assert.AreSame(Object4, enumerator.Current);
      Assert.IsFalse(enumerator.MoveNext());
    }


    [Test]
    public void Contains()
    {
      Object3 = null;
      Object5 = null;
      GC.Collect();


      Assert.IsFalse(WeakCollection.Contains(null));
      Assert.IsTrue(WeakCollection.Contains(Object1));
      Assert.IsTrue(WeakCollection.Contains(Object2));
      Assert.IsTrue(WeakCollection.Contains(Object4));
    }


    [Test]
    public void Purge()
    {
      var weakCollection = new WeakCollection<object>();
      var list = new List<object>();
      object obj;
      for (int i = 0; i < 100; i++)
      {
        obj = i;
        list.Add(obj);
        weakCollection.Add(obj);
      }
      obj = null;

      for (int i = list.Count - 1; i >= 0; i--)
      {
        if (i % 2 == 0 || i % 3 == 0)
          list.RemoveAt(i);
      }
      
      GC.Collect();

      int index = 0;
      foreach (object item in weakCollection)
      {
        // Because of compiler/JIT optimization: The last element "99" might be 
        // still be referenced by local variable and can't be purged.
        if (index == list.Count)
          continue;

        Assert.AreEqual(list[index], item);
        index++;
      }

      // WeakCollection<T> should be purged/compacted now.
      // --> Check again.
      index = 0;
      foreach (object item in weakCollection)
      {
        // Because of compiler/JIT optimization: The last element "99" might be 
        // still be referenced by local variable and can't be purged.
        if (index == list.Count)
          continue;

        Assert.AreEqual(list[index], item);
        index++;
      }
    }
  }
}
