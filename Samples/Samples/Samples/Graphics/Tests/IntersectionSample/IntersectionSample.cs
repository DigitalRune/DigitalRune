using System.Collections.Generic;
using System.Linq;
using DigitalRune.Collections;
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MathHelper = Microsoft.Xna.Framework.MathHelper;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This samples renders the intersection of two meshes.",
    "",
    1000)]
  [Controls(@"Sample
  Press <Left mouse button> to hide submeshes and show only intersection.")]
  public class IntersectionSample : Sample
  {
    private readonly CameraObject _cameraObject;
    private readonly Scene _scene;
    private readonly MeshRenderer _meshRenderer;
    private readonly DebugRenderer _debugRenderer;

    private readonly IntersectionRenderer _intersectionRenderer;

    private int _maxConvexity = 4;
    private readonly List<Pair<MeshNode>> _meshNodePairs = new List<Pair<MeshNode>>();


    public IntersectionSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, delegateGraphicsScreen);

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services, 10);
      _cameraObject.ResetPose(new Vector3F(0, 0, -4), ConstantsF.Pi, 0);
      GameObjectService.Objects.Add(_cameraObject);

      // Create a new scene with some lights.
      _scene = new Scene();
      SceneSample.InitializeDefaultXnaLights(_scene);

      _meshRenderer = new MeshRenderer();
      _debugRenderer = new DebugRenderer(GraphicsService, null);

      _intersectionRenderer = new IntersectionRenderer(GraphicsService, ContentManager)
      {
        DownsampleFactor = 1,
      };

      //_submeshA = MeshHelper.CreateSubmesh(GraphicsService.GraphicsDevice, new SphereShape(0.5f).GetMesh(0.001f, 5), MathHelper.ToRadians(70));
      //_submeshB = MeshHelper.CreateSubmesh(GraphicsService.GraphicsDevice, new BoxShape(1, 1, 2).GetMesh(0.001f, 5), MathHelper.ToRadians(70));

      var meshNodeA = CreateMeshNode(new[]
      {
        MeshHelper.CreateTorus(GraphicsService.GraphicsDevice, 1, 0.3f, 30),
        MeshHelper.CreateSubmesh(GraphicsService.GraphicsDevice, new BoxShape(1, 1, 2).GetMesh(0.001f, 5), MathHelper.ToRadians(70)),
      },
      Color.DarkBlue);
      meshNodeA.PoseWorld = new Pose(RandomHelper.Random.NextVector3F(-0.5f, 0.5f),
                                    RandomHelper.Random.NextQuaternionF());
      _scene.Children.Add(meshNodeA);
      _debugRenderer.DrawObject(meshNodeA, Color.Green, true, false);

      var shape = new TransformedShape(
        new GeometricObject(new SphereShape(0.5f), new Pose(new Vector3F(1, 0, 0))));
      var meshNodeB = CreateMeshNode(new[]
      {
        MeshHelper.CreateTorus(GraphicsService.GraphicsDevice, 1, 0.3f, 30),
        MeshHelper.CreateSubmesh(GraphicsService.GraphicsDevice, shape.GetMesh(0.001f, 4), MathHelper.ToRadians(90)),
      },
      Color.Gray);
      meshNodeB.PoseWorld = new Pose(RandomHelper.Random.NextVector3F(-1f, 1f),
                                    RandomHelper.Random.NextQuaternionF());
      _scene.Children.Add(meshNodeB);
      _debugRenderer.DrawObject(meshNodeB, Color.Green, true, false);

      var meshNodeC = CreateMeshNode(new[]
      {
        MeshHelper.CreateBox(GraphicsService.GraphicsDevice),
        MeshHelper.CreateSubmesh(GraphicsService.GraphicsDevice, new BoxShape(1, 1, 2).GetMesh(0.001f, 5), MathHelper.ToRadians(70))
      },
      Color.DarkGreen);
      meshNodeC.PoseWorld = new Pose(RandomHelper.Random.NextVector3F(-1f, 1f),
                                    RandomHelper.Random.NextQuaternionF());
      meshNodeC.ScaleLocal = new Vector3F(0.1f, 1f, 0.5f);
      _scene.Children.Add(meshNodeC);
      _debugRenderer.DrawObject(meshNodeC, Color.Green, true, false);

      _meshNodePairs.Add(new Pair<MeshNode>(meshNodeA, meshNodeB));
      _meshNodePairs.Add(new Pair<MeshNode>(meshNodeA, meshNodeC));
      _meshNodePairs.Add(new Pair<MeshNode>(meshNodeB, meshNodeC));

      CreateGuiControls();
    }


    public override void Update(GameTime gameTime)
    {
      _scene.Update(gameTime.ElapsedGameTime);

      _intersectionRenderer.Dummy = InputService.IsDown(Keys.Space);

      base.Update(gameTime);
    }


    private void Render(RenderContext context)
    {
      context.CameraNode = _cameraObject.CameraNode;
      context.Scene = _scene;

      var device = context.GraphicsService.GraphicsDevice;

      device.Clear(Color.White);

      // Render intersection into internal offscreen render targets.
      // This operation has to be performed before the actual scene rendering.
      // It destroys the current render target content.
      _intersectionRenderer.ComputeIntersection(
        _meshNodePairs,
        new Vector3F(1, 0.2f, 0),     // Diffuse color
        0.8f,                         // Alpha
        _maxConvexity,                // Max convexity
        context);                     // context.CameraNode and Viewport need to be set.

      device.SetRenderTarget(context.RenderTarget);
      device.Viewport = context.Viewport;
      device.Clear(Color.White);

      // Render submeshes.
      if (!InputService.IsDown(MouseButtons.Left))
      {
        device.DepthStencilState = DepthStencilState.Default;
        device.RasterizerState = RasterizerState.CullNone;
        device.BlendState = BlendState.Opaque;

        context.RenderPass = "Default";
        _meshRenderer.Render(_scene.GetDescendants().ToArray(), context);
        context.RenderPass = null;
      }

      if (!InputService.IsDown(MouseButtons.Right))
      {
        // Combine intersection image with current back buffer.
        // We can use depth tests and alpha blending.
        device.DepthStencilState = DepthStencilState.None;
        device.BlendState = BlendState.AlphaBlend;
        _intersectionRenderer.RenderIntersection(context);
      }

      _debugRenderer.Render(context);

      context.Scene = null;
      context.CameraNode = null;
    }


    private MeshNode CreateMeshNode(IEnumerable<Submesh> submeshes, Color color)
    {
      var mesh = new Mesh();
      mesh.Submeshes.AddRange(submeshes);

      var material = new Material();
      BasicEffectBinding defaultEffectBinding = new BasicEffectBinding(GraphicsService, null)
      {
        LightingEnabled = true,
        TextureEnabled = false,
        VertexColorEnabled = false
      };
      defaultEffectBinding.Set("DiffuseColor", color.ToVector4());
      defaultEffectBinding.Set("SpecularColor", new Vector3(1, 1, 1));
      defaultEffectBinding.Set("SpecularPower", 100f);
      material.Add("Default", defaultEffectBinding);

      var triangleMesh = mesh.ToTriangleMesh();
      var shape = new TriangleMeshShape(triangleMesh);
      var aabb = shape.GetAabb();
      mesh.BoundingShape = new TransformedShape(
        new GeometricObject(
          new BoxShape(aabb.Extent),
          new Pose(aabb.Center)));

      mesh.Materials.Add(material);
      foreach (var submesh in mesh.Submeshes)
        submesh.MaterialIndex = 0;

      return new MeshNode(mesh);
    }


    private void CreateGuiControls()
    {
      var panel = SampleFramework.AddOptions("Intersections");

      // ----- Light node controls
      SampleHelper.AddSlider(
        panel,
        "Max convexity",
        "F0",
        1,
        10,
        _maxConvexity,
        value => _maxConvexity = (int)value);

      SampleHelper.AddSlider(
        panel,
        "Downsample factor",
        "F2",
        1,
        4,
        _intersectionRenderer.DownsampleFactor,
        value => _intersectionRenderer.DownsampleFactor = value);

      SampleHelper.AddCheckBox(
        panel,
        "Use sissor test",
        _intersectionRenderer.EnableScissorTest,
        isChecked => _intersectionRenderer.EnableScissorTest = isChecked);

      SampleFramework.ShowOptionsWindow("Intersections");
    }
  }
}
