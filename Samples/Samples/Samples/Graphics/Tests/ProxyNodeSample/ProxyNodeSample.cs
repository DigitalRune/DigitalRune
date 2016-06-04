using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This samples shows how to implement a ProxyNode.",
    "",
    1000)]
  public class ProxyNodeSample : Sample
  {
    private readonly CameraObject _cameraObject;
    private readonly ProxyNode _proxyNode;

    private readonly Scene _scene;
    private readonly DebugRenderer _debugRenderer;
    private readonly MeshRenderer _renderer;


    public ProxyNodeSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, delegateGraphicsScreen);

      _renderer = new MeshRenderer();

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      GameObjectService.Objects.Add(_cameraObject);

      _scene = new Scene();
      SceneSample.InitializeDefaultXnaLights(_scene);

      // For advanced users: Set this flag if you want to analyze the imported opaque data of
      // effect bindings.
      EffectBinding.KeepOpaqueData = true;

      // Original model in scene graph.
      var modelNode = ContentManager.Load<ModelNode>("Dude/Dude").Clone();
      modelNode.PoseLocal = new Pose(new Vector3F(-2, 0, 0));
      var meshNode = modelNode.GetSubtree().OfType<MeshNode>().First();
      _scene.Children.Add(modelNode);

      // Clone referenced by proxy node.
      var modelNode2 = modelNode.Clone();
      var meshNode2 = modelNode2.GetSubtree().OfType<MeshNode>().First();
      meshNode2.SkeletonPose = meshNode.SkeletonPose;
      _proxyNode = new ProxyNode(null)
      {
        Name = "Proxy",
        PoseLocal = new Pose(new Vector3F(2, 0, 0), Matrix33F.CreateRotationY(ConstantsF.Pi)),
        ScaleLocal = new Vector3F(0.5f),
      };
      _scene.Children.Add(_proxyNode);
      _proxyNode.Node = modelNode2;

      var spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
      _debugRenderer = new DebugRenderer(GraphicsService, spriteFont);

      var mesh = meshNode.Mesh;
      foreach (var m in mesh.Materials)
      {
        //((ConstParameterBinding<Vector3>)m["Default"].ParameterBindings["SpecularColor"]).Value = new Vector3();
        ((SkinnedEffectBinding)m["Default"]).PreferPerPixelLighting = true;
      }

      var timeline = new TimelineClip(mesh.Animations.Values.First())
      {
        Duration = TimeSpan.MaxValue,
        LoopBehavior = LoopBehavior.Cycle,
      };
      AnimationService.StartAnimation(timeline, (IAnimatableProperty)meshNode.SkeletonPose);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _renderer.Dispose();
        _debugRenderer.Dispose();
        _scene.Dispose(false);

        _proxyNode.Node.Dispose(false);
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      _scene.Update(gameTime.ElapsedGameTime);

      _debugRenderer.Clear();
      foreach (var sceneNode in _scene.GetDescendants())
      {
        _debugRenderer.DrawText(sceneNode.Name ?? sceneNode.GetType().Name, sceneNode.PoseWorld.Position, Color.Green, true);
        _debugRenderer.DrawAxes(sceneNode.PoseWorld, 1, false);
      }

      _debugRenderer.DrawAxes(Pose.Identity, 1, false);

      base.Update(gameTime);
    }


    private void Render(RenderContext context)
    {
      context.CameraNode = _cameraObject.CameraNode;
      context.Scene = _scene;

      var device = context.GraphicsService.GraphicsDevice;

      device.DepthStencilState = DepthStencilState.Default;
      device.RasterizerState = RasterizerState.CullCounterClockwise;
      device.BlendState = BlendState.Opaque;

      device.Clear(Color.CornflowerBlue);

      context.RenderPass = "Default";
      var query = _scene.Query<ProxyAwareQuery>(context.CameraNode, context);
      _debugRenderer.DrawText(query.SceneNodes.Count.ToString());
      _renderer.Render(query.SceneNodes, context);

      _debugRenderer.Render(context);

      context.CameraNode = null;
      context.Scene = null;
    }
  }
}
