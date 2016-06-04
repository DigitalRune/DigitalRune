#if XBOX
using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;

namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample shows how to use custom animations loaded from the content pipeline.",
    @"The animation are processed using a custom processor: See AvatarAnimationProcessor.cs
The user can start animations. After the animation has finished, the avatar returns to the
stand animation.
In this sample, we keep the AnimationController instances that are returned by
AnimationService.StartAnimation(). The AnimationControllers are used to check which animation
is playing.",
    104)]
  [Controls(@"Sample
  Press <A>, <B>, <X>, <Y> to start animations.
  Press <Left Trigger> to start a walk animation and control the walk speed.")]
  public class CustomAvatarAnimationSample : AnimationSample
  {
    private readonly CameraObject _cameraObject;

    private readonly AvatarDescription _avatarDescription;
    private readonly AvatarRenderer _avatarRenderer;

    private AvatarPose _avatarPose;
    private Pose _pose = new Pose(new Vector3F(-0.5f, 0, 0));

    private TimelineClip _standAnimation;
    private AnimationController _standAnimationController;

    private TimelineGroup _faintAnimation;
    private TimelineGroup _jumpAnimation;
    private TimelineGroup _kickAnimation;
    private TimelineGroup _punchAnimation;
    private AnimationController _actionAnimationController;

    private TimelineClip _walkAnimation;
    private AnimationController _walkAnimationController;


    public CustomAvatarAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      _cameraObject.ResetPose(new Vector3F(0, 1, -3), ConstantsF.Pi, 0);
      GameObjectService.Objects.Add(_cameraObject);

      // Create a random avatar.
      _avatarDescription = AvatarDescription.CreateRandom();
      _avatarRenderer = new AvatarRenderer(_avatarDescription);

      // Wrap the Stand0 AvatarAnimationPreset (see WrappedAnimationSample) to create an
      // infinitely looping stand animation.
      AvatarAnimation standAnimationPreset = new AvatarAnimation(AvatarAnimationPreset.Stand0);
      TimelineGroup standAnimation = new TimelineGroup
      {
        new WrappedAvatarExpressionAnimation(standAnimationPreset),
        new WrappedAvatarSkeletonAnimation(standAnimationPreset),
      };
      _standAnimation = new TimelineClip(standAnimation)
      {
        LoopBehavior = LoopBehavior.Cycle,  // Cycle the Stand animation...
        Duration = TimeSpan.MaxValue,       // ...forever.
      };

      // Load animations from content pipeline.
      _faintAnimation = ContentManager.Load<TimelineGroup>("XboxAvatars/Faint");
      _jumpAnimation = ContentManager.Load<TimelineGroup>("XboxAvatars/Jump");
      _kickAnimation = ContentManager.Load<TimelineGroup>("XboxAvatars/Kick");
      _punchAnimation = ContentManager.Load<TimelineGroup>("XboxAvatars/Punch");

      // The walk cycle should loop: Put it into a timeline clip and set a
      // loop-behavior.
      TimelineGroup walkAnimation = ContentManager.Load<TimelineGroup>("XboxAvatars/Walk");
      _walkAnimation = new TimelineClip(walkAnimation)
      {
        LoopBehavior = LoopBehavior.Cycle,  // Cycle the Walk animation...
        Duration = TimeSpan.MaxValue,       // ...forever.
      };
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _avatarRenderer.Dispose();
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      if (_avatarPose == null)
      {
        if (_avatarRenderer.State == AvatarRendererState.Ready)
        {
          _avatarPose = new AvatarPose(_avatarRenderer);

          // Start stand animation.
          _standAnimationController = AnimationService.StartAnimation(_standAnimation, _avatarPose);
          _standAnimationController.AutoRecycle();
        }
      }
      else
      {
        // When the user presses buttons, we cross-fade to the custom animations.
        if (InputService.IsPressed(Buttons.A, false, LogicalPlayerIndex.One))
        {
          _actionAnimationController = AnimationService.StartAnimation(
            _jumpAnimation,
            _avatarPose,
            AnimationTransitions.Replace(TimeSpan.FromSeconds(0.3)));

          _actionAnimationController.AutoRecycle();
        }
        if (InputService.IsPressed(Buttons.B, false, LogicalPlayerIndex.One))
        {
          _actionAnimationController = AnimationService.StartAnimation(
            _punchAnimation,
            _avatarPose,
            AnimationTransitions.Replace(TimeSpan.FromSeconds(0.3)));

          _actionAnimationController.AutoRecycle();
        }
        if (InputService.IsPressed(Buttons.X, false, LogicalPlayerIndex.One))
        {
          _actionAnimationController = AnimationService.StartAnimation(
            _kickAnimation,
            _avatarPose,
            AnimationTransitions.Replace(TimeSpan.FromSeconds(0.3)));

          _actionAnimationController.AutoRecycle();
        }
        if (InputService.IsPressed(Buttons.Y, false, LogicalPlayerIndex.One))
        {
          _actionAnimationController = AnimationService.StartAnimation(
            _faintAnimation,
            _avatarPose,
            AnimationTransitions.Replace(TimeSpan.FromSeconds(0.3)));

          _actionAnimationController.AutoRecycle();
        }

        // The left trigger controls the speed of the walk cycle.
        float leftTrigger = Math.Abs(InputService.GetGamePadState(LogicalPlayerIndex.One).Triggers.Left);
        _walkAnimationController.Speed = leftTrigger * 2;
        if (_walkAnimationController.State != AnimationState.Playing)
        {
          // The walk cycle is not playing. 
          // --> Start walk animation if left trigger is pressed.
          if (leftTrigger > 0)
          {
            _walkAnimationController = AnimationService.StartAnimation(
              _walkAnimation,
              _avatarPose,
              AnimationTransitions.Replace(TimeSpan.FromSeconds(0.3)));

            _walkAnimationController.AutoRecycle();
          }
        }
        else
        {
          // The walk cycle is playing. 
          // --> Cross-fade to stand animation if left trigger is not pressed.
          if (leftTrigger == 0)
          {
            _standAnimationController = AnimationService.StartAnimation(
              _standAnimation,
              _avatarPose,
              AnimationTransitions.Replace(TimeSpan.FromSeconds(0.3)));

            _standAnimationController.AutoRecycle();
          }
        }

        // If none of the animations is playing, then restart the stand animation.
        if (_standAnimationController.State != AnimationState.Playing
           && _actionAnimationController.State != AnimationState.Playing
           && _walkAnimationController.State != AnimationState.Playing)
        {
          _standAnimationController = AnimationService.StartAnimation(
              _standAnimation,
              _avatarPose,
              AnimationTransitions.Replace(TimeSpan.FromSeconds(0.3)));

          _standAnimationController.AutoRecycle();
        }
      }
    }


    protected override void OnRender(RenderContext context)
    {
      // Set render context info.
      context.CameraNode = _cameraObject.CameraNode;

      // Clear screen.
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.CornflowerBlue);

      // Draw avatar.
      if (_avatarPose != null)
      {
        _avatarRenderer.World = _pose;
        _avatarRenderer.View = (Matrix)_cameraObject.CameraNode.View;
        _avatarRenderer.Projection = _cameraObject.CameraNode.Camera.Projection;
        _avatarRenderer.Draw(_avatarPose);
      }

      // Clean up.
      context.CameraNode = null;
    }
  }
}
#endif