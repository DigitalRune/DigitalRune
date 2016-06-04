// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using DigitalRune.Collections;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents a drop down button. 
  /// </summary>
  /// <remarks>
  /// <para>
  /// The button displays a single item of a collection of <see cref="Items"/>. When the button is
  /// pressed, all <see cref="Items"/> are displayed in a <see cref="DropDown"/> that pops up and
  /// the user can select another item.
  /// </para>
  /// <para>
  /// The <see cref="Items"/> can be any objects; normally they are strings. To display the objects
  /// the method <see cref="CreateControlForItem"/> is called. Per default, the objects are 
  /// displayed with <see cref="TextBlock"/>s, but <see cref="CreateControlForItem"/> can be 
  /// changed to another method that creates other controls (e.g. <see cref="Image"/>s).
  /// </para>
  /// </remarks>
  /// <example>
  /// The following example creates a drop down button containing several items.
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Create a drop down button.
  /// var dropDown = new DropDownButton
  /// {
  ///   HorizontalAlignment = HorizontalAlignment.Stretch,
  ///   Margin = new Vector4F(4),
  ///   MaxDropDownHeight = 250,
  /// };
  /// 
  /// // Add a few random items.
  /// for (int i = 0; i < 20; i++)
  ///   dropDown.Items.Add("Item " + i);
  /// 
  /// // Select the first item in the list.
  /// dropDown.SelectedIndex = 0;
  /// 
  /// // To show the drop down button, add it to an existing content control or panel.
  /// panel.Children.Add(dropDown);
  /// ]]>
  /// </code>
  /// Read the <see cref="SelectedIndex"/> property to find out which item is currently selected. 
  /// You can also attach an event handler to this property to be notified when the selection 
  /// changes.
  /// <code lang="csharp">
  /// <![CDATA[
  /// GameProperty<int> selectedIndexProperty = dropDown.Properties.Get<int>("SelectedIndex");
  /// selectedIndexProperty.Changed += OnSelectionChanged;
  /// ]]>
  /// </code>
  /// </example>
  public class DropDownButton : ButtonBase
  {
    // When the button is clicked, a DropDown popup is opened.
    // Important DropDownButton must create new UIControl instances for all items. It is not 
    // possible that an item is a UIControl and this control is Content of the DropDownButton 
    // and simultaneously Content of the DropDown.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private DropDown _dropDown;
    private bool _wasOpened;  // true if the DropDown was opened in the last frame.
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <value>The items.</value>
    public NotifyingCollection<object> Items { get; private set; }


    /// <summary>
    /// Gets or sets the method that creates <see cref="UIControl"/>s for the <see cref="Items"/>.
    /// </summary>
    /// <value>
    /// The method that creates <see cref="UIControl"/>s for the <see cref="Items"/>. If this 
    /// property is <see langword="null"/>, <see cref="TextBlock"/>s are used to display the 
    /// <see cref="Items"/>. The default is <see langword="null"/>.
    /// </value>
    public Func<object, UIControl> CreateControlForItem { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="DropDownStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int DropDownStylePropertyId = CreateProperty(
      typeof(DropDownButton), "DropDownStyle", GamePropertyCategories.Style, null, "DropDown",
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the <see cref="DropDown"/>. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the <see cref="DropDown"/>. If the style is not a valid
    /// string, the <see cref="DropDown"/> is disabled.
    /// </value>
    public string DropDownStyle
    {
      get { return GetValue<string>(DropDownStylePropertyId); }
      set { SetValue(DropDownStylePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="SelectedIndex"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int SelectedIndexPropertyId = CreateProperty(
      typeof(DropDownButton), "SelectedIndex", GamePropertyCategories.Default, null, -1,
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
    /// The ID of the <see cref="MaxDropDownHeight"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int MaxDropDownHeightPropertyId = CreateProperty(
      typeof(DropDownButton), "MaxDropDownHeight", GamePropertyCategories.Default, null, 400.0f,
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the maximal height of the <see cref="DropDown"/> in pixels. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The maximal height of the <see cref="DropDown"/> in pixels.
    /// </value>
    public float MaxDropDownHeight
    {
      get { return GetValue<float>(MaxDropDownHeightPropertyId); }
      set { SetValue(MaxDropDownHeightPropertyId, value); }
    }

    /// <summary> 
    /// The ID of the <see cref="Title"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int TitlePropertyId = CreateProperty(
      typeof(DropDownButton), "Title", GamePropertyCategories.Common, null, "Unnamed",
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the title that is displayed in the <see cref="DropDown"/> (only on Windows 
    /// Phone 7). This is a game object property.
    /// </summary>
    /// <value>
    /// The title that is displayed in the <see cref="DropDown"/> (only on Windows Phone 7).
    /// </value>
    public string Title
    {
      get { return GetValue<string>(TitlePropertyId); }
      set { SetValue(TitlePropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------


    /// <summary>
    /// Initializes static members of the <see cref="DropDownButton"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static DropDownButton()
    {
      // Using ClickMode.Press per default.
      OverrideDefaultValue(typeof(DropDownButton), ClickModePropertyId, ClickMode.Press);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DropDownButton"/> class.
    /// </summary>
    public DropDownButton()
    {
      Style = "DropDownButton";

      Items = new NotifyingCollection<object>(false, true);
      Items.CollectionChanged += OnItemsChanged;

      // Connect OnSelectedIndexChanged() to SelectedIndex property.
      var selectedIndex = Properties.Get<int>(SelectedIndexPropertyId);
      selectedIndex.Changed += OnSelectedIndexChanged;

      // We have a DropDown. It is opened or closed, when this DropDownButton is clicked.
      var click = Events.Get<EventArgs>("Click");
      click.Event += (s, e) =>
      {
        if (_dropDown == null)
          return;

        // We must check the drop down state of the last frame. Because
        // it might have already been closed in this frame...
        if (!_wasOpened)
          _dropDown.Open();
        else
          _dropDown.Close();
      };
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnLoad()
    {
      base.OnLoad();

      var dropDownStyle = DropDownStyle;
      if (!string.IsNullOrEmpty(dropDownStyle))
      {
        _dropDown = new DropDown(this)
        {
          Style = dropDownStyle,
        };
      }
    }


    /// <inheritdoc/>
    protected override void OnUnload()
    {
      if (_dropDown != null)
      {
        _dropDown.Close();
        _dropDown = null;
      }

      base.OnUnload();
    }


    private void OnItemsChanged(object sender, CollectionChangedEventArgs<object> eventArgs)
    {
      OnSelectedIndexChanged(null, null);
    }


    private void OnSelectedIndexChanged(object sender, GamePropertyEventArgs<int> eventArgs)
    {
      // Set the content to the selected item. We must create a new control because we cannot
      // display an item in the DropDownButton and in the DropDown at the same time. (Only one
      // can be the visual parent.)
      if (0 <= SelectedIndex && SelectedIndex < Items.Count)
      {
        var item = Items[SelectedIndex];
        Content = CreateControl(item);
      }
      else
      {
        Content = null;
      }
    }


    /// <summary>
    /// Creates a new control for the item. 
    /// </summary>
    internal UIControl CreateControl(object item)
    {
      if (CreateControlForItem != null)
      {
        return CreateControlForItem(item);
      }

      return new TextBlock
      {
        Style = ContentStyle,
        Text = item.ToString(),
      };
    }


    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Remember the state of the DropDown for the next frame.
      _wasOpened = (_dropDown.VisualParent != null);

      base.OnUpdate(deltaTime);
    }
    #endregion
  }
}
