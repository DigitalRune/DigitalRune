// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Analysis
{
  /// <summary>
  /// A base class for numerical integration strategies for ordinary differential equations (ODE).
  /// (Single-precision)
  /// </summary>
  /// <remarks>
  /// <para> Numerical integration is explained using following example: </para>
  /// <para>
  /// Consider the following ODE: <c>dx/dt = f(x, t)</c>. The goal of the numerical integration is 
  /// to compute the state x1 at time t1 when following information is given:
  /// <list type="bullet">
  /// <item>The state at time t0. In general, this state is an n-dimensional vector.</item>
  /// <item>The function f(x, t) that computes the first order derivative of x. </item>
  /// </list>
  /// </para>
  /// <para>
  /// <strong>Note:</strong> In this documentation we assume that the integration variable is
  /// <i>time</i> since this is very common for simulation tasks. Of course the integration variable
  /// can be any other quantity.
  /// </para>
  /// <para>
  /// The function f that computes the first order derivative depends on the state x and the time 
  /// t: For example, the state in rigid body simulation consist of the positions and velocities of
  /// the rigid bodies. When computing the new state of the simulation, the first order derivatives
  /// (velocities and accelerations) depend on the whole state and on time. This is because
  /// accelerations are computed through forces which depend on time (for example explosions),
  /// depend on velocities (damping forces) or depend on positions (spring forces).
  /// </para>
  /// </remarks>
  public abstract class OdeIntegratorF
  {
    /// <summary>
    /// Gets the function f(x, t) that computes the first order derivative.
    /// </summary>
    /// <value>The function f(x, t) that computes the first order derivative.</value>
    /// <remarks>
    /// The function has the form <c>VectorF Function(VectorF x, float time)</c>, 
    /// where x is the state vector for the given time. The function returns the first order
    /// derivative of state x for the given time.
    /// </remarks>
    public Func<VectorF, float, VectorF> FirstOrderDerivative { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="OdeIntegratorF"/> class.
    /// </summary>
    /// <param name="firstOrderDerivative">
    /// The function f(x, t) that computes the first order derivative of the vector x 
    /// (see <see cref="OdeIntegratorF.FirstOrderDerivative"/>).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="firstOrderDerivative"/> is <see langword="null"/>.
    /// </exception>
    protected OdeIntegratorF(Func<VectorF, float, VectorF> firstOrderDerivative)
    {
      if (firstOrderDerivative == null)
        throw new ArgumentNullException("firstOrderDerivative");

      FirstOrderDerivative = firstOrderDerivative;
    }


    /// <summary>
    /// Computes the new state x1 at time t1.
    /// </summary>
    /// <param name="x0">The state x0 at time t0.</param>
    /// <param name="t0">The time t0.</param>
    /// <param name="t1">The target time t1 for which the new state x1 is computed.</param>
    /// <returns>The new state x1 at time t1.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public abstract VectorF Integrate(VectorF x0, float t0, float t1);
  }
}
