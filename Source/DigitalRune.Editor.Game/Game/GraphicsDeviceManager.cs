// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Implements <see cref="IGraphicsDeviceService"/> which is needed by the MonoGame/XNA content
    /// pipeline for loading assets.
    /// </summary>
    internal class GraphicsDeviceManager : IGraphicsDeviceService
    {
        public GraphicsDevice GraphicsDevice { get; }


#pragma warning disable 67
        // Not used:
        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
#pragma warning restore 67


        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDeviceManager"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        public GraphicsDeviceManager(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }
    }
}
