#if !WP7 && !WP8
using System;
using DigitalRune.Geometry;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples shows how to implement a new scene node which renders a spherical fog volume.",
    @"The new FogSphereNode class is a SceneNode which describes a sphere shape filled with fog.
FogSphereNodes are rendered by the new FogSphereRenderer.
FogSphereNodes can be used to add light scattering effects to a scene. Example usage:
- Create a steamy atmosphere.
- Add a light glow around a light.
- Fog distant objects to separate the foreground from the background.
- Etc.",
    119)]
  public class FogSphereSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;


    public FogSphereSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // Create a graphics screen. This screen has to call the FogSphereRenderer
      // to handle the FogSphereNode!
      _graphicsScreen = new DeferredGraphicsScreen(Services) { DrawReticle = true };
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
      GameObjectService.Objects.Add(new DynamicSkyObject(Services, true, false, true));
      GameObjectService.Objects.Add(new GroundObject(Services));
      GameObjectService.Objects.Add(new DudeObject(Services));
      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));
      GameObjectService.Objects.Add(new DynamicObject(Services, 5));
      GameObjectService.Objects.Add(new DynamicObject(Services, 6));
      GameObjectService.Objects.Add(new DynamicObject(Services, 7));
      GameObjectService.Objects.Add(new FogObject(Services));
      GameObjectService.Objects.Add(new LavaBallsObject(Services));

      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-3, -8), 0, random.NextFloat(0, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree", scale, new Pose(position, orientation)));
      }

      // ----- Add a FogSphereNode to scene.
      var fogSphereNode = new FogSphereNode
      {
        Name = "FogSphere",
        ScaleLocal = new Vector3F(5, 3, 5),
        PoseWorld = new Pose(new Vector3F(0, 0, -3)),
      };
      _graphicsScreen.Scene.Children.Add(fogSphereNode);
    }


    public override void Update(GameTime gameTime)
    {
      var debugRenderer = _graphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Render bounding shape of FogSphereNodes for debugging.
      //debugRenderer.DrawObject(_graphicsScreen.Scene.GetSceneNode("FogSphere"), Color.Yellow, true, false);
    }
  }
}
#endif