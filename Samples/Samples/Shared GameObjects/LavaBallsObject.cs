using System;
using System.Collections.Generic;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples
{
  // Spawns lava balls and controls all lava ball instances.
  public class LavaBallsObject : GameObject
  {
    private readonly IServiceLocator _services;
    private ModelNode _modelPrototype;
    private RigidBody _bodyPrototype;
    private PointLight _pointLight;
    private AnimatableProperty<float> _glowIntensity;
    private ConstParameterBinding<Vector3> _emissiveColorBinding;

    // The individual instances:
    private readonly List<ModelNode> _models = new List<ModelNode>();
    private readonly List<RigidBody> _bodies = new List<RigidBody>();


    public LavaBallsObject(IServiceLocator services)
    {
      _services = services;
      Name = "LavaBalls";
    }


    // OnLoad() is called when the GameObject is added to the IGameObjectService.
    protected override void OnLoad()
    {
      // ----- Create prototype of a lava ball:

      // Use a sphere for physics simulation.
      _bodyPrototype = new RigidBody(new SphereShape(0.5f));

      // Load the graphics model.
      var content = _services.GetInstance<ContentManager>();
      _modelPrototype = content.Load<ModelNode>("LavaBall/LavaBall").Clone();

      // Attach a point light to the model. The light projects the glowing lava 
      // veins (cube map texture) onto the environment.
      _pointLight = new PointLight
      {
        Color = new Vector3F(1, 1, 1),
        DiffuseIntensity = 2,
        SpecularIntensity = 2,
        Range = 1.5f,
        Attenuation = 0.5f,
        Texture = content.Load<TextureCube>("LavaBall/LavaCubeMap"),
      };
      var pointLightNode = new LightNode(_pointLight);
      _modelPrototype.Children.Add(pointLightNode);

      // Get the emissive color binding of the material because the emissive color
      // will be animated.
      // The model contains one mesh node with a single material.
      var meshNode = (MeshNode)_modelPrototype.Children[0];
      var mesh = meshNode.Mesh;
      var material = mesh.Materials[0];

      // The material contains several effect bindings. The "EmissiveColor" is applied
      // in the "Material" pass. 
      // (For reference see material definition file: Samples\Media\LavaBall\Lava.drmat)
      _emissiveColorBinding = (ConstParameterBinding<Vector3>)material["Material"].ParameterBindings["EmissiveColor"];

      // Use the animation service to animate glow intensity of the lava.
      var animationService = _services.GetInstance<IAnimationService>();

      // Create an AnimatableProperty<float>, which stores the animation value.
      _glowIntensity = new AnimatableProperty<float>();

      // Create sine animation and play the animation back-and-forth.
      var animation = new SingleFromToByAnimation
      {
        From = 0.3f,
        To = 3.0f,
        Duration = TimeSpan.FromSeconds(1),
        EasingFunction = new SineEase { Mode = EasingMode.EaseInOut },
      };
      var clip = new AnimationClip<float>
      {
        Animation = animation,
        Duration = TimeSpan.MaxValue,
        LoopBehavior = LoopBehavior.Oscillate
      };
      animationService.StartAnimation(clip, _glowIntensity).AutoRecycle();
    }


    public void Spawn()
    {
      var scene = _services.GetInstance<IScene>();
      var simulation = _services.GetInstance<Simulation>();

      // Create a new instance by cloning the prototype.
      var model = _modelPrototype.Clone();
      var body = _bodyPrototype.Clone();

      // Spawn at random position.
      var randomPosition = new Vector3F(
        RandomHelper.Random.NextFloat(-10, 10),
        RandomHelper.Random.NextFloat(2, 5),
        RandomHelper.Random.NextFloat(-20, 0));
      body.Pose = new Pose(randomPosition, RandomHelper.Random.NextQuaternionF());
      model.PoseWorld = _bodyPrototype.Pose;
      scene.Children.Add(model);
      simulation.RigidBodies.Add(body);

      _models.Add(model);
      _bodies.Add(body);
    }


    // OnUnload() is called when the GameObject is removed from the IGameObjectService.
    protected override void OnUnload()
    {
      // Remove models from scene.
      foreach (var model in _models)
      {
        model.Parent.Children.Remove(_modelPrototype);
        model.Dispose(false);
      }

      // Remove rigid bodies from physics simulation.
      foreach (var body in _bodies)
        body.Simulation.RigidBodies.Remove(body);

      _models.Clear();
      _bodies.Clear();

      // Remove prototype.
      _modelPrototype.Dispose(false);
      _modelPrototype = null;
      _bodyPrototype = null;

      // Stop animation.
      var animationService = _services.GetInstance<IAnimationService>();
      animationService.StopAnimation(_glowIntensity);
      _glowIntensity = null;
    }


    // OnUpdate() is called once per frame.
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Synchronize graphics <--> physics.
      for (int i = 0; i < _models.Count; i++)
      {
        var model = _models[i];
        var body = _bodies[i];

        // Update SceneNode.LastPoseWorld - this is required for some effects, 
        // like object motion blur. 
        model.SetLastPose(true);

        model.PoseWorld = body.Pose;
      }

      // Animate emissive color of material and point light intensity.
      _emissiveColorBinding.Value = new Vector3(_glowIntensity.Value);
      _pointLight.DiffuseIntensity = _glowIntensity.Value;
      _pointLight.SpecularIntensity = _glowIntensity.Value;
    }
  }
}
