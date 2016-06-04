// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
#if NETFX_CORE || NET45
using System.Reflection;
#endif
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if PORTABLE || WINDOWS_UWP
#pragma warning disable 1574  // Disable warning "XML comment has cref attribute that could not be resolved."
#endif


namespace DigitalRune.Game.UI.Rendering
{
  /// <summary>
  /// Manages and renders the visual appearance of a UI. (Default implementation.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class implements <see cref="IUIRenderer"/> (see comments of <see cref="IUIRenderer"/>). 
  /// </para>
  /// <para>
  /// When creating the <see cref="UIRenderer"/> a UI theme (see <see cref="Theme"/>) must be 
  /// specified. The renderer will use the attributes and styles of the theme to render the 
  /// controls.
  /// </para>
  /// <para>
  /// <strong>Thread-Safety:</strong> This class is not thread-safe. <see cref="Render"/> must not 
  /// be called simultaneously in concurrent threads.
  /// </para>
  /// <para>
  /// <strong>Render Callbacks:</strong> This class has a dictionary <see cref="RenderCallbacks"/>
  /// which defines the methods used for rendering. When 
  /// <see cref="Render(UIControl, UIRenderContext)"/> is called, the style of the control is
  /// determined and the render callback for the style is used to render the control. If a render
  /// method is not given for a given style, the parent styles are used (a <see cref="ThemeStyle"/>
  /// can inherit from another style, see <see cref="ThemeStyle.Inherits"/>). See also 
  /// <see cref="RenderCallbacks"/>.
  /// </para>
  /// </remarks>
  public partial class UIRenderer : IUIRenderer, IDisposable
  {
    // Controls are rendered back to front because we use alpha blending (text and glow images).
    // TODO: Cache info in UIControl.RenderData. Check UIControl IsVisualValid to see if the cache must be updated.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Copy from CullNone but with activate scissors test.
    private static readonly RasterizerState CullNoneWithScissors = new RasterizerState
    {
      Name = "UIRenderer.CullNoneWithScissors",
      CullMode = RasterizerState.CullNone.CullMode,
      DepthBias = RasterizerState.CullNone.DepthBias,
      FillMode = RasterizerState.CullNone.FillMode,
      MultiSampleAntiAlias = RasterizerState.CullNone.MultiSampleAntiAlias,
      SlopeScaleDepthBias = RasterizerState.CullNone.SlopeScaleDepthBias,
      ScissorTestEnable = true,
    };

    private object _defaultCursor;
    private SpriteFont _defaultFont;
    private Texture2D _defaultTexture;
    private bool _batchIsActive;
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


    /// <inheritdoc/>
    public GraphicsDevice GraphicsDevice { get; set; }


    /// <inheritdoc/>
    public SpriteBatch SpriteBatch { get; private set; }


    /// <summary>
    /// Gets the UI theme.
    /// </summary>
    /// <value>The UI theme.</value>
    public Theme Theme { get; private set; }


    /// <inheritdoc/>
    public Dictionary<string, GameObject> Templates
    {
      get { return _templates; }
    }
    private readonly Dictionary<string, GameObject> _templates = new Dictionary<string, GameObject>();
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

#if !SILVERLIGHT
    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="UIRenderer"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="UIRenderer"/> class.
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="game">The XNA Game instance.</param>
    /// <param name="theme">The loaded UI theme.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="theme"/> is <see langword="null"/>.
    /// </exception>
    public UIRenderer(Microsoft.Xna.Framework.Game game, Theme theme)
    {
      if (game == null)
        throw new ArgumentNullException("game");
      if (theme == null)
        throw new ArgumentNullException("theme");

      GraphicsDevice = game.GraphicsDevice;
      SpriteBatch = new SpriteBatch(GraphicsDevice);
      Theme = theme;

      InitializeDefaultCursor();
      InitializeDefaultFont();
      InitializeDefaultTexture();
      InitializeRendering();
    }
#endif


    /// <summary>
    /// Initializes a new instance of the <see cref="UIRenderer"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="theme">The loaded UI theme.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> or <paramref name="theme"/> is <see langword="null"/>.
    /// </exception>
    public UIRenderer(GraphicsDevice graphicsDevice, Theme theme)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (theme == null)
        throw new ArgumentNullException("theme");

      GraphicsDevice = graphicsDevice;
      SpriteBatch = new SpriteBatch(GraphicsDevice);
      Theme = theme;

      InitializeDefaultCursor();
      InitializeDefaultFont();
      InitializeDefaultTexture();
      InitializeRendering();
    }


    private void InitializeDefaultCursor()
    {
#if !WP7 && !XBOX
      if (Theme.Cursors != null)
      {
        // Get cursor with attribute "IsDefault=true".
        _defaultCursor = Theme.Cursors
                              .Where(c => c.IsDefault)
                              .Select(c => c.Cursor)
                              .FirstOrDefault();
        if (_defaultCursor == null)
        {
          // The theme does not define a font with "IsDefault=true".
          // Check if a font is named "Default".
          _defaultCursor = Theme.Cursors
                                .Where(c => c.Name.Equals("default", StringComparison.OrdinalIgnoreCase))
                                .Select(c => c.Cursor)
                                .FirstOrDefault();
        }
      }

      if (_defaultCursor == null)
      {
        // Nothing found - use the default windows cursor.
        _defaultCursor = PlatformHelper.DefaultCursor;
      }
#else
      _defaultCursor = null;
#endif
    }


    private void InitializeDefaultFont()
    {
      if (Theme.Fonts != null)
      {
        _defaultFont = Theme.Fonts
                            .Where(f => f.IsDefault)
                            .Select(f => f.Font)
                            .FirstOrDefault();
        if (_defaultFont == null)
        {
          // The theme does not define a font with "IsDefault=true".
          // Check if a font is named "Default".
          _defaultFont = Theme.Fonts
                              .Where(f => f.Name.Equals("default", StringComparison.OrdinalIgnoreCase))
                              .Select(f => f.Font)
                              .FirstOrDefault();
        }
        if (_defaultFont == null)
        {
          // No default font found so far. --> Just use the first available font.
          _defaultFont = Theme.Fonts.Select(f => f.Font).FirstOrDefault();
        }
      }
     
      if (_defaultFont == null)
        throw new UIException("No default font found.");
    }


    private void InitializeDefaultTexture()
    {
      if (Theme.Textures != null)
      {
        _defaultTexture = Theme.Textures
                               .Where(t => t.IsDefault)
                               .Select(t => t.Texture)
                               .FirstOrDefault();
        if (_defaultTexture == null)
        {
          // The theme does not define a texture with "IsDefault=true".
          // Check if a texture is named "Default".
          _defaultTexture = Theme.Textures
                                 .Where(t => t.Name.Equals("default", StringComparison.OrdinalIgnoreCase))
                                 .Select(t => t.Texture)
                                 .FirstOrDefault();
        }
        if (_defaultTexture == null)
        {
          // No default texture found so far. --> Just use the first available texture.
          _defaultTexture = Theme.Textures.Select(t => t.Texture).FirstOrDefault();
        }
      }

      if (_defaultTexture == null)
        throw new UIException("No default texture found.");
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="UIRenderer"/> class.
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
    /// Releases the unmanaged resources used by an instance of the <see cref="UIRenderer"/> class 
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_whiteTexture")]
    protected virtual void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          SpriteBatch.Dispose();
          _whiteTexture.SafeDispose();
        }

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a style-specific attribute value.
    /// </summary>
    /// <typeparam name="T">The type of the attribute value.</typeparam>
    /// <param name="style">The style.</param>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="result">The attribute value.</param>
    /// <returns>
    /// <see langword="true"/> if the renderer can provide a value for the attribute; otherwise, 
    /// <see langword="false"/> if the renderer does not know the style or the attribute.
    /// </returns>
    /// <remarks>
    /// This method calls <see cref="OnParseAttribute{T}"/> to convert a 
    /// <see cref="ThemeAttribute"/> to a value of type <typeparamref name="T"/>.
    /// </remarks>
    public bool GetAttribute<T>(string style, string name, out T result)
    {
      result = default(T);

      if (style == null)
        return false;
      if (string.IsNullOrEmpty(name))
        return false;

      // Get style.
      ThemeStyle themeStyle;
      bool found = Theme.Styles.TryGet(style, out themeStyle);
      if (!found)
        return false;

      // Search for attribute including style inheritance.
      ThemeAttribute attribute = null;
      while (attribute == null && themeStyle != null)
      {
        if (!themeStyle.Attributes.TryGet(name, out attribute))
        {
          // Try ancestor.
          themeStyle = themeStyle.Inherits;
        }
      }

      if (attribute == null)
        return false;

      return OnParseAttribute(attribute, out result);
    }


    /// <summary>
    /// Called by <see cref="GetAttribute{T}"/> to convert attributes to values.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="attribute">The attribute.</param>
    /// <param name="result">The parsed value.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="ThemeAttribute.Value"/> could be converted to the
    /// type <typeparamref name="T"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The base implementation of this method supports following types: <see cref="Vector4F"/>,
    /// <see cref="Color"/>, <see cref="Rectangle"/>, <see cref="Rectangle"/>?,
    /// <see cref="Texture2D"/>, enumerations, types that have a <see cref="TypeConverter"/>, and
    /// types that implement <see cref="IConvertible"/>.
    /// </para>
    /// <para>
    /// Derived classes can override this method to add support for additional types.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected bool OnParseAttribute<T>(ThemeAttribute attribute, out T result)
    {
      result = default(T);

      if (result is Vector2F)
      {
        result = (T)(object)ThemeHelper.ParseVector2F(attribute.Value);
      }
      else if (result is Vector3F)
      {
        result = (T)(object)ThemeHelper.ParseVector3F(attribute.Value);
      }
      else if (result is Vector4F)
      {
        result = (T)(object)ThemeHelper.ParseVector4F(attribute.Value);
      }
      else if (typeof(T) == typeof(string) || typeof(T) == typeof(object))
      {
        result = (T)(object)attribute.Value;
      }
      else if (result is Color)
      {
        result = (T)(object)ThemeHelper.ParseColor(attribute.Value, Color.Black);
      }
#if !NETFX_CORE && !NET45
      else if (typeof(T).IsAssignableFrom(typeof(Rectangle)))
#else
      else if (typeof(T).GetTypeInfo().IsAssignableFrom(typeof(Rectangle).GetTypeInfo()))
#endif      
      {
        result = (T)(object)ThemeHelper.ParseRectangle(attribute.Value);
      }
#if !NETFX_CORE && !NET45
      else if (typeof(T).IsAssignableFrom(typeof(Texture2D)))
#else
      else if (typeof(T).GetTypeInfo().IsAssignableFrom(typeof(Texture2D).GetTypeInfo()))
#endif
      {
        result = (T)(object)GetTexture(attribute.Value);
      }
      else
      {
        try
        {
          result = ObjectHelper.Parse<T>(attribute.Value);
        }
        catch
        {
          return false;
        }
      }

      return true;
    }


    /// <inheritdoc/>
    public object GetCursor(string name)
    {
      if (string.IsNullOrEmpty(name))
        return _defaultCursor;

      ThemeCursor cursor;
      bool exists = Theme.Cursors.TryGet(name, out cursor);
      return (exists) ? cursor.Cursor : _defaultCursor;
    }


    /// <inheritdoc/>
    public SpriteFont GetFont(string name)
    {
      if (string.IsNullOrEmpty(name))
        return _defaultFont;

      ThemeFont font;
      bool exists = Theme.Fonts.TryGet(name, out font);
      return (exists) ? font.Font : _defaultFont;
    }


    /// <inheritdoc/>
    public Texture2D GetTexture(string name)
    {
      if (string.IsNullOrEmpty(name))
        return _defaultTexture;

      ThemeTexture texture;
      bool exists = Theme.Textures.TryGet(name, out texture);
      return (exists) ? texture.Texture : _defaultTexture;
    }


    /// <inheritdoc/>
    public void BeginBatch()
    {
      if (!_batchIsActive)
      {
        SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                          DepthStencilState.None, CullNoneWithScissors);
        _batchIsActive = true;
      }
    }


    /// <inheritdoc/>
    public void EndBatch()
    {
      if (_batchIsActive)
      {
        SpriteBatch.End();
        _batchIsActive = false;
      }
    }
    #endregion
  }
}
