using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Geometry.Partitioning.Tests
{
  [TestFixture]
  public class SpatialPartitionTest
  {
    class TestObject
    {
      public static int NextId;

      public int Id;
      public Aabb Aabb;
      public int Group;

      public TestObject(Aabb aabb)
      {
        Id = NextId++;
        Aabb = aabb;
        Group = RandomHelper.Random.NextInteger(0, 9);
      }

      public override string ToString()
      {
        return Id.ToString();
      }
    }


    List<TestObject> _testObjects = new List<TestObject>();
    List<TestObject> _testObjectsOfPartition2 = new List<TestObject>();
    private ISpatialPartition<int> _partition2; // Second test partition for partition vs. partition tests.
    private bool _conservativeAabb;
    private bool _conservativeOverlaps;


    private static Aabb GetAabbOfTestObject(List<TestObject> testObjects, int id)
    {
      TestObject testObject = testObjects.FirstOrDefault(to => to.Id == id);
      return (testObject != null) ? testObject.Aabb : new Aabb();      
    }


    private Aabb GetAabbOfTestObject(int id)
    {
      return GetAabbOfTestObject(_testObjects, id);
    }


    private Aabb GetAabbOfTestObjectOfPartition2(int id)
    {
      return GetAabbOfTestObject(_testObjectsOfPartition2, id);
    }


    private bool AreInSameGroup(Pair<int> pair)
    {
      TestObject firstObject = _testObjects.FirstOrDefault(to => to.Id == pair.First);
      TestObject secondObject = _testObjects.FirstOrDefault(to => to.Id == pair.Second);
      int firstGroup = (firstObject != null) ? firstObject.Group : 0;
      int secondGroup = (secondObject != null) ? secondObject.Group : 0;
      return firstGroup == secondGroup;
    }


    [SetUp]
    public void SetUp()
    {
      RandomHelper.Random = new Random(1234567);

      _testObjects.Clear();
      _testObjectsOfPartition2.Clear();

      TestObject.NextId = 0;
      _testObjectsOfPartition2.Add(new TestObject(GetRandomAabb()));
      _testObjectsOfPartition2.Add(new TestObject(GetRandomAabb()));
      _testObjectsOfPartition2.Add(new TestObject(GetRandomAabb()));
      _testObjectsOfPartition2.Add(new TestObject(GetRandomAabb()));

      _partition2 = new AabbTree<int>();
      _partition2.GetAabbForItem = GetAabbOfTestObjectOfPartition2;
      _partition2.Add(0);
      _partition2.Add(1);
      _partition2.Add(2);
      _partition2.Add(3);

      _conservativeAabb = false;
      _conservativeOverlaps = false;
    }


    [Test]
    public void TestDebugSpatialPartition()
    {
      TestPartition(new DebugSpatialPartition<int> { GetAabbForItem = GetAabbOfTestObject });
    }


    [Test]
    public void TestAabbTree()
    {
      TestPartition(new AabbTree<int> { GetAabbForItem = GetAabbOfTestObject });
    }


    [Test]
    public void TestCompressedAabbTree()
    {
      _conservativeAabb = true;
      _conservativeOverlaps = true;
      TestPartition(new CompressedAabbTree { GetAabbForItem = GetAabbOfTestObject });
    }


    [Test]
    public void TestSAP()
    {
      TestPartition(new SweepAndPruneSpace<int> { GetAabbForItem = GetAabbOfTestObject });
    }


    [Test]
    public void TestDynamicAabbTreeWithoutMotionPrediction()
    {
      _conservativeAabb = true;
      _conservativeOverlaps = true;
      TestPartition(new DynamicAabbTree<int> { GetAabbForItem = GetAabbOfTestObject });
    }


    [Test]
    public void TestDynamicAabbTreeWithMotionPrediction()
    {
      _conservativeAabb = true;
      _conservativeOverlaps = true;
      TestPartition(new DynamicAabbTree<int> { GetAabbForItem = GetAabbOfTestObject, EnableMotionPrediction = true });
    }


    [Test]
    public void TestDualPartition()
    {
      _conservativeAabb = true;
      _conservativeOverlaps = true;
      TestPartition(new DualPartition<int> { GetAabbForItem = GetAabbOfTestObject });
    }


    [Test]
    public void TestAdaptiveAabbTree()
    {
      TestPartition(new AdaptiveAabbTree<int> { GetAabbForItem = GetAabbOfTestObject });
    }


    private void TestPartition(ISpatialPartition<int> partition)
    {
      partition.Clear();
      Assert.AreEqual(0, partition.Count);

      partition.EnableSelfOverlaps = true;
      Assert.AreEqual(0, partition.GetOverlaps().Count());
      Assert.AreEqual(0, partition.GetOverlaps(0).Count());
      Assert.AreEqual(0, partition.GetOverlaps(new Aabb()).Count());
      Assert.AreEqual(0, partition.GetOverlaps(_partition2).Count());
      Assert.AreEqual(0, partition.GetOverlaps(Vector3F.One, Pose.Identity, _partition2, Vector3F.One, Pose.Identity).Count());


      var testObject = new TestObject(new Aabb(new Vector3F(10), new Vector3F(10)));
      _testObjects.Add(testObject);
      partition.Add(testObject.Id);

      for (int i = 0; i < 1000; i++)
      {
        // ----- Tests        
        Assert.AreEqual(_testObjects.Count, partition.Count, "Wrong number of items.");

        if (i > 10 && i % 6 == 0)
          TestGetOverlaps0(partition);
        if (i > 10 && i % 6 == 1)
          TestGetOverlaps1(partition);
        if (i > 10 && i % 6 == 2)
          TestGetOverlaps2(partition);
        if (i > 10 && i % 6 == 3)
          TestGetOverlaps3(partition);
        if (i > 10 && i % 6 == 4)
          TestGetOverlaps4(partition);
        if (i > 10 && i % 6 == 5)
          TestGetOverlaps5(partition);

        // Update partition. From time to time rebuild all.
        // For the above tests update should have been called automatically!
        partition.Update(i % 10 == 9);
        TestAabb(partition);

        var dice100 = RandomHelper.Random.Next(0, 100);
        if (dice100 < 2)
        {
          // Test remove/re-add without Update inbetween.
          if (partition.Count > 0)
          {
            partition.Remove(_testObjects[0].Id);
            partition.Add(_testObjects[0].Id);
          }
        }


        dice100 = RandomHelper.Random.Next(0, 100);
        if (dice100 < 10)
        {
          // Remove objects.
          int removeCount = RandomHelper.Random.NextInteger(1, 4);
          for (int k = 0; k < removeCount && partition.Count > 0; k++)
          {
            var index = RandomHelper.Random.NextInteger(0, partition.Count - 1);
            var obj = _testObjects[index];
            _testObjects.Remove(obj);
            partition.Remove(obj.Id);
          }
        }

        dice100 = RandomHelper.Random.Next(0, 100);
        if (dice100 < 10)
        {
          // Add new objects.
          int addCount = RandomHelper.Random.NextInteger(1, 4);
          for (int k = 0; k < addCount; k++)
          {
            var newObj = new TestObject(GetRandomAabb());
            _testObjects.Add(newObj);
            partition.Add(newObj.Id);
          }
        }
        else
        {
          // Move an object.
          int moveCount = RandomHelper.Random.NextInteger(1, 10);
          for (int k = 0; k < moveCount && partition.Count > 0; k++)
          {
            var index = RandomHelper.Random.NextInteger(0, partition.Count - 1);
            var obj = _testObjects[index];
            obj.Aabb = GetRandomAabb();
            partition.Invalidate(obj.Id);
          }
        }

        // From time to time invalidate all.
        if (dice100 < 3)
          partition.Invalidate();

        // From time to time change EnableSelfOverlaps.
        if (dice100 > 3 && dice100 < 6)
          partition.EnableSelfOverlaps = false;
        else if (dice100 < 10)
          partition.EnableSelfOverlaps = true;

        // From time to time change filter.
        if (dice100 > 10 && dice100 < 13)
        {
          partition.Filter = null;
        }
        else if (dice100 < 10)
        {
          if (partition.Filter == null)
            partition.Filter = new DelegatePairFilter<int>(AreInSameGroup);
        }
      }

      partition.Clear();
      Assert.AreEqual(0, partition.Count);
    }


    private Aabb GetRandomAabb()
    {
      var point = RandomHelper.Random.NextVector3F(0, 100);
      var point2 = RandomHelper.Random.NextVector3F(0, 100);
      var newAabb = new Aabb(point, point);
      newAabb.Grow(point2);
      return newAabb;
    }


    private void TestAabb(ISpatialPartition<int> partition)
    {
      if (_testObjects.Count == 0)
        return;

      // Compute desired result.
      var desiredAabb = _testObjects[0].Aabb;
      _testObjects.ForEach(obj => desiredAabb.Grow(obj.Aabb));

      // The AABB of the spatial partition can be slightly bigger.
      // E.g. the CompressedAabbTree adds a margin to avoid divisions by zero.
      Assert.IsTrue(Numeric.IsFinite(partition.Aabb.Minimum.X));
      Assert.IsTrue(Numeric.IsFinite(partition.Aabb.Minimum.Y));
      Assert.IsTrue(Numeric.IsFinite(partition.Aabb.Minimum.Z));
      Assert.IsTrue(Numeric.IsFinite(partition.Aabb.Maximum.X));
      Assert.IsTrue(Numeric.IsFinite(partition.Aabb.Maximum.Y));
      Assert.IsTrue(Numeric.IsFinite(partition.Aabb.Maximum.Z));

      if (_conservativeAabb)
      {
        // AABB can be bigger than actual objects.
        Assert.IsTrue(partition.Aabb.Contains(desiredAabb), "Wrong AABB: AABB is too small.");
      }
      else
      {
        // The AABB should be identical.
        Assert.IsTrue(Vector3F.AreNumericallyEqual(desiredAabb.Minimum, partition.Aabb.Minimum));
        Assert.IsTrue(Vector3F.AreNumericallyEqual(desiredAabb.Maximum, partition.Aabb.Maximum));        
      }
    }


    private void TestGetOverlaps0(ISpatialPartition<int> partition)
    {
      var aabb = GetRandomAabb();

      // Compute desired result.
      var desiredResults = new List<int>();
      foreach (var testObject in _testObjects)
      {
        if (GeometryHelper.HaveContact(aabb, testObject.Aabb))
          desiredResults.Add(testObject.Id);
      }

      var results = partition.GetOverlaps(aabb).ToList();
      CompareResults(desiredResults, results, "GetOverlaps(Aabb) returns different number of results.");
    }


    private void TestGetOverlaps1(ISpatialPartition<int> partition)
    {
      // Temporarily add random test object.
      var randomTestObject = new TestObject(GetRandomAabb());
      _testObjects.Add(randomTestObject);

      // Compute desired result.
      var desiredResults = new List<int>();
      foreach (var testObject in _testObjects)
      {
        if (testObject == randomTestObject)
          continue;

        if (partition.Filter == null || partition.Filter.Filter(new Pair<int>(randomTestObject.Id, testObject.Id)))
          if (GeometryHelper.HaveContact(randomTestObject.Aabb, testObject.Aabb))
            desiredResults.Add(testObject.Id);
      }

      var results = partition.GetOverlaps(randomTestObject.Id).ToList();
      CompareResults(desiredResults, results, "GetOverlaps(T) returns different number of results.");

      _testObjects.Remove(randomTestObject);
    }


    private void TestGetOverlaps2(ISpatialPartition<int> partition)
    {
      var aabb = GetRandomAabb();
      var ray = new Ray(aabb.Minimum, aabb.Extent.Normalized, aabb.Extent.Length);

      ray.Direction = RandomHelper.Random.NextVector3F(-1, 1).Normalized;

      // Compute desired result.
      var desiredResults = new List<int>();
      foreach (var testObject in _testObjects)
      {
        if (GeometryHelper.HaveContact(testObject.Aabb, ray))
          desiredResults.Add(testObject.Id);
      }

      var results = partition.GetOverlaps(ray).ToList();
      CompareResults(desiredResults, results, "GetOverlaps(Ray) returns different number of results.");
    }


    private void TestGetOverlaps3(ISpatialPartition<int> partition)
    {
      if (!partition.EnableSelfOverlaps)
        return;

      // Compute desired result.
      var desiredResults = new List<Pair<int>>();
      for (int i = 0; i < _testObjects.Count; i++)
      {
        var a = _testObjects[i];
        for (int j = i + 1; j < _testObjects.Count; j++)
        {
          var b = _testObjects[j];
          if (a != b)
            if (partition.Filter == null || partition.Filter.Filter(new Pair<int>(a.Id, b.Id)))
              if (GeometryHelper.HaveContact(a.Aabb, b.Aabb))
                desiredResults.Add(new Pair<int>(a.Id, b.Id));
        }
      }

      var results = partition.GetOverlaps().ToList();

      if (desiredResults.Count != results.Count)
      {
        var distinct = results.Except(desiredResults).ToList();
      }

      CompareResults(desiredResults, results, "GetOverlaps() returns different number of results.");
    }


    private void TestGetOverlaps4(ISpatialPartition<int> partition)
    {
      // Compute desired result.
      var desiredResults = new List<Pair<int>>();
      foreach (var a in _testObjects)
      {
        foreach (var b in _testObjectsOfPartition2)
        {
          if (partition.Filter == null || partition.Filter.Filter(new Pair<int>(a.Id, b.Id)))
            if (GeometryHelper.HaveContact(a.Aabb, b.Aabb))
              desiredResults.Add(new Pair<int>(a.Id, b.Id));
        }
      }

      var results = partition.GetOverlaps(_partition2).ToList();
      CompareResults(desiredResults, results, "GetOverlaps(Partition) returns different number of results.");
    }


    private void TestGetOverlaps5(ISpatialPartition<int> partition)
    {
      // Get random pose for _partition2
      var pose = new Pose(GetRandomAabb().Center, RandomHelper.Random.NextQuaternionF());
      var scale = RandomHelper.Random.NextVector3F(0.1f, 3f);

      // Compute desired result.
      var desiredResults = new List<Pair<int>>();
      foreach (var a in _testObjects)
      {
        foreach (var b in _testObjectsOfPartition2)
        {
          if (partition.Filter == null || partition.Filter.Filter(new Pair<int>(a.Id, b.Id)))
          {
            var aabbB = b.Aabb;
            aabbB.Scale(scale);
            var boxB = aabbB.Extent;
            var poseB = pose * new Pose(aabbB.Center);

            if (GeometryHelper.HaveContact(a.Aabb, boxB, poseB, true))
              desiredResults.Add(new Pair<int>(a.Id, b.Id));
          }
        }
      }

      var results = partition.GetOverlaps(Vector3F.One, Pose.Identity, _partition2, scale, pose).ToList();

      if (desiredResults.Count > results.Count)
        Debugger.Break();

      CompareResults(desiredResults, results, "GetOverlaps(Partition, Pose, Scale) returns a wrong number of results or has missed an overlap.");
    }


    private void CompareResults(List<int> expected, List<int> actual, string message)
    {
      // The spatial partition must have computed all desired overlaps. It is ok if it has computed 
      // a bit more. (Some partitions are more conservative.)
      if (_conservativeOverlaps)
      {
        Assert.LessOrEqual(expected.Count, actual.Count, message);
      }
      else
      {
        Assert.LessOrEqual(expected.Count, actual.Count, message);
        Assert.LessOrEqual(actual.Count - expected.Count, 3);
      }

      expected.ForEach(id => Assert.IsTrue(actual.Contains(id), message));
    }


    private void CompareResults(List<Pair<int>> expected, List<Pair<int>> actual, string message)
    {
      // The spatial partition must have computed all desired overlaps. It is ok if it has computed 
      // a  bit more. (Some partitions are more conservative.)
      if (_conservativeOverlaps)
      {
        Assert.LessOrEqual(expected.Count, actual.Count, message);
      }
      else
      {
        Assert.LessOrEqual(expected.Count, actual.Count, message);
        Assert.LessOrEqual(actual.Count - expected.Count, 3);
      }

      expected.ForEach(pair => Assert.IsTrue(actual.Contains(pair), message));
    }
  }
}
