// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Windows.Docking;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Extends the editor with new functionality. (Base class)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The editor defines the skeleton (shell) of a windows application. Extensions add new
    /// functionality: new services, menus, toolbars, windows, etc.
    /// </para>
    /// <para>
    /// For a correct startup/shutdown order:
    /// </para>
    /// <list type="number">
    /// <item>
    /// <term>Initialization</term>
    /// <description>
    /// In <see cref="OnInitialize"/> add custom command-line parameters and new services. Do not
    /// consume other services - other extensions may not have been initialized yet.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Startup</term>
    /// <description>
    /// In <see cref="OnStartup"/> read the parsed command-line parameters and consume other
    /// services. Be aware that the main window of the application is created <i>before</i> but
    /// shown <i>after</i> <see cref="OnStartup"/> is called.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Shutdown</term>
    /// <description>
    /// In <see cref="OnShutdown"/> reset any changes made to the editor. All services are still
    /// available and can be used during shutdown.
    /// </description>
    /// </item>
    /// <item>
    /// <term>De-initialization</term>
    /// <description>
    /// In <see cref="OnUninitialize"/> remove custom command-line parameters and services. 
    /// Do not access other services - other extensions may not have already been uninitialized.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public abstract class EditorExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _initialized;   // True if Initialize and Startup were executed.
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the editor.
        /// </summary>
        /// <value>The editor.</value>
        public IEditorService Editor { get; private set; }


        /// <summary>
        /// Gets or sets the command items provided by this extension.
        /// </summary>
        /// <value>The command items.</value>
        /// <remarks>
        /// Extensions can use custom command items to create new menus and toolbars. All items
        /// should be registered in this collection.
        /// </remarks>
        public CommandItemCollection CommandItems { get; } = new CommandItemCollection();


        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>The priority.</value>
        /// <remarks>
        /// The editor sorts <see cref="EditorExtension"/> by their priority. Extensions with a
        /// higher priority are initialized first.
        /// </remarks>
        public int Priority { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorExtension"/> class.
        /// </summary>
        protected EditorExtension()
        {
            Logger.Debug(CultureInfo.InvariantCulture, "Creating {0}.", GetType().Name);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes the editor extension.
        /// </summary>
        /// <param name="editor">The editor.</param>
        internal void Initialize(IEditorService editor)
        {
            Logger.Debug(CultureInfo.InvariantCulture, "Initializing {0}.", GetType().Name);

            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            Editor = editor;
            OnInitialize();
        }


        /// <summary>
        /// Called when the extension should be initialized.
        /// </summary>
        protected abstract void OnInitialize();


        /// <summary>
        /// Initializes the editor extension.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The editor extension has already been initialized.
        /// </exception>
        internal void Startup()
        {
            if (_initialized)
            {
                var message = Invariant($"Cannot startup extension {GetType().Name}. Extension has already been initialized.");
                throw new InvalidOperationException(message);
            }

            Logger.Debug(CultureInfo.InvariantCulture, "Starting {0}.", GetType().Name);
            OnStartup();
            _initialized = true;
        }


        /// <summary>
        /// Called when the extension needs to be initialized.
        /// </summary>
        /// <inheritdoc cref="OnInitialize"/>
        protected abstract void OnStartup();


        /// <summary>
        /// Shuts down this extension.
        /// </summary>
        internal void Shutdown()
        {
            // Only shut down the extension if it has Startup() has been called previously.
            // (Note: It is okay to call Shutdown() without previously calling Startup().)
            if (!_initialized)
                return;

            Logger.Debug(CultureInfo.InvariantCulture, "Shutting down {0}.", GetType().Name);
            OnShutdown();
            _initialized = false;
        }


        /// <summary>
        /// Called when the extension needs to be de-initialized.
        /// </summary>
        protected abstract void OnShutdown();


        /// <summary>
        /// De-initializes the editor extension.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Uninitializing")]
        internal void Uninitialize()
        {
            Logger.Debug(CultureInfo.InvariantCulture, "Uninitializing {0}.", GetType().Name);

            OnUninitialize();
            Editor = null;
        }


        /// <summary>
        /// Called when the extension should be uninitialized.
        /// </summary>
        protected abstract void OnUninitialize();


        /// <summary>
        /// Gets the <see cref="IDockTabItem"/> for a given ID.
        /// </summary>
        /// <param name="dockId">
        /// The ID of the <see cref="IDockTabItem"/> (see <see cref="IDockTabItem.DockId"/>).
        /// </param>
        /// <returns>
        /// The view model that matches the given ID, or <see langword="null"/> if the ID is
        /// unknown.
        /// </returns>
        /// <inheritdoc cref="OnGetViewModel" />
        internal IDockTabItem GetViewModel(string dockId)
        {
            if (string.IsNullOrEmpty(dockId))
                return null;

            return OnGetViewModel(dockId);
        }


        /// <summary>
        /// Called when the editor needs to get the <see cref="IDockTabItem"/> for a given ID.
        /// </summary>
        /// <param name="dockId">
        /// The ID of the <see cref="IDockTabItem"/> (see <see cref="IDockTabItem.DockId"/>).
        /// </param>
        /// <returns>
        /// The view model that matches the given ID, or <see langword="null"/> if the ID is
        /// unknown.
        /// </returns>
        /// <remarks>
        /// Each <see cref="IDockTabItem"/> has a <see cref="IDockTabItem.DockId"/>. This ID is
        /// written to a file when the window layout is serialized. When the window layout is loaded
        /// from file, the editor checks the editor extensions and this method is called.
        /// </remarks>
        protected abstract IDockTabItem OnGetViewModel(string dockId);
        #endregion
    }
}
