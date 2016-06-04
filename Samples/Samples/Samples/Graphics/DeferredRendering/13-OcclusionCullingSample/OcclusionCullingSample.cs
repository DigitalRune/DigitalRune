#if !WP7 && !WP8
using System.Text;
using DigitalRune.Geometry;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples shows how to implement occlusion culling using the OcclusionBuffer
to improve performance in complex scenes.",
    @"Occlusion culling can reduce rendering time by removing objects that do not contribute
to the final image. Occlusion culling requires a set of occluders. Occluders need to be 
added manually to the scene. In this example, the models loaded via the XNA content pipeline
already include occluders, which have been added in a 3D modeling tool.
Occlusion culling has a relatively large CPU/GPU overhead and should therefore only be used
if the performance gains are larger than the processing overhead.

Please keep in mind that the current level is not representative of an actual game level.
(Each building consists of many individual mesh nodes. For a practical game level, meshes 
should be merged to drastically reduce the number of scene nodes.)",
    113)]
  [Controls(@"Sample
  Press <C> to toggle occlusion culling. Notice the frame rate changes.
  Press <P> to toggle progressive shadow caster culling. Faster, but can cause shadow flickering!
  Press <Space> to toggle top-down view of scene.
  Debug: Press <V> to visualize the camera HZB. <1> ... <8> to switch HZB level.
  Debug: Press <L> to visualize the light HZB. <1> ... <8> to switch HZB level.
  Debug: Press <B> to visualize the occlusion query of a specific scene node.
  Debug: Press <N> to visualize the occlusion query of a shadow caster. (Conservative culling)
  Debug: Press <M> to visualize the occlusion query of a shadow volume. (Progressive culling)")]
  public class OcclusionCullingSample : Sample
  {
    private readonly OcclusionCullingScreen _graphicsScreen;
    private readonly StringBuilder _stringBuilder;


    public OcclusionCullingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // Occlusion culling is implemented in the OcclusionCullingScreen.
      _graphicsScreen = new OcclusionCullingScreen(Services);
      GraphicsService.Screens.Insert(0, _graphicsScreen);

      Services.Register(typeof(DebugRenderer), null, _graphicsScreen.DebugRenderer);
      Services.Register(typeof(IScene), null, _graphicsScreen.Scene);

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      // Load environment.
      GameObjectService.Objects.Add(new StaticSkyObject(Services)); // (includes light nodes)
      GameObjectService.Objects.Add(new StaticObject(Services, "Gravel/Gravel", 1, new Pose(new Vector3F(0, 0, 0))));

      // Set main directional light to enable shadow caster culling.
      _graphicsScreen.LightNode = ((LightNode)_graphicsScreen.Scene.GetSceneNode("Sunlight"));

      // To disable shadow maps and shadow caster culling:
      //((LightNode)_graphicsScreen.Scene.GetSceneNode("Sunlight")).Shadow = null;
      // _graphicsScreen.LightNode = null;
      
      // ----- Load building:
      // The model "Building/Building.fbx" represent a building that is assembled
      // from modular building blocks:
      //   "Building/concrete_door.fbx"
      //   "Building/concrete_double_window.fbx"
      //   "Building/concrete_small_double_window.fbx"
      //   "Building/wall_concrete_1.fbx"
      //   "Building/pillar_concrete_1.fbx"
      //   ...
      //
      // "Building/Building.fbx" only contains place holders (empty nodes) that
      // reference the building blocks. The building blocks are referenced by name.
      // Example: A place holder with name "concrete_door_001" is an instance of
      // "Building/concrete_door.fbx".
      var buildingNode = LoadBuilding("Building/", "Building");

      // Clone the building several times and add it to the scene.
#if XBOX
      const int numberOfColumns = 4;
      const int numberOfRows = 4;
#else
      const int numberOfColumns = 10;
      const int numberOfRows = 10;
#endif
      for (int i = 0; i < numberOfColumns; i++)
      {
        for (int j = 0; j < numberOfRows; j++)
        {
          var clone = buildingNode.Clone();
          clone.PoseWorld = new Pose(new Vector3F((i - numberOfColumns / 2) * 20, 0, (j - numberOfRows / 2) * 20));
          _graphicsScreen.Scene.Children.Add(clone);
        }
      }

      // For debug visualization:
      // Selects the object that is visualized with <B>, <N>, <M>.
      _graphicsScreen.DebugObject = _graphicsScreen.Scene.GetSceneNode("pillar_concrete_3");

      _stringBuilder = new StringBuilder();
    }


    private SceneNode LoadBuilding(string path, string level)
    {
      // Load the model that defines the building using place holders (empty nodes).
      var levelModel = ContentManager.Load<ModelNode>(path + level);

      // Iterate over all place holders and load the actual models.
      var buildingNode = new SceneNode();
      buildingNode.Children = new SceneNodeCollection();
      foreach (var placeholder in levelModel.Children)
      {
        var block = LoadBlock(placeholder, path);
        if (block != null)
          buildingNode.Children.Add(block);
      }

      return buildingNode;
    }


    private SceneNode LoadBlock(SceneNode placeholder, string path)
    {
      var node = TryLoadAsset(path, placeholder.Name);
      if (node != null)
      {
        // The modular building blocks usually consist of:
        //
        //    ModelNode
        //       |
        //    MeshNode
        //
        // We usually don't need the model node. => Skip it.
        if (node is ModelNode && node.Children != null && node.Children.Count == 1)
          node = node.Children[0];

        node = node.Clone();
        node.PoseWorld = placeholder.PoseWorld;
      }

      return node;
    }


    private SceneNode TryLoadAsset(string path, string name)
    {
      SceneNode node = null;
      try
      {
        string assetName = GetAssetName(name);
        node = ContentManager.Load<ModelNode>(path + assetName);
      }
      catch
      {
        // Model not found. Ignore.
      }

      return node;
    }


    private string GetAssetName(string placeholder)
    {
      // The name of the scene node identifies the name of the model to be loaded.
      // When multiple instances of the same model are used a 3-digit number is
      // attached to the name.
      // Example:
      //  Place holders: "brick_window_1", "brick_window_1_001", "brick_window_1_002", ...
      //  Asset name: "brick_window_1"
      if (placeholder.Length > 4 
          && placeholder[placeholder.Length - 4] == '_' 
          && char.IsDigit(placeholder[placeholder.Length - 3])
          && char.IsDigit(placeholder[placeholder.Length - 2])
          && char.IsDigit(placeholder[placeholder.Length - 1]))
      {
        // Remove number "_001" at end.
        return placeholder.Substring(0, placeholder.Length - 4);
      }

      return placeholder;
    }


    public override void Update(GameTime gameTime)
    {
      if (InputService.IsPressed(Keys.C, true))
      {
        _graphicsScreen.EnableCulling = !_graphicsScreen.EnableCulling;
        if (!_graphicsScreen.EnableCulling)
          _graphicsScreen.ResetShadowCasters();
      }
      else if (InputService.IsPressed(Keys.P, false))
        _graphicsScreen.OcclusionBuffer.ProgressiveShadowCasterCulling = !_graphicsScreen.OcclusionBuffer.ProgressiveShadowCasterCulling;
      else if (InputService.IsPressed(Keys.Space, false))
        _graphicsScreen.ShowTopDownView = !_graphicsScreen.ShowTopDownView;
      else if (InputService.IsPressed(Keys.V, false))
        _graphicsScreen.ToggleVisualization(DebugVisualization.CameraHzb);
      else if (InputService.IsPressed(Keys.L, false))
        _graphicsScreen.ToggleVisualization(DebugVisualization.LightHzb);
      else if (InputService.IsPressed(Keys.B, false))
        _graphicsScreen.ToggleVisualization(DebugVisualization.Object);
      else if (InputService.IsPressed(Keys.N, false))
        _graphicsScreen.ToggleVisualization(DebugVisualization.ShadowCaster);
      else if (InputService.IsPressed(Keys.M, false))
        _graphicsScreen.ToggleVisualization(DebugVisualization.ShadowVolume);
      else if (InputService.IsPressed(Keys.D1, false))
        _graphicsScreen.DebugLevel = 0;
      else if (InputService.IsPressed(Keys.D2, false))
        _graphicsScreen.DebugLevel = 1;
      else if (InputService.IsPressed(Keys.D3, false))
        _graphicsScreen.DebugLevel = 2;
      else if (InputService.IsPressed(Keys.D4, false))
        _graphicsScreen.DebugLevel = 3;
      else if (InputService.IsPressed(Keys.D5, false))
        _graphicsScreen.DebugLevel = 4;
      else if (InputService.IsPressed(Keys.D6, false))
        _graphicsScreen.DebugLevel = 5;
      else if (InputService.IsPressed(Keys.D7, false))
        _graphicsScreen.DebugLevel = 6;
      else if (InputService.IsPressed(Keys.D8, false))
        _graphicsScreen.DebugLevel = 7;

      _stringBuilder.Clear();
      _stringBuilder.Append("Culling: ");
      _stringBuilder.Append(_graphicsScreen.EnableCulling ? "Enabled" : "Disabled");
      _stringBuilder.AppendLine();

      _stringBuilder.Append("Shadow Caster Culling: ");
      _stringBuilder.Append(_graphicsScreen.OcclusionBuffer.ProgressiveShadowCasterCulling ? "Progressive" : "Conservative");
      _stringBuilder.AppendLine();

      var statistics = _graphicsScreen.OcclusionBuffer.Statistics;
      _stringBuilder.Append("Before occlusion culling: ");
      _stringBuilder.AppendNumber(statistics.ObjectsTotal);
      _stringBuilder.AppendLine();
      _stringBuilder.Append("After occlusion culling: ");
      _stringBuilder.AppendNumber(statistics.ObjectsVisible);
      _stringBuilder.AppendLine();
      _stringBuilder.Append("Number of shadow casters: ");
      _stringBuilder.AppendNumber(statistics.ShadowCastersTotal);
      _stringBuilder.AppendLine();
      _stringBuilder.Append("Visible shadow casters: ");
      _stringBuilder.AppendNumber(statistics.ShadowCastersVisible);
      _stringBuilder.AppendLine();

      _graphicsScreen.DebugRenderer.Clear();
      _graphicsScreen.DebugRenderer.DrawText(_stringBuilder, new Vector2F(10, 40), new Color(0, 255, 0));
    }
  }
}
#endif