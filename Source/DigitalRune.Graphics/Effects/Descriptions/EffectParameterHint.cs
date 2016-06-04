// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.Rendering;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Indicates how an effect parameter should be treated during state sorting.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="MeshRenderer"/> sorts meshes by render states before rendering them. State
  /// sorting is used to minimize number of state changes required for drawing a complex scene.
  /// </para>
  /// <para>
  /// Each effect parameter has an <see cref="EffectParameterHint"/>. The sort hint is a value 
  /// indicating how the effect parameter should be treated during state sorting. It basically puts
  /// each parameter into a certain category.
  /// </para>
  /// <para>
  /// The sort hint is stored in the effect parameter description (see 
  /// <see cref="EffectParameterDescription"/>). The parameter descriptions of an effect can be 
  /// read using the method <see cref="EffectHelper.GetParameterDescriptions"/>. The value is set 
  /// by one of the effect interpreters (see <see cref="IEffectInterpreter"/>) of the graphics 
  /// service (see <see cref="IGraphicsService.EffectInterpreters"/>) during effect initialization.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames", Justification = "Breaking change. Fix in next version.")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags")]
  [Flags]
  public enum EffectParameterHint
  {
    /// <summary>
    /// The effect parameter needs to be updated and applied once per effect pass. Examples for 
    /// per-pass parameters are: pass index.
    /// </summary>
    PerPass = 1,

    /// <summary>
    /// The effect parameter is unique for each mesh instance. Examples for instance parameters are: 
    /// world matrix.
    /// </summary>
    PerInstance = 2,
    
    /// <summary>
    /// The effect parameter depends on the location of the mesh in the scene. Multiple meshes which
    /// are close to each other in the scene may share the same parameter values. Examples of local 
    /// parameters are: local environment maps, local lights, etc.
    /// </summary>
    Local = 4,
    
    /// <summary>
    /// The effect parameter defines the material of a mesh. Multiple meshes can share the same 
    /// material. Material parameters are independent of the location of the object in the scene. 
    /// Examples of material parameters are: diffuse color, albedo texture, specular color, gloss 
    /// texture, normal map, etc.
    /// </summary>
    Material = 8,

    /// <summary>
    /// The effect parameter is identical for all meshes that use the same effect/technique. They do
    /// not depend on the object that is being rendered or on the location of the object in the 
    /// scene. Examples of global parameters are: view matrix, projection matrix, camera position, 
    /// etc.
    /// </summary>
    Global = 16,

    /// <summary>
    /// Any of the other values.
    /// </summary>
    Any = ~0, // 0xffffffff
  }
}
