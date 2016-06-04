// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Provides arguments for an event concerning a <see cref="SceneNode"/>.
  /// </summary>
  public sealed class SceneChangedEventArgs : EventArgs
  {
    private static readonly ResourcePool<SceneChangedEventArgs> Pool = new ResourcePool<SceneChangedEventArgs>(
      () => new SceneChangedEventArgs(),  // Create
      null,                               // Initialize
      null);                              // Uninitialize


    /// <summary>
    /// Gets or sets the scene node.
    /// </summary>
    /// <value>The scene node.</value>
    public SceneNode SceneNode { get; set; }


    /// <summary>
    /// Gets or sets the changes.
    /// </summary>
    /// <value>The changes.</value>
    public SceneChanges Changes { get; set; }


    /// <summary>
    /// Constructs a new instance of the <see cref="SceneChangedEventArgs"/> class.
    /// </summary>
    private SceneChangedEventArgs()
    {
    }


    /// <summary>
    /// Creates an instance of the <see cref="SceneChangedEventArgs"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <param name="sceneNode">The scene node.</param>
    /// <param name="changes">The changes.</param>
    /// <returns>
    /// A new or reusable instance of the <see cref="SceneChangedEventArgs"/> class.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap.
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle"/> when the instance is no longer
    /// needed.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sceneNode"/> is <see langword="null"/>.
    /// </exception>
    public static SceneChangedEventArgs Create(SceneNode sceneNode, SceneChanges changes)
    {
      if (sceneNode == null)
        throw new ArgumentNullException("sceneNode");

      var args = Pool.Obtain();
      args.SceneNode = sceneNode;
      args.Changes = changes;
      return args;
    }


    /// <summary>
    /// Recycles this instance of the <see cref="SceneChangedEventArgs"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    public void Recycle()
    {
      SceneNode = null;
      //Changes = SceneChanges.Any;
      Pool.Recycle(this);
    }
  }
}
