using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Tests
{

  [TestFixture]
  public class GeometricObjectTest
  {
    class MyGeometricObject : GeometricObject
    {
      public MyGeometricObject(Shape shape, Pose pose)
        : base(shape, pose)
      {
      }
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorsShouldThrowArgumentNullException()
    {
      new MyGeometricObject(null, new Pose());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetShapeShouldThrowArgumentNullException()
    {
      var g = new MyGeometricObject(new SphereShape(1), new Pose());
      g.Shape = null;
    }


    [Test]
    public void Clone()
    {
      var o = new GeometricObject(new BoxShape(1, 2, 3), new Pose(new Vector3F(1, 2, 3)));
      var clone = o.Clone();

      Assert.AreEqual(o.Pose, clone.Pose);
      Assert.AreEqual(((BoxShape)o.Shape).Extent, ((BoxShape)clone.Shape).Extent);
    }


    [Test]
    public void ICloneableClone()
    {
      var o = new GeometricObject(new BoxShape(1, 2, 3), new Pose(new Vector3F(1, 2, 3)));
      var clone = (GeometricObject)((IGeometricObject)o).Clone();

      Assert.AreEqual(o.Pose, clone.Pose);
      Assert.AreEqual(((BoxShape)o.Shape).Extent, ((BoxShape)clone.Shape).Extent);
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CloneShouldThrow()
    {
      var g = new MyGeometricObject(new SphereShape(1), new Pose());
      g.Clone();
    }
  }
}
