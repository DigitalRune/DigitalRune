using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    "This sample shows how to cross-fade between animations.",
    "",
    55)]
  [Controls(@"Sample
  Press <Space> to play 'Shoot' animation.
  Press <Up> to play 'Run' animation.")]
  public class CharacterCrossFadeSample : CharacterAnimationSample
  {
    private readonly MeshNode _meshNode;

    private readonly ITimeline _idleAnimation;
    private AnimationController _idleAnimationController;
    private readonly ITimeline _runAnimation;
    private AnimationController _runAnimationController;
    private readonly TimelineGroup _aimAndShootAnimation;
    private AnimationController _aimAndShootAnimationController;


    public CharacterCrossFadeSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var modelNode = ContentManager.Load<ModelNode>("Marine/PlayerMarine");
      _meshNode = modelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      SampleHelper.EnablePerPixelLighting(_meshNode);
      GraphicsScreen.Scene.Children.Add(_meshNode);

      Dictionary<string, SkeletonKeyFrameAnimation> animations = _meshNode.Mesh.Animations;

      // Create a looping 'Idle' animation.
      _idleAnimation = new AnimationClip<SkeletonPose>(animations["Idle"])
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };

      // Create a looping 'Run' animation.
      _runAnimation = new AnimationClip<SkeletonPose>(animations["Run"])
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };

      // Combine the 'Aim' and 'Shoot' animation. The 'Aim' animation should start immediately. 
      // The 'Shoot' animation should start after 0.3 seconds.
      // (Animations can be combined by creating timeline groups. All timelines/animations 
      // in a timeline group are played simultaneously. AnimationClips can be used to 
      // arrange animations on a timeline. The property Delay, for example, can be used to
      // set the begin time.)
      _aimAndShootAnimation = new TimelineGroup();
      _aimAndShootAnimation.Add(animations["Aim"]);
      _aimAndShootAnimation.Add(new AnimationClip<SkeletonPose>(animations["Shoot"]) { Delay = TimeSpan.FromSeconds(0.3) });

      // Start 'Idle' animation. We use a Replace transition with a fade-in.
      _idleAnimationController = AnimationService.StartAnimation(
        _idleAnimation,
        (IAnimatableProperty)_meshNode.SkeletonPose,
        AnimationTransitions.Replace(TimeSpan.FromSeconds(0.5)));

      _idleAnimationController.AutoRecycle();
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <Space> --> Cross-fade to 'Aim-and-Shoot' animation.
      if (InputService.IsPressed(Keys.Space, false))
      {
        // Start the new animation using a Replace transition with a fade-in time.
        _aimAndShootAnimationController = AnimationService.StartAnimation(
          _aimAndShootAnimation,
          (IAnimatableProperty)_meshNode.SkeletonPose,
          AnimationTransitions.Replace(TimeSpan.FromSeconds(0.1)));

        _aimAndShootAnimationController.AutoRecycle();
      }

      // <Up> --> Cross-fade to 'Run' animation - unless the 'Aim-and-Shoot' animation is playing
      // or the 'Run' animation is already playing.
      if (_aimAndShootAnimationController.State != AnimationState.Playing
          && _runAnimationController.State != AnimationState.Playing
          && InputService.IsDown(Keys.Up))
      {
        _runAnimationController = AnimationService.StartAnimation(
          _runAnimation,
          (IAnimatableProperty)_meshNode.SkeletonPose,
          AnimationTransitions.Replace(TimeSpan.FromSeconds(0.2)));

        _runAnimationController.AutoRecycle();
      }

      if (_aimAndShootAnimationController.State != AnimationState.Playing)
      {
        // If none of the animations are playing, or if the user releases the <Up> key,
        // then restart the 'Idle' animation.
        if (_runAnimationController.State != AnimationState.Playing && _idleAnimationController.State != AnimationState.Playing
            || _runAnimationController.State == AnimationState.Playing && InputService.IsUp(Keys.Up))
        {
          _idleAnimationController = AnimationService.StartAnimation(
            _idleAnimation,
            (IAnimatableProperty)_meshNode.SkeletonPose,
            AnimationTransitions.Replace(TimeSpan.FromSeconds(0.2)));

          _idleAnimationController.AutoRecycle();
        }
      }
    }
  }
}
