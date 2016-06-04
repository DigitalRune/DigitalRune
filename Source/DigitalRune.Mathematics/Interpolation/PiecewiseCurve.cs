// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
#if !UNITY
using System.Collections.ObjectModel;
#else
using DigitalRune.Collections.ObjectModel;
#endif
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Represents a curve that is defined by piecewise interpolation of curve keys (control points).
  /// </summary>
  /// <typeparam name="TParam">
  /// The type of the curve parameter (usually <see cref="float"/> or <see cref="double"/>).
  /// </typeparam>
  /// <typeparam name="TPoint">
  /// The type of the curve points (such as <see cref="Vector2F"/>, <see cref="Vector3F"/>, etc.).
  /// </typeparam>
  /// <typeparam name="TCurveKey">
  /// The type of the curve key. (A type derived from <see cref="CurveKey{TParam,TPoint}"/>.)
  /// </typeparam>
  /// <remarks>
  /// <para>
  /// A "piecewise curve", also known as "spline", is a curve with arbitrary length that is defined 
  /// by concatenating multiple curve segments.
  /// </para>
  /// <para>
  /// The <see cref="PiecewiseCurve{TParam,TPoint,TCurveKey}"/> is a collection of curve keys 
  /// (<see cref="CurveKey{TParam,TValue}"/>). Each curve key is a control point that defines a 
  /// point on the curve. The points between curve keys are created by spline interpolation of the 
  /// curve keys.
  /// </para>
  /// <para>
  /// The curve keys are also called "key frames" if the path represents an animation curve, or
  /// "waypoints" if the path represents a 2-dimensional or 3-dimensional path.
  /// </para>
  /// <para>
  /// Curve keys in a <see cref="PiecewiseCurve{TParam,TPoint,TCurveKey}"/> must not be 
  /// <see langword="null"/>.
  /// </para>
  /// <para>
  /// The methods in this interface assume that the curve keys are sorted ascending by the curve
  /// parameter (see <see cref="CurveKey{TParam,TValue}.Parameter"/>). If this is not the case, you 
  /// can call <see cref="Sort"/> to sort keys.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public abstract class PiecewiseCurve<TParam, TPoint, TCurveKey> 
    : Collection<TCurveKey>, ICurve<TParam, TPoint>
      where TCurveKey : CurveKey<TParam, TPoint>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value that defines how the curve looks after the last curve key.
    /// </summary>
    /// <value>
    /// The post-loop behavior. The default value is <see cref="CurveLoopType.Constant"/>.
    /// </value>
    /// <remarks>
    /// The parameter of the last curve key defines the end of the curve. If the user specifies a 
    /// parameter in a curve method that is greater, this property defines which curve keys and 
    /// values are used for computations.
    /// </remarks>
    public CurveLoopType PostLoop { get; set; }


    /// <summary>
    /// Gets or sets a value that defines how the curve looks before the first path key.
    /// </summary>
    /// <value>
    /// The pre-loop behavior. The default value is <see cref="CurveLoopType.Constant"/>.
    /// </value>
    /// <remarks>
    /// The parameter of the first curve key defines the start of the curve. If the user specifies a 
    /// parameter in a curve method that is less, this property defines which curve keys and values 
    /// are used for computations.
    /// </remarks>
    public CurveLoopType PreLoop { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the curve ends are smoothed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the curve ends are smoothed; otherwise, <see langword="false"/>. 
    /// The default value is <see langword="false"/>
    /// </value>
    /// <remarks>
    /// This property is only relevant if the path ends are B-splines or Catmull Rom splines. Theses
    /// spline types need additional neighbor points. At the path ends these neighbor points are
    /// missing. Per default, neighbor points are generated internally by mirroring the neighbor
    /// point on the other side of an end point through the end point. If this flag is set to 
    /// <see langword="true"/>, other virtual neighbor points are generated which result in a
    /// smoother curve.
    /// </remarks>
    public bool SmoothEnds { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public abstract TPoint GetPoint(TParam parameter);


    /// <inheritdoc/>
    public abstract TPoint GetTangent(TParam parameter);


    /// <inheritdoc/>
    public abstract TParam GetLength(TParam start, TParam end, int maxNumberOfIterations, TParam tolerance);


    /// <inheritdoc/>
    public abstract void Flatten(ICollection<TPoint> points, int maxNumberOfIterations, TParam tolerance);


    /// <summary>
    /// Gets the index of the curve key <i>before</i> or at the given parameter value.
    /// </summary>
    /// <param name="parameter">The parameter value.</param>
    /// <returns>The index of the curve key or <c>-1</c> if no suitable curve key exists.</returns>
    /// <remarks>
    /// This method assumes that the curve keys are sorted and returns index of the key with the 
    /// largest <see cref="CurveKey{TParam,TValue}.Parameter"/> value that is less than or equal to 
    /// the given parameter value. The parameter will lie between the key at the returned index and 
    /// the key at index + 1. If <paramref name="parameter"/> is beyond the start or end of the 
    /// path, a key index according to <see cref="PreLoop"/> and <see cref="PostLoop"/> is returned.
    /// </remarks>
    public abstract int GetKeyIndex(TParam parameter);


    /// <summary>
    /// Inserts an element into the <see cref="Collection{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">
    /// The object to insert. The value can be null for reference types.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero. Or <paramref name="index"/> is greater than 
    /// <see cref="Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void InsertItem(int index, TCurveKey item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      base.InsertItem(index, item);
    }


    /// <summary>
    /// Determines whether the given parameter corresponds to a mirrored oscillation loop.
    /// </summary>
    /// <param name="parameter">The parameter value.</param>
    /// <returns>
    /// <see langword="true"/> if the parameter is in a mirrored oscillation loop; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// When the parameter is less than the parameter of the first key or greater than the parameter
    /// of the last key, then the parameter is outside the regular curve. The outside behavior is 
    /// determined by <see cref="PreLoop"/> and <see cref="PostLoop"/>. If the loop type is 
    /// <see cref="CurveLoopType.Oscillate"/> the curve is mirrored after each loop cycle. This 
    /// method returns <see langword="true"/> if the parameter is outside and belongs to a curve 
    /// loop which is mirrored to the regular curve.
    /// </remarks>
    public abstract bool IsInMirroredOscillation(TParam parameter);


    /// <summary>
    /// Handles pre- and post-looping by changing the given parameter so that it lies on the curve.
    /// </summary>
    /// <param name="parameter">The parameter value.</param>
    /// <returns>The modified parameter value.</returns>
    /// <remarks>
    /// <para>
    /// If the parameter lies outside the curve the parameter is changed so that it lies on the 
    /// curve. The new parameter can be used to compute the curve result. 
    /// </para>
    /// <para>
    /// Following <see cref="CurveLoopType"/>s need special handling:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="CurveLoopType.Linear"/>: The parameter is not changed to lie on the curve; the
    /// linear extrapolation of the curve has to be computed.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="CurveLoopType.CycleOffset"/>: The parameter is corrected to be on the curve;
    /// the curve function at this parameter can be evaluated and the offset must be added. The 
    /// curve point offset is not handled in this method.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public abstract TParam LoopParameter(TParam parameter);


    /// <summary>
    /// Replaces the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="item">The new value for the element at the specified index.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero. Or <paramref name="index"/> is greater than 
    /// <see cref="Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void SetItem(int index, TCurveKey item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      base.SetItem(index, item);
    }


    /// <summary>
    /// Sorts the curve keys in the collection by their parameter (see
    /// <see cref="CurveKey{TParam,TValue}.Parameter"/>).
    /// </summary>
    public void Sort()
    {
      var sortedItems = Items.OrderBy(key => key.Parameter).ToArray();
      
      Clear();
      foreach (var item in sortedItems)
        Add(item);
    }


    /// <summary>
    /// Generates an object from its XML representation.
    /// </summary>
    /// <param name="reader">
    /// The <see cref="XmlReader"/> stream from which the object is deserialized. 
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected void ReadXml(XmlReader reader)
    {
      reader.ReadStartElement("PreLoop");
      string content = reader.ReadContentAsString();
      PreLoop = (CurveLoopType)Enum.Parse(typeof(CurveLoopType), content, false);
      reader.ReadEndElement();

      reader.ReadStartElement("PostLoop");
      content = reader.ReadContentAsString();
      PostLoop = (CurveLoopType)Enum.Parse(typeof(CurveLoopType), content, false);
      reader.ReadEndElement();

      Debug.Assert(reader.AttributeCount == 1, "CurveKeys should have \"Count\" as attribute.");
      int count = int.Parse(reader["Count"], CultureInfo.InvariantCulture);
      reader.ReadStartElement("CurveKeys");
      for (int i = 0; i < count; ++i)
      {
        Type type = Type.GetType(reader.GetAttribute("Type"));
        reader.ReadStartElement("CurveKey");
        XmlSerializer serializer = new XmlSerializer(type);
        TCurveKey key = (TCurveKey)serializer.Deserialize(reader);
        Add(key);
        reader.ReadEndElement();
      }
      reader.ReadEndElement();
    }


    /// <summary>
    /// Converts an object into its XML representation.
    /// </summary>
    /// <param name="writer">
    /// The <see cref="XmlWriter"/> stream to which the object is serialized. 
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected void WriteXml(XmlWriter writer)
    {
      writer.WriteStartElement("PreLoop");
      writer.WriteValue(PreLoop.ToString());
      writer.WriteEndElement();

      writer.WriteStartElement("PostLoop");
      writer.WriteValue(PostLoop.ToString());
      writer.WriteEndElement();

      writer.WriteStartElement("CurveKeys");
      writer.WriteStartAttribute("Count");
      writer.WriteValue(Count);
      writer.WriteEndAttribute();

      foreach (TCurveKey curveKey in this)
      {
        Type type = curveKey.GetType();
        writer.WriteStartElement("CurveKey");
        writer.WriteStartAttribute("Type");
        writer.WriteValue(type.AssemblyQualifiedName);
        writer.WriteEndAttribute();

        XmlSerializer serializer = new XmlSerializer(type);
        serializer.Serialize(writer, curveKey);

        writer.WriteEndElement();
      }

      writer.WriteEndElement();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the curve keys of the 
    /// <see cref="PiecewiseCurveF{TPoint,TCurveKey}"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="PiecewiseCurveF{TPoint,TCurveKey}"/>.
    /// </returns>
    public new List<TCurveKey>.Enumerator GetEnumerator()
    {
      return ((List<TCurveKey>)Items).GetEnumerator();
    }
    #endregion
  }
}
