using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  // Use SceneNode.UserFlags to decide which scene nodes should be rendered opaque
  // or transparent.
  public enum WboitFlags : short
  {
    Default = 0,
    Transparent = 1
  }


  [Sample(SampleCategory.Graphics,
    "Demonstrates weighted blended order-independent transparency.",
    "",
    201)]
  [Controls(@"Weighted Blended Order-Independent Transparency
  Press <Space> to toggle between weighted blended OIT and regular alpha blending.")]
  public class WeightedBlendedOITSample : Sample
  {
    private readonly WeightedBlendedOITScreen _graphicsScreen;


    public WeightedBlendedOITSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // Weighted blended OIT is implemented in custom graphics screen.
      // (Only mesh nodes are supported.)
      _graphicsScreen = new WeightedBlendedOITScreen(Services);
      GraphicsService.Screens.Insert(0, _graphicsScreen);

      // Create the standard camera object.
      var cameraGameObject = new CameraObject(Services, 500);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      // Add some lights.
      SceneSample.InitializeDefaultXnaLights(_graphicsScreen.Scene);

      // The tank model used in this example has special materials.
      // See
      // - Samples\Content\WeightedBlendedOIT\engine_diff_tex.drmat
      // - Samples\Content\WeightedBlendedOIT\turret_alt_diff_tex.drmat
      //
      // These materials support the following render passes:
      // - "Default"
      // - "AlphaBlend" same as "Default" except that the Alpha value is lowered.
      // - "ZOnly" renders only the depth into the depth buffer.
      // - "WeightedBlendedOIT" renders the transparent model into the WBOIT render targets.
      // These render passes are used in the WeightedBlendedOITScreen.

      // Load two instances of the "Tank" model:
      // The first instance is rendered opaque (for reference).
      var model = ContentManager.Load<ModelNode>("WeightedBlendedOIT/tank").Clone();
      model.PoseWorld = new Pose(new Vector3F(-4, 0, 0));
      _graphicsScreen.Scene.Children.Add(model);

      // The second instance is rendered transparent.
      model = ContentManager.Load<ModelNode>("WeightedBlendedOIT/tank").Clone();
      model.PoseWorld = new Pose(new Vector3F(4, 0, 0));
      // Set flag to indicate that this instance should be rendered transparent.
      model.GetSubtree().ForEach(node => node.UserFlags = (short)WboitFlags.Transparent);
      _graphicsScreen.Scene.Children.Add(model);
    }


    public override void Update(GameTime gameTime)
    {
      // Toggle weighted blended OIT on/off.
      if (InputService.IsPressed(Keys.Space, false))
        _graphicsScreen.EnableWboit = !_graphicsScreen.EnableWboit;
    }
  }
}
