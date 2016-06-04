#region ----- Copyright -----
/*
   Teapot creation code is taken from the Primitives3D sample, which is licensed
   under the Microsoft Public License (MS-PL).
   See http://create.msdn.com/en-US/education/catalog/sample/primitives_3d

  -----------------------------------------------------------------------------
   TeapotPrimitive.cs

   Microsoft XNA Community Game Platform
   Copyright (C) Microsoft Corporation. All rights reserved.
  -----------------------------------------------------------------------------
*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// "The" teapot.
  /// </summary>
  /// <remarks>
  /// This teapot model was created by Martin Newell and Jim Blinn in 1975.
  /// It consists of ten cubic Bézier patches, a type of curved surface which
  /// can be tessellated to create triangles at various levels of detail. The
  /// use of curved surfaces allows a smoothly curved, visually interesting,
  /// and instantly recognizable shape to be specified by a tiny amount of
  /// data, which made the teapot a popular test data set for computer graphics
  /// researchers. It has been used in so many papers and demos that many
  /// graphics programmers have come to think of it as a standard geometric
  /// primitive, right up there with cubes and spheres!
  /// </remarks>
  internal sealed class Teapot
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// The teapot model consists of 10 Bézier patches. Each patch has 16 control
    /// points, plus a flag indicating whether it should be mirrored in the Z axis
    /// as well as in X (all of the teapot is symmetrical from left to right, but
    /// only some parts are symmetrical from front to back). The control points
    /// are stored as integer indices into the TeapotControlPoints array.
    /// </summary>
    private class TeapotPatch
    {
      public readonly int[] Indices;
      public readonly bool MirrorZ;

      public TeapotPatch(bool mirrorZ, int[] indices)
      {
        Debug.Assert(indices.Length == 16);

        Indices = indices;
        MirrorZ = mirrorZ;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// Static data array defines the Bézier patches that make up the teapot.
    /// </summary>
    private static readonly TeapotPatch[] TeapotPatches =
    {
      // Rim.
      new TeapotPatch(true, new[]
      {
        102, 103, 104, 105, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
      }),

      // Body.
      new TeapotPatch (true, new[]
      { 
        12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27
      }),

      new TeapotPatch(true, new[]
      { 
        24, 25, 26, 27, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40
      }),

      // Lid.
      new TeapotPatch(true, new[]
      { 
        96, 96, 96, 96, 97, 98, 99, 100, 101, 101, 101, 101, 0, 1, 2, 3
      }),
            
      new TeapotPatch(true, new[]
      {
        0, 1, 2, 3, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117
      }),

      // Handle.
      new TeapotPatch(false, new[]
      {
        41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56
      }),

      new TeapotPatch(false, new[]
      { 
        53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 28, 65, 66, 67
      }),

      // Spout.
      new TeapotPatch(false, new[]
      { 
        68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83
      }),

      new TeapotPatch(false, new[]
      { 
        80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95
      }),

      // Bottom.
      new TeapotPatch(true, new[]
      { 
        118, 118, 118, 118, 124, 122, 119, 121,
        123, 126, 125, 120, 40, 39, 38, 37
      }),
    };


    /// <summary>
    /// Static array defines the control point positions that make up the teapot.
    /// </summary>
    private static readonly Vector3[] TeapotControlPoints = 
    {
      new Vector3(0f, 0.345f, -0.05f),
      new Vector3(-0.028f, 0.345f, -0.05f),
      new Vector3(-0.05f, 0.345f, -0.028f),
      new Vector3(-0.05f, 0.345f, -0f),
      new Vector3(0f, 0.3028125f, -0.334375f),
      new Vector3(-0.18725f, 0.3028125f, -0.334375f),
      new Vector3(-0.334375f, 0.3028125f, -0.18725f),
      new Vector3(-0.334375f, 0.3028125f, -0f),
      new Vector3(0f, 0.3028125f, -0.359375f),
      new Vector3(-0.20125f, 0.3028125f, -0.359375f),
      new Vector3(-0.359375f, 0.3028125f, -0.20125f),
      new Vector3(-0.359375f, 0.3028125f, -0f),
      new Vector3(0f, 0.27f, -0.375f),
      new Vector3(-0.21f, 0.27f, -0.375f),
      new Vector3(-0.375f, 0.27f, -0.21f),
      new Vector3(-0.375f, 0.27f, -0f),
      new Vector3(0f, 0.13875f, -0.4375f),
      new Vector3(-0.245f, 0.13875f, -0.4375f),
      new Vector3(-0.4375f, 0.13875f, -0.245f),
      new Vector3(-0.4375f, 0.13875f, -0f),
      new Vector3(0f, 0.007499993f, -0.5f),
      new Vector3(-0.28f, 0.007499993f, -0.5f),
      new Vector3(-0.5f, 0.007499993f, -0.28f),
      new Vector3(-0.5f, 0.007499993f, -0f),
      new Vector3(0f, -0.105f, -0.5f),
      new Vector3(-0.28f, -0.105f, -0.5f),
      new Vector3(-0.5f, -0.105f, -0.28f),
      new Vector3(-0.5f, -0.105f, -0f),
      new Vector3(0f, -0.105f, 0.5f),
      new Vector3(0f, -0.2175f, -0.5f),
      new Vector3(-0.28f, -0.2175f, -0.5f),
      new Vector3(-0.5f, -0.2175f, -0.28f),
      new Vector3(-0.5f, -0.2175f, -0f),
      new Vector3(0f, -0.27375f, -0.375f),
      new Vector3(-0.21f, -0.27375f, -0.375f),
      new Vector3(-0.375f, -0.27375f, -0.21f),
      new Vector3(-0.375f, -0.27375f, -0f),
      new Vector3(0f, -0.2925f, -0.375f),
      new Vector3(-0.21f, -0.2925f, -0.375f),
      new Vector3(-0.375f, -0.2925f, -0.21f),
      new Vector3(-0.375f, -0.2925f, -0f),
      new Vector3(0f, 0.17625f, 0.4f),
      new Vector3(-0.075f, 0.17625f, 0.4f),
      new Vector3(-0.075f, 0.2325f, 0.375f),
      new Vector3(0f, 0.2325f, 0.375f),
      new Vector3(0f, 0.17625f, 0.575f),
      new Vector3(-0.075f, 0.17625f, 0.575f),
      new Vector3(-0.075f, 0.2325f, 0.625f),
      new Vector3(0f, 0.2325f, 0.625f),
      new Vector3(0f, 0.17625f, 0.675f),
      new Vector3(-0.075f, 0.17625f, 0.675f),
      new Vector3(-0.075f, 0.2325f, 0.75f),
      new Vector3(0f, 0.2325f, 0.75f),
      new Vector3(0f, 0.12f, 0.675f),
      new Vector3(-0.075f, 0.12f, 0.675f),
      new Vector3(-0.075f, 0.12f, 0.75f),
      new Vector3(0f, 0.12f, 0.75f),
      new Vector3(0f, 0.06375f, 0.675f),
      new Vector3(-0.075f, 0.06375f, 0.675f),
      new Vector3(-0.075f, 0.007499993f, 0.75f),
      new Vector3(0f, 0.007499993f, 0.75f),
      new Vector3(0f, -0.04875001f, 0.625f),
      new Vector3(-0.075f, -0.04875001f, 0.625f),
      new Vector3(-0.075f, -0.09562501f, 0.6625f),
      new Vector3(0f, -0.09562501f, 0.6625f),
      new Vector3(-0.075f, -0.105f, 0.5f),
      new Vector3(-0.075f, -0.18f, 0.475f),
      new Vector3(0f, -0.18f, 0.475f),
      new Vector3(0f, 0.02624997f, -0.425f),
      new Vector3(-0.165f, 0.02624997f, -0.425f),
      new Vector3(-0.165f, -0.18f, -0.425f),
      new Vector3(0f, -0.18f, -0.425f),
      new Vector3(0f, 0.02624997f, -0.65f),
      new Vector3(-0.165f, 0.02624997f, -0.65f),
      new Vector3(-0.165f, -0.12375f, -0.775f),
      new Vector3(0f, -0.12375f, -0.775f),
      new Vector3(0f, 0.195f, -0.575f),
      new Vector3(-0.0625f, 0.195f, -0.575f),
      new Vector3(-0.0625f, 0.17625f, -0.6f),
      new Vector3(0f, 0.17625f, -0.6f),
      new Vector3(0f, 0.27f, -0.675f),
      new Vector3(-0.0625f, 0.27f, -0.675f),
      new Vector3(-0.0625f, 0.27f, -0.825f),
      new Vector3(0f, 0.27f, -0.825f),
      new Vector3(0f, 0.28875f, -0.7f),
      new Vector3(-0.0625f, 0.28875f, -0.7f),
      new Vector3(-0.0625f, 0.2934375f, -0.88125f),
      new Vector3(0f, 0.2934375f, -0.88125f),
      new Vector3(0f, 0.28875f, -0.725f),
      new Vector3(-0.0375f, 0.28875f, -0.725f),
      new Vector3(-0.0375f, 0.298125f, -0.8625f),
      new Vector3(0f, 0.298125f, -0.8625f),
      new Vector3(0f, 0.27f, -0.7f),
      new Vector3(-0.0375f, 0.27f, -0.7f),
      new Vector3(-0.0375f, 0.27f, -0.8f),
      new Vector3(0f, 0.27f, -0.8f),
      new Vector3(0f, 0.4575f, -0f),
      new Vector3(0f, 0.4575f, -0.2f),
      new Vector3(-0.1125f, 0.4575f, -0.2f),
      new Vector3(-0.2f, 0.4575f, -0.1125f),
      new Vector3(-0.2f, 0.4575f, -0f),
      new Vector3(0f, 0.3825f, -0f),
      new Vector3(0f, 0.27f, -0.35f),
      new Vector3(-0.196f, 0.27f, -0.35f),
      new Vector3(-0.35f, 0.27f, -0.196f),
      new Vector3(-0.35f, 0.27f, -0f),
      new Vector3(0f, 0.3075f, -0.1f),
      new Vector3(-0.056f, 0.3075f, -0.1f),
      new Vector3(-0.1f, 0.3075f, -0.056f),
      new Vector3(-0.1f, 0.3075f, -0f),
      new Vector3(0f, 0.3075f, -0.325f),
      new Vector3(-0.182f, 0.3075f, -0.325f),
      new Vector3(-0.325f, 0.3075f, -0.182f),
      new Vector3(-0.325f, 0.3075f, -0f),
      new Vector3(0f, 0.27f, -0.325f),
      new Vector3(-0.182f, 0.27f, -0.325f),
      new Vector3(-0.325f, 0.27f, -0.182f),
      new Vector3(-0.325f, 0.27f, -0f),
      new Vector3(0f, -0.33f, -0f),
      new Vector3(-0.1995f, -0.33f, -0.35625f),
      new Vector3(0f, -0.31125f, -0.375f),
      new Vector3(0f, -0.33f, -0.35625f),
      new Vector3(-0.35625f, -0.33f, -0.1995f),
      new Vector3(-0.375f, -0.31125f, -0f),
      new Vector3(-0.35625f, -0.33f, -0f),
      new Vector3(-0.21f, -0.31125f, -0.375f),
      new Vector3(-0.375f, -0.31125f, -0.21f),
    };
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<VertexPositionNormal> _vertices;
    private readonly List<ushort> _indices;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Queries the index of the current vertex. This starts at zero, and increments every time 
    /// a vertex is added.
    /// </summary>
    private int CurrentVertex
    {
      get { return _vertices.Count; }
    }


    /// <summary>
    /// Gets the vertices of the teapot.
    /// </summary>
    /// <value>The vertices of the teapot.</value>
    public VertexPositionNormal[] Vertices { get; private set; }


    /// <summary>
    /// Gets the indices of the teapot.
    /// </summary>
    /// <value>The indices of the teapot.</value>
    public ushort[] Indices { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Teapot"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Teapot"/> class.
    /// </summary>
    public Teapot()
      : this(1, 8)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Teapot"/> class using the specified settings.
    /// </summary>
    /// <param name="size">The size of the teapot.</param>
    /// <param name="tessellation">The tessellation level [1, 18].</param>
    /// <remarks>
    /// The maximum tessellation level when using a 16-bit index buffer is 18!
    /// </remarks>
    public Teapot(float size, int tessellation)
    {
      if (tessellation < 1)
        throw new ArgumentOutOfRangeException("tessellation");

      // Allocate enough space for default settings (tessellation = 8) to avoid
      // unnecessary memory re-allocations.
      _vertices = new List<VertexPositionNormal>(2592);
      _indices = new List<ushort>(12288);

      foreach (TeapotPatch patch in TeapotPatches)
      {
        // Because the teapot is symmetrical from left to right, we only store
        // data for one side, then tessellate each patch twice, mirroring in X.
        TessellatePatch(patch, tessellation, new Vector3(size, size, size));
        TessellatePatch(patch, tessellation, new Vector3(-size, size, size));

        if (patch.MirrorZ)
        {
          // Some parts of the teapot (the body, lid, and rim, but not the
          // handle or spout) are also symmetrical from front to back, so
          // we tessellate them four times, mirroring in Z as well as X.
          TessellatePatch(patch, tessellation, new Vector3(size, size, -size));
          TessellatePatch(patch, tessellation, new Vector3(-size, size, -size));
        }
      }

      Vertices = _vertices.ToArray();
      Indices = _indices.ToArray();
      _vertices = null;
      _indices = null;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Tessellates the specified Bézier patch.
    /// </summary>
    private void TessellatePatch(TeapotPatch patch, int tessellation, Vector3 scale)
    {
      // Look up the 16 control points for this patch.
      Vector3[] controlPoints = new Vector3[16];

      for (int i = 0; i < 16; i++)
      {
        int index = patch.Indices[i];
        controlPoints[i] = TeapotControlPoints[index] * scale;
      }

      // Is this patch being mirrored?
      bool isMirrored = Math.Sign(scale.X) != Math.Sign(scale.Z);

      // Create the index and vertex data.
      CreatePatchIndices(tessellation, isMirrored);
      CreatePatchVertices(controlPoints, tessellation, isMirrored);
    }


    /// <summary>
    /// Creates indices for a patch that is tessellated at the specified level.
    /// </summary>
    private void CreatePatchIndices(int tessellation, bool isMirrored)
    {
      int stride = tessellation + 1;

      for (int i = 0; i < tessellation; i++)
      {
        for (int j = 0; j < tessellation; j++)
        {
          // Make a list of six index values (two triangles).
          int[] indices =
          {
              i * stride + j,
              (i + 1) * stride + j,
              (i + 1) * stride + j + 1,

              i * stride + j,
              (i + 1) * stride + j + 1,
              i * stride + j + 1,
          };

          // If this patch is mirrored, reverse the
          // indices to keep the correct winding order.
          if (isMirrored)
          {
            Array.Reverse(indices);
          }

          // Create the indices.
          foreach (int index in indices)
          {
            _indices.Add((ushort)(CurrentVertex + index));
          }
        }
      }
    }


    /// <summary>
    /// Creates vertices for a patch that is tessellated at the specified level.
    /// </summary>
    private void CreatePatchVertices(Vector3[] patch, int tessellation, bool isMirrored)
    {
      Debug.Assert(patch.Length == 16);

      for (int i = 0; i <= tessellation; i++)
      {
        float ti = (float)i / tessellation;

        for (int j = 0; j <= tessellation; j++)
        {
          float tj = (float)j / tessellation;

          // Perform four horizontal Bézier interpolations
          // between the control points of this patch.
          Vector3 p1 = Bezier(patch[0], patch[1], patch[2], patch[3], ti);
          Vector3 p2 = Bezier(patch[4], patch[5], patch[6], patch[7], ti);
          Vector3 p3 = Bezier(patch[8], patch[9], patch[10], patch[11], ti);
          Vector3 p4 = Bezier(patch[12], patch[13], patch[14], patch[15], ti);

          // Perform a vertical interpolation between the results of the
          // previous horizontal interpolations, to compute the position.
          Vector3 position = Bezier(p1, p2, p3, p4, tj);

          // Perform another four Bézier interpolations between the control
          // points, but this time vertically rather than horizontally.
          Vector3 q1 = Bezier(patch[0], patch[4], patch[8], patch[12], tj);
          Vector3 q2 = Bezier(patch[1], patch[5], patch[9], patch[13], tj);
          Vector3 q3 = Bezier(patch[2], patch[6], patch[10], patch[14], tj);
          Vector3 q4 = Bezier(patch[3], patch[7], patch[11], patch[15], tj);

          // Compute vertical and horizontal tangent vectors.
          Vector3 tangentA = BezierTangent(p1, p2, p3, p4, tj);
          Vector3 tangentB = BezierTangent(q1, q2, q3, q4, ti);

          // Cross the two tangent vectors to compute the normal.
          Vector3 normal = Vector3.Cross(tangentA, tangentB);

          if (normal.Length() > 0.0001f)
          {
            normal.Normalize();

            // If this patch is mirrored, we must invert the normal.
            if (isMirrored)
              normal = -normal;
          }
          else
          {
            // In a tidy and well constructed Bézier patch, the preceding
            // normal computation will always work. But the classic teapot
            // model is not tidy or well constructed! At the top and bottom
            // of the teapot, it contains degenerate geometry where a patch
            // has several control points in the same place, which causes
            // the tangent computation to fail and produce a zero normal.
            // We 'fix' these cases by just hard-coding a normal that points
            // either straight up or straight down, depending on whether we
            // are on the top or bottom of the teapot. This is not a robust
            // solution for all possible degenerate Bézier patches, but hey,
            // it's good enough to make the teapot work correctly!

            if (position.Y > 0)
              normal = Vector3.Up;
            else
              normal = Vector3.Down;
          }

          // Create the vertex.
          _vertices.Add(new VertexPositionNormal(position, normal));
        }
      }
    }


    /// <summary>
    /// Performs a cubic Bézier interpolation between four scalar control
    /// points, returning the value at the specified time (t ranges 0 to 1).
    /// </summary>
    private static float Bezier(float p1, float p2, float p3, float p4, float t)
    {
      return p1 * (1 - t) * (1 - t) * (1 - t) +
             p2 * 3 * t * (1 - t) * (1 - t) +
             p3 * 3 * t * t * (1 - t) +
             p4 * t * t * t;
    }


    /// <summary>
    /// Performs a cubic Bézier interpolation between four Vector3 control
    /// points, returning the value at the specified time (t ranges 0 to 1).
    /// </summary>
    private static Vector3 Bezier(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
    {
      Vector3 result = new Vector3();

      result.X = Bezier(p1.X, p2.X, p3.X, p4.X, t);
      result.Y = Bezier(p1.Y, p2.Y, p3.Y, p4.Y, t);
      result.Z = Bezier(p1.Z, p2.Z, p3.Z, p4.Z, t);

      return result;
    }


    /// <summary>
    /// Computes the tangent of a cubic Bézier curve at the specified time,
    /// when given four scalar control points.
    /// </summary>
    private static float BezierTangent(float p1, float p2, float p3, float p4, float t)
    {
      return p1 * (-1 + 2 * t - t * t) +
             p2 * (1 - 4 * t + 3 * t * t) +
             p3 * (2 * t - 3 * t * t) +
             p4 * (t * t);
    }


    /// <summary>
    /// Computes the tangent of a cubic Bézier curve at the specified time,
    /// when given four Vector3 control points. This is used for calculating
    /// normals (by crossing the horizontal and vertical tangent vectors).
    /// </summary>
    private static Vector3 BezierTangent(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
    {
      Vector3 result = new Vector3();

      result.X = BezierTangent(p1.X, p2.X, p3.X, p4.X, t);
      result.Y = BezierTangent(p1.Y, p2.Y, p3.Y, p4.Y, t);
      result.Z = BezierTangent(p1.Z, p2.Z, p3.Z, p4.Z, t);

      result.Normalize();

      return result;
    }
    #endregion
  }
}
