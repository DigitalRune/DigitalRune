// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a perspective projection.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The perspective projection can be set in several ways:
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// By setting the properties <see cref="Projection.Left"/>, <see cref="Projection.Right"/>,
  /// <see cref="Projection.Bottom"/>, <see cref="Projection.Top"/>, <see cref="Projection.Near"/>,
  /// and <see cref="Projection.Far"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// By setting assigning a projection matrix using <see cref="Set(Matrix44F)"/> or the property
  /// <see cref="Projection.Inverse"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// By calling on of the following methods:
  /// <see cref="Set(float,float)"/>,
  /// <see cref="Set(float,float,float,float)"/>,
  /// <see cref="Set(DigitalRune.Mathematics.Algebra.Matrix44F)"/>,
  /// <see cref="SetFieldOfView(float,float)"/>,
  /// <see cref="SetFieldOfView(float,float,float,float)"/>,
  /// <see cref="SetOffCenter(float,float,float,float)"/>,
  /// <see cref="SetOffCenter(float,float,float,float,float,float)"/>,
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// The property <see cref="ViewVolume"/> defines the bounding shape of the projection which can
  /// be used for frustum culling. The shape is updated automatically when the properties of the
  /// projection change.
  /// </para>
  /// <para>
  /// This class supports near plane clipping for portal and reflection rendering, see
  /// <see cref="NearClipPlane"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="Projection"/>
  public class PerspectiveProjection : Projection
  {
    // References for near plane clipping and oblique frustums:
    // - E. Lengyel: "Oblique View Frustums for Mirrors and Portals". In Game Programming Gems 5.
    // - E. Lengyel: "Modifying the Projection Matrix to Perform Oblique Near-Plane Clipping
    //   (Lengyel’s Frustum)", http://www.terathon.com/code/oblique.html.
    // - E. Lengyel: "Oblique View Frustum Depth Projection and Clipping",
    //   http://www.terathon.com/lengyel/Lengyel-Oblique.pdf.


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// The default value for <see cref="Projection.Near"/>.
    /// </summary>
    private const float DefaultNear = 1.0f;


    /// <summary>
    /// The default value for <see cref="Projection.Far"/>.
    /// </summary>
    private const float DefaultFar = 1000.0f;


    /// <summary>
    /// The default value for <see cref="Projection.AspectRatio"/>.
    /// </summary>
    private const float DefaultAspectRatio = 16.0f / 9.0f;


    /// <summary>
    /// The default value for <see cref="Projection.FieldOfViewY"/>.
    /// </summary>
    private const float DefaultFieldOfViewY = 60.0f * ConstantsF.Pi / 180; // 60°
    #endregion

    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the near clip plane in view space.
    /// </summary>
    /// <value>
    /// The near clip plane in view space. The plane normal must point to the viewer.
    /// </value>
    /// <remarks>
    /// <para>
    /// When rendering mirrors or portals, the objects before the mirror or portal should not be
    /// rendered. This could be solved using clip planes, but these clip planes need to be supported
    /// by all shaders. Alternatively, we can also solve this problem by creating a view frustum
    /// where the near plane is parallel to the clip plane - such frustums are called oblique view
    /// frustums because the near plane (and also the far plane) are tilted compared to standard
    /// view frustums.
    /// </para>
    /// <para>
    /// Use the property <see cref="NearClipPlane"/> to set a clip plane for the near view-plane.
    /// Setting a near clip plane changes the projection matrix. However, it does not affect the
    /// shape (see <see cref="Projection.ViewVolume"/>) of the <see cref="Projection"/>!
    /// </para>
    /// <para>
    /// For general information about oblique view frustums, see
    /// <see href="http://www.terathon.com/code/oblique.html" />.
    /// </para>
    /// </remarks>
    public Plane? NearClipPlane
    {
      get { return _nearClipPlane; }
      set
      {
        if (_nearClipPlane == value)
          return;

        _nearClipPlane = value;
        Invalidate();
      }
    }
    private Plane? _nearClipPlane;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PerspectiveProjection"/> class.
    /// </summary>
    public PerspectiveProjection()
    {
      ViewVolume = new PerspectiveViewVolume();
      SetFieldOfView(DefaultFieldOfViewY, DefaultAspectRatio, DefaultNear, DefaultFar);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Projection CreateInstanceCore()
    {
      return new PerspectiveProjection();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Projection source)
    {
      var sourceTyped = (PerspectiveProjection)source;
      NearClipPlane = sourceTyped._nearClipPlane;
      SetOffCenter(sourceTyped.Left, sourceTyped.Right, 
                   sourceTyped.Bottom, sourceTyped.Top, 
                   sourceTyped.Near, sourceTyped.Far);
    }
    #endregion


    /// <overloads>
    /// <summary>
    /// Sets a symmetric, perspective projection based on width and height.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets a symmetric, perspective projection based on size and depth.
    /// </summary>
    /// <param name="width">The width of the frustum at the near clip plane.</param>
    /// <param name="height">The height of the frustum at the near clip plane.</param>
    /// <param name="near">The distance to the near clip plane.</param>
    /// <param name="far">The distance to the far clip plane.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
    /// </exception>
    public void Set(float width, float height, float near, float far)
    {
      ViewVolume.SetWidthAndHeight(width, height, near, far);
      Invalidate();
    }


    /// <summary>
    /// Sets a symmetric, perspective projection based on size.
    /// </summary>
    /// <param name="width">The width of the frustum at the near clip plane.</param>
    /// <param name="height">The height of the frustum at the near clip plane.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is negative or 0.
    /// </exception>
    public void Set(float width, float height)
    {
      ViewVolume.SetWidthAndHeight(width, height);
      Invalidate();
    }


    /// <overloads>
    /// <summary>
    /// Sets a symmetric, perspective projection based on field of view.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets a symmetric, perspective projection based on field of view and depth.
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
      ((PerspectiveViewVolume)ViewVolume).SetFieldOfView(fieldOfViewY, aspectRatio, near, far);
      Invalidate();
    }


    /// <summary>
    /// Sets a symmetric, perspective projection based on field of view.
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
      ((PerspectiveViewVolume)ViewVolume).SetFieldOfView(fieldOfViewY, aspectRatio);
      Invalidate();
    }


    /// <overloads>
    /// <summary>
    /// Sets an asymmetric (off-center), perspective projection.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets an asymmetric (off-center), perspective projection based on the given values (including
    /// depth).
    /// </summary>
    /// <param name="left">The minimum x-value of the frustum at the near clip plane.</param>
    /// <param name="right">The maximum x-value of the frustum at the near clip plane.</param>
    /// <param name="bottom">The minimum y-value of the frustum at the near clip plane.</param>
    /// <param name="top">The maximum y-value of the frustum at the near clip plane.</param>
    /// <param name="near">The distance to the near clip plane.</param>
    /// <param name="far">The distance to the far clip plane.</param>
    /// <remarks>
    /// This method can be used to define an asymmetric, off-center frustum.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="left"/> is greater than or equal to <paramref name="right"/>, 
    /// <paramref name="bottom"/> is greater than or equal to <paramref name="top"/>, or
    /// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public void SetOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
      ViewVolume.Set(left, right, bottom, top, near, far);
      Invalidate();
    }


    /// <summary>
    /// Sets an asymmetric (off-center), perspective projection based on the given values.
    /// </summary>
    /// <param name="left">The minimum x-value of the frustum at the near clip plane.</param>
    /// <param name="right">The maximum x-value of the frustum at the near clip plane.</param>
    /// <param name="bottom">The minimum y-value of the frustum at the near clip plane.</param>
    /// <param name="top">The maximum y-value of the frustum at the near clip plane.</param>
    /// <remarks>
    /// This method can be used to define an asymmetric, off-center frustum.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="left"/> is greater than or equal to <paramref name="right"/>, or
    /// <paramref name="bottom"/> is greater than or equal to <paramref name="top"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public void SetOffCenter(float left, float right, float bottom, float top)
    {
      ViewVolume.Set(left, right, bottom, top);
      Invalidate();
    }


    /// <summary>
    /// Sets the perspective projection from the given projection matrix.
    /// </summary>
    /// <param name="projection">The perspective projection.</param>
    public override void Set(Matrix44F projection)
    {
      const string message = "Given matrix is not a valid perspective projection matrix.";
      Debug.Assert(Numeric.IsZero(projection.M01), message);
      Debug.Assert(Numeric.IsZero(projection.M03), message);
      Debug.Assert(Numeric.IsZero(projection.M10), message);
      Debug.Assert(Numeric.IsZero(projection.M13), message);
      Debug.Assert(Numeric.IsZero(projection.M20), message);
      Debug.Assert(Numeric.IsZero(projection.M21), message);
      Debug.Assert(Numeric.IsZero(projection.M30), message);
      Debug.Assert(Numeric.IsZero(projection.M31), message);
      Debug.Assert(Numeric.IsZero(projection.M33), message);
      Debug.Assert(Numeric.AreEqual(projection.M32, -1), message);

      float near = projection.M23 / projection.M22;
      float far = near * projection.M22 / (1 + projection.M22);

      Debug.Assert(near > 0, message);
      Debug.Assert(far > 0, message);
      Debug.Assert(near < far, message);

      float rightMinusLeft = 2.0f * near / projection.M00;
      float leftPlusRight = projection.M02 * rightMinusLeft;
      float right = (leftPlusRight + rightMinusLeft) / 2.0f;
      float left = leftPlusRight - right;

      Debug.Assert(left < right, message);

      float topMinusBottom = 2.0f * near / projection.M11;
      float bottomPlusTop = projection.M12 * topMinusBottom;
      float top = (bottomPlusTop + topMinusBottom) / 2.0f;
      float bottom = bottomPlusTop - top;

      Debug.Assert(bottom < top, message);

      SetOffCenter(left, right, bottom, top, near, far);
      Invalidate();
    }


    /// <inheritdoc/>
    protected override Matrix44F ComputeProjection()
    {
      var projection = Matrix44F.CreatePerspectiveOffCenter(Left, Right, Bottom, Top, Near, Far);

      if (_nearClipPlane.HasValue)
      {
        Vector4F clipPlane = new Vector4F(_nearClipPlane.Value.Normal, -_nearClipPlane.Value.DistanceFromOrigin);

        // Calculate the clip-space corner point opposite the clipping plane as
        // (-sign(clipPlane.x), -sign(clipPlane.y), 1, 1) and transform it into
        // camera space by multiplying it by the inverse of the projection matrix.
        Vector4F q;
        q.X = (-Math.Sign(clipPlane.X) + projection.M02) / projection.M00;
        q.Y = (-Math.Sign(clipPlane.Y) + projection.M12) / projection.M11;
        q.Z = -1.0f;
        q.W = (1.0f + projection.M22) / projection.M23;

        // Calculate the scaled plane vector
        Vector4F c = clipPlane * (1.0f / Vector4F.Dot(clipPlane, q));

        // Replace the third row of the projection matrix
        projection.M20 = c.X;
        projection.M21 = c.Y;
        projection.M22 = c.Z;
        projection.M23 = c.W;
      }

      return projection;
    }


    /// <summary>
    /// Converts a 4x4 projection matrix to a perspective projection.
    /// </summary>
    /// <param name="matrix">The projection.</param>
    /// <returns>The perspective projection.</returns>
    public static explicit operator PerspectiveProjection(Matrix44F matrix)
    {
      var projection = new PerspectiveProjection();
      projection.Set(matrix);
      return projection;
    }


    /// <summary>
    /// Creates an perspective projection from a 4x4 transformation matrix.
    /// </summary>
    /// <param name="matrix">The projection matrix.</param>
    /// <returns>The perspective projection.</returns>
    public static PerspectiveProjection FromMatrix(Matrix44F matrix)
    {
      var projection = new PerspectiveProjection();
      projection.Set(matrix);
      return projection;
    }
    #endregion
  }
}
