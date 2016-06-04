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
  /// Defines an orthographic projection.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The projection can be set in several ways:
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
  /// <see cref="Set(Matrix44F)"/>, 
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
  /// </remarks>
  /// <seealso cref="Projection"/>
  public class OrthographicProjection : Projection
  {
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
    /// The default value for <see cref="Projection.Width"/>.
    /// </summary>
    private const float DefaultWidth = 16.0f;


    /// <summary>
    /// The default value for <see cref="Projection.Height"/>.
    /// </summary>
    private const float DefaultHeight = 9.0f;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="OrthographicProjection"/> class.
    /// </summary>
    public OrthographicProjection()
    {
      ViewVolume = new OrthographicViewVolume();
      Set(DefaultWidth, DefaultHeight, DefaultNear, DefaultFar);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Projection CreateInstanceCore()
    {
      return new OrthographicProjection();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Projection source)
    {
      var sourceTyped = (OrthographicProjection)source;
      SetOffCenter(sourceTyped.Left, sourceTyped.Right, 
                   sourceTyped.Bottom, sourceTyped.Top, 
                   sourceTyped.Near, sourceTyped.Far);
    }
    #endregion
    

    /// <overloads>
    /// <summary>
    /// Sets a right-handed, orthographic projection.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets a right-handed, orthographic projection with the specified size and depth.
    /// </summary>
    /// <param name="width">The width of the view volume.</param>
    /// <param name="height">The height of the view volume.</param>
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
    /// Sets a right-handed, orthographic projection with the specified size.
    /// </summary>
    /// <param name="width">The width of the view volume.</param>
    /// <param name="height">The height of the view volume.</param>
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
    /// Sets a customized (off-center), right-handed, orthographic projection.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets a customized (off-center), right-handed, orthographic projection (including depth).
    /// </summary>
    /// <param name="left">The minimum x-value of the view volume.</param>
    /// <param name="right">The maximum x-value of the view volume.</param>
    /// <param name="bottom">The minimum y-value of the view volume.</param>
    /// <param name="top">The maximum y-value of the view volume.</param>
    /// <param name="near">The distance to the near clip plane.</param>
    /// <param name="far">The distance to the far clip plane.</param>
    /// <remarks>
    /// This method can be used to define an off-center view volume.
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
    /// Sets a customized (off-center), right-handed, orthographic projection.
    /// </summary>
    /// <param name="left">The minimum x-value of the view volume.</param>
    /// <param name="right">The maximum x-value of the view volume.</param>
    /// <param name="bottom">The minimum y-value of the view volume.</param>
    /// <param name="top">The maximum y-value of the view volume.</param>
    /// <remarks>
    /// This method can be used to define an off-center view volume.
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
    /// Sets the orthographic projection from the given projection matrix.
    /// </summary>
    /// <param name="projection">The orthographic projection matrix.</param>
    public override void Set(Matrix44F projection)
    {
      string message = "Given matrix is not a valid orthographic projection matrix.";
      Debug.Assert(Numeric.IsZero(projection.M01), message);
      Debug.Assert(Numeric.IsZero(projection.M02), message);
      Debug.Assert(Numeric.IsZero(projection.M10), message);
      Debug.Assert(Numeric.IsZero(projection.M12), message);
      Debug.Assert(Numeric.IsZero(projection.M20), message);
      Debug.Assert(Numeric.IsZero(projection.M21), message);
      Debug.Assert(Numeric.IsZero(projection.M30), message);
      Debug.Assert(Numeric.IsZero(projection.M31), message);
      Debug.Assert(Numeric.IsZero(projection.M32), message);
      Debug.Assert(Numeric.AreEqual(projection.M33, 1), message);

      float rightMinusLeft = 2.0f / projection.M00;
      float leftPlusRight = -projection.M03 * rightMinusLeft;
      float right = (leftPlusRight + rightMinusLeft) / 2.0f;
      float left = leftPlusRight - right;

      Debug.Assert(left < right, message);

      float topMinusBottom = 2.0f / projection.M11;
      float bottomPlusTop = -projection.M13 * topMinusBottom;
      float top = (bottomPlusTop + topMinusBottom) / 2.0f;
      float bottom = bottomPlusTop - top;

      Debug.Assert(bottom < top, message);

      float nearMinusFar = 1.0f / projection.M22;
      float near = projection.M23 * nearMinusFar;
      float far = near - nearMinusFar;

      Debug.Assert(near < far, message);

      SetOffCenter(left, right, bottom, top, near, far);
      Invalidate();
    }


    /// <inheritdoc/>
    protected override Matrix44F ComputeProjection()
    {
      return Matrix44F.CreateOrthographicOffCenter(Left, Right, Bottom, Top, Near, Far);
    }


    /// <summary>
    /// Converts a 4x4 projection matrix to an orthographic projection.
    /// </summary>
    /// <param name="matrix">The projection matrix.</param>
    /// <returns>The orthographic projection.</returns>
    public static explicit operator OrthographicProjection(Matrix44F matrix)
    {
      var projection = new OrthographicProjection();
      projection.Set(matrix);
      return projection;
    }


    /// <summary>
    /// Creates an orthographic projection from a 4x4 transformation matrix.
    /// </summary>
    /// <param name="matrix">The projection matrix.</param>
    /// <returns>The orthographic projection.</returns>
    public static OrthographicProjection FromMatrix(Matrix44F matrix)
    {
      var projection = new OrthographicProjection();
      projection.Set(matrix);
      return projection;
    }
    #endregion
  }
}
