using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Graphics.Content.Tests
{
  [TestFixture]
  public class VertexBufferAccessorTest
  {
    [TestCase(DataFormat.R32G32B32A32_FLOAT, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF)]
    [TestCase(DataFormat.R32G32B32_FLOAT, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0u)]
    [TestCase(DataFormat.R32G32_FLOAT, 0xFFFFFFFF, 0xFFFFFFFF, 0u, 0u)]
    [TestCase(DataFormat.R32_FLOAT, 0xFFFFFFFF, 0u, 0u, 0u)]
    public void ReadWriteFloatTest(int format, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      const int numberOfVertices = 100;
      DataFormat vertexElementFormat = (DataFormat)format;
      int bytesPerElement = DirectXMesh.BytesPerElement(vertexElementFormat);
      Assert.Greater(bytesPerElement, 0);

      var vertexDeclaration = new[]
      {
        new VertexElement(VertexElementSemantic.Position, 0, vertexElementFormat, -1),
        new VertexElement(VertexElementSemantic.Normal, 0, vertexElementFormat, -1)
      };
      var vbAccessor = new VertexBufferAccessor(vertexDeclaration);

      var positions = new Vector4F[numberOfVertices];
      for (int i = 0; i < positions.Length; i++)
        positions[i] = new Vector4F(
          i / (float)numberOfVertices,
          (i + 10) / (float)numberOfVertices,
          (i + 20) / (float)numberOfVertices,
          (i + 30) / (float)numberOfVertices);

      var normals = new Vector4F[numberOfVertices];
      for (int i = 0; i < normals.Length; i++)
        normals[i] = new Vector4F(
          (i + 40) / (float)numberOfVertices,
          (i + 50) / (float)numberOfVertices,
          (i + 60) / (float)numberOfVertices,
          (i + 70) / (float)numberOfVertices);

      vbAccessor.SetElements(positions, VertexElementSemantic.Position, 0);
      vbAccessor.SetElements(normals, VertexElementSemantic.Normal, 0);

      byte[] vb;
      int n;
      int stride;
      vbAccessor.GetStream(0, out vb, out n, out stride);

      Assert.NotNull(vb);
      Assert.AreEqual(numberOfVertices, n);
      Assert.AreEqual(2 * bytesPerElement, stride);
      Assert.AreEqual(stride * n, vb.Length);

      vbAccessor = new VertexBufferAccessor(vertexDeclaration);
      vbAccessor.SetStream(0, vb, numberOfVertices);

      var positions1 = new Vector4F[numberOfVertices];
      var normals1 = new Vector4F[numberOfVertices];
      vbAccessor.GetElements(positions1, VertexElementSemantic.Position, 0);
      vbAccessor.GetElements(normals1, VertexElementSemantic.Normal, 0);

      for (int i = 0; i < positions.Length; i++)
      {
        Vector4F expected = AsFloat(positions[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, positions1[i].X);
        Assert.AreEqual(expected.Y, positions1[i].Y);
        Assert.AreEqual(expected.Z, positions1[i].Z);
        Assert.AreEqual(expected.W, positions1[i].W);
      }

      for (int i = 0; i < normals.Length; i++)
      {
        Vector4F expected = AsFloat(normals[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, normals1[i].X);
        Assert.AreEqual(expected.Y, normals1[i].Y);
        Assert.AreEqual(expected.Z, normals1[i].Z);
        Assert.AreEqual(expected.W, normals1[i].W);
      }
    }


    [TestCase(DataFormat.R16G16B16A16_FLOAT, 0xFFFFu, 0xFFFFu, 0xFFFFu, 0xFFFFu)]
    [TestCase(DataFormat.R16G16_FLOAT, 0xFFFFu, 0xFFFFu, 0u, 0u)]
    [TestCase(DataFormat.R16_FLOAT, 0xFFFFu, 0u, 0u, 0u)]
    public void ReadWriteHalfTest(int format, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      const int numberOfVertices = 100;
      DataFormat vertexElementFormat = (DataFormat)format;
      int bytesPerElement = DirectXMesh.BytesPerElement(vertexElementFormat);
      Assert.Greater(bytesPerElement, 0);

      var vertexDeclaration = new[]
      {
        new VertexElement(VertexElementSemantic.Position, 0, vertexElementFormat, -1),
        new VertexElement(VertexElementSemantic.Normal, 0, vertexElementFormat, -1)
      };
      var vbAccessor = new VertexBufferAccessor(vertexDeclaration);

      var positions = new Vector4F[numberOfVertices];
      for (int i = 0; i < positions.Length; i++)
        positions[i] = new Vector4F(
          i / (float)numberOfVertices,
          (i + 10) / (float)numberOfVertices,
          (i + 20) / (float)numberOfVertices,
          (i + 30) / (float)numberOfVertices);

      var normals = new Vector4F[numberOfVertices];
      for (int i = 0; i < normals.Length; i++)
        normals[i] = new Vector4F(
          (i + 40) / (float)numberOfVertices,
          (i + 50) / (float)numberOfVertices,
          (i + 60) / (float)numberOfVertices,
          (i + 70) / (float)numberOfVertices);

      vbAccessor.SetElements(positions, VertexElementSemantic.Position, 0);
      vbAccessor.SetElements(normals, VertexElementSemantic.Normal, 0);

      byte[] vb;
      int n;
      int stride;
      vbAccessor.GetStream(0, out vb, out n, out stride);

      Assert.NotNull(vb);
      Assert.AreEqual(numberOfVertices, n);
      Assert.AreEqual(2 * bytesPerElement, stride);
      Assert.AreEqual(stride * n, vb.Length);

      vbAccessor = new VertexBufferAccessor(vertexDeclaration);
      vbAccessor.SetStream(0, vb, numberOfVertices);

      var positions1 = new Vector4F[numberOfVertices];
      var normals1 = new Vector4F[numberOfVertices];
      vbAccessor.GetElements(positions1, VertexElementSemantic.Position, 0);
      vbAccessor.GetElements(normals1, VertexElementSemantic.Normal, 0);

      for (int i = 0; i < positions.Length; i++)
      {
        Vector4F expected = AsHalf(positions[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, positions1[i].X);
        Assert.AreEqual(expected.Y, positions1[i].Y);
        Assert.AreEqual(expected.Z, positions1[i].Z);
        Assert.AreEqual(expected.W, positions1[i].W);
      }

      for (int i = 0; i < normals.Length; i++)
      {
        Vector4F expected = AsHalf(normals[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, normals1[i].X);
        Assert.AreEqual(expected.Y, normals1[i].Y);
        Assert.AreEqual(expected.Z, normals1[i].Z);
        Assert.AreEqual(expected.W, normals1[i].W);
      }
    }


    [TestCase(DataFormat.R16G16B16A16_UNORM, 0xFFFFu, 0xFFFFu, 0xFFFFu, 0xFFFFu)]
    [TestCase(DataFormat.R10G10B10A2_UNORM, 0x3FFu, 0x3FFu, 0x3FFu, 0x3u)]
    [TestCase(DataFormat.R8G8B8A8_UNORM, 0xFFu, 0xFFu, 0xFFu, 0xFFu)]
    [TestCase(DataFormat.R16G16_UNORM, 0xFFFFu, 0xFFFFu, 0u, 0u)]
    [TestCase(DataFormat.B8G8R8A8_UNORM, 0xFFu, 0xFFu, 0xFFu, 0xFFu)]
    [TestCase(DataFormat.B8G8R8X8_UNORM, 0xFFu, 0xFFu, 0xFFu, 0u)]
    [TestCase(DataFormat.R8G8_UNORM, 0xFFu, 0xFFu, 0u, 0u)]
    [TestCase(DataFormat.R16_UNORM, 0xFFFFu, 0u, 0u, 0u)]
    [TestCase(DataFormat.B5G6R5_UNORM, 0x1Fu, 0x3Fu, 0x1Fu, 0u)]
    [TestCase(DataFormat.B5G5R5A1_UNORM, 0x1Fu, 0x1Fu, 0x1Fu, 0x1u)]
    [TestCase(DataFormat.R8_UNORM, 0xFFu, 0u, 0u, 0u)]
    [TestCase(DataFormat.B4G4R4A4_UNORM, 0xFu, 0xFu, 0xFu, 0xFu)]
    public void ReadWriteUNormTest(int format, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      const int numberOfVertices = 100;
      DataFormat vertexElementFormat = (DataFormat)format;
      int bytesPerElement = DirectXMesh.BytesPerElement(vertexElementFormat);
      Assert.Greater(bytesPerElement, 0);

      var vertexDeclaration = new[]
      {
        new VertexElement(VertexElementSemantic.Position, 0, vertexElementFormat, -1),
        new VertexElement(VertexElementSemantic.Normal, 0, vertexElementFormat, -1)
      };
      var vbAccessor = new VertexBufferAccessor(vertexDeclaration);

      var positions = new Vector4F[numberOfVertices];
      for (int i = 0; i < positions.Length; i++)
        positions[i] = new Vector4F(
          i / (float)numberOfVertices,
          (i + 10) / (float)numberOfVertices,
          (i + 20) / (float)numberOfVertices,
          (i + 30) / (float)numberOfVertices);

      var normals = new Vector4F[numberOfVertices];
      for (int i = 0; i < normals.Length; i++)
        normals[i] = new Vector4F(
          (i + 40) / (float)numberOfVertices,
          (i + 50) / (float)numberOfVertices,
          (i + 60) / (float)numberOfVertices,
          (i + 70) / (float)numberOfVertices);

      vbAccessor.SetElements(positions, VertexElementSemantic.Position, 0);
      vbAccessor.SetElements(normals, VertexElementSemantic.Normal, 0);

      byte[] vb;
      int n;
      int stride;
      vbAccessor.GetStream(0, out vb, out n, out stride);

      Assert.NotNull(vb);
      Assert.AreEqual(numberOfVertices, n);
      Assert.AreEqual(2 * bytesPerElement, stride);
      Assert.AreEqual(stride * n, vb.Length);

      vbAccessor = new VertexBufferAccessor(vertexDeclaration);
      vbAccessor.SetStream(0, vb, numberOfVertices);

      var positions1 = new Vector4F[numberOfVertices];
      var normals1 = new Vector4F[numberOfVertices];
      vbAccessor.GetElements(positions1, VertexElementSemantic.Position, 0);
      vbAccessor.GetElements(normals1, VertexElementSemantic.Normal, 0);

      for (int i = 0; i < positions.Length; i++)
      {
        Vector4F expected = AsUNorm(positions[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, positions1[i].X);
        Assert.AreEqual(expected.Y, positions1[i].Y);
        Assert.AreEqual(expected.Z, positions1[i].Z);
        Assert.AreEqual(expected.W, positions1[i].W);
      }

      for (int i = 0; i < normals.Length; i++)
      {
        Vector4F expected = AsUNorm(normals[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, normals1[i].X);
        Assert.AreEqual(expected.Y, normals1[i].Y);
        Assert.AreEqual(expected.Z, normals1[i].Z);
        Assert.AreEqual(expected.W, normals1[i].W);
      }
    }


    [TestCase(DataFormat.R32G32B32A32_UINT, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu)]
    [TestCase(DataFormat.R32G32B32_UINT, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0u)]
    [TestCase(DataFormat.R16G16B16A16_UINT, 0xFFFFu, 0xFFFFu, 0xFFFFu, 0xFFFFu)]
    [TestCase(DataFormat.R32G32_UINT, 0xFFFFFFFFu, 0xFFFFFFFFu, 0u, 0u)]
    [TestCase(DataFormat.R10G10B10A2_UINT, 0x3FFu, 0x3FFu, 0x3FFu, 0x3u)]
    [TestCase(DataFormat.R8G8B8A8_UINT, 0xFFu, 0xFFu, 0xFFu, 0xFFu)]
    [TestCase(DataFormat.R16G16_UINT, 0xFFFFu, 0xFFFFu, 0u, 0u)]
    [TestCase(DataFormat.R32_UINT, 0xFFFFFFFFu, 0u, 0u, 0u)]
    [TestCase(DataFormat.R8G8_UINT, 0xFFu, 0xFFu, 0u, 0u)]
    [TestCase(DataFormat.R16_UINT, 0xFFFFu, 0u, 0u, 0u)]
    [TestCase(DataFormat.R8_UINT, 0xFFu, 0u, 0u, 0u)]
    public void ReadWriteUIntTest(int format, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      const int numberOfVertices = 100;
      DataFormat vertexElementFormat = (DataFormat)format;
      int bytesPerElement = DirectXMesh.BytesPerElement(vertexElementFormat);
      Assert.Greater(bytesPerElement, 0);

      var vertexDeclaration = new[]
      {
        new VertexElement(VertexElementSemantic.Position, 0, vertexElementFormat, -1),
        new VertexElement(VertexElementSemantic.Normal, 0, vertexElementFormat, -1)
      };
      var vbAccessor = new VertexBufferAccessor(vertexDeclaration);

      var positions = new Vector4F[numberOfVertices];
      for (int i = 0; i < positions.Length; i++)
        positions[i] = new Vector4F(
          i / (float)numberOfVertices,
          (i + 1) / (float)numberOfVertices,
          (i + 2) / (float)numberOfVertices,
          (i + 3) / (float)numberOfVertices);

      var normals = new Vector4F[numberOfVertices];
      for (int i = 0; i < normals.Length; i++)
        normals[i] = new Vector4F(
          (i + 4) / (float)numberOfVertices,
          (i + 5) / (float)numberOfVertices,
          (i + 6) / (float)numberOfVertices,
          (i + 7) / (float)numberOfVertices);

      vbAccessor.SetElements(positions, VertexElementSemantic.Position, 0);
      vbAccessor.SetElements(normals, VertexElementSemantic.Normal, 0);

      byte[] vb;
      int n;
      int stride;
      vbAccessor.GetStream(0, out vb, out n, out stride);

      Assert.NotNull(vb);
      Assert.AreEqual(numberOfVertices, n);
      Assert.AreEqual(2 * bytesPerElement, stride);
      Assert.AreEqual(stride * n, vb.Length);

      vbAccessor = new VertexBufferAccessor(vertexDeclaration);
      vbAccessor.SetStream(0, vb, numberOfVertices);

      var positions1 = new Vector4F[numberOfVertices];
      var normals1 = new Vector4F[numberOfVertices];
      vbAccessor.GetElements(positions1, VertexElementSemantic.Position, 0);
      vbAccessor.GetElements(normals1, VertexElementSemantic.Normal, 0);

      for (int i = 0; i < positions.Length; i++)
      {
        Vector4F expected = AsUInt(positions[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, positions1[i].X);
        Assert.AreEqual(expected.Y, positions1[i].Y);
        Assert.AreEqual(expected.Z, positions1[i].Z);
        Assert.AreEqual(expected.W, positions1[i].W);
      }

      for (int i = 0; i < normals.Length; i++)
      {
        Vector4F expected = AsUInt(normals[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, normals1[i].X);
        Assert.AreEqual(expected.Y, normals1[i].Y);
        Assert.AreEqual(expected.Z, normals1[i].Z);
        Assert.AreEqual(expected.W, normals1[i].W);
      }
    }


    [TestCase(DataFormat.R16G16B16A16_SNORM, 0xFFFFu, 0xFFFFu, 0xFFFFu, 0xFFFFu)]
    [TestCase(DataFormat.R8G8B8A8_SNORM, 0xFFu, 0xFFu, 0xFFu, 0xFFu)]
    [TestCase(DataFormat.R16G16_SNORM, 0xFFFFu, 0xFFFFu, 0u, 0u)]
    [TestCase(DataFormat.R8G8_SNORM, 0xFFu, 0xFFu, 0u, 0u)]
    [TestCase(DataFormat.R16_SNORM, 0xFFFFu, 0u, 0u, 0u)]
    [TestCase(DataFormat.R8_SNORM, 0xFFu, 0u, 0u, 0u)]
    public void ReadWriteSNormTest(int format, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      const int numberOfVertices = 100;
      DataFormat vertexElementFormat = (DataFormat)format;
      int bytesPerElement = DirectXMesh.BytesPerElement(vertexElementFormat);
      Assert.Greater(bytesPerElement, 0);

      var vertexDeclaration = new[]
      {
        new VertexElement(VertexElementSemantic.Position, 0, vertexElementFormat, -1),
        new VertexElement(VertexElementSemantic.Normal, 0, vertexElementFormat, -1)
      };
      var vbAccessor = new VertexBufferAccessor(vertexDeclaration);

      var positions = new Vector4F[numberOfVertices];
      for (int i = 0; i < positions.Length; i++)
        positions[i] = new Vector4F(
          i / (float)numberOfVertices,
          (i + 1) / (float)numberOfVertices,
          (i + 2) / (float)numberOfVertices,
          (i + 3) / (float)numberOfVertices);

      var normals = new Vector4F[numberOfVertices];
      for (int i = 0; i < normals.Length; i++)
        normals[i] = new Vector4F(
          (i + 4) / (float)numberOfVertices,
          (i + 5) / (float)numberOfVertices,
          (i + 6) / (float)numberOfVertices,
          (i + 7) / (float)numberOfVertices);

      vbAccessor.SetElements(positions, VertexElementSemantic.Position, 0);
      vbAccessor.SetElements(normals, VertexElementSemantic.Normal, 0);

      byte[] vb;
      int n;
      int stride;
      vbAccessor.GetStream(0, out vb, out n, out stride);

      Assert.NotNull(vb);
      Assert.AreEqual(numberOfVertices, n);
      Assert.AreEqual(2 * bytesPerElement, stride);
      Assert.AreEqual(stride * n, vb.Length);

      vbAccessor = new VertexBufferAccessor(vertexDeclaration);
      vbAccessor.SetStream(0, vb, numberOfVertices);

      var positions1 = new Vector4F[numberOfVertices];
      var normals1 = new Vector4F[numberOfVertices];
      vbAccessor.GetElements(positions1, VertexElementSemantic.Position, 0);
      vbAccessor.GetElements(normals1, VertexElementSemantic.Normal, 0);

      for (int i = 0; i < positions.Length; i++)
      {
        Vector4F expected = AsSNorm(positions[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, positions1[i].X);
        Assert.AreEqual(expected.Y, positions1[i].Y);
        Assert.AreEqual(expected.Z, positions1[i].Z);
        Assert.AreEqual(expected.W, positions1[i].W);
      }

      for (int i = 0; i < normals.Length; i++)
      {
        Vector4F expected = AsSNorm(normals[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, normals1[i].X);
        Assert.AreEqual(expected.Y, normals1[i].Y);
        Assert.AreEqual(expected.Z, normals1[i].Z);
        Assert.AreEqual(expected.W, normals1[i].W);
      }
    }


    [TestCase(DataFormat.R32G32B32A32_SINT, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu)]
    [TestCase(DataFormat.R32G32B32_SINT, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0u)]
    [TestCase(DataFormat.R16G16B16A16_SINT, 0xFFFFu, 0xFFFFu, 0xFFFFu, 0xFFFFu)]
    [TestCase(DataFormat.R32G32_SINT, 0xFFFFFFFFu, 0xFFFFFFFFu, 0u, 0u)]
    [TestCase(DataFormat.R8G8B8A8_SINT, 0xFFu, 0xFFu, 0xFFu, 0xFFu)]
    [TestCase(DataFormat.R16G16_SINT, 0xFFFFu, 0xFFFFu, 0u, 0u)]
    [TestCase(DataFormat.R32_SINT, 0xFFFFFFFFu, 0u, 0u, 0u)]
    [TestCase(DataFormat.R8G8_SINT, 0xFFu, 0xFFu, 0u, 0u)]
    [TestCase(DataFormat.R16_SINT, 0xFFFFu, 0u, 0u, 0u)]
    [TestCase(DataFormat.R8_SINT, 0xFFu, 0u, 0u, 0u)]
    public void ReadWriteSIntTest(int format, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      const int numberOfVertices = 100;
      DataFormat vertexElementFormat = (DataFormat)format;
      int bytesPerElement = DirectXMesh.BytesPerElement(vertexElementFormat);
      Assert.Greater(bytesPerElement, 0);

      var vertexDeclaration = new[]
      {
        new VertexElement(VertexElementSemantic.Position, 0, vertexElementFormat, -1),
        new VertexElement(VertexElementSemantic.Normal, 0, vertexElementFormat, -1)
      };
      var vbAccessor = new VertexBufferAccessor(vertexDeclaration);

      var positions = new Vector4F[numberOfVertices];
      for (int i = 0; i < positions.Length; i++)
        positions[i] = new Vector4F(
          i / (float)numberOfVertices,
          (i + 1) / (float)numberOfVertices,
          (i + 2) / (float)numberOfVertices,
          (i + 3) / (float)numberOfVertices);

      var normals = new Vector4F[numberOfVertices];
      for (int i = 0; i < normals.Length; i++)
        normals[i] = new Vector4F(
          (i + 4) / (float)numberOfVertices,
          (i + 5) / (float)numberOfVertices,
          (i + 6) / (float)numberOfVertices,
          (i + 7) / (float)numberOfVertices);

      vbAccessor.SetElements(positions, VertexElementSemantic.Position, 0);
      vbAccessor.SetElements(normals, VertexElementSemantic.Normal, 0);

      byte[] vb;
      int n;
      int stride;
      vbAccessor.GetStream(0, out vb, out n, out stride);

      Assert.NotNull(vb);
      Assert.AreEqual(numberOfVertices, n);
      Assert.AreEqual(2 * bytesPerElement, stride);
      Assert.AreEqual(stride * n, vb.Length);

      vbAccessor = new VertexBufferAccessor(vertexDeclaration);
      vbAccessor.SetStream(0, vb, numberOfVertices);

      var positions1 = new Vector4F[numberOfVertices];
      var normals1 = new Vector4F[numberOfVertices];
      vbAccessor.GetElements(positions1, VertexElementSemantic.Position, 0);
      vbAccessor.GetElements(normals1, VertexElementSemantic.Normal, 0);

      for (int i = 0; i < positions.Length; i++)
      {
        Vector4F expected = AsSInt(positions[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, positions1[i].X);
        Assert.AreEqual(expected.Y, positions1[i].Y);
        Assert.AreEqual(expected.Z, positions1[i].Z);
        Assert.AreEqual(expected.W, positions1[i].W);
      }

      for (int i = 0; i < normals.Length; i++)
      {
        Vector4F expected = AsSInt(normals[i], redMask, greenMask, blueMask, alphaMask);
        Assert.AreEqual(expected.X, normals1[i].X);
        Assert.AreEqual(expected.Y, normals1[i].Y);
        Assert.AreEqual(expected.Z, normals1[i].Z);
        Assert.AreEqual(expected.W, normals1[i].W);
      }
    }


    private static Vector4F AsFloat(Vector4F v, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      v.X = (redMask> 0) ? v.X : 0;
      v.Y = (greenMask> 0) ? v.Y : 0;
      v.Z = (blueMask> 0) ? v.Z : 0;
      v.W = (alphaMask> 0) ? v.W : 0;
      return v;
    }


    private static Vector4F AsHalf(Vector4F v, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      v.X = (redMask> 0) ? HalfHelper.Unpack(HalfHelper.Pack(v.X)) : 0;
      v.Y = (greenMask> 0) ? HalfHelper.Unpack(HalfHelper.Pack(v.Y)) : 0;
      v.Z = (blueMask> 0) ? HalfHelper.Unpack(HalfHelper.Pack(v.Z)) : 0;
      v.W = (alphaMask> 0) ? HalfHelper.Unpack(HalfHelper.Pack(v.W)) : 0;
      return v;
    }


    private static Vector4F AsUNorm(Vector4F v, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      v.X = (redMask> 0) ? DataFormatHelper.UNormToFloat(DataFormatHelper.FloatToUNorm(v.X, redMask), redMask) : 0;
      v.Y = (greenMask> 0) ? DataFormatHelper.UNormToFloat(DataFormatHelper.FloatToUNorm(v.Y, greenMask), greenMask) : 0;
      v.Z = (blueMask> 0) ? DataFormatHelper.UNormToFloat(DataFormatHelper.FloatToUNorm(v.Z, blueMask), blueMask) : 0;
      v.W = (alphaMask> 0) ? DataFormatHelper.UNormToFloat(DataFormatHelper.FloatToUNorm(v.W, alphaMask), alphaMask) : 0;
      return v;
    }


    private static Vector4F AsUInt(Vector4F v, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      v.X = (redMask> 0) ? DataFormatHelper.UIntToFloat(DataFormatHelper.FloatToUInt(v.X, redMask), redMask) : 0;
      v.Y = (greenMask> 0) ? DataFormatHelper.UIntToFloat(DataFormatHelper.FloatToUInt(v.Y, greenMask), greenMask) : 0;
      v.Z = (blueMask> 0) ? DataFormatHelper.UIntToFloat(DataFormatHelper.FloatToUInt(v.Z, blueMask), blueMask) : 0;
      v.W = (alphaMask> 0) ? DataFormatHelper.UIntToFloat(DataFormatHelper.FloatToUInt(v.W, alphaMask), alphaMask) : 0;
      return v;
    }


    private static Vector4F AsSNorm(Vector4F v, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      v.X = (redMask> 0) ? DataFormatHelper.SNormToFloat(DataFormatHelper.FloatToSNorm(v.X, redMask), redMask) : 0;
      v.Y = (greenMask> 0) ? DataFormatHelper.SNormToFloat(DataFormatHelper.FloatToSNorm(v.Y, greenMask), greenMask) : 0;
      v.Z = (blueMask> 0) ? DataFormatHelper.SNormToFloat(DataFormatHelper.FloatToSNorm(v.Z, blueMask), blueMask) : 0;
      v.W = (alphaMask> 0) ? DataFormatHelper.SNormToFloat(DataFormatHelper.FloatToSNorm(v.W, alphaMask), alphaMask) : 0;
      return v;
    }


    private static Vector4F AsSInt(Vector4F v, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
    {
      v.X = (redMask> 0) ? DataFormatHelper.SIntToFloat(DataFormatHelper.FloatToSInt(v.X, redMask), redMask) : 0;
      v.Y = (greenMask> 0) ? DataFormatHelper.SIntToFloat(DataFormatHelper.FloatToSInt(v.Y, greenMask), greenMask) : 0;
      v.Z = (blueMask> 0) ? DataFormatHelper.SIntToFloat(DataFormatHelper.FloatToSInt(v.Z, blueMask), blueMask) : 0;
      v.W = (alphaMask> 0) ? DataFormatHelper.SIntToFloat(DataFormatHelper.FloatToSInt(v.W, alphaMask), alphaMask) : 0;
      return v;
    }
  }
}
