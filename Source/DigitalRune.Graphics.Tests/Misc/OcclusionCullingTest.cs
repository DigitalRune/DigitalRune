using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Graphics.Tests
{
  [TestFixture]
  public class OcclusionCullingTest
  {
    [Test]
    public void GetBoundsOrthographic()
    {
      // Get bounds of AABB in clip space. (Used in OcclusionCulling.fx.)
      Vector3F cameraPosition = new Vector3F(100, -200, 345);
      Vector3F cameraForward = new Vector3F(1, 2, 3).Normalized;
      Vector3F cameraUp = new Vector3F(-1, 0.5f, -2).Normalized;

      Matrix44F view = Matrix44F.CreateLookAt(cameraPosition, cameraPosition + cameraForward, cameraUp);
      Matrix44F proj = Matrix44F.CreateOrthographic(16, 9, -10, 100);
      Matrix44F viewProj = proj * view;

      Vector3F center = new Vector3F();
      Vector3F halfExtent = new Vector3F();
      Aabb aabb = new Aabb(center - halfExtent, center + halfExtent);
      Aabb aabb0, aabb1;
      GetBoundsOrtho(aabb, viewProj, out aabb0);
      GetBoundsOrthoSmart(aabb, viewProj, out aabb1);
      Assert.IsTrue(Aabb.AreNumericallyEqual(aabb0, aabb1));

      center = new Vector3F(-9, 20, -110);
      halfExtent = new Vector3F(5, 2, 10);
      aabb = new Aabb(center - halfExtent, center + halfExtent);
      GetBoundsOrtho(aabb, viewProj, out aabb0);
      GetBoundsOrthoSmart(aabb, viewProj, out aabb1);
      Assert.IsTrue(Aabb.AreNumericallyEqual(aabb0, aabb1));
    }


    private void GetBoundsOrtho(Aabb aabbWorld, Matrix44F viewProj, out Aabb aabbClip)
    {
      Vector3F minimum = aabbWorld.Minimum;
      Vector3F maximum = aabbWorld.Maximum;

      Vector3F v0 = (viewProj * new Vector4F(minimum.X, minimum.Y, minimum.Z, 1)).XYZ;
      Vector3F minimumClip = v0;
      Vector3F maximumClip = v0;
      Vector3F v1 = (viewProj * new Vector4F(maximum.X, minimum.Y, minimum.Z, 1)).XYZ;
      minimumClip = Vector3F.Min(minimumClip, v1);
      maximumClip = Vector3F.Max(maximumClip, v1);
      Vector3F v2 = (viewProj * new Vector4F(minimum.X, maximum.Y, minimum.Z, 1)).XYZ;
      minimumClip = Vector3F.Min(minimumClip, v2);
      maximumClip = Vector3F.Max(maximumClip, v2);
      Vector3F v3 = (viewProj * new Vector4F(maximum.X, maximum.Y, minimum.Z, 1)).XYZ;
      minimumClip = Vector3F.Min(minimumClip, v3);
      maximumClip = Vector3F.Max(maximumClip, v3);
      Vector3F v4 = (viewProj * new Vector4F(minimum.X, minimum.Y, maximum.Z, 1)).XYZ;
      minimumClip = Vector3F.Min(minimumClip, v4);
      maximumClip = Vector3F.Max(maximumClip, v4);
      Vector3F v5 = (viewProj * new Vector4F(maximum.X, minimum.Y, maximum.Z, 1)).XYZ;
      minimumClip = Vector3F.Min(minimumClip, v5);
      maximumClip = Vector3F.Max(maximumClip, v5);
      Vector3F v6 = (viewProj * new Vector4F(minimum.X, maximum.Y, maximum.Z, 1)).XYZ;
      minimumClip = Vector3F.Min(minimumClip, v6);
      maximumClip = Vector3F.Max(maximumClip, v6);
      Vector3F v7 = (viewProj * new Vector4F(maximum.X, maximum.Y, maximum.Z, 1)).XYZ;
      minimumClip = Vector3F.Min(minimumClip, v7);
      maximumClip = Vector3F.Max(maximumClip, v7);

      aabbClip.Minimum = minimumClip;
      aabbClip.Maximum = maximumClip;
    }


    private void GetBoundsOrthoSmart(Aabb aabbWorld, Matrix44F viewProj, out Aabb aabbClip)
    {
      Vector3F minimum = aabbWorld.Minimum;
      Vector3F maximum = aabbWorld.Maximum;
      Vector3F extent = maximum - minimum;

      Vector3F v0 = (viewProj * new Vector4F(minimum.X, minimum.Y, minimum.Z, 1)).XYZ;
      Vector3F minimumClip = v0;
      Vector3F maximumClip = v0;

      Vector3F d0 = extent.X * viewProj.GetColumn(0).XYZ;
      Vector3F d1 = extent.Y * viewProj.GetColumn(1).XYZ;
      Vector3F d2 = extent.Z * viewProj.GetColumn(2).XYZ;

      Vector3F v1 = v0 + d0;
      minimumClip = Vector3F.Min(minimumClip, v1);
      maximumClip = Vector3F.Max(maximumClip, v1);
      Vector3F v2 = v0 + d1;
      minimumClip = Vector3F.Min(minimumClip, v2);
      maximumClip = Vector3F.Max(maximumClip, v2);
      Vector3F v3 = v0 + d2;
      minimumClip = Vector3F.Min(minimumClip, v3);
      maximumClip = Vector3F.Max(maximumClip, v3);
      Vector3F v4 = v1 + d1;
      minimumClip = Vector3F.Min(minimumClip, v4);
      maximumClip = Vector3F.Max(maximumClip, v4);
      Vector3F v5 = v1 + d2;
      minimumClip = Vector3F.Min(minimumClip, v5);
      maximumClip = Vector3F.Max(maximumClip, v5);
      Vector3F v6 = v2 + d2;
      minimumClip = Vector3F.Min(minimumClip, v6);
      maximumClip = Vector3F.Max(maximumClip, v6);
      Vector3F v7 = v4 + d2;
      minimumClip = Vector3F.Min(minimumClip, v7);
      maximumClip = Vector3F.Max(maximumClip, v7);

      aabbClip.Minimum = minimumClip;
      aabbClip.Maximum = maximumClip;
    }


    [Test]
    public void GetBoundsPerspective()
    {
      // Get bounds of AABB in clip space. (Used in OcclusionCulling.fx.)
      // Note: Z = 0 or negative is handled conservatively. Point (0, 0, 0) is returned.
      Vector3F cameraPosition = new Vector3F(100, -200, 345);
      Vector3F cameraForward = new Vector3F(1, 2, 3).Normalized;
      Vector3F cameraUp = new Vector3F(-1, 0.5f, -2).Normalized;

      Matrix44F view = Matrix44F.CreateLookAt(cameraPosition, cameraPosition + cameraForward, cameraUp);
      Matrix44F proj = Matrix44F.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 16.0f / 9.0f, 1, 100);
      Matrix44F viewProj = proj * view;

      // Empty AABB at center of near plane.
      Vector3F center = cameraPosition + cameraForward;
      Vector3F halfExtent = new Vector3F();
      Aabb aabb = new Aabb(center - halfExtent, center + halfExtent);
      Aabb aabb0, aabb1;
      GetBoundsPersp(aabb, viewProj, out aabb0);
      GetBoundsPerspSmart(aabb, viewProj, out aabb1);
      Assert.IsTrue(Aabb.AreNumericallyEqual(aabb0, aabb1));

      // AABB inside frustum.
      center = view.Inverse.TransformPosition(new Vector3F(2, -3, -50));
      halfExtent = new Vector3F(1, 6, 10);
      aabb = new Aabb(center - halfExtent, center + halfExtent);
      GetBoundsPersp(aabb, viewProj, out aabb0);
      GetBoundsPerspSmart(aabb, viewProj, out aabb1);
      Assert.IsTrue(Aabb.AreNumericallyEqual(aabb0, aabb1));

      // Behind camera.
      center = view.Inverse.TransformPosition(new Vector3F(2, -3, 50));
      halfExtent = new Vector3F(1, 6, 10);
      aabb = new Aabb(center - halfExtent, center + halfExtent);
      GetBoundsPersp(aabb, viewProj, out aabb0);
      GetBoundsPerspSmart(aabb, viewProj, out aabb1);
      Assert.IsTrue(Aabb.AreNumericallyEqual(aabb0, aabb1));

      // Camera inside AABB.
      center = view.Inverse.TransformPosition(new Vector3F(2, -3, -50));
      halfExtent = new Vector3F(100, 100, 100);
      aabb = new Aabb(center - halfExtent, center + halfExtent);
      GetBoundsPersp(aabb, viewProj, out aabb0);
      GetBoundsPerspSmart(aabb, viewProj, out aabb1);
      Assert.IsTrue(Aabb.AreNumericallyEqual(aabb0, aabb1));
    }


    private void GetBoundsPersp(Aabb aabbWorld, Matrix44F viewProj, out Aabb aabbClip)
    {
      Vector3F minimum = aabbWorld.Minimum;
      Vector3F maximum = aabbWorld.Maximum;

      Vector4F v0 = (viewProj * new Vector4F(minimum.X, minimum.Y, minimum.Z, 1));
      Vector3F v;
      if (v0.Z < Numeric.EpsilonF)
        v = new Vector3F();
      else
        v = v0.XYZ / v0.W;

      Vector3F minimumClip = v;
      Vector3F maximumClip = v;

      Vector4F v1 = (viewProj * new Vector4F(maximum.X, minimum.Y, minimum.Z, 1));
      if (v1.Z < 0)
        v = new Vector3F();
      else
        v = v1.XYZ / v1.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      Vector4F v2 = (viewProj * new Vector4F(minimum.X, maximum.Y, minimum.Z, 1));
      if (v2.Z < 0)
        v = new Vector3F();
      else
        v = v2.XYZ / v2.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      Vector4F v3 = (viewProj * new Vector4F(maximum.X, maximum.Y, minimum.Z, 1));
      if (v3.Z < 0)
        v = new Vector3F();
      else
        v = v3.XYZ / v3.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      Vector4F v4 = (viewProj * new Vector4F(minimum.X, minimum.Y, maximum.Z, 1));
      if (v4.Z < 0)
        v = new Vector3F();
      else
        v = v4.XYZ / v4.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      Vector4F v5 = (viewProj * new Vector4F(maximum.X, minimum.Y, maximum.Z, 1));
      if (v5.Z < 0)
        v = new Vector3F();
      else
        v = v5.XYZ / v5.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      Vector4F v6 = (viewProj * new Vector4F(minimum.X, maximum.Y, maximum.Z, 1));
      if (v6.Z < 0)
        v = new Vector3F();
      else
        v = v6.XYZ / v6.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      Vector4F v7 = (viewProj * new Vector4F(maximum.X, maximum.Y, maximum.Z, 1));
      if (v7.Z < 0)
        v = new Vector3F();
      else
        v = v7.XYZ / v7.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      aabbClip.Minimum = minimumClip;
      aabbClip.Maximum = maximumClip;
    }


    private void GetBoundsPerspSmart(Aabb aabbWorld, Matrix44F viewProj, out Aabb aabbClip)
    {
      Vector3F minimum = aabbWorld.Minimum;
      Vector3F maximum = aabbWorld.Maximum;
      Vector3F extent = maximum - minimum;

      Vector4F v0 = viewProj * new Vector4F(minimum.X, minimum.Y, minimum.Z, 1);
      Vector4F d0 = extent.X * viewProj.GetColumn(0);
      Vector4F d1 = extent.Y * viewProj.GetColumn(1);
      Vector4F d2 = extent.Z * viewProj.GetColumn(2);

      Vector4F v1 = v0 + d0;
      Vector4F v2 = v0 + d1;
      Vector4F v3 = v0 + d2;
      Vector4F v4 = v1 + d1;
      Vector4F v5 = v1 + d2;
      Vector4F v6 = v2 + d2;
      Vector4F v7 = v4 + d2;

      Vector3F v;
      if (v0.Z < 0)
        v = new Vector3F();
      else
        v = v0.XYZ / v0.W;

      Vector3F minimumClip = v;
      Vector3F maximumClip = v;

      if (v1.Z < 0)
        v = new Vector3F();
      else
        v = v1.XYZ / v1.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      if (v2.Z < 0)
        v = new Vector3F();
      else
        v = v2.XYZ / v2.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      if (v3.Z < 0)
        v = new Vector3F();
      else
        v = v3.XYZ / v3.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      if (v4.Z < 0)
        v = new Vector3F();
      else
        v = v4.XYZ / v4.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      if (v5.Z < 0)
        v = new Vector3F();
      else
        v = v5.XYZ / v5.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      if (v6.Z < 0)
        v = new Vector3F();
      else
        v = v6.XYZ / v6.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      if (v7.Z < 0)
        v = new Vector3F();
      else
        v = v7.XYZ / v7.W;

      minimumClip = Vector3F.Min(minimumClip, v);
      maximumClip = Vector3F.Max(maximumClip, v);

      aabbClip.Minimum = minimumClip;
      aabbClip.Maximum = maximumClip;
    }
  }
}
