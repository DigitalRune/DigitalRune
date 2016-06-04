// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// Describes a contact (or the closest points) of two objects.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="Contact"/> is the result of a collision query. <see cref="Contact"/>s are usually
  /// stored in a <see cref="ContactSet"/>. A <see cref="Contact"/> describes a single contact point
  /// (or closest-point pair), whereas a <see cref="ContactSet"/> contains all contacts between two 
  /// objects. (The involved objects are called "object A" and "object B".)
  /// </para>
  /// <para>
  /// A <see cref="Contact"/> includes 2 points: a point on object A (see 
  /// <see cref="PositionALocal"/> or <see cref="PositionAWorld"/>) and a point on object B (see 
  /// <see cref="PositionBLocal"/> or <see cref="PositionBWorld"/>). The property 
  /// <see cref="Position"/> is a point that lies halfway between those two points.
  /// </para>
  /// <para>
  /// There are 4 types of contacts:
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// <strong>Touching Contact:</strong> Object A and object B are touching at the surface. The 
  /// <see cref="PenetrationDepth"/> is 0. The points on object A and object B are identical. 
  /// <see cref="Position"/>, <see cref="PositionAWorld"/>, and <see cref="PositionBWorld"/> are 
  /// identical.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <strong>Penetrating Contact:</strong> Object A and object B are penetrating each other. The 
  /// <see cref="PenetrationDepth"/> is greater than 0. <see cref="PositionAWorld"/>, and 
  /// <see cref="PositionBWorld"/> are different and describe the points on object A and B that 
  /// have maximum penetration. <see cref="Position"/> lies halfway between these two points.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <para>
  /// <strong>Closest points (separated objects):</strong> Object A and object B are separated. They
  /// are not in contact. This can be the result of a closest-point query (see 
  /// <see cref="CollisionDetection.GetClosestPoints"/>). Normal contact queries such as 
  /// <see cref="CollisionDetection.GetContacts"/> or the contact queries performed inside a 
  /// <see cref="CollisionDomain"/> ignore separated objects!
  /// </para>
  /// <para>
  /// The <see cref="PenetrationDepth"/> is negative. The absolute value of 
  /// <see cref="PenetrationDepth"/> indicates the distance between the object A and object B. (The
  /// "penetration depth" is the inverse of "separation distance".) <see cref="PositionAWorld"/>
  /// and <see cref="PositionBWorld"/> are the closest points between the two objects. 
  /// <see cref="Position"/> lies halfway between the closest points.
  /// </para>
  /// <para>
  /// Closest-point query is a special type of collision query. At first this might look confusing:
  /// Why does a closest-point query return a <see cref="Contact"/>? The reason that contacts and
  /// closest points are represented by the same class is that the collision detection internally
  /// treats contacts and closest points the same way.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <para>
  /// <strong>Ray Hit:</strong> Either object A or object B is a ray that hits the other object. The
  /// <see cref="PenetrationDepth"/> stores the distance from the ray origin to the contact position
  /// on the second object. The <see cref="Normal"/> describes the surface normal at the contact
  /// position.
  /// </para>
  /// <para>
  /// (Ray hits can easily be found by checking whether the property <see cref="IsRayHit"/> is set
  /// in a <see cref="Contact"/>.)
  /// </para>
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  public sealed class Contact : IRecyclable
  {
    // Special handling for raycasts: For separated ray and other object, nothing changes. For a 
    // penetrating ray all contact position should be on the surface.

    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly ResourcePool<Contact> Pool =
      new ResourcePool<Contact>(
        () => new Contact(),
        null,
        null);
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------
        
    ///// <summary>
    ///// Gets or sets the application data.
    ///// </summary>
    ///// <value>The application data.</value>
    ///// <remarks>
    ///// <para>
    ///// This property can store data of the application that uses the collision detection library.
    ///// This property is not used by the collision detection library itself.
    ///// </para>
    ///// </remarks>
    //public object ApplicationData { get; set; }


    /// <summary>
    /// Gets or sets the index of the shape feature of object A that was hit.
    /// </summary>
    /// <value>
    /// The index of the feature of object A that was hit. The default value is -1.
    /// </value>
    /// <remarks>
    /// This property indicates which feature of the <see cref="Shape"/> of object A created this 
    /// contact. This value is an index that depends on the type of <see cref="Shape"/>. For most 
    /// shapes, this value is not used (in this cases it is -1). See the shape documentation of 
    /// individual shapes (for example, <see cref="CompositeShape"/> or 
    /// <see cref="TriangleMeshShape"/>) to find out how it is used.
    /// </remarks>
    public int FeatureA
    {
      get { return _featureA; }
      set { _featureA = value; }
    }
    private int _featureA = -1;


    /// <summary>
    /// Gets or sets the index of the shape feature of object B that was hit.
    /// </summary>
    /// <value>
    /// The index of the feature of object B that was hit. The default value is -1.
    /// </value>
    /// <remarks>
    /// This property indicates which feature of the <see cref="Shape"/> of object B created this 
    /// contact. This value is an index that depends on the type of <see cref="Shape"/>. For most 
    /// shapes, this value is not used (in this cases it is -1). See the shape documentation of 
    /// individual shapes (for example, <see cref="CompositeShape"/> or 
    /// <see cref="TriangleMeshShape"/>) to find out how it is used.
    /// </remarks>
    public int FeatureB
    {
      get { return _featureB; }
      set { _featureB = value; }
    }
    private int _featureB = -1;


    /// <summary>
    /// Gets or sets the contact position (in world space).
    /// </summary>
    /// <value>The contact position (in world space).</value>
    /// <remarks>
    /// <para>
    /// This position is halfway between <see cref="PositionAWorld"/> and 
    /// <see cref="PositionBWorld"/>.
    /// </para>
    /// <para>
    /// For a touching contact this point is the exact position where the two objects touch. For
    /// penetrating contacts this position is a midpoint halfway along the penetration depth. For
    /// separated contacts this position is halfway between the two objects. 
    /// </para>
    /// </remarks>
    public Vector3F Position { get; set; }


    /// <summary>
    /// Gets or sets the contact position on object A in the local space of object A.
    /// </summary>
    /// <value>The contact position on A in the local space of object A.</value>
    public Vector3F PositionALocal { get; set; }


    /// <summary>
    /// Gets or sets the contact position on object B in the local space of object B.
    /// </summary>
    /// <value>The contact position on B in the local space of object B.</value>
    public Vector3F PositionBLocal { get; set; }


    /// <summary>
    /// Gets the contact position on object A (in world space).
    /// </summary>
    /// <value>The contact position on A (in world space).</value>
    public Vector3F PositionAWorld
    {
      get
      {
        if (IsRayHit)
          return Position;

        return Position + Normal * (PenetrationDepth / 2);
      }
    }


    /// <summary>
    /// Gets the contact position on object B (in world space).
    /// </summary>
    /// <value>The contact position on B (in world space).</value>
    public Vector3F PositionBWorld
    {
      get
      {
        if (IsRayHit)
          return Position;

        return Position - Normal * (PenetrationDepth / 2);
      }
    }


    /// <summary>
    /// Gets or sets the normalized contact normal (pointing from object A to object B; in world space).
    /// </summary>
    /// <value>
    /// The normalized contact normal (pointing from object A to object B; in world space).
    /// This vector must be normalized (the length must be <c>1</c>).
    /// </value>
    /// <remarks>
    /// This vector shows the direction into which object B has to move to move away from object A.
    /// This value is stored as a normalized vector.
    /// </remarks>
    public Vector3F Normal
    {
      get { return _normal; }
      set
      {
        Debug.Assert(value.IsNumericallyNormalized, "Normal vector must be normalized. Length = " + value.Length);
        _normal = value;
      }
    }
    private Vector3F _normal = Vector3F.UnitY;


    /// <summary>
    /// Gets or sets the penetration depth.
    /// </summary>
    /// <value>The penetration depth.</value>
    /// <remarks>
    /// <para>
    /// This is the distance which the two object have to move along the contact normal (see 
    /// <see cref="Normal"/>) to be in a touching state (no penetration and no separation). For 
    /// penetrating contacts this value is positive. For separated objects this value is negative.
    /// (The "penetration depth" is the inverse of the "separation distance".)
    /// </para>
    /// <para>
    /// <strong>For ray-casting:</strong> If this value is positive, the ray hits the object and the 
    /// <see cref="PenetrationDepth"/> is the distance from the origin of the ray origin to the 
    /// contact point. If the value is negative the ray misses the object and the absolute value of 
    /// the <see cref="PenetrationDepth"/> indicates the separation distance between the ray and the 
    /// other object (closest points).
    /// </para>
    /// </remarks>
    public float PenetrationDepth { get; set; }


    /// <summary>
    /// Gets or sets the lifetime of this contact (in seconds).
    /// </summary>
    /// <value>The lifetime of this contact (in seconds).</value>
    /// <remarks>
    /// A touching or penetrating contact can exist for a longer time. This time span is 
    /// automatically increased for persistent contacts.
    /// </remarks>
    public double Lifetime { get; set; }


    /// <summary>
    /// Gets or sets the user data.
    /// </summary>
    /// <value>The user data.</value>
    /// <remarks>
    /// <para>
    /// This property can store end-user data. This property is not used by the collision detection 
    /// library itself.
    /// </para>
    /// </remarks>
    public object UserData { get; set; }


    /// <summary>
    /// Gets a value indicating whether this contact is a hit by a ray.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this contact is a hit by a ray; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Normally, the contact points can lie within an object for a penetrating contacts. But ray
    /// casts (ray vs. any other shape) will always create a contact point on the surface of the 
    /// shape which is hit. If <see cref="IsRayHit"/> is <see langword="true"/> then 
    /// <see cref="Position"/>, <see cref="PositionAWorld"/> and <see cref="PositionBWorld"/> are
    /// always identical. The <see cref="PenetrationDepth"/> is the distance from origin of the ray
    /// to the contact point.
    /// </para>
    /// </remarks>
    public bool IsRayHit { get; set; }


    /// <summary>
    /// Gets a copy of the contact where the collision objects are swapped.
    /// </summary>
    /// <remarks>
    /// The local contact position and the shape features on A and B are swapped; the normal vector
    /// is inverted.
    /// </remarks>
    public Contact Swapped
    {
      get
      {
        var contact = Create();
        contact.PositionALocal = PositionBLocal;
        contact.PositionBLocal = PositionALocal;
        contact.Normal = (-Normal);
        contact.Position = Position;
        contact.PenetrationDepth = PenetrationDepth;
        contact.Lifetime = Lifetime;
          //contact.ApplicationData = ApplicationData;
        contact.UserData = UserData;
        contact.FeatureA = FeatureB;
        contact.FeatureB = FeatureA;
        contact.IsRayHit = IsRayHit;
        return contact;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Prevents a default instance of the <see cref="Contact"/> class from being created.
    /// </summary>
    private Contact()
    {
    }


    /// <summary>
    /// Creates an instance of the <see cref="Contact"/> class. (This method 
    /// reuses a previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="Contact"/> class.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap. 
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle()"/> when the instance is no longer 
    /// needed.
    /// </para>
    /// </remarks>
    public static Contact Create()
    {
      return Pool.Obtain();
    }


    /// <summary>
    /// Recycles this instance of the <see cref="Contact"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    public void Recycle()
    {
      Reset();
      Pool.Recycle(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Resets this contact to default values. 
    /// </summary>
    internal void Reset()
    {
      FeatureA = -1;
      FeatureB = -1;
      Lifetime = 0;
      UserData = null;
      IsRayHit = false;
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(
        CultureInfo.InvariantCulture,
        "Contact {{ Position = {0}, Normal = {1}, PenetrationDepth = {2}, Lifetime = {3} }}",
        Position, 
        Normal, 
        PenetrationDepth, 
        Lifetime);
    }
    #endregion
  }
}
