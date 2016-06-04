// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders a cube map ("skybox") into the background of the current render target.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A "skybox" is a cube map that is used as the background of a scene. A skybox is usually drawn 
  /// after all opaque objects to fill the background.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  internal class SkyboxRendererInternal : SceneNodeRenderer
  {
    // TODO: Remove previous SkyboxRenderer and rename this class to SkyboxRenderer.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Effect _effect;

    #region ----- Reach -----
    private VertexPositionTexture[] _faceVertices;
    #endregion
    
    #region ----- HiDef -----
    private EffectParameter _textureParameter;
    private EffectParameter _parameterWorldViewProjection;
    private EffectParameter _parameterColor;
    private EffectParameter _parameterRgbmMaxValue;
    private EffectParameter _parameterTextureSize;
    private EffectPass _passRgbToRgb;
    private EffectPass _passSRgbToRgb;
    private EffectPass _passRgbmToRgb;
    private EffectPass _passRgbToSRgb;
    private EffectPass _passSRgbToSRgb;
    private EffectPass _passRgbmToSRgb;
    private Submesh _submesh;
    #endregion

    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SkyboxRendererInternal"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SkyboxRendererInternal(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        InitializeReach(graphicsService);
      else
        InitializeHiDef(graphicsService);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is SkyboxNode;
    }


    /// <inheritdoc/>
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

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      var cameraNode = context.CameraNode;
      cameraNode.LastFrame = frame;

      bool reach = (context.GraphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach);
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as SkyboxNode;
        if (node == null)
          continue;

        // SkyboxNode is visible in current frame.
        node.LastFrame = frame;

        if (node.Texture != null)
        {
          if (reach)
            RenderReach(node, context);
          else
            RenderHiDef(node, context);
        }
      }
    }


    #region ----- Reach -----

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    private void InitializeReach(IGraphicsService graphicsService)
    {
      // Use BasicEffect for rendering.
      var graphicsDevice = graphicsService.GraphicsDevice;
      _effect = new BasicEffect(graphicsDevice)
      {
        FogEnabled = false,
        LightingEnabled = false,
        TextureEnabled = true,
        VertexColorEnabled = false
      };

      // Create single face of skybox.
      _faceVertices = new[]
      {
        new VertexPositionTexture(new Vector3(1, -1, -1), new Vector2(0, 1)),
        new VertexPositionTexture(new Vector3(1, 1, -1), new Vector2(0, 0)),
        new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(1, 1)),
        new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 0)),
      };
    }


    private void RenderReach(SkyboxNode node, RenderContext context)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.BlendState = node.EnableAlphaBlending ? BlendState.AlphaBlend : BlendState.Opaque;
      graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      // Change viewport to render all pixels at max z.
      var originalViewport = graphicsDevice.Viewport;
      var viewport = originalViewport;
      viewport.MinDepth = viewport.MaxDepth;
      graphicsDevice.Viewport = viewport;

      var cameraNode = context.CameraNode;
      var view = cameraNode.View;
      view.Translation = Vector3F.Zero;
      var projection = cameraNode.Camera.Projection;

      var basicEffect = (BasicEffect)_effect;
      basicEffect.View = (Matrix)view;
      basicEffect.Projection = projection;
      basicEffect.DiffuseColor = (Vector3)node.Color;
      basicEffect.Alpha = node.EnableAlphaBlending ? node.Alpha : 1;

      // Scale skybox such that it lies within view frustum:
      //   distance of a skybox corner = √3
      //   √3 * scale = far 
      //   => scale = far / √3
      // (Note: If  near > far / √3  then the skybox will be clipped.)
      float scale = projection.Far * 0.577f;

      var orientation = node.PoseWorld.Orientation;

      // Positive X
      basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.PositiveX);
      basicEffect.World = (Matrix)new Matrix44F(orientation * scale, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      // Negative X      
      // transform = scale * rotY(180°)
      var transform = new Matrix33F(-scale, 0, 0, 0, scale, 0, 0, 0, -scale);
      basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.NegativeX);
      basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      // Positive Y
      // transform = scale * rotX(90°) * rotY(90°)
      transform = new Matrix33F(0, 0, scale, scale, 0, 0, 0, scale, 0);
      basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.PositiveY);
      basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      // Negative Y
      // transform = scale * rotX(-90°) * rotY(90°)
      transform = new Matrix33F(0, 0, scale, -scale, 0, 0, 0, -scale, 0);
      basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.NegativeY);
      basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      // Cube maps are left-handed, where as the world is right-handed!

      // Positive Z (= negative Z in world space)
      // transform = scale * rotY(90°)
      transform = new Matrix33F(0, 0, scale, 0, scale, 0, -scale, 0, 0);
      basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.PositiveZ);
      basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      // Negative Z (= positive Z in world space)
      // transform = scale * rotY(-90°)
      transform = new Matrix33F(0, 0, -scale, 0, scale, 0, scale, 0, 0);
      basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.NegativeZ);
      basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      graphicsDevice.Viewport = originalViewport;
      savedRenderState.Restore();
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    private static Texture2D GetTexture2D(GraphicsDevice graphicsDevice, TextureCube textureCube, CubeMapFace cubeMapFace)
    {
      // Unfortunately, we cannot treat the TextureCube as Texture2D[] in XNA.
      // We could try to copy all faces into a 3x2 Texture2D, but this is problematic:
      //  - Extracting DXT compressed faces is difficult.
      //  - Additional texel border required for correct texture filtering at edges.
      //  + The skybox could be rendered with a single draw call.
      //
      // --> Manually convert TextureCube to Texture2D[6] and store array in Tag.

      var faces = textureCube.Tag as Texture2D[];
      if (faces == null || faces.Length != 6)
      {
        if (textureCube.Tag != null)
          throw new GraphicsException("The SkyboxRenderer (Reach profile) needs to store information in Tag property of the skybox texture, but the Tag property is already in use.");

        faces = new Texture2D[6];
        var size = textureCube.Size;

        int numberOfBytes;
        switch (textureCube.Format)
        {
          case SurfaceFormat.Color:
            numberOfBytes = size * size * 4;
            break;
          case SurfaceFormat.Dxt1:
            numberOfBytes = size * size / 2;
            break;
          default:
            throw new GraphicsException("The SkyboxRenderer (Reach profile) only supports the following surface formats: Color, Dxt1.");
        }

        var face = new byte[numberOfBytes];
        for (int i = 0; i < 6; i++)
        {
          var texture2D = new Texture2D(graphicsDevice, size, size, false, textureCube.Format);
          textureCube.GetData((CubeMapFace)i, face);
          texture2D.SetData(face);
          faces[i] = texture2D;
        }

        textureCube.Tag = faces;
        textureCube.Disposing += OnTextureCubeDisposing;
      }

      return faces[(int)cubeMapFace];
    }


    private static void OnTextureCubeDisposing(object sender, EventArgs eventArgs)
    {
      var textureCube = (TextureCube)sender;
      var faces = textureCube.Tag as Texture2D[];
      if (faces != null)
        foreach (var face in faces)
          face.Dispose();
    }
    #endregion


    #region ----- HiDef -----

    private void InitializeHiDef(IGraphicsService graphicsService)
    {
      _submesh = MeshHelper.GetBox(graphicsService);
      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Sky/Skybox");
      _parameterWorldViewProjection = _effect.Parameters["WorldViewProjection"];
      _parameterColor = _effect.Parameters["Color"];
      _parameterRgbmMaxValue = _effect.Parameters["RgbmMax"];
      _parameterTextureSize = _effect.Parameters["TextureSize"];
      _textureParameter = _effect.Parameters["Texture"];
      _passRgbToRgb = _effect.CurrentTechnique.Passes["RgbToRgb"];
      _passSRgbToRgb = _effect.CurrentTechnique.Passes["SRgbToRgb"];
      _passRgbmToRgb = _effect.CurrentTechnique.Passes["RgbmToRgb"];
      _passRgbToSRgb = _effect.CurrentTechnique.Passes["RgbToSRgb"];
      _passSRgbToSRgb = _effect.CurrentTechnique.Passes["SRgbToSRgb"];
      _passRgbmToSRgb = _effect.CurrentTechnique.Passes["RgbmToSRgb"];
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void RenderHiDef(SkyboxNode node, RenderContext context)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.BlendState = node.EnableAlphaBlending ? BlendState.AlphaBlend : BlendState.Opaque;

      bool sourceIsFloatingPoint = TextureHelper.IsFloatingPointFormat(node.Texture.Format);

      // Set sampler state. (Floating-point textures cannot use linear filtering. (XNA would throw an exception.))
      if (sourceIsFloatingPoint)
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      var cameraNode = context.CameraNode;
      Matrix44F view = cameraNode.View;
      Matrix44F projection = cameraNode.Camera.Projection;

      // Cube maps are left handed --> Sample with inverted z. (Otherwise, the 
      // cube map and objects or texts in it are mirrored.)
      var mirrorZ = Matrix44F.CreateScale(1, 1, -1);
      Matrix33F orientation = node.PoseWorld.Orientation;
      _parameterWorldViewProjection.SetValue((Matrix)(projection * view * new Matrix44F(orientation, Vector3F.Zero) * mirrorZ));

      Vector4 color = node.EnableAlphaBlending
                      ? new Vector4((Vector3)node.Color * node.Alpha, node.Alpha) // Premultiplied
                      : new Vector4((Vector3)node.Color, 1);                      // Opaque
      _parameterColor.SetValue(color);
      _textureParameter.SetValue(node.Texture);
      
      if (node.Encoding is RgbEncoding)
      {
        _parameterTextureSize.SetValue(node.Texture.Size);
        if (context.IsHdrEnabled())
          _passRgbToRgb.Apply();
        else
          _passRgbToSRgb.Apply();
      }
      else if (node.Encoding is SRgbEncoding)
      {
        if (!sourceIsFloatingPoint)
        {
          if (context.IsHdrEnabled())
            _passSRgbToRgb.Apply();
          else
            _passSRgbToSRgb.Apply();
        }
        else
        {
          throw new GraphicsException("sRGB encoded skybox cube maps must not use a floating point format.");
        }
      }
      else if (node.Encoding is RgbmEncoding)
      {
        float max = GraphicsHelper.ToGamma(((RgbmEncoding)node.Encoding).Max);
        _parameterRgbmMaxValue.SetValue(max);

        if (context.IsHdrEnabled())
          _passRgbmToRgb.Apply();
        else
          _passRgbmToSRgb.Apply();
      }
      else
      {
        throw new NotSupportedException("The SkyBoxRenderer supports only RgbEncoding, SRgbEncoding and RgbmEncoding.");
      }

      _submesh.Draw();
      savedRenderState.Restore();
    }
    #endregion

    #endregion
  }
}
#endif
