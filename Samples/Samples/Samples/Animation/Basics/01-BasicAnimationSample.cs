using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample shows how to use a from/to/by animation to animate the x position of a sprite.
The sprite color is animated with a key-frame animation. Both animations oscillate until the
animation is stopped by the user.",
    @"",
    1)]
  [Controls(@"Sample
  Press <Space> to start/stop animations.")]
  public class BasicAnimationSample : AnimationSample
  {
    private readonly AnimatableProperty<float> _animatableX = new AnimatableProperty<float> { Value = 300 };
    private readonly AnimatableProperty<Color> _animatableColor = new AnimatableProperty<Color> { Value = Color.Black };

    private readonly AnimationClip<float> _oscillatingXAnimation;
    private readonly AnimationClip<Color> _loopedColorAnimation;


    public BasicAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var titleSafeArea = GraphicsService.GraphicsDevice.Viewport.TitleSafeArea;
      float minX = titleSafeArea.Left + 100;
      float maxX = titleSafeArea.Right - 100;

      // A from/to/by animation that animates a float value from minX to maxX over 2 seconds
      // using an easing function smooth movement.
      SingleFromToByAnimation xAnimation = new SingleFromToByAnimation
      {
        From = minX,
        To = maxX,
        Duration = TimeSpan.FromSeconds(2),
        EasingFunction = new CubicEase { Mode = EasingMode.EaseInOut },
      };

      // From-To-By animations do not loop by default.
      // Use an AnimationClip to turn the 2 second xAnimation into an animation that
      // oscillates forever.
      _oscillatingXAnimation = new AnimationClip<float>(xAnimation)
      {
        LoopBehavior = LoopBehavior.Oscillate,
        Duration = TimeSpan.MaxValue,
      };


      // A color key-frame animation.
      ColorKeyFrameAnimation colorAnimation = new ColorKeyFrameAnimation
      {
        //EnableInterpolation = true,  // Interpolation is enabled by default.
      };
      // Add the key-frames.
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(0.0), Color.Red));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(0.5), Color.Orange));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(1.0), Color.Yellow));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(1.5), Color.Green));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(2.0), Color.Blue));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(2.5), Color.Indigo));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(3.0), Color.Violet));

      // The last-key frame defines the length of the animation (3.5 seconds). This is a
      // "loop frame", which means the value is the same as in the first key to create a looping
      // animation.
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(3.5), Color.Red));

      // If the key-frames are not sorted, call Sort().
      //colorAnimation.KeyFrames.Sort();

      // Use an AnimationClip to turn the 3.5 second colorAnimation into an animation that
      // loops forever.
      _loopedColorAnimation = new AnimationClip<Color>(colorAnimation)
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };

      StartAnimations();
    }


    public override void Update(GameTime gameTime)
    {
      // Start/stop animations when SPACE is pressed
      if (InputService.IsPressed(Keys.Space, false))
      {
        if (AnimationService.IsAnimated(_animatableX))
          StopAnimations();
        else
          StartAnimations();
      }

      base.Update(gameTime);
    }


    private void StartAnimations()
    {
      // Start animations.
      var animationController = AnimationService.StartAnimation(_oscillatingXAnimation, _animatableX);

      // The animation effectively starts when AnimationManager.Update() and Apply() are
      // called in SampleGame.cs. To start the animation immediately we can call UpdateAndApply() manually.
      animationController.UpdateAndApply();

      // Calling AutoRecycle() enables auto-recycling which makes sure that created animation
      // instances are recycled (using resource pooling) when the animation is stopped. 
      // This avoids unnecessary garbage collections.
      animationController.AutoRecycle();

      animationController = AnimationService.StartAnimation(_loopedColorAnimation, _animatableColor);
      animationController.UpdateAndApply();
      animationController.AutoRecycle();
    }


    private void StopAnimations()
    {
      // Stop all animations running on the animatable properties.
      AnimationService.StopAnimation(_animatableX);
      AnimationService.StopAnimation(_animatableColor);
    }


    protected override void OnRender(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.White);

      // Draw a sprite using the animated x value and the animated color.
      float x = _animatableX.Value;
      float y = graphicsDevice.Viewport.TitleSafeArea.Center.Y;
      Vector2 position = new Vector2(x, y) - new Vector2(Logo.Width, Logo.Height) / 2.0f;
      Color color = _animatableColor.Value;

      SpriteBatch.Begin();
      SpriteBatch.Draw(Logo, position, color);
      SpriteBatch.End();
    }
  }
}
