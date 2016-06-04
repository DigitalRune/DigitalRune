// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game.Input
{
  /// <summary>
  /// Defines a command that is activated by user input.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Input commands translate raw input into (such as button presses, key combinations, etc.) to 
  /// semantic actions (such as "Forward", "Jump", "Shoot", etc.).Input commands are managed by the 
  /// <see cref="InputService"/>. Input commands can be added or removed from the input service by 
  /// adding or removing them from the <see cref="IInputService.Commands"/> collection. The input 
  /// commands are identified by their <see cref="INamedObject.Name"/> in the 
  /// <see cref="IInputService.Commands"/> collection.
  /// </para>
  /// <para>
  /// When the input service is updated it calls the method <see cref="Update"/> of all registered
  /// input commands. In this method the input commands can check the user input and update their 
  /// <see cref="Value"/> accordingly. For example, the input command "Jump" might check whether the
  /// user has pressed the SPACE key and set the value to 1 if the key is down.
  /// </para>
  /// <para>
  /// Other game components can check the command and do not need to know which user input triggered
  /// the action. For example:
  /// <code lang="csharp">
  /// <![CDATA[
  /// if (InputService.Commands["Jump"].Value > 0)
  /// {
  ///   // Let the player character jump.
  ///   ...
  /// }
  /// ]]>
  /// </code>
  /// The input mapping can be reconfigured at runtime without affecting other game components. 
  /// </para>
  /// <para>
  /// Input commands should have a unique <see cref="INamedObject.Name"/>. The name must not be 
  /// changed while the command is added to an <see cref="IInputService"/>. Here are examples for 
  /// command names in a game: "Forward", "Strafe", "Turn", "Jump", "Shoot", etc. Typically, 
  /// command names describe the <i>intention</i> of the player.
  /// </para>
  /// <para>
  /// The <see cref="Value"/> indicates whether the command is active. A value of 0 indicates that 
  /// the action is inactive. A value of 1 typically indicates that the command is active - the user 
  /// wants to perform the action. Depending on the type of action, the value can also be negative.
  /// For example, the "Forward" command might use a value of 1 to indicate that the user wants to
  /// move forward and a value of -1 to indicate that the user wants to move backward. A value of 
  /// 0.5 can indicate that the user wants to move with half speed, which can result from an analog
  /// input device.
  /// </para>
  /// <para>
  /// The property <see cref="InputService"/> is automatically set when the command is added to
  /// an input service. The method <see cref="Update"/> is called in each frame by the 
  /// <see cref="InputService"/> to do any time intensive work. For instance, if the command
  /// smooths input values using a low-pass filter, this is best done in the <see cref="Update"/>
  /// method.
  /// </para>
  /// <para>
  /// This interface does not define how commands use the <strong>IsHandled</strong> flags
  /// (<see cref="IInputService.IsKeyboardHandled"/>, etc.) of the input service. 
  /// </para>
  /// </remarks>
  public interface IInputCommand : INamedObject
  {
    /// <summary>
    /// Gets or sets the input service.
    /// </summary>
    /// <value>The input service.</value>
    /// <remarks>
    /// This property is automatically set when the command is added to an 
    /// <see cref="IInputService"/> (see <see cref="IInputService.Commands"/>). This property is 
    /// <see langword="null"/> if this command is not added to an <see cref="IInputService"/> .
    /// </remarks>
    IInputService InputService { get; set; }


    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    /// <remarks>
    /// This value typically represents the value of an analog input (e.g. a joystick axis) or a
    /// boolean value (where 0 means "false" or "command is not active", and 1 means "true" or
    /// "command is active in this frame"). 
    /// This value is typically polled in each frame by the game modules that react to this command.
    /// </remarks>
    float Value { get; }


    /// <summary>
    /// Updates internal values of this command. This method is called automatically in each frame 
    /// by the input service.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    void Update(TimeSpan deltaTime);
  }
}
