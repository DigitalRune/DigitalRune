using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Game.UI
{
  // A UIScreen which displays a Window. The Window contains an Image control.
  class NormalUIScreen : UIScreen
  {
    private readonly Window _window;

    public Image Image { get; private set; }


    public NormalUIScreen(IUIRenderer renderer)
      : base("Normal", renderer)
    {
      Image = new Image
      {
        Width = 800,
        Height = 450,
      };

      _window = new Window
      {
        X = 100,
        Y = 50,
        Title = "3D Scene (Click scene to control camera. Press <Esc> to leave scene.)",
        CanResize = true,
        CloseButtonStyle = null,     // Hide close button.
        Content = new ScrollViewer
        {
          HorizontalAlignment = HorizontalAlignment.Stretch,
          VerticalAlignment = VerticalAlignment.Stretch,
          Content = Image
        },
      };
      _window.Show(this);
    }


    protected override void OnLoad()
    {
      base.OnLoad();

      // The window is resizable. Limit the max size.
      // Measure() computes the DesiredWidth and DesiredHeight.
      _window.Measure(new Vector2F(float.PositiveInfinity));
      _window.MaxWidth = _window.DesiredWidth;
      _window.MaxHeight = _window.DesiredHeight;
    }
  }
}
