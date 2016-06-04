// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Geometry.Collisions.Algorithms;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// Provides methods and settings for collision detection.
  /// </summary>
  public class CollisionDetection
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the collision algorithm matrix.
    /// </summary>
    /// <value>The algorithm matrix.</value>
    /// <remarks>
    /// Depending on the <see cref="Shape"/> of a <see cref="CollisionObject"/> different algorithms
    /// have to be used. This matrix determines which algorithms should be used.
    /// </remarks>
    public CollisionAlgorithmMatrix AlgorithmMatrix { get; private set; }


    /// <summary>
    /// Gets or sets the contact position tolerance.
    /// </summary>
    /// <value>The contact position tolerance. The default value is 0.01.</value>
    /// <remarks>
    /// This constant is required for contact persistence. When touching or penetrating objects
    /// move, existing contacts are updated. If the contact position moves less than this value, the
    /// contact lives on. If the contact position change is larger, the contact is removed.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float ContactPositionTolerance
    {
      get { return _contactPositionTolerance; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "Contact position tolerance must be greater than or equal to 0.");

        _contactPositionTolerance = value;
      }
    }
    private float _contactPositionTolerance;


    /// <summary>
    /// Gets or sets the mode of the continuous collision detection.
    /// </summary>
    /// <value>
    /// The mode of the continuous collision detection. The default value is 
    /// <see cref="Collisions.ContinuousCollisionDetectionMode.Linear"/>.
    /// </value>
    public ContinuousCollisionDetectionMode ContinuousCollisionDetectionMode { get; set; }

    
    /// <summary>
    /// Gets or sets the collision epsilon (numerical tolerance value).
    /// </summary>
    /// <value>
    /// The collision epsilon (numerical tolerance value). The default value is 0.001.
    /// </value>
    /// <remarks>
    /// Some collision algorithms are iterative and end when the error is less than this value.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float Epsilon 
    {
      get { return _epsilon; }
      set 
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "Collision tolerance 'epsilon' must be greater than or equal to 0.");

        _epsilon = value;
      }
    }
    private float _epsilon;


    /// <summary>
    /// Gets or sets the collision filter for contact queries.
    /// </summary>
    /// <value>
    /// The collision filter (can be <see langword="null"/> to enable all collisions and disable 
    /// filtering). The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This filter can be used to disable collision detection between selected collision objects.
    /// This filter is ignored in closest-point queries (see <see cref="GetClosestPoints"/> and
    /// <see cref="UpdateClosestPoints"/>). It is used for <see cref="HaveAabbContact"/>,
    /// <see cref="HaveContact"/>, <see cref="GetContacts"/> and <see cref="UpdateContacts"/>.
    /// </para>
    /// <para>
    /// Per default, this property is <see langword="null"/> and all collisions are enabled. If 
    /// collision filtering is needed, <see cref="CollisionFilter"/> or a custom implementation of 
    /// <see cref="IPairFilter{CollisionObject}"/> can be used.
    /// </para>
    /// </remarks>
    public IPairFilter<CollisionObject> CollisionFilter
    {
      get { return _collisionFilter; }
      set 
      {
        if (_collisionFilter == value)
          return;

        if (_collisionFilter != null)
          _collisionFilter.Changed -= OnCollisionFilterChanged;
        
        _collisionFilter = value;

        if (_collisionFilter != null)
          _collisionFilter.Changed += OnCollisionFilterChanged;

        OnCollisionFilterChanged(this, EventArgs.Empty);
      }
    }
    private IPairFilter<CollisionObject> _collisionFilter;


    /// <summary>
    /// Gets or sets the contact filter.
    /// </summary>
    /// <value>
    /// The contact filter. Can be <see langword="null"/> to disable contact filtering. The default
    /// filter is a <see cref="ContactReducer"/>.
    /// </value>
    /// <remarks>
    /// Contact filters are called in the narrow phase (in <see cref="CollisionAlgorithm"/>s) to
    /// post-process the found contacts. Example usages of a contact filter:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Remove redundant contacts. Some applications, like rigid body physics, needs only a minimal
    /// set of contacts, e.g. only 4 contacts per <see cref="ContactSet"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Remove "bad" contacts, for example contacts where the normal direction points into an
    /// undesired direction.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Merge contacts. For some applications it is useful to keep only one contact which is the
    /// average of all other contacts.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public IContactFilter ContactFilter { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the full contact set should be found per frame.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the full contact set should be found per frame; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Some <see cref="CollisionAlgorithm"/>s compute only 1 new <see cref="Contact"/> when they
    /// are executed. More contacts are added in upcoming collision tests. If 
    /// <see cref="FullContactSetPerFrame"/> is set to <see langword="true"/>, some algorithms will
    /// perform enhanced tests that find more than 1 contacts with each call.
    /// </para>
    /// </remarks>
    public bool FullContactSetPerFrame { get; set; }


    /// <summary>
    /// Occurs when the <see cref="CollisionFilter"/> changed. (This event is implemented as a
    /// <i>weak event</i>.)
    /// </summary>
    /// <remarks>
    /// This event is triggered when the <see cref="CollisionFilter"/> property was changed or when
    /// the current <see cref="CollisionFilter"/> triggered a Changed event.
    /// </remarks>
    internal event EventHandler<EventArgs> CollisionFilterChanged
    {
      add { _collisionFilterChangedEvent.Add(value); }
      remove { _collisionFilterChangedEvent.Remove(value); }
    }
    private readonly WeakEvent<EventHandler<EventArgs>> _collisionFilterChangedEvent = new WeakEvent<EventHandler<EventArgs>>();
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionDetection"/> class.
    /// </summary>
    public CollisionDetection()
    {
      Epsilon = 0.001f;
      ContactPositionTolerance = 0.01f;
      ContactFilter = new ContactReducer();
      ContinuousCollisionDetectionMode = ContinuousCollisionDetectionMode.Linear;

      // This property must be updated last because it uses "this".
      AlgorithmMatrix = new CollisionAlgorithmMatrix(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns <see langword="true"/> if the axis-aligned bounding boxes (AABBs) of two
    /// <see cref="CollisionObject"/>s are in contact.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <returns>
    /// <see langword="true"/> if the objects' AABBs are touching or intersecting; otherwise
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Only the (automatic computed) axis-aligned bounding boxes are tested - not the exact
    /// geometry of the objects. For an exact test, call <see cref="HaveContact"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public bool HaveAabbContact(CollisionObject objectA, CollisionObject objectB)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      // Collision filtering 
      if (objectA.Enabled == false || objectB.Enabled == false 
          || (CollisionFilter != null && !CollisionFilter.Filter(new Pair<CollisionObject>(objectA, objectB))))
      {
        return false;
      }

      // AABB test
      return GeometryHelper.HaveContact(objectA.GeometricObject.Aabb, objectB.GeometricObject.Aabb);
    }


    /// <summary>
    /// Returns <see langword="true"/> if two <see cref="CollisionObject"/>s are in contact.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <returns>
    /// <see langword="true"/> if the object are touching or intersecting; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    public bool HaveContact(CollisionObject objectA, CollisionObject objectB)
    {
      // Broad phase AABB check and collision filtering
      if (HaveAabbContact(objectA, objectB) == false)
        return false;

      // Narrow phase
      return AlgorithmMatrix[objectA, objectB].HaveContact(objectA, objectB);
    }


    /// <summary>
    /// Computes the closest points between two <see cref="CollisionObject"/>s.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <returns>
    /// The <see cref="ContactSet"/> with the closest-point information. The 
    /// <see cref="ContactSet"/> will have exactly 1 <see cref="Contact"/> (describing the
    /// closest-point pair).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Collision filtering (see <see cref="CollisionFilter"/>) is NOT applied.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    public ContactSet GetClosestPoints(CollisionObject objectA, CollisionObject objectB)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      return AlgorithmMatrix[objectA, objectB].GetClosestPoints(objectA, objectB);
    }


    /// <summary>
    /// Computes the contacts between two <see cref="CollisionObject"/>s.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <returns>
    /// A <see cref="ContactSet"/> describing the contact information if <paramref name="objectA"/>
    /// and <paramref name="objectB"/> are intersecting; otherwise, <see langword="null"/> if the
    /// objects are separated.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    public ContactSet GetContacts(CollisionObject objectA, CollisionObject objectB)
    {
      // Broad phase AABB check and collision filtering
      if (HaveAabbContact(objectA, objectB) == false)
        return null;

      // Narrow phase
      ContactSet contactSet = AlgorithmMatrix[objectA, objectB].GetContacts(objectA, objectB);

      Debug.Assert(contactSet != null, "CollisionAlgorithm.GetContacts should always return a ContactSet.");
      if (contactSet.HaveContact)
      {
        return contactSet;
      }
      else
      {
        contactSet.Recycle();
        return null;
      }
    }


    /// <summary>
    /// Gets the time of impact between two moving objects.
    /// </summary>
    /// <param name="objectA">The object A.</param>
    /// <param name="targetPoseA">The target pose of A.</param>
    /// <param name="objectB">The object B.</param>
    /// <param name="targetPoseB">The target pose of B.</param>
    /// <param name="allowedPenetration">
    /// The allowed penetration. A positive allowed penetration value makes sure that the objects 
    /// have a measurable contact at the time of impact.
    /// </param>
    /// <returns>The time of impact in the range [0, 1].</returns>
    /// <remarks>
    /// <para>
    /// Both objects are moved from their current pose (time = 0) to the given target pose (time =
    /// 1). If they collide during this movement the first time of impact is returned. A time of
    /// impact of 1 can mean that the objects do not collide or they collide at their target
    /// positions.
    /// </para>
    /// <para>
    /// The result is undefined if the objects are already in contact at their start poses.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> or <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    public float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      return AlgorithmMatrix[objectA, objectB].GetTimeOfImpact(objectA, targetPoseA, objectB, targetPoseB, allowedPenetration);
    }


    /// <summary>
    /// Updates the closest-point information in the given contact set.
    /// </summary>
    /// <param name="contactSet">
    /// The contact set containing the last known closest-point information.
    /// </param>
    /// <param name="deltaTime">
    /// The time step size in seconds. (The elapsed simulation time since 
    /// <see cref="UpdateClosestPoints"/> or <see cref="UpdateContacts"/> was last called for this
    /// contact set.)
    /// </param>
    /// <remarks>
    /// <para>
    /// If two objects move, the closest-point information will usually change and has to be
    /// updated. Using the contact set containing the last known closest points, this method can
    /// compute the new closest points faster than <see cref="GetClosestPoints"/> if the poses of
    /// the objects haven't changed drastically.
    /// </para>
    /// <para>
    /// Collision filtering (see <see cref="CollisionFilter"/>) is NOT applied.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="contactSet"/> is <see langword="null"/>.
    /// </exception>
    public void UpdateClosestPoints(ContactSet contactSet, float deltaTime)
    {
      if (contactSet == null)
        throw new ArgumentNullException("contactSet");

      AlgorithmMatrix[contactSet].UpdateClosestPoints(contactSet, deltaTime);
    }


    /// <summary>
    /// Updates the contact information in the given contact set.
    /// </summary>
    /// <param name="contactSet">The contact set containing the last known contacts.</param>
    /// <param name="deltaTime">
    /// The time step size in seconds. (The elapsed simulation time since 
    /// <see cref="UpdateClosestPoints"/> or <see cref="UpdateContacts"/> was last called for this
    /// contact set.)
    /// </param>
    /// <remarks>
    /// If two objects move, the contact information will usually change and has to be updated.
    /// Using the contact set containing the last known contacts, this method can compute the new
    /// contacts faster than <see cref="GetContacts"/> if the poses of the objects haven't changed
    /// drastically.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="contactSet"/> is <see langword="null"/>.
    /// </exception>
    public void UpdateContacts(ContactSet contactSet, float deltaTime)
    {
      if (contactSet == null)
        throw new ArgumentNullException("contactSet");

      // Broad phase AABB check and collision filtering
      if (HaveAabbContact(contactSet.ObjectA, contactSet.ObjectB))
      {
        // Narrow phase
        AlgorithmMatrix[contactSet].UpdateContacts(contactSet, deltaTime);
      }
      else
      {
        foreach (var contact in contactSet)
          contact.Recycle();

        contactSet.Clear();
        contactSet.HaveContact = false;
      }
    }


    /// <summary>
    /// Raises the <see cref="CollisionFilterChanged"/> event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="EventArgs"/> instance containing the event data.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnCollisionFilterChanged"/>
    /// in a derived class, be sure to call the base class's <see cref="OnCollisionFilterChanged"/> 
    /// method so that registered delegates receive the event.
    /// </remarks>
    internal void OnCollisionFilterChanged(object sender, EventArgs eventArgs)
    {
      _collisionFilterChangedEvent.Invoke(this, eventArgs);
    }
    #endregion
  }
}
