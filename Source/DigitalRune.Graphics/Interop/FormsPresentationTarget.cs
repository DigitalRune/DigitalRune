// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if WINDOWS
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
#if !MONOGAME
using Rectangle = Microsoft.Xna.Framework.Rectangle;
#endif


namespace DigitalRune.Graphics.Interop
{
  /// <summary>
  /// A Windows Forms control that implements <see cref="IPresentationTarget"/> to host a 3D view.
  /// </summary>
  public class FormsPresentationTarget : Control, IPresentationTarget
  {
    // Note: We handle cross-thread calls by setting CheckForIllegalCrossThreadCalls
    // to false (see Constructor). When the Game thread accesses the handle to present
    // the graphics, we have a cross-thread call.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _rendering;
#if MONOGAME
    private SwapChainRenderTarget _renderTarget;
#endif
    #endregion


    //--------------------------------------------------------------
    #region Properties and Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    IGraphicsService IPresentationTarget.GraphicsService
    {
      get { return _graphicsService; }
      set
      {
        if (_graphicsService == value)
          return;

        var oldGraphicsService = _graphicsService;
        _graphicsService = value;

        OnGraphicsServiceChanged(oldGraphicsService, _graphicsService);
      }
    }
    private IGraphicsService _graphicsService;


    /// <summary>
    /// Gets the graphics service.
    /// </summary>
    /// <value>The graphics service.</value>
    /// <inheritdoc cref="IPresentationTarget.GraphicsService"/>
    public IGraphicsService GraphicsService
    {
      get { return _graphicsService; }
    }


    /// <inheritdoc/>
    int IPresentationTarget.Width
    {
      get { return ClientSize.Width; }
    }


    /// <inheritdoc/>
    int IPresentationTarget.Height
    {
      get { return ClientSize.Height; }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IPresentationTarget.IsVisible
    {
      get { return Visible; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FormsPresentationTarget"/> class.
    /// </summary>
    public FormsPresentationTarget()
    {
      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.UserPaint, true);

      // When using separate threads for Forms and the Game then we need to allow
      // cross-thread-calls. (The Game thread paints the control which is owned by
      // the Form thread.)
      // .NET 2.0 by default catches cross-thread-calls. To allow them uncomment the
      // line below. (The graphics manager will cause cross-thread calls, for instance
      // when it accesses the Handle property.)
      CheckForIllegalCrossThreadCalls = false;
    }


    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="Control"/> and its child controls
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// true to release both managed and unmanaged resources; false to release only unmanaged
    /// resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
#if MONOGAME
          _renderTarget.SafeDispose();
#endif
        }
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
    private void OnGraphicsServiceChanged(IGraphicsService oldGraphicsService, IGraphicsService newGraphicsService)
    {
#if MONOGAME
      if (oldGraphicsService != null)
      {
        _renderTarget.SafeDispose();
        _renderTarget = null;
      }
#endif
    }


    /// <inheritdoc/>
    bool IPresentationTarget.BeginRender(RenderContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      var graphicsDevice = GraphicsService.GraphicsDevice;
      int width = ClientSize.Width;
      int height = ClientSize.Height;

#if MONOGAME
      if (_renderTarget == null
          || _renderTarget.GraphicsDevice != graphicsDevice
          || _renderTarget.Width != width
          || _renderTarget.Height != height)
      {
        if (_renderTarget == null)
          _renderTarget = new SwapChainRenderTarget(graphicsDevice, Handle, width, height);
        else
          _renderTarget.Resize(width, height);
      }

      graphicsDevice.SetRenderTarget(_renderTarget);
      context.RenderTarget = _renderTarget;
#else
      bool deviceNeedsReset;
      switch (graphicsDevice.GraphicsDeviceStatus)
      {
        case GraphicsDeviceStatus.Lost:
          return false;

        case GraphicsDeviceStatus.NotReset:
          deviceNeedsReset = true;
          break;

        default:
          var presentationParameters = graphicsDevice.PresentationParameters;
          deviceNeedsReset = (width > presentationParameters.BackBufferWidth)
                             || (height > presentationParameters.BackBufferHeight);
          break;
      }

      if (deviceNeedsReset)
      {
        try
        {
          var presentationParameters = graphicsDevice.PresentationParameters;
          presentationParameters.BackBufferWidth = Math.Max(width, presentationParameters.BackBufferWidth);
          presentationParameters.BackBufferHeight = Math.Max(height, presentationParameters.BackBufferHeight);
          graphicsDevice.Reset(presentationParameters);
        }
        catch (Exception)
        {
          return false;
        }
      }
#endif

      context.Viewport = new Viewport(0, 0, width, height);
      _rendering = true;
      return true;
    }


    /// <inheritdoc/>
    void IPresentationTarget.EndRender(RenderContext context)
    {
      if (!_rendering)
        return;

#pragma warning disable 168
      // ReSharper disable EmptyGeneralCatchClause
      try
      {
        var graphicsDevice = GraphicsService.GraphicsDevice;

#if MONOGAME
        graphicsDevice.SetRenderTarget(null);
        context.RenderTarget = null;
        _renderTarget.Present();
#else
        int width = ClientSize.Width;
        int height = ClientSize.Height;
        var sourceRectangle = new Rectangle(0, 0, width, height);
        graphicsDevice.Present(sourceRectangle, null, Handle);
#endif
      }
      catch (Exception)
      {
        // Do nothing. This happens when the layout of the window changes during rendering.
        // For example, when the user docks windows an OutOfVideoMemoryException might occur.
      }
      finally
      {
        _rendering = false;
      }
      // ReSharper restore EmptyGeneralCatchClause
#pragma warning restore 168
    }


    /// <summary>
    /// Raises the <see cref="Control.Paint"/> event.
    /// </summary>
    /// <param name="e">
    /// The <see cref="PaintEventArgs"/> instance containing the event data.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnPaint(PaintEventArgs e)
    {
      if (DesignMode)
      {
        // Since the XNA graphics device presents into this control we do not really
        // need to draw anything here. But in the designer and when the control is
        // resized (or something similar happens), we want to draw something.
        e.Graphics.Clear(Color.Black);
      }

      base.OnPaint(e);
    }


    /// <summary>
    /// Raises the <see cref="Control.Click"/> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnClick(EventArgs e)
    {
      // Set focus to this control to receive keyboard window messages.
      Focus();

      base.OnClick(e);
    }


    /// <summary>
    /// Processes Windows messages.
    /// </summary>
    /// <param name="m">The Windows <see cref="Message"/> to process.</param>
    protected override void WndProc(ref Message m)
    {
      try
      {
        base.WndProc(ref m);
      }
      catch (InvalidOperationException exception)
      {
        // Under certain circumstances cross-thread problems between the WinForm and 
        // WPF thread can occur. For example with WM_SETCURSOR at the wrong time.
        Debug.WriteLine("Exception in FormsPresentationTarget.WndProc: " + exception.Message);
        return;
      }

      if (GraphicsService == null)
        return;

      // Forward messages to XNA game window, so that the XNA Keyboard gets the key 
      // inputs. WndProc is only called if the control has focus. See also comments 
      // in WpfEnvironment.
      if (m.Msg == WindowMessages.WM_KEYDOWN
          || m.Msg == WindowMessages.WM_KEYUP
          || m.Msg == WindowMessages.WM_CHAR
          || m.Msg == WindowMessages.WM_DEADCHAR
          || m.Msg == WindowMessages.WM_SYSKEYDOWN
          || m.Msg == WindowMessages.WM_SYSKEYUP
          || m.Msg == WindowMessages.WM_SYSCHAR
          || m.Msg == WindowMessages.WM_SYSDEADCHAR
          || m.Msg == WindowMessages.WM_MOUSEWHEEL)
      {
        Form form = (Form)GraphicsService.GameForm;
        if (form != null)
          NativeMethods.PostMessage(form.Handle, m.Msg, m.WParam, m.LParam);
      }
    }
    #endregion
  }
}
#endif
