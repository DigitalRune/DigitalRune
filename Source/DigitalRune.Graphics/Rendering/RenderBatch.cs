// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders primitives in batches.
  /// </summary>
  /// <typeparam name="TVertex">The type of the vertex.</typeparam>
  /// <typeparam name="TIndex">The type of the index (ushort or int).</typeparam>
  /// <remarks>
  /// <para>
  /// Use the <see cref="RenderBatch{TVertex,TIndex}"/> when you need to render dynamic vertex data,
  /// e.g. for particle systems. The <see cref="RenderBatch{TVertex,TIndex}"/> must be initialized
  /// with a vertex and an index buffer. It automatically creates a suitable (dynamic) vertex and
  /// index buffer. The caller can call <see cref="Submit"/> to update vertices and indices. The
  /// method <see cref="Submit"/> return the indices where the new data can be added. If the batch
  /// is full, the <see cref="RenderBatch{TVertex,TIndex}"/> will automatically call
  /// <see cref="GraphicsDevice.DrawIndexedPrimitives"/> to draw the batch. <see cref="Flush"/> can
  /// be called to force a drawing of the current batch.
  /// </para>
  /// </remarks>
  internal sealed class RenderBatch<TVertex, TIndex> : IDisposable where TVertex : struct where TIndex : struct
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly VertexDeclaration _vertexDeclaration;
    private readonly VertexBuffer _vertexBuffer;
    private readonly bool _isVertexBufferDynamic;

    private readonly IndexElementSize _indexElementSize;
    private readonly IndexBuffer _indexBuffer;
    private readonly bool _isIndexBufferDynamic;

    // The start index of the current batch in the vertex/index buffer.
    private int _startVertex;
    private int _startIndex;

    // The next free index.
    private int _nextVertex;
    private int _nextIndex;

    // SetData options for current batch.
    private SetDataOptions _setDataOptions = SetDataOptions.NoOverwrite;

    // The primitive type of current batch.
    private PrimitiveType? _primitiveType;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this instance has been disposed of.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance has been disposed of; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsDisposed { get; private set; }


    /// <summary>
    /// Gets the vertices.
    /// </summary>
    /// <value>The vertices.</value>
    public TVertex[] Vertices { get; private set; }


    /// <summary>
    /// Gets the indices (either <strong>ushort[]</strong> or <strong>int[]</strong>).
    /// </summary>
    /// <value>The indices (either <strong>ushort[]</strong> or <strong>int[]</strong>).</value>
    public TIndex[] Indices { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderBatch{TVertex,TIndex}"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="vertexDeclaration">The vertex declaration.</param>
    /// <param name="vertices">The vertices array.</param>
    /// <param name="isVertexBufferDynamic">
    /// If set to <see langword="true"/>, the vertices can be dynamically updated. If set to 
    /// <see langword="false"/>, the vertex buffer is initialized only once (in this constructor).
    /// </param>
    /// <param name="indices">The indices array.</param>
    /// <param name="isIndexBufferDynamic">
    /// If set to <see langword="true"/>, the indices can be dynamically updated. If set to 
    /// <see langword="false"/>, the index buffer is initialized only once (in this constructor).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/>, <paramref name="vertices"/> or <paramref name="indices"/>
    /// is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="indices"/> array must be of type <strong>ushort[]</strong> or 
    /// <strong>int[]</strong>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public RenderBatch(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, TVertex[] vertices, bool isVertexBufferDynamic, TIndex[] indices, bool isIndexBufferDynamic)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (vertices == null)
        throw new ArgumentNullException("vertices");
      if (indices == null)
        throw new ArgumentNullException("indices");

      _vertexDeclaration = vertexDeclaration;
      _isVertexBufferDynamic = isVertexBufferDynamic;
      _isIndexBufferDynamic = isIndexBufferDynamic;

      Vertices = vertices;
      Indices = indices;

      if (isVertexBufferDynamic)
      {
        _vertexBuffer = new DynamicVertexBuffer(graphicsDevice, _vertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);
      }
      else
      {
        _vertexBuffer = new VertexBuffer(graphicsDevice, _vertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(vertices);
      }

      if (indices is ushort[])
      {
        _indexElementSize = IndexElementSize.SixteenBits;
      }
      else if (indices is int[])
      {
        _indexElementSize = IndexElementSize.ThirtyTwoBits;
      }
      else
      {
        throw new NotSupportedException("indices array must be of type ushort[] or int[]");
      }

      if (isIndexBufferDynamic)
      {
        _indexBuffer = new DynamicIndexBuffer(graphicsDevice, _indexElementSize, indices.Length, BufferUsage.WriteOnly);
      }
      else
      {
        _indexBuffer = new IndexBuffer(graphicsDevice, _indexElementSize, indices.Length, BufferUsage.None);

        _indexBuffer.SetData(indices);
      }
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="RenderBatch{TVertex,TIndex}"/> class.
    /// </summary>
    public void Dispose()
    {
      if (IsDisposed)
        return;

      _vertexBuffer.Dispose();
      _indexBuffer.Dispose();
      IsDisposed = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Resets the indices to the beginning of the buffers.
    /// </summary>
    /// <remarks>
    /// This method does not automatically flush the last batch.
    /// </remarks>
    public void Reset()
    {
      // Start from beginning.
      _startVertex = 0;
      _nextVertex = 0;
      _startIndex = 0;
      _nextIndex = 0;

      // Tell the graphics driver that we overwrite the old data.
      _setDataOptions = SetDataOptions.Discard;

      _primitiveType = null;
    }


    /// <summary>
    /// Informs the <see cref="RenderBatch{TVertex,TIndex}" /> that new vertices and indices will be
    /// added. (If necessary, the current batch is flushed.)
    /// </summary>
    /// <param name="primitiveType">The type of the primitive.</param>
    /// <param name="newVertices">The number of new vertices.</param>
    /// <param name="newIndices">The number new indices.</param>
    /// <param name="nextVertexBufferIndex">
    /// The start index in <see cref="Vertices"/> where the new vertices can be added.
    /// </param>
    /// <param name="nextIndexBufferIndex">
    /// The start index in <see cref="Indices"/> where the new indices can be added.
    /// </param>
    public void Submit(PrimitiveType primitiveType, int newVertices, int newIndices, 
      out int nextVertexBufferIndex, out int nextIndexBufferIndex)
    {
      var needReset = (_nextVertex + newVertices >= Vertices.Length || _nextIndex + newIndices >= Indices.Length);
      if (needReset || (_primitiveType.HasValue && primitiveType != _primitiveType.Value))
      {
        // We do not have enough buffer left or the primitive type was changed 
        // (and we cannot submit different primitives in one draw call).
        Flush();
        if (needReset)
          Reset();
      }

      _primitiveType = primitiveType;

      nextVertexBufferIndex = _nextVertex;
      nextIndexBufferIndex = _nextIndex;

      _nextVertex += newVertices;
      _nextIndex += newIndices;
    }


    /// <summary>
    /// Forces a drawing of the current batch.
    /// </summary>
    public void Flush()
    {
      int numberOfVertices = _nextVertex - _startVertex;
      int numberOfIndices = _nextIndex - _startIndex;
      if (numberOfVertices > 0 && numberOfIndices > 0)
      {
        var graphicsDevice = _vertexBuffer.GraphicsDevice;

#if XBOX   
      // Required by Xbox 360:
      if (_setDataOptions == SetDataOptions.Discard)
      {
        graphicsDevice.SetVertexBuffer(null);
        graphicsDevice.Indices = null;
      }
#endif

        // Copy vertices to vertex buffer.
        if (_isVertexBufferDynamic)
        {
          int vertexSize = _vertexDeclaration.VertexStride;
          ((DynamicVertexBuffer)_vertexBuffer).SetData(_startVertex * vertexSize, Vertices, _startVertex, numberOfVertices, vertexSize, _setDataOptions);
        }

        // Copy indices to index buffer.
        if (_isIndexBufferDynamic)
        {
          if (_indexElementSize == IndexElementSize.SixteenBits)
            ((DynamicIndexBuffer)_indexBuffer).SetData(_startIndex * 2, Indices, _startIndex, numberOfIndices, _setDataOptions);
          else
            ((DynamicIndexBuffer)_indexBuffer).SetData(_startIndex * 4, Indices, _startIndex, numberOfIndices, _setDataOptions);
        }

        int numberOfPrimitives;
        switch (_primitiveType.Value)
        {
          case PrimitiveType.LineList:
            numberOfPrimitives = numberOfIndices / 2;
            break;
          case PrimitiveType.LineStrip:
            numberOfPrimitives = numberOfIndices - 1;
            break;
          case PrimitiveType.TriangleList:
            numberOfPrimitives = numberOfIndices / 3;
            break;
          case PrimitiveType.TriangleStrip:
            numberOfPrimitives = numberOfIndices - 2;
            break;
          default:
            throw new InvalidOperationException("Unsupported type of primitives.");
        }

        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;
#if MONOGAME
        graphicsDevice.DrawIndexedPrimitives(_primitiveType.Value, 0, _startIndex, numberOfPrimitives);
#else
        graphicsDevice.DrawIndexedPrimitives(_primitiveType.Value, 0, _startVertex, numberOfVertices, _startIndex, numberOfPrimitives);
#endif

        _setDataOptions = SetDataOptions.NoOverwrite;
      }

      _startVertex = _nextVertex;
      _startIndex = _nextIndex;
      
      // Restart from beginning if remaining capacity is low.
      var vertexBufferBatchCapacity = Vertices.Length - _startVertex;
      var indexBufferBatchCapacity = Indices.Length - _startIndex;
      if (vertexBufferBatchCapacity < 128 || indexBufferBatchCapacity < 128 * 3)
        Reset();
    }
    #endregion
  }
}
