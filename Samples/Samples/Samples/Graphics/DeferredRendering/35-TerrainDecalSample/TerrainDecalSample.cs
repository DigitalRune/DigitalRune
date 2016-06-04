#if !WP7 && !WP8 && !XBOX
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to add decals to the terrain.",
    @"To add decals to the terrain you can use the standard DecalNodes. However, it is far more
efficient to use a TerrainDecalLayer to add a decal to terrain tiles.",
    135)]
  public class TerrainDecalSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly TerrainObject _terrainObject;


    public TerrainDecalSample(Microsoft.Xna.Framework.Game game)
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
      var cameraObject = new CameraObject(Services, 5000);
      cameraObject.ResetPose(new Vector3F(0, 2, 5), 0, 0);
      GameObjectService.Objects.Add(cameraObject);
      _graphicsScreen.ActiveCameraNode = cameraObject.CameraNode;

      // Add standard game objects.
      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new LavaBallsObject(Services));
      GameObjectService.Objects.Add(new DynamicSkyObject(Services, true, false, true)
      {
        EnableCloudShadows = false,
      });
      GameObjectService.Objects.Add(new FogObject(Services));

      // Create terrain.
      _terrainObject = new TerrainObject(Services);
      GameObjectService.Objects.Add(_terrainObject);

      // Add a blood decal to the terrain.
      var decal0 = new TerrainDecalLayer(GraphicsService)
      {
        Pose = GetRandomPose(),
        Width = 2,
        Height = 2,
        DiffuseColor = new Vector3F(0.08f),
        SpecularColor = new Vector3F(0.2f),
        SpecularPower = 100,
        DiffuseTexture = ContentManager.Load<Texture2D>("Decals/Decal_diffuse_mask"), // Original: "Decals/Blood_diffuse_mask",
        NormalTexture =  GraphicsService.GetDefaultNormalTexture(),   // Original: ContentManager.Load<Texture2D>("Decals/Blood_normal"),
        SpecularTexture = GraphicsService.GetDefaultTexture2DWhite(), // Original: ContentManager.Load<Texture2D>("Decals/Blood_specular"),
        FadeOutStart = 3,
        FadeOutEnd = 5,
        Alpha = 0.9f,
      };
      AddDecal(decal0);

      // Add a black blood decal (oil spill?)
      var decal1 = new TerrainDecalLayer(GraphicsService)
      {
        Pose = GetRandomPose(),
        Width = 2,
        Height = 2,
        DiffuseColor = new Vector3F(0.0f),
        SpecularColor = new Vector3F(0.5f),
        SpecularPower = 100,
        DiffuseTexture = ContentManager.Load<Texture2D>("Decals/Decal_diffuse_mask"), // Original: "Decals/Blood_diffuse_mask",
        NormalTexture = GraphicsService.GetDefaultNormalTexture(),   // Original: ContentManager.Load<Texture2D>("Decals/Blood_normal"),
        SpecularTexture = GraphicsService.GetDefaultTexture2DWhite(), // Original: ContentManager.Load<Texture2D>("Decals/Blood_specular"),
        FadeOutStart = 3,
        FadeOutEnd = 5,
      };
      AddDecal(decal1);

      // Add more random decals. The decals can share materials!
      var decal0Material = decal0.Material;
      for (int i = 0; i < 50; i++)
      {
        var decal = new TerrainDecalLayer(decal0Material)
        {
          Pose = GetRandomPose(),
          Width = 2,
          Height = 2,
        };
        AddDecal(decal);
      }
    }


    private static Pose GetRandomPose()
    {
      // Get a random position.
      const float decalAreaSize = 20;
      var randomPosition = new Vector3F(
        RandomHelper.Random.NextFloat(-decalAreaSize, decalAreaSize),
        0,
        RandomHelper.Random.NextFloat(-decalAreaSize, 0));

      // Decals are project along the forward (-z) direction.
      // To project onto the terrain we have to point the decal down.
      var downOrientation = Matrix33F.CreateRotationX(-ConstantsF.PiOver2);

      // Get a random rotation around z.
      var randomOrientation = Matrix33F.CreateRotationZ(RandomHelper.Random.NextFloat(0, ConstantsF.TwoPi));

      return new Pose(randomPosition, downOrientation * randomOrientation);
    }


    private void AddDecal(TerrainDecalLayer decal)
    {
      // Decals are added to the layers of a tile. We have to add the decal to each terrain tile
      // which it overlaps.
      foreach (var tile in _terrainObject.TerrainNode.Terrain.Tiles)
        if (GeometryHelper.HaveContact(tile.Aabb, decal.Aabb.Value))
          tile.Layers.Add(decal);
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();
    }
  }
}
#endif
