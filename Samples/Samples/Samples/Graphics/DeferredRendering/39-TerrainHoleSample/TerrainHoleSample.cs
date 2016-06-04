#if !WP7 && !WP8 && !XBOX
using System.Linq;
using DigitalRune;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to create holes in the terrain.",
    @"Holes in the terrain are useful for cave entrances.
The predefined terrain effects support two types of holes:
- Vertex holes are created by culling whole triangles. This method is very fast but the hole shape
  depends on the current tessellation pattern. The holes will disappear in the distance when the 
  tessellation is too coarse to represent the hole.
  To use this type of hole, it is best to make the hole a bit bigger than the cave entrance and
  to surround the hole with other 3D meshes (e.g. rocks) to hide the hole geometry.
  If the hole can be viewed from the distance, is best to hide it behind an impostor object (e.g. a
  billboard with a black spot) which is only turned on in the distance.
- Pixel holes use the texkill/clip shader operation to create holes. This creates perfect holes but
  is more costly because the pixel shader has to execute on the hole triangles and using texkill in
  the shader usually disables early-z optimization on the GPU.

The collision detection will always cull whole triangles if the HeightField shape is used. The
collision detection resolution is never 'per-pixel'.",
    139)]
  public class TerrainHoleSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly TerrainObject _terrainObject;
    private int _holeSize = 1;
    private Texture2D _holeTexture;


    public TerrainHoleSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);
      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

      Services.Register(typeof(DebugRenderer), null, _graphicsScreen.DebugRenderer);

      var scene = _graphicsScreen.Scene;
      Services.Register(typeof(IScene), null, scene);

      // Add standard game objects.
      var cameraGameObject = new CameraObject(Services, 5000);
      cameraGameObject.ResetPose(new Vector3F(0, 2, 0), 0, 0);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());
      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new DynamicSkyObject(Services, true, false, true)
      {
        EnableCloudShadows = false,
      });
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));

      // Create terrain.
      _terrainObject = new TerrainObject(Services);
      GameObjectService.Objects.Add(_terrainObject);

      UpdateHoleTexture();

      CreateGuiControls();
    }


    private void UpdateTerrainMaterial(bool enablePerPixelHoles)
    {
      Effect shadowMapEffect, gBufferEffect, materialEffect;
      if (!enablePerPixelHoles)
      {
        shadowMapEffect = ContentManager.Load<Effect>("DigitalRune/Terrain/TerrainShadowMap");
        gBufferEffect = ContentManager.Load<Effect>("DigitalRune/Terrain/TerrainGBuffer");
        materialEffect = ContentManager.Load<Effect>("DigitalRune/Terrain/TerrainMaterial");
      }
      else
      {
        shadowMapEffect = ContentManager.Load<Effect>("DigitalRune/Terrain/TerrainShadowMapHoles");
        gBufferEffect = ContentManager.Load<Effect>("DigitalRune/Terrain/TerrainGBufferHoles");
        materialEffect = ContentManager.Load<Effect>("DigitalRune/Terrain/TerrainMaterialHoles");
      }

      _terrainObject.TerrainNode.Material = new Material
      {
        { "ShadowMap", new EffectBinding(GraphicsService, shadowMapEffect, null, EffectParameterHint.Material) },
        { "GBuffer", new EffectBinding(GraphicsService, gBufferEffect, null, EffectParameterHint.Material) },
        { "Material", new EffectBinding(GraphicsService, materialEffect, null, EffectParameterHint.Material) }
      };
    }


    private void UpdateHoleTexture()
    {
      // Hole information could be loaded from a texture. Here, we procedurally create a pattern of 
      // holes. 
      // The hole buffer is array with 1 where there is no hole and 0 where there is a hole.
      // (This is similar to an alpha mask: 1 = opaque, 0 = transparent.)
      int numberOfSamples = _terrainObject.TerrainNode.Terrain.Tiles.First().HeightTexture.Width;
      float[] holes = new float[numberOfSamples * numberOfSamples];

      // Fill hole buffer with 1.
      for (int i = 0; i < holes.Length; i++)
        holes[i] = 1;

      if (_holeSize > 0)
      {
        // Add some 0 elements to create holes.
        int counterY = _holeSize;
        for (int y = 0; y < numberOfSamples - 1; y++)
        {
          int counterX = _holeSize;
          for (int x = 0; x < numberOfSamples - 1; x++)
          {
            holes[y * numberOfSamples + x] = 0;

            counterX--;
            if (counterX <= 0)
            {
              counterX = _holeSize;
              x += _holeSize * 2;
            }
          }

          counterY--;
          if (counterY <= 0)
          {
            counterY = _holeSize;
            y += _holeSize * 2;
          }
        }
      }

      // Copy hole buffer to a texture.
      TerrainHelper.CreateHoleTexture(
        GraphicsService.GraphicsDevice,
        holes,
        numberOfSamples,
        numberOfSamples,
        false,
        ref _holeTexture);

      foreach (var tile in _terrainObject.TerrainNode.Terrain.Tiles)
      {
        // Assign the hole texture to all terrain tiles. (Normally, each tile would have a
        // different hole texture or no hole texture at all.)
        tile.HoleTexture = _holeTexture;

        // We also have to add the holes to the collision detection height fields.
        // Get rigid body that represents this tile.
        var rigidBody = Simulation.RigidBodies.First(body => body.UserData == tile);
        var heightField = (HeightField)rigidBody.Shape;

        // Update the height values of the collision detection height field.
        float[] heights = TerrainHelper.GetTextureLevelSingle(tile.HeightTexture, 0);
        for (int z = 0; z < numberOfSamples; z++)
        {
          for (int x = 0; x < numberOfSamples; x++)
          {
            if ((!(holes[z * numberOfSamples + x] > 0.5f)))
            {
              // The HeightField class treats NaN as holes.
              heights[z * numberOfSamples + x] = float.NaN;
            }
          }
        }
        heightField.SetSamples(heights, numberOfSamples, numberOfSamples);
        heightField.Invalidate();
      }

      _terrainObject.TerrainNode.Terrain.Invalidate();
    }


    private void CreateGuiControls()
    {
      var panel = SampleFramework.AddOptions("Terrain");

      SampleHelper.AddCheckBox(
        panel,
        "Enable per-pixel holes",
        false,
        isChecked => UpdateTerrainMaterial(isChecked));

      SampleHelper.AddSlider(
        panel,
        "Hole size",
        "F0",
        0,
        8,
        _holeSize,
        value =>
        {
          _holeSize = (int)value;
          UpdateHoleTexture();
        });

      SampleHelper.AddSlider(
        panel,
        "Hole threshold",
        null,
        0,
        1,
        _terrainObject.TerrainNode.HoleThreshold,
        value => _terrainObject.TerrainNode.HoleThreshold = value);

      SampleFramework.ShowOptionsWindow("Terrain");
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // The hole texture was not loaded with the ContentManager, therefore we have to
        // dispose of it manually.
        _holeTexture.SafeDispose();
        _holeTexture = null;
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
