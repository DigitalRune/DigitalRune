// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents an item of <see cref="DropDownButton"/> control.
  /// </summary>
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
  ///   dropDown.Items.Add("DropDownItem " + i);
  /// 
  /// // Select the first item in the list.
  /// dropDown.SelectedIndex = 0;
  /// 
  /// // To show the drop down button, add it to an existing content control or panel.
  /// panel.Children.Add(dropDown);
  /// ]]>
  /// </code>
  /// Read the <see cref="DropDownButton.SelectedIndex"/> property to find out which item is 
  /// currently selected. You can also attach an event handler to this property to be notified when 
  /// the selection changes.
  /// <code lang="csharp">
  /// <![CDATA[
  /// GameProperty<int> selectedIndexProperty = dropDown.Properties.Get<int>("SelectedIndex");
  /// selectedIndexProperty.Changed += OnSelectionChanged;
  /// ]]>
  /// </code>
  /// </example>
  public class DropDownItem : ButtonBase
  {
    // This DropDownItem is like a ButtonBase - but with a different style.

    /// <summary>
    /// Initializes static members of the <see cref="DropDownItem"/> class.
    /// </summary>
    static DropDownItem()
    {
      // Per default, the item should be focused when the mouse moves over the item.
      OverrideDefaultValue(typeof(DropDownItem), FocusWhenMouseOverPropertyId, true);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DropDownItem"/> class.
    /// </summary>
    public DropDownItem()
    {
      Style = "DropDownItem";
    }
  }
}
