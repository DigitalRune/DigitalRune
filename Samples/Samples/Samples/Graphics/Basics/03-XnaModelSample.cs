using System.Linq;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows that you can still use the XNA features, like the XNA model class, to do 
the rendering.",
    @"This sample uses the DigitalRune CameraNode controlled by the Player component to view an 
XNA model (imported using the XNA model processor).",
    3)]
  public class XnaModelSample : Sample
  {
    private readonly CameraObject _cameraObject;
    private readonly Model _model;


    public XnaModelSample(Microsoft.Xna.Framework.Game game)
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

      // Load XNA model.
      _model = ContentManager.Load<Model>("Saucer3/saucer");

      // Enable default lighting.
      var basicEffects = _model.Meshes
                               .SelectMany(m => m.MeshParts)
                               .Select(mp => mp.Effect)
                               .OfType<BasicEffect>();
      foreach (var effect in basicEffects)
        effect.EnableDefaultLighting();
    }


    private void Render(RenderContext context)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      graphicsDevice.Clear(Color.CornflowerBlue);

      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.BlendState = BlendState.Opaque;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      // Draw XNA model (as usual in XNA) with the view and projection matrix of 
      // the DigitalRune CameraNode.
      var world = Matrix.CreateFromYawPitchRoll(0.5f, 0.3f, 0) * Matrix.CreateTranslation(0, 1.5f, 0);
      CameraNode cameraNode = _cameraObject.CameraNode;
      Matrix view = (Matrix)cameraNode.View;
      Matrix projection = cameraNode.Camera.Projection;
      _model.Draw(world, view, projection);
    }
  }
}
