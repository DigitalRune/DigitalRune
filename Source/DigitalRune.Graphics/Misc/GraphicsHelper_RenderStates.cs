// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  public static partial class GraphicsHelper
  {
    /// <summary>
    /// A default state object for disabled depth buffer writes and a depth buffer function of 
    /// "Equal".
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// DepthBufferWriteEnable = false<br/>
    /// DepthBufferFunction = CompareFunction.Equal
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly DepthStencilState DepthStencilStateNoWriteEqual = new DepthStencilState
    {
      Name = "GraphicsHelper.DepthStencilStateNoWriteEqual",
      DepthBufferWriteEnable = false,
      DepthBufferFunction = CompareFunction.Equal,
    };


    /// <summary>
    /// A default state object for disabled depth buffer writes and a depth buffer function of 
    /// "LessEqual".
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// DepthBufferWriteEnable = false<br/>
    /// DepthBufferFunction = CompareFunction.LessEqual
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly DepthStencilState DepthStencilStateNoWriteLessEqual = new DepthStencilState
    {
      Name = "GraphicsHelper.DepthStencilStateNoWriteLessEqual",
      DepthBufferWriteEnable = false,
      DepthBufferFunction = CompareFunction.LessEqual,
    };


    /// <summary>
    /// A default state object for disabled depth buffer writes and a depth buffer function of 
    /// "GreaterEqual".
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// DepthBufferWriteEnable = false<br/>
    /// DepthBufferFunction = CompareFunction.GreaterEqual
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly DepthStencilState DepthStencilStateNoWriteGreaterEqual = new DepthStencilState
    {
      Name = "GraphicsHelper.DepthStencilStateNoWriteGreaterEqual",
      DepthBufferWriteEnable = false,
      DepthBufferFunction = CompareFunction.GreaterEqual,
    };


    /// <summary>
    /// A default state object for disabled depth buffer writes and a depth buffer function of 
    /// "Greater".
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// DepthBufferWriteEnable = false<br/>
    /// DepthBufferFunction = CompareFunction.Greater
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly DepthStencilState DepthStencilStateNoWriteGreater = new DepthStencilState
    {
      Name = "GraphicsHelper.DepthStencilStateNoWriteGreater",
      DepthBufferWriteEnable = false,
      DepthBufferFunction = CompareFunction.Greater,
    };


    /// <summary>
    /// A default state object for enabled depth buffer writes and a disabled depth buffer test.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// DepthBufferWriteEnable = true <br/>
    /// DepthBufferFunction = CompareFunction.Always
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly DepthStencilState DepthStencilStateAlways = new DepthStencilState
    {
      Name = "GraphicsHelper.DepthStencilStateAlways",
      DepthBufferWriteEnable = true,
      DepthBufferFunction = CompareFunction.Always,
    };


    /// <summary>
    /// A default state object for rendering stencil volumes using the single pass Z-fail
    /// algorithm (Carmack's Reverse).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    internal static readonly DepthStencilState DepthStencilStateOnePassStencilFail = new DepthStencilState
    {
      Name = "GraphicsHelper.DepthStencilStateOnePassStencilFail",
      DepthBufferEnable = true,
      DepthBufferFunction = CompareFunction.LessEqual,
      DepthBufferWriteEnable = false,

      StencilEnable = true,
      TwoSidedStencilMode = true,
      ReferenceStencil = 0,
      StencilMask = ~0,
      StencilWriteMask = ~0,

      StencilFunction = CompareFunction.Always,
      StencilFail = StencilOperation.Keep,
      StencilDepthBufferFail = StencilOperation.Decrement,
      StencilPass = StencilOperation.Keep,

      CounterClockwiseStencilFunction = CompareFunction.Always,
      CounterClockwiseStencilFail = StencilOperation.Keep,
      CounterClockwiseStencilDepthBufferFail = StencilOperation.Increment,
      CounterClockwiseStencilPass = StencilOperation.Keep,
    };


    /// <summary>
    /// A default state object for rendering where the stencil is not 0 and also resetting the
    /// stencil.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    internal static readonly DepthStencilState DepthStencilStateStencilNotEqual0 = new DepthStencilState
    {
      Name = "GraphicsHelper.DepthStencilStateStencilNotEqual0",
      DepthBufferEnable = false,
      DepthBufferWriteEnable = false,

      StencilEnable = true,
      TwoSidedStencilMode = false,
      ReferenceStencil = 0,
      StencilMask = ~0,
      StencilWriteMask = ~0,

      StencilFunction = CompareFunction.NotEqual,
      StencilFail = StencilOperation.Keep,
      StencilDepthBufferFail = StencilOperation.Keep,
      StencilPass = StencilOperation.Zero,
    };


    /// <summary>
    /// A default state object for rendering where the stencil is 0 and also resetting the stencil.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    internal static readonly DepthStencilState DepthStencilStateStencilEqual0 = new DepthStencilState
    {
      Name = "GraphicsHelper.DepthStencilStateStencilEqual0",
      DepthBufferEnable = false,
      DepthBufferWriteEnable = false,

      StencilEnable = true,
      TwoSidedStencilMode = false,
      ReferenceStencil = 0,
      StencilMask = ~0,
      StencilWriteMask = ~0,

      StencilFunction = CompareFunction.Equal,
      StencilFail = StencilOperation.Zero,
      StencilDepthBufferFail = StencilOperation.Keep,
      StencilPass = StencilOperation.Keep,
    };


    /// <summary>
    /// A default state object for additive blending (colors and alpha values are accumulated).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// AlphaBlendFunction = BlendFunction.Add<br/>
    /// AlphaDestinationBlend = Blend.One<br/>
    /// AlphaSourceBlend = Blend.One<br/>
    /// ColorBlendFunction = BlendFunction.Add<br/>
    /// ColorDestinationBlend = Blend.One<br/>
    /// ColorSourceBlend = Blend.One
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly BlendState BlendStateAdd = new BlendState
    {
      Name = "GraphicsHelper.BlendStateAdd",
      AlphaBlendFunction = BlendFunction.Add,
      AlphaDestinationBlend = Blend.One,
      AlphaSourceBlend = Blend.One,
      ColorBlendFunction = BlendFunction.Add,
      ColorDestinationBlend = Blend.One,
      ColorSourceBlend = Blend.One,
    };


    /// <summary>
    /// A default state object for multiplicative blending (colors and alpha values are multiplied).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// AlphaBlendFunction = BlendFunction.Add<br/>
    /// AlphaDestinationBlend = Blend.SourceAlpha<br/>
    /// AlphaSourceBlend = Blend.Zero<br/>
    /// ColorBlendFunction = BlendFunction.Add<br/>
    /// ColorDestinationBlend = Blend.SourceColor<br/>
    /// ColorSourceBlend = Blend.Zero
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly BlendState BlendStateMultiply = new BlendState
    {
      Name = "GraphicsHelper.BlendStateMultiply",
      AlphaBlendFunction = BlendFunction.Add,
      AlphaDestinationBlend = Blend.SourceAlpha,
      AlphaSourceBlend = Blend.Zero,
      ColorBlendFunction = BlendFunction.Add,
      ColorDestinationBlend = Blend.SourceColor,
      ColorSourceBlend = Blend.Zero,
    };


    /// <summary>
    /// A default state object for disabled color writes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// ColorWriteChannels = ColorWriteChannels.None
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly BlendState BlendStateNoColorWrite = new BlendState
    {
      Name = "GraphicsHelper.BlendStateNoColorWrite",
      ColorWriteChannels = ColorWriteChannels.None,
    };


    /// <summary>
    /// A default state object for color writes in the red channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// ColorWriteChannels = ColorWriteChannels.Red
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly BlendState BlendStateWriteRed = new BlendState
    {
      Name = "GraphicsHelper.BlendStateWriteRed",
      ColorWriteChannels = ColorWriteChannels.Red,
    };


    /// <summary>
    /// A default state object for color writes in the blue channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// ColorWriteChannels = ColorWriteChannels.Blue
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly BlendState BlendStateWriteBlue = new BlendState
    {
      Name = "GraphicsHelper.BlendStateWriteBlue",
      ColorWriteChannels = ColorWriteChannels.Blue,
    };


    /// <summary>
    /// A default state object for color writes in the green channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// ColorWriteChannels = ColorWriteChannels.Green
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly BlendState BlendStateWriteGreen = new BlendState
    {
      Name = "GraphicsHelper.BlendStateWriteGreen",
      ColorWriteChannels = ColorWriteChannels.Green,
    };


    /// <summary>
    /// A default state object for color writes in the alpha channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// ColorWriteChannels = ColorWriteChannels.Alpha
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly BlendState BlendStateWriteAlpha = new BlendState
    {
      Name = "GraphicsHelper.BlendStateWriteAlpha",
      ColorWriteChannels = ColorWriteChannels.Alpha,
    };


    /// <summary>
    /// A 4-element array containing <see cref="BlendStateWriteRed"/>, 
    /// <see cref="BlendStateWriteGreen"/>, <see cref="BlendStateWriteBlue"/> and
    /// <see cref="BlendStateWriteAlpha"/>.
    /// </summary>
    internal static readonly BlendState[] BlendStateWriteSingleChannel =
    {
      BlendStateWriteRed,
      BlendStateWriteGreen,
      BlendStateWriteBlue,
      BlendStateWriteAlpha
    };


    /// <summary>
    /// A rasterizer state object with settings for culling primitives with clockwise winding order.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This state is identical to the built-in state <see cref="RasterizerState.CullClockwise"/> in
    /// XNA. The only difference is that <see cref="RasterizerState.MultiSampleAntiAlias"/> is set
    /// to <see langword="false"/>, which improves line rendering when MSAA is disabled.
    /// </para>
    /// <para>This instance must not be modified!</para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WireFrame", Justification = "Consistent with XNA.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly RasterizerState RasterizerStateCullClockwise = new RasterizerState
    {
      Name = "GraphicsHelper.RasterizerStateCullClockwise",
      CullMode = CullMode.CullClockwiseFace,
      MultiSampleAntiAlias = false
    };


    /// <summary>
    /// A rasterizer state object with settings for culling primitives with counter-clockwise
    /// winding order.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This state is identical to the built-in state
    /// <see cref="RasterizerState.CullCounterClockwise"/> in XNA. The only difference is that
    /// <see cref="RasterizerState.MultiSampleAntiAlias"/> is set to <see langword="false"/>, which
    /// improves line rendering when MSAA is disabled.
    /// </para>
    /// <para>This instance must not be modified!</para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WireFrame", Justification = "Consistent with XNA.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly RasterizerState RasterizerStateCullCounterClockwise = new RasterizerState
    {
      Name = "GraphicsHelper.RasterizerStateCullCounterClockwise",
      CullMode = CullMode.CullCounterClockwiseFace,
      MultiSampleAntiAlias = false
    };


    /// <summary>
    /// A rasterizer state object with settings for not culling primitives.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This state is identical to the built-in state <see cref="RasterizerState.CullNone"/> in XNA.
    /// The only difference is that <see cref="RasterizerState.MultiSampleAntiAlias"/> is set to
    /// <see langword="false"/>, which improves line rendering when MSAA is disabled.
    /// </para>
    /// <para>This instance must not be modified!</para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WireFrame", Justification = "Consistent with XNA.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly RasterizerState RasterizerStateCullNone = new RasterizerState
    {
      Name = "GraphicsHelper.RasterizerStateCullNone",
      CullMode = CullMode.None,
      MultiSampleAntiAlias = false
    };


    /// <summary>
    /// A default state object for wire-frame rendering.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default state object has the following settings:
    /// </para>
    /// <para>
    /// <c>
    /// CullMode = CullMode.None<br/>
    /// FillMode = FillMode.WireFrame<br/>
    /// MultiSampleAntiAlias = false
    /// </c>
    /// </para>
    /// <para>
    /// This instance must not be modified!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WireFrame", Justification = "Consistent with XNA.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly RasterizerState RasterizerStateWireFrame = new RasterizerState
    {
      Name = "GraphicsHelper.RasterizerStateWireFrame",
      CullMode = CullMode.None,
      FillMode = FillMode.WireFrame,
      MultiSampleAntiAlias = false
    };
  }
}
