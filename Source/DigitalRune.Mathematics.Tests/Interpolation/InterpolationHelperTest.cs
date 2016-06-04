using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;



namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class InterpolationHelperTest
  {
    [Test]
    public void Lerp()
    {
      Assert.AreEqual(1.0f, InterpolationHelper.Lerp(1.0f, 2.0f, 0.0f));
      Assert.AreEqual(1.5f, InterpolationHelper.Lerp(1.0f, 2.0f, 0.5f));
      Assert.AreEqual(2.0f, InterpolationHelper.Lerp(1.0f, 2.0f, 1.0f));
      Assert.AreEqual(1.5f, InterpolationHelper.Lerp(2.0f, 1.0f, 0.5f));

      Assert.AreEqual(1.0, InterpolationHelper.Lerp(1.0, 2.0, 0.0));
      Assert.AreEqual(1.5, InterpolationHelper.Lerp(1.0, 2.0, 0.5));
      Assert.AreEqual(2.0, InterpolationHelper.Lerp(1.0, 2.0, 1.0));
      Assert.AreEqual(1.5, InterpolationHelper.Lerp(2.0, 1.0, 0.5));
    }


    [Test]
    public void LerpVector2F()
    {
      Vector2F v = new Vector2F(1.0f, 10.0f);
      Vector2F w = new Vector2F(2.0f, 20.0f);
      Vector2F lerp0 = InterpolationHelper.Lerp(v, w, 0.0f);
      Vector2F lerp1 = InterpolationHelper.Lerp(v, w, 1.0f);
      Vector2F lerp05 = InterpolationHelper.Lerp(v, w, 0.5f);
      Vector2F lerp025 = InterpolationHelper.Lerp(v, w, 0.25f);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new Vector2F(1.5f, 15.0f), lerp05);
      Assert.AreEqual(new Vector2F(1.25f, 12.5f), lerp025);
    }


    [Test]
    public void LerpVector2D()
    {
      Vector2D v = new Vector2D(1.0, 10.0);
      Vector2D w = new Vector2D(2.0, 20.0);
      Vector2D lerp0 = InterpolationHelper.Lerp(v, w, 0.0);
      Vector2D lerp1 = InterpolationHelper.Lerp(v, w, 1.0);
      Vector2D lerp05 = InterpolationHelper.Lerp(v, w, 0.5);
      Vector2D lerp025 = InterpolationHelper.Lerp(v, w, 0.25);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new Vector2D(1.5, 15.0), lerp05);
      Assert.AreEqual(new Vector2D(1.25, 12.5), lerp025);
    }


    [Test]
    public void LerpVector3F()
    {
      Vector3F v = new Vector3F(1.0f, 10.0f, 100.0f);
      Vector3F w = new Vector3F(2.0f, 20.0f, 200.0f);
      Vector3F lerp0 = InterpolationHelper.Lerp(v, w, 0.0f);
      Vector3F lerp1 = InterpolationHelper.Lerp(v, w, 1.0f);
      Vector3F lerp05 = InterpolationHelper.Lerp(v, w, 0.5f);
      Vector3F lerp025 = InterpolationHelper.Lerp(v, w, 0.25f);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new Vector3F(1.5f, 15.0f, 150.0f), lerp05);
      Assert.AreEqual(new Vector3F(1.25f, 12.5f, 125.0f), lerp025);
    }


    [Test]
    public void LerpVector3D()
    {
      Vector3D v = new Vector3D(1.0, 10.0, 100.0);
      Vector3D w = new Vector3D(2.0, 20.0, 200.0);
      Vector3D lerp0 = InterpolationHelper.Lerp(v, w, 0.0);
      Vector3D lerp1 = InterpolationHelper.Lerp(v, w, 1.0);
      Vector3D lerp05 = InterpolationHelper.Lerp(v, w, 0.5);
      Vector3D lerp025 = InterpolationHelper.Lerp(v, w, 0.25);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new Vector3D(1.5, 15.0, 150.0), lerp05);
      Assert.AreEqual(new Vector3D(1.25, 12.5, 125.0), lerp025);
    }


    [Test]
    public void LerpVector4F()
    {
      Vector4F v = new Vector4F(1.0f, 10.0f, 100.0f, 1000.0f);
      Vector4F w = new Vector4F(2.0f, 20.0f, 200.0f, 2000.0f);
      Vector4F lerp0 = InterpolationHelper.Lerp(v, w, 0.0f);
      Vector4F lerp1 = InterpolationHelper.Lerp(v, w, 1.0f);
      Vector4F lerp05 = InterpolationHelper.Lerp(v, w, 0.5f);
      Vector4F lerp025 = InterpolationHelper.Lerp(v, w, 0.25f);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new Vector4F(1.5f, 15.0f, 150.0f, 1500.0f), lerp05);
      Assert.AreEqual(new Vector4F(1.25f, 12.5f, 125.0f, 1250.0f), lerp025);
    }


    [Test]
    public void LerpVector4D()
    {
      Vector4D v = new Vector4D(1.0, 10.0, 100.0, 1000.0);
      Vector4D w = new Vector4D(2.0, 20.0, 200.0, 2000.0);
      Vector4D lerp0 = InterpolationHelper.Lerp(v, w, 0.0);
      Vector4D lerp1 = InterpolationHelper.Lerp(v, w, 1.0);
      Vector4D lerp05 = InterpolationHelper.Lerp(v, w, 0.5);
      Vector4D lerp025 = InterpolationHelper.Lerp(v, w, 0.25);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new Vector4D(1.5, 15.0, 150.0, 1500.0), lerp05);
      Assert.AreEqual(new Vector4D(1.25, 12.5, 125.0, 1250.0), lerp025);
    }


    [Test]
    public void LerpVectorF()
    {
      VectorF v = new VectorF(new[] { 1.0f, 10.0f, 100.0f, 1000.0f });
      VectorF w = new VectorF(new[] { 2.0f, 20.0f, 200.0f, 2000.0f });
      VectorF lerp0 = InterpolationHelper.Lerp(v, w, 0.0f);
      VectorF lerp1 = InterpolationHelper.Lerp(v, w, 1.0f);
      VectorF lerp05 = InterpolationHelper.Lerp(v, w, 0.5f);
      VectorF lerp025 = InterpolationHelper.Lerp(v, w, 0.25f);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new VectorF(new[] {1.5f, 15.0f, 150.0f, 1500.0f}), lerp05);
      Assert.AreEqual(new VectorF(new[] {1.25f, 12.5f, 125.0f, 1250.0f}), lerp025);
    }


    [Test]
    public void LerpVectorD()
    {
      VectorD v = new VectorD(new[] { 1.0, 10.0, 100.0, 1000.0 });
      VectorD w = new VectorD(new[] { 2.0, 20.0, 200.0, 2000.0 });
      VectorD lerp0 = InterpolationHelper.Lerp(v, w, 0.0);
      VectorD lerp1 = InterpolationHelper.Lerp(v, w, 1.0);
      VectorD lerp05 = InterpolationHelper.Lerp(v, w, 0.5);
      VectorD lerp025 = InterpolationHelper.Lerp(v, w, 0.25);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new VectorD(new[] { 1.5, 15.0, 150.0, 1500.0 }), lerp05);
      Assert.AreEqual(new VectorD(new[] { 1.25, 12.5, 125.0, 1250.0 }), lerp025);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void LerpVectorFException()
    {
      InterpolationHelper.Lerp(null, new VectorF(), 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void LerpVectorDException()
    {
      InterpolationHelper.Lerp(null, new VectorD(), 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void LerpVectorFException2()
    {
      InterpolationHelper.Lerp(new VectorF(), null, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void LerpVectorDException2()
    {
      InterpolationHelper.Lerp(new VectorD(), null, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void LerpVectorFException3()
    {
      InterpolationHelper.Lerp(new VectorF(3), new VectorF(4), 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void LerpVectorDException3()
    {
      InterpolationHelper.Lerp(new VectorD(3), new VectorD(4), 0);
    }


    [Test]
    public void LerpQuaternionF()
    {
      // Warning: The not all results are not verified
      QuaternionF q1 = new QuaternionF(1.0f, 2.0f, 3.0f, 4.0f).Normalized;
      QuaternionF q2 = new QuaternionF(2.0f, 4.0f, 6.0f, 8.0f).Normalized;
      QuaternionF lerp = InterpolationHelper.Lerp(q1, q2, 0.75f);
      Assert.IsTrue(lerp.IsNumericallyNormalized);

      lerp = InterpolationHelper.Lerp(q1, q2, 0);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(q1, lerp));

      lerp = InterpolationHelper.Lerp(q1, q2, 1);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(q2, lerp));

      q1 = QuaternionF.Identity;
      q2 = QuaternionF.CreateRotation(Vector3F.UnitZ, (float) Math.PI / 2);
      lerp = InterpolationHelper.Lerp(q1, q2, 0.5f);
      Vector3F v = lerp.Rotate(Vector3F.UnitX);
      Vector3F result = new Vector3F(1.0f, 1.0f, 0.0f).Normalized;
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result, v));

      q1 = QuaternionF.Identity;
      q2 = QuaternionF.CreateRotation(Vector3F.UnitY, (float) Math.PI / 2);
      lerp = InterpolationHelper.Lerp(q1, q2, 0.5f);
      v = lerp.Rotate(Vector3F.UnitZ);
      result = new Vector3F(1.0f, 0.0f, 1.0f).Normalized;
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result, v));

      q1 = QuaternionF.Identity;
      q2 = QuaternionF.CreateRotation(Vector3F.UnitX, (float) Math.PI / 2);
      lerp = InterpolationHelper.Lerp(q1, q2, 0.5f);
      v = lerp.Rotate(Vector3F.UnitY);
      result = new Vector3F(0.0f, 1.0f, 1.0f).Normalized;
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result, v));

      q1 = new QuaternionF(-1.0f, 0.0f, 0.0f, 0.0f);
      q2 = QuaternionF.CreateRotation(-Vector3F.UnitZ, (float) -Math.PI / 2);
      lerp = InterpolationHelper.Lerp(q1, q2, 0.5f);
      v = lerp.Rotate(Vector3F.UnitX);
      result = new Vector3F(1.0f, 1.0f, 0.0f).Normalized;
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result, v));
    }


    [Test]
    public void LerpQuaternionD()
    {
      // Warning: The not all results are not verified
      QuaternionD q1 = new QuaternionD(1.0, 2.0, 3.0, 4.0).Normalized;
      QuaternionD q2 = new QuaternionD(2.0, 4.0, 6.0, 8.0).Normalized;
      QuaternionD lerp = InterpolationHelper.Lerp(q1, q2, 0.75);
      Assert.IsTrue(lerp.IsNumericallyNormalized);

      lerp = InterpolationHelper.Lerp(q1, q2, 0);
      Assert.IsTrue(QuaternionD.AreNumericallyEqual(q1, lerp));

      lerp = InterpolationHelper.Lerp(q1, q2, 1);
      Assert.IsTrue(QuaternionD.AreNumericallyEqual(q2, lerp));

      q1 = QuaternionD.Identity;
      q2 = QuaternionD.CreateRotation(Vector3D.UnitZ, Math.PI / 2);
      lerp = InterpolationHelper.Lerp(q1, q2, 0.5);
      Vector3D v = lerp.Rotate(Vector3D.UnitX);
      Vector3D result = new Vector3D(1.0, 1.0, 0.0).Normalized;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(result, v));

      q1 = QuaternionD.Identity;
      q2 = QuaternionD.CreateRotation(Vector3D.UnitY, Math.PI / 2);
      lerp = InterpolationHelper.Lerp(q1, q2, 0.5);
      v = lerp.Rotate(Vector3D.UnitZ);
      result = new Vector3D(1.0, 0.0, 1.0).Normalized;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(result, v));

      q1 = QuaternionD.Identity;
      q2 = QuaternionD.CreateRotation(Vector3D.UnitX, Math.PI / 2);
      lerp = InterpolationHelper.Lerp(q1, q2, 0.5);
      v = lerp.Rotate(Vector3D.UnitY);
      result = new Vector3D(0.0, 1.0, 1.0).Normalized;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(result, v));

      q1 = new QuaternionD(-1.0, 0.0, 0.0, 0.0);
      q2 = QuaternionD.CreateRotation(-Vector3D.UnitZ, -Math.PI / 2);
      lerp = InterpolationHelper.Lerp(q1, q2, 0.5);
      v = lerp.Rotate(Vector3D.UnitX);
      result = new Vector3D(1.0, 1.0, 0.0).Normalized;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(result, v));
    }


    [Test]
    public void StepSinglePrecision()
    {
      Assert.AreEqual(1, InterpolationHelper.Step(1, 2, 0, StepInterpolation.Left));
      Assert.AreEqual(2, InterpolationHelper.Step(1, 2, 0.5f, StepInterpolation.Left));
      Assert.AreEqual(2, InterpolationHelper.Step(1, 2, 1, StepInterpolation.Left));

      Assert.AreEqual(1, InterpolationHelper.Step(1, 2, 0, StepInterpolation.Right));
      Assert.AreEqual(1, InterpolationHelper.Step(1, 2, 0.5f, StepInterpolation.Right));
      Assert.AreEqual(2, InterpolationHelper.Step(1, 2, 1, StepInterpolation.Right));

      Assert.AreEqual(1, InterpolationHelper.Step(1, 2, 0, StepInterpolation.Centered));
      Assert.AreEqual(1, InterpolationHelper.Step(1, 2, 0.3f, StepInterpolation.Centered));
      Assert.AreEqual(2, InterpolationHelper.Step(1, 2, 0.6f, StepInterpolation.Centered));
      Assert.AreEqual(2, InterpolationHelper.Step(1, 2, 1, StepInterpolation.Centered));
      Assert.AreEqual(1, InterpolationHelper.Step(1, 10, 0.4f, StepInterpolation.Centered));
      Assert.AreEqual(10, InterpolationHelper.Step(1, 10, 0.6f, StepInterpolation.Centered));
    }


    [Test]
    public void StepDoublePrecision()
    {
      Assert.AreEqual(1, InterpolationHelper.Step(1.0, 2.0, 0.0, StepInterpolation.Left));
      Assert.AreEqual(2, InterpolationHelper.Step(1.0, 2.0, 0.5, StepInterpolation.Left));
      Assert.AreEqual(2, InterpolationHelper.Step(1.0, 2.0, 1.0, StepInterpolation.Left));

      Assert.AreEqual(1, InterpolationHelper.Step(1.0, 2.0, 0.0, StepInterpolation.Right));
      Assert.AreEqual(1, InterpolationHelper.Step(1.0, 2.0, 0.5, StepInterpolation.Right));
      Assert.AreEqual(2, InterpolationHelper.Step(1.0, 2.0, 1.0, StepInterpolation.Right));

      Assert.AreEqual(1, InterpolationHelper.Step(1.0, 2.0, 0.0, StepInterpolation.Centered));
      Assert.AreEqual(1, InterpolationHelper.Step(1.0, 2.0, 0.3, StepInterpolation.Centered));
      Assert.AreEqual(2, InterpolationHelper.Step(1.0, 2.0, 0.6, StepInterpolation.Centered));
      Assert.AreEqual(2, InterpolationHelper.Step(1.0, 2.0, 1.0, StepInterpolation.Centered));
      Assert.AreEqual(1, InterpolationHelper.Step(1.0, 10.0, 0.4, StepInterpolation.Centered));
      Assert.AreEqual(10, InterpolationHelper.Step(1.0, 10.0, 0.6, StepInterpolation.Centered));
    }


    //[Test]
    //public void Hermite()
    //{
    //  Vector2F v1 = new Vector2F(1.0f, 2.0f);
    //  Vector2F v2 = new Vector2F(-1.0f, 2.1f);
    //  Vector2F t1 = new Vector2F(2.0f, 1.0f); t1.Normalize();
    //  Vector2F t2 = new Vector2F(0.0f, 2.0f); t2.Normalize();

    //  Vector2F hermite = Vector2F.Hermite(v1, t1, v2, t2, 0.0f);
    //  Assert.IsTrue(Vector2F.AreNumericallyEqual(v1, hermite));

    //  hermite = Vector2F.Hermite(v1, t1, v2, t2, 1.0f);
    //  Assert.IsTrue(Vector2F.AreNumericallyEqual(v2, hermite));
    //}


    //[Test]
    //public void CatmullRom()
    //{
    //  Vector2F v1 = new Vector2F(1.0f, 2.0f);
    //  Vector2F v2 = new Vector2F(-1.0f, 2.1f);
    //  Vector2F v3 = new Vector2F(2.0f, 1.0f);
    //  Vector2F v4 = new Vector2F(0.0f, 2.0f);

    //  Vector2F catmullRom = Vector2F.CatmullRom(v1, v2, v3, v4, 0.0f);
    //  Assert.IsTrue(Vector2F.AreNumericallyEqual(v2, catmullRom));

    //  catmullRom = Vector2F.CatmullRom(v1, v2, v3, v4, 1.0f);
    //  Assert.IsTrue(Vector2F.AreNumericallyEqual(v3, catmullRom));

    //  Vector2F t2 = (v3 - v1) / 2.0f;
    //  Vector2F t3 = (v4 - v2) / 2.0f;
    //  Vector2F hermite = Vector2F.Hermite(v2, t2, v3, t3, 0.3f);
    //  catmullRom = Vector2F.CatmullRom(v1, v2, v3, v4, 0.3f);
    //  Assert.IsTrue(Vector2F.AreNumericallyEqual(hermite, catmullRom));

    //  hermite = Vector2F.Hermite(v2, t2, v3, t3, 0.6f);
    //  catmullRom = Vector2F.CatmullRom(v1, v2, v3, v4, 0.6f);
    //  Assert.IsTrue(Vector2F.AreNumericallyEqual(hermite, catmullRom));
    //}


    //[Test]
    //public void Hermite()
    //{
    //  Vector3F v1 = new Vector3F(1.0f, 2.0f, 3.0f);
    //  Vector3F v2 = new Vector3F(-1.0f, 2.1f, 3.0f);
    //  Vector3F t1 = new Vector3F(2.0f, 1.0f, 5.0f); t1.Normalize();
    //  Vector3F t2 = new Vector3F(0.0f, 2.0f, -3.0f); t2.Normalize();

    //  Vector3F hermite = Vector3F.Hermite(v1, t1, v2, t2, 0.0f);
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(v1, hermite));

    //  hermite = Vector3F.Hermite(v1, t1, v2, t2, 1.0f);
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(v2, hermite));
    //}


    //[Test]
    //public void CatmullRom()
    //{
    //  Vector3F v1 = new Vector3F(1.0f, 2.0f, 3.0f);
    //  Vector3F v2 = new Vector3F(-1.0f, 2.1f, 3.0f);
    //  Vector3F v3 = new Vector3F(2.0f, 1.0f, 5.0f);
    //  Vector3F v4 = new Vector3F(0.0f, 2.0f, -3.0f);

    //  Vector3F catmullRom = Vector3F.CatmullRom(v1, v2, v3, v4, 0.0f);
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(v2, catmullRom));

    //  catmullRom = Vector3F.CatmullRom(v1, v2, v3, v4, 1.0f);
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(v3, catmullRom));

    //  Vector3F t2 = (v3 - v1) / 2.0f;
    //  Vector3F t3 = (v4 - v2) / 2.0f;
    //  Vector3F hermite = Vector3F.Hermite(v2, t2, v3, t3, 0.3f);
    //  catmullRom = Vector3F.CatmullRom(v1, v2, v3, v4, 0.3f);
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(hermite, catmullRom));

    //  hermite = Vector3F.Hermite(v2, t2, v3, t3, 0.6f);
    //  catmullRom = Vector3F.CatmullRom(v1, v2, v3, v4, 0.6f);
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(hermite, catmullRom));
    //}




    //[Test]
    //public void Hermite()
    //{
    //  Vector4F v1 = new Vector4F(1.0f, 2.0f, 3.0f, 1.0f);
    //  Vector4F v2 = new Vector4F(-1.0f, 2.1f, 3.0f, 1.0f);
    //  Vector4F t1 = new Vector4F(2.0f, 1.0f, 5.0f, 0.0f); t1.Normalize();
    //  Vector4F t2 = new Vector4F(0.0f, 2.0f, -3.0f, 0.0f); t2.Normalize();

    //  Vector4F hermite = Vector4F.Hermite(v1, t1, v2, t2, 0.0f);
    //  Assert.IsTrue(Vector4F.AreNumericallyEqual(v1, hermite));

    //  hermite = Vector4F.Hermite(v1, t1, v2, t2, 1.0f);
    //  Assert.IsTrue(Vector4F.AreNumericallyEqual(v2, hermite));
    //}


    //[Test]
    //public void CatmullRom()
    //{
    //  Vector4F v1 = new Vector4F(1.0f, 2.0f, 3.0f, 1.0f);
    //  Vector4F v2 = new Vector4F(-1.0f, 2.1f, 3.0f, 1.0f);
    //  Vector4F v3 = new Vector4F(2.0f, 1.0f, 5.0f, 1.0f);
    //  Vector4F v4 = new Vector4F(0.0f, 2.0f, -3.0f, 1.0f);

    //  Vector4F catmullRom = Vector4F.CatmullRom(v1, v2, v3, v4, 0.0f);
    //  Assert.IsTrue(Vector4F.AreNumericallyEqual(v2, catmullRom));

    //  catmullRom = Vector4F.CatmullRom(v1, v2, v3, v4, 1.0f);
    //  Assert.IsTrue(Vector4F.AreNumericallyEqual(v3, catmullRom));

    //  Vector4F t2 = (v3 - v1) / 2.0f;
    //  Vector4F t3 = (v4 - v2) / 2.0f;
    //  Vector4F hermite = Vector4F.Hermite(v2, t2, v3, t3, 0.3f);
    //  catmullRom = Vector4F.CatmullRom(v1, v2, v3, v4, 0.3f);
    //  Assert.IsTrue(Vector4F.AreNumericallyEqual(hermite, catmullRom));

    //  hermite = Vector4F.Hermite(v2, t2, v3, t3, 0.6f);
    //  catmullRom = Vector4F.CatmullRom(v1, v2, v3, v4, 0.6f);
    //  Assert.IsTrue(Vector4F.AreNumericallyEqual(hermite, catmullRom));
    //}


    [Test]
    public void CosineInterpolationSinglePrecision()
    {
      Assert.AreEqual(1.0f, InterpolationHelper.CosineInterpolation(1.0f, 2.0f, 0.0f));
      Assert.AreEqual(1.5f, InterpolationHelper.CosineInterpolation(1.0f, 2.0f, 0.5f));
      Assert.AreEqual(2.0f, InterpolationHelper.CosineInterpolation(1.0f, 2.0f, 1.0f));
      Assert.AreEqual(1.5f, InterpolationHelper.CosineInterpolation(2.0f, 1.0f, 0.5f));
    }


    [Test]
    public void CosineInterpolationDoublePrecision()
    {
      Assert.AreEqual(1.0, InterpolationHelper.CosineInterpolation(1.0, 2.0, 0.0));
      Assert.AreEqual(1.5, InterpolationHelper.CosineInterpolation(1.0, 2.0, 0.5));
      Assert.AreEqual(2.0, InterpolationHelper.CosineInterpolation(1.0, 2.0, 1.0));
      Assert.AreEqual(1.5, InterpolationHelper.CosineInterpolation(2.0, 1.0, 0.5));
    }


    [Test]
    public void CosineInterpolationVector2F()
    {
      Vector2F v = new Vector2F(1.0f, 10.0f);
      Vector2F w = new Vector2F(2.0f, 20.0f);
      Vector2F lerp0 = InterpolationHelper.CosineInterpolation(v, w, 0.0f);
      Vector2F lerp1 = InterpolationHelper.CosineInterpolation(v, w, 1.0f);
      Vector2F lerp05 = InterpolationHelper.CosineInterpolation(v, w, 0.5f);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new Vector2F(1.5f, 15.0f), lerp05);
    }


    [Test]
    public void CosineInterpolationVector2D()
    {
      Vector2D v = new Vector2D(1.0, 10.0);
      Vector2D w = new Vector2D(2.0, 20.0);
      Vector2D lerp0 = InterpolationHelper.CosineInterpolation(v, w, 0.0);
      Vector2D lerp1 = InterpolationHelper.CosineInterpolation(v, w, 1.0);
      Vector2D lerp05 = InterpolationHelper.CosineInterpolation(v, w, 0.5);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.IsTrue(Vector2D.AreNumericallyEqual(new Vector2D(1.5, 15.0), lerp05));
    }


    [Test]
    public void CosineInterpolationVector3F()
    {
      Vector3F v = new Vector3F(1.0f, 10.0f, 100.0f);
      Vector3F w = new Vector3F(2.0f, 20.0f, 200.0f);
      Vector3F lerp0 = InterpolationHelper.CosineInterpolation(v, w, 0.0f);
      Vector3F lerp1 = InterpolationHelper.CosineInterpolation(v, w, 1.0f);
      Vector3F lerp05 = InterpolationHelper.CosineInterpolation(v, w, 0.5f);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new Vector3F(1.5f, 15.0f, 150.0f), lerp05);
    }


    [Test]
    public void CosineInterpolationVector3D()
    {
      Vector3D v = new Vector3D(1.0, 10.0, 100.0);
      Vector3D w = new Vector3D(2.0, 20.0, 200.0);
      Vector3D lerp0 = InterpolationHelper.CosineInterpolation(v, w, 0.0);
      Vector3D lerp1 = InterpolationHelper.CosineInterpolation(v, w, 1.0);
      Vector3D lerp05 = InterpolationHelper.CosineInterpolation(v, w, 0.5);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(1.5, 15.0, 150.0), lerp05));
    }


    [Test]
    public void CosineInterpolationVector4F()
    {
      Vector4F v = new Vector4F(1.0f, 10.0f, 100.0f, 1000.0f);
      Vector4F w = new Vector4F(2.0f, 20.0f, 200.0f, 2000.0f);
      Vector4F lerp0 = InterpolationHelper.CosineInterpolation(v, w, 0.0f);
      Vector4F lerp1 = InterpolationHelper.CosineInterpolation(v, w, 1.0f);
      Vector4F lerp05 = InterpolationHelper.CosineInterpolation(v, w, 0.5f);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new Vector4F(1.5f, 15.0f, 150.0f, 1500.0f), lerp05);
    }


    [Test]
    public void CosineInterpolationVector4D()
    {
      Vector4D v = new Vector4D(1.0, 10.0, 100.0, 1000.0);
      Vector4D w = new Vector4D(2.0, 20.0, 200.0, 2000.0);
      Vector4D lerp0 = InterpolationHelper.CosineInterpolation(v, w, 0.0);
      Vector4D lerp1 = InterpolationHelper.CosineInterpolation(v, w, 1.0);
      Vector4D lerp05 = InterpolationHelper.CosineInterpolation(v, w, 0.5);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(1.5, 15.0, 150.0, 1500.0), lerp05));
    }


    [Test]
    public void CosineInterpolationVectorF()
    {
      VectorF v = new VectorF(new[] { 1.0f, 10.0f, 100.0f, 1000.0f });
      VectorF w = new VectorF(new[] { 2.0f, 20.0f, 200.0f, 2000.0f });
      VectorF lerp0 = InterpolationHelper.CosineInterpolation(v, w, 0.0f);
      VectorF lerp1 = InterpolationHelper.CosineInterpolation(v, w, 1.0f);
      VectorF lerp05 = InterpolationHelper.CosineInterpolation(v, w, 0.5f);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.AreEqual(new VectorF(new[] { 1.5f, 15.0f, 150.0f, 1500.0f }), lerp05);
    }


    [Test]
    public void CosineInterpolationVectorD()
    {
      VectorD v = new VectorD(new[] { 1.0, 10.0, 100.0, 1000.0 });
      VectorD w = new VectorD(new[] { 2.0, 20.0, 200.0, 2000.0 });
      VectorD lerp0 = InterpolationHelper.CosineInterpolation(v, w, 0.0);
      VectorD lerp1 = InterpolationHelper.CosineInterpolation(v, w, 1.0);
      VectorD lerp05 = InterpolationHelper.CosineInterpolation(v, w, 0.5);

      Assert.AreEqual(v, lerp0);
      Assert.AreEqual(w, lerp1);
      Assert.IsTrue(VectorD.AreNumericallyEqual(new VectorD(new[] { 1.5, 15.0, 150.0, 1500.0 }), lerp05));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CosineInterpolationVectorFException()
    {
      InterpolationHelper.CosineInterpolation(null, new VectorF(), 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CosineInterpolationVectorDException()
    {
      InterpolationHelper.CosineInterpolation(null, new VectorD(), 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CosineInterpolationVectorFException2()
    {
      InterpolationHelper.CosineInterpolation(new VectorF(), null, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CosineInterpolationVectorDException2()
    {
      InterpolationHelper.CosineInterpolation(new VectorD(), null, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CosineInterpolationVectorFException3()
    {
      InterpolationHelper.CosineInterpolation(new VectorF(3), new VectorF(4), 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CosineInterpolationVectorDException3()
    {
      InterpolationHelper.CosineInterpolation(new VectorD(3), new VectorD(4), 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PolynomialInterpolationSinglePrecisionException()
    {
      InterpolationHelper.PolynomialInterpolation(null, 0);
    }



    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PolynomialInterpolationDoublePrecisionException()
    {
      InterpolationHelper.PolynomialInterpolation(null, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void PolynomialInterpolationSinglePrecisionException2()
    {
      InterpolationHelper.PolynomialInterpolation(new List<Vector2F>(), 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void PolynomialInterpolationDoublePrecisionException2()
    {
      InterpolationHelper.PolynomialInterpolation(new List<Vector2D>(), 0);
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void PolynomialInterpolationSinglePrecisionException3()
    {
      // Error: 2 identical x values.
      var points = new[] { new Vector2F(0, 1), new Vector2F(0, 4), new Vector2F(5, -1) };
      InterpolationHelper.PolynomialInterpolation(points, 0);
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void PolynomialInterpolationDoublePrecisionException3()
    {
      // Error: 2 identical x values.
      var points = new[] { new Vector2D(0, 1), new Vector2D(0, 4), new Vector2D(5, -1) };
      InterpolationHelper.PolynomialInterpolation(points, 0);
    }


    [Test]
    public void PolynomialInterpolationSinglePrecision()
    {
      var points = new[] { new Vector2F(0, 1), new Vector2F(3, 4), new Vector2F(5, -1) };

      Assert.IsTrue(Numeric.AreEqual(points[0].Y, InterpolationHelper.PolynomialInterpolation(points, points[0].X)));
      Assert.IsTrue(Numeric.AreEqual(points[1].Y, InterpolationHelper.PolynomialInterpolation(points, points[1].X)));
      Assert.IsTrue(Numeric.AreEqual(points[2].Y, InterpolationHelper.PolynomialInterpolation(points, points[2].X)));
    }


    [Test]
    public void PolynomialInterpolationDoublePrecision()
    {
      var points = new[] { new Vector2D(0, 1), new Vector2D(3, 4), new Vector2D(5, -1) };

      Assert.IsTrue(Numeric.AreEqual(points[0].Y, InterpolationHelper.PolynomialInterpolation(points, points[0].X)));
      Assert.IsTrue(Numeric.AreEqual(points[1].Y, InterpolationHelper.PolynomialInterpolation(points, points[1].X)));
      Assert.IsTrue(Numeric.AreEqual(points[2].Y, InterpolationHelper.PolynomialInterpolation(points, points[2].X)));
    }


    [Test]
    public void SlerpSinglePrecision()
    {
      // Warning: The not all results are not verified
      QuaternionF q1 = new QuaternionF(1.0f, 2.0f, 3.0f, 4.0f).Normalized;
      QuaternionF q2 = new QuaternionF(2.0f, 4.0f, 6.0f, 8.0f).Normalized;
      QuaternionF slerp = InterpolationHelper.Slerp(q1, q2, 0.75f);
      Assert.IsTrue(slerp.IsNumericallyNormalized);

      slerp = InterpolationHelper.Slerp(q1, q2, 0);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(q1, slerp));

      slerp = InterpolationHelper.Slerp(q1, q2, 1);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(q2, slerp));
    }


    [Test]
    public void SlerpDoublePrecision()
    {
      // Warning: The not all results are not verified
      QuaternionD q1 = new QuaternionD(1.0, 2.0, 3.0, 4.0).Normalized;
      QuaternionD q2 = new QuaternionD(2.0, 4.0, 6.0, 8.0).Normalized;
      QuaternionD slerp = InterpolationHelper.Slerp(q1, q2, 0.75);
      Assert.IsTrue(slerp.IsNumericallyNormalized);

      slerp = InterpolationHelper.Slerp(q1, q2, 0);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Assert.IsTrue(QuaternionD.AreNumericallyEqual(q1, slerp));

      slerp = InterpolationHelper.Slerp(q1, q2, 1);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Assert.IsTrue(QuaternionD.AreNumericallyEqual(q2, slerp));
    }



    [Test]
    public void SlerpZSinglePrecision()
    {
      QuaternionF q1 = QuaternionF.Identity;
      QuaternionF q2 = QuaternionF.CreateRotation(Vector3F.UnitZ, (float)Math.PI / 2);
      QuaternionF slerp = InterpolationHelper.Slerp(q1, q2, 0.5f);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Vector3F v = slerp.Rotate(Vector3F.UnitX);
      Vector3F result = new Vector3F(1.0f, 1.0f, 0.0f).Normalized;
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result, v));
    }


    [Test]
    public void SlerpZDoublePrecision()
    {
      QuaternionD q1 = QuaternionD.Identity;
      QuaternionD q2 = QuaternionD.CreateRotation(Vector3F.UnitZ, Math.PI / 2);
      QuaternionD slerp = InterpolationHelper.Slerp(q1, q2, 0.5);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Vector3D v = slerp.Rotate(Vector3D.UnitX);
      Vector3D result = new Vector3D(1.0, 1.0, 0.0).Normalized;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(result, v));
    }


    [Test]
    public void SlerpYSinglePrecision()
    {
      QuaternionF q1 = QuaternionF.Identity;
      QuaternionF q2 = QuaternionF.CreateRotation(Vector3F.UnitY, (float) Math.PI / 2);
      QuaternionF slerp = InterpolationHelper.Slerp(q1, q2, 0.5f);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Vector3F v = slerp.Rotate(Vector3F.UnitZ);
      Vector3F result = new Vector3F(1.0f, 0.0f, 1.0f).Normalized;
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result, v));
    }


    [Test]
    public void SlerpYDoublePrecision()
    {
      QuaternionD q1 = QuaternionD.Identity;
      QuaternionD q2 = QuaternionD.CreateRotation(Vector3F.UnitY, Math.PI / 2);
      QuaternionD slerp = InterpolationHelper.Slerp(q1, q2, 0.5);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Vector3D v = slerp.Rotate(Vector3D.UnitZ);
      Vector3D result = new Vector3D(1.0, 0.0, 1.0).Normalized;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(result, v));
    }


    [Test]
    public void SlerpXSinglePrecision()
    {
      QuaternionF q1 = QuaternionF.Identity;
      QuaternionF q2 = QuaternionF.CreateRotation(Vector3F.UnitX, (float) Math.PI / 2);
      QuaternionF slerp = InterpolationHelper.Slerp(q1, q2, 0.5f);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Vector3F v = slerp.Rotate(Vector3F.UnitY);
      Vector3F result = new Vector3F(0.0f, 1.0f, 1.0f).Normalized;
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result, v));
    }


    [Test]
    public void SlerpXDoublePrecision()
    {
      QuaternionD q1 = QuaternionD.Identity;
      QuaternionD q2 = QuaternionD.CreateRotation(Vector3F.UnitX, Math.PI / 2);
      QuaternionD slerp = InterpolationHelper.Slerp(q1, q2, 0.5);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Vector3D v = slerp.Rotate(Vector3F.UnitY);
      Vector3D result = new Vector3D(0.0, 1.0, 1.0).Normalized;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(result, v));
    }


    [Test]
    public void SlerpNegatedSinglePrecision()
    {
      QuaternionF q1 = new QuaternionF(-1.0f, 0.0f, 0.0f, 0.0f);
      QuaternionF q2 = QuaternionF.CreateRotation(-Vector3F.UnitZ, (float) -Math.PI / 2);
      QuaternionF slerp = InterpolationHelper.Slerp(q1, q2, 0.5f);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Vector3F v = slerp.Rotate(Vector3F.UnitX);
      Vector3F result = new Vector3F(1.0f, 1.0f, 0.0f).Normalized;
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result, v));
    }


    [Test]
    public void SlerpNegatedDoublePrecision()
    {
      QuaternionD q1 = new QuaternionD(-1.0, 0.0, 0.0, 0.0);
      QuaternionD q2 = QuaternionD.CreateRotation(-Vector3F.UnitZ, -Math.PI / 2);
      QuaternionD slerp = InterpolationHelper.Slerp(q1, q2, 0.5);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Vector3D v = slerp.Rotate(Vector3D.UnitX);
      Vector3D result = new Vector3D(1.0, 1.0, 0.0).Normalized;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(result, v));
    }


    [Test]
    public void SlerpGeneralSinglePrecision()
    {
      QuaternionF q1 = QuaternionF.CreateRotation(-Vector3F.UnitY, (float) Math.PI / 2);
      QuaternionF q2 = QuaternionF.CreateRotation(Vector3F.UnitZ, (float) Math.PI / 2);
      QuaternionF slerp = InterpolationHelper.Slerp(q1, q2, 0.5f);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Vector3F v = slerp.Rotate(Vector3F.UnitX);
      Vector3F result = new Vector3F(1.0f / 3.0f, 2.0f / 3.0f, 2.0f / 3.0f);  // I hope this is correct.
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result, v));

      q1 = QuaternionF.CreateRotation(-Vector3F.UnitY, (float) Math.PI / 2);
      q2 = QuaternionF.CreateRotation(-Vector3F.UnitZ, (float) -Math.PI / 2);
      slerp = InterpolationHelper.Slerp(q1, q2, 0.5f);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      v = slerp.Rotate(Vector3F.UnitX);
      result = new Vector3F(1.0f / 3.0f, 2.0f / 3.0f, 2.0f / 3.0f);  // I hope this is correct.
      Assert.IsTrue(Vector3F.AreNumericallyEqual(result, v));
    }


    [Test]
    public void SlerpGeneralDoublePrecision()
    {
      QuaternionD q1 = QuaternionD.CreateRotation(-Vector3D.UnitY, Math.PI / 2);
      QuaternionD q2 = QuaternionD.CreateRotation(Vector3D.UnitZ, Math.PI / 2);
      QuaternionD slerp = InterpolationHelper.Slerp(q1, q2, 0.5);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      Vector3D v = slerp.Rotate(Vector3D.UnitX);
      Vector3D result = new Vector3D(1.0 / 3.0, 2.0 / 3.0, 2.0 / 3.0);  // I hope this is correct.
      Assert.IsTrue(Vector3D.AreNumericallyEqual(result, v));

      q1 = QuaternionD.CreateRotation(-Vector3D.UnitY, Math.PI / 2);
      q2 = QuaternionD.CreateRotation(-Vector3D.UnitZ, -Math.PI / 2);
      slerp = InterpolationHelper.Slerp(q1, q2, 0.5);
      Assert.IsTrue(slerp.IsNumericallyNormalized);
      v = slerp.Rotate(Vector3D.UnitX);
      result = new Vector3D(1.0 / 3.0, 2.0 / 3.0, 2.0 / 3.0);  // I hope this is correct.
      Assert.IsTrue(Vector3D.AreNumericallyEqual(result, v));
    }


    [Test]
    public void SquadSinglePrecision()
    {
      QuaternionF q0 = QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.3f);
      QuaternionF q1 = QuaternionF.CreateRotation(new Vector3F(1, 0, 1), 0.4f);
      QuaternionF q2 = QuaternionF.CreateRotation(new Vector3F(1, 0, -1), -0.6f);
      QuaternionF q3 = QuaternionF.CreateRotation(new Vector3F(0, 1, 1), 0.2f);

      QuaternionF q, a, b, p;
      QuaternionF expected;

      InterpolationHelper.SquadSetup(q0, q1, q2, q3, out q, out a, out b, out p);

      // t = 0
      QuaternionF result = InterpolationHelper.Squad(q, a, b, p, 0.0f);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(q1, result));

      // t = 1.0f
      result = InterpolationHelper.Squad(q, a, b, p, 1.0f);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(q2, result));

      // Check series (just for debugging)
      QuaternionF r1, r2, r3, r4, r5, r6, r7, r8, r9;
      r1 = InterpolationHelper.Squad(q, a, b, p, 0.1f);
      r2 = InterpolationHelper.Squad(q, a, b, p, 0.2f);
      r3 = InterpolationHelper.Squad(q, a, b, p, 0.3f);
      r4 = InterpolationHelper.Squad(q, a, b, p, 0.4f);
      r5 = InterpolationHelper.Squad(q, a, b, p, 0.5f);
      r6 = InterpolationHelper.Squad(q, a, b, p, 0.6f);
      r7 = InterpolationHelper.Squad(q, a, b, p, 0.7f);
      r8 = InterpolationHelper.Squad(q, a, b, p, 0.8f);
      r9 = InterpolationHelper.Squad(q, a, b, p, 0.9f);

      // q0 = q1, q2 = q3
      InterpolationHelper.SquadSetup(q1, q1, q2, q2, out q, out a, out b, out p);
      result = InterpolationHelper.Squad(q, a, b, p, 0.5f);
      expected = InterpolationHelper.Slerp(q1, q2, 0.5f);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(expected, result));
    }


    [Test]
    public void SquadDoublePrecision()
    {
      QuaternionD q0 = QuaternionD.CreateRotation(new Vector3D(1, 1, 1), 0.3);
      QuaternionD q1 = QuaternionD.CreateRotation(new Vector3D(1, 0, 1), 0.4);
      QuaternionD q2 = QuaternionD.CreateRotation(new Vector3D(1, 0, -1), -0.6);
      QuaternionD q3 = QuaternionD.CreateRotation(new Vector3D(0, 1, 1), 0.2);

      QuaternionD q, a, b, p;
      QuaternionD expected;

      InterpolationHelper.SquadSetup(q0, q1, q2, q3, out q, out a, out b, out p);

      // t = 0
      QuaternionD result = InterpolationHelper.Squad(q, a, b, p, 0.0);
      Assert.IsTrue(QuaternionD.AreNumericallyEqual(q1, result));

      // t = 1.0f
      result = InterpolationHelper.Squad(q, a, b, p, 1.0);
      Assert.IsTrue(QuaternionD.AreNumericallyEqual(q2, result));

      // Check series (just for debugging)
      QuaternionD r1, r2, r3, r4, r5, r6, r7, r8, r9;
      r1 = InterpolationHelper.Squad(q, a, b, p, 0.1);
      r2 = InterpolationHelper.Squad(q, a, b, p, 0.2);
      r3 = InterpolationHelper.Squad(q, a, b, p, 0.3);
      r4 = InterpolationHelper.Squad(q, a, b, p, 0.4);
      r5 = InterpolationHelper.Squad(q, a, b, p, 0.5);
      r6 = InterpolationHelper.Squad(q, a, b, p, 0.6);
      r7 = InterpolationHelper.Squad(q, a, b, p, 0.7);
      r8 = InterpolationHelper.Squad(q, a, b, p, 0.8);
      r9 = InterpolationHelper.Squad(q, a, b, p, 0.9);

      // q0 = q1, q2 = q3
      InterpolationHelper.SquadSetup(q1, q1, q2, q2, out q, out a, out b, out p);
      result = InterpolationHelper.Squad(q, a, b, p, 0.5);
      expected = InterpolationHelper.Slerp(q1, q2, 0.5);
      Assert.IsTrue(QuaternionD.AreNumericallyEqual(expected, result));
    }


    [Test]
    public void HermiteSmoothStepD()
    {
      Assert.IsTrue(Numeric.AreEqual(0, InterpolationHelper.HermiteSmoothStep(-1.0f)));
      Assert.IsTrue(Numeric.AreEqual(0, InterpolationHelper.HermiteSmoothStep(0.0f)));
      Assert.IsTrue(Numeric.AreEqual(0.5, InterpolationHelper.HermiteSmoothStep(0.5f)));
      Assert.IsTrue(Numeric.AreEqual(1, InterpolationHelper.HermiteSmoothStep(1.0f)));
      Assert.IsTrue(Numeric.AreEqual(1, InterpolationHelper.HermiteSmoothStep(2.0f)));
      Assert.IsTrue(Numeric.AreEqual(1 - InterpolationHelper.HermiteSmoothStep(1 - 0.3f), InterpolationHelper.HermiteSmoothStep(0.3f)));
      Assert.Greater(InterpolationHelper.HermiteSmoothStep(1 - 0.3f), InterpolationHelper.HermiteSmoothStep(0.3f));
    }


    [Test]
    public void HermiteSmoothStepF()
    {
      Assert.IsTrue(Numeric.AreEqual(0, InterpolationHelper.HermiteSmoothStep(-1.0)));
      Assert.IsTrue(Numeric.AreEqual(0, InterpolationHelper.HermiteSmoothStep(0.0)));
      Assert.IsTrue(Numeric.AreEqual(0.5, InterpolationHelper.HermiteSmoothStep(0.5)));
      Assert.IsTrue(Numeric.AreEqual(1, InterpolationHelper.HermiteSmoothStep(1.0)));
      Assert.IsTrue(Numeric.AreEqual(1, InterpolationHelper.HermiteSmoothStep(2.0)));
      Assert.IsTrue(Numeric.AreEqual(1 - InterpolationHelper.HermiteSmoothStep(1 - 0.3), InterpolationHelper.HermiteSmoothStep(0.3)));
      Assert.Greater(InterpolationHelper.HermiteSmoothStep(1 - 0.3), InterpolationHelper.HermiteSmoothStep(0.3));
    }


    [Test]
    public void EaseInOutSmoothStepF()
    {
      Assert.IsTrue(Numeric.AreEqual(0, InterpolationHelper.EaseInOutSmoothStep(-1.0f)));
      Assert.IsTrue(Numeric.AreEqual(0, InterpolationHelper.EaseInOutSmoothStep(0.0f)));
      Assert.IsTrue(Numeric.AreEqual(0.5, InterpolationHelper.EaseInOutSmoothStep(0.5f)));
      Assert.IsTrue(Numeric.AreEqual(1, InterpolationHelper.EaseInOutSmoothStep(1.0f)));
      Assert.IsTrue(Numeric.AreEqual(1, InterpolationHelper.EaseInOutSmoothStep(2.0f)));
      Assert.IsTrue(Numeric.AreEqual(1 - InterpolationHelper.EaseInOutSmoothStep(1 - 0.3f), InterpolationHelper.EaseInOutSmoothStep(0.3f)));
      Assert.Greater(InterpolationHelper.EaseInOutSmoothStep(1 - 0.3f), InterpolationHelper.EaseInOutSmoothStep(0.3f));

      Assert.Greater(0.5f, InterpolationHelper.EaseInOutSmoothStep(0.3f));
      Assert.Less(0.5f, InterpolationHelper.EaseInOutSmoothStep(0.6f));
    }


    [Test]
    public void EaseInOutSmoothStepD()
    {
      Assert.IsTrue(Numeric.AreEqual(0, InterpolationHelper.EaseInOutSmoothStep(-1.0)));
      Assert.IsTrue(Numeric.AreEqual(0, InterpolationHelper.EaseInOutSmoothStep(0.0)));
      Assert.IsTrue(Numeric.AreEqual(0.5, InterpolationHelper.EaseInOutSmoothStep(0.5)));
      Assert.IsTrue(Numeric.AreEqual(1, InterpolationHelper.EaseInOutSmoothStep(1.0)));
      Assert.IsTrue(Numeric.AreEqual(1, InterpolationHelper.EaseInOutSmoothStep(2.0)));
      Assert.IsTrue(Numeric.AreEqual(1 - InterpolationHelper.EaseInOutSmoothStep(1 - 0.3), InterpolationHelper.EaseInOutSmoothStep(0.3)));
      Assert.Greater(InterpolationHelper.EaseInOutSmoothStep(1 - 0.3), InterpolationHelper.EaseInOutSmoothStep(0.3));

      Assert.Greater(0.5f, InterpolationHelper.EaseInOutSmoothStep(0.3));
      Assert.Less(0.5f, InterpolationHelper.EaseInOutSmoothStep(0.6));
    }
  }  
}
