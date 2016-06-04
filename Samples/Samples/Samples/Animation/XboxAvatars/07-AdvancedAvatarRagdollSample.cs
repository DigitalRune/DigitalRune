#if XBOX
using Microsoft.Xna.Framework.Graphics;
using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using DigitalRune.Physics.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample demonstrates 4 ways of using ragdolls",
    @"Mode 1: 'Animated ragdoll with dynamic bodies'
  The ragdoll is controlled by a predefined animation. The ragdoll consists
  of dynamic rigid bodies, which can be used for collision detection and
  realistic hit reaction.
Mode 2: 'Animated ragdoll with kinematic bodies'
  The ragdoll is controlled by a predefined animation. The ragdoll consists
  of kinematic bodies, which can be used for collision detection and to 'push'
  other objects. The ragdoll can only apply forces to other objects, but does
  not react to external forces.
Mode 3: 'Passive ragdoll'
  The ragdoll is controlled by the physics simulation. The ragdoll consists
  of dynamic bodies, which are affected by external forces such as gravity.
Mode 4: 'Active ragdoll with motors'
  The ragdoll consists of dynamic rigid bodies. Constraint motors are used to
  drive the joints of the ragdoll. The ragdoll reacts to external forces, such
  as gravity, while still trying to follow a given animation.",
    107)]
  [Controls(@"Sample
  Press <A>, <B>, <X>, <Y> to switch modes.")]
  public class AdvancedAvatarRagdollSample : AnimationSample
  {
    private readonly DebugRenderer _debugRenderer;
    private readonly CameraObject _cameraObject;
    private readonly GrabObject _grabObject;
    private readonly BallShooterObject _ballShooterObject;

    private readonly AvatarDescription _avatarDescription;
    private readonly AvatarRenderer _avatarRenderer;

    private Pose _pose = new Pose(new Vector3F(-0.5f, 0, 0));

    // The _skeletonAnimation is one of the predefined key frame animations.
    // The _skeletonAnimation is applied to the _targetPose.
    // The _targetPose is used to drive the _ragdoll.
    // The _ragdoll determines the _avatarPose.
    // The _avatarPose is the actual skeleton pose that is being rendered.
    private ITimeline _expressionAnimation;
    private ITimeline _skeletonAnimation;
    private SkeletonPose _targetPose;
    private AvatarPose _avatarPose;
    private Ragdoll _ragdoll;

    private AnimationController _animationController0;
    private AnimationController _animationController1;

    // The following properties define the mode of the ragdoll.
    enum RagdollMode { Mode1, Mode2, Mode3, Mode4 };
    private RagdollMode _mode;

    private string _statusMessage = string.Empty;


    public AdvancedAvatarRagdollSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // This sample uses for a DebugRenderer to draw text and rigid bodies.
      _debugRenderer = new DebugRenderer(GraphicsService, SpriteFont)
      {
        DefaultColor = Color.Black,
        DefaultTextPosition = new Vector2F(10),
      };

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      _cameraObject.ResetPose(new Vector3F(0, 1, -3), ConstantsF.Pi, 0);
      GameObjectService.Objects.Add(_cameraObject);

      // Add some objects which allow the user to interact with the rigid bodies.
      _grabObject = new GrabObject(Services);
      _ballShooterObject = new BallShooterObject(Services) { Speed = 20 };
      GameObjectService.Objects.Add(_grabObject);
      GameObjectService.Objects.Add(_ballShooterObject);

      // Add some default force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Create a random avatar.
      _avatarDescription = AvatarDescription.CreateRandom();
      _avatarRenderer = new AvatarRenderer(_avatarDescription);

      // Use the "Wave" animation preset.
      var avatarAnimation = new AvatarAnimation(AvatarAnimationPreset.Wave);
      _expressionAnimation = new AnimationClip<AvatarExpression>(new WrappedAvatarExpressionAnimation(avatarAnimation))
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };
      _skeletonAnimation = new AnimationClip<SkeletonPose>(new WrappedAvatarSkeletonAnimation(avatarAnimation))
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };

      // Add a ground plane in the simulation.
      Simulation.RigidBodies.Add(new RigidBody(new PlaneShape(Vector3F.UnitY, 0)) { MotionType = MotionType.Static });

      // Distribute a few dynamic spheres and boxes across the landscape.
      SphereShape sphereShape = new SphereShape(0.3f);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-10, 10);
        position.Y = 1;
        Simulation.RigidBodies.Add(new RigidBody(sphereShape) { Pose = new Pose(position) });
      }

      BoxShape boxShape = new BoxShape(0.6f, 0.6f, 0.6f);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-10, 10);
        position.Y = 1;
        Simulation.RigidBodies.Add(new RigidBody(boxShape) { Pose = new Pose(position) });
      }
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _animationController0.Stop();
        _animationController1.Stop();

        if (_ragdoll != null)
          _ragdoll.RemoveFromSimulation();

        _debugRenderer.Dispose();
        _avatarRenderer.Dispose();
      }
      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      if (_avatarPose == null)
      {
        if (_avatarRenderer.State == AvatarRendererState.Ready)
        {
          _avatarPose = new AvatarPose(_avatarRenderer);
          _targetPose = SkeletonPose.Create(_avatarPose.SkeletonPose.Skeleton);

          // Create a ragdoll for the avatar.
          _ragdoll = Ragdoll.CreateAvatarRagdoll(_avatarPose, Simulation);

          // Set the world space pose of the whole ragdoll. And copy the bone poses 
          // of the current skeleton pose.
          _ragdoll.Pose = _pose;
          _ragdoll.UpdateBodiesFromSkeleton(_avatarPose.SkeletonPose);

          // To simplify collision checks, we need a simple way to determine whether
          // a rigid body belongs to the ragdoll.
          // --> Set RigidBody.UserData = _ragdoll.
          // (Alternatively we could also set specific names for the rigid bodies,
          // or we could assign the collision objects to a certain collision group.)
          foreach (var body in _ragdoll.Bodies)
            if (body != null)
              body.UserData = _ragdoll;

          // Add rigid bodies and constraints to the simulation.
          _ragdoll.AddToSimulation(Simulation);

          // Start by playing the key frame animation.
          SwitchMode(RagdollMode.Mode1);

          // The facial expression can be applied directly to the _avatarPose.
          _animationController0 = AnimationService.StartAnimation(_expressionAnimation, _avatarPose);

          // The skeletal animation is applied to the _targetPose. The _targetPose
          // is used to drive the ragdoll. (See end of method.)
          _animationController1 = AnimationService.StartAnimation(_skeletonAnimation, (IAnimatableProperty<SkeletonPose>)_targetPose);
        }

        return;
      }

      if (InputService.IsPressed(Buttons.A, false, LogicalPlayerIndex.One))
        SwitchMode(RagdollMode.Mode1);
      else if (InputService.IsPressed(Buttons.B, false, LogicalPlayerIndex.One))
        SwitchMode(RagdollMode.Mode2);
      else if (InputService.IsPressed(Buttons.X, false, LogicalPlayerIndex.One))
        SwitchMode(RagdollMode.Mode3);
      else if (InputService.IsPressed(Buttons.Y, false, LogicalPlayerIndex.One))
        SwitchMode(RagdollMode.Mode4);

      if (_mode == RagdollMode.Mode1 || _mode == RagdollMode.Mode2)
      {
        // The ragdoll plays a certain animation. Check whether the character was 
        // hit by a ball.
        foreach (var contactConstraint in Simulation.ContactConstraints)
        {
          if (contactConstraint.BodyA.UserData == _ragdoll && contactConstraint.BodyB.Name.StartsWith("Ball")
              || contactConstraint.BodyB.UserData == _ragdoll && contactConstraint.BodyA.Name.StartsWith("Ball"))
          {
            // Switch to the "Passive Ragdoll" mode and let the character collapse.
            SwitchMode(RagdollMode.Mode3);

            // Hint: You can read contactConstraint.LinearConstraintImpulse.Length to
            // determine the strength of the impact.
          }
        }
      }

      switch (_mode)
      {
        case RagdollMode.Mode1:
          // In mode 1 we update the rigid bodies directly.
          _ragdoll.UpdateBodiesFromSkeleton(_targetPose);
          break;
        case RagdollMode.Mode2:
          // Compute how much time the simulation will advance in the next Update().
          TimeSpan nextSimulationTimeStep;
          int numberOfSubTimeSteps;
          Simulation.GetNextTimeStep(gameTime.ElapsedGameTime, out nextSimulationTimeStep, out numberOfSubTimeSteps);

          // In mode 2 velocity motors update the rigid bodies.
          _ragdoll.DriveToPose(_targetPose, (float)nextSimulationTimeStep.TotalSeconds);
          break;
        case RagdollMode.Mode3:
          // In mode 3 we don't have to update the rigid bodies.
          break;
        case RagdollMode.Mode4:
          // In mode 4 constraint motors control the joints of the ragdoll.
          // (The second parameter is only required for velocity motors.)
          _ragdoll.DriveToPose(_targetPose, 0);
          break;
      }

      // Copy the skeleton pose. (_avatarPose stores the skeleton pose which is 
      // being rendered.)
      _ragdoll.UpdateSkeletonFromBodies(_avatarPose.SkeletonPose);

      _debugRenderer.Clear();
      _debugRenderer.DrawText("\n");
      _debugRenderer.DrawText(_statusMessage);

      // Render rigid bodies.
      foreach (var body in Simulation.RigidBodies)
        if (!(body.Shape is EmptyShape))  // Do not draw dummy bodies which might be used by the ragdoll.
          _debugRenderer.DrawObject(body, Color.Black, true, false);
    }


    private void SwitchMode(RagdollMode mode)
    {
      _mode = mode;

      switch (_mode)
      {
        case RagdollMode.Mode1:
          _statusMessage = "Mode: Animated ragdoll with dynamic bodies";

          // Make rigid bodies dynamic. The rigid bodies are affected by forces
          // and constraints.
          foreach (var body in _ragdoll.Bodies)
            if (body != null)
              body.MotionType = MotionType.Dynamic;

          _ragdoll.DisableMotors();
          _ragdoll.DisableJoints();
          _ragdoll.DisableLimits();
          break;

        case RagdollMode.Mode2:
          _statusMessage = "Mode: Animated ragdoll with kinematic bodies";

          // Make rigid bodies kinematic. The rigid bodies can push dynamic objects, 
          // but they are themselves not affected by forces.
          foreach (var body in _ragdoll.Bodies)
            if (body != null)
              body.MotionType = MotionType.Kinematic;

          // Set all motors to velocity motors. The velocity motors drive the rigid
          // bodies to absolute target positions.
          foreach (var motor in _ragdoll.Motors)
            if (motor != null)
              motor.Mode = RagdollMotorMode.Velocity;

          _ragdoll.EnableMotors();
          _ragdoll.DisableJoints();
          _ragdoll.DisableLimits();
          break;

        case RagdollMode.Mode3:
          _statusMessage = "Mode: Passive ragdoll (damping only)";

          // Make rigid bodies dynamic. The rigid bodies are affected by forces
          // and constraints.
          foreach (var body in _ragdoll.Bodies)
            if (body != null)
              body.MotionType = MotionType.Dynamic;

          // Set all motors to constraint motors that only use damping. This adds 
          // a damping effect to all ragdoll limbs, which slows down motions and
          // increases stability.
          foreach (var motor in _ragdoll.Motors)
          {
            if (motor != null)
            {
              motor.Mode = RagdollMotorMode.Constraint;
              motor.ConstraintDamping = 5;
              motor.ConstraintSpring = 0;
            }
          }

          _ragdoll.EnableMotors();
          _ragdoll.EnableJoints();
          _ragdoll.EnableLimits();
          break;

        case RagdollMode.Mode4:
          _statusMessage = "Mode: Active ragdoll with motors";

          // Make rigid bodies dynamic. The rigid bodies are affected by forces
          // and constraints.
          foreach (var body in _ragdoll.Bodies)
            if (body != null)
              body.MotionType = MotionType.Dynamic;

          // Set all motors to constraint motors. The motors control the joints
          // of the ragdoll.
          foreach (var motor in _ragdoll.Motors)
          {
            if (motor != null)
            {
              motor.Mode = RagdollMotorMode.Constraint;
              motor.ConstraintDamping = 1000000;
              motor.ConstraintSpring = 10000000;
            }
          }

          _ragdoll.EnableMotors();
          _ragdoll.EnableJoints();

          // It is recommended to disable limits in this mode. The limits may conflict
          // with the goals of the constraint motors. This could lead to instabilities.
          _ragdoll.DisableLimits();
          break;
      }
    }


    protected override void OnRender(RenderContext context)
    {
      // Set render context info.
      context.CameraNode = _cameraObject.CameraNode;

      // Clear screen.
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.CornflowerBlue);

      // Draw avatar.
      if (_avatarPose != null)
      {
        _avatarRenderer.World = _pose;
        _avatarRenderer.View = (Matrix)_cameraObject.CameraNode.View;
        _avatarRenderer.Projection = _cameraObject.CameraNode.Camera.Projection;
        _avatarRenderer.Draw(_avatarPose);
      }

      // Draw reticle.
      var viewport = context.Viewport;
      SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
      SpriteBatch.Draw(
        Reticle,
        new Vector2(viewport.Width / 2 - Reticle.Width / 2, viewport.Height / 2 - Reticle.Height / 2),
        Color.Black);
      SpriteBatch.End();

      _debugRenderer.Render(context);

      // Clean up.
      context.CameraNode = null;
    }
  }
}
#endif