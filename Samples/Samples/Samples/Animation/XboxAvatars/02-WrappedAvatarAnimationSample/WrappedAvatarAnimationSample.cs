#if XBOX
using System;
using DigitalRune;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;

namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample shows how to wrap the XNA AvatarAnimation class to use the animations with
the DigitalRune Animation system.",
    @"",
    102)]
  [Controls(@"Sample
  Press <A> to cross-fade to next preset.
  Press <B> to cross-fade to previous preset.")]
  public class WrappedAvatarAnimationSample : AnimationSample
  {
    private readonly DebugRenderer _debugRenderer;
    private readonly CameraObject _cameraObject;

    private readonly AvatarDescription _avatarDescription;
    private readonly AvatarRenderer _avatarRenderer;

    private AvatarAnimationPreset _currentAvatarAnimationPreset;
    private AvatarPose _avatarPose;
    private Pose _pose = new Pose(new Vector3F(-0.5f, 0, 0));


    public WrappedAvatarAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // This sample uses for a DebugRenderer for rendering text.
      _debugRenderer = new DebugRenderer(GraphicsService, SpriteFont)
      {
        DefaultColor = Color.Black,
        DefaultTextPosition = new Vector2F(10),
      };

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      _cameraObject.ResetPose(new Vector3F(0, 1, -3), ConstantsF.Pi, 0);
      GameObjectService.Objects.Add(_cameraObject);

      // Create a random avatar.
      _avatarDescription = AvatarDescription.CreateRandom();
      _avatarRenderer = new AvatarRenderer(_avatarDescription);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _debugRenderer.Dispose();
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

          // Start the first animation.
          var wrappedAnimation = WrapAnimation(_currentAvatarAnimationPreset);
          AnimationService.StartAnimation(wrappedAnimation, _avatarPose).AutoRecycle();

          _debugRenderer.Clear();
          _debugRenderer.DrawText("\n\nCurrent Animation: " + _currentAvatarAnimationPreset);
        }
      }
      else
      {
        if (InputService.IsPressed(Buttons.A, false, LogicalPlayerIndex.One))
        {
          // Switch to next preset.
          _currentAvatarAnimationPreset++;
          if (!Enum.IsDefined(typeof(AvatarAnimationPreset), _currentAvatarAnimationPreset))
            _currentAvatarAnimationPreset = 0;

          // Cross-fade to new animation.
          var wrappedAnimation = WrapAnimation(_currentAvatarAnimationPreset);
          AnimationService.StartAnimation(wrappedAnimation,
                                           _avatarPose,
                                           AnimationTransitions.Replace(TimeSpan.FromSeconds(0.5))
                                          ).AutoRecycle();

          _debugRenderer.Clear();
          _debugRenderer.DrawText("\n\nCurrent Animation: " + _currentAvatarAnimationPreset);
        }

        if (InputService.IsPressed(Buttons.B, false, LogicalPlayerIndex.One))
        {
          // Switch to previous preset.
          _currentAvatarAnimationPreset--;
          if (!Enum.IsDefined(typeof(AvatarAnimationPreset), _currentAvatarAnimationPreset))
            _currentAvatarAnimationPreset = (AvatarAnimationPreset)EnumHelper.GetValues(typeof(AvatarAnimationPreset)).Length - 1;

          // Cross-fade to new animation.
          var wrappedAnimation = WrapAnimation(_currentAvatarAnimationPreset);
          AnimationService.StartAnimation(wrappedAnimation,
                                           _avatarPose,
                                           AnimationTransitions.Replace(TimeSpan.FromSeconds(0.5))
                                          ).AutoRecycle();

          _debugRenderer.Clear();
          _debugRenderer.DrawText("\n\nCurrent Animation: " + _currentAvatarAnimationPreset);
        }
      }
    }


    public ITimeline WrapAnimation(AvatarAnimationPreset preset)
    {
      // Return a timeline group that contains one animation for the Expression and one
      // animation for the SkeletonPose.
      var avatarAnimation = new AvatarAnimation(_currentAvatarAnimationPreset);
      var expressionAnimation = new WrappedAvatarExpressionAnimation(avatarAnimation);
      var skeletonAnimation = new WrappedAvatarSkeletonAnimation(avatarAnimation);
      return new TimelineGroup
      {
        expressionAnimation,
        skeletonAnimation,
      };
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

      // Draw debug info.
      _debugRenderer.Render(context);

      // Clean up.
      context.CameraNode = null;
    }
  }
}
#endif