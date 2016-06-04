using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Tests
{
  [TestFixture]
  public class ConstantsDTest
  {
    [Test]
    public void Constants()
    {
      Assert.IsTrue(Numeric.AreEqual(Math.E, ConstantsD.E));
      Assert.IsTrue(Numeric.AreEqual(Math.Log10(Math.E), ConstantsD.Log10OfE));
      Assert.IsTrue(Numeric.AreEqual(Math.Log(Math.E) / Math.Log(2), ConstantsD.Log2OfE));
      Assert.IsTrue(Numeric.AreEqual(1 / Math.PI, ConstantsD.OneOverPi));
      Assert.IsTrue(Numeric.AreEqual(Math.PI, ConstantsD.Pi));
      Assert.IsTrue(Numeric.AreEqual(Math.PI / 2, ConstantsD.PiOver2));
      Assert.IsTrue(Numeric.AreEqual(Math.PI / 4, ConstantsD.PiOver4));
      Assert.IsTrue(Numeric.AreEqual(Math.PI * 2, ConstantsD.TwoPi));
    }
  }
}
