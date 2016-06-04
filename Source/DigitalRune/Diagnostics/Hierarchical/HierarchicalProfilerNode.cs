// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace DigitalRune.Diagnostics
{
  /// <summary>
  /// Defines a node in the hierarchical measurements of a <see cref="HierarchicalProfiler"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="HierarchicalProfilerNode"/> stores the measured times for a method or a measured
  /// code section. It has a <see cref="Name"/> and is usually called like the method that is
  /// profiled. It links to a <see cref="Parent"/> node, which is the method or code section from
  /// which the code was called. It has <see cref="Children"/> which represent profiled methods or
  /// code sections that are called by the code of this node.
  /// </para>
  /// <para>
  /// <strong>Recursive calls:</strong> If this node is used to measure a recursive method,
  /// following conventions apply: <see cref="Count"/> counts all calls including recursive calls.
  /// <see cref="Minimum"/>, <see cref="Average"/> and <see cref="Maximum"/> ignore recursions and
  /// measure the time of the one call including recursions. That means, if the method "Foo" is
  /// called once, and it calls itself recursively, this counts as one call for the computation of
  /// Minimum/Average/Maximum.
  /// </para>
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public class HierarchicalProfilerNode : INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private TimeSpan _startTime; 

    // 0 = this node is not active. 
    // 1 = this node was entered once. 
    // 2 = this node was entered twice (1 recursion!).
    // ...
    private int _numberOfRecursions;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the name of this node.
    /// </summary>
    /// <value>The name of this node.</value>
    public string Name { get; private set; }


    /// <summary>
    /// Gets the profiler.
    /// </summary>
    /// <value>The profiler.</value>
    public HierarchicalProfiler Profiler { get; private set; }


    /// <summary>
    /// Gets the parent node.
    /// </summary>
    /// <value>The parent node.</value>
    public HierarchicalProfilerNode Parent { get; private set; }

    
    private HierarchicalProfilerNode FirstChild { get; set; }

    
    private HierarchicalProfilerNode LastChild { get; set; }


    private HierarchicalProfilerNode NextSibling { get; set; }


    /// <summary>
    /// Gets the child nodes.
    /// </summary>
    /// <value>The child nodes.</value>
    public IEnumerable<HierarchicalProfilerNode> Children
    {
      get 
      { 
        var child = FirstChild;
        while (child != null)
        {
          yield return child;
          child = child.NextSibling;
        }
      }
    }


    /// <summary>
    /// Gets how often this node was called.
    /// </summary>
    /// <value>The number of calls.</value>
    /// <remarks>
    /// The <see cref="Count"/> is incremented when <see cref="HierarchicalProfiler.Start"/> is
    /// called. 
    /// </remarks>
    public int Count { get; private set; }


    /// <summary>
    /// Gets the total accumulated time of this node.
    /// </summary>
    /// <value>The total time of this node.</value>
    public TimeSpan Sum { get; private set; }


    /// <summary>
    /// Gets the minimum time of all non-recursive calls.
    /// </summary>
    /// <value>The minimum time of all non-recursive calls.</value>
    public TimeSpan Minimum { get; private set; }


    /// <summary>
    /// Gets the maximum time of all non-recursive calls.
    /// </summary>
    /// <value>The maximum time of all non-recursive calls.</value>
    public TimeSpan Maximum { get; private set; }


    /// <summary>
    /// Gets the average (arithmetic mean) time of all non-recursive calls.
    /// </summary>
    /// <value>The average (arithmetic mean) time of all non-recursive calls.</value>
    public TimeSpan Average
    {
      get
      {
        if (Count == 0)
          return TimeSpan.Zero;

        return new TimeSpan(Sum.Ticks / Count);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="HierarchicalProfilerNode"/> class.
    /// (Only for Root nodes.)
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="profiler">The profiler.</param>
    internal HierarchicalProfilerNode(string name, HierarchicalProfiler profiler)
    {
      Name = name;
      Profiler = profiler;
      Reset();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="HierarchicalProfilerNode"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="parent">The parent.</param>
    internal HierarchicalProfilerNode(string name, HierarchicalProfilerNode parent)
    {
      Name = name;
      Profiler = parent.Profiler;
      Parent = parent;
      Reset();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Resets this node, all siblings and all children.
    /// </summary>
    internal void Reset()
    {
      _numberOfRecursions = 0;

      Count = 0;
      Sum = TimeSpan.Zero;
      Minimum = TimeSpan.Zero;
      Maximum = TimeSpan.Zero;

      if (FirstChild != null)
        FirstChild.Reset();
      if (NextSibling != null)
        NextSibling.Reset();
    }


    /// <summary>
    /// Starts time measurement.
    /// </summary>
    internal void Call()
    {
      Count++;

      if (_numberOfRecursions == 0)
        _startTime = Profiler.ElapsedTime;

      _numberOfRecursions++;
    }


    /// <summary>
    /// Stops time measurement.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if this ends the node. <see langword="false"/> if this node is still 
    /// active because only a recursive call returned.
    /// </returns>
    internal bool Return()
    {
      Debug.Assert(Count > 0);

      _numberOfRecursions--;

      if (_numberOfRecursions == 0)
      {
        // Time is measured when the whole recursion has ended.
        TimeSpan stopTime = Profiler.ElapsedTime;
        TimeSpan elapsedTime = stopTime - _startTime;
        Sum += stopTime - _startTime;

        if (Count == 1)
        {
          Minimum = elapsedTime;
          Maximum = elapsedTime;
        }
        else
        {
          if (elapsedTime < Minimum)
            Minimum = elapsedTime;

          if (elapsedTime > Maximum)
            Maximum = elapsedTime;
        }
      }

      return _numberOfRecursions == 0;
    }


    /// <summary>
    /// Gets a node with the given name. If no child with this name exists, a new child is appended.
    /// </summary>
    /// <param name="nodeName">The name of the node.</param>
    /// <returns>The child node.</returns>
    internal HierarchicalProfilerNode GetSubNode(string nodeName)
    {
      var child = FirstChild;
      while (child != null)
      {
        if (child.Name == nodeName)
          return child;

        child = child.NextSibling;
      }

      // Did not find a matching child node. --> Add new child.
      var node = new HierarchicalProfilerNode(nodeName, this);

      if (LastChild == null)
      {
        Debug.Assert(FirstChild == null);
        Debug.Assert(LastChild == null);
        FirstChild = node;
        LastChild = node;
      }
      else
      {
        LastChild.NextSibling = node;
        LastChild = node;
      }

      return node;
    }
    #endregion
  }
}
