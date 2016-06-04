#if !WP7 && !WP8
using System.Collections.Generic;
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
    @"This sample shows how to use a CompositeShadow.",
    @"A CompositeShadow is used to combine three shadow maps:
- A VarianceShadow creates smooth shadows of the distant hills and skyscrapers.
- A CascadedShadow creates shadows of static objects over a large distance.
- A CascadedShadow creates shadows of dynamic objects over a small distance.
Each shadow casting object is rendered in only one of these shadow maps using a
custom render callback.

Shadow map caching: To improve performance the VSM is only updated when the sun moves.
The CSM cascades are updated in different frames.
Press <F4> to show the Options window where you can disable shadow map caching.",
    126)]
  public class CompositeShadowSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;

    private readonly VarianceShadow _vsmShadow;
    private readonly CascadedShadow _staticCascadedShadow;
    private readonly CascadedShadow _dynamicCascadedShadow;

    private bool _enableShadowMapCaching = true;
    private Matrix33F _lastLightOrientation;
    private LightNode _lightNode;
    private List<SceneNode> _tempList = new List<SceneNode>();


    public CompositeShadowSample(Microsoft.Xna.Framework.Game game)
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

      // Create 3 different shadows:
      // The VarianceShadow covers the whole level.
      _vsmShadow = new VarianceShadow
      {
        Prefer16Bit = false,
        PreferredSize = 512,
        MinLightDistance = 200,
        MaxDistance = 200,
        FadeOutRange = 0,
        ShadowFog = 0,
        TargetArea = new Aabb(new Vector3F(-100, 0, -100), new Vector3F(100, 50, 100))
      };
      _vsmShadow.Filter = new Blur(GraphicsService);
      _vsmShadow.Filter.NumberOfPasses = 1;
      _vsmShadow.Filter.InitializeGaussianBlur(11, 3, false);

      // The CascadedShadow for static objects.
      _staticCascadedShadow = new CascadedShadow
      {
        PreferredSize = 1024,
        Prefer16Bit = true,
        Distances = new Vector4F(4, 12, 20, 80),
        MinLightDistance = 200,
      };

      // The CascadedShadow for dynamic objects covering a smaller distance.
      _dynamicCascadedShadow = new CascadedShadow
      {
        PreferredSize = 1024,
        Prefer16Bit = true,
        NumberOfCascades = 2,
        Distances = new Vector4F(4, 12, 0, 0),
        MinLightDistance = 200,
      };

      // Get directional light created by the DynamicSkyObject and replace the default
      // shadow with our a CompositeShadow.
      _lightNode = _graphicsScreen.Scene.GetDescendants().OfType<LightNode>().First(n => n.Shadow is CascadedShadow);
      _lightNode.Shadow = new CompositeShadow
      {
        Shadows =
        {
          _vsmShadow,
          _staticCascadedShadow,
          _dynamicCascadedShadow,
        }
      };

      // We do not want to render the same objects into all 3 shadow maps. We use a custom
      // render callback to render each objects into only one of the shadow maps.
      _graphicsScreen.ShadowMapRenderer.RenderCallback = context =>
      {
        // Query all shadow casters.
        var query = context.Scene.Query<ShadowCasterQuery>(context.CameraNode, context);
        if (query.ShadowCasters.Count == 0)
          return false;

        // Get the shadow which is currently being rendered.
        var shadow = context.Object;

        // Create a list of scene nodes for the current shadow.
        var list = _tempList;
        if (shadow == _vsmShadow)
        {
          // Get the hills and skyscrapers which have been marked with a user flag 
          // in ShadowSample.CreateScene.
          foreach (var node in query.ShadowCasters)
            if (node.UserFlags == 1)
              _tempList.Add(node);
        }
        else if (shadow == _staticCascadedShadow)
        {
          // Get all static objects except the hills/skyscrapers.
          foreach (var node in query.ShadowCasters)
            if (node.UserFlags == 0 && node.IsStatic)
              _tempList.Add(node);
        }
        else if (shadow == _dynamicCascadedShadow)
        {
          // Get all dynamic objects.
          foreach (var node in query.ShadowCasters)
            if (!node.IsStatic)
              _tempList.Add(node);
        }
        else
        {
          // Other shadows of other lights.
          list = query.ShadowCasters;
        }

        // Render the selected objects into the shadow map.
        _graphicsScreen.MeshRenderer.Render(list, context);

        _tempList.Clear();
        return true;
      };

      // Register the custom renderers for the VarianceShadow.
      _graphicsScreen.ShadowMapRenderer.Renderers.Add(new VarianceShadowMapRenderer(_graphicsScreen.ShadowMapRenderer.RenderCallback));
      _graphicsScreen.ShadowMaskRenderer.Renderers.Add(new VarianceShadowMaskRenderer(GraphicsService));

      CreateGuiControls();
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();

      if (_enableShadowMapCaching)
      {
        // Shadow map caching: 
        // Update VSM shadow only when light has moved.
        var newLightOrientation = _lightNode.PoseWorld.Orientation;
        _vsmShadow.IsLocked = (newLightOrientation == _lastLightOrientation);
        _lastLightOrientation = newLightOrientation;

        // Update one static shadow cascade per frame.
        for (int i = 0; i < _staticCascadedShadow.NumberOfCascades; i++)
          _staticCascadedShadow.IsCascadeLocked[i] = ((GraphicsService.Frame % _staticCascadedShadow.NumberOfCascades) != i);

        // Update one static dynamic cascade per frame.
        for (int i = 0; i < _dynamicCascadedShadow.NumberOfCascades; i++)
          _dynamicCascadedShadow.IsCascadeLocked[i] = ((GraphicsService.Frame % _dynamicCascadedShadow.NumberOfCascades) != i);
      }
      else
      {
        // No caching: Update shadow maps every frame.
        _vsmShadow.IsLocked = false;
        for (int i = 0; i < 4; i++)
        {
          _staticCascadedShadow.IsCascadeLocked[i] = false;
          _dynamicCascadedShadow.IsCascadeLocked[i] = false;
        }
      }
    }


    private void CreateGuiControls()
    {
      var panel = SampleFramework.AddOptions("Shadows");

      SampleHelper.AddCheckBox(
        panel,
        "Enable shadow map caching",
        _enableShadowMapCaching,
        isChecked => _enableShadowMapCaching = isChecked);

      SampleFramework.ShowOptionsWindow("Shadows");
    }
  }
}
#endif