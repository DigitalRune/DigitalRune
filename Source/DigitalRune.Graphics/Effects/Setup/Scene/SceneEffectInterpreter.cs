// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Graphics.SceneGraph;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides the descriptions for effects used in a <see cref="IScene"/>.
  /// </summary>
  /// <remarks>
  /// See <see cref="SceneEffectParameterSemantics"/> for a list of supported semantics.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
  public class SceneEffectInterpreter : DictionaryEffectInterpreter
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SceneEffectInterpreter"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    public SceneEffectInterpreter()
    {
      // Bounding Shapes
      //ParameterDescriptions.Add(SceneEffectParameterSemantics.BoundingBoxMax,       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingBoxMax, i, EffectParameterHint.PerInstance));
      //ParameterDescriptions.Add(SceneEffectParameterSemantics.BoundingBoxMin,       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingBoxMin, i, EffectParameterHint.PerInstance));
      //ParameterDescriptions.Add(SceneEffectParameterSemantics.BoundingBoxSize,      (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingBoxSize, i, EffectParameterHint.PerInstance));
      //ParameterDescriptions.Add(SceneEffectParameterSemantics.BoundingCenter,       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingCenter, i, EffectParameterHint.PerInstance));
      //ParameterDescriptions.Add(SceneEffectParameterSemantics.BoundingSphereRadius, (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingSphereRadius, i, EffectParameterHint.PerInstance));
      
      // Camera
      ParameterDescriptions.Add(SceneEffectParameterSemantics.CameraDirection,      (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.CameraDirection, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.CameraPosition,       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.CameraPosition, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastCameraDirection,  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastCameraDirection, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastCameraPosition,   (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastCameraPosition, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.CameraNear,           (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.CameraNear, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.CameraFar,            (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.CameraFar, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LodCameraPosition,    (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LodCameraPosition, i, EffectParameterHint.Global));
      
      // Lights
      ParameterDescriptions.Add(SceneEffectParameterSemantics.AmbientLight,                  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.AmbientLight, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.AmbientLightAttenuation,       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.AmbientLightAttenuation, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.AmbientLightUp,                (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.AmbientLightUp, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightDiffuse,       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightDiffuse, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightSpecular,      (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightSpecular, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightDirection,     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightDirection, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightTexture,       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightTexture, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightTextureOffset, (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightTextureOffset, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightTextureScale,  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightTextureScale, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightTextureMatrix, (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightTextureMatrix, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowMap,     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowMap, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowParameters,       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowParameters, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowNumberOfCascades, (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowNumberOfCascades, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowCascadeDistances, (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowCascadeDistances, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowViewProjections,  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowViewProjections, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowDepthBias,        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowDepthBias, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowNormalOffset,     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowNormalOffset, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowDepthBiasScale,   (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowDepthBiasScale, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowDepthBiasOffset,  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowDepthBiasOffset, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowMapSize,          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowMapSize, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowFilterRadius,     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowFilterRadius, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowJitterResolution, (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowJitterResolution, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowFadeOutRange,     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowFadeOutRange, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowFadeOutDistance,  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowFadeOutDistance, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowMaxDistance,      (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowMaxDistance, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DirectionalLightShadowFog,              (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightShadowFog, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.PointLightDiffuse,             (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.PointLightDiffuse, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.PointLightSpecular,            (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.PointLightSpecular, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.PointLightPosition,            (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.PointLightPosition, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.PointLightRange,               (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.PointLightRange, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.PointLightAttenuation,         (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.PointLightAttenuation, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.PointLightTexture,             (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.PointLightTexture, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.PointLightTextureMatrix,       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.PointLightTextureMatrix, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.SpotlightDiffuse,              (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SpotlightDiffuse, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.SpotlightSpecular,             (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SpotlightSpecular, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.SpotlightPosition,             (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SpotlightPosition, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.SpotlightDirection,            (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SpotlightDirection, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.SpotlightRange,                (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SpotlightRange, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.SpotlightFalloffAngle,         (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SpotlightFalloffAngle, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.SpotlightCutoffAngle,          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SpotlightCutoffAngle, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.SpotlightAttenuation,          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SpotlightAttenuation, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.SpotlightTexture,              (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SpotlightTexture, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.SpotlightTextureMatrix,        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SpotlightTextureMatrix, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectorLightDiffuse,         (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectorLightDiffuse, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectorLightSpecular,        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectorLightSpecular, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectorLightPosition,        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectorLightPosition, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectorLightDirection,       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectorLightDirection, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectorLightRange,           (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectorLightRange, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectorLightAttenuation,     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectorLightAttenuation, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectorLightTexture,         (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectorLightTexture, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectorLightViewProjection,  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectorLightViewProjection, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectorLightTextureMatrix,   (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectorLightTextureMatrix, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.EnvironmentMap,                (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.EnvironmentMap, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.EnvironmentMapSize,            (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.EnvironmentMapSize, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.EnvironmentMapDiffuse,         (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.EnvironmentMapDiffuse, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.EnvironmentMapSpecular,        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.EnvironmentMapSpecular, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.EnvironmentMapRgbmMax,         (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.EnvironmentMapRgbmMax, i, EffectParameterHint.Local));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.EnvironmentMapMatrix,          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.EnvironmentMapMatrix, i, EffectParameterHint.Local));

      // Obsolete
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ShadowNear,                    (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ShadowNear, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ShadowFar,                     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ShadowFar, i, EffectParameterHint.Global));

      // Environment, Fog
      ParameterDescriptions.Add(SceneEffectParameterSemantics.FogColor,               (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.FogColor, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.FogStart,               (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.FogStart, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.FogEnd,                 (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.FogEnd, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.FogDensity,             (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.FogDensity, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.FogParameters,          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.FogParameters, i, EffectParameterHint.Global));
      
      // Decals
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DecalAlpha,           (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DecalAlpha, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DecalNormalThreshold, (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DecalNormalThreshold, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DecalOptions,         (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DecalOptions, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.DecalOrientation,     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DecalOrientation, i, EffectParameterHint.PerInstance));

      // World, View, Projection
      ParameterDescriptions.Add(SceneEffectParameterSemantics.Position,                            (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.Position, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.World,                               (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.World, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.WorldInverse,                        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldInverse, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.WorldTranspose,                      (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldTranspose, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.WorldInverseTranspose,               (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldInverseTranspose, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.View,                                (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.View, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ViewInverse,                         (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewInverse, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ViewTranspose,                       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ViewInverseTranspose,                (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewInverseTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.Projection,                          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.Projection, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectionInverse,                   (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectionInverse, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectionTranspose,                 (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectionTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ProjectionInverseTranspose,          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectionInverseTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.WorldView,                           (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldView, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.WorldViewInverse,                    (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewInverse, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.WorldViewTranspose,                  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewTranspose, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.WorldViewInverseTranspose,           (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewInverseTranspose, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ViewProjection,                      (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewProjection, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ViewProjectionInverse,               (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewProjectionInverse, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ViewProjectionTranspose,             (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewProjectionTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.ViewProjectionInverseTranspose,      (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewProjectionInverseTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.WorldViewProjection,                 (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewProjection, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.WorldViewProjectionInverse,          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewProjectionInverse, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.WorldViewProjectionTranspose,        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewProjectionTranspose, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.WorldViewProjectionInverseTranspose, (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewProjectionInverseTranspose, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.UnscaledWorld,                       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.UnscaledWorld, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.UnscaledWorldView,                   (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.UnscaledWorldView, i, EffectParameterHint.PerInstance));

      // Last World, View, Projection
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastPosition,                            (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastPosition, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorld,                               (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorld, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorldInverse,                        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorldInverse, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorldTranspose,                      (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorldTranspose, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorldInverseTranspose,               (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorldInverseTranspose, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastView,                                (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastView, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastViewInverse,                         (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastViewInverse, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastViewTranspose,                       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastViewTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastViewInverseTranspose,                (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastViewInverseTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastProjection,                          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastProjection, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastProjectionInverse,                   (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastProjectionInverse, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastProjectionTranspose,                 (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastProjectionTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastProjectionInverseTranspose,          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastProjectionInverseTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorldView,                           (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorldView, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorldViewInverse,                    (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorldViewInverse, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorldViewTranspose,                  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorldViewTranspose, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorldViewInverseTranspose,           (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorldViewInverseTranspose, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastViewProjection,                      (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastViewProjection, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastViewProjectionInverse,               (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastViewProjectionInverse, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastViewProjectionTranspose,             (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastViewProjectionTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastViewProjectionInverseTranspose,      (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastViewProjectionInverseTranspose, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorldViewProjection,                 (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorldViewProjection, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorldViewProjectionInverse,          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorldViewProjectionInverse, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorldViewProjectionTranspose,        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorldViewProjectionTranspose, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(SceneEffectParameterSemantics.LastWorldViewProjectionInverseTranspose, (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.LastWorldViewProjectionInverseTranspose, i, EffectParameterHint.PerInstance));

      // Special
      ParameterDescriptions.Add(SceneEffectParameterSemantics.SceneNodeType, (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SceneNodeType, i, EffectParameterHint.PerInstance));
    }
  }
}
