using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;


namespace Samples.Game.UI
{
  // The window that is displayed in the UIScreen which is rendered onto the 3D objects.
  // The window contains 
  // - text, 
  // - 3 sliders that change the color (RGB) of the owning screen,
  // - a button that drops a new object
  class InGameWindow : Window
  {
    private readonly IServiceLocator _services;


    public InGameWindow(IServiceLocator services)
    {
      _services = services;
      Title = "In-Game GUI Window";
    }


    protected override void OnLoad()
    {
      base.OnLoad();

      var screen = Screen;

      var panel = new Canvas
      {
        Margin = new Vector4F(8),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
      };
      Content = panel;

      panel.Children.Add(new TextBlock
      {
        Text = "The GUI can be used in the 3D game! :-)",
      });

      panel.Children.Add(new TextBlock
      {
        Y = 32,
        Text = "Red: ",
      });

      var slider = new Slider
      {
        X = 52,
        Y = 32,
        Width = 200,
        Minimum = 0,
        Maximum = 255,
        Value = screen.Background.R,
      };
      panel.Children.Add(slider);
      GameProperty<float> sliderValue = slider.Properties.Get<float>("Value");
      sliderValue.Changed += (s, e) =>
      {
        Color color = screen.Background;
        color.R = (byte)e.NewValue;
        screen.Background = color;
      };

      panel.Children.Add(new TextBlock
      {
        X = 0,
        Y = 56,
        Text = "Green: ",
      });
      slider = new Slider
      {
        X = 52,
        Y = 56,
        Width = 200,
        Minimum = 0,
        Maximum = 255,
        Value = screen.Background.G,
      };
      panel.Children.Add(slider);
      sliderValue = slider.Properties.Get<float>("Value");
      sliderValue.Changed += (s, e) =>
      {
        Color color = screen.Background;
        color.G = (byte)e.NewValue;
        screen.Background = color;
      };

      panel.Children.Add(new TextBlock
      {
        X = 0,
        Y = 80,
        Text = "Blue: ",
      });
      slider = new Slider
      {
        X = 52,
        Y = 80,
        Width = 200,
        Minimum = 0,
        Maximum = 255,
        Value = screen.Background.B,
      };
      panel.Children.Add(slider);
      sliderValue = slider.Properties.Get<float>("Value");
      sliderValue.Changed += (s, e) =>
      {
        Color color = screen.Background;
        color.B = (byte)e.NewValue;
        screen.Background = color;
      };

      var button = new Button
      {
        X = 60,
        Y = 110,
        Width = 120,
        ToolTip = "Drop a new box",
        Content = new TextBlock { Text = "Drop" },
      };
      button.Click += (s, e) =>
      {
        // Create a new DynamicObject.
        var gameObjectService = _services.GetInstance<IGameObjectService>();
        gameObjectService.Objects.Add(new DynamicObject(_services, 1));
      };
      panel.Children.Add(button);
    }
  }
}
