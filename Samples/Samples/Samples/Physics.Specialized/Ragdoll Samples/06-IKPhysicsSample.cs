using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.Specialized;
using Microsoft.Xna.Framework;
using Samples.Animation;


namespace Samples.Physics.Specialized
{
  // Simulation for IK. Can use own timing.
  [Sample(SampleCategory.PhysicsSpecialized,
    @"This sample shows how to use the physics simulation for inverse kinematics.",
    @"You can use physics simulation to solve complex inverse kinematics (IK) problems:
Create a Simulation. Add rigid bodies for the bones of the IK chain. Connect the bodies
with constraints (joints and limits). Then apply forces or constraints to pull the bodies
to the IK targets. Execute a few simulations steps (Simulation.Update()). The constraint
solver of the physics simulation will move the bodies and solve the IK problem.

In this sample, we use the bodies and the constraints of the Dude ragdoll. A few selected
bodies (pelvis, hands and feet) are the IK target. You can use the reticle (left mouse
button) to grab the bodies and move them.",
    16)]
  public class IKPhysicsSample : CharacterAnimationSample
  {
    private GrabObject _grabObject;
    private readonly MeshNode _meshNode;
    private readonly Ragdoll _ragdoll;

    // The IK joints which pull the pelvis, hands and feet to the target positions.
    private List<Constraint> _ikJoints = new List<Constraint>();


    public IKPhysicsSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.DrawReticle = true;

      // Add game objects which allows to grab rigid bodies.
      _grabObject = new GrabObject(Services);
      GameObjectService.Objects.Add(_grabObject);

      // Add Dude model.
      var modelNode = ContentManager.Load<ModelNode>("Dude/Dude");
      _meshNode = modelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      _meshNode.PoseLocal = new Pose(new Vector3F(0, 0, 0));
      SampleHelper.EnablePerPixelLighting(_meshNode);
      GraphicsScreen.Scene.Children.Add(_meshNode);

      // Create a ragdoll for the Dude model.
      _ragdoll = new Ragdoll();
      DudeRagdollCreator.Create(_meshNode.SkeletonPose, _ragdoll, Simulation, 0.571f);

      // Set the initial world space pose of the whole ragdoll. And copy the bone poses of the
      // current skeleton pose.
      _ragdoll.Pose = _meshNode.PoseWorld;
      _ragdoll.UpdateBodiesFromSkeleton(_meshNode.SkeletonPose);

      // Enable constraints (joints and limits, no motors)
      _ragdoll.EnableJoints();
      _ragdoll.EnableLimits();
      _ragdoll.DisableMotors();

      foreach (var body in _ragdoll.Bodies)
      {
        if (body != null)
        {
          // Disable rigid body sleeping. (If we leave it enabled, the simulation might
          // disable slow bodies before they reach their IK goal.)
          body.CanSleep = false;

          // Disable collisions response.
          body.CollisionResponseEnabled = false;
        }
      }

      // Add rigid bodies and the constraints of the ragdoll to the simulation.
      _ragdoll.AddToSimulation(Simulation);

      // Disable all force effects (default gravity and damping).
      Simulation.ForceEffects.Clear();

      // Create constraints which hold selected bodies at their current position 
      // relative to the world.
      // To constrain the position + orientation, we use a FixedJoint.
      foreach (var boneName in new[] { "Pelvis" })
      {
        var ragdollBody = _ragdoll.Bodies[_meshNode.SkeletonPose.Skeleton.GetIndex(boneName)];
        var ikJoint = new FixedJoint
        {
          AnchorPoseALocal = ragdollBody.Pose,
          BodyA = Simulation.World,
          AnchorPoseBLocal = Pose.Identity,
          BodyB = ragdollBody,
          CollisionEnabled = false,
          MaxForce = 1000,
        };
        _ikJoints.Add(ikJoint);
        Simulation.Constraints.Add(ikJoint);
      }
      // To constrain only the position, we use a BallJoint.
      foreach(var boneName in new[] { "L_Hand", "R_Hand", "L_Ankle1", "R_Ankle" })
      {
        var ragdollBody = _ragdoll.Bodies[_meshNode.SkeletonPose.Skeleton.GetIndex(boneName)];
        var ikJoint = new BallJoint
        {
          AnchorPositionALocal = ragdollBody.Pose.Position,
          BodyA = Simulation.World,
          AnchorPositionBLocal = Vector3F.Zero,
          BodyB = ragdollBody,
          CollisionEnabled = false,
          MaxForce = 1000,
        };
        _ikJoints.Add(ikJoint);
        Simulation.Constraints.Add(ikJoint);
      }
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // Update Ragdoll.Pose and MeshNode.PoseWorld - see PassiveRagdollSample.
      PassiveRagdollSample.CorrectWorldSpacePose(_meshNode, _ragdoll);

      // Update model pose from rigid bodies.
      _ragdoll.UpdateSkeletonFromBodies(_meshNode.SkeletonPose);

      // Visualize rigid bodies.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      foreach (var body in Simulation.RigidBodies)
        debugRenderer.DrawObject(body, Color.Gray, true, false);

      // Draw bodies which are constrained by _ikJoints in a different color.
      foreach(var ikJoint in _ikJoints)
        debugRenderer.DrawObject(ikJoint.BodyB, new Color(0.7f, 0.7f, 0, 0.6f), false, false);

      // The GrabObject uses a constraint to let the user move bodies with the mouse.
      // When a body is grabbed, we disable the IK joint of this body to let it move.
      // When a body is released, we re-enable all IK joints to lock the bodies in
      // their current position.
      if (_grabObject.GrabbedBody != null)
      {
        foreach (var ikJoint in _ikJoints)
        {
          if (ikJoint.BodyB == _grabObject.GrabbedBody)
            ikJoint.Enabled = false;
        }
      }
      else
      {
        foreach (var ikJoint in _ikJoints)
        {
          if (!ikJoint.Enabled)
          {
            if (ikJoint is FixedJoint)
              ((FixedJoint)ikJoint).AnchorPoseALocal = ikJoint.BodyB.Pose;
            else
              ((BallJoint)ikJoint).AnchorPositionALocal = ikJoint.BodyB.Pose.Position;

            ikJoint.Enabled = true;
          }
        }
      }

      // Apply a strong damping to avoid instabilities.
      foreach (var body in _ragdoll.Bodies)
      {
        if (body != null)
        {
          body.LinearVelocity *= 0.1f;
          body.AngularVelocity *= 0.1f;
        }
      }
    }
  }
}
