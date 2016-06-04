#if !WP7 && !WP8
using DigitalRune;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using System;
using System.Linq;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to use batching and hardware instancing to render many instances of
a mesh efficiently.",
    @"This sample demonstrates several strategies which can be used to render many mesh instances:
A) No batching:
   Each instance is represented by a MeshNode. The MeshRenderer renders meshes individually, which
   requires one draw call per instance. This method is slow.
B) Batching using dynamic hardware instancing:
   Each instance is represented by a MeshNode. When the mesh nodes are rendered, the MeshRenderer
   checks whether the materials (effect files) support hardware instancing. If the materials support
   hardware instancing, the MeshRenderer can render all instances of the same mesh with a single
   draw call.
C) Batching using static hardware instancing:
   The class MeshInstancingNode<T> represents a mesh and a collection of mesh instances. A single
   MeshInstancingNode<T> can be used instead of storing individual MeshNodes. When the MeshRenderer
   renders a MeshInstancingNode<T> it renders all instances using only a single draw call. This is
   faster than strategy B because the graphics resources for hardware instancing only need to be
   created once (or whenever MeshInstancingNode<T>.Instances is modified).
D) Static batching:
   The MeshHelper can be used to merge all instances of a mesh into a new mesh. This creates one big
   vertex buffer with the pre-transformed vertices.

Important: B and C are not yet supported in MonoGame! Support for hardware instancing in MonoGame
will be added in the future.

Remarks:
- Strategies C and D are the fastest. It is recommended to use D for small meshes (e.g. grass
  billboards) and C for large meshes (e.g. trees).
- B and C require HiDef graphics profile and cannot be used with the Reach graphics profile.
- For B and C the material effects must implement hardware instancing. If the effect does not
  support hardware instancing, then strategy B renders the mesh correctly but without hardware
  instancing. Strategy C will render the mesh incorrectly. This can be observed with the glass box
  which is rendered incorrectly with strategy C because the material effect of the glass does not
  support instancing.
- C and D cannot be used with depth sorting. That means, transparent objects will be blended in 
  random order and not back-to-front. This can be observed with the glass box.
- C does not support occluders.
- No batching strategy can be used with skinned meshes or morphing (at the moment).

Debug drawing can be enabled in the Options window (F4) in the Batching tab:
- Bounding shapes are drawn in yellow.
- Occluders (inside the wall pieces) are drawn in green.",
    131)]
  public class BatchingSample : Sample
  {
    private enum BatchingType
    {
      NoBatching,
      DynamicHardwareInstancing,
      StaticHardwareInstancing,
      StaticBatching,
    };


    private BatchingType _batchingType;

    private readonly DeferredGraphicsScreen _graphicsScreen;

    // A scene graph containing many individual mesh nodes (strategies A and B).
    private SceneNode _originalMeshNodes;

    // A scene graph containing InstancingMeshNodes (strategy C).
    private SceneNode _staticInstancingNodes;

    // A scene graph containing only a few merged meshes (strategy D).
    private SceneNode _staticBatchingNodes;

    private bool _drawDebugInfo;


    public BatchingSample(Microsoft.Xna.Framework.Game game)
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

      // Create the scene data for the different batching strategies.
      CreateScene();

      // Choose the initially selected batching strategy.
      SetBatchingType(BatchingType.StaticBatching);

      CreateGuiControls();
    }


    private void CreateScene()
    {
      var random = new Random(1234567);

      // 3 empty scene nodes act as the root nodes for 3 different scene graphs.
      _originalMeshNodes = new SceneNode { Children = new SceneNodeCollection() };
      _staticInstancingNodes = new SceneNode { Children = new SceneNodeCollection() };
      _staticBatchingNodes = new SceneNode { Children = new SceneNodeCollection() };

      // Load several model nodes. Each of these models contains only one mesh node.
      // Store the mesh nodes in a list.
      MeshNode[] meshNodePrototypes =
      {
        (MeshNode)ContentManager.Load<ModelNode>("Grass/Grass").Children[0],
        (MeshNode)ContentManager.Load<ModelNode>("Parviflora/Parviflora").Children[0],
        (MeshNode)ContentManager.Load<ModelNode>("PalmTree/palm_tree").Children[0],
        (MeshNode)ContentManager.Load<ModelNode>("Rock/rock_05").Children[0],
        (MeshNode)ContentManager.Load<ModelNode>("GlassBox/GlassBox").Children[0],
        (MeshNode)ContentManager.Load<ModelNode>("Building/wall_concrete_1").Children[0],
      };

      for (int meshIndex = 0; meshIndex < meshNodePrototypes.Length; meshIndex++)
      {
        var meshNode = meshNodePrototypes[meshIndex];
        var mesh = meshNode.Mesh;
#if WINDOWS
        int numberOfInstances = (meshIndex == 0) ? 5000 : 200;
#else
        int numberOfInstances = (meshIndex == 0) ? 1000 : 50;
#endif

        int extent = (meshIndex < 2) ? 20 : 100;

        // Create a list of random scales and poses.
        var scales = new Vector3F[numberOfInstances];
        var poses = new Pose[numberOfInstances];
        for (int i = 0; i < numberOfInstances; i++)
        {
          // Combine a random scale with the original scale of the mesh.
          scales[i] = new Vector3F(random.NextFloat(0.5f, 1.2f)) * meshNode.ScaleWorld;

          // Combine a random pose with the original pose of the mesh.
          Vector3F position = new Vector3F(random.NextFloat(-extent, extent), 0, random.NextFloat(-extent, extent));
          Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));

          poses[i] = new Pose(position, orientation) * meshNode.PoseLocal;
        }

        // Strategy A or B: Create a MeshNode for each mesh instance.
        for (int i = 0; i < numberOfInstances; i++)
        {
          var clone = meshNode.Clone();
          clone.ScaleLocal = scales[i];
          clone.PoseLocal = poses[i];
          _originalMeshNodes.Children.Add(clone);
        }

        // Strategy C: Create one MeshInstancingNode which contains all instances for one mesh.
        var instances = new InstanceData[numberOfInstances];
        for (int i = 0; i < numberOfInstances; i++)
          instances[i] = new InstanceData(scales[i], poses[i], new Vector4F(1));

        // Create MeshInstancingNode.
        var instancingMeshNode = new MeshInstancingNode<InstanceData>(mesh, instances)
        {
          // It is recommended to manually set a suitable pose and shape, so that
          // the bounding shape contains all instances.
          PoseLocal = new Pose(new Vector3F(0, 2, 0)),
          Shape = new BoxShape(2 * extent, 4, 2 * extent),
        };
        _staticInstancingNodes.Children.Add(instancingMeshNode);

        // Strategy D: We merge all instances of the mesh into one huge static mesh.
        var mergedMesh = MeshHelper.Merge(mesh, scales, poses);
        _staticBatchingNodes.Children.Add(new MeshNode(mergedMesh));
      }

      // For static batching, instead of creating one merged mesh per mesh type,
      // we could also merge all mesh instances into a single huge static mesh.
      //var mergedMesh = MeshHelper.Merge(_originalMeshNodes.Children);
      //_staticBatchingNodes = new MeshNode(mergedMesh);
    }


    private void CreateGuiControls()
    {
      var panel = SampleFramework.AddOptions("Batching");

      SampleHelper.AddDropDown(
        panel,
        "Batching method",
        EnumHelper.GetValues(typeof(BatchingType)),
        (int)_batchingType,
        batchingType => SetBatchingType((BatchingType)batchingType));

      SampleHelper.AddCheckBox(
        panel,
        "Draw debug info",
        _drawDebugInfo,
        isChecked => _drawDebugInfo = isChecked);

      SampleFramework.ShowOptionsWindow("Batching");
    }


    private void SetBatchingType(BatchingType newBatchingType)
    {
      // Reset scene.
      _graphicsScreen.Scene.Children.Remove(_originalMeshNodes);
      _graphicsScreen.Scene.Children.Remove(_staticInstancingNodes);
      _graphicsScreen.Scene.Children.Remove(_staticBatchingNodes);

      _batchingType = newBatchingType;
      if (newBatchingType == BatchingType.NoBatching)
      {
        // Strategy A: Add many individual mesh nodes to scene. Disable dynamic hardware instancing.
        _graphicsScreen.Scene.Children.Add(_originalMeshNodes);
        _graphicsScreen.MeshRenderer.EnableInstancing = false;
      }
      else if (newBatchingType == BatchingType.DynamicHardwareInstancing)
      {
        // Strategy B: Many individual nodes. MeshRenderer performs dynamic hardware instancing.
        _graphicsScreen.Scene.Children.Add(_originalMeshNodes);
        _graphicsScreen.MeshRenderer.EnableInstancing = true;
      }
      else if (newBatchingType == BatchingType.StaticHardwareInstancing)
      {
        // Strategy C: A few InstancingMeshNodes.
        _graphicsScreen.Scene.Children.Add(_staticInstancingNodes);
      }
      else if (newBatchingType == BatchingType.StaticBatching)
      {
        // Strategy D: A few merged MeshNodes.
        _graphicsScreen.Scene.Children.Add(_staticBatchingNodes);
      }
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // In the other samples we do not dispose the scene nodes manually.
        // This is usually not necessary because the base class Sample disposes
        // the graphics screen which disposes the whole scene.
        // Depending on the currently set _batchingType, the _originalMeshNodes
        // might not be part of the scene --> Dispose them manually.
        // The disposeData flag is false because we do not want to dispose the
        // meshes. These have been loaded by the content manager and their lifetime
        // is managed by the content manager.
        _originalMeshNodes.Dispose(false);
        _staticInstancingNodes.Dispose(false);

        // This merged mesh node was not loaded via the content pipeline.
        // --> Dispose it together with the Mesh.
        _staticBatchingNodes.Dispose(true);
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
          _graphicsScreen.Scene.GetDescendants().OfType<MeshNode>().Cast<IGeometricObject>(),
          Color.Yellow,
          true,
          false);

        // Green occluders.
        foreach (var meshNode in _graphicsScreen.Scene.GetDescendants().OfType<MeshNode>())
          _graphicsScreen.DebugRenderer.DrawTriangles(
            meshNode.Mesh.Occluder,
            meshNode.PoseWorld,
            meshNode.ScaleWorld,
            Color.Green,
            true,
            true);
      }
    }
  }
}
#endif