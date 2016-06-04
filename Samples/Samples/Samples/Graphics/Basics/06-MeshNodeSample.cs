using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample introduces Meshes, Materials, MeshNodes and Models.",
    @"A model is a tree of SceneNodes. Models are usually processed by the content pipeline (using 
the DigitalRune Model Processor) and loaded using XNA's ContentManager. Models can contain 
various types of scene nodes - but in most cases they contain mesh nodes. A MeshNode is used 
to assign position, orientation and scale to the Mesh. A Mesh is a collection of Submeshes 
and Materials. A Mesh itself does not have a position - it only defines the vertex/index 
buffers (Submeshes), shaders and shader parameters (Materials).",
    6)]
  public class MeshNodeSample : Sample
  {
    private readonly CameraObject _cameraObject;

    // A DigitalRune model.
    private readonly ModelNode _model;

    // A renderer which can render meshes (MeshNodes).
    private readonly MeshRenderer _meshRenderer;

    private readonly DebugRenderer _debugRenderer;


    public MeshNodeSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, delegateGraphicsScreen);

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      GameObjectService.Objects.Add(_cameraObject);

      // For advanced users: Set this flag if you want to analyze the imported opaque data of
      // effect bindings.
      //EffectBinding.KeepOpaqueData = true;

      // Load a model. The model is processed using the DigitalRune Model Processor - not 
      // the default XNA model processor!
      // In the folder that contains tank.fbx, there is an XML file tank.drmdl which defines 
      // properties of the model. These XML files are automatically processed by the 
      // DigitalRune Model Processor. 
      // Each model itself is a tree of scene nodes. The grid model contains one mesh 
      // node. The tank model contains several mesh nodes (turret, cannon, hatch, 
      // wheels, ...).
      _model = ContentManager.Load<ModelNode>("Tank/tank");

      // The XNA ContentManager manages a single instance of each model. We clone 
      // the model, to get a copy that we can modify without changing the original 
      // instance. Cloning is fast because it only duplicates the scene nodes - but 
      // not the mesh and material information.
      _model = _model.Clone();

      // _model is the root of a tree of scene nodes. The mesh nodes are the child 
      // nodes. When we scale or move the _model, we automatically scale and move 
      // all child nodes.
      _model.ScaleLocal = new Vector3F(0.8f);
      _model.PoseWorld = new Pose(new Vector3F(0, 0, -2), Matrix33F.CreateRotationY(-0.3f));

      // Let's loop through the mesh nodes of the model:
      foreach (var meshNode in _model.GetSubtree().OfType<MeshNode>())
      {
        // Each MeshNode references a Mesh.
        Mesh mesh = meshNode.Mesh;

        // The mesh consists of several submeshes and several materials - usually 
        // one material per submesh, but several submeshes could reference the same 
        // materials.

        // Let's loop through the materials of this mesh.
        foreach (var material in mesh.Materials)
        {
          // A material is a collection of EffectBindings - one EffectBinding for each
          // render pass. For example, a complex game uses several render passes, like
          // a pre-Z pass, a G-buffer pass, a shadow map pass, a deferred material pass, 
          // etc.In simple games there is only one pass which is called "Default".
          var effectBinding = material["Default"];

          // An EffectBinding references an Effect (the XNA Effect class) and it has
          // "parameter bindings" and "technique bindings". These bindings select the 
          // values for the shader parameters when the mesh node is rendered. 

          // Let's change the binding for the DiffuseColor of the shader to give tank 
          // a red color.
          effectBinding.Set("DiffuseColor", new Vector4(1, 0.7f, 0.7f, 1));

          // The tank uses the default effect binding which is a BasicEffectBinding - this
          // effect binding uses the XNA BasicEffect. 
          // In this sample we do not define any lights, therefore we disable the lighting
          // in the shader.
          ((BasicEffectBinding)effectBinding).LightingEnabled = false;
        }
      }

      _meshRenderer = new MeshRenderer();

      var spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
      _debugRenderer = new DebugRenderer(GraphicsService, spriteFont);
    }


    private void Render(RenderContext context)
    {
      // Set the current camera node in the render context. This info is used 
      // by the renderers.
      context.CameraNode = _cameraObject.CameraNode;

      var device = context.GraphicsService.GraphicsDevice;

      device.Clear(Color.CornflowerBlue);

      device.DepthStencilState = DepthStencilState.Default;
      device.RasterizerState = RasterizerState.CullCounterClockwise;
      device.BlendState = BlendState.Opaque;

      // Render the meshes one by one (The next sample, SceneSample, shows how to 
      // do this more efficiently.)
      // We have to select a render pass. This info is needed by the MeshRenderer
      // to decide which shaders must be used.
      context.RenderPass = "Default";
      foreach (var meshNode in _model.GetSubtree().OfType<MeshNode>())
        _meshRenderer.Render(meshNode, context);

      _debugRenderer.Render(context);

      // Clean up.
      context.RenderPass = null;
      context.CameraNode = null;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // IMPORTANT: Always dispose scene nodes if they are no longer needed!
        _model.Dispose(false);  // Note: This statement disposes only our local clone.
                                // The original instance is still available in the 
                                // ContentManager.

        _meshRenderer.Dispose();
        _debugRenderer.Dispose();

        // Unload content.
        // (We have changed the properties of some loaded materials. Other samples
        // should use the default values. When we unload them now, the next sample
        // will reload them with default values.)
        ContentManager.Unload();
      }

      base.Dispose(disposing);
    }
  }
}
