// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides effect bindings for rendering a <see cref="IScene"/>.
  /// </summary>
  public class SceneEffectBinder : DictionaryEffectBinder
  {
    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="SceneEffectBinder"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    static SceneEffectBinder()
    {
      // Initialize default values.
      EmptyLightQuery = new LightQuery();

      DefaultLightColor3 = new Vector3(0, 0, 0);
      DefaultLightColor4 = new Vector4(0, 0, 0, 1);
      DefaultLightDirection3 = new Vector3(0, -1, 0);
      DefaultLightDirection4 = new Vector4(0, -1, 0, 0);
      DefaultLightUp3 = new Vector3(0, 1, 0);
      DefaultLightUp4 = new Vector4(0, 1, 0, 0);
      DefaultLightPosition3 = new Vector3(0, 0, 0);
      DefaultLightPosition4 = new Vector4(0, 0, 0, 1);

      var ambientLight = new AmbientLight();
      DefaultAmbientLightAttenuation = ambientLight.HemisphericAttenuation;

      var directionalLight = new DirectionalLight();
      DefaultDirectionalLightTextureOffset = (Vector2)directionalLight.TextureOffset;
      DefaultDirectionalLightTextureScale = (Vector2)directionalLight.TextureScale;

      var pointLight = new PointLight();
      DefaultPointLightRange = pointLight.Range;
      DefaultPointLightAttenuation = pointLight.Attenuation;

      var spotlight = new Spotlight();
      DefaultSpotlightRange = spotlight.Range;
      DefaultSpotlightFalloffAngle = spotlight.FalloffAngle;
      DefaultSpotlightCutoffAngle = spotlight.CutoffAngle;
      DefaultSpotlightAttenuation = spotlight.Attenuation;

      var projectorLight = new ProjectorLight();
      DefaultProjectorLightRange = projectorLight.Projection.Far;
      DefaultProjectorLightAttenuation = projectorLight.Attenuation;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SceneEffectBinder"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    public SceneEffectBinder()
    {
      var d = BoolBindings;
      d.Add(SceneEffectParameterSemantics.DecalOptions, (e, p, o) => CreateDelegateParameterBinding<bool>(e, p, GetDecalOptions));

      d = Int32Bindings;
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowNumberOfCascades, (e, p, o) => CreateDelegateParameterBinding<int>(e, p, GetDirectionalLightShadowNumberOfCascades));

      d = SingleBindings;
      d.Add(SceneEffectParameterSemantics.CameraNear, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetCameraNear));
      d.Add(SceneEffectParameterSemantics.CameraFar, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetCameraFar));
      d.Add(SceneEffectParameterSemantics.ShadowNear, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetShadowNear));
      d.Add(SceneEffectParameterSemantics.ShadowFar, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetCameraFar)); // ShadowFar = CameraFar
      d.Add(SceneEffectParameterSemantics.AmbientLightAttenuation, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetAmbientLightAttenuation));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowFilterRadius, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDirectionalLightShadowFilterRadius));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowJitterResolution, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDirectionalLightShadowJitterResolution));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowFadeOutRange, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDirectionalLightShadowFadeOutRange));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowFadeOutDistance, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDirectionalLightShadowFadeOutDistance));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowMaxDistance, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDirectionalLightShadowMaxDistance));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowFog, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDirectionalLightShadowFog));
      d.Add(SceneEffectParameterSemantics.PointLightRange, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetPointLightRange));
      d.Add(SceneEffectParameterSemantics.PointLightAttenuation, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetPointLightAttenuation));
      d.Add(SceneEffectParameterSemantics.SpotlightRange, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetSpotlightRange));
      d.Add(SceneEffectParameterSemantics.SpotlightFalloffAngle, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetSpotlightFalloffAngle));
      d.Add(SceneEffectParameterSemantics.SpotlightCutoffAngle, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetSpotlightCutoffAngle));
      d.Add(SceneEffectParameterSemantics.SpotlightAttenuation, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetSpotlightAttenuation));
      d.Add(SceneEffectParameterSemantics.ProjectorLightRange, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetProjectorLightRange));
      d.Add(SceneEffectParameterSemantics.ProjectorLightAttenuation, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetProjectorLightAttenuation));
      d.Add(SceneEffectParameterSemantics.EnvironmentMapSize, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetImageBasedLightTextureSize));
      d.Add(SceneEffectParameterSemantics.EnvironmentMapRgbmMax, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetImageBasedLightRgbmMax));
      d.Add(SceneEffectParameterSemantics.FogStart, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetFogStart));
      d.Add(SceneEffectParameterSemantics.FogEnd, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetFogEnd));
      d.Add(SceneEffectParameterSemantics.FogDensity, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetFogDensity));
      d.Add(SceneEffectParameterSemantics.SceneNodeType, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetSceneNodeType));
      d.Add(SceneEffectParameterSemantics.DecalAlpha, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDecalAlpha));
      d.Add(SceneEffectParameterSemantics.DecalNormalThreshold, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDecalNormalThreshold));

      d = SingleArrayBindings;
      d.Add(SceneEffectParameterSemantics.AmbientLightAttenuation, (e, p, o) => CreateDelegateParameterArrayBinding<float>(e, p, GetAmbientLightAttenuationArray));
      d.Add(SceneEffectParameterSemantics.PointLightRange, (e, p, o) => CreateDelegateParameterArrayBinding<float>(e, p, GetPointLightRangeArray));
      d.Add(SceneEffectParameterSemantics.PointLightAttenuation, (e, p, o) => CreateDelegateParameterArrayBinding<float>(e, p, GetPointLightAttenuationArray));
      d.Add(SceneEffectParameterSemantics.SpotlightRange, (e, p, o) => CreateDelegateParameterArrayBinding<float>(e, p, GetSpotlightRangeArray));
      d.Add(SceneEffectParameterSemantics.SpotlightFalloffAngle, (e, p, o) => CreateDelegateParameterArrayBinding<float>(e, p, GetSpotlightFalloffAngleArray));
      d.Add(SceneEffectParameterSemantics.SpotlightCutoffAngle, (e, p, o) => CreateDelegateParameterArrayBinding<float>(e, p, GetSpotlightCutoffAngleArray));
      d.Add(SceneEffectParameterSemantics.SpotlightAttenuation, (e, p, o) => CreateDelegateParameterArrayBinding<float>(e, p, GetSpotlightAttenuationArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightRange, (e, p, o) => CreateDelegateParameterArrayBinding<float>(e, p, GetProjectorLightRangeArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightAttenuation, (e, p, o) => CreateDelegateParameterArrayBinding<float>(e, p, GetProjectorLightAttenuationArray));

      d = MatrixBindings;
      d.Add(SceneEffectParameterSemantics.DirectionalLightTextureMatrix, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetDirectionalLightTextureMatrix));
      d.Add(SceneEffectParameterSemantics.PointLightTextureMatrix, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetPointLightTextureMatrix));
      d.Add(SceneEffectParameterSemantics.SpotlightTextureMatrix, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetSpotlightTextureMatrix));
      d.Add(SceneEffectParameterSemantics.ProjectorLightViewProjection, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetProjectorLightViewProjection));
      d.Add(SceneEffectParameterSemantics.ProjectorLightTextureMatrix, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetProjectorLightTextureMatrix));
      d.Add(SceneEffectParameterSemantics.EnvironmentMapMatrix, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetImageBasedLightTextureMatrix));

      d.Add(SceneEffectParameterSemantics.World, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorld));
      d.Add(SceneEffectParameterSemantics.WorldInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorldInverse));
      d.Add(SceneEffectParameterSemantics.WorldTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorldTranspose));
      d.Add(SceneEffectParameterSemantics.WorldInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorldInverseTranspose));
      d.Add(SceneEffectParameterSemantics.View, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetView));
      d.Add(SceneEffectParameterSemantics.ViewInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetViewInverse));
      d.Add(SceneEffectParameterSemantics.ViewTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetViewTranspose));
      d.Add(SceneEffectParameterSemantics.ViewInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetViewInverseTranspose));
      d.Add(SceneEffectParameterSemantics.Projection, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetProjection));
      d.Add(SceneEffectParameterSemantics.ProjectionInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetProjectionInverse));
      d.Add(SceneEffectParameterSemantics.ProjectionTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetProjectionTranspose));
      d.Add(SceneEffectParameterSemantics.ProjectionInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetProjectionInverseTranspose));
      d.Add(SceneEffectParameterSemantics.WorldView, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorldView));
      d.Add(SceneEffectParameterSemantics.WorldViewInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorldViewInverse));
      d.Add(SceneEffectParameterSemantics.WorldViewTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorldViewTranspose));
      d.Add(SceneEffectParameterSemantics.WorldViewInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorldViewInverseTranspose));
      d.Add(SceneEffectParameterSemantics.ViewProjection, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetViewProjection));
      d.Add(SceneEffectParameterSemantics.ViewProjectionInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetViewProjectionInverse));
      d.Add(SceneEffectParameterSemantics.ViewProjectionTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetViewProjectionTranspose));
      d.Add(SceneEffectParameterSemantics.ViewProjectionInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetViewProjectionInverseTranspose));
      d.Add(SceneEffectParameterSemantics.WorldViewProjection, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorldViewProjection));
      d.Add(SceneEffectParameterSemantics.WorldViewProjectionInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorldViewProjectionInverse));
      d.Add(SceneEffectParameterSemantics.WorldViewProjectionTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorldViewProjectionTranspose));
      d.Add(SceneEffectParameterSemantics.WorldViewProjectionInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetWorldViewProjectionInverseTranspose));
      d.Add(SceneEffectParameterSemantics.UnscaledWorld, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetUnscaledWorld));
      d.Add(SceneEffectParameterSemantics.UnscaledWorldView, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetUnscaledWorldView));

      d.Add(SceneEffectParameterSemantics.LastWorld, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorld));
      d.Add(SceneEffectParameterSemantics.LastWorldInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorldInverse));
      d.Add(SceneEffectParameterSemantics.LastWorldTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorldTranspose));
      d.Add(SceneEffectParameterSemantics.LastWorldInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorldInverseTranspose));
      d.Add(SceneEffectParameterSemantics.LastView, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastView));
      d.Add(SceneEffectParameterSemantics.LastViewInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastViewInverse));
      d.Add(SceneEffectParameterSemantics.LastViewTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastViewTranspose));
      d.Add(SceneEffectParameterSemantics.LastViewInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastViewInverseTranspose));
      d.Add(SceneEffectParameterSemantics.LastProjection, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastProjection));
      d.Add(SceneEffectParameterSemantics.LastProjectionInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastProjectionInverse));
      d.Add(SceneEffectParameterSemantics.LastProjectionTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastProjectionTranspose));
      d.Add(SceneEffectParameterSemantics.LastProjectionInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastProjectionInverseTranspose));
      d.Add(SceneEffectParameterSemantics.LastWorldView, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorldView));
      d.Add(SceneEffectParameterSemantics.LastWorldViewInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorldViewInverse));
      d.Add(SceneEffectParameterSemantics.LastWorldViewTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorldViewTranspose));
      d.Add(SceneEffectParameterSemantics.LastWorldViewInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorldViewInverseTranspose));
      d.Add(SceneEffectParameterSemantics.LastViewProjection, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastViewProjection));
      d.Add(SceneEffectParameterSemantics.LastViewProjectionInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastViewProjectionInverse));
      d.Add(SceneEffectParameterSemantics.LastViewProjectionTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastViewProjectionTranspose));
      d.Add(SceneEffectParameterSemantics.LastViewProjectionInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastViewProjectionInverseTranspose));
      d.Add(SceneEffectParameterSemantics.LastWorldViewProjection, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorldViewProjection));
      d.Add(SceneEffectParameterSemantics.LastWorldViewProjectionInverse, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorldViewProjectionInverse));
      d.Add(SceneEffectParameterSemantics.LastWorldViewProjectionTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorldViewProjectionTranspose));
      d.Add(SceneEffectParameterSemantics.LastWorldViewProjectionInverseTranspose, (e, p, o) => CreateDelegateParameterBinding<Matrix>(e, p, GetLastWorldViewProjectionInverseTranspose));

      d = MatrixArrayBindings;
      d.Add(SceneEffectParameterSemantics.DirectionalLightTextureMatrix, (e, p, o) => CreateDelegateParameterArrayBinding<Matrix>(e, p, GetDirectionalLightTextureMatrixArray));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowViewProjections, (e, p, o) => CreateDelegateParameterArrayBinding<Matrix>(e, p, GetDirectionalLightShadowViewProjections));
      d.Add(SceneEffectParameterSemantics.PointLightTextureMatrix, (e, p, o) => CreateDelegateParameterArrayBinding<Matrix>(e, p, GetPointLightTextureMatrixArray));
      d.Add(SceneEffectParameterSemantics.SpotlightTextureMatrix, (e, p, o) => CreateDelegateParameterArrayBinding<Matrix>(e, p, GetSpotlightTextureMatrixArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightViewProjection, (e, p, o) => CreateDelegateParameterArrayBinding<Matrix>(e, p, GetProjectorLightViewProjectionArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightTextureMatrix, (e, p, o) => CreateDelegateParameterArrayBinding<Matrix>(e, p, GetProjectorLightTextureMatrixArray));

      d = Vector2Bindings;
      d.Add(SceneEffectParameterSemantics.DirectionalLightTextureOffset, (e, p, o) => CreateDelegateParameterBinding<Vector2>(e, p, GetDirectionalLightTextureOffset));
      d.Add(SceneEffectParameterSemantics.DirectionalLightTextureScale, (e, p, o) => CreateDelegateParameterBinding<Vector2>(e, p, GetDirectionalLightTextureScale));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowMapSize, (e, p, o) => CreateDelegateParameterBinding<Vector2>(e, p, GetDirectionalLightShadowMapSize));

      d = Vector2ArrayBindings;
      d.Add(SceneEffectParameterSemantics.DirectionalLightTextureOffset, (e, p, o) => CreateDelegateParameterArrayBinding<Vector2>(e, p, GetDirectionalLightTextureOffsetArray));
      d.Add(SceneEffectParameterSemantics.DirectionalLightTextureScale, (e, p, o) => CreateDelegateParameterArrayBinding<Vector2>(e, p, GetDirectionalLightTextureScaleArray));

      d = Vector3Bindings;
      d.Add(SceneEffectParameterSemantics.CameraDirection, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetCameraDirection));
      d.Add(SceneEffectParameterSemantics.CameraPosition, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetCameraPosition));
      d.Add(SceneEffectParameterSemantics.LastCameraDirection, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetLastCameraDirection));
      d.Add(SceneEffectParameterSemantics.LastCameraPosition, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetLastCameraPosition));
      d.Add(SceneEffectParameterSemantics.LodCameraPosition, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetLodCameraPosition));

      d.Add(SceneEffectParameterSemantics.AmbientLight, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetAmbientLight));
      d.Add(SceneEffectParameterSemantics.AmbientLightUp, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetAmbientLightUp));
      d.Add(SceneEffectParameterSemantics.DirectionalLightDiffuse, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetDirectionalLightDiffuse));
      d.Add(SceneEffectParameterSemantics.DirectionalLightSpecular, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetDirectionalLightSpecular));
      d.Add(SceneEffectParameterSemantics.DirectionalLightDirection, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetDirectionalLightDirection));
      d.Add(SceneEffectParameterSemantics.PointLightDiffuse, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetPointLightDiffuse));
      d.Add(SceneEffectParameterSemantics.PointLightSpecular, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetPointLightSpecular));
      d.Add(SceneEffectParameterSemantics.PointLightPosition, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetPointLightPosition));
      d.Add(SceneEffectParameterSemantics.SpotlightDiffuse, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetSpotlightDiffuse));
      d.Add(SceneEffectParameterSemantics.SpotlightSpecular, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetSpotlightSpecular));
      d.Add(SceneEffectParameterSemantics.SpotlightPosition, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetSpotlightPosition));
      d.Add(SceneEffectParameterSemantics.SpotlightDirection, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetSpotlightDirection));
      d.Add(SceneEffectParameterSemantics.ProjectorLightDiffuse, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetProjectorLightDiffuse));
      d.Add(SceneEffectParameterSemantics.ProjectorLightSpecular, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetProjectorLightSpecular));
      d.Add(SceneEffectParameterSemantics.ProjectorLightPosition, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetProjectorLightPosition));
      d.Add(SceneEffectParameterSemantics.ProjectorLightDirection, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetProjectorLightDirection));
      d.Add(SceneEffectParameterSemantics.EnvironmentMapDiffuse, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetImageBasedLightDiffuse));
      d.Add(SceneEffectParameterSemantics.EnvironmentMapSpecular, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetImageBasedLightSpecular));
      d.Add(SceneEffectParameterSemantics.FogColor, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetFogColor));
      d.Add(SceneEffectParameterSemantics.DecalOrientation, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetDecalOrientation));

      d = Vector3ArrayBindings;
      d.Add(SceneEffectParameterSemantics.AmbientLight, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetAmbientLightArray));
      d.Add(SceneEffectParameterSemantics.AmbientLightUp, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetAmbientLightUpArray));
      d.Add(SceneEffectParameterSemantics.DirectionalLightDiffuse, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetDirectionalLightDiffuseArray));
      d.Add(SceneEffectParameterSemantics.DirectionalLightSpecular, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetDirectionalLightSpecularArray));
      d.Add(SceneEffectParameterSemantics.DirectionalLightDirection, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetDirectionalLightDirectionArray));
      d.Add(SceneEffectParameterSemantics.PointLightDiffuse, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetPointLightDiffuseArray));
      d.Add(SceneEffectParameterSemantics.PointLightSpecular, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetPointLightSpecularArray));
      d.Add(SceneEffectParameterSemantics.PointLightPosition, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetPointLightPositionArray));
      d.Add(SceneEffectParameterSemantics.SpotlightDiffuse, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetSpotlightDiffuseArray));
      d.Add(SceneEffectParameterSemantics.SpotlightSpecular, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetSpotlightSpecularArray));
      d.Add(SceneEffectParameterSemantics.SpotlightPosition, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetSpotlightPositionArray));
      d.Add(SceneEffectParameterSemantics.SpotlightDirection, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetSpotlightDirectionArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightDiffuse, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetProjectorLightDiffuseArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightSpecular, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetProjectorLightSpecularArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightPosition, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetProjectorLightPositionArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightDirection, (e, p, o) => CreateDelegateParameterArrayBinding<Vector3>(e, p, GetProjectorLightDirectionArray));

      d = Vector4Bindings;
      d.Add(SceneEffectParameterSemantics.CameraDirection, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetCameraDirection));
      d.Add(SceneEffectParameterSemantics.CameraPosition, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetCameraPosition));
      d.Add(SceneEffectParameterSemantics.LastCameraDirection, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetLastCameraDirection));
      d.Add(SceneEffectParameterSemantics.LastCameraPosition, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetLastCameraPosition));

      d.Add(SceneEffectParameterSemantics.AmbientLight, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetAmbientLight));
      d.Add(SceneEffectParameterSemantics.AmbientLightUp, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetAmbientLightUp));
      d.Add(SceneEffectParameterSemantics.DirectionalLightDiffuse, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetDirectionalLightDiffuse));
      d.Add(SceneEffectParameterSemantics.DirectionalLightSpecular, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetDirectionalLightSpecular));
      d.Add(SceneEffectParameterSemantics.DirectionalLightDirection, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetDirectionalLightDirection));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowCascadeDistances, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetDirectionalLightShadowCascadeDistances));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowDepthBias, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetDirectionalLightShadowDepthBias));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowNormalOffset, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetDirectionalLightShadowNormalOffset));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowDepthBiasScale, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetDirectionalLightShadowDepthBiasScale));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowDepthBiasOffset, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetDirectionalLightShadowDepthBiasOffset));
      /* Obsolete */d.Add(SceneEffectParameterSemantics.DirectionalLightShadowFilterRadius, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetDirectionalLightShadowFilterRadius));
      /* Obsolete */d.Add(SceneEffectParameterSemantics.DirectionalLightShadowJitterResolution, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetDirectionalLightShadowJitterResolution));
      d.Add(SceneEffectParameterSemantics.PointLightDiffuse, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetPointLightDiffuse));
      d.Add(SceneEffectParameterSemantics.PointLightSpecular, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetPointLightSpecular));
      d.Add(SceneEffectParameterSemantics.PointLightPosition, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetPointLightPosition));
      d.Add(SceneEffectParameterSemantics.SpotlightDiffuse, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetSpotlightDiffuse));
      d.Add(SceneEffectParameterSemantics.SpotlightSpecular, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetSpotlightSpecular));
      d.Add(SceneEffectParameterSemantics.SpotlightPosition, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetSpotlightPosition));
      d.Add(SceneEffectParameterSemantics.SpotlightDirection, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetSpotlightDirection));
      d.Add(SceneEffectParameterSemantics.ProjectorLightDiffuse, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetProjectorLightDiffuse));
      d.Add(SceneEffectParameterSemantics.ProjectorLightSpecular, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetProjectorLightSpecular));
      d.Add(SceneEffectParameterSemantics.ProjectorLightPosition, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetProjectorLightPosition));
      d.Add(SceneEffectParameterSemantics.ProjectorLightDirection, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetProjectorLightDirection));
      d.Add(SceneEffectParameterSemantics.EnvironmentMapDiffuse, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetImageBasedLightDiffuse));
      d.Add(SceneEffectParameterSemantics.EnvironmentMapSpecular, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetImageBasedLightSpecular));
      d.Add(SceneEffectParameterSemantics.FogColor, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetFogColor));
      d.Add(SceneEffectParameterSemantics.FogParameters, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetFogParameters));

      d = Vector4ArrayBindings;
      d.Add(SceneEffectParameterSemantics.AmbientLight, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetAmbientLightArray));
      d.Add(SceneEffectParameterSemantics.AmbientLightUp, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetAmbientLightUpArray));
      d.Add(SceneEffectParameterSemantics.DirectionalLightDiffuse, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetDirectionalLightDiffuseArray));
      d.Add(SceneEffectParameterSemantics.DirectionalLightSpecular, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetDirectionalLightSpecularArray));
      d.Add(SceneEffectParameterSemantics.DirectionalLightDirection, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetDirectionalLightDirectionArray));
      d.Add(SceneEffectParameterSemantics.PointLightDiffuse, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetPointLightDiffuseArray));
      d.Add(SceneEffectParameterSemantics.PointLightSpecular, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetPointLightSpecularArray));
      d.Add(SceneEffectParameterSemantics.PointLightPosition, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetPointLightPositionArray));
      d.Add(SceneEffectParameterSemantics.SpotlightDiffuse, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetSpotlightDiffuseArray));
      d.Add(SceneEffectParameterSemantics.SpotlightSpecular, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetSpotlightSpecularArray));
      d.Add(SceneEffectParameterSemantics.SpotlightPosition, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetSpotlightPositionArray));
      d.Add(SceneEffectParameterSemantics.SpotlightDirection, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetSpotlightDirectionArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightDiffuse, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetProjectorLightDiffuseArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightSpecular, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetProjectorLightSpecularArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightPosition, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetProjectorLightPositionArray));
      d.Add(SceneEffectParameterSemantics.ProjectorLightDirection, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetProjectorLightDirectionArray));

      d = TextureBindings;
      d.Add(SceneEffectParameterSemantics.DirectionalLightTexture, (e, p, o) => CreateDelegateParameterBinding<Texture>(e, p, GetDirectionalLightTexture));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowMap, (e, p, o) => CreateDelegateParameterBinding<Texture>(e, p, GetDirectionalLightShadowMap));
      d.Add(SceneEffectParameterSemantics.PointLightTexture, (e, p, o) => CreateDelegateParameterBinding<Texture>(e, p, GetPointLightTexture));
      d.Add(SceneEffectParameterSemantics.SpotlightTexture, (e, p, o) => CreateDelegateParameterBinding<Texture>(e, p, GetSpotlightTexture));
      d.Add(SceneEffectParameterSemantics.ProjectorLightTexture, (e, p, o) => CreateDelegateParameterBinding<Texture>(e, p, GetProjectorLightTexture));
      d.Add(SceneEffectParameterSemantics.EnvironmentMap, (e, p, o) => CreateDelegateParameterBinding<Texture>(e, p, GetImageBasedLightTexture));

      d = Texture2DBindings;
      d.Add(SceneEffectParameterSemantics.DirectionalLightTexture, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetDirectionalLightTexture));
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowMap, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetDirectionalLightShadowMap));
      d.Add(SceneEffectParameterSemantics.SpotlightTexture, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetSpotlightTexture));
      d.Add(SceneEffectParameterSemantics.ProjectorLightTexture, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetProjectorLightTexture));

      d = TextureCubeBindings;
      d.Add(SceneEffectParameterSemantics.PointLightTexture, (e, p, o) => CreateDelegateParameterBinding<TextureCube>(e, p, GetPointLightTexture));
      d.Add(SceneEffectParameterSemantics.EnvironmentMap, (e, p, o) => CreateDelegateParameterBinding<TextureCube>(e, p, GetImageBasedLightTexture));

      d = StructBindings;
      d.Add(SceneEffectParameterSemantics.DirectionalLightShadowParameters, (e, p, o) => new DirectionalLightShadowParameterBinding(e, p));
    }
    #endregion


    //--------------------------------------------------------------
    #region General Scene Node Callbacks
    //--------------------------------------------------------------

    private float GetSceneNodeType(DelegateParameterBinding<float> binding, RenderContext context)
    {
      var node = context.SceneNode;
      if (node == null || !node.IsStatic)
        return 0;

      return 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Camera Callbacks
    //--------------------------------------------------------------

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static float GetCameraNear(DelegateParameterBinding<float> binding, RenderContext context)
    {
      if (context.CameraNode == null)
        throw new EffectBindingException("CameraNode needs to be set in render context.");

      return context.CameraNode.Camera.Projection.Near;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static float GetCameraFar(DelegateParameterBinding<float> binding, RenderContext context)
    {
      if (context.CameraNode == null)
        throw new EffectBindingException("CameraNode needs to be set in render context.");

      return context.CameraNode.Camera.Projection.Far;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static Vector3 GetCameraDirection(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.CameraNode == null)
        throw new EffectBindingException("CameraNode needs to be set in render context.");

      return (Vector3)(-context.CameraNode.ViewInverse.GetColumn(2).XYZ);

      // Same as
      //return context.CameraNode.PoseWorld.ToWorldDirection(Vector3F.Forward);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static Vector4 GetCameraDirection(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.CameraNode == null)
        throw new EffectBindingException("CameraNode needs to be set in render context.");

      return (Vector4)(-context.CameraNode.ViewInverse.GetColumn(2));
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static Vector3 GetCameraPosition(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.CameraNode == null)
        throw new EffectBindingException("CameraNode needs to be set in render context.");

      return (Vector3)context.CameraNode.PoseWorld.Position;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static Vector4 GetCameraPosition(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.CameraNode == null)
        throw new EffectBindingException("CameraNode needs to be set in render context.");

      return new Vector4((Vector3)context.CameraNode.PoseWorld.Position, 1);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static Vector3 GetLastCameraDirection(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.CameraNode == null)
        throw new EffectBindingException("CameraNode needs to be set in render context.");

      var pose = context.CameraNode.LastPoseWorld;
      if (pose.HasValue)
        return (Vector3)(-pose.Value.Orientation.GetColumn(2));
      else
        return GetCameraDirection(binding, context);
    }


    private static Vector4 GetLastCameraDirection(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      var v = GetLastCameraDirection((DelegateParameterBinding<Vector3>)null, context);
      return new Vector4(v.X, v.Y, v.Z, 0);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CameraNode")]
    private static Vector3 GetLastCameraPosition(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.CameraNode == null)
        throw new EffectBindingException("CameraNode needs to be set in render context.");

      var pose = context.CameraNode.LastPoseWorld;
      if (pose.HasValue)
        return (Vector3)(pose.Value.Position);
      else
        return GetCameraPosition(binding, context);
    }


    private static Vector4 GetLastCameraPosition(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      var v = GetLastCameraPosition((DelegateParameterBinding<Vector3>)null, context);
      return new Vector4(v.X, v.Y, v.Z, 1);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static Vector3 GetLodCameraPosition(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      var lodCameraNode = context.LodCameraNode ?? context.CameraNode;
      if (lodCameraNode == null)
        throw new EffectBindingException("LodCameraNode or CameraNode needs to be set in render context.");

      return (Vector3)lodCameraNode.PoseWorld.Position;
    }
    #endregion


    //--------------------------------------------------------------
    #region Light Callbacks
    //--------------------------------------------------------------

    private static readonly LightQuery EmptyLightQuery = new LightQuery();

    private static readonly Vector3 DefaultLightColor3;
    private static readonly Vector4 DefaultLightColor4;
    private static readonly Vector3 DefaultLightDirection3;
    private static readonly Vector4 DefaultLightDirection4;
    private static readonly Vector3 DefaultLightUp3;
    private static readonly Vector4 DefaultLightUp4;
    private static readonly Vector3 DefaultLightPosition3;
    private static readonly Vector4 DefaultLightPosition4;

    private static readonly float DefaultAmbientLightAttenuation;

    private static readonly Vector2 DefaultDirectionalLightTextureOffset;
    private static readonly Vector2 DefaultDirectionalLightTextureScale;

    private static readonly float DefaultPointLightRange;
    private static readonly float DefaultPointLightAttenuation;

    private static readonly float DefaultSpotlightRange;
    private static readonly float DefaultSpotlightFalloffAngle;
    private static readonly float DefaultSpotlightCutoffAngle;
    private static readonly float DefaultSpotlightAttenuation;

    private static readonly float DefaultProjectorLightAttenuation;
    private static readonly float DefaultProjectorLightRange;

    private static readonly Func<LightNode, Vector3> GetLightNodeForward3 = lightNode => -(Vector3)lightNode.PoseWorld.Orientation.GetColumn(2);
    private static readonly Func<LightNode, Vector4> GetLightNodeForward4 = lightNode => new Vector4(-(Vector3)lightNode.PoseWorld.Orientation.GetColumn(2), 0);
    private static readonly Func<LightNode, Vector3> GetLightNodeUp3 = lightNode => (Vector3)lightNode.PoseWorld.Orientation.GetColumn(1);
    private static readonly Func<LightNode, Vector4> GetLightNodeUp4 = lightNode => new Vector4((Vector3)lightNode.PoseWorld.Orientation.GetColumn(1), 0);
    private static readonly Func<LightNode, Vector3> GetLightNodePosition3 = lightNode => (Vector3)lightNode.PoseWorld.Position;
    private static readonly Func<LightNode, Vector4> GetLightNodePosition4 = lightNode => new Vector4((Vector3)lightNode.PoseWorld.Position, 1);


    // Auxiliary methods
    internal static List<LightNode> QueryLightNodes<T>(RenderContext context) where T : Light
    {
      var scene = context.Scene;
      var sceneNode = context.SceneNode;
      if (scene != null)
      {
        if (sceneNode != null)
          return scene.Query<LightQuery>(sceneNode, context).GetLights<T>();

        if (context.CameraNode != null)
          return scene.Query<GlobalLightQuery>(context.CameraNode, context).GetLights<T>();
      }

      return EmptyLightQuery.GetLights<T>();
    }


    internal static LightNode GetLightNode<T>(EffectParameterBinding binding, RenderContext context) where T : Light
    {
      var lights = QueryLightNodes<T>(context);
      var usage = binding.Description;
      int index = (usage != null) ? usage.Index : 0;
      if (index < lights.Count)
        return lights[index];

      return null;
    }


    private static T GetLight<T>(EffectParameterBinding binding, RenderContext context) where T : Light
    {
      var lightNode = GetLightNode<T>(binding, context);
      return (lightNode != null) ? (T)lightNode.Light : null;
    }


    private static TValue GetLightNodeProperty<TLight, TValue>(DelegateParameterBinding<TValue> binding, RenderContext context, Func<LightNode, TValue> getProperty, TValue fallbackValue) where TLight : Light
    {
      var lightNode = GetLightNode<TLight>(binding, context);
      if (lightNode != null)
        return getProperty(lightNode);

      return fallbackValue;
    }


    private static void GetLightNodePropertyArray<TLight, TValue>(DelegateParameterArrayBinding<TValue> binding, RenderContext context, Func<LightNode, TValue> getProperty, TValue[] values, TValue fallbackValue) where TLight : Light
    {
      var lights = QueryLightNodes<TLight>(context);

      // Assign light properties to effect parameter bindings.
      var usage = binding.Description;
      int baseIndex = (usage != null) ? usage.Index : 0;
      int count = Math.Min(values.Length, lights.Count - baseIndex);
      int i;
      for (i = 0; i < count; i++)
      {
        var lightNode = lights[baseIndex + i];
        values[i] = getProperty(lightNode);
      }

      // Set remaining effect parameter bindings to their fallback value.
      for (; i < values.Length; i++)
        values[i] = fallbackValue;
    }


    private static TValue GetLightProperty<TLight, TValue>(DelegateParameterBinding<TValue> binding, RenderContext context, Func<TLight, TValue> getProperty, TValue fallbackValue) where TLight : Light
    {
      var light = GetLight<TLight>(binding, context);
      if (light != null)
        return getProperty(light);

      return fallbackValue;
    }


    private static void GetLightPropertyArray<TLight, TValue>(DelegateParameterArrayBinding<TValue> binding, RenderContext context, Func<TLight, TValue> getProperty, TValue[] values, TValue fallbackValue) where TLight : Light
    {
      var lights = QueryLightNodes<TLight>(context);

      // Assign light properties to effect parameter bindings.
      var usage = binding.Description;
      int baseIndex = (usage != null) ? usage.Index : 0;
      int count = Math.Min(values.Length, lights.Count - baseIndex);
      int i;
      for (i = 0; i < count; i++)
      {
        var lightNode = lights[baseIndex + i];
        TLight light = (TLight)lightNode.Light;
        values[i] = getProperty(light);
      }

      // Set remaining effect parameters to their fallback value.
      for (; i < values.Length; i++)
        values[i] = fallbackValue;
    }


    #region ----- Ambient Lights -----

    private static readonly Func<AmbientLight, Vector3> GetAmbientLightColorLdr3 = light => (Vector3)light.Color * light.Intensity;
    private static readonly Func<AmbientLight, Vector4> GetAmbientLightColorLdr4 = light => new Vector4((Vector3)light.Color * light.Intensity, 1);
    private static readonly Func<AmbientLight, Vector3> GetAmbientLightColorHdr3 = light => (Vector3)light.Color * light.Intensity * light.HdrScale;
    private static readonly Func<AmbientLight, Vector4> GetAmbientLightColorHdr4 = light => new Vector4((Vector3)light.Color * light.Intensity * light.HdrScale, 1);
    private static readonly Func<AmbientLight, float> GetAmbientLightAttenuationF = light => light.HemisphericAttenuation;


    private static Vector3 GetAmbientLight(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetAmbientLightColorHdr3, DefaultLightColor3);
      else
        return GetLightProperty(binding, context, GetAmbientLightColorLdr3, DefaultLightColor3);
    }


    private static Vector4 GetAmbientLight(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetAmbientLightColorHdr4, DefaultLightColor4);
      else
        return GetLightProperty(binding, context, GetAmbientLightColorLdr4, DefaultLightColor4);
    }


    private static void GetAmbientLightArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetAmbientLightColorHdr3, values, DefaultLightColor3);
      else
        GetLightPropertyArray(binding, context, GetAmbientLightColorLdr3, values, DefaultLightColor3);
    }


    private static void GetAmbientLightArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetAmbientLightColorHdr4, values, DefaultLightColor4);
      else
        GetLightPropertyArray(binding, context, GetAmbientLightColorLdr4, values, DefaultLightColor4);
    }


    private static float GetAmbientLightAttenuation(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetAmbientLightAttenuationF, DefaultAmbientLightAttenuation);
    }


    private static void GetAmbientLightAttenuationArray(DelegateParameterArrayBinding<float> binding, RenderContext context, float[] values)
    {
      GetLightPropertyArray(binding, context, GetAmbientLightAttenuationF, values, DefaultAmbientLightAttenuation);
    }


    private static Vector3 GetAmbientLightUp(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      return GetLightNodeProperty<AmbientLight, Vector3>(binding, context, GetLightNodeUp3, DefaultLightUp3);
    }


    private static Vector4 GetAmbientLightUp(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetLightNodeProperty<AmbientLight, Vector4>(binding, context, GetLightNodeUp4, DefaultLightUp4);
    }


    private static void GetAmbientLightUpArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      GetLightNodePropertyArray<AmbientLight, Vector3>(binding, context, GetLightNodeUp3, values, DefaultLightUp3);
    }


    private static void GetAmbientLightUpArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      GetLightNodePropertyArray<AmbientLight, Vector4>(binding, context, GetLightNodeUp4, values, DefaultLightUp4);
    }
    #endregion


    #region ----- Directional Lights -----

    private static readonly Func<DirectionalLight, Vector3> GetDirectionalLightDiffuseLdr3 = light => (Vector3)light.Color * light.DiffuseIntensity;
    private static readonly Func<DirectionalLight, Vector4> GetDirectionalLightDiffuseLdr4 = light => new Vector4((Vector3)light.Color * light.DiffuseIntensity, 1);
    private static readonly Func<DirectionalLight, Vector3> GetDirectionalLightSpecularLdr3 = light => (Vector3)light.Color * light.SpecularIntensity;
    private static readonly Func<DirectionalLight, Vector4> GetDirectionalLightSpecularLdr4 = light => new Vector4((Vector3)light.Color * light.SpecularIntensity, 1);
    private static readonly Func<DirectionalLight, Vector3> GetDirectionalLightDiffuseHdr3 = light => (Vector3)light.Color * light.DiffuseIntensity * light.HdrScale;
    private static readonly Func<DirectionalLight, Vector4> GetDirectionalLightDiffuseHdr4 = light => new Vector4((Vector3)light.Color * light.DiffuseIntensity * light.HdrScale, 1);
    private static readonly Func<DirectionalLight, Vector3> GetDirectionalLightSpecularHdr3 = light => (Vector3)light.Color * light.SpecularIntensity * light.HdrScale;
    private static readonly Func<DirectionalLight, Vector4> GetDirectionalLightSpecularHdr4 = light => new Vector4((Vector3)light.Color * light.SpecularIntensity * light.HdrScale, 1);
    private static readonly Func<DirectionalLight, Vector2> GetDirectionalLightTextureOffset_ = light => (Vector2)light.TextureOffset;
    private static readonly Func<DirectionalLight, Vector2> GetDirectionalLightTextureScale_ = light => (Vector2)light.TextureScale;
    private static readonly Func<DirectionalLight, Texture> GetDirectionalLightTexture_ = light => light.Texture;
    private static readonly Func<DirectionalLight, Texture2D> GetDirectionalLightTexture2D = light => light.Texture;
    // Note: In .NET 4.0 we only need Func<DirectionalLight, Texture2D>. But .NET 3.5 does not support co- and contra-variance!


    private static Vector3 GetDirectionalLightDiffuse(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetDirectionalLightDiffuseHdr3, DefaultLightColor3);
      else
        return GetLightProperty(binding, context, GetDirectionalLightDiffuseLdr3, DefaultLightColor3);
    }


    private static Vector4 GetDirectionalLightDiffuse(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetDirectionalLightDiffuseHdr4, DefaultLightColor4);
      else
        return GetLightProperty(binding, context, GetDirectionalLightDiffuseLdr4, DefaultLightColor4);
    }


    private static void GetDirectionalLightDiffuseArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetDirectionalLightDiffuseHdr3, values, DefaultLightColor3);
      else
        GetLightPropertyArray(binding, context, GetDirectionalLightDiffuseLdr3, values, DefaultLightColor3);
    }


    private static void GetDirectionalLightDiffuseArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetDirectionalLightDiffuseHdr4, values, DefaultLightColor4);
      else
        GetLightPropertyArray(binding, context, GetDirectionalLightDiffuseLdr4, values, DefaultLightColor4);
    }


    private static Vector3 GetDirectionalLightSpecular(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetDirectionalLightSpecularHdr3, DefaultLightColor3);
      else
        return GetLightProperty(binding, context, GetDirectionalLightSpecularLdr3, DefaultLightColor3);
    }


    private static Vector4 GetDirectionalLightSpecular(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetDirectionalLightSpecularHdr4, DefaultLightColor4);
      else
        return GetLightProperty(binding, context, GetDirectionalLightSpecularLdr4, DefaultLightColor4);
    }


    private static void GetDirectionalLightSpecularArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetDirectionalLightSpecularHdr3, values, DefaultLightColor3);
      else
        GetLightPropertyArray(binding, context, GetDirectionalLightSpecularLdr3, values, DefaultLightColor3);

    }


    private static void GetDirectionalLightSpecularArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetDirectionalLightSpecularHdr4, values, DefaultLightColor4);
      else
        GetLightPropertyArray(binding, context, GetDirectionalLightSpecularLdr4, values, DefaultLightColor4);
    }


    private static Vector3 GetDirectionalLightDirection(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      return GetLightNodeProperty<DirectionalLight, Vector3>(binding, context, GetLightNodeForward3, DefaultLightDirection3);
    }


    private static Vector4 GetDirectionalLightDirection(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetLightNodeProperty<DirectionalLight, Vector4>(binding, context, GetLightNodeForward4, DefaultLightDirection4);
    }


    private static void GetDirectionalLightDirectionArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      GetLightNodePropertyArray<DirectionalLight, Vector3>(binding, context, GetLightNodeForward3, values, DefaultLightDirection3);
    }


    private static void GetDirectionalLightDirectionArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      GetLightNodePropertyArray<DirectionalLight, Vector4>(binding, context, GetLightNodeForward4, values, DefaultLightDirection4);
    }


    private static Texture GetDirectionalLightTexture(DelegateParameterBinding<Texture> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetDirectionalLightTexture_, null);
    }


    private static Texture2D GetDirectionalLightTexture(DelegateParameterBinding<Texture2D> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetDirectionalLightTexture2D, null);
    }


    private static Vector2 GetDirectionalLightTextureOffset(DelegateParameterBinding<Vector2> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetDirectionalLightTextureOffset_, DefaultDirectionalLightTextureOffset);
    }


    private static void GetDirectionalLightTextureOffsetArray(DelegateParameterArrayBinding<Vector2> binding, RenderContext context, Vector2[] values)
    {
      GetLightPropertyArray(binding, context, GetDirectionalLightTextureOffset_, values, Vector2.Zero);
    }


    private static Vector2 GetDirectionalLightTextureScale(DelegateParameterBinding<Vector2> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetDirectionalLightTextureScale_, DefaultDirectionalLightTextureScale);
    }


    private static void GetDirectionalLightTextureScaleArray(DelegateParameterArrayBinding<Vector2> binding, RenderContext context, Vector2[] values)
    {
      GetLightPropertyArray(binding, context, GetDirectionalLightTextureScale_, values, Vector2.Zero);
    }


    private static readonly Func<LightNode, Matrix> GetDirectionalLightTextureMatrix_ = lightNode =>
    {
      var light = (DirectionalLight)lightNode.Light;
      var textureProjection = Matrix44F.CreateOrthographicOffCenter(
        -light.TextureOffset.X,
        -light.TextureOffset.X + Math.Abs(light.TextureScale.X),
        light.TextureOffset.Y,
        light.TextureOffset.Y + Math.Abs(light.TextureScale.Y),
        1, // Not relevant
        2); // Not relevant.
      var scale = Matrix44F.CreateScale(Math.Sign(light.TextureScale.X), Math.Sign(light.TextureScale.Y), 1);

      return (Matrix)(GraphicsHelper.ProjectorBiasMatrix * scale * textureProjection * lightNode.PoseWorld.Inverse);
    };


    private static Matrix GetDirectionalLightTextureMatrix(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLightNodeProperty<DirectionalLight, Matrix>(binding, context, GetDirectionalLightTextureMatrix_, Matrix.Identity);
    }


    private static void GetDirectionalLightTextureMatrixArray(DelegateParameterArrayBinding<Matrix> binding, RenderContext context, Matrix[] values)
    {
      GetLightNodePropertyArray<DirectionalLight, Matrix>(binding, context, GetDirectionalLightTextureMatrix_, values, Matrix.Identity);
    }


    private static T GetDirectionalLightShadowMap<T>(DelegateParameterBinding<T> binding, RenderContext context) where T : Texture
    {
      var lightNode = GetLightNode<DirectionalLight>(binding, context);
      if (lightNode == null)
        return context.GraphicsService.GetDefaultTexture2DWhite() as T;

      var shadow = lightNode.Shadow;
      if (shadow == null)
        return context.GraphicsService.GetDefaultTexture2DWhite() as T;

      var cascadedShadow = shadow as CascadedShadow;
      if (cascadedShadow != null)
        return cascadedShadow.ShadowMap as T;

      var standardShadow = shadow as StandardShadow;
      if (standardShadow != null)
        return standardShadow.ShadowMap as T;

      return context.GraphicsService.GetDefaultTexture2DWhite() as T;
    }


    // Workaround: MonoGame does not support struct parameter DirectionalLightShadowParameters.
    // CascadedShadow parameters need to be specified individually.
    private static TValue GetShadowProperty<TLight, TShadow, TValue>(DelegateParameterBinding<TValue> binding, RenderContext context, Func<TShadow, TValue> getProperty, TValue fallbackValue)
      where TLight : Light
      where TShadow : Shadow
    {
      var lightNode = GetLightNode<TLight>(binding, context);
      if (lightNode != null)
      {
        var shadow = lightNode.Shadow as TShadow;
        if (shadow != null)
          return getProperty(shadow);
      }

      return fallbackValue;
    }

    private static readonly Func<CascadedShadow, int> GetDirectionalLightShadowNumberOfCascades_ = shadow => shadow.NumberOfCascades;
    private static readonly Func<CascadedShadow, Vector4> GetDirectionalLightShadowCascadesDistances_ = shadow => (Vector4)shadow.Distances;
    private static readonly Func<CascadedShadow, Vector4> GetDirectionalLightShadowDepthBias_ = shadow => (Vector4)shadow.EffectiveDepthBias;
    private static readonly Func<CascadedShadow, Vector4> GetDirectionalLightShadowNormalOffset_ = shadow => (Vector4)shadow.EffectiveNormalOffset;
#pragma warning disable 618
    private static readonly Func<CascadedShadow, Vector4> GetDirectionalLightShadowDepthBiasScale_ = shadow => (Vector4)shadow.DepthBiasScale;
    private static readonly Func<CascadedShadow, Vector4> GetDirectionalLightShadowDepthBiasOffset_ = shadow => (Vector4)shadow.DepthBiasOffset;
#pragma warning restore 618
    private static readonly Func<CascadedShadow, Vector2> GetDirectionalLightShadowMapSize_ = shadow =>
    {
      var shadowMap = shadow.ShadowMap;
      return shadowMap != null ? new Vector2(shadowMap.Width, shadowMap.Height) : new Vector2();
    };
    private static readonly Func<CascadedShadow, float> GetDirectionalLightShadowFilterRadius_ = shadow => shadow.FilterRadius;
    private static readonly Func<CascadedShadow, float> GetDirectionalLightShadowJitterResolution_ = shadow => shadow.JitterResolution / NoiseHelper.DefaultJitterMapWidth;
    private static readonly Func<CascadedShadow, Vector4> GetDirectionalLightShadowFilterRadius4F_ = shadow => new Vector4(shadow.FilterRadius);
    private static readonly Func<CascadedShadow, Vector4> GetDirectionalLightShadowJitterResolution4F_ = shadow => new Vector4(shadow.JitterResolution / NoiseHelper.DefaultJitterMapWidth);
    private static readonly Func<CascadedShadow, float> GetDirectionalLightShadowFadeOutRange_ = shadow => shadow.FadeOutRange;
#pragma warning disable 618
    private static readonly Func<CascadedShadow, float> GetDirectionalLightShadowFadeOutDistance_ = shadow => shadow.FadeOutDistance;
    private static readonly Func<CascadedShadow, float> GetDirectionalLightShadowMaxDistance_ = shadow => shadow.Distances[shadow.NumberOfCascades - 1];
#pragma warning restore 618
    private static readonly Func<CascadedShadow, float> GetDirectionalLightShadowFog_ = shadow => shadow.ShadowFog;


    private static int GetDirectionalLightShadowNumberOfCascades(DelegateParameterBinding<int> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, int>(binding, context, GetDirectionalLightShadowNumberOfCascades_, 0);
    }


    private static Vector4 GetDirectionalLightShadowCascadeDistances(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, Vector4>(binding, context, GetDirectionalLightShadowCascadesDistances_, new Vector4());
    }


    private static void GetDirectionalLightShadowViewProjections(DelegateParameterArrayBinding<Matrix> binding, RenderContext context, Matrix[] values)
    {
      var lightNode = GetLightNode<DirectionalLight>(binding, context);
      if (lightNode != null)
      {
        var shadow = lightNode.Shadow as CascadedShadow;
        if (shadow != null)
        {
          Array.Copy(shadow.ViewProjections, values, Math.Min(shadow.ViewProjections.Length, values.Length));
          return;
        }
      }

      for (int i = 0; i < values.Length; i++)
        values[i] = Matrix.Identity;
    }


    private static Vector4 GetDirectionalLightShadowDepthBias(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, Vector4>(binding, context, GetDirectionalLightShadowDepthBias_, new Vector4(1));
    }


    private static Vector4 GetDirectionalLightShadowNormalOffset(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, Vector4>(binding, context, GetDirectionalLightShadowNormalOffset_, new Vector4(1));
    }


    private static Vector4 GetDirectionalLightShadowDepthBiasScale(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, Vector4>(binding, context, GetDirectionalLightShadowDepthBiasScale_, new Vector4(1));
    }


    private static Vector4 GetDirectionalLightShadowDepthBiasOffset(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, Vector4>(binding, context, GetDirectionalLightShadowDepthBiasOffset_, new Vector4(0));
    }


    private static Vector2 GetDirectionalLightShadowMapSize(DelegateParameterBinding<Vector2> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, Vector2>(binding, context, GetDirectionalLightShadowMapSize_, new Vector2(1, 1));
    }


    private static float GetDirectionalLightShadowFilterRadius(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, float>(binding, context, GetDirectionalLightShadowFilterRadius_, 1.0f);
    }


    private static float GetDirectionalLightShadowJitterResolution(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, float>(binding, context, GetDirectionalLightShadowJitterResolution_, 2048.0f / NoiseHelper.DefaultJitterMapWidth);
    }


    /* Obsolete */
    private static Vector4 GetDirectionalLightShadowFilterRadius(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, Vector4>(binding, context, GetDirectionalLightShadowFilterRadius4F_, new Vector4());
    }


    /* Obsolete */
    private static Vector4 GetDirectionalLightShadowJitterResolution(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, Vector4>(binding, context, GetDirectionalLightShadowJitterResolution4F_, new Vector4());
    }


    private static float GetDirectionalLightShadowFadeOutRange(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, float>(binding, context, GetDirectionalLightShadowFadeOutRange_, 0);
    }


    private static float GetDirectionalLightShadowFadeOutDistance(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, float>(binding, context, GetDirectionalLightShadowFadeOutDistance_, 0);
    }


    private static float GetDirectionalLightShadowMaxDistance(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, float>(binding, context, GetDirectionalLightShadowMaxDistance_, 0);
    }


    private static float GetDirectionalLightShadowFog(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetShadowProperty<DirectionalLight, CascadedShadow, float>(binding, context, GetDirectionalLightShadowFog_, 0);
    }
    #endregion


    #region ----- Point Lights -----

    private static readonly Func<PointLight, Vector3> GetPointLightDiffuseLdr3 = light => (Vector3)light.Color * light.DiffuseIntensity;
    private static readonly Func<PointLight, Vector4> GetPointLightDiffuseLdr4 = light => new Vector4((Vector3)light.Color * light.DiffuseIntensity, 1);
    private static readonly Func<PointLight, Vector3> GetPointLightSpecularLdr3 = light => (Vector3)light.Color * light.SpecularIntensity;
    private static readonly Func<PointLight, Vector4> GetPointLightSpecularLdr4 = light => new Vector4((Vector3)light.Color * light.SpecularIntensity, 1);
    private static readonly Func<PointLight, Vector3> GetPointLightDiffuseHdr3 = light => (Vector3)light.Color * light.DiffuseIntensity * light.HdrScale;
    private static readonly Func<PointLight, Vector4> GetPointLightDiffuseHdr4 = light => new Vector4((Vector3)light.Color * light.DiffuseIntensity * light.HdrScale, 1);
    private static readonly Func<PointLight, Vector3> GetPointLightSpecularHdr3 = light => (Vector3)light.Color * light.SpecularIntensity * light.HdrScale;
    private static readonly Func<PointLight, Vector4> GetPointLightSpecularHdr4 = light => new Vector4((Vector3)light.Color * light.SpecularIntensity * light.HdrScale, 1);
    private static readonly Func<PointLight, float> GetPointLightRangeF = light => light.Range;
    private static readonly Func<PointLight, float> GetPointLightAttenuationF = light => light.Attenuation;
    private static readonly Func<PointLight, Texture> GetPointLightTexture_ = light => light.Texture;
    private static readonly Func<PointLight, TextureCube> GetPointLightTextureCube = light => light.Texture;
    // Note: In .NET 4.0 we only need Func<PointLight, TextureCube>. But .NET 3.5 does not support co- and contra-variance!


    private static Vector3 GetPointLightDiffuse(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetPointLightDiffuseHdr3, DefaultLightColor3);
      else
        return GetLightProperty(binding, context, GetPointLightDiffuseLdr3, DefaultLightColor3);
    }


    private static Vector4 GetPointLightDiffuse(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetPointLightDiffuseHdr4, DefaultLightColor4);
      else
        return GetLightProperty(binding, context, GetPointLightDiffuseLdr4, DefaultLightColor4);
    }


    private static void GetPointLightDiffuseArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetPointLightDiffuseHdr3, values, DefaultLightColor3);
      else
        GetLightPropertyArray(binding, context, GetPointLightDiffuseLdr3, values, DefaultLightColor3);
    }


    private static void GetPointLightDiffuseArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetPointLightDiffuseHdr4, values, DefaultLightColor4);
      else
        GetLightPropertyArray(binding, context, GetPointLightDiffuseLdr4, values, DefaultLightColor4);
    }


    private static Vector3 GetPointLightSpecular(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetPointLightSpecularHdr3, DefaultLightColor3);
      else
        return GetLightProperty(binding, context, GetPointLightSpecularLdr3, DefaultLightColor3);
    }


    private static Vector4 GetPointLightSpecular(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetPointLightSpecularHdr4, DefaultLightColor4);
      else
        return GetLightProperty(binding, context, GetPointLightSpecularLdr4, DefaultLightColor4);
    }


    private static void GetPointLightSpecularArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetPointLightSpecularHdr3, values, DefaultLightColor3);
      else
        GetLightPropertyArray(binding, context, GetPointLightSpecularLdr3, values, DefaultLightColor3);
    }


    private static void GetPointLightSpecularArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetPointLightSpecularHdr4, values, DefaultLightColor4);
      else
        GetLightPropertyArray(binding, context, GetPointLightSpecularLdr4, values, DefaultLightColor4);
    }


    private static Vector3 GetPointLightPosition(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      return GetLightNodeProperty<PointLight, Vector3>(binding, context, GetLightNodePosition3, DefaultLightPosition3);
    }


    private static Vector4 GetPointLightPosition(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetLightNodeProperty<PointLight, Vector4>(binding, context, GetLightNodePosition4, DefaultLightPosition4);
    }


    private static void GetPointLightPositionArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      GetLightNodePropertyArray<PointLight, Vector3>(binding, context, GetLightNodePosition3, values, DefaultLightPosition3);
    }


    private static void GetPointLightPositionArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      GetLightNodePropertyArray<PointLight, Vector4>(binding, context, GetLightNodePosition4, values, DefaultLightPosition4);
    }


    private static float GetPointLightRange(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetPointLightRangeF, DefaultPointLightRange);
    }


    private static void GetPointLightRangeArray(DelegateParameterArrayBinding<float> binding, RenderContext context, float[] values)
    {
      GetLightPropertyArray(binding, context, GetPointLightRangeF, values, DefaultPointLightRange);
    }


    private static float GetPointLightAttenuation(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetPointLightAttenuationF, DefaultPointLightAttenuation);
    }


    private static void GetPointLightAttenuationArray(DelegateParameterArrayBinding<float> binding, RenderContext context, float[] values)
    {
      GetLightPropertyArray(binding, context, GetPointLightAttenuationF, values, DefaultPointLightAttenuation);
    }


    private static Texture GetPointLightTexture(DelegateParameterBinding<Texture> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetPointLightTexture_, null);
    }


    private static TextureCube GetPointLightTexture(DelegateParameterBinding<TextureCube> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetPointLightTextureCube, null);
    }


    private static readonly Func<LightNode, Matrix> GetPointLightTextureMatrix_ = lightNode =>
    {
      // Cube maps are left handed --> Sample with inverted z. (Otherwise, the 
      // cube map and objects or texts in it are mirrored.)
      var mirrorZ = Matrix44F.CreateScale(1, 1, -1);
      return (Matrix)(mirrorZ * lightNode.PoseWorld.Inverse);
    };


    private static Matrix GetPointLightTextureMatrix(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLightNodeProperty<PointLight, Matrix>(binding, context, GetPointLightTextureMatrix_, Matrix.Identity);
    }


    private static void GetPointLightTextureMatrixArray(DelegateParameterArrayBinding<Matrix> binding, RenderContext context, Matrix[] values)
    {
      GetLightNodePropertyArray<PointLight, Matrix>(binding, context, GetPointLightTextureMatrix_, values, Matrix.Identity);
    }
    #endregion


    #region ----- Spotlights -----

    private static readonly Func<Spotlight, Vector3> GetSpotlightDiffuseLdr3 = light => (Vector3)light.Color * light.DiffuseIntensity;
    private static readonly Func<Spotlight, Vector4> GetSpotlightDiffuseLdr4 = light => new Vector4((Vector3)light.Color * light.DiffuseIntensity, 1);
    private static readonly Func<Spotlight, Vector3> GetSpotlightSpecularLdr3 = light => (Vector3)light.Color * light.SpecularIntensity;
    private static readonly Func<Spotlight, Vector4> GetSpotlightSpecularLdr4 = light => new Vector4((Vector3)light.Color * light.SpecularIntensity, 1);
    private static readonly Func<Spotlight, Vector3> GetSpotlightDiffuseHdr3 = light => (Vector3)light.Color * light.DiffuseIntensity * light.HdrScale;
    private static readonly Func<Spotlight, Vector4> GetSpotlightDiffuseHdr4 = light => new Vector4((Vector3)light.Color * light.DiffuseIntensity * light.HdrScale, 1);
    private static readonly Func<Spotlight, Vector3> GetSpotlightSpecularHdr3 = light => (Vector3)light.Color * light.SpecularIntensity * light.HdrScale;
    private static readonly Func<Spotlight, Vector4> GetSpotlightSpecularHdr4 = light => new Vector4((Vector3)light.Color * light.SpecularIntensity * light.HdrScale, 1);
    private static readonly Func<Spotlight, float> GetSpotlightRangeF = light => light.Range;
    private static readonly Func<Spotlight, float> GetSpotlightFalloffAngleF = light => light.FalloffAngle;
    private static readonly Func<Spotlight, float> GetSpotlightCutoffAngleF = light => light.CutoffAngle;
    private static readonly Func<Spotlight, float> GetSpotlightAttenuationF = light => light.Attenuation;
    private static readonly Func<Spotlight, Texture> GetSpotlightTexture_ = light => light.Texture;
    private static readonly Func<Spotlight, Texture2D> GetSpotlightTexture2D = light => light.Texture;
    // Note: In .NET 4.0 we only need Func<Spotlight, Texture2D>. But .NET 3.5 does not support co- and contra-variance!


    private static Vector3 GetSpotlightDiffuse(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetSpotlightDiffuseHdr3, DefaultLightColor3);
      else
        return GetLightProperty(binding, context, GetSpotlightDiffuseLdr3, DefaultLightColor3);
    }


    private static Vector4 GetSpotlightDiffuse(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetSpotlightDiffuseHdr4, DefaultLightColor4);
      else
        return GetLightProperty(binding, context, GetSpotlightDiffuseLdr4, DefaultLightColor4);
    }


    private static void GetSpotlightDiffuseArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetSpotlightDiffuseHdr3, values, DefaultLightColor3);
      else
        GetLightPropertyArray(binding, context, GetSpotlightDiffuseLdr3, values, DefaultLightColor3);
    }


    private static void GetSpotlightDiffuseArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetSpotlightDiffuseHdr4, values, DefaultLightColor4);
      else
        GetLightPropertyArray(binding, context, GetSpotlightDiffuseLdr4, values, DefaultLightColor4);
    }


    private static Vector3 GetSpotlightSpecular(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetSpotlightSpecularHdr3, DefaultLightColor3);
      else
        return GetLightProperty(binding, context, GetSpotlightSpecularLdr3, DefaultLightColor3);
    }


    private static Vector4 GetSpotlightSpecular(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetSpotlightSpecularHdr4, DefaultLightColor4);
      else
        return GetLightProperty(binding, context, GetSpotlightSpecularLdr4, DefaultLightColor4);
    }


    private static void GetSpotlightSpecularArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetSpotlightSpecularHdr3, values, DefaultLightColor3);
      else
        GetLightPropertyArray(binding, context, GetSpotlightSpecularLdr3, values, DefaultLightColor3);
    }


    private static void GetSpotlightSpecularArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetSpotlightSpecularHdr4, values, DefaultLightColor4);
      else
        GetLightPropertyArray(binding, context, GetSpotlightSpecularLdr4, values, DefaultLightColor4);
    }


    private static Vector3 GetSpotlightPosition(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightNodeProperty<Spotlight, Vector3>(binding, context, GetLightNodePosition3, DefaultLightPosition3);
      else
        return GetLightNodeProperty<Spotlight, Vector3>(binding, context, GetLightNodePosition3, DefaultLightPosition3);
    }


    private static Vector4 GetSpotlightPosition(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetLightNodeProperty<Spotlight, Vector4>(binding, context, GetLightNodePosition4, DefaultLightPosition4);
    }


    private static void GetSpotlightPositionArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      GetLightNodePropertyArray<Spotlight, Vector3>(binding, context, GetLightNodePosition3, values, DefaultLightPosition3);
    }


    private static void GetSpotlightPositionArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      GetLightNodePropertyArray<Spotlight, Vector4>(binding, context, GetLightNodePosition4, values, DefaultLightPosition4);
    }


    private static Vector3 GetSpotlightDirection(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      return GetLightNodeProperty<Spotlight, Vector3>(binding, context, GetLightNodeForward3, DefaultLightDirection3);
    }


    private static Vector4 GetSpotlightDirection(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetLightNodeProperty<Spotlight, Vector4>(binding, context, GetLightNodeForward4, DefaultLightDirection4);
    }


    private static void GetSpotlightDirectionArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      GetLightNodePropertyArray<Spotlight, Vector3>(binding, context, GetLightNodeForward3, values, DefaultLightDirection3);
    }


    private static void GetSpotlightDirectionArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      GetLightNodePropertyArray<Spotlight, Vector4>(binding, context, GetLightNodeForward4, values, DefaultLightDirection4);
    }


    private static float GetSpotlightRange(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetSpotlightRangeF, DefaultSpotlightRange);
    }


    private static void GetSpotlightRangeArray(DelegateParameterArrayBinding<float> binding, RenderContext context, float[] values)
    {
      GetLightPropertyArray(binding, context, GetSpotlightRangeF, values, DefaultSpotlightRange);
    }


    private static float GetSpotlightFalloffAngle(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetSpotlightFalloffAngleF, DefaultSpotlightFalloffAngle);
    }


    private static void GetSpotlightFalloffAngleArray(DelegateParameterArrayBinding<float> binding, RenderContext context, float[] values)
    {
      GetLightPropertyArray(binding, context, GetSpotlightFalloffAngleF, values, DefaultSpotlightFalloffAngle);
    }


    private static float GetSpotlightCutoffAngle(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetSpotlightCutoffAngleF, DefaultSpotlightCutoffAngle);
    }


    private static void GetSpotlightCutoffAngleArray(DelegateParameterArrayBinding<float> binding, RenderContext context, float[] values)
    {
      GetLightPropertyArray(binding, context, GetSpotlightCutoffAngleF, values, DefaultSpotlightCutoffAngle);
    }


    private static float GetSpotlightAttenuation(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetSpotlightAttenuationF, DefaultSpotlightAttenuation);
    }


    private static void GetSpotlightAttenuationArray(DelegateParameterArrayBinding<float> binding, RenderContext context, float[] values)
    {
      GetLightPropertyArray(binding, context, GetSpotlightAttenuationF, values, DefaultSpotlightAttenuation);
    }


    private static Texture GetSpotlightTexture(DelegateParameterBinding<Texture> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetSpotlightTexture_, null);
    }


    private static Texture2D GetSpotlightTexture(DelegateParameterBinding<Texture2D> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetSpotlightTexture2D, null);
    }


    private static readonly Func<LightNode, Matrix> GetSpotlightTextureMatrix_ = lightNode =>
    {
      var light = (Spotlight)lightNode.Light;
      var projection = Matrix44F.CreatePerspectiveFieldOfView(light.CutoffAngle * 2 * lightNode.ScaleWorld.Y, 1, 1, 100);
      return (Matrix)(GraphicsHelper.ProjectorBiasMatrix * projection * lightNode.PoseWorld.Inverse);
    };


    private static Matrix GetSpotlightTextureMatrix(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLightNodeProperty<Spotlight, Matrix>(binding, context, GetSpotlightTextureMatrix_, Matrix.Identity);
    }


    private static void GetSpotlightTextureMatrixArray(DelegateParameterArrayBinding<Matrix> binding, RenderContext context, Matrix[] values)
    {
      GetLightNodePropertyArray<Spotlight, Matrix>(binding, context, GetSpotlightTextureMatrix_, values, Matrix.Identity);
    }
    #endregion


    #region ----- Projector Lights -----

    private static readonly Func<ProjectorLight, Vector3> GetProjectorLightDiffuseLdr3 = light => (Vector3)light.Color * light.DiffuseIntensity;
    private static readonly Func<ProjectorLight, Vector4> GetProjectorLightDiffuseLdr4 = light => new Vector4((Vector3)light.Color * light.DiffuseIntensity, 1);
    private static readonly Func<ProjectorLight, Vector3> GetProjectorLightSpecularLdr3 = light => (Vector3)light.Color * light.SpecularIntensity;
    private static readonly Func<ProjectorLight, Vector4> GetProjectorLightSpecularLdr4 = light => new Vector4((Vector3)light.Color * light.SpecularIntensity, 1);
    private static readonly Func<ProjectorLight, Vector3> GetProjectorLightDiffuseHdr3 = light => (Vector3)light.Color * light.DiffuseIntensity * light.HdrScale;
    private static readonly Func<ProjectorLight, Vector4> GetProjectorLightDiffuseHdr4 = light => new Vector4((Vector3)light.Color * light.DiffuseIntensity * light.HdrScale, 1);
    private static readonly Func<ProjectorLight, Vector3> GetProjectorLightSpecularHdr3 = light => (Vector3)light.Color * light.SpecularIntensity * light.HdrScale;
    private static readonly Func<ProjectorLight, Vector4> GetProjectorLightSpecularHdr4 = light => new Vector4((Vector3)light.Color * light.SpecularIntensity * light.HdrScale, 1);
    private static readonly Func<ProjectorLight, float> GetProjectorLightRangeF = light => light.Projection.Far;
    private static readonly Func<ProjectorLight, float> GetProjectorLightAttenuationF = light => light.Attenuation;
    private static readonly Func<LightNode, Matrix> GetProjectorLightViewProjection_ = lightNode => (Matrix)(((ProjectorLight)lightNode.Light).Projection.ToMatrix44F() * lightNode.PoseWorld.Inverse);
    private static readonly Func<ProjectorLight, Texture> GetProjectorLightTexture_ = light => light.Texture;
    private static readonly Func<ProjectorLight, Texture2D> GetProjectorLightTexture2D = light => light.Texture;
    // Note: In .NET 4.0 we only need Func<ProjectorLight, Texture2D>. But .NET 3.5 does not support co- and contra-variance!


    private static Vector3 GetProjectorLightDiffuse(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetProjectorLightDiffuseHdr3, DefaultLightColor3);
      else
        return GetLightProperty(binding, context, GetProjectorLightDiffuseLdr3, DefaultLightColor3);
    }


    private static Vector4 GetProjectorLightDiffuse(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetProjectorLightDiffuseHdr4, DefaultLightColor4);
      else
        return GetLightProperty(binding, context, GetProjectorLightDiffuseLdr4, DefaultLightColor4);
    }


    private static void GetProjectorLightDiffuseArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetProjectorLightDiffuseHdr3, values, DefaultLightColor3);
      else
        GetLightPropertyArray(binding, context, GetProjectorLightDiffuseLdr3, values, DefaultLightColor3);
    }


    private static void GetProjectorLightDiffuseArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetProjectorLightDiffuseHdr4, values, DefaultLightColor4);
      else
        GetLightPropertyArray(binding, context, GetProjectorLightDiffuseLdr4, values, DefaultLightColor4);
    }


    private static Vector3 GetProjectorLightSpecular(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetProjectorLightSpecularHdr3, DefaultLightColor3);
      else
        return GetLightProperty(binding, context, GetProjectorLightSpecularLdr3, DefaultLightColor3);
    }


    private static Vector4 GetProjectorLightSpecular(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetProjectorLightSpecularHdr4, DefaultLightColor4);
      else
        return GetLightProperty(binding, context, GetProjectorLightSpecularLdr4, DefaultLightColor4);
    }


    private static void GetProjectorLightSpecularArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetProjectorLightSpecularHdr3, values, DefaultLightColor3);
      else
        GetLightPropertyArray(binding, context, GetProjectorLightSpecularLdr3, values, DefaultLightColor3);
    }


    private static void GetProjectorLightSpecularArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      if (context.IsHdrEnabled())
        GetLightPropertyArray(binding, context, GetProjectorLightSpecularHdr4, values, DefaultLightColor4);
      else
        GetLightPropertyArray(binding, context, GetProjectorLightSpecularLdr4, values, DefaultLightColor4);
    }


    private static Vector3 GetProjectorLightPosition(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      return GetLightNodeProperty<ProjectorLight, Vector3>(binding, context, GetLightNodePosition3, DefaultLightPosition3);
    }


    private static Vector4 GetProjectorLightPosition(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetLightNodeProperty<ProjectorLight, Vector4>(binding, context, GetLightNodePosition4, DefaultLightPosition4);
    }


    private static void GetProjectorLightPositionArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      GetLightNodePropertyArray<ProjectorLight, Vector3>(binding, context, GetLightNodePosition3, values, DefaultLightPosition3);
    }


    private static void GetProjectorLightPositionArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      GetLightNodePropertyArray<ProjectorLight, Vector4>(binding, context, GetLightNodePosition4, values, DefaultLightPosition4);
    }


    private static Vector3 GetProjectorLightDirection(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      return GetLightNodeProperty<ProjectorLight, Vector3>(binding, context, GetLightNodeForward3, DefaultLightDirection3);
    }


    private static Vector4 GetProjectorLightDirection(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      return GetLightNodeProperty<ProjectorLight, Vector4>(binding, context, GetLightNodeForward4, DefaultLightDirection4);
    }


    private static void GetProjectorLightDirectionArray(DelegateParameterArrayBinding<Vector3> binding, RenderContext context, Vector3[] values)
    {
      GetLightNodePropertyArray<ProjectorLight, Vector3>(binding, context, GetLightNodeForward3, values, DefaultLightDirection3);
    }


    private static void GetProjectorLightDirectionArray(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      GetLightNodePropertyArray<ProjectorLight, Vector4>(binding, context, GetLightNodeForward4, values, DefaultLightDirection4);
    }


    private static float GetProjectorLightRange(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetProjectorLightRangeF, DefaultProjectorLightRange);
    }


    private static void GetProjectorLightRangeArray(DelegateParameterArrayBinding<float> binding, RenderContext context, float[] values)
    {
      GetLightPropertyArray(binding, context, GetProjectorLightRangeF, values, DefaultProjectorLightRange);
    }


    private static float GetProjectorLightAttenuation(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetProjectorLightAttenuationF, DefaultProjectorLightAttenuation);
    }


    private static void GetProjectorLightAttenuationArray(DelegateParameterArrayBinding<float> binding, RenderContext context, float[] values)
    {
      GetLightPropertyArray(binding, context, GetProjectorLightAttenuationF, values, DefaultProjectorLightAttenuation);
    }


    private static Texture GetProjectorLightTexture(DelegateParameterBinding<Texture> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetProjectorLightTexture_, null);
    }


    private static Texture2D GetProjectorLightTexture(DelegateParameterBinding<Texture2D> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetProjectorLightTexture2D, null);
    }


    private static Matrix GetProjectorLightViewProjection(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLightNodeProperty<ProjectorLight, Matrix>(
              binding,
              context,
              GetProjectorLightViewProjection_,
              Matrix.Identity);
    }


    private static void GetProjectorLightViewProjectionArray(DelegateParameterArrayBinding<Matrix> binding, RenderContext context, Matrix[] values)
    {
      GetLightNodePropertyArray<ProjectorLight, Matrix>(
        binding,
        context,
        GetProjectorLightViewProjection_,
        values,
        Matrix.Identity);
    }


    private static readonly Func<LightNode, Matrix> GetProjectorLightTextureMatrix_ = lightNode =>
    {
      var light = (ProjectorLight)lightNode.Light;
      return (Matrix)(GraphicsHelper.ProjectorBiasMatrix * light.Projection * lightNode.PoseWorld.Inverse);
    };


    private static Matrix GetProjectorLightTextureMatrix(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLightNodeProperty<ProjectorLight, Matrix>(binding, context, GetProjectorLightTextureMatrix_, Matrix.Identity);
    }


    private static void GetProjectorLightTextureMatrixArray(DelegateParameterArrayBinding<Matrix> binding, RenderContext context, Matrix[] values)
    {
      GetLightNodePropertyArray<ProjectorLight, Matrix>(binding, context, GetProjectorLightTextureMatrix_, values, Matrix.Identity);
    }
    #endregion


    #region ----- Image-Based Lighting (IBL), Environment Maps -----

    // Note: In .NET 4.0 we only need Func<IBL, TextureCube>. But .NET 3.5 does not support co- and contra-variance!
    private static readonly Func<ImageBasedLight, Texture> GetImageBasedLightTexture_ = light => light.Texture;
    private static readonly Func<ImageBasedLight, TextureCube> GetImageBasedLightTextureCube = light => light.Texture;
    private static readonly Func<ImageBasedLight, float> GetImageBasedLightTextureSize_ = light => light.Texture != null ? light.Texture.Size : 128;
    private static readonly Func<ImageBasedLight, Vector3> GetImageBasedLightDiffuseLdr3 = light => (Vector3)light.Color * GetDiffuseIntensity(light);
    private static readonly Func<ImageBasedLight, Vector4> GetImageBasedLightDiffuseLdr4 = light => new Vector4((Vector3)light.Color * GetDiffuseIntensity(light), 1);
    private static readonly Func<ImageBasedLight, Vector3> GetImageBasedLightSpecularLdr3 = light => (Vector3)light.Color * GetSpecularIntensity(light);
    private static readonly Func<ImageBasedLight, Vector4> GetImageBasedLightSpecularLdr4 = light => new Vector4((Vector3)light.Color * GetSpecularIntensity(light), 1);
    private static readonly Func<ImageBasedLight, Vector3> GetImageBasedLightDiffuseHdr3 = light => (Vector3)light.Color * GetDiffuseIntensity(light) * light.HdrScale;
    private static readonly Func<ImageBasedLight, Vector4> GetImageBasedLightDiffuseHdr4 = light => new Vector4((Vector3)light.Color * GetDiffuseIntensity(light) * light.HdrScale, 1);
    private static readonly Func<ImageBasedLight, Vector3> GetImageBasedLightSpecularHdr3 = light => (Vector3)light.Color * GetSpecularIntensity(light) * light.HdrScale;
    private static readonly Func<ImageBasedLight, Vector4> GetImageBasedLightSpecularHdr4 = light => new Vector4((Vector3)light.Color * GetSpecularIntensity(light) * light.HdrScale, 1);


    private static float GetDiffuseIntensity(ImageBasedLight light)
    {
      return Numeric.IsNaN(light.DiffuseIntensity) ? 0.0f : light.DiffuseIntensity;
    }


    private static float GetSpecularIntensity(ImageBasedLight light)
    {
      return Numeric.IsNaN(light.SpecularIntensity) ? 0.0f : light.SpecularIntensity;
    }


    private static Texture GetImageBasedLightTexture(DelegateParameterBinding<Texture> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetImageBasedLightTexture_, null);
    }


    private static TextureCube GetImageBasedLightTexture(DelegateParameterBinding<TextureCube> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetImageBasedLightTextureCube, null);
    }


    private static float GetImageBasedLightTextureSize(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetImageBasedLightTextureSize_, 128);
    }


    private static Vector3 GetImageBasedLightDiffuse(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetImageBasedLightDiffuseHdr3, DefaultLightColor3);
      else
        return GetLightProperty(binding, context, GetImageBasedLightDiffuseLdr3, DefaultLightColor3);
    }


    private static Vector4 GetImageBasedLightDiffuse(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetImageBasedLightDiffuseHdr4, DefaultLightColor4);
      else
        return GetLightProperty(binding, context, GetImageBasedLightDiffuseLdr4, DefaultLightColor4);
    }


    private static Vector3 GetImageBasedLightSpecular(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetImageBasedLightSpecularHdr3, DefaultLightColor3);
      else
        return GetLightProperty(binding, context, GetImageBasedLightSpecularLdr3, DefaultLightColor3);
    }


    private static Vector4 GetImageBasedLightSpecular(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (context.IsHdrEnabled())
        return GetLightProperty(binding, context, GetImageBasedLightSpecularHdr4, DefaultLightColor4);
      else
        return GetLightProperty(binding, context, GetImageBasedLightSpecularLdr4, DefaultLightColor4);
    }


    private static readonly Func<ImageBasedLight, float> GetImageBasedLightRgbmMax_ = light =>
    {
      var rgbm = light.Encoding as RgbmEncoding;
      return rgbm != null ? GraphicsHelper.ToGamma(rgbm.Max) : 1;
    };

    private static float GetImageBasedLightRgbmMax(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetLightProperty(binding, context, GetImageBasedLightRgbmMax_, 1);
    }


    private static readonly Func<LightNode, Matrix> GetImageBasedLightTextureMatrix_ = lightNode =>
    {
      // Cube maps are left handed --> Sample with inverted z. (Otherwise, the 
      // cube map and objects or texts in it are mirrored.)
      var mirrorZ = Matrix44F.CreateScale(1, 1, -1);
      return (Matrix)(mirrorZ * lightNode.PoseWorld.Inverse);
    };

    private static Matrix GetImageBasedLightTextureMatrix(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLightNodeProperty<ImageBasedLight, Matrix>(binding, context, GetImageBasedLightTextureMatrix_, Matrix.Identity);
    }
    #endregion
    


    #region ----- Shadows -----

    // Obsolete
    private static float GetShadowNear(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return Numeric.IsNaN(context.ShadowNear) ? GetCameraNear(binding, context) : context.ShadowNear;
    }
    #endregion

    #endregion


    //--------------------------------------------------------------
    #region Fog Callbacks
    //--------------------------------------------------------------

    // Returns null, or a list with at least one node.
    internal static List<FogNode> QueryFogNodes(RenderContext context)
    {
      var scene = context.Scene;
      if (scene != null && context.CameraNode != null)
      {
        var fogNodes = scene.Query<FogQuery>(context.CameraNode, context).FogNodes;
        if (fogNodes.Count > 0)
          return fogNodes;
      }

      return null;
    }


    private static float GetFogStart(DelegateParameterBinding<float> binding, RenderContext context)
    {
      var nodes = QueryFogNodes(context);
      if (nodes != null)
        return nodes[0].Fog.Start;

      return 0;
    }


    private static float GetFogEnd(DelegateParameterBinding<float> binding, RenderContext context)
    {
      var nodes = QueryFogNodes(context);
      if (nodes != null)
        return nodes[0].Fog.End;

      return 100;
    }


    private static float GetFogDensity(DelegateParameterBinding<float> binding, RenderContext context)
    {
      var nodes = QueryFogNodes(context);
      if (nodes != null)
        return nodes[0].Fog.Density;

      return 1;
    }


    private static Vector3 GetFogColor(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      var nodes = QueryFogNodes(context);
      if (nodes != null)
      {
        // Undo premultiplied alpha.
        return (Vector3)nodes[0].Fog.Color0.XYZ / nodes[0].Fog.Color1.W;
      }

      return new Vector3(1, 1, 1);
    }


    private static Vector4 GetFogColor(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      var nodes = QueryFogNodes(context);
      if (nodes != null)
      {
        // Undo premultiplied alpha.
        return new Vector4(
          (Vector3)nodes[0].Fog.Color0.XYZ / nodes[0].Fog.Color0.W,
          nodes[0].Fog.Color0.W);
      }

      return new Vector4(1, 1, 1, 0);
    }


    private static Vector4 GetFogParameters(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      var nodes = QueryFogNodes(context);
      if (nodes == null || context.CameraNode == null)
        return new Vector4(0, 100, 1, 0);

      var fogNode = nodes[0];
      var cameraNode = context.CameraNode;

      var fog = fogNode.Fog;
      float fogDensity = fog.Density;
      float heightFalloff = fog.HeightFalloff;

      // Important: This FogDensity is different from the FogDensity used by the FogRenderer and Fog.fx.
      // Fog.fx now handles these numerical problems. Since FogParameters is usually only used with
      // forward rendered materials, we do not make this improvements in our Forward.fx yet.
      // (The numerical problems are usually only visible when we look at things very far away
      // - like the sky box.)
      if (!Numeric.IsZero(heightFalloff))
      {
        float cameraDensity = (float)Math.Pow(2, -heightFalloff * cameraNode.PoseWorld.Position.Y);

        // Trick: If the heightFalloff is very large, the e^x function can quickly reach
        // the float limit! If this happens, the shader will not compute any fog and this
        // looks like the fog disappears. To avoid this problem we reduce the heightFalloff
        // to keep the result of e^x always within floating point range.
        const float Limit = 1e-37f;
        if (cameraDensity < Limit)
        {
          heightFalloff = (float)Math.Log(Limit) / -cameraNode.PoseWorld.Position.Y / ConstantsF.Ln2;
          cameraDensity = Limit;
        }

        // fogDensity is at world space height 0. If the fog node is on another height,
        // we change the fogDensity. 
        fogDensity *= (float)Math.Pow(2, -heightFalloff * (-fogNode.PoseWorld.Position.Y));

        // Combine camera and fog density.
        fogDensity *= cameraDensity;
      }

      // Height-dependent fog always starts at the viewer. We use the density
      // to specify the end parameter.
      return new Vector4(fog.Start, fog.End, fogDensity, heightFalloff);
    }
    #endregion


    //--------------------------------------------------------------
    #region Decals
    //--------------------------------------------------------------

    private static float GetDecalAlpha(DelegateParameterBinding<float> binding, RenderContext context)
    {
      var decalNode = context.SceneNode as DecalNode;
      if (decalNode == null)
        return 1.0f;

      return decalNode.Alpha;
    }


    private static float GetDecalNormalThreshold(DelegateParameterBinding<float> binding, RenderContext context)
    {
      var decalNode = context.SceneNode as DecalNode;
      if (decalNode == null)
        return 0.5f; // = cos(60°)

      return (float)Math.Cos(decalNode.NormalThreshold);
    }


    private static Vector3 GetDecalOrientation(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      var decalNode = context.SceneNode as DecalNode;
      if (decalNode == null)
        return new Vector3(0, 1, 0);

      return (Vector3)decalNode.PoseWorld.Orientation.GetColumn(2);
    }


    private static bool GetDecalOptions(DelegateParameterBinding<bool> binding, RenderContext context)
    {
      var decalNode = context.SceneNode as DecalNode;
      if (decalNode == null)
        return true;

      return decalNode.Options == DecalOptions.ProjectOnAll;
    }
    #endregion


    //--------------------------------------------------------------
    #region World, View, Projection
    //--------------------------------------------------------------

    private static Matrix GetWorld(RenderContext context)
    {
      var node = context.SceneNode;
      if (node == null)
        return Matrix.Identity;

      Matrix world = node.PoseWorld;
      Vector3F scale = node.ScaleWorld;
      world.M11 *= scale.X; world.M12 *= scale.X; world.M13 *= scale.X;
      world.M21 *= scale.Y; world.M22 *= scale.Y; world.M23 *= scale.Y;
      world.M31 *= scale.Z; world.M32 *= scale.Z; world.M33 *= scale.Z;

      return world;
    }


    private static Matrix GetUnscaledWorld(RenderContext context)
    {
      var node = context.SceneNode;
      if (node == null)
        return Matrix.Identity;

      return node.PoseWorld;
    }


    private static Matrix GetView(RenderContext context)
    {
      var node = context.CameraNode;
      if (node == null)
        return Matrix.Identity;

      return (Matrix)context.CameraNode.View;
    }


    private static Matrix GetProjection(RenderContext context)
    {
      var node = context.CameraNode;
      if (node == null)
        return Matrix.Identity;

      return context.CameraNode.Camera.Projection;
    }


    private static Matrix GetViewProjection(RenderContext context)
    {
      var node = context.CameraNode;
      if (node == null)
        return Matrix.Identity;

      return (Matrix)context.CameraNode.View * context.CameraNode.Camera.Projection;
    }


    private static Matrix GetWorld(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetWorld(context);
    }


    private static Matrix GetWorldInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetWorld(context));
    }


    private static Matrix GetWorldTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetWorld(context));
    }


    private static Matrix GetWorldInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetWorld(context)));
    }


    private static Matrix GetView(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetView(context);
    }


    private static Matrix GetViewInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetView(context));
    }


    private static Matrix GetViewTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetView(context));
    }


    private static Matrix GetViewInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetView(context)));
    }


    private static Matrix GetProjection(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetProjection(context);
    }


    private static Matrix GetProjectionInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetProjection(context));
    }


    private static Matrix GetProjectionTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetProjection(context));
    }


    private static Matrix GetProjectionInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetProjection(context)));
    }


    internal static Matrix GetWorldView(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetWorld(context) * GetView(context);
    }


    private static Matrix GetWorldViewInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetWorld(context) * GetView(context));
    }


    private static Matrix GetWorldViewTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetWorld(context) * GetView(context));
    }


    private static Matrix GetWorldViewInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetWorld(context) * GetView(context)));
    }


    private static Matrix GetViewProjection(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetViewProjection(context);
    }


    private static Matrix GetViewProjectionInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetViewProjection(context));
    }


    private static Matrix GetViewProjectionTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetViewProjection(context));
    }


    private static Matrix GetViewProjectionInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetViewProjection(context)));
    }


    private static Matrix GetWorldViewProjection(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetWorld(context) * GetViewProjection(context);
    }


    private static Matrix GetWorldViewProjectionInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetWorld(context) * GetViewProjection(context));
    }


    private static Matrix GetWorldViewProjectionTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetWorld(context) * GetViewProjection(context));
    }


    private static Matrix GetWorldViewProjectionInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetWorld(context) * GetViewProjection(context)));
    }


    internal static Matrix GetUnscaledWorld(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetUnscaledWorld(context);
    }


    internal static Matrix GetUnscaledWorldView(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetUnscaledWorld(context) * GetView(context);
    }
    #endregion


    //--------------------------------------------------------------
    #region Last World, View, Projection
    //--------------------------------------------------------------

    private static Matrix GetLastWorld(RenderContext context)
    {
      var node = context.SceneNode;
      if (node == null)
        return Matrix.Identity;

      Matrix world = node.LastPoseWorld.HasValue ? node.LastPoseWorld.Value : node.PoseWorld;
      Vector3F scale = node.LastScaleWorld.HasValue ? node.LastScaleWorld.Value : node.ScaleWorld;
      world.M11 *= scale.X; world.M12 *= scale.X; world.M13 *= scale.X;
      world.M21 *= scale.Y; world.M22 *= scale.Y; world.M23 *= scale.Y;
      world.M31 *= scale.Z; world.M32 *= scale.Z; world.M33 *= scale.Z;

      return world;
    }


    private static Matrix GetLastView(RenderContext context)
    {
      var cameraNode = context.CameraNode;
      if (cameraNode == null)
        return Matrix.Identity;

      if (cameraNode.LastPoseWorld.HasValue)
        return cameraNode.LastPoseWorld.Value.Inverse.ToXna();
      else
        return (Matrix)context.CameraNode.View;
    }


    private static Matrix GetLastProjection(RenderContext context)
    {
      var cameraNode = context.CameraNode;
      if (cameraNode == null)
        return Matrix.Identity;

      var camera = cameraNode.Camera;
      if (camera.LastProjection.HasValue)
        return (Matrix)camera.LastProjection.Value;

      return camera.Projection;
    }


    private static Matrix GetLastViewProjection(RenderContext context)
    {
      return GetLastView(context) * GetLastProjection(context);
    }


    private static Matrix GetLastWorld(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLastWorld(context);
    }


    private static Matrix GetLastWorldInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetLastWorld(context));
    }


    private static Matrix GetLastWorldTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetLastWorld(context));
    }


    private static Matrix GetLastWorldInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetLastWorld(context)));
    }


    private static Matrix GetLastView(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLastView(context);
    }


    private static Matrix GetLastViewInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetLastView(context));
    }


    private static Matrix GetLastViewTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetLastView(context));
    }


    private static Matrix GetLastViewInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetLastView(context)));
    }


    private static Matrix GetLastProjection(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLastProjection(context);
    }


    private static Matrix GetLastProjectionInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetLastProjection(context));
    }


    private static Matrix GetLastProjectionTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetLastProjection(context));
    }


    private static Matrix GetLastProjectionInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetLastProjection(context)));
    }


    private static Matrix GetLastWorldView(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLastWorld(context) * GetLastView(context);
    }


    private static Matrix GetLastWorldViewInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetLastWorld(context) * GetLastView(context));
    }


    private static Matrix GetLastWorldViewTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetLastWorld(context) * GetLastView(context));
    }


    private static Matrix GetLastWorldViewInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetLastWorld(context) * GetLastView(context)));
    }


    private static Matrix GetLastViewProjection(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLastViewProjection(context);
    }


    private static Matrix GetLastViewProjectionInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetLastViewProjection(context));
    }


    private static Matrix GetLastViewProjectionTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetLastViewProjection(context));
    }


    private static Matrix GetLastViewProjectionInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetLastViewProjection(context)));
    }


    private static Matrix GetLastWorldViewProjection(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return GetLastWorld(context) * GetLastViewProjection(context);
    }


    private static Matrix GetLastWorldViewProjectionInverse(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Invert(GetLastWorld(context) * GetLastViewProjection(context));
    }


    private static Matrix GetLastWorldViewProjectionTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(GetLastWorld(context) * GetLastViewProjection(context));
    }


    private static Matrix GetLastWorldViewProjectionInverseTranspose(DelegateParameterBinding<Matrix> binding, RenderContext context)
    {
      return Matrix.Transpose(Matrix.Invert(GetLastWorld(context) * GetLastViewProjection(context)));
    }
    #endregion
  }
}
