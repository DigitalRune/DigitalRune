// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using DigitalRune.Game.Input;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents a control that provides a scroll bar that has a sliding <see cref="Thumb"/> whose
  /// position corresponds to a value and buttons to change the value.
  /// </summary>
  /// <remarks>
  /// A <see cref="ScrollBar"/> has a sliding <see cref="Thumb"/>. <see cref="RangeBase.Value"/>
  /// defines the position of the thumb. <see cref="ViewportSize"/> defines the size of the thumb.
  /// The scroll bar also has two buttons that can be clicked to change the 
  /// <see cref="RangeBase.Value"/>. The "empty" space between the thumb and the buttons can also
  /// be clicked (like a repeat button) to change the <see cref="RangeBase.Value"/>.
  /// </remarks>
  public class ScrollBar : RangeBase
  {
    // See also comments in Slider.cs!

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Button _decrementButton;
    private Button _incrementButton;
    private Thumb _thumb;

    // Used to detect if the left mouse button was pressed over the control - only then
    // virtual button repeats count.
    private bool _isPressed;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="Orientation"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int OrientationPropertyId = CreateProperty(
      typeof(ScrollBar), "Orientation", GamePropertyCategories.Layout, null, Orientation.Vertical,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the orientation of the scroll bar. 
    /// This is a game object property.
    /// </summary>
    /// <value>The orientation.</value>
    /// <remarks>
    /// Changing this property has no effect after the scroll bar was loaded.
    /// </remarks>
    public Orientation Orientation
    {
      get { return GetValue<Orientation>(OrientationPropertyId); }
      set { SetValue(OrientationPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="ViewportSize"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ViewportSizePropertyId = CreateProperty(
      typeof(ScrollBar), "ViewportSize", GamePropertyCategories.Default, null, 0f,
      UIPropertyOptions.AffectsArrange);

    /// <summary>
    /// Gets or sets the size of the viewport relative to the full extent of the scrollable content. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The size of the viewport relative to the full extent of the scrollable content. This is a 
    /// value in the range ]0, 1]. 0 means that the extent of the scrollable content is infinite 
    /// (which does not happen in practice). 1 means that the full scrollable content is visible in 
    /// the scroll viewer. 0.5 means that the scroll viewer can show half of the scrollable content. 
    /// Etc. The default value is 0.1. 
    /// </value>
    /// <remarks>
    /// The <see cref="ViewportSize"/> defines the size of the draggable <see cref="Thumb"/>.
    /// </remarks>
    public float ViewportSize
    {
      get { return GetValue<float>(ViewportSizePropertyId); }
      set { SetValue(ViewportSizePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="ThumbStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ThumbStylePropertyId = CreateProperty(
      typeof(ScrollBar), "ThumbStyle", GamePropertyCategories.Style, null, "Thumb",
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the <see cref="Thumb"/>. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the <see cref="Thumb"/>. Can be <see langword="null"/> or an 
    /// empty string to hide the thumb.
    /// </value>
    public string ThumbStyle
    {
      get { return GetValue<string>(ThumbStylePropertyId); }
      set { SetValue(ThumbStylePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="DecrementButtonStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int DecrementButtonStylePropertyId = CreateProperty(
      typeof(ScrollBar), "DecrementButtonStyle", GamePropertyCategories.Style, null,
      "ScrollBarButton", UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the button that decreases the 
    /// <see cref="RangeBase.Value"/>. This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the button that decreases the <see cref="RangeBase.Value"/>.
    /// Can be <see langword="null"/> or an empty string to hide the button.
    /// </value>
    public string DecrementButtonStyle
    {
      get { return GetValue<string>(DecrementButtonStylePropertyId); }
      set { SetValue(DecrementButtonStylePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IncrementButtonStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IncrementButtonStylePropertyId = CreateProperty(
      typeof(ScrollBar), "IncrementButtonStyle", GamePropertyCategories.Style, null,
      "ScrollBarButton", UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the button that increments the 
    /// <see cref="RangeBase.Value"/>. This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the button that increments the <see cref="RangeBase.Value"/>.
    /// Can be <see langword="null"/> or an empty string to hide the button.
    /// </value>
    public string IncrementButtonStyle
    {
      get { return GetValue<string>(IncrementButtonStylePropertyId); }
      set { SetValue(IncrementButtonStylePropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="ScrollBar"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static ScrollBar()
    {
      OverrideDefaultValue(typeof(ScrollBar), ViewportSizePropertyId, 0.1f);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollBar"/> class.
    /// </summary>
    public ScrollBar()
    {
      Style = "ScrollBar";
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnLoad()
    {
      base.OnLoad();

      // Create decrement button.
      var decrementButtonStyle = DecrementButtonStyle;
      if (!string.IsNullOrEmpty(decrementButtonStyle))
      {
        _decrementButton = new Button
        {
          Name = "DecrementButton",
          Style = decrementButtonStyle,
          Focusable = false,          // Unlike normal buttons these arrow button can not be focused!
          IsRepeatButton = true,
        };
        VisualChildren.Add(_decrementButton);

        var click = _decrementButton.Events.Get<EventArgs>(ButtonBase.ClickEventId);
        click.Event += OnDecrementButtonClick;
      }

      // Create increment button.
      var incrementButtonStyle = IncrementButtonStyle;
      if (!string.IsNullOrEmpty(incrementButtonStyle))
      {
        _incrementButton = new Button
        {
          Name = "RightDownButton",
          Style = incrementButtonStyle,
          Focusable = false,          // Unlike normal buttons these arrow button can not be focused!
          IsRepeatButton = true,
        };
        VisualChildren.Add(_incrementButton);

        var click = _incrementButton.Events.Get<EventArgs>(ButtonBase.ClickEventId);
        click.Event += OnIncrementButtonClick;
      }

      // Create thumb.
      var thumbStyle = ThumbStyle;
      if (!string.IsNullOrEmpty(thumbStyle))
      {
        _thumb = new Thumb
        {
          Name = "ScrollBarThumb",
          Style = thumbStyle,
        };
        VisualChildren.Add(_thumb);
      }
    }


    /// <inheritdoc/>
    protected override void OnUnload()
    {
      // Remove thumb and buttons.
      VisualChildren.Remove(_thumb);
      _thumb = null;

      if (_decrementButton != null)
      {
        var click = _decrementButton.Events.Get<EventArgs>(ButtonBase.ClickEventId);
        click.Event -= OnDecrementButtonClick;
        VisualChildren.Remove(_decrementButton);
        _decrementButton = null;
      }

      if (_incrementButton != null)
      {
        var click = _incrementButton.Events.Get<EventArgs>(ButtonBase.ClickEventId);
        click.Event += OnIncrementButtonClick;
        VisualChildren.Remove(_incrementButton);
        _incrementButton = null;
      }

      base.OnUnload();
    }


    private void OnDecrementButtonClick(object sender, EventArgs eventArgs)
    {
      Value -= Math.Sign(Maximum - Minimum) * SmallChange;
    }


    private void OnIncrementButtonClick(object sender, EventArgs eventArgs)
    {
      Value += Math.Sign(Maximum - Minimum) * SmallChange;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnHandleInput(InputContext context)
    {
      base.OnHandleInput(context);

      if (!IsLoaded)
        return;

      var inputService = InputService;

      float change = 0;
      float value = Value;
      float minimum = Minimum;
      float maximum = Maximum;
      float range = maximum - minimum;
      Vector4F padding = Padding;

      if (!inputService.IsMouseOrTouchHandled)
      {
        // Check if "empty" space between thumbs and buttons is clicked.
        if (IsMouseDirectlyOver && inputService.IsPressed(MouseButtons.Left, false))
          _isPressed = true;

        if (_isPressed)
          inputService.IsMouseOrTouchHandled = true;

        if (_isPressed && IsMouseDirectlyOver && inputService.IsPressed(MouseButtons.Left, true))
        {
          // The area between the outer repeat buttons and the thumb acts as repeat button that
          // causes a LargeChange.
          if (Orientation == Orientation.Horizontal)
          {
            float thumbPosition = ActualX + (ActualWidth - padding.X - padding.Z) * (value - minimum) / range;
            if (context.MousePosition.X < thumbPosition)
              change -= Math.Sign(range) * LargeChange;
            else
              change += Math.Sign(range) * LargeChange;
          }
          else
          {
            float thumbPosition = ActualY + (ActualHeight - padding.Y - padding.W) * (value - minimum) / range;
            if (context.MousePosition.Y < thumbPosition)
              change -= Math.Sign(range) * LargeChange;
            else
              change += Math.Sign(range) * LargeChange;
          }
        }
        else if (inputService.IsUp(MouseButtons.Left))
        {
          _isPressed = false;
        }
      }
      else
      {
        _isPressed = false;
      }

      if (_thumb != null)
      {
        // Handle thumb dragging.
        if (_thumb.IsDragging && !Numeric.AreEqual(minimum, maximum))
        {
          if (Orientation == Orientation.Horizontal)
          {
            float contentWidth = ActualWidth - padding.X - padding.Z - _thumb.ActualWidth;
            change += _thumb.DragDelta.X / contentWidth * range;
          }
          else
          {
            float contentHeight = ActualHeight - padding.Y - padding.W - _thumb.ActualHeight;
            change += _thumb.DragDelta.Y / contentHeight * range;
          }
        }
      }

      if (change != 0.0f)
      {
        // Set new value.
        Value = value + change;
      }
    }


    /// <inheritdoc/>
    protected override void OnArrange(Vector2F position, Vector2F size)
    {
      // Update X or Y of the thumb to slide it to the correct position.
      if (_thumb != null)
      {
        float value = Value;
        float minimum = Minimum;
        float maximum = Maximum;
        float range = maximum - minimum;
        Vector4F padding = Padding;

        if (Orientation == Orientation.Horizontal)
        {
          float contentWidth = ActualWidth - padding.X - padding.Z;

          // ViewPortSize determines thumb width.
          float thumbWidth = contentWidth * ViewportSize;
          if (thumbWidth < _thumb.MinWidth)
            thumbWidth = _thumb.MinWidth;
          else if (thumbWidth > _thumb.MaxWidth)
            thumbWidth = _thumb.MaxWidth;

          _thumb.Width = thumbWidth;
          _thumb.Measure(new Vector2F(float.PositiveInfinity));

          // Compute movement range of thumb center.
          contentWidth = ActualWidth - padding.X - padding.Z - thumbWidth;

          float thumbCenterPosition = contentWidth / range * (value - Minimum);
          if (Numeric.AreEqual(minimum, maximum))
            thumbCenterPosition = 0;

          _thumb.X = padding.X + thumbCenterPosition;
        }
        else
        {
          float contentHeight = ActualHeight - padding.Y - padding.W;

          float thumbHeight = contentHeight * ViewportSize;
          if (thumbHeight < _thumb.MinHeight)
            thumbHeight = _thumb.MinHeight;
          else if (thumbHeight > _thumb.MaxHeight)
            thumbHeight = _thumb.MaxHeight;

          _thumb.Height = thumbHeight;
          _thumb.Measure(new Vector2F(float.PositiveInfinity));

          contentHeight = ActualHeight - padding.Y - padding.W - thumbHeight;

          float thumbCenterPosition = contentHeight / range * (value - Minimum);
          if (Numeric.AreEqual(minimum, maximum))
            thumbCenterPosition = 0;

          _thumb.Y = padding.Y + thumbCenterPosition;
        }
      }

      base.OnArrange(position, size);
    }
    #endregion
  }
}
