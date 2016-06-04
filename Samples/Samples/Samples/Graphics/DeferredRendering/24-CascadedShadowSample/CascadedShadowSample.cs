using DigitalRune;
#if !WP7 && !WP8
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample allows to test Cascaded Shadow Maps for sun shadows.",
    @"Press <F4> to open the Options window where you can change shadow settings.
This sample also demonstrates shadow map caching: It is not necessary to render the shadow map of
each cascade every frame. This can be controlled using the CascadedShadow.IsCascadeLocked flags. If
a cascade is locked frame then the cached shadow map of the last frame is used. This gives full
control over the cascade updates. To improve performance it might be useful to distribute cascaded
updates over several frames and to update distant cascades less often.",
    124)]
  public class CascadedShadowSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly CascadedShadow _cascadedShadow;

    // Shadow map caching:
    // Counts the number of skipped frames for each cascade.
    private readonly int[] _currentSkippedFrames = new int[4];
    // Stores the desired number of skipped frames for each cascade.
    // For example: 0 = update each frame, 1 = update every second frame, etc.
    private readonly int[] _targetSkippedFrames = new int[4];


    public CascadedShadowSample(Microsoft.Xna.Framework.Game game)
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

      ShadowSample.CreateScene(Services, ContentManager, _graphicsScreen);

      // Get the cascaded shadow of the sunlight (which was created by the DynamicSkyObject).
      _cascadedShadow = (CascadedShadow)((LightNode)_graphicsScreen.Scene.GetSceneNode("Sunlight")).Shadow;

      CreateGuiControls();
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();

      // To demonstrate shadow map caching, simply skip some frames.
      for (int cascade = 0; cascade < 4; cascade++)
      {
        if (_currentSkippedFrames[cascade] < _targetSkippedFrames[cascade])
        {
          _currentSkippedFrames[cascade]++;
          _cascadedShadow.IsCascadeLocked[cascade] = true;
        }
        else
        {
          _currentSkippedFrames[cascade] = 0;
          _cascadedShadow.IsCascadeLocked[cascade] = false;
        }
      }
    }


    private void CreateGuiControls()
    {
      var optionsPanel = SampleFramework.AddOptions("Shadows");

      // ----- Shadow controls
      var shadowPanel = SampleHelper.AddGroupBox(optionsPanel, "Shadow");

      SampleHelper.AddSlider(
        shadowPanel,
        "Shadow map resolution",
        "F0",
        16,
        1024,
        _cascadedShadow.PreferredSize,
        value =>
        {
          _cascadedShadow.PreferredSize = (int)value;
          _cascadedShadow.ShadowMap.SafeDispose();
          _cascadedShadow.ShadowMap = null;
        });

      SampleHelper.AddCheckBox(
       shadowPanel,
       "Prefer 16 bit",
       _cascadedShadow.Prefer16Bit,
        isChecked =>
        {
          _cascadedShadow.Prefer16Bit = isChecked;
          _cascadedShadow.ShadowMap.SafeDispose();
          _cascadedShadow.ShadowMap = null;
        });

      SampleHelper.AddSlider(
        shadowPanel,
        "Number of cascades",
        "F0",
        1,
        4,
        _cascadedShadow.NumberOfCascades,
        value =>
        {
          _cascadedShadow.NumberOfCascades = (int)value;
          switch ((int)value)
          {
            case 1: _cascadedShadow.Distances = new Vector4F(80); break;
            case 2: _cascadedShadow.Distances = new Vector4F(20, 80, 80, 80); break;
            case 3: _cascadedShadow.Distances = new Vector4F(12, 20, 80, 80); break;
            case 4: _cascadedShadow.Distances = new Vector4F(4, 12, 20, 80); break;
          }
          _cascadedShadow.ShadowMap.SafeDispose();
          _cascadedShadow.ShadowMap = null;
        });

      SampleHelper.AddSlider(
        shadowPanel,
        "Depth bias",
        "F2",
        0,
        10,
        _cascadedShadow.DepthBias.X,
        value => _cascadedShadow.DepthBias = new Vector4F(value));

      SampleHelper.AddSlider(
        shadowPanel,
        "Normal offset",
        "F2",
        0,
        10,
        _cascadedShadow.NormalOffset.X,
        value => _cascadedShadow.NormalOffset = new Vector4F(value));

      SampleHelper.AddSlider(
        shadowPanel,
        "Number of samples",
        "F0",
        -1,
        32,
        _cascadedShadow.NumberOfSamples,
        value => _cascadedShadow.NumberOfSamples = (int)value);

      SampleHelper.AddSlider(
        shadowPanel,
        "Filter radius",
        "F2",
        0,
        10,
        _cascadedShadow.FilterRadius,
        value => _cascadedShadow.FilterRadius = value);

      SampleHelper.AddSlider(
        shadowPanel,
        "Jitter resolution",
        "F0",
        1,
        10000,
        _cascadedShadow.JitterResolution,
        value => _cascadedShadow.JitterResolution = value);

      SampleHelper.AddSlider(
        shadowPanel,
        "Fade-out range",
        "F2",
        0,
        1,
        _cascadedShadow.FadeOutRange,
        value => _cascadedShadow.FadeOutRange = value);

      SampleHelper.AddSlider(
        shadowPanel,
        "Shadow fog",
        "F2",
        0,
        1,
        _cascadedShadow.ShadowFog,
        value => _cascadedShadow.ShadowFog = value);

      SampleHelper.AddDropDown(
        shadowPanel,
        "Cascade split selection",
        new[] { ShadowCascadeSelection.Fast, ShadowCascadeSelection.Best, ShadowCascadeSelection.BestDithered, },
        (int)_cascadedShadow.CascadeSelection,
        item => _cascadedShadow.CascadeSelection = item);

      SampleHelper.AddCheckBox(
        shadowPanel,
        "Visualize cascades",
        _cascadedShadow.VisualizeCascades,
        isChecked => _cascadedShadow.VisualizeCascades = isChecked);

      // ----- Shadow map caching
      var cachingPanel = SampleHelper.AddGroupBox(optionsPanel, "Shadow map caching");

      cachingPanel.Children.Add(new TextBlock
      {
        Text = "Set a value > 0 to update shadow map less frequently.",
        Margin = new Vector4F(0, 0, SampleHelper.Margin, 0),
      });

      SampleHelper.AddSlider(
        cachingPanel,
        "Cascade 0",
        "F0",
        0,
        10,
        _targetSkippedFrames[0],
        value => _targetSkippedFrames[0] = (int)value);

      SampleHelper.AddSlider(
        cachingPanel,
        "Cascade 1",
        "F0",
        0,
        10,
        _targetSkippedFrames[1],
        value => _targetSkippedFrames[1] = (int)value);

      SampleHelper.AddSlider(
        cachingPanel,
        "Cascade 2",
        "F0",
        0,
        10,
        _targetSkippedFrames[2],
        value => _targetSkippedFrames[2] = (int)value);

      SampleHelper.AddSlider(
        cachingPanel,
        "Cascade 3",
        "F0",
        0,
        10,
        _targetSkippedFrames[3],
        value => _targetSkippedFrames[3] = (int)value);

      SampleFramework.ShowOptionsWindow("Shadows");
    }
  }
}
#endif