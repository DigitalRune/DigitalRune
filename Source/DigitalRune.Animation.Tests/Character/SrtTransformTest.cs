using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using DigitalRune.Animation.Character;


namespace DigitalRune.Animation.Character.Tests
{
  [TestFixture]
  public class SrtTransformTest
  {
    [Test]
    public void ConstructorTest()
    {
      var rotationQ = new QuaternionF(1, 2, 3, 4).Normalized;

      var srt = new SrtTransform(rotationQ.ToRotationMatrix33());

      Assert.AreEqual(Vector3F.One, srt.Scale);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(rotationQ, srt.Rotation));
      Assert.AreEqual(Vector3F.Zero, srt.Translation);

      srt = new SrtTransform(rotationQ);

      Assert.AreEqual(Vector3F.One, srt.Scale);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(rotationQ, srt.Rotation));
      Assert.AreEqual(Vector3F.Zero, srt.Translation);

      srt = new SrtTransform(new Vector3F(-1, 2, -3), rotationQ.ToRotationMatrix33(), new Vector3F(10, 9, -8));
      Assert.AreEqual(new Vector3F(-1, 2, -3), srt.Scale);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(rotationQ, srt.Rotation));
      Assert.AreEqual(new Vector3F(10, 9, -8), srt.Translation);
    }


    [Test]
    public void IdentityTest()
    {
      var identity = SrtTransform.Identity;

      Assert.AreEqual(Vector3F.One, identity.Scale);
      Assert.AreEqual(QuaternionF.Identity, identity.Rotation);
      Assert.AreEqual(Vector3F.Zero, identity.Translation);
    }


    [Test]
    public void HasScaleTest()
    {
      var srt = new SrtTransform(new Vector3F(1, 1, 1), QuaternionF.Identity, Vector3F.Zero);
      Assert.IsFalse(srt.HasScale);

      srt.Scale = new Vector3F(1.00001f, 1.000001f, 1.000001f);
      Assert.IsFalse(srt.HasScale);

      srt.Scale = new Vector3F(1.1f, 1, 1);
      Assert.IsTrue(srt.HasScale);

      srt.Scale = new Vector3F(1, 1.1f, 1);
      Assert.IsTrue(srt.HasScale);

      srt.Scale = new Vector3F(1, 1, 1.1f);
      Assert.IsTrue(srt.HasScale);

      srt.Scale = new Vector3F(1.1f);
      Assert.IsTrue(srt.HasScale);
    }


    [Test]
    public void HasRotationTest()
    {
      var srt = new SrtTransform(new Vector3F(1, 1, 1), QuaternionF.Identity, Vector3F.Zero);
      Assert.IsFalse(srt.HasRotation);

      srt.Rotation = QuaternionF.CreateRotationX(0.000001f);
      Assert.IsFalse(srt.HasRotation);

      srt.Rotation = QuaternionF.CreateRotationX(0.1f);
      Assert.IsTrue(srt.HasRotation);
    }


    [Test]
    public void HasTranslationTest()
    {
      var srt = new SrtTransform(QuaternionF.Identity, Vector3F.Zero);
      Assert.IsFalse(srt.HasTranslation);

      srt.Translation = new Vector3F(0.000001f, 0.000001f, 0.000001f);
      Assert.IsFalse(srt.HasTranslation);

      srt.Translation = new Vector3F(1.1f, 0, 0);
      Assert.IsTrue(srt.HasTranslation);

      srt.Translation = new Vector3F(0, 1.1f, 0);
      Assert.IsTrue(srt.HasTranslation);

      srt.Translation = new Vector3F(0, 0, 1.1f);
      Assert.IsTrue(srt.HasTranslation);

      srt.Translation = new Vector3F(1.1f);
      Assert.IsTrue(srt.HasTranslation);
    }


    [Test]
    public void EqualsTest()
    {
      var a = new SrtTransform(new Vector3F(1, 2, 3), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, 5, 6));
      var b = new SrtTransform(new Vector3F(1, 2, 3), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, 5, 6));

      Assert.IsFalse(((object)a).Equals(3));

      Assert.IsTrue(a.Equals(b));
      Assert.IsTrue(((object)a).Equals(b));
      Assert.IsTrue(a == b);
      Assert.IsFalse(a != b);
      Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

      b = new SrtTransform(new Vector3F(1, 22, 3), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, 5, 6));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(((object)a).Equals(b));
      Assert.IsFalse(a == b);
      Assert.IsTrue(a != b);
      Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

      b = new SrtTransform(new Vector3F(1, 2, 33), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, 5, 6));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(((object)a).Equals(b));
      Assert.IsFalse(a == b);
      Assert.IsTrue(a != b);
      Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

      b = new SrtTransform(new Vector3F(11, 2, 3), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, 5, 6));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(((object)a).Equals(b));
      Assert.IsFalse(a == b);
      Assert.IsTrue(a != b);
      Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

      b = new SrtTransform(new Vector3F(1, 2, 3), new QuaternionF(11, 2, 3, 4).Normalized, new Vector3F(4, 5, 6));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(((object)a).Equals(b));
      Assert.IsFalse(a == b);
      Assert.IsTrue(a != b);
      Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

      b = new SrtTransform(new Vector3F(1, 2, 3), new QuaternionF(1, 22, 3, 4).Normalized, new Vector3F(4, 5, 6));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(((object)a).Equals(b));
      Assert.IsFalse(a == b);
      Assert.IsTrue(a != b);
      Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

      b = new SrtTransform(new Vector3F(1, 2, 3), new QuaternionF(1, 2, 33, 4).Normalized, new Vector3F(4, 5, 6));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(((object)a).Equals(b));
      Assert.IsFalse(a == b);
      Assert.IsTrue(a != b);
      Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

      b = new SrtTransform(new Vector3F(1, 2, 3), new QuaternionF(1, 2, 3, 44).Normalized, new Vector3F(4, 5, 6));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(((object)a).Equals(b));
      Assert.IsFalse(a == b);
      Assert.IsTrue(a != b);
      Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

      b = new SrtTransform(new Vector3F(1, 2, 3), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(44, 5, 6));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(((object)a).Equals(b));
      Assert.IsFalse(a == b);
      Assert.IsTrue(a != b);
      Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

      b = new SrtTransform(new Vector3F(1, 2, 3), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, 55, 6));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(((object)a).Equals(b));
      Assert.IsFalse(a == b);
      Assert.IsTrue(a != b);
      Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

      b = new SrtTransform(new Vector3F(1, 2, 3), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, 5, 66));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(((object)a).Equals(b));
      Assert.IsFalse(a == b);
      Assert.IsTrue(a != b);
      Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
    }


    [Test]
    public void IsValidTest()
    {
      var m = Matrix44F.CreateTranslation(1, 2, 3) * Matrix44F.CreateRotationY(0.1f) * Matrix44F.CreateScale(-2, 3, 4);
      Assert.IsTrue(SrtTransform.IsValid(m));

      // Concatenating to SRTs creates a skew.
      m = Matrix44F.CreateRotationZ(0.1f) * Matrix44F.CreateScale(-2, 3, 4) * m;
      Assert.IsFalse(SrtTransform.IsValid(m));
    }


    [Test]
    public void InterpolateTest()
    {
      SrtTransform a = new SrtTransform(new Vector3F(1, 2, 3), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, 5, 6));
      SrtTransform b = a;

      var c = SrtTransform.Interpolate(a, b, 0.5f);
      Assert.AreEqual(a, c);

      b = new SrtTransform(new Vector3F(7, 9, 8), new QuaternionF(6, 6, 4, 2).Normalized, new Vector3F(-2, 4, -9));
      c = SrtTransform.Interpolate(a, b, 0);
      Assert.AreEqual(a, c);

      c = SrtTransform.Interpolate(a, b, 1);
      Assert.AreEqual(b, c);

      c = SrtTransform.Interpolate(a, b, 0.3f);
      Assert.AreEqual(a.Translation * 0.7f + b.Translation * 0.3f, c.Translation);
      Assert.AreEqual(a.Scale * 0.7f + b.Scale * 0.3f, c.Scale);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(
        new QuaternionF(
          a.Rotation.W * 0.7f + b.Rotation.W * 0.3f,
          a.Rotation.V * 0.7f + b.Rotation.V * 0.3f).Normalized,
        c.Rotation));
    }



    [Test]
    public void FromToMatrixTest()
    {
      var t = new Vector3F(1, 2, 3);
      var r = new QuaternionF(1, 2, 3, 4).Normalized;
      var s = new Vector3F(2, 7, 9);
      var m = Matrix44F.CreateTranslation(t) * Matrix44F.CreateRotation(r) * Matrix44F.CreateScale(s);

      var srt = SrtTransform.FromMatrix(m);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(t, srt.Translation));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(r, srt.Rotation));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(s, srt.Scale));

      // XNA:
      srt = SrtTransform.FromMatrix((Matrix)m);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(t, srt.Translation));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(r, srt.Rotation));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(s, srt.Scale));

      // With negative scale, the decomposition is not unique (many possible combinations of 
      // axis mirroring + rotation).
      t = new Vector3F(1, 2, 3);
      r = new QuaternionF(1, 2, 3, 4).Normalized;
      s = new Vector3F(2, -7, 9);
      m = Matrix44F.CreateTranslation(t) * Matrix44F.CreateRotation(r) * Matrix44F.CreateScale(s);
      srt = SrtTransform.FromMatrix(m);
      var m2 = (Matrix44F)srt;
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(m, m2));

      m2 = srt.ToMatrix44F();
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(m, m2));

      m2 = srt;
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(m, m2));

      Matrix mXna = srt.ToXna();
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(m, (Matrix44F)mXna));

      mXna = srt;
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(m, (Matrix44F)mXna));
    }


    [Test]
    public void InverseTest()
    {
      var identity = SrtTransform.Identity;
      var a = new SrtTransform(new Vector3F(-2, -2, -2), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, -5, 6));
      var aInverse = a.Inverse;
      
      var aa = a * aInverse;
      Assert.IsTrue(SrtTransform.AreNumericallyEqual(identity, aa));

      aa = aInverse * a;
      Assert.IsTrue(SrtTransform.AreNumericallyEqual(identity, aa));


      a = new SrtTransform(new Vector3F(-3, 7, -4), QuaternionF.Identity, new Vector3F(4, -5, 6));
      aInverse = a.Inverse;

      aa = a * aInverse;
      Assert.IsTrue(SrtTransform.AreNumericallyEqual(identity, aa));

      aa = aInverse * a;
      Assert.IsTrue(SrtTransform.AreNumericallyEqual(identity, aa));
    }


    [Test]
    public void MultiplyWithUniformScaleIsAssociative()
    {
      var a = new SrtTransform(new Vector3F(2), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, -5, 6));
      var b = new SrtTransform(new Vector3F(-3), new QuaternionF(3, -2, 1, 9).Normalized, new Vector3F(7, -4, 2));
      var c = new SrtTransform(new Vector3F(4), new QuaternionF(7, -5, 3, 1).Normalized, new Vector3F(-8, -1, -7));

      // Assocative for uniform scale
      Assert.IsTrue(SrtTransform.AreNumericallyEqual((a*b)*c, a*(b*c)));
    }


    [Test]
    public void MultiplyWithScaleWithoutRotationIsAssociative()
    {
      var a = new SrtTransform(new Vector3F(2), QuaternionF.Identity, new Vector3F(4, -5, 6));
      var b = new SrtTransform(new Vector3F(-3), QuaternionF.Identity, new Vector3F(7, -4, 2));
      var c = new SrtTransform(new Vector3F(4), QuaternionF.Identity, new Vector3F(-8, -1, -7));

      // Assocative for uniform scale
      Assert.IsTrue(SrtTransform.AreNumericallyEqual((a * b) * c, a * (b * c)));
    }


    [Test]
    public void MultiplyWithoutRotationIsTheSameAsMatrixMultiply()
    {
      // Result is the same as Matrix mulitiplication without scale.
      var a = new SrtTransform(new Vector3F(1, 2, 3), QuaternionF.Identity, new Vector3F(4, -5, 6));
      var b = new SrtTransform(new Vector3F(5, 6, -3), QuaternionF.Identity, new Vector3F(7, -4, 2));

      var result1 = (a * b).ToMatrix44F();
      var result2 = a.ToMatrix44F() * b.ToMatrix44F();
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void MultiplyWithUniformScaleIsTheSameAsMatrixMultiply()
    {
      // Result is the same as Matrix mulitiplication without scale.
      var a = new SrtTransform(new Vector3F(7), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, -5, 6));
      var b = new SrtTransform(new Vector3F(-3), new QuaternionF(3, -2, 1, 9).Normalized, new Vector3F(7, -4, 2));

      var result1 = (a * b).ToMatrix44F();
      var result2 = a.ToMatrix44F() * b.ToMatrix44F();
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void Multiply()
    {
      var a = new SrtTransform(new Vector3F(1, 2, 7), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, -5, 6));
      var b = new SrtTransform(new Vector3F(-3, 9, -2), new QuaternionF(3, -2, 1, 9).Normalized, new Vector3F(7, -4, 2));

      var result1 = SrtTransform.Multiply(a, b).ToMatrix44F();
      var result2 = a * b;
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void MultiplyVector4()
    {
      var a = new SrtTransform(new Vector3F(1, 2, 7), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, -5, 6));
      var v = new Vector4F(7, 9, -12, -2);

      var result1 = a * v;
      var result2 = a.ToMatrix44F() * v;
      Assert.IsTrue(Vector4F.AreNumericallyEqual(result1, result2));

      result1 = SrtTransform.Multiply(a, v);
      result2 = a.ToMatrix44F() * v;
      Assert.IsTrue(Vector4F.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void ToParentDirection()
    {
      var a = new SrtTransform(new Vector3F(1, 2, 7), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, -5, 6));
      var v = new Vector3F(7, 9, -12);

      var result1 = a.ToParentDirection(v);
      var result2 = a.Rotation.ToRotationMatrix33() * v;
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void ToLocalDirection()
    {
      var a = new SrtTransform(new Vector3F(1, 2, 7), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, -5, 6));
      var v = new Vector3F(7, 9, -12);

      var result1 = a.ToLocalDirection(v);
      var result2 = a.Rotation.ToRotationMatrix33().Transposed * v;
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void ToParentPosition()
    {
      var a = new SrtTransform(new Vector3F(1, 2, 7), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, -5, 6));
      var v = new Vector3F(7, 9, -12);

      var result1 = a.ToParentPosition(v);
      var result2 = a.ToMatrix44F().TransformPosition(v);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void ToLocalPosition()
    {
      var a = new SrtTransform(new Vector3F(1, 2, 7), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(4, -5, 6));
      var v = new Vector3F(7, 9, -12);

      var result1 = a.ToLocalPosition(v);
      var result2 = a.ToMatrix44F().Inverse.TransformPosition(v);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void FromPose()
    {
      var pose = new Pose(new Vector3F(1, 2, 3), new QuaternionF(1, 2, 3, 4).Normalized);
      var srt = SrtTransform.FromPose(pose);

      Assert.AreEqual(Vector3F.One, srt.Scale);
      Assert.AreEqual(pose.Position, srt.Translation);
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(pose.Orientation, srt.Rotation.ToRotationMatrix33()));
      
      pose = new Pose(new Vector3F(1, 2, 3), new QuaternionF(1, 2, 3, 4).Normalized);
      srt = pose;

      Assert.AreEqual(Vector3F.One, srt.Scale);
      Assert.AreEqual(pose.Position, srt.Translation);
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(pose.Orientation, srt.Rotation.ToRotationMatrix33()));
    }


    [Test]
    public void ToPose()
    {
      var srt = new SrtTransform(new Vector3F(4, 5, 6), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(1, 2, 3));
      var pose = srt.ToPose();

      Assert.AreEqual(pose.Position, srt.Translation);
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(pose.Orientation, srt.Rotation.ToRotationMatrix33()));

      srt = new SrtTransform(new Vector3F(4, 5, 6), new QuaternionF(1, 2, 3, 4).Normalized, new Vector3F(1, 2, 3));
      pose = (Pose)srt;

      Assert.AreEqual(pose.Position, srt.Translation);
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(pose.Orientation, srt.Rotation.ToRotationMatrix33()));
    }
  }
}
