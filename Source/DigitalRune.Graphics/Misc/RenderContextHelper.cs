// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides helper methods for the <see cref="RenderContext"/> type.
  /// </summary>
  internal static class RenderContextHelper
  {
    /// <summary>
    /// Determines whether the current <see cref="RenderContext.RenderTarget"/> is 
    /// <see cref="SurfaceFormat.HdrBlendable"/>.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <returns>
    /// <see langword="true" /> if the current <see cref="RenderContext.RenderTarget"/> is 
    /// <see cref="SurfaceFormat.HdrBlendable"/>; otherwise, <see langword="false" />.
    /// </returns>
    internal static bool IsHdrEnabled(this RenderContext context)
    {
      return context.RenderTarget != null 
             && context.RenderTarget.Format == SurfaceFormat.HdrBlendable;
    }


    /// <summary>
    /// Validates the render context of the specified graphics resource. (Throws an exception if the
    /// graphics device is invalid.)
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="resource">The graphics resource.</param>
    /// <exception cref="GraphicsException">
    /// Invalid render context.
    /// </exception>
    [Conditional("DEBUG")]
    internal static void Validate(this RenderContext context, GraphicsResource resource)
    {
      if (context.GraphicsService == null)
        throw new GraphicsException("Invalid render context: Graphics service is not set.");

      if (resource != null && resource.GraphicsDevice != context.GraphicsService.GraphicsDevice)
        throw new GraphicsException("Invalid render context: Wrong graphics device.");
    }


    /// <summary>
    /// Throws a <see cref="GraphicsException"/> if <see cref="RenderContext.CameraNode"/> is not 
    /// set.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <exception cref="GraphicsException">
    /// The camera node is not set in render context.
    /// </exception>
    internal static void ThrowIfCameraMissing(this RenderContext context)
    {
      if (context.CameraNode == null)
        throw new GraphicsException("Camera node needs to be set in render context.");
    }


    /// <summary>
    /// Throws a <see cref="GraphicsException"/> if <see cref="RenderContext.LodCameraNode"/> is not 
    /// set.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <exception cref="GraphicsException">
    /// The LOD camera node is not set in render context.
    /// </exception>
    internal static void ThrowIfLodCameraMissing(this RenderContext context)
    {
      if (context.LodCameraNode == null)
        throw new GraphicsException("LOD camera node needs to be set in render context.");
    }


    /// <summary>
    /// Throws a <see cref="GraphicsException" /> if <see cref="RenderContext.GBuffer0"/> is not 
    /// set.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <exception cref="GraphicsException">
    /// G-buffer 0 is not set in render context.
    /// </exception>
    internal static void ThrowIfGBuffer0Missing(this RenderContext context)
    {
      if (context.GBuffer0 == null)
        throw new GraphicsException("GBuffer0 needs to be set in render context.");
    }


    /// <summary>
    /// Throws a <see cref="GraphicsException" /> if <see cref="RenderContext.GBuffer1"/> is not 
    /// set.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <exception cref="GraphicsException">
    /// G-buffer 1 is not set in render context.
    /// </exception>
    internal static void ThrowIfGBuffer1Missing(this RenderContext context)
    {
      if (context.GBuffer1 == null)
        throw new GraphicsException("GBuffer1 needs to be set in render context.");
    }


    /// <summary>
    /// Throws a <see cref="GraphicsException" /> if <see cref="RenderContext.RenderPass"/> is not 
    /// set.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <exception cref="GraphicsException">
    /// Render pass is not set in render context.
    /// </exception>
    internal static void ThrowIfRenderPassMissing(this RenderContext context)
    {
      if (context.RenderPass == null)
        throw new GraphicsException("Render pass needs to be set in render context.");
    }


    /// <summary>
    /// Throws a <see cref="GraphicsException" /> if <see cref="RenderContext.Scene"/> is not set.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <exception cref="GraphicsException">
    /// Scene is not set in render context.
    /// </exception>
    internal static void ThrowIfSceneMissing(this RenderContext context)
    {
      if (context.Scene == null)
        throw new GraphicsException("Scene needs to be set in render context.");
    }
  }
}
