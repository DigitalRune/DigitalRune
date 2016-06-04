// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;

#if XNA || MONOGAME
using System.Diagnostics;
using Microsoft.Xna.Framework;
#endif


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Defines a transformation that scales, rotates and translates (SRT) an object.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This type represents an affine transformation consisting of a scaling followed by a rotation 
  /// followed by a translation. Shearing (skewing) is not supported, thus this transformation
  /// cannot be used to describe general affine transformations. The <see cref="SrtTransform"/> is 
  /// very similar to the <see cref="DigitalRune.Geometry.Pose"/> type, but it adds a scale factor.
  /// </para>
  /// <para>
  /// Non-uniform scalings require special attention: When multiplying two SRT matrices, the result
  /// can contain a shearing if a non-uniform scaling and a rotation is used. SRT transformations do
  /// not support shearing. It is recommended to use this type either only with uniform scalings, or
  /// with non-uniform scalings without rotations. It is allowed to set non-uniform scaling and a
  /// rotation, but multiplying this transform with other transforms may not give the expected
  /// results.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> Newly created <see cref="SrtTransform"/>s should be initialized
  /// with <see cref="Identity"/>. The default constructor of the struct initializes the scale 
  /// vector and the rotation quaternion elements with 0 and therefore does not create a valid SRT 
  /// transformation.
  /// <code lang="csharp">	
  /// <![CDATA[
  /// // Do not use:
  /// SrtTransform srt = new SrtTransform(); // Not a valid SrtTransform!
  /// 
  /// // Initialize with identity instead:
  /// SrtTransform srt = SrtTransform.Identity;
  /// ]]>
  /// </code>
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  public struct SrtTransform : IEquatable<SrtTransform>
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// An SRT transform with no scale, rotation and translation.
    /// </summary>
    /// <remarks>
    /// The scale is set to <see cref="Vector3F.One"/>, the rotation is set to 
    /// <see cref="QuaternionF.Identity"/>, and the translation is set to 
    /// <see cref="Vector3F.Zero"/>.
    /// </remarks>
    public static readonly SrtTransform Identity = new SrtTransform(Vector3F.One, QuaternionF.Identity, Vector3F.Zero);
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The scale.
    /// </summary>
    public Vector3F Scale;

    
    /// <summary>
    /// The rotation.
    /// </summary>
    public QuaternionF Rotation;


    /// <summary>
    /// The translation.
    /// </summary>
    public Vector3F Translation;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the scale is not (1, 1, 1). 
    /// (Using a numerical tolerant comparison, see <see cref="Numeric"/>.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the scaling factor in any direction is not 1; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool HasScale
    {
      get
      {
        if (!Numeric.AreEqual(1, Scale.X) 
            || !Numeric.AreEqual(1, Scale.Y)
            || !Numeric.AreEqual(1, Scale.Z))
        {
          return true;
        }

        return false;
      }
    }


    /// <summary>
    /// Gets a value indicating whether the rotation is not the default rotation.
    /// (Using a numerical tolerant comparison, see <see cref="Numeric"/>.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the rotation describes a non-zero rotation; otherwise, 
    /// <see langword="false"/> if the rotation is not used (rotation angle is zero;
    /// <see cref="Rotation"/> is the identity quaternion).
    /// </value>
    public bool HasRotation
    {
      get
      {
        return !Numeric.AreEqual(Rotation.W, 1);
      }
    }


    /// <summary>
    /// Gets a value indicating whether the translation is not 0.
    /// (Using a numerical tolerant comparison, see <see cref="Numeric"/>.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the translation describes a non-zero translation; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool HasTranslation
    {
      get { return !Translation.IsNumericallyZero; }
    }


    /// <summary>
    /// Gets the inverse of this SRT transform.
    /// </summary>
    /// <value>The inverse of this SRT transform.</value>
    public SrtTransform Inverse
    {
      get
      {
        SrtTransform result = this;
        result.Invert();
        return result;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="SrtTransform"/> struct.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="SrtTransform"/> struct with the given rotation.
    /// </summary>
    /// <param name="rotation">The rotation.</param>
    public SrtTransform(QuaternionF rotation)
    {
      Scale = Vector3F.One;
      Rotation = rotation;
      Translation = Vector3F.Zero;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SrtTransform"/> struct with the given rotation.
    /// </summary>
    /// <param name="rotation">The rotation.</param>
    public SrtTransform(Matrix33F rotation)
    {
      Scale = Vector3F.One;
      Rotation = QuaternionF.CreateRotation(rotation);
      Translation = Vector3F.Zero;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SrtTransform"/> struct with the given rotation
    /// and translation.
    /// </summary>
    /// <param name="rotation">The rotation.</param>
    /// <param name="translation">The translation.</param>
    public SrtTransform(QuaternionF rotation, Vector3F translation)
    {
      Scale = Vector3F.One;
      Rotation = rotation;
      Translation = translation;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SrtTransform"/> struct with the given rotation
    /// and translation.
    /// </summary>
    /// <param name="rotation">The rotation.</param>
    /// <param name="translation">The translation.</param>
    public SrtTransform(Matrix33F rotation, Vector3F translation)
    {
      Scale = Vector3F.One;
      Rotation = QuaternionF.CreateRotation(rotation);
      Translation = translation;
    }    


    /// <summary>
    /// Initializes a new instance of the <see cref="SrtTransform"/> struct with the given scale,
    /// rotation and translation.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <param name="rotation">The rotation.</param>
    /// <param name="translation">The translation.</param>
    public SrtTransform(Vector3F scale, QuaternionF rotation, Vector3F translation)
    {
      Scale = scale;
      Rotation = rotation;
      Translation = translation;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SrtTransform"/> struct with the given scale,
    /// rotation and translation.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <param name="rotation">The rotation.</param>
    /// <param name="translation">The translation.</param>
    public SrtTransform(Vector3F scale, Matrix33F rotation, Vector3F translation)
    {
      Scale = scale;
      Rotation = QuaternionF.CreateRotation(rotation);
      Translation = translation;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(SrtTransform other)
    {
      return this == other;
    }


    /// <summary>
    /// Inverts the SRT transform.
    /// </summary>
    public void Invert()
    {
      Scale.X = 1 / Scale.X;
      Scale.Y = 1 / Scale.Y;
      Scale.Z = 1 / Scale.Z;

      Rotation.Conjugate();

      Translation = - Scale * Rotation.Rotate(Translation);
    }


    /// <summary>
    /// Converts a direction vector from local space to parent space.
    /// </summary>
    /// <param name="localDirection">The direction in local space.</param>
    /// <returns>The direction in parent space.</returns>
    /// <remarks>
    /// This method can be used to transform direction vectors. It applies only the rotation to the 
    /// vector. The scale and translation are ignored. 
    /// </remarks>
    public Vector3F ToParentDirection(Vector3F localDirection)
    {
      return Rotation.Rotate(localDirection);
    }


    /// <summary>
    /// Converts a direction vector from parent space to local space.
    /// </summary>
    /// <param name="worldDirection">The direction in parent space.</param>
    /// <returns>The direction in local space.</returns>
    /// <remarks>
    /// This method can be used to transform direction vectors. It applies only the rotation to the 
    /// vector. The scale and translation are ignored. 
    /// </remarks>
    public Vector3F ToLocalDirection(Vector3F worldDirection)
    {
      return Rotation.Conjugated.Rotate(worldDirection);
    }


    /// <summary>
    /// Converts a position vector from local space to parent space.
    /// </summary>
    /// <param name="localPosition">The position in local space.</param>
    /// <returns>The position in parent space.</returns>
    public Vector3F ToParentPosition(Vector3F localPosition)
    {
      return Translation + Rotation.Rotate(Scale * localPosition);
    }


    /// <summary>
    /// Converts a position vector from parent space to local space.
    /// </summary>
    /// <param name="worldPosition">The position in parent space.</param>
    /// <returns>The position in local space.</returns>
    public Vector3F ToLocalPosition(Vector3F worldPosition)
    {
      return Rotation.Conjugated.Rotate(worldPosition - Translation) * new Vector3F(1 / Scale.X, 1 / Scale.Y, 1 / Scale.Z);
    }


    /// <summary>
    /// Creates an <see cref="SrtTransform"/> from a matrix that contains a scale, a rotation, and a
    /// translation.
    /// </summary>
    /// <param name="srtMatrix">The SRT matrix.</param>
    /// <returns>
    /// An SRT transform that represents the same transformation as the 4x4-matrix.
    /// </returns>
    /// <remarks>
    /// <paramref name="srtMatrix"/> must only contain scaling, rotations and translations - 
    /// otherwise the result is undefined.
    /// </remarks>
    public static SrtTransform FromMatrix(Matrix44F srtMatrix)
    {
      SrtTransform srtTransform = new SrtTransform();
      srtMatrix.DecomposeFast(out srtTransform.Scale, out srtTransform.Rotation, out srtTransform.Translation);
      return srtTransform;
    }


    /// <summary>
    /// Converts this SRT transform to a 4x4 transformation matrix.
    /// </summary>
    /// <returns>
    /// A 4x4-matrix that represents the same transformation as the SRT transform.
    /// </returns>
    public Matrix44F ToMatrix44F()
    {
      return this;
    }


#if XNA || MONOGAME
    /// <overloads>
    /// <summary>
    /// Creates an <see cref="SrtTransform"/> from a matrix that contains a scale, a rotation and 
    /// a translation.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates an <see cref="SrtTransform"/> from a matrix that contains a scale, a rotation and 
    /// a translation. (Only available in the XNA-compatible build.)
    /// </summary>
    /// <param name="srtMatrix">The SRT matrix.</param>
    /// <returns>
    /// An SRT transform that represents the same transformation as the 4x4-matrix.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <paramref name="srtMatrix"/> must only contain scaling, rotations and translations - 
    /// otherwise the result is undefined.
    /// </para>
    /// <para>
    /// This method is available only in the XNA-compatible build of the DigitalRune.Animation.dll.
    /// </para>
    /// </remarks>
    public static SrtTransform FromMatrix(Matrix srtMatrix)
    {
      SrtTransform srtTransform = new SrtTransform();
      var m = (Matrix44F)srtMatrix;
      bool success = m.Decompose(out srtTransform.Scale, out srtTransform.Rotation, out srtTransform.Translation);

      //Debug.Assert(success, "Matrix is not a valid SRT matrix. SRT matrix must only contain scaling, rotations and translations.");

      return srtTransform;
    }


    /// <summary>
    /// Converts an SRT transform to a 4x4 transformation matrix (XNA Framework). 
    /// (Only available in the XNA-compatible build.)
    /// </summary>
    /// <returns>
    /// An 4x4-matrix that represents the same transformation as the SRT transform.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is available only in the XNA-compatible build of the DigitalRune.Animation.dll.
    /// </para>
    /// </remarks>
    public Matrix ToXna()
    {
      Vector3F s = Scale;
      Matrix33F r = Rotation.ToRotationMatrix33();
      Vector3F t = Translation;
      return new Matrix(s.X * r.M00, s.X * r.M10, s.X * r.M20, 0,
                        s.Y * r.M01, s.Y * r.M11, s.Y * r.M21, 0,
                        s.Z * r.M02, s.Z * r.M12, s.Z * r.M22, 0,
                        t.X, t.Y, t.Z, 1);
    }
#endif



    /// <summary>
    /// Creates a <see cref="Pose"/> from an <see cref="SrtTransform"/> (<see cref="Scale"/>
    /// will be ignored!).
    /// </summary>
    /// <returns>
    /// A pose that represents the same rotation and translation (ignoring all scalings).
    /// </returns>
    public Pose ToPose()
    {
      return new Pose(Translation, Rotation);
    }


    /// <summary>
    /// Creates an <see cref="SrtTransform"/> from a <see cref="Pose"/>.
    /// </summary>
    /// <param name="pose">The pose.</param>
    /// <returns>
    /// An <see cref="SrtTransform"/> that represents the same rotation and translation as
    /// the <paramref name="pose"/>.
    /// </returns>
    public static SrtTransform FromPose(Pose pose)
    {
      return new SrtTransform(pose.Orientation, pose.Position);
    }
    #endregion


    //--------------------------------------------------------------
    #region Static Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Determines whether two SRT transforms are equal (within a numerical tolerance).
    /// </summary>
    /// <param name="srtA">The first transform.</param>
    /// <param name="srtB">The second transform.</param>
    /// <returns>
    /// <see langword="true"/> if the given transforms are numerically equal; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public static bool AreNumericallyEqual(SrtTransform srtA, SrtTransform srtB)
    {
      return QuaternionF.AreNumericallyEqual(srtA.Rotation, srtB.Rotation)
             && Vector3F.AreNumericallyEqual(srtA.Translation, srtB.Translation)
             && Vector3F.AreNumericallyEqual(srtA.Scale, srtB.Scale);
    }


    /// <overloads>
    /// <summary>
    /// Interpolates two SRT transforms.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Interpolates two SRT transforms.
    /// </summary>
    /// <param name="startTransform">The start transform.</param>
    /// <param name="endTransform">The end transform.</param>
    /// <param name="parameter">
    /// The interpolation parameter. If the value is 0, the <paramref name="startTransform"/> is
    /// returned. If the value is 1, the <paramref name="endTransform"/> is returned. For values 
    /// between 0 and 1 an interpolated <see cref="SrtTransform"/> is returned.
    /// </param>
    /// <returns>An interpolated SRT transform.</returns>
    /// <remarks>
    /// All SRT components are interpolated using a linear interpolation (LERP). Spherical linear 
    /// interpolation (SLERP) is <b>not</b> used for quaternions.
    /// </remarks>
    public static SrtTransform Interpolate(SrtTransform startTransform, SrtTransform endTransform, float parameter)
    {
      // Lerp rotation.
      QuaternionF sourceRotation = startTransform.Rotation;
      QuaternionF targetRotation = endTransform.Rotation;

      // Get angle between quaternions:
      //float cosθ = QuaternionF.Dot(sourceRotation, targetRotation);
      float cosθ = sourceRotation.W * targetRotation.W + sourceRotation.X * targetRotation.X + sourceRotation.Y * targetRotation.Y + sourceRotation.Z * targetRotation.Z;

      // Invert one quaternion if we would move along the long arc of interpolation.
      if (cosθ < 0)
      {
        targetRotation.W = -targetRotation.W;
        targetRotation.X = -targetRotation.X;
        targetRotation.Y = -targetRotation.Y;
        targetRotation.Z = -targetRotation.Z;
      }

      QuaternionF resultRotation;
      resultRotation.W = sourceRotation.W + (targetRotation.W - sourceRotation.W) * parameter;
      resultRotation.X = sourceRotation.X + (targetRotation.X - sourceRotation.X) * parameter;
      resultRotation.Y = sourceRotation.Y + (targetRotation.Y - sourceRotation.Y) * parameter;
      resultRotation.Z = sourceRotation.Z + (targetRotation.Z - sourceRotation.Z) * parameter;

      // Linear interpolation creates non-normalized quaternions. We need to 
      // re-normalize the result.
      resultRotation.Normalize();

      return new SrtTransform(
        new Vector3F(startTransform.Scale.X + (endTransform.Scale.X - startTransform.Scale.X) * parameter,
                     startTransform.Scale.Y + (endTransform.Scale.Y - startTransform.Scale.Y) * parameter,
                     startTransform.Scale.Z + (endTransform.Scale.Z - startTransform.Scale.Z) * parameter),
        resultRotation,
        new Vector3F(startTransform.Translation.X + (endTransform.Translation.X - startTransform.Translation.X) * parameter,
                     startTransform.Translation.Y + (endTransform.Translation.Y - startTransform.Translation.Y) * parameter,
                     startTransform.Translation.Z + (endTransform.Translation.Z - startTransform.Translation.Z) * parameter));
    }


    /// <summary>
    /// Interpolates two SRT transforms.
    /// </summary>
    /// <param name="startTransform">The start transform.</param>
    /// <param name="endTransform">The end transform.</param>
    /// <param name="parameter">
    /// The interpolation parameter. If the value is 0, the <paramref name="startTransform"/> is
    /// returned. If the value is 1, the <paramref name="endTransform"/> is returned. For values 
    /// between 0 and 1 an interpolated <see cref="SrtTransform"/> is returned.
    /// </param>
    /// <param name="result">The interpolation result.</param>
    /// <remarks>
    /// All SRT components are interpolated using a linear interpolation (LERP). Spherical linear 
    /// interpolation (SLERP) is <b>not</b> used for quaternions.
    /// </remarks>
    public static void Interpolate(ref SrtTransform startTransform, ref SrtTransform endTransform, float parameter, ref SrtTransform result)
    {
      // Lerp scale.
      result.Scale.X = startTransform.Scale.X + (endTransform.Scale.X - startTransform.Scale.X) * parameter;
      result.Scale.Y = startTransform.Scale.Y + (endTransform.Scale.Y - startTransform.Scale.Y) * parameter;
      result.Scale.Z = startTransform.Scale.Z + (endTransform.Scale.Z - startTransform.Scale.Z) * parameter;

      // Lerp translation.
      result.Translation.X = startTransform.Translation.X + (endTransform.Translation.X - startTransform.Translation.X) * parameter;
      result.Translation.Y = startTransform.Translation.Y + (endTransform.Translation.Y - startTransform.Translation.Y) * parameter;
      result.Translation.Z = startTransform.Translation.Z + (endTransform.Translation.Z - startTransform.Translation.Z) * parameter;

      // Lerp rotation.
      QuaternionF sourceRotation = startTransform.Rotation;
      QuaternionF targetRotation = endTransform.Rotation;

      // Get angle between quaternions:
      //float cosθ = QuaternionF.Dot(sourceRotation, targetRotation);
      float cosθ = sourceRotation.W * targetRotation.W + sourceRotation.X * targetRotation.X + sourceRotation.Y * targetRotation.Y + sourceRotation.Z * targetRotation.Z;

      // Invert one quaternion if we would move along the long arc of interpolation.
      if (cosθ < 0)
      {
        // Lerp with inverted target!
        result.Rotation.W = sourceRotation.W - (targetRotation.W + sourceRotation.W) * parameter;
        result.Rotation.X = sourceRotation.X - (targetRotation.X + sourceRotation.X) * parameter;
        result.Rotation.Y = sourceRotation.Y - (targetRotation.Y + sourceRotation.Y) * parameter;
        result.Rotation.Z = sourceRotation.Z - (targetRotation.Z + sourceRotation.Z) * parameter;
      }
      else
      {
        // Normal lerp.
        result.Rotation.W = sourceRotation.W + (targetRotation.W - sourceRotation.W) * parameter;
        result.Rotation.X = sourceRotation.X + (targetRotation.X - sourceRotation.X) * parameter;
        result.Rotation.Y = sourceRotation.Y + (targetRotation.Y - sourceRotation.Y) * parameter;
        result.Rotation.Z = sourceRotation.Z + (targetRotation.Z - sourceRotation.Z) * parameter;
      }      

      // Linear interpolation creates non-normalized quaternions. We need to 
      // re-normalize the result.
      result.Rotation.Normalize();
    }
    

    /// <summary>
    /// Determines whether the specified matrix is a valid SRT matrix.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>
    /// <see langword="true"/> if the specified matrix is a valid SRT matrix; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsValid(Matrix44F matrix)
    {
      Vector3F s, t;
      Matrix33F r;
      return matrix.Decompose(out s, out r, out t);
    }
    #endregion


    //--------------------------------------------------------------
    #region Overrides
    //--------------------------------------------------------------

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>
    /// A 32-bit signed integer that is the hash code for this instance.
    /// </returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        int hashCode = Scale.GetHashCode();
        hashCode = (hashCode * 397) ^ Rotation.GetHashCode();
        hashCode = (hashCode * 397) ^ Translation.GetHashCode();
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
      return obj is SrtTransform && this == (SrtTransform)obj;
    }


    /// <overloads>
    /// <summary>
    /// Returns the string representation of this SRT transform.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Returns the string representation of this SRT transform.
    /// </summary>
    /// <returns>
    /// The string representation of this SRT transform.
    /// </returns>
    public override string ToString()
    {
      return ToString(CultureInfo.CurrentCulture);
    }


    /// <summary>
    /// Returns the string representation of this SRT transform using the specified culture-specific format
    /// information.
    /// </summary>
    /// <param name="provider">
    /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
    /// </param>
    /// <returns>The string representation of this SRT transform.</returns>
    public string ToString(IFormatProvider provider)
    {
      return string.Format(
        provider, 
        "SrtTransform {{ Scale = {0}, Rotation = {1}, Translation = {2} }}", 
        Scale, Rotation, Translation);
    }
    #endregion


    //--------------------------------------------------------------
    #region Overloaded Operators
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Multiplies an <see cref="SrtTransform"/> with another value.
    /// </summary>
    /// </overloads> 
    /// 
    /// <summary>
    /// Multiplies two SRT transforms.
    /// </summary>
    /// <param name="srt1">The first transform.</param>
    /// <param name="srt2">The second transform.</param>
    /// <returns>The product of srt1 and srt2: srt1 * srt2.</returns>
    /// <remarks>
    /// <para>
    /// When product (<paramref name="srt1"/> * <paramref name="srt2"/>) is applied to a vector 
    /// <i>v</i> the transformation are applied in the following order: 
    /// <i>v'</i> = srt1 * srt2 * <i>v</i><br/>
    /// That means, the vector is first transformed by <paramref name="srt2"/> and then by 
    /// <paramref name="srt1"/>.
    /// </para>
    /// </remarks>
    public static SrtTransform operator *(SrtTransform srt1, SrtTransform srt2)
    {
      //return new SrtTransform(srt1.Scale * srt2.Scale,
      //               srt1.Rotation * srt2.Rotation,
      //               srt1.Translation + srt1.Scale * srt1.Rotation.Rotate(srt2.Translation));

      SrtTransform result;
      result.Scale.X = srt1.Scale.X * srt2.Scale.X;
      result.Scale.Y = srt1.Scale.Y * srt2.Scale.Y;
      result.Scale.Z = srt1.Scale.Z * srt2.Scale.Z;

      result.Rotation.W = srt1.Rotation.W * srt2.Rotation.W - srt1.Rotation.X * srt2.Rotation.X - srt1.Rotation.Y * srt2.Rotation.Y - srt1.Rotation.Z * srt2.Rotation.Z;
      result.Rotation.X = srt1.Rotation.W * srt2.Rotation.X + srt1.Rotation.X * srt2.Rotation.W + srt1.Rotation.Y * srt2.Rotation.Z - srt1.Rotation.Z * srt2.Rotation.Y;
      result.Rotation.Y = srt1.Rotation.W * srt2.Rotation.Y - srt1.Rotation.X * srt2.Rotation.Z + srt1.Rotation.Y * srt2.Rotation.W + srt1.Rotation.Z * srt2.Rotation.X;
      result.Rotation.Z = srt1.Rotation.W * srt2.Rotation.Z + srt1.Rotation.X * srt2.Rotation.Y - srt1.Rotation.Y * srt2.Rotation.X + srt1.Rotation.Z * srt2.Rotation.W;

      // Quaternion rotation:
      Vector3F localV = srt1.Rotation.V;
      float localW = srt1.Rotation.W;
      float w = -(localV.X * srt2.Translation.X + localV.Y * srt2.Translation.Y + localV.Z * srt2.Translation.Z);
      float vX = localV.Y * srt2.Translation.Z - localV.Z * srt2.Translation.Y + localW * srt2.Translation.X;
      float vY = localV.Z * srt2.Translation.X - localV.X * srt2.Translation.Z + localW * srt2.Translation.Y;
      float vZ = localV.X * srt2.Translation.Y - localV.Y * srt2.Translation.X + localW * srt2.Translation.Z;
      float inverseX = -srt1.Rotation.X;
      float inverseY = -srt1.Rotation.Y;
      float inverseZ = -srt1.Rotation.Z;

      result.Translation.X = srt1.Translation.X + srt1.Scale.X * (vY * inverseZ - vZ * inverseY + w * inverseX + localW * vX);
      result.Translation.Y = srt1.Translation.Y + srt1.Scale.Y * (vZ * inverseX - vX * inverseZ + w * inverseY + localW * vY);
      result.Translation.Z = srt1.Translation.Z + srt1.Scale.Z * (vX * inverseY - vY * inverseX + w * inverseZ + localW * vZ);
      
      return result;
    }


    /// <overloads>
    /// <summary>
    /// Multiplies an <see cref="SrtTransform"/> with another value.
    /// </summary>
    /// </overloads> 
    /// 
    /// <summary>
    /// Multiplies two SRT transforms.
    /// </summary>
    /// <param name="srt1">The first transform.</param>
    /// <param name="srt2">The second transform.</param>
    /// <returns>The product of srt1 and srt2: srt1 * srt2.</returns>
    /// <remarks>
    /// <para>
    /// When product (<paramref name="srt1"/> * <paramref name="srt2"/>) is applied to a vector 
    /// <i>v</i> the transformation are applied in the following order: 
    /// <i>v'</i> = srt1 * srt2 * <i>v</i><br/>
    /// That means, the vector is first transformed by <paramref name="srt2"/> and then by 
    /// <paramref name="srt1"/>.
    /// </para>
    /// </remarks>
    public static SrtTransform Multiply(SrtTransform srt1, SrtTransform srt2)
    {
      return new SrtTransform(srt1.Scale * srt2.Scale,
                     srt1.Rotation * srt2.Rotation,
                     srt1.Translation + srt1.Scale * srt1.Rotation.Rotate(srt2.Translation));
    }


    /// <summary>
    /// Multiplies two SRT transforms.
    /// </summary>
    /// <param name="srt1">In: The first transform.</param>
    /// <param name="srt2">In: The second transform.</param>
    /// <param name="result">Out: The product of srt1 and srt2: srt1 * srt2.</param>
    /// <remarks>
    /// <para>
    /// When product (<paramref name="srt1"/> * <paramref name="srt2"/>) is applied to a vector 
    /// <i>v</i> the transformation are applied in the following order: 
    /// <i>v'</i> = srt1 * srt2 * <i>v</i><br/>
    /// That means, the vector is first transformed by <paramref name="srt2"/> and then by 
    /// <paramref name="srt1"/>.
    /// </para>
    /// </remarks>
    public static void Multiply(ref SrtTransform srt1, ref SrtTransform srt2, out SrtTransform result)
    {
      //return new SrtTransform(srt1.Scale * srt2.Scale,
      //               srt1.Rotation * srt2.Rotation,
      //               srt1.Translation + srt1.Scale * srt1.Rotation.Rotate(srt2.Translation));

      // Inlined:
      Vector3F localV = srt1.Rotation.V;
      float localW = srt1.Rotation.W;
      float w = -(localV.X * srt2.Translation.X + localV.Y * srt2.Translation.Y + localV.Z * srt2.Translation.Z);
      float vX = localV.Y * srt2.Translation.Z - localV.Z * srt2.Translation.Y + localW * srt2.Translation.X;
      float vY = localV.Z * srt2.Translation.X - localV.X * srt2.Translation.Z + localW * srt2.Translation.Y;
      float vZ = localV.X * srt2.Translation.Y - localV.Y * srt2.Translation.X + localW * srt2.Translation.Z;
      float inverseX = -srt1.Rotation.X;
      float inverseY = -srt1.Rotation.Y;
      float inverseZ = -srt1.Rotation.Z;

      result.Translation.X = srt1.Translation.X + srt1.Scale.X * (vY * inverseZ - vZ * inverseY + w * inverseX + localW * vX);
      result.Translation.Y = srt1.Translation.Y + srt1.Scale.Y * (vZ * inverseX - vX * inverseZ + w * inverseY + localW * vY);
      result.Translation.Z = srt1.Translation.Z + srt1.Scale.Z * (vX * inverseY - vY * inverseX + w * inverseZ + localW * vZ);

      result.Scale.X = srt1.Scale.X * srt2.Scale.X;
      result.Scale.Y = srt1.Scale.Y * srt2.Scale.Y;
      result.Scale.Z = srt1.Scale.Z * srt2.Scale.Z;

      w = srt1.Rotation.W * srt2.Rotation.W - srt1.Rotation.X * srt2.Rotation.X - srt1.Rotation.Y * srt2.Rotation.Y - srt1.Rotation.Z * srt2.Rotation.Z;
      float x = srt1.Rotation.W * srt2.Rotation.X + srt1.Rotation.X * srt2.Rotation.W + srt1.Rotation.Y * srt2.Rotation.Z - srt1.Rotation.Z * srt2.Rotation.Y;
      float y = srt1.Rotation.W * srt2.Rotation.Y - srt1.Rotation.X * srt2.Rotation.Z + srt1.Rotation.Y * srt2.Rotation.W + srt1.Rotation.Z * srt2.Rotation.X;
      float z = srt1.Rotation.W * srt2.Rotation.Z + srt1.Rotation.X * srt2.Rotation.Y - srt1.Rotation.Y * srt2.Rotation.X + srt1.Rotation.Z * srt2.Rotation.W;

      result.Rotation.W = w;
      result.Rotation.X = x;
      result.Rotation.Y = y;
      result.Rotation.Z = z;
    }


#if XNA || MONOGAME
    /// <summary>
    /// Multiplies two SRT transforms. (Only available in the XNA-compatible build.)
    /// </summary>
    /// <param name="srt1">In: The first transform.</param>
    /// <param name="srt2">In: The second transform.</param>
    /// <param name="result">Out: The product of srt1 and srt2 as 4 x 4 matrix: srt1 * srt2.</param>
    /// <remarks>
    /// <para>
    /// This method is available only in the XNA-compatible build of the DigitalRune.Animation.dll.
    /// </para>
    /// <para>
    /// When product (<paramref name="srt1"/> * <paramref name="srt2"/>) is applied to a vector 
    /// <i>v</i> the transformation are applied in the following order: 
    /// <i>v'</i> = srt1 * srt2 * <i>v</i><br/>
    /// That means, the vector is first transformed by <paramref name="srt2"/> and then by 
    /// <paramref name="srt1"/>.
    /// </para>
    /// </remarks>
    public static void Multiply(ref SrtTransform srt1, ref SrtTransform srt2, out Matrix result)
    {
      //return new SrtTransform(srt1.Scale * srt2.Scale,
      //               srt1.Rotation * srt2.Rotation,
      //               srt1.Translation + srt1.Scale * srt1.Rotation.Rotate(srt2.Translation));
      
      // Inlined:
      SrtTransform srtResult;

      Vector3F localV = srt1.Rotation.V;
      float localW = srt1.Rotation.W;
      float w = -(localV.X * srt2.Translation.X + localV.Y * srt2.Translation.Y + localV.Z * srt2.Translation.Z);
      float vX = localV.Y * srt2.Translation.Z - localV.Z * srt2.Translation.Y + localW * srt2.Translation.X;
      float vY = localV.Z * srt2.Translation.X - localV.X * srt2.Translation.Z + localW * srt2.Translation.Y;
      float vZ = localV.X * srt2.Translation.Y - localV.Y * srt2.Translation.X + localW * srt2.Translation.Z;
      float inverseX = -srt1.Rotation.X;
      float inverseY = -srt1.Rotation.Y;
      float inverseZ = -srt1.Rotation.Z;

      
      srtResult.Translation.X = srt1.Translation.X + srt1.Scale.X * (vY * inverseZ - vZ * inverseY + w * inverseX + localW * vX);
      srtResult.Translation.Y = srt1.Translation.Y + srt1.Scale.Y * (vZ * inverseX - vX * inverseZ + w * inverseY + localW * vY);
      srtResult.Translation.Z = srt1.Translation.Z + srt1.Scale.Z * (vX * inverseY - vY * inverseX + w * inverseZ + localW * vZ);

      srtResult.Scale.X = srt1.Scale.X * srt2.Scale.X;
      srtResult.Scale.Y = srt1.Scale.Y * srt2.Scale.Y;
      srtResult.Scale.Z = srt1.Scale.Z * srt2.Scale.Z;

      w = srt1.Rotation.W * srt2.Rotation.W - srt1.Rotation.X * srt2.Rotation.X - srt1.Rotation.Y * srt2.Rotation.Y - srt1.Rotation.Z * srt2.Rotation.Z;
      float x = srt1.Rotation.W * srt2.Rotation.X + srt1.Rotation.X * srt2.Rotation.W + srt1.Rotation.Y * srt2.Rotation.Z - srt1.Rotation.Z * srt2.Rotation.Y;
      float y = srt1.Rotation.W * srt2.Rotation.Y - srt1.Rotation.X * srt2.Rotation.Z + srt1.Rotation.Y * srt2.Rotation.W + srt1.Rotation.Z * srt2.Rotation.X;
      float z = srt1.Rotation.W * srt2.Rotation.Z + srt1.Rotation.X * srt2.Rotation.Y - srt1.Rotation.Y * srt2.Rotation.X + srt1.Rotation.Z * srt2.Rotation.W;

      srtResult.Rotation.W = w;
      srtResult.Rotation.X = x;
      srtResult.Rotation.Y = y;
      srtResult.Rotation.Z = z;

      result = srtResult;
    }
#endif


    /// <summary>
    /// Multiplies two SRT transforms.
    /// </summary>
    /// <param name="srt1">In: The first transform.</param>
    /// <param name="srt2">In: The second transform.</param>
    /// <param name="result">Out: The product of srt1 and srt2 as 4 x 4 matrix: srt1 * srt2.</param>
    /// <remarks>
    /// <para>
    /// When product (<paramref name="srt1"/> * <paramref name="srt2"/>) is applied to a vector 
    /// <i>v</i> the transformation are applied in the following order: 
    /// <i>v'</i> = srt1 * srt2 * <i>v</i><br/>
    /// That means, the vector is first transformed by <paramref name="srt2"/> and then by 
    /// <paramref name="srt1"/>.
    /// </para>
    /// </remarks>
    public static void Multiply(ref SrtTransform srt1, ref SrtTransform srt2, out Matrix44F result)
    {
      //return new SrtTransform(srt1.Scale * srt2.Scale,
      //               srt1.Rotation * srt2.Rotation,
      //               srt1.Translation + srt1.Scale * srt1.Rotation.Rotate(srt2.Translation));

      // Inlined:
      SrtTransform srtResult;

      Vector3F localV = srt1.Rotation.V;
      float localW = srt1.Rotation.W;
      float w = -(localV.X * srt2.Translation.X + localV.Y * srt2.Translation.Y + localV.Z * srt2.Translation.Z);
      float vX = localV.Y * srt2.Translation.Z - localV.Z * srt2.Translation.Y + localW * srt2.Translation.X;
      float vY = localV.Z * srt2.Translation.X - localV.X * srt2.Translation.Z + localW * srt2.Translation.Y;
      float vZ = localV.X * srt2.Translation.Y - localV.Y * srt2.Translation.X + localW * srt2.Translation.Z;
      float inverseX = -srt1.Rotation.X;
      float inverseY = -srt1.Rotation.Y;
      float inverseZ = -srt1.Rotation.Z;


      srtResult.Translation.X = srt1.Translation.X + srt1.Scale.X * (vY * inverseZ - vZ * inverseY + w * inverseX + localW * vX);
      srtResult.Translation.Y = srt1.Translation.Y + srt1.Scale.Y * (vZ * inverseX - vX * inverseZ + w * inverseY + localW * vY);
      srtResult.Translation.Z = srt1.Translation.Z + srt1.Scale.Z * (vX * inverseY - vY * inverseX + w * inverseZ + localW * vZ);

      srtResult.Scale.X = srt1.Scale.X * srt2.Scale.X;
      srtResult.Scale.Y = srt1.Scale.Y * srt2.Scale.Y;
      srtResult.Scale.Z = srt1.Scale.Z * srt2.Scale.Z;

      w = srt1.Rotation.W * srt2.Rotation.W - srt1.Rotation.X * srt2.Rotation.X - srt1.Rotation.Y * srt2.Rotation.Y - srt1.Rotation.Z * srt2.Rotation.Z;
      float x = srt1.Rotation.W * srt2.Rotation.X + srt1.Rotation.X * srt2.Rotation.W + srt1.Rotation.Y * srt2.Rotation.Z - srt1.Rotation.Z * srt2.Rotation.Y;
      float y = srt1.Rotation.W * srt2.Rotation.Y - srt1.Rotation.X * srt2.Rotation.Z + srt1.Rotation.Y * srt2.Rotation.W + srt1.Rotation.Z * srt2.Rotation.X;
      float z = srt1.Rotation.W * srt2.Rotation.Z + srt1.Rotation.X * srt2.Rotation.Y - srt1.Rotation.Y * srt2.Rotation.X + srt1.Rotation.Z * srt2.Rotation.W;

      srtResult.Rotation.W = w;
      srtResult.Rotation.X = x;
      srtResult.Rotation.Y = y;
      srtResult.Rotation.Z = z;

      result = srtResult;
    }


    /// <summary>
    /// Multiplies an <see cref="SrtTransform"/> with a vector.
    /// </summary>
    /// <param name="srt">The SRT transform.</param>
    /// <param name="vector">The vector.</param>
    /// <returns>The transformed vector.</returns>
    /// <remarks>
    /// Multiplying an SRT transform with a vector is equal to transforming a vector from local 
    /// space to parent space.
    /// </remarks>
    public static Vector4F operator *(SrtTransform srt, Vector4F vector)
    {
      return srt.ToMatrix44F() * vector;
    }


    /// <summary>
    /// Multiplies an <see cref="SrtTransform"/> with a vector.
    /// </summary>
    /// <param name="srt">The transform.</param>
    /// <param name="vector">The vector.</param>
    /// <returns>The transformed vector.</returns>
    /// <remarks>
    /// Multiplying a SRT matrix with a vector is equal to transforming a vector from local space
    /// to parent space.
    /// </remarks>
    public static Vector4F Multiply(SrtTransform srt, Vector4F vector)
    {
      return srt.ToMatrix44F() * vector;
    }


    /// <summary>
    /// Compares two <see cref="SrtTransform"/>s to determine whether they are the same.
    /// </summary>
    /// <param name="srt1">The first transform.</param>
    /// <param name="srt2">The second transform.</param>
    /// <returns>
    /// <see langword="true"/> if the transforms are equal; otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator ==(SrtTransform srt1, SrtTransform srt2)
    {
      return srt1.Rotation == srt2.Rotation
             && srt1.Translation == srt2.Translation
             && srt1.Scale == srt2.Scale;
    }


    /// <summary>
    /// Compares two <see cref="SrtTransform"/>s to determine whether they are the different.
    /// </summary>
    /// <param name="srt1">The first transform.</param>
    /// <param name="srt2">The second transform.</param>
    /// <returns>
    /// <see langword="true"/> if the transforms are different; otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator !=(SrtTransform srt1, SrtTransform srt2)
    {
      return srt1.Rotation != srt2.Rotation
             || srt1.Translation != srt2.Translation
             || srt1.Scale != srt2.Scale;
    }


    /// <summary>
    /// Converts an SRT transform to a 4x4 transformation matrix.
    /// </summary>
    /// <param name="srt">The transform.</param>
    /// <returns>
    /// A 4x4-matrix that represents the same transformation as the SRT transform.
    /// </returns>
    public static implicit operator Matrix44F(SrtTransform srt)
    {
      //Vector3F s = srt.Scale;
      //Matrix33F r = srt.Rotation.ToRotationMatrix33();
      //Vector3F t = srt.Translation;
      //return new Matrix44F(s.X * r.M00, s.Y * r.M01, s.Z * r.M02, t.X,
      //                     s.X * r.M10, s.Y * r.M11, s.Z * r.M12, t.Y,
      //                     s.X * r.M20, s.Y * r.M21, s.Z * r.M22, t.Z,
      //                     0, 0, 0, 1);

      // Inlined:
      float twoX = 2 * srt.Rotation.X;
      float twoY = 2 * srt.Rotation.Y;
      float twoZ = 2 * srt.Rotation.Z;
      float twoXX = twoX * srt.Rotation.X;
      float twoYY = twoY * srt.Rotation.Y;
      float twoZZ = twoZ * srt.Rotation.Z;
      float twoXY = twoX * srt.Rotation.Y;
      float twoXZ = twoX * srt.Rotation.Z;
      float twoYZ = twoY * srt.Rotation.Z;
      float twoXW = twoX * srt.Rotation.W;
      float twoYW = twoY * srt.Rotation.W;
      float twoZW = twoZ * srt.Rotation.W;
      return new Matrix44F(srt.Scale.X * (1 - (twoYY + twoZZ)), srt.Scale.Y * (twoXY - twoZW),       srt.Scale.Z * (twoYW + twoXZ),       srt.Translation.X,
                           srt.Scale.X * (twoXY + twoZW),       srt.Scale.Y * (1 - (twoXX + twoZZ)), srt.Scale.Z * (twoYZ - twoXW),       srt.Translation.Y,
                           srt.Scale.X * (twoXZ - twoYW),       srt.Scale.Y * (twoXW + twoYZ),       srt.Scale.Z * (1 - (twoXX + twoYY)), srt.Translation.Z,
                           0,                                   0,                                   0,                                   1);
    }


    /// <summary>
    /// Converts an SRT transform to a <see cref="Pose"/>. (<see cref="Scale"/> will be ignored!)
    /// </summary>
    /// <param name="srt">The transform.</param>
    /// <returns>
    /// A pose that represents the same rotation and translation (ignoring all scalings).
    /// </returns>
    public static explicit operator Pose(SrtTransform srt)
    {
      return new Pose(srt.Translation, srt.Rotation);
    }


    /// <summary>
    /// Converts a <see cref="Pose"/> to an SRT transform.
    /// </summary>
    /// <param name="pose">The pose.</param>
    /// <returns>
    /// An <see cref="SrtTransform"/> that represents the same rotation and translation as
    /// the <paramref name="pose"/>.
    /// </returns>
    public static implicit operator SrtTransform(Pose pose)
    {
      return new SrtTransform(pose.Orientation, pose.Position);
    }


#if XNA || MONOGAME
    /// <summary>
    /// Converts a SRT transform to a 4x4 transformation matrix (XNA Framework). 
    /// (Only available in the XNA-compatible build.)
    /// </summary>
    /// <param name="srt">The transform.</param>
    /// <returns>
    /// A 4x4-matrix that represents the same transformation as the SRT transform.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is available only in the XNA-compatible build of the DigitalRune.Animation.dll.
    /// </para>
    /// </remarks>
    public static implicit operator Matrix(SrtTransform srt)
    {
      //Vector3F s = srt.Scale;
      //Matrix33F r = srt.Rotation.ToRotationMatrix33();
      //Vector3F t = srt.Translation;
      //return new Matrix(s.X * r.M00, s.X * r.M10, s.X * r.M20, 0,
      //                  s.Y * r.M01, s.Y * r.M11, s.Y * r.M21, 0,
      //                  s.Z * r.M02, s.Z * r.M12, s.Z * r.M22, 0,
      //                  t.X, t.Y, t.Z, 1);

      // Inlined:
      float twoX = 2 * srt.Rotation.X;
      float twoY = 2 * srt.Rotation.Y;
      float twoZ = 2 * srt.Rotation.Z;
      float twoXX = twoX * srt.Rotation.X;
      float twoYY = twoY * srt.Rotation.Y;
      float twoZZ = twoZ * srt.Rotation.Z;
      float twoXY = twoX * srt.Rotation.Y;
      float twoXZ = twoX * srt.Rotation.Z;
      float twoYZ = twoY * srt.Rotation.Z;
      float twoXW = twoX * srt.Rotation.W;
      float twoYW = twoY * srt.Rotation.W;
      float twoZW = twoZ * srt.Rotation.W;

      return new Matrix(srt.Scale.X * (1 - (twoYY + twoZZ)), srt.Scale.X * (twoXY + twoZW),       srt.Scale.X * (twoXZ - twoYW),       0,
                        srt.Scale.Y * (twoXY - twoZW),       srt.Scale.Y * (1 - (twoXX + twoZZ)), srt.Scale.Y * (twoXW + twoYZ),       0,
                        srt.Scale.Z *(twoYW + twoXZ),        srt.Scale.Z * (twoYZ - twoXW),       srt.Scale.Z * (1 - (twoXX + twoYY)), 0,
                        srt.Translation.X,                   srt.Translation.Y,                   srt.Translation.Z,                   1);
    }
#endif
    #endregion
  }
}
