using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class CollectionChangedEventArgsTest
  {
    [Test]
    public void Test()
    {
      var e = CollectionChangedEventArgs<object>.Create();

      Assert.AreEqual(-1, e.OldItemsIndex);
      Assert.AreEqual(-1, e.NewItemsIndex);
      Assert.AreEqual(0, e.OldItems.Count);
      Assert.AreEqual(0, e.NewItems.Count);

      e.NewItemsIndex = 10;
      e.OldItemsIndex = 100;
      e.NewItems.Add(1);
      e.OldItems.Add(2);
      e.OldItems.Add(3);

      e.Recycle();

      e = CollectionChangedEventArgs<object>.Create();

      Assert.AreEqual(-1, e.OldItemsIndex);
      Assert.AreEqual(-1, e.NewItemsIndex);
      Assert.AreEqual(0, e.OldItems.Count);
      Assert.AreEqual(0, e.NewItems.Count);
    }
  }
}
