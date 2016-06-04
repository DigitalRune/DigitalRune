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
    public class DateTimeRangeTest
    {
        [Test]
        public void ConstructorTest()
        {
            var time0 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time1 = new DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var range = new DateTimeRange(time0, time1);

            Assert.AreEqual(time0, range.Min);
            Assert.AreEqual(time1, range.Max);
        }


        [Test]
        public void EqualityTest()
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            var time0 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time1 = new DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var range0 = new DateTimeRange(time0, time1);
            var range1 = new DateTimeRange(time0, time1);
            var range2 = new DateTimeRange(new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc), time1);
            var range3 = new DateTimeRange(time0, new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            // Equals(DateTimeRange)
            Assert.IsTrue(range0.Equals(range0));
            Assert.IsTrue(range0.Equals(range1));
            Assert.IsFalse(range0.Equals(range2));
            Assert.IsFalse(range0.Equals(range3));

            // Equals(object)
            Assert.IsTrue(range0.Equals((object)range0));
            Assert.IsTrue(range0.Equals((object)range1));
            Assert.IsFalse(range0.Equals((object)range2));
            Assert.IsFalse(range0.Equals((object)range3));
            Assert.IsFalse(range0.Equals(null));
            Assert.IsFalse(range0.Equals(range0.ToString()));

            // == operator
            Assert.IsTrue(range0 == range1);
            Assert.IsFalse(range0 == range2);
            Assert.IsFalse(range0 == range3);

            // != operator
            Assert.IsFalse(range0 != range1);
            Assert.IsTrue(range0 != range2);
            Assert.IsTrue(range0 != range3);
            // ReSharper restore SuspiciousTypeConversion.Global
        }


        [Test]
        public void GetHashCodeTest()
        {
            var time0 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time1 = new DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var range0 = new DateTimeRange(time0, time1);
            var range1 = new DateTimeRange(time0, time1);
            var range2 = new DateTimeRange(time0, new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            Assert.AreNotEqual(0.0, range0.GetHashCode());
            Assert.AreEqual(range0.GetHashCode(), range1.GetHashCode());
            Assert.AreNotEqual(range0.GetHashCode(), range2.GetHashCode());
        }


        [Test]
        public void ToStringTest()
        {
            var time0 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time1 = new DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var range = new DateTimeRange(time0, time1);

            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-AT");

            Assert.AreEqual("01.01.2000 00:00:00; 31.12.2015 00:00:00", range.ToString());
            Assert.AreEqual("01.01.2000 00:00:00; 31.12.2015 00:00:00", range.ToString(new CultureInfo("de-AT")));
            Assert.AreEqual("01/01/2000 00:00:00, 12/31/2015 00:00:00", range.ToString(CultureInfo.InvariantCulture));

            Thread.CurrentThread.CurrentCulture = currentCulture;
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParseShouldThrowIfNull()
        {
            DateTimeRange.Parse(null);
        }


        [Test]
        [ExpectedException(typeof(FormatException))]
        public void ParseShouldThrowIfInvalid()
        {
            DateTimeRange.Parse("x");
        }


        [Test]
        public void ParseTest()
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-AT");

            var time0 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time1 = new DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var range = new DateTimeRange(time0, time1);

            Assert.AreEqual(range, DateTimeRange.Parse("01.01.2000 00:00:00; 31.12.2015 00:00:00"));
            Assert.AreEqual(range, DateTimeRange.Parse("01.01.2000 00:00:00; 31.12.2015 00:00:00", new CultureInfo("de-AT")));
            Assert.AreEqual(range, DateTimeRange.Parse("01/01/2000 00:00:00, 12/31/2015 00:00:00", CultureInfo.InvariantCulture));

            Thread.CurrentThread.CurrentCulture = currentCulture;
        }


        [Test]
        public void ClampTest()
        {
            var time0 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time1 = new DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var range = new DateTimeRange(time0, time1);

            Assert.AreEqual(time0, range.Clamp(DateTime.MinValue));
            Assert.AreEqual(time0, range.Clamp(new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(time0, range.Clamp(time0));
            Assert.AreEqual(new DateTime(2000, 6, 30, 0, 0, 0, DateTimeKind.Utc), range.Clamp(new DateTime(2000, 6, 30, 0, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(time1, range.Clamp(time1));
            Assert.AreEqual(time1, range.Clamp(new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(time1, range.Clamp(DateTime.MaxValue));
        }


        [Test]
        public void ContainsTest()
        {
            var time0 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time1 = new DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var range = new DateTimeRange(time0, time1);

            Assert.IsFalse(range.Contains(DateTime.MinValue));
            Assert.IsFalse(range.Contains(new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc)));
            Assert.IsTrue(range.Contains(time0));
            Assert.IsTrue(range.Contains(new DateTime(2000, 6, 30, 0, 0, 0, DateTimeKind.Utc)));
            Assert.IsTrue(range.Contains(time1));
            Assert.IsFalse(range.Contains(new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
            Assert.IsFalse(range.Contains(DateTime.MaxValue));
        }
    }
}
