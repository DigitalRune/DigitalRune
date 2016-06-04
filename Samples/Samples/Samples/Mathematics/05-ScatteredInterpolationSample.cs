using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;

// Both XNA and DigitalRune have a class called MathHelper. To avoid compiler errors
// we need to define which MathHelper we want to use.
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Mathematics
{
  [Sample(SampleCategory.Mathematics,
    @"This example shows how to use scattered interpolation to compute a height field.",
    @"What is Scattered Interpolation?
Scattered interpolation can be used when you have several observed data pairs:
Given an input vector X you have observed the output vector Y.
You have several such observation pairs: (X1, Y1), (X2, Y2), ...
Now you want to compute the output vector Y for a new input vector X.
Scattered interpolation takes the observed data pairs as input and computes the
output vector Y for any input vector X.

In this example the X values are 2D vectors and the Y are height values.
Several random 2D vectors with random heights are generated.
Scattered interpolation is used to compute a complete height field from the random points.",
    5)]
  public class ScatteredInterpolationSample : BasicSample
  {
    public ScatteredInterpolationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      SetCamera(new Vector3F(0, 3, 5), 0, -0.5f);

      DoScatteredInterpolation();
    }


    private void DoScatteredInterpolation()
    {
      // Create random points for a height field.
      // The height field is in the x/z plane. Height is along the y axis.
      var points = new List<Vector3F>();
      for (int i = 0; i < 9; i++)
      {
        float x = i * 10;
        float y = RandomHelper.Random.NextFloat(0, 20);
        float z = RandomHelper.Random.NextInteger(0, 44) * 2;

        points.Add(new Vector3F(x, y, z));
      }

      // Now we setup scattered interpolation.
      // The RadialBasisRegression class does one form of scattered interpolation:
      // Multiple regression analysis with radial basis functions.
      RadialBasisRegressionF rbf = new RadialBasisRegressionF();

      // We must define a basis function which will be used to determine the influence
      // of each input data pair. The Gaussian bell curve is a good candidate for most
      // applications.
      // We set the standard deviation to 10. Choosing a higher standard deviation makes
      // the Gaussian bell curve wider and increases the influence of the data points.
      // If we choose a lower standard deviation, we limit the influence of the data points.
      rbf.BasisFunction = (x, i) => MathHelper.Gaussian(x, 1, 0, 10);

      // Feed the data points into the scattered interpolation instance.
      foreach (var point in points)
      {
        // For each random point we add a data pair (X, Y), where X is a 2D position
        // in the height field, and Y is the observed height.
        VectorF X = new VectorF(new[] { point.X, point.Z });
        VectorF Y = new VectorF(new[] { point.Y });
        var dataPair = new Pair<VectorF, VectorF>(X, Y);
        rbf.Add(dataPair);
      }

      // These were all data points. Now, we perform some precomputations.
      rbf.Setup();

      // Finally, we create a height field.
      var heightField = new float[100, 100];
      for (int x = 0; x < 100; x++)
      {
        for (int z = 0; z < 100; z++)
        {
          // The scattered interpolation instance can compute a height for 
          // any 2D input vector.
          float y = rbf.Compute(new VectorF(new float[] { x, z }))[0];
          heightField[x, z] = y;
        }
      }

      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Draw the random data points.
      const float scale = 0.04f;
      Vector3F offset = new Vector3F(-2, 0, -2);
      foreach (var point in points)
        debugRenderer.DrawPoint(scale * point + offset, Color.Black, false);

      // Draw the height field.
      const int stepSize = 2;
      for (int x = 0; x < 100; x += stepSize)
      {
        for (int z = 0; z < 100; z += stepSize)
        {
          float y0 = heightField[x, z];

          if (x + stepSize < 100)
          {
            float y1 = heightField[x + stepSize, z];
            debugRenderer.DrawLine(
              scale * new Vector3F(x, y0, z) + offset,
              scale * new Vector3F(x + stepSize, y1, z) + offset,
              Color.Black,
              false);
          }

          if (z + stepSize < 100)
          {
            float y2 = heightField[x, z + stepSize];
            debugRenderer.DrawLine(
              scale * new Vector3F(x, y0, z) + offset,
              scale * new Vector3F(x, y2, z + stepSize) + offset,
              Color.Black,
              false);
          }
        }
      }
    }
  }
}
