// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a star of a <see cref="StarfieldNode"/>.
  /// </summary>
  public struct Star : IEquatable<Star>
  {
    /// <summary>
    /// The star position given as a direction vector. (Does not need to be normalized.)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3F Position;


    /// <summary>
    /// The star size in pixels. To avoid flickering, the star size needs to be at least 2.8 pixels.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public float Size;


    /// <summary>
    /// The star color.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3F Color;


    /// <summary>
    /// Initializes a new instance of the <see cref="Star"/> struct.
    /// </summary>
    /// <param name="position">
    /// The star position given as a direction vector. (Does not need to be normalized.)
    /// </param>
    /// <param name="size">
    /// The star size in pixels. To avoid flickering, the star size needs to be at least 2.8 pixels.
    /// </param>
    /// <param name="color">The star color.</param>
    public Star(Vector3F position, float size, Vector3F color)
    {
      Position = position;
      Size = size;
      Color = color;
    }


    /// <overloads>
    /// <summary>
    /// Determines whether the current object is equal to another object.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified <see cref="Object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Object"/> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object"/> is equal to this instance; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is Star && Equals((Star)obj);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> 
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Star other)
    {
      return Position.Equals(other.Position) 
             && Size.Equals(other.Size) 
             && Color.Equals(other.Color);
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
        // ReSharper disable NonReadonlyFieldInGetHashCode
        int hashCode = Position.GetHashCode();
        hashCode = (hashCode * 397) ^ Size.GetHashCode();
        hashCode = (hashCode * 397) ^ Color.GetHashCode();
        return hashCode;
        // ReSharper restore NonReadonlyFieldInGetHashCode
      }
    }


    /// <summary>
    /// Compares two <see cref="Star"/>s to determine whether they are the same.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns>
    /// <see langword="true"/> if the instances are equal; otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Star left, Star right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares two <see cref="MatrixF"/>s to determine whether they are different.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns>
    /// <see langword="true"/> if the instances are different; otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Star left, Star right)
    {
      return !left.Equals(right);
    }
  }
}
