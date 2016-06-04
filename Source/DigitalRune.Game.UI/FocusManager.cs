// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Input;
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif


namespace DigitalRune.Game.UI
{
  /// <summary>
  /// Controls which UI control has the focus.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The focus defines which control receives the device input. Controls must be 
  /// <see cref="UIControl.Focusable"/> to be able to get the focus. 
  /// </para>
  /// <para>
  /// Focus scopes handle the focus movement. A control is a <see cref="FocusScope"/> if 
  /// <see cref="UIControl.IsFocusScope"/> is set. Per default, only <see cref="Window"/>s are focus
  /// scopes. If a control is inside a focus scope is focused, the focus can be moved to another
  /// control of the same focus scope using the arrow keys on the keyboard, the left thumb stick or
  /// the DPad on the gamepad.
  /// </para>
  /// <para>
  /// Each <see cref="UIScreen"/> has a <see cref="UIScreen.FocusManager"/>. It is allowed to
  /// exchange the focus manager instance. Derived focus manager classes can override 
  /// <see cref="OnMoveFocus"/> to change how the focus moves.
  /// </para>
  /// <para>
  /// Sometimes it is desirable to automatically move the focus to the control under the mouse, e.g.
  /// when the mouse moves over menu entries. For this <see cref="UIControl.FocusWhenMouseOver"/> 
  /// can be set for a control, e.g. for the control that represents the menu entry. 
  /// </para>
  /// <para>
  /// The UI control property <see cref="UIControl.AutoUnfocus"/> can be set if the focus should be
  /// removed from the control (and its nested controls) if the user clicks onto the non-focusable
  /// space of the screen. <see cref="UIControl.AutoUnfocus"/> is usually set for controls that are 
  /// focus scopes or for the <see cref="UIScreen"/>.
  /// </para>
  /// </remarks>
  public class FocusManager
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The focus manager adds a "LastFocusedControl" property to controls that are focus scopes.
    // The property stores the control that had the focus before the focus moved out of the scope.
    // If the focus scope gets focused, the focus moves back to the LastFocusedControl.
    private static readonly int LastFocusedControlPropertyId = GameObject.CreateProperty<UIControl>(
      "LastFocusedControl", GamePropertyCategories.Behavior, null, null).Id;

    // This list will be used within the MoveFocus method. Else it is empty.
    private readonly List<UIControl> _focusableControls = new List<UIControl>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    private IInputService InputService { get { return Screen.InputService; } }


    /// <summary>
    /// Gets the screen.
    /// </summary>
    /// <value>The screen.</value>
    public UIScreen Screen { get; private set; }


    /// <summary>
    /// Gets the control that currently has the focus.
    /// </summary>
    /// <value>The focused control.</value>
    public UIControl FocusedControl { get; private set; }


    /// <summary>
    /// Gets the focus scope that currently contains the focused control.
    /// </summary>
    /// <value>The focus scope.</value>
    /// <remarks>
    /// This value can be <see langword="null"/>, e.g. if the focused control is a child of the
    /// <see cref="UIScreen"/> and the screen is no focus scope. (Per default, screens are not
    /// focus scopes.)
    /// </remarks>
    public UIControl FocusScope { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FocusManager"/> class.
    /// </summary>
    /// <param name="screen">The screen.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="screen"/> is <see langword="null"/>.
    /// </exception>
    public FocusManager(UIScreen screen)
    {
      if (screen == null)
        throw new ArgumentNullException("screen");

      Screen = screen;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes the focus from the current <see cref="FocusedControl"/>.
    /// </summary>
    public void ClearFocus()
    {
      if (FocusedControl == null)
        return;

      // Reset IsFocused and IsFocusWithin in the FocusedControl and all ancestors.
      FocusedControl.IsFocused = false;
      var control = FocusedControl;
      while (control != null)
      {
        control.IsFocusWithin = false;
        control = control.VisualParent;
      }

      FocusedControl = null;
      FocusScope = null;
    }


    /// <summary>
    /// Moves focus to a control.
    /// </summary>
    /// <param name="control">
    /// The control that should get the focus. (If <paramref name="control"/> is 
    /// <see langword="null"/>, this method does nothing. Use <see cref="ClearFocus"/> to remove the
    /// focus from a control.)
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the focus was moved; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Disabled or invisible controls cannot be focused. If the <paramref name="control"/> is not
    /// itself <see cref="UIControl.Focusable"/>, the method tries to focus a child control.
    /// </remarks>
    public bool Focus(UIControl control)
    {
      if (control == null)
        return false;

      // Cannot focus a disabled or invisible control.
      if (!control.ActualIsEnabled || !control.ActualIsVisible)
        return false;

      if (!control.Focusable)
      {
        if (control.IsFocusScope)
        {
          // Try to move focus to the last focused control.
          var lastFocusedControl = control.GetValue<UIControl>(LastFocusedControlPropertyId);
          if (Focus(lastFocusedControl) && control.IsFocusWithin)
            return true;
        }

        // Control is not focusable. --> Try to focus a child.
        foreach (var child in control.VisualChildren)
        {
          if (Focus(child))
            return true;
        }

        return false;
      }

#if !WP7
#if PORTABLE
      if (GlobalSettings.PlatformID != PlatformID.WindowsPhone8)
#endif
      {
        // Move control to visible area. On phone this is typically not done to avoid non-smooth
        // scrolling.
        control.BringIntoView();
      }
#endif

      // Abort if control is already focused. 
      // (Note: We do the check here and not at the beginning of the method because we still want
      // to do the things above to bring the control into the view, even if the control already has 
      // focus.)
      if (control.IsFocused)
        return true;

      // First, clear the current IsFocused and IsFocusWithin flags.
      ClearFocus();

      // Set IsFocused and IsFocusWithin flags in FocusedControl and all ancestors.
      FocusedControl = control;
      FocusedControl.IsFocused = true;
      FocusScope = null;
      while (control != null)
      {
        if (control.IsFocusScope)
        {
          // Remember the first focus scope that we find.
          if (FocusScope == null)
            FocusScope = control;

          // Focus scope remembers last focused control.
          control.SetValue(LastFocusedControlPropertyId, FocusedControl);
        }

        control.IsFocusWithin = true;
        control = control.VisualParent;
      }

      return true;
    }


    /// <summary>
    /// Checks input devices and moves the focus if according input is detected.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="allowedPlayer">The player that controls the input.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    internal void MoveFocus(UIControl control, LogicalPlayerIndex allowedPlayer)
    {
      // Called by the UIControl after UIControl.OnHandleInput.

      if (control == null)
        return;

      var inputService = InputService;

      // Handle AutoUnfocus.
      if (control.AutoUnfocus && FocusedControl != null)
      {
        if (inputService.IsPressed(MouseButtons.Left, false) || inputService.IsPressed(MouseButtons.Right, false))
        {
          if (control == FocusScope && !FocusedControl.IsMouseOver                     // E.g. a window and not the focused control is clicked.
              || control.IsMouseDirectlyOver && !inputService.IsMouseOrTouchHandled)   // E.g. a screen and no child UIControl is clicked.
          {
            ClearFocus();
          }
        }
      }

      // Only focus scopes handle focus movement.
      // And only the current focus scope (which means the first focus scope in the hierarchy).
      if (!control.IsFocusScope || FocusScope != null && control != FocusScope)
        return;

      bool moveLeft = false;
      bool moveRight = false;
      bool moveUp = false;
      bool moveDown = false;

      // Check arrow keys.
      if (!inputService.IsKeyboardHandled)
      {
        // This focus scope "absorbs" arrow keys.
        if (inputService.IsDown(Keys.Left) 
            || inputService.IsDown(Keys.Right)
            || inputService.IsDown(Keys.Up) 
            || inputService.IsDown(Keys.Down))
        {
          inputService.IsKeyboardHandled = true;

        if (inputService.IsPressed(Keys.Left, true))
          moveLeft = true;
        else if (inputService.IsPressed(Keys.Right, true))
          moveRight = true;
        else if (inputService.IsPressed(Keys.Up, true))
          moveUp = true;
        else if (inputService.IsPressed(Keys.Down, true))
          moveDown = true;
        }
      }

#if !SILVERLIGHT
      // Check left thumb stick and d-pad.
      if (!moveLeft && !moveRight && !moveUp && !moveDown && !inputService.IsGamePadHandled(allowedPlayer))
      {
        if (inputService.IsDown(Buttons.LeftThumbstickLeft, allowedPlayer) 
            || inputService.IsDown(Buttons.LeftThumbstickRight, allowedPlayer) 
            || inputService.IsDown(Buttons.LeftThumbstickUp, allowedPlayer) 
            || inputService.IsDown(Buttons.LeftThumbstickDown, allowedPlayer)
            || inputService.IsDown(Buttons.DPadLeft, allowedPlayer)
            || inputService.IsDown(Buttons.DPadRight, allowedPlayer)
            || inputService.IsDown(Buttons.DPadUp, allowedPlayer)
            || inputService.IsDown(Buttons.DPadDown, allowedPlayer))
        {
          inputService.SetGamePadHandled(allowedPlayer, true);

        if (inputService.IsPressed(Buttons.LeftThumbstickLeft, true, allowedPlayer) || inputService.IsPressed(Buttons.DPadLeft, true, allowedPlayer))
          moveLeft = true;
        else if (inputService.IsPressed(Buttons.LeftThumbstickRight, true, allowedPlayer) || inputService.IsPressed(Buttons.DPadRight, true, allowedPlayer))
          moveRight = true;
        else if (inputService.IsPressed(Buttons.LeftThumbstickUp, true, allowedPlayer) || inputService.IsPressed(Buttons.DPadUp, true, allowedPlayer))
          moveUp = true;
        else if (inputService.IsPressed(Buttons.LeftThumbstickDown, true, allowedPlayer) || inputService.IsPressed(Buttons.DPadDown, true, allowedPlayer))
          moveDown = true;
        }
      }
#endif

      // Now, we know in which direction the focus should move.

      if (!moveLeft && !moveRight && !moveUp && !moveDown)
        return;

      // Collect all focusable controls.
      _focusableControls.Clear();
      GetFocusableControls(control);
      if (_focusableControls.Count == 0)
        return;

      // Call virtual method that does the job.
      var target = OnMoveFocus(moveLeft, moveRight, moveUp, moveDown, _focusableControls);

      Focus(target);

      _focusableControls.Clear();
    }


    /// <summary>
    /// Called when the focus should be moved to another control.
    /// </summary>
    /// <param name="moveLeft">If set to <see langword="true"/> the focus should move left.</param>
    /// <param name="moveRight">If set to <see langword="true"/> the focus should move right.</param>
    /// <param name="moveUp">If set to <see langword="true"/> the focus should move up.</param>
    /// <param name="moveDown">If set to <see langword="true"/> the focus should move down.</param>
    /// <param name="focusableControls">The focusable controls of the current focus scopes.</param>
    /// <returns>The target control that should receive the focus.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual UIControl OnMoveFocus(bool moveLeft, bool moveRight, bool moveUp, bool moveDown, List<UIControl> focusableControls)
    {
      // If nothing has focus yet, focus the next best control.
      if (FocusedControl == null)
        return _focusableControls[0];

      // Find new focused control.
      // We compute the closest distance from the FocusedControl's bounds to the other control's bounds.
      // We move to the closest non-intersecting control determined by the Manhattan distance metric.
      
      // To allow for a little bit of overlap we reduce the bounds of the focused control to
      // a horizontal or vertical line.
      RectangleF focusedControlBounds;
      if (moveLeft || moveRight)
      {
        // Reduce bounds to vertical line.
        focusedControlBounds = new RectangleF(FocusedControl.ActualX + FocusedControl.ActualWidth / 2, FocusedControl.ActualY, 0, FocusedControl.ActualHeight);
      }
      else
      {
        // Reduce bounds horizontal line.
        focusedControlBounds = new RectangleF(FocusedControl.ActualX, FocusedControl.ActualY + FocusedControl.ActualHeight / 2, FocusedControl.ActualWidth, 0);
      }

      // Get the distance of the focused control bounds (flattened to a line) to the bounds of the
      // other controls. We move to the control with the shortest Manhattan distance.
      UIControl target = null;
      float minDistance = float.MaxValue;
      foreach (var candidate in focusableControls)
      {
        RectangleF candidateBounds = new RectangleF(candidate.ActualX, candidate.ActualY, candidate.ActualWidth, candidate.ActualHeight);

        Vector2F distance = GetDistance(focusedControlBounds, candidateBounds);
        float manhattanDistance = Math.Abs(distance.X) + Math.Abs(distance.Y);

        if (moveLeft)
        {
          if (distance.X < 0 && manhattanDistance < minDistance)
          {
            target = candidate;
            minDistance = manhattanDistance;
          }
        }
        else if (moveRight)
        {
          if (distance.X > 0 && manhattanDistance < minDistance)
          {
            target = candidate;
            minDistance = manhattanDistance;
          }
        }
        else if (moveUp)
        {
          if (distance.Y < 0 && manhattanDistance < minDistance)
          {
            target = candidate;
            minDistance = manhattanDistance;
          }
        }
        else if (moveDown)
        {
          if (distance.Y > 0 && manhattanDistance < minDistance)
          {
            target = candidate;
            minDistance = manhattanDistance;
          }
        }
      }

      return target;
    }


    /// <summary>
    /// Recursively collects all focusable controls of the same focus scope.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <remarks>
    /// The focusable controls are stored in _focusableControls. <see cref="FocusedControl"/> will 
    /// be excluded from the list.
    /// </remarks>
    private void GetFocusableControls(UIControl control)
    {
      if (control == FocusedControl)
        return;

      if (control.Focusable && control.IsEnabled && control.IsVisible)
      {
        _focusableControls.Add(control);
      }
      else
      {
        foreach (var child in control.VisualChildren)
          if (child.IsEnabled && child.IsVisible)
            GetFocusableControls(child);
      }
    }


    /// <summary>
    /// Gets the distance vector between the closest points (pointing from the first to the second 
    /// rectangle).
    /// </summary>
    /// <param name="rectangle0">The first rectangle.</param>
    /// <param name="rectangle1">The second rectangle.</param>
    /// <returns>
    /// The distance vector between the closest points of the two rectangle. The vector points from 
    /// <paramref name="rectangle0"/> to <paramref name="rectangle1"/>. The vector is (0, 0) if the 
    /// rectangles are intersecting.
    /// </returns>
    private static Vector2F GetDistance(RectangleF rectangle0, RectangleF rectangle1)
    {
      RectangleF a = rectangle0;
      RectangleF b = rectangle1;

      // We check for separation in the 8 Voronoi regions of a.
      if (a.Top > b.Bottom)
      {
        // B is on top.

        if (a.Left > b.Right)
        {
          // B is on top and left.
          return new Vector2F(b.Right - a.Left, b.Bottom - a.Top);
        }

        if (a.Right < b.Left)
        {
          // B is on top and right.
          return new Vector2F(b.Left - a.Right, b.Bottom - a.Top);
        }

        // B is on top.
        return new Vector2F(0, b.Bottom - a.Top);
      }

      if (a.Bottom < b.Top)
      {
        // B is below.

        if (a.Left > b.Right)
        {
          // B is on below and left.
          return new Vector2F(b.Right - a.Left, b.Top - a.Bottom);
        }
        
        if (a.Right < b.Left)
        {
          // B is on below and right.
          return new Vector2F(b.Left - a.Right, b.Top - a.Bottom);
        }

        // B is on below.
        return new Vector2F(0, b.Top - a.Bottom);
      }

      if (a.Left > b.Right)
      {
        // B is left.
        return new Vector2F(b.Right - a.Left, 0);
      }
      
      if (a.Right < b.Left)
      {
        // B is right.
        return new Vector2F(b.Left - a.Right, 0);
      }

      // A and B are intersecting.
      return new Vector2F();
    }
    #endregion
  }
}
