using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to use hardware instancing and a custom shader",
    @"The custom shader (see Samples/Content/InstancedModel/InstancedModel.fx) renders each 
instance with a random color and a random alpha (using screen-door transparency). The DigitalRune 
MeshRenderer renders the meshes and will automatically use hardware instancing if the shader 
contains an ""instancing technique"" (see shader code). Please note: The only per-instance 
parameter that are currently supported in hardware instancing are:
  ""World"" (float4x4),
  ""InstanceColor"" (float3),
  ""InstanceAlpha"" (float)

For more information about instancing have a look at the following sample:
  DeferredRendering/31-BatchingSample.",
    8)]
  class InstancingSample : Sample
  {
    private readonly CameraObject _cameraObject;

    private readonly Scene _scene;
    private readonly MeshRenderer _meshRenderer;


    public InstancingSample(Microsoft.Xna.Framework.Game game)
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

      _scene = new Scene();
      _scene.Children.Add(_cameraObject.CameraNode);

      // Add a lot of instances of one model to the scene. This model uses a custom 
      // shader which supports instancing. See the *.drmdl, *.drmat and *.fx files 
      // in the directory of the FBX file.
      var model = ContentManager.Load<ModelNode>("InstancedModel/InstancedModel");
      for (int x = 1; x < 50; x++)
      {
        for (int z = 1; z < 20; z++)
        {
          var clone = model.Clone();
          Pose pose = clone.PoseLocal;
          pose.Position.X -= x;
          pose.Position.Z -= z;
          clone.PoseLocal = pose;
          clone.ScaleLocal = new Vector3F(0.7f);
          SetRandomColorAndAlpha(clone);
          _scene.Children.Add(clone);
        }
      }

      SceneSample.InitializeDefaultXnaLights(_scene);

      _meshRenderer = new MeshRenderer();
    }


    private void SetRandomColorAndAlpha(ModelNode model)
    {
      var randomColor3F = RandomHelper.Random.NextVector3F(0, 4);

      // Desaturate random color to avoid eye cancer. ;-)
      float luminance = Vector3F.Dot(randomColor3F, GraphicsHelper.LuminanceWeights);
      var randomColor = (Vector3)InterpolationHelper.Lerp(new Vector3F(luminance), randomColor3F, 0.5f);

      var randomAlpha = MathHelper.Clamp(RandomHelper.Random.NextFloat(0, 5), 0, 1);

      // Change the values of all effect parameters "InstanceColor" and "InstanceAlpha":
      foreach (MeshNode meshNode in model.GetDescendants().OfType<MeshNode>())
      {
        foreach (MaterialInstance materialInstance in meshNode.MaterialInstances)
        {
          foreach (EffectBinding effectBinding in materialInstance.EffectBindings)
          {
            if (effectBinding.ParameterBindings.Contains("InstanceColor"))
              effectBinding.Set("InstanceColor", randomColor);
            if (effectBinding.ParameterBindings.Contains("InstanceAlpha"))
              effectBinding.Set("InstanceAlpha", randomAlpha);
          }
        }
      }
    }


    public override void Update(GameTime gameTime)
    {
      _scene.Update(gameTime.ElapsedGameTime);

      base.Update(gameTime);
    }


    private void Render(RenderContext context)
    {
      context.CameraNode = _cameraObject.CameraNode;
      context.Scene = _scene;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.CornflowerBlue);
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;

      var query = _scene.Query<CameraFrustumQuery>(context.CameraNode, context);

      context.RenderPass = "Default";

      // The mesh renderer automatically sorts and renders the meshes. If the 
      // material of a mesh supports hardware instancing, the renderer will use it.
      _meshRenderer.Render(query.SceneNodes, context);

      context.RenderPass = null;
      context.CameraNode = null;
      context.Scene = null;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // IMPORTANT: Dispose scene nodes if they are no longer needed!
        _scene.Dispose(false);  // Disposes current and all descendant nodes.

        _meshRenderer.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
