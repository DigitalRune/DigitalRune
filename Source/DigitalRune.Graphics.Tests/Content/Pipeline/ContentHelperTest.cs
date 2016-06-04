using NUnit.Framework;


namespace DigitalRune.Graphics.Content.Pipeline.Tests
{
  [TestFixture]
  public class ContentHelperTest
  {
    [Test]
    public void IsValidTypeForEffectParameter()
    {
      Assert.IsTrue(ContentHelper.IsValidTypeForEffectParameter(1.0f));
      Assert.IsTrue(ContentHelper.IsValidTypeForEffectParameter(new float[3]));
      Assert.IsFalse(ContentHelper.IsValidTypeForEffectParameter(100L));
      Assert.IsFalse(ContentHelper.IsValidTypeForEffectParameter(new string[3]));
    }


    [Test]
    public void ParseSceneNodeName()
    {
      string name;
      int? lod;

      ContentHelper.ParseSceneNodeName(null, out name, out lod);
      Assert.AreEqual(null, name);
      Assert.IsFalse(lod.HasValue);

      ContentHelper.ParseSceneNodeName("", out name, out lod);
      Assert.AreEqual("", name);
      Assert.IsFalse(lod.HasValue);

      ContentHelper.ParseSceneNodeName("Name123_LOD0", out name, out lod);
      Assert.AreEqual("Name123", name);
      Assert.AreEqual(0, lod);

      ContentHelper.ParseSceneNodeName("_Name_123__LOD123", out name, out lod);
      Assert.AreEqual("_Name_123_", name);
      Assert.AreEqual(123, lod);

      ContentHelper.ParseSceneNodeName("_Name_123__lod123", out name, out lod);
      Assert.AreEqual("_Name_123_", name);
      Assert.AreEqual(123, lod);

      ContentHelper.ParseSceneNodeName("_LOD123", out name, out lod);
      Assert.AreEqual("_LOD123", name);
      Assert.IsFalse(lod.HasValue);

      ContentHelper.ParseSceneNodeName("xyz_LOD12345678901234567890", out name, out lod);
      Assert.AreEqual("xyz", name);
      Assert.IsFalse(lod.HasValue); // Because of integer overflow. Should not throw.
    }
  }
}