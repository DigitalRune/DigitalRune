// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProgressBar = DigitalRune.Game.UI.Controls.ProgressBar;
using TextBox = DigitalRune.Game.UI.Controls.TextBox;


namespace DigitalRune.Game.UI.Rendering
{
  public partial class UIRenderer
  {
    // TODO: Cache info in the controls. Check UIControl IsVisualValid to see if the cache must be updated.
    // Note: If a text block caches its texture, it might have to update if the parent has 
    // changed even if itself has not changed. This is because the VisualState can be inherited
    // (a text block of a button uses the Focused state from the parent).

#if SILVERLIGHT
    // A primitive replacement for the Stopwatch class which does not exist in Silverlight.
    private sealed class Stopwatch
    {
      private long _startTicks;
      public TimeSpan Elapsed { get { return new TimeSpan(DateTime.UtcNow.Ticks - _startTicks);} }
      public static Stopwatch StartNew()
      {
        var s = new Stopwatch();
        s.Start();
        return s;
      }
      public void Start()
      {
        _startTicks = DateTime.UtcNow.Ticks;
      }
      public void Reset()
      {
        _startTicks = DateTime.UtcNow.Ticks;
      }
    }
#endif

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // For drawing of blinking caret:
    // To keep it simple: We use the same animation timing for all carets. 
    // TODO: We could cache the caret animation info in UIControl.RenderData.
    private const float CaretBlinkTime = 0.4f;
    private const string StateId = "State";
#if XNA
    private readonly System.Diagnostics.Stopwatch _caretTimer = System.Diagnostics.Stopwatch.StartNew();
#else
    private readonly DigitalRune.Diagnostics.Stopwatch _caretTimer = DigitalRune.Diagnostics.Stopwatch.StartNew();
#endif
    private Vector2F _lastCaretPosition;
    private Action<UIControl, UIRenderContext> _renderUIControl;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a white 1x1 texture.
    /// </summary>
    /// <value>A texture with a single white texel.</value>
    public Texture2D WhiteTexture
    {
      get
      {
        if (_whiteTexture == null)
        {
          _whiteTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
          _whiteTexture.SetData(new[] { Color.White });
        }

        return _whiteTexture;
      }
    }
    private Texture2D _whiteTexture;


    /// <summary>
    /// Gets the render callbacks.
    /// </summary>
    /// <value>The render callbacks.</value>
    /// <remarks>
    /// <para>
    /// The render callbacks are methods that render a given control. The keys in this dictionary 
    /// are the style names, e.g. "TextBox". 
    /// </para>
    /// <para>
    /// The render callbacks get the <see cref="UIControl"/> to be rendered and a 
    /// <see cref="UIRenderContext"/> that contains additional information as input parameters. When
    /// the render callback is called, the background of the control has already been cleared by the
    /// <see cref="UIRenderer"/> if the <see cref="UIControl"/> uses a 
    /// <see cref="UIControl.Background"/> color. The <see cref="UIRenderContext"/> contains the 
    /// control's effective <see cref="UIRenderContext.Opacity"/> and the effective 
    /// <see cref="UIRenderContext.RenderTransform"/>. (These properties are automatically updated
    /// by the <see cref="UIRenderer"/>.)
    /// </para>
    /// <para>
    /// Render callbacks may cache information in <see cref="UIControl.RenderData"/>.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public Dictionary<string, Action<UIControl, UIRenderContext>> RenderCallbacks { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    private void InitializeRendering()
    {
      // Register default render callbacks.
      _renderUIControl = RenderUIControl;
      RenderCallbacks = new Dictionary<string, Action<UIControl, UIRenderContext>>();
      RenderCallbacks.Add("UIControl", _renderUIControl);
      RenderCallbacks.Add("TextBlock", RenderTextBlock);
      RenderCallbacks.Add("Image", RenderImageControl);
      RenderCallbacks.Add("Slider", RenderSlider);
      RenderCallbacks.Add("ProgressBar", RenderProgressBar);
      RenderCallbacks.Add("Console", RenderConsole);
      RenderCallbacks.Add("ContentControl", RenderContentControl);
      RenderCallbacks.Add("TextBox", RenderTextBox);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private static void SetState(UIRenderContext context, ThemeState state)
    {
      if (state != null)
        context.Data[StateId] = state;
      else
        context.Data.Remove(StateId);
    }


    private static ThemeState GetState(UIRenderContext context)
    {
      object state;
      context.Data.TryGetValue(StateId, out state);
      return state as ThemeState;
    }


    /// <inheritdoc/>
    public void Render(UIControl control, UIRenderContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");
      if (control == null)
        return;
      if (string.IsNullOrEmpty(control.Style))
        return;

      // Find style and visual state.
      ThemeStyle style;
      Theme.Styles.TryGet(control.Style, out style);
      var state = GetState(control, style);

      // Determine effective opacity.
      float opacity = GetOpacity(control, context, state);
      if (Numeric.IsZero(opacity))
        return;

      // Store current visual state in render context.
      var originalState = GetState(context);
      SetState(context, state);

      // Store absolute opacity and render transformation in render context.
      float originalOpacity = context.Opacity;
      context.Opacity = opacity;
      var originalRenderTransform = context.RenderTransform;
      if (control.HasRenderTransform)
        context.RenderTransform *= control.RenderTransform;

      // Make sure that the sprite batch is active. BeginBatch() safely handles redundant calls.
      BeginBatch();

      // Clear background if necessary.
      Color background = GetBackground(control, state, context.Opacity);
      if (background.A != 0)
        context.RenderTransform.Draw(SpriteBatch, WhiteTexture, GetActualBoundsRounded(control), null, background);

      // The rest is done by the render callback.
      Action<UIControl, UIRenderContext> callback = GetRenderCallback(control, style);
      callback(control, context);

      // Restore original render context.
      SetState(context, originalState);
      context.Opacity = originalOpacity;
      context.RenderTransform = originalRenderTransform;
    }


    /// <summary>
    /// Gets the render callback that should be used to draw the control with the given style.
    /// </summary>
    private Action<UIControl, UIRenderContext> GetRenderCallback(UIControl control, ThemeStyle style)
    {
      // Try to find a render callback for the given style. If nothing is found,
      // try to find a callback for the parent style.
      Action<UIControl, UIRenderContext> callback = null;
      while (callback == null && style != null)
      {
        if (!string.IsNullOrEmpty(style.Name) && RenderCallbacks.TryGetValue(style.Name, out callback))
          return callback;

        style = style.Inherits;
      }

      // Nothing found. Try with control.Style string (parameter 'style' can be null).
      Debug.Assert(!string.IsNullOrEmpty(control.Style), "UIControl.Style must not be null or an empty string.");
      if (RenderCallbacks.TryGetValue(control.Style, out callback))
        return callback;

      return _renderUIControl;
    }


    /// <summary>
    /// Gets the <see cref="ThemeState"/> for the current visual state of the given control.
    /// </summary>
    private static ThemeState GetState(UIControl control, ThemeStyle style)
    {
      if (style == null)
        return null;

      int stateCount = style.States.Count;
      if (stateCount == 0)
        return null;

      var stateName = control.VisualState;
      var parent = control.VisualParent;
      for (int i = 0; i < stateCount; i++)
      {
        // Return first matching state.
        var state = style.States[i];
        if (stateName == state.Name)
          return state;

        // If IsInherited flag is set, we also check the state of the VisualParent.
        // For example, a text block does not have a "Focused" state. But it can have 
        // one if it is inside a button.
        if (state.IsInherited && parent != null && parent.VisualState == state.Name)
          return state;
      }

      // The first state is the default state.
      return style.States[0];
    }


    /// <summary>
    /// Gets the actual bounds snapped to pixels.
    /// </summary>
    private static RectangleF GetActualBoundsRounded(UIControl control)
    {
      return new RectangleF(
        (int)(control.ActualX + 0.5f),
        (int)(control.ActualY + 0.5f),
        (int)(control.ActualWidth + 0.5f),
        (int)(control.ActualHeight + 0.5f));
    }


    /// <summary>
    /// Gets the content bounds (= actual bounds - padding) snapped to pixels.
    /// </summary>
    private static RectangleF GetContentBoundsRounded(UIControl control)
    {
      Vector4F padding = control.Padding;
      return new RectangleF(
        (int)(control.ActualX + padding.X + 0.5f),
        (int)(control.ActualY + padding.Y + 0.5f),
        (int)(control.ActualWidth - padding.X - padding.Z + 0.5f),
        (int)(control.ActualHeight - padding.Y - padding.W + 0.5f));
    }


    /// <summary>
    /// Gets the effective opacity (the product of the opacities of all visual ancestors).
    /// </summary>
    private static float GetOpacity(UIControl control, UIRenderContext context, ThemeState state)
    {
      float opacity = (state != null && state.Opacity.HasValue) ? state.Opacity.Value : control.Opacity;
      return opacity * context.Opacity;
    }


    /// <summary>
    /// Gets the background with pre-multiplied alpha for the given opacity.
    /// </summary>
    private static Color GetBackground(UIControl control, ThemeState state, float opacity)
    {
      Color background = (state != null && state.Background.HasValue) ? state.Background.Value : control.Background;

      // Premultiply with alpha.
#if !SILVERLIGHT
      return Color.FromNonPremultiplied(background.ToVector4() * new Vector4(1, 1, 1, opacity));
#else
      return ColorFromNonPremultiplied(background.ToVector4() * new Vector4(1, 1, 1, opacity));
#endif
    }


    /// <summary>
    /// Gets the foreground with pre-multiplied alpha for the given opacity.
    /// </summary>
    private static Color GetForeground(UIControl control, ThemeState state, float opacity)
    {
      Color foreground = (state != null && state.Foreground.HasValue) ? state.Foreground.Value : control.Foreground;

      // Premultiply with alpha.
#if !SILVERLIGHT
      return Color.FromNonPremultiplied(foreground.ToVector4() * new Vector4(1, 1, 1, opacity));
#else
      return ColorFromNonPremultiplied(foreground.ToVector4() * new Vector4(1, 1, 1, opacity));
#endif
    }


    /// <summary>
    /// RenderCallback for the style "UIControl".
    /// </summary>
    private void RenderUIControl(UIControl control, UIRenderContext context)
    {
      // Background images.
      RenderImages(control, context, false);

      // Visual children.
      foreach (var child in control.VisualChildren)
        child.Render(context);

      // Overlay images.
      RenderImages(control, context, true);
    }


    /// <summary>
    /// RenderCallback for the style "ContentControl".
    /// </summary>
    private void RenderContentControl(UIControl control, UIRenderContext context)
    {
      var contentControl = control as ContentControl;
      if (contentControl == null || contentControl.Content == null || !contentControl.ClipContent)
      {
        // No content or no clipping - render as normal "UIControl".
        RenderUIControl(control, context);
        return;
      }

      // Background images.
      RenderImages(control, context, false);

      EndBatch();

      // Render Content and clip with scissor rectangle.
      Rectangle originalScissorRectangle = GraphicsDevice.ScissorRectangle;
      Rectangle scissorRectangle = context.RenderTransform.Transform(contentControl.ContentBounds).ToRectangle(true);
#if !SILVERLIGHT
      GraphicsDevice.ScissorRectangle = Rectangle.Intersect(scissorRectangle, originalScissorRectangle);
#else
      GraphicsDevice.ScissorRectangle = RectangleIntersect(scissorRectangle, originalScissorRectangle);
#endif

      BeginBatch();
      contentControl.Content.Render(context);
      EndBatch();

      GraphicsDevice.ScissorRectangle = originalScissorRectangle;

      BeginBatch();

      // Visual children except Content.
      foreach (var child in control.VisualChildren)
        if (contentControl.Content != child)
          child.Render(context);

      // Overlay images.
      RenderImages(control, context, true);
    }


    /// <summary>
    /// RenderCallback for the style "TextBlock".
    /// </summary>
    private void RenderTextBlock(UIControl control, UIRenderContext context)
    {
      // Background images.
      RenderImages(control, context, false);

      // Visual children.
      foreach (var child in control.VisualChildren)
        child.Render(context);

      // Render text.
      var textBlock = control as TextBlock;
      if (textBlock != null && textBlock.VisualText.Length > 0)
      {
        RectangleF contentBounds = GetContentBoundsRounded(textBlock);
        Rectangle originalScissorRectangle = GraphicsDevice.ScissorRectangle;
        if (textBlock.VisualClip)
        {
          // If clipping is enabled - set scissors rectangle.
          EndBatch();

          Rectangle scissorRectangle = context.RenderTransform.Transform(contentBounds).ToRectangle(true);
#if !SILVERLIGHT
          GraphicsDevice.ScissorRectangle = Rectangle.Intersect(scissorRectangle, originalScissorRectangle);
#else
          GraphicsDevice.ScissorRectangle = RectangleIntersect(scissorRectangle, originalScissorRectangle);
#endif

          BeginBatch();
        }

        // Render text.
        Vector2F position = new Vector2F(contentBounds.X, contentBounds.Y);
        Color foreground = GetForeground(control, GetState(context), context.Opacity);
        context.RenderTransform.DrawString(SpriteBatch, GetFont(textBlock.Font), textBlock.VisualText, position, foreground);

        if (textBlock.VisualClip)
        {
          // If clipping is enabled - remove scissors rectangle.
          EndBatch();
          GraphicsDevice.ScissorRectangle = originalScissorRectangle;
          BeginBatch();
        }
      }

      // Overlay images.
      RenderImages(control, context, true);
    }


    /// <summary>
    /// RenderCallback for the style "Image".
    /// </summary>
    private void RenderImageControl(UIControl control, UIRenderContext context)
    {
      // Background images.
      RenderImages(control, context, false);

      // Render image.
      var image = control as Image;
      if (image != null && image.Texture != null)
      {
        Color foreground = GetForeground(control, GetState(context), context.Opacity);
#if !WINDOWS_UWP
        context.RenderTransform.Draw(SpriteBatch, image.Texture, GetContentBoundsRounded(image), image.SourceRectangle, foreground);
#else
        context.RenderTransform.Draw(
          SpriteBatch, 
          image.Texture, 
          GetContentBoundsRounded(image), 
          image.SourceRectangle != Rectangle.Empty ? image.SourceRectangle : image.Texture.Bounds,
          foreground);
#endif
      }

      // Visual children.
      foreach (var child in control.VisualChildren)
        child.Render(context);

      // Overlay images.
      RenderImages(control, context, true);
    }


    /// <summary>
    /// RenderCallback for the style "Slider".
    /// </summary>
    private void RenderSlider(UIControl control, UIRenderContext context)
    {
      // Special: An image with the name "Indicator" is drawn from the left up to the slider
      // position.

      ThemeImage indicatorImage = null;
      ThemeState state = GetState(context);
      if (state != null)
      {
        // Background images - except the image called "Indicator".
        foreach (var image in state.Images)
        {
          if (image.Name == "Indicator")
            indicatorImage = image;
          else if (!image.IsOverlay)
            RenderImage(GetActualBoundsRounded(control), image, context.Opacity, context.RenderTransform);
        }
      }

      // Render indicator image.
      Slider slider = control as Slider;
      if (indicatorImage != null && slider != null)
      {
        RectangleF indicatorBounds = GetContentBoundsRounded(slider);

        // Size of indicator image depends on the slider value.
        indicatorBounds.Width = (int)((slider.Value - slider.Minimum)/ (slider.Maximum - slider.Minimum) * indicatorBounds.Width);
        if (indicatorBounds.Width > 0 && indicatorBounds.Height > 0)
          RenderImage(indicatorBounds, indicatorImage, context.Opacity, context.RenderTransform);
      }

      // Visual children.
      foreach (var child in control.VisualChildren)
        child.Render(context);

      // Overlay images.
      RenderImages(control, context, true);
    }


    /// <summary>
    /// RenderCallback for the style "ProgressBar".
    /// </summary>
    private void RenderProgressBar(UIControl control, UIRenderContext context)
    {
      // See comments of RenderSlider above.

      ThemeImage indicatorImage = null;
      ThemeState state = GetState(context);
      if (state != null)
      {
        foreach (var image in state.Images)
        {
          if (image.Name == "Indicator")
            indicatorImage = image;
          else if (!image.IsOverlay)
            RenderImage(GetActualBoundsRounded(control), image, context.Opacity, context.RenderTransform);
        }
      }

      ProgressBar bar = control as ProgressBar;
      if (indicatorImage != null && bar != null)
      {
        RectangleF indicatorBounds = GetContentBoundsRounded(bar);

        // Render indicator.
        if (!bar.IsIndeterminate)
        {
          indicatorBounds.Width = (int)((bar.Value - bar.Minimum) / (bar.Maximum - bar.Minimum) * indicatorBounds.Width);
        }
        else
        {
          // In indeterminate mode the indicator is 1/4 wide and moves left and right.
          float width = indicatorBounds.Width / 4.0f;
          float range = width * 3;
          float center = indicatorBounds.X + width / 2 + (bar.Value - bar.Minimum) / (bar.Maximum - bar.Minimum) * range;
          indicatorBounds.X = (int)(center - width / 2);
          indicatorBounds.Width = (int)(width);
        }

        if (indicatorBounds.Width > 0 && indicatorBounds.Height > 0)
          RenderImage(indicatorBounds, indicatorImage, context.Opacity, context.RenderTransform);
      }

      foreach (var child in control.VisualChildren)
        child.Render(context);

      RenderImages(control, context, true);
    }


    /// <summary>
    /// RenderCallback for the style "Console".
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    private void RenderConsole(UIControl control, UIRenderContext context)
    {
      // Background images.
      RenderImages(control, context, false);

      // Render console text and caret.
      var console = control as Controls.Console;
      if (console != null)
      {
        RectangleF contentBounds = GetContentBoundsRounded(console);

        // Draw all visual lines. A fixed-width font is assumed and the text was already prepared -
        // no clipping necessary.
        var font = GetFont(console.Font);
        Color foreground = GetForeground(control, GetState(context), context.Opacity);
        for (int i = 0; i < console.VisualLines.Count; i++)
        {
          if (console.VisualLines[i] != null)
            context.RenderTransform.DrawString(
              SpriteBatch,
              font,
              console.VisualLines[i],
              new Vector2F(contentBounds.X, contentBounds.Y + i * font.LineSpacing),
              foreground);
        }

        // Draw blinking caret rectangle.
        if (console.VisualCaretX >= 0
            && console.VisualCaretY >= 0
            && console.IsFocused
            && IsCaretVisible(new Vector2F(console.VisualCaretX, console.VisualCaretY)))
        {
          float charWidth = font.MeasureString("A").X;
          Vector4F padding = console.Padding;
          RectangleF caretRectangle = new RectangleF(
            console.ActualX + padding.X + console.VisualCaretX * charWidth - 1,  // minus 1 pixel
            console.ActualY + padding.Y + console.VisualCaretY * font.LineSpacing - 1,
            1,
            font.LineSpacing + 2);

          context.RenderTransform.Draw(SpriteBatch, WhiteTexture, caretRectangle, null, foreground);
        }
      }

      // Visual children.
      foreach (var child in control.VisualChildren)
        child.Render(context);

      // Overlay images.
      RenderImages(control, context, true);
    }


    /// <summary>
    /// RenderCallback for the style "TextBox".
    /// </summary>
    private void RenderTextBox(UIControl control, UIRenderContext context)
    {
      // Textbox content is always clipped using the scissors rectangle.
      // TODO: TextBox should determine if clipping is necessary and set a VisualClip flag.

      // Background images.
      RenderImages(control, context, false);

      var textBox = control as TextBox;
      if (textBox != null)
      {
        RectangleF contentBounds = GetContentBoundsRounded(textBox);
        Rectangle originalScissorRectangle = GraphicsDevice.ScissorRectangle;

        EndBatch();
        Rectangle scissorRectangle = context.RenderTransform.Transform(textBox.VisualClip).ToRectangle(true);
#if !SILVERLIGHT
        GraphicsDevice.ScissorRectangle = Rectangle.Intersect(scissorRectangle, originalScissorRectangle);
#else
        GraphicsDevice.ScissorRectangle = RectangleIntersect(scissorRectangle, originalScissorRectangle);
#endif
        BeginBatch();

        bool hasSelection = (textBox.VisualSelectionBounds.Count > 0);
        bool hasFocus = textBox.IsFocused;
        if (hasSelection && hasFocus)
        {
          // Render selection.
          Color selectionColor = textBox.SelectionColor;
#if !SILVERLIGHT
          selectionColor = Color.FromNonPremultiplied(selectionColor.ToVector4() * new Vector4(1, 1, 1, context.Opacity));
#else
          selectionColor = ColorFromNonPremultiplied(selectionColor.ToVector4() * new Vector4(1, 1, 1, context.Opacity));
#endif
          foreach (RectangleF selection in textBox.VisualSelectionBounds)
          {
            // The selection rectangle of an empty line has zero width.
            // Show a small rectangle to indicate that the selection covers the line. 
            RectangleF rectangle = selection;
            rectangle.Width = Math.Max(rectangle.Width, 4);

            // Draw rectangle using TextBox.SelectionColor.
            context.RenderTransform.Draw(SpriteBatch, WhiteTexture, rectangle, null, selectionColor);
          }
        }

        // Render text.
        Vector2F position = new Vector2F(contentBounds.X, contentBounds.Y);
        if (!textBox.IsMultiline)
          position.X -= textBox.VisualOffset;
        else
          position.Y -= textBox.VisualOffset;

        var font = GetFont(textBox.Font);
        Color foreground = GetForeground(control, GetState(context), context.Opacity);
        context.RenderTransform.DrawString(SpriteBatch, font, textBox.VisualText, position, foreground);

        if (!hasSelection
            && hasFocus
            && !textBox.VisualCaret.IsNaN
            && IsCaretVisible(textBox.VisualCaret))
        {
          // Render caret.
          RectangleF caret = new RectangleF(textBox.VisualCaret.X, textBox.VisualCaret.Y, 2, font.LineSpacing);
          context.RenderTransform.Draw(SpriteBatch, WhiteTexture, caret, null, foreground);
        }

        EndBatch();
        GraphicsDevice.ScissorRectangle = originalScissorRectangle;
        BeginBatch();
      }

      // Visual children.
      foreach (var child in control.VisualChildren)
        child.Render(context);

      // Overlay images.
      RenderImages(control, context, true);
    }


    private bool IsCaretVisible(Vector2F caretPosition)
    {
      // For blinking caret. A custom stopwatch timer is used to determine blinking times.

      // If caret position changes, the caret is made visible.
      if (caretPosition != _lastCaretPosition)
      {
        _caretTimer.Reset();
        _caretTimer.Start();
        _lastCaretPosition = caretPosition;
        return true;
      }

      _lastCaretPosition = caretPosition;
      float elapsedTime = (float)_caretTimer.Elapsed.TotalSeconds;
      if (elapsedTime > 2 * CaretBlinkTime)
      {
        // Begin new period.
        _caretTimer.Reset();
        _caretTimer.Start();
      }

      // CaretBlinkTime visible, then CaretBlinkTime invisible, then visible again...
      return elapsedTime < CaretBlinkTime || elapsedTime > 2 * CaretBlinkTime;
    }


    /// <summary>
    /// Renders the <see cref="ThemeImage"/>s of the current visual state of the given context.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="context">The render context.</param>
    /// <param name="drawOverlays">
    /// If set to <see langword="true"/> only overlay images are rendered; otherwise, only
    /// background images are rendered. See <see cref="ThemeImage.IsOverlay"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void RenderImages(UIControl control, UIRenderContext context, bool drawOverlays)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      var state = GetState(context);
      if (state == null)
        return;

      int numberOfImages = state.Images.Count;
      if (numberOfImages == 0)
        return;

      RectangleF bounds = GetActualBoundsRounded(control);
      for (int i = 0; i < numberOfImages; i++)
      {
        var image = state.Images[i];
        if (image.IsOverlay != drawOverlays)
          continue;

        RenderImage(bounds, image, context.Opacity, context.RenderTransform);
      }
    }


    /// <summary>
    /// Renders a <see cref="ThemeImage"/>.
    /// </summary>
    /// <param name="bounds">The control's bounding rectangle.</param>
    /// <param name="image">The image.</param>
    /// <param name="opacity">The opacity.</param>
    /// <param name="transform">The render transform.</param>
    /// <remarks>
    /// This method uses the <paramref name="image"/> properties (alignment, margin, etc.) and the 
    /// render transformation to render the image into the target (<paramref name="bounds"/>).
    /// </remarks>
    public void RenderImage(RectangleF bounds, ThemeImage image, float opacity, RenderTransform transform)
    {
      if (image == null)
        return;

      // Get the texture atlas containing the image.
      Texture2D texture = (image.Texture != null) ? image.Texture.Texture : _defaultTexture;
      
      // Get bounds without margin.
      Rectangle source = image.SourceRectangle;
      Vector4F margin = image.Margin;
      bounds.X += margin.X;
      bounds.Y += margin.Y;
      bounds.Width -= margin.X + margin.Z;
      bounds.Height -= margin.Y + margin.W;

      // Get tint color and premultiply alpha.
      Vector4 colorVector = image.Color.ToVector4();
      colorVector.W *= opacity;
#if !SILVERLIGHT
      Color color = Color.FromNonPremultiplied(colorVector);
#else
      Color color = ColorFromNonPremultiplied(colorVector);
#endif


      if (image.HorizontalAlignment == HorizontalAlignment.Stretch || image.VerticalAlignment == VerticalAlignment.Stretch)
      {
        // Draw stretched image using a 9-grid layout.
        RenderStretchedImage(texture, image, bounds, transform, color);
      }
      else
      {
        // Draw a non-stretched image.
        RenderImage(texture, bounds, source, image.HorizontalAlignment, image.VerticalAlignment, image.TileMode, transform, color);
      }
    }


    /// <summary>
    /// Renders an image with alignment and optional stretching.
    /// </summary>
    /// <param name="texture">The UI texture.</param>
    /// <param name="image">The image.</param>
    /// <param name="area">The area to fill.</param>
    /// <param name="transform">The render transform.</param>
    /// <param name="color">The tint color.</param>
    private void RenderStretchedImage(Texture2D texture, ThemeImage image, RectangleF area, RenderTransform transform, Color color)
    {
      Rectangle source = image.SourceRectangle;
      int left = (int)image.Border.X;
      int top = (int)image.Border.Y;
      int right = (int)image.Border.Z;
      int bottom = (int)image.Border.W;

      switch (image.HorizontalAlignment)
      {
        case HorizontalAlignment.Center:
          area.X = (float)Math.Floor(area.X + area.Width / 2.0f - source.Width / 2.0f); // Always round down for consistent results!
          left = source.Width;
          right = 0;
          break;
        case HorizontalAlignment.Left:
          left = source.Width;
          right = 0;
          break;
        case HorizontalAlignment.Right:
          left = 0;
          right = source.Width;
          break;
      }

      switch (image.VerticalAlignment)
      {
        case VerticalAlignment.Center:
          area.Y = (float)Math.Floor(area.Y + area.Height / 2.0f - source.Height / 2.0f); // Always round down for consistent results!
          top = source.Height;
          bottom = 0;
          break;
        case VerticalAlignment.Top:
          top = source.Height;
          bottom = 0;
          break;
        case VerticalAlignment.Bottom:
          top = 0;
          bottom = source.Height;
          break;
      }

      // Draw 9-grid layout:
      //
      //   -----------------------
      //   | 1 |      2      | 3 |
      //   -----------------------
      //   |   |             |   |
      //   | 4 |      5      | 6 |
      //   |   |             |   |
      //   -----------------------
      //   | 7 |      8      | 9 |
      //   -----------------------

      Vector2F destinationPosition;
      RectangleF destinationRectangle;
      Rectangle sourceRectangle;

      // Cell #1 (no stretching)
      destinationPosition = new Vector2F(area.X, area.Y);
      sourceRectangle = new Rectangle(source.X, source.Y, left, top);
      transform.Draw(SpriteBatch, texture, destinationPosition, sourceRectangle, color);

      // Cell #2 (horizontal stretching)
      destinationRectangle = new RectangleF(area.X + left, area.Y, area.Width - left - right, top);
      sourceRectangle = new Rectangle(source.X + left, source.Y, source.Width - left - right, top);
      transform.Draw(SpriteBatch, texture, destinationRectangle, sourceRectangle, color);

      // Cell #3 (no stretching)
      destinationPosition = new Vector2F(area.X + area.Width - right, area.Y);
      sourceRectangle = new Rectangle(source.X + source.Width - right, source.Y, right, top);
      transform.Draw(SpriteBatch, texture, destinationPosition, sourceRectangle, color);

      // Cell #4 (vertical stretching)
      destinationRectangle = new RectangleF(area.X, area.Y + top, left, area.Height - top - bottom);
      sourceRectangle = new Rectangle(source.X, source.Y + top, left, source.Height - top - bottom);
      transform.Draw(SpriteBatch, texture, destinationRectangle, sourceRectangle, color);

      // Cell #5 (horizontal and vertical stretching)
      destinationRectangle = new RectangleF(area.X + left, area.Y + top, area.Width - left - right, area.Height - top - bottom);
      sourceRectangle = new Rectangle(source.X + left, source.Y + top, source.Width - left - right, source.Height - top - bottom);
      transform.Draw(SpriteBatch, texture, destinationRectangle, sourceRectangle, color);

      // Cell #6 (vertical stretching)
      destinationRectangle = new RectangleF(area.X + area.Width - right, area.Y + top, right, area.Height - top - bottom);
      sourceRectangle = new Rectangle(source.X + source.Width - right, source.Y + top, right, source.Height - top - bottom);
      transform.Draw(SpriteBatch, texture, destinationRectangle, sourceRectangle, color);

      // Cell #7 (no stretching)
      destinationPosition = new Vector2F(area.X, area.Y + area.Height - bottom);
      sourceRectangle = new Rectangle(source.X, source.Y + source.Height - bottom, left, bottom);
      transform.Draw(SpriteBatch, texture, destinationPosition, sourceRectangle, color);

      // Cell #8 (horizontal stretching)
      destinationRectangle = new RectangleF(area.X + left, area.Y + area.Height - bottom, area.Width - left - right, bottom);
      sourceRectangle = new Rectangle(source.X + left, source.Y + source.Height - bottom, source.Width - left - right, bottom);
      transform.Draw(SpriteBatch, texture, destinationRectangle, sourceRectangle, color);

      // Cell #9 (no stretching)
      destinationPosition = new Vector2F(area.X + area.Width - right, area.Y + area.Height - bottom);
      sourceRectangle = new Rectangle(source.X + source.Width - right, source.Y + source.Height - bottom, right, bottom);
      transform.Draw(SpriteBatch, texture, destinationPosition, sourceRectangle, color);
    }


    /// <summary>
    /// Renders an image with alignment and optional tiling.
    /// </summary>
    /// <param name="texture">The UI texture.</param>
    /// <param name="area">The area to fill.</param>
    /// <param name="source">The source rectangle of the image in the UI texture.</param>
    /// <param name="horizontalAlignment">The horizontal alignment.</param>
    /// <param name="verticalAlignment">The vertical alignment.</param>
    /// <param name="tileMode">The tile mode.</param>
    /// <param name="transform">The render transform.</param>
    /// <param name="color">The tint color.</param>
    private void RenderImage(Texture2D texture, RectangleF area, Rectangle source, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, TileMode tileMode, RenderTransform transform, Color color)
    {
      bool tileHorizontally = (tileMode == TileMode.TileX || tileMode == TileMode.TileXY);
      bool tileVertically = (tileMode == TileMode.TileY || tileMode == TileMode.TileXY);

      // Compute destination rectangle of base image (left/top tile).
      RectangleF destination;
      switch (horizontalAlignment)
      {
        case HorizontalAlignment.Center:
          destination.X = (float)Math.Floor(area.X + area.Width / 2.0f - source.Width / 2.0f);    // Always round down for consistent results!
          destination.Width = source.Width;

          if (tileHorizontally)
            while (destination.X > area.X)
              destination.X -= destination.Width;

          break;
        
        case HorizontalAlignment.Right:
          destination.X = area.X + area.Width - source.Width;
          destination.Width = source.Width;

          if (tileHorizontally)
            while (destination.X > area.X)
              destination.X -= destination.Width;

          break;
        
        case HorizontalAlignment.Stretch:
          destination.X = area.X;
          destination.Width = area.X + area.Width - source.Width;
          break;
        
        default:
          destination.X = area.X;
          destination.Width = source.Width;
          break;
      }

      switch (verticalAlignment)
      {
        case VerticalAlignment.Center:
          destination.Y = (float)Math.Floor(area.Y + area.Height / 2.0f - source.Height / 2.0f);  // Always round down for consistent results!
          destination.Height = source.Height;

          if (tileVertically)
            while (destination.Y > area.Y)
              destination.Y -= destination.Height;

          break;

        case VerticalAlignment.Bottom:
          destination.Y = area.Y + area.Height - source.Height;
          destination.Height = source.Height;
          
          if (tileVertically)
            while (destination.Y > area.Y)
              destination.Y -= destination.Height;

          break;

        case VerticalAlignment.Stretch:
          destination.Y = area.Y;
          destination.Height = area.Height;
          break;

        default:
          destination.Y = area.Y;
          destination.Height = source.Height;
          break;
      }

      switch (tileMode)
      {
        case TileMode.None:
          ClipX(area, ref destination, ref source);
          ClipY(area, ref destination, ref source);
          transform.Draw(SpriteBatch, texture, destination, source, color);
          break;

        case TileMode.TileX:
          ClipY(area, ref destination, ref source);
          RenderTileX(texture, area, destination, source, transform, color);
          break;
        
        case TileMode.TileY:
          ClipX(area, ref destination, ref source);
          RenderTileY(texture, area, destination, source, transform, color);
          break;
        
        case TileMode.TileXY:
          RenderTileXY(texture, area, destination, source, transform, color);
          break;
      }      
    }


    /// <summary>
    /// Renders an image repeated times in horizontal direction.
    /// </summary>
    /// <param name="texture">The UI texture.</param>
    /// <param name="area">The area to fill.</param>
    /// <param name="destination">The destination rectangle of the left, top tile.</param>
    /// <param name="source">The source rectangle of the image in the UI texture.</param>
    /// <param name="transform">The render transform.</param>
    /// <param name="color">The tint color.</param>
    private void RenderTileX(Texture2D texture, RectangleF area, RectangleF destination, Rectangle source, RenderTransform transform, Color color)
    {
      // Clip and draw first tile.
      RectangleF clippedDestination = destination;
      Rectangle clippedSource = source;
      bool rightEdgeClipped = ClipX(area, ref clippedDestination, ref clippedSource);
      transform.Draw(SpriteBatch, texture, clippedDestination, clippedSource, color);

      if (rightEdgeClipped)
      {
        // No more tiles to draw.
        return;
      }

      // Draw intermediate tiles.
      destination.X += destination.Width;
      float areaRight = area.Right;
      while (destination.Right < areaRight)
      {
        transform.Draw(SpriteBatch, texture, destination, source, color);
        destination.X += destination.Width;
      }

      // Clip and draw last tile.
      clippedDestination = destination;
      clippedSource = source;
      ClipX(area, ref clippedDestination, ref clippedSource);
      transform.Draw(SpriteBatch, texture, clippedDestination, clippedSource, color);
    }


    /// <summary>
    /// Renders an image repeated times in vertical direction.
    /// </summary>
    /// <param name="texture">The UI texture.</param>
    /// <param name="area">The area to fill.</param>
    /// <param name="destination">The destination rectangle of the left, top tile.</param>
    /// <param name="source">The source rectangle of the image in the UI texture.</param>
    /// <param name="transform">The render transform.</param>
    /// <param name="color">The tint color.</param>
    private void RenderTileY(Texture2D texture, RectangleF area, RectangleF destination, Rectangle source, RenderTransform transform, Color color)
    {
      // Clip and draw first tile.
      RectangleF clippedDestination = destination;
      Rectangle clippedSource = source;
      bool bottomEdgeClipped = ClipY(area, ref clippedDestination, ref clippedSource);
      transform.Draw(SpriteBatch, texture, clippedDestination, clippedSource, color);

      if (bottomEdgeClipped)
        return;

      // Draw intermediate tiles.
      destination.Y += destination.Height;
      float areaBottom = area.Bottom;
      while (destination.Bottom < areaBottom)
      {
        transform.Draw(SpriteBatch, texture, destination, source, color);
        destination.Y += destination.Height;
      }

      // Clip and draw last tile.
      clippedDestination = destination;
      clippedSource = source;
      ClipY(area, ref clippedDestination, ref clippedSource);
      transform.Draw(SpriteBatch, texture, clippedDestination, clippedSource, color);
    }


    /// <summary>
    /// Renders an image repeated times in horizontal and vertical direction.
    /// </summary>
    /// <param name="texture">The UI texture.</param>
    /// <param name="area">The area to fill.</param>
    /// <param name="destination">The destination rectangle of the left, top tile.</param>
    /// <param name="source">The source rectangle of the image in the UI texture.</param>
    /// <param name="transform">The render transform.</param>
    /// <param name="color">The tint color.</param>
    private void RenderTileXY(Texture2D texture, RectangleF area, RectangleF destination, Rectangle source, RenderTransform transform, Color color)
    {
      // Clip and draw first row.
      RectangleF clippedDestination = destination;
      Rectangle clippedSource = source;
      bool bottomEdgeClipped = ClipY(area, ref clippedDestination, ref clippedSource);
      RenderTileX(texture, area, clippedDestination, clippedSource, transform, color);

      if (bottomEdgeClipped)
        return;

      // Draw intermediate rows.
      destination.Y += destination.Height;
      float areaBottom = area.Bottom;
      while (destination.Bottom < areaBottom)
      {
        RenderTileX(texture, area, destination, source, transform, color);
        destination.Y += destination.Height;
      }

      // Clip and draw last row.
      clippedDestination = destination;
      clippedSource = source;
      ClipY(area, ref clippedDestination, ref clippedSource);
      RenderTileX(texture, area, clippedDestination, clippedSource, transform, color);
    }


    /// <summary>
    /// Clips the image horizontally to the given area.
    /// </summary>
    /// <param name="area">The allowed area.</param>
    /// <param name="destination">The destination rectangle of the image. </param>
    /// <param name="source">The source rectangle of the image in the UI texture.</param>
    /// <returns>
    /// <see langword="true"/> if the right of the image was clipped; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This computes the clipping and modifies <paramref name="destination"/> and 
    /// <paramref name="source"/> accordingly.
    /// </remarks>
    private static bool ClipX(RectangleF area, ref RectangleF destination, ref Rectangle source)
    {
      // Clip left edge.
      if (destination.X < area.X)
      {
        float delta = area.X - destination.X;
        destination.X += delta;
        destination.Width -= delta;
        source.X += (int)delta;
        source.Width -= (int)delta;
      }

      // Clip right edge.
      float destinationRight = destination.Right;
      float areaRight = area.Right;
      if (destinationRight > areaRight)
      {
        float delta = destinationRight - areaRight;
        destination.Width -= delta;
        source.Width -= (int)delta;
        return true;
      }

      return false;
    }


    /// <summary>
    /// Clips the image vertically to the given area.
    /// </summary>
    /// <param name="area">The allowed area.</param>
    /// <param name="destination">The destination rectangle of the image.</param>
    /// <param name="source">The source rectangle of the image in the UI texture.</param>
    /// <returns>
    /// <see langword="true"/> if the bottom of the image was clipped; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This computes the clipping and modifies <paramref name="destination"/> and 
    /// <paramref name="source"/> accordingly.
    /// </remarks>
    private static bool ClipY(RectangleF area, ref RectangleF destination, ref Rectangle source)
    {
      // Clip top edge.
      if (destination.Y < area.Y)
      {
        float delta = area.Y - destination.Y;
        destination.Y += delta;
        destination.Height -= delta;
        source.Y += (int)delta;
        source.Height -= (int)delta;
      }

      // Clip bottom edge.
      float destinationBottom = destination.Bottom;
      float areaBottom = area.Bottom;
      if (destinationBottom > areaBottom)
      {
        float delta = destinationBottom - areaBottom;
        destination.Height -= delta;
        source.Height -= (int)delta;
        return true;
      }

      return false;
    }

#if SILVERLIGHT
    // Rectangle.Intersect does not exist in Silverlight. Here is a replacement.
    private static Rectangle RectangleIntersect(Rectangle rectangle1, Rectangle rectangle2)
    {
      int left = Math.Max(rectangle1.X, rectangle2.X);
      int top = Math.Max(rectangle1.Y, rectangle2.Y);
      int right = Math.Min(rectangle1.Right, rectangle2.Right);
      int bottom = Math.Min(rectangle1.Bottom, rectangle2.Bottom);

      if (left < right && top < bottom)
        return new Rectangle(left, top, right - left, bottom - top);

      return Rectangle.Empty;
    }

    // Color.FromNonPremultiplied(Vector4) does not exist in Silverlight. Here is a replacement.
    private static Color ColorFromNonPremultiplied(Vector4 vector)
    {
      return new Color(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W, vector.W);
    }
#endif
    #endregion
  }
}
