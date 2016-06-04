using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class StepSegment2FTest
  {
    [Test]
    public void Test()
    {
      var s = new StepSegment2F
      {
        Point1 = new Vector2F(1, 2), 
        Point2 = new Vector2F(3, 4), 
        StepType = StepInterpolation.Centered
      };

      Assert.AreEqual(StepInterpolation.Centered, s.StepType);
      Assert.AreEqual(Vector2F.Zero, s.GetTangent(0.3f));
      Assert.AreEqual(0, s.GetLength(0, 1, 100, 0.0001f));
      Assert.AreEqual(new Vector2F(1, 2), s.GetPoint(0.3f));
      Assert.AreEqual(new Vector2F(3, 4), s.GetPoint(0.56f));
    }
  }
}
