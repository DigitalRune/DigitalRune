using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  // A simple effect that draws bullet holes. Particles must be added externally.
  public static class Decals
  {
    public static ParticleSystem Create(ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "Decals",
        MaxNumberOfParticles = 50,
      };

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 5;
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Size).DefaultValue = 0.3f;

      // Following particle parameters are initialized externally:
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Parameters.AddVarying<Vector3F>("Normal");
      ps.Parameters.AddVarying<Vector3F>("Axis");

      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Color).DefaultValue = new Vector3F(0.667f, 0.667f, 0.667f);

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Effectors.Add(new SingleLinearSegment3Effector
      {
        OutputParameter = ParticleParameterNames.Alpha,
        Time0 = 0,
        Value0 = 1,
        Time1 = 0.9f,
        Value1 = 1,
        Time2 = 1,
        Value2 = 0,
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Angle);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Angle,
        Distribution = new UniformDistributionF(-0.5f, 0.5f),
      });

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/BulletHole");

      // Particle billboards use a custom billboard orientation:
      ps.Parameters.AddUniform<BillboardOrientation>(ParticleParameterNames.BillboardOrientation).DefaultValue =
        BillboardOrientation.WorldOriented;

      ps.Parameters.AddUniform<BlendState>(ParticleParameterNames.BlendState).DefaultValue = BlendState.AlphaBlend;

      // If the user places too many decals, then we run out of particles. If the
      // MaxNumberOfParticles limit is reached, no more decals can be placed. To 
      // avoid this we add the ReserveParticleEffector, which kills old particles 
      // if the MaxNumberOfParticles limit is reached.
      ps.Effectors.Add(new ReserveParticleEffector());

      ParticleSystemValidator.Validate(ps);

      return ps;
    }
  }
}