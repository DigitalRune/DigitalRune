// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Analysis
{
  /// <summary>
  /// Performs numerical integration using <i>Explicit Euler</i> (single-precision).
  /// </summary>
  /// <remarks>
  /// See <see cref="OdeIntegratorF"/> for a description of numerical integration of ODE. Using the
  /// Explicit Euler method the new state x1 at time t1 will be computes as: 
  /// x1 = x0 + (t1 - t0) * f(x0, t0).
  /// </remarks>
  public class ExplicitEulerIntegratorF : OdeIntegratorF
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ExplicitEulerIntegratorF"/> class.
    /// </summary>
    /// <param name="firstOrderDerivative">
    /// The function f(x, t) that computes the first order derivative of the vector x (see 
    /// <see cref="OdeIntegratorF.FirstOrderDerivative"/>).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="firstOrderDerivative"/> is <see langword="null"/>.
    /// </exception>
    public ExplicitEulerIntegratorF(Func<VectorF, float, VectorF> firstOrderDerivative)
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
      VectorF result = x0 + dt * d;
      return result;
    }
  }
}
