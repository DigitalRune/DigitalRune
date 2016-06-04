#if !WP7 && !WP8 && !XBOX
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Samples.Physics.Specialized;
using CurveLoopType = DigitalRune.Mathematics.Interpolation.CurveLoopType;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to add a road to a terrain.",
    @"The TerrainRoadLayer can be used to add a road texture to a terrain. This class can also be
used to change the height values of the terrain to 'carve' the road into the terrain.
The road is created from a 3d spline path.
This sample also uses the vehicle of one of the vehicle samples.",
    136)]
  public class RoadSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly TerrainObject _terrainObject;
    private TerrainRoadLayer _roadLayer;
    private Path3F _roadPath;


    public RoadSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);
      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

      Services.Register(typeof(DebugRenderer), null, _graphicsScreen.DebugRenderer);

      var scene = _graphicsScreen.Scene;
      Services.Register(typeof(IScene), null, scene);

      // Add gravity and damping to the physics simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services, 5000);
      cameraGameObject.ResetPose(new Vector3F(0, 2, 5), 0, 0);
      GameObjectService.Objects.Add(cameraGameObject);

      // Add the vehicle object from the vehicle sample.
      var vehicleObject = new ConstraintVehicleObject(Services);
      GameObjectService.Objects.Add(vehicleObject);

      // Add the car-follow-camera from the vehicle sample.
      var vehicleCameraObject = new VehicleCameraObject(vehicleObject.Vehicle.Chassis, Services);
      GameObjectService.Objects.Add(vehicleCameraObject);

      // Now, we have two CameraNodes. The graphics screen uses the camera node of the CameraObject,
      // as usual.
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;
      // The CameraObject should not react to input.
      cameraGameObject.IsEnabled = false;
      // The CameraNode of the VehicleCameraObject controls the other CameraNode.
      vehicleCameraObject.CameraNode.SceneChanged += (s, e) =>
      {
        cameraGameObject.CameraNode.SetLastPose(false);
        cameraGameObject.CameraNode.PoseWorld = vehicleCameraObject.CameraNode.PoseWorld;
      };

      // Add standard game objects.
      GameObjectService.Objects.Add(new DynamicSkyObject(Services, true, false, true)
      {
        EnableCloudShadows = false,
        FogSampleAngle = 0.1f,
        FogSaturation = 1,
      });

      var fogObject = new FogObject(Services) { AttachToCamera = true };
      GameObjectService.Objects.Add(fogObject);

      // Set nice default fog values.
      // (Note: If we change the fog values here, the GUI in the options window is not
      // automatically updated.)
      fogObject.FogNode.IsEnabled = true;
      fogObject.FogNode.Fog.Start = 100;
      fogObject.FogNode.Fog.End = 2500;
      fogObject.FogNode.Fog.Start = 100;
      fogObject.FogNode.Fog.HeightFalloff = 0.25f;

      // Add the terrain
      _terrainObject = new TerrainObject(Services);
      GameObjectService.Objects.Add(_terrainObject);

      // Add the TerrainRoadLayer to the terrain.
      CreateRoad();

      // Modify the terrain height values.
      ClampTerrainToRoad();
    }


    private void CreateRoad()
    {
      //RandomHelper.Random = new Random(1234567);

      // Set isClosed to true join the start and the end of the road.
      bool isClosed = false;

      // Create a new TerrainRoadLayer which paints a road onto the terrain.
      // The road itself is defined by a mesh which is set later.
      _roadLayer = new TerrainRoadLayer(GraphicsService)
      {
        DiffuseColor = new Vector3F(0.5f),
        SpecularColor = new Vector3F(1),
        DiffuseTexture = ContentManager.Load<Texture2D>("Terrain/Road-Asphalt-Diffuse"),
        NormalTexture = ContentManager.Load<Texture2D>("Terrain/Road-Asphalt-Normal"),
        SpecularTexture = ContentManager.Load<Texture2D>("Terrain/Road-Asphalt-Specular"),
        HeightTexture = ContentManager.Load<Texture2D>("Terrain/Road-Asphalt-Height"),

        // The size of the tileable detail textures in world space units.
        TileSize = 5,

        // The border blend range controls how the border of the road fades out.
        // We fade out 5% of the texture on each side of the road.
        BorderBlendRange = new Vector4F(0.05f, 0.05f, 0.05f, 0.05f),
      };

      // Create 3D spline path with some semi-random control points.
      _roadPath = new Path3F
      {
        PreLoop = isClosed ? CurveLoopType.Cycle : CurveLoopType.Linear,
        PostLoop = isClosed ? CurveLoopType.Cycle : CurveLoopType.Linear,
        SmoothEnds = isClosed,
      };

      // The position of the next path key.
      Vector3F position = new Vector3F(
        RandomHelper.Random.NextFloat(-20, 20),
        0,
        RandomHelper.Random.NextFloat(-20, 20));

      // The direction to the next path key.
      Vector3F direction = QuaternionF.CreateRotationY(RandomHelper.Random.NextFloat(0, 10)).Rotate(Vector3F.Forward);

      // Add path keys.
      for (int j = 0; j < 10; j++)
      {
        // Instead of a normal PathKey3F, we use a TerrainRoadPathKey which allows to control
        // the road with and the side falloff.
        var key = new TerrainRoadPathKey
        {
          Interpolation = SplineInterpolation.CatmullRom,
          Parameter = j,
          Point = position,

          // The width of the road at the path key.
          Width = RandomHelper.Random.NextFloat(6, 10),

          // The side falloff (which is used in ClampTerrainToRoad to blend the height values with
          // the road).
          SideFalloff = RandomHelper.Random.NextFloat(20, 40),
        };

        _roadPath.Add(key);

        // Get next random position and direction.
        position += direction * RandomHelper.Random.NextFloat(20, 40);
        position.Y += RandomHelper.Random.NextFloat(-2, 2);
        direction = QuaternionF.CreateRotationY(RandomHelper.Random.NextFloat(-1, 1))
                               .Rotate(direction);
      }

      if (isClosed)
      {
        // To create a closed path the first and the last key should be identical.
        _roadPath[_roadPath.Count - 1].Point = _roadPath[0].Point;
        ((TerrainRoadPathKey)_roadPath[_roadPath.Count - 1]).Width = ((TerrainRoadPathKey)_roadPath[0]).Width;
        ((TerrainRoadPathKey)_roadPath[_roadPath.Count - 1]).SideFalloff =
          ((TerrainRoadPathKey)_roadPath[0]).SideFalloff;

        // Since the path is closed we do not have to fade out the start and the end of the road.
        _roadLayer.BorderBlendRange *= new Vector4F(1, 0, 1, 0);
      }

      // Convert the path to a mesh.
      Submesh roadSubmesh;
      Aabb roadAabb;
      float roadLength;
      TerrainRoadLayer.CreateMesh(
        GraphicsService.GraphicsDevice,
        _roadPath,
        4,
        10,
        0.1f,
        out roadSubmesh,
        out roadAabb,
        out roadLength);

      // Set the mesh in the road layer.
      _roadLayer.SetMesh(roadSubmesh, roadAabb, roadLength, true);

      if (isClosed)
      {
        // When the path is closed, the end texture and the start texture coordinates should 
        // match. This is the case if (roadLength / tileSize) is an integer.
        var numberOfTiles = (int)(roadLength / _roadLayer.TileSize);
        _roadLayer.TileSize = roadLength / numberOfTiles;
      }

      // The road layer is added to the layers of a tile. We have to add the road to each terrain
      // tile which it overlaps.
      foreach (var tile in _terrainObject.TerrainNode.Terrain.Tiles)
        if (GeometryHelper.HaveContact(tile.Aabb, _roadLayer.Aabb.Value))
          tile.Layers.Add(_roadLayer);
    }


    // Modify the terrain height values to add the road to the terrain.
    private void ClampTerrainToRoad()
    {
      // We have to manipulate the height and normal texture of each tile which contains the road.
      foreach (var tile in _terrainObject.TerrainNode.Terrain.Tiles)
      {
        // Get the current height texture of the tile and extract the heights into a float array.
        var heightTexture = tile.HeightTexture;
        float[] heights = TerrainHelper.GetTextureLevelSingle(heightTexture, 0);

        // Create a temporary height field.
        var heightField = new HeightField(
          tile.OriginX,
          tile.OriginZ,
          tile.WidthX,
          tile.WidthZ,
          heights,
          heightTexture.Width,
          heightTexture.Height);

        // Change the height values of the height field.
        TerrainRoadLayer.ClampTerrainToRoad(heightField, _roadPath, 8, 30, 10, 0.1f);

        // Rebuild the height texture.
        TerrainHelper.CreateHeightTexture(
          GraphicsService.GraphicsDevice,
          heights,
          heightTexture.Width,
          heightTexture.Height,
          false,
          ref heightTexture);

        // Rebuild the normal texture.
        var normalTexture = tile.NormalTexture;
        TerrainHelper.CreateNormalTexture(
          GraphicsService.GraphicsDevice,
          heights,
          heightTexture.Width,
          heightTexture.Height,
          tile.CellSize,
          false,
          ref normalTexture);

        // Get rigid body that represents this tile.
        var rigidBody = Simulation.RigidBodies.First(body => body.UserData == tile);

        // Update the height values of the collision detection height field.
        ((HeightField)rigidBody.Shape).SetSamples(heights, heightTexture.Width, heightTexture.Height);
      }
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();

      // Visualize the road path for debugging.
      //Vector3F? lastPoint = null;
      //foreach (var key in _roadPath)
      //{
      //  _graphicsScreen.DebugRenderer.DrawAxes(new Pose(key.Point), 1, false);

      //  if (lastPoint.HasValue)
      //    _graphicsScreen.DebugRenderer.DrawLine(lastPoint.Value, key.Point, Color.Yellow, false);

      //  lastPoint = key.Point;
      //}
    }
  }
}
#endif
