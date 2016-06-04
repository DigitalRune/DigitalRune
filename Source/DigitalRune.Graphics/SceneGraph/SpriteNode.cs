// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a 2D sprite in a scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="SpriteNode"/> positions a <see cref="Sprite"/> in a 3D scene. Sprites are 
  /// positioned in world space, but rendered in screen space. That means, the position of the scene
  /// node is projected into the current viewport and the 2D sprite is rendered at the resulting
  /// pixel position. The orientation of the scene node is irrelevant, it has no influence on how
  /// the sprite is rendered.
  /// </para>
  /// <para>
  /// <strong>Hit Testing:</strong><br/>
  /// The properties <see cref="LastBounds"/>, <see cref="LastDepth"/>, and 
  /// <see cref="SceneNode.LastFrame"/> can be used for simple hit testing, i.e. to check whether a
  /// pixel position hits the sprite. Hit testing against <see cref="LastBounds"/> and 
  /// <see cref="LastDepth"/> has the following limitations:
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// No occlusion: It is not possible to check whether the sprite was occluded by other objects. 
  /// Hit testing against the <see cref="LastBounds"/> may return a hit even when the sprite is
  /// occluded by other geometry.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// No rotations: The property <see cref="LastBounds"/> stores the unrotated bounding box. I.e.
  /// hit testing against rotated sprites does not work.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// No transparency: Transparent parts of the sprite count as a hit.
  /// </description>
  /// </item>
  /// </list>
  /// </remarks>
  /// <example>
  /// <para>
  /// The following method can be used to make a hit test against the sprite node:
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Test whether a mouse position hits the sprite.
  /// public static bool HitTest(IGraphicsService graphicsService, SpriteNode node, Point p)
  /// {
  ///   // Disabled nodes can be ignored.
  ///   if (!node.IsEnabled)
  ///     return false;
  /// 
  ///   // node.LastBounds is only valid if the sprite node was rendered in the last frame.
  ///   if (node.LastFrame != graphicsService.Frame)
  ///     return false;
  /// 
  ///   return node.LastBounds.Contains(p);
  /// }
  /// ]]>
  /// </code>
  /// <para>
  /// If multiple sprite nodes are hit, the property <see cref="LastDepth"/> can be used to sort the
  /// resulting sprite nodes by distance to the viewer.
  /// </para>
  /// </example>
  /// <seealso cref="Graphics.Sprite"/>
  /// <seealso cref="ImageSprite"/>
  /// <seealso cref="TextSprite"/>
  public class SpriteNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the sprite.
    /// </summary>
    /// <value>The sprite.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Sprite Sprite
    {
      get { return _sprite; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _sprite = value;
      }
    }
    private Sprite _sprite;


    /// <summary>
    /// Gets or sets the normalized animation time.
    /// </summary>
    /// <value>
    /// The normalized animation time where 0 marks the start of the animation and 1 marks the end 
    /// of the animation. The default value is 0.
    /// </value>
    /// <remarks>
    /// An <see cref="ImageSprite"/> can contain multiple animation frames. The normalized animation 
    /// time determines the current frame. (See <see cref="PackedTexture"/> for more information.)
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or greater than 1.
    /// </exception>
    public float AnimationTime
    {
      get { return _animationTime; }
      set
      {
        if (value < 0 || value > 1)
          throw new ArgumentOutOfRangeException("value", "The normalized animation time must be a value in the range [0, 1].");

        _animationTime = value;
      }
    }
    private float _animationTime;


    /// <summary>
    /// Gets or sets the tint color.
    /// </summary>
    /// <value>The tint color (non-premultiplied). The default value is white (1, 1, 1).</value>
    public Vector3F Color { get; set; }


    /// <summary>
    /// Gets or sets the opacity of the sprite.
    /// </summary>
    /// <value>The opacity. The default value is 1 (opaque).</value>
    public float Alpha { get; set; }


    /// <summary>
    /// Gets or sets the 2D origin of the sprite relative to the scene node.
    /// </summary>
    /// <value>
    /// The 2D origin of the sprite relative to the scene node. (0, 0) is the upper-left corner of 
    /// the sprite and (1, 1) is the lower-right corner of the sprite. The default value is (0, 0).
    /// </value>
    public Vector2F Origin { get; set; }


    /// <summary>
    /// Gets or sets the angle (in radians) to rotate the sprite clockwise.
    /// </summary>
    /// <value>
    /// The angle (in radians) to rotate the sprite clockwise around its origin. The default value 
    /// is 0.
    /// </value>
    public float Rotation { get; set; }


    /// <summary>
    /// Gets or sets the 2D scale of the sprite. 
    /// </summary>
    /// <value>
    /// The 2D scale of the sprite. The default value is (1, 1).
    /// </value>
    public Vector2F Scale
    {
      get { return new Vector2F(ScaleLocal.X, ScaleLocal.Y); }
      set { ScaleLocal = new Vector3F(value.X, value.Y, 1); }
    }


    /// <summary>
    /// Gets the location and size in pixel at which the sprite was rendered.
    /// </summary>
    /// <value>The 2D bounding box of the sprite in pixel.</value>
    /// <remarks>
    /// <para>
    /// The properties <see cref="LastBounds"/> and <see cref="LastDepth"/> are updated every time
    /// the sprite node is rendered. The property <see cref="SceneNode.LastFrame"/> indicates at
    /// which frame the sprite node was rendered last.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> The property <see cref="LastBounds"/> stores the unrotated bounding
    /// box.
    /// </para>
    /// </remarks>
    public Rectangle LastBounds { get; internal set; }


    /// <summary>
    /// Gets the depth at which the sprite was rendered.
    /// </summary>
    /// <value>The depth of the sprite in the range [0, 1].</value>
    /// <inheritdoc cref="LastBounds"/>
    public float LastDepth { get; internal set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteNode"/> class.
    /// </summary>
    /// <param name="sprite">The sprite.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sprite"/> is <see langword="null"/>.
    /// </exception>
    public SpriteNode(Sprite sprite)
    {
      if (sprite == null)
        throw new ArgumentNullException("sprite");

      _sprite = sprite;
      IsRenderable = true;
      Shape = Shape.Infinite;
      Color = Vector3F.One;
      Alpha = 1;
      LastBounds = Rectangle.Empty;
      LastDepth = 0;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new SpriteNode Clone()
    {
      return (SpriteNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new SpriteNode(Sprite);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone SpriteNode properties.
      var sourceTyped = (SpriteNode)source;
      AnimationTime = sourceTyped.AnimationTime;
      Color = sourceTyped.Color;
      Alpha = sourceTyped.Alpha;
      Origin = sourceTyped.Origin;
      Rotation = sourceTyped.Rotation;
      // Scale is cloned in base class.
    }
    #endregion

    #endregion
  }
}
