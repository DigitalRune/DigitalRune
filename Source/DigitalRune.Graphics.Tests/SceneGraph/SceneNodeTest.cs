using NUnit.Framework;


namespace DigitalRune.Graphics.SceneGraph.Tests
{
  [TestFixture]
  public class SceneNodeTest
  {
    [Test]
    public void UserFlagsTest()
    {
      var sceneNode = new SceneNode();
      sceneNode.UserFlags = 0x5555;
      Assert.AreEqual(0x5555, sceneNode.UserFlags);
    }
  }
}
