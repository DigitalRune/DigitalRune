// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
#if !NETFX_CORE && !PORTABLE
using DigitalRune.Mathematics.Algebra.Design;
#endif
#if XNA || MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Mathematics.Algebra
{
  /// <summary>
  /// Defines a 4-dimensional vector (double-precision).
  /// </summary>
  /// <remarks>
  /// The four components (x, y, z, w) are stored with double-precision.
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
#if !NETFX_CORE && !PORTABLE
  [TypeConverter(typeof(Vector4DConverter))]
#endif
#if !XBOX && !UNITY
  [DataContract]
#endif
  public struct Vector4D : IEquatable<Vector4D>
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// Returns a <see cref="Vector4D"/> with all of its components set to zero.
    /// </summary>
    public static readonly Vector4D Zero = new Vector4D(0, 0, 0, 0);

    /// <summary>
    /// Returns a <see cref="Vector4D"/> with all of its components set to one.
    /// </summary>
    public static readonly Vector4D One = new Vector4D(1, 1, 1, 1);

    /// <summary>
    /// Returns the x unit <see cref="Vector4D"/> (1, 0, 0, 0).
    /// </summary>
    public static readonly Vector4D UnitX = new Vector4D(1, 0, 0, 0);

    /// <summary>
    /// Returns the y unit <see cref="Vector4D"/> (0, 1, 0, 0).
    /// </summary>
    public static readonly Vector4D UnitY = new Vector4D(0, 1, 0, 0);

    /// <summary>
    /// Returns the z unit <see cref="Vector4D"/> (0, 0, 1, 0).
    /// </summary>
    public static readonly Vector4D UnitZ = new Vector4D(0, 0, 1, 0);

    /// <summary>
    /// Returns the w unit <see cref="Vector4D"/> (0, 0, 0, 1).
    /// </summary>
    public static readonly Vector4D UnitW = new Vector4D(0, 0, 0, 1);
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The x component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public double X;

    /// <summary>
    /// The y component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public double Y;

    /// <summary>
    /// The z component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public double Z;

    /// <summary>
    /// The w component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public double W;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the component at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <value>The component at <paramref name="index"/>.</value>
    /// <remarks>
    /// The index is zero based: x = vector[0], y = vector[1] ... w = vector[3].
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public double this[int index]
    {
      get
      {
        switch (index)
        {
          case 0: return X;
          case 1: return Y;
          case 2: return Z;
          case 3: return W;
          default: throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 to 3.");
        }
      }
      set
      {
        switch (index)
        {
          case 0: X = value; break;
          case 1: Y = value; break;
          case 2: Z = value; break;
          case 3: W = value; break;
          default: throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 to 3.");
        }
      }
    }


    /// <summary>
    /// Gets a value indicating whether a component of the vector is <see cref="double.NaN"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a component of the vector is <see cref="double.NaN"/>; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsNaN
    {
      get { return Numeric.IsNaN(X) || Numeric.IsNaN(Y) || Numeric.IsNaN(Z) || Numeric.IsNaN(W); }
    }


    /// <summary>
    /// Returns a value indicating whether this vector is normalized (the length is numerically
    /// equal to 1).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this vector is numerically normalized; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <see cref="IsNumericallyNormalized"/> compares the length of this vector against 1.0 using
    /// the default tolerance value (see <see cref="Numeric.EpsilonD"/>).
    /// </remarks>
    public bool IsNumericallyNormalized
    {
      get { return Numeric.AreEqual(LengthSquared, 1.0); }
    }


    /// <summary>
    /// Returns a value indicating whether this vector has zero size (the length is numerically
    /// equal to 0).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this vector is numerically zero; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// The length of this vector is compared to 0 using the default tolerance value (see 
    /// <see cref="Numeric.EpsilonD"/>).
    /// </remarks>
    public bool IsNumericallyZero
    {
      get { return Numeric.IsZero(LengthSquared, Numeric.EpsilonDSquared); }
    }


    /// <summary>
    /// Gets or sets the length of this vector.
    /// </summary>
    /// <returns>The length of the this vector.</returns>
    /// <exception cref="MathematicsException">
    /// The vector has a length of 0. The length cannot be changed.
    /// </exception>
    [XmlIgnore]
#if XNA || MONOGAME
    [ContentSerializerIgnore]
#endif
    public double Length
    {
      get
      {
        return Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
      }
      set
      {
        double length = Length;
        if (Numeric.IsZero(length))
          throw new MathematicsException("Cannot change length of a vector with length 0.");

        double scale = value / length;
        X *= scale;
        Y *= scale;
        Z *= scale;
        W *= scale;
      }
    }


    /// <summary>
    /// Returns the squared length of this vector.
    /// </summary>
    /// <returns>The squared length of this vector.</returns>
    public double LengthSquared
    {
      get
      {
        return X * X + Y * Y + Z * Z + W * W;
      }
    }


    /// <summary>
    /// Returns the normalized vector.
    /// </summary>
    /// <value>The normalized vector.</value>
    /// <remarks>
    /// The property does not change this instance. To normalize this instance you need to call 
    /// <see cref="Normalize"/>.
    /// </remarks>
    /// <exception cref="DivideByZeroException">
    /// The length of the vector is zero. The quaternion cannot be normalized.
    /// </exception>
    public Vector4D Normalized
    {
      get
      {
        Vector4D v = this;
        v.Normalize();
        return v;
      }
    }


    /// <summary>
    /// Gets or sets the components x, y and z as a <see cref="Vector3D"/>.
    /// </summary>
    /// <value>The 3-dimensional vector (x, y, z).</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
    public Vector3D XYZ
    {
      get { return new Vector3D(X, Y, Z); }
      set
      {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
      }
    }


    /// <summary>
    /// Gets the value of the largest component.
    /// </summary>
    /// <value>The value of the largest component.</value>
    public double LargestComponent
    {
      get
      {
        if (X >= Y && X >= Z && X >= W)
          return X;

        if (Y >= Z && Y >= W)
          return Y;

        if (Z >= W)
          return Z;

        return W;
      }
    }


    /// <summary>
    /// Gets the index (zero-based) of the largest component.
    /// </summary>
    /// <value>The index (zero-based) of the largest component.</value>
    /// <remarks>
    /// <para>
    /// This method returns the index of the component (X, Y, Z or W) which has the largest value. 
    /// The index is zero-based, i.e. the index of X is 0. 
    /// </para>
    /// <para>
    /// If there are several components with equally large values, the smallest index of these is 
    /// returned.
    /// </para>
    /// </remarks>
    public int IndexOfLargestComponent
    {
      get
      {
        if (X >= Y && X >= Z && X >= W)
          return 0;

        if (Y >= Z && Y >= W)
          return 1;

        if (Z >= W)
          return 2;

        return 3;
      }
    }


    /// <summary>
    /// Gets the value of the largest component.
    /// </summary>
    /// <value>The value of the largest component.</value>
    public double SmallestComponent
    {
      get
      {
        if (X <= Y && X <= Z && X <= W)
          return X;

        if (Y <= Z && Y <= W)
          return Y;

        if (Z <= W)
          return Z;

        return W;
      }
    }


    /// <summary>
    /// Gets the index (zero-based) of the smallest component.
    /// </summary>
    /// <value>The index (zero-based) of the smallest component.</value>
    /// <remarks>
    /// <para>
    /// This method returns the index of the component (X, Y, Z or W) which has the smallest value. 
    /// The index is zero-based, i.e. the index of X is 0. 
    /// </para>
    /// <para>
    /// If there are several components with equally small values, the smallest index of these is 
    /// returned.
    /// </para>
    /// </remarks>
    public int IndexOfSmallestComponent
    {
      get
      {
        if (X <= Y && X <= Z && X <= W)
          return 0;

        if (Y <= Z && Y <= W)
          return 1;

        if (Z <= W)
          return 2;

        return 3;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of <see cref="Vector4D"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of <see cref="Vector4D"/>.
    /// </summary>
    /// <param name="x">Initial value for the x component.</param>
    /// <param name="y">Initial value for the y component.</param>
    /// <param name="z">Initial value for the z component.</param>
    /// <param name="w">Initial value for the z component.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Vector4D(double x, double y, double z, double w)
    {
      X = x;
      Y = y;
      Z = z;
      W = w;
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Vector4D"/>.
    /// </summary>
    /// <param name="componentValue">The initial value for 4 the vector components.</param>
    /// <remarks>
    /// All components are set to <paramref name="componentValue"/>.
    /// </remarks>
    public Vector4D(double componentValue)
    {
      X = componentValue;
      Y = componentValue;
      Z = componentValue;
      W = componentValue;
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Vector4D"/>.
    /// </summary>
    /// <param name="components">
    /// Array with the initial values for the components x, y, z and w.
    /// </param>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="components"/> has less than 4 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="components"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public Vector4D(double[] components)
    {
      X = components[0];
      Y = components[1];
      Z = components[2];
      W = components[3];
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Vector4D"/>.
    /// </summary>
    /// <param name="components">
    /// List with the initial values for the components x, y, z and w.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="components"/> has less than 4 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="components"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public Vector4D(IList<double> components)
    {
      X = components[0];
      Y = components[1];
      Z = components[2];
      W = components[3];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Vector4D"/> class.
    /// </summary>
    /// <param name="vector">The vector (x, y, z).</param>
    /// <param name="w">The w component.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Vector4D(Vector3D vector, double w)
    {
      X = vector.X;
      Y = vector.Y;
      Z = vector.Z;
      W = w;
    }
    #endregion


    //--------------------------------------------------------------
    #region Interfaces and Overrides
    //--------------------------------------------------------------

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        int hashCode = X.GetHashCode();
        hashCode = (hashCode * 397) ^ Y.GetHashCode();
        hashCode = (hashCode * 397) ^ Z.GetHashCode();
        hashCode = (hashCode * 397) ^ W.GetHashCode();
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <overloads>
    /// <summary>
    /// Indicates whether a vector and a another object are equal.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <param name="obj">Another object to compare to.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> and this instance are the same type and
    /// represent the same value; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is Vector4D && this == (Vector4D)obj;
    }


    #region IEquatable<Vector4D> Members
    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(Vector4D other)
    {
      return this == other;
    }
    #endregion


    /// <overloads>
    /// <summary>
    /// Returns the string representation of a vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Returns the string representation of this vector.
    /// </summary>
    /// <returns>The string representation of this vector.</returns>
    public override string ToString()
    {
      return ToString(CultureInfo.CurrentCulture);
    }


    /// <summary>
    /// Returns the string representation of this vector using the specified culture-specific format
    /// information.
    /// </summary>
    /// <param name="provider">
    /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
    /// </param>
    /// <returns>The string representation of this vector.</returns>
    public string ToString(IFormatProvider provider)
    {
      return string.Format(provider, "({0}; {1}; {2}; {3})", X, Y, Z, W);
    }
    #endregion


    //--------------------------------------------------------------
    #region Overloaded Operators
    //--------------------------------------------------------------

    /// <summary>
    /// Negates a vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The negated vector.</returns>
    public static Vector4D operator -(Vector4D vector)
    {
      vector.X = -vector.X;
      vector.Y = -vector.Y;
      vector.Z = -vector.Z;
      vector.W = -vector.W;
      return vector;
    }


    /// <summary>
    /// Negates a vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The negated vector.</returns>
    public static Vector4D Negate(Vector4D vector)
    {
      vector.X = -vector.X;
      vector.Y = -vector.Y;
      vector.Z = -vector.Z;
      vector.W = -vector.W;
      return vector;
    }


    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    public static Vector4D operator +(Vector4D vector1, Vector4D vector2)
    {
      vector1.X += vector2.X;
      vector1.Y += vector2.Y;
      vector1.Z += vector2.Z;
      vector1.W += vector2.W;
      return vector1;
    }


    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    public static Vector4D Add(Vector4D vector1, Vector4D vector2)
    {
      vector1.X += vector2.X;
      vector1.Y += vector2.Y;
      vector1.Z += vector2.Z;
      vector1.W += vector2.W;
      return vector1;
    }


    /// <summary>
    /// Subtracts a vector from a vector.
    /// </summary>
    /// <param name="minuend">The first vector (minuend).</param>
    /// <param name="subtrahend">The second vector (subtrahend).</param>
    /// <returns>The difference of the two vectors.</returns>
    public static Vector4D operator -(Vector4D minuend, Vector4D subtrahend)
    {
      minuend.X -= subtrahend.X;
      minuend.Y -= subtrahend.Y;
      minuend.Z -= subtrahend.Z;
      minuend.W -= subtrahend.W;
      return minuend;
    }


    /// <summary>
    /// Subtracts a vector from a vector.
    /// </summary>
    /// <param name="minuend">The first vector (minuend).</param>
    /// <param name="subtrahend">The second vector (subtrahend).</param>
    /// <returns>The difference of the two vectors.</returns>
    public static Vector4D Subtract(Vector4D minuend, Vector4D subtrahend)
    {
      minuend.X -= subtrahend.X;
      minuend.Y -= subtrahend.Y;
      minuend.Z -= subtrahend.Z;
      minuend.W -= subtrahend.W;
      return minuend;
    }


    /// <overloads>
    /// <summary>
    /// Multiplies a vector by a scalar or a vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The vector with each component multiplied by scalar.</returns>
    public static Vector4D operator *(Vector4D vector, double scalar)
    {
      vector.X *= scalar;
      vector.Y *= scalar;
      vector.Z *= scalar;
      vector.W *= scalar;
      return vector;
    }


    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The vector with each component multiplied by <paramref name="scalar"/>.</returns>
    public static Vector4D operator *(double scalar, Vector4D vector)
    {
      vector.X *= scalar;
      vector.Y *= scalar;
      vector.Z *= scalar;
      vector.W *= scalar;
      return vector;
    }


    /// <overloads>
    /// <summary>
    /// Multiplies a vector by a scalar or a vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The vector with each component multiplied by <paramref name="scalar"/>.</returns>
    public static Vector4D Multiply(double scalar, Vector4D vector)
    {
      vector.X *= scalar;
      vector.Y *= scalar;
      vector.Z *= scalar;
      vector.W *= scalar;
      return vector;
    }


    /// <summary>
    /// Multiplies the components of two vectors by each other.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The component-wise product of the two vectors.</returns>
    public static Vector4D operator *(Vector4D vector1, Vector4D vector2)
    {
      vector1.X *= vector2.X;
      vector1.Y *= vector2.Y;
      vector1.Z *= vector2.Z;
      vector1.W *= vector2.W;
      return vector1;
    }


    /// <summary>
    /// Multiplies the components of two vectors by each other.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The component-wise product of the two vectors.</returns>
    public static Vector4D Multiply(Vector4D vector1, Vector4D vector2)
    {
      vector1.X *= vector2.X;
      vector1.Y *= vector2.Y;
      vector1.Z *= vector2.Z;
      vector1.W *= vector2.W;
      return vector1;
    }


    /// <overloads>
    /// <summary>
    /// Divides the vector by a scalar or a vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Divides a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The vector with each component divided by <paramref name="scalar"/>.</returns>
    public static Vector4D operator /(Vector4D vector, double scalar)
    {
      double f = 1 / scalar;
      vector.X *= f;
      vector.Y *= f;
      vector.Z *= f;
      vector.W *= f;
      return vector;
    }


    /// <overloads>
    /// <summary>
    /// Divides the vector by a scalar or a vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Divides a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The vector with each component divided by <paramref name="scalar"/>.</returns>
    public static Vector4D Divide(Vector4D vector, double scalar)
    {
      double f = 1 / scalar;
      vector.X *= f;
      vector.Y *= f;
      vector.Z *= f;
      vector.W *= f;
      return vector;
    }


    /// <summary>
    /// Divides the components of a vector by the components of another 
    /// vector.
    /// </summary>
    /// <param name="dividend">The first vector (dividend).</param>
    /// <param name="divisor">The second vector (divisor).</param>
    /// <returns>The component-wise product of the two vectors.</returns>
    public static Vector4D operator /(Vector4D dividend, Vector4D divisor)
    {
      dividend.X /= divisor.X;
      dividend.Y /= divisor.Y;
      dividend.Z /= divisor.Z;
      dividend.W /= divisor.W;
      return dividend;
    }


    /// <summary>
    /// Divides the components of a vector by the components of another 
    /// vector.
    /// </summary>
    /// <param name="dividend">The first vector (dividend).</param>
    /// <param name="divisor">The second vector (divisor).</param>
    /// <returns>The component-wise division of the two vectors.</returns>
    public static Vector4D Divide(Vector4D dividend, Vector4D divisor)
    {
      dividend.X /= divisor.X;
      dividend.Y /= divisor.Y;
      dividend.Z /= divisor.Z;
      dividend.W /= divisor.W;
      return dividend;
    }


    /// <summary>
    /// Tests if two vectors are equal.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if the vectors are equal; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// For the test the corresponding components of the vectors are compared.
    /// </remarks>
    public static bool operator ==(Vector4D vector1, Vector4D vector2)
    {
      return vector1.X == vector2.X
          && vector1.Y == vector2.Y
          && vector1.Z == vector2.Z
          && vector1.W == vector2.W;
    }


    /// <summary>
    /// Tests if two vectors are not equal.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if the vectors are different; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// For the test the corresponding components of the vectors are compared.
    /// </remarks>
    public static bool operator !=(Vector4D vector1, Vector4D vector2)
    {
      return vector1.X != vector2.X
          || vector1.Y != vector2.Y
          || vector1.Z != vector2.Z
          || vector1.W != vector2.W;
    }


    /// <summary>
    /// Tests if each component of a vector is greater than the corresponding component of another
    /// vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is greater than its
    /// counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator >(Vector4D vector1, Vector4D vector2)
    {
      return vector1.X > vector2.X
          && vector1.Y > vector2.Y
          && vector1.Z > vector2.Z
          && vector1.W > vector2.W;
    }


    /// <summary>
    /// Tests if each component of a vector is greater or equal than the corresponding component of
    /// another vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is greater or equal
    /// than its counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator >=(Vector4D vector1, Vector4D vector2)
    {
      return vector1.X >= vector2.X
          && vector1.Y >= vector2.Y
          && vector1.Z >= vector2.Z
          && vector1.W >= vector2.W;
    }


    /// <summary>
    /// Tests if each component of a vector is less than the corresponding component of another
    /// vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is less than its 
    /// counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator <(Vector4D vector1, Vector4D vector2)
    {
      return vector1.X < vector2.X
          && vector1.Y < vector2.Y
          && vector1.Z < vector2.Z
          && vector1.W < vector2.W;
    }


    /// <summary>
    /// Tests if each component of a vector is less or equal than the corresponding component of
    /// another vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is less or equal than
    /// its counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator <=(Vector4D vector1, Vector4D vector2)
    {
      return vector1.X <= vector2.X
          && vector1.Y <= vector2.Y
          && vector1.Z <= vector2.Z
          && vector1.W <= vector2.W;
    }


    /// <overloads>
    /// <summary>
    /// Converts a vector to another data type.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts a vector to an array of 4 <see langword="double"/> values.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>
    /// The array with 4 <see langword="double"/> values. The order of the elements is: x, y, z, w
    /// </returns>
    public static explicit operator double[](Vector4D vector)
    {
      return new[] { vector.X, vector.Y, vector.Z, vector.W };
    }


    /// <summary>
    /// Converts this vector to an array of 4 <see langword="double"/> values.
    /// </summary>
    /// <returns>
    /// The array with 4 <see langword="double"/> values. The order of the elements is: x, y, z, w
    /// </returns>
    public double[] ToArray()
    {
      return (double[])this;
    }


    /// <summary>
    /// Converts a vector to a list of 4 <see langword="double"/> values.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>
    /// The list with 4 <see langword="double"/> values. The order of the elements is: x, y, z, w
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    public static explicit operator List<double>(Vector4D vector)
    {
      List<double> result = new List<double>(4) { vector.X, vector.Y, vector.Z, vector.W };
      return result;
    }


    /// <summary>
    /// Converts this vector to a list of 4 <see langword="double"/> values.
    /// </summary>
    /// <returns>
    /// The list with 4 <see langword="double"/> values. The order of the elements is: x, y, z, w
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    public List<double> ToList()
    {
      return (List<double>)this;
    }


    /// <summary>
    /// Performs an explicit conversion from <see cref="Vector4D"/> to <see cref="Vector4F"/>.
    /// </summary>
    /// <param name="vector">The DigitalRune <see cref="Vector4D"/>.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator Vector4F(Vector4D vector)
    {
      return new Vector4F((float)vector.X, (float)vector.Y, (float)vector.Z, (float)vector.W);
    }


    /// <summary>
    /// Converts this <see cref="Vector4D"/> to <see cref="Vector4F"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public Vector4F ToVector4F()
    {
      return new Vector4F((float)X, (float)Y, (float)Z, (float)W);
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="Vector4D"/> to <see cref="VectorD"/>.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator VectorD(Vector4D vector)
    {
      VectorD result = new VectorD(4);
      result[0] = vector.X; 
      result[1] = vector.Y; 
      result[2] = vector.Z; 
      result[3] = vector.W;
      return result;
    }


    /// <summary>
    /// Converts this <see cref="Vector4D"/> to <see cref="VectorD"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public VectorD ToVectorD()
    {
      return this;
    }


#if XNA || MONOGAME
    /// <summary>
    /// Performs an conversion from <see cref="Vector4"/> (XNA Framework) to <see cref="Vector4D"/>
    /// (DigitalRune Mathematics).
    /// </summary>
    /// <param name="vector">The <see cref="Vector4"/> (XNA Framework).</param>
    /// <returns>The <see cref="Vector4D"/> (DigitalRune Mathematics).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    public static explicit operator Vector4D(Vector4 vector)
    {
      return new Vector4D(vector.X, vector.Y, vector.Z, vector.W);
    }


    /// <summary>
    /// Converts this <see cref="Vector4D"/> (DigitalRune Mathematics) to <see cref="Vector4"/> 
    /// (XNA Framework).
    /// </summary>
    /// <param name="vector">The <see cref="Vector4"/> (XNA Framework).</param>
    /// <returns>The <see cref="Vector4D"/> (DigitalRune Mathematics).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the 
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Vector4D FromXna(Vector4 vector)
    {
      return new Vector4D(vector.X, vector.Y, vector.Z, vector.W);
    }


    /// <summary>
    /// Performs an conversion from <see cref="Vector4D"/> (DigitalRune Mathematics) to 
    /// <see cref="Vector4"/> (XNA Framework).
    /// </summary>
    /// <param name="vector">The <see cref="Vector4D"/> (DigitalRune Mathematics).</param>
    /// <returns>The <see cref="Vector4"/> (XNA Framework).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    public static explicit operator Vector4(Vector4D vector)
    {
      return new Vector4((float)vector.X, (float)vector.Y, (float)vector.Z, (float)vector.W);
    }


    /// <summary>
    /// Converts this <see cref="Vector4D"/> (DigitalRune Mathematics) to <see cref="Vector4"/> 
    /// (XNA Framework).
    /// </summary>
    /// <returns>The <see cref="Vector4"/> (XNA Framework).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the 
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Vector4 ToXna()
    {
      return new Vector4((float)X, (float)Y, (float)Z, (float)W);
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Sets each vector component to its absolute value.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets each vector component to its absolute value.
    /// </summary>
    public void Absolute()
    {
      X = Math.Abs(X);
      Y = Math.Abs(Y);
      Z = Math.Abs(Z);
      W = Math.Abs(W);
    }


    /// <overloads>
    /// <summary>
    /// Clamps the vector components to the range [min, max].
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Clamps the vector components to the range [min, max].
    /// </summary>
    /// <param name="min">The min limit.</param>
    /// <param name="max">The max limit.</param>
    /// <remarks>
    /// This operation is carried out per component. Component values less than 
    /// <paramref name="min"/> are set to <paramref name="min"/>. Component values greater than 
    /// <paramref name="max"/> are set to <paramref name="max"/>.
    /// </remarks>
    public void Clamp(double min, double max)
    {
      X = MathHelper.Clamp(X, min, max);
      Y = MathHelper.Clamp(Y, min, max);
      Z = MathHelper.Clamp(Z, min, max);
      W = MathHelper.Clamp(W, min, max);
    }


    /// <overloads>
    /// <summary>
    /// Clamps near-zero vector components to zero.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Clamps near-zero vector components to zero.
    /// </summary>
    /// <remarks>
    /// Each vector component is compared to zero. If the component is in the interval 
    /// [-<see cref="Numeric.EpsilonD"/>, +<see cref="Numeric.EpsilonD"/>] it is set to zero, 
    /// otherwise it remains unchanged.
    /// </remarks>
    public void ClampToZero()
    {
      X = Numeric.ClampToZero(X);
      Y = Numeric.ClampToZero(Y);
      Z = Numeric.ClampToZero(Z);
      W = Numeric.ClampToZero(W);
    }


    /// <summary>
    /// Clamps near-zero vector components to zero.
    /// </summary>
    /// <param name="epsilon">The tolerance value.</param>
    /// <remarks>
    /// Each vector component is compared to zero. If the component is in the interval 
    /// [-<paramref name="epsilon"/>, +<paramref name="epsilon"/>] it is set to zero, otherwise it 
    /// remains unchanged.
    /// </remarks>
    public void ClampToZero(double epsilon)
    {
      X = Numeric.ClampToZero(X, epsilon);
      Y = Numeric.ClampToZero(Y, epsilon);
      Z = Numeric.ClampToZero(Z, epsilon);
      W = Numeric.ClampToZero(W, epsilon);
    }


    /// <summary>
    /// Normalizes the vector.
    /// </summary>
    /// <remarks>
    /// A vectors is normalized by dividing its components by the length of the vector.
    /// </remarks>
    /// <exception cref="DivideByZeroException">
    /// The length of this vector is zero. The vector cannot be normalized.
    /// </exception>
    public void Normalize()
    {
      double length = Length;
      if (Numeric.IsZero(length))
        throw new DivideByZeroException("Cannot normalize a vector with length 0.");

      double scale = 1 / length;
      X *= scale;
      Y *= scale;
      Z *= scale;
      W *= scale;
    }


    /// <summary>
    /// Tries to normalize the vector.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the vector was normalized; otherwise, <see langword="false"/> if 
    /// the vector could not be normalized. (The length is numerically zero.)
    /// </returns>
    public bool TryNormalize()
    {
      double lengthSquared = LengthSquared;
      if (Numeric.IsZero(lengthSquared, Numeric.EpsilonDSquared))
        return false;

      double length = Math.Sqrt(lengthSquared);

      double scale = 1.0 / length;
      X *= scale;
      Y *= scale;
      Z *= scale;
      W *= scale;

      return true;
    }


    /// <overloads>
    /// <summary>
    /// Projects a vector onto another vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets this vector to its projection onto the axis given by the target vector.
    /// </summary>
    /// <param name="target">The target vector.</param>
    public void ProjectTo(Vector4D target)
    {
      this = Dot(this, target) / target.LengthSquared * target;
    }
    #endregion


    //--------------------------------------------------------------
    #region Static Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns a vector with the absolute values of the elements of the given vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>A vector with the absolute values of the elements of the given vector.</returns>
    /// <remarks>
    /// The original vector is copied and then each vector element is set to its absolute value (see
    /// <see cref="Math.Abs(float)"/>).
    /// </remarks>
    public static Vector4D Absolute(Vector4D vector)
    {
      return new Vector4D(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z), Math.Abs(vector.W));
    }


    /// <overloads>
    /// <summary>
    /// Determines whether two vectors are equal (regarding a given tolerance).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether two vectors are equal (regarding the tolerance 
    /// <see cref="Numeric.EpsilonD"/>).
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if the vectors are equal (within the tolerance 
    /// <see cref="Numeric.EpsilonD"/>); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The two vectors are compared component-wise. If the differences of the components are less
    /// than <see cref="Numeric.EpsilonD"/> the vectors are considered as being equal.
    /// </remarks>
    public static bool AreNumericallyEqual(Vector4D vector1, Vector4D vector2)
    {
      return Numeric.AreEqual(vector1.X, vector2.X)
          && Numeric.AreEqual(vector1.Y, vector2.Y)
          && Numeric.AreEqual(vector1.Z, vector2.Z)
          && Numeric.AreEqual(vector1.W, vector2.W);
    }


    /// <summary>
    /// Determines whether two vectors are equal (regarding a specific tolerance).
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <param name="epsilon">The tolerance value.</param>
    /// <returns>
    /// <see langword="true"/> if the vectors are equal (within the tolerance 
    /// <paramref name="epsilon"/>); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The two vectors are compared component-wise. If the differences of the components are less
    /// than <paramref name="epsilon"/> the vectors are considered as being equal.
    /// </remarks>
    public static bool AreNumericallyEqual(Vector4D vector1, Vector4D vector2, double epsilon)
    {
      return Numeric.AreEqual(vector1.X, vector2.X, epsilon)
          && Numeric.AreEqual(vector1.Y, vector2.Y, epsilon)
          && Numeric.AreEqual(vector1.Z, vector2.Z, epsilon)
          && Numeric.AreEqual(vector1.W, vector2.W, epsilon);
    }


    /// <summary>
    /// Returns a vector with the vector components clamped to the range [min, max].
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="min">The min limit.</param>
    /// <param name="max">The max limit.</param>
    /// <returns>A vector with clamped components.</returns>
    /// <remarks>
    /// This operation is carried out per component. Component values less than 
    /// <paramref name="min"/> are set to <paramref name="min"/>. Component values greater than 
    /// <paramref name="max"/> are set to <paramref name="max"/>.
    /// </remarks>
    public static Vector4D Clamp(Vector4D vector, double min, double max)
    {
      return new Vector4D(MathHelper.Clamp(vector.X, min, max),
                          MathHelper.Clamp(vector.Y, min, max),
                          MathHelper.Clamp(vector.Z, min, max),
                          MathHelper.Clamp(vector.W, min, max));
    }


    /// <summary>
    /// Returns a vector with near-zero vector components clamped to 0.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The vector with small components clamped to zero.</returns>
    /// <remarks>
    /// Each vector component (X, Y, Z and W) is compared to zero. If the component is in the
    /// interval [-<see cref="Numeric.EpsilonD"/>, +<see cref="Numeric.EpsilonD"/>] it is set to 
    /// zero, otherwise it remains unchanged.
    /// </remarks>
    public static Vector4D ClampToZero(Vector4D vector)
    {
      vector.X = Numeric.ClampToZero(vector.X);
      vector.Y = Numeric.ClampToZero(vector.Y);
      vector.Z = Numeric.ClampToZero(vector.Z);
      vector.W = Numeric.ClampToZero(vector.W);
      return vector;
    }


    /// <summary>
    /// Returns a vector with near-zero vector components clamped to 0.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="epsilon">The tolerance value.</param>
    /// <returns>The vector with small components clamped to zero.</returns>
    /// <remarks>
    /// Each vector component (X, Y, Z and W) is compared to zero. If the component is in the
    /// interval [-<paramref name="epsilon"/>, +<paramref name="epsilon"/>] it is set to zero, 
    /// otherwise it remains unchanged.
    /// </remarks>
    public static Vector4D ClampToZero(Vector4D vector, double epsilon)
    {
      vector.X = Numeric.ClampToZero(vector.X, epsilon);
      vector.Y = Numeric.ClampToZero(vector.Y, epsilon);
      vector.Z = Numeric.ClampToZero(vector.Z, epsilon);
      vector.W = Numeric.ClampToZero(vector.W, epsilon);
      return vector;
    }


    /// <summary>
    /// Returns the dot product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The dot product.</returns>
    /// <remarks>
    /// The method calculates the dot product (also known as scalar product or inner product).
    /// </remarks>
    public static double Dot(Vector4D vector1, Vector4D vector2)
    {
      return vector1.X * vector2.X
           + vector1.Y * vector2.Y
           + vector1.Z * vector2.Z
           + vector1.W * vector2.W;
    }


    /// <summary>
    /// Performs the homogeneous divide or perspective divide: X, Y and Z are divided by W.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The vector (X/W, Y/W, Z/W).</returns>
    /// <exception cref="DivideByZeroException">
    /// Component W is 0.
    /// </exception>
    public static Vector3D HomogeneousDivide(Vector4D vector)
    {
      double w = vector.W;

      if (w == 1.0)
        return new Vector3D(vector.X, vector.Y, vector.Z);

      double oneOverW = 1 / w;
      return new Vector3D(vector.X * oneOverW, vector.Y * oneOverW, vector.Z * oneOverW);
    }


    /// <summary>
    /// Returns a vector that contains the lowest value from each matching pair of components.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The minimized vector.</returns>
    public static Vector4D Min(Vector4D vector1, Vector4D vector2)
    {
      vector1.X = Math.Min(vector1.X, vector2.X);
      vector1.Y = Math.Min(vector1.Y, vector2.Y);
      vector1.Z = Math.Min(vector1.Z, vector2.Z);
      vector1.W = Math.Min(vector1.W, vector2.W);
      return vector1;
    }


    /// <summary>
    /// Returns a vector that contains the highest value from each matching pair of components.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The maximized vector.</returns>
    public static Vector4D Max(Vector4D vector1, Vector4D vector2)
    {
      vector1.X = Math.Max(vector1.X, vector2.X);
      vector1.Y = Math.Max(vector1.Y, vector2.Y);
      vector1.Z = Math.Max(vector1.Z, vector2.Z);
      vector1.W = Math.Max(vector1.W, vector2.W);
      return vector1;
    }


    /// <summary>
    /// Projects a vector onto an axis given by the target vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="target">The target vector.</param>
    /// <returns>
    /// The projection of <paramref name="vector"/> onto <paramref name="target"/>.
    /// </returns>
    public static Vector4D ProjectTo(Vector4D vector, Vector4D target)
    {
      return Dot(vector, target) / target.LengthSquared * target;
    }



    /// <overloads>
    /// <summary>
    /// Converts the string representation of a 4-dimensional vector to its <see cref="Vector4D"/>
    /// equivalent.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts the string representation of a 4-dimensional vector to its <see cref="Vector4D"/>
    /// equivalent.
    /// </summary>
    /// <param name="s">A string representation of a 4-dimensional vector.</param>
    /// <returns>
    /// A <see cref="Vector4D"/> that represents the vector specified by the <paramref name="s"/>
    /// parameter.
    /// </returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not a valid <see cref="Vector4D"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Vector4D Parse(string s)
    {
      return Parse(s, CultureInfo.CurrentCulture);
    }


    /// <summary>
    /// Converts the string representation of a 4-dimensional vector in a specified culture-specific
    /// format to its <see cref="Vector4D"/> equivalent.
    /// </summary>
    /// <param name="s">A string representation of a 4-dimensional vector.</param>
    /// <param name="provider">
    /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information about
    /// <paramref name="s"/>. 
    /// </param>
    /// <returns>
    /// A <see cref="Vector4D"/> that represents the vector specified by the <paramref name="s"/>
    /// parameter.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not a valid <see cref="Vector4D"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Vector4D Parse(string s, IFormatProvider provider)
    {
      Match m = Regex.Match(s, @"\((?<x>.*);(?<y>.*);(?<z>.*);(?<w>.*)\)", RegexOptions.None);
      if (m.Success)
      {
        return new Vector4D(
          double.Parse(m.Groups["x"].Value, provider),
          double.Parse(m.Groups["y"].Value, provider),
          double.Parse(m.Groups["z"].Value, provider),
          double.Parse(m.Groups["w"].Value, provider));
      }

      throw new FormatException("String is not a valid Vector4D.");
    }
    #endregion
  }
}
