using DigitalRune.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    "This sample shows how to create your first, very simple particle system.",
    "",
    1)]
  public class BasicParticlesSample : ParticleSample
  {
    private readonly ParticleSystem _particleSystem;
    private readonly ParticleSystemNode _particleSystemNode;


    public BasicParticlesSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Create a new "empty" particle system.
      _particleSystem = new ParticleSystem();

      // Names are optional, but very useful for debugging.
      _particleSystem.Name = "MyFirstParticleSystem";

      // The particle system uses pre-allocated arrays. We should define an upper limit for
      // the number of particles that can be alive at the same moment.
      _particleSystem.MaxNumberOfParticles = 200;

      // The particle system's Pose defines the position and orientation of the particle system
      // in the world. 
      _particleSystem.Pose = new Pose(new Vector3F(0, 2, 0));

      // The properties of the particles in the particle system are defined using 
      // "particle parameters" (in the collection _particleSystem.Parameters).
      // Per default, there is only one parameter: "NormalizedAge" - which is managed
      // by the particle system itself and is the age of a particle in the range 0 - 1.

      // All our particles should live for 1 second after they have been created. Therefore,
      // we add a "uniform" parameter called "Lifetime" and set it to 1.
      var lifetimeParameter = _particleSystem.Parameters.AddUniform<float>("Lifetime");
      lifetimeParameter.DefaultValue = 1f;

      // Each particle should have a position value. Therefore, we add a "varying" parameter
      // called "Position". "Varying" means that each particle has its own position value.
      // The particle system will internally allocate a Vector3F array to store all particle
      // positions.
      _particleSystem.Parameters.AddVarying<Vector3F>("Position");

      // When particles are created, we want them to appear at random position in a spherical
      // volume. We add an effector which initializes the particle "Positions" of newly created
      // particles.
      _particleSystem.Effectors.Add(new StartPositionEffector
      {
        // This effector should initialize the "Position" parameter.
        // Parameter = "Position",     // "Position" is the default value anyway.

        // The start values should be chosen from this random value distribution:
        Distribution = new SphereDistribution { OuterRadius = 2 }
      });

      // The particles should slowly fade in and out to avoid sudden appearance and disappearance.
      // We add a varying particle parameter called "Alpha" to store the alpha value per particle.
      _particleSystem.Parameters.AddVarying<float>("Alpha");

      // The SingleFadeEffector animates a float parameter from 0 to a target value and
      // back to 0.
      _particleSystem.Effectors.Add(new SingleFadeEffector
      {
        // If TargetValueParameter is not set, then the target value is 1.
        //TargetValueParameter = 1,

        // The fade-in/out times are relative to a time parameter. 
        // By default the "NormalizedAge" of the particles is used.
        //TimeParameter = "NormalizedAge",

        // The Alpha value should be animated.
        ValueParameter = "Alpha",

        // The fade-in/out times relative to the normalized age.
        FadeInStart = 0.0f,
        FadeInEnd = 0.3f,
        FadeOutStart = 0.5f,
        FadeOutEnd = 1.0f,
      });

      // Next, we choose a texture for the particles. All particles use the same texture 
      // parameter, which means the parameter is "uniform".
      var textureParameter = _particleSystem.Parameters.AddUniform<Texture2D>("Texture");
      textureParameter.DefaultValue = ContentManager.Load<Texture2D>("Particles/LensFlare");

      // The blend mode is a value between 0 and 1, where 0 means additive blending
      // 1 means alpha blending. Values between 0 and 1 are allowed. The particles in
      // this example should be drawn using additive alpha blending. 
      var blendModeParameter = _particleSystem.Parameters.AddUniform<float>("BlendMode");
      blendModeParameter.DefaultValue = 0.0f;

      // There is a lot to configure. Did we forget anything? - We can use an optional helper method
      // to validate our particle system. Uninitialized or missing parameters are printed to the
      // Console. Check the Visual Studio Output window for any messages.
      ParticleSystemValidator.Validate(_particleSystem);

      // Adding the particle system to a ParticleSystemService is optional but very useful
      // because the service will update the particle system for us in each frame.
      ParticleSystemService.ParticleSystems.Add(_particleSystem);

      // To render the particle effect, we need to create a scene node and add it to the
      // scene graph.
      _particleSystemNode = new ParticleSystemNode(_particleSystem);
      GraphicsScreen.Scene.Children.Add(_particleSystemNode);

      // A tip for the future: 
      // The class ParticleParameterNames is a collection of strings that can be used for 
      // common particle parameters. It is recommended to use the particle parameter names in
      // this class to avoid problems because of typing errors in the source code.
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Clean up.
        // This code demonstrates how to destroy the particle system.
        // Note: These operations are redundant because the Sample base class
        // removes all particle system and disposes the whole graphics screen.
        //ParticleSystemService.ParticleSystems.Remove(_particleSystem);
        //GraphicsScreen.Scene.Children.Remove(_particleSystemNode);
        //_particleSystemNode.Dispose(false);
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // Add 2 new particles in each frame.
      _particleSystem.AddParticles(2);

      // Particles are added and simulated when the particle system service is updated. 
      // In this example the service is updated in Game1.Update(). The service is updated
      // after all GameComponents, before Game1.Draw().

      // The ParticleSystemNode needs to be synchronized with the ParticleSystem.
      // The Synchronize() method takes a snapshot of the current particles which 
      // is then rendered by the graphics service.
      // (This explicit synchronization is necessary because the particle system 
      // service and the graphics service may run in parallel on multiple threads.)
      _particleSystemNode.Synchronize(GraphicsService);

      Profiler.AddValue("ParticleCount", ParticleHelper.CountNumberOfParticles(ParticleSystemService.ParticleSystems));
    }
  }
}
