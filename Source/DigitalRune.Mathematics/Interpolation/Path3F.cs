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
  /// Represents a 3-dimensional path that is defined by piecewise interpolation of key points 
  /// (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="Path3F"/> is a "piecewise curve". That means, the path is defined by key points
  /// (<see cref="PathKey3F"/>) that are interpolated using spline interpolation. See 
  /// <see cref="PiecewiseCurveF{TValue,TCurveKey}"/> for more information on piecewise curves.
  /// </para>
  /// <para>
  /// <strong>Path Keys:</strong> 
  /// The path keys (<see cref="PathKey3F"/>) define the control points of the path. Each path
  /// key defines a point on the curve <see cref="CurveKey{TParam,TValue}.Point"/>. These points are 
  /// interpolated using spline interpolation. Each path key defines the type of spline 
  /// interpolation that is used from this path key to the next (see 
  /// <see cref="CurveKey{TParam,TValue}.Interpolation"/>). The path keys also contain additional 
  /// information that might me required for interpolation such as: 
  /// <see cref="CurveKey{TParam,TValue}.TangentIn"/>, 
  /// <see cref="CurveKey{TParam,TValue}.TangentOut"/>.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public class Path3F : PiecewiseCurveF<Vector3F, PathKey3F>, IXmlSerializable
  {
    /// <inheritdoc/>
    public override Vector3F GetPoint(float parameter)
    {
      int numberOfKeys = Count;
      if (numberOfKeys == 0)
        return new Vector3F(float.NaN);

      // Correct parameter.
      float loopedParameter = LoopParameter(parameter);

      #region ----- Handle CurveLoopType.Linear -----
      var firstKey = Items[0];
      var lastKey = Items[numberOfKeys - 1];
      float curveStart = firstKey.Parameter;
      float curveEnd = lastKey.Parameter;
      if (loopedParameter < curveStart)
      {
        Debug.Assert(PreLoop == CurveLoopType.Linear);
        var tangent = GetTangent(loopedParameter);
        return firstKey.Point + tangent * (loopedParameter - curveStart);
      }

      if (loopedParameter > curveEnd)
      {
        Debug.Assert(PostLoop == CurveLoopType.Linear);
        var tangent = GetTangent(loopedParameter);
        return lastKey.Point + tangent * (loopedParameter - curveEnd);
      }
      #endregion

      // Special case: Only 1 point.
      if (numberOfKeys == 1)
        return firstKey.Point;

      var cycleOffset = GetCycleOffset(parameter);

      // Special case: Parameter = parameter of last key.
      if (loopedParameter == lastKey.Parameter
          && Items[numberOfKeys - 2].Interpolation != SplineInterpolation.BSpline)  // BSplines need special handling because they do not go through key points.)
      {
        return lastKey.Point + cycleOffset;
      }

      // Get near keys.
      int index = GetKeyIndex(loopedParameter);
      // If the looped parameter == parameter of last key, we want to use
      // the previous key.
      if (index == numberOfKeys - 1)
        index--;

      Debug.Assert(0 <= index && index < numberOfKeys - 1);
      var p2 = Items[index];
      var p3 = Items[index + 1];

      // Compute relative spline parameter.
      float splineStart = p2.Parameter;
      float splineEnd = p3.Parameter;
      float splineLength = (splineEnd - splineStart);
      loopedParameter = (loopedParameter - splineStart) / splineLength;

      // Get spline point.
      var spline = GetSpline(index);
      var result = spline.GetPoint(loopedParameter) + cycleOffset;
      ((IRecyclable)spline).Recycle();
      return result;
    }


    /// <summary>
    /// Gets the cycle offset for a given parameter.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>The cycle offset.</returns>
    /// <remarks>
    /// The cycle offset is a zero vector if the <see cref="CurveLoopType"/> is unequal to 
    /// <see cref="CurveLoopType.CycleOffset"/> or if the <paramref name="parameter"/> is on the
    /// curve. 
    /// </remarks>
    private Vector3F GetCycleOffset(float parameter)
    {
      var firstKey = Items[0];
      var lastKey = Items[Count - 1];
      float curveStart = firstKey.Parameter;
      float curveEnd = lastKey.Parameter;
      float curveLength = curveEnd - curveStart;

      // Handle cycle offset.
      var cycleOffset = new Vector3F();
      if (!Numeric.IsZero(curveLength))
      {
        var endDifference = lastKey.Point - firstKey.Point;
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


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public override Vector3F GetTangent(float parameter)
    {
      int numberOfKeys = Count;
      if (numberOfKeys == 0)
        return new Vector3F();

      var firstKey = Items[0];
      var lastKey = Items[numberOfKeys - 1];
      float curveStart = firstKey.Parameter;
      float curveEnd = lastKey.Parameter;

      if (PreLoop == CurveLoopType.Constant && parameter < curveStart
          || PostLoop == CurveLoopType.Constant && parameter > curveEnd)
      {
        return new Vector3F();
      }

      float loopedParameter = LoopParameter(parameter);

      // Note: Tangents from splines are relative to the spline parameter range [0,1]. We have
      // to divide by the spline length measured in the curve parameter unit.

      // Handle CurveLoopType.Linear:
      // If parameter is outside: Use spline tangent. Exceptions are Bézier and Hermite
      // were the exact tangent is given in the outer keys.
      if (loopedParameter < curveStart)
      {
        Debug.Assert(PreLoop == CurveLoopType.Linear);

        Vector3F tangent;
        switch (firstKey.Interpolation)
        {
          case SplineInterpolation.Bezier:
            tangent = (firstKey.Point - firstKey.TangentIn) * 3;
            break;
          case SplineInterpolation.Hermite:
            tangent = firstKey.TangentIn;
            break;
          default:
            if (numberOfKeys > 1)
            {
              var spline = GetSpline(0);
              tangent = spline.GetTangent(0);
              ((IRecyclable)spline).Recycle();
            }
            else
            {
              tangent = new Vector3F();
            }
            break;
        }

        float splineLength = (numberOfKeys > 1) ? Items[1].Parameter - curveStart : 0;
        return (splineLength > 0) ? tangent / splineLength : tangent;
      }
      else if (loopedParameter > curveEnd)
      {
        Debug.Assert(PostLoop == CurveLoopType.Linear);

        Vector3F tangent;
        switch (lastKey.Interpolation)
        {
          case SplineInterpolation.Bezier:
            tangent = (lastKey.TangentOut - lastKey.Point) * 3;
            break;
          case SplineInterpolation.Hermite:
            tangent = lastKey.TangentOut;
            break;
          default:
            if (numberOfKeys > 1)
            {
              var spline = GetSpline(numberOfKeys - 2);
              tangent = spline.GetTangent(1);
              ((IRecyclable)spline).Recycle();
            }
            else
            {
              tangent = new Vector3F();
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
          // Degenerate curve with 1 key.
          // Note: The following tangents are not scaled because we do not have enough information.
          if (PostLoop == CurveLoopType.Linear)
          {
            // Pick outgoing tangent.
            var p = firstKey;
            if (p.Interpolation == SplineInterpolation.Bezier)
              return (p.TangentOut - p.Point) * 3;

            if (p.Interpolation == SplineInterpolation.Hermite)
              return p.TangentOut;
          }
          else if (PreLoop == CurveLoopType.Linear)
          {
            // Pick incoming tangent.
            var p = firstKey;
            if (p.Interpolation == SplineInterpolation.Bezier)
              return (p.Point - p.TangentIn) * 3;

            if (p.Interpolation == SplineInterpolation.Hermite)
              return p.TangentIn;
          }

          return new Vector3F();
        }

        int index = GetKeyIndex(loopedParameter);
        if (index == numberOfKeys - 1)
          index = index - 1;  // For the last key we take the previous spline.

        Debug.Assert(0 <= index && index < numberOfKeys - 1);
        var p2 = Items[index];
        var p3 = Items[index + 1];

        float splineStart = p2.Parameter;
        float splineEnd = p3.Parameter;
        float splineLength = splineEnd - splineStart;
        loopedParameter = (loopedParameter - splineStart) / splineLength;

        var spline = GetSpline(index);
        var tangent = spline.GetTangent(loopedParameter) / splineLength;
        ((IRecyclable)spline).Recycle();

        if (IsInMirroredOscillation(parameter))
          return -tangent;  // Mirrored direction.
        else
          return tangent;   // Normal direction.
      }
    }


    /// <inheritdoc/>
    public override float GetLength(float start, float end, int maxNumberOfIterations, float tolerance)
    {
      int numberOfKeys = Count;
      if (numberOfKeys == 0)
        return 0;
      if (numberOfKeys == 1 && PreLoop != CurveLoopType.Linear && PostLoop != CurveLoopType.Linear)
        return 0;

      // TODO: Maybe a piecewise computation would be faster when the curve has non-continuous parts.
      // Difficulty: start and end are not on spline ends and can be outside.
      // For non-continuous curves the result can have an error larger than tolerance.
      return CurveHelper.GetLength(this, start, end, 5, maxNumberOfIterations, tolerance);
    }


    /// <inheritdoc/>
    public override void Flatten(ICollection<Vector3F> points, int maxNumberOfIterations, float tolerance)
    {
      // Flatten each spline separately. The tolerance is simply divided into equal parts.
      for (int i = 0; i < Count - 1; i++)
      {
        var spline = GetSpline(i);
        spline.Flatten(points, maxNumberOfIterations, tolerance / (Count - 1));
        ((IRecyclable)spline).Recycle();
      }
    }


    /// <summary>
    /// Parameterizes the path by its length.
    /// </summary>
    /// <param name="maxNumberOfIterations">
    /// The maximum number of iterations which are taken to compute the length.
    /// </param>
    /// <param name="tolerance">
    /// The tolerance value. This method will return an approximation of the precise length. The 
    /// absolute error will be less than this tolerance.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="tolerance"/> is negative or 0.
    /// </exception>
    /// <remarks>
    /// Normally, the <see cref="CurveKey{TParam,TValue}.Parameter"/> is not equal to the length of 
    /// the curve from the first key up to the current key. This method will compute length of the
    /// segments and set the key <see cref="CurveKey{TParam,TValue}.Parameter"/>s to the distance 
    /// from the first key. The parameter of the first key will be set to 0. The parameter of the 
    /// second key will be set to the length of the first segment. The parameter of the third key
    /// will be set to the added length of the first two segments. And so on.
    /// </remarks>
    public void ParameterizeByLength(int maxNumberOfIterations, float tolerance)
    {
      if (tolerance <= 0)
        throw new ArgumentOutOfRangeException("tolerance", "The tolerance must be greater than zero.");

      int numberOfKeys = Count;
      if (numberOfKeys == 0)
        return;

      Items[0].Parameter = 0;
      for (int i = 1; i < numberOfKeys; i++)
      {
        var last = Items[i - 1];
        var current = Items[i];
        var spline = GetSpline(i - 1);
        float length = spline.GetLength(0, 1, maxNumberOfIterations, tolerance);
        current.Parameter = last.Parameter + length;
        ((IRecyclable)spline).Recycle();
      }
    }


    /// <summary>
    /// Gets the spline after the key with the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>
    /// The spline or <see langword="null"/> if the interpolation is none of the implemented spline 
    /// classes.
    /// </returns>
    private ICurve<float, Vector3F> GetSpline(int index)
    {
      int numberOfKeys = Count;
      Debug.Assert(index >= 0 && index < numberOfKeys);
      Debug.Assert(numberOfKeys > 1, "Path3F.GetSpline() was called for a curve without 1 full spline segment.");

      // Get the spline points p2 and p3. p1 and p4 are the neighbors.
      var p2 = Items[index];
      var p3 = Items[Math.Min(numberOfKeys - 1, index + 1)];

      if (Items[index].Interpolation == SplineInterpolation.StepLeft)
      {
        var spline = StepSegment3F.Create();
        spline.Point1 = p2.Point;
        spline.Point2 = p3.Point;
        spline.StepType = StepInterpolation.Left;
        return spline;
      }
      if (Items[index].Interpolation == SplineInterpolation.StepCentered)
      {
        var spline = StepSegment3F.Create();
        spline.Point1 = p2.Point;
        spline.Point2 = p3.Point;
        spline.StepType = StepInterpolation.Centered;
        return spline;
      }
      if (Items[index].Interpolation == SplineInterpolation.StepRight)
      {
        var spline = StepSegment3F.Create();
        spline.Point1 = p2.Point;
        spline.Point2 = p3.Point;
        spline.StepType = StepInterpolation.Right;
        return spline;
      }
      if (Items[index].Interpolation == SplineInterpolation.Linear)
      {
        var spline = LineSegment3F.Create();
        spline.Point1 = p2.Point;
        spline.Point2 = p3.Point;
        return spline;
      }
      if (Items[index].Interpolation == SplineInterpolation.Bezier)
      {
        var spline = BezierSegment3F.Create();
        spline.Point1 = p2.Point;
        spline.Point2 = p3.Point;
        spline.ControlPoint1 = p2.TangentOut;
        spline.ControlPoint2 = p3.TangentIn;
        return spline;
      }
      if (Items[index].Interpolation == SplineInterpolation.Hermite)
      {
        var spline = HermiteSegment3F.Create();
        spline.Point1 = p2.Point;
        spline.Point2 = p3.Point;
        spline.Tangent1 = p2.TangentOut;
        spline.Tangent2 = p3.TangentIn;
        return spline;
      }

      Vector3F p1;
      Vector3F p4;
      #region ----- Find CatmullRom/BSpline neigbor points p1 and p4 -----
      if (index > 0)
        p1 = Items[index - 1].Point;
      else if (SmoothEnds && PreLoop == CurveLoopType.Cycle)
      {
        // Wrap around.
        p1 = Items[numberOfKeys - 2].Point;
      }
      else if (SmoothEnds && PreLoop == CurveLoopType.CycleOffset)
      {
        // Wrap around and add offset.
        p1 = Items[numberOfKeys - 2].Point - (Items[numberOfKeys - 1].Point - Items[0].Point);
      }
      else
      {
        // Mirror point 1 through point 0.
        p1 = Items[0].Point - (Items[1].Point - Items[0].Point);
      }

      Debug.Assert(index > 0 || numberOfKeys > 1);
      if (index + 2 < numberOfKeys)
      {
        p4 = Items[index + 2].Point;
      }
      else if (SmoothEnds && PostLoop == CurveLoopType.Cycle)
      {
        // Wrap around.
        p4 = Items[1].Point;
      }
      else if (SmoothEnds && PostLoop == CurveLoopType.CycleOffset)
      {
        // Wrap around and add offset.
        p4 = Items[1].Point + (Items[numberOfKeys - 1].Point - Items[0].Point);
      }
      else
      {
        // Mirror point Count-2 through last point.
        p4 = Items[numberOfKeys - 1].Point + (Items[numberOfKeys - 1].Point - Items[numberOfKeys - 2].Point);
      }
      #endregion

      if (Items[index].Interpolation == SplineInterpolation.BSpline)
      {
        var spline = BSplineSegment3F.Create();
        spline.Point1 = p1;
        spline.Point2 = p2.Point;
        spline.Point3 = p3.Point;
        spline.Point4 = p4;
        return spline;
      }

      Debug.Assert((Items[index].Interpolation == SplineInterpolation.CatmullRom));

      {
        var spline = CatmullRomSegment3F.Create();
        spline.Point1 = p1;
        spline.Point2 = p2.Point;
        spline.Point3 = p3.Point;
        spline.Point4 = p4;
        return spline;
      }
    }


    /// <summary>
    /// Gets the curve parameter for the given curve length (for length-parameterized splines).
    /// </summary>
    /// <param name="length">The length.</param>
    /// <param name="maxNumberOfIterations">
    /// The maximum number of iterations which are taken to compute the length.
    /// </param>
    /// <param name="tolerance">
    /// The tolerance value. This method will return an approximation of the precise parameter. The
    /// absolute error will be less than this tolerance.
    /// </param>
    /// <returns>The parameter at which the curve has the given length.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="tolerance"/> is negative or 0.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method assumes that the spline is length-parameterized; This means: The parameter of a
    /// key is equal to the length of the spline at this key. If this is not the case, 
    /// <see cref="ParameterizeByLength"/> can be called to correct the key parameters
    /// automatically.
    /// </para>
    /// <para>
    /// Normally, the spline curve parameter is not linearly proportional to the length. Therefore, 
    /// if the point at a given curve length is required, this method can be used to compute the
    /// curve parameter which will return the point for the given distance. The result of this
    /// method can be used in <see cref="GetPoint"/>.
    /// </para>
    /// <para>
    /// This method uses an iterative algorithm. The iterations end when the 
    /// <paramref name="maxNumberOfIterations"/> were performed, or when the 
    /// <paramref name="tolerance"/> criterion is met - whichever comes first.
    /// </para>
    /// </remarks>
    public float GetParameterFromLength(float length, int maxNumberOfIterations, float tolerance)
    {
      if (tolerance <= 0)
        throw new ArgumentOutOfRangeException("tolerance", "The tolerance must be greater than zero.");

      int numberOfKeys = Count;
      if (numberOfKeys == 0)
        return float.NaN;

      // Handle CurveLoopType.Linear:
      float curveStart = Items[0].Parameter;
      float curveEnd = Items[numberOfKeys - 1].Parameter;
      float curveLength = curveEnd - curveStart;
      if (length < curveStart && PreLoop == CurveLoopType.Linear)
      {
        var tangent = GetTangent(curveStart);
        return curveStart - (curveStart - length) / tangent.Length;
      }

      if (length > curveEnd && PostLoop == CurveLoopType.Linear)
      {
        var tangent = GetTangent(curveEnd);
        return curveEnd + (length - curveEnd) / tangent.Length;
      }

      if (Numeric.IsZero(curveLength))
        return curveStart;

      // Correct parameter.
      float loopedLength = LoopParameter(length);

      // Find key index for the parameter.
      int index = GetKeyIndex(loopedLength);
      if (index == numberOfKeys - 1)
        index--;  // For the last key we take the previous spline.

      // Get spline.
      var spline = GetSpline(index);

      // Get relative length on the segment from length on whole curve.
      float splineStart = Items[index].Parameter;
      float splineEnd = Items[index + 1].Parameter;
      float splineLength = (splineEnd - splineStart);
      loopedLength = loopedLength - splineStart;
      Debug.Assert(0 <= loopedLength && loopedLength <= splineLength);

      float result = CurveHelper.GetParameter(spline, loopedLength, splineLength, maxNumberOfIterations, tolerance);
      
      float localParameter = Items[index].Parameter + result * splineLength;

      ((IRecyclable)spline).Recycle();
      spline = null;

      // Handle looping: We return a parameter that is outside if the original length parameter is outside.
      // Otherwise the returned parameter would correspond to the correct parameter on the path, but
      // the total distance from the first key would wrong because some "loops" are missing.
      if (length < curveStart && PreLoop != CurveLoopType.Constant)
      {
        int numberOfPeriods = (int)((length - curveEnd) / curveLength);
        if (PreLoop == CurveLoopType.Oscillate && numberOfPeriods % 2 == -1)
          return curveStart + curveStart - localParameter + curveLength * (numberOfPeriods + 1); // odd = mirrored
        else
          return localParameter + numberOfPeriods * curveLength;
      }
      else if (length > curveEnd && PostLoop != CurveLoopType.Constant)
      {
        int numberOfPeriods = (int)((length - curveStart) / curveLength);
        if (PostLoop == CurveLoopType.Oscillate && numberOfPeriods % 2 == 1)
          return curveEnd + curveEnd - localParameter + curveLength * (numberOfPeriods - 1); // odd = mirrored
        else
          return localParameter + numberOfPeriods * curveLength;
      }
      else
        return localParameter;
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
