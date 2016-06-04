using System;
using System.Collections.Generic;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // TextRenderer is a custom scene node renderer which draws TextNodes using a 
  // SpriteBatch and a SpriteFont.
  public class TextRenderer : SceneNodeRenderer
  {
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _spriteFont;


    public TextRenderer(IGraphicsService graphicsService, SpriteFont font)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");
      if (font == null)
        throw new ArgumentNullException("font");

      _spriteBatch = new SpriteBatch(graphicsService.GraphicsDevice);
      _spriteFont = font;

      // The TextRenderer should be called after all other scene node renderers.
      // This is only relevant if different types of scene nodes (e.g. MeshNodes, 
      // TextNodes, ...) are rendered at the same time.
      Order = 100;
    }


    // CanRender() checks whether a given scene node can be rendered with this
    // scene node renderer.
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is TextNode;
    }


    // Render() draws a list of scene nodes.
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      // For simplicity we ignore the 'order' parameter and do not sort the TextNodes
      // by distance.
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var cameraNode = context.CameraNode;
      if (cameraNode == null)
        return; // No camera set.

      Matrix view = (Matrix)cameraNode.View;
      Matrix projection = cameraNode.Camera.Projection;
      var viewport = graphicsDevice.Viewport;

      // Use the SpriteBatch for rendering text.
      _spriteBatch.Begin();

      for (int i = 0; i < nodes.Count; i++)
      {
        var node = nodes[i] as TextNode;
        if (node != null)
        {
          // Draw text centered at position of TextNode.
          Vector3 positionWorld = (Vector3)node.PoseWorld.Position;
          Vector3 positionScreen = viewport.Project(positionWorld, projection, view, Matrix.Identity);
          Vector2 position2D = new Vector2(positionScreen.X, positionScreen.Y);
          Vector2 size = _spriteFont.MeasureString(node.Text);
          _spriteBatch.DrawString(_spriteFont, node.Text, position2D - size / 2, node.Color);
        }
      }

      _spriteBatch.End();
    }
  }
}