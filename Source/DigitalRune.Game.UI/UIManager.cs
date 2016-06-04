// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using DigitalRune;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;


namespace DigitalRune.Game.UI
{
  /// <summary>
  /// Manages the game user interface.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The most important job of this class is to update the screens. Therefore, <see cref="Update"/>
  /// must be called once per frame.
  /// </para>
  /// <para>
  /// The <see cref="UIManager"/> monitors the game window orientation and calls 
  /// <see cref="UIControl.InvalidateMeasure"/> when it changes (only relevant on Windows Phone 7).
  /// </para>
  /// </remarks>
  public class UIManager : IUIService
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private class UIScreenComparer : IComparer<UIScreen>
    {
      public static readonly UIScreenComparer Instance = new UIScreenComparer();

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
      public int Compare(UIScreen x, UIScreen y)
      {
        return y.ZIndex.CompareTo(x.ZIndex);
      }
    }
    #endregion

    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The screens sorted front-to-back.
    private readonly List<UIScreen> _sortedScreens = new List<UIScreen>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public object Cursor { get; set; }


    /// <inheritdoc/>
    public object GameForm { get; private set; }


    /// <inheritdoc/>
    public IInputService InputService { get; private set; }


    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public KeyMap KeyMap
    {
      get { return _keyMap; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _keyMap = value;
      }
    }
    private KeyMap _keyMap;


    /// <inheritdoc/>
    public UIScreenCollection Screens { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

#if !SILVERLIGHT
    /// <summary>
    /// Initializes a new instance of the <see cref="UIManager"/> class.
    /// </summary>
    /// <param name="game">The XNA game instance.</param>
    /// <param name="inputService">The input service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="inputService"/> is <see langword="null"/>.
    /// </exception>
    public UIManager(Microsoft.Xna.Framework.Game game, IInputService inputService)
    {
      if (game == null)
        throw new ArgumentNullException("game");
      if (inputService == null)
        throw new ArgumentNullException("inputService");

      _sortedScreens = new List<UIScreen>();

      GameForm = PlatformHelper.GetForm(game.Window.Handle);

      game.Window.OrientationChanged += OnGameWindowOrientationChanged;

      InputService = inputService;
      KeyMap = KeyMap.AutoKeyMap;
      Screens = new UIScreenCollection(this);
    }
#else

    /// <summary>
    /// Initializes a new instance of the <see cref="UIManager"/> class.
    /// </summary>
    /// <param name="inputService">The input service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputService"/> is <see langword="null"/>.
    /// </exception>
    public UIManager(IInputService inputService)
    {
      if (inputService == null)
        throw new ArgumentNullException("inputService");

      _sortedScreens = new List<UIScreen>();

      InputService = inputService;
      KeyMap = KeyMap.AutoKeyMap;
      Screens = new UIScreenCollection(this);
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnGameWindowOrientationChanged(object sender, EventArgs eventArgs)
    {
      // Invalidate all screens.
      foreach (var screen in Screens)
        screen.InvalidateMeasure();
    }


    /// <summary>
    /// Updates the UI.
    /// </summary>
    /// <param name="deltaTime">The size of the current time step.</param>
    public void Update(TimeSpan deltaTime)
    {
      // Make sure _sortedScreens is up-to-date.
      if (Screens.IsDirty)
      {
        lock (((ICollection)Screens).SyncRoot)
        {
          // Reset flag.
          Screens.IsDirty = false;

          // Copy all entries from Screens and sort by DrawOrder (front-to-back).
          _sortedScreens.Clear();
          foreach (var screen in Screens)
            _sortedScreens.Add(screen);

          _sortedScreens.Sort(UIScreenComparer.Instance);
        }
      }

      // ----- Update screens. 
      // Screens are drawn back-to-front. (Screens with a low draw order are drawn first.)
      // The screens are updated front-to-back, that means that we update the screens in 
      // reverse draw order. The screen that is drawn last is on top and gets the first
      // chance to handle the input.
      // Since we do not iterate over the Screens collection, a screen may remove itself 
      // from the UIManager.
      foreach (var screen in _sortedScreens)
        screen.NewFrame();
      foreach (UIScreen screen in _sortedScreens)
        screen.Update(deltaTime);

      // ----- Update mouse cursor.

      // The cursor set in UIService.Cursor has top priority.
      if (GameForm != null)
      {
        var desiredCursor = Cursor;

        if (desiredCursor == null)
        {
          // Search screens and check if the control under the mouse wants a special cursor.
          foreach (var screen in _sortedScreens)
          {
            if (screen.IsEnabled && screen.IsVisible)
            {
              // Search for Cursor beginning at ControlUnderMouse up the control hierarchy.
              var control = screen.ControlUnderMouse;
              while (control != null)
              {
                if (control.Cursor != null)
                {
                  desiredCursor = control.Cursor;
                  break;
                }
                control = control.VisualParent;
              }
            }
          }
        }

        if (desiredCursor == null)
        {
          // Search for a default cursor in screens.
          foreach (var screen in _sortedScreens)
          {
            if (screen.Renderer != null && screen.IsEnabled && screen.IsVisible)
            {
              desiredCursor = screen.Renderer.GetCursor(null);
              if (desiredCursor != null)
                break;
            }
          }
        }

        // Set the desired cursor.
        PlatformHelper.SetCursor(GameForm, desiredCursor);
      }
    }
    #endregion
  }
}
