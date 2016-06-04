using System;
using DigitalRune.Text;
using NUnit.Framework;


namespace DigitalRune.Tests
{
  [TestFixture]
  public class StringHelperTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComputeMatchShouldThrowArgumentNullException0()
    {
      StringHelper.ComputeMatch(null, "asd");
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComputeMatchShouldThrowArgumentNullException1()
    {
      StringHelper.ComputeMatch("asd", null);
    }


    [Test]
    public void ComputeMatch()
    {
      Assert.AreEqual(0, StringHelper.ComputeMatch("", ""));
      Assert.AreEqual(0, StringHelper.ComputeMatch("", "asdf"));
      Assert.AreEqual(0, StringHelper.ComputeMatch("1", ""));

      Assert.AreEqual(1, StringHelper.ComputeMatch("1", "1"));
      Assert.AreEqual(1, StringHelper.ComputeMatch("asdf", "asdf"));

      Assert.IsTrue(StringHelper.ComputeMatch("asdf", "ASDF") > 0);
      Assert.IsTrue(StringHelper.ComputeMatch("asdf", "blabla_ASDF") > 0);
      Assert.IsTrue(StringHelper.ComputeMatch("asdf", "ASDF") > StringHelper.ComputeMatch("asdf", "blabla_ASDF"));
      Assert.IsTrue(StringHelper.ComputeMatch("asdf", "bla_ASDF") > StringHelper.ComputeMatch("asdf", "AaaSddDddF"));
    }


    [Test]
    public void SplitTextAndNumber()
    {
      string text;
      int number;

      StringHelper.SplitTextAndNumber(null, out text, out number);
      Assert.AreEqual("", text);
      Assert.AreEqual(-1, number);

      "text".SplitTextAndNumber(out text, out number);
      Assert.AreEqual("text", text);
      Assert.AreEqual(-1, number);

      "123".SplitTextAndNumber(out text, out number);
      Assert.AreEqual(String.Empty, text);
      Assert.AreEqual(123, number);

      "text123".SplitTextAndNumber(out text, out number);
      Assert.AreEqual("text", text);
      Assert.AreEqual(123, number);

      "123text".SplitTextAndNumber(out text, out number);
      Assert.AreEqual("123text", text);
      Assert.AreEqual(-1, number);

      "123text456".SplitTextAndNumber(out text, out number);
      Assert.AreEqual("123text", text);
      Assert.AreEqual(456, number);
    }
  }    
}
