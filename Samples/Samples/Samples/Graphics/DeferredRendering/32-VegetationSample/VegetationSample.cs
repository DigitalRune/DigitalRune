#if !WP7 && !WP8
using System.Collections.Generic;
using DigitalRune.Graphics.Effects;
using DigitalRune;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using System.Linq;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to use the MeshInstancingNode and custom effects to render many plant
instances which are swaying in the wind.",
    @"A custom game object 'WindObject' is used to generate a wind vector.
A custom game object class 'VegetationObject' is used to place plant mesh instances in the scene.
The instances are managed in several VegetationObjects. Each VegetationObject plants instances of
one mesh type in a grid. One MeshInstancingNode is created per grid cell. Only MeshInstancingNodes
within a certain max distance are added to the scene.

Three different plants are displayed. The meshes and effects can be found in the folder
<DIGITALRUNE_FOLDER/Samples/Content/Vegetation>.
The palm tree mesh and the smaller bird's nest plant use the *Vegetation.fx effects.
The bird's nest plant is a mesh with two levels of detail.
The grass mesh uses the *Grass.fx effects.

The effects uses a vertex shader which animates the vertices to create a swaying animation. This
method is described in Vegetation Procedural Animation and Shading in Crysis, GPU Gems 3, pp. 373
(http://http.developer.nvidia.com/GPUGems3/gpugems3_ch16.html).
The swaying animation consists of 3 parts:
1. The trunk animation bends the whole plant left and right.
2. The branch animation moves the branches and big leaves up and down.
3. The leaf animation adds a vertical flutter to the vertical edges of the big leaves.
The palm tree mesh and the bird's nest plant mesh use vertex colors to control the animation as
described in the article. However, the vertex colors are used a bit differently: The red color
defines the intensity of the up-down branch swaying. The green color is used to create a random
oscillation phase per branch/leave. The blue color defines the intensity of the horizontal leaf-edge
flutter.

The effects use screen door transparency (= alpha test with a dither pattern) to fade objects out.
The effects also add a simple translucency effect.

Important notes:
For swaying you need to increase bounding box of plant models. In this sample the bounding box
is manually set in the *.drmdl files of the models.

The MeshInstancingNode is not yet supported in MonoGame! Support for hardware instancing in MonoGame
will be added in the future.

The sample adds some GUI controls to the 'Game Object' and the 'Vegetation' tab of the Options
window.",
    132)]
  public class VegetationSample : Sample
  {
    private enum VertexColorChannel { None, Red, Green, Blue, };
    private VertexColorChannel _vertexColorChannel;

    private readonly DeferredGraphicsScreen _graphicsScreen;

    // Dictionary used to store the default values of effect parameters.
    private readonly Dictionary<EffectParameterBinding, Vector3> _defaultEffectParameterValues = new Dictionary<EffectParameterBinding, Vector3>();

    private bool _enableTranslucency = true;
    private float _windWaveFrequency = 0.2f;
    private float _windWaveRandomness = 0.3f;
    private float _trunkFrequencyMultiplier = 1;
    private float _branchFrequencyMultiplier = 1;
    private float _trunkScaleMultiplier = 1;
    private float _branchScaleMultiplier = 1;
    private float _leafScaleMultiplier = 1;
    private bool _drawDebugInfo;

    // A list of all used plant meshes.
    private readonly List<Mesh> _meshes = new List<Mesh>();


    public VegetationSample(Microsoft.Xna.Framework.Game game)
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
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      // Add standard game objects.
      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new DynamicSkyObject(Services, true, false, true));
      GameObjectService.Objects.Add(new GroundObject(Services));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new LavaBallsObject(Services));

      // Add a new game object which controls the wind velocity and "Wind" parameters in effects.
      GameObjectService.Objects.Add(new WindObject(Services));

#if MONOGAME    // TODO: Test if MonoGame supports effect annotations.
      // The vegetation effects use an effect parameter named "LodDistances". By default all
      // effect parameters are shared per "Material". However, we want to change the parameter
      // per instance. Therefore, the effect declares the parameter like this:
      // float3 LodDistances < string Hint = "PerInstance"; >;
      // However, MonoGame does not yet support effect annotations. Therefore, we tell the
      // graphics service that the "LodDistances" parameter should be stored per instance by
      // adding a new entry to the default effect interpreter.
      var defaultEffectInterpreter = GraphicsService.EffectInterpreters.OfType<DefaultEffectInterpreter>().First();
      if (!defaultEffectInterpreter.ParameterDescriptions.ContainsKey("LodDistances"))
        defaultEffectInterpreter.ParameterDescriptions.Add(
          "LodDistances",
          (parameter, i) => new EffectParameterDescription(parameter, "LodDistances", i, EffectParameterHint.PerInstance));
#endif

      // Load three different plant models.
      // The palm tree consists of a single mesh. It uses the *Vegetation.fx effects.
      ModelNode palmModelNode = ContentManager.Load<ModelNode>("Vegetation/PalmTree/palm_tree");
      Mesh palmMesh = ((MeshNode)palmModelNode.Children[0]).Mesh;

      // The bird's nest plant consists of 2 LODs. It uses the *Vegetation.fx effects.
      ModelNode plantModelNode = ContentManager.Load<ModelNode>("Vegetation/BirdnestPlant/BirdnestPlant");
      LodGroupNode plantLodGroupNode = plantModelNode.GetDescendants().OfType<LodGroupNode>().First().Clone();

      // The grass model consists of one mesh. It uses the *Grass.fx effects.
      ModelNode grassModelNode = ContentManager.Load<ModelNode>("Vegetation/Grass/grass");
      Mesh grassMesh = ((MeshNode)grassModelNode.Children[0]).Mesh;

      // Store all used meshes in a list for use in UpdateMaterialEffectParameters.
      _meshes.Add(palmMesh);
      foreach (var meshNode in plantLodGroupNode.Levels.Select(lodEntry => lodEntry.Node).OfType<MeshNode>())
        _meshes.Add(meshNode.Mesh);
      _meshes.Add(grassMesh);

      // We can add individual plant instances to the scene like this:
      // (However, this is inefficient for large amounts of plants.)
      _graphicsScreen.Scene.Children.Add(new MeshNode(palmMesh)
      {
        PoseLocal = new Pose(new Vector3F(-2, 0, 0))
      });
      plantLodGroupNode.PoseLocal = Pose.Identity;
      _graphicsScreen.Scene.Children.Add(plantLodGroupNode);
      _graphicsScreen.Scene.Children.Add(new MeshNode(grassMesh)
      {
        PoseLocal = new Pose(new Vector3F(2, 0, 0))
      });

#if WINDOWS
      int numberOfInstancesPerCell = 100;
#else
      int numberOfInstancesPerCell = 10;
#endif

      // It is more efficient to group instances in batches and render them using mesh instancing.
      // This is handled by the VegetationObject class.
      GameObjectService.Objects.Add(new VegetationObject(Services, palmMesh, numberOfInstancesPerCell, 20, 10, 10, 1)
      {
        Name = "PalmTrees"
      });

      // The bird's nest plant has 2 LODs. We create two VegetationObjects. One displays the
      // detailed meshes near the camera. The second displays the low-poly meshes in the distance.
      var plantMeshLod0 = ((MeshNode)plantLodGroupNode.Levels[0].Node).Mesh;
      _meshes.Add(plantMeshLod0);
      GameObjectService.Objects.Add(new VegetationObject(Services, plantMeshLod0, numberOfInstancesPerCell, 20, 10, 10, 2)
      {
        Name = "PlantLOD0",
        MaxDistance = plantLodGroupNode.Levels[1].Distance,
      });

      var plantMeshLod1 = ((MeshNode)plantLodGroupNode.Levels[1].Node).Mesh;
      _meshes.Add(plantMeshLod1);
      GameObjectService.Objects.Add(new VegetationObject(Services, plantMeshLod1, numberOfInstancesPerCell, 20, 10, 10, 2)
      {
        Name = "PlantLOD1",
        MinDistance = plantLodGroupNode.Levels[1].Distance,
        MaxDistance = plantLodGroupNode.MaxDistance,
        CastsShadows = false,  // No shadows in the distance.
      });

      // Grass, lots of it...
      GameObjectService.Objects.Add(new VegetationObject(Services, grassMesh, numberOfInstancesPerCell * 10, 10, 20, 20, 3)
      {
        Name = "Grass",
        MaxDistance = 30,
        CastsShadows = false,
      });

      CreateGuiControls();
    }


    private void CreateGuiControls()
    {
      var panel = SampleFramework.AddOptions("Vegetation");

      var swayPanel = SampleHelper.AddGroupBox(panel, "Swaying");

      SampleHelper.AddSlider(
        swayPanel,
        "Wind wave frequency",
        "F2",
        0,
        0.5f,
        _windWaveFrequency,
        value =>
        {
          _windWaveFrequency = value;
          UpdateMaterialEffectParameters();
        });

      SampleHelper.AddSlider(
        swayPanel,
        "Wind wave randomness",
        "F2",
        0,
        1,
        _windWaveRandomness,
        value =>
        {
          _windWaveRandomness = value;
          UpdateMaterialEffectParameters();
        });

      SampleHelper.AddSlider(
        swayPanel,
        "Trunk frequency multiplier",
        "F2",
        0,
        10,
        _trunkFrequencyMultiplier,
        value =>
        {
          _trunkFrequencyMultiplier = value;
          UpdateMaterialEffectParameters();
        });

      SampleHelper.AddSlider(
        swayPanel,
        "Branch frequency multiplier",
        "F2",
        0,
        10,
        _branchFrequencyMultiplier,
        value =>
        {
          _branchFrequencyMultiplier = value;
          UpdateMaterialEffectParameters();
        });

      SampleHelper.AddSlider(
        swayPanel,
        "Trunk scale multiplier",
        "F2",
        0,
        10,
        _trunkScaleMultiplier,
        value =>
        {
          _trunkScaleMultiplier = value;
          UpdateMaterialEffectParameters();
        });

      SampleHelper.AddSlider(
        swayPanel,
        "Branch scale multiplier",
        "F2",
        0,
        10,
        _branchScaleMultiplier,
        value =>
        {
          _branchScaleMultiplier = value;
          UpdateMaterialEffectParameters();
        });

      SampleHelper.AddSlider(
        swayPanel,
        "Leaf scale multiplier",
        "F2",
        0,
        10,
        _leafScaleMultiplier,
        value =>
        {
          _leafScaleMultiplier = value;
          UpdateMaterialEffectParameters();
        });

      SampleHelper.AddDropDown(
        swayPanel,
        "Render vertex color",
        EnumHelper.GetValues(typeof(VertexColorChannel)),
        0,
        item =>
        {
          _vertexColorChannel = (VertexColorChannel)item;
          UpdateMaterialEffectParameters();
        });

      SampleHelper.AddCheckBox(
        panel,
        "Enable translucency",
        true,
        isChecked =>
        {
          _enableTranslucency = isChecked;
          UpdateMaterialEffectParameters();
        });

      SampleHelper.AddCheckBox(
        panel,
        "Draw debug info",
        _drawDebugInfo,
        isChecked => _drawDebugInfo = isChecked);

      SampleFramework.ShowOptionsWindow("Vegetation");
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Unload content.
        // (We have changed the properties of some loaded materials. Other samples
        // should use the default values. When we unload them now, the next sample
        // will reload them with default values.)
        ContentManager.Unload();
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();
      if (_drawDebugInfo)
      {
        // Yellow bounding boxes.
        _graphicsScreen.DebugRenderer.DrawObjects(
          _graphicsScreen.Scene
                         .GetDescendants()
                         .Where(node => node is MeshNode || node is LodGroupNode)
                         .Cast<IGeometricObject>(),     // This cast is only required to compile on Xbox.
          Color.Yellow,
          true,
          false);
      }
    }


    /// <summary>
    /// Updates the material effect parameters for the vegetation and grass effects.
    /// </summary>
    private void UpdateMaterialEffectParameters()
    {
      // The parameters which are changed here are "Material" parameters (see enumeration
      // EffectParameterHint). That means, the effect parameters are shared per material. We
      // only need to update the effect parameter bindings of each mesh to update all mesh
      // instances. - On the downside, this means that two instances cannot use different
      // parameter values.

      // When this method is called the first time, the default values of certain effect
      // parameters are stored in a dictionary.

      foreach (var mesh in _meshes)
      {
        foreach (var material in mesh.Materials)
        {
          foreach (var entry in material)
          {
            var name = entry.Key;
            var effectBinding = entry.Value;

            // The DeferredGraphicsScreen uses following render passes:
            if (name == "GBuffer" || name == "ShadowMap" || name == "Material")
            {
              if (name == "Material")
              {
                // Update "TranslucencyColor".
                var translucencyColorBinding = (ConstParameterBinding<Vector3>)effectBinding.ParameterBindings["TranslucencyColor"];
                Vector3 defaultTranslucency;
                if (!_defaultEffectParameterValues.TryGetValue(translucencyColorBinding, out defaultTranslucency))
                {
                  defaultTranslucency = translucencyColorBinding.Value;
                  _defaultEffectParameterValues[translucencyColorBinding] = defaultTranslucency;
                }
                if (_enableTranslucency)
                  translucencyColorBinding.Value = defaultTranslucency;
                else
                  translucencyColorBinding.Value = new Vector3(0, 0, 0);

                // Update "VertexColorMask" (only used for debugging).
                if (effectBinding.ParameterBindings.Contains("VertexColorMask"))
                {
                  var vertexColorMaskBinding = (ConstParameterBinding<Vector3>)effectBinding.ParameterBindings["VertexColorMask"];
                  switch (_vertexColorChannel)
                  {
                    case VertexColorChannel.None:
                      vertexColorMaskBinding.Value = new Vector3(0, 0, 0);
                      break;
                    case VertexColorChannel.Red:
                      vertexColorMaskBinding.Value = new Vector3(1, 0, 0);
                      break;
                    case VertexColorChannel.Green:
                      vertexColorMaskBinding.Value = new Vector3(0, 1, 0);
                      break;
                    case VertexColorChannel.Blue:
                      vertexColorMaskBinding.Value = new Vector3(0, 0, 1);
                      break;
                  }
                }
              }

              // Update "WindWaveParameters".
              var windWaveParametersBinding = (ConstParameterBinding<Vector2>)effectBinding.ParameterBindings["WindWaveParameters"];
              windWaveParametersBinding.Value = new Vector2(_windWaveFrequency, _windWaveRandomness);

              // Update "SwayFrequencies".
              var swayFrequenciesBinding = (ConstParameterBinding<Vector3>)effectBinding.ParameterBindings["SwayFrequencies"];
              Vector3 defaultFrequencies;
              if (!_defaultEffectParameterValues.TryGetValue(swayFrequenciesBinding, out defaultFrequencies))
              {
                defaultFrequencies = swayFrequenciesBinding.Value;
                _defaultEffectParameterValues[swayFrequenciesBinding] = defaultFrequencies;
              }
              swayFrequenciesBinding.Value = new Vector3(
                defaultFrequencies.X * _trunkFrequencyMultiplier,
                defaultFrequencies.Y * _branchFrequencyMultiplier,
                0);   // not yet used in the effects.

              // Update "SwayScales".
              var swayScalesBinding = (ConstParameterBinding<Vector3>)effectBinding.ParameterBindings["SwayScales"];
              Vector3 defaultScales;
              if (!_defaultEffectParameterValues.TryGetValue(swayScalesBinding, out defaultScales))
              {
                defaultScales = swayScalesBinding.Value;
                _defaultEffectParameterValues[swayScalesBinding] = defaultScales;
              }
              swayScalesBinding.Value = new Vector3(
                defaultScales.X * _trunkScaleMultiplier,
                defaultScales.Y * _branchScaleMultiplier,
                defaultScales.Z * _leafScaleMultiplier);
            }
          }
        }
      }
    }
  }
}
#endif
