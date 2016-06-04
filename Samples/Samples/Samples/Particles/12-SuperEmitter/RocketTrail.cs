using System;
using DigitalRune;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  // A rocket smoke trail that supports resource pooling.
  // The particle parameter "EmitterVelocity" can be set to modify initial movement of the 
  // explosion particles.
  public class RocketTrail : ParticleSystem
  {
    private static readonly ResourcePool<ParticleSystem> Pool = new ResourcePool<ParticleSystem>(
      () => new RocketTrail(ServiceLocator.Current.GetInstance<ContentManager>()),
      null,
      null);


    public static ParticleSystem Obtain()
    {
      return Pool.Obtain();
    }


    private RocketTrail(ContentManager contentManager)
    {
      MaxNumberOfParticles = 200;

      Parameters.AddVarying<float>(ParticleParameterNames.Lifetime);
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Lifetime,
        Distribution = new UniformDistributionF(1, 2),
      });

      Parameters.AddUniform<float>(ParticleParameterNames.EmissionRate);
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.EmissionRate,
        DefaultValue = 60,
      });
      Effectors.Add(new StreamEmitter
      {
        EmissionRateParameter = ParticleParameterNames.EmissionRate,
      });

      Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
      });

      Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction);
      Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new DirectionDistribution { Deviation = ConstantsF.Pi, Direction = Vector3F.UnitY },
      });

      Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(0.1f, 0.3f),
      });

      // The StartVelocityBiasEffector adds a velocity to new particles. We use this to add
      // the rocket velocity (stored in the parameter "EmitterVelocity") to the start velocities
      // of the particles.
      Parameters.AddUniform<Vector3F>(ParticleParameterNames.EmitterVelocity);
      Effectors.Add(new StartVelocityBiasEffector { Strength = 0.1f });

      Effectors.Add(new LinearVelocityEffector());

      Parameters.AddVarying<float>(ParticleParameterNames.Angle);
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Angle,
        Distribution = new UniformDistributionF(-ConstantsF.Pi, ConstantsF.Pi),
      });

      Parameters.AddVarying<float>(ParticleParameterNames.AngularSpeed);
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AngularSpeed,
        Distribution = new UniformDistributionF(-1, 1),
      });
      Effectors.Add(new AngularVelocityEffector());

      Parameters.AddVarying<float>("StartSize");
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "StartSize",
        Distribution = new UniformDistributionF(0.2f, 0.5f),
      });

      Parameters.AddVarying<float>("EndSize");
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "EndSize",
        Distribution = new UniformDistributionF(0.5f, 1f),
      });

      Parameters.AddVarying<float>(ParticleParameterNames.Size);
      Effectors.Add(new SingleLerpEffector
      {
        ValueParameter = ParticleParameterNames.Size,
        StartParameter = "StartSize",
        EndParameter = "EndSize",
      });

      Parameters.AddVarying<Vector3F>(ParticleParameterNames.Color);
      Effectors.Add(new StartValueEffector<Vector3F>
      {
        Parameter = ParticleParameterNames.Color,
        Distribution = new LineSegmentDistribution { Start = new Vector3F(0.5f, 0.4f, 0.25f), End = new Vector3F(0.7f, 0.6f, 0.5f) },
      });

      Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      Effectors.Add(new FuncEffector<float, float>
      {
        InputParameter = ParticleParameterNames.NormalizedAge,
        OutputParameter = ParticleParameterNames.Alpha,
        Func = age => 6.7f * age * (1 - age) * (1 - age),
      });


      Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/Smoke");

      // Draw behind explosion.
      Parameters.AddUniform<int>(ParticleParameterNames.DrawOrder).DefaultValue = -100;

      // The ParticleSystemRecycler recycles this instance into the specified resource 
      // pool when all particles are dead.
      Effectors.Add(new ParticleSystemRecycler
      {
        ResourcePool = Pool,

        // Set a minimum life-time to avoid that the particle system is recycled too early.
        // (The rocket trail might need a few frames before particles are created.)
        MinRuntime = TimeSpan.FromSeconds(0.05f),
      });

      ParticleSystemValidator.Validate(this);
    }
  }
}