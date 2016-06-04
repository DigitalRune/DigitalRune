using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;


namespace Samples.Game.UI
{
  // A window that animates the RenderScale and RenderRotation when the windows is loaded/closed.
  public class RotatingWindow : AnimatedWindow
  {
    public RotatingWindow(IServiceLocator services) 
      : base(services)
    {
      Title = "RotatingWindow";

      Width = 250;
      Height = 100;

      Content = new TextBlock
      {
        Margin = new Vector4F(8),
        Text = "The 'RenderScale' and the 'RenderRotation' of this window are animated.",
        WrapText = true,
      };

      // Set the center of the scale and rotation transformations to the center of the window. 
      RenderTransformOrigin = new Vector2F(0.5f, 0.5f);

      // The loading animation is a timeline group of two animations. 
      // One animations animates the RenderScale from (0, 0) to its current value.
      // The other animation animates the RenderRotation from 10 to its current value.
      // The base class AnimatedWindow will apply this timeline group on this window 
      // when the window is loaded.
      LoadingAnimation = new TimelineGroup
      {
        new Vector2FFromToByAnimation
        {
          TargetProperty = "RenderScale",
          From = new Vector2F(0, 0),
          Duration = TimeSpan.FromSeconds(0.8),
          EasingFunction = new HermiteEase { Mode = EasingMode.EaseOut },
        },
        new SingleFromToByAnimation
        {
          TargetProperty = "RenderRotation",
          From = 10,
          Duration = TimeSpan.FromSeconds(0.8),
          EasingFunction = new HermiteEase { Mode = EasingMode.EaseOut },
        }
      };

      // The closing animation is a timeline group of two animations. 
      // One animations animates the RenderScale from its current value to (0, 0).
      // The other animation animates the RenderRotation its current value to 10.
      // The base class AnimatedWindow will apply this timeline group on this window when
      // the window is loaded.
      ClosingAnimation = new TimelineGroup
      {
        new Vector2FFromToByAnimation
        {
          TargetProperty = "RenderScale",
          To = new Vector2F(0, 0),
          Duration = TimeSpan.FromSeconds(0.8),
          EasingFunction = new HermiteEase { Mode = EasingMode.EaseIn },
        },
        new SingleFromToByAnimation
        {
          TargetProperty = "RenderRotation",
          To = 10,
          Duration = TimeSpan.FromSeconds(0.8),
          EasingFunction = new HermiteEase { Mode = EasingMode.EaseIn },
        }
      };
    }
  }
}
