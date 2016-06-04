using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Collections;
using NUnit.Framework;


namespace DigitalRune.Linq.Tests
{
  [TestFixture]
  public class LinqHelperTest
  {

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DoShouldThrowWhenSourceIsNull()
    {
      LinqHelper.Do<int>(null, x => x--);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DoShouldThrowWhenActionIsNull()
    {
      LinqHelper.Do(new[] { 1, 2, 3 }, (Action<int>)null);
    }


    [Test]
    public void Do()
    {
      var list = new List<object>();
      var count = LinqHelper.Do(new[] { 10, 20, 30 }, x => list.Add(x)).Count();
      Assert.AreEqual(3, count);
      Assert.AreEqual(10, list[0]);
      Assert.AreEqual(20, list[1]);
      Assert.AreEqual(30, list[2]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DoWithIndexShouldThrowWhenSourceIsNull()
    {
      LinqHelper.Do<int>(null, (x, i) => x--);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DoWithIndexShouldThrowWhenActionIsNull()
    {
      LinqHelper.Do(new[] { 1, 2, 3 }, (Action<int, int>)null);
    }


    [Test]
    public void DoWithIndex()
    {
      var list = new List<Pair<int, int>>();
      var count = LinqHelper.Do(new[] { 10, 20, 30 }, (x, index) => list.Add(new Pair<int, int>(x, index))).Count();
      Assert.AreEqual(3, count);
      Assert.AreEqual(10, list[0].First);
      Assert.AreEqual(20, list[1].First);
      Assert.AreEqual(30, list[2].First);
      Assert.AreEqual(0, list[0].Second);
      Assert.AreEqual(1, list[1].Second);
      Assert.AreEqual(2, list[2].Second);
    }

    
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ForEachShouldThrowWhenSourceIsNull()
    {
      LinqHelper.ForEach<int>(null, x => x--);
    }

    
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ForEachShouldThrowWhenActionIsNull()
    {
      LinqHelper.ForEach(new[] { 1, 2, 3 }, (Action<int>)null);
    }

    
    [Test]
    public void ForEachWithIndex()
    {
      var list = new List<object>();
      LinqHelper.ForEach(new[] { 10, 20, 30 }, x => list.Add(x));
      Assert.AreEqual(3, list.Count);
      Assert.AreEqual(10, list[0]);
      Assert.AreEqual(20, list[1]);
      Assert.AreEqual(30, list[2]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ForEachWithIndexShouldThrowWhenSourceIsNull()
    {
      LinqHelper.ForEach<int>(null, (x, i) => x--);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ForEachWithIndexShouldThrowWhenActionIsNull()
    {
      LinqHelper.ForEach(new[] { 1, 2, 3 }, (Action<int, int>)null);
    }


    [Test]
    public void ForEach()
    {
      var list = new List<Pair<int, int>>();
      LinqHelper.ForEach(new[] { 10, 20, 30 }, (x, index) => list.Add(new Pair<int, int>(x, index)));
      Assert.AreEqual(3, list.Count);
      Assert.AreEqual(10, list[0].First);
      Assert.AreEqual(20, list[1].First);
      Assert.AreEqual(30, list[2].First);
      Assert.AreEqual(0, list[0].Second);
      Assert.AreEqual(1, list[1].Second);
      Assert.AreEqual(2, list[2].Second);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void IndexOfShouldThrowWhenPredicateIsNull()
    {
      LinqHelper.IndexOf(new[] { 1, 2, 3, 4 }, null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void IndexOfShouldThrowWhenSourceIsNull()
    {
      LinqHelper.IndexOf<int>(null, x => true);
    }


    [Test]
    public void IndexOf()
    {
      Assert.AreEqual(0, LinqHelper.IndexOf(new [] { 11, 22, 33, 44, 55}, x => true));
      Assert.AreEqual(-1, LinqHelper.IndexOf(new[] { 11, 22, 33, 44, 55 }, x => false));
      Assert.AreEqual(3, LinqHelper.IndexOf(new[] { 11, 22, 33, 44, 55 }, x => x > 40));
    }


    [Test]
    public void SequenceTest()
    {
      Assert.AreEqual(1, LinqHelper.Return("element").Count());
      Assert.AreEqual(1, LinqHelper.Return<object>(null).Count());
      Assert.AreEqual("element", LinqHelper.Return("element").ElementAt(0));
    }
  }
}
