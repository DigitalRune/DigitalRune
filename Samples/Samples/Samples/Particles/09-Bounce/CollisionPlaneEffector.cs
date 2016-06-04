using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles;


namespace Samples.Particles
{
  // This class shows how to implement a custom effector.
  // The CollisionPlaneEffector uses following particle parameters:
  // - a Position parameter,
  // - a Direction parameter,
  // - a LinearSpeed parameter,
  // - a Restitution parameter (optional) that defines the "bounciness" (0 means no bounce, 1 means 
  //   perfectly elastic bounce).
  // If the particle positions are behind a configurable plane, the particle direction is reflected
  // and the LinearSpeed is reduced (depending on the restitution).
  // 
  // This class is not optimized for performance. More information on creating particle effectors
  // can be found in the user documentation of DigitalRune Particles.
  public class CollisionPlaneEffector : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<Vector3F> _positionParameter;
    private IParticleParameter<Vector3F> _directionParameter;
    private IParticleParameter<float> _linearSpeedParameter;
    private IParticleParameter<float> _restitutionParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    [ParticleParameter(ParticleParameterUsage.In)]
    public string PositionParameter { get; set; }

    [ParticleParameter(ParticleParameterUsage.InOut)]
    public string DirectionParameter { get; set; }

    [ParticleParameter(ParticleParameterUsage.InOut)]
    public string LinearSpeedParameter { get; set; }

    [ParticleParameter(ParticleParameterUsage.In, Optional = true)]
    public string RestitutionParameter { get; set; }


    public Plane Plane
    {
      get { return _plane; }
      set { _plane = value; }
    }
    private Plane _plane;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public CollisionPlaneEffector()
    {
      PositionParameter = ParticleParameterNames.Position;
      DirectionParameter = ParticleParameterNames.Direction;
      LinearSpeedParameter = ParticleParameterNames.LinearSpeed;
      _plane = new Plane(new Vector3F(0, 1, 0), 0);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    // Creates a new instance of this type.
    protected override ParticleEffector CreateInstanceCore()
    {
      return new CollisionPlaneEffector();
    }


    // Copies all parameter of the given effector.
    protected override void CloneCore(ParticleEffector source)
    {
      base.CloneCore(source);

      var sourceTyped = (CollisionPlaneEffector)source;
      PositionParameter = sourceTyped.PositionParameter;
      DirectionParameter = sourceTyped.DirectionParameter;
      LinearSpeedParameter = sourceTyped.LinearSpeedParameter;
      RestitutionParameter = sourceTyped.RestitutionParameter;
      Plane = sourceTyped.Plane;
    }
    #endregion


    // OnRequeryParameters is called when the particle system is started and when the particle 
    // parameter collection is changed. Here we cache references to the needed parameters.
    protected override void OnRequeryParameters()
    {
      _positionParameter = ParticleSystem.Parameters.Get<Vector3F>(PositionParameter);
      _directionParameter = ParticleSystem.Parameters.Get<Vector3F>(DirectionParameter);
      _linearSpeedParameter = ParticleSystem.Parameters.Get<float>(LinearSpeedParameter);
      _restitutionParameter = ParticleSystem.Parameters.Get<float>(RestitutionParameter);
    }


    // Called when the particle effector is removed from the particle system.
    // This method should remove any held references.
    protected override void OnUninitialize()
    {
      _positionParameter = null;
      _directionParameter = null;
      _linearSpeedParameter = null;
      _restitutionParameter = null;
    }


    // This method is called each frame to update particles.
    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      // Abort if we are missing particle parameters.
      if (_positionParameter == null
          || _linearSpeedParameter == null
          || _directionParameter == null)
      {
        return;
      }

      // Get the direction and linear speed particle parameter arrays.
      for (int i = startIndex; i < startIndex + count; i++)
      {
        // Get the particle position and check if the position is behind the plane.
        Vector3F position = _positionParameter.GetValue(i);
        if (Vector3F.Dot(position, _plane.Normal) > _plane.DistanceFromOrigin)
          continue;

        // Get the linear velocity of the particle.
        Vector3F velocity = _directionParameter.GetValue(i) * _linearSpeedParameter.GetValue(i);

        // Check if the particle is moving into the plane or away.
        float normalSpeed = Vector3F.Dot(velocity, _plane.Normal);
        if (normalSpeed > 0)
          continue;

        // Get the restitution. If there is no restitution particle parameter, we use a default value.
        float restitution = (_restitutionParameter != null) ? _restitutionParameter.GetValue(i) : 0.5f;

        // Change the velocity to let the particle bounce off.
        velocity = velocity - (1 + restitution) * _plane.Normal * normalSpeed;

        // Update LinearSpeed and Direction from the velocity vector.
        // The speed is the magnitude of the velocity vector.
        var newSpeed = velocity.Length;
        _linearSpeedParameter.SetValue(i, newSpeed);

        // Direction stores the normalized direction of the velocity vector.
        if (!Numeric.IsZero(newSpeed))
          _directionParameter.SetValue(i, velocity / newSpeed);
      }
    }
    #endregion
  }
}
