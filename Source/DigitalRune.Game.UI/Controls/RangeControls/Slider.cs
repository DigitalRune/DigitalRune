// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using DigitalRune.Game.Input;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Input;
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif


namespace DigitalRune.Game.UI.Controls
{  
  /// <summary>
  /// Represents a control that lets the user select from a range of values by moving a 
  /// <see cref="Thumb"/> control. 
  /// </summary>
  /// <remarks>
  /// <para>
  /// To change the value, the user can move the thumb or click in the non-thumb area. This area
  /// acts as a repeat button. The slider can also be moved using LEFT/RIGHT/HOME/END on the 
  /// keyboard or the left thumb stick or the directional pad of the gamepad.
  /// </para>
  /// <para>
  /// The area in which the thumb moves can be restricted using the <see cref="UIControl.Padding"/>.
  /// </para>
  /// </remarks>
  /// <example>
  /// The following examples shows how to create a slider and display its value.
  /// <code lang="csharp">
  /// <![CDATA[
  /// var slider = new Slider
  /// {
  ///   Value = 60,
  ///   Margin = new Vector4F(4),
  ///   HorizontalAlignment = HorizontalAlignment.Stretch,
  /// };
  /// 
  /// var textBlock = new TextBlock
  /// {
  ///   Margin = new Vector4F(4, 0, 4, 4),
  ///   Text = "(Value = 60)",
  ///   HorizontalAlignment = HorizontalAlignment.Right
  /// };
  /// 
  /// // To connect the slider with the text box, we need to get the "Value" property.
  /// var valueProperty = slider.Properties.Get<float>("Value");
  /// 
  /// // This property is a GameObjectProperty<float>. We can attach an event handler to 
  /// // the Changed event of the property.
  /// valueProperty.Changed += (s, e) => textBlock.Text = "(Value = " + (int)e.NewValue + ")";
  /// 
  /// panel.Children.Add(slider);
  /// panel.Children.Add(textBlock);
  /// ]]>
  /// </code>
  /// </example>
  public class Slider : RangeBase
  {
    // A slider uses a Thumb as visual child.
    // The horizontal Padding determines the allowed visual range the thumb can move.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Thumb _thumb;

    // Used to detect if the left mouse button was pressed over the control - only then
    // virtual button repeats count.
    private bool _isPressed;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    // IsDirectionReversed
    // IsMoveToPointEnabled
    // IsSnapToTickEnabled
    // Orientation
    // TickFrequency
    // TickPlacement
    // Ticks
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="ThumbStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ThumbStylePropertyId = CreateProperty(
      typeof(Slider), "ThumbStyle", GamePropertyCategories.Style, null, "Thumb", 
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the thumb control. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the thumb control. Can be <see langword="null"/> or an empty 
    /// string to hide the thumb.
    /// </value>
    public string ThumbStyle
    {
      get { return GetValue<string>(ThumbStylePropertyId); }
      set { SetValue(ThumbStylePropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="Slider"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static Slider()
    {
      // Sliders can have the focus.
      OverrideDefaultValue(typeof(Slider), FocusablePropertyId, true);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Slider"/> class.
    /// </summary>
    public Slider()
    {
      Style = "Slider";
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnLoad()
    {
      base.OnLoad();

      // Create thumb.
      var thumbStyle = ThumbStyle;
      if (!string.IsNullOrEmpty(thumbStyle))
      {
        _thumb = new Thumb
        {
          Name = "SliderThumb",
          Style = thumbStyle,
        };

        VisualChildren.Add(_thumb);
      }
    }


    /// <inheritdoc/>
    protected override void OnUnload()
    {
      VisualChildren.Remove(_thumb);
      _thumb = null;

      base.OnUnload();
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

      if (IsFocusWithin)
      {
        // Change the value if Keyboard left/right/home/end is pressed.
        if (!inputService.IsKeyboardHandled)
        {
          if (inputService.IsPressed(Keys.Left, true))
          {
            inputService.IsKeyboardHandled = true;
            change -= Math.Sign(range) * SmallChange;
          }

          if (inputService.IsPressed(Keys.Right, true))
          {
            inputService.IsKeyboardHandled = true;
            change += Math.Sign(range) * SmallChange;
          }

          if (inputService.IsPressed(Keys.Home, true))
          {
            inputService.IsKeyboardHandled = true;
            Value = minimum;
          }

          if (inputService.IsPressed(Keys.End, true))
          {
            inputService.IsKeyboardHandled = true;
            Value = maximum;
          }
        }

#if !SILVERLIGHT
        // Change value if left thumb stick or DPad is pressed.
        if (!inputService.IsGamePadHandled(context.AllowedPlayer))
        {
          if ((inputService.IsPressed(Buttons.LeftThumbstickLeft, true, context.AllowedPlayer))
              || (inputService.IsPressed(Buttons.DPadLeft, true, context.AllowedPlayer)))
          {
            inputService.SetGamePadHandled(context.AllowedPlayer, true);
            change -= Math.Sign(range) * SmallChange;
          }

          if ((inputService.IsPressed(Buttons.LeftThumbstickRight, true, context.AllowedPlayer))
              || (inputService.IsPressed(Buttons.DPadRight, true, context.AllowedPlayer)))
          {
            inputService.SetGamePadHandled(context.AllowedPlayer, true);
            change += Math.Sign(range) * SmallChange;
          }
        }
#endif
      }

      if (!inputService.IsMouseOrTouchHandled)
      {
        // Handle mouse clicks.

        // Remember real physical mouse button presses on slider.
        if (IsMouseDirectlyOver && inputService.IsPressed(MouseButtons.Left, false))
          _isPressed = true;

        // While pressed, the slider "captures" mouse input.
        if (_isPressed)
          inputService.IsMouseOrTouchHandled = true;

        // If the slider was pressed, virtual key presses are registered so that the slider
        // works like a repeat button.
        if (_isPressed && IsMouseDirectlyOver && inputService.IsPressed(MouseButtons.Left, true))
        {
          float thumbPosition = ActualX + (ActualWidth - padding.X - padding.Z) * (value - minimum) / range;
          if (context.MousePosition.X < thumbPosition)
            change -= Math.Sign(range) * LargeChange;
          else
            change += Math.Sign(range) * LargeChange;
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

      // Handle thumb dragging.
      if (_thumb != null && _thumb.IsDragging)
      {
        float contentWidth = ActualWidth - padding.X - padding.Z - _thumb.ActualWidth;
        change += _thumb.DragDelta.X / contentWidth * range;
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
      // Compute new thumb position.
      if (_thumb != null)
      {
        float value = Value;
        float minimum = Minimum;
        float maximum = Maximum;
        float range = maximum - minimum;
        Vector4F padding = Padding;

        float contentWidth = size.X - padding.X - padding.Z - _thumb.DesiredWidth;
        float thumbCenterPosition = contentWidth / range * (value - Minimum);

        // The thumb uses UIControl.X for its positioning.
        _thumb.X = padding.X + thumbCenterPosition;
      }

      base.OnArrange(position, size);
    }
    #endregion
  }
}
