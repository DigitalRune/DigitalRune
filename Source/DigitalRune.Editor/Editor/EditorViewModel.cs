// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Editor.Properties;
using DigitalRune.ServiceLocation;
using DigitalRune.Windows.Framework;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Manages the editor (including main window, services, extensions, etc.).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Services:</strong><br/>
    /// The editor adds the following services to the service container:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="IEditorService"/></item>
    /// <item><see cref="IMessageBus"/></item>
    /// <item><see cref="IViewLocator"/></item>
    /// </list>
    /// </remarks>
    public partial class EditorViewModel : Screen, IEditorService
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _isInitialized;
        private ResourceDictionary _resourceDictionary;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets an <see cref="EditorViewModel" /> instance which can be used at design-time.
        /// </summary>
        /// <value>
        /// An <see cref="EditorViewModel" /> instance which can be used at design-time.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static EditorViewModel DesignInstance
        {
            get
            {
                return new EditorViewModel(new ServiceContainer())
                {
                    ApplicationName = "MyEditor",
                    CaptionBarItemsRight = { "CaptionBarItemsRight", "CaptionBarItemRight1" },
                    StatusBarItemsLeft = { "StatusBarItemsLeft0", "StatusBarItemsLeft1" },
                    StatusBarItemsCenter = { "StatusBarItemsCenter", "StatusBarItemCenter1" },
                    StatusBarItemsRight = { "StatusBarItemsRight", "StatusBarItemRight1" },
                };
            }
        }

        /// <inheritdoc/>
        public string ApplicationName
        {
            get { return _applicationName; }
            set
            {
                if (SetProperty(ref _applicationName, value))
                    UpdateTitle();
            }
        }
        private string _applicationName;


        /// <inheritdoc/>
        public object ApplicationIcon
        {
            get { return _applicationIcon; }
            set { SetProperty(ref _applicationIcon, value); }
        }
        private object _applicationIcon;


        /// <inheritdoc/>
        public string Subtitle
        {
            get { return _subtitle; }
            set
            {
                if (SetProperty(ref _subtitle, value))
                    UpdateTitle();
            }
        }
        private string _subtitle;


        /// <inheritdoc/>
        public int ExitCode
        {
            get { return _exitCode ?? (int)Editor.ExitCode.ERROR_SUCCESS; }
        }
        private int? _exitCode;


        /// <summary>
        /// Gets a value indicating whether the shutdown of the application has been initiated.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the application is shutting down; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        public bool IsShuttingDown
        {
            get { return _exitCode.HasValue; }
        }


        /// <inheritdoc/>
        public ServiceContainer Services { get; }


        /// <inheritdoc/>
        public EditorExtensionCollection Extensions { get; }


        /// <summary>
        /// Gets the extensions ordered by priority.
        /// </summary>
        /// <value>The extensions ordered by priority.</value>
        private IEnumerable<EditorExtension> OrderedExtensions
        {
            get
            {
                // Note: OrderByDescending is a stable sort.
                return Extensions.OrderByDescending(e => e.Priority);
            }
        }


        /// <inheritdoc/>
        public IList<object> StatusBarItemsLeft { get; } = new ObservableCollection<object>();


        /// <inheritdoc/>
        public IList<object> StatusBarItemsCenter { get; } = new ObservableCollection<object>();


        /// <inheritdoc/>
        public IList<object> StatusBarItemsRight { get; } = new ObservableCollection<object>();


        /// <inheritdoc/>
        public IList<object> CaptionBarItemsRight { get; } = new ObservableCollection<object>();


        #region ----- Window -----

        /// <inheritdoc/>
        public EditorWindow Window
        {
            get { return _window; }
            internal set
            {
                if (_window == value)
                    return;

                if (_window != null)
                    RemoveInputBindings(_window);

                _window = value;

                // Update input/command bindings but only if editor IsOpen is set. If not IsOpen
                // UpdateInputAndCommandBindings will be called in OnActivated.
                if (_window != null && IsOpen)
                    UpdateInputAndCommandBindings();
            }
        }
        private EditorWindow _window;
        #endregion


        /// <summary>
        /// Gets the command that is executed when the window becomes the foreground window.
        /// </summary>
        /// <value>
        /// The command that is executed when the window becomes the foreground window.
        /// </value>
        public ICommand WindowActivationCommand { get; }


        /// <inheritdoc/>
        public event EventHandler<EventArgs> WindowActivated;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorViewModel"/> class using the given
        /// name and service provider.
        /// </summary>
        /// <param name="serviceContainer">The <see cref="ServiceContainer"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceContainer"/> is <see langword="null"/>.
        /// </exception>
        public EditorViewModel(ServiceContainer serviceContainer)
        {
            Logger.Debug("Creating EditorViewModel.");

            if (serviceContainer == null)
                throw new ArgumentNullException(nameof(serviceContainer));

            Services = serviceContainer;

            DockStrategy = new EditorDockStrategy();

            _applicationName = EditorHelper.GetDefaultApplicationName();
            _subtitle = null;

            Extensions = new EditorExtensionCollection();

            InitializeCommandItems();

            WindowActivationCommand = new DelegateCommand(() => OnWindowActivated(EventArgs.Empty));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Configures the editor.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the editor was initialized successfully; otherwise,
        /// <see langword="false"/>. <see langword="false"/> can also indicate that command line
        /// arguments have caused an application exit.
        /// </returns>
        public bool Initialize()
        {
            Logger.Debug("Configuring editor.");

            _isInitialized = true;

            UpdateTitle();
            InitializeCommandLineParser();

            // Register the base services.
            Services.Register(typeof(IEditorService), null, this);
            Services.Register(typeof(IMessageBus), null, new MessageBus());
            Services.Register(typeof(IViewLocator), null, typeof(EditorViewLocator));

            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Resources/DataTemplates.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_resourceDictionary);

            // Register views.
            Services.RegisterView(typeof(EditorViewModel), typeof(EditorWindow));

            // Initialize editor extensions.
            Extensions.IsLocked = true;
            foreach (var extension in OrderedExtensions)
                extension.Initialize(this);

            // Parse command-line arguments.
            ParseCommandLineArguments();

            return !IsShuttingDown;
        }


        /// <summary>
        /// Starts the editor.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the editor was started successfully; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Startup()
        {
            Logger.Debug("Starting editor.");

            if (!_isInitialized)
                Initialize();

            if (IsShuttingDown)
            {
                // Shutdown was initiated when the command-line arguments were processed.
                return false;
            }

            foreach (var extension in OrderedExtensions)
            {
                extension.Startup();
                if (IsShuttingDown)
                    return false;
            }

            InvalidateUI();

            return !IsShuttingDown;
        }


        /// <inheritdoc/>
        public void Exit(int exitCode = (int)Editor.ExitCode.ERROR_SUCCESS)
        {
            if (!_exitCode.HasValue)
                _exitCode = exitCode;

            // Exit can be called by command line parsing. In this case we are not yet conducted.
            Conductor?.DeactivateItemAsync(this, true);
        }


        /// <summary>
        /// Shuts down the editor.
        /// </summary>
        /// <remarks>
        /// This method should only be called by the owner of the <see cref="EditorViewModel"/>.
        /// <see cref="EditorExtension"/>s should call <see cref="Exit"/> to end the application.
        /// </remarks>
        public void Shutdown()
        {
            Logger.Debug("Shutting down editor.");

            foreach (var extension in OrderedExtensions.Reverse())
                extension.Shutdown();

            foreach (var extension in OrderedExtensions.Reverse())
                extension.Uninitialize();

            Extensions.IsLocked = false;

            Settings.Default.Save();

            EditorHelper.UnregisterResources(_resourceDictionary);
        }


        /// <summary>
        /// Updates the window title.
        /// </summary>
        private void UpdateTitle()
        {
            DisplayName = string.IsNullOrEmpty(Subtitle)
                          ? ApplicationName
                          : Invariant($"{Subtitle} - {ApplicationName}");
        }


        /// <summary>
        /// Raises the <see cref="WindowActivated"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong><br/> When overriding
        /// <see cref="OnWindowActivated"/> in a derived class, be sure to call the base class's
        /// <see cref="OnWindowActivated"/> method so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnWindowActivated(EventArgs eventArgs)
        {
            WindowActivated?.Invoke(this, eventArgs);
        }
        #endregion
    }
}
