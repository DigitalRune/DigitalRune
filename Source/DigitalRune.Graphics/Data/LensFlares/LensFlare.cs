// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a lens flare effect.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A lens flare is an effect caused by a camera lens when looking at bright light. A lens flare
  /// usually consists of multiple elements which move across the screen depending on the position 
  /// of the light source: bloom and halo around the light source, light streaks, secondary rings,
  /// hexagonal patterns caused by the lens' aperture blades. The elements (e.g. a ring, a halo)
  /// is defined by creating a <see cref="LensFlareElement"/> and adding it to the 
  /// <see cref="Elements"/> collection.
  /// </para>
  /// <para>
  /// A <see cref="LensFlareNode"/> needs to be created to define the position and orientation of a
  /// lens flare within a 3D scene.
  /// </para>
  /// <para>
  /// The property <see cref="IsDirectional"/> defines whether the light is caused by a
  /// directional light (such as the sun) or a local light source. The light direction is defined by
  /// the forward direction of the <see cref="LensFlareNode"/>.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// <see cref="LensFlare"/>s are cloneable. When <see cref="Clone()"/> is called all properties 
  /// including the <see cref="Elements"/> are duplicated (deep copy).
  /// </para>
  /// </remarks>
  /// <seealso cref="LensFlareElement"/>
  /// <seealso cref="LensFlareNode"/>
  public class LensFlare : INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the lens flare effect.
    /// </summary>
    /// <value>The name of the lens flare effect.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets the elements of the lens flare.
    /// </summary>
    /// <value>The elements of the lens flare.</value>
    /// <remarks>
    /// A <see cref="LensFlareElement"/> defines a single element of lens flare. Most lens flare
    /// effects consist of multiple elements: halos, streaks, rings or hexagons, ...
    /// </remarks>
    public LensFlareElementCollection Elements { get; private set; }


    /// <summary>
    /// Gets the bounding shape.
    /// </summary>
    /// <value>The bounding shape.</value>
    internal Shape Shape { get; private set; }


    /// <summary>
    /// Gets a value indicating whether the lens flare is caused by a directional light, such as the
    /// sun. (Directional lights are treated as if placed at an infinite distance. See remarks.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the light source is a directional light; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Directional lights are treated as if placed at an infinite distance. The light direction is
    /// defined by the forward direction (see <see cref="Vector3F.Forward"/>) of the 
    /// <see cref="LensFlareNode"/>.
    /// </remarks>
    public bool IsDirectional { get; private set; }


    /// <summary>
    /// Gets or sets the size of the lens flare used in the occlusion query. See remarks.
    /// </summary>
    /// <value>
    /// The size of the lens flare used in the occlusion query. The default value is 0.1 for 
    /// directional lights and 0.5 for local lights. See remarks!
    /// </value>
    /// <remarks>
    /// <para>
    /// The query size is the approximate size of the light source. This value is used in the
    /// occlusion query to determine the visibility of the lens flare effect. The meaning of the 
    /// value depends on the type of lens flare:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <term>Directional lights (IsDirectional = true)</term>
    /// <description>
    /// The query size is the height of the light source relative to the viewport. 
    /// Example: <c>QuerySize = 0.1</c> means that the light source is approximately 1/10 of the viewport.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Local lights (IsDirectional = false)</term>
    /// <description>
    /// The query size is the size of the light source in world space.
    /// Example: <c>QuerySize = 0.5</c> means that the light source is 0.5 units wide.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float QuerySize
    {
      get { return _querySize; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value");

        _querySize = value;
        if (!IsDirectional)
          ((SphereShape)Shape).Radius = new Vector2F(_querySize).Length / 2;
      }
    }
    private float _querySize;


    /// <summary>
    /// Gets or sets the intensity of the lens flare.
    /// </summary>
    /// <value>The intensity of the lens flare. The default value is 1.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float Intensity
    {
      get { return _intensity; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value");

        _intensity = value;
      }
    }
    private float _intensity;


    /// <summary>
    /// Gets or sets the height of the lens flare relative to the viewport.
    /// </summary>
    /// <value>
    /// The height of the lens flare relative to the viewport. The default value is 0.2.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float Size
    {
      get { return _size; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value");

        _size = value;
      }
    }
    private float _size;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="LensFlare" /> class.
    /// </summary>
    /// <param name="isDirectionalLight">
    /// If set to <see langword="true"/>, the lens flare is caused by a a directional light.
    /// (See <see cref="IsDirectional"/> for more info.)
    /// </param>
    public LensFlare(bool isDirectionalLight)
    {
      Elements = new LensFlareElementCollection();
      IsDirectional = isDirectionalLight;
      if (isDirectionalLight)
      {
        _querySize = 0.1f;
        Shape = Shape.Infinite;
      }
      else
      {
        _querySize = 0.5f;
        Shape = new SphereShape(0.5f / 2);
      }

      _intensity = 1;
      _size = 0.2f;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="LensFlare"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="LensFlare"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// See class documentation of <see cref="LensFlare"/> (Section "Cloning") for more information 
    /// about cloning.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="LensFlare"/> derived class and <see cref="CloneCore"/> to create a copy of the 
    /// current instance. Classes that derive from <see cref="LensFlare"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </para>
    /// </remarks>
    public LensFlare Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LensFlare"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method,
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="LensFlare"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private LensFlare CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone LensFlare. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the <see cref="LensFlare"/>
    /// derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="LensFlare"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="LensFlare"/> derived class must 
    /// implement this method. A typical implementation is to simply call the default constructor 
    /// and return the result. 
    /// </para>
    /// </remarks>
    protected virtual LensFlare CreateInstanceCore()
    {
      return new LensFlare(IsDirectional);
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="LensFlare"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="LensFlare"/> derived class must 
    /// implement this method. A typical implementation is to call <c>base.CloneCore(this)</c> to 
    /// copy all properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(LensFlare source)
    {
      Name = source.Name;
      QuerySize = source.QuerySize;
      Intensity = source.Intensity;
      Size = source.Size;
      foreach (var element in source.Elements)
        Elements.Add(element.Clone());
    }
    #endregion


    /// <summary>
    /// Called when the size and intensity of a lens flare is determined.
    /// </summary>
    /// <param name="node">The lens flare node.</param>
    /// <param name="context">The render context.</param>
    /// <param name="visiblePixels">
    /// The number of visible pixels as determined by the last hardware occlusion query. 
    /// (Not available in Reach profile.)
    /// </param>
    /// <param name="totalPixels">
    /// The total number of pixels tested in the hardware occlusion query. 
    /// (Not available in Reach profile.)
    /// </param>
    /// <param name="size">
    /// Out: The actual size of the lens flare in relative to the viewport.
    /// </param>
    /// <param name="intensity">Out: The actual intensity of the lens flare.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong><br/>
    /// This method can be overridden in derived types to adjust the size and intensity of the lens
    /// flare. The base implementation creates a lens flare with constant size. The intensity
    /// depends on the number of visible pixels and the fog.
    /// <code lang="csharp">
    /// <![CDATA[
    /// protected virtual void OnGetSizeAndIntensity(LensFlareNode node, RenderContext context, int visiblePixels, int totalPixels, out float size, out float intensity)
    /// {
    ///   // Constant size.
    ///   var size = Size;
    ///   
    ///   intensity = node.Intensity * Intensity;
    ///    
    ///   // Intensity depends on the number of visible (unoccluded) pixels.
    ///   if (context.GraphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
    ///     intensity *= (float)visiblePixels / totalPixels;
    ///   
    ///   // Fog decreases the intensity.
    ///   var scene = context.Scene;
    ///   var cameraNode = context.CameraNode;
    ///   if (scene != null && cameraNode != null)
    ///   {
    ///     var query = scene.Query<FogQuery>(cameraNode);
    ///     foreach (var fogNode in query.FogNodes)
    ///     {
    ///       var flarePosition = IsDirectional   // For directional flares, choose a position "far" away.
    ///                         ? cameraNode.PoseWorld.Position + node.PoseWorld.Orientation.GetColumn(2) * cameraNode.Camera.Projection.Far
    ///                         : node.PoseWorld.Position;
    ///       intensity *= (1 - fogNode.Fog.GetIntensity(fogNode, cameraNode, flarePosition));
    ///     }
    ///   }
    /// }
    /// ]]>
    /// </code>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected internal virtual void OnGetSizeAndIntensity(LensFlareNode node, RenderContext context, int visiblePixels, int totalPixels, out float size, out float intensity)
    {
      // Constant size.
      size = Size;

      intensity = node.Intensity * Intensity;

      // Intensity depends on the number of visible (unoccluded) pixels.
      if (context.GraphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
        intensity *= (float)visiblePixels / totalPixels;

      // Fog decreases the intensity.
      var scene = context.Scene;
      var cameraNode = context.CameraNode;
      if (scene != null && cameraNode != null)
      {
        var query = scene.Query<FogQuery>(cameraNode, context);
        foreach (var fogNode in query.FogNodes)
        {
          var flarePosition = IsDirectional  // For directional flares, choose a position "far" away ;-)
                              ? cameraNode.PoseWorld.Position + node.PoseWorld.Orientation.GetColumn(2) * cameraNode.Camera.Projection.Far
                              : node.PoseWorld.Position;
          intensity *= (1 - fogNode.Fog.GetIntensity(fogNode, cameraNode, flarePosition));
        }
      }
    }
    #endregion
  }
}
