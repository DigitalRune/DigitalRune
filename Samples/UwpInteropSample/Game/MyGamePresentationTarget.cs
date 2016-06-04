using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Interop;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;


namespace UwpInteropSample
{
  /// <summary>
  /// Shows the 3D game scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class is a XAML element. It is derived from <see cref="SwapChainPresentationTarget"/>
  /// (DigitalRune Graphics) which is derived from <see cref="SwapChainPanel"/> (UWP).
  /// </para>
  /// <para>
  /// When this element is loaded, it create a graphics screen and adds a 3D models. It also
  /// registers itself in the DigitalRune graphics service. 
  /// </para>
  /// <para>
  /// The <see cref="MyGamePresentationTarget"/> updates the field-of-view of the cameras when its
  /// size is changed (e.g. when the application window is resized).
  /// </para>
  /// </remarks>
  internal class MyGamePresentationTarget : SwapChainPresentationTarget
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private MyGraphicsScreen _graphicsScreen;
    #endregion


    //--------------------------------------------------------------
    #region Properties and Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the graphics screens that should be presented in this element.
    /// </summary>
    /// <value>The graphics screens that should be presented in this element.</value>
    public IList<GraphicsScreen> GraphicsScreens { get; private set; }
    #endregion



    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="MyGamePresentationTarget"/> class.
    /// </summary>
    public MyGamePresentationTarget()
    {
      if (!DesignMode.DesignModeEnabled)
      {
        GraphicsScreens = new List<GraphicsScreen>();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnLoaded(object sender, RoutedEventArgs eventArgs)
    {
      var game = ServiceLocator.Current.GetInstance<MyGame>();

      // Important: If the game loop is executed in a parallel thread, all accesses to game services
      // must be synchronized using a lock provided by the game:
      Lock = game.Lock;

      lock (Lock)
      {
        // Register the presentation target in the graphics service.
        var graphicsService = ServiceLocator.Current.GetInstance<IGraphicsService>();
        graphicsService.PresentationTargets.Add(this);

        // Create graphics screen.
        _graphicsScreen = new MyGraphicsScreen(GraphicsService);
        GraphicsService.Screens.Add(_graphicsScreen);
        GraphicsScreens.Add(_graphicsScreen);

        // Add a few models.
        CreateLevel();

        // Initialize the camera's projection matrix.
        UpdateCamera();
      }
    }


    private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
    {
      lock (Lock)
      {
        // Dispose scene objects.
        _graphicsScreen.Scene.Dispose(false);

        // Remove graphics screen.
        GraphicsScreens.Remove(_graphicsScreen);
        GraphicsService.Screens.Remove(_graphicsScreen);
        _graphicsScreen = null;

        // Unregister the presentation target from the graphics service.
        var graphicsService = ServiceLocator.Current.GetInstance<IGraphicsService>();
        graphicsService.PresentationTargets.Remove(this);
      }

      Lock = null;
    }


    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (GraphicsService == null)
        return;

      lock (Lock)
        UpdateCamera();
    }


    // Update the field-of-view of the camera to match the aspect ratio of this XAML element.
    private void UpdateCamera()
    {
      var cameraNode = _graphicsScreen.CameraNode;
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
          if (width > 0 && height > 0)
            orthographicProjection.Set(width, height);

          return;
        }

        var perspectiveProjection = projection as PerspectiveProjection;
        if (perspectiveProjection != null)
        {
          // Perspective camera.
          float height = perspectiveProjection.Height;
          float width = height * aspectRatio;
          if (width > 0 && height > 0)
            perspectiveProjection.Set(width, height);
        }
      }
    }


    // Load the "game level" consisting of some 3D models, lights, etc.
    private void CreateLevel()
    {
      var content = ServiceLocator.Current.GetInstance<ContentManager>();

      AddLights(_graphicsScreen.Scene);

      var groundModel = content.Load<ModelNode>("Ground/Ground").Clone();
      _graphicsScreen.Scene.Children.Add(groundModel);

      var tankModel = content.Load<ModelNode>("Tank/tank").Clone();
      _graphicsScreen.Scene.Children.Add(tankModel);
    }


    // Add light sources for standard three-point lighting.
    private static void AddLights(Scene scene)
    {
      var ambientLight = new AmbientLight
      {
        Color = new Vector3F(0.05333332f, 0.09882354f, 0.1819608f),
        Intensity = 1,
        HemisphericAttenuation = 0,
      };
      scene.Children.Add(new LightNode(ambientLight));

      var keyLight = new DirectionalLight
      {
        Color = new Vector3F(1, 0.9607844f, 0.8078432f),
        DiffuseIntensity = 1,
        SpecularIntensity = 1,
      };
      var keyLightNode = new LightNode(keyLight)
      {
        Name = "KeyLight",
        Priority = 10,   // This is the most important light.
        PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(-0.5265408f, -0.5735765f, -0.6275069f))),
      };
      scene.Children.Add(keyLightNode);

      var fillLight = new DirectionalLight
      {
        Color = new Vector3F(0.9647059f, 0.7607844f, 0.4078432f),
        DiffuseIntensity = 1,
        SpecularIntensity = 0,
      };
      var fillLightNode = new LightNode(fillLight)
      {
        Name = "FillLight",
        PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(0.7198464f, 0.3420201f, 0.6040227f))),
      };
      scene.Children.Add(fillLightNode);

      var backLight = new DirectionalLight
      {
        Color = new Vector3F(0.3231373f, 0.3607844f, 0.3937255f),
        DiffuseIntensity = 1,
        SpecularIntensity = 1,
      };
      var backLightNode = new LightNode(backLight)
      {
        Name = "BackLight",
        PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(0.4545195f, -0.7660444f, 0.4545195f))),
      };
      scene.Children.Add(backLightNode);
    }
    #endregion
  }
}