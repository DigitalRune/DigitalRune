// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Windows;

#if SILVERLIGHT
using System.Windows.Browser;
#endif

#if WINDOWS_PHONE
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Navigation;
#endif

#if !SILVERLIGHT && !WINDOWS_PHONE
using System.Windows.Threading;
#endif


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Instantiates a WPF, Silverlight or Windows Phone 7 application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A bootstrapper handles the startup, configuration and shutdown of an application. The base
    /// class <see cref="Bootstrapper"/> provides the general startup and shutdown procedure.
    /// Applications need to create a custom bootstrapper which inherits from 
    /// <see cref="Bootstrapper"/> and implements the application-specific configuration.
    /// </para>
    /// <para>
    /// <strong>WPF:</strong><br/>
    /// The custom bootstrapper needs to override <see cref="OnStartup"/> and load the main window
    /// if no <strong>Application.StartupUri</strong> is set.
    /// </para>
    /// <para>
    /// <strong>Silverlight:</strong><br/>
    /// The custom bootstrapper needs to override <see cref="OnStartup"/> and set the
    /// <strong>Application.RootVisual</strong>.
    /// </para>
    /// <para>
    /// <strong>Windows Phone 7:</strong><br/>
    /// The Windows Phone 7 version contains additional phone-specific features. For example, a
    /// custom bootstrappers can override the method <strong>CreatePhoneApplicationFrame()</strong>
    /// to provide a custom frame control instead of using the standard
    /// <strong>PhoneApplicationFrame</strong>.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following example shows a typical App.xaml file in Silverlight that uses a
    /// <see cref="Bootstrapper"/>. The bootstrapper is added to the application's resources.
    /// <code lang="xaml">
    /// <![CDATA[
    /// <Application x:Class="MyApplication.App"
    ///              xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///              xmlns:local="clr-namespace:MyApplication"
    ///              xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    ///              xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    ///              mc:Ignorable="d">
    /// 
    ///   <!--  Application Resources  -->
    ///   <Application.Resources>
    ///     <ResourceDictionary>
    ///       <ResourceDictionary.MergedDictionaries>
    ///         <ResourceDictionary Source="Resources/Styles.xaml" />
    ///       </ResourceDictionary.MergedDictionaries>
    /// 
    ///       <!--  Bootstrapper  -->
    ///       <local:MyApplicationBootstrapper x:Key="Bootstrapper" />
    ///  
    ///       <!--  Helper classes  -->
    ///       <local:ViewModelLocator x:Key="Locator" d:IsDataSource="True" />
    ///       <local:LocalizedResources x:Key="LocalizedResources" d:IsDataSource="True" />
    ///     </ResourceDictionary>
    ///   </Application.Resources>
    /// </Application>
    /// ]]>
    /// </code>
    /// The is no code-behind required. The file App.xaml.cs is basically empty. The startup and
    /// configuration is done by the bootstrapper.
    /// <code lang="csharp">
    /// <![CDATA[
    /// namespace MyApplication
    /// {  
    ///   public partial class App
    ///   {
    ///     public App()
    ///     {
    ///       InitializeComponent();
    ///     }
    ///   }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public class Bootstrapper
    {
#if WINDOWS_PHONE
        private PhoneApplicationService _phoneService;
        private bool _phoneApplicationInitialized;
#endif


        /// <summary>
        /// The application.
        /// </summary>
        public Application Application { get; protected set; }


#if WINDOWS_PHONE
        /// <summary>
        /// Gets the root frame of the phone application.
        /// </summary>
        /// <returns>The root frame of the phone application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }
#endif


        /// <summary>
        /// Initializes a new instance of the <see cref="Bootstrapper"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Bootstrapper()
        {
            if (WindowsHelper.IsInDesignMode)
                StartDesignTime();
            else
                StartRuntime();
        }


        private void StartDesignTime()
        {
            OnConfigure();
        }


        private void StartRuntime()
        {
            // ----- Silverlight-specific initialization.
            Application = Application.Current;
            Application.Startup += OnStartup;
            Application.Exit += OnExit;

#if !DEBUG
#if SILVERLIGHT || WINDOWS_PHONE
            Application.UnhandledException += OnUnhandledException;
#else
            if (!Debugger.IsAttached)
                Application.DispatcherUnhandledException += OnUnhandledException;
#endif
#endif

#if WINDOWS_PHONE
            // ----- Windows Phone-specific initialization.
            _phoneService = new PhoneApplicationService();
            _phoneService.Launching += OnLaunch;
            _phoneService.Activated += OnActivate;
            _phoneService.Deactivated += OnDeactivate;
            _phoneService.Closing += OnClose;
            Application.ApplicationLifetimeObjects.Add(_phoneService);
#endif

            // ----- Application-specific initialization.
            OnConfigure();

#if WINDOWS_PHONE
            // ----- Initialize navigation frame.
            // Avoid double-initialization. (I have no idea why this is necessary, but it
            // is in all Microsoft samples/templates? -- MartinG)
            if (_phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = CreatePhoneApplicationFrame();
            RootFrame.Navigated += OnNavigated;
            RootFrame.NavigationFailed += OnNavigationFailed;

            _phoneApplicationInitialized = true;
#endif
        }


        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <remarks>
        /// Derived classes can override this method to perform application-specific initialization,
        /// such as setting up an IoC container, configuring services, etc.
        /// </remarks>
        protected virtual void OnConfigure()
        {
        }


#if WINDOWS_PHONE
        /// <summary>
        /// Creates the navigation frame of the Windows Phone application.
        /// </summary>
        /// <returns>The <see cref="PhoneApplicationFrame"/>.</returns>
        protected virtual PhoneApplicationFrame CreatePhoneApplicationFrame()
        {
            return new PhoneApplicationFrame();
        }


        void OnNavigated(object sender, NavigationEventArgs eventArgs)
        {
            if (Application.RootVisual != RootFrame)
                Application.RootVisual = RootFrame;

            RootFrame.Navigated -= OnNavigated;
        }
#endif


        /// <summary>
        /// Called when the application is started.
        /// </summary>
        /// <param name="sender">The <see cref="Application"/>.</param>
        /// <param name="eventArgs">
        /// The <see cref="StartupEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnStartup(object sender, StartupEventArgs eventArgs)
        {
        }


#if SILVERLIGHT
        /// <summary>
        /// Called when an exception that is raised by Silverlight is not handled.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="ApplicationUnhandledExceptionEventArgs"/> instance containing the event
        /// data.
        /// </param>
        protected virtual void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs eventArgs)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!Debugger.IsAttached)
            {
                // This will allow the application to continue running after an exception has 
                // been thrown but not handled. 
                // For production applications this error handling should be replaced with something
                // that will report the error to the website and stop the application.
                eventArgs.Handled = true;
                WindowsHelper.CheckBeginInvokeOnUI(() => ReportErrorToDOM(eventArgs));
            }
        }


        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs eventArgs)
        {
            try
            {
                string errorMsg = eventArgs.ExceptionObject.Message + eventArgs.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
#elif WINDOWS_PHONE
        /// <summary>
        /// Called when an exception that is raised by Silverlight is not handled.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="ApplicationUnhandledExceptionEventArgs"/> instance containing the event
        /// data.
        /// </param>
        protected virtual void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs eventArgs)
        {
        }
#else
        /// <summary>
        /// Called when an exception is thrown by an application but not handled.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="DispatcherUnhandledExceptionEventArgs"/> instance containing the event
        /// data.
        /// </param>
        protected virtual void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
        {
        }
#endif


#if SILVERLIGHT || WINDOWS_PHONE
        /// <summary>
        /// Called just before the application shuts down (cannot be canceled).
        /// </summary>
        /// <param name="sender">The <see cref="Application"/>.</param>
        /// <param name="eventArgs">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnExit(object sender, EventArgs eventArgs)
        {
        }
#else
        /// <summary>
        /// Called just before the application shuts down (cannot be canceled).
        /// </summary>
        /// <param name="sender">The <see cref="Application"/>.</param>
        /// <param name="eventArgs">
        /// The <see cref="ExitEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnExit(object sender, ExitEventArgs eventArgs)
        {
        }
#endif


#if WINDOWS_PHONE
        /// <summary>
        /// Called when the when the Windows Phone application is being launched.
        /// </summary>
        /// <param name="sender">The <see cref="PhoneApplicationService"/>.</param>
        /// <param name="eventArgs">
        /// The <see cref="LaunchingEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnLaunch(object sender, LaunchingEventArgs eventArgs)
        {
        }


        /// <summary>
        /// Called when the Windows Phone application is being made active after previously being put 
        /// into a dormant state or tombstoned.
        /// </summary>
        /// <param name="sender">The <see cref="PhoneApplicationService"/>.</param>
        /// <param name="eventArgs">
        /// The <see cref="ActivatedEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnActivate(object sender, ActivatedEventArgs eventArgs)
        {
        }


        /// <summary>
        /// Called when the application is being deactivated.
        /// </summary>
        /// <param name="sender">The <see cref="PhoneApplicationService"/>.</param>
        /// <param name="eventArgs">
        /// The <see cref="DeactivatedEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnDeactivate(object sender, DeactivatedEventArgs eventArgs)
        {
        }


        /// <summary>
        /// Called when the application is exiting.
        /// </summary>
        /// <param name="sender">The <see cref="PhoneApplicationService"/>.</param>
        /// <param name="eventArgs">
        /// The <see cref="ClosingEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnClose(object sender, ClosingEventArgs eventArgs)
        {
        }


        /// <summary>
        /// Called when an error is encountered while navigating to the requested content.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="NavigationFailedEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnNavigationFailed(object sender, NavigationFailedEventArgs eventArgs)
        {
        }
#endif
    }
}
