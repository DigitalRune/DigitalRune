// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Manages a pool of reusable render targets.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class manages a list of render targets. To get a render target call the
  /// <strong>Obtain2D</strong> or <strong>ObtainCube</strong> method. After the render target is
  /// not needed any more call <strong>Recycle</strong>. Once per frame, <see cref="Update"/> must
  /// be called. This method updates internal render target usage data and removes render targets
  /// which have not been used for a while (see property <see cref="FrameLimit"/>).
  /// <see cref="Clear"/> should be called when the graphics settings of the game have changed, e.g.
  /// when the size of the back buffer is changed, or at certain events, e.g. when a new level is
  /// loaded.
  /// </para>
  /// <para>
  /// <strong>Thread-Safety:</strong> This class is <strong>not</strong> thread-safe.
  /// </para>
  /// </remarks>
  public class RenderTargetPool : IDisposable
  {
    // Notes:
    // For debugging it is helpful to be able to break in the debugger when a
    // new render target is created. We can set a break point in this code.
    // Users without a source code license cannot do this. For them it would be
    // helpful to have a RenderTargetPool.Created event, or (more flexible) to
    // have virtual methods RenderTargetPool.OnCreateRenderTarget() methods
    // which they can override.
    //
    // Dispose() has the same effect as Clear(). The RenderTargetPool stays usable.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Fields are internal for unit testing.
    internal readonly List<RenderTarget2D> RenderTargets2D = new List<RenderTarget2D>();
    internal readonly List<RenderTargetCube> RenderTargetsCube = new List<RenderTargetCube>();
    internal readonly List<int> Counters2D = new List<int>();
    internal readonly List<int> CountersCube = new List<int>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the graphics service.
    /// </summary>
    /// <value>The graphics service.</value>
    public IGraphicsService GraphicsService { get; private set; }


    /// <summary>
    /// Gets or sets the number of frames a recycled render target will be cached before it is 
    /// released.
    /// </summary>
    /// <value>
    /// The number of frames a recycled render target will be cached before it is released. The 
    /// default value is 10.
    /// </value>
    /// <remarks>
    /// When a render target is recycled (see <see cref="Recycle(RenderTarget2D)"/>) and re-added to
    /// the pool of render targets, a frame counter for this render target is set to 0. Each frame 
    /// the counter of all render targets in the pool is incremented. If a counter gets equal to or 
    /// larger than <see cref="FrameLimit"/>, the render target is disposed. This mechanism is used 
    /// to avoid that render targets, which are not needed anymore, are kept alive.
    /// </remarks>
    public int FrameLimit { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether render target pooling is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if render target pooling is enabled; otherwise,
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// Setting <see cref="Enabled"/> to <see langword="false"/> clears the render target pool and
    /// disables pooling. The <strong>Obtain</strong>/<strong>Recycle</strong> methods can be called
    /// normally. But internally no render targets are reused.
    /// </remarks>
    public bool Enabled
    {
      get { return _enabled; }
      set
      {
        _enabled = value;
        if (!value)
          Clear();
      }
    }
    private bool _enabled;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderTargetPool"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public RenderTargetPool(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      GraphicsService = graphicsService;
      FrameLimit = 10;
      Enabled = true;
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="RenderTargetPool"/> class.
    /// </summary>
    /// <remarks>
    /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in
    /// <see langword="true"/>, and then suppresses finalization of the instance.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "RenderTargetPool is reusable.")]
    public void Dispose()
    {
      Dispose(true);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="RenderTargetPool"/>
    /// class
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        Clear();
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes all cached render targets.
    /// </summary>
    public void Clear()
    {
#if XNA
      // Warning: If we dispose a render target, and this render target is still
      // set in an effect parameter, then XNA might dispose the GraphicsDevice when the
      // device is reset and a garbage collection is performed. :-(
      foreach (var item in RenderTargets2D)
        if (item != null)
          item.Dispose();
#endif
      RenderTargets2D.Clear();

#if XNA
      foreach (var item in RenderTargetsCube)
        if (item != null)
          item.Dispose();
#endif
      RenderTargetsCube.Clear();

      Counters2D.Clear();
      CountersCube.Clear();
    }


    /// <summary>
    /// Obtains a 2D render target that matches the given specification.
    /// </summary>
    /// <param name="format">
    /// The render target format. If a property in the format is <see langword="null"/>, then the 
    /// value of the back buffer is used. 
    /// </param>
    /// <returns>A render target with the given specified format.</returns>
    /// <remarks>
    /// This method returns a render target from the pool. If no suitable, free render target is 
    /// found, a new one is created.
    /// </remarks>
    public RenderTarget2D Obtain2D(RenderTargetFormat format)
    {
      RenderTarget2D renderTarget = null;

      var pp = GraphicsService.GraphicsDevice.PresentationParameters;
      int width = format.Width ?? pp.BackBufferWidth;
      int height = format.Height ?? pp.BackBufferHeight;
      bool mipmap = format.Mipmap ?? false;
      var surfaceFormat = format.SurfaceFormat ?? pp.BackBufferFormat;
      var depthStencilFormat = format.DepthStencilFormat ?? pp.DepthStencilFormat;
      int multiSampleCount = format.MultiSampleCount ?? pp.MultiSampleCount;
      var renderTargetUsage = format.RenderTargetUsage ?? RenderTargetUsage.DiscardContents;

      if (Enabled)
      {
        int numberOfItems = RenderTargets2D.Count;
        int index;
        for (index = numberOfItems - 1; index >= 0; index--)
        {
          var candidate = RenderTargets2D[index];
          if (candidate.Width == width
              && candidate.Height == height
              && (candidate.LevelCount > 1) == mipmap
              && candidate.Format == surfaceFormat
              && candidate.DepthStencilFormat == depthStencilFormat
              && candidate.MultiSampleCount == multiSampleCount
              && candidate.RenderTargetUsage == renderTargetUsage)
          {
            renderTarget = candidate;
            break;
          }
        }

        if (renderTarget != null)
        {
          numberOfItems--;
          if (index < numberOfItems)
          {
            RenderTargets2D[index] = RenderTargets2D[numberOfItems];
            Counters2D[index] = Counters2D[numberOfItems];
          }

          RenderTargets2D.RemoveAt(numberOfItems);
          Counters2D.RemoveAt(numberOfItems);

          return renderTarget;
        }
      }

      renderTarget = new RenderTarget2D(GraphicsService.GraphicsDevice, width, height, mipmap, 
        surfaceFormat, depthStencilFormat, multiSampleCount, renderTargetUsage);

      return renderTarget;
    }


    /// <summary>
    /// Obtains a cube map render target that matches the given specification.
    /// </summary>
    /// <param name="format">
    /// The render target format. If a property in the format is <see langword="null"/>, then the
    /// value of the back buffer is used. The <see cref="RenderTargetFormat.Width"/> is used to
    /// define the size of the cube map. <see cref="RenderTargetFormat.Height"/> is ignored.
    /// </param>
    /// <returns>A cube map render target with the specified format.</returns>
    /// <remarks>
    /// This method returns a render target from the pool. If no suitable, free render target is 
    /// found, a new one is created.
    /// </remarks>
    public RenderTargetCube ObtainCube(RenderTargetFormat format)
    {
      RenderTargetCube renderTarget = null;

      var pp = GraphicsService.GraphicsDevice.PresentationParameters;
      int size = format.Width ?? pp.BackBufferWidth;
      bool mipmap = format.Mipmap ?? false;
      var surfaceFormat = format.SurfaceFormat ?? pp.BackBufferFormat;
      var depthStencilFormat = format.DepthStencilFormat ?? pp.DepthStencilFormat;
      int multiSampleCount = format.MultiSampleCount ?? pp.MultiSampleCount;
      var renderTargetUsage = format.RenderTargetUsage ?? RenderTargetUsage.DiscardContents;

      if (Enabled)
      {
        int numberOfItems = RenderTargetsCube.Count;
        int index;
        for (index = numberOfItems - 1; index >= 0; index--)
        {
          var candidate = RenderTargetsCube[index];
          if (candidate.Size == size
              && (candidate.LevelCount > 1) == mipmap
              && candidate.Format == surfaceFormat
              && candidate.DepthStencilFormat == depthStencilFormat
              && candidate.MultiSampleCount == multiSampleCount
              && candidate.RenderTargetUsage == renderTargetUsage)
          {
            renderTarget = candidate;
            break;
          }
        }

        if (renderTarget != null)
        {
          numberOfItems--;
          if (index < numberOfItems)
          {
            RenderTargetsCube[index] = RenderTargetsCube[numberOfItems];
            CountersCube[index] = CountersCube[numberOfItems];
          }

          RenderTargetsCube.RemoveAt(numberOfItems);
          CountersCube.RemoveAt(numberOfItems);

          return renderTarget;
        }
      }

      renderTarget = new RenderTargetCube(GraphicsService.GraphicsDevice, size, mipmap, 
        surfaceFormat, depthStencilFormat, multiSampleCount, renderTargetUsage);

      return renderTarget;
    }


    /// <overloads>
    /// <summary>
    /// Releases a render target and puts it back into the pool for future reuse.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Releases a render target and puts it back into the pool for future reuse.
    /// </summary>
    /// <param name="renderTarget">The render target.</param>
    /// <exception cref="ArgumentException">
    /// The type of <paramref name="renderTarget"/> is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Chosen to avoid nesting.")]
    public void Recycle(Texture renderTarget)
    {
      if (renderTarget == null)
        return;

      if (renderTarget is RenderTarget2D)
        Recycle((RenderTarget2D)renderTarget);
      else if (renderTarget is RenderTargetCube)
        Recycle((RenderTargetCube)renderTarget);
      else
        throw new ArgumentException("Unsupported type of render target.", "renderTarget");
    }


    /// <summary>
    /// Releases a render target and puts it back into the pool for future reuse.
    /// </summary>
    /// <param name="renderTarget">The render target.</param>
    public void Recycle(RenderTarget2D renderTarget)
    {
      if (renderTarget == null || renderTarget.IsDisposed)
        return;

      if (Enabled)
      {
        if ((GlobalSettings.ValidationLevelInternal & GlobalSettings.ValidationLevelUserHighExpensive) != 0)
          if (RenderTargets2D.Contains(renderTarget))
            throw new InvalidOperationException(
              "Cannot recycle render target because it is already in the render target pool.");

        RenderTargets2D.Add(renderTarget);
        Counters2D.Add(0);
      }
      else
      {
        renderTarget.Dispose();
      }
    }


    /// <summary>
    /// Releases a render target and puts it back into the pool for future reuse.
    /// </summary>
    /// <param name="renderTarget">The render target.</param>
    public void Recycle(RenderTargetCube renderTarget)
    {
      if (renderTarget == null || renderTarget.IsDisposed)
        return;

      if (Enabled)
      {
        if ((GlobalSettings.ValidationLevelInternal & GlobalSettings.ValidationLevelUserHighExpensive) != 0)
          if (RenderTargetsCube.Contains(renderTarget))
            throw new InvalidOperationException("Cannot recycle render target because it is already in the render target pool.");

        RenderTargetsCube.Add(renderTarget);
        CountersCube.Add(0);
      }
      else
      {
        renderTarget.Dispose();
      }
    }


    /// <summary>
    /// Manages the cached render targets.
    /// </summary>
    /// <remarks>
    /// This method must be called once per frame. It disposes render targets that are not needed
    /// anymore (see <see cref="FrameLimit"/>).
    /// </remarks>
    public void Update()
    {
      if (Enabled)
      {
        Update(RenderTargets2D, Counters2D);
        Update(RenderTargetsCube, CountersCube);
      }
    }


    private void Update<T>(List<T> renderTargets, List<int> counters) where T : Texture
    {
      int numberOfItems = counters.Count;
      for (int i = 0; i < numberOfItems; i++)
      {
        int counter = counters[i];

        if (counter < FrameLimit && !renderTargets[i].IsDisposed)
        {
          // Keep render target cached. Increase counter.
          counters[i] = counter + 1;
        }
        else
        {
          // Dispose render target and move last render target into this slot.
          renderTargets[i].Dispose();
          numberOfItems--;
          if (i < numberOfItems) // Nothing to do if this entry is the last in the list.
          {
            renderTargets[i] = renderTargets[numberOfItems];
            counters[i] = counters[numberOfItems];

            // We have to check the same index again.
            i--;
          }

          renderTargets.RemoveAt(numberOfItems);
          counters.RemoveAt(numberOfItems);
        }
      }
    }
    #endregion
  }
}
