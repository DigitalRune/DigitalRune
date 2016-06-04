using System;
using Microsoft.Xna.Framework.Graphics;


namespace WpfInteropSample2
{
  /// <summary>
  /// Implements a simple <see cref="IGraphicsDeviceService"/> which is needed by the 
  /// MonoGame/XNA content pipeline for loading assets.
  /// </summary>
  internal class DummyGraphicsDeviceManager : IGraphicsDeviceService
  {
    public GraphicsDevice GraphicsDevice { get; private set; }


#pragma warning disable 67
    // Not used:
    public event EventHandler<EventArgs> DeviceCreated;
    public event EventHandler<EventArgs> DeviceDisposing;
    public event EventHandler<EventArgs> DeviceReset;
    public event EventHandler<EventArgs> DeviceResetting;
#pragma warning restore 67


    public DummyGraphicsDeviceManager(GraphicsDevice graphicsDevice)
    {
      GraphicsDevice = graphicsDevice;
    }
  }
}