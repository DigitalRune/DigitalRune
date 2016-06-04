using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Tests
{
  [TestFixture]
  public class CollisionObjectTest
  {
    [Test]
    public void AxisAlignedBoundingBox()
    {
      CollisionObject obj = new CollisionObject(new GeometricObject(new SphereShape(0.3f), new Pose(new Vector3F(1, 2, 3))));

      Assert.AreEqual(new Aabb(new Vector3F(0.7f, 1.7f, 2.7f), new Vector3F(1.3f, 2.3f, 3.3f)),
                      obj.GeometricObject.Aabb);
    }


    [Test]
    public void ConstructorTest()
    {
      Assert.AreEqual(typeof(EmptyShape), new CollisionObject().GeometricObject.Shape.GetType());
      Assert.AreEqual(Pose.Identity, new CollisionObject().GeometricObject.Pose);
    }


    //[Test]
    //public void ToStringTest()
    //{
    //  Assert.AreEqual("CollisionObject{Name=\"\"}", new CollisionObject().ToString());
    //  Assert.AreEqual("CollisionObject{Name=\"Cube1\"}", new CollisionObject { Name = "Cube1" }.ToString());
    //}
  }
}
