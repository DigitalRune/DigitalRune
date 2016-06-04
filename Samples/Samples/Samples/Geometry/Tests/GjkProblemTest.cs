#if WINDOWS
using System;
using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    "",
    "",
    1000)]
  [Controls(@"Sample
  Use numpad to move one object.")]
  public class GjkProblemTest : BasicSample
  {
    private CollisionObject _part1A;
    private CollisionObject _part1B;
    private CollisionObject _part2;


    public GjkProblemTest(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      SetCamera(new Vector3F(0, 1, 10), 0, 0);

      var points1a = new List<Vector3F>
      {
                new Vector3F(0.0f, 0.0f, -0.1875f),
                new Vector3F(0.0f, 0.0f, 0.1875f),
                new Vector3F(10.0f, 0.0f, -0.1875f),
                new Vector3F(10.0f, 0.0f, 0.1875f),
                new Vector3F(10.0f, 5.0f, -0.1875f),
                new Vector3F(10.0f, 5.0f, 0.1875f),
                new Vector3F(0.0f, 5.0f, -0.1875f),
                new Vector3F(0.0f, 5.0f, 0.1875f)
            };

      var points1b = new List<Vector3F>
      {
                new Vector3F(0.0f, 0.0f, -0.1875f),
                new Vector3F(10.0f, 0.0f, -0.1875f),
                new Vector3F(10.0f, 5.0f, -0.1875f),
                new Vector3F(0.0f, 5.0f, -0.1875f),
                new Vector3F(0.0f, 0.0f, 0.1875f),
                new Vector3F(10.0f, 0.0f, 0.1875f),
                new Vector3F(10.0f, 5.0f, 0.1875f),
                new Vector3F(0.0f, 5.0f, 0.1875f)
            };

      var matrix1 = new Matrix44F(0.0f, 1.0f, 0.0f, 208.5f, -1.0f, 0.0f, 0.0f, 10.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f);

      _part1A = new CollisionObject(new GeometricObject(new ConvexPolyhedron(points1a), Pose.FromMatrix(matrix1)));
      _part1B = new CollisionObject(new GeometricObject(new ConvexPolyhedron(points1b), Pose.FromMatrix(matrix1)));

      var points2 = new List<Vector3F>
      {
                new Vector3F(0.0f, 0.0f, -0.375f),
                new Vector3F(0.0f, 0.0f, 0.375f),
                new Vector3F(23.0f, 0.0f, -0.375f),
                new Vector3F(23.0f, 0.0f, 0.375f),
                new Vector3F(23.0f, 10.0f, -0.375f),
                new Vector3F(23.0f, 10.0f, 0.375f),
                new Vector3F(0.0f, 10.0f, -0.375f),
                new Vector3F(0.0f, 10.0f, 0.375f)
            };

      var matrix2 = new Matrix44F(0.0f, 0.0f, -1.0f, 208.125f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 5.0f, 0.0f, 0.0f, 0.0f, 1.0f);

      _part2 = new CollisionObject(new GeometricObject(new ConvexPolyhedron(points2), Pose.FromMatrix(matrix2)));

    }


    public override void Update(GameTime gameTime)
    {
      var debugRenderer = GraphicsScreen.DebugRenderer;

      var cd = new CollisionDetection();


      float sizeB = _part2.GeometricObject.Aabb.Extent.Length;
      var perturbationEpsilon = sizeB * Math.Max(0.001f / 10, Numeric.EpsilonF * 10);
      var perturbationAngle = perturbationEpsilon / sizeB;

      Vector3F v;
      v = new Vector3F(1, 0, 0);

      var translation = v * perturbationEpsilon;
      var rotation = Matrix33F.CreateRotation(v, perturbationAngle);

      var poseB = _part2.GeometricObject.Pose;
      var origPose = poseB;
      poseB.Position = translation + poseB.Position;
      poseB.Orientation = rotation * poseB.Orientation;
      //((GeometricObject)_part2.GeometricObject).Pose = poseB;


      var cp1 = cd.GetClosestPoints(_part1A, _part2);
      //var cp2 = cd.GetClosestPoints(_part1B, _part2);

      ((GeometricObject)_part2.GeometricObject).Pose = origPose;

      debugRenderer.Clear();
      debugRenderer.DrawObject(_part1A.GeometricObject, Color.DarkGreen, true, false);
      //debugRenderer.DrawObject(_part1B.GeometricObject, Color.DarkBlue, false, false);
      debugRenderer.DrawObject(_part2.GeometricObject, Color.DarkViolet, true, false);

      debugRenderer.DrawContact(cp1[0], 0.1f, Color.Yellow, true);
      //debugRenderer.DrawContact(cp2[0], 0.1f, Color.Pink, true);
    }
  }
}
#endif