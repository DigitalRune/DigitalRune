// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if PORTABLE || WINDOWS_UWP
#pragma warning disable 1574  // Disable warning "XML comment has cref attribute that could not be resolved."
#endif


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Defines the standard semantics for default effect parameters.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The standard semantic define the meaning of effect parameters. See 
  /// <see cref="EffectParameterDescription"/>.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> General semantics specified in an .fx files are case-insensitive. 
  /// Therefore, use the <see cref="StringComparer.InvariantCultureIgnoreCase"/> string comparer
  /// for parsing .fx files. But when accessed from code (C# or VB.NET) the strings are
  /// case-sensitive!  That means the standard semantics stored in 
  /// <see cref="EffectParameterDescription"/> can be  compared directly.
  /// </para>
  /// </remarks>
  public static class DefaultEffectParameterSemantics
  {
    #region ----- Animation -----

    /// <summary>
    /// The weight of a morph target (<see cref="float"/> or an array of <see cref="float"/>).
    /// </summary>
    public const string MorphWeight = "MorphWeight";


    /// <summary>
    /// The skinning matrices for mesh skinning (array of <see cref="Matrix"/>).
    /// </summary>
    public const string Bones = "Bones";
    #endregion


    #region ----- Material -----

    /// <summary>
    /// The diffuse material color as RGB (<see cref="Vector3"/>) or RGBA (<see cref="Vector4"/>).
    /// </summary>
    public const string DiffuseColor = "DiffuseColor";


    /// <summary>
    /// The albedo texture (<see cref="Texture2D"/>).
    /// </summary>
    public const string DiffuseTexture = "DiffuseTexture";


    /// <summary>
    /// The specular material color as RGB (<see cref="Vector3"/>) or RGBA (<see cref="Vector4"/>).
    /// </summary>
    public const string SpecularColor = "SpecularColor";


    /// <summary>
    /// The gloss texture (<see cref="Texture2D"/>) containing the specular intensity (not specular
    /// power).
    /// </summary>
    public const string SpecularTexture = "SpecularTexture";


    /// <summary>
    /// The material specular color exponent as a single value (<see cref="float"/>) or a
    /// per-component value (<see cref="Vector3"/>).
    /// </summary>
    public const string SpecularPower = "SpecularPower";


    /// <summary>
    /// The emissive material color as RGB (<see cref="Vector3"/>) or RGBA (<see cref="Vector4"/>).
    /// </summary>
    public const string EmissiveColor = "EmissiveColor";


    /// <summary>
    /// The emissive texture (<see cref="Texture2D"/>).
    /// </summary>
    public const string EmissiveTexture = "EmissiveTexture";


    /// <summary>
    /// The opacity (alpha) as a single value (<see cref="float"/>).
    /// </summary>
    public const string Opacity = "Opacity";


    /// <summary>
    /// The opacity (alpha) as a single value (<see cref="float"/>).
    /// </summary>
    public const string Alpha = "Alpha";

    
    /// <summary>
    /// The blend mode (<see cref="float"/>): 0 = additive blending, 1 = normal alpha blending
    /// </summary>
    public const string BlendMode = "BlendMode";


    /// <summary>
    /// The reference value (<see cref="float"/>) used for alpha testing.
    /// </summary>
    public const string ReferenceAlpha = "ReferenceAlpha";


    ///// <summary>
    ///// The opacity texture (<see cref="Texture2D"/>).
    ///// </summary>
    //public const string OpacityTexture = "OpacityTexture";


    /// <summary>
    /// The surface normal texture (<see cref="Texture2D"/>).
    /// </summary>
    public const string NormalTexture = "NormalTexture";


    ///// <summary>
    ///// The height value of a bump map (<see cref="float"/>).
    ///// </summary>
    //public const string Height = "Height";


    ///// <summary>
    ///// The height texture (<see cref="Texture2D"/>).
    ///// </summary>
    //public const string HeightTexture = "HeightTexture";


    /// <summary>
    /// The power of the Fresnel term (<see cref="float"/>).
    /// </summary>
    public const string FresnelPower = "FresnelPower";


    ///// <summary>
    ///// The refraction value that gives the coefficients to determine the normal for an environment 
    ///// map lookup. Given as a single value (<see cref="float"/>), a per-component value 
    ///// (<see cref="Vector4"/>), or a texture (<see cref="Texture2D"/>).
    ///// </summary>
    //public const string Refraction = "Refraction";


    ///// <summary>
    ///// The texture coordinate transform matrix (<see cref="Matrix"/>).
    ///// </summary>
    //public const string TextureMatrix = "TextureMatrix";


    /// <summary>
    /// The instance color as RGB (<see cref="Vector3"/>).
    /// </summary>
    public const string InstanceColor = "InstanceColor";


    /// <summary>
    /// The instance opacity (alpha) as a single value (<see cref="float"/>).
    /// </summary>
    public const string InstanceAlpha = "InstanceAlpha";
    #endregion   


    #region ----- Render Properties -----

    /// <summary>
    /// The zero-based index of the current effect pass (<see cref="int"/>).
    /// </summary>
    public const string PassIndex = "PassIndex";


    /// <summary>
    /// The source texture which is usually the last backbuffer or the result of a previous
    /// post-processor (<see cref="Texture2D"/>).
    /// </summary>
    public const string SourceTexture = "SourceTexture";


    // RenderTargetSize was removed because:
    // - It does not work with cube map render targets. (context.RenderTarget is currentlyRenderTarget2D.
    // - ViewportSize should be used instead. Having two similar semantics is confusing.
    // - It does not work if we forget to set context.RenderTarget.
    ///// <summary>
    ///// The render target width and height in pixels (<see cref="Vector2"/>).
    ///// </summary>
    //public const string RenderTargetSize = "RenderTargetSize";


    /// <summary>
    /// The viewport width and height in pixels (<see cref="Vector2"/>).
    /// </summary>
    public const string ViewportSize = "ViewportSize";
    #endregion


    #region ----- Simulation -----

    /// <summary>
    /// The <see cref="RenderContext.Time">simulation time</see> in seconds (<see cref="float"/>).
    /// </summary>
    public const string Time = "Time";


    /// <summary>
    /// The <see cref="RenderContext.Time">simulation time</see> of the previous frame in seconds
    /// (<see cref="float"/>).
    /// </summary>
    public const string LastTime = "LastTime";


    /// <summary>
    /// The time since the previous frame in seconds (<see cref="float"/>).
    /// </summary>
    public const string ElapsedTime = "ElapsedTime";
    #endregion


    #region ----- Deferred Rendering -----

    /// <summary>
    /// The G-buffer texture (<see cref="Texture"/> or <see cref="Texture2D"/>).
    /// </summary>
    public const string GBuffer = "GBuffer";


    /// <summary>
    /// The light buffer texture (<see cref="Texture"/> or <see cref="Texture2D"/>).
    /// </summary>
    public const string LightBuffer = "LightBuffer";


    /// <summary>
    /// The normals fitting texture (<see cref="Texture"/> or <see cref="Texture2D"/>) for encoding
    /// "best fit" normals.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public const string NormalsFittingTexture = "NormalsFittingTexture";
    #endregion


    #region ----- Misc -----

    ///// <summary>
    ///// A random value (<see cref="float"/>, <see cref="Vector2"/>, <see cref="Vector3"/>, or 
    ///// <see cref="Vector4"/>).
    ///// </summary>
    //public const string RandomValue = "RandomValue";


    /// <summary>
    /// An 8-bit texture (alpha only) with 16x16 dither values (<see cref="Texture"/> or 
    /// <see cref="Texture2D"/>).
    /// </summary>
    public const string DitherMap = "DitherMap";


    /// <summary>
    /// A quadratic RGBA texture (8 bit per channel) with random values (<see cref="Texture"/> or 
    /// <see cref="Texture2D"/>).
    /// </summary>
    public const string JitterMap = "JitterMap";


    /// <summary>
    /// The width of the quadratic <see cref="JitterMap"/> in texels (<see cref="float"/> or <see cref="Vector2"/>).
    /// </summary>
    public const string JitterMapSize = "JitterMapSize";


    /// <summary>
    /// A quadratic, tileable RGBA texture (8 bit per channel) with smooth noise values 
    /// (<see cref="Texture"/> or <see cref="Texture2D"/>).
    /// </summary>
    public const string NoiseMap = "NoiseMap";


    /// <summary>
    /// A 4-element vector with user-defined data for debugging (<see cref="Vector4"/>).
    /// </summary>
    /// <seealso cref="DefaultEffectBinder.Debug0"/>
    /// <seealso cref="DefaultEffectBinder.Debug1"/>
    public const string Debug = "Debug";


    /// <summary>
    /// A value containing <see cref="float.NaN"/> (<see cref="float"/>).
    /// </summary>
    public const string NaN = "NaN";
    #endregion
  }
}
