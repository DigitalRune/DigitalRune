// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Solves constraints (joints, contacts).
  /// </summary>
  internal abstract class ConstraintSolver
  {
    /// <summary>
    /// Gets the simulation.
    /// </summary>
    /// <value>The simulation.</value>
    public Simulation Simulation { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintSolver"/> class.
    /// </summary>
    /// <param name="simulation">The simulation.</param>
    protected ConstraintSolver(Simulation simulation)
    {
      Debug.Assert(simulation != null);
      Simulation = simulation;
    }


    /// <summary>
    /// Solves all constraints of the specified island.
    /// </summary>
    /// <param name="island">The simulation island.</param>
    /// <param name="deltaTime">The time step size.</param>
    public abstract void Solve(SimulationIsland island, float deltaTime);
  }
}
