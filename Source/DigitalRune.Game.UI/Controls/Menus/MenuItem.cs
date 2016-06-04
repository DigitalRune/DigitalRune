// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents an item of a menu (e.g. a <see cref="ContextMenu"/>).
  /// </summary>
  /// <remarks>
  /// A <see cref="MenuItem"/> is basically a button - but with a different style.
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
  public class MenuItem : ButtonBase
  {
    /// <summary>
    /// Initializes static members of the <see cref="MenuItem"/> class.
    /// </summary>
    static MenuItem()
    {
      // Per default, the item should be focused when the mouse moves over the item.
      OverrideDefaultValue(typeof(MenuItem), FocusWhenMouseOverPropertyId, true);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="MenuItem"/> class.
    /// </summary>
    public MenuItem()
    {
      Style = "MenuItem";
    }
  }
}
