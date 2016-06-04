using System;
using DigitalRune.Animation;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;


namespace Samples.Game.UI
{
  // A window that animates the Opacity when the windows is loaded/closed.
  public class FadingWindow : AnimatedWindow
  {
    public FadingWindow(IServiceLocator services) 
      : base(services)
    {
      Title = "FadingWindow";

      Width = 200;
      Height = 100;

      Content = new TextBlock
      {
        Margin = new Vector4F(8),
        Text = "The 'Opacity' of this window is animated.",
        WrapText = true,
      };

      LoadingAnimation = new SingleFromToByAnimation
      {
        TargetProperty = "Opacity",           // Transition the property UIControl.Opacity 
        From = 0,                             // from 0 to its actual value
        Duration = TimeSpan.FromSeconds(0.3), // over a duration of 0.3 seconds.
      };

      ClosingAnimation = new SingleFromToByAnimation
      {
        TargetProperty = "Opacity",           // Transition the property UIControl.Opacity
        To = 0,                               // from its current value to 0
        Duration = TimeSpan.FromSeconds(0.3), // over a duration 0.3 seconds.
      };
    }
  }
}
