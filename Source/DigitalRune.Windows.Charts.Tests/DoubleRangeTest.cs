using System;
using System.Globalization;
using System.Threading;
#if NETFX_CORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestFixtureSetUp = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.ClassInitializeAttribute;
using TestFixtureTearDown = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.ClassCleanupAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestFixtureSetUp = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using TestFixtureTearDown = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#else
using NUnit.Framework;
#endif


namespace DigitalRune.Windows.Charts.Tests
{
    [TestFixture]
    public class DoubleRangeTest
    {
        [Test]
        public void ConstructorTest()
        {
            var range = new DoubleRange(-1.2, +3.4);

            Assert.AreEqual(-1.2, range.Min);
            Assert.AreEqual(+3.4, range.Max);
        }


        [Test]
        public void EqualityTest()
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            var range0 = new DoubleRange(-1.2, +3.4);
            var range1 = new DoubleRange(-1.2, +3.4);
            var range2 = new DoubleRange(0, +3.4);
            var range3 = new DoubleRange(-1.2, +3.5);
            var range4 = new DoubleRange(double.NaN, +3.4);
            var range5 = new DoubleRange(double.NaN, double.NaN);

            // Equals(DoubleRange)
            Assert.IsTrue(range0.Equals(range0));
            Assert.IsTrue(range0.Equals(range1));
            Assert.IsFalse(range0.Equals(range2));
            Assert.IsFalse(range0.Equals(range3));
            Assert.IsFalse(range0.Equals(range4));
            Assert.IsTrue(range5.Equals(range5));

            // Equals(object)
            Assert.IsTrue(range0.Equals((object)range0));
            Assert.IsTrue(range0.Equals((object)range1));
            Assert.IsFalse(range0.Equals((object)range2));
            Assert.IsFalse(range0.Equals((object)range3));
            Assert.IsFalse(range0.Equals((object)range4));
            Assert.IsTrue(range5.Equals((object)range5));
            Assert.IsFalse(range0.Equals(null));
            Assert.IsFalse(range0.Equals(range0.ToString()));

            // == operator
            Assert.IsTrue(range0 == range1);
            Assert.IsFalse(range0 == range2);
            Assert.IsFalse(range0 == range3);
            Assert.IsFalse(range0 == range4);

            // != operator
            Assert.IsFalse(range0 != range1);
            Assert.IsTrue(range0 != range2);
            Assert.IsTrue(range0 != range3);
            Assert.IsTrue(range0 != range4);
            // ReSharper restore SuspiciousTypeConversion.Global
        }


        [Test]
        public void GetHashCodeTest()
        {
            var range0 = new DoubleRange(-1.2, +3.4);
            var range1 = new DoubleRange(-1.2, +3.4);
            var range2 = new DoubleRange(-1.2, +3.5);

            Assert.AreNotEqual(0, range0.GetHashCode());
            Assert.AreEqual(range0.GetHashCode(), range1.GetHashCode());
            Assert.AreNotEqual(range0.GetHashCode(), range2.GetHashCode());
        }


        [Test]
        public void ToStringTest()
        {
            var range = new DoubleRange(-1.2, +3.4);

            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-AT");

            Assert.AreEqual("-1,2; 3,4", range.ToString());
            Assert.AreEqual("-1.2, 3.4", range.ToString(CultureInfo.InvariantCulture));

            Thread.CurrentThread.CurrentCulture = currentCulture;
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParseShouldThrowIfNull()
        {
            DoubleRange.Parse(null);
        }


        [Test]
        [ExpectedException(typeof(FormatException))]
        public void ParseShouldThrowIfInvalid()
        {
            DoubleRange.Parse("x");
        }


        [Test]
        public void ParseTest()
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-AT");

            Assert.AreEqual(new DoubleRange(-1.2, +3.4), DoubleRange.Parse("-1,2; 3,4"));
            Assert.AreEqual(new DoubleRange(-1.2, +3.4), DoubleRange.Parse("-1.2, 3.4", CultureInfo.InvariantCulture));

            Thread.CurrentThread.CurrentCulture = currentCulture;
        }


        [Test]
        public void ClampTest()
        {
            var range = new DoubleRange(-1.2, +3.4);

            Assert.AreEqual(-1.2, range.Clamp(double.NegativeInfinity));
            Assert.AreEqual(-1.2, range.Clamp(-10));
            Assert.AreEqual(-1.2, range.Clamp(-1.2));
            Assert.AreEqual(0.0, range.Clamp(0.0));
            Assert.AreEqual(+3.4, range.Clamp(+3.4));
            Assert.AreEqual(+3.4, range.Clamp(+10));
            Assert.AreEqual(+3.4, range.Clamp(double.PositiveInfinity));
            Assert.IsTrue(double.IsNaN(range.Clamp(double.NaN)));
        }


        [Test]
        public void ContainsTest()
        {
            var range = new DoubleRange(-1.2, +3.4);

            Assert.IsFalse(range.Contains(double.NegativeInfinity));
            Assert.IsFalse(range.Contains(-10));
            Assert.IsTrue(range.Contains(-1.2));
            Assert.IsTrue(range.Contains(0.0));
            Assert.IsTrue(range.Contains(+3.4));
            Assert.IsFalse(range.Contains(+10));
            Assert.IsFalse(range.Contains(Double.PositiveInfinity));
            Assert.IsFalse(range.Contains(double.NaN));
        }
    }
}
