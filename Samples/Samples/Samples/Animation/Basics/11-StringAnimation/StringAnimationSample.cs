using System;
using DigitalRune.Animation;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample animates a string value.",
    @"DigitalRune Animation does not directly support string animations. To add string animations
you need to create a StringTraits class (see StringTraits.cs) that tells the animation service
how strings are created/recycled/added/interpolated/blended/etc. This StringTraits class can be
used in a custom animation (see StringKeyFrameAnimation.cs).",
    11)]
  public class StringAnimationSample : AnimationSample
  {
    private readonly AnimatableProperty<string> _animatableString = new AnimatableProperty<string>();


    public StringAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // A key-frame animation of string values.
      // Note: The key frame animation plays all strings and then holds the last key frame
      // because the default KeyFrameAnimation.FillBehavior is "Hold".
      StringKeyFrameAnimation keyFrameAnimation = new StringKeyFrameAnimation();
      keyFrameAnimation.KeyFrames.Add(new KeyFrame<string>(TimeSpan.FromSeconds(0), "The"));
      keyFrameAnimation.KeyFrames.Add(new KeyFrame<string>(TimeSpan.FromSeconds(1), "quick"));
      keyFrameAnimation.KeyFrames.Add(new KeyFrame<string>(TimeSpan.FromSeconds(2), "brown"));
      keyFrameAnimation.KeyFrames.Add(new KeyFrame<string>(TimeSpan.FromSeconds(3), "fox"));
      keyFrameAnimation.KeyFrames.Add(new KeyFrame<string>(TimeSpan.FromSeconds(4), "jumps"));
      keyFrameAnimation.KeyFrames.Add(new KeyFrame<string>(TimeSpan.FromSeconds(5), "over"));
      keyFrameAnimation.KeyFrames.Add(new KeyFrame<string>(TimeSpan.FromSeconds(6), "the"));
      keyFrameAnimation.KeyFrames.Add(new KeyFrame<string>(TimeSpan.FromSeconds(7), "lazy"));
      keyFrameAnimation.KeyFrames.Add(new KeyFrame<string>(TimeSpan.FromSeconds(8), "dog."));

      // Use an AnimationClip to loop the first 9 seconds of the key-frame animation 
      // forever. The animation speed is set to 2 to make the animation play faster.
      AnimationClip<string> timelineClip = new AnimationClip<string>(keyFrameAnimation)
      {
        LoopBehavior = LoopBehavior.Cycle,
        ClipStart = TimeSpan.FromSeconds(0),
        ClipEnd = TimeSpan.FromSeconds(9),
        Duration = TimeSpan.MaxValue,
        Speed = 2,
      };

      // Start the animation.
      AnimationService.StartAnimation(timelineClip, _animatableString);
    }


    protected override void OnRender(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.White);

      if (_animatableString.Value != null)
      {
        // Draw the animated string value.
        SpriteBatch.Begin();

        const float scale = 5;
        Vector2 size = SpriteFont.MeasureString(_animatableString.Value);
        Rectangle bounds = graphicsDevice.Viewport.TitleSafeArea;
        Vector2 position = new Vector2(bounds.Center.X, bounds.Center.Y) - size * scale / 2;

        SpriteBatch.DrawString(
          SpriteFont,
          _animatableString.Value,
          position,
          Color.Black,
          0,
          new Vector2(0),
          new Vector2(scale),
          SpriteEffects.None,
          0);

        SpriteBatch.End();
      }
    }
  }
}
