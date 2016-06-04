using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Samples.Animation;


namespace Samples.Physics.Specialized
{
  [Sample(SampleCategory.PhysicsSpecialized,
    @"This sample shows how to use the Ragdoll class to manage collision objects for animated models.",
    @"In this sample we create a ragdoll. Only the bodies are used. Collision response is disabled.
Joints, limits and motors are not needed.
The method Update() moves the bodies using the Ragdoll.UpdateBodiesFromSkeleton() method and 
it checks if a ball shot by the user collides with the head. If the ball hits the head, the 
background color changes to red.",
    11)]
  [Controls(@"Sample
  Press <Space> to display model in bind pose.")]
  public class CollisionDetectionOnlyRagdollSample : CharacterAnimationSample
  {
    private BallShooterObject _ballShooterObject;
    private readonly MeshNode _meshNode;
    private readonly Ragdoll _ragdoll;


    public CollisionDetectionOnlyRagdollSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.DrawReticle = true;
      SetCamera(new Vector3F(0, 1, 6), 0, 0);

      // Add a game object which allows to shoot balls.
      _ballShooterObject = new BallShooterObject(Services);
      GameObjectService.Objects.Add(_ballShooterObject);

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
      AnimationService.StartAnimation(loopingAnimation, (IAnimatableProperty)_meshNode.SkeletonPose);

      // Create a ragdoll for the Dude model.
      _ragdoll = new Ragdoll();
      DudeRagdollCreator.Create(_meshNode.SkeletonPose, _ragdoll, Simulation, 0.571f);

      // Set the world space pose of the whole ragdoll. 
      _ragdoll.Pose = _meshNode.PoseWorld;
      // And copy the bone poses of the current skeleton pose.
      _ragdoll.UpdateBodiesFromSkeleton(_meshNode.SkeletonPose);

      foreach (var body in _ragdoll.Bodies)
      {
        if (body != null)
        {
          // Set all bodies to kinematic - they should not be affected by forces.
          body.MotionType = MotionType.Kinematic;

          // Disable collision response.
          body.CollisionResponseEnabled = false;
        }
      }

      // In this sample, we do not need joints, limits or motors.
      _ragdoll.DisableJoints();
      _ragdoll.DisableLimits();
      _ragdoll.DisableMotors();

      // Add ragdoll rigid bodies to the simulation.
      _ragdoll.AddToSimulation(Simulation);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // <Space> --> Reset skeleton pose to bind pose.
      if (InputService.IsDown(Keys.Space))
        _meshNode.SkeletonPose.ResetBoneTransforms();

      // ----- Detect collisions
      // Get all contact sets of the head.
      RigidBody headBody = _ragdoll.Bodies[7];
      var headContacts = Simulation.CollisionDomain.GetContacts(headBody.CollisionObject);
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      foreach (var contactSet in headContacts)
      {
        // Get the rigid body that collided with the head.
        RigidBody otherBody;
        if (contactSet.ObjectA.GeometricObject == headBody)
          otherBody = contactSet.ObjectB.GeometricObject as RigidBody;
        else
          otherBody = contactSet.ObjectA.GeometricObject as RigidBody;

        // If the head collided with a ball (from the BallShooter.cs), then set the hit flag.
        if (otherBody != null && otherBody.Name.StartsWith("Ball"))
        {
          debugRenderer.DrawText("\n\nHIT DETECTED!!!");
          GraphicsScreen.BackgroundColor = Color.DarkRed;
          break;
        }
      }

      // Move model in a circle.
      var rotation = Matrix33F.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds * 0.3f);
      _meshNode.PoseWorld = new Pose(rotation * new Vector3F(3, 0, 0), rotation);

      // Sync pose with ragdoll.
      _ragdoll.Pose = _meshNode.PoseWorld;

      // Update the bodies to match the current skeleton pose.
      // This method changes the body poses directly. 
      _ragdoll.UpdateBodiesFromSkeleton(_meshNode.SkeletonPose);

      // Use DebugRenderer to visualize rigid bodies.
      foreach (var body in Simulation.RigidBodies)
      {
        if (body.Name.StartsWith("Ball"))
          debugRenderer.DrawObject(body, Color.Gray, false, false);
        else
          debugRenderer.DrawObject(body, Color.Gray, true, false);
      }
    }
  }
}
