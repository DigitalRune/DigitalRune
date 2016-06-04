#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples shows how to render bodies of water.",
    @"Two bodies of water are created using WaterNodes. One WaterNode uses a box shape. It reflects
the skybox or a planar reflection. Optionally, the water flow is defined by a flow map. The camera
can also dive underwater.
The second WaterNode uses an inclined surface mesh. Using WaterFlow, the water automatically flows
in the right direction. This WaterNode does not have an underwater effect.
To render the WaterNodes, a WaterRenderer was added to the DeferredGraphicsScreen.
Alpha-blended objects are not always rendered correctly in this sample. In the DeferredGraphicsScreen
water and alpha-blended objects are sorted by depth and rendered in this order. It might be better
to change the graphics screen to render first all alpha-blended objects under the water surface, then
the water and then the alpha-blended objects above the water surface.",
    117)]
  [Controls(@"Sample
  Hold <J> to display debug info.
  Press <K> to switch between skybox reflection and planar reflection.
  Press <L> to enable/disable the water flow map.")]
  public class WaterSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly WaterNode _waterNode0;
    private readonly WaterFlow _waterFlow0;
    private readonly WaterNode _waterNode1;


    public WaterSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
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
      var dynamicSkyObject = new DynamicSkyObject(Services, true, false, true);
      GameObjectService.Objects.Add(dynamicSkyObject);

      // Add a ground plane with some detail to see the water refractions.
      Simulation.RigidBodies.Add(new RigidBody(new PlaneShape(new Vector3F(0, 1, 0), 0)));
      GameObjectService.Objects.Add(new StaticObject(Services, "Gravel/Gravel", 1, new Pose(new Vector3F(0, 0.001f, 0))));

      GameObjectService.Objects.Add(new DudeObject(Services));
      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));
      GameObjectService.Objects.Add(new DynamicObject(Services, 5));
      GameObjectService.Objects.Add(new DynamicObject(Services, 6));
      GameObjectService.Objects.Add(new DynamicObject(Services, 7));
      GameObjectService.Objects.Add(new FogObject(Services) { AttachToCamera = true });

      // The LavaBalls class controls all lava ball instances.
      var lavaBalls = new LavaBallsObject(Services);
      GameObjectService.Objects.Add(lavaBalls);

      // Add a few palm trees.
      var random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-3, -8), 0, random.NextFloat(0, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree", scale, new Pose(position, orientation)));
      }

      // Define the appearance of the water.
      var water = new Water
      {
        SpecularColor = new Vector3F(10f),

        // Small water ripples/waves are created using scrolling normal maps.
        NormalMap0 = ContentManager.Load<Texture2D>("Water/Wave0"),
        NormalMap1 = ContentManager.Load<Texture2D>("Water/Wave1"),
        NormalMap0Scale = 1.8f,
        NormalMap1Scale = 2.2f,
        NormalMap0Velocity = new Vector3F(-0.02f, 0, 0.03f),
        NormalMap1Velocity = new Vector3F(0.02f, 0, -0.03f),
        NormalMap0Strength = 0.5f,
        NormalMap1Strength = 0.5f,

        ReflectionDistortion = 0.2f,
        ReflectionColor = new Vector3F(0.7f),
        RefractionDistortion = 0.05f,
      };

      // Create a box-shaped body of water.
      // We use a TransformedShape containing a BoxShape because the top of the 
      // water body must be at height 0.
      var shape = new TransformedShape(new GeometricObject(
        new BoxShape(10, 1, 20),
        new Pose(new Vector3F(0, -0.5f, 0))));
      _waterNode0 = new WaterNode(water, shape)
      {
        PoseWorld = new Pose(new Vector3F(-1, 0.5f, 0), Matrix33F.CreateRotationY(0.1f)),
        SkyboxReflection = _graphicsScreen.Scene.GetDescendants().OfType<SkyboxNode>().First(),
        DepthBufferWriteEnable = true,
      };
      _graphicsScreen.Scene.Children.Add(_waterNode0);

      // Optional: Create a WaterFlow to move the water using a flow texture.
      _waterFlow0 = new WaterFlow
      {
        FlowMapSpeed = 0.5f,
        FlowMap = GenerateFlowMap(),
        CycleDuration = 3f,
        NoiseMapStrength = 0.1f,
        NoiseMapScale = 0.5f,
      };
      _waterNode0.Flow = _waterFlow0;

      // Optional: Use a planar reflection instead of the skybox reflection.
      // We add a PlanarReflectionNode as a child of the WaterNode.
      var renderToTexture = new RenderToTexture
      {
        Texture = new RenderTarget2D(GraphicsService.GraphicsDevice, 512, 512, false, SurfaceFormat.HdrBlendable, DepthFormat.None),
      };
      var planarReflectionNode = new PlanarReflectionNode(renderToTexture)
      {
        // Same shape as WaterNode.
        Shape = _waterNode0.Shape,

        // Reflection plane is horizontal.
        NormalLocal = new Vector3F(0, 1, 0),
      };
      _waterNode0.PlanarReflection = planarReflectionNode;
      _waterNode0.Children = new SceneNodeCollection(1) { planarReflectionNode };

      // Create a short river with an inclined water surface.
      // Using a WaterFlow with a SurfaceSlopeSpeed, the water automatically flows
      // down the inclined surface.
      _waterNode1 = new WaterNode(water, GetSpiralShape())
      {
        PoseWorld = new Pose(new Vector3F(10, 1.5f, 0), Matrix33F.CreateRotationY(0.1f)),
        EnableUnderwaterEffect = false,
        SkyboxReflection = _graphicsScreen.Scene.GetDescendants().OfType<SkyboxNode>().First(),
        Flow = new WaterFlow
        {
          SurfaceSlopeSpeed = 0.5f,
          CycleDuration = 2f,
          NoiseMapStrength = 0.1f,
          NoiseMapScale = 1,
        }
      };
      _graphicsScreen.Scene.Children.Add(_waterNode1);
    }


    public override void Update(GameTime gameTime)
    {
      var debugRenderer = _graphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Draw debug info.
      if (InputService.IsDown(Keys.J))
      {
        // Bounding shapes.
        debugRenderer.DrawShape(_waterNode0.Volume, _waterNode0.PoseWorld, _waterNode0.ScaleWorld, Color.Blue, true, false);
        debugRenderer.DrawShape(_waterNode1.Volume, _waterNode1.PoseWorld, _waterNode1.ScaleWorld, Color.Blue, true, false);

        // Flow map.
        if (_waterNode0.Flow != null)
          debugRenderer.DrawTexture(_waterNode0.Flow.FlowMap, new Rectangle(0, 0, 400, 400));
      }

      // Toggle reflection.
      if (InputService.IsPressed(Keys.K, true))
        _waterNode0.PlanarReflection.IsEnabled = !_waterNode0.PlanarReflection.IsEnabled;

      // Toggle water flow.
      if (InputService.IsPressed(Keys.L, true))
      {
        if (_waterNode0.Flow == _waterFlow0)
          _waterNode0.Flow = null;
        else
          _waterNode0.Flow = _waterFlow0;
      }
    }


    // Creates a 32x32 texture which defines the water flow (direction + speed).
    private Texture2D GenerateFlowMap()
    {
      const int size = 32;
      var data = new Color[size * size];
      for (int y = 0; y < size; y++)
      {
        for (int x = 0; x < size; x++)
        {
          Vector2F flowDirection;
          float flowSpeed;
          GetFlow(new Vector2F(x / (float)size, y / (float)size), out flowDirection, out flowSpeed);

          // Encode in color. The flow map stores the normalized 2D direction in r and g.
          // The speed (magnitude of the flow vector) is stored in b, where 0 represents
          // no movement and 1 represents movement with max speed.

          flowSpeed = MathHelper.Clamp(flowSpeed, 0, 1);

          // Convert to byte.
          data[y * size + x] = new Color(
            (byte)((flowDirection.X / 2 + 0.5f) * 255),
            (byte)((flowDirection.Y / 2 + 0.5f) * 255),
            (byte)(flowSpeed * 255));
        }
      }

      var texture = new Texture2D(GraphicsService.GraphicsDevice, size, size, true, SurfaceFormat.Color);
      texture.SetData(data);

      return texture;
    }


    // Returns the flow vector for a given position.
    // x and y are in the range [0, 1].
    private static void GetFlow(Vector2F position, out Vector2F direction, out float speed)
    {
      // Create a circular movement around (0.5, 0.5).

      // Vector from center to position is:
      var radius = position - new Vector2F(0.5f, 0.5f);

      // The flow direction is orthogonal to the radius vector.
      direction = new Vector2F(radius.Y, -radius.X);
      direction.TryNormalize();

      // The speed is max in the center and is 0 at the texture border.
      speed = 1;
      if (!Numeric.IsZero(radius.Length))
        speed = 1 - InterpolationHelper.HermiteSmoothStep(MathHelper.Clamp((radius.Length - 0.1f) / 0.4f, 0, 1));
    }


    // Creates a TriangleMeshShape for the water surface. The surface is curved and
    // inclined.
    private static Shape GetSpiralShape()
    {
      var triangleMesh = new TriangleMesh();

      const int numberOfQuads = 12;
      const float innerRadius = 1;
      const float outerRadius = 2;
      float angle = 0;
      float y = 0;
      for (int i = 0; i <= numberOfQuads; i++, angle += 0.4f, y -= 0.02f)
      {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);
        triangleMesh.Vertices.Add(new Vector3F(outerRadius * cos, y, outerRadius * sin));
        triangleMesh.Vertices.Add(new Vector3F(innerRadius * cos, y, innerRadius * sin));

        if (i == 5)
          y -= 0.2f;
      }

      // Add 2 triangles per quad.
      for (int i = 0; i < numberOfQuads; i++)
      {
        triangleMesh.Indices.Add(i * 2 + 0);
        triangleMesh.Indices.Add(i * 2 + 1);
        triangleMesh.Indices.Add(i * 2 + 2);

        triangleMesh.Indices.Add(i * 2 + 1);
        triangleMesh.Indices.Add(i * 2 + 3);
        triangleMesh.Indices.Add(i * 2 + 2);
      }

      return new TriangleMeshShape(triangleMesh);
    }
  }
}
#endif