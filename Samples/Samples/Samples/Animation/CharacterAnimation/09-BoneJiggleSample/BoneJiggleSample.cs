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
    @"This sample shows how to make a bone-based jiggle effect.",
    @"Here, the jiggle effect is applied to the head bone - but it should also be useful for
other jiggle effects. ;-)",
    59)]
  [Controls(@"Sample
  Press <Space> to pause/resume the animation and watch the head.
  Press <Enter> to reset the jiggle effect.")]
  public class BoneJiggleSample : CharacterAnimationSample
  {
    private readonly MeshNode _meshNode;

    private AnimationController _walkAnimationController;

    private readonly BoneJiggler _boneJiggler;


    public BoneJiggleSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var modelNode = ContentManager.Load<ModelNode>("Dude/Dude");
      _meshNode = modelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      _meshNode.PoseLocal = new Pose(new Vector3F(0, 0, 0), Matrix33F.CreateRotationY(ConstantsF.Pi));
      SampleHelper.EnablePerPixelLighting(_meshNode);
      GraphicsScreen.Scene.Children.Add(_meshNode);

      var animations = _meshNode.Mesh.Animations;
      var walkAnimation = new AnimationClip<SkeletonPose>(animations.Values.First())
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };
      _walkAnimationController = AnimationService.StartAnimation(walkAnimation, (IAnimatableProperty)_meshNode.SkeletonPose);
      _walkAnimationController.AutoRecycle();

      // Create a BoneJiggler instance for the head bone (bone index 7).
      _boneJiggler = new BoneJiggler(_meshNode.SkeletonPose, 7, new Vector3F(1.1f, 0, 0))
      {
        Spring = 100,
        Damping = 3,
      };
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _walkAnimationController.Stop();
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <Space> --> Pause/Resume animation.
      if (InputService.IsPressed(Keys.Space, false))
      {
        if (_walkAnimationController.IsPaused)
          _walkAnimationController.Resume();
        else
          _walkAnimationController.Pause();
      }

      // <Enter> --> Reset BoneJiggler.
      if (InputService.IsDown(Keys.Enter))
        _boneJiggler.Reset();

      // Update BoneJiggler. This will change the bone transform of the affected bone.
      var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
      _boneJiggler.Update(deltaTime, _meshNode.PoseWorld);
    }
  }
}
