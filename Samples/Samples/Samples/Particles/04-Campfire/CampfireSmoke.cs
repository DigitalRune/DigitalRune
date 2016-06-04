using DigitalRune.Graphics;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  // Creates a smoke effect for a campfire.
  public static class CampfireSmoke
  {
    public static ParticleSystem CreateCampfireSmoke(ContentManager contentManager)
    {
      ParticleSystem ps = new ParticleSystem
      {
        Name = "CampfireSmoke",
        MaxNumberOfParticles = 50,
      };

      // Each particle lives for a random time span.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Lifetime);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Lifetime,
        Distribution = new UniformDistributionF(2.0f, 2.4f),
      });

      // Add an effector that emits particles at a constant rate.
      ps.Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 15,
      });

      // Particle positions start on a circular area (in the xy-plane).
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        Distribution = new CircleDistribution { OuterRadius = 0.4f, InnerRadius = 0 }
      });

      // Particles move in forward direction with a slight random deviation with a random speed.
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new DirectionDistribution { Deviation = 0.15f, Direction = Vector3F.Forward },
      });
      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(0, 1),
      });

      // The LinearVelocityEffector uses the Direction and LinearSpeed to update the Position
      // of particles.
      ps.Effectors.Add(new LinearVelocityEffector());

      // Lets apply a damping (= exponential decay) to the LinearSpeed using the SingleDampingEffector.
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Damping).DefaultValue = 1.0f;
      ps.Effectors.Add(new SingleDampingEffector
      {
        // Following parameters are equal to the default values. No need to set them.
        //ValueParameter = ParticleParameterNames.LinearSpeed,
        //DampingParameter = ParticleParameterNames.Damping,
      });

      // To create a wind effect, we apply an acceleration to all particles.
      ps.Parameters.AddUniform<Vector3F>("Wind").DefaultValue = new Vector3F(-1, 3, -0.5f);
      ps.Effectors.Add(new LinearAccelerationEffector { AccelerationParameter = "Wind" });

      // Each particle starts with a random rotation angle and a random angular speed.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Angle);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Angle,
        Distribution = new UniformDistributionF(-ConstantsF.PiOver2, ConstantsF.PiOver2),
      });
      ps.Parameters.AddVarying<float>(ParticleParameterNames.AngularSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AngularSpeed,
        Distribution = new UniformDistributionF(-2f, 2f),
      });

      // The AngularVelocityEffector uses the AngularSpeed to update the particle Angle.
      ps.Effectors.Add(new AngularVelocityEffector
      {
        AngleParameter = ParticleParameterNames.Angle,
        SpeedParameter = ParticleParameterNames.AngularSpeed,
      });

      // Each particle gets a random start and end size.
      ps.Parameters.AddVarying<float>("StartSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "StartSize",
        Distribution = new UniformDistributionF(0.5f, 0.7f),
      });
      ps.Parameters.AddVarying<float>("EndSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "EndSize",
        Distribution = new UniformDistributionF(1.0f, 1.4f),
      });

      // The Size is computed from linear interpolation between the StartSize and the EndSize.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Effectors.Add(new SingleLerpEffector
      {
        ValueParameter = ParticleParameterNames.Size,
        FactorParameter = ParticleParameterNames.NormalizedAge,
        StartParameter = "StartSize",
        EndParameter = "EndSize",
      });

      // The Color slowly changes linearly from light gray to a darker gray.
      ps.Parameters.AddUniform<Vector3F>("StartColor").DefaultValue = new Vector3F(0.8f, 0.8f, 0.8f);
      ps.Parameters.AddUniform<Vector3F>("EndColor").DefaultValue = new Vector3F(0.3f, 0.3f, 0.3f);
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Color);
      ps.Effectors.Add(new Vector3FLerpEffector
      {
        ValueParameter = ParticleParameterNames.Color,
        StartParameter = "StartColor",
        EndParameter = "EndColor",
      });

      // The Alpha value is 0 for a short time, then it fades in to the TargetAlpha and finally
      // it fades out again.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 0.33f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        TimeParameter = ParticleParameterNames.NormalizedAge,
        FadeInStart = 0.36f,
        FadeInEnd = 0.6f,
        FadeOutStart = 0.6f,
        FadeOutEnd = 1.0f,
      });

      // DigitalRune Graphics supports "texture atlases": The class PackedTexture 
      // describes a single texture or tile set packed into a texture atlas. The 
      // smoke texture in this example consists of 2 tiles.
      ps.Parameters.AddUniform<PackedTexture>(ParticleParameterNames.Texture).DefaultValue =
        new PackedTexture(
          "Smoke2",
          contentManager.Load<Texture2D>("Campfire/Smoke2"),
          Vector2F.Zero, Vector2F.One,
          2, 1);

      // The particle parameter "AnimationTime" determines which tile is used,
      // where 0 = first tile, 1 = last tile.
      // --> Chooses a random tile for each particle when it is created.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.AnimationTime);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AnimationTime,
        Distribution = new UniformDistributionF(0, 1),
      });

      // Smoke needs alpha blending.
      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 1;

      ParticleSystemValidator.Validate(ps);

      return ps;
    }
  }
}