using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.Specialized;
using Microsoft.Xna.Framework;
using Samples.Animation;


namespace Samples.Physics.Specialized
{
  [Sample(SampleCategory.PhysicsSpecialized,
    @"This sample shows how to use the Ragdoll class to interact with other rigid bodies.",
    @"In this sample we create a ragdoll. The rigid bodies are kinematic - therefore, they push 
other rigid bodies but cannot be pushed themselves.
Ragdoll motors (in Velocity mode) are used to set the velocities of the rigid bodies.
In Update() the method Ragdoll.DriveToPose() is called in each frame to update the motor 
target positions. Ragdoll joints and limits are not used.",
    12)]
  public class KinematicRagdollSample : CharacterAnimationSample
  {
    private BallShooterObject _ballShooterObject;
    private GrabObject _grabObject;
    private readonly MeshNode _meshNode;
    private readonly Ragdoll _ragdoll;


    public KinematicRagdollSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.DrawReticle = true;

      // Add game objects which allow to shoot balls and grab rigid bodies.
      _ballShooterObject = new BallShooterObject(Services) { Speed = 10 };
      GameObjectService.Objects.Add(_ballShooterObject);
      _grabObject = new GrabObject(Services);
      GameObjectService.Objects.Add(_grabObject);

      var modelNode = ContentManager.Load<ModelNode>("Dude/Dude");
      _meshNode = modelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      _meshNode.PoseLocal = new Pose(new Vector3F(0, 0, 0), Matrix33F.CreateRotationY(ConstantsF.Pi));
      SampleHelper.EnablePerPixelLighting(_meshNode);
      GraphicsScreen.Scene.Children.Add(_meshNode);

      var animations = _meshNode.Mesh.Animations;
      var loopingAnimation = new AnimationClip<SkeletonPose>(animations.Values.First())
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };
      var animationController = AnimationService.StartAnimation(loopingAnimation, (IAnimatableProperty)_meshNode.SkeletonPose);
      animationController.UpdateAndApply();

      // Create a ragdoll for the Dude model.
      _ragdoll = new Ragdoll();
      DudeRagdollCreator.Create(_meshNode.SkeletonPose, _ragdoll, Simulation, 0.571f);

      // Set the world space pose of the whole ragdoll. And copy the bone poses of the
      // current skeleton pose.
      _ragdoll.Pose = _meshNode.PoseWorld;
      _ragdoll.UpdateBodiesFromSkeleton(_meshNode.SkeletonPose);

      // Set all bodies to kinematic -  they should not be affected by forces.
      foreach (var body in _ragdoll.Bodies)
      {
        if (body != null)
        {
          body.MotionType = MotionType.Kinematic;
        }
      }

      // Set all motors to velocity motors. Velocity motors change RigidBody.LinearVelocity
      // RigidBody.AngularVelocity to move the rigid bodies. 
      foreach (RagdollMotor motor in _ragdoll.Motors)
      {
        if (motor != null)
        {
          motor.Mode = RagdollMotorMode.Velocity;
        }
      }
      _ragdoll.EnableMotors();

      // In this sample, we do not need joints or limits.
      _ragdoll.DisableJoints();
      _ragdoll.DisableLimits();

      // Add ragdoll rigid bodies to the simulation.
      _ragdoll.AddToSimulation(Simulation);

      // Add a rigid body.
      var box = new RigidBody(new BoxShape(0.4f, 0.4f, 0.4f))
      {
        Name = "Box",
        Pose = new Pose(new Vector3F(0, 3, 0)),
      };
      Simulation.RigidBodies.Add(box);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // Compute how much time the simulation will advance in the next Update().
      TimeSpan nextSimulationTimeStep;
      int numberOfSubTimeSteps;
      Simulation.GetNextTimeStep(gameTime.ElapsedGameTime, out nextSimulationTimeStep, out numberOfSubTimeSteps);

      // Update the motors. The velocity motors will modify the rigid body velocities.
      _ragdoll.DriveToPose(_meshNode.SkeletonPose, (float)nextSimulationTimeStep.TotalSeconds);

      // Use DebugRenderer to visualize rigid bodies.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      foreach (var body in Simulation.RigidBodies)
      {
        if (body.Name.StartsWith("Ball") || body.Name.StartsWith("Box"))
          debugRenderer.DrawObject(body, Color.Gray, false, false);
        else
          debugRenderer.DrawObject(body, Color.Gray, true, false);
      }
    }
  }
}
