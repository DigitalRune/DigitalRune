using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample shows how to smoothly transition between animations.",
    @"When the user presses a key, a horizontal movement is cross-faded to a vertical movement.
With another key the animations can be faded-out or faded-in. A fade-in or a cross-fade is
created by starting a new animation and specifying a Replace transition with a fade-in time.
A fade-out is created using AnimationController.Stop(). See Update() method.",
    6)]
  [Controls(@"Sample
  Press <Space> to cross-fade between a horizontal and a vertical animation.
  Press <Enter> to stop/start all animations (using fade-out/fade-in).")]
  public class CrossFadeSample : AnimationSample
  {
    private readonly AnimatableProperty<Vector2> _animatedPosition = new AnimatableProperty<Vector2>();

    // Current state: 0 = no animation, 1 = horizontal animation, 2 = vertical animation
    private int _state;
    private readonly AnimationClip<Vector2> _horizontalAnimation;
    private readonly AnimationClip<Vector2> _verticalAnimation;
    private AnimationController _currentAnimationController;


    public CrossFadeSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      Rectangle bounds = GraphicsService.GraphicsDevice.Viewport.TitleSafeArea;

      // Set the base value of the _animatedPosition. When the animations are stopped,
      // _animatedPosition will return to this value.
      _animatedPosition.Value = new Vector2(bounds.Center.X, bounds.Center.Y);

      // Create an oscillating horizontal animation.
      Vector2FromToByAnimation fromToAnimation = new Vector2FromToByAnimation
      {
        From = new Vector2(200, bounds.Center.Y),
        To = new Vector2(bounds.Right - 200, bounds.Center.Y),
        Duration = TimeSpan.FromSeconds(2),
        EasingFunction = new CubicEase { Mode = EasingMode.EaseInOut },
      };

      _horizontalAnimation = new AnimationClip<Vector2>(fromToAnimation)
      {
        LoopBehavior = LoopBehavior.Oscillate,
        Duration = TimeSpan.MaxValue,
      };

      // Create an oscillating vertical animation.
      fromToAnimation = new Vector2FromToByAnimation
      {
        From = new Vector2(bounds.Center.X, 100),
        To = new Vector2(bounds.Center.X, bounds.Bottom - 100),
        Duration = TimeSpan.FromSeconds(2),
        EasingFunction = new CubicEase { Mode = EasingMode.EaseInOut },
      };

      _verticalAnimation = new AnimationClip<Vector2>(fromToAnimation)
      {
        LoopBehavior = LoopBehavior.Oscillate,
        Duration = TimeSpan.MaxValue,
      };

      // Start the horizontal movement. AnimationService.StartAnimation() returns an
      // AnimationController. We keep this AnimationController because it is needed to fade-out
      // the animation.
      _state = 1;
      _currentAnimationController = AnimationService.StartAnimation(_horizontalAnimation, _animatedPosition);
      _currentAnimationController.UpdateAndApply();

      // When the animation is stopped, all intermediate animation objects should be recycled.
      _currentAnimationController.AutoRecycle();
    }


    public override void Update(GameTime gameTime)
    {
      if (InputService.IsPressed(Keys.Space, false))
      {
        if (_state == 1)
        {
          // Fade-in the vertical animation, replacing the previous animations.
          _state = 2;

          _currentAnimationController = AnimationService.StartAnimation(
            _verticalAnimation,
            _animatedPosition,
            AnimationTransitions.Replace(TimeSpan.FromSeconds(0.5)));  // Replace all previous animations using a fade-in of 0.5 seconds.

          _currentAnimationController.AutoRecycle();
        }
        else
        {
          // Fade-in the horizontal animation, replacing the previous animations.
          _state = 1;

          _currentAnimationController = AnimationService.StartAnimation(
            _horizontalAnimation,
            _animatedPosition,
            AnimationTransitions.Replace(TimeSpan.FromSeconds(0.5)));

          _currentAnimationController.AutoRecycle();
        }
      }

      if (InputService.IsPressed(Keys.Enter, false))
      {
        if (_state == 0)
        {
          // Fade-in the horizontal animation.
          _state = 1;

          _currentAnimationController = AnimationService.StartAnimation(
            _horizontalAnimation,
            _animatedPosition,
            AnimationTransitions.Replace(TimeSpan.FromSeconds(0.5)));

          _currentAnimationController.AutoRecycle();
        }
        else
        {
          // Fade-out the current animation.
          _state = 0;

          _currentAnimationController.Stop(TimeSpan.FromSeconds(0.5)); // Fade-out over 0.5 seconds.
        }
      }

      base.Update(gameTime);
    }


    protected override void OnRender(RenderContext context)
    {
      GraphicsService.GraphicsDevice.Clear(Color.White);

      // Draw sprite centered at animated position.
      Vector2 position = _animatedPosition.Value - new Vector2(Logo.Width, Logo.Height) / 2.0f;

      SpriteBatch.Begin();
      SpriteBatch.Draw(Logo, position, Color.Red);
      SpriteBatch.End();
    }
  }
}
