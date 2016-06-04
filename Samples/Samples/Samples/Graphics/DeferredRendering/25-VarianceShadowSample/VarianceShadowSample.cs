using DigitalRune;
#if !WP7 && !WP8
using System.Linq;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows to implement Variance Shadow Mapping (VSM).",
    @"This sample shows how you can add a custom shadow mapping type to your project.
Variance Shadow Mapping (VSM) is a shadow mapping method which creates a shadow map that can
be filtered (i.e. blurred). This allows to create filtered shadows which are great for smooth
shadows of distant hills. However, VSM suffer from light bleeding artifacts and need more memory.
To add a new shadow type you have to create class derived from the base class Shadow, a renderer
which renders the shadow map and a renderer which renders the shadow mask.
In this sample a single VSM is used for the whole level. This creates smooth shadows for the
hills and 'skyscrapers'. Most other objects do not generate a shadow because they are too small
for the shadow map resolution.
Press <F4> to open the Options window where you can change shadow settings.",
    125)]
  public class VarianceShadowSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly VarianceShadow _varianceShadow;
    private readonly LightNode _lightNode;
    private Matrix33F _lastLightOrientation;
    private bool _updateShadowMap;


    public VarianceShadowSample(Microsoft.Xna.Framework.Game game)
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

      // Create test scene.
      ShadowSample.CreateScene(Services, ContentManager, _graphicsScreen);

      // Get directional light created by the DynamicSkyObject and replace the default 
      // shadow with our custom VSM shadow.
      _lightNode = _graphicsScreen.Scene.GetDescendants().OfType<LightNode>().First(n => n.Shadow is CascadedShadow);
      _varianceShadow = new VarianceShadow
      {
        // If a target area is set, the VSM covers the given area.
        // If no target area is set, the VSM covers the area in front of the camera.
        TargetArea = new Aabb(new Vector3F(-100, 0, -100), new Vector3F(100, 50, 100)),
      };
      _lightNode.Shadow = _varianceShadow;

      // Apply a blur filter to the shadow map.
      _varianceShadow.Filter = new Blur(GraphicsService);
      _varianceShadow.Filter.InitializeGaussianBlur(11, 3, false);

      // Register our custom shadow map and shadow mask renderers.
      _graphicsScreen.ShadowMapRenderer.Renderers.Add(new VarianceShadowMapRenderer(_graphicsScreen.ShadowMapRenderer.RenderCallback));
      _graphicsScreen.ShadowMaskRenderer.Renderers.Add(new VarianceShadowMaskRenderer(GraphicsService));

      CreateGuiControls();
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();

      // Check if the sun has moved.
      var newLightOrientation = _lightNode.PoseWorld.Orientation;
      _updateShadowMap |= (newLightOrientation != _lastLightOrientation);
      _lastLightOrientation = newLightOrientation;

      // Shadow map caching:
      // Only update the shadow map if the sun has moved or a parameter was changed.
      _varianceShadow.IsLocked = !_updateShadowMap;
      _updateShadowMap = false;
    }


    private void CreateGuiControls()
    {
      var optionsPanel = SampleFramework.AddOptions("Shadows");

      SampleHelper.AddSlider(
        optionsPanel,
        "Shadow map resolution",
        "F0",
        16,
        4096,
        _varianceShadow.PreferredSize,
        value =>
        {
          _varianceShadow.PreferredSize = (int)value;
          _varianceShadow.ShadowMap.SafeDispose();
          _varianceShadow.ShadowMap = null;
        });

      SampleHelper.AddCheckBox(
        optionsPanel,
        "Prefer 16 bit per channel",
        _varianceShadow.Prefer16Bit,
        isChecked =>
        {
          _varianceShadow.Prefer16Bit = isChecked;
          _varianceShadow.ShadowMap.SafeDispose();
          _varianceShadow.ShadowMap = null;
        });

      SampleHelper.AddSlider(
        optionsPanel,
        "Min variance",
        "F4",
        0,
        0.001f,
        _varianceShadow.MinVariance,
        value =>
        {
          _varianceShadow.MinVariance = value;
          _updateShadowMap = true;
        });

      SampleHelper.AddSlider(
        optionsPanel,
        "Light bleeding reduction",
        "F2",
        0, 0.999f,
        _varianceShadow.LightBleedingReduction,
        value =>
        {
          _varianceShadow.LightBleedingReduction = value;
          _updateShadowMap = true;
        });

      SampleHelper.AddCheckBox(
        optionsPanel,
        "Blur shadow map",
        _varianceShadow.Filter.Enabled,
        isChecked =>
        {
          _varianceShadow.Filter.Enabled = isChecked;
          _updateShadowMap = true;
        });

      SampleFramework.ShowOptionsWindow("Shadows");
    }
  }
}
#endif