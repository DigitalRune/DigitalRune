using System.Linq;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample shows how to use a CcdIkSolver to let an arm reach for a target.",
    @"A CcdIkSolver modifies the arm of the model.
Limits are used to keep the palm of the hand parallel to the ground.",
    72)]
  [Controls(@"Sample
  Press <4>-<9> on the numpad to move the target.")]
  public class CcdIKSample : CharacterAnimationSample
  {
    private readonly MeshNode _meshNode;

    private Vector3F _targetPosition = new Vector3F(0.3f, 1, 0.3f);
    private readonly CcdIKSolver _ikSolver;


    public CcdIKSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var modelNode = ContentManager.Load<ModelNode>("Dude/Dude");
      _meshNode = modelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      _meshNode.PoseLocal = new Pose(new Vector3F(0, 0, 0));
      SampleHelper.EnablePerPixelLighting(_meshNode);
      GraphicsScreen.Scene.Children.Add(_meshNode);

      // Create the IK solver. The CCD method can solve long bone chains with 
      // limits. 
      _ikSolver = new CcdIKSolver
      {
        SkeletonPose = _meshNode.SkeletonPose,

        // The chain starts at the upper arm.
        RootBoneIndex = 13,

        // The chain ends at the hand bone.
        TipBoneIndex = 15,

        // The offset from the hand center to the hand origin.
        TipOffset = new Vector3F(0.1f, 0, 0),

        // We can set a bone gain less than 1 to get a "smoother" result in most cases -
        // at the cost of more iterations.
        //BoneGain = 0.9f,

        // This solver uses an iterative method and will make up to 100 iterations if necessary.
        NumberOfIterations = 10,

        // A method that applies bone limits.
        LimitBoneTransforms = LimitBoneTransform,
      };
    }


    private void LimitBoneTransform(int boneIndex)
    {
      // This method is called by the CcdIKSolver in each internal iteration after a bone
      // was modified.
      // The job of this method is to apply bone limits; for example, the elbow should not
      // bend backwards, etc. 
      // To apply a limit, get the bone transform or bone pose from skeleton pose, check if 
      // is in the allowed range. If it is outside the allowed range, rotate it back to the
      // nearest allowed rotation.

      // The parameter of this method is the boneIndex of the bone that was modified.
      // We can only apply the limit of this bone - or we can simply limit all bones 
      // all the time and ignore the parameter.

      // Here, for example, we only make sure that the palm of the hand is always parallel 
      // to the ground plane - as if the character wants to grab a horizontal bar or as 
      // if it wants to place the hand on horizontal plane.
      SrtTransform bonePoseAbsolute = _meshNode.SkeletonPose.GetBonePoseAbsolute(15);
      Vector3F palmAxis = bonePoseAbsolute.ToParentDirection(-Vector3F.UnitY);
      bonePoseAbsolute.Rotation = QuaternionF.CreateRotation(palmAxis, Vector3F.UnitY) * bonePoseAbsolute.Rotation;
      _meshNode.SkeletonPose.SetBonePoseAbsolute(15, bonePoseAbsolute);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // ----- Move target if <NumPad4-9> are pressed.
      Vector3F translation = new Vector3F();
      if (InputService.IsDown(Keys.NumPad4))
        translation.X -= 1;
      if (InputService.IsDown(Keys.NumPad6))
        translation.X += 1;
      if (InputService.IsDown(Keys.NumPad8))
        translation.Y += 1;
      if (InputService.IsDown(Keys.NumPad5))
        translation.Y -= 1;
      if (InputService.IsDown(Keys.NumPad9))
        translation.Z += 1;
      if (InputService.IsDown(Keys.NumPad7))
        translation.Z -= 1;

      translation = translation * deltaTime;
      _targetPosition += translation;

      // Convert target world space position to model space. - The IK solvers work in model space.
      Vector3F localTargetPosition = _meshNode.PoseWorld.ToLocalPosition(_targetPosition);

      // Reset the affected bones. This is optional. It removes unwanted twist from the bones.
      _meshNode.SkeletonPose.ResetBoneTransforms(_ikSolver.RootBoneIndex, _ikSolver.TipBoneIndex);

      // Let IK solver update the bones.
      _ikSolver.Target = localTargetPosition;
      _ikSolver.Solve(deltaTime);

      // Draws the IK target.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      debugRenderer.DrawAxes(new Pose(_targetPosition), 0.1f, false);
    }
  }
}
