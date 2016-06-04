using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  // The TextNode contains only a text (string) and a color.
  public class TextNode : SceneNode
  {
    // ----- The properties of the new scene node.
    public Color Color { get; set; }
    public string Text { get; set; }


    // ----- Constructor: Set relevant scene node properties.
    public TextNode()
    {
      // The IsRenderable flag needs to be set to indicate that the scene node should 
      // be handled during rendering.
      IsRenderable = true;

      // The CastsShadows flag needs to be set if the scene node needs to be rendered 
      // into the shadow maps. But in this case the scene node should be ignored.
      CastsShadows = false;

      // A bounding shape needs to be set for frustum culling.
      Shape = new PointShape();
    }


    // ----- The following methods are required by the cloning mechanism:

    // CreateInstanceCore() is called when a clone needs to be created.
    protected override SceneNode CreateInstanceCore()
    {
      return new TextNode();
    }


    // CloneCore() is called to initialize the clone.
    protected override void CloneCore(SceneNode source)
    {
      // Clone the SceneNode properties (base class).
      base.CloneCore(source);

      // Clone the TextNode properties.
      var sourceTextNode = (TextNode)source;
      Color = sourceTextNode.Color;
      Text = sourceTextNode.Text;
    }
  }
}