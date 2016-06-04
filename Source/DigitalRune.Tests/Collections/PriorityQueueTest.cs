using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class PriorityQueueTest
  {
    [Test]
    public void Properties()
    {
      var pq = new PriorityQueue<int>();

      Assert.NotNull(((ICollection)pq).SyncRoot);
      Assert.AreNotEqual(pq, ((ICollection)pq).SyncRoot);
      Assert.IsFalse(((ICollection)pq).IsSynchronized);
      Assert.IsFalse(((ICollection<int>)pq).IsReadOnly);
    }


    [Test]
    public void Count()
    {
      var pq = new PriorityQueue<int>();
      Assert.AreEqual(0, pq.Count);

      pq.Enqueue(123);
      Assert.AreEqual(1, pq.Count);


      pq.Enqueue(31);
      pq.Enqueue(64);
      Assert.AreEqual(3, pq.Count);

      for (int i = 0; i < 30; i++)
        pq.Enqueue(i);

      pq.Peek();
      pq.Dequeue();
      pq.Dequeue();
      Assert.AreEqual(31, pq.Count);

      pq.Clear();
      Assert.AreEqual(0, pq.Count);
    }


    [Test]
    public void Comparer()
    {
      var pq = new PriorityQueue<int>();
      Assert.AreEqual(Comparer<int>.Default, pq.Comparer);

      pq = new PriorityQueue<int>((x, y) => x - y);
      Assert.AreEqual(3, pq.Comparer.Compare(5, 2));
    }


    [Test]
    public void Constructor()
    {
      Assert.Throws(typeof(ArgumentNullException), () => new PriorityQueue<object>((IComparer<object>)null));
      Assert.Throws(typeof(ArgumentNullException), () => new PriorityQueue<object>((Comparison<object>)null));
    }


    [Test]
    public void Test()
    {
      var pq = new PriorityQueue<int>();

      var random = new Random();
      for (int i = 0; i < 100; i++)
      {
        var value = random.Next(0, 20);

        pq.Enqueue(value);
        pq.Validate();
      }


      var max = int.MaxValue;
      for (int i = 0; i < 100; i++)
      {
        Assert.AreEqual(100 - i, pq.Count);

        var peek = pq.Peek();
        var value = pq.Dequeue();

        Assert.AreEqual(peek, value);
        Assert.IsTrue(max >= value);
        max = value;

        pq.TrimExcess();
      }
    }


    [Test]
    public void MinPriorityQueue()
    {
      var pq = new PriorityQueue<int>((x, y) => y - x);

      pq.Enqueue(2);
      pq.Enqueue(1);
      pq.Enqueue(3);

      Assert.AreEqual(1, pq.Dequeue());
      Assert.AreEqual(2, pq.Dequeue());
      Assert.AreEqual(3, pq.Dequeue());
    }


    [Test]
    public void Clear()
    {
      var pq = new PriorityQueue<int>();

      pq.Clear();
      Assert.AreEqual(0, pq.Count);

      pq.Enqueue(777);
      pq.Enqueue(234);
      pq.Enqueue(13);

      pq.Clear();
      Assert.AreEqual(0, pq.Count);
    }


    [Test]
    public void Contains()
    {
      var pq = new PriorityQueue<object>();

      for (int i = 0; i < 20; i++)
        pq.Enqueue(i);

      Assert.IsTrue(pq.Contains(0));
      Assert.IsTrue(pq.Contains(14));
      Assert.IsTrue(pq.Contains(19));

      Assert.IsFalse(pq.Contains(-1));
      Assert.IsFalse(pq.Contains(20));

      pq.Dequeue();
      Assert.IsFalse(pq.Contains(19));
    }


    [Test]
    public void CopyTo()
    {
      var pq = new PriorityQueue<int>();
      pq.Enqueue(1);
      pq.Enqueue(1);
      pq.Enqueue(1);

      int[] array = new int[10];
      pq.CopyTo(array, 3);
      Assert.AreEqual(1, array[3]);
      Assert.AreEqual(1, array[4]);
      Assert.AreEqual(1, array[5]);

      array = new int[10];
      ((ICollection)pq).CopyTo(array, 3);
      Assert.AreEqual(1, array[3]);
      Assert.AreEqual(1, array[4]);
      Assert.AreEqual(1, array[5]);
    }


    [Test]
    public void PeekThrowsException()
    {
      Assert.Throws(typeof(InvalidOperationException), () => new PriorityQueue<int>().Peek());
    }


    [Test]
    public void DequeueThrowsException()
    {
      Assert.Throws(typeof(InvalidOperationException), () => new PriorityQueue<int>().Dequeue());
    }


    [Test]
    public void Remove()
    {
      var pq = new PriorityQueue<object>();

      for (int i = 0; i < 20; i++)
        pq.Enqueue(i);

      Assert.IsTrue(pq.Remove(0));
      Assert.IsTrue(pq.Remove(14));
      Assert.IsTrue(pq.Remove(19));

      Assert.IsFalse(pq.Remove(-1));
      Assert.IsFalse(pq.Remove(20));

      Assert.AreEqual(17, pq.Count);
    }


    [Test]
    public void TestNullAndDuplicates()
    {
      var pq = new PriorityQueue<object>();

      pq.Enqueue(null);
      pq.Enqueue(1);
      pq.Enqueue(null);
      pq.Enqueue(2);
      pq.Enqueue(2);

      Assert.AreEqual(2, pq.Dequeue());
      Assert.AreEqual(2, pq.Dequeue());
      Assert.AreEqual(1, pq.Dequeue());
      Assert.AreEqual(null, pq.Dequeue());
      Assert.AreEqual(null, pq.Dequeue());
    }


    [Test]
    public void Enumerator()
    {
      var pq = new PriorityQueue<int>();

      for (int i = 0; i < 10; i++)
        pq.Enqueue(i);

      var list = new List<int>();
      foreach (var i in pq)
      {
        Assert.IsTrue(!list.Contains(i));
        list.Add(i);
      }

      Assert.AreEqual(pq.Count, list.Count);
      
      Assert.Throws(typeof(InvalidOperationException), () =>
      {
        foreach (var i in pq)
        {
          pq.Enqueue(1);
        }
      });

      Assert.Throws(typeof(InvalidOperationException), () =>
      {
        foreach (var i in pq)
        {
          pq.Dequeue();
        }
      });

      Assert.Throws(typeof(InvalidOperationException), () =>
      {
        foreach (var i in pq)
        {
          pq.Remove(1);
        }
      });

      Assert.Throws(typeof(InvalidOperationException), () =>
      {
        foreach (var i in pq)
        {
          pq.Clear();
        }
      });
    }


    //[Test]
    //public void CreateFromCollection()
    //{
    //  var deque = new Deque<int>(new[] { 2, 3, 4, 5, 6 });
    //  Assert.AreEqual(5, deque.Count);
    //  Assert.AreEqual(2, deque[0]);
    //  Assert.AreEqual(3, deque[1]);
    //  Assert.AreEqual(4, deque[2]);
    //  Assert.AreEqual(5, deque[3]);
    //  Assert.AreEqual(6, deque[4]);

    //  Assert.That(() => { new Deque<int>(null); }, Throws.TypeOf<ArgumentNullException>());
    //}


    //[Test]
    //public void CreateWithCapacity()
    //{
    //  var deque = new Deque<int>(0);
    //  deque = new Deque<int>(1);
    //  deque = new Deque<int>(10);
    //  Assert.That(() => { new Deque<int>(-1); }, Throws.TypeOf<ArgumentOutOfRangeException>());
    //}


    //[Test]
    //public void SyncRoot()
    //{
    //  var deque = new Deque<int>();
    //  object syncRoot = ((ICollection)deque).SyncRoot;
    //  Assert.IsNotNull(syncRoot);
    //  Assert.AreSame(syncRoot, ((ICollection)deque).SyncRoot);
    //}


    //[Test]
    //public void IsSynchronizedShouldBeFalse()
    //{
    //  var deque = new Deque<int>();
    //  Assert.IsFalse(((ICollection)deque).IsSynchronized);
    //}


    //[Test]
    //public void IsReadOnlyShouldBeFalse()
    //{
    //  var deque = new Deque<int>();
    //  Assert.IsFalse(((ICollection<int>)deque).IsReadOnly);
    //}


    //[Test]
    //public void Head()
    //{
    //  var deque = new Deque<int>();
    //  Assert.That(() => { int i = deque.Head; }, Throws.TypeOf<InvalidOperationException>());
    //  Assert.That(() => { deque.Head = 0; }, Throws.TypeOf<InvalidOperationException>());

    //  deque.EnqueueTail(123);
    //  Assert.AreEqual(123, deque.Head);

    //  deque.EnqueueTail(234);
    //  Assert.AreEqual(123, deque.Head);

    //  deque.EnqueueHead(345);
    //  Assert.AreEqual(345, deque.Head);

    //  deque.Head = 1;
    //  Assert.AreEqual(1, deque.Head);

    //  deque.DequeueHead();
    //  Assert.AreEqual(123, deque.Head);
    //}


    //[Test]
    //public void Tail()
    //{
    //  var deque = new Deque<int>();
    //  Assert.That(() => { int i = deque.Tail; }, Throws.TypeOf<InvalidOperationException>());
    //  Assert.That(() => { deque.Tail = 0; }, Throws.TypeOf<InvalidOperationException>());

    //  deque.EnqueueHead(123);
    //  Assert.AreEqual(123, deque.Tail);

    //  deque.EnqueueTail(234);
    //  Assert.AreEqual(234, deque.Tail);

    //  deque.EnqueueHead(345);
    //  Assert.AreEqual(234, deque.Tail);

    //  deque.Tail = 1;
    //  Assert.AreEqual(1, deque.Tail);

    //  deque.DequeueHead();
    //  Assert.AreEqual(1, deque.Tail);

    //  deque.DequeueTail();
    //  Assert.AreEqual(123, deque.Head);
    //}


    //[Test]
    //public void Indexer()
    //{
    //  var deque = new Deque<int>(new[] { 2, 3, 4, 5, 6 });
    //  Assert.AreEqual(5, deque.Count);
    //  Assert.AreEqual(2, deque[0]);
    //  Assert.AreEqual(3, deque[1]);
    //  Assert.AreEqual(4, deque[2]);
    //  Assert.AreEqual(5, deque[3]);
    //  Assert.AreEqual(6, deque[4]);

    //  deque[0] = 1;
    //  deque[4] = 4;
    //  Assert.AreEqual(1, deque[0]);
    //  Assert.AreEqual(4, deque[4]);

    //  Assert.That(() => { int i = deque[-1]; }, Throws.TypeOf<ArgumentOutOfRangeException>());
    //  Assert.That(() => { int i = deque[5]; }, Throws.TypeOf<ArgumentOutOfRangeException>());
    //  Assert.That(() => { deque[-1] = 0; }, Throws.TypeOf<ArgumentOutOfRangeException>());
    //  Assert.That(() => { deque[-5] = 0; }, Throws.TypeOf<ArgumentOutOfRangeException>());

    //  deque.Clear();
    //  Assert.That(() => { int i = deque[0]; }, Throws.TypeOf<ArgumentOutOfRangeException>());
    //  Assert.That(() => { int i = deque[0]; }, Throws.TypeOf<ArgumentOutOfRangeException>());
    //  Assert.That(() => { deque[0] = 0; }, Throws.TypeOf<ArgumentOutOfRangeException>());
    //  Assert.That(() => { deque[0] = 0; }, Throws.TypeOf<ArgumentOutOfRangeException>());
    //}


    //[Test]
    //public void Add()
    //{
    //  var deque = new Deque<int>();
    //  ((ICollection<int>)deque).Add(456);
    //  Assert.AreEqual(1, deque.Count);
    //  Assert.AreEqual(456, deque.Head);
    //  Assert.AreEqual(456, deque.Tail);
    //  Assert.AreEqual(456, deque[0]);
    //}


    //[Test]
    //public void Clear()
    //{
    //  var deque = new Deque<int>(new[] { 2, 3, 4, 5, 6 });
    //  deque.Clear();
    //  Assert.AreEqual(0, deque.Count);

    //  deque = new Deque<int>(new[] { 2, 3, 4, 5, 6 });
    //  deque.EnqueueHead(1);
    //  deque.EnqueueHead(0);
    //  deque.Clear();
    //  Assert.AreEqual(0, deque.Count);

    //  deque = new Deque<int>();
    //  deque.EnqueueHead(1);
    //  deque.EnqueueHead(0);
    //  deque.Clear();
    //  Assert.AreEqual(0, deque.Count);
    //}


    //[Test]
    //public void Contains()
    //{
    //  var deque = new Deque<int>(new[] { 2, 3, 4, 5, 6 });
    //  Assert.IsTrue(deque.Contains(2));
    //  Assert.IsTrue(deque.Contains(6));
    //  Assert.IsFalse(deque.Contains(7));

    //  deque = new Deque<int>(new[] { 2, 3, 4, 5, 6 });
    //  deque.EnqueueHead(1);
    //  deque.EnqueueHead(0);
    //  Assert.IsTrue(deque.Contains(0));
    //  Assert.IsTrue(deque.Contains(6));
    //  Assert.IsFalse(deque.Contains(7));

    //  var deque2 = new Deque<string>();
    //  deque2.EnqueueHead("item 1");
    //  deque2.EnqueueHead(null);
    //  Assert.IsTrue(deque2.Contains("item 1"));
    //  Assert.IsTrue(deque2.Contains(null));
    //  Assert.IsFalse(deque2.Contains(String.Empty));
    //  Assert.IsFalse(deque2.Contains("item 2"));
    //}


    //[Test]
    //public void CopyTo()
    //{
    //  int[] array = new int[5];
    //  var deque = new Deque<int>(new[] { 2, 3, 4 });
    //  deque.CopyTo(array, 2);
    //  Assert.AreEqual(0, array[0]);
    //  Assert.AreEqual(0, array[1]);
    //  Assert.AreEqual(2, array[2]);
    //  Assert.AreEqual(3, array[3]);
    //  Assert.AreEqual(4, array[4]);

    //  array = new int[5];
    //  deque = new Deque<int>();
    //  deque.EnqueueHead(3);
    //  deque.EnqueueHead(2);
    //  deque.EnqueueTail(4);
    //  deque.CopyTo(array, 2);
    //  Assert.AreEqual(0, array[0]);
    //  Assert.AreEqual(0, array[1]);
    //  Assert.AreEqual(2, array[2]);
    //  Assert.AreEqual(3, array[3]);
    //  Assert.AreEqual(4, array[4]);

    //  Assert.That(() => deque.CopyTo(null, 0), Throws.TypeOf<ArgumentNullException>());
    //  Assert.That(() => deque.CopyTo(new int[5], -1), Throws.TypeOf<ArgumentOutOfRangeException>());
    //  Assert.That(() => deque.CopyTo(new int[2], 0), Throws.TypeOf<ArgumentException>());
    //  Assert.That(() => deque.CopyTo(new int[5], 5), Throws.TypeOf<ArgumentOutOfRangeException>());
    //  Assert.That(() => deque.CopyTo(new int[5], 4), Throws.TypeOf<ArgumentException>());
    //}


    //[Test]
    //public void CopyToShouldNotThrowIfEmpty()
    //{
    //  int[] array = new int[0];
    //  var deque = new Deque<int>();
    //  deque.CopyTo(array, 0);
    //}


    //[Test]
    //public void EnqueueDequeueHead()
    //{
    //  var deque = new Deque<int>();
    //  Assert.That(() => deque.DequeueHead(), Throws.TypeOf<InvalidOperationException>());
    //  deque.EnqueueHead(1);
    //  Assert.AreEqual(1, deque.Count);
    //  Assert.AreEqual(1, deque.DequeueHead());
    //  Assert.AreEqual(0, deque.Count);

    //  deque = new Deque<int>(new[] { 2, 3, 4 });
    //  Assert.AreEqual(2, deque.DequeueHead());
    //  deque.EnqueueHead(0);
    //  Assert.AreEqual(3, deque.Count);
    //  Assert.AreEqual(0, deque.DequeueHead());
    //  Assert.AreEqual(2, deque.Count);
    //}


    //[Test]
    //public void EnqueueDequeueTail()
    //{
    //  var deque = new Deque<int>();
    //  Assert.That(() => deque.DequeueTail(), Throws.TypeOf<InvalidOperationException>());
    //  deque.EnqueueHead(1);
    //  Assert.AreEqual(1, deque.Count);
    //  Assert.AreEqual(1, deque.DequeueTail());
    //  Assert.AreEqual(0, deque.Count);

    //  deque = new Deque<int>(new[] { 2, 3, 4 });
    //  Assert.AreEqual(4, deque.DequeueTail());
    //  deque.EnqueueTail(0);
    //  Assert.AreEqual(3, deque.Count);
    //  Assert.AreEqual(0, deque.DequeueTail());
    //  Assert.AreEqual(2, deque.Count);
    //}


    //[Test]
    //public void IndexOf()
    //{
    //  var deque = new Deque<int>(new[] { 2, 3, 4, 5, 6 });
    //  Assert.AreEqual(0, deque.IndexOf(2));
    //  Assert.AreEqual(4, deque.IndexOf(6));
    //  Assert.AreEqual(-1, deque.IndexOf(7));

    //  deque = new Deque<int>(new[] { 2, 3, 4, 5, 6 });
    //  deque.EnqueueHead(1);
    //  deque.EnqueueHead(0);
    //  Assert.AreEqual(0, deque.IndexOf(0));
    //  Assert.AreEqual(2, deque.IndexOf(2));
    //  Assert.AreEqual(6, deque.IndexOf(6));
    //  Assert.AreEqual(-1, deque.IndexOf(7));

    //  var deque2 = new Deque<string>();
    //  deque2.EnqueueHead("item 1");
    //  deque2.EnqueueHead(null);
    //  Assert.AreEqual(1, deque2.IndexOf("item 1"));
    //  Assert.AreEqual(0, deque2.IndexOf(null));
    //  Assert.AreEqual(-1, deque2.IndexOf(String.Empty));
    //  Assert.AreEqual(-1, deque2.IndexOf("item 2"));
    //}


    //[Test]
    //public void ToArray()
    //{
    //  var deque = new Deque<int>();
    //  var array = deque.ToArray();
    //  Assert.AreEqual(0, array.Length);

    //  deque = new Deque<int>(new[] { 2, 3, 4 });
    //  array = deque.ToArray();
    //  Assert.AreEqual(3, array.Length);
    //  Assert.AreEqual(2, array[0]);
    //  Assert.AreEqual(3, array[1]);
    //  Assert.AreEqual(4, array[2]);
    //}


    //[Test]
    //public void TrimExcess()
    //{
    //  var deque = new Deque<int>();
    //  deque.TrimExcess();

    //  deque = new Deque<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    //  deque.TrimExcess();

    //  deque.Clear();
    //  deque.TrimExcess();
    //}


    //[Test]
    //public void Enumerator()
    //{
    //  var deque = new Deque<int>(new[] { 2, 3, 4 });
    //  Assert.IsNotNull(((IEnumerable<int>)deque).GetEnumerator());
    //  Assert.IsNotNull(((IEnumerable)deque).GetEnumerator());

    //  Deque<int>.Enumerator enumerator = deque.GetEnumerator();
    //  Assert.AreEqual(0, enumerator.Current);
    //  Assert.That(() => ((IEnumerator)enumerator).Current, Throws.TypeOf<InvalidOperationException>());

    //  Assert.IsTrue(enumerator.MoveNext());
    //  Assert.AreEqual(2, enumerator.Current);
    //  Assert.AreEqual(2, ((IEnumerator)enumerator).Current);
    //  Assert.IsTrue(enumerator.MoveNext());
    //  Assert.AreEqual(3, enumerator.Current);
    //  Assert.AreEqual(3, ((IEnumerator)enumerator).Current);
    //  Assert.IsTrue(enumerator.MoveNext());
    //  Assert.AreEqual(4, enumerator.Current);
    //  Assert.AreEqual(4, ((IEnumerator)enumerator).Current);
    //  Assert.IsFalse(enumerator.MoveNext());
    //  Assert.AreEqual(0, enumerator.Current);
    //  Assert.That(() => ((IEnumerator)enumerator).Current, Throws.TypeOf<InvalidOperationException>());

    //  enumerator.Reset();
    //  Assert.IsTrue(enumerator.MoveNext());
    //  Assert.AreEqual(2, enumerator.Current);

    //  enumerator.Dispose();
    //  Assert.AreEqual(0, enumerator.Current);
    //  Assert.IsFalse(enumerator.MoveNext());
    //  Assert.That(() => ((IEnumerator)enumerator).Current, Throws.TypeOf<InvalidOperationException>());

    //  enumerator = deque.GetEnumerator();
    //  enumerator.MoveNext();
    //  deque.EnqueueTail(5);
    //  Assert.That(() => enumerator.MoveNext(), Throws.TypeOf<InvalidOperationException>());
    //  Assert.That(() => enumerator.Reset(), Throws.TypeOf<InvalidOperationException>());
    //}


    //[Test]
    //public void UnsupportedMethods()
    //{
    //  var deque = new Deque<int>();
    //  Assert.That(() => ((ICollection<int>)deque).Remove(1), Throws.TypeOf<NotSupportedException>());
    //  Assert.That(() => ((IList<int>)deque).RemoveAt(0), Throws.TypeOf<NotSupportedException>());
    //  Assert.That(() => ((IList<int>)deque).Insert(0, 1), Throws.TypeOf<NotSupportedException>());
    //}
  }
}

