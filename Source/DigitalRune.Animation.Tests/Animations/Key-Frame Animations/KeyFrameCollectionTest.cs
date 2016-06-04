using System.Linq;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class KeyFrameCollectionTest
  {
    [Test]
    public void GetEnumeratorTest()
    {
      var collection = new KeyFrameCollection<QuaternionF>();
      var keyFrame = new KeyFrame<QuaternionF>();
      var keyFrame2 = new KeyFrame<QuaternionF>();
      collection.Add(keyFrame);
      collection.Add(keyFrame2);

      foreach (var k in collection)
      { }

      Assert.AreEqual(2, collection.Count());
      Assert.Contains(keyFrame, collection);
      Assert.Contains(keyFrame2, collection);
    }
  }
}
