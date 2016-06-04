// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if WINDOWS
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Message = System.Windows.Interop.MSG;


namespace DigitalRune.Graphics.Interop
{
  /// <summary>
  /// Provides an execution environment for a WPF application in a Windows Forms application.
  /// </summary>
  public static class WpfEnvironment
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static volatile bool _applicationStarted;   // Needs to be volatile because it is used 
                                                        // for synchronization!
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the WPF application.
    /// </summary>
    /// <value>The WPF application.</value>
    public static Application Application { get; private set; }


    /// <summary>
    /// Gets the dispatcher of the WPF thread.
    /// </summary>
    /// <value>The dispatcher of the WPF thread.</value>
    public static Dispatcher Dispatcher
    {
      get
      {
        if (Application == null)
          return null;

        return Application.Dispatcher;
      }
    }


    /// <summary>
    /// Gets or sets the Windows Forms main window (for example, the XNA game window).
    /// </summary>
    /// <value>The Windows Forms main window (for example, the XNA game window).</value>
    public static Form Form { get; private set; }


    private static IntPtr Handle { get; set; }
    #endregion
    


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes the WPF environment.
    /// </summary>
    /// <param name="winFormHandle">The handle of the Windows Forms main window.</param>
    /// <remarks>
    /// If the WPF environment is already initialized, this method does nothing. (This means, 
    /// redundant calls of this method are no problem.)
    /// </remarks>
    public static void Startup(IntPtr winFormHandle)
    {
      // Already running?
      if (Application != null)
        return;
      
      if (winFormHandle == IntPtr.Zero)
        throw new ArgumentException("Invalid winFormHandle.");

      Form = Control.FromHandle(winFormHandle) as Form;
      if (Form != null)
        Handle = Form.Handle;

      // Create an STA thread for the WPF Window.
      // The WPF window will have its own WPF message loop in this thread.
      Thread wpfThread = new Thread(WpfMainMethod);
      wpfThread.SetApartmentState(ApartmentState.STA);  //Many WPF UI elements need to be created inside STA
      wpfThread.Start();

      // Block until application is created.
      while (!_applicationStarted) { }
    }


    /// <summary>
    /// Shuts down the WPF environment.
    /// </summary>
    public static void Shutdown()
    {
      if (Application == null)
        return;

      Dispatcher.Invoke((Action)(Application.Shutdown));
    }


    private static void WpfMainMethod()
    {
      // Create a new WPF application.
      Application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

      // Filter WPF message loop to forward keyboard messages to the Windows Forms 
      // message loop. If this is uncommented, the XNA Keyboard will not receive all 
      // inputs if the WPF window is active.
      ComponentDispatcher.ThreadFilterMessage += OnThreadFilterMessage;

      // The debugger will not break when an exception occurs inside the WPF thread 
      // (unless catching first chance exceptions is enabled). If we break in the 
      // UnhandledExceptionFilter event then the user sees the call stack of the event. 
      // The call stack is unwound after this event.
      Dispatcher.UnhandledExceptionFilter += (s, e) => Debugger.Break();

      // Forward exceptions in the WPF thread to the WinForms thread. 
      Application.DispatcherUnhandledException += (s, e) =>
      {
        // Set flag to avoid blocking of the forms thread.
        _applicationStarted = true;

        // Forward the exception to the main thread.
        if (Form != null)
        {
          Form.BeginInvoke(new Action(delegate
          {
            throw new GraphicsException("Exception occurred in WPF environment.", e.Exception);
          }));
        }
      };

      Application.Startup += (s, e) => _applicationStarted = true;
      
      Application.Run();  // This call blocks until Shutdown() is called.
    }


    /// <summary>
    /// Called when the message pump receives a keyboard message.
    /// </summary>
    /// <param name="msg">A structure with the message data.</param>
    /// <param name="handled">
    /// <see langword="true"/> if the message was handled; otherwise, <see langword="false"/>.
    /// </param>
    private static void OnThreadFilterMessage(ref Message msg, ref bool handled)
    {
      if (Form == null)
        return;

      // Forward keyboard messages to the message loop of the game window. If they 
      // are not forwarded, XNA Keyboard class will not detect keypresses when the 
      // WPF window is active.
      // To forward the messages to WinForms, we could also call 
      // System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(wpfWindow);
      // This forwards the same keyboard messages, except the mouse wheel message.
      int message = msg.message;
      if (message == WindowMessages.WM_KEYDOWN
          || message == WindowMessages.WM_KEYUP
          || message == WindowMessages.WM_CHAR
          || message == WindowMessages.WM_DEADCHAR
          || message == WindowMessages.WM_SYSKEYDOWN
          || message == WindowMessages.WM_SYSKEYUP
          || message == WindowMessages.WM_SYSCHAR
          || message == WindowMessages.WM_SYSDEADCHAR
          || message == WindowMessages.WM_MOUSEWHEEL
#if MONOGAME
          || message == WindowMessages.WM_MOUSEMOVE
          || message == WindowMessages.WM_LBUTTONDOWN
          || message == WindowMessages.WM_LBUTTONUP
          || message == WindowMessages.WM_LBUTTONDBLCLK
          || message == WindowMessages.WM_MBUTTONDOWN
          || message == WindowMessages.WM_MBUTTONUP
          || message == WindowMessages.WM_MBUTTONDBLCLK
          || message == WindowMessages.WM_RBUTTONDOWN
          || message == WindowMessages.WM_RBUTTONUP
          || message == WindowMessages.WM_RBUTTONDBLCLK
#endif
        )
      {
        NativeMethods.PostMessage(Handle, msg.message, msg.wParam, msg.lParam);
      }
    }
    #endregion
  }
}
#endif
