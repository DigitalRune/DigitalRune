using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class GeometryHelperTest
  {
    [SetUp]
    public void Setup()
    {
      RandomHelper.Random = new Random(1234567);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComputeBoundingShapeWithNullArgument()
    {
      GeometryHelper.CreateBoundingShape(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComputeBoundingShapeWithEmptyList()
    {
      GeometryHelper.CreateBoundingShape(new List<Vector3F>());
    }


    [Test]
    public void ComputeBoundingShapeWithPlanarPoints()
    {
      var points = new[]
      {
        new Vector3F(-3, 0, 0),
        new Vector3F(-2, 0, 0),
        new Vector3F(-1, 0, 0),
        new Vector3F(1, 0, 0),
        new Vector3F(2, 0, 0),
        new Vector3F(3, 0, 0),
      };
      var shape = GeometryHelper.CreateBoundingShape(points);
      BoxShape box = (BoxShape)shape;
      Assert.AreEqual(new Vector3F(6, 0, 0), box.Extent);
    }


    [Test]
    public void ComputeBoundingShapeCenteredSphere()
    {
      RandomHelper.Random = new Random(123);
      var radius = new Vector3F(3, 0, 0);
      var points = new List<Vector3F>
      {
        new Vector3F(3, 0, 0), new Vector3F(-3, 0, 0), 
        new Vector3F(0, 3, 0), new Vector3F(0, -3, 0), 
        new Vector3F(0, 0, 3), new Vector3F(0, 0, -3), 
      };

      for (int i = 0; i < 40; i++)
        points.Add(RandomHelper.Random.NextQuaternionF().Rotate(radius));

      var shape = GeometryHelper.CreateBoundingShape(points);
      SphereShape s = (SphereShape)shape;
      Assert.IsTrue(Numeric.AreEqual(3, s.Radius));

      var cd = new CollisionDetection();

      GeometricObject geometry = new GeometricObject(shape);
      CollisionObject boundingObject = new CollisionObject(geometry);

      // Test if all points are in the bounding shape.
      for (int i = 0; i < points.Count; i++)
      {
        var point = points[i];

        // Test against a sphere around the point. Some points are exactly on the surface
        // and are very sensitive to tiny numerical errors.
        var pointGeometry = new GeometricObject(new SphereShape(Numeric.EpsilonF * 10), new Pose(point));

        Assert.IsTrue(cd.HaveContact(new CollisionObject(pointGeometry), boundingObject));
      }
    }



    [Test]
    public void ComputeBoundingBoxWithPlanarPoints()
    {
      var points = new[]
      {
        new Vector3F(-3, 0, 0),
        new Vector3F(-2, 0, 0),
        new Vector3F(-1, 0, 0),
        new Vector3F(1, 0, 0),
        new Vector3F(2, 0, 0),
        new Vector3F(3, 0, 0),
      };
      Vector3F box;
      Pose pose;
      GeometryHelper.ComputeBoundingBox(points, out box, out pose);
      Assert.IsTrue(Numeric.AreEqual(6, pose.ToWorldDirection(box).X));
      Assert.IsTrue(Numeric.AreEqual(0, pose.ToWorldDirection(box).Y));
      Assert.IsTrue(Numeric.AreEqual(0, pose.ToWorldDirection(box).Z));
    }


    [Test]
    public void CreateBoundingShape()
    {
      int numberOfBoxes = 0;
      int numberOfSpheres = 0;
      const int numberOfTests = 100;
      RandomHelper.Random = new Random(727);
      for (int test = 0; test < numberOfTests; test++)
      {
        // Fill list with a random number of random points.
        int numberOfPoints = RandomHelper.Random.NextInteger(2, 100);
        List<Vector3F> points = new List<Vector3F>();
        for (int i = 0; i < numberOfPoints; i++)
          points.Add(RandomHelper.Random.NextVector3F(-10, 100));

        Shape shape = GeometryHelper.CreateBoundingShape(points);
        GeometricObject geometry = new GeometricObject(shape);
        CollisionObject boundingObject = new CollisionObject(geometry);

        if (shape is BoxShape)
          numberOfBoxes++;
        if (shape is SphereShape)
          numberOfSpheres++;
        if (((TransformedShape)shape).Child.Shape is BoxShape)
          numberOfBoxes++;
        else
          numberOfSpheres++;

        Assert.IsNotNull(shape);

        var cd = new CollisionDetection();

        // Test if all points are in the bounding shape.
        for (int i = 0; i < numberOfPoints; i++)
        {
          var point = points[i];

          // Test against a sphere around the point. Some points are exactly on the surface
          // and are very sensitive to tiny numerical errors.
          var pointGeometry = new GeometricObject(new SphereShape(Numeric.EpsilonF * 10), new Pose(point));

          Assert.IsTrue(cd.HaveContact(new CollisionObject(pointGeometry), boundingObject));
        }
      }

      Console.WriteLine("ShapeHelper.CreateBoundingShape: Number of Boxes : Number of Spheres = " + numberOfBoxes + " : " + numberOfSpheres);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComputeBoxWithNullArgument()
    {
      Vector3F box;
      Pose pose;
      GeometryHelper.ComputeBoundingBox(null, out box, out pose);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComputeBoxWithEmptyList()
    {
      Vector3F box;
      Pose pose;
      GeometryHelper.ComputeBoundingBox(new List<Vector3F>(), out box, out pose);
    }


    [Test]
    public void ComputeBox()
    {
      Vector3F box;
      Pose pose;

      List<Vector3F> points = new List<Vector3F>()
      {
        new Vector3F(1, 0, 0),
      };
      GeometryHelper.ComputeBoundingBox(points, out box, out pose);
      Assert.IsTrue(Numeric.AreEqual(box.X, 0));
      Assert.IsTrue(Numeric.AreEqual(box.Y, 0));
      Assert.IsTrue(Numeric.AreEqual(box.Z, 0));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), pose.Position));

      points = new List<Vector3F>()
      {
        new Vector3F(1, 0, 0),
        new Vector3F(1, 0, 0),
      };
      GeometryHelper.ComputeBoundingBox(points, out box, out pose);
      Assert.IsTrue(Numeric.AreEqual(box.X, 0));
      Assert.IsTrue(Numeric.AreEqual(box.Y, 0));
      Assert.IsTrue(Numeric.AreEqual(box.Z, 0));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), pose.Position));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 1, 0),
        new Vector3F(0, 3, 0),
      };
      GeometryHelper.ComputeBoundingBox(points, out box, out pose);
      Assert.IsTrue(Numeric.AreEqual(box.X, 0));
      Assert.IsTrue(Numeric.AreEqual(box.Y, 2));
      Assert.IsTrue(Numeric.AreEqual(box.Z, 0));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 2, 0), pose.Position));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 1, 0),
        new Vector3F(0, 3, 0),
        new Vector3F(0, 3, 0),
        new Vector3F(0, 1, 0),
      };
      GeometryHelper.ComputeBoundingBox(points, out box, out pose);
      Assert.IsTrue(Numeric.AreEqual(box.X, 0));
      Assert.IsTrue(Numeric.AreEqual(box.Y, 2));
      Assert.IsTrue(Numeric.AreEqual(box.Z, 0));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 2, 0), pose.Position));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 1, 0),
        new Vector3F(0, 5, 0),
        new Vector3F(0, 1, 2),
        new Vector3F(0, 5, 2),
      };
      GeometryHelper.ComputeBoundingBox(points, out box, out pose);
      Assert.IsTrue(Numeric.AreEqual(box.X, 0));
      Assert.IsTrue(Numeric.AreEqual(box.Y, 4));
      Assert.IsTrue(Numeric.AreEqual(box.Z, 2));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 3, 1), pose.Position));

      // Cube
      points = new List<Vector3F>()
      {
        new Vector3F(-1, -1, -1),
        new Vector3F(-1, -1, 1),
        new Vector3F(-1, 1, -1),
        new Vector3F(-1, 1, 1),
        new Vector3F(1, -1, -1),
        new Vector3F(1, -1, 1),
        new Vector3F(1, 1, -1),
        new Vector3F(1, 1, 1),
      };
      GeometryHelper.ComputeBoundingBox(points, out box, out pose);
      Assert.IsTrue(Numeric.AreEqual(box.X, 2));
      Assert.IsTrue(Numeric.AreEqual(box.Y, 2));
      Assert.IsTrue(Numeric.AreEqual(box.Z, 2));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), pose.Position));

      // Cube
      points = new List<Vector3F>()
      {
        new Vector3F(-1, -1, -1),
        new Vector3F(-1, -1, 1),
        new Vector3F(-1, 1, -1),
        new Vector3F(-1, 1, 1),
        new Vector3F(-1, 0, 0),
        new Vector3F(0, 0, 0),
        new Vector3F(1, 0, 0),
        new Vector3F(1, -1, -1),
        new Vector3F(1, -1, 1),
        new Vector3F(1, 1, -1),
        new Vector3F(1, 1, 1),
      };
      GeometryHelper.ComputeBoundingBox(points, out box, out pose);
      Assert.IsTrue(Numeric.AreEqual(box.X, 2));
      Assert.IsTrue(Numeric.AreEqual(box.Y, 2));
      Assert.IsTrue(Numeric.AreEqual(box.Z, 2));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), pose.Position));

      // Box
      points = new List<Vector3F>()
      {
        new Vector3F(-2, -3, -1),
        new Vector3F(-2, -3, 1),
        new Vector3F(-2, 3, -1),
        new Vector3F(-2, 3, 1),
        new Vector3F(-2, 0, 0),
        new Vector3F(0, 0, 0),
        new Vector3F(2, 0, 0),
        new Vector3F(2, -3, -1),
        new Vector3F(2, -3, 1),
        new Vector3F(2, 3, -1),
        new Vector3F(2, 3, 1),
      };
      GeometryHelper.ComputeBoundingBox(points, out box, out pose);
      Assert.IsTrue(Numeric.AreEqual(box.X, 4));
      Assert.IsTrue(Numeric.AreEqual(box.Y, 6));
      Assert.IsTrue(Numeric.AreEqual(box.Z, 2));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), pose.Position));
    }


    [Test]
    public void ComputeBoxForCube()
    {
      Vector3F box;
      Pose pose;

      // Cube
      var points = new List<Vector3F>()
      {
        new Vector3F(-1, -1, -1),
        new Vector3F(-1, -1, 1),
        new Vector3F(-1, 1, -1),
        new Vector3F(-1, 1, 1),
        new Vector3F(1, -1, -1),
        new Vector3F(1, -1, 1),
        new Vector3F(1, 1, -1),
        new Vector3F(1, 1, 1),
      };

      // Translate and rotate cube.
      Pose cubePose = new Pose(new Vector3F(10, 2, 3), RandomHelper.Random.NextQuaternionF());
      for (int i = 0; i < points.Count; i++)
        points[i] = cubePose.ToWorldPosition(points[i]);

      GeometryHelper.ComputeBoundingBox(points, out box, out pose);
      Assert.IsTrue(Numeric.AreEqual(box.X, 2, 0.2f));
      Assert.IsTrue(Numeric.AreEqual(box.Y, 2, 0.2f));
      Assert.IsTrue(Numeric.AreEqual(box.Z, 2, 0.2f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(10, 2, 3), pose.Position));
    }


    [Test]
    public void ComputeBoxForCapsules()
    {
      Vector3F box;
      Pose pose;

      var points = new CapsuleShape(1, 3).GetMesh(0.01f, 4).Vertices;

      Pose cubePose = new Pose(new Vector3F(10, 2, 3), RandomHelper.Random.NextQuaternionF());
      for (int i = 0; i < points.Count; i++)
        points[i] = cubePose.ToWorldPosition(points[i]);

      GeometryHelper.ComputeBoundingBox(points, out box, out pose);
      Assert.IsTrue(Numeric.AreEqual(box.LargestComponent, 3, 0.2f));
      Assert.IsTrue(Numeric.AreEqual(box.SmallestComponent, 2, 0.2f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(10, 2, 3), pose.Position));
    }


    [Test]
    public void ComputeBoxWithRandomPoints()
    {
      Vector3F box;
      Pose pose;

      const int numberOfTests = 100;
      RandomHelper.Random = new Random(377);
      for (int test = 0; test < numberOfTests; test++)
      {
        // Fill list with a random number of random points.
        int numberOfPoints = RandomHelper.Random.NextInteger(2, 100);
        List<Vector3F> points = new List<Vector3F>();
        for (int i = 0; i < numberOfPoints; i++)
          points.Add(RandomHelper.Random.NextVector3F(-10, 100));

        GeometryHelper.ComputeBoundingBox(points, out box, out pose);

        // Check if sphere can be valid.
        Assert.IsTrue(box.X >= 0);
        Assert.IsTrue(box.Y >= 0);
        Assert.IsTrue(box.Z >= 0);
        Assert.IsTrue(!float.IsNaN(pose.Position.Length));
        Assert.IsTrue(pose.Orientation.IsRotation);

        // Test if all points are in the box.
        // And test if all sides touch a point. If not the box could be smaller!
        bool minX = false; // Stores if the minX face is touched.
        bool minY = false;
        bool minZ = false;
        bool maxX = false;
        bool maxY = false;
        bool maxZ = false;
        for (int i = 0; i < numberOfPoints; i++)
        {
          var point = points[i];

          var localPoint = pose.ToLocalPosition(point);

          //if (!Box.HaveContact(box, localPoint))
          //  Debugger.Break();

          Assert.IsTrue(Numeric.Compare(Math.Abs(localPoint.X), box.X / 2, 0.001f) <= 0);
          Assert.IsTrue(Numeric.Compare(Math.Abs(localPoint.Y), box.Y / 2, 0.001f) <= 0);
          Assert.IsTrue(Numeric.Compare(Math.Abs(localPoint.Z), box.Z / 2, 0.001f) <= 0);

          if (Numeric.AreEqual(localPoint.X, -box.X / 2, 0.001f))
            minX = true;
          if (Numeric.AreEqual(localPoint.X, box.X / 2, 0.001f))
            maxX = true;
          if (Numeric.AreEqual(localPoint.Y, -box.Y / 2, 0.001f))
            minY = true;
          if (Numeric.AreEqual(localPoint.Y, box.Y / 2, 0.001f))
            maxY = true;
          if (Numeric.AreEqual(localPoint.Z, -box.Z / 2, 0.001f))
            minZ = true;
          if (Numeric.AreEqual(localPoint.Z, box.Z / 2, 0.001f))
            maxZ = true;
        }

        Assert.IsTrue(minX);
        Assert.IsTrue(minY);
        Assert.IsTrue(minZ);
        Assert.IsTrue(maxX);
        Assert.IsTrue(maxY);
        Assert.IsTrue(maxZ);
      }
    }


    [Test]
    public void ComputeCapsuleWithRandomPoints()
    {
      float height;
      float radius;
      Pose pose;

      const int numberOfTests = 100;
      RandomHelper.Random = new Random(377);
      for (int test = 0; test < numberOfTests; test++)
      {
        // Fill list with a random number of random points.
        int numberOfPoints = RandomHelper.Random.NextInteger(2, 100);
        List<Vector3F> points = new List<Vector3F>();
        for (int i = 0; i < numberOfPoints; i++)
          points.Add(RandomHelper.Random.NextVector3F(-10, 100));

        GeometryHelper.ComputeBoundingCapsule(points, out radius, out height, out pose);

        // Check if sphere can be valid.
        Assert.IsTrue(radius >= 0);
        Assert.IsTrue(height >= 2 * radius);
        Assert.IsTrue(!float.IsNaN(pose.Position.Length));
        Assert.IsTrue(pose.Orientation.IsRotation);

        // Test if all points are in the shape.
        var cd = new CollisionDetection();

        GeometricObject geometry = new GeometricObject(new CapsuleShape(radius, height), pose);
        CollisionObject boundingObject = new CollisionObject(geometry);

        // Test if all points are in the bounding shape.
        for (int i = 0; i < numberOfPoints; i++)
        {
          var point = points[i];

          // Test against a sphere around the point. Some points are exactly on the surface
          // and are very sensitive to tiny numerical errors.
          var pointGeometry = new GeometricObject(new SphereShape(Numeric.EpsilonF * (height + 1)), new Pose(point));
          Assert.IsTrue(cd.HaveContact(new CollisionObject(pointGeometry), boundingObject));
        }
      }
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComputeSphereWithNullArgument()
    {
      Vector3F center;
      float radius;
      GeometryHelper.ComputeBoundingSphere(null, out radius, out center);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComputeSphereWithEmptyList()
    {
      Vector3F center;
      float radius;
      GeometryHelper.ComputeBoundingSphere(new List<Vector3F>(), out radius, out center);
    }


    [Test]
    public void ComputeSphere()
    {
      Vector3F center;
      float radius;

      List<Vector3F> points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(0, radius));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
        new Vector3F(0, 0, 0),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(0, radius));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
        new Vector3F(2, 0, 0),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(1, radius));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
        new Vector3F(1, 0, 0),
        new Vector3F(2, 0, 0),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(1, radius));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
        new Vector3F(1, 0, 0),
        new Vector3F(2, 0, 0),
        new Vector3F(2.000001f, 0, 0),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(1, radius));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
        new Vector3F(1, 0, 0),
        new Vector3F(-1, 0, 0),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(1, radius));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
        new Vector3F(1, 0, 0),
        new Vector3F(-1, 0, 0),
        new Vector3F(0, 1, 0),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(1, radius));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
        new Vector3F(1, 0, 0),
        new Vector3F(-1, 0, 0),
        new Vector3F(0, 1, 0),
        new Vector3F(0, 0, 1),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(1, radius));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
        new Vector3F(1, 0, 0),
        new Vector3F(-1, 0, 0),
        new Vector3F(0, 1, 0),
        new Vector3F(0, 0, 1),
        new Vector3F(0, -1, 0),
        new Vector3F(0, -1.000001f, 0),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(1, radius));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
        new Vector3F(0.1f, 0, 0),
        new Vector3F(0, 0.2f, 0),
        new Vector3F(0, 0, 0),
        new Vector3F(-1, 0, 0),
        new Vector3F(0, 1, 0),
        new Vector3F(0, 0, 1),
        new Vector3F(0, 0, 0),
        new Vector3F(0, -1, 0),
        new Vector3F(0, -1.000001f, 0),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(1, radius));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
        new Vector3F(0.1f, 0, 0),
        new Vector3F(0, 0.2f, 0),
        new Vector3F(0, 0, 0),
        new Vector3F(-1, 0, 0),
        new Vector3F(0, 1, 0),
        new Vector3F(0, 0, 0),
        new Vector3F(0, -1, 0),
        new Vector3F(0, -1.000001f, 0),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(1, radius));

      points = new List<Vector3F>()
      {
        new Vector3F(0, 0, 0),
        new Vector3F(0.1f, 0, 0),
        new Vector3F(0, 0.2f, 0),
        new Vector3F(0, 0, 0),
        new Vector3F(-1, 0, 0),
        new Vector3F(0, 1, 0),
        new Vector3F(0, 0, 1),
        new Vector3F(0, 0, 0),
        new Vector3F(0, -0.4f, 0.3f),
        new Vector3F(0, -1, 0),
        new Vector3F(0, -1.000001f, 0),
      };
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), center));
      Assert.IsTrue(Numeric.AreEqual(1, radius));
    }


    [Test]
    public void ComputeSphereWithRandomPoints()
    {
      Vector3F center;
      float radius;

      const int numberOfTests = 1000;
      RandomHelper.Random = new Random(377);
      for (int test = 0; test < numberOfTests; test++)
      {
        // Fill list with a random number of random points.
        int numberOfPoints = RandomHelper.Random.NextInteger(2, 100);
        List<Vector3F> points = new List<Vector3F>();
        for (int i = 0; i < numberOfPoints; i++)
          points.Add(RandomHelper.Random.NextVector3F(-10, 100));

        GeometryHelper.ComputeBoundingSphere(points, out radius, out center);

        // Check if sphere can be valid.
        Assert.IsTrue(radius >= 0);
        Assert.IsTrue(!float.IsNaN(center.X + center.Y + center.Z));

        // We test with a greater epsilon. Numeric.EpsilonF is too small.
        float epsilon = 0.0001f * (radius + 1);

        // Test if all points are in the sphere and count the support points.
        int numberOfSupportPoints = 0;
        for (int i = 0; i < numberOfPoints; i++)
        {
          var p = points[i];

          float distanceFromSurface = (p - center).Length - radius;

          if (Numeric.IsZero(distanceFromSurface, epsilon))
            numberOfSupportPoints++;

          //if (Numeric.Compare(distanceFromSurface, 0, epsilon) > 0)
          //  Debugger.Break();

          Assert.IsTrue(Numeric.Compare(distanceFromSurface, 0, epsilon) <= 0);
        }

        //if (numberOfSupportPoints < 2)
        //  Debugger.Break();

        // We need at least two support points.
        Assert.GreaterOrEqual(numberOfSupportPoints, 2);

        // Compute approximate sphere (center = point average) and test if the minimal sphere is 
        // really smaller.
        var approximateCenter = points[0];
        for (int i = 1; i < numberOfPoints; i++)
          approximateCenter += points[i];
        approximateCenter /= numberOfPoints;
        float approximateRadius = 0;
        for (int i = 0; i < numberOfPoints; i++)
          if ((points[i] - approximateCenter).Length > approximateRadius)
            approximateRadius = (points[i] - approximateCenter).Length;
        //if (approximateRadius < radius)
        //  Debugger.Break();
        Assert.GreaterOrEqual(approximateRadius, radius);
      }
    }


    [Test]
    public void ComputeCircumscribedSphereFor3Points()
    {
      Vector3F center;
      float radius;

      // Compute sphere and test if all points are on the surface.
      Vector3F p0 = new Vector3F(0, 0, 0);
      Vector3F p1 = new Vector3F(1, 0, 0);
      Vector3F p2 = new Vector3F(0, 1, 0);

      GeometryHelper.ComputeCircumscribedSphere(p0, p1, p2, out radius, out center);

      float distance = (p0 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p1 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p2 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));

      // Compute sphere and test if all points are on the surface.
      p0 = new Vector3F(0, 0, 1);
      p1 = new Vector3F(1, 0, 0);
      p2 = new Vector3F(0, 1, 0);

      GeometryHelper.ComputeCircumscribedSphere(p0, p1, p2, out radius, out center);

      distance = (p0 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p1 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p2 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));


      // Compute sphere and test if all points are on the surface.
      p0 = new Vector3F(120, -44, 45);
      p1 = new Vector3F(-21, 23, -78);
      p2 = new Vector3F(93, 231, 65);

      GeometryHelper.ComputeCircumscribedSphere(p0, p1, p2, out radius, out center);

      distance = (p0 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p1 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p2 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));

      // Test a degenerate case.
      p0 = new Vector3F(120, -44, 45);
      p1 = new Vector3F(-21, 23, -78);
      p2 = p1;

      GeometryHelper.ComputeCircumscribedSphere(p0, p1, p2, out radius, out center);

      Assert.IsTrue(float.IsNaN(radius));
    }


    [Test]
    public void ComputeCircumscribedSphereFor4Points()
    {
      Vector3F center;
      float radius;

      // Compute sphere and test if all points are on the surface.
      Vector3F p0 = new Vector3F(0, 0, 0);
      Vector3F p1 = new Vector3F(1, 0, 0);
      Vector3F p2 = new Vector3F(0, 1, 0);
      Vector3F p3 = new Vector3F(0, 0, 1);

      GeometryHelper.ComputeCircumscribedSphere(p0, p1, p2, p3, out radius, out center);

      float distance = (p0 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p1 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p2 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p3 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));

      // Compute sphere and test if all points are on the surface.
      p0 = new Vector3F(120, -44, 45);
      p1 = new Vector3F(-21, 23, -78);
      p2 = new Vector3F(93, 231, 65);
      p3 = new Vector3F(88, -97, -75);

      GeometryHelper.ComputeCircumscribedSphere(p0, p1, p2, p3, out radius, out center);

      distance = (p0 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p1 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p2 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));
      distance = (p3 - center).Length;
      Assert.IsTrue(Numeric.AreEqual(distance, radius));


      // Test a degenerate case.
      p0 = new Vector3F(120, -44, 45);
      p1 = new Vector3F(-21, 23, -78);
      p2 = new Vector3F(93, 231, 65);
      p3 = p2;

      GeometryHelper.ComputeCircumscribedSphere(p0, p1, p2, p3, out radius, out center);

      Assert.IsTrue(float.IsNaN(radius));
    }


    [Test]
    public void HaveContactAabb()
    {
      var aabb1 = new Aabb(new Vector3F(10, 10, 10), new Vector3F(10, 10, 10));
      var aabb2 = new Aabb(new Vector3F(10, 10, 10), new Vector3F(20, 20, 20));
      var aabb3 = new Aabb(new Vector3F(30, 10, 10), new Vector3F(40, 20, 20));
      var aabbInf = new Aabb(new Vector3F(float.NegativeInfinity), new Vector3F(float.PositiveInfinity));
      var aabbNaN = new Aabb(new Vector3F(float.NaN), new Vector3F(float.NaN));
      Assert.IsTrue(GeometryHelper.HaveContact(aabb1, aabb1));
      Assert.IsTrue(GeometryHelper.HaveContact(aabb1, aabbInf));
      Assert.IsTrue(GeometryHelper.HaveContact(aabbInf, aabbInf));
      Assert.IsTrue(GeometryHelper.HaveContact(aabb1, aabb2));
      Assert.IsFalse(GeometryHelper.HaveContact(aabb2, aabb3));

      Assert.IsTrue(GeometryHelper.HaveContact(aabb3, new Vector3F(30, 10, 10)));
      Assert.IsTrue(GeometryHelper.HaveContact(aabb3, new Vector3F(32, 12, 12)));
      Assert.IsFalse(GeometryHelper.HaveContact(aabb3, new Vector3F(29, 10, 10)));

      Assert.IsFalse(GeometryHelper.HaveContact(aabb1, aabbNaN));
      Assert.IsFalse(GeometryHelper.HaveContact(aabbNaN, aabb1));
      Assert.IsFalse(GeometryHelper.HaveContact(aabbInf, aabbNaN));
      Assert.IsFalse(GeometryHelper.HaveContact(aabbNaN, aabbInf));
    }


    [Test]
    public void HaveContactAabbPoint()
    {
      var p = new Vector3F(10, 10, 10);
      var aabb1 = new Aabb(new Vector3F(10, 10, 10), new Vector3F(10, 10, 10));
      var aabb2 = new Aabb(new Vector3F(10, 10, 10), new Vector3F(20, 20, 20));
      var aabb3 = new Aabb(new Vector3F(30, 10, 10), new Vector3F(40, 20, 20));
      var aabbInf = new Aabb(new Vector3F(float.NegativeInfinity), new Vector3F(float.PositiveInfinity));
      var aabbNaN = new Aabb(new Vector3F(float.NaN), new Vector3F(float.NaN));
      Assert.IsTrue(GeometryHelper.HaveContact(aabb1, p));
      Assert.IsTrue(GeometryHelper.HaveContact(aabb2, p));
      Assert.IsFalse(GeometryHelper.HaveContact(aabb3, p));
      Assert.IsTrue(GeometryHelper.HaveContact(aabbInf, p));

      Assert.IsFalse(GeometryHelper.HaveContact(aabbNaN, p));
    }


    [Test]
    public void HaveContactAabbBox2()
    {
      var box = new Vector3F(1, 1, 1);
      var aabb = new Aabb(new Vector3F(-1, -1, -1), new Vector3F(1, 1, 1));

      Assert.IsTrue(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(0, 0, 0)), true));
      Assert.IsTrue(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(1, 0, 0)), true));

      Assert.IsFalse(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(-2, 0, 0)), true));
      Assert.IsFalse(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(0, -2, 0)), true));
      Assert.IsFalse(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(0, 0, -2)), true));
      Assert.IsFalse(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(2, 0, 0)), true));
      Assert.IsFalse(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(0, 2, 0)), true));
      Assert.IsFalse(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(0, 0, 2)), true));

      Assert.IsFalse(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(1.5f, 1.5f, 0), QuaternionF.CreateRotationZ(MathHelper.ToRadians(45))), true));
      Assert.IsFalse(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(1.5f, 1.5f, 0), QuaternionF.CreateRotationZ(MathHelper.ToRadians(-45))), true));
      Assert.IsFalse(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(1.5f, 1.5f, 0), QuaternionF.CreateRotationZ(MathHelper.ToRadians(45)) * QuaternionF.CreateRotationY(MathHelper.ToRadians(90))), true));

      // Edge test case where MakeEdgeTest makes a difference.
      Assert.IsFalse(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(1.6f, 1.6f, 0), QuaternionF.CreateRotationY(MathHelper.ToRadians(45)) * QuaternionF.CreateRotationZ(MathHelper.ToRadians(45))), true));
      Assert.IsTrue(GeometryHelper.HaveContact(aabb, box, new Pose(new Vector3F(1.6f, 1.6f, 0), QuaternionF.CreateRotationY(MathHelper.ToRadians(45)) * QuaternionF.CreateRotationZ(MathHelper.ToRadians(45))), false));
    }


    [Test]
    public void HaveContactAabbRay()
    {
      var aabb = new Aabb(new Vector3F(-1), new Vector3F(1));

      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(-2, 2, 0), new Vector3F(1, 0, 0), 10)));
      Assert.AreEqual(true, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(-2, 0, 0), new Vector3F(1, 0, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(-2, -2, 0), new Vector3F(1, 0, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(-2, 2, 0), new Vector3F(-1, 0, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(-2, 0, 0), new Vector3F(-1, 0, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(-2, -2, 0), new Vector3F(-1, 0, 0), 10)));

      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, -2, -2), new Vector3F(0, 1, 0), 10)));
      Assert.AreEqual(true, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, -2, 0), new Vector3F(0, 1, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, -2, 2), new Vector3F(0, 1, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, -2, 0), new Vector3F(0, -1, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, -2, 0), new Vector3F(0, -1, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, -2, 0), new Vector3F(0, -1, 0), 10)));

      Assert.AreEqual(true, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, 2f, 0), new Vector3F(1, -1, 0).Normalized, 10)));
      Assert.AreEqual(true, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, 2f, 0), new Vector3F(1, -1.1f, 0).Normalized, 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, 2f, 0), new Vector3F(1, -0.9f, 0).Normalized, 10)));

      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, 1.5f, 0), -new Vector3F(1, -1, 0).Normalized, 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, 1.5f, 0), -new Vector3F(1, -1.1f, 0).Normalized, 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, 1.5f, 0), -new Vector3F(1, -0.9f, 0).Normalized, 10)));

      Assert.AreEqual(true, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0).Normalized, 0)));
      Assert.AreEqual(true, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0).Normalized, 0)));

      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(-1.1f, 0, 0), -new Vector3F(1, 0, 0).Normalized, 10)));

      // Ray is parallel to one AABB side but not touching.
      Assert.AreEqual(false, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(1, -2, 0), new Vector3F(0, -1, 0).Normalized, 10)));

      // Ray is parallel to one AABB side and touching.
      Assert.AreEqual(true, GeometryHelper.HaveContact(aabb, new Ray(new Vector3F(1, 0, 0), new Vector3F(0, -1, 0).Normalized, 10)));
    }


    [Test]
    public void HaveContactAabbRayFast()
    {
      var aabb = new Aabb(new Vector3F(-1), new Vector3F(1));

      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(-2, 2, 0), new Vector3F(1, 0, 0), 10)));
      Assert.AreEqual(true, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(-2, 0, 0), new Vector3F(1, 0, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(-2, -2, 0), new Vector3F(1, 0, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(-2, 2, 0), new Vector3F(-1, 0, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(-2, 0, 0), new Vector3F(-1, 0, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(-2, -2, 0), new Vector3F(-1, 0, 0), 10)));

      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, -2, -2), new Vector3F(0, 1, 0), 10)));
      Assert.AreEqual(true, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, -2, 0), new Vector3F(0, 1, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, -2, 2), new Vector3F(0, 1, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, -2, 0), new Vector3F(0, -1, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, -2, 0), new Vector3F(0, -1, 0), 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, -2, 0), new Vector3F(0, -1, 0), 10)));

      Assert.AreEqual(true, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, 2f, 0), new Vector3F(1, -1, 0).Normalized, 10)));
      Assert.AreEqual(true, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, 2f, 0), new Vector3F(1, -1.1f, 0).Normalized, 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, 2f, 0), new Vector3F(1, -0.9f, 0).Normalized, 10)));

      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, 1.5f, 0), -new Vector3F(1, -1, 0).Normalized, 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, 1.5f, 0), -new Vector3F(1, -1.1f, 0).Normalized, 10)));
      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, 1.5f, 0), -new Vector3F(1, -0.9f, 0).Normalized, 10)));

      Assert.AreEqual(true, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0).Normalized, 0)));
      Assert.AreEqual(true, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0).Normalized, 0)));

      Assert.AreEqual(false, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(-1.1f, 0, 0), -new Vector3F(1, 0, 0).Normalized, 10)));

      // Ray is parallel to one AABB side but not touching. Method returns false positive!
      Assert.AreEqual(true, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(1, -2, 0), new Vector3F(0, -1, 0).Normalized, 10)));

      // Ray is parallel to one AABB side and touching.
      Assert.AreEqual(true, GeometryHelper.HaveContactFast(aabb, new Ray(new Vector3F(1, 0, 0), new Vector3F(0, -1, 0).Normalized, 10)));
    }



    [Test]
    public void HaveContactAabbMovingAabb()
    {
      Assert.IsTrue(GeometryHelper.HaveContact(
        new Aabb(new Vector3F(0), new Vector3F(1)),
        new Aabb(new Vector3F(0), new Vector3F(1)),
        new Vector3F(0, 0, 0)));

      Assert.IsFalse(GeometryHelper.HaveContact(
        new Aabb(new Vector3F(0), new Vector3F(1)),
        new Aabb(new Vector3F(2), new Vector3F(3)),
        new Vector3F(0, 0, 0)));

      Assert.IsFalse(GeometryHelper.HaveContact(
        new Aabb(new Vector3F(0, 0, 0), new Vector3F(1, 1, 1)),
        new Aabb(new Vector3F(-2, 0, 0), new Vector3F(-1, 1, 1)),
        new Vector3F(0, 0, 0)));

      Assert.IsTrue(GeometryHelper.HaveContact(
        new Aabb(new Vector3F(0, 0, 0), new Vector3F(1, 1, 1)),
        new Aabb(new Vector3F(-2, 0, 0), new Vector3F(-1, 1, 1)),
        new Vector3F(2, 0, 0)));

      Assert.IsFalse(GeometryHelper.HaveContact(
        new Aabb(new Vector3F(0, 0, 0), new Vector3F(1, 1, 1)),
        new Aabb(new Vector3F(-2, 0, 0), new Vector3F(-1, 1, 1)),
        new Vector3F(0, 2, 0)));

      Assert.IsFalse(GeometryHelper.HaveContact(
        new Aabb(new Vector3F(0, 0, 0), new Vector3F(1, 1, 1)),
        new Aabb(new Vector3F(-2, 0, 0), new Vector3F(-1, 1, 1)),
        new Vector3F(-2, 0, 0)));

      Assert.IsTrue(GeometryHelper.HaveContact(
              new Aabb(new Vector3F(0, 0, 0), new Vector3F(1, 1, 1)),
              new Aabb(new Vector3F(3 - 0.1f, 1, 0), new Vector3F(4, 2, 1)),
              new Vector3F(-2, -2, 0)));

      Assert.IsFalse(GeometryHelper.HaveContact(
              new Aabb(new Vector3F(0, 0, 0), new Vector3F(1, 1, 1)),
              new Aabb(new Vector3F(3 + 0.1f, 1, 0), new Vector3F(4, 2, 1)),
              new Vector3F(-2, -2, 0)));

    }


    [Test]
    public void GetClosestPointAabbPoint()
    {
      Aabb aabb = new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6));

      // Touching corner contacts.
      TestGetClosestPointAabbPoint(aabb, new Vector3F(1, 2, 3), new Vector3F(1, 2, 3), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(1, 2, 6), new Vector3F(1, 2, 6), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(1, 5, 3), new Vector3F(1, 5, 3), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(1, 5, 6), new Vector3F(1, 5, 6), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(4, 2, 3), new Vector3F(4, 2, 3), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(4, 2, 6), new Vector3F(4, 2, 6), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(4, 5, 3), new Vector3F(4, 5, 3), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(4, 5, 6), new Vector3F(4, 5, 6), true);

      // Touching face contacts.
      TestGetClosestPointAabbPoint(aabb, new Vector3F(2, 2, 4), new Vector3F(2, 2, 4), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(2, 3, 4), new Vector3F(2, 3, 4), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(2, 5, 4), new Vector3F(2, 5, 4), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(1, 3, 4), new Vector3F(1, 3, 4), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(2, 3, 3), new Vector3F(2, 3, 3), true);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(2, 3, 6), new Vector3F(2, 3, 6), true);

      // Intersection
      TestGetClosestPointAabbPoint(aabb, new Vector3F(2, 3, 4), new Vector3F(2, 3, 4), true);

      // Separated contacts
      TestGetClosestPointAabbPoint(aabb, new Vector3F(0, 0, 0), new Vector3F(1, 2, 3), false);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(10, 10, 10), new Vector3F(4, 5, 6), false);

      // Separated contacts (in Voronoi regions of faces).
      TestGetClosestPointAabbPoint(aabb, new Vector3F(2, 1, 4), new Vector3F(2, 2, 4), false);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(5, 3, 4), new Vector3F(4, 3, 4), false);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(2, 7, 4), new Vector3F(2, 5, 4), false);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(-1, 3, 4), new Vector3F(1, 3, 4), false);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(2, 3, 2), new Vector3F(2, 3, 3), false);
      TestGetClosestPointAabbPoint(aabb, new Vector3F(2, 3, 7), new Vector3F(2, 3, 6), false);

      // We could also check separated contacts in the Voronoi regions of the edges and corners.
      // But this should be enough.
    }


    private void TestGetClosestPointAabbPoint(Aabb aabb, Vector3F point, Vector3F expectedPoint, bool expectedResult)
    {
      Vector3F pointOnAabb;
      bool result = GeometryHelper.GetClosestPoint(aabb, point, out pointOnAabb);
      Assert.AreEqual(expectedPoint, pointOnAabb);
      Assert.AreEqual(expectedResult, result);
    }


    [Test]
    public void CartesionToSphericalCoordinateConversion()
    {
      float radius, azimuth, inclination;
      Vector3F v;

      v = new Vector3F(0, 0, 0);
      GeometryHelper.ToSphericalCoordinates(v, out radius, out inclination, out azimuth);
      Assert.AreEqual(0.0f, radius);
      Assert.AreEqual(0.0f, azimuth);
      Assert.AreEqual(0.0f, inclination);

      v = new Vector3F(0, 0, 2);
      GeometryHelper.ToSphericalCoordinates(v, out radius, out inclination, out azimuth);
      Assert.AreEqual(2.0f, radius);
      Assert.AreEqual(0.0f, azimuth);
      Assert.AreEqual(0.0f, inclination);

      v = new Vector3F(2, 0, 0);
      GeometryHelper.ToSphericalCoordinates(v, out radius, out inclination, out azimuth);
      Assert.AreEqual(2.0f, radius);
      Assert.AreEqual(0.0f, azimuth);
      Assert.AreEqual(ConstantsF.PiOver2, inclination);

      v = new Vector3F(0, 2, 0);
      GeometryHelper.ToSphericalCoordinates(v, out radius, out inclination, out azimuth);
      Assert.AreEqual(2.0f, radius);
      Assert.AreEqual(ConstantsF.PiOver2, azimuth);
      Assert.AreEqual(ConstantsF.PiOver2, inclination);

      v = new Vector3F(0, 0, -2);
      GeometryHelper.ToSphericalCoordinates(v, out radius, out inclination, out azimuth);
      Assert.AreEqual(2.0f, radius);
      Assert.AreEqual(0.0f, azimuth);
      Assert.AreEqual(ConstantsF.Pi, inclination);

      v = new Vector3F(-2, 0, 0);
      GeometryHelper.ToSphericalCoordinates(v, out radius, out inclination, out azimuth);
      Assert.AreEqual(2.0f, radius);
      Assert.AreEqual(ConstantsF.Pi, azimuth);
      Assert.AreEqual(ConstantsF.PiOver2, inclination);

      v = new Vector3F(0, -2, 0);
      GeometryHelper.ToSphericalCoordinates(v, out radius, out inclination, out azimuth);
      Assert.AreEqual(2.0f, radius);
      Assert.AreEqual(-ConstantsF.PiOver2, azimuth);
      Assert.AreEqual(ConstantsF.PiOver2, inclination);

      radius = 0;
      azimuth = 0;
      inclination = 0;
      v = GeometryHelper.ToCartesianCoordinates(radius, inclination, azimuth);
      Assert.AreEqual(new Vector3F(0, 0, 0), v);

      radius = 2;
      azimuth = 0;
      inclination = 0;
      v = GeometryHelper.ToCartesianCoordinates(radius, inclination, azimuth);
      Assert.AreEqual(new Vector3F(0, 0, 2), v);

      radius = 2;
      azimuth = 0;
      inclination = ConstantsF.PiOver2;
      v = GeometryHelper.ToCartesianCoordinates(radius, inclination, azimuth);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(2, 0, 0), v));

      radius = 2;
      azimuth = ConstantsF.PiOver2;
      inclination = ConstantsF.PiOver2;
      v = GeometryHelper.ToCartesianCoordinates(radius, inclination, azimuth);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 2, 0), v));

      radius = 2;
      azimuth = 0;
      inclination = ConstantsF.Pi;
      v = GeometryHelper.ToCartesianCoordinates(radius, inclination, azimuth);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, -2), v));

      radius = 2;
      azimuth = ConstantsF.Pi;
      inclination = ConstantsF.PiOver2;
      v = GeometryHelper.ToCartesianCoordinates(radius, inclination, azimuth);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-2, 0, 0), v));

      radius = 2;
      azimuth = -ConstantsF.PiOver2;
      inclination = ConstantsF.PiOver2;
      v = GeometryHelper.ToCartesianCoordinates(radius, inclination, azimuth);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -2, 0), v));
    }


    [Test]
    public void GetContactRayTriangle()
    {
      float hitDistance;
      bool hit;

      Ray ray = new Ray(new Vector3F(-1, 0, 0), Vector3F.UnitX, 2);
      Triangle triangle = new Triangle(new Vector3F(0, 0, 1), new Vector3F(0, 1, 0), new Vector3F(0, -1, -1));

      hit = GeometryHelper.GetContact(ray, triangle, true, out hitDistance);
      Assert.IsTrue(hit);
      Assert.IsTrue(Numeric.AreEqual(1, hitDistance));

      hit = GeometryHelper.GetContact(ray, triangle, false, out hitDistance);
      Assert.IsTrue(hit);
      Assert.IsTrue(Numeric.AreEqual(1, hitDistance));

      ray = new Ray(new Vector3F(-1, 0, 0), -Vector3F.UnitX, 2);
      hit = GeometryHelper.GetContact(ray, triangle, false, out hitDistance);
      Assert.IsFalse(hit);
      //Assert.IsTrue(Numeric.AreEqual(1, hitDistance));

      ray = new Ray(new Vector3F(1, 0, 0), -Vector3F.UnitX, 2);
      hit = GeometryHelper.GetContact(ray, triangle, false, out hitDistance);
      Assert.IsFalse(hit);
      //Assert.IsTrue(Numeric.AreEqual(1, hitDistance));

      ray = new Ray(new Vector3F(1, 0, 0), -Vector3F.UnitX, 2);
      hit = GeometryHelper.GetContact(ray, triangle, true, out hitDistance);
      Assert.IsTrue(hit);
      Assert.IsTrue(Numeric.AreEqual(1, hitDistance));

      ray = new Ray(new Vector3F(1, 1.1f, 0), -Vector3F.UnitX, 2);
      hit = GeometryHelper.GetContact(ray, triangle, true, out hitDistance);
      Assert.IsFalse(hit);
    }


    [Test]
    public void ExtractPlanes()
    {
      var view = Matrix44F.CreateRotation(RandomHelper.Random.NextQuaternionF())
                 * Matrix44F.CreateTranslation(RandomHelper.Random.NextVector3F(0, 1));

      var projection = Matrix44F.CreatePerspectiveFieldOfView(
        MathHelper.ToRadians(90),
        4.0f / 3.0f,
        0.1f,
        10f);

      var viewProjection = projection * view;

      var planes = new List<Plane>(6);
      GeometryHelper.ExtractPlanes(viewProjection, planes, true);

      // Note: This check works only for a small near-far range. The numerical computations
      // create a huge error.
      ComparePlanes(planes, viewProjection);
    }


    [Test]
    public void ExtractPlanesException()
    {
      var view = Matrix44F.CreateRotation(RandomHelper.Random.NextQuaternionF())
                 * Matrix44F.CreateTranslation(RandomHelper.Random.NextVector3F(0, 1));

      var projection = Matrix44F.CreatePerspectiveFieldOfView(
        MathHelper.ToRadians(90),
        4.0f / 3.0f,
        0.1f,
        10000f);

      var viewProjection = projection * view;

      var planes = new List<Plane>(6);
      Assert.Throws<DivideByZeroException>(() => GeometryHelper.ExtractPlanes(viewProjection, planes, true));
    }


    private static void ComparePlanes(List<Plane> planes, Matrix44F viewProjection)
    {
      var viewProjectionInverse = viewProjection.Inverse;

      var nearPlane = new Plane(
        viewProjectionInverse.TransformPosition(new Vector3F(1, -1, 0)),
        viewProjectionInverse.TransformPosition(new Vector3F(1, 1, 0)),
        viewProjectionInverse.TransformPosition(new Vector3F(-1, 1, 0)));
      ComparePlanes(nearPlane, planes[0]);

      var farPlane = new Plane(
        viewProjectionInverse.TransformPosition(new Vector3F(1, 1, 1)),
        viewProjectionInverse.TransformPosition(new Vector3F(1, -1, 1)),
        viewProjectionInverse.TransformPosition(new Vector3F(-1, -1, 1)));
      ComparePlanes(farPlane, planes[1]);

      var leftPlane = new Plane(
        viewProjectionInverse.TransformPosition(new Vector3F(-1, -1, 0)),
        viewProjectionInverse.TransformPosition(new Vector3F(-1, 1, 1)),
        viewProjectionInverse.TransformPosition(new Vector3F(-1, -1, 1)));
      ComparePlanes(leftPlane, planes[2]);

      var rightPlane = new Plane(
        viewProjectionInverse.TransformPosition(new Vector3F(1, -1, 1)),
        viewProjectionInverse.TransformPosition(new Vector3F(1, 1, 1)),
        viewProjectionInverse.TransformPosition(new Vector3F(1, 1, 0)));
      ComparePlanes(rightPlane, planes[3]);

      var bottomPlane = new Plane(
        viewProjectionInverse.TransformPosition(new Vector3F(1, -1, 0)),
        viewProjectionInverse.TransformPosition(new Vector3F(-1, -1, 1)),
        viewProjectionInverse.TransformPosition(new Vector3F(1, -1, 1)));
      ComparePlanes(bottomPlane, planes[4]);

      var topPlane = new Plane(
        viewProjectionInverse.TransformPosition(new Vector3F(1, 1, 0)),
        viewProjectionInverse.TransformPosition(new Vector3F(1, 1, 1)),
        viewProjectionInverse.TransformPosition(new Vector3F(-1, 1, 1)));
      ComparePlanes(topPlane, planes[5]);
    }


    private static void ComparePlanes(Plane plane0, Plane plane1)
    {
      Assert.IsTrue(Numeric.AreEqual(plane0.DistanceFromOrigin, plane1.DistanceFromOrigin));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(plane0.Normal, plane1.Normal));
    }


    [Test]
    public void GetBarycentric()
    {
      RandomHelper.Random = new Random(1234567);
      for (int i = 0; i < 100; i++)
      {
        var t = new Triangle(RandomHelper.Random.NextVector3F(-100, 100), RandomHelper.Random.NextVector3F(-100, 100), RandomHelper.Random.NextVector3F(-100, 100));
        var p = RandomHelper.Random.NextVector3F(-100, 100);

        float u, v, w;
        GeometryHelper.GetBarycentricFromPoint(t, p, out u, out v, out w);

        float u2, v2, w2;
        GeometryHelper.GetBarycentricFromPoint(ref t, ref p, out u2, out v2, out w2);

        Assert.AreEqual(u, u2);
        Assert.AreEqual(v, v2);
        Assert.AreEqual(w, w2);

        var o = GeometryHelper.GetPointFromBarycentric(t, u, v, w);

        // p - o must have the same direction as the normal.
        var n = t.Normal;
        var po = (p - o);
        if (!po.TryNormalize())
          po = n;

        float epsilon = Numeric.EpsilonF * 10;
        Assert.IsTrue(Vector3F.AreNumericallyEqual(po, n, epsilon) || Vector3F.AreNumericallyEqual(po, -n, epsilon));
      }
    }


    [Test]
    public void GetLineParametersParallelLineSegments()
    {
      var a = new LineSegment(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0));
      var b = new LineSegment(new Vector3F(2, 0, 0), new Vector3F(3, 0, 0));
      float s, t;
      GeometryHelper.GetLineParameters(a, b, out s, out t);
      Assert.AreEqual(2, s);
      Assert.AreEqual(0, t);

      a = new LineSegment(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0));
      b = new LineSegment(new Vector3F(1, 0, 0), new Vector3F(3, 0, 0));
      GeometryHelper.GetLineParameters(a, b, out s, out t);
      Assert.AreEqual(1, s);
      Assert.AreEqual(0, t);

      a = new LineSegment(new Vector3F(0, 0, 0), new Vector3F(4, 0, 0));
      b = new LineSegment(new Vector3F(1, 0, 0), new Vector3F(3, 0, 0));
      GeometryHelper.GetLineParameters(a, b, out s, out t);
      Assert.AreEqual(0.25f, s);
      Assert.AreEqual(0, t);

      a = new LineSegment(new Vector3F(1, 0, 0), new Vector3F(2, 0, 0));
      b = new LineSegment(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0));
      GeometryHelper.GetLineParameters(a, b, out s, out t);
      Assert.AreEqual(0, s);
      Assert.AreEqual(1, t);

      a = new LineSegment(new Vector3F(2, 0, 0), new Vector3F(3, 0, 0));
      b = new LineSegment(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0));
      GeometryHelper.GetLineParameters(a, b, out s, out t);
      Assert.AreEqual(-1, s);
      Assert.AreEqual(1, t);

      a = new LineSegment(new Vector3F(1, 0, 0), new Vector3F(2, 0, 0));
      b = new LineSegment(new Vector3F(0, 0, 0), new Vector3F(2, 0, 0));
      GeometryHelper.GetLineParameters(a, b, out s, out t);
      Assert.AreEqual(0, s);
      Assert.AreEqual(0.5f, t);
    }
  }
}
