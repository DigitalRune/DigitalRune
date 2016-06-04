// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Binds an <see cref="EffectParameter"/> to the shadow parameters of a directional
  /// light shadow map.
  /// </summary>
  [DebuggerDisplay("{GetType().Name,nq}(Parameter = {Parameter.Name}, Value = {Value})")]
  public class DirectionalLightShadowParameterBinding : EffectParameterBinding
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private class StandardShadowParameters
    {
      public StandardShadow StandardShadow;
      public EffectParameter ParameterNear;
      public EffectParameter ParameterFar;
      public EffectParameter ParameterView;
      public EffectParameter ParameterProjection;
      public EffectParameter ParameterDepthBias;
      public EffectParameter ParameterShadowMapSize;
      public EffectParameter ParameterFilterRadius;
      public EffectParameter ParameterJitterResolution;
    }


    private class CascadedShadowParameters
    {
      public CascadedShadow CascadedShadow;
      public EffectParameter ParameterNumberOfCascades;
      public EffectParameter ParameterCascadeDistances;
      public EffectParameter ParameterViewProjections;
      public EffectParameter ParameterDepthBiasScale;
      public EffectParameter ParameterDepthBiasOffset;
      public EffectParameter ParameterShadowMapSize;
      public EffectParameter ParameterFilterRadius;
      public EffectParameter ParameterJitterResolution;
      public EffectParameter ParameterFadeOutDistance;
      public EffectParameter ParameterMaxDistance;
      public EffectParameter ParameterShadowFog;
    }
    #endregion
    

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private StandardShadowParameters _standardShadowParameters;
    private CascadedShadowParameters _cascadedShadowParameters;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectionalLightShadowParameterBinding"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectionalLightShadowParameterBinding"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    protected DirectionalLightShadowParameterBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DirectionalLightShadowParameterBinding"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    public DirectionalLightShadowParameterBinding(Effect effect, EffectParameter parameter)
      : base(effect, parameter)
    {
      Initialize();
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void Initialize()
    {
      try
      {
        if (Parameter.Elements.Count > 0)
          return;

        if (Parameter.StructureMembers.Count == 0)
          return;

        if (Parameter.StructureMembers["NumberOfCascades"] == null)
        {
          _standardShadowParameters = new StandardShadowParameters
          {
            ParameterNear = Parameter.StructureMembers["Near"],
            ParameterFar = Parameter.StructureMembers["Far"],
            ParameterView = Parameter.StructureMembers["View"],
            ParameterProjection = Parameter.StructureMembers["Projection"],
            ParameterDepthBias = Parameter.StructureMembers["DepthBias"],
            ParameterShadowMapSize = Parameter.StructureMembers["ShadowMapSize"],
            ParameterFilterRadius = Parameter.StructureMembers["FilterRadius"],
            ParameterJitterResolution = Parameter.StructureMembers["JitterResolution"],
          };
        }
        else
        {
          _cascadedShadowParameters = new CascadedShadowParameters
          {
            ParameterNumberOfCascades = Parameter.StructureMembers["NumberOfCascades"],
            ParameterCascadeDistances = Parameter.StructureMembers["CascadeDistance"],
            ParameterViewProjections = Parameter.StructureMembers["ViewProjections"],
            ParameterDepthBiasScale = Parameter.StructureMembers["DepthBiasScale"],
            ParameterDepthBiasOffset = Parameter.StructureMembers["DepthBiasOffset"],
            ParameterShadowMapSize = Parameter.StructureMembers["ShadowMapSize"],
            ParameterFilterRadius = Parameter.StructureMembers["FilterRadius"],
            ParameterJitterResolution = Parameter.StructureMembers["JitterResolution"],
            ParameterFadeOutDistance = Parameter.StructureMembers["FadeOutDistance"],
            ParameterMaxDistance = Parameter.StructureMembers["MaxDistance"],
            ParameterShadowFog = Parameter.StructureMembers["ShadowFog"],
          };
        }
      }
      catch (Exception exception)
      {
        throw new GraphicsException("Could not initialize DirectionalLightShadowParameterBinding: " + exception.Message);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override EffectParameterBinding CreateInstanceCore()
    {
      return new DirectionalLightShadowParameterBinding();
    }


    /// <inheritdoc/>
    protected override void CloneCore(EffectParameterBinding source)
    {
      // Clone EffectParameterBinding properties.
      base.CloneCore(source);

      Initialize();
    }
    #endregion


    /// <inheritdoc/>
    protected override void OnUpdate(RenderContext context)
    {
      if (context == null)
        return;
      var lightNode = SceneEffectBinder.GetLightNode<DirectionalLight>(this, context);
      if (_standardShadowParameters != null)
      {
        _standardShadowParameters.StandardShadow = lightNode.Shadow as StandardShadow;
      }
      else if (_cascadedShadowParameters != null)
      {
        _cascadedShadowParameters.CascadedShadow = lightNode.Shadow as CascadedShadow;
      }
    }


    /// <inheritdoc/>
    protected override void OnApply(RenderContext context)
    {
      if (_standardShadowParameters != null && _standardShadowParameters.StandardShadow != null)
      {
        var shadow = _standardShadowParameters.StandardShadow;
        if (_standardShadowParameters.ParameterNear != null)
          _standardShadowParameters.ParameterNear.SetValue(shadow.Near);
        if (_standardShadowParameters.ParameterFar != null)
          _standardShadowParameters.ParameterFar.SetValue(shadow.Far);
        if (_standardShadowParameters.ParameterView != null)
          _standardShadowParameters.ParameterView.SetValue(shadow.View);
        if (_standardShadowParameters.ParameterProjection != null)
          _standardShadowParameters.ParameterProjection.SetValue(shadow.Projection);
#pragma warning disable 618
        if (_standardShadowParameters.ParameterDepthBias != null)
          _standardShadowParameters.ParameterDepthBias.SetValue(new Vector2(shadow.DepthBiasScale, shadow.DepthBiasOffset));
#pragma warning restore 618
        if (_standardShadowParameters.ParameterShadowMapSize != null)
          _standardShadowParameters.ParameterShadowMapSize.SetValue(new Vector2(shadow.ShadowMap.Width, shadow.ShadowMap.Height));
        if (_standardShadowParameters.ParameterFilterRadius != null)
          _standardShadowParameters.ParameterFilterRadius.SetValue(shadow.FilterRadius);
        if (_standardShadowParameters.ParameterJitterResolution != null)
          _standardShadowParameters.ParameterJitterResolution.SetValue(shadow.JitterResolution / NoiseHelper.DefaultJitterMapWidth);
      }
      else if (_cascadedShadowParameters != null)
      {
        var shadow = _cascadedShadowParameters.CascadedShadow;
        if (_cascadedShadowParameters.ParameterNumberOfCascades != null)
          _cascadedShadowParameters.ParameterNumberOfCascades.SetValue(shadow.NumberOfCascades);
        if (_cascadedShadowParameters.ParameterCascadeDistances != null)
        {
          if (context != null)
          {
            var near = (context.CameraNode != null) ? context.CameraNode.Camera.Projection.Near : 0.1f;
            _cascadedShadowParameters.ParameterCascadeDistances.SetValue(
              new Vector4(near, shadow.Distances[0], shadow.Distances[1], shadow.Distances[2]));
          }
          else
          {
            _cascadedShadowParameters.ParameterCascadeDistances.SetValue(new Vector4());
          }
        }
        if (_cascadedShadowParameters.ParameterViewProjections != null)
          _cascadedShadowParameters.ParameterViewProjections.SetValue(shadow.ViewProjections);
        if (_cascadedShadowParameters.ParameterShadowMapSize != null)
          _cascadedShadowParameters.ParameterShadowMapSize.SetValue(new Vector2(shadow.ShadowMap.Width, shadow.ShadowMap.Height));
        if (_cascadedShadowParameters.ParameterFilterRadius != null)
          _cascadedShadowParameters.ParameterFilterRadius.SetValue(new Vector4(shadow.FilterRadius));
        if (_cascadedShadowParameters.ParameterJitterResolution != null)
          _cascadedShadowParameters.ParameterJitterResolution.SetValue(new Vector4(shadow.JitterResolution / NoiseHelper.DefaultJitterMapWidth));
        if (_cascadedShadowParameters.ParameterShadowFog != null)
          _cascadedShadowParameters.ParameterShadowFog.SetValue(shadow.ShadowFog);
#pragma warning disable 618
        if (_cascadedShadowParameters.ParameterFadeOutDistance != null)
          _cascadedShadowParameters.ParameterFadeOutDistance.SetValue(shadow.FadeOutDistance);
        if (_cascadedShadowParameters.ParameterMaxDistance != null)
          _cascadedShadowParameters.ParameterMaxDistance.SetValue(shadow.MaxDistance);
        if (_cascadedShadowParameters.ParameterDepthBiasScale != null)
          _cascadedShadowParameters.ParameterDepthBiasScale.SetValue((Vector4)shadow.DepthBiasScale);
        if (_cascadedShadowParameters.ParameterDepthBiasOffset != null)
          _cascadedShadowParameters.ParameterDepthBiasOffset.SetValue((Vector4)shadow.DepthBiasOffset);
#pragma warning restore 618
      }
    }
    #endregion
  }
}
