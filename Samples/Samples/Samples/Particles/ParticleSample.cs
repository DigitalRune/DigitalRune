using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace Samples.Particles
{
  // The base class for particle samples.
  public abstract class ParticleSample : BasicSample
  {
    protected ParticleSample(Microsoft.Xna.Framework.Game game) 
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3F(0, 2, 10), 0, 0);

      GameObjectService.Objects.Add(new SandboxObject(Services));
    }
  }
}
