// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Partitioning;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Filters collision objects by forwarding to a filter that works on scene nodes.
  /// </summary>
  internal sealed class SceneCollisionObjectFilter : IPairFilter<CollisionObject>
  {
    /// <summary>
    /// Gets or sets the scene node filter.
    /// </summary>
    /// <value>The scene node filter.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public IPairFilter<SceneNode> SceneNodeFilter
    {
      get { return _filter; }
      set
      {
        if (_filter == value)
          return;

        if (value == null)
          throw new ArgumentNullException("value");

        _filter.Changed -= OnChanged;
        _filter = value;
        _filter.Changed += OnChanged;
      }
    }
    private IPairFilter<SceneNode> _filter;


    /// <inheritdoc/>
    public event EventHandler<EventArgs> Changed;


    /// <summary>
    /// Initializes a new instance of the <see cref="SceneCollisionObjectFilter"/> class.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="filter"/> is <see langword="null"/>.
    /// </exception>
    public SceneCollisionObjectFilter(IPairFilter<SceneNode> filter)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      _filter = filter;
      _filter.Changed += OnChanged;
    }


    /// <inheritdoc/>
    public bool Filter(Pair<CollisionObject> pair)
    {
      var nodeA = pair.First.GeometricObject as SceneNode;
      var nodeB = pair.Second.GeometricObject as SceneNode;

      if (nodeA == null || nodeB == null)
        return true;
      
      return _filter.Filter(new Pair<SceneNode>(nodeA, nodeB));
    }
    
    
    private void OnChanged(object sender, EventArgs eventArgs)
    {
      var handler = Changed;

      if (handler != null)
        handler(this, eventArgs);
    }
  }
}
