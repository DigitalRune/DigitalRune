// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Statistics;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Solves constraints using <i>Sequential Impulses</i>.
  /// </summary>
  internal sealed class SequentialImpulseBasedSolver : ConstraintSolver
  {
    // Constraint Ordering: 
    //   Tokamak: builds a directed graph of contacts pointing away from the gravity vector. And 
    //            contact constraints are solve in that order. 
    //   Henge3D: sorts constraints by the z values of the related bodies.
    //   Pulsk:   Adds forces of all bodies which gives the gravity or main acceleration direction 
    //            and sorts bodies and contacts in this direction.
    //
    // In our experiments randomization of constraints is more stable in general. z-sorting
    // is very good for a single stack. But in a high wall you see waves going through the 
    // wall from bottom to top. When using randomization these directed waves do not occur.

    // Following quick-and-dirty RNG is from Numerical Recipes and could replace Random in this 
    // class. BUT: This is slower on the Xbox! Maybe a bit faster in Windows...
    // If this is used, there must be one RNG per island or the access must be thread-safe.
    //private uint _randomNumber = 55711;
    //int NextRandom(int maxIncluded)
    //{
    //  _randomNumber = 1664525 * _randomNumber + 1013904223;
    //  return (int)(_randomNumber % (maxIncluded + 1));
    //}


    /// <summary>
    /// Initializes a new instance of the <see cref="SequentialImpulseBasedSolver"/> class.
    /// </summary>
    /// <param name="simulation">The simulation.</param>
    public SequentialImpulseBasedSolver(Simulation simulation)
      : base(simulation)
    {
    }


    /// <inheritdoc/>
    public override void Solve(SimulationIsland island, float deltaTime)
    {
      // Abort if the island is empty.
      int numberOfContacts = island.ContactConstraintsInternal.Count;
      int numberOfConstraints = island.ConstraintsInternal.Count;
      if (numberOfContacts == 0 && numberOfConstraints == 0)
        return;

      // We set stacking tolerance value for each contact constraint. Set value to 0 if there are 
      // not many objects. A non-zero value can create non-smooth rolling movement or additional unwanted torque.
      float stackingTolerance = (island.RigidBodies.Count < 6) ? 0 : Simulation.Settings.Constraints.StackingTolerance;

      // Setup contact constraints.
      for (int i = 0; i < numberOfContacts; i++)
      {
        var contact = island.ContactConstraintsInternal[i];
        contact.StackingTolerance = stackingTolerance;
        contact.Setup();
      }

      // Setup other constraints.
      for (int i = 0; i < numberOfConstraints; i++)
      {
        var constraint = island.ConstraintsInternal[i];
        constraint.Setup();
      }
      

      // Randomly reorder constraints. This improves convergence of the solver but the reordering
      // can cost a bit of performance and the reordered constraints could be less cache optimal.
      RandomizeConstraints(island);

      // Apply impulses iteratively.
      int numberOfIterations = Simulation.Settings.Constraints.NumberOfConstraintIterations;
      for (int iteration = 0; iteration < numberOfIterations; iteration++)
      {
        bool impulseWasApplied = false;

        // Apply impulses at contacts.
        for (int i = 0; i < numberOfContacts; i++)
        {
          var contact = island.ContactConstraintsInternal[i];
          bool result = contact.ApplyImpulse();
          impulseWasApplied = impulseWasApplied || result;
        }

        // Apply impulses of non-contact constraints.
        for (int i = 0; i < numberOfConstraints; i++)
        {
          var constraint = island.ConstraintsInternal[i];
          bool result = constraint.ApplyImpulse();
          impulseWasApplied = impulseWasApplied || result;
        }

        // If no constraint in the island applied a significant impulse, we can abort and skip
        // the rest of the iterations.
        if (!impulseWasApplied)
        {
          // Note: If this early-out introduces problems we could early-out after
          // n loops where no impulse was applied.
          break;
        }
      }
    }


    private void RandomizeConstraints(SimulationIsland island)
    {
      if (Simulation.Settings.Constraints.RandomizeConstraints)
      {
        // RNGs are stored per island because islands are processes in parallel (if multithreading
        // is enabled) and this needs to be thread-safe and deterministic.
        var random = island.Random;
        if (random == null)
        {
          random = new Random(55711);
          island.Random = random;
        }

        // Randomize constraints using the Fisher–Yates shuffle.
        // (See http://en.wikipedia.org/wiki/Fisher-Yates_shuffle.)
        for (int i = island.ContactConstraintsInternal.Count - 1; i >= 1; i--)
        {
          var contact = island.ContactConstraintsInternal[i];
          //var j = NextRandom(i);  
          var j = random.NextInteger(0, i); 
          island.ContactConstraintsInternal[i] = island.ContactConstraintsInternal[j];
          island.ContactConstraintsInternal[j] = contact;
        }
        for (int i = island.ConstraintsInternal.Count - 1; i >= 1; i--)
        {
          var constraint = island.ConstraintsInternal[i];
          //var j = NextRandom(i);
          var j = random.NextInteger(0, i);
          island.ConstraintsInternal[i] = island.ConstraintsInternal[j];
          island.ConstraintsInternal[j] = constraint;
        }
      }
    }
  }
}
