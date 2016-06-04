using System;
using NUnit.Framework;
using Rhino.Mocks;


namespace DigitalRune.Graphics.Tests
{
  internal class MyGraphicsScreen : GraphicsScreen
  {
    public MyGraphicsScreen(IGraphicsService graphicsService) : base(graphicsService) { }
    protected override void OnUpdate(TimeSpan deltaTime) { }
    protected override void OnRender(RenderContext context) { }
  }


  [TestFixture]
  public class GraphicsScreenTest
  {
    [Test]
    public void DefaultConstructor()
    {
      GraphicsScreenCollection graphicsScreenCollection = new GraphicsScreenCollection();
      Assert.AreEqual(0, graphicsScreenCollection.Count);
    }


    [Test]
    public void Constructor()
    {
      var graphicsServiceStub = MockRepository.GenerateStub<IGraphicsService>();

      var graphicsScreen0 = new MyGraphicsScreen(graphicsServiceStub);
      var graphicsScreen1 = new MyGraphicsScreen(graphicsServiceStub);
      var graphicsScreen2 = new MyGraphicsScreen(graphicsServiceStub);
      GraphicsScreenCollection graphicsScreenCollection = new GraphicsScreenCollection 
      { 
        graphicsScreen0, 
        graphicsScreen1, 
        graphicsScreen2 
      };
      Assert.AreEqual(3, graphicsScreenCollection.Count);
      Assert.AreSame(graphicsScreen0, graphicsScreenCollection[0]);
      Assert.AreSame(graphicsScreen1, graphicsScreenCollection[1]);
      Assert.AreSame(graphicsScreen2, graphicsScreenCollection[2]);
    }
  }
}
