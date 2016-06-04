using System.Windows.Media;
using NUnit.Framework;


namespace DigitalRune.Windows.Tests
{
  [TestFixture]
  public class ColorHelperTest
  {
    [Test]
    public void ToSrgb()
    {
      Assert.AreEqual(Color.FromArgb(), Color.FromArgb().ToSrgb());
      Color sRgb = Color.FromArgb(0, 0, 0, 0);

    }
  }
}
