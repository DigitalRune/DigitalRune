using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Scene3D;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Graphics.Tests
{
  [TestFixture]
  public class OrthographicProjectionTest
  {
    [Test]
    public void OrthographicTest()
	  {
      Vector3F position = new Vector3F(1, 2, 3);
      QuaternionF orientation = QuaternionF.CreateRotation(new Vector3F(2, 3, 6), 0.123f);
      CameraInstance cameraInstance = new CameraInstance(new Camera(new OrthographicProjection()))
      {
        PoseLocal = new Pose(position, orientation),
      };

      ((OrthographicProjection)cameraInstance.Camera.Projection).Set(4, 3, 2, 10);
      Matrix44F projection = Matrix44F.CreateOrthographic(4, 3, 2, 10);
      Assert.AreEqual(position, cameraInstance.PoseWorld.Position);
      Assert.AreEqual(orientation.ToRotationMatrix33(), cameraInstance.PoseWorld.Orientation);
      Assert.AreEqual(4, cameraInstance.Camera.Projection.Width);
      Assert.AreEqual(3, cameraInstance.Camera.Projection.Height);
      Assert.AreEqual(4.0f / 3.0f, cameraInstance.Camera.Projection.AspectRatio);
      Assert.AreEqual(2, cameraInstance.Camera.Projection.Near);
      Assert.AreEqual(10, cameraInstance.Camera.Projection.Far);
      Assert.AreEqual(-2, cameraInstance.Camera.Projection.Left);
      Assert.AreEqual(2, cameraInstance.Camera.Projection.Right);
      Assert.AreEqual(-1.5f, cameraInstance.Camera.Projection.Bottom);
      Assert.AreEqual(1.5f, cameraInstance.Camera.Projection.Top);
      Assert.AreEqual(8, cameraInstance.Camera.Projection.Depth);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection, cameraInstance.Camera.Projection));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection.Inverse, cameraInstance.Camera.Projection.Inverse));
      Assert.IsNotNull(cameraInstance.BoundingShape);

      // Test shape using collision detection. Remove rotation to simplify test.
      cameraInstance.PoseWorld = new Pose(cameraInstance.PoseWorld.Position);
      CollisionDetection collisionDetection = new CollisionDetection();
      var point = new PointShape();
      CollisionObject pointCollisionObject = new CollisionObject(new GeometricObject(point));
      CollisionObject cameraCollisionObject = new CollisionObject(cameraInstance);

      point.Position = position + new Vector3F(-2, -1.5f, -2);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(-2.1f, -1.6f, -1.9f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(2, 1.5f, -10);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(2.1f, 1.6f, -10.1f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));

      cameraInstance.PoseWorld = new Pose(position, orientation);
      ((OrthographicProjection)cameraInstance.Camera.Projection).Set(8, 4, 1, 100);
      cameraInstance.Camera.Projection.Near = 1;
      cameraInstance.Camera.Projection.Far = 100;
      projection = Matrix44F.CreateOrthographic(8, 4, 1, 100);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection.Inverse, cameraInstance.Camera.Projection.Inverse));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection, cameraInstance.Camera.Projection));

      // Test shape using collision detection. Remove rotation to simplify test.
      cameraInstance.PoseWorld = new Pose(cameraInstance.PoseWorld.Position);
      point.Position = position + new Vector3F(-4, -2f, -1);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(-4.1f, -1.9f, -0.9f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(4, 2f, -100);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(4.1f, 2.1f, -100.1f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
    }

    [Test]
    public void OrthographicOffCenterTest()
    {
      Vector3F position = new Vector3F(1, 2, 3);
      QuaternionF orientation = QuaternionF.CreateRotation(new Vector3F(2, 3, 6), 0.123f);
      CameraInstance cameraInstance = new CameraInstance(new Camera(new OrthographicProjection()))
      {
        PoseLocal = new Pose(position, orientation),
      };

      ((OrthographicProjection)cameraInstance.Camera.Projection).SetOffCenter(0, 16, 0, 9, 2, 10);
      Matrix44F projection = Matrix44F.CreateOrthographicOffCenter(0, 16, 0, 9, 2, 10);
      Assert.AreEqual(position, cameraInstance.PoseWorld.Position);
      Assert.AreEqual(orientation.ToRotationMatrix33(), cameraInstance.PoseWorld.Orientation);
      Assert.AreEqual(16, cameraInstance.Camera.Projection.Width);
      Assert.AreEqual(9, cameraInstance.Camera.Projection.Height);
      Assert.AreEqual(16.0f / 9.0f, cameraInstance.Camera.Projection.AspectRatio);
      Assert.AreEqual(2, cameraInstance.Camera.Projection.Near);
      Assert.AreEqual(10, cameraInstance.Camera.Projection.Far);
      Assert.AreEqual(0, cameraInstance.Camera.Projection.Left);
      Assert.AreEqual(16, cameraInstance.Camera.Projection.Right);
      Assert.AreEqual(0, cameraInstance.Camera.Projection.Bottom);
      Assert.AreEqual(9, cameraInstance.Camera.Projection.Top);
      Assert.AreEqual(8, cameraInstance.Camera.Projection.Depth);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection, cameraInstance.Camera.Projection));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection.Inverse, cameraInstance.Camera.Projection.Inverse));
      Assert.IsNotNull(cameraInstance.BoundingShape);

      CollisionDetection collisionDetection = new CollisionDetection();
      var point = new PointShape();
      CollisionObject pointCollisionObject = new CollisionObject(new GeometricObject(point));
      CollisionObject cameraCollisionObject = new CollisionObject(cameraInstance);

      // Test shape using collision detection. Remove rotation to simplify test.
      cameraInstance.PoseWorld = new Pose(cameraInstance.PoseWorld.Position);
      point.Position = position + new Vector3F(0, 0, -2);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(-0.1f, -0.1f, -1.9f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(16, 9, -10);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(16.1f, 9.1f, -10.1f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));

      cameraInstance.Camera.Projection.Left = -2;
      cameraInstance.Camera.Projection.Right = 4;
      cameraInstance.Camera.Projection.Bottom = 10;
      cameraInstance.Camera.Projection.Top = 14;
      cameraInstance.Camera.Projection.Near = 15;
      cameraInstance.Camera.Projection.Far = 100;
      projection = Matrix44F.CreateOrthographicOffCenter(-2, 4, 10, 14, 15, 100);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection.Inverse, cameraInstance.Camera.Projection.Inverse));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection, cameraInstance.Camera.Projection));

      // Test shape using collision detection. Remove rotation to simplify test.
      cameraInstance.PoseWorld = new Pose(cameraInstance.PoseWorld.Position);
      point.Position = position + new Vector3F(-2, 10, -15);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(-2.1f, -10.1f, -14.9f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(4, 14, -100);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(4.1f, 14.1f, -100.1f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
    }

    [Test]
    public void SetProjectionTest()
    {
      OrthographicProjection projection = new OrthographicProjection();
      projection.Set(4, 3, 2, 10);

      OrthographicProjection camera2 = new OrthographicProjection();
      camera2.Set(4, 3);
      camera2.Near = 2;
      camera2.Far = 10;

      OrthographicProjection camera3 = new OrthographicProjection
      {
        Left = -2,
        Right = 2,
        Bottom = -1.5f,
        Top = 1.5f,
        Near = 2,
        Far = 10,
      };

      Matrix44F expected = Matrix44F.CreateOrthographic(4, 3, 2, 10);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, projection));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, camera2));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, camera3.ToMatrix44F()));
    }

    [Test]
    public void SetProjectionOffCenterTest()
    {
      OrthographicProjection projection = new OrthographicProjection();
      projection.SetOffCenter(0, 4, 1, 4, 2, 10);

      OrthographicProjection camera2 = new OrthographicProjection();
      camera2.SetOffCenter(0, 4, 1, 4);
      camera2.Near = 2;
      camera2.Far = 10;

      Projection camera3 = new OrthographicProjection
      {
        Left = 0,
        Right = 4,
        Bottom = 1,
        Top = 4,
        Near = 2,
        Far = 10,
      };

      Matrix44F expected = Matrix44F.CreateOrthographicOffCenter(0, 4, 1, 4, 2, 10);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, projection));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, camera2));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, camera3.ToMatrix44F()));
    }
  }
}
