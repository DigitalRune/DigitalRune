// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region Credits
/*
    The types in this file are based on the Code Project article "WPF Single Instance Application" 
    by Arik Poznanski which is licensed under the Microsoft Public License (Ms-PL). 
    See http://www.codeproject.com/Articles/84270/WPF-Single-Instance-Application.aspx 
  
    The code has been heavily refactored to make it more flexible and reusable: It is now possible
    to start multiple instances of the same application. Each application instance can decide 
    whether to start run as a new instance or to notify the first instance.
 

    Microsoft Public License (Ms-PL)

    This license governs use of the accompanying software. If you use the software, you accept this 
    license. If you do not accept the license, do not use the software.

    1. Definitions

    The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same 
    meaning here as under U.S. copyright law.

    A "contribution" is the original software, or any additions or changes to the software.

    A "contributor" is any person that distributes its contribution under this license.

    "Licensed patents" are a contributor's patent claims that read directly on its contribution.

    2. Grant of Rights

    (A) Copyright Grant- Subject to the terms of this license, including the license conditions and 
    limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
    copyright license to reproduce its contribution, prepare derivative works of its contribution, and 
    distribute its contribution or any derivative works that you create.

    (B) Patent Grant- Subject to the terms of this license, including the license conditions and 
    limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
    license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or 
    otherwise dispose of its contribution in the software or derivative works of the contribution in 
    the software.

    3. Conditions and Limitations

    (A) No Trademark License- This license does not grant you rights to use any contributors' name, 
    logo, or trademarks.

    (B) If you bring a patent claim against any contributor over patents that you claim are infringed 
    by the software, your patent license from such contributor to the software ends automatically.

    (C) If you distribute any portion of the software, you must retain all copyright, patent, 
    trademark, and attribution notices that are present in the software.

    (D) If you distribute any portion of the software in source code form, you may do so only under 
    this license by including a complete copy of this license with your distribution. If you 
    distribute any portion of the software in compiled or object code form, you may only do so under a 
    license that complies with this license.

    (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no 
    express warranties, guarantees or conditions. You may have additional consumer rights under your 
    local laws which this license cannot change. To the extent permitted under your local laws, the 
    contributors exclude the implied warranties of merchantability, fitness for a particular purpose 
    and non-infringement. 
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using DigitalRune.Windows;
using DigitalRune.Windows.Interop;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Helper class that ensures that only one instance of an application is running at a time.
    /// </summary>
    /// <remarks>
    /// Note: The <see cref="SingleInstanceApplication"/> should be used with some caution, because
    /// it does no security checking. For example, if one instance of an application that uses this
    /// class is running as Administrator, any other instance, even if it is not running as
    /// Administrator, can activate it with command line arguments. For most applications, this will
    /// not be much of an issue.
    /// </remarks>
    /// <example>
    /// <para>
    /// To run only a single instance of an application the <strong>App</strong> class needs to call
    /// <see cref="Initialize"/>. The method returns a value indicating whether the current instance
    /// is the first instance of the application. The application can then decide whether to run as
    /// a new instance or whether call <see cref="SignalFirstInstance()"/> to notify the first
    /// instance that a new instance has been started and exit. The command line arguments of the
    /// second instance will be transmitted to the first instance.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> When defining a custom Main method the <strong>Build
    /// Action</strong> of the App.xaml needs to be set from <strong>ApplicationDefinition</strong>
    /// to <strong>Page</strong> in the Properties Window.
    /// </para>
    /// <code lang="csharp">
    /// <![CDATA[
    /// /// <summary>
    /// /// Interaction logic for App.xaml
    /// /// </summary>
    /// public partial class App : Application
    /// {
    ///     private const string UniqueName = "My_Unique_Application_String";
    /// 
    ///     [STAThread]
    ///     public static void Main()
    ///     {
    ///         if (SingleInstanceApplication.Initialize(UniqueName, OnOtherInstanceStarted))
    ///         {
    ///             var application = new App();
    ///             application.InitializeComponent();
    ///             application.Run();
    ///         }
    ///         else
    ///         {
    ///             // Call OnOtherInstanceStarted in first instance.
    ///             SingleInstanceApplication.SignalFirstInstance();
    ///         }
    /// 
    ///         // Allow single instance code to perform cleanup operations
    ///         SingleInstance<App>.Cleanup();
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Called in the first application instance when another instance was started.
    ///     /// </summary>
    ///     /// <param name="args">The command line arguments.</param>
    ///     /// <returns>Not used.</returns>
    ///     private bool OnOtherInstanceStarted(string[] args)
    ///     {
    ///         // A second instance has been started.
    ///         // --> Handle command line arguments of second instance.
    ///         // ...
    ///         return true;
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public static class SingleInstanceApplication
    {
        //--------------------------------------------------------------
        #region Nested Types
        //--------------------------------------------------------------

        /// <summary>
        /// Remoting service class which is exposed by the server i.e the first instance and called by
        /// the second instance to pass on the command line arguments to the first instance and cause it
        /// to activate itself.
        /// </summary>
        private class IpcRemoteService : MarshalByRefObject
        {
            /// <summary>
            /// Activates the first instance of the application.
            /// </summary>
            /// <param name="args">List of arguments to pass to the first instance.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            public void InvokeFirstInstance(string[] args)
            {
                if (Application.Current != null)
                {
                    // Do an asynchronous call to ActivateFirstInstance function
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(ActivateFirstInstanceCallback), args);
                }
            }


            /// <summary>
            /// Remoting Object's lease expires after every 5 minutes by default. We need to override the
            /// InitializeLifetimeService method to ensure that lease never expires.
            /// </summary>
            /// <returns>Always <see langword="null"/>.</returns>
            public override object InitializeLifetimeService()
            {
                return null;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        /// <summary>
        /// String delimiter used in channel names.
        /// </summary>
        private const string Delimiter = ":";

        /// <summary>
        /// Suffix to the channel name.
        /// </summary>
        private const string ChannelNameSuffix = "SingeInstanceIPCChannel";

        /// <summary>
        /// Remote service name.
        /// </summary>
        private const string RemoteServiceName = "SingleInstanceApplicationService";

        /// <summary>
        /// IPC protocol used (string).
        /// </summary>
        private const string IpcProtocol = "ipc://";

        /// <summary>
        /// Application mutex.
        /// </summary>
        private static Mutex _singleInstanceMutex;

        /// <summary>
        /// <see langword="true"/> if this is the first instance; otherwise, <see langword="false"/>.
        /// </summary>
        private static bool _isFirstInstance;

        /// <summary>
        /// Name of the IPC channel for communications.
        /// </summary>
        private static string _channelName;

        /// <summary>
        /// IPC channel for communications.
        /// </summary>
        private static IpcServerChannel _channel;

        /// <summary>
        /// Callback that notifies the application that another instance was started.
        /// </summary>
        private static Func<string[], bool> _callback;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets an array of command line arguments for the application.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public static string[] CommandLineArgs
        {
            get { return _commandLineArgs; }
        }
        private static string[] _commandLineArgs;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes the application and checks whether this is the first instance of the
        /// application.
        /// </summary>
        /// <param name="uniqueName">The system-wide unique name of the application.</param>
        /// <param name="callback">
        /// The callback which notifies the application that another instance has been started. Can be
        /// <see langword="null"/>. The function argument is the array command line arguments of the 
        /// newly started application. The <see cref="bool"/> return value is currently not used.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this is the first instance of the application; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="SingleInstanceApplication"/> has already been initialized.
        /// </exception>
        public static bool Initialize(string uniqueName, Func<string[], bool> callback)
        {
            if (WindowsHelper.IsInDesignMode)
            {
                // Do nothing at design-time.
                return true;
            }

            if (_channelName != null)
                throw new InvalidOperationException("The single-instance application has already been initialized.");

            _callback = callback;
            _commandLineArgs = GetCommandLineArgs(uniqueName);

            // Build unique application ID and the IPC channel name.
            string applicationIdentifier = uniqueName + Environment.UserName;
            _channelName = String.Concat(applicationIdentifier, Delimiter, ChannelNameSuffix);

            // Create system-wide mutex based on unique application ID.
            _singleInstanceMutex = new Mutex(false, applicationIdentifier);

            // The application that acquires the mutex is considered to be the first instance.
            _isFirstInstance = _singleInstanceMutex.WaitOne(0);
            if (_isFirstInstance)
                CreateRemoteService();

            return _isFirstInstance;
        }


        /// <summary>
        /// Gets command line args - for ClickOnce deployed applications, command line args may not
        /// be passed directly, they have to be retrieved.
        /// </summary>
        /// <returns>An array of command line arguments.</returns>
        private static string[] GetCommandLineArgs(string uniqueApplicationName)
        {
            string[] args = null;
            if (AppDomain.CurrentDomain.ActivationContext == null)
            {
                // The application was not clickonce deployed, get args from standard API's.
                args = Environment.GetCommandLineArgs();
            }
            else
            {
                // The application was clickonce deployed.
                // Clickonce deployed apps cannot receive traditional command line arguments.
                // As a workaround command line arguments can be written to a shared location before 
                // the app is launched and the app can obtain its command line arguments from the 
                // shared location.
                string appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), uniqueApplicationName);
                string cmdLinePath = Path.Combine(appFolderPath, "cmdline.txt");
                if (File.Exists(cmdLinePath))
                {
                    try
                    {
                        using (TextReader reader = new StreamReader(cmdLinePath, Encoding.Unicode))
                        {
                            args = Win32.CommandLineToArgvW(reader.ReadToEnd());
                        }

                        File.Delete(cmdLinePath);
                    }
                    catch (IOException)
                    {
                    }
                }
            }

            return args ?? new string[0];
        }


        /// <summary>
        /// Creates a remote service for communication.
        /// </summary>
        private static void CreateRemoteService()
        {
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider
            {
                TypeFilterLevel = TypeFilterLevel.Full
            };

            IDictionary props = new Dictionary<string, string>();
            props["name"] = _channelName;
            props["portName"] = _channelName;
            props["exclusiveAddressUse"] = "false";

            // Create the IPC Server channel with the channel properties.
            _channel = new IpcServerChannel(props, serverProvider);

            // Register the channel with the channel services.
            ChannelServices.RegisterChannel(_channel, true);

            // Expose the remote service with the RemoteServiceName.
            IpcRemoteService remoteService = new IpcRemoteService();
            RemotingServices.Marshal(remoteService, RemoteServiceName);
        }


        /// <summary>
        /// Signals the first instance that a new instance of the application has been started.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the first instance was successfully notified; 
        /// <see langword="false"/> if the first instance could not be reached.
        /// </returns>
        public static bool SignalFirstInstance()
        {
            return SignalFirstInstance(_channelName, _commandLineArgs);
        }


        /// <summary>
        /// Creates a client channel and obtains a reference to the remoting service exposed by the
        /// server -  in this case, the remoting service exposed by the first instance. Calls a function
        /// of the remoting service  class to pass on command line arguments from the second instance to
        /// the first and cause it to activate itself.
        /// </summary>
        /// <param name="channelName">The application's IPC channel name.</param>
        /// <param name="args">
        /// Command line arguments for the second instance, passed to the first instance to take
        /// appropriate action.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the first instance was successfully notified; 
        /// <see langword="false"/> if the first instance could not be reached.
        /// </returns>
        private static bool SignalFirstInstance(string channelName, string[] args)
        {
            IpcClientChannel secondInstanceChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(secondInstanceChannel, true);

            string remotingServiceUrl = IpcProtocol + channelName + "/" + RemoteServiceName;

            // Obtain a reference to the remoting service exposed by the server i.e the first instance of the application.
            IpcRemoteService firstInstanceRemoteServiceReference = (IpcRemoteService)RemotingServices.Connect(typeof(IpcRemoteService), remotingServiceUrl);

            // Check that the remote service exists, in some cases the first instance may not yet have 
            // created one, in which case we wait a few milliseconds and retry.
            for (int i = 0; i < 40 && firstInstanceRemoteServiceReference == null; i++)
                Thread.Sleep(25);

            if (firstInstanceRemoteServiceReference == null)
            {
                // Remove service not found.
                return false;
            }

            // Invoke a method of the remote service exposed by the first instance passing on the 
            // command line arguments and causing the first instance to activate itself.
            // The remove service call may fail if the pipe is busy (e.g. if multiple instances
            // are opened at once.)
            bool success = false;
            for (int i = 0; i < 100 && !success; i++)
            {
                try
                {
                    firstInstanceRemoteServiceReference.InvokeFirstInstance(args);
                    success = true;
                }
                catch
                {
                    // Remove service call failed. Pipe is probably busy.
                    // --> Wait and retry.
                    Thread.Sleep(25);
                }
            }

            return success;
        }


        /// <summary>
        /// Callback for activating first instance of the application.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        /// <returns>Always <see langword="null"/>.</returns>
        private static object ActivateFirstInstanceCallback(object arg)
        {
            // Get command line args to be passed to first instance.
            string[] args = arg as string[];
            ActivateFirstInstance(args);
            return null;
        }


        /// <summary>
        /// Activates the first instance of the application with arguments from a second instance.
        /// </summary>
        /// <param name="args">
        /// An array of arguments to supply the first instance of the application.
        /// </param>
        private static void ActivateFirstInstance(string[] args)
        {
            // Set main window state and process command line args
            if (Application.Current == null || _callback == null)
                return;

            _callback(args);
        }


        /// <summary>
        /// Cleans up single-instance code, clearing shared resources, mutexes, etc.
        /// </summary>
        public static void Cleanup()
        {
            if (_singleInstanceMutex != null)
            {
                if (_isFirstInstance)
                    _singleInstanceMutex.ReleaseMutex();

                _singleInstanceMutex.Close();
                _singleInstanceMutex = null;
            }

            if (_channel != null)
            {
                ChannelServices.UnregisterChannel(_channel);
                _channel = null;
            }

            _callback = null;
            _commandLineArgs = null;
            _channelName = null;
            _isFirstInstance = false;
        }
        #endregion
    }
}
