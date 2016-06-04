// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Game.UI
{
  /// <summary>
  /// Manages tool tips.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Each <see cref="UIScreen"/> has a <see cref="UIScreen.ToolTipManager"/>. (Currently it is not
  /// possible to use a custom ToolTipManager.)
  /// </para>
  /// <para>
  /// Tool tips can be defined per control using the property <see cref="UIControl.ToolTip"/>. The 
  /// tool tip can be a <see cref="UIControl"/>, a <see cref="String"/>, or an <see cref="Object"/>:
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// <strong>UIControl:</strong> If the tool tip is a control, then the control is shown as the
  /// content of the <see cref="ToolTipManager.ToolTipControl"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <strong>String:</strong> If the tool tip is a <see cref="String"/>, then the string will be
  /// wrapped in a 
  /// <see cref="TextBlock"/> and shown in the <see cref="ToolTipControl"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <strong>Object:</strong> If the tool tip is an <see cref="Object"/>, then the string
  /// representation of the object will be shown as the tool tip. (The string will be wrapped in a 
  /// <see cref="TextBlock"/> and shown in the <see cref="ToolTipControl"/>.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// The user can override this behavior by setting the <see cref="CreateToolTipContent"/> 
  /// callback. The callback receives the value stored in <see cref="UIControl.ToolTip"/> and 
  /// returns the <see cref="UIControl"/> that will be shown in the <see cref="ToolTipControl"/>.
  /// </para>
  /// <para>
  /// <strong>Control Styles:</strong> Per default, the <see cref="ToolTipControl"/> uses the style 
  /// "ToolTip" and the <see cref="TextBlock"/> that wraps the tool tip content uses the style 
  /// "ToolTipText". (See <see cref="UIControl.Style"/> for more information about styles.)
  /// </para>
  /// </remarks>
  public class ToolTipManager
  {
    // There is only one ToolTipControl. It is a ContentControl with a certain style.
    // It will be displayed on top of all other UIControls if the mouse hasn't moved
    // for a certain time and the control under the mouse has specified a ToolTip.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // How long the mouse cursor has been still.
    private TimeSpan _noMouseMoveDuration;

    // The control for which the tool tip is shown.
    private UIControl _control;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a callback that creates a <see cref="UIControl"/> for a 
    /// <see cref="UIControl.ToolTip"/>.
    /// </summary>
    /// <value>
    /// <para>
    /// A method that creates a <see cref="UIControl"/> for a <see cref="UIControl.ToolTip"/>. This 
    /// method receives the value of the <see cref="UIControl.ToolTip"/> property and must return a
    /// <see cref="UIControl"/> which should be shown in the <see cref="ToolTipControl"/>. 
    /// </para>
    /// <para>
    /// If the method is null or returns null, the <see cref="UIControl.ToolTip"/> will be wrapped 
    /// in a <see cref="TextBlock"/>s and shown in the <see cref="ToolTipControl"/>.
    /// </para>
    /// <para>
    /// The default is <see langword="null"/>.
    /// </para>
    /// </value>
    public Func<object, UIControl> CreateToolTipContent { get; set; }


    /// <summary>
    /// Gets a value indicating whether a tool tip is currently shown.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a tool tip is currently visible; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsToolTipOpen
    {
      get { return ToolTipControl.IsVisible; }
    }


    /// <summary>
    /// Gets or sets the screen.
    /// </summary>
    /// <value>The screen.</value>
    public UIScreen Screen { get; private set; }


    /// <summary>
    /// Gets the <see cref="ContentControl"/> that shows the tool tip.
    /// </summary>
    /// <value>The tool tip control.</value>
    public ContentControl ToolTipControl { get; private set; }


    /// <summary>
    /// Gets the time which the mouse has to stand still before a tool tip pops up.
    /// </summary>
    /// <value>The time which the mouse has to stand still before a tool tip pops up.</value>
    public TimeSpan ToolTipDelay { get { return Screen.ToolTipDelay; } }


    /// <summary>
    /// Gets the offset of the tool tip to the mouse position.
    /// </summary>
    /// <value>The offset of the tool tip to the mouse position.</value>
    public float ToolTipOffset { get { return Screen.ToolTipOffset; } }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolTipManager"/> class.
    /// </summary>
    /// <param name="screen">The screen.</param>
    internal ToolTipManager(UIScreen screen)
    {
      Screen = screen;
      Screen.InputProcessed += OnScreenInputProcessed;

      ToolTipControl = new ContentControl
      {
        Name = "ToolTip",
        Style = "ToolTip",
        IsVisible = false,
      };
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnScreenInputProcessed(object sender, InputEventArgs eventArgs)
    {
      // Automatically hide the ToolTip if the Screen does not process input (because then
      // we would not see mouse movement, so we close it immediately).
      // Or close it if another UIControl was added on top that could hide the ToolTip.
      if (IsToolTipOpen)
      {
        if (!Screen.InputEnabled                                              // Screen does not handle input.
            || Screen.Children[Screen.Children.Count - 1] != ToolTipControl)  // Another window was opened.
        {
          // ----- Screen was disabled, or another control was shown on top.
          CloseToolTip();
          return;
        }
      }

      var context = eventArgs.Context;
      if (context.MousePositionDelta == Vector2F.Zero)
      {
        // ----- No mouse movement --> Increase counter.
        _noMouseMoveDuration += context.DeltaTime;
      }
      else
      {
        // ----- Mouse moved --> Close tool tip if mouse is no longer over control. Reset counter.
        if (_control == null || !_control.IsMouseOver)
          CloseToolTip();

        _noMouseMoveDuration = TimeSpan.Zero;
        return;
      }

      if (_noMouseMoveDuration >= ToolTipDelay)
      {
        // ----- Mouse was not moving for ToolTipDelay seconds --> Show tool tip.

        // Get control under the mouse cursor. Search up the control hierarchy until we
        // find a control with a tool tip string.
        var control = Screen.ControlUnderMouse;
        while (control != null && control.ToolTip == null)
          control = control.VisualParent;

        if (control != null)
        {
          // Show or update tool tip.
          ShowToolTip(control.ToolTip, context.MousePosition);
          _control = control;
        }

        // We do not want to check on the same position again in the next frame, so we set
        // an extreme value that will be automatically reset when the mouse moves in the future.
        _noMouseMoveDuration = TimeSpan.MinValue;
      }
    }


    /// <summary>
    /// Shows a tool tip.
    /// </summary>
    /// <param name="toolTip">The tool tip.</param>
    /// <param name="mousePosition">The mouse position.</param>
    public void ShowToolTip(object toolTip, Vector2F mousePosition)
    {
      if (toolTip == null)
        return;

      UIControl content = null;
      if (CreateToolTipContent != null)
        content = CreateToolTipContent(toolTip);

      if (content == null)
      {
        var control = toolTip as UIControl;
        if (control != null)
          content = control;
        else
          content = new TextBlock { Style = "ToolTipText", Text = toolTip.ToString() };
      }

      ToolTipControl.Content = content;
      ToolTipControl.IsVisible = true;

      // If this is the first time, we have to add the control to the screen. 
      // Otherwise, we make sure that the tool tip is on top.
      if (ToolTipControl.VisualParent == null)
        Screen.Children.Add(ToolTipControl);
      else
        Screen.BringToFront(ToolTipControl);

      // Determine position next to mouse. Position it so that it fits onto the screen.
      ToolTipControl.Measure(new Vector2F(float.PositiveInfinity));
      float x = mousePosition.X;
      if (x + ToolTipControl.DesiredWidth > Screen.ActualWidth)
      {
        // Draw the tool tip on the left.
        x = x - ToolTipControl.DesiredWidth;
      }

      float y = mousePosition.Y + ToolTipOffset;
      if (y + ToolTipControl.DesiredHeight > Screen.ActualHeight)
      {
        // Draw the tool tip on top.
        // (We use 0.5 * offset if tool tip is above cursor. This assumes that the cursor is 
        // similar to the typical arrow where the hot spot is at the top. If the cursor is a 
        // different shape we might need to adjust the offset.)
        y = y - ToolTipControl.DesiredHeight - 1.5f * ToolTipOffset;
      }

      ToolTipControl.X = x;
      ToolTipControl.Y = y;
    }


    /// <summary>
    /// Hides the tool tip or does nothing if no tool tip is visible.
    /// </summary>
    public void CloseToolTip()
    {
      ToolTipControl.Content = null;
      ToolTipControl.IsVisible = false;
      _control = null;
    }
    #endregion
  }
}
