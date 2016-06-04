// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Represents a curve.
  /// </summary>
  /// <typeparam name="TParam">
  /// The type of the curve parameter (usually <see cref="float"/> or <see cref="double"/>).
  /// </typeparam>
  /// <typeparam name="TPoint">
  /// The type of the curve points (such as <see cref="Vector2F"/>, <see cref="Vector3F"/>, etc.).
  /// </typeparam>
  /// <remarks>
  /// <para>
  /// Curves can be used to describe animation curves, 2D paths, 3D paths, and more.
  /// </para>
  /// <para>
  /// Mathematically, a curve is a function of the form <i>point = C(parameter)</i>. The <i>curve 
  /// parameter</i> is a scalar. The result of <i>C(parameter)</i> is a <i>point</i> on the curve.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public interface ICurve<TParam, TPoint>
  {
    /// <summary>
    /// Computes a point on the curve.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>
    /// The curve point. (If the curve does not contain any points a vector with NaN values is
    /// returned.)
    /// </returns>
    TPoint GetPoint(TParam parameter);


    /// <summary>
    /// Computes the tangent for a point on the curve.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>The curve tangent.</returns>
    /// <remarks>
    /// This method computes the curve tangent (also known as <i>slope</i> or <i>velocity</i>) at
    /// the curve position determined by <paramref name="parameter"/>.
    /// </remarks>
    TPoint GetTangent(TParam parameter);


    /// <summary>
    /// Computes the approximated length of the curve for the parameter interval 
    /// [<paramref name="start"/>, <paramref name="end"/>].
    /// </summary>
    /// <param name="start">The parameter value of the start position.</param>
    /// <param name="end">The parameter value of the end position.</param>
    /// <param name="maxNumberOfIterations">
    /// The maximum number of iterations which are taken to compute the length.
    /// </param>
    /// <param name="tolerance">
    /// The tolerance value. This method will return an approximation of the precise length. 
    /// The absolute error will be less than this tolerance. 
    /// </param>
    /// <returns>
    /// The approximated length of the curve for the given parameter interval. 
    /// </returns>
    /// <remarks>
    /// For some curves the length is computed with an iterative algorithm. The iterations end when 
    /// the <paramref name="maxNumberOfIterations"/> were performed, or when the 
    /// <paramref name="tolerance"/> criterion is met - whichever comes first.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
    TParam GetLength(TParam start, TParam end, int maxNumberOfIterations, TParam tolerance);


    /// <overloads>
    /// <summary>
    /// Computes the points of a sequence of line segments which approximate the curve.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Computes the points of a sequence of line segments which approximate the curve.
    /// </summary>
    /// <param name="points">
    /// A collection to which all points of the line segments are added. See remarks.
    /// </param>
    /// <param name="maxNumberOfIterations">
    /// The maximum number of iterations which are taken to compute the approximation.
    /// </param>
    /// <param name="tolerance">
    /// The tolerance value. The absolute error of the approximated polygon will be less than this 
    /// tolerance. 
    /// </param>
    /// <remarks>
    /// <para>
    /// This method computes a sequence of line segments which approximates the curve. For each line
    /// segment, the start and end point are added to <paramref name="points"/>. For example, if a 
    /// curve is approximated with two line segments (A, B) and (B, C) where A, B, C are three key 
    /// points, then following points will be added to collection: A, B, B, C. This means, that 
    /// duplicate points are added to the collection. The advantage of this is that the 
    /// approximation can represent "gaps" in the curve. And it is easy to flatten several curves 
    /// into the same <paramref name="points"/> collection.
    /// </para>
    /// <para>
    /// For some curves the approximation is computed with an iterative algorithm. The iterations 
    /// end when the <paramref name="maxNumberOfIterations"/> were performed, or when the 
    /// <paramref name="tolerance"/> criterion is met - whichever comes first.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="tolerance"/> is 0 or less than 0.
    /// </exception>
    void Flatten(ICollection<TPoint> points, int maxNumberOfIterations, TParam tolerance);


    ///// <summary>
    ///// Computes the points of a sequence of line segments which approximate the curve for 
    ///// the parameter interval [<paramref name="start"/>, <paramref name="end"/>].
    ///// </summary>
    ///// <param name="points">
    ///// A collection to which all points of the line segments are added. See remarks.
    ///// </param>
    ///// <param name="start">The parameter value of the start position.</param>
    ///// <param name="end">The parameter value of the end position.</param>
    ///// <param name="maxNumberOfIterations">
    ///// The maximum number of iterations which are taken to compute the approximation.
    ///// </param>
    ///// <param name="tolerance">
    ///// The tolerance value. The absolute error of the approximated polygon will be less than this 
    ///// tolerance. 
    ///// </param>
    ///// <inheritdoc cref="Flatten(ICollection{TPoint},int,float)"/>
    ///// <exception cref="ArgumentOutOfRangeException">
    ///// <paramref name="tolerance"/> is 0 or less than 0.
    ///// </exception>
    //void Flatten(ICollection<TPoint> points, TParam start, TParam end, int maxNumberOfIterations, TParam tolerance);
  }
}
