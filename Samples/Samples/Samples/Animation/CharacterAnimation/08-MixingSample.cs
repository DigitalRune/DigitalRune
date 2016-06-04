using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample shows how to mix animations on one skeleton.",
    @"The lower body parts play an 'Idle' or a 'Run' animation. The upper body parts play a
'Shoot' animation.",
    58)]
  [Controls(@"Sample
  Press <Up> to play 'Run' animation.")]
  public class MixingSample : CharacterAnimationSample
  {
    private readonly MeshNode _meshNode;

    private readonly AnimationClip<SkeletonPose> _idleAnimation;
    private readonly AnimationClip<SkeletonPose> _runAnimation;
    private AnimationController _runAnimationController;
    private AnimationController _idleAnimationController;

    private bool _isRunning;


    public MixingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var modelNode = ContentManager.Load<ModelNode>("Marine/PlayerMarine");
      _meshNode = modelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      SampleHelper.EnablePerPixelLighting(_meshNode);
      GraphicsScreen.Scene.Children.Add(_meshNode);

      var animations = _meshNode.Mesh.Animations;
      _runAnimation = new AnimationClip<SkeletonPose>(animations["Run"])
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };
      _idleAnimation = new AnimationClip<SkeletonPose>(animations["Idle"])
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };

      // Create a 'Shoot' animation that only affects the upper body.
      var shootAnimation = animations["Shoot"];

      // The SkeletonKeyFrameAnimations allows to set a weight for each bone channel. 
      // For the 'Shoot' animation, we set the weight to 0 for all bones that are 
      // not descendants of the second spine bone (bone index 2). That means, the 
      // animation affects only the upper body bones and is disabled on the lower 
      // body bones.
      for (int i = 0; i < _meshNode.Mesh.Skeleton.NumberOfBones; i++)
      {
        if (!SkeletonHelper.IsAncestorOrSelf(_meshNode.SkeletonPose, 2, i))
          shootAnimation.SetWeight(i, 0);
      }

      var loopedShootingAnimation = new AnimationClip<SkeletonPose>(shootAnimation)
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };

      // Start 'Idle' animation.
      _idleAnimationController = AnimationService.StartAnimation(_idleAnimation, (IAnimatableProperty)_meshNode.SkeletonPose);
      _idleAnimationController.AutoRecycle();

      // Start looping the 'Shoot' animation. We use a Compose transition. This will add the 
      // 'Shoot' animation to the animation composition chain and keeping all other playing 
      // animations.
      // The 'Idle' animation animates the whole skeleton. The 'Shoot' animation replaces 
      // the 'Idle' animation on the bones of the upper body.
      AnimationService.StartAnimation(loopedShootingAnimation,
                                      (IAnimatableProperty)_meshNode.SkeletonPose,
                                      AnimationTransitions.Compose()
                                     ).AutoRecycle();
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      if (InputService.IsDown(Keys.Up))
      {
        if (!_isRunning)
        {
          _isRunning = true;

          // Start 'Run' animation. We use a Replace transition and replace the 'Idle' 
          // animation which is the first in the animation composition chain. Since we only
          // replace one specific animation, the 'Shoot' animation will stay in the composition
          // chain and keep playing.
          _runAnimationController = AnimationService.StartAnimation(
            _runAnimation,
            (IAnimatableProperty)_meshNode.SkeletonPose,
            AnimationTransitions.Replace(_idleAnimationController.AnimationInstance, TimeSpan.FromSeconds(0.3)));
          _runAnimationController.AutoRecycle();
        }
      }
      else
      {
        if (_isRunning)
        {
          _isRunning = false;

          // Start 'Idle' animation and replace the 'Run' animation.
          _idleAnimationController = AnimationService.StartAnimation(
             _idleAnimation,
             (IAnimatableProperty)_meshNode.SkeletonPose,
             AnimationTransitions.Replace(_runAnimationController.AnimationInstance, TimeSpan.FromSeconds(0.3)));
          _idleAnimationController.AutoRecycle();
        }
      }
    }
  }
}
