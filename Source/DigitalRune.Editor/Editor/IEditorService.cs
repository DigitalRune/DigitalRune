// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Xml.Linq;
using DigitalRune.Collections;
using DigitalRune.CommandLine;
using DigitalRune.Editor.Commands;
using DigitalRune.ServiceLocation;
using DigitalRune.Windows.Controls;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents the editor.
    /// </summary>
    public interface IEditorService : IDockControl, IConductor, IScreen, IActivatable, IGuardClose, IDisplayName
    {
        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application. The default value is the application name that is defined
        /// in the assembly information of the entry assembly.
        /// </value>
        /// <remarks>
        /// The name of the application can be specified in the constructor of
        /// <see cref="EditorViewModel"/>. If no name is specified when calling the constructor, the
        /// title of the executing assembly is used.
        /// </remarks>
        string ApplicationName { get; set; }


        /// <summary>
        /// Gets or sets the icon ( <see cref="ImageSource"/> or <see cref="MultiColorGlyph"/>) of
        /// the application.
        /// </summary>
        /// <value>The image. Can be <see langword="null"/>.</value>
        object ApplicationIcon { get; set; }


        /// <summary>
        /// Gets or sets the sub-title, which is shown in the window title before the application
        /// name.
        /// </summary>
        /// <value>
        /// The sub-title, which is shown in the window title before the application name.
        /// </value>
        string Subtitle { get; set; }


        /// <summary>
        /// Gets the exit code of the application.
        /// </summary>
        /// <value>The exit code of the application.</value>
        int ExitCode { get; }


        /// <summary>
        /// Gets the service container that provides access to all available services.
        /// </summary>
        /// <value>The service container that provides access to all available services.</value>
        ServiceContainer Services { get; }


        /// <summary>
        /// Gets the registered extensions.
        /// </summary>
        /// <value>The registered extensions.</value>
        /// <remarks>
        /// All required extensions must be added to this list before
        /// <see cref="EditorViewModel.Startup"/> is called.
        /// </remarks>
        EditorExtensionCollection Extensions { get; }


        /// <summary>
        /// Gets a list of all left-aligned status bar items.
        /// </summary>
        IList<object> StatusBarItemsLeft { get; }


        /// <summary>
        /// Gets a list of all centered status bar items.
        /// </summary>
        IList<object> StatusBarItemsCenter { get; }


        /// <summary>
        /// Gets a list of all right-aligned status bar items.
        /// </summary>
        IList<object> StatusBarItemsRight { get; }


        /// <summary>
        /// Gets a list of all right-aligned caption bar items.
        /// </summary>
        IList<object> CaptionBarItemsRight { get; }


        /// <summary>
        /// Gets the window.
        /// </summary>
        /// <value>The window.</value>
        EditorWindow Window { get; }


        /// <summary>
        /// Occurs when the editor window becomes the foreground window.
        /// </summary>
        event EventHandler<EventArgs> WindowActivated;


        /// <summary>
        /// Initiates the shutdown of the application.
        /// </summary>
        /// <param name="exitCode">
        /// An integer exit code for the application. The default exit code is 0.
        /// </param>
        /// <remarks>
        /// Note that it is not guaranteed that the application exits immediately. In certain cases,
        /// e.g. the editor contains unsaved documents, the user may prevent the application from
        /// closing.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Exit")]
        void Exit(int exitCode = (int)Editor.ExitCode.ERROR_SUCCESS);


        /// <summary>
        /// Gets a value indicating whether the shutdown of the application has been initiated.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the application is shutting down; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        bool IsShuttingDown { get; }


        /// <summary>
        /// Gets the collections of command item nodes that define the main menu of the editor.
        /// </summary>
        /// <value>
        /// The collections of command item nodes that define the main menu of the editor.
        /// </value>
        /// <remarks>
        /// <para>
        /// Editor extensions can insert menu items in the menu of the editor by adding a collection
        /// of command item nodes. A command item node is a <see cref="MergeableNode{T}"/> that
        /// contains an <see cref="ICommandItem"/>. The command item defines the UI control and
        /// associated actions. The command item node defines the point where the command item
        /// should be inserted in the menu of the editor.
        /// </para>
        /// <para>
        /// When creating the main window of the editor, the editor merges all collections of
        /// command item nodes and builds the menu from the result of the merge operation.
        /// </para>
        /// <para>
        /// <see cref="InvalidateUI"/> can be called to update the menu structure at runtime.
        /// <see cref="InvalidateUI"/> will raise the <see cref="UIInvalidated"/> event and causes
        /// the editor to rebuilt menus and toolbars.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        ICollection<MergeableNodeCollection<ICommandItem>> MenuNodeCollections { get; }


        /// <summary>
        /// Gets the collections of command item nodes that define the toolbars of the editor.
        /// </summary>
        /// <value>
        /// The collections of command item nodes that define the toolbars of the editor.
        /// </value>
        /// <remarks>
        /// <para>
        /// Editor extensions can insert toolbars and toolbar items in the toolbar tray of the
        /// editor by adding a collection of command item nodes. A command item node is a
        /// <see cref="MergeableNode{T}"/> that contains an <see cref="ICommandItem"/>. The command
        /// item defines the UI control and associated actions. The command item node defines the
        /// point where the command item should be inserted in the toolbar tray of the editor.
        /// </para>
        /// <para>
        /// When creating the main window of the application, the editor merges all collections of
        /// command item nodes and builds the toolbars from the result of the merge operation.
        /// </para>
        /// <para>
        /// <see cref="InvalidateUI"/> can be called to update the toolbar structure at runtime.
        /// <see cref="InvalidateUI"/> will raise the <see cref="UIInvalidated"/> event and causes
        /// the editor to rebuilt menus and toolbars.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        ICollection<MergeableNodeCollection<ICommandItem>> ToolBarNodeCollections { get; }


        /// <summary>
        /// Gets the collections of command item nodes that define the context menu of a
        /// <see cref="IDockTabItem"/>.
        /// </summary>
        /// <value>
        /// The collections of command item nodes that define the context menu of a
        /// <see cref="IDockTabItem"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// Editor extensions can insert menu items in the context menu of a
        /// <see cref="IDockTabItem"/> by adding a collection of command item nodes. A command item
        /// node is a <see cref="MergeableNode{T}"/> that contains an <see cref="ICommandItem"/>.
        /// The command item defines the UI control and associated actions. The command item node
        /// defines the point where the command item should be inserted in the context menu.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        ICollection<MergeableNodeCollection<ICommandItem>> DockContextMenuNodeCollections { get; }


        /// <summary>
        /// Gets the menu items.
        /// </summary>
        /// <value>The menu items.</value>
        /// <remarks>
        /// <para>
        /// This collection is built from the command item nodes in the 
        /// <see cref="MenuNodeCollections"/>. To update the menu call <see cref="InvalidateUI"/>.
        /// </para>
        /// <para>
        /// This collection is created once. <see cref="InvalidateUI"/> changes only the content.
        /// </para>
        /// </remarks>
        MenuItemViewModelCollection Menu { get; }


        /// <summary>
        /// Gets the toolbars.
        /// </summary>
        /// <value>The toolbars.</value>
        /// <remarks>
        /// <para>
        /// The menu items are created automatically from the <see cref="ToolBarNodeCollections"/>.
        /// This collection should not be modified.
        /// </para>
        /// <para>
        /// This collection is created once. <see cref="InvalidateUI"/> changes only the content.
        /// </para>
        /// </remarks>
        ToolBarViewModelCollection ToolBars { get; }


        /// <summary>
        /// Gets the default context menu of a <see cref="IDockTabItem"/>.
        /// </summary>
        /// <value>The default context menu of a <see cref="IDockTabItem"/>.</value>
        /// <remarks>
        /// <para>
        /// This context menu is built from the command item nodes in the
        /// <see cref="DockContextMenuNodeCollections"/>. To update the context menu call
        /// <see cref="InvalidateUI"/>.
        /// </para>
        /// <para>
        /// This collection is created once. <see cref="InvalidateUI"/> changes only the content.
        /// </para>
        /// </remarks>
        MenuItemViewModelCollection DockContextMenu { get; }


        /// <summary>
        /// Gets the toolbar context menu.
        /// </summary>
        /// <value>The toolbar context menu.</value>
        /// <remarks>
        /// If the <see cref="CommandExtension"/> is used, this context menu is controlled by the 
        /// commands extension and cannot be customized.
        /// </remarks>
        MenuItemViewModelCollection ToolBarContextMenu { get; }


        /// <summary>
        /// Occurs when the command item collections that define menus, toolbars, context menus,
        /// etc. changed.
        /// </summary>
        /// <remarks>
        /// The event can be raised by calling <see cref="InvalidateUI"/>. Editor extensions that
        /// dynamically create menus, toolbars, context menus, or similar based on command item
        /// nodes (see <see cref="MenuNodeCollections"/>, <see cref="ToolBarNodeCollections"/>,
        /// <see cref="DockContextMenuNodeCollections"/>) should rebuild the controls if this event
        /// is raised.
        /// </remarks>
        /// <seealso cref="InvalidateUI"/>
        event EventHandler<EventArgs> UIInvalidated;


        /// <summary>
        /// Notifies the editor that the command item nodes have changed and raises the
        /// <see cref="UIInvalidated"/> event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The menus, toolbars, context menus, etc. of the application are assembled from
        /// collections of command item nodes. See <see cref="MenuNodeCollections"/>,
        /// <see cref="ToolBarNodeCollections"/>, and <see cref="DockContextMenuNodeCollections"/>.
        /// When these collections are modified the menus, toolbars, or context menus need to be
        /// rebuilt. The method <see cref="InvalidateUI"/> raises the <see cref="UIInvalidated"/>
        /// event. All editor extensions that create controls based on command item nodes should
        /// listen for this event and rebuild the controls when the event is raised.
        /// </para>
        /// <para>
        /// Editor extensions that modify command item nodes at runtime should call
        /// <see cref="InvalidateUI"/> to signal that controls need to be updated.
        /// </para>
        /// </remarks>
        /// <see cref="UIInvalidated"/>
        void InvalidateUI();


        /// <summary>
        /// Gets the command line parser.
        /// </summary>
        /// <value>The command line parser.</value>
        /// <remarks>
        /// Extensions can add custom command line arguments. This must be done in
        /// <see cref="EditorExtension.OnInitialize"/>. The command line arguments are automatically
        /// parsed at the end of <see cref="EditorViewModel.Initialize"/>. The results are available
        /// in <see cref="CommandLineResult"/> and can be used in
        /// <see cref="EditorExtension.OnStartup"/>.
        /// </remarks>
        CommandLineParser CommandLineParser { get; }


        /// <summary>
        /// Gets the command line parse result.
        /// </summary>
        /// <value>The command line parse result.</value>
        ParseResult CommandLineResult { get; }


        /// <summary>
        /// Occurs when the selected item or the hierarchy of the docking layout changed.
        /// </summary>
        /// <remarks>
        /// Resizing of panes or floating windows do not trigger the event.
        /// </remarks>
        event EventHandler<EventArgs> LayoutChanged;


        /// <summary>
        /// Occurs when the <see cref="IDockControl.ActiveDockTabItem"/> was changed.
        /// </summary>
        event EventHandler<EventArgs> ActiveDockTabItemChanged;


        /// <summary>
        /// Gets the items that are controlled by this conductor.
        /// </summary>
        /// <value>A collection of items that are conducted by this conductor.</value>
        new IEnumerable<EditorDockTabItemViewModel> Items { get; }


        /// <summary>
        /// Activates the specified item.
        /// </summary>
        /// <param name="item">
        /// The <see cref="IDockTabItem"/> to activate. Must not be <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="item"/> does not implement <see cref="IDockTabItem"/>.
        /// </exception>
        void ActivateItem(object item);


        /// <summary>
        /// Loads the docking layout.
        /// </summary>
        /// <param name="storedLayout">The stored layout.</param>
        /// <inheritdoc cref="DockSerializer.Load"/>
        void LoadLayout(XElement storedLayout);


        /// <summary>
        /// Saves the docking layout.
        /// </summary>
        /// <param name="excludeNonPersistentItems">
        /// <see langword="true"/> to exclude non-persistent <see cref="IDockTabItem"/>s.
        /// <see langword="false"/> to store the layout of all (persistent and non-persistent)
        /// <see cref="IDockTabItem"/>s.
        /// </param>
        /// <returns>The <see cref="XElement"/> with the serialized layout.</returns>
        /// <inheritdoc cref="DockSerializer.Save(IDockControl,bool)"/>
        XElement SaveLayout(bool excludeNonPersistentItems = false);
    }
}
