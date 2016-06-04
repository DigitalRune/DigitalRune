// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a text, which is rendered as billboard.
  /// </summary>
  /// <inheritdoc/>
  public class TextBillboard : Billboard
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

        Invalidate();
      }
    }
    private object _text;


    /// <summary>
    /// Gets or sets the font.
    /// </summary>
    /// <value>The font. Can be <see langword="null"/>.</value>
    public SpriteFont Font
    {
      get { return _font; }
      set
      {
        _font = value;
        Invalidate();
      }
    }
    private SpriteFont _font;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TextBillboard"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TextBillboard"/> class.
    /// </summary>
    public TextBillboard()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TextBillboard"/> class.
    /// </summary>
    /// <param name="text">The text. See <see cref="Text"/> for more information.</param>
    /// <param name="font">The font.</param>
    public TextBillboard(object text, SpriteFont font)
    {
      _font = font;
      Text = text;   // The property setter calls Invalidate()!
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="Sprite.Clone"/>
    public new TextBillboard Clone()
    {
      return (TextBillboard)base.Clone();
    }


    /// <inheritdoc/>
    protected override Billboard CreateInstanceCore()
    {
      return new TextBillboard();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Billboard source)
    {
      // Clone Billboard properties.
      base.CloneCore(source);

      // Clone TextBillboard properties.
      var sourceTyped = (TextBillboard)source;
      _text = sourceTyped.Text;
      Font = sourceTyped.Font;
    }
    #endregion


    /// <summary>
    /// Invalidates the text billboard. See remarks.
    /// </summary>
    /// <remarks>
    /// This method needs to be called when the text is specified using a 
    /// <see cref="StringBuilder"/> and the contents of the <see cref="StringBuilder"/> has changed.
    /// </remarks>
    public void Invalidate()
    {
      // Update bounding shape.
      var text = Text as string;
      var stringBuilder = Text as StringBuilder;
      if (Text == null 
          || text != null && text.Length == 0 
          || stringBuilder != null && stringBuilder.Length == 0)
      {
        // Empty.
        Shape.Radius = 0;
        return;
      }

      if (Font == null)
      {
        // Use dummy size.
        Shape.Radius = 1;
        return;
      }

      Vector2 size = (text != null) ? Font.MeasureString(text) : Font.MeasureString(stringBuilder);
      Shape.Radius = size.Length() / 2;
    }
    #endregion
  }
}
