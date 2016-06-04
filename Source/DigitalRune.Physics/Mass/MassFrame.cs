// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics
{
  /// <summary>
  /// Defines the mass properties of a rigid body.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>A simplified explanation of mass: </strong>
  /// <see cref="Mass"/> defines how difficult it is to change the linear velocity of a body. For
  /// example, if mass is infinite it is impossible to move a resting body or to stop a body that is
  /// already in motion. The <see cref="Inertia"/> matrix is the rotational equivalent of 
  /// <see cref="Mass"/>. It defines how difficult it is to change the angular velocity of a body.
  /// </para>
  /// <para>
  /// <strong>Center of Mass:</strong> For the simulation the center of mass is the center of a 
  /// rigid body. The center of mass has a special importance because if forces act on an 
  /// unconstrained rigid body (e.g. a body floating in space) any rotations will be around the 
  /// center of mass. In contrast, the local space origin of a rigid body can be anywhere where the
  /// user wants it to be. For example, the origin of rigid body with a <see cref="ConeShape"/> is
  /// at the base of the cone whereas the center of mass of a cone is inside the cone. Or the origin
  /// of a human can be at the feet and the center of mass is above the pelvis area. This allows
  /// easy placement of the rigid body in a game editor. <see cref="Pose"/>. 
  /// <see cref="Geometry.Pose.Position"/> defines the position of the center of mass relative to
  /// the local space of the rigid body.
  /// </para>
  /// <para>
  /// <strong>Inertia Tensor:</strong> The inertia tensor describes the "rotational mass" of a 
  /// rigid body. In general, the inertia tensor is 3 x 3 matrix. But for all rigid bodies a rotated
  /// coordinate space can be found where all off-diagonal elements of the inertia matrix are 0. The
  /// axes of this rotated coordinate space are called the "principal axes". <see cref="Inertia"/>
  /// stores the diagonal elements of the diagonalized inertia matrix. And 
  /// <see cref="Pose"/>.<see cref="Geometry.Pose.Orientation"/> stores the orientation of the 
  /// principal axis space relative to the local space of the rigid body.
  /// </para>
  /// <para>
  /// <strong>Pose:</strong> As described above, <see cref="Pose"/> stores the position of the
  /// center of mass and the orientation of the coordinate space where the inertia tensor is a
  /// diagonal matrix. In other words, the pose describe the transformation from a space, where the
  /// center of mass is at the origin and the inertia matrix is a diagonal matrix, to the local
  /// space of the body. In other words, the pose position is equal to the center of mass and the
  /// columns of the pose orientation are the principal axes.
  /// </para>
  /// <para>
  /// <strong>Creating new <see cref="MassFrame"/> instances:</strong> You can define 
  /// <see cref="Mass"/>, <see cref="Inertia"/> and <see cref="Pose"/> manually but this is 
  /// non-trivial for complex shapes. Therefore, is much simpler to use 
  /// <see cref="FromShapeAndDensity"/> or <see cref="FromShapeAndMass"/> to create a 
  /// <see cref="MassFrame"/> instance. The first method takes a shape and a density and computes 
  /// the mass properties. The second method takes a shape and a target mass value and computes mass
  /// frame properties so that the mass is equal to the target mass.
  /// </para>
  /// <para>
  /// <strong>Composite Objects:</strong> In some cases a rigid body consists of parts with
  /// different densities, for example: A hammer has a metal head and a wooden shaft. To model these
  /// kind of objects you can create a rigid body with a <see cref="CompositeShape"/>. Normally, all
  /// child object in the <see cref="CompositeShape"/> have the same density - but: If the child of
  /// a <see cref="CompositeShape"/> is a <see cref="RigidBody"/>, the mass properties of this
  /// child rigid body are used. Remember: The children of a <see cref="CompositeShape"/> are of the
  /// type <see cref="IGeometricObject"/> and a <see cref="RigidBody"/> implements
  /// <see cref="IGeometricObject"/>. Therefore, a <see cref="RigidBody"/> can be the child of a
  /// <see cref="CompositeShape"/>. 
  /// </para>
  /// <para>
  /// Now, to model a hammer: Create a rigid body for the metal head. Create a rigid body for the
  /// wooden shaft. Add both rigid bodies to a <see cref="CompositeShape"/>. Call 
  /// <see cref="FromShapeAndDensity"/> to compute a <see cref="MassFrame"/> instance. Then you can
  /// create the rigid body "hammer" where the shape is the <see cref="CompositeShape"/> and the 
  /// <see cref="MassFrame"/> is the computed mass frame instance. The first two bodies are only
  /// used to define the composite shape - they are never added to the simulation directly.
  /// </para>
  /// <para>
  /// Whenever <see cref="FromShapeAndDensity"/> or <see cref="FromShapeAndMass"/> are called for
  /// composite shapes, they check if a child shape is a rigid body. And if a child is a rigid body
  /// the <see cref="MassFrame"/> of this child rigid body is used directly for this child.
  /// </para>
  /// </remarks>
  public sealed class MassFrame
  {
    // Note:
    // The name "mass frame" is also used in COLLADA.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the mass limit. Mass values above this value are treated as infinite.
    /// </summary>
    /// <value>The mass limit in the range [0, ∞[. The default is 1e10.</value>
    /// <remarks>
    /// <para>
    /// If a mass value (<see cref="Mass"/> or an element of <see cref="Inertia"/>) is above this
    /// limit, the simulation treats this value as infinite which allows certain optimizations. 
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public static float MassLimit
    {
      get { return _massLimit; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "MassLimit must be 0 or positive.");

        _massLimit = value;
      }
    }
    private static float _massLimit = 1e10f;


    /// <summary>
    /// Gets or sets the density.
    /// </summary>
    /// <value>The density. The default value is 1000.</value>
    /// <remarks>
    /// If this <see cref="MassFrame"/> instance was created with <see cref="FromShapeAndDensity"/>
    /// the density is the density that was given in the <see cref="FromShapeAndDensity"/> call.
    /// If this <see cref="MassFrame"/> instance was created with <see cref="FromShapeAndMass"/>
    /// the density is set to 0 to indicate that the <see cref="MassFrame"/> was computed for a
    /// given target mass.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float Density
    {
      get { return _density; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "Density must be greater than or equal to 0.");

        _density = value;
      }
    }
    private float _density = 1000;


    /// <summary>
    /// Gets or sets the mass.
    /// </summary>
    /// <value>The mass. The default is 1000.</value>
    /// <remarks>
    /// If the mass is 0 or above the <see cref="MassLimit"/>, the simulation will treat the rigid
    /// body as a kinematic body (the body will not be moved by simulation forces).
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float Mass
    {
      get { return _mass; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "Mass must be greater than or equal to 0.");

        _mass = (Numeric.IsNaN(value)) ? float.PositiveInfinity : value;

        // Very small and very large masses are treated as kinematic (inverse mass is 0) 
        // because both make no sense for the simulation.
        MassInverse = (Numeric.IsZero(_mass) || _mass >= MassLimit) ? 0 : 1 / _mass;
      }
    }
    private float _mass = 1000.0f;


    /// <summary>
    /// Gets or sets the inverse of the mass (1 / mass).
    /// </summary>
    /// <value>The inverse of the mass (1 / mass).</value>
    /// <remarks>
    /// Extreme values (0 mass or very large mass) are clamped to 0.
    /// </remarks>
    internal float MassInverse
    {
      get { return _massInverse; }
      private set { _massInverse = value; }
    }
    private float _massInverse = 1.0f / 1000.0f;


    /// <summary>
    /// Gets or sets the inertia.
    /// </summary>
    /// <value>The inertia.</value>
    /// <remarks>
    /// This vector contains the diagonal elements of the diagonalized inertia matrix.
    /// </remarks>
    public Vector3F Inertia
    {
      get { return _inertia; }
      set
      {
        _inertia = (value.IsNaN) ? new Vector3F(float.PositiveInfinity) : value;
        UpdateInertiaInverse();
      }
    }
    private Vector3F _inertia = new Vector3F(166.67f);  // ~ 1 m box inertia for 1000 kg.


    // The inverse of 0 or large value is clamped to 0.
    /// <summary>
    /// Gets or sets the inverse of the inertia.
    /// </summary>
    /// <value>The inverse of the inertia.</value>
    /// <remarks>
    /// Extreme values (0 mass or very large mass) are clamped to 0.
    /// </remarks>
    internal Vector3F InertiaInverse
    {
      get { return _inertiaInverse; }
      private set { _inertiaInverse = value; }
    }
    private Vector3F _inertiaInverse = new Vector3F(1.0f / 166.67f);


    /// <summary>
    /// Gets or sets the pose that defines the center of mass and the principal axes.
    /// </summary>
    /// <value>
    /// The pose of the space where the center of mass is the origin and the inertia matrix is a 
    /// diagonal matrix.
    /// </value>
    public Pose Pose
    {
      get { return _pose; }
      set
      {
        Debug.Assert(!value.Position.IsNaN && !value.Orientation.IsNaN, "MassFrame.Pose is set to value containing NaN");
        _pose = value;
      }
    }
    private Pose _pose = Pose.Identity;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="MassFrame"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="MassFrame"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    public MassFrame Clone()
    {
      MassFrame clone = new MassFrame
      {
        _density = _density,
        _inertia = _inertia,
        _inertiaInverse = _inertiaInverse,
        _mass = _mass,
        _massInverse = _massInverse,
        _pose = _pose
      };
      return clone;
    }


    /// <summary>
    /// Changes the mass so that it is equal to the given target mass and the related properties
    /// (inertia) are scaled accordingly.
    /// </summary>
    /// <param name="targetMass">The target mass.</param>
    /// <remarks>
    /// Scaling mass frames is simple: Just change the mass, and scale the inertia by the factor
    /// <i>m<sub>new</sub> / m<sub>old</sub></i>. This is exactly what this method does.
    /// </remarks>
    public void Adjust(float targetMass)
    {
      float scale = targetMass / Mass;
      Mass = targetMass;
      Inertia = Inertia * scale;
    }


    /// <summary>
    /// Computes a mass frame for the given shape and density.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <param name="scale">The scale of the shape.</param>
    /// <param name="density">The density.</param>
    /// <param name="relativeDistanceThreshold">
    /// The relative distance threshold. If no mass or inertia formula for the given shape are known
    /// the shape is approximated with a triangle mesh and the mass frame of this mesh is returned.
    /// The relative distance threshold controls the accuracy of the approximated mesh. Good default
    /// values are 0.05 to get an approximation with an error of about 5%.
    /// </param>
    /// <param name="iterationLimit">
    /// The iteration limit. For some shapes the mass properties are computed with an iterative
    /// algorithm. No more than <paramref name="iterationLimit"/> iterations will be performed.
    /// A value of 3 gives good results in most cases. Use a value of -1 to get only a coarse
    /// approximation.
    /// </param>
    /// <returns>
    /// A new <see cref="MassFrame"/> for the given parameters is returned.
    /// </returns>
    /// <remarks>
    /// <strong>Composite shapes:</strong> If the given <paramref name="shape"/> is a 
    /// <see cref="CompositeShape"/>, the computed mass properties are only correct if the children
    /// of the composite shape do not overlap. Overlapping parts are counted twice and so the result
    /// will not be correct. If a child of a composite shape is a <see cref="RigidBody"/> the 
    /// <see cref="MassFrame"/> of the rigid body will be used for this child.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="density"/> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="relativeDistanceThreshold"/> is negative.
    /// </exception>
    public static MassFrame FromShapeAndDensity(Shape shape, Vector3F scale, float density, float relativeDistanceThreshold, int iterationLimit)
    {
      return ComputeMassProperties(shape, scale, density, true, relativeDistanceThreshold, iterationLimit);
    }


    /// <summary>
    /// Computes a mass frame for the given shape and target mass.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <param name="scale">The scale of the shape.</param>
    /// <param name="mass">
    /// The target mass. The mass of the computed <see cref="MassFrame"/> will be equal to this 
    /// value. Other mass properties are adjusted to match the target mass.
    /// </param>
    /// <param name="relativeDistanceThreshold">
    /// The relative distance threshold. If no mass or inertia formula for the given shape are known
    /// the shape is approximated with a triangle mesh and the mass frame of this mesh is returned.
    /// The relative distance threshold controls the accuracy of the approximated mesh. Good default
    /// values are 0.05 to get an approximation with an error of about 5%.
    /// </param>
    /// <param name="iterationLimit">
    /// The iteration limit. For some shapes the mass properties are computed with an iterative
    /// algorithm. No more than <paramref name="iterationLimit"/> iterations will be performed.
    /// A value of 3 gives good results in most cases. Use a value of -1 to get only a coarse
    /// approximation.
    /// </param>
    /// <returns>
    /// A new <see cref="MassFrame"/> for the given parameters is returned.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Composite shapes:</strong> If the given <paramref name="shape"/> is a 
    /// <see cref="CompositeShape"/>, the computed mass properties are only correct if the children
    /// of the composite shape do not overlap. Overlapping parts are counted twice and so the result
    /// will not be correct. If a child of a composite shape is a <see cref="RigidBody"/> the 
    /// <see cref="MassFrame"/> of the rigid body will be used for this child.
    /// </para>
    /// <para>
    /// <strong>Density:</strong> Since this method does not use a density value, the 
    /// <see cref="Density"/> property of the <see cref="MassFrame"/> is set to 0.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="mass"/> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="relativeDistanceThreshold"/> is negative.
    /// </exception>
    public static MassFrame FromShapeAndMass(Shape shape, Vector3F scale, float mass, float relativeDistanceThreshold, int iterationLimit)
    {
      return ComputeMassProperties(shape, scale, mass, false, relativeDistanceThreshold, iterationLimit);
    }


    /// <summary>
    /// Computes the mass properties for the given shape and parameters.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="densityOrMass">The density or target mass value.</param>
    /// <param name="isDensity">
    /// If set to <see langword="true"/> <paramref name="densityOrMass"/> is interpreted as density;
    /// otherwise, <paramref name="densityOrMass"/> is interpreted as the desired target mass.
    /// </param>
    /// <param name="relativeDistanceThreshold">The relative distance threshold.</param>
    /// <param name="iterationLimit">
    /// The iteration limit. Can be -1 or 0 to use an approximation.
    /// </param>
    /// <returns>
    /// A new <see cref="MassFrame"/> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="densityOrMass"/> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="relativeDistanceThreshold"/> is negative.
    /// </exception>
    private static MassFrame ComputeMassProperties(Shape shape, Vector3F scale, float densityOrMass, bool isDensity, float relativeDistanceThreshold, int iterationLimit)
    {
      if (shape == null)
        throw new ArgumentNullException("shape");
      if (densityOrMass <= 0)
        throw new ArgumentOutOfRangeException("densityOrMass", "Density and mass must be greater than 0.");
      if (relativeDistanceThreshold < 0)
        throw new ArgumentOutOfRangeException("relativeDistanceThreshold", "Relative distance threshold must not be negative.");

      // Call MassHelper to compute mass, COM and inertia matrix (not diagonalized).
      float mass;
      Vector3F centerOfMass;
      Matrix33F inertia;
      MassHelper.GetMass(shape, scale, densityOrMass, isDensity, relativeDistanceThreshold, iterationLimit,
                         out mass, out centerOfMass, out inertia);

      // If anything is NaN, we use default values for a static/kinematic body.
      if (Numeric.IsNaN(mass) || centerOfMass.IsNaN || inertia.IsNaN)
        return new MassFrame { Mass = 0, Inertia = Vector3F.Zero };

      if (!Numeric.IsZero(inertia.M01) || !Numeric.IsZero(inertia.M02) || !Numeric.IsZero(inertia.M12))
      {
        // Inertia off-diagonal elements are not 0.
        // --> Have to make inertia a diagonal matrix.
        Vector3F inertiaDiagonal;
        Matrix33F rotation;
        MassHelper.DiagonalizeInertia(inertia, out inertiaDiagonal, out rotation);

        MassFrame massFrame = new MassFrame
        {
          Mass = mass,
          Inertia = inertiaDiagonal,
          Pose = new Pose(centerOfMass, rotation),
          Density = isDensity ? densityOrMass : 0
        };
        return massFrame;
      }
      else
      {
        // Inertia is already a diagonal matrix.
        MassFrame massFrame = new MassFrame
        {
          Mass = mass,
          Inertia = new Vector3F(inertia.M00, inertia.M11, inertia.M22),
          Pose = new Pose(centerOfMass),
          Density = isDensity ? densityOrMass : 0
        };
        return massFrame;
      }
    }


    private void UpdateInertiaInverse()
    {
      // If an inertia element is 0, then the object is a line or a point and we can ignore
      // rotation around this axis. => Use inverse inertia = 0.
      // If an inertia element is very large, then treat this object as kinematic
      // regarding this axis. => Use inverse inertia = 0.
      // We use a very small epsilon because small objects have tiny inertia values.
      InertiaInverse = new Vector3F(
        (Numeric.IsZero(_inertia.X, 1e-10f) || _inertia.X >= MassLimit) ? 0 : 1 / _inertia.X,
        (Numeric.IsZero(_inertia.Y, 1e-10f) || _inertia.Y >= MassLimit) ? 0 : 1 / _inertia.Y,
        (Numeric.IsZero(_inertia.Z, 1e-10f) || _inertia.Z >= MassLimit) ? 0 : 1 / _inertia.Z);
    }
    #endregion
  }
}
