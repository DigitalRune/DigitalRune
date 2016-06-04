#if !WP7 && !WP8 && !XBOX
using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune;
using DigitalRune.Game;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // Creates a procedural terrain.
  // This class is similar to TerrainObject, except that the height values are not loaded from
  // a height texture, instead they are created by the ProceduralTerrainCreator class.
  // The material layers do not use blend texture, instead they are based on terrain height and
  // slope values.
  public class ProceduralTerrainObject : GameObject
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    // The resolution (world units per texel) of the detail textures which are splatted
    // onto the terrain.
    private const float DetailCellSize = 0.005f;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IServiceLocator _services;
    private IGraphicsService _graphicsService;
    private Simulation _simulation;
    private CameraObject _cameraObject;

    // The terrain tiles.
    private TerrainTile _terrainTile;
    private RigidBody _rigidBody;

    // Input parameter for the procedural terrain creation. 
    private float _noiseWidth = 2;
    private float _noiseHeight = 700;
    private float _noiseMu = 1.03f;   // 1 = Perlin noise, > 1 = noise with exponential distribution.

    // Some flags which tell when we have to re-initialize the terrain.
    private bool _updateGeometryTexture;
    private float _previousCameraFar;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    // The scene node which represents the whole terrain.
    public TerrainNode TerrainNode { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public ProceduralTerrainObject(IServiceLocator services)
    {
      if (services == null)
        throw new ArgumentNullException("services");

      _services = services;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    protected override void OnLoad()
    {
      // Get common services and game objects.
      _graphicsService = _services.GetInstance<IGraphicsService>();
      var content = _services.GetInstance<ContentManager>();
      var scene = _services.GetInstance<IScene>();
      _simulation = _services.GetInstance<Simulation>();
      var gameObjectService = _services.GetInstance<IGameObjectService>();
      _cameraObject = gameObjectService.Objects.OfType<CameraObject>().First();
      _previousCameraFar = _cameraObject.CameraNode.Camera.Projection.Far;

      // Create a new terrain.
      var terrain = new Terrain();
      _terrainTile = new TerrainTile(_graphicsService)
      {
        CellSize = 2,
      };
      terrain.Tiles.Add(_terrainTile);

      var shadowMapEffect = content.Load<Effect>("DigitalRune/Terrain/TerrainShadowMap");
      var gBufferEffect = content.Load<Effect>("DigitalRune/Terrain/TerrainGBuffer");
      var materialEffect = content.Load<Effect>("DigitalRune/Terrain/TerrainMaterial");
      var material = new Material
      {
        { "ShadowMap", new EffectBinding(_graphicsService, shadowMapEffect, null, EffectParameterHint.Material) },
        { "GBuffer", new EffectBinding(_graphicsService, gBufferEffect, null, EffectParameterHint.Material) },
        { "Material", new EffectBinding(_graphicsService, materialEffect, null, EffectParameterHint.Material) }
      };
      TerrainNode = new TerrainNode(terrain, material)
      {
        BaseClipmap =
        {
          CellsPerLevel = 128,
          NumberOfLevels = 6,
        },
        DetailClipmap =
        {
          CellsPerLevel = 1364,
          NumberOfLevels = 9,
        },
      };
      scene.Children.Add(TerrainNode);

      // Create a rigid body with a height field for collision detection.
      var heightField = new HeightField
      {
        Depth = 1,
        UseFastCollisionApproximation = false,
      };
      _rigidBody = new RigidBody(heightField, new MassFrame(), null)
      {
        MotionType = MotionType.Static,
        UserData = _terrainTile,
      };
      _simulation.RigidBodies.Add(_rigidBody);

      InitializeHeightsAndNormals();

      InitializeClipmapCellSizes();

      InitializeTerrainLayers(content);

      // Enable mipmaps when using anisotropic filtering on AMD graphics cards:
      //TerrainNode.DetailClipmap.EnableMipMap = true;

      CreateGuiControls();
    }


    public void InitializeHeightsAndNormals()
    {
      // Create a procedural height field.
      int numberOfSamples = 1025;
      float[] heights = new float[numberOfSamples * numberOfSamples];
      var creator = new ProceduralTerrainCreator(RandomHelper.Random.Next(), 256, _noiseMu);
      creator.CreateTerrain(
        7777, 9999, _noiseWidth * _terrainTile.CellSize, _noiseWidth * _terrainTile.CellSize,
        0, _noiseHeight, 8, heights, numberOfSamples, numberOfSamples);

      float tileSize = (numberOfSamples - 1) * _terrainTile.CellSize;

      _terrainTile.OriginX = -tileSize / 2;
      _terrainTile.OriginZ = -tileSize / 2;

      Debug.Assert(Numeric.IsZero(_terrainTile.OriginX % _terrainTile.CellSize), "The tile origin must be an integer multiple of the cell size.");
      Debug.Assert(Numeric.IsZero(_terrainTile.OriginZ % _terrainTile.CellSize), "The tile origin must be an integer multiple of the cell size.");

      //TerrainHelper.SmoothTexture(heights, numberOfSamples, numberOfSamples, 1e10f);

      // Rebuild height texture.
      Texture2D heightTexture = _terrainTile.HeightTexture;
      TerrainHelper.CreateHeightTexture(
        _graphicsService.GraphicsDevice,
        heights,
        numberOfSamples,
        numberOfSamples,
        false,
        ref heightTexture);

      // Rebuild normal texture.
      Texture2D normalTexture = _terrainTile.NormalTexture;
      TerrainHelper.CreateNormalTexture(
        _graphicsService.GraphicsDevice,
        heights,
        numberOfSamples,
        numberOfSamples,
        _terrainTile.CellSize,
        false,
        ref normalTexture);

      _terrainTile.HeightTexture = heightTexture;
      _terrainTile.NormalTexture = normalTexture;

      // Set the height field data for collision detection.
      var heightField = (HeightField)_rigidBody.Shape;
      heightField.OriginX = _terrainTile.OriginX;
      heightField.OriginZ = _terrainTile.OriginZ;
      heightField.WidthX = tileSize;
      heightField.WidthZ = tileSize;
      heightField.SetSamples(heights, heightTexture.Width, heightTexture.Height);
      heightField.Invalidate();

      TerrainNode.Terrain.Invalidate();
    }


    // Initialize the terrain layers which define the detail textures which are painted onto
    // the terrain.
    // The materials are blended based on the terrain heights and slopes.
    private void InitializeTerrainLayers(ContentManager content)
    {
      var materialGravel = new TerrainMaterialLayer(_graphicsService)
      {
        DiffuseTexture = content.Load<Texture2D>("Terrain/Gravel-Diffuse"),
        NormalTexture = content.Load<Texture2D>("Terrain/Gravel-Normal"),
        SpecularTexture = content.Load<Texture2D>("Terrain/Gravel-Specular"),
        DiffuseColor = new Vector3F(1 / 0.246f, 1 / 0.205f, 1 / 0.171f) * new Vector3F(0.042f, 0.039f, 0.027f),
        TileSize = DetailCellSize * 512,
      };
      _terrainTile.Layers.Add(materialGravel);

      var materialGrass = new TerrainMaterialLayer(_graphicsService)
      {
        DiffuseTexture = content.Load<Texture2D>("Terrain/Grass-Dry-Diffuse"),
        NormalTexture = content.Load<Texture2D>("Terrain/Grass-Dry-Normal"),
        SpecularTexture = content.Load<Texture2D>("Terrain/Grass-Dry-Specular"),
        DiffuseColor = new Vector3F(0.17f, 0.20f, 0.11f),
        TileSize = DetailCellSize * 1024,
        TerrainHeightMin = -1000,
        TerrainHeightMax = 40,
        TerrainSlopeMin = -1,
        TerrainSlopeMax = 0.3f,
      };
      _terrainTile.Layers.Add(materialGrass);

      var materialGrassDry = new TerrainMaterialLayer(_graphicsService)
      {
        DiffuseTexture = content.Load<Texture2D>("Terrain/Grass-Dry-Diffuse"),
        NormalTexture = content.Load<Texture2D>("Terrain/Grass-Dry-Normal"),
        SpecularTexture = content.Load<Texture2D>("Terrain/Grass-Dry-Specular"),
        DiffuseColor = new Vector3F(0.15f, 0.18f, 0.12f),
        TileSize = DetailCellSize * 1024,
        TerrainHeightMin = -1000,
        TerrainHeightMax = 60,
        TerrainHeightBlendRange = 10,
        TerrainSlopeMin = 0.3f,
        TerrainSlopeMax = 0.4f,
      };
      _terrainTile.Layers.Add(materialGrassDry);

      var materialRock = new TerrainMaterialLayer(_graphicsService)
      {
        DiffuseTexture = content.Load<Texture2D>("Terrain/Rock-02-Diffuse"),
        NormalTexture = content.Load<Texture2D>("Terrain/Rock-02-Normal"),
        SpecularTexture = content.Load<Texture2D>("Terrain/Rock-02-Specular"),
        HeightTexture = content.Load<Texture2D>("Terrain/Rock-02-Height"),
        TileSize = DetailCellSize * 1024 * 10,
        DiffuseColor = new Vector3F(0.15f, 0.15f, 0.12f),
        SpecularColor = new Vector3F(2),
        SpecularPower = 100,
        TerrainHeightMin = -1000,
        TerrainHeightMax = 1000,
        TerrainHeightBlendRange = 20,
        TerrainSlopeMin = 0.5f,
        TerrainSlopeMax = 3,
      };
      _terrainTile.Layers.Add(materialRock);
      var materialRockDetail = new TerrainMaterialLayer(_graphicsService)
      {
        DiffuseTexture = content.Load<Texture2D>("Terrain/Rock-02-Diffuse"),
        NormalTexture = content.Load<Texture2D>("Terrain/Rock-02-Normal"),
        SpecularTexture = content.Load<Texture2D>("Terrain/Rock-02-Specular"),
        HeightTexture = content.Load<Texture2D>("Terrain/Rock-02-Height"),
        TileSize = DetailCellSize * 1024,
        DiffuseColor = new Vector3F(0.15f, 0.15f, 0.13f),
        SpecularColor = new Vector3F(0.5f),
        SpecularPower = 20,
        Alpha = 0.7f,
        FadeOutStart = 4,
        FadeOutEnd = 6,
        TerrainHeightMin = -1000,
        TerrainHeightMax = 1000,
        TerrainHeightBlendRange = 10,
        TerrainSlopeMin = 0.5f,
        TerrainSlopeMax = 3,
        BlendHeightInfluence = 1,
      };
      _terrainTile.Layers.Add(materialRockDetail);

      var materialSnow = new TerrainMaterialLayer(_graphicsService)
      {
        DiffuseTexture = content.Load<Texture2D>("Terrain/Snow-Diffuse"),
        NormalTexture = content.Load<Texture2D>("Terrain/Snow-Normal"),
        SpecularTexture = content.Load<Texture2D>("Terrain/Snow-Specular"),
        TileSize = DetailCellSize * 512,
        DiffuseColor = new Vector3F(1),
        SpecularColor = new Vector3F(1),
        SpecularPower = 100,
        TerrainHeightMin = 60,
        TerrainHeightMax = 1000,
        TerrainHeightBlendRange = 1,
        TerrainSlopeMin = -1,
        TerrainSlopeMax = 0.5f,
        TerrainSlopeBlendRange = 0.1f,
      };
      _terrainTile.Layers.Add(materialSnow);
    }


    // Initialize the cell sizes of the clipmaps.
    private void InitializeClipmapCellSizes()
    {
      // See TerrainObject.cs for detailed comments.
      TerrainNode.BaseClipmap.CellSizes[0] = _terrainTile.CellSize;
      TerrainNode.DetailClipmap.CellSizes[0] = DetailCellSize;

      var projection = _cameraObject.CameraNode.Camera.Projection;
      Vector3F nearCorner = new Vector3F(projection.Left, projection.Bottom, projection.Near);
      var maxViewDistance = (nearCorner / projection.Near * projection.Far).Length;
      var maxTerrainSize = 2 * maxViewDistance;
      var terrainExtent = TerrainNode.Terrain.Aabb.Extent;
      maxTerrainSize = Math.Min(maxTerrainSize, Math.Max(terrainExtent.X, terrainExtent.Z));

      int cellsPerLevelWithoutFilterBorder = TerrainNode.DetailClipmap.CellsPerLevel - (8 + 8 + 1);

      int numberOfLevels = TerrainNode.DetailClipmap.NumberOfLevels;
      float b = (float)Math.Pow(maxTerrainSize / (cellsPerLevelWithoutFilterBorder * DetailCellSize), 1.0f / (numberOfLevels - 1.0f));

      var cellSizes = new float[numberOfLevels];
      for (int i = 1; i < numberOfLevels; i++)
        TerrainNode.DetailClipmap.CellSizes[i] = DetailCellSize * (float)Math.Pow(b, i);

      if (numberOfLevels == 1)
        cellSizes[0] = maxTerrainSize / cellsPerLevelWithoutFilterBorder;

      TerrainNode.BaseClipmap.Invalidate();
      TerrainNode.DetailClipmap.Invalidate();
    }


    protected override void OnUnload()
    {
      _cameraObject = null;

      // Remove terrain from scene.
      TerrainNode.Parent.Children.Remove(TerrainNode);

      TerrainNode.Dispose(false);
      TerrainNode = null;

      // We have to dispose the textures which were not loaded via the content manager.
      _terrainTile.HeightTexture.SafeDispose();
      _terrainTile.HeightTexture = null;
      _terrainTile.NormalTexture.SafeDispose();
      _terrainTile.NormalTexture = null;

      _rigidBody.Simulation.RigidBodies.Remove(_rigidBody);

      _terrainTile = null;
      _rigidBody = null;
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Re-initialize geometry if the user has changed an important terrain property.
      if (_updateGeometryTexture)
        InitializeHeightsAndNormals();

      // Re-initialize cell sizes if the geometry has changed or if the camera far distance
      // was changed.
      var projection = _cameraObject.CameraNode.Camera.Projection;
      float cameraFar = projection.Far;
      if (_previousCameraFar != cameraFar || _updateGeometryTexture)
        InitializeClipmapCellSizes();

      _previousCameraFar = cameraFar;
      _updateGeometryTexture = false;
    }


    // Add GUI controls to the Options window.
    private void CreateGuiControls()
    {
      var sampleFramework = _services.GetInstance<SampleFramework>();
      var optionsPanel = sampleFramework.AddOptions("Game Objects");
      var panel = SampleHelper.AddGroupBox(optionsPanel, "ProceduralTerrainObject");

      SampleHelper.AddSlider(
        panel, "Noise width", null, 0.1f, 10f, _noiseWidth,
        value =>
        {
          _noiseWidth = value;
          _updateGeometryTexture = true;
        });

      SampleHelper.AddSlider(
        panel, "Noise height", null, 0, 1000, _noiseHeight,
        value =>
        {
          _noiseHeight = value;
          _updateGeometryTexture = true;
        });

      SampleHelper.AddSlider(
        panel, "Noise mu", null, 1, 1.16f, _noiseMu,
        value =>
        {
          _noiseMu = value;
          _updateGeometryTexture = true;
        });
    }
    #endregion
  }
}
#endif
