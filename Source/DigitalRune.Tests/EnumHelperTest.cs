using System;
using NUnit.Framework;


namespace DigitalRune.Tests
{
  public enum MyEnum
  {
    Value1 = 0,
    Value2 = 10,
    Value3 = 20,
  }


  [TestFixture]
  public class EnumHelperTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetValuesShouldThrowWhenParamIsNull()
    {
      EnumHelper.GetValues(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void GetValuesShouldThrowWhenParamIsNotAnEnumeration()
    {
      EnumHelper.GetValues(typeof(EnumHelperTest));
    }


    [Test]
    public void GetValues()
    {
      Array values = EnumHelper.GetValues(typeof(MyEnum));
      Assert.IsNotNull(values);
      Assert.AreEqual(3, values.Length);
      Assert.AreEqual(MyEnum.Value1, values.GetValue(0));
      Assert.AreEqual(MyEnum.Value2, values.GetValue(1));
      Assert.AreEqual(MyEnum.Value3, values.GetValue(2));
    }


    [Test]
    public void TryParseShouldFailWhenStringIsNull()
    {
      MyEnum value;
      bool result = EnumHelper.TryParse(null, false, out value);
      Assert.IsFalse(result);
    }

    
    [Test]
    public void TryParseShouldFailWhenStringIsEmpty()
    {
      MyEnum value;
      bool result = EnumHelper.TryParse("", true, out value);
      Assert.IsFalse(result);
    }


    [Test]
    public void TryParseShouldFailWhenStringIsUnknown()
    {
      MyEnum value;
      bool result = EnumHelper.TryParse("Value4", true, out value);
      Assert.IsFalse(result);
    }


    [Test]
    public void TryParseCaseSensitive()
    {
      MyEnum value;
      bool result = EnumHelper.TryParse("value1", false, out value);
      Assert.IsFalse(result);

      result = EnumHelper.TryParse("Value2", false, out value);
      Assert.IsTrue(result);
      Assert.AreEqual(MyEnum.Value2, value);
    }


    [Test]
    public void TryParseCaseInsensitive()
    {
      MyEnum value;
      bool result = EnumHelper.TryParse("value2", true, out value);
      Assert.IsTrue(result);
      Assert.AreEqual(MyEnum.Value2, value);
    }


    [Test]
    public void ConvertNumberToEnum()
    {
      MyEnum value;
      bool result = EnumHelper.TryParse("10", true, out value);
      Assert.IsTrue(result);
      Assert.AreEqual(MyEnum.Value2, value);
    }
  }
}
