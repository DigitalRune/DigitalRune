// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Text;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a 2D text rendered in screen space.
  /// </summary>
  /// <inheritdoc/>
  public class TextSprite : Sprite
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the text. See remarks.
    /// </summary>
    /// <value>The text - see remarks. The default value is <see langword="null"/>.</value>
    /// <remarks>
    /// <para>
    /// The value can be set as a <see cref="string"/>, a <see cref="StringBuilder"/>, or a general
    /// <see cref="object"/>. If it is a general object, the value is converted to its string 
    /// representation by calling <see cref="object.ToString"/> immediately. (The property 
    /// internally stores either a <see cref="string"/> or <see cref="StringBuilder"/>.)
    /// </para>
    /// <para>
    /// Depending on the value that was set, the get accessor returns either <see langword="null"/>,
    /// a <see cref="string"/>, or a <see cref="StringBuilder"/>.
    /// </para>
    /// </remarks>
    public object Text
    {
      get { return _text; }
      set
      {
        if (value == null || value is string || value is StringBuilder)
          _text = value;
        else 
          _text = value.ToString();
      }
    }
    private object _text;


    /// <summary>
    /// Gets or sets the font.
    /// </summary>
    /// <value>The font. Can be <see langword="null"/>.</value>
    public SpriteFont Font { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TextSprite"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TextSprite"/> class.
    /// </summary>
    public TextSprite()
    {      
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TextSprite"/> class.
    /// </summary>
    /// <param name="text">The text. See <see cref="Text"/> for more information.</param>
    /// <param name="font">The font.</param>
    public TextSprite(object text, SpriteFont font)
    {
      Text = text;
      Font = font;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="Sprite.Clone"/>
    public new TextSprite Clone()
    {
      return (TextSprite)base.Clone();
    }


    /// <inheritdoc/>
    protected override Sprite CreateInstanceCore()
    {
      return new TextSprite();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Sprite source)
    {
      // Clone Sprite properties.
      base.CloneCore(source);

      // Clone TextSprite properties.
      var sourceTyped = (TextSprite)source;
      _text = sourceTyped.Text;
      Font = sourceTyped.Font;
    }
    #endregion

    #endregion
  }
}
