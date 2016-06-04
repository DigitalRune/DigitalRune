// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a 2D image rendered in screen space.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A sprite is a 2D image, such as a bitmap or a text. Sprites are positioned in world space, but
  /// rendered in screen space. That means, a 16x16 pixel sprite is usually exactly 16x16 pixel on 
  /// screen. (Internally the images and texts are rendered using the XNA <see cref="SpriteBatch"/>,
  /// hence the name.)
  /// </para>
  /// <para>
  /// DigitalRune Graphics supports two types of sprites:
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// <description><see cref="ImageSprite"/> ... static or animated bitmap</description>
  /// </item>
  /// <item>
  /// <description><see cref="TextSprite"/> ... text using a bitmap font</description>
  /// </item>
  /// </list>
  /// <para>
  /// A sprite is positioned in world space by creating a new <see cref="SpriteNode"/> and adding it
  /// to a 3D scene. 
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// <see cref="Sprite"/>s are cloneable. <see cref="Clone"/> creates a copy of the current 
  /// sprite. The <see cref="ImageSprite.Texture"/> of a <see cref="ImageSprite"/> or the 
  /// <see cref="TextSprite.Text"/> of a <see cref="TextSprite"/> is copied by reference (no deep 
  /// copy).
  /// </para>
  /// </remarks>
  /// <seealso cref="ImageSprite"/>
  /// <seealso cref="TextSprite"/>
  /// <seealso cref="SpriteNode"/>
  public abstract class Sprite : INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the sprite.
    /// </summary>
    /// <value>The name of the sprite.</value>
    public string Name { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="Sprite"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="Sprite"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// See class documentation of <see cref="Sprite"/> (Section "Cloning") for more information 
    /// about cloning.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="Sprite"/> derived class and <see cref="CloneCore"/> to create a copy of the 
    /// current instance. Classes that derive from <see cref="Sprite"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </para>
    /// </remarks>
    public Sprite Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Sprite"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="Sprite"/> method, which this 
    /// method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="Sprite"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private Sprite CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone Sprite. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="Sprite"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="Sprite"/> derived class must implement this method. A typical implementation is
    /// to simply call the default constructor and return the result. 
    /// </para>
    /// </remarks>
    protected abstract Sprite CreateInstanceCore();


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="Sprite"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Sprite"/> derived class must implement
    /// this method. A typical implementation is to call <c>base.CloneCore(this)</c> to copy all 
    /// properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(Sprite source)
    {
      Name = source.Name;
    }
    #endregion

    #endregion
  }
}
