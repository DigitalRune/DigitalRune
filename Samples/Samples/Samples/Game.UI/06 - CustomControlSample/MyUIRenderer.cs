using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using Microsoft.Xna.Framework;


namespace Samples.Game.UI
{
  // This UIRenderer adds a render callback for TreeViewItems.
  internal class MyUIRenderer : UIRenderer
  {
    public MyUIRenderer(Microsoft.Xna.Framework.Game game, Theme theme)
      : base(game, theme)
    {
      // Add a new render method for the style "TreeViewItem".
      RenderCallbacks.Add("TreeViewItem", RenderTreeViewItem);
    }


    public void RenderTreeViewItem(UIControl control, UIRenderContext context)
    {
      var treeViewItem = control as TreeViewItem;
      if (treeViewItem != null && treeViewItem.IsSelected && treeViewItem.Header != null)
      {
        // Draw a blue rectangle behind selected tree view items.
        context.RenderTransform.Draw(
          SpriteBatch,
          WhiteTexture,
          treeViewItem.Header.ActualBounds,
          null,
          Color.CornflowerBlue);
      }

      // Call the default render callback to draw all the rest.
      RenderCallbacks["UIControl"](control, context);
    }
  }
}
