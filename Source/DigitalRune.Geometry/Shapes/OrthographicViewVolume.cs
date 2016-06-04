// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents an orthographic view volume.
  /// </summary>
  /// <remarks>
  /// The <see cref="OrthographicViewVolume"/> class is designed to model the view volume of a 
  /// orthographic camera: The observer is looking from the origin along the negative z-axis. The 
  /// x-axis points to the right and the y-axis points upwards. <see cref="ViewVolume.Near"/> and 
  /// <see cref="ViewVolume.Far"/> specify the distance from the origin (observer) to the near and 
  /// far clip planes (<see cref="ViewVolume.Near"/> &lt; <see cref="ViewVolume.Far"/>).
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class OrthographicViewVolume : ViewVolume
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Vector3F _boxCenter;
    private readonly BoxShape _box;
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
      get { return _boxCenter + _box.InnerPoint; }
    }


    /// <summary>
    /// Gets the horizontal field of view (always <see cref="Single.NaN"/>).
    /// </summary>
    /// <value>The horizontal field of view (always <see cref="Single.NaN"/>).</value>
    public override float FieldOfViewX
    {
      get { return Single.NaN; }
    }


    /// <summary>
    /// Gets the vertical field of view (always <see cref="Single.NaN"/>).
    /// </summary>
    /// <value>The vertical field of view (always <see cref="Single.NaN"/>).</value>
    public override float FieldOfViewY
    {
      get { return Single.NaN; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="OrthographicViewVolume"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="OrthographicViewVolume"/> class using default 
    /// settings.
    /// </summary>
    public OrthographicViewVolume() : this(2, 2, 1, 4)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="OrthographicViewVolume"/> class as a symmetric
    /// view volume.
    /// </summary>
    /// <param name="width">The width of the view volume at the near clip plane.</param>
    /// <param name="height">The height of the view volume at the near clip plane.</param>
    /// <param name="near">The distance to the near clip plane.</param>
    /// <param name="far">The distance to the far clip plane.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="height"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="near"/> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
    /// </exception>
    public OrthographicViewVolume(float width, float height, float near, float far)
    {
      _boxCenter = Vector3F.Zero;
      _box = new BoxShape();

      SetWidthAndHeight(width, height, near, far);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="OrthographicViewVolume"/> class as an 
    /// asymmetric, off-center view volume.
    /// </summary>
    /// <param name="left">The minimum x-value of the view volume at the near clip plane.</param>
    /// <param name="right">The maximum x-value of the view volume at the near clip plane.</param>
    /// <param name="bottom">The minimum y-value of the view volume at the near clip plane.</param>
    /// <param name="top">The maximum y-value of the view volume at the near clip plane.</param>
    /// <param name="near">The distance to the near clip plane.</param>
    /// <param name="far">The distance to the far clip plane.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="left"/> is greater than or equal to <paramref name="right"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="bottom"/> is greater than or equal to <paramref name="top"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="near"/> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
    /// </exception>
    public OrthographicViewVolume(float left, float right, float bottom, float top, float near, float far)
    {
      _boxCenter = Vector3F.Zero;
      _box = new BoxShape();

      Set(left, right, bottom, top, near, far);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new OrthographicViewVolume();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (OrthographicViewVolume)sourceShape;
      Set(source.Left, source.Right, source.Bottom, source.Top, source.Near, source.Far);
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      return _box.GetAabb(scale, pose * new Pose(_boxCenter * scale));
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
        "OrthographicViewVolume {{ Left = {0}, Right = {1}, Bottom = {2}, Top = {3}, Near = {4}, Far = {5} }}",
        Left, Right, Bottom, Top, Near, Far);
    }


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
      Vector3F localDirection = direction;
      Vector3F localVertex = _box.GetSupportPoint(localDirection);
      return _boxCenter + localVertex;
    }


    /// <summary>
    /// Gets a support point for a given normalized direction vector.
    /// </summary>
    /// <param name="directionNormalized">
    /// The normalized direction vector for which to get the support point.
    /// </param>
    /// <returns>
    /// A support point regarding the given direction.
    /// </returns>
    /// <remarks>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </remarks>
    public override Vector3F GetSupportPointNormalized(Vector3F directionNormalized)
    {
      Vector3F localDirection = directionNormalized;
      Vector3F localVertex = _box.GetSupportPointNormalized(localDirection);
      return _boxCenter + localVertex;
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
      return Width * Height * Depth;
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
      float left = Math.Min(Left, Right);
      float right = Math.Max(Left, Right);
      float top = Math.Max(Top, Bottom);
      float bottom = Math.Min(Top, Bottom);

      TriangleMesh mesh = new TriangleMesh();
      // -y face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(left, bottom, near),
        Vertex1 = new Vector3F(left, bottom, far),
        Vertex2 = new Vector3F(right, bottom, far),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(right, bottom, far),
        Vertex1 = new Vector3F(right, bottom, near),
        Vertex2 = new Vector3F(left, bottom, near),
      }, true);
      
      // +x face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(right, top, near),
        Vertex1 = new Vector3F(right, bottom, near),
        Vertex2 = new Vector3F(right, bottom, far),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(right, bottom, far),
        Vertex1 = new Vector3F(right, top, far),
        Vertex2 = new Vector3F(right, top, near),
      }, true);
        
      // -z face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(right, top, far),
        Vertex1 = new Vector3F(right, bottom, far),
        Vertex2 = new Vector3F(left, bottom, far),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(left, bottom, far),
        Vertex1 = new Vector3F(left, top, far),
        Vertex2 = new Vector3F(right, top, far),
      }, true);

      // -x face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(left, top, far),
        Vertex1 = new Vector3F(left, bottom, far),
        Vertex2 = new Vector3F(left, bottom, near),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(left, bottom, near),
        Vertex1 = new Vector3F(left, top, near),
        Vertex2 = new Vector3F(left, top, far),
      }, true);

      // +z face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(left, top, near),
        Vertex1 = new Vector3F(left, bottom, near),
        Vertex2 = new Vector3F(right, bottom, near),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(right, bottom, near),
        Vertex1 = new Vector3F(right, top, near),
        Vertex2 = new Vector3F(left, top, near),
      }, true);

      // +y face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(left, top, far),
        Vertex1 = new Vector3F(left, top, near),
        Vertex2 = new Vector3F(right, top, near),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(right, top, near),
        Vertex1 = new Vector3F(right, top, far),
        Vertex2 = new Vector3F(left, top, far),
      }, true);

      return mesh;
    }


    /// <summary>
    /// Updates the shape.
    /// </summary>
    protected override void Update()
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

      // Sort near and far.
      float near, far;
      if (Near <= Far)
      {
        near = Near;
        far = Far;
      }
      else
      {
        near = Far;
        far = Near;
      }

      // Update shape.
      float width = right - left;
      float height = top - bottom;
      float depth = far - near;

      _box.WidthX = width;
      _box.WidthY = height;
      _box.WidthZ = depth;
      float centerX = left + width / 2.0f;
      float centerY = bottom + height / 2.0f;
      float centerZ = -(near + depth / 2.0f);
      _boxCenter = new Vector3F(centerX, centerY, centerZ);
    }
    #endregion
  }
}
