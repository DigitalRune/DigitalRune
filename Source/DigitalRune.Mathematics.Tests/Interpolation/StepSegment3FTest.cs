using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class StepSegment3FTest
  {
    [Test]
    public void Test()
    {
      var s = new StepSegment3F
      {
        Point1 = new Vector3F(1, 2, 3),
        Point2 = new Vector3F(3, 4, 5),
        StepType = StepInterpolation.Centered
      };

      Assert.AreEqual(StepInterpolation.Centered, s.StepType);
      Assert.AreEqual(Vector3F.Zero, s.GetTangent(0.3f));
      Assert.AreEqual(0, s.GetLength(0, 1, 100, 0.0001f));
      Assert.AreEqual(new Vector3F(1, 2, 3), s.GetPoint(0.3f));
      Assert.AreEqual(new Vector3F(3, 4, 5), s.GetPoint(0.56f));
    }
  }
}
