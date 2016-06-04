using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;


namespace Samples.Geometry
{
  // This class creates several game objects to create a level with interesting obstacles to
  // test the character controller. (Here the game objects are hard-coded. A real game would
  // load the level from a file.)
  public static class CharacterControllerLevel
  {
    public static void Load(CollisionDomain collisionDomain)
    {
      // Create a box for the ground.
      AddObject("Ground", new Pose(new Vector3F(0, -5, 0)), new BoxShape(60, 10, 60), collisionDomain);

      // Create a small flying sphere to visualize the approx. head height. - This is just
      // for debugging so that we have a feeling for heights.
      AddObject("Sphere", new Pose(new Vector3F(0, 1.5f, 0)), new SphereShape(0.2f), collisionDomain);

      // Create small walls at the level boundary.
      AddObject("WallLeft", new Pose(new Vector3F(-30, 1, 0)), new BoxShape(0.3f, 2, 60), collisionDomain);
      AddObject("WallRight", new Pose(new Vector3F(30, 1, 0)), new BoxShape(0.3f, 2, 60), collisionDomain);
      AddObject("WallFront", new Pose(new Vector3F(0, 1, -30)), new BoxShape(60, 2, 0.3f), collisionDomain);
      AddObject("WallBack", new Pose(new Vector3F(0, 1, 30)), new BoxShape(60, 2, 0.3f), collisionDomain);

      // Create a few bigger objects.
      // We position the boxes so that we have a few corners we can run into. Character controllers
      // should be stable when the user runs into corners.
      AddObject("House0", new Pose(new Vector3F(10, 1, -10)), new BoxShape(8, 2, 8f), collisionDomain);
      AddObject("House1", new Pose(new Vector3F(13, 1, -4)), new BoxShape(2, 2, 4), collisionDomain);
      AddObject("House2", new Pose(new Vector3F(10, 2, -15), Matrix33F.CreateRotationY(-0.3f)), new BoxShape(8, 4, 2), collisionDomain);

      //
      // Create stairs with increasing step height.
      //
      // Each step is a box. With this object we can test if our character can climb up
      // stairs. The character controller has a step height limit. Increasing step heights
      // let us test if the step height limit works.
      float startHeight = 0;
      const float stepDepth = 1f;
      for (int i = 0; i < 10; i++)
      {
        float stepHeight = 0.1f + i * 0.05f;
        Pose pose = new Pose(new Vector3F(0, startHeight + stepHeight / 2, -2 - i * stepDepth));
        BoxShape shape = new BoxShape(2, stepHeight, stepDepth);
        AddObject("Step" + i, pose, shape, collisionDomain);
        startHeight += stepHeight;
      }

      //
      // Create a height field.
      //
      // Terrain that is uneven is best modeled with a height field. Height fields are faster
      // than general triangle meshes.
      // The height direction is the y direction. 
      // The height field lies in the x/z plane.
      var numberOfSamplesX = 20;
      var numberOfSamplesZ = 20;
      var samples = new float[numberOfSamplesX * numberOfSamplesZ];
      // Create arbitrary height values. 
      for (int z = 0; z < numberOfSamplesZ; z++)
      {
        for (int x = 0; x < numberOfSamplesX; x++)
        {
          if (x == 0 || z == 0 || x == 19 || z == 19)
          {
            // Set this boundary elements to a low height, so that the height field is connected
            // to the ground.
            samples[z * numberOfSamplesX + x] = -1;
          }
          else
          {
            // A sine/cosine function that creates some interesting waves.
            samples[z * numberOfSamplesX + x] = 1.0f + (float)(Math.Cos(z / 2f) * Math.Sin(x / 2f) * 1.0f);
          }
        }
      }
      var heightField = new HeightField(0, 0, 20, 20, samples, numberOfSamplesX, numberOfSamplesZ);
      AddObject("Heightfield", new Pose(new Vector3F(10, 0, 10)), heightField, collisionDomain);

      // Create rubble on the floor (small random objects on the floor).
      // Our character should be able to move over small bumps on the ground.
      for (int i = 0; i < 50; i++)
      {
        Pose pose = new Pose(
          new Vector3F(RandomHelper.Random.NextFloat(-5, 5), 0, RandomHelper.Random.NextFloat(10, 20)),
          RandomHelper.Random.NextQuaternionF());
        BoxShape shape = new BoxShape(RandomHelper.Random.NextVector3F(0.05f, 0.8f));
        AddObject("Stone" + i, pose, shape, collisionDomain);
      }

      // Create some slopes to see how our character performs on/under sloped surfaces.
      // Here we can test how the character controller behaves if the head touches an inclined
      // ceiling.
      AddObject("SlopeGround", new Pose(new Vector3F(-2, 1.8f, -12), QuaternionF.CreateRotationX(0.4f)), new BoxShape(2, 0.5f, 10), collisionDomain);
      AddObject("SlopeRoof", new Pose(new Vector3F(-2, 5.6f, -12), QuaternionF.CreateRotationX(-0.4f)), new BoxShape(2, 0.5f, 10), collisionDomain);

      // Slopes with different tilt angles.
      // The character controller has a slope limit. Only flat slopes should be climbable. 
      for (int i = 0; i < 10; i++)
      {
        float stepHeight = 0.1f + i * 0.1f;
        Pose pose = new Pose(
          new Vector3F(-10, i * 0.5f, -i * 2),
          Matrix33F.CreateRotationX(MathHelper.ToRadians(10) + i * MathHelper.ToRadians(10)));
        BoxShape shape = new BoxShape(8 * (1 - i * 0.1f), 0.5f, 30);
        AddObject("Slope" + i, pose, shape, collisionDomain);
        startHeight += stepHeight;
      }

      // Create a slope with a wall on one side.
      // This objects let's us test how the character controller behaves while falling and
      // sliding along a vertical wall. (Run up the slope and then jump down while moving into
      // the wall.)
      AddObject("LongSlope", new Pose(new Vector3F(-20, 3, -10), Matrix33F.CreateRotationX(0.4f)), new BoxShape(4, 5f, 30), collisionDomain);
      AddObject("LongSlopeWall", new Pose(new Vector3F(-22, 5, -10)), new BoxShape(0.5f, 10f, 25), collisionDomain);

      // Create a mesh object to test walking on triangle meshes.
      // Normally, the mesh would be loaded from a file. Here, we make a composite shape and 
      // let DigitalRune Geometry compute a mesh for it. Then we throw away the composite
      // shape and use only the mesh. - We do this to test triangle meshes. Using the composite
      // shape instead of the triangle mesh would be a lot faster.
      CompositeShape compositeShape = new CompositeShape();
      compositeShape.Children.Add(new GeometricObject(heightField, Pose.Identity));
      compositeShape.Children.Add(new GeometricObject(new CylinderShape(1, 2), new Pose(new Vector3F(10, 1, 10))));
      compositeShape.Children.Add(new GeometricObject(new SphereShape(3), new Pose(new Vector3F(15, 0, 15))));
      compositeShape.Children.Add(new GeometricObject(new BoxShape(1, 2, 3), new Pose(new Vector3F(15, 0, 5))));
      ITriangleMesh mesh = compositeShape.GetMesh(0.01f, 3);
      TriangleMeshShape meshShape = new TriangleMeshShape(mesh);

      // Collision detection speed for triangle meshes can be improved by using a spatial 
      // partition. Here, we assign an AabbTree to the triangle mesh shape. The tree is
      // built automatically when needed and it stores triangle indices (therefore the generic 
      // parameter of the AabbTree is int).
      meshShape.Partition = new AabbTree<int>()
      {
        // The tree is automatically built using a mixed top-down/bottom-up approach. Bottom-up
        // building is slower but produces better trees. If the tree building takes too long,
        // we can lower the BottomUpBuildThreshold (default is 128).
        BottomUpBuildThreshold = 0,
      };

      AddObject("Mesh", new Pose(new Vector3F(-30, 0, 10)), meshShape, collisionDomain);
    }


    // Adds a game object and adds a collision object to the collision domain.
    private static void AddObject(string name, Pose pose, Shape shape, CollisionDomain collisionDomain)
    {
      // Create game object.
      var geometricObject = new GeometricObject(shape, pose);

      // Create collision object.
      var collisionObject = new CollisionObject(geometricObject)
      {
        CollisionGroup = 1,
      };

      // Add collision object to collision domain.
      collisionDomain.CollisionObjects.Add(collisionObject);
    }
  }
}
