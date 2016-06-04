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
    public class CategoriesTest
    {
        [Test]
        public void ShouldMatchDefaultCategoryAttributes()
        {
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Action.Category, Categories.Action);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Appearance.Category, Categories.Appearance);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Asynchronous.Category, Categories.Asynchronous);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Behavior.Category, Categories.Behavior);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Data.Category, Categories.Data);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Default.Category, Categories.Default);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Design.Category, Categories.Design);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.DragDrop.Category, Categories.DragDrop);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Focus.Category, Categories.Focus);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Format.Category, Categories.Format);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Key.Category, Categories.Key);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Layout.Category, Categories.Layout);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.Mouse.Category, Categories.Mouse);
            Assert.AreEqual(System.ComponentModel.CategoryAttribute.WindowStyle.Category, Categories.WindowStyle);
        }
    }
}
