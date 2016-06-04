using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Partitioning.Tests
{
  [TestFixture]
  public class DynamicAabbTreeTest
  {
    private Aabb GetAabbForItem(int i)
    {
      switch (i)
      {
        case 0:
          return new Aabb(new Vector3F(float.NegativeInfinity), new Vector3F(float.PositiveInfinity));
        case 1:
          return new Aabb(new Vector3F(-1), new Vector3F(2));
        case 2:
          return new Aabb(new Vector3F(1), new Vector3F(3));
        case 3:
          return new Aabb(new Vector3F(4), new Vector3F(5));
        case 4:
          return new Aabb(new Vector3F(0), new Vector3F(1, float.NaN, 1));
        case 5: case 6: case 7:
          return new Aabb(new Vector3F(0), new Vector3F(float.PositiveInfinity));
        default:
          return new Aabb(new Vector3F(), new Vector3F());
      }
    }


    [Test]
    public void Infinite()
    {
      GlobalSettings.ValidationLevel = 0xff;

      var partition = new DynamicAabbTree<int>
      {
        EnableSelfOverlaps = true,
        GetAabbForItem = GetAabbForItem
      };

      partition.Add(1);
      partition.Add(0);
      partition.Add(2);
      partition.Add(3);

      Assert.AreEqual(new Aabb(new Vector3F(float.NegativeInfinity), new Vector3F(float.PositiveInfinity)), partition.Aabb);

      var overlaps = partition.GetOverlaps().ToArray();
      Assert.AreEqual(4, overlaps.Length);
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 1)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 2)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 3)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(1, 2)));

      partition.Add(5);
      partition.Add(6);
      partition.Add(7);
      partition.Update(true);
      Assert.AreEqual(new Aabb(new Vector3F(float.NegativeInfinity), new Vector3F(float.PositiveInfinity)), partition.Aabb);

      overlaps = partition.GetOverlaps().ToArray();
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 1)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 2)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 3)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(1, 2)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(1, 5)));
    }


    [Test]
    public void NaN()
    {
      GlobalSettings.ValidationLevel = 0x00;

      var partition = new DynamicAabbTree<int>
      {
        EnableSelfOverlaps = true,
        GetAabbForItem = GetAabbForItem
      };

      partition.Add(1);
      partition.Add(4);
      partition.Add(2);
      partition.Add(3);

      // Aabb builder throws exception.
      Assert.Throws<GeometryException>(() => partition.Update(false));
    }


    [Test]
    public void NaNWithValidation()
    {
      GlobalSettings.ValidationLevel = 0xff;

      var partition = new DynamicAabbTree<int>();
      partition.EnableSelfOverlaps = true;
      partition.GetAabbForItem = GetAabbForItem;

      partition.Add(1);
      partition.Add(4);
      partition.Add(2);
      partition.Add(3);

      // Full rebuild.
      Assert.Throws<GeometryException>(() => partition.Update(true));

      partition = new DynamicAabbTree<int>();
      partition.EnableSelfOverlaps = true;
      partition.GetAabbForItem = GetAabbForItem;

      partition.Add(1);
      partition.Add(2);
      partition.Add(3);
      partition.Update(true);
      partition.Add(4);

      // Partial rebuild.
      Assert.Throws<GeometryException>(() => partition.Update(false));
    }

    [Test]
    public void Clone()
    {
      DynamicAabbTree<int> partition = new DynamicAabbTree<int>();
      partition.GetAabbForItem = i => new Aabb();
      partition.EnableSelfOverlaps = true;
      partition.Filter = new DelegatePairFilter<int>(pair => true);
      partition.EnableMotionPrediction = true;
      partition.MotionPrediction = 0.25f;
      partition.OptimizationPerFrame = 0.1f;
      partition.RelativeMargin = 0.33f;
      partition.Add(0);
      partition.Add(1);
      partition.Add(2);
      partition.Add(3);

      var clone = (DynamicAabbTree<int>)partition.Clone();
      Assert.NotNull(clone);
      Assert.AreNotSame(clone, partition);
      Assert.AreEqual(clone.EnableSelfOverlaps, partition.EnableSelfOverlaps);
      Assert.AreEqual(clone.Filter, partition.Filter);
      Assert.AreEqual(clone.EnableMotionPrediction, partition.EnableMotionPrediction);
      Assert.AreEqual(clone.MotionPrediction, partition.MotionPrediction);
      Assert.AreEqual(clone.OptimizationPerFrame, partition.OptimizationPerFrame);
      Assert.AreEqual(clone.RelativeMargin, partition.RelativeMargin);
      Assert.AreEqual(0, clone.Count);

      clone.Add(0);
      Assert.AreEqual(4, partition.Count);
      Assert.AreEqual(1, clone.Count);
    }
  }
}
