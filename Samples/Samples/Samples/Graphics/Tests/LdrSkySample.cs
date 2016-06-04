#if !WP7 && !WP8
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Storages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "",
    "",
    1000)]
  public class LdrSkySample : Sample
  {
    private readonly CameraObject _cameraObject;

    private Ephemeris _ephemeris;
#if XBOX
    private DateTime _time;
#else
    private DateTimeOffset _time;
#endif

    private TextureCube _milkyWay;

    private StarfieldNode _starfield;
    private SkyObjectNode _sun;
    private SkyObjectNode _moon;
    private SkyboxNode _milkyWaySkybox;

    private GradientSkyNode _gradientSky;
    private GradientTextureSkyNode _gradientTextureSky;
    private CieSkyFilter _cieSkyFilter;

    private ScatteringSkyNode _scatteringSky;

    private CloudLayerNode _cloudLayerNode;

    private SkyboxNode _skybox;   // The skybox into which the complete sky is rendered.

    private CloudMapRenderer _cloudMapRenderer;
    private SkyRenderer _skyRenderer;
    private ColorEncoder _colorEncoder;
    
    private DebugRenderer _debugRenderer;
    private IList<SceneNode> _skyNodes;

    private HdrFilter _hdrFilter;

    private bool _updateCubeMap = true;




    public LdrSkySample(Microsoft.Xna.Framework.Game game)
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


      var spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
      _debugRenderer = new DebugRenderer(GraphicsService, spriteFont);

      _cieSkyFilter = new CieSkyFilter(GraphicsService);
      _cieSkyFilter.Exposure = 5;
      _cieSkyFilter.Strength = 0.9f;

      _gradientSky = new GradientSkyNode();
      //_gradientSky.GroundColor = new Vector4F(0, 0, 1, 1);
      //_gradientSky.ZenithColor = new Vector4F(0, 0, 1, 1);
      //_gradientSky.FrontColor = new Vector4F(0, 0, 1, 1);
      //_gradientSky.BackColor = new Vector4F(0, 0, 1, 1);
      //_gradientSky.FrontZenithShift = 0.3f;
      //_gradientSky.FrontGroundShift = 0.1f;
      //_gradientSky.BackGroundShift = 0.1f;
      _gradientSky.CieSkyStrength = 0;

      _gradientTextureSky = new GradientTextureSkyNode();
      _gradientTextureSky.TimeOfDay = _time.TimeOfDay;
      _gradientTextureSky.Color = new Vector4F(1);
      _gradientTextureSky.FrontTexture = ContentManager.Load<Texture2D>("Sky/GradientSkyFront");
      _gradientTextureSky.BackTexture = ContentManager.Load<Texture2D>("Sky/GradientSkyBack");
      _gradientTextureSky.CieSkyStrength = 1;

      _scatteringSky = new ScatteringSkyNode();
      _scatteringSky.SunIntensity *= 2;
      _scatteringSky.BetaMie *= 2;
      _scatteringSky.GMie = 0.75f;
      _scatteringSky.ScaleHeight = _scatteringSky.AtmosphereHeight * 0.25f;

      InitializeStarfield();

      _cloudMapRenderer = new CloudMapRenderer(GraphicsService);
      _skyRenderer = new SkyRenderer(GraphicsService);

      _milkyWay = ContentManager.Load<TextureCube>("Sky/MilkyWay");
      _milkyWaySkybox = new SkyboxNode(_milkyWay) { Color = new Vector3F(0.05f) };

      _sun = new SkyObjectNode
      {
        GlowColor0 = new Vector3F(1, 1, 1) * 5,
        GlowExponent0 = 4000,

        //GlowColor1 = new Vector3F(0.4f) * 0.1f,
        //GlowExponent1 = 100
      };

      _moon = new SkyObjectNode
      {
        Texture = new PackedTexture(ContentManager.Load<Texture2D>("Sky/Moon")),
        SunLight = new Vector3F(1, 1, 1) * 1,
        AmbientLight = new Vector3F(0.001f) * 1,
        LightWrap = 0.1f,
        LightSmoothness = 1,
        AngularDiameter = new Vector2F(MathHelper.ToRadians(5)),

        GlowColor0 = new Vector3F(0.005f * 0),
        GlowCutoffThreshold = 0.001f,
        GlowExponent0 = 100
      };

      var cloudMap = new LayeredCloudMap
      {
        Density = 10,
        Coverage = 0.5f,
        Size = 1024,
      };
      var scale = CreateScale(0.2f);
      cloudMap.Layers[0] = new CloudMapLayer(null, scale * CreateScale(1), -0.5f, 1, 0.011f * 0);
      cloudMap.Layers[1] = new CloudMapLayer(null, scale * CreateScale(1.7f), -0.5f, 1f / 2f, 0.017f * 0);
      cloudMap.Layers[2] = new CloudMapLayer(null, scale * CreateScale(3.97f), -0.5f, 1f / 4f, 0.033f * 0);
      cloudMap.Layers[3] = new CloudMapLayer(null, scale * CreateScale(8.1f), -0.5f, 1f / 8f, 0.043f * 0);
      cloudMap.Layers[4] = new CloudMapLayer(null, scale * CreateScale(16, 17), -0.5f, 1f / 16f, 0.051f * 0);
      cloudMap.Layers[5] = new CloudMapLayer(null, scale * CreateScale(32, 31), -0.5f, 1f / 32f, 0.059f * 0);
      cloudMap.Layers[6] = new CloudMapLayer(null, scale * CreateScale(64, 67), -0.5f, 1f / 64f, 0.067f * 0);
      cloudMap.Layers[7] = new CloudMapLayer(null, scale * CreateScale(128, 127), -0.5f, 1f / 128f, 0.081f * 0);
      _cloudLayerNode = new CloudLayerNode(cloudMap)
      {
        ForwardScatterScale = 2.5f,
        ForwardScatterOffset = 0.3f,
        TextureMatrix = CreateScale(0.5f),
        SkyCurvature = 0.9f,
        NumberOfSamples = 16,
      };

      _ephemeris = new Ephemeris();
      // Approx. location of Ternberg: Latitude = 48, Longitude = 15, Altitude = 300
      _ephemeris.Latitude = 0;
      _ephemeris.Longitude = 15;
      _ephemeris.Altitude = 300;
#if XBOX
      //_time = new DateTime(2013, 5, 1, 17, 17, 0, 0);
      _time = DateTime.Now;
#else
      _time = new DateTimeOffset(2013, 5, 1, 12, 0, 0, 0, TimeSpan.Zero);
      //_time = DateTimeOffset.UtcNow;
#endif
      UpdateEphemeris();

      _milkyWaySkybox.DrawOrder = 0;
      _starfield.DrawOrder = 1;
      _sun.DrawOrder = 2;
      _moon.DrawOrder = 3;
      _scatteringSky.DrawOrder = 4;
      _gradientSky.DrawOrder = 4;
      _gradientTextureSky.DrawOrder = 4;
      _cloudLayerNode.DrawOrder = 5;

      _skyNodes = new SceneNode[]
      {
        _milkyWaySkybox,
        _starfield, 
        _sun, 
        _moon,
        _scatteringSky,
        //_gradientSky,
        //_gradientTextureSky,
        _cloudLayerNode,
      };

      var graphicsDevice = GraphicsService.GraphicsDevice;
      _skybox = new SkyboxNode(
        new RenderTargetCube(graphicsDevice, 512, false, SurfaceFormat.Color, DepthFormat.None))
      {
        Encoding = ColorEncoding.Rgbm,
      };

      _hdrFilter = new HdrFilter(GraphicsService)
      {
        MinExposure = 0.5f,
        MaxExposure = 2,
        BloomIntensity = 1,
        BloomThreshold = 0.6f,
        AdaptionSpeed = 100,
      };

      _colorEncoder = new ColorEncoder(GraphicsService)
      {
        SourceEncoding = ColorEncoding.Rgb,
        TargetEncoding = _skybox.Encoding,
      };
    }


    private void InitializeStarfield()
    {
      // Load star positions and luminance from file with 9110 predefined stars.
      const int numberOfStars = 9110;
      var stars = new Star[numberOfStars];
      var storage = Services.GetInstance<IStorage>();
      using (var reader = new BinaryReader(storage.OpenFile("Sky/Stars.bin")))
      {
        for (int i = 0; i < numberOfStars; i++)
        {
          stars[i].Position.X = reader.ReadSingle();
          stars[i].Position.Y = reader.ReadSingle();
          stars[i].Position.Z = reader.ReadSingle();

          // To avoid flickering, the star size should be >= 2.8 px.
          stars[i].Size = 2.8f;

          stars[i].Color.X = reader.ReadSingle();
          stars[i].Color.Y = reader.ReadSingle();
          stars[i].Color.Z = reader.ReadSingle();
        }

        Debug.Assert(reader.PeekChar() == -1, "End of file should be reached.");
      }

      _starfield = new StarfieldNode();
      _starfield.Color = new Vector3F(1);
      _starfield.Stars = stars;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _debugRenderer.Dispose();
      }

      base.Dispose(disposing);
    }


    private void UpdateEphemeris()
    {
#if XBOX
      _ephemeris.Time = new DateTimeOffset(_time.Ticks, TimeSpan.Zero);
#else
      _ephemeris.Time = _time;
#endif
      _ephemeris.Update();

      var sunDirection = (Vector3F)_ephemeris.SunDirectionRefracted;
      var sunUp = sunDirection.Orthonormal1;
      var moonDirection = (Vector3F)_ephemeris.MoonPosition.Normalized;
      var moonUp = (Vector3F)_ephemeris.EquatorialToWorld.TransformDirection(Vector3D.Up);

#if true
      _starfield.PoseWorld = new Pose((Matrix33F)_ephemeris.EquatorialToWorld.Minor);
      _sun.LookAt((Vector3F)_ephemeris.SunDirectionRefracted, sunUp);
      _moon.SunDirection = (Vector3F)_ephemeris.SunPosition.Normalized;
#else
      Vector3F sunRotationAxis = new Vector3F(0, -0.1f, 1).Normalized;
      float hour = (float)_time.TimeOfDay.TotalHours / 24;
      Matrix33F sunRotation = Matrix33F.CreateRotation(sunRotationAxis, hour * ConstantsF.TwoPi - ConstantsF.PiOver2);

      _starfield.Orientation = sunRotation;
      _sun.Direction = sunRotation * new Vector3F(1, 0, 0);
      _moon.SunDirection = _sun.Direction;
#endif

      _milkyWaySkybox.PoseWorld = new Pose(
        (Matrix33F)_ephemeris.EquatorialToWorld.Minor
        * Matrix33F.CreateRotationZ(ConstantsF.PiOver2)
        * Matrix33F.CreateRotationX(ConstantsF.PiOver2));

      _moon.LookAt(moonDirection, moonUp);
      _cieSkyFilter.SunDirection = sunDirection;
      _gradientSky.SunDirection = sunDirection;
      _gradientTextureSky.SunDirection = sunDirection;
      _gradientTextureSky.TimeOfDay = _time.TimeOfDay;
      _scatteringSky.SunDirection = sunDirection;

      _cloudLayerNode.SunDirection = _scatteringSky.SunDirection;
      _cloudLayerNode.SunLight = ChangeSaturation(_scatteringSky.GetSunlight() / 5f, 1);
      //_cloudPlaneRenderer.Color = new Vector4F(ChangeSaturation(_scatteringSky.GetFogColor(128), 0.9f) * 1.0f, 1);
      //Vector3F c = (_scatteringSky.GetFogColor(128) + _scatteringSky.GetSunlight() / 10) / 2;
      //_cloudPlaneRenderer.Color = new Vector4F(c, 1);
      _cloudLayerNode.AmbientLight = _scatteringSky.GetAmbientLight(1024) / 6f;
      //_cloudLayerNode.AmbientLight = _scatteringSky.GetFogColor(128) * _scatteringSky.GetAmbientLight(256).Length / 6f;
    }


    public override void Update(GameTime gameTime)
    {
      //var deltaTime = gameTime.ElapsedGameTime;

      var dateDelta = TimeSpan.FromHours(0.04);
      bool updateEphemeris = false;
      if (InputService.IsDown(Keys.Up))
      {
        _time += dateDelta;
        updateEphemeris = true;
      }
      else if (InputService.IsDown(Keys.Down))
      {
        _time -= dateDelta;
        updateEphemeris = true;
      }

      if (updateEphemeris)
      {
        UpdateEphemeris();
      }

      //var cloudMap = (LayeredCloudMap)_cloudLayerNode.CloudMap;
      //for (int i = 0; i < 8; i++)
      //{
      //  var matrix = cloudMap.Layers[i].TextureMatrix;

      //  // Increase translation to scroll texture.
      //  if (i % 2 == 0)
      //    matrix.M12 += (float)deltaTime.TotalSeconds * (1) * 0.001f * matrix.M00;
      //  else
      //    matrix.M12 += (float)deltaTime.TotalSeconds * (1) * 0.001f / matrix.M11;

      //  cloudMap.Layers[i].TextureMatrix = matrix;
      //}

      // TODO: Don't update cube map every frame.
      _updateCubeMap = true;

      base.Update(gameTime);
    }


    private void Render(RenderContext context)
    {
      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      if (_updateCubeMap)
      {
        _updateCubeMap = false;

        _cloudMapRenderer.Render(_skyNodes, context);

        // Create a camera with 45° FOV for a single cube map face.
        var perspectiveProjection = new PerspectiveProjection();
        perspectiveProjection.SetFieldOfView(ConstantsF.PiOver2, 1, 1, 100);
        context.CameraNode = new CameraNode(new Camera(perspectiveProjection));

        var size = _skybox.Texture.Size;
        var hdrFormat = new RenderTargetFormat(size, size, false, SurfaceFormat.HdrBlendable, DepthFormat.None);
        var hdrTarget = context.GraphicsService.RenderTargetPool.Obtain2D(hdrFormat);
        var ldrFormat = new RenderTargetFormat(size, size, false, SurfaceFormat.Color, DepthFormat.None);
        var ldrTarget = context.GraphicsService.RenderTargetPool.Obtain2D(ldrFormat);

        var spriteBatch = GraphicsService.GetSpriteBatch();
        for (int side = 0; side < 6; side++)
        {
          // Rotate camera to face the current cube map face.
          var cubeMapFace = (CubeMapFace)side;
          context.CameraNode.View = Matrix44F.CreateLookAt(
            new Vector3F(),
            GraphicsHelper.GetCubeMapForwardDirection(cubeMapFace),
            GraphicsHelper.GetCubeMapUpDirection(cubeMapFace));

          // Render sky into HDR render target.
          graphicsDevice.SetRenderTarget(hdrTarget);
          context.RenderTarget = hdrTarget;
          context.Viewport = graphicsDevice.Viewport;
          graphicsDevice.Clear(Color.Black);
          _skyRenderer.Render(_skyNodes, context);

          graphicsDevice.BlendState = BlendState.Opaque;

          // Convert HDR to RGBM.
          context.SourceTexture = hdrTarget;
          context.RenderTarget = ldrTarget;
          _colorEncoder.Process(context);
          context.SourceTexture = null;

          // Copy RGBM texture into cube map face.
          graphicsDevice.SetRenderTarget((RenderTargetCube)_skybox.Texture, cubeMapFace);
          spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null);
          spriteBatch.Draw(ldrTarget, new Vector2(0, 0), Color.White);
          spriteBatch.End();
        }

        context.GraphicsService.RenderTargetPool.Recycle(ldrTarget);
        context.GraphicsService.RenderTargetPool.Recycle(hdrTarget);
      }

      graphicsDevice.BlendState = BlendState.Opaque;
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

      context.CameraNode = _cameraObject.CameraNode;

      var tempFormat = new RenderTargetFormat(originalRenderTarget);
      tempFormat.SurfaceFormat = SurfaceFormat.HdrBlendable;
      var tempTarget = context.GraphicsService.RenderTargetPool.Obtain2D(tempFormat);
      graphicsDevice.SetRenderTarget(tempTarget);
      graphicsDevice.Viewport = originalViewport;
      context.RenderTarget = tempTarget;
      context.Viewport = originalViewport;

      _skyRenderer.Render(_skybox, context);

      context.SourceTexture = tempTarget;
      context.RenderTarget = originalRenderTarget;
      _hdrFilter.Process(context);
      context.SourceTexture = null;

      context.GraphicsService.RenderTargetPool.Recycle(tempTarget);

      RenderDebugInfo(context);

      context.CameraNode = null;
    }


    private void RenderDebugInfo(RenderContext context)
    {
      _debugRenderer.Clear();
      _debugRenderer.DrawAxes(Pose.Identity, 0.5f, true);

      //_debugRenderer.DrawTexture(_cloudLayerNode._renderTarget, new Rectangle(1280-512, 0, 512, 512));

#if XBOX
      _debugRenderer.DrawText(_ephemeris.Time.DateTime.ToString());
#else
      _debugRenderer.DrawText(_ephemeris.Time.ToString());
#endif

      _debugRenderer.PointSize = 10;

      var extraterrestrialSunlight = Ephemeris.ExtraterrestrialSunlight;

      Vector3F sun;
      Vector3F ambient;
      Ephemeris.GetSunlight(_scatteringSky.ObserverAltitude, 2.2f, _scatteringSky.SunDirection, out sun, out ambient);

      var scatterSun = _scatteringSky.GetSunlight() / _scatteringSky.SunIntensity * extraterrestrialSunlight;
      var scatterAmbient = _scatteringSky.GetAmbientLight(1024);
      scatterAmbient = scatterAmbient / _scatteringSky.SunIntensity * extraterrestrialSunlight;

      var scatterFog = _scatteringSky.GetFogColor(128) / _scatteringSky.SunIntensity * extraterrestrialSunlight;
      var luminance = Vector3F.Dot(GraphicsHelper.LuminanceWeights, scatterFog);
      scatterFog = InterpolationHelper.Lerp(scatterFog, new Vector3F(luminance), 0.7f);

      _debugRenderer.DrawText("Extraterrestrial sun intensity:" + extraterrestrialSunlight.Length);
      _debugRenderer.DrawText("Spectrum sun intensity:" + sun.Length);
      _debugRenderer.DrawText("Scatter sun intensity:" + scatterSun.Length);

      _debugRenderer.DrawText("\nSpectrum ambient intensity:" + ambient.Length);
      _debugRenderer.DrawText("Scatter ambient intensity:" + scatterAmbient.Length);

      _debugRenderer.DrawText("\nScatter fog intensity:" + scatterFog.Length);

      _debugRenderer.DrawPoint(new Vector3F(-0.5f, 0, 0), new Color((Vector3)extraterrestrialSunlight.Normalized), true);

      sun.TryNormalize();
      ambient /= ambient.Length;
      _debugRenderer.DrawPoint(new Vector3F(0, 0, 0), new Color((Vector3)sun), true);
      _debugRenderer.DrawPoint(new Vector3F(0, -0.5f, 0), new Color((Vector3)ambient), true);

      scatterSun.TryNormalize();
      scatterAmbient.TryNormalize();
      _debugRenderer.DrawPoint(new Vector3F(0.5f, 0, 0), new Color((Vector3)scatterSun), true);
      _debugRenderer.DrawPoint(new Vector3F(0.5f, -0.5f, 0), new Color((Vector3)scatterAmbient), true);

      scatterFog.TryNormalize();
      _debugRenderer.DrawPoint(new Vector3F(0, 0.5f, 0), new Color((Vector3)scatterFog), true);

      _debugRenderer.PointSize = 40f;
      _debugRenderer.Render(context);
    }


    /// <summary>
    /// Creates the texture matrix for scaling texture coordinates.
    /// </summary>
    /// <param name="s">The uniform scale factor.</param>
    /// <returns>The texture matrix.</returns>
    private static Matrix33F CreateScale(float s)
    {
      return CreateScale(s, s);
    }


    /// <summary>
    /// Creates the texture matrix for scaling the texture coordinates.
    /// </summary>
    /// <param name="su">The scale factor for u texture coordinates.</param>
    /// <param name="sv">The scale factor for v texture coordinates.</param>
    /// <returns>The texture matrix.</returns>
    private static Matrix33F CreateScale(float su, float sv)
    {
      return new Matrix33F(
        su, 0, 0,
        0, sv, 0,
        0, 0, 1);
    }


    /// <summary>
    /// Changes the saturation of a color.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <param name="saturation">
    /// The saturation. Less than 1 to desaturate the color, greater than 1 to saturate the color.
    /// </param>
    /// <returns>The saturated color.</returns>
    private static Vector3F ChangeSaturation(Vector3F color, float saturation)
    {
      float colorDesaturated = Vector3F.Dot(GraphicsHelper.LuminanceWeights, color);
      return InterpolationHelper.Lerp(new Vector3F(colorDesaturated), color, saturation);
    }
  }
}
#endif