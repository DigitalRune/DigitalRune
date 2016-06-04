// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a drawing composed of lines and shapes rendered with a certain stroke and fill.
  /// </summary>
  /// <remarks>
  /// <para>
  /// 2D shapes can be rendered using an outline and filled interior. The stroke properties (see 
  /// <see cref="StrokeColor"/>, <see cref="StrokeAlpha"/>, <see cref="StrokeThickness"/> and 
  /// <see cref="StrokeDashPattern"/>) define the line style used for the outline. If 
  /// <see cref="StrokeAlpha"/> or the <see cref="StrokeThickness"/> is 0, no outlines are rendered.
  /// The fill properties (see <see cref="FillColor"/> and <see cref="FillAlpha"/>) define the fill 
  /// color. If <see cref="FillAlpha"/> is 0, the interior of the figure is not filled. Some shapes,
  /// like simple line segments, do not have an interior which can be filled and the fill properties
  /// are ignored.
  /// </para>
  /// <para>
  /// 3D lines, even closed 3D curves, do not have an interior and are also not filled.
  /// </para>
  /// <para>
  /// If pose, scale, figure and other properties are constant, it is recommended to set the 
  /// <see cref="SceneNode.IsStatic"/> flag to <see langword="true"/>. If this flag is set, the 
  /// figure renderer can perform optimizations.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="FigureNode"/> is cloned the <see cref="Figure"/> 
  /// is not duplicated. The <see cref="Figure"/> is copied by reference (shallow copy). The
  /// original <see cref="FigureNode"/> and the cloned instance will reference the same 
  /// <see cref="Figure"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="DigitalRune.Graphics.Figure"/>
  public class FigureNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the drawing.
    /// </summary>
    /// <value>The drawing.</value>
    /// <remarks>
    /// See <see cref="DigitalRune.Graphics.Figure"/> for more information.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Figure Figure
    {
      get { return _figure; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _figure = value;
        Shape = value.BoundingShape;
        RenderData.SafeDispose();
      }
    }
    private Figure _figure;


    /// <summary>
    /// Gets or sets the stroke color.
    /// </summary>
    /// <value>The stroke color (non-premultiplied). The default value is white (1, 1, 1).</value>
    public Vector3F StrokeColor
    {
      get { return _strokeColor; }
      set
      {
        if (_strokeColor == value)
          return;

        _strokeColor = value;
        RenderData.SafeDispose();
      }
    }
    private Vector3F _strokeColor;


    /// <summary>
    /// Gets or sets the opacity of the stroked line.
    /// </summary>
    /// <value>The opacity of the stroked line. The default value is 1 (opaque).</value>
    /// <remarks>
    /// If this value is 0, no lines are not drawn.
    /// </remarks>
    public float StrokeAlpha
    {
      get { return _strokeAlpha; }
      set
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_strokeAlpha == value)
          return;

        _strokeAlpha = value;
        RenderData.SafeDispose();
      }
    }
    private float _strokeAlpha;


    /// <summary>
    /// Gets or sets the stroke thickness.
    /// </summary>
    /// <value>The stroke thickness.</value>
    /// <remarks>
    /// If this value is 0, no lines are drawn.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value is negative.
    /// </exception>
    public float StrokeThickness
    {
      get { return _strokeThickness; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The thickness must not be negative.");
        
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_strokeThickness == value)
          return;

        _strokeThickness = value;
        RenderData.SafeDispose();
      }
    }
    private float _strokeThickness;


    /// <summary>
    /// Gets or sets the stroke dash pattern.
    /// </summary>
    /// <value>
    /// The dash pattern. The default value is (1, 0, 0, 0), which creates a solid line.
    /// </value>
    /// <remarks>
    /// <para>
    /// The dash pattern is defined as a 4 component vector: (dash, gap, dash, gap). The length of a
    /// dash or a gap is relative to the <see cref="StrokeThickness"/>. For example, (5, 1, 1, 1)
    /// defines a dash pattern with a 5 unit dash, a 1 unit gap, a 1 unit dash followed by a 1 unit
    /// gap, then the pattern is repeated.
    /// </para>
    /// <para>
    /// To render a solid line, set the gap components to 0, e.g. (1, 0, 0, 0).
    /// </para>
    /// <para>
    /// The property <see cref="DashInWorldSpace"/> determines if the dash/gap size is in pixels or
    /// in world space units and whether perspective foreshortening is applied.
    /// </para>
    /// </remarks>
    public Vector4F StrokeDashPattern
    {
      get { return _strokeDashPattern; }
      set
      {
        if (_strokeDashPattern == value)
          return;

        _strokeDashPattern = value;
        RenderData.SafeDispose();
      }
    }
    private Vector4F _strokeDashPattern;


    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="StrokeDashPattern"/> is computed in
    /// world space or in screen space.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the dash pattern is computed in world space; otherwise, 
    /// <see langword="false" /> to compute the dash pattern in screen space.
    /// </value>
    /// <remarks>
    /// This property is only relevant if the line uses a <see cref="StrokeDashPattern"/>.
    /// <para>
    /// If this value is <see langword="false"/>, the dash and gap size is computed in screen space.
    /// That means, the dash/gap size is given in pixels, and the dash size does not depend on the
    /// distance from the viewer.
    /// </para>
    /// <para>
    /// If this value is <see langword="true"/>, the dash and gap size is computed in world space.
    /// That means, the dash/gap size is relative to world space units, and the dash size is
    /// foreshortened: A dash near the camera is longer than a dash in the distance.
    /// </para>
    /// </remarks>
    public bool DashInWorldSpace
    {
      get { return _dashInWorldSpace; }
      set
      {
        if (_dashInWorldSpace == value)
          return;

        _dashInWorldSpace = value;
        RenderData.SafeDispose();
      }
    }
    private bool _dashInWorldSpace;


    /// <summary>
    /// Gets or sets the fill color.
    /// </summary>
    /// <value>The fill color (non-premultiplied). The default value is white (1, 1, 1).</value>
    public Vector3F FillColor
    {
      get { return _fillColor; }
      set
      {
        if (_fillColor == value)
          return;

        _fillColor = value;
        RenderData.SafeDispose();
      }
    }
    private Vector3F _fillColor;


    /// <summary>
    /// Gets or sets the fill opacity.
    /// </summary>
    /// <value>The fill opacity. The default value is 1 (opaque).</value>
    /// <remarks>
    /// If this value is 0, the figure is not filled.
    /// </remarks>
    public float FillAlpha
    {
      get { return _fillAlpha; }
      set
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_fillAlpha == value)
          return;

        _fillAlpha = value;
        RenderData.SafeDispose();
      }
    }
    private float _fillAlpha;


    /// <summary>
    /// Gets or sets the draw order.
    /// </summary>
    /// <value>
    /// The draw order. The value must be in the range [0, 65535]. The default value is 0.
    /// </value>
    /// <remarks>
    /// This property defines the order in which <see cref="FigureNode"/>s should be drawn.
    /// Nodes with a lower <see cref="DrawOrder"/> should be drawn first. Nodes with a higher
    /// <see cref="DrawOrder"/> should be drawn last. This means that usually nodes with a higher
    /// draw order cover nodes with a lower draw order.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is out of range.
    /// </exception>
    public int DrawOrder
    {
      get { return _drawOrder; }
      set
      {
        if (_drawOrder < 0 || _drawOrder > ushort.MaxValue)
          throw new ArgumentOutOfRangeException("value", "The draw order must be in the range [0, 65535].");

        _drawOrder = value;
      }
    }
    private int _drawOrder;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FigureNode" /> class.
    /// </summary>
    /// <param name="figure">The figure.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="figure"/> is <see langword="null"/>.
    /// </exception>
    public FigureNode(Figure figure)
    {
      if (figure == null)
        throw new ArgumentNullException("figure");

      IsRenderable = true;
      _figure = figure;
      Shape = figure.BoundingShape;

      StrokeColor = new Vector3F(1, 1, 1);
      StrokeAlpha = 1;
      StrokeThickness = 1;
      StrokeDashPattern = new Vector4F(1, 0, 0, 0);
      DashInWorldSpace = true;
      FillColor = new Vector3F(1, 1, 1);
      FillAlpha = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new FigureNode Clone()
    {
      return (FigureNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new FigureNode(Figure);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone FigureNode properties.
      var sourceTyped = (FigureNode)source;
      StrokeColor = sourceTyped.StrokeColor;
      StrokeAlpha = sourceTyped.StrokeAlpha;
      StrokeThickness = sourceTyped.StrokeThickness;
      StrokeDashPattern = sourceTyped.StrokeDashPattern;
      DashInWorldSpace = sourceTyped.DashInWorldSpace;
      FillColor = sourceTyped.FillColor;
      FillAlpha = sourceTyped.FillAlpha;
      DrawOrder = sourceTyped.DrawOrder;
    }
    #endregion

    #endregion
  }
}
