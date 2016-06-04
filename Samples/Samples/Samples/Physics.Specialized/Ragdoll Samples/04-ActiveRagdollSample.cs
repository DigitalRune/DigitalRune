using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.Specialized;
using Microsoft.Xna.Framework;
using Samples.Animation;


namespace Samples.Physics.Specialized
{
  [Sample(SampleCategory.PhysicsSpecialized,
    "This sample creates an active (= animated) ragdoll.",
    "Constraint ragdoll motors are used to move the ragdoll limbs. The ragdoll also reacts to impacts.",
    14)]
  public class ActiveRagdollSample : CharacterAnimationSample
  {
    private BallShooterObject _ballShooterObject;
    private GrabObject _grabObject;
    private readonly MeshNode _meshNode;

    // This skeleton pose is animated. It defines the desired pose for the dude.
    private readonly SkeletonPose _targetSkeletonPose;

    private readonly Ragdoll _ragdoll;


    public ActiveRagdollSample(Microsoft.Xna.Framework.Game game)
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
      _meshNode.PoseLocal = new Pose(new Vector3F(0, 0, 0));
      SampleHelper.EnablePerPixelLighting(_meshNode);
      GraphicsScreen.Scene.Children.Add(_meshNode);

      // Create a copy of the dude's skeleton.
      _targetSkeletonPose = SkeletonPose.Create(_meshNode.Mesh.Skeleton);

      // Animate the _targetSkeletonPose.
      var animations = _meshNode.Mesh.Animations;
      var loopingAnimation = new AnimationClip<SkeletonPose>(animations.Values.First())
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };
      AnimationService.StartAnimation(loopingAnimation, (IAnimatableProperty)_targetSkeletonPose);

      // Create a ragdoll for the Dude model.
      _ragdoll = new Ragdoll();
      DudeRagdollCreator.Create(_targetSkeletonPose, _ragdoll, Simulation, 0.571f);

      // Set the world space pose of the whole ragdoll. And copy the bone poses of the
      // current skeleton pose.
      _ragdoll.Pose = _meshNode.PoseWorld;
      _ragdoll.UpdateBodiesFromSkeleton(_targetSkeletonPose);

      // In this sample we use an active ragdoll. We need joints because constraint ragdoll
      // motors only affect the body rotations.
      _ragdoll.EnableJoints();

      // We disable limits. If limits are enabled, the ragdoll could get unstable if 
      // the animation tries to move a limb beyond an allowed limit. (This happens if
      // a pose in the animation violates one of our limits.)
      _ragdoll.DisableLimits();

      // Set all motors to constraint motors. Constraint motors are like springs that
      // rotate the limbs to a target position.
      foreach (RagdollMotor motor in _ragdoll.Motors)
      {
        if (motor != null)
        {
          motor.Mode = RagdollMotorMode.Constraint;
          motor.ConstraintDamping = 10000;
          motor.ConstraintSpring = 100000;
        }
      }
      _ragdoll.EnableMotors();

      // Add rigid bodies and the constraints of the ragdoll to the simulation.
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

      CorrectWorldSpacePose();

      // Update the actual skeleton pose which is used during rendering.
      _ragdoll.UpdateSkeletonFromBodies(_meshNode.SkeletonPose);

      // Set the new motor targets using the animated pose.
      // (The second parameter is only required for velocity motors.)
      _ragdoll.DriveToPose(_targetSkeletonPose, 0);

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


    private void CorrectWorldSpacePose()
    {
      // Notes:
      // The Ragdoll class is simply a container for rigid bodies, joints, limits, motors, etc.
      // It has a Ragdoll.Pose property that determines the world space pose of the model.
      // The Ragdoll class does not update this property. It only reads it.
      // Let's say the ragdoll and model are created at the world space origin. Then the user
      // grabs the ragdoll and throws it 100 units away. Then the Ragdoll.Pose (and the root bone)
      // is still at the origin and the first body (the pelvis) is 100 units away. 
      // You can observe this if you comment out this method and look at the debug rendering of 
      // the skeleton.
      // To avoid this we correct the Ragdoll.Pose and make sure that it is always near the 
      // pelvis bone.

      var pelvis = _targetSkeletonPose.Skeleton.GetIndex("Pelvis");

      // This is different from the PassiveRagdollSample:
      // We use the _targetSkeletonPose to define the distance between pelvis and the Pose.
      var pelvisBonePoseAbsoluteInverse = _targetSkeletonPose.GetBonePoseAbsolute(pelvis).Inverse;

      _ragdoll.Pose = _ragdoll.Bodies[pelvis].Pose * _ragdoll.BodyOffsets[pelvis].Inverse * (Pose)pelvisBonePoseAbsoluteInverse;
      _meshNode.PoseWorld = _ragdoll.Pose;
    }
  }
}
