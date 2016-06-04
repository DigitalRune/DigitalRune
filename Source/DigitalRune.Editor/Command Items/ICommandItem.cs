// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Defines an item that invokes an <see cref="ICommand"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A command item describes an command that can be triggered by a menu item, a toolbar item, or
    /// both. It defines the command, the appearance and the behavior in the UI.
    /// </para>
    /// <para>
    /// Command items must have a unique name. Command items of different extensions with the same
    /// name will be merged in menus or toolbars - if the item performs a unique action it should
    /// also have a unique name.
    /// </para>
    /// </remarks>
    public interface ICommandItem : INotifyPropertyChanged, INamedObject
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="Text"/> should always be shown.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="Text"/> should always be shown; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// In some cases the UI text is not shown by default: For example, when an item with an
        /// icon is used to create a toolbar button, only the icon is shown and text is hidden. When
        /// this property is set to <see langword="true"/>, the UI text is always shown.
        /// </remarks>
        bool AlwaysShowText { get; }


        /// <summary>
        /// Gets the command category.
        /// </summary>
        /// <value>The command category.</value>
        /// <remarks>
        /// The command category is used to group command items when shown in a list. Examples:
        /// "File", "Edit", "Format", "View", etc. The most common categories are defined in
        /// <see cref="CommandCategories"/>.
        /// </remarks>
        string Category { get; }


        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>The <see cref="ICommand"/>.</value>
        ICommand Command { get; }


        /// <summary>
        /// Gets the parameter passed to the command when it is executed.
        /// </summary>
        /// <value>The parameter passed to the command when it is executed.</value>
        object CommandParameter { get; }


        /// <summary>
        /// Gets the icon that represents this item.
        /// </summary>
        /// <value>The icon.</value>
        object Icon { get; }


        /// <summary>
        /// Gets the input gestures that trigger the <see cref="Command"/> of this item.
        /// </summary>
        /// <value>The input gestures.</value>
        InputGestureCollection InputGestures { get; }


        /// <summary>
        /// Gets a value indicating whether this item is checkable.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this item is checkable; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This is similar to <see cref="MenuItem.IsCheckable">MenuItem.IsCheckable</see>.
        /// </remarks>
        bool IsCheckable { get; }


        /// <summary>
        /// Gets a value indicating whether this item is checked.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this item is checked; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// If this value is <see langword="true"/>, then in the UI representation of this element
        /// it will be indicated that this item is activated. For example, a check mark is drawn.
        /// </remarks>
        bool IsChecked { get; }


        /// <summary>
        /// Gets the UI text.
        /// </summary>
        /// <value>The UI text.</value>
        /// <remarks>
        /// This is the text that is shown in the user interface (e.g. menu or toolbar). The text
        /// can contain an underscore in front of a key to define the access key.
        /// </remarks>
        string Text { get; }


        /// <summary>
        /// Gets the tool tip text that explains the purpose of this item.
        /// </summary>
        /// <value>The tool tip text.</value>
        string ToolTip { get; }


        /// <summary>
        /// Gets or sets a value indicating whether this command item is visible.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this command item is visible; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool IsVisible { get; set; }


        /// <summary>
        /// Creates a menu item view model for this command item.
        /// </summary>
        /// <returns>A view model that represents this item in a menu.</returns>
        MenuItemViewModel CreateMenuItem();


        /// <summary>
        /// Creates a toolbar item for this command item.
        /// </summary>
        /// <returns>A view model that represents this item in a toolbar.</returns>
        ToolBarItemViewModel CreateToolBarItem();
    }
}
