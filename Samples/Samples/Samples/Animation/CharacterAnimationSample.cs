using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Animation
{
  // The base class for character animation samples.
  public class CharacterAnimationSample : BasicSample
  {
    public CharacterAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3F(0, 1, 3), 0, 0);

      // Add gravity and damping to the physics simulation. 
      // Note: The physics simulation is only used by the ragdoll samples.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a ground object.
      GameObjectService.Objects.Add(new GroundObject(Services));
    }
  }
}
