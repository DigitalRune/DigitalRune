using NUnit.Framework;


namespace DigitalRune.Graphics.Content.Tests
{
  [TestFixture]
  public class WrapModeTest
  {
    [Test]
    public void WrapMirrorTest()
    {
      Assert.AreEqual(0, TextureHelper.WrapMirror(-3, 1));
      Assert.AreEqual(0, TextureHelper.WrapMirror(-2, 1));
      Assert.AreEqual(0, TextureHelper.WrapMirror(-1, 1));
      Assert.AreEqual(0, TextureHelper.WrapMirror(0, 1));
      Assert.AreEqual(0, TextureHelper.WrapMirror(1, 1));
      Assert.AreEqual(0, TextureHelper.WrapMirror(2, 1));
      Assert.AreEqual(0, TextureHelper.WrapMirror(3, 1));

      Assert.AreEqual(0, TextureHelper.WrapMirror(-5, 2));
      Assert.AreEqual(0, TextureHelper.WrapMirror(-4, 2));
      Assert.AreEqual(1, TextureHelper.WrapMirror(-3, 2));
      Assert.AreEqual(1, TextureHelper.WrapMirror(-2, 2));
      Assert.AreEqual(0, TextureHelper.WrapMirror(-1, 2));
      Assert.AreEqual(0, TextureHelper.WrapMirror(0, 2));
      Assert.AreEqual(1, TextureHelper.WrapMirror(1, 2));
      Assert.AreEqual(1, TextureHelper.WrapMirror(2, 2));
      Assert.AreEqual(0, TextureHelper.WrapMirror(3, 2));
      Assert.AreEqual(0, TextureHelper.WrapMirror(4, 2));
      Assert.AreEqual(1, TextureHelper.WrapMirror(5, 2));

      Assert.AreEqual(2, TextureHelper.WrapMirror(-9, 3));
      Assert.AreEqual(1, TextureHelper.WrapMirror(-8, 3));
      Assert.AreEqual(0, TextureHelper.WrapMirror(-7, 3));
      Assert.AreEqual(0, TextureHelper.WrapMirror(-6, 3));
      Assert.AreEqual(1, TextureHelper.WrapMirror(-5, 3));
      Assert.AreEqual(2, TextureHelper.WrapMirror(-4, 3));
      Assert.AreEqual(2, TextureHelper.WrapMirror(-3, 3));
      Assert.AreEqual(1, TextureHelper.WrapMirror(-2, 3));
      Assert.AreEqual(0, TextureHelper.WrapMirror(-1, 3));
      Assert.AreEqual(0, TextureHelper.WrapMirror(0, 3));
      Assert.AreEqual(1, TextureHelper.WrapMirror(1, 3));
      Assert.AreEqual(2, TextureHelper.WrapMirror(2, 3));
      Assert.AreEqual(2, TextureHelper.WrapMirror(3, 3));
      Assert.AreEqual(1, TextureHelper.WrapMirror(4, 3));
      Assert.AreEqual(0, TextureHelper.WrapMirror(5, 3));
      Assert.AreEqual(0, TextureHelper.WrapMirror(6, 3));
      Assert.AreEqual(1, TextureHelper.WrapMirror(7, 3));
      Assert.AreEqual(2, TextureHelper.WrapMirror(8, 3));
    }
  }
}
