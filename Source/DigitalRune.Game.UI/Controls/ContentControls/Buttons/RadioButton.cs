// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Enables the user to select a single option from a of choices when paired with other 
  /// <see cref="RadioButton"/> controls. 
  /// </summary>
  /// <remarks>
  /// Radio buttons can be grouped into different groups. Radio buttons belong together if they have
  /// the same parent and the same <see cref="GroupName"/>.
  /// </remarks>
  /// <example>
  /// The following examples creates two groups of radio buttons. The radio buttons are surrounded
  /// by group boxes.
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Group 123
  /// var radioButton1 = new RadioButton
  /// {
  ///   Margin = new Vector4F(4, 8, 4, 4),
  ///   Content = new TextBlock { Text = "Option 1" },
  ///   IsChecked = true,
  ///   GroupName = "Group 123"
  /// };
  /// 
  /// var radioButton2 = new RadioButton
  /// {
  ///   Margin = new Vector4F(4, 2, 4, 2),
  ///   Content = new TextBlock { Text = "Option 2" },
  ///   GroupName = "Group 123"
  /// };
  /// 
  /// var radioButton3 = new RadioButton
  /// {
  ///   Margin = new Vector4F(4, 2, 4, 4),
  ///   Content = new TextBlock { Text = "Option 3" },
  ///   GroupName = "Group 123"
  /// };
  /// 
  /// var stackPanel = new StackPanel();
  /// stackPanel.Children.Add(radioButton1);
  /// stackPanel.Children.Add(radioButton2);
  /// stackPanel.Children.Add(radioButton3);
  /// 
  /// var groupBox123 = new GroupBox
  /// {
  ///   Title = "Options 1, 2, 3",
  ///   HorizontalAlignment = HorizontalAlignment.Stretch,
  ///   Margin = new Vector4F(4),
  ///   Content = stackPanel
  /// };
  /// 
  /// // Group ABC
  /// var radioButtonA = new RadioButton
  /// {
  ///   Margin = new Vector4F(4, 8, 4, 4),
  ///   Content = new TextBlock { Text = "Option A" },
  ///   IsChecked = true,
  ///   GroupName = "Group ABC"
  /// };
  /// 
  /// var radioButtonB = new RadioButton
  /// {
  ///   Margin = new Vector4F(4, 2, 4, 2),
  ///   Content = new TextBlock { Text = "Option B" },
  ///   GroupName = "Group ABC"
  /// };
  /// 
  /// var radioButtonC = new RadioButton
  /// {
  ///   Margin = new Vector4F(4, 2, 4, 4),
  ///   Content = new TextBlock { Text = "Option C" },
  ///   GroupName = "Group ABC"
  /// };
  /// 
  /// stackPanel = new StackPanel();
  /// stackPanel.Children.Add(radioButtonA);
  /// stackPanel.Children.Add(radioButtonB);
  /// stackPanel.Children.Add(radioButtonC);
  /// 
  /// var groupBoxABC = new GroupBox
  /// {
  ///   Title = "Options A, B, C",
  ///   HorizontalAlignment = HorizontalAlignment.Stretch,
  ///   Margin = new Vector4F(4),
  ///   Content = stackPanel
  /// };
  /// 
  /// // To show the groups, add them to an existing panel.
  /// panel.Children.Add(groupBox123);
  /// panel.Children.Add(groupBoxABC);
  /// ]]>
  /// </code>
  /// </example>
  /// <seealso cref="CheckBox"/>
  public class RadioButton : ToggleButton
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the group.
    /// </summary>
    /// <value>The name of the group.</value>
    public string GroupName { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioButton"/> class.
    /// </summary>
    public RadioButton()
    {
      Style = "RadioButton";
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnToggle()
    {
      if (IsChecked)
      {
        // A checked radio button is clicked again --> nothing to do.
        return;
      }

      // Uncheck all other radio buttons of the parent and the same Group.
      var parent = VisualParent;
      if (parent != null)
      {
        foreach (var child in parent.VisualChildren)
        {
          if (child != this)
          {
            RadioButton radioButton = child as RadioButton;
            if (radioButton != null && radioButton.GroupName == GroupName)
            {
              radioButton.IsChecked = false;
            }
          }
        }
      }

      // Check this radio button.
      IsChecked = true;
    }
    #endregion
  }
}
