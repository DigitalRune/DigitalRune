// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// Represents an object which can collide with other objects.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="CollisionObject"/> contains an <see cref="IGeometricObject"/> and adds 
  /// information for the collision detection system.
  /// </para>
  /// <para>
  /// The <see cref="CollisionDetection"/> provides methods to can make collision queries between 
  /// two <see cref="CollisionObject"/>s. When collisions between multiple 
  /// <see cref="CollisionObject"/>s need to be computed, it is more efficient to manage 
  /// <see cref="CollisionObject"/>s in a <see cref="CollisionDomain"/>. A collision domain will 
  /// cache data to speed up collision detection. A <see cref="CollisionObject"/> can only belong to
  /// one <see cref="CollisionDomain"/> at a time.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> A <see cref="CollisionObject"/> registers event handlers for 
  /// <see cref="IGeometricObject.PoseChanged"/> and <see cref="IGeometricObject.ShapeChanged"/> of
  /// the contained <see cref="IGeometricObject"/>. Therefore, an <see cref="IGeometricObject"/>
  /// will have an indirect reference to its <see cref="CollisionObject"/>. When the 
  /// <see cref="CollisionObject"/> is no longer used the property <see cref="GeometricObject"/>
  /// should be set to <see langword="null"/> which unregisters the event handlers. This is
  /// necessary in order to avoid potential memory leaks.
  /// </para>
  /// </remarks>
  public class CollisionObject
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private enum ShapeType
    {
      Default,
      Ray,
      RayThatStopsAtFirstHit,
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Cached shape info to avoid casting the shape to find out if it is a ray.
    // Is set in OnShapeChanged().
    private ShapeType _shapeType;

    // Cache shape because in OnShapeChanged we need to be able to check if the type
    // has changed.
    private Shape _shape;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    internal bool ShapeTypeChanged = true;

    /// <summary>
    /// Gets the <see cref="Domain"/>.
    /// </summary>
    /// <value>The <see cref="CollisionDomain"/>.</value>
    /// <remarks>
    /// This property is automatically set when the <see cref="CollisionObject"/> is added to a
    /// <see cref="CollisionDomain"/>.
    /// </remarks>
    public CollisionDomain Domain
    {
      get { return _domain; }
      internal set
      {
        _domain = value;
        Changed = true;
        
        ShapeTypeChanged = true;
      }
    }
    private CollisionDomain _domain;


    /// <summary>
    /// Gets or sets the geometric object.
    /// </summary>
    /// <value>The geometric object.</value>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> The property <see cref="GeometricObject"/> should be set to 
    /// <see langword="null"/> if the <see cref="CollisionObject"/> is no longer used. The 
    /// <see cref="CollisionObject"/> handles the events of the <see cref="IGeometricObject"/> 
    /// (<see cref="IGeometricObject.PoseChanged"/> and <see cref="IGeometricObject.ShapeChanged"/>).
    /// Therefore, the <see cref="IGeometricObject"/> instance has implicit strong references to the
    /// <see cref="CollisionObject"/>. The <see cref="CollisionObject"/> cannot be garbage collected
    /// as long the <see cref="IGeometricObject"/> instance is alive. The 
    /// <see cref="CollisionObject"/> can be garbage collected if the <see cref="IGeometricObject"/> 
    /// instance can be garbage collected or this property is <see langword="null"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public IGeometricObject GeometricObject
    {
      get { return _geometricObject; }
      set
      {
        if (_domain != null && value == null)
          throw new ArgumentNullException("value", "CollisionObject.GeometricObject must not be set to null as long as the CollisionObject is part of a CollisionDomain.");

        if (_geometricObject != value)
        {
          if (_geometricObject != null)
          {
            _geometricObject.ShapeChanged -= OnShapeChanged;
            _geometricObject.PoseChanged -= OnPoseChanged;
          }

          _geometricObject = value;

          if (_geometricObject != null)
          {
            _geometricObject.ShapeChanged += OnShapeChanged;
            _geometricObject.PoseChanged += OnPoseChanged;
          }

          // OnPoseChanged does not do any relevant work.
          //OnPoseChanged();

          // OnShapeChanged does relevant work and must be called.
          OnShapeChanged(null, ShapeChangedEventArgs.Empty);

          Changed = true;
        }
      }
    }
    private IGeometricObject _geometricObject;


    ///// <summary>
    ///// Gets or sets the application data.
    ///// </summary>
    ///// <value>The application data.</value>
    ///// <remarks>
    ///// This property can store data of the application that uses the collision detection library.
    ///// This property is not used by the collision detection library itself.
    ///// </remarks>
    //public object ApplicationData { get; set; }


    ///// <summary>
    ///// Gets or sets the user data.
    ///// </summary>
    ///// <value>The user data.</value>
    ///// <remarks>
    ///// This property can store end-user data.
    ///// This property is not used by the collision detection library itself.
    ///// </remarks>
    //public object UserData { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the data has changed in a way
    /// such that the cached contact info is invalid.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if modified; otherwise, <see langword="false"/>. This value is 
    /// initially set to <see langword="true"/> to indicate that it has not been processed by a 
    /// <see cref="CollisionDomain"/>.
    /// </value>
    /// <remarks>
    /// This flag is automatically reset by the <see cref="CollisionDomain"/>. 
    /// </remarks>
    internal bool Changed { get; set; }


    /// <summary>
    /// Gets or sets the collision object type.
    /// </summary>
    /// <value>
    /// The collision object type.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
    public CollisionObjectType Type
    {
      get { return _type; }
      set
      {
        if (value != _type)
        {
          _type = value;
          Changed = true;
        }
      }
    }
    private CollisionObjectType _type;


    /// <summary>
    /// Gets or sets the collision group ID.
    /// </summary>
    /// <value>The collision group ID.</value>
    /// <remarks>
    /// Each collision group is represented by an integer number. Collision groups are used to group
    /// collision objects of the same type. Some collision filter implementations (see 
    /// <see cref="CollisionDetection.CollisionFilter"/> and <see cref="CollisionFilter"/>) let you
    /// control the collision filtering for whole groups (additionally to individual collision
    /// objects).
    /// </remarks>
    public int CollisionGroup { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="CollisionObject"/> is enabled.
    /// </summary>
    /// <value><see langword="true"/> if enabled; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// Disabled <see cref="CollisionObject"/>s will not collide with anything. Use this flag if the
    /// collision object should be temporarily disabled. If the collision object should be disabled
    /// for a longer period, it is more efficient to remove the object from the
    /// <see cref="CollisionDomain.CollisionObjects"/> of the <see cref="CollisionDomain"/> and
    /// re-add the object to the collision domain when it is needed again.
    /// </remarks>
    public bool Enabled { get; set; }


    /// <summary>
    /// Gets a value indicating whether this instance is ray that stops at first hit.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is ray that stops at first hit; otherwise, <see langword="false"/>.
    /// </value>
    internal bool IsRayThatStopsAtFirstHit { get { return _shapeType == ShapeType.RayThatStopsAtFirstHit; } }


    /// <summary>
    /// Gets a value indicating whether this instance is ray.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is ray; otherwise, <see langword="false"/>.
    /// </value>
    internal bool IsRay { get { return _shapeType != ShapeType.Default; } }


    // Contact sets of a collision object are stored in linked lists. The collision 
    // object stores the head of the list. The list is managed completely by the 
    // ContactSetCollection!
    internal ContactSet ContactSets;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionObject"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionObject"/> class.
    /// </summary>
    internal CollisionObject()
      : this(new GeometricObject())
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionObject"/> class with the given
    /// geometric object.
    /// </summary>
    /// <param name="geometricObject">
    /// The geometric object (see property <see cref="GeometricObject"/>).
    /// </param>
    public CollisionObject(IGeometricObject geometricObject)
    {
      GeometricObject = geometricObject;
      Enabled = true;
      Changed = true;
      Type = CollisionObjectType.Default;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Internal -----

    // The following methods are used internally by the collision detection to make direct 
    // changes to a CollisionObject during collision checks.

    /// <summary>
    /// Copies the data from the specified <see cref="CollisionObject"/> and sets the specified
    /// <see cref="IGeometricObject"/>. (For internal use only.)
    /// </summary>
    /// <param name="collisionObject">The collision object.</param>
    /// <param name="geometricObject">The geometric object.</param>
    internal void SetInternal(CollisionObject collisionObject, IGeometricObject geometricObject)
    {
      Changed = collisionObject.Changed;
      CollisionGroup = collisionObject.CollisionGroup;
      _domain = collisionObject._domain;
      Enabled = collisionObject.Enabled;
      _geometricObject = geometricObject;
      _type = collisionObject._type;
      _shapeType = collisionObject._shapeType;
      _shape = geometricObject.Shape;
      ShapeTypeChanged = collisionObject.ShapeTypeChanged;
    }


    /// <summary>
    /// Resets the collision object. (For internal use only.)
    /// </summary>
    internal void ResetInternal()
    {
      Changed = true;
      CollisionGroup = 0;
      _domain = null;
      Enabled = true;
      _geometricObject = null;
      _type = CollisionObjectType.Default;
      _shapeType = ShapeType.Default;
      _shape = null;
      ShapeTypeChanged = true;
    }
    #endregion


    /// <summary>
    /// Called when the pose was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="EventArgs"/> instance containing the event data.
    /// </param>
    private void OnPoseChanged(object sender, EventArgs eventArgs)
    {
      Changed = true;
    }


    /// <summary>
    /// Called when the shape was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="ShapeChangedEventArgs"/> instance containing the event data.
    /// </param>
    private void OnShapeChanged(object sender, ShapeChangedEventArgs eventArgs)
    {
      Changed = true;

      var shape = (_geometricObject != null) ? _geometricObject.Shape : null;

      // Check if shape type was changed (which invalidates the cached collision algos).
      // Instead of comparing the types, we simply compare the instances.
      ShapeTypeChanged = ShapeTypeChanged || (_shape != shape);

      // Remember current shape.
      _shape = shape;

      RayShape ray = shape as RayShape;
      if (ray != null)
        _shapeType = ray.StopsAtFirstHit ? ShapeType.RayThatStopsAtFirstHit : ShapeType.Ray;
      else
        _shapeType = ShapeType.Default;

      if (_domain == null)
        return;

      Debug.Assert(_domain != null);

      // Clear existing contact info only.
      int feature = eventArgs.Feature;
      if (feature == -1)
      {
        // Invalidate all contacts that contain this collision object.
        foreach (var contactSet in _domain.ContactSets.GetContacts(this))
        {
          foreach (var contact in contactSet)
            contact.Recycle();

          contactSet.Clear();
          contactSet.IsValid = false;
        }
        return;
      }

      Debug.Assert(feature >= 0);

      // Remove only the contacts of the given feature.
      foreach (var contactSet in _domain.ContactSets.GetContacts(this))
      {
        if (contactSet.ObjectA == this)
        {
          for (int i = contactSet.Count - 1; i >= 0; i--)
          {
            Contact contact = contactSet[i];
            if (contact.FeatureA == -1 || contact.FeatureA == feature)
            {
              contactSet.RemoveAt(i);
              contact.Recycle();
            }
          }
        }
        else
        {
          Debug.Assert(contactSet.ObjectB == this);
          for (int i = contactSet.Count - 1; i >= 0; i--)
          {
            Contact contact = contactSet[i];
            if (contact.FeatureB == -1 || contact.FeatureB == feature)
            {
              contactSet.RemoveAt(i);
              contact.Recycle();
            }
          }
        }

        contactSet.IsValid = false;
      }
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "CollisionObject {{ {0} }}", GeometricObject);
    }
    #endregion
  }
}
