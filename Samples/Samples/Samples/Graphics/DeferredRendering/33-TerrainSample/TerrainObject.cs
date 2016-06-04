#if !WP7 && !WP8 && !XBOX
using System;
using System.Collections.Generic;
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
using DigitalRune.Physics;
using DigitalRune.Threading;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // Creates a large terrain based on height maps.
  public class TerrainObject : GameObject
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // The terrain consists of tiles.
    private class Tile
    {
      // The tile info for rendering.
      public TerrainTile TerrainTile;

      // A rigid body which contains the tile height field for collision detection.
      public RigidBody RigidBody;
    }
    #endregion


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
    private DeferredGraphicsScreen _graphicsScreen;
    private Simulation _simulation;
    private CameraObject _cameraObject;

    // The terrain tiles.
    private Tile[,] _tiles;

    // The min and max terrain height used to scale the terrain.
    private float _minHeight = -132f;
    private float _maxHeight = 540;

    // The smoothness parameter used to smooth the input height texture 
    // (0 = sharp, greater than 0 = smoother).
    private float _smoothness = 1e10f;

    // A flag which determines if terrain height mipmaps use filtering or nearest-neighbor
    // sampling (= simply dropping every other sample).
    private bool _useNearestNeighborMipmaps;

    private bool _showClipmaps;

    // Some flags which tell when we have to re-initialize the terrain.
    private bool _updateGeometryTexture;
    private bool _updateDetailClipmapCellSizes;
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

    public TerrainObject(IServiceLocator services)
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
      _graphicsScreen = _graphicsService.Screens.OfType<DeferredGraphicsScreen>().First();
      var content = _services.GetInstance<ContentManager>();
      var scene = _services.GetInstance<IScene>();
      _simulation = _services.GetInstance<Simulation>();
      var gameObjectService = _services.GetInstance<IGameObjectService>();
      _cameraObject = gameObjectService.Objects.OfType<CameraObject>().First();
      _previousCameraFar = _cameraObject.CameraNode.Camera.Projection.Far;

      // Create a new terrain.
      var terrain = new Terrain();

      // The terrain is made up of terrain tiles which can be loaded independently. 
      // Each terrain tile consists of height and normal textures which define the terrain
      // geometry and terrain layers which define the material (detail textures).
      // In this sample we create 2x2 tiles.
      _tiles = new Tile[2, 2];
      for (int row = 0; row < 2; row++)
      {
        for (int column = 0; column < 2; column++)
        {
          // Create a tile and add it to the terrain.
          // (The tile content is loaded later.)
          var terrainTile = new TerrainTile(_graphicsService)
          {
            CellSize = 1,   // The terrain has a resolution of 1 height sample per world space unit.
          };
          terrain.Tiles.Add(terrainTile);

          // Create a rigid body with a height field for collision detection and add
          // it to the simulation. (The height data is loaded later.)
          var heightField = new HeightField
          {
            Depth = 1,
            UseFastCollisionApproximation = false,
          };
          var rigidBody = new RigidBody(heightField, new MassFrame(), null)
          {
            MotionType = MotionType.Static,
            UserData = terrainTile,
          };
          _simulation.RigidBodies.Add(rigidBody);

          // Store the tile for use later in this sample.
          _tiles[row, column] = new Tile
          {
            TerrainTile = terrainTile,
            RigidBody = rigidBody,
          };
        }
      }

      // Create a terrain node which represents the terrain in the scene graph.
      // The terrain node is rendered by the TerrainRenderer (see DeferredGraphicsScreen).
      // The material used to render the terrain is customizable.  The material must specify 
      // the effects for the different render passes which we use in the DeferredGraphicsScreen 
      // ("ShadowMap", "GBuffer", "Material").
      // The prebuilt DigitalRune content contains standard terrain effects. However, you could
      // change the effects to change how the material is rendered.
      // We can create the material by loading a .drmat file. Or we can create the material in
      // code like this:
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
        // The terrain rendering uses clipmaps.
        // The clipmaps are updated by the TerrainClipmapRenderer (see DeferredGraphicsScreen)
        // when the camera moves.
        // The base clipmap contains the basic geometry info (height, normals, hole info).
        // It also determines the terrain mesh resolution.
        BaseClipmap =
        {
          CellsPerLevel = 128,
          NumberOfLevels = 6
        },
        // The detail clipmap contains the splatted detail textures (e.g. grass, rock, ...).
        // (The max texture size in XNA is 4096x4096. That means we can fit 9 clipmap levels
        // into a single texture.)
        DetailClipmap =
        {
          CellsPerLevel = 1365,
          NumberOfLevels = 9,
        },
      };
      scene.Children.Add(TerrainNode);

      // Load the height and normal maps which define the terrain geometry.
      InitializeHeightsAndNormals();

      // Set the clipmap cell sizes.
      InitializeClipmapCellSizes();

      // Create the terrain layers which define the detail textures (e.g. grass, rock, ...)
      InitializeTerrainLayers(content);

      // Special note for AMD GPUs:
      // If we want anisotropic filtering for the terrain, then we need to enable mipmaps for
      // AMD GPUs. NVIDIA and Intel can do anisotropic filtering without mipmaps.
      //TerrainNode.DetailClipmap.EnableMipMap = true;

      CreateGuiControls();
    }


    // Initialize the terrain geometry from height textures.
    public void InitializeHeightsAndNormals()
    {
      var content = _services.GetInstance<ContentManager>();

      // Create the height and normal texture for the 4 tiles.
      // We can do this in parallel. We use following lock for parts which are not thread-safe.
      var lockObject = new object();

      Parallel.For(0, 2, row =>
      //for (int row = 0; row < 2; row++)
      {
        Parallel.For(0, 2, column =>
        //for (int column = 0; column < 2; column++)
        {
          string tilePostfix = "-" + row + "-" + column; // e.g. "-0-1"
          var tile = _tiles[row, column];
          var terrainTile = tile.TerrainTile;

          // The height data is loaded from a 16-bit grayscale texture.
          Texture2D inputHeightTexture;
          lock (lockObject)
             inputHeightTexture = content.Load<Texture2D>("Terrain/Terrain001-Height" + tilePostfix);

          Debug.Assert(inputHeightTexture.Width == inputHeightTexture.Height, "This code assumes that terrain tiles are square.");

          // The height textures of the tiles overlap by one texel to avoid gaps. That means, the
          // input height texture for each tile is 1025 x 1025. The last texture column of the top
          // left tile is the same as the first column of the top right tile. The last row of the
          // top left tile is the same as the first row of the bottom left tile.
          // The tile size in world space units is:
          float tileSize = (inputHeightTexture.Width - 1) * terrainTile.CellSize;

          terrainTile.OriginX = -tileSize + tileSize * column;
          terrainTile.OriginZ = -tileSize + tileSize * row;

          Debug.Assert(Numeric.IsZero(terrainTile.OriginX % terrainTile.CellSize), "The tile origin must be an integer multiple of the cell size.");
          Debug.Assert(Numeric.IsZero(terrainTile.OriginZ % terrainTile.CellSize), "The tile origin must be an integer multiple of the cell size.");

          // Extract the height values from the texture.
          float[] heights = TerrainHelper.GetTextureLevelSingle(inputHeightTexture, 0);

          // Transform height values from [0, 1] to [_minHeight, _maxheight].
          TerrainHelper.TransformTexture(heights, (_maxHeight - _minHeight), _minHeight);

          // Optional: Smooth the processed height map. (Very useful to remove artifacts from 8-bit
          // height maps.)
          TerrainHelper.SmoothTexture(heights, inputHeightTexture.Width, inputHeightTexture.Height, _smoothness);

          // We have to create a height and normal texture for each tile.
          // Reuse existing textures to avoid unnecessary allocation. (XNA can run out of memory
          // if we allocate to many textures in a short time.)
          Texture2D heightTexture = terrainTile.HeightTexture;
          Texture2D normalTexture = terrainTile.NormalTexture;

          // Convert the height value array into a suitable height texture.
          TerrainHelper.CreateHeightTexture(
            _graphicsService.GraphicsDevice,
            heights,
            inputHeightTexture.Width,
            inputHeightTexture.Height,
            _useNearestNeighborMipmaps,
            ref heightTexture);

          // Create a normal texture from the height values.
          TerrainHelper.CreateNormalTexture(
            _graphicsService.GraphicsDevice,
            heights,
            inputHeightTexture.Width,
            inputHeightTexture.Height,
            terrainTile.CellSize,
            _useNearestNeighborMipmaps,
            ref normalTexture);

          // If terrainTile.HeightTexture/NormalTexture was null, then CreateHeight/CreateNormal
          // has created a new texture. We have to dispose this textures in Dispose().
          terrainTile.HeightTexture = heightTexture;
          terrainTile.NormalTexture = normalTexture;

          // Set the height field data for collision detection.
          var heightField = (HeightField)tile.RigidBody.Shape;
          heightField.OriginX = terrainTile.OriginX;
          heightField.OriginZ = terrainTile.OriginZ;
          heightField.WidthX = tileSize;
          heightField.WidthZ = tileSize;
          lock (lockObject)
          {
            heightField.SetSamples(heights, heightTexture.Width, heightTexture.Height);
            heightField.Invalidate();
          }
        });
      });

      // Inform the terrain that the old texture content is invalid.
      TerrainNode.Terrain.Invalidate();
    }


    // Initialize the terrain layers which define the detail textures which are painted onto
    // the terrain.
    private void InitializeTerrainLayers(ContentManager content)
    {
      // The appearance of each terrain tile can be specified using layers. Each layer usually
      // specifies one material type, e.g. grass, rock, snow. Layers can also be used to add
      // decals or roads to the terrain, which is demonstrated in other samples.

      for (int row = 0; row < 2; row++)
      {
        for (int column = 0; column < 2; column++)
        {
          var terrainTile = _tiles[row, column].TerrainTile;
          string tilePostfix = "-" + row + "-" + column; // e.g. "-0-1"

          // This first layer contains only a base tint texture which is visible where no other
          // detail material layers are rendered (e.g. in the distance).
          var tintTexture = content.Load<Texture2D>("Terrain/Terrain001-Tint" + tilePostfix);
          var baseColorLayer = new TerrainMaterialLayer(_graphicsService)
          {
            TintTexture = tintTexture,
            TintStrength = 1,
          };
          terrainTile.Layers.Add(baseColorLayer);

          // The tiling of the detail textures can be visible and unattractive in the distance.
          // To avoid this, we can fade-out the detail material layers.
          int fadeOutStart = 4;   // The fade-out starts in clipmap level 4.
          int fadeOutEnd = 6;     // The fade-out ends in clipmap level 6, which means the layer is
                                  // not drawn into the detail clipmap levels >= 6.

          // Add a gravel texture which covers the whole terrain near the camera and fades
          // out in the distance.
          var materialGravel = new TerrainMaterialLayer(_graphicsService)
          {
            // The tiling detail material textures.
            DiffuseTexture = content.Load<Texture2D>("Terrain/Gravel-Diffuse"),
            NormalTexture = content.Load<Texture2D>("Terrain/Gravel-Normal"),
            SpecularTexture = content.Load<Texture2D>("Terrain/Gravel-Specular"),

            // The size of one tile in world space units.
            TileSize = DetailCellSize * 512,

            // The diffuse detail texture is multiplied with the tint texture.
            TintTexture = tintTexture,
            TintStrength = 1.0f,

            // The diffuse color is set to 1 / average texture color. This turns the average
            // detail texture color and conserve the color of the tint texture.
            // (To determine the average color manually: Load the texture in a image-processing
            // tool, like GIMP. Blur the texture until it is one solid color. Pick the color using
            // a color picker tool. If the image-processing tool uses sRGB, then convert the color
            // to linear RGB: colorLinear = colorSRgb^2.2)
            DiffuseColor = new Vector3F(1 / 0.246f, 1 / 0.205f, 1 / 0.171f),

            SpecularColor = new Vector3F(0.5f),
            SpecularPower = 20,

            FadeOutStart = fadeOutStart,
            FadeOutEnd = fadeOutEnd,
          };
          terrainTile.Layers.Add(materialGravel);

          // Over the gravel we add a layer of grass.
          // A blend texture contains a mask which defines where the grass is visible.
          // The blend range determines how quickly the width of the transition between grass
          // and the underlying layers.
          float blendRange = 0.6f;
          var materialGrass = new TerrainMaterialLayer(_graphicsService)
          {
            DiffuseTexture = content.Load<Texture2D>("Terrain/Grass-Dry-Diffuse"),
            NormalTexture = content.Load<Texture2D>("Terrain/Grass-Dry-Normal"),
            SpecularTexture = content.Load<Texture2D>("Terrain/Grass-Dry-Specular"),
            TileSize = DetailCellSize * 1024,
            TintTexture = tintTexture,
            TintStrength = 1,
            DiffuseColor = new Vector3F(1 / 0.25f),
            SpecularColor = new Vector3F(1),
            SpecularPower = 20,

            BlendTexture = content.Load<Texture2D>("Terrain/Terrain001-Blend-Grass" + tilePostfix),
            // The blend texture can contain a blend mask in each channel (R, G, B or A).
            // Use the red channel (channel 0)
            BlendTextureChannel = 0,
            BlendRange = blendRange,

            FadeOutStart = fadeOutStart,
            FadeOutEnd = fadeOutEnd,
          };
          terrainTile.Layers.Add(materialGrass);

          // Side note: Terrain layers can also be defined using .drmat files:
          //terrainTile.Layers.Add(new TerrainMaterialLayer(content.Load<Material>("Terrain/MyLayerMaterial")));

          // Let's add a layer of sand. Sand occurs only in the tiles 1/0 and 1/1.
          if (row == 1)
          {
            var materialSand = new TerrainMaterialLayer(_graphicsService)
            {
              DiffuseTexture = content.Load<Texture2D>("Terrain/Sand-Diffuse"),
              NormalTexture = content.Load<Texture2D>("Terrain/Sand-Normal"),
              SpecularTexture = content.Load<Texture2D>("Terrain/Sand-Specular"),
              TileSize = DetailCellSize * 512,
              TintTexture = tintTexture,
              TintStrength = 1f,
              DiffuseColor = new Vector3F(1 / 0.429f, 1 / 0.347f, 1 / 0.275f),
              SpecularColor = new Vector3F(10),
              SpecularPower = 50,
              BlendTexture = content.Load<Texture2D>("Terrain/Terrain001-Blend-Sand" + tilePostfix),
              BlendTextureChannel = 0,
              BlendRange = blendRange,
              FadeOutStart = fadeOutStart,
              FadeOutEnd = fadeOutEnd,
            };
            terrainTile.Layers.Add(materialSand);
          }

          // The dirt layer is used on the brown rocks near the beach. To make the rocks more
          // interesting we use a rock texture with a larger tile size. This is also visible
          // in the distance.
          // Then we also mix-in the gravel texture to add more detail near the camera.
          var materialDirt = new TerrainMaterialLayer(_graphicsService)
          {
            DiffuseTexture = content.Load<Texture2D>("Terrain/Rock-02-Diffuse"),
            NormalTexture = content.Load<Texture2D>("Terrain/Rock-02-Normal"),
            SpecularTexture = content.Load<Texture2D>("Terrain/Rock-02-Specular"),
            HeightTexture = content.Load<Texture2D>("Terrain/Rock-02-Height"),
            TileSize = DetailCellSize * 1024 * 10,
            TintTexture = tintTexture,
            TintStrength = 1f,
            DiffuseColor = new Vector3F(1 / 0.702f) * new Vector3F(0.9f, 1, 0.9f),
            SpecularColor = new Vector3F(2),
            SpecularPower = 100,
            BlendTexture = content.Load<Texture2D>("Terrain/Terrain001-Blend-Dirt" + tilePostfix),
            BlendTextureChannel = 0,
            BlendRange = blendRange,
          };
          terrainTile.Layers.Add(materialDirt);
          var materialDirtDetail = new TerrainMaterialLayer(_graphicsService)
          {
            DiffuseTexture = content.Load<Texture2D>("Terrain/Gravel-Diffuse"),
            NormalTexture = content.Load<Texture2D>("Terrain/Gravel-Normal"),
            SpecularTexture = content.Load<Texture2D>("Terrain/Gravel-Specular"),
            TileSize = DetailCellSize * 512,
            TintTexture = tintTexture,
            TintStrength = 1f,
            DiffuseColor = new Vector3F(1 / 0.59f, 1 / 0.537f, 1 / 0.5f),
            SpecularColor = new Vector3F(0.5f),
            SpecularPower = 20,

            // This layer is transparent to blend with the underlying rock texture.
            Alpha = 0.5f,

            BlendTexture = content.Load<Texture2D>("Terrain/Terrain001-Blend-Dirt" + tilePostfix),
            BlendTextureChannel = 0,
            BlendRange = blendRange,
            FadeOutStart = fadeOutStart,
            FadeOutEnd = fadeOutEnd,
          };
          terrainTile.Layers.Add(materialDirtDetail);

          // The gray rocks use two layered rock textures.
          var materialRock = new TerrainMaterialLayer(_graphicsService)
          {
            DiffuseTexture = content.Load<Texture2D>("Terrain/Rock-02-Diffuse"),
            NormalTexture = content.Load<Texture2D>("Terrain/Rock-02-Normal"),
            SpecularTexture = content.Load<Texture2D>("Terrain/Rock-02-Specular"),
            HeightTexture = content.Load<Texture2D>("Terrain/Rock-02-Height"),
            TileSize = DetailCellSize * 1024 * 20,
            TintTexture = tintTexture,
            TintStrength = 1f,
            DiffuseColor = new Vector3F(1 / 0.702f) * 1,
            SpecularColor = new Vector3F(2),
            SpecularPower = 100,
            BlendTexture = content.Load<Texture2D>("Terrain/Terrain001-Blend-Rock" + tilePostfix),
            BlendTextureChannel = 0,
            BlendRange = blendRange,
          };
          terrainTile.Layers.Add(materialRock);
          var materialRockDetail = new TerrainMaterialLayer(_graphicsService)
          {
            DiffuseTexture = content.Load<Texture2D>("Terrain/Rock-02-Diffuse"),
            NormalTexture = content.Load<Texture2D>("Terrain/Rock-02-Normal"),
            SpecularTexture = content.Load<Texture2D>("Terrain/Rock-02-Specular"),
            HeightTexture = content.Load<Texture2D>("Terrain/Rock-02-Height"),
            TileSize = DetailCellSize * 1024,
            TintTexture = tintTexture,
            TintStrength = 1f,
            DiffuseColor = new Vector3F(1 / 0.702f) * 1,
            SpecularColor = new Vector3F(2),
            SpecularPower = 100,
            Alpha = 0.5f,
            BlendTexture = content.Load<Texture2D>("Terrain/Terrain001-Blend-Rock" + tilePostfix),
            BlendTextureChannel = 0,
            BlendRange = blendRange,
            FadeOutStart = fadeOutStart,
            FadeOutEnd = fadeOutEnd,
          };
          terrainTile.Layers.Add(materialRockDetail);

          // Add some gravel from hydraulic erosion over the rocks.
          var materialFlow = new TerrainMaterialLayer(_graphicsService)
          {
            DiffuseTexture = content.Load<Texture2D>("Terrain/Gravel-Diffuse"),
            NormalTexture = content.Load<Texture2D>("Terrain/Gravel-Normal"),
            SpecularTexture = content.Load<Texture2D>("Terrain/Gravel-Specular"),
            TileSize = DetailCellSize * 512,
            TintTexture = tintTexture,
            TintStrength = 1f,
            DiffuseColor = new Vector3F(1 / 0.246f, 1 / 0.205f, 1 / 0.171f),
            SpecularColor = new Vector3F(1),
            BlendRange = blendRange,
            BlendTexture = content.Load<Texture2D>("Terrain/Terrain001-Blend-Flow" + tilePostfix),
            BlendTextureChannel = 0,
            SpecularPower = 20,
            FadeOutStart = fadeOutStart,
            FadeOutEnd = fadeOutEnd,
          };
          terrainTile.Layers.Add(materialFlow);

          // Add snow in the tiles 0/0 and 0/1.
          if (row == 0)
          {
            var materialSnow = new TerrainMaterialLayer(_graphicsService)
            {
              DiffuseTexture = content.Load<Texture2D>("Terrain/Snow-Diffuse"),
              NormalTexture = content.Load<Texture2D>("Terrain/Snow-Normal"),
              SpecularTexture = content.Load<Texture2D>("Terrain/Snow-Specular"),
              TileSize = DetailCellSize * 512,
              TintTexture = tintTexture,
              TintStrength = 0.0f,
              DiffuseColor = new Vector3F(1),
              SpecularColor = new Vector3F(10),
              SpecularPower = 100,
              BlendTexture = content.Load<Texture2D>("Terrain/Terrain001-Blend-Snow" + tilePostfix),
              BlendTextureChannel = 0,
              BlendRange = 0.5f,
              BlendThreshold = 0.4f,
              BlendNoiseInfluence = 0.5f,
              NoiseTileSize = 20,
            };
            terrainTile.Layers.Add(materialSnow);
          }
        }
      }
    }


    // Initialize the cell sizes of the clipmaps.
    private void InitializeClipmapCellSizes()
    {
      // The terrain is rendered using clipmaps.
      // The height/normal/hole info of the terrain is rendered into the base clipmap.
      // The cell sizes of the base clipmap determines the cell size of the mesh which is
      // used to render the terrain.
      // The cell size of base clipmap level 0 is usually equal to the tile cell size
      // (e.g. 1 sample per world space unit).
      // The cell sizes of the clipmap levels >= 1 are computed automatically.
      TerrainNode.BaseClipmap.CellSizes[0] = _tiles[0, 0].TerrainTile.CellSize;

      // The tiling detail textures are splatted into the detail clipmap.
      // The cell size of the first detail clipmap level determines the material resolution near
      // the camera. Typical values are 0.005 world space units per texel. Some of AAA games use
      // values up to 0.001 world space units per texel for ultra quality settings.
      TerrainNode.DetailClipmap.CellSizes[0] = DetailCellSize;

      // Per default CellSizes[1] - CellSizes[N] are computed automatically. Each level uses
      // twice the resolution. If CellSizes[0] is 0.005 and if we use 9 clipmap levels, the cell
      // sizes are: 0.005, 0.01, 0.02, 0.04, 0.08, 0.16, 0.32, 0.64, 1.28
      // --> If each clipmap level uses a resolution of 1024, then the last level covers a an
      // area of approx. 1310 x 1310 units. In this sample we want to use a much larger camera
      // far distance. Therefore, we have to compute larger cell sizes.

      // The longest distance that is visible from the camera frustum is along one of the frustum
      // edges. Assuming a symmetric frustum, we compute the length from the camera to the far
      // plane along the frustum edge.
      var projection = _cameraObject.CameraNode.Camera.Projection;
      Vector3F nearCorner = new Vector3F(projection.Left, projection.Bottom, projection.Near);
      float maxViewDistance = (nearCorner / projection.Near * projection.Far).Length;

      // The terrain distance must cover 2 * maxViewDistance (because the clipmap is centered on
      // the camera.
      float maxTerrainSize = 2 * maxViewDistance;

      // It is not necessary to make the clipmap bigger than the actual terrain.
      Vector3F terrainExtent = TerrainNode.Terrain.Aabb.Extent;
      maxTerrainSize = Math.Min(maxTerrainSize, Math.Max(terrainExtent.X, terrainExtent.Z));

      // Get the number of cells per detail clipmap level.
      // Reserve a border for anisotropic filtering in a texture atlas (max 8 pixels on each side)
      // + 1 because the clipmap snaps to cell sizes.
      int cellsPerLevelWithoutFilterBorder = TerrainNode.DetailClipmap.CellsPerLevel - (8 + 8 + 1);

      // Per default the cell sizes are computed as:
      //    CellSizes[x] = CellSizes[0] * 2^x
      // To cover a larger area we compute a new base b such that
      //    CellSizes[x] = CellSizes[0] * b^x
      int numberOfLevels = TerrainNode.DetailClipmap.NumberOfLevels;
      float b = (float)Math.Pow(maxTerrainSize / (cellsPerLevelWithoutFilterBorder * DetailCellSize), 1.0f / (numberOfLevels - 1.0f));

      float[] cellSizes = new float[numberOfLevels];
      for (int i = 1; i < numberOfLevels; i++)
        TerrainNode.DetailClipmap.CellSizes[i] = DetailCellSize * (float)Math.Pow(b, i);

      // If there is only one clipmap level, it has to cover the hole terrain.
      if (numberOfLevels == 1)
        cellSizes[0] = maxTerrainSize / cellsPerLevelWithoutFilterBorder;

      // We have to inform the clipmap when we change the content of the cell sizes arrays.
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

      foreach (var tile in _tiles)
      {
        // We have to dispose the textures which were not loaded via the content manager.
        tile.TerrainTile.HeightTexture.SafeDispose();
        tile.TerrainTile.HeightTexture = null;
        tile.TerrainTile.NormalTexture.SafeDispose();
        tile.TerrainTile.NormalTexture = null;
        
        tile.RigidBody.Simulation.RigidBodies.Remove(tile.RigidBody);

        tile.TerrainTile = null;
        tile.RigidBody = null;
      }
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
      if (_previousCameraFar != cameraFar || _updateGeometryTexture || _updateDetailClipmapCellSizes)
        InitializeClipmapCellSizes();

      _previousCameraFar = cameraFar;
      _updateDetailClipmapCellSizes = false;
      _updateGeometryTexture = false;

      // Visualize clipmaps for debugging.
      if (_showClipmaps)
      {
        for (int i = 0; i < 2; i++)
        {
          var clipmap = (i == 0) ? TerrainNode.BaseClipmap : TerrainNode.DetailClipmap;
          int height = 180;
          for (int j = 0; j < clipmap.Textures.Length; j++)
          {
            var texture = clipmap.Textures[j];
            var aspectRatio = texture.Width / (float)texture.Height;
            _graphicsScreen.DebugRenderer.DrawTexture(texture, new Rectangle(500 * i, j * height, (int)(height * aspectRatio), height));
          }
        }
      }
    }


    // Add GUI controls to the Options window.
    private void CreateGuiControls()
    {
      var sampleFramework = _services.GetInstance<SampleFramework>();
      var optionsPanel = sampleFramework.AddOptions("Game Objects");
      var panel = SampleHelper.AddGroupBox(optionsPanel, "TerrainObject");

      SampleHelper.AddCheckBox(
        panel,
        "Draw wireframe",
        _graphicsScreen.TerrainRenderer.DrawWireFrame,
        isChecked => _graphicsScreen.TerrainRenderer.DrawWireFrame = isChecked);

      SampleHelper.AddCheckBox(
        panel,
        "Show clipmaps",
        _showClipmaps,
        isChecked => _showClipmaps = isChecked);

      SampleHelper.AddSlider(
        panel,
        "Min height",
        null,
        -500,
        0,
        _minHeight,
        value =>
        {
          _minHeight = value;
          _updateGeometryTexture = true;
        });

      SampleHelper.AddSlider(
        panel,
        "Max height",
        null,
        0,
        2000,
        _maxHeight,
        value =>
        {
          _maxHeight = value;
          _updateGeometryTexture = true;
        });

      SampleHelper.AddCheckBox(
        panel,
        "Smooth height map",
        _smoothness > 0,
        isChecked =>
        {
          _smoothness = isChecked ? 1e6f : 0;
          _updateGeometryTexture = true;
        });

      SampleHelper.AddSlider(
        panel,
        "Height map cell size",
        null,
        0.5f,
        4f,
        TerrainNode.Terrain.Tiles[0].CellSize,
        value =>
        {
          foreach (var tile in TerrainNode.Terrain.Tiles)
            tile.CellSize = value;

          _updateGeometryTexture = true;
        });

      SampleHelper.AddCheckBox(
        panel,
        "Nearest neighbor mipmaps",
        _useNearestNeighborMipmaps,
        isChecked =>
        {
          _useNearestNeighborMipmaps = isChecked;
          _updateGeometryTexture = true;
        });

      var texelsPerLevelList = new List<int> { 64, 128, 256 };
      SampleHelper.AddDropDown(
        panel,
        "Cells per level",
        texelsPerLevelList,
        texelsPerLevelList.IndexOf(TerrainNode.BaseClipmap.CellsPerLevel),
        value => TerrainNode.BaseClipmap.CellsPerLevel = value);

      SampleHelper.AddSlider(
        panel,
        "Number of levels",
        "F0",
        1,
        6,
        TerrainNode.BaseClipmap.NumberOfLevels,
        value => TerrainNode.BaseClipmap.NumberOfLevels = (int)value);

#if !MONOGAME
      var detailTexelsPerLevelList = new List<int> { 512, 1024, 1365 };
#else
      var detailTexelsPerLevelList = new List<int> { 512, 1024, 1365, 2048 };
#endif
      SampleHelper.AddDropDown(
        panel,
        "Detail clipmap cells per level",
        detailTexelsPerLevelList,
        detailTexelsPerLevelList.IndexOf(TerrainNode.DetailClipmap.CellsPerLevel),
        value =>
        {
          TerrainNode.DetailClipmap.CellsPerLevel = value;
          _updateDetailClipmapCellSizes = true;
        });

      SampleHelper.AddSlider(
        panel,
        "Number of detail clipmap levels",
        "F0",
        1,
        9,
        TerrainNode.DetailClipmap.NumberOfLevels,
        value =>
        {
          TerrainNode.DetailClipmap.NumberOfLevels = (int)value;
          _updateDetailClipmapCellSizes = true;
        });

      SampleHelper.AddSlider(
        panel,
        "Mipmap bias",
        null,
        -10,
        10,
        TerrainNode.DetailClipmap.LevelBias,
        value => TerrainNode.DetailClipmap.LevelBias = value);

      SampleHelper.AddCheckBox(
       panel,
       "Enable mipmap",
       TerrainNode.DetailClipmap.EnableMipMap,
       isChecked => TerrainNode.DetailClipmap.EnableMipMap = isChecked);

      SampleHelper.AddCheckBox(
        panel,
        "Enable anisotropic filtering",
        TerrainNode.DetailClipmap.EnableAnisotropicFiltering,
        isChecked => TerrainNode.DetailClipmap.EnableAnisotropicFiltering = isChecked);
    }
    #endregion
  }
}
#endif
