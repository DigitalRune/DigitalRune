using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  internal static class MathAssert
  {
    public static void AreEqual<TParam, TPoint>(CurveKey<TParam, TPoint> expected, CurveKey<TParam, TPoint> curveKey)
    {
      Assert.AreEqual(expected.Interpolation, curveKey.Interpolation);
      Assert.AreEqual(expected.Parameter, curveKey.Parameter);
      Assert.AreEqual(expected.Point, curveKey.Point);
      Assert.AreEqual(expected.TangentIn, curveKey.TangentIn);
      Assert.AreEqual(expected.TangentOut, curveKey.TangentOut);
    }
  }
}
