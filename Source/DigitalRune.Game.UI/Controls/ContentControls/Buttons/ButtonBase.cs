// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using DigitalRune.Game.Input;
using Microsoft.Xna.Framework.Input;
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Implements the basic functionality common to button controls.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Buttons can be pressed using the left mouse button, or the ENTER or SPACE keys on the keyboard
  /// or the A button on the gamepad (when the button is focused).
  /// </para>
  /// <para>
  /// <strong>Visual States:</strong> The <see cref="VisualState"/>s of this control are:
  /// "Disabled", "Default", "MouseOver", "Focused", "Pressed"
  /// </para>
  /// </remarks>
  public class ButtonBase : ContentControl
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override string VisualState
    {
      get
      {
        if (!ActualIsEnabled)
          return "Disabled";

        if (IsDown)
          return "Pressed";

        if (IsMouseOver)
          return "MouseOver";

        if (IsFocused)
          return "Focused";

        return "Default";
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="ClickMode"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ClickModePropertyId = CreateProperty(
      typeof(ButtonBase), "ClickMode", GamePropertyCategories.Behavior, null, ClickMode.Release, 
      UIPropertyOptions.None);
    
    /// <summary>
    /// Gets or sets the <see cref="Controls.ClickMode"/>. 
    /// This is a game object property.
    /// </summary>
    /// <value>The <see cref="Controls.ClickMode"/>.</value>
    public ClickMode ClickMode
    {
      get { return GetValue<ClickMode>(ClickModePropertyId); }
      set { SetValue(ClickModePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IsDown"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsDownPropertyId = CreateProperty(
      typeof(ButtonBase), "IsDown", GamePropertyCategories.Default, null, false, 
      UIPropertyOptions.AffectsRender);
    
    /// <summary>
    /// Gets a value indicating whether this button is currently pressed down. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this button is currently pressed down; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsDown
    {
      get { return GetValue<bool>(IsDownPropertyId); }
      private set { SetValue(IsDownPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IsClicked"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsClickedPropertyId = CreateProperty(
      typeof(ButtonBase), "IsClicked", GamePropertyCategories.Default, null, false, 
      UIPropertyOptions.AffectsRender);
    
    /// <summary>
    /// Gets (or sets) a value indicating whether this button was clicked in this frame. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance was clicked in this frame; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsClicked
    {
      get { return GetValue<bool>(IsClickedPropertyId); }
      protected set { SetValue(IsClickedPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Click"/> game object event.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ClickEventId = CreateEvent(
      typeof(ButtonBase), "Click", GamePropertyCategories.Default, null, EventArgs.Empty);
    
    /// <summary>
    /// Occurs when the button is clicked (<see cref="IsClicked"/> changed to 
    /// <see langword="true"/>). This is a game object event.
    /// </summary>
    public event EventHandler<EventArgs> Click
    {
      add
      {
        var click = Events.Get<EventArgs>(ClickEventId);
        click.Event += value;
      }
      remove
      {
        var click = Events.Get<EventArgs>(ClickEventId);
        click.Event -= value;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="ButtonBase"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static ButtonBase()
    {
      // Buttons are focusable by default.
      OverrideDefaultValue(typeof(ButtonBase), FocusablePropertyId, true);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonBase"/> class.
    /// </summary>
    public ButtonBase()
    {
      Style = "ButtonBase";

      // IsClicked raises ClickEvent automatically.
      var isClicked = Properties.Get<bool>(IsClickedPropertyId);
      isClicked.Changed += (s, e) => { if (e.NewValue) OnClick(EventArgs.Empty); };

      // When IsEnabled or IsVisible are changed, OnDisable should be called.
      var isEnabled = Properties.Get<bool>(IsEnabledPropertyId);
      isEnabled.Changed += OnDisable;
      var isVisible = Properties.Get<bool>(IsVisiblePropertyId);
      isVisible.Changed += OnDisable;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnDisable(object sender, GamePropertyEventArgs<bool> eventArgs)
    {
      if (!IsEnabled || !IsVisible)
      {
        // Reset the state.
        IsClicked = false;
        IsDown = false;
      }
    }


    /// <inheritdoc/>
    protected override void OnHandleInput(InputContext context)
    {
      base.OnHandleInput(context);

      IsClicked = false;

      if (!IsLoaded)
        return;

      if (ClickMode == ClickMode.Press)
        HandlePressMode(context);
      else
        HandleReleaseMode(context);
    }


    private void HandlePressMode(InputContext context)
    {
      var inputService = InputService;

      if (!IsDown)
      {
        // Check if button gets pressed down.
        if (IsMouseOver 
            && !inputService.IsMouseOrTouchHandled 
            && inputService.IsPressed(MouseButtons.Left, false))
        {
          inputService.IsMouseOrTouchHandled = true;
          IsDown = true;
          IsClicked = true;
        }

        if (IsFocusWithin 
            && !inputService.IsKeyboardHandled 
            && (inputService.IsPressed(Keys.Enter, false) || inputService.IsPressed(Keys.Space, false)))
        {
          inputService.IsKeyboardHandled = true;
          IsDown = true;
          IsClicked = true;
        }

#if !SILVERLIGHT
        if (IsFocusWithin 
            && !inputService.IsGamePadHandled(context.AllowedPlayer) 
            && inputService.IsPressed(Buttons.A, false, context.AllowedPlayer))
        {
          inputService.SetGamePadHandled(context.AllowedPlayer, true);
          IsDown = true;
          IsClicked = true;
        }
#endif
      }
      else
      {
        if ((!inputService.IsMouseOrTouchHandled && inputService.IsDown(MouseButtons.Left))
            || (!inputService.IsKeyboardHandled && (inputService.IsDown(Keys.Enter) || inputService.IsDown(Keys.Space)))
#if !SILVERLIGHT
            || (!inputService.IsGamePadHandled(context.AllowedPlayer) && inputService.IsDown(Buttons.A, context.AllowedPlayer))
#endif
          )
        {
          // IsDown stays true.
        }
        else
        {
          IsDown = false;
        }

        // Input is still captured for this frame.
        inputService.IsMouseOrTouchHandled = true;
        inputService.IsKeyboardHandled = true;
#if !SILVERLIGHT
        inputService.SetGamePadHandled(context.AllowedPlayer, true);
#endif
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private void HandleReleaseMode(InputContext context)
    {
      var inputService = InputService; 

      if (!IsDown)
      {
        // Check if button gets pressed down.
        if (IsMouseOver 
            && !inputService.IsMouseOrTouchHandled 
            && inputService.IsPressed(MouseButtons.Left, false))
        {
          inputService.IsMouseOrTouchHandled = true;
          IsDown = true;
        }

        if (IsFocusWithin 
            && !inputService.IsKeyboardHandled 
            && (inputService.IsPressed(Keys.Enter, false) || inputService.IsPressed(Keys.Space, false)))
        {
          inputService.IsKeyboardHandled = true;
          IsDown = true;
        }

#if !SILVERLIGHT
        if (IsFocusWithin 
            && !inputService.IsGamePadHandled(context.AllowedPlayer) 
            && inputService.IsPressed(Buttons.A, false, context.AllowedPlayer))
        {
          inputService.SetGamePadHandled(context.AllowedPlayer, true);
          IsDown = true;
        }
#endif
      }
      else
      {
        if ((!inputService.IsMouseOrTouchHandled && inputService.IsDown(MouseButtons.Left))
            || (!inputService.IsKeyboardHandled && (inputService.IsDown(Keys.Enter) || inputService.IsDown(Keys.Space)))
#if !SILVERLIGHT
            || (!inputService.IsGamePadHandled(context.AllowedPlayer) && inputService.IsDown(Buttons.A, context.AllowedPlayer))
#endif
          )
        {
          // IsDown stays true.
        }
        else
        {
          // Released!
          IsDown = false;

          // A click is created only if the release comes from the mouse over the control, or if 
          // the release comes from a button/key when the control is focused.
          if (IsMouseOver && !inputService.IsMouseOrTouchHandled && inputService.IsReleased(MouseButtons.Left)
              || IsFocusWithin && !inputService.IsKeyboardHandled && (inputService.IsReleased(Keys.Enter) || inputService.IsReleased(Keys.Space))
#if !SILVERLIGHT
              || IsFocusWithin && !inputService.IsGamePadHandled(context.AllowedPlayer) && inputService.IsReleased(Buttons.A, context.AllowedPlayer)
#endif
            )
          {
            IsClicked = true;
          }
        }

        // Input is still captured for this frame.
        inputService.IsMouseOrTouchHandled = true;
        inputService.IsKeyboardHandled = true;
#if !SILVERLIGHT
        inputService.SetGamePadHandled(context.AllowedPlayer, true);
#endif
      }
    }


    /// <summary>
    /// Raises the <see cref="Click"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors: </strong>When overriding <see cref="OnClick"/> in a 
    /// derived class, be sure to call the base class's <see cref="OnClick"/> method so that 
    /// registered delegates receive the event.
    /// </remarks>
    protected virtual void OnClick(EventArgs eventArgs)
    {
      var click = Events.Get<EventArgs>(ClickEventId);
      click.Raise();
    }
    #endregion
  }
}
