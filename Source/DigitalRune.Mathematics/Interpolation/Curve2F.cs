// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Represents a 2-dimensional curve that is used to define a function <i>y = f(x)</i> 
  /// (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// In contrast to general curves, the <see cref="Curve2F"/> is a specialized type of curve that 
  /// is used to define a function <i>y = f(x)</i>.
  /// </para>
  /// <para>
  /// A general curve is function of the form <i><strong>p</strong> = C(u)</i>. The curve parameter 
  /// <i>u</i> is a scalar. The result of <i>C(u)</i> is a point <i><strong>p</strong></i> on the 
  /// curve. In the 2-dimensional case (see <see cref="Path2F"/>) the function can be written as 
  /// <i>(x, y) = C(u)</i>.
  /// </para>
  /// <para>
  /// The <see cref="Curve2F"/> is a special type of curve where the curve parameter and 
  /// the x-component of the resulting points are identical: <i>(x, y) = C(x)</i>.
  /// </para>
  /// <para>
  /// <strong>Application:</strong> A <see cref="Curve2F"/> can be used to define an animation 
  /// curve. An animation curve describes how a quantity y (size, offset, etc.) evolves over time. 
  /// the curve parameter is typically <i>time</i>. These curves must be monotonic in the parameter 
  /// axis (x-axis). There can only be one valid y value for each curve parameter x.
  /// </para>
  /// <para>
  /// <strong>Curve Keys:</strong> 
  /// The curve keys (<see cref="CurveKey2F"/>) define the control points of the curve. The 
  /// parameter of the curve keys (see <see cref="CurveKey{TParam,TValue}.Parameter"/>) are always 
  /// identical to the x-component of the points (see <see cref="CurveKey{TParam,TValue}.Point"/>). 
  /// Therefore, it is not necessary to set the curve parameter manually.
  /// </para>
  /// <para>
  /// The curve keys are interpolated using spline interpolation. Each curve key defines the type
  /// of spline interpolation that is used from this curve key to the next (see 
  /// <see cref="CurveKey{TParam,TValue}.Interpolation"/>). Some types of interpolation require 
  /// additional information: <see cref="SplineInterpolation.Bezier"/> requires additional 
  /// control points and <see cref="SplineInterpolation.Hermite"/> requires tangents. These values
  /// are stored in the properties <see cref="CurveKey{TParam,TValue}.TangentIn"/> and 
  /// <see cref="CurveKey{TParam,TValue}.TangentOut"/> of the curve keys.
  /// </para>
  /// <para>
  /// It is possible to specify invalid control points or tangents. Neither <see cref="Curve2F"/> 
  /// nor <see cref="CurveKey2F"/> checks whether the values in 
  /// <see cref="CurveKey{TParam,TValue}.TangentIn"/> or <see cref="CurveKey{TParam,TValue}.TangentOut"/>
  /// are a valid. In general, the results are undefined if a curve contains invalid tangents.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public class Curve2F : PiecewiseCurveF<Vector2F, CurveKey2F>, IXmlSerializable
  {
    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    public override Vector2F GetPoint(float parameter)
    {
      int numberOfKeys = Count;
      if (numberOfKeys == 0)
        return new Vector2F(float.NaN, float.NaN);
      
      float interpolatedX = parameter; // The x result must be equal to the parameter.

      CurveKey2F firstKey = Items[0];
      CurveKey2F lastKey = Items[numberOfKeys - 1];
      float curveStart = firstKey.Parameter;
      float curveEnd = lastKey.Parameter;

      // Correct parameter.
      float loopedParameter = LoopParameter(parameter);
      ICurve<float, float> xSpline, ySpline;

      #region ----- Handle Linear Pre- and PostLoops -----
      // If parameter is outside: Use spline tangent. Exceptions are Bézier and Hermite
      // were the exact tangent is given in the outer keys.
      if (loopedParameter < curveStart)
      {
        Debug.Assert(PreLoop == CurveLoopType.Linear);

        // Get tangent.
        Vector2F tangent;
        switch (firstKey.Interpolation)
        {
          case SplineInterpolation.Bezier:
            tangent = (firstKey.Point - firstKey.TangentIn) * 3;
            break;
          case SplineInterpolation.Hermite:
            tangent = firstKey.TangentIn;
            break;
          default:
            {
              if (numberOfKeys == 1)
                return new Vector2F(parameter, firstKey.Point.Y);

              GetSplines(0, out xSpline, out ySpline);
              tangent = new Vector2F(xSpline.GetTangent(0), ySpline.GetTangent(0));
              ((IRecyclable)xSpline).Recycle();
              ((IRecyclable)ySpline).Recycle();
              break;
            }
        }

        float k = 0;
        if (Numeric.IsZero(tangent.X) == false)
          k = tangent.Y / tangent.X;

        return new Vector2F(interpolatedX, firstKey.Point.Y + k * (loopedParameter - curveStart));
      }
      else if (loopedParameter > curveEnd)
      {
        Debug.Assert(PostLoop == CurveLoopType.Linear);

        // Get tangent.
        Vector2F tangent;
        switch (lastKey.Interpolation)
        {
          case SplineInterpolation.Bezier:
            tangent = (lastKey.TangentOut - lastKey.Point) * 3;
            break;
          case SplineInterpolation.Hermite:
            tangent = lastKey.TangentOut;
            break;
          default:
            {
              if (numberOfKeys == 1)
                return new Vector2F(parameter, firstKey.Point.Y);

              GetSplines(numberOfKeys - 2, out xSpline, out ySpline);
              tangent = new Vector2F(xSpline.GetTangent(1), ySpline.GetTangent(1));
              ((IRecyclable)xSpline).Recycle();
              ((IRecyclable)ySpline).Recycle();
              break;
            }
        }

        float k = 0;
        if (Numeric.IsZero(tangent.X) == false)
          k = tangent.Y / tangent.X;

        return new Vector2F(interpolatedX, lastKey.Point.Y + k * (loopedParameter - curveEnd));
      }
      #endregion

      // Special case: Only 1 point.
      if (numberOfKeys == 1)
        return new Vector2F(parameter, firstKey.Point.Y);

      float cycleOffset = GetCycleOffset(parameter);

      // Special case: Parameter = parameter of last key.
      if (loopedParameter == lastKey.Parameter
          && Items[numberOfKeys - 2].Interpolation != SplineInterpolation.BSpline)  // BSplines need special handling because they do not go through key points.
      {
        return new Vector2F(parameter, lastKey.Point.Y + cycleOffset);
      }

      // Get near keys.
      int index = GetKeyIndex(loopedParameter);
      // If the looped parameter == parameter of last key, we want to use the previous key.
      if (index == numberOfKeys - 1)
        index--;

      Debug.Assert(0 <= index && index < numberOfKeys - 1);
      CurveKey2F p2 = Items[index];
      CurveKey2F p3 = Items[index + 1];
      float splineStart = p2.Parameter;
      float splineEnd = p3.Parameter;
      float splineLength = (splineEnd - splineStart);

      // Get splines for this segment.
      GetSplines(index, out xSpline, out ySpline);

      // Find correct parameter.
      float relativeParameter = 0;
      if (!Numeric.IsZero(splineLength))
      {
        if (Items[index].Interpolation == SplineInterpolation.Bezier
          || Items[index].Interpolation == SplineInterpolation.BSpline
          || Items[index].Interpolation == SplineInterpolation.CatmullRom
          || Items[index].Interpolation == SplineInterpolation.Hermite)
        {
          // x spline is not linearly proportional. Need root finding.
          relativeParameter = CurveHelper.GetParameter(xSpline, loopedParameter, 20);
          if (Numeric.IsNaN(relativeParameter) && Items[index].Interpolation == SplineInterpolation.BSpline)
          {
            // Search neighbor splines. BSpline do not normally go exactly from p2 to p3.
            // Try left neighbor spline first.
            if (index-1 >= 0 && Items[index - 1].Interpolation == SplineInterpolation.BSpline)
            {
              ((IRecyclable)xSpline).Recycle();
              ((IRecyclable)ySpline).Recycle();
              GetSplines(index - 1, out xSpline, out ySpline);
              relativeParameter = CurveHelper.GetParameter(xSpline, loopedParameter, 20);
            }

            // If we didn't get a solution search the right neighbor.
            if (Numeric.IsNaN(relativeParameter) 
                && index + 1 < numberOfKeys - 1 
                && Items[index + 1].Interpolation == SplineInterpolation.BSpline)
            {
              ((IRecyclable)xSpline).Recycle();
              ((IRecyclable)ySpline).Recycle();
              GetSplines(index + 1, out xSpline, out ySpline);
              relativeParameter = CurveHelper.GetParameter(xSpline, loopedParameter, 20);
            }
          }

          Debug.Assert(
            Items[index].Interpolation == SplineInterpolation.BSpline   // BSplines do sometimes not include the boundary points, but for the rest we expect a solution.
            || Numeric.AreEqual(xSpline.GetPoint(relativeParameter), loopedParameter, Math.Max(Math.Abs(splineLength) * Numeric.EpsilonF * 10, Numeric.EpsilonF)));
        }
        else
        {
          relativeParameter = (loopedParameter - splineStart) / splineLength;
        }
      }

      // Find y value for parameter.
      float interpolatedY = ySpline.GetPoint(relativeParameter);

      ((IRecyclable)xSpline).Recycle();
      ((IRecyclable)ySpline).Recycle();

      return new Vector2F(interpolatedX, interpolatedY + cycleOffset);
    }


    /// <summary>
    /// Gets the cycle offset for a given parameter.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>The cycle offset.</returns>
    /// <remarks>
    /// The cycle offset is 0 if the <see cref="CurveLoopType"/> is unequal
    /// to <see cref="CurveLoopType.CycleOffset"/> or if the <paramref name="parameter"/>
    /// is on the curve. 
    /// </remarks>
    private float GetCycleOffset(float parameter)
    {
      CurveKey2F firstKey = Items[0];
      CurveKey2F lastKey = Items[Count - 1];
      float curveStart = firstKey.Parameter;
      float curveEnd = lastKey.Parameter;
      float curveLength = curveEnd - curveStart;

      // Handle cycle offset.
      float cycleOffset = 0;
      if (!Numeric.IsZero(curveLength))
      {
        float endDifference = lastKey.Point.Y - firstKey.Point.Y;
        if (parameter < curveStart && PreLoop == CurveLoopType.CycleOffset)
        {
          int numberOfPeriods = (int)((parameter - curveEnd) / curveLength);
          cycleOffset = numberOfPeriods * endDifference;
        }
        else if (parameter > curveEnd && PostLoop == CurveLoopType.CycleOffset)
        {
          int numberOfPeriods = (int)((parameter - curveStart) / curveLength);
          cycleOffset = numberOfPeriods * endDifference;
        }
      }

      return cycleOffset;
    }

    
    /// <summary>
    /// Computes the tangent for a point on the curve.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>
    /// The curve tangent. If the function is not differentiable at the given parameter, either the 
    /// incoming or outgoing tangent is returned.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The curve tangent can be used to compute the slope <i>k</i> of the function at the given 
    /// parameter.
    /// <code lang="csharp">
    /// <![CDATA[
    /// Vector2F tangent = curve.GetTangent(0.5f); // Get the tangent at x = 0.5.
    /// float k = tangent.Y / tangent.X;           // Compute the slope at x = 0.5.
    /// ]]>
    /// </code>
    /// </para>
    /// <para>
    /// If the curve keys contain invalid tangents, the results are undefined.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    public override Vector2F GetTangent(float parameter)
    {
      int numberOfKeys = Count;
      if (numberOfKeys == 0)
        return new Vector2F();

      CurveKey2F firstKey = Items[0];
      CurveKey2F lastKey = Items[numberOfKeys - 1];
      float curveStart = firstKey.Parameter;
      float curveEnd = lastKey.Parameter;

      if (PreLoop == CurveLoopType.Constant && parameter < curveStart
          || PostLoop == CurveLoopType.Constant && parameter > curveEnd)
      {
        return Vector2F.UnitX;
      }

      // Correct parameter.
      float loopedParameter = LoopParameter(parameter);
      ICurve<float, float> xSpline, ySpline;

      // Note: Tangents from splines are relative to the spline parameter range [0,1]. We have
      // to divide by the spline length measured in the curve parameter unit.

      // Handle CurveLoopType.Linear:
      // If parameter is outside: Use spline tangent. Exceptions are Bézier and Hermite
      // were the exact tangent is given in the outer keys.
      if (loopedParameter < curveStart)
      {
        Debug.Assert(PreLoop == CurveLoopType.Linear);

        Vector2F tangent;
        switch (firstKey.Interpolation)
        {
          case SplineInterpolation.Bezier:
            tangent = (firstKey.Point - firstKey.TangentIn) * 3;
            break;
          case SplineInterpolation.Hermite:
            tangent = firstKey.TangentIn;
            break;
          case SplineInterpolation.StepLeft:
          case SplineInterpolation.StepCentered:
          case SplineInterpolation.StepRight:
            tangent = Vector2F.UnitX;
            break;
          default:
            if (numberOfKeys > 1)
            {
              GetSplines(0, out xSpline, out ySpline);
              tangent = new Vector2F(xSpline.GetTangent(0), ySpline.GetTangent(0));
              ((IRecyclable)xSpline).Recycle();
              ((IRecyclable)ySpline).Recycle();
            }
            else
            {
              tangent = Vector2F.UnitX;
            }
            break;
        }

        float splineLength = (numberOfKeys > 1) ? Items[1].Parameter - curveStart : 0;
        return (splineLength > 0) ? tangent / splineLength : tangent;
      }
      else if (loopedParameter > curveEnd)
      {
        Debug.Assert(PostLoop == CurveLoopType.Linear);

        Vector2F tangent;
        switch (lastKey.Interpolation)
        {
          case SplineInterpolation.Bezier:
            tangent = (lastKey.TangentOut - lastKey.Point) * 3;
            break;
          case SplineInterpolation.Hermite:
            tangent = lastKey.TangentOut;
            break;
          case SplineInterpolation.StepLeft:
          case SplineInterpolation.StepCentered:
          case SplineInterpolation.StepRight:
            tangent = Vector2F.UnitX;
            break;
          default:
            if (numberOfKeys > 1)
            {
              GetSplines(numberOfKeys - 2, out xSpline, out ySpline);
              tangent = new Vector2F(xSpline.GetTangent(1), ySpline.GetTangent(1));
              ((IRecyclable)xSpline).Recycle();
              ((IRecyclable)ySpline).Recycle();
            }
            else
            {
              tangent = Vector2F.UnitX;
            }
            break;
        }

        float splineLength = (numberOfKeys > 1) ? curveEnd - Items[numberOfKeys - 2].Parameter : 0;
        return (splineLength > 0) ? tangent / splineLength : tangent;
      }
      else
      {
        if (numberOfKeys == 1)
        {
          // Degenerate curves with 1 key.
          // Note: The following tangents are not scaled because we do not have enough information.
          if (PostLoop == CurveLoopType.Linear)
          {
            // Pick outgoing tangent.
            CurveKey2F p = firstKey;
            if (p.Interpolation == SplineInterpolation.Bezier)
              return (p.TangentOut - p.Point) * 3;

            if (p.Interpolation == SplineInterpolation.Hermite)
              return p.TangentOut;
          }
          else if (PreLoop == CurveLoopType.Linear)
          {
            // Pick incoming tangent.
            CurveKey2F p = firstKey;
            if (p.Interpolation == SplineInterpolation.Bezier)
              return (p.Point - p.TangentIn) * 3;

            if (p.Interpolation == SplineInterpolation.Hermite)
              return p.TangentIn;
          }

          return Vector2F.UnitX;
        }

        int index = GetKeyIndex(loopedParameter);
        if (index == numberOfKeys - 1)
          index--;  // For the last key we take the previous spline.

        Debug.Assert(0 <= index && index < numberOfKeys - 1);
        CurveKey2F p2 = Items[index];
        CurveKey2F p3 = Items[index + 1];

        // Special case: Step interpolation.
        if (p2.Interpolation == SplineInterpolation.StepLeft
            || p2.Interpolation == SplineInterpolation.StepCentered
            || p2.Interpolation == SplineInterpolation.StepRight)
        {
          return Vector2F.UnitX;
        }

        float splineStart = p2.Parameter;
        float splineEnd = p3.Parameter;
        float splineLength = splineEnd - splineStart;

        // Get splines for this segment.
        GetSplines(index, out xSpline, out ySpline);

        // Find correct parameter.
        float relativeParameter = 0;
        if (!Numeric.IsZero(splineLength))
        {
          if (Items[index].Interpolation == SplineInterpolation.Bezier
              || Items[index].Interpolation == SplineInterpolation.BSpline
              || Items[index].Interpolation == SplineInterpolation.CatmullRom
              || Items[index].Interpolation == SplineInterpolation.Hermite)
          {
            // x spline is not linearly proportional. Need root finding.
            relativeParameter = CurveHelper.GetParameter(xSpline, loopedParameter, 20);
            if (Numeric.IsNaN(relativeParameter) && Items[index].Interpolation == SplineInterpolation.BSpline)
            {
              // Search neighbor splines. BSpline do not normally go exactly from p2 to p3.
              // Try left neighbor spline first.
              if (index - 1 >= 0 && Items[index - 1].Interpolation == SplineInterpolation.BSpline)
              {
                ((IRecyclable)xSpline).Recycle();
                ((IRecyclable)ySpline).Recycle();
                GetSplines(index - 1, out xSpline, out ySpline);
                relativeParameter = CurveHelper.GetParameter(xSpline, loopedParameter, 20);
              }

              // If we didn't get a solution search the right neighbor.
              if (Numeric.IsNaN(relativeParameter) && index + 1 < numberOfKeys - 1
                  && Items[index + 1].Interpolation == SplineInterpolation.BSpline)
              {
                ((IRecyclable)xSpline).Recycle();
                ((IRecyclable)ySpline).Recycle();
                GetSplines(index + 1, out xSpline, out ySpline);
                relativeParameter = CurveHelper.GetParameter(xSpline, loopedParameter, 20);
              }
            }
            Debug.Assert(
              Items[index].Interpolation == SplineInterpolation.BSpline
              || // BSplines do sometimes not include the boundary points, but for the rest we expect a solution.
              Numeric.AreEqual(
                xSpline.GetPoint(relativeParameter),
                loopedParameter,
                Math.Max(Math.Abs(splineLength) * Numeric.EpsilonF * 10, Numeric.EpsilonF)));
          }
          else
          {
            relativeParameter = (loopedParameter - splineStart) / splineLength;
          }
        }

        float tangentX = xSpline.GetTangent(relativeParameter);
        float tangentY = ySpline.GetTangent(relativeParameter);
        if (IsInMirroredOscillation(parameter))
          tangentY = -tangentY;  // Mirrored direction.

        ((IRecyclable)xSpline).Recycle();
        ((IRecyclable)ySpline).Recycle();

        return new Vector2F(tangentX, tangentY) / splineLength;
      }
    }


    /// <summary>
    /// Not supported.
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
    /// <returns>The approximated length of the curve in the given interval.</returns>
    /// <exception cref="NotSupportedException">
    /// This method is not supported.
    /// </exception>
    public override float GetLength(float start, float end, int maxNumberOfIterations, float tolerance)
    {
      throw new NotSupportedException();
    }



    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="maxNumberOfIterations">The max number of iterations.</param>
    /// <param name="tolerance">The tolerance.</param>
    /// <exception cref="NotSupportedException">
    /// This operation is not supported. A <see cref="Curve2F"/> cannot be flattened.
    /// </exception>
    public override void Flatten(ICollection<Vector2F> points, int maxNumberOfIterations, float tolerance)
    {
      throw new NotSupportedException();
    }


    /// <summary>
    /// Gets the splines for the given segment index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="xSpline">The x spline.</param>
    /// <param name="ySpline">The y spline.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private void GetSplines(int index, out ICurve<float, float> xSpline, out ICurve<float, float> ySpline)
    {
      // Spline keys
      CurveKey2F p2 = Items[index];
      CurveKey2F p3 = Items[Math.Min(Count - 1, index + 1)];

      if (Items[index].Interpolation == SplineInterpolation.StepLeft)
      {
        var spline = StepSegment1F.Create();
        spline.Point1 = p2.Point.X;
        spline.Point2 = p3.Point.X;
        spline.StepType = StepInterpolation.Left;
        xSpline = spline;

        spline = StepSegment1F.Create();
        spline.Point1 = p2.Point.Y;
        spline.Point2 = p3.Point.Y;
        spline.StepType = StepInterpolation.Left;
        ySpline = spline;
      }
      else if (Items[index].Interpolation == SplineInterpolation.StepCentered)
      {
        var spline = StepSegment1F.Create();
        spline.Point1 = p2.Point.X;
        spline.Point2 = p3.Point.X;
        spline.StepType = StepInterpolation.Centered;
        xSpline = spline;

        spline = StepSegment1F.Create();
        spline.Point1 = p2.Point.Y;
        spline.Point2 = p3.Point.Y;
        spline.StepType = StepInterpolation.Centered;
        ySpline = spline;
      }
      else if (Items[index].Interpolation == SplineInterpolation.StepRight)
      {
        var spline = StepSegment1F.Create();
        spline.Point1 = p2.Point.X;
        spline.Point2 = p3.Point.X;
        spline.StepType = StepInterpolation.Right;
        xSpline = spline;

        spline = StepSegment1F.Create();
        spline.Point1 = p2.Point.Y;
        spline.Point2 = p3.Point.Y;
        spline.StepType = StepInterpolation.Right;
        ySpline = spline;
      }
      else if (Items[index].Interpolation == SplineInterpolation.Linear)
      {
        var spline = LineSegment1F.Create();
        spline.Point1 = p2.Point.X;
        spline.Point2 = p3.Point.X;
        xSpline = spline;

        spline = LineSegment1F.Create();
        spline.Point1 = p2.Point.Y;
        spline.Point2 = p3.Point.Y;
        ySpline = spline;
      }
      else if (Items[index].Interpolation == SplineInterpolation.Bezier)
      {
        var spline = BezierSegment1F.Create();
        spline.Point1 = p2.Point.X;
        spline.ControlPoint1 = p2.TangentOut.X;
        spline.Point2 = p3.Point.X;
        spline.ControlPoint2 = p3.TangentIn.X;
        xSpline = spline;

        spline = BezierSegment1F.Create();
        spline.Point1 = p2.Point.Y;
        spline.ControlPoint1 = p2.TangentOut.Y;
        spline.Point2 = p3.Point.Y;
        spline.ControlPoint2 = p3.TangentIn.Y;
        ySpline = spline;
      }
      else if (Items[index].Interpolation == SplineInterpolation.Hermite)
      {
        var spline = HermiteSegment1F.Create();
        spline.Point1 = p2.Point.X;
        spline.Tangent1 = p2.TangentOut.X;
        spline.Point2 = p3.Point.X;
        spline.Tangent2 = p3.TangentIn.X;
        xSpline = spline;

        spline = HermiteSegment1F.Create();
        spline.Point1 = p2.Point.Y;
        spline.Tangent1 = p2.TangentOut.Y;
        spline.Point2 = p3.Point.Y;
        spline.Tangent2 = p3.TangentIn.Y;
        ySpline = spline;
      }
      else
      {
        Vector2F p1; 
        Vector2F p4; 
        #region ----- Find CatmullRom/BSpline neigbor points p1 and p4 -----
        if (index > 0)
          p1 = Items[index - 1].Point;
        else if (SmoothEnds && PreLoop == CurveLoopType.Constant && Items[index].Interpolation == SplineInterpolation.CatmullRom)
        {
          // Mirror point 1 through point 0 for x component.
          // Set a constant y.
          // This does not work for BSplines because they would not run through the last point.
          p1 = new Vector2F(Items[0].Point.X - (Items[1].Point.X - Items[0].Point.X),
                            p2.Point.Y);
        }
        else if (SmoothEnds && PreLoop == CurveLoopType.Cycle)
        {
          // Wrap around.
          p1 = Items[Count - 2].Point;
          // Add offset to x.
          p1.X = p1.X - (Items[Count - 1].Point.X - Items[0].Point.X);
        }
        else if (SmoothEnds && PreLoop == CurveLoopType.CycleOffset)
        {
          // Wrap around and add offset. 
          p1 = Items[Count - 2].Point - (Items[Count - 1].Point - Items[0].Point);
        }
        else if (SmoothEnds && PreLoop == CurveLoopType.Oscillate)
        {
          // Mirror point 1 through point 0 for x component.
          // Y should be the same as p3.
          p1 = new Vector2F(Items[0].Point.X - (Items[1].Point.X - Items[0].Point.X),
                            p3.Point.Y);
        }
        else
        {
          // Mirror point 1 through point 0.
          p1 = Items[0].Point - (Items[1].Point - Items[0].Point);
        }

        Debug.Assert(index > 0 || Count > 1);
        if (index + 2 < Count)
          p4 = Items[index + 2].Point;
        else if (SmoothEnds && PostLoop == CurveLoopType.Constant && Items[index].Interpolation == SplineInterpolation.CatmullRom)
        {
          // This does not work for BSplines because they would not run through the last point.
          // Mirror point Count-2 through last point for x component.
          // Set a constant y.
          p4 = new Vector2F(Items[Count - 1].Point.X + (Items[Count - 1].Point.X - Items[Count - 2].Point.X),
                            p3.Point.Y);
        }
        else if (SmoothEnds && PostLoop == CurveLoopType.Cycle)
        {
          // Wrap around.
          p4 = Items[1].Point;
          // Add offset to x.
          p4.X = p4.X + (Items[Count - 1].Point.X - Items[0].Point.X);
        }
        else if (SmoothEnds && PostLoop == CurveLoopType.CycleOffset)
        {
          // Wrap around and add offset. 
          p4 = Items[1].Point + (Items[Count - 1].Point - Items[0].Point);
        }
        else if (SmoothEnds && PostLoop == CurveLoopType.Oscillate)
        {
          // Mirror point Count-2 through last point for x component.
          // y should be the same as p2.Y.
          p4 = new Vector2F(Items[Count - 1].Point.X + (Items[Count - 1].Point.X - Items[Count - 2].Point.X),
                            p2.Point.Y);
        }
        else
        {
          // Mirror point Count-2 through last point.
          p4 = Items[Count - 1].Point + (Items[Count - 1].Point - Items[Count - 2].Point);
        }
        #endregion
    
        if (Items[index].Interpolation == SplineInterpolation.BSpline)
        {
          var spline = BSplineSegment1F.Create();
          spline.Point1 = p1.X;
          spline.Point2 = p2.Point.X;
          spline.Point3 = p3.Point.X;
          spline.Point4 = p4.X;
          xSpline = spline;

          spline = BSplineSegment1F.Create();
          spline.Point1 = p1.Y;
          spline.Point2 = p2.Point.Y;
          spline.Point3 = p3.Point.Y;
          spline.Point4 = p4.Y;
          ySpline = spline;
        }
        else
        {
          Debug.Assert((Items[index].Interpolation == SplineInterpolation.CatmullRom));

          var spline = CatmullRomSegment1F.Create();
          spline.Point1 = p1.X;
          spline.Point2 = p2.Point.X;
          spline.Point3 = p3.Point.X;
          spline.Point4 = p4.X;
          xSpline = spline;

          spline = CatmullRomSegment1F.Create();
          spline.Point1 = p1.Y;
          spline.Point2 = p2.Point.Y;
          spline.Point3 = p3.Point.Y;
          spline.Point4 = p4.Y;
          ySpline = spline;
        }
      }
    }


    #region Implementation of IXmlSerializable

    /// <summary>
    /// This method is reserved and should not be used. When implementing the 
    /// <see cref="IXmlSerializable"/> interface, you should return <see langword="null"/> from this
    /// method, and instead, if specifying a custom schema is required, apply the 
    /// <see cref="XmlSchemaProviderAttribute"/> to the class.
    /// </summary>
    /// <returns>
    /// An <see cref="XmlSchema"/> that describes the XML representation of the object that is
    /// produced by the <see cref="IXmlSerializable.WriteXml(XmlWriter)"/> method and consumed by
    /// the <see cref="IXmlSerializable.ReadXml(XmlReader)"/> method.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    XmlSchema IXmlSerializable.GetSchema()
    {
      return null;
    }


    /// <summary>
    /// Generates an object from its XML representation.
    /// </summary>
    /// <param name="reader">
    /// The <see cref="XmlReader"/> stream from which the object is deserialized. 
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    void IXmlSerializable.ReadXml(XmlReader reader)
    {
      reader.ReadStartElement();
      ReadXml(reader);
      reader.ReadStartElement("SmoothEnds");
      SmoothEnds = reader.ReadContentAsBoolean();
      reader.ReadEndElement();
      reader.ReadEndElement();
    }


    /// <summary>
    /// Converts an object into its XML representation.
    /// </summary>
    /// <param name="writer">
    /// The <see cref="XmlWriter"/> stream to which the object is serialized. 
    /// </param>
    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
      WriteXml(writer);
      writer.WriteStartElement("SmoothEnds");
      writer.WriteValue(SmoothEnds);
      writer.WriteEndElement();
    }
    #endregion
  }
}
