using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This example demonstrates how to render billboards.",
    "",
    11)]
  public class BillboardSample : Sample
  {
    private readonly CameraObject _cameraObject;
    private readonly Scene _scene;
    private readonly MeshRenderer _meshRenderer;           // Handles MeshNodes.
    private readonly BillboardRenderer _billboardRenderer; // Handles BillboardNodes and ParticleSystemNodes.
    private readonly DebugRenderer _debugRenderer;         // Used for drawing text labels.

    private readonly ImageBillboard _animatedBillboard;


    public BillboardSample(Microsoft.Xna.Framework.Game game)
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

      // In this example we need three renderers:
      // The MeshRenderer handles MeshNodes.
      _meshRenderer = new MeshRenderer();

      // The BillboardRenderer handles BillboardNodes and ParticleSystemNodes.
      _billboardRenderer = new BillboardRenderer(GraphicsService, 2048);

      // The DebugRenderer is used to draw text.
      var spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
      _debugRenderer = new DebugRenderer(GraphicsService, spriteFont);

      // Create a new empty scene.
      _scene = new Scene();

      // Add the camera node to the scene.
      _scene.Children.Add(_cameraObject.CameraNode);

      // Add a few models to the scene.
      var sandbox = ContentManager.Load<ModelNode>("Sandbox/Sandbox").Clone();
      _scene.Children.Add(sandbox);

      // Add some lights to the scene which have the same properties as the lights 
      // of BasicEffect.EnableDefaultLighting().
      SceneSample.InitializeDefaultXnaLights(_scene);

      var texture = new PackedTexture(ContentManager.Load<Texture2D>("Billboard/BillboardReference"));

      // ----- View plane-aligned billboards with variations.
      // View plane-aligned billboards are rendered parallel to the screen.
      // The up-axis of the BillboardNode determines the up direction of the 
      // billboard.
      var pose0 = new Pose(new Vector3F(-9, 1.0f, 1.5f));
      var pose1 = pose0;
      var billboard = new ImageBillboard(texture);
      var billboardNode = new BillboardNode(billboard);
      billboardNode.Name = "View plane-aligned\nVarying color\nVarying alpha";
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      billboardNode.Color = new Vector3F(1, 0, 0);
      billboardNode.Alpha = 0.9f;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      billboardNode.Color = new Vector3F(0, 1, 0);
      billboardNode.Alpha = 0.7f;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      billboardNode.Color = new Vector3F(0, 0, 1);
      billboardNode.Alpha = 0.3f;
      _scene.Children.Add(billboardNode);

      // ----- View plane-aligned billboards with different blend modes
      // blend mode = 0 ... additive blend
      // blend mode = 1 ... alpha blend
      pose0.Position.X += 2;
      pose1 = pose0;
      billboard = new ImageBillboard(texture);
      billboard.BlendMode = 0.0f;
      billboardNode = new BillboardNode(billboard);
      billboardNode.Name = "View plane-aligned\nVarying blend mode";
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboard = new ImageBillboard(texture);
      billboard.BlendMode = 0.333f;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboard = new ImageBillboard(texture);
      billboard.BlendMode = 0.667f;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboard = new ImageBillboard(texture);
      billboard.BlendMode = 1.0f;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      // ----- View plane-aligned billboards with alpha test
      pose0.Position.X += 2;
      pose1 = pose0;
      billboard = new ImageBillboard(texture);
      billboard.AlphaTest = 0.9f;
      billboardNode = new BillboardNode(billboard);
      billboardNode.Name = "View plane-aligned\nVarying reference alpha";
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboard = new ImageBillboard(texture);
      billboard.AlphaTest = 0.667f;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboard = new ImageBillboard(texture);
      billboard.AlphaTest = 0.333f;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboard = new ImageBillboard(texture);
      billboard.AlphaTest = 0.0f;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      // ----- View plane-aligned billboards with different scale and rotation
      pose0.Position.X += 2;
      pose1 = pose0;
      billboard = new ImageBillboard(texture);
      billboard.Orientation = BillboardOrientation.ViewPlaneAligned;
      billboardNode = new BillboardNode(billboard);
      billboardNode.Name = "View plane-aligned\nVarying scale\nVarying rotation";
      billboardNode.PoseWorld = pose1;
      billboardNode.ScaleLocal = new Vector3F(0.4f);
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = billboardNode.Clone();
      billboardNode.Name = null;
      billboardNode.PoseWorld = pose1 * new Pose(Matrix33F.CreateRotationZ(MathHelper.ToRadians(-15)));
      billboardNode.ScaleLocal = new Vector3F(0.6f);
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = billboardNode.Clone();
      billboardNode.Name = null;
      billboardNode.PoseWorld = pose1 * new Pose(Matrix33F.CreateRotationZ(MathHelper.ToRadians(-30)));
      billboardNode.ScaleLocal = new Vector3F(0.8f);
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = billboardNode.Clone();
      billboardNode.Name = null;
      billboardNode.PoseWorld = pose1 * new Pose(Matrix33F.CreateRotationZ(MathHelper.ToRadians(-45)));
      billboardNode.ScaleLocal = new Vector3F(1.0f);
      _scene.Children.Add(billboardNode);

      // ----- Viewpoint-oriented billboards
      // Viewpoint-orientated billboards always face the player. (The face normal 
      // points directly to the camera.)
      pose0.Position.X += 2;
      pose1 = pose0;
      billboard = new ImageBillboard(texture);
      billboard.Orientation = BillboardOrientation.ViewpointOriented;
      billboardNode = new BillboardNode(billboard);
      billboardNode.Name = "Viewpoint-oriented";
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      // ----- Screen-aligned billboards
      // View plane-aligned billboards and screen-aligned billboards are similar. The 
      // billboards are rendered parallel to the screen. The orientation can be changed 
      // by rotating the BillboardNode. The difference is that the orientation of view 
      // plane-aligned billboards is relative to world space and the orientation of 
      // screen-aligned billboards is relative to view space.
      // Screen-aligned billboards are, for example, used for text label.
      pose0.Position.X += 2;
      pose1 = pose0;
      billboard = new ImageBillboard(texture);
      billboard.Orientation = BillboardOrientation.ScreenAligned;
      billboardNode = new BillboardNode(billboard);
      billboardNode.Name = "Screen-aligned";
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      // ----- Axial, view plane-aligned billboards
      pose0.Position.X += 2;
      pose1 = pose0;
      billboard = new ImageBillboard(texture);
      billboard.Orientation = BillboardOrientation.AxialViewPlaneAligned;
      billboardNode = new BillboardNode(billboard);
      billboardNode.Name = "Axial, view plane-aligned";
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = billboardNode.Clone();
      billboardNode.Name = null;
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      // ----- Axial, viewpoint-oriented billboards
      pose0.Position.X += 2;
      pose1 = pose0;
      billboard = new ImageBillboard(texture);
      billboard.Orientation = BillboardOrientation.AxialViewpointOriented;
      billboardNode = new BillboardNode(billboard);
      billboardNode.Name = "Axial, viewpoint-oriented";
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      // ----- World-oriented billboards
      // World-oriented billboards have a fixed orientation in world space. The 
      // orientation is determine by the BillboardNode.
      pose0.Position.X += 2;
      pose1 = pose0;
      pose1.Orientation *= Matrix33F.CreateRotationY(0.2f);
      billboard = new ImageBillboard(texture);
      billboard.Orientation = BillboardOrientation.WorldOriented;
      billboardNode = new BillboardNode(billboard);
      billboardNode.Name = "World-oriented";
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      pose1.Orientation *= Matrix33F.CreateRotationY(0.2f);
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1 * new Pose(Matrix33F.CreateRotationZ(MathHelper.ToRadians(15)));
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      pose1.Orientation *= Matrix33F.CreateRotationY(0.2f);
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1 * new Pose(Matrix33F.CreateRotationZ(MathHelper.ToRadians(30)));
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      pose1.Orientation *= Matrix33F.CreateRotationY(0.2f);
      billboardNode = new BillboardNode(billboard);
      billboardNode.PoseWorld = pose1 * new Pose(Matrix33F.CreateRotationZ(MathHelper.ToRadians(45)));
      _scene.Children.Add(billboardNode);

      // ----- Animated billboards
      // DigitalRune Graphics supports "texture atlases". I.e. textures can be packed 
      // together into a single, larger texture file. A PackedTexture can describe a 
      // single texture packed into a texture atlas or a tile set packed into a 
      // texture atlas. In this example the "beeWingFlap" is a set of three tiles.
      // Tile sets can be used for sprite animations. (The animation is set below in 
      // Update().)
      pose0.Position.X += 2;
      pose1 = pose0;
      texture = new PackedTexture("Bee", ContentManager.Load<Texture2D>("Particles/beeWingFlap"), Vector2F.Zero, Vector2F.One, 3, 1);
      _animatedBillboard = new ImageBillboard(texture);
      billboardNode = new BillboardNode(_animatedBillboard);
      billboardNode.Name = "Animated billboards";
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(_animatedBillboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      pose1.Position.Z -= 1;
      billboardNode = new BillboardNode(_animatedBillboard);
      billboardNode.PoseWorld = pose1;
      _scene.Children.Add(billboardNode);

      // Use DebugRenderer to draw node names above billboard nodes.
      foreach (var node in _scene.GetDescendants().OfType<BillboardNode>())
        _debugRenderer.DrawText(node.Name, node.PoseWorld.Position + new Vector3F(0, 1, 0), new Vector2F(0.5f), Color.Yellow, false);
    }


    public override void Update(GameTime gameTime)
    {
      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // Update animated billboards ("bee"):
      // AnimationTime is the normalized animation time where 
      //   0 = start of animation (first frame) and
      //   1 = end of animation (last frame)
      // The AnimationTime can be set per Billboard or per BillboardNode.
      _animatedBillboard.AnimationTime = (_animatedBillboard.AnimationTime + deltaTime * 10.0f) % 1;

      // Update the scene - this must be called once per frame.
      _scene.Update(gameTime.ElapsedGameTime);

      base.Update(gameTime);
    }


    private void Render(RenderContext context)
    {
      // Set render context info.
      context.CameraNode = _cameraObject.CameraNode;
      context.Scene = _scene;

      // Frustum culling: Get all scene nodes which overlap the view frustum.
      var query = _scene.Query<CameraFrustumQuery>(context.CameraNode, context);

      // Render meshes.
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.CornflowerBlue);
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      context.RenderPass = "Default";
      _meshRenderer.Render(query.SceneNodes, context);
      context.RenderPass = null;

      // Render billboards using alpha blending.
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
      _billboardRenderer.Render(query.SceneNodes, context, RenderOrder.BackToFront);

      _debugRenderer.Render(context);

      // Clean up.
      context.Scene = null;
      context.CameraNode = null;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // IMPORTANT: Dispose scene nodes if they are no longer needed!
        _scene.Dispose(false);  // Disposes current and all descendant nodes.

        _meshRenderer.Dispose();
        _billboardRenderer.Dispose();
        _debugRenderer.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
