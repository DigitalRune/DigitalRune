#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to use light clipping to make a light affect only an certain area.",
    @"The light clip geometry is made up of two boxes. Clipping is applied to all initial light nodes.
(Light nodes that are created later are not clipped.)

Light clipping can be used, for example, to limit a light to the interior of a house. Only objects
inside the house are lit, other objects are not affected. This is a lot cheaper than using shadow maps!

Light Clipping and Forward Rendering:
A forward rendered/transparent mesh is lit when its origin is inside the clip geometry. The object is
either fully lit or not lit at all.
Further, for ambient and directional lights, the sort hint must be changed from Global to Local in
the HLSL effect. Otherwise, the clip geometry is ignored. For example:
  float3 AmbientLight : AMBIENTLIGHT < string Hint = ""Local""; >;",
    120)]
  public class LightClipSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly GeometricObject _clip;


    public LightClipSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);
      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

      Services.Register(typeof(DebugRenderer), null, _graphicsScreen.DebugRenderer);
      Services.Register(typeof(IScene), null, _graphicsScreen.Scene);

      // Add gravity and damping to the physics simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new DynamicSkyObject(Services, true, false, true));
      GameObjectService.Objects.Add(new GroundObject(Services));

      GameObjectService.Objects.Add(new DudeObject(Services));
      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));
      GameObjectService.Objects.Add(new DynamicObject(Services, 3));
      GameObjectService.Objects.Add(new DynamicObject(Services, 4));
      GameObjectService.Objects.Add(new DynamicObject(Services, 6));
      GameObjectService.Objects.Add(new DynamicObject(Services, 7));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new FogObject(Services));

      // The LavaBalls class controls all lava ball instances.
      var lavaBalls = new LavaBallsObject(Services);
      GameObjectService.Objects.Add(lavaBalls);

      // Create a lava ball instance.
      lavaBalls.Spawn();

      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-3, -8), 0, random.NextFloat(0, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree", scale, new Pose(position, orientation)));
      }

      var boxShape = new BoxShape(3, 3, 3);
      var compositeShape = new CompositeShape
      {
        Children =
        {
          new GeometricObject(boxShape, new Pose(new Vector3F(-2, 1.4f, 0))),
          new GeometricObject(boxShape, new Pose(new Vector3F(2, 1.4f, 0))),
        }
      };
      _clip = new GeometricObject(compositeShape, Pose.Identity);

      foreach (var lightNode in _graphicsScreen.Scene.GetDescendants().OfType<LightNode>())
      {
        lightNode.Clip = _clip;
        //lightNode.InvertClip = true;
      }
    }


    public override void Update(GameTime gameTime)
    {
      // This sample clears the debug renderer each frame.
      _graphicsScreen.DebugRenderer.Clear();

      _graphicsScreen.DebugRenderer.DrawObject(_clip, Color.Yellow, true, false);
    }
  }
}
#endif