using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace Samples.Game.UI
{
  [Sample(SampleCategory.GameUI,
    @"This sample shows how to create an animated GUI.",
    @"This sample uses the DigitalRune Game UI library to create a GUI.
The DigitalRune Animation library is used to animate the GUI controls.",
    4)]
  public class UIAnimationSample : Sample
  {
    // Important note: 
    // If we want the UIControls to handle gamepad input, we must set a logical player. 
    // This is done in GamePadComponent.cs

    private readonly DelegateGraphicsScreen _graphicsScreen;
    private readonly UIScreen _uiScreen;
    private readonly StackPanel _buttonStackPanel;

    // True if the exit animation is running.
    private bool _exiting;

    private AnimationController _exitAnimationController;


    public UIAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add a DelegateGraphicsScreen as the first graphics screen to the graphics
      // service. This lets us do the rendering in the Render method of this class.
      _graphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, _graphicsScreen);

      // Load a UI theme, which defines the appearance and default values of UI controls.
      Theme theme = ContentManager.Load<Theme>("UI Themes/BlendBlue/Theme");

      // Create a UI renderer, which uses the theme info to renderer UI controls.
      UIRenderer renderer = new UIRenderer(Game, theme);

      // Create a UIScreen and add it to the UI service. The screen is the root of the 
      // tree of UI controls. Each screen can have its own renderer.
      _uiScreen = new UIScreen("SampleUIScreen", renderer);
      UIService.Screens.Add(_uiScreen);

      // ----- Add buttons.
      Button button0 = new Button
      {
        Name = "Button0",
        Content = new TextBlock { Text = "Animate Buttons" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch,
      };
      button0.Click += (s, e) => PlayStartAnimation();

      Button button1 = new Button
      {
        Name = "Button1",
        Content = new TextBlock { Text = "Open FadingWindow" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      button1.Click += (s, e) => new FadingWindow(Services).Show(_uiScreen);

      Button button2 = new Button
      {
        Name = "Button2",
        Content = new TextBlock { Text = "Open ScalingWindow" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      button2.Click += (s, e) => new ScalingWindow(Services).Show(_uiScreen);

      Button button3 = new Button
      {
        Name = "Button3",
        Content = new TextBlock { Text = "Open RotatingWindow" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      button3.Click += (s, e) => new RotatingWindow(Services).Show(_uiScreen);

      Button button4 = new Button
      {
        Name = "Button4",
        Content = new TextBlock { Text = "Open EasingWindow" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      button4.Click += (s, e) => new EasingWindow(Services).Show(_uiScreen);

      Button button5 = new Button
      {
        Name = "Button5",
        Content = new TextBlock { Text = "Close All Windows" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      button5.Click += (s, e) => CloseWindows();

      Button button6 = new Button
      {
        Name = "Button6",
        Content = new TextBlock { Text = "Exit" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      button6.Click += (s, e) => Exit();

      _buttonStackPanel = new StackPanel { Margin = new Vector4F(40) };
      _buttonStackPanel.Children.Add(button0);
      _buttonStackPanel.Children.Add(button1);
      _buttonStackPanel.Children.Add(button2);
      _buttonStackPanel.Children.Add(button3);
      _buttonStackPanel.Children.Add(button4);
      _buttonStackPanel.Children.Add(button5);
      _buttonStackPanel.Children.Add(button6);
      _uiScreen.Children.Add(_buttonStackPanel);

      // Optional: If we want to allow the user to use buttons in the screen with 
      // keyboard or game pad, we have to make it a focus scope. Normally, only 
      // windows are focus scopes.
      _uiScreen.IsFocusScope = true;
      _uiScreen.Focus();

      PlayStartAnimation();
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Remove UIScreen from UI service.
        UIService.Screens.Remove(_uiScreen);
      }

      base.Dispose(disposing);
    }


    private void CloseWindows()
    {
      // Get a copy of all currently open windows.
      var windows = _uiScreen.Children.OfType<Window>().ToArray();

      // We iterate over the copied window list and close the windows. 
      // (Note: We cannot iterate over _screen.Children directly because closing a 
      // window modifies this collection.)
      foreach (Window window in windows)
        window.Close();
    }


    // Animates the buttons to slide in from the left.
    private void PlayStartAnimation()
    {
      // Create an animation that animates the RenderTranslation of a UIControl:
      Vector2FFromToByAnimation animation = new Vector2FFromToByAnimation
      {
        TargetProperty = "RenderTranslation",   // Animate the property UIControl.RenderTranslation
        From = new Vector2F(-400, 0),           // from (-400, 0) to its default value
        Duration = TimeSpan.FromSeconds(0.8),   // over a duration of 0.8 seconds.
        EasingFunction = new HermiteEase { Mode = EasingMode.EaseInOut },
      };

      // We apply this animation to all buttons. Each animation should be started with a 
      // different delay. The delay is negative, which means that a part of the animation
      // beginning is skipped.
      // To add a delay we wrap the animation in TimelineClips. The TimelineClips can be 
      // grouped together in a TimelineGroup.
      const float delay = 0.05f;
      TimelineGroup timelineGroup = new TimelineGroup
      {
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(-6 * delay), TargetObject = "Button0" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(-5 * delay), TargetObject = "Button1" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(-4 * delay), TargetObject = "Button2" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(-3 * delay), TargetObject = "Button3" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(-2 * delay), TargetObject = "Button4" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(-1 * delay), TargetObject = "Button5" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(0 * delay), TargetObject = "Button6" },
      };

      // The animation can be removed after it has finished.
      timelineGroup.FillBehavior = FillBehavior.Stop;

      // Start the animation.
#if !XBOX && !WP7
      var animationController = AnimationService.StartAnimation(timelineGroup, (IEnumerable<IAnimatableObject>)_buttonStackPanel.Children.OfType<Button>());
#else
      var animationController = AnimationService.StartAnimation(timelineGroup, _buttonStackPanel.Children.OfType<Button>().Cast<IAnimatableObject>());
#endif

      // Note: The animation effectively starts when AnimationManager.Update() and Apply() are
      // called. To start the animation immediately we can call UpdateAndApply() manually.
      animationController.UpdateAndApply();
    }


    // Animates the buttons to slide out to the left.
    public void PlayExitAnimation()
    {
      // An animation that animates the RenderTranslation of a UIControl:
      Vector2FFromToByAnimation animation = new Vector2FFromToByAnimation
      {
        TargetProperty = "RenderTranslation",
        To = new Vector2F(-400, 0),
        Duration = TimeSpan.FromSeconds(0.8),
        EasingFunction = new HermiteEase { Mode = EasingMode.EaseInOut },
      };

      // We apply this animation to all buttons. Each animation should be started with a 
      // different delay. 
      // To add a delay we wrap the animation in TimelineClips. The TimelineClips can be 
      // grouped together in a TimelineGroup.
      const float delay = 0.05f;
      TimelineGroup timelineGroup = new TimelineGroup
      {
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(6 * delay), TargetObject = "Button0" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(5 * delay), TargetObject = "Button1" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(4 * delay), TargetObject = "Button2" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(3 * delay), TargetObject = "Button3" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(2 * delay), TargetObject = "Button4" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(1 * delay), TargetObject = "Button5" },
        new TimelineClip(animation) { Delay = TimeSpan.FromSeconds(0 * delay), TargetObject = "Button6" },
      };

      // The animation should hold the animation value after it is finished (the buttons should
      // not jump back onto the screen).
      timelineGroup.FillBehavior = FillBehavior.Hold;

      // Start the animation and keep the animation controller. We need it to query the
      // state of the animation in Update().
#if !XBOX && !WP7
      _exitAnimationController = AnimationService.StartAnimation(timelineGroup, _buttonStackPanel.Children.OfType<Button>());
#else
      _exitAnimationController = AnimationService.StartAnimation(timelineGroup, _buttonStackPanel.Children.OfType<Button>().Cast<IAnimatableObject>());
#endif
    }


    // Starts to play the exit animation and closes all windows.
    private void Exit()
    {
      PlayExitAnimation();
      CloseWindows();
      _exiting = true;
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // If the exit animation is playing and has finished, then start from the 
      // beginning.
      if (_exiting && _exitAnimationController.State == AnimationState.Filling)
      {
        _exitAnimationController.Stop();

        // Here, we would exit the game.
        //Game.Exit();
        // In this project we switch to the next sample instead.
        SampleFramework.LoadNextSample();
      }
    }


    private void Render(RenderContext context)
    {
      _uiScreen.Draw(context.DeltaTime);
    }
  }
}
