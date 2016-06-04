using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using DirectionalLight = DigitalRune.Graphics.DirectionalLight;


namespace Samples
{
  // This game object creates a sky using a skybox texture. It also sets the main
  // lights and adds a lens flare for the sun.
  public class StaticSkyObject : GameObject
  {
    private readonly IServiceLocator _services;
    private SkyboxNode _skyboxNode;
    private LightNode _ambientLightNode;
    private LightNode _sunlightNode;


    // The brightness of the sky box.
    public float SkyExposure { get; set; }


    public StaticSkyObject(IServiceLocator services)
    {
      _services = services;
      Name = "StaticSky";
      SkyExposure = 0.2f;
    }


    // OnLoad() is called when the GameObject is added to the IGameObjectService.
    protected override void OnLoad()
    {
      var content = _services.GetInstance<ContentManager>();
      _skyboxNode = new SkyboxNode(content.Load<TextureCube>("Sky2"))
      {
        Color = new Vector3F(SkyExposure),
      };

      // The ambient light.
      var ambientLight = new AmbientLight
      {
        Color = new Vector3F(0.9f, 0.9f, 1f),
        HdrScale = 0.1f,
        Intensity = 0.5f,
        HemisphericAttenuation = 0.8f,
      };
      _ambientLightNode = new LightNode(ambientLight)
      {
        Name = "Ambient",
      };

      // The main directional light.
      var sunlight = new DirectionalLight
      {
        Color = new Vector3F(1, 0.9607844f, 0.9078432f),
        HdrScale = 0.4f,
        DiffuseIntensity = 1,
        SpecularIntensity = 1,
      };
      _sunlightNode = new LightNode(sunlight)
      {
        Name = "Sunlight",
        Priority = 10,   // This is the most important light.
        PoseWorld = new Pose(QuaternionF.CreateRotationY(-1.4f) * QuaternionF.CreateRotationX(-0.6f)),

        // This light uses Cascaded Shadow Mapping.
        Shadow = new CascadedShadow
        {
#if XBOX
          PreferredSize = 512,
#else
          PreferredSize = 1024,
#endif
          Prefer16Bit = true,
        }
      };

      // Add a lens flare for the key light.
      var lensFlare = new LensFlare(true) { QuerySize = 0.2f, Size = 0.2f, Name = "Sun Flare" };
      var lensFlareTexture = content.Load<Texture2D>("LensFlare/LensFlares");
      var circleTexture = new PackedTexture("Circle", lensFlareTexture, new Vector2F(0, 0), new Vector2F(0.25f, 0.5f));
      var glowTexture = new PackedTexture("Glow", lensFlareTexture, new Vector2F(0.25f, 0), new Vector2F(0.25f, 0.5f));
      var ringTexture = new PackedTexture("Ring", lensFlareTexture, new Vector2F(0.5f, 0), new Vector2F(0.25f, 0.5f));
      var haloTexture = new PackedTexture("Halo", lensFlareTexture, new Vector2F(0.75f, 0), new Vector2F(0.25f, 0.5f));
      var sunTexture = new PackedTexture("Sun", lensFlareTexture, new Vector2F(0, 0.5f), new Vector2F(0.25f, 0.5f));
      var streaksTexture = new PackedTexture("Streaks", lensFlareTexture, new Vector2F(0.25f, 0.5f), new Vector2F(0.25f, 0.5f));
      var flareTexture = new PackedTexture("Flare", lensFlareTexture, new Vector2F(0.5f, 0.5f), new Vector2F(0.25f, 0.5f));
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

      // Add lens flare as a child of the sunlight.
      var lensFlareNode = new LensFlareNode(lensFlare);
      _sunlightNode.Children = new SceneNodeCollection();
      _sunlightNode.Children.Add(lensFlareNode);

      // Add scene nodes to scene graph.
      var scene = _services.GetInstance<IScene>();
      scene.Children.Add(_skyboxNode);
      scene.Children.Add(_ambientLightNode);
      scene.Children.Add(_sunlightNode);
    }


    // OnUnload() is called when the GameObject is removed from the IGameObjectService.
    protected override void OnUnload()
    {
      _skyboxNode.Parent.Children.Remove(_skyboxNode);
      _skyboxNode.Dispose(false);
      _skyboxNode = null;

      _ambientLightNode.Parent.Children.Remove(_ambientLightNode);
      _ambientLightNode.Dispose(false);
      _ambientLightNode = null;

      _sunlightNode.Parent.Children.Remove(_sunlightNode);
      _sunlightNode.Dispose(false);
      _sunlightNode = null;
    }
  }
}
