// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a transformed shape.
  /// </summary>
  /// <remarks> 
  /// <para>
  /// This shape can be used to add a local transformation (scaling, rotation and translation) to a 
  /// <see cref="Shape"/>. The actual shape and the transformation is stored in 
  /// <see cref="Child"/>.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class TransformedShape : Shape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the child <see cref="IGeometricObject"/>.
    /// </summary>
    /// <value>
    /// The child <see cref="IGeometricObject"/>. Must not be <see langword="null"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public IGeometricObject Child
    {
      get { return _child; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (_child != value)
        {
          // Unregister event handlers from old GeometricObject.
          _child.PoseChanged -= OnChildPoseChanged;
          _child.ShapeChanged -= OnChildShapeChanged;

          // Set new GeometricObject.
          _child = value;

          // Register event handlers for new GeometricObject.
          _child.PoseChanged += OnChildPoseChanged;
          _child.ShapeChanged += OnChildShapeChanged;

          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private IGeometricObject _child;


    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>An inner point.</value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get
      {
        Debug.Assert(_child != null, "GeometricObject must not be null.");

        // Return the inner point of the child.
        return _child.Pose.ToWorldPosition(_child.Shape.InnerPoint * _child.Scale);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TransformedShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TransformedShape"/> class.
    /// </summary>
    /// <remarks>
    /// <see cref="Child"/> is initialized with a <see cref="Child"/>
    /// with an <see cref="EmptyShape"/>.
    /// </remarks>
    public TransformedShape()
    {
      _child = new GeometricObject(Empty);
      _child.PoseChanged += OnChildPoseChanged;
      _child.ShapeChanged += OnChildShapeChanged;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TransformedShape"/> class from the given 
    /// geometric object.
    /// </summary>
    /// <param name="child">The geometric object (pose + shape).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="child"/> is <see langword="null"/>.
    /// </exception>
    public TransformedShape(IGeometricObject child)
    {
      if (child == null)
        throw new ArgumentNullException("child");

      _child = child;
      _child.PoseChanged += OnChildPoseChanged;
      _child.ShapeChanged += OnChildShapeChanged;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      var clone = Child.Clone();
      return new TransformedShape(clone);
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      // Note: 
      // Uniform scaling is no problem. The scale can be applied anytime in the process.
      // With uniform scaling we compute the child AABB directly for world space.
      // Non-uniform scaling cannot be used with rotated objects. With non-uniform
      // scaling we compute the AABB of the child in the parent space. Then we
      // apply the scale in parent space. Then we compute a world space AABB that contains
      // the parent space AABB.

      if (scale.X == scale.Y && scale.Y == scale.Z)
      {
        // Uniform scaling:
        // Transform the shape to world space and return its AABB.
        var childPose = new Pose(_child.Pose.Position * scale.X, _child.Pose.Orientation);
        return _child.Shape.GetAabb(scale.X * _child.Scale, pose * childPose);
      }
      else
      {
        // Non-uniform scaling:
        // Get AABB of child, transform the box to world space and return its AABB.
        return _child.Aabb.GetAabb(scale, pose);

        // Possible improvement: We can check if child.Pose.Orientation contains no orientation.
        // Then we compute a tighter AABB like in the uniform case.
      }
    }


    /// <inheritdoc/>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      Vector3F scale = Vector3F.Absolute(Child.Scale);
      return Child.Shape.GetVolume(relativeError, iterationLimit) * scale.X * scale.Y * scale.Z;
    }


    /// <summary>
    /// Called when the shape of a child geometric object was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="EventArgs"/> instance containing the event data.
    /// </param>
    private void OnChildPoseChanged(object sender, EventArgs eventArgs)
    {
      OnChanged(ShapeChangedEventArgs.Empty);
    }

    
    /// <summary>
    /// Called when the shape of a child geometric object was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="ShapeChangedEventArgs"/> instance containing the event data.
    /// </param>
    private void OnChildShapeChanged(object sender, ShapeChangedEventArgs eventArgs)
    {
      OnChanged(eventArgs);
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      // Convert absolute error to relative error.
      Vector3F extents = GetAabb(Vector3F.One, Pose.Identity).Extent;
      float maxExtent = extents.LargestComponent;
      float relativeThreshold = !Numeric.IsZero(maxExtent) 
                                ? absoluteDistanceThreshold / maxExtent
                                : Numeric.EpsilonF;

      // Get child mesh.
      TriangleMesh mesh = _child.Shape.GetMesh(relativeThreshold, iterationLimit);

      // Transform child mesh into local space of this parent shape.
      mesh.Transform(_child.Pose.ToMatrix44F() * Matrix44F.CreateScale(_child.Scale));
      return mesh;
    }
    #endregion
  }
}
