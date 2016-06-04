using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;


namespace Samples.Mathematics
{
  [Sample(SampleCategory.Mathematics,
    @"This example shows how to perform a Principal Components Analysis (PCA).",
    @"Several random 2D vectors are create. Then the principal components of these 
points are computed.",
    7)]
  public class PcaSample : BasicSample
  {
    public PcaSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;
      DoPca();
    }


    private void DoPca()
    {
      // Create random 2-dimensional vectors.
      var points = new List<VectorF>();
      for (int i = 0; i < 20; i++)
      {
        var x = RandomHelper.Random.NextFloat(-200, 200);
        var y = RandomHelper.Random.NextFloat(-100, 100);
        var randomVector = new VectorF(new float[] { x, y });
        points.Add(randomVector);
      }

      // Draw a small cross for each point.
      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      debugRenderer.Clear();
      var center = new Vector3F(640, 360, 0);
      foreach (var point in points)
      {
        var x = point[0];
        var y = point[1];
        debugRenderer.DrawLine(
          center + new Vector3F(x - 10, y - 10, 0),
          center + new Vector3F(x + 10, y + 10, 0),
          Color.Black,
          true);
        debugRenderer.DrawLine(
          center + new Vector3F(x + 10, y - 10, 0),
          center + new Vector3F(x - 10, y + 10, 0),
          Color.Black,
          true);
      }

      // Compute the average of the points.
      var average = new Vector2F();
      foreach (var point in points)
        average += (Vector2F)point;

      average /= points.Count;

      // Compute the PCA of the point set.
      var pca = new PrincipalComponentAnalysisF(points);

      // Get the principal components.
      // pca.V is a matrix where each column represents a principal component.
      // The first column represents the first principal component, which can be loosely 
      // interpreted as the "direction of the widest spread".
      Vector2F pc0 = (Vector2F)pca.V.GetColumn(0);
      // The second column represents the second principal component, which is a vector
      // orthogonal to the first.
      Vector2F pc1 = (Vector2F)pca.V.GetColumn(1);

      // pca.Variances contains the variances of the data points along the principal components.
      // The square root of the variance is the standard deviation.
      float standardDeviation0 = (float)Math.Sqrt(pca.Variances[0]);
      float standardDeviation1 = (float)Math.Sqrt(pca.Variances[1]);

      // Draw a line in the direction of the first principal component through the average point.
      // The line length is proportional to the standard deviation with an arbitrary scaling.
      debugRenderer.DrawLine(
         center + new Vector3F(average.X, average.Y, 0) - 3 * standardDeviation0 * new Vector3F(pc0.X, pc0.Y, 0),
         center + new Vector3F(average.X, average.Y, 0) + 3 * standardDeviation0 * new Vector3F(pc0.X, pc0.Y, 0),
         Color.Black,
         true);

      // Draw a line in the direction of the second principal component through the average point.
      debugRenderer.DrawLine(
         center + new Vector3F(average.X, average.Y, 0) - 3 * standardDeviation1 * new Vector3F(pc1.X, pc1.Y, 0),
         center + new Vector3F(average.X, average.Y, 0) + 3 * standardDeviation1 * new Vector3F(pc1.X, pc1.Y, 0),
         Color.Black,
         true);
    }
  }
}
