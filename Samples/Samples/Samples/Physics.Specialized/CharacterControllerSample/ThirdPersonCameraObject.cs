using System;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Input;


namespace Samples.Physics.Specialized
{
  /// <summary>
  /// Controls a camera that is attached to a <see cref="CharacterControllerObject"/>.
  /// </summary>
  /// <remarks>
  /// The camera can be behind the player (third-person) or in the player's head (first-person).
  /// </remarks>
  [Controls(@"Camera
  Use <Mouse Wheel> or <DPad Left>/<DPad Right> to control the camera distance.")]
  public class ThirdPersonCameraObject : GameObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IServiceLocator _services;
    private readonly IInputService _inputService;

    // The player to which the camera is attached.
    private readonly CharacterControllerObject _characterControllerObject;

    // Distance of camera to player's head. Set to 0 for first-person mode.
    private float _thirdPersonDistance = 3;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    public CameraNode CameraNode { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public ThirdPersonCameraObject(CharacterControllerObject characterControllerObject, IServiceLocator services)
    {
      Name = "ThirdPersonCamera";

      _characterControllerObject = characterControllerObject;

      _services = services;
      _inputService = services.GetInstance<IInputService>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    protected override void OnLoad()
    {
      var graphicsService = _services.GetInstance<IGraphicsService>();

      // Define camera projection.
      var projection = new PerspectiveProjection();
      projection.SetFieldOfView(
        ConstantsF.PiOver4,
        graphicsService.GraphicsDevice.Viewport.AspectRatio,
        0.1f,
        1000.0f);

      // Create a camera node.
      CameraNode = new CameraNode(new Camera(projection));
    }


    protected override void OnUnload()
    {
      CameraNode.Dispose(false);
      CameraNode = null;
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Mouse centering (controlled by the MenuComponent) is disabled if the game
      // is inactive or if the GUI is active. In these cases, we do not want to move
      // the player.
      if (!_inputService.EnableMouseCentering)
        return;

      // Mouse wheel, DPad up/down --> Change third-person camera distance.
      _thirdPersonDistance -= _inputService.MouseWheelDelta * 0.01f;
      if (_inputService.IsDown(Buttons.DPadLeft, LogicalPlayerIndex.One))
        _thirdPersonDistance -= 0.2f;
      if (_inputService.IsDown(Buttons.DPadRight, LogicalPlayerIndex.One))
        _thirdPersonDistance += 0.2f;

      _thirdPersonDistance = Math.Max(0, _thirdPersonDistance);

      // Get pose of the player. (This is the ground position, not the head position.)
      Pose pose = _characterControllerObject.Pose;

      // Create offset vector from player to the camera.
      Matrix33F orientation = pose.Orientation;
      Vector3F thirdPersonDistance = orientation * new Vector3F(0, 0, _thirdPersonDistance);

      // Compute camera position. 
      Vector3F eyeHeight = new Vector3F(0, _characterControllerObject.CharacterController.Height - 0.12f, 0);
      Vector3F position = pose.Position + eyeHeight + thirdPersonDistance;

      // Update SceneNode.LastPoseWorld - this is required for some effects, like
      // camera motion blur. 
      CameraNode.LastPoseWorld = CameraNode.PoseWorld;

      // Set the new camera pose.
      CameraNode.PoseWorld = new Pose(position, orientation);
    }
    #endregion
  }
}
