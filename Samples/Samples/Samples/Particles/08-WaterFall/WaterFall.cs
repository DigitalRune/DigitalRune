using System;
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
  // Waterfall effect consisting of spray particles and water particles.
  // The water particles use a special billboard orientation to follow the direction of the
  // water. The whole effect is preloaded to hide the start of the waterfall from the user.
  public static class WaterFall
  {
    public static ParticleSystem CreateWaterFall(ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "WaterFall",

        // Preload 2 seconds of the effect using a larger time step.
        PreloadDuration = TimeSpan.FromSeconds(2),
        PreloadDeltaTime = TimeSpan.FromSeconds(0.1),

        Children = new ParticleSystemCollection
        {
          CreateSpray(contentManager),
          CreateWater(contentManager),
        }
      };

      // This parent particle system defines the uniform Gravity parameter for the child
      // particle systems. Uniform particle parameters can be "inherited" - if a child
      // does not have a required uniform parameter, it uses the parameter of the parent.
      ps.Parameters.AddUniform<Vector3F>("Gravity").DefaultValue = new Vector3F(0, -1f, 0);

      ParticleSystemValidator.Validate(ps);
      ParticleSystemValidator.Validate(ps.Children[0]);
      ParticleSystemValidator.Validate(ps.Children[1]);

      return ps;
    }

    public static ParticleSystem CreateSpray(ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "Spray",
        MaxNumberOfParticles = 100
      };

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 2.5f;

      ps.Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 18,
      });

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        DefaultValue = new Vector3F(0.0f, 0.1f, 0.0f),
        Distribution = new LineSegmentDistribution { Start = new Vector3F(-0.5f, 0.05f, 0.0f), End = new Vector3F(0.5f, 0.05f, 0.0f) }
      });

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        DefaultValue = Vector3F.Forward,
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        DefaultValue = 1.5f,
      });

      ps.Effectors.Add(new LinearVelocityEffector());

      // "Gravity" is a uniform parameter inherited from the parent particle system.
      ps.Effectors.Add(new LinearAccelerationEffector { AccelerationParameter = "Gravity" });

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
        Distribution = new UniformDistributionF(-0.8f, 0.8f)
      });
      ps.Effectors.Add(new AngularVelocityEffector());

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Parameters.AddUniform<float>("StartSize").DefaultValue = 0.4f;
      ps.Parameters.AddUniform<float>("EndSize").DefaultValue = 1.4f;
      ps.Effectors.Add(new SingleLerpEffector
      {
        ValueParameter = ParticleParameterNames.Size,
        StartParameter = "StartSize",
        EndParameter = "EndSize",
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 0.5f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        FadeInStart = 0.0f,
        FadeInEnd = 0.2f,
        FadeOutStart = 0.95f,
        FadeOutEnd = 1.0f,
        TargetValueParameter = "TargetAlpha",
        ValueParameter = ParticleParameterNames.Alpha,
      });

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/Spray");

      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 1;

      // Spray should be drawn on top of water. "DrawOrder" is supported by class ParticleBatch.
      ps.Parameters.AddUniform<int>(ParticleParameterNames.DrawOrder).DefaultValue = 100;

      return ps;
    }


    public static ParticleSystem CreateWater(ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "Water",
        MaxNumberOfParticles = 50
      };

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 2.0f;

      ps.Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 5,
      });

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        DefaultValue = new Vector3F(0.0f, 0.1f, 0.0f),
        Distribution = new LineSegmentDistribution { Start = new Vector3F(-0.1f, 0.05f, 0.0f), End = new Vector3F(0.1f, 0.05f, 0.0f) }
      });

      // Render particles with a custom orientation. (The orientation of each particle
      // is defined by the parameters "Normal" and "Axis".)
      ps.Parameters.AddUniform<BillboardOrientation>(ParticleParameterNames.BillboardOrientation).DefaultValue =
        new BillboardOrientation(BillboardNormal.Custom, false, true);

      // The "Normal" parameter is the face normal of the particle billboard.
      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Normal).DefaultValue = new Vector3F(0, 1, 0);

      // The "Axis" parameter is the up-axis of the particle billboard. In this case
      // the "Axis" is the direction of the water flow.
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Axis);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Axis,
        DefaultValue = Vector3F.Forward,
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(1.5f, 1.5f),
      });

      ps.Effectors.Add(new LinearVelocityEffector
      {
        DirectionParameter = ParticleParameterNames.Axis
      });

      // "Gravity" is a uniform parameter inherited from the parent particle system.
      ps.Effectors.Add(new LinearAccelerationEffector
      {
        AccelerationParameter = "Gravity",
        DirectionParameter = ParticleParameterNames.Axis,
      });

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Angle).DefaultValue = ConstantsF.Pi;

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Effectors.Add(new SingleLinearSegment3Effector
      {
        OutputParameter = ParticleParameterNames.Size,
        Time0 = 0,
        Value0 = 0.6f,
        Time1 = 1,
        Value1 = 2,
        Time2 = 1,
        Value2 = 1.6f,
      });

      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Color).DefaultValue = new Vector3F(0.90f, 0.95f, 1.0f);

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 1.0f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        FadeInStart = 0.0f,
        FadeInEnd = 0.2f,
        FadeOutStart = 0.95f,
        FadeOutEnd = 1.0f,
        TargetValueParameter = "TargetAlpha",
        ValueParameter = ParticleParameterNames.Alpha,
      });

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/Water");

      return ps;
    }
  }
}