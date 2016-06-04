#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples shows a simple fur effect.",
    @"The fur can be animated using the 'FurDisplacement' effect parameter. This effect
parameter is automatically set each frame using an effect parameter binding.",
    111)]
  public class FurSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;

    private readonly MeshNode _meshNode;
    private readonly RigidBody _rigidBody;

    // The total game time, used by ComputeFurDisplacement().
    // (Side note: This field is static (global variable) because the ComputeFurDisplacement 
    // delegate is created only once when the model is loaded. It is not recreated 
    // when this sample is restarted but the model is already loaded by the ContentManager. 
    // Therefore, it should not reference a field of a specific FurSample instance.)
    private static TimeSpan _time;


    public FurSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);

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

      // Tell the engine how to interpret effect parameters with the name or semantic "FurDisplacement".
      // We could add our own effect interpreter instance, as it is done in the CloudQuadSample.
      // Or, we add the parameter description to the standard DefaultEffectInterpreter instance:
      var defaultEffectInterpreter = GraphicsService.EffectInterpreters.OfType<DefaultEffectInterpreter>().First();

      // If the effect parameter name or semantic in the .fx file is called "FurDisplacement", then
      // return an EffectParameterDescription. The hint "PerInstance" tells the engine that this
      // parameter needs to be updated for each scene node.
      defaultEffectInterpreter.ParameterDescriptions.Add(
        "FurDisplacement",
        (parameter, i) => new EffectParameterDescription(parameter, "FurDisplacement", i, EffectParameterHint.PerInstance));

      // Tell the engine how to create an effect parameter binding for "FurDisplacement".
      // We could add our own effect binder instance, as it is done in the CloudQuadSample.
      // Or, we add the parameter description to the standard DefaultEffectBinder instance:
      var defaultEffectBinder = GraphicsService.EffectBinders.OfType<DefaultEffectBinder>().First();

      // If the effect parameter represents "FurDisplacement", then create a DelegateParameterBinding
      // for the parameter. When an object is rendered using this effect the method ComputeFurDisplacement
      // is called to update the value of the effect parameter.
      defaultEffectBinder.Vector3Bindings.Add(
        "FurDisplacement",
        (effect, parameter, data) => new DelegateParameterBinding<Vector3>(effect, parameter, ComputeFurDisplacement));

      // Load model. (When the model and its effects are loaded, the engine will
      // check for the FurDisplacement parameter and create the appropriate parameter binding.)
      _meshNode = (MeshNode)ContentManager.Load<ModelNode>("Fur/FurBall").Children[0].Clone();
      _rigidBody = new RigidBody(new SphereShape(0.5f));

      // Store a reference to the rigid body in SceneNode.UserData.
      _meshNode.UserData = _rigidBody;

      // Set a random pose.
      _rigidBody.Pose = new Pose(new Vector3F(0, 1, 0), RandomHelper.Random.NextQuaternionF());
      _meshNode.PoseWorld = _rigidBody.Pose;

      // Add rigid body to physics simulation and model to scene.
      Simulation.RigidBodies.Add(_rigidBody);
      _graphicsScreen.Scene.Children.Add(_meshNode);
    }


    // This method is called when the fur objects is rendered.
    private Vector3 ComputeFurDisplacement(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      // Animate the fur using a periodic animation.
      float p = (((float)_time.TotalSeconds) / 5.0f);
      float deltaX = (float)(Math.Pow(Math.Sin(p), 2) + Math.Cos(16 * p) + 1) * 0.001f;
      float deltaZ = (float)Math.Cos(2 * p) * 0.002f;
      Vector3F furDisplacement = new Vector3F(deltaX, -0.01f, deltaZ);

      // Add the current rigid body movement to the displacement.
      var meshNode = (MeshNode)context.SceneNode;
      var rigidBody = (RigidBody)meshNode.UserData;
      furDisplacement += -Vector3F.Clamp(rigidBody.LinearVelocity * 0.1f, -0.02f, 0.02f);

      return (Vector3)furDisplacement;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Clean up.
        var defaultEffectInterpreter = GraphicsService.EffectInterpreters.OfType<DefaultEffectInterpreter>().First();
        defaultEffectInterpreter.ParameterDescriptions.Remove("FurDisplacement");
        var defaultEffectBinder = GraphicsService.EffectBinders.OfType<DefaultEffectBinder>().First();
        defaultEffectBinder.Vector3Bindings.Remove("FurDisplacement");
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // Store the current time. (The value is is used in ComputeFurDisplacement.)
      _time = gameTime.TotalGameTime;

      // Update SceneNode.LastPoseWorld - this is required for some effects 
      // like object motion blur. 
      _meshNode.SetLastPose(true);

      // Synchronize graphics <--> physics.
      _meshNode.PoseWorld = _rigidBody.Pose;

      _graphicsScreen.DebugRenderer.Clear();
    }
  }
}
#endif