#if SILVERLIGHT || WINDOWS_PHONE
using System.Globalization;
using DigitalRune.Windows.Tests;
#if NETFX_CORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestFixtureSetUp = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.ClassInitializeAttribute;
using TestFixtureTearDown = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.ClassCleanupAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#else
using NUnit.Framework;
#endif


namespace System.Windows.Tests
{
    [TestFixture]
    public class GridLengthConverterTest
    {
        [Test]
        public void CanConvertFromTest()
        {
            var converter = new GridLengthConverter();
            Assert.IsTrue(converter.CanConvertFrom(typeof(string)));
            Assert.IsTrue(converter.CanConvertFrom(typeof(int)));
            Assert.IsTrue(converter.CanConvertFrom(typeof(double)));
            Assert.IsFalse(converter.CanConvertFrom(typeof(object)));
            Assert.IsFalse(converter.CanConvertFrom(typeof(GridLength)));
        }


        [Test]
        public void CanConvertToTest()
        {
            var converter = new GridLengthConverter();
            Assert.IsTrue(converter.CanConvertTo(typeof(string)));
            Assert.IsFalse(converter.CanConvertTo(typeof(GridLength)));
        }


        [Test]
        public void ConvertToShouldThrowWhenValueIsNull()
        {
            var converter = new GridLengthConverter();
            AssertHelper.Throws<ArgumentNullException>(() => converter.ConvertTo(null, CultureInfo.InvariantCulture, null, typeof(string)));
        }


        [Test]
        public void ConvertToShouldThrowWhenValueIsWrongType()
        {
            var converter = new GridLengthConverter();
            AssertHelper.Throws<ArgumentException>(() => converter.ConvertTo(null, CultureInfo.InvariantCulture, new object(), typeof(string)));
        }


        [Test]
        public void ConvertToShouldThrowWhenTypeIsNull()
        {
            var converter = new GridLengthConverter();
            AssertHelper.Throws<ArgumentNullException>(() => converter.ConvertTo(null, CultureInfo.InvariantCulture, new GridLength(1.0, GridUnitType.Auto), null));
            ;
        }


        [Test]
        public void ConvertToShouldThrowWhenTypeIsWrong()
        {
            var converter = new GridLengthConverter();
            AssertHelper.Throws<ArgumentException>(() => converter.ConvertTo(null, CultureInfo.InvariantCulture, new GridLength(1.0, GridUnitType.Auto), typeof(double)));
        }


        [Test]
        public void ConvertToTest()
        {
            var converter = new GridLengthConverter();
            Assert.AreEqual("Auto", converter.ConvertTo(null, CultureInfo.InvariantCulture, new GridLength(1.0, GridUnitType.Auto), typeof(string)));
            Assert.AreEqual("*", converter.ConvertTo(null, CultureInfo.InvariantCulture, new GridLength(1.0, GridUnitType.Star), typeof(string)));
            Assert.AreEqual("1.5*", converter.ConvertTo(null, CultureInfo.InvariantCulture, new GridLength(1.5, GridUnitType.Star), typeof(string)));
            Assert.AreEqual("100", converter.ConvertTo(null, CultureInfo.InvariantCulture, new GridLength(100, GridUnitType.Pixel), typeof(string)));
        }


        [Test]
        public void ConvertFromShouldThrowWhenValueIsNull()
        {
            var converter = new GridLengthConverter();
            AssertHelper.Throws<ArgumentNullException>(() => converter.ConvertFrom(null, CultureInfo.InvariantCulture, null));
            ;
        }


        [Test]
        public void ConvertFromTest()
        {
            var converter = new GridLengthConverter();
            Assert.AreEqual(new GridLength(1.0, GridUnitType.Auto), converter.ConvertFrom(null, CultureInfo.InvariantCulture, "Auto"));
            Assert.AreEqual(new GridLength(1.0, GridUnitType.Star), converter.ConvertFrom(null, CultureInfo.InvariantCulture, "*"));
            Assert.AreEqual(new GridLength(1.5, GridUnitType.Star), converter.ConvertFrom(null, CultureInfo.InvariantCulture, "1.5*"));
            Assert.AreEqual(new GridLength(100, GridUnitType.Pixel), converter.ConvertFrom(null, CultureInfo.InvariantCulture, "100"));
            Assert.AreEqual(new GridLength(100, GridUnitType.Pixel), converter.ConvertFrom(null, CultureInfo.InvariantCulture, "100 px"));
            Assert.AreEqual(new GridLength(96, GridUnitType.Pixel), converter.ConvertFrom(null, CultureInfo.InvariantCulture, "1 in"));
            Assert.AreEqual(new GridLength(37.795275590551178, GridUnitType.Pixel), converter.ConvertFrom(null, CultureInfo.InvariantCulture, "1 cm"));
            Assert.AreEqual(new GridLength(1.3333333333333333, GridUnitType.Pixel), converter.ConvertFrom(null, CultureInfo.InvariantCulture, "1 pt"));

            Assert.AreEqual(new GridLength(1.0, GridUnitType.Auto), converter.ConvertFrom(null, CultureInfo.InvariantCulture, double.NaN));
            Assert.AreEqual(new GridLength(100, GridUnitType.Pixel), converter.ConvertFrom(null, CultureInfo.InvariantCulture, 100.0));
        }
    }
}
#endif
