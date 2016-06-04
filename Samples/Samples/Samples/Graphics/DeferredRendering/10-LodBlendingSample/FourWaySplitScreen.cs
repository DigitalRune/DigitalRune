#if !WP7 && !WP8
using DigitalRune.Graphics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // This graphics screen renders 4 views using the same camera. Each view uses
  // different LOD settings:
  //
  //   TOP, LEFT:                        TOP, RIGHT:
  //     LODs are highlighted.             LODs are highlighted.
  //     LOD blending is disabled.         LOD blending is enabled.
  //                                    
  //   BOTTOM, LEFT:                     BOTTOM, RIGHT:
  //     Original LODs are rendered.       Original LODs are rendered.
  //     LOD blending is disabled.         LOD blending is enabled.
  //
  // Other than that, rendering is the same as in the DeferredGraphicsScreen.
  sealed class FourWaySplitScreen : DeferredGraphicsScreen
  {
    public FourWaySplitScreen(IServiceLocator services)
      : base(services)
    {
    }


    protected override void OnRender(RenderContext context)
    {
      if (ActiveCameraNode == null)
        return;

      var renderTargetPool = GraphicsService.RenderTargetPool;
      var graphicsDevice = GraphicsService.GraphicsDevice;
      var originalRenderTarget = context.RenderTarget;
      var fullViewport = context.Viewport;

      // Get a render target for the first camera. Use half the width and height.
      int halfWidth = fullViewport.Width / 2;
      int halfHeight = fullViewport.Height / 2;
      var format = new RenderTargetFormat(context.RenderTarget)
      {
        Width = halfWidth,
        Height = halfHeight
      };

      var renderTarget0 = renderTargetPool.Obtain2D(format);
      var renderTarget1 = renderTargetPool.Obtain2D(format);
      var renderTarget2 = renderTargetPool.Obtain2D(format);
      var viewport0 = new Viewport(0, 0, halfWidth, halfHeight);
      var viewport1 = new Viewport(halfWidth, 0, halfWidth, halfHeight);
      var viewport2 = new Viewport(0, halfHeight, halfWidth, halfHeight);

      context.Scene = Scene;
      context.CameraNode = ActiveCameraNode;
      context.LodCameraNode = context.CameraNode;
      context.LodHysteresis = 0.5f;

      // Reduce detail level by increasing the LOD bias.
      context.LodBias = 2.0f;

      for (int i = 0; i < 4; i++)
      {
        if (i == 0)
        {
          // TOP, LEFT
          context.RenderTarget = renderTarget0;
          context.Viewport = new Viewport(0, 0, viewport0.Width, viewport0.Height);
          context.LodBlendingEnabled = false;
        }
        else if (i == 1)
        {
          // TOP, RIGHT
          context.RenderTarget = renderTarget1;
          context.Viewport = new Viewport(0, 0, viewport1.Width, viewport1.Height);
          context.LodBlendingEnabled = true;
        }
        else if (i == 2)
        {
          // BOTTOM, LEFT
          context.RenderTarget = renderTarget2;
          context.Viewport = new Viewport(0, 0, viewport2.Width, viewport2.Height);
          context.LodBlendingEnabled = false;
        }
        else
        {
          // BOTTOM, RIGHT
          context.RenderTarget = originalRenderTarget;
          context.Viewport = new Viewport(fullViewport.X + halfWidth, fullViewport.Y + halfHeight, halfWidth, halfHeight);
          context.LodBlendingEnabled = true;
        }

        var sceneQuery = Scene.Query<SceneQueryWithLodBlending>(context.CameraNode, context);

        if (i == 0 || i == 1)
        {
          // TOP
          for (int j = 0; j < sceneQuery.RenderableNodes.Count; j++)
            if (sceneQuery.RenderableNodes[j].UserFlags == 1)
              sceneQuery.RenderableNodes[j] = null;
        }
        else
        {
          // BOTTOM
          for (int j = 0; j < sceneQuery.RenderableNodes.Count; j++)
            if (sceneQuery.RenderableNodes[j].UserFlags == 2)
              sceneQuery.RenderableNodes[j] = null;
        }

        RenderScene(sceneQuery, context, true, true, true, true);
        
        sceneQuery.Reset();
      }

      // ----- Copy screens.
      // Copy the previous screens from the temporary render targets into the back buffer.
      context.Viewport = fullViewport;
      graphicsDevice.Viewport = fullViewport;

      SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
      SpriteBatch.Draw(renderTarget0, viewport0.Bounds, Color.White);
      SpriteBatch.Draw(renderTarget1, viewport1.Bounds, Color.White);
      SpriteBatch.Draw(renderTarget2, viewport2.Bounds, Color.White);
      SpriteBatch.End();

      renderTargetPool.Recycle(renderTarget0);
      renderTargetPool.Recycle(renderTarget1);
      renderTargetPool.Recycle(renderTarget2);

      context.Scene = null;
      context.CameraNode = null;
      context.LodCameraNode = null;
      context.RenderPass = null;
    }
  }
}
#endif