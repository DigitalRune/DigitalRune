// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a shape consisting of several other geometric objects.
  /// </summary>
  /// <remarks> 
  /// <para>
  /// This shape is a collection of geometric objects (see <see cref="Children"/>). Each child has a
  /// <see cref="IGeometricObject.Shape"/>, <see cref="IGeometricObject.Scale"/> and a 
  /// <see cref="IGeometricObject.Pose"/> (= position and orientation). All children are defined in
  /// the local space of the composite shape. That means that the 
  /// <see cref="IGeometricObject.Pose"/> defines the position and orientation of each child
  /// relative to the parent composite shape. The resulting composite shape can be concave.
  /// </para>
  /// <para>
  /// Other names for this type of shape: Complex, Compound, Group, ...
  /// </para>
  /// <para>
  /// <strong>Spatial Partitioning:</strong> A spatial partitioning method (see 
  /// <see cref="Partition"/> can be used to improve runtime performance if this composite shape
  /// consists of a lot of children. A spatial partition improves the collision detection speed at
  /// the cost of additional memory. If <see cref="Partition"/> is <see langword="null"/>, no
  /// spatial partitioning method is used (which is the default). If a spatial partitioning scheme
  /// should be used, the property <see cref="Partition"/> must be set to an instance of
  /// <see cref="ISpatialPartition{T}"/>. The items in the spatial partition will be the indices of
  /// the <see cref="Children"/> of this composite shape. The composite shape will automatically
  /// fill and update the spatial partition. Following example shows how a complex composite shape
  /// can be improved by using an AABB tree:
  /// <code lang="csharp">
  /// <![CDATA[
  /// myCompositeShape.Partition = new AabbTree<int>();
  /// ]]>
  /// </code>
  /// </para>
  /// <para>
  /// <strong>Shape Features:</strong> If a <see cref="CompositeShape"/> is involved in a 
  /// <see cref="Contact"/>, the shape feature property (<see cref="Contact.FeatureA"/> and
  /// <see cref="Contact.FeatureB"/>) contains the index of the child that caused the 
  /// <see cref="Contact"/>.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> 
  /// If a <see cref="CompositeShape"/> is cloned, all <see cref="Children"/> and the spatial
  /// partition (if any is in use) will be cloned (deep copy).
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class CompositeShape : Shape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the child geometric objects.
    /// </summary>
    /// <value>The collection of child shapes with scale and pose.</value>
    /// <remarks>
    /// The poses (positions and orientations) of the children is relative to the parent. That means 
    /// the poses define the transformations in the local space of the <see cref="CompositeShape"/>.
    /// </remarks>
    public NotifyingCollection<IGeometricObject> Children { get; private set; }


    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>
    /// An inner point. (If the <see cref="CompositeShape"/> is empty, (0, 0, 0) is returned.
    /// </value>
    /// <remarks>
    /// This point is a random "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get
      {
        // Return the inner point of the first shape or the default.
        if (Children.Count > 0)
        {
          Debug.Assert(Children[0].Shape is EmptyShape == false, "EmptyShape as a child of a composite shape is not supported.");
          var child = Children[0];
          return child.Pose.ToWorldPosition(child.Shape.InnerPoint * child.Scale);
        }
        
        return Vector3F.Zero;
      }
    }


    /// <summary>
    /// Gets or set the spatial partition used to improve the performance of geometric queries.
    /// </summary>
    /// <value>
    /// The spatial partition. Per default no spatial partition is used and this property is 
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// The spatial partition stores only the indices into the <see cref="Children"/> collection. 
    /// This property can be set to <see langword="null"/> to remove a spatial partition. If a 
    /// spatial partition is set, the composite shape will automatically fill and update the
    /// spatial partition.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public ISpatialPartition<int> Partition
    {
      get { return _partition; }
      set
      {
        if (_partition != value)
        {
          // Clear old partition.
          if (_partition != null)
          {
            _partition.GetAabbForItem = null;
            _partition.Clear();
          }

          // Set new partition.
          _partition = value;

          // Fill new partition.
          if (_partition != null)
          {
            _partition.GetAabbForItem = i => Children[i].Aabb;
            _partition.Clear();
            int numberOfChildren = Children.Count;
            for (int i = 0; i < numberOfChildren; i++)
              _partition.Add(i);
          }
        }
      }
    }
    private ISpatialPartition<int> _partition;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeShape"/> class.
    /// </summary>
    public CompositeShape()
    {
      Children = new NotifyingCollection<IGeometricObject>(false, false);
      Children.CollectionChanged += OnChildrenChanged;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

#if XNA || MONOGAME
    /// <summary>
    /// Sets the spatial partition. (For use by the content pipeline only.)
    /// </summary>
    /// <param name="partition">The spatial partition.</param>
    /// <remarks>
    /// This method is used internally to directly set the spatial partition. The spatial partition
    /// might already be initialized and should not be invalidated.
    /// </remarks>
    internal void SetPartition(ISpatialPartition<int> partition)
    {
      if (partition != null)
      {
        _partition = partition;
        _partition.GetAabbForItem = i => Children[i].Aabb;

        // ----- Validate spatial partition.
        // Some spatial partitions, such as the CompressedAabbTree, are pre-initialized when 
        // loaded via content pipeline. Other spatial partitions need to be initialized manually.
        int numberOfChildren = Children.Count;
        if (_partition.Count != numberOfChildren)
        {
          // The partition is not initialized.
          _partition.Clear();
          for (int i = 0; i < numberOfChildren; i++)
            _partition.Add(i);

          _partition.Update(false);
        }
        else
        {
          // The partition is already initialized.
          Debug.Assert(Enumerable.Range(0, numberOfChildren).All(_partition.Contains), "Invalid partition. The pre-initialized partition does not contain the same children as the CompositeShape.");
        }
      }
    }
#endif


    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new CompositeShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (CompositeShape)sourceShape;
      foreach (var geometry in source.Children)
        Children.Add((IGeometricObject)geometry.Clone());

      if (source.Partition != null)
        Partition = source.Partition.Clone();
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      // If we have a spatial partition, we can use the AABB of the whole spatial partition to 
      // quickly get an approximate AABB.
      if (Partition != null)
      {
        // Get AABB of spatial partition and compute the AABB in world space.
        return Partition.Aabb.GetAabb(scale, pose);
      }

      // No children? - Return arbitrary AABB.
      var numberOfGeometries = Children.Count;
      if (numberOfGeometries == 0)
        return new Aabb(pose.Position, pose.Position);

      // See also comments in TransformShape.GetAabb().

      if (scale.X == scale.Y && scale.Y == scale.Z)
      {
        // Uniform scaling.

        // Get union of children's AABBs. scale is applied at the end.
        var child = Children[0];

        // Scale child pose.
        Pose childPose = new Pose(child.Pose.Position * scale.X, child.Pose.Orientation);

        // Get child aabb in final space.
        Aabb aabb = child.Shape.GetAabb(scale.X * child.Scale, pose * childPose);
        for (int i = 1; i < numberOfGeometries; i++)
        {
          child = Children[i];
          childPose = new Pose(child.Pose.Position * scale.X, child.Pose.Orientation);
          aabb.Grow(child.Shape.GetAabb(scale.X * child.Scale, pose * childPose));
        }

        return aabb;
      }
      else
      {
        // Get AABB of children in the parent space without scale.
        Aabb aabb = Children[0].Aabb;
        for (int i = 1; i < numberOfGeometries; i++)
          aabb.Grow(Children[i].Aabb);

        // Now, from this compute an AABB in world space.
        return aabb.GetAabb(scale, pose);
      }
    }


    /// <inheritdoc/>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      float volume = 0;
      foreach (var child in Children)
      {
        var scale = Vector3F.Absolute(child.Scale);
        volume += child.Shape.GetVolume(relativeError, iterationLimit) * scale.X * scale.Y * scale.Z;
      }

      return volume;
    }


    /// <summary>
    /// Called when the <see cref="Children"/> collection was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="CollectionChangedEventArgs{IGeometricObject}"/> instance containing the event
    /// data.
    /// </param>
    private void OnChildrenChanged(object sender, CollectionChangedEventArgs<IGeometricObject> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      // Handle removed items.
      var oldItems = eventArgs.OldItems;
      int numberOfOldItems = oldItems.Count;
      for (int i = 0; i < numberOfOldItems; i++)
      {
        var geometricObject = oldItems[i];

        geometricObject.PoseChanged -= OnChildShapeChanged;
        geometricObject.ShapeChanged -= OnChildShapeChanged;
      }

      // Handle new items.
      var newItems = eventArgs.NewItems;
      int numberOfNewItems = newItems.Count;
      for (int i = 0; i < numberOfNewItems; i++)
      {
        var geometricObject = newItems[i];

        geometricObject.PoseChanged += OnChildShapeChanged;
        geometricObject.ShapeChanged += OnChildShapeChanged;
      }

      // Rebuild spatial partition.
      if (_partition != null)
      {
        _partition.Clear();
        int numberOfChildren = Children.Count;
        for (int i = 0; i < numberOfChildren; i++)
          _partition.Add(i);
      }

      if (numberOfOldItems == 1
          && numberOfNewItems == 1
          && eventArgs.OldItemsIndex == eventArgs.NewItemsIndex)
      {
        // Exactly one item was replaced. 
        // --> Set the feature index of the item in the event args.
        var shapeChangedEventArgs = ShapeChangedEventArgs.Create(eventArgs.OldItemsIndex);
        OnChanged(shapeChangedEventArgs);
        shapeChangedEventArgs.Recycle();
      }
      else
      {
        // Multiple items added or removed. The indices of multiple items have changed.
        // --> Do not set a feature index. Use the default.
        OnChanged(ShapeChangedEventArgs.Empty);
      }
    }


    /// <summary>
    /// Called when a child object was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="EventArgs"/> instance containing the event data.
    /// </param>
    private void OnChildShapeChanged(object sender, EventArgs eventArgs)
    {
      // Invalidate partition.
      var index = Children.IndexOf(sender as IGeometricObject);
      if (Partition != null)
      {
        if (index >= 0)
          Partition.Invalidate(index);
        else
          Partition.Invalidate();
      }

      var shapeChangedEventArgs = ShapeChangedEventArgs.Create(index);
      OnChanged(shapeChangedEventArgs);
      shapeChangedEventArgs.Recycle();
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
      float maxExtent = GetAabb(Vector3F.One, Pose.Identity).Extent.LargestComponent;
      float relativeThreshold = !Numeric.IsZero(maxExtent)
                                ? absoluteDistanceThreshold / maxExtent
                                : Numeric.EpsilonF;

      // Get meshes of children and add them to mesh in parent space.
      TriangleMesh mesh = new TriangleMesh();
      int numberOfGeometries = Children.Count;
      for (int childIndex = 0; childIndex < numberOfGeometries; childIndex++)
      {
        IGeometricObject geometricObject = Children[childIndex];

        // Get child mesh.
        var childMesh = geometricObject.Shape.GetMesh(relativeThreshold, iterationLimit);

        // Transform child mesh into local space of this parent shape.
        childMesh.Transform(geometricObject.Pose.ToMatrix44F() * Matrix44F.CreateScale(geometricObject.Scale));

        // Add to parent mesh.
        mesh.Add(childMesh, false);
      }

      return mesh;
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "CompositeShape {{ Count = {0} }}", Children.Count);
    }
    #endregion
  }
}
