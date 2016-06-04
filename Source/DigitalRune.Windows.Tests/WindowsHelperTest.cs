#if NETFX_CORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestFixtureSetUp = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.ClassInitializeAttribute;
using TestFixtureTearDown = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.ClassCleanupAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#else
using System.Windows;
using NUnit.Framework;
#endif


namespace DigitalRune.Windows.Tests
{
    [TestFixture]
    public class WindowsHelperTest
    {
#if !NETFX_CORE && !WINDOWS_PHONE && !SILVERLIGHT
        [Test]
        public void RoundToDevicePixelsTest()
        {
            // 1 : 1 (device pixels : device-independent pixels)
            Assert.AreEqual(-1.0, WindowsHelper.RoundToDevicePixels(-0.5, 1.0));
            Assert.AreEqual(0.0, WindowsHelper.RoundToDevicePixels(-0.4, 1.0));
            Assert.AreEqual(0.0, WindowsHelper.RoundToDevicePixels(0.0, 1.0));
            Assert.AreEqual(0.0, WindowsHelper.RoundToDevicePixels(0.4, 1.0));
            Assert.AreEqual(1.0, WindowsHelper.RoundToDevicePixels(0.5, 1.0));
            Assert.AreEqual(1.0, WindowsHelper.RoundToDevicePixels(1.0, 1.0));
            Assert.AreEqual(1.0, WindowsHelper.RoundToDevicePixels(1.4, 1.0));
            Assert.AreEqual(2.0, WindowsHelper.RoundToDevicePixels(1.5, 1.0));

            // 2 : 1 (device pixels : device-independent pixels)
            Assert.AreEqual(-0.5, WindowsHelper.RoundToDevicePixels(-0.5, 0.5));
            Assert.AreEqual(-0.5, WindowsHelper.RoundToDevicePixels(-0.25, 0.5));
            Assert.AreEqual(0.0, WindowsHelper.RoundToDevicePixels(-0.24, 0.5));
            Assert.AreEqual(0.0, WindowsHelper.RoundToDevicePixels(0.0, 0.5));
            Assert.AreEqual(0.0, WindowsHelper.RoundToDevicePixels(0.24, 0.5));
            Assert.AreEqual(0.5, WindowsHelper.RoundToDevicePixels(0.25, 0.5));
            Assert.AreEqual(0.5, WindowsHelper.RoundToDevicePixels(0.5, 0.5));
            Assert.AreEqual(0.5, WindowsHelper.RoundToDevicePixels(0.74, 0.5));
            Assert.AreEqual(1.0, WindowsHelper.RoundToDevicePixels(0.75, 0.5));

            // Points
            Assert.AreEqual(new Point(5.0, 7.5), WindowsHelper.RoundToDevicePixels(new Point(5.4, 7.7), new Size(1.0, 0.5)));

            // Rectangles
            Assert.AreEqual(new Rect(5.0, 7.5, 10, 10.5), WindowsHelper.RoundToDevicePixels(new Rect(5.4, 7.7, 9.7, 10.6), new Size(1.0, 0.5)));
        }


        [Test]
        public void RoundToDevicePixelsCenterTest()
        {
            // 2 : 1 (device pixels : device-independent pixels)
            Assert.AreEqual(-0.75, WindowsHelper.RoundToDevicePixelsCenter(-0.5, 0.5));
            Assert.AreEqual(-0.25, WindowsHelper.RoundToDevicePixelsCenter(-0.4, 0.5));
            Assert.AreEqual(0.25, WindowsHelper.RoundToDevicePixelsCenter(0.0, 0.5));
            Assert.AreEqual(0.25, WindowsHelper.RoundToDevicePixelsCenter(0.4, 0.5));
            Assert.AreEqual(0.75, WindowsHelper.RoundToDevicePixelsCenter(0.5, 0.5));
            Assert.AreEqual(0.75, WindowsHelper.RoundToDevicePixelsCenter(0.9, 0.5));
            Assert.AreEqual(1.25, WindowsHelper.RoundToDevicePixelsCenter(1.0, 0.5));
            Assert.AreEqual(1.75, WindowsHelper.RoundToDevicePixelsCenter(1.5, 0.5));

            // Points
            Assert.AreEqual(new Point(5.5, 7.75), WindowsHelper.RoundToDevicePixelsCenter(new Point(5.2, 7.7), new Size(1.0, 0.5)));

            // Rectangles
            Assert.AreEqual(new Rect(5.5, 7.75, 10, 10.5), WindowsHelper.RoundToDevicePixelsCenter(new Rect(5.2, 7.7, 9.7, 10.6), new Size(1.0, 0.5)));
        }


        [Test]
        public void RoundToDevicePixelsEvenTest()
        {
            // 2 : 1 (device pixels : device-independent pixels)
            Assert.AreEqual(-1.0, WindowsHelper.RoundToDevicePixelsEven(-0.9, 0.5));
            Assert.AreEqual(-1.0, WindowsHelper.RoundToDevicePixelsEven(-0.5, 0.5));
            Assert.AreEqual(0.0, WindowsHelper.RoundToDevicePixelsEven(-0.4, 0.5));
            Assert.AreEqual(0.0, WindowsHelper.RoundToDevicePixelsEven(0.0, 0.5));
            Assert.AreEqual(0.0, WindowsHelper.RoundToDevicePixelsEven(0.4, 0.5));
            Assert.AreEqual(1.0, WindowsHelper.RoundToDevicePixelsEven(0.5, 0.5));
            Assert.AreEqual(1.0, WindowsHelper.RoundToDevicePixelsEven(1.4, 0.5));
            Assert.AreEqual(2.0, WindowsHelper.RoundToDevicePixelsEven(1.5, 0.5));
        }


        [Test]
        public void RoundToDevicePixelsOddTest()
        {
            // 2 : 1 (device pixels : device-independent pixels)
            Assert.AreEqual(-0.5, WindowsHelper.RoundToDevicePixelsOdd(-0.9, 0.5));
            Assert.AreEqual(-0.5, WindowsHelper.RoundToDevicePixelsOdd(-0.1, 0.5));
            Assert.AreEqual(0.5, WindowsHelper.RoundToDevicePixelsOdd(0.0, 0.5));
            Assert.AreEqual(0.5, WindowsHelper.RoundToDevicePixelsOdd(0.4, 0.5));
            Assert.AreEqual(0.5, WindowsHelper.RoundToDevicePixelsOdd(0.9, 0.5));
            Assert.AreEqual(1.5, WindowsHelper.RoundToDevicePixelsOdd(1.0, 0.5));
            Assert.AreEqual(1.5, WindowsHelper.RoundToDevicePixelsOdd(1.9, 0.5));
            Assert.AreEqual(2.5, WindowsHelper.RoundToDevicePixelsOdd(2.0, 0.5));
        }
#endif
    }
}
