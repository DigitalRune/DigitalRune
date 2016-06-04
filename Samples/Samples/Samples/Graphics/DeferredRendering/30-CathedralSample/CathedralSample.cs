// Sibenik Cathedral model not included.

//#if !WP7 && !WP8
//using System;
//using System.Linq;
//using DigitalRune.Geometry;
//using DigitalRune.Geometry.Shapes;
//using DigitalRune.Graphics;
//using DigitalRune.Graphics.PostProcessing;
//using DigitalRune.Graphics.Rendering;
//using DigitalRune.Graphics.SceneGraph;
//using DigitalRune.Mathematics;
//using DigitalRune.Mathematics.Algebra;
//using DigitalRune.Physics.ForceEffects;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using DirectionalLight = DigitalRune.Graphics.DirectionalLight;
//using MathHelper = DigitalRune.Mathematics.MathHelper;


//namespace Samples.Graphics
//{
//  [Sample(SampleCategory.Graphics,
//    @"This sample demonstrates image-based lighting (IBL) inside the Sibenik Cathedral model.",
//    @"Two image-based lights are added to the scene. One in the hall and one above the altar.
//The environment map is reflected on the marble tiles and columns. You can turn on/off, change
//the intensities of the image-based lights or change the glossiness in the Options window (F4).",
//    130)]
//  public class CathedralSample : Sample
//  {
//    private readonly DeferredGraphicsScreen _graphicsScreen;

//    // Rotate scene to show that lights do not have to be axis-aligned.
//    private static readonly Pose _globalRotation = new Pose(Matrix33F.CreateRotationY(MathHelper.ToRadians(0)));

//    // 2 image-based lights.
//    private ImageBasedLight[] _imageBasedLights = new ImageBasedLight[2];
//    private LightNode[] _lightNodes = new LightNode[2];

//    // Some stuff used to render environment maps for the IBL at runtime.
//    private bool _updateEnvironmentMaps = true;
//    private int _oldEnvironmentMapTimeStamp;
//    private SceneCaptureNode[] _sceneCaptureNodes = new SceneCaptureNode[2];
//    private ColorEncoder _colorEncoder;


//    public CathedralSample(Microsoft.Xna.Framework.Game game)
//      : base(game)
//    {
//      SampleFramework.IsMouseVisible = false;

//      _graphicsScreen = new DeferredGraphicsScreen(Services);
//      _graphicsScreen.DrawReticle = true;
//      GraphicsService.Screens.Insert(0, _graphicsScreen);
//      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

//      Services.Register(typeof(DebugRenderer), null, _graphicsScreen.DebugRenderer);
//      Services.Register(typeof(IScene), null, _graphicsScreen.Scene);

//      // Add gravity and damping to the physics simulation.
//      Simulation.ForceEffects.Add(new Gravity());
//      Simulation.ForceEffects.Add(new Damping());

//      // Add a custom game object which controls the camera.
//      var cameraGameObject = new CameraObject(Services);
//      GameObjectService.Objects.Add(cameraGameObject);
//      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

//      // Add standard game objects.
//      GameObjectService.Objects.Add(new GrabObject(Services));
//      GameObjectService.Objects.Add(new GroundObject(Services));
//      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
//      GameObjectService.Objects.Add(new LavaBallsObject(Services));

//      // Add a skybox including lights. The sky should be blindingly bright from inside
//      // the cathedral. -> Pump up the SkyExposure. 
//      GameObjectService.Objects.Add(new StaticSkyObject(Services) { SkyExposure = 1.0f });

//      // Decrease the intensity of ambient lights. We don't want the interior to be too bright.
//      var ambientLights = _graphicsScreen.Scene
//                                         .GetDescendants()
//                                         .OfType<LightNode>()
//                                         .Select(n => n.Light)
//                                         .OfType<AmbientLight>();
//      foreach (var ambientLight in ambientLights)
//        ambientLight.Intensity *= 0.667f;

//      // Add a god ray post-process filter and a game object which updates the god ray directions.
//      var godRayFilter = new GodRayFilter(GraphicsService)
//      {
//        Intensity = new Vector3F(1.0f, 0.9f, 0.8f),
//        NumberOfSamples = 16,
//        NumberOfPasses = 2,
//        Softness = 0,
//      };
//      _graphicsScreen.PostProcessors.Add(godRayFilter);
//      GameObjectService.Objects.Add(new GodRayObject(Services, godRayFilter));

//      // Add a grain filter to add some noise in the night.
//      _graphicsScreen.PostProcessors.Add(new GrainFilter(GraphicsService)
//      {
//        IsAnimated = true,
//        LuminanceThreshold = 0.3f,
//        ScaleWithLuminance = true,
//        Strength = 0.05f,
//        GrainScale = 1.5f,
//      });

//      // Load Sibenik cathedral.
//      GameObjectService.Objects.Add(new StaticObject(Services, "Cathedral/sibenik", 1.0f, _globalRotation * new Pose(new Vector3F(0, 0.1f, 0))));

//      // Create an image-based light in the hall.
//      // (No diffuse light, only specular reflections.)
//      _imageBasedLights[0] = new ImageBasedLight
//      {
//        Shape = Shape.Infinite,
//        DiffuseIntensity = 1,
//        SpecularIntensity = 1f,
//        BlendMode = 0.5f, // Mix diffuse light to scene. (0 = add, 1 = replace, 0.5 = mix)
//        EnableLocalizedReflection = true,
//        LocalizedReflectionBox = new Aabb(new Vector3F(-15, -4, -8), new Vector3F(16, 4, 8)),
//      };

//      _lightNodes[0] = new LightNode(_imageBasedLights[0]);
//      _lightNodes[0].PoseLocal = _globalRotation * new Pose(new Vector3F(4, 4, 0));
//      _graphicsScreen.Scene.Children.Add(_lightNodes[0]);

//      // Add a second image-based light above the altar.
//      // (This time we add some diffuse light to the scene to add some red bounce light.)
//      _imageBasedLights[1] = new ImageBasedLight
//      {
//        Shape = new BoxShape(18, 50, 18),
//        DiffuseIntensity = 1,
//        SpecularIntensity = 1,
//        BlendMode = 0.5f,
//        FalloffRange = 0.2f,
//        EnableLocalizedReflection = true,
//        LocalizedReflectionBox = new Aabb(new Vector3F(-8, -6, -8), new Vector3F(26, 12, 8)),
//      };

//      _lightNodes[1] = new LightNode(_imageBasedLights[1]);
//      _lightNodes[1].PoseLocal = _globalRotation * new Pose(new Vector3F(-12, 8, 0));
//      _graphicsScreen.Scene.Children.Add(_lightNodes[1]);

//      // Add more test objects to the scenes.
//      AddLightProbes();
//      AddTestSpheres(new Vector3F(0, 0, 0));
//      AddTestSpheres(new Vector3F(-10, 2, 0));

//      CreateGuiControls();
//    }


//    /// <summary>
//    /// Adds small reflective spheres at the IBL positions to visualize the environment maps.
//    /// </summary>
//    private void AddLightProbes()
//    {
//      // Create sphere mesh with high specular exponent.
//      var mesh = SampleHelper.CreateMesh(
//        ContentManager,
//        GraphicsService,
//        new SphereShape(0.1f),
//        new Vector3F(0, 0, 0),
//        new Vector3F(1, 1, 1),
//        1000000.0f);

//      // Add a sphere under each image-based light node.
//      for (int i = 0; i < _lightNodes.Length; i++)
//      {
//        _lightNodes[i].Children = new SceneNodeCollection(1);
//        _lightNodes[i].Children.Add(new MeshNode(mesh));
//      }
//    }


//    /// <summary>
//    /// Adds bigger spheres with different diffuse and specular properties.
//    /// </summary>
//    private void AddTestSpheres(Vector3F position)
//    {
//      const int NumX = 4;
//      const int NumZ = 4;
//      for (int x = 0; x < NumX; x++)
//      {
//        Vector3F baseColor = new Vector3F(1.0f, 0.71f, 0.29f);       // Gold
//        Vector3F diffuse = new Vector3F(1 - (float)(x + 1) / NumX);  // Decrease diffuse coefficient.
//        Vector3F specular = new Vector3F((float)(x + 1) / NumX);     // Increase specular coefficient.

//        for (int z = 0; z < NumZ; z++)
//        {
//          // Increase glossiness.
//          float specularPower = (float)Math.Pow(10, z + 1);

//          var mesh = SampleHelper.CreateMesh(
//            ContentManager,
//            GraphicsService,
//            new SphereShape(0.2f),
//            baseColor * diffuse,
//            baseColor * specular,
//            specularPower);

//          _graphicsScreen.Scene.Children.Add(
//            new MeshNode(mesh)
//            {
//              PoseWorld = _globalRotation * new Pose(position + new Vector3F(x, 1.0f, z - (NumZ - 1) / 2.0f))
//            });
//        }
//      }
//    }


//    private void CreateGuiControls()
//    {
//      var panel = SampleFramework.AddOptions("IBL");

//      SampleHelper.AddButton(
//        panel,
//        "Update environment maps",
//        () => _updateEnvironmentMaps = true,
//        null);

//      SampleHelper.AddCheckBox(
//        panel,
//        "Enable localized reflections",
//        true,
//        isChecked =>
//        {
//          for (int i = 0; i < _imageBasedLights.Length; i++)
//            _imageBasedLights[i].EnableLocalizedReflection = isChecked;
//        });

//      for (int i = 0; i < _lightNodes.Length; i++)
//      {
//        int index = i;
//        SampleHelper.AddCheckBox(
//          panel,
//          "IBL " + index + ": Enable image-based light ",
//          true,
//          isChecked =>
//          {
//            _lightNodes[index].IsEnabled = isChecked;
//          });
//        SampleHelper.AddSlider(
//          panel,
//          "IBL " + index + ": Diffuse Intensity",
//          "F3",
//          0.0f,
//          1.0f,
//          _imageBasedLights[index].DiffuseIntensity,
//          value =>
//          {
//            _imageBasedLights[index].DiffuseIntensity = value;
//          });

//        SampleHelper.AddSlider(
//          panel,
//          "IBL " + index + ": Specular Intensity",
//          "F3",
//          0.0f,
//          1.0f,
//          _imageBasedLights[index].SpecularIntensity,
//          value =>
//          {
//            _imageBasedLights[index].SpecularIntensity = value;
//          });

//        SampleHelper.AddSlider(
//          panel,
//          "IBL " + index + ": Blend mode",
//          "F2",
//          0,
//          1,
//          _imageBasedLights[index].BlendMode,
//          value =>
//          {
//            _imageBasedLights[index].BlendMode = value;
//          });
//      }

//      SampleHelper.AddSlider(
//        panel,
//        "Specular power (floor)",
//        "F0",
//        0,
//        200000,
//        10000,
//        value =>
//        {
//          // Change "SpecularPower" in material "mramor6x6".
//          var effectBindings = _graphicsScreen.Scene
//                                              .GetDescendants()
//                                              .OfType<MeshNode>()
//                                              .SelectMany(n => n.Mesh.Materials)
//                                              .Where(m => m.Name == "mramor6x6")
//                                              .Distinct()
//                                              .SelectMany(m => m.EffectBindings)
//                                              .Where(b => b.ParameterBindings.Contains("SpecularPower"));
//          foreach (var effectBinding in effectBindings)
//          {
//            effectBinding.Set("SpecularPower", value);
//          }
//        });

//      SampleFramework.ShowOptionsWindow("IBL");
//    }


//    protected override void Dispose(bool disposing)
//    {
//      if (disposing)
//      {
//        for (int i = 0; i < _sceneCaptureNodes.Length; i++)
//        {
//          _sceneCaptureNodes[i].RenderToTexture.Texture.Dispose();
//          _sceneCaptureNodes[i].Dispose(false);
//        }

//        _colorEncoder.Dispose();
//      }

//      base.Dispose(disposing);
//    }


//    public override void Update(GameTime gameTime)
//    {
//      // Debug rendering:
//      _graphicsScreen.DebugRenderer.Clear();

//      /*
//      for (int i = 0; i < _lightNodes.Length; i++)
//      {
//        if (!_lightNodes[i].IsEnabled)
//          continue;

//        // Draw axes at IBL positions.
//        _graphicsScreen.DebugRenderer.DrawAxes(_lightNodes[i].PoseWorld, 0.5f, false);

//        // Draw bounding boxes in yellow.
//        if (_imageBasedLights[i].Shape is BoxShape)
//          _graphicsScreen.DebugRenderer.DrawObject(_lightNodes[i], Color.Yellow, true, true);

//        // Draw box for localization of reflections in orange.
//        if (_imageBasedLights[i].EnableLocalizedReflection)
//        {
//          var aabb = _imageBasedLights[i].LocalizedReflectionBox.GetValueOrDefault();
//          aabb.Minimum *= _lightNodes[i].ScaleLocal;
//          aabb.Maximum *= _lightNodes[i].ScaleLocal;
//          var extent = aabb.Extent;
//          var pose = _lightNodes[i].PoseWorld * new Pose(aabb.Center);
//          _graphicsScreen.DebugRenderer.DrawBox(extent.X, extent.Y, extent.Z, pose, Color.Orange, true, true);
//        }
//      }
//      //*/

//      UpdateEnvironmentMaps();
//    }


//    /// <summary>
//    /// Renders the environment maps for the image-based lights.
//    /// </summary>
//    /// <remarks>
//    /// This method uses the current DeferredGraphicsScreen to render new environment maps at
//    /// runtime. The DeferredGraphicsScreen has a SceneCaptureRenderer which we can use to
//    /// capture environment maps of the current scene.
//    /// To capture new environment maps the flag _updateEnvironmentMaps must be set to true.
//    /// When this flag is set, SceneCaptureNodes are added to the scene. When the graphics
//    /// screen calls the SceneCaptureRenderer the next time, the new environment maps will be
//    /// captured.
//    /// The flag _updateEnvironmentMaps remains true until the new environment maps are available.
//    /// This method checks the SceneCaptureNode.LastFrame property to check if new environment maps
//    /// have been computed. Usually, the environment maps will be available in the next frame.
//    /// (However, the XNA Game class can skip graphics rendering if the game is running slowly.
//    /// Then we would have to wait more than 1 frame.)
//    /// When environment maps are being rendered, the image-based lights are disabled to capture
//    /// only the scene with ambient and directional lights. Dynamic objects are also disabled
//    /// to capture only the static scene.
//    /// </remarks>
//    private void UpdateEnvironmentMaps()
//    {
//      if (!_updateEnvironmentMaps)
//        return;

//      // One-time initializations: 
//      if (_sceneCaptureNodes[0] == null)
//      {
//        // Create cube maps and scene capture nodes.
//        // (Note: A cube map size of 256 is enough for surfaces with a specular power
//        // in the range [0, 200000].)
//        for (int i = 0; i < _sceneCaptureNodes.Length; i++)
//        {
//          var renderTargetCube = new RenderTargetCube(
//            GraphicsService.GraphicsDevice,
//            256,
//            true,
//            SurfaceFormat.Color,
//            DepthFormat.None);

//          var renderToTexture = new RenderToTexture { Texture = renderTargetCube };
//          var projection = new PerspectiveProjection();
//          projection.SetFieldOfView(ConstantsF.PiOver2, 1, 1, 100);
//          _sceneCaptureNodes[i] = new SceneCaptureNode(renderToTexture)
//          {
//            CameraNode = new CameraNode(new Camera(projection))
//            {
//              PoseWorld = _lightNodes[i].PoseWorld,
//            },
//          };

//          _imageBasedLights[i].Texture = renderTargetCube;
//        }

//        // We use a ColorEncoder to encode a HDR image in a normal Color texture.
//        _colorEncoder = new ColorEncoder(GraphicsService)
//        {
//          SourceEncoding = ColorEncoding.Rgb,
//          TargetEncoding = ColorEncoding.Rgbm,
//        };

//        // The SceneCaptureRenderer has a render callback which defines what is rendered
//        // into the scene capture render targets.
//        _graphicsScreen.SceneCaptureRenderer.RenderCallback = context =>
//        {
//          var graphicsDevice = GraphicsService.GraphicsDevice;
//          var renderTargetPool = GraphicsService.RenderTargetPool;

//          // Get scene nodes which are visible by the current camera.
//          CustomSceneQuery sceneQuery = context.Scene.Query<CustomSceneQuery>(context.CameraNode, context);

//          // The final image has to be rendered into this render target.
//          var ldrTarget = context.RenderTarget;

//          // Use an intermediate HDR render target with the same resolution as the final target.
//          var format = new RenderTargetFormat(ldrTarget)
//          {
//            SurfaceFormat = SurfaceFormat.HdrBlendable,
//            DepthStencilFormat = DepthFormat.Depth24Stencil8
//          };
//          var hdrTarget = renderTargetPool.Obtain2D(format);

//          graphicsDevice.SetRenderTarget(hdrTarget);
//          context.RenderTarget = hdrTarget;

//          // Render scene (without post-processing, without lens flares, no debug rendering, no reticle).
//          _graphicsScreen.RenderScene(sceneQuery, context, false, false, false, false);

//          // Convert the HDR image to RGBM image.
//          context.SourceTexture = hdrTarget;
//          context.RenderTarget = ldrTarget;
//          _colorEncoder.Process(context);
//          context.SourceTexture = null;

//          // Clean up.
//          renderTargetPool.Recycle(hdrTarget);
//          context.RenderTarget = ldrTarget;
//        };
//      }

//      if (_sceneCaptureNodes[0].Parent == null)
//      {
//        // Add the scene capture nodes to the scene.
//        for (int i = 0; i < _sceneCaptureNodes.Length; i++)
//          _graphicsScreen.Scene.Children.Add(_sceneCaptureNodes[i]);

//        // Remember the old time stamp of the nodes.
//        _oldEnvironmentMapTimeStamp = _sceneCaptureNodes[0].LastFrame;

//        // Disable all lights except ambient and directional lights.
//        // We do not capture the image-based lights or any other lights (e.g. point lights)
//        // in the cube map.
//        foreach (var lightNode in _graphicsScreen.Scene.GetDescendants().OfType<LightNode>())
//          lightNode.IsEnabled = (lightNode.Light is AmbientLight) || (lightNode.Light is DirectionalLight);

//        // Disable dynamic objects.
//        foreach (var node in _graphicsScreen.Scene.GetDescendants())
//          if (node is MeshNode || node is LodGroupNode)
//            if (!node.IsStatic)
//              node.IsEnabled = false;
//      }
//      else
//      {
//        // The scene capture nodes are part of the scene. Check if they have been
//        // updated.
//        if (_sceneCaptureNodes[0].LastFrame != _oldEnvironmentMapTimeStamp)
//        {
//          // We have new environment maps. Restore the normal scene.
//          for (int i = 0; i < _sceneCaptureNodes.Length; i++)
//            _graphicsScreen.Scene.Children.Remove(_sceneCaptureNodes[i]);
//          _updateEnvironmentMaps = false;

//          foreach (var lightNode in _graphicsScreen.Scene.GetDescendants().OfType<LightNode>())
//            lightNode.IsEnabled = true;

//          foreach (var node in _graphicsScreen.Scene.GetDescendants())
//            if (node is MeshNode || node is LodGroupNode)
//              if (!node.IsStatic)
//                node.IsEnabled = true;
//        }
//      }
//    }
//  }
//}
//#endif
