//using System;
//using System.Linq;
//using NUnit.Framework;


//namespace DigitalRune.Tests
//{
//  [TestFixture]
//  public class ResourcePoolUnsafeTest
//  {
//    [Test]
//    [ExpectedException(typeof(ArgumentNullException))]
//    public void ConstructorShouldThrowWhenNull()
//    {
//      var resourcePool = new ResourcePoolUnsafe<object>(null, null, null);
//    }


//    [Test]
//    public void ShouldBeRegistered()
//    {
//      var resourcePool = new ResourcePoolUnsafe<object>(() => new object(), null, null);
//      Assert.IsTrue(ResourcePool.Pools.Contains(resourcePool));
//    }


//    [Test]
//    public void ObtainAndRecycle()
//    {
//      object expected = null;
//      Action<object> initialize = s =>
//      {
//        Assert.IsNotNull(s);
//        if (expected != null)
//          Assert.AreSame(expected, s);
//      };
//      Action<object> uninitialize = s =>
//      {
//        Assert.IsNotNull(s);
//        if (expected != null)
//          Assert.AreSame(expected, s);
//      };

//      var resourcePool = new ResourcePoolUnsafe<object>(() => new object(), initialize, uninitialize);
//      object item1 = resourcePool.Obtain();
//      Assert.IsNotNull(item1);
//      object item2 = resourcePool.Obtain();

//      expected = item2;
//      resourcePool.Recycle(item2);
//      object item3 = resourcePool.Obtain();
//      Assert.AreEqual(item2, item3);
//    }
//  }
//}
