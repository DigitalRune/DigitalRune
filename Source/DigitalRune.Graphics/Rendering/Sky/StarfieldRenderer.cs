// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="StarfieldNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Each star is rendered using a billboard projected onto the far plane. Anti-aliasing in the
  /// shader is used to get smooth dots.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  internal class StarfieldRenderer : SceneNodeRenderer
  {
    // References:
    // [ZFX] Hochwertiges Rendern von Sternen 2.0, http://zfx.info/viewtopic.php?f=11&t=8

    // Notes on Instancing:
    // Hardware instancing could be used to reduce the buffer sizes for stars. But 
    // performances tests have shown that hardware instancing is significantly slower.
    //
    // 9110 stars on Intel Core i7-3770K, GeForce 680:
    // - with instancing: 16 bytes (quad) + 145760 bytes (stars) => ~4ms (draw)
    // - without instancing: 728800 bytes                        => <0.2ms (draw)
    // 
    // Rendering all stars with a single draw call (duplicate data, no instancing)
    // is optimal.

    // TODO:
    // - Add scintillation (search YouTube for "star scintillation" for example videos).
    //   Ideas: Modulate star intensity with a noise function in the vertex shader.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    private struct StarVertex : IVertexType
    {
      public HalfVector4 PositionAndSize;
      public HalfVector4 Color;
      public Vector2 Texture;   // HalfVector2 works on Windows when using 
                                // [StructLayout(LayoutKind.Sequential, Pack = 4)].
                                // But this option is not available on Xbox 360.

      public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
      (
        new VertexElement(0, VertexElementFormat.HalfVector4, VertexElementUsage.Position, 0),
        new VertexElement(8, VertexElementFormat.HalfVector4, VertexElementUsage.Color, 0),
        new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
      );
      VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _effectParameterViewportSize;
    private readonly EffectParameter _effectParameterWorldViewProjection;
    private readonly EffectParameter _effectParameterIntensity;
    private readonly EffectPass _effectPassLinear;
    private readonly EffectPass _effectPassGamma;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="StarfieldRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public StarfieldRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        throw new NotSupportedException("The StarfieldRenderer does not support the Reach profile.");

      // Load effect.
      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Sky/Starfield");
      _effectParameterViewportSize = _effect.Parameters["ViewportSize"];
      _effectParameterWorldViewProjection = _effect.Parameters["WorldViewProjection"];
      _effectParameterIntensity = _effect.Parameters["Intensity"];
      _effectPassLinear = _effect.Techniques[0].Passes["Linear"];
      _effectPassGamma = _effect.Techniques[0].Passes["Gamma"];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is StarfieldNode;
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

      // Camera properties
      var cameraNode = context.CameraNode;
      Matrix view = (Matrix)cameraNode.View;
      Matrix projection = cameraNode.Camera.Projection;
      Matrix viewProjection = view * projection;

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      // Blend additively over any cosmos textures.
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.BlendState = BlendState.Additive;

      _effectParameterViewportSize.SetValue(new Vector2(context.Viewport.Width, context.Viewport.Height));

      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as StarfieldNode;
        if (node == null)
          continue;

        // SkyboxNode is visible in current frame.
        node.LastFrame = frame;

        if (node.Stars != null && node.Stars.Count > 0)
        {
          Matrix world = (Matrix)new Matrix44F(node.PoseWorld.Orientation, Vector3F.Zero);
          _effectParameterWorldViewProjection.SetValue(world * viewProjection);

          // In [ZFX] the star luminance of the precomputed star data is scaled with 
          // float const viewFactor = tan(fov);
          // float const resolutionFactor = resolution / 1920.0f;
          // float const luminanceScale = 1.0f / (viewFactor * viewFactor) * (resolutionFactor * resolutionFactor);
          // We ignore this here, but we could add this factor to the Intensity parameter.
          _effectParameterIntensity.SetValue((Vector3)node.Color);

          if (context.IsHdrEnabled())
            _effectPassLinear.Apply();
          else
            _effectPassGamma.Apply();

          var mesh = GetStarfieldMesh(node, context);
          mesh.Draw();
        }
      }

      savedRenderState.Restore();
    }


    /// <summary>
    /// Gets/creates the mesh for rendering the starfield.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="context">The render context.</param>
    /// <returns>The mesh for rendering the starfield.</returns>
    /// <exception cref="GraphicsException">
    /// The number of stars is greater than half the max number of primitives which the graphics
    /// device can draw in a single draw call.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    private static Submesh GetStarfieldMesh(StarfieldNode node, RenderContext context)
    {
      // The mesh is cached in the scene node.
      var mesh = node.RenderData as Submesh;
      if (mesh == null)
      {
        mesh = new Submesh { PrimitiveType = PrimitiveType.TriangleList };
        node.RenderData = mesh;
      }

      if (mesh.VertexBuffer == null || mesh.VertexBuffer.IsDisposed)
      {
        int numberOfStars = node.Stars.Count;

        // The total number of stars is limited by the graphics device max primitives per call limit.
        var graphicsDevice = context.GraphicsService.GraphicsDevice;
        int maxNumberOfStars = graphicsDevice.GetMaxPrimitivesPerCall() / 2;
        if (numberOfStars > maxNumberOfStars)
          throw new GraphicsException("The number of stars must be less than " + maxNumberOfStars);

        // Create a vertex buffer with a quad for each star.
        //   1--2
        //   | /|
        //   |/ |
        //   0--3
        mesh.VertexCount = numberOfStars * 4;
        var vertices = new StarVertex[numberOfStars * 4];
        for (int i = 0; i < numberOfStars; i++)
        {
          var star = node.Stars[i];
          var positionAndSize = new HalfVector4(star.Position.X, star.Position.Y, star.Position.Z, star.Size);
          var color = new HalfVector4(star.Color.X, star.Color.Y, star.Color.Z, 1);

          vertices[i * 4 + 0].PositionAndSize = positionAndSize;
          vertices[i * 4 + 0].Color = color;
          vertices[i * 4 + 0].Texture = new Vector2(0, 1);

          vertices[i * 4 + 1].PositionAndSize = positionAndSize;
          vertices[i * 4 + 1].Color = color;
          vertices[i * 4 + 1].Texture = new Vector2(0, 0);

          vertices[i * 4 + 2].PositionAndSize = positionAndSize;
          vertices[i * 4 + 2].Color = color;
          vertices[i * 4 + 2].Texture = new Vector2(1, 0);

          vertices[i * 4 + 3].PositionAndSize = positionAndSize;
          vertices[i * 4 + 3].Color = color;
          vertices[i * 4 + 3].Texture = new Vector2(1, 1);
        }
        mesh.VertexBuffer = new VertexBuffer(graphicsDevice, typeof(StarVertex), vertices.Length, BufferUsage.None);
        mesh.VertexBuffer.SetData(vertices);

        // Create index buffer for quad (= two triangles, clockwise).
        var indices = new ushort[numberOfStars * 6];
        for (int i = 0; i < numberOfStars; i++)
        {
          indices[i * 6 + 0] = (ushort)(i * 4 + 0);
          indices[i * 6 + 1] = (ushort)(i * 4 + 1);
          indices[i * 6 + 2] = (ushort)(i * 4 + 2);
          indices[i * 6 + 3] = (ushort)(i * 4 + 0);
          indices[i * 6 + 4] = (ushort)(i * 4 + 2);
          indices[i * 6 + 5] = (ushort)(i * 4 + 3);
        }
        mesh.IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
        mesh.IndexBuffer.SetData(indices);

        mesh.PrimitiveCount = numberOfStars * 2;
      }

      return mesh;
    }
    #endregion
  }
}
#endif
