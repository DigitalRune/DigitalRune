// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = Microsoft.Xna.Framework.MathHelper;


namespace DigitalRune.Graphics
{
  partial class GraphicsHelper
  {
    /// <overloads>
    /// <summary>
    /// Projects a position into screen space.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Projects a position from object space into screen space.
    /// </summary>
    /// <param name="viewport">The viewport.</param>
    /// <param name="position">The position in object space.</param>
    /// <param name="projection">The projection matrix.</param>
    /// <param name="view">The view matrix.</param>
    /// <param name="world">The world matrix.</param>
    /// <returns>
    /// The position in screen space: The x- and y-components define the pixel position. The 
    /// z-component defines the depth in clip space. (The depth of the clipping volume ranges from 
    /// <see cref="Viewport.MinDepth"/> to <see cref="Viewport.MaxDepth"/> - usually [0, 1].)
    /// </returns>
    public static Vector3F Project(this Viewport viewport, Vector3F position, Matrix44F projection, Matrix44F view, Matrix44F world)
    {
      Matrix44F worldViewProjection = projection * view * world;
      return Project(viewport, position, worldViewProjection);
    }


    /// <summary>
    /// Projects a position from world space into screen space.
    /// </summary>
    /// <param name="viewport">The viewport.</param>
    /// <param name="position">The position in world space.</param>
    /// <param name="projection">The projection matrix.</param>
    /// <param name="view">The view matrix.</param>
    /// <returns>
    /// The position in screen space: The x- and y-components define the pixel position. The 
    /// z-component defines the depth in clip space. (The depth of the clipping volume ranges from 
    /// <see cref="Viewport.MinDepth"/> to <see cref="Viewport.MaxDepth"/> - usually [0, 1].)
    /// </returns>
    public static Vector3F Project(this Viewport viewport, Vector3F position, Matrix44F projection, Matrix44F view)
    {
      Matrix44F viewProjection = projection * view;
      return Project(viewport, position, viewProjection);
    }


    /// <summary>
    /// Projects a position from world space into screen space.
    /// </summary>
    /// <param name="viewport">The viewport.</param>
    /// <param name="position">The position in view space.</param>
    /// <param name="projection">The projection matrix.</param>
    /// <returns>
    /// The position in screen space: The x- and y-components define the pixel position. The 
    /// z-component defines the depth in clip space mapped to the range
    /// [<see cref="Viewport.MinDepth"/>, <see cref="Viewport.MaxDepth"/>] (usually [0, 1]).
    /// </returns>
    public static Vector3F Project(this Viewport viewport, Vector3F position, Matrix44F projection)
    {
      // Transform position to clip space. (TransformPosition() transforms the position 
      // to clip space and performs the homogeneous divide.)
      Vector3F positionClip = projection.TransformPosition(position);
      Vector3F positionScreen = new Vector3F
      {
        X = (1f + positionClip.X) * 0.5f * viewport.Width + viewport.X,
        Y = (1f - positionClip.Y) * 0.5f * viewport.Height + viewport.Y,
        Z = positionClip.Z * (viewport.MaxDepth - viewport.MinDepth) + viewport.MinDepth
      };
      return positionScreen;
    }


    /// <summary>
    /// Projects a position from world space into viewport.
    /// </summary>
    /// <param name="viewport">The viewport.</param>
    /// <param name="position">The position in view space.</param>
    /// <param name="projection">The projection matrix.</param>
    /// <returns>
    /// The position in the viewport: The x- and y-components define the pixel position
    /// in the range [0, viewport width/height]. The z-component defines the depth in clip space.
    /// </returns>
    internal static Vector3F ProjectToViewport(this Viewport viewport, Vector3F position, Matrix44F projection)
    {
      // Transform position to clip space. (TransformPosition() transforms the position 
      // to clip space and performs the homogeneous divide.)
      Vector3F positionClip = projection.TransformPosition(position);
      Vector3F positionScreen = new Vector3F
      {
        X = (1f + positionClip.X) * 0.5f * viewport.Width,
        Y = (1f - positionClip.Y) * 0.5f * viewport.Height,
        Z = positionClip.Z,
      };
      return positionScreen;
    }


    /// <overloads>
    /// <summary>
    /// Projects a position back from screen space.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Projects a position from screen space into object space.
    /// </summary>
    /// <param name="viewport">The <see cref="Viewport"/>.</param>
    /// <param name="position">
    /// The position in screen space: The x- and y-components define the pixel position. The 
    /// z-component defines the depth in clip space. (The depth of the clipping volume ranges from 
    /// <see cref="Viewport.MinDepth"/> to <see cref="Viewport.MaxDepth"/> - usually [0, 1].)
    /// </param>
    /// <param name="projection">The projection matrix.</param>
    /// <param name="view">The view matrix.</param>
    /// <param name="world">The world matrix.</param>
    /// <returns>The position in object space.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Vector3F Unproject(this Viewport viewport, Vector3F position, Matrix44F projection, Matrix44F view, Matrix44F world)
    {
      Matrix44F worldViewProjection = projection * view * world;
      return Unproject(viewport, position, worldViewProjection);
    }


    /// <summary>
    /// Projects a position from screen space into world space.
    /// </summary>
    /// <param name="viewport">The <see cref="Viewport"/>.</param>
    /// <param name="position">
    /// The position in screen space: The x- and y-components define the pixel position. The 
    /// z-component defines the depth in clip space. (The depth of the clipping volume ranges from 
    /// <see cref="Viewport.MinDepth"/> to <see cref="Viewport.MaxDepth"/> - usually [0, 1].)
    /// </param>
    /// <param name="projection">The projection matrix.</param>
    /// <param name="view">The view matrix.</param>
    /// <returns>The position in world space.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Vector3F Unproject(this Viewport viewport, Vector3F position, Matrix44F projection, Matrix44F view)
    {
      Matrix44F worldViewProjection = projection * view;
      return Unproject(viewport, position, worldViewProjection);
    }


    /// <summary>
    /// Projects a position from screen space into view space.
    /// </summary>
    /// <param name="viewport">The <see cref="Viewport"/>.</param>
    /// <param name="position">
    /// The position in screen space: The x- and y-components define the pixel position. The 
    /// z-component defines the depth in clip space. (The depth of the clipping volume ranges from 
    /// <see cref="Viewport.MinDepth"/> to <see cref="Viewport.MaxDepth"/> - usually [0, 1].)
    /// </param>
    /// <param name="projection">The projection matrix.</param>
    /// <returns>The position in view space.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Vector3F Unproject(this Viewport viewport, Vector3F position, Matrix44F projection)
    {
      Matrix44F fromClipSpace = projection.Inverse;
      Vector3F positionClip = new Vector3F
      {
        X = (position.X - viewport.X) / viewport.Width * 2f - 1f,
        Y = -((position.Y - viewport.Y) / viewport.Height * 2f - 1f),
        Z = (position.Z - viewport.MinDepth) / (viewport.MaxDepth - viewport.MinDepth),
      };

      // Transform position from clip space to the desired coordinate space. 
      // (TransformPosition() undoes the homogeneous divide and transforms the 
      // position from clip space to the desired coordinate space.)
      return fromClipSpace.TransformPosition(positionClip);
    }


    /// <overloads>
    /// <summary>
    /// Gets a scissor rectangle that encloses the specified object.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets a scissor rectangle that encloses the specified sphere.
    /// </summary>
    /// <param name="cameraNode">The camera node.</param>
    /// <param name="viewport">The viewport.</param>
    /// <param name="positionWorld">The sphere center in world space.</param>
    /// <param name="radius">The sphere radius.</param>
    /// <returns>The scissor rectangle.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="cameraNode"/> is <see langword="null"/>.
    /// </exception>
    public static Rectangle GetScissorRectangle(CameraNode cameraNode, Viewport viewport, Vector3F positionWorld, float radius)
    {
      var rectangle = GetViewportRectangle(cameraNode, viewport, positionWorld, radius);
      rectangle.X += viewport.X;
      rectangle.Y += viewport.Y;

      return rectangle;
    }


    /// <summary>
    /// Gets a scissor rectangle that encloses the specified geometric object.
    /// </summary>
    /// <param name="cameraNode">The camera node.</param>
    /// <param name="viewport">The viewport.</param>
    /// <param name="geometricObject">The geometric object.</param>
    /// <returns>The scissor rectangle.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="cameraNode"/> or <paramref name="geometricObject"/> is 
    /// <see langword="null"/>.
    /// </exception>
    public static Rectangle GetScissorRectangle(CameraNode cameraNode, Viewport viewport, IGeometricObject geometricObject)
    {
      var rectangle = GetViewportRectangle(cameraNode, viewport, geometricObject);
      rectangle.X += viewport.X;
      rectangle.Y += viewport.Y;

      return rectangle;
    }


    /// <overloads>
    /// <summary>
    /// Gets a rectangle that encloses the specified object in the viewport.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the rectangle that encloses the specified sphere in the viewport.
    /// </summary>
    /// <param name="cameraNode">The camera node.</param>
    /// <param name="viewport">The viewport.</param>
    /// <param name="positionWorld">The sphere center in world space.</param>
    /// <param name="radius">The sphere radius.</param>
    /// <returns>The rectangle that encloses the sphere.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="cameraNode"/> is <see langword="null"/>.
    /// </exception>
    internal static Rectangle GetViewportRectangle(CameraNode cameraNode, Viewport viewport, Vector3F positionWorld, float radius)
    {
      if (cameraNode == null)
        throw new ArgumentNullException("cameraNode");

      // Relative bounds: (left, top, right, bottom).
      Vector4F bounds = GetBounds(cameraNode, positionWorld, radius);

      // Rectangle in viewport.
      int left = (int)(bounds.X * viewport.Width);               // implicit floor()
      int top = (int)(bounds.Y * viewport.Height);               // implicit floor()
      int right = (int)Math.Ceiling(bounds.Z * viewport.Width);
      int bottom = (int)Math.Ceiling(bounds.W * viewport.Height);
      return new Rectangle(left, top, right - left, bottom - top);
    }


    /// <summary>
    /// Gets the rectangle that encloses the specified geometric object in the viewport.
    /// </summary>
    /// <param name="cameraNode">The camera node.</param>
    /// <param name="viewport">The viewport.</param>
    /// <param name="geometricObject">The geometric object.</param>
    /// <returns>The rectangle that encloses <paramref name="geometricObject"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="cameraNode"/> or <paramref name="geometricObject"/> is 
    /// <see langword="null"/>.
    /// </exception>
    internal static Rectangle GetViewportRectangle(CameraNode cameraNode, Viewport viewport, IGeometricObject geometricObject)
    {
      if (cameraNode == null)
        throw new ArgumentNullException("cameraNode");
      if (geometricObject == null)
        throw new ArgumentNullException("geometricObject");

      // For a uniformly scaled sphere we can use the specialized GetScissorRectangle method.
      var sphereShape = geometricObject.Shape as SphereShape;
      if (sphereShape != null)
      {
        Vector3F scale = geometricObject.Scale;
        if (scale.X == scale.Y && scale.Y == scale.Z)
        {
          return GetViewportRectangle(cameraNode, viewport, geometricObject.Pose.Position,
            scale.X * sphereShape.Radius);
        }
      }

      // Relative bounds: (left, top, right, bottom).
      Vector4F bounds = GetBounds(cameraNode, geometricObject);

      // Rectangle in viewport.
      int left = (int)(bounds.X * viewport.Width);               // implicit floor()
      int top = (int)(bounds.Y * viewport.Height);               // implicit floor()
      int right = (int)Math.Ceiling(bounds.Z * viewport.Width);
      int bottom = (int)Math.Ceiling(bounds.W * viewport.Height);
      return new Rectangle(left, top, right - left, bottom - top);
    }


    /// <summary>
    /// Gets the bounds of the specified sphere relative to the viewport.
    /// </summary>
    /// <param name="cameraNode">The camera node.</param>
    /// <param name="positionWorld">The sphere center in world space.</param>
    /// <param name="radius">The sphere radius.</param>
    /// <returns>
    /// The bounds (left, top, right, bottom) where each entry is in the range [0, 1].
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="cameraNode"/> is <see langword="null"/>.
    /// </exception>
    internal static Vector4F GetBounds(CameraNode cameraNode, Vector3F positionWorld, float radius)
    {
      var camera = cameraNode.Camera;
      var projection = camera.Projection;
      float near = projection.Near;
      float left = projection.Left;
      float width = projection.Width;
      float top = projection.Top;
      float height = projection.Height;

      Vector3F l = cameraNode.PoseWorld.ToLocalPosition(positionWorld);
      float r = radius;

      // Default bounds (left, top, right, bottom)
      var bounds = new Vector4F(0, 0, 1, 1);

      // ----- Solve for N = (x, 0, z).

      // Discriminant already divided by 4:
      float d = (r * r * l.X * l.X - (l.X * l.X + l.Z * l.Z) * (r * r - l.Z * l.Z));
      if (d > 0)
      {
        // Camera is outside the sphere.

        float rootD = (float)Math.Sqrt(d);

        // Now check two possible solutions (+/- rootD):
        float nx1 = (r * l.X + rootD) / (l.X * l.X + l.Z * l.Z);
        float nx2 = (r * l.X - rootD) / (l.X * l.X + l.Z * l.Z);

        float nz1 = (r - nx1 * l.X) / l.Z;
        float nz2 = (r - nx2 * l.X) / l.Z;

        // Compute tangent position (px, 0, pz) on the sphere.
        float pz1 = (l.X * l.X + l.Z * l.Z - r * r) / (l.Z - (nz1 / nx1) * l.X);
        float pz2 = (l.X * l.X + l.Z * l.Z - r * r) / (l.Z - (nz2 / nx2) * l.X);

        if (pz1 < 0)
        {
          // Plane (nx1, 0, nz1) is within camera frustum.

          float px = -pz1 * nz1 / nx1;

          float x = nz1 * near / nx1;             // x coordinate on the near plane.
          float boundsX = (x - left) / width;    // Value relative to viewport. (0 = left, 1 = right)

          // Shrink the scissor rectangle on the left or on the right side.
          if (px < l.X)
            bounds.X = Math.Max(bounds.X, boundsX);
          else
            bounds.Z = Math.Min(bounds.Z, boundsX);
        }

        if (pz2 < 0)
        {
          float px = -pz2 * nz2 / nx2;

          float x = nz2 * near / nx2;
          float scissorX = (x - left) / width;

          if (px < l.X)
            bounds.X = Math.Max(bounds.X, scissorX);
          else
            bounds.Z = Math.Min(bounds.Z, scissorX);
        }
      }

      // ----- Solve for N = (0, y, z) first.

      d = (r * r * l.Y * l.Y - (l.Y * l.Y + l.Z * l.Z) * (r * r - l.Z * l.Z));
      if (d > 0)
      {
        // Camera is outside the sphere.

        float rootD = (float)Math.Sqrt(d);

        float ny1 = (r * l.Y + rootD) / (l.Y * l.Y + l.Z * l.Z);
        float ny2 = (r * l.Y - rootD) / (l.Y * l.Y + l.Z * l.Z);

        float nz1 = (r - ny1 * l.Y) / l.Z;
        float nz2 = (r - ny2 * l.Y) / l.Z;

        float pz1 = (l.Y * l.Y + l.Z * l.Z - r * r) / (l.Z - (nz1 / ny1) * l.Y);
        float pz2 = (l.Y * l.Y + l.Z * l.Z - r * r) / (l.Z - (nz2 / ny2) * l.Y);

        if (pz1 < 0)
        {
          float py = -pz1 * nz1 / ny1;

          float y = nz1 * near / ny1;
          float scissorY = -(y - top) / height;

          if (py > l.Y)
            bounds.Y = Math.Max(bounds.Y, scissorY);
          else
            bounds.W = Math.Min(bounds.W, scissorY);
        }

        if (pz2 < 0)
        {
          float py = -pz2 * nz2 / ny2;

          float y = nz2 * near / ny2;
          float scissorY = -(y - top) / height;

          if (py > l.Y)
            bounds.Y = Math.Max(bounds.Y, scissorY);
          else
            bounds.W = Math.Min(bounds.W, scissorY);
        }
      }

      bounds.X = MathHelper.Clamp(bounds.X, 0, 1);
      bounds.Y = MathHelper.Clamp(bounds.Y, 0, 1);
      bounds.Z = MathHelper.Clamp(bounds.Z, bounds.X, 1);
      bounds.W = MathHelper.Clamp(bounds.W, bounds.Y, 1);

      return bounds;
    }


    /// <overloads>
    /// <summary>
    /// Gets a the bounds of the specified object relative to the viewport.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets a the bounds of the specified geometric object relative to the viewport.
    /// </summary>
    /// <param name="cameraNode">The camera node.</param>
    /// <param name="geometricObject">The geometric object.</param>
    /// <returns>
    /// The bounds (left, top, right, bottom) where each entry is in the range [0, 1].
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="cameraNode"/> or <paramref name="geometricObject"/> is 
    /// <see langword="null"/>.
    /// </exception>
    internal static Vector4F GetBounds(CameraNode cameraNode, IGeometricObject geometricObject)
    {
      // Notes:
      // Do not call this GetBounds() method for spheres. Use the other overload for spheres.
      //
      // At first this problem seems trivial, we only have to get the support 
      // points of the geometric object's shape in the directions of the frustum
      // plane normal vectors. The we project these points to the near plane...
      // But this does not work because which can be seen if you draw simple
      // drop down sketch. Actually each eye ray has its own direction therefore
      // it has its normal and its own support direction!

      if (cameraNode == null)
        throw new ArgumentNullException("cameraNode");
      if (geometricObject == null)
        throw new ArgumentNullException("geometricObject");

      Debug.Assert(!(geometricObject.Shape is SphereShape), "Call a different GetBounds() overload for spheres!");

      // Projection properties.
      var camera = cameraNode.Camera;
      var projection = camera.Projection;
      float near = projection.Near;
      float left = projection.Left;
      float right = projection.Right;
      float width = projection.Width;
      float top = projection.Top;
      float bottom = projection.Bottom;
      float height = projection.Height;

      // Get AABB in view space.
      Pose localToViewPose = cameraNode.PoseWorld.Inverse * geometricObject.Pose;
      Aabb aabb = geometricObject.Shape.GetAabb(geometricObject.Scale, localToViewPose);

      // Is the AABB in front of the near plane (= totally clipped)?
      if (aabb.Minimum.Z >= -near)
        return new Vector4F(0);

      // Does the AABB contain the origin?
      if (GeometryHelper.HaveContact(aabb, Vector3F.Zero))
        return new Vector4F(0, 0, 1, 1);

      // Project the AABB far face to the near plane.
      Vector2F min;
      min.X = aabb.Minimum.X / -aabb.Minimum.Z * near;
      min.Y = aabb.Minimum.Y / -aabb.Minimum.Z * near;
      Vector2F max;
      max.X = aabb.Maximum.X / -aabb.Minimum.Z * near;
      max.Y = aabb.Maximum.Y / -aabb.Minimum.Z * near;

      // If the AABB z extent overlaps the origin, some results are invalid.
      if (aabb.Maximum.Z > -Numeric.EpsilonF)
      {
        if (aabb.Minimum.X < 0)
          min.X = left;
        if (aabb.Maximum.X > 0)
          max.X = right;
        if (aabb.Minimum.Y < 0)
          min.Y = bottom;
        if (aabb.Maximum.Y > 0)
          max.Y = top;
      }
      else
      {
        // The AABB near face is also in front. Project AABB near face to near plane
        // and take the most extreme.
        min.X = Math.Min(min.X, aabb.Minimum.X / -aabb.Maximum.Z * near);
        min.Y = Math.Min(min.Y, aabb.Minimum.Y / -aabb.Maximum.Z * near);
        max.X = Math.Max(max.X, aabb.Maximum.X / -aabb.Maximum.Z * near);
        max.Y = Math.Max(max.Y, aabb.Maximum.Y / -aabb.Maximum.Z * near);
      }

      Vector4F bounds;
      bounds.X = (min.X - left) / width;
      bounds.Y = (top - max.Y) / height;
      bounds.Z = (max.X - left) / width;
      bounds.W = (top - min.Y) / height;

      bounds.X = MathHelper.Clamp(bounds.X, 0, 1);
      bounds.Y = MathHelper.Clamp(bounds.Y, 0, 1);
      bounds.Z = MathHelper.Clamp(bounds.Z, bounds.X, 1);
      bounds.W = MathHelper.Clamp(bounds.W, bounds.Y, 1);

      return bounds;
    }


    /// <summary>
    /// Estimates the size of an object in pixels.
    /// </summary>
    /// <param name="cameraNode">The camera node with perspective projection.</param>
    /// <param name="viewport">The viewport.</param>
    /// <param name="geometricObject">The geometric object.</param>
    /// <returns>
    /// The estimated width and height of <paramref name="geometricObject"/> in pixels.
    /// </returns>
    /// <remarks>
    /// The method assumes that the object is fully visible by the camera, i.e. it does not perform
    /// frustum culling. It estimates the size of <paramref name="geometricObject"/> based on its 
    /// bounding shape.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="cameraNode"/> or <paramref name="geometricObject"/> is 
    /// <see langword="null"/>.
    /// </exception>
    internal static Vector2F GetScreenSize(CameraNode cameraNode, Viewport viewport, IGeometricObject geometricObject)
    {
      // This implementation is just for reference. (It is preferable to optimize 
      // and inline the code when needed.)

      if (cameraNode == null)
        throw new ArgumentNullException("cameraNode");
      if (geometricObject == null)
        throw new ArgumentNullException("geometricObject");

      // Use bounding sphere of AABB in world space.
      var aabb = geometricObject.Aabb;
      float diameter = aabb.Extent.Length;
      float width = diameter;
      float height = diameter;

      Matrix44F proj = cameraNode.Camera.Projection;
      bool isOrthographic = (proj.M33 != 0);

      // ----- xScale, yScale:
      // Orthographic Projection:
      //   proj.M00 = 2 / (right - left)
      //   proj.M11 = 2 / (top - bottom)
      // 
      // Perspective Projection:
      //   proj.M00 = 2 * zNear / (right - left) = 1 / tan(fovX/2)
      //   proj.M11 = 2 * zNear / (top - bottom) = 1 / tan(fovY/2)
      float xScale = Math.Abs(proj.M00);
      float yScale = Math.Abs(proj.M11);

      // Screen size [px].
      Vector2F screenSize;

      if (isOrthographic)
      {
        // ----- Orthographic Projection
        // sizeX = viewportWidth * width / (right - left)
        //       = viewportWidth * width * xScale / 2
        screenSize.X = viewport.Width * width * xScale / 2;

        // sizeY = viewportHeight* height / (top - bottom)
        //       = viewportHeight* height * xScale / 2
        screenSize.Y = viewport.Height * height * yScale / 2;
      }
      else
      {
        // ----- Perspective Projection
        // Camera properties.
        Pose cameraPose = cameraNode.PoseWorld;
        Vector3F cameraPosition = cameraPose.Position;
        Matrix33F cameraOrientation = cameraPose.Orientation;
        Vector3F cameraForward = -cameraOrientation.GetColumn(2);

        // Get planar distance from camera to object by projecting the distance
        // vector onto the look direction.
        Vector3F cameraToObject = aabb.Center - cameraPosition;
        float distance = Vector3F.Dot(cameraToObject, cameraForward);

        // Assume that object is in front of camera (no frustum culling).
        distance = Math.Abs(distance);

        // Avoid division by zero.
        if (distance < Numeric.EpsilonF)
          distance = Numeric.EpsilonF;

        // sizeX = viewportWidth * width / (objectDistance * 2 * tan(fovX/2))
        //       = viewportWidth * width * zNear / (objectDistance * (right - left))
        //       = viewportWidth * width * xScale / (2 * objectDistance)
        screenSize.X = viewport.Width * width * xScale / (2 * distance);

        // sizeY = viewportHeight * height / (objectDistance * 2 * tan(fovY/2))
        //       = viewportHeight * height * zNear / (objectDistance * (top - bottom))
        //       = viewportHeight * height * yScale / (2 * objectDistance)
        screenSize.Y = viewport.Height * height * yScale / (2 * distance);
      }

      return screenSize;
    }


    /// <overloads>
    /// <summary>
    /// Gets the view-normalized distance ("LOD distance").
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Calculates the view-normalized distance ("LOD distance") of the specified scene node.
    /// </summary>
    /// <param name="sceneNode">The scene node.</param>
    /// <param name="cameraNode">The camera node.</param>
    /// <returns>The view-normalized distance.</returns>
    /// <remarks>
    /// <para>
    /// The <i>view-normalized distance</i> is defined as:
    /// </para>
    /// <para>
    /// <i>distance<sub>normalized</sub></i> = <i>distance</i> / <i>yScale</i>
    /// </para>
    /// <para>
    /// where <i>distance</i> is the 3D Euclidean distance between the camera and the object.
    /// <i>yScale</i> is the second diagonal entry of the projection matrix.
    /// </para>
    /// <para>
    /// For symmetric perspective projections the above equation is the same as
    /// </para>
    /// <para>
    /// <i>distance<sub>normalized</sub></i> = <i>distance</i> * tan(<i>fov<sub>Y</sub></i> / 2)
    /// </para>
    /// <para>
    /// where <i>fov<sub>Y</sub></i> is the camera's vertical field-of-view.
    /// </para>
    /// <para>
    /// In other words, the view-normalized distance is the camera distance times a camera 
    /// correction factor. The correction factor accounts for the camera field-of-view. The 
    /// resulting value is inversely proportional to the screen size of the object and independent
    /// of the current field-of-view. It can be used to specify LOD distances or similar metrics.
    /// </para>
    /// <para>
    /// Note that tan(<i>fov<sub>Y</sub></i>/2) is 1 if the <i>fov<sub>Y</sub></i> = 90°. This means 
    /// that distance and view-normalized distance are identical for a camera with a vertical 
    /// field-of-view of 90°. (This 90° FOV camera is the "reference camera".)
    /// </para>
    /// <para>
    /// The view-normalized distance is only defined for cameras with perspective projections. The 
    /// result is undefined for orthographic projections!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static float GetViewNormalizedDistance(SceneNode sceneNode, CameraNode cameraNode)
    {
      Debug.Assert(
        sceneNode.ScaleWorld.X > 0 && sceneNode.ScaleWorld.Y > 0 && sceneNode.ScaleWorld.Z > 0,
        "Assuming that all scale factors are positive.");

      Pose cameraPose = cameraNode.PoseWorld;
      Vector3F cameraToObject = sceneNode.PoseWorld.Position - cameraPose.Position;

      // Get planar distance by projecting the distance vector onto the look direction.
      // This is stable for sideways movement but unstable for camera rotations.
      //Vector3F cameraForward = -cameraPose.Orientation.GetColumn(2);
      //float distance = Math.Abs(Vector3F.Dot(cameraToObject, cameraForward));

      // Get normal (radial) distance (stable for camera rotations, unstable for sideways movement).
      float distance = cameraToObject.Length;

      // Make distance independent of current FOV and scale.
      distance = GetViewNormalizedDistance(distance, cameraNode.Camera.Projection);
      distance /= sceneNode.ScaleWorld.LargestComponent;

      return distance;
    }


    /// <summary>
    /// Converts the specified distance to a view-normalized distance ("LOD distance").
    /// </summary>
    /// <param name="distance">The 3D Euclidean distance between the object and the camera.</param>
    /// <param name="projection">The projection transformation.</param>
    /// <returns>The view-normalized distance.</returns>
    /// <inheritdoc cref="GetViewNormalizedDistance(DigitalRune.Graphics.SceneGraph.SceneNode,DigitalRune.Graphics.SceneGraph.CameraNode)"/>
    public static float GetViewNormalizedDistance(float distance, Matrix44F projection)
    {
      Debug.Assert(distance >= 0, "The distance should be greater than or equal to 0.");

      float yScale = Math.Abs(projection.M11);
      return distance / yScale;
    }
  }
}
