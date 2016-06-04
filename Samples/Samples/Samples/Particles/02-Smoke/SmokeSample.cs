using DigitalRune.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles;
using Microsoft.Xna.Framework;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    @"This sample shows how to create a basic smoke effect (similar to the smoke effect in the
App Hub XNA Particle Sample).",
    @"",
    2)]
  public class SmokeSample : ParticleSample
  {
    private readonly ParticleSystemNode _particleSystemNode0;
    private readonly ParticleSystemNode _particleSystemNode1;
    private readonly ParticleSystemNode _particleSystemNode2;


    public SmokeSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Create a single particle system and add it multiple times to the scene 
      // graph ("instancing"). By default, all instances look identical. The 
      // properties ParticleSystemNode.Color/Alpha/AngleOffset can be used to 
      // render the particles with some variations.
      var particleSystem = Smoke.Create(ContentManager);
      ParticleSystemService.ParticleSystems.Add(particleSystem);

      _particleSystemNode0 = new ParticleSystemNode(particleSystem);
      GraphicsScreen.Scene.Children.Add(_particleSystemNode0);

      _particleSystemNode1 = new ParticleSystemNode(particleSystem);
      _particleSystemNode1.PoseWorld = new Pose(new Vector3F(5, 0, -5));
      _particleSystemNode1.Color = new Vector3F(0.9f, 0.8f, 0.7f);
      _particleSystemNode1.Alpha = 0.8f;
      _particleSystemNode1.AngleOffset = 0.3f;
      GraphicsScreen.Scene.Children.Add(_particleSystemNode1);

      _particleSystemNode2 = new ParticleSystemNode(particleSystem);
      _particleSystemNode2.PoseWorld = new Pose(new Vector3F(-10, 5, -5), Matrix33F.CreateRotationZ(-ConstantsF.PiOver2));
      _particleSystemNode2.Color = new Vector3F(0.5f, 0.5f, 0.5f);
      _particleSystemNode2.AngleOffset = 0.6f;
      GraphicsScreen.Scene.Children.Add(_particleSystemNode2);
    }


    public override void Update(GameTime gameTime)
    {
      // Synchronize particles <-> graphics.
      _particleSystemNode0.Synchronize(GraphicsService);
      _particleSystemNode1.Synchronize(GraphicsService);
      _particleSystemNode2.Synchronize(GraphicsService);

      Profiler.AddValue("ParticleCount", ParticleHelper.CountNumberOfParticles(ParticleSystemService.ParticleSystems));
    }
  }
}
