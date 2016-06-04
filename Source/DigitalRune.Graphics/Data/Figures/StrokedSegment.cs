// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Wraps a curve segment and determines whether it is stroked or not.
  /// </summary>
  /// <typeparam name="TParam">
  /// The type of the curve parameter (usually <see cref="float"/> or <see cref="double"/>).
  /// </typeparam>
  /// <typeparam name="TPoint">
  /// The type of the curve points (such as <see cref="Vector2F"/>, <see cref="Vector3F"/>, etc.).
  /// </typeparam>
  /// <remarks>
  /// Curve segments within a <see cref="PathFigure2F"/> are stroked by default. The 
  /// <see cref="StrokedSegment{TParam,TPoint}"/> is a decorator that wraps another curve and adds
  /// an annotation that defines whether the curve is stroke or not.
  /// </remarks>
  /// <example>
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
  /// </example>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Param")]
  public class StrokedSegment<TParam, TPoint> : ICurve<TParam, TPoint>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether this curve segment is stroked.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this curve segment is stroked; otherwise, <see langword="false"/>.
    /// The default value is <see langword="true"/>.
    /// </value>
    public bool IsStroked { get; set; }


    /// <summary>
    /// Gets or sets the curve.
    /// </summary>
    /// <value>The curve.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public ICurve<TParam, TPoint> Curve
    {
      get { return _curve; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _curve = value;
      }
    }
    private ICurve<TParam, TPoint> _curve;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="StrokedSegment{TParam,TPoint}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="StrokedSegment{TParam,TPoint}"/> class with the
    /// specified stroked curve.
    /// </summary>
    /// <param name="curve">The curve.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="curve"/> is <see langword="null"/>.
    /// </exception>
    public StrokedSegment(ICurve<TParam, TPoint> curve)
      : this(curve, true)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="StrokedSegment{TParam,TPoint}"/> class.
    /// </summary>
    /// <param name="curve">The curve.</param>
    /// <param name="isStroked">
    /// <see langword="true"/> if this curve segment is stroked; otherwise, <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="curve"/> is <see langword="null"/>.
    /// </exception>
    public StrokedSegment(ICurve<TParam, TPoint> curve, bool isStroked)
    {
      if (curve == null)
        throw new ArgumentNullException("curve");

      _curve = curve;
      IsStroked = isStroked;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public TPoint GetPoint(TParam parameter)
    {
      return Curve.GetPoint(parameter);
    }


    /// <inheritdoc/>
    public TPoint GetTangent(TParam parameter)
    {
      return Curve.GetTangent(parameter);
    }


    /// <inheritdoc/>
    public TParam GetLength(TParam start, TParam end, int maxNumberOfIterations, TParam tolerance)
    {
      return Curve.GetLength(start, end, maxNumberOfIterations, tolerance);
    }


    /// <inheritdoc/>
    public void Flatten(ICollection<TPoint> points, int maxNumberOfIterations, TParam tolerance)
    {
      Curve.Flatten(points, maxNumberOfIterations, tolerance);
    }
    #endregion
  }


  /// <summary>
  /// Wraps a 2D curve segment (single-precision) and determines whether it is stroked or not.
  /// </summary>
  /// <inheritdoc/>
  public class StrokedSegment2F : StrokedSegment<float, Vector2F>
  {
    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="StrokedSegment2F"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="StrokedSegment2F"/> class with the specified 
    /// stroked curve.
    /// </summary>
    /// <param name="curve">The curve.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="curve"/> is <see langword="null"/>.
    /// </exception>
    public StrokedSegment2F(ICurve<float, Vector2F> curve)
      : base(curve)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="StrokedSegment2F"/> class.
    /// </summary>
    /// <param name="curve">The curve.</param>
    /// <param name="isStroked">
    /// <see langword="true"/> if this curve segment is stroked; otherwise, <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="curve"/> is <see langword="null"/>.
    /// </exception>
    public StrokedSegment2F(ICurve<float, Vector2F> curve, bool isStroked)
      : base(curve, isStroked)
    {
    }
  }


  /// <summary>
  /// Wraps a 3D curve segment (single-precision) and determines whether it is stroked or not.
  /// </summary>
  /// <inheritdoc/>
  public class StrokedSegment3F : StrokedSegment<float, Vector3F>
  {
    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="StrokedSegment3F"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="StrokedSegment3F"/> class with the specified 
    /// stroked curve.
    /// </summary>
    /// <param name="curve">The curve.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="curve"/> is <see langword="null"/>.
    /// </exception>
    public StrokedSegment3F(ICurve<float, Vector3F> curve)
      : base(curve)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="StrokedSegment3F"/> class.
    /// </summary>
    /// <param name="curve">The curve.</param>
    /// <param name="isStroked">
    /// <see langword="true"/> if this curve segment is stroked; otherwise, <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="curve"/> is <see langword="null"/>.
    /// </exception>
    public StrokedSegment3F(ICurve<float, Vector3F> curve, bool isStroked)
      : base(curve, isStroked)
    {
    }
  }
}
