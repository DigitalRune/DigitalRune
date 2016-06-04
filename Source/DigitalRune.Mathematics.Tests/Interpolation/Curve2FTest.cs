using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class Curve2FTest
  {
    private Curve2F CreateCurve()
    {
      Curve2F curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(10, 1),
        Interpolation = SplineInterpolation.Linear,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(15, 3),
        Interpolation = SplineInterpolation.StepLeft,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(22, 5),
        Interpolation = SplineInterpolation.StepCentered,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(25, 4),
        Interpolation = SplineInterpolation.StepRight,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(30, 7),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector2F(32, 5),
        TangentOut = new Vector2F(33, 10),
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(34, 10),
        Interpolation = SplineInterpolation.BSpline,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(40, 3),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector2F(1, 0),
        TangentOut = new Vector2F(1, 1),
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(45, 10),
        Interpolation = SplineInterpolation.CatmullRom,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(48, 5),
        Interpolation = SplineInterpolation.CatmullRom,
      });

      curve.Sort();
      return curve;
    }


    [Test]
    public void GetPointShouldReturnNanIfCurveIsEmpty()
    {
      Curve2F empty = new Curve2F();
      empty.Sort();

      Vector2F p = empty.GetPoint(-0.5f);
      Assert.IsNaN(p.X);
      Assert.IsNaN(p.Y);

      p = empty.GetPoint(0);
      Assert.IsNaN(p.X);
      Assert.IsNaN(p.Y);

      p = empty.GetPoint(0.5f);
      Assert.IsNaN(p.X);
      Assert.IsNaN(p.Y);
    }


    [Test]
    public void GetPoint()
    {
      Curve2F empty = new Curve2F();
      Assert.IsTrue(float.IsNaN(empty.GetPoint(1).X));
      Assert.IsTrue(float.IsNaN(empty.GetPoint(0).Y));

      Curve2F curve = CreateCurve();
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(-10, 1), curve.GetPoint(-10)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(11, 1 + 2f/5f), curve.GetPoint(11)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(19, 5), curve.GetPoint(19)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(23, 5), curve.GetPoint(23)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(24, 4), curve.GetPoint(24)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(29, 4), curve.GetPoint(29)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(30, 7), curve.GetPoint(30)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(33.9999f, 10), curve.GetPoint(33.9999f)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(40, 3), curve.GetPoint(40)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(45, 10), curve.GetPoint(45)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(48, 5), curve.GetPoint(48)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(49, curve.GetPoint(47).Y), curve.GetPoint(49)));

      // Tested with internal assert in Debug Build.
      curve.GetPoint(31);
      curve.GetPoint(33);
      curve.GetPoint(35);
      curve.GetPoint(36);
      curve.GetPoint(39);
      curve.GetPoint(41);
      curve.GetPoint(46);
      curve.GetPoint(47);

      //CatmullRomSegment3F catmullOscillate = new CatmullRomSegment3F()
      //{
      //  Point1 = new Vector3F(10, 12, 14),
      //  Point2 = new Vector3F(10, 14, 8),
      //  Point3 = new Vector3F(20, 14, 8),
      //  Point4 = new Vector3F(20, 14, 8),
      //};
      //Assert.IsTrue(Vector3F.AreNumericallyEqual(catmullOscillate.GetPoint(0.3f), curve.GetPoint(43)));
      //Assert.IsTrue(Vector3F.AreNumericallyEqual(catmullOscillate.GetPoint(0.9f), curve.GetPoint(51)));

      //CatmullRomSegment3F catmullCircle = new CatmullRomSegment3F()
      //{
      //  Point1 = new Vector3F(10, 12, 14),
      //  Point2 = new Vector3F(10, 14, 8),
      //  Point3 = new Vector3F(20, 14, 8),
      //  Point4 = new Vector3F(0, 0, 1),
      //};
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(-10, -7), curve.GetPoint(-10)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(49, 1 + 2f/5f), curve.GetPoint(49)));

      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(4, curve.GetPoint(42).Y), curve.GetPoint(4)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(50, 5 + 4f/5f), curve.GetPoint(50)));

      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(4, curve.GetPoint(42).Y - 4), curve.GetPoint(4f)));
      CatmullRomSegment3F catmull = new CatmullRomSegment3F 
      { 
        Point1 = new Vector3F(40, 3, 0),
        Point2 = new Vector3F(45, 10, 0),
        Point3 = new Vector3F(48, 5, 0),
        Point4 = new Vector3F(48, 5, 0),
      };
      Vector3F endTangent = catmull.GetTangent(1);
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(55f, 5 + (55-48) * endTangent.Y / endTangent.X), curve.GetPoint(55f)));
      //Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector3F(20, 14, 8) + catmullOscillate.GetTangent(1) / 10 * 50, curve.GetPoint(100f)));

      // Test more linear pre- and post-behavior.
      curve = new Curve2F();
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(0, 0),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector2F(-10, 10),
        TangentOut = new Vector2F(5, 4),
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(10, 3),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector2F(8, 2),
        TangentOut = new Vector2F(15, 4),
      });
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(-10, 10), curve.GetPoint(-10f)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(15, 4), curve.GetPoint(15f)));

      curve = new Curve2F();
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(0, 0),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector2F(1, 2),
        TangentOut = new Vector2F(5, 4),
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(10, 3),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector2F(8, 2),
        TangentOut = new Vector2F(2, -1),
      });
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(-10, -20), curve.GetPoint(-10f)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(15, 3 - 0.5f * 5), curve.GetPoint(15f)));
    }


    [Test]
    public void OneKeyCurvesTest()
    {
      // Test curves with 1 point
      Curve2F curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.Linear,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(1, 2) , curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));

      // Test curves with 1 point
      curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.StepLeft,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));

      // Test curves with 1 point
      curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.BSpline,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));

      // Test curves with 1 point
      curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.CatmullRom,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));

      // Test curves with 1 point
      curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector2F(2, -2),
        TangentOut = new Vector2F(2, 2),
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 3), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(2, -2), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(2, -2), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 3), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(2, -2), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));

      // Test curves with 1 point
      curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector2F(1, 2) - new Vector2F(2, -2) / 3.0f,
        TangentOut = new Vector2F(1, 2) + new Vector2F(2, 2) / 3.0f,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(2, 2), curve.GetTangent(1)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(2, 2), curve.GetTangent(2)));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 3), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(2, -2), curve.GetTangent(0)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(2, -2), curve.GetTangent(1)));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 3), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(2, -2), curve.GetTangent(0)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(2, 2), curve.GetTangent(1)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(2, 2), curve.GetTangent(2)));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2F(1, 0), curve.GetTangent(2));
    }


    [Test]
    public void TwoKeyCurvesTest()
    {
      Curve2F curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.Linear,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(3, 4),
        Interpolation = SplineInterpolation.Linear,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(0, 1), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 5), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2F(0, 3), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 3), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2F(0, 1), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 5), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2F(0, 3), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 3), curve.GetPoint(4));

      // Test curves with 1 point
      curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.StepRight,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(3, 4),
        Interpolation = SplineInterpolation.StepRight,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 2), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2F(0, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 2), curve.GetPoint(4));

      curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.BSpline,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(3, 4),
        Interpolation = SplineInterpolation.BSpline,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(0, 1), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 5), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2F(0, 3), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 3), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2F(0, 1), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 5), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2F(0, 3), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 3), curve.GetPoint(4));

      curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.CatmullRom,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(3, 4),
        Interpolation = SplineInterpolation.CatmullRom,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(0, 1), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 5), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2F(0, 3), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 3), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2F(0, 1), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 5), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2F(0, 3), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 3), curve.GetPoint(4));

      curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector2F(2, 0),
        TangentOut = new Vector2F(2, -1.2f),
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(3, 4),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector2F(2, 0),
        TangentOut = new Vector2F(2, -2),
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 3), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(curve.GetPoint(2).Y, curve.GetPoint(0).Y);
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(curve.GetPoint(2).Y, curve.GetPoint(4).Y);
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.IsTrue(Numeric.AreEqual(curve.GetPoint(2).Y - 2, curve.GetPoint(0).Y));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.IsTrue(Numeric.AreEqual(curve.GetPoint(2).Y + 2, curve.GetPoint(4).Y));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(curve.GetPoint(2).Y, curve.GetPoint(0).Y);
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(curve.GetPoint(2).Y, curve.GetPoint(4).Y);

      curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector2F(1, 2) + new Vector2F(2, 0) / 3,
        TangentOut = new Vector2F(1, 2) + new Vector2F(2, -1.2f) / 3,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(3, 4),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector2F(3, 4) + new Vector2F(2, 0) / 3,
        TangentOut = new Vector2F(3, 4) + new Vector2F(2, -2) / 3,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 3), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(curve.GetPoint(2).Y, curve.GetPoint(0).Y);
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(curve.GetPoint(2).Y, curve.GetPoint(4).Y);
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.IsTrue(Numeric.AreEqual(curve.GetPoint(2).Y - 2, curve.GetPoint(0).Y));
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.IsTrue(Numeric.AreEqual(curve.GetPoint(2).Y + 2, curve.GetPoint(4).Y));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(curve.GetPoint(2).Y, curve.GetPoint(0).Y);
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(curve.GetPoint(2).Y, curve.GetPoint(4).Y);
    }


    [Test]
    public void ZeroLengthSplineTest()
    {
      Curve2F curve = new Curve2F();
      curve.Add(new CurveKey2F()
                  {
                    Point = new Vector2F(1, 2),
                    Interpolation = SplineInterpolation.CatmullRom,
                  });
      curve.Add(new CurveKey2F()
                  {
                    Point = new Vector2F(1, 4),
                    Interpolation = SplineInterpolation.CatmullRom,
                  });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.IsTrue(new Vector2F(0, 2) == curve.GetPoint(0) || new Vector2F(0, 4) == curve.GetPoint(0)); // Degenerate case. Any useful result is ok.
      Assert.IsTrue(new Vector2F(1, 2) == curve.GetPoint(0) || new Vector2F(1, 4) == curve.GetPoint(1)); // Degenerate case. Any useful result is ok.
      Assert.IsTrue(new Vector2F(2, 2) == curve.GetPoint(0) || new Vector2F(2, 4) == curve.GetPoint(2)); // Degenerate case. Any useful result is ok.
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2F(0, 2), curve.GetPoint(0));
      Assert.IsTrue(new Vector2F(1, 2) == curve.GetPoint(0) || new Vector2F(1, 4) == curve.GetPoint(1)); // Degenerate case. Any useful result is ok.
      Assert.IsTrue(new Vector2F(2, 2) == curve.GetPoint(0) || new Vector2F(2, 4) == curve.GetPoint(2)); // Degenerate case. Any useful result is ok.
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.IsTrue(new Vector2F(0, 2) == curve.GetPoint(0) || new Vector2F(0, 4) == curve.GetPoint(0)); // Degenerate case. Any useful result is ok.
      Assert.IsTrue(new Vector2F(1, 2) == curve.GetPoint(0) || new Vector2F(1, 4) == curve.GetPoint(1)); // Degenerate case. Any useful result is ok.
      Assert.IsTrue(new Vector2F(2, 2) == curve.GetPoint(0) || new Vector2F(2, 4) == curve.GetPoint(2)); // Degenerate case. Any useful result is ok.
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.IsTrue(new Vector2F(0, 2) == curve.GetPoint(0) || new Vector2F(0, 4) == curve.GetPoint(0)); // Degenerate case. Any useful result is ok.
      Assert.IsTrue(new Vector2F(1, 2) == curve.GetPoint(0) || new Vector2F(1, 4) == curve.GetPoint(1)); // Degenerate case. Any useful result is ok.
      Assert.IsTrue(new Vector2F(2, 2) == curve.GetPoint(0) || new Vector2F(2, 4) == curve.GetPoint(2)); // Degenerate case. Any useful result is ok.
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.IsTrue(new Vector2F(0, 2) == curve.GetPoint(0) || new Vector2F(0, 4) == curve.GetPoint(0)); // Degenerate case. Any useful result is ok.
      Assert.IsTrue(new Vector2F(1, 2) == curve.GetPoint(0) || new Vector2F(1, 4) == curve.GetPoint(1)); // Degenerate case. Any useful result is ok.
      Assert.IsTrue(new Vector2F(2, 2) == curve.GetPoint(0) || new Vector2F(2, 4) == curve.GetPoint(2)); // Degenerate case. Any useful result is ok.
      Assert.AreEqual(new Vector2F(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2F(4, 4), curve.GetPoint(4));
      Assert.AreEqual(false, curve.IsInMirroredOscillation(0));
    }

    [Test]
    public void GetTangent()
    {
      Curve2F curve = CreateCurve();
      curve.GetTangent(0);
    }


    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void GetLength()
    {
      Curve2F curve = CreateCurve();
      curve.GetLength(0, 1, 10, 0.1f);
    }


    [Test]
    public void BSplineTest()
    {
      // BSpline are difficult because the spline does not start/end at the given x parameter values
      // if the x distances of the keys are different.
      Curve2F curve = new Curve2F();
      curve.Add(new CurveKey2F()
                  {
                    Point = new Vector2F(1, 2),
                    Interpolation = SplineInterpolation.BSpline,
                  });
      curve.Add(new CurveKey2F()
                  {
                    Point = new Vector2F(2, 4),
                    Interpolation = SplineInterpolation.BSpline,
                  });
      curve.Add(new CurveKey2F()
                  {
                    Point = new Vector2F(10, 20),
                    Interpolation = SplineInterpolation.BSpline,
                  });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(1.1f, 2.2f), curve.GetPoint(1.1f)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(1.9f, 3.8f), curve.GetPoint(1.9f)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(2, 4), curve.GetPoint(2)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(2.1f, 4.2f), curve.GetPoint(2.1f)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(9.9f, 19.8f), curve.GetPoint(9.9f)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(10, 20), curve.GetPoint(10)));

      curve = new Curve2F();
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(1, 2),
        Interpolation = SplineInterpolation.BSpline,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(9, 18),
        Interpolation = SplineInterpolation.BSpline,
      });
      curve.Add(new CurveKey2F()
      {
        Point = new Vector2F(10, 20),
        Interpolation = SplineInterpolation.BSpline,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2F(1, 2), curve.GetPoint(1));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(1.1f, 2.2f), curve.GetPoint(1.1f)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(8.9f, 17.8f), curve.GetPoint(8.9f)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(9, 18), curve.GetPoint(9)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(9.1f, 18.2f), curve.GetPoint(9.1f)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(9.9f, 19.8f), curve.GetPoint(9.9f)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F(10, 20), curve.GetPoint(10)));
    }


    [Test]
    public void SerializationXml()
    {
      CurveKey2F curveKey1 = new CurveKey2F
      {
        Interpolation = SplineInterpolation.Bezier,
        Point = new Vector2F(1.2f, 3.4f),
        TangentIn = new Vector2F(0.7f, 2.6f),
        TangentOut = new Vector2F(1.9f, 3.3f)
      };
      CurveKey2F curveKey2 = new CurveKey2F
      {
        Interpolation = SplineInterpolation.Hermite,
        Point = new Vector2F(2.2f, 4.4f),
        TangentIn = new Vector2F(1.7f, 3.6f),
        TangentOut = new Vector2F(2.9f, 4.3f)
      };
      Curve2F curve = new Curve2F { curveKey1, curveKey2 };
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.CycleOffset;
      curve.SmoothEnds = true;

      const string fileName = "SerializationCurve2F.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Curve2F));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, curve);
      writer.Close();

      serializer = new XmlSerializer(typeof(Curve2F));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      curve = (Curve2F)serializer.Deserialize(fileStream);
      Assert.AreEqual(2, curve.Count);
      MathAssert.AreEqual(curveKey1, curve[0]);
      MathAssert.AreEqual(curveKey2, curve[1]);
      Assert.AreEqual(CurveLoopType.Cycle, curve.PreLoop);
      Assert.AreEqual(CurveLoopType.CycleOffset, curve.PostLoop);
      Assert.AreEqual(true, curve.SmoothEnds);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      CurveKey2F curveKey1 = new CurveKey2F
      {
        Interpolation = SplineInterpolation.Bezier,
        Point = new Vector2F(1.2f, 3.4f),
        TangentIn = new Vector2F(0.7f, 2.6f),
        TangentOut = new Vector2F(1.9f, 3.3f)
      };
      CurveKey2F curveKey2 = new CurveKey2F
      {
        Interpolation = SplineInterpolation.Hermite,
        Point = new Vector2F(2.2f, 4.4f),
        TangentIn = new Vector2F(1.7f, 3.6f),
        TangentOut = new Vector2F(2.9f, 4.3f)
      };
      Curve2F curve = new Curve2F { curveKey1, curveKey2 };
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.CycleOffset;
      curve.SmoothEnds = true;

      const string fileName = "SerializationCurve2F.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, curve);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      curve = (Curve2F)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(2, curve.Count);
      MathAssert.AreEqual(curveKey1, curve[0]);
      MathAssert.AreEqual(curveKey2, curve[1]);
      Assert.AreEqual(CurveLoopType.Cycle, curve.PreLoop);
      Assert.AreEqual(CurveLoopType.CycleOffset, curve.PostLoop);
      Assert.AreEqual(true, curve.SmoothEnds);
    }
  }
}
