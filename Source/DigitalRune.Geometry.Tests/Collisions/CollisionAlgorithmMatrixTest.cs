using System;
using DigitalRune.Geometry.Shapes;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Tests
{
  [TestFixture]
  public class CollisionAlgorithmMatrixTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestException()
    {
      new CollisionAlgorithmMatrix(new CollisionDetection())[typeof(BoxShape), typeof(CapsuleShape)] = null;
    }
  }
}
