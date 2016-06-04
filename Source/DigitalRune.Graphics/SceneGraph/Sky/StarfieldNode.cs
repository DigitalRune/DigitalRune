// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a set of stars.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class renders a list of stars (see property <see cref="Stars"/>). Each star is rendered 
  /// using a billboard projected onto the far plane. Anti-aliasing in the shader is used to get 
  /// smooth dots.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="StarfieldNode"/> is cloned the <see cref="Stars"/> 
  /// property is copied by reference (shallow copy). The original <see cref="StarfieldNode"/> and 
  /// the cloned instance will reference the same list of stars.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class StarfieldNode : SkyNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the tint color.
    /// </summary>
    /// <value>
    /// The tint color which is multiplied with the star colors. The default value is (1, 1, 1).
    /// </value>
    /// <remarks>
    /// This color can be used to change the color or the brightness of the stars.
    /// </remarks>
    public Vector3F Color { get; set; }


    /// <summary>
    /// Gets or sets the stars.
    /// </summary>
    /// <value>The stars.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public IList<Star> Stars
    {
      get { return _stars; }
      set
      {
        _stars = value;

        // Invalidate render data.
        RenderData.SafeDispose();
      }
    }
    private IList<Star> _stars;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="StarfieldNode"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="StarfieldNode"/> class.
    /// </summary>
    public StarfieldNode() : this(null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="StarfieldNode" /> class with the given set of
    /// stars.
    /// </summary>
    /// <param name="stars">The stars.</param>
    public StarfieldNode(IList<Star> stars)
    {
      Color = Vector3F.One;
      Stars = stars;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new StarfieldNode Clone()
    {
      return (StarfieldNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new SkyboxNode();
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone StarfieldNode properties.
      var sourceTyped = (StarfieldNode)source;
      Color = sourceTyped.Color;
      Stars = sourceTyped.Stars;
    }
    #endregion
  }
}
