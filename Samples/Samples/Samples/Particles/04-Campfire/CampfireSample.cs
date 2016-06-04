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
    @"This sample shows how to create a campfire effect that uses different particle textures
from texture atlases.",
    @"",
    4)]
  public class CampfireSample : ParticleSample
  {
    private readonly ParticleSystemNode _particleSystemNode;


    public CampfireSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var particleSystem = Campfire.CreateCampfire(ContentManager);

      // Add a smoke effect as a child to the campfire.
      particleSystem.Children = new ParticleSystemCollection();
      particleSystem.Children.Add(CampfireSmoke.CreateCampfireSmoke(ContentManager));

      // Position the campfire (including its child) in the level.
      // (The fire effect lies in the xy plane and shoots into the forward direction (= -z axis).
      // Therefore, we rotate the particle system to shoot upwards.)
      particleSystem.Pose = new Pose(new Vector3F(0, 0.2f, 0), Matrix33F.CreateRotationX(ConstantsF.PiOver2));

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
