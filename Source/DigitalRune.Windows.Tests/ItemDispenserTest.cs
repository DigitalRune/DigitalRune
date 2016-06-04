//using System;
//using System.Collections.Generic;
//#if NETFX_CORE || WINDOWS_PHONE
//using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
//using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
//using TestFixtureSetUp = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.ClassInitializeAttribute;
//using TestFixtureTearDown = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.ClassCleanupAttribute;
//using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
//using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
//using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
//#else
//using NUnit.Framework;
//#endif


//namespace DigitalRune.Windows.Tests
//{
//    [TestFixture]
//    public class ItemDispenserTest
//    {
//        private List<string> _items;

//        [SetUp]
//        public void SetUp()
//        {
//            _items = new List<string>
//            {
//                "Alfa",
//                "Bravo",
//                "Charlie",
//                "Delta",
//            };
//        }

//        [Test]
//        public void DefaultConstructorTest()
//        {
//            var itemDispenser = new ItemDispenser<string>();
//            Assert.AreEqual(0, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(0, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(0, itemDispenser.NumberOfItemsInUse);
//            Assert.IsNotNull(itemDispenser.Items);
//        }


//        [Test]
//        public void ConstructorTest()
//        {
//            var itemDispenser = new ItemDispenser<string>(_items);
//            Assert.AreEqual(4, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(4, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(0, itemDispenser.NumberOfItemsInUse);
//            Assert.IsNotNull(itemDispenser.Items);
//        }


//        [Test]
//        public void ItemDispenserWithValueTypes()
//        {
//            var itemDispenser = new ItemDispenser<int>(new[] { 1, 2, 3 });
//            Assert.AreEqual(3, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(3, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(0, itemDispenser.NumberOfItemsInUse);
//            Assert.IsNotNull(itemDispenser.Items);

//            int i = itemDispenser.NextItem();
//            Assert.AreEqual(1, i);
//            Assert.AreEqual(3, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(2, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(1, itemDispenser.NumberOfItemsInUse);

//            i = itemDispenser.NextItem();
//            Assert.AreEqual(2, i);
//            Assert.AreEqual(3, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(1, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(2, itemDispenser.NumberOfItemsInUse);

//            itemDispenser.ReleaseItem(1);
//            Assert.AreEqual(3, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(2, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(1, itemDispenser.NumberOfItemsInUse);

//            i = itemDispenser.NextItem();
//            Assert.AreEqual(1, i);
//            Assert.AreEqual(3, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(1, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(2, itemDispenser.NumberOfItemsInUse);
//        }


//        [Test]
//        public void ItemsTest()
//        {
//            var itemDispenser = new ItemDispenser<string>();
//            itemDispenser.Items = _items;
//            Assert.AreEqual(4, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(4, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(0, itemDispenser.NumberOfItemsInUse);
//            Assert.AreSame(_items, itemDispenser.Items);
//        }


//        [Test]
//        public void ItemsException()
//        {
//            var itemDispenser = new ItemDispenser<string>();
//            AssertHelper.Throws<ArgumentNullException>(() => itemDispenser.Items = null);
//        }

//        [Test]
//        public void EmptyDispenserException()
//        {
//            var itemDispenser = new ItemDispenser<string>();
//            AssertHelper.Throws<ItemNotAvailableException>(() => itemDispenser.NextItem());
//        }

//        [Test]
//        public void OutOfItemsException()
//        {
//            var itemDispenser = new ItemDispenser<string>(_items);
//            itemDispenser.NextItem();
//            itemDispenser.NextItem();
//            itemDispenser.NextItem();
//            itemDispenser.NextItem();
//            AssertHelper.Throws<ItemNotAvailableException>(() => itemDispenser.NextItem());
//        }

//        [Test]
//        public void NextItemAndReleaseItemTest()
//        {
//            var itemDispenser = new ItemDispenser<string>(_items);
//            Assert.AreEqual("Alfa", itemDispenser.NextItem());
//            Assert.AreEqual(4, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(3, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(1, itemDispenser.NumberOfItemsInUse);

//            Assert.AreEqual("Bravo", itemDispenser.NextItem());
//            Assert.AreEqual(2, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(2, itemDispenser.NumberOfItemsInUse);

//            Assert.AreEqual("Charlie", itemDispenser.NextItem());
//            Assert.AreEqual(1, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(3, itemDispenser.NumberOfItemsInUse);

//            Assert.AreEqual("Delta", itemDispenser.NextItem());
//            Assert.AreEqual(0, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(4, itemDispenser.NumberOfItemsInUse);

//            itemDispenser.ReleaseItem("Delta");
//            Assert.AreEqual(1, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(3, itemDispenser.NumberOfItemsInUse);

//            Assert.AreEqual("Delta", itemDispenser.NextItem());
//            Assert.AreEqual(0, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(4, itemDispenser.NumberOfItemsInUse);

//            itemDispenser.ReleaseItem("Delta");
//            Assert.AreEqual(1, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(3, itemDispenser.NumberOfItemsInUse);

//            itemDispenser.ReleaseItem("Bravo");
//            Assert.AreEqual(2, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(2, itemDispenser.NumberOfItemsInUse);

//            Assert.AreEqual("Bravo", itemDispenser.NextItem());
//            Assert.AreEqual(1, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(3, itemDispenser.NumberOfItemsInUse);

//            itemDispenser.Items.Add("Echo");
//            Assert.AreEqual(5, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(2, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(3, itemDispenser.NumberOfItemsInUse);

//            itemDispenser.Items.Remove("Delta");
//            Assert.AreEqual(4, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(1, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(3, itemDispenser.NumberOfItemsInUse);

//            itemDispenser.ReleaseItem("Delta"); // Should have no effect
//            Assert.AreEqual(4, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(1, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(3, itemDispenser.NumberOfItemsInUse);

//            Assert.AreEqual("Echo", itemDispenser.NextItem());
//            Assert.AreEqual(0, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(4, itemDispenser.NumberOfItemsInUse);

//            // Remove a used item
//            itemDispenser.Items.Remove("Alfa");
//            itemDispenser.ReleaseItem("Alfa");
//        }


//        [Test]
//        public void ReleaseAllTest()
//        {
//            var itemDispenser = new ItemDispenser<string>();
//            itemDispenser.ReleaseAllItems();
//            Assert.AreEqual(0, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(0, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(0, itemDispenser.NumberOfItemsInUse);
//            Assert.IsNotNull(itemDispenser.Items);

//            itemDispenser.Items = _items;
//            itemDispenser.ReleaseAllItems();
//            Assert.AreEqual(4, itemDispenser.TotalNumberOfItems);
//            Assert.AreEqual(4, itemDispenser.NumberOfItemsAvailable);
//            Assert.AreEqual(0, itemDispenser.NumberOfItemsInUse);
//            Assert.AreSame(_items, itemDispenser.Items);
//        }
//    }
//}
