using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class PiecewiseCurveTest
  {
    private Path3F CreatePath()
    {
      Path3F path = new Path3F();
      path.Add(new PathKey3F()
      {
        Parameter = 10,
        Point = new Vector3F(1, 2, 3),
        Interpolation = SplineInterpolation.StepLeft,
        TangentIn = new Vector3F(1, 0, 0),
        TangentOut = new Vector3F(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 15,
        Point = new Vector3F(4, 5, 7),
        Interpolation = SplineInterpolation.StepCentered,
        TangentIn = new Vector3F(1, 0, 0),
        TangentOut = new Vector3F(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 18,
        Point = new Vector3F(5, 7, 10),
        Interpolation = SplineInterpolation.StepRight,
        TangentIn = new Vector3F(1, 0, 0),
        TangentOut = new Vector3F(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 20,
        Point = new Vector3F(5, 7, 13),
        Interpolation = SplineInterpolation.Linear,
        TangentIn = new Vector3F(1, 0, 0),
        TangentOut = new Vector3F(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 25,
        Point = new Vector3F(6, 7, 14),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector3F(5, 6, 13),
        TangentOut = new Vector3F(7, 8, 15),
      });      
      path.Add(new PathKey3F()
      {
        Parameter = 31,
        Point = new Vector3F(8, 10, 16),
        Interpolation = SplineInterpolation.BSpline,
        TangentIn = new Vector3F(1, 0, 0),
        TangentOut = new Vector3F(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 35,
        Point = new Vector3F(10, 12, 14),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector3F(1, 0, 0),
        TangentOut = new Vector3F(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 40,
        Point = new Vector3F(10, 14, 8),
        Interpolation = SplineInterpolation.CatmullRom,
        TangentIn = new Vector3F(1, 0, 0),
        TangentOut = new Vector3F(1, 0, 0),
      });
      return path;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void InsertNullKeyException()
    {
      Curve2F curve = new Curve2F();
      curve.Add(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ReplaceKeyWithNullException()
    {
      Curve2F curve = new Curve2F();
      curve.Add(new CurveKey2F());
      curve[0] = null;
    }


    [Test]
    public void GetKey()
    {
      Path3F empty = new Path3F();
      empty.Sort();
      Assert.AreEqual(-1, empty.GetKeyIndex(20));

      Path3F path = CreatePath();
      path.PreLoop = CurveLoopType.Constant;
      path.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(0, path.GetKeyIndex(-28));
      Assert.AreEqual(0, path.GetKeyIndex(3));
      Assert.AreEqual(0, path.GetKeyIndex(10));
      Assert.AreEqual(3, path.GetKeyIndex(20));
      Assert.AreEqual(4, path.GetKeyIndex(28));
      Assert.AreEqual(7, path.GetKeyIndex(40));
      Assert.AreEqual(6, path.GetKeyIndex(42));
      Assert.AreEqual(2, path.GetKeyIndex(78));

      path.PreLoop = CurveLoopType.Linear;
      path.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(-1, path.GetKeyIndex(-28));
      Assert.AreEqual(-1, path.GetKeyIndex(3));
      Assert.AreEqual(0, path.GetKeyIndex(10));
      Assert.AreEqual(3, path.GetKeyIndex(20));
      Assert.AreEqual(4, path.GetKeyIndex(28));
      Assert.AreEqual(7, path.GetKeyIndex(40));
      Assert.AreEqual(0, path.GetKeyIndex(42));
      Assert.AreEqual(2, path.GetKeyIndex(78));

      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(5, path.GetKeyIndex(-28));
      Assert.AreEqual(5, path.GetKeyIndex(3));
      Assert.AreEqual(0, path.GetKeyIndex(10));
      Assert.AreEqual(3, path.GetKeyIndex(20));
      Assert.AreEqual(4, path.GetKeyIndex(28));
      Assert.AreEqual(7, path.GetKeyIndex(40));
      Assert.AreEqual(0, path.GetKeyIndex(42));
      Assert.AreEqual(2, path.GetKeyIndex(78));

      path.PreLoop = CurveLoopType.CycleOffset;
      path.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(5, path.GetKeyIndex(-28));
      Assert.AreEqual(5, path.GetKeyIndex(3));
      Assert.AreEqual(0, path.GetKeyIndex(10));
      Assert.AreEqual(3, path.GetKeyIndex(20));
      Assert.AreEqual(4, path.GetKeyIndex(28));
      Assert.AreEqual(7, path.GetKeyIndex(40));
      Assert.AreEqual(7, path.GetKeyIndex(42));
      Assert.AreEqual(7, path.GetKeyIndex(78));

      path.PreLoop = CurveLoopType.Oscillate;
      path.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(5, path.GetKeyIndex(-28));
      Assert.AreEqual(1, path.GetKeyIndex(3));
      Assert.AreEqual(0, path.GetKeyIndex(10));
      Assert.AreEqual(3, path.GetKeyIndex(20));
      Assert.AreEqual(4, path.GetKeyIndex(28));
      Assert.AreEqual(7, path.GetKeyIndex(40));
      Assert.AreEqual(7, path.GetKeyIndex(42));
      Assert.AreEqual(7, path.GetKeyIndex(78));
    }


    [Test]
    public void LoopParameter()
    {
      Path3F empty = new Path3F();
      empty.Sort();
      Assert.AreEqual(3, empty.LoopParameter(3));
      Assert.AreEqual(false, empty.IsInMirroredOscillation(3));

      Path3F path = CreatePath();
      path.PreLoop = CurveLoopType.Constant;
      path.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(10, path.LoopParameter(3));
      Assert.AreEqual(false, path.IsInMirroredOscillation(3));
      Assert.AreEqual(10, path.LoopParameter(10));
      Assert.AreEqual(false, path.IsInMirroredOscillation(10));
      Assert.AreEqual(13, path.LoopParameter(13));
      Assert.AreEqual(false, path.IsInMirroredOscillation(13));
      Assert.AreEqual(40, path.LoopParameter(40));
      Assert.AreEqual(false, path.IsInMirroredOscillation(40));
      Assert.AreEqual(35, path.LoopParameter(45));
      Assert.AreEqual(true, path.IsInMirroredOscillation(45));
      Assert.AreEqual(14, path.LoopParameter(74));
      Assert.AreEqual(false, path.IsInMirroredOscillation(74));

      path.PreLoop = CurveLoopType.Linear;
      path.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(3, path.LoopParameter(3));
      Assert.AreEqual(false, path.IsInMirroredOscillation(3));
      Assert.AreEqual(10, path.LoopParameter(10));
      Assert.AreEqual(false, path.IsInMirroredOscillation(10));
      Assert.AreEqual(13, path.LoopParameter(13));
      Assert.AreEqual(false, path.IsInMirroredOscillation(13));
      Assert.AreEqual(40, path.LoopParameter(40));
      Assert.AreEqual(false, path.IsInMirroredOscillation(40));
      Assert.AreEqual(15, path.LoopParameter(45));
      Assert.AreEqual(false, path.IsInMirroredOscillation(45));
      Assert.AreEqual(14, path.LoopParameter(74));
      Assert.AreEqual(false, path.IsInMirroredOscillation(74));

      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(20, path.LoopParameter(-40));
      Assert.AreEqual(false, path.IsInMirroredOscillation(-40));
      Assert.AreEqual(33, path.LoopParameter(3));
      Assert.AreEqual(false, path.IsInMirroredOscillation(3));
      Assert.AreEqual(10, path.LoopParameter(10));
      Assert.AreEqual(false, path.IsInMirroredOscillation(10));
      Assert.AreEqual(13, path.LoopParameter(13));
      Assert.AreEqual(false, path.IsInMirroredOscillation(13));
      Assert.AreEqual(40, path.LoopParameter(40));
      Assert.AreEqual(false, path.IsInMirroredOscillation(40));
      Assert.AreEqual(15, path.LoopParameter(45));
      Assert.AreEqual(false, path.IsInMirroredOscillation(45));
      Assert.AreEqual(14, path.LoopParameter(74));
      Assert.AreEqual(false, path.IsInMirroredOscillation(74));
      Assert.AreEqual(20, path.LoopParameter(200));
      Assert.AreEqual(false, path.IsInMirroredOscillation(200));

      path.PreLoop = CurveLoopType.CycleOffset;
      path.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(20, path.LoopParameter(-40));
      Assert.AreEqual(false, path.IsInMirroredOscillation(-40));
      Assert.AreEqual(33, path.LoopParameter(3));
      Assert.AreEqual(false, path.IsInMirroredOscillation(3));
      Assert.AreEqual(10, path.LoopParameter(10));
      Assert.AreEqual(false, path.IsInMirroredOscillation(10));
      Assert.AreEqual(13, path.LoopParameter(13));
      Assert.AreEqual(false, path.IsInMirroredOscillation(13));
      Assert.AreEqual(40, path.LoopParameter(40));
      Assert.AreEqual(false, path.IsInMirroredOscillation(40));
      Assert.AreEqual(45, path.LoopParameter(45));
      Assert.AreEqual(false, path.IsInMirroredOscillation(45));
      Assert.AreEqual(74, path.LoopParameter(74));
      Assert.AreEqual(false, path.IsInMirroredOscillation(74));
      Assert.AreEqual(200, path.LoopParameter(200));
      Assert.AreEqual(false, path.IsInMirroredOscillation(200));

      path.PreLoop = CurveLoopType.Oscillate;
      path.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(20, path.LoopParameter(-40));
      Assert.AreEqual(false, path.IsInMirroredOscillation(-40));
      Assert.AreEqual(17, path.LoopParameter(3));
      Assert.AreEqual(true, path.IsInMirroredOscillation(3));
      Assert.AreEqual(10, path.LoopParameter(10));
      Assert.AreEqual(false, path.IsInMirroredOscillation(10));
      Assert.AreEqual(13, path.LoopParameter(13));
      Assert.AreEqual(false, path.IsInMirroredOscillation(13));
      Assert.AreEqual(40, path.LoopParameter(40));
      Assert.AreEqual(false, path.IsInMirroredOscillation(40));
      Assert.AreEqual(40, path.LoopParameter(45));
      Assert.AreEqual(false, path.IsInMirroredOscillation(45));
      Assert.AreEqual(40, path.LoopParameter(74));
      Assert.AreEqual(false, path.IsInMirroredOscillation(74));
      Assert.AreEqual(40, path.LoopParameter(200));
      Assert.AreEqual(false, path.IsInMirroredOscillation(200));
    }


    [Test]
    public void Sort()
    {
      Path3F empty = new Path3F();
      empty.Sort();

      Path3F path = CreatePath();

      // Un-sort the keys.
      path[7].Parameter = 13;

      path.Sort();
      Assert.AreEqual(8, path.Count);
      Assert.AreEqual(10, path[0].Parameter);
      Assert.AreEqual(13, path[1].Parameter);
      Assert.AreEqual(15, path[2].Parameter);
      // ...
      Assert.AreEqual(35, path[7].Parameter);
    }
  }
}
