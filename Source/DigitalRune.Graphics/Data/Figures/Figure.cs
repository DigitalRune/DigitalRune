// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a drawing composed of lines and 2D shapes.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <i>figure</i> is a drawing composed of lines and shapes.
  /// </para>
  /// <para>
  /// A figure may contain smooth curves such as Bézier splines. These smooth curves are "flattened"
  /// for rendering, which means they are approximated using line segments. The properties 
  /// <see cref="MaxNumberOfIterations"/> and <see cref="Tolerance"/> determine how detailed the 
  /// approximations will be. See also <see cref="ICurve{TParam,TPoint}.Flatten"/>.
  /// </para>
  /// <para>
  /// The figure has <see cref="BoundingShape"/> which can be used for culling and a 
  /// <see cref="HitShape"/> which can be used for more accurate hit testing ("picking").
  /// </para>
  /// </remarks>
  public abstract class Figure
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isDirty = true;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    internal FigureRenderData RenderData
    {
      get
      {
        UpdateRenderData();
        return _renderData;
      }
    }
    private FigureRenderData _renderData;


    /// <summary>
    /// Gets or sets the maximum number of iterations which are taken when flattening smooth curves.
    /// </summary>
    /// <value>
    /// The maximum number of iterations which are taken when flattening smooth curves. The default
    /// value is 10.
    /// </value>
    /// <remarks>
    /// Changing <see cref="MaxNumberOfIterations"/> or <see cref="Tolerance"/> does not 
    /// automatically redraw the existing figures with the new settings. The figures need to be 
    /// invalidated (see method <see cref="Invalidate"/>) to force a redraw.
    /// </remarks>
    public static int MaxNumberOfIterations
    {
      get { return _maxNumberOfIterations; }
      set { _maxNumberOfIterations = value; }
    }
    private static int _maxNumberOfIterations = 10;


    /// <summary>
    /// Gets or sets the tolerance which is allowed when flattening smooth curves.
    /// </summary>
    /// <value>
    /// The tolerance which is allowed when flattening smooth curves. The default value is 0.01.
    /// </value>
    /// <inheritdoc cref="MaxNumberOfIterations"/>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value is zero or less than zero.
    /// </exception>
    public static float Tolerance
    {
      get { return _tolerance; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "The tolerance must be greater than zero.");

        _tolerance = value;
      }
    }
    private static float _tolerance = 0.01f;


    /// <summary>
    /// Gets a value indicating whether this figure or any part of it is filled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if figure has a fill; otherwise, <see langword="false"/>.
    /// </value>
    internal abstract bool HasFill { get; }


    /// <summary>
    /// Gets the collision shape for bounding volume tests and culling.
    /// </summary>
    /// <value>The collision shape for bounding volume tests and culling.</value>
    /// <inheritdoc cref="HitShape"/>
    public Shape BoundingShape
    {
      get { return RenderData.BoundingShape; }
    }


    /// <summary>
    /// Gets the collision shape for hit tests.
    /// </summary>
    /// <value>The collision shape for hit tests.</value>
    /// <remarks>
    /// <para>
    /// The <see cref="BoundingShape"/> is a very simply shape which can be used for simple bounding
    /// volume tests and culling (e.g. view frustum culling). The <see cref="HitShape"/> is a
    /// detailed shape which can be used for more accurate hit testing.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The <see cref="HitShape"/> was designed for hit testing 
    /// ("picking") to determine whether the mouse cursor or another object intersects the rendered
    /// figure. The representation of the figure is updated during rendering. That means, the 
    /// <see cref="HitShape"/> may be invalid when it is not rendered!
    /// </para>
    /// </remarks>
    public Shape HitShape
    {
      get { return RenderData.HitShape; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Invalidates cached render data. (Must be called when properties of a figure are changed.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Renderers may cache the data they need to render the figure. When properties of a figure
    /// already in the collection are changed, (e.g. a key point of a curve is moved or new points
    /// are added to a path), then <see cref="Invalidate"/> must be called manually to inform the
    /// renderer that cached data may be invalid.
    /// </para>
    /// <para>
    /// When a figure is part of another figure (composite or transformed figures), then 
    /// <see cref="Invalidate"/> needs to be called on the root figure because the render data is
    /// usually cached in the root figure.
    /// </para>
    /// </remarks>
    public void Invalidate()
    {
      if (_isDirty)
        return;

      _isDirty = true;

      if (_renderData != null)
        _renderData.Reset();
    }


    private void UpdateRenderData()
    {
      if (!_isDirty)
        return;

      _isDirty = false;

      if (_renderData == null)
        _renderData = new FigureRenderData();

      _renderData.Update(this);
    }


    /// <summary>
    /// Flattens the specified vertices.
    /// </summary>
    /// <param name="vertices">The vertices.</param>
    /// <param name="strokeIndices">The line indices (2 indices per line).</param>
    /// <param name="fillIndices">The triangle indices (3 indices per triangle).</param>
    internal abstract void Flatten(ArrayList<Vector3F> vertices, ArrayList<int> strokeIndices, ArrayList<int> fillIndices);
    #endregion
  }
}
