// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides the bindings for default effect parameters. 
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Technique Bindings:</strong> By default, the <see cref="ByNameTechniqueBinding"/> is
  /// used for effects that contain several techniques. If there is only one technique, we use the
  /// default EffectTechniqueBinding.
  /// </para>
  /// <para>
  /// <strong>Parameter Bindings:</strong> See <see cref="DefaultEffectParameterSemantics"/> for
  /// supported semantics.
  /// </para>
  /// </remarks>
  public class DefaultEffectBinder : DictionaryEffectBinder
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IGraphicsService _graphicsService;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the value for effect parameters with the semantic
    /// <see cref="DefaultEffectParameterSemantics.Debug"/> and index 0.
    /// </summary>
    /// <value>
    /// The value for effect parameters with the semantic 
    /// <see cref="DefaultEffectParameterSemantics.Debug"/> and index 0.
    /// </value>
    /// <remarks>
    /// Use this value to simply and quickly inject values into a shader while debugging, e.g. you
    /// can set this property to (1, 0 , 0, 0) if a key is pressed on the keyboard and use this info
    /// in the shader to perform a different action.
    /// </remarks>
    public static Vector4 Debug0 { get; set; }


    /// <summary>
    /// Gets or sets the value for effect parameters with the semantic 
    /// <see cref="DefaultEffectParameterSemantics.Debug"/> and index 1.
    /// </summary>
    /// <value>
    /// The value for effect parameters with the semantic
    /// <see cref="DefaultEffectParameterSemantics.Debug"/> and index 1.
    /// </value>
    /// <remarks>
    /// Use this value to simply and quickly inject values into a shader while debugging, e.g. you
    /// can set this property to (1, 0, 0, 0) if a key is pressed on the keyboard and use this info
    /// in the shader to perform a different action.
    /// </remarks>
    public static Vector4 Debug1 { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultEffectBinder"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public DefaultEffectBinder(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _graphicsService = graphicsService;

      // Configure look-up tables.
      var lut = Int32Bindings;
      lut.Add(DefaultEffectParameterSemantics.PassIndex, (e, p, o) => CreateDelegateParameterBinding<int>(e, p, GetPassIndexAsInt32));

      lut = SingleBindings;
      lut.Add(DefaultEffectParameterSemantics.PassIndex, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetPassIndexAsSingle));
      lut.Add(DefaultEffectParameterSemantics.SpecularPower, (e, p, o) => CreateConstParameterBinding<float>(e, p, o, "SpecularPower"));
      lut.Add(DefaultEffectParameterSemantics.Opacity, (e, p, o) => CreateConstParameterBinding<float>(e, p, o, "Alpha"));
      lut.Add(DefaultEffectParameterSemantics.Alpha, (e, p, o) => CreateConstParameterBinding<float>(e, p, o, "Alpha"));
      lut.Add(DefaultEffectParameterSemantics.BlendMode, (e, p, o) => CreateConstParameterBinding<float>(e, p, o, "BlendMode"));
      lut.Add(DefaultEffectParameterSemantics.InstanceAlpha, (e, p, o) => CreateConstParameterBinding<float>(e, p, o, "InstanceAlpha"));
      lut.Add(DefaultEffectParameterSemantics.Time, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetTime));
      lut.Add(DefaultEffectParameterSemantics.LastTime, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetLastTime));
      lut.Add(DefaultEffectParameterSemantics.ElapsedTime, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetElapsedTime));
      lut.Add(DefaultEffectParameterSemantics.JitterMapSize, CreateJitterMapSizeBindingF);
      lut.Add(DefaultEffectParameterSemantics.MorphWeight, (e, p, o) => new ConstParameterBinding<float>(e, p, 0.0f));
      lut.Add(DefaultEffectParameterSemantics.NaN, (e, p, o) => new ConstParameterBinding<float>(e, p, float.NaN));

      lut = SingleArrayBindings;
      lut.Add(DefaultEffectParameterSemantics.MorphWeight, (e, p, o) => new ConstParameterArrayBinding<float>(e, p, new float[p.Elements.Count]));

      lut = MatrixArrayBindings;
#if ANIMATION
      lut.Add(DefaultEffectParameterSemantics.Bones, (e, p, o) => new SkeletonPoseParameterBinding(e, p));
#endif

      lut = Vector2Bindings;
      //lut.Add(DefaultEffectParameterSemantics.RenderTargetSize, (e, p, o) => CreateDelegateParameterBinding<Vector2>(e, p, GetRenderTargetSize));
      lut.Add(DefaultEffectParameterSemantics.ViewportSize, (e, p, o) => CreateDelegateParameterBinding<Vector2>(e, p, GetViewportSize));
      lut.Add(DefaultEffectParameterSemantics.JitterMapSize, CreateJitterMapSizeBinding2F);

      lut = Vector3Bindings;
      lut.Add(DefaultEffectParameterSemantics.DiffuseColor, (e, p, o) => CreateConstParameterBindingVector3(e, p, o, "DiffuseColor"));
      lut.Add(DefaultEffectParameterSemantics.SpecularColor, (e, p, o) => CreateConstParameterBindingVector3(e, p, o, "SpecularColor"));
      lut.Add(DefaultEffectParameterSemantics.EmissiveColor, (e, p, o) => CreateConstParameterBindingVector3(e, p, o, "EmissiveColor"));
      lut.Add(DefaultEffectParameterSemantics.InstanceColor, (e, p, o) => CreateConstParameterBindingVector3(e, p, o, "InstanceColor"));

      lut = Vector4Bindings;
      lut.Add(DefaultEffectParameterSemantics.DiffuseColor, (e, p, o) => CreateConstParameterBindingVector4(e, p, o, "DiffuseColor", 1));
      lut.Add(DefaultEffectParameterSemantics.SpecularColor, (e, p, o) => CreateConstParameterBindingVector4(e, p, o, "SpecularColor", 1));
      lut.Add(DefaultEffectParameterSemantics.EmissiveColor, (e, p, o) => CreateConstParameterBindingVector4(e, p, o, "EmissiveColor", 1));
      lut.Add(DefaultEffectParameterSemantics.Debug, (e, p, o) => CreateDelegateParameterBinding<Vector4>(e, p, GetDebug));

      lut = TextureBindings;
      lut.Add(DefaultEffectParameterSemantics.DiffuseTexture, (e, p, o) => CreateConstParameterBinding<Texture>(e, p, o, "Texture"));
      lut.Add(DefaultEffectParameterSemantics.SpecularTexture, CreateSpecularTextureBinding<Texture>);
      lut.Add(DefaultEffectParameterSemantics.NormalTexture, CreateNormalTextureBinding<Texture>);
      lut.Add(DefaultEffectParameterSemantics.SourceTexture, (e, p, o) => CreateDelegateParameterBinding<Texture>(e, p, GetSourceTexture));
      lut.Add(DefaultEffectParameterSemantics.GBuffer, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetGBuffer));
      lut.Add(DefaultEffectParameterSemantics.LightBuffer, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetLightBuffer));
      lut.Add(DefaultEffectParameterSemantics.NormalsFittingTexture, CreateNormalsFittingTextureBinding<Texture>);
      lut.Add(DefaultEffectParameterSemantics.DitherMap, CreateDitherMapBinding<Texture>);
      lut.Add(DefaultEffectParameterSemantics.JitterMap, CreateJitterMapBinding<Texture>);
      lut.Add(DefaultEffectParameterSemantics.NoiseMap, CreateNoiseMapBinding<Texture>);

      lut = Texture2DBindings;
      lut.Add(DefaultEffectParameterSemantics.DiffuseTexture, (e, p, o) => CreateConstParameterBinding<Texture2D>(e, p, o, "Texture"));
      lut.Add(DefaultEffectParameterSemantics.SpecularTexture, CreateSpecularTextureBinding<Texture2D>);
      lut.Add(DefaultEffectParameterSemantics.NormalTexture, CreateNormalTextureBinding<Texture2D>);
      lut.Add(DefaultEffectParameterSemantics.SourceTexture, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetSourceTexture2D));
      lut.Add(DefaultEffectParameterSemantics.GBuffer, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetGBuffer));
      lut.Add(DefaultEffectParameterSemantics.LightBuffer, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetLightBuffer));
      lut.Add(DefaultEffectParameterSemantics.NormalsFittingTexture, CreateNormalsFittingTextureBinding<Texture2D>);
      lut.Add(DefaultEffectParameterSemantics.DitherMap, CreateDitherMapBinding<Texture2D>);
      lut.Add(DefaultEffectParameterSemantics.JitterMap, CreateJitterMapBinding<Texture2D>);
      lut.Add(DefaultEffectParameterSemantics.NoiseMap, CreateNoiseMapBinding<Texture2D>);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override EffectTechniqueBinding GetBinding(Effect effect)
    {
      if (effect == null)
        throw new ArgumentNullException("effect");

      if (effect.Techniques.Count > 1)
        return new ByNameTechniqueBinding(effect);
      else
        return EffectTechniqueBinding.Default;
    }


    private static int GetPassIndexAsInt32(DelegateParameterBinding<int> binding, RenderContext context)
    {
      return Math.Max(context.PassIndex, 0);
    }


    private static float GetPassIndexAsSingle(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return Math.Max(context.PassIndex, 0.0f);
    }


    private static float GetTime(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return (float)context.Time.TotalSeconds;
    }


    private static float GetLastTime(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return (float)(context.Time - context.DeltaTime).TotalSeconds;
    }


    private static float GetElapsedTime(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return (float)context.DeltaTime.TotalSeconds;
    }


    //private Vector2 GetRenderTargetSize(DelegateParameterBinding<Vector2> binding, RenderContext context)
    //{
    //  var renderTarget = context.RenderTarget;
    //  if (renderTarget != null)
    //    return new Vector2(renderTarget.Width, renderTarget.Height);

    //  var presentationParameters = context.GraphicsService.GraphicsDevice.PresentationParameters;
    //  return new Vector2(presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
    //}


    private static Vector2 GetViewportSize(DelegateParameterBinding<Vector2> binding, RenderContext context)
    {
      var viewport = context.GraphicsService.GraphicsDevice.Viewport;
      return new Vector2(viewport.Width, viewport.Height);
    }


    private static Texture GetSourceTexture(DelegateParameterBinding<Texture> binding, RenderContext context)
    {
      return context.SourceTexture;
    }


    private static Texture2D GetSourceTexture2D(DelegateParameterBinding<Texture2D> binding, RenderContext context)
    {
      return context.SourceTexture;
    }


    private static Texture2D GetGBuffer(DelegateParameterBinding<Texture2D> binding, RenderContext context)
    {
      if (binding.Description.Index == 0)
        return context.GBuffer0;
      if (binding.Description.Index == 1)
        return context.GBuffer1;
      if (binding.Description.Index == 2)
        return context.GBuffer2;
      if (binding.Description.Index == 3)
        return context.GBuffer3;

      return null;
    }


    private static Texture2D GetLightBuffer(DelegateParameterBinding<Texture2D> binding, RenderContext context)
    {
      if (binding.Description.Index == 0)
        return context.LightBuffer0;
      if (binding.Description.Index == 1)
        return context.LightBuffer1;

      return null;
    }


    private EffectParameterBinding CreateNormalsFittingTextureBinding<T>(Effect effect,
      EffectParameter parameter, IDictionary<string, object> opaqueData) where T : Texture
    {
      return new ConstParameterBinding<T>(effect, parameter, _graphicsService.GetNormalsFittingTexture() as T);
    }


    private EffectParameterBinding CreateSpecularTextureBinding<T>(Effect effect,
      EffectParameter parameter, IDictionary<string, object> opaqueData) where T : Texture
    {
      object value = null;
      if (opaqueData != null)
        opaqueData.TryGetValue(parameter.Name, out value);

      var texture = value as T;
      if (texture == null)
        texture = _graphicsService.GetDefaultTexture2DWhite() as T;

      return new ConstParameterBinding<T>(effect, parameter, texture);
    }


    private EffectParameterBinding CreateNormalTextureBinding<T>(Effect effect,
      EffectParameter parameter, IDictionary<string, object> opaqueData) where T : Texture
    {
      object value = null;
      if (opaqueData != null)
        opaqueData.TryGetValue(parameter.Name, out value);

      var texture = value as T;
      if (texture == null)
        texture = _graphicsService.GetDefaultNormalTexture() as T;

      return new ConstParameterBinding<T>(effect, parameter, texture);
    }


    private EffectParameterBinding CreateDitherMapBinding<T>(Effect effect,
      EffectParameter parameter, IDictionary<string, object> opaqueData) where T : Texture
    {
      var ditherMap = NoiseHelper.GetDitherTexture(_graphicsService);
      return new ConstParameterBinding<T>(effect, parameter, ditherMap as T);
    }


    private EffectParameterBinding CreateJitterMapBinding<T>(Effect effect,
      EffectParameter parameter, IDictionary<string, object> opaqueData) where T : Texture
    {
      var jitterMap = NoiseHelper.GetGrainTexture(_graphicsService, NoiseHelper.DefaultJitterMapWidth);
      return new ConstParameterBinding<T>(effect, parameter, jitterMap as T);
    }


    private EffectParameterBinding CreateJitterMapSizeBindingF(Effect effect,
      EffectParameter parameter, IDictionary<string, object> opaqueData) 
    {
      return new ConstParameterBinding<float>(effect, parameter, NoiseHelper.DefaultJitterMapWidth);
    }


    private EffectParameterBinding CreateJitterMapSizeBinding2F(Effect effect,
      EffectParameter parameter, IDictionary<string, object> opaqueData)
    {
      return new ConstParameterBinding<Vector2>(effect, parameter, new Vector2(NoiseHelper.DefaultJitterMapWidth));
    }


    private EffectParameterBinding CreateNoiseMapBinding<T>(Effect effect,
      EffectParameter parameter, IDictionary<string, object> opaqueData) where T : Texture
    {
      var noiseMap = NoiseHelper.GetNoiseTexture(_graphicsService);
      return new ConstParameterBinding<T>(effect, parameter, noiseMap as T);
    }


    private static Vector4 GetDebug(DelegateParameterBinding<Vector4> binding, RenderContext context)
    {
      if (binding.Description.Index == 0)
        return Debug0;
      if (binding.Description.Index == 1)
        return Debug1;
      return new Vector4(0, 0, 0, 0);
    }
    #endregion
  }
}
