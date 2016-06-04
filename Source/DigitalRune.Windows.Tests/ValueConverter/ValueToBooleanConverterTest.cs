#if !SILVERLIGHT && !WINDOWS_PHONE
using System;
using System.Globalization;
using System.Windows;
using NUnit.Framework;


namespace DigitalRune.Windows.Tests
{
    internal enum MockEnum { Value1, Value2, Value3 }


    [TestFixture]
    public class ValueToBooleanConverterTest
    {
        [Test]
        public void DefaultValueShouldBeNull()
        {
            ValueToBooleanConverter valueConverter = new ValueToBooleanConverter();
            Assert.IsNull(valueConverter.DefaultValue);
        }


        [Test]
        public void DefaultValueShouldEqualValue()
        {
            ValueToBooleanConverter valueConverter = new ValueToBooleanConverter { DefaultValue = 123 };
            Assert.AreEqual(123, valueConverter.DefaultValue);

            valueConverter.DefaultValue = "Value";
            Assert.AreEqual("Value", valueConverter.DefaultValue);
        }


        [TestCase(MockEnum.Value1, typeof(bool), null, false)]
        [TestCase(MockEnum.Value1, typeof(bool?), null, false)]
        [TestCase(MockEnum.Value1, typeof(bool), MockEnum.Value2, false)]
        [TestCase(MockEnum.Value1, typeof(bool?), MockEnum.Value2, false)]
        [TestCase(MockEnum.Value1, typeof(bool), MockEnum.Value1, true)]
        [TestCase(MockEnum.Value1, typeof(bool?), MockEnum.Value1, true)]
        [TestCase(MockEnum.Value1, typeof(bool), "Value2", false)]
        [TestCase(MockEnum.Value1, typeof(bool?), "Value2", false)]
        [TestCase(MockEnum.Value1, typeof(bool), "Value1", true)]
        [TestCase(MockEnum.Value1, typeof(bool?), "Value1", true)]
        public void Convert(object value, Type targetType, object parameter, object result)
        {
            ValueToBooleanConverter valueConverter = new ValueToBooleanConverter();
            Assert.AreEqual(result, valueConverter.Convert(value, targetType, parameter, CultureInfo.InvariantCulture));
        }


        [TestCase(false, typeof(MockEnum), MockEnum.Value3, MockEnum.Value1, MockEnum.Value1)]
        [TestCase(true, typeof(MockEnum), MockEnum.Value3, MockEnum.Value1, MockEnum.Value3)]
        [TestCase(false, typeof(MockEnum), "Value3", MockEnum.Value1, MockEnum.Value1)]
        [TestCase(true, typeof(MockEnum), "Value3", MockEnum.Value1, MockEnum.Value3)]
        [TestCase(false, typeof(MockEnum), MockEnum.Value3, "Value1", MockEnum.Value1)]
        [TestCase(true, typeof(MockEnum), MockEnum.Value3, "Value1", MockEnum.Value3)]
        [TestCase(false, typeof(MockEnum), "Value3", "Value1", MockEnum.Value1)]
        [TestCase(true, typeof(MockEnum), "Value3", "Value1", MockEnum.Value3)]
        public void ConvertBack(object value, Type targetType, object parameter, object defaultValue, object result)
        {
            ValueToBooleanConverter valueConverter = new ValueToBooleanConverter { DefaultValue = defaultValue };
            Assert.AreEqual(result, valueConverter.ConvertBack(value, targetType, parameter, CultureInfo.InvariantCulture));
        }


        [TestCase(null, typeof(MockEnum), MockEnum.Value3, MockEnum.Value1)]
        [TestCase(false, typeof(MockEnum), MockEnum.Value3, null)]
        [TestCase(true, typeof(MockEnum), null, MockEnum.Value1)]
        public void ConvertBackShouldFail(object value, Type targetType, object parameter, object defaultValue)
        {
            ValueToBooleanConverter valueConverter = new ValueToBooleanConverter { DefaultValue = defaultValue };
            Assert.AreEqual(DependencyProperty.UnsetValue, valueConverter.ConvertBack(value, targetType, parameter, CultureInfo.InvariantCulture));
        }
    }
}
#endif
