using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"This sample shows how to use DigitalRune Geometry to compute a convex hull and a 
tight-fitting oriented bounding box for any random point set.",
    @"",
    3)]
  public class ConvexHullSample : BasicSample
  {
    public ConvexHullSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3F(0, 1, 10), 0, 0);

      // Generate random points.
      var points = new List<Vector3F>();
      for (int i = 0; i < 100; i++)
        points.Add(RandomHelper.Random.NextVector3F(-1, 1));

      // Apply random transformation to points to make this sample more interesting.
      Matrix44F transform = new Matrix44F(
        Matrix33F.CreateRotation(RandomHelper.Random.NextQuaternionF()) * Matrix33F.CreateScale(RandomHelper.Random.NextVector3F(0.1f, 2f)),
        RandomHelper.Random.NextVector3F(-1, 1));

      for (int i = 0; i < points.Count; i++)
        points[i] = transform.TransformPosition(points[i]);

      // Compute convex hull. The result is the mesh of the hull represented as a
      // Doubly-Connected Edge List (DCEL).
      DcelMesh convexHull = GeometryHelper.CreateConvexHull(points);

      // We don't need the DCEL representation. Let's store the hull as a simpler triangle mesh.
      TriangleMesh convexHullMesh = convexHull.ToTriangleMesh();

      // Compute a tight-fitting oriented bounding box.
      Vector3F boundingBoxExtent;   // The bounding box dimensions (widths in X, Y and Z).
      Pose boundingBoxPose;         // The pose (world space position and orientation) of the bounding box.
      GeometryHelper.ComputeBoundingBox(points, out boundingBoxExtent, out boundingBoxPose);
      // (Note: The GeometryHelper also contains methods to compute a bounding sphere.)

      var debugRenderer = GraphicsScreen.DebugRenderer;
      foreach (var point in points)
        debugRenderer.DrawPoint(point, Color.White, true);

      debugRenderer.DrawShape(new TriangleMeshShape(convexHullMesh), Pose.Identity, Vector3F.One, Color.Violet, false, false);
      debugRenderer.DrawBox(boundingBoxExtent.X, boundingBoxExtent.Y, boundingBoxExtent.Z, boundingBoxPose, Color.Red, true, false);
    }
  }
}
