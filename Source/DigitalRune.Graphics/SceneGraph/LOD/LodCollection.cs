// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Stores the levels of detail (LODs) of an object.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="LodCollection"/> is a list of scene nodes sorted by distance. Each scene node
  /// represents a <i>level of detail</i> (LOD). 
  /// </para>
  /// <para>
  /// <strong>LOD0, LOD1, ... LOD<i>n-1</i>:</strong><br/>
  /// LODs can be accessed by index. The LOD with index <i>i</i> is called LOD<i>i</i>. By 
  /// definition, LOD0 is the highest level of detail and LOD<i>n-1</i> is the lowest level of 
  /// detail.
  /// </para>
  /// <para>
  /// <strong>LOD Distances:</strong><br/>
  /// LODs are selected based on the current camera and the distance to the camera. The LOD
  /// distances stored in the <see cref="LodCollection"/> are <i>view-normalized</i> distances,
  /// which means that distance values are corrected based on the camera's field-of-view. The
  /// resulting LOD distances are independent of the current field-of-view. See 
  /// <see cref="GraphicsHelper.GetViewNormalizedDistance(float, Matrix44F)"/> for more information.
  /// </para>
  /// <para>
  /// The camera that serves as reference for LOD distance computations needs to be stored in the
  /// render context (see property <see cref="RenderContext.LodCameraNode"/>).
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  [DebuggerDisplay("{GetType().Name,nq}(Count = {Count})")]
  public class LodCollection : ICollection<LodEntry>
  {
    // Notes: 
    // The LodCollection handles the SceneChanged event of the LODs. If this turns 
    // out to be too expensive, should make LodGroupNode.Begin/EndUpdate() public 
    // and update the LODs when EndUpdate() is called.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Enumerates the elements of a <see cref="LodCollection"/>. 
    /// </summary>
    public struct Enumerator : IEnumerator<LodEntry>
    {
      private readonly LodCollection _collection;
      private int _index;
      private LodEntry _current;


      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <value>The element in the collection at the current position of the enumerator.</value>
      public LodEntry Current
      {
        get { return _current; }
      }


      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <value>The element in the collection at the current position of the enumerator.</value>
      /// <exception cref="InvalidOperationException">
      /// The enumerator is positioned before the first element of the collection or after the last 
      /// element.
      /// </exception>
      object IEnumerator.Current
      {
        get
        {
          if (_index == 0 || _index == _collection._size + 1)
            throw new InvalidOperationException("The enumerator is positioned before the first element of the collection.");

          return _current;
        }
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="LodCollection.Enumerator"/> struct.
      /// </summary>
      /// <param name="collection">The <see cref="LodCollection"/> to be enumerated.</param>
      internal Enumerator(LodCollection collection)
      {
        _collection = collection;
        _index = 0;
        _current = new LodEntry();
      }


      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting 
      /// unmanaged resources.
      /// </summary>
      public void Dispose()
      {
      }


      /// <summary>
      /// Advances the enumerator to the next element of the collection.
      /// </summary>
      /// <returns>
      /// <see langword="true"/> if the enumerator was successfully advanced to the next element; 
      /// <see langword="false"/> if the enumerator has passed the end of the collection.
      /// </returns>
      /// <exception cref="InvalidOperationException">
      /// The collection was modified after the enumerator was created.
      /// </exception>
      public bool MoveNext()
      {
        if (_index >= _collection._size)
        {
          _current = new LodEntry();
          return false;
        }

        _current = _collection._array[_index];
        _index++;
        return true;
      }


      /// <summary>
      /// Sets the enumerator to its initial position, which is before the first element in the 
      /// <see cref="LodCollection"/>.
      /// </summary>
      /// <exception cref="InvalidOperationException">
      /// The <see cref="LodCollection"/> was modified after the enumerator was created.
      /// </exception>
      void IEnumerator.Reset()
      {
        _index = 0;
        _current = new LodEntry();
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly LodGroupNode _owner;
    private LodEntry[] _array;
    private int _size;
    private bool _ignoreChanges;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the number of LODs.
    /// </summary>
    /// <value>The number of LODs.</value>
    public int Count
    {
      get { return _size; }
    }


    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the <see cref="ICollection{T}"/> is read-only; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection<LodEntry>.IsReadOnly
    {
      get { return false; }
    }


    /// <summary>
    /// Gets the LOD at the specified index.
    /// </summary>
    /// <param name="index">The LOD index.</param>
    /// <value>The LOD at the specified index.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or equal to or greater than <see cref="Count"/>.
    /// </exception>
    public LodEntry this[int index]
    {
      get
      {
        if (index < 0 || index >= _size)
          throw new ArgumentOutOfRangeException("index", "The LOD index is out of range.");

        return _array[index];
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="LodCollection" /> class.
    /// </summary>
    /// <param name="owner">The <see cref="LodGroupNode" /> that owns this collection.</param>
    /// <param name="capacity">The initial capacity.</param>
    internal LodCollection(LodGroupNode owner, int capacity)
    {
      _owner = owner;
      _array = new LodEntry[capacity];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Attaches a scene node to the <see cref="LodGroupNode"/>.
    /// </summary>
    /// <param name="node">The scene node to be added.</param>
    private void AttachNode(SceneNode node)
    {
      SetProxy(node, _owner);
      node.PoseLocal = _owner.PoseLocal;
      node.ScaleLocal = _owner.ScaleLocal;
      node.SceneChanged += OnLodSceneChanged;
    }


    /// <summary>
    /// Detaches a scene node from the <see cref="LodGroupNode"/>.
    /// </summary>
    /// <param name="node">The scene node to be removed.</param>
    private void DetachNode(SceneNode node)
    {
      SetProxy(node, null);
      node.SceneChanged -= OnLodSceneChanged;
      node.PoseLocal = Pose.Identity;
      node.ScaleLocal = Vector3F.One;
    }


    /// <summary>
    /// Sets the specified <see cref="SceneNode"/> as the <see cref="SceneNode.Proxy"/> in all 
    /// referenced nodes.
    /// </summary>
    /// <param name="referencedNode">The referenced node.</param>
    /// <param name="proxyNode">The proxy node.</param>
    private static void SetProxy(SceneNode referencedNode, SceneNode proxyNode)
    {
      Debug.Assert(referencedNode != null, "node must not be null.");

      referencedNode.Proxy = proxyNode;
      if (referencedNode.Children != null)
        foreach (var childNode in referencedNode.Children)
          SetProxy(childNode, proxyNode);
    }


    internal void SetPose(Pose pose)
    {
      _ignoreChanges = true;

      for (int i = 0; i < _size; i++)
        _array[i].Node.PoseLocal = pose;

      _ignoreChanges = false;
    }


    internal void SetScale(Vector3F scale)
    {
      _ignoreChanges = true;

      for (int i = 0; i < _size; i++)
        _array[i].Node.ScaleLocal = scale;

      _ignoreChanges = false;
    }


    /// <summary>
    /// Called when the subtree of a LOD changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="SceneChangedEventArgs"/> instance containing the event data.
    /// </param>
    private void OnLodSceneChanged(object sender, SceneChangedEventArgs eventArgs)
    {
      if (_ignoreChanges)
        return;

      switch (eventArgs.Changes)
      {
        case SceneChanges.NodeAdded:
          SetProxy(eventArgs.SceneNode, _owner);
          _owner.UpdateBoundingShape();
          break;
        case SceneChanges.NodeRemoved:
          SetProxy(eventArgs.SceneNode, null);
          _owner.UpdateBoundingShape();
          break;
        case SceneChanges.PoseChanged:
        case SceneChanges.ShapeChanged:
          _owner.UpdateBoundingShape();
          break;
      }
    }


    /// <summary>
    /// Gets the camera node to use in LOD computations.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <returns>The camera node to use in LOD computations.</returns>
    private static CameraNode GetCameraNode(RenderContext context)
    {
      context.ThrowIfLodCameraMissing();
      return context.LodCameraNode;
    }


    /// <summary>
    /// Gets the view-dependent LOD data which is cached in the camera node.
    /// </summary>
    /// <param name="cameraNode">The camera node.</param>
    /// <returns>
    /// The view-dependent LOD data which is cached in the camera node.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    private LodData GetLodData(CameraNode cameraNode)
    {
      Debug.Assert(cameraNode != null, "The camera node must not be null.");

      object data;
      cameraNode.ViewDependentData.TryGetValue(_owner, out data);

      var lodData = data as LodData;
      if (lodData == null)
      {
        lodData = new LodData();
        cameraNode.ViewDependentData[_owner] = lodData;
      }

      return lodData;
    }


    /// <summary>
    /// Gets the LOD or LOD transitions for the specified distance.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="distance">The view-normalized distance (including any LOD bias).</param>
    /// <returns>
    /// An <see cref="LodSelection" /> that describes the current LOD or LOD transition.
    /// </returns>
    internal LodSelection SelectLod(RenderContext context, float distance)
    {
      // The latest LOD selection is cached to avoid recomputations.
      var cameraNode = GetCameraNode(context);
      var data = GetLodData(cameraNode);

      // The cached data is "timestamped" using the frame number.
      if (data.Frame == context.Frame)
      {
        // LOD selection is up-to-date.
        return data.Selection;
      }

      // Update LOD selection for the current frame.
      data.Frame = context.Frame;
      data.Selection = SelectLod(data.Selection.CurrentIndex, distance, context.ScaledLodHysteresis);

      return data.Selection;
    }


    private LodSelection SelectLod(int current, float distance, float hysteresis)
    {
      float t = hysteresis / 2;

      // Check LOD(n-1) ... LOD1.
      for (int i = _size - 1; i >= 1; i--)
      {
        float lodDistance = _array[i].Distance;
        float t2 = lodDistance + t;
        if (t2 <= distance)
        {
          // Discrete LOD found.
          return new LodSelection(i, _array[i].Node);
        }

        float t1 = lodDistance - t;
        if (t1 < distance)
        {
          // LOD transition found.
          int next;
          float transition;
          if (i <= current)
          {
            // LODi --> LOD(i - 1).
            current = i;
            next = i - 1;
            transition = (t2 - distance) / hysteresis;
          }
          else
          {
            // LOD(i - 1) --> LODi.
            current = i - 1;
            next = i;
            transition = (distance - t1) / hysteresis;
          }

          return new LodSelection(current, _array[current].Node, next, _array[next].Node, transition);
        }
      }

      // LOD0
      return new LodSelection(0, _array[0].Node);
    }


    /// <summary>
    /// Adds a LOD to the <see cref="LodCollection"/>.
    /// </summary>
    /// <param name="distance">
    /// The distance at which the LOD will be visible. (Must be normalized - see 
    /// <see cref="LodCollection"/>.)
    /// </param>
    /// <param name="node">
    /// The LOD node (a single scene node or subtree that represents the LOD).
    /// </param>
    /// <remarks>
    /// If a LOD at the same distance already exists in the <see cref="LodCollection"/>, it will be
    /// overwritten.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="distance"/> is negative, infinite or NaN.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public void Add(float distance, SceneNode node)
    {
      if (!Numeric.IsZeroOrPositiveFinite(distance))
        throw new ArgumentOutOfRangeException("distance", "The LOD distance must be 0 or a finite positive value.");
      if (node == null)
        throw new ArgumentNullException("node", "The LOD node must not be null.");

      int index;
      for (index = 0; index < _size; index++)
      {
        bool isLess = distance < _array[index].Distance;
        bool areEqual = Numeric.AreEqual(distance, _array[index].Distance);
        if (isLess || areEqual)
        {
          // LOD index found.
          if (areEqual)
          {
            // Replace existing LOD.
            DetachNode(_array[index].Node);

            _array[index].Distance = distance;
            _array[index].Node = node;

            AttachNode(node);
            _owner.UpdateBoundingShape();
            return;
          }
          
          break;
        }
      }

      // Insert LOD.
      if (_size == _array.Length)
        Resize();

      if (index < _size)
        Array.Copy(_array, index, _array, index + 1, _size - index);

      _array[index].Distance = distance;
      _array[index].Node = node;
      _size++;

      AttachNode(node);
      _owner.UpdateBoundingShape();
    }


    private void Resize()
    {
      var array = new LodEntry[_array.Length * 2];
      if (_size > 0)
        Array.Copy(_array, 0, array, 0, _size);

      _array = array;
    }


    /// <summary>
    /// Removes all LODs from the <see cref="LodCollection"/>.
    /// </summary>
    public void Clear()
    {
      if (_size > 0)
      {
        for (int i = 0; i < _size; i++)
          DetachNode(_array[i].Node);

        Array.Clear(_array, 0, _size);
        _size = 0;

        _owner.UpdateBoundingShape();
      }
    }


    /// <overloads>
    /// <summary>
    /// Determines the index of a specific LOD in the <see cref="LodCollection"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines the index of the LOD at a specific distance in the <see cref="LodCollection"/>.
    /// </summary>
    /// <param name="distance">
    /// The LOD distance. (Must be normalized - see <see cref="LodCollection"/>.)
    /// </param>
    /// <returns>
    /// The index of the LOD at the specified <paramref name="distance"/> if found in the 
    /// <see cref="LodCollection"/>; otherwise, -1.
    /// </returns>
    public int IndexOf(float distance)
    {
      for (int i = _size - 1; i >= 0; i--)
        if (Numeric.IsLessOrEqual(_array[i].Distance, distance))
          return i;

      return -1;
    }


    /// <summary>
    /// Determines the index of a specific LOD in the <see cref="LodCollection"/>.
    /// </summary>
    /// <param name="node">
    /// The LOD node (a single scene node or subtree that represents the LOD).
    /// </param>
    /// <returns>
    /// The index of <paramref name="node"/> if found in the <see cref="LodCollection"/>; 
    /// otherwise, -1.
    /// </returns>
    public int IndexOf(SceneNode node)
    {
      for (int i = 0; i < _size; i++)
        if (node == _array[i].Node)
          return i;

      return -1;
    }


    /// <overloads>
    /// <summary>
    /// Removes a specific LOD from the <see cref="LodCollection"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Removes the LOD at the specified index from the <see cref="LodCollection"/>.
    /// </summary>
    /// <param name="index">The index of the LOD to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="LodCollection"/>.
    /// </exception>
    public void Remove(int index)
    {
      if (index >= _size)
        throw new ArgumentOutOfRangeException("index");

      DetachNode(_array[index].Node);

      _size--;
      if (index < _size)
        Array.Copy(_array, index + 1, _array, index, _size - index);

      _array[_size] = new LodEntry();

      _owner.UpdateBoundingShape();
    }


    /// <summary>
    /// Removes the LOD at a specific distance from the <see cref="LodCollection"/>.
    /// </summary>
    /// <param name="distance">
    /// The LOD distance. (Must be normalized - see <see cref="LodCollection"/>.)
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the LOD was found and removed; otherwise, <see langword="false"/> 
    /// if the LOD was not found in the <see cref="LodCollection"/>.
    /// </returns>
    public bool Remove(float distance)
    {
      int index = IndexOf(distance);
      if (index < 0)
        return false;

      Remove(index);
      return true;
    }


    /// <summary>
    /// Removes the specified LOD node from the <see cref="LodCollection"/>.
    /// </summary>
    /// <param name="node">
    /// The LOD node (a single scene node or subtree that represents the LOD).
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the LOD was found and removed; otherwise, <see langword="false"/> 
    /// if the LOD was not found in the <see cref="LodCollection"/>.
    /// </returns>
    public bool Remove(SceneNode node)
    {
      int index = IndexOf(node);
      if (index < 0)
        return false;

      Remove(index);
      return true;
    }


    #region ----- IEnumerable -----

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<LodEntry> IEnumerable<LodEntry>.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }
    #endregion


    #region ----- ICollection<T> -----

    /// <summary>
    /// Adds an item to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <see cref="LodEntry.Distance"/> is negative, infinite or NaN.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <see cref="LodEntry.Node"/> is <see langword="null"/>.
    /// </exception>
    void ICollection<LodEntry>.Add(LodEntry item)
    {
      Add(item.Distance, item.Node);
    }

    /// <summary>
    /// Determines the index of a specific LOD in the <see cref="LodCollection"/>.
    /// </summary>
    /// <param name="lod">The LOD to locate.</param>
    /// <returns>
    /// The index of <paramref name="lod"/> if found in the <see cref="LodCollection"/>; 
    /// otherwise, -1.
    /// </returns>
    private int IndexOf(LodEntry lod)
    {
      for (int i = 0; i < _size; i++)
        if (lod.Node == _array[i].Node && Numeric.AreEqual(lod.Distance, _array[i].Distance))
          return i;

      return -1;
    }


    /// <summary>
    /// Determines whether the <see cref="ICollection{T}"/> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="ICollection{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> is found in the 
    /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection<LodEntry>.Contains(LodEntry item)
    {
      return IndexOf(item) >= 0;
    }


    /// <summary>
    /// Copies the elements of the <see cref="ICollection{T}"/> to an <see cref="Array"/>, starting 
    /// at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="ICollection{T}"/>. The <see cref="Array"/> must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal to 
    /// or greater than the length of <paramref name="array"/>. Or the number of elements in the 
    /// source <see cref="ICollection{T}"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void ICollection<LodEntry>.CopyTo(LodEntry[] array, int arrayIndex)
    {
      Array.Copy(_array, 0, array, arrayIndex, _size);
    }


    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the 
    /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>. This method also returns 
    /// <see langword="false"/> if <paramref name="item"/> is not found in the original 
    /// <see cref="ICollection{T}"/>.
    /// </returns>
    bool ICollection<LodEntry>.Remove(LodEntry item)
    {
      int index = IndexOf(item);
      if (index < 0)
        return false;

      Remove(index);
      return true;
    }
    #endregion

    #endregion
  }
}
