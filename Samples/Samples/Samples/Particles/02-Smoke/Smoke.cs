using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  // Creates a smoke effect.
  public static class Smoke
  {
    public static ParticleSystem Create(ContentManager contentManager)
    {
      ParticleSystem ps = new ParticleSystem
      {
        Name = "Smoke",
        MaxNumberOfParticles = 200,
      };

      // All particles should live for 5 seconds.
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 5;

      // Add an effector that emits particles at a constant rate.
      ps.Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 30,
      });

      // The reference frame can be either "Local" or "World" (Default).
      // - "Local" means that the particle positions, directions, velocities, etc.
      //   are relative to the ParticleSystemNode in the scene graph.
      // - "World" means that those values are given in world space. The position
      //   of the ParticleSystemNode in the scene graph does not affect the particles. 
      // (For more information check out sample "11-ReferenceFrame".)
      ps.ReferenceFrame = ParticleReferenceFrame.Local;

      // Particle positions start in the center of the particle system.
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        DefaultValue = Vector3F.Zero,
      });

      // Particles move in the up direction with a random deviation of 0.5 radians and a 
      // random speed.
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new DirectionDistribution { Deviation = 0.5f, Direction = Vector3F.Up },
      });
      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(0.5f, 1),
      });

      // The LinearVelocityEffector uses the Direction and LinearSpeed to update the Position
      // of particles.
      ps.Effectors.Add(new LinearVelocityEffector
      {
        // Following parameters are equal to the default values. No need to set them.
        //PositionParameter = ParticleParameterNames.Position,
        //DirectionParameter = ParticleParameterNames.Direction,
        //SpeedParameter = ParticleParameterNames.LinearSpeed,
      });

      // To create a wind effect, we apply an acceleration to all particles.
      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.LinearAcceleration).DefaultValue =
        new Vector3F(0.2f, -0.1f, 0);

      ps.Effectors.Add(new LinearAccelerationEffector
      {
        // Following parameters are equal to the default values. No need to set them.
        //AccelerationParameter = ParticleParameterNames.LinearAcceleration,
        //DirectionParameter = ParticleParameterNames.Direction,
        //SpeedParameter = ParticleParameterNames.LinearSpeed,        
      });

      // Each particle starts with a random rotation angle and a random angular speed.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Angle);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Angle,
        Distribution = new UniformDistributionF(-ConstantsF.Pi, ConstantsF.Pi),
      });
      ps.Parameters.AddVarying<float>(ParticleParameterNames.AngularSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AngularSpeed,
        Distribution = new UniformDistributionF(-2, 2),
      });
      // The AngularVelocityEffector uses the AngularSpeed to update the particle Angle.
      ps.Effectors.Add(new AngularVelocityEffector
      {
        // Following parameters are equal to the default values. No need to set them.
        //AngleParameter = ParticleParameterNames.Angle,
        //SpeedParameter = ParticleParameterNames.AngularSpeed,
      });

      ps.Parameters.AddVarying<float>("StartSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "StartSize",
        Distribution = new UniformDistributionF(0.1f, 0.5f),
      });

      ps.Parameters.AddVarying<float>("EndSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "EndSize",
        Distribution = new UniformDistributionF(2, 4),
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Effectors.Add(new SingleLerpEffector
      {
        ValueParameter = ParticleParameterNames.Size,
        StartParameter = "StartSize",
        EndParameter = "EndSize",
      });

      // Particle alpha fades in to a target value of 1 and then back out to 0.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 1f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        FadeInStart = 0f,
        FadeInEnd = 0.2f,
        FadeOutStart = 0.7f,
        FadeOutEnd = 1f,
      });

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/Smoke");

      ParticleSystemValidator.Validate(ps);

      return ps;
    }
  }
}