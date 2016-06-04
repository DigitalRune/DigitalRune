using System;
using System.Collections.Generic;
using DigitalRune;
using DigitalRune.Collections;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Graphics
{
  /// <summary>
  /// Renders an image containing the intersections of meshes.
  /// </summary>
  /// <remarks>
  /// The rendering is done in two steps. The method <see cref="ComputeIntersection"/> renders the
  /// intersection volumes into internal offscreen render targets (color + depth) using depth
  /// peeling. The method <see cref="RenderIntersection"/> combines the intersection image with the
  /// current render targets.
  /// </remarks>
  public sealed class IntersectionRenderer : IDisposable
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// A default depth/stencil state object for rendering stencil volumes using the single pass
    /// Z-fail algorithm (Carmack's Reverse).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    private static readonly DepthStencilState DepthStencilStateOnePassStencilFail = new DepthStencilState
    {
      Name = "IntersectionRenderer.DepthStencilStateOnePassStencilFail",
      DepthBufferEnable = true,
      DepthBufferFunction = CompareFunction.LessEqual,
      DepthBufferWriteEnable = false,

      StencilEnable = true,
      TwoSidedStencilMode = true,
      ReferenceStencil = 0,
      StencilMask = ~0,
      StencilWriteMask = ~0,

      StencilFunction = CompareFunction.Always,
      StencilFail = StencilOperation.Keep,
      StencilDepthBufferFail = StencilOperation.Decrement,
      StencilPass = StencilOperation.Keep,

      CounterClockwiseStencilFunction = CompareFunction.Always,
      CounterClockwiseStencilFail = StencilOperation.Keep,
      CounterClockwiseStencilDepthBufferFail = StencilOperation.Increment,
      CounterClockwiseStencilPass = StencilOperation.Keep,
    };


    /// <summary>
    /// A blend state where all render targets are disabled.
    /// </summary>
    private readonly BlendState BlendStateNoWrite = new BlendState
    {
      Name = "IntersectionRenderer.BlendStateNoWrite",
      ColorWriteChannels = ColorWriteChannels.None,
      ColorWriteChannels1 = ColorWriteChannels.None,
      ColorWriteChannels2 = ColorWriteChannels.None,
      ColorWriteChannels3 = ColorWriteChannels.None,
    };


    /// <summary>
    /// A default depth/stencil state object for rendering where stencil ≠ 0 and also resetting the
    /// stencil.
    /// </summary>
    private static readonly DepthStencilState DepthStencilStateStencilNotEqual0 = new DepthStencilState
    {
      Name = "IntersectionRenderer.DepthStencilStateStencilNotEqual0",
      DepthBufferEnable = true,
      DepthBufferWriteEnable = true,
      DepthBufferFunction = CompareFunction.Less,

      StencilEnable = true,
      TwoSidedStencilMode = false,
      ReferenceStencil = 0,
      StencilMask = ~0,
      StencilWriteMask = ~0,

      StencilFunction = CompareFunction.NotEqual,
      StencilFail = StencilOperation.Keep,
      StencilDepthBufferFail = StencilOperation.Keep,
      StencilPass = StencilOperation.Keep,
    };


    /// <summary>
    /// A default depth/stencil state object for "normal" rendering using depth compare function
    /// "Less".
    /// </summary>
    private static readonly DepthStencilState DepthStencilStateWriteLess = new DepthStencilState
    {
      Name = "IntersectionRenderer.DepthStencilStateWriteLess",
      DepthBufferEnable = true,
      DepthBufferWriteEnable = true,
      DepthBufferFunction = CompareFunction.Less,
    };


    /// <summary>
    /// A default rasterizer state with no culling and enabled scissor test.
    /// </summary>
    private static readonly RasterizerState CullNoneScissor = new RasterizerState
    {
      Name = "IntersectionRenderer.CullNoneScissor",
      CullMode = CullMode.None,
      ScissorTestEnable = true,
    };


    /// <summary>
    /// A default rasterizer state with counter-clockwise culling and enabled scissor test.
    /// </summary>
    private static readonly RasterizerState CullCounterClockwiseScissor = new RasterizerState
    {
      Name = "IntersectionRenderer.CullCounterClockwiseScissor",
      CullMode = CullMode.CullCounterClockwiseFace,
      ScissorTestEnable = true,
    };
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IGraphicsService _graphicsService;

    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterCameraParameters;
    private readonly EffectParameter _parameterWorld;
    private readonly EffectParameter _parameterView;
    private readonly EffectParameter _parameterProjection;
    private readonly EffectParameter _parameterColor;
    private readonly EffectParameter _parameterTexture;
    private readonly EffectPass _passPeel;
    private readonly EffectPass _passMark;
    private readonly EffectPass _passDraw;
    private readonly EffectPass _passCombine;
    private readonly EffectPass _passRender;

    // These lists are fields to avoid allocations.
    private readonly List<Pair<MeshNode, MeshNode>> _pairs = new List<Pair<MeshNode, MeshNode>>();
    private readonly List<MeshNode> _partners = new List<MeshNode>();

    private RenderTarget2D _intersectionImage;
    private Vector3F _color;
    private float _alpha;
    private Rectangle _totalScissorRectangle;

    private bool _isDisposed;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the downsample factor.
    /// </summary>
    /// <value>
    /// The downsample factor. The default value is 1.
    /// </value>
    /// <remarks>
    /// Use a <see cref="DownsampleFactor"/> greater than 1 to reduce the resolution of the
    /// intersection image to improve performance.
    /// </remarks>
    public float DownsampleFactor { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the intersection renderer uses the scissor test
    /// to improve performance.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the scissor test is enabled; otherwise, <see langword="false" />.
    /// The default value is <see langword="true" />.
    /// </value>
    public bool EnableScissorTest { get; set; }


    public bool Dummy { get; set; }    // TODO: This is only for debugging and will be removed.
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="IntersectionRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="content">The content manager.</param>
    public IntersectionRenderer(IGraphicsService graphicsService, ContentManager content)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");
      if (content == null)
        throw new ArgumentNullException("content");

      _graphicsService = graphicsService;

      Effect effect = content.Load<Effect>("Intersection");
      _parameterViewportSize = effect.Parameters["ViewportSize"];
      _parameterCameraParameters = effect.Parameters["CameraParameters"];
      _parameterWorld = effect.Parameters["World"];
      _parameterView = effect.Parameters["View"];
      _parameterProjection = effect.Parameters["Projection"];
      _parameterColor = effect.Parameters["Color"];
      _parameterTexture = effect.Parameters["Texture0"];
      _passPeel = effect.CurrentTechnique.Passes["Peel"];
      _passMark = effect.CurrentTechnique.Passes["Mark"];
      _passDraw = effect.CurrentTechnique.Passes["Draw"];
      _passCombine = effect.CurrentTechnique.Passes["Combine"];
      _passRender = effect.CurrentTechnique.Passes["Render"];

      DownsampleFactor = 1;
      EnableScissorTest = true;
    }


    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting
    /// unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      if (!_isDisposed)
      {
        _isDisposed = true;

        _intersectionImage.SafeDispose();
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Computes the intersection of <see cref="MeshNode"/>s.
    /// </summary>
    /// <param name="meshNodePairs">
    /// A collection of <see cref="MeshNode"/> pairs.The renderer computes the intersection volume 
    /// of each pair.
    /// </param>
    /// <param name="color">The diffuse color used for the intersection.</param>
    /// <param name="alpha">The opacity of the intersection.</param>
    /// <param name="maxConvexity">
    /// The maximum convexity of the submeshes. A convex mesh has a convexity of 1. A concave mesh
    /// has a convexity greater than 1. Convexity is the number of layers required for depth peeling 
    /// (= the number of front face layers when looking at the object).
    /// </param>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <para>
    /// This method renders an off-screen image (color and depth) of the intersection volume. This 
    /// operation destroys the currently set render target and depth/stencil buffer.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="IntersectionRenderer"/> has already been disposed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="meshNodePairs"/> or <see cref="context"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The convexity must be greater than 0.
    /// </exception>
    /// <exception cref="GraphicsException">
    /// Invalid render context: Graphics service is not set.
    /// </exception>
    /// <exception cref="GraphicsException">
    /// Invalid render context: Wrong graphics device.
    /// </exception>
    /// <exception cref="GraphicsException">
    /// Invalid render context: Scene is not set.
    /// </exception>
    /// <exception cref="GraphicsException">
    /// Invalid render context: Camera node needs to be set in render context.
    /// </exception>
    public void ComputeIntersection(IEnumerable<Pair<MeshNode>> meshNodePairs,
      Vector3F color, float alpha, float maxConvexity, RenderContext context)
    {
      if (_isDisposed)
        throw new ObjectDisposedException("IntersectionRenderer has already been disposed.");
      if (meshNodePairs == null)
        throw new ArgumentNullException("meshNodePairs");
      if (maxConvexity < 1)
        throw new ArgumentOutOfRangeException("maxConvexity", "The max convexity must be greater than 0.");
      if (context == null)
        throw new ArgumentNullException("context");
      if (context.GraphicsService == null)
        throw new GraphicsException("Invalid render context: Graphics service is not set.");
      if (_graphicsService != context.GraphicsService)
        throw new GraphicsException("Invalid render context: Wrong graphics service.");
      if (context.CameraNode == null)
        throw new GraphicsException("Camera node needs to be set in render context.");
      if (context.Scene == null)
        throw new GraphicsException("A scene needs to be set in the render context.");

      // Create 2 ordered pairs for each unordered pair.
      _pairs.Clear();
      foreach (var pair in meshNodePairs)
      {
        if (pair.First == null || pair.Second == null)
          continue;

        // Frustum culling.
        if (!context.Scene.HaveContact(pair.First, context.CameraNode))
          continue;
        if (!context.Scene.HaveContact(pair.Second, context.CameraNode))
          continue;

        _pairs.Add(new Pair<MeshNode, MeshNode>(pair.First, pair.Second));
        _pairs.Add(new Pair<MeshNode, MeshNode>(pair.Second, pair.First));
      }
      
      var renderTargetPool = _graphicsService.RenderTargetPool;

      if (_pairs.Count == 0)
      {
        renderTargetPool.Recycle(_intersectionImage);
        _intersectionImage = null;
        return;
      }

      // Color and alpha are applied in RenderIntersection().
      _color = color;
      _alpha = alpha;

      var graphicsDevice = _graphicsService.GraphicsDevice;

      // Save original render states.
      var originalBlendState = graphicsDevice.BlendState;
      var originalDepthStencilState = graphicsDevice.DepthStencilState;
      var originalRasterizerState = graphicsDevice.RasterizerState;
      var originalScissorRectangle = graphicsDevice.ScissorRectangle;

      // Get offscreen render targets.
      var viewport = context.Viewport;
      viewport.X = 0;
      viewport.Y = 0;
      viewport.Width = (int)(viewport.Width / DownsampleFactor);
      viewport.Height = (int)(viewport.Height / DownsampleFactor);
      var renderTargetFormat = new RenderTargetFormat(viewport.Width, viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

      // Try to reuse any existing render targets.
      // (Usually they are recycled in RenderIntersection()).
      var currentScene = _intersectionImage;
      if (currentScene == null || !renderTargetFormat.IsCompatibleWith(currentScene))
      {
        currentScene.SafeDispose();
        currentScene = renderTargetPool.Obtain2D(renderTargetFormat);
      }
      var lastScene = renderTargetPool.Obtain2D(renderTargetFormat);

      // Set shared effect parameters.
      var cameraNode = context.CameraNode;
      var view = (Matrix)cameraNode.View;
      var projection = cameraNode.Camera.Projection;
      var near = projection.Near;
      var far = projection.Far;
      _parameterViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));

      // The DepthEpsilon has to be tuned if depth peeling does not work because
      // of numerical problems equality z comparisons.
      _parameterCameraParameters.SetValue(new Vector3(near, far - near, 0.0000001f));
      _parameterView.SetValue(view);
      _parameterProjection.SetValue((Matrix)projection);

      var defaultTexture = _graphicsService.GetDefaultTexture2DBlack();

      // Handle all pairs.
      bool isFirstPass = true;
      while (true)
      {
        // Find a mesh node A and all mesh nodes to which it needs to be clipped.
        MeshNode meshNodeA = null;
        _partners.Clear();
        for (int i = 0; i < _pairs.Count; i++)
        {
          var pair = _pairs[i];

          if (pair.First == null)
            continue;

          if (meshNodeA == null)
            meshNodeA = pair.First;

          if (pair.First == meshNodeA)
          {
            _partners.Add(pair.Second);

            //  Remove this pair.
            _pairs[i] = new Pair<MeshNode, MeshNode>();
          }
        }

        // Abort if we have handled all pairs.
        if (meshNodeA == null)
          break;

        var worldTransformA = (Matrix)(meshNodeA.PoseWorld * Matrix44F.CreateScale(meshNodeA.ScaleWorld));

        if (EnableScissorTest)
        {
          // Scissor rectangle of A.
          var scissorA = GraphicsHelper.GetScissorRectangle(context.CameraNode, viewport, meshNodeA);

          // Union of scissor rectangles of partners.
          Rectangle partnerRectangle = GraphicsHelper.GetScissorRectangle(context.CameraNode, viewport, _partners[0]);
          for (int i = 1; i < _partners.Count; i++)
          {
            var a = GraphicsHelper.GetScissorRectangle(context.CameraNode, viewport, _partners[i]);
            partnerRectangle = Rectangle.Union(partnerRectangle, a);
          }

          // Use intersection of A and partners.
          graphicsDevice.ScissorRectangle = Rectangle.Intersect(scissorA, partnerRectangle);
          
          // We store the union of all scissor rectangles for use in RenderIntersection().
          if (isFirstPass)
            _totalScissorRectangle = graphicsDevice.ScissorRectangle;
          else
            _totalScissorRectangle = Rectangle.Union(_totalScissorRectangle, graphicsDevice.ScissorRectangle);
        }

        // Depth peeling of A.
        for (int layer = 0; layer < maxConvexity; layer++)
        {
          // Set and clear render target.
          graphicsDevice.SetRenderTarget(currentScene);
          graphicsDevice.Clear(new Color(1, 1, 1, 0));  // RGB = "a large depth", A = "empty area"

          // Render a depth layer of A.
          graphicsDevice.DepthStencilState = DepthStencilStateWriteLess;
          graphicsDevice.BlendState = BlendState.Opaque;
          graphicsDevice.RasterizerState = EnableScissorTest ? CullCounterClockwiseScissor : RasterizerState.CullCounterClockwise;
          _parameterWorld.SetValue(worldTransformA);
          _parameterTexture.SetValue((layer == 0) ? defaultTexture : lastScene);
          _passPeel.Apply();
          foreach (var submesh in meshNodeA.Mesh.Submeshes)
            submesh.Draw();

          // Render partners to set stencil.
          graphicsDevice.DepthStencilState = DepthStencilStateOnePassStencilFail;
          graphicsDevice.BlendState = BlendStateNoWrite;
          graphicsDevice.RasterizerState = EnableScissorTest ? CullNoneScissor : RasterizerState.CullNone;
          foreach (var partner in _partners)
          {
            _parameterWorld.SetValue((Matrix)(partner.PoseWorld * Matrix44F.CreateScale(partner.ScaleWorld)));
            _passMark.Apply();
            foreach (var submesh in partner.Mesh.Submeshes)
              submesh.Draw();
          }

          // Clear depth buffer. Leave stencil buffer unchanged.
          graphicsDevice.Clear(ClearOptions.DepthBuffer, new Color(0, 1, 0), 1, 0);

          // Render A to compute lighting.
          graphicsDevice.DepthStencilState = DepthStencilStateStencilNotEqual0;
          graphicsDevice.BlendState = BlendState.Opaque;
          graphicsDevice.RasterizerState = EnableScissorTest ? CullCounterClockwiseScissor :  RasterizerState.CullCounterClockwise;
          _parameterWorld.SetValue(worldTransformA);
          _passDraw.Apply();
          foreach (var submesh in meshNodeA.Mesh.Submeshes)
            submesh.Draw();

          // Combine last intersection image with current.
          if (!isFirstPass)
          {
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.RasterizerState = EnableScissorTest ? CullNoneScissor : RasterizerState.CullNone;
            _parameterTexture.SetValue(lastScene);
            _passCombine.Apply();
            graphicsDevice.DrawFullScreenQuad();
          }

          isFirstPass = false;

          // ----- Swap render targets.
          MathHelper.Swap(ref lastScene, ref currentScene);
        }
      }

      // Store final images for RenderIntersection.
      _intersectionImage = lastScene;

      // Scale scissor rectangle back to full-screen resolution.
      if (DownsampleFactor > 1)
      {
        _totalScissorRectangle.X = (int)(_totalScissorRectangle.X * DownsampleFactor);
        _totalScissorRectangle.Y = (int)(_totalScissorRectangle.Y * DownsampleFactor);
        _totalScissorRectangle.Width = (int)(_totalScissorRectangle.Width * DownsampleFactor);
        _totalScissorRectangle.Height = (int)(_totalScissorRectangle.Height * DownsampleFactor);
      }


      // Restore original render state.
      graphicsDevice.BlendState = originalBlendState ?? BlendState.Opaque;
      graphicsDevice.DepthStencilState = originalDepthStencilState ?? DepthStencilState.Default;
      graphicsDevice.RasterizerState = originalRasterizerState ?? RasterizerState.CullCounterClockwise;
      graphicsDevice.ScissorRectangle = originalScissorRectangle;

      renderTargetPool.Recycle(currentScene);
      _partners.Clear();
      _pairs.Clear();
    }


    /// <summary>
    /// Renders the intersection created in <see cref="ComputeIntersection" /> into the the current
    /// render target.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <exception cref="ObjectDisposedException">
    /// This <see cref="IntersectionRenderer" /> has already been disposed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method uses the currently set render states, i.e. the caller can decide to render the
    /// intersection with depth test or alpha-blending.
    /// </para>
    /// <para>
    /// If <see cref="ComputeIntersection"/> was not called, <see cref="RenderIntersection"/> does
    /// nothing.
    /// </para>
    /// </remarks>
    public void RenderIntersection(RenderContext context)
    {
      if (_isDisposed)
        throw new ObjectDisposedException("IntersectionRenderer has already been disposed.");
      if (context == null)
        throw new ArgumentNullException("context");

      if (_intersectionImage == null)
        return;

      var graphicsDevice = _graphicsService.GraphicsDevice;

      // Save original render state.
      var originalRasterizerState = graphicsDevice.RasterizerState;
      var originalScissorRectangle = graphicsDevice.ScissorRectangle;

      if (EnableScissorTest)
      {
        graphicsDevice.RasterizerState = CullNoneScissor;
        graphicsDevice.ScissorRectangle = _totalScissorRectangle;
      }
      else
      {
        graphicsDevice.RasterizerState = RasterizerState.CullNone;
      }

      // Combine intersection image with current target.
      var viewport = context.Viewport;
      _parameterViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
      _parameterColor.SetValue(new Vector4(_color.X * _alpha, _color.Y * _alpha, _color.Z * _alpha, _alpha));
      _parameterTexture.SetValue(_intersectionImage);
      _passRender.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // Recycle intersection image.
      // (Note: Recycling is a bit faster than disposing and recreating as long 
      // as the resolution is the same each frame.)
      var renderTargetPool = _graphicsService.RenderTargetPool;
      renderTargetPool.Recycle(_intersectionImage);
      _intersectionImage = null;

      // Restore original render state.
      graphicsDevice.RasterizerState = originalRasterizerState ?? RasterizerState.CullCounterClockwise;
      graphicsDevice.ScissorRectangle = originalScissorRectangle;
    }
    #endregion
  }
}
