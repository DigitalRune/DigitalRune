// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Input;
#if !SILVERLIGHT
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
#else
using Keys = System.Windows.Input.Key;
#endif
#if USE_DIGITALRUNE_MATHEMATICS
using DigitalRune.Mathematics.Algebra;
#else
using Vector2F = Microsoft.Xna.Framework.Vector2;
using Vector3F = Microsoft.Xna.Framework.Vector3;
#endif


namespace DigitalRune.Game.Input
{
#pragma warning disable 1584,1711,1572,1581,1580,1574       // cref attribute could not be resolved.
  /// <summary>
  /// Manages user input from keyboard, mouse, Xbox 360 controllers and other devices.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Note: Touch, accelerometer and gamepad input is not supported in Silverlight.
  /// (But those devices are supported on the Windows Phone.)
  /// </para>
  /// <para>
  /// The input manager is the central method that should be used to check for user input. It 
  /// contains many convenience methods that allow to detected key/button presses and double-clicks.
  /// </para>
  /// <para>
  /// Typically, many game components can handle the input. But often input should only be processed
  /// by the foremost game component (e.g. the top-most window). For this, game components can set 
  /// the flags <see cref="IsAccelerometerHandled"/>, <see cref="IsGamePadHandled(LogicalPlayerIndex)"/>, 
  /// <see cref="IsKeyboardHandled"/>, and <see cref="IsMouseOrTouchHandled"/> to indicate that 
  /// input has already been processed and other game components should ignore the input. These 
  /// flags are reset by the input service in each frame, but otherwise the input service itself 
  /// does not read this flags. It is up to the game components to decide whether they want to
  /// consider these flags or not. (If, for example, <see cref="IsMouseOrTouchHandled"/> is set,
  /// methods like <see cref="IsDown(MouseButtons)"/> still work normally.)
  /// </para>
  /// <para>
  /// <strong>Logical Players and Game Controllers: </strong>The <see cref="PlayerIndex"/> in XNA
  /// identifies a game controller. Beware that "Player One" may not be using the game controller
  /// that is assigned to <strong>PlayerIndex.One</strong>! A game needs to detect which player uses
  /// which game controller at runtime. (See example below.)
  /// </para>
  /// <para>
  /// The <see cref="LogicalPlayerIndex"/> identifies a player. <see cref="SetLogicalPlayer"/> must
  /// be called to assign a game controller to a player. Gamepad input can be queried using the 
  /// <see cref="PlayerIndex"/> to get the input of a certain game controller or the 
  /// <see cref="LogicalPlayerIndex"/> to get the input of a certain player.
  /// <strong>LogicalPlayerIndex.Any</strong> can be used to query the game controllers of all 
  /// players. Note that game controllers that are not associated with any player are ignored when
  /// <strong>LogicalPlayerIndex.Any</strong> is used.
  /// </para>
  /// <para>
  /// <strong>IMPORTANT: </strong>The methods that take the <see cref="LogicalPlayerIndex"/> as a
  /// parameter return default values when no game controller is assigned to the specified player.
  /// Be sure to call 
  /// <see cref="SetLogicalPlayer"/> to assign game controllers to players.
  /// </para>
  /// <para>
  /// <strong>IsUp, IsDown and IsPressed: </strong>The input service defines simple methods that
  /// allow to check if a key or button is currently held down or not. This methods are called 
  /// <strong>IsDown</strong> and <strong>IsUp</strong>. The methods <strong>IsPressed</strong> and
  /// <strong>IsReleased</strong> check whether a key or button was pressed down or released exactly
  /// in this frame. That means, if a key is not held down, <strong>IsUp</strong> returns true and
  /// all other methods return false. Then when the key is pressed, <strong>IsDown
  /// </strong> is true and <strong>IsPressed</strong> is true. If the key is still held down in the
  /// next frame, <strong>IsDown</strong> is still true but <strong>IsPressed</strong> is false.
  /// </para>
  /// <para>
  /// <strong>Double-Clicks: </strong>The methods <strong>IsDoubleClick</strong> can be used to
  /// detect double-clicks. The two clicks must be within the 
  /// <see cref="InputSettings.DoubleClickTime"/> to count as double-click. For GUI controls it is
  /// also necessary to check if both clicks were in the same region - but this is not checked by
  /// the input service and is left to the GUI system.
  /// </para>
  /// <para>
  /// <strong>Virtual Key/Button Presses: </strong>When a key or button is held down for longer than
  /// <see cref="InputSettings.RepetitionDelay"/> the input service starts to create "IsPressed"
  /// events at a frequency defined by <see cref="InputSettings.RepetitionInterval"/> - this is
  /// convenient for text input in text box controls and text editors. The property 
  /// <see cref="PressedKeys"/> contains a list of all keys that where pressed down in the current
  /// frame - including the virtual presses created by keys/buttons that were held down for a long
  /// time. In the <strong>IsPressed</strong> methods the second parameter allows to specify if
  /// virtual key/button repetitions events should be included or not.
  /// </para>
  /// <para>
  /// <strong>Accelerometer: </strong>The accelerometer can only be used on the Windows Phone 7
  /// device. In the Windows Phone 7 emulator the arrow keys and the space key can be used to
  /// create accelerometer readings.
  /// </para>
  /// </remarks>
  /// <example>
  /// At runtime an application needs to figure out which game controller is used to control the 
  /// game. This is typically done by prompting the user to press Start or button A at the start 
  /// screen. Include the following code in the <strong>Update</strong> method of the game:
  /// <code lang="csharp">
  /// <![CDATA[
  /// if (_inputManager.GetLogicalPlayer(LogicalPlayerIndex.One) == null)
  /// {
  ///   // Wait until the user presses A or START on any connected gamepad.
  ///   for (var controller = PlayerIndex.One; controller <= PlayerIndex.Four; controller++)
  ///   {
  ///     if (_inputManager.IsDown(Buttons.A, controller) || _inputManager.IsDown(Buttons.Start, controller))
  ///     {
  ///       // A or START was pressed. Assign the controller to the first "logical player".
  ///       _inputManager.SetLogicalPlayer(LogicalPlayerIndex.One, controller);
  ///       break;
  ///     }
  ///   }
  /// }
  /// ]]>
  /// </code>
  /// All subsequent methods can use <strong>LogicalPlayerIndex.One</strong> to query the input of 
  /// the player.
  /// </example>
#pragma warning restore 1584,1711,1572,1581,1580,1574
  public interface IInputService
  {
    /// <summary>
    /// Gets or sets the settings that define input handling, timing, etc.
    /// </summary>
    /// <value>The input settings.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    InputSettings Settings { get; set; }


    /// <summary>
    /// Gets the max number of players (= max number of game controllers that can be connected).
    /// </summary>
    /// <value>The max number of players.</value>
    /// <remarks>
    /// This number shows the maximal number of game controllers that can be connected and are 
    /// supported by this input service.
    /// </remarks>
    int MaxNumberOfPlayers { get; }


    /// <summary>
    /// Gets or sets a value indicating whether mouse or touch input has already been handled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if mouse or touch input has already been handled; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This value is automatically reset (= set to <see langword="false"/>) by the input service
    /// in each frame. Game components can set this flag to indicate that they have handled the
    /// mouse or touch input and other game components should not handle this input anymore. 
    /// </remarks>
    bool IsMouseOrTouchHandled { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether keyboard input has already been handled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if keyboard input has already been handled; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This value is automatically reset (= set to <see langword="false"/>) by the input service
    /// in each frame. Game components can set this flag to indicate that they have handled the
    /// keyboard input and other game components should not handle this input anymore. 
    /// </remarks>
    bool IsKeyboardHandled { get; set; }


#if !SILVERLIGHT
    /// <summary>
    /// Gets the game controller assigned to the specified player. (Not available in Silverlight.)
    /// </summary>
    /// <param name="player">
    /// The <see cref="LogicalPlayerIndex"/> that identifies the player.
    /// </param>
    /// <returns>
    /// The <see cref="PlayerIndex"/> that identifies the game controller. Returns 
    /// <see langword="null"/>, if no game controller is assigned to <paramref name="player"/>.
    /// </returns>
    /// <remarks>
    /// Use <see cref="SetLogicalPlayer"/> to assign a game controller to a player.
    /// </remarks>
    /// <seealso cref="SetLogicalPlayer"/>
    PlayerIndex? GetLogicalPlayer(LogicalPlayerIndex player);


    /// <summary>
    /// Assigns a game controller to a player. (Not available in Silverlight.)
    /// </summary>
    /// <param name="player">
    /// The <see cref="LogicalPlayerIndex"/> that identifies the player.
    /// </param>
    /// <param name="controller">
    /// The <see cref="PlayerIndex"/> that identifies the game controller. (Can be 
    /// <see langword="null"/> to remove the current assignment.)
    /// </param>
    /// <seealso cref="GetLogicalPlayer"/>
    /// <exception cref="ArgumentException">
    /// <paramref name="player"/> is invalid.
    /// </exception>
    void SetLogicalPlayer(LogicalPlayerIndex player, PlayerIndex? controller);


    /// <overloads>
    /// <summary>
    /// Sets the <strong>IsGamePadHandled</strong> flags. (Not available in Silverlight.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets the <strong>IsGamePadHandled</strong> flags for the given player. (Not available in Silverlight.)
    /// </summary>
    /// <param name="player">
    /// The <see cref="LogicalPlayerIndex"/> that identifies the player. 
    /// (<see cref="LogicalPlayerIndex.Any"/> to set the <strong>IsGamePadHandled</strong> flag of 
    /// all players.)
    /// </param>
    /// <param name="value">
    /// The new value for the <strong>IsGamePadHandled</strong> flag.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    void SetGamePadHandled(LogicalPlayerIndex player, bool value);


    /// <summary>
    /// Sets the <strong>IsGamePadHandled</strong> flags of a given game controller. 
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="controller">
    /// The <see cref="PlayerIndex"/> that identifies the game controller.
    /// </param>
    /// <param name="value">
    /// The new value for the <strong>IsGamePadHandled</strong> flag.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    void SetGamePadHandled(PlayerIndex controller, bool value);


    /// <overloads>
    /// <summary>
    /// Gets a value indicating whether gamepad input has already been handled. 
    /// (Not available in Silverlight.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets a value indicating whether gamepad input of a given player has already been handled.
    /// </summary>
    /// <param name="player">
    /// The <see cref="LogicalPlayerIndex"/> that identifies the player. 
    /// (<see cref="LogicalPlayerIndex.Any"/> to check all of players.)
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input for the given <paramref name="player"/> was already 
    /// handled. If <paramref name="player"/> is <see langword="LogicalPlayerIndex.Any"/> 
    /// <see langword="true"/> is returned if any game controller input was already handled.
    /// </returns>
    /// <remarks>
    /// This flags are automatically reset (= set to <see langword="false"/>) by the input service
    /// in each frame. Game components can set this flag to indicate that they have handled the
    /// game controller input and other game components should not handle this input anymore.
    /// To set these flags use <see cref="SetGamePadHandled(LogicalPlayerIndex,bool)"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    bool IsGamePadHandled(LogicalPlayerIndex player);


    /// <summary>
    /// Gets a value indicating whether the input of a given game controller has already been 
    /// handled. (Not available in Silverlight.)
    /// </summary>
    /// <param name="controller">
    /// The <see cref="PlayerIndex"/> that identifies the game controller.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input for the given <paramref name="controller"/> was already 
    /// handled.
    /// </returns>
    /// <inheritdoc cref="IsGamePadHandled(LogicalPlayerIndex)"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    bool IsGamePadHandled(PlayerIndex controller);
#endif


    /// <summary>
    /// Sets all "IsHandled" flags to the given value.
    /// </summary>
    /// <param name="value">The value for the flags.</param>
    void SetAllHandled(bool value);

    
    /// <summary>
    /// Gets or sets a value indicating whether accelerometer input has already been handled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if accelerometer input has already been handled; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This value is automatically reset (= set to <see langword="false"/>) by the input service
    /// in each frame. Game components can set this flag to indicate that they have handled the
    /// accelerometer input and other game components should not handle this input anymore. 
    /// </remarks>
    bool IsAccelerometerHandled { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the mouse position is reset in each frame.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the mouse position is reset in each frame; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// If <see cref="EnableMouseCentering"/> is <see langword="true"/>, the input service will
    /// reset the mouse position to <see cref="InputSettings.MouseCenter"/> in each frame. This is 
    /// necessary, for example, for first-person shooters that need only relative mouse input.
    /// </remarks>
    bool EnableMouseCentering { get; set; }


    /// <summary>
    /// Gets the state of the current mouse state.
    /// </summary>
    /// <value>The state of the current mouse state.</value>
    MouseState MouseState { get; }


    /// <summary>
    /// Gets the mouse state of the last frame.
    /// </summary>
    /// <value>The mouse state of the last frame.</value>
    MouseState PreviousMouseState { get; }


    /// <summary>
    /// Gets a value representing the rotation change of the mouse wheel.
    /// </summary>
    /// <value>The rotation change of the mouse wheel.</value>
    float MouseWheelDelta { get; }


    /// <summary>
    /// Gets the raw mouse position.
    /// </summary>
    /// <value>The raw mouse position.</value>
    /// <remarks>
    /// <para>
    /// <see cref="MousePositionRaw"/> is the mouse position relative to the game window - as it 
    /// was read using the XNA <see cref="Mouse"/> class. <see cref="MousePositionDeltaRaw"/> 
    /// is the mouse position change since the last frame. Both properties are read-only.
    /// </para>
    /// <para>
    /// The properties <see cref="MousePosition"/> and <see cref="MousePositionDelta"/> are set to
    /// the same values as <see cref="MousePositionRaw"/> and <see cref="MousePositionDeltaRaw"/> in 
    /// each frame. These properties have a setter and can therefore be modified by other game 
    /// components. <see cref="MousePosition"/> and <see cref="MousePositionDelta"/> store any 
    /// changed values for the rest of the frame. This is useful if the mouse position needs to be 
    /// transformed. For example, the mouse position can be transformed to be relative to a viewport 
    /// within the game window.
    /// </para>
    /// <para>
    /// If <see cref="MousePosition"/> is modified, <see cref="MousePositionDelta"/> should be
    /// modified accordingly.
    /// </para>
    /// </remarks>
    Vector2F MousePositionRaw { get; }


    /// <summary>
    /// Gets the raw mouse position change since the last frame.
    /// </summary>
    /// <value>The raw mouse position change.</value>
    /// <inheritdoc cref="MousePositionRaw"/>
    Vector2F MousePositionDeltaRaw { get; }


    /// <summary>
    /// Gets or sets the mouse position.
    /// </summary>
    /// <value>The mouse position.</value>
    /// <inheritdoc cref="MousePositionRaw"/>
    Vector2F MousePosition { get; set; }


    /// <summary>
    /// Gets or sets the mouse position change since the last frame.
    /// </summary>
    /// <value>The mouse position change.</value>
    /// <inheritdoc cref="MousePositionRaw"/>
    Vector2F MousePositionDelta { get; set; }


    /// <summary>
    /// Gets the state of the current keyboard state.
    /// </summary>
    /// <value>The state of the current keyboard state.</value>
    KeyboardState KeyboardState { get; }


    /// <summary>
    /// Gets the keyboard state of the last frame.
    /// </summary>
    /// <value>The keyboard state of the last frame.</value>
    KeyboardState PreviousKeyboardState { get; }


    /// <summary>
    /// Gets the pressed keys.
    /// </summary>
    /// <value>The pressed keys.</value>
    /// <remarks>
    /// This list includes keys that were "up" in the last frame and are "down" in this frame.
    /// The list also includes artificial key presses generated by the key repetition feature.
    /// </remarks>
    ReadOnlyCollection<Keys> PressedKeys { get; }


    /// <summary>
    /// Gets the pressed modifier keys.
    /// </summary>
    /// <value>The pressed modifier keys.</value>
    /// <remarks>
    /// The special keys ChatPadGreen and ChatPadOrange are ignored and not detected.
    /// </remarks>
    ModifierKeys ModifierKeys { get; }


#if !SILVERLIGHT
    /// <overloads>
    /// <summary>
    /// Gets the state of a game controller. (Not available in Silverlight.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the gamepad state for the given player.
    /// </summary>
    /// <param name="player">
    /// The <see cref="LogicalPlayerIndex"/> that identifies the player. (Note: 
    /// <see cref="LogicalPlayerIndex.Any"/> is not allowed.)
    /// </param>
    /// <returns>The gamepad state of the current frame.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    GamePadState GetGamePadState(LogicalPlayerIndex player);


    /// <summary>
    /// Gets the gamepad state of the given game controller. (Not available in Silverlight.)
    /// </summary>
    /// <param name="controller">
    /// The <see cref="PlayerIndex"/> that identifies the game controller.
    /// </param>
    /// <returns>The gamepad state of the current frame.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    GamePadState GetGamePadState(PlayerIndex controller);


    /// <overloads>
    /// <summary>
    /// Gets the gamepad state of the last frame. (Not available in Silverlight.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the gamepad state of the last frame for the given player. 
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="player">
    /// The <see cref="LogicalPlayerIndex"/> that identifies the player. 
    /// </param>
    /// <returns>
    /// The gamepad state of the last frame.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    GamePadState GetPreviousGamePadState(LogicalPlayerIndex player);


    /// <summary>
    /// Gets the gamepad state of the last frame of the given game controller. 
    /// (Only available in XNA Windows Phone builds.)
    /// </summary>
    /// <param name="controller">
    /// The <see cref="PlayerIndex"/> that identifies the game controller.
    /// </param>
    /// <returns>
    /// The gamepad state of the last frame.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    GamePadState GetPreviousGamePadState(PlayerIndex controller);


#if !MONOGAME
    /// <summary>
    /// Gets a value indicating whether an accelerometer is connected and can be used. 
    /// (Only available in XNA Windows Phone builds.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if an accelerometer is connected and can be used; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This type is only available on the following platforms: XNA Windows Phone.
    /// </remarks>
    bool IsAccelerometerActive { get; }


    /// <summary>
    /// Gets the accelerometer value. 
    /// (Only available in XNA Windows Phone builds.)
    /// </summary>
    /// <value>The accelerometer value.</value>
    /// <remarks>
    /// Use <see cref="IsAccelerometerActive"/> to check if an accelerometer is actually connected.
    /// </remarks>
    /// <remarks>
    /// This type is only available on the following platforms: XNA Windows Phone.
    /// </remarks>
    Vector3F AccelerometerValue { get; }
#endif


    /// <summary>
    /// Gets the touch collection. (Not available in Silverlight.)
    /// </summary>
    /// <value>The touch collection.</value>
    TouchCollection TouchCollection { get; }


    /// <summary>
    /// Gets the detected touch gestures. (Not available in Silverlight.)
    /// </summary>
    /// <value>The detected touch gestures.</value>
    /// <remarks>
    /// <see cref="TouchPanel.EnabledGestures"/> must be set to enable gesture detection.
    /// Per default, no gestures are detected.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    List<GestureSample> Gestures { get; }
#endif


    /// <summary>
    /// Gets the input commands.
    /// </summary>
    /// <value>The input commands.</value>
    InputCommandCollection Commands { get; }


#if !SILVERLIGHT
    /// <overloads>
    /// <summary>
    /// Determines whether the specified button or key is down. (Not available in Silverlight.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified button is down for the given player. 
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="player">
    /// The <see cref="LogicalPlayerIndex"/> that identifies the player. 
    /// (<see cref="LogicalPlayerIndex.Any"/> to check the game controllers of all available 
    /// players.)
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified button is down; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsDown(Buttons button, LogicalPlayerIndex player);


    /// <summary>
    /// Determines whether the specified button is down on the given game controller. 
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="controller">
    /// The <see cref="PlayerIndex"/> that identifies the game controller.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified button is down; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsDown(Buttons button, PlayerIndex controller);


    /// <overloads>
    /// <summary>
    /// Determines whether the specified button or key is up. (Not available in Silverlight.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified button is up for the given player. 
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="player">
    /// The <see cref="LogicalPlayerIndex"/> that identifies the player. 
    /// (<see cref="LogicalPlayerIndex.Any"/> to check the game controllers of all available 
    /// players.)
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified button is up; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsUp(Buttons button, LogicalPlayerIndex player);


    /// <summary>
    /// Determines whether the specified button is up on the given game controller. 
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="controller">
    /// The <see cref="PlayerIndex"/> that identifies the game controller.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified button is up; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsUp(Buttons button, PlayerIndex controller);


    /// <overloads>
    /// <summary>
    /// Determines whether the specified button or key has been pressed. 
    /// (Not available in Silverlight.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified button has been pressed by the given player. 
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="useButtonRepetition">
    /// If set to <see langword="true"/> physical and virtual button presses (see 
    /// <see cref="IInputService"/>) are returned; otherwise, only physical button presses are
    /// returned.
    /// </param>
    /// <param name="player">
    /// The <see cref="LogicalPlayerIndex"/> that identifies the player. 
    /// (<see cref="LogicalPlayerIndex.Any"/> to check the game controllers of all available 
    /// players.)
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified button was previously up and has been pressed; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsPressed(Buttons button, bool useButtonRepetition, LogicalPlayerIndex player);


    /// <summary>
    /// Determines whether the specified button has been pressed on the given game controller. 
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="useButtonRepetition">
    /// If set to <see langword="true"/> physical and virtual button presses (see 
    /// <see cref="IInputService"/>) are returned; otherwise, only physical button presses are
    /// returned.
    /// </param>
    /// <param name="controller">
    /// The <see cref="PlayerIndex"/> that identifies the game controller.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified button was previously up and has been pressed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsPressed(Buttons button, bool useButtonRepetition, PlayerIndex controller);


    /// <overloads>
    /// <summary>
    /// Determines whether the specified button or key has been released. 
    /// (Not available in Silverlight.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary> 
    /// Determines whether the specified button has been released by the given player.
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="player">
    /// The <see cref="LogicalPlayerIndex"/> that identifies the player. 
    /// (<see cref="LogicalPlayerIndex.Any"/> to check the game controllers of all available 
    /// players.)
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified button was previously down and has been released; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsReleased(Buttons button, LogicalPlayerIndex player);


    /// <summary>
    /// Determines whether the specified button has been released on the given game controller.
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="controller">
    /// The <see cref="PlayerIndex"/> that identifies the game controller.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified button was previously down and has been released;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsReleased(Buttons button, PlayerIndex controller);


    /// <overloads>
    /// <summary>
    /// Determines whether the specified button or key has been double-clicked.
    /// (Not available in Silverlight.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified button has been double-clicked by the given player.
    /// (Not available in Silverlight.)
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="player">
    /// The <see cref="LogicalPlayerIndex"/> that identifies the player. 
    /// (<see cref="LogicalPlayerIndex.Any"/> to check the game controllers of all available 
    /// players.)
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified button has been double-clicked; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    bool IsDoubleClick(Buttons button, LogicalPlayerIndex player);


    /// <summary>
    /// Determines whether the specified button has been double-clicked on the given game 
    /// controller. (Not available in Silverlight.)
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="controller">
    /// The <see cref="PlayerIndex"/> that identifies the game controller.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified button has been double-clicked; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    bool IsDoubleClick(Buttons button, PlayerIndex controller);
#endif


    /// <summary>
    /// Determines whether the specified key is down.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if the specified key is down; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsDown(Keys key);


    /// <summary>
    /// Determines whether the specified key is up.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if the specified key is up; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsUp(Keys key);


    /// <summary>
    /// Determines whether the specified key was previously up and has been pressed.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="useKeyRepetition">
    /// If set to <see langword="true"/> physical and virtual key presses (see 
    /// <see cref="IInputService"/>) are returned; otherwise, only physical key presses are 
    /// returned.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="key"/> was previously up and has been pressed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsPressed(Keys key, bool useKeyRepetition);
    

    /// <summary>
    /// Determines whether the specified key was previously down and has been released.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if the specified key was previously down and has been released;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsReleased(Keys key);


    /// <summary>
    /// Determines whether the specified key was double-clicked.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if the specified key was double-clicked; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    bool IsDoubleClick(Keys key);


    /// <summary>
    /// Determines whether the specified button is down.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <returns>
    /// <see langword="true"/> if the specified button is down; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsDown(MouseButtons button);


    /// <summary>
    /// Determines whether the specified button is up.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <returns>
    /// <see langword="true"/> if the specified button is up; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsUp(MouseButtons button);


    /// <summary>
    /// Determines whether the specified button was previously up and has been pressed.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="useButtonRepetition">
    /// If set to <see langword="true"/> physical and virtual button presses (see 
    /// <see cref="IInputService"/>) are returned; otherwise, only physical button presses are
    /// returned.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified button was previously up and has been pressed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsPressed(MouseButtons button, bool useButtonRepetition);


    /// <summary>
    /// Determines whether the specified button was previously down and has been released.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <returns>
    /// <see langword="true"/> if the specified button was previously down and has been released;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsReleased(MouseButtons button);


    /// <summary>
    /// Determines whether the specified button has been double-clicked.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <returns>
    /// <see langword="true"/> if the specified button has been double-clicked; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    bool IsDoubleClick(MouseButtons button);
  }
}