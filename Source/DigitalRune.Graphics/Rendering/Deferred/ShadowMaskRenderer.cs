// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders the shadow mask from the shadow map of a <see cref="LightNode"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The shadow mask is an image as seen from the camera where for each pixel the shadow info is
  /// stored. A value of 0 means the pixel is in the shadow. A value of 1 means the pixel is fully
  /// lit. (The shadow mask is rendered into the current render target.)
  /// </para>
  /// <para>
  /// This renderer renders the shadow masks and sets the properties <see cref="Shadow.ShadowMask"/>
  /// and <see cref="Shadow.ShadowMaskChannel"/> of the handled <see cref="Shadow"/> instances. The 
  /// <see cref="ShadowMaskRenderer"/> handles <see cref="StandardShadow"/>s,
  /// <see cref="CubeMapShadow"/>, <see cref="CascadedShadow"/>s, and
  /// <see cref="CompositeShadow"/>s. To handle new shadow types, you need to add a custom
  /// <see cref="SceneNodeRenderer"/> to the <see cref="SceneRenderer.Renderers"/> collection.
  /// </para>
  /// <para>
  /// <see cref="RenderContext.GBuffer0"/> needs to be set in the render context.
  /// </para>
  /// <para>
  /// <see cref="RecycleShadowMasks"/> should be called every frame when shadow masks are not needed
  /// anymore. This method returns all shadow mask render targets to the render target pool and
  /// allows other render operations to reuse the render targets.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer changes the current render target of the graphics device because it uses the
  /// graphics device to render the shadow masks into internal render targets. The render target
  /// and the viewport of the graphics device are undefined after rendering.
  /// </para>
  /// </remarks>
  public class ShadowMaskRenderer : SceneRenderer
  {
    // Notes:
    // Possible Optimization: Compute GetLightContribution() lazily. If a node has a higher
    // priority than all the rest, then we never need to compute GetLightContribution() for this
    // node. --> Set all SortTags to -1. And initialize it in a custom IComparer when needed. 


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IGraphicsService _graphicsService;

    private RenderTarget2D[] _shadowMasks;
    private List<LightNode>[] _shadowMaskBins;      // A list of scene nodes for each channel of the shadow masks.
    private readonly List<LightNode> _lightNodes;   // Temporary list for sorting light nodes.

    private UpsampleFilter _upsampleFilter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the max number of shadows masks.
    /// </summary>
    /// <value>The max number of shadows masks. The default value is 1.</value>
    /// <remarks>
    /// A shadow mask is an RGBA8 render target (= 4 channels). One shadow mask can store 4 shadows
    /// (or more, if shadow casting lights do not overlap). Two shadow masks can store 8 shadows (or
    /// more)... In most scenarios a single shadow mask should be enough.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or greater than 255.
    /// </exception>
    public int MaxNumberOfShadowsMasks
    {
      get { return _maxNumberOfShadowsMasks; }
      set
      {
        if (value < 0 || value > 255)
          throw new ArgumentOutOfRangeException("value", "The number of shadows masks must be in the range [0, 255].");

        _maxNumberOfShadowsMasks = value;
        _shadowMasks = new RenderTarget2D[value];
        ShadowMasks = new ReadOnlyCollection<RenderTarget2D>(_shadowMasks);
        _shadowMaskBins = new List<LightNode>[value * 4];
        for (int i = 0; i < _shadowMaskBins.Length; i++)
          _shadowMaskBins[i] = new List<LightNode>();
      }
    }
    private int _maxNumberOfShadowsMasks;


    /// <summary>
    /// Gets the shadow masks. (For debugging only.)
    /// </summary>
    /// <value>The shadow masks. (For debugging only.)</value>
    /// <remarks>
    /// The list may contain null entries.
    /// </remarks>
    public ReadOnlyCollection<RenderTarget2D> ShadowMasks { get; private set; }


    /// <summary>
    /// Gets or sets a filter that is applied to the shadow masks as a post-process.
    /// </summary>
    /// <value>
    /// The filter that is applied to the shadow masks as a post-process. The default value is
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The shadow quality can be improved by filtering the resulting shadow mask. For example, an
    /// anisotropic, cross-bilateral Gaussian filter can be applied to create soft shadows.
    /// </para>
    /// <para>
    /// The configured post-process filter needs to support reading from and writing into the same
    /// render target. This is supported by any separable box or Gaussian blur because they filter
    /// the image in two passes. Single pass blurs, e.g. a Poisson blur, cannot be used.
    /// </para>
    /// </remarks>
    public PostProcessor Filter { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the shadow mask is created using only the half scene
    /// resolution to improve performance.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the shadow mask is created using only the half scene resolution to
    /// improve performance; otherwise, <see langword="false"/> to use the full resolution for best
    /// quality. The default is <see langword="false" />.
    /// </value>
    public bool UseHalfResolution { get; set; }


    /// <summary>
    /// Gets or sets a value controlling the bilateral upsampling. (Only used when
    /// <see cref="UseHalfResolution"/> is <see langword="true" />.)
    /// </summary>
    /// <value>
    /// The depth sensitivity for bilateral upsampling. Use 0 to use bilinear upsampling and disable
    /// bilateral upsampling. Use values greater than 0, to enable bilateral upsampling. The default
    /// value is 1000.
    /// </value>
    /// <remarks>
    /// <para>
    /// If <see cref="UseHalfResolution"/> is <see langword="true" />, the shadow mask is created
    /// using the half scene resolution. Creating shadows using the low resolution shadow mask can
    /// create artifacts, e.g. a non-shadowed halo around objects. To avoid these artifacts,
    /// bilateral upsampling can be enabled, by setting <see cref="UpsampleDepthSensitivity"/> to a
    /// value greater than 0.
    /// </para>
    /// <para>
    /// For more information about bilateral upsampling, see <see cref="UpsampleFilter"/> and
    /// <see cref="UpsampleFilter.DepthSensitivity"/>.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Upsample")]
    public float UpsampleDepthSensitivity { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowMaskRenderer"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowMaskRenderer"/> class with a single
    /// shadow mask.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public ShadowMaskRenderer(IGraphicsService graphicsService)
      : this(graphicsService, 1)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowMaskRenderer"/> class with the specified
    /// number of shadow masks.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="maxNumberOfShadowMasks">The max number of shadow masks.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxNumberOfShadowMasks"/> is negative or greater than 255.
    /// </exception>
    public ShadowMaskRenderer(IGraphicsService graphicsService, int maxNumberOfShadowMasks)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _graphicsService = graphicsService;
      _lightNodes = new List<LightNode>();
      MaxNumberOfShadowsMasks = maxNumberOfShadowMasks;
      UpsampleDepthSensitivity = 1000;

      Renderers.Add(new StandardShadowMaskRenderer(graphicsService));
      Renderers.Add(new CubeMapShadowMaskRenderer(graphicsService));
      Renderers.Add(new CascadedShadowMaskRenderer(graphicsService));
      Renderers.Add(new CompositeShadowMaskRenderer(graphicsService, Renderers));
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
          RecycleShadowMasks();
        }
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return (node is LightNode) && base.CanRender(node, context);
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      // Start with a clean state. Remove any references from/to light nodes.
      RecycleShadowMasks();

      base.Render(nodes, context, order);
    }


    internal override void BatchJobs(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      // Assign temporary IDs to scene node renderers.
      for (int i = 0; i < Renderers.Count; i++)
        Renderers[i].Id = (uint)(i & 0xff);  // ID = index clamped to [0, 255].

      Debug.Assert(_lightNodes.Count == 0, "Internal list of light nodes has not been cleared.");
      _lightNodes.Clear();

      // Collect all shadow-casting lights.
      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as LightNode;
        if (node != null 
            && node.Shadow != null 
            && (node.Shadow.ShadowMap != null || node.Shadow is CompositeShadow))
        {
          _lightNodes.Add(node);
        }
      }

      // If there are too many shadow-casting lights, sort them by importance.
      numberOfNodes = _lightNodes.Count;
      if (_lightNodes.Count > _shadowMaskBins.Length)
      {
        for (int i = 0; i < numberOfNodes; i++)
        {
          var lightNode = _lightNodes[i];
          lightNode.SortTag = lightNode.GetLightContribution(context.CameraNode.PoseWorld.Position, 0.7f);
        }

        _lightNodes.Sort(DescendingLightNodeComparer.Instance);
      }

      // Add jobs.
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = _lightNodes[i];

        var job = new Job();
        job.Node = node;
        foreach (var renderer in Renderers)
        {
          if (renderer.CanRender(node, context))
          {
            job.Renderer = renderer;
            break;
          }
        }

        if (job.Renderer == null)
          continue;

        // Assign shadow mask index.
        int shadowMaskIndex = AssignShadowMask(node, context);
        if (shadowMaskIndex < 0)
          continue;

        job.SortKey = GetSortKey(shadowMaskIndex, job.Renderer.Order, job.Renderer.Id);
        Jobs.Add(ref job);
      }

      foreach (var bin in _shadowMaskBins)
        bin.Clear();

      if (order != RenderOrder.UserDefined)
      {
        // Sort draw jobs.
        Jobs.Sort(Comparer.Instance);
      }
    }


    /// <summary>
    /// Gets the sort key.
    /// </summary>
    /// <param name="shadowMaskIndex">The index of the shadow mask.</param>
    /// <param name="order">The order of the renderer.</param>
    /// <param name="id">The ID of the renderer.</param>
    /// <returns>The key for sorting draw jobs.</returns>
    private static uint GetSortKey(int shadowMaskIndex, int order, uint id)
    {
      Debug.Assert(0 <= shadowMaskIndex && shadowMaskIndex <= byte.MaxValue, "Shadow mask index is out of range.");
      Debug.Assert(0 <= order && order <= byte.MaxValue, "Order is out of range.");
      Debug.Assert(id <= byte.MaxValue, "ID is out of range.");

      // ------------------------------------------------------
      // |  unused  |  shadow mask index  |  order  |  ID     |
      // |  8 bit   |  8 bit              |  8 bit  |  8 bit  |
      // ------------------------------------------------------

      return (uint)shadowMaskIndex << 16
             | (uint)order << 8
             | id;
    }


    private int AssignShadowMask(LightNode lightNode, RenderContext context)
    {
      // Each shadow mask has 4 8-bit channels. We must assign a shadow mask channel to 
      // each shadow-casting light. Non-overlapping lights can use the same channel.
      // Overlapping lights must use different channels. If we run out of channels,
      // we remove some lights from the list.

      var scene = context.Scene;

      var viewport = context.Viewport;
      int maskWidth = viewport.Width;
      int maskHeight = viewport.Height;
      if (UseHalfResolution && Numeric.IsLessOrEqual(UpsampleDepthSensitivity, 0))
      {
        // Half-res rendering with no upsampling.
        maskWidth /= 2;
        maskHeight /= 2;
      }

      // Loop over all bins until we find one which can be used for this light node.
      int binIndex;
      for (binIndex = 0; binIndex < _shadowMaskBins.Length; binIndex++)
      {
        var bin = _shadowMaskBins[binIndex];

        // Check if the light node touches any other light nodes in this bin.
        bool hasContact = false;
        foreach (var otherLightNode in bin)
        {
          if (scene.HaveContact(lightNode, otherLightNode))
          {
            hasContact = true;
            break;
          }
        }

        // No overlap. Use this bin.
        if (!hasContact)
        {
          bin.Add(lightNode);
          break;
        }
      }

      if (binIndex >= _shadowMaskBins.Length)
        return -1;  // Light node does not fit into any bin.

      int shadowMaskIndex = binIndex / 4;

      if (_shadowMasks[shadowMaskIndex] == null)
      {
        // Create shadow mask.
        var shadowMaskFormat = new RenderTargetFormat(maskWidth, maskHeight, false, SurfaceFormat.Color, DepthFormat.None);
        _shadowMasks[shadowMaskIndex] = context.GraphicsService.RenderTargetPool.Obtain2D(shadowMaskFormat);
      }

      // Assign shadow mask to light node.
      lightNode.Shadow.ShadowMask = _shadowMasks[shadowMaskIndex];
      lightNode.Shadow.ShadowMaskChannel = binIndex % 4;

      return shadowMaskIndex;
    }


    internal override void ProcessJobs(RenderContext context, RenderOrder order)
    {
      var graphicsDevice = _graphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      Debug.Assert(_shadowMasks.Length > 0);
      Debug.Assert(_shadowMasks[0] != null);

      RenderTarget2D lowResTarget = null;
      if (UseHalfResolution && Numeric.IsGreater(UpsampleDepthSensitivity, 0))
      {
        // Half-res rendering with upsampling.
        var format = new RenderTargetFormat(_shadowMasks[0]);
        format.Width /= 2;
        format.Height /= 2;
        lowResTarget = _graphicsService.RenderTargetPool.Obtain2D(format);
      }

      int index = 0;
      var jobs = Jobs.Array;
      int jobCount = Jobs.Count;
      int lastShadowMaskIndex = -1;
      while (index < jobCount)
      {
        int shadowMaskIndex = (int)(jobs[index].SortKey >> 16);
        var renderer = jobs[index].Renderer;

        // Find end of current batch.
        int endIndexExclusive = index + 1;
        while (endIndexExclusive < jobCount)
        {
          if ((int)(jobs[endIndexExclusive].SortKey >> 16) != lastShadowMaskIndex
              || jobs[endIndexExclusive].Renderer != renderer)
          {
            break;
          }

          endIndexExclusive++;
        }

        // Restore the render state. (The integrated scene node renderers properly
        // restore the render state, but third-party renderers might mess it up.)
        if (index > 0)
          savedRenderState.Restore();

        if (shadowMaskIndex != lastShadowMaskIndex)
        {
          // Done with current shadow mask. Apply filter.
          if (lastShadowMaskIndex >= 0)
            PostProcess(context, context.RenderTarget, _shadowMasks[lastShadowMaskIndex]);

          // Switch to next shadow mask.
          lastShadowMaskIndex = shadowMaskIndex;

          var shadowMask = lowResTarget ?? _shadowMasks[shadowMaskIndex];

          // Set device render target and clear it to white (= no shadow).
          graphicsDevice.SetRenderTarget(shadowMask);
          context.RenderTarget = shadowMask;
          context.Viewport = graphicsDevice.Viewport;
          graphicsDevice.Clear(Color.White);
        }

        // Submit batch to renderer.
        // (Use Accessor to expose current batch as IList<SceneNode>.)
        JobsAccessor.Set(Jobs, index, endIndexExclusive);
        renderer.Render(JobsAccessor, context, order);
        JobsAccessor.Reset();

        index = endIndexExclusive;
      }

      // Done with last shadow mask. Apply filter.
      PostProcess(context, context.RenderTarget, _shadowMasks[lastShadowMaskIndex]);

      savedRenderState.Restore();
      graphicsDevice.ResetTextures();
      graphicsDevice.SetRenderTarget(null);
      context.RenderTarget = target;
      context.Viewport = viewport;

      _graphicsService.RenderTargetPool.Recycle(lowResTarget);
    }


    private void PostProcess(RenderContext context, RenderTarget2D source, RenderTarget2D target)
    {
      Debug.Assert(source != null);
      Debug.Assert(target != null);

      var originalSourceTexture = context.SourceTexture;
      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;

      if (Filter != null && Filter.Enabled)
      {
        context.SourceTexture = source;
        context.RenderTarget = source;
        context.Viewport = new Viewport(0, 0, source.Width, source.Height);
        Filter.Process(context);
      }

      bool doUpsampling = UseHalfResolution && Numeric.IsGreater(UpsampleDepthSensitivity, 0);

      Debug.Assert(doUpsampling && source != target || !doUpsampling && source == target);

      if (doUpsampling)
      {
        if (_upsampleFilter == null)
          _upsampleFilter = _graphicsService.GetUpsampleFilter();

        var graphicsDevice = _graphicsService.GraphicsDevice;
        var originalBlendState = graphicsDevice.BlendState;
        graphicsDevice.BlendState = BlendState.Opaque;

        // The previous scene render target is bound as texture.
        // --> Switch scene render targets!
        context.SourceTexture = source;
        context.RenderTarget = target;
        context.Viewport = new Viewport(0, 0, target.Width, target.Height);
        _upsampleFilter.DepthSensitivity = UpsampleDepthSensitivity;
        _upsampleFilter.Mode = UpsamplingMode.Bilateral;
        _upsampleFilter.RebuildZBuffer = false;
        _upsampleFilter.Process(context);

        if (originalBlendState != null)
          graphicsDevice.BlendState = originalBlendState;
      }

      context.SourceTexture = originalSourceTexture;
      context.RenderTarget = originalRenderTarget;
      context.Viewport = originalViewport;
    }


    /// <summary>
    /// Recycles the shadow masks.
    /// </summary>
    /// <remarks>
    /// This method also resets the shadow properties <see cref="Shadow.ShadowMask"/> and
    /// <see cref="Shadow.ShadowMaskChannel"/>.
    /// </remarks>
    public void RecycleShadowMasks()
    {
      foreach (var lightNode in _lightNodes)
      {
        var shadow = lightNode.Shadow;
        if (shadow != null)
        {
          shadow.ShadowMask = null;
          shadow.ShadowMaskChannel = 0;
        }
      }
      _lightNodes.Clear();

      var renderTargetPool = _graphicsService.RenderTargetPool;
      for (int i = 0; i < _shadowMasks.Length; i++)
      {
        renderTargetPool.Recycle(_shadowMasks[i]);
        _shadowMasks[i] = null;
      }
    }
    #endregion
  }
}
#endif
