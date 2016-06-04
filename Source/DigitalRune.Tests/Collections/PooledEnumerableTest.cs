using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class PooledEnumerableTest
  {
    [Test]
    public void Test1()
    {
      FooWork.NumberOfInstances = 0;
      var e = Foo();

      var x = 0;
      foreach (var i in e)
      {
        Assert.AreEqual(x++, i);
      }
      Assert.AreEqual(10, x);

      x = 0;
      foreach (var i in Foo())
      {
        Assert.AreEqual(x++, i);
      }
      Assert.AreEqual(10, x);

      x = 0;
      e = Foo();
      foreach (var i in e)
      {
        Assert.AreEqual(x++, i);
      }
      Assert.AreEqual(10, x);

      Assert.AreEqual(1, FooWork.NumberOfInstances);

      // 2 parallel enumerators
      var enum0 = Foo().GetEnumerator();
      var enum1 = Foo().GetEnumerator();
      bool hasNext = true;
      x = 0;
      hasNext = enum0.MoveNext();
      enum1.MoveNext();
      while (hasNext)
      {        
        Assert.AreEqual(x, enum0.Current);
        Assert.AreEqual(x, enum1.Current);
        hasNext = enum0.MoveNext();
        enum1.MoveNext();
        x++;
      }
      Assert.AreEqual(10, x);

      Assert.AreEqual(2, FooWork.NumberOfInstances);
    }
    
    //public IEnumerable<int> Foo()
    //{
    //  DoSomeWork0();
    //  for (int i = 0; i < 10; i++)
    //  {
    //    yield return i;
    //  }
    //}

    public IEnumerable<int> Foo()
    {
      return FooWork.Create();
    }    


    internal class FooWork : PooledEnumerable<int>
    {
      public static int NumberOfInstances = 0;

      private static readonly ResourcePool<FooWork> _pool = new ResourcePool<FooWork>(() => new FooWork(), x => x.Initialize(), null);

      private int _i;

      private FooWork()
      {
        NumberOfInstances++;
      }

      public static FooWork Create()
      {
        var enumerable = _pool.Obtain();
        enumerable._i = -1;
        //DoSomeWork0();
        return enumerable;
      }

      protected override bool OnNext(out int current)
      {
        _i++;
        current = _i;
        return (_i < 10);
      }

      protected override void OnRecycle()
      {
        _i = -1;
        _pool.Recycle(this);
      }
    }
  }
}
