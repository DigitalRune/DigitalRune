using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class LineAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComputeCollisionException()
    {
      LineAlgorithm algo = new LineAlgorithm(new CollisionDetection());
      algo.GetClosestPoints(new CollisionObject(), new CollisionObject()); // no line!
    }


    [Test]
    public void ComputeCollisionLineLine()
    {
      CollisionObject line0 = new CollisionObject(new GeometricObject
      {
        Shape = new LineShape(new Vector3F(1, 2, 3), new Vector3F(1, 0, 0)),
      });
      //line0.Name = "line0";

      CollisionObject line1 = new CollisionObject(new GeometricObject
      {
        Shape = new LineShape(new Vector3F(0, 0, 1), new Vector3F(1, 0, 0)),
      });
      //line1.Name = "line1";

      LineAlgorithm algo = new LineAlgorithm(new CollisionDetection());

      ContactSet set;

      ((GeometricObject)line1.GeometricObject).Pose = new Pose(new Vector3F(1, 2, 2), line1.GeometricObject.Pose.Orientation);
      set = algo.GetClosestPoints(line0, line1);      
      Assert.AreEqual(0, set[0].PenetrationDepth);
      Assert.AreEqual(Vector3F.UnitY, set[0].Normal);
      Assert.AreEqual(true, algo.HaveContact(line0, line1));

      ((GeometricObject)line1.GeometricObject).Pose = new Pose(line1.GeometricObject.Pose.Position, QuaternionF.CreateRotationZ(ConstantsF.PiOver2));
      set = algo.GetClosestPoints(line0, line1);
      Assert.IsTrue(Numeric.AreEqual(0, set[0].PenetrationDepth));
      Assert.AreEqual(new Vector3F(1, 2, 3), set[0].Position);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(Vector3F.UnitZ, set[0].Normal));
      Assert.AreEqual(true, algo.HaveContact(line0, line1));

      ((GeometricObject)line1.GeometricObject).Pose = new Pose(new Vector3F(1, 2, 4), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));
      set = algo.GetClosestPoints(line1, line0);
      Assert.IsTrue(Numeric.AreEqual(-2, set[0].PenetrationDepth));
      Assert.AreEqual(new Vector3F(1, 2, 4), set[0].Position);
      Assert.AreEqual(-Vector3F.UnitZ, set[0].Normal);
      Assert.AreEqual(false, algo.HaveContact(line0, line1));

      algo.UpdateContacts(set, 0.01f);
      Assert.AreEqual(0, set.Count);
    }


    [Test]
    public void ComputeCollisionLineOther()
    {
      CollisionObject line0 = new CollisionObject();
      //line0.Name = "line0";
      ((GeometricObject)line0.GeometricObject).Shape = new LineShape(new Vector3F(0, 0, 1), new Vector3F(1, 0, 0));
      ((GeometricObject)line0.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 2));

      CollisionObject sphere = new CollisionObject();
      //sphere.Name = "sphere";
      ((GeometricObject)sphere.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)sphere.GeometricObject).Pose = Pose.Identity;

      LineAlgorithm algo = new LineAlgorithm(new CollisionDetection());

      ContactSet set;

      set = algo.GetClosestPoints(line0, sphere);
      Assert.IsTrue(Numeric.AreEqual(-2, set[0].PenetrationDepth));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(-Vector3F.UnitZ, set[0].Normal));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 2), set[0].Position, 0.001f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 1), set[0].PositionALocal, 0.001f));
      Assert.AreEqual(false, algo.HaveContact(line0, sphere));

      set = set.Swapped;
      ((GeometricObject)sphere.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 2.1f));
      algo.UpdateContacts(set, 0);
      Assert.IsTrue(Numeric.AreEqual(0.1f, set[0].PenetrationDepth, 0.001f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(Vector3F.UnitZ, set[0].Normal, 0.1f));   // Large epsilon because MPR for spheres is not very accurate.
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 3), set[0].Position, 0.1f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 1), set[0].PositionALocal, 0.1f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 1), set[0].PositionBLocal, 0.1f));
      Assert.AreEqual(true, algo.HaveContact(line0, sphere));
    }


    [Test]
    public void TestLineSegmentToLineSegment1()
    {
      LineSegment s0 = new LineSegment(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0));
      LineSegment s1 = new LineSegment(new Vector3F(0, 1, 0), new Vector3F(1, 1, 0));

      Vector3F p0, p1;

      GeometryHelper.GetClosestPoints(s0, s1, out p0, out p1);
      Assert.AreEqual(new Vector3F(0, 0, 0), p0);
      Assert.AreEqual(new Vector3F(0, 1, 0), p1);
    }


    [Test]
    public void GetClosestPointsPointSegment()
    {
      LineSegment s0 = new LineSegment(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0));

      Vector3F p;
      bool haveContact = GeometryHelper.GetClosestPoints(s0, new Vector3F(1, 1, 0), out p);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(1, 0, 0), p);

      haveContact = GeometryHelper.GetClosestPoints(s0, new Vector3F(0, 0, 0), out p);
      Assert.AreEqual(true, haveContact);
      Assert.AreEqual(new Vector3F(0, 0, 0), p);

      haveContact = GeometryHelper.GetClosestPoints(s0, new Vector3F(0.5f, 1, 0), out p);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(0.5f, 0, 0), p);

      // zero length segment
      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(3, 4, 5), new Vector3F(3, 4, 5)),
                                                 new Vector3F(0.5f, 1, 0), out p);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(3, 4, 5), p);
    }


    [Test]
    public void GetClosestPointsPointLine()
    {
      Line line = new Line(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0));

      Vector3F p;
      bool haveContact = GeometryHelper.GetClosestPoint(line, new Vector3F(1, 1, 0), out p);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(1, 0, 0), p);

      haveContact = GeometryHelper.GetClosestPoint(line, new Vector3F(0, 0, 0), out p);
      Assert.AreEqual(true, haveContact);
      Assert.AreEqual(new Vector3F(0, 0, 0), p);

      haveContact = GeometryHelper.GetClosestPoint(line, new Vector3F(0.5f, 1, 0), out p);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(0.5f, 0, 0), p);
    }


    [Test]
    public void GetClosestPointsLineLine()
    {
      Vector3F p0, p1;
      bool haveContact;

      haveContact = GeometryHelper.GetClosestPoints(new Line(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0)),
                                                 new Line(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(true, haveContact);
      Assert.AreEqual(0, p0.X);
      Assert.AreEqual(0, p0.Z);
      Assert.AreEqual(p0.Y, p1.Y);
      Assert.AreEqual(0, p1.X);
      Assert.AreEqual(0, p1.Z);

      haveContact = GeometryHelper.GetClosestPoints(new Line(new Vector3F(2, 0, 0), new Vector3F(0, 1, 0)),
                                                 new Line(new Vector3F(2, 0, 0), new Vector3F(1, 0, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(true, haveContact);
      Assert.AreEqual(new Vector3F(2, 0, 0), p0);
      Assert.AreEqual(new Vector3F(2, 0, 0), p1);

      haveContact = GeometryHelper.GetClosestPoints(new Line(new Vector3F(2, 0, 1), new Vector3F(0, 1, 0)),
                                                 new Line(new Vector3F(2, 0, 0), new Vector3F(1, 0, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(2, 0, 1), p0);
      Assert.AreEqual(new Vector3F(2, 0, 0), p1);
    }


    [Test]
    public void GetClosestPointsSegmentSegment()
    {
      Vector3F p0, p1;
      bool haveContact;

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0)),
                                                 new LineSegment(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(true, haveContact);
      Assert.AreEqual(0, p0.X);
      Assert.AreEqual(0, p0.Z);
      Assert.AreEqual(p0.Y, p1.Y);
      Assert.AreEqual(0, p1.X);
      Assert.AreEqual(0, p1.Z);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(2, 0, 0), new Vector3F(2, 1, 0)),
                                                 new LineSegment(new Vector3F(2, 0, 0), new Vector3F(3, 0, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(true, haveContact);
      Assert.AreEqual(new Vector3F(2, 0, 0), p0);
      Assert.AreEqual(new Vector3F(2, 0, 0), p1);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(2, 0, 1), new Vector3F(2, 1, 1)),
                                                 new LineSegment(new Vector3F(2, 0, 0), new Vector3F(1, 0, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(2, 0, 1), p0);
      Assert.AreEqual(new Vector3F(2, 0, 0), p1);

      // Segment0 has zero length.
      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(2, 0, 1), new Vector3F(2, 0, 1)),
                                                 new LineSegment(new Vector3F(2, 0, 0), new Vector3F(1, 0, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(2, 0, 1), p0);
      Assert.AreEqual(new Vector3F(2, 0, 0), p1);

      // Segment0 has zero length.
      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(2, 2, 1), new Vector3F(2, 0, 1)),
                                                 new LineSegment(new Vector3F(1, 0, 0), new Vector3F(1, 0, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(2, 0, 1), p0);
      Assert.AreEqual(new Vector3F(1, 0, 0), p1);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(0, 0, 0), new Vector3F(10, 0, 0)),
                                                 new LineSegment(new Vector3F(1, 2, 0), new Vector3F(5, 4, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(1, 0, 0), p0);
      Assert.AreEqual(new Vector3F(1, 2, 0), p1);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(0, 0, 0), new Vector3F(10, 0, 0)),
                                                 new LineSegment(new Vector3F(1, 4, 0), new Vector3F(5, 2, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(5, 0, 0), p0);
      Assert.AreEqual(new Vector3F(5, 2, 0), p1);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(0, 0, 0), new Vector3F(10, 0, 0)),
                                                 new LineSegment(new Vector3F(-1, 2, 0), new Vector3F(9, 3, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(0, 0, 0), p0);
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(p1 - p0, new Vector3F(-1, 2, 0) - new Vector3F(9, 3, 0)))); // shortest distance is normal segment 2.

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(0, 0, 0), new Vector3F(10, 0, 0)),
                                                 new LineSegment(new Vector3F(-1, 3, 0), new Vector3F(11, 2, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(10, 0, 0), p0);
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(p1 - p0, new Vector3F(-1, 3, 0) - new Vector3F(11, 2, 0)))); // shortest distance is normal segment 2.


      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(0, 0, 0), new Vector3F(10, 0, 0)),
                                                 new LineSegment(new Vector3F(5, 4, 0), new Vector3F(5, 2, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(5, 0, 0), p0);
      Assert.AreEqual(new Vector3F(5, 2, 0), p1);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(0, 0, 0), new Vector3F(10, 0, 0)),
                                                 new LineSegment(new Vector3F(5, 2, 0), new Vector3F(5, 4, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(5, 0, 0), p0);
      Assert.AreEqual(new Vector3F(5, 2, 0), p1);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(5, 4, 0), new Vector3F(5, 2, 0)),
                                                 new LineSegment(new Vector3F(0, 0, 0), new Vector3F(10, 0, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(5, 0, 0), p1);
      Assert.AreEqual(new Vector3F(5, 2, 0), p0);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(5, 2, 0), new Vector3F(5, 3, 0)),
                                                 new LineSegment(new Vector3F(0, 0, 0), new Vector3F(10, 0, 0)),
                                                 out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(5, 0, 0), p1);
      Assert.AreEqual(new Vector3F(5, 2, 0), p0);
    }


    [Test]
    public void GetClosestPointsLineSegment()
    {
      Vector3F p0, p1;
      bool haveContact;

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0)),
                                                 new Line(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0)), out p0, out p1);
      Assert.AreEqual(true, haveContact);
      Assert.AreEqual(0, p0.X);
      Assert.AreEqual(0, p0.Z);
      Assert.AreEqual(p0.Y, p1.Y);
      Assert.AreEqual(0, p1.X);
      Assert.AreEqual(0, p1.Z);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(2, 0, 0), new Vector3F(3, 0, 0)),
                                                 new Line(new Vector3F(2, 0, 0), new Vector3F(0, 1, 0)), out p0, out p1);
      Assert.AreEqual(true, haveContact);
      Assert.AreEqual(new Vector3F(2, 0, 0), p0);
      Assert.AreEqual(new Vector3F(2, 0, 0), p1);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(2, 0, 0), new Vector3F(1, 0, 0)),
                                                 new Line(new Vector3F(2, 0, 1), new Vector3F(0, 1, 0)), out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(2, 0, 1), p0);
      Assert.AreEqual(new Vector3F(2, 0, 0), p1);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(2, 0, 0), new Vector3F(1, 0, 0)),
                                                 new Line(new Vector3F(2, 0, 1), new Vector3F(0, 1, 0)), out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(2, 0, 1), p0);
      Assert.AreEqual(new Vector3F(2, 0, 0), p1);


      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(-2, 2, 0), new Vector3F(-1, 2, 0)),
                                                 new Line(new Vector3F(0, 0, 0), new Vector3F(0, 0, 1)), out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(0, 0, 0), p0);
      Assert.AreEqual(new Vector3F(-1, 2, 0), p1);

      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(2, 2, 0), new Vector3F(3, 2.5f, 0)),
                                                 new Line(new Vector3F(0, 0, 0), new Vector3F(0, 0, 1)), out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(0, 0, 0), p0);
      Assert.AreEqual(new Vector3F(2, 2, 0), p1);

      // Zero length segment.
      haveContact = GeometryHelper.GetClosestPoints(new LineSegment(new Vector3F(2, 2, 0), new Vector3F(2, 2f, 0)),
                                                 new Line(new Vector3F(0, 0, 0), new Vector3F(0, 0, 1)), out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(0, 0, 0), p0);
      Assert.AreEqual(new Vector3F(2, 2, 0), p1);
    }


    [Test]
    public void GetClosestPointsLinePlane()
    {
      Vector3F p0, p1;
      bool haveContact;

      haveContact = GeometryHelper.GetClosestPoints(new Plane(new Vector3F(0, 1, 0), 1),
                                                 new Line(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0)), out p0, out p1);
      Assert.AreEqual(false, haveContact);  // Plane is really a plane - no half space.
      Assert.AreEqual(0, p1.Y);
      Assert.AreEqual(0, p1.Z);
      Assert.AreEqual(1, p0.Y);
      Assert.AreEqual(p0.X, p1.X);
      Assert.AreEqual(p0.Z, p1.Z);

      haveContact = GeometryHelper.GetClosestPoints(new Plane(new Vector3F(0, 1, 0), 1),
                                                 new Line(new Vector3F(2, 0, 0), new Vector3F(0, 1, 0)), out p0, out p1);
      Assert.AreEqual(true, haveContact);
      Assert.AreEqual(new Vector3F(2, 1, 0), p1);
      Assert.AreEqual(new Vector3F(2, 1, 0), p0);

      haveContact = GeometryHelper.GetClosestPoints(new Plane(new Vector3F(0, 1, 0), 1),
                                                 new Line(new Vector3F(2, 3, 0), new Vector3F(1, 0, 0)), out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(3, p1.Y);
      Assert.AreEqual(1, p0.Y);
      Assert.AreEqual(p0.X, p1.X);
      Assert.AreEqual(p0.Z, p1.Z);
    }


    [Test]
    public void GetClosestPointsSegmentPlane()
    {
      Vector3F p0, p1;
      bool haveContact;

      haveContact = GeometryHelper.GetClosestPoints(new Plane(new Vector3F(0, 1, 0), 1),
                                                 new LineSegment(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0)), out p0, out p1);
      Assert.AreEqual(false, haveContact);  // Plane is really a plane - no half space.
      Assert.AreEqual(0, p1.Y);
      Assert.AreEqual(0, p1.Z);
      Assert.AreEqual(1, p0.Y);
      Assert.AreEqual(p0.X, p1.X);
      Assert.AreEqual(p0.Z, p1.Z);
      Assert.IsTrue(p1.X >= 0 && p1.X <= 1);

      haveContact = GeometryHelper.GetClosestPoints(new Plane(new Vector3F(0, 1, 0), 1),
                                                 new LineSegment(new Vector3F(2, 2, 0), new Vector3F(3, 4, 0)), out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(2, 2, 0), p1);
      Assert.AreEqual(new Vector3F(2, 1, 0), p0);

      haveContact = GeometryHelper.GetClosestPoints(new Plane(new Vector3F(0, 1, 0), 1),
                                                 new LineSegment(new Vector3F(2, 4, 0), new Vector3F(3, 2, 0)), out p0, out p1);
      Assert.AreEqual(false, haveContact);
      Assert.AreEqual(new Vector3F(3, 2, 0), p1);
      Assert.AreEqual(new Vector3F(3, 1, 0), p0);

      haveContact = GeometryHelper.GetClosestPoints(new Plane(new Vector3F(0, 1, 0), 1),
                                                 new LineSegment(new Vector3F(2, 2, 0), new Vector3F(3, 1, 0)), out p0, out p1);
      Assert.AreEqual(true, haveContact);
      Assert.AreEqual(new Vector3F(3, 1, 0), p1);
      Assert.AreEqual(new Vector3F(3, 1, 0), p0);

      haveContact = GeometryHelper.GetClosestPoints(new Plane(new Vector3F(0, 1, 0), 1),
                                                 new LineSegment(new Vector3F(2, 2, 0), new Vector3F(4, 0, 0)), out p0, out p1);
      Assert.AreEqual(true, haveContact);
      Assert.AreEqual(new Vector3F(3, 1, 0), p0);
      Assert.AreEqual(new Vector3F(3, 1, 0), p1);
    }
  }
}
