// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="SkyObjectNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  internal class SkyObjectRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly VertexPositionTexture[] _vertices = new VertexPositionTexture[4];

    private readonly Effect _effect;
    private readonly EffectParameter _effectParameterViewProjection;
    private readonly EffectParameter _effectParameterUp;
    private readonly EffectParameter _effectParameterRight;
    private readonly EffectParameter _effectParameterNormal;
    private readonly EffectParameter _effectParameterSunDirection;
    private readonly EffectParameter _effectParameterSunLight;
    private readonly EffectParameter _effectParameterAmbientLight;
    private readonly EffectParameter _effectParameterObjectTexture;
    private readonly EffectParameter _effectParameterTextureParameters;
    private readonly EffectParameter _effectParameterLightWrapSmoothness;
    private readonly EffectParameter _effectParameterGlow0;
    private readonly EffectParameter _effectParameterGlow1;
    private readonly EffectPass _effectPassObjectLinear;
    private readonly EffectPass _effectPassObjectGamma;
    private readonly EffectPass _effectPassGlowLinear;
    private readonly EffectPass _effectPassGlowGamma;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SkyObjectRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public SkyObjectRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        throw new NotSupportedException("The SkyObjectRenderer does not support the Reach profile.");

      // Load effect.
      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Sky/SkyObject");
      _effectParameterViewProjection = _effect.Parameters["ViewProjection"];
      _effectParameterUp = _effect.Parameters["Up"];
      _effectParameterRight = _effect.Parameters["Right"];
      _effectParameterNormal = _effect.Parameters["Normal"];
      _effectParameterSunDirection = _effect.Parameters["SunDirection"];
      _effectParameterSunLight = _effect.Parameters["SunLight"];
      _effectParameterAmbientLight = _effect.Parameters["AmbientLight"];
      _effectParameterObjectTexture = _effect.Parameters["ObjectTexture"];
      _effectParameterTextureParameters = _effect.Parameters["TextureParameters"];
      _effectParameterLightWrapSmoothness = _effect.Parameters["LightWrapSmoothness"];
      _effectParameterGlow0 = _effect.Parameters["Glow0"];
      _effectParameterGlow1 = _effect.Parameters["Glow1"];
      _effectPassObjectLinear = _effect.Techniques[0].Passes["ObjectLinear"];
      _effectPassObjectGamma = _effect.Techniques[0].Passes["ObjectGamma"];
      _effectPassGlowLinear = _effect.Techniques[0].Passes["GlowLinear"];
      _effectPassGlowGamma = _effect.Techniques[0].Passes["GlowGamma"];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is SkyObjectNode;
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

      int numberOfNodes = nodes.Count;
      if (nodes.Count == 0)
        return;

      context.Validate(_effect);
      context.ThrowIfCameraMissing();

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.BlendState = BlendState.AlphaBlend;

      // Camera properties
      var cameraNode = context.CameraNode;
      Pose cameraPose = cameraNode.PoseWorld;
      Matrix view = (Matrix)new Matrix44F(cameraPose.Orientation.Transposed, new Vector3F(0));
      Matrix projection = cameraNode.Camera.Projection;
      _effectParameterViewProjection.SetValue(view * projection);

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as SkyObjectNode;
        if (node == null)
          continue;

        // SkyObjectNode is visible in current frame.
        node.LastFrame = frame;

        // Get billboard axes from scene node pose.
        Matrix33F orientation = node.PoseWorld.Orientation;
        Vector3F right = orientation.GetColumn(0);
        Vector3F up = orientation.GetColumn(1);
        Vector3F normal = orientation.GetColumn(2);
        Vector3F forward = -normal;

        _effectParameterNormal.SetValue((Vector3)(normal));

        // ----- Render object texture.
        var texture = node.Texture;
        if (texture != null)
        {
          _effectParameterUp.SetValue((Vector3)(up));
          _effectParameterRight.SetValue((Vector3)(right));
          _effectParameterSunLight.SetValue((Vector3)node.SunLight);
          _effectParameterAmbientLight.SetValue(new Vector4((Vector3)node.AmbientLight, node.Alpha));
          _effectParameterObjectTexture.SetValue(texture.TextureAtlas);
          _effectParameterLightWrapSmoothness.SetValue(new Vector2(node.LightWrap, node.LightSmoothness));
          _effectParameterSunDirection.SetValue((Vector3)node.SunDirection);

          float halfWidthX = (float)Math.Tan(node.AngularDiameter.X / 2);
          float halfWidthY = (float)Math.Tan(node.AngularDiameter.Y / 2);

          // Texture coordinates of packed texture.
          Vector2F texCoordLeftTop = texture.GetTextureCoordinates(new Vector2F(0, 0), 0);
          Vector2F texCoordRightBottom = texture.GetTextureCoordinates(new Vector2F(1, 1), 0);
          float texCoordLeft = texCoordLeftTop.X;
          float texCoordTop = texCoordLeftTop.Y;
          float texCoordRight = texCoordRightBottom.X;
          float texCoordBottom = texCoordRightBottom.Y;

          _effectParameterTextureParameters.SetValue(new Vector4(
            (texCoordLeft + texCoordRight) / 2,
            (texCoordTop + texCoordBottom) / 2,
            1 / ((texCoordRight - texCoordLeft) / 2),    // 1 / half extent
            1 / ((texCoordBottom - texCoordTop) / 2)));

          _vertices[0].Position = (Vector3)(forward - right * halfWidthX - up * halfWidthY);
          _vertices[0].TextureCoordinate = new Vector2(texCoordLeft, texCoordBottom);
          _vertices[1].Position = (Vector3)(forward - right * halfWidthX + up * halfWidthY);
          _vertices[1].TextureCoordinate = new Vector2(texCoordLeft, texCoordTop);
          _vertices[2].Position = (Vector3)(forward + right * halfWidthX - up * halfWidthY);
          _vertices[2].TextureCoordinate = new Vector2(texCoordRight, texCoordBottom);
          _vertices[3].Position = (Vector3)(forward + right * halfWidthX + up * halfWidthY);
          _vertices[3].TextureCoordinate = new Vector2(texCoordRight, texCoordTop);

          if (context.IsHdrEnabled())
            _effectPassObjectLinear.Apply();
          else
            _effectPassObjectGamma.Apply();

          graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertices, 0, 2);
        }

        // ----- Render glows.
        if (node.GlowColor0.LengthSquared > 0 || node.GlowColor1.LengthSquared > 0)
        {
          _effectParameterGlow0.SetValue(new Vector4((Vector3)node.GlowColor0, node.GlowExponent0));
          _effectParameterGlow1.SetValue(new Vector4((Vector3)node.GlowColor1, node.GlowExponent1));

          float halfWidth0 = (float)Math.Tan(Math.Acos(Math.Pow(node.GlowCutoffThreshold / node.GlowColor0.LargestComponent, 1 / node.GlowExponent0)));
          if (!Numeric.IsPositiveFinite(halfWidth0))
            halfWidth0 = 0;
          float halfWidth1 = (float)Math.Tan(Math.Acos(Math.Pow(node.GlowCutoffThreshold / node.GlowColor1.LargestComponent, 1 / node.GlowExponent1)));
          if (!Numeric.IsPositiveFinite(halfWidth1))
            halfWidth1 = 0;
          float halfWidth = Math.Max(halfWidth0, halfWidth1);

          _vertices[0].Position = (Vector3)(forward - right * halfWidth - up * halfWidth);
          _vertices[0].TextureCoordinate = (Vector2)new Vector2F(0, 1);
          _vertices[1].Position = (Vector3)(forward - right * halfWidth + up * halfWidth);
          _vertices[1].TextureCoordinate = (Vector2)new Vector2F(0, 0);
          _vertices[2].Position = (Vector3)(forward + right * halfWidth - up * halfWidth);
          _vertices[2].TextureCoordinate = (Vector2)new Vector2F(1, 1);
          _vertices[3].Position = (Vector3)(forward + right * halfWidth + up * halfWidth);
          _vertices[3].TextureCoordinate = (Vector2)new Vector2F(1, 0);

          if (context.IsHdrEnabled())
            _effectPassGlowLinear.Apply();
          else
            _effectPassGlowGamma.Apply();

          graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertices, 0, 2);
        }
      }

      savedRenderState.Restore();
    }
    #endregion
  }
}
#endif
