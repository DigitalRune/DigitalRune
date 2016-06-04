// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Base class for controls that can switch states, such as <see cref="CheckBox"/> or 
  /// <see cref="RadioButton"/>.
  /// </summary>
  /// <remarks>
  /// <strong>Visual States:</strong> The <see cref="VisualState"/>s of this control are:
  /// "Checked-Disabled", "Unchecked-Disabled", "Checked-Default", "Unchecked-Default", 
  /// "Checked-MouseOver", "Unchecked-MouseOver", "Checked-Focused", "Unchecked-Focused", 
  /// "Checked-Pressed", "Unchecked-Pressed"
  /// </remarks>
  public abstract class ToggleButton : ButtonBase
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
          return IsChecked ? "Checked-Disabled" : "Unchecked-Disabled";

        if (IsDown)
          return IsChecked ? "Checked-Pressed" : "Unchecked-Pressed";

        if (IsMouseOver)
          return IsChecked ? "Checked-MouseOver" : "Unchecked-MouseOver";

        if (IsFocused)
          return IsChecked ? "Checked-Focused" : "Unchecked-Focused";

        return IsChecked ? "Checked-Default" : "Unchecked-Default";
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="IsChecked"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsCheckedPropertyId = CreateProperty(
      typeof(ToggleButton), "IsChecked", GamePropertyCategories.Behavior, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets a value indicating whether this toggle button is checked. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this toggle button is checked; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsChecked
    {
      get { return GetValue<bool>(IsCheckedPropertyId); }
      set { SetValue(IsCheckedPropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleButton"/> class.
    /// </summary>
    protected ToggleButton()
    {
      Style = "ToggleButton";
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnClick(EventArgs eventArgs)
    {
      OnToggle();

      base.OnClick(eventArgs);
    }


    /// <summary>
    /// Called when the toggle button is clicked.
    /// </summary>
    /// <remarks>
    /// This method must be implemented by derived classes and is responsible for changing the
    /// <see cref="IsChecked"/> property.
    /// </remarks>
    protected abstract void OnToggle();
    #endregion
  }
}
