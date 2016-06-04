#if KINECT
using System.Linq;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.Specialized;
using Samples.Animation;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;


namespace Samples.Kinect
{
  [Sample(SampleCategory.Kinect,
    @"This sample application shows how to use Kinect and ragdolls to animate 3D human models in
real-time.",
    @"To animate the Dude, a 'Marionette' approach is used: A ragdoll is created for the model
that is used to animated the model. Then we use selected joint positions (hands, elbows,
neck, knees, ankles) of the Kinect player skeleton as target positions. With BallJoint
constraints we pull ragdoll bodies to the target positions. These BallJoints act as the
'strings' of our marionette.
The pelvis body is kinematic and is positioned directly at the Kinect SpineBase position.

Only the first player in front of the Kinect sensor is used. (No model for the second player.)

The advantage of this approach is that the physics simulation will make sure that the ragdoll
limits are not violated by bad Kinect input. The disadvantage is:
Since the skeleton of the Dude model is very different from the Kinect player skeleton,
it is hard to make this approach stable. This sample is only quick proof-of-concept. To get
better results, we need a model that has a similar bone hierarchy and the same proportions
as the Kinect body. The constraint parameters (MaxForce, ErrorReduction, Softness) of the 
marionette constraints and the ragdoll constraints could also deserve more tweaking.
We could also add more constraints to stabilize the ragdoll.",
    2)]
  [Controls(@"Sample
  Press <NumPad 7>/<NumPad 4> to increase/decrease Kinect sensor height.
  Press <NumPad 8>/<NumPad 5> to increase/decrease y offset of Kinect skeleton.
  Press <NumPad 9>/<NumPad 6> to increase/decrease scale of Kinect skeleton.
  Press <NumPad +>/<NumPad -> to increase/decrease the filter strength.
  Press <NumPad 0> to toggle drawing of Kinect skeletons.
  Press <NumPad 1> to toggle drawing of Kinect skeletons.
  Press <NumPad 2> to toggle drawing of the model skeletons (green).
  Press <NumPad 3> to toggle drawing of the ragdoll.
  Press <NumPad Decimal Point> to toggle drawing of the joint limits.")]
  class KinectRagdollMarionetteSample : BasicSample
  {
    private readonly KinectWrapper _kinectWrapper;
    
    // The height of the Kinect sensor position. In our case it was 0.8 m above the floor.
    private float _kinectSensorHeight = 0.8f;

    private MeshNode _meshNode;
    private Ragdoll _ragdoll;

    // "Marionette constraints":
    // BallJoints constraints that pull important joints of the ragdoll to the target positions
    // All these joints will use a small MaxForce because these constraints should be weaker than 
    // the normal ragdoll joints and limits.
    private BallJoint _headSpring;
    private BallJoint _elbowLeftSpring;
    private BallJoint _handLeftSpring;
    private BallJoint _elbowRightSpring;
    private BallJoint _handRightSpring;
    private BallJoint _kneeLeftSpring;
    private BallJoint _ankleLeftSpring;
    private BallJoint _kneeRightSpring;
    private BallJoint _ankleRightSpring;

    private bool _drawModel = true;
    private bool _drawKinectSkeletons = true;
    private bool _drawModelSkeletons;
    private bool _drawConstraints;
    private bool _drawRigidBodies;


    public KinectRagdollMarionetteSample(Game game)
      : base(game)
    {
      SetCamera(new Vector3F(0, 1, 1), 0, 0);

      // Add a background object.
      GameObjectService.Objects.Add(new SandboxObject(Services));

      // Add a KinectWrapper which controls the Kinect device.
      _kinectWrapper = new KinectWrapper(game);
      game.Components.Add(_kinectWrapper);

      InitializeModelAndRagdoll();
      InitializeMarionetteConstraints();
    }


    private void InitializeModelAndRagdoll()
    {
      // Load Dude model.
      var contentManager = Services.GetInstance<ContentManager>();
      var dudeModelNode = contentManager.Load<ModelNode>("Dude/Dude");
      _meshNode = dudeModelNode.GetSubtree().OfType<MeshNode>().First().Clone();
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

      // Disable sleeping.
      foreach (var body in _ragdoll.Bodies)
      {
        if (body != null)
        {
          body.CanSleep = false;
          //body.CollisionResponseEnabled = false;
        }
      }

      // The pelvis bone (index 1) is updated directly from the Kinect hip center.
      _ragdoll.Bodies[1].MotionType = MotionType.Kinematic;

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
          motor.ConstraintDamping = 100;
          motor.ConstraintSpring = 0;
        }
      }
      _ragdoll.EnableMotors();

      // Add rigid bodies and the constraints of the ragdoll to the simulation.
      _ragdoll.AddToSimulation(Simulation);
    }


    private void InitializeMarionetteConstraints()
    {
      // Create constraints that pull important body parts to Kinect joint positions.
      // The Update() method below will update the BallJoint.AnchorPositionALocal vectors.

      // Limit the maximal forces that these joints can apply. We do not want these joints to be
      // so strong that they can violate the ragdoll joint and limit constraints. Increasing
      // this force makes the ragdoll more responsive but can also violate ragdoll constraints
      // (e.g. by stretching the limbs).
      const float maxForce = 1000;

      var ragdollSkeleton = _meshNode.SkeletonPose.Skeleton;

      _headSpring = new BallJoint
      {
        BodyA = Simulation.World,
        BodyB = _ragdoll.Bodies[ragdollSkeleton.GetIndex("Head")],
        MaxForce = maxForce,
      };
      Simulation.Constraints.Add(_headSpring);
      _elbowLeftSpring = new BallJoint
      {
        BodyA = Simulation.World,
        BodyB = _ragdoll.Bodies[ragdollSkeleton.GetIndex("L_Forearm")],
        MaxForce = maxForce / 2,    // Elbow springs are weaker because the correct 
        // hand position is more important and the hand 
        // constraint should therefore be stronger.
      };
      // This constraint should be attached at the elbow position and not at the center of the forearm:
      var elbowLeftJointPosition = _meshNode.SkeletonPose.GetBonePoseAbsolute(ragdollSkeleton.GetIndex("L_Forearm")).Translation;
      _elbowLeftSpring.AnchorPositionBLocal = _elbowLeftSpring.BodyB.Pose.ToLocalPosition(elbowLeftJointPosition);
      Simulation.Constraints.Add(_elbowLeftSpring);

      _handLeftSpring = new BallJoint
      {
        BodyA = Simulation.World,
        BodyB = _ragdoll.Bodies[ragdollSkeleton.GetIndex("L_Hand")],
        MaxForce = maxForce,
      };
      Simulation.Constraints.Add(_handLeftSpring);

      _elbowRightSpring = new BallJoint
      {
        BodyA = Simulation.World,
        BodyB = _ragdoll.Bodies[ragdollSkeleton.GetIndex("R_Forearm")],
        MaxForce = maxForce / 2,
      };
      // This constraint should be attached at the elbow position and not at the center of the forearm:
      var elbowRightJointPosition = _meshNode.SkeletonPose.GetBonePoseAbsolute(ragdollSkeleton.GetIndex("R_Forearm")).Translation;
      _elbowRightSpring.AnchorPositionBLocal = _elbowRightSpring.BodyB.Pose.ToLocalPosition(elbowRightJointPosition);
      Simulation.Constraints.Add(_elbowRightSpring);

      _handRightSpring = new BallJoint
      {
        BodyA = Simulation.World,
        BodyB = _ragdoll.Bodies[ragdollSkeleton.GetIndex("R_Hand")],
        MaxForce = maxForce,
      };
      Simulation.Constraints.Add(_handRightSpring);

      _kneeLeftSpring = new BallJoint
      {
        BodyA = Simulation.World,
        BodyB = _ragdoll.Bodies[ragdollSkeleton.GetIndex("L_Knee2")],
        MaxForce = maxForce,
      };
      // This constraint should be attached at the knee position and not at the center of the lower leg:
      var kneeLeftJointPosition = _meshNode.SkeletonPose.GetBonePoseAbsolute(ragdollSkeleton.GetIndex("L_Knee2")).Translation;
      _kneeLeftSpring.AnchorPositionBLocal = _kneeLeftSpring.BodyB.Pose.ToLocalPosition(kneeLeftJointPosition);
      Simulation.Constraints.Add(_kneeLeftSpring);

      _ankleLeftSpring = new BallJoint
      {
        BodyA = Simulation.World,
        BodyB = _ragdoll.Bodies[ragdollSkeleton.GetIndex("L_Ankle1")],
        MaxForce = maxForce,
      };
      Simulation.Constraints.Add(_ankleLeftSpring);

      _kneeRightSpring = new BallJoint
      {
        BodyA = Simulation.World,
        BodyB = _ragdoll.Bodies[ragdollSkeleton.GetIndex("R_Knee")],
        MaxForce = maxForce,
      };
      // This constraint should be attached at the knee position and not at the center of the lower leg:
      var kneeRightJointPosition = _meshNode.SkeletonPose.GetBonePoseAbsolute(ragdollSkeleton.GetIndex("R_Knee")).Translation;
      _kneeRightSpring.AnchorPositionBLocal = _kneeRightSpring.BodyB.Pose.ToLocalPosition(kneeRightJointPosition);
      Simulation.Constraints.Add(_kneeRightSpring);

      _ankleRightSpring = new BallJoint
      {
        BodyA = Simulation.World,
        BodyB = _ragdoll.Bodies[ragdollSkeleton.GetIndex("R_Ankle")],
        MaxForce = maxForce,
      };
      Simulation.Constraints.Add(_ankleRightSpring);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Clean up.
        Game.Components.Remove(_kinectWrapper);
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      HandleInput(deltaTime);

      CorrectWorldSpacePose();

      // Update the skeleton bone transforms from the current rigid body positions.
      var skeletonPose = _meshNode.SkeletonPose;
      _ragdoll.UpdateSkeletonFromBodies(skeletonPose);

      // Since not all joints are constrained by the Kinect skeleton, we might want 
      // to show some bones always in a specific pose if they move too much.
      skeletonPose.SetBoneTransform(skeletonPose.Skeleton.GetIndex("Head"), SrtTransform.Identity);

      _meshNode.IsEnabled = _drawModel;

      if (_kinectWrapper.IsTrackedA)
      {
        // The Kinect position (0, 0, 0) is at the Kinect sensor and not on the floor. In this
        // sample the floor is at height 0. Therefore, we add a vertical offset to the Kinect 
        // positions.
        var offset = new Vector3F(0, _kinectSensorHeight, 0);

        // The new pelvis position. (We keep the original rotation and use the Kinect position.)
        var kinectSkeletonPose = _kinectWrapper.SkeletonPoseA;
        var newPose = new Pose(kinectSkeletonPose.GetBonePoseAbsolute(0).Translation + offset)
                      * new Pose(skeletonPose.Skeleton.GetBindPoseRelative(1).Rotation.Inverse);

        // If the new position is too far away from the last position, then we limit the 
        // position change. If the ragdoll makes very large jumps in a single step, then 
        // it could get tangled up.
        var oldPose = _ragdoll.Bodies[1].Pose; // Pelvis has bone index 1.
        var translation = newPose.Position - oldPose.Position;
        const float maxTranslation = 0.1f;
        if (translation.Length > maxTranslation)
        {
          translation.Length = maxTranslation;
          newPose.Position = oldPose.Position + translation;
        }        
        _ragdoll.Bodies[1].Pose = newPose;

        // ----- Update the target positions for the animation joints.        

        var kinectSkeleton = kinectSkeletonPose.Skeleton;
        var kinectHeadPosition = kinectSkeletonPose.GetBonePoseAbsolute(kinectSkeleton.GetIndex("Neck")).Translation + offset;
        // The ragdoll torso cannot bend sideways. Correct target position to avoid sideway pull.
        kinectHeadPosition.X = newPose.Position.X;
        _headSpring.AnchorPositionALocal = kinectHeadPosition;
        
        // In this sample, the model on the screen should act like a mirror for the players'
        // movements. Therefore, we mirror the skeletons, e.g. the right Kinect arm controls left 
        // model arm.
        _elbowLeftSpring.AnchorPositionALocal = kinectSkeletonPose.GetBonePoseAbsolute(kinectSkeleton.GetIndex("ElbowRight")).Translation + offset;
        _handLeftSpring.AnchorPositionALocal = kinectSkeletonPose.GetBonePoseAbsolute(kinectSkeleton.GetIndex("HandRight")).Translation + offset;
        _elbowRightSpring.AnchorPositionALocal = kinectSkeletonPose.GetBonePoseAbsolute(kinectSkeleton.GetIndex("ElbowLeft")).Translation + offset;
        _handRightSpring.AnchorPositionALocal = kinectSkeletonPose.GetBonePoseAbsolute(kinectSkeleton.GetIndex("HandLeft")).Translation + offset;
        _kneeLeftSpring.AnchorPositionALocal = kinectSkeletonPose.GetBonePoseAbsolute(kinectSkeleton.GetIndex("KneeRight")).Translation + offset;
        _ankleLeftSpring.AnchorPositionALocal = kinectSkeletonPose.GetBonePoseAbsolute(kinectSkeleton.GetIndex("AnkleRight")).Translation + offset;
        _kneeRightSpring.AnchorPositionALocal = kinectSkeletonPose.GetBonePoseAbsolute(kinectSkeleton.GetIndex("KneeLeft")).Translation + offset;
        _ankleRightSpring.AnchorPositionALocal = kinectSkeletonPose.GetBonePoseAbsolute(kinectSkeleton.GetIndex("AnkleLeft")).Translation + offset;
      }

      DrawDebugInfo();
    }


    private void HandleInput(float deltaTime)
    {
      // Change sensor height.
      if (InputService.IsDown(Keys.NumPad7))
        _kinectSensorHeight += 0.1f * deltaTime;
      if (InputService.IsDown(Keys.NumPad4))
        _kinectSensorHeight -= 0.1f * deltaTime;

      // Change y offset of Kinect skeleton data.
      if (InputService.IsDown(Keys.NumPad8))
        _kinectWrapper.Offset = _kinectWrapper.Offset + new Vector3F(0, 0.1f * deltaTime, 0);
      if (InputService.IsDown(Keys.NumPad5))
        _kinectWrapper.Offset = _kinectWrapper.Offset - new Vector3F(0, 0.1f * deltaTime, 0);

      // Change scale of Kinect skeleton data.
      if (InputService.IsDown(Keys.NumPad9))
        _kinectWrapper.Scale = _kinectWrapper.Scale + new Vector3F(0.1f * deltaTime);
      if (InputService.IsDown(Keys.NumPad6))
        _kinectWrapper.Scale = _kinectWrapper.Scale - new Vector3F(0.1f * deltaTime);

      // Toggle drawing of model.
      if (InputService.IsPressed(Keys.NumPad0, false))
        _drawModel = !_drawModel;

      // Toggle drawing of Kinect skeletons.
      if (InputService.IsPressed(Keys.NumPad1, false))
        _drawKinectSkeletons = !_drawKinectSkeletons;

      // Toggle drawing of model skeletons.
      if (InputService.IsPressed(Keys.NumPad2, false))
        _drawModelSkeletons = !_drawModelSkeletons;

      // Toggle drawing of ragdoll bodies.
      if (InputService.IsPressed(Keys.NumPad3, false))
        _drawRigidBodies = !_drawRigidBodies;

      // Toggle drawing of ragdoll constraints.
      if (InputService.IsPressed(Keys.Decimal, false))
        _drawConstraints = !_drawConstraints;
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

      int pelvis = _meshNode.SkeletonPose.Skeleton.GetIndex("Pelvis");
      SrtTransform pelvisBindPoseAbsoluteInverse = _meshNode.SkeletonPose.Skeleton.GetBindPoseAbsoluteInverse(pelvis);
      _ragdoll.Pose = _ragdoll.Bodies[pelvis].Pose * _ragdoll.BodyOffsets[pelvis].Inverse * (Pose)pelvisBindPoseAbsoluteInverse;
      _meshNode.PoseWorld = _ragdoll.Pose;
    }


    private void DrawDebugInfo()
    {
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      if (!_kinectWrapper.IsRunning)
      {
        // Kinect not found. Draw error message.
        debugRenderer.DrawText("\n\nKINECT SENSOR IS NOT AVAILABLE! PLEASE MAKE SURE THE DEVICE IS CONNECTED.");
        return;
      }

      // Draw Kinect skeletons for debugging.
      if (_drawKinectSkeletons)
      {
        var pose = new Pose(new Vector3F(0, _kinectSensorHeight, 0));
        debugRenderer.DrawSkeleton(_kinectWrapper.SkeletonPoseA, pose, Vector3F.One, 0.1f, Color.Orange, true);
      }

      // Draw model skeletons of tracked players for debugging.
      if (_drawModelSkeletons)
        debugRenderer.DrawSkeleton(_meshNode, 0.1f, Color.GreenYellow, true);

      // Use DebugRenderer to visualize rigid bodies and constraints.
      if (_drawRigidBodies)
        foreach (var body in _ragdoll.Bodies)
          debugRenderer.DrawObject(body, Color.Gray, false, false);

      if (_drawConstraints)
        debugRenderer.DrawConstraints(_ragdoll, 0.1f, true);

      // Draw Kinect settings info.
      debugRenderer.DrawText("\n\nKinect sensor height (<NumPad 7>/<NumPad 4>): " + _kinectSensorHeight
        + "\nKinect skeleton Y offset (<NumPad 8>/<NumPad 5>): " + _kinectWrapper.Offset.Y
        + "\nKinect skeleton scale (<NumPad 9>/<NumPad 6>): " + _kinectWrapper.Scale.X);
    }
  }
}
#endif