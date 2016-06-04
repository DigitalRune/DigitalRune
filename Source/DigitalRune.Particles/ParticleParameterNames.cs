// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Particles
{
  /// <summary>
  /// Contains names of common particle parameters.
  /// </summary>
  public static class ParticleParameterNames
  {
    /// <summary>
    /// The lifetime of a particle in seconds.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string Lifetime = "Lifetime";


    /// <summary>
    /// The normalized age in the range [0, 1]. This parameter is created and managed by the
    /// <see cref="ParticleSystem"/> class.<br/>
    /// (Parameter type: varying, value type: <see cref="float"/>)
    /// </summary>
    public const string NormalizedAge = "NormalizedAge";


    /// <summary>
    /// The particle position.<br/>
    /// (Parameter type: varying, value type: <see cref="Vector3F"/>)
    /// </summary>
    public const string Position = "Position";


    /// <summary>
    /// The normalized direction vector that defines the movement direction.<br/>
    /// (Parameter type: varying, value type: <see cref="Vector3F"/>)
    /// </summary>
    public const string Direction = "Direction";


    /// <summary>
    /// The linear (scalar) speed along the <see cref="Direction"/> vector.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string LinearSpeed = "LinearSpeed";


    /// <summary>
    /// The acceleration vector that changes the linear velocity (<see cref="Direction"/> and 
    /// <see cref="LinearSpeed"/>).<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="Vector3F"/>)
    /// </summary>
    public const string LinearAcceleration = "LinearAcceleration";


    /// <summary>
    /// The rotation angle in radians.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string Angle = "Angle";


    ///// <summary>
    ///// The normalized rotation axis.<br/>
    ///// </summary>
    //public const string RotationAxis = "RotationAxis";


    /// <summary>
    /// The angular speed that changes the <see cref="Angle"/>.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string AngularSpeed = "AngularSpeed";


    /// <summary>
    /// The angular acceleration that changes the <see cref="AngularSpeed"/>.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string AngularAcceleration = "AngularAcceleration";


    /// <summary>
    /// The emission rate that defines how many particles are generated in particles per second.<br/>
    /// (Parameter type: uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string EmissionRate = "EmissionRate";


    /// <summary>
    /// The emitter velocity vector, usually used to modify the start velocity of particles.<br/>
    /// (Parameter type: uniform, value type: <see cref="Vector3F"/>)
    /// </summary>
    public const string EmitterVelocity = "EmitterVelocity";


    /// <summary>
    /// The damping factor that defines the strength of the damping. 0 means no damping. Higher 
    /// values mean stronger damping.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string Damping = "Damping";


    /// <summary>
    /// The particle size.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string Size = "Size";


    /// <summary>
    /// The particle color.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="Vector3F"/>)
    /// </summary>
    public const string Color = "Color";


    /// <summary>
    /// The particle opacity (0 = transparent, 1 = opaque).<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string Alpha = "Alpha";


    /// <summary>
    /// The reference value used in the alpha test. The reference value is a value in the range
    /// [0, 1]. If the alpha of a pixel is less than the reference alpha, the pixel is discarded.<br/>
    /// (Parameter type: uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string AlphaTest = "AlphaTest"; // Note: AlphaTest is currently uniform, because in
                                                 // Reach profile (AlphaTestEffect) the value cannot
                                                 // be set per particle.

    /// <summary>
    /// The particle mass.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string Mass = "Mass";


    /// <summary>
    /// The coefficient of restitution (= bounciness) of a particle (0 = no bounce,
    /// 1 = full bounce, no energy is lost).<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string Restitution = "Restitution";


    /// <summary>
    /// The particle billboard orientation.<br/>
    /// (Parameter type: uniform, value type: depends on the used particle renderer)
    /// </summary>
    public const string BillboardOrientation = "BillboardOrientation";


    /// <summary>
    /// The normal vector of a particle.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="Vector3F"/>)
    /// </summary>
    public const string Normal = "Normal";


    /// <summary>
    /// The axis vector of a particle.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="Vector3F"/>)
    /// </summary>
    public const string Axis = "Axis";


    /// <summary>
    /// The blend state that should be used by the renderer.<br/>
    /// (Parameter type: uniform, value type: depends on the used particle renderer)
    /// </summary>
    public const string BlendState = "BlendState";


    /// <summary>
    /// The blend mode that should be used by the renderer: 0 = additive blending, 1 = alpha 
    /// blending<br/>
    /// (Parameter type: uniform, value type: depends on the used particle renderer)
    /// </summary>
    public const string BlendMode = "BlendMode";


    /// <summary>
    /// The parameter that defines the draw order for particle systems on the same world space 
    /// position. Particle systems with higher draw order value are drawn on top of particle systems 
    /// with lower draw order.<br/>
    /// (Parameter type: uniform, value type: <see cref="int"/>)
    /// </summary>
    public const string DrawOrder = "DrawOrder";


    /// <summary>
    /// The parameter that indicates if the particles in the particle system should be rendered 
    /// depth-sorted.<br/>
    /// (Parameter type: uniform, value type: <see cref="bool"/>)
    /// </summary>
    public const string IsDepthSorted = "IsDepthSorted";


    /// <summary>
    /// The particle size in x-direction.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string SizeX = "SizeX";


    /// <summary>
    /// The particle size in y-direction.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string SizeY = "SizeY";


    /// <summary>
    /// The particle texture.<br/>
    /// (Parameter type: varying or uniform, value type: depends on the used particle renderer)
    /// </summary>
    public const string Texture = "Texture";


    /// <summary>
    /// The index of the frame in an animated texture (or the index of a texture in a texture 
    /// atlas).<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="int"/>)
    /// </summary>
    public const string Frame = "Frame";


    /// <summary>
    /// The normalized animation time (0 = start of animation, 1 = end of animation), which selects
    /// the current frame of an animated texture.<br/>
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string AnimationTime = "AnimationTime";


    /// <summary>
    /// The softness for rendering soft particles.<br/>
    /// (Parameter type: uniform, value type: <see cref="float"/>)
    /// </summary>
    public const string Softness = "Softness";


    #region ----- Ribbons -----

    /// <summary>
    /// The type of the particles, which determines whether particles are rendered as individual 
    /// billboards, connected quad strips ("ribbons"), meshes, etc.<br/>
    /// (Parameter type: uniform, value type: depends on the used particle renderer)
    /// </summary>
    public const string Type = "Type";


    ///// <summary>
    ///// For ribbons: A value indicating whether the newest particle is connected to the origin 
    ///// (see property <see cref="ParticleSystem.Pose"/>) of the particle system.<br/>
    ///// (Parameter type: uniform, value type: <see cref="bool"/>)
    ///// </summary>
    //public const string StartsAtOrigin = "StartsAtOrigin";


    /// <summary>
    /// For ribbons: Defines how a texture is applied to a particle ribbon ("tiling distance").<br/>
    /// 0 = No tiling: The texture is stretched along the ribbon.<br/>
    /// 1 = Tiling: The texture is repeated every particle.<br/>
    /// <i>n</i> = Tiling with lower frequency: The texture is repeated every <i>n</i> particles.<br/>
    /// (Parameter type: uniform, value type: <see cref="int"/>)
    /// </summary>
    public const string TextureTiling = "TextureTiling";


    // OBSOLETE
    /// <summary>
    /// The index of the next particle that this particle is connected to, for rendering ribbons 
    /// (a.k.a. trails or lines).<br/>
    /// (Parameter type: varying, value type: <see cref="int"/>)
    /// </summary>
    public const string LinkedIndex = "LinkedIndex";
    #endregion
  }
}
