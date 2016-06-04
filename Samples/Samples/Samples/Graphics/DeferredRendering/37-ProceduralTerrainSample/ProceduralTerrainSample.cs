#if !WP7 && !WP8 && !XBOX
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to create a procedural terrain with height- and slope-based texturing.",
    @"",
    137)]
  public class ProceduralTerrainSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;


    public ProceduralTerrainSample(Microsoft.Xna.Framework.Game game)
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
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      // Add standard game objects.
      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new LavaBallsObject(Services));
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

      // Add a procedural terrain.
      var terrainObject = new ProceduralTerrainObject(Services);
      GameObjectService.Objects.Add(terrainObject);

      SampleFramework.ShowOptionsWindow();
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();
    }
  }
}
#endif
