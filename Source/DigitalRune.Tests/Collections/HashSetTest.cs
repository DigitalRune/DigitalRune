// Skip unit tests in Silverlight. Silverlight has an implementation of HashSet<T>,
// but the implementation is missing certain methods.
#if WINDOWS || WINDOWS_PHONE || XBOX360

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class HashSetTest
  {
    [Test]
    public void ConstructorTest1()
    {
      var set = new HashSet<string>();

      Assert.AreEqual(0, set.Count);
      Assert.IsNotNull(set.Comparer);
      Assert.IsFalse(((ICollection<string>)set).IsReadOnly);
    }

    
    [Test]
    public void ConstructorTest2()
    {
      var comparer = StringComparer.InvariantCultureIgnoreCase;
      var set = new HashSet<string>(comparer);

      Assert.AreEqual(0, set.Count);
      Assert.AreSame(comparer, set.Comparer);
    }


    [Test]
    public void ConstructorTest3()
    {
      var set = new HashSet<string>(new[] { "Item #1", "Item #2", "Item #3", "item #1" });

      Assert.AreEqual(4, set.Count);
      Assert.IsTrue(set.Contains("Item #1"));
      Assert.IsTrue(set.Contains("Item #2"));
      Assert.IsTrue(set.Contains("Item #3"));
      Assert.IsTrue(set.Contains("item #1"));
    }


    [Test]
    public void ConstructorTest4()
    {
      var comparer = StringComparer.InvariantCultureIgnoreCase;
      var set = new HashSet<string>(new[] { "Item #1", "Item #2", "Item #3", "item #1" }, comparer);

      Assert.AreEqual(3, set.Count);
      Assert.AreSame(comparer, set.Comparer);
      Assert.IsTrue(set.Contains("Item #1"));
      Assert.IsTrue(set.Contains("Item #2"));
      Assert.IsTrue(set.Contains("Item #3"));
      Assert.IsTrue(set.Contains("item #1"));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorShouldThrowIfNull1()
    {
      new HashSet<string>((IEnumerable<string>)null);
    }


    [Test]
    public void ShouldAcceptNullValues()
    {
      var set = new HashSet<string>();
      set.Add(null);
      Assert.AreEqual(1, set.Count);
      Assert.IsTrue(set.Contains(null));
    }


    [Test]
    public void ShouldResize()
    {
      var items = Enumerable.Range(0, 100).ToArray();
      var set = new HashSet<int>();
      foreach (int item in items)
        set.Add(item);

      Assert.AreEqual(items.Length, set.Count);
      foreach (int item in items)
        Assert.IsTrue(set.Contains(item));
    }


    [Test]
    public void ShouldImplementICollectionOfT()
    {
      var set = new HashSet<int>();
      var collection = (ICollection<int>)set;

      collection.Add(1);
      collection.Add(2);
      collection.Add(3);
      Assert.AreEqual(3, collection.Count);
      Assert.IsTrue(collection.Contains(1));
      Assert.IsTrue(collection.Contains(2));
      Assert.IsTrue(collection.Contains(3));

      int[] buffer = new int[4];
      collection.CopyTo(buffer, 1);
      Assert.AreEqual(0, buffer[0]);
      Assert.AreEqual(1, buffer[1]);
      Assert.AreEqual(2, buffer[2]);
      Assert.AreEqual(3, buffer[3]);

      collection.Remove(1);
      Assert.AreEqual(2, collection.Count);
      Assert.IsFalse(collection.Contains(1));

      collection.Clear();
      Assert.AreEqual(0, collection.Count);
    }


    [Test]
    public void CopyToExceptions()
    {
      var set = new HashSet<int>(Enumerable.Range(0, 10));
      Assert.That(() => set.CopyTo(null), Throws.TypeOf(typeof(ArgumentNullException)));

      int[] buffer = new int[9];
      Assert.That(() => set.CopyTo(buffer, 0), Throws.TypeOf(typeof(ArgumentException)));
      Assert.That(() => set.CopyTo(buffer, -1), Throws.TypeOf(typeof(ArgumentOutOfRangeException)));
      Assert.That(() => set.CopyTo(buffer, 9), Throws.TypeOf(typeof(ArgumentException)));
    }


    [Test]
    public void CopyToShouldNotThrowIfEmpty()
    {
      int[] array = new int[0];
      var set = new HashSet<int>();
      set.CopyTo(array, 0);
    }


    [Test]
    public void AddRemove()
    {
      var set = new HashSet<int>();
      foreach (int item in Enumerable.Range(100, 100))
      {
        bool added = set.Add(item);
        Assert.IsTrue(added);
      }

      Assert.AreEqual(100, set.Count);
      Assert.IsFalse(set.Add(120));

      foreach (int item in Enumerable.Range(120, 20))
      {
        bool removed = set.Remove(item);
        Assert.IsTrue(removed);
      }

      Assert.AreEqual(80, set.Count);
      Assert.IsFalse(set.Remove(120));

      foreach (int item in Enumerable.Range(100, 200))
        set.Add(item);

      Assert.AreEqual(200, set.Count);
    }


    [Test]
    public void TrimExcess()
    {
      var set = new HashSet<int>();
      foreach (int item in Enumerable.Range(100, 1000))
        set.Add(item);

      Assert.AreEqual(1000, set.Count);

      set.TrimExcess();

      Assert.AreEqual(1000, set.Count);
      foreach (int item in Enumerable.Range(100, 1000))
        Assert.IsTrue(set.Contains(item));

      foreach (int item in Enumerable.Range(100, 1000).Where(i => i % 2 == 0))
        set.Remove(item);

      set.TrimExcess();
      Assert.AreEqual(500, set.Count);
      Assert.IsTrue(set.All(i => i % 2 == 1));
    }


    [Test]
    public void Chains()
    {
      var set = new HashSet<int>();

      // Initial hash table size is 7.

      // Build chain (hash collisions).
      Assert.IsTrue(set.Add(1));
      Assert.IsTrue(set.Add(8));
      Assert.IsTrue(set.Add(15));

      // Try to remove item not in chain.
      Assert.IsFalse(set.Remove(22));

      // Remove items from chain.
      Assert.IsTrue(set.Remove(1));
      Assert.IsTrue(set.Remove(15));
      Assert.IsTrue(set.Remove(8));

      Assert.AreEqual(0, set.Count);
    }


    [Test]
    public void SetEquals()
    {
      var set0 = new HashSet<int>(new[] { 0, 1, 2, 3 });
      var set1 = new HashSet<int>(new[] { 0, 1, 2, 3 });
      var set2 = new HashSet<int>(new[] { 0, 1, 2, 3, 4 });
      var set3 = new HashSet<int>(new[] { 0, 1, 2, 3, 5 });
      var set4 = new HashSet<string>();
      var set5 = new HashSet<string>();

      Assert.That(() => { set0.SetEquals(null); }, Throws.TypeOf(typeof(ArgumentNullException)));

      Assert.IsTrue(set0.SetEquals(set0));
      Assert.IsTrue(set0.SetEquals(set1));
      Assert.IsFalse(set0.SetEquals(set2));
      Assert.IsFalse(set2.SetEquals(set3));

      // Empty set.
      Assert.IsTrue(set4.SetEquals(set5));
    }


    [Test]
    public void SetComparer()
    {
      var set0 = new HashSet<int>(new[] { 0, 1, 2, 3 });
      var set1 = new HashSet<int>(new[] { 0, 1, 2, 3 });
      var set2 = new HashSet<int>(new[] { 0, 1, 2, 3, 4 });
      var set3 = new HashSet<int>(new[] { 0, 1, 2, 3, 5 });
      var set4 = new HashSet<string>();
      var set5 = new HashSet<string>();

      var comparer = HashSet<int>.CreateSetComparer();
      Assert.IsNotNull(comparer);
      //Assert.AreSame(comparer, HashSet<int>.CreateSetComparer());
      Assert.IsTrue(comparer.Equals(set0, set0));
      Assert.IsTrue(comparer.Equals(set0, set1));
      Assert.IsFalse(comparer.Equals(set0, set2));
      Assert.IsFalse(comparer.Equals(set2, set3));

      Assert.AreEqual(0, comparer.GetHashCode(null));
      Assert.AreEqual(comparer.GetHashCode(set0), comparer.GetHashCode(set1));
      Assert.AreNotEqual(comparer.GetHashCode(set0), comparer.GetHashCode(set2));
      Assert.AreNotEqual(comparer.GetHashCode(set2), comparer.GetHashCode(set3));

      // Empty set.
      var comparer2 = HashSet<string>.CreateSetComparer();
      Assert.IsNotNull(comparer2);
      Assert.IsTrue(comparer2.Equals(set4, set5));
      Assert.AreEqual(comparer2.GetHashCode(set4), comparer2.GetHashCode(set5));
    }


    [Test]
    public void RemoveWhere()
    {
      var set = new HashSet<int>(Enumerable.Range(0, 10));
      Assert.That(() => { set.RemoveWhere(null); }, Throws.TypeOf(typeof(ArgumentNullException)));
      
      set.RemoveWhere(i => i % 2 == 0);
      Assert.AreEqual(5, set.Count);
      Assert.IsTrue(set.All(i => i % 2 == 1));
    }


    [Test]
    public void IntersectWith()
    {
      var set = new HashSet<int>();

      Assert.That(() => set.IntersectWith(null), Throws.TypeOf(typeof(ArgumentNullException)));

      set.IntersectWith(new[] { 3, 9, 11 });
      Assert.IsTrue(set.SetEquals(new HashSet<int>()));

      set.AddRange(Enumerable.Range(0, 10));
      set.IntersectWith(set);
      Assert.IsTrue(set.SetEquals(Enumerable.Range(0, 10)));
      
      set.IntersectWith(new[] { 3, 9, 11 });
      Assert.IsTrue(set.SetEquals(new[] { 3, 9 }));

      set.IntersectWith(Enumerable.Empty<int>());
      Assert.IsTrue(set.SetEquals(new int[0]));
    }


    [Test]
    public void ExceptWith()
    {
      var set = new HashSet<int>();

      Assert.That(() => set.ExceptWith(null), Throws.TypeOf(typeof(ArgumentNullException)));

      set.ExceptWith(new[] { 3, 9, 11 });
      Assert.IsTrue(set.SetEquals(new HashSet<int>()));

      set.AddRange(Enumerable.Range(0, 10));
      set.ExceptWith(new HashSet<int>(new[] { 3, 9, 11 }));
      Assert.IsTrue(set.SetEquals(new[] { 0, 1, 2, 4, 5, 6, 7, 8 }));

      set.ExceptWith(Enumerable.Range(0, 4));
      Assert.IsTrue(set.SetEquals(new[] { 4, 5, 6, 7, 8 }));

      set.ExceptWith(Enumerable.Empty<int>());
      Assert.IsTrue(set.SetEquals(new[] { 4, 5, 6, 7, 8 }));

      set.ExceptWith(set);
      Assert.IsTrue(set.SetEquals(new int[0]));
    }


    [Test]
    public void Overlaps()
    {
      var set = new HashSet<int>();

      Assert.That(() => { set.Overlaps(null); }, Throws.TypeOf(typeof(ArgumentNullException)));
      Assert.IsFalse(set.Overlaps(new[] { 3, 9, 11 }));

      set.AddRange(Enumerable.Range(0, 10));
      Assert.IsTrue(set.Overlaps(set));
      Assert.IsTrue(set.Overlaps(new[] { 3, 9, 11 }));
      Assert.IsTrue(set.Overlaps(new HashSet<int>(new[] { 3, 9, 11 })));
      Assert.IsFalse(set.Overlaps(new[] { 11 }));
      Assert.IsFalse(set.Overlaps(new HashSet<int>(new[] { 11 })));
    }


    [Test]
    public void SymmetricExceptWith()
    {
      var set = new HashSet<int>();

      Assert.That(() => set.SymmetricExceptWith(null), Throws.TypeOf(typeof(ArgumentNullException)));

      set.SymmetricExceptWith(new[] { 3, 9, 11 });
      Assert.IsTrue(set.SetEquals(new[] { 3, 9, 11 }));

      set.SymmetricExceptWith(Enumerable.Range(0, 10));
      Assert.IsTrue(set.SetEquals(new[] { 0, 1, 2, 4, 5, 6, 7, 8, 11 }));

      set.SymmetricExceptWith(set);
      Assert.IsTrue(set.SetEquals(new int[0]));
    }


    [Test]
    public void UnionWith()
    {
      var set = new HashSet<int>();

      Assert.That(() => set.UnionWith(null), Throws.TypeOf(typeof(ArgumentNullException)));

      set.UnionWith(new HashSet<int>(new[] { 3, 9, 11 }));
      Assert.IsTrue(set.SetEquals(new[] { 3, 9, 11 }));

      set.UnionWith(Enumerable.Range(0, 11));
      Assert.IsTrue(set.SetEquals(Enumerable.Range(0, 12)));

      set.UnionWith(set);
      Assert.IsTrue(set.SetEquals(Enumerable.Range(0, 12)));
    }


    [Test]
    public void SubsetsAndSupersets()
    {
      var set0 = new HashSet<int>();
      var set1 = new HashSet<int>(new[] { 0, 1, 2, 3 });
      var set2 = new HashSet<int>(new[] { 0, 1, 2, 3 });
      var set3 = new HashSet<int>(new[] { 0, 1, 2, 3, 4 });
      var set4 = new HashSet<int>(new[] { 0, 1, 2, 3, 5 });

      Assert.That(() => { set1.IsProperSubsetOf(null); }, Throws.TypeOf(typeof(ArgumentNullException)));
      Assert.That(() => { set1.IsProperSupersetOf(null); }, Throws.TypeOf(typeof(ArgumentNullException)));
      Assert.That(() => { set1.IsSubsetOf(null); }, Throws.TypeOf(typeof(ArgumentNullException)));
      Assert.That(() => { set1.IsSupersetOf(null); }, Throws.TypeOf(typeof(ArgumentNullException)));

      Assert.IsTrue(set1.IsSubsetOf(set1));
      Assert.IsTrue(set1.IsSubsetOf(set2));
      Assert.IsTrue(set1.IsSubsetOf(set3));
      Assert.IsFalse(set3.IsSubsetOf(set4));
      Assert.IsFalse(set1.IsProperSubsetOf(set1));
      Assert.IsFalse(set1.IsProperSubsetOf(set2));
      Assert.IsTrue(set1.IsProperSubsetOf(set3));
      Assert.IsFalse(set3.IsProperSubsetOf(set4));
      Assert.IsFalse(set3.IsSubsetOf(set2));
      Assert.IsFalse(set3.IsProperSubsetOf(set2));
      Assert.IsTrue(set3.IsSupersetOf(set3));
      Assert.IsTrue(set3.IsSupersetOf(set2));
      Assert.IsFalse(set3.IsSupersetOf(set4));
      Assert.IsFalse(set3.IsProperSupersetOf(set3));
      Assert.IsTrue(set3.IsProperSupersetOf(set2));
      Assert.IsFalse(set3.IsProperSupersetOf(set4));

      // Empty set.
      Assert.IsTrue(set0.IsSubsetOf(set0));
      Assert.IsTrue(set0.IsSubsetOf(set1));
      Assert.IsFalse(set0.IsProperSubsetOf(set0));
      Assert.IsTrue(set0.IsProperSubsetOf(set1));
      Assert.IsTrue(set0.IsSupersetOf(set0));
      Assert.IsFalse(set0.IsProperSupersetOf(set0));
      Assert.IsFalse(set0.IsSupersetOf(set1));
      Assert.IsTrue(set0.IsProperSubsetOf(set1));
      Assert.IsFalse(set1.IsSubsetOf(set0));
      Assert.IsFalse(set1.IsProperSubsetOf(set0));
      Assert.IsTrue(set1.IsSupersetOf(set0));
      Assert.IsTrue(set1.IsProperSupersetOf(set0));
    }
  }
}
#endif
