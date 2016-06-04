using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Ray = DigitalRune.Geometry.Shapes.Ray;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"This sample shows how to use a spatial partition to make fast overlap tests.",
    @"Several collision objects are added to an axis-aligned bounding box (AABB) tree. The AABB tree is one 
of several spatial types. It can be used to make fast overlap tests. For example you can query which 
objects overlap an AABB or a ray.
In this sample the AABB tree is used to make ray tests for mouse picking. This sample is similar to the
02-PickingSample. However, the spatial partition is used directly instead of a collision domain - this can
be faster in certain cases.",
    13)]
  public class SpatialPartitionSample : BasicSample
  {
    private ISpatialPartition<GeometricObject> _spatialPartition;


    public SpatialPartitionSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.Gray;
      GraphicsScreen.DrawReticle = true;
      SetCamera(new Vector3F(0, 1, 10), 0, 0);

      // Create a spatial partition. DigitalRune Geometry supports several types, see also
      // http://digitalrune.github.io/DigitalRune-Documentation/html/e32cab3b-cc7c-42ee-8ec9-23dd4467edd0.htm#WhichPartition
      // An AabbTree is useful for static objects. A DynamicAabbTree is good for moving objects.
      // The spatial partition can manage different types of items. In this case it manages
      // GeometricObjects. A delegate has to inform the spatial partition how to get the AABB
      // of an object.
      //_spatialPartition = new DynamicAabbTree<GeometricObject>
      _spatialPartition = new AabbTree<GeometricObject>
      {
        GetAabbForItem = geometricObject => geometricObject.Aabb,

        // Optional: The tree is automatically built using a mixed top-down/bottom-up approach. 
        // Bottom-up building is slower but produces better trees. If the tree building takes too 
        // long, we can lower the BottomUpBuildThreshold (default is 128).
        //BottomUpBuildThreshold = 0,

        // Optional: A filter can be set to disable certain kind of overlaps.
        //Filter = ...
      };

      // Create a triangle mesh.
      var triangleMesh = new SphereShape(1).GetMesh(0.01f, 4);
      var triangleMeshShape = new TriangleMeshShape(triangleMesh)
      {
        // TriangleMeshShapes can also use a spatial partition to manage triangle.
        // The items in the spatial partition are the triangle indices. The GetAabbForItem
        // delegate is set automatically.
        Partition = new AabbTree<int>(),
      };

      // Spatial partitions are built automatically when needed. However, it is still recommended
      // to call Update to initialize the spatial partition explicitly.
      triangleMeshShape.Partition.Update(false);

      // Add a lot of triangle mesh objects to _spatialPartition.
      var random = new Random();
      for (int i = 0; i < 50; i++)
      {
        var randomPosition = new Vector3F(random.NextFloat(-6, 6), random.NextFloat(-3, 3), random.NextFloat(-10, 0));
        var geometricObject = new GeometricObject(triangleMeshShape, new Pose(randomPosition));
        _spatialPartition.Add(geometricObject);
      }

      _spatialPartition.Update(false);
    }


    public override void Update(GameTime gameTime)
    {
      // If an item in a spatial partition changes (e.g. if it moves or if the AABB changes) you 
      // have to call
      //_spatialPartition.Invalidate(changedGeometryobject);
      // If many or all items have changed you can call 
      //_spatialPartition.Invalidate();
      // After an item was invalidated, the spatial partition needs to be rebuilt. This happens
      // automatically when needed or when you call
      //_spatialPartition.Update(forceRebuild: false);

      // Get a ray which shoots forward.
      var cameraPose = GraphicsScreen.CameraNode.PoseWorld;
      var ray = new Ray
      {
        Origin = cameraPose.Position,
        Direction = cameraPose.ToWorldDirection(Vector3F.Forward),
        Length = 100,
      };

      // Draw objects.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      foreach (var geometricObject in _spatialPartition)
        debugRenderer.DrawObject(geometricObject, Color.LightGreen, false, false);

      GeometricObject closestHitGeometricObject = null;
      Triangle closestHitTriangle = new Triangle();
      float closestHitDistance = float.PositiveInfinity;

      // Use the spatial partition to get all objects where the AABB overlaps the ray.
      foreach (var geometricObject in _spatialPartition.GetOverlaps(ray))
      {
        var triangleMeshShape = (TriangleMeshShape)geometricObject.Shape;

        // Transform the ray into the local space of the triangle mesh.
        var localRay = new Ray
        {
          Origin = geometricObject.Pose.ToLocalPosition(ray.Origin),
          Direction = geometricObject.Pose.ToLocalDirection(ray.Direction),
          Length = ray.Length,
        };

        // Use the spatial partition of the mesh shape to compute all triangles where the 
        // AABB overlaps the ray.
        foreach (var triangleIndex in triangleMeshShape.Partition.GetOverlaps(localRay))
        {
          var triangle = triangleMeshShape.Mesh.GetTriangle(triangleIndex);

          // Check if ray intersects the triangle and remember the closest hit.
          float hitDistance;
          if (GeometryHelper.GetContact(localRay, triangle, false, out hitDistance)
              && hitDistance < closestHitDistance)
          {
            closestHitGeometricObject = geometricObject;
            closestHitTriangle = triangle;
            closestHitDistance = hitDistance;
          }
        }
      }

      // Draw hit triangle.
      if (closestHitGeometricObject != null)
        debugRenderer.DrawTriangle(closestHitTriangle, closestHitGeometricObject.Pose, Vector3F.One, Color.Red, true, true);
    }
  }
}
