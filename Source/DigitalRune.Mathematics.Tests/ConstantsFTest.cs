using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Tests
{
  [TestFixture]
  public class ConstantsFTest
  {
    [Test]
    public void Constants()
    {
      Assert.IsTrue(Numeric.AreEqual((float)Math.E, ConstantsF.E));
      Assert.IsTrue(Numeric.AreEqual((float)Math.Log10(Math.E), ConstantsF.Log10OfE));
      Assert.IsTrue(Numeric.AreEqual((float)Math.Log(Math.E) / (float)Math.Log(2), ConstantsF.Log2OfE));
      Assert.IsTrue(Numeric.AreEqual(1 / (float)Math.PI, ConstantsF.OneOverPi));
      Assert.IsTrue(Numeric.AreEqual((float)Math.PI, ConstantsF.Pi));
      Assert.IsTrue(Numeric.AreEqual((float)Math.PI / 2f, ConstantsF.PiOver2));
      Assert.IsTrue(Numeric.AreEqual((float)Math.PI / 4f, ConstantsF.PiOver4));
      Assert.IsTrue(Numeric.AreEqual((float)Math.PI * 2f, ConstantsF.TwoPi));
    }
  }
}
