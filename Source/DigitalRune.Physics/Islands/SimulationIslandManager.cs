// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Physics.Constraints;


namespace DigitalRune.Physics
{
  /// <summary>
  /// Manages <see cref="SimulationIsland"/>s of a <see cref="Simulation"/>.
  /// </summary>
  public class SimulationIslandManager : IComparer<SimulationIsland>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Simulation _simulation;

    // Islands are found using the union find algorithm. The union finder instance is reset
    // in each frame.
    private readonly UnionFinder _unionFinder;

    // A list where the union elements will be stored and sorted.
    private readonly List<UnionElement> _sortedElements = new List<UnionElement>(256);
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the simulation islands.
    /// </summary>
    /// <value>The simulation islands.</value>
    public ReadOnlyCollection<SimulationIsland> Islands
    {
      get
      {
        if (_islands == null)
          _islands = new ReadOnlyCollection<SimulationIsland>(IslandsInternal);

        return _islands;
      }
    }
    private ReadOnlyCollection<SimulationIsland> _islands;


    /// <summary>
    /// Gets the simulation islands. (For internal use only.)
    /// </summary>
    /// <value>The simulation islands.</value>
    internal List<SimulationIsland> IslandsInternal { get; private set; }


    /// <summary>
    /// Gets or sets the island links that are created by contact sets.
    /// </summary>
    /// <value>The contact set links.</value>
    internal List<Pair<RigidBody>> ContactSetLinks { get; private set; }
    // This list is filled in Simulation.UpdateContacts.
    // Going through all contact constraints is slower because for many objects 
    // there are 4 contact constraints per contact set.

    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationIslandManager"/> class.
    /// </summary>
    /// <param name="simulation">The simulation.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="simulation"/> is <see langword="null"/>.
    /// </exception>
    internal SimulationIslandManager(Simulation simulation)
    {
      if (simulation == null)
        throw new ArgumentNullException("simulation");

      _simulation = simulation;
      _unionFinder = new UnionFinder();
      IslandsInternal = new List<SimulationIsland>();

      ContactSetLinks = new List<Pair<RigidBody>>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Updates the <see cref="Islands"/> list and the <see cref="RigidBody.IslandId"/> in the rigid 
    /// bodies.
    /// </summary>
    internal void Update()
    {
      int numberOfRigidBodies = _simulation.RigidBodies.Count;

      // Reset list and union finder.
      int numberOfIslands = IslandsInternal.Count;
      for (int i = 0; i < numberOfIslands; i++)
        IslandsInternal[i].Recycle();

      IslandsInternal.Clear();
      _unionFinder.Reset(numberOfRigidBodies);

      // First put each body in its own island.
      for (int i = 0; i < numberOfRigidBodies; i++)
        _simulation.RigidBodies[i].IslandId = i;

      // Merge islands where the bodies have contact.
      int numberOfContactConstraints = _simulation.ContactConstraintsInternal.Count;
      int numberOfIslandLinks = ContactSetLinks.Count;
      for (int i = 0; i < numberOfIslandLinks; i++)
      {
        var pair = ContactSetLinks[i];
        var bodyA = pair.First;
        var bodyB = pair.Second;
        if (bodyA.MotionType == MotionType.Dynamic && bodyB.MotionType == MotionType.Dynamic)
        {
          _unionFinder.Unite(bodyA.IslandId, bodyB.IslandId);
        }
      }

      // Merge islands where the bodies are constrained.
      // Note: For some limit constraints this is not necessary if the limit is currently not 
      // active - but the performance win would be minimal.
      int numberOfConstraints = _simulation.Constraints.Count;
      for (int i = 0; i < numberOfConstraints; i++)
      {
        var constraint = _simulation.Constraints[i];
        var bodyA = constraint.BodyA;
        var bodyB = constraint.BodyB;
        if (constraint.Enabled
            && bodyA.MotionType == MotionType.Dynamic
            && bodyB.MotionType == MotionType.Dynamic)
        {
          _unionFinder.Unite(bodyA.IslandId, bodyB.IslandId);
        }
      }

      // Now store final Island IDs in bodies.
      for (int i = 0; i < numberOfRigidBodies; i++)
      {
        var body = _simulation.RigidBodies[i];
        if (body.MotionType == MotionType.Dynamic)
        {
          body.IslandId = _unionFinder.FindUnion(body.IslandId);
        }
        else
        {
          // Set static/kinematic bodies to "no island".
          body.IslandId = -1;
        }
      }

      // Sort union finder elements by their IDs.
      // We use a new list. We could also sort the original array of _unionFinder.Elements.
      // But sorting a new list is faster than sorting the array. (List.Sort seems to 
      // be faster than Array.Sort in this case.)
      // After that all elements of an island are next to each other and
      // the original element index is stored in each Size field. 
      // But union finder methods cannot be used anymore!
      _sortedElements.Clear();
      for (int i = 0; i < numberOfRigidBodies; i++)
      {
        // Store island ID in each ID field and the original index in the Size field.
        _sortedElements.Add(new UnionElement(_simulation.RigidBodies[i].IslandId, i));
      }
      _sortedElements.Sort();

      // Create islands list.
      numberOfIslands = 0;
      int currentIslandId = -1; // Id of island after union find.
      SimulationIsland currentIsland = null;
      for (int i = 0; i < numberOfRigidBodies; i++)
      {
        var element = _sortedElements[i];

        // Get body for this union finder entry.
        // Before sorting the union elements, we stored the index of the body in element.Size.
        int rigidBodyIndex = element.Size;
        var body = _simulation.RigidBodies[rigidBodyIndex];

        // Static bodies have the id -1. We do not create islands for static bodies.
        if (body.IslandId == -1)
          continue;

        // If the element Id is different, we create a new empty island.
        if (element.Id != currentIslandId)
        {
          currentIsland = SimulationIsland.Create();
          currentIsland.Simulation = _simulation;
          currentIslandId = element.Id;
          IslandsInternal.Add(currentIsland);
          numberOfIslands++;
        }

        Debug.Assert(currentIsland != null);
        Debug.Assert(currentIslandId != -1);
        Debug.Assert(numberOfIslands > 0);

        // We change the body's island ID so that it is identical to the index of the island in the
        // Islands list.
        body.IslandId = numberOfIslands - 1;

        // Add body to island.
        currentIsland.RigidBodiesInternal.Add(body);
      }

      // Sort contacts into islands.
      // TODO: This could be optimized if the ContactSetsLinks have references to their contact constraints.
      for (int i = 0; i < numberOfContactConstraints; i++)
      {
        var contact = _simulation.ContactConstraintsInternal[i];

        //if (!contact.Enabled)       // Contacts are always enabled!
        //  continue;

        var islandId = GetIslandId(contact);
        if (islandId >= 0)
          IslandsInternal[islandId].ContactConstraintsInternal.Add(contact);
      }

      // Sort constraints into islands.
      for (int i = 0; i < numberOfConstraints; i++)
      {
        var constraint = _simulation.Constraints[i];

        // Disabled constraints are not added to islands! This way it is not
        // necessary to check the Enabled flag in the constraint solver.
        if (!constraint.Enabled)
          continue;

        var islandId = GetIslandId(constraint);
        if (islandId >= 0)
          IslandsInternal[islandId].ConstraintsInternal.Add(constraint);
      }

      if (_simulation.Settings.EnableMultithreading)
      {
        // When multithreading is enabled, sort islands by size. The biggest islands
        // should be solved first to balance the work across all available threads.
        IslandsInternal.Sort(this);
      }
    }


    /// <summary>
    /// Gets the island id of a constraint
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <returns>
    /// The island ID of a dynamic body of the constraint.
    /// </returns>
    private static int GetIslandId(IConstraint constraint)
    {
      RigidBody bodyAIslandId = constraint.BodyA;

      Debug.Assert(
        bodyAIslandId.IslandId == -1
        || constraint.BodyB.IslandId == -1
        || bodyAIslandId.IslandId == constraint.BodyB.IslandId,
        "Dynamic rigid bodies linked with a constraint need to be in the same island.");

      // Return the island ID of the dynamic object. Static/kinematic objects have island ID -1.
      if (bodyAIslandId.IslandId >= 0)
        return bodyAIslandId.IslandId;
      else
        return constraint.BodyB.IslandId;
    }


    #region ----- IComparer<SimulationIsland> -----

    /// <summary>
    /// Compares two islands by size.
    /// </summary>
    /// <param name="first">The first island.</param>
    /// <param name="second">The second island.</param>
    /// <returns>
    /// A signed integer that indicates the relative size of the simulation islands, as shown in the following table.
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Meaning</description>
    /// </listheader>
    /// <item>
    /// <term>Less than zero</term>
    /// <description>The first island is bigger than the second island.</description>
    /// </item>
    /// <item>
    /// <term>Zero</term>
    /// <description>The first and the second island are equal in size.</description>
    /// </item>
    /// <item>
    /// <term>Greater than zero</term>
    /// <description>The second island is bigger than the first.</description>
    /// </item>
    /// </list>
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    int IComparer<SimulationIsland>.Compare(SimulationIsland first, SimulationIsland second)
    {
      int constraintsInFirst = first.ConstraintsInternal.Count + first.ContactConstraintsInternal.Count;
      int constraintsInSecond = second.ConstraintsInternal.Count + second.ContactConstraintsInternal.Count;
      return constraintsInSecond - constraintsInFirst;
    }
    #endregion
    #endregion
  }
}
