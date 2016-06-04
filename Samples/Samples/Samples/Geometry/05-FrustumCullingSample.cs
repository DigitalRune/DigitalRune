using System;
using System.Collections.Generic;
using DigitalRune.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

// Both XNA and DigitalRune have a class called MathHelper. To avoid compiler errors
// we need to define which MathHelper we want to use.
using MathHelper = Microsoft.Xna.Framework.MathHelper;

// DigitalRune has a Plane structure which we use instead of the XNA Plane structure.
using Plane = DigitalRune.Geometry.Shapes.Plane;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"This sample shows how to use DigitalRune Geometry for frustum culling in a graphics engine.",
    @"A camera frustum is moved randomly. Collision detection is used to avoid rendering objects 
that are outside the camera frustum.
The scene can be viewed from the viewpoint of the moving camera and from bird's eye view to 
see the moving camera frustum and the culled objects.
Red objects are rendered. White objects are culled.
When viewed from the viewpoint of the moving camera, frustum culling should provide a noticeable 
performance improvement. (Hold <F4> to show/reset profiling data.)",
    5)]
  [Controls(@"Sample
  Press <T> to toggle between camera view and bird's-eye view.
  Press <C> to enable/disable frustum culling.")]
  public class FrustumCullingSample : BasicSample
  {
    // A few constants.
#if WINDOWS_PHONE || ANDROID || IOS
    private const int NumberOfObjects = 1000;  // The number of random objects in the scene.
#else
    private const int NumberOfObjects = 2000; // The number of random objects in the scene.
#endif
    private const float LevelSize = 1000;     // The size of the level in meter.

    // The collision domain that manages collision objects.
    private readonly CollisionDomain _domain;

    // Camera viewing the whole scene from above.
    private readonly CameraNode _topDownCameraNode;
    // Camera moving inside the scene and used for frustum culling.
    private readonly CameraNode _sceneCameraNode;

    // A list where we will store the planes of the camera frustum.
    private readonly List<Plane> _planes = new List<Plane>(6);

    // A few parameters to create a random camera movement. 
    private Vector3F _cameraTargetMovement = Vector3F.Zero;  // The random movement vector of the camera.
    private float _cameraTargetRotation;                     // The random rotation vector of the camera.
    private float _cameraTargetUpdateTime = float.MaxValue;  // The time since the target movement/rotation was updated.

    // Flags, which the user can change with the keyboard.
    private bool _topViewEnabled = false;   // True for bird's-eye view. False for camera view.
    private bool _cullingEnabled = true;   // True to use frustum culling. False to disable frustum culling.


    public FrustumCullingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;

      // The top-down camera.
      var orthographicProjection = new OrthographicProjection();
      orthographicProjection.Set(
        LevelSize * 1.1f * GraphicsService.GraphicsDevice.Viewport.AspectRatio,
        LevelSize * 1.1f,
        1,
        10000f);
      var topDownCamera = new Camera(orthographicProjection);
      _topDownCameraNode = new CameraNode(topDownCamera)
      {
        View = Matrix44F.CreateLookAt(new Vector3F(0, 1000, 0), new Vector3F(0, 0, 0), -Vector3F.UnitZ),
      };

      // The perspective camera moving through the scene.
      var perspectiveProjection = new PerspectiveProjection();
      perspectiveProjection.SetFieldOfView(
        MathHelper.ToRadians(45),
        GraphicsService.GraphicsDevice.Viewport.AspectRatio,
        1,
        500);
      var sceneCamera = new Camera(perspectiveProjection);
      _sceneCameraNode = new CameraNode(sceneCamera);

      // Initialize collision detection.
      // We use one collision domain that manages all objects.
      _domain = new CollisionDomain(new CollisionDetection())
      {
        // We exchange the default broad phase with a DualPartition. The DualPartition
        // has special support for frustum culling.
        BroadPhase = new DualPartition<CollisionObject>(),
      };

      // Create a lot of random objects and add them to the collision domain.
      RandomHelper.Random = new Random(12345);
      for (int i = 0; i < NumberOfObjects; i++)
      {
        // A real scene consists of a lot of complex objects such as characters, vehicles,
        // buildings, lights, etc. When doing frustum culling we need to test each objects against
        // the viewing frustum. If it intersects with the viewing frustum, the object is visible
        // from the camera's point of view. However, in practice we do not test the exact object
        // against the viewing frustum. Each objects is approximated by a simpler shape. In our
        // example, we assume that each object is approximated with an oriented bounding box.
        // (We could also use an other shape, such as a bounding sphere.)

        // Create a random box.
        Shape randomShape = new BoxShape(RandomHelper.Random.NextVector3F(1, 10));

        // Create a random position.
        Vector3F randomPosition;
        randomPosition.X = RandomHelper.Random.NextFloat(-LevelSize / 2, LevelSize / 2);
        randomPosition.Y = RandomHelper.Random.NextFloat(0, 2);
        randomPosition.Z = RandomHelper.Random.NextFloat(-LevelSize / 2, LevelSize / 2);

        // Create a random orientation.
        QuaternionF randomOrientation = RandomHelper.Random.NextQuaternionF();

        // Create object and add it to collision domain.
        var geometricObject = new GeometricObject(randomShape, new Pose(randomPosition, randomOrientation));
        var collisionObject = new CollisionObject(geometricObject)
        {
          CollisionGroup = 0,
        };
        _domain.CollisionObjects.Add(collisionObject);
      }

      // Per default, the collision domain computes collision between all objects. 
      // In this sample we do not need this information and disable it with a collision 
      // filter.
      // In a real application, we would use this collision information for rendering,
      // for example, to find out which lights overlap with which meshes, etc.
      var filter = new CollisionFilter();
      // Disable collision between objects in collision group 0.
      filter.Set(0, 0, false);
      _domain.CollisionDetection.CollisionFilter = filter;

      // Start with the scene camera.
      GraphicsScreen.CameraNode = _sceneCameraNode;

      // We will collect a few statistics for debugging.
      Profiler.SetFormat("NoCull", 1000, "Time in ms to submit DebugRenderer draw jobs without frustum culling.");
      Profiler.SetFormat("WithCull", 1000, "Time in ms to submit DebugRenderer draw jobs with frustum culling.");
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Clean up.
        GraphicsScreen.CameraNode = null;
        _topDownCameraNode.Dispose(false);
        _sceneCameraNode.Dispose(false);
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // <T> --> Toggle between bird's-eye view and camera view.
      if (InputService.IsPressed(Keys.T, true))
      {
        _topViewEnabled = !_topViewEnabled;
        if (_topViewEnabled)
          GraphicsScreen.CameraNode = _topDownCameraNode;
        else
          GraphicsScreen.CameraNode = _sceneCameraNode;
      }

      // <C> --> Enable or disable frustum culling.
      if (InputService.IsPressed(Keys.C, true))
        _cullingEnabled = !_cullingEnabled;

      // Elapsed time since the last frame:
      float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // We update the camera movement target all 10 seconds.
      const float cameraTargetUpdateInterval = 10;

      // Get the current camera position.
      var currentPosition = _sceneCameraNode.PoseWorld.Position;
      var currentOrientation = _sceneCameraNode.PoseWorld.Orientation;

      // Update the camera movement. We move a fraction of the targetMovement / targetRotation.
      _sceneCameraNode.PoseWorld = new Pose(
        currentPosition + _cameraTargetMovement * timeStep / cameraTargetUpdateInterval,
        Matrix33F.CreateRotationY(_cameraTargetRotation * timeStep / cameraTargetUpdateInterval) * currentOrientation);

      // When the cameraTargetUpdateInterval has passed, we choose a new random movement
      // vector and rotation angle.
      _cameraTargetUpdateTime += timeStep;
      if (_cameraTargetUpdateTime > cameraTargetUpdateInterval)
      {
        _cameraTargetUpdateTime = 0;

        // Get random rotation angle.
        _cameraTargetRotation = RandomHelper.Random.NextFloat(-ConstantsF.TwoPi, ConstantsF.TwoPi);

        // Get a random movement vector. We get random vector until we have a movement vector
        // that does not move the camera outside of the level boundaries.
        do
        {
          _cameraTargetMovement = RandomHelper.Random.NextVector3F(-LevelSize, LevelSize);
          _cameraTargetMovement.Y = 0;

        } while (Math.Abs(_sceneCameraNode.PoseWorld.Position.X + _cameraTargetMovement.X) > LevelSize / 2
                 || Math.Abs(_sceneCameraNode.PoseWorld.Position.Z + _cameraTargetMovement.Z) > LevelSize / 2);
      }

      // Update collision domain. 
      if (_cullingEnabled)
        _domain.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

      // Render objects.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      debugRenderer.DrawText("\n\nCulling " + (_cullingEnabled ? "Enabled" : "Disabled"));

      // Draw frustum.
      debugRenderer.DrawObject(_sceneCameraNode, Color.Red, true, false);

      if (!_cullingEnabled)
      {
        Profiler.Start("NoCull");
        // Simply render all objects.
        // Frustum culling is not used, so we render ALL objects. Most of them will not
        // be visible in the _sceneCamera and a lot of rendering time is wasted.
        foreach (var collisionObject in _domain.CollisionObjects)
        {
          var geometricObject = collisionObject.GeometricObject;
          debugRenderer.DrawObject(geometricObject, Color.Red, false, false);
        }
        Profiler.Stop("NoCull");
      }
      else
      {
        if (_topViewEnabled)
        {
          // Render all objects just for debugging.
          foreach (var collisionObject in _domain.CollisionObjects)
          {
            var geometricObject = collisionObject.GeometricObject;
            debugRenderer.DrawObject(geometricObject, Color.White, false, false);
          }
        }

        // Use frustum culling:
        Profiler.Start("WithCull");

        // Get the combined WorldViewProjection matrix of the camera.
        Matrix44F worldViewProjection = _sceneCameraNode.Camera.Projection.ToMatrix44F() * _sceneCameraNode.PoseWorld.Inverse;

        // Extract the frustum planes of the camera.
        _planes.Clear();
        GeometryHelper.ExtractPlanes(worldViewProjection, _planes, false);

        // Get the broad phase partition.
        var partition = (DualPartition<CollisionObject>)_domain.BroadPhase;

        // ----- Frustum Culling:
        // Use the broad phase partition to get all objects where the axis-aligned
        // bounding box (AABB) overlaps the volume defined by the frustum planes.
        // We draw these objects and can ignore all other objects.
        foreach (var collisionObject in partition.GetOverlaps(_planes))
        {
          var geometricObject = collisionObject.GeometricObject;
          debugRenderer.DrawObject(geometricObject, Color.Red, false, false);
        }

        Profiler.Stop("WithCull");
      }
    }
  }
}
