using System;
using NUnit.Framework;


namespace DigitalRune.Tests
{
  [TestFixture]
  public class ReferenceEqualityComparerTest
  {
    [Test]    
    public void Test()
    {
      var comparer = ReferenceEqualityComparer<object>.Default;
      Assert.IsNotNull(comparer);
      Assert.AreEqual(comparer, ReferenceEqualityComparer<object>.Default);      

      object intA = 10;
      object intB = 10;
      Assert.AreEqual(intA.GetHashCode(), comparer.GetHashCode(intA));

      Assert.IsTrue(comparer.Equals(null, null));
      Assert.IsFalse(comparer.Equals(intA, null));
      Assert.IsFalse(comparer.Equals(null, intB));
      Assert.IsFalse(comparer.Equals(intA, intB));
      Assert.IsTrue(comparer.Equals(intA, intA));
      Assert.IsFalse(comparer.Equals(10, 10));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetHashArgumentNullException()
    {
      ReferenceEqualityComparer<object>.Default.GetHashCode(null);
    }
  }
}
