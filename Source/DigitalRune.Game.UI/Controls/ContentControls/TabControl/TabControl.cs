// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using DigitalRune.Collections;
using Microsoft.Xna.Framework.Input;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents a control that contains multiple items that share the same space on the screen. 
  /// </summary>
  /// <remarks>
  /// Tabs can be switched with the gamepad shoulder buttons or the mouse.
  /// </remarks>
  /// <example>
  /// The following example creates a tab controls containing 3 tab items:
  /// <code lang="csharp">
  /// <![CDATA[
  /// var tabControl = new TabControl
  /// {
  ///   HorizontalAlignment = HorizontalAlignment.Stretch,
  ///   Margin = new Vector4F(4)
  /// };
  /// 
  /// // Add 3 pages to to the tab control.
  /// var tabItem0 = new TabItem
  /// {
  ///   TabPage = new TextBlock { Margin = new Vector4F(4), Text = "Page 0" },
  ///   Content = new TextBlock { Text = "Content of page 0" }
  /// };
  /// var tabItem1 = new TabItem
  /// {
  ///   TabPage = new TextBlock { Margin = new Vector4F(4), Text = "Page 1" },
  ///   Content = new TextBlock { Text = "Content of page 1" }
  /// };
  /// var tabItem2 = new TabItem
  /// {
  ///   TabPage = new TextBlock { Margin = new Vector4F(4), Text = "Page 2" },
  ///   Content = new TextBlock { Text = "Content of page 2" }
  /// };
  /// tabControl.Items.Add(tabItem0);
  /// tabControl.Items.Add(tabItem1);
  /// tabControl.Items.Add(tabItem2);
  /// 
  /// // Select the second page.
  /// tabControl.SelectedIndex = 1;
  /// 
  /// // To show the tab control, add it to an existing content control or panel.
  /// panel.Children.Add(tabControl);
  /// ]]>
  /// </code>
  /// </example>
  public class TabControl : ContentControl
  {
    // Contains a panel with TabItems. In the content area the TabItem.TabPage of 
    // the selected item is displayed.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private StackPanel _itemsPanel;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <value>The items.</value>
    public NotifyingCollection<TabItem> Items { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="SelectedIndex"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int SelectedIndexPropertyId = CreateProperty(
      typeof(TabControl), "SelectedIndex", GamePropertyCategories.Default, null, -1, 
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the index of the selected item. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The index of the selected item; or -1 if no item is selected. 
    /// </value>
    public int SelectedIndex
    {
      get { return GetValue<int>(SelectedIndexPropertyId); }
      set { SetValue(SelectedIndexPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="TabItemPanelStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int TabItemPanelStylePropertyId = CreateProperty(
      typeof(TabControl), "TabItemPanelStyle", GamePropertyCategories.Style, null, "TabItemPanel", 
      UIPropertyOptions.None);
    
    /// <summary>
    /// Gets or sets the style that is used for the panel that displays the selected item. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is used for the panel that displays the selected item. If this property is 
    /// <see langword="null"/> or an empty string the tab items are hidden.
    /// </value>
    public string TabItemPanelStyle
    {
      get { return GetValue<string>(TabItemPanelStylePropertyId); }
      set { SetValue(TabItemPanelStylePropertyId, value); }
    }
    #endregion
    

    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TabControl"/> class.
    /// </summary>
    public TabControl()
    {
      Style = "TabControl";

      Items = new NotifyingCollection<TabItem>(false, false);
      Items.CollectionChanged += OnItemsChanged;

      // When selected index changes, call Select. The user might have changed SelectedIndex 
      // directly without using Select().
      var selectedIndex = Properties.Get<int>(SelectedIndexPropertyId);
      selectedIndex.Changed += (s, e) => Select(e.NewValue);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnLoad()
    {
      base.OnLoad();

      var tabItemPanelStyle = TabItemPanelStyle;
      if (!string.IsNullOrEmpty(tabItemPanelStyle))
      {
        _itemsPanel = new StackPanel
        {
          Style = tabItemPanelStyle,
        };

        foreach (var item in Items)
          _itemsPanel.Children.Add(item);

        VisualChildren.Add(_itemsPanel);
      }
    }


    /// <inheritdoc/>
    protected override void OnUnload()
    {
      VisualChildren.Remove(_itemsPanel);
      _itemsPanel.Children.Clear();
      _itemsPanel = null;

      base.OnUnload();
    }


    private void OnItemsChanged(object sender, CollectionChangedEventArgs<TabItem> eventArgs)
    {
      // TabItems are added to the panel. We must make sure the same order is used 
      // in Items and in the panel. 
      // TabItem.TabPanel is set in this method.

      // ----- Moving items.
      if (eventArgs.Action == CollectionChangedAction.Move)
      {
        if (_itemsPanel != null)
          _itemsPanel.Children.Move(eventArgs.OldItemsIndex, eventArgs.NewItemsIndex);

        return;
      }

      // ----- Removing items.
      foreach (var oldItem in eventArgs.OldItems)
      {
        oldItem.IsSelected = false;
        oldItem.TabControl = null;
        if (_itemsPanel != null)
          _itemsPanel.Children.Remove(oldItem);
      }

      // ----- Add items.
      int newItemsIndex = eventArgs.NewItemsIndex;
      foreach (var newItem in eventArgs.NewItems)
      {
        // Clear IsSelected flag of the new item.
        newItem.IsSelected = false;

        newItem.TabControl = this;

        // Insert new item at correct position!
        if (newItemsIndex == -1)
        {
          if (_itemsPanel != null)
            _itemsPanel.Children.Add(newItem);
        }
        else
        {
          if (_itemsPanel != null)
            _itemsPanel.Children.Insert(newItemsIndex, newItem);

          newItemsIndex++;
        }
      }

      // Update SelectedIndex.
      for (int i = 0; i < Items.Count; i++)
      {
        var item = Items[i];
        if (item.IsSelected)
        {
          SelectedIndex = i;
          return;
        }
      }

      // Nothing selected. Select first entry.
      if (Items.Count > 0)
        Select(0);
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

#if !SILVERLIGHT
      // Gamepad shoulder buttons switches tab items.
      if (!inputService.IsGamePadHandled(context.AllowedPlayer))
      {
        if (inputService.IsPressed(Buttons.RightShoulder, true, context.AllowedPlayer))
        {
          inputService.SetGamePadHandled(context.AllowedPlayer, true);

          // Select next item.
          if (SelectedIndex < Items.Count - 1)
            Select(SelectedIndex + 1);

          // If focus was in the old item, then the focus moves to the new item.
          if (!IsFocusWithin)
            screen.FocusManager.Focus(this);
        }
        else if (inputService.IsPressed(Buttons.LeftShoulder, true, context.AllowedPlayer))
        {
          inputService.SetGamePadHandled(context.AllowedPlayer, true);

          // Select previous item.
          if (SelectedIndex > 0)
            Select(SelectedIndex - 1);

          // If focus was in the old item, then the focus moves to the new item.
          if (!IsFocusWithin)
            screen.FocusManager.Focus(this);
        }
      }
#endif
    }


    /// <overloads>
    /// <summary>
    /// Selects a <see cref="TabItem"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Selects a <see cref="TabItem"/> by index.
    /// </summary>
    /// <param name="index">The index of the item in the <see cref="Items"/> collection.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is out of range.
    /// </exception>
    public void Select(int index)
    {
      if (index < 0 || index >= Items.Count)
        throw new ArgumentOutOfRangeException("index");

      Select(Items[index]);
    }


    /// <summary>
    /// Selects the specified <see cref="TabItem"/>.
    /// </summary>
    /// <param name="tabItem">
    /// The <see cref="TabItem"/> (must be one of the <see cref="Items"/>).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="tabItem"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="tabItem"/> is not an item of this <see cref="TabControl"/>.
    /// </exception>
    public void Select(TabItem tabItem)
    {
      if (tabItem == null)
        throw new ArgumentNullException("tabItem");
      if (tabItem.TabControl != this)
        throw new ArgumentException("TabItem cannot be selected. It is not an item of this TabControl");

      if (tabItem.IsSelected)
        return;

      // Unselect all other items.
      int newSelectedIndex = 0;
      for (int i = 0; i < Items.Count; i++)
      {
        var item = Items[i];
        if (item != tabItem)
          item.IsSelected = false;
        else
          newSelectedIndex = i;
      }

      // Select new item.
      tabItem.IsSelected = true;
      SelectedIndex = newSelectedIndex;

      // Set Content to tab page of selected item.
      // If the focus was within the tab page, then move focus to the new tab page.
      bool wasFocused = IsFocusWithin;
      Content = tabItem.TabPage;
      if (wasFocused && Screen != null)
        Screen.FocusManager.Focus(Content);
    }


    /// <summary>
    /// Updates the content of the <see cref="TabControl"/>.
    /// </summary>
    internal void UpdateContent()
    {
      int index = SelectedIndex;
      if (0 <= index && index < Items.Count)
      {
        var tabItem = Items[index];
        Content = tabItem.TabPage;
        InvalidateMeasure();
      }
    }
    #endregion
  }
}
