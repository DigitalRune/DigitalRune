using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Constraints;
using NUnit.Framework;


namespace DigitalRune.Physics.Constraints.Tests
{
  [TestFixture]
  public class ConstraintHelperTest
  {
    [Test]
    public void SpringDampingTest()
    {
      float erp = 0.3f;
      float cfm = 0.001f;
      
      float spring = ConstraintHelper.ComputeSpringConstant(1 / 60f, erp, cfm);
      float damping = ConstraintHelper.ComputeDampingConstant(1 / 60f, erp, cfm);
      
      Assert.IsTrue(Numeric.AreEqual(erp, ConstraintHelper.ComputeErrorReduction(1 / 60f, spring, damping)));
      Assert.IsTrue(Numeric.AreEqual(cfm, ConstraintHelper.ComputeSoftness(1 / 60f, spring, damping)));
    }

    [Test]
    public void ComputeKMatrix()
    {
      var b = new RigidBody(new EmptyShape());

      b.MassFrame = new MassFrame()
      {
        Mass = 3,
        Inertia = new Vector3F(0.4f, 0.5f, 0.6f),
      };

      var r = new Vector3F(1, 2, 3);
      var k = ConstraintHelper.ComputeKMatrix(b, r);

      var desiredK = 1 / b.MassFrame.Mass * Matrix33F.Identity - r.ToCrossProductMatrix() * Matrix33F.CreateScale(b.MassFrame.InertiaInverse) * r.ToCrossProductMatrix();

      Assert.AreEqual(desiredK, k);
    }


    [Test]
    public void SetVelocityTest()
    {
      var body = new RigidBody(new BoxShape(1, 2, 3));

      body.Pose = new Pose(new Vector3F(10, 20, 30), QuaternionF.CreateRotationY(1.1f));
      body.LinearVelocity = new Vector3F(1, 2, 3);
      body.AngularVelocity = new Vector3F(4, 5, 6);

      Vector3F pointLocal = new Vector3F(0.5f, 0.9f, 1.3f);
      Vector3F point = body.Pose.ToWorldPosition(pointLocal);
      Vector3F targetVelocity = new Vector3F(7, -8, 9);
      Assert.AreNotEqual(targetVelocity, body.GetVelocityOfLocalPoint(pointLocal));
      
      ConstraintHelper.SetVelocityOfWorldPoint(body, point, targetVelocity);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(targetVelocity, body.GetVelocityOfLocalPoint(pointLocal)));
    }
  }
}
