using System.Threading;
using DigitalRune.Threading;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class SynchronizedHashtableTest
  {
    private SynchronizedHashtable<int, object> _c;
    private volatile bool _stop;

    [Test]
    public void Test1()
    {
      _stop = false;
      _c = new SynchronizedHashtable<int, object>(10);
      var t1 = Parallel.Start(Query1);
      var t2 = Parallel.Start(Query2);
      var t3 = Parallel.Start(Add);
      var t4 = Parallel.Start(Remove);

      Thread.Sleep(2000);

      _stop = true;
      t1.Wait();
      t2.Wait();
      t3.Wait();
      t4.Wait();
    }

    private void Query1()
    {
      while (!_stop)
      {
        foreach (var entry in _c)
        {
          if (entry.Value == null || entry.Key != (int)entry.Value)
            Assert.Fail();
        }
      }
    }

    private void Query2()
    {
      while (!_stop)
      {
        for (int i = 0; i < 10; i++)
        {
          object o;
          if (_c.TryGet(i, out o))
            if (o == null || (int)o != i)
              Assert.Fail();
        }
      }
    }

    private void Add()
    {
      while (!_stop)
      {
        for (int i = 0; i < 10; i++)
          _c.Add(i, i);
      }
    }


    private void Remove()
    {
      while (!_stop)
      {
        for (int i = 0; i < 10; i++)
          _c.Remove(i);
      }
    }
  }
}
