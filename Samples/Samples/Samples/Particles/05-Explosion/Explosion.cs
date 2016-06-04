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
  // This class inherits from class ParticleSystem and creates an explosion effect.
  public class Explosion : ParticleSystem
  {
    public Explosion(ContentManager contentManager)
    {
      // The explosion particle systems owns 3 child particle systems. 
      // (The parent particle system does not have any particles.)
      Children = new ParticleSystemCollection
      {
        CreateFlash(contentManager),
        CreateHotCore(contentManager),
        CreateSmoke(contentManager),
      };

      ParticleSystemValidator.Validate(Children[0]);
      ParticleSystemValidator.Validate(Children[1]);
      ParticleSystemValidator.Validate(Children[2]);
    }


    // Emit a few particles.
    public void Explode()
    {
      Children[0].AddParticles(1);
      Children[1].AddParticles(8);
      Children[2].AddParticles(10);
    }


    // Creates a particle system that display a single particle: a bright billboard 
    // for a "flash" effect.
    private ParticleSystem CreateFlash(ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "Flash",
        MaxNumberOfParticles = 1,
        ReferenceFrame = ParticleReferenceFrame.World,

        // Optimization tip: Use same random number generator as parent.
        Random = Random,
      };

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 0.3f;

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        DefaultValue = Vector3F.Zero,
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Parameters.AddUniform<float>("StartSize").DefaultValue = 0.0f;
      ps.Parameters.AddUniform<float>("EndSize").DefaultValue = 40.0f;
      ps.Effectors.Add(new SingleLerpEffector
      {
        ValueParameter = ParticleParameterNames.Size,
        FactorParameter = ParticleParameterNames.NormalizedAge,
        StartParameter = "StartSize",
        EndParameter = "EndSize",
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 0.8f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        TimeParameter = ParticleParameterNames.NormalizedAge,
        FadeInStart = 0.0f,
        FadeInEnd = 0.2f,
        FadeOutStart = 0.75f,
        FadeOutEnd = 1.0f,
      });

      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Color).DefaultValue =
        new Vector3F(1, 1, 216.0f / 255.0f);

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/Flash");

      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0;

      return ps;
    }


    // Creates a hot red glowing core particle system for an explosion effect.
    private ParticleSystem CreateHotCore(ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "HotCore",
        MaxNumberOfParticles = 10,

        // Optimization tip: Use same random number generator as parent.
        Random = Random,
      };

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 1.0f;

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        DefaultValue = Vector3F.Zero,
      });

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new SphereDistribution { InnerRadius = 1.0f, OuterRadius = 1.0f },
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(5, 10),
      });

      ps.Effectors.Add(new LinearVelocityEffector
      {
        PositionParameter = ParticleParameterNames.Position,
        DirectionParameter = ParticleParameterNames.Direction,
        SpeedParameter = ParticleParameterNames.LinearSpeed,
      });

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Damping).DefaultValue = 6.0f;
      ps.Effectors.Add(new SingleDampingEffector
      {
        ValueParameter = ParticleParameterNames.LinearSpeed,
        DampingParameter = ParticleParameterNames.Damping,
      });

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
        Distribution = new UniformDistributionF(-1f, 1f),
      });

      ps.Effectors.Add(new AngularVelocityEffector
      {
        AngleParameter = ParticleParameterNames.Angle,
        SpeedParameter = ParticleParameterNames.AngularSpeed,
      });

      ps.Parameters.AddVarying<float>("StartSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "StartSize",
        Distribution = new UniformDistributionF(1f, 2.0f),
      });
      ps.Parameters.AddVarying<float>("EndSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "EndSize",
        Distribution = new UniformDistributionF(4f, 8.0f),
      });
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Effectors.Add(new SingleLerpEffector
      {
        ValueParameter = ParticleParameterNames.Size,
        FactorParameter = ParticleParameterNames.NormalizedAge,
        StartParameter = "StartSize",
        EndParameter = "EndSize",
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 0.55f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        TimeParameter = ParticleParameterNames.NormalizedAge,
        FadeInStart = 0.0f,
        FadeInEnd = 0.0f,
        FadeOutStart = 0.5f,
        FadeOutEnd = 1.0f,
      });

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/Explosion2");

      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0;

      return ps;
    }


    private ParticleSystem CreateSmoke(ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "Smoke",
        MaxNumberOfParticles = 20,

        // Optimization tip: Use same random number generator as parent.
        Random = Random,
      };

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 4.0f;

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        DefaultValue = Vector3F.Zero,
      });

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new SphereDistribution { InnerRadius = 1.0f, OuterRadius = 1.0f },
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(5, 10),
      });

      ps.Effectors.Add(new LinearVelocityEffector
      {
        PositionParameter = ParticleParameterNames.Position,
        DirectionParameter = ParticleParameterNames.Direction,
        SpeedParameter = ParticleParameterNames.LinearSpeed,
      });

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Damping).DefaultValue = 2.0f;
      ps.Effectors.Add(new SingleDampingEffector
      {
        ValueParameter = ParticleParameterNames.LinearSpeed,
        DampingParameter = ParticleParameterNames.Damping,

      });

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
        Distribution = new UniformDistributionF(-0.4f, 0.4f),
      });

      ps.Effectors.Add(new AngularVelocityEffector
      {
        AngleParameter = ParticleParameterNames.Angle,
        SpeedParameter = ParticleParameterNames.AngularSpeed,
      });

      ps.Parameters.AddVarying<float>("StartSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "StartSize",
        Distribution = new UniformDistributionF(2f, 4.0f),
      });
      ps.Parameters.AddVarying<float>("EndSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "EndSize",
        Distribution = new UniformDistributionF(6f, 16.0f),
      });
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Effectors.Add(new SingleLerpEffector
      {
        ValueParameter = ParticleParameterNames.Size,
        FactorParameter = ParticleParameterNames.NormalizedAge,
        StartParameter = "StartSize",
        EndParameter = "EndSize",
      });

      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Color).DefaultValue =
        new Vector3F(88.0f / 255.0f, 88.0f / 255.0f, 88.0f / 255.0f);

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 1.0f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        TimeParameter = ParticleParameterNames.NormalizedAge,
        FadeInStart = 0.0f,
        FadeInEnd = 0.35f,
        FadeOutStart = 0.5f,
        FadeOutEnd = 1.0f,
      });

      // DigitalRune Graphics supports "texture atlases": The class PackedTexture 
      // describes a single texture or tile set packed into a texture atlas. The 
      // clouds texture in this example consists of 2 tiles.
      ps.Parameters.AddUniform<PackedTexture>(ParticleParameterNames.Texture).DefaultValue =
        new PackedTexture(
          "Clouds",
          contentManager.Load<Texture2D>("Particles/Clouds"),
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

      // The ParticleBatch should render smoke particle back-to-front.
      ps.Parameters.AddUniform<bool>(ParticleParameterNames.IsDepthSorted).DefaultValue = true;

      return ps;
    }
  }
}