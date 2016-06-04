// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if PORTABLE || WINDOWS_UWP
#pragma warning disable 1574  // Disable warning "XML comment has cref attribute that could not be resolved."
#endif


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Defines the standard semantics for effect parameters used in a <see cref="IScene"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The standard semantic define the meaning of effect parameters. See 
  /// <see cref="EffectParameterDescription"/>.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> General semantics specified in an .fx files are case-insensitive.
  /// Therefore, use the <see cref="StringComparer.InvariantCultureIgnoreCase"/> string comparer for
  /// parsing .fx files. But when accessed from code (C# or VB.NET) the strings are case-sensitive!
  /// That means the standard semantics stored in <see cref="EffectParameterDescription"/> can be
  /// compared directly.
  /// </para>
  /// </remarks>
  public static class SceneEffectParameterSemantics
  {
    #region ----- Bounding Shapes -----

    ///// <summary>
    ///// The bounding box maximum in x, y, and z (<see cref="Vector3"/>).
    ///// </summary>
    //public const string BoundingBoxMax = "BoundingBoxMax";


    ///// <summary>
    ///// The bounding box minimum in x, y, and z (<see cref="Vector3"/>).
    ///// </summary>
    //public const string BoundingBoxMin = "BoundingBoxMin";


    ///// <summary>
    ///// The bounding box size in x, y, and z (<see cref="Vector3"/>).
    ///// </summary>
    //public const string BoundingBoxSize = "BoundingBoxSize";


    ///// <summary>
    ///// The bounding box center (<see cref="Vector3"/> or <see cref="Vector4"/>).
    ///// </summary>
    //public const string BoundingCenter = "BoundingCenter";


    ///// <summary>
    ///// The median bounding sphere radius (<see cref="float"/>).
    ///// </summary>
    //public const string BoundingSphereRadius = "BoundingSphereRadius";
    #endregion


    #region ----- Animation -----

    /// <summary>
    /// The skinning matrices for mesh skinning (array of <see cref="Matrix"/>).
    /// </summary>
    [Obsolete("Use DefaultEffectParameterSemantics.Bones instead.")]
    public const string Bones = DefaultEffectParameterSemantics.Bones;
    #endregion


    #region ----- Scene Nodes -----

    /// <summary>
    /// The scene node type (<see cref="float"/>).
    /// </summary>
    internal const string SceneNodeType = "SceneNodeType";
    #endregion


    #region ----- Camera -----

    /// <summary>
    /// The camera direction in world space (<see cref="Vector3"/> or <see cref="Vector4"/>).
    /// </summary>
    public const string CameraDirection = "CameraDirection";


    /// <summary>
    /// The camera position in world space (<see cref="Vector3"/> or <see cref="Vector4"/>).
    /// </summary>
    public const string CameraPosition = "CameraPosition";


    /// <summary>
    /// The camera direction of the last frame in world space (<see cref="Vector3"/> or 
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string LastCameraDirection = "LastCameraDirection";


    /// <summary>
    /// The camera position of the last frame in world space (<see cref="Vector3"/> or 
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string LastCameraPosition = "LastCameraPosition";


    /// <summary>
    /// The distance of the camera near plane (<see cref="float"/>).
    /// </summary>
    public const string CameraNear = "CameraNear";


    /// <summary>
    /// The distance of the camera far plane (<see cref="float"/>).
    /// </summary>
    public const string CameraFar = "CameraFar";


    /// <summary>
    /// The position of the camera used as reference for LOD calculations (<see cref="Vector3"/>).
    /// </summary>
    public const string LodCameraPosition = "LodCameraPosition";
    #endregion


    #region ----- Lights -----

    /// <summary>
    /// The intensity of an ambient light (RGB as <see cref="Vector3"/> or RGBA as 
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string AmbientLight = "AmbientLight";


    /// <summary>
    /// The hemispheric attenuation factor of the ambient light (<see cref="float"/>).
    /// (0 = pure ambient, no hemispheric lighting; 1 = one-sided hemispheric lighting)
    /// </summary>
    public const string AmbientLightAttenuation = "AmbientLightAttenuation";


    /// <summary>
    /// The up vector of the ambient light in world space (<see cref="Vector3" />
    /// or <see cref="Vector4"/>). (Used for hemispheric attenuation 
    /// <see cref="AmbientLightAttenuation"/>.)
    /// </summary>
    public const string AmbientLightUp = "AmbientLightUp";


    /// <summary>
    /// The diffuse intensity of a directional light (<see cref="Vector3"/> or 
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string DirectionalLightDiffuse = "DirectionalLightDiffuse";


    /// <summary>
    /// The specular intensity of a directional light (<see cref="Vector3"/> or 
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string DirectionalLightSpecular = "DirectionalLightSpecular";


    /// <summary>
    /// The light direction in world space (<see cref="Vector3"/> or <see cref="Vector4"/>).
    /// </summary>
    public const string DirectionalLightDirection = "DirectionalLightDirection";


    /// <summary>
    /// The texture of a directional light (<see cref="Texture2D"/>).
    /// </summary>
    public const string DirectionalLightTexture = "DirectionalLightTexture";


    /// <summary>
    /// The texture offset of a directional light (<see cref="Vector2"/>).
    /// </summary>
    public const string DirectionalLightTextureOffset = "DirectionalLightTextureOffset";


    /// <summary>
    /// The texture scale of a directional light (<see cref="Vector2"/>).
    /// </summary>
    public const string DirectionalLightTextureScale = "DirectionalLightTextureScale";


    /// <summary>
    /// The texture matrix of a directional light which converts positions from world
    /// space to the texture space of the light (<see cref="Matrix"/>).
    /// </summary>
    public const string DirectionalLightTextureMatrix = "DirectionalLightTextureMatrix";


    /// <summary>
    /// The shadow map of a directional light shadow (<see cref="Texture2D"/>).
    /// </summary>
    public const string DirectionalLightShadowMap = "DirectionalLightShadowMap";


    /// <summary>
    /// The shadow parameters of a directional light shadow.
    /// (The type is either struct <c>ShadowParameters</c> as defined in ShadowMap.fxh
    /// or struct <c>CascadedShadowParameters</c> as defined in CascadedShadowMap.fxh.)
    /// </summary>
    public const string DirectionalLightShadowParameters = "DirectionalLightShadowParameters";


    /// <summary>
    /// The number of cascades of a directional light shadow (<see cref="int"/>).
    /// </summary>
    public const string DirectionalLightShadowNumberOfCascades = "DirectionalLightShadowNumberOfCascades";


    /// <summary>
    /// The cascade split distances of a directional light shadow (<see cref="Vector4"/>).
    /// </summary>
    public const string DirectionalLightShadowCascadeDistances = "DirectionalLightShadowCascadeDistances";


    /// <summary>
    /// The transform matrices of a directional light shadow (array of <see cref="Matrix"/>).
    /// </summary>
    public const string DirectionalLightShadowViewProjections = "DirectionalLightShadowViewProjections";


    /// <summary>
    /// The depth bias of each cascade of a directional light shadow (<see cref="Vector4"/>).
    /// </summary>
    public const string DirectionalLightShadowDepthBias = "DirectionalLightShadowDepthBias";


    /// <summary>
    /// The normal offset of each cascade of a directional light shadow (<see cref="Vector4"/>).
    /// </summary>
    public const string DirectionalLightShadowNormalOffset = "DirectionalLightShadowNormalOffset";


    /// <summary>
    /// The depth bias scale of each cascade of a directional light shadow (<see cref="Vector4"/>).
    /// </summary>
    public const string DirectionalLightShadowDepthBiasScale = "DirectionalLightShadowDepthBiasScale";    // TODO: Remove.


    /// <summary>
    /// The depth bias offset of each cascade of a directional light shadow (<see cref="Vector4"/>).
    /// </summary>
    public const string DirectionalLightShadowDepthBiasOffset = "DirectionalLightShadowDepthBiasOffset";  // TODO: Remove.


    /// <summary>
    /// The shadow map size of a directional light shadow (<see cref="Vector2"/>).
    /// </summary>
    public const string DirectionalLightShadowMapSize = "DirectionalLightShadowMapSize";


    /// <summary>
    /// The filter radius of a directional light shadow (<see cref="float"/>).
    /// </summary>
    public const string DirectionalLightShadowFilterRadius = "DirectionalLightShadowFilterRadius";


    /// <summary>
    /// The jitter resolution (for jitter sampling) of a directional light shadow 
    /// (<see cref="float"/>).
    /// </summary>
    public const string DirectionalLightShadowJitterResolution = "DirectionalLightShadowJitterResolution";


    /// <summary>
    /// The relative range over which directional light shadows are faded out
    /// (<see cref="float"/>).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "FadeOut")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OutRange")]
    public const string DirectionalLightShadowFadeOutRange = "DirectionalLightShadowFadeOutRange";


    /// <summary>
    /// The distance where a directional light shadow starts to fade out (<see cref="float"/>).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string DirectionalLightShadowFadeOutDistance = "DirectionalLightShadowFadeOutDistance";  // TODO: Remove.


    /// <summary>
    /// The maximum distance up to which a directional light shadow is rendered 
    /// (<see cref="float"/>).
    /// </summary>
    public const string DirectionalLightShadowMaxDistance = "DirectionalLightShadowMaxDistance";


    /// <summary>
    /// The shadow factor that is used beyond the maximum distance of a directional light shadow 
    /// (<see cref="float"/>).
    /// </summary>
    public const string DirectionalLightShadowFog = "DirectionalLightShadowFog";


    /// <summary>
    /// The diffuse intensity of a point light (<see cref="Vector3"/> or <see cref="Vector4"/>).
    /// </summary>
    public const string PointLightDiffuse = "PointLightDiffuse";


    /// <summary>
    /// The specular intensity of a point light (<see cref="Vector3"/> or <see cref="Vector4"/>).
    /// </summary>
    public const string PointLightSpecular = "PointLightSpecular";


    /// <summary>
    /// The position of a point light in world space (<see cref="Vector3"/> or 
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string PointLightPosition = "PointLightPosition";


    /// <summary>
    /// The range of a point light (<see cref="float"/>).
    /// </summary>
    public const string PointLightRange = "PointLightRange";


    /// <summary>
    /// The attenuation exponent of a point light (<see cref="float"/>).
    /// </summary>
    public const string PointLightAttenuation = "PointLightAttenuation";


    /// <summary>
    /// The texture of a point light (<see cref="TextureCube"/>).
    /// </summary>
    public const string PointLightTexture = "PointLightTexture";


    /// <summary>
    /// The texture matrix of a point light which converts directions from world
    /// space to the texture space of the light (<see cref="Matrix"/>).
    /// </summary>
    public const string PointLightTextureMatrix = "PointLightTextureMatrix";


    /// <summary>
    /// The diffuse intensity of a spotlight (<see cref="Vector3"/> or <see cref="Vector4"/>).
    /// </summary>
    public const string SpotlightDiffuse = "SpotlightDiffuse";


    /// <summary>
    /// The specular intensity of a spotlight (<see cref="Vector3"/> or <see cref="Vector4"/>).
    /// </summary>
    public const string SpotlightSpecular = "SpotlightSpecular";


    /// <summary>
    /// The position of a spotlight in world space (<see cref="Vector3"/> or 
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string SpotlightPosition = "SpotlightPosition";


    /// <summary>
    /// The direction of a spotlight in world space (<see cref="Vector3"/> or 
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string SpotlightDirection = "SpotlightDirection";


    /// <summary>
    /// The range of a spotlight (<see cref="float"/>).
    /// </summary>
    public const string SpotlightRange = "SpotlightRange";


    /// <summary>
    /// The falloff (umbra) angle of the spotlight in radians (<see cref="float"/>).
    /// </summary>
    /// <seealso cref="Spotlight"/>
    /// <seealso cref="Spotlight.FalloffAngle"/>
    public const string SpotlightFalloffAngle = "SpotlightFalloffAngle";


    /// <summary>
    /// The cutoff (penumbra) angle of the spotlight in radians (<see cref="float"/>).
    /// </summary>
    /// <seealso cref="Spotlight"/>
    /// <seealso cref="Spotlight.CutoffAngle"/>
    public const string SpotlightCutoffAngle = "SpotlightCutoffAngle";


    /// <summary>
    /// The attenuation exponent of a spotlight (<see cref="float"/>).
    /// </summary>
    public const string SpotlightAttenuation = "SpotlightAttenuation";


    /// <summary>
    /// The texture of a spotlight (<see cref="Texture2D"/>).
    /// </summary>
    public const string SpotlightTexture = "SpotlightTexture";


    /// <summary>
    /// The texture matrix of a spotlight which converts positions from world
    /// space to the texture space of the light (<see cref="Matrix"/>).
    /// </summary>
    public const string SpotlightTextureMatrix = "SpotlightTextureMatrix";


    /// <summary>
    /// The diffuse intensity of a projector light (<see cref="Vector3"/> or <see cref="Vector4"/>).
    /// </summary>
    public const string ProjectorLightDiffuse = "ProjectorLightDiffuse";


    /// <summary>
    /// The specular intensity of a projector light (<see cref="Vector3"/> or 
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string ProjectorLightSpecular = "ProjectorLightSpecular";


    /// <summary>
    /// The position of a projector light in world space (<see cref="Vector3"/> or 
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string ProjectorLightPosition = "ProjectorLightPosition";


    /// <summary>
    /// The direction of a projector light in world space (<see cref="Vector3"/> or 
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string ProjectorLightDirection = "ProjectorLightDirection";


    /// <summary>
    /// The range of a projector light (<see cref="float"/>).
    /// </summary>
    public const string ProjectorLightRange = "ProjectorLightRange";


    /// <summary>
    /// The attenuation exponent of a projector light (<see cref="float"/>).
    /// </summary>
    public const string ProjectorLightAttenuation = "ProjectorLightAttenuation";


    /// <summary>
    /// The texture that is projected by the projector light.
    /// </summary>
    public const string ProjectorLightTexture = "ProjectorLightTexture";


    /// <summary>
    /// The view-projection matrix of the projector light.
    /// </summary>
    public const string ProjectorLightViewProjection = "ProjectorLightViewProjection";


    /// <summary>
    /// The texture matrix of a projector light which converts positions from world
    /// space to the texture space of the light (<see cref="Matrix"/>).
    /// </summary>
    public const string ProjectorLightTextureMatrix = "ProjectorLightTextureMatrix";


    // Obsolete: Only kept for backward compatibility.
    /// <summary>
    /// The distance to the near plane of the shadow projection (<see cref="float"/>). (Only valid
    /// during shadow map creation.)
    /// </summary>
    public const string ShadowNear = "ShadowNear";

    // Obsolete: Only kept for backward compatibility.
    /// <summary>
    /// The distance to the far plane of the shadow projection (<see cref="float"/>). (Only valid
    /// during shadow map creation.)
    /// </summary>
    public const string ShadowFar = "ShadowFar";


    /// <summary>
    /// The cube map texture containing the environment (<see cref="TextureCube"/>).
    /// (Environment cube maps in a scene are defined using <see cref="ImageBasedLight"/>s.)
    /// </summary>
    public const string EnvironmentMap = "EnvironmentMap";


    /// <summary>
    /// The side length of one cube map face of the environment map in texels (<see cref="float"/>).
    /// </summary>
    public const string EnvironmentMapSize = "EnvironmentMapSize";


    /// <summary>
    /// The intensity of diffuse environment map reflections (<see cref="Vector3"/> or <see cref="Vector4"/>).
    /// </summary>
    public const string EnvironmentMapDiffuse = "EnvironmentMapDiffuse";


    /// <summary>
    /// The intensity of specular environment map reflections (<see cref="Vector3"/> or <see cref="Vector4"/>).
    /// </summary>
    public const string EnvironmentMapSpecular = "EnvironmentMapSpecular";


    /// <summary>
    /// The max value (see also <see cref="RgbmEncoding.Max"/>) of the RGBM encoding in gamma
    /// space (<see cref="float"/>). If the environment map is encoded using sRGB, this value
    /// is 1. 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rgbm")]
    public const string EnvironmentMapRgbmMax = "EnvironmentMapRgbmMax";

    /// <summary>
    /// The texture matrix of the environment map which converts positions from world
    /// space to the texture space of the cube map (<see cref="Matrix"/>).
    /// </summary>
    public const string EnvironmentMapMatrix = "EnvironmentMapMatrix";
    #endregion


    #region ----- Environment, Fog -----

    /// <summary>
    /// The fog color (RGBA as <see cref="Vector4"/>). 
    /// </summary>
    public const string FogColor = "FogColor";


    /// <summary>
    /// The start distance of the fog (<see cref="float"/>). 
    /// </summary>
    public const string FogStart = "FogStart";


    /// <summary>
    /// The end distance of the fog (<see cref="float"/>).
    /// </summary>
    public const string FogEnd = "FogEnd";


    /// <summary>
    /// The density of the fog (<see cref="float"/>).
    /// </summary>
    public const string FogDensity = "FogDensity";


    /// <summary>
    /// The combined parameters of the fog; a <see cref="Vector4"/> containing:
    /// (start distance, end distance or 1 / density, fog curve exponent, height falloff).
    /// </summary>
    public const string FogParameters = "FogParameters";
    #endregion


    #region ----- Decals -----

    /// <summary>
    /// The opacity of the decal (<see cref="float"/>).
    /// </summary>
    public const string DecalAlpha = "DECALALPHA";


    /// <summary>
    /// The normal threshold of the decal given as cos(α) (<see cref="float"/>).
    /// </summary>
    public const string DecalNormalThreshold = "DECALNORMALTHRESHOLD";


    /// <summary>
    /// The decal options.
    /// </summary>
    public const string DecalOptions = "DECALOPTIONS";


    /// <summary>
    /// The orientation of the decal (= z-axis in world space, <see cref="Vector3"/>).
    /// </summary>
    public const string DecalOrientation = "DECALORIENTATION";
    #endregion


    #region ----- World, View, Projection -----

    /// <summary>
    /// The position of the object in world space (<see cref="Vector3"/> or <see cref="Vector4"/>).
    /// </summary>
    public const string Position = "Position";


    /// <summary>
    /// The world matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string World = "World";


    /// <summary>
    /// The inverse world matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string WorldInverse = "WorldInverse";


    /// <summary>
    /// The transpose of the world matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string WorldTranspose = "WorldTranspose";


    /// <summary>
    /// The transpose of the inverse world matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string WorldInverseTranspose = "WorldInverseTranspose";


    /// <summary>
    /// The view matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string View = "View";


    /// <summary>
    /// The inverse of the view matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string ViewInverse = "ViewInverse";


    /// <summary>
    /// The transpose of the view matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string ViewTranspose = "ViewTranspose";


    /// <summary>
    /// The transpose of the inverse view matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string ViewInverseTranspose = "ViewInverseTranspose";


    /// <summary>
    /// The projection matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string Projection = "Projection";


    /// <summary>
    /// The inverse projection matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string ProjectionInverse = "ProjectionInverse";


    /// <summary>
    /// The transpose of the projection matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string ProjectionTranspose = "ProjectionTranspose";


    /// <summary>
    /// The transpose of the inverse projection matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string ProjectionInverseTranspose = "ProjectionInverseTranspose";


    /// <summary>
    /// The world-view matrix (<see cref="Matrix"/>).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string WorldView = "WorldView";


    /// <summary>
    /// The inverse world-view matrix (<see cref="Matrix"/>).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string WorldViewInverse = "WorldViewInverse";


    /// <summary>
    /// The transpose of the world-view matrix (<see cref="Matrix"/>).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string WorldViewTranspose = "WorldViewTranspose";


    /// <summary>
    /// The transpose of the inverse world-view matrix (<see cref="Matrix"/>).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string WorldViewInverseTranspose = "WorldViewInverseTranspose";


    /// <summary>
    /// The view-projection matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string ViewProjection = "ViewProjection";


    /// <summary>
    /// The inverse view-projection matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string ViewProjectionInverse = "ViewProjectionInverse";


    /// <summary>
    /// The transpose of the view-projection matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string ViewProjectionTranspose = "ViewProjectionTranspose";


    /// <summary>
    /// The transpose of the inverse view-projection matrix (<see cref="Matrix"/>).
    /// </summary>
    public const string ViewProjectionInverseTranspose = "ViewProjectionInverseTranspose";


    /// <summary>
    /// The world-view-projection matrix (<see cref="Matrix"/>).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string WorldViewProjection = "WorldViewProjection";


    /// <summary>
    /// The inverse world-view-projection matrix (<see cref="Matrix"/>).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string WorldViewProjectionInverse = "WorldViewProjectionInverse";


    /// <summary>
    /// The transpose of the world-view-projection matrix (<see cref="Matrix"/>).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string WorldViewProjectionTranspose = "WorldViewProjectionTranspose";


    /// <summary>
    /// The transpose of the inverse world-view-projection matrix (<see cref="Matrix"/>).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string WorldViewProjectionInverseTranspose = "WorldViewProjectionInverseTranspose";


    /// <summary>
    /// Same as <see cref="World"/>, except that the matrix does not contain any scale factors.
    /// </summary>
    public const string UnscaledWorld = "UnscaledWorld";


    /// <summary>
    /// Same as <see cref="WorldView"/>, except that the matrix does not contain any scale factors.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string UnscaledWorldView = "UnscaledWorldView";
    #endregion


    #region ----- Last World, View, Projection -----

    /// <summary>
    /// The position of the object in world space (<see cref="Vector3"/> or <see cref="Vector4"/>)
    /// of the last frame.
    /// </summary>
    public const string LastPosition = "LastPosition";


    /// <summary>
    /// The world matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastWorld = "LastWorld";


    /// <summary>
    /// The inverse world matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastWorldInverse = "LastWorldInverse";


    /// <summary>
    /// The transpose of the world matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastWorldTranspose = "LastWorldTranspose";


    /// <summary>
    /// The transpose of the inverse world matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastWorldInverseTranspose = "LastWorldInverseTranspose";


    /// <summary>
    /// The view matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastView = "LastView";


    /// <summary>
    /// The inverse view matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastViewInverse = "LastViewInverse";


    /// <summary>
    /// The transpose of the view matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastViewTranspose = "LastViewTranspose";


    /// <summary>
    /// The transpose of the inverse view matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastViewInverseTranspose = "LastViewInverseTranspose";


    /// <summary>
    /// The projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastProjection = "LastProjection";


    /// <summary>
    /// The inverse projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastProjectionInverse = "LastProjectionInverse";


    /// <summary>
    /// The transpose of the projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastProjectionTranspose = "LastProjectionTranspose";


    /// <summary>
    /// The transpose of the inverse projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastProjectionInverseTranspose = "LastProjectionInverseTranspose";


    /// <summary>
    /// The world-view matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string LastWorldView = "LastWorldView";


    /// <summary>
    /// The inverse world-view matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string LastWorldViewInverse = "LastWorldViewInverse";


    /// <summary>
    /// The transpose of the world-view matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string LastWorldViewTranspose = "LastWorldViewTranspose";


    /// <summary>
    /// The transpose of the inverse world-view matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string LastWorldViewInverseTranspose = "LastWorldViewInverseTranspose";


    /// <summary>
    /// The view-projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastViewProjection = "LastViewProjection";


    /// <summary>
    /// The inverse view-projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastViewProjectionInverse = "LastViewProjectionInverse";


    /// <summary>
    /// The transpose of the view-projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastViewProjectionTranspose = "LastViewProjectionTranspose";


    /// <summary>
    /// The transpose of the inverse view-projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    public const string LastViewProjectionInverseTranspose = "LastViewProjectionInverseTranspose";


    /// <summary>
    /// The world-view-projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string LastWorldViewProjection = "LastWorldViewProjection";


    /// <summary>
    /// The inverse world-view-projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string LastWorldViewProjectionInverse = "LastWorldViewProjectionInverse";


    /// <summary>
    /// The transpose of the world-view-projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string LastWorldViewProjectionTranspose = "LastWorldViewProjectionTranspose";


    /// <summary>
    /// The transpose of the inverse world-view-projection matrix (<see cref="Matrix"/>) of the last frame.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public const string LastWorldViewProjectionInverseTranspose = "LastWorldViewProjectionInverseTranspose";
    #endregion
  }
}
