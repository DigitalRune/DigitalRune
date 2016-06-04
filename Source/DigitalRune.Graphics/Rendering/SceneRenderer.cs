// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// A configurable renderer that combines multiple scene node renderers.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="SceneRenderer"/> contains a configurable list of renderers (see property 
  /// <see cref="Renderers"/>). It batches scene nodes by type and dispatches them to the 
  /// appropriate scene node renderer.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> The <see cref="SceneRenderer"/> is empty by default. An empty
  /// <see cref="SceneRenderer"/> does not render anything.
  /// </para>
  /// </remarks>
  public partial class SceneRenderer : SceneNodeRenderer
  {
    // Notes regarding resource pooling:
    // A graphics screen can have several SceneRenderers. It makes sense to take
    // the ArrayList<Job> from a resource pool to reduce memory consumption.
    // However:
    // - The ArrayList<Job> is relatively small.
    // - SceneRenderer may be nested. In this case the ArrayList<Job> cannot be reused.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Defines a draw job of the <see cref="SceneRenderer"/>.
    /// </summary>
    [DebuggerDisplay("Job({SortKey}, Node={Node.GetType().Name,nq}({Node.Name}), Renderer={Renderer.GetType().Name,nq})")]
    internal struct Job
    {
      /// <summary>The sort key.</summary>
      public uint SortKey;

      /// <summary>The scene node.</summary>
      public SceneNode Node;

      /// <summary>The scene node renderer that handles the job.</summary>
      public SceneNodeRenderer Renderer;
    }


    internal class Comparer : IComparer<Job>
    {
      public static readonly Comparer Instance = new Comparer();
      public int Compare(Job x, Job y)
      {
        if (x.SortKey < y.SortKey)
          return -1;
        if (x.SortKey > y.SortKey)
          return +1;

        return 0;
      }
    }
    #endregion

    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    internal readonly ArrayList<Job> Jobs;
    internal readonly Accessor JobsAccessor;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the list of scene node renderers managed by this instance.
    /// </summary>
    /// <value>The list of scene node renderers.</value>
    public SceneNodeRendererCollection Renderers { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new empty instance of the <see cref="SceneRenderer" /> class.
    /// </summary>
    public SceneRenderer()
    {
      Order = 0;
      Renderers = new SceneNodeRendererCollection();

      // Start with a reasonably large capacity to avoid frequent re-allocations.
      Jobs = new ArrayList<Job>(64);
      JobsAccessor = new Accessor();
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
          foreach (var renderer in Renderers)
            renderer.Dispose();
        }
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------    

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      foreach (var renderer in Renderers)
        if (renderer.CanRender(node, context))
          return true;

      return false;
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      context.ThrowIfCameraMissing();

      Debug.Assert(Jobs.Count == 0, "Job list was not properly reset.");

      BatchJobs(nodes, context, order);
      if (Jobs.Count > 0)
      {
        ProcessJobs(context, order);
        Jobs.Clear();
      }

      //PostProcess(context);
    }


    internal virtual void BatchJobs(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      // Assign temporary IDs to scene node renderers.
      for (int i = 0; i < Renderers.Count; i++)
        Renderers[i].Id = (uint)(i & 0xff);  // ID = index clamped to [0, 255].

      // Get camera properties for calculating the distance between scene node and camera.
      Vector3F cameraPosition = new Vector3F();
      Vector3F lookDirection = new Vector3F();
      bool sortByDistance = (order == RenderOrder.FrontToBack || order == RenderOrder.BackToFront);
      bool backToFront = (order == RenderOrder.BackToFront);
      if (sortByDistance)
      {
        var cameraNode = context.CameraNode;
        Pose cameraPose = cameraNode.PoseWorld;
        cameraPosition = cameraPose.Position;
        lookDirection = -cameraPose.Orientation.GetColumn(2);
      }

      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i];
        if (node == null || !FilterSceneNode(node))
          continue;

        var job = new Job();
        job.Node = node;
        foreach (var renderer in Renderers)
        {
          if (renderer.CanRender(node, context))
          {
            job.Renderer = renderer;
            break;
          }
        }

        if (job.Renderer == null)
          continue;

        float distance = 0;
        if (sortByDistance)
        {
          // Determine distance to camera.
          Vector3F cameraToNode = node.PoseWorld.Position - cameraPosition;
          distance = Vector3F.Dot(cameraToNode, lookDirection);
          if (backToFront)
            distance = -distance;
        }

        job.SortKey = GetSortKey(distance, job.Renderer.Order, job.Renderer.Id);
        Jobs.Add(ref job);
      }

      if (order != RenderOrder.UserDefined)
      {
        // Sort draw jobs.
        Jobs.Sort(Comparer.Instance);
      }
    }


    // true = accept node; false = reject node.
    internal virtual bool FilterSceneNode(SceneNode node)
    {
      return true;
    }


    /// <summary>
    /// Gets the sort key.
    /// </summary>
    /// <param name="distance">The distance.</param>
    /// <param name="order">The order of the renderer.</param>
    /// <param name="id">The ID of the renderer.</param>
    /// <returns>The key for sorting draw jobs.</returns>
    private static uint GetSortKey(float distance, int order, uint id)
    {
      Debug.Assert(0 <= order && order <= byte.MaxValue, "Order is out of range.");
      Debug.Assert(id <= byte.MaxValue, "ID is out of range.");

      // ------------------------------------
      // |   distance   |  order  |  ID     |
      // |    16 bit    |  8 bit  |  8 bit  |
      // ------------------------------------

      return Numeric.GetSignificantBitsSigned(distance, 16) << 16 
             | (uint)order << 8
             | id;
    }


    internal virtual void ProcessJobs(RenderContext context, RenderOrder order)
    {
      if (order == RenderOrder.BackToFront || order == RenderOrder.FrontToBack)
      {
        // The scene nodes are already sorted by distance.
        order = RenderOrder.UserDefined;
      }

      var savedRenderState = new RenderStateSnapshot(context.GraphicsService.GraphicsDevice);

      int index = 0;
      var jobs = Jobs.Array;
      int jobCount = Jobs.Count;
      while (index < jobCount)
      {
        var renderer = jobs[index].Renderer;

        // Find end of current batch.
        int endIndexExclusive = index + 1;
        while (endIndexExclusive < jobCount && jobs[endIndexExclusive].Renderer == renderer)
          endIndexExclusive++;

        // Restore the render state. (The integrated scene node renderers properly
        // restore the render state, but third-party renderers might mess it up.)
        if (index > 0)
          savedRenderState.Restore();

        // Submit batch to renderer.
        // (Use Accessor to expose current batch as IList<SceneNode>.)
        JobsAccessor.Set(Jobs, index, endIndexExclusive);
        renderer.Render(JobsAccessor, context, order);
        JobsAccessor.Reset();

        index = endIndexExclusive;
      }

      savedRenderState.Restore();
    }


    //internal virtual void PostProcess(RenderContext context)
    //{
    //}
    #endregion
  }
}
