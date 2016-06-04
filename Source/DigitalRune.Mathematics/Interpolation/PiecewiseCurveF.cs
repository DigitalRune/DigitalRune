// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Represents a curve that is defined by piecewise interpolation of curve keys (control points).
  /// (Single-precision)
  /// </summary>
  /// <typeparam name="TPoint">
  /// The type of the curve points (such as <see cref="Vector2F"/>, <see cref="Vector3F"/>, etc.).
  /// </typeparam>
  /// <typeparam name="TCurveKey">
  /// The type of the curve key. (A type derived from <see cref="CurveKey{TParam,TPoint}"/>.)
  /// </typeparam>
  /// <inheritdoc cref="PiecewiseCurve{TParam,TPoint,TCurveKey}"/>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public abstract class PiecewiseCurveF<TPoint, TCurveKey> 
    : PiecewiseCurve<float, TPoint, TCurveKey>
      where TCurveKey : CurveKey<float, TPoint>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the index of the curve key <i>before</i> or at the given parameter value.
    /// </summary>
    /// <param name="parameter">The parameter value.</param>
    /// <returns>
    /// The index of the curve key or <c>-1</c> if no suitable curve key exists.
    /// </returns>
    /// <remarks>
    /// This method assumes that the curve keys are sorted and returns index of the key with the
    /// largest <see cref="CurveKey{TParam,TValue}.Parameter"/> value that is less than or equal to
    /// the given parameter value. The parameter will lie between the key at the returned index and
    /// the key at index + 1. If <paramref name="parameter"/> is beyond the start or end of the
    /// path, a key index according to <see cref="PiecewiseCurve{TParam,TPoint,TCurveKey}.PreLoop"/> 
    /// and <see cref="PiecewiseCurve{TParam,TPoint,TCurveKey}.PostLoop"/> is returned.
    /// </remarks>
    public override int GetKeyIndex(float parameter)
    {
      int numberOfKeys = Count;
      if (numberOfKeys == 0)
        return -1;

      // Handle looping.
      parameter = LoopParameter(parameter);

      // If parameter is left outside than we have no previous key. 
      // This happens for CurveLoopType.Linear.
      if (parameter < Items[0].Parameter)
        return -1;

      // If parameter is right outside, return the last key. 
      if (parameter > Items[numberOfKeys - 1].Parameter)
        return numberOfKeys - 1;

      // Binary search.
      int start = 0;
      int end = numberOfKeys - 1;
      while (start <= end)
      {
        int index = start + (end - start >> 1);
        float comparison = Items[index].Parameter - parameter;
        if (comparison == 0)
        {
          return index;
        }

        if (comparison < 0)
        {
          Debug.Assert(parameter > Items[index].Parameter);
          start = index + 1;
        }
        else
        {
          Debug.Assert(parameter < Items[index].Parameter);
          end = index - 1;
        }
      }

      return start - 1;
    }


    /// <summary>
    /// Determines whether the given parameter corresponds to a mirrored oscillation loop.
    /// </summary>
    /// <param name="parameter">The parameter value.</param>
    /// <returns>
    /// <see langword="true"/> if the parameter is in a mirrored oscillation loop; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// When the parameter is less than the parameter of the first key or greater than the parameter
    /// of the last key, then the parameter is outside the regular curve. The outside behavior is 
    /// determined by <see cref="PiecewiseCurve{TParam,TPoint,TCurveKey}.PreLoop"/> and 
    /// <see cref="PiecewiseCurve{TParam,TPoint,TCurveKey}.PostLoop"/>. If the loop type is 
    /// <see cref="CurveLoopType.Oscillate"/> the curve is mirrored after each loop cycle. This 
    /// method returns <see langword="true"/> if the parameter is outside and belongs to a curve 
    /// loop which is mirrored to the regular curve.
    /// </remarks>
    public override bool IsInMirroredOscillation(float parameter)
    {
      int numberOfKeys = Count;
      if (numberOfKeys == 0)
        return false;

      float curveStart = Items[0].Parameter;
      float curveEnd = Items[numberOfKeys - 1].Parameter;
      float curveLength = curveEnd - curveStart;

      if (Numeric.IsZero(curveLength))
        return false;

      if (parameter < curveStart && PreLoop == CurveLoopType.Oscillate)
        return ((int)((curveStart - parameter) / curveLength)) % 2 == 0;
      else if (parameter > curveEnd && PostLoop == CurveLoopType.Oscillate)
        return ((int)((parameter - curveEnd) / curveLength)) % 2 == 0;
      else
        return false;
    }


    /// <summary>
    /// Handles pre- and post-looping by changing the given parameter so that it lies on the curve.
    /// </summary>
    /// <param name="parameter">The parameter value.</param>
    /// <returns>The modified parameter value.</returns>
    /// <remarks>
    /// <para>
    /// If the parameter lies outside the curve the parameter is changed so that it lies on the 
    /// curve. The new parameter can be used to compute the curve result. 
    /// </para>
    /// <para>
    /// Following <see cref="CurveLoopType"/>s need special handling:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="CurveLoopType.Linear"/>: The parameter is not changed to lie on the curve; the
    /// linear extrapolation of the curve has to be computed.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="CurveLoopType.CycleOffset"/>: The parameter is corrected to be on the curve; the
    /// curve function at this parameter can be evaluated and the offset must be added. The curve
    /// point offset is not handled in this method.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public override float LoopParameter(float parameter)
    {
      int numberOfKeys = Count;
      if (numberOfKeys == 0)
        return parameter;

      TCurveKey firstKey = Items[0];
      TCurveKey lastKey = Items[numberOfKeys - 1];
      float curveStart = firstKey.Parameter;
      float curveEnd = lastKey.Parameter;
      float curveLength = curveEnd - curveStart;

      if (parameter < curveStart)
      {
        #region ----- Pre-loop -----

        // Handle pre-loop. For some loop types we return immediately. For some
        // we adjust the parameter.
        if (PreLoop == CurveLoopType.Linear)
          return parameter;   // Linear is the only type where a parameter outside the curve range makes sense.

        if (PreLoop == CurveLoopType.Constant || numberOfKeys == 1)
          return curveStart;

        if (Numeric.IsZero(curveLength))
          return curveStart;

        int numberOfPeriods = (int)((curveStart - parameter) / curveLength);
        if (PreLoop == CurveLoopType.Cycle || PreLoop == CurveLoopType.CycleOffset)
          return parameter + curveLength * (numberOfPeriods + 1);
        
        Debug.Assert(PreLoop == CurveLoopType.Oscillate);
        if (numberOfPeriods % 2 == 0)
        {
          // even = mirrored
          return curveStart + curveStart - (parameter + curveLength * numberOfPeriods);
        }
        else
        {
          // odd = not mirrored
          return parameter + curveLength * (numberOfPeriods + 1);
        }       
        #endregion
      }
      else if (parameter > curveEnd)
      {
        #region ----- Post-loop -----

        // Handle post-loop. For some loop types we return immediately. For some
        // we adjust the parameter.
        if (PostLoop == CurveLoopType.Linear)
          return parameter; // Linear is the only type where a parameter outside the curve range makes sense.

        if (PostLoop == CurveLoopType.Constant || numberOfKeys == 1)
          return curveEnd;

        if (Numeric.IsZero(curveLength))
          return curveStart;

        int numberOfPeriods = (int)((parameter - curveEnd) / curveLength);
        if (PostLoop == CurveLoopType.Cycle || PostLoop == CurveLoopType.CycleOffset)
          return parameter - curveLength * (numberOfPeriods + 1);
        
        Debug.Assert(PostLoop == CurveLoopType.Oscillate);
        if (numberOfPeriods % 2 == 0)
        {
          // even = mirrored
          return curveEnd - (parameter - curveLength * numberOfPeriods - curveEnd);
        }
        else
        {
          // odd = not mirrored
          return parameter - curveLength * (numberOfPeriods + 1);
        }
        #endregion
      }

      return parameter;
    }
    #endregion
  }
}
