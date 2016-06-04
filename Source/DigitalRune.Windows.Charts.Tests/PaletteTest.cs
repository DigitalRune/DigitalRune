using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
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



namespace DigitalRune.Windows.Charts.Tests
{
    // Notes:
    // - The Offsets of GradientStops in Silverlight are only computed using single precision. Some
    //   comparisons are therefore converting the values to float to avoid precision issues.

    [TestFixture]
    public class PaletteTest
    {
        [Test]
        public void ConstructorTest()
        {
            Palette palette = new Palette();
            Assert.AreEqual(0, palette.Count);
        }


        [Test]
        public void ConstructorTest2()
        {
            List<PaletteEntry> paletteEntries = new List<PaletteEntry>
            {
                new PaletteEntry(0, Colors.Red),
                new PaletteEntry(2, Colors.Blue),
                new PaletteEntry(1, Colors.Green)
            };

            Palette palette = new Palette(paletteEntries);
            Assert.AreEqual(3, palette.Count);
            Assert.AreEqual(new PaletteEntry(0, Colors.Red), palette[0]);
            Assert.AreEqual(new PaletteEntry(2, Colors.Blue), palette[1]);
            Assert.AreEqual(new PaletteEntry(1, Colors.Green), palette[2]);
        }


        [Test]
        public void AddTest()
        {
            Palette palette = new Palette();
            Assert.AreEqual(0, palette.Count);

            palette.Add(0, Colors.Red);
            Assert.AreEqual(1, palette.Count);
            Assert.AreEqual(new PaletteEntry(0, Colors.Red), palette[0]);

            palette.Add(2, Colors.Blue);
            Assert.AreEqual(2, palette.Count);
            Assert.AreEqual(new PaletteEntry(2, Colors.Blue), palette[1]);

            palette.Add(new PaletteEntry(1, Colors.Green));
            Assert.AreEqual(3, palette.Count);
            Assert.AreEqual(new PaletteEntry(1, Colors.Green), palette[2]);
        }


        [Test]
        public void RemoveTest()
        {
            Palette palette = new Palette
            {
                new PaletteEntry(0, Colors.Red),
                new PaletteEntry(2, Colors.Blue),
                new PaletteEntry(1, Colors.Green)
            };

            Assert.AreEqual(3, palette.Count);
            Assert.AreEqual(new PaletteEntry(0, Colors.Red), palette[0]);
            Assert.AreEqual(new PaletteEntry(2, Colors.Blue), palette[1]);
            Assert.AreEqual(new PaletteEntry(1, Colors.Green), palette[2]);

            bool result = palette.Remove(1);
            Assert.IsTrue(result);
            Assert.AreEqual(2, palette.Count);
            Assert.AreEqual(new PaletteEntry(0, Colors.Red), palette[0]);
            Assert.AreEqual(new PaletteEntry(2, Colors.Blue), palette[1]);

            result = palette.Remove(2);
            Assert.IsTrue(result);
            Assert.AreEqual(1, palette.Count);
            Assert.AreEqual(new PaletteEntry(0, Colors.Red), palette[0]);

            result = palette.Remove(1);
            Assert.IsFalse(result);
            Assert.AreEqual(1, palette.Count);

            result = palette.Remove(0);
            Assert.IsTrue(result);
            Assert.AreEqual(0, palette.Count);
        }


        [Test]
        public void ContainsTest()
        {
            Palette palette = new Palette
            {
                new PaletteEntry(0, Colors.Red),
                new PaletteEntry(2, Colors.Blue),
                new PaletteEntry(1, Colors.Green)
            };

            Assert.IsTrue(palette.Contains(0));
            Assert.IsTrue(palette.Contains(2));
            Assert.IsTrue(palette.Contains(1));
            Assert.IsFalse(palette.Contains(3));
        }


        [Test]
        public void GetColorException()
        {
            Palette palette = new Palette(PaletteMode.Closest);
            AssertHelper.Throws<PaletteException>(() => palette.GetColor(0.5));
        }


        [Test]
        public void GetColorException_Equal()
        {
            Palette palette = new Palette(PaletteMode.Equal)
            {
                new PaletteEntry(0, Colors.Red),
                new PaletteEntry(2, Colors.Blue),
                new PaletteEntry(1, Colors.Green)
            };

            AssertHelper.Throws<PaletteException>(() => palette.GetColor(0.5));
        }


        [Test]
        public void GetColorException_LessOrEqual()
        {
            Palette palette = new Palette(PaletteMode.LessOrEqual)
            {
                new PaletteEntry(0, Colors.Red),
                new PaletteEntry(2, Colors.Blue),
                new PaletteEntry(1, Colors.Green)
            };

            AssertHelper.Throws<PaletteException>(() => palette.GetColor(-1));
        }


        [Test]
        public void GetColorException_GreaterOrEqual()
        {
            Palette palette = new Palette(PaletteMode.GreaterOrEqual)
            {
                new PaletteEntry(0, Colors.Red),
                new PaletteEntry(2, Colors.Blue),
                new PaletteEntry(1, Colors.Green)
            };

            AssertHelper.Throws<PaletteException>(() => palette.GetColor(3));
        }


        [Test]
        public void TryGetColorEqual()
        {
            Palette palette = new Palette(PaletteMode.Equal)
            {
                new PaletteEntry(0, Colors.Red),
                new PaletteEntry(2, Colors.Blue),
                new PaletteEntry(1, Colors.Green)
            };

            Color color;
            bool result = palette.TryGetColor(0, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Red, color);
            result = palette.TryGetColor(1, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Green, color);
            result = palette.TryGetColor(2, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Blue, color);

            result = palette.TryGetColor(0.5, out color);
            Assert.IsFalse(result);
        }


        [Test]
        public void TryGetColorLessOrEqual()
        {
            Palette palette = new Palette(PaletteMode.LessOrEqual)
            {
                new PaletteEntry(0, Colors.Red),
                new PaletteEntry(2, Colors.Blue),
                new PaletteEntry(1, Colors.Green)
            };

            Color color;
            bool result = palette.TryGetColor(0, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Red, color);
            result = palette.TryGetColor(0.5, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Red, color);
            result = palette.TryGetColor(1, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Green, color);
            result = palette.TryGetColor(2, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Blue, color);
            result = palette.TryGetColor(3, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Blue, color);

            result = palette.TryGetColor(-0.5, out color);
            Assert.IsFalse(result);
        }


        [Test]
        public void TryGetColorGreaterOrEqual()
        {
            Palette palette = new Palette(PaletteMode.GreaterOrEqual)
            {
                new PaletteEntry(0, Colors.Red),
                new PaletteEntry(2, Colors.Blue),
                new PaletteEntry(1, Colors.Green)
            };

            Color color;
            bool result = palette.TryGetColor(-1, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Red, color);
            result = palette.TryGetColor(0, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Red, color);
            result = palette.TryGetColor(0.5, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Green, color);
            result = palette.TryGetColor(1, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Green, color);
            result = palette.TryGetColor(2, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Blue, color);

            result = palette.TryGetColor(3, out color);
            Assert.IsFalse(result);
        }


        [Test]
        public void TryGetColorClosest()
        {
            Palette palette = new Palette(PaletteMode.Closest)
            {
                new PaletteEntry(0, Colors.Red),
                new PaletteEntry(2, Colors.Blue),
                new PaletteEntry(1, Colors.Green)
            };

            Color color;
            bool result = palette.TryGetColor(-1, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Red, color);
            result = palette.TryGetColor(0, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Red, color);
            result = palette.TryGetColor(0.5, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Green, color);
            result = palette.TryGetColor(1, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Green, color);
            result = palette.TryGetColor(2, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Blue, color);
            result = palette.TryGetColor(3, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Colors.Blue, color);
        }


        [Test]
        public void TryGetColorInterpolate()
        {
            Color red = Color.FromArgb(255, 255, 0, 0);
            Color transparentGreen = Color.FromArgb(0, 0, 255, 0);
            Color blue = Color.FromArgb(255, 0, 0, 255);
            Palette palette = new Palette(PaletteMode.Interpolate)
            {
                new PaletteEntry(0, red),
                new PaletteEntry(2, blue),
                new PaletteEntry(1, transparentGreen)
            };

            Color color;
            bool result = palette.TryGetColor(-1, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(red, color);
            result = palette.TryGetColor(0, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(red, color);
            result = palette.TryGetColor(0.5, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(Color.FromArgb(127, 127, 127, 0), color);
            result = palette.TryGetColor(1, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(transparentGreen, color);
            result = palette.TryGetColor(2, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(blue, color);
            result = palette.TryGetColor(3, out color);
            Assert.IsTrue(result);
            Assert.AreEqual(blue, color);
        }


        [Test]
        public void GetGradientException()
        {
            Palette palette = new Palette(PaletteMode.Interpolate);
            AssertHelper.Throws<PaletteException>(() => palette.GetGradient(0, 1));
        }


        [Test]
        public void GetGradientException2()
        {
            Palette palette = new Palette(PaletteMode.Equal)
            {
                new PaletteEntry(0, Colors.Red),
                new PaletteEntry(1, Colors.Blue)
            };
            AssertHelper.Throws<NotSupportedException>(() => palette.GetGradient(0, 1));
        }


        [Test]
        public void GetGradientInterpolate()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Interpolate)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(0, 2);
                Assert.AreEqual(3, gradient.Count);
                Assert.AreEqual(0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(0.5, gradient[1].Offset);
                Assert.AreEqual(color1, gradient[1].Color);
                Assert.AreEqual(1, gradient[2].Offset);
                Assert.AreEqual(color2, gradient[2].Color);
            });
        }


        [Test]
        public void GetGradientInterpolate2()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Interpolate)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(-0.5, 2.5);
                Assert.AreEqual(5, gradient.Count);
                Assert.AreEqual(0.0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(0.5f / 3.0f, (float)gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
                Assert.AreEqual(1.5f / 3.0f, (float)gradient[2].Offset);
                Assert.AreEqual(color1, gradient[2].Color);
                Assert.AreEqual(2.5f / 3.0f, (float)gradient[3].Offset);
                Assert.AreEqual(color2, gradient[3].Color);
                Assert.AreEqual(1.0, gradient[4].Offset);
                Assert.AreEqual(color2, gradient[4].Color);
            });
        }


        [Test]
        public void GetGradientInterpolate3()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Interpolate)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(-0.5, 1.5);
                Assert.AreEqual(4, gradient.Count);
                Assert.AreEqual(0.0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(0.5 / 2.0, gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
                Assert.AreEqual(1.5 / 2.0, gradient[2].Offset);
                Assert.AreEqual(color1, gradient[2].Color);
                Assert.AreEqual(1.0, gradient[3].Offset);
                Assert.AreEqual(Color.FromArgb(255, 127, 255, 127), gradient[3].Color);
            });
        }


        [Test]
        public void GetGradientInterpolate4()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Interpolate)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(-0.5, -0.25);
                Assert.AreEqual(2, gradient.Count);
                Assert.AreEqual(0.0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(1.0, gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
            });
        }


        [Test]
        public void GetGradientInterpolate5()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Interpolate)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(0.5, 2.5);
                Assert.AreEqual(4, gradient.Count);
                Assert.AreEqual(0.0, gradient[0].Offset);
                Assert.AreEqual(Color.FromArgb(255, 0, 127, 0), gradient[0].Color);
                Assert.AreEqual(0.5 / 2.0, gradient[1].Offset);
                Assert.AreEqual(color1, gradient[1].Color);
                Assert.AreEqual(1.5 / 2.0, gradient[2].Offset);
                Assert.AreEqual(color2, gradient[2].Color);
                Assert.AreEqual(1.0, gradient[3].Offset);
                Assert.AreEqual(color2, gradient[3].Color);
            });
        }


        [Test]
        public void GetGradientInterpolate6()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Interpolate)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(2.25, 2.5);
                Assert.AreEqual(2, gradient.Count);
                Assert.AreEqual(0.0, gradient[0].Offset);
                Assert.AreEqual(color2, gradient[0].Color);
                Assert.AreEqual(1.0, gradient[1].Offset);
                Assert.AreEqual(color2, gradient[1].Color);
            });
        }


        [Test]
        public void GetGradientClosest()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Closest)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(0, 2);
                Assert.AreEqual(6, gradient.Count);
                Assert.AreEqual(0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(0.25, gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
                Assert.AreEqual(0.25, gradient[2].Offset);
                Assert.AreEqual(color1, gradient[2].Color);
                Assert.AreEqual(0.75, gradient[3].Offset);
                Assert.AreEqual(color1, gradient[3].Color);
                Assert.AreEqual(0.75, gradient[4].Offset);
                Assert.AreEqual(color2, gradient[4].Color);
                Assert.AreEqual(1, gradient[5].Offset);
                Assert.AreEqual(color2, gradient[5].Color);
            });
        }


        [Test]
        public void GetGradientClosest2()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Closest)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(-1, 3);
                Assert.AreEqual(6, gradient.Count);
                Assert.AreEqual(0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(1.5 / 4.0, gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
                Assert.AreEqual(1.5 / 4.0, gradient[2].Offset);
                Assert.AreEqual(color1, gradient[2].Color);
                Assert.AreEqual(2.5 / 4.0, gradient[3].Offset);
                Assert.AreEqual(color1, gradient[3].Color);
                Assert.AreEqual(2.5 / 4.0, gradient[4].Offset);
                Assert.AreEqual(color2, gradient[4].Color);
                Assert.AreEqual(1, gradient[5].Offset);
                Assert.AreEqual(color2, gradient[5].Color);
            });
        }


        [Test]
        public void GetGradientClosest3()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Closest)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(-1, -0.5);
                Assert.AreEqual(2, gradient.Count);
                Assert.AreEqual(0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(1, gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
            });
        }


        [Test]
        public void GetGradientClosest4()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Closest)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(-1, -0.75);
                Assert.AreEqual(2, gradient.Count);
                Assert.AreEqual(0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(1, gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
            });
        }


        [Test]
        public void GetGradientClosest5()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Closest)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(2.5, 3);
                Assert.AreEqual(2, gradient.Count);
                Assert.AreEqual(0, gradient[0].Offset);
                Assert.AreEqual(color2, gradient[0].Color);
                Assert.AreEqual(1, gradient[1].Offset);
                Assert.AreEqual(color2, gradient[1].Color);
            });
        }


        [Test]
        public void GetGradientClosest6()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Closest)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(2.75, 3);
                Assert.AreEqual(2, gradient.Count);
                Assert.AreEqual(0, gradient[0].Offset);
                Assert.AreEqual(color2, gradient[0].Color);
                Assert.AreEqual(1, gradient[1].Offset);
                Assert.AreEqual(color2, gradient[1].Color);
            });
        }


        [Test]
        public void GetGradientClosest7()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Closest)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(-1, 0.5);
                Assert.AreEqual(3, gradient.Count);
                Assert.AreEqual(0.0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(1.0, gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
                Assert.AreEqual(1.0, gradient[2].Offset);
                Assert.AreEqual(color1, gradient[2].Color);
            });
        }


        [Test]
        public void GetGradientClosest8()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Closest)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(-1, 0.75);
                Assert.AreEqual(4, gradient.Count);
                Assert.AreEqual(0.0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(1.5f / 1.75f, (float)gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
                Assert.AreEqual(1.5f / 1.75f, (float)gradient[2].Offset);
                Assert.AreEqual(color1, gradient[2].Color);
                Assert.AreEqual(1.0, gradient[3].Offset);
                Assert.AreEqual(color1, gradient[3].Color);
            });
        }


        [Test]
        public void GetGradientClosest9()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Closest)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(-1, 1.75);
                Assert.AreEqual(6, gradient.Count);
                Assert.AreEqual(0.0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(1.5f / 2.75f, (float)gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
                Assert.AreEqual(1.5f / 2.75f, (float)gradient[2].Offset);
                Assert.AreEqual(color1, gradient[2].Color);
                Assert.AreEqual(2.5f / 2.75f, (float)gradient[3].Offset);
                Assert.AreEqual(color1, gradient[3].Color);
                Assert.AreEqual(2.5f / 2.75f, (float)gradient[4].Offset);
                Assert.AreEqual(color2, gradient[4].Color);
                Assert.AreEqual(1.0, gradient[5].Offset);
                Assert.AreEqual(color2, gradient[5].Color);
            });
        }


        [Test]
        public void GetGradientGreaterOrEqual()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.GreaterOrEqual)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(0, 2);
                Assert.AreEqual(5, gradient.Count);
                Assert.AreEqual(0.0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(0.0, gradient[1].Offset);
                Assert.AreEqual(color1, gradient[1].Color);
                Assert.AreEqual(0.5, gradient[2].Offset);
                Assert.AreEqual(color1, gradient[2].Color);
                Assert.AreEqual(0.5, gradient[3].Offset);
                Assert.AreEqual(color2, gradient[3].Color);
                Assert.AreEqual(1.0, gradient[4].Offset);
                Assert.AreEqual(color2, gradient[4].Color);
            });
        }


        [Test]
        public void GetGradientGreaterOrEqual2()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.GreaterOrEqual)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(0.5, 1.5);
                Assert.AreEqual(4, gradient.Count);
                Assert.AreEqual(0.0, gradient[0].Offset);
                Assert.AreEqual(color1, gradient[0].Color);
                Assert.AreEqual(0.5, gradient[1].Offset);
                Assert.AreEqual(color1, gradient[1].Color);
                Assert.AreEqual(0.5, gradient[2].Offset);
                Assert.AreEqual(color2, gradient[2].Color);
                Assert.AreEqual(1.0, gradient[3].Offset);
                Assert.AreEqual(color2, gradient[3].Color);
            });
        }


        [Test]
        public void GetGradientGreaterOrEqual3()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.GreaterOrEqual)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(1.25, 1.5);
                Assert.AreEqual(2, gradient.Count);
                Assert.AreEqual(0.0, gradient[0].Offset);
                Assert.AreEqual(color2, gradient[0].Color);
                Assert.AreEqual(1.0, gradient[1].Offset);
                Assert.AreEqual(color2, gradient[1].Color);
            });
        }


        [Test]
        public void GetGradientLessOrEqual()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.LessOrEqual)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(0, 2);
                Assert.AreEqual(5, gradient.Count);
                Assert.AreEqual(0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(0.5, gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
                Assert.AreEqual(0.5, gradient[2].Offset);
                Assert.AreEqual(color1, gradient[2].Color);
                Assert.AreEqual(1, gradient[3].Offset);
                Assert.AreEqual(color1, gradient[3].Color);
                Assert.AreEqual(1, gradient[4].Offset);
                Assert.AreEqual(color2, gradient[4].Color);
            });
        }


        [Test]
        public void GetGradientLessOrEqual2()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.LessOrEqual)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(0.5, 1.5);
                Assert.AreEqual(4, gradient.Count);
                Assert.AreEqual(0, gradient[0].Offset);
                Assert.AreEqual(color0, gradient[0].Color);
                Assert.AreEqual(0.5, gradient[1].Offset);
                Assert.AreEqual(color0, gradient[1].Color);
                Assert.AreEqual(0.5, gradient[2].Offset);
                Assert.AreEqual(color1, gradient[2].Color);
                Assert.AreEqual(1, gradient[3].Offset);
                Assert.AreEqual(color1, gradient[3].Color);
            });
        }


        [Test]
        public void GetSingularGradient()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Interpolate)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(1, 1);
                Assert.AreEqual(2, gradient.Count);
                Assert.AreEqual(0, gradient[0].Offset);
                Assert.AreEqual(color1, gradient[0].Color);
                Assert.AreEqual(1, gradient[1].Offset);
                Assert.AreEqual(color1, gradient[1].Color);
            });
        }


        [Test]
        public void GetGradientNegativeInterpolated()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                Color color0 = Color.FromArgb(255, 0, 0, 0);
                Color color1 = Color.FromArgb(255, 0, 255, 0);
                Color color2 = Color.FromArgb(255, 255, 255, 255);
                Palette palette = new Palette(PaletteMode.Interpolate)
                {
                    new PaletteEntry(0, color0),
                    new PaletteEntry(2, color2),
                    new PaletteEntry(1, color1)
                };

                GradientStopCollection gradient = palette.GetGradient(2, 0);
                Assert.AreEqual(3, gradient.Count);
                Assert.AreEqual(0, gradient[0].Offset);
                Assert.AreEqual(color2, gradient[0].Color);
                Assert.AreEqual(0.5, gradient[1].Offset);
                Assert.AreEqual(color1, gradient[1].Color);
                Assert.AreEqual(1, gradient[2].Offset);
                Assert.AreEqual(color0, gradient[2].Color);
            });
        }


        [Test]
        public void DefaultColorInterpolationModeShouldBeSRgb()
        {
            Palette palette = new Palette();
            Assert.AreEqual(ColorInterpolationMode.SRgbLinearInterpolation, palette.ColorInterpolationMode);
        }


        [Test]
        public void SRgbInterpolationMode()
        {
            Palette palette = new Palette(PaletteMode.Interpolate)
            {
                new PaletteEntry(0, Color.FromArgb(0, 20, 40, 60)),
                new PaletteEntry(1, Color.FromArgb(255, 255, 255, 255)),
            };
            palette.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
            Assert.AreEqual(Color.FromArgb(127, 137, 147, 157), palette.GetColor(0.5));
        }


#if !SILVERLIGHT && !WINDOWS_PHONE
        [Test]
        public void ScRgbInterpolationMode()
        {
            Palette palette = new Palette(PaletteMode.Interpolate)
            {
                new PaletteEntry(0, Color.FromScRgb(0.0f, 0.1f, 0.2f, 0.3f)),
                new PaletteEntry(1, Color.FromScRgb(1.0f, 1.0f, 1.0f, 1.0f)),
            };
            palette.ColorInterpolationMode = ColorInterpolationMode.ScRgbLinearInterpolation;
            Assert.AreEqual(Color.FromScRgb(0.5f, 0.55f, 0.6f, 0.65f), palette.GetColor(0.5));
        }
#endif
    }
}
