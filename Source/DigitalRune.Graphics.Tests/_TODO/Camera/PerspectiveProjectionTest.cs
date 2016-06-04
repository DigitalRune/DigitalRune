using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Scene3D;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Graphics.Tests
{
  [TestFixture]
  public class PerspectiveProjectionTest
  {
    [Test]
    public void GetWidthAndHeightTest()
    {
      float width, height;
      PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(90), 1, 1, out width, out height);
      Assert.IsTrue(Numeric.AreEqual(2, width));
      Assert.IsTrue(Numeric.AreEqual(2, height));

      PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, out width, out height);
      Assert.IsTrue(Numeric.AreEqual(2.0528009f, width));
      Assert.IsTrue(Numeric.AreEqual(1.1547005f, height));

      // We are pretty confident that the Projection.CreateProjectionXxx() works. 
      // Use Projection.CreateProjectionXxx() to test GetWidthAndHeight().
      Matrix44F projection = Matrix44F.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);
      Matrix44F projection2 = Matrix44F.CreatePerspective(width, height, 1, 10);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection, projection2));
    }

    [Test]
    public void PerspectiveTest()
    {
      Vector3F position = new Vector3F(1, 2, 3);
      QuaternionF orientation = QuaternionF.CreateRotation(new Vector3F(2, 3, 6), 0.123f);
      CameraInstance cameraInstance = new CameraInstance(new Camera(new PerspectiveProjection()))
      {
        PoseLocal = new Pose(position, orientation),
      };

      ((PerspectiveProjection)cameraInstance.Camera.Projection).SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 10.0f, 1, 10);
      Matrix44F projection = Matrix44F.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16.0f / 10.0f, 1, 10);
      Assert.AreEqual(position, cameraInstance.PoseWorld.Position);
      Assert.AreEqual(orientation.ToRotationMatrix33(), cameraInstance.PoseWorld.Orientation);
      Assert.AreEqual(MathHelper.ToRadians(60), cameraInstance.Camera.Projection.FieldOfViewY);
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(85.4601055f), cameraInstance.Camera.Projection.FieldOfViewX));
      Assert.AreEqual(16.0f / 10.0f, cameraInstance.Camera.Projection.AspectRatio);
      Assert.IsTrue(Numeric.AreEqual(1.8475209f, cameraInstance.Camera.Projection.Width));
      Assert.IsTrue(Numeric.AreEqual(1.1547005f, cameraInstance.Camera.Projection.Height));
      Assert.AreEqual(1, cameraInstance.Camera.Projection.Near);
      Assert.AreEqual(10, cameraInstance.Camera.Projection.Far);
      Assert.IsTrue(Numeric.AreEqual(-0.9237604f, cameraInstance.Camera.Projection.Left));
      Assert.IsTrue(Numeric.AreEqual(0.9237604f, cameraInstance.Camera.Projection.Right));
      Assert.IsTrue(Numeric.AreEqual(-0.5773503f, cameraInstance.Camera.Projection.Bottom));
      Assert.IsTrue(Numeric.AreEqual(0.5773503f, cameraInstance.Camera.Projection.Top));
      Assert.AreEqual(9, cameraInstance.Camera.Projection.Depth);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection, cameraInstance.Camera.Projection));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection.Inverse, cameraInstance.Camera.Projection.Inverse));
      Assert.IsNotNull(cameraInstance.BoundingShape);

      // Test shape using collision detection. Remove rotation to simplify test.
      cameraInstance.PoseWorld = new Pose(cameraInstance.PoseWorld.Position);
      CollisionDetection collisionDetection = new CollisionDetection();
      var point = new PointShape();
      CollisionObject pointCollisionObject = new CollisionObject(new GeometricObject(point));
      CollisionObject cameraCollisionObject = new CollisionObject(cameraInstance);

      point.Position = position + new Vector3F(0, 0, -1);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(0, 0, -10);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(0, 0, -0.9f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(0, 0, -10.1f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));

      point.Position = position + new Vector3F(-0.9237604f, -0.5773f, -1);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(-0.924f, -0.5773f, -1);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(-0.9237604f, -0.58f, -1);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(0.9237604f, 0.5773f, -1);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(0.924f, 0.5773f, -1);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(0.9237604f, 0.58f, -1);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));

      point.Position = position + new Vector3F(-9.237604f, -5.773f, -10);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(-9.24f, -5.773f, -10);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(-9.237604f, -5.8f, -10);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(9.237604f, 5.773f, -10);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(9.24f, 5.773f, -10);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(9.237604f, 5.8f, -10);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
    }

    [Test]
    public void PerspectiveOffCenterTest()
    {
      Vector3F position = new Vector3F(1, 2, 3);
      QuaternionF orientation = QuaternionF.CreateRotation(new Vector3F(2, 3, 6), 0.123f);
      CameraInstance cameraInstance = new CameraInstance(new Camera(new PerspectiveProjection()))
      {
        PoseLocal = new Pose(position, orientation),
      };
      ((PerspectiveProjection)cameraInstance.Camera.Projection).SetOffCenter(1, 5, 2, 5, 1, 10);

      Matrix44F projection = Matrix44F.CreatePerspectiveOffCenter(1, 5, 2, 5, 1, 10);
      Assert.AreEqual(position, cameraInstance.PoseWorld.Position);
      Assert.AreEqual(orientation.ToRotationMatrix33(), cameraInstance.PoseWorld.Orientation);
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(33.690067f), cameraInstance.Camera.Projection.FieldOfViewX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(15.255119f), cameraInstance.Camera.Projection.FieldOfViewY));
      Assert.AreEqual(4.0f / 3.0f, cameraInstance.Camera.Projection.AspectRatio);
      Assert.AreEqual(4, cameraInstance.Camera.Projection.Width);
      Assert.AreEqual(3, cameraInstance.Camera.Projection.Height);
      Assert.AreEqual(1, cameraInstance.Camera.Projection.Left);
      Assert.AreEqual(5, cameraInstance.Camera.Projection.Right);
      Assert.AreEqual(2, cameraInstance.Camera.Projection.Bottom);
      Assert.AreEqual(5, cameraInstance.Camera.Projection.Top);
      Assert.AreEqual(1, cameraInstance.Camera.Projection.Near);
      Assert.AreEqual(10, cameraInstance.Camera.Projection.Far);
      Assert.AreEqual(9, cameraInstance.Camera.Projection.Depth);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection, cameraInstance.Camera.Projection));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(projection.Inverse, cameraInstance.Camera.Projection.Inverse));
      Assert.IsNotNull(cameraInstance.BoundingShape);

      // Test shape using collision detection. Remove rotation to simplify test.
      cameraInstance.PoseWorld = new Pose(cameraInstance.PoseWorld.Position);
      CollisionDetection collisionDetection = new CollisionDetection();
      var point = new PointShape();
      CollisionObject pointCollisionObject = new CollisionObject(new GeometricObject(point));
      CollisionObject cameraCollisionObject = new CollisionObject(cameraInstance);

      point.Position = position + new Vector3F(3, 3, -1);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(30, 30, -10);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(3, 3, -0.9f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(30, 30, -10.1f);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));

      point.Position = position + new Vector3F(1, 2, -1);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(0.9f, 2, -1);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(1, 1.9f, -1);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(5, 5, -1);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(5.1f, 5, -1);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(5, 5.1f, -1);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));

      point.Position = position + new Vector3F(10, 20, -10);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(9.9f, 20, -10);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(10, 19.9f, -10);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(50, 50, -10);
      Assert.IsTrue(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(50.1f, 50, -10);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
      point.Position = position + new Vector3F(50, 50.1f, -10);
      Assert.IsFalse(collisionDetection.HaveContact(pointCollisionObject, cameraCollisionObject));
    }

    [Test]
    public void SetProjectionTest()
    {
      PerspectiveProjection projection = new PerspectiveProjection();
      projection.Set(4, 3, 2, 10);

      PerspectiveProjection projection2 = new PerspectiveProjection();
      projection2.Set(4, 3);
      projection2.Near = 2;
      projection2.Far = 10;

      Projection projection3 = new PerspectiveProjection
      {
        Left = -2,
        Right = 2,
        Bottom = -1.5f,
        Top = 1.5f,
        Near = 2,
        Far = 10,
      };

      Matrix44F expected = Matrix44F.CreatePerspective(4, 3, 2, 10);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, projection));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, projection2));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, projection3.ToMatrix44F()));
    }

    [Test]
    public void SetProjectionFieldOfViewTest()
    {
      PerspectiveProjection projection = new PerspectiveProjection();
      projection.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);

      PerspectiveProjection projection2 = new PerspectiveProjection();
      projection2.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f);
      projection2.Near = 1;
      projection2.Far = 10;

      Projection projection3 = new PerspectiveProjection
      {
        Left = -2.0528009f / 2.0f,
        Right = 2.0528009f / 2.0f,
        Bottom = -1.1547005f / 2.0f,
        Top = 1.1547005f / 2.0f,
        Near = 1,
        Far = 10,
      };

      Matrix44F expected = Matrix44F.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, projection));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, projection2));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, projection3.ToMatrix44F()));
    }

    [Test]
    public void SetProjectionOffCenterTest()
    {
      PerspectiveProjection projection = new PerspectiveProjection();
      projection.SetOffCenter(0, 4, 1, 4, 2, 10);

      PerspectiveProjection projection2 = new PerspectiveProjection();
      projection2.SetOffCenter(0, 4, 1, 4);
      projection2.Near = 2;
      projection2.Far = 10;

      Matrix44F expected = Matrix44F.CreatePerspectiveOffCenter(0, 4, 1, 4, 2, 10);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, projection));
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(expected, projection2.ToMatrix44F()));
    }  
  }
}
