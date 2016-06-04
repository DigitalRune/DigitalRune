using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  /// <summary>
  /// A custom force effect that pulls objects to the origin - like a gravity of a planet.
  /// </summary>
  public class CustomGravity : ForceField
  {
    public override void Apply(RigidBody body)
    {
      // Gravity forces act on the center of mass.
      Vector3F centerOfMass = body.PoseCenterOfMass.Position;

      // Normalize the vector.
      if (!centerOfMass.TryNormalize())
      {
        // Unable to normalize the vector, the body is already in the gravity origin.
        return;
      }

      // The gravity should act from the center of mass to the origin.
      Vector3F gravityDirection = -centerOfMass;

      // Add gravity force with an acceleration of 9.81 m/s².
      // (force = mass * acceleration)
      AddForce(body, body.MassFrame.Mass * gravityDirection * 9.81f);
    }
  }
}