// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a perspective view volume (frustum).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A perspective view volume is a frustum. A frustum is a portion of a pyramid that lies between 
  /// two cutting planes.
  /// </para>
  /// <para>
  /// The <see cref="PerspectiveViewVolume"/> class is designed to model the view volume of a 
  /// perspective camera: The observer is looking from the origin along the negative z-axis. The 
  /// x-axis points to the right and the y-axis points upwards. <see cref="ViewVolume.Near"/> and 
  /// <see cref="ViewVolume.Far"/> are positive values that specify the distance from the origin 
  /// (observer) to the near and far clip planes 
  /// (<see cref="ViewVolume.Near"/> ≤ <see cref="ViewVolume.Far"/>).
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class PerspectiveViewVolume : ViewVolume
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Cached vertices.
    private Vector3F _nearBottomLeftVertex;
    private Vector3F _nearBottomRightVertex;
    private Vector3F _nearTopLeftVertex;
    private Vector3F _nearTopRightVertex;
    private Vector3F _farBottomLeftVertex;
    private Vector3F _farBottomRightVertex;
    private Vector3F _farTopLeftVertex;
    private Vector3F _farTopRightVertex;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>An inner point.</value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get { return _innerPoint; }
    }
    private Vector3F _innerPoint;


    /// <summary>
    /// Gets the horizontal field of view.
    /// </summary>
    /// <value>The horizontal field of view.</value>
    public override float FieldOfViewX
    {
      get
      {
        // Sort left and right.
        float left, right;
        if (Left <= Right)
        {
          left = Left;
          right = Right;
        }
        else
        {
          left = Right;
          right = Left;
        }

        float distance = (Near <= Far) ? Near : Far;
        return (float)Math.Atan((-left) / distance) + (float)Math.Atan(right / distance);
      }
    }


    /// <summary>
    /// Gets the vertical field of view.
    /// </summary>
    /// <value>The vertical field of view.</value>
    public override float FieldOfViewY
    {
      get
      {
        // Sort bottom and top.
        float bottom, top;
        if (Bottom <= Top)
        {
          bottom = Bottom;
          top = Top;
        }
        else
        {
          bottom = Top;
          top = Bottom;
        }

        float distance = (Near <= Far) ? Near : Far;
        return (float)Math.Atan((-bottom) / distance) + (float)Math.Atan(top / distance);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="PerspectiveViewVolume"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="PerspectiveViewVolume"/> class using default 
    /// settings.
    /// </summary>
    public PerspectiveViewVolume()
    {
      SetWidthAndHeight(2, 2, 1, 4);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="PerspectiveViewVolume"/> class with the given
    /// field of view and depth.
    /// </summary>
    /// <param name="fieldOfViewY">The vertical field of view.</param>
    /// <param name="aspectRatio">The aspect ratio (width / height).</param>
    /// <param name="near">The distance to the near clip plane.</param>
    /// <param name="far">The distance to the far clip plane.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fieldOfViewY"/> is not between 0 and π radians (0° and 180°),
    /// <paramref name="aspectRatio"/> is negative or 0, <paramref name="near"/> is negative or 0,
    /// or <paramref name="far"/> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
    /// </exception>
    public PerspectiveViewVolume(float fieldOfViewY, float aspectRatio, float near, float far)
    {
      SetFieldOfView(fieldOfViewY, aspectRatio, near, far);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new PerspectiveViewVolume();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (PerspectiveViewVolume)sourceShape;
      Set(source.Left, source.Right, source.Bottom, source.Top, source.Near, source.Far);
    }
    #endregion


    /// <summary>
    /// Gets a support point for a given direction.
    /// </summary>
    /// <param name="direction">
    /// The direction for which to get the support point. The vector does not need to be normalized.
    /// The result is undefined if the vector is a zero vector.
    /// </param>
    /// <returns>
    /// A support point regarding the given direction.
    /// </returns>
    /// <remarks>
    /// <para>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </para>
    /// </remarks>
    public override Vector3F GetSupportPoint(Vector3F direction)
    {
      if (direction.X > 0)
      {
        // Get a right vertex.

        if (direction.Y > 0)
        {
          // Get a top right vertex.
          if (Vector3F.Dot(_nearTopRightVertex, direction) > Vector3F.Dot(_farTopRightVertex, direction))
            return _nearTopRightVertex;
          else
            return _farTopRightVertex;
        }
        else
        {
          // Get a bottom right vertex;
          if (Vector3F.Dot(_nearBottomRightVertex, direction) > Vector3F.Dot(_farBottomRightVertex, direction))
            return _nearBottomRightVertex;
          else
            return _farBottomRightVertex;
        }
      }
      else
      {
        // Get a left vertex.

        if (direction.Y > 0)
        {
          // Get a top left vertex.
          if (Vector3F.Dot(_nearTopLeftVertex, direction) > Vector3F.Dot(_farTopLeftVertex, direction))
            return _nearTopLeftVertex;
          else
            return _farTopLeftVertex;
        }
        else
        {
          // Get a bottom left vertex;
          if (Vector3F.Dot(_nearBottomLeftVertex, direction) > Vector3F.Dot(_farBottomLeftVertex, direction))
            return _nearBottomLeftVertex;
          else
            return _farBottomLeftVertex;
        }
      }
    }



    /// <summary>
    /// Gets a support point for a given normalized direction vector.
    /// </summary>
    /// <param name="directionNormalized">
    /// The normalized direction vector for which to get the support point.
    /// </param>
    /// <returns>A support point regarding the given direction.</returns>
    /// <remarks>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </remarks>
    public override Vector3F GetSupportPointNormalized(Vector3F directionNormalized)
    {
      if (directionNormalized.X > 0)
      {
        // Get a right vertex.

        if (directionNormalized.Y > 0)
        {
          // Get a top right vertex.
          if (Vector3F.Dot(_nearTopRightVertex, directionNormalized) > Vector3F.Dot(_farTopRightVertex, directionNormalized))
            return _nearTopRightVertex;
          else
            return _farTopRightVertex;
        }
        else
        {
          // Get a bottom right vertex;
          if (Vector3F.Dot(_nearBottomRightVertex, directionNormalized) > Vector3F.Dot(_farBottomRightVertex, directionNormalized))
            return _nearBottomRightVertex;
          else
            return _farBottomRightVertex;
        }
      }
      else
      {
        // Get a left vertex.

        if (directionNormalized.Y > 0)
        {
          // Get a top left vertex.
          if (Vector3F.Dot(_nearTopLeftVertex, directionNormalized) > Vector3F.Dot(_farTopLeftVertex, directionNormalized))
            return _nearTopLeftVertex;
          else
            return _farTopLeftVertex;
        }
        else
        {
          // Get a bottom left vertex;
          if (Vector3F.Dot(_nearBottomLeftVertex, directionNormalized) > Vector3F.Dot(_farBottomLeftVertex, directionNormalized))
            return _nearBottomLeftVertex;
          else
            return _farBottomLeftVertex;
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// <returns>The volume of this shape.</returns>
    public float GetVolume()
    {
      float nearArea = Width * Height;
      float scale = Far / Near;
      float farArea = nearArea * scale * scale;

      // Volume is total pyramid minus the pyramid before the near plane.
      return 1.0f / 3.0f * (farArea * Far - nearArea * Near);
    }


    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used.</param>
    /// <returns>The volume of this shape.</returns>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      return GetVolume();
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      // Get coordinates of corners:
      float near = -Math.Min(Near, Far);
      float far = -Math.Max(Near, Far);
      float leftNear = Math.Min(Left, Right);
      float rightNear = Math.Max(Left, Right);
      float topNear = Math.Max(Top, Bottom);
      float bottomNear = Math.Min(Top, Bottom);
      float farFactor = 1 / near * far;    // Multiply near-values by this factor to get far-values.
      float leftFar = leftNear * farFactor;
      float rightFar = rightNear * farFactor;
      float topFar = topNear * farFactor;
      float bottomFar = bottomNear * farFactor;

      TriangleMesh mesh = new TriangleMesh();

      // -y face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(leftNear, bottomNear, near), 
        Vertex1 = new Vector3F(leftFar, bottomFar, far),
        Vertex2 = new Vector3F(rightFar, bottomFar, far),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(rightFar, bottomFar, far),
        Vertex1 = new Vector3F(rightNear, bottomNear, near),
        Vertex2 = new Vector3F(leftNear, bottomNear, near),
      }, true);

      // +x face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(rightNear, topNear, near),
        Vertex1 = new Vector3F(rightNear, bottomNear, near),
        Vertex2 = new Vector3F(rightFar, bottomFar, far),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(rightFar, bottomFar, far),
        Vertex1 = new Vector3F(rightFar, topFar, far),
        Vertex2 = new Vector3F(rightNear, topNear, near),
      }, true);

      // -z face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(rightFar, topFar, far),
        Vertex1 = new Vector3F(rightFar, bottomFar, far),
        Vertex2 = new Vector3F(leftFar, bottomFar, far),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(leftFar, bottomFar, far),
        Vertex1 = new Vector3F(leftFar, topFar, far),
        Vertex2 = new Vector3F(rightFar, topFar, far),
      }, true);

      // -x face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(leftFar, topFar, far),
        Vertex1 = new Vector3F(leftFar, bottomFar, far),
        Vertex2 = new Vector3F(leftNear, bottomNear, near),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(leftNear, bottomNear, near),
        Vertex1 = new Vector3F(leftNear, topNear, near),
        Vertex2 = new Vector3F(leftFar, topFar, far),
      }, true);
      
      // +z face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(leftNear, topNear, near),
        Vertex1 = new Vector3F(leftNear, bottomNear, near),
        Vertex2 = new Vector3F(rightNear, bottomNear, near),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(rightNear, bottomNear, near),
        Vertex1 = new Vector3F(rightNear, topNear, near),
        Vertex2 = new Vector3F(leftNear, topNear, near),
      }, true);

      // +y face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(leftFar, topFar, far),
        Vertex1 = new Vector3F(leftNear, topNear, near),
        Vertex2 = new Vector3F(rightNear, topNear, near),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(rightNear, topNear, near),
        Vertex1 = new Vector3F(rightFar, topFar, far),
        Vertex2 = new Vector3F(leftFar, topFar, far),
      }, true);

      return mesh;
    }


    /// <summary>
    /// Updates the shape.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Breaking change. Change exception type in next version.")]
    protected override void Update()
    {
      // Sort left and right.
      float left = Left;
      float right = Right;
      if (left > right)
        MathHelper.Swap(ref left, ref right);

      // Sort bottom and top.
      float bottom = Bottom;
      float top = Top;
      if (bottom > top)
        MathHelper.Swap(ref bottom, ref top);

      // Sort near and far.
      float near = Near;
      float far = Far;
      if (near <= 0)
        throw new ArgumentOutOfRangeException("near", "The near plane distance of a perspective view volume needs to be greater than 0.");
      if (far <= 0)
        throw new ArgumentOutOfRangeException("far", "The far plane distance of a perspective view volume needs to be greater than 0.");
      if (near > far)
        MathHelper.Swap(ref near, ref far);

      // Update near view rectangle.
      _nearBottomLeftVertex = new Vector3F(left, bottom, -near);
      _nearBottomRightVertex = new Vector3F(right, bottom, -near);
      _nearTopLeftVertex = new Vector3F(left, top, -near);
      _nearTopRightVertex= new Vector3F(right, top, -near);

      // Update far view rectangle.
      float factor = far / near;
      left = left * factor;
      right = right * factor;
      bottom = bottom * factor;
      top = top * factor;

      _farBottomLeftVertex = new Vector3F(left, bottom, -far);
      _farBottomRightVertex = new Vector3F(right, bottom, -far);
      _farTopLeftVertex = new Vector3F(left, top, -far);
      _farTopRightVertex = new Vector3F(right, top, -far);

      _innerPoint = new Vector3F(left + right, bottom + top, -near - far) * 0.5f;
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(
        CultureInfo.InvariantCulture,
        "PerspectiveViewVolume {{ Left = {0}, Right = {1}, Bottom = {2}, Top = {3}, Near = {4}, Far = {5} }}",
        Left, Right, Bottom, Top, Near, Far);
    }


    /// <overloads>
    /// <summary>
    /// Sets the dimensions of the frustum to the specified field of view.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets the dimensions of the frustum to the specified field of view and near/far values.
    /// </summary>
    /// <param name="fieldOfViewY">The vertical field of view.</param>
    /// <param name="aspectRatio">The aspect ratio (width / height).</param>
    /// <param name="near">The distance to the near clip plane.</param>
    /// <param name="far">The distance to the far clip plane.</param>
    /// <remarks>
    /// This method creates a symmetric frustum.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fieldOfViewY"/> is not between 0 and π radians (0° and 180°),
    /// <paramref name="aspectRatio"/> is negative or 0, <paramref name="near"/> is negative or 0,
    /// or <paramref name="far"/> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
    /// </exception>
    public void SetFieldOfView(float fieldOfViewY, float aspectRatio, float near, float far)
    {
      if (near <= 0)
        throw new ArgumentOutOfRangeException("near", "The near plane distance of a frustum needs to be greater than 0.");
      if (far <= 0)
        throw new ArgumentOutOfRangeException("far", "The far plane distance of a frustum needs to be greater than 0.");
      if (near >= far)
        throw new ArgumentException("The near plane distance of a frustum needs to be less than the far plane distance (near < far).");

      float width, height;
      GetWidthAndHeight(fieldOfViewY, aspectRatio, near, out width, out height);
      SetWidthAndHeight(width, height, near, far);
    }


    /// <summary>
    /// Sets the dimensions of the frustum to the specified field of view.
    /// </summary>
    /// <param name="fieldOfViewY">The vertical field of view.</param>
    /// <param name="aspectRatio">The aspect ratio (width / height).</param>
    /// <remarks>
    /// This method creates a symmetric frustum.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fieldOfViewY"/> is not between 0 and π radians (0° and 180°), or
    /// <paramref name="aspectRatio"/> is negative or 0.
    /// </exception>
    public void SetFieldOfView(float fieldOfViewY, float aspectRatio)
    {
      float width, height;
      GetWidthAndHeight(fieldOfViewY, aspectRatio, Near, out width, out height);
      SetWidthAndHeight(width, height);
    }


    /// <summary>
    /// Converts the vertical field of view of a symmetric frustum to a horizontal field of view.
    /// </summary>
    /// <param name="fieldOfViewY">The vertical field of view in radians.</param>
    /// <param name="aspectRatio">The aspect ratio (width / height).</param>
    /// <returns>The horizontal field of view in radians.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fieldOfViewY"/> is not between 0 and π radians (0° and 180°), or 
    /// <paramref name="aspectRatio"/> is negative or 0.
    /// </exception>
    public static float GetFieldOfViewX(float fieldOfViewY, float aspectRatio)
    {
      if (fieldOfViewY <= 0f || fieldOfViewY >= ConstantsF.Pi)
        throw new ArgumentOutOfRangeException("fieldOfViewY", "The field of view must be between 0 radians and π radians.");
      if (aspectRatio <= 0)
        throw new ArgumentOutOfRangeException("aspectRatio", "The aspect ratio must not be negative or 0.");

      float height = 2.0f * (float)Math.Tan(fieldOfViewY / 2.0f);
      float width = height * aspectRatio;
      float horizontalFieldOfView = 2.0f * (float)Math.Atan(width / 2.0f);
      return horizontalFieldOfView;
    }


    /// <summary>
    /// Converts a horizontal field of view of a symmetric frustum to a vertical field of view.
    /// </summary>
    /// <param name="fieldOfViewX">The horizontal field of view in radians.</param>
    /// <param name="aspectRatio">The aspect ratio.</param>
    /// <returns>The vertical field of view in radians.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fieldOfViewX"/> is not between 0 and π radians (0° and 180°), or
    /// <paramref name="aspectRatio"/> is negative or 0.
    /// </exception>
    public static float GetFieldOfViewY(float fieldOfViewX, float aspectRatio)
    {
      if (fieldOfViewX <= 0f || fieldOfViewX >= ConstantsF.Pi)
        throw new ArgumentOutOfRangeException("fieldOfViewX", "The field of view must be between 0 radians and π radians.");
      if (aspectRatio <= 0)
        throw new ArgumentOutOfRangeException("aspectRatio", "The aspect ratio must not be negative or 0.");

      float width = 2.0f * (float)Math.Tan(fieldOfViewX / 2.0f);
      float height = width / aspectRatio;
      float verticalFieldOfView = 2.0f * (float)Math.Atan(height / 2.0f);
      return verticalFieldOfView;
    }


    /// <summary>
    /// Gets the extent of the frustum at the given distance.
    /// </summary>
    /// <param name="fieldOfView">The field of view in radians.</param>
    /// <param name="distance">The distance at which the extent is calculated.</param>
    /// <returns>The extent of the view volume at the given distance.</returns>
    /// <remarks>
    /// <para>
    /// To calculate the width of the frustum the horizontal field of view must be specified.
    /// To calculate the height of the frustum the vertical field of view needs to be specified.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fieldOfView"/> is not between 0 and π radians (0° and 180°), or
    /// <paramref name="distance"/> is negative.
    /// </exception>
    public static float GetExtent(float fieldOfView, float distance)
    {
      if (fieldOfView <= 0f || fieldOfView >= ConstantsF.Pi)
        throw new ArgumentOutOfRangeException("fieldOfView", "The field of view must be between 0 radians and π radians.");
      if (distance < 0)
        throw new ArgumentOutOfRangeException("distance", "The distance must not be negative.");

      return 2.0f * distance * (float)Math.Tan(fieldOfView / 2.0f);
    }


    /// <summary>
    /// Converts a field of view of a symmetric frustum to width and height.
    /// </summary>
    /// <param name="fieldOfViewY">The vertical field of view in radians.</param>
    /// <param name="aspectRatio">The aspect ratio (width / height).</param>
    /// <param name="distance">
    /// The distance at which <paramref name="width"/> and <paramref name="height"/> are calculated.
    /// </param>
    /// <param name="width">
    /// The width of the view volume at the specified <paramref name="distance"/>.
    /// </param>
    /// <param name="height">
    /// The height of the view volume at the specified <paramref name="distance"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fieldOfViewY"/> is not between 0 and π radians (0° and 180°), 
    /// <paramref name="aspectRatio"/> is negative or 0, or <paramref name="distance"/> is negative.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void GetWidthAndHeight(float fieldOfViewY, float aspectRatio, float distance, out float width, out float height)
    {
      if (fieldOfViewY <= 0f || fieldOfViewY >= ConstantsF.Pi)
        throw new ArgumentOutOfRangeException("fieldOfViewY", "The field of view must be between 0 radians and π radians.");
      if (aspectRatio <= 0)
        throw new ArgumentOutOfRangeException("aspectRatio", "The aspect ratio must not be negative or 0.");
      if (distance < 0)
        throw new ArgumentOutOfRangeException("distance", "The distance must not be negative.");

      height = 2.0f * distance * (float)Math.Tan(fieldOfViewY / 2.0f);
      width = height * aspectRatio;
    }


    /// <summary>
    /// Gets the field of view from a frustum with the given extent.
    /// </summary>
    /// <param name="extent">
    /// The extent of the frustum at the specified <paramref name="distance"/>.
    /// </param>
    /// <param name="distance">The distance.</param>
    /// <returns>The field of view for the given extent.</returns>
    /// <remarks>
    /// To get the horizontal field of view the horizontal extent (x direction) needs to be 
    /// specified. To get the vertical field of view the vertical extent (y direction) needs to be
    /// specified.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="extent"/> is negative, or <paramref name="distance"/> is negative or 0.
    /// </exception>
    public static float GetFieldOfView(float extent, float distance)
    {
      if (extent < 0)
        throw new ArgumentOutOfRangeException("extent", "The extent of the frustum must be greater than or equal to 0.");
      if (distance <= 0)
        throw new ArgumentOutOfRangeException("distance", "The distance must be greater than 0.");

      return 2.0f * (float)Math.Atan(extent / (2.0f * distance));
    }
    #endregion
  }
}
