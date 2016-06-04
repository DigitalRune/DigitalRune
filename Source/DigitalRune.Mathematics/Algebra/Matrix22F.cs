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
  /// Defines a 2 x 2 matrix (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// All indices are zero-based. The matrix looks like this:
  /// <code>
  /// M00 M01
  /// M10 M11
  /// </code>
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
  [TypeConverter(typeof(ExpandableObjectConverter))]
#endif
#if !XBOX && !UNITY
  [DataContract]
#endif
  public struct Matrix22F : IEquatable<Matrix22F>
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// Returns a <see cref="Matrix22F"/> with all of its components set to zero.
    /// </summary>
    public static readonly Matrix22F Zero = new Matrix22F(0, 0,
                                                          0, 0);

    /// <summary>
    /// Returns a <see cref="Matrix22F"/> with all of its components set to one.
    /// </summary>
    public static readonly Matrix22F One = new Matrix22F(1, 1,
                                                         1, 1);

    /// <summary>
    /// Returns the 2 x 2 identity matrix.
    /// </summary>
    public static readonly Matrix22F Identity = new Matrix22F(1, 0,
                                                              0, 1);
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
          case 2: return M10;
          case 3: return M11;
          default: 
            throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 to 3.");
        }
      }
      set
      {
        switch (index)
        {
          case 0: M00 = value; break;
          case 1: M01 = value; break;
          case 2: M10 = value; break;
          case 3: M11 = value; break;
          default: 
            throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 to 3.");
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
    /// The indices are zero-based: [0,0] is the first element, [1,1] is the last element.
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
              default:
                throw new ArgumentOutOfRangeException("column", "The column index is out of range. Allowed values are 0, or 1.");
            }
          case 1:
            switch (column)
            {
              case 0: return M10;
              case 1: return M11;
              default:
                throw new ArgumentOutOfRangeException("column", "The column index is out of range. Allowed values are 0, or 1.");
            }
          default:
            throw new ArgumentOutOfRangeException("row", "The row index is out of range. Allowed values are 0, or 1.");
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
              default:
                throw new ArgumentOutOfRangeException("column", "The column index is out of range. Allowed values are 0, or 1.");
            }
            break;
          case 1:
            switch (column)
            {
              case 0: M10 = value; break;
              case 1: M11 = value; break;
              default:
                throw new ArgumentOutOfRangeException("column", "The column index is out of range. Allowed values are 0, or 1.");
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
        return M00 * M11 - M01 * M10;
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
        return Numeric.IsNaN(M00) || Numeric.IsNaN(M01)
               || Numeric.IsNaN(M10) || Numeric.IsNaN(M11);
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
      get { return M01 == M10; }
    }


    /// <summary>
    /// Gets the matrix trace (the sum of the diagonal elements).
    /// </summary>
    /// <value>The matrix trace.</value>
    public float Trace
    {
      get
      {
        return M00 + M11;
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
    public Matrix22F Transposed
    {
      get
      {
        Matrix22F result = this;
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
    public Matrix22F Inverse
    {
      get
      {
        Matrix22F result = this;
        result.Invert();
        return result;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix22F"/> struct.
    /// </summary>
    /// <param name="elementValue">The initial value for the matrix elements.</param>
    /// <remarks>
    /// All matrix elements are set to <paramref name="elementValue"/>.
    /// </remarks>
    public Matrix22F(float elementValue)
      : this(elementValue, elementValue, elementValue, elementValue)
    {
    }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix22F"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix22F"/> class.
    /// </summary>
    /// <param name="m00">The element in the first row, first column.</param>
    /// <param name="m01">The element in the first row, second column.</param>
    /// <param name="m10">The element in the second row, first column.</param>
    /// <param name="m11">The element in the second row, second column.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Matrix22F(float m00, float m01,
                     float m10, float m11)
    {
      M00 = m00; M01 = m01;
      M10 = m10; M11 = m11;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix22F"/> struct.
    /// </summary>
    /// <param name="elements">The array with the initial values for the matrix elements.</param>
    /// <param name="order">The order of the matrix elements in <paramref name="elements"/>.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="elements"/> has less than 4 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="elements"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public Matrix22F(float[] elements, MatrixOrder order)
    {
      if (order == MatrixOrder.RowMajor)
      {
        // First row
        M00 = elements[0]; M01 = elements[1];
        // Second row
        M10 = elements[2]; M11 = elements[3];
      }
      else
      {
        // First column
        M00 = elements[0]; M10 = elements[1];
        // Second column
        M01 = elements[2]; M11 = elements[3];
      }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix22F"/> struct.
    /// </summary>
    /// <param name="elements">The list with the initial values for the matrix elements.</param>
    /// <param name="order">The order of the matrix elements in <paramref name="elements"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="elements"/> has less than 4 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="elements"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public Matrix22F(IList<float> elements, MatrixOrder order)
    {
      if (order == MatrixOrder.RowMajor)
      {
        // First row
        M00 = elements[0]; M01 = elements[1];
        // Second row
        M10 = elements[2]; M11 = elements[3];
      }
      else
      {
        // First column
        M00 = elements[0]; M10 = elements[1];
        // Second column
        M01 = elements[2]; M11 = elements[3];
      }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix22F"/> struct.
    /// </summary>
    /// <param name="elements">The array with the initial values for the matrix elements.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="elements"/> has less than 2x2 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="elements"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [CLSCompliant(false)]
    public Matrix22F(float[,] elements)
    {
      M00 = elements[0, 0]; M01 = elements[0, 1];
      M10 = elements[1, 0]; M11 = elements[1, 1];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix22F"/> struct.
    /// </summary>
    /// <param name="elements">The array with the initial values for the matrix elements.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="elements"/> has less than 2x2 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="elements"/> or the arrays in elements[0] must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public Matrix22F(float[][] elements)
    {
      M00 = elements[0][0]; M01 = elements[0][1];
      M10 = elements[1][0]; M11 = elements[1][1];
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
        hashCode = (hashCode * 397) ^ M10.GetHashCode();
        hashCode = (hashCode * 397) ^ M11.GetHashCode();
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
      return obj is Matrix22F && this == (Matrix22F)obj;
    }


    #region IEquatable<Matrix22F> Members
    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(Matrix22F other)
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
        "({0}; {1})\n({2}; {3})\n",
        M00, M01, M10, M11);
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
    public static Matrix22F operator -(Matrix22F matrix)
    {
      matrix.M00 = -matrix.M00; matrix.M01 = -matrix.M01;
      matrix.M10 = -matrix.M10; matrix.M11 = -matrix.M11;
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
    public static Matrix22F Negate(Matrix22F matrix)
    {
      matrix.M00 = -matrix.M00; matrix.M01 = -matrix.M01;
      matrix.M10 = -matrix.M10; matrix.M11 = -matrix.M11;
      return matrix;
    }


    /// <summary>
    /// Adds two matrices.
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second Matrix.</param>
    /// <returns>The sum of the two matrices.</returns>
    public static Matrix22F operator +(Matrix22F matrix1, Matrix22F matrix2)
    {
      matrix1.M00 += matrix2.M00; matrix1.M01 += matrix2.M01;
      matrix1.M10 += matrix2.M10; matrix1.M11 += matrix2.M11;
      return matrix1;
    }


    /// <summary>
    /// Adds two matrices.
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second Matrix.</param>
    /// <returns>The sum of the two matrices.</returns>
    public static Matrix22F Add(Matrix22F matrix1, Matrix22F matrix2)
    {
      matrix1.M00 += matrix2.M00; matrix1.M01 += matrix2.M01;
      matrix1.M10 += matrix2.M10; matrix1.M11 += matrix2.M11;
      return matrix1;
    }


    /// <summary>
    /// Subtracts two matrices.
    /// </summary>
    /// <param name="minuend">The first matrix (minuend).</param>
    /// <param name="subtrahend">The second matrix (subtrahend).</param>
    /// <returns>The difference of the two matrices.</returns>
    public static Matrix22F operator -(Matrix22F minuend, Matrix22F subtrahend)
    {
      minuend.M00 -= subtrahend.M00;
      minuend.M01 -= subtrahend.M01;
      minuend.M10 -= subtrahend.M10;
      minuend.M11 -= subtrahend.M11;
      return minuend;
    }


    /// <summary>
    /// Subtracts two matrices.
    /// </summary>
    /// <param name="minuend">The first matrix (minuend).</param>
    /// <param name="subtrahend">The second matrix (subtrahend).</param>
    /// <returns>The difference of the two matrices.</returns>
    public static Matrix22F Subtract(Matrix22F minuend, Matrix22F subtrahend)
    {
      minuend.M00 -= subtrahend.M00;
      minuend.M01 -= subtrahend.M01;
      minuend.M10 -= subtrahend.M10;
      minuend.M11 -= subtrahend.M11;
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
    public static Matrix22F operator *(Matrix22F matrix, float scalar)
    {
      matrix.M00 *= scalar; matrix.M01 *= scalar;
      matrix.M10 *= scalar; matrix.M11 *= scalar;
      return matrix;
    }


    /// <summary>
    /// Multiplies a matrix by a scalar.
    /// </summary>
    /// <param name="scalar">The scalar.</param>
    /// <param name="matrix">The matrix.</param>
    /// <returns>
    /// The matrix with each element multiplied by <paramref name="scalar"/>.
    /// </returns>
    public static Matrix22F operator *(float scalar, Matrix22F matrix)
    {
      matrix.M00 *= scalar; matrix.M01 *= scalar;
      matrix.M10 *= scalar; matrix.M11 *= scalar;
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
    public static Matrix22F Multiply(float scalar, Matrix22F matrix)
    {
      matrix.M00 *= scalar; matrix.M01 *= scalar;
      matrix.M10 *= scalar; matrix.M11 *= scalar;
      return matrix;
    }


    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second matrix.</param>
    /// <returns>The matrix with the product the two matrices.</returns>
    public static Matrix22F operator *(Matrix22F matrix1, Matrix22F matrix2)
    {
      Matrix22F product;
      product.M00 = matrix1.M00 * matrix2.M00 + matrix1.M01 * matrix2.M10;
      product.M01 = matrix1.M00 * matrix2.M01 + matrix1.M01 * matrix2.M11;
      product.M10 = matrix1.M10 * matrix2.M00 + matrix1.M11 * matrix2.M10;
      product.M11 = matrix1.M10 * matrix2.M01 + matrix1.M11 * matrix2.M11;
      return product;
    }


    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    /// <param name="matrix1">The first matrix.</param>
    /// <param name="matrix2">The second matrix.</param>
    /// <returns>The matrix with the product the two matrices.</returns>
    public static Matrix22F Multiply(Matrix22F matrix1, Matrix22F matrix2)
    {
      Matrix22F product;
      product.M00 = matrix1.M00 * matrix2.M00 + matrix1.M01 * matrix2.M10;
      product.M01 = matrix1.M00 * matrix2.M01 + matrix1.M01 * matrix2.M11;
      product.M10 = matrix1.M10 * matrix2.M00 + matrix1.M11 * matrix2.M10;
      product.M11 = matrix1.M10 * matrix2.M01 + matrix1.M11 * matrix2.M11;
      return product;
    }


    /// <summary>
    /// Multiplies a matrix with a column vector.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="vector">The column vector.</param>
    /// <returns>The resulting column vector.</returns>
    public static Vector2F operator *(Matrix22F matrix, Vector2F vector)
    {
      Vector2F result;
      result.X = matrix.M00 * vector.X + matrix.M01 * vector.Y;
      result.Y = matrix.M10 * vector.X + matrix.M11 * vector.Y;
      return result;
    }


    /// <summary>
    /// Multiplies a matrix with a column vector.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="vector">The column vector.</param>
    /// <returns>The resulting column vector.</returns>
    public static Vector2F Multiply(Matrix22F matrix, Vector2F vector)
    {
      Vector2F result;
      result.X = matrix.M00 * vector.X + matrix.M01 * vector.Y;
      result.Y = matrix.M10 * vector.X + matrix.M11 * vector.Y;
      return result;
    }


    /// <summary>
    /// Divides a matrix by a scalar.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The matrix with each element divided by scalar.</returns>
    public static Matrix22F operator /(Matrix22F matrix, float scalar)
    {
      float f = 1 / scalar;
      matrix.M00 *= f; matrix.M01 *= f;
      matrix.M10 *= f; matrix.M11 *= f;
      return matrix;
    }


    /// <summary>
    /// Divides a matrix by a scalar.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The matrix with each element divided by scalar.</returns>
    public static Matrix22F Divide(Matrix22F matrix, float scalar)
    {
      float f = 1 / scalar;
      matrix.M00 *= f; matrix.M01 *= f;
      matrix.M10 *= f; matrix.M11 *= f;
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
    public static bool operator ==(Matrix22F matrix1, Matrix22F matrix2)
    {
      return (matrix1.M00 == matrix2.M00) && (matrix1.M01 == matrix2.M01)
          && (matrix1.M10 == matrix2.M10) && (matrix1.M11 == matrix2.M11);
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
    public static bool operator !=(Matrix22F matrix1, Matrix22F matrix2)
    {
      return (matrix1.M00 != matrix2.M00) || (matrix1.M01 != matrix2.M01)
          || (matrix1.M10 != matrix2.M10) || (matrix1.M11 != matrix2.M11);
    }


    /// <overloads>
    /// <summary>
    /// Performs an explicit conversion from <see cref="Matrix22F"/> to another type.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Performs an explicit conversion from <see cref="Matrix22F"/> to a 2-dimensional 
    /// <see langword="float"/> array.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The result of the conversion.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    public static explicit operator float[,](Matrix22F matrix)
    {
      float[,] result = new float[2, 2];

      result[0, 0] = matrix.M00; result[0, 1] = matrix.M01;
      result[1, 0] = matrix.M10; result[1, 1] = matrix.M11;

      return result;
    }


    /// <summary>
    /// Converts this <see cref="Matrix22F"/> to a 2-dimensional <see langword="float"/> array.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    public float[,] ToArray2D()
    {
      return (float[,]) this;
    }


    /// <summary>
    /// Performs an explicit conversion from <see cref="Matrix22F"/> 
    /// to a jagged <see langword="float"/> array.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator float[][](Matrix22F matrix)
    {
      float[][] result = new float[2][];
      result[0] = new float[2]; result[1] = new float[2];

      result[0][0] = matrix.M00; result[0][1] = matrix.M01;
      result[1][0] = matrix.M10; result[1][1] = matrix.M11;

      return result;
    }


    /// <summary>
    /// Converts this <see cref="Matrix22F"/> to a jagged <see langword="float"/> array.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public float[][] ToArrayJagged()
    {
      return (float[][]) this;
    }


    /// <overloads>
    /// <summary>
    /// Performs an implicit conversion from <see cref="Matrix22F"/> to another data type.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Performs an implicit conversion from <see cref="Matrix22F"/> to <see cref="MatrixF"/>.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator MatrixF(Matrix22F matrix)
    {
      MatrixF result = new MatrixF(2, 2);
      result[0, 0] = matrix.M00; result[0, 1] = matrix.M01;
      result[1, 0] = matrix.M10; result[1, 1] = matrix.M11;
      return result;
    }


    /// <summary>
    /// Converts this <see cref="Matrix22F"/> to <see cref="MatrixF"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public MatrixF ToMatrixF()
    {
      return this;
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="Matrix22F"/> to <see cref="Matrix22D"/>.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Matrix22D(Matrix22F matrix)
    {
      return new Matrix22D(matrix.M00, matrix.M01,
                           matrix.M10, matrix.M11);
    }


    /// <summary>
    /// Converts this <see cref="Matrix22F"/> to <see cref="Matrix22D"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public Matrix22D ToMatrix22D()
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
      M00 = Math.Abs(M00); M01 = Math.Abs(M01);
      M10 = Math.Abs(M10); M11 = Math.Abs(M11);
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
      M10 = Numeric.ClampToZero(M10); M11 = Numeric.ClampToZero(M11);
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
      M10 = Numeric.ClampToZero(M10, epsilon); M11 = Numeric.ClampToZero(M11, epsilon);
    }


    /// <summary>
    /// Gets a column as <see cref="Vector2F"/>.
    /// </summary>
    /// <param name="index">The index of the column.</param>
    /// <returns>The column vector.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public Vector2F GetColumn(int index)
    {
      Vector2F column;
      switch (index)
      {
        case 0:
          column.X = M00;
          column.Y = M10;
          break;
        case 1:
          column.X = M01;
          column.Y = M11;
          break;
        default:
          throw new ArgumentOutOfRangeException("index", "The column index is out of range. Allowed values are 0, or 1.");
      }
      return column;
    }


    /// <summary>
    /// Sets a column from a <see cref="Vector2F"/>.
    /// </summary>
    /// <param name="index">The index of the column.</param>
    /// <param name="columnVector">The column vector.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public void SetColumn(int index, Vector2F columnVector)
    {
      switch (index)
      {
        case 0:
          M00 = columnVector.X;
          M10 = columnVector.Y;
          break;
        case 1:
          M01 = columnVector.X;
          M11 = columnVector.Y;
          break;
        default:
          throw new ArgumentOutOfRangeException("index", "The column index is out of range. Allowed values are 0, or 1.");
      }
    }


    /// <summary>
    /// Gets a row as <see cref="Vector2F"/>.
    /// </summary>
    /// <param name="index">The index of the row.</param>
    /// <returns>The row vector.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public Vector2F GetRow(int index)
    {
      Vector2F row;
      switch (index)
      {
        case 0:
          row.X = M00;
          row.Y = M01;
          break;
        case 1:
          row.X = M10;
          row.Y = M11;
          break;
        default:
          throw new ArgumentOutOfRangeException("index", "The row index is out of range. Allowed values are 0, or 1.");
      }
      return row;
    }


    /// <summary>
    /// Sets a row from a <see cref="Vector2F"/>.
    /// </summary>
    /// <param name="index">The index of the row (0, 1, or 2).</param>
    /// <param name="rowVector">The row vector.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public void SetRow(int index, Vector2F rowVector)
    {
      switch (index)
      {
        case 0:
          M00 = rowVector.X;
          M01 = rowVector.Y;
          break;
        case 1:
          M10 = rowVector.X;
          M11 = rowVector.Y;
          break;
        default:
          throw new ArgumentOutOfRangeException("index", "The row index is out of range. Allowed values are 0, or 1.");
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
    /// Converts this matrix to an array of <see langword="float"/> values.
    /// </summary>
    /// <param name="order">The order of the matrix elements in the array.</param>
    /// <returns>The result of the conversion.</returns>
    public float[] ToArray1D(MatrixOrder order)
    {
      float[] array = new float[4];

      if (order == MatrixOrder.ColumnMajor)
      {
        array[0] = M00; array[1] = M10;
        array[2] = M01; array[3] = M11;
      }
      else
      {
        array[0] = M00; array[1] = M01;
        array[2] = M10; array[3] = M11;
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
      List<float> result = new List<float>(4);

      if (order == MatrixOrder.ColumnMajor)
      {
        result.Add(M00); result.Add(M10);
        result.Add(M01); result.Add(M11);
      }
      else
      {
        result.Add(M00); result.Add(M01);
        result.Add(M10); result.Add(M11);
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
      float determinant = M00 * M11 - M01 * M10;

      // We check if determinant is zero using a very small epsilon, since the determinant
      // is the result of multiplications of potentially small numbers.
      if (Numeric.IsZero(determinant, Numeric.EpsilonFSquared))
        return false;

      float f = 1.0f / determinant;
      float m00 = M00;
      M00 = M11 * f;
      M01 = -M01 * f;
      M10 = -M10 * f;
      M11 = m00 * f;
      return true;
    }


    /// <summary>
    /// Transposes this matrix.
    /// </summary>
    public void Transpose()
    {
      MathHelper.Swap(ref M01, ref M10);
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
    public static Matrix22F Absolute(Matrix22F matrix)
    {
      return new Matrix22F(Math.Abs(matrix.M00), Math.Abs(matrix.M01),
                           Math.Abs(matrix.M10), Math.Abs(matrix.M11));
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
    public static bool AreNumericallyEqual(Matrix22F matrix1, Matrix22F matrix2)
    {
      return Numeric.AreEqual(matrix1.M00, matrix2.M00)
          && Numeric.AreEqual(matrix1.M01, matrix2.M01)
          && Numeric.AreEqual(matrix1.M10, matrix2.M10)
          && Numeric.AreEqual(matrix1.M11, matrix2.M11);
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
    public static bool AreNumericallyEqual(Matrix22F matrix1, Matrix22F matrix2, float epsilon)
    {
      return Numeric.AreEqual(matrix1.M00, matrix2.M00, epsilon)
          && Numeric.AreEqual(matrix1.M01, matrix2.M01, epsilon)
          && Numeric.AreEqual(matrix1.M10, matrix2.M10, epsilon)
          && Numeric.AreEqual(matrix1.M11, matrix2.M11, epsilon);
    }


    /// <summary>
    /// Returns a matrix with the matrix elements clamped to the range [min, max].
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The matrix with small elements clamped to zero.</returns>
    /// <remarks>
    /// Each matrix element is compared to zero. If it is in the interval 
    /// [-<see cref="Numeric.EpsilonF"/>, +<see cref="Numeric.EpsilonF"/>] it is set to zero, 
    /// otherwise it remains unchanged.
    /// </remarks>
    public static Matrix22F ClampToZero(Matrix22F matrix)
    {
      matrix.M00 = Numeric.ClampToZero(matrix.M00);
      matrix.M01 = Numeric.ClampToZero(matrix.M01);

      matrix.M10 = Numeric.ClampToZero(matrix.M10);
      matrix.M11 = Numeric.ClampToZero(matrix.M11);

      return matrix;
    }


    /// <summary>
    /// Returns a matrix with the matrix elements clamped to the range [min, max].
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="epsilon">The tolerance value.</param>
    /// <returns>The matrix with small elements clamped to zero.</returns>
    /// <remarks>
    /// Each matrix element is compared to zero. If it is in the interval 
    /// [-<paramref name="epsilon"/>, +<paramref name="epsilon"/>] it is set to zero, otherwise it 
    /// remains unchanged.
    /// </remarks>
    public static Matrix22F ClampToZero(Matrix22F matrix, float epsilon)
    {
      matrix.M00 = Numeric.ClampToZero(matrix.M00, epsilon);
      matrix.M01 = Numeric.ClampToZero(matrix.M01, epsilon);

      matrix.M10 = Numeric.ClampToZero(matrix.M10, epsilon);
      matrix.M11 = Numeric.ClampToZero(matrix.M11, epsilon);

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
    /// <param name="scale">The uniform scale factor that is applied to the x- and y-axis.</param>
    /// <returns>The created scaling matrix.</returns>
    public static Matrix22F CreateScale(float scale)
    {
      Matrix22F result = new Matrix22F 
      {
        M00 = scale, 
        M11 = scale
      };
      return result;
    }


    /// <summary>
    /// Creates a scaling matrix.
    /// </summary>
    /// <param name="scaleX">The value to scale by on the x-axis.</param>
    /// <param name="scaleY">The value to scale by on the y-axis.</param>
    /// <returns>The created scaling matrix.</returns>
    public static Matrix22F CreateScale(float scaleX, float scaleY)
    {
      Matrix22F result = new Matrix22F 
      {
        M00 = scaleX,
        M11 = scaleY
      };
      return result;
    }


    /// <summary>
    /// Creates a scaling matrix.
    /// </summary>
    /// <param name="scale">Amounts to scale by the x, and y-axis.</param>
    /// <returns>The created scaling matrix.</returns>
    public static Matrix22F CreateScale(Vector2F scale)
    {
      Matrix22F result = new Matrix22F 
      {
        M00 = scale.X,
        M11 = scale.Y
      };
      return result;
    }


    /// <summary>
    /// Creates a rotation matrix.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>The created rotation matrix.</returns>
    public static Matrix22F CreateRotation(float angle)
    {
      float cos = (float)Math.Cos(angle);
      float sin = (float)Math.Sin(angle);

      Matrix22F result;
      result.M00 = cos;
      result.M01 = -sin;
      result.M10 = sin;
      result.M11 = cos;
      return result;
    }
    #endregion
  }
}
