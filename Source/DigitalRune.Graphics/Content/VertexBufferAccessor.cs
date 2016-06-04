// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Reads or writes elements from/to vertex buffer(s) based on a vertex declaration.
  /// </summary>
  internal class VertexBufferAccessor
  {
    private readonly VertexElement[] _inputDesc;
    private readonly int[] _defaultStrides;

    private readonly byte[][] _vertexBuffers;
    private readonly int[] _strides;
    private readonly int[] _numberOfVertices;


    /// <summary>
    /// Initializes a new instance of the <see cref="VertexBufferAccessor"/> class.
    /// </summary>
    /// <param name="vertexDeclaration">The vertex declaration.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vertexDeclaration"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="vertexDeclaration"/> is invalid.
    /// </exception>
    public VertexBufferAccessor(IList<VertexElement> vertexDeclaration)
    {
      if (vertexDeclaration == null)
        throw new ArgumentNullException("vertexDeclaration");

      DirectXMesh.Validate(vertexDeclaration);

      int[] offsets;
      DirectXMesh.ComputeInputLayout(vertexDeclaration, out offsets, out _defaultStrides);

      _inputDesc = new VertexElement[vertexDeclaration.Count];

      for (int j = 0; j < vertexDeclaration.Count; j++)
      {
        //if (vertexDeclaration[ j ].InputSlotClass == D3D11_INPUT_PER_INSTANCE_DATA)
        //{
        //  // Does not currently support instance data layouts
        //  Release();
        //  throw new NotSupportedException("Vertex buffers with instance data layouts are not supported.");
        //}

        _inputDesc[j] = vertexDeclaration[j];
        _inputDesc[j].AlignedByteOffset = offsets[j];
      }

      _vertexBuffers = new byte[DirectXMesh.D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT][];
      _strides = new int[DirectXMesh.D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT];
      _numberOfVertices = new int[DirectXMesh.D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT];
    }


    private bool TryGetVertexElement(VertexElementSemantic semantic, int semanticIndex, out VertexElement vertexElement)
    {
      for (int i = 0; i < _inputDesc.Length; i++)
      {
        if (_inputDesc[i].Semantic == semantic && _inputDesc[i].SemanticIndex == semanticIndex)
        {
          vertexElement = _inputDesc[i];
          return true;
        }
      }

      vertexElement = default(VertexElement);
      return false;
    }


    /// <summary>
    /// Sets the specified vertex buffer.
    /// </summary>
    /// <param name="inputSlot">The input slot. The default slot is 0.</param>
    /// <param name="vertexBuffer">The vertex buffer.</param>
    /// <param name="numberOfVertices">The number of vertices.</param>
    /// <param name="stride">The vertex stride. Can be 0 to use the default stride.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vertexBuffer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfVertices"/> is negative.<br/>
    /// Or, <paramref name="inputSlot"/> is invalid.<br/>
    /// Or, <paramref name="stride"/> is invalid.
    /// </exception>
    public void SetStream(int inputSlot, byte[] vertexBuffer, int numberOfVertices, int stride = 0)
    {
      if (vertexBuffer == null)
        throw new ArgumentNullException("vertexBuffer");
      if (numberOfVertices < 0)
        throw new ArgumentOutOfRangeException("numberOfVertices", "The number of vertices must not be negative.");
      if (inputSlot < 0 || inputSlot >= DirectXMesh.D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT)
        throw new ArgumentOutOfRangeException("inputSlot", "Invalid input slot.");
      if (stride < 0 || stride > DirectXMesh.D3D11_REQ_MULTI_ELEMENT_STRUCTURE_SIZE_IN_BYTES)
        throw new ArgumentOutOfRangeException("stride", "Invalid vertex stride.");

      _vertexBuffers[inputSlot] = vertexBuffer;
      _strides[inputSlot] = stride > 0 ? stride : _defaultStrides[inputSlot];
      _numberOfVertices[inputSlot] = numberOfVertices;
    }


    /// <summary>
    /// Gets the specified vertex buffer. (Creates a new vertex buffer if necessary.)
    /// </summary>
    /// <param name="inputSlot">The input slot. The default slot is 0.</param>
    /// <param name="vertexBuffer">The vertex buffer.</param>
    /// <param name="numberOfVertices">The number of vertices.</param>
    /// <param name="stride">The vertex stride. Can be 0 to use the default stride.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vertexBuffer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfVertices"/> is negative.<br/>
    /// Or, <paramref name="inputSlot"/> is invalid.<br/>
    /// Or, <paramref name="stride"/> is invalid.
    /// </exception>
    public void GetStream(int inputSlot, out byte[] vertexBuffer, out int numberOfVertices, out int stride)
    {
      if (inputSlot < 0 || inputSlot >= DirectXMesh.D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT)
        throw new ArgumentOutOfRangeException("inputSlot", "Invalid input slot.");

      vertexBuffer = _vertexBuffers[inputSlot];
      if (vertexBuffer == null)
      {
        stride = _defaultStrides[inputSlot];
        numberOfVertices = 0;
      }
      else
      {
        stride = _strides[inputSlot];
        numberOfVertices = _numberOfVertices[inputSlot];
      }
    }


    /// <overloads>
    /// <summary>
    /// Writes the specified elements to the vertex buffer.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Writes the specified elements to the vertex buffer.
    /// </summary>
    /// <param name="buffer">The elements to write to the vertex buffer.</param>
    /// <param name="semantic">The semantic.</param>
    /// <param name="semanticIndex">The Index of the semantic.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="semantic"/> and <paramref name="semanticIndex"/> are invalid.<br/>
    /// Or, the number of elements in <paramref name="buffer"/> exceeds the number of vertices.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Format conversion to/from <see cref="DataFormat.R11G11B10_FLOAT"/> is not supported.
    /// </exception>
    public void SetElements(float[] buffer, VertexElementSemantic semantic, int semanticIndex)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      var tempBuffer = new Vector4F[buffer.Length];
      for (int i = 0; i < buffer.Length; i++)
        tempBuffer[i].X = buffer[i];

      SetElements(tempBuffer, semantic, semanticIndex);
    }


    /// <summary>
    /// Writes the specified elements to the vertex buffer.
    /// </summary>
    /// <param name="buffer">The elements to write to the vertex buffer.</param>
    /// <param name="semantic">The semantic.</param>
    /// <param name="semanticIndex">The Index of the semantic.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="semantic"/> and <paramref name="semanticIndex"/> are invalid.<br/>
    /// Or, the number of elements in <paramref name="buffer"/> exceeds the number of vertices.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Format conversion to/from <see cref="DataFormat.R11G11B10_FLOAT"/> is not supported.
    /// </exception>
    public void SetElements(Vector2F[] buffer, VertexElementSemantic semantic, int semanticIndex)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      var tempBuffer = new Vector4F[buffer.Length];
      for (int i = 0; i < buffer.Length; i++)
      {
        tempBuffer[i].X = buffer[i].X;
        tempBuffer[i].Y = buffer[i].Y;
      }

      SetElements(tempBuffer, semantic, semanticIndex);
    }


    /// <summary>
    /// Writes the specified elements to the vertex buffer.
    /// </summary>
    /// <param name="buffer">The elements to write to the vertex buffer.</param>
    /// <param name="semantic">The semantic.</param>
    /// <param name="semanticIndex">The Index of the semantic.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="semantic"/> and <paramref name="semanticIndex"/> are invalid.<br/>
    /// Or, the number of elements in <paramref name="buffer"/> exceeds the number of vertices.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Format conversion to/from <see cref="DataFormat.R11G11B10_FLOAT"/> is not supported.
    /// </exception>
    public void SetElements(Vector3F[] buffer, VertexElementSemantic semantic, int semanticIndex)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      var tempBuffer = new Vector4F[buffer.Length];
      for (int i = 0; i < buffer.Length; i++)
      {
        tempBuffer[i].X = buffer[i].X;
        tempBuffer[i].Y = buffer[i].Y;
        tempBuffer[i].Z = buffer[i].Z;
      }

      SetElements(tempBuffer, semantic, semanticIndex);
    }


    /// <summary>
    /// Writes the specified elements to the vertex buffer.
    /// </summary>
    /// <param name="buffer">The elements to write to the vertex buffer.</param>
    /// <param name="semantic">The semantic.</param>
    /// <param name="semanticIndex">The Index of the semantic.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="semantic"/> and <paramref name="semanticIndex"/> are invalid.<br/>
    /// Or, the number of elements in <paramref name="buffer"/> exceeds the number of vertices.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Format conversion to/from <see cref="DataFormat.R11G11B10_FLOAT"/> is not supported.
    /// </exception>
    public unsafe void SetElements(Vector4F[] buffer, VertexElementSemantic semantic, int semanticIndex)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      VertexElement vertexElement;
      if (!TryGetVertexElement(semantic, semanticIndex, out vertexElement))
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Invalid semantic {0}{1}.", semantic, semanticIndex);
        throw new ArgumentException(message);
      }

      int inputSlot = vertexElement.InputSlot;

      byte[] vertexBuffer;
      int stride;
      int numberOfVertices;
      GetStream(inputSlot, out vertexBuffer, out numberOfVertices, out stride);
      if (vertexBuffer == null)
      {
        stride = _defaultStrides[0];
        numberOfVertices = buffer.Length;
        vertexBuffer = new byte[stride * numberOfVertices];

        _vertexBuffers[inputSlot] = vertexBuffer;
        _strides[inputSlot] = stride;
        _numberOfVertices[inputSlot] = numberOfVertices;
      }

      if (buffer.Length > numberOfVertices)
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Size of buffer {0} exceeds number of vertices {1}.", buffer.Length, numberOfVertices);
        throw new ArgumentException(message);
      }

      Debug.Assert(stride > 0);

      switch (vertexElement.Format)
      {
        case DataFormat.R32G32B32A32_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 16 > eptr)  // Safety check.
                break;

              float* addr = (float*)ptr;
              addr[0] = buffer[i].X;
              addr[1] = buffer[i].Y;
              addr[2] = buffer[i].Z;
              addr[3] = buffer[i].W;

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32B32A32_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 16 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              addr[0] = DataFormatHelper.FloatToUInt(buffer[i].X, 0xFFFFFFFF);
              addr[1] = DataFormatHelper.FloatToUInt(buffer[i].Y, 0xFFFFFFFF);
              addr[2] = DataFormatHelper.FloatToUInt(buffer[i].Z, 0xFFFFFFFF);
              addr[3] = DataFormatHelper.FloatToUInt(buffer[i].W, 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32B32A32_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 16 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              addr[0] = DataFormatHelper.FloatToSInt(buffer[i].X, 0xFFFFFFFF);
              addr[1] = DataFormatHelper.FloatToSInt(buffer[i].Y, 0xFFFFFFFF);
              addr[2] = DataFormatHelper.FloatToSInt(buffer[i].Z, 0xFFFFFFFF);
              addr[3] = DataFormatHelper.FloatToSInt(buffer[i].W, 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32B32_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 12 > eptr)  // Safety check.
                break;

              float* addr = (float*)ptr;
              addr[0] = buffer[i].X;
              addr[1] = buffer[i].Y;
              addr[2] = buffer[i].Z;

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32B32_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 12 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              addr[0] = DataFormatHelper.FloatToUInt(buffer[i].X, 0xFFFFFFFF);
              addr[1] = DataFormatHelper.FloatToUInt(buffer[i].Y, 0xFFFFFFFF);
              addr[2] = DataFormatHelper.FloatToUInt(buffer[i].Z, 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32B32_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 12 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              addr[0] = DataFormatHelper.FloatToSInt(buffer[i].X, 0xFFFFFFFF);
              addr[1] = DataFormatHelper.FloatToSInt(buffer[i].Y, 0xFFFFFFFF);
              addr[2] = DataFormatHelper.FloatToSInt(buffer[i].Z, 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16B16A16_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              addr[0] = HalfHelper.Pack(buffer[i].X);
              addr[1] = HalfHelper.Pack(buffer[i].Y);
              addr[2] = HalfHelper.Pack(buffer[i].Z);
              addr[3] = HalfHelper.Pack(buffer[i].W);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16B16A16_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              addr[0] = (ushort)DataFormatHelper.FloatToUNorm(buffer[i].X, 0xFFFF);
              addr[1] = (ushort)DataFormatHelper.FloatToUNorm(buffer[i].Y, 0xFFFF);
              addr[2] = (ushort)DataFormatHelper.FloatToUNorm(buffer[i].Z, 0xFFFF);
              addr[3] = (ushort)DataFormatHelper.FloatToUNorm(buffer[i].W, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16B16A16_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              addr[0] = (ushort)DataFormatHelper.FloatToUInt(buffer[i].X, 0xFFFF);
              addr[1] = (ushort)DataFormatHelper.FloatToUInt(buffer[i].Y, 0xFFFF);
              addr[2] = (ushort)DataFormatHelper.FloatToUInt(buffer[i].Z, 0xFFFF);
              addr[3] = (ushort)DataFormatHelper.FloatToUInt(buffer[i].W, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16B16A16_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              addr[0] = (ushort)DataFormatHelper.FloatToSNorm(buffer[i].X, 0xFFFF);
              addr[1] = (ushort)DataFormatHelper.FloatToSNorm(buffer[i].Y, 0xFFFF);
              addr[2] = (ushort)DataFormatHelper.FloatToSNorm(buffer[i].Z, 0xFFFF);
              addr[3] = (ushort)DataFormatHelper.FloatToSNorm(buffer[i].W, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16B16A16_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              addr[0] = (ushort)DataFormatHelper.FloatToSInt(buffer[i].X, 0xFFFF);
              addr[1] = (ushort)DataFormatHelper.FloatToSInt(buffer[i].Y, 0xFFFF);
              addr[2] = (ushort)DataFormatHelper.FloatToSInt(buffer[i].Z, 0xFFFF);
              addr[3] = (ushort)DataFormatHelper.FloatToSInt(buffer[i].W, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              float* addr = (float*)ptr;
              addr[0] = buffer[i].X;
              addr[1] = buffer[i].Y;

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              addr[0] = DataFormatHelper.FloatToUInt(buffer[i].X, 0xFFFFFFFF);
              addr[1] = DataFormatHelper.FloatToUInt(buffer[i].Y, 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              addr[0] = DataFormatHelper.FloatToSInt(buffer[i].X, 0xFFFFFFFF);
              addr[1] = DataFormatHelper.FloatToSInt(buffer[i].Y, 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R10G10B10A2_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              uint x = DataFormatHelper.FloatToUNorm(buffer[i].X, 0x3FF);
              uint y = DataFormatHelper.FloatToUNorm(buffer[i].Y, 0x3FF) << 10;
              uint z = DataFormatHelper.FloatToUNorm(buffer[i].Z, 0x3FF) << 20;
              uint w = DataFormatHelper.FloatToUNorm(buffer[i].W, 0x3) << 30;
              *(uint*)ptr = x | y | z | w;

              ptr += stride;
            }
          }
          break;
        case DataFormat.R10G10B10A2_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              uint x = DataFormatHelper.FloatToUInt(buffer[i].X, 0x3FF);
              uint y = DataFormatHelper.FloatToUInt(buffer[i].Y, 0x3FF) << 10;
              uint z = DataFormatHelper.FloatToUInt(buffer[i].Z, 0x3FF) << 20;
              uint w = DataFormatHelper.FloatToUInt(buffer[i].W, 0x3) << 30;
              *(uint*)ptr = x | y | z | w;

              ptr += stride;
            }
          }
          break;
        case DataFormat.R11G11B10_FLOAT:
          throw new NotSupportedException("Format conversion to/from R11G11B10_Float is not supported.");
        case DataFormat.R8G8B8A8_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ptr[0] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].X, 0xFF);
              ptr[1] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].Y, 0xFF);
              ptr[2] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].Z, 0xFF);
              ptr[3] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].W, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8B8A8_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ptr[0] = (byte)DataFormatHelper.FloatToUInt(buffer[i].X, 0xFF);
              ptr[1] = (byte)DataFormatHelper.FloatToUInt(buffer[i].Y, 0xFF);
              ptr[2] = (byte)DataFormatHelper.FloatToUInt(buffer[i].Z, 0xFF);
              ptr[3] = (byte)DataFormatHelper.FloatToUInt(buffer[i].W, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8B8A8_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ptr[0] = (byte)DataFormatHelper.FloatToSNorm(buffer[i].X, 0xFF);
              ptr[1] = (byte)DataFormatHelper.FloatToSNorm(buffer[i].Y, 0xFF);
              ptr[2] = (byte)DataFormatHelper.FloatToSNorm(buffer[i].Z, 0xFF);
              ptr[3] = (byte)DataFormatHelper.FloatToSNorm(buffer[i].W, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8B8A8_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ptr[0] = (byte)DataFormatHelper.FloatToSInt(buffer[i].X, 0xFF);
              ptr[1] = (byte)DataFormatHelper.FloatToSInt(buffer[i].Y, 0xFF);
              ptr[2] = (byte)DataFormatHelper.FloatToSInt(buffer[i].Z, 0xFF);
              ptr[3] = (byte)DataFormatHelper.FloatToSInt(buffer[i].W, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              addr[0] = HalfHelper.Pack(buffer[i].X);
              addr[1] = HalfHelper.Pack(buffer[i].Y);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              addr[0] = (ushort)DataFormatHelper.FloatToUNorm(buffer[i].X, 0xFFFF);
              addr[1] = (ushort)DataFormatHelper.FloatToUNorm(buffer[i].Y, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              addr[0] = (ushort)DataFormatHelper.FloatToUInt(buffer[i].X, 0xFFFF);
              addr[1] = (ushort)DataFormatHelper.FloatToUInt(buffer[i].Y, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              addr[0] = (ushort)DataFormatHelper.FloatToSNorm(buffer[i].X, 0xFFFF);
              addr[1] = (ushort)DataFormatHelper.FloatToSNorm(buffer[i].Y, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              addr[0] = (ushort)DataFormatHelper.FloatToSInt(buffer[i].X, 0xFFFF);
              addr[1] = (ushort)DataFormatHelper.FloatToSInt(buffer[i].Y, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              *(float*)ptr = buffer[i].X;

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              *(uint*)ptr = DataFormatHelper.FloatToUInt(buffer[i].X, 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              *(uint*)ptr = DataFormatHelper.FloatToSInt(buffer[i].X, 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.B8G8R8A8_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ptr[0] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].Z, 0xFF);
              ptr[1] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].Y, 0xFF);
              ptr[2] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].X, 0xFF);
              ptr[3] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].W, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.B8G8R8X8_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ptr[0] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].Z, 0xFF);
              ptr[1] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].Y, 0xFF);
              ptr[2] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].X, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              ptr[0] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].X, 0xFF);
              ptr[1] = (byte)DataFormatHelper.FloatToUNorm(buffer[i].Y, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              ptr[0] = (byte)DataFormatHelper.FloatToUInt(buffer[i].X, 0xFF);
              ptr[1] = (byte)DataFormatHelper.FloatToUInt(buffer[i].Y, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              ptr[0] = (byte)DataFormatHelper.FloatToSNorm(buffer[i].X, 0xFF);
              ptr[1] = (byte)DataFormatHelper.FloatToSNorm(buffer[i].Y, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              ptr[0] = (byte)DataFormatHelper.FloatToSInt(buffer[i].X, 0xFF);
              ptr[1] = (byte)DataFormatHelper.FloatToSInt(buffer[i].Y, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              *(ushort*)ptr = HalfHelper.Pack(buffer[i].X);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              *(ushort*)ptr = (ushort)DataFormatHelper.FloatToUNorm(buffer[i].X, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              *(ushort*)ptr = (ushort)DataFormatHelper.FloatToUInt(buffer[i].X, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              *(ushort*)ptr = (ushort)DataFormatHelper.FloatToSNorm(buffer[i].X, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              *(ushort*)ptr = (ushort)DataFormatHelper.FloatToSInt(buffer[i].X, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.B5G6R5_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              uint x = DataFormatHelper.FloatToUNorm(buffer[i].X, 0x1F) << 11;
              uint y = DataFormatHelper.FloatToUNorm(buffer[i].Y, 0x3F) << 5;
              uint z = DataFormatHelper.FloatToUNorm(buffer[i].Z, 0x1F);

              *(ushort*)ptr = (ushort)(x | y | z);

              ptr += stride;
            }
          }
          break;
        case DataFormat.B5G5R5A1_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              uint x = DataFormatHelper.FloatToUNorm(buffer[i].X, 0x1F) << 10;
              uint y = DataFormatHelper.FloatToUNorm(buffer[i].Y, 0x1F) << 5;
              uint z = DataFormatHelper.FloatToUNorm(buffer[i].Z, 0x1F);
              uint w = DataFormatHelper.FloatToUNorm(buffer[i].W, 0x1) << 15;

              *(ushort*)ptr = (ushort)(x | y | z | w);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 1 > eptr)  // Safety check.
                break;

              *ptr = (byte)DataFormatHelper.FloatToUNorm(buffer[i].X, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 1 > eptr)  // Safety check.
                break;

              *ptr = (byte)DataFormatHelper.FloatToUInt(buffer[i].X, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 1 > eptr)  // Safety check.
                break;

              *ptr = (byte)DataFormatHelper.FloatToSNorm(buffer[i].X, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 1 > eptr)  // Safety check.
                break;

              *ptr = (byte)DataFormatHelper.FloatToSInt(buffer[i].X, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.B4G4R4A4_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              uint x = DataFormatHelper.FloatToUNorm(buffer[i].X, 0xF) << 8;
              uint y = DataFormatHelper.FloatToUNorm(buffer[i].Y, 0xF) << 4;
              uint z = DataFormatHelper.FloatToUNorm(buffer[i].Z, 0xF);
              uint w = DataFormatHelper.FloatToUNorm(buffer[i].W, 0xF) << 12;

              *(ushort*)ptr = (ushort)(x | y | z | w);

              ptr += stride;
            }
          }
          break;
      }
    }


    /// <overloads>
    /// <summary>
    /// Reads the specified elements from the vertex buffer.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Reads the specified elements to the vertex buffer.
    /// </summary>
    /// <param name="buffer">The elements extracted from the vertex buffer.</param>
    /// <param name="semantic">The semantic.</param>
    /// <param name="semanticIndex">The Index of the semantic.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="semantic"/> and <paramref name="semanticIndex"/> are invalid.<br/>
    /// Or, the number of elements in <paramref name="buffer"/> exceeds the number of vertices.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Format conversion to/from <see cref="DataFormat.R11G11B10_FLOAT"/> is not supported.
    /// </exception>
    public void GetElements(float[] buffer, VertexElementSemantic semantic, int semanticIndex)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      var tempBuffer = new Vector4F[buffer.Length];

      GetElements(tempBuffer, semantic, semanticIndex);

      for (int i = 0; i < buffer.Length; i++)
        buffer[i] = tempBuffer[i].X;
    }


    /// <summary>
    /// Reads the specified elements to the vertex buffer.
    /// </summary>
    /// <param name="buffer">The elements extracted from the vertex buffer.</param>
    /// <param name="semantic">The semantic.</param>
    /// <param name="semanticIndex">The Index of the semantic.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="semantic"/> and <paramref name="semanticIndex"/> are invalid.<br/>
    /// Or, the number of elements in <paramref name="buffer"/> exceeds the number of vertices.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Format conversion to/from <see cref="DataFormat.R11G11B10_FLOAT"/> is not supported.
    /// </exception>
    public void GetElements(Vector2F[] buffer, VertexElementSemantic semantic, int semanticIndex)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      var tempBuffer = new Vector4F[buffer.Length];

      GetElements(tempBuffer, semantic, semanticIndex);

      for (int i = 0; i < buffer.Length; i++)
      {
        buffer[i].X = tempBuffer[i].X;
        buffer[i].Y = tempBuffer[i].Y;
      }
    }


    /// <summary>
    /// Reads the specified elements to the vertex buffer.
    /// </summary>
    /// <param name="buffer">The elements extracted from the vertex buffer.</param>
    /// <param name="semantic">The semantic.</param>
    /// <param name="semanticIndex">The Index of the semantic.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="semantic"/> and <paramref name="semanticIndex"/> are invalid.<br/>
    /// Or, the number of elements in <paramref name="buffer"/> exceeds the number of vertices.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Format conversion to/from <see cref="DataFormat.R11G11B10_FLOAT"/> is not supported.
    /// </exception>
    public void GetElements(Vector3F[] buffer, VertexElementSemantic semantic, int semanticIndex)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      var tempBuffer = new Vector4F[buffer.Length];

      GetElements(tempBuffer, semantic, semanticIndex);

      for (int i = 0; i < buffer.Length; i++)
      {
        buffer[i].X = tempBuffer[i].X;
        buffer[i].Y = tempBuffer[i].Y;
        buffer[i].Z = tempBuffer[i].Z;
      }
    }


    /// <summary>
    /// Reads the specified elements to the vertex buffer.
    /// </summary>
    /// <param name="buffer">The elements extracted from the vertex buffer.</param>
    /// <param name="semantic">The semantic.</param>
    /// <param name="semanticIndex">The Index of the semantic.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="semantic"/> and <paramref name="semanticIndex"/> are invalid.<br/>
    /// Or, the number of elements in <paramref name="buffer"/> exceeds the number of vertices.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Format conversion to/from <see cref="DataFormat.R11G11B10_FLOAT"/> is not supported.
    /// </exception>
    public unsafe void GetElements(Vector4F[] buffer, VertexElementSemantic semantic, int semanticIndex)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      VertexElement vertexElement;
      if (!TryGetVertexElement(semantic, semanticIndex, out vertexElement))
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Invalid semantic {0}{1}.", semantic, semanticIndex);
        throw new ArgumentException(message);
      }

      int inputSlot = vertexElement.InputSlot;

      byte[] vertexBuffer;
      int stride;
      int numberOfVertices;
      GetStream(inputSlot, out vertexBuffer, out numberOfVertices, out stride);
      if (vertexBuffer == null)
      {
        stride = _defaultStrides[0];
        numberOfVertices = buffer.Length;
        vertexBuffer = new byte[stride * numberOfVertices];

        _vertexBuffers[inputSlot] = vertexBuffer;
        _strides[inputSlot] = stride;
        _numberOfVertices[inputSlot] = numberOfVertices;
      }

      if (buffer.Length > numberOfVertices)
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Size of buffer {0} exceeds number of vertices {1}.", buffer.Length, numberOfVertices);
        throw new ArgumentException(message);
      }

      Debug.Assert(stride > 0);

      switch (vertexElement.Format)
      {
        case DataFormat.R32G32B32A32_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 16 > eptr)  // Safety check.
                break;

              float* addr = (float*)ptr;
              buffer[i].X = addr[0];
              buffer[i].Y = addr[1];
              buffer[i].Z = addr[2];
              buffer[i].W = addr[3];

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32B32A32_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 16 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              buffer[i].X = DataFormatHelper.UIntToFloat(addr[0], 0xFFFFFFFF);
              buffer[i].Y = DataFormatHelper.UIntToFloat(addr[1], 0xFFFFFFFF);
              buffer[i].Z = DataFormatHelper.UIntToFloat(addr[2], 0xFFFFFFFF);
              buffer[i].W = DataFormatHelper.UIntToFloat(addr[3], 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32B32A32_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 16 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              buffer[i].X = DataFormatHelper.SIntToFloat(addr[0], 0xFFFFFFFF);
              buffer[i].Y = DataFormatHelper.SIntToFloat(addr[1], 0xFFFFFFFF);
              buffer[i].Z = DataFormatHelper.SIntToFloat(addr[2], 0xFFFFFFFF);
              buffer[i].W = DataFormatHelper.SIntToFloat(addr[3], 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32B32_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 12 > eptr)  // Safety check.
                break;

              float* addr = (float*)ptr;
              buffer[i].X = addr[0];
              buffer[i].Y = addr[1];
              buffer[i].Z = addr[2];

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32B32_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 12 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              buffer[i].X = DataFormatHelper.UIntToFloat(addr[0], 0xFFFFFFFF);
              buffer[i].Y = DataFormatHelper.UIntToFloat(addr[1], 0xFFFFFFFF);
              buffer[i].Z = DataFormatHelper.UIntToFloat(addr[2], 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32B32_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 12 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              buffer[i].X = DataFormatHelper.SIntToFloat(addr[0], 0xFFFFFFFF);
              buffer[i].Y = DataFormatHelper.SIntToFloat(addr[1], 0xFFFFFFFF);
              buffer[i].Z = DataFormatHelper.SIntToFloat(addr[2], 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16B16A16_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              buffer[i].X = HalfHelper.Unpack(addr[0]);
              buffer[i].Y = HalfHelper.Unpack(addr[1]);
              buffer[i].Z = HalfHelper.Unpack(addr[2]);
              buffer[i].W = HalfHelper.Unpack(addr[3]);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16B16A16_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              buffer[i].X = DataFormatHelper.UNormToFloat(addr[0], 0xFFFF);
              buffer[i].Y = DataFormatHelper.UNormToFloat(addr[1], 0xFFFF);
              buffer[i].Z = DataFormatHelper.UNormToFloat(addr[2], 0xFFFF);
              buffer[i].W = DataFormatHelper.UNormToFloat(addr[3], 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16B16A16_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              buffer[i].X = DataFormatHelper.UIntToFloat(addr[0], 0xFFFF);
              buffer[i].Y = DataFormatHelper.UIntToFloat(addr[1], 0xFFFF);
              buffer[i].Z = DataFormatHelper.UIntToFloat(addr[2], 0xFFFF);
              buffer[i].W = DataFormatHelper.UIntToFloat(addr[3], 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16B16A16_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              buffer[i].X = DataFormatHelper.SNormToFloat(addr[0], 0xFFFF);
              buffer[i].Y = DataFormatHelper.SNormToFloat(addr[1], 0xFFFF);
              buffer[i].Z = DataFormatHelper.SNormToFloat(addr[2], 0xFFFF);
              buffer[i].W = DataFormatHelper.SNormToFloat(addr[3], 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16B16A16_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              buffer[i].X = DataFormatHelper.SIntToFloat(addr[0], 0xFFFF);
              buffer[i].Y = DataFormatHelper.SIntToFloat(addr[1], 0xFFFF);
              buffer[i].Z = DataFormatHelper.SIntToFloat(addr[2], 0xFFFF);
              buffer[i].W = DataFormatHelper.SIntToFloat(addr[3], 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              float* addr = (float*)ptr;
              buffer[i].X = addr[0];
              buffer[i].Y = addr[1];

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              buffer[i].X = DataFormatHelper.UIntToFloat(addr[0], 0xFFFFFFFF);
              buffer[i].Y = DataFormatHelper.UIntToFloat(addr[1], 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32G32_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 8 > eptr)  // Safety check.
                break;

              uint* addr = (uint*)ptr;
              buffer[i].X = DataFormatHelper.SIntToFloat(addr[0], 0xFFFFFFFF);
              buffer[i].Y = DataFormatHelper.SIntToFloat(addr[1], 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R10G10B10A2_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              uint v = *(uint*)ptr;
              buffer[i].X = DataFormatHelper.UNormToFloat(v, 0x3FF);
              buffer[i].Y = DataFormatHelper.UNormToFloat(v >> 10, 0x3FF);
              buffer[i].Z = DataFormatHelper.UNormToFloat(v >> 20, 0x3FF);
              buffer[i].W = DataFormatHelper.UNormToFloat(v >> 30, 0x3);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R10G10B10A2_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              uint v = *(uint*)ptr;
              buffer[i].X = DataFormatHelper.UIntToFloat(v, 0x3FF);
              buffer[i].Y = DataFormatHelper.UIntToFloat(v >> 10, 0x3FF);
              buffer[i].Z = DataFormatHelper.UIntToFloat(v >> 20, 0x3FF);
              buffer[i].W = DataFormatHelper.UIntToFloat(v >> 30, 0x3);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R11G11B10_FLOAT:
          throw new NotSupportedException("Format conversion to/from R11G11B10_Float is not supported.");
        case DataFormat.R8G8B8A8_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.UNormToFloat(ptr[0], 0xFF);
              buffer[i].Y = DataFormatHelper.UNormToFloat(ptr[1], 0xFF);
              buffer[i].Z = DataFormatHelper.UNormToFloat(ptr[2], 0xFF);
              buffer[i].W = DataFormatHelper.UNormToFloat(ptr[3], 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8B8A8_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.UIntToFloat(ptr[0], 0xFF);
              buffer[i].Y = DataFormatHelper.UIntToFloat(ptr[1], 0xFF);
              buffer[i].Z = DataFormatHelper.UIntToFloat(ptr[2], 0xFF);
              buffer[i].W = DataFormatHelper.UIntToFloat(ptr[3], 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8B8A8_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.SNormToFloat(ptr[0], 0xFF);
              buffer[i].Y = DataFormatHelper.SNormToFloat(ptr[1], 0xFF);
              buffer[i].Z = DataFormatHelper.SNormToFloat(ptr[2], 0xFF);
              buffer[i].W = DataFormatHelper.SNormToFloat(ptr[3], 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8B8A8_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.SIntToFloat(ptr[0], 0xFF);
              buffer[i].Y = DataFormatHelper.SIntToFloat(ptr[1], 0xFF);
              buffer[i].Z = DataFormatHelper.SIntToFloat(ptr[2], 0xFF);
              buffer[i].W = DataFormatHelper.SIntToFloat(ptr[3], 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              buffer[i].X = HalfHelper.Unpack(addr[0]);
              buffer[i].Y = HalfHelper.Unpack(addr[1]);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              buffer[i].X = DataFormatHelper.UNormToFloat(addr[0], 0xFFFF);
              buffer[i].Y = DataFormatHelper.UNormToFloat(addr[1], 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              buffer[i].X = DataFormatHelper.UIntToFloat(addr[0], 0xFFFF);
              buffer[i].Y = DataFormatHelper.UIntToFloat(addr[1], 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              buffer[i].X = DataFormatHelper.SNormToFloat(addr[0], 0xFFFF);
              buffer[i].Y = DataFormatHelper.SNormToFloat(addr[1], 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16G16_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              ushort* addr = (ushort*)ptr;
              buffer[i].X = DataFormatHelper.SIntToFloat(addr[0], 0xFFFF);
              buffer[i].Y = DataFormatHelper.SIntToFloat(addr[1], 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              buffer[i].X = *(float*)ptr;

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.UIntToFloat(*(uint*)ptr, 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R32_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.SIntToFloat(*(uint*)ptr, 0xFFFFFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.B8G8R8A8_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              buffer[i].Z = DataFormatHelper.UNormToFloat(ptr[0], 0xFF);
              buffer[i].Y = DataFormatHelper.UNormToFloat(ptr[1], 0xFF);
              buffer[i].X = DataFormatHelper.UNormToFloat(ptr[2], 0xFF);
              buffer[i].W = DataFormatHelper.UNormToFloat(ptr[3], 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.B8G8R8X8_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 4 > eptr)  // Safety check.
                break;

              buffer[i].Z = DataFormatHelper.UNormToFloat(ptr[0], 0xFF);
              buffer[i].Y = DataFormatHelper.UNormToFloat(ptr[1], 0xFF);
              buffer[i].X = DataFormatHelper.UNormToFloat(ptr[2], 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.UNormToFloat(ptr[0], 0xFF);
              buffer[i].Y = DataFormatHelper.UNormToFloat(ptr[1], 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.UIntToFloat(ptr[0], 0xFF);
              buffer[i].Y = DataFormatHelper.UIntToFloat(ptr[1], 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.SNormToFloat(ptr[0], 0xFF);
              buffer[i].Y = DataFormatHelper.SNormToFloat(ptr[1], 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8G8_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.SIntToFloat(ptr[0], 0xFF);
              buffer[i].Y = DataFormatHelper.SIntToFloat(ptr[1], 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16_FLOAT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              buffer[i].X = HalfHelper.Unpack(*(ushort*)ptr);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.UNormToFloat(*(ushort*)ptr, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.UIntToFloat(*(ushort*)ptr, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.SNormToFloat(*(ushort*)ptr, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R16_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.SIntToFloat(*(ushort*)ptr, 0xFFFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.B5G6R5_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              uint v = *(ushort*)ptr;

              buffer[i].X = DataFormatHelper.UNormToFloat(v >> 11, 0x1F);
              buffer[i].Y = DataFormatHelper.UNormToFloat(v >> 5, 0x3F);
              buffer[i].Z = DataFormatHelper.UNormToFloat(v, 0x1F);

              ptr += stride;
            }
          }
          break;
        case DataFormat.B5G5R5A1_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              uint v = *(ushort*)ptr;

              buffer[i].X = DataFormatHelper.UNormToFloat(v >> 10, 0x1F);
              buffer[i].Y = DataFormatHelper.UNormToFloat(v >> 5, 0x1F);
              buffer[i].Z = DataFormatHelper.UNormToFloat(v, 0x1F);
              buffer[i].W = DataFormatHelper.UNormToFloat(v >> 15, 0x1);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 1 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.UNormToFloat(ptr[0], 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8_UINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 1 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.UIntToFloat(*ptr, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8_SNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 1 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.SNormToFloat(*ptr, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.R8_SINT:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 1 > eptr)  // Safety check.
                break;

              buffer[i].X = DataFormatHelper.SIntToFloat(*ptr, 0xFF);

              ptr += stride;
            }
          }
          break;
        case DataFormat.B4G4R4A4_UNORM:
          fixed (byte* vbPtr = vertexBuffer)
          {
            byte* eptr = vbPtr + stride * numberOfVertices;
            byte* ptr = vbPtr + vertexElement.AlignedByteOffset;
            for (int i = 0; i < buffer.Length; i++)
            {
              if (ptr + 2 > eptr)  // Safety check.
                break;

              uint v = *(ushort*)ptr;

              buffer[i].X = DataFormatHelper.UNormToFloat(v >> 8, 0xF);
              buffer[i].Y = DataFormatHelper.UNormToFloat(v >> 4, 0xF);
              buffer[i].Z = DataFormatHelper.UNormToFloat(v, 0xF);
              buffer[i].W = DataFormatHelper.UNormToFloat(v >> 12, 0xF);

              ptr += stride;
            }
          }
          break;
      }
    }
  }
}
