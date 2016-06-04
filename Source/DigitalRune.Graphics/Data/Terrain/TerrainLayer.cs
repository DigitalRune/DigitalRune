// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a material layer (detail textures, decals, roads, etc.) of the terrain.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A terrain is split into one or more tiles. The terrain tiles defines the geometry (height,
  /// normals, holes) of the terrain. Each tile has a set of material layers (dirt, grass, decals,
  /// roads) that define the appearance. The material layers are applied (blended) one after the
  /// other, which means that a layer can override previous layers.
  /// </para>
  /// <para>
  /// Example: The first layer draws a dirt texture that covers the whole tile. The second layer
  /// draws a grass texture, which covers only parts of the tile defined by a blend map. Additional
  /// layers add roads and decals like dirt, leaves, sewer grates, etc.
  /// </para>
  /// <para>
  /// Each terrain tile can have its own set of terrain layers, but they can also share the same
  /// <see cref="TerrainLayer"/> instances.
  /// </para>
  /// <para>
  /// <strong>Materials:</strong><br/>
  /// Each <see cref="TerrainLayer"/> has a <see cref="Material"/> which is used to render the layer
  /// information. The <see cref="TerrainClipmapRenderer"/> uses the material to render the layer
  /// information into <see cref="TerrainClipmap"/>s. The <see cref="TerrainClipmapRenderer"/>
  /// requires that the material has a render pass called "Base" when it wants to render
  /// information, such as heights, into the <see cref="TerrainNode.BaseClipmap"/> and a render pass
  /// called "Detail" when it wants to render information, such as a grass texture, into the
  /// <see cref="TerrainNode.DetailClipmap"/>.
  /// </para>
  /// <para>
  /// <strong>Fade-in/out by distance:</strong><br/>
  /// The properties <see cref="FadeInStart"/>, <see cref="FadeInEnd"/>, <see cref="FadeOutStart"/>,
  /// <see cref="FadeOutEnd"/> can be used to define into which clipmap levels the layer is
  /// rendered. For example if (<see cref="FadeInStart"/>, <see cref="FadeInEnd"/>,
  /// <see cref="FadeOutStart"/>, <see cref="FadeOutEnd"/>) is (1, 3, 5, 7), then the layer is
  /// rendered into the clipmap level 1 with a low opacity. The opacity increases in level 2 and
  /// reaches 100% in level 3. The layer is rendered with full opacity in level 4. Then the opacity
  /// decreases again and reaches 0% at level 7. The default values are (0, 0,
  /// <see cref="int.MaxValue"/>, <see cref="int.MaxValue"/>) which means that fading is disabled
  /// and the terrain layer is always visible at all distances.
  /// </para>
  /// <para>
  /// These fade-in/out properties can be used to render details, like decals, only near the camera.
  /// It can also be used to have one layer draw a detailed rock texture only near the camera.
  /// Another layer can render a low resolution rock texture only in the distance.
  /// </para>
  /// <para>
  /// The fade-in/out properties are based on clipmap levels and not view-distance. (The reason: If
  /// the distance depends on the camera, then all cached clipmaps have to be redrawn when the
  /// camera moves. This needs to be avoided.)
  /// </para>
  /// <para>
  /// <strong>Cache invalidation:</strong><br/>
  /// When the <see cref="Terrain"/> is used with the <see cref="TerrainNode"/>, then the terrain
  /// data is cached in clipmaps. Therefore, it is important to notify the terrain system when a
  /// tile or layer has changed and the cached data is invalid. When tiles or layers are added to or
  /// removed from the terrain, this happens automatically. But when the properties or the contents
  /// of tiles/layers are changed, the affected region needs to be invalidated explicitly by calling
  /// the appropriate <see cref="Terrain.Invalidate()"/> method of the <see cref="Terrain"/> or the
  /// <see cref="TerrainTile"/>. For example, when the contents of a height map is changed, the
  /// affected region on the terrain needs to be invalidated by calling
  /// <see cref="Terrain.Invalidate(DigitalRune.Geometry.Shapes.Aabb)"/> or
  /// <see cref="Terrain.Invalidate(TerrainTile)"/>.
  /// </para>
  /// <para>
  /// <strong>Disposing:</strong><br/>
  /// <see cref="TerrainLayer"/>s are disposable. Derived classes should dispose all auto-generated
  /// resources. Resources set by the user or loaded via a content manager are not disposed by the
  /// <see cref="TerrainLayer"/>.
  /// </para>
  /// </remarks>
  public abstract class TerrainLayer : IDisposable, IInternalTerrainLayer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
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
    /// Gets (or sets) the axis-aligned bounding box of the area that is influenced by this terrain
    /// layer.
    /// </summary>
    /// <value>
    /// The axis-aligned bounding box of the area that is influenced by this terrain layer. The
    /// default value is <see langword="null"/>, which means that the terrain layer affects the
    /// entire terrain tile.
    /// </value>
    /// <remarks>
    /// See <see cref="TerrainLayer"/> for more details.
    /// </remarks>
    public Aabb? Aabb { get; protected set; }


    /// <summary>
    /// Gets (or sets) the material that is used to render this terrain layer.
    /// </summary>
    /// <value>
    /// The material that is used to render this terrain layer. Must not be <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// A <see cref="Material"/> can be shared by multiple terrain layers.
    /// </para>
    /// <para>
    /// See <see cref="TerrainLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <param name="value"> is <see langword="null"/>.
    /// </param>
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public Material Material
    {
      get { return _material; }
      protected set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _material = value;
        MaterialInstance = new MaterialInstance(Material);
      }
    }
    private Material _material;


    /// <summary>
    /// Gets the material instance.
    /// </summary>
    /// <value>The material instance.</value>
    /// <remarks>
    /// The <see cref="MaterialInstance"/> is unique to the terrain layer. When effect parameters
    /// in the material instance are changed, only the current terrain layer is affected.
    /// </remarks>
    public MaterialInstance MaterialInstance { get; private set; }


    /// <summary>
    /// Gets or sets the clipmap level where this terrain layer starts to fade in.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The clipmap level where this layer starts to fade in.
    /// The default value is 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <seealso cref="FadeInEnd"/>
    /// <seealso cref="FadeOutStart"/>
    /// <seealso cref="FadeOutEnd"/>
    public int FadeInStart
    {
      get
      {
        int value;
        if (TryGetParameter("FadeInStart", out value))
          return value;

        return _fadeInStart;
      }
      set
      {
        _fadeInStart = value;
        TrySetParameter("FadeInStart", value);
      }
    }
    private int _fadeInStart;



    /// <summary>
    /// Gets or sets the clipmap level where the fade-in ends and this terrain layer is fully
    /// visible. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The clipmap level where the fade-in ends and this terrain layer is fully visible. The
    /// default value is 0, which means that the terrain is immediately visible.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <seealso cref="FadeInStart"/>
    /// <seealso cref="FadeOutStart"/>
    /// <seealso cref="FadeOutEnd"/>
    public int FadeInEnd
    {
      get
      {
        int value;
        if (TryGetParameter("FadeInEnd", out value))
          return value;

        return _fadeInEnd;
      }
      set
      {
        _fadeInEnd = value;
        TrySetParameter("FadeInEnd", value);
      }
    }
    private int _fadeInEnd;


    /// <summary>
    /// Gets or sets the clipmap level where this terrain layer starts to fade out.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The clipmap level where this terrain layer starts to fade out. The default value is
    /// <see cref="int.MaxValue"/>, which means that the terrain layer does not fade out.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <seealso cref="FadeInStart"/>
    /// <seealso cref="FadeInEnd"/>
    /// <seealso cref="FadeOutEnd"/>
    public int FadeOutStart
    {
      get
      {
        int value;
        if (TryGetParameter("FadeOutStart", out value))
          return value;

        return _fadeOutStart;
      }
      set
      {
        _fadeOutStart = value;
        TrySetParameter("FadeOutStart", value);
      }
    }
    private int _fadeOutStart;


    /// <summary>
    /// Gets or sets the clipmap level where the fade-out ends and this terrain layer is not
    /// rendered anymore. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The clipmap level where the fade-out ends and this terrain layer is not rendered anymore.
    /// The default value is <see cref="int.MaxValue"/>, which means that the terrain layer does not
    /// fade out.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <seealso cref="FadeInStart"/>
    /// <seealso cref="FadeInEnd"/>
    /// <seealso cref="FadeOutStart"/>
    public int FadeOutEnd
    {
      get
      {
        int value;
        if (TryGetParameter("FadeOutEnd", out value))
          return value;

        return _fadeOutEnd;
      }
      set
      {
        _fadeOutEnd = value;
        TrySetParameter("FadeOutEnd", value);
      }
    }
    private int _fadeOutEnd;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainLayer"/> class.
    /// </summary>
    protected TerrainLayer()
    {
      _fadeOutStart = int.MaxValue;
      _fadeOutEnd = int.MaxValue;
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="TerrainLayer"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in
    /// <see langword="true"/>, and then suppresses finalization of the instance.
    /// </para>
    /// <para>
    /// See <see cref="TerrainLayer"/> for more details.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="TerrainLayer"/> class
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    /// <remarks>
    /// See <see cref="TerrainLayer"/> for more details.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
        }

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    void IInternalTerrainLayer.OnDraw(GraphicsDevice graphicsDevice, Rectangle rectangle, Vector2F topLeftPosition, Vector2F bottomRightPosition)
    {
      OnDraw(graphicsDevice, rectangle, topLeftPosition, bottomRightPosition);
    }


    /// <summary>
    /// Called when the <see cref="TerrainClipmapRenderer"/> wants to draw this layer into a
    /// clipmap.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="rectangle">
    /// The screen-space rectangle of the clipmap into which to render.
    /// </param>
    /// <param name="topLeftPosition">
    /// The world space position of the top-left corner of the <paramref name="rectangle"/>.
    /// </param>
    /// <param name="bottomRightPosition">
    /// The world space position of the bottom-right corner of the <paramref name="rectangle"/>.
    /// </param>
    /// <remarks>
    /// The default implementation renders a single screen-space quad covering the
    /// <paramref name="rectangle"/>. The vertex shader will get the screen-space position in the
    /// "POSITION0" vertex attribute and the world space position in the "TEXCOORD0" vertex
    /// attribute.
    /// </remarks>
    internal virtual void OnDraw(GraphicsDevice graphicsDevice, Rectangle rectangle, Vector2F topLeftPosition, Vector2F bottomRightPosition)
    {
      graphicsDevice.DrawQuad(rectangle, topLeftPosition, bottomRightPosition);
    }


    // Try to read parameter from Base or Detail render pass.
    private bool TryGetParameter<T>(string name, out T value)
    {
      if (TryGetParameter(false, name, out value))
        return true;

      if (TryGetParameter(true, name, out value))
        return true;

      return false;
    }


    private bool TryGetParameter<T>(bool useDetailPass, string name, out T value)
    {
      var parameterBinding = TryGetParameterBinding<T>(useDetailPass, name);
      if (parameterBinding != null)
      {
        value = parameterBinding.Value;
        return true;
      }

      value = default(T);
      return false;
    }


    private void TrySetParameter<T>(string name, T value)
    {
      TrySetParameter(false, name, value);
      TrySetParameter(true, name, value);
    }


    private void TrySetParameter<T>(bool useDetailPass, string name, T value)
    {
      var parameterBinding = TryGetParameterBinding<T>(useDetailPass, name);
      if (parameterBinding != null)
        parameterBinding.Value = value;
    }


    /// <summary>
    /// Gets an effect parameter value from the material.
    /// </summary>
    /// <typeparam name="T">The type of the effect parameter.</typeparam>
    /// <param name="useDetailPass">
    /// <see langword="false"/> to use the "Base" render pass.
    /// <see langword="true"/> to use the "Detail" render pass.
    /// </param>
    /// <param name="name">The effect parameter name.</param>
    /// <returns>The value of the effect parameter as stored in the material.</returns>
    /// <exception cref="GraphicsException">
    /// The material does not contain the specified render pass.<br/>
    /// Or, the effect does not have the specified effect parameter.<br/>
    /// Or, the effect parameter binding is not a <see cref="ConstParameterBinding{T}"/>.<br/>
    /// Or, the effect parameter is of a different type.
    /// </exception>
    internal T GetParameter<T>(bool useDetailPass, string name)
    {
      var parameterBinding = GetParameterBinding<T>(useDetailPass, name);
      return parameterBinding.Value;
    }


    /// <summary>
    /// Sets an effect parameter value in the material's "Base" and "Detail" pass.
    /// </summary>
    /// <typeparam name="T">The type of the effect parameter.</typeparam>
    /// <param name="name">The effect parameter name.</param>
    /// <param name="value">The effect parameter value.</param>
    /// <exception cref="GraphicsException">
    /// The material does not contain the specified render pass.<br/>
    /// Or, the effect does not have the specified effect parameter.<br/>
    /// Or, the effect parameter binding is not a <see cref="ConstParameterBinding{T}"/>.<br/>
    /// Or, the effect parameter is of a different type.
    /// </exception>
    internal void SetParameter<T>(string name, T value)
    {
      SetParameter(false, name, value);
      SetParameter(true, name, value);
    }


    /// <summary>
    /// Sets an effect parameter value in the material.
    /// </summary>
    /// <typeparam name="T">The type of the effect parameter.</typeparam>
    /// <param name="useDetailPass">
    /// <see langword="false"/> to use the "Base" render pass.
    /// <see langword="true"/> to use the "Detail" render pass.
    /// </param>
    /// <param name="name">The effect parameter name.</param>
    /// <param name="value">The effect parameter value.</param>
    /// <exception cref="GraphicsException">
    /// The material does not contain the specified render pass.<br/>
    /// Or, the effect does not have the specified effect parameter.<br/>
    /// Or, the effect parameter binding is not a <see cref="ConstParameterBinding{T}"/>.<br/>
    /// Or, the effect parameter is of a different type.
    /// </exception>
    internal void SetParameter<T>(bool useDetailPass, string name, T value)
    {
      var parameterBinding = GetParameterBinding<T>(useDetailPass, name);
      parameterBinding.Value = value;
    }


    private ConstParameterBinding<T> TryGetParameterBinding<T>(bool useDetailPass, string parameterName)
    {
      string renderPass = useDetailPass ? "Detail" : "Base";
      EffectBinding effectBinding;
      if (!Material.TryGet(renderPass, out effectBinding))
        return null;

      var parameterBindingUntyped = effectBinding.ParameterBindings[parameterName];
      if (parameterBindingUntyped == null)
        return null;

      var parameterBinding = parameterBindingUntyped as ConstParameterBinding<T>;
      if (parameterBinding == null)
      {
        // Special handling of Texture2D which is often stored as Texture:
        // Replace ConstParameterBinding<Texture> with ConstParameterBinding<Texture2D>.
        if (typeof(T) == typeof(Texture2D))
        {
          var parameterBinding2 = effectBinding.ParameterBindings[parameterName] as ConstParameterBinding<Texture>;
          if (parameterBinding2 != null)
          {
            var value = (T)(object)parameterBinding2.Value;
            parameterBinding = effectBinding.Set(parameterBinding2.Parameter, value);
          }
        }
      }

      return parameterBinding;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ConstParameterBinding")]
    private ConstParameterBinding<T> GetParameterBinding<T>(bool useDetailPass, string parameterName)
    {
      string renderPass = useDetailPass ? "Detail" : "Base";
      EffectBinding effectBinding;
      if (!Material.TryGet(renderPass, out effectBinding))
        throw new GraphicsException(
          string.Format(
            CultureInfo.InvariantCulture,
            "Cannot access effect parameter '{0}' from the layer material because the material " +
            "material does not have a render pass '{1}'.", parameterName, renderPass));

      var parameterBindingUntyped = effectBinding.ParameterBindings[parameterName];
      if (parameterBindingUntyped == null)
        throw new GraphicsException(
            string.Format(
              CultureInfo.InvariantCulture,
              "Cannot access effect parameter '{0}' from the layer material because the effect " +
              "does not have this parameter.", parameterName));

      var parameterBinding = parameterBindingUntyped as ConstParameterBinding<T>;
      if (parameterBinding == null)
      {
        // Special handling of Texture2D which is often stored as Texture:
        // Replace ConstParameterBinding<Texture> with ConstParameterBinding<Texture2D>.
        if (typeof(T) == typeof(Texture2D))
        {
          var parameterBinding2 = effectBinding.ParameterBindings[parameterName] as ConstParameterBinding<Texture>;
          if (parameterBinding2 != null)
          {
            var value = (T)(object)parameterBinding2.Value;
            parameterBinding = effectBinding.Set(parameterBinding2.Parameter, value);
          }
        }
      }

      if (parameterBinding == null)
        throw new GraphicsException(
          string.Format(
            CultureInfo.InvariantCulture,
            "Cannot access effect parameter '{0}' (expected type: '{1}') from the layer material " +
            "because the parameter binding is not a ConstParameterBinding or the effect parameter is " +
            "of a different type.", parameterName, typeof(T).Name));

      return parameterBinding;
    }
    #endregion
  }
}
