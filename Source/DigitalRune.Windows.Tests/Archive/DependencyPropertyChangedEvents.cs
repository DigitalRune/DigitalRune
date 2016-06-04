using System.Windows;
using System.Windows.Controls;
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
    public class DependencyPropertyChangedEventsTest
    {
        [Test]
        public void ChangeNotifications()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                new DependencyPropertyChangedEventsTestFrameworkElement().ChangedNotifications();
            });
        }
    }


    public class DependencyPropertyChangedEventsTestFrameworkElement : FrameworkElement
    {
        public void ChangedNotifications()
        {
            bool eventHandlerCalled;
            var button = new Button();
            WindowsHelper.RegisterPropertyChangedEventHandler(
                this,
                button,
                "Width",
                typeof(FrameworkElement),
                typeof(FrameworkElement),
                (s, e) =>
                {
                    Assert.IsTrue(s is Button);
                    eventHandlerCalled = true;
                });

            eventHandlerCalled = false;
            button.Width = 100;
            Assert.IsTrue(eventHandlerCalled);
        }
    }


#if !SILVERLIGHT && !WINDOWS_PHONE
    [TestFixture]
    public class DependencyPropertyChangedEventsTest_WPF
    {
        [Test]
        public void ChangedNotifications()
        {
            bool eventHandlerCalled;
            var button = new Button();
            WindowsHelper.RegisterPropertyChangedEventHandler(
                null,
                button,
                "Width",
                typeof(FrameworkElement),
                typeof(FrameworkElement),
                (s, e) =>
                {
                    Assert.IsTrue(s is Button);
                    eventHandlerCalled = true;
                });

            eventHandlerCalled = false;
            button.Width = 100;
            Assert.IsTrue(eventHandlerCalled);
        }
    }
#endif
}
