// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if WINDOWS
using System;
using System.Windows.Forms.Integration;


namespace DigitalRune.Graphics.Interop
{
  /// <summary>
  /// A WPF control that implements <see cref="IPresentationTarget"/> to host a 3D view.
  /// </summary>
  /// <remarks>
  /// <para>
  /// When a project uses the <see cref="ElementPresentationTarget"/> it needs to reference .NET
  /// assembly WindowsFormsIntegration.dll.
  /// </para>
  /// <para>
  /// Internally, a <see cref="Interop.FormsPresentationTarget"/> is hosted as the child of the 
  /// <see cref="WindowsFormsHost"/>.
  /// </para>
  /// </remarks>
  /// <example>
  /// <para>
  /// The following example shows how to register the <see cref="ElementPresentationTarget"/> and
  /// how to handle mouse events in a custom WPF window. (Note that the mouse events are Windows
  /// Form events, not WPF events.)
  /// </para>
  /// <code lang="xaml">
  /// <![CDATA[
  /// <Window x:Class="MyApplication.MyWindow"
  ///         xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  ///         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  ///         xmlns:dr="http://schemas.digitalrune.com/windows"
  ///         Title="My Window">
  ///   <Grid>
  ///     <!--  WPF presentation target into which XNA graphics can be rendered:  -->
  ///     <dr:ElementPresentationTarget x:Name="MyPresentationTarget" />
  ///   </Grid>
  /// </Window>
  /// ]]>
  /// </code>
  /// <code lang="csharp">
  /// <![CDATA[
  /// using System.ComponentModel;
  /// using System.Windows;
  /// using System.Windows.Forms;
  /// using DigitalRune.Graphics;
  /// 
  /// namespace MyApplication
  /// {
  ///   public partial class MyWindow
  ///   {
  ///     private IGraphicsService _graphicsService;
  /// 
  ///     public MyWindow(IGraphicsService graphicsService)
  ///     {
  ///       _graphicsService = graphicsService;
  /// 
  ///       InitializeComponent();
  ///       Loaded += OnLoaded;
  ///       MyPresentationTarget.Child.MouseDown += OnMouseDown;
  ///     }
  /// 
  ///     private void OnLoaded(object sender, RoutedEventArgs eventArgs)
  ///     {
  ///       // Register render targets.
  ///       _graphicsService.PresentationTargets.Add(MyPresentationTarget);
  ///     }
  /// 
  ///     protected override void OnClosing(CancelEventArgs eventArgs)
  ///     {
  ///       // Unregister render targets.
  ///       _graphicsService.PresentationTargets.Remove(PresentationTarget0);
  /// 
  ///       base.OnClosing(eventArgs);
  ///     }
  /// 
  ///     private void OnMouseDown(object sender, MouseEventArgs eventArgs)
  ///     {
  ///       // Handle mouse event here.
  ///     }
  ///   }
  /// }
  /// ]]>
  /// </code>
  /// </example>
  public class ElementPresentationTarget : WindowsFormsHost, IPresentationTarget
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly FormsPresentationTarget _formsPresentationTarget;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPresentationTarget"/> class.
    /// </summary>
    public ElementPresentationTarget()
    {
      _formsPresentationTarget = new FormsPresentationTarget();
      Child = _formsPresentationTarget;
    }
    #endregion


    //--------------------------------------------------------------
    #region IPresentationTarget Members
    //--------------------------------------------------------------

    /// <inheritdoc/>
    IGraphicsService IPresentationTarget.GraphicsService
    {
      get { return _formsPresentationTarget.GraphicsService; }
      set { ((IPresentationTarget)_formsPresentationTarget).GraphicsService = value; }
    }


    /// <summary>
    /// Gets the graphics service.
    /// </summary>
    /// <value>The graphics service.</value>
    /// <inheritdoc cref="IPresentationTarget.GraphicsService"/>
    public IGraphicsService GraphicsService
    {
      get { return _formsPresentationTarget.GraphicsService; }
    }


    /// <inheritdoc/>
    IntPtr IPresentationTarget.Handle
    {
      get
      {
        if (!_formsPresentationTarget.IsDisposed)
          return _formsPresentationTarget.Handle;

        return IntPtr.Zero;
      }
    }


    /// <inheritdoc/>
    int IPresentationTarget.Width
    {
      get { return _formsPresentationTarget.Width; }
    }


    /// <inheritdoc/>
    int IPresentationTarget.Height
    {
      get { return _formsPresentationTarget.Height; }
    }


    /// <inheritdoc/>
    bool IPresentationTarget.BeginRender(RenderContext context)
    {
      return ((IPresentationTarget)_formsPresentationTarget).BeginRender(context);
    }


    /// <inheritdoc/>
    void IPresentationTarget.EndRender(RenderContext context)
    {
      ((IPresentationTarget)_formsPresentationTarget).EndRender(context);
    }
    #endregion
  }
}
#endif
