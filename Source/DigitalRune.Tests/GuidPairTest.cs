//using System;
//using NUnit.Framework;


//namespace DigitalRune.Tests
//{
//  [TestFixture]
//  public class GuidPairTest
//  {
//    [Test]
//    public void Test1()
//    {
//      Guid a = Guid.NewGuid();
//      Guid b = Guid.NewGuid();
      
//      // Constructors
//      GuidPair pair1 = new GuidPair();
//      pair1.GuidA = a;
//      pair1.GuidB = b;
//      Assert.AreEqual(a, pair1.GuidA);
//      Assert.AreEqual(b, pair1.GuidB);

//      GuidPair pair2 = new GuidPair(a, b);
//      Assert.AreEqual(a, pair2.GuidA);
//      Assert.AreEqual(b, pair2.GuidB);

//      GuidPair swappedPair = new GuidPair(b, a);
//      Assert.AreEqual(a, swappedPair.GuidB);
//      Assert.AreEqual(b, swappedPair.GuidA);

//      // HashCode
//      Assert.AreEqual(pair1.GetHashCode(), pair2.GetHashCode());
//      Assert.AreEqual(pair1.GetHashCode(), swappedPair.GetHashCode());
//      Assert.AreNotEqual(new GuidPair(a, a).GetHashCode(), new GuidPair(b, b).GetHashCode());

//      // Equals
//      Assert.AreEqual(pair1, pair2);
//      Assert.IsTrue(pair1.Equals(pair1));
//      Assert.IsTrue(pair1.Equals(pair2));
//      Assert.IsTrue(pair1.Equals(swappedPair));
//      Assert.IsTrue(pair1.Equals((object)pair2));
//      Assert.IsFalse(pair1.Equals(null));
//      Assert.IsFalse(pair1.Equals((object)a));
      
//      // ToString
//      Assert.AreEqual("(GuidA=" + a + ", GuidB=" + b + ")", pair1.ToString());
//      Assert.AreEqual("(GuidA=" + b + ", GuidB=" + a + ")", swappedPair.ToString());
//    }


//    [Test]
//    public void CompareToTest()
//    {
//      Guid guidA = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
//      Guid guidB = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
//      Guid guidC = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
//      Guid guidD = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);

//      Assert.AreEqual(0, new GuidPair(guidA, guidB).CompareTo(new GuidPair(guidA, guidB)));
//      Assert.AreEqual(0, new GuidPair(guidA, guidB).CompareTo(new GuidPair(guidB, guidA)));

//      Assert.AreEqual(-1, new GuidPair(guidB, guidC).CompareTo(new GuidPair(guidC, guidD)));
//      Assert.AreEqual(-1, new GuidPair(guidC, guidB).CompareTo(new GuidPair(guidB, guidD)));
//      Assert.AreEqual(1, new GuidPair(guidB, guidC).CompareTo(new GuidPair(guidD, guidA)));
//      Assert.AreEqual(1, new GuidPair(guidB, guidC).CompareTo(new GuidPair(guidA, guidD)));
//    }

//    [Test]
//    public void Operators()
//    {
//      Guid guidA = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
//      Guid guidB = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
//      Guid guidC = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
//      Guid guidD = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);

//      Assert.IsTrue(new GuidPair(guidA, guidB) == (new GuidPair(guidA, guidB)));
//      Assert.IsTrue(new GuidPair(guidA, guidB) == (new GuidPair(guidB, guidA)));
//      Assert.IsFalse(new GuidPair(guidA, guidB) == (new GuidPair(guidB, guidC)));

//      Assert.IsFalse(new GuidPair(guidA, guidB) != (new GuidPair(guidA, guidB)));
//      Assert.IsFalse(new GuidPair(guidA, guidB) != (new GuidPair(guidB, guidA)));
//      Assert.IsTrue(new GuidPair(guidA, guidB) != (new GuidPair(guidB, guidC)));

//      Assert.IsFalse(new GuidPair(guidA, guidB) < (new GuidPair(guidA, guidB)));
//      Assert.IsFalse(new GuidPair(guidA, guidB) < (new GuidPair(guidB, guidA)));
//      Assert.IsTrue(new GuidPair(guidA, guidB) < (new GuidPair(guidB, guidC)));
//      Assert.IsTrue(new GuidPair(guidD, guidA) < (new GuidPair(guidB, guidC)));

//      Assert.IsTrue(new GuidPair(guidA, guidB) <= (new GuidPair(guidA, guidB)));
//      Assert.IsTrue(new GuidPair(guidA, guidB) <= (new GuidPair(guidB, guidA)));
//      Assert.IsTrue(new GuidPair(guidA, guidB) <= (new GuidPair(guidB, guidC)));
//      Assert.IsTrue(new GuidPair(guidD, guidA) <= (new GuidPair(guidB, guidC)));
//      Assert.IsFalse(new GuidPair(guidD, guidC) <= (new GuidPair(guidB, guidC)));

//      Assert.IsFalse(new GuidPair(guidA, guidB) > (new GuidPair(guidA, guidB)));
//      Assert.IsFalse(new GuidPair(guidA, guidB) > (new GuidPair(guidB, guidA)));
//      Assert.IsFalse(new GuidPair(guidA, guidB) > (new GuidPair(guidB, guidC)));
//      Assert.IsFalse(new GuidPair(guidD, guidA) > (new GuidPair(guidB, guidC)));
//      Assert.IsTrue(new GuidPair(guidD, guidB) > (new GuidPair(guidB, guidC)));

//      Assert.IsTrue(new GuidPair(guidA, guidB) >= (new GuidPair(guidA, guidB)));
//      Assert.IsTrue(new GuidPair(guidA, guidB) >= (new GuidPair(guidB, guidA)));
//      Assert.IsFalse(new GuidPair(guidA, guidB) >= (new GuidPair(guidB, guidC)));
//      Assert.IsFalse(new GuidPair(guidD, guidA) >= (new GuidPair(guidB, guidC)));
//      Assert.IsTrue(new GuidPair(guidD, guidB) >= (new GuidPair(guidB, guidC)));
//    }
//  }
//}
