using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  // Creates a smoke effect for an explosion.
  public class RocketExplosionSmoke : ParticleSystem
  {
    public RocketExplosionSmoke(ContentManager contentManager)
    {
      MaxNumberOfParticles = 200;

      Parameters.AddVarying<float>(ParticleParameterNames.Lifetime);
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Lifetime,
        Distribution = new UniformDistributionF(2, 4),
      });

      // 30 particles are created instantly, and then no more particles.
      Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 30 * 60,
        EmissionLimit = 30,
      });

      Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      Effectors.Add(new StartPositionEffector());

      Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction);
      Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new DirectionDistribution { Deviation = ConstantsF.TwoPi, Direction = Vector3F.UnitY },
      });

      Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(1, 6),
      });

      // The StartVelocityBiasEffector adds a velocity to new particles. We use this to add
      // the rocket velocity (stored in the parameter "EmitterVelocity") to the start velocities
      // of the particles.
      Effectors.Add(new StartVelocityBiasEffector
      {
        BiasVelocityParameter = ParticleParameterNames.EmitterVelocity,
        Strength = 1
      });
      
      Effectors.Add(new LinearVelocityEffector());

      Parameters.AddUniform<Vector3F>(ParticleParameterNames.LinearAcceleration).DefaultValue = new Vector3F(0, -2.0f, 0);
      Effectors.Add(new LinearAccelerationEffector());

      Parameters.AddUniform<float>(ParticleParameterNames.Damping).DefaultValue = 2.0f;
      Effectors.Add(new SingleDampingEffector());

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

      Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      Effectors.Add(new FuncEffector<float, float>
      {
        InputParameter = ParticleParameterNames.NormalizedAge,
        OutputParameter = ParticleParameterNames.Alpha,
        Func = age => 6.7f * age * (1 - age) * (1 - age),
      });

      Parameters.AddVarying<float>("StartSize");
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "StartSize",
        Distribution = new UniformDistributionF(1, 3),
      });

      Parameters.AddVarying<float>("EndSize");
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "EndSize",
        Distribution = new UniformDistributionF(5, 10),
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
        Distribution = new LineSegmentDistribution { Start = new Vector3F(0.8f, 0.8f, 0.8f), End = new Vector3F(1, 1, 1) },
      });

      Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/Smoke");
    }
  }
}