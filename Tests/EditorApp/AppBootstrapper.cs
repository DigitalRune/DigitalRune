using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DigitalRune.Editor;
using DigitalRune.Editor.About;
using DigitalRune.Editor.Colors;
using DigitalRune.Editor.Commands;
using DigitalRune.Editor.Diagnostics;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Errors;
using DigitalRune.Editor.Game;
using DigitalRune.Editor.Properties;
using DigitalRune.Editor.Options;
using DigitalRune.Editor.Output;
using DigitalRune.Editor.Printing;
using DigitalRune.Editor.QuickLaunch;
using DigitalRune.Editor.Search;
using DigitalRune.Editor.Status;
using DigitalRune.Editor.Themes;
using DigitalRune.Editor.Layout;
using DigitalRune.Editor.Models;
using DigitalRune.Editor.Outlines;
using DigitalRune.Editor.Shader;
using DigitalRune.Editor.Text;
using DigitalRune.Editor.Textures;
using DigitalRune.ServiceLocation;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using NLog;


namespace EditorApp
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class AppBootstrapper : Bootstrapper
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private const string UniqueName = "DigitalRune_EditorApp";
        private const string ApplicationName = "EditorApp";
        private const int ExitCodeConfigurationFailed = 1;
        private const string Email = "office@digitalrune.com";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ServiceContainer _serviceContainer;
        private EditorViewModel _editor;
        private bool _configurationFailed;
        private DocumentExtension _documentExtension;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        protected override void OnConfigure()
        {
            if (WindowsHelper.IsInDesignMode)
                return;

            Logger.Info("Configuring editor.");

#if !DEBUG
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif

            _serviceContainer = new ServiceContainer();

            // Configure general services.
            _serviceContainer.Register(typeof(IWindowService), null, typeof(WindowManager));

            // Configure editor.
            _editor = new EditorViewModel(_serviceContainer)
            {
                ApplicationName = ApplicationName,
                ApplicationIcon = BitmapFrame.Create(new Uri("pack://application:,,,/DigitalRune.Editor;component/Resources/Raido.ico", UriKind.RelativeOrAbsolute))
            };
            // Core extensions
            _editor.Extensions.Add(new CommandExtension());
            _editor.Extensions.Add(new LayoutExtension());
            _editor.Extensions.Add(new AboutExtension());
            _editor.Extensions.Add(new OptionsExtension());
            _editor.Extensions.Add(new PrintExtension());
            _editor.Extensions.Add(new QuickLaunchExtension());
            _editor.Extensions.Add(new ThemeExtension());
            _editor.Extensions.Add(new StatusExtension());
            _editor.Extensions.Add(new SearchExtension());

            // Common tool windows.
            _editor.Extensions.Add(new OutputExtension());
            _editor.Extensions.Add(new ErrorExtension());
            _editor.Extensions.Add(new OutlineExtension());
            _editor.Extensions.Add(new PropertiesExtension());

            // Document extensions.
            _documentExtension = new DocumentExtension();
            _editor.Extensions.Add(_documentExtension);
            _editor.Extensions.Add(new TextExtension());
            _editor.Extensions.Add(new ShaderExtension());
            _editor.Extensions.Add(new TexturesExtension());
            _editor.Extensions.Add(new ModelsExtension());

            // Other extensions.
            _editor.Extensions.Add(new DiagnosticsExtension());
            _editor.Extensions.Add(new ColorExtension());
            _editor.Extensions.Add(new GameExtension());
            _editor.Extensions.Add(new TestExtension0());

            try
            {
                bool success = _editor.Initialize();
                if (!success)
                {
                    // The editor could not be configured or command line arguments caused the
                    // editor to shut down (e.g. if "--help" was specified).
                    Application.Shutdown(_editor.ExitCode);
                    return;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Editor configuration failed.");

                _configurationFailed = true;
                Environment.ExitCode = ExitCodeConfigurationFailed;

                ExceptionHelper.ShowException(exception, ApplicationName, Email);
            }
        }


        protected override void OnStartup(object sender, StartupEventArgs eventArgs)
        {
            if (WindowsHelper.IsInDesignMode)
                return;

            if (_configurationFailed)
            {
                // The editor configuration has failed.
                // The Application is running and waits until the Exception window is closed.
                return;
            }

            bool isFirstInstance = SingleInstanceApplication.Initialize(UniqueName, OnOtherInstanceStarted);
            if (!isFirstInstance)
            {
                // If the command-line arguments are empty, then start new application instance.
                // But if a "file" was specified on the command-line, then open the document in
                // the first instance.
                if (_editor.CommandLineResult.ParsedArguments["file"] != null)
                {
                    // Notify first instance.
                    SingleInstanceApplication.SignalFirstInstance();

                    // Shutdown application.
                    Application.Shutdown(_editor.ExitCode);
                    return;
                }
            }

            // Start editor.
            bool success = _editor.Startup();
            if (!success)
            {
                Application.Shutdown(_editor.ExitCode);
                return;
            }

            // Set About dialog information.
            var aboutService = _serviceContainer.GetInstance<IAboutService>();
            if (aboutService != null)
            {
                aboutService.Copyright = "© 2008-2016 DigitalRune GmbH";
                aboutService.Icon = _editor.ApplicationIcon;
                var additionalInfo = new TextBlock();
                additionalInfo.Inlines.Add(new Run("Additional information and support: "));
                var link = new Hyperlink(new Run("http://www.digitalrune.com/"))
                {
                    NavigateUri = new Uri("http://www.digitalrune.com/")
                };
                link.Click += delegate { Process.Start(new ProcessStartInfo(link.NavigateUri.ToString())); };
                additionalInfo.Inlines.Add(link);
                aboutService.Information = additionalInfo;
                aboutService.InformationAsString = "Additional information and support: http://www.digitalrune.com/";
            }

            // Show window.
            var windowService = _serviceContainer.GetInstance<IWindowService>().ThrowIfMissing();
            windowService.ShowWindow(_editor, null, false);
        }


        protected override void OnExit(object sender, ExitEventArgs eventArgs)
        {
            Logger.Info("Exiting application.");

            // Clean up.
            _editor.Shutdown();
            _serviceContainer.Dispose();
            SingleInstanceApplication.Cleanup();

            // Set application's exit code.
            eventArgs.ApplicationExitCode = _editor.ExitCode;
        }


        /// <summary>
        /// Called in the first application instance when another instance was started.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Not used.</returns>
        private bool OnOtherInstanceStarted(string[] args)
        {
            Logger.Info("Receiving command-line arguments from another application instance.");

            if (_editor == null)
            {
                Logger.Warn("Application is not ready to accept external command-line arguments. Request ignored. Command-line arguments: {0}", args);
                return false;
            }
            
            // A second instance has been started.
            // --> Handle command line arguments of second instance.
            if (_editor.IsOpen)
            {
                HandleExternalCommandLineArgsAsync(args).Forget();
            }
            else
            {
                // Handle request when editor is loaded.
                EventHandler<ActivationEventArgs> handler = null;
                handler = (s, e) =>
                {
                    Debug.Assert(e.Opened, "This event handler should only be called when the editor is opened.");
                    _editor.Activated -= handler;
                    HandleExternalCommandLineArgsAsync(args).Forget();
                };
                _editor.Activated += handler;
            }

            return true;
        }


        private async Task HandleExternalCommandLineArgsAsync(string[] args)
        {
            // Bring application to foreground:
            WindowsHelper.ActivateWindow(Application.MainWindow);

            // Ignore first argument (name of executable).
            args = args.Skip(1).ToArray();

            // Parse new command-line arguments.
            // The same arguments have already been parsed by the second instance. That means, 
            // we know that they are valid and do not contain "--help" or similar.
            var parseResult = _editor.CommandLineParser.Parse(args);

            // Let the document service open files specified on the command line.
            await _documentExtension.OpenFromCommandLineAsync(parseResult);
        }


        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
        {
            // An unhandled exception. Report the exception and then exit.
            Logger.Error(eventArgs.Exception, "Unhandled exception.");

            ExceptionHelper.ShowException(eventArgs.Exception, ApplicationName, Email);

            Application.Shutdown();
            eventArgs.Handled = true;
        }
        #endregion
    }
}
