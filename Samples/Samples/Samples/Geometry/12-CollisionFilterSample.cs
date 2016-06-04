using System;
using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"This sample shows how to use a broad phase or a narrow phase collision filter to control which
objects can intersect",
    @"Objects are added to a collision domain. A filter is used to disable contact computations between
objects of the same color.",
    12)]
  public class CollisionFilterSample : BasicSample
  {
    public CollisionFilterSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.Gray;
      GraphicsScreen.DrawReticle = true;
      SetCamera(new Vector3F(0, 0, 10), 0, 0);

      // ----- Initialize collision detection system.
      // We use one collision domain that manages all objects.
      var domain = new CollisionDomain();

      // Let's set a filter which disables collision between object in the same collision group.
      // We can use a broad phase or a narrow phase filter:

      // Option A) Broad phase filter
      // The collision detection broad phase computes bounding box overlaps. 
      // A broad phase filter is best used if the filtering rules are simple and do not change 
      // during the runtime of your application.
      //domain.BroadPhase.Filter = new DelegatePairFilter<CollisionObject>(
      //  pair => pair.First.CollisionGroup != pair.Second.CollisionGroup);

      // Option B) Narrow phase filter
      // The collision detection narrow phase computes contacts between objects where the broad
      // phase has detected a bounding box overlap. Use a narrow phase filter if the filtering rules 
      // are complex or can change during the runtime of your application.
      var filter = new CollisionFilter();
      // Disable collision between objects in the same groups.
      filter.Set(0, 0, false);
      filter.Set(1, 1, false);
      filter.Set(2, 2, false);
      filter.Set(3, 3, false);
      domain.CollisionDetection.CollisionFilter = filter;

      // Create a random list of points.
      var points = new List<Vector3F>();
      for (int i = 0; i < 100; i++)
        points.Add(RandomHelper.Random.NextVector3F(-1.5f, 1.5f));

      // Add lots of spheres to the collision domain. Assign spheres to different collision groups.
      var random = new Random();
      var sphereShape = new SphereShape(0.7f);
      for (int i = 0; i < 20; i++)
      {
        var randomPosition = new Vector3F(random.NextFloat(-6, 6), random.NextFloat(-3, 3), 0);
        var geometricObject = new GeometricObject(sphereShape, new Pose(randomPosition));
        var collisionObject = new CollisionObject(geometricObject)
        {
          // A collision group is simply an integer. We can assign collision objects to collision
          // groups to control the collision filtering.
          CollisionGroup = random.NextInteger(0, 3),
        };
        domain.CollisionObjects.Add(collisionObject);
      }

      // Compute collisions. (The objects do not move in this sample. Therefore, we only have to 
      // call Update once.)
      domain.Update(0);

      // Draw objects. The color depends on the collision group.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      foreach (var collisionObject in domain.CollisionObjects)
      {
        Color color;
        switch (collisionObject.CollisionGroup)
        {
          case 0: color = Color.LightBlue; break;
          case 1: color = Color.Yellow; break;
          case 2: color = Color.Orange; break;
          default: color = Color.LightGreen; break;
        }

        debugRenderer.DrawObject(collisionObject.GeometricObject, color, false, false);
      }

      debugRenderer.DrawContacts(domain.ContactSets, 0.1f, Color.Red, true);
    }
  }
}
