// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents a check box.
  /// </summary>
  /// <example>
  /// <para>
  /// The following example shows how to create a check box.
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// var checkBox = new CheckBox
  /// {
  ///   Margin = new Vector4F(4),
  ///   Content = new TextBlock { Text = "Enable feature XYZ" },
  ///   IsChecked = true
  /// };
  /// 
  /// // To show the check box, add it to an existing content control or panel.
  /// panel.Children.Add(checkBox);
  /// ]]>
  /// </code>
  /// <para>
  /// Read the <see cref="ToggleButton.IsChecked"/> property to determine the state of the check 
  /// box. You can also attach an event handler to this property to be notified when the state 
  /// changes. (The <see cref="GameProperty{T}.Changed"/> event only fires when the value 
  /// effectively changes.)
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// GameProperty<bool> isCheckedProperty = checkBox.Properties.Get<bool>("IsChecked");
  /// isCheckedProperty.Changed += OnCheckBoxChanged;
  /// ]]>
  /// </code>
  /// <para>
  /// Alternatively, you can also handle the <see cref="ButtonBase.Click"/> event. (Check boxes are
  /// derived from <see cref="ToggleButton"/>.)
  /// </para>
  /// </example>
  /// <seealso cref="RadioButton"/>
  public class CheckBox : ToggleButton
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CheckBox"/> class.
    /// </summary>
    public CheckBox()
    {
      Style = "CheckBox";
    }
      
  
    /// <inheritdoc/>
    protected override void OnToggle()
    {
      IsChecked = !IsChecked;
    }
  }
}
