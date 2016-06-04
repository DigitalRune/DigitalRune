using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Game.UI
{
  // A resizable window with a ScrollViewer.
  public class ResizableWindow : Window
  {
    public ResizableWindow(ContentManager content)
    {
      Title = "Resizable Window";
      Width = 320;
      Height = 240;
      CanResize = true;

      var image = new Image
      {
        Texture = content.Load<Texture2D>("Sky"),
      };

      var scrollViewer = new ScrollViewer
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        Content = image
      };

      Content = scrollViewer;
    }
  }
}
