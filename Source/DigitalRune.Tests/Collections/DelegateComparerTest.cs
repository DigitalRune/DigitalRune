using System;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class DelegateComparerTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorNullException()
    {
      new DelegateComparer<int>(null);
    }


    [Test]
    public void Test()
    {
      // Test a senseless comparison operator.
      var c = new DelegateComparer<int>((x, y) => x + y);
      Assert.AreEqual(10, c.Compare(3, 7));
    }
  }
}

