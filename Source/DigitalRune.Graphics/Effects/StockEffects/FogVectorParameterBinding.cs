// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Binds an effect parameter to the XNA FogVector parameter.
  /// </summary>
  /// <remarks>
  /// This parameter binding also sets the FogEnabled flag of the effect bindings.
  /// This is possible because fog settings are global.
  /// </remarks>
  [DebuggerDisplay("FogVectorParameterBinding(Parameter = {Parameter.Name}, Value = {Value})")]
  internal sealed class FogVectorParameterBinding : EffectParameterBinding<Vector4>
  {
    // Note: Make default constructor protected, if class is unsealed.


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="FogVectorParameterBinding"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="FogVectorParameterBinding"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    private FogVectorParameterBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="FogVectorParameterBinding"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    public FogVectorParameterBinding(Effect effect, EffectParameter parameter)
      : base(effect, parameter)
    {
    }


    /// <inheritdoc/>
    protected override EffectParameterBinding CreateInstanceCore()
    {
      return new FogVectorParameterBinding();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnUpdate(RenderContext context)
    {
      var stockEffectBinding = context.MaterialBinding as IStockEffectBinding;
      if (stockEffectBinding == null)
        throw new EffectBindingException("XNA stock effect binding not found in render context.");

      // Fallback values.
      Value = new Vector4(0, 0, 0, 0);
      stockEffectBinding.FogEnabled = false;

      if (context.CameraNode == null)
        return;

      var nodes = SceneEffectBinder.QueryFogNodes(context);
      if (nodes == null)
        return;

      var node = nodes[0];
      var fog = node.Fog;

      if (Numeric.IsZero(fog.Density) || Numeric.IsZero(fog.Color0.W))
        return;

      Matrix worldView = SceneEffectBinder.GetWorldView(null, context);
      float fogStart = fog.Start;
      float fogEnd = fog.End;

      // This how XNA uses its fog vector.
      float x = 1f / (fogStart - fogEnd);
      Value = new Vector4(worldView.M13 * x,
                          worldView.M23 * x,
                          worldView.M33 * x,
                          (worldView.M43 + fogStart) * x);
      stockEffectBinding.FogEnabled = true;
    }


    /// <inheritdoc/>
    protected override void OnApply(RenderContext context)
    {
      Parameter.SetValue(Value);
    }
  }
}
