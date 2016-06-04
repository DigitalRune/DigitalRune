using DigitalRune.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles;
using Microsoft.Xna.Framework;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    "This sample shows how to use animated textures to display a deadly swarm of bees.",
    "",
    17)]
  public class AnimatedTextureSample : ParticleSample
  {
    private readonly ParticleSystem _beeSwarm;
    private readonly ParticleSystemNode _particleSystemNode;


    public AnimatedTextureSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _beeSwarm = BeeSwarm.Create(ContentManager);
      _beeSwarm.Pose = new Pose(new Vector3F(0, 4, 0));

      // Create 100 bees.
      _beeSwarm.AddParticles(100);

      ParticleSystemService.ParticleSystems.Add(_beeSwarm);

      _particleSystemNode = new ParticleSystemNode(_beeSwarm);
      GraphicsScreen.Scene.Children.Add(_particleSystemNode);
    }


    public override void Update(GameTime gameTime)
    {
      // The bee swarm effect needs the CameraPose to determine the bee texture orientation.
      _beeSwarm.Parameters.Get<Pose>("CameraPose").DefaultValue = GraphicsScreen.CameraNode.PoseWorld;

      // Synchronize particles <-> graphics.
      _particleSystemNode.Synchronize(GraphicsService);

      Profiler.AddValue("ParticleCount", ParticleHelper.CountNumberOfParticles(ParticleSystemService.ParticleSystems));
    }
  }
}
