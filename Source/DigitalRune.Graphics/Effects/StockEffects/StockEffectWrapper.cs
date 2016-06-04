// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  // We wrap the stock effects and removes the default function of OnApply().
  // We do not want the OnApply() method to change our effect parameters.
  internal sealed class WrappedAlphaTestEffect : AlphaTestEffect
  {
    public WrappedAlphaTestEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }
#if !MONOGAME
    protected override void OnApply() { }
#else
    protected override bool OnApply() { return false; }
#endif
  }


  internal sealed class WrappedBasicEffect : BasicEffect
  {
    public WrappedBasicEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }
#if !MONOGAME
    protected override void OnApply() { }
#else
    protected override bool OnApply() { return false; }
#endif
  }


  internal sealed class WrappedDualTextureEffect : DualTextureEffect
  {
    public WrappedDualTextureEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }
#if !MONOGAME
    protected override void OnApply() { }
#else
    protected override bool OnApply() { return false; }
#endif
  }


  internal sealed class WrappedEnvironmentMapEffect : EnvironmentMapEffect
  {
    public WrappedEnvironmentMapEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }
#if !MONOGAME
    protected override void OnApply() { }
#else
    protected override bool OnApply() { return false; }
#endif
  }


  internal sealed class WrappedSkinnedEffect : SkinnedEffect
  {
    public WrappedSkinnedEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }
#if !MONOGAME
    protected override void OnApply() { }
#else
    protected override bool OnApply() { return false; }
#endif
  }


  internal static class StockEffectWrapper
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static AlphaTestEffect GetAlphaTestEffect(this IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__WrappedAlphaTestEffect";
      object effect;
      graphicsService.Data.TryGetValue(key, out effect);
      var instance = effect as WrappedAlphaTestEffect;
      if (instance == null)
      {
        instance = new WrappedAlphaTestEffect(graphicsService.GraphicsDevice);
        graphicsService.Data[key] = instance;
      }
      return instance;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static BasicEffect GetBasicEffect(this IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__WrappedBasicEffect";
      object effect;
      graphicsService.Data.TryGetValue(key, out effect);
      var instance = effect as WrappedBasicEffect;
      if (instance == null)
      {
        instance = new WrappedBasicEffect(graphicsService.GraphicsDevice);
        graphicsService.Data[key] = instance;
      }
      return instance;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static DualTextureEffect GetDualTextureEffect(this IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__WrappedDualTextureEffect";
      object effect;
      graphicsService.Data.TryGetValue(key, out effect);
      var instance = effect as WrappedDualTextureEffect;
      if (instance == null)
      {
        instance = new WrappedDualTextureEffect(graphicsService.GraphicsDevice);
        graphicsService.Data[key] = instance;
      }
      return instance;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static EnvironmentMapEffect GetEnvironmentMapEffect(this IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__WrappedEnvironmentMapEffect";
      object effect;
      graphicsService.Data.TryGetValue(key, out effect);
      var instance = effect as WrappedEnvironmentMapEffect;
      if (instance == null)
      {
        instance = new WrappedEnvironmentMapEffect(graphicsService.GraphicsDevice);
        graphicsService.Data[key] = instance;
      }
      return instance;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static SkinnedEffect GetSkinnedEffect(this IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__WrappedSkinnedEffect";
      object effect;
      graphicsService.Data.TryGetValue(key, out effect);
      var instance = effect as WrappedSkinnedEffect;
      if (instance == null)
      {
        instance = new WrappedSkinnedEffect(graphicsService.GraphicsDevice);
        graphicsService.Data[key] = instance;
      }
      return instance;
    }
  }
}
