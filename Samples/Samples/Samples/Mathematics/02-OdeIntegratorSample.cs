using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Analysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Mathematics
{
  [Sample(SampleCategory.Mathematics,
    @"This example shows how to numerically integrate an ordinary differential equation (ODE).",
    @"Equation dy/dt = -12 * y is numerically integrated with three different methods.
y(t) is plotted from t = 0 to t = 1, starting with y(0) = 100.
With increasing time step Explicit Euler integration is the first to become unstable.",
    2)]
  [Controls(@"Sample
  Press <1>/<2> to increase/decrease the size of the integration time step.")]
  public class OdeIntegratorSample : BasicSample
  {
    // The time step size for the numerical integration steps.
    private float _deltaTime = 0.03f;


    public OdeIntegratorSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;
    }


    // This method computes dy/dt = -12 * y.
    private static VectorF ComputeDerivative(VectorF y, float t)
    {
      return -12 * y;
    }


    public override void Update(GameTime gameTime)
    {
      // Pressing <1> increases the time step.
      if (InputService.IsDown(Keys.D1))
        _deltaTime *= 1.01f;

      // Pressing <2> increases the time step.
      if (InputService.IsDown(Keys.D2))
        _deltaTime /= 1.01f;

      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      debugRenderer.Clear();
      debugRenderer.DrawText("\n\nIntegration time step = " + _deltaTime);

      // Draw y(t) three times using different integration methods.
      PlotGraph(new ExplicitEulerIntegratorF(ComputeDerivative), 200, "Explicit Euler");
      PlotGraph(new MidpointIntegratorF(ComputeDerivative), 350, "Midpoint Method");
      PlotGraph(new RungeKutta4IntegratorF(ComputeDerivative), 500, "Runge Kutta");

      base.Update(gameTime);
    }


    // Draws y(t). yOffset is used to position the graph on the screen.
    private void PlotGraph(OdeIntegratorF integrator, float yOffset, string text)
    {
      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      debugRenderer.DrawText(text, new Vector2F(400, yOffset - 90), Color.Black);
      debugRenderer.DrawLine(
        new Vector3F(90, yOffset, 0),
        new Vector3F(700, yOffset, 0),
        Color.Black,
        true);
      debugRenderer.DrawLine(
        new Vector3F(100, yOffset - 110, 0),
        new Vector3F(100, yOffset + 10, 0),
        Color.Black,
        true);

      // In the general case y can be a vector. In this example, y is one-dimensional.
      // We start with y(0) = 100.
      VectorF y = new VectorF(new float[1] { 100 });
      float lastY = y[0];

      // Plot the graph from t = 0 to t = 1.
      for (float time = 0; time < 1; time += _deltaTime)
      {
        // Given y(time), compute y(time + deltaTime).
        y = integrator.Integrate(y, time, time + _deltaTime);

        debugRenderer.DrawLine(
          new Vector3F(100 + time * 600, yOffset - lastY, 0),
          new Vector3F(100 + (time + _deltaTime) * 600, yOffset - y[0], 0),
          Color.Black,
          true);

        lastY = y[0];
      }
    }
  }
}
