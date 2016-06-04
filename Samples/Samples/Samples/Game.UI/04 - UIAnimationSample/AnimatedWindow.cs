using System;
using System.ComponentModel;
using DigitalRune.Animation;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Practices.ServiceLocation;


namespace Samples.Game.UI
{
  // A window that appears at a random screen position. It plays an animation when it
  // is loaded or closed.
  public class AnimatedWindow : Window
  {
    private readonly IAnimationService _animationService;

    // True if the window is currently closing (playing the ClosingAnimation).
    private bool _closing;

    // The animation controller of the ClosingAnimation animation.
    private AnimationController _transitionOutController;

    // The animation that is played when the window is loaded.
    public ITimeline LoadingAnimation { get; set; }

    // The animation that is played when the window is closing.
    public ITimeline ClosingAnimation { get; set; }


    public AnimatedWindow(IServiceLocator services)
    {
      // Get the animation service.
      _animationService = services.GetInstance<IAnimationService>();

      // Catch closing event.
      Closing += OnClosing;
    }


    protected override void OnLoad()
    {
      base.OnLoad();

      // Call Measure to update DesiredWith and DesiredHeight.
      Measure(new Vector2F(float.PositiveInfinity));

      // Set a random position on the screen where the whole window is visible and to the right
      // of the MainScreenComponent buttons.
      X = RandomHelper.Random.NextInteger(200, (int)(Screen.ActualWidth - DesiredWidth));
      Y = RandomHelper.Random.NextInteger(0, (int)(Screen.ActualHeight - DesiredHeight));

      if (LoadingAnimation != null)
      {
        // Start the animation.
        var animationController = _animationService.StartAnimation(LoadingAnimation, this);

        // Note: The animation effectively starts when AnimationManager.Update() and Apply() are
        // called. To start the animation immediately we can call UpdateAndApply() manually.
        animationController.UpdateAndApply();
      }
    }


    private void OnClosing(object sender, CancelEventArgs eventArgs)
    {
      if (!_closing && ClosingAnimation != null)
      {
        // Start the closing animation.
        _transitionOutController = _animationService.StartAnimation(ClosingAnimation, this);

        // Remember that we are closing. (The ClosingAnimation is playing.)
        _closing = true;

        // Cancel the close operation. We want to keep this window opened until the closing
        // animation is finished.
        eventArgs.Cancel = true;
      }
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      base.OnUpdate(deltaTime);

      // If the closing animation is playing, we wait until the closing animation is finished,
      // then we really close the window.
      if (_closing)
      {
        // 'Filling' or 'Stopped' indicates that the animation has finished.
        if (_transitionOutController.State == AnimationState.Filling
            || _transitionOutController.State == AnimationState.Stopped)
        {
          Close();
        }
      }
    }
  }
}
