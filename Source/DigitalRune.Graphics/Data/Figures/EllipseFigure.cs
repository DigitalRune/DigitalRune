// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents an ellipse in the xy plane.
  /// </summary>
  public class EllipseFigure : Figure
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly ArcSegment2F _arcSegment = new ArcSegment2F
    {
      Radius = new Vector2F(0.5f)
    };
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------    

    /// <summary>
    /// Gets or sets a value indicating whether the ellipse is filled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this ellipse is filled; otherwise, <see langword="false"/>.
    /// The default value is <see langword="true"/>.
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
    private bool _isFilled = true;


    /// <summary>
    /// Gets or sets the radius of the ellipse in the x direction.
    /// </summary>
    /// <value>
    /// The radius of the ellipse in the x direction.
    /// </value>
    public float RadiusX
    {
      get { return _arcSegment.Radius.X; }
      set
      {
        if (_arcSegment.Radius.X == value)
          return;

        _arcSegment.Radius = new Vector2F(value, _arcSegment.Radius.Y);
        Invalidate();
      }
    }


    /// <summary>
    /// Gets or sets the radius of the ellipse in the y direction.
    /// </summary>
    /// <value>
    /// The radius of the ellipse in the y direction.
    /// </value>
    public float RadiusY
    {
      get { return _arcSegment.Radius.Y; }
      set
      {
        if (_arcSegment.Radius.Y == value)
          return;

        _arcSegment.Radius = new Vector2F(_arcSegment.Radius.X, value);
        Invalidate();
      }
    }


    /// <inheritdoc/>
    internal override bool HasFill { get { return IsFilled; } }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    internal override void Flatten(Collections.ArrayList<Vector3F> vertices, Collections.ArrayList<int> strokeIndices, Collections.ArrayList<int> fillIndices)
    {
      _arcSegment.IsLargeArc = true;
      _arcSegment.Point1 = new Vector2F(RadiusX, 0);
      _arcSegment.Point2 = _arcSegment.Point1;
      _arcSegment.Radius = new Vector2F(RadiusX, RadiusY);
      _arcSegment.RotationAngle = 0;
      _arcSegment.SweepClockwise = false;

      var tempVertices = ResourcePools<Vector2F>.Lists.Obtain();
      _arcSegment.Flatten(tempVertices, MaxNumberOfIterations, Tolerance);

      int numberOfVertices = tempVertices.Count;
      if (numberOfVertices < 2)
        return;

      int startIndex = vertices.Count;

      // Add 3D vertices. We skip the duplicated vertices.
      for (int i = 0; i < numberOfVertices; i += 2)
        vertices.Add(new Vector3F(tempVertices[i].X, tempVertices[i].Y, 0));

      // Add stroke indices. 
      for (int i = 0; i < numberOfVertices - 1; i++)
        strokeIndices.Add(startIndex + (i + 1) / 2);

      // Closing stroke:
      strokeIndices.Add(startIndex);

      if (IsFilled)
      {
        // Add a center vertex.
        var centerIndex = vertices.Count;
        vertices.Add(new Vector3F(0, 0, 0));

        // Add one triangle per circle segment. 
        for (int i = 0; i < numberOfVertices / 2 - 1; i++)
        {
          fillIndices.Add(centerIndex);
          fillIndices.Add(startIndex + i + 1);
          fillIndices.Add(startIndex + i);
        }

        // Last triangle:
        fillIndices.Add(centerIndex);
        fillIndices.Add(startIndex);
        fillIndices.Add(centerIndex - 1);
      }

      ResourcePools<Vector2F>.Lists.Recycle(tempVertices);
    }
    #endregion
  }
}
