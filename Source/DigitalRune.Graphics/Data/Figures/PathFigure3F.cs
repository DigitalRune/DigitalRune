// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a 3D figure composed of lines and curves.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="PathFigure3F"/> can be used to define data for line rendering. Lines are 
  /// defined using curve segments (see property <see cref="Segments"/>). A curve segment is any
  /// object that implements <see cref="ICurve{TParam,TPoint}">ICurve&lt;float, Vector3F&gt;</see>. 
  /// Examples: <see cref="LineSegment3F"/>, <see cref="BezierSegment3F"/>, <see cref="Path3F"/>, 
  /// etc. 
  /// </para>
  /// <para>
  /// In contrast to a <see cref="PathFigure2F"/>, the <see cref="PathFigure3F"/> cannot be filled.
  /// </para>
  /// </remarks>
  public class PathFigure3F : Figure
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------    
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    internal override bool HasFill { get { return false; } }


    /// <summary>
    /// Gets the curve segments.
    /// </summary>
    /// <value>The curve segments.</value>
    /// <remarks>
    /// Curve segments need to be added to the <see cref="Segments"/> collection. A curve segment is 
    /// any object that implements <see cref="ICurve{TParam,TPoint}">ICurve&lt;float, Vector3F&gt;</see>. 
    /// Examples: <see cref="LineSegment3F"/>, <see cref="BezierSegment3F"/>, <see cref="Path3F"/>, 
    /// etc. 
    /// </remarks>
    public PathSegment3FCollection Segments { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PathFigure3F"/> class.
    /// </summary>
    public PathFigure3F()
    {
      Segments = new PathSegment3FCollection(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    internal override void Flatten(ArrayList<Vector3F> vertices, ArrayList<int> strokeIndices, ArrayList<int> fillIndices)
    {
      var segmentVertices = ResourcePools<Vector3F>.Lists.Obtain();

      Vector3F previousVertex = new Vector3F(float.NaN);
      foreach (var curve in Segments)
      {
        if (!IsStroked(curve))
          continue;

        segmentVertices.Clear();
        curve.Flatten(segmentVertices, MaxNumberOfIterations, Tolerance);

        // Flatten should always return an even number of vertices. But if Flatten() 
        // is erroneous we simply ignore the last vertex.
        int vertexCount = segmentVertices.Count;
        if (vertexCount % 2 != 0)
          vertexCount--;

        if (vertexCount < 2)
          continue;

        Vector3F startVertex = segmentVertices[0];
        bool isNewShape;
        int segmentStartIndex;  // Start of current segment in 'vertices'.
        if (startVertex == previousVertex)
        {
          // Continuation of current shape: Skip first vertex of segment.
          isNewShape = false;
          segmentStartIndex = vertices.Count - 1;
        }
        else
        {
          // Start of new shape: Previous segments are no longer needed.
          isNewShape = true;
          segmentStartIndex = vertices.Count;
        }

        CommitVertices(segmentVertices, isNewShape, vertices);

        // Apply stroke.
        int lineCount = vertexCount / 2;
        Stroke(segmentStartIndex, lineCount, strokeIndices);

        previousVertex = vertices.Array[vertices.Count - 1];
      }

      ResourcePools<Vector3F>.Lists.Recycle(segmentVertices);
    }


    // Adds vertices from segmentVertices to vertices.
    private static void CommitVertices(List<Vector3F> segmentVertices, bool isNewShape, ArrayList<Vector3F> vertices)
    {
      if (isNewShape)
      {
        // Add start vertex.
        vertices.Add(segmentVertices[0]);
      }

      // Add end vertices.
      int numberOfVertices = segmentVertices.Count;
      for (int i = 1; i < numberOfVertices; i += 2)
        vertices.Add(segmentVertices[i]);
    }


    private static bool IsStroked(ICurve<float, Vector3F> segment)
    {
      var strokedSegment = segment as StrokedSegment3F;
      return strokedSegment == null || strokedSegment.IsStroked;
    }


    // Adds indices to strokeIndices.
    private static void Stroke(int startIndex, int lineCount, ArrayList<int> strokeIndices)
    {
      for (int i = startIndex; i < startIndex + lineCount; i++)
      {
        strokeIndices.Add(i);
        strokeIndices.Add(i + 1);
      }
    }
    #endregion
  }
}
