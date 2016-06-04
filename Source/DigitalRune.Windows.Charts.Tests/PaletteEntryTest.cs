using System.Windows.Media;
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
    [TestFixture]
    public class PaletteEntryTest
    {
        [Test]
        public void ConstructorTest()
        {
            PaletteEntry paletteEntry = new PaletteEntry();
            Assert.AreEqual(0, paletteEntry.Value);
            Assert.AreEqual(new Color(), paletteEntry.Color);
        }


        [Test]
        public void ConstructorTest2()
        {
            PaletteEntry paletteEntry = new PaletteEntry(0, new Color());
            Assert.AreEqual(0, paletteEntry.Value);
            Assert.AreEqual(new Color(), paletteEntry.Color);

            paletteEntry = new PaletteEntry(1, Colors.Red);
            Assert.AreEqual(1, paletteEntry.Value);
            Assert.AreEqual(Colors.Red, paletteEntry.Color);
        }


        [Test]
        public void ValueTest()
        {
            PaletteEntry paletteEntry = new PaletteEntry(10, Color.FromArgb(128, 10, 20, 30));
            Assert.AreEqual(10, paletteEntry.Value);

            paletteEntry.Value = -10;
            Assert.AreEqual(-10, paletteEntry.Value);
        }


        [Test]
        public void ColorTest()
        {
            PaletteEntry paletteEntry = new PaletteEntry(10, Color.FromArgb(128, 10, 20, 30));
            Assert.AreEqual(Color.FromArgb(128, 10, 20, 30), paletteEntry.Color);

            paletteEntry.Color = Colors.Cyan;
            Assert.AreEqual(Colors.Cyan, paletteEntry.Color);
        }

        [Test]
        public void EqualsTest()
        {
            PaletteEntry paletteEntry = new PaletteEntry();
            PaletteEntry paletteEntry2 = new PaletteEntry(0, Colors.Black);
            PaletteEntry paletteEntry3 = new PaletteEntry(1, new Color());

            Assert.IsTrue(paletteEntry.Equals(paletteEntry));
            Assert.IsTrue(paletteEntry.Equals((object)paletteEntry));
            Assert.IsFalse(paletteEntry.Equals(paletteEntry2));
            Assert.IsFalse(paletteEntry.Equals(paletteEntry3));
            Assert.IsFalse(paletteEntry2.Equals(paletteEntry3));

            Assert.IsFalse(paletteEntry.Equals(null));
            Assert.IsFalse(paletteEntry.Equals(new Color()));
        }


        [Test]
        public void GetHashCodeTest()
        {
            PaletteEntry paletteEntry = new PaletteEntry();
            PaletteEntry paletteEntry2 = new PaletteEntry(0, Colors.Black);
            PaletteEntry paletteEntry3 = new PaletteEntry(1, new Color());

            Assert.AreNotEqual(paletteEntry.GetHashCode(), paletteEntry2.GetHashCode());
            Assert.AreNotEqual(paletteEntry.GetHashCode(), paletteEntry3.GetHashCode());
            Assert.AreNotEqual(paletteEntry2.GetHashCode(), paletteEntry3.GetHashCode());
        }


        [Test]
        public void ToStringTest()
        {
            PaletteEntry paletteEntry1 = new PaletteEntry();
            PaletteEntry paletteEntry2 = new PaletteEntry(1, Colors.Black);
            string s1 = paletteEntry1.ToString();
            string s2 = paletteEntry2.ToString();
            Assert.IsNotNull(s1);
            Assert.IsNotNull(s2);
            Assert.AreNotEqual(s1, s2);
        }
    }
}
