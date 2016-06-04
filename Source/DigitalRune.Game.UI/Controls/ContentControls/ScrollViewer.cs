// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
#if WP7 || PORTABLE
using DigitalRune.Game.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endif

using MathHelper = Microsoft.Xna.Framework.MathHelper;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents a scrollable area that can contain other visible controls. 
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="ScrollViewer"/> displays a part of the <see cref="ContentControl.Content"/>,
  /// which can be larger than the size of the <see cref="ScrollViewer"/>.
  /// </para>
  /// <para>
  /// The visible part can be controlled using two scroll bars or the mouse wheel. When scrolling
  /// with the mouse wheel, the scroll speed is proportional to <see cref="RangeBase.SmallChange"/>. 
  /// </para>
  /// <para>
  /// <strong>Phone and Tablets:</strong> On phone and tablets the <see cref="ScrollViewer"/> can be
  /// controlled with touch input (only vertical scrolling). A flick gesture creates a scroll
  /// velocity that is slowly damped. The vertical scroll bar is transparent by default and becomes
  /// visible during scrolling. The dynamic scroll behavior can be configured using 
  /// <see cref="MinScrollVelocity"/>, <see cref="MaxScrollVelocity"/>, <see cref="ScrollDamping"/>
  /// and <see cref="FlickScrollVelocityFactor"/>, and is the same for all 
  /// <see cref="ScrollViewer"/> instances.
  /// </para>
  /// <para>
  /// The user can push the content beyond the usual limits by dragging the content with the finger
  /// or by scrolling really fast. The content automatically bounces back when the user is not
  /// touching the control. This effect is simulated with a "damped spring". The properties
  /// <see cref="SpringConstant"/>, <see cref="SpringDamping"/> and <see cref="SpringLength"/>
  /// control the effect. The visual effect is achieved by applying a scale transform to the
  /// <see cref="Content"/> (see <see cref="UIControl.RenderScale"/>). (Any existing render
  /// transform will be overwritten!)
  /// </para>
  /// </remarks>
  /// <example>
  /// The following example shows to display a large image inside a scroll viewer.
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Use an image control to display a texture.
  /// var image = new Image
  /// {
  ///   Texture = content.Load<Texture2D>("Image_1024x1024")
  /// };
  /// 
  /// // Use a scroll viewer to show a region of the texture.
  /// var scrollViewer = new ScrollViewer
  /// {
  ///   Margin = new Vector4F(4),
  ///   Width = 200
  ///   Height = 200,
  /// };
  /// scrollViewer.Content = image;
  /// 
  /// // To show the scroll viewer, add it to an existing content control or panel.
  /// panel.Children.Add(scrollViewer);
  /// ]]>
  /// </code>
  /// </example>
  public class ScrollViewer : ContentControl
  {
    // Important: When both scroll bars are visible, a small rectangle area in the lower left
    // corner is not covered by the scroll bars. This make the scroll bar positioning a bit
    // more complicated/annoying.
    //
    // ----- ScrollViewer vs. Draggable Controls
    // On phone/tablet the ScrollViewer handles dragging before the child controls get the
    // chance to handle input. Draggable controls (e.g. ScrollBar, Slider) inside a 
    // ScrollViewer are not handled properly and should be avoided.
    //
    // Suggested solution:
    // - IInputService.IsMouseOrTouchHandled is not enough information.
    //   Add property IInputService.IsDragHandled.
    // - Most controls can ignore IsDragHandled.
    // - The Thumb used in ScrollBars and Sliders sets IsDragHandled during dragging.
    // - The ScrollViewer lets the children handle input first. If no child handles
    //   dragging it can attempt to handle dragging.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private ScrollBar _horizontalScrollBar;
    private ScrollBar _verticalScrollBar;
    private RectangleF _contentBounds;

#if WP7 || PORTABLE
    private bool _isTouchDevice;

    // Dynamic scrolling with flick gestures:
    private const float ScrollBarFadeVelocity = 1;
    private bool _isDragging;
    private bool _scrollToleranceExceeded;
    private Vector2F _scrollStartPosition;
    private Vector2F _scrollStartOffset;
    private Vector2F _scrollVelocity;

    // On phone/tablet the user can drag the content beyond the limit.
    private Vector2F _virtualOffset;  // The virtual offset, which can be beyond the limits.
#endif
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Phone/tablet only: Gets or sets the minimal scroll velocity that determines when scrolling
    /// is stopped.
    /// </summary>
    /// <value>
    /// The minimal scroll velocity that determines when scrolling is stopped. The default is 100.
    /// </value>
    public static float MinScrollVelocity
    {
      get { return _minScrollVelocity; }
      set { _minScrollVelocity = value; }
    }
    private static float _minScrollVelocity = 100;


    /// <summary>
    /// Phone/tablet only: Gets or sets the maximal allowed scroll velocity. (Higher scrolling
    /// velocities are clamped to this value.)
    /// </summary>
    /// <value>
    /// The maximal allowed scroll velocity. The default is 2400.
    /// </value>
    public static float MaxScrollVelocity
    {
      get { return _maxScrollVelocity; }
      set { _maxScrollVelocity = value; }
    }
    private static float _maxScrollVelocity = 2400;


    /// <summary>
    /// Phone/tablet only: Gets or sets the damping factor with which the scrolling is damped.
    /// </summary>
    /// <value>
    /// The damping factor with which the scrolling is damped. The default value is 1.5. Higher
    /// values increase damping.
    /// </value>
    public static float ScrollDamping
    {
      get { return _scrollDamping; }
      set { _scrollDamping = value; }
    }
    private static float _scrollDamping = 1.5f;


    /// <summary>
    /// Phone/tablet only: Gets or sets the spring constant which is applied when the user drags
    /// the content beyond the limit.
    /// </summary>
    /// <value>
    /// The spring constant which is applied when the user drags the content beyond the limit. The 
    /// default value is 180.
    /// </value>
    public static float SpringConstant
    {
      get { return _springConstant; }
      set { _springConstant = value; }
    }
    private static float _springConstant = 180.0f;


    /// <summary>
    /// Phone/tablet only: Gets or sets the damping which is applied when the user drags the content
    /// beyond the limit.
    /// </summary>
    /// <value>
    /// The damping which is applied when the user drags the content beyond the limit. The default
    /// value is 12. Higher values increase damping.
    /// </value>
    public static float SpringDamping
    {
      get { return _springDamping; }
      set { _springDamping = value; }
    }
    private static float _springDamping = 12.0f;


    /// <summary>
    /// Phone/tablet only: Gets or sets the max spring length which defines how far the user can
    /// drag the content beyond the limit.
    /// </summary>
    /// <value>
    /// The max spring length which defines how far the user can drag the content beyond the limit.
    /// The default value is 100.
    /// </value>
    public static float SpringLength
    {
      get { return _springLength; }
      set { _springLength = value; }
    }
    private static float _springLength = 100.0f;


    /// <summary>
    /// Phone/tablet only: Gets or sets the factor that defines the scroll velocity after a flick
    /// gesture.
    /// </summary>
    /// <value>
    /// The factor that defines the scroll velocity after a flick gesture. The default is 0.04.
    /// Use higher values to scroll faster after a flick gesture.
    /// </value>
    public static float FlickScrollVelocityFactor
    {
      get { return _flickScrollVelocityFactor; }
      set { _flickScrollVelocityFactor = value; }
    }
    private static float _flickScrollVelocityFactor = 0.04f;


    /// <summary>
    /// Gets or sets the vertical scroll threshold that the finger movement has to exceed 
    /// to start a scroll action.
    /// </summary>
    /// <value>The scroll threshold in pixels.</value>
    public static float ScrollThreshold
    {
      get { return _scrollThreshold; }
      set { _scrollThreshold = value; }
    }
    private static float _scrollThreshold = 15;


    /// <summary>
    /// Gets the extent width which is equal to the desired width of the 
    /// <see cref="ContentControl.Content"/>.
    /// </summary>
    /// <value>
    /// The extent width which is equal to the desired width of the 
    /// <see cref="ContentControl.Content"/>.
    /// </value>
    public float ExtentWidth { get; private set; }


    /// <summary>
    /// Gets the extent height which is equal to the desired height of the 
    /// <see cref="ContentControl.Content"/>.
    /// </summary>
    /// <value>
    /// The extent height which is equal to the desired height of the 
    /// <see cref="ContentControl.Content"/>.
    /// </value>
    public float ExtentHeight { get; private set; }


    /// <summary>
    /// Gets the width of the viewport which defines the visual part of the 
    /// <see cref="ContentControl.Content"/>.
    /// </summary>
    /// <value>
    /// The width of the viewport which defines the visual part of the 
    /// <see cref="ContentControl.Content"/>.
    /// </value>
    public float ViewportWidth { get; private set; }


    /// <summary>
    /// Gets the height of the viewport which defines the visual part of the 
    /// <see cref="ContentControl.Content"/>.
    /// </summary>
    /// <value>
    /// The height of the viewport which defines the visual part of the 
    /// <see cref="ContentControl.Content"/>.
    /// </value>
    public float ViewportHeight { get; private set; }


    /// <inheritdoc/>
    public override RectangleF ContentBounds
    {
      get { return _contentBounds; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="HorizontalScrollBarVisibility"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int HorizontalScrollBarVisibilityPropertyId = CreateProperty(
      typeof(ScrollViewer), "HorizontalScrollBarVisibility", GamePropertyCategories.Behavior, null,
      ScrollBarVisibility.Auto, UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the <see cref="ScrollBarVisibility"/> of the horizontal 
    /// <see cref="ScrollBar"/>. This is a game object property.
    /// </summary>
    /// <value>
    /// The <see cref="ScrollBarVisibility"/> of the horizontal <see cref="ScrollBar"/>.
    /// </value>
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
      get { return GetValue<ScrollBarVisibility>(HorizontalScrollBarVisibilityPropertyId); }
      set { SetValue(HorizontalScrollBarVisibilityPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="VerticalScrollBarVisibility"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int VerticalScrollBarVisibilityPropertyId = CreateProperty(
      typeof(ScrollViewer), "VerticalScrollBarVisibility", GamePropertyCategories.Behavior, null,
      ScrollBarVisibility.Auto, UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the <see cref="ScrollBarVisibility"/> of the vertical <see cref="ScrollBar"/>. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The <see cref="ScrollBarVisibility"/> of the vertical <see cref="ScrollBar"/>.
    /// </value>
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
      get { return GetValue<ScrollBarVisibility>(VerticalScrollBarVisibilityPropertyId); }
      set { SetValue(VerticalScrollBarVisibilityPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="HorizontalOffset"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int HorizontalOffsetPropertyId = CreateProperty(
      typeof(ScrollViewer), "HorizontalOffset", GamePropertyCategories.Layout, null, 0.0f,
      UIPropertyOptions.AffectsArrange);

    /// <summary>
    /// Gets or sets the horizontal offset (= the <see cref="RangeBase.Value"/> of the 
    /// horizontal <see cref="ScrollBar"/>). This is a game object property.
    /// </summary>
    /// <value>
    /// The horizontal offset (= the <see cref="RangeBase.Value"/> of the horizontal 
    /// <see cref="ScrollBar"/>).
    /// </value>
    public float HorizontalOffset
    {
      get { return GetValue<float>(HorizontalOffsetPropertyId); }
      set { SetValue(HorizontalOffsetPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="VerticalOffset"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int VerticalOffsetPropertyId = CreateProperty(
      typeof(ScrollViewer), "VerticalOffset", GamePropertyCategories.Layout, null, 0.0f,
      UIPropertyOptions.AffectsArrange);

    /// <summary>
    /// Gets or sets the vertical offset (= the <see cref="RangeBase.Value"/> of the vertical 
    /// <see cref="ScrollBar"/>). This is a game object property.
    /// </summary>
    /// <value>
    /// The vertical offset (= the <see cref="RangeBase.Value"/> of the vertical 
    /// <see cref="ScrollBar"/>).
    /// </value>
    public float VerticalOffset
    {
      get { return GetValue<float>(VerticalOffsetPropertyId); }
      set { SetValue(VerticalOffsetPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="HorizontalScrollBarStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int HorizontalScrollBarStylePropertyId = CreateProperty(
      typeof(ScrollViewer), "HorizontalScrollBarStyle", GamePropertyCategories.Style, null,
      "HorizontalScrollBar", UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the horizontal <see cref="ScrollBar"/>. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the horizontal <see cref="ScrollBar"/>. Can be 
    /// <see langword="null"/> or an empty string to hide the scroll bar.
    /// </value>
    public string HorizontalScrollBarStyle
    {
      get { return GetValue<string>(HorizontalScrollBarStylePropertyId); }
      set { SetValue(HorizontalScrollBarStylePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="VerticalScrollBarStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int VerticalScrollBarStylePropertyId = CreateProperty(
      typeof(ScrollViewer), "VerticalScrollBarStyle", GamePropertyCategories.Style, null,
      "VerticalScrollBar", UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the vertical <see cref="ScrollBar"/>. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the vertical <see cref="ScrollBar"/>. Can be 
    /// <see langword="null"/> or an empty string to hide the title.
    /// </value>
    public string VerticalScrollBarStyle
    {
      get { return GetValue<string>(VerticalScrollBarStylePropertyId); }
      set { SetValue(VerticalScrollBarStylePropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollViewer"/> class.
    /// </summary>
    public ScrollViewer()
    {
      Style = "ScrollViewer";
      ClipContent = true;

#if WP7 || PORTABLE
      _isTouchDevice = GlobalSettings.PlatformID == PlatformID.WindowsPhone7
                       || GlobalSettings.PlatformID == PlatformID.WindowsPhone8
                       || GlobalSettings.PlatformID == PlatformID.WindowsStore
                       || GlobalSettings.PlatformID == PlatformID.iOS
                       || GlobalSettings.PlatformID == PlatformID.Android;

      TouchPanel.EnabledGestures |= GestureType.Flick;

      // Update _virtualHorizontalOffset whenever HorizontalOffset is changes.
      var horizontalOffsetProperty = Properties.Get<float>(HorizontalOffsetPropertyId);
      horizontalOffsetProperty.Changed += (s, e) => _virtualOffset.X = e.NewValue;

      // Update _virtualVerticalOffset whenever VerticalOffset changes.
      var verticalOffsetProperty = Properties.Get<float>(VerticalOffsetPropertyId);
      verticalOffsetProperty.Changed += (s, e) => _virtualOffset.Y = e.NewValue;
#endif
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnLoad()
    {
      base.OnLoad();

      // Create horizontal scroll bar.
      var horizontalScrollBarStyle = HorizontalScrollBarStyle;
      if (!string.IsNullOrEmpty(horizontalScrollBarStyle))
      {
        _horizontalScrollBar = new ScrollBar
        {
          Style = horizontalScrollBarStyle,
          Value = HorizontalOffset,
        };
        VisualChildren.Add(_horizontalScrollBar);

        // If not set, we set a fixed Height for the scroll bar. This simplifies our layout process.
        if (Numeric.IsNaN(_horizontalScrollBar.Height))
          _horizontalScrollBar.Height = 16;

        // Connect ScrollBar.Value with HorizontalOffset (two-way connection).
        var scrollBarValue = _horizontalScrollBar.Properties.Get<float>(RangeBase.ValuePropertyId);
        var horizontalOffset = Properties.Get<float>(HorizontalOffsetPropertyId);
        scrollBarValue.Changed += horizontalOffset.Change;
        horizontalOffset.Changed += scrollBarValue.Change;

#if WP7 || PORTABLE
        if (_isTouchDevice)
          _horizontalScrollBar.Opacity = 0;
#endif
      }

      // Create vertical scroll bar.
      var verticalScrollBarStyle = VerticalScrollBarStyle;
      if (!string.IsNullOrEmpty(verticalScrollBarStyle))
      {
        _verticalScrollBar = new ScrollBar
        {
          Style = verticalScrollBarStyle,
          Value = VerticalOffset,
        };
        VisualChildren.Add(_verticalScrollBar);

        // If not set, we set a fixed Width for the scroll bar. This simplifies our layout process.
        if (Numeric.IsNaN(_verticalScrollBar.Width))
          _verticalScrollBar.Width = 16;

        // Connect ScrollBar.Value with VerticalOffset (two-way connection).
        var scrollBarValue = _verticalScrollBar.Properties.Get<float>(RangeBase.ValuePropertyId);
        var verticalOffset = Properties.Get<float>(VerticalOffsetPropertyId);
        scrollBarValue.Changed += verticalOffset.Change;
        verticalOffset.Changed += scrollBarValue.Change;

#if WP7 || PORTABLE
        if (_isTouchDevice)
          _verticalScrollBar.Opacity = 0;
#endif
      }
    }


    /// <inheritdoc/>
    protected override void OnUnload()
    {
      // Remove scroll bars.
      if (_horizontalScrollBar != null)
      {
        var scrollBarValue = _horizontalScrollBar.Properties.Get<float>(RangeBase.ValuePropertyId);
        var horizontalOffset = Properties.Get<float>(HorizontalOffsetPropertyId);

        scrollBarValue.Changed -= horizontalOffset.Change;
        horizontalOffset.Changed -= scrollBarValue.Change;

        VisualChildren.Remove(_horizontalScrollBar);
        _horizontalScrollBar = null;
      }

      if (_verticalScrollBar != null)
      {
        var scrollBarValue = _verticalScrollBar.Properties.Get<float>(RangeBase.ValuePropertyId);
        var verticalOffset = Properties.Get<float>(VerticalOffsetPropertyId);

        scrollBarValue.Changed -= verticalOffset.Change;
        verticalOffset.Changed -= scrollBarValue.Change;

        VisualChildren.Remove(_verticalScrollBar);
        _verticalScrollBar = null;
      }

      base.OnUnload();
    }


    /// <inheritdoc/>
    protected override Vector2F OnMeasure(Vector2F availableSize)
    {
      float width = Width;
      float height = Height;
      bool hasWidth = Numeric.IsPositiveFinite(width);
      bool hasHeight = Numeric.IsPositiveFinite(height);
      bool hasContent = (Content != null);

      if (hasWidth && width < availableSize.X)
        availableSize.X = width;
      if (hasHeight && height < availableSize.Y)
        availableSize.Y = height;

      // Measure all children, except the ScrollBars and the Content.
      foreach (var child in VisualChildren)
        if (child != _horizontalScrollBar && child != _verticalScrollBar && child != Content)
          child.Measure(availableSize);

      Vector4F padding = Padding;

      // Determine the extent size (the desired size of the content).
      ExtentWidth = 0;
      ExtentHeight = 0;
      if (hasContent)
      {
        // Pass 1: Assume that scroll bars are invisible and that the viewport takes
        // up all available space.
        Vector2F viewportSize = new Vector2F(availableSize.X - padding.X - padding.Z, availableSize.Y - padding.Y - padding.W);
        var horizontalScrollBarVisibility = HorizontalScrollBarVisibility;
        var verticalScrollBarVisibility = VerticalScrollBarVisibility;
        bool horizontalScrollBarVisible, verticalScrollBarVisible;
        CalculateViewport(availableSize, padding, horizontalScrollBarVisibility, verticalScrollBarVisibility, out horizontalScrollBarVisible, out verticalScrollBarVisible, ref viewportSize);

        // When a scroll bar is not "disabled" then the content can use all desired
        // space in the according direction.
        Vector2F contentSize = viewportSize;
        if (horizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
          contentSize.X = float.PositiveInfinity;
        if (verticalScrollBarVisibility != ScrollBarVisibility.Disabled)
          contentSize.Y = float.PositiveInfinity;

        Content.Measure(contentSize);
        ExtentWidth = Content.DesiredWidth;
        ExtentHeight = Content.DesiredHeight;

        // Pass 2: It is possible that a scroll bar was made visible in Pass 1 and
        // has reduced the viewport size.
        CalculateViewport(availableSize, padding, horizontalScrollBarVisibility, verticalScrollBarVisibility, out horizontalScrollBarVisible, out verticalScrollBarVisible, ref viewportSize);
        contentSize = viewportSize;
        if (horizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
          contentSize.X = float.PositiveInfinity;
        if (verticalScrollBarVisibility != ScrollBarVisibility.Disabled)
          contentSize.Y = float.PositiveInfinity;

        Content.Measure(contentSize);
        ExtentWidth = Content.DesiredWidth;
        ExtentHeight = Content.DesiredHeight;
      }

      // The desired size is determined by the VisualChildren - except the Content.
      // The Content is only relevant if the scroll bars are disabled.
      Vector2F desiredSize = new Vector2F(padding.X + padding.Z, padding.Y + padding.W);
      if (hasWidth)
      {
        desiredSize.X = width;
      }
      else
      {
        foreach (var child in VisualChildren)
        {
          if (child == Content)
            desiredSize.X = Math.Max(desiredSize.X, padding.X + child.DesiredWidth + padding.Z);
          else
            desiredSize.X = Math.Max(desiredSize.X, child.DesiredWidth);
        }
      }

      if (hasHeight)
      {
        desiredSize.Y = height;
      }
      else
      {
        foreach (var child in VisualChildren)
        {
          if (child == Content)
            desiredSize.Y = Math.Max(desiredSize.Y, padding.Y + child.DesiredHeight + padding.W);
          else
            desiredSize.Y = Math.Max(desiredSize.Y, child.DesiredHeight);
        }
      }

      return desiredSize;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    protected override void OnArrange(Vector2F position, Vector2F size)
    {
      // This method handles only the content and the scroll bars. Other visual children are
      // ignored.

      Vector4F padding = Padding;
      var horizontalScrollBarVisibility = HorizontalScrollBarVisibility;
      var verticalScrollBarVisibility = VerticalScrollBarVisibility;

      // Determine visibility of scroll bars and size of viewport.
      // Pass 1: Assume that scroll bars are invisible and that the viewport takes
      // up all available space.
      Vector2F viewportSize = new Vector2F(size.X - padding.X - padding.Z, size.Y - padding.Y - padding.W);
      bool horizontalScrollBarVisible, verticalScrollBarVisible;
      CalculateViewport(size, padding, horizontalScrollBarVisibility, verticalScrollBarVisibility, out horizontalScrollBarVisible, out verticalScrollBarVisible, ref viewportSize);

      // Pass 2: It is possible that a scroll bar was made visible in Pass 1 and has
      // reduced the viewport size so that the other scrollbar is needed too.
      CalculateViewport(size, padding, horizontalScrollBarVisibility, verticalScrollBarVisibility, out horizontalScrollBarVisible, out verticalScrollBarVisible, ref viewportSize);

      // Now, we know if the bars are visible and the exact viewport size.
      Vector2F contentPosition = new Vector2F(
        position.X + padding.X,
        position.Y + padding.Y);

      float verticalOverlap = 0;
      float horizontalOverlap = 0;
#if WP7 || PORTABLE
      if (_isTouchDevice)
      {
        // On phone the bars overlap the content area.
        // --> Make sure that the scroll bars do not overlap each other.
        if (horizontalScrollBarVisible)
          verticalOverlap = _horizontalScrollBar.DesiredHeight;
        if (verticalScrollBarVisible)
          horizontalOverlap = _verticalScrollBar.DesiredWidth;
      }
#endif

      // Set bar properties. Remeasure with the new properties and arrange the bars.
      if (horizontalScrollBarVisible)
      {
        _horizontalScrollBar.Minimum = 0;
        _horizontalScrollBar.Maximum = Math.Max(0, ExtentWidth - viewportSize.X);
        _horizontalScrollBar.ViewportSize = Math.Min(1, viewportSize.X / ExtentWidth);
        _horizontalScrollBar.Arrange(
          new Vector2F(contentPosition.X, position.Y + size.Y - padding.W - _horizontalScrollBar.DesiredHeight),
          new Vector2F(viewportSize.X - horizontalOverlap, _horizontalScrollBar.DesiredHeight));
      }

      if (verticalScrollBarVisible)
      {
        _verticalScrollBar.Minimum = 0;
        _verticalScrollBar.Maximum = Math.Max(0, ExtentHeight - viewportSize.Y);
        _verticalScrollBar.ViewportSize = Math.Min(1, viewportSize.Y / ExtentHeight);
        _verticalScrollBar.Arrange(
          new Vector2F(position.X + size.X - padding.Z - _verticalScrollBar.DesiredWidth, contentPosition.Y),
          new Vector2F(_verticalScrollBar.DesiredWidth, viewportSize.Y - verticalOverlap));
      }

      // Store content bounds for clipping.
      _contentBounds = new RectangleF(contentPosition.X, contentPosition.Y, viewportSize.X, viewportSize.Y);
      ViewportWidth = viewportSize.X;
      ViewportHeight = viewportSize.Y;

      var content = Content;
      if (content == null)
        return;

      // Get content position and content size. Consider disabled bars and the scrolling offsets.
      Vector2F contentSize = new Vector2F(content.DesiredWidth, content.DesiredHeight);

      if (horizontalScrollBarVisibility == ScrollBarVisibility.Disabled)
        contentSize.X = viewportSize.X;
      else
        contentPosition.X -= HorizontalOffset;

      if (verticalScrollBarVisibility == ScrollBarVisibility.Disabled)
        contentSize.Y = viewportSize.Y;
      else
        contentPosition.Y -= VerticalOffset;

      if (content.HorizontalAlignment == HorizontalAlignment.Stretch)
        contentSize.X = Math.Max(contentSize.X, viewportSize.X);

      if (content.VerticalAlignment == VerticalAlignment.Stretch)
        contentSize.Y = Math.Max(contentSize.Y, viewportSize.Y);

      content.Arrange(contentPosition, contentSize);
    }


    private void CalculateViewport(Vector2F size, Vector4F padding, ScrollBarVisibility horizontalScrollBarVisibility, ScrollBarVisibility verticalScrollBarVisibility,
                                   out bool horizontalScrollBarVisible, out bool verticalScrollBarVisible, ref Vector2F viewportSize)
    {
      // Determine whether scroll bars are visible.
      horizontalScrollBarVisible = false;
      if (_horizontalScrollBar != null)
      {
        // When the visibility is "Auto", the bar is only visible when content is too large for the viewport.
        if (horizontalScrollBarVisibility == ScrollBarVisibility.Auto)
          horizontalScrollBarVisible = (ExtentWidth > viewportSize.X);
        else if (horizontalScrollBarVisibility == ScrollBarVisibility.Visible)
          horizontalScrollBarVisible = true;

        _horizontalScrollBar.IsVisible = horizontalScrollBarVisible;
        if (horizontalScrollBarVisible)
          _horizontalScrollBar.Measure(size);
      }

      verticalScrollBarVisible = false;
      if (_verticalScrollBar != null)
      {
        // When the visibility is "Auto", the bar is only visible when content is too large for the viewport.
        if (verticalScrollBarVisibility == ScrollBarVisibility.Auto)
          verticalScrollBarVisible = (ExtentHeight > viewportSize.Y);
        else if (verticalScrollBarVisibility == ScrollBarVisibility.Visible)
          verticalScrollBarVisible = true;

        _verticalScrollBar.IsVisible = verticalScrollBarVisible;
        if (verticalScrollBarVisible)
          _verticalScrollBar.Measure(size);
      }

      // Subtract scroll bars from viewport size.
#if !WP7
#if PORTABLE
      if (!_isTouchDevice)
#endif
      {
        // The bar area has to be removed from the content area. (Note: On phone/tablet
        // the bars overlap the content area. They are invisible most of the time.)
        if (horizontalScrollBarVisible)
          viewportSize.Y = size.Y - padding.Y - padding.W - _horizontalScrollBar.DesiredHeight;
        if (verticalScrollBarVisible)
          viewportSize.X = size.X - padding.X - padding.Z - _verticalScrollBar.DesiredWidth;
      }
#endif
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
    protected override void OnHandleInput(InputContext context)
    {
      var inputService = InputService;

#if WP7 || PORTABLE
      if (_isTouchDevice)
      {
        // Scrolling on phone/tablet has priority over the actions of the visual children.
        // Therefore base.OnHandleInput() is called after this code section.

        Vector2F mousePosition = inputService.MousePosition;
        float t = (float)context.DeltaTime.TotalSeconds;
        if (_isDragging)
        {
          if (inputService.IsMouseOrTouchHandled || inputService.IsUp(MouseButtons.Left))
          {
            // Dragging ends.
            _isDragging = false;

            // Check flick gesture.
            foreach (var gesture in inputService.Gestures)
            {
              if (gesture.GestureType == GestureType.Flick)
              {
                // Flick detected.
                // --> Set a scroll velocity proportional to the flick delta.
                _scrollVelocity = (Vector2F)(-gesture.Delta * FlickScrollVelocityFactor / t);
                _scrollVelocity = Vector2F.Clamp(_scrollVelocity, -MaxScrollVelocity, MaxScrollVelocity);

                inputService.IsMouseOrTouchHandled = true;
                break;
              }
            }
          }
          else
          {
            // Dragging continues.
            bool canScrollHorizontally = (ExtentWidth > ViewportWidth);
            bool canScrollVertically = (ExtentHeight > ViewportHeight);
            if (!_scrollToleranceExceeded)
            {
              // Check if drag tolerance has been exceeded.
              if (canScrollHorizontally && Math.Abs(mousePosition.X - _scrollStartPosition.X) > _scrollThreshold
                  || canScrollVertically && Math.Abs(mousePosition.Y - _scrollStartPosition.Y) > _scrollThreshold)
              {
                // Start dragging. (Use current mouse position to avoid a "jump".)
                _scrollStartPosition = mousePosition;
                _scrollToleranceExceeded = true;
              }
            }

            if (_scrollToleranceExceeded)
            {
              // Drag content.
              if (canScrollHorizontally && inputService.MousePositionDelta.X != 0
                  || canScrollVertically && inputService.MousePositionDelta.Y != 0)
              {
                inputService.IsMouseOrTouchHandled = true;

                Vector2F minOffset = new Vector2F(0, 0);
                Vector2F maxOffset = new Vector2F(Math.Max(ExtentWidth - ViewportWidth, 0),
                  Math.Max(ExtentHeight - ViewportHeight, 0));
                Vector2F minVirtualOffset = minOffset - new Vector2F(SpringLength);
                Vector2F maxVirtualOffset = maxOffset + new Vector2F(SpringLength);
                Vector2F newOffset = _scrollStartOffset + _scrollStartPosition - mousePosition;

                if (canScrollHorizontally)
                {
                  HorizontalOffset = MathHelper.Clamp(newOffset.X, minOffset.X, maxOffset.X);
                  _virtualOffset.X = MathHelper.Clamp(newOffset.X, minVirtualOffset.X, maxVirtualOffset.X);
                }

                if (canScrollVertically)
                {
                  VerticalOffset = MathHelper.Clamp(newOffset.Y, minOffset.Y, maxOffset.Y);
                  _virtualOffset.Y = MathHelper.Clamp(newOffset.Y, minVirtualOffset.Y, maxVirtualOffset.Y);
                }

                _scrollVelocity = -inputService.MousePositionDelta / t;
                _scrollVelocity = Vector2F.Clamp(_scrollVelocity, -MaxScrollVelocity, MaxScrollVelocity);
              }
            }
          }
        }
        else
        {
          if (!inputService.IsMouseOrTouchHandled
              && inputService.IsPressed(MouseButtons.Left, false)
              && IsMouseOver)
          {
            // Dragging starts.
            _isDragging = true;

            // Remember the mouse position.
            _scrollStartPosition = mousePosition;
            _scrollStartOffset = _virtualOffset;
            _scrollToleranceExceeded = false;
          }
        }

        if (!inputService.IsMouseOrTouchHandled && inputService.IsDown(MouseButtons.Left))
          _scrollVelocity = Vector2F.Zero;
      }
#endif

      base.OnHandleInput(context);

      if (!IsLoaded)
        return;

#if !WP7 && !XBOX
      // Mouse wheel scrolls vertically when the mouse cursor is over the scroll viewer.
      if (!inputService.IsMouseOrTouchHandled && IsMouseOver)
      {
        if (inputService.MouseWheelDelta != 0
            && VerticalScrollBarVisibility != ScrollBarVisibility.Disabled
            && _verticalScrollBar != null)
        {
          inputService.IsMouseOrTouchHandled = true;

          var screen = Screen;
          float offset = inputService.MouseWheelDelta / screen.MouseWheelScrollDelta * screen.MouseWheelScrollLines;
          offset *= _verticalScrollBar.SmallChange;
          offset = VerticalOffset - offset;
          if (offset < 0)
            offset = 0;
          if (offset > ExtentHeight - ViewportHeight)
            offset = ExtentHeight - ViewportHeight;

          VerticalOffset = offset;
        }
      }
#endif

      // Scroll with game pad right stick.
      if (!inputService.IsGamePadHandled(context.AllowedPlayer))
      {
        var gamePadState = inputService.GetGamePadState(context.AllowedPlayer);
        Vector2 rightStick = gamePadState.ThumbSticks.Right;
        float x = rightStick.X;
        float y = rightStick.Y;

        if (!Numeric.IsZero(x + y) && IsInActiveWindow())
        {
          if (_horizontalScrollBar != null)
          {
            float offset = HorizontalOffset + 0.5f * x * _horizontalScrollBar.SmallChange;
            offset = MathHelper.Clamp(offset, 0, ExtentWidth - ViewportWidth);
            HorizontalOffset = offset;
          }

          if (_verticalScrollBar != null)
          {
            float offset = VerticalOffset - 0.5f * y * _verticalScrollBar.SmallChange;
            offset = MathHelper.Clamp(offset, 0, ExtentHeight - ViewportHeight);
            VerticalOffset = offset;
          }

          inputService.SetGamePadHandled(context.AllowedPlayer, true);
        }
      }
    }


    private bool IsInActiveWindow()
    {
      var window = Window.GetWindow(this);
      if (window != null)
        return window.IsActive;

      // Not in a window. Only scroll if the screen is a focus scope and does not
      // have any child windows.
      var screen = Screen;
      if (!screen.IsFocusScope)
        return false;

      for (int i = 0; i < screen.Children.Count; i++)
        if (screen.Children[i] is Window)
          return false;

      // A screen with a focus scope and no child windows.
      return true;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    protected override void OnUpdate(TimeSpan deltaTime)
    {
#if WP7 || PORTABLE
      float t = (float)deltaTime.TotalSeconds;
      bool canScrollHorizontally = (ExtentWidth > ViewportWidth);
      bool canScrollVertically = (ExtentHeight > ViewportHeight);

      Vector2F minOffset = new Vector2F(0, 0);
      Vector2F maxOffset = new Vector2F(Math.Max(ExtentWidth - ViewportWidth, 0), Math.Max(ExtentHeight - ViewportHeight, 0));
      Vector2F minVirtualOffset = minOffset - new Vector2F(SpringLength);
      Vector2F maxVirtualOffset = maxOffset + new Vector2F(SpringLength);

      if (!_isDragging)
      {
        // The user is currently not interacting with the content:
        // Move content on with current velocity.

        // ----- Spring
        // Apply spring force if user has dragged content beyond the limit.
        Vector2F springLength = new Vector2F();
        if (_virtualOffset.X < minOffset.X)
          springLength.X = minOffset.X - _virtualOffset.X;
        else if (_virtualOffset.X > maxOffset.X)
          springLength.X = maxOffset.X - _virtualOffset.X;
        if (_virtualOffset.Y < minOffset.Y)
          springLength.Y = minOffset.Y - _virtualOffset.Y;
        else if (_virtualOffset.Y > maxOffset.Y)
          springLength.Y = maxOffset.Y - _virtualOffset.Y;

        _scrollVelocity = _scrollVelocity + SpringConstant * springLength * t;

        // ----- Damping
        // Apply damping to velocity.
        // See DigitalRune Blog for an explanation of damping formulas:
        // http://www.digitalrune.com/Support/Blog/tabid/719/EntryId/78/Damping-in-Computer-Games.aspx

        // Use a stronger damping if the spring is active.
        Vector2F damping = new Vector2F(springLength.X != 0 ? SpringDamping : ScrollDamping,
                                        springLength.Y != 0 ? SpringDamping : ScrollDamping);

        _scrollVelocity = (Vector2F.One - Vector2F.Min(damping * t, Vector2F.One)) * _scrollVelocity;

        // ----- Position Update
        // Compute new scroll offset.
        Vector2F newOffset = _virtualOffset + _scrollVelocity * t;

        // ----- Limits
        // Stop scroll velocity when we hit the max spring length.
        if (newOffset.X < minVirtualOffset.X || newOffset.X > maxVirtualOffset.X)
          _scrollVelocity.X = 0;
        if (newOffset.Y < minVirtualOffset.Y || newOffset.Y > maxVirtualOffset.Y)
          _scrollVelocity.Y = 0;

        // ----- Snapping
        // Snap to min or max offset when the spring pushes the content back.
        if (_virtualOffset.X < minOffset.X && newOffset.X > minOffset.X)
        {
          newOffset.X = minOffset.X;
          _scrollVelocity.X = 0;
        }
        else if (_virtualOffset.X > maxOffset.X && newOffset.X < maxOffset.X)
        {
          newOffset.X = maxOffset.X;
          _scrollVelocity.X = 0;
        }
        if (_virtualOffset.Y < minOffset.Y && newOffset.Y > minOffset.Y)
        {
          newOffset.Y = minOffset.Y;
          _scrollVelocity.Y = 0;
        }
        else if (_virtualOffset.Y > maxOffset.Y && newOffset.Y < maxOffset.Y)
        {
          newOffset.Y = maxOffset.Y;
          _scrollVelocity.Y = 0;
        }

        // ----- Velocity Clamping
        // When the velocity reaches a min limit, clamp it to zero. Otherwise, the content 
        // never really stops.
        if (_scrollVelocity.LengthSquared < MinScrollVelocity * MinScrollVelocity)
          _scrollVelocity = Vector2F.Zero;

        // ----- Update HorizontalOffset and VerticalOffset.
        if (canScrollHorizontally)
        {
          HorizontalOffset = MathHelper.Clamp(newOffset.X, minOffset.X, maxOffset.X);
          _virtualOffset.X = MathHelper.Clamp(newOffset.X, minVirtualOffset.X, maxVirtualOffset.X);
        }

        if (canScrollVertically)
        {
          VerticalOffset = MathHelper.Clamp(newOffset.Y, minOffset.Y, maxOffset.Y);
          _virtualOffset.Y = MathHelper.Clamp(newOffset.Y, minVirtualOffset.Y, maxVirtualOffset.Y);
        }
      }

      // ----- Scale Transform
      if (Content != null)
      {
        // Apply scale transform to content if it is pushed beyond the limit.
        Vector2F scale = Vector2F.One;
        Vector2F transformOrigin = new Vector2F(0.5f);

        // Scale content horizontally.
        if (_virtualOffset.X < minOffset.X)
        {
          // User pushes content to the right.
          scale.X = (ExtentWidth - (minOffset.X - _virtualOffset.X)) / ExtentWidth;
          transformOrigin.X = 1.0f;
        }
        else if (_virtualOffset.X > maxOffset.X)
        {
          // User pushes content to the left.
          scale.X = (ExtentWidth - (_virtualOffset.X - maxOffset.X)) / ExtentWidth;
          transformOrigin.X = 0.0f;
        }

        // Scale content vertically.
        if (_virtualOffset.Y < minOffset.Y)
        {
          // User pushes content down.
          scale.Y = (ExtentHeight - (minOffset.Y - _virtualOffset.Y)) / ExtentHeight;
          transformOrigin.Y = 1.0f;
        }
        else if (_virtualOffset.Y > maxOffset.Y)
        {
          // User pushes content up.
          scale.Y = (ExtentHeight - (_virtualOffset.Y - maxOffset.Y)) / ExtentHeight;
          transformOrigin.Y = 0.0f;
        }

        Content.RenderScale = scale;
        Content.RenderTransformOrigin = transformOrigin;
      }

      // ----- ScrollBar Opacity Animation
      if (_isTouchDevice)
      {
        // Animate opacity of horizontal and vertical scroll bars.
        if (!_isDragging && _scrollVelocity.LengthSquared < 1)
        {
          // Fade out.
          if (_horizontalScrollBar != null
              && _horizontalScrollBar.IsVisible
              && _horizontalScrollBar.Opacity > 0)
          {
            _horizontalScrollBar.Opacity = MathHelper.Clamp(_horizontalScrollBar.Opacity - ScrollBarFadeVelocity * t, 0,
              1);
          }
          if (_verticalScrollBar != null
              && _verticalScrollBar.IsVisible
              && _verticalScrollBar.Opacity > 0)
          {
            _verticalScrollBar.Opacity = MathHelper.Clamp(_verticalScrollBar.Opacity - ScrollBarFadeVelocity * t, 0, 1);
          }
        }
        else if (_scrollVelocity.LengthSquared >= 1)
        {
          // Fade in.
          if (_horizontalScrollBar != null
              && _horizontalScrollBar.IsVisible
              && canScrollHorizontally
              && _horizontalScrollBar.Opacity < 1)
          {
            _horizontalScrollBar.Opacity = MathHelper.Clamp(
              _horizontalScrollBar.Opacity + 2 * ScrollBarFadeVelocity * t, 0, 1);
          }
          if (_verticalScrollBar != null
              && _verticalScrollBar.IsVisible
              && canScrollVertically
              && _verticalScrollBar.Opacity < 1)
          {
            _verticalScrollBar.Opacity = MathHelper.Clamp(_verticalScrollBar.Opacity + 2 * ScrollBarFadeVelocity * t, 0,
              1);
          }
        }
      }
#else
      // Coerce offsets to allowed range.
      Vector2F minOffset = new Vector2F(0, 0);
      Vector2F maxOffset = new Vector2F(Math.Max(ExtentWidth - ViewportWidth, 0), Math.Max(ExtentHeight - ViewportHeight, 0));

      float horizontalOffset = HorizontalOffset;
      if (horizontalOffset < minOffset.X)
        HorizontalOffset = minOffset.X;
      else if (horizontalOffset > maxOffset.X)
        HorizontalOffset = maxOffset.X;

      float verticalOffset = VerticalOffset;
      if (verticalOffset < minOffset.Y)
        VerticalOffset = minOffset.Y;
      else if (verticalOffset > maxOffset.Y)
        VerticalOffset = maxOffset.Y;
#endif

      base.OnUpdate(deltaTime);
    }


    /// <summary>
    /// Changes <see cref="HorizontalOffset"/> and <see cref="VerticalOffset"/> such that the
    /// <paramref name="control"/> is visible in the viewport.
    /// </summary>
    /// <param name="control">The control.</param>
    internal void BringIntoView(UIControl control)
    {
      if (!control.IsArrangeValid)
        control.UpdateLayout();

      BringIntoView(control.ActualBounds);
    }


    /// <summary>
    /// Changes <see cref="HorizontalOffset"/> and <see cref="VerticalOffset"/> such that the given
    /// rectangle (in screen coordinates) is scrolled into the viewport.
    /// </summary>
    private void BringIntoView(RectangleF actualBounds)
    {
      BringIntoViewHorizontal(actualBounds);
      BringIntoViewVertical(actualBounds);
    }


    /// <summary>
    /// Changes <see cref="HorizontalOffset"/> such that the given rectangle (in screen coordinates)
    /// is scrolled into the viewport.
    /// </summary>
    private void BringIntoViewHorizontal(RectangleF actualBounds)
    {
      float viewportOrigin = ActualX + Padding.X;

      if (actualBounds.X < viewportOrigin && actualBounds.Width > ViewportWidth)
        return; // Control is visible and too large for the viewport.

      float x = actualBounds.X;
      float horizontalOffset = HorizontalOffset;
      if (x + actualBounds.Width > viewportOrigin + ViewportWidth)
      {
        // End of control is not visible.
        float delta = (x + actualBounds.Width) - (viewportOrigin + ViewportWidth);
        horizontalOffset += delta;
        x -= delta;
      }

      if (x < viewportOrigin)
      {
        // Start of control is not visible.
        float delta = viewportOrigin - x;
        horizontalOffset -= delta;
      }

      HorizontalOffset = horizontalOffset;
    }


    /// <summary>
    /// Changes <see cref="VerticalOffset"/> such that the given rectangle (in screen coordinates)
    /// is scrolled into the viewport.
    /// </summary>
    private void BringIntoViewVertical(RectangleF actualBounds)
    {
      float viewportOrigin = ActualY + Padding.Y;

      if (actualBounds.Y < viewportOrigin && actualBounds.Height > ViewportHeight)
        return; // Control is visible and too large for the viewport.

      float y = actualBounds.Y;
      float verticalOffset = VerticalOffset;
      if (y + actualBounds.Height > viewportOrigin + ViewportHeight)
      {
        // End of control is not visible.
        float delta = (y + actualBounds.Height) - (viewportOrigin + ViewportHeight);
        verticalOffset += delta;
        y -= delta;
      }

      if (y < viewportOrigin)
      {
        // Start of control is not visible.
        verticalOffset -= viewportOrigin - y;
      }

      VerticalOffset = verticalOffset;
    }
    #endregion
  }
}
