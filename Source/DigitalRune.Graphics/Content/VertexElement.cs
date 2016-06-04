// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Defines a single element for the input-assembler stage.
  /// </summary>
  internal struct VertexElement : IEquatable<VertexElement>
  {
    // Reference: https://msdn.microsoft.com/en-us/library/windows/desktop/ff476180.aspx

    /// <summary>
    /// Gets or sets the HLSL semantic of the element in a shader input-signature.
    /// </summary>
    /// <value>The HLSL semantic of the element in a shader input-signature.</value>
    public VertexElementSemantic Semantic
    {
      get { return _semantic; }
      set { _semantic = value; }
    }
    private VertexElementSemantic _semantic;


    /// <summary>
    /// Gets or sets the index of the semantic.
    /// </summary>
    /// <value>
    /// The (zero-based) index of the semantic. The semantic index is an integer number that
    /// modifies the semantic. It is required when there are more than one element with the same
    /// semantic in the input stream. The default value is 0.
    /// </value>
    public int SemanticIndex
    {
      get { return _semanticIndex; }
      set { _semanticIndex = value; }
    }
    private int _semanticIndex;


    /// <summary>
    /// Gets or sets the data type of the element.
    /// </summary>
    /// <value>The data type of the element.</value>
    public DataFormat Format
    {
      get { return _format; }
      set { _format = value; }
    }
    private DataFormat _format;


    // InputSlot, InputSlotClass, InstanceDataStepRate are not yet implemented.
    internal int InputSlot { get { return 0; } }


    /// <summary>
    /// Gets or sets the aligned offset in bytes from the beginning of the stream to the beginning
    /// of the element.
    /// </summary>
    /// <value>
    /// The aligned offset in bytes from the beginning of the stream to the beginning of the
    /// element. Use -1 for convenience to define the current element directly after the previous
    /// one, including any packing if necessary.
    /// </value>
    public int AlignedByteOffset
    {
      get { return _alignedByteOffset; }
      set { _alignedByteOffset = value; }
    }
    private int _alignedByteOffset;  // D3D11_APPEND_ALIGNED_ELEMENT = -1


    /// <summary>
    /// Initializes a new instance of the <see cref="VertexElement"/> struct.
    /// </summary>
    /// <param name="semantic">The HLSL semantic of the element in a shader input-signature.</param>
    /// <param name="semanticIndex">The (zero-based) index of the semantic.</param>
    /// <param name="format">The data type of the element.</param>
    /// <param name="alignedByteOffset">
    /// The aligned offset in bytes from the beginning of the stream to the beginning of the
    /// element. Use -1 for convenience to define the current element directly after the previous
    /// one, including any packing if necessary.
    /// </param>
    public VertexElement(VertexElementSemantic semantic, int semanticIndex, DataFormat format, int alignedByteOffset = -1)
    {
      _semantic = semantic;
      _semanticIndex = semanticIndex;
      _format = format;
      _alignedByteOffset = alignedByteOffset;
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(VertexElement other)
    {
      return Semantic == other.Semantic 
             && SemanticIndex == other.SemanticIndex 
             && Format == other.Format 
             && AlignedByteOffset == other.AlignedByteOffset;
    }


    /// <summary>
    /// Determines whether the specified <see cref="Object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">Another object to compare to.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object"/> is equal to this instance;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is VertexElement && Equals((VertexElement)obj);
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
      unchecked
      {
        var hashCode = (int)Semantic;
        hashCode = (hashCode * 397) ^ SemanticIndex;
        hashCode = (hashCode * 397) ^ (int)Format;
        hashCode = (hashCode * 397) ^ AlignedByteOffset;
        return hashCode;
      }
    }


    /// <summary>
    /// Compares two <see cref="VertexElement"/> objects to determine whether they are the same.
    /// </summary>
    /// <param name="left">The first element.</param>
    /// <param name="right">The second element.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(VertexElement left, VertexElement right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares two <see cref="VertexElement"/> objects to determine whether they are different.
    /// </summary>
    /// <param name="left">The first element.</param>
    /// <param name="right">The second element.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(VertexElement left, VertexElement right)
    {
      return !left.Equals(right);
    }


    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString()
    {
      return string.Format(
        "Semantic: {0}, SemanticIndex: {1}, Format: {2}, AlignedByteOffset: {3}",
        Semantic, SemanticIndex, Format, AlignedByteOffset);
    }
  }
}
