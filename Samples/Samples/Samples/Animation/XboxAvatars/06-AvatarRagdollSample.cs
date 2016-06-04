#if XBOX
using Microsoft.Xna.Framework.Graphics;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using DigitalRune.Physics.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    "This sample shows how to create an ragdoll for an avatar.",
    "",
    106)]
  public class AvatarRagdollSample : AnimationSample
  {
    private readonly DebugRenderer _debugRenderer;
    private readonly CameraObject _cameraObject;
    private readonly GrabObject _grabObject;
    private readonly BallShooterObject _ballShooterObject;

    private readonly AvatarDescription _avatarDescription;
    private readonly AvatarRenderer _avatarRenderer;

    private AvatarPose _avatarPose;
    private Pose _pose = new Pose(new Vector3F(-0.5f, 0, 0));

    private Ragdoll _ragdoll;


    public AvatarRagdollSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // This sample uses for a DebugRenderer for the rendering rigid bodies.
      _debugRenderer = new DebugRenderer(GraphicsService, SpriteFont);

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

      // Add a ground plane in the simulation.
      Simulation.RigidBodies.Add(new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        MotionType = MotionType.Static
      });

      // Create a random avatar.
      _avatarDescription = AvatarDescription.CreateRandom();
      _avatarRenderer = new AvatarRenderer(_avatarDescription);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
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

          // Create a ragdoll for the avatar.
          _ragdoll = Ragdoll.CreateAvatarRagdoll(_avatarPose, Simulation);

          // Set the world space pose of the whole ragdoll. And copy the bone poses of the
          // current skeleton pose.
          _ragdoll.Pose = _pose;
          _ragdoll.UpdateBodiesFromSkeleton(_avatarPose.SkeletonPose);

          // In this sample we use a passive ragdoll where we need joints to hold the
          // limbs together and limits to control the angular movement.
          _ragdoll.EnableJoints();
          _ragdoll.EnableLimits();

          // Set all motors to constraint motors that only use damping. This adds a damping
          // effect to all ragdoll limbs.
          foreach (RagdollMotor motor in _ragdoll.Motors)
          {
            if (motor != null)
            {
              motor.Mode = RagdollMotorMode.Constraint;
              motor.ConstraintDamping = 5;
              motor.ConstraintSpring = 0;
            }
          }
          _ragdoll.EnableMotors();

          // Add rigid bodies and the constraints of the ragdoll to the simulation.
          _ragdoll.AddToSimulation(Simulation);
        }
      }
      else
      {
        // Copy skeleton pose from ragdoll.
        _ragdoll.UpdateSkeletonFromBodies(_avatarPose.SkeletonPose);
      }

      // Render rigid bodies.
      _debugRenderer.Clear();
      foreach (var body in Simulation.RigidBodies)
        if (!(body.Shape is EmptyShape))  // Do not draw dummy bodies which might be used by the ragdoll.
          _debugRenderer.DrawObject(body, Color.Black, true, false);
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