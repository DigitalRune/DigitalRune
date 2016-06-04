using System;
using System.Collections.Generic;
using DigitalRune;
using DigitalRune.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    "This sample shows how to use a particle effect with resource pooling.",
    "Teleport effects are triggered periodically at random locations.",
    10)]
  public class TeleportSample : ParticleSample
  {
    // A resource pool of teleport effects.
    private readonly ResourcePool<Teleport> _pool = new ResourcePool<Teleport>(
      () => new Teleport(),
      null,
      null);

    private readonly BoxDistribution _boxDistribution;
    private TimeSpan _waitTime;

    // List of active teleport effects.
    private readonly List<Teleport> _teleportEffects = new List<Teleport>();


    public TeleportSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _boxDistribution = new BoxDistribution
      {
        MinValue = new Vector3F(-5, 0, -5),
        MaxValue = new Vector3F(5, 0, 5),
      };
    }


    public override void Update(GameTime gameTime)
    {
      _waitTime -= gameTime.ElapsedGameTime;

      if (_waitTime < TimeSpan.Zero)
      {
        // Time to start the next effect at a random position.
        var position = _boxDistribution.Next(RandomHelper.Random);

        // Create teleport effect (the effect comes from a resource pool).
        var teleport = _pool.Obtain();
        teleport.Initialize(ContentManager);
        teleport.Pose = new Pose(position);

        // Add the teleport effect to the particle system service and the scene.
        ParticleSystemService.ParticleSystems.Add(teleport.ParticleSystem);
        GraphicsScreen.Scene.Children.Add(teleport.ParticleSystemNode);
        _teleportEffects.Add(teleport);

        _waitTime = TimeSpan.FromSeconds(1);
      }

      // Update teleport effects and recycle them if they are dead.
      for (int i = _teleportEffects.Count - 1; i >= 0; i--)
      {
        var teleport = _teleportEffects[i];
        bool isAlive = teleport.Update(GraphicsService);
        if (!isAlive)
        {
          ParticleSystemService.ParticleSystems.Remove(teleport.ParticleSystem);
          GraphicsScreen.Scene.Children.Remove(teleport.ParticleSystemNode);
          _teleportEffects.RemoveAt(i);

          _pool.Recycle(teleport);
        }
      }

      Profiler.AddValue("ParticleCount", ParticleHelper.CountNumberOfParticles(ParticleSystemService.ParticleSystems));
    }
  }
}
