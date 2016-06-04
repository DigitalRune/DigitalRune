// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using DigitalRune.Collections;
using DigitalRune.Game.Input;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Input;
#if WP7 || PORTABLE
using Microsoft.Xna.Framework.Input.Touch;
#endif
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents a popup menu that enables a control to expose functionality that is specific to 
  /// the context of the control. 
  /// </summary>
  /// <remarks>
  /// The <see cref="ContentControl.Content"/> of the <see cref="ContextMenu"/> is a 
  /// <see cref="StackPanel"/> containing the menu items. The <see cref="ContentControl.ContentStyle"/> 
  /// defines the appearance of the panel.
  /// </remarks>
  /// <example>
  /// The following example creates a multi-line text box with a context menu.
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Create a multi-line text box.
  /// var textBox = new TextBox
  /// {
  ///   Margin = new Vector4F(4),
  ///   Text = "Lorem ipsum dolor sit ...",
  ///   MaxLines = 5,   // Show max 5 lines of text.
  ///   HorizontalAlignment = HorizontalAlignment.Stretch,
  /// };
  /// 
  /// // Add a context menu (Cut, Copy, Paste) to the text box.
  /// var contextMenu = new ContextMenu();
  /// var cut = new MenuItem { Content = new TextBlock { Text = "Cut" } };
  /// var copy = new MenuItem { Content = new TextBlock { Text = "Copy" } };
  /// var paste = new MenuItem { Content = new TextBlock { Text = "Paste" } };
  /// cut.Click += (s, e) => textBox.Cut();
  /// copy.Click += (s, e) => textBox.Copy();
  /// paste.Click += (s, e) => textBox.Paste();
  /// contextMenu.Items.Add(cut);
  /// contextMenu.Items.Add(copy);
  /// contextMenu.Items.Add(paste);
  /// textBox.ContextMenu = contextMenu;
  /// 
  /// // To show the text box, add it to an existing content control or panel.
  /// panel.Children.Add(textBox);
  /// ]]>
  /// </code>
  /// </example>
  public class ContextMenu : ContentControl
  {
    // The context menu is a scroll viewer with a stack panel filled with menu items.
    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private StackPanel _panel;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="UIControl"/> that opened this <see cref="ContextMenu"/>.
    /// </summary>
    /// <value>
    /// The <see cref="UIControl"/> that opened this <see cref="ContextMenu"/>.
    /// </value>
    public UIControl Owner { get; private set; }   // This control will get focus back when menu closes.


    /// <summary>
    /// Gets the menu items.
    /// </summary>
    /// <value>
    /// The menu items. These can be any <see cref="UIControl"/> but usually <see cref="MenuItem"/>s
    /// or clickable controls, like buttons, should be used.
    /// </value>
    public NotifyingCollection<UIControl> Items { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="Offset"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int OffsetPropertyId = CreateProperty(
      typeof(ContextMenu), "Offset", GamePropertyCategories.Layout, null, 0.0f, 
      UIPropertyOptions.None);
    
    /// <summary>
    /// Gets or sets the offset relative to the opening position. 
    /// This is a game object property.
    /// </summary>
    /// <value>The offset to the opening position.</value>
    public float Offset
    {
      get { return GetValue<float>(OffsetPropertyId); }
      set { SetValue(OffsetPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IsOpen"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsOpenPropertyId = CreateProperty(
      typeof(ContextMenu), "IsOpen", GamePropertyCategories.Default, null, false, 
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets a value indicating whether this context menu is currently is visible. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this context menu is currently visible; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsOpen
    {
      get { return GetValue<bool>(IsOpenPropertyId); }
      private set { SetValue(IsOpenPropertyId, value); }
    }
    #endregion
    

    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="ContextMenu"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static ContextMenu()
    {
      // TODO: Check whether this is needed?
      OverrideDefaultValue(typeof(ContextMenu), IsFocusScopePropertyId, true);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ContextMenu"/> class.
    /// </summary>
    public ContextMenu()
    {
      Style = "ContextMenu";

      Items = new NotifyingCollection<UIControl>(false, false);
      Items.CollectionChanged += OnItemsChanged;

#if WP7 || PORTABLE
      TouchPanel.EnabledGestures |= GestureType.Hold;
#endif
    }    
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnLoad()
    {
      base.OnLoad();

      // Create panel with items.
      if (_panel == null)
      {
        var contentStyle = ContentStyle;
        if (!string.IsNullOrEmpty(contentStyle))
        {
          _panel = new StackPanel
          {
            Style = contentStyle
          };

          foreach (var item in Items)
            _panel.Children.Add(item);

          Content = _panel;
        }
      }
    }


    private void OnItemsChanged(object sender, CollectionChangedEventArgs<UIControl> eventArgs)
    {
      // Items are put into the stack panel. We must make sure they have the same order in
      // both collections.     
      // For new items that are buttons, the Click event needs to be handled because clicking 
      // closes the context menu.
      
      // ----- Moving items.
      if (eventArgs.Action == CollectionChangedAction.Move)
      {
        if (_panel != null)
          _panel.Children.Move(eventArgs.OldItemsIndex, eventArgs.NewItemsIndex);

        return;
      }

      // ----- Removing items.
      foreach (var item in eventArgs.OldItems)
      {
        if (_panel != null)
          _panel.Children.Remove(item);

        var button = item as ButtonBase;
        if (button != null)
          button.Click -= OnMenuItemClick;
      }

      // ----- Adding new items.
      int newItemsIndex = eventArgs.NewItemsIndex;
      foreach (var item in eventArgs.NewItems)
      {
        if (newItemsIndex == -1)
        {
          // Append items.
          if (_panel != null)
            _panel.Children.Add(item);
        }
        else
        {
          // Insert items at correct position.
          if (_panel != null)
            _panel.Children.Insert(newItemsIndex, item);

          newItemsIndex++;
        }

        var button = item as ButtonBase;
        if (button != null)
          button.Click += OnMenuItemClick;
      }
      
      InvalidateMeasure();
    }


    private void OnMenuItemClick(object sender, EventArgs eventArgs)
    {
      Close();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnHandleInput(InputContext context)
    {
      base.OnHandleInput(context);

      if (!IsLoaded)
        return;

      var screen = Screen;
      var inputService = InputService;

      // ESC --> Close. 
      // Note: We do not check the InputService.IsHandled flags because the popup closes 
      // when ESC is pressed - even if the ESC was caught by another game component.
      if (inputService.IsDown(Keys.Escape))
      {
        inputService.IsKeyboardHandled = true;
        Close();
      }

#if !SILVERLIGHT
      // Same for BACK or B on gamepad.
      if (inputService.IsPressed(Buttons.Back, false, context.AllowedPlayer) 
          || inputService.IsPressed(Buttons.B, false, context.AllowedPlayer))
      {
        inputService.SetGamePadHandled(context.AllowedPlayer, true);
        Close();
      }
#endif

      // If another control is opened above this popup, then this popup closes.
      // Exception: Tooltips are okay above the popup.
      if (screen.Children[screen.Children.Count - 1] != this)
      {
        if (screen.Children[screen.Children.Count - 1] != screen.ToolTipManager.ToolTipControl
            || screen.Children[screen.Children.Count - 2] != this)
        {
          Close();
        }
      }

      // If mouse is pressed somewhere else or if GamePad.A is pressed, we close the context menu.
      if (!IsMouseOver  // If mouse is pressed over context menu, we still have to wait for MouseUp.
          && (inputService.IsPressed(MouseButtons.Left, false)
              || inputService.IsPressed(MouseButtons.Right, false)))
      {
        Close();
      }

      // Like a normal window: mouse does not act through this popup.
      if (IsMouseOver)
        inputService.IsMouseOrTouchHandled = true;
    }


    /// <summary>
    /// Opens this <see cref="ContextMenu"/> (adds it to the <see cref="UIScreen"/>).
    /// </summary>
    /// <param name="owner">The control that opened this context menu.</param>
    /// <param name="position">
    /// The position of the mouse cursor - or where the context menu should be opened.
    /// (<see cref="Offset"/> will be applied to this position.)
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="owner"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="owner"/> is not loaded. The owner needs to be a visible control.
    /// </exception>
    public void Open(UIControl owner, Vector2F position)
    {
      if (owner == null)
        throw new ArgumentNullException("owner");

      var screen = owner.Screen;
      if (screen == null)
        throw new ArgumentException("Invalid owner. The owner must be loaded.", "owner");

      if (!IsEnabled)
        return;

      // Close if already opened.
      Close();

      Owner = owner;

      // Make visible and add to screen.
      IsVisible = true;
      screen.Children.Add(this);

      // Choose position near the given position. 
      // The context menu is positioned so that it fits onto the screen.
      Measure(new Vector2F(float.PositiveInfinity));
      float x = position.X;
      if (x + DesiredWidth > screen.ActualWidth)
      {
        // Show context menu on the left.
        x = x - DesiredWidth;
      }

      float offset = Offset;
      float y = position.Y + offset;
      if (y + DesiredHeight > screen.ActualHeight)
      {
        // Show context menu on top.
        // (We use 0.5 * offset if context menu is above cursor. This assumes that the cursor 
        // is similar to the typical arrow where the hot spot is at the top. If the cursor is a 
        // different shape we might need to adjust the offset.)
        y = y - DesiredHeight - 1.5f * offset;
      }

      X = x;
      Y = y;

#if WP7 || PORTABLE
#if PORTABLE
      if (GlobalSettings.PlatformID == PlatformID.WindowsPhone8)
#endif
      {
        // Imitate position of Silverlight WP7 context menus.
        if (screen.ActualHeight >= screen.ActualWidth)
        {
          // Portrait mode.
          X = 0;
          HorizontalAlignment = HorizontalAlignment.Stretch;
          VerticalAlignment = VerticalAlignment.Top;
        }
        else
        {
          // Landscape mode.
          Y = 0;
          Width = screen.ActualWidth / 2;
          HorizontalAlignment = HorizontalAlignment.Left;
          VerticalAlignment = VerticalAlignment.Stretch;
          if (position.X <= screen.ActualWidth / 2)
          {
            // Show context menu on right half of the screen.
            X = screen.ActualWidth / 2;
          }
          else
          {
            // Show context menu on the left half of the screen.
            X = 0;
          }
        }
      }
#endif

      screen.FocusManager.Focus(this);
      IsOpen = true;
    }


    /// <summary>
    /// Closes this <see cref="ContextMenu"/> (removes it from the <see cref="UIScreen"/>).
    /// </summary>
    public void Close()
    {
      var screen = Screen;
      if (screen != null && screen.IsLoaded)
      {
        if (!(Owner is UIScreen) && IsFocusWithin)
        {
          // Move focus back to owner.
          screen.FocusManager.Focus(Owner);
        }

        screen.Children.Remove(this);
      }

      IsOpen = false;
      Owner = null;
    }


#if WP7 || PORTABLE
    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan deltaTime)
    {
#if PORTABLE
      if (GlobalSettings.PlatformID == PlatformID.WindowsPhone8)
#endif
      {
        // Close if orientation of Screen has changed. (Default behavior of Silverlight context menus.)
        bool isScreenPortrait = (Screen.ActualHeight >= Screen.ActualWidth);
        bool isMenuPortrait = (HorizontalAlignment == HorizontalAlignment.Stretch);
        bool isMenuLandscape = (VerticalAlignment == VerticalAlignment.Stretch);
        if (isMenuPortrait && !isScreenPortrait || isMenuLandscape && isScreenPortrait)
        {
          Close();
        }
      }

      base.OnUpdate(deltaTime);
    }
#endif
    #endregion
  }
}
