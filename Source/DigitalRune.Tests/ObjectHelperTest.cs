using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using DigitalRune.Mathematics;
using NUnit.Framework;

#if !NETFX_CORE && !SILVERLIGHT
using System.Drawing;
#endif


namespace DigitalRune.Tests
{
  public struct VectorA
  {
    public float X, Y;


    public static VectorA Parse(string text, IFormatProvider provider)
    {
      Regex r = new Regex(@"\((?<x>.*);(?<y>.*)\)", RegexOptions.None);
      Match m = r.Match(text);
      if (m.Success)
      {
        var result = new VectorA();
        result.X = float.Parse(m.Groups["x"].Value, provider);
        result.Y = float.Parse(m.Groups["y"].Value, provider);
        return result;
      }

      throw new FormatException("String is not a valid VectorA.");
    }
  }

  public struct VectorB
  {
    public float X, Y;


    public static VectorB Parse(string text)
    {
      Regex r = new Regex(@"\((?<x>.*);(?<y>.*)\)", RegexOptions.None);
      Match m = r.Match(text);
      if (m.Success)
      {
        var result = new VectorB();
        result.X = float.Parse(m.Groups["x"].Value);
        result.Y = float.Parse(m.Groups["y"].Value);
        return result;
      }

      throw new FormatException("String is not a valid Vector2.");
    }
  }

  public struct VectorC
  {
  }


  [TestFixture]
  public class GetPropertyNameTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ExtensionMethodShouldThrowIfExpessionIsNull()
    {
      TestObject test = new TestObject();
      Assert.AreEqual("ObjectProperty", test.GetPropertyName((Expression<Func<TestObject, object>>)null));      
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void StaticMethodShouldThrowIfExpessionIsNull()
    {
      Assert.AreEqual("ObjectProperty", ObjectHelper.GetPropertyName((Expression<Func<object>>)null));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ExtensionMethodShouldThrowIfExpessionIsInvalid()
    {
      TestObject test = new TestObject();
      Assert.AreEqual("ObjectProperty", test.GetPropertyName(o => o.Func()));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void StaticMethodShouldThrowIfExpessionIsInvalid()
    {
      TestObject test = new TestObject();
      Assert.AreEqual("ObjectProperty", ObjectHelper.GetPropertyName(() => test.Func()));
    }


    [Test]
    public void GetNameUsingExtensionMethod()
    {
      TestObject test = new TestObject();
      Assert.AreEqual("ValueProperty", test.GetPropertyName(x => x.ValueProperty));
      Assert.AreEqual("ObjectProperty", test.GetPropertyName(x => x.ObjectProperty));
    }


    [Test]
    public void GetPropertyName()
    {
      TestObject test = new TestObject();
      Assert.AreEqual("ValueProperty", ObjectHelper.GetPropertyName(() => test.ValueProperty));
      Assert.AreEqual("ObjectProperty", ObjectHelper.GetPropertyName(() => test.ObjectProperty));
    }


    private sealed class TestObject
    {
      public int ValueProperty { get; set; }
      public object ObjectProperty { get; set; }
      public object Func() { return null; }
    }


#if !NETFX_CORE && !SILVERLIGHT
    [Test]
    public void GetTypeConverter()
    {
      var converter = ObjectHelper.GetTypeConverter(typeof(Size));
      Assert.IsNotNull(converter);
    }
#endif


    [Flags]
    private enum TestEnum
    {
      A = 1,
      B = 2,
      C = 3,
      D = 4,
    }


    [Test]
    public void Parse()
    {
      // Parse string.
      string str = ObjectHelper.Parse<string>("blah");
      Assert.AreEqual("blah", str);

      var f = ObjectHelper.Parse<float>("-10e5");
      Assert.AreEqual(-10e5f, f);

      // Parse an enumeration.
      TestEnum e = ObjectHelper.Parse<TestEnum>("A, C, D");
      Assert.AreEqual(TestEnum.A | TestEnum.C | TestEnum.D, e);

      var va = (VectorA)ObjectHelper.Parse(typeof(VectorA), "(1.01; 2.02)");
      Assert.IsTrue(Numeric.AreEqual(va.X, 1.01f));
      Assert.IsTrue(Numeric.AreEqual(va.Y, 2.02f));

#if WINDOWS_PHONE
      var vb = (VectorB)ObjectHelper.Parse(typeof(VectorB), "(1.01; 2.02)");
#else
      var vb = (VectorB)ObjectHelper.Parse(typeof(VectorB), "(1,01; 2,02)");      // Use this if culture in the test is de.
#endif
      Assert.IsTrue(Numeric.AreEqual(vb.X, 1.01f));
      Assert.IsTrue(Numeric.AreEqual(vb.Y, 2.02f));

      Assert.Throws(typeof(NotSupportedException), () => ObjectHelper.Parse<VectorC>("(1.01; 2.02)"));
    }
    

#if !NETFX_CORE && !SILVERLIGHT
    [Test]
    public void ParseWithTypeConverter()
    {
      // Parse a thing that has a TypeConverter.
      var parsedSize = ObjectHelper.Parse<Size>("10, 20");
      Assert.AreEqual(10, parsedSize.Width);
      Assert.AreEqual(20, parsedSize.Height);
    }
#endif
  }
}
