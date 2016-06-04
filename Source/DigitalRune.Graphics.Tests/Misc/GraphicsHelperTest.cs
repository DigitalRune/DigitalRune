using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;


namespace DigitalRune.Graphics.Tests
{
  [TestFixture]
  public class GraphicsHelperTest
  {
    private GraphicsDevice _graphicsDevice0;
    private GraphicsDevice _graphicsDevice1;
    private IGraphicsService _graphicsService0;
    private IGraphicsService _graphicsService1;

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

      _graphicsDevice0 = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);
      _graphicsDevice1 = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);
      _graphicsService0 = new GraphicsManager(_graphicsDevice0, new ContentManager(new GameServiceContainer()));
      _graphicsService1 = new GraphicsManager(_graphicsDevice1, new ContentManager(new GameServiceContainer()));
    }

    [TearDown]
    public void TearDown()
    {
      _graphicsDevice0.Dispose();
      _graphicsDevice1.Dispose();
    }


    [Test]
    public void DefaultTextures()
    {
      Assert.AreEqual(_graphicsService0.GetDefaultTexture2DBlack(), _graphicsService0.GetDefaultTexture2DBlack());
      Assert.AreNotEqual(_graphicsService0.GetDefaultTexture2DBlack(), _graphicsService1.GetDefaultTexture2DBlack());

      var t = _graphicsService1.GetDefaultTexture2DWhite();
      _graphicsDevice1.Dispose();
      
      // Note: Since the graphics device is also disposed and re-created when the game is
      // moved between screens - we must not auto-dispose our textures.
      //Assert.IsTrue(t.IsDisposed);
    }
  }
}