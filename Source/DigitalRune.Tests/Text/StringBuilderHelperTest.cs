using System.Globalization;
using System.Text;
using System.Threading;
using NUnit.Framework;


namespace DigitalRune.Text.Tests
{
  [TestFixture]
  public class StringBuilderHelperTest
  {
    [Test]
    public void AppendIntTest()
    {
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
      StringBuilder stringBuilder = new StringBuilder();

      stringBuilder.AppendNumber(1234567890);
      Assert.AreEqual("1234567890", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(-1234567890);
      Assert.AreEqual("-1234567890", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(1234567890, AppendNumberOptions.None);
      Assert.AreEqual("1234567890", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(1234567890, AppendNumberOptions.PositiveSign);
      Assert.AreEqual("+1234567890", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(1234567890, AppendNumberOptions.NumberGroup);
      Assert.AreEqual("1,234,567,890", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(1234567890, AppendNumberOptions.PositiveSign | AppendNumberOptions.NumberGroup);
      Assert.AreEqual("+1,234,567,890", stringBuilder.ToString());
    }


    [Test]
    public void AppendFloatTest()
    {
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
      StringBuilder stringBuilder = new StringBuilder();

      stringBuilder.Clear();
      stringBuilder.AppendNumber(float.NaN);
      Assert.AreEqual("NaN", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(float.PositiveInfinity);
      Assert.AreEqual("+Infinity", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(float.NegativeInfinity);
      Assert.AreEqual("-Infinity", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(1234.12645f);
      Assert.AreEqual("1234.13", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(1.23456f, 4, AppendNumberOptions.NumberGroup);
      Assert.AreEqual("1.2346", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(-1234.12645f);
      Assert.AreEqual("-1234.12", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(1234.12645f, AppendNumberOptions.None);
      Assert.AreEqual("1234.13", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(1234.12645f, AppendNumberOptions.PositiveSign);
      Assert.AreEqual("+1234.13", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(1234.12645f, AppendNumberOptions.NumberGroup);
      Assert.AreEqual("1,234.13", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(1234.12645f, AppendNumberOptions.PositiveSign | AppendNumberOptions.NumberGroup);
      Assert.AreEqual("+1,234.13", stringBuilder.ToString());

      stringBuilder.Clear();
      stringBuilder.AppendNumber(123412600f, AppendNumberOptions.PositiveSign | AppendNumberOptions.NumberGroup);
      Assert.AreEqual("+123,412,600.00", stringBuilder.ToString()); // Note: Value differs because of limited precision of floating-point.

      // If the number is too long the AppendNumber falls back to StringBuilder.Append() and uses
      // exponential notation.
      stringBuilder.Clear();
      stringBuilder.AppendNumber(1234126000000000000000f);
      Assert.IsTrue(stringBuilder.ToString().Length > 0);
    }
  }
}
