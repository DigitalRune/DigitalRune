// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Defines a shape that represents an empty volume. This shape will not collide with any other 
  /// shape.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public sealed class EmptyShape : Shape
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
    /// Initializes a new instance of the <see cref="EmptyShape"/> class.
    /// </summary>
    [Obsolete("Do not create new EmptyShape instances. Use Shape.Empty instead.")]
    public EmptyShape()
    {
    }


    // Dummy constructor
    // Make default constructor private in next release and remove this constructor.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dummy")]
    internal EmptyShape(int dummy)
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
      return new EmptyShape(0);
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      return new Aabb(pose.Position, pose.Position);
    }


    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used</param>
    /// <returns>0</returns>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      return 0;
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    /// <remarks>
    /// An <see cref="EmptyShape"/> has no valid mesh. This method will return an empty
    /// triangle mesh.
    /// </remarks>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      return new TriangleMesh();
    }
    #endregion
  }
}
