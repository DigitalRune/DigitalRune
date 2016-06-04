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
    @"This sample shows how to use a SceneCaptureNode to capture an environment map.",
    @"The 'Bubble' model is placed in the scene. A SceneCaptureNode with an cube map render target
and a CameraNode are placed in the center of the model. The SceneCaptureRenderer will render the
scene, as seen from center of the model, to the cube map. The cube map is used as a reflection map
on the model.",
    115)]
  public class SceneCaptureCubeSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;


    public SceneCaptureCubeSample(Microsoft.Xna.Framework.Game game)
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
      //GameObjectService.Objects.Add(new StaticSkyObject(Services));
      GameObjectService.Objects.Add(new DynamicSkyObject(Services, true, false, true));
      GameObjectService.Objects.Add(new GroundObject(Services));
      GameObjectService.Objects.Add(new DudeObject(Services));
      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));
      GameObjectService.Objects.Add(new DynamicObject(Services, 5));
      GameObjectService.Objects.Add(new DynamicObject(Services, 6));
      GameObjectService.Objects.Add(new DynamicObject(Services, 7));
      GameObjectService.Objects.Add(new FogObject(Services) { AttachToCamera = true });
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

      // Load the "Bubble" mesh and place it at a fixed position in the scene.
      var modelNode = ContentManager.Load<ModelNode>("Bubble/Bubble");
      var meshNode = modelNode.GetDescendants().OfType<MeshNode>().First().Clone();
      meshNode.PoseWorld = new Pose(new Vector3F(0, 1, 0));
      _graphicsScreen.Scene.Children.Add(meshNode);

      // Surface of the mesh should reflect the scene in real-time. Reflections are
      // created using environment mapping: The scene is rendered into a cube map,
      // which is then applied to the mesh.
      // To render the scene into a cube map, we need to define a CameraNode and a
      // SceneCaptureNode: The CameraNode defines the point from where the scene is
      // captured. The SceneCaptureNode defines where and in which format the captured
      // image is needed.

      // Attach a camera to the center of the mesh.
      var projection = new PerspectiveProjection();
      projection.SetFieldOfView(ConstantsF.PiOver2, 1, 0.1f, 20);
      var captureCameraNode = new CameraNode(new Camera(projection));
      meshNode.Children = new SceneNodeCollection { captureCameraNode };

      // Attach a SceneCaptureNode with a cube map render target to the mesh.
      var renderToTexture = new RenderToTexture
      {
        Texture = new RenderTargetCube(
          GraphicsService.GraphicsDevice,
          256,
          true,
          SurfaceFormat.Color,
          DepthFormat.None),
      };
      var sceneCaptureNode = new SceneCaptureNode(renderToTexture)
      {
        Shape = meshNode.Shape,
        CameraNode = captureCameraNode,
      };
      meshNode.Children.Add(sceneCaptureNode);

      // The bubble model uses a special effect and is rendered in the "AlphaBlend"
      // render pass. Let's modify the effect parameters to use the created cube map
      // as the reflection map of the bubble.
      var effectBinding = meshNode.Mesh.Materials[0]["AlphaBlend"];
      effectBinding.Set("ReflectionStrength", 0.5f);
      effectBinding.Set("RefractionStrength", 0.0f);
      effectBinding.Set("FresnelBias", 1.0f);
      effectBinding.Set("BlendMode", 1.0f);
      effectBinding.Set("Alpha", 1.0f);
      effectBinding.Set("CustomEnvironmentMap", (TextureCube)renderToTexture.Texture);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Unload content.
        // We have modified the material of the a mesh. These changes should not
        // affect other samples. Therefore, we unload the assets. The next sample
        // will reload them with default values.)
        ContentManager.Unload();
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();
    }
  }
}
#endif