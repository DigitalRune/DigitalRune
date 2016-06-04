using DigitalRune.Animation;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using Microsoft.Xna.Framework;
using CurveLoopType = DigitalRune.Mathematics.Interpolation.CurveLoopType;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    "This sample uses a curve-based animation to move a sprite on a 2D path.",
    "",
    3)]
  public class PathAnimationSample : AnimationSample
  {
    private readonly AnimatableProperty<Vector2F> _animatablePosition = new AnimatableProperty<Vector2F>();


    public PathAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      Rectangle bounds = GraphicsService.GraphicsDevice.Viewport.Bounds;

      // Create a 2D path.
      Path2F path = new Path2F
      {
        // Path is cyclic.
        PreLoop = CurveLoopType.Cycle,
        PostLoop = CurveLoopType.Cycle,

        //  End of path should smoothly interpolate with start of path.
        SmoothEnds = true,
      };

      // The spline type.
      const SplineInterpolation splineInterpolation = SplineInterpolation.BSpline;

      // Add path keys. The parameter of a path key is the time in seconds.
      path.Add(new PathKey2F
      {
        Parameter = 0,
        Point = new Vector2F(bounds.Center.X, bounds.Center.Y),
        Interpolation = splineInterpolation,
      });
      path.Add(new PathKey2F
      {
        Parameter = 0.5f,
        Point = new Vector2F(bounds.Center.X / 2.0f, 2.0f * bounds.Center.Y / 3.0f),
        Interpolation = splineInterpolation,
      });
      path.Add(new PathKey2F
      {
        Parameter = 1.0f,
        Point = new Vector2F(bounds.Center.X, 1.0f * bounds.Center.Y / 3.0f),
        Interpolation = splineInterpolation,
      });
      path.Add(new PathKey2F
      {
        Parameter = 1.5f,
        Point = new Vector2F(3.0f * bounds.Center.X / 2.0f, 2.0f * bounds.Center.Y / 3.0f),
        Interpolation = splineInterpolation,
      });
      path.Add(new PathKey2F
      {
        Parameter = 2.0f,
        Point = new Vector2F(bounds.Center.X, bounds.Center.Y),
        Interpolation = splineInterpolation,
      });
      path.Add(new PathKey2F
      {
        Parameter = 2.5f,
        Point = new Vector2F(bounds.Center.X / 2.0f, 4.0f * bounds.Center.Y / 3.0f),
        Interpolation = splineInterpolation,
      });

      path.Add(new PathKey2F
      {
        Parameter = 3.0f,
        Point = new Vector2F(bounds.Center.X, 5.0f * bounds.Center.Y / 3.0f),
        Interpolation = splineInterpolation,
      });
      path.Add(new PathKey2F
      {
        Parameter = 3.5f,
        Point = new Vector2F(3.0f * bounds.Center.X / 2.0f, 4.0f * bounds.Center.Y / 3.0f),
        Interpolation = splineInterpolation,
      });
      path.Add(new PathKey2F
      {
        Parameter = 4.0f,
        Point = new Vector2F(bounds.Center.X, bounds.Center.Y),
        Interpolation = splineInterpolation,
      });

      // Create a path animation using the path.
      // (Start at parameter value 0 and loop forever.)
      Path2FAnimation pathAnimation = new Path2FAnimation(path)
      {
        StartParameter = 0,
        EndParameter = float.PositiveInfinity,
      };

      AnimationService.StartAnimation(pathAnimation, _animatablePosition).UpdateAndApply();
    }


    protected override void OnRender(RenderContext context)
    {
      GraphicsService.GraphicsDevice.Clear(Color.White);

      // Draw sprite centered at the animated position.
      Vector2 position = (Vector2)_animatablePosition.Value - new Vector2(Logo.Width, Logo.Height) / 2.0f;

      SpriteBatch.Begin();
      SpriteBatch.Draw(Logo, position, Color.Red);
      SpriteBatch.End();
    }
  }
}
