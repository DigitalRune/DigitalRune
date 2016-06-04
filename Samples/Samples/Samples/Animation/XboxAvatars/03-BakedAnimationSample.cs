#if XBOX
using System;
using System.Diagnostics;
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
    @"This sample shows how to convert an XNA AvatarAnimation into an AvatarExpressionKeyFrameAnimation
and a SkeletonKeyFrameAnimation.",
    @"Using AvatarExpressionKeyFrameAnimations and SkeletonKeyFrameAnimations is faster than wrapping
an AvatarAnimation as it is done in the WrappedAnimationSample.)",
    103)]
  [Controls(@"Sample
  Press <A> to restart animation.")]
  public class BakedAnimationSample : AnimationSample
  {
    private readonly CameraObject _cameraObject;

    private readonly AvatarDescription _avatarDescription;
    private readonly AvatarRenderer _avatarRenderer;

    private ITimeline _waveAnimation;
    private AvatarPose _avatarPose;
    private Pose _pose = new Pose(new Vector3F(-0.5f, 0, 0));


    public BakedAnimationSample(Microsoft.Xna.Framework.Game game)
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

      // Convert animation.
      _waveAnimation = BakeAnimation(new AvatarAnimation(AvatarAnimationPreset.Clap));
    }


    private ITimeline BakeAnimation(AvatarAnimation avatarAnimation)
    {
      // Create an AvatarExpression key frame animation that will be applied to the Expression
      // property of an AvatarPose.
      AvatarExpressionKeyFrameAnimation expressionAnimation = new AvatarExpressionKeyFrameAnimation
      {
        TargetProperty = "Expression"
      };

      // Create a SkeletonPose key frame animation that will be applied to the SkeletonPose
      // property of an AvatarPose.
      SkeletonKeyFrameAnimation skeletonKeyFrameAnimation = new SkeletonKeyFrameAnimation
      {
        TargetProperty = "SkeletonPose"
      };

      // In the next loop, we sample the original animation with 30 Hz and store the key frames.
      int numberOfKeyFrames = 0;
      AvatarExpression previousExpression = new AvatarExpression();
      TimeSpan time = TimeSpan.Zero;
      TimeSpan length = avatarAnimation.Length;
      TimeSpan step = new TimeSpan(333333); //  1/30 seconds;
      while (true)
      {
        // Advance time in AvatarAnimation.
        avatarAnimation.CurrentPosition = time;

        // Add expression key frame if this is the first key frame or if the key frame is 
        // different from the last key frame.
        AvatarExpression expression = avatarAnimation.Expression;
        if (time == TimeSpan.Zero || !expression.Equals(previousExpression))
          expressionAnimation.KeyFrames.Add(new KeyFrame<AvatarExpression>(time, expression));

        previousExpression = expression;

        // Convert bone transforms to SrtTransforms and add key frames to the SkeletonPose
        // animation.
        for (int i = 0; i < avatarAnimation.BoneTransforms.Count; i++)
        {
          SrtTransform boneTransform = SrtTransform.FromMatrix(avatarAnimation.BoneTransforms[i]);
          skeletonKeyFrameAnimation.AddKeyFrame(i, time, boneTransform);
          numberOfKeyFrames++;
        }

        // Abort if we have arrived at the end time.
        if (time == length)
          break;

        // Increase time. We check that we do not step over the end time. 
        if (time + step > length)
          time = length;
        else
          time += step;
      }

      // Compress animation to save memory.
      float numberOfRemovedKeyFrames = skeletonKeyFrameAnimation.Compress(0.1f, 0.1f, 0.001f);
      Debug.WriteLine("Compression removed " + numberOfRemovedKeyFrames * 100 + "% of the key frames.");

      // Finalize the skeleton key frame animation. This optimizes the internal data structures.
      skeletonKeyFrameAnimation.Freeze();

      return new TimelineGroup
      {
        expressionAnimation,
        skeletonKeyFrameAnimation,
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
          AnimationService.StartAnimation(_waveAnimation, _avatarPose).AutoRecycle();
        }
      }
      else if (InputService.IsPressed(Buttons.A, false, LogicalPlayerIndex.One))
      {
        // Restart animation using a cross-fade of 0.5 seconds.
        AnimationService.StartAnimation(_waveAnimation,
                                         _avatarPose,
                                         AnimationTransitions.Replace(TimeSpan.FromSeconds(0.5))
                                        ).AutoRecycle();
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