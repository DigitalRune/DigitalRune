#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics.ForceEffects;
using DigitalRune.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample allows to test various shadow settings for spotlights and point lights.",
    @"Press <F4> to open the Options window where you can change shadow settings.
To focus on shadows, the other lights and the materials are not rendered when the sample is started.",
    123)]
  public class ShadowSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly LightNode _spotlightNode;
    private readonly Spotlight _spotlight;
    private readonly StandardShadow _standardShadow;
    private readonly LightNode _pointLightNode;
    private readonly PointLight _pointLight;
    private readonly CubeMapShadow _cubeMapShadow;


    public ShadowSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      _graphicsScreen = new DeferredGraphicsScreen(Services)
      {
        // For debugging: Disable materials and only show light buffer.
        DebugMode = DeferredGraphicsDebugMode.VisualizeDiffuseLightBuffer
      };
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);

      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

      Services.Register(typeof(DebugRenderer), null, _graphicsScreen.DebugRenderer);
      Services.Register(typeof(IScene), null, _graphicsScreen.Scene);

      // Add gravity and damping to the physics simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      GameObjectService.Objects.Add(new GrabObject(Services));

      CreateScene(Services, ContentManager, _graphicsScreen);

      // Disable existing lights.
      foreach (var lightNode in _graphicsScreen.Scene.GetDescendants().OfType<LightNode>())
        lightNode.IsEnabled = false;

      // Add a dim ambient light.
      _graphicsScreen.Scene.Children.Add(
        new LightNode(
          new AmbientLight
          {
            HemisphericAttenuation = 1,
            Intensity = 0.001f,
          }));

      // Add some test lights with shadows.
      _spotlight = new Spotlight { Range = 10, FalloffAngle = 0.8f, CutoffAngle = 1f };
      _standardShadow = new StandardShadow();
      _spotlightNode = new LightNode(_spotlight)
      {
        PoseWorld = new Pose(new Vector3F(0, 1f, -2)),
        Shadow = _standardShadow,
        IsEnabled = true,
      };
      _graphicsScreen.Scene.Children.Add(_spotlightNode);

      _cubeMapShadow = new CubeMapShadow();
      _pointLight = new PointLight { Range = 10, };
      _pointLightNode = new LightNode(_pointLight)
      {
        PoseWorld = new Pose(new Vector3F(0, 1f, -2)),
        Shadow = _cubeMapShadow,
        IsEnabled = false,
      };
      _graphicsScreen.Scene.Children.Add(_pointLightNode);

      CreateGuiControls();
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();
    }


    // Creates a test scene with a lot of randomly placed objects.
    internal static void CreateScene(ServiceContainer services, ContentManager content, DeferredGraphicsScreen graphicsScreen)
    {
      var gameObjectService = services.GetInstance<IGameObjectService>();
      var graphicsService = services.GetInstance<IGraphicsService>();

      gameObjectService.Objects.Add(new DynamicSkyObject(services, true, false, true)
      {
        EnableAmbientLight = false, // Disable ambient light of sky to make shadows more visible.
        EnableCloudShadows = false
      });

      gameObjectService.Objects.Add(new GroundObject(services));
      gameObjectService.Objects.Add(new DynamicObject(services, 1));
      gameObjectService.Objects.Add(new DynamicObject(services, 2));
      gameObjectService.Objects.Add(new DynamicObject(services, 3));
      gameObjectService.Objects.Add(new DynamicObject(services, 5));
      gameObjectService.Objects.Add(new DynamicObject(services, 6));
      gameObjectService.Objects.Add(new DynamicObject(services, 7));
      gameObjectService.Objects.Add(new ObjectCreatorObject(services));
      gameObjectService.Objects.Add(new LavaBallsObject(services));

      var random = new Random();

      // Spheres
      var sphereMesh = SampleHelper.CreateMesh(content, graphicsService, new SphereShape(1));
      for (int i = 0; i < 100; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-100, 100), random.NextFloat(0, 3), random.NextFloat(-100, 100));
        float scale = random.NextFloat(0.5f, 3f);
        var meshNode = new MeshNode(sphereMesh)
        {
          PoseLocal = new Pose(position),
          ScaleLocal = new Vector3F(scale),
          IsStatic = true,
        };
        graphicsScreen.Scene.Children.Add(meshNode);
      }

      // Boxes
      var boxMesh = SampleHelper.CreateMesh(content, graphicsService, new BoxShape(1, 1, 1));
      for (int i = 0; i < 100; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-100, 100), random.NextFloat(0, 3), random.NextFloat(-100, 100));
        QuaternionF orientation = random.NextQuaternionF();
        Vector3F scale = random.NextVector3F(0.1f, 4f);
        var meshNode = new MeshNode(boxMesh)
        {
          PoseLocal = new Pose(position, orientation),
          ScaleLocal = scale,
          IsStatic = true,
        };
        graphicsScreen.Scene.Children.Add(meshNode);
      }

      // Height field with smooth hills.
      var numberOfSamplesX = 20;
      var numberOfSamplesZ = 20;
      var samples = new float[numberOfSamplesX * numberOfSamplesZ];
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
            samples[z * numberOfSamplesX + x] = 1 + (float)(Math.Cos(z / 2f) * Math.Sin(x / 2f) * 1);
          }
        }
      }
      var heightField = new HeightField(0, 0, 20, 20, samples, numberOfSamplesX, numberOfSamplesZ);
      var heightFieldMesh = SampleHelper.CreateMesh(content, graphicsService, heightField);
      var heightFieldMeshNode = new MeshNode(heightFieldMesh)
      {
        PoseLocal = new Pose(new Vector3F(20, 0, -20)),
        ScaleLocal = new Vector3F(1, 2, 1),
        IsStatic = true,
      };
      graphicsScreen.Scene.Children.Add(heightFieldMeshNode);

      // Dudes.
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-20, 20), 0, random.NextFloat(-20, 20));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        gameObjectService.Objects.Add(new DudeObject(services) { Pose = new Pose(position, orientation) });
      }

      // Palm trees.
      for (int i = 0; i < 100; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-80, 80), 0, random.NextFloat(-100, 100));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        gameObjectService.Objects.Add(new StaticObject(services, "PalmTree/palm_tree", scale, new Pose(position, orientation)));
      }

      // Rocks
      for (int i = 0; i < 100; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-80, 80), 1, random.NextFloat(-100, 100));
        QuaternionF orientation = RandomHelper.Random.NextQuaternionF();
        float scale = random.NextFloat(0.5f, 1.2f);
        gameObjectService.Objects.Add(new StaticObject(services, "Rock/rock_05", scale, new Pose(position, orientation)));
      }

      // Grass
      for (int i = 0; i < 100; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-20, 20), 0, random.NextFloat(-20, 20));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        gameObjectService.Objects.Add(new StaticObject(services, "Grass/Grass", scale, new Pose(position, orientation)));
      }

      // More plants
      for (int i = 0; i < 100; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-20, 20), 0, random.NextFloat(-20, 20));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        gameObjectService.Objects.Add(new StaticObject(services, "Parviflora/Parviflora", scale, new Pose(position, orientation)));
      }

      // "Skyscrapers"
      for (int i = 0; i < 20; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(90, 100), 0, random.NextFloat(-100, 100));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        Vector3F scale = new Vector3F(random.NextFloat(6, 20), random.NextFloat(10, 100), random.NextFloat(6, 20));
        var meshNode = new MeshNode(boxMesh)
        {
          PoseLocal = new Pose(position, orientation),
          ScaleLocal = scale,
          IsStatic = true,
          UserFlags = 1, // Mark the distant huge objects. Used in render callbacks in the CompositeShadowSample.
        };
        graphicsScreen.Scene.Children.Add(meshNode);
      }

      // "Hills"
      for (int i = 0; i < 20; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-90, -100), 0, random.NextFloat(-100, 100));
        Vector3F scale = new Vector3F(random.NextFloat(10, 20), random.NextFloat(10, 30), random.NextFloat(10, 20));
        var meshNode = new MeshNode(sphereMesh)
        {
          PoseLocal = new Pose(position),
          ScaleLocal = scale,
          IsStatic = true,
          UserFlags = 1, // Mark the distant huge objects. Used in render callbacks in the CompositeShadowSample.
        };
        graphicsScreen.Scene.Children.Add(meshNode);
      }
    }


    private void CreateGuiControls()
    {
      var panel = SampleFramework.AddOptions("Shadows");

      // ----- Light node controls
      var lightNodePanel = SampleHelper.AddGroupBox(panel, "Light Nodes");

      SampleHelper.AddDropDown(
        lightNodePanel,
        "Light type",
        new[] { "Spotlight", "PointLight" },
        0,
        selectedItem =>
        {
          bool enableSpotlight = (selectedItem == "Spotlight");
          _spotlightNode.IsEnabled = enableSpotlight;
          _pointLightNode.IsEnabled = !enableSpotlight;
        });

      SampleHelper.AddSlider(
        lightNodePanel,
        "Range",
        "F2",
        1,
        30,
        10,
        value =>
        {
          _spotlight.Range = value;
          _pointLight.Range = value;
        });

      SampleHelper.AddSlider(
        lightNodePanel,
        "Y position",
        "F2",
        0,
        10,
        1,
        value =>
        {
          foreach (var node in new[] { _spotlightNode, _pointLightNode })
          {
            var pose = node.PoseWorld;
            pose.Position.Y = value;
            node.PoseWorld = pose;
          }
        });

      SampleHelper.AddSlider(
        lightNodePanel,
        "X rotation",
        "F0",
        -90,
        90,
        1,
        value =>
        {
          var pose = _spotlightNode.PoseWorld;
          pose.Orientation = Matrix33F.CreateRotationX(MathHelper.ToRadians(value));
          _spotlightNode.PoseWorld = pose;
        });

      SampleHelper.AddSlider(
        lightNodePanel,
        "Spotlight angle",
        "F2",
        1,
        89,
        MathHelper.ToDegrees(_spotlight.CutoffAngle),
        value =>
        {
          float angle = MathHelper.ToRadians(value);
          _spotlight.FalloffAngle = 0.8f * angle;
          _spotlight.CutoffAngle = angle;
        });

      // ----- Shadow controls
      var shadowPanel = SampleHelper.AddGroupBox(panel, "Shadow");

      SampleHelper.AddSlider(
        shadowPanel,
        "Shadow map resolution",
        "F0",
        16,
        1024,
        _standardShadow.PreferredSize,
        value =>
        {
          _standardShadow.PreferredSize = (int)value;
          _cubeMapShadow.PreferredSize = (int)value;
        });

      SampleHelper.AddCheckBox(
       shadowPanel,
       "Prefer 16 bit",
       _standardShadow.Prefer16Bit,
       isChecked =>
       {
         _standardShadow.Prefer16Bit = isChecked;
         _cubeMapShadow.Prefer16Bit = isChecked;
       });

      SampleHelper.AddSlider(
        shadowPanel,
        "Depth bias",
        "F2",
        0,
        10,
        _standardShadow.DepthBias,
        value =>
        {
          _standardShadow.DepthBias = value;
          _cubeMapShadow.DepthBias = value;
        });

      SampleHelper.AddSlider(
        shadowPanel,
        "Normal offset",
        "F2",
        0,
        10,
        _standardShadow.NormalOffset,
        value =>
        {
          _standardShadow.NormalOffset = value;
          _cubeMapShadow.NormalOffset = value;
        });

      SampleHelper.AddSlider(
        shadowPanel,
        "Number of samples",
        "F0",
        -1,
        32,
        _standardShadow.NumberOfSamples,
        value =>
        {
          _standardShadow.NumberOfSamples = (int)value;
          _cubeMapShadow.NumberOfSamples = (int)value;
        });

      SampleHelper.AddSlider(
        shadowPanel,
        "Filter radius",
        "F2",
        0,
        10,
        _standardShadow.FilterRadius,
        value =>
        {
          _standardShadow.FilterRadius = value;
          _cubeMapShadow.FilterRadius = value;
        });

      SampleHelper.AddSlider(
        shadowPanel,
        "Jitter resolution",
        "F0",
        1,
        10000,
        _standardShadow.JitterResolution,
        value =>
        {
          _standardShadow.JitterResolution = value;
          _cubeMapShadow.JitterResolution = value;
        });

      SampleFramework.ShowOptionsWindow("Shadows");
    }
  }
}
#endif