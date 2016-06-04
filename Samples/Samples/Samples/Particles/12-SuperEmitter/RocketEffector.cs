using System;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles;


namespace Samples.Particles
{
  // This effector creates a RocketTrail particle system for each particle. All RocketTrail
  // particle systems are stored in varying particle parameter "RocketTrail". Each rocket trail
  // moves with the associated particle. 
  // When a particle dies, a RocketExplosion particle system is triggered.
  // Note: The child RocketTrail and RocketExplosion particle systems remove themselves 
  // automatically when all their particles have died (using a ParticleSystemRecycler effector).
  public class RocketEffector : ParticleEffector
  {
    // Needed particle parameters.
    private IParticleParameter<float> _normalizedAgeParameter;
    private IParticleParameter<Vector3F> _positionParameter;
    private IParticleParameter<Vector3F> _directionParameter;
    private IParticleParameter<float> _linearSpeedParameter;
    private IParticleParameter<ParticleSystem> _trailParameter;


    // Creates a new instance of this class (for cloning).
    protected override ParticleEffector CreateInstanceCore()
    {
      return new RocketEffector();
    }


    protected override void OnRequeryParameters()
    {
      _normalizedAgeParameter = ParticleSystem.Parameters.Get<float>(ParticleParameterNames.NormalizedAge);
      _positionParameter = ParticleSystem.Parameters.Get<Vector3F>(ParticleParameterNames.Position);
      _directionParameter = ParticleSystem.Parameters.Get<Vector3F>(ParticleParameterNames.Direction);
      _linearSpeedParameter = ParticleSystem.Parameters.Get<float>(ParticleParameterNames.LinearSpeed);

      // The "RocketTrail" parameter is required and added by this effector.
      // Note: It is safe to call AddVarying() several times. The particle parameter is added only
      // once.
      _trailParameter = ParticleSystem.Parameters.AddVarying<ParticleSystem>("RocketTrail");
    }


    protected override void OnUninitialize()
    {
      _normalizedAgeParameter = null;
      _positionParameter = null;
      _directionParameter = null;
      _linearSpeedParameter = null;
      _trailParameter = null;
    }


    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      // Get the particle parameter arrays.
      var ages = _normalizedAgeParameter.Values;
      var positions = _positionParameter.Values;
      var directions = _directionParameter.Values;
      var speeds = _linearSpeedParameter.Values;
      var trails = _trailParameter.Values;

      for (int i = startIndex; i < startIndex + count; i++)
      {
        float age = ages[i];

        if (age >= 1000)
        {
          // The particle is dead and the death was already handled by us.
          continue;
        }

        if (age < 1.0f)
        {
          // The particle is still living.

          // Get the rocket trail for this particle.
          var trail = trails[i];
          if (trail == null)
          {
            // There is no rocket trail --> Create a new RocketTrail particle system for this
            // particle.
            // The RocketTrail is stored in the "RocketTrail" particle parameter and it is
            // added to the children of the Rockets particle system (because the Rockets particle
            // system should automatically update the RocketTrail particle system).
            trail = RocketTrail.Obtain();
            trails[i] = trail;
            ParticleSystem.Children.Add(trail);
          }

          // Move the RocketTrail with the particle.
          trail.Pose = new Pose(positions[i]);

          // Set the rocket particle velocity as the "EmitterVelocity" of the RocketTrail. This
          // velocity influences the start velocities of the rocket trails smoke particles.
          // The smoke should initially move with the rocket.
          trail.Parameters.Get<Vector3F>(ParticleParameterNames.EmitterVelocity).DefaultValue = directions[i] * speeds[i];
        }
        else
        {
          // The particle has just died.

          // Set the age to any high number that indicates that we have already dealt with the
          // death of this particle.
          ages[i] = 1000;

          // Update the RocketTrail particle system for this rocket particle one last time.
          var trail = trails[i];
          if (trail != null)
          {
            trail.Pose = new Pose(positions[i]);
            trail.Parameters.Get<float>(ParticleParameterNames.EmissionRate).DefaultValue = 0;

            // Remove the reference in the "RocketTrail" parameter. 
            trails[i] = null;

            // Note: The RocketTrail has a ParticleSystemRecycler effector that will automatically
            // remove the RocketTrail from ParticleSystem.Children when all RocketTrail particles
            // are dead.
          }

          // Trigger an explosion add the position where the rocket particle has died.
          var explosion = RocketExplosion.Obtain();
          explosion.Pose = new Pose(positions[i]);
          explosion.Parameters.Get<Vector3F>(ParticleParameterNames.EmitterVelocity).DefaultValue = directions[i] * speeds[i];
          ParticleSystem.Children.Add(explosion);

          // Note: The RocketExplosion as a ParticleSystemRecycler effector that will automatically
          // remove the RocketExplosion from ParticleSystem.Children when all RocketExplosion 
          // particles are dead.
        }
      }
    }
  }
}
