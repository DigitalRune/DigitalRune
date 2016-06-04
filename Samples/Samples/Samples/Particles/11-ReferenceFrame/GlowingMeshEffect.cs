using DigitalRune.Geometry.Meshes;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  public static class GlowingMeshEffect
  {
    public static ParticleSystem Create(ITriangleMesh mesh, ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "GlowingMeshEffect",
        MaxNumberOfParticles = 100
      };

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 1.0f;

      ps.Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 100,
      });

      // The particles start on random positions on the surface of the given triangle mesh.
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartOnMeshEffector
      {
        Parameter = ParticleParameterNames.Position,
        Mesh = mesh
      });

      // Just to demonstrate a new custom effector:
      // The size follows a user-defined curve using the FuncEffector.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Effectors.Add(new FuncEffector<float, float>
      {
        InputParameter = ParticleParameterNames.NormalizedAge,
        OutputParameter = ParticleParameterNames.Size,
        Func = age => 6.7f * age * (1 - age) * (1 - age) * 0.4f,
      });

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Color);
      ps.Effectors.Add(new StartValueEffector<Vector3F>
      {
        Parameter = ParticleParameterNames.Color,
        Distribution = new BoxDistribution
        {
          MinValue = new Vector3F(0.5f, 0.5f, 0.5f),
          MaxValue = new Vector3F(1, 1, 1)
        }
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Effectors.Add(new FuncEffector<float, float>
      {
        InputParameter = ParticleParameterNames.NormalizedAge,
        OutputParameter = ParticleParameterNames.Alpha,
        Func = age => 6.7f * age * (1 - age) * (1 - age),
      });

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/Star");

      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0;


      ps.Parameters.AddUniform<BillboardOrientation>(ParticleParameterNames.BillboardOrientation).DefaultValue =
        BillboardOrientation.ScreenAligned;

      ParticleSystemValidator.Validate(ps);

      return ps;
    }
  }
}
