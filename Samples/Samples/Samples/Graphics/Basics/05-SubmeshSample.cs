using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample renders a few built-in submeshes.",
    @"In XNA a submesh is called ModelMeshPart. It is a batch of primitives that can be rendered 
in one draw call. (A submesh is usually a part of a mesh - a mesh consists of at least one 
submesh. But in this sample the submesh is used directly.)",
    5)]
  public class SubmeshSample : Sample
  {
    private readonly CameraObject _cameraObject;

    private readonly BasicEffect _effect;

    private readonly Submesh _sphere;
    private readonly Submesh _box;
    private readonly Submesh _cone;
    private readonly Submesh _torus;
    private readonly Submesh _teapot;


    public SubmeshSample(Microsoft.Xna.Framework.Game game)
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

      var graphicsDevice = GraphicsService.GraphicsDevice;

      // The MeshHelper class can create submeshes for several basic shapes:
      _sphere = MeshHelper.CreateUVSphere(graphicsDevice, 20);
      _torus = MeshHelper.CreateTorus(graphicsDevice, 0.5f, 0.667f, 16);
      _teapot = MeshHelper.CreateTeapot(graphicsDevice, 1, 8);

      // MeshHelper.CreateBox() returns a new submesh for a box. Instead we can call
      // MeshHelper.GetBox(), which returns a shared submesh. - GetBox() will always 
      // return the same instance.
      _box = MeshHelper.GetBox(GraphicsService);

      // We can also create a submesh that uses line primitives.
      _cone = MeshHelper.GetConeLines(GraphicsService);

      // We use a normal XNA BasicEffect to render the submeshes.
      _effect = new BasicEffect(graphicsDevice) { PreferPerPixelLighting = true };
      _effect.EnableDefaultLighting();
    }


    private void Render(RenderContext context)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      graphicsDevice.Clear(Color.CornflowerBlue);

      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;

      // Use the view and projection info from the camera node controlled by the 
      // Player game component.
      _effect.View = (Matrix)_cameraObject.CameraNode.View;
      _effect.Projection = _cameraObject.CameraNode.Camera.Projection;

      _effect.World = Matrix.CreateTranslation(-1, 1, 0);
      _effect.DiffuseColor = new Vector3(1, 0, 0);
      _effect.LightingEnabled = true;
      _effect.CurrentTechnique.Passes[0].Apply();

      // Render the submesh using the currently active shader.
      _torus.Draw();

      _effect.World = Matrix.CreateTranslation(1, 1, 0);
      _effect.DiffuseColor = new Vector3(0, 1, 0);
      _effect.CurrentTechnique.Passes[0].Apply();
      _teapot.Draw();

      _effect.World = Matrix.CreateScale(0.5f, 1, 0.5f) * Matrix.CreateTranslation(-2, 1, -2);
      _effect.DiffuseColor = new Vector3(0, 0, 1);
      _effect.CurrentTechnique.Passes[0].Apply();
      _sphere.Draw();

      _effect.World = Matrix.CreateTranslation(0, 1, -2);
      _effect.DiffuseColor = new Vector3(1, 1, 0);
      _effect.CurrentTechnique.Passes[0].Apply();
      _box.Draw();

      _effect.World = Matrix.CreateTranslation(2, 1, -2);
      _effect.DiffuseColor = new Vector3(1, 0, 1);
      _effect.LightingEnabled = false;
      _effect.CurrentTechnique.Passes[0].Apply();
      _cone.Draw();
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _effect.Dispose();

        _sphere.Dispose();
        _torus.Dispose();
        _teapot.Dispose();
        // Note: We do not dispose the box and the cone because those are shared submeshes!!!
      }

      base.Dispose(disposing);
    }
  }
}
