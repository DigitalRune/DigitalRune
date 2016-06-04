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
  /// <summary>
  /// Defines settings for the input service.
  /// </summary>
  public class InputSettings
  {
    /// <summary>
    /// Gets or sets the double click time interval which defines the time that is allowed between
    /// two clicks to still count as a double click.
    /// </summary>
    /// <value>
    /// The double click time interval. The default value is 500 ms.
    /// </value>
    public TimeSpan DoubleClickTime { get; set; }


    /// <summary>
    /// Gets or sets the dimensions, in pixels, of the area within which the 
    /// user must click twice for the operating system to consider the two 
    /// clicks a double-click.
    /// </summary>
    /// <value>
    /// A 2-dimensional vector that indicates the dimensions, in pixels, of the 
    /// area within which the user must click twice to consider the two clicks 
    /// a double-click. The default value is (100, 100) on phones and (4, 4) 
    /// on other platforms.
    /// </value>
    public Vector2F DoubleClickSize { get; set; }


    /// <summary>
    /// Gets or sets the mouse center for the mouse centering.
    /// </summary>
    /// <value>The mouse center in pixels. The default values (300, 300).</value>
    /// <remarks>
    /// If <see cref="IInputService.EnableMouseCentering"/> is <see langword="true"/>, the input
    /// service will reset the mouse position to <see cref="MouseCenter"/> in each frame. This is 
    /// necessary, for example, for first-person shooters that need only relative mouse input.
    /// </remarks>
    public Vector2F MouseCenter { get; set; }


    /// <summary>
    /// Gets or sets the repetition start delay for virtual key or button presses.
    /// </summary>
    /// <value>The repetition start delay. The default value is 500 ms.</value>
    /// <remarks>
    /// If a key or button is held down for longer than the <see cref="RepetitionDelay"/>, the input 
    /// service will start to generate virtual key/button presses at a rate defined by 
    /// <see cref="RepetitionInterval"/>. (See <see cref="IInputService"/> for more info.)
    /// </remarks>
    public TimeSpan RepetitionDelay { get; set; }


    /// <summary>
    /// Gets or sets the repetition interval for virtual key or button presses.
    /// </summary>
    /// <value>The repetition interval. The default value is 100 ms.</value>
    /// <remarks>
    /// If a key or button is held down for longer than the <see cref="RepetitionDelay"/>,
    /// the input service will start to generate virtual key/button presses at a rate defined by 
    /// <see cref="RepetitionInterval"/>. (See <see cref="IInputService"/> for more info.)
    /// </remarks>
    public TimeSpan RepetitionInterval { get; set; }


    /// <summary>
    /// Gets or sets the thumbstick threshold for detecting thumbstick button presses.
    /// </summary>
    /// <value>
    /// The thumbstick threshold value in the range [0, 1]. A thumbstick axis counts as "down" if 
    /// its absolute value exceeds the threshold value. The default value is 0.5.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Breaking change. Fix in next version.")]
    public float ThumbstickThreshold { get; set; }


    /// <summary>
    /// Gets or sets the trigger threshold for detecting button presses.
    /// </summary>
    /// <value>
    /// The trigger threshold value in the range [0, 1]. A trigger counts as "down" if its value 
    /// exceeds the threshold value. The default value is 0.2.
    /// </value>
    public float TriggerThreshold { get; set; }


#if !SILVERLIGHT
    /// <summary>
    /// Gets or sets the type of gamepad dead zone processing that is used for analog sticks
    /// of the gamepads. (Not available in Silverlight.)
    /// </summary>
    /// <value>
    /// The type of dead zone processing. The default value is
    /// <strong>GamePadDeadZone.IndependentAxes</strong>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GamePad")]
    public GamePadDeadZone GamePadDeadZone { get; set; }
#endif


    /// <summary>
    /// Initializes a new instance of the <see cref="InputSettings"/> class.
    /// </summary>
    public InputSettings()
    {
      DoubleClickTime = new TimeSpan(0, 0, 0, 0, 500);    // 500 ms
      if (GlobalSettings.PlatformID == PlatformID.WindowsPhone7
          || GlobalSettings.PlatformID == PlatformID.WindowsPhone8
          || GlobalSettings.PlatformID == PlatformID.WindowsStore
          || GlobalSettings.PlatformID == PlatformID.Android
          || GlobalSettings.PlatformID == PlatformID.iOS)
      {
        // No mouse on phone. Mouse is always used for touch input.
        // Use 100 pixels for big fingers.
        DoubleClickSize = new Vector2F(100);
      }
      else
      {
        DoubleClickSize = new Vector2F(4);
      }

      MouseCenter = new Vector2F(300, 300);
      RepetitionDelay = new TimeSpan(0, 0, 0, 0, 500);    // 500 ms
      RepetitionInterval = new TimeSpan(0, 0, 0, 0, 100); // 100 ms
      ThumbstickThreshold = 0.5f;
      TriggerThreshold = 0.2f;
#if !SILVERLIGHT
      GamePadDeadZone = GamePadDeadZone.IndependentAxes;
#endif
    }
  }
}
