// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using DigitalRune.Game.Input;
using Microsoft.Xna.Framework.Input;
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents a button control. 
  /// </summary>
  /// <example>
  /// The following examples shows how to create a button and handle the 
  /// <see cref="ButtonBase.Click"/> event.
  /// <code lang="csharp">
  /// <![CDATA[
  /// var button = new Button
  /// {
  ///   Content = new TextBlock { Text = "Click Me!" },
  ///   Margin = new Vector4F(4),
  ///   Padding = new Vector4F(6),
  ///   HorizontalAlignment = HorizontalAlignment.Stretch,
  /// };
  /// 
  /// // To show the button, add it to an existing content control or panel.
  /// panel.Children.Add(button);
  /// 
  /// // To handle button clicks, simply add an event handler to the Click event.
  /// button.Click += OnButtonClicked;
  /// ]]>
  /// </code>
  /// </example>
  public class Button : ButtonBase
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="IsCancel"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsCancelPropertyId = CreateProperty(
      typeof(Button), "IsCancel", GamePropertyCategories.Behavior, null, false, 
      UIPropertyOptions.None);
    
    /// <summary>
    /// Gets or sets a value that indicates whether a <see cref="Button"/> is a Cancel button. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this <see cref="Button"/> is a Cancel button; otherwise, 
    /// <see langword="false"/>. The default is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// A user can activate the Cancel button by pressing the ESC key on the keyboard or the B or
    /// BACK button on the gamepad. 
    /// </remarks>
    public bool IsCancel
    {
      get { return GetValue<bool>(IsCancelPropertyId); }
      set { SetValue(IsCancelPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IsDefault"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsDefaultPropertyId = CreateProperty(
      typeof(Button), "IsDefault", GamePropertyCategories.Behavior, null, false, 
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets a value that indicates whether a <see cref="Button"/> is the default button. 
    /// This is a game object property. 
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this <see cref="Button"/> is the default button; otherwise, 
    /// <see langword="false"/>. The default is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// A user can activate the default button by pressing the ENTER or SPACE key on the keyboard
    /// or the A or START button on the gamepad. 
    /// </remarks>
    public bool IsDefault
    {
      get { return GetValue<bool>(IsDefaultPropertyId); }
      set { SetValue(IsDefaultPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IsRepeatButton"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsRepeatButtonPropertyId = CreateProperty(
      typeof(Button), "IsRepeatButton", GamePropertyCategories.Behavior, null, false, 
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets a value indicating whether this instance is repeat button. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is repeat button; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// A repeat button raises the <see cref="ButtonBase.Click"/> event repeatedly from the time it
    /// is pressed until it is released. Repeat buttons automatically set the 
    /// <see cref="ButtonBase.ClickMode"/> to <see cref="ClickMode.Press"/>.
    /// </remarks>
    public bool IsRepeatButton
    {
      get { return GetValue<bool>(IsRepeatButtonPropertyId); }
      set { SetValue(IsRepeatButtonPropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// </summary>
    public Button()
    {
      Style = "Button";

      // Handle IsRepeatButton changes:
      // If the button is turned into a repeat button, the mode must change to Press.
      var isRepeatButtonProperty = Properties.Get<bool>(IsRepeatButtonPropertyId);
      isRepeatButtonProperty.Changed += (s, e) =>
                                        {
                                          if (IsRepeatButton)
                                            ClickMode = ClickMode.Press;
                                        };
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    protected override void OnHandleInput(InputContext context)
    {
      base.OnHandleInput(context);

      if (IsClicked)
      {
        // Button was clicked with mouse. No need for more checks.
        return;
      }

      if (!IsLoaded)
        return;

      var inputService = InputService;

      // Repeat button behavior and mouse.
      if (IsRepeatButton)
      {
        if (IsMouseOver
            && IsDown
            && inputService.IsPressed(MouseButtons.Left, true))
        {
          inputService.IsMouseOrTouchHandled = true;
          IsClicked = true;
        }

        // Repeat button behavior and keyboard.
        if (IsFocusWithin)
        {
          if (!inputService.IsKeyboardHandled
              && (inputService.IsPressed(Keys.Enter, true) || inputService.IsPressed(Keys.Space, true)))
          {
            inputService.IsKeyboardHandled = true;
            IsClicked = true;
          }

#if !SILVERLIGHT
          // Repeat button behavior and gamepad.
          if (!inputService.IsGamePadHandled(context.AllowedPlayer)
              && inputService.IsPressed(Buttons.A, true, context.AllowedPlayer))
          {
            inputService.SetGamePadHandled(context.AllowedPlayer, true);
            IsClicked = true;
          }
#endif
        }
      }

      bool isDefault = IsDefault;
      bool isCancel = IsCancel;
      if (isDefault || isCancel)
      {
        // Handling IsDefault, IsCancel and keyboard.
        if (!inputService.IsKeyboardHandled)
        {
          if ((isDefault && inputService.IsPressed(Keys.Enter, false))
              || (isDefault && inputService.IsPressed(Keys.Space, false))
              || (isCancel && inputService.IsPressed(Keys.Escape, false)))
          {
            inputService.IsKeyboardHandled = true;
            IsClicked = true;
          }
        }

#if !SILVERLIGHT
        // Handling IsDefault, IsCancel and gamepad.
        if (!inputService.IsGamePadHandled(context.AllowedPlayer))
        {
          if ((isDefault && inputService.IsPressed(Buttons.A, false, context.AllowedPlayer))
              || (isDefault && inputService.IsPressed(Buttons.Start, false, context.AllowedPlayer))
              || (isCancel && inputService.IsPressed(Buttons.B, false, context.AllowedPlayer))
              || (isCancel && inputService.IsPressed(Buttons.Back, false, context.AllowedPlayer)))
          {
            inputService.SetGamePadHandled(context.AllowedPlayer, true);
            IsClicked = true;
          }
        }
#endif
      }
    }
    #endregion
  }
}
