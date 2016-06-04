// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.SceneGraph;
#if PARTICLES
using DigitalRune.Particles;
#endif


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines the orientation of a billboard.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Billboards are oriented, textured polygons (usually quads) used for drawing particles and 
  /// other effects. Billboards can have various orientations depending on the type of effect.
  /// </para>
  /// <para>
  /// The <see cref="BillboardOrientation"/> class defines various standard orientations, but can 
  /// also be used to define custom billboard orientations.
  /// </para>
  /// <para>
  /// The orientation of a billboard is defined by two vectors: the normal vector and the axis 
  /// vector. 
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// <term>Billboard Normal</term>
  /// <description>
  /// <para>
  /// The normal vector of a billboard is the vector that points away from the billboard plane, 
  /// usually towards the viewpoint (camera). The property <see cref="Normal"/> is an enumeration 
  /// that defines how the normal vector is chosen.
  /// </para>
  /// <para>
  /// If not chosen automatically, the normal vector of a regular <see cref="Billboard"/> is given
  /// by the <see cref="BillboardNode"/>. The normal vector is defined by the local z-axis 
  /// (0, 0, 1) of the scene node.
  /// </para>
  /// <para>
  /// The normal vector of particles is defined by a particle parameter. The 
  /// <see cref="ParticleSystem"/> needs to have a uniform or varying particle parameter called 
  /// "Normal".
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term>Billboard Axis</term>
  /// <description>
  /// <para>
  /// The axis vector of a billboard is a vector that lies in the billboard plane. All billboard
  /// rotations are relative to this vector. Normally, the up-vector in world space or view space is
  /// used. Different axis vectors must be used for effects that have a direction (e.g. laser 
  /// beams). <see cref="IsAxisInViewSpace"/> determines if this vectors is interpreted as a vector
  /// in view space or in world space.
  /// </para>
  /// <para>
  /// The axis vector of a regular <see cref="Billboard"/> is set in defined by the 
  /// <see cref="BillboardNode"/>. The axis vector is given by the local up direction (0, 1, 0) of 
  /// the scene node.
  /// </para>
  /// <para>
  /// The axis vector of particles is defined by a particle parameter. The 
  /// <see cref="ParticleSystem"/> needs to have a uniform or varying particle parameter called 
  /// "Axis".
  /// </para>
  /// </description>
  /// </item>
  /// </list>
  /// <para>
  /// If the normal and the axis vectors are not perpendicular, then one vector is kept constant and
  /// the second vector is made orthonormal to the first vector. For most effects, the normal vector
  /// should be the fixed axis (e.g. fire, smoke). For billboards with a fixed direction in world 
  /// space (e.g. distant trees), the axis vector should be the fixed vector. The property 
  /// <see cref="IsAxisFixed"/> determines, which axis is fixed.
  /// </para>
  /// </remarks>
  /// <seealso cref="Billboard"/>
  /// <seealso cref="BillboardNormal"/>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  public struct BillboardOrientation : IEquatable<BillboardOrientation>
  {
    // Possible combinations:
    //
    // Normal is            Axis given in  Fixed vector  BillboardOrientation
    // ---------------------------------------------------------------------------
    // view plane aligned,  world,         normal        ViewPlaneAligned
    // view plane aligned,  world,         axis          AxialViewPlaneAligned
    // view plane aligned,  view,          normal        ScreenAligned
    // view plane aligned,  view,          axis          ?
    // view point oriented, world,         normal        ViewpointOriented
    // view point oriented, world,         axis          AxialViewpointOriented
    // view point oriented, view,          normal        ?
    // view point oriented, view,          axis          ?
    // custom,              world,         normal        NormalWorldOriented
    // custom,              world,         axis          AxialWorldOriented
    // custom,              view,          normal        ?
    // custom,              view,          axis          ?


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // Internal the billboard orientation is stored as an enum.
    // (The enum value can be used for state sorting.)
    // ------------------------------------------------------------------------------------------------------
    // |  28 bits: unused  |  2 bits: Normal vector      |  1 bit: Axis given in  |  1 bit: Fixed vector    |
    // |                   |  00 ... view plane aligned  |  0 ... world space     |  0 ... normal is fixed  |
    // |                   |  01 ... view point aligned  |  1 ... view space      |  1 ... axis is fixed    |
    // |                   |  11 ... custom              |                        |                         |
    // ------------------------------------------------------------------------------------------------------

    [Flags]
    private enum Flags
    {
      NormalIsFixed = 0,
      AxisIsFixed = 1,
      
      AxisInWorldSpace = 0,
      AxisInViewSpace = 2,
      
      ViewPlaneAlignedNormal = 0,
      ViewpointOrientedNormal = 4,
      CustomNormal = 12,

      // Bit masks for the different categories.
      FixedMask = 1,
      AxisMask = 2,
      NormalMask = 12,
    }
    #endregion
    

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Flags _flags;


    /// <summary>
    /// Settings for screen-aligned billboards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Screen-aligned billboards have the same orientation as the screen: They are always parallel 
    /// to the view plane and the axis vector matches the up-axis of the screen. 
    /// </para>
    /// <para>
    /// Examples using <see cref="ScreenAligned"/>: Text labels, which should always be readable and
    /// therefore need to have the same orientation as the screen.
    /// </para>
    /// </remarks>
    public static readonly BillboardOrientation ScreenAligned =
      new BillboardOrientation(BillboardNormal.ViewPlaneAligned, true, false);


    /// <summary>
    /// Settings for view plane-aligned billboards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// View plane-aligned billboards are used to render most particle effects: The billboards are 
    /// always parallel to the view plane (screen) and rotate with the camera. If not specified 
    /// explicitly, the billboard axis is the up vector (0, 1, 0) in world space.
    /// </para>
    /// <para>
    /// Examples using <see cref="ViewPlaneAligned"/>: Fire, smoke, etc.
    /// </para>
    /// </remarks>
    public static readonly BillboardOrientation ViewPlaneAligned =
      new BillboardOrientation(BillboardNormal.ViewPlaneAligned, false, false);


    /// <summary>
    /// Settings for viewpoint-oriented billboards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// </para>
    /// Viewpoint-oriented billboards are always oriented to face the viewpoint (camera position).
    /// If not specified explicitly, the billboard axis is the up vector (0, 1, 0) in world space.
    /// <para>
    /// Examples using <see cref="ViewpointOriented"/>: Impostors for large, distant objects 
    /// such clouds.
    /// </para>
    /// </remarks>
    public static readonly BillboardOrientation ViewpointOriented =
      new BillboardOrientation(BillboardNormal.ViewpointOriented, false, false);


    /// <summary>
    /// Settings for billboards with a free orientation in world space.
    /// </summary>
    /// <remarks>
    /// <para>
    /// </para>
    /// The orientation is defined by the normal vector and the axis vector. These two billboard
    /// vectors define a fixed orientation in world space.
    /// <para>
    /// Examples using <see cref="WorldOriented"/>: foliage with a fixed orientation, or decals.
    /// </para>
    /// </remarks>
    public static readonly BillboardOrientation WorldOriented =
      new BillboardOrientation(BillboardNormal.Custom, false, false);


    /// <summary>
    /// Settings for axial billboards parallel to the view plane.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Axial billboards are also known as constrained billboards or cylindrical billboards. The 
    /// billboard axis is given in world space. Billboards are rotated around the billboard axis to 
    /// face the view plane. If not specified explicitly, the billboard axis is the up vector 
    /// (0, 1, 0) in world space.
    /// </para>
    /// <para>
    /// Examples using <see cref="AxialViewPlaneAligned"/>: Impostors close to the camera such as 
    /// grass.
    /// </para>
    /// </remarks>
    public static readonly BillboardOrientation AxialViewPlaneAligned = new BillboardOrientation(
      BillboardNormal.ViewPlaneAligned, false, true);


    /// <summary>
    /// Settings for axial billboards oriented towards the viewer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Axial billboards are also known as constrained billboards or cylindrical billboards. The 
    /// billboard axis is the up-axis in world space. Billboards are rotated around the billboard 
    /// axis towards the viewpoint. If not specified explicitly, the billboard axis is the up vector 
    /// (0, 1, 0) in world space.
    /// </para>
    /// <para>
    /// Examples using <see cref="AxialViewpointOriented"/>: Impostors far away from the camera such
    /// as distant trees.
    /// </para>
    /// </remarks>
    public static readonly BillboardOrientation AxialViewpointOriented =
      new BillboardOrientation(BillboardNormal.ViewpointOriented, false, true);
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating which normal vector is used for rendering the billboard.
    /// </summary>
    /// <value>The normal vector that is used for rendering the billboard.</value>
    public BillboardNormal Normal 
    { 
      get
      {
        switch (_flags & Flags.NormalMask)
        {
          case Flags.ViewPlaneAlignedNormal:
            return BillboardNormal.ViewPlaneAligned;
          case Flags.ViewpointOrientedNormal:
            return BillboardNormal.ViewpointOriented;
          default:
            return BillboardNormal.Custom;
        }
      }
    }


    /// <summary>
    /// Gets a value indicating whether the billboard axis is given in view space.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the axis vector is given in view space; otherwise, 
    /// <see langword="false"/> if the axis vector is given in world space.
    /// </value>
    public bool IsAxisInViewSpace
    {
      get { return (_flags & Flags.AxisMask) == Flags.AxisInViewSpace; }
    }


    /// <summary>
    /// Gets a value indicating whether the billboard normal or the billboard axis is the fixed 
    /// axis.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the billboard axis vector is the fixed axis and the normal vector 
    /// is adjusted; otherwise, <see langword="false"/> if the normal vector is fixed and the axis
    /// vector is adjusted.
    /// </value>
    /// <remarks>
    /// To orient a billboard two vectors, the axis vector and the normal vector, are required. If 
    /// these two vectors are not perpendicular, then one vector is kept constant and the second 
    /// vector is made orthonormal to the first vector. For most particle effects, the normal should 
    /// be the fixed axis (e.g. for fire, smoke). For billboards with a fixed direction in world 
    /// space (e.g. for tree billboards or laser beams), the axis vector should be the fixed vector.
    /// </remarks>
    public bool IsAxisFixed
    {
      get { return (_flags & Flags.FixedMask) == Flags.AxisIsFixed; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="BillboardOrientation" /> struct.
    /// </summary>
    /// <param name="normal">The normal vector that is used for rendering the billboard.</param>
    /// <param name="isAxisInViewSpace">
    /// <see langword="true"/> if the axis vector is given in view space; otherwise, 
    /// <see langword="false"/> if the axis vector is given in world space.
    /// </param>
    /// <param name="isAxisFixed">
    /// <see langword="true"/> if the billboard axis vector is the fixed axis and the normal vector 
    /// is adjusted; otherwise, <see langword="false"/> if the normal vector is fixed and the axis
    /// vector is adjusted.
    /// </param>
    public BillboardOrientation(BillboardNormal normal, bool isAxisInViewSpace, bool isAxisFixed)
    {
      _flags = 0;
      switch (normal)
      {
        case BillboardNormal.ViewPlaneAligned:
          _flags |= Flags.ViewPlaneAlignedNormal;
          break;
        case BillboardNormal.ViewpointOriented:
          _flags |= Flags.ViewpointOrientedNormal;
          break;
        case BillboardNormal.Custom:
          _flags |= Flags.CustomNormal;
          break;
      }

      _flags |= isAxisInViewSpace ? Flags.AxisInViewSpace : Flags.AxisInWorldSpace;
      _flags |= isAxisFixed ? Flags.AxisIsFixed : Flags.NormalIsFixed;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures 
    /// like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      // Note: enum.GetHashCode() causes boxing. Use int.GetHashCode() instead.
      return ((int)_flags).GetHashCode();
    }


    /// <summary>
    /// Determines whether the specified <see cref="Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object" /> is equal to this instance; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is BillboardOrientation && Equals((BillboardOrientation)obj);
    }


    /// <summary>
    /// Determines whether the specified <see cref="BillboardOrientation"/> is equal to this 
    /// instance.
    /// </summary>
    /// <param name="other">
    /// The <see cref="BillboardOrientation"/> to compare with this instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="BillboardOrientation"/> is equal to this 
    /// instance; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(BillboardOrientation other)
    {
      return _flags == other._flags;
    }


    /// <summary>
    /// Compares two objects to determine whether they are the same. 
    /// </summary>
    /// <param name="left">Object to the left of the equality operator.</param>
    /// <param name="right">Object to the right of the equality operator.</param>
    /// <returns>
    /// <see langword="true"/> if the objects are the same; <see langword="false"/> otherwise. 
    /// </returns>
    public static bool operator ==(BillboardOrientation left, BillboardOrientation right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares two objects to determine whether they are different. 
    /// </summary>
    /// <param name="left">Object to the left of the inequality operator.</param>
    /// <param name="right">Object to the right of the inequality operator.</param>
    /// <returns>
    /// <see langword="true"/> if the objects are different; <see langword="false"/> otherwise. 
    /// </returns>
    public static bool operator !=(BillboardOrientation left, BillboardOrientation right)
    {
      return !left.Equals(right);
    }
    #endregion
  }
}
