using System;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class Path3FAnimationTest
  {
    [Test]
    public void CheckDefaultValues()
    {
      var animation = new Path3FAnimation();
      Assert.IsNull(animation.Path);
    }


    [Test]
    public void ShouldDoNothingWhenPathIsNull()
    {
      var animation = new Path3FAnimation();

      Vector3F defaultSource = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F defaultTarget = new Vector3F(10.0f, 20.0f, 30.0f);
      Assert.AreEqual(defaultSource, animation.GetValue(TimeSpan.FromSeconds(0.0), defaultSource, defaultTarget));
    }


    [Test]
    public void ShouldDoNothingWhenPathIsEmpty()
    {
      var animation = new Path3FAnimation();
      animation.Path = new Path3F();

      Vector3F defaultSource = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F defaultTarget = new Vector3F(10.0f, 20.0f, 30.0f);
      Assert.AreEqual(defaultSource, animation.GetValue(TimeSpan.FromSeconds(0.0), defaultSource, defaultTarget));
    }


    [Test]
    public void GetTotalDurationTest()
    {
      var animation = new Path3FAnimation();
      animation.Path = new Path3F
      {
        new PathKey3F { Parameter = 2.0f, Point = new Vector3F(2.0f, 22.0f, 10.0f), Interpolation = SplineInterpolation.Linear },
        new PathKey3F { Parameter = 3.0f, Point = new Vector3F(3.0f, 33.0f, 20.0f), Interpolation = SplineInterpolation.Linear },
        new PathKey3F { Parameter = 4.0f, Point = new Vector3F(4.0f, 44.0f, 30.0f), Interpolation = SplineInterpolation.Linear },
      };
      animation.Path.PreLoop = CurveLoopType.Linear;
      animation.Path.PostLoop = CurveLoopType.Cycle;
      animation.EndParameter = float.NaN;

      Assert.AreEqual(TimeSpan.FromSeconds(4.0), animation.GetTotalDuration());

      animation.EndParameter = float.PositiveInfinity;
      Assert.AreEqual(TimeSpan.MaxValue, animation.GetTotalDuration());
    }


    [Test]
    public void SimplePath()
    {
      var animation = new Path3FAnimation();
      animation.Path = new Path3F
      {
        new PathKey3F { Parameter = 2.0f, Point = new Vector3F(2.0f, 22.0f, 10.0f), Interpolation = SplineInterpolation.Linear },
        new PathKey3F { Parameter = 3.0f, Point = new Vector3F(3.0f, 33.0f, 20.0f), Interpolation = SplineInterpolation.Linear },
        new PathKey3F { Parameter = 4.0f, Point = new Vector3F(4.0f, 44.0f, 30.0f), Interpolation = SplineInterpolation.Linear },
      };
      animation.Path.PreLoop = CurveLoopType.Linear;
      animation.Path.PostLoop = CurveLoopType.Cycle;
      animation.EndParameter = float.PositiveInfinity;

      Vector3F defaultSource = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F defaultTarget = new Vector3F(10.0f, 20.0f, 30.0f);

      // Pre-Loop
      Assert.AreEqual(new Vector3F(0.0f, 0.0f, -10.0f), animation.GetValue(TimeSpan.FromSeconds(0.0), defaultSource, defaultTarget));
      Assert.AreEqual(new Vector3F(1.0f, 11.0f, 0.0f), animation.GetValue(TimeSpan.FromSeconds(1.0), defaultSource, defaultTarget));

      Assert.AreEqual(new Vector3F(2.0f, 22.0f, 10.0f), animation.GetValue(TimeSpan.FromSeconds(2.0), defaultSource, defaultTarget));
      Assert.AreEqual(new Vector3F(3.0f, 33.0f, 20.0f), animation.GetValue(TimeSpan.FromSeconds(3.0), defaultSource, defaultTarget));
      Assert.AreEqual(new Vector3F(4.0f, 44.0f, 30.0f), animation.GetValue(TimeSpan.FromSeconds(4.0), defaultSource, defaultTarget));

      // Post-Loop
      Assert.AreEqual(new Vector3F(3.0f, 33.0f, 20.0f), animation.GetValue(TimeSpan.FromSeconds(5.0), defaultSource, defaultTarget));
    }


    [Test]
    public void AdditivePath()
    {
      var animation = new Path3FAnimation();
      animation.Path = new Path3F
      {
        new PathKey3F { Parameter = 2.0f, Point = new Vector3F(2.0f, 22.0f, 10.0f), Interpolation = SplineInterpolation.Linear },
        new PathKey3F { Parameter = 3.0f, Point = new Vector3F(3.0f, 33.0f, 20.0f), Interpolation = SplineInterpolation.Linear },
        new PathKey3F { Parameter = 4.0f, Point = new Vector3F(4.0f, 44.0f, 30.0f), Interpolation = SplineInterpolation.Linear },
      };
      animation.Path.PreLoop = CurveLoopType.Linear;
      animation.Path.PostLoop = CurveLoopType.Cycle;
      animation.EndParameter = float.PositiveInfinity;
      animation.IsAdditive = true;

      Vector3F defaultSource = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F defaultTarget = new Vector3F(10.0f, 20.0f, 30.0f);

      // Pre-Loop
      Assert.AreEqual(defaultSource + new Vector3F(0.0f, 0.0f, -10.0f), animation.GetValue(TimeSpan.FromSeconds(0.0), defaultSource, defaultTarget));
      Assert.AreEqual(defaultSource + new Vector3F(1.0f, 11.0f, 0.0f), animation.GetValue(TimeSpan.FromSeconds(1.0), defaultSource, defaultTarget));

      Assert.AreEqual(defaultSource + new Vector3F(2.0f, 22.0f, 10.0f), animation.GetValue(TimeSpan.FromSeconds(2.0), defaultSource, defaultTarget));
      Assert.AreEqual(defaultSource + new Vector3F(3.0f, 33.0f, 20.0f), animation.GetValue(TimeSpan.FromSeconds(3.0), defaultSource, defaultTarget));
      Assert.AreEqual(defaultSource + new Vector3F(4.0f, 44.0f, 30.0f), animation.GetValue(TimeSpan.FromSeconds(4.0), defaultSource, defaultTarget));

      // Post-Loop
      Assert.AreEqual(defaultSource + new Vector3F(3.0f, 33.0f, 20.0f), animation.GetValue(TimeSpan.FromSeconds(5.0), defaultSource, defaultTarget));
    }
  }
}
