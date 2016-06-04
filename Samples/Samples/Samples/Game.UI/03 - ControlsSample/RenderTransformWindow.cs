using System;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Game.UI
{
  /// Demonstrates the use of a render transforms.
  public class RenderTransformWindow : Window
  {
    private readonly CheckBox _checkBox;
    private readonly Button _button;
    private float _rotation;


    public RenderTransformWindow()
    {
      Title = "RenderTransform Demo";

      // Ensure that the child controls are drawn outside the window.
      ClipContent = true;

      var textBlock = new TextBlock
      {
        X = 10,
        Y = 10,
        Text = "All controls can be scaled, rotated and translated:"
      };

      _checkBox = new CheckBox
      {
        X = 10,
        Y = 30,
        Content = new TextBlock { Text = "Enable RenderTransform" },
      };

      _button = new Button
      {
        X = 100,
        Y = 150,
        Content = new TextBlock { Text = "Button with\nRenderTransform" },
      };

      var canvas = new Canvas
      {
        Width = 315,
        Height = 250,
      };
      canvas.Children.Add(textBlock);
      canvas.Children.Add(_checkBox);
      canvas.Children.Add(_button);

      Content = canvas;
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      base.OnUpdate(deltaTime);

      if (_checkBox.IsChecked)
      {
        // ----- Animation
        // We want the button to rotate around its center, therefore we set the render 
        // transform origin to the center of the control.
        _button.RenderTransformOrigin = new Vector2F(0.5f, 0.5f);

        // Scale the button.
        _button.RenderScale = new Vector2F(2, 2);

        // Rotate the button by 1° per frame.
        _rotation = (_rotation + 1) % 360;
        _button.RenderRotation = MathHelper.ToRadians(_rotation);
      }
      else
      {
        // ----- No animation
        // Undo scale and rotation.
        _button.RenderScale = new Vector2F(1, 1);
        _button.RenderRotation = 0;
      }
    }
  }
}
