// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Contains the flattened representation of a figure, which is used for rendering and hit 
  /// testing.
  /// </summary>
  /// <remarks>
  /// The <see cref="FigureRenderData"/> has a <see cref="BoundingShape"/> which can be used for
  /// culling and a <see cref="HitShape"/> for accurate hit testing. (The 
  /// <see cref="FigureRenderData"/> implements <see cref="ITriangleMesh"/>, which is used for the
  /// <see cref="HitShape"/>. Filled areas are represented as triangles and stroked lines are 
  /// represented as degenerate triangles.)
  /// </remarks>
  internal class FigureRenderData : ITriangleMesh
  {
    /// <summary>The points of the flattened figure.</summary>
    public ArrayList<Vector3F> Vertices;

    /// <summary>The indices of the fill areas (triangle list).</summary>
    public ArrayList<int> FillIndices;

    /// <summary>The indices of the stroke (line list).</summary>
    public ArrayList<int> StrokeIndices;


    /// <summary>
    /// Initializes a new instance of the <see cref="FigureRenderData"/> class.
    /// </summary>
    public FigureRenderData()
    {
      BoundingShape = new TransformedShape
      {
        Child = new GeometricObject(new BoxShape(new Vector3F(Single.MaxValue)))
      };

      // The HitShape is created on demand.
    }


    /// <summary>
    /// Resets the render data.
    /// </summary>
    public void Reset()
    {
      // Clear vertex and index data.
      if (Vertices != null)
        Vertices.Clear();
      if (StrokeIndices != null)
        StrokeIndices.Clear();
      if (FillIndices != null)
        FillIndices.Clear();

      ResetBoundingShape();
      ResetHitShape();
    }


    public void Update(Figure figure)
    {
      if (Vertices != null)
        Vertices.Clear();
      else
        Vertices = new ArrayList<Vector3F>(2);

      if (StrokeIndices != null)
        StrokeIndices.Clear();
      else
        StrokeIndices = new ArrayList<int>(2);

      if (FillIndices != null)
        FillIndices.Clear();
      else if (figure.HasFill)
        FillIndices = new ArrayList<int>(3);

      figure.Flatten(Vertices, StrokeIndices, FillIndices);

      UpdateBoundingShape();
      UpdateHitShape();
    }



    //--------------------------------------------------------------
    #region Bounding Shape
    //--------------------------------------------------------------

    /// <summary>The bounding shape.</summary> 
    public TransformedShape BoundingShape;


    private void ResetBoundingShape()
    {
      // Set bounding shape to infinity.
      ((BoxShape)BoundingShape.Child.Shape).Extent = new Vector3F(Single.MaxValue);
    }


    private void UpdateBoundingShape()
    {
      // ----- Update BoundingShape
      // Compute a transformed shape with a box from the minimal AABB.
      var boxObject = (GeometricObject)BoundingShape.Child;
      var boxShape = (BoxShape)boxObject.Shape;
      var vertexArray = (Vertices != null) ? Vertices.Array : null;
      if (vertexArray != null && vertexArray.Length > 0)
      {
        Aabb aabb = new Aabb(vertexArray[0], vertexArray[0]);
        var numberOfVertices = vertexArray.Length;
        for (int i = 1; i < numberOfVertices; i++)
          aabb.Grow(vertexArray[i]);

        // Update existing shape.
        boxObject.Pose = new Pose(aabb.Center);
        boxShape.Extent = aabb.Extent;
      }
      else
      {
        // Set an "empty" shape.
        boxObject.Pose = Pose.Identity;
        boxShape.Extent = Vector3F.Zero;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Hit Shape
    //--------------------------------------------------------------

    // The triangle mesh shape for hit testing.
    private int _numberOfStrokes;
    private int _numberOfFillTriangles;


    /// <summary>
    /// Gets the hit shape.
    /// </summary>
    /// <value>The hit shape.</value>
    public TriangleMeshShape HitShape
    {
      get
      {
        if (_hitShape == null)
        {
          _hitShape = new TriangleMeshShape(this) { IsTwoSided = true };
          UpdateHitShape();
        }

        return _hitShape;
      }
    }
    private TriangleMeshShape _hitShape;


    private void ResetHitShape()
    {
      if (_hitShape != null)
      {
        _numberOfStrokes = 0;
        _numberOfFillTriangles = 0;
        _hitShape.Invalidate();
      }
    }


    private void UpdateHitShape()
    {
      if (_hitShape != null)
      {
        int numberOfStrokes = (StrokeIndices != null) ? StrokeIndices.Count / 2 : 0;
        int numberOfFillTriangles = (FillIndices != null) ? FillIndices.Count / 3 : 0;
        int numberOfHitTriangles = numberOfStrokes + numberOfFillTriangles;

        if (_hitShape.Partition == null && numberOfHitTriangles > 32)
        {
          // Create spatial partition for efficiency.
          _numberOfStrokes = 0;       // "Clear" the triangle mesh before setting the partition,
          _numberOfFillTriangles = 0; // otherwise the partition is filled twice!

          _hitShape.Partition = new CompressedAabbTree
          {
            BottomUpBuildThreshold = 0,
          };
        }

        // Invalidate shape.
        _numberOfStrokes = numberOfStrokes;
        _numberOfFillTriangles = numberOfFillTriangles;
        _hitShape.Invalidate();
      }
    }


    #region ----- ITriangleMesh -----

    /// <inheritdoc/>
    int ITriangleMesh.NumberOfTriangles
    {
      get { return _numberOfStrokes + _numberOfFillTriangles; }
    }


    /// <inheritdoc/>
    Triangle ITriangleMesh.GetTriangle(int index)
    {
      var vertices = Vertices.Array;
      if (index < _numberOfStrokes)
      {
        var indices = StrokeIndices.Array;
        return new Triangle(
          vertices[indices[index * 2 + 0]],
          vertices[indices[index * 2 + 0]],
          vertices[indices[index * 2 + 1]]);
      }

      index -= _numberOfStrokes;
      if (index < _numberOfFillTriangles)
      {
        var indices = FillIndices.Array;
        return new Triangle(
          vertices[indices[index * 3 + 0]],
          vertices[indices[index * 3 + 1]],
          vertices[indices[index * 3 + 2]]);
      }

      throw new ArgumentOutOfRangeException("index");
    }
    #endregion

    #endregion
  }
}
