using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  // A particle system that draws one ribbon by connecting all particles.
  public static class RibbonEffect
  {
    public static ParticleSystem Create(ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "Ribbon",
        MaxNumberOfParticles = 50,
      };

      // Ribbons are enabled by setting the "Type" to ParticleType.Ribbon. Consecutive 
      // living particles are connected and rendered as ribbons (quad strips). At least 
      // two living particles are required to create a ribbon. Dead particles 
      // ("NormalizedAge" ≥ 1) can be used as delimiters to terminate one ribbon and 
      // start the next ribbon. 
      ps.Parameters.AddUniform<ParticleType>(ParticleParameterNames.Type).DefaultValue =
        ParticleType.Ribbon;

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 1;

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector());

      // The parameter "Axis" determines the orientation of the ribbon. 
      // We could use a fixed orientation. It is also possible to "twist" the ribbon
      // by using a varying parameter.
      //ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Axis).DefaultValue =
      //  Vector3F.Up;

      ps.Effectors.Add(new RibbonEffector());
      ps.Effectors.Add(new ReserveParticleEffector { Reserve = 1 });

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Size).DefaultValue = 1;

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Color);
      ps.Effectors.Add(new StartValueEffector<Vector3F>
      {
        Parameter = ParticleParameterNames.Color,
        Distribution = new BoxDistribution { MinValue = new Vector3F(0.5f, 0.5f, 0.5f), MaxValue = new Vector3F(1, 1, 1) }
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Effectors.Add(new FuncEffector<float, float>
      {
        InputParameter = ParticleParameterNames.NormalizedAge,
        OutputParameter = ParticleParameterNames.Alpha,
        Func = age => 6.7f * age * (1 - age) * (1 - age),
      });

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/Ribbon");

      // The parameter "TextureTiling" defines how the texture spreads across the ribbon.
      // 0 ... no tiling, 
      // 1 ... repeat every particle, 
      // n ... repeat every n-th particle
      ps.Parameters.AddUniform<int>(ParticleParameterNames.TextureTiling).DefaultValue =
        1;

      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0;

      ParticleSystemValidator.Validate(ps);

      return ps;
    }
  }
}
