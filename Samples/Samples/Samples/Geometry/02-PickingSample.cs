using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"This sample shows how to use ray casting to pick objects under the mouse cursor.",
    @"3 objects are drawn: A sphere, a box and a triangle-mesh (convex hull of random points).
A raycast is performed under the reticle/mouse cursor in viewing direction. Objects that
are hit by the ray are drawn in a different color.
Per default the object under reticle is picked. Press <Ctrl> to show mouse cursor and
pick object using the mouse cursor.",
    2)]
  [Controls(@"Sample
  Hold <Ctrl> to show mouse cursor.")]
  public class PickingSample : BasicSample
  {
    // The collision domain that manages collision objects.
    private CollisionDomain _domain;

    // A few collision objects.
    private CollisionObject _box;
    private CollisionObject _sphere;
    private CollisionObject _mesh;

    // A ray object used for picking.
    private CollisionObject _ray;


    public PickingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      GraphicsScreen.DrawReticle = true;
      SetCamera(new Vector3F(0, 1, 10), 0, 0);

      // ----- Initialize collision detection system.
      // We use one collision domain that manages all objects.
      _domain = new CollisionDomain
      {
        // Optional: Change the broad phase type. The default type is the SweepAndPruneSpace, 
        // which is very fast for physics simulation. The DualPartition is better for ray casts.
        // See also http://digitalrune.github.io/DigitalRune-Documentation/html/e32cab3b-cc7c-42ee-8ec9-23dd4467edd0.htm#WhichPartition
        BroadPhase = new DualPartition<CollisionObject>(),
      };

      // Optional: Set a broad phase filter.
      // Per default, the collision domain computes contacts between all collision objects. If we
      // are only interested in ray vs non-ray-shape contacts, we can set a filter to avoid 
      // unnecessary intersection computations and improve performance.
      _domain.BroadPhase.Filter = new DelegatePairFilter<CollisionObject>(
        pair =>
        {
          var firstIsRay = pair.First.GeometricObject.Shape is RayShape;
          var secondIsRay = pair.Second.GeometricObject.Shape is RayShape;
          return firstIsRay != secondIsRay;
        });

      // Create a collision object with a box shape at position (0, 0, 0) with a random rotation.
      _box = new CollisionObject(
        new GeometricObject(
          new BoxShape(1, 2, 3),
          new Pose(new Vector3F(0, 0, 0), RandomHelper.Random.NextQuaternionF())));

      // Create a collision object with a sphere shape at position (-5, 0, 0).
      _sphere = new CollisionObject(new GeometricObject(new SphereShape(1), new Pose(new Vector3F(-5, 0, 0))));

      // Create a random list of points.
      var points = new List<Vector3F>();
      for (int i = 0; i < 100; i++)
        points.Add(RandomHelper.Random.NextVector3F(-1.5f, 1.5f));

      // Create a triangle mesh of the convex hull.
      // (See also the ConvexHullSample for info on convex hull creation.)
      TriangleMesh triangleMesh = GeometryHelper.CreateConvexHull(points).ToTriangleMesh();

      // We use this random triangle mesh to define a shape.
      TriangleMeshShape meshShape = new TriangleMeshShape(triangleMesh);

      // Optional: We can use a spatial partitioning method, to speed up collision
      // detection for large meshes. AABB trees are good for static triangle meshes.
      // To use spatial partitioning we have to set a valid spatial partition instance
      // in the Partition property.
      // The spatial partition will store indices of the mesh triangles, therefore
      // the generic type argument is "int".
      meshShape.Partition = new AabbTree<int>()
      {
        // Optional: The tree is automatically built using a mixed top-down/bottom-up approach. 
        // Bottom-up building is slower but produces better trees. If the tree building takes too 
        // long, we can lower the BottomUpBuildThreshold (default is 128).
        BottomUpBuildThreshold = 0,
      };

      // Optional: Build the AABB tree. (This is done automatically when the AABB tree is used for
      // the first time, but Update can also be called explicitly to control when the tree is built.)
      meshShape.Partition.Update(false);


      // Create a collision object with the random triangle mesh shape.
      _mesh = new CollisionObject(new GeometricObject(meshShape, new Pose(new Vector3F(5, 0, 0))));

      // Add collision object to collision domain.
      _domain.CollisionObjects.Add(_box);
      _domain.CollisionObjects.Add(_sphere);
      _domain.CollisionObjects.Add(_mesh);

      // For picking we create a ray.
      // The ray shoot from its local origin in +x direction.
      // (Note: The last parameter is the length of the ray. In theory, rays have
      // an infinite length. However, in the collision detection we use rays with
      // a finite length. This increases the performance and improves the numerical
      // stability of the algorithms.)
      RayShape rayShape = new RayShape(Vector3F.Zero, Vector3F.Forward, 1000);
      _ray = new CollisionObject(new GeometricObject(rayShape, Pose.Identity));

      // The ray is just one additional collision object in our collision domain.
      _domain.CollisionObjects.Add(_ray);

      // The collision domain manages now 4 objects: a box, a sphere, a triangle mesh and a ray.
    }


    public override void Update(GameTime gameTime)
    {
      var mousePosition = InputService.MousePosition;
      var viewport = GraphicsService.GraphicsDevice.Viewport;
      var cameraNode = GraphicsScreen.CameraNode;

      // Update picking ray.
      if (InputService.IsDown(Keys.LeftControl) || InputService.IsDown(Keys.RightControl))
      {
        // Pick using mouse cursor.
        SampleFramework.IsMouseVisible = true;
        GraphicsScreen.DrawReticle = false;

        // Convert the mouse screen position on the near viewing plane into a
        // world space position.
        Vector3 rayStart = viewport.Unproject(
          new Vector3(mousePosition.X, mousePosition.Y, 0),
          cameraNode.Camera.Projection,
          (Matrix)cameraNode.View,
          Matrix.Identity);

        // Convert the mouse screen position on the far viewing plane into a
        // world space position.
        Vector3 rayEnd = viewport.Unproject(
          new Vector3(mousePosition.X, mousePosition.Y, 1),
          cameraNode.Camera.Projection,
          (Matrix)cameraNode.View,
          Matrix.Identity);

        // Update ray. The ray should start at the near viewing plane under the
        // mouse cursor and shoot into viewing direction. Therefore we change the
        // pose of the ray object such that the ray origin is at rayStart and the
        // orientation rotates the ray from shooting into +x direction into a ray
        // shooting in viewing direction (rayEnd - rayStart).
        ((GeometricObject)_ray.GeometricObject).Pose = new Pose(
          (Vector3F)rayStart,
          QuaternionF.CreateRotation(Vector3F.Forward, (Vector3F)(rayEnd - rayStart)));
      }
      else
      {
        // Pick using reticle.
        SampleFramework.IsMouseVisible = false;
        GraphicsScreen.DrawReticle = true;
        ((GeometricObject)_ray.GeometricObject).Pose = cameraNode.PoseWorld;
      }

      // Update collision domain. This computes new contact information.
      _domain.Update(gameTime.ElapsedGameTime);

      // Draw objects. Change the color if the ray hits the object.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      if (_domain.HaveContact(_ray, _box))
        debugRenderer.DrawObject(_box.GeometricObject, Color.Red, false, false);
      else
        debugRenderer.DrawObject(_box.GeometricObject, Color.White, false, false);

      if (_domain.HaveContact(_ray, _sphere))
        debugRenderer.DrawObject(_sphere.GeometricObject, Color.Red, false, false);
      else
        debugRenderer.DrawObject(_sphere.GeometricObject, Color.White, false, false);

      if (_domain.HaveContact(_ray, _mesh))
        debugRenderer.DrawObject(_mesh.GeometricObject, Color.Red, false, false);
      else
        debugRenderer.DrawObject(_mesh.GeometricObject, Color.White, false, false);

      // For triangle meshes we also know which triangle was hit!
      // Get the contact information for ray-mesh hits.
      ContactSet rayMeshContactSet = _domain.ContactSets.GetContacts(_ray, _mesh);

      // rayMeshContactSet is a collection of Contacts between ray and mesh.
      // If rayMeshContactSet is null or it contains no Contacts, then we have no contact.
      if (rayMeshContactSet != null && rayMeshContactSet.Count > 0)
      {
        // A contact set contains information for a pair of touching objects A and B.
        // We know that the two objects are ray and mesh, but we do not know if A or B
        // is the ray.
        bool objectAIsRay = rayMeshContactSet.ObjectA == _ray;

        // Get the contact.
        Contact contact = rayMeshContactSet[0];

        // Get the feature index of the mesh feature that was hit.
        int featureIndex = objectAIsRay ? contact.FeatureB : contact.FeatureA;

        // For triangle meshes the feature index is the index of the triangle that was hit.
        // Get the triangle from the triangle mesh shape.
        Triangle triangle = ((TriangleMeshShape)_mesh.GeometricObject.Shape).Mesh.GetTriangle(featureIndex);

        debugRenderer.DrawShape(new TriangleShape(triangle), _mesh.GeometricObject.Pose, _mesh.GeometricObject.Scale, Color.Yellow, false, false);
      }
    }
  }
}
