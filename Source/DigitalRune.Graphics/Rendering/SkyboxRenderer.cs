// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
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
  [Obsolete("The SkyboxRenderer was replaced by the SkyboxNode and the SkyRenderer.")]
  public class SkyboxRenderer
  {
    private Effect _effect;


    /// <summary>
    /// Initializes a new instance of the <see cref="SkyboxRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SkyboxRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      var graphicsDevice = graphicsService.GraphicsDevice;
      if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        InitializeReach(graphicsDevice);
      else
        InitializeHiDef(graphicsService);
    }


    /// <summary>
    /// Renders a skybox.
    /// </summary>
    /// <param name="texture">The cube map with the sky texture.</param>
    /// <param name="orientation">The orientation of the skybox.</param>
    /// <param name="exposure">The exposure factor that is multiplied to the cube map values to change the brightness.
    /// (Usually 1 or higher).</param>
    /// <param name="context">
    /// The render context. (<see cref="RenderContext.CameraNode"/> needs to be set.)
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> or <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void Render(TextureCube texture, Matrix33F orientation, float exposure, RenderContext context)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");
      if (context == null)
        throw new ArgumentNullException("context");

      context.Validate(_effect);
      context.ThrowIfCameraMissing();

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        RenderReach(texture, orientation, exposure, context);
      else
        RenderHiDef(texture, orientation, exposure, context);
    }


    #region ----- Reach -----

    private VertexPositionTexture[] _faceVertices;


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    private void InitializeReach(GraphicsDevice graphicsDevice)
    {
      // Use BasicEffect for rendering.
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


    private void RenderReach(TextureCube texture, Matrix33F orientation, float exposure, RenderContext context)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.BlendState = BlendState.Opaque;
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
      basicEffect.DiffuseColor = new Vector3(exposure);

      // Scale skybox such that it lies within view frustum:
      //   distance of a skybox corner = √3
      //   √3 * scale = far 
      //   => scale = far / √3
      // (Note: If  near > far / √3  then the skybox will be clipped.)
      float scale = projection.Far * 0.577f;

      // Positive X
      basicEffect.Texture = GetTexture2D(graphicsDevice, texture, CubeMapFace.PositiveX);
      basicEffect.World = (Matrix)new Matrix44F(orientation * scale, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      // Negative X      
      // transform = scale * rotY(180°)
      var transform = new Matrix33F(-scale, 0, 0, 0, scale, 0, 0, 0, -scale);
      basicEffect.Texture = GetTexture2D(graphicsDevice, texture, CubeMapFace.NegativeX);
      basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      // Positive Y
      // transform = scale * rotX(90°) * rotY(90°)
      transform = new Matrix33F(0, 0, scale, scale, 0, 0, 0, scale, 0);
      basicEffect.Texture = GetTexture2D(graphicsDevice, texture, CubeMapFace.PositiveY);
      basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      // Negative Y
      // transform = scale * rotX(-90°) * rotY(90°)
      transform = new Matrix33F(0, 0, scale, -scale, 0, 0, 0, -scale, 0);
      basicEffect.Texture = GetTexture2D(graphicsDevice, texture, CubeMapFace.NegativeY);
      basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      // Cube maps are left-handed, where as the world is right-handed!

      // Positive Z (= negative Z in world space)
      // transform = scale * rotY(90°)
      transform = new Matrix33F(0, 0, scale, 0, scale, 0, -scale, 0, 0);
      basicEffect.Texture = GetTexture2D(graphicsDevice, texture, CubeMapFace.PositiveZ);
      basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      // Negative Z (= positive Z in world space)
      // transform = scale * rotY(-90°)
      transform = new Matrix33F(0, 0, -scale, 0, scale, 0, scale, 0, 0);
      basicEffect.Texture = GetTexture2D(graphicsDevice, texture, CubeMapFace.NegativeZ);
      basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3F.Zero);
      basicEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

      graphicsDevice.Viewport = originalViewport;
      savedRenderState.Restore();
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
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

    private EffectParameter _textureParameter;
    private EffectParameter _parameterWorldViewProjection;
    private EffectParameter _parameterExposure;
    private EffectPass _passLinear;
    private EffectPass _passGamma;
    private Submesh _submesh;


    private void InitializeHiDef(IGraphicsService graphicsService)
    {
      _submesh = MeshHelper.GetBox(graphicsService);
      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Sky/Skybox");
      _parameterWorldViewProjection = _effect.Parameters["WorldViewProjection"];
      _parameterExposure = _effect.Parameters["Color"];
      _textureParameter = _effect.Parameters["Texture"];
      _passLinear = _effect.CurrentTechnique.Passes["SRgbToRgb"];
      _passGamma = _effect.CurrentTechnique.Passes["SRgbToSRgb"];
    }


    private void RenderHiDef(TextureCube texture, Matrix33F orientation, float exposure, RenderContext context)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.BlendState = BlendState.Opaque;

      var cameraNode = context.CameraNode;
      Matrix44F view = cameraNode.View;
      Matrix44F projection = cameraNode.Camera.Projection;

      // Cube maps are left handed --> Sample with inverted z. (Otherwise, the 
      // cube map and objects or texts in it are mirrored.)
      var mirrorZ = Matrix44F.CreateScale(1, 1, -1);
      _parameterWorldViewProjection.SetValue(
        (Matrix)(projection * view * new Matrix44F(orientation, Vector3F.Zero) * mirrorZ));
      _parameterExposure.SetValue(new Vector4(exposure, exposure, exposure, 1));
      _textureParameter.SetValue(texture);

      if (context.IsHdrEnabled())
        _passLinear.Apply();
      else
        _passGamma.Apply();

      _submesh.Draw();
      savedRenderState.Restore();
    }
    #endregion
  }
}
