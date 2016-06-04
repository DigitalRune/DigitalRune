using DigitalRune.Physics;
using NUnit.Framework;


namespace DigitalRune.Physics.Tests
{
  [TestFixture]
  public class UnionFinderTest
  {
    [Test]
    public void Test0()
    {
      var u = new UnionFinder();

      u.Reset(10);

      u.Unite(0, 9);
      Assert.AreEqual(9, u.NumberOfUnions);
      
      u.Unite(4, 1);
      Assert.AreEqual(8, u.NumberOfUnions);

      u.Unite(1, 1);
      Assert.AreEqual(8, u.NumberOfUnions);

      u.Unite(5, 6);
      Assert.AreEqual(7, u.NumberOfUnions);

      u.Unite(9, 4);
      Assert.AreEqual(6, u.NumberOfUnions);

      Assert.AreEqual(u.FindUnion(0), u.FindUnion(1));
      Assert.IsTrue(u.AreUnited(0, 1));
      Assert.AreEqual(u.FindUnion(1), u.FindUnion(4));
      Assert.IsTrue(u.AreUnited(4, 1));
      Assert.AreEqual(u.FindUnion(4), u.FindUnion(9));
      Assert.IsTrue(u.AreUnited(9, 4));
      Assert.AreEqual(u.FindUnion(5), u.FindUnion(6));
      Assert.IsTrue(u.AreUnited(6, 5));
      Assert.AreNotEqual(u.FindUnion(0), u.FindUnion(3));
      Assert.IsFalse(u.AreUnited(3, 0));

      Assert.AreEqual(4, u.GetUnionSize(0));
    }
  }
}
