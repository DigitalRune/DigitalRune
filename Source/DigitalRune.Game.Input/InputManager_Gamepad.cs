// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !SILVERLIGHT
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace DigitalRune.Game.Input
{
  partial class InputManager
  {
    // Note: 
    //   - IsDown(), IsPressed(), IsReleased() and IsDoubleClick() return false if the 
    //     LogicalPlayerIndex is not yet assigned to a game controller.
    //   - IsUp() returns true if LogicalPlayerIndex is not assigned.


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    // Constants to identify certain groups of buttons.
    private const Buttons LeftThumbstickButtons = Buttons.LeftThumbstickDown
                                                  | Buttons.LeftThumbstickLeft
                                                  | Buttons.LeftThumbstickRight
                                                  | Buttons.LeftThumbstickUp;
    private const Buttons RightThumbstickButtons = Buttons.RightThumbstickDown
                                                   | Buttons.RightThumbstickLeft
                                                   | Buttons.RightThumbstickRight
                                                   | Buttons.RightThumbstickUp;
    private const Buttons ThumbstickButtons = LeftThumbstickButtons | RightThumbstickButtons;
    private const Buttons Triggers = Buttons.LeftTrigger | Buttons.RightTrigger;
    private const Buttons AnalogButtons = ThumbstickButtons | Triggers;

    // Default gamepad state used if no game controller is assigned to a player.
    private static readonly GamePadState DefaultGamePadState = new GamePadState();
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly GamePadState[] _newGamePadStates;
    private readonly GamePadState[] _previousGamePadStates;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public PlayerIndex? GetLogicalPlayer(LogicalPlayerIndex player)
    {
      // For efficiency: No boundary checks - array will throw exception anyways.
      int index = (int)player;
      return _logicalPlayers[index];
    }


    /// <inheritdoc/>
    public void SetLogicalPlayer(LogicalPlayerIndex player, PlayerIndex? controller)
    {
      // For efficiency: No boundary checks - array will throw exception anyways.
      int index = (int)player;
      _logicalPlayers[index] = controller;
    }


    private void UpdateGamePads(TimeSpan deltaTime)
    {
      // Same as for mouse and keyboard, except for all gamepads.
      for (int player = 0; player < MaxNumberOfPlayers; player++)
      {
        PlayerIndex playerIndex = (PlayerIndex)player;

        // ----- Update gamepad states.
        _previousGamePadStates[player] = _newGamePadStates[player];
        _newGamePadStates[player] = GamePad.GetState(playerIndex, Settings.GamePadDeadZone);

        var newGamePadState = _newGamePadStates[player];
        var previousGamePadState = _previousGamePadStates[player];
        var lastGamePadButton = _lastGamePadButtons[player];

        var isConnected = newGamePadState.IsConnected;
        if (GlobalSettings.PlatformID == PlatformID.WindowsPhone7
            || GlobalSettings.PlatformID == PlatformID.WindowsPhone8)
        {
          // In WP7 the first gamepad is never connected but it is used for the Back button.
          // In MonoGame/WP8 the first gamepad is connected when the back button is down.
          // To detect Back double-clicks, we treat the first gamepad as connected.
          if (playerIndex == PlayerIndex.One)
            isConnected = true;
        }

        // ---- Reset state and skip rest of loop if this gamepad is not connected.
        if (!isConnected)
        {
          lastGamePadButton.IsDoubleClick = false;
          lastGamePadButton.DownDuration = TimeSpan.Zero;
          lastGamePadButton.TimeSinceLastClick = TimeSpan.MaxValue;
          continue;
        }

        // ----- Find pressed button.
        Buttons? pressedButton = null;
        foreach (Buttons button in _gamePadButtons)
        {
          if (IsDown(ref newGamePadState, button) && !IsDown(ref previousGamePadState, button))
          {
            // A new button press.
            pressedButton = button;
            break;
          }
        }

        // ----- Handle key double clicks and key repetition.
        lastGamePadButton.IsDoubleClick = false;
        lastGamePadButton.IsVirtualPress = false;
        if (!pressedButton.HasValue)
        {
          // No gamepad button pressed.
          // Increase or reset down duration.
          if (IsDown(lastGamePadButton.Button, playerIndex))
          {
            // Previously pressed gamepad button is still down.
            // Increase down duration.
            lastGamePadButton.DownDuration += deltaTime;

            // If the start interval is exceeded, we generate a virtual button press.
            if (lastGamePadButton.DownDuration >= Settings.RepetitionDelay)
            {
              // Generate virtual button press.
              lastGamePadButton.IsVirtualPress = true;

              // Subtract repetition interval from down duration. This way the repetition interval
              // must pass until the if condition is true again.
              lastGamePadButton.DownDuration -= Settings.RepetitionInterval;
            }
          }
          else
          {
            // Reset down duration.
            lastGamePadButton.DownDuration = TimeSpan.Zero;
          }

          // Measure time between clicks.
          if (lastGamePadButton.TimeSinceLastClick != TimeSpan.MaxValue)
            lastGamePadButton.TimeSinceLastClick += deltaTime;
        }
        else
        {
          // A key was pressed.
          // Check for double-click.
          if (pressedButton.Value == lastGamePadButton.Button
              && lastGamePadButton.TimeSinceLastClick < Settings.DoubleClickTime - deltaTime)
          {
            // Double-click detected.
            lastGamePadButton.IsDoubleClick = true;

            // The current click cannot be used for another double-click.
            lastGamePadButton.TimeSinceLastClick = TimeSpan.MaxValue;
          }
          else
          {
            // Wrong button pressed or button pressed too late.
            // Restart double-click logic.
            lastGamePadButton.TimeSinceLastClick = TimeSpan.Zero;
          }

          lastGamePadButton.Button = pressedButton.Value;
          lastGamePadButton.DownDuration = TimeSpan.Zero;
        }
      }
    }


    /// <inheritdoc/>
    public GamePadState GetGamePadState(LogicalPlayerIndex player)
    {
      PlayerIndex? controller = _logicalPlayers[(int)player];
      if (controller.HasValue)
        return _newGamePadStates[(int)controller.Value];

      return DefaultGamePadState;
    }


    /// <inheritdoc/>
    public GamePadState GetGamePadState(PlayerIndex controller)
    {
      int index = (int)controller;
      return _newGamePadStates[index];
    }


    /// <inheritdoc/>
    public GamePadState GetPreviousGamePadState(LogicalPlayerIndex player)
    {
      PlayerIndex? controller = _logicalPlayers[(int)player];
      if (controller.HasValue)
        return _previousGamePadStates[(int)controller.Value];

      return DefaultGamePadState;
    }


    /// <inheritdoc/>
    public GamePadState GetPreviousGamePadState(PlayerIndex controller)
    {
      int index = (int)controller;
      return _previousGamePadStates[index];
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private bool IsDown(ref GamePadState state, Buttons button)
    {
      // Important: button can contain several button flags.
      // All buttons must be pressed to return true.

      if ((button & AnalogButtons) != 0)
      {
        // For triggers and thumbsticks we apply our custom thresholds!
        if ((button & ThumbstickButtons) != 0)
        {
          var thumbstickThreshold = Settings.ThumbstickThreshold;
          if ((button & LeftThumbstickButtons) != 0)
          {
            if ((button & Buttons.LeftThumbstickLeft) != 0)
            {
              // Abort if this button is not pressed (below threshold).
              if (state.ThumbSticks.Left.X > -thumbstickThreshold)
                return false;

              // button might contain more flags. Remove the flag that was already handled.
              button = button & ~Buttons.LeftThumbstickLeft;
            }

            if ((button & Buttons.LeftThumbstickRight) != 0)
            {
              if (state.ThumbSticks.Left.X < thumbstickThreshold)
                return false;

              button = button & ~Buttons.LeftThumbstickRight;
            }

            if ((button & Buttons.LeftThumbstickUp) != 0)
            {
              if (state.ThumbSticks.Left.Y < thumbstickThreshold)
                return false;

              button = button & ~Buttons.LeftThumbstickUp;
            }

            if ((button & Buttons.LeftThumbstickDown) != 0)
            {
              if (state.ThumbSticks.Left.Y > -thumbstickThreshold)
                return false;

              button = button & ~Buttons.LeftThumbstickDown;
            }
          }

          if ((button & RightThumbstickButtons) != 0)
          {
            if ((button & Buttons.RightThumbstickLeft) != 0)
            {
              if (state.ThumbSticks.Right.X > -thumbstickThreshold)
                return false;

              button = button & ~Buttons.RightThumbstickLeft;
            }

            if ((button & Buttons.RightThumbstickRight) != 0)
            {
              if (state.ThumbSticks.Right.X < thumbstickThreshold)
                return false;

              button = button & ~Buttons.RightThumbstickRight;
            }

            if ((button & Buttons.RightThumbstickUp) != 0)
            {
              if (state.ThumbSticks.Right.Y < thumbstickThreshold)
                return false;

              button = button & ~Buttons.RightThumbstickUp;
            }

            if ((button & Buttons.RightThumbstickDown) != 0)
            {
              if (state.ThumbSticks.Right.Y > -thumbstickThreshold)
                return false;

              button = button & ~Buttons.RightThumbstickDown;
            }
          }
        }

        if ((button & Triggers) != 0)
        {
          var triggerThreshold = Settings.TriggerThreshold;
          if ((button & Buttons.LeftTrigger) != 0)
          {
            if (state.Triggers.Left < triggerThreshold)
              return false;

            button = button & ~Buttons.LeftTrigger;
          }

          if ((button & Buttons.RightTrigger) != 0)
          {
            if (state.Triggers.Right < triggerThreshold)
              return false;

            button = button & ~Buttons.RightTrigger;
          }
        }
      }

      if ((int)button == 0)
      {
        // Buttons were handled in the check above.
        // All required buttons are down.
        return true;
      }

      return state.IsButtonDown(button);
    }


    /// <inheritdoc/>
    public bool IsDown(Buttons button, LogicalPlayerIndex player)
    {
      if (player == LogicalPlayerIndex.Any)
      {
        // Check game controllers of all players.
        foreach (PlayerIndex? controller in _logicalPlayers)
          if (controller.HasValue && IsDown(ref _newGamePadStates[(int)controller.Value], button))
            return true;

        return false;
      }
      else
      {
        PlayerIndex? controller = _logicalPlayers[(int)player];
        return controller.HasValue && IsDown(ref _newGamePadStates[(int)controller.Value], button);
      }
    }


    /// <inheritdoc/>
    public bool IsDown(Buttons button, PlayerIndex controller)
    {
      int index = (int)controller;
      return IsDown(ref _newGamePadStates[index], button);
    }


    /// <inheritdoc/>
    public bool IsUp(Buttons button, LogicalPlayerIndex player)
    {
      if (player == LogicalPlayerIndex.Any)
      {
        // Check game controllers of all players.
        bool isUp = true;
        foreach (PlayerIndex? controller in _logicalPlayers)
        {
          if (controller.HasValue)
          {
            if (!IsDown(ref _newGamePadStates[(int)controller.Value], button))
              return true;
            
            isUp = false;
          }
        }

        return isUp;
      }
      else
      {
        PlayerIndex? controller = _logicalPlayers[(int)player];
        if (controller.HasValue)
          return !IsDown(ref _newGamePadStates[(int)controller.Value], button);

        return true;
      }
    }


    /// <inheritdoc/>
    public bool IsUp(Buttons button, PlayerIndex controller)
    {
      int index = (int)controller;
      return !IsDown(ref _newGamePadStates[index], button);
    }


    /// <inheritdoc/>
    public bool IsPressed(Buttons button, bool useButtonRepetition, LogicalPlayerIndex player)
    {
      if (player == LogicalPlayerIndex.Any)
      {
        // Check game controllers of all players.
        foreach (PlayerIndex? controller in _logicalPlayers)
          if (controller.HasValue && IsPressed(button, useButtonRepetition, controller.Value))
            return true;

        return false;
      }
      else
      {
        PlayerIndex? controller = _logicalPlayers[(int)player];
        return controller.HasValue && IsPressed(button, useButtonRepetition, controller.Value);
      }
    }


    /// <inheritdoc/>
    public bool IsPressed(Buttons button, bool useButtonRepetition, PlayerIndex controller)
    {
      int index = (int)controller;
      if (useButtonRepetition)
      {
        var lastGamePadButton = _lastGamePadButtons[index];
        if (lastGamePadButton.Button == button && lastGamePadButton.IsVirtualPress)
        {
          return true;
        }
      }

      return IsDown(ref _newGamePadStates[index], button) && !IsDown(ref _previousGamePadStates[index], button);
    }


    /// <inheritdoc/>
    public bool IsReleased(Buttons button, LogicalPlayerIndex player)
    {
      if (player == LogicalPlayerIndex.Any)
      {
        // Check game controllers of all players.
        foreach (PlayerIndex? controller in _logicalPlayers)
          if (controller.HasValue && IsReleased(button, controller.Value))
            return true;

        return false;
      }
      else
      {
        PlayerIndex? controller = _logicalPlayers[(int)player];
        return controller.HasValue && IsReleased(button, controller.Value);
      }
    }


    /// <inheritdoc/>
    public bool IsReleased(Buttons button, PlayerIndex controller)
    {
      int index = (int)controller;
      return !IsDown(ref _newGamePadStates[index], button) && IsDown(ref _previousGamePadStates[index], button);
    }


    /// <inheritdoc/>
    public bool IsDoubleClick(Buttons button, LogicalPlayerIndex player)
    {
      if (player == LogicalPlayerIndex.Any)
      {
        // Check game controllers of all players.
        foreach (PlayerIndex? controller in _logicalPlayers)
          if (controller.HasValue && IsDoubleClick(button, controller.Value))
            return true;

        return false;
      }
      else
      {
        PlayerIndex? controller = _logicalPlayers[(int)player];
        return controller.HasValue && IsDoubleClick(button, controller.Value);
      }
    }


    /// <inheritdoc/>
    public bool IsDoubleClick(Buttons button, PlayerIndex controller)
    {
      int index = (int)controller;
      var lastGamePadButton = _lastGamePadButtons[index];
      return lastGamePadButton.Button == button && lastGamePadButton.IsDoubleClick;
    }
    #endregion
  }
}
#endif
