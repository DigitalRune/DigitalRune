// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Stores/restores render states.
  /// </summary>
  /// <remarks>
  /// This struct type gets a snapshot of the current graphics device render states when it is
  /// created. It restores the saved render states when <see cref="Restore"/> is called. 
  /// Only the following states of the <see cref="GraphicsDevice"/> are stored: 
  /// <see cref="BlendState"/>, <see cref="DepthStencilState"/> and <see cref="RasterizerState"/>.
  /// </remarks>
  internal struct RenderStateSnapshot
  {
    private readonly GraphicsDevice _graphicsDevice;
    private readonly BlendState _blendState;
    private readonly DepthStencilState _depthStencilState;
    private readonly RasterizerState _rasterizerState;
    
    // Note: It is not good to save the sampler states because following can happen:
    // A float texture is set in a texture stage (e.g. a light buffer) and a previous effect has
    // changed the sampler state so that GraphicsDevice.SamplerStates[x] == null. If the 
    // next effect does not use this texture slot, everything is fine. But if we restore the
    // sampler state to LinearWrap, then an exception will be thrown. 
    //private readonly SamplerState _samplerState0;
    //private readonly SamplerState _samplerState1;


    /// <summary>
    /// Initializes a new instance of the <see cref="RenderStateSnapshot" /> struct.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <exception cref="ArgumentNullException">graphicsDevice</exception>
    public RenderStateSnapshot(GraphicsDevice graphicsDevice)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");

      _graphicsDevice = graphicsDevice;
      _blendState = graphicsDevice.BlendState;
      _depthStencilState = graphicsDevice.DepthStencilState;
      _rasterizerState = graphicsDevice.RasterizerState;
      //_samplerState0 = graphicsDevice.SamplerStates[0];
      //_samplerState1 = graphicsDevice.SamplerStates[1];
    }


    /// <summary>
    /// Restores the graphics device state that was stored in the constructor of this instance.
    /// </summary>
    public void Restore()
    {
      // Reset state. Attention: We must not set null. But null could be read
      // in the constructor if a render state was set in an .fx file.
      _graphicsDevice.BlendState = _blendState ?? BlendState.Opaque;
      _graphicsDevice.DepthStencilState = _depthStencilState ?? DepthStencilState.Default;
      _graphicsDevice.RasterizerState = _rasterizerState ?? RasterizerState.CullCounterClockwise;
      //_graphicsDevice.SamplerStates[0] = _samplerState0 ?? SamplerState.LinearWrap;
      //_graphicsDevice.SamplerStates[1] = _samplerState1 ?? SamplerState.LinearWrap;
    }
  }
}
