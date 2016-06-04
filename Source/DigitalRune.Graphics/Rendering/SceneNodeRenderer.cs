// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Base class of all scene node renderers.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="SceneNodeRenderer"/> renders one or more types of scene nodes. For example, the
  /// <see cref="MeshRenderer"/> handles <see cref="MeshNode"/>s, the 
  /// <see cref="BillboardRenderer"/> handles <see cref="BillboardNode"/>s and 
  /// <see cref="ParticleSystemNode"/>s, the <see cref="LensFlareRenderer"/> handles 
  /// <see cref="LensFlareNode"/>, etc.
  /// </para>
  /// <para>
  /// The <see cref="SceneRenderer"/> is a special type of scene node renderer. It does not handle
  /// scene nodes itself. Instead other renderers can be added to the <see cref="SceneRenderer"/>.
  /// The collection of renderers can be treated as a single scene node renderer.
  /// </para>
  /// <para>
  /// <strong>Important: Possible Switch of Render Target!</strong><br/>
  /// A scene node renderer may replace the current render target with a new, compatible render 
  /// target! That means that a scene node renderer may discard the current render target and set a
  /// new render target on the graphics device and in the render context. Any references to the
  /// previous render target will be invalid and should be updated.
  /// </para>
  /// </remarks>
  public abstract class SceneNodeRenderer : IDisposable
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly SceneNode[] _oneNodeArray = new SceneNode[1];

    /// <summary>Temporary ID set during rendering.</summary>
    internal uint Id;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this instance has been disposed of.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance has been disposed of; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsDisposed { get; private set; }


    /// <summary>
    /// Gets the draw order of this scene node renderer.
    /// </summary>
    /// <value>
    /// An integer value in the range [0, 255], which defines the draw order of this scene node 
    /// renderer.
    /// </value>
    /// <remarks>
    /// The order is used by composite renderers. For example, the <see cref="SceneRenderer"/> is a
    /// composite renderer that manages a list of renderers and dispatches draw jobs. When no 
    /// explicit render order for the scene nodes is defined, the scene nodes are batched by 
    /// renderer and the renderer with the lowest order is invoked first.
    /// </remarks>
    public int Order
    {
      get { return _order; }
      set
      {
        if (value < 0 || value > byte.MaxValue)
          throw new ArgumentOutOfRangeException("value", "The order value must be in the range [0, 255].");

        _order = value;
      }
    }
    private int _order;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Releases all resources used by an instance of the <see cref="SceneNodeRenderer"/> class.
    /// </summary>
    /// <remarks>
    /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
    /// <see langword="true"/>, and then suppresses finalization of the instance.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="SceneNodeRenderer"/> 
    /// class and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      IsDisposed = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    internal void ThrowIfDisposed()
    {
      if (IsDisposed)
        throw new ObjectDisposedException(GetType().FullName);
    }


    /// <summary>
    /// Determines whether this renderer can handle the specified scene node.
    /// </summary>
    /// <param name="node">The scene node to be rendered.</param>
    /// <param name="context">The render context.</param>
    /// <returns>
    /// <see langword="true"/> if this instance renders the specified node; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public abstract bool CanRender(SceneNode node, RenderContext context);


    /// <overloads>
    /// <summary>
    /// Renders the specified scene nodes.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Renders the specified scene nodes.
    /// </summary>
    /// <param name="nodes">The scene nodes. The list may contain null entries.</param>
    /// <param name="context">
    /// The render context. (The property <see cref="RenderContext.CameraNode"/> selects the 
    /// currently active camera. Some renderers require additional information in the render 
    /// context. See remarks.)
    /// </param>
    /// <remarks>
    /// For mesh nodes: The <see cref="RenderContext.RenderPass"/> in the render context determines 
    /// the effect binding that should be used for rendering meshes. The effect binding might 
    /// require additional information in the render context.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="nodes"/> or <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void Render(IList<SceneNode> nodes, RenderContext context)
    {
      Render(nodes, context, RenderOrder.Default);
    }


    /// <summary>
    /// Renders the specified scene node.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <param name="context">
    /// The render context. (The property <see cref="RenderContext.CameraNode"/> selects the 
    /// currently active camera. Some renderers require additional information in the render 
    /// context. See remarks.)
    /// </param>
    /// <inheritdoc cref="Render(IList{SceneNode},RenderContext)"/>
    public void Render(SceneNode node, RenderContext context)
    {
      if (node == null)
        return;

      _oneNodeArray[0] = node;
      Render(_oneNodeArray, context, RenderOrder.Default);
      _oneNodeArray[0] = null;
    }


    /// <summary>
    /// Renders the specified scene nodes.
    /// </summary>
    /// <param name="nodes">The scene nodes. The list may contain null entries.</param>
    /// <param name="context">
    /// The render context. (The property <see cref="RenderContext.CameraNode"/> selects the 
    /// currently active camera. Some renderers require additional information in the render 
    /// context. See remarks.)
    /// </param>
    /// <param name="order">The render order.</param>
    /// <inheritdoc cref="Render(IList{SceneNode},RenderContext)"/>
    public abstract void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order);
    #endregion
  }
}
