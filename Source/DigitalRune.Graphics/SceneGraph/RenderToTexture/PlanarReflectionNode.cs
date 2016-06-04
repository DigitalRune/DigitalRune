// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents an image of a planar reflection (e.g. a flat mirror).
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="PlanarReflectionNode"/> can be used to create a reflection image, e.g. for flat
  /// mirrors, water surfaces, etc. The render-to-texture operation is usually performed by a
  /// <see cref="PlanarReflectionRenderer"/>.
  /// </para>
  /// <para>
  /// The reflection plane goes through the local origin of this scene node. The orientation and
  /// front side of the mirror is defined by <see cref="NormalLocal"/>. The <see cref="Shape"/>
  /// should be set to encompass the reflecting object.
  /// </para>
  /// </remarks>
  public class PlanarReflectionNode : RenderToTextureNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the camera node.
    /// </summary>
    /// <value>The camera node.</value>
    internal CameraNode CameraNode { get; set; }


    /// <summary>
    /// Gets or sets the far plane distance for the reflection camera.
    /// </summary>
    /// <value>
    /// The far plane distance for the reflection camera. If this value is <see langword="null"/>,
    /// the far distance of the normal camera is used. The default value is <see langword="null"/>.
    /// </value>
    public float? Far { get; set; }


    /// <summary>
    /// Gets or sets the field-of-view scale.
    /// </summary>
    /// <value>The field-of-view scale. The default value is 1.</value>
    /// <remarks>
    /// If this value is 1, then the reflection camera will capture the scene which can be scene
    /// from a perfectly planar reflection (e.g. a flat mirror). However, when the reflection is
    /// distorted (e.g. using normal maps), then the reflection can show parts of the scene which
    /// are not visible in a flat mirror. For this case you can set the
    /// <see cref="FieldOfViewScale"/> to a value greater than 1 to capture a bigger image.
    /// </remarks>
    public float FieldOfViewScale { get; set; }


    /// <summary>
    /// Gets or sets the LOD bias.
    /// </summary>
    /// <value>The LOD bias. The default value is <see langword="null"/>.</value>
    /// <remarks>
    /// This LOD bias is applied in addition to the LOD bias of the normal camera when rendering the
    /// reflected scene.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float? LodBias { get; set; }


    /// <summary>
    /// Gets or sets the normal of the reflection plane in local space.
    /// </summary>
    /// <value>
    /// The normal of the reflection plane in local space. The default value is (0, 0, 1).
    /// </value>
    /// <remarks>
    /// This normal defines the orientation of the reflection plane. The normal points away from the
    /// front side of the reflection plane.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// The normal vector must be normalized.
    /// </exception>
    public Vector3F NormalLocal
    {
      get { return _normalLocal; }
      set
      {
        if (!value.IsNumericallyNormalized)
          throw new ArgumentException("The normal vector must be normalized.");

        _normalLocal = value;
      }
    }
    private Vector3F _normalLocal;


    /// <summary>
    /// Gets the normal of the reflection plane in world space.
    /// </summary>
    /// <value>The normal world of the reflection plane in world space.</value>
    public Vector3F NormalWorld
    {
      get { return PoseWorld.ToWorldDirection(_normalLocal); }
    }


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
    /// Initializes a new instance of the <see cref="PlanarReflectionNode" /> class.
    /// </summary>
    /// <param name="renderToTexture">The render texture target.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="renderToTexture"/> is <see langword="null"/>.
    /// </exception>
    public PlanarReflectionNode(RenderToTexture renderToTexture)
      : base(renderToTexture)
    {
      CameraNode = new CameraNode(new Camera(new PerspectiveProjection()));
      FieldOfViewScale = 1;
      _normalLocal = new Vector3F(0, 0, 1);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new PlanarReflectionNode Clone()
    {
      return (PlanarReflectionNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new PlanarReflectionNode(RenderToTexture);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone PlanarReflectionNode properties.
      var sourceTyped = (PlanarReflectionNode)source;
      Far = sourceTyped.Far;
      FieldOfViewScale = sourceTyped.FieldOfViewScale;
      LodBias = sourceTyped.LodBias;
      NormalLocal = sourceTyped.NormalLocal;
    }
    #endregion

    #endregion
  }
}
