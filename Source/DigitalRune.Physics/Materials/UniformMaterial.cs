// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Materials
{
  /// <summary>
  /// Defines a <see cref="IMaterial"/> with constant material properties for the whole rigid body.
  /// </summary>
  public class UniformMaterial : IMaterial, INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the static friction coefficient.
    /// </summary>
    /// <value>
    /// The static friction coefficient in the range [0, ∞[.
    /// The default value is 0.5.
    /// </value>
    /// <remarks>
    /// Dry friction resists relative lateral motion of two solid surfaces in contact. Dry 
    /// friction is subdivided into static friction between non-moving surfaces and dynamic 
    /// friction between moving surfaces.
    /// </remarks>
    /// <seealso cref="DynamicFriction"/>
    public float StaticFriction { get; set; }


    /// <summary>
    /// Gets or sets the dynamic friction (kinetic friction) coefficient.
    /// </summary>
    /// <value>
    /// The dynamic friction coefficient in the range [0, ∞[.
    /// The default value is 0.5.
    /// </value>
    /// <remarks>
    /// Dry friction resists relative lateral motion of two solid surfaces in contact. Dry 
    /// friction is subdivided into static friction between non-moving surfaces and dynamic 
    /// friction between moving surfaces.
    /// </remarks>
    /// <seealso cref="StaticFriction"/>
    public float DynamicFriction { get; set; }


    /// <summary>
    /// Gets or sets the coefficient of restitution (bounciness).
    /// </summary>
    /// <value>
    /// The coefficient of restitution in the range [0, 1]. The default value is 0.1.
    /// </value>
    /// <remarks>
    /// <para>
    /// The coefficient of restitution or bounciness of an object is a fractional value 
    /// representing the ratio of velocities after and before an impact. An object with a
    /// restitution of 1 collides elastically, while an object with a restitution less than 1
    /// collides inelastically. For a value of 0, the object effectively "stops" at the surface
    /// with which it collides - not bouncing at all.
    /// </para>
    /// <para>
    /// For a stable simulation it is recommended to use 0 or low values when possible.
    /// </para>
    /// </remarks>
    public float Restitution { get; set; }


    /// <summary>
    /// Gets a value indicating whether this material supports surface motion.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this material supports surface motion; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This flag indicates whether the material supports a surface motion - it does not indicate if
    /// the current <see cref="SurfaceMotion"/> is non-zero. The simulation has optimizations for
    /// contacts where materials will never have a surface motion. If a material will at any time 
    /// use a non-zero surface motion this flag must be set to <see langword="true"/>, even if the 
    /// current surface motion velocity is zero.
    /// </para>
    /// <para>
    /// This flag is read-only and can only be set in the constructor of this class.
    /// </para>
    /// </remarks>
    public bool SupportsSurfaceMotion { get; private set; }


    /// <summary>
    /// Gets or sets the velocity of the rigid body surface (in local space of the body).
    /// </summary>
    /// <value>
    /// A velocity that describes the speed and the direction in which the surface is moving 
    /// relative to the rigid body. The default value is a (0, 0, 0).
    /// </value>
    /// <remarks>
    /// This property can be used to simulate conveyor belts or similar objects.
    /// </remarks>
    /// <exception cref="PhysicsException">
    /// This material does not support surface motion. (<see cref="SupportsSurfaceMotion"/> is 
    /// <see langword="false"/>.)
    /// </exception>
    public Vector3F SurfaceMotion
    {
      get { return _surfaceMotion; }
      set
      {
        if (!SupportsSurfaceMotion)
          throw new PhysicsException("This material does not support surface motion.");

        _surfaceMotion = value;
      }
    }
    private Vector3F _surfaceMotion;


    /// <summary>
    /// Gets or sets the name of the material.
    /// </summary>
    /// <value>
    /// The name of the material, for example "Wood", "Ice". The default value is
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property is not used by the simulation. It can be used to manage different materials
    /// and for debugging.
    /// </remarks>
    public string Name { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="UniformMaterial"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="UniformMaterial"/> class.
    /// </summary>
    /// <remarks>
    /// The created material does not support <see cref="SurfaceMotion"/>.
    /// </remarks>
    public UniformMaterial()
      : this(null, false)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="UniformMaterial"/> class.
    /// </summary>
    /// <param name="name">The name of the material. Can be <see langword="null"/>.</param>
    public UniformMaterial(string name)
      : this(name, false)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="UniformMaterial"/> class.
    /// </summary>
    /// <param name="name">The name of the material. Can be <see langword="null"/>.</param>
    /// <param name="supportsSurfaceMotion">
    /// If set to <see langword="true"/> the material supports surface motion. See also
    /// <see cref="SupportsSurfaceMotion"/> and <see cref="SurfaceMotion"/>.
    /// </param>
    public UniformMaterial(string name, bool supportsSurfaceMotion)
    {
      Name = name;
      DynamicFriction = 0.5f;
      StaticFriction = 0.5f;
      Restitution = 0.1f;
      SupportsSurfaceMotion = supportsSurfaceMotion;
      _surfaceMotion = Vector3F.Zero;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="UniformMaterial"/> class from a given material.
    /// </summary>
    /// <param name="material">The material from which the properties are copied.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="material"/> is <see langword="null"/>.
    /// </exception>
    public UniformMaterial(UniformMaterial material)
    {
      if (material == null)
        throw new ArgumentNullException("material");

      Name = material.Name;
      DynamicFriction = material.DynamicFriction;
      StaticFriction = material.StaticFriction;
      Restitution = material.Restitution;
      SupportsSurfaceMotion = material.SupportsSurfaceMotion;
      _surfaceMotion = material.SurfaceMotion;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="UniformMaterial"/> class from a given material.
    /// </summary>
    /// <param name="name">The name of the material. Can be <see langword="null"/>.</param>
    /// <param name="material">The material from which the properties are copied.</param>
    public UniformMaterial(string name, MaterialProperties material)
    {
      Name = name;
      DynamicFriction = material.DynamicFriction;
      StaticFriction = material.StaticFriction;
      Restitution = material.Restitution;
      SupportsSurfaceMotion = material.SupportsSurfaceMotion;
      _surfaceMotion = material.SurfaceMotion;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="MaterialProperties"/> for the given rigid body, position and shape
    /// feature.
    /// </summary>
    /// <param name="body">The rigid body.</param>
    /// <param name="positionLocal">
    /// The local position on the rigid body for which the material properties should be returned.
    /// </param>
    /// <param name="featureIndex">
    /// The index of the shape feature from which the material properties are needed. For a
    /// <see cref="CompositeShape"/> the feature index is the index of the child of the composite
    /// shape. For a <see cref="TriangleMeshShape"/> the feature index is the index of a triangle.
    /// </param>
    /// <returns>
    /// The <see cref="MaterialProperties"/> of the given rigid body at the given position and
    /// child feature.
    /// </returns>
    public MaterialProperties GetProperties(RigidBody body, Vector3F positionLocal, int featureIndex)
    {
      MaterialProperties parameters = new MaterialProperties(
        StaticFriction,
        DynamicFriction,
        Restitution,
        SupportsSurfaceMotion,
        SurfaceMotion);

      return parameters;
    }
    #endregion
  }
}
