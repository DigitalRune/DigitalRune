// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Input;

#if USE_DIGITALRUNE_MATHEMATICS
using DigitalRune.Mathematics.Algebra;
#else
using Vector2F = Microsoft.Xna.Framework.Vector2;
using Vector3F = Microsoft.Xna.Framework.Vector3;
#endif


namespace DigitalRune.Game.Input
{
  partial class InputManager
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public bool EnableMouseCentering
    {
      get { return _enableMouseCentering; }
      set
      {
        if (_enableMouseCentering == value)
          return;

        _enableMouseCentering = value;

#if MONOGAME
        Mouse.IsRelative = value;
#endif
        
        if (_enableMouseCentering)
        {
          if (GlobalSettings.PlatformID != PlatformID.WindowsPhone7 && GlobalSettings.PlatformID != PlatformID.WindowsPhone8 && GlobalSettings.PlatformID != PlatformID.Android && GlobalSettings.PlatformID != PlatformID.iOS)
          { 
            // Immediately reset mouse position, so that mouse delta is zero in the next frame.
            // Otherwise, the 3D camera would make a jump in the next frame.
            var mouseCenter = Settings.MouseCenter;
            Mouse.SetPosition((int)mouseCenter.X, (int)mouseCenter.Y);
            _newMouseState = Mouse.GetState();
            MousePosition = MousePositionRaw;
            MousePositionDelta = MousePositionDeltaRaw;
          }
        }
      }
    }
    private bool _enableMouseCentering;


    /// <inheritdoc/>
    public MouseState MouseState
    {
      get { return _newMouseState; }
    }
    private MouseState _newMouseState;


    /// <inheritdoc/>
    public MouseState PreviousMouseState
    {
      get { return _previousMouseState; }
    }
    private MouseState _previousMouseState;


    /// <inheritdoc/>
    public Vector2F MousePositionRaw
    {
      get
      {
        return new Vector2F(_newMouseState.X, _newMouseState.Y);
      }
    }


    /// <inheritdoc/>
    public Vector2F MousePositionDeltaRaw
    {
      get
      {
        if (!EnableMouseCentering)
          return new Vector2F(_newMouseState.X - _previousMouseState.X, _newMouseState.Y - _previousMouseState.Y);

#if MONOGAME
        return new Vector2F(_newMouseState.DeltaX, _newMouseState.DeltaY);
#else
        return new Vector2F(_newMouseState.X - Settings.MouseCenter.X, _newMouseState.Y - Settings.MouseCenter.Y);
#endif
      }
    }


    /// <inheritdoc/>
    public Vector2F MousePosition { get; set; }


    /// <inheritdoc/>
    public Vector2F MousePositionDelta { get; set; }


    /// <inheritdoc/>
    public float MouseWheelDelta
    {
      get { return _newMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void UpdateMouse(TimeSpan deltaTime)
    {
      // ----- Update mouse states.
      _previousMouseState = _newMouseState;
      _newMouseState = Mouse.GetState();
      MousePosition = MousePositionRaw;
      MousePositionDelta = MousePositionDeltaRaw;

      // ----- Find pressed mouse button.
      MouseButtons? pressedMouseButton = null;
      foreach (MouseButtons button in _mouseButtons)
      {
        if (IsDown(ref _newMouseState, button) && IsUp(ref _previousMouseState, button))
        {
          pressedMouseButton = button;
          break;
        }
      }

      // ----- Handle mouse button double clicks and button repetition.
      _lastMouseButton.IsDoubleClick = false;
      _lastMouseButton.IsVirtualPress = false;
      if (!pressedMouseButton.HasValue)
      {
        // No mouse button pressed.
        // Increase or reset down duration.
        if (IsDown(_lastMouseButton.Button))
        {
          // Previously pressed mouse button is still down.
          // Increase down duration.
          _lastMouseButton.DownDuration += deltaTime;

          // If the start interval is exceeded, we generate a virtual button press.
          if (_lastMouseButton.DownDuration >= Settings.RepetitionDelay)
          {
            // Generate virtual button press.
            _lastMouseButton.IsVirtualPress = true;

            // Subtract repetition interval from down duration. This way the repetition interval
            // must pass until the if condition is true again.
            _lastMouseButton.DownDuration -= Settings.RepetitionInterval;
          }
        }
        else
        {
          // Reset down duration.
          _lastMouseButton.DownDuration = TimeSpan.Zero;
        }

        // Measure time between clicks.
        if (_lastMouseButton.TimeSinceLastClick != TimeSpan.MaxValue)
          _lastMouseButton.TimeSinceLastClick += deltaTime;
      }
      else
      {
        // Mouse button was pressed.
        // Check for double-click.
        if (pressedMouseButton == _lastMouseButton.Button
            && _lastMouseButton.TimeSinceLastClick < Settings.DoubleClickTime - deltaTime
            && Vector2F.Absolute(_lastMouseButton.MouseClickPosition - MousePosition) < Settings.DoubleClickSize)
        {
          // Double-click detected.
          _lastMouseButton.IsDoubleClick = true;

          // The current click cannot be used for another double-click.
          _lastMouseButton.TimeSinceLastClick = TimeSpan.MaxValue;
        }
        else
        {
          // Wrong button pressed or button pressed too late.
          // Restart double-click logic.
          _lastMouseButton.TimeSinceLastClick = TimeSpan.Zero;
        }

        _lastMouseButton.Button = pressedMouseButton.Value;
        _lastMouseButton.DownDuration = TimeSpan.Zero;
        _lastMouseButton.MouseClickPosition = MousePosition;
      }

      // ----- Reset mouse position if mouse-centering is enabled. 
      if (EnableMouseCentering)
        if (GlobalSettings.PlatformID != PlatformID.WindowsPhone7 && GlobalSettings.PlatformID != PlatformID.WindowsPhone8 && GlobalSettings.PlatformID != PlatformID.Android && GlobalSettings.PlatformID != PlatformID.iOS)
          Mouse.SetPosition((int)Settings.MouseCenter.X, (int)Settings.MouseCenter.Y);
    }


    /// <inheritdoc/>
    public bool IsDown(MouseButtons button)
    {
      return IsDown(ref _newMouseState, button);
    }


    /// <inheritdoc/>
    private static bool IsDown(ref MouseState mouseState, MouseButtons button)
    {
      switch (button)
      {
        case MouseButtons.Left:
          return mouseState.LeftButton == ButtonState.Pressed;
        case MouseButtons.Middle:
          return mouseState.MiddleButton == ButtonState.Pressed;
        case MouseButtons.Right:
          return mouseState.RightButton == ButtonState.Pressed;
        case MouseButtons.XButton1:
          return mouseState.XButton1 == ButtonState.Pressed;
        case MouseButtons.XButton2:
          return mouseState.XButton2 == ButtonState.Pressed;
        default:
          return false;
      }
    }


    /// <inheritdoc/>
    public bool IsUp(MouseButtons button)
    {
      return IsUp(ref _newMouseState, button);
    }


    /// <inheritdoc/>
    private static bool IsUp(ref MouseState mouseState, MouseButtons button)
    {
      switch (button)
      {
        case MouseButtons.Left:
          return mouseState.LeftButton == ButtonState.Released;
        case MouseButtons.Middle:
          return mouseState.MiddleButton == ButtonState.Released;
        case MouseButtons.Right:
          return mouseState.RightButton == ButtonState.Released;
        case MouseButtons.XButton1:
          return mouseState.XButton1 == ButtonState.Released;
        case MouseButtons.XButton2:
          return mouseState.XButton2 == ButtonState.Released;
        default:
          return false;
      }
    }


    /// <inheritdoc/>
    public bool IsPressed(MouseButtons button, bool useButtonRepetition)
    {
      if (useButtonRepetition)
      {
        if (_lastMouseButton.Button == button && _lastMouseButton.IsVirtualPress)
        {
          return true;
        }
      }

      switch (button)
      {
        case MouseButtons.Left:
          return _newMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
        case MouseButtons.Middle:
          return _newMouseState.MiddleButton == ButtonState.Pressed && _previousMouseState.MiddleButton == ButtonState.Released;
        case MouseButtons.Right:
          return _newMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released;
        case MouseButtons.XButton1:
          return _newMouseState.XButton1 == ButtonState.Pressed && _previousMouseState.XButton1 == ButtonState.Released;
        case MouseButtons.XButton2:
          return _newMouseState.XButton2 == ButtonState.Pressed && _previousMouseState.XButton2 == ButtonState.Released;
        default:
          return false;
      }
    }


    /// <inheritdoc/>
    public bool IsReleased(MouseButtons button)
    {
      switch (button)
      {
        case MouseButtons.Left:
          return _newMouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Pressed;
        case MouseButtons.Middle:
          return _newMouseState.MiddleButton == ButtonState.Released && _previousMouseState.MiddleButton == ButtonState.Pressed;
        case MouseButtons.Right:
          return _newMouseState.RightButton == ButtonState.Released && _previousMouseState.RightButton == ButtonState.Pressed;
        case MouseButtons.XButton1:
          return _newMouseState.XButton1 == ButtonState.Released && _previousMouseState.XButton1 == ButtonState.Pressed;
        case MouseButtons.XButton2:
          return _newMouseState.XButton2 == ButtonState.Released && _previousMouseState.XButton2 == ButtonState.Pressed;
        default:
          return false;
      }
    }


    /// <inheritdoc/>
    public bool IsDoubleClick(MouseButtons button)
    {
      return _lastMouseButton.Button == button && _lastMouseButton.IsDoubleClick;
    }
    #endregion
  }
}
