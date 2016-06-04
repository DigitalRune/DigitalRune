// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
#if ANIMATION
using DigitalRune.Animation.Character;
#endif
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Provides methods for rendering debug information.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class is your one-stop-shop for simple debug rendering. It has several Draw<i>Xyz</i>() 
  /// methods to draw points, lines, geometric objects, text, textures, etc. Calling a 
  /// Draw<i>Xyz</i>() method does not immediately draw something - instead the draw job is cached 
  /// and all draw jobs are rendered when <see cref="Render"/> is called. <see cref="Clear"/> can be
  /// used to remove all current draw jobs. 
  /// </para>
  /// <para>
  /// Many draw calls allow to specify a flag "drawOverScene". If this flag is set, the objects are 
  /// drawn over the scene - they ignore the z-buffer information of the scene. Many draw calls for 
  /// solid shapes allow to specify a flag "drawWireFrame". If this flag is set, a simplified line 
  /// representation is drawn instead of solid faces.
  /// </para>
  /// <para>
  /// For text rendering, a <see cref="SpriteFont"/> must be set. The <see cref="DebugRenderer"/>
  /// does not have a default sprite font.
  /// </para>
  /// <para>
  /// Primitives drawn with solid faces can be transparent. All color values use non-premultiplied
  /// alpha.
  /// </para>
  /// <para>
  /// This class assumes that all input color values are non-premultiplied alpha values.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
  public class DebugRenderer : IDisposable
  {
    // Todos and ideas for new functions:
    // - 2D positioning, 
    // - text blocks at all screen corners (not only top-left), 
    // - Text alignment (centered, right), 
    // - different point sizes, 
    // - different text sizes, 
    // - draw only objects within a certain range, 
    // ...


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IGraphicsService _graphicsService;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    #region ----- Batches -----
    // Render batches and similar things are only allocated when needed because they
    // usually reserve a lot of memory when they are created.

    private SpriteBatch SpriteBatch
    {
      get
      {
        if (_spriteBatch == null)
          _spriteBatch = _graphicsService.GetSpriteBatch();

        return _spriteBatch;
      }
    }
    private SpriteBatch _spriteBatch;


    private PointBatch InScenePointBatch
    {
      get
      {
        if (_inScenePointBatch == null)
          _inScenePointBatch = new PointBatch(Effect);

        return _inScenePointBatch;
      }
    }
    private PointBatch _inScenePointBatch;


    private PointBatch OverScenePointBatch
    {
      get
      {
        if (_overScenePointBatch == null)
          _overScenePointBatch = new PointBatch(Effect);

        return _overScenePointBatch;
      }
    }
    private PointBatch _overScenePointBatch;


    private LineBatch InSceneLineBatch
    {
      get
      {
        if (_inSceneLineBatch == null)
          _inSceneLineBatch = new LineBatch(Effect);

        return _inSceneLineBatch;
      }
    }
    private LineBatch _inSceneLineBatch;


    private LineBatch OverSceneLineBatch
    {
      get
      {
        if (_overSceneLineBatch == null)
          _overSceneLineBatch = new LineBatch(Effect);

        return _overSceneLineBatch;
      }
    }
    private LineBatch _overSceneLineBatch;


    private TextBatch InSceneTextBatch
    {
      get
      {
        if (_inSceneTextBatch == null)
        {
          _inSceneTextBatch = new TextBatch(SpriteBatch, SpriteFont)
          {
            EnableDepthTest = true
          };
        }

        return _inSceneTextBatch;
      }
    }
    private TextBatch _inSceneTextBatch;


    private TextBatch OverScene2DTextBatch
    {
      get
      {
        if (_overScene2DTextBatch == null)
        {
          _overScene2DTextBatch = new TextBatch(SpriteBatch, SpriteFont)
          {
            EnableDepthTest = false
          };
        }

        return _overScene2DTextBatch;
      }
    }
    private TextBatch _overScene2DTextBatch;


    private TextBatch OverScene3DTextBatch
    {
      get
      {
        if (_overScene3DTextBatch == null)
        {
          _overScene3DTextBatch = new TextBatch(SpriteBatch, SpriteFont)
          {
            EnableDepthTest = false
          };
        }

        return _overScene3DTextBatch;
      }
    }
    private TextBatch _overScene3DTextBatch;


    private PrimitiveBatch OpaquePrimitiveBatch
    {
      get
      {
        if (_opaquePrimitiveBatch == null)
        {
          _opaquePrimitiveBatch = new PrimitiveBatch(_graphicsService, Effect)
          {
            DrawWireFrame = false,
            SortBackToFront = false
          };
        }

        return _opaquePrimitiveBatch;
      }
    }
    private PrimitiveBatch _opaquePrimitiveBatch;


    private PrimitiveBatch TransparentPrimitiveBatch
    {
      get
      {
        if (_transparentPrimitiveBatch == null)
        {
          _transparentPrimitiveBatch = new PrimitiveBatch(_graphicsService, Effect)
          {
            DrawWireFrame = false,
            SortBackToFront = true,
          };
        }

        return _transparentPrimitiveBatch;
      }
    }
    private PrimitiveBatch _transparentPrimitiveBatch;


    private PrimitiveBatch InSceneWireFramePrimitiveBatch
    {
      get
      {
        if (_inSceneWireFramePrimitiveBatch == null)
        {
          _inSceneWireFramePrimitiveBatch = new PrimitiveBatch(_graphicsService, Effect)
          {
            DrawWireFrame = true,
            SortBackToFront = false,
          };
        }

        return _inSceneWireFramePrimitiveBatch;
      }
    }
    private PrimitiveBatch _inSceneWireFramePrimitiveBatch;


    private PrimitiveBatch OverSceneWireFramePrimitiveBatch
    {
      get
      {
        if (_overSceneWireFramePrimitiveBatch == null)
        {
          _overSceneWireFramePrimitiveBatch = new PrimitiveBatch(_graphicsService, Effect)
          {
            DrawWireFrame = true,
            SortBackToFront = false,
          };
        }

        return _overSceneWireFramePrimitiveBatch;
      }
    }
    private PrimitiveBatch _overSceneWireFramePrimitiveBatch;


    private TriangleBatch OpaqueTriangleBatch
    {
      get
      {
        if (_opaqueTriangleBatch == null)
          _opaqueTriangleBatch = new TriangleBatch(Effect);

        return _opaqueTriangleBatch;
      }
    }
    private TriangleBatch _opaqueTriangleBatch;


    private TriangleBatch TransparentTriangleBatch
    {
      get
      {
        if (_transparentTriangleBatch == null)
          _transparentTriangleBatch = new TriangleBatch(Effect);

        return _transparentTriangleBatch;
      }
    }
    private TriangleBatch _transparentTriangleBatch;


    private TriangleBatch InSceneWireframeTriangleBatch
    {
      get
      {
        if (_inSceneWireFrameTriangleBatch == null)
          _inSceneWireFrameTriangleBatch = new TriangleBatch(Effect);

        return _inSceneWireFrameTriangleBatch;
      }
    }
    private TriangleBatch _inSceneWireFrameTriangleBatch;


    private TriangleBatch OverSceneWireframeTriangleBatch
    {
      get
      {
        if (_overSceneWireFrameTriangleBatch == null)
          _overSceneWireFrameTriangleBatch = new TriangleBatch(Effect);

        return _overSceneWireFrameTriangleBatch;
      }
    }
    private TriangleBatch _overSceneWireFrameTriangleBatch;


    private TextureBatch TextureBatch
    {
      get
      {
        if (_textureBatch == null)
          _textureBatch = new TextureBatch(SpriteBatch);

        return _textureBatch;
      }
    }
    private TextureBatch _textureBatch;


    private StringBuilder StringBuilder
    {
      get
      {
        if (_stringBuilder == null)
          _stringBuilder = new StringBuilder();

        return _stringBuilder;
      }
    }
    private StringBuilder _stringBuilder;

    #endregion


    /// <summary>
    /// Gets a value indicating whether this instance has been disposed of.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance has been disposed of; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsDisposed { get; private set; }


    /// <summary>
    /// Gets the effect used for rendering.
    /// </summary>
    /// <value>
    /// The effect used for rendering. If this value is <see langword="null"/>, the debug renderer 
    /// does not draw points, lines or triangles. The default value is a new 
    /// <see cref="BasicEffect"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public BasicEffect Effect
    {
      get
      {
        if (_effect == null)
        {
          _effect = new BasicEffect(_graphicsService.GraphicsDevice)
          {
            FogEnabled = false,
            PreferPerPixelLighting = false,
            TextureEnabled = false,
            VertexColorEnabled = true,
            World = Matrix.Identity,
          };
          _effect.EnableDefaultLighting();
        }

        return _effect;
      }
    }
    private BasicEffect _effect;


    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="DebugRenderer"/> is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>. The default value is 
    /// <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// If this value is <see langword="false"/> no debug info is drawn.
    /// </remarks>
    public bool Enabled { get; set; }


    /// <summary>
    /// Gets or sets the default color that is used if no color is explicitly specified.
    /// </summary>
    /// <value>The default color.</value>
    public Color DefaultColor { get; set; }


    /// <summary>
    /// Gets or sets the size of drawn points.
    /// </summary>
    /// <value>The size of a visible point (in pixels).</value>
    public float PointSize
    {
      get { return _pointSize; }
      set
      {
        _pointSize = value;

        if (_inScenePointBatch != null)
          _inScenePointBatch.PointSize = value;

        if (_overScenePointBatch != null)
          _overScenePointBatch.PointSize = value;
      }
    }
    private float _pointSize;


    /// <summary>
    /// Gets or sets the sprite font.
    /// </summary>
    /// <value>
    /// The sprite font. The default value is <see langword="null"/> - all texts are ignored!
    /// </value>
    public SpriteFont SpriteFont
    {
      get { return _spriteFont; }
      set
      {
        _spriteFont = value;

        if (_inSceneTextBatch != null)
          _inSceneTextBatch.SpriteFont = value;

        if (_overScene2DTextBatch != null)
          _overScene2DTextBatch.SpriteFont = value;

        if (_overScene3DTextBatch != null)
          _overScene3DTextBatch.SpriteFont = value;
      }
    }
    private SpriteFont _spriteFont;


    /// <summary>
    /// Gets or sets the default text position.
    /// </summary>
    /// <value>
    /// The default text position. The default value is (NaN, NaN) - in which case the default text 
    /// position is the upper left corner of the title-safe area.
    /// </value>
    public Vector2F DefaultTextPosition
    {
      get { return _defaultTextPosition; }
      set { _defaultTextPosition = value; }
    }
    private Vector2F _defaultTextPosition = new Vector2F(float.NaN);


    /// <summary>
    /// Gets or sets the size of the arrow head (relative to the arrow length).
    /// </summary>
    /// <value>
    /// The relative size of the arrow head. This value should be in the range [0, 1].
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public float ArrowHeadSize
    {
      get { return _arrowHeadSize; }
      set { _arrowHeadSize = value; }
    }
    private float _arrowHeadSize = 0.2f;


    /// <summary>
    /// Gets or sets a value indicating whether the debug renderer automatically sets the required
    /// render states (depth-stencil, blend and rasterizer states required to render solid or
    /// wireframe, in or over scene, opaque or transparent).
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the debug renderer automatically sets the required render states;
    /// otherwise, <see langword="false" /> to use the currently set depth-stencil, blend and
    /// rasterizer states. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// To be able to render the different categories of primitives (solid/wireframe, in/over scene
    /// opaque/transparent) the debug renderer automatically sets the required render states. If
    /// this property is <see langword="false"/>, the renderer uses the render states currently set
    /// in the graphics device. This can be used, for example, to use the debug renderer to render
    /// models with additive blending. However, all primitives (solid/wireframe, in/over scene,
    /// etc.) will use the user-defined render state and might not be rendered as expected; for
    /// example if the blend state is set to "opaque", transparent primitives will also appear
    /// opaque.
    /// </para>
    /// <para>
    /// Note: Points, text and textures ignore this property and are always rendered with automatic
    /// render states.
    /// </para>
    /// </remarks>
    public bool AutoRenderStates
    {
      get { return _autoRenderStates; }
      set
      {
        _autoRenderStates = value;

        if (_opaquePrimitiveBatch != null)
          _opaquePrimitiveBatch.AutoRasterizerState = value;

        if (_transparentPrimitiveBatch != null)
          _transparentPrimitiveBatch.AutoRasterizerState = value;

        if (_inSceneWireFramePrimitiveBatch != null)
          _inSceneWireFramePrimitiveBatch.AutoRasterizerState = value;

        if (_overSceneWireFramePrimitiveBatch != null)
          _overSceneWireFramePrimitiveBatch.AutoRasterizerState = value;
      }
    }
    private bool _autoRenderStates;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="spriteFont">The sprite font.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public DebugRenderer(IGraphicsService graphicsService, SpriteFont spriteFont)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _graphicsService = graphicsService;
      _spriteFont = spriteFont;
      Enabled = true;
      DefaultColor = Color.LightGreen;
      _pointSize = 5;
      _autoRenderStates = true;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DebugRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="spriteBatch">The sprite batch.</param>
    /// <param name="spriteFont">The sprite font.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    [Obsolete("It is no longer necessary to specify a SpriteBatch.")]
    public DebugRenderer(IGraphicsService graphicsService, SpriteBatch spriteBatch, SpriteFont spriteFont)
      : this(graphicsService, spriteFont)
    {
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="DebugRenderer"/> class.
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
    /// Releases the unmanaged resources used by an instance of the <see cref="DebugRenderer"/> class 
    /// and optionally releases the managed resources.
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
          _effect.SafeDispose();

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void ThrowIfDisposed()
    {
      if (IsDisposed)
        throw new ObjectDisposedException(GetType().FullName);
    }


    /// <summary>
    /// Updates the internal caches of the <see cref="DebugRenderer"/>. 
    /// (Usually you do not need to call this method, see remarks.)
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last frame.</param>
    /// <remarks>
    /// This method does not need to be called in typical applications. A "typical" application
    /// is an app where <see cref="GraphicsManager.Update"/> of the <see cref="GraphicsManager"/> 
    /// is called every frame. However, if for some reasons 
    /// <see cref="GraphicsManager"/>.<see cref="GraphicsManager.Update"/> is not regularly called 
    /// in an app, then <see cref="Update"/> of the <see cref="DebugRenderer"/> should be called 
    /// manually, usually once per frame.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    [Obsolete("This method is not needed anymore.")]
    public void Update(TimeSpan deltaTime)
    {
    }


    /// <summary>
    /// Draws the debug information.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public void Render(RenderContext context)
    {
      ThrowIfDisposed();

      if (context == null)
        throw new ArgumentNullException("context");

      if (!Enabled)
        return;

      context.Validate(_spriteBatch);
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      // ----- Draw points, lines, and text within 3D scene.
      if (_autoRenderStates)
      {
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = GraphicsHelper.RasterizerStateCullCounterClockwise;
        graphicsDevice.BlendState = BlendState.Opaque;
      }

      // No wireframe mode in OpenGLES
      bool skipWireframes = (GlobalSettings.PlatformID == PlatformID.Android || GlobalSettings.PlatformID == PlatformID.iOS); 

      if (_inSceneLineBatch != null)
        _inSceneLineBatch.Render(context);

      if (_inSceneWireFramePrimitiveBatch != null && !skipWireframes)
        _inSceneWireFramePrimitiveBatch.Render(context);

      if (_autoRenderStates)
        graphicsDevice.RasterizerState = GraphicsHelper.RasterizerStateWireFrame;

      if (_inSceneWireFrameTriangleBatch != null && !skipWireframes)
        _inSceneWireFrameTriangleBatch.Render(context);

      if (_autoRenderStates)
        graphicsDevice.RasterizerState = GraphicsHelper.RasterizerStateCullCounterClockwise;

      if (_inScenePointBatch != null)
        _inScenePointBatch.Render(context);

      if (_inSceneTextBatch != null)
        _inSceneTextBatch.Render(context);

      // ----- Draw opaque objects within 3D scene.
      if (_autoRenderStates)
      {
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = GraphicsHelper.RasterizerStateCullCounterClockwise;
        graphicsDevice.BlendState = BlendState.Opaque;
      }

      if (_opaquePrimitiveBatch != null)
        _opaquePrimitiveBatch.Render(context);

      if (_opaqueTriangleBatch != null)
        _opaqueTriangleBatch.Render(context);

      // ----- Draw transparent objects within 3D scene.
      if (_autoRenderStates)
      {
        graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        graphicsDevice.BlendState = BlendState.AlphaBlend;
      }

      if (_transparentPrimitiveBatch != null)
        _transparentPrimitiveBatch.Render(context);

      if (_transparentTriangleBatch != null)
        _transparentTriangleBatch.Render(context);

      // ----- Draw points, lines, text, textures over scene.
      if (_autoRenderStates)
      {
        graphicsDevice.DepthStencilState = DepthStencilState.None;
        graphicsDevice.BlendState = BlendState.Opaque;
      }

      if (_overSceneWireFramePrimitiveBatch != null && !skipWireframes)
        _overSceneWireFramePrimitiveBatch.Render(context);

      if (_autoRenderStates)
        graphicsDevice.RasterizerState = GraphicsHelper.RasterizerStateWireFrame;

      if (_overSceneWireFrameTriangleBatch != null && !skipWireframes)
        _overSceneWireFrameTriangleBatch.Render(context);

      if (_autoRenderStates)
        graphicsDevice.RasterizerState = GraphicsHelper.RasterizerStateCullCounterClockwise;

      if (_overSceneLineBatch != null)
        _overSceneLineBatch.Render(context);

      if (_overScenePointBatch != null)
        _overScenePointBatch.Render(context);

      if (_overScene3DTextBatch != null)
        _overScene3DTextBatch.Render(context);

      if (_textureBatch != null)
        _textureBatch.Render(context);

      if (_overScene2DTextBatch != null)
        _overScene2DTextBatch.Render(context);

      // ----- Draw debug text.
      if (_stringBuilder != null && _stringBuilder.Length > 0 && _spriteFont != null)
      {
        var spriteBatch = SpriteBatch;
        spriteBatch.Begin();

        Vector2 position = new Vector2(DefaultTextPosition.X, DefaultTextPosition.Y);
        if (Numeric.IsNaN(position.X))
        {
          Point titleSafeLocation = graphicsDevice.Viewport.TitleSafeArea.Location;
          position.X = titleSafeLocation.X;
          position.Y = titleSafeLocation.Y;
        }

        spriteBatch.DrawString(_spriteFont, _stringBuilder, position, DefaultColor, 0, new Vector2(), 1.0f, SpriteEffects.None, 0.5f);
        spriteBatch.End();
      }

      graphicsDevice.ResetTextures();
      savedRenderState.Restore();
    }


    /// <summary>
    /// Clears the debug renderer (removes all draw jobs).
    /// </summary>
    public void Clear()
    {
      if (_inScenePointBatch != null)
        _inScenePointBatch.Clear();

      if (_overScenePointBatch != null)
        _overScenePointBatch.Clear();

      if (_inSceneLineBatch != null)
        _inSceneLineBatch.Clear();

      if (_overSceneLineBatch != null)
        _overSceneLineBatch.Clear();

      if (_inSceneTextBatch != null)
        _inSceneTextBatch.Clear();

      if (_overScene2DTextBatch != null)
        _overScene2DTextBatch.Clear();

      if (_overScene3DTextBatch != null)
        _overScene3DTextBatch.Clear();

      if (_opaquePrimitiveBatch != null)
        _opaquePrimitiveBatch.Clear();

      if (_transparentPrimitiveBatch != null)
        _transparentPrimitiveBatch.Clear();

      if (_inSceneWireFramePrimitiveBatch != null)
        _inSceneWireFramePrimitiveBatch.Clear();

      if (_overSceneWireFramePrimitiveBatch != null)
        _overSceneWireFramePrimitiveBatch.Clear();

      if (_opaqueTriangleBatch != null)
        _opaqueTriangleBatch.Clear();

      if (_transparentTriangleBatch != null)
        _transparentTriangleBatch.Clear();

      if (_inSceneWireFrameTriangleBatch != null)
        _inSceneWireFrameTriangleBatch.Clear();

      if (_overSceneWireFrameTriangleBatch != null)
        _overSceneWireFrameTriangleBatch.Clear();

      if (_textureBatch != null)
        _textureBatch.Clear();

      if (_stringBuilder != null)
        _stringBuilder.Clear();
    }


    /// <summary>
    /// Draws a point.
    /// </summary>
    /// <param name="position">The position in world space.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    public void DrawPoint(Vector3F position, Color color, bool drawOverScene)
    {
      if (!Enabled)
        return;

      var renderer = drawOverScene ? OverScenePointBatch : InScenePointBatch;
      renderer.Add(position, color);
    }


    /// <summary>
    /// Draws a line.
    /// </summary>
    /// <param name="start">The start position in world space.</param>
    /// <param name="end">The end position in world space.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    public void DrawLine(Vector3F start, Vector3F end, Color color, bool drawOverScene)
    {
      if (!Enabled)
        return;

      var renderer = drawOverScene ? OverSceneLineBatch : InSceneLineBatch;
      renderer.Add(start, end, color);
    }


    /// <overloads>
    /// <summary>
    /// Draws a triangle (with counter-clockwise winding for front faces).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Draws a triangle (with counter-clockwise winding for front faces).
    /// </summary>
    /// <param name="vertex0">The first vertex position in world space.</param>
    /// <param name="vertex1">The second vertex position in world space.</param>
    /// <param name="vertex2">The third vertex position in world space.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the object is drawn
    /// with solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawTriangle(Vector3F vertex0, Vector3F vertex1, Vector3F vertex2,
      Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      TriangleBatch batch;
      if (drawOverScene)
        batch = OverSceneWireframeTriangleBatch;
      else if (drawWireFrame)
        batch = InSceneWireframeTriangleBatch;
      else if (color.A == 255)
        batch = OpaqueTriangleBatch;
      else
        batch = TransparentTriangleBatch;

      var normal = Vector3F.Cross(vertex1 - vertex0, vertex2 - vertex0);
      // (normal is normalized in the BasicEffect HLSL.)

      // Draw with swapped winding order!
      batch.Add(ref vertex0, ref vertex2, ref vertex1, ref normal, ref color);
    }


    /// <summary>
    /// Draws a triangle (with counter-clockwise winding for front faces).
    /// </summary>
    /// <param name="vertex0">The first vertex position in world space.</param>
    /// <param name="vertex1">The second vertex position in world space.</param>
    /// <param name="vertex2">The third vertex position in world space.</param>
    /// <param name="normal">
    /// The normal vector of the triangle (pointing away from the front side).
    /// </param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the object is drawn
    /// with solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawTriangle(Vector3F vertex0, Vector3F vertex1, Vector3F vertex2, Vector3F normal,
      Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      TriangleBatch batch;
      if (drawOverScene)
        batch = OverSceneWireframeTriangleBatch;
      else if (drawWireFrame)
        batch = InSceneWireframeTriangleBatch;
      else if (color.A == 255)
        batch = OpaqueTriangleBatch;
      else
        batch = TransparentTriangleBatch;

      // Draw with swapped winding order!
      batch.Add(ref vertex0, ref vertex2, ref vertex1, ref normal, ref color);
    }


    /// <summary>
    /// Draws a triangle (with counter-clockwise winding for front faces).
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the object is drawn
    /// with solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawTriangle(Triangle triangle, Pose pose, Vector3F scale,
      Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      TriangleBatch batch;
      if (drawOverScene)
        batch = OverSceneWireframeTriangleBatch;
      else if (drawWireFrame)
        batch = InSceneWireframeTriangleBatch;
      else if (color.A == 255)
        batch = OpaqueTriangleBatch;
      else
        batch = TransparentTriangleBatch;

      var transform = pose * Matrix44F.CreateScale(scale);

      // Transform to world space.
      triangle.Vertex0 = transform.TransformPosition(triangle.Vertex0);
      triangle.Vertex1 = transform.TransformPosition(triangle.Vertex1);
      triangle.Vertex2 = transform.TransformPosition(triangle.Vertex2);

      var normal = triangle.Normal;

      // Draw with swapped winding order!
      batch.Add(ref triangle.Vertex0, ref triangle.Vertex2, ref triangle.Vertex1, ref normal, ref color);
    }


    /// <summary>
    /// Draws a triangle (with counter-clockwise winding for front faces).
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="normal">
    /// The normal vector of the triangle (pointing away from the front side).
    /// </param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the object is drawn
    /// with solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawTriangle(Triangle triangle, Pose pose, Vector3F scale, Vector3F normal,
      Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      TriangleBatch batch;
      if (drawOverScene)
        batch = OverSceneWireframeTriangleBatch;
      else if (drawWireFrame)
        batch = InSceneWireframeTriangleBatch;
      else if (color.A == 255)
        batch = OpaqueTriangleBatch;
      else
        batch = TransparentTriangleBatch;

      var transform = pose * Matrix44F.CreateScale(scale);
      normal = transform.TransformNormal(normal);

      // Transform to world space.
      triangle.Vertex0 = transform.TransformPosition(triangle.Vertex0);
      triangle.Vertex1 = transform.TransformPosition(triangle.Vertex1);
      triangle.Vertex2 = transform.TransformPosition(triangle.Vertex2);

      // Draw with swapped winding order!
      batch.Add(ref triangle.Vertex0, ref triangle.Vertex2, ref triangle.Vertex1, ref normal, ref color);
    }


    /// <summary>
    /// Draws the triangles of the given mesh (with counter-clockwise winding for front faces).
    /// </summary>
    /// <param name="mesh">The triangle mesh.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the object is drawn
    /// with solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <remarks>
    /// Warning: Calling this method every frame to render the same triangle mesh is very 
    /// inefficient! If the triangle mesh does not change, call <see cref="DrawShape"/> with a 
    /// <see cref="TriangleMeshShape"/> instead!
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawTriangles(ITriangleMesh mesh, Pose pose, Vector3F scale,
      Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled || mesh == null)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      TriangleBatch batch;
      if (drawOverScene)
        batch = OverSceneWireframeTriangleBatch;
      else if (drawWireFrame)
        batch = InSceneWireframeTriangleBatch;
      else if (color.A == 255)
        batch = OpaqueTriangleBatch;
      else
        batch = TransparentTriangleBatch;

      if (Vector3F.AreNumericallyEqual(scale, Vector3F.One) && !pose.HasRotation && !pose.HasTranslation)
      {
        int numberOfTriangles = mesh.NumberOfTriangles;
        for (int i = 0; i < numberOfTriangles; i++)
        {
          var triangle = mesh.GetTriangle(i);

          var normal = Vector3F.Cross(triangle.Vertex1 - triangle.Vertex0, triangle.Vertex2 - triangle.Vertex0);
          // (normal is normalized in the BasicEffect HLSL.)

          // Draw with swapped winding order!
          batch.Add(ref triangle.Vertex0, ref triangle.Vertex2, ref triangle.Vertex1, ref normal, ref color);
        }
      }
      else
      {
        var transform = pose * Matrix44F.CreateScale(scale);

        int numberOfTriangles = mesh.NumberOfTriangles;
        for (int i = 0; i < numberOfTriangles; i++)
        {
          var triangle = mesh.GetTriangle(i);

          // Transform to world space.
          triangle.Vertex0 = transform.TransformPosition(triangle.Vertex0);
          triangle.Vertex1 = transform.TransformPosition(triangle.Vertex1);
          triangle.Vertex2 = transform.TransformPosition(triangle.Vertex2);

          var normal = Vector3F.Cross(triangle.Vertex1 - triangle.Vertex0, triangle.Vertex2 - triangle.Vertex0);
          // (normal is normalized in the BasicEffect HLSL.)

          // Draw with swapped winding order!
          batch.Add(ref triangle.Vertex0, ref triangle.Vertex2, ref triangle.Vertex1, ref normal, ref color);
        }
      }
    }


    /// <summary>
    /// Draws a texture.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="rectangle">The destination rectangle in screen space.</param>
    public void DrawTexture(Texture2D texture, Rectangle rectangle)
    {
      if (!Enabled || texture == null)
        return;

      TextureBatch.Add(texture, rectangle);
    }


    /// <overloads>
    /// <summary>
    /// Draws the text to the screen.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Draws the text to the screen.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <remarks>
    /// The text is added as a text line to a list of text that is drawn to the screen.
    /// </remarks>
    public void DrawText(string text)
    {
      if (!Enabled || text == null)
        return;

      StringBuilder.AppendLine(text);
    }


    /// <summary>
    /// Draws the text to the screen.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <remarks>
    /// The text is added as a text line to a list of text that is drawn to the screen.
    /// </remarks>
    public void DrawText(StringBuilder text)
    {
      if (!Enabled || text == null)
        return;

      var stringBuilder = StringBuilder;

      if (text.Length > 0)
        StringBuilderExtensions.Append(stringBuilder, text);

      stringBuilder.AppendLine();
    }


    /// <summary>
    /// Draws a text on a 2D position in screen space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in screen space (measured in pixels).</param>
    /// <param name="color">The color.</param>
    public void DrawText(string text, Vector2F position, Color color)
    {
      DrawText(text, position, Vector2F.Zero, color);
    }


    /// <summary>
    /// Draws a text on a 2D position in screen space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in screen space (measured in pixels).</param>
    /// <param name="color">The color.</param>
    public void DrawText(StringBuilder text, Vector2F position, Color color)
    {
      DrawText(text, position, Vector2F.Zero, color);
    }


    /// <summary>
    /// Draws a text on a 2D position in screen space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in screen space (measured in pixels).</param>
    /// <param name="relativeOrigin">
    /// The relative origin of the text. (0, 0) means that the upper-left corner of the text is at
    /// <paramref name="position"/>; (1, 1) means that the lower-right corner of the text is at 
    /// <paramref name="position"/>. Use (0.5, 0.5) to center the text.
    /// </param>
    /// <param name="color">The color.</param>
    public void DrawText(string text, Vector2F position, Vector2F relativeOrigin, Color color)
    {
      if (!Enabled || string.IsNullOrEmpty(text))
        return;

      OverScene2DTextBatch.Add(text, position, relativeOrigin, color);
    }


    /// <summary>
    /// Draws a text on a 2D position in screen space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in screen space (measured in pixels).</param>
    /// <param name="relativeOrigin">
    /// The relative origin of the text. (0, 0) means that the upper-left corner of the text is at
    /// <paramref name="position"/>; (1, 1) means that the lower-right corner of the text is at 
    /// <paramref name="position"/>. Use (0.5, 0.5) to center the text.
    /// </param>
    /// <param name="color">The color.</param>
    public void DrawText(StringBuilder text, Vector2F position, Vector2F relativeOrigin, Color color)
    {
      if (!Enabled || text == null || text.Length == 0)
        return;

      OverScene2DTextBatch.Add(text, position, relativeOrigin, color);
    }


    /// <summary>
    /// Draws a text on a 3D position in world space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in world space.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    public void DrawText(string text, Vector3F position, Color color, bool drawOverScene)
    {
      DrawText(text, position, Vector2F.Zero, color, drawOverScene);
    }


    /// <summary>
    /// Draws a text on a 3D position in world space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in world space.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    public void DrawText(StringBuilder text, Vector3F position, Color color, bool drawOverScene)
    {
      DrawText(text, position, Vector2F.Zero, color, drawOverScene);
    }


    /// <summary>
    /// Draws a text on a 3D position in world space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in world space.</param>
    /// <param name="relativeOrigin">
    /// The relative origin of the text. (0, 0) means that the upper-left corner of the text is at
    /// <paramref name="position"/>; (1, 1) means that the lower-right corner of the text is at 
    /// <paramref name="position"/>. Use (0.5, 0.5) to center the text.
    /// </param>
    /// <param name="color">The color.</param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    public void DrawText(string text, Vector3F position, Vector2F relativeOrigin, Color color, bool drawOverScene)
    {
      if (!Enabled || string.IsNullOrEmpty(text))
        return;

      var renderer = drawOverScene ? OverScene3DTextBatch : InSceneTextBatch;
      renderer.Add(text, position, relativeOrigin, color);
    }


    /// <summary>
    /// Draws a text on a 3D position in world space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in world space.</param>
    /// <param name="relativeOrigin">
    /// The relative origin of the text. (0, 0) means that the upper-left corner of the text is at
    /// <paramref name="position"/>; (1, 1) means that the lower-right corner of the text is at 
    /// <paramref name="position"/>. Use (0.5, 0.5) to center the text.
    /// </param>
    /// <param name="color">The color.</param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    public void DrawText(StringBuilder text, Vector3F position, Vector2F relativeOrigin, Color color, bool drawOverScene)
    {
      if (!Enabled || text == null || text.Length == 0)
        return;

      var renderer = drawOverScene ? OverScene3DTextBatch : InSceneTextBatch;
      renderer.Add(text, position, relativeOrigin, color);
    }


    /// <summary>
    /// Draws an axis-aligned bounding-box (AABB). Wire-frame only.
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box.</param>
    /// <param name="pose">The pose of the AABB.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene 
    /// (depth-test disabled).
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void DrawAabb(Aabb aabb, Pose pose, Color color, bool drawOverScene)
    {
      if (!Enabled)
        return;

      Vector3F corner0 = pose.ToWorldPosition(new Vector3F(aabb.Minimum.X, aabb.Minimum.Y, aabb.Maximum.Z));
      Vector3F corner1 = pose.ToWorldPosition(new Vector3F(aabb.Maximum.X, aabb.Minimum.Y, aabb.Maximum.Z));
      Vector3F corner2 = pose.ToWorldPosition(aabb.Maximum);
      Vector3F corner3 = pose.ToWorldPosition(new Vector3F(aabb.Minimum.X, aabb.Maximum.Y, aabb.Maximum.Z));
      Vector3F corner4 = pose.ToWorldPosition(aabb.Minimum);
      Vector3F corner5 = pose.ToWorldPosition(new Vector3F(aabb.Maximum.X, aabb.Minimum.Y, aabb.Minimum.Z));
      Vector3F corner6 = pose.ToWorldPosition(new Vector3F(aabb.Maximum.X, aabb.Maximum.Y, aabb.Minimum.Z));
      Vector3F corner7 = pose.ToWorldPosition(new Vector3F(aabb.Minimum.X, aabb.Maximum.Y, aabb.Minimum.Z));

      DrawLine(corner0, corner1, color, drawOverScene);
      DrawLine(corner1, corner2, color, drawOverScene);
      DrawLine(corner2, corner3, color, drawOverScene);
      DrawLine(corner0, corner3, color, drawOverScene);
      DrawLine(corner4, corner5, color, drawOverScene);
      DrawLine(corner5, corner6, color, drawOverScene);
      DrawLine(corner6, corner7, color, drawOverScene);
      DrawLine(corner7, corner4, color, drawOverScene);
      DrawLine(corner0, corner4, color, drawOverScene);
      DrawLine(corner1, corner5, color, drawOverScene);
      DrawLine(corner2, corner6, color, drawOverScene);
      DrawLine(corner3, corner7, color, drawOverScene);
    }


    /// <summary>
    /// Draws the axis-aligned bounding-boxes (AABBs) of a collection of geometries.
    /// </summary>
    /// <param name="geometricObjects">The geometric objects.</param>
    /// <param name="color">
    /// The color. If this parameter is <see langword="null"/>, each AABB is drawn with a unique 
    /// color.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void DrawAabbs(IEnumerable<IGeometricObject> geometricObjects, Color? color, bool drawOverScene)
    {
      if (!Enabled || geometricObjects == null)
        return;

      foreach (var geometricObject in geometricObjects)
      {
        Color geoColor = color ?? GraphicsHelper.GetUniqueColor(geometricObject);
        DrawAabb(geometricObject.Aabb, Pose.Identity, geoColor, drawOverScene);
      }
    }


    /// <summary>
    /// Draws an arrow pointing from <paramref name="start"/> to <paramref name="end"/>.
    /// </summary>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <seealso cref="ArrowHeadSize"/>
    public void DrawArrow(Vector3F start, Vector3F end, Color color, bool drawOverScene)
    {
      if (!Enabled)
        return;

      // TODO: Arrow could also be drawn as cylinder + cone, or with a preprepared model.
      Vector3F shaft = end - start;
      float length = shaft.Length;
      if (Numeric.IsZero(length))
        return;

      DrawLine(start, end, color, drawOverScene);

      Vector3F shaftDirection = shaft / length;
      Vector3F normal = (shaftDirection).Orthonormal1;
      DrawLine(end, end - ArrowHeadSize * shaft + normal * length * 0.05f, color, drawOverScene);
      DrawLine(end, end - ArrowHeadSize * shaft - normal * length * 0.05f, color, drawOverScene);
    }


    /// <summary>
    /// Draws 3 axes for a coordinate cross.
    /// </summary>
    /// <param name="pose">The pose (position and orientation).</param>
    /// <param name="size">The size in world space.</param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <remarks>
    /// The x-axis is drawn red, the y-axis is drawn green, and the z-axis is drawn blue.
    /// </remarks>
    public void DrawAxes(Pose pose, float size, bool drawOverScene)
    {
      if (!Enabled)
        return;

      DrawArrow(pose.Position, pose.Position + pose.ToWorldDirection(Vector3F.UnitX) * size, Color.Red, drawOverScene);
      DrawArrow(pose.Position, pose.Position + pose.ToWorldDirection(Vector3F.UnitY) * size, Color.Green, drawOverScene);
      DrawArrow(pose.Position, pose.Position + pose.ToWorldDirection(Vector3F.UnitZ) * size, Color.Blue, drawOverScene);
    }


    /// <summary>
    /// Draws a contact.
    /// </summary>
    /// <param name="contact">The contact.</param>
    /// <param name="normalLength">The length of the normal vector in world space.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <remarks>
    /// The penetration depth is visualized with a dark red line.
    /// </remarks>
    public void DrawContact(Contact contact, float normalLength, Color color, bool drawOverScene)
    {
      if (!Enabled || contact == null)
        return;

      DrawPoint(contact.Position, color, drawOverScene);
      DrawLine(contact.Position, contact.Position + contact.Normal * normalLength, color, drawOverScene);

      // Draw a red line that visualizes the penetration depth.
      var halfPenetration = contact.Normal * contact.PenetrationDepth / 2;
      DrawLine(contact.Position - halfPenetration, contact.Position + halfPenetration, Color.DarkRed, drawOverScene);
    }


    /// <overloads>
    /// <summary>
    /// Draws contacts.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Draws contacts.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <param name="normalLength">The length of the normal vector in world space.</param>
    /// <param name="color">
    /// The color. If this parameter is <see langword="null"/>, each contact is drawn with a unique 
    /// color.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <remarks>
    /// The penetration depth is visualized with a dark red line.
    /// </remarks>
    public void DrawContacts(ContactSet contactSet, float normalLength, Color? color, bool drawOverScene)
    {
      if (!Enabled || contactSet == null)
        return;

      foreach (var contact in contactSet)
      {
        Color contactColor = color ?? GraphicsHelper.GetUniqueColor(contact);
        DrawContact(contact, normalLength, contactColor, drawOverScene);
      }
    }


    /// <summary>
    /// Draws contacts.
    /// </summary>
    /// <param name="contactSets">The contact sets.</param>
    /// <param name="normalLength">The length of the normal vector in world space.</param>
    /// <param name="color">
    /// The color. If this parameter is <see langword="null"/>, each contact is drawn with a unique 
    /// color.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <remarks>
    /// The penetration depth is visualized with a dark red line.
    /// </remarks>
    public void DrawContacts(ContactSetCollection contactSets, float normalLength, Color? color, bool drawOverScene)
    {
      if (!Enabled || contactSets == null)
        return;

      foreach (var contactSet in contactSets)
        DrawContacts(contactSet, normalLength, color, drawOverScene);
    }


    /// <summary>
    /// Draws a box.
    /// </summary>
    /// <param name="widthX">The x-size of the box.</param>
    /// <param name="widthY">The y-size of the box.</param>
    /// <param name="widthZ">The z-size of the box.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawBox(float widthX, float widthY, float widthZ, Pose pose, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      PrimitiveBatch batch;
      if (drawOverScene)
        batch = OverSceneWireFramePrimitiveBatch;
      else if (drawWireFrame)
        batch = InSceneWireFramePrimitiveBatch;
      else if (color.A == 255)
        batch = OpaquePrimitiveBatch;
      else
        batch = TransparentPrimitiveBatch;

      batch.AddBox(widthX, widthY, widthZ, pose, color);
    }


    /// <overloads>
    /// <summary>
    /// Draws a view volume (viewing frustum).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Draws a view volume (viewing frustum).
    /// </summary>
    /// <param name="viewVolume">The view volume.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="viewVolume"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawViewVolume(ViewVolume viewVolume, Pose pose, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (viewVolume == null)
        throw new ArgumentNullException("viewVolume");

      DrawViewVolume(viewVolume is PerspectiveViewVolume, viewVolume.Left, viewVolume.Right, viewVolume.Bottom, viewVolume.Top, viewVolume.Near, viewVolume.Far, pose, color, drawWireFrame, drawOverScene);
    }


    /// <summary>
    /// Draws a view volume (viewing frustum).
    /// </summary>
    /// <param name="viewVolume">The view volume.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="viewVolume"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawViewVolume(ViewVolume viewVolume, Pose pose, Vector3F scale, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (viewVolume == null)
        throw new ArgumentNullException("viewVolume");

      if (!Enabled)
        return;

      float left = viewVolume.Left * scale.X;
      float right = viewVolume.Right * scale.X;
      float bottom = viewVolume.Bottom * scale.Y;
      float top = viewVolume.Top * scale.Y;
      float near = viewVolume.Near * scale.Z;
      float far = viewVolume.Far * scale.Z;
      DrawViewVolume(viewVolume is PerspectiveViewVolume, left, right, bottom, top, near, far, pose, color, drawWireFrame, drawOverScene);
    }


    /// <summary>
    /// Draws a view volume (viewing frustum).
    /// </summary>
    /// <param name="isPerspective">
    /// <see langword="true"/> for perspective view volumes, <see langword="false"/> for 
    /// orthographic view volumes.
    /// </param>
    /// <param name="left">The minimum x-value of the view volume at the near clip plane.</param>
    /// <param name="right">The maximum x-value of the view volume at the near clip plane.</param>
    /// <param name="bottom">The minimum y-value of the view volume at the near clip plane.</param>
    /// <param name="top">The maximum y-value of the view volume at the near clip plane.</param>
    /// <param name="near">The distance to the near clip plane.</param>
    /// <param name="far">The distance to the far clip plane.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawViewVolume(bool isPerspective, float left, float right, float bottom, float top, float near, float far, Pose pose, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      PrimitiveBatch batch;
      if (drawOverScene)
        batch = OverSceneWireFramePrimitiveBatch;
      else if (drawWireFrame)
        batch = InSceneWireFramePrimitiveBatch;
      else if (color.A == 255)
        batch = OpaquePrimitiveBatch;
      else
        batch = TransparentPrimitiveBatch;

      batch.AddViewVolume(isPerspective, left, right, bottom, top, near, far, pose, color);
    }


    /// <summary>
    /// Draws a sphere.
    /// </summary>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="pose">The pose of the sphere in world space.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawSphere(float radius, Pose pose, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      PrimitiveBatch batch;
      if (drawOverScene)
        batch = OverSceneWireFramePrimitiveBatch;
      else if (drawWireFrame)
        batch = InSceneWireFramePrimitiveBatch;
      else if (color.A == 255)
        batch = OpaquePrimitiveBatch;
      else
        batch = TransparentPrimitiveBatch;

      batch.AddSphere(radius, pose, color);
    }


    /// <summary>
    /// Draws a capsule that is centered at the local origin and parallel to the local y axis.
    /// </summary>
    /// <param name="radius">The radius of the capsule.</param>
    /// <param name="height">The total height of the capsule.</param>
    /// <param name="pose">The pose of the sphere in world space.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawCapsule(float radius, float height, Pose pose, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      PrimitiveBatch batch;
      if (drawOverScene)
        batch = OverSceneWireFramePrimitiveBatch;
      else if (drawWireFrame)
        batch = InSceneWireFramePrimitiveBatch;
      else if (color.A == 255)
        batch = OpaquePrimitiveBatch;
      else
        batch = TransparentPrimitiveBatch;

      batch.AddCapsule(radius, height, pose, color);
    }


    /// <summary>
    /// Draws a cylinder that is centered at the local origin and parallel to the local y axis.
    /// </summary>
    /// <param name="radius">The radius of the cylinder.</param>
    /// <param name="height">The total height of the cylinder.</param>
    /// <param name="pose">The pose of the sphere in world space.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawCylinder(float radius, float height, Pose pose, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      PrimitiveBatch batch;
      if (drawOverScene)
        batch = OverSceneWireFramePrimitiveBatch;
      else if (drawWireFrame)
        batch = InSceneWireFramePrimitiveBatch;
      else if (color.A == 255)
        batch = OpaquePrimitiveBatch;
      else
        batch = TransparentPrimitiveBatch;

      batch.AddCylinder(radius, height, pose, color);
    }


    /// <summary>
    /// Draws a cone with the base on the local xz plane pointing up into the local +y direction.
    /// </summary>
    /// <param name="radius">The radius of the cone.</param>
    /// <param name="height">The total height of the cone.</param>
    /// <param name="pose">The pose of the sphere in world space.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawCone(float radius, float height, Pose pose, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      PrimitiveBatch batch;
      if (drawOverScene)
        batch = OverSceneWireFramePrimitiveBatch;
      else if (drawWireFrame)
        batch = InSceneWireFramePrimitiveBatch;
      else if (color.A == 255)
        batch = OpaquePrimitiveBatch;
      else
        batch = TransparentPrimitiveBatch;

      batch.AddCone(radius, height, pose, color);
    }


    /// <summary>
    /// Draws a geometric object.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public void DrawShape(Shape shape, Pose pose, Vector3F scale, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled || shape == null)
        return;

      // We have special support for a few shapes:

      // ----- Box
      BoxShape box = shape as BoxShape;
      if (box != null)
      {
        DrawBox(box.WidthX * scale.X, box.WidthY * scale.Y, box.WidthZ * scale.Z, pose, color, drawWireFrame, drawOverScene);
        return;
      }

      // ----- Capsule
      CapsuleShape capsule = shape as CapsuleShape;
      if (capsule != null && Numeric.AreEqual(scale.X, scale.Z))
      {
        DrawCapsule(capsule.Radius * scale.X, capsule.Height * scale.Y, pose, color, drawWireFrame, drawOverScene);
        return;
      }

      // ----- Cylinder
      CylinderShape cylinder = shape as CylinderShape;
      if (cylinder != null && Numeric.AreEqual(scale.X, scale.Z))
      {
        DrawCylinder(cylinder.Radius * scale.X, cylinder.Height * scale.Y, pose, color, drawWireFrame, drawOverScene);
        return;
      }

      // ----- Cone
      ConeShape cone = shape as ConeShape;
      if (cone != null && Numeric.AreEqual(scale.X, scale.Z))
      {
        DrawCone(cone.Radius * scale.X, cone.Height * scale.Y, pose, color, drawWireFrame, drawOverScene);
        return;
      }

      // ----- EmptyShape
      EmptyShape empty = shape as EmptyShape;
      if (empty != null)
      {
        DrawAxes(pose, 0.5f, drawOverScene);
        return;
      }

      // ----- InfiniteShape
      var infinite = shape as InfiniteShape;
      if (infinite != null)
      {
        DrawAxes(pose, 0.5f, drawOverScene);
        return;
      }

      // ----- Line
      LineShape line = shape as LineShape;
      if (line != null)
      {
        var start = line.PointOnLine - line.Direction * LineShape.MeshSize / 2;
        var end = line.PointOnLine + line.Direction * LineShape.MeshSize / 2;

        // Apply scale and pose.
        start = pose.ToWorldPosition(start * scale);
        end = pose.ToWorldPosition(end * scale);

        DrawLine(start, end, color, drawOverScene);
        return;
      }

      // ----- LineSegment
      LineSegmentShape segment = shape as LineSegmentShape;
      if (segment != null)
      {
        DrawLine(pose.ToWorldPosition(segment.Start * scale), pose.ToWorldPosition(segment.End * scale), color, drawOverScene);
        return;
      }

      // ----- Point
      PointShape point = shape as PointShape;
      if (point != null)
      {
        DrawPoint(pose.ToWorldPosition(point.Position * scale), color, drawOverScene);
        return;
      }

      // ----- Ray
      RayShape ray = shape as RayShape;
      if (ray != null)
      {
        Vector3F start = ray.Origin;
        Vector3F end = ray.Origin + ray.Direction * ray.Length;
        DrawLine(pose.ToWorldPosition(start * scale), pose.ToWorldPosition(end * scale), color, drawOverScene);
        return;
      }

      // ----- ViewVolume
      ViewVolume viewVolume = shape as ViewVolume;
      if (viewVolume != null)
      {
        DrawViewVolume(viewVolume, pose, scale, color, drawWireFrame, drawOverScene);
        return;
      }

      // ----- Sphere
      SphereShape sphere = shape as SphereShape;
      if (sphere != null && Numeric.AreEqual(scale.X, scale.Y) && Numeric.AreEqual(scale.Y, scale.Z))
      {
        DrawSphere(sphere.Radius * scale.X, pose, color, drawWireFrame, drawOverScene);
        return;
      }

      // ----- TransformedShape
      var transformed = shape as TransformedShape;
      if (transformed != null)
      {
        // Draw child shape directly. (Because the child shape could be a special shape, like
        // a sphere or a box, then we do not want to draw the triangle mesh.)
        // Combining transformation this way only works for uniform scaling or when the child
        // is not rotated!
        Pose childPose = transformed.Child.Pose;
        if (Numeric.AreEqual(scale.X, scale.Y) && Numeric.AreEqual(scale.Y, scale.Z) || !childPose.HasRotation)
        {
          Vector3F childScale = transformed.Child.Scale;
          childPose.Position *= scale;

          DrawShape(transformed.Child.Shape, pose * childPose, scale * childScale, color, drawWireFrame, drawOverScene);
          return;
        }
      }

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      PrimitiveBatch batch;
      if (drawOverScene)
        batch = OverSceneWireFramePrimitiveBatch;
      else if (drawWireFrame)
        batch = InSceneWireFramePrimitiveBatch;
      else if (color.A == 255)
        batch = OpaquePrimitiveBatch;
      else
        batch = TransparentPrimitiveBatch;

      batch.AddShape(shape, pose, scale, color);
    }


    /// <summary>
    /// Draws a geometric object.
    /// </summary>
    /// <param name="geometricObject">The geometric object.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawObject(IGeometricObject geometricObject, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (geometricObject == null)  // Enabled property is checked in DrawShape.
        return;

      DrawShape(geometricObject.Shape, geometricObject.Pose, geometricObject.Scale, color, drawWireFrame, drawOverScene);
    }


    /// <summary>
    /// Draws geometric objects.
    /// </summary>
    /// <param name="geometricObjects">The geometric objects.</param>
    /// <param name="color">
    /// The color. If this parameter is <see langword="null"/>, each geometric object is drawn with 
    /// a unique color.
    /// </param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawObjects(IEnumerable<IGeometricObject> geometricObjects, Color? color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled || geometricObjects == null)
        return;

      foreach (var geometricObject in geometricObjects)
      {
        Color geoColor = color ?? GraphicsHelper.GetUniqueColor(geometricObject);
        DrawShape(geometricObject.Shape, geometricObject.Pose, geometricObject.Scale, geoColor, drawWireFrame, drawOverScene);
      }
    }


    /// <overloads>
    /// <summary>
    /// Draws a mesh or submesh (without textures).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Draws a submesh.
    /// </summary>
    /// <param name="submesh">The submesh.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void DrawMesh(Submesh submesh, Pose pose, Vector3F scale, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled || submesh == null)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      PrimitiveBatch batch;
      if (drawOverScene)
        batch = OverSceneWireFramePrimitiveBatch;
      else if (drawWireFrame)
        batch = InSceneWireFramePrimitiveBatch;
      else if (color.A == 255)
        batch = OpaquePrimitiveBatch;
      else
        batch = TransparentPrimitiveBatch;

      batch.AddSubmesh(submesh, pose, scale, color);
    }


    /// <summary>
    /// Draws a mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawMesh(Mesh mesh, Pose pose, Vector3F scale, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled || mesh == null)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      PrimitiveBatch batch;
      if (drawOverScene)
        batch = OverSceneWireFramePrimitiveBatch;
      else if (drawWireFrame)
        batch = InSceneWireFramePrimitiveBatch;
      else if (color.A == 255)
        batch = OpaquePrimitiveBatch;
      else
        batch = TransparentPrimitiveBatch;

      foreach (var submesh in mesh.Submeshes)
        batch.AddSubmesh(submesh, pose, scale, color);
    }


    /// <overloads>
    /// <summary>
    /// Draws a model (without textures).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Draws a model (without textures).
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawModel(Model model, Pose pose, Vector3F scale, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled || model == null)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      PrimitiveBatch batch;
      if (drawOverScene)
        batch = OverSceneWireFramePrimitiveBatch;
      else if (drawWireFrame)
        batch = InSceneWireFramePrimitiveBatch;
      else if (color.A == 255)
        batch = OpaquePrimitiveBatch;
      else
        batch = TransparentPrimitiveBatch;

      batch.AddModel(model, pose, scale, color);
    }


    /// <summary>
    /// Draws a model (meshes without textures).
    /// </summary>
    /// <param name="sceneNode">The scene node, usually a <see cref="ModelNode"/>.</param>
    /// <param name="color">The color.</param>
    /// <param name="drawWireFrame">
    /// If set to <see langword="true"/> the wire-frame is drawn; otherwise the mesh is drawn with 
    /// solid faces.
    /// </param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true"/> the object is drawn over the graphics scene (depth-test 
    /// disabled).
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Drawing solid objects with disabled depth test is not yet supported.
    /// </exception>
    public void DrawModel(SceneNode sceneNode, Color color, bool drawWireFrame, bool drawOverScene)
    {
      if (!Enabled || sceneNode == null)
        return;

      if (!drawWireFrame && drawOverScene)
        throw new NotSupportedException("Drawing solid objects with disabled depth test is not yet supported.");

      PrimitiveBatch batch;
      if (drawOverScene)
        batch = OverSceneWireFramePrimitiveBatch;
      else if (drawWireFrame)
        batch = InSceneWireFramePrimitiveBatch;
      else if (color.A == 255)
        batch = OpaquePrimitiveBatch;
      else
        batch = TransparentPrimitiveBatch;

      foreach (var meshNode in sceneNode.GetSubtree().OfType<MeshNode>())
        foreach (var submesh in meshNode.Mesh.Submeshes)
          batch.AddSubmesh(submesh, meshNode.PoseWorld, meshNode.ScaleWorld, color);
    }


#if ANIMATION

    /// <overloads>
    /// <summary>
    /// Draws skeleton bones, bone space axes and bone names of a <see cref="MeshNode"/> for debugging.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Draws skeleton bones, bone space axes and bone names of a <see cref="MeshNode"/> for debugging.
    /// </summary>
    /// <param name="meshNode">The mesh node.</param>
    /// <param name="axisLength">The visible length of the bone space axes.</param>
    /// <param name="color">The color for the bones and the bone names.</param>
    /// <param name="drawOverScene">
    /// If set to <see langword="true" /> the object is drawn over the graphics scene (depth-test
    /// disabled).
    /// </param>
    /// <remarks>
    /// This method draws the skeleton for debugging. It draws a line for each bone and the bone
    /// name. At the bone origin it draws 3 lines (red, green, blue) that visualize the bone
    /// space axes (x, y, z).
    /// </remarks>
    public void DrawSkeleton(MeshNode meshNode, float axisLength, Color color, bool drawOverScene)
    {
      if (meshNode == null)
        return;

      DrawSkeleton(meshNode.SkeletonPose, meshNode.PoseWorld, meshNode.ScaleWorld, axisLength, color, drawOverScene);
    }


    /// <summary>
    /// Draws skeleton bones, bone space axes and bone names of a <see cref="MeshNode" /> for debugging.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="pose">The pose (position and orientation) of the skeleton.</param>
    /// <param name="scale">The scale of the skeleton.</param>
    /// <param name="axisLength">The visible length of the bone space axes.</param>
    /// <param name="color">The color for the bones and the bone names.</param>
    /// <param name="drawOverScene">If set to <see langword="true" /> the object is drawn over the graphics scene (depth-test
    /// disabled).</param>
    /// <remarks>
    /// This method draws the skeleton for debugging. It draws a line for each bone and the bone
    /// name. At the bone origin it draws 3 lines (red, green, blue) that visualize the bone
    /// space axes (x, y, z).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    public void DrawSkeleton(SkeletonPose skeletonPose, Pose pose, Vector3F scale, float axisLength, Color color, bool drawOverScene)
    {
      if (!Enabled || skeletonPose == null)
        return;

      LineBatch lineBatch;
      TextBatch textBatch;
      if (drawOverScene)
      {
        lineBatch = OverSceneLineBatch;
        textBatch = OverScene3DTextBatch;
      }
      else
      {
        lineBatch = InSceneLineBatch;
        textBatch = InSceneTextBatch;
      }

      var skeleton = skeletonPose.Skeleton;
      var world = pose * Matrix44F.CreateScale(scale);
      for (int i = 0; i < skeleton.NumberOfBones; i++)
      {
        string name = skeleton.GetName(i);
        SrtTransform bonePose = skeletonPose.GetBonePoseAbsolute(i);
        var translation = bonePose.Translation;
        var rotation = bonePose.Rotation.ToRotationMatrix33();

        var translationWorld = world.TransformPosition(translation);

        int parentIndex = skeleton.GetParent(i);
        if (parentIndex >= 0)
        {
          // Draw line to parent joint representing the parent bone.
          SrtTransform parentPose = skeletonPose.GetBonePoseAbsolute(parentIndex);
          lineBatch.Add(translationWorld, world.TransformPosition(parentPose.Translation), color);
        }

        // Add three lines in Red, Green and Blue.
        lineBatch.Add(translationWorld, world.TransformPosition(translation + rotation.GetColumn(0) * axisLength), Color.Red);
        lineBatch.Add(translationWorld, world.TransformPosition(translation + rotation.GetColumn(1) * axisLength), Color.Green);
        lineBatch.Add(translationWorld, world.TransformPosition(translation + rotation.GetColumn(2) * axisLength), Color.Blue);

        // Draw name.
        if (string.IsNullOrEmpty(name))
          name = "-";

        textBatch.Add(
          string.Format(CultureInfo.InvariantCulture, "{0} {1}", name, i),
          world.TransformPosition(translation + rotation.GetColumn(0) * axisLength * 0.5f),
          new Vector2F(0, 1),
          color);
      }
    }
#endif
    #endregion
  }
}
