using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  // Creates the hot glowing core of an explosion.
  public class RocketExplosionCore : ParticleSystem
  {
    public RocketExplosionCore(ContentManager contentManager)
    {
      MaxNumberOfParticles = 200;

      Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 1;

      Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 20 * 60,
        EmissionLimit = 20,
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
        Distribution = new UniformDistributionF(0.5f, 2),
      });

      // The StartVelocityBiasEffector adds a velocity to new particles. We use this to add
      // the rocket velocity (stored in the parameter "EmitterVelocity") to the start velocities
      // of the particles.
      Effectors.Add(new StartVelocityBiasEffector { Strength = 1 });

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

      Parameters.AddVarying<float>("TargetAlpha");
      Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "TargetAlpha",
        Distribution = new UniformDistributionF(0.3f, 0.5f),
      });

      Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        FadeInStart = 0f,
        FadeInEnd = 0.3f,
        FadeOutStart = 0.3f,
        FadeOutEnd = 1f,
      });

      Parameters.AddUniform<float>("StartSize").DefaultValue = 3;
      Parameters.AddUniform<float>("EndSize").DefaultValue = 5;
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
        Distribution = new LineSegmentDistribution { Start = new Vector3F(0.5f), End = new Vector3F(0.66f) },
      });

      Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/Explosion");

      Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0;

      // Draw on top of smoke.
      Parameters.AddUniform<int>(ParticleParameterNames.DrawOrder).DefaultValue = 100;
    }
  }
}