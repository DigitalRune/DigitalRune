using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    "This sample shows how to make a character look at a specific target.",
    "Several bones (starting at the spine, up to the head) are affected by LookAtIKISolvers.",
    70)]
  [Controls(@"Sample
  Press <4>-<9> on the numpad to move the target.")]
  public class LookAtIKSample : CharacterAnimationSample
  {
    private readonly MeshNode _meshNode;

    private Vector3F _targetPosition = new Vector3F(-1, 0, 0);

    // The IK solver - one per affected bone.
    private readonly LookAtIKSolver _spine1IK;
    private readonly LookAtIKSolver _spine2IK;
    private readonly LookAtIKSolver _spine3IK;
    private readonly LookAtIKSolver _neckIK;
    private readonly LookAtIKSolver _headIK;


    public LookAtIKSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
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

      // Create LookAtIKSolvers for some spine bones, the neck and the head.

      _spine1IK = new LookAtIKSolver
      {
        SkeletonPose = _meshNode.SkeletonPose,
        BoneIndex = 3,

        // The bone space axis that points in look direction.
        Forward = Vector3F.UnitY,

        // The bone space axis that points in up direction
        Up = Vector3F.UnitX,

        // An arbitrary rotation limit.
        Limit = ConstantsF.PiOver4,

        // We use a weight of 1 for the head, and lower weights for all other bones. Thus, most
        // of the looking will be done by the head bone, and the influence on the other bones is
        // smaller.
        Weight = 0.2f,

        // It is important to set the EyeOffsets. If we do not set EyeOffsets, the IK solver 
        // assumes that the eyes are positioned in the origin of the bone. 
        // Approximate EyeOffsets are sufficient.
        EyeOffset = new Vector3F(0.8f, 0, 0),
      };

      _spine2IK = new LookAtIKSolver
      {
        SkeletonPose = _meshNode.SkeletonPose,
        BoneIndex = 4,
        Forward = Vector3F.UnitY,
        Up = Vector3F.UnitX,
        Limit = ConstantsF.PiOver4,
        Weight = 0.2f,
        EyeOffset = new Vector3F(0.64f, 0, 0),
      };

      _spine3IK = new LookAtIKSolver
      {
        SkeletonPose = _meshNode.SkeletonPose,
        BoneIndex = 5,
        Forward = Vector3F.UnitY,
        Up = Vector3F.UnitX,
        Limit = ConstantsF.PiOver4,
        Weight = 0.3f,
        EyeOffset = new Vector3F(0.48f, 0, 0),
      };

      _neckIK = new LookAtIKSolver
      {
        SkeletonPose = _meshNode.SkeletonPose,
        BoneIndex = 6,
        Forward = Vector3F.UnitY,
        Up = Vector3F.UnitX,
        Limit = ConstantsF.PiOver4,
        Weight = 0.4f,
        EyeOffset = new Vector3F(0.32f, 0, 0),
      };

      _headIK = new LookAtIKSolver
      {
        SkeletonPose = _meshNode.SkeletonPose,
        BoneIndex = 7,
        Forward = Vector3F.UnitY,
        Up = Vector3F.UnitX,
        EyeOffset = new Vector3F(0.16f, 0.16f, 0),
        Weight = 1.0f,
        Limit = ConstantsF.PiOver4,
      };
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

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

      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
      translation = translation * deltaTime;
      _targetPosition += translation;

      // Convert target world space position to model space.
      // (The IK solvers work in model space.)
      Vector3F localTargetPosition = _meshNode.PoseWorld.ToLocalPosition(_targetPosition);

      // Update the IK solver target positions.
      _spine1IK.Target = localTargetPosition;
      _spine2IK.Target = localTargetPosition;
      _spine3IK.Target = localTargetPosition;
      _neckIK.Target = localTargetPosition;
      _headIK.Target = localTargetPosition;

      // Run the IK solvers. - This immediately modifies the affected bones. Therefore, 
      // it is important to run the solvers in the correct order (from parent to child 
      // bone).
      _spine1IK.Solve(deltaTime);
      _spine2IK.Solve(deltaTime);
      _spine3IK.Solve(deltaTime);
      _neckIK.Solve(deltaTime);
      _headIK.Solve(deltaTime);

      // Draws the IK target.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      debugRenderer.DrawAxes(new Pose(_targetPosition), 0.1f, false);
    }
  }
}
