// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders instances of one mesh.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the instance buffer.</typeparam>
  /// <remarks>
  /// <para>
  /// Use the <see cref="InstanceRenderBatch{T}"/> when you need to render several instances of one
  /// submesh. The <see cref="InstanceRenderBatch{T}"/> must be initialized with a buffer which
  /// contains instance data. It automatically creates a suitable vertex. The caller can call
  /// <see cref="Submit"/> to update instances. The method <see cref="Submit"/> returns the indices
  /// where the new data can be added. If the batch is full, the 
  /// <see cref="InstanceRenderBatch{T}"/> will automatically call
  /// <see cref="GraphicsDevice.DrawInstancedPrimitives"/> to draw the batch. <see cref="Flush"/>
  /// can be called to force a drawing of the current batch.
  /// </para>
  /// </remarks>
  internal sealed class InstanceRenderBatch<T> : IDisposable where T : struct, IVertexType
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private RenderContext _renderContext;
    private Submesh _submesh;
    private EffectPassBinding _effectPassBinding;
    private EffectParameterBindingCollection _effectPassParameterBindings;

    private readonly VertexBufferBinding[] _vertexBuffers = new VertexBufferBinding[2];
    private readonly VertexDeclaration _vertexDeclaration;
    private readonly DynamicVertexBuffer _instanceVertexBuffer;

    // The start index of the current batch in the instance buffer.
    private int _startInstance;

    // The next free index.
    private int _nextInstance;

    // SetData options for current batch.
    private SetDataOptions _setDataOptions = SetDataOptions.NoOverwrite;
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
    /// Gets the instance data.
    /// </summary>
    /// <value>The instance data.</value>
    public T[] Instances { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceRenderBatch{T}"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="instances">The instance array.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> or <paramref name="instances"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public InstanceRenderBatch(GraphicsDevice graphicsDevice, T[] instances)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (instances == null)
        throw new ArgumentNullException("instances");
      if (instances.Length == 0)
        throw new ArgumentException("Parameter instances must not be an empty array.");

      _vertexDeclaration = instances[0].VertexDeclaration;
      Instances = instances;
      _instanceVertexBuffer = new DynamicVertexBuffer(graphicsDevice, _vertexDeclaration, instances.Length, BufferUsage.WriteOnly);
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="RenderBatch{TVertex,TIndex}"/> class.
    /// </summary>
    public void Dispose()
    {
      if (IsDisposed)
        return;

      _renderContext = null;
      _submesh = null;
      _effectPassBinding = new EffectPassBinding();
      _effectPassParameterBindings = null;

      _instanceVertexBuffer.Dispose();
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
      _startInstance = 0;
      _nextInstance = 0;

      // Tell the graphics driver that we overwrite the old data.
      _setDataOptions = SetDataOptions.Discard;
    }


    /// <summary>
    /// Sets the new submesh.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="submesh">The submesh.</param>
    /// <param name="effectPassBinding">The effect pass binding.</param>
    /// <param name="effectPassParameterBindings">
    /// The effect parameter bindings containing per-pass parameter bindings.
    /// (Can also contain other parameter binding, e.g. per-instance, which are ignored.
    /// </param>
    /// <remarks>
    /// The current batch is automatically flushed when the submesh is changed.
    /// </remarks>
    public void SetSubmesh(RenderContext context, Submesh submesh, EffectPassBinding effectPassBinding,
                           EffectParameterBindingCollection effectPassParameterBindings)
    {
      Flush();

      _renderContext = context;
      _submesh = submesh;
      _effectPassBinding = effectPassBinding;
      _effectPassParameterBindings = effectPassParameterBindings;
    }



    /// <summary>
    /// Informs the <see cref="InstanceRenderBatch{T}" /> that new vertices and indices will be
    /// added. (If necessary, the current batch is flushed.)
    /// </summary>
    /// <param name="newInstances">The number of new instances.</param>
    /// <param name="nextInstanceIndex">
    /// The start index in <see cref="Instances"/> where the new instance data can be added.
    /// </param>
    public void Submit(int newInstances, out int nextInstanceIndex)
    {
      bool needReset = (_nextInstance + newInstances >= Instances.Length);
      if (needReset)
      {
        // We do not have enough buffer left.
        Flush();
        Reset();
      }

      nextInstanceIndex = _nextInstance;
      _nextInstance += newInstances;
    }


    /// <summary>
    /// Forces drawing of the current batch.
    /// </summary>
    public void Flush()
    {
      var graphicsDevice = _instanceVertexBuffer.GraphicsDevice;

      int numberOfInstances = _nextInstance - _startInstance;
      if (numberOfInstances > 0)
      {
#if XBOX
        // Required by Xbox 360:
        if (_setDataOptions == SetDataOptions.Discard)
        {
          graphicsDevice.SetVertexBuffer(null);
          graphicsDevice.Indices = null;
        }
#endif

        var vertexBuffer = _submesh.VertexBuffer;
        var indexBuffer = _submesh.IndexBuffer;
        Debug.Assert(indexBuffer != null, "Hardware instancing failed: The submesh has no index buffer.");

        // Copy instance data to instance vertex buffer.
        int vertexSize = _vertexDeclaration.VertexStride;
        _instanceVertexBuffer.SetData(_startInstance * vertexSize, Instances, _startInstance, numberOfInstances,
                                      vertexSize, _setDataOptions);

        // Set vertex buffers and index buffer.
        _vertexBuffers[0] = new VertexBufferBinding(vertexBuffer, _submesh.StartVertex, 0);
        _vertexBuffers[1] = new VertexBufferBinding(_instanceVertexBuffer, _startInstance, 1);
        graphicsDevice.SetVertexBuffers(_vertexBuffers);
        graphicsDevice.Indices = indexBuffer;

        foreach (var pass in _effectPassBinding)
        {
          // Update and apply local, per-instance and per-pass bindings.
          foreach (var binding in _effectPassParameterBindings)
          {
            if (binding.Description.Hint == EffectParameterHint.PerPass)
              binding.Update(_renderContext);

            binding.Apply(_renderContext);
          }

          pass.Apply();
          graphicsDevice.DrawInstancedPrimitives(
            _submesh.PrimitiveType,
            0,
            0,
            _submesh.VertexCount,
            _submesh.StartIndex,
            _submesh.PrimitiveCount,
            numberOfInstances);
        }

        _startInstance = _nextInstance;
        _setDataOptions = SetDataOptions.NoOverwrite;
      }

      //// Restart from beginning if remaining capacity is low.
      //var vertexBufferBatchCapacity = Vertices.Length - _startVertex;
      //var indexBufferBatchCapacity = Indices.Length - _startIndex;
      //if (vertexBufferBatchCapacity < 128 || indexBufferBatchCapacity < 128 * 3)
      //  Reset();

      // Remove strong references.
      _vertexBuffers[0] = new VertexBufferBinding();

      // Reset vertex buffer to remove second vertex stream.
      graphicsDevice.SetVertexBuffer(null);
    }
    #endregion
  }
}
