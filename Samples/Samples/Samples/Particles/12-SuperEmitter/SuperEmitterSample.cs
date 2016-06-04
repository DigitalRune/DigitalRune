using DigitalRune.Diagnostics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Particles;
using Microsoft.Xna.Framework;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    @"This sample shows how to create a ""super-emitter"".",
    @"A super-emitter is a particle system that spawns other particle systems.
The class Rockets is a particle system that simulates the paths of a few rockets. Each 
rocket particle creates and controls other nested particle systems (trail + explosion).",
    12)]
  public class SuperEmitterSample : ParticleSample
  {
    private readonly ParticleSystemNode _particleSystemNode;


    public SuperEmitterSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      ParticleSystem particleSystem = new Rockets();
      ParticleSystemService.ParticleSystems.Add(particleSystem);

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
