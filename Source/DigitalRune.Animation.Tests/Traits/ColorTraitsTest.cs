using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRune.Animation.Traits.Tests
{
  [TestFixture]
  public class ColorTraitsTest
  {
    [Test]
    public void IdentityTest()
    {
      var traits = ColorTraits.Instance;
      var value = new Color(0.1f, 0.2f, 0.3f, 1.0f);
      Assert.AreEqual(value, traits.Add(value, traits.Identity()));
      Assert.AreEqual(value, traits.Add(traits.Identity(), value));
    }


    [Test]
    public void MultiplyTest()
    {
      var traits = ColorTraits.Instance;
      var value = new Color(0.1f, 0.2f, 0.3f, 1.0f);
      Assert.AreEqual(new Color(0, 0, 0, 0), traits.Multiply(value, 0));
      Assert.AreEqual(value, traits.Multiply(value, 1));
      Assert.AreEqual(new Color(value.ToVector4() + value.ToVector4()), traits.Multiply(value, 2));
      Assert.AreEqual(new Color(value.ToVector4() + value.ToVector4() + value.ToVector4()), traits.Multiply(value, 3));
    }


    [Test]
    public void FromByTest()
    {
      // IAnimationValueTraits<T> is used in a from-by animation to a add a relative offset to
      // the start value.

      var traits = ColorTraits.Instance;
      var from = new Color(0.1f, 0.2f, 0.3f, 1.0f);
      var by = new Color(0.4f, 0.5f, 0.6f, 0.5f);

      var to = traits.Add(from, by);
      Assert.AreEqual(new Color(by.ToVector4() + from.ToVector4()), to);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void InterpolationTest()
    {
      var traits = ColorTraits.Instance;
      var value0 = new Color(0.1f, 0.2f, 0.3f, 1.0f);
      var value1 = new Color(0.4f, 0.5f, 0.6f, 0.5f);
      Assert.AreEqual(value0, traits.Interpolate(value0, value1, 0.0f));
      Assert.AreEqual(value1, traits.Interpolate(value0, value1, 1.0f));
      Assert.AreEqual(new Color(0.25f * value0.ToVector4() + 0.75f * value1.ToVector4()), traits.Interpolate(value0, value1, 0.75f));
    }
  }
}
