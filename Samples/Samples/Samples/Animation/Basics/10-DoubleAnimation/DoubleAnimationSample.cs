using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample animates a double value.",
    @"DigitalRune Animation does not directly support double animations. To add double animations
you need to create a DoubleTraits class (see DoubleTraits.cs) that tells the animation service
how double values are created/recycled/added/interpolated/blended/etc. This DoubleTraits class
can be used in a custom animation (see DoubleFromToByAnimation.cs).",
    10)]
  public class DoubleAnimationSample : AnimationSample
  {
    private readonly AnimatableProperty<double> _animatableDouble = new AnimatableProperty<double>();


    public DoubleAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      Rectangle bounds = GraphicsService.GraphicsDevice.Viewport.TitleSafeArea;

      // A 2 second from/to animation.
      DoubleFromToByAnimation doubleAnimation = new DoubleFromToByAnimation
      {
        From = bounds.Left + 200,
        To = bounds.Right - 200,
        Duration = TimeSpan.FromSeconds(2),
        EasingFunction = new QuadraticEase { Mode = EasingMode.EaseInOut },
      };

      // Make the from/to animation oscillate forever.
      AnimationClip<double> oscillatingDoubleAnimation = new AnimationClip<double>(doubleAnimation)
      {
        LoopBehavior = LoopBehavior.Oscillate,
        Duration = TimeSpan.MaxValue,
      };

      AnimationService.StartAnimation(oscillatingDoubleAnimation, _animatableDouble).UpdateAndApply();
    }


    protected override void OnRender(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.White);

      // Draw the sprite using the animated value for the x position.
      Rectangle bounds = graphicsDevice.Viewport.TitleSafeArea;
      Vector2 position = new Vector2((float)_animatableDouble.Value, bounds.Center.Y) - new Vector2(Logo.Width, Logo.Height) / 2.0f;

      SpriteBatch.Begin();
      SpriteBatch.Draw(Logo, position, Color.Red);
      SpriteBatch.End();
    }
  }
}
