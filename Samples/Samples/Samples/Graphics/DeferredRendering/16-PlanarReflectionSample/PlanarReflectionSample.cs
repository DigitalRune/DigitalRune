#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples shows how to use PlanarReflectionNodes to create reflections on flat objects.",
    @"The scene has two reflective meshes, the ground and a wall. PlanarReflectionNodes are added as
children of these mesh nodes. The DeferredGraphicsScreen uses a PlanarReflectionRenderer to render
the reflections. The meshes use a special effect (MaterialReflective.fx) to apply the created
reflection texture.

Interesting note: Two reflection images are created in each frame, but only when the
PlanarReflectionNodes are not frustum culled. The reflection image of the ground is actually
visible in the reflection of the wall and vice versa. However, the reflection in the reflection
effect is limited, since the reflection texture only captures the first order reflection as seen
from the player camera to make best use of the texture resolution.",
    116)]
  public class PlanarReflectionSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly PlanarReflectionNode _planarReflectionNode0;
    private readonly PlanarReflectionNode _planarReflectionNode1;


    public PlanarReflectionSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // Create a graphics screen. This screen has to call the PlanarReflectionRenderer
      // to handle the PlanarReflectionNodes!
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

#if MONOGAME
      // ----- Workaround for missing effect parameter semantics in MonoGame.
      // The effect used by the reflecting ground object defines some new effect
      // parameters and sets the EffectParameterHint to "PerInstance", e.g.:
      //   texture ReflectionTexture < string Hint = "PerInstance"; >;
      // "PerInstance" means that each mesh instance which uses the effect can 
      // have an individual parameter value, i.e. if there are two instances
      // each instance needs a different ReflectionTexture.
      // MonoGame does not yet support effect parameter annotations in shader 
      // code. But we can add the necessary effect parameter descriptions here:
      var effectInterpreter = GraphicsService.EffectInterpreters.OfType<DefaultEffectInterpreter>().First();
      if (!effectInterpreter.ParameterDescriptions.ContainsKey("ReflectionTexture"))
      {
        effectInterpreter.ParameterDescriptions.Add("ReflectionTexture", (parameter, index) => new EffectParameterDescription(parameter, "ReflectionTexture", index, EffectParameterHint.PerInstance));
        effectInterpreter.ParameterDescriptions.Add("ReflectionTextureSize", (parameter, index) => new EffectParameterDescription(parameter, "ReflectionTextureSize", index, EffectParameterHint.PerInstance));
        effectInterpreter.ParameterDescriptions.Add("ReflectionMatrix", (parameter, index) => new EffectParameterDescription(parameter, "ReflectionMatrix", index, EffectParameterHint.PerInstance));
        effectInterpreter.ParameterDescriptions.Add("ReflectionNormal", (parameter, index) => new EffectParameterDescription(parameter, "ReflectionNormal", index, EffectParameterHint.PerInstance));
      }
#endif

      // Get a ground model which can render a planar reflection. See 
      // GroundReflective/MaterialReflective.fx.
      var groundModel = ContentManager.Load<ModelNode>("GroundReflective/Ground");

      // Use the reflective mesh as the ground.
      var groundMesh = groundModel.GetSubtree().OfType<MeshNode>().First().Clone();
      groundMesh.PoseWorld = new Pose(new Vector3F(0, 0.01f, 0));  // Small y offset to draw above the default ground model from GroundObject.
      _graphicsScreen.Scene.Children.Add(groundMesh);

      // Use another instance of the mesh as a wall.
      var wallMesh = groundMesh.Clone();
      wallMesh.ScaleLocal = new Vector3F(0.2f, 1, 0.1f);
      wallMesh.PoseWorld = new Pose(new Vector3F(5, 2, -5), Matrix33F.CreateRotationY(-0.7f) * Matrix33F.CreateRotationX(ConstantsF.PiOver2));
      _graphicsScreen.Scene.Children.Add(wallMesh);

      // Create a PlanarReflectionNode and add it to the children of the first ground mesh.
      // The RenderToTexture class defines the render target for the reflection.
      var renderToTexture0 = new RenderToTexture
      {
        Texture = new RenderTarget2D(
          GraphicsService.GraphicsDevice, 
          1024, 1024, 
          false,  // No mipmaps. Mipmaps can reduce reflection quality.
          SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8),
      };
      _planarReflectionNode0 = new PlanarReflectionNode(renderToTexture0)
      {
        // The reflection is limited to the bounding shape of the ground mesh.
        Shape = groundMesh.Shape,

        // The normal of the reflection plane.
        NormalLocal = new Vector3F(0, 1, 0),
      };
      groundMesh.Children = new SceneNodeCollection(1) { _planarReflectionNode0 };

      // Add another PlanarReflectionNode to the wall.
      var renderToTexture1 = new RenderToTexture
      {
        Texture = new RenderTarget2D(
          GraphicsService.GraphicsDevice,
          1024, 1024, false, 
          SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8),
      };
      _planarReflectionNode1 = new PlanarReflectionNode(renderToTexture1)
      {
        Shape = groundMesh.Shape,
        NormalLocal = new Vector3F(0, 1, 0),
      };
      wallMesh.Children = new SceneNodeCollection(1) { _planarReflectionNode1 };
      
      // Now we have to use the texture that contains the reflection.
      // We use effect parameter bindings to use the reflection texture in the shader of the meshes.
      SetReflectionEffectParameters(groundMesh, _planarReflectionNode0);
      SetReflectionEffectParameters(wallMesh, _planarReflectionNode1);
    }


    private static void SetReflectionEffectParameters(MeshNode meshNode, PlanarReflectionNode planarReflectionNode)
    {
      // Loop through the materials of the mesh. The material uses the effect 
      // GroundReflective/MaterialReflective.fx.
      foreach (var materialInstance in meshNode.MaterialInstances)
      {
        // Get effect binding for the "Material" render pass. (Not the "GBuffer" or other passes.)
        var effectBinding = materialInstance["Material"];

        // Set reflection texture and size parameters.
        var texture = (Texture2D)planarReflectionNode.RenderToTexture.Texture;
        effectBinding.Set<Texture>("ReflectionTexture", texture);
        effectBinding.Set<Vector2>("ReflectionTextureSize", new Vector2(texture.Width, texture.Height));

        // The reflection texture matrix and the reflection normal may change over
        // time. Therefore, we need to set a delegate that updates the value once
        // per frame.
        effectBinding.Set<Matrix>("ReflectionMatrix", (binding, context) => (Matrix)planarReflectionNode.RenderToTexture.TextureMatrix);
        effectBinding.Set<Vector3>("ReflectionNormal", (binding, context) => (Vector3)planarReflectionNode.NormalWorld);
      }
    }


    public override void Update(GameTime gameTime)
    {
      var debugRenderer = _graphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Draw bounding shapes of PlanarReflectionNodes for debugging.
      //debugRenderer.DrawObject(_planarReflectionNode0, Color.Red, true, false);
      //debugRenderer.DrawObject(_planarReflectionNode1, Color.Red, true, false);
    }
  }
}
#endif