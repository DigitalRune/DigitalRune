using System;
using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class CollectionHelperTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddRangeShouldThrowArgumentNullException1()
    {
      CollectionHelper.AddRange(null, new [] { 1, 2, 3});
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddRangeShouldThrowArgumentNullException2()
    {
      CollectionHelper.AddRange(new List<int>(), null);
    }


    [Test]
    public void AddRange()
    {
      var list = new List<int>();
      CollectionHelper.AddRange(list, new[] { 1, 2, 3 });
      Assert.AreEqual(3, list.Count);
      Assert.AreEqual(1, list[0]);
      Assert.AreEqual(2, list[1]);
      Assert.AreEqual(3, list[2]);
    }
  }
}
