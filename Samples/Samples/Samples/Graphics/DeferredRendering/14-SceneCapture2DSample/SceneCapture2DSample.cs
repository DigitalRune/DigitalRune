#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples shows how to use a SceneCaptureNode to render the 3D scene into an offscreen
render target. The resulting texture is applied to a model in the scene.",
    @"A static camera is placed in the scene. TV models are loaded and SceneCaptureNodes are added
to these models. When a SceneCaptureNode is within the view frustum, the image of the static camera
is rendered to an offscreen render target. The render target is used as the texture of the TV model.
The texture is also drawn in the bottom right corner.

Important note: The SceneCaptureNode is attached to the TV model. The render-to-texture operation
is only performed if a SceneCaptureNode is visible from the player camera. If the player camera is
looking away from all TV models, the texture is not updated!",
    114)]
  public class SceneCapture2DSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly CameraNode _sceneCaptureCameraNode;
    private readonly RenderToTexture _renderToTexture;


    public SceneCapture2DSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // Create a graphics screen. This screen has to call the SceneCaptureRenderer 
      // to handle the SceneCaptureNodes!
      _graphicsScreen = new DeferredGraphicsScreen(Services) { DrawReticle = true };
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

      // More standard objects.
      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new StaticSkyObject(Services));
      GameObjectService.Objects.Add(new GroundObject(Services));
      GameObjectService.Objects.Add(new DudeObject(Services));
      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));
      GameObjectService.Objects.Add(new DynamicObject(Services, 5));
      GameObjectService.Objects.Add(new DynamicObject(Services, 6));
      GameObjectService.Objects.Add(new DynamicObject(Services, 7));
      GameObjectService.Objects.Add(new FogObject(Services));
      GameObjectService.Objects.Add(new LavaBallsObject(Services));

      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-3, -8), 0, random.NextFloat(0, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree", scale, new Pose(position, orientation)));
      }

      // Create another camera which defines the view that should be captured.
      _sceneCaptureCameraNode = cameraGameObject.CameraNode.Clone();

      // Define the target for the render-to-texture operations.
      _renderToTexture = new RenderToTexture
      {
        Texture = new RenderTarget2D(
          GraphicsService.GraphicsDevice,
          1280 / 2, 720 / 2, 
          true,
          SurfaceFormat.Color, 
          DepthFormat.Depth24Stencil8)
      };

      // Add a few TV objects.
      for (int i = 0; i < 10; i++)
        GameObjectService.Objects.Add(new DynamicObject(Services, 3));

      // Attach a SceneCaptureNode to each TV mesh. The SceneCaptureNodes share the
      // same RenderToTexture instance, which means that all TVs show the same image.
      // The SceneCaptureNode has the same bounding shape as the TV mesh. This shape
      // is used for culling: The texture is only updated when at least one TV is 
      // within the player's view.
      foreach (var meshNode in _graphicsScreen.Scene.GetDescendants().OfType<MeshNode>().Where(n => n.Name == "TV"))
      {
        if (meshNode.Children == null)
          meshNode.Children = new SceneNodeCollection();

        meshNode.Children.Add(new SceneCaptureNode(_renderToTexture)
        {
          CameraNode = _sceneCaptureCameraNode,

          // For culling: Assign TV shape to SceneCaptureNode.
          Shape = meshNode.Shape,
        });
      }

      // Get the material of the TV mesh.
      var tvModel = _graphicsScreen.Scene
                                  .GetDescendants()
                                  .OfType<ModelNode>()
                                  .First(n => n.Name == "TVBox");
      var tvMesh = (MeshNode)tvModel.Children[0];
      var tvScreenMaterial = tvMesh.Mesh
                                   .Materials
                                   .First(m => m.Name == "TestCard");

      // Use the texture on the TV screen.
      tvScreenMaterial["Material"].Set("EmissiveTexture", _renderToTexture.Texture);
      tvScreenMaterial["Material"].Set("Exposure", 0.5f);

      // Also assign the texture to the light projected by the TV screen.
      var lightNode = (LightNode)tvModel.Children[1];
      var projectorLight = (ProjectorLight)lightNode.Light;
      projectorLight.Texture = (Texture2D)_renderToTexture.Texture;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Unload content.
        // We have modified the material of the TV mesh. These changes should not
        // affect other samples. Therefore, we unload the assets. The next sample
        // will reload them with default values.)
        ContentManager.Unload();
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      var debugRenderer = _graphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Draw wireframe of the camera used by the SceneCaptureNodes.
      debugRenderer.DrawObject(_sceneCaptureCameraNode, Color.Red, true, false);

      // Draw the captured image.
      int height = GraphicsService.GraphicsDevice.PresentationParameters.BackBufferHeight;
      debugRenderer.DrawTexture((Texture2D)_renderToTexture.Texture, new Rectangle(0, height - 300, 300, 300));
    }
  }
}
#endif