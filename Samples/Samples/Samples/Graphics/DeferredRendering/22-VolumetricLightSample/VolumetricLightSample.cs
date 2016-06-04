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
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how create volumetric light effects (light shafts).",
    @"The volumetric light effects are created using a new VolumetricLightNode. The node must be added
to the children of a light node. The DeferredGraphicsScreen uses a VolumetricLightRenderer to render
the new nodes.
The effect is created using raymarching (sampling the light intensities at several position in the
light AABB). Noise is used to hide banding artifacts.",
    122)]
  public class VolumetricLightSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly VolumetricLightRenderer _volumetricLightRenderer;
    private int _numberOfSamples = 10;
    private int _mipMapBias;


    public VolumetricLightSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // Create a graphics screen. This screen has to call the VolumetricLightRenderer
      // to handle the VolumetricLightNode!
      _graphicsScreen = new DeferredGraphicsScreen(Services);
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
      GameObjectService.Objects.Add(new DynamicSkyObject(Services, true, false, true)
      {
#if XBOX
        Time = new DateTime(2014, 06, 01, 0, 0, 0, 0),
#else
        Time = new DateTimeOffset(2014, 06, 01, 0, 0, 0, 0, TimeSpan.Zero),
#endif
      });

      //GameObjectService.Objects.Add(new GroundObject(Services));
      // Add a ground plane with some detail to see the water refractions.
      Simulation.RigidBodies.Add(new RigidBody(new PlaneShape(new Vector3F(0, 1, 0), 0)));
      GameObjectService.Objects.Add(new StaticObject(Services, "Gravel/Gravel", 1, new Pose(new Vector3F(0, 0.001f, 0))));

      GameObjectService.Objects.Add(new DudeObject(Services));
      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));
      GameObjectService.Objects.Add(new DynamicObject(Services, 3));
      GameObjectService.Objects.Add(new DynamicObject(Services, 4));
      GameObjectService.Objects.Add(new DynamicObject(Services, 5));
      GameObjectService.Objects.Add(new DynamicObject(Services, 6));
      GameObjectService.Objects.Add(new DynamicObject(Services, 7));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new FogObject(Services));

      var lavaBallsObject = new LavaBallsObject(Services);
      GameObjectService.Objects.Add(lavaBallsObject);
      lavaBallsObject.Spawn();

      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-3, -8), 0, random.NextFloat(0, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree", scale, new Pose(position, orientation)));
      }

      // Add some additional test lights
      GameObjectService.Objects.Add(new TestLightsObject(Services));

      // The following lights are not needed.
      _graphicsScreen.Scene.GetDescendants().First(n => n.Name == "AmbientLight").IsEnabled = false;
      _graphicsScreen.Scene.GetDescendants().First(n => n.Name == "DirectionalLightWithShadow").IsEnabled = false;

      // Add a volumetric light node under each light node (except ambient and directional lights).
      foreach (var lightNode in _graphicsScreen.Scene.GetSubtree().OfType<LightNode>())
      {
        if (lightNode.Light is PointLight || lightNode.Light is Spotlight || lightNode.Light is ProjectorLight)
        {
          if (lightNode.Children == null)
            lightNode.Children = new SceneNodeCollection();

          lightNode.Children.Add(new VolumetricLightNode
          {
            Color = new Vector3F(0.1f),
            NumberOfSamples = _numberOfSamples,
            MipMapBias = _mipMapBias,
          });
        }
      }

      // Get the renderer used by the screen.
      _volumetricLightRenderer = _graphicsScreen.AlphaBlendSceneRenderer.Renderers.OfType<VolumetricLightRenderer>().First();
    }


    public override void Update(GameTime gameTime)
    {
      // Toggle animated noise.
      if (InputService.IsPressed(Keys.H, false))
        _volumetricLightRenderer.AnimateNoise = !_volumetricLightRenderer.AnimateNoise;

      // Change number of samples.
      if (InputService.IsPressed(Keys.J, true))
      {
        if ((InputService.ModifierKeys & ModifierKeys.Shift) != 0)
          _numberOfSamples++;
        else
          _numberOfSamples = Math.Max(1, _numberOfSamples - 1);

        foreach (var volumetricLightNode in _graphicsScreen.Scene.GetSubtree().OfType<VolumetricLightNode>())
          volumetricLightNode.NumberOfSamples = _numberOfSamples;
      }

      // Change mipmap bias for light texture.
      if (InputService.IsPressed(Keys.K, true))
      {
        if ((InputService.ModifierKeys & ModifierKeys.Shift) != 0)
          _mipMapBias++;
        else
          _mipMapBias = Math.Max(0, _mipMapBias - 1);

        foreach (var volumetricLightNode in _graphicsScreen.Scene.GetSubtree().OfType<VolumetricLightNode>())
          volumetricLightNode.MipMapBias = _mipMapBias;
      }

      // Toggle off-screen rendering.
      if (InputService.IsPressed(Keys.L, true))
        _volumetricLightRenderer.EnableOffscreenRendering = !_volumetricLightRenderer.EnableOffscreenRendering;

      var debugRenderer = _graphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      _graphicsScreen.DebugRenderer.DrawText("\n\nPress <H> to toggle animation of noise: " + _volumetricLightRenderer.AnimateNoise
        + "\nPress <J> or <Shift>+<J> to decrease or increase the number of samples: " + _numberOfSamples
        + "\nPress <K> or <Shift>+<K> to decrease or increase the mipmap bias: " + _mipMapBias
        + "\nPress <L> to toggle half-res offscreen rendering: " + _volumetricLightRenderer.EnableOffscreenRendering);
    }
  }
}
#endif