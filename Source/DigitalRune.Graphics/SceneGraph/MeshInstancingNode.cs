// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Allows access to <see cref="MeshInstancingNode{T}"/> members which are relevant for a renderer.
  /// The interface allows access without the need to know the exact generic type.
  /// </summary>
  internal interface IMeshInstancingNode
  {
    VertexBuffer InstanceVertexBuffer { get; }
    void UpdateInstanceVertexBuffer(GraphicsDevice graphicsDevice);
  }


  /// <summary>
  /// Represents a <see cref="MeshNode"/> which uses hardware instancing to efficiently render many
  /// instances of a single <see cref="Mesh"/>.
  /// </summary>
  /// <typeparam name="T">
  /// The vertex type which stores instance data, usually <see cref="InstanceData"/>.
  /// </typeparam>
  /// <remarks>
  /// <para>
  /// This mesh node has a property <see cref="Instances"/> which defines a collection of instance
  /// data. The element type is a vertex structure. Each element in the collection encodes data for
  /// one instance of the mesh, such as world matrix, color and alpha.
  /// </para>
  /// <para>
  /// <strong>Vertex structure:</strong>A suitable vertex structure to use for the instance data is
  /// <see cref="InstanceData"/>. <see cref="InstanceData"/> is supported by the standard material
  /// effects. However, custom vertex types can be used to store different instance data. If the
  /// vertex type is not <see cref="InstanceData"/> then the mesh must use custom material effects,
  /// i.e. custom HLSL shaders that know how to interpret the encoded instance data.
  /// </para>
  /// <para>
  /// If an element in <see cref="Instances"/> is changed at runtime, 
  /// <see cref="InvalidateInstances"/> must be called to inform the mesh node that any internally
  /// cached data must be updated.
  /// </para>
  /// <para>
  /// <strong>Bounding shape:</strong> The <see cref="SceneNode.Shape"/> must be set manually to a
  /// suitable shape which contains all instances of the mesh! Per default, the shape is
  /// <see cref="Geometry.Shapes.Shape.Infinite"/> which is safe for rendering but those not allow
  /// culling and therefore is very inefficient.
  /// </para>
  /// <para>
  /// <strong>Scales, poses and world transforms:</strong> When a mesh instance is rendered, a
  /// material effect can combine the scale and pose of the scene node with the transform stored in
  /// the instance data. The effect can also ignore the scale/pose of this mesh node and only use
  /// the transform in the instance data. Currently, the  predefined material effects of DigitalRune
  /// Graphics use the later method because the shader is faster. That means, the instance data
  /// should store the world space matrix, and the scale/pose of the mesh node is ignored. (However,
  /// the scale/pose of the mesh is still used to position the bounding box for frustum culling!)
  /// </para>
  /// <para>
  /// <strong>Limitations:</strong> Following <see cref="MeshNode"/> features are currently not
  /// supported and will be ignored when an <see cref="MeshInstancingNode{T}"/> is rendered: 
  /// <list type="bullet">
  /// <item>mesh skinning (see also <see cref="MeshNode.SkeletonPose"/>)</item>
  /// <item>morphing (see also <see cref="MeshNode.MorphWeights"/>)</item>
  /// <item>occluders (see also <see cref="Mesh.Occluder"/>)</item>
  /// </list>
  /// Additionally, the <see cref="MeshInstancingNode{T}"/> can only be used in the HiDef graphics
  /// profile and not in the Reach graphics profile!
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When an <see cref="MeshInstancingNode{T}"/> is cloned the
  /// <see cref="Instances"/> collection is only copied by references (shallow copy). The original 
  /// and the cloned mesh node will reference the same <see cref="Instances"/> collection.
  /// </para>
  /// </remarks>
  public class MeshInstancingNode<T> : MeshNode, IMeshInstancingNode where T : struct, IVertexType
  {
    // Notes:
    // - When the instance vertex buffer is dynamic, we always use Discard.
    //   Using a bigger buffer with rolling overwrites using NoOverwrite/Discard
    //   could be faster in some cases.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // ReSharper disable once StaticFieldInGenericType
    private static readonly object SharedArrayLock = new object();
    private static T[] SharedArray;

    private bool _isValid;  // TODO: Move into SceneNodeFlags?
    private bool _useDynamicVertexBuffer;
    private ICollection<T> _instances;
    private VertexBuffer _instanceVertexBuffer;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the bounding shape of this scene node.
    /// </summary>
    /// <value>
    /// The bounding shape. The bounding shape contains only the current node - it does not include
    /// the bounds of the children! The default value is an
    /// <see cref="DigitalRune.Geometry.Shapes.Shape.Infinite"/> shape.
    /// </value>
    /// <remarks>
    /// The default bounding shape is <see cref="DigitalRune.Geometry.Shapes.Shape.Infinite"/>,
    /// which does not allow frustum culling. When all instances are set a bounding shape that
    /// covers all mesh instances should be set!
    /// </remarks>
    /// <inheritdoc cref="SceneNode.Shape"/>
    public new Shape Shape
    {
      get { return base.Shape; }
      set { base.Shape = value; }
    }


    /// <summary>
    /// Gets or sets the collection which stores data for each instance.
    /// </summary>
    /// <value>
    /// The collection which stores data for each instance. The default value is 
    /// <see langword="null"/>, which means that no instances are rendered.
    /// </value>
    /// <remarks>
    /// This collection must be set by the user. The most efficient collection type is
    /// an array of <typeparamref name="T"/> (<c>T[]</c>). However, any other collection type (e.g.
    /// <see cref="List{T}"/>) can be used as well.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public ICollection<T> Instances
    {
      get { return _instances; }
      set
      {
        if (_instances == value)
          return;

        _instances = value;
        InvalidateInstances();
      }
    }


    VertexBuffer IMeshInstancingNode.InstanceVertexBuffer
    {
      get { return _instanceVertexBuffer; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="MeshInstancingNode{T}"/> class.
    /// </summary>
    internal MeshInstancingNode()
    {
      // This internal constructor is called when the node is cloned.
    }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="MeshInstancingNode{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="MeshInstancingNode{T}"/> class.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>. 
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MonoGame")]
    public MeshInstancingNode(Mesh mesh)
      : base(mesh)
    {
      Shape = Shape.Infinite;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="MeshInstancingNode{T}" /> class.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh" />.</param>
    /// <param name="instances">The instances.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MonoGame")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "instances")]
    public MeshInstancingNode(Mesh mesh, ICollection<T> instances)
      : base(mesh)
    {
      Shape = Shape.Infinite;
      Instances = instances;
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing, bool disposeData)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          if (_instanceVertexBuffer != null)
          {
            _instanceVertexBuffer.Dispose();
            _instanceVertexBuffer = null;
          }
        }

        base.Dispose(disposing, disposeData);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new MeshInstancingNode<T> Clone()
    {
      return (MeshInstancingNode<T>)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new MeshInstancingNode<T>(Mesh);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone MeshNode properties.
      base.CloneCore(source);

      // Clone MeshInstancingNode properties.
      var sourceTyped = (MeshInstancingNode<T>)source;
      Instances = sourceTyped.Instances;
    }
    #endregion


    /// <summary>
    /// Notifies this <see cref="MeshInstancingNode{T}"/> that an element in <see cref="Instances"/>
    /// was modified.
    /// </summary>
    public void InvalidateInstances()
    {
      // Do nothing if already invalid. (The user may create a new MeshInstancingNode<T>,
      // set the instances and call InvalidateInstances(). In this case we do not want to
      // set _useDynamicVertexBuffer.)
      if (!_isValid)
        return;

      _isValid = false;

      // If InvalidateInstances is called by the user, then he might call it more often and
      // we use a dynamic instead of a static VB.
      _useDynamicVertexBuffer = true;
    }


    void IMeshInstancingNode.UpdateInstanceVertexBuffer(GraphicsDevice graphicsDevice)
    {
      UpdateInstanceVertexBuffer(graphicsDevice);
    }


    internal void UpdateInstanceVertexBuffer(GraphicsDevice graphicsDevice)
    {
      // Dynamic VB can lose their content in DX9!
      var dynamicVertexBuffer = _instanceVertexBuffer as DynamicVertexBuffer;
      if (dynamicVertexBuffer != null && dynamicVertexBuffer.IsContentLost)
        _isValid = false;

      if (_isValid)
        return;

      _isValid = true;
      
      // Check if existing VB must be disposed.
      if (_instanceVertexBuffer != null)
      {
        // Dispose existing VB it is static or too small.
        if (dynamicVertexBuffer == null || _instanceVertexBuffer.VertexCount < Instances.Count)
        {
          _instanceVertexBuffer.Dispose();
          _instanceVertexBuffer = null;
        }
      }

      if (Instances == null || Instances.Count == 0)
        return;

      // Create a VB. We check SceneNode.IsStatic and calls to InvalidateInstances to guess which
      // VB type we should use.
      if (!IsStatic || _useDynamicVertexBuffer)
      {
        // Dynamic vertex buffer.
        _instanceVertexBuffer = new DynamicVertexBuffer(
          graphicsDevice,
          new T().VertexDeclaration,
          Instances.Count,
          BufferUsage.WriteOnly);
      }
      else
      {
        // Static vertex buffer.
        _instanceVertexBuffer = new VertexBuffer(
          graphicsDevice,
          new T().VertexDeclaration,
          Instances.Count,
          BufferUsage.WriteOnly);
      }

      SetInstanceVertexBufferData();
    }


    private void SetInstanceVertexBufferData()
    {
      // VB.SetData(T[]) can only be used with an array of T[] (not IList<T>).
      // If Instances is not a T[], then we have to copy it into an array. We
      // use a static shared array to avoid garbage. Only one thread can access
      // this array at a time.

      var array = Instances as T[];
      if (array == null)
      {
        // Setting SharedArray is not protected because if two threads set it in parallel,
        // we only create garbage.
        lock (SharedArrayLock)
        {
          array = SharedArray;
          if (array == null || array.Length < Instances.Count)
          {
            array = new T[(int)MathHelper.NextPowerOf2((uint)Instances.Count - 1)];
            SharedArray = array;
          }

          Instances.CopyTo(array, 0);
        }
      }

      var dynamicInstanceVertexBuffer = _instanceVertexBuffer as DynamicVertexBuffer;
      if (dynamicInstanceVertexBuffer != null)
        dynamicInstanceVertexBuffer.SetData(array, 0, Instances.Count, SetDataOptions.Discard);
      else
        _instanceVertexBuffer.SetData(array, 0, Instances.Count);
    }


    internal override void OnInitializeShape()
    {
      // The MeshNode sets Shape to Mesh.BoundingShape.
      // The MeshInstancingNode does not change the Shape. This is the responsibility of the user.
    }
    #endregion
  }
}
