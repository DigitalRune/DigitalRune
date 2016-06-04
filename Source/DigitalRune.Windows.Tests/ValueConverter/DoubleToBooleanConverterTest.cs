using System;
using System.Globalization;
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



namespace DigitalRune.Windows.Tests
{
    [TestFixture]
    public class DoubleToBooleanConverterTest
    {
        [Test]
        public void ComparisonOperationTest()
        {
            DoubleToBooleanConverter converter = new DoubleToBooleanConverter();
            Assert.AreEqual(ComparisonOperator.Equal, converter.Comparison);

            converter.Comparison = ComparisonOperator.GreaterOrEqual;
            Assert.AreEqual(ComparisonOperator.GreaterOrEqual, converter.Comparison);
        }


        [Test]
        public void ConvertTest()
        {
            DoubleToBooleanConverter converter = new DoubleToBooleanConverter();

            converter.Comparison = ComparisonOperator.Equal;
            object result = converter.Convert(1.00000000000000000000000001, typeof(bool), 1.0, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(true, (bool)result);
            result = converter.Convert(1.000001, typeof(bool), 1.0, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(false, (bool)result);

            converter.Comparison = ComparisonOperator.NotEqual;
            result = converter.Convert(1.00000000000000000000000001, typeof(bool), 1.0, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(false, (bool)result);
            result = converter.Convert(1.000001, typeof(bool), 1.0, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(true, (bool)result);

            converter.Comparison = ComparisonOperator.Greater;
            result = converter.Convert(1.00000000000000000000000001, typeof(bool), 1.0, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(false, (bool)result);
            result = converter.Convert(1.000001, typeof(bool), 1.0, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(true, (bool)result);

            converter.Comparison = ComparisonOperator.GreaterOrEqual;
            result = converter.Convert(1.00000000000000000000000001, typeof(bool), 1.0, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(true, (bool)result);
            result = converter.Convert(1.000001, typeof(bool), 1.0, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(true, (bool)result);

            converter.Comparison = ComparisonOperator.Less;
            result = converter.Convert(1.0, typeof(bool), 1.00000000000000000000000001, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(false, (bool)result);
            result = converter.Convert(1.0, typeof(bool), 1.000001, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(true, (bool)result);

            converter.Comparison = ComparisonOperator.LessOrEqual;
            result = converter.Convert(1.0, typeof(bool), 1.00000000000000000000000001, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(true, (bool)result);
            result = converter.Convert(1.0, typeof(bool), 1.000001, CultureInfo.CurrentCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(true, (bool)result);
        }


        [Test]
        public void ConvertFromStringsTest()
        {
            DoubleToBooleanConverter converter = new DoubleToBooleanConverter();

            converter.Comparison = ComparisonOperator.Equal;
            object result = converter.Convert("1.00000000000000000000000001", typeof(bool), "1.0", CultureInfo.InvariantCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(true, (bool)result);
            result = converter.Convert("1.000001", typeof(bool), "1.0", CultureInfo.InvariantCulture);
            Assert.IsTrue(result is bool);
            Assert.AreEqual(false, (bool)result);
        }


        [Test]
        public void ConvertBackTest()
        {
            DoubleToBooleanConverter converter = new DoubleToBooleanConverter();
            AssertHelper.Throws<NotImplementedException>(() => converter.ConvertBack(false, typeof(double), null, CultureInfo.CurrentCulture));
        }
    }
}
