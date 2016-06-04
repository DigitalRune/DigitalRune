#if !WP7 && !WP8
using DigitalRune.Game.Input;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows a 4-way split screen of the same scene with different LOD blending modes.",
    @"Sample
  Press <B> on keyboard or game pad to drop another LOD model (barrel).",
    110)]
  public class LodBlendingSample : Sample
  {
    private readonly FourWaySplitScreen _graphicsScreen;

    public LodBlendingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      _graphicsScreen = new FourWaySplitScreen(Services);
      _graphicsScreen.DrawReticle = false;
      GraphicsService.Screens.Insert(0, _graphicsScreen);

      Services.Register(typeof(DebugRenderer), null, _graphicsScreen.DebugRenderer);
      Services.Register(typeof(IScene), null, _graphicsScreen.Scene);

      // Add gravity and damping to the physics Simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new GroundObject(Services));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new FogObject(Services));
      GameObjectService.Objects.Add(new StaticSkyObject(Services));

      // Add a few LOD test objects.
      for (int i = 0; i < 10; i++)
        GameObjectService.Objects.Add(new LodTestObject(Services));
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();

      if (InputService.IsPressed(Keys.B, true) || InputService.IsPressed(Buttons.B, true, LogicalPlayerIndex.One))
        GameObjectService.Objects.Add(new LodTestObject(Services));

      base.Update(gameTime);
    }
  }
}
#endif