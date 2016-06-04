using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;


namespace WpfInteropSample2
{
  // Creates a graphics screen and adds 3D models and lights to the scene graph.
  internal static class TestLevel
  {
    public static Scene Create()
    {
      var graphicsService = ServiceLocator.Current.GetInstance<IGraphicsService>();
      var content = ServiceLocator.Current.GetInstance<ContentManager>();

      var screen = new MyGraphicsScreen(graphicsService);
      graphicsService.Screens.Add(screen);

      AddLights(screen.Scene);

      var groundModel = content.Load<ModelNode>("Ground/Ground").Clone();
      screen.Scene.Children.Add(groundModel);

      var tankModel = content.Load<ModelNode>("Tank/tank").Clone();
      screen.Scene.Children.Add(tankModel);

      return screen.Scene;
    }


    // Add light sources for standard three-point lighting.
    private static void AddLights(Scene scene)
    {
      var ambientLight = new AmbientLight
      {
        Color = new Vector3F(0.05333332f, 0.09882354f, 0.1819608f),
        Intensity = 1,
        HemisphericAttenuation = 0,
      };
      scene.Children.Add(new LightNode(ambientLight));

      var keyLight = new DirectionalLight
      {
        Color = new Vector3F(1, 0.9607844f, 0.8078432f),
        DiffuseIntensity = 1,
        SpecularIntensity = 1,
      };
      var keyLightNode = new LightNode(keyLight)
      {
        Name = "KeyLight",
        Priority = 10,   // This is the most important light.
        PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(-0.5265408f, -0.5735765f, -0.6275069f))),
      };
      scene.Children.Add(keyLightNode);

      var fillLight = new DirectionalLight
      {
        Color = new Vector3F(0.9647059f, 0.7607844f, 0.4078432f),
        DiffuseIntensity = 1,
        SpecularIntensity = 0,
      };
      var fillLightNode = new LightNode(fillLight)
      {
        Name = "FillLight",
        PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(0.7198464f, 0.3420201f, 0.6040227f))),
      };
      scene.Children.Add(fillLightNode);

      var backLight = new DirectionalLight
      {
        Color = new Vector3F(0.3231373f, 0.3607844f, 0.3937255f),
        DiffuseIntensity = 1,
        SpecularIntensity = 1,
      };
      var backLightNode = new LightNode(backLight)
      {
        Name = "BackLight",
        PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(0.4545195f, -0.7660444f, 0.4545195f))),
      };
      scene.Children.Add(backLightNode);
    }
  }
}
