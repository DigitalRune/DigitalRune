// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Analysis;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Provides helper methods for curves.
  /// </summary>
  internal static partial class CurveHelper
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // This nested class gets a curve parameter using a root finder. This class can be 
    // resource pooled to avoid the allocation of new root finders and delegates.
    // This class supports float, Vector2F and Vector3F curves.
    private sealed class GetParameterHelper1F
    {
      private readonly ImprovedNewtonRaphsonMethodF _rootFinder;
      private ICurve<float, float> _curve;

      public GetParameterHelper1F()
      {
        _rootFinder = new ImprovedNewtonRaphsonMethodF(GetPoint, GetTangent);
      }

      private float GetPoint(float parameter) { return _curve.GetPoint(parameter); }
      private float GetTangent(float parameter) { return _curve.GetTangent(parameter); }

      public float GetParameter(ICurve<float, float> curve, float interpolatedPoint, int maxNumberOfIterations)
      {
        _curve = curve;
        _rootFinder.MaxNumberOfIterations = maxNumberOfIterations;
        float parameter = _rootFinder.FindRoot(0, 1, interpolatedPoint);
        _curve = null;
        return parameter;
      }
    }


    // Same as GetParameterHelper1F but for Vector2F splines.
    private sealed class GetParameterHelper2F
    {
      // Note:
      // The root finder searches the solution for f(parameter) = length. The tolerance 
      // can be used for the parameter or for the length, but not both because 
      // their range is very different. 
      // --> Use Numeric.EpsilonF for the parameter. Use tolerance to compare the lengths. 
      // We use tolerance/10 in GetLength to get an accurate enough length value.

      private readonly ImprovedNewtonRaphsonMethodF _rootFinder;
      private ICurve<float, Vector2F> _curve;
      private int _maxNumberOfIterations;
      private float _tolerance;

      public GetParameterHelper2F()
      {
        _rootFinder = new ImprovedNewtonRaphsonMethodF(GetLength, GetTangentLength);
      }

      private float GetLength(float parameter)
      {
        return _curve.GetLength(0, parameter, _maxNumberOfIterations, _tolerance / 10);
      }

      private float GetTangentLength(float parameter)
      {
        return _curve.GetTangent(parameter).Length;
      }

      public float GetParameter(ICurve<float, Vector2F> curve, float length, int maxNumberOfIterations, float tolerance)
      {
        _curve = curve;
        _maxNumberOfIterations = maxNumberOfIterations;
        _rootFinder.MaxNumberOfIterations = maxNumberOfIterations;
        _tolerance = tolerance;
        _rootFinder.EpsilonY = tolerance;
        float parameter = _rootFinder.FindRoot(0, 1, length);
        _curve = null;
        return parameter;
      }
    }


    // Same as GetParameterHelper1F but for Vector3F splines.
    private sealed class GetParameterHelper3F
    {
      private readonly ImprovedNewtonRaphsonMethodF _rootFinder;
      private ICurve<float, Vector3F> _curve;
      private int _maxNumberOfIterations;
      private float _tolerance;

      public GetParameterHelper3F()
      {
        _rootFinder = new ImprovedNewtonRaphsonMethodF(GetLength, GetTangentLength);
      }

      private float GetLength(float parameter)
      {
        return _curve.GetLength(0, parameter, _maxNumberOfIterations, _tolerance / 10);
      }

      private float GetTangentLength(float parameter)
      {
        return _curve.GetTangent(parameter).Length;
      }

      public float GetParameter(ICurve<float, Vector3F> curve, float length, int maxNumberOfIterations, float tolerance)
      {
        _curve = curve;
        _maxNumberOfIterations = maxNumberOfIterations;
        _rootFinder.MaxNumberOfIterations = maxNumberOfIterations;
        _tolerance = tolerance;
        _rootFinder.EpsilonY = tolerance;
        float parameter = _rootFinder.FindRoot(0, 1, length);
        _curve = null;
        return parameter;
      }
    }


    // Same as GetParameterHelper1F but for GetLength for Vector2F and Vector3F.
    private sealed class GetLengthHelper
    {
      private readonly RombergIntegratorF _integrator;
      private readonly Func<float, float> _function2F;
      private readonly Func<float, float> _function3F;
      private ICurve<float, Vector2F> _curve2F;
      private ICurve<float, Vector3F> _curve3F;

      public GetLengthHelper()
      {
        _integrator = new RombergIntegratorF();
        _function2F = GetTangentLength2F;
        _function3F = GetTangentLength3F;
      }

      private float GetTangentLength2F(float x) { return _curve2F.GetTangent(x).Length; }
      private float GetTangentLength3F(float x) { return _curve3F.GetTangent(x).Length; }

      public float GetLength(ICurve<float, Vector2F> curve, float start, float end, int minNumberOfIterations, int maxNumberOfIterations, float tolerance)
      {
        _curve2F = curve;
        _integrator.MinNumberOfIterations = minNumberOfIterations;
        _integrator.MaxNumberOfIterations = maxNumberOfIterations;
        _integrator.Epsilon = tolerance;
        float length = Math.Abs(_integrator.Integrate(_function2F, start, end));
        _curve2F = null;
        return length;
      }

      public float GetLength(ICurve<float, Vector3F> curve, float start, float end, int minNumberOfIterations, int maxNumberOfIterations, float tolerance)
      {
        _curve3F = curve;
        _integrator.MinNumberOfIterations = minNumberOfIterations;
        _integrator.MaxNumberOfIterations = maxNumberOfIterations;
        _integrator.Epsilon = tolerance;
        float length = Math.Abs(_integrator.Integrate(_function3F, start, end));
        _curve3F = null;
        return length;
      }
    }
    #endregion


    // A pool of reusable GetParameterHelper instances.
    private static ResourcePool<GetParameterHelper1F> GetParameterHelpers1F
    {
      get
      {
        if (_getParameterHelpers1F == null)
          _getParameterHelpers1F = new ResourcePool<GetParameterHelper1F>(
            () => new GetParameterHelper1F(),
            null,
            null);

        return _getParameterHelpers1F;
      }
    }
    private static ResourcePool<GetParameterHelper1F> _getParameterHelpers1F;

    private static ResourcePool<GetParameterHelper2F> GetParameterHelpers2F
    {
      get
      {
        if (_getParameterHelpers2F == null)
          _getParameterHelpers2F = new ResourcePool<GetParameterHelper2F>(
            () => new GetParameterHelper2F(),
            null,
            null);

        return _getParameterHelpers2F;
      }
    }
    private static ResourcePool<GetParameterHelper2F> _getParameterHelpers2F;

    private static ResourcePool<GetParameterHelper3F> GetParameterHelpers3F
    {
      get
      {
        if (_getParameterHelpers3F == null)
          _getParameterHelpers3F = new ResourcePool<GetParameterHelper3F>(
            () => new GetParameterHelper3F(),
            null,
            null);

        return _getParameterHelpers3F;
      }
    }
    private static ResourcePool<GetParameterHelper3F> _getParameterHelpers3F;


    private static ResourcePool<GetLengthHelper> GetLengthHelpers
    {
      get
      {
        if (_getLengthHelpers == null)
          _getLengthHelpers = new ResourcePool<GetLengthHelper>(
            () => new GetLengthHelper(),
            null,
            null);

        return _getLengthHelpers;
      }
    }
    private static ResourcePool<GetLengthHelper> _getLengthHelpers;


    // Returns the curve parameter in the range [0, 1] where the curve value is interpolated point.
    internal static float GetParameter(ICurve<float, float> curve, float interpolatedPoint, int maxNumberOfIterations)
    {
      var getParameterHelper = GetParameterHelpers1F.Obtain();
      var result = getParameterHelper.GetParameter(curve, interpolatedPoint, maxNumberOfIterations);
      GetParameterHelpers1F.Recycle(getParameterHelper);
      return result;
    }


    internal static float GetParameter(ICurve<float, Vector2F> curve, float desiredLength, float totalCurveLength, int maxNumberOfIterations, float tolerance)
    {
      // Root finders may return NaN if the initial bracket is invalid. By checking the
      // borders explicitly, we avoid problems.
      if (desiredLength <= tolerance)
        return 0;
      if (desiredLength + tolerance >= totalCurveLength)
        return 1;

      var getParameterHelper = GetParameterHelpers2F.Obtain();
      var result = getParameterHelper.GetParameter(curve, desiredLength, maxNumberOfIterations, tolerance);
      GetParameterHelpers2F.Recycle(getParameterHelper);
      
      return result;
    }


    internal static float GetParameter(ICurve<float, Vector3F> curve, float desiredLength, float totalCurveLength, int maxNumberOfIterations, float tolerance)
    {
      if (desiredLength <= tolerance)
        return 0;
      if (desiredLength + tolerance >= totalCurveLength)
        return 1;

      var getParameterHelper = GetParameterHelpers3F.Obtain();
      var result = getParameterHelper.GetParameter(curve, desiredLength, maxNumberOfIterations, tolerance);
      GetParameterHelpers3F.Recycle(getParameterHelper);
      return result;
    }


    /// <summary>
    /// Computes the approximated length of the curve for the parameter interval
    /// [<paramref name="start"/>, <paramref name="end"/>].
    /// </summary>
    /// <param name="curve">The curve.</param>
    /// <param name="start">The parameter value of the start position.</param>
    /// <param name="end">The parameter value of the end position.</param>
    /// <param name="minNumberOfIterations">
    /// The minimum number of iterations which are taken to compute the length.
    /// </param>
    /// <param name="maxNumberOfIterations">
    /// The maximum number of iterations which are taken to compute the length.
    /// </param>
    /// <param name="tolerance">
    /// The tolerance value. This method will return an approximation of the precise length.
    /// The absolute error will be less than this tolerance.
    /// </param>
    /// <returns>
    /// The approximated length of the curve in the given interval.
    /// </returns>
    /// <remarks>
    /// The length is computed with an iterative algorithm. The iterations end when 
    /// the <paramref name="maxNumberOfIterations"/> were performed, or when the 
    /// <paramref name="tolerance"/> criterion is met - whichever comes first.
    /// </remarks>
    internal static float GetLength(ICurve<float, Vector2F> curve, float start, float end, int minNumberOfIterations, int maxNumberOfIterations, float tolerance)
    {
      if (tolerance <= 0)
        throw new ArgumentOutOfRangeException("tolerance", "The tolerance must be greater than zero.");

      var getLengthHelper = GetLengthHelpers.Obtain();
      var length = getLengthHelper.GetLength(curve, start, end, minNumberOfIterations, maxNumberOfIterations, tolerance);
      GetLengthHelpers.Recycle(getLengthHelper);
      return length;
    }


    /// <summary>
    /// Computes the approximated length of the curve for the parameter interval
    /// [<paramref name="start"/>, <paramref name="end"/>].
    /// </summary>
    /// <param name="curve">The curve.</param>
    /// <param name="start">The parameter value of the start position.</param>
    /// <param name="end">The parameter value of the end position.</param>
    /// <param name="minNumberOfIterations">
    /// The minimum number of iterations which are taken to compute the length.
    /// </param>
    /// <param name="maxNumberOfIterations">
    /// The maximum number of iterations which are taken to compute the length.
    /// </param>
    /// <param name="tolerance">
    /// The tolerance value. This method will return an approximation of the precise length.
    /// The absolute error will be less than this tolerance.
    /// </param>
    /// <returns>
    /// The approximated length of the curve in the given interval.
    /// </returns>
    /// <remarks>
    /// The length is computed with an iterative algorithm. The iterations end when 
    /// the <paramref name="maxNumberOfIterations"/> were performed, or when the 
    /// <paramref name="tolerance"/> criterion is met - whichever comes first.
    /// </remarks>
    internal static float GetLength(ICurve<float, Vector3F> curve, float start, float end, int minNumberOfIterations, int maxNumberOfIterations, float tolerance)
    {
      if (tolerance <= 0)
        throw new ArgumentOutOfRangeException("tolerance", "The tolerance must be greater than zero.");

      var getLengthHelper = GetLengthHelpers.Obtain();
      var length = getLengthHelper.GetLength(curve, start, end, minNumberOfIterations, maxNumberOfIterations, tolerance);
      GetLengthHelpers.Recycle(getLengthHelper);
      return length;
    }
  }
}
