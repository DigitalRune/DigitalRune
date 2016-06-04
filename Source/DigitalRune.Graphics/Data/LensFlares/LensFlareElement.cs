// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines an element of a lens flare.
  /// </summary>
  /// <remarks>
  /// <see cref="LensFlareElement"/>s need to be added to the <see cref="LensFlare.Elements"/>
  /// collection of a <see cref="LensFlare"/>.
  /// </remarks>
  /// <seealso cref="LensFlare"/>
  public class LensFlareElement
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the color of the element.
    /// </summary>
    /// <value>The color of the element. The default value is opaque white.</value>
    public Color Color { get; set; }


    /// <summary>
    /// The distance of the element.
    /// </summary>
    /// <value>
    /// The distance of the element: 0 = light source, 1 = center of screen.
    /// Distance can be negative or greater than 1. The default value is 0.
    /// </value>
    public float Distance { get; set; }


    /// <summary>
    /// Gets or sets the origin relative to the image.
    /// </summary>
    /// <value>
    /// The origin relative to the image, where (0, 0) is the upper-left corner of the image and
    /// (1, 1) is the lower-right corner of the image. The default value is (0.5, 0.5).
    /// </value>
    public Vector2F Origin { get; set; }


    /// <summary>
    /// Gets or sets the angle (in radians) to rotate the element.
    /// </summary>
    /// <value>
    /// The angle (in radians) to rotate the element around its origin. <see cref="float.NaN"/> 
    /// can be set to automatically rotate the element depending on the position of the light 
    /// source. The default value is 0.
    /// </value>
    public float Rotation { get; set; }


    /// <summary>
    /// Gets or sets the scale of the element relative to <see cref="LensFlare.Size"/>. 
    /// </summary>
    /// <value>
    /// The scale of the element relative to <see cref="LensFlare.Size"/>. The default value is 
    /// (1, 1).
    /// </value>
    public Vector2F Scale { get; set; }


    /// <summary>
    /// Gets or sets the texture.
    /// </summary>
    /// <value>The texture. The default value is <see langword="null" />.</value>
    public PackedTexture Texture { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="LensFlareElement" /> class.
    /// </summary>
    public LensFlareElement()
    {
      Scale = Vector2F.One;
      Color = Color.White;
      Origin = new Vector2F(0.5f, 0.5f);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LensFlareElement" /> class.
    /// </summary>
    /// <param name="distance">
    /// The distance of the element: 0 = light source, 1 = center of screen.
    /// Distance can be negative or greater than 1. The default value is 0.
    /// </param>
    /// <param name="scale">
    /// The scale of the element relative to <see cref="LensFlare.Size"/>.
    /// </param>
    /// <param name="rotation">
    /// The angle (in radians) to rotate the element around its center. <see cref="float.NaN"/> 
    /// can be set to automatically rotate the element depending on the position of the light 
    /// source.
    /// </param>
    /// <param name="color">The color of the element.</param>
    /// <param name="origin">
    /// The origin relative to the image, where (0, 0) is the upper-left corner of the image and
    /// (1, 1) is the lower-right corner of the image.
    /// </param>
    /// <param name="texture">The texture containing the image.</param>
    public LensFlareElement(float distance, float scale, float rotation, Color color, Vector2F origin, PackedTexture texture)
      : this(distance, new Vector2F(scale), rotation, color, origin, texture)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LensFlareElement" /> class.
    /// </summary>
    /// <param name="distance">
    /// The distance of the element: 0 = light source, 1 = center of screen.
    /// Distance can be negative or greater than 1. The default value is 0.
    /// </param>
    /// <param name="scale">
    /// The scale of the element relative to <see cref="LensFlare.Size"/>.
    /// </param>
    /// <param name="rotation">
    /// The angle (in radians) to rotate the element around its center. <see cref="float.NaN"/> 
    /// can be set to automatically rotate the element depending on the position of the light 
    /// source.
    /// </param>
    /// <param name="color">The color of the element.</param>
    /// <param name="origin">
    /// The origin relative to the image, where (0, 0) is the upper-left corner of the image and
    /// (1, 1) is the lower-right corner of the image.
    /// </param>
    /// <param name="texture">The texture containing the image.</param>
    public LensFlareElement(float distance, Vector2F scale, float rotation, Color color, Vector2F origin, PackedTexture texture)
    {
      Distance = distance;
      Scale = scale;
      Rotation = rotation;
      Color = color;
      Origin = origin;
      Texture = texture;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="LensFlareElement"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="LensFlareElement"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="LensFlareElement"/> derived class and <see cref="CloneCore"/> to create a copy of the 
    /// current instance. Classes that derive from <see cref="LensFlareElement"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </remarks>
    public LensFlareElement Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LensFlareElement"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method,
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="LensFlareElement"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private LensFlareElement CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone LensFlareElement. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="LensFlareElement"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="LensFlareElement"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="LensFlareElement"/> derived class 
    /// must implement this method. A typical implementation is to simply call the default 
    /// constructor and return the result. 
    /// </para>
    /// </remarks>
    protected virtual LensFlareElement CreateInstanceCore()
    {
      return new LensFlareElement();
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="LensFlareElement"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="LensFlareElement"/> derived class 
    /// must implement this method. A typical implementation is to call <c>base.CloneCore(this)</c> 
    /// to copy all properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(LensFlareElement source)
    {
      Color = source.Color;
      Distance = source.Distance;
      Origin = source.Origin;
      Rotation = source.Rotation;
      Scale = source.Scale;
      Texture = source.Texture;
    }
    #endregion
    
    #endregion
  }
}
