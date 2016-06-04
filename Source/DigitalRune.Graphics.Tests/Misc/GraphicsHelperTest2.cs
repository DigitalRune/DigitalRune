using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics.Tests
{
  [TestFixture]
  public class GraphicsHelperTest2
  {
    [Test]
    public void ProjectTest()
    {
      Viewport viewport = new Viewport(0, 0, 640, 480);
      PerspectiveProjection projection = new PerspectiveProjection();
      projection.SetFieldOfView(MathHelper.ToRadians(60), viewport.AspectRatio, 10, 1000);
      Matrix44F view = Matrix44F.CreateLookAt(new Vector3F(0, 0, 0), new Vector3F(0, 0, -1), Vector3F.Up);

      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(320, 240, 0), viewport.Project(new Vector3F(0, 0, -10), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), viewport.Project(new Vector3F(projection.Left, projection.Top, -10), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(640, 0, 0), viewport.Project(new Vector3F(projection.Right, projection.Top, -10), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 480, 0), viewport.Project(new Vector3F(projection.Left, projection.Bottom, -10), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(640, 480, 0), viewport.Project(new Vector3F(projection.Right, projection.Bottom, -10), projection, view)));

      Vector3[] farCorners = new Vector3[4];
      GraphicsHelper.GetFrustumFarCorners(projection, farCorners);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(320, 240, 1), viewport.Project(new Vector3F(0, 0, -1000), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 1), viewport.Project((Vector3F)farCorners[0], projection, view), 1e-4f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(640, 0, 1), viewport.Project((Vector3F)farCorners[1], projection, view), 1e-4f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 480, 1), viewport.Project((Vector3F)farCorners[2], projection, view), 1e-4f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(640, 480, 1), viewport.Project((Vector3F)farCorners[3], projection, view), 1e-4f));
    }



    [Test]
    public void UnprojectTest()
    {
      Viewport viewport = new Viewport(0, 0, 640, 480);
      PerspectiveProjection projection = new PerspectiveProjection();
      projection.SetFieldOfView(MathHelper.ToRadians(60), viewport.AspectRatio, 10, 1000);
      Matrix44F view = Matrix44F.CreateLookAt(new Vector3F(0, 0, 0), new Vector3F(0, 0, -1), Vector3F.Up);

      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, -10), viewport.Unproject(new Vector3F(320, 240, 0), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(projection.Left, projection.Top, -10), viewport.Unproject(new Vector3F(0, 0, 0), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(projection.Right, projection.Top, -10), viewport.Unproject(new Vector3F(640, 0, 0), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(projection.Left, projection.Bottom, -10), viewport.Unproject(new Vector3F(0, 480, 0), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(projection.Right, projection.Bottom, -10), viewport.Unproject(new Vector3F(640, 480, 0), projection, view)));

      Vector3[] farCorners = new Vector3[4];
      GraphicsHelper.GetFrustumFarCorners(projection, farCorners);
      Assert.IsTrue(Vector3F.AreNumericallyEqual((Vector3F)farCorners[0], viewport.Unproject(new Vector3F(0, 0, 1), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual((Vector3F)farCorners[1], viewport.Unproject(new Vector3F(640, 0, 1), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual((Vector3F)farCorners[2], viewport.Unproject(new Vector3F(0, 480, 1), projection, view)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual((Vector3F)farCorners[3], viewport.Unproject(new Vector3F(640, 480, 1), projection, view)));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetScreenSizeException()
    {
      var viewport = new Viewport(10, 10, 200, 100);
      var geometricObject = new GeometricObject(new SphereShape());
      GraphicsHelper.GetScreenSize(null, viewport, geometricObject);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetScreenSizeException2()
    {
      var cameraNode = new CameraNode(new Camera(new PerspectiveProjection()));
      var viewport = new Viewport(10, 10, 200, 100);
      GraphicsHelper.GetScreenSize(cameraNode, viewport, null);
    }


    [Test]
    public void GetScreenSizeWithPerspective()
    {
      // Camera
      var projection = new PerspectiveProjection();
      projection.SetFieldOfView(MathHelper.ToRadians(90), 2.0f / 1.0f, 1.0f, 100f);
      var camera = new Camera(projection);
      var cameraNode = new CameraNode(camera);
      cameraNode.PoseWorld = new Pose(new Vector3F(123, 456, -789), Matrix33F.CreateRotation(new Vector3F(1, -2, 3), MathHelper.ToRadians(75)));

      // 2:1 viewport
      var viewport = new Viewport(10, 10, 200, 100);
      
      // Test object
      var shape = new SphereShape();
      var geometricObject = new GeometricObject(shape);

      // Empty sphere at camera position.
      shape.Radius = 0;
      geometricObject.Pose = cameraNode.PoseWorld;
      Vector2F screenSize = GraphicsHelper.GetScreenSize(cameraNode, viewport, geometricObject);
      Assert.AreEqual(0, screenSize.X);
      Assert.AreEqual(0, screenSize.Y);

      // Empty sphere centered at near plane.
      shape.Radius = 0;
      geometricObject.Pose = cameraNode.PoseWorld * new Pose(new Vector3F(0.123f, -0.543f, -1));
      screenSize = GraphicsHelper.GetScreenSize(cameraNode, viewport, geometricObject);
      Assert.AreEqual(0, screenSize.X);
      Assert.AreEqual(0, screenSize.Y);

      // Create sphere which as a bounding sphere of ~1 unit diameter:
      // Since the bounding sphere is based on the AABB, we need to make the 
      // actual sphere a bit smaller.
      shape.Radius = 1 / (2 * (float)Math.Sqrt(3)) + Numeric.EpsilonF;

      // Sphere at camera position.
      geometricObject.Pose = cameraNode.PoseWorld;
      screenSize = GraphicsHelper.GetScreenSize(cameraNode, viewport, geometricObject);
      Assert.Greater(screenSize.X, 200);
      Assert.Greater(screenSize.Y, 100);

      // Sphere at near plane.
      geometricObject.Pose = cameraNode.PoseWorld * new Pose(new Vector3F(0.123f, -0.543f, -1));
      screenSize = GraphicsHelper.GetScreenSize(cameraNode, viewport, geometricObject);
      Assert.IsTrue(Numeric.AreEqual(screenSize.X, 50.0f, 10f));
      Assert.IsTrue(Numeric.AreEqual(screenSize.Y, 50.0f, 10f));

      // Double distance --> half size
      geometricObject.Pose = cameraNode.PoseWorld * new Pose(new Vector3F(0.123f, -0.543f, -2));
      screenSize = GraphicsHelper.GetScreenSize(cameraNode, viewport, geometricObject);
      Assert.IsTrue(Numeric.AreEqual(screenSize.X, 25.0f, 5f));
      Assert.IsTrue(Numeric.AreEqual(screenSize.Y, 25.0f, 5f));
    }


    [Test]
    public void GetScreenSizeWithOrthographic()
    {
      // Camera
      var projection = new OrthographicProjection();
      projection.SetOffCenter(0, 4, 0, 2);
      var camera = new Camera(projection);
      var cameraNode = new CameraNode(camera);
      cameraNode.PoseWorld = new Pose(new Vector3F(123, 456, -789), Matrix33F.CreateRotation(new Vector3F(1, -2, 3), MathHelper.ToRadians(75)));

      // 2:1 viewport
      var viewport = new Viewport(10, 10, 200, 100);

      // Test object
      var shape = new SphereShape();
      var geometricObject = new GeometricObject(shape);

      // Empty sphere at camera position.
      shape.Radius = 0;
      geometricObject.Pose = cameraNode.PoseWorld;
      Vector2F screenSize = GraphicsHelper.GetScreenSize(cameraNode, viewport, geometricObject);
      Assert.AreEqual(0, screenSize.X);
      Assert.AreEqual(0, screenSize.Y);

      // Empty sphere centered at near plane.
      shape.Radius = 0;
      geometricObject.Pose = cameraNode.PoseWorld * new Pose(new Vector3F(0.123f, -0.543f, -1));
      screenSize = GraphicsHelper.GetScreenSize(cameraNode, viewport, geometricObject);
      Assert.AreEqual(0, screenSize.X);
      Assert.AreEqual(0, screenSize.Y);

      // Create sphere which as a bounding sphere of ~1 unit diameter:
      // Since the bounding sphere is based on the AABB, we need to make the 
      // actual sphere a bit smaller.
      shape.Radius = 1 / (2 * (float)Math.Sqrt(3)) + Numeric.EpsilonF;

      // Sphere at camera position.
      geometricObject.Pose = cameraNode.PoseWorld;
      screenSize = GraphicsHelper.GetScreenSize(cameraNode, viewport, geometricObject);
      Assert.IsTrue(Numeric.AreEqual(screenSize.X, 50.0f, 10f));
      Assert.IsTrue(Numeric.AreEqual(screenSize.Y, 50.0f, 10f));

      // Sphere at near plane.
      geometricObject.Pose = cameraNode.PoseWorld * new Pose(new Vector3F(0.123f, -0.543f, -1));
      screenSize = GraphicsHelper.GetScreenSize(cameraNode, viewport, geometricObject);
      Assert.IsTrue(Numeric.AreEqual(screenSize.X, 50.0f, 10f));
      Assert.IsTrue(Numeric.AreEqual(screenSize.Y, 50.0f, 10f));

      // Double distance --> same size
      geometricObject.Pose = cameraNode.PoseWorld * new Pose(new Vector3F(0.123f, -0.543f, -2));
      screenSize = GraphicsHelper.GetScreenSize(cameraNode, viewport, geometricObject);
      Assert.IsTrue(Numeric.AreEqual(screenSize.X, 50.0f, 10f));
      Assert.IsTrue(Numeric.AreEqual(screenSize.Y, 50.0f, 10f));
    }
  }
}
