// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Input;

#if !WP7 && !XBOX
using System.Runtime.InteropServices;
#endif
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif


namespace DigitalRune.Game.Input
{
  partial class InputManager
  {
    //--------------------------------------------------------------
    #region Native Methods
      //--------------------------------------------------------------

#if WINDOWS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
    [DllImport("user32.dll")]
    private static extern short GetKeyState(int key);
#endif
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public KeyboardState KeyboardState
    {
      get { return _newKeyboardState; }
    }
    private KeyboardState _newKeyboardState;


    /// <inheritdoc/>
    public KeyboardState PreviousKeyboardState
    {
      get { return _previousKeyboardState; }
    }
    private KeyboardState _previousKeyboardState;


    /// <inheritdoc/>
    public ReadOnlyCollection<Keys> PressedKeys
    {
      get { return _pressedKeysAsReadOnly; }
    }
    private readonly List<Keys> _pressedKeys;
    private readonly ReadOnlyCollection<Keys> _pressedKeysAsReadOnly;


    /// <inheritdoc/>
    public ModifierKeys ModifierKeys
    {
      get
      {
        if (_modifierKeys.HasValue)
          return _modifierKeys.Value;

        // Lazy evaluation. _modifierKeys is automatically reset to null in Update().
        // Check which modifier keys are pressed.
        _modifierKeys = ModifierKeys.None;
#if !SILVERLIGHT
        if (IsDown(Keys.LeftShift) || IsDown(Keys.RightShift))
          _modifierKeys = _modifierKeys | ModifierKeys.Shift;
        if (IsDown(Keys.LeftControl) || IsDown(Keys.RightControl))
          _modifierKeys = _modifierKeys | ModifierKeys.Control;
        if (IsDown(Keys.LeftAlt))
          _modifierKeys = _modifierKeys | ModifierKeys.Alt;
        if (IsDown(Keys.RightAlt))
          _modifierKeys = _modifierKeys | ModifierKeys.ControlAlt;
#else
        if (IsDown(Keys.Shift))
          _modifierKeys = _modifierKeys | ModifierKeys.Shift;
        if (IsDown(Keys.Ctrl))
          _modifierKeys = _modifierKeys | ModifierKeys.Control;
        if (IsDown(Keys.Alt))
          _modifierKeys = _modifierKeys | ModifierKeys.Alt;
#endif


        return _modifierKeys.Value;
      }
    }
    private ModifierKeys? _modifierKeys;
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void UpdateKeyboard(TimeSpan deltaTime)
    {
      // ----- Update keyboard states.
      _previousKeyboardState = _newKeyboardState;
      _newKeyboardState = Keyboard.GetState();

      // ----- Reset modifier keys. (They are evaluated when needed.)
      _modifierKeys = null;

      // ----- Find pressed keys 
      // (We store all keys in a list and check for virtual key presses).
      _pressedKeys.Clear();
      foreach (Keys key in _keys)
      {
        if (_newKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key))
        {
          _pressedKeys.Add(key);
        }
      }

      // ----- Handle key double clicks and key repetition.
      _lastKey.IsDoubleClick = false;
      _lastKey.IsVirtualPress = false;
      if (_pressedKeys.Count == 0)
      {
        // No key pressed.
        // Increase or reset down duration.
        if (IsDown(_lastKey.Button))
        {
          // Previously pressed key is still down.
          // Increase down duration.
          _lastKey.DownDuration += deltaTime;

          // If the start interval is exceeded, we generate a virtual key press.
          if (_lastKey.DownDuration >= Settings.RepetitionDelay)
          {
            _pressedKeys.Add(_lastKey.Button);
            _lastKey.IsVirtualPress = true;

            // Subtract repetition interval from down duration. This way the repetition interval
            // must pass until the if condition is true again.
            _lastKey.DownDuration -= Settings.RepetitionInterval;
          }
        }
        else
        {
          // Reset down duration.
          _lastKey.DownDuration = TimeSpan.Zero;
        }

        // Measure time between clicks.
        if (_lastKey.TimeSinceLastClick != TimeSpan.MaxValue)
          _lastKey.TimeSinceLastClick += deltaTime;
      }
      else
      {
        // Key was pressed.
        // Check for double-click.
        if (_pressedKeys[0] == _lastKey.Button
            && _lastKey.TimeSinceLastClick < Settings.DoubleClickTime - deltaTime)
        {
          // Double-click detected.
          _lastKey.IsDoubleClick = true;

          // The current click cannot be used for another double-click.
          _lastKey.TimeSinceLastClick = TimeSpan.MaxValue;
        }
        else
        {
          // Wrong key pressed or key pressed too late.
          // Restart double-click logic.
          _lastKey.TimeSinceLastClick = TimeSpan.Zero;
        }

        _lastKey.Button = _pressedKeys[0];
        _lastKey.DownDuration = TimeSpan.Zero;
      }
    }


    /// <inheritdoc/>
    public bool IsDown(Keys key)
    {
#if WINDOWS
      if (key == Keys.CapsLock)
      {
        // Special handling of Caps Lock.
        return (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
      }
#endif

      return _newKeyboardState.IsKeyDown(key);
    }


    /// <inheritdoc/>
    public bool IsUp(Keys key)
    {
#if WINDOWS
      if (key == Keys.CapsLock)
      {
        // Special handling of Caps Lock.
        return (((ushort)GetKeyState(0x14)) & 0xffff) == 0;
      }
#endif

      return _newKeyboardState.IsKeyUp(key);
    }


    /// <inheritdoc/>
    public bool IsPressed(Keys key, bool useKeyRepetition)
    {
      if (useKeyRepetition)
      {
        if (_lastKey.Button == key && _lastKey.IsVirtualPress)
        {
          return true;
        }
      }

      return _newKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }


    /// <inheritdoc/>
    public bool IsReleased(Keys key)
    {
      return _newKeyboardState.IsKeyUp(key) && _previousKeyboardState.IsKeyDown(key);
    }


    /// <inheritdoc/>
    public bool IsDoubleClick(Keys key)
    {
      return _lastKey.Button == key && _lastKey.IsDoubleClick;
    }
    #endregion
  }
}
