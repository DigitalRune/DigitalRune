// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Analysis
{
  /// <summary>
  /// Performs numerical integration using <i>Midpoint method</i> (single-precision).
  /// </summary>
  /// <remarks>
  /// See <see cref="OdeIntegratorF"/> for a description of numerical integration of ODE.
  /// </remarks>
  public class MidpointIntegratorF : OdeIntegratorF
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MidpointIntegratorF"/> class.
    /// </summary>
    /// <param name="firstOrderDerivative">
    /// The function f(x, t) that computes the first order derivative of the vector x (see 
    /// <see cref="OdeIntegratorF.FirstOrderDerivative"/>).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="firstOrderDerivative"/> is <see langword="null"/>.
    /// </exception>
    public MidpointIntegratorF(Func<VectorF, float, VectorF> firstOrderDerivative)
      : base(firstOrderDerivative)
    {
    }


    /// <summary>
    /// Computes the new state x1 at time t1.
    /// </summary>
    /// <param name="x0">The state x0 at time t0.</param>
    /// <param name="t0">The time t0.</param>
    /// <param name="t1">The target time t1 for which the new state x1 is computed.</param>
    /// <returns>The new state x1 at time t1.</returns>
    public override VectorF Integrate(VectorF x0, float t0, float t1)
    {
      float dt = (t1 - t0);
      VectorF d = FirstOrderDerivative(x0, t0);
      VectorF midPoint = x0 + dt / 2 * d;
      d = FirstOrderDerivative(midPoint, t0 + dt / 2);
      VectorF result = x0 + dt * d;
      return result;
    }
  }
}
