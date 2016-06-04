// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Collections;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="FigureNode"/>s. 
  /// </summary>
  /// <remarks>
  /// <para>
  /// This renderer requires the HiDef graphics profile. If the current graphics profile is Reach, 
  /// <see cref="Render"/> throws a <see cref="NotSupportedException"/>.
  /// </para>
  /// <para>
  /// The <see cref="FigureRenderer"/> is a scene node renderer which handles 
  /// <see cref="FigureNode"/>s.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  public class FigureRenderer : SceneNodeRenderer
  {
    // TODO: Split OnLoadEffect() into two methods OnLoadStrokeEffect() and OnLoadStrokeEffect().
    // (Currently only the effect for rendering strokes can be overridden.)


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private struct Job
    {
      /// <summary>The sort key.</summary>
      public uint SortKey;

      /// <summary>The figure node.</summary>
      public FigureNode Node;
    }


    private class Comparer : IComparer<Job>
    {
      public static readonly IComparer<Job> Instance = new Comparer();
      public int Compare(Job x, Job y)
      {
        return x.SortKey.CompareTo(y.SortKey);
      }
    }


    private enum RenderMode { Undefined, Fill, Stroke }
    #endregion


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// The maximum buffer size (number of line segments).
    /// </summary>
    /// <remarks>
    /// The maximum buffer size is limited because <see cref="ushort"/> values are internally used 
    /// as indices.
    /// </remarks>
    public const int MaxBufferSize = (ushort.MaxValue + 1) / 4; // = 16384 billboards
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IGraphicsService _graphicsService;

    private readonly ArrayList<Job> _jobs;
    private RenderMode _mode;

    // Resources for fill.
    private readonly BasicEffect _fillEffect;
    private readonly RenderBatch<VertexPositionColor, ushort> _fillBatch;

    // Resources for strokes.
    private readonly Effect _strokeEffect;
    private readonly EffectParameter _parameterViewport;
    private readonly EffectParameter _parameterView;
    private readonly EffectParameter _parameterViewInverse;
    private readonly EffectParameter _parameterProjection;
    private readonly EffectParameter _parameterCameraNear;
    private readonly RenderBatch<StrokeVertex, ushort> _strokeBatch;

    private Matrix44F _view;
    private Pose _viewInverse;
    private Matrix44F _projection;
    private float _cameraNear;
    private Viewport _viewport;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the size of the buffer (= number of line segments/triangles).
    /// </summary>
    /// <value>The size of the buffer (= number of line segments/triangles).</value>
    /// <remarks>
    /// The buffer size is the maximal number of line segments or triangles that can be rendered 
    /// with a single draw call.
    /// </remarks>
    public int BufferSize { get; private set; }


    /// <summary>
    /// Gets or sets the options for rendering figures.
    /// </summary>
    /// <value>
    /// The options for rendering figures. The default value is 
    /// <see cref="FigureRenderOptions.RenderFillAndStroke"/>.
    /// </value>
    public FigureRenderOptions Options { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FigureRenderer" /> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="bufferSize">
    /// The size of the internal buffer (= max number of line segments or triangles that can be 
    /// rendered in a single draw call). Max allowed value is 16384.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="bufferSize"/> is 0, negative, or greater than <see cref="MaxBufferSize"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The current graphics profile is Reach.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Effect should be loaded in constructor. Alternative designs complicate API.")]
    public FigureRenderer(IGraphicsService graphicsService, int bufferSize)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");
      if (bufferSize <= 0 || bufferSize > MaxBufferSize)
        throw new ArgumentOutOfRangeException("bufferSize", "The buffer size must be in the range [1, " + MaxBufferSize + "].");
      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        throw new NotSupportedException("The FigureRenderer does not support the Reach profile.");

      _graphicsService = graphicsService;

      Order = 5;
      BufferSize = bufferSize;
      Options = FigureRenderOptions.RenderFillAndStroke;

      // Start with a reasonably large capacity to avoid frequent re-allocations.
      _jobs = new ArrayList<Job>(32);

      // ReSharper disable DoNotCallOverridableMethodsInConstructor
      _strokeEffect = OnLoadEffect(graphicsService);
      // ReSharper restore DoNotCallOverridableMethodsInConstructor

      _parameterViewport = _strokeEffect.Parameters["ViewportSize"];
      _parameterView = _strokeEffect.Parameters["View"];
      _parameterViewInverse = _strokeEffect.Parameters["ViewInverse"];
      _parameterProjection = _strokeEffect.Parameters["Projection"];
      _parameterCameraNear = _strokeEffect.Parameters["CameraNear"];

      var graphicsDevice = graphicsService.GraphicsDevice;

      // Create stroke indices. (The content of the index buffer does not change.)
      var indices = new ushort[bufferSize * 6];
      for (int i = 0; i < bufferSize; i++)
      {
        // Create index buffer for quad (= two triangles, clockwise).
        //   1--2
        //   | /|
        //   |/ |
        //   0--3
        indices[i * 6 + 0] = (ushort)(i * 4 + 0);
        indices[i * 6 + 1] = (ushort)(i * 4 + 1);
        indices[i * 6 + 2] = (ushort)(i * 4 + 2);
        indices[i * 6 + 3] = (ushort)(i * 4 + 0);
        indices[i * 6 + 4] = (ushort)(i * 4 + 2);
        indices[i * 6 + 5] = (ushort)(i * 4 + 3);
      }

      _strokeBatch = new RenderBatch<StrokeVertex, ushort>(
        graphicsDevice,
        StrokeVertex.VertexDeclaration,
        new StrokeVertex[bufferSize * 4],
        true,
        indices,
        false);

      _fillEffect = new BasicEffect(graphicsDevice)
      {
        Alpha = 1,
        DiffuseColor = new Vector3(1, 1, 1),
        FogEnabled = false,
        PreferPerPixelLighting = false,
        World = Matrix.Identity,
        LightingEnabled = false,
        TextureEnabled = false,
        VertexColorEnabled = true,
      };

      _fillBatch = new RenderBatch<VertexPositionColor, ushort>(
        graphicsDevice,
        VertexPositionColor.VertexDeclaration,
        new VertexPositionColor[bufferSize * 3],
        true,
        new ushort[bufferSize * 3],
        true);
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
          _strokeBatch.Dispose();
          _fillBatch.Dispose();
          _fillEffect.Dispose();

          // Note: Do not dispose _strokeEffect. The effect is managed by the 
          // ContentManager and may be shared.
        }
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when the <see cref="Effect" /> for rendering lines and shapes is loaded.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>The <see cref="Effect" /> that renders lines and shapes.</returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Derived types may override this method to use a 
    /// different effect for rendering lines and shapes. (The method is called by the constructor of
    /// the base class. This means that derived classes may not be initialized yet!)
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual Effect OnLoadEffect(IGraphicsService graphicsService)
    {
      return graphicsService.Content.Load<Effect>("DigitalRune/Line");
    }


    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is FigureNode;
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      context.Validate(_strokeEffect);
      context.ThrowIfCameraMissing();
      Debug.Assert(_jobs.Count == 0, "Job list was not properly reset.");

      BatchJobs(nodes, context, order);
      if (_jobs.Count > 0)
      {
        ProcessJobs(context);
        _jobs.Clear();
      }
    }


    private void BatchJobs(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      // Get camera properties used to calculate the distance of the scene node to the camera.
      var cameraNode = context.CameraNode;
      Vector3F cameraPosition = new Vector3F();
      Vector3F lookDirection = new Vector3F();
      bool sortByDistance = (order == RenderOrder.BackToFront || order == RenderOrder.FrontToBack);
      bool backToFront = (order == RenderOrder.BackToFront);
      if (sortByDistance)
      {
        Pose cameraPose = cameraNode.PoseWorld;
        cameraPosition = cameraPose.Position;
        lookDirection = -cameraPose.Orientation.GetColumn(2);
      }

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      // Add draw jobs to list.
      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as FigureNode;
        if (node == null)
          continue;

        // figureNode is visible in current frame.
        node.LastFrame = frame;

        // Determine distance to camera.
        float distance = 0;
        if (sortByDistance)
        {
          Vector3F cameraToNode = node.PoseWorld.Position - cameraPosition;
          distance = Vector3F.Dot(cameraToNode, lookDirection);
          if (backToFront)
            distance = -distance;
        }

        var job = new Job
        {
          SortKey = GetSortKey(distance, node.DrawOrder),
          Node = node
        };
        _jobs.Add(ref job);
      }

      if (_jobs.Count > 0 && order != RenderOrder.UserDefined)
      {
        // Sort draw jobs.
        _jobs.Sort(Comparer.Instance);
      }
    }


    /// <summary>
    /// Gets the sort key.
    /// </summary>
    /// <param name="distance">The normalized distance [0, 1].</param>
    /// <param name="drawOrder">The draw order.</param>
    /// <returns>The key for sorting draw jobs.</returns>
    private static uint GetSortKey(float distance, int drawOrder)
    {
      // -----------------------------
      // |  distance  |  draw order  |
      // |   16 bit   |   16 bit     |
      // -----------------------------

      uint sortKey = Numeric.GetSignificantBitsSigned(distance, 16) << 16
                     | (uint)drawOrder;

      return sortKey;
    }


    private void ProcessJobs(RenderContext context)
    {
      // Set render states.
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.RasterizerState = RasterizerState.CullNone;

      bool renderFill = (Options & FigureRenderOptions.RenderFill) != 0;
      if (renderFill)
        SetupFill(context);

      bool renderStroke = (Options & FigureRenderOptions.RenderStroke) != 0;
      if (renderStroke)
        SetupStroke(context);

      var jobs = _jobs.Array;
      int jobCount = _jobs.Count;
      _mode = RenderMode.Undefined;
      for (int i = 0; i < jobCount; i++)
      {
        var node = jobs[i].Node;
        var renderData = node.Figure.RenderData;
        if (renderData == null)
          continue;

        var vertices = renderData.Vertices;
        if (vertices == null || vertices.Count == 0)
          continue;

        // We can cache a static vertex buffer for static figures with camera-independent dash patterns.
        if (node.IsStatic && node.DashInWorldSpace)
        {
          var nodeRenderData = node.RenderData as FigureNodeRenderData;
          if (nodeRenderData == null)
          {
            nodeRenderData = new FigureNodeRenderData();
            node.RenderData = nodeRenderData;
          }

          if (!nodeRenderData.IsValid)
            CacheVertexBuffer(node, _graphicsService.GraphicsDevice);
        }

        // Render filled polygons.
        var fillIndices = renderData.FillIndices;
        if (renderFill
            && fillIndices != null
            && fillIndices.Count > 0
            && !Numeric.IsZero(node.FillAlpha))
        {
          Fill(node, vertices, fillIndices);
        }

        // Render stroked lines.
        var strokeIndices = renderData.StrokeIndices;
        if (renderStroke
            && strokeIndices != null
            && strokeIndices.Count > 0
            && !Numeric.IsZero(node.StrokeThickness)
            && !Numeric.IsZero(node.StrokeAlpha))
        {
          Stroke(node, vertices, strokeIndices);
        }
      }

      Flush();

      savedRenderState.Restore();
    }


    private void Flush()
    {
      if (_mode == RenderMode.Fill)
        _fillBatch.Flush();
      else if (_mode == RenderMode.Stroke)
        _strokeBatch.Flush();
    }


    private void SetupFill(RenderContext context)
    {
      // Set parameters for rendering filled polygons.
      var cameraNode = context.CameraNode;
      var camera = cameraNode.Camera;

      _fillEffect.View = (Matrix)cameraNode.View;
      _fillEffect.Projection = camera.Projection;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void Fill(FigureNode node, ArrayList<Vector3F> vertices, ArrayList<int> indices)
    {
      if (_mode != RenderMode.Fill)
      {
        Flush();
        _fillEffect.CurrentTechnique.Passes[0].Apply();
        _mode = RenderMode.Fill;
      }

      // Use cached vertex buffer if available.
      var nodeRenderData = node.RenderData as FigureNodeRenderData;
      if (nodeRenderData != null && nodeRenderData.IsValid)
      {
        Flush();
        var graphicsDevice = _graphicsService.GraphicsDevice;
        graphicsDevice.SetVertexBuffer(nodeRenderData.FillVertexBuffer);
        graphicsDevice.Indices = nodeRenderData.FillIndexBuffer;
        int primitiveCount = nodeRenderData.FillIndexBuffer.IndexCount / 3;
#if MONOGAME
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
#else
        int vertexCount = nodeRenderData.FillVertexBuffer.VertexCount;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, primitiveCount);
#endif
        return;
      }

      Matrix44F world = node.PoseWorld * Matrix44F.CreateScale(node.ScaleWorld);
      Vector3F color3F = node.FillColor * node.FillAlpha;
      Color color = new Color(color3F.X, color3F.Y, color3F.Z, node.FillAlpha);

      var numberOfVertices = vertices.Count;
      var numberOfIndices = indices.Count;

      VertexPositionColor[] batchVertices = _fillBatch.Vertices;
      ushort[] batchIndices = _fillBatch.Indices;

      if (numberOfVertices > batchVertices.Length || numberOfIndices > batchIndices.Length)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "The BufferSize of this FigureRenderer is not large enough to render the FigureNode (Name = \"{0}\").",
          node.Name);
        throw new GraphicsException(message);
      }

      int vertexBufferStartIndex, indexBufferStartIndex;
      _fillBatch.Submit(PrimitiveType.TriangleList, numberOfVertices, numberOfIndices,
        out vertexBufferStartIndex, out indexBufferStartIndex);

      // Copy all vertices.
      Vector3F[] vertexArray = vertices.Array;
      for (int i = 0; i < numberOfVertices; i++)
      {
        batchVertices[vertexBufferStartIndex + i].Position = (Vector3)(world.TransformPosition(vertexArray[i]));
        batchVertices[vertexBufferStartIndex + i].Color = color;
      }

      // Copy all indices.
      int[] indexArray = indices.Array;
      for (int i = 0; i < numberOfIndices; i++)
      {
        batchIndices[indexBufferStartIndex + i] = (ushort)(vertexBufferStartIndex + indexArray[i]);
      }
    }


    private void SetupStroke(RenderContext context)
    {
      // Set parameters for rendering stroked lines.
      var cameraNode = context.CameraNode;
      var camera = cameraNode.Camera;
      _view = cameraNode.View;
      _viewInverse = cameraNode.PoseWorld;
      _projection = camera.Projection.ToMatrix44F();
      //var viewProjection = projection * view;
      _cameraNear = camera.Projection.Near;
      _viewport = context.Viewport;

      _parameterView.SetValue((Matrix)_view);
      _parameterViewInverse.SetValue((Matrix)_viewInverse);
      _parameterProjection.SetValue((Matrix)_projection);
      _parameterViewport.SetValue(new Vector2(_viewport.Width, _viewport.Height));
      _parameterCameraNear.SetValue(_cameraNear);
    }


    private void Stroke(FigureNode node, ArrayList<Vector3F> strokeVertices, ArrayList<int> strokeIndices)
    {
      if (_mode != RenderMode.Stroke)
      {
        Flush();
        _strokeEffect.CurrentTechnique.Passes[0].Apply();
        _mode = RenderMode.Stroke;
      }

      // Use cached vertex buffer if available.
      var nodeRenderData = node.RenderData as FigureNodeRenderData;
      if (nodeRenderData != null && nodeRenderData.IsValid)
      {
        Flush();
        var graphicsDevice = _graphicsService.GraphicsDevice;
        graphicsDevice.SetVertexBuffer(nodeRenderData.StrokeVertexBuffer);
        graphicsDevice.Indices = nodeRenderData.StrokeIndexBuffer;
        int primitiveCount = nodeRenderData.StrokeIndexBuffer.IndexCount / 3;
#if MONOGAME
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
#else
        int vertexCount = nodeRenderData.StrokeVertexBuffer.VertexCount;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, primitiveCount);
#endif
        return;
      }

      var batchVertices = _strokeBatch.Vertices;

      var world = node.PoseWorld * Matrix44F.CreateScale(node.ScaleWorld);
      var worldView = _view * world;

      var thickness = node.StrokeThickness;
      var color3F = node.StrokeColor * node.StrokeAlpha;
      var color = new HalfVector4(color3F.X, color3F.Y, color3F.Z, node.StrokeAlpha);
      var dash = node.StrokeDashPattern * node.StrokeThickness;
      bool usesDashPattern = (dash.Y + dash.Z) != 0;
      var dashSum = new HalfVector4(
        dash.X,
        dash.X + dash.Y,
        dash.X + dash.Y + dash.Z,
        dash.X + dash.Y + dash.Z + dash.W);

      // Convert to vertices.
      float lastDistance = 0;
      Vector3F lastPosition = new Vector3F(float.NaN);
      Vector3F lastWorld = new Vector3F();
      Vector3F lastView = new Vector3F();
      Vector3F lastProjected = new Vector3F();

      var data0 = new HalfVector4(0, 1, thickness, 0);
      var data1 = new HalfVector4(0, 0, thickness, 0);
      var data2 = new HalfVector4(1, 0, thickness, 0);
      var data3 = new HalfVector4(1, 1, thickness, 0);

      Vector3F[] figurePoints = strokeVertices.Array;
      int[] figureIndices = strokeIndices.Array;
      int numberOfLineSegments = strokeIndices.Count / 2;

      for (int i = 0; i < numberOfLineSegments; i++)
      {
        var startIndex = figureIndices[i * 2 + 0];
        var endIndex = figureIndices[i * 2 + 1];
        var start = figurePoints[startIndex];
        var end = figurePoints[endIndex];

        var notConnectedWithLast = start != lastPosition;
        lastPosition = end;

        Vector3F startWorld = notConnectedWithLast ? world.TransformPosition(start) : lastWorld;
        Vector3F endWorld = world.TransformPosition(end);
        lastWorld = endWorld;

        // Compute start/end distances of lines from beginning of line strip
        // for dash patterns.
        float startDistance = 0;
        float endDistance = 1;
        if (usesDashPattern)
        {
          if (!node.DashInWorldSpace)
          {
            Vector3F startView = notConnectedWithLast ? worldView.TransformPosition(start) : lastView;
            var endView = worldView.TransformPosition(end);
            lastView = endView;

            // Clip to near plane - otherwise lines which end near the camera origin
            // (where planar z == 0) will disappear. (Projection singularity!)
            float deltaZ = Math.Abs(startView.Z - endView.Z);
            float pStart = MathHelper.Clamp((startView.Z - (-_cameraNear)) / deltaZ, 0, 1);
            startView = InterpolationHelper.Lerp(startView, endView, pStart);
            float pEnd = MathHelper.Clamp((endView.Z - (-_cameraNear)) / deltaZ, 0, 1);
            endView = InterpolationHelper.Lerp(endView, startView, pEnd);

            Vector3F startProjected;
            if (notConnectedWithLast)
            {
              lastDistance = 0;
              startProjected = _viewport.ProjectToViewport(startView, _projection);
            }
            else
            {
              startProjected = lastProjected;
            }
            var endProjected = _viewport.ProjectToViewport(endView, _projection);
            lastProjected = endProjected;

            startDistance = lastDistance;
            endDistance = startDistance + (endProjected - startProjected).Length;
            lastDistance = endDistance;
          }
          else
          {
            if (notConnectedWithLast)
              lastDistance = 0;

            startDistance = lastDistance;
            endDistance = startDistance + (endWorld - startWorld).Length;
            lastDistance = endDistance;

            // The shader needs to know that DashInWorldSpace is true. To avoid
            // effect parameter changes, we store the value in the sign of the distance!
            startDistance = -startDistance;
            endDistance = -endDistance;
          }
        }

        var s = new Vector4(startWorld.X, startWorld.Y, startWorld.Z, startDistance);
        var e = new Vector4(endWorld.X, endWorld.Y, endWorld.Z, endDistance);

        int index, dummy;
        _strokeBatch.Submit(PrimitiveType.TriangleList, 4, 6, out index, out dummy);

        batchVertices[index + 0].Start = s;
        batchVertices[index + 0].End = e;
        batchVertices[index + 0].Data = data0;
        batchVertices[index + 0].Color = color;
        batchVertices[index + 0].Dash = dashSum;

        batchVertices[index + 1].Start = s;
        batchVertices[index + 1].End = e;
        batchVertices[index + 1].Data = data1;
        batchVertices[index + 1].Color = color;
        batchVertices[index + 1].Dash = dashSum;

        batchVertices[index + 2].Start = s;
        batchVertices[index + 2].End = e;
        batchVertices[index + 2].Data = data2;
        batchVertices[index + 2].Color = color;
        batchVertices[index + 2].Dash = dashSum;

        batchVertices[index + 3].Start = s;
        batchVertices[index + 3].End = e;
        batchVertices[index + 3].Data = data3;
        batchVertices[index + 3].Color = color;
        batchVertices[index + 3].Dash = dashSum;
      }
    }


    private static void CacheVertexBuffer(FigureNode node, GraphicsDevice graphicsDevice)
    {
      var figureRenderData = node.Figure.RenderData;
      var nodeRenderData = (FigureNodeRenderData)node.RenderData;
      Vector3F[] positions = figureRenderData.Vertices.Array;

      #region ----- Cache vertex/index buffer for fill. -----
      var fillIndices = figureRenderData.FillIndices;
      if (fillIndices != null
          && fillIndices.Count > 0
          && !Numeric.IsZero(node.FillAlpha))
      {
        // This code is similar to the code in Fill().

        Matrix44F world = node.PoseWorld * Matrix44F.CreateScale(node.ScaleWorld);
        Vector3F color3F = node.FillColor * node.FillAlpha;
        Color color = new Color(color3F.X, color3F.Y, color3F.Z, node.FillAlpha);

        int numberOfVertices = figureRenderData.Vertices.Count;
        int numberOfIndices = figureRenderData.FillIndices.Count;

        VertexPositionColor[] vertices = new VertexPositionColor[numberOfVertices];

        // Copy all vertices.
        for (int i = 0; i < numberOfVertices; i++)
        {
          vertices[i].Position = (Vector3)world.TransformPosition(positions[i]);
          vertices[i].Color = color;
        }

        nodeRenderData.FillVertexBuffer = new VertexBuffer(
          graphicsDevice,
          VertexPositionColor.VertexDeclaration,
          numberOfVertices,
          BufferUsage.WriteOnly);
        nodeRenderData.FillVertexBuffer.SetData(vertices);

        if (numberOfVertices <= ushort.MaxValue)
        {
          // Copy all indices from int[] to ushort[].
          int[] int32Indices = figureRenderData.FillIndices.Array;
          ushort[] indices = new ushort[numberOfIndices];
          for (int i = 0; i < numberOfIndices; i++)
            indices[i] = (ushort)int32Indices[i];

          nodeRenderData.FillIndexBuffer = new IndexBuffer(
            graphicsDevice,
            IndexElementSize.SixteenBits,
            numberOfIndices,
            BufferUsage.WriteOnly);
          nodeRenderData.FillIndexBuffer.SetData(indices);
        }
        else
        {
          nodeRenderData.FillIndexBuffer = new IndexBuffer(
            graphicsDevice,
            IndexElementSize.ThirtyTwoBits,
            numberOfIndices,
            BufferUsage.WriteOnly);
          // Note: The FillIndices.Array may contain more than numberOfIndices entries! -->
          // Specify number of indices explicitly!
          nodeRenderData.FillIndexBuffer.SetData(figureRenderData.FillIndices.Array, 0, numberOfIndices);
        }
      }
      #endregion

      #region ----- Cache vertex/index buffer for stroke. -----

      var strokeIndices = figureRenderData.StrokeIndices;
      if (strokeIndices != null
          && strokeIndices.Count > 0
          && !Numeric.IsZero(node.StrokeThickness)
          && !Numeric.IsZero(node.StrokeAlpha))
      {
        // This code is similar to the code in Stroke() and in the ctor.

        Matrix44F world = node.PoseWorld * Matrix44F.CreateScale(node.ScaleWorld);

        float thickness = node.StrokeThickness;
        Vector3F color3F = node.StrokeColor * node.StrokeAlpha;
        HalfVector4 color = new HalfVector4(color3F.X, color3F.Y, color3F.Z, node.StrokeAlpha);
        Vector4F dash = node.StrokeDashPattern * node.StrokeThickness;
        bool usesDashPattern = (dash.Y + dash.Z) != 0;
        HalfVector4 dashSum = new HalfVector4(
          dash.X,
          dash.X + dash.Y,
          dash.X + dash.Y + dash.Z,
          dash.X + dash.Y + dash.Z + dash.W);

        // Convert to vertices.
        float lastDistance = 0;
        Vector3F lastPosition = new Vector3F(float.NaN);
        Vector3F lastWorld = new Vector3F();

        HalfVector4 data0 = new HalfVector4(0, 1, thickness, 0);
        HalfVector4 data1 = new HalfVector4(0, 0, thickness, 0);
        HalfVector4 data2 = new HalfVector4(1, 0, thickness, 0);
        HalfVector4 data3 = new HalfVector4(1, 1, thickness, 0);

        int[] figureIndices = strokeIndices.Array;
        int numberOfLineSegments = strokeIndices.Count / 2;
        int numberOfVertices = numberOfLineSegments * 4;

        StrokeVertex[] vertices = new StrokeVertex[numberOfVertices];
        for (int i = 0; i < numberOfLineSegments; i++)
        {
          int startIndex = figureIndices[i * 2 + 0];
          int endIndex = figureIndices[i * 2 + 1];
          Vector3F start = positions[startIndex];
          Vector3F end = positions[endIndex];

          bool notConnectedWithLast = start != lastPosition;
          lastPosition = end;

          Vector3F startWorld = notConnectedWithLast ? world.TransformPosition(start) : lastWorld;
          Vector3F endWorld = world.TransformPosition(end);
          lastWorld = endWorld;

          // Compute start/end distances of lines from beginning of line strip
          // for dash patterns.
          float startDistance = 0;
          float endDistance = 1;
          if (usesDashPattern)
          {
            Debug.Assert(node.DashInWorldSpace, "Cannot cache vertex buffer for figure with screen-space dash patterns.");

            if (notConnectedWithLast)
              lastDistance = 0;

            startDistance = lastDistance;
            endDistance = startDistance + (endWorld - startWorld).Length;
            lastDistance = endDistance;

            // The shader needs to know that DashInWorldSpace is true. To avoid
            // effect parameter changes, we store the value in the sign of the distance!
            startDistance = -startDistance;
            endDistance = -endDistance;
          }

          Vector4 s = new Vector4(startWorld.X, startWorld.Y, startWorld.Z, startDistance);
          Vector4 e = new Vector4(endWorld.X, endWorld.Y, endWorld.Z, endDistance);

          vertices[i * 4 + 0].Start = s;
          vertices[i * 4 + 0].End = e;
          vertices[i * 4 + 0].Data = data0;
          vertices[i * 4 + 0].Color = color;
          vertices[i * 4 + 0].Dash = dashSum;

          vertices[i * 4 + 1].Start = s;
          vertices[i * 4 + 1].End = e;
          vertices[i * 4 + 1].Data = data1;
          vertices[i * 4 + 1].Color = color;
          vertices[i * 4 + 1].Dash = dashSum;

          vertices[i * 4 + 2].Start = s;
          vertices[i * 4 + 2].End = e;
          vertices[i * 4 + 2].Data = data2;
          vertices[i * 4 + 2].Color = color;
          vertices[i * 4 + 2].Dash = dashSum;

          vertices[i * 4 + 3].Start = s;
          vertices[i * 4 + 3].End = e;
          vertices[i * 4 + 3].Data = data3;
          vertices[i * 4 + 3].Color = color;
          vertices[i * 4 + 3].Dash = dashSum;
        }

        nodeRenderData.StrokeVertexBuffer = new VertexBuffer(
          graphicsDevice,
          StrokeVertex.VertexDeclaration,
          vertices.Length,
          BufferUsage.WriteOnly);
        nodeRenderData.StrokeVertexBuffer.SetData(vertices);

        // Create stroke indices.
        int numberOfIndices = numberOfLineSegments * 6;
        if (numberOfVertices <= ushort.MaxValue)
        {
          ushort[] indices = new ushort[numberOfIndices];
          for (int i = 0; i < numberOfLineSegments; i++)
          {
            // Create index buffer for quad (= two triangles, clockwise).
            //   1--2
            //   | /|
            //   |/ |
            //   0--3
            indices[i * 6 + 0] = (ushort)(i * 4 + 0);
            indices[i * 6 + 1] = (ushort)(i * 4 + 1);
            indices[i * 6 + 2] = (ushort)(i * 4 + 2);
            indices[i * 6 + 3] = (ushort)(i * 4 + 0);
            indices[i * 6 + 4] = (ushort)(i * 4 + 2);
            indices[i * 6 + 5] = (ushort)(i * 4 + 3);
          }

          nodeRenderData.StrokeIndexBuffer = new IndexBuffer(
            graphicsDevice,
            IndexElementSize.SixteenBits,
            numberOfIndices,
            BufferUsage.WriteOnly);
          nodeRenderData.StrokeIndexBuffer.SetData(indices);
        }
        else
        {
          int[] indices = new int[numberOfIndices];
          for (int i = 0; i < numberOfLineSegments; i++)
          {
            // Create index buffer for quad (= two triangles, clockwise).
            //   1--2
            //   | /|
            //   |/ |
            //   0--3
            indices[i * 6 + 0] = i * 4 + 0;
            indices[i * 6 + 1] = i * 4 + 1;
            indices[i * 6 + 2] = i * 4 + 2;
            indices[i * 6 + 3] = i * 4 + 0;
            indices[i * 6 + 4] = i * 4 + 2;
            indices[i * 6 + 5] = i * 4 + 3;
          }

          nodeRenderData.StrokeIndexBuffer = new IndexBuffer(
            graphicsDevice,
            IndexElementSize.ThirtyTwoBits,
            numberOfIndices,
            BufferUsage.WriteOnly);
          nodeRenderData.StrokeIndexBuffer.SetData(indices);
        }
      }
      #endregion

      nodeRenderData.IsValid = true;
    }
    #endregion
  }
}
