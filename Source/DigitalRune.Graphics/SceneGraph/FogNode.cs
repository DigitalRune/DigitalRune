// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents (global) fog in a scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="FogNode"/> references a <see cref="Fog"/> instance, which is applied to objects 
  /// in the scene. Usually, there is only one <see cref="FogNode"/> enabled in the scene; but it is
  /// possible to use several <see cref="FogNode"/>s, e.g. to combine a height-based fog on the
  /// ground with a uniform fog in the distance.
  /// </para>
  /// <para>
  /// The default <see cref="SceneNode.Shape" /> is an <see cref="InfiniteShape"/> which covers the
  /// whole game world. 
  /// </para>
  /// <para>
  /// For height-based fog (see <see cref="Fog"/>) the y position of the fog node defines the "base
  /// level" of the fog. The height fog effect moves up or down when the fog node is moved up or
  /// down.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="FogNode"/> is cloned the <see cref="Fog"/> is not
  /// cloned. The <see cref="Fog"/> is copied by reference (shallow copy). The original 
  /// <see cref="FogNode"/> and the cloned instance will reference the same 
  /// <see cref="Graphics.Fog"/> object. 
  /// </para>
  /// </remarks>
  public class FogNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the fog properties.
    /// </summary>
    /// <value>The fog properties.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Fog Fog
    {
      get { return _fog; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _fog = value;
      }
    }
    private Fog _fog;


    /// <summary>
    /// Gets or sets the priority.
    /// </summary>
    /// <value>The priority. The default value is 0.</value>
    /// <remarks>
    /// The priority can be used to define the order in which multiple fog nodes should be applied.
    /// Fog with a higher priority should be rendered last. If the rendering system cannot handle 
    /// an arbitrary number of fog nodes, fog nodes with a lower priority might be ignored by the 
    /// rendering system.
    /// </remarks>
    public int Priority { get; set; }


    /// <summary>
    /// Gets or sets the bounding shape of this scene node.
    /// </summary>
    /// <value>
    /// The bounding shape. The bounding shape contains only the current node - it does not include 
    /// the bounds of the children! The default value is an 
    /// <see cref="DigitalRune.Geometry.Shapes.Shape.Infinite"/> shape.
    /// </value>
    /// <inheritdoc cref="SceneNode.Shape"/>
    public new Shape Shape
    {
      get { return base.Shape; }
      set { base.Shape = value; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FogNode"/> class.
    /// </summary>
    /// <param name="fog">The fog properties.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="fog"/> is <see langword="null"/>.
    /// </exception>
    public FogNode(Fog fog)
    {
      if (fog == null)
        throw new ArgumentNullException("fog");

      _fog = fog;
      Shape = Shape.Infinite;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new FogNode Clone()
    {
      return (FogNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new FogNode(_fog);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone FogNode properties.
      var sourceTyped = (FogNode)source;
      Fog = sourceTyped.Fog;
      Priority = sourceTyped.Priority;
    }
    #endregion

    #endregion
  }
}
