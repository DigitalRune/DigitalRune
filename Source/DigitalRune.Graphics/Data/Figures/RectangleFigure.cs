// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a rectangle in the xy plane.
  /// </summary>
  public class RectangleFigure : Figure
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------    

    /// <summary>
    /// Gets or sets a value indicating whether the rectangle is filled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this rectangle is filled; otherwise, <see langword="false"/>.
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
    /// Gets or sets the width of the rectangle in the x direction.
    /// </summary>
    /// <value>The width of the rectangle in the x direction. The default value is 1.</value>
    public float WidthX
    {
      get { return _widthX; }
      set
      {
        if (_widthX == value)
          return;

        _widthX = value;
        Invalidate();
      }
    }
    private float _widthX;


    /// <summary>
    /// Gets or sets the width of the rectangle in the y direction.
    /// </summary>
    /// <value>The width of the rectangle in the y direction. The default value is 1.</value>
    public float WidthY
    {
      get { return _widthY; }
      set
      {
        if (_widthY == value)
          return;

        _widthY = value;
        Invalidate();
      }
    }
    private float _widthY = 1;


    /// <inheritdoc/>
    internal override bool HasFill { get { return IsFilled; } }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleFigure"/> class.
    /// </summary>
    public RectangleFigure()
    {
      IsFilled = true;
      WidthX = 1;
      WidthY = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    internal override void Flatten(ArrayList<Vector3F> vertices, ArrayList<int> strokeIndices, ArrayList<int> fillIndices)
    {
      int index = vertices.Count;

      float x = WidthX / 2;
      float y = WidthY / 2;

      vertices.Add(new Vector3F(-x, -y, 0));
      vertices.Add(new Vector3F(-x, y, 0));
      vertices.Add(new Vector3F(x, y, 0));
      vertices.Add(new Vector3F(x, -y, 0));

      strokeIndices.Add(index + 0);
      strokeIndices.Add(index + 1);

      strokeIndices.Add(index + 1);
      strokeIndices.Add(index + 2);

      strokeIndices.Add(index + 2);
      strokeIndices.Add(index + 3);

      strokeIndices.Add(index + 3);
      strokeIndices.Add(index + 0);

      if (IsFilled)
      {
        fillIndices.Add(index + 0);
        fillIndices.Add(index + 1);
        fillIndices.Add(index + 2);

        fillIndices.Add(index + 0);
        fillIndices.Add(index + 2);
        fillIndices.Add(index + 3);
      }
    }
    #endregion
  }
}
