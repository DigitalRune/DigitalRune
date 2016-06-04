using System.Linq;
using System.Windows;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Interop;
using Microsoft.Practices.ServiceLocation;


namespace WpfInteropSample2
{
  /// <summary>
  /// Shows the 3D game scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="MyGamePresentationTarget"/> is a WPF element that shows the 3D scene. When a 
  /// <see cref="MyGamePresentationTarget"/> is loaded it is automatically registered in the
  /// DigitalRune graphics service. 
  /// </para>
  /// <para>
  /// The <see cref="MyGamePresentationTarget"/> updates the field-of-view of the cameras when its 
  /// size is changed (e.g. when the WPF window is resized).
  /// </para>
  /// </remarks>
  internal class MyGamePresentationTarget : D3DImagePresentationTarget
  {
    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="MyGamePresentationTarget"/> class.
    /// </summary>
    public MyGamePresentationTarget()
    {
      if (!WindowsHelper.IsInDesignMode)
      {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnLoaded(object sender, RoutedEventArgs eventArgs)
    {
      if (GraphicsService == null)
      {
        // Register the presentation target in the graphics service.
        var graphicsService = ServiceLocator.Current.GetInstance<IGraphicsService>();
        graphicsService.PresentationTargets.Add(this);
      }
    }


    private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
    {
      if (GraphicsService != null)
      {
        // Unregister the presentation target from the graphics service.
        var graphicsService = ServiceLocator.Current.GetInstance<IGraphicsService>();
        graphicsService.PresentationTargets.Remove(this);
      }
    }


    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);

      // Update the field-of-view of the cameras of all graphics screens.
      var graphicsService = ServiceLocator.Current.GetInstance<IGraphicsService>();
      foreach (var screen in graphicsService.Screens.OfType<MyGraphicsScreen>())
      {
        var cameraNode = screen.CameraNode;
        if (cameraNode != null)
        {
          float aspectRatio = (float)ActualWidth / (float)ActualHeight;

          // Get projection from camera and adjust field-of-view.
          var projection = cameraNode.Camera.Projection;

          var orthographicProjection = projection as OrthographicProjection;
          if (orthographicProjection != null)
          {
            // Orthographic camera.
            float height = orthographicProjection.Height;
            float width = height * aspectRatio;
            orthographicProjection.Set(width, height);
            continue;
          }

          var perspectiveProjection = projection as PerspectiveProjection;
          if (perspectiveProjection != null)
          {
            // Perspective camera.
            float height = perspectiveProjection.Height;
            float width = height * aspectRatio;
            perspectiveProjection.Set(width, height);
          }
        }
      }
    }
    #endregion
  }
}