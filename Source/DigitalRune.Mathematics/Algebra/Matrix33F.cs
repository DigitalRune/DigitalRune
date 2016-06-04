// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;


namespace DigitalRune.Mathematics.Algebra
{
  /// <summary>
  /// Defines a 3 x 3 matrix (single-precision).
  /// </summary>
  /// <remarks>
  /// All indices are zero-based. The matrix looks like this:
  /// <code>
  /// M00 M01 M02
  /// M10 M11 M12
  /// M20 M21 M22
  /// </code>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
  [TypeConverter(typeof(ExpandableObjectConverter))]
#endif
#if !XBOX && !UNITY
  [DataContract]
#endif
  public struct Matrix33F : IEquatable<Matrix33F>
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// Returns a <see cref="Matrix33F"/> with all of its components set to zero.
    /// </summary>
    public static readonly Matrix33F Zero = new Matrix33F(0, 0, 0,
                                                          0, 0, 0,
                                                          0, 0, 0);

    /// <summary>
    /// Returns a <see cref="Matrix33F"/> with all of its components set to one.
    /// </summary>
    public static readonly Matrix33F One = new Matrix33F(1, 1, 1,
                                                         1, 1, 1,
                                                         1, 1, 1);

    /// <summary>
    /// Returns the 3 x 3 identity matrix.
    /// </summary>
    public static readonly Matrix33F Identity = new Matrix33F(1, 0, 0,
                                                              0, 1, 0,
                                                              0, 0, 1);
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The element in first row, first column.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public float M00;

    /// <summary>
    /// The element in first row, second column.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public float M01;

    /// <summary>
    /// The element in first row, third column.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public float M02;

    /// <summary>
    /// The element in second row, first column.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public float M10;

    /// <summary>
    /// The element in second row, second column.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public float M11;

    /// <summary>
    /// The element in second row, third column.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public float M12;

    /// <summary>
    /// The element in third row, first column.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public float M20;

    /// <summary>
    /// The element in third row, second column.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public float M21;

    /// <summary>
    /// The element in third row, third column.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public float M22;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <value>The element at <paramref name="index"/>.</value>
    /// <remarks>
    /// The matrix elements are in row-major order.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public float this[int index]
    {
      get
      {
        switch (index)
        {
          case 0: return M00;
          case 1: return M01;
          case 2: return M02;
          case 3: return M10;
          case 4: return M11;
          case 5: return M12;
          case 6: return M20;
          case 7: return M21;
          case 8: return M22;
          default:
            throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 to 8.");
        }
      }
      set
      {
        switch (index)
        {
          case 0: M00 = value; break;
          case 1: M01 = value; break;
          case 2: M02 = value; break;
          case 3: M10 = value; break;
          case 4: M11 = value; break;
          case 5: M12 = value; break;
          case 6: M20 = value; break;
          case 7: M21 = value; break;
          case 8: M22 = value; break;
          default:
            throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 to 8.");
        }
      }
    }


    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <value>The element at the specified row and column.</value>
    /// <remarks>
    /// The indices are zero-based: [0,0] is the first element, [2,2] is the last element.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The index [<paramref name="row"/>, <paramref name="column"/>] is out of range.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
    public float this[int row, int column]
    {
      get
      {
        switch (row)
        {
          case 0:
            switch (column)
            {
              case 0: return M00;
              case 1: return M01;
              case 2: return M02;
              default:
                throw new ArgumentOutOfRangeException("column", "The column index is out of range. Allowed values are 0, 1, or 2.");
            }
          case 1:
            switch (column)
            {
              case 0: return M10;
              case 1: return M11;
              case 2: return M12;
              default:
                throw new ArgumentOutOfRangeException("column", "The column index is out of range. Allowed values are 0, 1, or 2.");
            }
          case 2:
            switch (column)
            {
              case 0: return M20;
              case 1: return M21;
              case 2: return M22;
              default:
                throw new ArgumentOutOfRangeException("column", "The column index is out of range. Allowed values are 0, 1, or 2.");
            }
          default:
            throw new ArgumentOutOfRangeException("row", "The row index is out of range. Allowed values are 0, 1, or 2.");
        }
      }

      set
      {
        switch (row)
        {
          case 0:
            switch (column)
            {
              case 0: M00 = value; break;
              case 1: M01 = value; break;
              case 2: M02 = value; break;
              default:
                throw new ArgumentOutOfRangeException("column", "The column index is out of range. Allowed values are 0, 1, or 2.");
            }
            break;
          case 1:
            switch (column)
            {
              case 0: M10 = value; break;
              case 1: M11 = value; break;
              case 2: M12 = value; break;
              default:
                throw new ArgumentOutOfRangeException("column", "The column index is out of range. Allowed values are 0, 1, or 2.");
            }
            break;
          case 2:
            switch (column)
            {
              case 0: M20 = value; break;
              case 1: M21 = value; break;
              case 2: M22 = value; break;
              default:
                throw new ArgumentOutOfRangeException("column", "The column index is out of range. Allowed values are 0, 1, or 2.");
            }
            break;
          default:
            throw new ArgumentOutOfRangeException("row", "The row index is out of range. Allowed values are 0, 1, or 2.");
        }
      }
    }


    /// <summary>
    /// Returns the determinant of this matrix.
    /// </summary>
    /// <value>The determinant of this matrix.</value>
    public float Determinant
    {
      get
      {
        // Following the rule of Sarrus:
        // Develop after first row
        return M00 * (M11 * M22 - M12 * M21)
               - M01 * (M10 * M22 - M12 * M20)
               + M02 * (M10 * M21 - M20 * M11);
      }
    }


    /// <summary>
    /// Gets a value indicating whether an element of the matrix is <see cref="float.NaN"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if an element of the matrix is <see cref="float.NaN"/>; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsNaN
    {
      get
      {
        return Numeric.IsNaN(M00) || Numeric.IsNaN(M01) || Numeric.IsNaN(M02)
               || Numeric.IsNaN(M10) || Numeric.IsNaN(M11) || Numeric.IsNaN(M12)
               || Numeric.IsNaN(M20) || Numeric.IsNaN(M21) || Numeric.IsNaN(M22);
      }
    }


    /// <summary>
    /// Gets a value indicating whether this instance is orthogonal.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is an orthogonal matrix; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsOrthogonal
    {
      get
      {
        // Orthogonal = The inverse is the same as the transposed.
        // Note: The normal Numeric.EpsilonF is too low in practice!
        return AreNumericallyEqual(Identity, this * Transposed, Numeric.EpsilonF * 10);
      }
    }


    /// <summary>
    /// Gets a value indicating whether this instance is a rotation matrix.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is a rotation matrix; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsRotation
    {
      get
      {
        // Rotation matrices are orthogonal matrices where the determinant is +1.
        return Numeric.AreEqual(1, Determinant) && IsOrthogonal;
      }
    }


    /// <summary>
    /// Gets a value indicating whether this matrix is symmetric.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this matrix is symmetric; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// The matrix elements are compared for equality - no tolerance value to handle numerical
    /// errors is used.
    /// </remarks>
    public bool IsSymmetric
    {
      get { return M01 == M10 && M02 == M20 && M12 == M21; }
    }


    /// <summary>
    /// Gets the matrix trace (the sum of the diagonal elements).
    /// </summary>
    /// <value>The matrix trace.</value>
    public float Trace
    {
      get
      {
        return M00 + M11 + M22;
      }
    }


    /// <summary>
    /// Returns the transposed of this matrix.
    /// </summary>
    /// <returns>The transposed of this matrix.</returns>
    /// <remarks>
    /// The property does not change this instance. To transpose this instance you need to call 
    /// <see cref="Transpose"/>.
    /// </remarks>
    public Matrix33F Transposed
    {
      get
      {
        Matrix33F result = this;
        result.Transpose();
        return result;
      }
    }


    /// <summary>
    /// Returns the inverse of this matrix.
    /// </summary>
    /// <value>The inverse of this matrix.</value>
    /// <remarks>
    /// The property does not change this instance. To invert this instance you need to call 
    /// <see cref="Invert"/>.
    /// </remarks>
    /// <exception cref="MathematicsException">
    /// The matrix is singular (i.e. it is not invertible).
    /// </exception>
    /// <seealso cref="Invert"/>
    /// <seealso cref="TryInvert"/>
    public Matrix33F Inverse
    {
      get
      {
        Matrix33F result = this;
        result.Invert();
        return result;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    
    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix33F"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix33F"/> struct.
    /// </summary>
    /// <param name="elementValue">The initial value for the matrix elements.</param>
    /// <remarks>
    /// All matrix elements are set to <paramref name="elementValue"/>.
    /// </remarks>
    public Matrix33F(float elementValue)
      : this(elementValue, elementValue, elementValue,
             elementValue, elementValue, elementValue,
             elementValue, elementValue, elementValue)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix33F"/> class.
    /// </summary>
    /// <param name="m00">The element in the first row, first column.</param>
    /// <param name="m01">The element in the first row, second column.</param>
    /// <param name="m02">The element in the first row, third column.</param>
    /// <param name="m10">The element in the second row, first column.</param>
    /// <param name="m11">The element in the second row, second column.</param>
    /// <param name="m12">The element in the second row, third column.</param>
    /// <param name="m20">The element in the third row, first column.</param>
    /// <param name="m21">The element in the third row, second column.</param>
    /// <param name="m22">The element in the third row, third column.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Matrix33F(float m00, float m01, float m02,
                    float m10, float m11, float m12,
                    float m20, float m21, float m22)
    {
      M00 = m00; M01 = m01; M02 = m02;
      M10 = m10; M11 = m11; M12 = m12;
      M20 = m20; M21 = m21; M22 = m22;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix33F"/> struct.
    /// </summary>
    /// <param name="elements">The array with the initial values for the matrix elements.</param>
    /// <param name="order">The order of the matrix elements in <paramref name="elements"/>.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="elements"/> has less than 9 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="elements"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public Matrix33F(float[] elements, MatrixOrder order)
    {
      if (order == MatrixOrder.RowMajor)
      {
        // First row
        M00 = elements[0]; M01 = elements[1]; M02 = elements[2];
        // Second row
        M10 = elements[3]; M11 = elements[4]; M12 = elements[5];
        // Third row
        M20 = elements[6]; M21 = elements[7]; M22 = elements[8];
      }
      else
      {
        // First column
        M00 = elements[0]; M10 = elements[1]; M20 = elements[2];
        // Second column
        M01 = elements[3]; M11 = elements[4]; M21 = elements[5];
        // Third column
        M02 = elements[6]; M12 = elements[7]; M22 = elements[8];
      }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix33F"/> struct.
    /// </summary>
    /// <param name="elements">The list with the initial values for the matrix elements.</param>
    /// <param name="order">The order of the matrix elements in <paramref name="elements"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="elements"/> has less than 9 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="elements"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public Matrix33F(IList<float> elements, MatrixOrder order)
    {
      if (order == MatrixOrder.RowMajor)
      {
        // First row
        M00 = elements[0]; M01 = elements[1]; M02 = elements[2];
        // Second row
        M10 = elements[3]; M11 = elements[4]; M12 = elements[5];
        // Third row
        M20 = elements[6]; M21 = elements[7]; M22 = elements[8];
      }
      else
      {
        // First column
        M00 = elements[0]; M10 = elements[1]; M20 = elements[2];
        // Second column
        M01 = elements[3]; M11 = elements[4]; M21 = elements[5];
        // Third column
        M02 = elements[6]; M12 = elements[7]; M22 = elements[8];
      }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix33F"/> struct.
    /// </summary>
    /// <param name="elements">The array with the initial values for the matrix elements.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="elements"/> has less than 3x3 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="elements"/> or the arrays in elements[0] must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [CLSCompliant(false)]
    public Matrix33F(float[,] elements)
    {
      M00 = elements[0, 0]; M01 = elements[0, 1]; M02 = elements[0, 2];
      M10 = elements[1, 0]; M11 = elements[1, 1]; M12 = elements[1, 2];
      M20 = elements[2, 0]; M21 = elements[2, 1]; M22 = elements[2, 2];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix33F"/> struct.
    /// </summary>
    /// <param name="elements">The array with the initial values for the matrix elements.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="elements"/> has less than 3x3 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="elements"/> or the arrays in elements[0] must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public Matrix33F(float[][] elements)
    {
      M00 = elements[0][0]; M01 = elements[0][1]; M02 = elements[0][2];
      M10 = elements[1][0]; M11 = elements[1][1]; M12 = elements[1][2];
      M20 = elements[2][0]; M21 = elements[2][1]; M22 = elements[2][2];
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
        int hashCode = M00.GetHashCode();
        hashCode = (hashCode * 397) ^ M01.GetHashCode();
        hashCode = (hashCode * 397) ^ M02.GetHashCode();
        hashCode = (hashCode * 397) ^ M10.GetHashCode();
        hashCode = (hashCode * 397) ^ M11.GetHashCode();
        hashCode = (hashCode * 397) ^ M12.GetHashCode();
        hashCode = (hashCode * 397) ^ M20.GetHashCode();
        hashCode = (hashCode * 397) ^ M21.GetHashCode();
        hashCode = (hashCode * 397) ^ M22.GetHashCode();
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <overloads>
    /// <summary>
    /// Indicates whether the current object is equal to another object.
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
      return obj is Matrix33F && this == (Matrix33F)obj;
    }


    #region IEquatable<Matrix33F> Members
    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(Matrix33F other)
    {
      return this == other;
    }
    #endregion


    /// <overloads>
    /// <summary>
    /// Returns the string representation of this matrix.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Returns the string representation of this matrix.
    /// </summary>
    /// <returns>The string representation of this matrix.</returns>
    public override string ToString()
    {
      return ToString(CultureInfo.CurrentCulture);
    }


    /// <summary>
    /// Returns the string representation of this matrix using the specified culture-specific format
    /// information.
    /// </summary>
    /// <param name="provider">
    /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
    /// </param>
    /// <returns>The string representation of this matrix.</returns>
    public string ToString(IFormatProvider provider)
    {
      return string.Format(provider,
        "({0}; {1}; {2})\n({3}; {4}; {5})\n({6}; {7}; {8})\n",
        M00, M01, M02, M10, M11, M12, M20, M21, M22);
    }
    #endregion


    //--------------------------------------------------------------
    #region Overloaded Operators
    //--------------------------------------------------------------

    /// <summary>
    /// Negates a matrix.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The negated matrix.</returns>
    /// <remarks>
    /// Each element of the matrix is negated.
    /// </remarks>
    public static Matrix33F operator -(Matrix33F matrix)
    {
      matrix.M00 = -matrix.M00; matrix.M01 = -matrix.M01; matrix.M02 = -matrix.M02;
      matrix.M10 = -matrix.M10; matrix.M11 = -matrix.M11; matrix.M12 = -matrix.M12;
      matrix.M20 = -matrix.M20; matrix.M21 = -matrix.M21; matrix.M22 = -matrix.M22;
      return matrix;
    }


    /// <summary>
    /// Negates a matrix.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The negated matrix.</returns>
    /// <remarks>
    /// Each element of the matrix is negated.
    /// </remarks>
    public static Matrix33F Negate(Matrix33F matrix)
    {
      matrix.M00 = -matrix.M00; matrix.M01 = -matrix.M01; matrix.M02 = -matrix.M02;
      matrix.M10 = -matrix.M10; matrix.M11 = -matrix.M11; matrix.M12 = -matrix.M12;
      matrix.M20 = -matrix.M20; matrix.M21 = -matrix.M21; matrix.M22 = -matrix.M22;
      return matrix;
    }


    /// <summary>
    /// Adds two matrices.
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second Matrix.</param>
    /// <returns>The sum of the two matrices.</returns>
    public static Matrix33F operator +(Matrix33F matrix1, Matrix33F matrix2)
    {
      matrix1.M00 += matrix2.M00; matrix1.M01 += matrix2.M01; matrix1.M02 += matrix2.M02;
      matrix1.M10 += matrix2.M10; matrix1.M11 += matrix2.M11; matrix1.M12 += matrix2.M12;
      matrix1.M20 += matrix2.M20; matrix1.M21 += matrix2.M21; matrix1.M22 += matrix2.M22;
      return matrix1;
    }


    /// <summary>
    /// Adds two matrices.
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second Matrix.</param>
    /// <returns>The sum of the two matrices.</returns>
    public static Matrix33F Add(Matrix33F matrix1, Matrix33F matrix2)
    {
      matrix1.M00 += matrix2.M00; matrix1.M01 += matrix2.M01; matrix1.M02 += matrix2.M02;
      matrix1.M10 += matrix2.M10; matrix1.M11 += matrix2.M11; matrix1.M12 += matrix2.M12;
      matrix1.M20 += matrix2.M20; matrix1.M21 += matrix2.M21; matrix1.M22 += matrix2.M22;
      return matrix1;
    }


    /// <summary>
    /// Subtracts two matrices.
    /// </summary>
    /// <param name="minuend">The first matrix (minuend).</param>
    /// <param name="subtrahend">The second matrix (subtrahend).</param>
    /// <returns>The difference of the two matrices.</returns>
    public static Matrix33F operator -(Matrix33F minuend, Matrix33F subtrahend)
    {
      minuend.M00 -= subtrahend.M00;
      minuend.M01 -= subtrahend.M01;
      minuend.M02 -= subtrahend.M02;
      minuend.M10 -= subtrahend.M10;
      minuend.M11 -= subtrahend.M11;
      minuend.M12 -= subtrahend.M12;
      minuend.M20 -= subtrahend.M20;
      minuend.M21 -= subtrahend.M21;
      minuend.M22 -= subtrahend.M22;
      return minuend;
    }


    /// <summary>
    /// Subtracts two matrices.
    /// </summary>
    /// <param name="minuend">The first matrix (minuend).</param>
    /// <param name="subtrahend">The second matrix (subtrahend).</param>
    /// <returns>The difference of the two matrices.</returns>
    public static Matrix33F Subtract(Matrix33F minuend, Matrix33F subtrahend)
    {
      minuend.M00 -= subtrahend.M00;
      minuend.M01 -= subtrahend.M01;
      minuend.M02 -= subtrahend.M02;
      minuend.M10 -= subtrahend.M10;
      minuend.M11 -= subtrahend.M11;
      minuend.M12 -= subtrahend.M12;
      minuend.M20 -= subtrahend.M20;
      minuend.M21 -= subtrahend.M21;
      minuend.M22 -= subtrahend.M22;
      return minuend;
    }


    /// <overloads>
    /// <summary>
    /// Multiplies a matrix by a scalar, matrix or vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Multiplies a matrix and a scalar.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The matrix with each element multiplied by <paramref name="scalar"/>.</returns>
    public static Matrix33F operator *(Matrix33F matrix, float scalar)
    {
      matrix.M00 *= scalar; matrix.M01 *= scalar; matrix.M02 *= scalar;
      matrix.M10 *= scalar; matrix.M11 *= scalar; matrix.M12 *= scalar;
      matrix.M20 *= scalar; matrix.M21 *= scalar; matrix.M22 *= scalar;
      return matrix;
    }


    /// <summary>
    /// Multiplies a matrix by a scalar.
    /// </summary>
    /// <param name="scalar">The scalar.</param>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The matrix with each element multiplied by <paramref name="scalar"/>.</returns>
    public static Matrix33F operator *(float scalar, Matrix33F matrix)
    {
      matrix.M00 *= scalar; matrix.M01 *= scalar; matrix.M02 *= scalar;
      matrix.M10 *= scalar; matrix.M11 *= scalar; matrix.M12 *= scalar;
      matrix.M20 *= scalar; matrix.M21 *= scalar; matrix.M22 *= scalar;
      return matrix;
    }


    /// <overloads>
    /// <summary>
    /// Multiplies a matrix by a scalar, matrix or vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Multiplies a matrix by a scalar.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The matrix with each element multiplied by <paramref name="scalar"/>.</returns>
    public static Matrix33F Multiply(float scalar, Matrix33F matrix)
    {
      matrix.M00 *= scalar; matrix.M01 *= scalar; matrix.M02 *= scalar;
      matrix.M10 *= scalar; matrix.M11 *= scalar; matrix.M12 *= scalar;
      matrix.M20 *= scalar; matrix.M21 *= scalar; matrix.M22 *= scalar;
      return matrix;
    }


    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second matrix.</param>
    /// <returns>The matrix with the product the two matrices.</returns>
    public static Matrix33F operator *(Matrix33F matrix1, Matrix33F matrix2)
    {
      Matrix33F product;
      product.M00 = matrix1.M00 * matrix2.M00 + matrix1.M01 * matrix2.M10 + matrix1.M02 * matrix2.M20;
      product.M01 = matrix1.M00 * matrix2.M01 + matrix1.M01 * matrix2.M11 + matrix1.M02 * matrix2.M21;
      product.M02 = matrix1.M00 * matrix2.M02 + matrix1.M01 * matrix2.M12 + matrix1.M02 * matrix2.M22;
      product.M10 = matrix1.M10 * matrix2.M00 + matrix1.M11 * matrix2.M10 + matrix1.M12 * matrix2.M20;
      product.M11 = matrix1.M10 * matrix2.M01 + matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21;
      product.M12 = matrix1.M10 * matrix2.M02 + matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22;
      product.M20 = matrix1.M20 * matrix2.M00 + matrix1.M21 * matrix2.M10 + matrix1.M22 * matrix2.M20;
      product.M21 = matrix1.M20 * matrix2.M01 + matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21;
      product.M22 = matrix1.M20 * matrix2.M02 + matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22;
      return product;
    }


    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second matrix.</param>
    /// <returns>The matrix with the product the two matrices.</returns>
    public static Matrix33F Multiply(Matrix33F matrix1, Matrix33F matrix2)
    {
      Matrix33F product;
      product.M00 = matrix1.M00 * matrix2.M00 + matrix1.M01 * matrix2.M10 + matrix1.M02 * matrix2.M20;
      product.M01 = matrix1.M00 * matrix2.M01 + matrix1.M01 * matrix2.M11 + matrix1.M02 * matrix2.M21;
      product.M02 = matrix1.M00 * matrix2.M02 + matrix1.M01 * matrix2.M12 + matrix1.M02 * matrix2.M22;
      product.M10 = matrix1.M10 * matrix2.M00 + matrix1.M11 * matrix2.M10 + matrix1.M12 * matrix2.M20;
      product.M11 = matrix1.M10 * matrix2.M01 + matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21;
      product.M12 = matrix1.M10 * matrix2.M02 + matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22;
      product.M20 = matrix1.M20 * matrix2.M00 + matrix1.M21 * matrix2.M10 + matrix1.M22 * matrix2.M20;
      product.M21 = matrix1.M20 * matrix2.M01 + matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21;
      product.M22 = matrix1.M20 * matrix2.M02 + matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22;
      return product;
    }


    /// <summary>
    /// Multiplies a matrix with a column vector.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="vector">The column vector.</param>
    /// <returns>The resulting column vector.</returns>
    public static Vector3F operator *(Matrix33F matrix, Vector3F vector)
    {
      Vector3F result;
      result.X = matrix.M00 * vector.X + matrix.M01 * vector.Y + matrix.M02 * vector.Z;
      result.Y = matrix.M10 * vector.X + matrix.M11 * vector.Y + matrix.M12 * vector.Z;
      result.Z = matrix.M20 * vector.X + matrix.M21 * vector.Y + matrix.M22 * vector.Z;
      return result;
    }


    /// <summary>
    /// Multiplies a matrix with a column vector.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="vector">The column vector.</param>
    /// <returns>The resulting column vector.</returns>
    public static Vector3F Multiply(Matrix33F matrix, Vector3F vector)
    {
      Vector3F result;
      result.X = matrix.M00 * vector.X + matrix.M01 * vector.Y + matrix.M02 * vector.Z;
      result.Y = matrix.M10 * vector.X + matrix.M11 * vector.Y + matrix.M12 * vector.Z;
      result.Z = matrix.M20 * vector.X + matrix.M21 * vector.Y + matrix.M22 * vector.Z;
      return result;
    }


    /// <summary>
    /// Multiplies the transposed of the given matrix with a column vector.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="vector">The column vector.</param>
    /// <returns>The resulting column vector.</returns>
    /// <remarks>
    /// This method transposes the given matrix and multiplies the transposed matrix with the given
    /// vector.
    /// </remarks>
    public static Vector3F MultiplyTransposed(Matrix33F matrix, Vector3F vector)
    {
      Vector3F result;
      result.X = matrix.M00 * vector.X + matrix.M10 * vector.Y + matrix.M20 * vector.Z;
      result.Y = matrix.M01 * vector.X + matrix.M11 * vector.Y + matrix.M21 * vector.Z;
      result.Z = matrix.M02 * vector.X + matrix.M12 * vector.Y + matrix.M22 * vector.Z;
      return result;
    }


    /// <summary>
    /// Divides a matrix by a scalar.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The matrix with each element divided by scalar.</returns>
    public static Matrix33F operator /(Matrix33F matrix, float scalar)
    {
      float f = 1 / scalar;
      matrix.M00 *= f; matrix.M01 *= f; matrix.M02 *= f;
      matrix.M10 *= f; matrix.M11 *= f; matrix.M12 *= f;
      matrix.M20 *= f; matrix.M21 *= f; matrix.M22 *= f;
      return matrix;
    }


    /// <summary>
    /// Divides a matrix by a scalar.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The matrix with each element divided by scalar.</returns>
    public static Matrix33F Divide(Matrix33F matrix, float scalar)
    {
      float f = 1 / scalar;
      matrix.M00 *= f; matrix.M01 *= f; matrix.M02 *= f;
      matrix.M10 *= f; matrix.M11 *= f; matrix.M12 *= f;
      matrix.M20 *= f; matrix.M21 *= f; matrix.M22 *= f;
      return matrix;
    }


    /// <summary>
    /// Tests if two matrices are equal.
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second matrix.</param>
    /// <returns>
    /// <see langword="true"/> if the matrices are equal; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// For the test the corresponding elements of the matrices are compared.
    /// </remarks>
    public static bool operator ==(Matrix33F matrix1, Matrix33F matrix2)
    {
      return (matrix1.M00 == matrix2.M00) && (matrix1.M01 == matrix2.M01) && (matrix1.M02 == matrix2.M02)
          && (matrix1.M10 == matrix2.M10) && (matrix1.M11 == matrix2.M11) && (matrix1.M12 == matrix2.M12)
          && (matrix1.M20 == matrix2.M20) && (matrix1.M21 == matrix2.M21) && (matrix1.M22 == matrix2.M22);
    }


    /// <summary>
    /// Tests if two matrices are not equal.
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second matrix.</param>
    /// <returns>
    /// <see langword="true"/> if the matrices are different; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// For the test the corresponding elements of the matrices are compared.
    /// </remarks>
    public static bool operator !=(Matrix33F matrix1, Matrix33F matrix2)
    {
      return (matrix1.M00 != matrix2.M00) || (matrix1.M01 != matrix2.M01) || (matrix1.M02 != matrix2.M02)
          || (matrix1.M10 != matrix2.M10) || (matrix1.M11 != matrix2.M11) || (matrix1.M12 != matrix2.M12)
          || (matrix1.M20 != matrix2.M20) || (matrix1.M21 != matrix2.M21) || (matrix1.M22 != matrix2.M22);
    }


    /// <overloads>
    /// <summary>
    /// Performs an explicit conversion from <see cref="Matrix22F"/> to another type.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Performs an explicit conversion from <see cref="Matrix33F"/> to a 2-dimensional 
    /// <see langword="float"/> array.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The result of the conversion.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    public static explicit operator float[,](Matrix33F matrix)
    {
      float[,] result = new float[3, 3];

      result[0, 0] = matrix.M00; result[0, 1] = matrix.M01; result[0, 2] = matrix.M02;
      result[1, 0] = matrix.M10; result[1, 1] = matrix.M11; result[1, 2] = matrix.M12;
      result[2, 0] = matrix.M20; result[2, 1] = matrix.M21; result[2, 2] = matrix.M22;

      return result;
    }


    /// <summary>
    /// Converts this <see cref="Matrix33F"/> to a 2-dimensional <see langword="float"/> array.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    public float[,] ToArray2D()
    {
      return (float[,]) this;
    }


    /// <summary>
    /// Performs an explicit conversion from <see cref="Matrix33F"/> to a jagged 
    /// <see langword="float"/> array.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator float[][](Matrix33F matrix)
    {
      float[][] result = new float[3][];
      result[0] = new float[3]; result[1] = new float[3]; result[2] = new float[3];

      result[0][0] = matrix.M00; result[0][1] = matrix.M01; result[0][2] = matrix.M02;
      result[1][0] = matrix.M10; result[1][1] = matrix.M11; result[1][2] = matrix.M12;
      result[2][0] = matrix.M20; result[2][1] = matrix.M21; result[2][2] = matrix.M22;

      return result;
    }


    /// <summary>
    /// Converts this <see cref="Matrix33F"/> to a jagged <see langword="float"/> array.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public float[][] ToArrayJagged()
    {
      return (float[][]) this;
    }


    /// <overloads>
    /// <summary>
    /// Performs an implicit conversion from <see cref="Matrix33F"/> to another data type.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Performs an implicit conversion from <see cref="Matrix33F"/> to <see cref="MatrixF"/>.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator MatrixF(Matrix33F matrix)
    {
      MatrixF result = new MatrixF(3, 3);
      result[0, 0] = matrix.M00; result[0, 1] = matrix.M01; result[0, 2] = matrix.M02;
      result[1, 0] = matrix.M10; result[1, 1] = matrix.M11; result[1, 2] = matrix.M12;
      result[2, 0] = matrix.M20; result[2, 1] = matrix.M21; result[2, 2] = matrix.M22;
      return result;
    }


    /// <summary>
    /// Converts this <see cref="Matrix33F"/> to <see cref="MatrixF"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public MatrixF ToMatrixF()
    {
      return this;
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="Matrix33F"/> to <see cref="Matrix33D"/>.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Matrix33D(Matrix33F matrix)
    {
      return new Matrix33D(matrix.M00, matrix.M01, matrix.M02,
                           matrix.M10, matrix.M11, matrix.M12,
                           matrix.M20, matrix.M21, matrix.M22);
    }


    /// <summary>
    /// Converts this <see cref="Matrix33F"/> to <see cref="Matrix33D"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public Matrix33D ToMatrix33D()
    {
      return this;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Sets each matrix element to its absolute value.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets each matrix element to its absolute value.
    /// </summary>
    public void Absolute()
    {
      M00 = Math.Abs(M00); M01 = Math.Abs(M01); M02 = Math.Abs(M02);
      M10 = Math.Abs(M10); M11 = Math.Abs(M11); M12 = Math.Abs(M12);
      M20 = Math.Abs(M20); M21 = Math.Abs(M21); M22 = Math.Abs(M22);
    }


    /// <overloads>
    /// <summary>
    /// Clamps near-zero matrix elements to zero.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Clamps near-zero matrix elements to zero.
    /// </summary>
    /// <remarks>
    /// Each matrix element is compared to zero. If the element is in the interval 
    /// [-<see cref="Numeric.EpsilonF"/>, +<see cref="Numeric.EpsilonF"/>] it is set to zero, 
    /// otherwise it remains unchanged.
    /// </remarks>
    public void ClampToZero()
    {
      M00 = Numeric.ClampToZero(M00); M01 = Numeric.ClampToZero(M01);
      M02 = Numeric.ClampToZero(M02);

      M10 = Numeric.ClampToZero(M10); M11 = Numeric.ClampToZero(M11);
      M12 = Numeric.ClampToZero(M12);

      M20 = Numeric.ClampToZero(M20); M21 = Numeric.ClampToZero(M21);
      M22 = Numeric.ClampToZero(M22);
    }


    /// <summary>
    /// Clamps near-zero matrix elements to zero.
    /// </summary>
    /// <param name="epsilon">The tolerance value.</param>
    /// <remarks>
    /// Each matrix element is compared to zero. If the element is in the interval 
    /// [-<paramref name="epsilon"/>, +<paramref name="epsilon"/>] it is set to zero, otherwise it 
    /// remains unchanged.
    /// </remarks>
    public void ClampToZero(float epsilon)
    {
      M00 = Numeric.ClampToZero(M00, epsilon); M01 = Numeric.ClampToZero(M01, epsilon);
      M02 = Numeric.ClampToZero(M02, epsilon);

      M10 = Numeric.ClampToZero(M10, epsilon); M11 = Numeric.ClampToZero(M11, epsilon);
      M12 = Numeric.ClampToZero(M12, epsilon);

      M20 = Numeric.ClampToZero(M20, epsilon); M21 = Numeric.ClampToZero(M21, epsilon);
      M22 = Numeric.ClampToZero(M22, epsilon);
    }


    /// <summary>
    /// Gets a column as <see cref="Vector3F"/>.
    /// </summary>
    /// <param name="index">The index of the column (0, 1, or 2).</param>
    /// <returns>The column vector.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public Vector3F GetColumn(int index)
    {
      Vector3F column;
      switch (index)
      {
        case 0:
          column.X = M00;
          column.Y = M10;
          column.Z = M20;
          break;
        case 1:
          column.X = M01;
          column.Y = M11;
          column.Z = M21;
          break;
        case 2:
          column.X = M02;
          column.Y = M12;
          column.Z = M22;
          break;
        default:
          throw new ArgumentOutOfRangeException("index", "The column index is out of range. Allowed values are 0, 1, or 2.");
      }
      return column;
    }


    /// <summary>
    /// Sets a column from a <see cref="Vector3F"/>.
    /// </summary>
    /// <param name="index">The index of the column (0, 1, or 2).</param>
    /// <param name="columnVector">The column vector.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public void SetColumn(int index, Vector3F columnVector)
    {
      switch (index)
      {
        case 0:
          M00 = columnVector.X;
          M10 = columnVector.Y;
          M20 = columnVector.Z;
          break;
        case 1:
          M01 = columnVector.X;
          M11 = columnVector.Y;
          M21 = columnVector.Z;
          break;
        case 2:
          M02 = columnVector.X;
          M12 = columnVector.Y;
          M22 = columnVector.Z;
          break;
        default:
          throw new ArgumentOutOfRangeException("index", "The column index is out of range. Allowed values are 0, 1, or 2.");
      }
    }


    /// <summary>
    /// Gets a row as <see cref="Vector3F"/>.
    /// </summary>
    /// <param name="index">The index of the row (0, 1, or 2).</param>
    /// <returns>The row vector.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public Vector3F GetRow(int index)
    {
      Vector3F row;
      switch (index)
      {
        case 0:
          row.X = M00;
          row.Y = M01;
          row.Z = M02;
          break;
        case 1:
          row.X = M10;
          row.Y = M11;
          row.Z = M12;
          break;
        case 2:
          row.X = M20;
          row.Y = M21;
          row.Z = M22;
          break;
        default:
          throw new ArgumentOutOfRangeException("index", "The row index is out of range. Allowed values are 0, 1, or 2.");
      }
      return row;
    }


    /// <summary>
    /// Sets a row from a <see cref="Vector3F"/>.
    /// </summary>
    /// <param name="index">The index of the row (0, 1, or 2).</param>
    /// <param name="rowVector">The row vector.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public void SetRow(int index, Vector3F rowVector)
    {
      switch (index)
      {
        case 0:
          M00 = rowVector.X;
          M01 = rowVector.Y;
          M02 = rowVector.Z;
          break;
        case 1:
          M10 = rowVector.X;
          M11 = rowVector.Y;
          M12 = rowVector.Z;
          break;
        case 2:
          M20 = rowVector.X;
          M21 = rowVector.Y;
          M22 = rowVector.Z;
          break;
        default:
          throw new ArgumentOutOfRangeException("index", "The row index is out of range. Allowed values are 0, 1, or 2.");
      }
    }


    /// <summary>
    /// Inverts the matrix.
    /// </summary>
    /// <exception cref="MathematicsException">
    /// The matrix is singular (i.e. it is not invertible).
    /// </exception>
    /// <seealso cref="Inverse"/>
    /// <seealso cref="TryInvert"/>
    public void Invert()
    {
      if (TryInvert() == false)
        throw new MathematicsException("Matrix is singular (i.e. it is not invertible).");
    }


    /// <summary>
    /// Re-orthogonalizes this instance.
    /// </summary>
    /// <remarks>
    /// Use this method to re-orthogonalize a former orthogonal matrix. Rotation matrices are 
    /// orthogonal matrices and because of numerical errors they start to get non-orthogonal after
    /// several computations. Calling this method regularly will keep the matrices orthogonal.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void Orthogonalize()
    {
      // Using modified/stabilized Gram-Schmidt Orthogonalization, see
      // http://en.wikipedia.org/wiki/Gram%E2%80%93Schmidt_process and
      // http://fgiesen.wordpress.com/2013/06/02/modified-gram-schmidt-orthogonalization/.

      var column0 = new Vector3F(M00, M10, M20);
      var column1 = new Vector3F(M01, M11, M21);
      var column2 = new Vector3F(M02, M12, M22);

      column1 = column1 - Vector3F.ProjectTo(column1, column0);
      column2 = column2 - Vector3F.ProjectTo(column2, column0) - Vector3F.ProjectTo(column2, column1);

      column0.TryNormalize();
      column1.TryNormalize();
      column2.TryNormalize();

      M00 = column0.X; M01 = column1.X; M02 = column2.X;
      M10 = column0.Y; M11 = column1.Y; M12 = column2.Y;
      M20 = column0.Z; M21 = column1.Z; M22 = column2.Z;
    }


    /// <summary>
    /// Converts this matrix to an array of <see langword="float"/> values.
    /// </summary>
    /// <param name="order">The order of the matrix elements in the array.</param>
    /// <returns>The result of the conversion.</returns>
    public float[] ToArray1D(MatrixOrder order)
    {
      float[] array = new float[9];

      if (order == MatrixOrder.ColumnMajor)
      {
        array[0] = M00; array[1] = M10; array[2] = M20;
        array[3] = M01; array[4] = M11; array[5] = M21;
        array[6] = M02; array[7] = M12; array[8] = M22;
      }
      else
      {
        array[0] = M00; array[1] = M01; array[2] = M02;
        array[3] = M10; array[4] = M11; array[5] = M12;
        array[6] = M20; array[7] = M21; array[8] = M22;
      }

      return array;
    }


    /// <summary>
    /// Converts this matrix to a list of <see langword="float"/> values.
    /// </summary>
    /// <param name="order">The order of the matrix elements in the list.</param>
    /// <returns>The result of the conversion.</returns>
    public IList<float> ToList(MatrixOrder order)
    {
      List<float> result = new List<float>(9);

      if (order == MatrixOrder.ColumnMajor)
      {
        result.Add(M00); result.Add(M10); result.Add(M20);
        result.Add(M01); result.Add(M11); result.Add(M21);
        result.Add(M02); result.Add(M12); result.Add(M22);
      }
      else
      {
        result.Add(M00); result.Add(M01); result.Add(M02);
        result.Add(M10); result.Add(M11); result.Add(M12);
        result.Add(M20); result.Add(M21); result.Add(M22);
      }

      return result;
    }


    /// <summary>
    /// Inverts the matrix if it is invertible.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the matrix is invertible; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is the equivalent to <see cref="Invert"/>, except that no exceptions are thrown.
    /// The return value indicates whether the operation was successful.
    /// </para>
    /// <para>
    /// Due to numerical errors it can happen that some singular matrices are not recognized as 
    /// singular by this method. This method is optimized for fast matrix inversion and not for safe
    /// detection of singular matrices. If you need to detect if a matrix is singular, you can, for 
    /// example, compute its <see cref="Determinant"/> and see if it is near zero.
    /// </para>
    /// </remarks>
    public bool TryInvert()
    {
      // Calculate the determinant
      float m11m22_m12m21 = M11 * M22 - M12 * M21;
      float m10m22_m12m20 = M10 * M22 - M12 * M20;
      float m10m21_m20m11 = M10 * M21 - M20 * M11;
      float determinant = M00 * m11m22_m12m21 - M01 * m10m22_m12m20 + M02 * m10m21_m20m11;

      // We check if determinant is zero using a very small epsilon, since the determinant
      // is the result of many multiplications of potentially small numbers.
      if (Numeric.IsZero(determinant, Numeric.EpsilonFSquared * Numeric.EpsilonF))
        return false;

      Matrix33F transposedAdjoint;
      transposedAdjoint.M00 = m11m22_m12m21;
      transposedAdjoint.M01 = -(M01 * M22 - M21 * M02);
      transposedAdjoint.M02 = M01 * M12 - M02 * M11;
      transposedAdjoint.M10 = -m10m22_m12m20;
      transposedAdjoint.M11 = M00 * M22 - M20 * M02;
      transposedAdjoint.M12 = -(M00 * M12 - M02 * M10);
      transposedAdjoint.M20 = m10m21_m20m11;
      transposedAdjoint.M21 = -(M00 * M21 - M01 * M20);
      transposedAdjoint.M22 = M00 * M11 - M01 * M10;

      float f = 1.0f / determinant;
      M00 = transposedAdjoint.M00 * f;
      M10 = transposedAdjoint.M10 * f;
      M20 = transposedAdjoint.M20 * f;
      M01 = transposedAdjoint.M01 * f;
      M11 = transposedAdjoint.M11 * f;
      M21 = transposedAdjoint.M21 * f;
      M02 = transposedAdjoint.M02 * f;
      M12 = transposedAdjoint.M12 * f;
      M22 = transposedAdjoint.M22 * f;
      return true;
    }


    /// <summary>
    /// Transposes this matrix.
    /// </summary>
    public void Transpose()
    {
      MathHelper.Swap(ref M01, ref M10);
      MathHelper.Swap(ref M02, ref M20);
      MathHelper.Swap(ref M12, ref M21);
    }
    #endregion


    //--------------------------------------------------------------
    #region Static Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns a matrix with the absolute values of the elements of the given matrix.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>A matrix with the absolute values of the elements of the given matrix.</returns>
    public static Matrix33F Absolute(Matrix33F matrix)
    {
      return new Matrix33F(Math.Abs(matrix.M00), Math.Abs(matrix.M01), Math.Abs(matrix.M02),
                           Math.Abs(matrix.M10), Math.Abs(matrix.M11), Math.Abs(matrix.M12),
                           Math.Abs(matrix.M20), Math.Abs(matrix.M21), Math.Abs(matrix.M22));
    }


    /// <overloads>
    /// <summary>
    /// Determines whether two matrices are equal (regarding a given tolerance).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether two matrices are equal (regarding the tolerance 
    /// <see cref="Numeric.EpsilonF"/>).
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second matrix.</param>
    /// <returns>
    /// <see langword="true"/> if the matrices are equal (within the tolerance 
    /// <see cref="Numeric.EpsilonF"/>); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The two matrices are compared component-wise. If the differences of the components are less
    /// than <see cref="Numeric.EpsilonF"/> the matrices are considered as being equal.
    /// </remarks>
    public static bool AreNumericallyEqual(Matrix33F matrix1, Matrix33F matrix2)
    {
      return Numeric.AreEqual(matrix1.M00, matrix2.M00)
          && Numeric.AreEqual(matrix1.M01, matrix2.M01)
          && Numeric.AreEqual(matrix1.M02, matrix2.M02)
          && Numeric.AreEqual(matrix1.M10, matrix2.M10)
          && Numeric.AreEqual(matrix1.M11, matrix2.M11)
          && Numeric.AreEqual(matrix1.M12, matrix2.M12)
          && Numeric.AreEqual(matrix1.M20, matrix2.M20)
          && Numeric.AreEqual(matrix1.M21, matrix2.M21)
          && Numeric.AreEqual(matrix1.M22, matrix2.M22);
    }


    /// <summary>
    /// Determines whether two matrices are equal (regarding a specific tolerance).
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second matrix.</param>
    /// <param name="epsilon">The tolerance value.</param>
    /// <returns>
    /// <see langword="true"/> if the matrices are equal (within the tolerance
    /// <paramref name="epsilon"/>); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The two matrices are compared component-wise. If the differences of the components are less
    /// than <paramref name="epsilon"/> the matrices are considered as being equal.
    /// </remarks>
    public static bool AreNumericallyEqual(Matrix33F matrix1, Matrix33F matrix2, float epsilon)
    {
      return Numeric.AreEqual(matrix1.M00, matrix2.M00, epsilon)
          && Numeric.AreEqual(matrix1.M01, matrix2.M01, epsilon)
          && Numeric.AreEqual(matrix1.M02, matrix2.M02, epsilon)
          && Numeric.AreEqual(matrix1.M10, matrix2.M10, epsilon)
          && Numeric.AreEqual(matrix1.M11, matrix2.M11, epsilon)
          && Numeric.AreEqual(matrix1.M12, matrix2.M12, epsilon)
          && Numeric.AreEqual(matrix1.M20, matrix2.M20, epsilon)
          && Numeric.AreEqual(matrix1.M21, matrix2.M21, epsilon)
          && Numeric.AreEqual(matrix1.M22, matrix2.M22, epsilon);
    }


    /// <summary>
    /// Returns a matrix with the matrix elements clamped to the range [min, max].
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>
    /// The matrix with small elements clamped to zero.
    /// </returns>
    /// <remarks>
    /// Each matrix element is compared to zero. If it is in the interval 
    /// [-<see cref="Numeric.EpsilonF"/>, +<see cref="Numeric.EpsilonF"/>] it is set to zero, 
    /// otherwise it remains unchanged.
    /// </remarks>
    public static Matrix33F ClampToZero(Matrix33F matrix)
    {
      matrix.M00 = Numeric.ClampToZero(matrix.M00);
      matrix.M01 = Numeric.ClampToZero(matrix.M01);
      matrix.M02 = Numeric.ClampToZero(matrix.M02);

      matrix.M10 = Numeric.ClampToZero(matrix.M10);
      matrix.M11 = Numeric.ClampToZero(matrix.M11);
      matrix.M12 = Numeric.ClampToZero(matrix.M12);

      matrix.M20 = Numeric.ClampToZero(matrix.M20);
      matrix.M21 = Numeric.ClampToZero(matrix.M21);
      matrix.M22 = Numeric.ClampToZero(matrix.M22);

      return matrix;
    }


    /// <summary>
    /// Returns a matrix with the matrix elements clamped to the range [min, max].
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="epsilon">The tolerance value.</param>
    /// <returns>
    /// The matrix with small elements clamped to zero.
    /// </returns>
    /// <remarks>
    /// Each matrix element is compared to zero. If it is in the interval 
    /// [-<paramref name="epsilon"/>, +<paramref name="epsilon"/>] it is set to zero, otherwise it 
    /// remains unchanged.
    /// </remarks>
    public static Matrix33F ClampToZero(Matrix33F matrix, float epsilon)
    {
      matrix.M00 = Numeric.ClampToZero(matrix.M00, epsilon);
      matrix.M01 = Numeric.ClampToZero(matrix.M01, epsilon);
      matrix.M02 = Numeric.ClampToZero(matrix.M02, epsilon);

      matrix.M10 = Numeric.ClampToZero(matrix.M10, epsilon);
      matrix.M11 = Numeric.ClampToZero(matrix.M11, epsilon);
      matrix.M12 = Numeric.ClampToZero(matrix.M12, epsilon);

      matrix.M20 = Numeric.ClampToZero(matrix.M20, epsilon);
      matrix.M21 = Numeric.ClampToZero(matrix.M21, epsilon);
      matrix.M22 = Numeric.ClampToZero(matrix.M22, epsilon);

      return matrix;
    }


    /// <overloads>
    /// <summary>
    /// Creates a scaling matrix.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates a uniform scaling matrix.
    /// </summary>
    /// <param name="scale">
    /// The uniform scale factor that is applied to the x-, y-, and z-axis.
    /// </param>
    /// <returns>The created scaling matrix.</returns>
    public static Matrix33F CreateScale(float scale)
    {
      Matrix33F result = new Matrix33F 
      {
        M00 = scale, 
        M11 = scale, 
        M22 = scale
      };
      return result;
    }


    /// <summary>
    /// Creates a scaling matrix.
    /// </summary>
    /// <param name="scaleX">The value to scale by on the x-axis.</param>
    /// <param name="scaleY">The value to scale by on the y-axis.</param>
    /// <param name="scaleZ">The value to scale by on the z-axis.</param>
    /// <returns>The created scaling matrix.</returns>
    public static Matrix33F CreateScale(float scaleX, float scaleY, float scaleZ)
    {
      Matrix33F result = new Matrix33F 
      {
        M00 = scaleX,
        M11 = scaleY,
        M22 = scaleZ
      };
      return result;
    }


    /// <summary>
    /// Creates a scaling matrix.
    /// </summary>
    /// <param name="scale">Amounts to scale by the x, y, and z-axis.</param>
    /// <returns>The created scaling matrix.</returns>
    public static Matrix33F CreateScale(Vector3F scale)
    {
      Matrix33F result = new Matrix33F 
      {
        M00 = scale.X,
        M11 = scale.Y,
        M22 = scale.Z
      };
      return result;
    }


    /// <overloads>
    /// <summary>
    /// Creates a rotation matrix.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates a rotation matrix from axis and angle.
    /// </summary>
    /// <param name="axis">The rotation axis. (Does not need to be normalized.)</param>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>The created rotation matrix.</returns>
    /// <exception cref="ArgumentException">
    /// The <paramref name="axis"/> vector has 0 length.
    /// </exception>
    public static Matrix33F CreateRotation(Vector3F axis, float angle)
    {
      if (!axis.TryNormalize())
        throw new ArgumentException("The axis vector has length 0.");

      float x = axis.X;
      float y = axis.Y;
      float z = axis.Z;
      float x2 = x * x;
      float y2 = y * y;
      float z2 = z * z;
      float xy = x * y;
      float xz = x * z;
      float yz = y * z;
      float cos = (float) Math.Cos(angle);
      float sin = (float) Math.Sin(angle);
      float xsin = x * sin;
      float ysin = y * sin;
      float zsin = z * sin;
      float oneMinusCos = 1.0f - cos;

      Matrix33F result;
      result.M00 = x2 + cos * (1.0f - x2);
      result.M01 = xy * oneMinusCos - zsin;
      result.M02 = xz * oneMinusCos + ysin;
      result.M10 = xy * oneMinusCos + zsin;
      result.M11 = y2 + cos * (1.0f - y2);
      result.M12 = yz * oneMinusCos - xsin;
      result.M20 = xz * oneMinusCos - ysin;
      result.M21 = yz * oneMinusCos + xsin;
      result.M22 = z2 + cos * (1.0f - z2);
      return result;
    }


    /// <summary>
    /// Creates a rotation matrix from a unit quaternion.
    /// </summary>
    /// <param name="rotation">The rotation described by a unit quaternion.</param>
    /// <returns>The created rotation matrix.</returns>
    public static Matrix33F CreateRotation(QuaternionF rotation)
    {
      return rotation.ToRotationMatrix33();
    }


    /// <summary>
    /// Creates a matrix that specifies a rotation around the x-axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>The created rotation matrix.</returns>
    public static Matrix33F CreateRotationX(float angle)
    {
      float cos = (float) Math.Cos(angle);
      float sin = (float) Math.Sin(angle);
      return new Matrix33F(1, 0, 0,
                           0, cos, -sin,
                           0, sin, cos);
    }


    /// <summary>
    /// Creates a matrix that specifies a rotation around the y-axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>The created rotation matrix.</returns>
    public static Matrix33F CreateRotationY(float angle)
    {
      float cos = (float) Math.Cos(angle);
      float sin = (float) Math.Sin(angle);
      return new Matrix33F(cos, 0, sin,
                           0, 1, 0,
                           -sin, 0, cos);
    }


    /// <summary>
    /// Creates a matrix that specifies a rotation around the z-axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>The created rotation matrix.</returns>
    public static Matrix33F CreateRotationZ(float angle)
    {
      float cos = (float) Math.Cos(angle);
      float sin = (float) Math.Sin(angle);
      return new Matrix33F(cos, -sin, 0,
                           sin, cos, 0,
                           0, 0, 1);
    }
    #endregion
  }
}
