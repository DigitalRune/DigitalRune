using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample shows how to animate a Vector2 using two separate float animations for x and y.
Composite animations (e.g. Vector2Animation) combine other animations to create an animation
of a complex animation value type.",
    @"",
    2)]
  public class CompositeAnimationSample : AnimationSample
  {
    private readonly AnimatableProperty<Vector2> _animatablePosition = new AnimatableProperty<Vector2>();


    public CompositeAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      Rectangle bounds = GraphicsService.GraphicsDevice.Viewport.TitleSafeArea;

      // A single from/to animation.
      SingleFromToByAnimation fromToAnimation = new SingleFromToByAnimation
      {
        From = bounds.Top + 200,
        To = bounds.Bottom - 200,
        Duration = TimeSpan.FromSeconds(2),
        EasingFunction = new SineEase { Mode = EasingMode.EaseInOut },
      };

      // Create an animation that oscillates forever.
      AnimationClip<float> loopedSingleAnimationX = new AnimationClip<float>(fromToAnimation)
      {
        LoopBehavior = LoopBehavior.Oscillate,
        Duration = TimeSpan.MaxValue,
      };

      // Create an animation that oscillates forever. The animations starts 1 second into
      // the fromToAnimation - that means, loopedSingleAnimationX is 1 second "behind" this
      // animation.
      AnimationClip<float> loopedSingleAnimationY = new AnimationClip<float>(fromToAnimation)
      {
        LoopBehavior = LoopBehavior.Oscillate,
        Duration = TimeSpan.MaxValue,
        Delay = TimeSpan.FromSeconds(-1),
      };

      // Create a composite animation that combines the two float animations to animate
      // a Vector2 value.
      Vector2Animation compositeAnimation = new Vector2Animation(loopedSingleAnimationX, loopedSingleAnimationY);

      // Start animation.
      AnimationService.StartAnimation(compositeAnimation, _animatablePosition).UpdateAndApply();
    }


    protected override void OnRender(RenderContext context)
    {
      GraphicsService.GraphicsDevice.Clear(Color.White);

      // Draw the sprite centered at the animated position.
      Vector2 position = _animatablePosition.Value - new Vector2(Logo.Width, Logo.Height) / 2.0f;

      SpriteBatch.Begin();
      SpriteBatch.Draw(Logo, position, Color.Gold);
      SpriteBatch.End();
    }
  }
}
