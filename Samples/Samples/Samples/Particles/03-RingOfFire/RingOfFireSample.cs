using System.Linq;
using DigitalRune.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    @"This sample shows how to create a particle system consisting of two child particle systems.
The effect is a ring of fire consisting of a fire and a smoke effect.",
    @"",
    3)]
  public class RingOfFireSample : ParticleSample
  {
    private readonly ParticleSystemNode _particleSystemNode;


    public RingOfFireSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Create a new "empty" particle system.
      var particleSystem = new ParticleSystem();

      // Particle systems can have child particle systems. 
      // Add a fire and a smoke effect as children.
      var fire = Fire.Create(ContentManager);
      var smoke = Smoke.Create(ContentManager);  // The smoke effect from the previous sample.
      particleSystem.Children = new ParticleSystemCollection { fire, smoke };

      // If we need to, we can modify the predefined effects.
      // Change the smoke particle lifetime.
      smoke.Parameters.Get<float>(ParticleParameterNames.Lifetime).DefaultValue = 4;
      // Change the smoke's start positions to a ring.
      smoke.Effectors.OfType<StartPositionEffector>().First().Distribution =
        new CircleDistribution { InnerRadius = 2, OuterRadius = 2 };

      // Position the particle system (including its child) in the level.
      particleSystem.Pose = new Pose(new Vector3F(0, 3, 0));

      // We only need to add the parent particle system to the particle system service.
      // The service will automatically update the parent system each frame. The parent
      // system will automatically update its children.
      ParticleSystemService.ParticleSystems.Add(particleSystem);

      // Add the particle system to the scene graph.
      _particleSystemNode = new ParticleSystemNode(particleSystem);
      GraphicsScreen.Scene.Children.Add(_particleSystemNode);
    }


    public override void Update(GameTime gameTime)
    {
      // Synchronize particles <-> graphics.
      _particleSystemNode.Synchronize(GraphicsService);

      Profiler.AddValue("ParticleCount", ParticleHelper.CountNumberOfParticles(ParticleSystemService.ParticleSystems));
    }
  }
}
