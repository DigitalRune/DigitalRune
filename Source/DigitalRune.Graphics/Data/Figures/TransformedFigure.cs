// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a figure that is scaled, rotated, or translated in 3D space.
  /// </summary>
  /// <remarks>
  /// The <see cref="TransformedFigure"/> takes an existing figure (see property 
  /// <see cref="Child"/>) and transforms it in 3D.
  /// </remarks>
  public class TransformedFigure : Figure
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the figure that is transformed.
    /// </summary>
    /// <value>The figure that is transformed.</value>
    /// <exception cref="ArgumentNullException">
    /// The value is <see langword="null"/>.
    /// </exception>
    public Figure Child
    {
      get { return _child; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (value == _child)
          return;

        _child = value;
        Invalidate();
      }
    }
    private Figure _child;


    /// <summary>
    /// Gets or sets the pose (position and orientation) that is applied to the figure.
    /// </summary>
    /// <value>The pose (position and orientation) that is applied to the figure.</value>
    public Pose Pose
    {
      get { return _pose; }
      set
      {
        if (_pose == value)
          return;

        _pose = value;
        Invalidate();
      }
    }
    private Pose _pose;


    /// <summary>
    /// Gets or sets the scale factor that is applied to the figure.
    /// </summary>
    /// <value>The scale factor that is applied to the figure.</value>
    public Vector3F Scale
    {
      get { return _scale; }
      set
      {
        if (_scale == value)
          return;

        _scale = value;
        Invalidate();
      }
    }
    private Vector3F _scale;


    /// <inheritdoc/>
    internal override bool HasFill { get { return Child.HasFill; } }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformedFigure" /> class.
    /// </summary>
    /// <param name="child">The figure that is transformed.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="child"/> is <see langword="null"/>.
    /// </exception>
    public TransformedFigure(Figure child)
    {
      if (child == null)
        throw new ArgumentNullException("child");

      _child = child;
      Pose = Pose.Identity;
      Scale = new Vector3F(1, 1, 1);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    internal override void Flatten(ArrayList<Vector3F> vertices, ArrayList<int> strokeIndices, ArrayList<int> fillIndices)
    {
      int startIndex = vertices.Count;

      // Flatten child.
      Child.Flatten(vertices, strokeIndices, fillIndices);

      // Transform the newly added vertices.
      var vertexArray = vertices.Array;
      var numberOfVertices = vertices.Count;
      for (int i = startIndex; i < numberOfVertices; i++)
      {
        vertexArray[i].X *= Scale.X;
        vertexArray[i].Y *= Scale.Y;
        vertexArray[i].Z *= Scale.Z;

        vertexArray[i] = Pose.ToWorldPosition(vertexArray[i]);
      }
    }
    #endregion
  }
}
