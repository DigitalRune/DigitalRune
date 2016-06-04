// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if PARTICLES
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents an instance of a particle effect in a 3D scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="ParticleSystemNode"/> positions a <see cref="ParticleSystem"/> in a 3D scene.
  /// Particles can be rendered using the <see cref="BillboardRenderer"/>. Only particle system that
  /// have the required particle parameters are rendered. See list below.
  /// </para>
  /// <note type="important">
  /// <strong>Synchronization:</strong>
  /// The particle system service (which simulates the particle motion) and the graphics service 
  /// (which draws particles) may be run in sequence or in parallel. In every frame the particle 
  /// information needs to be copied from the <see cref="Particles.ParticleSystem"/> to the 
  /// <see cref="ParticleSystemNode"/>. This is done by calling <see cref="Synchronize"/>. This 
  /// method needs to be called by the application logic for each <see cref="ParticleSystemNode"/>! 
  /// (It needs to be called at a point where the particle system service and the graphics service 
  /// are idle - usually when the game objects are updated.)
  /// </note>
  /// <para>
  /// Note that both the particle system and the scene node have an "Enabled" flag. The 
  /// <see cref="Particles.ParticleSystem"/>'s <see cref="Particles.ParticleSystem.Enabled"/> flag 
  /// determines whether the particle system is simulated by the particle system manager. If the 
  /// particle system is disabled, particles are no longer emitted or updated. The 
  /// <see cref="ParticleSystemNode"/>'s <see cref="SceneNode.IsEnabled"/> flag determines whether 
  /// the scene node is rendered.
  /// </para>
  /// <para>
  /// <strong>Particle Reference Frame and Instancing:</strong><br/>
  /// The <see cref="Particles.ParticleSystem.ReferenceFrame"/> defines the reference frame of the 
  /// particles. 
  /// </para>
  /// <para>
  /// <see cref="ParticleReferenceFrame.World"/> means that the particles are positioned directly in
  /// world space. In this case the pose (position and orientation) of the particle system controls 
  /// the pose of the scene node. In <see cref="Synchronize"/> the 
  /// <see cref="Particles.ParticleSystem.Pose"/> of the <see cref="ParticleSystem"/> is copied to
  /// <see cref="SceneNode.PoseWorld"/> of the <see cref="ParticleSystemNode"/>.
  /// </para>
  /// <para>
  /// <see cref="ParticleReferenceFrame.Local"/> means that the particles are relative to the scene 
  /// node. Multiple instance of the same particle system may be positioned and rendered within the 
  /// scene. Particles can be scaled, rotated, or moved by changing the 
  /// <see cref="SceneNode.ScaleLocal"/> or <see cref="SceneNode.PoseLocal"/> of the 
  /// <see cref="ParticleSystemNode"/>. The properties <see cref="SceneNode.ScaleLocal"/>, 
  /// <see cref="Color"/>, <see cref="Alpha"/>, and <see cref="AngleOffset"/> can be used to add 
  /// variety to the instances of a particle system.
  /// </para>
  /// <para>
  /// <strong>Bounding Shape:</strong><br/>
  /// The <see cref="Particles.ParticleSystem.Shape"/> of the <see cref="ParticleSystem"/> is used
  /// for frustum culling. The <see cref="Particles.ParticleSystem.Shape"/> is not currently not
  /// updated automatically. It is recommended to set a shape which is large enough to hold all
  /// particles. The <see cref="Particles.ParticleSystem.Shape"/> must not be changed while the
  /// graphics service is rendering the scene!
  /// </para>
  /// <para>
  /// <strong>Nested Particle Systems:</strong><br/>
  /// A particle system may include other particle systems. Only the root particle system needs to
  /// be added to the scene using a <see cref="ParticleSystemNode"/>. The 
  /// <see cref="BillboardRenderer"/> automatically renders all nested particle systems that have
  /// the required particle parameters. The <see cref="Particles.ParticleSystem.Shape"/> needs to be
  /// large enough to include all nested particle systems.
  /// </para>
  /// <para>
  /// <strong>Particle Parameters</strong><br/>
  /// The following particle parameters are read by the <see cref="BillboardRenderer"/>.
  /// </para>
  /// <para>
  /// <list type="table">
  /// <listheader>
  /// <term>Parameter Name</term><description>Description</description>
  /// </listheader>
  /// <item>
  /// <term><see cref="ParticleParameterNames.Alpha"/></term>
  /// <description>
  /// <para>
  /// The particle opacity (0 = transparent, 1 = opaque).
  /// </para>
  /// <para>
  /// Parameter type: varying or uniform, value type: <see cref="float"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.Angle"/></term>
  /// <description>
  /// <para>
  /// The rotation angle in radians.
  /// </para>
  /// <para>
  /// Parameter type: varying or uniform, value type: <see cref="float"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.AlphaTest"/></term>
  /// <description>
  /// <para>
  /// The reference value used in the alpha test. The reference value is a value in the range
  /// [0, 1]. If the alpha of a pixel is less than the reference alpha, the pixel is discarded.
  /// (Requires HiDef profile, not supported in Reach profile)
  /// </para>
  /// <para>
  /// Parameter type: uniform, value type: <see cref="float"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.AnimationTime"/></term>
  /// <description>
  /// <para>
  /// The normalized animation time where 0 marks the start of the animation and 1 marks the end 
  /// of the animation. Only relevant if the particle texture contains multiple animation frames. 
  /// The normalized animation time determines the current frame.
  /// </para>
  /// <para>
  /// Parameter type: varying or uniform, value type: <see cref="float"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.Axis"/></term>
  /// <description>
  /// <para>
  /// The axis vector of a particle.
  /// </para>
  /// <para>
  /// Parameter type: varying or uniform, value type: <see cref="Vector3F"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.BillboardOrientation"/></term>
  /// <description>
  /// <para>
  /// The billboard orientation of the particles. 
  /// </para>
  /// <para>
  /// Parameter type: uniform, value type: <see cref="BillboardOrientation"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.BlendMode"/></term>
  /// <description>
  /// <para>
  /// The blend mode of the particles where 0 = additive blending, 1 = alpha blending. Intermediate 
  /// values between 0 and 1 are allowed.
  /// </para>
  /// <para>
  /// Parameter type: varying or uniform, value type: <see cref="float"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.Color"/></term>
  /// <description>
  /// <para>
  /// The particle tint color.
  /// </para>
  /// <para>
  /// Parameter type: varying or uniform, value type: <see cref="Vector3F"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.DrawOrder"/></term>
  /// <description>
  /// <para>
  /// A value that defines the draw order for particle systems on the same world space position. 
  /// Particle systems with higher draw order are drawn on top of particle systems with lower draw 
  /// order. 
  /// </para>
  /// <para>
  /// Parameter type: uniform, value type: <see cref="int"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.IsDepthSorted"/></term>
  /// <description>
  /// <para>
  /// A parameter that defines if particles should be drawn back to front or oldest to newest. If 
  /// this parameter is <see langword="false"/> or absent, the particles are drawn from oldest to 
  /// newest.
  /// </para>
  /// <para>
  /// Parameter type: uniform, value type: <see cref="bool"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.Normal"/></term>
  /// <description>
  /// <para>
  /// The normal vector of a particle.
  /// </para>
  /// <para>
  /// Parameter type: varying or uniform, value type: <see cref="Vector3F"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.NormalizedAge"/></term>
  /// <description>
  /// <para>
  /// The normalized age in the range [0, 1]. This parameter is automatically created and managed by
  /// the <see cref="Particles.ParticleSystem"/> class. 
  /// </para>
  /// <para>
  /// Parameter type: varying, value type: <see cref="float"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.Position"/></term>
  /// <description>
  /// <para>
  /// The particle position.
  /// </para>
  /// <para>
  /// Parameter type: varying, value type: <see cref="Vector3F"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term>
  /// <see cref="ParticleParameterNames.Size"/>, 
  /// <see cref="ParticleParameterNames.SizeX"/>,
  /// <see cref="ParticleParameterNames.SizeY"/>
  /// </term>
  /// <description>
  /// <para>
  /// The particle size. The size can be defined using a single parameter 
  /// (<see cref="ParticleParameterNames.Size"/>), or using different parameters for width 
  /// (<see cref="ParticleParameterNames.SizeX"/>) and height 
  /// (<see cref="ParticleParameterNames.SizeY"/>).
  /// </para>
  /// <para>
  /// Parameter type: varying or uniform, value type: <see cref="float"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.Softness"/></term>
  /// <description>
  /// <para>
  /// The softness for rendering <i>soft particles</i>: Regular particles are rendered as flat 
  /// billboards, which create hard edges when they intersect with other geometry in the scene. 
  /// Soft particles have a volume and create soft transitions when they intersect with other 
  /// geometry.
  /// </para>
  /// <para>
  /// Parameter type: varying, value type: <see cref="float"/>
  /// </para>
  /// <para>
  /// 0 ... Disabled: The particles are rendered with hard edges.<br/>
  /// -1 or NaN ... Automatic: The thickness of the particle is determined automatically.<br/>
  /// &gt;1 ... Manual: The value defines the thickness of the particle (= soft particle distance 
  /// threshold).
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.Texture"/></term>
  /// <description>
  /// <para>
  /// The particle texture (using premultiplied alpha).
  /// </para>
  /// <para>
  /// Parameter type: uniform, value type: <see cref="PackedTexture"/> or 
  /// <see cref="Texture2D"/>
  /// </para>
  /// <para>
  /// This parameter is required!
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.TextureTiling"/></term>
  /// <description>
  /// <para>
  /// For ribbons: Defines how a texture is applied to a particle ribbon ("tiling distance").
  /// </para>
  /// <para>
  /// Parameter type: uniform, value type: <see cref="int"/>
  /// </para>
  /// <para>
  /// 0 ... No tiling: The texture is stretched along the ribbon.<br/>
  /// 1 ... Tiling: The texture is repeated every particle.<br/>
  /// <i>n</i> ... Tiling with lower frequency: The texture is repeated every <i>n</i> particles.<br/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ParticleParameterNames.Type"/></term>
  /// <description>
  /// <para>
  /// The type of the particles, which determines whether particles are rendered as individual 
  /// billboards or connected quad strips ("ribbons").
  /// </para>
  /// <para>
  /// Parameter type: uniform, value type: <see cref="ParticleType"/>
  /// </para>
  /// <para>
  /// This parameter is optional.
  /// </para>
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// <strong>Particle Ribbons (a.k.a. "Beams", "Lines", "Trails"):</strong><br/>
  /// When the particle system has a uniform particle parameter "Type" set to 
  /// <see cref="ParticleType.Ribbon"/> then subsequent living particles are connected and rendered 
  /// as ribbons (quad strips). At least two living particles are required to create a ribbon. Dead
  /// particles ("NormalizedAge" ≥ 1) can be used as delimiters to terminate one ribbon and start 
  /// the next ribbon.
  /// </para>
  /// <para>
  /// The "Position" parameter defines the points along the ribbon curve. The "Axis" parameter can 
  /// be set to define the orientation of the ribbon. The axis needs to be normal to the ribbon 
  /// curve (i.e. the axis does not point into the ribbon direction).
  /// </para>
  /// <para>
  /// The uniform "TextureTiling" parameter defines how the texture is applied to the ribbon. If the
  /// value is 0 the texture is stretched along the ribbon curve where the texture coordinate u = 0 
  /// is mapped to the start and u = 1 is mapped to the end of the ribbon. If the "TextureTiling" 
  /// value is 1 the texture is repeated at every particle (= point along the ribbon curve). If the 
  /// "TextureTiling" is <i>n</i>, where <i>n</i> &gt; 1, then the texture is repeated every 
  /// <i>n</i> particles.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// When a <see cref="ParticleSystemNode"/> is cloned the <see cref="ParticleSystem"/> is not 
  /// duplicated. The <see cref="ParticleSystem"/> is copied by reference (shallow copy). The 
  /// original <see cref="ParticleSystemNode"/> and the cloned instance will reference the same 
  /// <see cref="Particles.ParticleSystem"/> object.
  /// </para>
  /// </remarks>
  public class ParticleSystemNode : SceneNode
  {
    // To document:
    // ParticleSystem parameters should not be added/removed.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the particle system.
    /// </summary>
    /// <value>The particle system.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public ParticleSystem ParticleSystem
    {
      get { return _particleSystem; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _particleSystem = value;
        SynchronizeShape();
      }
    }
    private ParticleSystem _particleSystem;


    /// <summary>
    /// Gets or sets the tint color of the particle system instance.
    /// </summary>
    /// <value>The tint color (non-premultiplied). The default value is white (1, 1, 1).</value>
    public Vector3F Color { get; set; }


    /// <summary>
    /// Gets or sets the opacity (alpha) of the particle system instance.
    /// </summary>
    /// <value>The opacity (alpha). The default value is 1.</value>
    public float Alpha { get; set; }


    /// <summary>
    /// Gets or sets the rotation offset which is added to all particles.
    /// </summary>
    /// <value>The rotation offset in radians which is added to all particles.</value>
    /// <remarks>
    /// The offset can be used to add variety to particle system when it is instanced by multiple
    /// nodes.
    /// </remarks>
    public float AngleOffset { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystemNode" /> class.
    /// </summary>
    /// <param name="particleSystem">The particle system.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="particleSystem"/> is <see langword="null"/>.
    /// </exception>
    public ParticleSystemNode(ParticleSystem particleSystem)
    {
      if (particleSystem == null)
        throw new ArgumentNullException("particleSystem");

      IsRenderable = true;
      _particleSystem = particleSystem;
      Color = new Vector3F(1, 1, 1);
      Alpha = 1.0f;

      SynchronizeShape();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new ParticleSystemNode Clone()
    {
      return (ParticleSystemNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new ParticleSystemNode(ParticleSystem);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone ParticleSystemNode properties.
      var sourceTyped = (ParticleSystemNode)source;
      Color = sourceTyped.Color;
      Alpha = sourceTyped.Alpha;

      // Shape.Clone creates a shallow copy of Shape, but ParticleReferenceFrame.World requires a
      // deep copy. --> Reset and synchronize shape explicitly.
      Shape = Shape.Empty;
      SynchronizeShape();
    }
    #endregion


    /// <summary>
    /// Synchronizes the graphics data with the particle system data. (Needs to be called once per
    /// frame!)
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <remarks>
    /// This method needs to be called once per frame to synchronize the graphics service with the
    /// particle system service. It creates a snapshot of the particle system and converts the 
    /// particles to render data.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public void Synchronize(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      int frame = graphicsService.Frame + 1;

      SynchronizeShape();

      // ----- Synchronize pose.
      if (ParticleSystem.ReferenceFrame == ParticleReferenceFrame.World)
      {
        PoseWorld = ParticleSystem.Pose;

        // Note: Scale is ignored and not copied. Because we cannot simply set ScaleWorld. 
        // ParticleSystems with reference frame World should not use a scale.
      }

      // If the ReferenceFrame is Local, then ParticleSystem.Pose is irrelevant. 
      // (The particle system can be instanced. Particles are relative to the scene 
      // node.)

      // ----- Synchronize render data.
      // Render data is cached in ParticleSystem.RenderData.
      var renderData = ParticleSystem.RenderData as ParticleSystemData;
      if (renderData == null)
      {
        renderData = new ParticleSystemData(ParticleSystem);
        ParticleSystem.RenderData = renderData;
      }
      else if (renderData.Frame == frame)
      {
        // Render data is up-to-date.
        return;
      }
      else if (!ParticleSystem.Enabled && renderData.Frame == frame - 1)
      {
        // The particle system was updated in the last frame and the particles haven't 
        // changed since. (The particle system is disabled.)
        renderData.Frame = frame;
        return;
      }

      // Synchronize render data of root particle system.
      renderData.Update(ParticleSystem);

      // Synchronize render data of nested particle systems.
      if (ParticleSystem.Children != null && ParticleSystem.Children.Count > 0)
      {
        if (renderData.NestedRenderData == null)
          renderData.NestedRenderData = new List<ParticleSystemData>();
        else
          renderData.NestedRenderData.Clear();

        foreach (var childParticleSystem in ParticleSystem.Children)
          SynchronizeNested(renderData.NestedRenderData, childParticleSystem);
      }
      else
      {
        // Clear nested render data.
        renderData.NestedRenderData = null;
      }

      renderData.Frame = frame;
    }


    private void SynchronizeShape()
    {
      Debug.Assert(ParticleSystem != null);

      // Note: Scene does not allow TransformedShapes with infinite shapes.
      // --> Handle infinite shapes explicitly. (The code below only checks for InfiniteShape.
      // LineShape or PlaneShape will raise an exception in Scene!)

      if (ParticleSystem.ReferenceFrame == ParticleReferenceFrame.Local || ParticleSystem.Shape is InfiniteShape)
      {
        Shape = ParticleSystem.Shape;
      }
      else
      {
        // Particles are simulated in world space. ParticleSystem.Shape must not be scale by
        // SceneNode.ScaleWorld. --> Add a TransformedShape that negates the scale.
        var transformedShape = Shape as TransformedShape;
        if (transformedShape == null)
        {
          transformedShape = new TransformedShape(new GeometricObject());
          Shape = transformedShape;
        }

        var geometricObject = transformedShape.Child as GeometricObject;
        if (geometricObject == null)
        {
          geometricObject = new GeometricObject();
          transformedShape.Child = geometricObject;
        }

        geometricObject.Shape = ParticleSystem.Shape;
        geometricObject.Scale = Vector3F.One / ScaleWorld;
      }
    }


    private void SynchronizeNested(List<ParticleSystemData> nestedData, ParticleSystem particleSystem)
    {
      // Render data is cached in ParticleSystem.RenderData.
      var renderData = particleSystem.RenderData as ParticleSystemData;
      if (renderData == null)
      {
        renderData = new ParticleSystemData(particleSystem);
        particleSystem.RenderData = renderData;
      }

      // Note: renderData.Frame is set in root particle system. It is not necessary
      // to check the frame number for nested particle systems.

      renderData.Update(particleSystem);
      nestedData.Add(renderData);

      // Synchronize nested particle systems.
      if (particleSystem.Children != null)
        foreach (var child in particleSystem.Children)
          SynchronizeNested(nestedData, child);
    }
    #endregion
  }
}
#endif
