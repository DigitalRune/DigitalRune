// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents a control with another <see cref="UIControl"/> as content.
  /// </summary>
  /// <remarks>
  /// The <see cref="Content"/> is another <see cref="UIControl"/> that is drawn inside the 
  /// <see cref="ContentControl"/>. The <see cref="UIControl.Padding"/> is applied to the bounds of
  /// the <see cref="Content"/>.
  /// </remarks>
  public class ContentControl : UIControl
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    /// <value>The content.</value>
    public UIControl Content
    {
      get { return _content; }
      set
      {
        if (_content == value)
          return;

        var oldContent = _content;
        _content = value;
        OnContentChanged(_content, oldContent);
      }
    }
    private UIControl _content;


    /// <summary>
    /// Gets the content bounds that define where the <see cref="Content"/> is drawn.
    /// </summary>
    /// <value>The content bounds.</value>
    /// <remarks>
    /// Per default, the content bounds are the <see cref="UIControl.ActualBounds"/> of this control 
    /// minus the <see cref="UIControl.Padding"/>. Derived classes can define a different placement 
    /// strategy by overriding this property.
    /// </remarks>
    public virtual RectangleF ContentBounds
    {
      get
      {
        Vector4F padding = Padding;
        return new RectangleF(
          ActualX + padding.X, 
          ActualY + padding.Y,
          ActualWidth - padding.X - padding.Z,
          ActualHeight - padding.Y - padding.W);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="ContentStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ContentStylePropertyId = CreateProperty<string>(
      typeof(ContentControl), "ContentStyle", GamePropertyCategories.Style, null, null,
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the <see cref="Content"/>. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the <see cref="Content"/>. This value can be 
    /// <see langword="null"/> in which case the style of the <see cref="Content"/> is not changed.
    /// </value>
    public string ContentStyle
    {
      get { return GetValue<string>(ContentStylePropertyId); }
      set { SetValue(ContentStylePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="ClipContent"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ClipContentPropertyId = CreateProperty(
      typeof(ContentControl), "ClipContent", GamePropertyCategories.Appearance, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="Content"/> is clipped to the
    /// <see cref="ContentBounds"/> or whether the <see cref="Content"/> can draw outside the
    /// <see cref="ContentBounds"/>. This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the <see cref="Content"/> is clipped to the 
    /// <see cref="ContentBounds"/>; otherwise, <see langword="false"/> if the <see cref="Content"/>
    /// can draw outside the <see cref="ContentBounds"/>. The default is <see langword="false"/>
    /// because clipping costs performance and most games use a fixed layout where clipping is not
    /// needed.
    /// </value>
    public bool ClipContent 
    {
      get { return GetValue<bool>(ClipContentPropertyId); }
      set { SetValue(ClipContentPropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentControl"/> class.
    /// </summary>
    public ContentControl()
    {
      Style = "ContentControl";
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnTemplateChanged(EventArgs eventArgs)
    {
      base.OnTemplateChanged(eventArgs);

      // If the template changes, we apply the new ContentStyle to our Content.
      if (IsLoaded && Content != null && !string.IsNullOrEmpty(ContentStyle))
        Content.Style = ContentStyle;
    }


    /// <summary>
    /// Called when the <see cref="Content"/> was exchanged.
    /// </summary>
    /// <param name="newContent">The new content.</param>
    /// <param name="oldContent">The old content.</param>
    protected virtual void OnContentChanged(UIControl newContent, UIControl oldContent)
    {
      if (oldContent != null)
        VisualChildren.Remove(oldContent);

      if (newContent != null)
      {
        // Apply ContentStyle to Content.
        if (IsLoaded && !string.IsNullOrEmpty(ContentStyle))
          newContent.Style = ContentStyle;

        VisualChildren.Add(newContent);
      }

      InvalidateMeasure();
    }


    /// <inheritdoc/>
    protected override bool HitTest(UIControl control, Vector2F position)
    {
      // If control is the child and ClipContent is enabled, then the mouse must be within the
      // ContentBounds. (The child part that is outside the ContentBounds is invisible and cannot
      // be clicked.)
      if (control != null && control == Content && ClipContent)
      {
        return ContentBounds.Contains(position);
      }

      return base.HitTest(control, position);
    }


    /// <inheritdoc/>
    protected override Vector2F OnMeasure(Vector2F availableSize)
    {
      // Similar to UIControl.OnMeasure, but for the content the Padding is applied.

      float width = Width;
      float height = Height;
      bool hasWidth = Numeric.IsPositiveFinite(width);
      bool hasHeight = Numeric.IsPositiveFinite(height);
      bool hasContent = (Content != null);

      if (hasWidth && width < availableSize.X)
        availableSize.X = width;

      if (hasHeight && height < availableSize.Y)
        availableSize.Y = height;

      foreach (var child in VisualChildren)
        if (child != Content)
          child.Measure(availableSize);

      Vector4F padding = Padding;
      if (hasContent)
      {
        availableSize.X -= padding.X + padding.Z;
        availableSize.Y -= padding.Y + padding.W;
        Content.Measure(availableSize);
      }

      Vector2F desiredSize = Vector2F.Zero;
      if (hasWidth)
      {
        desiredSize.X = width;
      }
      else
      {
        float contentSize = hasContent ? Content.DesiredWidth : 0;
        desiredSize.X = padding.X + contentSize + padding.Z;
        foreach (var child in VisualChildren)
          desiredSize.X = Math.Max(desiredSize.X, child.DesiredWidth);
      }
      if (hasHeight)
      {
        desiredSize.Y = height;
      }
      else
      {
        float contentSize = hasContent ? Content.DesiredHeight : 0;
        desiredSize.Y = padding.Y + contentSize + padding.W;
        foreach (var child in VisualChildren)
          desiredSize.Y = Math.Max(desiredSize.Y, child.DesiredHeight);
      }

      return desiredSize;
    }


    /// <inheritdoc/>
    protected override void OnArrange(Vector2F position, Vector2F size)
    {
      // Similar to UIControl.OnArrange, but for the content the Padding is applied.
      foreach (var child in VisualChildren)
        if (child != Content)
          Arrange(child, position, size);

      if (Content != null)
      {
        Vector4F padding = Padding;
        position.X += padding.X;
        position.Y += padding.Y;
        size.X -= padding.X + padding.Z;
        size.Y -= padding.Y + padding.W;
        Arrange(Content, position, size);
      }
    }
    #endregion
  }
}
