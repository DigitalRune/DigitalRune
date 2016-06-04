using DigitalRune.Diagnostics;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    "This sample shows how to create a bouncing particles using a custom effector.",
    "Particles get stretched in motion direction.",
    9)]
  public class BounceSample : ParticleSample
  {
    private readonly ParticleSystemNode _particleSystemNode;


    public BounceSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var particleSystem = BouncingSparks.Create(ContentManager);
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
