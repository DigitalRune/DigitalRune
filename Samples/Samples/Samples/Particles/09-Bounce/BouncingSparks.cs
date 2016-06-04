using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  // Creates a bouncing sparks effect. The custom CollisionPlaneEffector is used to make the
  // particles bounce. Particles are stretched in motion direction.
  public static class BouncingSparks
  {
    public static ParticleSystem Create(ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "BouncingSparks",
        MaxNumberOfParticles = 200,
      };

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 3;

      ps.Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 40,
      });

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        Distribution = new LineSegmentDistribution { Start = new Vector3F(-0.2f, 0, -6), End = new Vector3F(0.2f, 0, -6) }
      });

      // The particles are rendered using axial billboards.
      ps.Parameters.AddUniform<BillboardOrientation>(ParticleParameterNames.BillboardOrientation).DefaultValue = 
        BillboardOrientation.AxialViewPlaneAligned;

      // The "Axis" parameter defines the up-axis of the particle billboard. In this
      // case the "Axis" is the direction of each particle.
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Axis);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Axis,
        Distribution = new DirectionDistribution { Deviation = 0.3f, Direction = new Vector3F(1, 0.5f, -1f).Normalized },
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(15, 20),
      });

      ps.Effectors.Add(new LinearVelocityEffector
      {
        DirectionParameter = ParticleParameterNames.Axis,
      });

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Damping).DefaultValue = 0.5f;
      ps.Effectors.Add(new SingleDampingEffector());

      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.LinearAcceleration).DefaultValue = new Vector3F(0, -5f, 0);
      ps.Effectors.Add(new LinearAccelerationEffector
      {
        DirectionParameter = ParticleParameterNames.Axis
      });

      // Create a collision plane effector for the ground.
      // We do not position the collision plane at height. Instead, we add a small offset otherwise
      // the particle billboards would be cut off by the visible ground.
      ps.Effectors.Add(new CollisionPlaneEffector
      {
        DirectionParameter = ParticleParameterNames.Axis,
        Plane = new Plane(new Vector3F(0, 1, 0), 0.03f),
      });

      // Add more collision plane effectors for the 4 walls of our sandbox.
      const float offset = 0.08f;
      ps.Effectors.Add(new CollisionPlaneEffector
      {
        DirectionParameter = ParticleParameterNames.Axis,
        Plane = new Plane(new Vector3F(-1, 0, 0), -10 + offset),
      });
      ps.Effectors.Add(new CollisionPlaneEffector
      {
        DirectionParameter = ParticleParameterNames.Axis,
        Plane = new Plane(new Vector3F(0, 0, 1), -10 + offset),
      });
      ps.Effectors.Add(new CollisionPlaneEffector
      {
        DirectionParameter = ParticleParameterNames.Axis,
        Plane = new Plane(new Vector3F(1, 0, 0), -10 + offset),
      });
      ps.Effectors.Add(new CollisionPlaneEffector
      {
        DirectionParameter = ParticleParameterNames.Axis,
        Plane = new Plane(new Vector3F(0, 0, -1), -10 + offset),
      });

      // Particles billboards get stretched in the y-direction. The stretch is time-dependent.
      // (The y-direction of a particle is defined by the "Axis" parameter.)
      ps.Parameters.AddUniform<float>(ParticleParameterNames.SizeX).DefaultValue = 0.05f;
      ps.Parameters.AddVarying<float>(ParticleParameterNames.SizeY);
      ps.Effectors.Add(new SingleLinearSegment3Effector
      {
        OutputParameter = ParticleParameterNames.SizeY,
        Time0 = 0,
        Value0 = 0.05f,
        Time1 = 0.01f,
        Value1 = 0.5f,
        Time2 = 1,
        Value2 = 0.03f,
      });

      ps.Parameters.AddUniform<Vector3F>("StartColor").DefaultValue = new Vector3F(1.0f, 1.0f, 0.8f);
      ps.Parameters.AddUniform<Vector3F>("EndColor").DefaultValue = new Vector3F(1.0f, 0.3f, 0.0f);
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Color);
      ps.Effectors.Add(new Vector3FLerpEffector
      {
        ValueParameter = ParticleParameterNames.Color,
        StartParameter = "StartColor",
        EndParameter = "EndColor",
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 1f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        FadeInStart = 0f,
        FadeInEnd = 0.0f,
        FadeOutStart = 0.99f,
        FadeOutEnd = 1f,
      });

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        contentManager.Load<Texture2D>("Particles/Spark");

      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0.0f;
      
      return ps;
    }
  }
}