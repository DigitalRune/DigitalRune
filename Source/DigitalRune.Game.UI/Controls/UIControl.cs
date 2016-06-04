// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
#if !SILVERLIGHT
using Microsoft.Xna.Framework.Input.Touch;
#endif
using MouseButtons = DigitalRune.Game.Input.MouseButtons;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents the base class for user interface (UI) controls.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>The Visual Tree:</strong> Controls are managed in a visual tree. Each control has a 
  /// <see cref="VisualParent"/> and <see cref="VisualChildren"/>. <see cref="VisualChildren"/> are
  /// managed by the control classes themselves; this collection should not be directly changed by
  /// the user. The controls will automatically put a "logical child" into the 
  /// <see cref="VisualChildren"/> collection if necessary. The root of the visual tree is the 
  /// <see cref="UIScreen"/> which starts all update, layout and render traversals. The 
  /// <see cref="VisualParent"/> of the <see cref="UIScreen"/> is <see langword="null"/>. Only
  /// objects in the visual tree handle input, take part in the layout process and are rendered.
  /// </para>
  /// <para>
  /// <strong>The Logical Tree: </strong> <see cref="ContentControl"/>s have a 
  /// <see cref="ContentControl.Content"/> property. <see cref="Panel"/>s and the 
  /// <see cref="UIScreen"/> have a <strong>Children</strong> property. Other controls may have an 
  /// <strong>Items</strong> property. Using these properties the user can create parent-child
  /// relationships which define the logical tree. 
  /// </para>
  /// <para>
  /// <strong>A Logical/Visual Tree Example:</strong> A <see cref="Window"/> is a 
  /// <see cref="ContentControl"/>, therefore it has a <see cref="ContentControl.Content"/> 
  /// property. The user adds a logical child to the window by setting the Content property. The
  /// window will automatically put the Content into the <see cref="VisualChildren"/> collection
  /// because the Content should be updated and rendered with the window. The window has a few more
  /// visual children: An <see cref="Image"/> control that draws the window icon, a 
  /// <see cref="TextBlock"/> that draws the window title and a <see cref="Button"/> that represents
  /// the Close button of a window. These "internal" controls are also visual children.
  /// </para>
  /// <para>
  /// <strong>IsLoaded:</strong> The <see cref="UIControl"/> is loaded 
  /// (<see cref="GameObject.IsLoaded"/>) when the control is added to the 
  /// <see cref="VisualChildren"/> of a loaded control, or when the control is in the 
  /// <see cref="VisualChildren"/> of an unloaded parent control and the parent control is loaded. 
  /// <see cref="GameObject.Update"/> is automatically called if the control is loaded.
  /// </para>
  /// <para>
  /// <strong>PropertyChanged Events: </strong> <see cref="INotifyPropertyChanged.PropertyChanged"/>
  /// events are only raised for game object properties, like <see cref="Background"/>, 
  /// <see cref="IsEnabled"/>, etc. The event is not raised for "normal" properties, like 
  /// <see cref="ActualIsEnabled"/>, <see cref="Screen"/>, <see cref="VisualState"/>, etc.
  /// </para>
  /// <para>
  /// <strong>Render Transforms:</strong> Controls have a <see cref="RenderTransform"/> that is
  /// defined using the game object properties <see cref="RenderTransformOrigin"/>, 
  /// <see cref="RenderTranslation"/>, <see cref="RenderRotation"/> and <see cref="RenderScale"/>.
  /// The render transform can be used to scale, rotate and translate the control. The render
  /// transform is correctly applied when input is handled (e.g. mouse clicks). But the render
  /// transform is ignored in the layout process.
  /// </para>
  /// <para>
  /// <strong>UIControl Properties:</strong> A <see cref="UIControl"/> is derived from 
  /// <see cref="GameObject"/> and therefore has game object properties (see 
  /// <see cref="GameProperty{T}"/>) and game object events (see <see cref="GameEvent{T}"/>). One
  /// big advantage of the game object property system is that they can be initialized in the XML
  /// file of the UI <see cref="Theme"/>. The UI control properties are extended game object 
  /// properties: They must be created using <see cref="CreateProperty{T}"/> and 
  /// <see cref="CreateEvent{T}"/>. <see cref="UIPropertyOptions"/> can be assigned to UI control 
  /// properties (see <see cref="GetPropertyOptions"/> and <see cref="SetPropertyOptions"/>). 
  /// Depending on the <see cref="UIPropertyOptions"/> property changes automatically invoke 
  /// <see cref="InvalidateMeasure"/>, <see cref="InvalidateArrange"/>, or 
  /// <see cref="InvalidateVisual"/> when required. Game object properties have only one global 
  /// default value. If a control class, e.g. a Button, should have class-specific default values, 
  /// the method <see cref="OverrideDefaultValue{T}"/> can be used.
  /// </para>
  /// <para>
  /// <strong>Styles and Templates:</strong> Each control has a <see cref="Style"/>. When the 
  /// control is loaded. The control retrieves style information (e.g. default values) from the 
  /// <see cref="IUIRenderer"/> and creates a template game object with property values determined
  /// by the used <see cref="Style"/>. This game object is set as the 
  /// <see cref="GameObject.Template"/>. To apply a new style to an already loaded control, the
  /// control must first be removed from the visual tree and afterwards added back again to the
  /// visual tree.
  /// </para>
  /// <para>
  /// <strong>The Layout Process:</strong> The layout process is a simplified version of the 
  /// WPF/Silverlight layout process. The layout process starts at the root of the visual tree (the 
  /// <see cref="UIScreen"/>) or when <see cref="UpdateLayout"/> is called. First, 
  /// <see cref="Measure"/> is called for all controls in the visual tree. <see cref="Measure"/>
  /// computes the <see cref="DesiredWidth"/> and <see cref="DesiredHeight"/> of a control. These
  /// values are determined either by the theme or the user (see <see cref="Width"/>, 
  /// <see cref="Height"/>, <see cref="MinWidth"/> and <see cref="MinHeight"/>) or automatically 
  /// computed by the control. It is also possible to call <see cref="Measure"/> manually. 
  /// </para>
  /// <para>
  /// After that, <see cref="Arrange(Vector2F, Vector2F)"/> is called for all controls in the visual
  /// tree. <see cref="Arrange(Vector2F, Vector2F)"/> computes the <see cref="ActualX"/>, 
  /// <see cref="ActualY"/>, <see cref="ActualWidth"/> and <see cref="ActualHeight"/> of a control 
  /// in screen coordinates. The actual bounds are determined by the properties <see cref="X"/>, 
  /// <see cref="Y"/>, <see cref="Margin"/>, <see cref="HorizontalAlignment"/> and 
  /// <see cref="VerticalAlignment"/>. (<see cref="X"/> and <see cref="Y"/> are usually only used 
  /// for controls in a <see cref="Canvas"/> or directly under the <see cref="UIScreen"/>.) The 
  /// measure and arrange results are cached, and the layout process is only repeated when 
  /// necessary. A new layout process can be issued using <see cref="InvalidateMeasure"/> or 
  /// <see cref="InvalidateArrange"/>. When game object properties are changed, they automatically 
  /// invalidate the layout if necessary. Derived classes can override <see cref="OnMeasure"/> and 
  /// <see cref="OnArrange"/> to customize the layout results.
  /// </para>
  /// <para>
  /// <strong>The Rendering Process:</strong> The rendering process starts at the root of the visual
  /// tree (the screen) when <see cref="UIScreen.Draw(TimeSpan)"/> is called by the user. 
  /// <see cref="Render"/> will be called for each control in the visual tree. Derived classes can 
  /// override <see cref="OnRender"/> to do custom drawing. Per default, <see cref="OnRender"/> lets
  /// the renderer draw the control (see <see cref="IUIRenderer.Render"/>).
  /// </para>
  /// <para>
  /// <strong>Custom Rendering:</strong> There are several ways to customize the appearance and 
  /// rendering of controls:
  /// <list type="bullet">
  /// <item>
  /// If the <see cref="UIRenderer"/> is used: Change the styles in the UI theme (see the themes in 
  /// the example projects).
  /// </item>
  /// <item>
  /// If the <see cref="UIRenderer"/> is used: Add a new render method to the 
  /// <see cref="UIRenderer.RenderCallbacks"/> of the <see cref="UIRenderer"/> if custom drawing 
  /// code is necessary.
  /// </item>
  /// <item>
  /// Use your own <see cref="IUIRenderer"/> instead of the <see cref="UIRenderer"/>.
  /// </item>
  /// <item>
  /// Override <see cref="OnRender"/> of a specific <see cref="UIControl"/>.
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// <strong>Visual States:</strong> The <see cref="VisualState"/>s of this control are:
  /// "Disabled", "Default"
  /// </para>
  /// </remarks>
  public partial class UIControl : GameObject
  {
    //--------------------------------------------------------------
    #region Static Fields
    //--------------------------------------------------------------

    // The input event args can be cached and reused since UIControls are never updated in parallel.
    private static readonly InputEventArgs _inputEventArgs = new InputEventArgs();

    // Global list of UIPropertyOptions for all known properties.
    // For simplicity: Options are "global". If "X" of one type only AffectsRender and "X" in
    // another type AffectsArrange, then "X" AffectsArrange and AffectsRender everywhere.
    private static readonly DataStore<UIPropertyOptions> _uiPropertyOptions = new DataStore<UIPropertyOptions>();

    // The properties of each control type.
    private static readonly Dictionary<Type, List<IUIProperty>> _propertiesPerType = new Dictionary<Type, List<IUIProperty>>();

    // The events of each control type.
    private static readonly Dictionary<Type, List<IUIEvent>> _eventsPerType = new Dictionary<Type, List<IUIEvent>>();
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // When we loop over the visual children we loop over a copy of the collection. This way
    // the children can remove themselves from the original collection.
    private readonly List<UIControl> _visualChildrenCopy;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="UIScreen"/>.
    /// </summary>
    /// <value>
    /// The <see cref="UIScreen"/> or <see langword="null"/> if the control is not in a visual tree
    /// under a screen.
    /// </value>
    public UIScreen Screen
    {
      get
      {
        if (_screen != null)
          return _screen;

        // Search for screen in visual tree and cache the value.
        var control = this;
        do
        {
          _screen = control as UIScreen;
          if (_screen != null)
            return _screen;

          control = control.VisualParent;
        } while (control != null);

        return null;
      }
    }
    private UIScreen _screen;


    /// <summary>
    /// Gets the <see cref="IUIService"/>.
    /// </summary>
    /// <value>
    /// The <see cref="IUIService"/> or <see langword="null"/> if the control is not in a visual
    /// tree under a screen.
    /// </value>
    public IUIService UIService
    {
      get { return (Screen != null) ? Screen.UIService : null; }
    }


    /// <summary>
    /// Gets the <see cref="IInputService"/>.
    /// </summary>
    /// <value>
    /// The <see cref="IInputService"/> or <see langword="null"/> if the control is not in a visual
    /// tree under a screen.
    /// </value>
    public IInputService InputService
    {
      get { return (UIService != null) ? UIService.InputService : null; }
    }


    /// <summary>
    /// Gets the visual state of the control as string.
    /// </summary>
    /// <value>The visual state of the control as string.</value>
    /// <remarks>
    /// The visual state defines how the control should be rendered. The possible states depend on
    /// the type of control. The default states are "Default" and "Disabled". Other controls can add
    /// additional states like "MouseOver", "Pressed", etc.
    /// </remarks>
    public virtual string VisualState
    {
      get { return ActualIsEnabled ? "Default" : "Disabled"; }
    }


    /// <summary>
    /// Gets or sets the style.
    /// </summary>
    /// <value>The style.</value>
    /// <remarks>
    /// The style of a control defines how it should be rendered and what default values it should
    /// use. When a control is loaded or when the <see cref="Style"/> is changed, a template game
    /// object will be created with default values defined by the renderer. (The renderer defines
    /// the visual theme including default values.) This game object is then set as the
    /// <see cref="GameObject.Template"/> of this <see cref="UIControl"/>.
    /// </remarks>
    public string Style
    {
      get { return _style; }
      set
      {
        if (_style == value)
          return;

        _style = value;

        ApplyTemplate();
      }
    }
    private string _style;


    /// <summary>
    /// Gets the visual parent.
    /// </summary>
    /// <value>The visual parent.</value>
    public UIControl VisualParent { get; internal set; }


    /// <summary>
    /// Gets the visual children.
    /// </summary>
    /// <value>The visual children.</value>
    /// <remarks>
    /// This collection should only be modified by the controls themselves and not by the user.
    /// </remarks>
    public VisualChildCollection VisualChildren { get; private set; }


    /// <summary>
    /// Gets a value indicating whether this control is actually enabled, taking into account the 
    /// <see cref="IsEnabled"/> flag and the state of the <see cref="VisualParent"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this control <see cref="IsEnabled"/> and 
    /// <see cref="ActualIsEnabled"/> of the <see cref="VisualParent"/> is <see langword="true"/>; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool ActualIsEnabled
    {
      // The control must be loaded and enabled. If there is a visual parent is must be enabled too.
      // Note: The UIScreen can be enabled even if it does not have a visual parent.
      get { return IsLoaded && IsEnabled && (VisualParent == null || VisualParent.ActualIsEnabled); }
    }


    /// <summary>
    /// Gets a value indicating whether this control is actually visible, taking into account the 
    /// <see cref="IsVisible"/> flag and the state of the <see cref="VisualParent"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this control <see cref="IsVisible"/> and 
    /// <see cref="ActualIsVisible"/> of the <see cref="VisualParent"/> is <see langword="true"/>; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool ActualIsVisible
    {
      get { return IsLoaded && IsVisible && (VisualParent == null || VisualParent.ActualIsVisible); }
    }


    /// <summary>
    /// Gets or sets the <see cref="LogicalPlayerIndex"/> from which input is accepted.
    /// </summary>
    /// <value>The <see cref="LogicalPlayerIndex"/> from which input is accepted.</value>
    public LogicalPlayerIndex AllowedPlayer { get; set; }


    /// <summary>
    /// Gets or sets the context menu that should pop up when the control is right-clicked
    /// (tap-and-hold on Windows Phone 7).
    /// </summary>
    /// <value>
    /// The context menu. The default is <see langword="null"/>.
    /// </value>
    public ContextMenu ContextMenu { get; set; }


    /// <summary>
    /// Gets or sets the mouse cursor that should be displayed when the mouse is over this control.
    /// </summary>
    /// <value>
    /// The mouse cursor that should be displayed when the mouse is over this control.
    /// </value>
    /// <remarks>
    /// This object must be of type <strong>System.Windows.Forms.Cursor</strong>. (The type 
    /// <see cref="System.Object"/> is used to avoid referencing 
    /// <strong>System.Windows.Forms.dll</strong> in this portable library.)
    /// </remarks>
    public object Cursor { get; set; }


    /// <summary>
    /// Gets a value indicating whether this control has a render transform that is not the 
    /// identity transformation.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this control has render transform that is not the identity 
    /// transformation; otherwise, <see langword="false"/>.
    /// </value>
    public bool HasRenderTransform
    {
      get
      {
        if (!_hasRenderTransform.HasValue)
          _hasRenderTransform = (RenderScale != Vector2F.One || RenderRotation != 0 || RenderTranslation != Vector2F.Zero);

        return _hasRenderTransform.Value;
      }
    }
    private bool? _hasRenderTransform;


    /// <summary>
    /// Gets (or sets) a value indicating whether the focus is on this control or on any of 
    /// the visual children.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this control <see cref="IsFocused"/> or if the focus is within
    /// any of the visual children; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsFocusWithin { get; internal set; }


    /// <summary>
    /// Gets a value indicating whether the mouse is over this control and not over a visual child 
    /// control.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this mouse is over this control and not over a visual child; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool IsMouseDirectlyOver { get; private set; }


    /// <summary>
    /// Gets the render transformation.
    /// </summary>
    /// <value>The render transformation.</value>
    /// <remarks>
    /// <para>
    /// Render transforms scales, rotates and translates the control. This can be used for 
    /// animations, but it should be used sparingly since it costs performance. When handling input
    /// (e.g. mouse clicks) the render transform is considered and, for example, rotated buttons can
    /// be clicked normally.
    /// </para>
    /// <para>
    /// The render transformation is defined using the properties 
    /// <see cref="RenderTransformOrigin"/>, <see cref="RenderScale"/>, 
    /// <see cref="RenderRotation"/>, and <see cref="RenderTranslation"/>.
    /// </para>
    /// </remarks>
    public RenderTransform RenderTransform
    {
      get
      {
        return new RenderTransform(
          new Vector2F(ActualX, ActualY),
          ActualWidth,
          ActualHeight,
          RenderTransformOrigin,
          RenderScale,
          RenderRotation,
          RenderTranslation);
      }
    }


    /// <summary>
    /// Gets or sets a user-defined tag.
    /// </summary>
    /// <value>The user-defined tag.</value>
    [Obsolete("The property 'Tag' has been replaced by 'UserData'.")]
    public object Tag
    {
      get { return UserData; }
      set { UserData = value; }
    }


    /// <summary>
    /// Gets or sets user-defined data.
    /// </summary>
    /// <value>User-defined data.</value>
    /// <remarks>
    /// This property is intended for application-specific data and is not used by the control 
    /// itself. 
    /// </remarks>
    public object UserData { get; set; }


    /// <summary>
    /// Occurs before the device input is processed.
    /// </summary>
    /// <remarks>
    /// Handle this event to handle input BEFORE the control gets a chance to handle the input.
    /// </remarks>
    public event EventHandler<InputEventArgs> InputProcessing;


    /// <summary>
    /// Occurs after the device input was processed.
    /// </summary>
    public event EventHandler<InputEventArgs> InputProcessed;
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="Background"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int BackgroundPropertyId = CreateProperty(
      typeof(UIControl), "Background", GamePropertyCategories.Appearance, null, Color.Transparent,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the background color. 
    /// This is a game object property.
    /// </summary>
    /// <value>The background color.</value>
    /// <remarks>
    /// If the background color is not transparent (<strong>Color.A</strong> is not 0), then the
    /// <see cref="IUIRenderer"/> clears the background rectangle of this control with the 
    /// background color.
    /// </remarks>
    public Color Background
    {
      get { return GetValue<Color>(BackgroundPropertyId); }
      set { SetValue(BackgroundPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Foreground"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ForegroundPropertyId = CreateProperty(
      typeof(UIControl), "Foreground", GamePropertyCategories.Appearance, null, Color.Black,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the foreground color. 
    /// This is a game object property.
    /// </summary>
    /// <value>The foreground color.</value>
    /// <remarks>
    /// How this property is used depends on the control type.
    /// </remarks>
    public Color Foreground
    {
      get { return GetValue<Color>(ForegroundPropertyId); }
      set { SetValue(ForegroundPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Opacity"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int OpacityPropertyId = CreateProperty(
      typeof(UIControl), "Opacity", GamePropertyCategories.Appearance, null, 1.0f,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the opacity. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The opacity. 0 for a fully transparent (invisible) control. 1 for a fully opaque control. 
    /// </value>
    /// <remarks>
    /// If this value is less than 1, the control including its visual children are rendered
    /// transparent.
    /// </remarks>
    public float Opacity
    {
      get { return GetValue<float>(OpacityPropertyId); }
      set { SetValue(OpacityPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Font"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int FontPropertyId = CreateProperty<string>(
      typeof(UIControl), "Font", GamePropertyCategories.Appearance, null, null,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the font that is used if the control renders text. 
    /// This is a game object property.
    /// </summary>
    /// <value>The font.</value>
    public string Font
    {
      get { return GetValue<string>(FontPropertyId); }
      set { SetValue(FontPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IsEnabled"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsEnabledPropertyId = CreateProperty(
      typeof(UIControl), "IsEnabled", GamePropertyCategories.Common, null, true,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets a value indicating whether this control is enabled. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this control is enabled; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Disabled controls and its visual children are drawn and take part in the layout process but 
    /// they do not handle input. See also <see cref="ActualIsEnabled"/>.
    /// </remarks>
    public bool IsEnabled
    {
      get { return GetValue<bool>(IsEnabledPropertyId); }
      set { SetValue(IsEnabledPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IsVisible"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsVisiblePropertyId = CreateProperty(
      typeof(UIControl), "IsVisible", GamePropertyCategories.Appearance, null, true,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets a value indicating whether this control is visible.
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this control is visible; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This property can be set to <see langword="false"/> to hide the control and its visual
    /// children. Invisible controls are not drawn, they do not take part in the layout process and
    /// they do not handle input. See also <see cref="ActualIsVisible"/>.
    /// </remarks>
    public bool IsVisible
    {
      get { return GetValue<bool>(IsVisiblePropertyId); }
      set { SetValue(IsVisiblePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IsMouseOver"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsMouseOverPropertyId = CreateProperty(
      typeof(UIControl), "IsMouseOver", GamePropertyCategories.Input, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets a value indicating whether the mouse is over this control or over a visual 
    /// child. This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the mouse is over this control or over a visual child; otherwise,
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Use <see cref="IsMouseDirectlyOver"/> to check if the mouse is over this control but not
    /// over a visual child.
    /// </para>
    /// <para>
    /// <see cref="IsMouseOver"/> is set automatically before the input is handled (before
    /// <see cref="OnHandleInput"/>) and <see cref="IsMouseDirectlyOver"/> is set after all visual
    /// children have handled the input.
    /// </para>
    /// </remarks>
    public bool IsMouseOver
    {
      get { return GetValue<bool>(IsMouseOverPropertyId); }
      private set { SetValue(IsMouseOverPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="ToolTip"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ToolTipPropertyId = CreateProperty<object>(
      typeof(UIControl), "ToolTip", GamePropertyCategories.Default, null, null,
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the tool tip.
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The tool tip, which is either a <see cref="UIControl"/>, a <see cref="String"/>, or an
    /// <see cref="Object"/> (see remarks). The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If a tool tip is set, the <see cref="ToolTipManager"/> will automatically display a tool tip
    /// if the mouse hovers over the control for certain time without moving.
    /// </para>
    /// <para>
    /// The tool tip can be a <see cref="UIControl"/>, a <see cref="String"/>, or an 
    /// <see cref="Object"/>:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <strong>UIControl:</strong> If the tool tip is a control, then the control is shown as the 
    /// content of the <see cref="ToolTipManager.ToolTipControl"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <strong>String:</strong> If the tool tip is a <see cref="String"/>, then the string will be 
    /// wrapped in a <see cref="TextBlock"/> and shown in the 
    /// <see cref="ToolTipManager.ToolTipControl"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <strong>Object:</strong> If the tool tip is an <see cref="Object"/>, then the string 
    /// representation of the object will be shown as the tool tip. (The string will be wrapped in a
    /// <see cref="TextBlock"/> and shown in the <see cref="ToolTipManager.ToolTipControl"/>.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// The user can override this behavior by setting a 
    /// <see cref="ToolTipManager.CreateToolTipContent"/> callback in the 
    /// <see cref="ToolTipManager"/>. The callback receives the value stored in 
    /// <see cref="ToolTip"/> and returns the <see cref="UIControl"/> that will be shown in the 
    /// <see cref="ToolTipManager.ToolTipControl"/>.
    /// </para>
    /// </remarks>
    public object ToolTip
    {
      get { return GetValue<object>(ToolTipPropertyId); }
      set { SetValue(ToolTipPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="RenderTransformOrigin"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int RenderTransformOriginPropertyId = CreateProperty(
      typeof(UIControl), "RenderTransformOrigin", GamePropertyCategories.Appearance, null,
      Vector2F.Zero, UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the relative origin of the <see cref="RenderTransform"/>. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The relative origin of the render transformation. (0, 0) represents the upper left corner
    /// and (1, 1) represents the lower right corner of the element.
    /// </value>
    /// <seealso cref="RenderTransform"/>
    public Vector2F RenderTransformOrigin
    {
      get { return GetValue<Vector2F>(RenderTransformOriginPropertyId); }
      set { SetValue(RenderTransformOriginPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="RenderScale"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int RenderScalePropertyId = CreateProperty(
      typeof(UIControl), "RenderScale", GamePropertyCategories.Appearance, null, Vector2F.One,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the scale of the <see cref="RenderTransform"/>. 
    /// This is a game object property.
    /// </summary>
    /// <value>The scale factor.</value>
    /// <seealso cref="RenderTransform"/>
    public Vector2F RenderScale
    {
      get { return GetValue<Vector2F>(RenderScalePropertyId); }
      set { SetValue(RenderScalePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="RenderRotation"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int RenderRotationPropertyId = CreateProperty(
      typeof(UIControl), "RenderRotation", GamePropertyCategories.Appearance, null, 0.0f,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the rotation of the <see cref="RenderTransform"/>. 
    /// This is a game object property.
    /// </summary>
    /// <value>The rotation angle in radians.</value>
    /// <seealso cref="RenderTransform"/>
    public float RenderRotation
    {
      get { return GetValue<float>(RenderRotationPropertyId); }
      set { SetValue(RenderRotationPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="RenderTranslation"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int RenderTranslationPropertyId = CreateProperty(
      typeof(UIControl), "RenderTranslation", GamePropertyCategories.Appearance, null,
      Vector2F.Zero, UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the translation of the <see cref="RenderTransform"/>. 
    /// This is a game object property.
    /// </summary>
    /// <value>The translation vector.</value>
    /// <seealso cref="RenderTransform"/>
    public Vector2F RenderTranslation
    {
      get { return GetValue<Vector2F>(RenderTranslationPropertyId); }
      set { SetValue(RenderTranslationPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IsFocused"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsFocusedPropertyId = CreateProperty(
      typeof(UIControl), "IsFocused", GamePropertyCategories.Input, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets a value indicating whether this control has the input focus. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this control has the input focus; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This value is controlled by the <see cref="FocusManager"/>.
    /// </remarks>
    public bool IsFocused
    {
      get { return GetValue<bool>(IsFocusedPropertyId); }
      internal set { SetValue(IsFocusedPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IsFocusScope"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsFocusScopePropertyId = CreateProperty(
      typeof(UIControl), "IsFocusScope", GamePropertyCategories.Input, null, false,
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets a value indicating whether this control is a focus scope. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this control is a focus scope; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Within focus scopes the focus can be moved using arrow keys on the keyboard or thumbstick 
    /// and direction pad on the gamepad. See <see cref="FocusManager"/>.
    /// </remarks>
    public bool IsFocusScope
    {
      get { return GetValue<bool>(IsFocusScopePropertyId); }
      set { SetValue(IsFocusScopePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="AutoUnfocus"/> game object property.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unfocus")]
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int AutoUnfocusPropertyId = CreateProperty(
      typeof(UIControl), "AutoUnfocus", GamePropertyCategories.Input, null, false,
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets a value indicating whether this control clears the focus when the mouse clicks
    /// another control than the currently focused control. This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this control clears the focus when a control other than the
    /// currently focused control is clicked; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// For <see cref="UIScreen"/>s the property <see cref="AutoUnfocus"/> is usually set because 
    /// when the user clicks the screen he/she usually wants to interact with the 3D scene behind 
    /// the screen. The currently selected control should lose the focus.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public bool AutoUnfocus
    {
      get { return GetValue<bool>(AutoUnfocusPropertyId); }
      set { SetValue(AutoUnfocusPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Focusable"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int FocusablePropertyId = CreateProperty(
      typeof(UIControl), "Focusable", GamePropertyCategories.Input, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="UIControl"/> can receive the input 
    /// focus. This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this focus can receive the input focus; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Text blocks are usually not focusable. Buttons and other interactive elements are usually
    /// focusable. 
    /// </remarks>
    public bool Focusable
    {
      get { return GetValue<bool>(FocusablePropertyId); }
      set { SetValue(FocusablePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="FocusWhenMouseOver"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int FocusWhenMouseOverPropertyId = CreateProperty(
      typeof(UIControl), "FocusWhenMouseOver", GamePropertyCategories.Input, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets a value indicating whether the control automatically receives focus when the
    /// mouse is over the control (without being clicked). This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the control automatically receives focus when the mouse is over
    /// the control; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This property can be set for menus where the focus (= the "selection") should follow the 
    /// mouse cursor.
    /// </remarks>
    public bool FocusWhenMouseOver
    {
      get { return GetValue<bool>(FocusWhenMouseOverPropertyId); }
      set { SetValue(FocusWhenMouseOverPropertyId, value); }
    }


    // Offset that is added to compute Arrange position. 
    // Makes sense for controls with free layout/draggable content: Positioning in UIScreen, 
    // Canvas, Thumb positioning in ScrollBar.
    // X/Y should only be used with Canvas and UIScreen. Other controls do not check this values 
    // when computing their desired size in OnMeasure.

    /// <summary> 
    /// The ID of the <see cref="X"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int XPropertyId = CreateProperty(
      typeof(UIControl), "X", GamePropertyCategories.Layout, null, 0.0f,
      UIPropertyOptions.AffectsArrange);

    /// <summary>
    /// Gets or sets the x-position offset of the control. (Use this only for controls in a 
    /// <see cref="Canvas"/> or under the <see cref="UIScreen"/>.) This is a game object property.
    /// </summary>
    /// <value>The x-position offset of the control.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float X
    {
      get { return GetValue<float>(XPropertyId); }
      set { SetValue(XPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Y"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int YPropertyId = CreateProperty(
      typeof(UIControl), "Y", GamePropertyCategories.Layout, null, 0.0f,
      UIPropertyOptions.AffectsArrange);

    /// <summary>
    /// Gets or sets the y-position offset of the control. (Use this only for controls in a 
    /// <see cref="Canvas"/> or under the <see cref="UIScreen"/>.) This is a game object property.
    /// </summary>
    /// <value>The y-position offset of the control.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float Y
    {
      get { return GetValue<float>(YPropertyId); }
      set { SetValue(YPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Width"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int WidthPropertyId = CreateProperty(
      typeof(UIControl), "Width", GamePropertyCategories.Layout, null, 0.0f,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the user-defined width. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The user-defined width. If this value is NaN, the desired width of the control is computed
    /// automatically.
    /// </value>
    public float Width
    {
      get { return GetValue<float>(WidthPropertyId); }
      set { SetValue(WidthPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Height"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int HeightPropertyId = CreateProperty(
      typeof(UIControl), "Height", GamePropertyCategories.Layout, null, 0.0f,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the user-defined height. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The user-defined height. If this value is NaN, the desired height of the control is computed
    /// automatically. The default value is NaN
    /// </value>
    public float Height
    {
      get { return GetValue<float>(HeightPropertyId); }
      set { SetValue(HeightPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="MinWidth"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int MinWidthPropertyId = CreateProperty(
      typeof(UIControl), "MinWidth", GamePropertyCategories.Layout, null, 0.0f,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the minimal width of the control. 
    /// This is a game object property.
    /// </summary>
    /// <value>The minimal width of the control.</value>
    public float MinWidth
    {
      get { return GetValue<float>(MinWidthPropertyId); }
      set { SetValue(MinWidthPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="MinHeight"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int MinHeightPropertyId = CreateProperty(
      typeof(UIControl), "MinHeight", GamePropertyCategories.Layout, null, 0.0f,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the minimal height of the control. 
    /// This is a game object property.
    /// </summary>
    /// <value>The minimal height of the control.</value>
    public float MinHeight
    {
      get { return GetValue<float>(MinHeightPropertyId); }
      set { SetValue(MinHeightPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="MaxWidth"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int MaxWidthPropertyId = CreateProperty(
      typeof(UIControl), "MaxWidth", GamePropertyCategories.Layout, null, float.NaN,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the maximal width of the control. 
    /// This is a game object property.
    /// </summary>
    /// <value>The maximal width of the control.</value>
    public float MaxWidth
    {
      get { return GetValue<float>(MaxWidthPropertyId); }
      set { SetValue(MaxWidthPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="MaxHeight"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int MaxHeightPropertyId = CreateProperty(
      typeof(UIControl), "MaxHeight", GamePropertyCategories.Layout, null, float.NaN,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the maximal height of the control. 
    /// This is a game object property.
    /// </summary>
    /// <value>The maximal height of the control.</value>
    public float MaxHeight
    {
      get { return GetValue<float>(MaxHeightPropertyId); }
      set { SetValue(MaxHeightPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Padding"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int PaddingPropertyId = CreateProperty(
      typeof(UIControl), "Padding", GamePropertyCategories.Layout, null, Vector4F.Zero,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the padding. 
    /// This is a game object property.
    /// </summary>
    /// <value>The padding as 4D vector (left, top, right bottom).</value>
    /// <remarks>
    /// How this value is used depends on the control type.
    /// </remarks>
    public Vector4F Padding
    {
      get { return GetValue<Vector4F>(PaddingPropertyId); }
      set { SetValue(PaddingPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Margin"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int MarginPropertyId = CreateProperty(
      typeof(UIControl), "Margin", GamePropertyCategories.Layout, null, Vector4F.Zero,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the margin. 
    /// This is a game object property.
    /// </summary>
    /// <value>The margin as 4D vector (left, top, right bottom).</value>
    public Vector4F Margin
    {
      get { return GetValue<Vector4F>(MarginPropertyId); }
      set { SetValue(MarginPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="HorizontalAlignment"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int HorizontalAlignmentPropertyId = CreateProperty(
      typeof(UIControl), "HorizontalAlignment", GamePropertyCategories.Layout, null,
      HorizontalAlignment.Left, UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the horizontal alignment of this control. 
    /// This is a game object property.
    /// </summary>
    /// <value>The horizontal alignment.</value>
    /// <remarks>
    /// When both a <see cref="Width"/> and a <see cref="HorizontalAlignment"/> of 
    /// <see cref="UI.HorizontalAlignment.Stretch"/> are set, then the explicitly set width has 
    /// priority and horizontal alignment will be ignored.
    /// </remarks>
    public HorizontalAlignment HorizontalAlignment
    {
      get { return GetValue<HorizontalAlignment>(HorizontalAlignmentPropertyId); }
      set { SetValue(HorizontalAlignmentPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="VerticalAlignment"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int VerticalAlignmentPropertyId = CreateProperty(
      typeof(UIControl), "VerticalAlignment", GamePropertyCategories.Layout, null,
      VerticalAlignment.Top, UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the vertical alignment of this control. 
    /// This is a game object property.
    /// </summary>
    /// <value>The vertical alignment.</value>
    /// <remarks>
    /// When both a <see cref="Height"/> and a <see cref="VerticalAlignment"/> of 
    /// <see cref="UI.VerticalAlignment.Stretch"/> are set, then the explicitly set height has 
    /// priority and vertical alignment will be ignored.
    /// </remarks>
    public VerticalAlignment VerticalAlignment
    {
      get { return GetValue<VerticalAlignment>(VerticalAlignmentPropertyId); }
      set { SetValue(VerticalAlignmentPropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="UIControl"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static UIControl()
    {
      OverrideDefaultValue(typeof(UIControl), WidthPropertyId, float.NaN);
      OverrideDefaultValue(typeof(UIControl), HeightPropertyId, float.NaN);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="UIControl"/> class.
    /// </summary>
    public UIControl()
    {
      Style = "UIControl";

      VisualChildren = new VisualChildCollection(this);
      _visualChildrenCopy = new List<UIControl>();

      var isVisible = Properties.Get<bool>(IsVisiblePropertyId);
      isVisible.Changed += OnIsVisibleChanged;

      var isEnabled = Properties.Get<bool>(IsEnabledPropertyId);
      isEnabled.Changed += OnIsEnabledChanged;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnLoad()
    {
      _screen = null;

      base.OnLoad();

      InvalidateMeasure();
      ApplyTemplate();

      // Load all visual children.
      foreach (var child in VisualChildren)
        child.Load();
    }


    /// <inheritdoc/>
    protected override void OnUnload()
    {
      CleanState();

      // Unload children.
      foreach (var child in VisualChildren)
        child.Unload();

      Template = null;
      _screen = null;

      base.OnUnload();
    }


    private void CleanState()
    {
      ClearIsMouseOver();

      if (IsFocusWithin && Screen != null && Screen != this)
        Screen.FocusManager.ClearFocus();
    }


    private void ClearIsMouseOver()
    {
      IsMouseOver = false;
      IsMouseDirectlyOver = false;

      foreach (var child in VisualChildren)
        child.ClearIsMouseOver();
    }


    /// <summary>
    /// Moves the input focus to this control.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the focus was moved to this control; otherwise <see langword="false"/>.
    /// </returns>
    public bool Focus()
    {
      if (Screen != null)
        return Screen.FocusManager.Focus(this);

      return false;
    }


    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      if (!IsEnabled || !IsVisible)
        return;

      // Update children in reverse drawing order (front-to-back).
      UpdateVisualChildrenCopy();
      for (int i = _visualChildrenCopy.Count - 1; i >= 0; i--)
        _visualChildrenCopy[i].OnUpdate(deltaTime);

      base.OnUpdate(deltaTime);
    }


    private void OnIsVisibleChanged(object sender, GamePropertyEventArgs<bool> eventArgs)
    {
      // We need to clean the state because of the "Missing HandleInput" Problem: 
      // In one frame a window is hidden using the close button. In the next frame HandleInput
      // is skipped on the window children.
      // A potential problem occurs if a window is made visible in a frame after its
      // HandleInput() was called. --> HandleInput is not called for these children in this
      // frame. That means some states that are determined in HandleInput are still from the 
      // frame when the window was hidden! If we did not call ClearIsMouseOver recursively
      // the CloseButton would shine for one frame when the window reappears because 
      // CloseButton.IsMouseOver would still be set.

      if (!IsVisible)
        CleanState();
    }


    private void OnIsEnabledChanged(object sender, GamePropertyEventArgs<bool> eventArgs)
    {
      if (!IsEnabled)
        CleanState();
    }


    /// <summary>
    /// Handles the input.
    /// </summary>
    /// <param name="context">The input context.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    internal void HandleInput(InputContext context)
    {
      // This method is automatically called by the visual parent.
      Debug.Assert(Screen != null, "Cannot process input. Screen is not set.");
      Debug.Assert(InputService != null, "Cannot process input. Input service is not set.");

      if (!IsEnabled || !IsVisible || !IsLoaded)
        return;

      if (_inputEventArgs.Context != null)
        throw new UIException("HandleInput() was called while an InputProcessing/InputProcessed event was active. Such recursive calls are forbidden.");

      // AllowedPlayer, MousePosition(Delta) of the input context are updated and restored 
      // at the end.
      LogicalPlayerIndex originalAllowedPlayer = context.AllowedPlayer;
      Vector2F originalMousePosition = context.MousePosition;
      Vector2F originalMousePositionDelta = context.MousePositionDelta;

      // If the AllowedPlayer of this control is set to a specific player, then we use this
      // value. Otherwise, we use the player of the input context (which is probably more 
      // specific than "Any").
      if (AllowedPlayer != LogicalPlayerIndex.Any)
        context.AllowedPlayer = AllowedPlayer;

      if (HasRenderTransform)
      {
        // ----- Untransform mouse position using RenderTransform.
        // (Convert mouse position from screen space to the untransformed local space of 
        // the control.)
        var transform = RenderTransform;
        context.MousePosition = transform.FromRenderPosition(context.MousePosition);
        context.MousePositionDelta = transform.FromRenderDirection(context.MousePositionDelta);
      }

      // ----- InputProcessing Event
      var handler = InputProcessing;
      if (handler != null)
      {
        _inputEventArgs.Context = context;
        handler(this, _inputEventArgs);
        _inputEventArgs.Context = null;
      }

      // IsMouseOver is set before input is handled. IsMouseDirectlyOver is set after visual 
      // children have handled input.
      IsMouseOver = !InputService.IsMouseOrTouchHandled
                    && context.IsMouseOver
                    && HitTest(null, context.MousePosition);

      // ----- virtual OnHandleInput method
      OnHandleInput(context);

      // ----- InputProcessed Event
      handler = InputProcessed;
      if (handler != null)
      {
        _inputEventArgs.Context = context;
        handler(this, _inputEventArgs);
        _inputEventArgs.Context = null;
      }

      // Restore original input context.
      context.AllowedPlayer = originalAllowedPlayer;
      context.MousePosition = originalMousePosition;
      context.MousePositionDelta = originalMousePositionDelta;
    }


    /// <summary>
    /// Called when the control should handle device input.
    /// </summary>
    /// <param name="context">The input context.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnHandleInput"/> in a 
    /// derived class, be sure to call the base class's <see cref="OnHandleInput"/> method. The base
    /// implementation of this method calls <see cref="OnHandleInput"/> for all visual children.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
    protected virtual void OnHandleInput(InputContext context)
    {
      // Should only be called for visible and enabled controls.
      if (!IsLoaded)
        return;

      // Check if the mouse is over this control. 
      // - The IsMouseOver can only be set if the mouse was not handled before (for example by a
      //   control that overlaps this control).
      // - The parent must allow IsMouseOver (context.IsMouseOver flag).
      // - And the mouse must be in the hit box of the control (HitTest() method).
      var screen = Screen;
      var inputService = InputService;
      bool isMouseOver = IsMouseOver;

      // Check whether to focus the control.
      if (isMouseOver && Focusable && !IsFocusWithin)
      {
        if (FocusWhenMouseOver
            || inputService.IsPressed(MouseButtons.Left, false) // The control is clicked.
            || inputService.IsPressed(MouseButtons.Right, false))
        {
          screen.FocusManager.Focus(this);
        }
      }

      UpdateVisualChildrenCopy();

      // ----- Update all children.
      // Remember if the mouse is over any child.
      bool isMouseOverChild = false;

      // The focused child must be update first!
      UIControl focusedChild = GetFocusedChild();
      if (focusedChild != null)
      {
        HandleInput(focusedChild, context);
        isMouseOverChild = focusedChild.IsMouseOver;
      }

      // Update children in reverse order (front-to-back).
      for (int i = _visualChildrenCopy.Count - 1; i >= 0; i--)
      {
        UIControl child = _visualChildrenCopy[i];

        // The focused child was already updated.
        if (child == focusedChild)
          continue;

        HandleInput(child, context);
        isMouseOverChild = isMouseOverChild || child.IsMouseOver;
      }

      // Handle mouse input.
      IsMouseDirectlyOver = isMouseOver && !isMouseOverChild;

      // Update Screen.ControlUnderMouse.
      if (IsMouseDirectlyOver)
        screen.ControlUnderMouse = this;

      // Open ContextMenu if right mouse buttons is pressed or with tap-and-hold on phone.
      if (!inputService.IsMouseOrTouchHandled
          && isMouseOver
          && ContextMenu != null
          && ContextMenu.IsEnabled
          && ContextMenu.Items.Count > 0)
      {
        if (inputService.IsPressed(MouseButtons.Right, false))
        {
          inputService.IsMouseOrTouchHandled = true;
          ContextMenu.Open(this, context.MousePosition);
        }
#if !SILVERLIGHT
        else
        {
          foreach (var gesture in inputService.Gestures)
          {
            if (gesture.GestureType == GestureType.Hold)
            {
              inputService.IsMouseOrTouchHandled = true;
              ContextMenu.Open(this, context.MousePosition);
              break;
            }
          }
        }
#endif
      }

      // Now the focus manager gets a chance to check if any input was not handled and the 
      // focus should be moved.
      screen.FocusManager.MoveFocus(this, context.AllowedPlayer);
    }


    private void HandleInput(UIControl child, InputContext context)
    {
      // Children can be removed during update. (E.g. the first child could remove the 
      // second child as response to input.) Removed children do not get input.
      if (!child.IsLoaded)
        return;

      // Set IsMouseOver in input context.
      bool oldIsMouseOver = context.IsMouseOver;
      context.IsMouseOver = oldIsMouseOver && HitTest(child, context.MousePosition);

      // Update child.
      child.HandleInput(context);

      // Restore IsMouseOver flag.
      context.IsMouseOver = oldIsMouseOver;
    }


    /// <summary>
    /// Tests if a position is over a control.
    /// </summary>
    /// <param name="control">
    /// The control. If <see langword="null"/>, the position is checked against this control.
    /// </param>
    /// <param name="position">The position.</param>
    /// <returns>
    /// <see langword="true"/> if a mouse click at the <paramref name="position"/> can hit
    /// <paramref name="control"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The default implementation returns <see langword="true"/> if <paramref name="control"/> is
    /// this control or <see langword="null"/> and if the <paramref name="position"/> is within the
    /// <see cref="ActualBounds"/>. The default implementation returns always <see langword="true"/> 
    /// if <paramref name="control"/> is a control other than this control.
    /// </para>
    /// <para>
    /// <see cref="HitTest"/> is automatically called during the input traversal of the visual tree
    /// to set the <see cref="InputContext.IsMouseOver"/> flag in the <see cref="InputContext"/>. 
    /// It is not recommended to call <see cref="HitTest"/> manually.
    /// </para>
    /// <para>
    /// This method can be used in two ways: 
    /// </para>
    /// <para>
    /// A) <c>myControl.HitTest(null, position)</c> is called to check if the position is within 
    /// this control. <see cref="HitTest"/> can be changed in derived classes to create controls 
    /// where the hit zone is not rectangular, e.g. a round button. 
    /// </para>
    /// <para>
    /// B) <c>parent.HitTest(child, position)</c> is automatically called by a parent control (e.g.
    /// a <see cref="ContentControl"/>) during the input traversal to check "if the parent allows 
    /// the child to be hit". <see cref="HitTest"/> can be changed in derived classes to clip the 
    /// hit zone of the child. For example, a <see cref="ScrollViewer"/> (which is derived from
    /// <see cref="ContentControl"/>) will only return true if the position of the child is within 
    /// the viewport. The child itself does not know that part of it is invisible.
    /// </para>
    /// </remarks>
    protected virtual bool HitTest(UIControl control, Vector2F position)
    {
      if (control == this || control == null)
        return ActualBounds.Contains(position);

      return true;
    }


    /// <summary>
    /// Renders the control (including visual children).
    /// </summary>
    /// <param name="context">The render context.</param>
    public void Render(UIRenderContext context)
    {
      if (IsVisible
          && !Numeric.IsZero(ActualWidth)
          && !Numeric.IsZero(ActualHeight))
      {
        OnRender(context);

        // Any cached visual information must have been updated - or it is not needed until
        // IsVisible or Opacity are changed (which causes InvalidateVisual).
        IsVisualValid = true;
      }
    }


    /// <summary>
    /// Called when the control and its visual children should be rendered.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <para>
    /// The base implementation calls <see cref="IUIRenderer.Render"/> to let the renderer perform
    /// the drawing. This method can be overridden in derived classes to perform custom drawing
    /// using any means. 
    /// </para>
    /// <para>
    /// <strong>Important:</strong> If this method changes the render states or uses a sprite batch
    /// other than the <see cref="IUIRenderer.SpriteBatch"/> of the <see cref="IUIRenderer"/>, 
    /// <see cref="IUIRenderer.EndBatch"/> must be called to flush the current sprite batch before
    /// custom drawing code is executed.
    /// <code lang="csharp">
    /// <![CDATA[
    /// public class CustomControl : UIControl
    /// {
    ///   ...
    /// 
    ///   protected override void OnRender(RenderContext context)
    ///   {
    ///     // Get the renderer of the screen that owns this control.
    ///     var renderer = Screen.Renderer;
    /// 
    ///     // The renderer batches all SpriteBatch drawing calls together. Since we want to 
    ///     // change the graphics device settings, we have to commit the current batch.
    ///     renderer.EndBatch();
    /// 
    ///     // Do custom rendering here.
    ///     ...
    ///   }
    /// }
    /// ]]>
    /// </code>
    /// </para>
    ///  </remarks>
    protected virtual void OnRender(UIRenderContext context)
    {
      Screen.Renderer.Render(this, context);
    }


    /// <summary>
    /// Attempts to bring this element into view, within any scrollable regions it is contained
    /// within.
    /// </summary>
    public void BringIntoView()
    {
      // Find parent ScrollViewer and call its BringIntoView method. ScrollViewers can be nested!

      // Search for a parent that is a scroll viewer.
      UIControl parent = VisualParent;
      while (parent != null)
      {
        var scrollViewer = parent as ScrollViewer;
        if (scrollViewer != null)
          scrollViewer.BringIntoView(this);

        parent = parent.VisualParent;
      }
    }


    /// <summary>
    /// Gets the visual child where <see cref="IsFocusWithin"/> is <see langword="true"/>.
    /// </summary>
    /// <returns>The visual child that contains the focus.</returns>
    private UIControl GetFocusedChild()
    {
      if (!IsFocusWithin || IsFocused)
        return null;

      foreach (var child in _visualChildrenCopy)
        if (child.IsFocusWithin)
          return child;

      Debug.Assert(false, "IsFocusWithin is set. IsFocused is false. But could not find a child with IsFocusWithin.");
      return null;
    }


    /// <summary>
    /// Gets a control by name from the visual subtree of this control.
    /// </summary>
    /// <param name="name">The control name.</param>
    /// <returns>
    /// The control with the given name; or <see langword="null"/> if no matching control is found.
    /// </returns>
    public UIControl GetControl(string name)
    {
      if (Name == name)
        return this;

      // Make a depth-first search.
      foreach (var child in VisualChildren)
      {
        var control = child.GetControl(name);
        if (control != null)
          return control;
      }

      return null;
    }


    /// <summary>
    /// Updates <see cref="_visualChildrenCopy"/>.
    /// </summary>
    private void UpdateVisualChildrenCopy()
    {
      if (VisualChildren.IsChanged)
      {
        _visualChildrenCopy.Clear();
        foreach (var child in VisualChildren)
          _visualChildrenCopy.Add(child);

        VisualChildren.IsChanged = false;
      }
    }


    /// <inheritdoc/>
    protected override void OnPropertyChanged<T>(GameProperty<T> gameProperty, T oldValue, T newValue)
    {
      int propertyId = gameProperty.Metadata.Id;

      // Check UIPropertyOptions and invalidate measure/arrange/render if needed.
      var options = GetPropertyOptions(propertyId);
      if ((options & UIPropertyOptions.AffectsMeasure) != 0)
        InvalidateMeasure();
      else if ((options & UIPropertyOptions.AffectsArrange) != 0)
        InvalidateArrange();
      else if ((options & UIPropertyOptions.AffectsRender) != 0)
        InvalidateVisual();

      if (propertyId == RenderScalePropertyId
          || propertyId == RenderRotationPropertyId
          || propertyId == RenderTranslationPropertyId)
      {
        // Invalidate HasRenderTransform flag.
        _hasRenderTransform = null;
      }

      base.OnPropertyChanged(gameProperty, oldValue, newValue);
    }
    #endregion
  }
}
