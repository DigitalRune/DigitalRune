#if !ANDROID && !IOS
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
    @"This example demonstrates how to add lens flare effects for directional and local light 
sources.",
    @"Hint: One lens flare effect is not immediately visible when the sample is start. Look up
and search the sky!",
    10)]
  public class LensFlareSample : Sample
  {
    private readonly CameraObject _cameraObject;
    private readonly Scene _scene;
    private readonly MeshRenderer _meshRenderer;           // Handles MeshNodes.
    private readonly LensFlareRenderer _lensFlareRenderer; // Handles LensFlareNodes.


    public LensFlareSample(Microsoft.Xna.Framework.Game game)
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

      // Create a new empty scene.
      _scene = new Scene();

      // Add the camera node to the scene.
      _scene.Children.Add(_cameraObject.CameraNode);

      // Add a few models to the scene.
      var ground = ContentManager.Load<ModelNode>("Ground/Ground").Clone();
      _scene.Children.Add(ground);

      var box = ContentManager.Load<ModelNode>("MetalGrateBox/MetalGrateBox").Clone();
      box.PoseLocal = new Pose(new Vector3F(0.5f, 0.5f, 0.5f), Matrix33F.CreateRotationY(0.1f));
      _scene.Children.Add(box);

      // Add some lights to the scene which have the same properties as the lights 
      // of BasicEffect.EnableDefaultLighting().
      SceneSample.InitializeDefaultXnaLights(_scene);

      // Add a lens flare for the sun light.
      var lensFlare = new LensFlare(true);  // The sun is a directional light source.
      lensFlare.Name = "Sun Flare";

      // The Size determines the screen size of the lens flare elements. The value
      // is relative to the viewport height.
      lensFlare.Size = 0.28f;       // 0.28 * viewport height

      // The QuerySize of a directional light is the estimated size relative to the 
      // viewport. This value is used in the hardware occlusion query, which determines 
      // whether the lens flare is visible.
      lensFlare.QuerySize = 0.18f;  // 0.18 * viewport height

      // All lens flare elements are packed into one texture ("texture atlas"). 
      // The PackedTexture identifies an element within the texture atlas.
      // See file Media/LensFlare/LensFlares.png.
      // (Credits: The sun lens flare was copied from the XNA racing game - http://exdream.com/XnaRacingGame/.)
      var lensFlareTexture = ContentManager.Load<Texture2D>("LensFlare/LensFlares");
      var circleTexture = new PackedTexture("Circle", lensFlareTexture, new Vector2F(0, 0), new Vector2F(0.25f, 0.5f));
      var glowTexture = new PackedTexture("Glow", lensFlareTexture, new Vector2F(0.25f, 0), new Vector2F(0.25f, 0.5f));
      var ringTexture = new PackedTexture("Ring", lensFlareTexture, new Vector2F(0.5f, 0), new Vector2F(0.25f, 0.5f));
      var haloTexture = new PackedTexture("Halo", lensFlareTexture, new Vector2F(0.75f, 0), new Vector2F(0.25f, 0.5f));
      var sunTexture = new PackedTexture("Sun", lensFlareTexture, new Vector2F(0, 0.5f), new Vector2F(0.25f, 0.5f));
      var streaksTexture = new PackedTexture("Streaks", lensFlareTexture, new Vector2F(0.25f, 0.5f), new Vector2F(0.25f, 0.5f));
      var flareTexture = new PackedTexture("Flare", lensFlareTexture, new Vector2F(0.5f, 0.5f), new Vector2F(0.25f, 0.5f));

      // Add a few elements (circles, glow, rings, halos, streaks, ...) to the lens flare. 
      lensFlare.Elements.Add(new LensFlareElement(-0.2f, 0.55f, 0.0f, new Color(175, 175, 255, 20), new Vector2F(0.5f, 0.5f), circleTexture));
      lensFlare.Elements.Add(new LensFlareElement(0.0f, 0.9f, 0.0f, new Color(255, 255, 255, 255), new Vector2F(0.5f, 0.5f), sunTexture));
      lensFlare.Elements.Add(new LensFlareElement(0.0f, 1.8f, 0.0f, new Color(255, 255, 255, 128), new Vector2F(0.5f, 0.5f), streaksTexture));
      lensFlare.Elements.Add(new LensFlareElement(0.0f, 2.6f, 0.0f, new Color(255, 255, 200, 64), new Vector2F(0.5f, 0.5f), glowTexture));
      lensFlare.Elements.Add(new LensFlareElement(0.5f, 0.12f, 0.0f, new Color(60, 60, 180, 35), new Vector2F(0.5f, 0.5f), circleTexture));
      lensFlare.Elements.Add(new LensFlareElement(0.55f, 0.46f, 0.0f, new Color(100, 100, 200, 60), new Vector2F(0.5f, 0.5f), circleTexture));
      lensFlare.Elements.Add(new LensFlareElement(0.6f, 0.17f, 0.0f, new Color(120, 120, 220, 40), new Vector2F(0.5f, 0.5f), circleTexture));
      lensFlare.Elements.Add(new LensFlareElement(0.85f, 0.2f, 0.0f, new Color(60, 60, 255, 100), new Vector2F(0.5f, 0.5f), ringTexture));
      lensFlare.Elements.Add(new LensFlareElement(1.5f, 0.2f, 0.0f, new Color(255, 60, 60, 130), new Vector2F(0.5f, 0.5f), flareTexture));
      lensFlare.Elements.Add(new LensFlareElement(0.15f, 0.15f, 0.0f, new Color(255, 60, 60, 90), new Vector2F(0.5f, 0.5f), flareTexture));
      lensFlare.Elements.Add(new LensFlareElement(1.3f, 0.6f, 0.0f, new Color(60, 60, 255, 180), new Vector2F(0.5f, 0.5f), haloTexture));
      lensFlare.Elements.Add(new LensFlareElement(1.4f, 0.2f, 0.0f, new Color(220, 80, 80, 98), new Vector2F(0.5f, 0.5f), haloTexture));
      lensFlare.Elements.Add(new LensFlareElement(1.5f, 0.1f, 0.0f, new Color(220, 80, 80, 85), new Vector2F(0.5f, 0.5f), circleTexture));
      lensFlare.Elements.Add(new LensFlareElement(1.6f, 0.5f, 0.0f, new Color(60, 60, 255, 80), new Vector2F(0.5f, 0.5f), haloTexture));
      lensFlare.Elements.Add(new LensFlareElement(1.8f, 0.3f, 0.0f, new Color(90, 60, 255, 110), new Vector2F(0.5f, 0.5f), ringTexture));
      lensFlare.Elements.Add(new LensFlareElement(1.95f, 0.5f, 0.0f, new Color(60, 60, 255, 120), new Vector2F(0.5f, 0.5f), haloTexture));
      lensFlare.Elements.Add(new LensFlareElement(2.0f, 0.15f, 0.0f, new Color(60, 60, 255, 85), new Vector2F(0.5f, 0.5f), circleTexture));

      // The scene node "KeyLight" (defined in SceneSample.InitializeDefaultXnaLights())
      // is the main directional light source. 
      var keyLightNode = _scene.GetDescendants().First(n => n.Name == "KeyLight");

      // Let's attach the lens flare to the "KeyLight" node. 
      // (Note: It is not necessary to attach a lens flare to a light node. Lens flares
      // can be added anywhere within the scene. But attaching the lens flare to the
      // light node ensures that the lens flare always has the same position and direction 
      // as the light source.)
      var lensFlareNode = new LensFlareNode(lensFlare);
      keyLightNode.Children = new SceneNodeCollection();
      keyLightNode.Children.Add(lensFlareNode);

      // Add a second lens flare. 
      // The previous lens flare was a caused by a directional light source (distance = infinite). 
      // This time we add a local lens flare.
      lensFlare = new LensFlare(false);
      lensFlare.Name = "Anamorphic Flare";
      lensFlare.Size = 0.3f;      // 0.3 * viewport height

      // The QuerySize of a local lens flare is estimated size of the light source
      // in world space.
      lensFlare.QuerySize = 0.2f; // 0.2 meters 

      // Add some elements (glow, horizontal streaks, ...) to the lens flare effect.
      var anamorphicFlareTexture = ContentManager.Load<Texture2D>("LensFlare/AnamorphicFlare");
      flareTexture = new PackedTexture("AnamorphicFlare", anamorphicFlareTexture, new Vector2F(0, 0), new Vector2F(1.0f, 87f / 256f));
      var flare1Texture = new PackedTexture("Flare0", anamorphicFlareTexture, new Vector2F(227f / 512f, 88f / 256f), new Vector2F(285f / 512f, 15f / 256f));
      var flare2Texture = new PackedTexture("Flare1", anamorphicFlareTexture, new Vector2F(0, 87f / 256f), new Vector2F(226f / 512f, 168f / 256f));
      lensFlare.Elements.Add(new LensFlareElement(0.0f, 0.8f, 0.0f, new Color(255, 255, 255, 255), new Vector2F(0.5f, 0.5f), flareTexture));
      lensFlare.Elements.Add(new LensFlareElement(1.0f, new Vector2F(0.6f, 0.5f), 0.0f, new Color(172, 172, 255, 32), new Vector2F(0.5f, 0.5f), flare1Texture));
      lensFlare.Elements.Add(new LensFlareElement(1.5f, 1.2f, float.NaN, new Color(200, 200, 255, 24), new Vector2F(0.5f, 0.2f), flare2Texture));
      lensFlare.Elements.Add(new LensFlareElement(2.0f, 2.0f, float.NaN, new Color(172, 172, 255, 48), new Vector2F(0.5f, 0.2f), flare2Texture));

      // Position the lens flare near the origin.
      lensFlareNode = new LensFlareNode(lensFlare);
      lensFlareNode.PoseWorld = new Pose(new Vector3F(-0.5f, 1, 0));
      _scene.Children.Add(lensFlareNode);

      // In this example we need two renderers:
      // The MeshRenderer handles MeshNodes.
      _meshRenderer = new MeshRenderer();

      // The LensFlareRenderer handles LensFlareNodes.
      _lensFlareRenderer = new LensFlareRenderer(GraphicsService);
    }


    public override void Update(GameTime gameTime)
    {
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
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

      // Frustum culling: Get all scene nodes which overlap the view frustum.
      var query = _scene.Query<CameraFrustumQuery>(context.CameraNode, context);

      // Render all meshes that are in the camera frustum.
      context.RenderPass = "Default";
      _meshRenderer.Render(query.SceneNodes, context);

      // Update the visibility of the lens flare effects. 
      // LensFlareRenderer.UpdateOcclusion() needs to be called after all opaque 
      // objects are rendered. The z-buffer needs to contain the depth information 
      // of the scene. The method performs hardware occlusion queries to determine 
      // the visibility of the lens flare nodes. (Note: Hardware occlusion queries 
      // are not supported in Reach profile.)
      _lensFlareRenderer.UpdateOcclusion(query.SceneNodes, context);

      // At this point we could render transparent objects (e.g. alpha blended meshes,
      // particles, etc.). But the sample does not contain any transparent objects.)

      // Render lens flares.
      _lensFlareRenderer.Render(query.SceneNodes, context);

      // Clean up.
      context.RenderPass = null;
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
        _lensFlareRenderer.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
#endif