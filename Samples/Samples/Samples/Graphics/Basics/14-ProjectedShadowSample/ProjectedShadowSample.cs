using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Plane = DigitalRune.Geometry.Shapes.Plane;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to implement a renderer for planar projected shadows.",
    @"Planar projected shadows is one of the simplest shadow algorithms. A shadow of an object is
create by rendering the object a second time in black with a transformation matrix which projects
the mesh's vertices onto a flat plane.
This method is fast and simple but it works only for shadows on flat planes and not for shadows on
curved surfaces.
This sample creates a new renderer called 'ProjectedShadowRenderer'. This renderer can create shadows
for MeshNodes. It renders the MeshNodes using XNA's BasicEffect. It applies a 'shadow matrix' which
turns the mesh into the flat, black shadow.",
    14)]
  public class ProjectedShadowSample : Sample
  {
    private readonly CameraObject _cameraObject;

    private readonly Scene _scene;

    private readonly MeshRenderer _meshRenderer;
    private readonly DebugRenderer _debugRenderer;
    private readonly ProjectedShadowRenderer _projectedShadowRenderer;

    private readonly SceneNode[] _tankMeshNodes;

    private readonly LightNode _mainDirectionalLightNode;
    private float _lightAngle;


    public ProjectedShadowSample(Microsoft.Xna.Framework.Game game)
    : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, delegateGraphicsScreen);

      // Create a new empty scene.
      _scene = new Scene();
      Services.Register(typeof(IScene), null, _scene);

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      _cameraObject.ResetPose(new Vector3F(-8, 6, 8), -ConstantsF.PiOver4, -0.4f);
      GameObjectService.Objects.Add(_cameraObject);

      // Add a default light setup (ambient light + 3 directional lights).
      var defaultLightsObject = new DefaultLightsObject(Services);
      GameObjectService.Objects.Add(defaultLightsObject);

      // Get the main directional light.
      _mainDirectionalLightNode = ((LightNode)_scene.GetSceneNode("KeyLight"));

      // Add a ground plane model to the scene graph.
      var grid = ContentManager.Load<ModelNode>("Ground/Ground").Clone();
      grid.ScaleLocal = new Vector3F(0.3f);
      _scene.Children.Add(grid);

      // Add a tank model to the scene graph.
      var tank = ContentManager.Load<ModelNode>("Tank/tank").Clone();
      _scene.Children.Add(tank);

      // Remember the mesh nodes of tank node.
      _tankMeshNodes = tank.GetSubtree().Where(n => n is MeshNode).ToArray();

      // Create the renderers.
      _meshRenderer = new MeshRenderer();

      var spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
      _debugRenderer = new DebugRenderer(GraphicsService, spriteFont);

      _projectedShadowRenderer = new ProjectedShadowRenderer(GraphicsService)
      {
        // The plane onto which the shadows are projected. It is positioned a bit above the ground
        // plane to avoid z-fighting.
        ShadowedPlane = new Plane(new Vector3F(0, 1, 0), 0.01f),

        // The shadow color is a transparent black.
        ShadowColor = new Vector4F(0, 0, 0, 0.4f),

        // The light position is set in Update().
        //LightPosition = ...
      };
    }


    public override void Update(GameTime gameTime)
    {
      // Move the directional light in a circle.
      float deltaTimeF = (float)gameTime.ElapsedGameTime.TotalSeconds;
      _lightAngle += 0.3f * deltaTimeF;
      var position = QuaternionF.CreateRotationY(_lightAngle).Rotate(new Vector3F(6, 6, 0));

      // Make the light look at the world space origin.
      var lightTarget = Vector3F.Zero;
      var lookAtMatrix = Matrix44F.CreateLookAt(position, lightTarget, Vector3F.Up);
      
      // A look-at matrix is the inverse of a normal world or pose matrix.
      _mainDirectionalLightNode.PoseWorld =
        new Pose(lookAtMatrix.Translation, lookAtMatrix.Minor).Inverse;

      // Update the light position of the renderer.
      // To create a local light shadow (light rays are not parallel), we have to set the light 
      // position and a 4th component of 1.
      //_projectedShadowRenderer.LightPosition = new Vector4F(position, 1);
      // To create a directional light shadow (light rays are parallel), we have to set the inverse
      // light direction and 0.
      var lightRayDirection = (lightTarget - position);
      _projectedShadowRenderer.LightPosition = new Vector4F(-lightRayDirection, 0);

      // For debugging: Draw coordinate axes at (0, 0, 0).
      _debugRenderer.Clear();
      _debugRenderer.DrawAxes(Pose.Identity, 1, true);

      // Draw light node. (Will be drawn as a coordinate cross.)
      _debugRenderer.DrawObject(_mainDirectionalLightNode, Color.Yellow, false, true);

      // Update the scene - this must be called once per frame.
      _scene.Update(gameTime.ElapsedGameTime);

      base.Update(gameTime);
    }


    private void Render(RenderContext context)
    {
      // Set render context info.
      context.CameraNode = _cameraObject.CameraNode;
      context.Scene = _scene;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.CornflowerBlue);

      // Frustum culling: Get all scene nodes which overlap the view frustum.
      var query = _scene.Query<CameraFrustumQuery>(context.CameraNode, context);

      // Default render states for opaque meshes.
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

      // Render the meshes using the "Default" render pass.
      context.RenderPass = "Default";
      _meshRenderer.Render(query.SceneNodes, context);
      context.RenderPass = null;

      // Render the shadow of the tank's meshes.
      // (In a more sophisticated application, we could use SceneNode.UserFlags to mark all
      // mesh nodes that should cast a projected shadow. Then we could create a custom SceneQuery
      // and call _projectedShadowRenderer.Render(mySceneQuery.ShadowCasters, context);
      _projectedShadowRenderer.Render(_tankMeshNodes, context);

      // Draw debug info.
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

        // Dispose renderers.
        _meshRenderer.Dispose();
        _debugRenderer.Dispose();
        _projectedShadowRenderer.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
