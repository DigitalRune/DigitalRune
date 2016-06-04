using DigitalRune;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

// Both XNA and DigitalRune also have a type called CurveLoopType. To avoid compiler 
// errors we need to define which CurveLoopType we want to use.
using CurveLoopType = DigitalRune.Mathematics.Interpolation.CurveLoopType;


namespace Samples.Mathematics
{
  [Sample(SampleCategory.Mathematics,
    @"This example shows how to use Curve2F.",
    @"A curve with random key points is generated. The curve loop type and spline 
interpolation type can be changed.",
    3)]
  [Controls(@"Sample
  Press <Space> generate a random curve.
  Press <1> to change pre/post-loop type.
  Press <2> to change interpolation type.
  Press <3> to change toggle curve end smoothing.")]
  public class CurveSample : BasicSample
  {
    // A curve.
    private Curve2F _curve;

    // The curve loop type that we will use.
    private CurveLoopType _loopType = CurveLoopType.Cycle;

    // The spline interpolation type that we will use.
    private SplineInterpolation _interpolationType = SplineInterpolation.CatmullRom;

    // The setting for curve end smoothing.
    private bool _smoothEnds = false;


    public CurveSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;
    }


    public override void Update(GameTime gameTime)
    {
      bool updateCurve = false;

      // When the sample is started or when <Space> is pressed, we create a random curve.
      if (_curve == null || InputService.IsPressed(Keys.Space, true))
      {
        _curve = new Curve2F
        {
          PreLoop = _loopType,
          PostLoop = _loopType,
          SmoothEnds = _smoothEnds
        };

        // Create random key points.
        for (float x = 0; x <= 1; x += 0.1f)
        {
          float y = RandomHelper.Random.NextFloat(0, 1);

          _curve.Add(new CurveKey2F
          {
            Point = new Vector2F(x, y),

            // We use the same interpolation type (spline type) for all curve keys. But 
            // it is possible to use a different interpolation type for each curve key.
            Interpolation = _interpolationType,
          });
        }

        updateCurve = true;
      }

      // If <1> is pressed, we change the loop type.
      if (InputService.IsPressed(Keys.D1, true))
      {
        // Choose next enumeration value for _loopType.
        if ((int)_loopType + 1 >= EnumHelper.GetValues(typeof(CurveLoopType)).Length)
          _loopType = 0;
        else
          _loopType++;

        _curve.PreLoop = _loopType;
        _curve.PostLoop = _loopType;
        updateCurve = true;
      }

      // If <2> is pressed, we change the spline type.
      if (InputService.IsPressed(Keys.D2, true))
      {
        // Choose next enumeration value for _interpolationType.
        if ((int)_interpolationType + 1 >= EnumHelper.GetValues(typeof(SplineInterpolation)).Length)
          _interpolationType = 0;
        else
          _interpolationType++;

        // We skip Bézier and Hermite splines because both need additional information per
        // curve key (control points or tangents), which we did not specify in the curve creation.
        while (_interpolationType == SplineInterpolation.Bezier
            || _interpolationType == SplineInterpolation.Hermite)
        {
          _interpolationType++;
        }

        _curve.ForEach(key => key.Interpolation = _interpolationType);
        updateCurve = true;
      }

      // If <3> is pressed, we toggle "SmoothEnds". 
      // If SmoothEnds is enabled the curve is smoother at the first and at the last 
      // key point. The effect of SmoothEnds is visible, for example, if the loop type 
      // is "Oscillate" and the spline type is "CatmullRom".
      if (InputService.IsPressed(Keys.D3, true))
      {
        _smoothEnds = !_smoothEnds;
        _curve.SmoothEnds = _smoothEnds;
        updateCurve = true;
      }

      if (updateCurve)
      {
        var debugRenderer = GraphicsScreen.DebugRenderer2D;
        debugRenderer.Clear();
        debugRenderer.DrawText(
          string.Format("\n\nLoop type = {0}\nInterpolation = {1}\nSmoothEnds = {2}",
          _curve.PreLoop, _curve[0].Interpolation, _curve.SmoothEnds));

        // Draw two lines to create chart axes.
        debugRenderer.DrawLine(new Vector3F(100, 300, 0), new Vector3F(1000, 300, 0), Color.Black, true);
        debugRenderer.DrawLine(new Vector3F(300, 100, 0), new Vector3F(300, 320, 0), Color.Black, true);

        Vector2F scale = new Vector2F(400, 200);
        Vector2F offset = new Vector2F(300, 100);

        // Draw a small cross for all curve key points.
        for (int index = 0; index < _curve.Count; index++)
        {
          CurveKey2F key = _curve[index];
          Vector2F point = scale * key.Point + offset;
          debugRenderer.DrawLine(new Vector3F(point.X - 5, point.Y - 5, 0), new Vector3F(point.X + 5, point.Y + 5, 0), Color.Black, true);
          debugRenderer.DrawLine(new Vector3F(point.X + 5, point.Y - 5, 0), new Vector3F(point.X - 5, point.Y + 5, 0), Color.Black, true);
        }

        // Draw the curve.
        const float stepSize = 0.02f;
        for (float x = -0.5f; x < 1.7f; x += stepSize)
        {
          Vector2F point0 = scale * _curve.GetPoint(x) + offset;
          Vector2F point1 = scale * _curve.GetPoint(x + stepSize) + offset;
          debugRenderer.DrawLine(new Vector3F(point0.X, point0.Y, 0), new Vector3F(point1.X, point1.Y, 0), Color.Black, true);
        }
      }

      base.Update(gameTime);
    }
  }
}
