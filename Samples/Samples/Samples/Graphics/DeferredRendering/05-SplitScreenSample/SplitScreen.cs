#if !WP7 && !WP8
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples
{
  // Derives from the DeferredGraphicsScreen and changes the OnRender method
  // to render the scene twice for a split-screen game..
  sealed class SplitScreen : DeferredGraphicsScreen
  {
    // The camera for the second player.
    public CameraNode ActiveCameraNodeB { get; set; }


    public SplitScreen(IServiceLocator services)
      : base(services)
    {
    }


    protected override void OnRender(RenderContext context)
    {
      // This screen expects two cameras.
      if (ActiveCameraNode == null || ActiveCameraNodeB == null)
        return;

      var renderTargetPool = GraphicsService.RenderTargetPool;
      var graphicsDevice = GraphicsService.GraphicsDevice;
      var originalRenderTarget = context.RenderTarget;
      var fullViewport = context.Viewport;

      // Get a render target for the first camera. Use half the width because we split
      // the screen horizontally.
      var format = new RenderTargetFormat(context.RenderTarget)
      {
        Width = fullViewport.Width / 2
      };
      var renderTargetA = renderTargetPool.Obtain2D(format);

      context.Scene = Scene;
      context.LodHysteresis = 0.5f;
      context.LodBias = 1.0f;
      context.LodBlendingEnabled = true;

      for (int i = 0; i < 2; i++)
      {
        if (i == 0)
        {
          // The first camera renders into renderTargetA.
          context.CameraNode = ActiveCameraNode;
          context.Viewport = new Viewport(0, 0, fullViewport.Width / 2, fullViewport.Height);
          context.RenderTarget = renderTargetA;
        }
        else
        {
          // The second camera renders into the right half of the final render target.
          context.CameraNode = ActiveCameraNodeB;
          context.Viewport = new Viewport(fullViewport.X + fullViewport.Width / 2, fullViewport.Y, fullViewport.Width / 2, fullViewport.Height);
          context.RenderTarget = originalRenderTarget;
        }
        context.LodCameraNode = context.CameraNode;

        // Get all scene nodes which overlap the camera frustum.
        CustomSceneQuery sceneQuery = Scene.Query<CustomSceneQuery>(context.CameraNode, context);

        // Render the scene nodes of the sceneQuery.
        RenderScene(sceneQuery, context, true, true, true, true);

        // ----- Copy image of first camera.
        if (i == 1)
        {
          // Copy the upper screen from the temporary render target back into the back buffer.
          context.Viewport = fullViewport;
          graphicsDevice.Viewport = fullViewport;

          SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
          SpriteBatch.Draw(
            renderTargetA,
            new Rectangle(0, 0, fullViewport.Width / 2, fullViewport.Height),
            Color.White);
          SpriteBatch.End();

          renderTargetPool.Recycle(renderTargetA);
        }
      }

      // Clean-up
      context.Scene = null;
      context.CameraNode = null;
      context.LodCameraNode = null;
      context.RenderPass = null;
    }
  }
}
#endif