using System.Linq;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;

// Both XNA and DigitalRune also have a type called CurveLoopType. To avoid compiler 
// errors we need to define which CurveLoopType we want to use.
using CurveLoopType = DigitalRune.Mathematics.Interpolation.CurveLoopType;


namespace Samples.Mathematics
{
  [Sample(SampleCategory.Mathematics,
    "This example shows how to move an object a long a 3D path with constant speed (using Path3F).",
    "",
    4)]
  public class PathSample : BasicSample
  {
    // The original 3D path.
    private Path3F _path;

    // The elapsed time, used to animate an object along the path.
    private float _time;


    public PathSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      SetCamera(new Vector3F(0, 2, 5), 0, 0);

      CreatePath();
    }


    // Creates a random 3D path.
    private void CreatePath()
    {
      // Create a cyclic path.
      _path = new Path3F
      {
        PreLoop = CurveLoopType.Cycle,
        PostLoop = CurveLoopType.Cycle,
        SmoothEnds = true
      };

      // Add random path key points.
      for (int i = 0; i < 5; i++)
      {
        float x = RandomHelper.Random.NextFloat(-3, 3);
        float y = RandomHelper.Random.NextFloat(1, 3);
        float z = RandomHelper.Random.NextFloat(-3, 0);
        var key = new PathKey3F
        {
          Parameter = i,
          Point = new Vector3F(x, y, z),
          Interpolation = SplineInterpolation.CatmullRom
        };
        _path.Add(key);
      }

      // The last key uses the same position as the first key to create a closed path.
      var lastKey = new PathKey3F
      {
        Parameter = _path.Count,
        Point = _path[0].Point,
        Interpolation = SplineInterpolation.CatmullRom,
      };
      _path.Add(lastKey);

      // The current path parameter goes from 0 to 5. This path parameter is not linearly
      // proportional to the path length. This is not suitable for animations.
      // To move an object with constant speed along the path, the path parameter should
      // be linearly proportional to the length of the path.
      // ParameterizeByLength() changes the path parameter so that the path parameter
      // at the each key is equal to the length of path (measured from the first key position
      // to the current key position).
      // ParameterizeByLength() uses and iterative process, we end the process after 10 
      // iterations or when the error is less than 0.001f.
      _path.ParameterizeByLength(10, 0.001f);

      // Now, the parameter of the first key (_path[0]) is unchanged.
      // The parameter of the second key (_path[1]) is equal to the length of the path
      // from the first key to the second key.
      // The parameter of the third key (_path[2]) is equal to the length of the path
      // from the first key to the third key.
      // And so on. 

      // The parameter of the last key is equal to the length of the whole path:
      //   float pathLength = _path[_path.Count - 1].Parameter;

      // Important: The path parameter is now equal to the path length at the path keys.
      // But in general between path keys the path parameter is not linearly proportional
      // to the path length. This is due to the nature of splines.
      //
      // Example: 
      // Lets assume the second path key is at path length 100 and the third key is 
      // at path length 200.
      // If we call _path.GetPoint(100), we get the position of the second key.
      // If we call _path.GetPoint(200), we get the position ot the third key.
      // We can call _path.GetPoint(130) to get a position on the path between the second and
      // third key. But it is not guaranteed that the path is exactly 130 long at this position.
      // We only know that the point is somewhere between 100 and 200 path length.
      //
      // To get the path point at exactly the distance 130 from the path start, we have to call
      //   float parameter = _path.GetParameterFromLength(130, 10, 0.01f);
      // This uses an iterative root finding process to find the path parameter where the
      // path length is 130.
      // Then we can get the path position with 
      //   Vector3F pathPointAt130Length = _path.GetPoint(parameter).
    }


    public override void Update(GameTime gameTime)
    {
      // Update _time.
      _time += (float)gameTime.ElapsedGameTime.TotalSeconds;

      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Draw the path key points.
      foreach (var point in _path.Select(key => key.Point))
        debugRenderer.DrawPoint(point, Color.Black, false);

      // Draw the path.
      for (float i = 0; i < _path.Last().Parameter; i += 0.1f)
      {
        var point0 = _path.GetPoint(i);
        var point1 = _path.GetPoint((i + 0.1f));
        debugRenderer.DrawLine(point0, point1, Color.Black, false);
      }

      // Move an object with constant speed along the path.
      const float speed = 2;
      var traveledDistance = _time * speed;
      // Get path parameter where the path length is equal to traveledDistance.
      var parameter = _path.GetParameterFromLength(traveledDistance, 10, 0.01f);
      // Get path point at the traveledDistance.
      Vector3F position = _path.GetPoint(parameter);
      // Get the path tangent at traveledDistance and use it as the forward direction.
      Vector3F forward = _path.GetTangent(parameter).Normalized;
      // Draw an object on the path.
      DrawObject(position, forward);

      base.Update(gameTime);
    }


    // Draws an arrow like object at the given position and pointing into the given
    // forward direction.
    private void DrawObject(Vector3F position, Vector3F forward)
    {
      // Compute two vectors that are orthogonal to the forward direction.
      Vector3F right, up;
      if (Vector3F.AreNumericallyEqual(forward, Vector3F.Up))
      {
        // The forward direction is close to the up vector (0, 1, 0). In this case we 
        // simply set the default directions right (1, 0, 0) and backward (0, 0, 1).
        right = Vector3F.Right;
        up = Vector3F.Backward;
      }
      else
      {
        // Use the cross product calculate the orthogonal directions.
        right = Vector3F.Cross(forward, Vector3F.Up).Normalized;
        up = Vector3F.Cross(right, forward);
      }

      // Length of the object.
      const float length = 0.3f;
      // Width of the object.
      const float width = 0.1f;
      // Position of the tip of the object.
      Vector3F cusp = position + forward * length / 2;

      // We draw the object with 4 lines.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.DrawLine(cusp, cusp - length * forward + width * up + width * right, Color.Black, false);
      debugRenderer.DrawLine(cusp, cusp - length * forward - width * up + width * right, Color.Black, false);
      debugRenderer.DrawLine(cusp, cusp - length * forward - width * up - width * right, Color.Black, false);
      debugRenderer.DrawLine(cusp, cusp - length * forward + width * up - width * right, Color.Black, false);
    }
  }
}
