using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;


namespace DigitalRune.CommandLine.Tests
{
    [TypeConverter(typeof(CustomTypeConverter))]
    class CustomType
    {
        public string Value;
        public CustomType(string value)
        {
            Value = value;
        }
    }


    class CustomTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return new CustomType((string)value);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
                return ((CustomType)value).Value;

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }


    [TestFixture]
    public class ValueArgumentTest
    {
        [Test]
        public void ParseFloat()
        {
            ValueArgument<float> argument = new ValueArgument<float>("float", "Description")
            {
                Category = "Category",
                IsOptional = true,
            };

            Assert.AreEqual("float", argument.Name);
            Assert.AreEqual("Description", argument.Description);
            Assert.AreEqual("Category", argument.Category);

            Assert.IsFalse(string.IsNullOrEmpty(argument.GetSyntax()));
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetHelp()));
            Assert.IsFalse(argument.AllowMultiple);

            int i = 1;
            string[] args = { "other", "123.4", "-456", "other" };
            var result = (ArgumentResult<float>)argument.Parse(args, ref i);
            Assert.AreEqual(2, i);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(123.4f, result.Values[0]);
        }


        [Test]
        public void ParseFloats()
        {
            ValueArgument<float> argument = new ValueArgument<float>("float", "Description")
            {
                Category = "Category",
                IsOptional = true,
                AllowMultiple = true,
            };

            Assert.AreEqual("float", argument.Name);
            Assert.AreEqual("Description", argument.Description);
            Assert.AreEqual("Category", argument.Category);
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetSyntax()));
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetHelp()));
            Assert.IsTrue(argument.AllowMultiple);

            int i = 1;
            string[] args = { "other", "123.4", "-567.8", };
            var result = (ArgumentResult<float>)argument.Parse(args, ref i);
            Assert.AreEqual(3, i);
            Assert.AreEqual(2, result.Values.Count);
            Assert.AreEqual(123.4f, result.Values[0]);
            Assert.AreEqual(-567.8f, result.Values[1]);
        }


        [Test]
        public void ParseCustomType()
        {
            ValueArgument<CustomType> argument = new ValueArgument<CustomType>("Custom", "")
            {
                AllowMultiple = true
            };

            int i = 1;
            string[] args = { "other", "Xyz", "12345" };
            var result = (ArgumentResult<CustomType>)argument.Parse(args, ref i);
            Assert.AreEqual(3, i);
            Assert.AreEqual(2, result.Values.Count);
            Assert.AreEqual("Xyz", result.Values[0].Value);
            Assert.AreEqual("12345", result.Values[1].Value);
        }
    }
}
