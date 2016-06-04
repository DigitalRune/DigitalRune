#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MathHelper = DigitalRune.Mathematics.MathHelper;
using Plane = DigitalRune.Geometry.Shapes.Plane;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to render and infinite plane of water including waves.",
    @"",
    118)]
  [Controls(@"Sample
  Hold <H>/<Shift>+<H> to decrease/increase the wave height.
  Press <J> to switch water color.
  Press <K> to switch between skybox reflection and planar reflection.
  Press <L> to change caustic settings.")]
  public class OceanSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;

    private readonly WaterNode _waterNode;
    private int _waterColorType;
    private int _causticType;


    public OceanSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);
      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

      Services.Register(typeof(DebugRenderer), null, _graphicsScreen.DebugRenderer);
      Services.Register(typeof(IScene), null, _graphicsScreen.Scene);

      // Add gravity and damping to the physics Simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      // More standard objects.
      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      //GameObjectService.Objects.Add(new StaticSkyObject(Services));
      var dynamicSkyObject = new DynamicSkyObject(Services, true, false, true);
      GameObjectService.Objects.Add(dynamicSkyObject);

      // Add an island model.
      GameObjectService.Objects.Add(new StaticObject(Services, "Island/Island", new Vector3F(30), new Pose(new Vector3F(0, 0.75f, 0)), true, true));

      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));
      GameObjectService.Objects.Add(new DynamicObject(Services, 5));
      GameObjectService.Objects.Add(new DynamicObject(Services, 6));
      GameObjectService.Objects.Add(new DynamicObject(Services, 7));
      GameObjectService.Objects.Add(new FogObject(Services) { AttachToCamera = true });

      // The LavaBalls class controls all lava ball instances.
      var lavaBalls = new LavaBallsObject(Services);
      GameObjectService.Objects.Add(lavaBalls);

      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 20; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-7, 4), 0, random.NextFloat(13, 18));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.8f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree", scale, new Pose(position, orientation)));
      }

      // Define the appearance of the water.
      var waterOcean = new Water
      {
        SpecularColor = new Vector3F(20f),
        SpecularPower = 500,

        NormalMap0 = null,
        NormalMap1 = null,

        RefractionDistortion = 0.1f,
        ReflectionColor = new Vector3F(0.2f),
        RefractionColor = new Vector3F(0.6f),

        // Water is scattered in high waves and this makes the wave crests brighter.
        // ScatterColor defines the intensity of this effect.
        ScatterColor = new Vector3F(0.05f, 0.1f, 0.1f),

        // Foam is automatically rendered where the water intersects geometry and
        // where wave are high.
        FoamMap = ContentManager.Load<Texture2D>("Water/Foam"),
        FoamMapScale = 5,
        FoamColor = new Vector3F(1),
        FoamCrestMin = 0.3f,
        FoamCrestMax = 0.8f,

        // Approximate underwater caustics are computed in real-time from the waves.
        CausticsSampleCount = 3,
        CausticsIntensity = 3,
        CausticsPower = 100,
      };

      // If we do not specify a shape in the WaterNode constructor, we get an infinite
      // water plane.
      _waterNode = new WaterNode(waterOcean, null)
      {
        PoseWorld = new Pose(new Vector3F(0, 0.5f, 0)),
        SkyboxReflection = _graphicsScreen.Scene.GetDescendants().OfType<SkyboxNode>().First(),

        // ExtraHeight must be set to a value greater than the max. wave height. 
        ExtraHeight = 2,
      };
      _graphicsScreen.Scene.Children.Add(_waterNode);

      // OceanWaves can be set to displace water surface using a displacement map.
      // The displacement map is computed by the WaterWaveRenderer (see DeferredGraphicsScreen)
      // using FFT and a statistical ocean model.
      _waterNode.Waves = new OceanWaves
      {
        TextureSize = 256,
        HeightScale = 0.004f,
        Wind = new Vector3F(10, 0, 10),
        Directionality = 1,
        Choppiness = 1,
        TileSize = 20,

        // If we enable CPU queries, we can call OceanWaves.GetDisplacement()
        // (see Update() method below).
        EnableCpuQueries = true,
      };

      // Optional: Use a planar reflection instead of the skybox reflection.
      // We add a PlanarReflectionNode as a child of the WaterNode.
      var renderToTexture = new RenderToTexture
      {
        Texture = new RenderTarget2D(GraphicsService.GraphicsDevice, 512, 512, false, SurfaceFormat.HdrBlendable, DepthFormat.None),
      };
      var planarReflectionNode = new PlanarReflectionNode(renderToTexture)
      {
        Shape = _waterNode.Shape,
        NormalLocal = new Vector3F(0, 1, 0),
        IsEnabled = false,
      };
      _waterNode.PlanarReflection = planarReflectionNode;
      _waterNode.Children = new SceneNodeCollection(1) { planarReflectionNode };

      // To let rigid bodies swim, we add a Buoyancy force effect. This force effect
      // computes buoyancy of a flat water surface.
      Simulation.ForceEffects.Add(new Buoyancy
      {
        Surface = new Plane(new Vector3F(0, 1, 0), _waterNode.PoseWorld.Position.Y),
        Density = 1500,
        AngularDrag = 0.3f,
        LinearDrag = 3,
      });
    }


    public override void Update(GameTime gameTime)
    {
      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      var debugRenderer = _graphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Change wave height.
      if (InputService.IsDown(Keys.H))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float delta = sign * deltaTime * 0.01f;
        var oceanWaves = ((OceanWaves)_waterNode.Waves);
        oceanWaves.HeightScale = Math.Max(0, oceanWaves.HeightScale + delta);
      }

      // Switch water color.
      if (InputService.IsPressed(Keys.J, true))
      {
        if (_waterColorType == 0)
        {
          _waterColorType = 1;
          _waterNode.Water.UnderwaterFogDensity = new Vector3F(12, 8, 8) * 0.04f;
          _waterNode.Water.WaterColor = new Vector3F(10, 30, 79) * 0.002f;
        }
        else
        {
          _waterColorType = 0;
          _waterNode.Water.UnderwaterFogDensity = new Vector3F(1, 0.8f, 0.6f);
          _waterNode.Water.WaterColor = new Vector3F(0.2f, 0.4f, 0.5f);
        }
      }

      // Toggle reflection.
      if (InputService.IsPressed(Keys.K, true))
      {
        _waterNode.PlanarReflection.IsEnabled = !_waterNode.PlanarReflection.IsEnabled;
      }

      // Switch caustics.
      if (InputService.IsPressed(Keys.L, true))
      {
        if (_causticType == 0)
        {
          _causticType = 1;
          _waterNode.Water.CausticsSampleCount = 5;
          _waterNode.Water.CausticsIntensity = 10;
          _waterNode.Water.CausticsPower = 200;
        }
        else if (_causticType == 1)
        {
          // Disable caustics
          _causticType = 2;
          _waterNode.Water.CausticsIntensity = 0;
        }
        else
        {
          _causticType = 0;
          _waterNode.Water.CausticsSampleCount = 3;
          _waterNode.Water.CausticsIntensity = 3;
          _waterNode.Water.CausticsPower = 100;
        }
      }

      // Move rigid bodies with the waves:
      // The Buoyancy force effect is only designed for a flat water surface.
      // This code applies some impulses to move the bodies. It is not physically 
      // correct but looks ok.
      // The code tracks 3 arbitrary positions on each body. Info for the positions
      // are stored in RigidBody.UserData. The wave displacements of the previous
      // frame and the current frame are compared an impulse proportional to the 
      // displacement change is applied.
      foreach (var body in Simulation.RigidBodies)
      {
        if (body.MotionType != MotionType.Dynamic)
          continue;

        // Check how much the body penetrates the water using a simple AABB check.
        Aabb aabb = body.Aabb;
        float waterPenetration = (float)Math.Pow(
          MathHelper.Clamp((_waterNode.PoseWorld.Position.Y - aabb.Minimum.Y) / aabb.Extent.Y, 0, 1),
          3);

        if (waterPenetration < 0)
        {
          body.UserData = null;
          continue;
        }

        // 3 displacement vectors are stored in the UserData.
        var previousDisplacements = body.UserData as Vector3F[];
        if (previousDisplacements == null)
        {
          previousDisplacements = new Vector3F[3];
          body.UserData = previousDisplacements;
        }

        for (int i = 0; i < 3; i++)
        {
          // Get an arbitrary position on or near the body.
          Vector3F position = new Vector3F(
            (i < 2) ? aabb.Minimum.X : aabb.Maximum.X,
            aabb.Minimum.Y,
            (i % 2 == 0) ? aabb.Minimum.Z : aabb.Maximum.Z);

          // Get wave displacement of this position.
          var waves = (OceanWaves)_waterNode.Waves;
          Vector3F displacement, normal;
          waves.GetDisplacement(position.X, position.Z, out displacement, out normal);

          // Compute velocity from displacement change.
          Vector3F currentVelocity = body.GetVelocityOfWorldPoint(position);
          Vector3F desiredVelocity = (displacement - previousDisplacements[i]) / deltaTime;

          // Apply impulse proportional to the velocity change of the water.
          Vector3F velocityDelta = desiredVelocity - currentVelocity;
          body.ApplyImpulse(
            velocityDelta * body.MassFrame.Mass * waterPenetration * 0.1f,
            position);

          previousDisplacements[i] = displacement;
        }
      }
    }
  }
}
#endif