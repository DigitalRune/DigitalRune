using DigitalRune.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Particles;
using Microsoft.Xna.Framework;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    @"A simple rain effect is created using a particle effect centered at the camera using 
axial billboards for the raindrops.",
    "",
    7)]
  public class RainSample : ParticleSample
  {
    private readonly ParticleSystem _particleSystem;
    private readonly ParticleSystemNode _particleSystemNode;


    public RainSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _particleSystem = Rain.Create(ContentManager);
      ParticleSystemService.ParticleSystems.Add(_particleSystem);

      _particleSystemNode = new ParticleSystemNode(_particleSystem);
      GraphicsScreen.Scene.Children.Add(_particleSystemNode);
    }


    public override void Update(GameTime gameTime)
    {
      // Move the particle system with the camera.
      _particleSystem.Pose = new Pose(GraphicsScreen.CameraNode.PoseWorld.Position);

      // Synchronize particles <-> graphics.
      _particleSystemNode.Synchronize(GraphicsService);

      Profiler.AddValue("ParticleCount", ParticleHelper.CountNumberOfParticles(ParticleSystemService.ParticleSystems));
    }
  }
}
