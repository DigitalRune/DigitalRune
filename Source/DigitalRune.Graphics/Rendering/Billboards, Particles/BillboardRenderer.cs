// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Geometry;
#if !WP7
using DigitalRune.Graphics.PostProcessing;
#endif
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;
#if PARTICLES
using DigitalRune.Particles;
#endif


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders billboards and particles.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="BillboardRenderer"/> is a scene node renderer which handles 
  /// <see cref="BillboardNode"/>s and <see cref="ParticleSystemNode"/>s.
  /// </para>
  /// <para>
  /// Particle systems need to have certain particle parameters for rendering. If a required 
  /// particle parameter is missing, the particle system is ignored by the renderer! See 
  /// <see cref="ParticleSystemNode"/> for more information.
  /// </para>
  /// <para>
  /// <strong>Buffer Size:</strong> The renderer batches billboards and particles using an internal
  /// buffer. The property <see cref="BufferSize"/> limits the number of billboards/particles that 
  /// can be drawn with a single draw call.
  /// </para>
  /// <para>
  /// <strong>Render States:</strong> The <see cref="BillboardRenderer"/> changes the following
  /// render states of the graphics device.
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// The appropriate <see cref="BlendState"/> is set depending on the type of billboards/particles.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Culling is disabled in the <see cref="RasterizerState"/>. Billboards/particles are rendered
  /// "two-sided".
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <para>
  /// When using HiDef profile with soft particles the <see cref="DepthStencilState"/> is set to
  /// <strong>None</strong> (= depth-reads and depth-writes are disabled).
  /// </para>
  /// <para>
  /// When using Reach profile or HiDef profile (without soft particles) the 
  /// <see cref="DepthStencilState"/> is not changed. The <see cref="DepthStencilState"/> should be 
  /// set explicitly before rendering billboards/particles. In most cases depth-writes should be 
  /// disabled, for example:
  /// <code lang="csharp">
  /// <![CDATA[
  /// graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
  /// billboardRenderer.Render(nodes, context);
  /// ]]>
  /// </code>
  /// </para>
  /// </description>
  /// </item>
  /// </list>
  /// <para>
  /// <strong>Soft Particles (require HiDef profile):</strong><br/>
  /// Billboards and particles are usually rendered as flat quads, which cause hard edges when they
  /// intersect with other geometry in the scene. "Soft particles" are rendered by performing an
  /// explicit depth test in the pixel shader. Soft particles fade out near the camera and create 
  /// smooth transitions when they intersect with other geometry.
  /// </para>
  /// <para>
  /// To enable rendering of soft particles set the property <see cref="EnableSoftParticles"/> to 
  /// <see langword="true"/>. In addition the depth buffer needs to be set in the render context 
  /// (property <see cref="RenderContext.GBuffer0"/>). 
  /// </para>
  /// <para>
  /// For image billboards: The <see cref="ImageBillboard.Softness"/> property defines whether 
  /// billboards are rendered "hard" or "soft".
  /// </para>
  /// <para>
  /// For particle systems: The <see cref="ParticleParameterNames.Softness"/> parameter (a uniform
  /// particle parameter of type <see cref="float"/>) defines whether particles are rendered "hard"
  /// or "soft".
  /// </para>
  /// <para>
  /// <strong>High-Speed, Off-Screen Particles (require HiDef profile):</strong><br/>
  /// Large amounts of particles covering the screen can cause a lot of overdraw. This can reduce
  /// the frame rate, if the game is limited by the GPU's fill rate. One solution to this problem
  /// is to render particles into a low-resolution off-screen buffer. This reduces the amount of
  /// overdraw, at the expense of additional image processing overhead and image quality.
  /// </para>
  /// <para>
  /// To enable off-screen rendering set the property <see cref="EnableOffscreenRendering"/> to
  /// <see langword="true"/>. In addition a low-resolution copy of the depth buffer (half width and
  /// height) needs to be stored in <c>renderContext.Data[RenderContextKey.DepthBufferHalf]</c>.
  /// </para>
  /// <para>
  /// In XNA off-screen rendering clears the current back buffer. If necessary the renderer will 
  /// automatically rebuild the back buffer including the depth buffer. For the rebuild step it will
  /// use the same parameters (e.g. near and far bias) as the current 
  /// <see cref="RebuildZBufferRenderer"/> stored in 
  /// <c>renderContext.Data[RenderContextKey.RebuildZBufferRenderer]</c>.
  /// </para>
  /// <note type="warning">
  /// When off-screen rendering is enabled the <see cref="BillboardRenderer"/> automatically 
  /// switches render targets and invalidates the current depth-stencil buffer.
  /// </note>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
  public partial class BillboardRenderer : SceneNodeRenderer
  {
    // Notes: 
    // If view plane-aligned billboards are sorted by planar distance, it is guaranteed
    // that the z-order is correct and there are no intersection.
    // Viewpoint-oriented billboards need to be sorted by linear distance, but 
    // intersections are possible.
    // World-oriented billboards are difficult to sort, intersections are possible.
    // --> Use planar distance for view plane-aligned billboards and linear distance
    //     otherwise.
    //
    // Comparing planar distance and linear distance is not correct, but should work.
    // Intersection between view plane-aligned billboards and viewpoint-oriented 
    // billboards should be minimal: View plane-aligned billboards are usually only 
    // used near the camera and viewpoint-oriented billboards are used for distant 
    // impostors.
    // Intersection between viewpoint-oriented billboards should also be no problems,
    // if billboards are opaque and rendered with depth-writes/depth-test.
    // But distant alpha-transparent impostors (e.g. clouds) could be problematic -
    // needs to be checked in practice!
    //
    // Reach profile:
    // The Reach profile does not support alpha testing of billboards because the
    // AlphaTestEffect tests the total alpha which includes the blend mode. The
    // alpha test should only check the texture alpha!
    //
    // Off-screen Particles vs. Soft Particles:
    // In theory both options can be enabled independently, but in practice off-screen
    // particles are always rendered as soft particles.
    //
    // Text rendering:
    // Theoretically we would draw billboards, particles and texts with the same 
    // shader to reduce code and avoid state switches. But unfortunately the internals 
    // of the SpriteFont are hidden and can only be rendered using the SpriteBatch.
    //
    // Known issues:
    // The constant FontTextureId is used to identify a draw job with a TextBillboard.
    // This assumes that the normal texture IDs never reach FontTextureId (= 2^32 - 1). 
    // If the texture IDs reach this value, there will be a conflict! Resulting behavior: 
    // The billboard is treated as a text billboard and DrawText() ignores the node.
    // The billboard will not be rendered.
    //
    // TODOs:
    // - Support "Origin" for easier placement of billboards.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Represent a <see cref="BillboardNode"/> or a <see cref="ParticleSystem"/> of a 
    /// <see cref="ParticleSystemNode"/>.
    /// </summary>
    /// <remarks>
    /// Note that a <see cref="ParticleSystemNode"/> may have of several nested
    /// <see cref="ParticleSystem"/>s. Each active <see cref="ParticleSystem"/> is represent by a
    /// <see cref="Job"/>.
    /// </remarks>
    [DebuggerDisplay("Job({SortKey})")]
    private struct Job
    {
      // Note: We could used StructLayout(LayoutKind.Explicit) and FieldOffset(n) to
      // pack the Job structure on Windows. However, some members are aligned at odd 
      // addresses. For example, TextureId would be at FieldOffset(1). This raises a
      // MissingMethodException on Xbox 360. (The .NET CF only supports even addresses: 
      // FieldOffset(0), FieldOffset(2), etc.)

      /// <summary>The sort key.</summary>
      public ulong SortKey;

      /// <summary>The texture ID.</summary>
      public uint TextureId { get { return (uint)SortKey; } }

      /// <summary>
      /// The <see cref="BillboardNode"/> or <see cref="ParticleSystemNode"/>.
      /// </summary>
      public SceneNode Node;

#if PARTICLES
      /// <summary>
      /// The render data of the particle system.
      /// </summary>
      public ParticleSystemData ParticleSystemData;
#endif
    }


    private class Comparer : IComparer<Job>
    {
      public static readonly IComparer<Job> Instance = new Comparer();
      public int Compare(Job x, Job y)
      {
        return x.SortKey.CompareTo(y.SortKey);
      }
    }
    #endregion


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
    public const int MaxBufferSize = BillboardBatchReach.MaxBufferSize;

    /// <summary>
    /// The texture ID that is used for text. (When sorting by textures, the text billboards should
    /// be rendered last.)
    /// </summary>
    private const uint FontTextureId = uint.MaxValue;
    #endregion

    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private uint _textureCount;
    private readonly ArrayList<Job> _jobs;

    // Camera information extracted from the view matrix.
    private Pose _cameraPose;         // The position and orientation of the camera in world space.
    private Vector3F _cameraForward;  // The camera forward vector in world space.
    private Vector3F _defaultNormal;  // The default normal vector.
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the graphics service.
    /// </summary>
    /// <value>The graphics service.</value>
    private IGraphicsService GraphicsService { get; set; }


    /// <summary>
    /// Gets the size of the buffer (number of billboards).
    /// </summary>
    /// <value>The size of the buffer (= number of billboards).</value>
    /// <remarks>
    /// The buffer size is the maximal number of billboards that can be rendered with a single draw
    /// call.
    /// </remarks>
    public int BufferSize { get; private set; }


    /// <summary>
    /// Gets or sets the depth threshold used for edge detection when upsampling the off-screen 
    /// buffer.
    /// </summary>
    /// <value>The depth threshold in world space units. The default value is 1 unit.</value>
    /// <remarks>
    /// <para>
    /// When off-screen rendering is enabled (see <see cref="EnableOffscreenRendering"/>), the 
    /// renderer uses bilinear interpolation when upsampling the off-screen buffer, except for edges
    /// where nearest-depth upsampling is used. The <see cref="DepthThreshold"/> is the threshold 
    /// value used for edge detection.
    /// </para>
    /// <para>
    /// In general: A large depth threshold improves image quality, but can cause edge artifacts. A
    /// small depth threshold improves the quality at geometry edges, but may reduce quality at 
    /// non-edges.
    /// </para>
    /// </remarks>
    public float DepthThreshold { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether off-screen rendering is enabled. (Requires HiDef
    /// profile.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if billboards and particles are rendered into an off-screen buffer; 
    /// otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When <see cref="EnableOffscreenRendering"/> is set, all particles are rendered into a 
    /// low-resolution off-screen buffer. The final off-screen buffer is upscaled and combined with
    /// the scene.
    /// </para>
    /// <para>
    /// This option should be enabled if the amount of particle overdraw causes a frame rate drop.
    /// Off-screen rendering reduces overdraw, at the expense of additional image processing 
    /// overhead and image quality.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> A downsampled version of the depth buffer (half width and 
    /// height) needs to be stored in <c>renderContext.Data[RenderContextKey.DepthBufferHalf]</c>.
    /// </para>
    /// <para>
    /// When off-screen rendering is used, the hardware depth buffer information is lost. This 
    /// renderer restores the depth buffer when it combines the off-screen buffer with the render
    /// target in the final step. The restored depth buffer is not totally accurate.
    /// For the rebuild step the renderer will use the same parameters (e.g. near and far bias) as 
    /// the current <see cref="RebuildZBufferRenderer"/> stored in 
    /// <c>renderContext.Data[RenderContextKey.RebuildZBufferRenderer]</c>.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public bool EnableOffscreenRendering { get; set; }


#if !WP7
    /// <summary>
    /// Gets or sets the upsampling filter that is used for combining the off-screen buffer with 
    /// the scene.
    /// </summary>
    /// <value>
    /// The upsampling filter for off-screen rendering. The default value is 
    /// <see cref="Rendering.UpsamplingFilter.NearestDepth"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [Obsolete("The property UpsamplingFilter has been renamed to avoid confusion with the new UpsampleFilter post-processor. Use the new property UpsamplingMode instead.")]
    public UpsamplingFilter UpsamplingFilter
    {
      get { return (UpsamplingFilter)UpsamplingMode; }
      set { UpsamplingMode = (UpsamplingMode)value; }
    }


    /// <summary>
    /// Gets or sets the upsampling filter that is used for combining the off-screen buffer with 
    /// the scene.
    /// </summary>
    /// <value>
    /// The upsampling filter for off-screen rendering. The default value is 
    /// <see cref="PostProcessing.UpsamplingMode.NearestDepth"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public UpsamplingMode UpsamplingMode { get; set; }
#endif


    /// <summary>
    /// Gets or sets the factor used to bias the camera near plane when the z-buffer is
    /// reconstructed. (Only used when <see cref="EnableOffscreenRendering"/> is set.)
    /// </summary>
    /// <value>The near bias factor. The default value is 1 (no bias).</value>
    /// <remarks>
    /// <para>
    /// When off-screen rendering is used, the hardware depth buffer information is lost. This 
    /// renderer restores the depth buffer when it combines the off-screen buffer with the render
    /// target in the final step. The restored depth buffer is not totally accurate.
    /// <see cref="NearBias"/> and <see cref="FarBias"/> can be used to bias the restored depth
    /// values to reduce z-fighting of any geometry which is rendered using the restored depth
    /// buffer.
    /// </para>
    /// </remarks>
    [Obsolete("The NearBias is now determined by the RebuildZBufferRenderer stored in RenderContext[RenderContextKeys.RebuildZBufferRenderer].")]
    public float NearBias { get; set; }


    /// <summary>
    /// Gets or sets the bias factor used to bias the camera near plane when the z-buffer is
    /// reconstructed. (Only used when <see cref="EnableOffscreenRendering"/> is set.)
    /// </summary>
    /// <value>The far bias factor. The default value is 0.995f.</value>
    /// <inheritdoc cref="NearBias"/>
    [Obsolete("The FarBias is now determined by the RebuildZBufferRenderer stored in RenderContext[RenderContextKeys.RebuildZBufferRenderer].")]
    public float FarBias { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether soft particles are enabled. (Requires HiDef 
    /// profile.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if soft particles are enabled; otherwise, <see langword="false"/>.
    /// The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When soft particles are enabled the renderer performs an explicit depth test in the pixel
    /// shader and creates smooth transitions when a particle intersects with other geometry in the
    /// scene.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The depth buffer needs to be set in the render context (property
    /// <see cref="RenderContext.GBuffer0"/>).
    /// </para>
    /// </remarks>
    public bool EnableSoftParticles { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="BillboardRenderer" /> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="BillboardRenderer" /> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="bufferSize">
    /// The size of the internal buffer (= max number of billboards that can be rendered in a single 
    /// draw call). Max allowed value is 16384.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="bufferSize"/> is 0, negative, or greater than <see cref="MaxBufferSize"/>.
    /// </exception>
    public BillboardRenderer(IGraphicsService graphicsService, int bufferSize)
      : this(graphicsService, bufferSize, null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="BillboardRenderer" /> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="bufferSize">
    /// The size of the internal buffer (= max number of billboards that can be rendered in a single 
    /// draw call). Max allowed value is 16384.
    /// </param>
    /// <param name="spriteFont">
    /// The default font, which is used in case the font of a <see cref="TextSprite"/> is not set.
    /// Can be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="bufferSize"/> is 0, negative, or greater than <see cref="MaxBufferSize"/>.
    /// </exception>
    public BillboardRenderer(IGraphicsService graphicsService, int bufferSize, SpriteFont spriteFont)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");
      if (bufferSize <= 0 || bufferSize > MaxBufferSize)
        throw new ArgumentOutOfRangeException("bufferSize", "The buffer size must be in the range [1, " + MaxBufferSize + "].");

      Order = 2;
      GraphicsService = graphicsService;
      BufferSize = bufferSize;
      DepthThreshold = 1;
#if !WP7
      UpsamplingMode = UpsamplingMode.NearestDepth;
#endif

      // Start with a reasonably large capacity to avoid frequent re-allocations.
      _jobs = new ArrayList<Job>(32);

      InitializeBillboards(graphicsService);
      InitializeText(spriteFont);
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
          DisposeBillboards();
          DisposeText();
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
      return node is BillboardNode 
#if PARTICLES
             || node is ParticleSystemNode
#endif
             ;
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      context.ThrowIfCameraMissing();
      Debug.Assert(_jobs.Count == 0, "Job list was not properly reset.");

      // Reset counter.
      _textureCount = 0;

      BatchJobs(nodes, context, order);
      if (_jobs.Count > 0)
      {
        ProcessJobs(context);
        _jobs.Clear();
      }

      if ((GlobalSettings.ValidationLevelInternal & GlobalSettings.ValidationLevelDevBasic) != 0)
        ValidateNodes(nodes);
    }


    private void BatchJobs(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      // Camera parameters.
      var cameraNode = context.CameraNode;
      _cameraPose = cameraNode.PoseWorld;
      _defaultNormal = _cameraPose.Orientation.GetColumn(2);  // Local z-axis.
      _cameraForward = -_defaultNormal;

      bool sortByDistance = (order != RenderOrder.UserDefined);
      bool backToFront = (order == RenderOrder.Default || order == RenderOrder.BackToFront);

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      // Add draw jobs to list.
      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i];
        var billboardNode = node as BillboardNode;
        if (billboardNode != null)
        {
          // BillboardNode is visible in current frame.
          billboardNode.LastFrame = frame;

          AddJob(billboardNode, sortByDistance, backToFront);
          continue;
        }

#if PARTICLES
        var particleSystemNode = node as ParticleSystemNode;
        if (particleSystemNode != null)
        {
          // ParticleSystemNode is visible in current frame.
          particleSystemNode.LastFrame = frame;

          AddJob(particleSystemNode, sortByDistance, backToFront);
          continue;
        }
#endif
      }

      if (_jobs.Count > 0 && order != RenderOrder.UserDefined)
      {
        // Sort draw jobs.
        _jobs.Sort(Comparer.Instance);
      }
    }


    // Creates the draw jobs for a billboard node.
    private void AddJob(BillboardNode node, bool sortByDistance, bool backToFront)
    {
      var billboard = node.Billboard;

      float distance = 0;
      if (sortByDistance)
      {
        // Determine distance to camera.
        Vector3F cameraToNode = node.PoseWorld.Position - _cameraPose.Position;

        // Planar distance: Project vector onto look direction.
        distance = Vector3F.Dot(cameraToNode, _cameraForward);

        // Use linear distance for viewpoint-oriented and world-oriented billboards.
        if (billboard.Orientation.Normal != BillboardNormal.ViewPlaneAligned)
          distance = cameraToNode.LengthSquared * Math.Sign(distance);

        if (backToFront)
          distance = -distance;
      }

      uint textureId;
      var imageBillboard = billboard as ImageBillboard;
      if (imageBillboard != null)
      {
        // Image billboard
        textureId = GetTextureId(imageBillboard.Texture);
      }
      else
      {
        // Text billboard: Use predefined constant.
        textureId = FontTextureId;
      }

      // Add draw job to list.
      var job = new Job
      {
        SortKey = GetSortKey(distance, 0, textureId),
        Node = node,
      };
      _jobs.Add(ref job);
    }


#if PARTICLES
    // Creates the draw jobs for a particle system node.
    private void AddJob(ParticleSystemNode node, bool sortByDistance, bool backToFront)
    {
      var renderData = node.ParticleSystem.RenderData as ParticleSystemData;
      if (renderData == null)
        return;

      // Add root particle system.
      AddJob(node, renderData, sortByDistance, backToFront);

      // Add nested particle systems.
      if (renderData.NestedRenderData != null)
        foreach (var nestedRenderData in renderData.NestedRenderData)
          AddJob(node, nestedRenderData, sortByDistance, backToFront);
    }


    private void AddJob(ParticleSystemNode node, ParticleSystemData particleSystemData, bool sortByDistance, bool backToFront)
    {
      if (particleSystemData.Particles.Count == 0)
        return;

      float distance = 0;
      if (sortByDistance)
      {
        // Position relative to ParticleSystemNode (root particle system).
        Vector3F position = particleSystemData.Pose.Position;

        // Position in world space.
        position = node.PoseWorld.ToWorldPosition(position);

        // Determine distance to camera.
        Vector3F cameraToNode = position - _cameraPose.Position;

        // Planar distance: Project vector onto look direction.
        distance = Vector3F.Dot(cameraToNode, _cameraForward);

        // Use linear distance for viewpoint-oriented and world-oriented billboards.
        if (particleSystemData.BillboardOrientation.Normal != BillboardNormal.ViewPlaneAligned)
          distance = cameraToNode.LengthSquared * Math.Sign(distance);

        if (backToFront)
          distance = -distance;
      }

      // Add draw job to list.
      ushort drawOrder = (ushort)particleSystemData.DrawOrder;
      var textureId = GetTextureId(particleSystemData.Texture);
      var job = new Job
      {
        SortKey = GetSortKey(distance, drawOrder, textureId),
        Node = node,
        ParticleSystemData = particleSystemData,
      };
      _jobs.Add(ref job);
    }
#endif


    /// <summary>
    /// Gets the sort key.
    /// </summary>
    /// <param name="distance">The distance.</param>
    /// <param name="drawOrder">The draw order.</param>
    /// <param name="textureId">The texture ID.</param>
    /// <returns>The key for sorting draw jobs.</returns>
    private static ulong GetSortKey(float distance, ushort drawOrder, uint textureId)
    {
      // --------------------------------------------
      // |  distance  |  draw order  |  texture ID  |
      // |   16 bit   |   16 bit     |   32 bit     |
      // --------------------------------------------

      ulong sortKey = (ulong)Numeric.GetSignificantBitsSigned(distance, 16) << 48
                      | (ulong)drawOrder << 32
                      | textureId;

      return sortKey;
    }


    private void ProcessJobs(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      PrepareBillboards(context);

      // Batch billboards using the same texture.
      int index = 0;
      var jobs = _jobs.Array;
      int jobCount = _jobs.Count;
      while (index < jobCount)
      {
        uint textureId = jobs[index].TextureId;

        int endIndex = index + 1;
        while (endIndex < jobCount && jobs[endIndex].TextureId == textureId)
          endIndex++;

        // Submit batch.
        if (textureId == FontTextureId)
        {
          // Text
          EndBillboards(context);
          DrawText(index, endIndex, context);
        }
        else
        {
          // Billboards, particles
          BeginBillboards(context);
          DrawBillboards(index, endIndex, context);
        }

        index = endIndex;
      }

      EndBillboards(context);
      savedRenderState.Restore();
    }


    #region ----- Texture IDs -----

    // Each Texture2D gets a unique ID, which is used for state sorting.
    // The ID is assigned during BatchJobs() and reset during ProcessJobs().
    private uint GetTextureId(PackedTexture texture)
    {
      if (texture == null)
        return 0; // _defaultTexture will be used.

      if (texture.TextureAtlasEx.Id == 0)
      {
        _textureCount++;
        texture.TextureAtlasEx.Id = _textureCount;
      }

      return texture.TextureAtlasEx.Id;
    }


    private static void ResetTextureId(PackedTexture texture)
    {
      texture.TextureAtlasEx.Id = 0;
    }


    private static void ValidateNodes(IList<SceneNode> nodes)
    {
      // Check whether all texture IDs have been reset.
      for (int i = 0; i < nodes.Count; i++)
      {
        var node = nodes[i];
        var billboardNode = node as BillboardNode;
        if (billboardNode != null)
        {
          var imageBillboard = billboardNode.Billboard as ImageBillboard;
          if (imageBillboard != null
              && imageBillboard.Texture != null
              && imageBillboard.Texture.TextureAtlasEx.Id != 0)
          {
            throw new GraphicsException("Texture ID has not been reset.");
          }
        }

#if PARTICLES
        var particleSystemNode = node as ParticleSystemNode;
        if (particleSystemNode != null)
        {
          var renderData = particleSystemNode.ParticleSystem.RenderData as ParticleSystemData;
          if (renderData != null)
          {
            if (renderData.Texture != null
                && renderData.Texture.TextureAtlasEx.Id != 0)
            {
              throw new GraphicsException("Texture ID has not been reset.");
            }

            if (renderData.NestedRenderData != null)
            {
              foreach (var nestedRenderData in renderData.NestedRenderData)
              {
                if (nestedRenderData.Texture != null
                    && nestedRenderData.Texture.TextureAtlasEx.Id != 0)
                {
                  throw new GraphicsException("Texture ID has not been reset.");
                }
              }
            }
          }
        }
#endif
      }
    }
    #endregion

    #endregion
  }
}
