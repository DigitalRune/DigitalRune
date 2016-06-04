// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Collections;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a convex hull of several shapes.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This shape is similar to a <see cref="CompositeShape"/> - only that the resulting shape is the 
  /// convex hull of all child shapes. This shape is per definition convex.
  /// </para>
  /// </remarks>
  // Not much to do in this class because the ConvexShape implements the correct support mapping.
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class ConvexHullOfShapes : ConvexShape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the child objects.
    /// </summary>
    /// <value>The collection of child shapes and poses.</value>
    /// <remarks>
    /// The child objects must have a <see cref="ConvexShape"/>s.
    /// </remarks>
    public NotifyingCollection<IGeometricObject> Children { get; private set; }


    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>
    /// An inner point. If <see cref="Children"/> is empty, then (0, 0, 0) is returned.
    /// </value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space). 
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get
      {
        int numberOfGeometries = Children.Count;

        // Return the average of child inner points.
        Vector3F innerPoint = Vector3F.Zero;
        if (numberOfGeometries == 0)
          return innerPoint;

        for (int i = 0; i < numberOfGeometries; i++)
        {
          IGeometricObject child = Children[i];
          innerPoint += child.Pose.ToWorldPosition(child.Shape.InnerPoint);
        }

        return innerPoint / numberOfGeometries;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeShape"/> class.
    /// </summary>
    public ConvexHullOfShapes()
    {
      Children = new NotifyingCollection<IGeometricObject>(false, false);
      Children.CollectionChanged += OnChildrenChanged;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new ConvexHullOfShapes();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (ConvexHullOfShapes)sourceShape;
      foreach (var geometry in source.Children)
        Children.Add((IGeometricObject)geometry.Clone());
    }
    #endregion


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
      // The direction vector does not need to be normalized: Below we project the points onto
      // the direction vector and measure the length of the projection. However, we do not need
      // the correct length, we only need a value which we can compare.

      // Get support vertices of children and return the one with the largest distance.
      float maxDistance = Single.NegativeInfinity;
      Vector3F supportVertex = new Vector3F();
      int numberOfGeometries = Children.Count;
      for (int i = 0; i < numberOfGeometries; i++)
      {
        Pose pose = Children[i].Pose;
        Vector3F directionLocal = pose.ToLocalDirection(directionNormalized);
        Vector3F childSupportVertexLocal = ((ConvexShape)Children[i].Shape).GetSupportPointNormalized(directionLocal);
        Vector3F childSupportVertex = pose.ToWorldPosition(childSupportVertexLocal);
        float distance = Vector3F.Dot(childSupportVertex, directionNormalized);
        if (distance > maxDistance)
        {
          supportVertex = childSupportVertex;
          maxDistance = distance;
        }
      }
      return supportVertex;
    }


    /// <summary>
    /// Called when the <see cref="Children"/> collection was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="CollectionChangedEventArgs{IGeometricObject}"/> instance containing the event
    /// data.
    /// </param>
    /// <exception cref="GeometryException">
    /// The child geometric object in the <see cref="ConvexHullOfShapes"/> is not a 
    /// <see cref="ConvexShape"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void OnChildrenChanged(object sender, CollectionChangedEventArgs<IGeometricObject> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      // Handle removed items.
      var oldGeometries = eventArgs.OldItems;
      int numberOfOldGeometries = oldGeometries.Count;
      for (int i = 0; i < numberOfOldGeometries; i++)
      {
        IGeometricObject geometricObject = oldGeometries[i];

        geometricObject.PoseChanged -= OnChildPoseChanged;
        geometricObject.ShapeChanged -= OnChildShapeChanged;
      }

      // Handle new items.
      var newGeometries = eventArgs.NewItems;
      int numberOfNewGeometries = newGeometries.Count;
      for (int i = 0; i < numberOfNewGeometries; i++)
      {
        IGeometricObject geometricObject = newGeometries[i];

        if (geometricObject.Shape is ConvexShape == false)
          throw new GeometryException("The child objects in a ConvexHullOfShapes must have ConvexShapes.");

        geometricObject.PoseChanged += OnChildPoseChanged;
        geometricObject.ShapeChanged += OnChildShapeChanged;
      }

      OnChanged(ShapeChangedEventArgs.Empty);
    }


    /// <summary>
    /// Called when a child pose was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void OnChildPoseChanged(object sender, EventArgs eventArgs)
    {
      OnChanged(ShapeChangedEventArgs.Empty);
    }


    /// <summary>
    /// Called when a child shape was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="ShapeChangedEventArgs"/> instance containing the event data.
    /// </param>
    /// <exception cref="GeometryException">
    /// The child geometric object in the <see cref="ConvexHullOfShapes"/> is not a 
    /// <see cref="ConvexShape"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void OnChildShapeChanged(object sender, ShapeChangedEventArgs eventArgs)
    {
      IGeometricObject geometricObject = (IGeometricObject)sender;
      if (geometricObject.Shape is ConvexShape == false)
        throw new GeometryException("The child objects in a ConvexHullOfShapes must have ConvexShapes.");

      OnChanged(ShapeChangedEventArgs.Empty);
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "ConvexHullOfShapes {{ Count = {0} }}", Children.Count);
    }
    #endregion
  }
}
