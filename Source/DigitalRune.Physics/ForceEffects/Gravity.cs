// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Applies a gravity force to rigid bodies.
  /// </summary>
  /// <remarks>
  /// Gravity pulls all bodies into a predefined direction, which is usually "down".
  /// </remarks>
  public class Gravity : ForceField
  {
    // Notes:
    // If the stacking optimization is toggled in a body, the island is kept awake.
    // Maybe there is a method to gradually apply the reduced gravity so that no harsh
    // force discontinuities occur.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Vector3F _direction;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the gravity acceleration vector.
    /// </summary>
    /// <value>The acceleration vector. The default value is <c>(0, -9.81, 0)</c>.</value>
    public Vector3F Acceleration
    {
      get { return _acceleration; }
      set
      {
        _acceleration = value;

        _direction = _acceleration;
        _direction.TryNormalize();
      }
    }
    private Vector3F _acceleration;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Gravity"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Gravity"/> class.
    /// </summary>
    /// <remarks>
    /// The property <see cref="ForceField.AreaOfEffect"/> is initialized with a new instance of
    /// <see cref="GlobalAreaOfEffect"/>.
    /// </remarks>
    public Gravity()
    {
      Initialize();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Gravity"/> class.
    /// </summary>
    /// <param name="areaOfEffect">The area of effect.</param>
    public Gravity(IAreaOfEffect areaOfEffect)
      : base(areaOfEffect)
    {
      Initialize();
    }


    private void Initialize()
    {
      Acceleration = new Vector3F(0, -9.81f, 0);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override void Apply(RigidBody body)
    {
      if (body == null)
        throw new ArgumentNullException("body", "Rigid body in area of effect must not be null.");

      bool isInStack = false;

      // Stacking optimization: 
      // If the body is in an island with at least 5 bodies and a contact applies a force against
      // the gravity direction, the body is considered as "in a stack" and a reduced gravity
      // is applied.
      float stackingOptimization = Simulation.Settings.Constraints.StackingFactor;
      if (stackingOptimization > 0
          && body.IslandId >= 0 
          && body.IslandId < Simulation.IslandManager.IslandsInternal.Count
          && Simulation.IslandManager.IslandsInternal[body.IslandId].RigidBodiesInternal.Count > 5)
      {
        var contactSets = Simulation.CollisionDomain.GetContacts(body.CollisionObject);
        foreach (var contactSet in contactSets)
        {
          // Check if all contact normals point against the gravity direction.
          Vector3F gravityDirection = (contactSet.ObjectA == body.CollisionObject) ? _direction : -_direction;
          foreach (var contact in contactSet)
          {
            if (Vector3F.Dot(contact.Normal, gravityDirection) <= 0.999f)
            {
              // Wrong normal. No stack.
              isInStack = false;
              break;
            }
            else
            {
              isInStack = true;
            }
          }
          // If isInStack is set, then all contact act against gravity - a stacking situation.
          if (isInStack)
            break;
        }
      }

      // Apply normal gravity or reduced gravity for the stacking optimization.
      if (!isInStack)
      {
        AddForce(body, Acceleration * body.MassFrame.Mass);
      }
      else
      {
        AddForce(body, Acceleration * body.MassFrame.Mass / (stackingOptimization + 1));
      }
    }
    #endregion
  }
}
