using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Tests
{
  [TestFixture]
  public class CollisionDetectionBroadPhaseTest
  {
    [Test]
    public void Test1()
    {
      // All objects touch.
      CollisionObject a = new CollisionObject { GeometricObject = new GeometricObject { Shape = new SphereShape(1) } };
      CollisionObject b = new CollisionObject { GeometricObject = new GeometricObject { Shape = new SphereShape(1) } };
      CollisionObject c = new CollisionObject { GeometricObject = new GeometricObject { Shape = new SphereShape(1) } };
      CollisionObject d = new CollisionObject { GeometricObject = new GeometricObject { Shape = new SphereShape(1) } };

      CollisionDomain domain = new CollisionDomain(new CollisionDetection());
      Assert.AreEqual(0, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.InternalBroadPhase.Update();
      Assert.AreEqual(0, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Add(a);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(0, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Add(b);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(1, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Add(c);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(3, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Add(d);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(6, domain.InternalBroadPhase.CandidatePairs.Count);

      foreach (ContactSet set in domain.InternalBroadPhase.CandidatePairs)
        Assert.AreNotEqual(set.ObjectA, set.ObjectB);

      domain.CollisionObjects.Remove(b);
      domain.CollisionObjects.Remove(b);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(3, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Remove(a);
      domain.CollisionObjects.Remove(a);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(1, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Remove(d);
      domain.CollisionObjects.Remove(d);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(0, domain.InternalBroadPhase.CandidatePairs.Count);
    }


    [Test]
    public void Test2()
    {
      // All objects touch.
      CollisionObject a = new CollisionObject(new GeometricObject(new SphereShape(1), new Pose(new Vector3F(0, 0, 0))));
      CollisionObject b = new CollisionObject(new GeometricObject(new SphereShape(1), new Pose(new Vector3F(1, 1, 0))));
      CollisionObject c = new CollisionObject(new GeometricObject(new SphereShape(1), new Pose(new Vector3F(10, 10, 10))));
      CollisionObject d = new CollisionObject(new GeometricObject(new SphereShape(1), new Pose(new Vector3F(0, 0, -1))));

      CollisionDomain domain = new CollisionDomain(new CollisionDetection());
      Assert.AreEqual(0, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.InternalBroadPhase.Update();
      Assert.AreEqual(0, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Add(a);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(0, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Add(b);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(1, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Add(c);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(1, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Add(d);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(3, domain.InternalBroadPhase.CandidatePairs.Count);

      foreach (ContactSet set in domain.InternalBroadPhase.CandidatePairs)
        Assert.AreNotEqual(set.ObjectA, set.ObjectB);

      domain.CollisionObjects.Remove(b);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(1, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Remove(a);
      domain.CollisionObjects.Remove(a);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(0, domain.InternalBroadPhase.CandidatePairs.Count);

      domain.CollisionObjects.Remove(d);
      domain.CollisionObjects.Remove(d);
      domain.InternalBroadPhase.Update();
      Assert.AreEqual(0, domain.InternalBroadPhase.CandidatePairs.Count);
    }


    [Test]
    public void Test3()
    {
      CollisionDomain domain = new CollisionDomain(new CollisionDetection());

      RandomHelper.Random = new Random(123456);

      const float testAreaSize = 20;

      // Add 100 random objects.
      for (int i = 0; i < 100; i++)
        domain.CollisionObjects.Add(new CollisionObject(new GeometricObject(new SphereShape(1), new Pose(RandomHelper.Random.NextVector3F(0, testAreaSize)))));

      for (int i = 0; i < 100; i++)
      {
        domain.Update(0.03f);

        for (int j = 0; j < domain.CollisionObjects.Count; j++)
        {
          var a = domain.CollisionObjects[j];

          for (int k = j + 1; k < domain.CollisionObjects.Count; k++)
          {
            var b = domain.CollisionObjects[k];

            bool contained = domain.InternalBroadPhase.CandidatePairs.Contains(a, b);
            bool haveContact = GeometryHelper.HaveContact(a.GeometricObject.Aabb, b.GeometricObject.Aabb);
            //if (contained != haveContact)
              //Debugger.Break();
            Assert.AreEqual(contained, haveContact);
          }

          // Set new random position for a few.
          if (RandomHelper.Random.NextFloat(0, 1) < 0.7f)
            ((GeometricObject)a.GeometricObject).Pose = new Pose(RandomHelper.Random.NextVector3F(0, testAreaSize));
        }

        // Add new object.
        domain.CollisionObjects.Add(new CollisionObject 
        {
          GeometricObject = new GeometricObject 
          { 
            Shape = new SphereShape(1),
            Pose = new Pose(RandomHelper.Random.NextVector3F(0, testAreaSize)),
          }
        });
        
        // Remove random object.
        domain.CollisionObjects.Remove(domain.CollisionObjects[RandomHelper.Random.NextInteger(0, domain.CollisionObjects.Count - 1)]);

        Console.WriteLine("Candidate pairs: " + domain.InternalBroadPhase.CandidatePairs.Count);
      }
    }

    [Test]
    public void UpdateSingle()
    {
      CollisionDomain domain = new CollisionDomain(new CollisionDetection());

      // Add 100 random objects.
      for (int i = 0; i < 100; i++)
        domain.CollisionObjects.Add(new CollisionObject(new GeometricObject(new SphereShape(1), new Pose(RandomHelper.Random.NextVector3F(0, 20)))));

      for (int i = 0; i < 100; i++)
      {
        domain.Update(domain.CollisionObjects[33]);

        for (int j = 0; j < domain.CollisionObjects.Count; j++)
        {
          var a = domain.CollisionObjects[j];

          for (int k = j + 1; k < domain.CollisionObjects.Count; k++)
          {
            var b = domain.CollisionObjects[k];

            Assert.AreEqual(domain.InternalBroadPhase.CandidatePairs.Contains(a, b),
                            GeometryHelper.HaveContact(a.GeometricObject.Aabb, b.GeometricObject.Aabb));
          }
        }

        // Set new random position for one.
        ((GeometricObject)domain.CollisionObjects[33].GeometricObject).Pose = new Pose(RandomHelper.Random.NextVector3F(0, 20));

        Console.WriteLine("Candidate pairs: " + domain.InternalBroadPhase.CandidatePairs.Count);
      }
    }
  }
}
