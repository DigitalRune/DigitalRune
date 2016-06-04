// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines the direction and speed of water flow.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class can be used to make the water of a <see cref="WaterNode"/> flow in user-defined
  /// directions and with user-defined speed.
  /// </para>
  /// <para>
  /// <strong>Defining Flow:</strong><br/>
  /// Water flow is defined by three influences:
  /// <list type="bullet">
  /// <item>
  /// <see cref="BaseVelocity"/> defines a uniform constant flow which is applied to the whole
  /// water surface.
  /// </item>
  /// <item>
  /// Water flow can also be defined by surface slope of the water surface. The slope is defined by 
  /// the vertex normals of the water <see cref="WaterNode.Volume"/>. Water is usually running 
  /// downhill. <see cref="SurfaceSlopeSpeed"/> defines the speed at which water flows down an
  /// inclined water surface.
  /// </item>
  /// <item>
  /// A <see cref="FlowMap"/> texture can be used to define a detailed, complex flow pattern.
  /// <see cref="FlowMapSpeed"/> scales the speed defined by the flow map.
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// <strong>Speed and Normal Strength Limits:</strong><br/>
  /// Flow speed from all three influences (base velocity, surface slope and flow map) is summed up
  /// and clamped to <see cref="MaxSpeed"/>. The intensity of normal maps should be reduced if the
  /// water moves faster. The normal map strength have full strength when the flow speed is
  /// zero. The normal map strength is reduced to <see cref="MinStrength"/> when the flow speed
  /// reaches its maximum (<see cref="MaxSpeed"/>).
  /// </para>
  /// <para>
  /// <strong>Reference frame:</strong><br/>
  /// The <see cref="BaseVelocity"/> is relative to the local space of the <see cref="WaterNode"/>.
  /// The <see cref="FlowMap"/> is also relative to the <see cref="WaterNode"/>; it is aligned with
  /// the local axes of the node. It fills the local AABB, and the map is always projected top-down.
  /// </para>
  /// <para>
  /// <strong>Rendering Flow:</strong><br/>
  /// Water flow is created by scrolling the normal maps of the water into the flow direction. If
  /// two points on the water surface have a different flow direction or flow speed, then this
  /// creates distortion. To hide the distortions, two layers of normal maps are alternated:
  /// One normal map layer fades in, is scrolled and before it gets too distorted it fades out. At
  /// the same time a second normal map layer is added but with a half cycle offset.
  /// <see cref="CycleDuration"/> defines the cycle period. The normal map cycling can cause pulsing
  /// artifacts. Noise can be used to hide this artifact, by giving each point on the water surface
  /// a different cycle offset. <see cref="NoiseMapScale"/> defines the scale of the applied noise
  /// texture in x and z direction. <see cref="NoiseMapStrength"/> defines how much noise is applied
  /// to each pixel (= how much a cycle can be offset). The noise map itself is always generated
  /// automatically.
  /// </para>
  /// <para>
  /// <strong>Limitations:</strong> Currently, the <see cref="WaterRenderer"/> does not render
  /// <see cref="WaterFlow"/> when the <see cref="WaterNode"/> also uses <see cref="WaterWaves"/>.
  /// </para>
  /// </remarks>
  public class WaterFlow : IDisposable
  {
    // TODO: 
    // - Implement cloning?
    // - Currently, all flow is in local space. We could add a property ReferenceFrame = World/Local,
    //   UseWorldSpace, etc. to allow locally fixed flow too. E.g. to allow flow to rotate when
    //   the model rotates.
    // - FlowMap could be a packed texture.
    // - Flow could also be specified in vertex attributes.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the uniform, constant velocity applied to the whole water surface.
    /// </summary>
    /// <value>
    /// The uniform, constant velocity applied to the whole water surface.
    /// The default value is (0, 0, 0).
    /// </value>
    /// <remarks>
    /// The y component of the velocity is usually irrelevant because water flows only horizontally
    /// across the water surface.
    /// </remarks>
    public Vector3F BaseVelocity { get; set; }


    /// <summary>
    /// Gets or sets the scale factor for surface-slope-based flow.
    /// </summary>
    /// <value>The scale factor for surface-slope-based flow.</value>
    /// <remarks>
    /// Vertex normals of the water <see cref="WaterNode.Volume"/> can be used to define flow.
    /// Water is always flowing downhill. <see cref="SurfaceSlopeSpeed"/> defines the speed at which
    /// water flows down an inclined water surface.
    /// </remarks>
    public float SurfaceSlopeSpeed { get; set; }


    /// <summary>
    /// Gets or sets the texture which defines the flow direction and speed.
    /// </summary>
    /// <value>The texture which defines the flow direction and speed.</value>
    /// <remarks>
    /// <para>
    /// The RG channels stores horizontal flow direction. The B channel stores the
    /// flow speed. The water pixel shader will apply the flow map flow like this:
    /// </para>
    /// <para>
    /// Given a flow map value f = (r, g, b), the 3D flow velocity is 
    ///   <c>flowVelocity.xz = (f.rg * 2 - 1) * f.b * FlowMapSpeed</c>.
    /// (The component flowVelocity.y is always 0.)
    /// </para>
    /// <para>
    /// The <see cref="FlowMap"/> is also relative to the <see cref="WaterNode"/>; it is aligned
    /// with the local axes of the node. It fills the local AABB, and the map is always projected
    /// top-down.
    /// </para>
    /// </remarks>
    public Texture2D FlowMap { get; set; }


    /// <summary>
    /// Gets or sets the scale factor for flow-map-based flow.
    /// </summary>
    /// <value>The scale factor for flow-map-based flow.</value>
    public float FlowMapSpeed { get; set; }


    /// <summary>
    /// Gets or sets the cycle duration in seconds of a normal map layer.
    /// </summary>
    /// <value>The cycle duration in seconds of a normal map layer</value>
    /// <remarks>
    /// <para>
    /// Flow animation is usually created by moving a normal map layer over the water plane. If
    /// different pixels move the water plane in different directions, the normal map gets
    /// distorted. Therefore, the animation has to alternate between two normal map layers. Each
    /// normal map layer is moved and reset while the other normal map is visible.
    /// <see cref="CycleDuration"/> defines how long each normal map layer is shown before it is
    /// reset. Too short cycle times can create an unwanted "pulsing" effect. Too long cycle times
    /// can create unwanted visible distortions.
    /// </para>
    /// </remarks>
    public float CycleDuration { get; set; }


    /// <summary>
    /// Gets or sets the world space scale of the noise map.
    /// </summary>
    /// <value>The world space scale of the noise map.</value>
    /// <remarks>
    /// Noise can be used to remove unwanted visual artifacts of flow rendering. A noise value is
    /// applied to each surface pixel. <see cref="NoiseMapScale"/> defines the horizontal frequency
    /// of the noise. Increasing <see cref="NoiseMapScale"/> stretches the noise, reducing the
    /// frequency.
    /// </remarks>
    public float NoiseMapScale { get; set; }


    /// <summary>
    /// Gets or sets the noise strength.
    /// </summary>
    /// <value>The noise strength.</value>
    /// <remarks>
    /// Noise can be used to remove unwanted visual artifacts of flow rendering. A noise value is
    /// applied to each surface pixel. <see cref="NoiseMapStrength"/> defines how much noise is
    /// applied per pixel.
    /// </remarks>
    public float NoiseMapStrength { get; set; }


    /// <summary>
    /// Gets or sets the maximal speed limit.
    /// </summary>
    /// <value>The maximal speed limit.</value>
    /// <remarks>
    /// Flow speed from all three influences (base velocity, surface slope and flow map) is summed
    /// up and clamped to <see cref="MaxSpeed"/>.
    /// </remarks>
    public float MaxSpeed { get; set; }


    /// <summary>
    /// Gets or sets the minimum strength of the normal maps.
    /// </summary>
    /// <value>The minimum strength of the normal maps.</value>
    /// <remarks>
    /// The intensity of normal maps should be reduced if the water moves faster. The normal maps
    /// have full strength when the flow speed is zero. The normal map strength is reduced
    /// to <see cref="MinStrength"/> when the flow speed reaches its maximum (<see cref="MaxSpeed"/>).
    /// </remarks>
    public float MinStrength { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="WaterFlow"/> class.
    /// </summary>
    public WaterFlow()
    {
      BaseVelocity = new Vector3F(0, 0, 0);
      SurfaceSlopeSpeed = 0;
      FlowMapSpeed = 0;
      CycleDuration = 2;
      NoiseMapScale = 1;
      NoiseMapStrength = 0.2f;
      MaxSpeed = 2;
      MinStrength = 0.5f;
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="WaterFlow"/> class.
    /// </summary>
    /// <remarks>
    /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
    /// <see langword="true"/>, and then suppresses finalization of the instance.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="WaterFlow"/> class 
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        FlowMap.SafeDispose();
        FlowMap = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
