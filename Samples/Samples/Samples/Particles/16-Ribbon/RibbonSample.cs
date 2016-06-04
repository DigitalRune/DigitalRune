using DigitalRune.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    "This sample shows how to draw a ribbon using particles.",
    "",
    16)]
  public class RibbonSample : ParticleSample
  {
    private readonly RigidBody _rigidBody;
    private readonly ModelNode _modelNode;
    private readonly ParticleSystem _particleSystem;
    private readonly ParticleSystemNode _particleSystemNode;


    public RibbonSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.DrawReticle = true;

      GameObjectService.Objects.Add(new GrabObject(Services));

      // Load a sphere model.
      _modelNode = ContentManager.Load<ModelNode>("Particles/Sphere").Clone();
      GraphicsScreen.Scene.Children.Add(_modelNode);

      // Add gravity and damping to the physics simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Create a rigid body for the sphere.
      _rigidBody = new RigidBody(new SphereShape(0.5f))
      {
        Pose = new Pose(new Vector3F(-3, 0, 0)),
        LinearVelocity = new Vector3F(10, 10, -3f),
      };
      Simulation.RigidBodies.Add(_rigidBody);

      _particleSystem = RibbonEffect.Create(ContentManager);
      ParticleSystemService.ParticleSystems.Add(_particleSystem);
      _particleSystemNode = new ParticleSystemNode(_particleSystem);
      GraphicsScreen.Scene.Children.Add(_particleSystemNode);
    }


    public override void Update(GameTime gameTime)
    {
      // Move the model and the particle system with the rigid body.
      _modelNode.PoseWorld = _rigidBody.Pose;
      _particleSystem.Pose = _rigidBody.Pose;

      // Synchronize particles <-> graphics.
      _particleSystemNode.Synchronize(GraphicsService);

      Profiler.AddValue("ParticleCount", ParticleHelper.CountNumberOfParticles(ParticleSystemService.ParticleSystems));
    }
  }
}
