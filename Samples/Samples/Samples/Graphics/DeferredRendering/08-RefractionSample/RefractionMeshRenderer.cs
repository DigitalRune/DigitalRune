#if !WP7 && !WP8
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;

namespace Samples.Graphics
{
  // Wraps a MeshRenderer. If a mesh should be rendered which uses the "SourceTexture" effect 
  // parameter, the current back buffer is copied to a texture before the object is rendered.
  public class RefractionMeshRenderer : SceneNodeRenderer
  {
    private readonly List<SceneNode> _tempList = new List<SceneNode>();
    private readonly MeshRenderer _meshRenderer;


    public RefractionMeshRenderer(IGraphicsService graphicsService)
    {
      _meshRenderer = new MeshRenderer();
    }


    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return _meshRenderer.CanRender(node, context);
    }


    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      var graphicsService = context.GraphicsService;
      var renderTargetPool = graphicsService.RenderTargetPool;
      var graphicsDevice = graphicsService.GraphicsDevice;

      // Get a shared RebuildZBufferRenderer which was added by the graphics screen.
      var rebuildZBufferRenderer = (RebuildZBufferRenderer)context.Data[RenderContextKeys.RebuildZBufferRenderer];
      
      // We only support a render order of "user defined". This is always the case 
      // if this renderer is added to a SceneRenderer. The SceneRenderer does the sorting.
      Debug.Assert(order == RenderOrder.UserDefined);

      // This renderer assumes that the current render target is an off-screen render target.
      Debug.Assert(context.RenderTarget != null);

      graphicsDevice.ResetTextures();

      // Remember the format of the current render target.
      var backBufferFormat = new RenderTargetFormat(context.RenderTarget);

      // In the loop below we will use the context.SourceTexture property. 
      // Remember the original source texture. 
      var originalSourceTexture = context.SourceTexture;
      
      context.SourceTexture = null;
      for (int i = 0; i < nodes.Count; i++)
      {
        var node = (MeshNode)nodes[i];

        // Check if the next node wants to sample from the back buffer.
        if (RequiresSourceTexture(node, context))
        {
          // The effect of the node wants to sample from the "SourceTexture". 
          // Per default, DigitalRune Graphics uses a delegate effect parameter 
          // binding to set the "SourceTexture" parameters to the 
          // RenderContext.SourceTexture value. However, this property is usually 
          // null. We need to manually set RenderContext.SourceTexture to the 
          // current back buffer render target. Since, we cannot read from this
          // render target and write to this render target at the same time,
          // we have to copy it.

          context.SourceTexture = context.RenderTarget;

          // Set a new render target and copy the content of the lastBackBuffer
          // and the depth buffer.
          context.RenderTarget = renderTargetPool.Obtain2D(backBufferFormat);
          graphicsDevice.SetRenderTarget(context.RenderTarget);
          graphicsDevice.Viewport = context.Viewport;
          rebuildZBufferRenderer.Render(context, context.SourceTexture);
        }

        // Add current node to a temporary list.
        _tempList.Add(node);

        // Add all following nodes until another node wants to sample from the
        // back buffer.
        for (int j = i + 1; j < nodes.Count; j++)
        {
          node = (MeshNode)nodes[j];

          if (RequiresSourceTexture(node, context))
            break;

          _tempList.Add(node);
          i++;
        }

        // Render nodes.
        _meshRenderer.Render(_tempList, context);

        renderTargetPool.Recycle(context.SourceTexture);
        context.SourceTexture = null;

        _tempList.Clear();
      }

      // Restore original render context.
      context.SourceTexture = originalSourceTexture;
    }


    // Checks if an effect of the uses the "SourceTexture" effect parameter.
    // This method assumes that the parameter "SourceTexture" is a "global" parameter
    // which means that it is stored with the effect (not in the Materials and not
    // in the MaterialInstances). 
    // TODO: To improve performance, make this check only once and store a flag in the
    // SceneNode.UserFlags.
    private bool RequiresSourceTexture(MeshNode meshNode, RenderContext context)
    {
      foreach (var material in meshNode.Mesh.Materials)
      {
        EffectBinding effectBinding;
        if (material.TryGet(context.RenderPass, out effectBinding))
        {
          var effect = material[context.RenderPass].Effect;
          var parameterBindings = effect.GetParameterBindings();
          foreach (var binding in parameterBindings)
            if (binding.Description.Semantic == DefaultEffectParameterSemantics.SourceTexture)
              return true;
        }
      }

      return false;
    }
  }
}
#endif