using System.Linq;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Samples.Animation;


namespace Samples.Physics.Specialized
{
  [Sample(SampleCategory.PhysicsSpecialized,
    @"This sample creates a passive, damped ragdoll.",
    @"Constraint ragdoll motors (damping only) are used to create a damping effect, which removes
a lot of unwanted jitter.",
    13)]
  [Controls(@"Sample
  Press <Space> to toggle rendering of constraints.")]
  public class PassiveRagdollSample : CharacterAnimationSample
  {
    private BallShooterObject _ballShooterObject;
    private GrabObject _grabObject;
    private readonly MeshNode _meshNode;
    private readonly Ragdoll _ragdoll;
    private bool _drawConstraints = true;


    public PassiveRagdollSample(Microsoft.Xna.Framework.Game game)
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

      // Create a ragdoll for the Dude model.
      _ragdoll = new Ragdoll();
      DudeRagdollCreator.Create(_meshNode.SkeletonPose, _ragdoll, Simulation, 0.571f);

      // Set the world space pose of the whole ragdoll. And copy the bone poses of the
      // current skeleton pose.
      _ragdoll.Pose = _meshNode.PoseWorld;
      _ragdoll.UpdateBodiesFromSkeleton(_meshNode.SkeletonPose);

      // Uncomment to disable dynamic movement (for debugging during ragdoll creation):
      //foreach (var body in _ragdoll.Bodies)
      //  if (body != null)
      //    body.MotionType = MotionType.Kinematic;

      // In this sample we use a passive ragdoll where we need joints to hold the
      // limbs together and limits to restrict angular movement.
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

      if (InputService.IsPressed(Keys.Space, true))
        _drawConstraints = !_drawConstraints;

      CorrectWorldSpacePose(_meshNode, _ragdoll);

      // Update the skeleton bone transforms from the current rigid body positions.
      _ragdoll.UpdateSkeletonFromBodies(_meshNode.SkeletonPose);

      // Use DebugRenderer to visualize rigid bodies and constraints.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      foreach (var body in Simulation.RigidBodies)
      {
        if (body.Name.StartsWith("Ball") || body.Name.StartsWith("Box"))
          debugRenderer.DrawObject(body, Color.Gray, false, false);
        else
          debugRenderer.DrawObject(body, Color.Gray, true, false);
      }
      if (_drawConstraints)
        debugRenderer.DrawConstraints(_ragdoll, 0.1f, true);
    }


    internal static void CorrectWorldSpacePose(MeshNode meshNode, Ragdoll ragdoll)
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

      int pelvis = meshNode.SkeletonPose.Skeleton.GetIndex("Pelvis");
      SrtTransform pelvisBindPoseAbsoluteInverse = meshNode.SkeletonPose.Skeleton.GetBindPoseAbsoluteInverse(pelvis);
      ragdoll.Pose = ragdoll.Bodies[pelvis].Pose * ragdoll.BodyOffsets[pelvis].Inverse * (Pose)pelvisBindPoseAbsoluteInverse;
      meshNode.PoseWorld = ragdoll.Pose;
    }
  }
}
