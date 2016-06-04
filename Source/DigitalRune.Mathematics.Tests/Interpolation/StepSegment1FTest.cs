using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class StepSegment1FTest
  {
    [Test]
    public void Test()
    {
      var s = new StepSegment1F
      {
        Point1 = 3, 
        Point2 = 4, 
        StepType = StepInterpolation.Centered
      };

      Assert.AreEqual(StepInterpolation.Centered, s.StepType);
      Assert.AreEqual(0, s.GetTangent(0.3f));
      Assert.AreEqual(0, s.GetLength(0, 1, 100, 0.0001f));
      Assert.AreEqual(3, s.GetPoint(0.3f));
      Assert.AreEqual(4, s.GetPoint(0.56f));
    }
  }
}
