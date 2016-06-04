using System.Windows.Media;
using DigitalRune.Mathematics;
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
    public class ColorHelperTest
    {
        [Test]
        public void HsvConversions()  // Old unit test.
        {
            Color c = ColorHelper.FromHsv(12, 99, 0);
            Assert.AreEqual(0, c.R);
            Assert.AreEqual(0, c.G);
            Assert.AreEqual(0, c.B);
            Assert.AreEqual(255, c.A);

            c = ColorHelper.FromHsv(12, 0, 100);
            Assert.AreEqual(255, c.R);
            Assert.AreEqual(255, c.G);
            Assert.AreEqual(255, c.B);

            c = ColorHelper.FromHsv(0, 100, 100);
            Assert.AreEqual(255, c.R);
            Assert.AreEqual(0, c.G);
            Assert.AreEqual(0, c.B);

            c = ColorHelper.FromHsv(60, 100, 100);
            Assert.AreEqual(255, c.R);
            Assert.AreEqual(255, c.G);
            Assert.AreEqual(0, c.B);

            c = ColorHelper.FromHsv(120, 100, 100);
            Assert.AreEqual(0, c.R);
            Assert.AreEqual(255, c.G);
            Assert.AreEqual(0, c.B);

            c = ColorHelper.FromHsv(120, 100, 50);
            Assert.AreEqual(0, c.R);
            Assert.AreEqual(128, c.G);
            Assert.AreEqual(0, c.B);
            Assert.AreEqual(120, (int)c.GetH());
            Assert.AreEqual(100, (int)c.GetS());
            Assert.AreEqual(50, (int)c.GetV());

            c = ColorHelper.FromHsv(180, 100, 100);
            Assert.AreEqual(0, c.R);
            Assert.AreEqual(255, c.G);
            Assert.AreEqual(255, c.B);

            c = ColorHelper.FromHsv(240, 100, 100);
            Assert.AreEqual(0, c.R);
            Assert.AreEqual(0, c.G);
            Assert.AreEqual(255, c.B);

            c = ColorHelper.FromHsv(300, 100, 100);
            Assert.AreEqual(255, c.R);
            Assert.AreEqual(0, c.G);
            Assert.AreEqual(255, c.B);

            c = ColorHelper.FromHsv(360, 100, 100);
            Assert.AreEqual(255, c.R);
            Assert.AreEqual(0, c.G);
            Assert.AreEqual(0, c.B);

            ColorHelper.SetH(ref c, 60);
            Assert.AreEqual(255, c.R);
            Assert.AreEqual(255, c.G);
            Assert.AreEqual(0, c.B);
        }


        [Test]
        public void ColorConversions() // New unit test.
        {
            // Test cases from Wikipedia (see http://en.wikipedia.org/wiki/HSL_and_HSV)
            // The decimal values are converted to byte (Color struct).
            // --> Checks need to use tolerance.

            //                         R              G              B          H     S     V       H     S     L
            TestColorConversion(ToByte(1.000), ToByte(1.000), ToByte(1.000), 0.0, 0.000, 1.000, 0.0, 0.000, 1.000);
            TestColorConversion(ToByte(0.500), ToByte(0.500), ToByte(0.500), 0.0, 0.000, 0.500, 0.0, 0.000, 0.500);
            TestColorConversion(ToByte(0.000), ToByte(0.000), ToByte(0.000), 0.0, 0.000, 0.000, 0.0, 0.000, 0.000);
            TestColorConversion(ToByte(0.750), ToByte(0.750), ToByte(0.000), 60.0, 1.000, 0.750, 60.0, 1.000, 0.375);
            TestColorConversion(ToByte(0.000), ToByte(0.500), ToByte(0.000), 120.0, 1.000, 0.500, 120.0, 1.000, 0.250);
            TestColorConversion(ToByte(0.500), ToByte(1.000), ToByte(1.000), 180.0, 0.500, 1.000, 180.0, 1.000, 0.750);
            TestColorConversion(ToByte(0.500), ToByte(0.500), ToByte(1.000), 240.0, 0.500, 1.000, 240.0, 1.000, 0.750);
            TestColorConversion(ToByte(0.750), ToByte(0.250), ToByte(0.750), 300.0, 0.667, 0.750, 300.0, 0.500, 0.500);
            TestColorConversion(ToByte(0.628), ToByte(0.643), ToByte(0.142), 61.8, 0.779, 0.643, 61.8, 0.638, 0.393);
            TestColorConversion(ToByte(0.255), ToByte(0.104), ToByte(0.918), 251.1, 0.887, 0.918, 251.1, 0.832, 0.511);
            TestColorConversion(ToByte(0.116), ToByte(0.675), ToByte(0.255), 134.9, 0.828, 0.675, 134.9, 0.707, 0.396);
            TestColorConversion(ToByte(0.941), ToByte(0.785), ToByte(0.053), 49.5, 0.944, 0.941, 49.5, 0.893, 0.497);
            TestColorConversion(ToByte(0.704), ToByte(0.187), ToByte(0.897), 283.7, 0.792, 0.897, 283.7, 0.775, 0.542);
            TestColorConversion(ToByte(0.931), ToByte(0.463), ToByte(0.316), 14.3, 0.661, 0.931, 14.3, 0.817, 0.624);
            TestColorConversion(ToByte(0.998), ToByte(0.974), ToByte(0.532), 56.9, 0.467, 0.998, 56.9, 0.991, 0.765);
            TestColorConversion(ToByte(0.099), ToByte(0.795), ToByte(0.591), 162.4, 0.875, 0.795, 162.4, 0.779, 0.447);
            TestColorConversion(ToByte(0.211), ToByte(0.149), ToByte(0.597), 248.3, 0.750, 0.597, 248.3, 0.601, 0.373);
            TestColorConversion(ToByte(0.495), ToByte(0.493), ToByte(0.721), 240.5, 0.316, 0.721, 240.5, 0.290, 0.607);
        }


        private static byte ToByte(double d)
        {
            return (byte)(d * 255.0 + 0.5);
        }


        private void TestColorConversion(byte r, byte g, byte b, double hHsv, double sHsv, double vHsv, double hHsl, double sHsl, double lHsl)
        {
            sHsv *= 100;
            vHsv *= 100;
            sHsl *= 100;
            lHsl *= 100;

            double tolerance = 1.1; // 1.1 / 255 = ~4% tolerance
            Color c = ColorHelper.FromHsv(hHsv, sHsv, vHsv);
            Assert.IsTrue(Numeric.AreEqual(r, c.R, tolerance));
            Assert.IsTrue(Numeric.AreEqual(g, c.G, tolerance));
            Assert.IsTrue(Numeric.AreEqual(b, c.B, tolerance));
            Assert.AreEqual(255, c.A);

            c = ColorHelper.FromHsl(hHsl, sHsl, lHsl);
            Assert.IsTrue(Numeric.AreEqual(r, c.R, tolerance));
            Assert.IsTrue(Numeric.AreEqual(g, c.G, tolerance));
            Assert.IsTrue(Numeric.AreEqual(b, c.B, tolerance));
            Assert.AreEqual(255, c.A);

            tolerance = 0.8; // 8% tolerance
            double h, s, v, l;
            Color.FromArgb(255, r, g, b).ToHsv(out h, out s, out v);
            Assert.IsTrue(Numeric.AreEqual(hHsv, h, tolerance));
            Assert.IsTrue(Numeric.AreEqual(sHsv, s, tolerance));
            Assert.IsTrue(Numeric.AreEqual(vHsv, v, tolerance));

            Color.FromArgb(255, r, g, b).ToHsl(out h, out s, out l);
            Assert.IsTrue(Numeric.AreEqual(hHsl, h, tolerance));
            Assert.IsTrue(Numeric.AreEqual(sHsl, s, tolerance));
            Assert.IsTrue(Numeric.AreEqual(lHsl, l, tolerance));
        }


        [Test]
        public void ToSRgb()
        {
            Assert.AreEqual(Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0).ToSRgb());
            Assert.AreEqual(Color.FromArgb(1, 13, 13, 13), Color.FromArgb(1, 1, 1, 1).ToSRgb());
            Assert.AreEqual(Color.FromArgb(128, 128, 128, 128), Color.FromArgb(128, 55, 55, 55).ToSRgb());
            Assert.AreEqual(Color.FromArgb(128, 188, 188, 188), Color.FromArgb(128, 128, 128, 128).ToSRgb());
            Assert.AreEqual(Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 255, 255, 255).ToSRgb());
        }


        [Test]
        public void ToLinear()
        {
            Assert.AreEqual(Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0).ToLinear());
            Assert.AreEqual(Color.FromArgb(1, 0, 0, 0), Color.FromArgb(1, 1, 1, 1).ToLinear());
            Assert.AreEqual(Color.FromArgb(1, 1, 1, 1), Color.FromArgb(1, 10, 10, 10).ToLinear());
            Assert.AreEqual(Color.FromArgb(128, 55, 55, 55), Color.FromArgb(128, 128, 128, 128).ToLinear());
            Assert.AreEqual(Color.FromArgb(128, 128, 128, 128), Color.FromArgb(128, 188, 188, 188).ToLinear());
            Assert.AreEqual(Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 255, 255, 255).ToLinear());
        }
    }
}
