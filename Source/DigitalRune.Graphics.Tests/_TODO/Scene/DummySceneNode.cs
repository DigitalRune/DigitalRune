using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Scene3D;


namespace DigitalRune.Graphics.Tests
{
  internal class DummySceneNode : SceneNode
  {
    public DummySceneNode()
    {
      BoundingShape = new SphereShape();
    }
  }
}
