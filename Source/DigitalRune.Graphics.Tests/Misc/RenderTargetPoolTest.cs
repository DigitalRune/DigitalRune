using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;


namespace DigitalRune.Graphics.Tests
{
  [TestFixture]
  public class RenderTargetPoolTest
  {
    private GraphicsDevice _graphicsDevice;
    private IGraphicsService _graphicsService;

    [SetUp]
    public void SetUp()
    {
      var form = new Form();
      var parameters = new PresentationParameters
      {
        BackBufferWidth = 1280,
        BackBufferHeight = 720,
        BackBufferFormat = SurfaceFormat.Color,
        DepthStencilFormat = DepthFormat.Depth24Stencil8,
        DeviceWindowHandle = form.Handle,
        PresentationInterval = PresentInterval.Immediate,
        IsFullScreen = false
      };

      _graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);
      _graphicsService = new GraphicsManager(_graphicsDevice, new ContentManager(new GameServiceContainer()));
    }

    [TearDown]
    public void TearDown()
    {
      _graphicsDevice.Dispose();
    }


    [Test]
    public void Properties()
    {
      var p = new RenderTargetPool(_graphicsService);
      Assert.AreEqual(p.GraphicsService, _graphicsService);
      Assert.IsTrue(p.FrameLimit > 0);
    }


    [Test]
    public void Clear()
    {
      var p = new RenderTargetPool(_graphicsService);
      var r0 = p.Obtain2D(new RenderTargetFormat(100, 100, false, SurfaceFormat.Color, DepthFormat.None));
      var r1 = p.Obtain2D(new RenderTargetFormat(100, 100, false, SurfaceFormat.Color, DepthFormat.None));

      p.Recycle(r0);
      p.Recycle(r1);

      //Assert.IsFalse(r0.IsDisposed);      // Not disposing RT anymore because of XNA bug.
      //Assert.IsFalse(r1.IsDisposed);

      p.Clear();

      //Assert.IsTrue(r0.IsDisposed);
      //Assert.IsTrue(r1.IsDisposed);
      Assert.AreEqual(0, p.RenderTargets2D.Count);
      Assert.AreEqual(0, p.Counters2D.Count);
    }


    [Test]
    public void FrameLimit()
    {
      var p = new RenderTargetPool(_graphicsService);
      p.FrameLimit = 5;

      var r0 = p.Obtain2D(new RenderTargetFormat(100, 100, false, SurfaceFormat.Color, DepthFormat.None));
      var r1 = p.Obtain2D(new RenderTargetFormat(100, 100, false, SurfaceFormat.Color, DepthFormat.None));

      Assert.IsFalse(r0.IsDisposed);
      Assert.IsFalse(r1.IsDisposed);

      p.Recycle(r0);

      p.Update();
      p.Update();

      p.Recycle(r1);

      p.Update();
      p.Update();
      
      Assert.IsFalse(r0.IsDisposed);
      Assert.IsFalse(r1.IsDisposed);
      Assert.AreEqual(2, p.Counters2D.Count);

      p.Update();
      p.Update();

      Assert.IsTrue(r0.IsDisposed);
      Assert.IsFalse(r1.IsDisposed);
      Assert.AreEqual(1, p.Counters2D.Count);

      p.Update();
      p.Update();

      Assert.IsTrue(r0.IsDisposed);
      Assert.IsTrue(r1.IsDisposed);
      Assert.AreEqual(0, p.Counters2D.Count);
    }


    [Test]
    public void ObtainRecycle()
    {
      var p = new RenderTargetPool(_graphicsService);
      p.FrameLimit = 5;

      var r0 = p.Obtain2D(new RenderTargetFormat(32, 32, false, SurfaceFormat.Color, DepthFormat.None));
      var r1 = p.Obtain2D(new RenderTargetFormat(32, 64, false, SurfaceFormat.Color, DepthFormat.None));
      var r2 = p.Obtain2D(new RenderTargetFormat(32, 32, true, SurfaceFormat.Color, DepthFormat.None));
      var r3 = p.Obtain2D(new RenderTargetFormat(32, 32, false, SurfaceFormat.Single, DepthFormat.None));
      var r4 = p.Obtain2D(new RenderTargetFormat(32, 32, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8));

      p.Recycle(r0);
      p.Recycle(r1);
      p.Recycle(r2);
      p.Recycle(r3);
      p.Recycle(r4);

      Assert.AreEqual(r0, p.Obtain2D(new RenderTargetFormat(32, 32, false, SurfaceFormat.Color, DepthFormat.None)));
      Assert.AreNotEqual(r0, p.Obtain2D(new RenderTargetFormat(32, 32, false, SurfaceFormat.Color, DepthFormat.None)));
      p.Recycle(r0);

      Assert.AreEqual(r2, p.Obtain2D(new RenderTargetFormat(32, 32, true, SurfaceFormat.Color, DepthFormat.None)));
      Assert.AreNotEqual(r2, p.Obtain2D(new RenderTargetFormat(32, 32, true, SurfaceFormat.Color, DepthFormat.None)));
      p.Recycle(r2);

      Assert.AreEqual(r3, p.Obtain2D(new RenderTargetFormat(32, 32, false, SurfaceFormat.Single, DepthFormat.None)));
      Assert.AreNotEqual(r3, p.Obtain2D(new RenderTargetFormat(32, 32, false, SurfaceFormat.Single, DepthFormat.None)));
      p.Recycle(r3);

      Assert.AreEqual(r1, p.Obtain2D(new RenderTargetFormat(32, 64, false, SurfaceFormat.Color, DepthFormat.None)));
      Assert.AreNotEqual(r1, p.Obtain2D(new RenderTargetFormat(32, 64, false, SurfaceFormat.Color, DepthFormat.None)));
      p.Recycle(r1);

      Assert.AreEqual(r4, p.Obtain2D(new RenderTargetFormat(32, 32, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8)));
      Assert.AreNotEqual(r4, p.Obtain2D(new RenderTargetFormat(32, 32, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8)));
      p.Recycle(r4);
    }


    [Test]
    public void ValidateDuplicateRecycle2D()
    {
      GlobalSettings.ValidationLevel = 0;
      var p = new RenderTargetPool(_graphicsService);
      var r0 = p.Obtain2D(new RenderTargetFormat(32, 32, false, SurfaceFormat.Color, DepthFormat.None));
      p.Recycle(r0);
      p.Recycle(r0);

      GlobalSettings.ValidationLevel = 0xff;
      p = new RenderTargetPool(_graphicsService);
      p.Recycle(r0);
      Assert.Throws<InvalidOperationException>(() => p.Recycle(r0));
    }


    [Test]
    public void ValidateDuplicateRecycleCube()
    {
      GlobalSettings.ValidationLevel = 0;
      var p = new RenderTargetPool(_graphicsService);
      var r0 = p.ObtainCube(new RenderTargetFormat(32, 32, false, SurfaceFormat.Color, DepthFormat.None));
      p.Recycle(r0);
      p.Recycle(r0);

      GlobalSettings.ValidationLevel = 0xff;
      p = new RenderTargetPool(_graphicsService);
      p.Recycle(r0);
      Assert.Throws<InvalidOperationException>(() => p.Recycle(r0));
    }
  }
}