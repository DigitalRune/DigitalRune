// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Represents an occlusion buffer that supports frustum culling, distance culling, occlusion 
  /// culling, and shadow caster culling.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The occlusion buffer can be used to determine which objects in a scene need to be rendered.
  /// Objects that are outside the viewing frustum or occluded by other objects don't have to be
  /// processed during rendering. The occlusion buffer implements <i>frustum culling</i>,
  /// <i>distance culling</i>, <i>occlusion culling</i>, and <i>shadow caster culling</i>.
  /// </para>
  /// <para>
  /// To use occlusion culling: The method <see cref="Render(IList{SceneNode},RenderContext)"/>
  /// clears the occlusion buffer and renders all occluders. This method needs to be called once per
  /// frame, even if there are no occluders in the scene. <see cref="Query"/> performs culling on a
  /// list of scene nodes. Scene nodes that are culled are replaced by null entries in the list.
  /// </para>
  /// <para>
  /// <strong>Frustum Culling:</strong><br/>
  /// The occlusion buffer implements frustum culling to determine which objects are within the
  /// viewing frustum of the camera. Objects outside the viewing frustum are hidden from the camera
  /// and do not have to be rendered. (The occlusion buffer can be used for frustum culling instead
  /// of using <see cref="IScene.Query{T}">scene queries</see>.)
  /// </para>
  /// <para>
  /// The following example shows how to use the occlusion buffer for frustum culling. The active
  /// camera needs to be set in the render context.
  /// </para>
  /// <code lang="csharp" title="Example: Frustum culling using the occlusion buffer">
  /// <![CDATA[
  /// // Clear the occlusion buffer.
  /// occlusionBuffer.Render(null, context);
  /// 
  /// // Perform frustum culling on the list of scene nodes. 
  /// // (Scene nodes that culled are replaced by null entries.)
  /// occlusionBuffer.Query(sceneNodes, context);
  /// ]]>
  /// </code>
  /// <para>
  /// <strong>Distance Culling and LOD Distance:</strong><br/>
  /// (Prerequisite: <see cref="RenderContext.LodCameraNode"/> needs to be set in the render
  /// context!)
  /// </para>
  /// <para>
  /// The occlusion buffer automatically calculates the LOD distance of each scene node in 
  /// <see cref="Query"/> and performs distance culling if a <see cref="SceneNode.MaxDistance"/> is
  /// set. Scene nodes that are beyond their max draw distance are removed. The LOD distance of the
  /// remaining scene nodes is stored in <see cref="SceneNode.SortTag"/>. This value can be used for
  /// LOD selection (see <see cref="LodGroupNode"/> and <see cref="ISceneQuery"/>).
  /// </para>
  /// <para>
  /// <strong>Occlusion Culling:</strong><br/>
  /// Occlusion culling is the process of determining which objects are hidden from a certain
  /// viewpoint. This is achieved by testing the scene nodes against a set of occluders. An
  /// <i>occluder</i> is an object within a scene that obscures the view and prevents objects behind
  /// it from being seen.
  /// </para>
  /// <para>
  /// By calling <see cref="Render(IList{SceneNode},RenderContext)"/> all occluders within a scene
  /// are rendered into a depth buffer. The following objects act as occluders during occlusion
  /// culling:
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// <see cref="OccluderNode"/>s
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <see cref="MeshNode"/>s with occluders (property <see cref="Mesh.Occluder"/>)
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <see cref="LodGroupNode"/>s if the highest level of detail ("LOD0") is an
  /// <see cref="OccluderNode"/> or a <see cref="MeshNode"/> with an occluder
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Any <see cref="SceneNode"/> can act as an occluder if the appropriate
  /// <see cref="SceneNodeRenderer"/> is passed to 
  /// <see cref="Render(IList{SceneNode},LightNode,SceneNodeRenderer,RenderContext)"/>.
  /// </description>
  /// </item>
  /// </list>
  /// <para>
  /// <see cref="Query"/> can be called to test whether scene nodes are visible. (The bounds of the
  /// scene nodes are compared with the current occlusion buffer.)
  /// </para>
  /// <para>
  /// The following example shows how to use the occlusion buffer for occlusion culling.
  /// </para>
  /// <code lang="csharp" title="Example: Occlusion culling using the occlusion buffer">
  /// <![CDATA[
  /// // Render the occluders into the occlusion buffer.
  /// occlusionBuffer.Render(occluders, null, null, context);
  /// 
  /// // Perform occlusion culling on the list of scene nodes.
  /// // (Scene nodes that are culled are replaced by null entries.)
  /// occlusionBuffer.Query(sceneNodes, context);
  /// ]]>
  /// </code>
  /// <para>
  /// The next example shows how to render custom scene nodes into the occlusion buffer. For
  /// example, <see cref="MeshNode"/>s that support an "Occluder" render pass can be rendered
  /// directly into the occlusion buffer if the appropriate render is provided.
  /// </para>
  /// <code lang="csharp" title="Example: Rendering custom occluders">
  /// <![CDATA[
  /// // Render the occluders into the occlusion buffer. MeshNodes that do not have an
  /// // occluder but have an "Occluder" render pass are rendered using the MeshRenderer.
  /// context.RenderPass = "Occluder";
  /// occlusionBuffer.Render(occluders, null, meshRenderer, context);
  /// context.RenderPass = null;
  /// 
  /// // Perform occlusion culling on the list of scene nodes.
  /// // Scene nodes that are culled are replaced by null entries.
  /// occlusionBuffer.Query(sceneNodes, context);
  /// ]]>
  /// </code>
  /// <para>
  /// <strong>Shadow Caster Culling:</strong><br/>
  /// Shadow caster culling determines which shadows contribute to the final image. If the shadow
  /// cast by an object is not visible, the object can be culled and does not need to be rendered
  /// into the shadow map.
  /// </para>
  /// <para>
  /// The occlusion buffer implements shadow caster culling for the main directional light of a
  /// scene. Multiple directional lights with shadows are not supported.
  /// </para>
  /// <para>
  /// Shadow caster culling involves the following tests:
  /// </para>
  /// <list type="number">
  /// <item>
  /// Frustum culling in light space: The shadow caster is tested against the light frustum.
  /// </item>
  /// <item>
  /// Occlusion culling in light space: The shadow caster is tested against the occluders from the
  /// light's point of view.
  /// </item>
  /// <item>
  /// Then the extent of the shadow volume is determined. The actual extent of the shadow volume is
  /// unknown. When "progressive" shadow caster culling is enabled (see property 
  /// <see cref="ProgressiveShadowCasterCulling"/>), the occlusion buffer estimates the extent of
  /// the shadow volume. When "conservative" shadow caster culling is enabled, the occlusion buffer
  /// assumes that shadow volume simply extends to the edge of the light space.
  /// </item>
  /// <item>
  /// Frustum culling camera space: The shadow volume is tested against the camera frustum.
  /// </item>
  /// <item>
  /// Occlusion culling camera space: The shadow volume is tested against the occluders from the
  /// camera's point of view.
  /// </item>
  /// </list>
  /// <para>
  /// "Progressive" shadow caster culling is more aggressive than "conservative" shadow caster
  /// culling, but may cause problems: In some cases it is not possible to estimate the correct
  /// extent of the shadow volume. A shadow caster might be culled, even though its shadow should be
  /// visible. Shadows can start to flicker. In these cases "progressive" shadow caster culling
  /// needs to be disabled. The property <see cref="ProgressiveShadowCasterCulling"/> determines
  /// whether progressive shadow caster culling is active.
  /// </para>
  /// <para>
  /// To perform shadow caster culling the main directional light needs to be passed to the
  /// <see cref="Render(IList{SceneNode},LightNode,RenderContext)"/> method.
  /// </para>
  /// <code lang="csharp" title="Example: Occlusion culling and shadow caster culling">
  /// <![CDATA[
  /// // Render the occluders into the occlusion buffer.
  /// // lightNode is the main directional light that casts shadows.
  /// occlusionBuffer.Render(occluders, lightNode, context);
  /// 
  /// // Perform occlusion culling and shadow caster culling on the
  /// // list of scene nodes.
  /// occlusionBuffer.Query(sceneNodes, context);
  /// ]]>
  /// </code>
  /// <para>
  /// Shadow caster that are culled are internally marked using a flag. The
  /// <see cref="ShadowMapRenderer"/> will automatically skip these scene nodes.
  /// </para>
  /// <para>
  /// <strong>Performance:</strong><br/>
  /// The methods <see cref="Render(IList{SceneNode},RenderContext)"/> and <see cref="Query"/> can
  /// be called multiple times per frame. However, each call has a certain latency. It is therefore
  /// recommended to batch all occluders and scene nodes and call the methods only once per frame.
  /// </para>
  /// <para>
  /// Occlusion culling is preformed on the GPU. The occlusion culling results needs to be read back
  /// from the GPU to the CPU, which may stall the pipeline. Depending on various factors (platform,
  /// timing, GPU load, etc.), the process may take less than a millisecond or up to several
  /// milliseconds. Occlusion culling should only be used when there is an overall performance gain.
  /// This can only be determined by experimentation.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// Occlusion culling is performed on the GPU. The methods
  /// <see cref="Render(IList{SceneNode},RenderContext)"/> and <see cref="Query"/> override the
  /// current render target of the graphics device. The render target and the viewport of the
  /// graphics device are undefined after these methods were executed.
  /// </para>
  /// <para>
  /// The <strong>Visualize</strong> methods render into the current render target and viewport of
  /// the graphics device.
  /// </para>
  /// </remarks>
  public class OcclusionBuffer : IDisposable
  {
    // GPU-based occlusion culling using hierarchical Z buffer (HZB) including shadow
    // caster culling.
    // 
    // References:
    // - Stephen Hill and Daniel Collin: "Practical, Dynamic Visibility for Games",
    //   http://blog.selfshadow.com/publications/practical-visibility/
    // - Hierarchical Z-Buffer Occlusion Culling
    //   http://www.nickdarnell.com/2010/06/hierarchical-z-buffer-occlusion-culling/
    //   http://www.nickdarnell.com/2010/07/hierarchical-z-buffer-occlusion-culling-shadows/
    //
    // Shadow caster culling as described in the articles above has a fundamental 
    // flaw: In certain situation the information in the light HZB is not sufficient
    // to determine the extent of the shadow volume. This problem appears when the
    // shadow caster is/has an occluder:
    // - Case #1: The GPU rasters occluders conservatively, i.e. the occluders may 
    //   be larger than the actual shadow caster. When the shadow caster is tested
    //   against the light HZB, it only samples its own occluder. The extent of 
    //   the shadow volume is too small.
    // - Case #2: The shadow caster has an occluder. The occluder is surrounded by
    //   other occluders in the light HZB. When the shadow caster is tested against
    //   the light HZB, it only samples its own and the surrounding occluders. But
    //   the actual depth behind the shadow caster is not revealed. The extent of 
    //   the shadow volume is too small.
    // 
    // - "Conservative culling": The shadow volume extends to the edge of the light space. Safe.
    // - "Progressive culling": The shadow volume is estimated by sampling the light HZB.
    // 
    // Solutions when "progressive culling" is too aggressive:
    // - Solution #1: Make occluder smaller than shadow caster.
    // - Solution #2: Remove occluder from shadow caster.
    // - Solution #3: Disable "progressive culling".
    //
    // Large objects:
    // - Infinite AABBs are handled by "clamping" the AABB to the camera/light AABB.
    // - Frustum culling is less efficient than using Scene.Query() for large objects.
    //   Reason: The AABB corners are tested against the frustum planes. For large
    //   objects we also need to test the frustum corners against the AABB.
    //   See: http://www.iquilezles.org/www/articles/frustumcorrect/frustumcorrect.htm
    // - Occlusion culling does not work if objects reach behind the camera.
    //   Reason: To avoid back-projections, points behind the camera are mapped to
    //   the near plane. --> Object is always visible.


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    // The AABB of a scene node is tested by making up to n x n depth
    // comparisons in the occlusion buffer.
    private const int AabbCoverage = 4; // n = 4

    // The default width of the render target storing the results.
    private const int ResultsBufferWidth = 256;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // A RenderBatch is used to render occluders.
    private readonly RenderBatch<Vector3F, ushort> _renderBatch;

    // Effect "OcclusionCulling.fx"
    private readonly Effect _effect;
    private readonly EffectParameter _parameterClampAabbMinimum;
    private readonly EffectParameter _parameterClampAabbMaximum;
    private readonly EffectParameter _parameterCameraViewProj;
    private readonly EffectParameter _parameterCameraNear;
    private readonly EffectParameter _parameterCameraFar;
    private readonly EffectParameter _parameterCameraPosition;
    private readonly EffectParameter _parameterNormalizationFactor;
    private readonly EffectParameter _parameterLightViewProj;
    private readonly EffectParameter _parameterLightToCamera;
    private readonly EffectParameter _parameterHzbSize;
    private readonly EffectParameter _parameterTargetSize;
    private readonly EffectParameter _parameterAtlasSize;
    private readonly EffectParameter _parameterTexelOffset;
    private readonly EffectParameter _parameterHalfTexelOffset;
    private readonly EffectParameter _parameterMaxLevel;
    private readonly EffectParameter _parameterHzbTexture;
    private readonly EffectParameter _parameterLightHzbTexture;
    private readonly EffectParameter _parameterDebugLevel;
    private readonly EffectParameter _parameterDebugMinimum;
    private readonly EffectParameter _parameterDebugMaximum;
    private readonly EffectTechnique _techniqueOccluder;
    private readonly EffectTechnique _techniqueDownsample;
    private readonly EffectTechnique _techniqueCopy;
    private readonly EffectTechnique _techniqueQuery;
    private readonly EffectTechnique _techniqueVisualize;

    // Lists of occluders.
    private readonly List<SceneNode> _sceneNodes;
    private readonly List<IOcclusionProxy> _occlusionProxies;

    // IOcclusionProxy can be updated in parallel.
    private readonly Action _updateOcclusionProxies;
    private readonly Action<int> _updateOcclusionProxy;
    private Task _updateTask;

    // AABBs for clamping infinite AABBs.
    private Aabb _cameraAabb;
    private Aabb _lightAabb;

    // Light frustum for rendering light HZB.
    private readonly PerspectiveViewVolume _splitVolume;
    private readonly CameraNode _orthographicCameraNode;
    private readonly Vector3F[] _frustumCorners = new Vector3F[8];
    private bool _lightHzbAvailable;


    // 512 x 256 hierarchical depth buffer (HZB) stored as a 512 x 384 texture atlas.
    // (XNA does not support rendering to a mipmap level. Therefore we need to store
    // the hierarchical depth buffer in a texture atlas.)
    private RenderTarget2D _cameraHzb;
    private RenderTarget2D _lightHzb;

    // For reference: The texture atlas layout.
    //private Rectangle[] Levels =
    //{
    //  new Rectangle(0, 0, 512, 256),
    //  new Rectangle(0, 256, 256, 128),
    //  new Rectangle(256, 256, 128, 64),
    //  new Rectangle(384, 256, 64, 32),
    //  new Rectangle(448, 256, 32, 16),
    //  new Rectangle(480, 256, 16, 8),
    //  new Rectangle(496, 256, 8, 4),
    //  new Rectangle(504, 256, 4, 2),
    //  //new Rectangle(508, 256, 2, 1)   // Only when using 2x2 depth comparisons.
    //};

    // Temporary render targets used during HZB creation and downsampling.
    private RenderTarget2D[] _hzbLevels;

    // Vertex data for submitting query.
    private OcclusionVertex[] _queryData;

    // List of shadow casters to be tested.
    private readonly List<SceneNode> _shadowCasters;

    // The render target into which the results are written.
    private RenderTarget2D _resultsBuffer;

    // Buffer for result readback.
    private float[] _results;
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
    /// Gets or sets a value indicating whether multithreading is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if multithreading is enabled; otherwise, <see langword="false"/>. The
    /// default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When multithreading is enabled, the scene will distribute the workload across multiple
    /// processors (CPU cores) to improve the performance.
    /// </para>
    /// <para>
    /// Multithreading adds an additional overhead, therefore it should only be enabled if the 
    /// current system has more than one CPU core and if the other cores are not fully utilized by
    /// the application. Multithreading should be disabled if the system has only one CPU core or if
    /// all other CPU cores are busy. In some cases it might be necessary to run a benchmark of the
    /// application and compare the performance with and without multithreading to decide whether
    /// multithreading should be enabled or not.
    /// </para>
    /// <para>
    /// The scene internally uses the class <see cref="Parallel"/> for parallelization.
    /// <see cref="Parallel"/> is a static class that defines how many worker threads are created, 
    /// how the workload is distributed among the worker threads and more. (See 
    /// <see cref="Parallel"/> to find out more on how to configure parallelization.)
    /// </para>
    /// </remarks>
    /// <seealso cref="Parallel"/>
    public bool EnableMultithreading { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether progressive shadow caster culling is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if progressive shadow caster culling is enabled; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// See <see cref="OcclusionBuffer">class documentation</see> for more information.
    /// </remarks>
    public bool ProgressiveShadowCasterCulling { get; set; }


    /// <summary>
    /// Gets the occlusion culling statistics.
    /// </summary>
    /// <value>The occlusion culling statistics.</value>
    public OcclusionCullingStatistics Statistics { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="OcclusionBuffer"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="OcclusionBuffer"/> with a default size of 
    /// 512 x 256 and a triangle buffer size of 21845.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public OcclusionBuffer(IGraphicsService graphicsService)
      : this(graphicsService, 512, 256, ushort.MaxValue / 3)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="OcclusionBuffer"/> class with the specified
    /// buffer size.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="width">The width of the occlusion buffer.</param>
    /// <param name="height">The height of the occlusion buffer.</param>
    /// <param name="bufferSize">
    /// The size of the internal triangle buffer (= max number of occluder triangles that can be
    /// rendered in a single draw call). Needs to be large enough to store the most complex 
    /// occluder.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public OcclusionBuffer(IGraphicsService graphicsService, int width, int height, int bufferSize)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      // For simplicity only accept power-of-two formats.
      if (!MathHelper.IsPowerOf2(width) && !MathHelper.IsPowerOf2(height))
        throw new ArgumentException("Width and height of occlusion buffer expected to be a power of two.");

      // The current texture atlas layout assumes that width ≥ height.
      if (width < height)
        throw new ArgumentException("Width expected to be greater than or equal to the height of the occlusion buffer.");

      var graphicsDevice = graphicsService.GraphicsDevice;
      if (bufferSize < 1)
        throw new ArgumentOutOfRangeException("bufferSize", "The buffer size needs to be creater than 1.");
      if (bufferSize >= graphicsDevice.GetMaxPrimitivesPerCall())
        throw new ArgumentOutOfRangeException("bufferSize", "The buffer size exceeds the max number of primitives supported by the current graphics device.");

      // ----- RenderBatch handles occluders.
      // bufferSize is the max number of triangles per draw call.

      // Vertex buffer size:
      // - In the worst case n triangles need n * 3 vertices.
      // - The max size is limited to 65536 because 16-bit indices are used.
      var vertices = new Vector3F[Math.Min(bufferSize * 3, ushort.MaxValue + 1)];

      // Index buffer size: number of triangles * 3
      var indices = new ushort[bufferSize * 3];

      _renderBatch = new RenderBatch<Vector3F, ushort>(
        graphicsDevice,
        VertexPosition.VertexDeclaration,
        vertices, true,
        indices, true);

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/OcclusionCulling");
      _parameterClampAabbMinimum = _effect.Parameters["ClampAabbMinimum"];
      _parameterClampAabbMaximum = _effect.Parameters["ClampAabbMaximum"];
      _parameterCameraViewProj = _effect.Parameters["CameraViewProj"];
      _parameterCameraNear = _effect.Parameters["CameraNear"];
      _parameterCameraFar = _effect.Parameters["CameraFar"];
      _parameterCameraPosition = _effect.Parameters["CameraPosition"];
      _parameterNormalizationFactor = _effect.Parameters["NormalizationFactor"];
      _parameterLightViewProj = _effect.Parameters["LightViewProj"];
      _parameterLightToCamera = _effect.Parameters["LightToCamera"];
      _parameterHzbSize = _effect.Parameters["HzbSize"];
      _parameterTargetSize = _effect.Parameters["TargetSize"];
      _parameterAtlasSize = _effect.Parameters["AtlasSize"];
      _parameterTexelOffset = _effect.Parameters["TexelOffset"];
      _parameterHalfTexelOffset = _effect.Parameters["HalfTexelOffset"];
      _parameterMaxLevel = _effect.Parameters["MaxLevel"];
      _parameterHzbTexture = _effect.Parameters["HzbTexture"];
      _parameterLightHzbTexture = _effect.Parameters["LightHzb"];
      _parameterDebugLevel = _effect.Parameters["DebugLevel"];
      _parameterDebugMinimum = _effect.Parameters["DebugMinimum"];
      _parameterDebugMaximum = _effect.Parameters["DebugMaximum"];
      _techniqueOccluder = _effect.Techniques["Occluder"];
      _techniqueDownsample = _effect.Techniques["Downsample"];
      _techniqueCopy = _effect.Techniques["Copy"];
      _techniqueQuery = _effect.Techniques["Query"];
      _techniqueVisualize = _effect.Techniques["Visualize"];

      _occlusionProxies = new List<IOcclusionProxy>();
      _sceneNodes = new List<SceneNode>();

      // Store delegate methods to avoid garbage.
      _updateOcclusionProxies = UpdateOcclusionProxies;
      _updateOcclusionProxy = UpdateOcclusionProxy;

      _splitVolume = new PerspectiveViewVolume();
      _orthographicCameraNode = new CameraNode(new Camera(new OrthographicProjection()));

      _shadowCasters = new List<SceneNode>();

/*
      // By default, enable multithreading on multi-core systems.
#if WP7 || UNITY
      // Cannot access Environment.ProcessorCount in phone app. (Security issue.)
      EnableMultithreading = false;
#else
      // Enable multithreading by default if the current system has multiple processors.
      EnableMultithreading = Environment.ProcessorCount > 1;

      // Multithreading works but Parallel.For of Xamarin.Android/iOS is very inefficient.
      if (GlobalSettings.PlatformID == PlatformID.Android || GlobalSettings.PlatformID == PlatformID.iOS)
        EnableMultithreading = false;
#endif
*/
      // Disable multithreading by default. Multithreading causes massive lags in the
      // XNA version, but the MonoGame version is not affected!?
      EnableMultithreading = false;

      // For best performance: Enable progressive shadow caster culling.
      ProgressiveShadowCasterCulling = true;

      Statistics = new OcclusionCullingStatistics();

      InitializeBuffers(graphicsDevice, width, height);
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="OcclusionBuffer"/> class.
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
    /// Releases the unmanaged resources used by an instance of the <see cref="OcclusionBuffer"/>
    /// class and optionally releases the managed resources.
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
          _orthographicCameraNode.Dispose(false);
          _renderBatch.Dispose();
          _cameraHzb.Dispose();
          _lightHzb.Dispose();
          foreach (var level in _hzbLevels)
            level.Dispose();
          if (_resultsBuffer != null)
            _resultsBuffer.Dispose();

          // _effect is managed by ContentManager. (Do not dispose.)
        }

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void InitializeBuffers(GraphicsDevice graphicsDevice, int width, int height)
    {
      // The final occlusion buffer is stored in a texture atlas because XNA does
      // not support rendering to individual mipmap levels.
      int atlasWidth = width;
      int atlasHeight = height + height / 2;
      _cameraHzb = new RenderTarget2D(graphicsDevice, atlasWidth, atlasHeight, false, SurfaceFormat.Single, DepthFormat.None);
      _lightHzb = new RenderTarget2D(graphicsDevice, atlasWidth, atlasHeight, false, SurfaceFormat.Single, DepthFormat.None);

      // A chain of render targets is used for rendering and downsampling.
      int numberOfLevels = GetNumberOfLevels(width);
      _hzbLevels = new RenderTarget2D[numberOfLevels];

      // Occluders are rendered into level 0, which requires a depth buffer.
      _hzbLevels[0] = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
      width >>= 1;
      height >>= 1;

      // The remaining levels are used for downsampling.
      for (int i = 1; i < numberOfLevels; i++)
      {
        _hzbLevels[i] = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.None);
        width >>= 1;
        height >>= 1;
      }
    }


    /// <summary>
    /// Gets the number of levels in the depth hierarchy.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <returns>The number of levels in the depth hierarchy.</returns>
    private static int GetNumberOfLevels(int size)
    {
      int numberOfLevels = 1;
      while (size > AabbCoverage)
      {
        size >>= 1;
        numberOfLevels++;
      }

      return numberOfLevels;
    }


    /// <overloads>
    /// <summary>
    /// Clears the occlusion buffer and renders the specified list of occluders.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Clears the occlusion buffer and renders the specified list of occluders.
    /// </summary>
    /// <param name="occluders">The occluders.</param>
    /// <param name="context">The render context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void Render(IList<SceneNode> occluders, RenderContext context)
    {
      Render(occluders, null, null, context);
    }


    /// <summary>
    /// Clears the occlusion buffer and renders the specified list of occluders.
    /// </summary>
    /// <param name="occluders">The occluders.</param>
    /// <param name="lightNode">
    /// The light node that casts directional shadows. Only required when using shadow caster
    /// culling.
    /// </param>
    /// <param name="context">The render context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void Render(IList<SceneNode> occluders, LightNode lightNode, RenderContext context)
    {
      Render(occluders, lightNode, null, context);
    }


    /// <summary>
    /// Clears the occlusion buffer and renders the specified list of occluders.
    /// </summary>
    /// <param name="occluders">The occluders.</param>
    /// <param name="renderer">
    /// A <see cref="SceneNodeRenderer"/> for rendering custom scene nodes into the
    /// occlusion buffer.
    /// </param>
    /// <param name="context">The render context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void Render(IList<SceneNode> occluders, SceneNodeRenderer renderer, RenderContext context)
    {
      Render(occluders, null, renderer, context);
    }


    /// <summary>
    /// Clears the occlusion buffer and renders the specified list of occluders.
    /// </summary>
    /// <param name="occluders">The occluders.</param>
    /// <param name="lightNode">
    /// Optional: The light node that casts directional shadows. Only required when using shadow
    /// caster culling.
    /// </param>
    /// <param name="renderer">
    /// Optional: A <see cref="SceneNodeRenderer"/> for rendering custom scene nodes into the
    /// occlusion buffer.
    /// </param>
    /// <param name="context">The render context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "occluders")]
    public void Render(IList<SceneNode> occluders, LightNode lightNode, SceneNodeRenderer renderer, RenderContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      context.ThrowIfCameraMissing();

      // ----- Sort occluders by type: IOcclusionProxy vs. SceneNode
      SortOccluders(occluders, renderer, context);
      Statistics.Occluders = _occlusionProxies.Count + _sceneNodes.Count;

      // ----- Update all IOcclusionProxy in background.
      if (_occlusionProxies.Count > 0)
      {
        if (EnableMultithreading)
          _updateTask = Parallel.Start(_updateOcclusionProxies);
        else
          UpdateOcclusionProxies();
      }

      // ----- Backup render state.
      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var originalRenderState = new RenderStateSnapshot(graphicsDevice);

      // ----- Camera properties
      var cameraNode = context.CameraNode;
      Matrix cameraView = (Matrix)cameraNode.View;
      var cameraProjection = cameraNode.Camera.Projection;
      Matrix cameraViewProjection = cameraView * cameraProjection;

      if (lightNode == null)
      {
        _lightHzbAvailable = false;
      }
      else
      {
        // ----- Render light HZB.
        _lightHzbAvailable = true;

        var shadow = lightNode.Shadow as CascadedShadow;
        if (shadow == null)
          throw new ArgumentException("LightNode expected to have a CascadedShadow.", "lightNode");

        // Set up orthographic camera similar to CascadedShadowMapRenderer.
        context.CameraNode = _orthographicCameraNode;

        // Part of camera frustum covered by shadow map.
        var maxShadowDistance = shadow.Distances[shadow.NumberOfCascades - 1];
        _splitVolume.SetFieldOfView(cameraProjection.FieldOfViewY, cameraProjection.AspectRatio, cameraProjection.Near, Math.Min(cameraProjection.Far, maxShadowDistance));

        // Find the bounding sphere of the camera frustum.
        Vector3F center;
        float radius;
        GetBoundingSphere(_splitVolume, out center, out radius);

        Matrix33F orientation = lightNode.PoseWorld.Orientation;
        Vector3F lightBackward = orientation.GetColumn(2);
        var orthographicProjection = (OrthographicProjection)_orthographicCameraNode.Camera.Projection;

        // Create a tight orthographic frustum around the cascade's bounding sphere.
        orthographicProjection.SetOffCenter(-radius, radius, -radius, radius, 0, 2 * radius);
        center = cameraNode.PoseWorld.ToWorldPosition(center);
        Vector3F cameraPosition = center + radius * lightBackward;
        Pose frustumPose = new Pose(cameraPosition, orientation);

        // For rendering the shadow map, move near plane back by MinLightDistance 
        // to catch occluders in front of the cascade.
        orthographicProjection.Near = -shadow.MinLightDistance;
        _orthographicCameraNode.PoseWorld = frustumPose;

        Pose lightView = frustumPose.Inverse;
        Matrix lightViewProjection = (Matrix)lightView * orthographicProjection;

        _parameterCameraViewProj.SetValue(lightViewProjection);
        _parameterCameraNear.SetValue(orthographicProjection.Near);
        _parameterCameraFar.SetValue(orthographicProjection.Far);

        RenderOccluders(renderer, context);
        CreateDepthHierarchy(_lightHzb, context);

        // Set effect parameters for use in Query().
        _lightAabb = _orthographicCameraNode.Aabb;
        _parameterLightViewProj.SetValue(lightViewProjection);
        _parameterLightToCamera.SetValue(Matrix.Invert(lightViewProjection) * cameraViewProjection);

        context.CameraNode = cameraNode;
      }

      // ----- Render camera HZB.
      // Set camera parameters. (These effect parameters are also needed in Query()!)
      _cameraAabb = cameraNode.Aabb;
      _parameterCameraViewProj.SetValue(cameraViewProjection);
      _parameterCameraNear.SetValue(cameraProjection.Near);
      _parameterCameraFar.SetValue(cameraProjection.Far);

      var lodCameraNode = context.LodCameraNode;
      if (lodCameraNode != null)
      {
        // Enable distance culling.
        _parameterCameraPosition.SetValue((Vector3)lodCameraNode.PoseWorld.Position);
        float yScale = Math.Abs(lodCameraNode.Camera.Projection.ToMatrix44F().M11);
        _parameterNormalizationFactor.SetValue(1.0f / yScale * cameraNode.LodBias * context.LodBias);
      }
      else
      {
        // Disable distance culling.
        _parameterCameraPosition.SetValue(new Vector3());
        _parameterNormalizationFactor.SetValue(0);
      }

      RenderOccluders(renderer, context);
      CreateDepthHierarchy(_cameraHzb, context);

      _sceneNodes.Clear();
      _occlusionProxies.Clear();

      // Restore render state.
      graphicsDevice.SetRenderTarget(null);
      originalRenderState.Restore();

      context.RenderTarget = originalRenderTarget;
      context.Viewport = originalViewport;
    }


    /// <summary>
    /// Sorts occluders by type (<see cref="IOcclusionProxy"/> vs. <see cref="SceneNode"/>).
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="renderer">The renderer.</param>
    /// <param name="context">The context.</param>
    private void SortOccluders(IList<SceneNode> nodes, SceneNodeRenderer renderer, RenderContext context)
    {
      Debug.Assert(_occlusionProxies.Count == 0, "The list of IOcclusionProxy has not been cleared.");
      Debug.Assert(_sceneNodes.Count == 0, "The list of SceneNodes has not been cleared.");

      if (nodes == null)
        return;

      int numberOfNodes = nodes.Count;
      if (renderer == null)
      {
        // Search only for IOcclusionProxy.
        for (int i = 0; i < numberOfNodes; i++)
        {
          var node = nodes[i];
          if (node == null)
            continue;

          var occlusionProxy = node as IOcclusionProxy;
          if (occlusionProxy != null && occlusionProxy.HasOccluder)
            _occlusionProxies.Add(occlusionProxy);
        }
      }
      else
      {
        // Search for IOcclusionProxy and renderable scene nodes.
        for (int i = 0; i < numberOfNodes; i++)
        {
          var node = nodes[i];
          if (node == null)
            continue;

          var occlusionProxy = node as IOcclusionProxy;
          if (occlusionProxy != null && occlusionProxy.HasOccluder)
            _occlusionProxies.Add(occlusionProxy);
          else if (renderer.CanRender(node, context))
            _sceneNodes.Add(node);
        }
      }
    }


    /// <summary>
    /// Determines whether the specified scene node acts as an occluder during occlusion culling.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="node"/> acts as an occluder; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Scene nodes that act as occluders are rendered into the occlusion buffer when
    /// <see cref="Render(IList{SceneNode},RenderContext)"/> is called. (Note: By passing a
    /// <see cref="SceneNodeRenderer"/> to <see cref="Render(IList{SceneNode},SceneNodeRenderer,RenderContext)"/>
    /// it is possible to render additional scene nodes that are not automatically supported by the
    /// occlusion buffer.)
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool IsOccluder(SceneNode node)
    {
      var occlusionProxy = node as IOcclusionProxy;
      return occlusionProxy != null && occlusionProxy.HasOccluder;
    }


    private void UpdateOcclusionProxies()
    {
      int numberOfOccluders = _occlusionProxies.Count;
      if (EnableMultithreading)
      {
        Parallel.For(0, numberOfOccluders, _updateOcclusionProxy);
      }
      else
      {
        for (int i = 0; i < numberOfOccluders; i++)
          UpdateOcclusionProxy(i);
      }
    }


    private void UpdateOcclusionProxy(int index)
    {
      _occlusionProxies[index].UpdateOccluder();
    }


    private void GetBoundingSphere(ViewVolume viewVolume, out Vector3F center, out float radius)
    {
      float left = viewVolume.Left;
      float top = viewVolume.Top;
      float right = viewVolume.Right;
      float bottom = viewVolume.Bottom;
      float near = viewVolume.Near;
      float far = viewVolume.Far;

      _frustumCorners[0] = new Vector3F(left, top, -near);
      _frustumCorners[1] = new Vector3F(right, top, -near);
      _frustumCorners[2] = new Vector3F(left, bottom, -near);
      _frustumCorners[3] = new Vector3F(right, bottom, -near);

      float farOverNear = far / near;
      left *= farOverNear;
      top *= farOverNear;
      right *= farOverNear;
      bottom *= farOverNear;

      _frustumCorners[4] = new Vector3F(left, top, -far);
      _frustumCorners[5] = new Vector3F(right, top, -far);
      _frustumCorners[6] = new Vector3F(left, bottom, -far);
      _frustumCorners[7] = new Vector3F(right, bottom, -far);

      GeometryHelper.ComputeBoundingSphere(_frustumCorners, out radius, out center);
    }


    /// <summary>
    /// Renders the occluders.
    /// </summary>
    /// <param name="renderer">
    /// Optional: A <see cref="SceneNodeRenderer"/> for rendering custom scene nodes into the
    /// occlusion buffer.
    /// </param>
    /// <param name="context">The context.</param>
    private void RenderOccluders(SceneNodeRenderer renderer, RenderContext context)
    {
      // ----- Clear occlusion buffer.
      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;

      graphicsDevice.SetRenderTarget(_hzbLevels[0]);
      context.RenderTarget = _hzbLevels[0];
      context.Viewport = graphicsDevice.Viewport;
      graphicsDevice.Clear(Color.White);

      // ----- Render scene nodes using custom scene node renderer.
      if (renderer != null && _sceneNodes.Count > 0)
      {
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;
        graphicsDevice.BlendState = BlendState.Opaque;

        renderer.Render(_sceneNodes, context);
      }

      // ----- Render all IOcclusionProxy.
      if (EnableMultithreading)
        _updateTask.Wait();

      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.BlendState = BlendState.Opaque;

      _effect.CurrentTechnique = _techniqueOccluder;
      _techniqueOccluder.Passes[0].Apply();

      // Go through list of occluders and submit triangles to render batch.
      int numberOfOccluders = _occlusionProxies.Count;
      for (int i = 0; i < numberOfOccluders; i++)
      {
        var data = _occlusionProxies[i].GetOccluder();

        int vertexBufferIndex, indexBufferIndex;
        _renderBatch.Submit(PrimitiveType.TriangleList, data.Vertices.Length, data.Indices.Length, out vertexBufferIndex, out indexBufferIndex);

        // Copy triangle vertices.
        data.Vertices.CopyTo(_renderBatch.Vertices, vertexBufferIndex);

        // Adjust and copy triangle indices.
        for (int j = 0; j < data.Indices.Length; j++, indexBufferIndex++)
          _renderBatch.Indices[indexBufferIndex] = (ushort)(vertexBufferIndex + data.Indices[j]);
      }

      _renderBatch.Flush();
    }


    /// <summary>
    /// Creates the depth hierarchy.
    /// </summary>
    /// <param name="hzb">The render target.</param>
    /// <param name="context">The render context.</param>
    private void CreateDepthHierarchy(RenderTarget2D hzb, RenderContext context)
    {
      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;

      // ----- Create hierarchy by successive downsampling.
      _effect.CurrentTechnique = _techniqueDownsample;
      for (int i = 1; i < _hzbLevels.Length; i++)
      {
        var sourceTexture = _hzbLevels[i - 1];
        var targetTexture = _hzbLevels[i];

        // Set new render target. (Unsets sourceTexture. Needs to be called before
        // sourceTexture can be used.)
        graphicsDevice.SetRenderTarget(targetTexture);

        _parameterHzbSize.SetValue(new Vector2(sourceTexture.Width, sourceTexture.Height));
        _parameterTargetSize.SetValue(new Vector2(targetTexture.Width, targetTexture.Height));
        _parameterHalfTexelOffset.SetValue(new Vector2(0.5f / sourceTexture.Width, 0.5f / sourceTexture.Height));
        _parameterHzbTexture.SetValue(sourceTexture);
        _techniqueDownsample.Passes[0].Apply();

        graphicsDevice.DrawFullScreenQuad();
      }

      // ----- Copy hierarchy into texture atlas.
      int bufferWidth = _hzbLevels[0].Width;
      int bufferHeight = _hzbLevels[0].Height;
      graphicsDevice.SetRenderTarget(hzb);
      _parameterTargetSize.SetValue(new Vector2(hzb.Width, hzb.Height));
      _effect.CurrentTechnique = _techniqueCopy;
      for (int i = 0; i < _hzbLevels.Length; i++)
      {
        var level = _hzbLevels[i];
        _parameterHzbTexture.SetValue(level);
        _techniqueCopy.Passes[0].Apply();

        // Get bounds of level in texture atlas.
        var bounds = (i == 0)
                     ? new Rectangle(0, 0, level.Width, level.Height)
                     : new Rectangle(bufferWidth - 2 * level.Width, bufferHeight, level.Width, level.Height);

        graphicsDevice.DrawQuad(bounds);
      }
    }


    /// <summary>
    /// Tests the specified scene nodes against the occlusion buffer to check which scene nodes are
    /// visible. (Performs frustum culling, distance culling, occlusion culling, and shadow caster
    /// culling.)
    /// </summary>
    /// <param name="nodes">
    /// In: The scene nodes that should be tested for visibility.<br/>
    /// Out: The list of visible scene nodes. Occluded scene nodes are replaced with null entries.
    /// </param>
    /// <param name="context">The render context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="nodes"/> or <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public void Query(IList<SceneNode> nodes, RenderContext context)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      context.ThrowIfCameraMissing();

      int numberOfNodes = nodes.Count;  // Note: nodes may contain null entries!
      if (numberOfNodes == 0)
      {
        Statistics.ObjectsTotal = 0;
        Statistics.ObjectsCulled = 0;
        Statistics.ShadowCastersTotal = 0;
        Statistics.ShadowCastersCulled = 0;
        return;
      }

      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var originalRenderState = new RenderStateSnapshot(graphicsDevice);

      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.BlendState = BlendState.Opaque;

      // Note: The camera parameters are set in Render().

      // ----- Query data.
      // The vertices store the AABB and the pixel address to which the results is
      // written. In MonoGame, we can simply submit the data as a point list. However,
      // XNA does not support point lists. As a workaround we can submit the data as a
      // line strip:
      // - The line strip needs to be continuous: Line strips go left-to-right at even
      //   lines and right-to-left at odd lines.
      // - According to DirectX 9 line rasterization rules, the last pixel of a line
      //   is excluded. An additional vertex needs to be appended to ensure that the
      //   last pixel is rendered.
      if (_queryData == null || _queryData.Length < numberOfNodes + 1)
      {
        // We need at least numberOfNodes + 1 vertices. We use NextPowerOf2() which
        // returns a value > numberOfNodes.
        _queryData = new OcclusionVertex[MathHelper.NextPowerOf2((uint)numberOfNodes)];
      }

      Debug.Assert(_shadowCasters.Count == 0, "List of shadow casters has not been cleared.");

      int index = 0;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i];
        if (node == null)
          continue;

        // Ignore empty shapes.
        if (node.Shape is EmptyShape)
        {
          nodes[i] = null;  // Cull scene node!
          continue;
        }

        if (node.CastsShadows)
          _shadowCasters.Add(node);

        // Pixel address
        _queryData[index].Pixel = ToPixelAddress(index);

        // AABB
        var aabb = node.Aabb;
        _queryData[index].Minimum = aabb.Minimum;
        _queryData[index].Maximum = aabb.Maximum;

        // Position, Scale and MaxDistance are used for distance culling.
        _queryData[index].Position = node.PoseWorld.Position;
        _queryData[index].Scale = node.ScaleWorld;
        _queryData[index].MaxDistance = node.MaxDistance;

        index++;
      }

      // Append additional vertex. (For XNA line strips.)
      {
        // Copy last vertex and increment pixel address.
        _queryData[index] = _queryData[index - 1];
        _queryData[index].Pixel = ToPixelAddress(index);
      }

      int actualNumberOfNodes = index;

      // Allocate render target storing the results.
      int numberOfQueries = _lightHzbAvailable ? actualNumberOfNodes + _shadowCasters.Count : actualNumberOfNodes;
      int desiredBufferHeight = (numberOfQueries - 1) / ResultsBufferWidth + 1;
      Debug.Assert(ResultsBufferWidth * desiredBufferHeight >= actualNumberOfNodes, "Sanity check.");
      if (_resultsBuffer == null || _resultsBuffer.Height < desiredBufferHeight)
      {
        if (_resultsBuffer != null)
          _resultsBuffer.Dispose();

        _resultsBuffer = new RenderTarget2D(graphicsDevice, ResultsBufferWidth, desiredBufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
        _results = new float[ResultsBufferWidth * desiredBufferHeight];
      }

      // Set new render target before binding the _cameraHzb.
      graphicsDevice.SetRenderTarget(_resultsBuffer);

      float width = _hzbLevels[0].Width;
      float height = _hzbLevels[0].Height;
      Vector2 sourceSize = new Vector2(width, height);
      Vector2 targetSize = new Vector2(_resultsBuffer.Width, _resultsBuffer.Height);
      Vector2 texelSize = new Vector2(1.0f / _cameraHzb.Width, 1.0f / _cameraHzb.Height);
      Vector2 halfTexelSize = 0.5f * texelSize;

      _parameterClampAabbMinimum.SetValue((Vector3)_cameraAabb.Minimum);
      _parameterClampAabbMaximum.SetValue((Vector3)_cameraAabb.Maximum);
      _parameterHzbSize.SetValue(sourceSize);
      _parameterTargetSize.SetValue(targetSize);
      _parameterAtlasSize.SetValue(new Vector2(_cameraHzb.Width, _cameraHzb.Height));
      _parameterTexelOffset.SetValue(texelSize);
      _parameterHalfTexelOffset.SetValue(halfTexelSize);
      _parameterMaxLevel.SetValue((float)_hzbLevels.Length - 1);
      _parameterHzbTexture.SetValue(_cameraHzb);
      _parameterLightHzbTexture.SetValue(_lightHzb);

      _effect.CurrentTechnique = _techniqueQuery;
      _techniqueQuery.Passes[0].Apply();

#if MONOGAME
      var primitiveType = PrimitiveType.PointList;
#else
      var primitiveType = PrimitiveType.LineStrip;
#endif

      graphicsDevice.DrawUserPrimitives(primitiveType, _queryData, 0, actualNumberOfNodes);

      // Query shadow casters.
      int numberOfShadowCasters = _shadowCasters.Count;
      if (_lightHzbAvailable)
      {
        int offset = actualNumberOfNodes;
        for (int i = 0; i < numberOfShadowCasters; i++)
        {
          var node = _shadowCasters[i];

          // Pixel address
          _queryData[i].Pixel = ToPixelAddress(offset + i);

          // AABB
          var aabb = node.Aabb;
          _queryData[i].Minimum = aabb.Minimum;
          _queryData[i].Maximum = aabb.Maximum;

          // Position, Scale and MaxDistance are used for distance culling.
          _queryData[i].Position = node.PoseWorld.Position;
          _queryData[i].Scale = node.ScaleWorld;
          _queryData[i].MaxDistance = node.MaxDistance;
        }

        // Append additional vertex. (For XNA line strips.)
        {
          // Copy last vertex and increment pixel address.
          _queryData[numberOfShadowCasters] = _queryData[numberOfShadowCasters - 1];
          _queryData[numberOfShadowCasters].Pixel = ToPixelAddress(offset + numberOfShadowCasters);
        }

        _parameterClampAabbMinimum.SetValue((Vector3)_lightAabb.Minimum);
        _parameterClampAabbMaximum.SetValue((Vector3)_lightAabb.Maximum);

        int passIndex = ProgressiveShadowCasterCulling ? 2 : 1;
        _techniqueQuery.Passes[passIndex].Apply();

        graphicsDevice.DrawUserPrimitives(primitiveType, _queryData, 0, numberOfShadowCasters);
      }

      // Read back results.
      graphicsDevice.SetRenderTarget(null);
      _resultsBuffer.GetData(_results);

      index = 0;
      int objectsCulled = 0;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i];
        if (node == null)
          continue;

        int resultIndex = ToResultsBufferIndex(index);

        float viewNormalizedDistance = _results[resultIndex];
        if (viewNormalizedDistance >= 0)
        {
          // Store view-normalized distance in SortTag.
          node.SortTag = viewNormalizedDistance;
        }
        else
        {
          // Scene node culled.
          nodes[i] = null;
          objectsCulled++;
        }

        index++;
      }

      int shadowCastersCulled = 0;
      if (_lightHzbAvailable)
      {
        int offset = actualNumberOfNodes;
        for (int i = 0; i < numberOfShadowCasters; i++)
        {
          var node = _shadowCasters[i];
          int resultIndex = ToResultsBufferIndex(offset + i);

          // ReSharper disable once CompareOfFloatsByEqualityOperator
          float viewNormalizedDistance = _results[resultIndex];
          if (_results[resultIndex] >= 0)
          {
            // Shadow caster is visible.
            node.ClearFlag(SceneNodeFlags.IsShadowCasterCulled);
            node.SortTag = viewNormalizedDistance;
          }
          else
          {
            // Shadow caster is culled.
            node.SetFlag(SceneNodeFlags.IsShadowCasterCulled);
            shadowCastersCulled++;
          }
        }
      }

      Statistics.ObjectsTotal = actualNumberOfNodes;
      Statistics.ObjectsCulled = objectsCulled;
      Statistics.ShadowCastersTotal = _shadowCasters.Count;
      Statistics.ShadowCastersCulled = shadowCastersCulled;

      _shadowCasters.Clear();

      originalRenderState.Restore();

      context.RenderTarget = originalRenderTarget;
      context.Viewport = originalViewport;
    }


    /// <summary>
    /// Get the pixel address at which the result of the query with the given index is written.
    /// </summary>
    /// <param name="index">The index of the query.</param>
    /// <returns>The pixel address (x, y).</returns>
    private static Vector2F ToPixelAddress(int index)
    {
      int x, y;
      ToPixelAddress(index, out x, out y);
      return new Vector2F(x, y);
    }


    /// <summary>
    /// Get the pixel address at which the result of the query with the given index is written.
    /// </summary>
    /// <param name="index">The index of the query.</param>
    /// <param name="x">The x pixel address.</param>
    /// <param name="y">The y pixel address.</param>
    private static void ToPixelAddress(int index, out int x, out int y)
    {
      x = index % ResultsBufferWidth;
      y = index / ResultsBufferWidth;
      if (y % 2 == 1)
      {
        // Odd lines are rastered right-to-left! (Necessary for line strips.)
        x = ResultsBufferWidth - x - 1;
      }
    }


    /// <summary>
    /// Gets the index of the result for a given query.
    /// </summary>
    /// <param name="index">The index of the query.</param>
    /// <returns>The index in the results buffer.</returns>
    private static int ToResultsBufferIndex(int index)
    {
      int x, y;
      ToPixelAddress(index, out x, out y);
      return y * ResultsBufferWidth + x;
    }


    /// <summary>
    /// Resets state of the shadow casters.
    /// </summary>
    /// <param name="nodes">The shadow-casting scene nodes.</param>
    /// <remarks>
    /// <para>
    /// When shadow caster culling is enabled, the method <see cref="Query"/> marks shadow casting
    /// scene nodes as visible or hidden. The shadow map renderer will ignore scene nodes that are
    /// marked as hidden.
    /// </para>
    /// <para>
    /// When shadow caster culling gets disabled, <see cref="ResetShadowCasters"/> should be called
    /// once to reset the state of the shadow casting scene nodes. The method goes through all scene
    /// nodes and marks them as visible.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="nodes"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
    public void ResetShadowCasters(IList<SceneNode> nodes)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");

      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i];
        if (node != null && node.GetFlag(SceneNodeFlags.CastsShadows | SceneNodeFlags.IsShadowCasterCulled))
          node.ClearFlag(SceneNodeFlags.IsShadowCasterCulled);
      }
    }


    /// <summary>
    /// Debugging: Visualizes a level of the camera's hierarchical Z buffer.
    /// </summary>
    /// <param name="level">
    /// The index of the level to visualize where 0 is the most detailed level.
    /// </param>
    /// <remarks>
    /// This method renders a visualization of the occlusion buffer into the current render target
    /// and viewport using the current blend state.
    /// </remarks>
    /// <param name="context">The render context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void VisualizeCameraBuffer(int level, RenderContext context)
    {
      VisualizeBuffer(level, context, 0);
    }


    /// <summary>
    /// Debugging: Visualizes a level of the light's hierarchical Z buffer. (Only valid when shadow
    /// caster culling is used.)
    /// </summary>
    /// <param name="level">
    /// The index of the level to visualize where 0 is the most detailed level.
    /// </param>
    /// <remarks>
    /// This method renders a visualization of the occlusion buffer into the current render target
    /// and viewport using the current blend state.
    /// </remarks>
    /// <param name="context">The render context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void VisualizeLightBuffer(int level, RenderContext context)
    {
      VisualizeBuffer(level, context, 2);
    }


    /// <summary>
    /// Debugging: Visualizes the occlusion query for the specified scene node.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// This method renders a visualization of the occlusion buffer and the coverage of the
    /// specified scene node in the occlusion buffer into the current render target and viewport
    /// using the current blend state.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> or <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void VisualizeObject(SceneNode node, RenderContext context)
    {
      // Clamp infinite AABBs to camera AABB.
      _parameterClampAabbMinimum.SetValue((Vector3)_cameraAabb.Minimum);
      _parameterClampAabbMaximum.SetValue((Vector3)_cameraAabb.Maximum);

      VisualizeQuery(node, context, 1);
    }


    /// <summary>
    /// Debugging: Visualizes the occlusion query for the specified shadow caster.
    /// </summary>
    /// <param name="node">The shadow caster.</param>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// This method renders a visualization of the occlusion buffer and the coverage of the
    /// specified shadow caster in the occlusion buffer into the current render target and viewport
    /// using the current blend state.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> or <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void VisualizeShadowCaster(SceneNode node, RenderContext context)
    {
      // Clamp infinite AABBs to light AABB.
      _parameterClampAabbMinimum.SetValue((Vector3)_lightAabb.Minimum);
      _parameterClampAabbMaximum.SetValue((Vector3)_lightAabb.Maximum);

      VisualizeQuery(node, context, 3);
    }


    /// <summary>
    /// Debugging: Visualizes the occlusion query for the specified shadow volume.
    /// </summary>
    /// <param name="node">The shadow caster.</param>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// This method renders a visualization of the occlusion buffer and the coverage of the
    /// specified shadow volume in the occlusion buffer into the current render target and viewport
    /// using the current blend state.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> or <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void VisualizeShadowVolume(SceneNode node, RenderContext context)
    {
      // Clamp infinite AABBs to light AABB.
      _parameterClampAabbMinimum.SetValue((Vector3)_lightAabb.Minimum);
      _parameterClampAabbMaximum.SetValue((Vector3)_lightAabb.Maximum);

      VisualizeQuery(node, context, 4);
    }


    private void VisualizeBuffer(int level, RenderContext context, int passIndex)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var originalRenderState = new RenderStateSnapshot(graphicsDevice);

      var viewport = graphicsDevice.Viewport;
      _parameterTargetSize.SetValue(new Vector2(viewport.Width, viewport.Height));
      _parameterDebugLevel.SetValue((float)level);
      _effect.CurrentTechnique = _techniqueVisualize;
      _techniqueVisualize.Passes[passIndex].Apply();

      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      // Do not override blend state: Use current blend state.

      graphicsDevice.DrawFullScreenQuad();

      originalRenderState.Restore();
    }


    private void VisualizeQuery(SceneNode node, RenderContext context, int passIndex)
    {
      if (node == null)
        throw new ArgumentNullException("node");
      if (context == null)
        throw new ArgumentNullException("context");

      context.ThrowIfCameraMissing();

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var originalRenderState = new RenderStateSnapshot(graphicsDevice);

      var viewport = graphicsDevice.Viewport;
      _parameterTargetSize.SetValue(new Vector2(viewport.Width, viewport.Height));

      var aabb = node.Aabb;
      _parameterDebugMinimum.SetValue((Vector3)aabb.Minimum);
      _parameterDebugMaximum.SetValue((Vector3)aabb.Maximum);

      _effect.CurrentTechnique = _techniqueVisualize;
      _techniqueVisualize.Passes[passIndex].Apply();

      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      // Do not override blend state: Use current blend state.

      graphicsDevice.DrawFullScreenQuad();

      originalRenderState.Restore();
    }
    #endregion
  }
}
#endif
