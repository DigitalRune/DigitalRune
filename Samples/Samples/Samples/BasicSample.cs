using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace Samples
{
  // The class BasicSample is an abstract base class that automatically registers
  // a SampleGraphicsScreen for rendering in the graphics service. 
  // The SampleGraphicsScreen provides a DebugRenderer for 2D output, a DebugRenderer
  // for 3D output and a Scene. The Scene supports MeshNodes, BillboardNodes, and
  // ParticleSystemNodes (other types of scene nodes are not supported).
  public abstract class BasicSample : Sample
  {
    private CameraObject _cameraObject;

    protected SampleGraphicsScreen GraphicsScreen { get; private set; }


    protected BasicSample(Microsoft.Xna.Framework.Game game) 
      : base(game)
    {
      // Create a graphics screen for rendering basic stuff.
      GraphicsScreen = new SampleGraphicsScreen(Services) { ClearBackground = true, };

      // The order of the graphics screens is back-to-front. Add the screen at index 0,
      // i.e. behind all other screens. The screen should be rendered first and all other
      // screens (menu, GUI, help, ...) should be on top.
      GraphicsService.Screens.Insert(0, GraphicsScreen);

      // GameObjects that need to render stuff will retrieve the DebugRenderers or
      // Scene through the service provider.
      Services.Register(typeof(DebugRenderer), null, GraphicsScreen.DebugRenderer);
      Services.Register(typeof(DebugRenderer), "DebugRenderer2D", GraphicsScreen.DebugRenderer2D);
      Services.Register(typeof(IScene), null, GraphicsScreen.Scene);

      // Add a default light setup (ambient light + 3 directional lights).
      var defaultLightsObject = new DefaultLightsObject(Services);
      GameObjectService.Objects.Add(defaultLightsObject);
    }


    // Adds a 3D camera and positions it using position and yaw/pitch angles [rad].
    protected void SetCamera(Vector3F position, float yaw, float pitch)
    {
      if (_cameraObject == null)
      {
        _cameraObject = new CameraObject(Services);
        GameObjectService.Objects.Add(_cameraObject);
        GraphicsScreen.CameraNode = _cameraObject.CameraNode;
      }

      _cameraObject.ResetPose(position, yaw, pitch);
    }
  }
}
