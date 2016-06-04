// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a 2D figure composed of lines and curves.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="PathFigure2F"/> can be used to define complex shapes composed of lines and 
  /// curves. The figure is 2D and lies in the xy plane where x-axis points to the right and y-axis
  /// points upwards. The curve segments need to be added to the <see cref="Segments"/> collection. 
  /// A curve segment is any object that implements 
  /// <see cref="ICurve{TParam,TPoint}">ICurve&lt;float, Vector2F&gt;</see>. Examples: 
  /// <see cref="LineSegment2F"/>, <see cref="BezierSegment2F"/>, <see cref="Path2F"/>, ...
  /// </para>
  /// <para>
  /// <strong>Fill:</strong><br/>
  /// Closed shapes within the figure can be filled: A closed shape is defined by consecutive, 
  /// connected curve segments. For example: Four <see cref="LineSegment2F"/> can be used to create 
  /// a filled rectangle. The end of the previous segment needs to match the start of the next
  /// segment. The last segment of a closed shape needs to be connected with the first segment.
  /// </para>
  /// <para>
  /// The property <see cref="IsFilled"/> determines whether closed shapes will be filled. (The 
  /// property is <see langword="true"/> by default.)
  /// </para>
  /// <para>
  /// <strong>Stroked vs. Unstroked Segments:</strong><br/>
  /// By default all curve segments are stroked meaning that the curve segment will be rendered with
  /// a given stroke color and thickness. The <see cref="StrokedSegment2F"/> can be used to add a 
  /// curve segment which is not stroked. This class is a decorator that wraps a curve segment and 
  /// allows to define whether it is stroked or not.
  /// </para>
  /// <para>
  /// The following example creates rectangles where all or only some edges are stroked:
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Box where all edges are stroked. (Curve segments are stroked by default.)
  /// var boxFigure1 = new PathFigure2F
  /// {
  ///   Segments =
  ///   {
  ///     new LineSegment2F { Point1 = new Vector2F(0, 0), Point2 = new Vector2F(0, 1) },
  ///     new LineSegment2F { Point1 = new Vector2F(0, 1), Point2 = new Vector2F(1, 1) },
  ///     new LineSegment2F { Point1 = new Vector2F(1, 1), Point2 = new Vector2F(1, 0) },
  ///     new LineSegment2F { Point1 = new Vector2F(1, 0), Point2 = new Vector2F(0, 0) }
  ///   }
  /// };
  /// var figureNode1 = new FigureNode(boxFigure1)
  /// {
  ///   StrokeColor = new Vector3F(0, 0, 0),
  ///   StrokeThickness = 2,
  ///   FillColor = new Vector3F(0.5f, 0.5f, 0.5f)
  /// };
  /// 
  /// // Box where top and bottom edges are stroked.
  /// var boxFigure2 = new PathFigure2F
  /// {
  ///   Segments =
  ///   {
  ///     new StrokedSegment2F(
  ///       new LineSegment2F { Point1 = new Vector2F(0, 0), Point2 = new Vector2F(0, 1) }, 
  ///       false),
  ///     new LineSegment2F { Point1 = new Vector2F(0, 1), Point2 = new Vector2F(1, 1) },
  ///     new StrokedSegment2F(
  ///       new LineSegment2F { Point1 = new Vector2F(1, 1), Point2 = new Vector2F(1, 0) }, 
  ///       false),
  ///     new LineSegment2F { Point1 = new Vector2F(1, 0), Point2 = new Vector2F(0, 0) }
  ///   }
  /// };
  /// var figureNode2 = new FigureNode(boxFigure2)
  /// {
  ///   StrokeColor = new Vector3F(0, 0, 0),
  ///   StrokeThickness = 2,
  ///   FillColor = new Vector3F(0.5f, 0.5f, 0.5f)
  /// };
  /// ]]>
  /// </code>
  /// </remarks>
  public class PathFigure2F : Figure
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------    

    /// <inheritdoc/>
    internal override bool HasFill { get { return IsFilled; } }


    /// <summary>
    /// Gets or sets a value indicating whether the interior of the figure is filled or empty.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the interior of the figure is filled; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    public bool IsFilled
    {
      get { return _isFilled; }
      set
      {
        if (_isFilled == value)
          return;

        _isFilled = value;
        Invalidate();
      }
    }
    private bool _isFilled;


    /// <summary>
    /// Gets the curve segments.
    /// </summary>
    /// <value>The curve segments.</value>
    /// <remarks>
    /// Curve segments need to be added to the <see cref="Segments"/> collection. A curve segment is 
    /// any object that implements <see cref="ICurve{TParam,TPoint}">ICurve&lt;float, Vector2F&gt;</see>. 
    /// Examples: <see cref="LineSegment2F"/>, <see cref="BezierSegment2F"/>, <see cref="Path2F"/>, 
    /// etc. 
    /// </remarks>
    public PathSegment2FCollection Segments { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PathFigure2F"/> class.
    /// </summary>
    public PathFigure2F()
    {
      Segments = new PathSegment2FCollection(this);
      _isFilled = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    internal override void Flatten(ArrayList<Vector3F> vertices, ArrayList<int> strokeIndices, ArrayList<int> fillIndices)
    {
      Triangulator triangulator = null;
      var segmentVertices = ResourcePools<Vector2F>.Lists.Obtain();
      var segmentStartIndices = ResourcePools<int>.Lists.Obtain();

      Vector2F previousVertex = new Vector2F(float.NaN);
      foreach (var curve in Segments)
      {
        segmentVertices.Clear();
        curve.Flatten(segmentVertices, MaxNumberOfIterations, Tolerance);

        // Flatten should always return an even number of vertices. But if Flatten() 
        // is erroneous we simply ignore the last vertex.
        int vertexCount = segmentVertices.Count;
        if (vertexCount % 2 != 0)
          vertexCount--;

        if (vertexCount < 2)
          continue;

        Vector2F segmentStartVertex = segmentVertices[0];
        bool isNewShape;
        int segmentStartIndex;  // Start of current segment in 'vertices'.
        if (segmentStartVertex == previousVertex)
        {
          // Continuation of current shape: Skip first vertex of segment.
          isNewShape = false;
          segmentStartIndex = vertices.Count - 1;
        }
        else
        {
          // Start of new shape: Previous segments are no longer needed.
          isNewShape = true;
          segmentStartIndices.Clear();
          segmentStartIndex = vertices.Count;
        }

        // Check for closed shapes.
        Vector2F segmentEndVertex = segmentVertices[segmentVertices.Count - 1];
        bool isClosedShape = false;
        int shapeStartIndex = -1;

        // Check if current segment is closed.
        if (vertexCount > 2 && segmentStartVertex == segmentEndVertex)
        {
          // Closed shape found.
          isClosedShape = true;
          shapeStartIndex = segmentStartIndex;
        }

        // Check if last n segments are closed.
        if (!isClosedShape)
        {
          int numberOfSegments = segmentStartIndices.Count;
          var vertexArray = vertices.Array;
          Vector3F v = new Vector3F(segmentEndVertex.X, segmentEndVertex.Y, 0);
          for (int i = 0; i < numberOfSegments; i++)
          {
            int startIndex = segmentStartIndices[i];
            if (v == vertexArray[startIndex])
            {
              // Closed shape found.
              isClosedShape = true;
              shapeStartIndex = startIndex;
              break;
            }
          }
        }

        if (isClosedShape)
          segmentStartIndices.Clear();
        else
          segmentStartIndices.Add(segmentStartIndex);

        CommitVertices(segmentVertices, isNewShape, isClosedShape, vertices);

        if (IsStroked(curve))
        {
          // Apply stroke.
          int lineCount = vertexCount / 2;
          Stroke(segmentStartIndex, lineCount, isClosedShape, shapeStartIndex, strokeIndices);
        }

        if (isClosedShape && IsFilled)
        {
          // Apply fill.
          Fill(vertices, shapeStartIndex, fillIndices, ref triangulator);
        }

        previousVertex.X = segmentEndVertex.X;
        previousVertex.Y = segmentEndVertex.Y;
      }

      ResourcePools<Vector2F>.Lists.Recycle(segmentVertices);
      ResourcePools<int>.Lists.Recycle(segmentStartIndices);
      if (triangulator != null)
        triangulator.Recycle();
    }


    private static void CommitVertices(List<Vector2F> segmentVertices, bool isNewShape, bool isClosedShape, ArrayList<Vector3F> vertices)
    {
      if (isNewShape)
      {
        // Add start vertex.
        Vector2F v = segmentVertices[0];
        vertices.Add(new Vector3F(v.X, v.Y, 0));
      }

      int numberOfVertices = segmentVertices.Count;
      if (isClosedShape)
      {
        // Ignore last vertex.
        numberOfVertices--;
      }

      // Add end vertices.
      for (int i = 1; i < numberOfVertices; i += 2)
      {
        Vector2F v = segmentVertices[i];
        vertices.Add(new Vector3F(v.X, v.Y, 0));
      }
    }


    private static bool IsStroked(ICurve<float, Vector2F> segment)
    {
      var strokedSegment = segment as StrokedSegment2F;
      return strokedSegment == null || strokedSegment.IsStroked;
    }


    private static void Stroke(int startIndex, int lineCount, bool isClosedShape, int shapeStartIndex, ArrayList<int> strokeIndices)
    {
      for (int i = startIndex; i < startIndex + lineCount - 1; i++)
      {
        strokeIndices.Add(i);
        strokeIndices.Add(i + 1);
      }

      // Last line.
      strokeIndices.Add(startIndex + lineCount - 1);
      strokeIndices.Add(isClosedShape ? shapeStartIndex : startIndex + lineCount);
    }


    private static void Fill(ArrayList<Vector3F> vertices, int startIndex, ArrayList<int> fillIndices, ref Triangulator triangulator)
    {
      if (triangulator == null)
        triangulator = Triangulator.Create();

      triangulator.Triangulate(vertices, startIndex, vertices.Count - startIndex, fillIndices);
    }
    #endregion
  }
}
