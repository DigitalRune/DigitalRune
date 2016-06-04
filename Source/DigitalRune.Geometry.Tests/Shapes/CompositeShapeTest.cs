using System;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class CompositeShapeTest
  {
    GeometricObject child0, child1;
    CompositeShape cs;

    [SetUp]
    public void SetUp()
    {
      child0 = new GeometricObject(new CircleShape(3), new Pose(new Vector3F(0, 5, 0), QuaternionF.CreateRotationX(ConstantsF.PiOver2)));
      child1 = new GeometricObject(new CircleShape(3), new Pose(new Vector3F(0, -5, 0), QuaternionF.CreateRotationX(ConstantsF.PiOver2)));

      cs = new CompositeShape();
      cs.Children.Add(child0);
      cs.Children.Add(child1);
    }

    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new CompositeShape().Children.Count);
      Assert.AreEqual(2, cs.Children.Count);
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new CompositeShape().InnerPoint);
      Assert.AreEqual(new Vector3F(0, 5, 0), cs.InnerPoint);
      cs.Children.Add(new GeometricObject(new PointShape(new Vector3F(5, 0, 0)), new Pose(new Vector3F(1, 0, 0), QuaternionF.Identity)));
      Assert.AreEqual(new Vector3F(0, 5, 0), cs.InnerPoint);
    }


    //[Test]
    //public void GetAabb()
    //{
    //  Assert.AreEqual(new Aabb(), new ConvexHullOfPoints().GetAabb(Pose.Identity));
    //  Assert.AreEqual(new Aabb(new Vector3F(10, 100, -13), new Vector3F(10, 100, -13)),
    //                 new ConvexHullOfPoints().GetAabb(new Pose(new Vector3F(10, 100, -13),
    //                                                                     QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
    //  Assert.AreEqual(new Aabb(new Vector3F(11, 102, 1003), new Vector3F(11, 102, 1003)),
    //                 new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetAabb(new Pose(new Vector3F(10, 100, 1000),
    //                                                                     QuaternionF.Identity)));
    //  QuaternionF rotation = QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f);
    //  Vector3F worldPos = rotation.Rotate(new Vector3F(1, 2, 3)) + new Vector3F(10, 100, 1000);
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(worldPos, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetAabb(new Pose(new Vector3F(10, 100, 1000), rotation)).Minimum));
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(worldPos, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetAabb(new Pose(new Vector3F(10, 100, 1000), rotation)).Maximum));
    //}


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3F(1, 1, 1)));

    //  Assert.AreEqual(1, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(2, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(3, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.ProjectTo(new Vector3F(1, 2, 3), new Vector3F(1, 1, 1)).Length, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetSupportPointDistance(new Vector3F(1, 1, 1))));
    //}


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("CompositeShape { Count = 2 }", cs.ToString());
    }


    //[Test]
    //public void TestBinarySerialization()
    //{
    //  MemoryStream stream = new MemoryStream();
    //  BinaryFormatter formatter = new BinaryFormatter();
    //  formatter.Serialize(stream, cs);

    //  stream.Seek(0, SeekOrigin.Begin);
    //  CompositeShape cs2 = (CompositeShape) formatter.Deserialize(stream);
    //  Assert.AreEqual(cs, cs2);

    //  CompositeShape cs3 = new CompositeShape();
    //  cs3.Children.Add(new DefaultGeometry(new Pose(new Vector3F(0, 5, 0), QuaternionF.CreateRotationX(ConstantsF.PiOver2)), new CircleShape(3)));
    //  cs3.Children.Add(new DefaultGeometry(new Pose(new Vector3F(0, -5, 0), QuaternionF.CreateRotationX(ConstantsF.PiOver2)), new CircleShape(3)));
    //  Assert.AreEqual(cs3, cs2);
    //}


    //[Test]
    //public void TestXmlSerialization()
    //{
    //  StringBuilder buffer = new StringBuilder();
    //  StringWriter writer = new StringWriter(buffer);
    //  XmlSerializer serializer = new XmlSerializer(typeof(CompositeShape));
    //  serializer.Serialize(writer, cs);
    //  writer.Close();

    //  StringReader reader = new StringReader(buffer.ToString());
    //  CompositeShape cs2 = (CompositeShape) serializer.Deserialize(reader);
    //  Assert.AreEqual(cs, cs2);
    //}


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestNullChilds0()
    {
      new CompositeShape().Children.Add(null);
    }


    [Test]
    public void Clone()
    {
      CompositeShape compositeShape = new CompositeShape();
      for (int i = 0; i < 10; i++)
      {
        Pose pose = new Pose(new Vector3F(i, i, i));
        PointShape point = new PointShape(i, i, i);
        GeometricObject geometry = new GeometricObject(point, pose);
        compositeShape.Children.Add(geometry);
      }

      CompositeShape clone = compositeShape.Clone() as CompositeShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(10, clone.Children.Count);
      for (int i = 0; i < 10; i++)
      {
        Assert.IsNotNull(clone.Children[i]);
        Assert.AreNotSame(compositeShape.Children[i], clone.Children[i]);
        Assert.IsTrue(clone.Children[i] is GeometricObject);
        Assert.AreEqual(compositeShape.Children[i].Pose, clone.Children[i].Pose);
        Assert.IsNotNull(clone.Children[i].Shape);
        Assert.AreNotSame(compositeShape.Children[i].Shape, clone.Children[i].Shape);
        Assert.IsTrue(clone.Children[i].Shape is PointShape);
        Assert.AreEqual(((PointShape)compositeShape.Children[i].Shape).Position, ((PointShape)clone.Children[i].Shape).Position);
      }

      Assert.AreEqual(compositeShape.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(compositeShape.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void CloneWithPartition()
    {
      CompositeShape compositeShape = new CompositeShape();
      for (int i = 0; i < 10; i++)
      {
        Pose pose = new Pose(new Vector3F(i, i, i));
        PointShape point = new PointShape(i, i, i);
        GeometricObject geometry = new GeometricObject(point, pose);
        compositeShape.Children.Add(geometry);
      }

      CloneWithPartition(compositeShape, new AabbTree<int>());
      CloneWithPartition(compositeShape, new AdaptiveAabbTree<int>());
      CloneWithPartition(compositeShape, new CompressedAabbTree());
      CloneWithPartition(compositeShape, new DynamicAabbTree<int>());
      CloneWithPartition(compositeShape, new SweepAndPruneSpace<int>());
    }


    private void CloneWithPartition(CompositeShape compositeShape, ISpatialPartition<int> partition)
    {
      compositeShape.Partition = partition;
      CompositeShape clone = compositeShape.Clone() as CompositeShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(10, clone.Children.Count);
      for (int i = 0; i < 10; i++)
      {
        Assert.IsNotNull(clone.Children[i]);
        Assert.AreNotSame(compositeShape.Children[i], clone.Children[i]);
        Assert.IsTrue(clone.Children[i] is GeometricObject);
        Assert.AreEqual(compositeShape.Children[i].Pose, clone.Children[i].Pose);
        Assert.IsNotNull(clone.Children[i].Shape);
        Assert.AreNotSame(compositeShape.Children[i].Shape, clone.Children[i].Shape);
        Assert.IsTrue(clone.Children[i].Shape is PointShape);
        Assert.AreEqual(((PointShape)compositeShape.Children[i].Shape).Position, ((PointShape)clone.Children[i].Shape).Position);
      }

      Assert.IsNotNull(clone.Partition);
      Assert.IsInstanceOf(partition.GetType(), clone.Partition);
      Assert.AreEqual(compositeShape.Children.Count, clone.Partition.Count);
      Assert.AreNotSame(partition, clone.Partition);
    }
  }
}
