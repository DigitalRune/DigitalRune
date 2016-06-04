using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class ConvexHullOfShapesTest
  {
    GeometricObject child0, child1;
    ConvexHullOfShapes cs;
    
    [SetUp]
    public void SetUp()
    {
      child0 = new GeometricObject(new CircleShape(3), new Pose(new Vector3F(0, 5, 0), QuaternionF.CreateRotationX(ConstantsF.PiOver2)));
      child1 = new GeometricObject(new CircleShape(3), new Pose(new Vector3F(0, -5, 0), QuaternionF.CreateRotationX(ConstantsF.PiOver2)));

      cs = new ConvexHullOfShapes();
      cs.Children.Add(child0);
      cs.Children.Add(child1);
    }

    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new ConvexHullOfShapes().Children.Count);
      Assert.AreEqual(2, cs.Children.Count);
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new ConvexHullOfShapes().InnerPoint);
      Assert.AreEqual(new Vector3F(0, 0, 0), cs.InnerPoint);
      cs.Children.Add(new GeometricObject(new PointShape(new Vector3F(5, 0, 0)), new Pose(new Vector3F(1, 0, 0), QuaternionF.Identity)));
      Assert.AreEqual(new Vector3F(2, 0, 0), cs.InnerPoint);
    }


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new ConvexHullOfShapes().GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new ConvexHullOfShapes().GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new ConvexHullOfShapes().GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new ConvexHullOfShapes().GetSupportPoint(new Vector3F(1, 1, 1)));

      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(3, 5, 0), cs.GetSupportPoint(new Vector3F(1, 0, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(3, 5, 0), cs.GetSupportPoint(new Vector3F(0, 1, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 5, 3), cs.GetSupportPoint(new Vector3F(0, 0, 1))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-3, 5, 0), cs.GetSupportPoint(new Vector3F(-1, 0, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(3, -5, 0), cs.GetSupportPoint(new Vector3F(0, -1, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 5, -3), cs.GetSupportPoint(new Vector3F(0, 0, -1))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 5, 0) + 3 * new Vector3F(1, 0, 1).Normalized, cs.GetSupportPoint(new Vector3F(1, 1, 1))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -5, 0) + 3 * new Vector3F(-1, 0, -1).Normalized, cs.GetSupportPoint(new Vector3F(-1, -1, -1))));
    }


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("ConvexHullOfShapes { Count = 2 }", cs.ToString());
    }


    [Test]
    public void Clone()
    {
      ConvexHullOfShapes convexHullOfShapes = new ConvexHullOfShapes();
      for (int i = 0; i < 10; i++)
      {
        Pose pose = new Pose(new Vector3F(i, i, i));
        PointShape point = new PointShape(i, i, i);
        GeometricObject geometry = new GeometricObject(point, pose);
        convexHullOfShapes.Children.Add(geometry);
      }

      ConvexHullOfShapes clone = convexHullOfShapes.Clone() as ConvexHullOfShapes;
      Assert.IsNotNull(clone);
      Assert.AreEqual(10, clone.Children.Count);
      for (int i = 0; i < 10; i++)
      {
        Assert.IsNotNull(clone.Children[i]);
        Assert.AreNotSame(convexHullOfShapes.Children[i], clone.Children[i]);
        Assert.IsTrue(clone.Children[i] is GeometricObject);
        Assert.AreEqual(convexHullOfShapes.Children[i].Pose, clone.Children[i].Pose);
        Assert.IsNotNull(clone.Children[i].Shape);
        Assert.AreNotSame(convexHullOfShapes.Children[i].Shape, clone.Children[i].Shape);
        Assert.IsTrue(clone.Children[i].Shape is PointShape);
        Assert.AreEqual(((PointShape)convexHullOfShapes.Children[i].Shape).Position, ((PointShape)clone.Children[i].Shape).Position);
      }

      Assert.AreEqual(convexHullOfShapes.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(convexHullOfShapes.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }
  }
}
