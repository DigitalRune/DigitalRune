using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  // Creates a bee swarm effect using texture animation.
  // The "CameraPose" particle parameter must be set externally each frame.
  public class BeeSwarm
  {
    public static ParticleSystem Create(ContentManager contentManager)
    {
      var ps = new ParticleSystem
      {
        Name = "BeeSwarm",
        MaxNumberOfParticles = 100,
      };

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = float.PositiveInfinity;

      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        DefaultValue = new Vector3F(0, 0, 0)
      });

      ps.Parameters.AddUniform<float>(ParticleParameterNames.SizeY).DefaultValue = 0.1f;

      // The SizeX is varying because the BeeEffector sets a negative size if the bee should look in the
      // opposite direction.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.SizeX);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.SizeX,
        DefaultValue = 0.1f,
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(1, 2),
      });

      // The BeeEffector creates the random movement of the bees.
      ps.Parameters.AddVarying<Vector3F>("TargetPosition");
      ps.Parameters.AddUniform<Pose>("CameraPose").DefaultValue = Pose.Identity;
      ps.Effectors.Add(new BeeEffector
      {
        PositionParameter = ParticleParameterNames.Position,
        TargetPositionParameter = "TargetPosition",
        SpeedParameter = ParticleParameterNames.LinearSpeed,
        SizeXParameter = ParticleParameterNames.SizeX,
        CameraPoseParameter = "CameraPose",
        InvertLookDirection = true,
        MaxRange = 4.0f,
      });

      // The texture is a set of 3 images.
      ps.Parameters.AddUniform<PackedTexture>(ParticleParameterNames.Texture).DefaultValue =
        new PackedTexture(
          "Bee",
          contentManager.Load<Texture2D>("Particles/beeWingFlap"),
          Vector2F.Zero, Vector2F.One,
          3, 1);

      // The Frame particle parameter stores the index of the animation frame and the
      // AnimationTime particle parameter stores the current progress in seconds. 
      ps.Parameters.AddVarying<int>("Frame");
      ps.Parameters.AddVarying<float>("AnimationTime");

      // Initialize the AnimationTime with a random value, otherwise all bees would look
      // the same.
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "AnimationTime",
        Distribution = new UniformDistributionF(0, 0.125f),
      });

      // The AnimationEffector advances the AnimationTime and sets the Frame. 
      // It changes frames at 24 fps.
      ps.Effectors.Add(new AnimationEffector
      {
        AnimationTimeParameter = "AnimationTime",
        FramesPerSecond = 24,
        NumberOfFrames = 3,
      });

      ParticleSystemValidator.Validate(ps);

      return ps;
    }
  }
}
