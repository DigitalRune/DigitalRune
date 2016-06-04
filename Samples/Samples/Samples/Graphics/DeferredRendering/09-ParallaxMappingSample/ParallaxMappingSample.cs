#if !WP7 && !WP8
using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample demonstrates Parallax Mapping and Parallax Occlusion Mapping.",
    @"The ground model can be used with Normal Mapping, Parallax Mapping (PM) and Parallax Occlusion 
Mapping (POM). 
Parallax mapping methods are used to show the bumpiness of the material. 
PM requires a height map which is sampled once per pixel. POM is more expensive and requires
many height map samples. POM can also compute self-shadowing.

Use the keyboard to switch between the materials and change material parameters.

For more info, have a look at the material and effect files of the loaded ground model.",
    109)]
  public class ParallaxMappingSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;

    private readonly MeshNode _meshNode;
    private readonly Material _normalMaterial;
    private readonly Material _parallaxMappingMaterial;
    private readonly Material _parallaxOcclusionMappingMaterial;

    private float _heightScale;
    private float _heightBias;
    private int _lodThreshold;
    private int _maxSamples;
    private float _shadowStrength;

    private int _currentMaterialIndex;


    public ParallaxMappingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
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

      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new DynamicSkyObject(Services, false, false, true));
      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));

      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-3, -8), 0, random.NextFloat(0, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree", scale, new Pose(position, orientation)));
      }

      // Load ground model which uses normal mapping.
      var modelNode = ContentManager.Load<ModelNode>("Parallax/Ground");
      _meshNode = modelNode.Children.OfType<MeshNode>().First().Clone();
      _meshNode.ScaleLocal = new Vector3F(0.1f);
      _meshNode.IsStatic = true;

      Debug.Assert(_meshNode.Mesh.Materials.Count == 1, "Mesh should have only one material.");

      // Load materials with normal mapping, parallax mapping and parallax occlusion mapping.
      _normalMaterial = ContentManager.Load<Material>("Parallax/Normal").Clone();
      _parallaxMappingMaterial = ContentManager.Load<Material>("Parallax/PM").Clone();
      _parallaxOcclusionMappingMaterial = ContentManager.Load<Material>("Parallax/POM").Clone();

      // Get default values from materials.
      var parameterBindings = _parallaxOcclusionMappingMaterial["Material"].ParameterBindings;
      _heightScale = ((ConstParameterBinding<float>)parameterBindings["HeightScale"]).Value;
      _heightBias = ((ConstParameterBinding<float>)parameterBindings["HeightBias"]).Value;
      _lodThreshold = ((ConstParameterBinding<int>)parameterBindings["LodThreshold"]).Value;
      _maxSamples = ((ConstParameterBinding<int>)parameterBindings["MaxSamples"]).Value;
      _shadowStrength = ((ConstParameterBinding<float>)parameterBindings["ShadowStrength"]).Value;

      // Start test with POM material.
      _currentMaterialIndex = 2;
      UpdateMesh();

      // Add nodes to scene graph.
      _graphicsScreen.Scene.Children.Add(_meshNode);

      // Create rigid body for ground plane.
      Simulation.RigidBodies.Add(new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        MotionType = MotionType.Static,
      });
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();

      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // Material type
      if (InputService.IsPressed(Keys.N, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;

        if (isShiftDown)
          _currentMaterialIndex++;
        else
          _currentMaterialIndex--;

        if (_currentMaterialIndex > 2)
          _currentMaterialIndex = 0;
        else if (_currentMaterialIndex < 0)
          _currentMaterialIndex = 2;

        UpdateMesh();
      }

      // Height scale
      if (InputService.IsDown(Keys.H))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float delta = sign * deltaTime * 0.05f;
        _heightScale = Math.Max(0, _heightScale + delta);
        UpdateMesh();
      }
      
      // Height bias
      if (InputService.IsDown(Keys.J))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float delta = sign * deltaTime * 0.05f;
        _heightBias = _heightBias + delta;
        UpdateMesh();
      }

      // LOD threshold
      if (InputService.IsPressed(Keys.K, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        int delta = isShiftDown ? +1 : -1;
        _lodThreshold = Math.Max(0, _lodThreshold + delta);
        UpdateMesh();
      }

      // Max number of samples
      if (InputService.IsPressed(Keys.L, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        int delta = isShiftDown ? +1 : -1;  
        _maxSamples = Math.Max(0, _maxSamples + delta);
        UpdateMesh();
      }

      // Shadow strength
      if (InputService.IsDown(Keys.I))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float delta = sign * deltaTime * 40.0f;
        _shadowStrength = Math.Max(0, _shadowStrength + delta);
        UpdateMesh();
      }

      string materialName;
      if (_currentMaterialIndex == 0)
        materialName = "Normal Mapping";
      else if (_currentMaterialIndex == 1)
        materialName = "Parallax Mapping";
      else
        materialName = "Parallax Occlusion Mapping";

      _graphicsScreen.DebugRenderer.DrawText("\n\nPress <N> or <Shift>+<N> to switch material: " + materialName
        + "\nHold <H> or <Shift>+<H> to decrease or increase the height scale: " + _heightScale
        + "\nHold <J> or <Shift>+<J> to decrease or increase the height bias: " + _heightBias
        + "\n\nPOM only:"
        + "\nPress <K> or <Shift>+<K> to decrease or increase the LOD threshold: " + _lodThreshold
        + "\nPress <L> or <Shift>+<L> to decrease or increase the max number of samples: " + _maxSamples
        + "\nPress <I> or <Shift>+<I> to decrease or increase the self-shadow strength: " + _shadowStrength);
    }


    // Updates the material the mesh and the material parameters.
    private void UpdateMesh()
    {
      switch (_currentMaterialIndex)
      {
        case 0:
          _meshNode.Mesh.Materials[0] = _normalMaterial;
          break;
        case 1:
          _meshNode.Mesh.Materials[0] = _parallaxMappingMaterial;
          break;
        case 2:
          _meshNode.Mesh.Materials[0] = _parallaxOcclusionMappingMaterial;
          break;
      }

      // The MeshNode is not automatically notified when the properties of the
      // Mesh change. The Mesh needs to be re-assigned to update the MeshNode!
      _meshNode.Mesh = _meshNode.Mesh;

      var gBufferPass = _parallaxMappingMaterial["GBuffer"];
      gBufferPass.Set("HeightScale", _heightScale);
      gBufferPass.Set("HeightBias", _heightBias);

      var materialPass = _parallaxMappingMaterial["Material"];
      materialPass.Set("HeightScale", _heightScale);
      materialPass.Set("HeightBias", _heightBias);

      gBufferPass = _parallaxOcclusionMappingMaterial["GBuffer"];
      gBufferPass.Set("HeightScale", _heightScale);
      gBufferPass.Set("HeightBias", _heightBias);
      gBufferPass.Set("LodThreshold", _lodThreshold);
      gBufferPass.Set("MaxSamples", _maxSamples);

      materialPass = _parallaxOcclusionMappingMaterial["Material"];
      materialPass.Set("HeightScale", _heightScale);
      materialPass.Set("HeightBias", _heightBias);
      materialPass.Set("LodThreshold", _lodThreshold);
      materialPass.Set("MaxSamples", _maxSamples);
      materialPass.Set("ShadowStrength", _shadowStrength);
    }
  }
}
#endif