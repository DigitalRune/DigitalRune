// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Manages and draws <see cref="UIControl"/>s and <see cref="Window"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A screen is the root of a tree of <see cref="UIControl"/>s. Controls can be added to the 
  /// screen using the <see cref="Children"/> collection. The controls can be positioned using
  /// <see cref="UIControl.X"/>, <see cref="UIControl.Y"/> like in a canvas, or using alignment and
  /// margins as usual.
  /// </para>
  /// <para>
  /// The screen starts all HandleInput/Measure/Arrange/Render traversals of the visual tree of
  /// <see cref="UIControl"/>s. 
  /// </para>
  /// <para>
  /// Screens are not drawn automatically. <see cref="Draw(TimeSpan)"/> must be called manually to 
  /// draw the screen and all contained controls. If <see cref="UIControl.Width"/> and 
  /// <see cref="UIControl.Height"/> are not set, the screen fills the whole viewport. The screen
  /// does not limit the layout size of the child controls. - Children can be positions outside the
  /// screen area.
  /// </para>
  /// <para>
  /// Per default, screens are not focus scopes (see <see cref="FocusManager"/>) because usually 
  /// the 3D scene is drawn behind the screen and the screen should not "absorb" arrow key input or
  /// thumbstick/direction pad input.
  /// </para>
  /// </remarks>
  public class UIScreen : UIControl
#if !SILVERLIGHT
    , IDrawable
#endif
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // We reuse the InputContext and RenderContext instances between frames.
    private readonly InputContext _inputContext = new InputContext();
    private readonly UIRenderContext _renderContext = new UIRenderContext();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="IUIService"/>.
    /// </summary>
    /// <value>The <see cref="IUIService"/>.</value>
    /// <remarks>
    /// This property is automatically set when the screen is added to the 
    /// <see cref="IUIService.Screens"/> collection of a <see cref="IUIService"/>.
    /// </remarks>
    public new IUIService UIService { get; internal set; }


    /// <summary>
    /// Gets the renderer that defines the styles and renders controls for this screen.
    /// </summary>
    /// <value>The renderer that defines the styles and renders controls for this screen.</value>
    public IUIRenderer Renderer { get; private set; }


    /// <summary>
    /// Gets the children.
    /// </summary>
    /// <value>The children.</value>
    public NotifyingCollection<UIControl> Children { get; private set; }


    /// <summary>
    /// Gets the control under mouse cursor.
    /// </summary>
    /// <value>The control under mouse cursor.</value>
    public UIControl ControlUnderMouse { get; internal set; }


    /// <summary>
    /// Gets or sets a value indicating whether input is enabled for this screen.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if input is enabled for this screen; otherwise, 
    /// <see langword="false"/> if the screen and all child controls ignore input.
    /// </value>
    public bool InputEnabled { get; set; }


    /// <summary>
    /// Gets or sets the <see cref="FocusManager"/>.
    /// </summary>
    /// <value>
    /// The <see cref="FocusManager"/>. Can be set to another <see cref="FocusManager"/> but not to 
    /// <see langword="null"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public FocusManager FocusManager
    {
      get { return _focusManager; }
      set
      {
        if (_focusManager == value)
          return;

        if (value == null)
          throw new ArgumentNullException("value");

        // Store focus.
        var focusedControl = _focusManager.FocusedControl;
        _focusManager.ClearFocus();

        _focusManager = value;

        // Restore previous focus.
        _focusManager.Focus(focusedControl);
      }
    }
    private FocusManager _focusManager;


    /// <summary>
    /// Gets the <see cref="ToolTipManager"/>.
    /// </summary>
    /// <value>The <see cref="ToolTipManager"/>.</value>
    public ToolTipManager ToolTipManager { get; private set; }


#if !SILVERLIGHT
    /// <summary>
    /// Returns the same value as <see cref="UIControl.IsVisible"/>.
    /// </summary>
    /// <value>
    /// The same value as <see cref="UIControl.IsVisible"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IDrawable.Visible
    {
      get { return IsVisible; }
    }
#endif


    /// <summary>
    /// Gets or sets the z-index that determines the draw order of all 
    /// <see cref="IUIService.Screens"/> of the same <see cref="IUIService"/>
    /// </summary>
    /// <value>
    /// The z-index that determines the draw order of all <see cref="IUIService.Screens"/> of the 
    /// same <see cref="IUIService"/>.
    /// </value>
    /// <remarks>
    /// Lower values mean the screen is drawn first, behind other screens. Screens are updated in
    /// the reverse order: The screen with the highest value is updated first because it covers the
    /// other screens.
    /// </remarks>
    public int ZIndex
    {
      get { return _drawOrder; }
      set
      {
        if (_drawOrder == value)
          return;

        _drawOrder = value;
#if !SILVERLIGHT
        OnDrawOrderChanged(EventArgs.Empty);
#endif
      }
    }
    private int _drawOrder;


#if !SILVERLIGHT
    /// <summary>
    /// Returns the same value as <see cref="ZIndex"/>.
    /// </summary>
    /// <value>
    /// The same value as <see cref="ZIndex"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    int IDrawable.DrawOrder
    {
      get { return _drawOrder; }
    }


    /// <summary>
    /// Event raised after the <see cref="IDrawable.Visible"/> property value has changed.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    event EventHandler<EventArgs> IDrawable.VisibleChanged
    {
      add { VisibleChanged += value; }
      remove { VisibleChanged -= value; }
    }
    private event EventHandler<EventArgs> VisibleChanged;


    /// <summary>
    /// Event raised after the <see cref="IDrawable.DrawOrder"/> property value has changed.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    event EventHandler<EventArgs> IDrawable.DrawOrderChanged
    {
      add { ZIndexChanged += value; }
      remove { ZIndexChanged -= value; }
    }
    private event EventHandler<EventArgs> ZIndexChanged;
#endif
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="ToolTipDelay"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ToolTipDelayPropertyId = CreateProperty(
      typeof(UIScreen), "ToolTipDelay", GamePropertyCategories.Default, null, 
      TimeSpan.FromMilliseconds(500), UIPropertyOptions.None);

    /// <summary>
    /// Gets the time which the mouse has to stand still before a tool tip pops up. 
    /// This is a game object property.
    /// </summary>
    /// <value>The time which the mouse has to stand still before a tool tip pops up.</value>
    public TimeSpan ToolTipDelay
    {
      get { return GetValue<TimeSpan>(ToolTipDelayPropertyId); }
      set { SetValue(ToolTipDelayPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="ToolTipOffset"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ToolTipOffsetPropertyId = CreateProperty(
      typeof(UIScreen), "ToolTipOffset", GamePropertyCategories.Default, null, 20f,
      UIPropertyOptions.None);

    /// <summary>
    /// Gets the offset of the tool tip to the mouse position. 
    /// This is a game object property.
    /// </summary>
    /// <value>The offset of the tool tip to the mouse position.</value>
    public float ToolTipOffset
    {
      get { return GetValue<float>(ToolTipOffsetPropertyId); }
      set { SetValue(ToolTipOffsetPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="MouseWheelScrollDelta"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int MouseWheelScrollDeltaPropertyId = CreateProperty(
      typeof(UIScreen), "MouseWheelScrollDelta", GamePropertyCategories.Behavior, null,
      PlatformHelper.MouseWheelScrollDelta,
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the amount of the delta value of a single mouse wheel rotation increment. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The amount of the delta value of a single mouse wheel rotation increment.
    /// </value>
    public int MouseWheelScrollDelta
    {
      get { return GetValue<int>(MouseWheelScrollDeltaPropertyId); }
      set { SetValue(MouseWheelScrollDeltaPropertyId, value); }
    }

    /// <summary> 
    /// The ID of the <see cref="MouseWheelScrollLines"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int MouseWheelScrollLinesPropertyId = CreateProperty(
      typeof(UIScreen), "MouseWheelScrollLines", GamePropertyCategories.Behavior, null,
      PlatformHelper.MouseWheelScrollLines,
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the number of lines to scroll when the mouse wheel is rotated. 
    /// This is a game object property.
    /// </summary>
    /// <value>The number of lines to scroll when the mouse wheel is rotated.</value>
    public int MouseWheelScrollLines
    {
      get { return GetValue<int>(MouseWheelScrollLinesPropertyId); }
      set { SetValue(MouseWheelScrollLinesPropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="UIScreen"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static UIScreen()
    {
      // Screens do auto-unfocus because the screen area usually shows the game graphics and
      // when the user clicks the game it should remove the focus.
      OverrideDefaultValue(typeof(UIScreen), AutoUnfocusPropertyId, true);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="UIScreen"/> class.
    /// </summary>
    /// <param name="name">The name of the screen.</param>
    /// <param name="renderer">
    /// The renderer that defines the styles and visual appearance for controls in this screen. 
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or <paramref name="renderer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is an empty string.
    /// </exception>
    public UIScreen(string name, IUIRenderer renderer)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("String is empty.", "name");
      if (renderer == null)
        throw new ArgumentNullException("renderer");

      Name = name;
      Renderer = renderer;

      Style = "UIScreen";

      Children = new NotifyingCollection<UIControl>(false, false);
      Children.CollectionChanged += OnChildrenChanged;

      _focusManager = new FocusManager(this);
      ToolTipManager = new ToolTipManager(this);

#if !SILVERLIGHT
      // Call OnVisibleChanged when IsVisible changes.
      var isVisible = Properties.Get<bool>(IsVisiblePropertyId);
      isVisible.Changed += (s, e) => OnVisibleChanged(EventArgs.Empty);
#endif

      InputEnabled = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnChildrenChanged(object sender, CollectionChangedEventArgs<UIControl> eventArgs)
    {
      // Children are also stuffed into the VisualChildren. Both collections should use 
      // the same order. 

      // Handle moved items.
      if (eventArgs.Action == CollectionChangedAction.Move)
      {
        // Move visual children too.
        VisualChildren.Move(eventArgs.OldItemsIndex, eventArgs.NewItemsIndex);
        return;
      }

      // Handle removed items.
      foreach (var oldItem in eventArgs.OldItems)
        VisualChildren.Remove(oldItem);

      // Handle new items.
      int newItemsIndex = eventArgs.NewItemsIndex;
      if (newItemsIndex == -1)
      {
        // Append items.
        foreach (var newItem in eventArgs.NewItems)
          VisualChildren.Add(newItem);
      }
      else
      {
        // Make sure that the same order is used in both collections.
        foreach (var newItem in eventArgs.NewItems)
        {
          VisualChildren.Insert(newItemsIndex, newItem);
          newItemsIndex++;
        }
      }

      InvalidateMeasure();
    }


    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      if (!IsEnabled || !IsVisible)
        return;

      // Make sure the layout is up-to-date.
      UpdateLayout();

      // Mouse was handled somewhere else? Probably the screen is covered by another screen.
      var inputService = InputService;
      if (inputService.IsMouseOrTouchHandled)
        ControlUnderMouse = null;

      // Set up input context.
      _inputContext.DeltaTime = deltaTime;
      _inputContext.ScreenMousePosition = inputService.MousePosition;
      _inputContext.ScreenMousePositionDelta = inputService.MousePositionDelta;
      _inputContext.MousePosition = inputService.MousePosition;
      _inputContext.MousePositionDelta = inputService.MousePositionDelta;
      _inputContext.IsMouseOver = true;

      // Start HandleInput traversal.
      HandleInput(_inputContext);

      // Input could have change the layout. --> Update so that the game objects that are
      // updated in the same frame can see the new layout.
      UpdateLayout();

      base.OnUpdate(deltaTime);
    }


    /// <inheritdoc/>
    protected override void OnHandleInput(InputContext context)
    {
      if (!InputEnabled)
        return;

      base.OnHandleInput(context);
    }


    /// <inheritdoc/>
    protected override Vector2F OnMeasure(Vector2F availableSize)
    {
      // The desired size is either set by the user or the whole viewport.
      float desiredWidth = Width;
      float desiredHeight = Height;

      if (!Numeric.IsPositiveFinite(desiredWidth))
        desiredWidth = Renderer.GraphicsDevice.Viewport.Width;
      if (!Numeric.IsPositiveFinite(desiredHeight))
        desiredHeight = Renderer.GraphicsDevice.Viewport.Height;

      foreach (var child in VisualChildren)
        child.Measure(new Vector2F(desiredWidth, desiredHeight));

      return new Vector2F(desiredWidth, desiredHeight);
    }


#if !SILVERLIGHT
    /// <overloads>
    /// <summary>
    /// Draws the screen with all controls.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Draws the screen with all controls.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game's timing state.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="gameTime"/> is <see langword="null"/>.
    /// </exception>
    public void Draw(GameTime gameTime)
    {
      if (gameTime == null)
        throw new ArgumentNullException("gameTime");

      Draw(gameTime.ElapsedGameTime);
    }
#endif

    
    /// <summary>
    /// Draws the screen with all controls.
    /// </summary>
    /// <param name="deltaTime">The size of the current time step.</param>
    public void Draw(TimeSpan deltaTime)
    {
      // Set up render context.
      _renderContext.DeltaTime = deltaTime;
      Debug.Assert(_renderContext.Opacity == 1.0f, "Opacity in render context has not been reset.");
      Debug.Assert(_renderContext.RenderTransform == RenderTransform.Identity, "RenderTransform in render context has not been reset.");

      // Render screen including all controls.
      Render(_renderContext);
    }


    /// <inheritdoc/>
    protected override void OnRender(UIRenderContext context)
    {
      // Make sure the layout is up-to-date.
      UpdateLayout();

      var originalScissorRectangle = Renderer.GraphicsDevice.ScissorRectangle;

      // The renderer uses scissor test. We need to set a default rectangle. 
      // (In MonoGame with OpenGL, the default scissor rectangle might not cover the full screen.)
      Renderer.GraphicsDevice.ScissorRectangle = ActualBounds.ToRectangle(true);

      // Start the rendering process.
      Renderer.BeginBatch();
      Renderer.Render(this, context);
      Renderer.EndBatch();

      Renderer.GraphicsDevice.ScissorRectangle = originalScissorRectangle;
    }


    /// <summary>
    /// Brings a control to the front of the z-order.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="control"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="control"/> is not a child of this UI screen.
    /// </exception>
    public void BringToFront(UIControl control)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      int index = Children.IndexOf(control);
      if (index == -1)
        throw new ArgumentException("control is not a child of this UI screen.");

      Children.Move(index, Children.Count - 1);
    }

 
#if !SILVERLIGHT
    /// <summary>
    /// Raises the <see cref="IDrawable.DrawOrderChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    ///// <remarks>
    ///// <strong>Notes to Inheritors: </strong>When overriding <see cref="OnDrawOrderChanged"/> in a 
    ///// derived class, be sure to call the base class's <see cref="OnDrawOrderChanged"/> method so 
    ///// that registered delegates receive the event.
    ///// </remarks>
    private void OnDrawOrderChanged(EventArgs eventArgs)
    {
      var handler = ZIndexChanged;

      if (handler != null)
        handler(this, eventArgs);
    }


    /// <summary>
    /// Raises the <see cref="VisibleChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    ///// <remarks>
    ///// <strong>Notes to Inheritors: </strong>When overriding <see cref="OnVisibleChanged"/> in a 
    ///// derived class, be sure to call the base class's <see cref="OnVisibleChanged"/> method so 
    ///// that registered delegates receive the event.
    ///// </remarks>
    private void OnVisibleChanged(EventArgs eventArgs)
    {
      var handler = VisibleChanged;

      if (handler != null)
        handler(this, eventArgs);
    }
#endif
    #endregion
  }
}
