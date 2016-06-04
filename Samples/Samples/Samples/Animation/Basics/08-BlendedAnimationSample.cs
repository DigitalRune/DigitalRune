using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample shows how to blend two animations.",
    @"Animation blending can be used to ""average"" two different animations. For example, when
animating characters a 2 second ""Walk"" animation cycle can be blended with a 1 second ""Run""
animation cycle to create a 1.5 second ""FastWalk"" animation cycle.
To blend animations you put them into a BlendGroup. The BlendGroup has a weight for each animation.
If the animations have different durations and you want to average the durations you must call
BlendGroup.SynchronizeDurations once.
In this sample, the first animation has a duration of 4 seconds. It moves the sprite horizontally
in the upper half of the screen. The tint color changes from red to green.
The second animation has a duration of 1 seconds. It moves the sprite horizontally in the lower
half of the screen. The tint color changes from black to white.",
    8)]
  [Controls(@"Sample
  Press <Up> or <Down> to change the blend weight and smoothly transition between both animations.")]
  public class BlendedAnimationSample : AnimationSample
  {
    // An animatable sprite (see AnimatableObjectSample).
    private readonly AnimatableSprite _animatableSprite;

    private readonly BlendGroup _blendedAnimation;


    public BlendedAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      Rectangle bounds = GraphicsService.GraphicsDevice.Viewport.TitleSafeArea;

      // Create the animatable object.
      _animatableSprite = new AnimatableSprite("SpriteA", SpriteBatch, Logo)
      {
        Position = new Vector2(bounds.Center.X, bounds.Center.Y / 2.0f),
        Color = Color.Red,
      };

      Vector2FromToByAnimation slowLeftRightAnimation = new Vector2FromToByAnimation
      {
        TargetProperty = "Position",
        From = new Vector2(bounds.Left + 100, bounds.Top + 200),
        To = new Vector2(bounds.Right - 100, bounds.Top + 200),
        Duration = TimeSpan.FromSeconds(4),
        EasingFunction = new HermiteEase { Mode = EasingMode.EaseInOut },
      };

      ColorKeyFrameAnimation redGreenAnimation = new ColorKeyFrameAnimation
      {
        TargetProperty = "Color",
        EnableInterpolation = true,
      };
      redGreenAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(0), Color.Red));
      redGreenAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(4), Color.Green));

      TimelineGroup animationA = new TimelineGroup
      {
        slowLeftRightAnimation,
        redGreenAnimation,
      };

      Vector2FromToByAnimation fastLeftRightAnimation = new Vector2FromToByAnimation
      {
        TargetProperty = "Position",
        From = new Vector2(bounds.Left + 100, bounds.Bottom - 200),
        To = new Vector2(bounds.Right - 100, bounds.Bottom - 200),
        Duration = TimeSpan.FromSeconds(1),
        EasingFunction = new HermiteEase { Mode = EasingMode.EaseInOut },
      };

      ColorKeyFrameAnimation blackWhiteAnimation = new ColorKeyFrameAnimation
      {
        TargetProperty = "Color",
        EnableInterpolation = true,
      };
      blackWhiteAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(0), Color.Black));
      blackWhiteAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(1), Color.White));

      TimelineGroup animationB = new TimelineGroup
      {
        fastLeftRightAnimation,
        blackWhiteAnimation,
      };

      // Create a BlendGroup that blends animationA and animationB. 
      // The BlendGroup uses the TargetProperty values of the contained animations to 
      // to match the animations that should be blended:
      //   slowLeftRightAnimation with fastLeftRightAnimation
      //   redGreenAnimation with blackWhiteAnimation
      _blendedAnimation = new BlendGroup
      {
        LoopBehavior = LoopBehavior.Oscillate,
        Duration = TimeSpan.MaxValue,
      };
      _blendedAnimation.Add(animationA, 1);
      _blendedAnimation.Add(animationB, 0);
      _blendedAnimation.SynchronizeDurations();

      // Start blended animation.
      AnimationService.StartAnimation(_blendedAnimation, _animatableSprite).UpdateAndApply();
    }


    public override void Update(GameTime gameTime)
    {
      // <Up> --> Increase blend weight of first animation.
      if (InputService.IsDown(Keys.Up))
      {
        float weight = _blendedAnimation.GetWeight(0);

        weight += 1 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (weight > 1)
          weight = 1;

        _blendedAnimation.SetWeight(0, weight);
        _blendedAnimation.SetWeight(1, 1 - weight);
      }

      // <Down> --> Increase blend weight of second animation.
      if (InputService.IsDown(Keys.Down))
      {
        float weight = _blendedAnimation.GetWeight(0);

        weight -= 1 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (weight < 0)
          weight = 0;

        _blendedAnimation.SetWeight(0, weight);
        _blendedAnimation.SetWeight(1, 1 - weight);
      }


      base.Update(gameTime);
    }


    protected override void OnRender(RenderContext context)
    {
      GraphicsService.GraphicsDevice.Clear(Color.White);

      _animatableSprite.Draw();
    }
  }
}
