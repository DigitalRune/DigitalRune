// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.Interop;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides the base class for graphics screens, which implement the rendering pipeline and draw 
  /// game content.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A graphics screen represents a layer in a 3D application. Multiple screens might be stacked on 
  /// one another, for example: 
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// The back (first) screen renders a 3D world.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// In front of the first screen is another screen that renders the HUD.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// In front of these layers is a graphics screen that renders a GUI. For example an 
  /// "Options Dialog".
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// To display a graphics screen it must be added to a <see cref="IGraphicsService"/> (see 
  /// property <see cref="IGraphicsService.Screens"/>). The graphics service renders screens back to
  /// front.
  /// </para>
  /// <para>
  /// Each class that derives from <see cref="GraphicsScreen"/> can override the methods 
  /// <see cref="OnUpdate"/> and <see cref="OnRender"/>. <see cref="OnUpdate"/> is usually called 
  /// once per frame and the screen can update its internal state in this method. 
  /// <see cref="OnUpdate"/> is called by the <see cref="GraphicsManager"/> in its 
  /// <see cref="GraphicsManager.Update"/> method. 
  /// </para>
  /// <para>
  /// Each screen implements its own rendering pipeline by overriding <see cref="OnRender"/>. That 
  /// means, a screen that renders a 3D world can implement a different render pipeline than a 
  /// screen that draws the HUD or a GUI on top. Each graphics screen can use its own type of scene 
  /// management. <see cref="OnRender"/> is called by the <see cref="GraphicsManager"/> when one of 
  /// its <strong>Render</strong>-methods is called. Special notes: If a screen is fully covered by 
  /// another screen, the graphics service might not call <see cref="OnRender"/>. On the other hand, 
  /// if the application has several <see cref="IPresentationTarget"/>s, it can happen that 
  /// <see cref="OnRender"/> is called several times per frame. 
  /// </para>
  /// <para>
  /// <see cref="OnUpdate"/> and <see cref="OnRender"/> are usually not called in a frame if
  /// the screen is invisible (e.g. if <see cref="IsVisible"/> is <see langword="false"/> or
  /// if the screen is totally covered by another screen).
  /// </para>
  /// <para>
  /// The property <see cref="Coverage"/> indicates whether a screen covers the entire view or 
  /// whether a screen is partially transparent. The property needs to be set by each graphics
  /// screen depending on the content which is going to be rendered. The graphics service reads the 
  /// property to determine which screens need to be rendered. 
  /// </para>
  /// </remarks>
  /// <seealso cref="GraphicsScreenCollection"/>
  /// <seealso cref="GraphicsManager"/>
  public abstract class GraphicsScreen : INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private int _lastFrame = -1;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the graphics service.
    /// </summary>
    /// <value>The graphics service.</value>
    public IGraphicsService GraphicsService { get; private set; }


    /// <summary>
    /// Gets or sets the name of this graphics screen.
    /// </summary>
    /// <value>
    /// The name of the graphics screen. The default value is <see langword="null"/>.
    /// </value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the screen covers the entire view or only a part of 
    /// it.
    /// </summary>
    /// <value>
    /// The <see cref="GraphicsScreenCoverage"/>. When not sure what to return, use
    /// <see cref="GraphicsScreenCoverage.Partial"/> (default value).
    /// </value>
    /// <remarks>
    /// This property is a hint that indicates whether other <see cref="GraphicsScreen"/>s that lie 
    /// in the background need to be rendered. When <see cref="Coverage"/> is set to 
    /// <see cref="GraphicsScreenCoverage.Partial"/> screens in the background are rendered first.
    /// </remarks>
    public GraphicsScreenCoverage Coverage { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this instance is visible.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is visible; otherwise, <see langword="false"/>.
    /// The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// If <see cref="IsVisible"/> is false, <see cref="OnUpdate"/> and <see cref="OnRender"/>
    /// are not called.
    /// </remarks>
    public bool IsVisible { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the previous graphics screens should render into an
    /// off-screen render target.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if graphics screens below this screen should render into an
    /// off-screen render target; otherwise, <see langword="false"/> to render into the back buffer. 
    /// The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Usually, each graphics screen renders into a back buffer, but in some cases a graphics 
    /// screen wants to further process the result of the previous graphics screens. This can be 
    /// used, for example, to apply post-processing effects to the result of the graphics screens.
    /// In this case <see cref="RenderPreviousScreensToTexture"/> is set to <see langword="true"/>.
    /// <see cref="SourceTextureFormat"/> defines the format of the off-screen render target.
    /// The result of the previous screens is available as the 
    /// <see cref="RenderContext.SourceTexture"/> in the <see cref="RenderContext"/>.
    /// </remarks>
    public bool RenderPreviousScreensToTexture { get; set; }


    /// <summary>
    /// Gets or sets the source texture format.
    /// </summary>
    /// <value>
    /// The source texture format, or <see langword="null"/> to use the same format as the device 
    /// back buffer. The default value is <see langword="null"/>.
    /// </value>
    public RenderTargetFormat SourceTextureFormat { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsScreen"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    protected GraphicsScreen(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      GraphicsService = graphicsService;
      Coverage = GraphicsScreenCoverage.Partial;
      IsVisible = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Updates the state of the graphics screen.
    /// </summary>
    /// <param name="deltaTime">The time that has elapsed since the last update.</param>
    /// <remarks>
    /// <para>
    /// A graphics screen can update its internal state in this method.
    /// </para>
    /// <para>
    /// This method is called automatically by the <see cref="GraphicsManager"/> once per frame
    /// before the screen needs to be rendered. The screens are updated in front-to-back order as
    /// they are registered in the <see cref="GraphicsManager"/>. Only visible screens are updated.
    /// That means, if a screen is hidden by another screen in front of it, it is not updated. The
    /// <see cref="GraphicsManager"/> checks the <see cref="Coverage"/> property of the foreground
    /// screens to determine whether a screen in the background is visible.
    /// </para>
    /// </remarks>
    public void Update(TimeSpan deltaTime)
    {
      if (!IsVisible)
        return;

      _lastFrame = GraphicsService.Frame;
      OnUpdate(deltaTime);
    }


    /// <summary>
    /// Called when <see cref="GraphicsScreen.Update"/> is called.
    /// </summary>
    /// <param name="deltaTime">The time that has elapsed since the last update.</param>
    /// <remarks>
    /// This method is automatically called by <see cref="GraphicsScreen.Update"/>, but only if the
    /// screen is visible (see <see cref="GraphicsScreen.IsVisible"/>). Derived classes can override
    /// this method to update the state of the screen, for example, to advance
    /// time dependent animations.
    /// </remarks>
    protected abstract void OnUpdate(TimeSpan deltaTime);


    /// <summary>
    /// Renders the graphics screen.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <para>
    /// This method implements the rendering pipeline of the graphics screen.
    /// </para>
    /// <para>
    /// It is called automatically by the <see cref="GraphicsManager"/> to render the screen. The
    /// screens are rendered in back-to-front order. That means the screens in the background are
    /// rendered first. The <see cref="GraphicsManager"/> checks the <see cref="Coverage"/>
    /// property of the foreground screens to determine whether a screen in the background needs to
    /// be rendered.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void Render(RenderContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");
      if (GraphicsService != context.GraphicsService)
        throw new GraphicsException("The graphics screen belongs to a different graphics service.");
      if (!IsVisible)
        return;

      // Only GraphicsScreens registered in the GraphicsManager are updated explicitly.
      // GraphicsScreens that are invisible or obscured by other GraphicsScreens during
      // GraphicsManager.Update() are ignored.
      // When using multiple presentation targets, some GraphicsScreens might not be
      // up to date.
      if (_lastFrame != GraphicsService.Frame)
      {
        _lastFrame = GraphicsService.Frame;
        OnUpdate(GraphicsService.DeltaTime);
      }

      context.Screen = this;
      OnRender(context);
      context.Screen = null;
    }


    /// <summary>
    /// Called when <see cref="GraphicsScreen.Render"/> is called.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <para>
    /// This method implements the rendering pipeline of the graphics screen.
    /// </para>
    /// <para>
    /// This method is automatically called by <see cref="GraphicsScreen.Render"/>, but only if the
    /// screen is visible (see <see cref="GraphicsScreen.IsVisible"/>). Derived classes can override
    /// this method to draw game content. <paramref name="context"/> is guaranteed to be not
    /// <see langword="null"/>.
    /// </para>
    /// <para>
    /// If the previous graphics screen has been rendered into an off-screen render target (see
    /// property <see cref="GraphicsScreen.RenderPreviousScreensToTexture"/>), then the results is
    /// available as the <see cref="RenderContext.SourceTexture"/> in the
    /// <see cref="RenderContext"/>.
    /// </para>
    /// </remarks>
    protected abstract void OnRender(RenderContext context);
    #endregion
  }
}
