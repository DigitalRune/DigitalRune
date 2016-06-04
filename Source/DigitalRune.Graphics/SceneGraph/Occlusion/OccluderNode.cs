// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents an occluder in a 3D scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="OccluderNode"/> positions an <see cref="Occluder"/> in a 3D scene.
  /// <see cref="Occluder"/>s are often added as child nodes to the scene nodes they represents.
  /// This ensure that the occluders are automatically updated when parent scene node is moved.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="OccluderNode"/> is cloned, the
  /// <see cref="Occluder"/> is not duplicated. The <see cref="Occluder"/> is copied by reference
  /// (shallow copy). The original <see cref="OccluderNode"/> and the cloned instance will reference
  /// the same <see cref="Graphics.Occluder"/> object.
  /// </para>
  /// </remarks>
  /// <seealso cref="Graphics.Occluder"/>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class OccluderNode : SceneNode, IOcclusionProxy
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the occluder.
    /// </summary>
    /// <value>The occluder.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Occluder Occluder
    {
      get { return _occluder; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _occluder = value;
        Shape = value.Shape;
        RenderData = null;
      }
    }
    private Occluder _occluder;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="OccluderNode"/> class.
    /// </summary>
    internal OccluderNode()
    {
      // This internal constructor is called when loaded from an asset.
      // The occluder (shared resource) will be set later by using fix-up code
      // defined in OcclusionNodeReader.
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="OccluderNode"/> class.
    /// </summary>
    /// <param name="occluder">The occluder.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="occluder"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public OccluderNode(Occluder occluder)
    {
      if (occluder == null)
        throw new ArgumentNullException("occluder");

      Occluder = occluder;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new OccluderNode Clone()
    {
      return (OccluderNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new OccluderNode(Occluder);
    }


    ///// <inheritdoc/>
    //protected override void CloneCore(SceneNode source)
    //{
    //  // Clone SceneNode properties.
    //  base.CloneCore(source);

    //  // Clone OccluderNode properties.
    //  var sourceTyped = (OccluderNode)source;
    //}
    #endregion


    #region ----- IOcclusionProxy -----

    /// <inheritdoc/>
    bool IOcclusionProxy.HasOccluder
    {
      get { return true; }
    }


    /// <inheritdoc/>
    void IOcclusionProxy.UpdateOccluder()
    {
      var data = RenderData as OccluderData;
      if (data == null)
      {
        data = new OccluderData(Occluder);
        RenderData = data;
        IsDirty = true;
      }

      if (IsDirty)
      {
        data.Update(Occluder, PoseWorld, ScaleWorld);
        IsDirty = false;
      }
    }


    /// <inheritdoc/>
    OccluderData IOcclusionProxy.GetOccluder()
    {
      Debug.Assert(!IsDirty, "Call IOcclusionProxy.UpdateOccluder() before calling GetOccluder().");
      return (OccluderData)RenderData;
    }
    #endregion

    #endregion
  }
}
