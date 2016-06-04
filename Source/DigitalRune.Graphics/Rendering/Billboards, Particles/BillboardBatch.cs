// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Base implementation of <see cref="IBillboardBatch"/>.
  /// </summary>
  /// <typeparam name="T">The type of vertex.</typeparam>
  /// <inheritdoc/>
  internal abstract class BillboardBatch<T> : IBillboardBatch where T : struct, IVertexType
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// The maximum buffer size (number of billboards).
    /// </summary>
    /// <remarks>
    /// The maximum buffer size is limited because <see cref="ushort"/> values are internally used 
    /// as indices.
    /// </remarks>
    public const int MaxBufferSize = (ushort.MaxValue + 1) / 4; // = 16384 billboards
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly RenderBatch<T, ushort> _renderBatch; 
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
    /// Gets the graphics device.
    /// </summary>
    /// <value>The graphics device.</value>
    public GraphicsDevice GraphicsDevice { get; private set; }


    /// <summary>
    /// Gets the size of the buffer (number of billboards).
    /// </summary>
    /// <value>The size of the buffer (= number of billboards).</value>
    /// <remarks>
    /// The buffer size is the maximal number of billboards that can be rendered with a single draw
    /// call.
    /// </remarks>
    public int BufferSize { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BillboardBatch{T}"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="bufferSize"/> is 0, negative, or greater than <see cref="MaxBufferSize"/>.
    /// </exception>
    public BillboardBatch(GraphicsDevice graphicsDevice, int bufferSize)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (bufferSize <= 0 || bufferSize > MaxBufferSize)
        throw new ArgumentOutOfRangeException("bufferSize", "The buffer size must be in the range [1, " + MaxBufferSize + "].");

      GraphicsDevice = graphicsDevice;
      BufferSize = bufferSize;

      // Create vertex buffer.
      var vertices = new T[bufferSize * 4];

      // Create index buffer. (The content of the index buffer does not change.)
      ushort[] indices = new ushort[bufferSize * 6];
      for (int i = 0; i < bufferSize; i++)
      {
        // Create index buffer for quad (= two triangles, clockwise).
        //   1--2
        //   | /|
        //   |/ |
        //   0--3
        indices[i * 6 + 0] = (ushort)(i * 4 + 0);
        indices[i * 6 + 1] = (ushort)(i * 4 + 1);
        indices[i * 6 + 2] = (ushort)(i * 4 + 2);
        indices[i * 6 + 3] = (ushort)(i * 4 + 0);
        indices[i * 6 + 4] = (ushort)(i * 4 + 2);
        indices[i * 6 + 5] = (ushort)(i * 4 + 3);
      }

      _renderBatch = new RenderBatch<T, ushort>(
        graphicsDevice, 
        vertices[0].VertexDeclaration, 
        vertices, 
        true,
        indices,
        false);
    }
    
    
    /// <summary>
    /// Releases all resources used by an instance of the <see cref="BillboardBatch{T}"/> class.
    /// </summary>
    /// <remarks>
    /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
    /// <see langword="true"/>, and then suppresses finalization of the instance.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the 
    /// <see cref="BillboardBatchReach"/> class and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
          _renderBatch.Dispose();
        }

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public virtual void Begin(RenderContext context)
    {
    }


    /// <inheritdoc/>
    public virtual void End()
    {
      _renderBatch.Flush();
    }


    /// <inheritdoc/>
    public void DrawBillboard(ref BillboardArgs billboard, PackedTexture texture)
    {
      int index, dummy;
      _renderBatch.Submit(PrimitiveType.TriangleList, 4, 6, out index, out dummy);

      OnDrawBillboard(ref billboard, texture, _renderBatch.Vertices, index);
    }


    /// <summary>
    /// Adds the specified billboard (4 vertices) to the vertex buffer.
    /// </summary>
    /// <param name="b">The billboard.</param>
    /// <param name="vertices">The vertex buffer.</param>
    /// <param name="texture">The packed texture.</param>
    /// <param name="index">The index of the next free slot in the vertex buffer.</param>
    protected abstract void OnDrawBillboard(ref BillboardArgs b, PackedTexture texture, T[] vertices, int index);


    /// <inheritdoc/>
    public void DrawRibbon(ref RibbonArgs p0, ref RibbonArgs p1, PackedTexture texture)
    {
      int index, dummy;
      _renderBatch.Submit(PrimitiveType.TriangleList, 4, 6, out index, out dummy);

      OnDrawRibbon(ref p0, ref p1, texture, _renderBatch.Vertices, index);
    }


    /// <summary>
    /// Adds a segment of a particle ribbon (4 vertices) to the vertex buffer.
    /// </summary>
    /// <param name="p0">The segment start.</param>
    /// <param name="p1">The segment end.</param>
    /// <param name="texture">The packed texture.</param>
    /// <param name="vertices">The vertex buffer.</param>
    /// <param name="index">The index of the next free slot in the vertex buffer.</param>
    protected abstract void OnDrawRibbon(ref RibbonArgs p0, ref RibbonArgs p1, PackedTexture texture, T[] vertices, int index);
    #endregion
  }
}
