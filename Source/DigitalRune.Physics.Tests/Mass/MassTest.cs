using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Physics.Tests
{
  [TestFixture]
  public class MassTest
  {
    [SetUp]
    public void SetUp()
    {
      RandomHelper.Random = new Random(1234567);
    }


    [Test]
    public void BoxMass()
    {
      var b = new BoxShape(1, 2, 3);
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(b, new Vector3F(1, -2, -3), 1, true, 0.001f, 10, out m0, out com0, out i0);

      var m = b.GetMesh(0.1f, 1);
      m.Transform(Matrix44F.CreateScale(1, -2, -3));
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(m, out m1, out com1, out i1);

      Assert.AreEqual(m0, m1);
      Assert.AreEqual(com0, com1);
      Assert.AreEqual(i0, i1);

      // Try other density.
      float m2;
      Vector3F com2;
      Matrix33F i2;
      MassHelper.GetMass(b, new Vector3F(1, -2, -3), 0.7f, true, 0.001f, 10, out m2, out com2, out i2);
      Assert.AreEqual(m0 * 0.7f, m2);
      Assert.AreEqual(com0, com2);
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 0.7f, i2));

      const float e = 0.01f;

      // Try with target mass.
      float m3;
      Vector3F com3;
      Matrix33F i3;
      MassHelper.GetMass(b, new Vector3F(1, -2, -3), 23, false, 0.001f, 10, out m3, out com3, out i3);
      Assert.IsTrue(Numeric.AreEqual(23, m3, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com3, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 23 / m0, i3, e * (1 + i0.Trace)));
    }


    [Test]
    public void ConeMass()
    {
      var s = new ConeShape(1, 2);
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 1, true, 0.001f, 10, out m0, out com0, out i0);

      var m = s.GetMesh(0.001f, 10);
      m.Transform(Matrix44F.CreateScale(1, -2, -3));
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(m, out m1, out com1, out i1);

      const float e = 0.01f;
      Assert.IsTrue(Numeric.AreEqual(m0, m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, i1, e * (1 + i0.Trace)));

      // Try other density.
      float m2;
      Vector3F com2;
      Matrix33F i2;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 0.7f, true, 0.001f, 10, out m2, out com2, out i2);
      Assert.IsTrue(Numeric.AreEqual(m0 * 0.7f, m2, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com2, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 0.7f, i2, e * (1 + i0.Trace)));

      // Try with target mass.
      float m3;
      Vector3F com3;
      Matrix33F i3;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 23, false, 0.001f, 10, out m3, out com3, out i3);
      Assert.IsTrue(Numeric.AreEqual(23, m3, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com3, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 23 / m0, i3, e * (1 + i0.Trace)));
    }


    [Test]
    public void SphereMass()
    {
      var s = new SphereShape(2);
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 1, true, 0.001f, 10, out m0, out com0, out i0);

      var m = s.GetMesh(0.001f, 10);
      m.Transform(Matrix44F.CreateScale(1, -2, -3));
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(m, out m1, out com1, out i1);

      const float e = 0.01f;
      Assert.IsTrue(Numeric.AreEqual(m0, m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, i1, e * (1 + i0.Trace)));

      // Try other density.
      float m2;
      Vector3F com2;
      Matrix33F i2;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 0.7f, true, 0.001f, 10, out m2, out com2, out i2);
      Assert.IsTrue(Numeric.AreEqual(m0 * 0.7f, m2, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com2, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 0.7f, i2, e * (1 + i0.Trace)));

      // Try with target mass.
      float m3;
      Vector3F com3;
      Matrix33F i3;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 23, false, 0.001f, 10, out m3, out com3, out i3);
      Assert.IsTrue(Numeric.AreEqual(23, m3, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com3, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 23 / m0, i3, e * (1 + i0.Trace)));
    }


    [Test]
    public void CapsuleMass()
    {
      var s = new CapsuleShape(1, 3);
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 1, true, 0.001f, 10, out m0, out com0, out i0);

      var m = s.GetMesh(0.001f, 10);
      m.Transform(Matrix44F.CreateScale(1, -2, -3));
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(m, out m1, out com1, out i1);

      const float e = 0.01f;
      Assert.IsTrue(Numeric.AreEqual(m0, m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, i1, e * (1 + i0.Trace)));

      // Try other density.
      float m2;
      Vector3F com2;
      Matrix33F i2;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 0.7f, true, 0.001f, 10, out m2, out com2, out i2);
      Assert.IsTrue(Numeric.AreEqual(m0 * 0.7f, m2, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com2, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 0.7f, i2, e * (1 + i0.Trace)));

      // Try with target mass.
      float m3;
      Vector3F com3;
      Matrix33F i3;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 23, false, 0.001f, 10, out m3, out com3, out i3);
      Assert.IsTrue(Numeric.AreEqual(23, m3, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com3, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 23 / m0, i3, e * (1 + i0.Trace)));
    }


    [Test]
    public void CylinderMass()
    {
      var s = new CylinderShape(1, 3);
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 1, true, 0.001f, 10, out m0, out com0, out i0);

      var m = s.GetMesh(0.001f, 10);
      m.Transform(Matrix44F.CreateScale(1, -2, -3));
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(m, out m1, out com1, out i1);

      const float e = 0.01f;
      Assert.IsTrue(Numeric.AreEqual(m0, m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, i1, e * (1 + i0.Trace)));

      // Try other density.
      float m2;
      Vector3F com2;
      Matrix33F i2;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 0.7f, true, 0.001f, 10, out m2, out com2, out i2);
      Assert.IsTrue(Numeric.AreEqual(m0 * 0.7f, m2, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com2, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 0.7f, i2, e * (1 + i0.Trace)));

      // Try with target mass.
      float m3;
      Vector3F com3;
      Matrix33F i3;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 23, false, 0.001f, 10, out m3, out com3, out i3);
      Assert.IsTrue(Numeric.AreEqual(23, m3, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com3, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 23 / m0, i3, e * (1 + i0.Trace)));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetShapeMassArgumentNullException()
    {
      float m;
      Vector3F com;
      Matrix33F i;
      MassHelper.GetMass(null, Vector3F.One, 1, true, 0.1f, 1, out m, out com, out i);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetShapeMassArgumentOutOfRangeException()
    {
      float m;
      Vector3F com;
      Matrix33F i;
      MassHelper.GetMass(new SphereShape(1), Vector3F.One, -1, true, 0.1f, 1, out m, out com, out i);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetShapeMassArgumentOutOfRangeException2()
    {
      float m;
      Vector3F com;
      Matrix33F i;
      MassHelper.GetMass(new SphereShape(1), Vector3F.One, 1, true, -0.1f, 1, out m, out com, out i);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetMeshMassArgumentNullException()
    {
      float m;
      Vector3F com;
      Matrix33F i;
      MassHelper.GetMass(null, out m, out com, out i);
    }


    [Test]
    public void ScaledConvexMass()
    {
      var s = new ScaledConvexShape(new CapsuleShape(1, 3), new Vector3F(0.9f, -0.8f, 1.2f));
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 1, true, 0.001f, 10, out m0, out com0, out i0);

      var m = s.GetMesh(0.001f, 6);
      m.Transform(Matrix44F.CreateScale(1, -2, -3));
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(m, out m1, out com1, out i1);

      const float e = 0.01f;
      Assert.IsTrue(Numeric.AreEqual(m0, m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, i1, e * (1 + i0.Trace)));

      // Try other density.
      float m2;
      Vector3F com2;
      Matrix33F i2;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 0.7f, true, 0.001f, 10, out m2, out com2, out i2);
      Assert.IsTrue(Numeric.AreEqual(m0 * 0.7f, m2, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com2, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 0.7f, i2, e * (1 + i0.Trace)));
    }


    [Test]
    public void TransformedShapeMassWithScaling()
    {
      var s = new TransformedShape(new GeometricObject(new BoxShape(3, 2, 1), new Vector3F(0.7f), new Pose(new Vector3F(-1, 7, 4), RandomHelper.Random.NextQuaternionF())));
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(2), 1, true, 0.001f, 10, out m0, out com0, out i0);

      var m = s.GetMesh(0.001f, 6);
      m.Transform(Matrix44F.CreateScale(2));
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(m, out m1, out com1, out i1);

      const float e = 0.01f;
      Assert.IsTrue(Numeric.AreEqual(m0, m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, i1, e * (1 + i0.Trace)));
    }


    [Test]
    public void TransformedShapeMassWithNonuniformScaling()
    {
      var s = new TransformedShape(new GeometricObject(new BoxShape(3, 2, 1), new Vector3F(0.7f, 0.8f, 0.9f), new Pose(new Vector3F(-1, 7, 4))));
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(2, 2.1f, 2.8f), 1, true, 0.001f, 10, out m0, out com0, out i0);

      var m = s.GetMesh(0.001f, 6);
      m.Transform(Matrix44F.CreateScale(2, 2.1f, 2.8f));
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(m, out m1, out com1, out i1);

      const float e = 0.01f;
      Assert.IsTrue(Numeric.AreEqual(m0, m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, i1, e * (1 + i0.Trace)));
    }


    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TransformedShapeNonuniformScaleWithRotationNotSupported()
    {
      var s = new TransformedShape(new GeometricObject(new BoxShape(3, 2, 1), new Vector3F(0.7f, 0.8f, 0.9f), new Pose(new Vector3F(-1, 7, 4), QuaternionF.CreateRotationX(1))));
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(2, 2.1f, 2.8f), 1, true, 0.001f, 10, out m0, out com0, out i0);
    }


    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TransformedShapeNegativeScalingNotSupported()
    {
      var s = new TransformedShape(new GeometricObject(new BoxShape(3, 2, 1), new Vector3F(0.7f, 0.8f, 0.9f), new Pose(new Vector3F(-1, 7, 4))));
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(2, 2.1f, -2.8f), 1, true, 0.001f, 10, out m0, out com0, out i0);
    }


    [Test]
    public void CompositeShapeWithNonUniformScaling()
    {
      var s = new CompositeShape();
      s.Children.Add(new GeometricObject(new BoxShape(1, 2, 3), new Vector3F(1.1f, 0.3f, 0.8f), new Pose(new Vector3F(100, 10, 0))));
      s.Children.Add(new GeometricObject(new SphereShape(1), new Vector3F(1.1f, 0.3f, 0.8f), new Pose(new Vector3F(-10, -10, 0))));
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(2, 2.1f, 2.8f), 0.7f, true, 0.001f, 10, out m0, out com0, out i0);

      var m = s.GetMesh(0.001f, 6);
      m.Transform(Matrix44F.CreateScale(2, 2.1f, 2.8f));
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(m, out m1, out com1, out i1);

      const float e = 0.01f;
      Assert.IsTrue(Numeric.AreEqual(m0, 0.7f * m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, 0.7f * i1, e * (1 + i0.Trace)));

      // Try with target mass.
      float m3;
      Vector3F com3;
      Matrix33F i3;
      MassHelper.GetMass(s, new Vector3F(2, 2.1f, 2.8f), 23, false, 0.001f, 10, out m3, out com3, out i3);
      Assert.IsTrue(Numeric.AreEqual(23, m3, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com3, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 23 / m0, i3, e * (1 + i0.Trace)));
    }


    [Test]
    public void CompositeShapeWithRotatedChildren()
    {
      var s = new CompositeShape();
      s.Children.Add(new GeometricObject(new BoxShape(1, 2, 3), new Vector3F(1.1f, 0.3f, 0.8f), new Pose(new Vector3F(100, 10, 0), RandomHelper.Random.NextQuaternionF())));
      s.Children.Add(new GeometricObject(new ConeShape(1, 2), new Vector3F(1.1f, 0.3f, 0.8f), new Pose(new Vector3F(-10, -10, 0), RandomHelper.Random.NextQuaternionF())));
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(2), 1, true, 0.001f, 10, out m0, out com0, out i0);

      var m = s.GetMesh(0.001f, 6);
      m.Transform(Matrix44F.CreateScale(2));
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(m, out m1, out com1, out i1);

      const float e = 0.01f;
      Assert.IsTrue(Numeric.AreEqual(m0, m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, i1, e * (1 + i0.Trace)));
    }


    [Test]
    public void CompositeShapeWithRigidBodies()
    {
      // The first composite shape does not use rigid bodies.
      var s = new CompositeShape();
      s.Children.Add(new GeometricObject(new BoxShape(1, 2, 3), new Vector3F(1.1f, 0.3f, 0.8f), new Pose(new Vector3F(100, 10, 0), RandomHelper.Random.NextQuaternionF())));
      s.Children.Add(new GeometricObject(new ConeShape(1, 2), new Vector3F(1.1f, 0.3f, 0.8f), new Pose(new Vector3F(-10, -10, 0), RandomHelper.Random.NextQuaternionF())));
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(1), 0.7f, true, 0.001f, 10, out m0, out com0, out i0);

      // The second composite shape uses rigid bodies as children.
      var r0 = new RigidBody(s.Children[0].Shape);
      r0.Pose = s.Children[0].Pose;
      r0.Scale = s.Children[0].Scale;
      r0.MassFrame = MassFrame.FromShapeAndDensity(r0.Shape, r0.Scale, 0.7f, 0.001f, 10);
      var r1 = new RigidBody(s.Children[1].Shape);
      r1.Pose = s.Children[1].Pose;
      r1.Scale = s.Children[1].Scale;
      r1.MassFrame = MassFrame.FromShapeAndDensity(r1.Shape, r1.Scale, 0.7f, 0.001f, 10);
      var s1 = new CompositeShape();
      s1.Children.Add(r0);
      s1.Children.Add(r1);

      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(s1, new Vector3F(1), 100, true, 0.001f, 10, out m1, out com1, out i1);

      const float e = 0.01f;
      Assert.IsTrue(Numeric.AreEqual(m0, m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, i1, e * (1 + i0.Trace)));
    }


    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void CompositeShapeWithRigidBodiesDoesNotSupportScaling()
    {
      // The first composite shape does not use rigid bodies.
      var s = new CompositeShape();
      s.Children.Add(new GeometricObject(new BoxShape(1, 2, 3), new Vector3F(1.1f, 0.3f, 0.8f), new Pose(new Vector3F(100, 10, 0), RandomHelper.Random.NextQuaternionF())));
      s.Children.Add(new GeometricObject(new ConeShape(1, 2), new Vector3F(1.1f, 0.3f, 0.8f), new Pose(new Vector3F(-10, -10, 0), RandomHelper.Random.NextQuaternionF())));

      // The second composite shape uses rigid bodies as children.
      var r0 = new RigidBody(s.Children[0].Shape);
      r0.Pose = s.Children[0].Pose;
      r0.Scale = s.Children[0].Scale;
      r0.MassFrame = MassFrame.FromShapeAndDensity(r0.Shape, r0.Scale, 0.7f, 0.001f, 10);
      var r1 = new RigidBody(s.Children[1].Shape);
      r1.Pose = s.Children[1].Pose;
      r1.Scale = s.Children[1].Scale;
      r1.MassFrame = MassFrame.FromShapeAndDensity(r1.Shape, r1.Scale, 0.7f, 0.001f, 10);
      var s1 = new CompositeShape();
      s1.Children.Add(r0);
      s1.Children.Add(r1);

      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(s1, 
        new Vector3F(2),  // !!!
        100, true, 0.001f, 10, out m1, out com1, out i1);
    }


    [Test]
    public void ApproximateAabbMass()
    {
      var s = new ConvexHullOfPoints(new[] { new Vector3F(1, 1, 1), new Vector3F(2, 4, 6) });
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(1.2f, 2.1f, 0.6f), 0.7f, true, 0.001f, 0, out m0, out com0, out i0);

      var s2 = new TransformedShape(new GeometricObject(new BoxShape(1, 3, 5), new Pose(new Vector3F(1.5f, 2.5f, 3.5f))));
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(s2, new Vector3F(1.2f, 2.1f, 0.6f), 0.7f, true, 0.001f, 0, out m1, out com1, out i1);

      const float e = 0.0001f;
      Assert.IsTrue(Numeric.AreEqual(m0, m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, i1, e * (1 + i0.Trace)));
    }


    [Test]
    public void GeneralShape()
    {
      var s = new CylinderShape(1, 3);
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 0.7f, true, 0.001f, 4, out m0, out com0, out i0);

      var m = s.GetMesh(0.001f, 10);
      var s2 = new TriangleMeshShape(m);
      float m1;
      Vector3F com1;
      Matrix33F i1;
      MassHelper.GetMass(s2, new Vector3F(1, -2, -3), 0.7f, true, 0.001f, 4, out m1, out com1, out i1);

      const float e = 0.01f;
      Assert.IsTrue(Numeric.AreEqual(m0, m1, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com1, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0, i1, e * (1 + i0.Trace)));

      // Try with target mass.
      float m2;
      Vector3F com2;
      Matrix33F i2;
      MassHelper.GetMass(s2, new Vector3F(1, -2, -3), 23, false, 0.001f, 4, out m2, out com2, out i2);
      Assert.IsTrue(Numeric.AreEqual(23, m2, e * (1 + m0)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(com0, com2, e * (1 + com0.Length)));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(i0 * 23 / m0, i2, e * (1 + i0.Trace)));
    }


    [Test]
    public void EmptyShape()
    {
      var s = new EmptyShape();
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 0.7f, true, 0.001f, 4, out m0, out com0, out i0);

      Assert.AreEqual(0, m0);
      Assert.AreEqual(Vector3F.Zero, com0);
      Assert.AreEqual(Matrix33F.Zero, i0);
    }


    [Test]
    public void InfiniteShape()
    {
      var s = new InfiniteShape();
      float m0;
      Vector3F com0;
      Matrix33F i0;
      MassHelper.GetMass(s, new Vector3F(1, -2, -3), 0.7f, true, 0.001f, 4, out m0, out com0, out i0);

      Assert.AreEqual(float.PositiveInfinity, m0);
      Assert.AreEqual(Vector3F.Zero, com0);
      Assert.AreEqual(Matrix33F.CreateScale(float.PositiveInfinity), i0);
    }


    [Test]
    public void DiagonalizeTest()
    {
      for (int i = 0; i < 1000; i++)
      {
        var inertia = RandomHelper.Random.NextMatrix33F(0, 100);

        // Make symmetric
        inertia.M10 = inertia.M01;
        inertia.M12 = inertia.M21;
        inertia.M20 = inertia.M02;

        Assert.IsTrue(inertia.IsSymmetric);

        Vector3F inertiaDiagonale;
        Matrix33F rotation;
        MassHelper.DiagonalizeInertia(inertia, out inertiaDiagonale, out rotation);

        Assert.IsTrue(rotation.IsRotation);

        var inertia2 = rotation * Matrix33F.CreateScale(inertiaDiagonale) * rotation.Transposed;
        Assert.IsTrue(Matrix33F.AreNumericallyEqual(inertia, inertia2, 0.001f));  // Epsilon = 10^-4 is already too small :-(
      }
    }
  }
}
