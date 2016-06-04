// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
#endif
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Mathematics.Algebra
{
  /// <summary>
  /// Defines an n-dimensional vector (single-precision).
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
  [TypeConverter(typeof(ExpandableObjectConverter))]
#endif
  public class VectorF 
    : IEquatable<VectorF>, 
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
      ISerializable,
#endif
      IXmlSerializable
  {
    // TODO: Remove ArgumentNullException and let runtime throw NullReferenceException. (Minor optimization)

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private float[] _v;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the internal array that is used to store the vector values.
    /// </summary>
    /// <value>
    /// The internal array that is used to store the vector values; must not be 
    /// <see langword="null"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public float[] InternalArray
    {
      get { return _v; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _v = value;
      }
    }

    /// <summary>
    /// Gets or sets the component at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <value>The component at <paramref name="index"/>.</value>
    /// <remarks>
    /// The index is zero based. 
    /// </remarks>
    /// <exception cref="IndexOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public float this[int index]
    {
      get { return _v[index]; }
      set { _v[index] = value; }
    }


    /// <summary>
    /// Gets a value indicating whether a component of the vector is <see cref="float.NaN"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a component of the vector is <see cref="float.NaN"/>; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsNaN
    {
      get
      {
        for (int i = 0; i < _v.Length; i++)
          if (Numeric.IsNaN(_v[i]))
            return true;

        return false;
      }
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
    /// the default tolerance value (see <see cref="Numeric.EpsilonF"/>).
    /// </remarks>
    public bool IsNumericallyNormalized
    {
      get { return Numeric.AreEqual(LengthSquared, 1.0f); }
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
    /// <see cref="Numeric.EpsilonF"/>).
    /// </remarks>
    public bool IsNumericallyZero
    {
      get { return Numeric.IsZero(LengthSquared, Numeric.EpsilonFSquared); }
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
    public float Length
    {
      get
      {
        return (float) Math.Sqrt(LengthSquared);
      }
      set
      {
        float length = Length;
        if (Numeric.IsZero(length))
          throw new MathematicsException("Cannot change length of a vector with length 0.");

        float scale = value / length;
        for (int i = 0; i < NumberOfElements; i++)
          _v[i] = _v[i] * scale;
      }
    }


    /// <summary>
    /// Returns the squared length of this vector.
    /// </summary>
    /// <returns>The squared length of this vector.</returns>
    public float LengthSquared
    {
      get { return Dot(this, this); }
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
    public VectorF Normalized
    {
      get
      {
        VectorF v = Clone();
        v.Normalize();
        return v;
      }
    }


    /// <summary>
    /// Gets the number of elements <i>n</i>.
    /// </summary>
    /// <value>The number of elements <i>n</i>.</value>
    public int NumberOfElements
    {
      get { return _v.Length; }
    }


    /// <summary>
    /// Gets the value of the largest element.
    /// </summary>
    /// <value>The value of the largest element.</value>
    public float LargestElement
    {
      get
      {
        float max = _v[0];
        for (int i = 1; i < NumberOfElements; i++)
          if (_v[i] > max)
            max = _v[i];

        return max;
      }
    }


    /// <summary>
    /// Gets the index (zero-based) of the largest element.
    /// </summary>
    /// <value>The index (zero-based) of the largest element.</value>
    /// <remarks>
    /// <para>
    /// This method returns the index of the element which has the largest value. 
    /// </para>
    /// <para>
    /// If there are several largest elements with equally large values, the smallest index of these 
    /// is returned.
    /// </para>
    /// </remarks>
    public int IndexOfLargestElement
    {
      get
      {
        float maxValue = _v[0];
        int maxIndex = 0;
        for (int i = 1; i < NumberOfElements; i++)
        {
          if (_v[i] > maxValue)
          {
            maxValue = _v[i];
            maxIndex = i;
          }
        }

        return maxIndex;
      }
    }


    /// <summary>
    /// Gets the value of the smallest element.
    /// </summary>
    /// <value>The value of the smallest element.</value>
    public float SmallestElement
    {
      get
      {
        float min = _v[0];
        for (int i = 1; i < NumberOfElements; i++)
          if (_v[i] < min)
            min = _v[i];

        return min;
      }
    }


    /// <summary>
    /// Gets the index (zero-based) of the smallest element.
    /// </summary>
    /// <value>The index (zero-based) of the smallest element.</value>
    /// <remarks>
    /// <para>
    /// This method returns the index of the element which has the smallest value.
    /// </para>
    /// <para>
    /// If there are several smallest element with equally large values, the smallest index of these
    /// is returned.
    /// </para>
    /// </remarks>
    public int IndexOfSmallestElement
    {
      get
      {
        float minValue = _v[0];
        int minIndex = 0;
        for (int i = 1; i < NumberOfElements; i++)
        {
          if (_v[i] < minValue)
          {
            minValue = _v[i];
            minIndex = i;
          }
        }

        return minIndex;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorF"/> class with 4 vector elements.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> This constructor is used for serialization. Normally, the other 
    /// constructors should be used.
    /// </remarks>
    public VectorF()
      : this(4)
    {
    }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="VectorF"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="VectorF"/> class.
    /// </summary>
    /// <param name="numberOfElements">The number of elements.</param>
    /// <remarks>
    /// The vector elements are set to <c>0</c>.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="numberOfElements"/> must be greater than 0.
    /// </exception>
    public VectorF(int numberOfElements)
    {
      if (numberOfElements <= 0)
        throw new ArgumentException("The number of elements must be greater than 0.", "numberOfElements");

      _v = new float[numberOfElements];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="VectorF"/> class.
    /// </summary>
    /// <param name="numberOfElements">The number of elements.</param>
    /// <param name="value">The initial value for the vector elements.</param>
    /// <remarks>
    /// All elements are set to <paramref name="value"/>.
    /// </remarks>
    public VectorF(int numberOfElements, float value)
      : this(numberOfElements)
    {
      for (int i = 0; i < numberOfElements; i++)
        _v[i] = value;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="VectorF"/> class.
    /// </summary>
    /// <param name="elements">The array with the initial values for the vector elements.</param>
    /// <remarks>
    /// The created <see cref="VectorF"/> will have the same number of elements as the array 
    /// <paramref name="elements"/>.
    /// </remarks>
    /// <exception cref="NullReferenceException">
    /// <paramref name="elements"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public VectorF(float[] elements)
      : this(elements.Length)
    {
      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = elements[i];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="VectorF"/> class.
    /// </summary>
    /// <param name="elements">The list with the initial values for the vector elements.</param>
    /// <remarks>
    /// The created <see cref="VectorF"/> will have the same number of elements as the list 
    /// <paramref name="elements"/>.
    /// </remarks>
    /// <exception cref="NullReferenceException">
    /// <paramref name="elements"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public VectorF(IList<float> elements)
      : this(elements.Count)
    {
      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = elements[i];
    }


#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
    /// <summary>
    /// Initializes a new instance of the <see cref="VectorF"/> class with serialized data.
    /// </summary>
    /// <param name="info">The object that holds the serialized object data.</param>
    /// <param name="context">The contextual information about the source or destination.</param>
    /// <exception cref="SerializationException">
    /// Couldn't deserialize <see cref="VectorF"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected VectorF(SerializationInfo info, StreamingContext context)
    {
      try
      {
        _v = (float[]) info.GetValue("Elements", typeof(float[]));
      }
      catch (Exception exception)
      {
        throw new SerializationException("Couldn't deserialize VectorF.", exception);
      }
    }
#endif
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
      unchecked
      {
        int hashCode = 0;
        for (int i = 0; i < NumberOfElements; i++)
          hashCode = (hashCode * 397) ^ _v[i].GetHashCode();

        return hashCode;
      }
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
      VectorF v = obj as VectorF;
      if (v == null)
        return false;

      return this == v;
    }


    #region IEquatable<VectorF> Members
    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(VectorF other)
    {
      return this == other;
    }
    #endregion


    /// <overloads>
    /// <summary>
    /// Returns the string representation of this vector.
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
      StringBuilder sb = new StringBuilder();
      sb.Append("(");
      for (int i = 0; i < NumberOfElements; i++)
      {
        sb.Append(_v[i]);
        if (i + 1 < NumberOfElements)
          sb.Append("; ");
      }
      sb.Append(")");
      return string.Format(provider, sb.ToString());
    }


    #region ISerializable Members

#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
    /// <summary>
    /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target 
    /// object.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
    /// <param name="context">
    /// The destination (see <see cref="StreamingContext"/>) for this serialization.
    /// </param>
    /// <exception cref="SecurityException">
    /// The caller does not have the required permission.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
    protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Elements", _v);
    }


    /// <summary>
    /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target 
    /// object.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
    /// <param name="context">
    /// The destination (see <see cref="StreamingContext"/>) for this serialization.
    /// </param>
    /// <exception cref="SecurityException">
    /// The caller does not have the required permission.
    /// </exception>
    [SecurityCritical]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new ArgumentNullException("info");

      GetObjectData(info, context);
    }
#endif
    #endregion


    #region IXmlSerializable Members

    /// <summary>
    /// This property is reserved, apply the <see cref="XmlSchemaProviderAttribute"/> to the class 
    /// instead.
    /// </summary>
    /// <returns>
    /// An <see cref="XmlSchema"/> that describes the XML representation of the object that is 
    /// produced by the <see cref="IXmlSerializable.WriteXml(XmlWriter)"/> method and consumed by
    /// the <see cref="IXmlSerializable.ReadXml(XmlReader)"/> method.
    /// </returns>
    public XmlSchema GetSchema()
    {
      return null;
    }


    /// <summary>
    /// Generates an object from its XML representation.
    /// </summary>
    /// <param name="reader">
    /// The <see cref="XmlReader"/> stream from which the object is deserialized.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public void ReadXml(XmlReader reader)
    {
      reader.ReadStartElement();
      reader.ReadStartElement("Dimension");
      int n = reader.ReadContentAsInt();
      reader.ReadEndElement();

      reader.ReadStartElement("Elements");
      _v = new float[n];
      for (int i = 0; i < n; i++)
        _v[i] = reader.ReadElementContentAsFloat();

      reader.ReadEndElement();
      reader.ReadEndElement();
    }


    /// <summary>
    /// Converts an object into its XML representation.
    /// </summary>
    /// <param name="writer">
    /// The <see cref="XmlWriter"/> stream to which the object is serialized.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public void WriteXml(XmlWriter writer)
    {
      writer.WriteStartElement("Dimension");
      writer.WriteValue(NumberOfElements);
      writer.WriteEndElement();

      writer.WriteStartElement("Elements");
      for (int i = 0; i < NumberOfElements; i++)
      {
        writer.WriteStartElement("E");
        writer.WriteValue(_v[i]);
        writer.WriteEndElement();
      }

      writer.WriteEndElement();
    }
    #endregion
    #endregion


    //--------------------------------------------------------------
    #region Overloaded Operators
    //--------------------------------------------------------------

    /// <summary>
    /// Negates a vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The negated vector.</returns>
    public static VectorF operator -(VectorF vector)
    {
      if (vector == null)
        return null;

      VectorF result = vector.Clone();
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = -result[i];

      return result;
    }


    /// <summary>
    /// Negates a vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The negated vector.</returns>
    public static VectorF Negate(VectorF vector)
    {
      return -vector;
    }


    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    public static VectorF operator +(VectorF vector1, VectorF vector2)
    {
      if (vector1 == null)
        throw new ArgumentNullException("vector1");
      if (vector2 == null)
        throw new ArgumentNullException("vector2");
      if (vector1.NumberOfElements != vector2.NumberOfElements)
        throw new ArgumentException("The number of elements of the two vectors does not match.");

      VectorF result = new VectorF(vector1.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = vector1[i] + vector2[i];

      return result;
    }


    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    public static VectorF Add(VectorF vector1, VectorF vector2)
    {
      return vector1 + vector2;
    }


    /// <summary>
    /// Subtracts a vector from a vector.
    /// </summary>
    /// <param name="minuend">The first vector (minuend).</param>
    /// <param name="subtrahend">The second vector (subtrahend).</param>
    /// <returns>The difference of the two vectors.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="minuend"/> or <paramref name="subtrahend"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    public static VectorF operator -(VectorF minuend, VectorF subtrahend)
    {
      if (minuend == null)
        throw new ArgumentNullException("minuend");
      if (subtrahend == null)
        throw new ArgumentNullException("subtrahend");
      if (minuend.NumberOfElements != subtrahend.NumberOfElements)
        throw new ArgumentException("The number of elements of the two vectors does not match.");

      VectorF result = new VectorF(minuend.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = minuend[i] - subtrahend[i];

      return result;
    }


    /// <summary>
    /// Subtracts a vector from a vector.
    /// </summary>
    /// <param name="minuend">The first vector (minuend).</param>
    /// <param name="subtrahend">The second vector (subtrahend).</param>
    /// <returns>The difference of the two vectors.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="minuend"/> or <paramref name="subtrahend"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    public static VectorF Subtract(VectorF minuend, VectorF subtrahend)
    {
      return minuend - subtrahend;
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
    /// <returns>The vector with each element multiplied by scalar.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector"/> is <see langword="null"/>.
    /// </exception>
    public static VectorF operator *(VectorF vector, float scalar)
    {
      if (vector == null)
        throw new ArgumentNullException("vector");

      VectorF result = new VectorF(vector.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = vector[i] * scalar;

      return result;
    }


    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The vector with each element multiplied by <paramref name="scalar"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector"/> is <see langword="null"/>.
    /// </exception>
    public static VectorF operator *(float scalar, VectorF vector)
    {
      return vector * scalar;
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
    /// <returns>The vector with each element multiplied by <paramref name="scalar"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector"/> is <see langword="null"/>.
    /// </exception>
    public static VectorF Multiply(float scalar, VectorF vector)
    {
      return vector * scalar;
    }


    /// <summary>
    /// Multiplies the components of two vectors by each other.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The element-wise product of the two vectors.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    public static VectorF operator *(VectorF vector1, VectorF vector2)
    {
      if (vector1 == null)
        throw new ArgumentNullException("vector1");
      if (vector2 == null)
        throw new ArgumentNullException("vector2");
      if (vector1.NumberOfElements != vector2.NumberOfElements)
        throw new ArgumentException("The number of elements of the two vectors does not match.");

      VectorF result = new VectorF(vector1.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = vector1[i] * vector2[i];

      return result;
    }


    /// <summary>
    /// Multiplies the components of two vectors by each other.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The element-wise product of the two vectors.</returns>
    public static VectorF Multiply(VectorF vector1, VectorF vector2)
    {
      return vector1 * vector2;
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
    /// <returns>The vector with each element divided by <paramref name="scalar"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector"/> is <see langword="null"/>.
    /// </exception>
    public static VectorF operator /(VectorF vector, float scalar)
    {
      if (vector == null)
        throw new ArgumentNullException("vector");

      float f = 1 / scalar;
      VectorF result = new VectorF(vector.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = vector[i] * f;

      return result;
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
    /// <returns>The vector with each element divided by <paramref name="scalar"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector"/> is <see langword="null"/>.
    /// </exception>
    public static VectorF Divide(VectorF vector, float scalar)
    {
      return vector / scalar;
    }


    /// <summary>
    /// Divides the elements of a vector by the elements of another 
    /// vector.
    /// </summary>
    /// <param name="dividend">The first vector (dividend).</param>
    /// <param name="divisor">The second vector (divisor).</param>
    /// <returns>The element-wise product of the two vectors.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="dividend"/> or <paramref name="divisor"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    public static VectorF operator /(VectorF dividend, VectorF divisor)
    {
      if (dividend == null)
        throw new ArgumentNullException("dividend");
      if (divisor == null)
        throw new ArgumentNullException("divisor");
      if (dividend.NumberOfElements != divisor.NumberOfElements)
        throw new ArgumentException("The number of elements of the two vectors does not match.");

      VectorF result = new VectorF(dividend.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = dividend[i] / divisor[i];

      return result;
    }


    /// <summary>
    /// Divides the elements of a vector by the elements of another 
    /// vector.
    /// </summary>
    /// <param name="dividend">The first vector (dividend).</param>
    /// <param name="divisor">The second vector (divisor).</param>
    /// <returns>The element-wise division of the two vectors.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="dividend"/> or <paramref name="divisor"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    public static VectorF Divide(VectorF dividend, VectorF divisor)
    {
      return dividend / divisor;
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
    /// For the test the corresponding elements of the vectors are compared.
    /// </remarks>
    public static bool operator ==(VectorF vector1, VectorF vector2)
    {
      if (ReferenceEquals(vector1, vector2))
        return true;
      if (ReferenceEquals(vector1, null) || ReferenceEquals(vector2, null))
        return false;

      if (vector1.NumberOfElements != vector2.NumberOfElements)
        return false;

      for (int i = 0; i < vector1.NumberOfElements; i++)
        if (vector1[i] != vector2[i])
          return false;

      return true;
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
    /// For the test the corresponding elements of the vectors are compared.
    /// </remarks>
    public static bool operator !=(VectorF vector1, VectorF vector2)
    {
      return !(vector1 == vector2);
    }


    /// <summary>
    /// Tests if each element of a vector is greater than the corresponding element of another
    /// vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is greater than its
    /// counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator >(VectorF vector1, VectorF vector2)
    {
      if (vector1 == null)
        throw new ArgumentNullException("vector1");
      if (vector2 == null)
        throw new ArgumentNullException("vector2");
      if (vector1.NumberOfElements != vector2.NumberOfElements)
        throw new ArgumentException("The number of elements of the two vectors does not match.");

      VectorF result = new VectorF(vector1.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        if (vector1[i] <= vector2[i])
          return false;

      return true;
    }


    /// <summary>
    /// Tests if each element of a vector is greater or equal than the corresponding element of
    /// another vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is greater or equal
    /// than its counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator >=(VectorF vector1, VectorF vector2)
    {
      if (vector1 == null)
        throw new ArgumentNullException("vector1");
      if (vector2 == null)
        throw new ArgumentNullException("vector2");
      if (vector1.NumberOfElements != vector2.NumberOfElements)
        throw new ArgumentException("The number of elements of the two vectors does not match.");

      VectorF result = new VectorF(vector1.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        if (vector1[i] < vector2[i])
          return false;

      return true;
    }


    /// <summary>
    /// Tests if each element of a vector is less than the corresponding element of another vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is less than its 
    /// counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator <(VectorF vector1, VectorF vector2)
    {
      if (vector1 == null)
        throw new ArgumentNullException("vector1");
      if (vector2 == null)
        throw new ArgumentNullException("vector2");
      if (vector1.NumberOfElements != vector2.NumberOfElements)
        throw new ArgumentException("The number of elements of the two vectors does not match.");

      VectorF result = new VectorF(vector1.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        if (vector1[i] >= vector2[i])
          return false;

      return true;
    }


    /// <summary>
    /// Tests if each element of a vector is less or equal than the corresponding element of another
    /// vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is less or equal than
    /// its counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator <=(VectorF vector1, VectorF vector2)
    {
      if (vector1 == null)
        throw new ArgumentNullException("vector1");
      if (vector2 == null)
        throw new ArgumentNullException("vector2");
      if (vector1.NumberOfElements != vector2.NumberOfElements)
        throw new ArgumentException("The number of elements of the two vectors does not match.");

      VectorF result = new VectorF(vector1.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        if (vector1[i] > vector2[i])
          return false;

      return true;
    }


    /// <overloads>
    /// <summary>
    /// Converts a vector to another data type.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts a vector to an array of <see langword="float"/> values.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The array.</returns>
    public static explicit operator float[](VectorF vector)
    {
      if (vector == null)
        return null;

      float[] result = new float[vector.NumberOfElements];
      for (int i = 0; i < vector.NumberOfElements; i++)
        result[i] = vector[i];

      return result;
    }


    /// <summary>
    /// Converts this vector to an array of <see langword="float"/> values.
    /// </summary>
    /// <returns>The array.</returns>
    public float[] ToArray()
    {
      return (float[]) this;
    }


    /// <summary>
    /// Converts a vector to a list of <see langword="float"/> values.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The list with of <see langword="float"/> values.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    public static explicit operator List<float>(VectorF vector)
    {
      if (vector == null)
        return null;

      List<float> result = new List<float>(vector.NumberOfElements);
      for (int i = 0; i < vector.NumberOfElements; i++)
        result.Add(vector[i]);

      return result;
    }


    /// <summary>
    /// Converts this vector to a list of <see langword="float"/> values.
    /// </summary>
    /// <returns>The list of <see langword="float"/> values.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    public List<float> ToList()
    {
      return (List<float>) this;
    }


    /// <summary>
    /// Performs an explicit conversion from <see cref="VectorF"/> to <see cref="Vector2F"/>.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The result of the conversion.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// This vector has more than 2 elements.
    /// </exception>
    public static explicit operator Vector2F(VectorF vector)
    {
      if (vector == null)
        throw new ArgumentNullException("vector");
      if (vector.NumberOfElements != 2)
        throw new InvalidCastException("The number of elements does not match.");

      return new Vector2F(vector[0], vector[1]);
    }


    /// <summary>
    /// Converts this <see cref="VectorF"/> to <see cref="Vector2F"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    /// <exception cref="InvalidCastException">
    /// This vector has more than 2 elements.
    /// </exception>
    public Vector2F ToVector2F()
    {
      return (Vector2F) this;
    }


    /// <summary>
    /// Performs an explicit conversion from <see cref="VectorF"/> to <see cref="Vector3F"/>.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The result of the conversion.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// This vector has more than 3 elements.
    /// </exception>
    public static explicit operator Vector3F(VectorF vector)
    {
      if (vector == null)
        throw new ArgumentNullException("vector");
      if (vector.NumberOfElements != 3)
        throw new InvalidCastException("The number of elements does not match.");

      return new Vector3F(vector[0], vector[1], vector[2]);
    }


    /// <summary>
    /// Converts this <see cref="VectorF"/> to <see cref="Vector3F"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    /// <exception cref="InvalidCastException">
    /// This vector has more than 3 elements.
    /// </exception>
    public Vector3F ToVector3F()
    {
      return (Vector3F) this;
    }


    /// <summary>
    /// Performs an explicit conversion from <see cref="VectorF"/> to <see cref="Vector4F"/>.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The result of the conversion.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// This vector has more than 4 elements.
    /// </exception>
    public static explicit operator Vector4F(VectorF vector)
    {
      if (vector == null)
        throw new ArgumentNullException("vector");
      if (vector.NumberOfElements != 4)
        throw new InvalidCastException("The number of elements does not match.");

      return new Vector4F(vector[0], vector[1], vector[2], vector[3]);
    }


    /// <summary>
    /// Converts this <see cref="VectorF"/> to <see cref="Vector4F"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    /// <exception cref="InvalidCastException">
    /// This vector has more than 4 elements.
    /// </exception>
    public Vector4F ToVector4F()
    {
      return (Vector4F) this;
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="VectorF"/> to <see cref="VectorD"/>.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator VectorD(VectorF vector)
    {
      if (vector == null)
        return null;

      VectorD result = new VectorD(vector.NumberOfElements);
      for (int i = 0; i < vector.NumberOfElements; i++)
        result[i] = vector[i];

      return result;
    }


    /// <summary>
    /// Converts this <see cref="VectorF"/> to <see cref="VectorD"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public VectorD ToVectorD()
    {
      return this;
    }


    /// <summary>
    /// Performs an explicit conversion from <see cref="VectorF"/> to <see cref="MatrixF"/>.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    /// The created matrix will represent the vector as a column vector: <i>n</i> rows, 1 column.
    /// </remarks>
    public static explicit operator MatrixF(VectorF vector)
    {
      if (vector == null)
        return null;
      MatrixF result = new MatrixF(vector.NumberOfElements, 1);
      for (int i = 0; i < vector.NumberOfElements; i++)
        result[i, 0] = vector[i];

      return result;
    }


    /// <summary>
    /// Converts this <see cref="VectorF"/> to <see cref="MatrixF"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    /// The created matrix will represent the vector as a column vector: <i>n</i> rows, 1 column.
    /// </remarks>
    public MatrixF ToMatrixF()
    {
      return (MatrixF) this;
    }
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
    /// Sets each vector element to its absolute value.
    /// </summary>
    public void Absolute()
    {
      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = Math.Abs(_v[i]);
    }


    /// <overloads>
    /// <summary>
    /// Clamps the vector components to the range [min, max].
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Clamps the vector elements to the range [min, max].
    /// </summary>
    /// <param name="min">The min limit.</param>
    /// <param name="max">The max limit.</param>
    /// <remarks>
    /// This operation is carried out per element. Element values less than <paramref name="min"/> 
    /// are set to <paramref name="min"/>. Element values greater than <paramref name="max"/> are 
    /// set to <paramref name="max"/>.
    /// </remarks>
    public void Clamp(float min, float max)
    {
      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = MathHelper.Clamp(_v[i], min, max);
    }


    /// <overloads>
    /// <summary>
    /// Clamps near-zero vector elements to zero.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Clamps near-zero vector elements to zero.
    /// </summary>
    /// <remarks>
    /// Each vector element is compared to zero. If the element is in the interval 
    /// [-<see cref="Numeric.EpsilonF"/>, +<see cref="Numeric.EpsilonF"/>] it is set to zero, 
    /// otherwise it remains unchanged.
    /// </remarks>
    public void ClampToZero()
    {
      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = Numeric.ClampToZero(_v[i]);
    }


    /// <summary>
    /// Clamps near-zero vector elements to zero.
    /// </summary>
    /// <param name="epsilon">The tolerance value.</param>
    /// <remarks>
    /// Each vector element is compared to zero. If the element is in the interval 
    /// [-<paramref name="epsilon"/>, +<paramref name="epsilon"/>] it is set to zero, otherwise it 
    /// remains unchanged.
    /// </remarks>
    public void ClampToZero(float epsilon)
    {
      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = Numeric.ClampToZero(_v[i], epsilon);

    }


    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>A copy of this instance.</returns>
    public VectorF Clone()
    {
      return new VectorF(_v);
    }


    /// <summary>
    /// Gets a subvector of this vector.
    /// </summary>
    /// <param name="startIndex">The index of the first element of the subvector.</param>
    /// <param name="subvectorLength">The length of the subvector.</param>
    /// <returns>The subvector.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="startIndex"/> is negative or equal to or greater than the 
    /// <see cref="NumberOfElements"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="subvectorLength"/> is less than 1. Or <paramref name="startIndex"/> + 
    /// <paramref name="subvectorLength"/> exceeds the <see cref="NumberOfElements"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public VectorF GetSubvector(int startIndex, int subvectorLength)
    {
      if (startIndex < 0 || startIndex >= NumberOfElements)
        throw new ArgumentOutOfRangeException("startIndex", "The startIndex must be less than the number of elements.");
      if (subvectorLength <= 0)
        throw new ArgumentOutOfRangeException("subvectorLength", "The subvectorLength must be greater than 0.");
      if (startIndex + subvectorLength > NumberOfElements)
        throw new ArgumentOutOfRangeException("subvectorLength", "startIndex + subvectorLength must not exceed the number of elements.");

      VectorF result = new VectorF(subvectorLength);
      for (int i = startIndex; i < startIndex + subvectorLength; i++)
        result[i - startIndex] = _v[i];

      return result;
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
      float length = Length;
      if (Numeric.IsZero(length))
        throw new DivideByZeroException("Cannot normalize a vector with length 0.");

      float scale = 1 / length;
      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = _v[i] * scale;
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
      float lengthSquared = LengthSquared;
      if (Numeric.IsZero(lengthSquared, Numeric.EpsilonFSquared))
        return false;

      float length = (float)Math.Sqrt(lengthSquared);
      float scale = 1 / length;
      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = _v[i] * scale;

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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    public void ProjectTo(VectorF target)
    {
      if (target == null)
        throw new ArgumentNullException("target");

      Set(Dot(this, target) / target.LengthSquared * target);
    }


    /// <overloads>
    /// <summary>
    /// Sets the elements of vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets this instance to a copy of the specified vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <remarks>
    /// <paramref name="vector"/> can have more elements than this instance. The exceeding elements
    /// are ignored.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="vector"/> must have at least <see cref="NumberOfElements"/> elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="vector"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public void Set(VectorF vector)
    {
      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = vector[i];
    }


    /// <summary>
    /// Sets all vector elements to the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Set(float value)
    {
      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = value;
    }


    /// <summary>
    /// Sets the vector elements to the values of the array.
    /// </summary>
    /// <param name="elements">The elements array.</param>
    /// <remarks>
    /// <paramref name="elements"/> can have more elements than this instance. The exceeding
    /// elements are ignored.
    /// </remarks>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="elements"/> must have at least <see cref="NumberOfElements"/> elements.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="elements"/> is <see langword="null"/>.
    /// </exception>
    public void Set(float[] elements)
    {
      if (elements == null)
        throw new ArgumentNullException("elements");

      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = elements[i];
    }


    /// <summary>
    /// Sets the vector elements to the values of the list.
    /// </summary>
    /// <param name="elements">The elements list.</param>
    /// <remarks>
    /// <paramref name="elements"/> can have more elements than this instance. The exceeding 
    /// elements are ignored.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="elements"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="elements"/> must have at least <see cref="NumberOfElements"/> elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="elements"/> must not be <see langword="null"/>.
    /// </exception>
    public void Set(IList<float> elements)
    {
      if (elements == null)
        throw new ArgumentNullException("elements");

      for (int i = 0; i < NumberOfElements; i++)
        _v[i] = elements[i];
    }


    /// <summary>
    /// Sets a subvector of this instance.
    /// </summary>
    /// <param name="startIndex">The start index.</param>
    /// <param name="subvector">The subvector.</param>
    /// <remarks>
    /// The elements of the subvector are copied into this vector, beginning at the 
    /// <paramref name="startIndex"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="subvector"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// The <paramref name="startIndex"/> or the number of elements of the subvector is to high, so 
    /// that the subvector does not fit into this vector.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void SetSubvector(int startIndex, VectorF subvector)
    {
      if (subvector == null)
        throw new ArgumentNullException("subvector");

      for (int i = 0; i < subvector.NumberOfElements; i++)
        _v[i + startIndex] = subvector[i];
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
    public static VectorF Absolute(VectorF vector)
    {
      if (vector == null)
        return null;

      VectorF result = new VectorF(vector.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = Math.Abs(vector[i]);

      return result;
    }


    /// <overloads>
    /// <summary>
    /// Determines whether two vectors are equal (regarding a given tolerance).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether two vectors are equal (regarding the tolerance 
    /// <see cref="Numeric.EpsilonF"/>).
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if the vectors are equal (within the tolerance 
    /// <see cref="Numeric.EpsilonF"/>); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The two vectors are compared component-wise. If the differences of the components are less
    /// than <see cref="Numeric.EpsilonF"/> the vectors are considered as being equal.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    public static bool AreNumericallyEqual(VectorF vector1, VectorF vector2)
    {
      if (vector1 == null && vector2 == null)
        return true;

      if (vector1 == null)
        throw new ArgumentNullException("vector1");
      if (vector2 == null)
        throw new ArgumentNullException("vector2");

      if (vector1.NumberOfElements != vector2.NumberOfElements)
        return false;

      for (int i = 0; i < vector1.NumberOfElements; i++)
        if (Numeric.AreEqual(vector1[i], vector2[i]) == false)
          return false;

      return true;
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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    public static bool AreNumericallyEqual(VectorF vector1, VectorF vector2, float epsilon)
    {
      if (vector1 == null && vector2 == null)
        return true;

      if (vector1 == null)
        throw new ArgumentNullException("vector1");
      if (vector2 == null)
        throw new ArgumentNullException("vector2");

      if (vector1.NumberOfElements != vector2.NumberOfElements)
        return false;

      for (int i = 0; i < vector1.NumberOfElements; i++)
        if (Numeric.AreEqual(vector1[i], vector2[i], epsilon) == false)
          return false;

      return true;
    }


    /// <summary>
    /// Returns a vector with the vector elements clamped to the range [min, max].
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="min">The min limit.</param>
    /// <param name="max">The max limit.</param>
    /// <returns>A vector with clamped elements.</returns>
    /// <remarks>
    /// This operation is carried out per element. Element values less than <paramref name="min"/> 
    /// are set to <paramref name="min"/>. Element values greater than <paramref name="max"/> are 
    /// set to <paramref name="max"/>.
    /// </remarks>
    public static VectorF Clamp(VectorF vector, float min, float max)
    {
      if (vector == null)
        return null;

      VectorF result = new VectorF(vector.NumberOfElements);
      for (int i = 0; i < vector.NumberOfElements; i++)
        result[i] = MathHelper.Clamp(vector[i], min, max);

      return result;
    }


    /// <summary>
    /// Returns a vector with near-zero vector elements clamped to 0.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The vector with small elements clamped to zero.</returns>
    /// <remarks>
    /// Each vector element is compared to zero. If the element is in the interval 
    /// [-<see cref="Numeric.EpsilonF"/>, +<see cref="Numeric.EpsilonF"/>] it is set to zero, 
    /// otherwise it remains unchanged.
    /// </remarks>
    public static VectorF ClampToZero(VectorF vector)
    {
      if (vector == null)
        return null;

      for (int i = 0; i < vector.NumberOfElements; i++)
        vector[i] = Numeric.ClampToZero(vector[i]);

      return vector;
    }


    /// <summary>
    /// Returns a vector with near-zero vector elements clamped to 0.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="epsilon">The tolerance value.</param>
    /// <returns>The vector with small elements clamped to zero.</returns>
    /// <remarks>
    /// Each vector element is compared to zero. If the element is in the interval 
    /// [-<paramref name="epsilon"/>, +<paramref name="epsilon"/>] it is set to zero, otherwise it 
    /// remains unchanged.
    /// </remarks>
    public static VectorF ClampToZero(VectorF vector, float epsilon)
    {
      if (vector == null)
        return null;

      for (int i = 0; i < vector.NumberOfElements; i++)
        vector[i] = Numeric.ClampToZero(vector[i], epsilon);

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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    public static float Dot(VectorF vector1, VectorF vector2)
    {
      if (vector1 == null)
        throw new ArgumentNullException("vector1");
      if (vector2 == null)
        throw new ArgumentNullException("vector2");
      if (vector1.NumberOfElements != vector2.NumberOfElements)
        throw new ArgumentException("The number of elements of the two vectors does not match.");

      float result = 0;
      for (int i = 0; i < vector1.NumberOfElements; i++)
        result += vector1[i] * vector2[i];

      return result;
    }


    /// <summary>
    /// Returns a vector that contains the lowest value from each matching pair of elements.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The minimized vector.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    public static VectorF Min(VectorF vector1, VectorF vector2)
    {
      if (vector1 == null)
        throw new ArgumentNullException("vector1");
      if (vector2 == null)
        throw new ArgumentNullException("vector2");
      if (vector1.NumberOfElements != vector2.NumberOfElements)
        throw new ArgumentException("The number of elements of the two vectors does not match.");

      VectorF min = new VectorF(vector1.NumberOfElements);
      for (int i = 0; i < min.NumberOfElements; i++)
        min[i] = Math.Min(vector1[i], vector2[i]);

      return min;
    }


    /// <summary>
    /// Returns a vector that contains the highest value from each matching pair of components.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The maximized vector.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The numbers of elements of the two vectors do not match.
    /// </exception>
    public static VectorF Max(VectorF vector1, VectorF vector2)
    {
      if (vector1 == null)
        throw new ArgumentNullException("vector1");
      if (vector2 == null)
        throw new ArgumentNullException("vector2");
      if (vector1.NumberOfElements != vector2.NumberOfElements)
        throw new ArgumentException("The number of elements of the two vectors does not match.");

      VectorF max = new VectorF(vector1.NumberOfElements);
      for (int i = 0; i < max.NumberOfElements; i++)
        max[i] = Math.Max(vector1[i], vector2[i]);

      return max;
    }


    /// <summary>
    /// Projects a vector onto an axis given by the target vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="target">The target vector.</param>
    /// <returns>
    /// The projection of <paramref name="vector"/> onto <paramref name="target"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vector"/> or <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    public static VectorF ProjectTo(VectorF vector, VectorF target)
    {
      if (vector == null)
        throw new ArgumentNullException("vector");
      if (target == null)
        throw new ArgumentNullException("target");

      return Dot(vector, target) / target.LengthSquared * target;
    }
    #endregion
  }
}
