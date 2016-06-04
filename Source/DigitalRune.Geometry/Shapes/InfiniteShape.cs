// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Defines a shape that represents an infinitely large volume. This shape will collide with 
  /// every other shape (except an <see cref="DigitalRune.Geometry.Shapes.EmptyShape"/>).
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public sealed class InfiniteShape : Shape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>An inner point. Always (0, 0, 0).</value>
    public override Vector3F InnerPoint
    {
      get { return Vector3F.Zero; }
    }


    /// <inheritdoc/>
    public override event EventHandler<ShapeChangedEventArgs> Changed
    {
      add { }
      remove { }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="InfiniteShape"/> class.
    /// </summary>
    [Obsolete("Do not create new InfiniteShape instances. Use Shape.Infinite instead.")]
    public InfiniteShape()
    {
    }


    // Dummy constructor
    // Make default constructor private in next release and remove this constructor.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dummy")]
    internal InfiniteShape(int dummy)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new InfiniteShape(0);
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      return new Aabb(new Vector3F(float.NegativeInfinity), new Vector3F(float.PositiveInfinity));
    }


    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used</param>
    /// <returns>Positive infinity (<see cref="float.PositiveInfinity"/>)</returns>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      return float.PositiveInfinity;
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    /// <remarks>
    /// An <see cref="InfiniteShape"/> has no valid mesh. This method will return an empty triangle
    /// mesh.
    /// </remarks>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      return new TriangleMesh();
    }
    #endregion
  }
}
