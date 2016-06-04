using System.Text.RegularExpressions;
using NUnit.Framework;


namespace DigitalRune.Windows.Docking.Tests
{
    [TestFixture]
    public class SerializationTest
    {
        private DockControlViewModel CreateDockLayout()
        {
            var dockControl = new DockControlViewModel(new TestDockStrategy());

            var item0 = new DockTabItemViewModel
            {
                Title = "Item 0",
                DockId = "Item0"
            };
            var item1 = new DockTabItemViewModel
            {
                Title = "Item 1",
                DockId = "Item1"
            };
            var item2 = new DockTabItemViewModel
            {
                Title = "Item 2",
                DockId = "Item2"
            };
            var item3 = new DockTabItemViewModel
            {
                Title = "Item 3",
                DockId = "Item3"
            };
            var item4 = new DockTabItemViewModel
            {
                Title = "Item 4",
                DockId = "Item4"
            };
            var item5 = new DockTabItemViewModel
            {
                Title = "Item 5",
                DockId = "Item5"
            };
            dockControl.DockStrategy.Dock(item0);
            dockControl.DockStrategy.Float(item0);
            dockControl.DockStrategy.Dock(item1);
            dockControl.DockStrategy.Dock(item2);
            dockControl.DockStrategy.Dock(item3, DockHelper.GetParent(dockControl.RootPane, item2), DockPosition.Right);
            dockControl.DockStrategy.Dock(item4, dockControl.RootPane, DockPosition.Bottom);
            dockControl.DockStrategy.AutoHide(item4);
            dockControl.DockStrategy.Dock(item5, dockControl.FloatWindows[0].RootPane, DockPosition.Inside);

            return dockControl;
        }


        [Test]
        public void Serialization0()
        {
            DockControlViewModel dockControl = CreateDockLayout();
            
            // Serialize layout.
            var xElement1 = DockSerializer.Save(dockControl);

            // Make some changes.
            dockControl.DockStrategy.Close(dockControl.FloatWindows[0]);

            // Deserialize and serialize again.
            DockSerializer.Load(dockControl, xElement1);
            var xElement2 = DockSerializer.Save(dockControl);

            // We want to compare the two created XMLs. But first we must remove the 
            // LastActivation="..." entries, because they will not match.
            var string1 = xElement1.ToString();
            var string2 = xElement2.ToString();
            var r = new Regex(@"LastActivation="".*""");
            string1 = r.Replace(string1, string.Empty);
            string2 = r.Replace(string2, string.Empty);

            Assert.AreEqual(string1, string2);
        }
    }
}
