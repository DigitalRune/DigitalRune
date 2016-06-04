// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a material projected onto another surface.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A decal is a <see cref="Material"/> projected onto other geometry. Static decals can be used 
  /// to add variety to a scene: posters on walls, dirt on objects, puddles on the floor, etc. 
  /// Dynamic decals are created by the game logic based on events in the game: bullet holes at a 
  /// point of impact, blood splatters, foot prints on the ground, tire marks on the street, etc.
  /// </para>
  /// <para>
  /// The visual properties are defined by a decal material (see <see cref="Material"/>). Note that 
  /// it is not possible to use the same materials as used for regular meshes. Decals need to be 
  /// rendered with special vertex and pixel shaders and therefore require a different 
  /// <see cref="Effect"/> than meshes.
  /// </para>
  /// <para>
  /// The properties <see cref="Width"/>, <see cref="Height"/> and <see cref="Depth"/> define the 
  /// bounding volume of the decal. The decal material will be projected onto meshes within the 
  /// this bounding volume.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="DecalNode"/> is cloned the <see cref="Material"/> 
  /// is only copied by reference (shallow copy). The original decal node and the cloned decal node 
  /// will reference the same <see cref="Graphics.Material"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="Graphics.Material"/>
  public class DecalNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // Defines the decal projection volume: 
    // - A unit cube centered at (0, 0, -0.5).
    // - The shape is shared by all decal nodes (singleton).
    private sealed class DecalVolume : OrthographicViewVolume
    {
      /// <summary>The decal projection volume.</summary>
      public static readonly DecalVolume Instance = new DecalVolume();

      public override event EventHandler<ShapeChangedEventArgs> Changed { add { } remove { } }

      private DecalVolume() : base(1.0f, 1.0f, 0.0f, 1.0f)
      { }

      protected override Shape CreateInstanceCore()
      {
        return new DecalVolume();
      }

      protected override void CloneCore(Shape sourceShape)
      { }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Optimization: Store the hash values of all render passes for fast lookup.
    private int[] _passHashes;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the width of the decal.
    /// </summary>
    /// <value>The width of the decal.</value>
    /// <remarks>
    /// <para>
    /// The properties <see cref="Width"/>, <see cref="Height"/> and <see cref="Depth"/> define the
    /// bounding volume of the decal. The decal material will be projected onto meshes within
    /// this bounding volume.
    /// </para>
    /// <para>
    /// Note: (<see cref="Width"/>, <see cref="Height"/>, <see cref="Depth"/>) is the same as
    /// <see cref="SceneNode.ScaleLocal"/>.
    /// </para>
    /// </remarks>
    public float Width
    {
      get { return ScaleLocal.X; }
      set { ScaleLocal = new Vector3F(value, ScaleLocal.Y, ScaleLocal.Z); }
    }


    /// <summary>
    /// Gets or sets the height of the decal.
    /// </summary>
    /// <value>The height of the decal.</value>
    /// <inheritdoc cref="Width"/>
    public float Height
    {
      get { return ScaleLocal.Y; }
      set { ScaleLocal = new Vector3F(ScaleLocal.X, value, ScaleLocal.Z); }
    }


    /// <summary>
    /// Gets or sets the depth of the decal.
    /// </summary>
    /// <value>The depth of the decal.</value>
    /// <inheritdoc cref="Width"/>
    public float Depth
    {
      get { return ScaleLocal.Z; }
      set { ScaleLocal = new Vector3F(ScaleLocal.X, ScaleLocal.Y, value); }
    }


    /// <summary>
    /// Gets or sets the options for rendering the decal.
    /// </summary>
    /// <value>
    /// The options for rendering the decal. The default value is 
    /// <see cref="DecalOptions.ProjectOnAll"/>.
    /// </value>
    public DecalOptions Options { get; set; }


    /// <summary>
    /// Gets or sets the decal material.
    /// </summary>
    /// <value>The decal material.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public Material Material
    {
      get { return _material; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _material = value;
        MaterialInstance = new MaterialInstance(Material);

        int i = 0;
        _passHashes = new int[value.Passes.Count];
        foreach (var pass in value.Passes)
          _passHashes[i++] = pass.GetHashCode();
      }
    }
    private Material _material;


    /// <summary>
    /// Gets the material instance.
    /// </summary>
    /// <value>The material instance.</value>
    public MaterialInstance MaterialInstance { get; private set; }


    /// <summary>
    /// Gets or sets the opacity of the decal.
    /// </summary>
    /// <value>The opacity of the decal. The default value is 1 (opaque).</value>
    public float Alpha { get; set; }


    /// <summary>
    /// Gets or sets the draw order.
    /// </summary>
    /// <value>The draw order. The default value is 0.</value>
    /// <remarks>
    /// This property defines the order in which decals are drawn. Decals are drawn in ascending 
    /// order, i.e. decals with a higher value are drawn on top of decals with a lower value. The 
    /// draw order is only relevant when multiple decals overlap.
    /// </remarks>
    public int DrawOrder { get; set; }


    /// <summary>
    /// Gets or sets the normal threshold in radians.
    /// </summary>
    /// <value>The normal threshold in radians. The default value is π/3 (= 60°).</value>
    /// <remarks>
    /// <para>
    /// When a decal is applied to an uneven surface, such as a corner, it may be stretched along the 
    /// sides. This can be prevented by setting a normal threshold: The renderer checks whether the 
    /// normal of the receiving surface is within a certain angle. If the normal deviates by more 
    /// than the normal threshold, the decal is clipped.
    /// </para>
    /// <para>
    /// The normal threshold needs to be in the range [0, π]. The default value is π/3
    /// (= 60°).
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or greater than π.
    /// </exception>
    public float NormalThreshold
    {
      get { return _normalThreshold; }
      set
      {
        if (Numeric.IsLess(value, 0) || Numeric.IsGreater(value, ConstantsF.Pi))
          throw new ArgumentOutOfRangeException("value", "The normal threshold of a decal needs to be in the range [0, π].");

        _normalThreshold = value;
      }
    }
    private float _normalThreshold;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DecalNode"/> class.
    /// </summary>
    /// <param name="material">The decal material.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="material"/> is <see langword="null"/>.
    /// </exception>
    public DecalNode(Material material)
    {
      if (material == null)
        throw new ArgumentNullException("material");

      IsRenderable = true;
      Shape = DecalVolume.Instance;
      Material = material;
      Alpha = 1;
      NormalThreshold = ConstantsF.Pi / 3; // = 60°
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new DecalNode Clone()
    {
      return (DecalNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new DecalNode(Material);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone DecalNode properties.
      var sourceTyped = (DecalNode)source;
      _passHashes = sourceTyped._passHashes;
      Alpha = sourceTyped.Alpha;
      DrawOrder = sourceTyped.DrawOrder;
      NormalThreshold = sourceTyped.NormalThreshold;
      Options = sourceTyped.Options;
      // (Width, Height, Depth) is ScaleLocal, which is copied in base class.
    }
    #endregion


    /// <summary>
    /// Determines whether the decal node supports the specified render pass.
    /// </summary>
    /// <param name="passHash">The hash value of the render pass.</param>
    /// <returns>
    /// <see langword="true"/> if the decal node contains a material with the specified render pass; 
    /// otherwise, <see langword="false"/> if the decal node does not support the specified render 
    /// pass.
    /// </returns>
    /// <remarks>
    /// The method is used only internally as an optimization. Only the hash value is checked, 
    /// therefore the method may return a false positive!
    /// </remarks>
    internal bool IsPassSupported(int passHash)
    {
      for (int i = 0; i < _passHashes.Length; i++)
        if (_passHashes[i] == passHash)
          return true;

      return false;
    }
    #endregion
  }
}
