// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Xml.Serialization;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{  
  /// <summary>
  /// Defines the volume of an <see cref="IGeometricObject"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="Shape"/> defines the space that is occupied by an object. <see cref="Shape"/> is
  /// the common base class of all types of shapes in DigitalRune Geometry.
  /// </para>
  /// <para>
  /// Shapes are defined in the local coordinate system of the owning object (also known as local 
  /// space, object space or body space). Shapes are abstracted from other properties such as color,
  /// material, or even position and orientation. Most shapes contain only properties that define 
  /// their dimensions ("width", "height", "radius", etc.). For example, a <see cref="SphereShape"/> 
  /// consists only of a radius that defines the size of the sphere. The sphere is centered in the 
  /// local coordinate system.
  /// </para>
  /// <para>
  /// However, some shapes like the <see cref="PointShape"/>, the <see cref="TriangleShape"/> or the
  /// <see cref="RayShape"/>, are defined using position and orientation vectors ("Vertex0", 
  /// "Origin", "Direction", etc.). Thus, they can be also positioned in local coordinate space. 
  /// </para>
  /// <para>
  /// An <see cref="IGeometricObject"/> is used to position an object in the world coordinate space.
  /// An <see cref="IGeometricObject"/> consists of a <see cref="IGeometricObject.Shape"/>, a
  /// <see cref="IGeometricObject.Scale"/> and a <see cref="IGeometricObject.Pose"/> (= position and
  /// orientation).
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> Shapes are cloneable. The method <see cref="Clone"/> creates a deep 
  /// copy of the shape - except when documented otherwise (see description of derived classes).
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  [XmlInclude(typeof(BoxShape))]
  [XmlInclude(typeof(CapsuleShape))]
  [XmlInclude(typeof(CircleShape))]
  //[XmlInclude(typeof(CompositeShape))]
  [XmlInclude(typeof(ConeShape))]
  //[XmlInclude(typeof(ConvexHullOfPoints))]
  //[XmlInclude(typeof(ConvexHullOfShapes))]
  [XmlInclude(typeof(ConvexPolyhedron))]
  [XmlInclude(typeof(CylinderShape))]
  [XmlInclude(typeof(EmptyShape))]
  [XmlInclude(typeof(InfiniteShape))]
  //[XmlInclude(typeof(HeightField))]
  [XmlInclude(typeof(LineShape))]
  [XmlInclude(typeof(LineSegmentShape))]
  //[XmlInclude(typeof(MinkowskiDifferenceShape))]
  //[XmlInclude(typeof(MinkowskiSumShape))]
  [XmlInclude(typeof(OrthographicViewVolume))]
  [XmlInclude(typeof(PerspectiveViewVolume))]
  [XmlInclude(typeof(PlaneShape))]
  [XmlInclude(typeof(PointShape))]
  [XmlInclude(typeof(RayShape))]
  [XmlInclude(typeof(RectangleShape))]
  [XmlInclude(typeof(SphereShape))]
  //[XmlInclude(typeof(TransformedShape))]
  [XmlInclude(typeof(ScaledConvexShape))]
  [XmlInclude(typeof(TriangleShape))]
  //[XmlInclude(typeof(TriangleMeshShape))]
  // TODO: Extend list for new shapes.
  public abstract class Shape 
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// An immutable shape that is infinitely small and does not collide with other shapes.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly EmptyShape Empty = new EmptyShape(0);


    /// <summary>
    /// An immutable shape that is infinitely large and collides with every other shape
    /// (except <see cref="EmptyShape"/>s).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly InfiniteShape Infinite = new InfiniteShape(0);
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>An inner point.</value>
    /// <remarks>
    /// This property returns a random point of the shape. If possible, a point in the center of the 
    /// shape is returned. If not possible, a surface point is returned.
    /// </remarks>
    public abstract Vector3F InnerPoint { get; }


    /// <summary>
    /// Occurs when the shape was changed.
    /// </summary>
    public virtual event EventHandler<ShapeChangedEventArgs> Changed
    {
      add { _changed += value; } 
      remove { _changed -= value; }
    }
    private event EventHandler<ShapeChangedEventArgs> _changed;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="Shape"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="Shape"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="Shape"/> derived class and <see cref="CloneCore"/> to create a copy of the 
    /// current instance. Classes that derive from <see cref="Shape"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </remarks>
    public Shape Clone()
    {
      if (this == Empty || this == Infinite)
      {
        // Empty and Infinite are immutable default shapes that should not need 
        // be duplicated (to avoid unnecessary memory allocations).
        return this;
      }

      Shape clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Shape"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a protected method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method, 
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone shape. A derived class does not implement <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private Shape CreateInstance()
    {
      Shape newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone shape. The derived class {0} does not implement CreateInstanceCore().",
          GetType());

        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the <see cref="Shape"/> 
    /// derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="Shape"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Shape"/> derived class must implement 
    /// this method. A typical implementation is to simply call the default constructor and return 
    /// the result. 
    /// </para>
    /// </remarks>
    protected abstract Shape CreateInstanceCore();


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="Shape"/>.
    /// </summary>
    /// <param name="sourceShape">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Shape"/> derived class must implement 
    /// this method. A typical implementation is to call <c>base.CloneCore(this)</c> to copy all 
    /// properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    protected abstract void CloneCore(Shape sourceShape);
    #endregion


    /// <overloads>
    /// <summary>
    /// Computes the axis-aligned bounding box (AABB) for this shape.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Computes the axis-aligned bounding box (AABB) for this shape in local space.
    /// </summary>
    /// <returns>The AABB of the shape positioned in local space.</returns>
    /// <remarks>
    /// <para>
    /// The AABB is axis-aligned to the axes of the local space.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Aabb GetAabb()
    {
      return GetAabb(Vector3F.One, Pose.Identity);
    }


    /// <summary>
    /// Computes the axis-aligned bounding box (AABB) for this shape positioned in world space using
    /// the given <see cref="Pose"/>.
    /// </summary>
    /// <param name="pose">
    /// The <see cref="Pose"/> of the shape. This pose defines how the shape should be positioned
    /// in world space.
    /// </param>
    /// <returns>The AABB of the shape positioned in world space.</returns>
    /// <remarks>
    /// <para>
    /// The AABB is axis-aligned to the axes of the world space (or the parent coordinate space).
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Aabb GetAabb(Pose pose)
    {
      return GetAabb(Vector3F.One, pose);
    }


    /// <summary>
    /// Computes the axis-aligned bounding box (AABB) for this shape positioned in world space using
    /// the given scale and <see cref="Pose"/>.
    /// </summary>
    /// <param name="scale">
    /// The scale factor by which the shape should be scaled. The scaling is applied in the shape's
    /// local space before the pose is applied.
    /// </param>
    /// <param name="pose">
    /// The <see cref="Pose"/> of the shape. This pose defines how the shape should be positioned in
    /// world space.
    /// </param>
    /// <returns>The AABB of the shape positioned in world space.</returns>
    /// <remarks>
    /// The AABB is axis-aligned to the axes of the world space (or the parent coordinate space).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public abstract Aabb GetAabb(Vector3F scale, Pose pose);


    /// <summary>
    /// Gets a mesh that represents this shape.
    /// </summary>
    /// <param name="relativeDistanceThreshold">The relative distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    /// <remarks>
    /// <para>
    /// If a mesh can exactly represent the shape, an exact mesh is returned (for example
    /// for a <see cref="BoxShape"/>). Otherwise a mesh with a relative error will be returned
    /// (for example for a <see cref="SphereShape"/>). The relative error is less than
    /// <paramref name="relativeDistanceThreshold"/> % of the largest AABB extent. If the
    /// mesh is generated by an iterative algorithm, no more than <paramref name="iterationLimit"/>
    /// iterations are performed. If the <paramref name="iterationLimit"/> is reached first, the
    /// returned mesh will have a higher relative error.
    /// </para>
    /// <para>
    /// This method calls <see cref="OnGetMesh"/> which must be implemented in derived classes.
    /// See <see cref="OnGetMesh"/> for more information about the generated mesh.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="relativeDistanceThreshold"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="iterationLimit"/> is negative or 0.
    /// </exception>
    public TriangleMesh GetMesh(float relativeDistanceThreshold, int iterationLimit)
    {
      if (Numeric.IsLess(relativeDistanceThreshold, 0))
        throw new ArgumentOutOfRangeException("relativeDistanceThreshold", "The relative distance threshold must not be negative.");
      if (iterationLimit <= 0)
        throw new ArgumentOutOfRangeException("iterationLimit", "The iteration limit must be greater than 0.");

      // Compute absolute distance threshold.
      float maxExtent = GetAabb().Extent.LargestComponent;
      float absoluteThreshold = maxExtent * relativeDistanceThreshold;

      return OnGetMesh(absoluteThreshold, iterationLimit);
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The parameters are guaranteed to be in a valid range -
    /// no parameter validation necessary.
    /// </para>
    /// <para>
    /// If an exact mesh can be returned, this mesh should be generated. If the shape can only be
    /// approximated, the absolute distance error should be less than 
    /// <paramref name="absoluteDistanceThreshold"/> or at max <paramref name="iterationLimit"/> 
    /// iterations should be performed for iterative mesh generation algorithms.
    /// </para>
    /// </remarks>
    protected abstract TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit);


    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// <param name="relativeError">
    /// The desired relative error for approximations (in the range [0, 1]). For example, use the 
    /// value 0.01 to get a maximal error of about 1%.
    /// </param>
    /// <param name="iterationLimit">
    /// The iteration limit. Must be greater than or equal to 0. For most cases a small value like 4 
    /// is appropriate. 
    /// </param>
    /// <returns>The volume of this shape.</returns>
    /// <remarks>
    /// <para>
    /// If the volume can be computed with an exact formula, then the exact volume is returned. But 
    /// for some shapes an approximate volume is computed. For approximated volumes 
    /// <paramref name="relativeError"/> defines the desired relative error and if the volume is 
    /// computed by an iterative algorithm, no more than <paramref name="iterationLimit"/>
    /// iterations are performed. If the <paramref name="iterationLimit"/> is reached first, the
    /// returned mesh will have a higher relative error. 
    /// </para>
    /// <para>
    /// Currently, <paramref name="relativeError"/> is proportional to the error of the approximated
    /// volume. But it is not guaranteed that the relative error between the approximated volume
    /// and the exact volume is less than <paramref name="relativeError"/>. It is only guaranteed 
    /// that a smaller <paramref name="relativeError"/> value leads to a more accurate 
    /// approximation.
    /// </para>
    /// <para>
    /// Remember: To compute the volume of a scaled shape, you can compute the volume of the 
    /// unscaled shape and multiply the result with the scaling factors: 
    /// </para>
    /// <para>
    /// <i>volume<sub>scaled</sub> = volume<sub>unscaled</sub> * scale<sub>X</sub> * scale<sub>Y</sub> * scale<sub>Z</sub></i>
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The base implementation of this method computes the
    /// volume from the mesh of the shape (see <see cref="Shape.GetMesh"/>). And if 
    /// <paramref name="iterationLimit"/> is 0, the volume of the axis-aligned bounding box (see 
    /// <see cref="Shape.GetAabb(Pose)"/>) is used. Derived classes should override this method to 
    /// compute a more accurate volume or to provide a faster implementation.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="relativeError"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="iterationLimit"/> is negative or 0.
    /// </exception>
    public virtual float GetVolume(float relativeError, int iterationLimit)
    {
      if (Numeric.IsLess(relativeError, 0))
        throw new ArgumentOutOfRangeException("relativeError", "The relative error must not be negative.");
      if (iterationLimit < 0)
        throw new ArgumentOutOfRangeException("iterationLimit", "The iteration limit must be greater than 0.");

      // Use AABB volume if iterationLimit is 0.
      if (iterationLimit == 0)
        return GetAabb().Volume;

      var mesh = GetMesh(relativeError, iterationLimit);
      float volume = mesh.GetVolume();
      return volume;
    }


    /// <summary>
    /// Raises the <see cref="Shape.Changed"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="ShapeChangedEventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnChanged"/> in a derived
    /// class, be sure to call the base class's <see cref="OnChanged"/> method so that registered
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnChanged(ShapeChangedEventArgs eventArgs)
    {
      var handler = _changed;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
