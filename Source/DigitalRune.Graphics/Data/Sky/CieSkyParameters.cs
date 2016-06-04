// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines the parameters of the CIE sky luminance distribution.
  /// </summary>
  /// <remarks>
  /// The CIE Sky Model uses 5 parameters a, b, c, d, e to define the distribution of luminance in
  /// the sky. This type provides parameters for several predefined sky types according to the CIE
  /// standard (see <see cref="Type1"/> to <see cref="Type15"/>).
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public struct CieSkyParameters : IEquatable<CieSkyParameters>
  {
    /// <summary>
    /// CIE Standard Overcast Sky, steep luminance gradation towards zenith, azimuthal uniformity.
    /// </summary>
    public static readonly CieSkyParameters Type1 = new CieSkyParameters(4.0f, -0.70f, 0f, -1.0f,  0f);
    
    /// <summary>
    /// Overcast, with steep luminance gradation and slight brightening towards the sun.
    /// </summary>
    public static readonly CieSkyParameters Type2 = new CieSkyParameters(4.0f, -0.70f, 2, -1.5f, 0.15f);

    /// <summary>
    /// Overcast, moderately graded with azimuthal uniformity.
    /// </summary>
    public static readonly CieSkyParameters Type3 = new CieSkyParameters(1.1f, -0.8f, 0f, -1.0f, 0.00f);

    /// <summary>
    /// Overcast, moderately graded and slight brightening towards the sun.
    /// </summary>
    public static readonly CieSkyParameters Type4 = new CieSkyParameters(1.1f,  -0.8f, 2f,  -1.5f, 0.15f);

    /// <summary>
    /// Sky of uniform luminance.
    /// </summary>
    public static readonly CieSkyParameters Type5 = new CieSkyParameters(0.0f, -1.0f, 0f, -1.0f, 0.00f);

    /// <summary>
    /// Partly cloudy sky, no gradation towards zenith, slight brightening towards the sun.
    /// </summary>
    public static readonly CieSkyParameters Type6 = new CieSkyParameters(0.0f, -1.0f, 2f, -1.5f, 0.15f);

    /// <summary>
    /// Partly cloudy sky, no gradation towards zenith, brighter circumsolar region.
    /// </summary>
    public static readonly CieSkyParameters Type7 = new CieSkyParameters(0.0f, -1.0f, 5f, -2.5f, 0.30f);

    /// <summary>
    /// Partly cloudy sky, no gradation towards zenith, distinct solar corona.
    /// </summary>
    public static readonly CieSkyParameters Type8 = new CieSkyParameters(0.0f, -1.0f, 10f, -3.0f, 0.45f);

    /// <summary>
    /// Partly cloudy, with the obscured sun.
    /// </summary>
    public static readonly CieSkyParameters Type9 = new CieSkyParameters(-1.0f, -0.55f, 2f, -1.5f, 0.15f);

    /// <summary>
    /// Partly cloudy, with brighter circumsolar region.
    /// </summary>
    public static readonly CieSkyParameters Type10 = new CieSkyParameters(-1.0f, -0.55f, 5f, -2.5f, 0.30f);

    /// <summary>
    /// White-blue sky with distinct solar corona.
    /// </summary>
    public static readonly CieSkyParameters Type11 = new CieSkyParameters(-1.0f, -0.55f, 10f, -3.0f, 0.45f);

    /// <summary>
    /// CIE Standard Clear Sky, low illuminance turbidity.
    /// </summary>
    public static readonly CieSkyParameters Type12 = new CieSkyParameters(-1.0f, -0.32f, 10f, -3.0f, 0.45f);

    /// <summary>
    /// CIE Standard Clear Sky, polluted atmosphere.
    /// </summary>
    public static readonly CieSkyParameters Type13 = new CieSkyParameters(-1.0f, -0.32f, 16f, -3.0f, 0.30f);

    /// <summary>
    /// Cloudless turbid sky with broad solar corona.
    /// </summary>
    public static readonly CieSkyParameters Type14 = new CieSkyParameters(-1.0f, -0.15f, 16f, -3.0f, 0.30f);

    /// <summary>
    /// White-blue turbid sky with broad solar corona.
    /// </summary>
    public static readonly CieSkyParameters Type15 = new CieSkyParameters(-1.0f, -0.15f, 24f, -2.8f, 0.15f);

    ///// <summary>
    ///// Far Cry 3 sky model.
    ///// </summary>
    //public static readonly CieSkyParameters TypeFarCry3 = new CieSkyParameters(-1, -0.08f, 24, -3, 0.3f);  // SIGGRAPH paper uses -24 instead of +24?! 


    /// <summary>The parameter a of the CIE Sky Model.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float A;

    /// <summary>The parameter b of the CIE Sky Model.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float B;

    /// <summary>The parameter c of the CIE Sky Model.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float C;

    /// <summary>The parameter d of the CIE Sky Model.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float D;

    /// <summary>The parameter e of the CIE Sky Model.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float E;


    /// <summary>
    /// Initializes a new instance of the <see cref="CieSkyParameters"/> struct.
    /// </summary>
    /// <param name="a">The parameter a.</param>
    /// <param name="b">The parameter b.</param>
    /// <param name="c">The parameter c.</param>
    /// <param name="d">The parameter d.</param>
    /// <param name="e">The parameter e.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public CieSkyParameters(float a, float b, float c, float d, float e)
    {
      A = a;
      B = b;
      C = c;
      D = d;
      E = e;
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
      return obj is CieSkyParameters && Equals((CieSkyParameters)obj);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> 
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(CieSkyParameters other)
    {
      return A.Equals(other.A) 
             && B.Equals(other.B) 
             && C.Equals(other.C) 
             && D.Equals(other.D) 
             && E.Equals(other.E);
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
        var hashCode = A.GetHashCode();
        hashCode = (hashCode * 397) ^ B.GetHashCode();
        hashCode = (hashCode * 397) ^ C.GetHashCode();
        hashCode = (hashCode * 397) ^ D.GetHashCode();
        hashCode = (hashCode * 397) ^ E.GetHashCode();
        return hashCode;
        // ReSharper restore NonReadonlyFieldInGetHashCode
      }
    }


    /// <summary>
    /// Compares two sets of <see cref="CieSkyParameters"/>s to determine whether they are the same.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns>
    /// <see langword="true"/> if the instances are equal; otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator ==(CieSkyParameters left, CieSkyParameters right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares two sets of <see cref="CieSkyParameters"/>s to determine whether they are 
    /// different.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns>
    /// <see langword="true"/> if the instances are different; otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator !=(CieSkyParameters left, CieSkyParameters right)
    {
      return !left.Equals(right);
    }
  }
}
