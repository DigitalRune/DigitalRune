using System;
using DigitalRune.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    @"This sample shows how to create an explosion effect.",
    @"An explosion effect is created by deriving from the ParticleSystem class.
The explosion is triggered periodically.",
    5)]
  public class ExplosionSample : ParticleSample
  {
    private static readonly TimeSpan ExplosionInterval = TimeSpan.FromSeconds(5);

    private readonly Explosion _explosion;
    private readonly ParticleSystemNode _particleSystemNode;
    private readonly SoundEffect _explosionSound;
    private TimeSpan _timeUntilExplosion = TimeSpan.Zero;


    public ExplosionSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Create an instance of the Explosion particle system.
      _explosion = new Explosion(ContentManager);
      _explosion.Pose = new Pose(new Vector3F(0, 5, 0));
      ParticleSystemService.ParticleSystems.Add(_explosion);

      _particleSystemNode = new ParticleSystemNode(_explosion);
      GraphicsScreen.Scene.Children.Add(_particleSystemNode);

      _explosionSound = ContentManager.Load<SoundEffect>("Particles/Explo1");
    }


    public override void Update(GameTime gameTime)
    {
      // If enough time has passed, trigger the explosion sound and the explosion effect.
      _timeUntilExplosion -= gameTime.ElapsedGameTime;
      if (_timeUntilExplosion <= TimeSpan.Zero)
      {
        _explosion.Explode();
        _explosionSound.Play(0.2f, 0, 0);
        _timeUntilExplosion = ExplosionInterval;
      }

      // Synchronize particles <-> graphics.
      _particleSystemNode.Synchronize(GraphicsService);

      Profiler.AddValue("ParticleCount", ParticleHelper.CountNumberOfParticles(ParticleSystemService.ParticleSystems));
    }
  }
}
