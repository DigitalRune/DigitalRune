// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Graphics.Interop;
#if !WP7
using DigitalRune.Graphics.PostProcessing;
#endif
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides information about the current render states.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="RenderContext"/> is passed to every Render method. It is used to pass 
  /// information to a renderer, and it should contain all information that is required to render 
  /// an object or to perform a rendering step.
  /// </para>
  /// <para>
  /// Additional information can be stored in the <see cref="Data"/> dictionary.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// The render context is cloneable. <see cref="Clone"/> makes a copy of the current render 
  /// context. The new instance contains a new <see cref="Data"/> dictionary. The properties and the
  /// contents of the <see cref="Data"/> dictionary are copied by reference (shallow copy).
  /// </para>
  /// </remarks>
  public class RenderContext
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    #region ----- General -----

    /// <summary>
    /// Gets or sets the graphics service.
    /// </summary>
    /// <value>The graphics service.</value>
    public IGraphicsService GraphicsService { get; private set; }


    /// <summary>
    /// Gets or sets the <see cref="PresentationTarget"/> that is currently being rendered. 
    /// </summary>
    /// <value>
    /// The <see cref="PresentationTarget"/>, or <see langword="null"/> if the current presentation
    /// target is the default XNA window.
    /// </value>
    public IPresentationTarget PresentationTarget { get; set; }


    /// <summary>
    /// Gets or sets the <see cref="GraphicsScreen"/> that is currently being rendered.
    /// </summary>
    /// <value>The <see cref="GraphicsScreen"/>.</value>
    public GraphicsScreen Screen { get; set; }


    /// <summary>
    /// Gets the total elapsed time.
    /// </summary>
    /// <value>The total elapsed time.</value>
    /// <inheritdoc cref="IGraphicsService.Time"/>
    public TimeSpan Time { get; set; }


    /// <summary>
    /// Gets the elapsed time since the last frame.
    /// </summary>
    /// <value>The elapsed time since the last frame.</value>
    /// <inheritdoc cref="IGraphicsService.DeltaTime"/>
    public TimeSpan DeltaTime { get; set; }


    /// <summary>
    /// Gets or sets the number of the current frame.
    /// </summary>
    /// <value>The number of the current frame.</value>
    /// <inheritdoc cref="IGraphicsService.Frame"/>
    public int Frame { get; set; }


    /// <summary>
    /// Gets or sets the current data object.
    /// </summary>
    /// <value>The current data object.</value>
    public object Object { get; set; }
    #endregion


    #region ----- Render State -----

    /// <summary>
    /// Gets or sets the source texture that contains the source image for the current render
    /// operation. 
    /// </summary>
    /// <value>
    /// The source texture; or <see langword="null"/> if there is no source texture.
    /// </value>
    /// <remarks>
    /// This property is used by <see cref="GraphicsScreen"/>s and <see cref="PostProcessor"/>s.
    /// The source texture is usually the content of the last render operation, e.g. the result
    /// of the last graphics screen or the last post-processor.
    /// </remarks>
    public Texture2D SourceTexture { get; set; }


    /// <summary>
    /// Gets or sets the target of the current rendering operations.
    /// </summary>
    /// <value>
    /// The target of the current rendering operations; or <see langword="null"/> if the device
    /// back buffer is the target.
    /// </value>
    public RenderTarget2D RenderTarget { get; set; }


    /// <summary>
    /// Gets the viewport (= the portion of the back buffer that should be used for rendering).
    /// </summary>
    /// <value>The viewport.</value>
    /// <remarks>
    /// <para>
    /// Usually, a graphics screen uses the full back buffer. But when the game is rendered into 
    /// a <see cref="IPresentationTarget"/>, it can happen that the back buffer is larger than the 
    /// current presentation target. The graphics screens and drawable objects should only use
    /// the portion of the back buffer that is specified by <see cref="Viewport"/>.
    /// </para>
    /// </remarks>
    public Viewport Viewport { get; set; }


    /// <summary>
    /// Gets or sets the texture that contains the rendered scene.
    /// </summary>
    /// <value>
    /// The scene texture; or <see langword="null"/> if there is no scene texture available.
    /// </value>
    /// <remarks>
    /// This property is usually <see langword="null"/>. However, in operations like off-screen
    /// rendering you need to combine an off-screen texture with the last scene texture. In this
    /// case <see cref="SourceTexture"/> will specify the off-screen texture and 
    /// <see cref="SceneTexture"/> will specify the last scene texture. 
    /// </remarks>
    public Texture2D SceneTexture { get; set; }
    #endregion


    #region ----- Deferred Rendering Buffers -----

    /// <summary>
    /// Gets or sets the first texture containing G-buffer data.
    /// </summary>
    /// <value>The first G-buffer texture.</value>
    public RenderTarget2D GBuffer0 { get; set; }

    /// <summary>
    /// Gets or sets the second texture containing G-buffer data.
    /// </summary>
    /// <value>The second G-buffer texture.</value>
    public RenderTarget2D GBuffer1 { get; set; }

    /// <summary>
    /// Gets or sets the third texture containing G-buffer data.
    /// </summary>
    /// <value>The third G-buffer texture.</value>
    public RenderTarget2D GBuffer2 { get; set; }

    /// <summary>
    /// Gets or sets the fourth texture containing G-buffer data.
    /// </summary>
    /// <value>The fourth G-buffer texture.</value>
    public RenderTarget2D GBuffer3 { get; set; }


    /// <summary>
    /// Gets or sets the first texture containing light buffer data.
    /// </summary>
    /// <value>The first light buffer texture.</value>
    public RenderTarget2D LightBuffer0 { get; set; }


    /// <summary>
    /// Gets or sets the first texture containing light buffer data.
    /// </summary>
    /// <value>The first light buffer texture.</value>
    public RenderTarget2D LightBuffer1 { get; set; }
    #endregion


    #region ----- Effect -----

    /// <summary>
    /// Gets or sets a string that identifies the current render pass.
    /// </summary>
    /// <value>The string that identifies the current render pass.</value>
    public string RenderPass
    {
      get { return _renderPass; }
      set
      {
        _renderPass = value;
        RenderPassHash = (value != null) ? value.GetHashCode() : 0;
      }
    }
    private string _renderPass;


    /// <summary>
    /// The cached hash value of <see cref="RenderPass"/>.
    /// </summary>
    internal int RenderPassHash;


    /// <summary>
    /// Gets or sets a string that identifies the current technique.
    /// </summary>
    /// <value>The string that identifies the current technique.</value>
    public string Technique { get; set; }


    /// <summary>
    /// Gets or sets the effect binding of the current material.
    /// </summary>
    /// <value>
    /// The effect binding of the current material; or <see langword="null"/> if no material is 
    /// currently used.
    /// </value>
    public EffectBinding MaterialBinding { get; set; }


    /// <summary>
    /// Gets or sets the effect binding of the current material instance.
    /// </summary>
    /// <value>
    /// The effect binding of the current material instance; or <see langword="null"/> if no 
    /// material is currently used.
    /// </value>
    public EffectBinding MaterialInstanceBinding { get; set; }


    ///// <summary>
    ///// Gets or sets the effect instance that is currently active.
    ///// </summary>
    ///// <value>The effect instance that is currently in use; or <see langword="null"/> if no effect 
    ///// instance is currently active.
    ///// </value>
    //public EffectInstance EffectInstance { get; set; }


    // TODO: Add code to ctor (techniqueIndex = -1) and reset.
    ///// <summary>
    ///// Gets or sets the effect that is currently active.
    ///// </summary>
    ///// <value>
    ///// The effect that is currently in use; or <see langword="null"/> if no effect is currently 
    ///// active.
    ///// </value>
    //public Effect Effect { get; set; }


    ///// <summary>
    ///// Gets or sets the effect technique that is currently active.
    ///// </summary>
    ///// <value>
    ///// The effect technique that is currently in use; or <see langword="null"/> if no effect is 
    ///// currently active.
    ///// </value>
    //public EffectTechnique Technique { get; set; }


    ///// <summary>
    ///// Gets or sets the index of the <see cref="EffectTechnique"/> that is currently active.
    ///// </summary>
    ///// <value>
    ///// The index of the <see cref="EffectTechnique"/> (-1 if we are not currently inside
    ///// <see cref="Microsoft.Xna.Framework.Graphics.Effect.Begin()"/>/
    ///// <see cref="Microsoft.Xna.Framework.Graphics.Effect.End"/> of an 
    ///// <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>.)
    ///// </value>
    //public int TechniqueIndex { get; set; }


    /// <summary>
    /// Gets or sets the index of the current <see cref="EffectPass"/>.
    /// </summary>
    /// <value>
    /// The index of the current <see cref="EffectPass"/>, or -1 if not applicable.
    /// </value>
    public int PassIndex { get; set; }  // Needed by the DefaultEffectBinder.
    #endregion


    #region ----- Scene -----

    /// <summary>
    /// Gets or sets the scene.
    /// </summary>
    /// <value>The scene.</value>
    public IScene Scene { get; set; }


    /// <summary>
    /// Gets or sets the active camera.
    /// </summary>
    /// <value>The active camera.</value>
    public CameraNode CameraNode { get; set; }


    /// <summary>
    /// Gets or sets the currently rendered scene node.
    /// </summary>
    /// <value>The currently rendered scene node.</value>
    public SceneNode SceneNode { get; set; }


    /// <summary>
    /// Gets or sets a scene node that provides additional context for the current render operation.
    /// </summary>
    /// <value>A scene node that provides additional information.</value>
    /// <remarks>
    /// <para>
    /// The purpose of the reference node depends on the current render operation. In most cases
    /// it will be <see langword="null"/>. Here are some examples where a reference node is useful:
    /// </para>
    /// <para>
    /// Shadow map rendering: When an object is rendered into the shadow map, the render context
    /// stores the currently rendered object in <see cref="SceneNode"/>. <see cref="ReferenceNode"/>
    /// contains the <see cref="LightNode"/> which owns the shadow map. This allows effect parameter
    /// bindings to find information about the light and the shadow.
    /// </para>
    /// <para>
    /// Render-to-texture: When an object is rendered into a texture of an 
    /// <see cref="RenderToTextureNode"/>, the render context stores the currently rendered object 
    /// in <see cref="SceneNode"/>. <see cref="ReferenceNode"/> contains the 
    /// <see cref="RenderToTextureNode"/>.
    /// </para>
    /// </remarks>
    public SceneNode ReferenceNode { get; set; }
    #endregion


    #region ----- Level of Detail (LOD) -----

    /// <summary>
    /// Gets or sets the global LOD bias.
    /// </summary>
    /// <value>The global LOD bias in the range [0, ∞[. The default value is 1.</value>
    /// <remarks>
    /// <para>
    /// The LOD bias is a factor that is multiplied to the distance of a scene node. It can be used 
    /// to increase or decrease the level of detail based on scene, performance, platform, or other 
    /// criteria.
    /// </para>
    /// <para>
    /// <strong>Performance Tips:</strong>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Increase the LOD bias during computationally intensive scenes (e.g. large number of 
    /// objects or characters on screen).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Increase the LOD bias of fast moving cameras.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Increase/decrease LOD bias based on the games quality settings (e.g. minimal details vs.
    /// maximal details).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Increase/decrease LOD bias based on platform (e.g. PC vs. mobile platforms).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Increase/decrease LOD bias based on screen resolution. (Note: The LOD metric 
    /// "view-normalized distance" does not account for resolution changes.)
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// A <see cref="LodBias"/> of 0 forces all objects to be rendered with the highest level of 
    /// detail. A large <see cref="LodBias"/>, such as <see cref="float.PositiveInfinity"/>, forces
    /// all objects to be drawn with the lowest level of detail.
    /// </para>
    /// </remarks>
    /// <seealso cref="SceneGraph.CameraNode.LodBias"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float LodBias
    {
      get { return _lodBias; }
      set
      {
        if (!(value >= 0))
          throw new ArgumentOutOfRangeException("value", "The LOD bias must be in the range [0, ∞[");

        _lodBias = value;
        ScaledLodHysteresis = _lodHysteresis * value;
      }
    }
    private float _lodBias;


    /// <summary>
    /// Gets or sets a value indicating whether smooth LOD transitions are enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to enable smooth LOD transitions; otherwise, <see langword="false"/>.
    /// The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When <see cref="LodBlendingEnabled"/> is <see langword="false"/> the renderer instantly 
    /// switches LODs, which can result in apparent "popping" of the geometry in the scene. The 
    /// property can be set to <see langword="true"/> to enable smooth transitions: The renderer 
    /// draws both LODs and blends them using screen-door transparency (stipple patterns).
    /// </para>
    /// <para>
    /// The length of the transition phase is determined by the <see cref="LodHysteresis"/>. If the
    /// LOD hysteresis is 0, blending is also disabled.
    /// </para>
    /// <para>
    /// Blending of LODs is expensive and increases the workload during LOD transitions. It is 
    /// therefore recommended to keep the LOD hysteresis small and to disable LOD blending during 
    /// computationally intensive scenes.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public bool LodBlendingEnabled { get; set; }


    /// <summary>
    /// Gets or sets the camera that is used as reference for LOD calculations.
    /// </summary>
    /// <value>
    /// The camera that is used as reference for LOD calculations. 
    /// </value>
    /// <remarks>
    /// <para>
    /// LOD selection depends on the current camera (field-of-view) and the distance of the object 
    /// to the camera. The <see cref="LodCameraNode"/> references the camera that is used for LOD 
    /// computations.
    /// </para>
    /// <para>
    /// In most cases the same camera is used for rendering as well as LOD calculations. In this 
    /// case the same <see cref="SceneGraph.CameraNode"/> instance needs to be assigned to 
    /// <see cref="CameraNode"/> and <see cref="LodCameraNode"/>. LOD calculations will fail if the
    /// <see cref="LodCameraNode"/> is not set.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public CameraNode LodCameraNode { get; set; }


    /// <summary>
    /// Gets or sets the LOD hysteresis, which is the distance over which an object transitions from
    /// on level of detail to the next level. (Needs to be normalized - see remarks.)
    /// </summary>
    /// <value>
    /// The LOD hysteresis. The value needs to be normalized - see remarks. The default value is 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// The <i>LOD hysteresis</i> introduces a lag into the LOD transitions. Instead of switching 
    /// between LODs at a certain threshold distance, the distance for switching to the lower LOD is
    /// further away than the threshold distance and the distance for switching to the higher LOD is
    /// closer.
    /// </para>
    /// <para>
    /// Example: The LOD distance for LOD2 is 100. With an LOD hysteresis of 10, the object 
    /// transitions from LOD1 to LOD2 at distance 105, and from LOD2 to LOD1 at distance 95.
    /// </para>
    /// <para>
    /// The LOD hysteresis can be set to avoid flickering when the camera is near a threshold 
    /// distance.
    /// </para>
    /// <para>
    /// The value stored in this property is a <i>view-normalized distance</i> as described here: 
    /// <see cref="GraphicsHelper.GetViewNormalizedDistance(SceneGraph.SceneNode,SceneGraph.CameraNode)"/>. 
    /// The method <see cref="GraphicsHelper.GetViewNormalizedDistance(float, Matrix44F)"/> can be 
    /// used to convert a distance to a view-normalized distance. The resulting value is independent
    /// of the current field-of-view.
    /// </para>
    /// <para>
    /// <strong>Tips:</strong>
    /// It is recommended to keep the LOD hysteresis tight: When LOD blending (see 
    /// <see cref="LodBlendingEnabled"/>) is set, the renderer has to render both LODs during 
    /// transitions and blend them using screen-door transparency (stipple patterns).
    /// </para>
    /// <para>
    /// In most games the transition range depends on the average speed of the camera. A fast moving
    /// player (e.g. in a racing game) requires a larger LOD hysteresis than a slow moving player
    /// (e.g. a first-person shooter).
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative, infinite or NaN.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float LodHysteresis
    {
      get { return _lodHysteresis; }
      set
      {
        if (!Numeric.IsZeroOrPositiveFinite(value))
          throw new ArgumentOutOfRangeException("value", "The LOD hysteresis must be 0 or a finite positive value.");

        _lodHysteresis = value;
        ScaledLodHysteresis = value * _lodBias;
      }
    }
    private float _lodHysteresis;

    // LOD hysteresis corrected by LOD bias.
    internal float ScaledLodHysteresis; 
    #endregion


    #region ----- Shadows -----

    // Obsolete: Only kept for backward compatibility.
    /// <summary>
    /// Gets or sets the distance of the shadow near plane.
    /// </summary>
    /// <value>The distance of the shadow near plane.</value>
    /// <remarks>
    /// <para>
    /// When rendering cascaded shadow maps and a <see cref="CascadedShadow.MinLightDistance"/> is 
    /// set, the shadow projection does not match the camera projection. The shadow projection is a
    /// tight projection around the cascade. But the camera projection has a greater depth to catch
    /// all occluders in front of the cascade. <see cref="ShadowNear"/> specifies the distances from
    /// the camera to the near plane of the shadow projection.
    /// </para>
    /// <para>
    /// The value is temporarily set by the <see cref="ShadowMapRenderer"/>.
    /// </para>
    /// </remarks>
    internal float ShadowNear { get; set; }
    #endregion
    

    #region ----- Misc -----

    /// <summary>
    /// Gets or sets a user-defined object.
    /// </summary>
    /// <value>The a user-defined object.</value>
    /// <remarks>
    /// <see cref="UserData"/> can be used to store user-defined data with the render context.
    /// Additionally, <see cref="Data"/> can be used to store more custom data that can be accessed 
    /// using a string key.
    /// </remarks>
    public object UserData { get; set; }


    /// <summary>
    /// Gets a generic collection of name/value pairs which can be used to store custom data.
    /// </summary>
    /// <value>
    /// A generic collection of name/value pairs which can be used to store custom data.
    /// </value>
    /// <remarks>
    /// <see cref="UserData"/> can be used to store user-defined data with the render context.
    /// Additionally, <see cref="Data"/> can be used to store more custom data that can be
    /// accessed using a string key.
    /// </remarks>
    /// <seealso cref="RenderContextKeys"/>
    public Dictionary<string, object> Data
    {
      get
      {
        if (_data == null)
          _data = new Dictionary<string, object>();

        return _data;
      }
    }
    private Dictionary<string, object> _data;
    #endregion

    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderContext"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public RenderContext(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      GraphicsService = graphicsService;
      Reset();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="RenderContext"/> that is a clone of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="RenderContext"/> that is a clone of the current instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// See class documentation of <see cref="RenderContext"/> (section "Cloning") for more 
    /// information.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="RenderContext"/> derived class and <see cref="CloneCore"/> to create a copy of
    /// the current instance. Classes that derive from <see cref="RenderContext"/> need to
    /// implement <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </para>
    /// </remarks>
    public RenderContext Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RenderContext"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method, 
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="RenderContext"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private RenderContext CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone RenderContext. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="RenderContext"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="RenderContext"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="RenderContext"/> derived class must 
    /// implement this method. A typical implementation is to simply call the default constructor 
    /// and return the result. 
    /// </para>
    /// </remarks>
    protected virtual RenderContext CreateInstanceCore()
    {
      return new RenderContext(GraphicsService);
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="RenderContext"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="RenderContext"/> derived class must 
    /// implement this method. A typical implementation is to call <c>base.CloneCore(this)</c> to 
    /// copy all properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(RenderContext source)
    {
      PresentationTarget = source.PresentationTarget;
      SourceTexture = source.SourceTexture;
      RenderTarget = source.RenderTarget;
      Viewport = source.Viewport;
      SceneTexture = source.SceneTexture;
      Screen = source.Screen;
      Time = source.Time;
      DeltaTime = source.DeltaTime;
      Frame = source.Frame;
      Object = source.Object;
      GBuffer0 = source.GBuffer0;
      GBuffer1 = source.GBuffer1;
      GBuffer2 = source.GBuffer2;
      GBuffer3 = source.GBuffer3;
      LightBuffer0 = source.LightBuffer0;
      LightBuffer1 = source.LightBuffer1;
      RenderPass = source.RenderPass;
      Technique = source.Technique;
      MaterialBinding = source.MaterialBinding;
      MaterialInstanceBinding = source.MaterialInstanceBinding;
      PassIndex = source.PassIndex;
      Scene = source.Scene;
      CameraNode = source.CameraNode;
      SceneNode = source.SceneNode;
      ReferenceNode = source.ReferenceNode;
      LodBias = source.LodBias;
      LodBlendingEnabled = source.LodBlendingEnabled;
      LodCameraNode = source.LodCameraNode;
      LodHysteresis = source.LodHysteresis;
      ShadowNear = source.ShadowNear;
      UserData = source.UserData;

      // Copy content of Data dictionary. 
      if (_data != null)
        _data.Clear();

      if (source._data != null)
      {
        if (_data == null)
          _data = new Dictionary<string, object>(source._data.Count);

        foreach (var entry in source._data)
          _data.Add(entry.Key, entry.Value);
      }
    }
    #endregion


    /// <summary>
    /// Resets the render context to default values.
    /// </summary>
    public void Reset()
    {
      PresentationTarget = null;
      SourceTexture = null;
      RenderTarget = null;
      Viewport = new Viewport(0, 0, 800, 600);
      SceneTexture = null;
      Screen = null;
      Time = TimeSpan.Zero;
      DeltaTime = TimeSpan.Zero;
      Frame = -1;
      Object = null;
      GBuffer0 = null;
      GBuffer1 = null;
      GBuffer2 = null;
      GBuffer3 = null;
      LightBuffer0 = null;
      LightBuffer1 = null;
      RenderPass = null;
      Technique = null;
      MaterialBinding = null;
      MaterialInstanceBinding = null;
      PassIndex = -1;
      Scene = null;
      CameraNode = null;
      SceneNode = null;
      ReferenceNode = null;
      LodBias = 1;
      LodBlendingEnabled = false;
      LodCameraNode = null;
      LodHysteresis = 0;
      ShadowNear = float.NaN;
      UserData = null;

      if (_data != null)
        _data.Clear();
    }


    /// <summary>
    /// Copies the properties of the specified render context.
    /// </summary>
    /// <param name="source">The render context from properties are copied.</param>
    /// <remarks>
    /// <para>
    /// All properties and the content of the <see cref="Data"/> dictionary are copied from 
    /// <paramref name="source"/> to this instance by reference (shallow copy).
    /// </para>
    /// <para>
    /// The <see cref="Set"/> method internally calls <see cref="CloneCore"/>, which can be 
    /// overridden in derived classes.
    /// </para>
    /// </remarks>
    public void Set(RenderContext source)
    {
      CloneCore(source);
    }
    #endregion
  }
}
