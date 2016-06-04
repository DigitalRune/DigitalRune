using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;


namespace Samples.Game.UI
{
  // A window that allows to test easing functions on a slider.
  public class EasingWindow : AnimatedWindow
  {
    private readonly IInputService _inputService;
    private readonly IAnimationService _animationService;

    // The DropDownButton that selects easing function.
    private readonly DropDownButton _functionDropDown;

    // The DropDownButton that selects the easing mode.
    private readonly DropDownButton _modeDropDown;

    // The Slider control which will be animated.
    private readonly Slider _slider;


    public EasingWindow(IServiceLocator services) 
      : base(services)
    {
      _inputService = services.GetInstance<IInputService>();
      _animationService = services.GetInstance<IAnimationService>();

      Title = "EasingWindow";

      StackPanel stackPanel = new StackPanel { Margin = new Vector4F(8) };
      Content = stackPanel;

      TextBlock textBlock = new TextBlock
      {
        Text = "Test different Easing Functions in this window.",
        Margin = new Vector4F(0, 0, 0, 8),
      };
      stackPanel.Children.Add(textBlock);

      StackPanel horizontalPanel = new StackPanel
      {
        Orientation = Orientation.Horizontal,
        Margin = new Vector4F(0, 0, 0, 8)
      };
      stackPanel.Children.Add(horizontalPanel);

      textBlock = new TextBlock
      {
        Text = "Easing Function:",
        Width = 80,
        Margin = new Vector4F(0, 0, 8, 0),
      };
      horizontalPanel.Children.Add(textBlock);

      _functionDropDown = new DropDownButton
      {
        Width = 100,

        // The DropDownButton automatically converts the items to string (using ToString) and 
        // displays this string. This is not helpful for this sort of items. We want to display
        // the type name of the items instead. The following is a callback which creates a
        // TextBlock for each item. It is called when the drop-down list is opened.
        CreateControlForItem = item => new TextBlock { Text = item.GetType().Name },
      };
      horizontalPanel.Children.Add(_functionDropDown);

      _functionDropDown.Items.Add(new BackEase());
      _functionDropDown.Items.Add(new BounceEase());
      _functionDropDown.Items.Add(new CircleEase());
      _functionDropDown.Items.Add(new CubicEase());
      _functionDropDown.Items.Add(new ElasticEase());
      _functionDropDown.Items.Add(new ExponentialEase());
      _functionDropDown.Items.Add(new LogarithmicEase());
      _functionDropDown.Items.Add(new HermiteEase());
      _functionDropDown.Items.Add(new PowerEase());
      _functionDropDown.Items.Add(new QuadraticEase());
      _functionDropDown.Items.Add(new QuinticEase());
      _functionDropDown.Items.Add(new SineEase());
      _functionDropDown.SelectedIndex = 0;

      horizontalPanel = new StackPanel
      {
        Orientation = Orientation.Horizontal,
        Margin = new Vector4F(0, 0, 0, 8)
      };
      stackPanel.Children.Add(horizontalPanel);

      textBlock = new TextBlock
      {
        Text = "Easing Mode:",
        Width = 80,
        Margin = new Vector4F(0, 0, 8, 0),
      };
      horizontalPanel.Children.Add(textBlock);

      _modeDropDown = new DropDownButton
      {
        Width = 100,
      };
      horizontalPanel.Children.Add(_modeDropDown);

      _modeDropDown.Items.Add(EasingMode.EaseIn);
      _modeDropDown.Items.Add(EasingMode.EaseOut);
      _modeDropDown.Items.Add(EasingMode.EaseInOut);
      _modeDropDown.SelectedIndex = 0;

      _slider = new Slider
      {
        Margin = new Vector4F(0, 16, 0, 0),
        SmallChange = 0.01f,
        LargeChange = 0.1f,
        Minimum = -0.5f,
        Maximum = 1.5f,
        Width = 250,
        HorizontalAlignment = HorizontalAlignment.Center,
      };
      stackPanel.Children.Add(_slider);

      // Display the current value of the slider.
      var valueLabel = new TextBlock
      {
        Text = _slider.Value.ToString("F2"),
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new Vector4F(0, 0, 0, 8),
      };
      stackPanel.Children.Add(valueLabel);

      // Update the text every time the slider value changes.
      var valueProperty = _slider.Properties.Get<float>("Value");
      valueProperty.Changed += (s, e) => valueLabel.Text = e.NewValue.ToString("F2");

      Button button = new Button
      {
        Content = new TextBlock { Text = "Animate" },
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new Vector4F(0, 0, 0, 8),
      };
      button.Click += OnButtonClicked;
      stackPanel.Children.Add(button);

      textBlock = new TextBlock
      {
        Text = "(Press the Animate button to animate the slider\n"
                + "value using the selected EasingFunction.\n"
                + "The slider goes from -0.5 to 1.5. The animation\n"
                + "animates the value to 0 or to 1.)",
      };
      stackPanel.Children.Add(textBlock);

      // When the window is loaded, the window appears under the mouse cursor and flies to
      // its position.
      Vector2F mousePosition = _inputService.MousePosition;

      // The loading animation is a timeline group of three animations:
      // - One animations animates the RenderScale from (0, 0) to its current value.
      // - The other animations animate the X and Y positions from the mouse position 
      //   to current values.
      // The base class AnimatedWindow will apply this timeline group on this window 
      // when the window is loaded.
      TimelineGroup timelineGroup = new TimelineGroup
      {
        new Vector2FFromToByAnimation
        {
          TargetProperty = "RenderScale",
          From = new Vector2F(0, 0),
          Duration = TimeSpan.FromSeconds(0.3),
          EasingFunction = new HermiteEase { Mode = EasingMode.EaseOut },
        },
        new SingleFromToByAnimation
        {
          TargetProperty = "X",     
          From = mousePosition.X, 
          Duration = TimeSpan.FromSeconds(0.3),
        },        
        new SingleFromToByAnimation
        {
          TargetProperty = "Y",     
          From = mousePosition.Y, 
          Duration = TimeSpan.FromSeconds(0.3),
          EasingFunction = new QuadraticEase { Mode = EasingMode.EaseIn },
        },
      };
      // The default FillBehavior is "Hold". But this animation can be removed when it is finished. 
      // It should not "Hold" the animation value. If FillBehavior is set to Hold, we cannot
      // drag the window with the mouse because the animation overrides the value.
      timelineGroup.FillBehavior = FillBehavior.Stop;
      LoadingAnimation = timelineGroup;

      // The closing animation is a timeline group of three animations:
      // - One animations animates the RenderScale to (0, 0).
      // - The other animations animate the X and Y positions to the mouse position.
      // The base class AnimatedWindow will apply this timeline group on this window 
      // when the window is loaded.
      ClosingAnimation = new TimelineGroup
      {
        new Vector2FFromToByAnimation
        {
          TargetProperty = "RenderScale",
          To = new Vector2F(0, 0),
          Duration = TimeSpan.FromSeconds(0.3),
          EasingFunction = new HermiteEase { Mode = EasingMode.EaseIn },
        },
        new SingleFromToByAnimation
        {
          TargetProperty = "X",
          To = mousePosition.X,
          Duration = TimeSpan.FromSeconds(0.3),
        },        
        new SingleFromToByAnimation
        {
          TargetProperty = "Y",
          To = mousePosition.Y,
          Duration = TimeSpan.FromSeconds(0.3),
          EasingFunction = new QuadraticEase { Mode = EasingMode.EaseOut },
        },
      };
    }


    // Called when the "Animate" button was clicked.
    private void OnButtonClicked(object sender, EventArgs eventArgs)
    {
      // Get the selected easing function from the DropDownButton.
      EasingFunction easingFunction = (EasingFunction)_functionDropDown.Items[_functionDropDown.SelectedIndex];

      // Set the selected easing mode.
      easingFunction.Mode = (EasingMode)_modeDropDown.Items[_modeDropDown.SelectedIndex];

      // The current slider value:
      float startValue = _slider.Value;

      // Set the slider to the target value.
      _slider.Value = (startValue > 0.5f) ? 0 : 1;

      // Create a from-animation that uses the easing function. It animates the slider
      // value from startValue to the new value.
      SingleFromToByAnimation animation = new SingleFromToByAnimation
      {
        TargetProperty = "Value",
        From = startValue,
        Duration = TimeSpan.FromSeconds(1),
        EasingFunction = easingFunction,
        FillBehavior = FillBehavior.Stop,  // Stop the animation when it is finished. 
      };

      // Start the animation.
      // (Use the Replace transition to replace any currently running animations.
      // This is necessary because the user could press the Animate button while 
      // an animation is running.)
      _animationService.StartAnimation(animation, _slider, AnimationTransitions.Replace())
                       .UpdateAndApply();  // Apply new animation value immediately.
    }
  }
}
