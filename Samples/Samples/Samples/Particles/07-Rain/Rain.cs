using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  // A simple rain effect.
  public static class Rain
  {
    public static ParticleSystem Create(ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "Rain",
        MaxNumberOfParticles = 2000,
      };

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 0.8f;

      ps.Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 1200,
      });

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        Distribution = new BoxDistribution { MinValue = new Vector3F(-20, 15, -20), MaxValue = new Vector3F(20, 15, 20) }
      });

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new DirectionDistribution { Deviation = 0f, Direction = -Vector3F.UnitY },
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(20, 30),
      });

      ps.Effectors.Add(new LinearVelocityEffector());

      ps.Parameters.AddUniform<float>(ParticleParameterNames.SizeX).DefaultValue = 0.03f;
      ps.Parameters.AddVarying<float>(ParticleParameterNames.SizeY);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.SizeY,
        Distribution = new UniformDistributionF(0.5f, 1.5f),
      });

      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Color).DefaultValue = new Vector3F(0.5f, 0.7f, 0.9f);

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Alpha).DefaultValue = 0.5f;

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/RainDrop");

      // DigitalRune Graphics can render particles with different billboard orientations. 
      // The rain drops should use axial billboards in the up direction (a.k.a. cylindrical 
      // billboards).
      ps.Parameters.AddUniform<BillboardOrientation>(ParticleParameterNames.BillboardOrientation).DefaultValue = BillboardOrientation.AxialViewPlaneAligned;

      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0;

      ParticleSystemValidator.Validate(ps);

      return ps;
    }
  }
}