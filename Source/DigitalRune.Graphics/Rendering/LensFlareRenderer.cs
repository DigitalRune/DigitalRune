// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="LensFlareNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The lens flare renderer performs hardware occlusion queries to determine the visibility of the 
  /// lens flares. The query results are delayed by one or more frames, which means that lens flares
  /// need at least two frames to become visible.
  /// </para>
  /// <para>
  /// Lens flares need to be rendered in two passes:
  /// </para>
  /// <list type="number">
  /// <item>
  /// <term>Occlusion Queries</term>
  /// <description>
  /// <para>
  /// The method <see cref="UpdateOcclusion(IList{SceneNode},RenderContext)"/> 
  /// needs to be called after the scene is rendered. The Z-buffer of the current render target 
  /// needs to contain the depth information of the scene. The method performs a hardware occlusion 
  /// query to check the visibility of the light source. In a deferred lighting renderer the method 
  /// can be called at the end of the "Material" pass. No pixels are rendered into the current 
  /// render target during the occlusion queries.
  /// </para>
  /// <para>
  /// Hardware occlusion queries only run in HiDef profile. The method has no effect when run in 
  /// Reach profile.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term>Render Pass</term>
  /// <description>
  /// <para>
  /// The method <seealso cref="Render"/> needs to be called to render the lens flares into the 
  /// current render target. The method can be called at any point after the occlusion queries.
  /// </para>
  /// </description>
  /// </item>
  /// </list>
  /// <para>
  /// <strong>Reach Profile:</strong> The <see cref="LensFlareRenderer"/> does not determine the 
  /// visibility of the lens flares because hardware occlusion queries are not supported in Reach 
  /// profile. Lens flares in front of the camera are always visible. As a workaround: Create a new
  /// class derived from <see cref="LensFlare"/> and override 
  /// <see cref="LensFlare.OnGetSizeAndIntensity"/>. In this method determine the visibility of the
  /// light source, for example, by using a ray-test.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  public class LensFlareRenderer : SceneNodeRenderer
  {
    // Alternatives to hardware occlusion queries:
    // - Make ray tests using collision detection.
    // - Read back the back buffer or z-buffer and test sun color or depth on CPU.
    // - "Texture Masking for Faster Lens Flare", Game Programming Gems 2, pp. 474

    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Additional view-dependent data stored per camera node.
    /// </summary>
    private sealed class OcclusionData : IDisposable
    {
      public OcclusionQuery OcclusionQuery; // The hardware occlusion query.
      public int TotalPixels;   // The total number of pixels (estimated screen area) rendered in the occlusion query.
      public int VisiblePixels; // The number of visible pixels.

      public void Dispose()
      {
        if (OcclusionQuery != null)
        {
          OcclusionQuery.Dispose();
          OcclusionQuery = null;
          TotalPixels = 0;
          VisiblePixels = 0;
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Lens flares below these thresholds are ignored.
    private const int MinPixelSize = 1;
    private const float MinIntensity = 0.01f;

    private readonly SceneNode[] _oneNodeArray = new SceneNode[1];

    // A screen-aligned quad (4 vertices) is rendered per occlusion query.
    // The geometry is rendered using a BasicEffect.
    private readonly BasicEffect _basicEffect;
    private readonly VertexPositionColor[] _queryGeometry;

    // The lens flares are rendered using a SpriteBatch with additive blending.
    private readonly SpriteBatch _spriteBatch;

    // Use custom effect in HiDef.
    private readonly Effect _effect;
    private readonly EffectTechnique _techniqueLinear;
    private readonly EffectTechnique _techniqueGamma;
    private readonly EffectParameter _transformParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="LensFlareRenderer" /> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="LensFlareRenderer" /> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public LensFlareRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      Order = 4;

      var graphicsDevice = graphicsService.GraphicsDevice;
      _basicEffect = new BasicEffect(graphicsDevice)
      {
        FogEnabled = false,
        LightingEnabled = false,
        TextureEnabled = false,
        VertexColorEnabled = true,
      };
      _queryGeometry = new VertexPositionColor[4];

      _spriteBatch = graphicsService.GetSpriteBatch();

      if (graphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
      {
        // Use custom effect with sRGB reads in pixel shader.
        try
        {
          _effect = graphicsService.Content.Load<Effect>("DigitalRune/SpriteEffect");
          _transformParameter = _effect.Parameters["Transform"];
          _techniqueLinear = _effect.Techniques["Sprite"];
          _techniqueGamma = _effect.Techniques["SpriteWithGamma"];
        }
        catch (ContentLoadException)
        {
          // If we cannot load the HiDef effect, fall back to Reach. This happens in the Linux
          // build when it runs in Windows.
          _effect = null;
        }
      }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LensFlareRenderer" /> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="spriteBatch">
    /// The sprite batch used for rendering. Can be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    [Obsolete("It is no longer necessary to specify a SpriteBatch.")]
    public LensFlareRenderer(IGraphicsService graphicsService, SpriteBatch spriteBatch)
      : this(graphicsService)
    {
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
          _basicEffect.Dispose();

          // Note: Do not dispose _effect in HiDef profile. The effect is managed by
          // the ContentManager and may be shared.
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
      return node is LensFlareNode;
    }


    /// <summary>
    /// Performs occlusion queries to determine the visibility of the lens flares effects.
    /// (Requires HiDef profile.)
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <param name="context">
    /// The render context. (The property <see cref="RenderContext.CameraNode"/> selects the 
    /// currently active camera.)
    /// </param>
    /// <inheritdoc cref="UpdateOcclusion(IList{SceneNode},RenderContext)"/>
    public void UpdateOcclusion(SceneNode node, RenderContext context)
    {
      ThrowIfDisposed();

      if (node == null)
        return;

      _oneNodeArray[0] = node;
      UpdateOcclusion(_oneNodeArray, context);
      _oneNodeArray[0] = null;
    }


    /// <summary>
    /// Performs occlusion queries to determine the intensity of the lens flares effects.
    /// (Requires HiDef profile.)
    /// </summary>
    /// <param name="nodes">The scene nodes. The list may contain null entries.</param>
    /// <param name="context">
    /// The render context. (The property <see cref="RenderContext.CameraNode"/> selects the 
    /// currently active camera.)
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="nodes"/> or <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public void UpdateOcclusion(IList<SceneNode> nodes, RenderContext context)
    {
      // Measures the visibility of the light source by drawing a screen-aligned quad
      // using an occlusion query. The query result is used in the next frame.

      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      context.Validate(_basicEffect);
      context.ThrowIfCameraMissing();

      // Occlusion queries require HiDef profile.
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        return;

      int numberOfNodes = nodes.Count;
      if (nodes.Count == 0)
        return;

      // Camera properties
      var cameraNode = context.CameraNode;
      var cameraPose = cameraNode.PoseWorld;
      Vector3F cameraRight = cameraPose.Orientation.GetColumn(0);    // 1st column vector
      Vector3F cameraUp = cameraPose.Orientation.GetColumn(1);       // 2nd column vector
      Vector3F cameraForward = -cameraPose.Orientation.GetColumn(2); // 3rd column vector (negated)
      Matrix44F view = cameraNode.View;
      Matrix44F projection = cameraNode.Camera.Projection;
      bool isOrthographic = (projection.M33 != 0);

      // The following factors are used to estimate the size of a quad in screen space.
      // (The equation is described in GraphicsHelper.GetScreenSize().)
      float xScale = Math.Abs(projection.M00 / 2);
      float yScale = Math.Abs(projection.M11 / 2);

      var viewport = graphicsDevice.Viewport;

      // Lens flares of directional lights are rendered directly on the screen.
      // --> Set up projection transformation for rendering in screen space.
      var orthographicProjection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);

      // Set render states for rendering occlusion query geometry (quad).
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.BlendState = GraphicsHelper.BlendStateNoColorWrite;
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as LensFlareNode;
        if (node == null)
          continue;

        object dummy;
        cameraNode.ViewDependentData.TryGetValue(node, out dummy);
        var renderData = dummy as OcclusionData;
        if (renderData == null)
        {
          renderData = new OcclusionData();
          cameraNode.ViewDependentData[node] = renderData;
        }

        if (renderData.OcclusionQuery != null)
        {
          // Wait until previous occlusion query has completed.
          if (!renderData.OcclusionQuery.IsComplete)
            continue;

          // ----- Read result of previous occlusion query.
          int visiblePixels = renderData.OcclusionQuery.PixelCount;

          // OcclusionData.TotalPixels is only an approximation.
          // --> Clamp pixel count to [0, TotalPixels].
          if (visiblePixels > renderData.TotalPixels)
            visiblePixels = renderData.TotalPixels;

          renderData.VisiblePixels = visiblePixels;
        }

        // ----- Run new occlusion query.
        var lensFlare = node.LensFlare;

        // The user can disable the lens flare by setting LensFlare.Intensity to 0.
        float intensity = node.Intensity * lensFlare.Intensity;
        if (intensity < MinIntensity)
        {
          renderData.VisiblePixels = 0;
          continue;
        }

        float querySize;
        if (lensFlare.IsDirectional)
        {
          // ----- Directional lights

          // Ignore directional lights if camera has orthographic projection.
          // (The light source is infinitely far way and the camera frustum has only 
          // limited width and height. It is very unlikely that camera catches the 
          // directional light.
          if (isOrthographic)
          {
            renderData.VisiblePixels = 0;
            continue;
          }

          // Directional lights are positioned at infinite distance and are not affected
          // by the position of the camera.
          Vector3F lightDirectionWorld = -node.PoseWorld.Orientation.GetColumn(2);  // 3rd column vector (negated)
          Vector3F lightDirectionView = cameraPose.ToLocalDirection(lightDirectionWorld);
          if (lightDirectionView.Z < 0)
          {
            // Light comes from behind camera.
            renderData.VisiblePixels = 0;
            continue;
          }

          // Project position to viewport.
          Vector3F screenPosition = viewport.ProjectToViewport(-lightDirectionView, projection);

          // LensFlare.QuerySize is the size relative to viewport.
          querySize = lensFlare.QuerySize * viewport.Height;
          renderData.TotalPixels = (int)(querySize * querySize);
          if (renderData.TotalPixels < MinPixelSize)
          {
            // Cull small light sources.
            renderData.VisiblePixels = 0;
            continue;
          }

          // Draw quad in screen space.
          querySize /= 2;
          _queryGeometry[0].Position = new Vector3(screenPosition.X - querySize, screenPosition.Y - querySize, -1);
          _queryGeometry[1].Position = new Vector3(screenPosition.X + querySize, screenPosition.Y - querySize, -1);
          _queryGeometry[2].Position = new Vector3(screenPosition.X - querySize, screenPosition.Y + querySize, -1);
          _queryGeometry[3].Position = new Vector3(screenPosition.X + querySize, screenPosition.Y + querySize, -1);

          _basicEffect.World = Matrix.Identity;
          _basicEffect.View = Matrix.Identity;
          _basicEffect.Projection = orthographicProjection;
          _basicEffect.CurrentTechnique.Passes[0].Apply();

          if (renderData.OcclusionQuery == null)
            renderData.OcclusionQuery = new OcclusionQuery(graphicsDevice);

          renderData.OcclusionQuery.Begin();
          graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _queryGeometry, 0, 2);
          renderData.OcclusionQuery.End();
        }
        else
        {
          // ----- Local lights

          // Determine planar distance to camera.
          Vector3F position = node.PoseWorld.Position;
          Vector3F cameraToNode = position - cameraPose.Position;
          float distance = Vector3F.Dot(cameraToNode, cameraForward);
          if (distance < cameraNode.Camera.Projection.Near)
          {
            // Light is behind near plane.
            renderData.VisiblePixels = 0;
            continue;
          }

          Debug.Assert(
            node.ScaleWorld.X > 0 && node.ScaleWorld.Y > 0 && node.ScaleWorld.Z > 0,
            "Assuming that all scale factors are positive.");

          // LensFlare.QuerySize is the size in world space.
          querySize = node.ScaleWorld.LargestComponent * node.LensFlare.QuerySize;

          // Estimate screen space size of query geometry.
          float screenSizeX = viewport.Width * querySize * xScale;
          float screenSizeY = viewport.Height * querySize * yScale;
          if (!isOrthographic)
          {
            float oneOverDistance = 1 / distance;
            screenSizeX *= oneOverDistance;
            screenSizeY *= oneOverDistance;
          }

          renderData.TotalPixels = (int)(screenSizeX * screenSizeY);
          if (renderData.TotalPixels < MinPixelSize)
          {
            // Cull small light sources.
            renderData.VisiblePixels = 0;
            continue;
          }

          // Draw screen-aligned quad in world space.
          querySize /= 2;
          Vector3F upVector = querySize * cameraUp;
          Vector3F rightVector = querySize * cameraRight;

          // Offset quad by half its size towards the camera. Otherwise, the geometry 
          // of the light source could obstruct the query geometry.
          position -= querySize * cameraToNode.Normalized;
          _queryGeometry[0].Position = (Vector3)(position - rightVector - upVector);
          _queryGeometry[1].Position = (Vector3)(position - rightVector + upVector);
          _queryGeometry[2].Position = (Vector3)(position + rightVector - upVector);
          _queryGeometry[3].Position = (Vector3)(position + rightVector + upVector);

          _basicEffect.World = Matrix.Identity;
          _basicEffect.View = (Matrix)view;
          _basicEffect.Projection = (Matrix)projection;
          _basicEffect.CurrentTechnique.Passes[0].Apply();

          if (renderData.OcclusionQuery == null)
            renderData.OcclusionQuery = new OcclusionQuery(graphicsDevice);

          renderData.OcclusionQuery.Begin();
          graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _queryGeometry, 0, 2);
          renderData.OcclusionQuery.End();
        }
      }

      savedRenderState.Restore();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      // Lens flares are used sparsely in most games. --> Early out, if possible.
      int numberOfNodes = nodes.Count;
      if (nodes.Count == 0)
        return;

      context.Validate(_spriteBatch);
      context.ThrowIfCameraMissing();

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      bool hiDef = (graphicsDevice.GraphicsProfile == GraphicsProfile.HiDef);

      // Camera properties
      var cameraNode = context.CameraNode;
      var cameraPose = cameraNode.PoseWorld;
      Vector3F cameraForward = -cameraPose.Orientation.GetColumn(2); // 3rd column vector (negated)
      Matrix44F view = cameraNode.View;
      Matrix44F projection = cameraNode.Camera.Projection;

      // The flares are positioned on a line from the origin through the center of 
      // the screen.
      var viewport = graphicsDevice.Viewport;
      Vector2F screenCenter = new Vector2F(viewport.Width / 2.0f, viewport.Height / 2.0f);

      if (_transformParameter != null)
      {
        // ----- Original:
        // Matrix matrix = (Matrix)(Matrix44F.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1)
        //                 * Matrix44F.CreateTranslation(-0.5f, -0.5f, 0)); // Half-pixel offset (only for Direct3D 9).
        // ----- Inlined:
        Matrix matrix = new Matrix();
        float oneOverW = 1.0f / viewport.Width;
        float oneOverH = 1.0f / viewport.Height;
        matrix.M11 = oneOverW * 2f;
        matrix.M22 = -oneOverH * 2f;
        matrix.M33 = -1f;
        matrix.M44 = 1f;
#if MONOGAME
        matrix.M41 = -1f;
        matrix.M42 = 1f;
#else
        // Direct3D 9: half-pixel offset
        matrix.M41 = -oneOverW - 1f;
        matrix.M42 = oneOverH + 1f;
#endif

        _transformParameter.SetValue(matrix);
      }

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      // Choose current effect technique: Linear vs. Gamma-corrected Writes.
      if (_effect != null)
        _effect.CurrentTechnique = context.IsHdrEnabled() ? _techniqueLinear : _techniqueGamma;

      _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive, null, null, null, _effect);
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as LensFlareNode;
        if (node == null)
          continue;

        var lensFlare = node.LensFlare;
        float size, intensity;
        if (hiDef)
        {
          // HiDef profile
          object dummy;
          cameraNode.ViewDependentData.TryGetValue(node, out dummy);
          var renderData = dummy as OcclusionData;
          if (renderData == null || renderData.VisiblePixels == 0)
            continue;

          lensFlare.OnGetSizeAndIntensity(node, context, renderData.VisiblePixels, renderData.TotalPixels, out size, out intensity);
        }
        else
        {
          // Reach profile
          lensFlare.OnGetSizeAndIntensity(node, context, 0, 0, out size, out intensity);
        }

        if (size <= 0 || intensity < MinIntensity)
          continue;

        // LensFlareNode is visible in current frame.
        node.LastFrame = frame;

        // Project position to screen space.
        Vector2F screenPosition;
        if (lensFlare.IsDirectional)
        {
          // ----- Directional lights
          Vector3F lightDirectionWorld = -node.PoseWorld.Orientation.GetColumn(2);  // 3rd column vector (negated)
          Vector3F lightDirectionView = cameraPose.ToLocalDirection(lightDirectionWorld);

          // In Reach profile check light direction for visibility.
          // (In HiDef profile this check is done UpdateOcclusion().)
          if (!hiDef && lightDirectionView.Z < 0)
          {
            // Light comes from behind camera.
            continue;
          }

          Vector3F position = viewport.ProjectToViewport(-lightDirectionView, projection);
          screenPosition = new Vector2F(position.X, position.Y);
        }
        else
        {
          // ----- Local lights
          Vector3F position = node.PoseWorld.Position;

          // In Reach profile check light direction for visibility.
          // (In HiDef profile this check is done UpdateOcclusion().)
          if (!hiDef)
          {
            Vector3F cameraToNode = position - cameraPose.Position;
            float distance = Vector3F.Dot(cameraToNode, cameraForward);
            if (distance < cameraNode.Camera.Projection.Near)
            {
              // Light is behind near plane.
              continue;
            }
          }

          position = viewport.ProjectToViewport(position, projection * view);
          screenPosition = new Vector2F(position.X, position.Y);
        }

        Vector2F flareVector = screenCenter - screenPosition;
        foreach (var flare in lensFlare.Elements)
        {
          if (flare == null)
            continue;

          var packedTexture = flare.Texture;
          if (packedTexture == null)
            continue;

          // Position the flare on a line from the lens flare origin through the 
          // screen center.
          Vector2F position = screenPosition + flareVector * flare.Distance;

          // The intensity controls the alpha value.
          Vector4 color = flare.Color.ToVector4();
          color.W *= intensity;

          // Get texture.
          Texture2D textureAtlas = packedTexture.TextureAtlas;
          Vector2F textureAtlasSize = new Vector2F(textureAtlas.Width, textureAtlas.Height);
          Vector2F textureOffset = packedTexture.Offset * textureAtlasSize;
          Vector2F textureSize = packedTexture.Scale * textureAtlasSize;
          Rectangle sourceRectangle = new Rectangle((int)textureOffset.X, (int)textureOffset.Y, (int)textureSize.X, (int)textureSize.Y);

          // The image rotates around its origin (= reference point) - usually the
          // center of the image.
          Vector2F origin = textureSize * flare.Origin;
          float rotation = flare.Rotation;
          Vector2F direction = flareVector;
          if (Numeric.IsNaN(rotation) && direction.TryNormalize())
          {
            // NaN = automatic rotation:
            // Determine angle between direction and reference vector (0, 1):
            // From http://www.euclideanspace.com/maths/algebra/vectors/angleBetween/issues/index.htm:
            // rotation = atan2(v2.y,v2.x) - atan2(v1.y,v1.x)
            //          = atan2(v2.y,v2.x) - atan2(1,0)
            //          = atan2(v2.y,v2.x) - π/2
            rotation = (float)Math.Atan2(direction.Y, direction.X) - ConstantsF.PiOver2;
          }

          Vector2F scale = size * viewport.Height * flare.Scale / textureSize.Y;

          // Render flare using additive blending.
          _spriteBatch.Draw(textureAtlas, (Vector2)position, sourceRectangle, new Color(color),
                            rotation, (Vector2)origin, (Vector2)scale, SpriteEffects.None, 0);
        }
      }

      _spriteBatch.End();
      savedRenderState.Restore();
    }
    #endregion
  }
}
