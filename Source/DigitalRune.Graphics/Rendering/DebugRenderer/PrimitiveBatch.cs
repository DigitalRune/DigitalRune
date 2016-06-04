// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders a batch of primitives.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A valid <see cref="Effect"/> must be set; otherwise, <see cref="Render"/> will not draw any 
  /// primitives. The <see cref="PrimitiveBatch"/> uses the currently set blend state and 
  /// depth-stencil state. 
  /// </para>
  /// </remarks>
  internal sealed class PrimitiveBatch
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // TODO: Use ResourcePoolUnsafe for performance.
    private static readonly ResourcePool<PrimitiveJob> PrimitiveJobPool = new ResourcePool<PrimitiveJob>(
       () => new PrimitiveJob(),
       null,
       p => p.Reset());

    private enum PrimitiveJobType
    {
      Box,
      Capsule,
      Cone,
      Cylinder,
      Sphere,
      ViewVolume,
      Shape,
      Model,
      Submesh,
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct PrimitiveSize
    {
      // Box
      [FieldOffset(0)]
      public Vector3F Extent;

      // Capsule, Cone, Cylinder, Sphere
      [FieldOffset(0)]
      public float Radius;
      [FieldOffset(4)]
      public float Height;

      // ViewVolume
      [FieldOffset(0)]
      public float Left;
      [FieldOffset(4)]
      public float Right;
      [FieldOffset(8)]
      public float Bottom;
      [FieldOffset(12)]
      public float Top;
      [FieldOffset(16)]
      public float Near;
      [FieldOffset(20)]
      public float Far;

      // Shape, Model, Submesh
      [FieldOffset(0)]
      public Vector3F Scale;
    }

    private class PrimitiveJob
    {
      public PrimitiveJobType Type;

      public Shape Shape;
      public Model Model;
      public Submesh Submesh;

      public Pose Pose;
      public Color Color;
      public PrimitiveSize Size;
      public float Depth;

      public void Reset()
      {
        Shape = null;
        Model = null;
        Submesh = null;
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    private class PrimitiveJobDepthComparer : Singleton<PrimitiveJobDepthComparer>, IComparer<PrimitiveJob>
    {
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
      public int Compare(PrimitiveJob x, PrimitiveJob y)
      {
        if (x.Depth < y.Depth)
          return -1;
        if (x.Depth > y.Depth)
          return +1;

        return 0;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IGraphicsService _graphicsService;

    private Matrix[] _boneTransforms;

    private readonly List<PrimitiveJob> _primitives = new List<PrimitiveJob>();
    private readonly LineBatch _lineBatch;
    private readonly TriangleBatch _triangleBatch;

    private bool _usesTransparency;

    private Submesh _boxLinePrimitive;
    private Submesh _boxPrimitive;
    private Submesh _circleLinePrimitive;
    private Submesh _hemisphereLinePrimitive;
    private Submesh _hemispherePrimitive;
    private Submesh _icospherePrimitive;
    private Submesh _cylinderLinePrimitive;
    private Submesh _cylinderPrimitive;
    private Submesh _coneLinePrimitive;
    private Submesh _conePrimitive;
    private Submesh _uncappedCylinderPrimitive;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the effect.
    /// </summary>
    /// <value>The effect.</value>
    /// <remarks>
    /// If this value is <see langword="null"/>, then <see cref="Render"/> does nothing.
    /// </remarks>
    public BasicEffect Effect { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the objects should be sorted and rendered back to 
    /// front (usually necessary for transparent objects).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if objects are sorted and drawn back to front; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool SortBackToFront { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether objects should be drawn only with lines.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if objects are drawn with lines; otherwise, <see langword="false"/>.
    /// </value>
    public bool DrawWireFrame { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the primitive batch may change the rasterizer state.
    /// (Required for wireframe rendering.)
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the primitive batch may change the rasterizer state; otherwise, 
    /// <see langword="false" /> to use the currently set rasterizer state. The default value is <see langword="true" />.
    /// </value>
    /// <remarks>
    /// Normally, the primitive batch chooses its own rasterizer state to create render solid 
    /// faces or a wireframe. If this property is <see langword="false" />, the primitive
    /// batch uses the currently set rasterizer state. This allows the caller to define how
    /// the primitives should be rendered, but solid vs. wireframe rendering might not work as
    /// expected.
    /// </remarks>
    public bool AutoRasterizerState { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveBatch" /> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="effect">The effect.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService" /> is <see langword="null" />.
    /// </exception>
    public PrimitiveBatch(IGraphicsService graphicsService, BasicEffect effect)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _graphicsService = graphicsService;

      Effect = effect;
      _lineBatch = new LineBatch(effect);
      _triangleBatch = new TriangleBatch(effect);
      AutoRasterizerState = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Clears the primitive batch.
    /// </summary>
    public void Clear()
    {
      foreach (var job in _primitives)
        PrimitiveJobPool.Recycle(job);

      _primitives.Clear();
      _usesTransparency = false;
    }


    /// <summary>
    /// Adds a box for rendering.
    /// </summary>
    /// <param name="widthX">The width along the x-axis.</param>
    /// <param name="widthY">The width along the y-axis.</param>
    /// <param name="widthZ">The width along the z-axis.</param>
    /// <param name="pose">The pose (position and orientation).</param>
    /// <param name="color">The color.</param>
    public void AddBox(float widthX, float widthY, float widthZ, Pose pose, Color color)
    {
      var job = PrimitiveJobPool.Obtain();
      job.Type = PrimitiveJobType.Box;
      job.Pose = pose;
      job.Color = color;
      job.Size.Extent = new Vector3F(widthX, widthY, widthZ);
      _primitives.Add(job);

      _usesTransparency = _usesTransparency || (color.A < 255);
    }


    /// <summary>
    /// Adds a capsule for rendering.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="height">The height.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="color">The color.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="height"/> is too small or <paramref name="radius"/> is too big. The capsule 
    /// height must be greater than or equal to two times the capsule radius.
    /// </exception>
    public void AddCapsule(float radius, float height, Pose pose, Color color)
    {
      if (height < 2 * radius)
        throw new ArgumentException("The capsule height must be greater than or equal to two times the capsule radius.");

      var job = PrimitiveJobPool.Obtain();
      job.Type = PrimitiveJobType.Capsule;
      job.Pose = pose;
      job.Color = color;
      job.Size.Radius = radius;
      job.Size.Height = height;
      _primitives.Add(job);

      _usesTransparency = _usesTransparency || (color.A < 255);
    }


    /// <summary>
    /// Adds a cone for rendering.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="height">The height.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="color">The color.</param>
    public void AddCone(float radius, float height, Pose pose, Color color)
    {
      var job = PrimitiveJobPool.Obtain();
      job.Type = PrimitiveJobType.Cone;
      job.Pose = pose;
      job.Color = color;
      job.Size.Radius = radius;
      job.Size.Height = height;
      _primitives.Add(job);

      _usesTransparency = _usesTransparency || (color.A < 255);
    }


    /// <summary>
    /// Adds a cylinder for rendering.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="height">The height.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="color">The color.</param>
    public void AddCylinder(float radius, float height, Pose pose, Color color)
    {
      var job = PrimitiveJobPool.Obtain();
      job.Type = PrimitiveJobType.Cylinder;
      job.Pose = pose;
      job.Color = color;
      job.Size.Radius = radius;
      job.Size.Height = height;
      _primitives.Add(job);

      _usesTransparency = _usesTransparency || (color.A < 255);
    }


    /// <summary>
    /// Adds a sphere for rendering.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="color">The color.</param>
    public void AddSphere(float radius, Pose pose, Color color)
    {
      var job = PrimitiveJobPool.Obtain();
      job.Type = PrimitiveJobType.Sphere;
      job.Pose = pose;
      job.Color = color;
      job.Size.Radius = radius;
      _primitives.Add(job);

      _usesTransparency = _usesTransparency || (color.A < 255);
    }


    /// <summary>
    /// Adds a view volume for rendering.
    /// </summary>
    /// <param name="isPerspective">
    /// <see langword="true"/> for perspective view volumes, <see langword="false"/> for 
    /// orthographic view volumes.
    /// </param>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <param name="bottom">The bottom.</param>
    /// <param name="top">The top.</param>
    /// <param name="near">The near.</param>
    /// <param name="far">The far.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="color">The color.</param>
    public void AddViewVolume(bool isPerspective, float left, float right, float bottom, float top, float near, float far, Pose pose, Color color)
    {
      var job = PrimitiveJobPool.Obtain();
      if (isPerspective)
      {
        job.Type = PrimitiveJobType.ViewVolume;
        job.Pose = pose;
        job.Color = color;
        job.Size.Left = left;
        job.Size.Right = right;
        job.Size.Bottom = bottom;
        job.Size.Top = top;
        job.Size.Near = near;
        job.Size.Far = far;
      }
      else
      {
        var aabb = new Aabb(new Vector3F(left, bottom, -far), new Vector3F(right, top, -near));
        pose.Position += pose.ToWorldDirection(aabb.Center);

        job.Type = PrimitiveJobType.Box;
        job.Pose = pose;
        job.Color = color;
        job.Size.Extent = aabb.Extent;
      }

      _primitives.Add(job);

      _usesTransparency = _usesTransparency || (color.A < 255);
    }


    /// <summary>
    /// Adds a shape for rendering.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="color">The color.</param>
    public void AddShape(Shape shape, Pose pose, Vector3F scale, Color color)
    {
      var job = PrimitiveJobPool.Obtain();
      job.Type = PrimitiveJobType.Shape;
      job.Shape = shape;
      job.Pose = pose;
      job.Color = color;
      job.Size.Scale = scale;
      _primitives.Add(job);

      _usesTransparency = _usesTransparency || (color.A < 255);
    }


    /// <summary>
    /// Adds a model for rendering.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="color">The color.</param>
    public void AddModel(Model model, Pose pose, Vector3F scale, Color color)
    {
      var job = PrimitiveJobPool.Obtain();
      job.Type = PrimitiveJobType.Model;
      job.Model = model;
      job.Pose = pose;
      job.Color = color;
      job.Size.Scale = scale;
      _primitives.Add(job);

      _usesTransparency = _usesTransparency || (color.A < 255);
    }


    /// <summary>
    /// Adds a model for rendering.
    /// </summary>
    /// <param name="submesh">The submesh.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="color">The color.</param>
    public void AddSubmesh(Submesh submesh, Pose pose, Vector3F scale, Color color)
    {
      var job = PrimitiveJobPool.Obtain();
      job.Type = PrimitiveJobType.Submesh;
      job.Submesh = submesh;
      job.Pose = pose;
      job.Color = color;
      job.Size.Scale = scale;
      _primitives.Add(job);

      _usesTransparency = _usesTransparency || (color.A < 255);
    }


    /// <summary>
    /// Draws the batched primitives.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void Render(RenderContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      if (Effect == null || _primitives.Count == 0)
        return;

      context.Validate(Effect);
      context.ThrowIfCameraMissing();

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      if (AutoRasterizerState)
        graphicsDevice.RasterizerState = DrawWireFrame ? GraphicsHelper.RasterizerStateWireFrame : GraphicsHelper.RasterizerStateCullCounterClockwise;

      // Sort primitives if necessary.
      Matrix44F view = context.CameraNode.View;
      if (SortBackToFront && _usesTransparency)
      {
        // Update depth (distance from camera).
        foreach (var job in _primitives)
        {
          Vector3F position = job.Pose.Position;
          job.Depth = view.TransformPosition(position).Z;
        }

        _primitives.Sort(PrimitiveJobDepthComparer.Instance);
      }

      // Reset the texture stages. If a floating point texture is set, we get exceptions
      // when a sampler with bilinear filtering is set.
      graphicsDevice.ResetTextures();

      Effect.LightingEnabled = !DrawWireFrame;
      Effect.TextureEnabled = false;
      Effect.View = (Matrix)view;
      Effect.Projection = context.CameraNode.Camera.Projection;

      foreach (var job in _primitives)
      {
        Effect.Alpha = job.Color.A / 255.0f;
        Effect.DiffuseColor = job.Color.ToVector3();
        Effect.VertexColorEnabled = false;

        switch (job.Type)
        {
          case PrimitiveJobType.Box:
            RenderBox(graphicsService, job);
            break;
          case PrimitiveJobType.Capsule:
            RenderCapsule(graphicsService, job);
            break;
          case PrimitiveJobType.Cone:
            RenderCone(graphicsService, job);
            break;
          case PrimitiveJobType.Cylinder:
            RenderCylinder(graphicsService, job);
            break;
          case PrimitiveJobType.ViewVolume:
            RenderViewVolume(job, context);
            break;
          case PrimitiveJobType.Sphere:
            RenderSphere(graphicsService, job, context);
            break;
          case PrimitiveJobType.Shape:
            RenderShape(graphicsDevice, job);
            break;
          case PrimitiveJobType.Model:
            RenderModel(graphicsDevice, job);
            break;
          case PrimitiveJobType.Submesh:
            RenderSubmesh(job);
            break;
        }
      }

      if (DrawWireFrame)
      {
        _lineBatch.Render(context);
        _lineBatch.Clear();
      }

      savedRenderState.Restore();
    }


    private void RenderBox(IGraphicsService graphicsService, PrimitiveJob job)
    {
      Effect.World = Matrix.CreateScale((Vector3)job.Size.Extent) * job.Pose;
      Effect.CurrentTechnique.Passes[0].Apply();

      if (DrawWireFrame)
      {
        if (_boxLinePrimitive == null)
          _boxLinePrimitive = MeshHelper.GetBoxLines(graphicsService);

        _boxLinePrimitive.Draw();
      }
      else
      {
        if (_boxPrimitive == null)
          _boxPrimitive = MeshHelper.GetBox(graphicsService);

        _boxPrimitive.Draw();
      }
    }


    private void RenderViewVolume(PrimitiveJob job, RenderContext context)
    {
      float left = job.Size.Left;
      float right = job.Size.Right;
      float bottom = job.Size.Bottom;
      float top = job.Size.Top;
      float near = job.Size.Near;
      float far = job.Size.Far;

      float farOverNear = far / near;

      Vector3F corner0 = job.Pose.ToWorldPosition(new Vector3F(left, bottom, -near));
      Vector3F corner1 = job.Pose.ToWorldPosition(new Vector3F(right, bottom, -near));
      Vector3F corner2 = job.Pose.ToWorldPosition(new Vector3F(right, top, -near));
      Vector3F corner3 = job.Pose.ToWorldPosition(new Vector3F(left, top, -near));
      Vector3F corner4 = job.Pose.ToWorldPosition(new Vector3F(left * farOverNear, bottom * farOverNear, -far));
      Vector3F corner5 = job.Pose.ToWorldPosition(new Vector3F(right * farOverNear, bottom * farOverNear, -far));
      Vector3F corner6 = job.Pose.ToWorldPosition(new Vector3F(right * farOverNear, top * farOverNear, -far));
      Vector3F corner7 = job.Pose.ToWorldPosition(new Vector3F(left * farOverNear, top * farOverNear, -far));

      if (DrawWireFrame)
      {
        _lineBatch.Add(corner0, corner1, job.Color);
        _lineBatch.Add(corner1, corner2, job.Color);
        _lineBatch.Add(corner2, corner3, job.Color);
        _lineBatch.Add(corner0, corner3, job.Color);
        _lineBatch.Add(corner4, corner5, job.Color);
        _lineBatch.Add(corner5, corner6, job.Color);
        _lineBatch.Add(corner6, corner7, job.Color);
        _lineBatch.Add(corner7, corner4, job.Color);
        _lineBatch.Add(corner0, corner4, job.Color);
        _lineBatch.Add(corner1, corner5, job.Color);
        _lineBatch.Add(corner2, corner6, job.Color);
        _lineBatch.Add(corner3, corner7, job.Color);
      }
      else
      {
        _triangleBatch.Clear();

        // Right face
        var n = job.Pose.ToWorldDirection(new Vector3F(near, 0, right));
        // (normal is normalized in the BasicEffect HLSL.)
        _triangleBatch.Add(ref corner1, ref corner2, ref corner5, ref n, ref job.Color);
        _triangleBatch.Add(ref corner2, ref corner6, ref corner5, ref n, ref job.Color);

        // Left face
        n = job.Pose.ToWorldDirection(new Vector3F(-near, 0, -left));
        _triangleBatch.Add(ref corner0, ref corner4, ref corner3, ref n, ref job.Color);
        _triangleBatch.Add(ref corner3, ref corner4, ref corner7, ref n, ref job.Color);

        // Top face
        n = job.Pose.ToWorldDirection(new Vector3F(0, near, top));
        _triangleBatch.Add(ref corner3, ref corner7, ref corner2, ref n, ref job.Color);
        _triangleBatch.Add(ref corner7, ref corner6, ref corner2, ref n, ref job.Color);

        // Bottom face
        n = job.Pose.ToWorldDirection(new Vector3F(0, -near, -bottom));
        _triangleBatch.Add(ref corner0, ref corner1, ref corner4, ref n, ref job.Color);
        _triangleBatch.Add(ref corner4, ref corner1, ref corner5, ref n, ref job.Color);

        // Near face
        n = job.Pose.Orientation.GetColumn(2);
        _triangleBatch.Add(ref corner0, ref corner2, ref corner1, ref n, ref job.Color);
        _triangleBatch.Add(ref corner2, ref corner0, ref corner3, ref n, ref job.Color);

        // Far face
        n = -n;
        _triangleBatch.Add(ref corner4, ref corner5, ref corner7, ref n, ref job.Color);
        _triangleBatch.Add(ref corner7, ref corner5, ref corner6, ref n, ref job.Color);

        _triangleBatch.Render(context);
        _triangleBatch.Clear();
      }
    }


    private void RenderSphere(IGraphicsService graphicsService, PrimitiveJob job, RenderContext context)
    {
      if (DrawWireFrame)
      {
        Pose cameraPose = context.CameraNode.PoseWorld;
        Matrix44F cameraProjection = context.CameraNode.Camera.Projection;
        if (Numeric.AreEqual(1.0f, cameraProjection.M33))
        {
          // Orthographic projection
          Vector3F position = job.Pose.Position;
          Vector3F right = cameraPose.Orientation.GetColumn(0);
          Vector3F up = cameraPose.Orientation.GetColumn(1);
          Vector3F forward = cameraPose.Orientation.GetColumn(2);
          Matrix pose = new Matrix(right.X, right.Y, right.Z, 0,
                                   up.X, up.Y, up.Z, 0,
                                   forward.X, forward.Y, forward.Z, 0,
                                   position.X, position.Y, position.Z, 1);
          Effect.World = Matrix.CreateScale(job.Size.Radius) * pose;
          Effect.CurrentTechnique.Passes[0].Apply();

          if (_circleLinePrimitive == null)
            _circleLinePrimitive = MeshHelper.GetCircleLines(graphicsService);

          _circleLinePrimitive.Draw();
        }
        else
        {
          // Perspective projection
          Vector3F right = cameraPose.Orientation * Vector3F.Right;
          RenderSphereOutline(job.Size.Radius, ref job.Pose.Position, ref cameraPose, ref right, ref job.Color);
        }
      }
      else
      {
        Effect.World = Matrix.CreateScale(job.Size.Radius) * job.Pose;
        Effect.CurrentTechnique.Passes[0].Apply();

        if (_icospherePrimitive == null)
          _icospherePrimitive = MeshHelper.GetIcosphere(graphicsService);

        _icospherePrimitive.Draw();
      }
    }


    // Render outline of sphere in perspective projection.
    private void RenderSphereOutline(float radius, ref Vector3F center, ref Pose cameraPose, ref Vector3F right, ref Color color)
    {
      // TODO: Try alternative algorithm. See http://www.gamasutra.com/view/feature/131351/the_mechanics_of_robust_stencil_.php?page=6

      // Forward direction of camera in world space.
      Vector3F forward = cameraPose.Orientation * Vector3F.Forward;

      // Vector pointing from camera position to center of sphere.
      Vector3F cameraToCenter = center - cameraPose.Position;
      float cameraToCenterLength = cameraToCenter.Length;
      if (Numeric.IsLessOrEqual(cameraToCenterLength, radius))
      {
        // Camera is within sphere.
        return;
      }

      cameraToCenter /= cameraToCenterLength;

      // ----- Next, find the outline of the sphere, which is visible from the camera.

      // We need to find cameraToPoint (the line from the camera to a point on the outline).
      // cameraToPoint (= adjacent), radius (= opposite) and cameraToCenter (= hypotenuse)
      // form a right-angled triangle.
      // α is the angle between cameraToPoint and cameraToCenter:
      float α = (float)Math.Asin(radius / cameraToCenterLength);

      // Determine the normal of the triangle.
      Vector3F normal = Vector3F.Cross(right, cameraToCenter);
      if (normal.IsNumericallyZero)
        normal = Vector3F.Up;

      // adjacent = √(hypotenuse² + opposite²)
      float radiusSquared = radius * radius;
      float cameraToCenterLengthSquared = cameraToCenterLength * cameraToCenterLength;
      float cameraToPointLength = (float)Math.Sqrt(cameraToCenterLengthSquared - radiusSquared);
      Vector3F cameraToPoint = Matrix33F.CreateRotation(normal, α) * cameraToCenter;

      // Get the first point on the outline in world space.
      Vector3F pStart = cameraPose.Position + cameraToPoint * cameraToPointLength;

      // Incrementally rotate the right vector 360° and repeat to find the remaining 
      // points on the outline.
      const int NumberOfSegments = 32;
      for (int i = 1; i <= NumberOfSegments; i++)
      {
        float β = i * ConstantsF.TwoPi / NumberOfSegments;

        Vector3F rightRotated = Matrix33F.CreateRotation(forward, -β) * right;
        normal = Vector3F.Cross(rightRotated, cameraToCenter);
        if (normal.IsNumericallyZero)
          normal = Vector3F.Up;

        cameraToPointLength = (float)Math.Sqrt(cameraToCenterLengthSquared - radiusSquared);
        cameraToPoint = Matrix33F.CreateRotation(normal, α) * cameraToCenter;
        Vector3F pEnd = cameraPose.Position + cameraToPoint * cameraToPointLength;
        _lineBatch.Add(pStart, pEnd, color);
        pStart = pEnd;
      }
    }


    private void RenderCapsule(IGraphicsService graphicsService, PrimitiveJob job)
    {
      float radius = job.Size.Radius;
      float height = job.Size.Height;

      if (DrawWireFrame)
      {
        if (_hemisphereLinePrimitive == null)
          _hemisphereLinePrimitive = MeshHelper.GetHemisphereLines(graphicsService);

        Effect.World = Matrix.CreateScale(radius) * Matrix.CreateTranslation(0, height / 2 - radius, 0) * job.Pose;
        Effect.CurrentTechnique.Passes[0].Apply();
        _hemisphereLinePrimitive.Draw();

        Effect.World = Matrix.CreateScale(-radius, -radius, radius) * Matrix.CreateTranslation(0, -height / 2 + radius, 0) * job.Pose;
        Effect.CurrentTechnique.Passes[0].Apply();
        _hemisphereLinePrimitive.Draw();

        // 4 lines
        Vector3F dx = radius * job.Pose.ToWorldDirection(new Vector3F(1, 0, 0));
        Vector3F dy = (height / 2 - radius) * job.Pose.ToWorldDirection(new Vector3F(0, 1, 0));
        Vector3F dz = radius * job.Pose.ToWorldDirection(new Vector3F(0, 0, 1));
        Vector3F center = job.Pose.Position;
        _lineBatch.Add(center + dy + dx, center - dy + dx, job.Color);
        _lineBatch.Add(center + dy - dx, center - dy - dx, job.Color);
        _lineBatch.Add(center + dy + dz, center - dy + dz, job.Color);
        _lineBatch.Add(center + dy - dz, center - dy - dz, job.Color);
      }
      else
      {
        if (_hemispherePrimitive == null)
          _hemispherePrimitive = MeshHelper.GetHemisphere(graphicsService);

        Effect.World = Matrix.CreateScale(radius) * Matrix.CreateTranslation(0, height / 2 - radius, 0) * job.Pose;
        Effect.CurrentTechnique.Passes[0].Apply();
        _hemispherePrimitive.Draw();

        Effect.World = Matrix.CreateScale(-radius, -radius, radius) * Matrix.CreateTranslation(0, -height / 2 + radius, 0) * job.Pose;
        Effect.CurrentTechnique.Passes[0].Apply();
        _hemispherePrimitive.Draw();

        if (_uncappedCylinderPrimitive == null)
          _uncappedCylinderPrimitive = MeshHelper.GetUncappedCylinder(graphicsService);

        Effect.World = Matrix.CreateScale(radius, height / 2 - radius, radius) * job.Pose;
        Effect.CurrentTechnique.Passes[0].Apply();

        _uncappedCylinderPrimitive.Draw();
      }
    }


    private void RenderCone(IGraphicsService graphicsService, PrimitiveJob job)
    {
      float radius = job.Size.Radius;
      float height = job.Size.Height;

      Effect.World = Matrix.CreateScale(radius, height, radius) * job.Pose;
      Effect.CurrentTechnique.Passes[0].Apply();

      if (DrawWireFrame)
      {
        if (_coneLinePrimitive == null)
          _coneLinePrimitive = MeshHelper.GetConeLines(graphicsService);

        _coneLinePrimitive.Draw();
      }
      else
      {
        if (_conePrimitive == null)
          _conePrimitive = MeshHelper.GetCone(graphicsService);

        _conePrimitive.Draw();
      }
    }


    private void RenderCylinder(IGraphicsService graphicsService, PrimitiveJob job)
    {
      float radius = job.Size.Radius;
      float height = job.Size.Height;

      Effect.World = Matrix.CreateScale(radius, height / 2, radius) * job.Pose;
      Effect.CurrentTechnique.Passes[0].Apply();

      if (DrawWireFrame)
      {
        if (_cylinderLinePrimitive == null)
          _cylinderLinePrimitive = MeshHelper.GetCylinderLines(graphicsService);

        _cylinderLinePrimitive.Draw();
      }
      else
      {
        if (_cylinderPrimitive == null)
          _cylinderPrimitive = MeshHelper.GetCylinder(graphicsService);

        _cylinderPrimitive.Draw();
      }
    }


    private void RenderShape(GraphicsDevice graphicsDevice, PrimitiveJob job)
    {
      Submesh submesh;
      Matrix44F matrix;
      ShapeMeshCache.GetMesh(_graphicsService, job.Shape, out submesh, out matrix);
      if (submesh.VertexBuffer == null)
        return;   // This could happen for shapes without a mesh, like an InfiniteShape.

      Effect.World = (Matrix)matrix * Matrix.CreateScale((Vector3)job.Size.Scale) * job.Pose;
      Effect.CurrentTechnique.Passes[0].Apply();

      var originalRasterizerState = graphicsDevice.RasterizerState;
      var triangleMeshShape = job.Shape as TriangleMeshShape;
      if (triangleMeshShape != null && triangleMeshShape.IsTwoSided && originalRasterizerState.CullMode != CullMode.None)
      {
        if (AutoRasterizerState)
        {
          // For two-sided meshes we disable back-face culling.
          graphicsDevice.RasterizerState = DrawWireFrame ? GraphicsHelper.RasterizerStateWireFrame : GraphicsHelper.RasterizerStateCullNone;
          submesh.Draw();
          graphicsDevice.RasterizerState = originalRasterizerState;
        }
        else
        {
          submesh.Draw();
        }
      }
      else
      {
        submesh.Draw();
      }
    }


    private void RenderModel(GraphicsDevice graphicsDevice, PrimitiveJob job)
    {
      var numberOfBones = job.Model.Bones.Count;
      if (_boneTransforms == null || _boneTransforms.Length < numberOfBones)
        _boneTransforms = new Matrix[numberOfBones];

      job.Model.CopyAbsoluteBoneTransformsTo(_boneTransforms);
      foreach (var mesh in job.Model.Meshes)
      {
        Effect.World = _boneTransforms[mesh.ParentBone.Index] * Matrix.CreateScale((Vector3)job.Size.Scale) * job.Pose;
        Effect.CurrentTechnique.Passes[0].Apply();

        foreach (var part in mesh.MeshParts)
        {
          graphicsDevice.SetVertexBuffer(part.VertexBuffer);
          graphicsDevice.Indices = part.IndexBuffer;
#if MONOGAME
          graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
#else
          graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
#endif
        }
      }
    }


    private void RenderSubmesh(PrimitiveJob job)
    {
      Effect.World = Matrix.CreateScale((Vector3)job.Size.Scale) * job.Pose;
      Effect.CurrentTechnique.Passes[0].Apply();

      job.Submesh.Draw();
    }
    #endregion
  }
}
