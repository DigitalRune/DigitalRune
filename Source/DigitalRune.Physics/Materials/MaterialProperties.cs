// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Materials
{
  /// <summary>
  /// Defines material properties of a rigid body.
  /// </summary>
  public struct MaterialProperties : IEquatable<MaterialProperties>
  {
    // TODO: SpinFriction, RollFriction, Anisotropic friction and friction directions, CFM

    /// <summary>
    /// Gets or sets the static friction coefficient.
    /// </summary>
    /// <value>
    /// The static friction coefficient in the range [0, ∞[.
    /// </value>
    /// <remarks>
    /// Dry friction resists relative lateral motion of two solid surfaces in contact. Dry 
    /// friction is subdivided into static friction between non-moving surfaces and dynamic 
    /// friction between moving surfaces.
    /// </remarks>
    /// <seealso cref="DynamicFriction"/>
    public float StaticFriction
    {
      get { return _staticFriction; }
      set { _staticFriction = value; }
    }
    private float _staticFriction;


    /// <summary>
    /// Gets or sets the dynamic friction (kinetic friction) coefficient.
    /// </summary>
    /// <value>
    /// The dynamic friction coefficient in the range [0, ∞[.
    /// </value>
    /// <remarks>
    /// Dry friction resists relative lateral motion of two solid surfaces in contact. Dry 
    /// friction is subdivided into static friction between non-moving surfaces and dynamic 
    /// friction between moving surfaces.
    /// </remarks>
    /// <seealso cref="StaticFriction"/>
    public float DynamicFriction
    {
      get { return _dynamicFriction; }
      set { _dynamicFriction = value; }
    }
    private float _dynamicFriction;


    /// <summary>
    /// Gets or sets the coefficient of restitution (bounciness).
    /// </summary>
    /// <value>The coefficient of restitution in the range [0, 1].</value>
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
    public float Restitution
    {
      get { return _restitution; }
      set { _restitution = value; }
    }
    private float _restitution;


    /// <summary>
    /// Gets or sets a value indicating whether this material supports surface motion.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this material supports surface motion; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This flag indicates whether the material supports a surface motion - it does not indicate if
    /// the current <see cref="SurfaceMotion"/> is non-zero. The simulation has optimizations for
    /// contacts where materials will never have a surface motion. If a material will at any time 
    /// use a non-zero surface motion this flag must be set to <see langword="true"/>, even if the 
    /// current surface motion velocity is zero.
    /// </remarks>
    public bool SupportsSurfaceMotion
    {
      get { return _supportsSurfaceMotion; }
      set { _supportsSurfaceMotion = value; }
    }
    private bool _supportsSurfaceMotion;


    /// <summary>
    /// Gets or sets the velocity of the rigid body surface (in local space of the body).
    /// </summary>
    /// <value>
    /// A velocity that describes the speed and direction in which the surface is moving relative
    /// to the rigid body. The default value is a (0, 0, 0).
    /// </value>
    /// <remarks>
    /// This property can be used to simulate conveyor belts or similar objects.
    /// </remarks>
    public Vector3F SurfaceMotion
    {
      get { return _surfaceMotion; }
      set { _surfaceMotion = value; }
    }
    private Vector3F _surfaceMotion;


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialProperties"/> structure.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialProperties"/> structure.
    /// </summary>
    /// <param name="staticFriction">The static friction.</param>
    /// <param name="dynamicFriction">The dynamic friction.</param>
    /// <param name="restitution">The coefficient of restitution.</param>
    public MaterialProperties(float staticFriction, float dynamicFriction, float restitution)
    {
      _staticFriction = staticFriction;
      _dynamicFriction = dynamicFriction;
      _restitution = restitution;
      _supportsSurfaceMotion = false;
      _surfaceMotion = Vector3F.Zero;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialProperties"/> structure.
    /// </summary>
    /// <param name="staticFriction">The static friction.</param>
    /// <param name="dynamicFriction">The dynamic friction.</param>
    /// <param name="restitution">The coefficient of restitution.</param>
    /// <param name="supportsSurfaceMotion">
    /// If set to <see langword="true"/> the material supports surface motion.
    /// </param>
    /// <param name="surfaceMotion">The surface motion velocity.</param>
    public MaterialProperties(float staticFriction, float dynamicFriction, float restitution, bool supportsSurfaceMotion, Vector3F surfaceMotion)
    {
      _staticFriction = staticFriction;
      _dynamicFriction = dynamicFriction;
      _restitution = restitution;
      _supportsSurfaceMotion = supportsSurfaceMotion;
      _surfaceMotion = surfaceMotion;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialProperties"/> structure.
    /// </summary>
    /// <param name="staticFriction">The static friction.</param>
    /// <param name="dynamicFriction">The dynamic friction.</param>
    /// <param name="restitution">The coefficient of restitution.</param>
    /// <param name="surfaceMotion">The surface motion velocity.</param>
    public MaterialProperties(float staticFriction, float dynamicFriction, float restitution, Vector3F surfaceMotion)
    {
      _staticFriction = staticFriction;
      _dynamicFriction = dynamicFriction;
      _restitution = restitution;
      _supportsSurfaceMotion = true;
      _surfaceMotion = surfaceMotion;
    }


    #region ----- Equality Members -----

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other" />
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(MaterialProperties other)
    {
      // ReSharper disable CompareOfFloatsByEqualityOperator
      return _staticFriction == other._staticFriction
             && _dynamicFriction == other._dynamicFriction
             && _restitution == other._restitution
             && _supportsSurfaceMotion == other._supportsSurfaceMotion
             && _surfaceMotion == other._surfaceMotion;
      // ReSharper restore CompareOfFloatsByEqualityOperator
    }


    /// <summary>
    /// Determines whether the specified <see cref="Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object" /> is equal to this instance;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is MaterialProperties && Equals((MaterialProperties)obj);
    }


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures
    /// like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        var hashCode = _staticFriction.GetHashCode();
        hashCode = (hashCode * 397) ^ _dynamicFriction.GetHashCode();
        hashCode = (hashCode * 397) ^ _restitution.GetHashCode();
        hashCode = (hashCode * 397) ^ _supportsSurfaceMotion.GetHashCode();
        hashCode = (hashCode * 397) ^ _surfaceMotion.GetHashCode();
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <summary>
    /// Compares <see cref="MaterialProperties"/> to determine whether they are the same.
    /// </summary>
    /// <param name="left">The first <see cref="MaterialProperties"/>.</param>
    /// <param name="right">The second <see cref="MaterialProperties"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the 
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(MaterialProperties left, MaterialProperties right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares <see cref="MaterialProperties"/> to determine whether they are different.
    /// </summary>
    /// <param name="left">The first <see cref="MaterialProperties"/>.</param>
    /// <param name="right">The second <see cref="MaterialProperties"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are 
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(MaterialProperties left, MaterialProperties right)
    {
      return !left.Equals(right);
    }
    #endregion

  }
}
