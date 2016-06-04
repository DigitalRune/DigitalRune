using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class ArrayListTest
  {
    [Test]
    public void Insert()
    {
      var list = new ArrayList<int>(4);

      Assert.That(() => list.Insert(-1, 0), Throws.Exception);
      Assert.That(() => list.Insert(1, 0), Throws.Exception);

      // Insert into empty list.
      list.Insert(0, 1);
      Assert.AreEqual(1, list.Count);
      Assert.AreEqual(1, list.Array[0]);

      // Insert at begin.
      list.Insert(0, 2);
      Assert.AreEqual(2, list.Count);
      Assert.AreEqual(2, list.Array[0]);
      Assert.AreEqual(1, list.Array[1]);

      // Insert at end.
      list.Insert(2, 3);
      Assert.AreEqual(3, list.Count);
      Assert.AreEqual(2, list.Array[0]);
      Assert.AreEqual(1, list.Array[1]);
      Assert.AreEqual(3, list.Array[2]);

      // Insert in middle.
      list.Insert(1, 4);
      Assert.AreEqual(4, list.Count);
      Assert.AreEqual(2, list.Array[0]);
      Assert.AreEqual(4, list.Array[1]);
      Assert.AreEqual(1, list.Array[2]);
      Assert.AreEqual(3, list.Array[3]);
    }


    [Test]
    public void RemoveAt()
    {
      var list = new ArrayList<int>(4);
      list.Add(0);
      list.Add(1);
      list.Add(2);
      list.Add(3);
      list.Add(4);

      Assert.That(() => list.RemoveAt(-1), Throws.Exception);
      Assert.That(() => list.RemoveAt(6), Throws.Exception);

      // Remove from start.
      list.RemoveAt(0);
      Assert.AreEqual(4, list.Count);
      Assert.AreEqual(1, list.Array[0]);
      Assert.AreEqual(2, list.Array[1]);
      Assert.AreEqual(3, list.Array[2]);
      Assert.AreEqual(4, list.Array[3]);
      Assert.AreEqual(0, list.Array[4]);

      // Remove from middle.
      list.RemoveAt(2);
      Assert.AreEqual(3, list.Count);
      Assert.AreEqual(1, list.Array[0]);
      Assert.AreEqual(2, list.Array[1]);
      Assert.AreEqual(4, list.Array[2]);
      Assert.AreEqual(0, list.Array[3]);
      Assert.AreEqual(0, list.Array[4]);

      // Remove from end.
      list.RemoveAt(2);
      Assert.AreEqual(2, list.Count);
      Assert.AreEqual(1, list.Array[0]);
      Assert.AreEqual(2, list.Array[1]);
      Assert.AreEqual(0, list.Array[2]);
      Assert.AreEqual(0, list.Array[3]);
      Assert.AreEqual(0, list.Array[4]);
    }
  }
}
