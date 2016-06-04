// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Provides arguments for an <see cref="Shape.Changed"/> event of a <see cref="Shape"/>.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public sealed class ShapeChangedEventArgs : EventArgs, IRecyclable
  {
    private static readonly ResourcePool<ShapeChangedEventArgs> Pool =
      new ResourcePool<ShapeChangedEventArgs>(
        () => new ShapeChangedEventArgs(),
        null,
        null);


    /// <summary>
    /// Represents an event with no event data.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public new static readonly ShapeChangedEventArgs Empty = new ShapeChangedEventArgs();


    /// <summary>
    /// Gets the index of the shape feature that has changed.
    /// </summary>
    /// <value>The index of the shape feature that has changed. The default value is -1.</value>
    /// <remarks>
    /// This property indicates which feature of the <see cref="Shape"/> has changed. This value is
    /// an index that depends on the type of <see cref="Shape"/>. For most shapes, this value is not
    /// used (in this cases it is -1). See the shape documentation of individual shapes (for 
    /// example, <see cref="CompositeShape"/> or <see cref="TriangleMeshShape"/>) to find out how it
    /// is used.
    /// </remarks>
    public int Feature { get; internal set; }


    /// <summary>
    /// Prevents a default instance of the <see cref="ShapeChangedEventArgs"/> class from being 
    /// created.
    /// </summary>
    private ShapeChangedEventArgs()
    {
      Feature = -1;
    }


    /// <overloads>
    /// <summary>
    /// Creates an instance of the <see cref="ShapeChangedEventArgs"/> class. (This method reuses a 
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates an instance of the <see cref="ShapeChangedEventArgs"/> class. (This method reuses a 
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="ShapeChangedEventArgs"/> class.
    /// </returns>
    /// <inheritdoc cref="Create(int)"/>
    public static ShapeChangedEventArgs Create()
    {
      return Create(-1);
    }


    /// <summary>
    /// Creates an instance of the <see cref="ShapeChangedEventArgs"/> class with a given feature. 
    /// (This method reuses a previously recycled instance or allocates a new instance if 
    /// necessary.)
    /// </summary>
    /// <param name="feature">The index of the shape feature that has changed.</param>
    /// <returns>
    /// A new or reusable instance of the <see cref="ShapeChangedEventArgs"/> class.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap. 
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle()"/> when the instance is no longer 
    /// needed.
    /// </para>
    /// </remarks>
    public static ShapeChangedEventArgs Create(int feature)
    {
      var eventArgs = Pool.Obtain();
      eventArgs.Feature = feature;
      return eventArgs;
    }


    /// <summary>
    /// Recycles this instance of the <see cref="ShapeChangedEventArgs"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    public void Recycle()
    {
      Pool.Recycle(this);
    }
  }
}
