using System;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Input;


namespace Samples
{
  // Shoots a ball when a button is pressed. (No graphics model is added. A DebugRenderer
  // needs to be used to render the rigid bodies.)
  [Controls(@"Shoot Ball
  Press <Right Mouse> or <Right Trigger> to shoot a ball.")]
  public class BallShooterObject : GameObject
  {
    private readonly IInputService _inputService;
    private readonly Simulation _simulation;
    private readonly IGameObjectService _gameObjectService;

    // Prepare a number of balls which can be reused as ammunition.
    private RigidBody[] _balls;
    private int _nextBallIndex;

    public float Speed { get; set; }


    public BallShooterObject(IServiceLocator services)
    {
      Name = "BallShooter";
      Speed = 100;

      _inputService = services.GetInstance<IInputService>();
      _simulation = services.GetInstance<Simulation>();
      _gameObjectService = services.GetInstance<IGameObjectService>();
    }


    // OnLoad() is called when the GameObject is added to the IGameObjectService.
    protected override void OnLoad()
    {
      // Prepare n balls.
      const int n = 10;
      _balls = new RigidBody[n];
      var sphereShape = new SphereShape(0.25f);
      for (int i = 0; i < _balls.Length; i++)
      {
        _balls[i] = new RigidBody(sphereShape)  // Note: All rigid bodies share the same shape.
        {
          // Assign a name. (Just for debugging.)
          Name = "Ball" + i,

          // The balls are shot with a high velocity. We need to enable "Continuous Collision 
          // Detection" - otherwise, we could miss some collision.
          CcdEnabled = true,
        };
      }
    }


    // OnUnload() is called when the GameObject is removed from the IGameObjectService.
    protected override void OnUnload()
    {
      // Remove all balls from simulation.
      foreach(var ball in _balls)
        _simulation.RigidBodies.Remove(ball);
    }


    // OnUpdate() is called once per frame.
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Fire ball if right mouse button or right trigger is pressed.
      if (_inputService.IsPressed(MouseButtons.Right, true) 
          || _inputService.IsPressed(Buttons.RightTrigger, true, LogicalPlayerIndex.One))
      {
        RigidBody ball = _balls[_nextBallIndex];

        // Remove ball from physics simulation in case it has already been used.
        if (ball.Simulation != null)
          ball.Simulation.RigidBodies.Remove(ball);

        // Get the forward direction of the camera.
        var cameraGameObject = (CameraObject)_gameObjectService.Objects["Camera"];
        var cameraNode = cameraGameObject.CameraNode;
        Pose cameraPose = cameraNode.PoseWorld;
        Vector3F forward = cameraPose.ToWorldDirection(Vector3F.Forward);

        // Place the ball at the position of the camera and shoot forward by directly
        // setting the velocity.
        ball.Pose = cameraPose;
        ball.LinearVelocity = forward * Speed;

        // Add the ball to the physics simulation.
        _simulation.RigidBodies.Add(ball);

        // Select next ball in array.
        _nextBallIndex = (_nextBallIndex + 1) % _balls.Length;
      }
    }
  }
}
