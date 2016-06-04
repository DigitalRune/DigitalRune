using DigitalRune;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;


namespace Samples.Particles
{
  // An explosion smoke effect that supports resource pooling.
  // The particle parameter "EmitterVelocity" can be set to modify initial movement of the 
  // explosion particles.
  public class RocketExplosion : ParticleSystem
  {
    private static readonly ResourcePool<ParticleSystem> Pool = new ResourcePool<ParticleSystem>(
      () => new RocketExplosion(ServiceLocator.Current.GetInstance<ContentManager>()),
      null,
      null);


    public static ParticleSystem Obtain()
    {
      return Pool.Obtain();
    }


    private RocketExplosion(ContentManager contentManager)
    {
      Children = new ParticleSystemCollection
      {
        new RocketExplosionSmoke(contentManager),
        new RocketExplosionCore(contentManager),
      };

      // This EmitterVelocity parameter can be used by all child particle systems.
      Parameters.AddUniform<Vector3F>(ParticleParameterNames.EmitterVelocity);

      // The ParticleSystemRecycler recycles this instance into the resource pool when all 
      // particles are dead.
      Effectors.Add(new ParticleSystemRecycler
      {
        ResourcePool = Pool,
      });

      ParticleSystemValidator.Validate(this);
      ParticleSystemValidator.Validate(Children[0]);
      ParticleSystemValidator.Validate(Children[1]);
    }
  }
}
