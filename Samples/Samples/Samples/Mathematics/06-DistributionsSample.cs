using System;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

// Both XNA and DigitalRune have a class called MathHelper. To avoid compiler errors
// we need to define which MathHelper we want to use.
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Mathematics
{
  [Sample(SampleCategory.Mathematics,
    @"This example shows how to use random value distributions.",
    @"A target cross is drawn on the screen. When <Space> is pressed, a shot is fired.
The random value distributions determine the result of the shot.
A uniform distribution determines a random angle from 0° to 360°.
A Gaussian distribution determines the distance from the goal. Using a Gaussian distribution
more shots will hit near the goal. In the distance there will be less hits. This is more
realistic than using only uniform distributions.",
    6)]
  [Controls(@"Sample
  Press <Space> to fire a shot.")]
  public class DistributionsSample : BasicSample
  {
    private readonly Vector3F _center = new Vector3F(300, 300, 0);
    private readonly Random _random = new Random();

    // Here are two random value distributions.
    // The first distribution is used to determine a random angle from 0° to 360°.
    private readonly Distribution<float> _angleDistribution = new UniformDistributionF(0, 360);

    // The second distribution is used to determine a random distance from the goal.
    // The Expected Value is 0 (no distance from the goal). A standard deviation of 40 is used. 
    private readonly Distribution<float> _distanceDistribution = new FastGaussianDistributionF(0, 40);

    public DistributionsSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      GraphicsScreen.ClearBackground = true;

      // Draw a cross for the target.
      debugRenderer.DrawLine(_center - new Vector3F(100, 0, 0), _center + new Vector3F(100, 0, 0), Color.Black, true);
      debugRenderer.DrawLine(_center - new Vector3F(0, 100, 0), _center + new Vector3F(0, 100, 0), Color.Black, true);
    }


    public override void Update(GameTime gameTime)
    {
      // If <Space> is pressed, we fire a shot.
      if (InputService.IsPressed(Keys.Space, true))
      {
        // Get a random angle and a random distance from the target.
        float angle = _angleDistribution.Next(_random);
        float distance = _distanceDistribution.Next(_random);

        // Create a vector v with the length of distance.
        Vector3F v = new Vector3F(0, distance, 0);

        // Rotate v.
        QuaternionF rotation = QuaternionF.CreateRotationZ(MathHelper.ToRadians(angle));
        v = rotation.Rotate(v);

        // Draw a small cross for the hit.
        var debugRenderer = GraphicsScreen.DebugRenderer2D;
        debugRenderer.DrawLine(
          _center + v + new Vector3F(-10, -10, 0),
          _center + v + new Vector3F(10, 10, 0),
          Color.Black,
          true);
        debugRenderer.DrawLine(
          _center + v + new Vector3F(10, -10, 0),
          _center + v + new Vector3F(-10, 10, 0),
          Color.Black, true);
      }

      base.Update(gameTime);
    }
  }
}
