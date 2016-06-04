// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Input;
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif


namespace DigitalRune.Game.Input
{
#pragma warning disable 1584,1711,1572,1581,1580,1574       // cref attribute could not be resolved.
  /// <summary>
  /// Defines a combination of keys, buttons and more that can be used to trigger an action.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The user can trigger an action by pressing keyboard keys, gamepad buttons or mouse buttons.
  /// The user can also use analog input (see <see cref="Axis"/>). The action is triggered if any of
  /// the given keys/buttons are pressed. 
  /// </para>
  /// <para>
  /// The mapping defines "positive" and "negative" keys/buttons. For example, for a "Shoot" 
  /// command, only the "positive" button is relevant. For a "Move Horizontal" command, the positive
  /// button can be used to move left, while the negative buttons can be used to move right. If
  /// mapped to a numeric value, the mapping creates -1 if the negative button is pressed and +1 if
  /// the positive button is pressed. If no button is pressed, or both positive and negative buttons
  /// are pressed concurrently, the value is 0.
  /// </para>
  /// <para>
  /// <strong>Modifiers:</strong> <see cref="ModifierKeys"/> and <see cref="ModifierButtons"/> can 
  /// also be specified. The action is only triggered if the modifiers are down while the 
  /// keys/buttons are pressed. <see cref="ModifierKeys"/> are only relevant for keyboard and mouse 
  /// input. <see cref="ModifierButtons"/> are only relevant for gamepad input.
  /// </para>
  /// </remarks>
#pragma warning restore 1584,1711,1572,1581,1580,1574
  public class InputMapping
  {
    /// <summary>
    /// Gets or sets the description that describes the result of the "positive" buttons,
    /// e.g. "Move Right".
    /// </summary>
    /// <value>
    /// The description of the positive keys and buttons.
    /// The default value is <see langword="null"/>.
    /// </value>
    public string PositiveDescription { get; set; }


    /// <summary>
    /// Gets or sets the description that describes the result of the "negative" buttons,
    /// e.g. "Move Left".
    /// </summary>
    /// <value>
    /// The description of the negative keys and buttons.
    /// The default value is <see langword="null"/>.
    /// </value>
    public string NegativeDescription { get; set; }


    /// <summary>
    /// Gets or sets the modifier keys that must be down. Keys and mouse buttons do not trigger
    /// any actions if the modifier keys are released.
    /// </summary>
    /// <value>The modifier keys.</value>
    public ModifierKeys ModifierKeys { get; set; }


#if !SILVERLIGHT
    /// <summary>
    /// Gets or sets the modifier buttons that must be down. Gamepad buttons do not trigger any 
    /// actions if the modifier buttons are released. (Not available in Silverlight.)
    /// </summary>
    /// <value>The modifier buttons.</value>
    public Buttons? ModifierButtons { get; set; }
#endif


    /// <summary>
    /// Gets or sets the type of the key or button press that triggers the action.
    /// </summary>
    /// <value>The type of the key or button press.</value>
    public PressType PressType { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="Axis"/> and the positive and negative
    /// keys/buttons are inverted.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the <see cref="Axis"/> and the positive and negative keys/buttons
    /// are inverted; otherwise, <see langword="false"/>.
    /// </value>
    public bool Invert { get; set; }


    /// <summary>
    /// Gets or sets the key that triggers a positive action.
    /// </summary>
    /// <value>The key that triggers a positive action.</value>
    public Keys? PositiveKey { get; set; }


    /// <summary>
    /// Gets or sets the key that triggers a negative action.
    /// </summary>
    /// <value>The key that triggers a negative action.</value>
    public Keys? NegativeKey { get; set; }


#if !SILVERLIGHT
    /// <summary>
    /// Gets or sets the gamepad button that triggers a positive action.
    /// (Not available in Silverlight.)
    /// </summary>
    /// <value>The gamepad button that triggers a positive action.</value>
    public Buttons? PositiveButton { get; set; }


    /// <summary>
    /// Gets or sets the gamepad button that triggers a negative action.
    /// (Not available in Silverlight.)
    /// </summary>
    /// <value>The gamepad button that triggers a negative action.</value>
    public Buttons? NegativeButton { get; set; }
#endif


    /// <summary>
    /// Gets or sets the mouse button that triggers a positive action.
    /// </summary>
    /// <value>The mouse button that triggers a positive action.</value>
    public MouseButtons? PositiveMouseButton { get; set; }


    /// <summary>
    /// Gets or sets the mouse button that triggers a negative action.
    /// </summary>
    /// <value>The mouse button that triggers a negative action.</value>
    public MouseButtons? NegativeMouseButton { get; set; }


    /// <summary>
    /// Gets or sets the analog device input that controls the action.
    /// </summary>
    /// <value>The analog device input that controls the action.</value>
    public DeviceAxis? Axis { get; set; }
  }
}
