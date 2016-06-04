#if !ANDROID
using System;
using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class SynchronizedSynchronizedNotifyingCollectionTest
  {
    private List<string> _list;

    [Test]
    public void Constructor()
    {
      Assert.AreEqual(true, new SynchronizedNotifyingCollection<string>().AllowNull);
      Assert.AreEqual(true, new SynchronizedNotifyingCollection<string>().AllowDuplicates);
      Assert.AreEqual(false, new SynchronizedNotifyingCollection<string>(false, true).AllowNull);
      Assert.AreEqual(false, new SynchronizedNotifyingCollection<string>(true, false).AllowDuplicates);
    }

    [Test]
    public void Test1()
    {
      _list = new List<string>();

      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(false, false);
      collection.CollectionChanged += OnCollectionChanged;

      CompareCollections(_list, collection);

      collection.Add("a");
      Assert.AreEqual(1, collection.Count);
      CompareCollections(_list, collection);

      collection.Add("b");
      collection.Add("c");
      Assert.AreEqual(3, collection.Count);
      CompareCollections(_list, collection);

      collection[1] = "d";
      Assert.AreEqual(3, collection.Count);
      CompareCollections(_list, collection);

      collection[1] = collection[1];
      Assert.AreEqual(3, collection.Count);
      CompareCollections(_list, collection);

      collection.RemoveAt(0);
      Assert.AreEqual(2, collection.Count);
      CompareCollections(_list, collection);

      collection.Clear();
      CompareCollections(_list, collection);
      Assert.AreEqual(0, collection.Count);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddRangeShouldThrowWhenNull()
    {
      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>();
      collection.AddRange(null);
    }


    [Test]
    public void TestRangeMethods()
    {
      _list = new List<string>();

      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(false, false);
      collection.CollectionChanged += OnCollectionChanged;

      CompareCollections(_list, collection);

      collection.AddRange(new[] { "a", "b", "c" });
      Assert.AreEqual(3, collection.Count);
      Assert.AreEqual("a", collection[0]);
      Assert.AreEqual("b", collection[1]);
      Assert.AreEqual("c", collection[2]);
      CompareCollections(_list, collection);

      collection.AddRange(new[] { "d", "e" });
      Assert.AreEqual(5, collection.Count);
      Assert.AreEqual("d", collection[3]);
      Assert.AreEqual("e", collection[4]);
      CompareCollections(_list, collection);
    }


    [Test]
    public void TestAllowedNullsAndDuplicates()
    {
      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(true, true);
      collection.Add(null);
      collection.Add("a");
      collection.Add(collection[1]);

      Assert.AreEqual(3, collection.Count);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestNullElements()
    {
      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(false, true);
      collection.Add(null);

      // Duplicate null values should be allowed, even if AllowDuplicates is false.
      collection.Add(null);
    }    


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestNullElements2()
    {
      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(false, true);
      collection.Insert(0, null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestNullElements3()
    {
      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(false, true);
      collection.Add("a");
      collection[0] = null;
    }


    [Test]
    public void DuplicateNullValues()
    {
      // Duplicate null values should be allowed, even if allowDuplicates is false.
      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(true, false);
      collection.Add(null);
      collection.Add(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDuplicateElements()
    {
      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(true, false);
      collection.Add("a");
      collection.Add(collection[0]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDuplicateElements2()
    {
      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(true, false);
      collection.Add("a");
      collection.Add("b");
      collection.Insert(1, collection[0]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDuplicateElements3()
    {
      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(true, false);
      collection.Add("a");
      collection.Add("b");
      collection[0] = collection[1];
    }


    [Test]
    public void TestDuplicateElements4()
    {
      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(true, false);
      collection.Add("a");
      collection.Add("b");

      Assert.That(() => collection[1] = "a", Throws.ArgumentException);

      // But replacing the same item should work!
      collection[0] = "a";
    }


    private void CompareCollections(IList<string> list1, IList<string> list2)
    {
      Assert.AreEqual(list1.Count, list2.Count, "Collections have different number of items.");

      for (int i = 0; i < list1.Count; i++)
        Assert.AreSame(list1[i], list2[i]);
    }


    private void OnCollectionChanged(object sender, CollectionChangedEventArgs<string> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Add)
        Assert.IsTrue(eventArgs.NewItems.Count != 0
                      && eventArgs.NewItemsIndex >= 0
                      && eventArgs.OldItems.Count == 0 
                      && eventArgs.OldItemsIndex == -1);
      if (eventArgs.Action == CollectionChangedAction.Clear)
        Assert.IsTrue(eventArgs.NewItems.Count == 0
                      && eventArgs.NewItemsIndex == -1
                      && eventArgs.OldItems.Count != 0
                      && eventArgs.OldItemsIndex == 0);
      if (eventArgs.Action == CollectionChangedAction.Remove)
        Assert.IsTrue(eventArgs.NewItems.Count == 0
                      && eventArgs.NewItemsIndex == -1
                      && eventArgs.OldItems.Count != 0
                      && eventArgs.OldItemsIndex >= 0);
      if (eventArgs.Action == CollectionChangedAction.Replace)
        Assert.IsTrue(eventArgs.NewItems.Count != 0
                      && eventArgs.NewItemsIndex >= 0
                      && eventArgs.OldItems.Count != 0
                      && eventArgs.OldItemsIndex >= 0);

      foreach (var oldItem in eventArgs.OldItems)
        _list.Remove(oldItem);
      
      if (eventArgs.NewItems.Count > 0)
        _list.InsertRange(eventArgs.NewItemsIndex, eventArgs.NewItems);
    }


    [Test]
    public void GetEnumerator()
    {
      SynchronizedNotifyingCollection<string> collection = new SynchronizedNotifyingCollection<string>(true, false);
      collection.Add("a");
      collection.Add("b");

      var enumerator = collection.GetEnumerator();
      enumerator.MoveNext();           
      Assert.AreEqual("a", enumerator.Current);
      enumerator.MoveNext();
      Assert.AreEqual("b", enumerator.Current);
    }


    [Test]
    public void MoveThrowsArgumentOutOfRangeException()
    {
      var c = new SynchronizedNotifyingCollection<int>();
      c.Add(1);
      c.Add(2);
      c.Add(3);
      c.Add(4);
      c.Add(5);

      Assert.Throws<ArgumentOutOfRangeException>(() => c.Move(-1, 1));
      Assert.Throws<ArgumentOutOfRangeException>(() => c.Move(5, 1));
      Assert.Throws<ArgumentOutOfRangeException>(() => c.Move(1, -1));
      Assert.Throws<ArgumentOutOfRangeException>(() => c.Move(-1, 5));
    }


    [Test]
    public void MoveTest()
    {
      var c = new SynchronizedNotifyingCollection<int>();
      c.Add(1);
      c.Add(2);
      c.Add(3);
      c.Add(4);
      c.Add(5);

      c.CollectionChanged += OnMove;

      c.Move(0, 0);   // Ok, does nothing.
      c.Move(1, 4);

      Assert.AreEqual(1, c[0]);
      Assert.AreEqual(3, c[1]);
      Assert.AreEqual(4, c[2]);
      Assert.AreEqual(5, c[3]);
      Assert.AreEqual(2, c[4]);

      Assert.AreEqual(5, c.Count);
    }


    private void OnMove(object sender, CollectionChangedEventArgs<int> eventArgs)
    {
      Assert.AreEqual(CollectionChangedAction.Move, eventArgs.Action);
      Assert.AreEqual(1, eventArgs.OldItemsIndex);
      Assert.AreEqual(4, eventArgs.NewItemsIndex);
      Assert.AreEqual(0, eventArgs.NewItems.Count);
      Assert.AreEqual(2, eventArgs.OldItems[0]);
    }
  }
}
#endif