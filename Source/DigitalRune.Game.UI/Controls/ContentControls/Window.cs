// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
using System;
using System.ComponentModel;
using DigitalRune.Game.Input;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Provides the ability to create, configure, show, and manage the lifetime of windows and 
  /// dialog boxes.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A window is <see cref="ContentControl"/>. The <see cref="ContentControl.Content"/> is usually
  /// a <see cref="Panel"/>. Besides the <see cref="ContentControl.Content"/> the window contains an
  /// <see cref="Icon"/>, a <see cref="Title"/> and a Close button.
  /// </para>
  /// <para>
  /// The window can be dragged with the mouse if <see cref="CanDrag"/> is <see langword="true"/>. A
  /// dragging operation starts when the user clicks any part of the window (except over nested
  /// controls). The window can be resized with the mouse if <see cref="CanResize"/> is 
  /// <see langword="true"/>. A resize operation starts when the user clicks the border of the
  /// window. <see cref="ResizeBorder"/> defines the size of the border where resize operations can
  /// start. For windows that can be dragged or resized, use only top/left for the 
  /// <see cref="UIControl.VerticalAlignment"/> and <see cref="UIControl.HorizontalAlignment"/>.
  /// </para>
  /// <para>
  /// <strong>Visual States:</strong> The <see cref="VisualState"/>s of this control are:
  /// "Disabled", "Default", "Active"
  /// </para>
  /// </remarks>
  /// <example>
  /// The following example shows how to create a simple message box.
  /// <code lang="csharp">
  /// <![CDATA[
  /// private void ShowMessageBox(UIScreen screen, string title, string message)
  /// {
  ///   // ----- Create the message box.
  ///   var text = new TextBlock
  ///   {
  ///     Text = message,
  ///     Margin = new Vector4F(4),
  ///     HorizontalAlignment = HorizontalAlignment.Center,
  ///   };
  /// 
  ///   var button = new Button
  ///   {
  ///     Content = new TextBlock { Text = "Ok" },
  ///     IsCancel = true,    // Cancel buttons are clicked when the user presses ESC (or BACK or B on the gamepad).
  ///     IsDefault = true,   // Default buttons are clicked when the user presses ENTER or SPACE (or START or A on the gamepad).
  ///     Margin = new Vector4F(4),
  ///     Width = 60,
  ///     HorizontalAlignment = HorizontalAlignment.Center,
  ///   };
  /// 
  ///   var stackPanel = new StackPanel { Margin = new Vector4F(4) };
  ///   stackPanel.Children.Add(text);
  ///   stackPanel.Children.Add(button);
  ///   
  ///   var window = new Window
  ///   {
  ///     CanResize = false,
  ///     IsModal = true,   // Modal dialogs consume all input until the window is closed.
  ///     Content = stackPanel,
  ///     MinHeight = 0,
  ///     Title = title,
  ///   };
  /// 
  ///   button.Click += (s, e) => window.Close();
  /// 
  ///   // ----- Show the window in the center of the screen.
  ///   // First, we need to open the window. 
  ///   window.Show(screen);
  /// 
  ///   // The window is now part of the visual tree of controls and can be measured. (The 
  ///   // window does not have a fixed size. Window.Width and Window.Height are NaN. The 
  ///   // size is calculated automatically depending on its content.)
  ///   window.Measure(new Vector2F(float.PositiveInfinity));
  ///   
  ///   // Measure() computes DesiredWidth and DesiredHeight. With this info we can center the 
  ///   // window on the screen.
  ///   window.X = screen.ActualWidth / 2 - window.DesiredWidth / 2;
  ///   window.Y = screen.ActualHeight / 2 - window.DesiredHeight / 2;
  /// }
  /// ]]>
  /// </code>
  /// </example>
  public class Window : ContentControl
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // The direction in which we are currently resizing. Determines the appearance 
    // of the mouse cursor.
    [Flags]
    private enum ResizeDirection
    {
      None = 0,
      N = 1,
      E = 2,
      S = 4,
      W = 8,
      NE = 1 + 2,
      SE = 2 + 4,
      SW = 4 + 8,
      NW = 1 + 8,
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Image _icon;
    private TextBlock _caption;
    private Button _closeButton;

    // For resizing:
    private bool _isResizing;
    private ResizeDirection _resizeDirection; // Set if mouse is over resize border or if currently resizing.
    private bool _setSpecialCursor;

    // For dragging:
    private bool _isDragging;

    // For resizing and dragging:
    private Vector2F _mouseStartPosition;
    private float _originalX;
    private float _originalY;
    private float _originalWidth;
    private float _originalHeight;
    private Vector2F _startPosition;
    private Vector2F _startSize;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the owner of this window that was specified in <see cref="Show"/> (typically a 
    /// <see cref="UIScreen"/>).
    /// </summary>
    /// <value>
    /// The owner of this window that was specified in <see cref="Show"/> (typically a 
    /// <see cref="UIScreen"/>).
    /// </value>
    public UIControl Owner { get; private set; }  // The owner gets back the focus when this window is closed.


    /// <inheritdoc/>
    public override string VisualState
    {
      get
      {
        if (!ActualIsEnabled)
          return "Disabled";

        if (IsActive)
          return "Active";

        return "Default";
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="CanDrag"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int CanDragPropertyId = CreateProperty(
      typeof(Window), "CanDrag", GamePropertyCategories.Behavior, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets a value indicating whether this window can dragged with the mouse. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this window can dragged with the mouse; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    public bool CanDrag
    {
      get { return GetValue<bool>(CanDragPropertyId); }
      set { SetValue(CanDragPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="CanResize"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int CanResizePropertyId = CreateProperty(
      typeof(Window), "CanResize", GamePropertyCategories.Behavior, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets a value indicating whether this window can be resized with the mouse. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this window can be resized with the mouse; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Note: Resizing should be disabled if a render transformation is permanently applied to the 
    /// window.
    /// </remarks>
    public bool CanResize
    {
      get { return GetValue<bool>(CanResizePropertyId); }
      set { SetValue(CanResizePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="ResizeBorder"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ResizeBorderPropertyId = CreateProperty(
      typeof(Window), "ResizeBorder", GamePropertyCategories.Layout, null, new Vector4F(4),
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the dimensions of the window border where resize operations can start. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The dimensions of the window border where resize operations can start. The default value
    /// is (4, 4, 4, 4).
    /// </value>
    public Vector4F ResizeBorder
    {
      get { return GetValue<Vector4F>(ResizeBorderPropertyId); }
      set { SetValue(ResizeBorderPropertyId, value); }
    }


#if !WINDOWS_UWP
    /// <summary> 
    /// The ID of the <see cref="DialogResult"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int DialogResultPropertyId = CreateProperty<bool?>(
      typeof(Window), "DialogResult", GamePropertyCategories.Default, null, null,
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the dialog result. 
    /// This is a game object property.
    /// </summary>
    /// <value>The dialog result.</value>
    /// <remarks>
    /// <para>
    /// This property is set to <see langword="null"/> when <see cref="Show"/> is called. Otherwise
    /// this property is not changed automatically. It is typically expected that OK buttons set
    /// this property to <see langword="true"/> and Cancel buttons set this property to 
    /// <see langword="false"/>.
    /// </para>
    /// <para>
    /// <strong>Special notes for Windows Universal (UWP):</strong> <br/>
    /// Usually, the type of this property is a nullable Boolean. In UWP nullable game object
    /// properties cannot be used because of a bug in .NET Native. Therefore, the type of this
    /// property is not nullable in the UWP build. A possible workaround is to use a different
    /// property for the dialog result, e.g. a property that uses returns an integer or an
    /// enumeration.
    /// </para>
    /// </remarks>
    public bool? DialogResult
    {
      get { return GetValue<bool?>(DialogResultPropertyId); }
      set { SetValue(DialogResultPropertyId, value); }
    }
#else
    // .NET Native bug: Nullable game object properties (e.g. bool?, Rectangle?) cannot be used 
    // because the create an access violation.

    /// <summary> 
    /// The ID of the <see cref="DialogResult"/> game object property.
    /// </summary>
    public static readonly int DialogResultPropertyId = CreateProperty<bool>(
      typeof(Window), "DialogResult", GamePropertyCategories.Default, null, false,
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the dialog result. 
    /// This is a game object property.
    /// </summary>
    /// <value>The dialog result.</value>
    /// <remarks>
    /// <para>
    /// This property is set to <see langword="null"/> when <see cref="Show"/> is called. Otherwise
    /// this property is not changed automatically. It is typically expected that OK buttons set
    /// this property to <see langword="true"/> and Cancel buttons set this property to 
    /// <see langword="false"/>.
    /// </para>
    /// <para>
    /// <strong>Special notes for Windows Universal (UWP):</strong> <br/>
    /// Usually, the type of this property is a nullable Boolean. In UWP nullable game object
    /// properties cannot be used because of a bug in .NET Native. Therefore, the type of this
    /// property is not nullable in the UWP build. A possible workaround is to use a different
    /// property for the dialog result, e.g. a property that uses returns an integer or an
    /// enumeration.
    /// </para>
    /// </remarks>
    public bool DialogResult
    {
      get { return GetValue<bool>(DialogResultPropertyId); }
      set { SetValue(DialogResultPropertyId, value); }
    }
#endif


    /// <summary> 
    /// The ID of the <see cref="HideOnClose"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int HideOnClosePropertyId = CreateProperty(
      typeof(Window), "HideOnClose", GamePropertyCategories.Behavior, null, false,
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets a value indicating whether a this window is made in visible or totally
    /// removed from the control tree when the window is closed. This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if <see cref="Close"/> hides this window by setting
    /// <see cref="UIControl.IsVisible"/> to <see langword="false"/>; otherwise, 
    /// <see langword="false"/> if <see cref="Close"/> detaches the window from the 
    /// <see cref="UIScreen"/>. The default value is <see langword="false"/>.
    /// </value>
    public bool HideOnClose
    {
      get { return GetValue<bool>(HideOnClosePropertyId); }
      set { SetValue(HideOnClosePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IsActive"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsActivePropertyId = CreateProperty(
      typeof(Window), "IsActive", GamePropertyCategories.Common, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets a value indicating whether this window is the currently active window. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this window is the currently active window; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Of all visible windows, only one window can be active. If <see cref="Activate"/> is called 
    /// the window is made active and all other windows are made inactive. Do not change 
    /// <see cref="IsActive"/> directly, use <see cref="Activate"/> instead.
    /// </remarks>
    public bool IsActive
    {
      get { return GetValue<bool>(IsActivePropertyId); }
      private set { SetValue(IsActivePropertyId, value); }
    }


    // blocks all input for objects behind the window.
    /// <summary> 
    /// The ID of the <see cref="IsModal"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsModalPropertyId = CreateProperty(
      typeof(Window), "IsModal", GamePropertyCategories.Behavior, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets a value indicating whether this window is a modal dialog. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this window is a modal dialog; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// A modal window blocks all input from windows that are behind the modal window. The user must
    /// close the modal window before he/she can interact with the other windows. The default value 
    /// is <see langword="false"/>.
    /// </remarks>
    public bool IsModal
    {
      get { return GetValue<bool>(IsModalPropertyId); }
      set { SetValue(IsModalPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="IconStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IconStylePropertyId = CreateProperty(
      typeof(Window), "IconStyle", GamePropertyCategories.Style, null, "Icon",
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the <see cref="Image"/> control that draws the 
    /// <see cref="Icon"/>. This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the <see cref="Image"/> control that draws the 
    /// <see cref="Icon"/>. Can be <see langword="null"/> or an empty string to hide the icon. The 
    /// default value is "Icon".
    /// </value>
    public string IconStyle
    {
      get { return GetValue<string>(IconStylePropertyId); }
      set { SetValue(IconStylePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Icon"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IconPropertyId = CreateProperty<Texture2D>(
      typeof(Window), "Icon", GamePropertyCategories.Appearance, null, null,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the texture that contains the window icon. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The texture that contains the window icon. The default value is <see langword="null"/>.
    /// </value>
    public Texture2D Icon
    {
      get { return GetValue<Texture2D>(IconPropertyId); }
      set { SetValue(IconPropertyId, value); }
    }


#if !WINDOWS_UWP
    /// <summary> 
    /// The ID of the <see cref="IconSourceRectangle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IconSourceRectanglePropertyId = CreateProperty<Rectangle?>(
      typeof(Window), "IconSourceRectangle", GamePropertyCategories.Appearance, null, null,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the region of the <see cref="Icon"/> texture that contains the icon. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The region of the <see cref="Icon"/> texture that contains the icon. Can be 
    /// <see langword="null"/> if the whole <see cref="Icon"/> texture should be drawn. 
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Special notes for Windows Universal (UWP):</strong> <br/>
    /// Usually, the type of this property is a nullable rectangle. In UWP nullable game object
    /// properties cannot be used because of a bug in .NET Native. Therefore, the type of this
    /// property is not nullable in the UWP build. Use an <see cref="Rectangle.Empty"/> rectangle if
    /// the whole texture should be displayed.
    /// </para>
    /// </remarks>
    public Rectangle? IconSourceRectangle
    {
      get { return GetValue<Rectangle?>(IconSourceRectanglePropertyId); }
      set { SetValue(IconSourceRectanglePropertyId, value); }
    }
#else
    // .NET Native bug: Nullable game object properties (e.g. bool?, Rectangle?) cannot be used 
    // because the create an access violation.

    /// <summary> 
    /// The ID of the <see cref="IconSourceRectangle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IconSourceRectanglePropertyId = CreateProperty<Rectangle>(
      typeof(Window), "IconSourceRectangle", GamePropertyCategories.Appearance, null, Rectangle.Empty,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the region of the <see cref="Icon"/> texture that contains the icon. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The region of the <see cref="Icon"/> texture that contains the icon. Can be 
    /// <see langword="null"/> if the whole <see cref="Icon"/> texture should be drawn. 
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Special notes for Windows Universal (UWP):</strong> <br/>
    /// Usually, the type of this property is a nullable rectangle. In UWP nullable game object
    /// properties cannot be used because of a bug in .NET Native. Therefore, the type of this
    /// property is not nullable in the UWP build. Use an <see cref="Rectangle.Empty"/> rectangle if
    /// the whole texture should be displayed.
    /// </para>
    /// </remarks>
    public Rectangle IconSourceRectangle
    {
      get { return GetValue<Rectangle>(IconSourceRectanglePropertyId); }
      set { SetValue(IconSourceRectanglePropertyId, value); }
    }
#endif

    /// <summary> 
    /// The ID of the <see cref="TitleTextBlockStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int TitleTextBlockStylePropertyId = CreateProperty(
      typeof(Window), "TitleTextBlockStyle", GamePropertyCategories.Style, null, "TitleTextBlock",
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the <see cref="TextBlock"/> that draws the window 
    /// <see cref="Title"/>. This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the <see cref="TextBlock"/> that draws the window 
    /// <see cref="Title"/>. Can be <see langword="null"/> or an empty string to hide the title.
    /// The default value is "TitleTextBlock".
    /// </value>
    public string TitleTextBlockStyle
    {
      get { return GetValue<string>(TitleTextBlockStylePropertyId); }
      set { SetValue(TitleTextBlockStylePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Title"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int TitlePropertyId = CreateProperty(
      typeof(Window), "Title", GamePropertyCategories.Common, null, "Unnamed",
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the window title that is visible in the caption bar. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The window title that is visible in the caption bar. The default value is "Unnamed".
    /// </value>
    public string Title
    {
      get { return GetValue<string>(TitlePropertyId); }
      set { SetValue(TitlePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="CloseButtonStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int CloseButtonStylePropertyId = CreateProperty(
      typeof(Window), "CloseButtonStyle", GamePropertyCategories.Style, null, "CloseButton",
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the Close button. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the Close button. Set this property to <see langword="null"/> 
    /// or an empty string to remove the Close button. The default value is "CloseButton".
    /// </value>
    public string CloseButtonStyle
    {
      get { return GetValue<string>(CloseButtonStylePropertyId); }
      set { SetValue(CloseButtonStylePropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Closing"/> game object event.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ClosingEventId = CreateEvent(
      typeof(Window), "Closing", GamePropertyCategories.Default, null, new CancelEventArgs());

    /// <summary>
    /// Occurs when the window is closing. Allows to cancel the closing operation. 
    /// This is a game object event.
    /// </summary>
    public event EventHandler<CancelEventArgs> Closing
    {
      add
      {
        var closing = Events.Get<CancelEventArgs>(ClosingEventId);
        closing.Event += value;
      }
      remove
      {
        var closing = Events.Get<CancelEventArgs>(ClosingEventId);
        closing.Event -= value;
      }
    }


    /// <summary> 
    /// The ID of the <see cref="Closed"/> game object event.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int ClosedEventId = CreateEvent(
      typeof(Window), "Closed", GamePropertyCategories.Default, null, EventArgs.Empty);

    /// <summary>
    /// Occurs when the window was closed using the <see cref="Close"/> method. 
    /// This is a game object event.
    /// </summary>
    /// <remarks>
    /// Depending on <see cref="HideOnClose"/> "closed" means either removed from the 
    /// <see cref="UIScreen"/> or only hidden (<see cref="UIControl.IsVisible"/> is 
    /// <see langword="false"/>). This event is only called if the window is closed using the 
    /// <see cref="Close"/> method.
    /// </remarks>
    public event EventHandler<EventArgs> Closed
    {
      add
      {
        var closed = Events.Get<EventArgs>(ClosedEventId);
        closed.Event += value;
      }
      remove
      {
        var closed = Events.Get<EventArgs>(ClosedEventId);
        closed.Event -= value;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="Window"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static Window()
    {
      // Windows are the standard focus scopes.
      OverrideDefaultValue(typeof(Window), IsFocusScopePropertyId, true);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Window"/> class.
    /// </summary>
    public Window()
    {
      Style = "Window";
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnLoad()
    {
      base.OnLoad();

      // Create icon.
      var iconStyle = IconStyle;
      if (!string.IsNullOrEmpty(iconStyle))
      {
        _icon = new Image
        {
          Name = "WindowIcon",
          Style = iconStyle,
          Texture = Icon,
          SourceRectangle = IconSourceRectangle,
        };
        VisualChildren.Add(_icon);

        // Connect Icon property with Image.Texture.
        GameProperty<Texture2D> icon = Properties.Get<Texture2D>(IconPropertyId);
        GameProperty<Texture2D> imageTexture = _icon.Properties.Get<Texture2D>(Image.TexturePropertyId);
        icon.Changed += imageTexture.Change;

#if !WINDOWS_UWP
        // Connect IconSourceRectangle property with Image.SourceRectangle.
        GameProperty<Rectangle?> iconSourceRectangle = Properties.Get<Rectangle?>(IconSourceRectanglePropertyId);
        GameProperty<Rectangle?> imageSourceRectangle = _icon.Properties.Get<Rectangle?>(Image.SourceRectanglePropertyId);
        iconSourceRectangle.Changed += imageSourceRectangle.Change;
#else
        // Connect IconSourceRectangle property with Image.SourceRectangle.
        GameProperty<Rectangle> iconSourceRectangle = Properties.Get<Rectangle>(IconSourceRectanglePropertyId);
        GameProperty<Rectangle> imageSourceRectangle = _icon.Properties.Get<Rectangle>(Image.SourceRectanglePropertyId);
        iconSourceRectangle.Changed += imageSourceRectangle.Change;
#endif
      }

      // Create text block for title.
      var titleTextBlockStyle = TitleTextBlockStyle;
      if (!string.IsNullOrEmpty(titleTextBlockStyle))
      {
        _caption = new TextBlock
        {
          Name = "WindowTitle",
          Style = titleTextBlockStyle,
          Text = Title,
        };
        VisualChildren.Add(_caption);

        // Connect Title property with TextBlock.Text.
        GameProperty<string> title = Properties.Get<string>(TitlePropertyId);
        GameProperty<string> captionText = _caption.Properties.Get<string>(TextBlock.TextPropertyId);
        title.Changed += captionText.Change;
      }

      // Create Close button.
      var closeButtonStyle = CloseButtonStyle;
      if (!string.IsNullOrEmpty(closeButtonStyle))
      {
        _closeButton = new Button
        {
          Name = "CloseButton",
          Style = closeButtonStyle,
          Focusable = false,
        };
        VisualChildren.Add(_closeButton);

        _closeButton.Click += OnCloseButtonClick;
      }
    }


    /// <inheritdoc/>
    protected override void OnUnload()
    {
      // Clean up and remove controls for icon, title and close button.
      if (_icon != null)
      {
        var icon = Properties.Get<Texture2D>(IconPropertyId);
        var imageTexture = _icon.Properties.Get<Texture2D>(Image.TexturePropertyId);
        icon.Changed -= imageTexture.Change;

#if !WINDOWS_UWP
        var iconSourceRectangle = Properties.Get<Rectangle?>(IconSourceRectanglePropertyId);
        var imageSourceRectangle = _icon.Properties.Get<Rectangle?>(Image.SourceRectanglePropertyId);
        iconSourceRectangle.Changed -= imageSourceRectangle.Change;
#else
        var iconSourceRectangle = Properties.Get<Rectangle>(IconSourceRectanglePropertyId);
        var imageSourceRectangle = _icon.Properties.Get<Rectangle>(Image.SourceRectanglePropertyId);
        iconSourceRectangle.Changed -= imageSourceRectangle.Change;
#endif

        VisualChildren.Remove(_icon);
        _icon = null;
      }

      if (_caption != null)
      {
        var title = Properties.Get<string>(TitlePropertyId);
        var captionText = _caption.Properties.Get<string>(TextBlock.TextPropertyId);
        title.Changed -= captionText.Change;

        VisualChildren.Remove(_caption);
        _caption = null;
      }

      if (_closeButton != null)
      {
        _closeButton.Click -= OnCloseButtonClick;
        VisualChildren.Remove(_closeButton);
        _closeButton = null;
      }

      if (_setSpecialCursor && UIService != null)
      {
        UIService.Cursor = null;
        _setSpecialCursor = false;
      }

      Owner = null;
      IsActive = false;
      base.OnUnload();
    }


    private void OnCloseButtonClick(object sender, EventArgs eventArgs)
    {
      Close();
    }


    /// <summary>
    /// Activates this window (and deactivates all other windows).
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if this window was successfully activated; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Activating also brings the window to the front.
    /// </remarks>
    public bool Activate()
    {
      // We must have a screen. Otherwise, we are not loaded.
      var screen = Screen;
      if (screen == null)
        return false;

      // Move focus into this window.
      if (!IsFocusWithin)
      {
        bool gotFocus = screen.FocusManager.Focus(this);

        // If we didn't get the focus (maybe this window does not contain focusable elements),
        // at least remove the focus from the other windows.
        if (!gotFocus)
          screen.FocusManager.ClearFocus();
      }

      if (IsActive)
        return true;

      // Deactivate all other windows on the screen.
      foreach (var child in screen.Children)
      {
        var window = child as Window;
        if (window != null && window != this)
          window.IsActive = false;
      }

      // Activate this window.
      IsActive = true;
      screen.BringToFront(this);

      return true;
    }


    /// <summary>
    /// Opens a window and returns without waiting for the newly opened window to close.
    /// </summary>
    /// <param name="owner">
    /// The owner of this window. If this window is closed, the focus moves back to the owner. Must
    /// not be <see langword="null"/>.
    /// </param>
    /// <remarks>
    /// The window is added to the <see cref="UIScreen"/> of the <paramref name="owner"/> 
    /// (unless it was already added to a screen) and activated (see <see cref="Activate"/>).
    /// <see cref="DialogResult"/> is reset to <see langword="null"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="owner"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="owner"/> is not loaded. The owner needs to be a visible control.
    /// </exception>
    public void Show(UIControl owner)
    {
      if (owner == null)
        throw new ArgumentNullException("owner");

      var screen = owner.Screen;
      if (screen == null)
        throw new ArgumentException("Invalid owner. Owner must be loaded.", "owner");

      Owner = owner;

      // Make visible and add to screen.
      IsVisible = true;
      if (VisualParent == null)
        screen.Children.Add(this);

      Activate();
#if !WINDOWS_UWP
      DialogResult = null;
#else
      DialogResult = false;
#endif
    }


    /// <summary>
    /// Closes this window.
    /// </summary>
    /// <remarks>
    /// This method raises the <see cref="Closing"/> and <see cref="Closed"/> events. The close
    /// operation can be canceled in <see cref="Closing"/>. If <see cref="HideOnClose"/> is 
    /// <see langword="true"/>, the window will only be hidden (<see cref="UIControl.IsVisible"/>
    /// is set to <see langword="false"/>). If <see cref="HideOnClose"/> is <see langword="false"/>
    /// the window is removed from the control tree.
    /// </remarks>
    public void Close()
    {
      if (!IsLoaded || (HideOnClose && !IsVisible))
        return; // Windows is already closed.

      if (_isResizing || _isDragging)
      {
        if (UIService != null)
          UIService.Cursor = null;

        _setSpecialCursor = false;
        _isResizing = false;
        _isDragging = false;
      }

      // Raise Closing event and check if close should be canceled.
      var eventArgs = new CancelEventArgs();
      var closing = Events.Get<CancelEventArgs>(ClosingEventId);
      closing.Raise(eventArgs);
      if (eventArgs.Cancel)
        return;

      var screen = Screen;

      // Move focus back to owner - but only if owner is not the screen and the focus
      // is currently in the window.
      if (!(Owner is UIScreen) && IsFocusWithin && screen != null)
        screen.FocusManager.Focus(Owner);

      Owner = null;
      IsActive = false;

      // Hide or remove from parent.
      if (HideOnClose)
      {
        IsVisible = false;
      }
      else
      {
        if (screen != null)
          screen.Children.Remove(this);
      }

      // Raise Closed event.
      var closed = Events.Get<EventArgs>(ClosedEventId);
      closed.Raise();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
    protected override void OnHandleInput(InputContext context)
    {
      var screen = Screen;
      var inputService = InputService;
      var uiService = UIService;

      bool mouseOrTouchWasHandled = inputService.IsMouseOrTouchHandled;
      UIControl oldFocusedControl = screen.FocusManager.FocusedControl;

      if (mouseOrTouchWasHandled && _setSpecialCursor)
      {
        // This window has changed the cursor in the last frame, but in this frame another
        // window has the mouse.
        // Minor problem: If the other window has also changed the cursor, then we remove
        // its special cursor. But this case should be rare.
        uiService.Cursor = null;
        _setSpecialCursor = false;
      }

      // Continue resizing and dragging if already in progress.
      ContinueResizeAndDrag(context);

      base.OnHandleInput(context);

      if (!IsLoaded)
        return;

      // Handling of activation is very complicated. For example: The window is clicked, but 
      // a context menu is opened by this click. And there are other difficult cases... Horror!
      if (!mouseOrTouchWasHandled)
      {
        // The mouse was not handled by any control that handled input before this window;
        if (!screen.IsFocusWithin                                       // Nothing in window is focused.
            || IsFocusWithin                                            // This window is focused.
            || oldFocusedControl == screen.FocusManager.FocusedControl) // The focus was not changed by a visual child. (Don't 
        {                                                               // activate window if focus moved to a new context menu or other popup!!!)
          // Mouse must be over the window and left or right mouse button must be pressed. 
          if (IsMouseOver)
          {
            if ((inputService.IsPressed(MouseButtons.Left, false)
                || inputService.IsPressed(MouseButtons.Right, false)))
            {
              Activate();
            }
          }
        }
      }

      // If the focus moves into this window, it should become activated.
      if (IsFocusWithin && !IsActive)
        Activate();

      // Check whether to start resizing or dragging.
      StartResizeAndDrag(context);

      // Update mouse cursor.
      if ((uiService.Cursor == null || _setSpecialCursor)          // Cursor of UIService was set by this window.
          && (!inputService.IsMouseOrTouchHandled || _isResizing)) // Mouse was not yet handled or is currently resizing.
      {
        switch (_resizeDirection)
        {
          case ResizeDirection.N:
          case ResizeDirection.S:
            uiService.Cursor = screen.Renderer.GetCursor("SizeNS");
            _setSpecialCursor = true;
            break;
          case ResizeDirection.E:
          case ResizeDirection.W:
            uiService.Cursor = screen.Renderer.GetCursor("SizeWE");
            _setSpecialCursor = true;
            break;
          case ResizeDirection.NE:
          case ResizeDirection.SW:
            uiService.Cursor = screen.Renderer.GetCursor("SizeNESW");
            _setSpecialCursor = true;
            break;
          case ResizeDirection.NW:
          case ResizeDirection.SE:
            uiService.Cursor = screen.Renderer.GetCursor("SizeNWSE");
            _setSpecialCursor = true;
            break;
          default:
            uiService.Cursor = null;
            _setSpecialCursor = false;
            break;
        }
      }

      // Mouse cannot act through a window.
      if (IsMouseOver)
        inputService.IsMouseOrTouchHandled = true;

      if (IsModal)
      {
        // Modal windows absorb all input.
        inputService.IsMouseOrTouchHandled = true;
#if !SILVERLIGHT
        inputService.SetGamePadHandled(context.AllowedPlayer, true);
#endif
        inputService.IsKeyboardHandled = true;
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private void StartResizeAndDrag(InputContext context)
    {
      if (_isResizing || _isDragging)
        return;

      bool canDrag = CanDrag;
      bool canResize = CanResize;
      if (!canDrag && !canResize)
        return;

      var inputService = InputService;

      // ----- Find out if mouse position is over the border
      _resizeDirection = ResizeDirection.None;
      if (!inputService.IsMouseOrTouchHandled   // Mouse is available for resizing.
          && canResize)                         // Window allows resizing.
      {
        // Position relative to window:
        Vector2F mousePosition = context.MousePosition - new Vector2F(ActualX, ActualY);

        // Find resize direction.
        if (IsMouseDirectlyOver)
        {
          bool isWest = (0 <= mousePosition.X && mousePosition.X <= ResizeBorder.X);
          bool isEast = (ActualWidth - ResizeBorder.Z < mousePosition.X && mousePosition.X < ActualWidth);
          bool isNorth = (0 <= mousePosition.Y && mousePosition.Y < ResizeBorder.Y);
          bool isSouth = (ActualHeight - ResizeBorder.W <= mousePosition.Y && mousePosition.Y < ActualHeight);
          if (isSouth && isEast)
            _resizeDirection = ResizeDirection.SE;
          else if (isSouth && isWest)
            _resizeDirection = ResizeDirection.SW;
          else if (isNorth && isEast)
            _resizeDirection = ResizeDirection.NE;
          else if (isNorth && isWest)
            _resizeDirection = ResizeDirection.NW;
          else if (isSouth)
            _resizeDirection = ResizeDirection.S;
          else if (isEast)
            _resizeDirection = ResizeDirection.E;
          else if (isWest)
            _resizeDirection = ResizeDirection.W;
          else if (isNorth)
            _resizeDirection = ResizeDirection.N;
        }
      }

      // ----- Start resizing.
      if (canResize)
      {
        if (_resizeDirection != ResizeDirection.None && inputService.IsPressed(MouseButtons.Left, false))
        {
          _isResizing = true;
          inputService.IsMouseOrTouchHandled = true;
          _mouseStartPosition = context.ScreenMousePosition;
          _startPosition = new Vector2F(ActualX, ActualY);
          _startSize = new Vector2F(ActualWidth, ActualHeight);
          BackupBounds();
          return;
        }
      }

      // ----- Start dragging.
      if (canDrag)
      {
        // The window can be grabbed on any point that is not a visual child 
        // (except for icon and title).
        bool isOverDragArea = IsMouseDirectlyOver
                             || (_icon != null && _icon.IsMouseOver)
                             || (_caption != null && _caption.IsMouseOver);
        if (isOverDragArea && inputService.IsPressed(MouseButtons.Left, false))
        {
          _isDragging = true;
          inputService.IsMouseOrTouchHandled = true;
          _mouseStartPosition = context.ScreenMousePosition;
          _startPosition = new Vector2F(ActualX, ActualY);
          BackupBounds();
          return;
        }
      }
    }


    private void ContinueResizeAndDrag(InputContext context)
    {
      if (!_isResizing && !_isDragging)
      {
        // Nothing to do.
        return;
      }

      var screen = Screen;
      var inputService = InputService;

      // ----- Stop dragging/resizing
      bool cancel = inputService.IsDown(Keys.Escape);
      if (cancel)
      {
        inputService.IsKeyboardHandled = true;
        RestoreBounds();
      }

      if (cancel                                  // <ESC> cancels dragging/resizing.
          || inputService.IsUp(MouseButtons.Left) // Mouse button is up.
          || inputService.IsMouseOrTouchHandled   // Mouse was handled by another control.
          || _isResizing && !CanResize            // CanResize has been reset by user during resizing.
          || _isDragging && !CanDrag)             // CanDrag has been reset by user during dragging.
      {
        screen.UIService.Cursor = null;
        _setSpecialCursor = false;
        _isResizing = false;
        _isDragging = false;
        return;
      }

      // Clamp mouse position to screen. (Only relevant if game runs in windowed-mode.)
      Vector2F screenMousePosition = context.ScreenMousePosition;
      float left = screen.ActualX;
      float right = left + screen.ActualWidth;
      float top = screen.ActualY;
      float bottom = top + screen.ActualHeight;
      screenMousePosition.X = MathHelper.Clamp(screenMousePosition.X, left, right);
      screenMousePosition.Y = MathHelper.Clamp(screenMousePosition.Y, top, bottom);

      Vector2F delta = screenMousePosition - _mouseStartPosition;

      // Undo render transform of screen.
      if (screen.HasRenderTransform)
        delta = screen.RenderTransform.FromRenderDirection(delta);

      if (delta != Vector2F.Zero)
      {
        // ----- Handle ongoing resizing operation.
        if (_isResizing)
        {
          // Resizing only works when there is no render transform or the render
          // transform origin is the top, left corner. Otherwise, it does not work
          // correct because resizing the window simultaneously changes the render 
          // transform!

          // Restore original render transform.
          var transform = new Rendering.RenderTransform(_startPosition, _startSize.X, _startSize.Y, RenderTransformOrigin, RenderScale, RenderRotation, RenderTranslation);

          // Apply delta in local space of window.
          delta = transform.FromRenderDirection(delta);

          // Ensure limits.
          if ((_resizeDirection & ResizeDirection.E) != 0)
          {
            float width = _startSize.X + delta.X;
            width = MathHelper.Clamp(width, MinWidth, MaxWidth);
            delta.X = width - _startSize.X;
          }
          else if ((_resizeDirection & ResizeDirection.W) != 0)
          {
            float width = _startSize.X - delta.X;
            width = MathHelper.Clamp(width, MinWidth, MaxWidth);
            delta.X = _startSize.X - width;
          }
          else
          {
            delta.X = 0;
          }

          if ((_resizeDirection & ResizeDirection.S) != 0)
          {
            float height = _startSize.Y + delta.Y;
            height = MathHelper.Clamp(height, MinHeight, MaxHeight);
            delta.Y = height - _startSize.Y;
          }
          else if ((_resizeDirection & ResizeDirection.N) != 0)
          {
            float height = _startSize.Y - delta.Y;
            height = MathHelper.Clamp(height, MinHeight, MaxHeight);
            delta.Y = _startSize.Y - height;
          }
          else
          {
            delta.Y = 0;
          }

          Vector2F topLeft = _startPosition;
          Vector2F bottomRight = _startPosition + _startSize;

          switch (_resizeDirection)
          {
            case ResizeDirection.N:
            case ResizeDirection.W:
            case ResizeDirection.NW:
              topLeft += delta;
              break;
            case ResizeDirection.NE:
              bottomRight.X += delta.X;
              topLeft.Y += delta.Y;
              break;
            case ResizeDirection.E:
            case ResizeDirection.SE:
            case ResizeDirection.S:
              bottomRight += delta;
              break;
            case ResizeDirection.SW:
              topLeft.X += delta.X;
              bottomRight.Y += delta.Y;
              break;
          }

          topLeft = transform.ToRenderPosition(topLeft);
          bottomRight = transform.ToRenderPosition(bottomRight);

          X = topLeft.X;  // Note: Setting X, Y changes the render transform!
          Y = topLeft.Y;

          transform = RenderTransform;
          topLeft = transform.FromRenderPosition(topLeft);
          bottomRight = transform.FromRenderPosition(bottomRight);
          Width = bottomRight.X - topLeft.X;
          Height = bottomRight.Y - topLeft.Y;

          InvalidateArrange();
          inputService.IsMouseOrTouchHandled = true;
          return;
        }

        // ----- Handle ongoing dragging operation.
        if (_isDragging)
        {
          X = _startPosition.X + delta.X;
          Y = _startPosition.Y + delta.Y;
          InvalidateArrange();
          inputService.IsMouseOrTouchHandled = true;
        }
      }
    }


    private void BackupBounds()
    {
      _originalX = X;
      _originalY = Y;
      _originalWidth = Width;
      _originalHeight = Height;
    }


    private void RestoreBounds()
    {
      X = _originalX;
      Y = _originalY;
      Width = _originalWidth;
      Height = _originalHeight;
    }


    /// <summary>
    /// Gets the window that contains the given <paramref name="control"/>.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>
    /// The window that contains the <paramref name="control"/>, or <see langword="null"/> if
    /// the control is not part of a window (controls can be direct children of the screen, no 
    /// intermediate window is required).
    /// </returns>
    public static Window GetWindow(UIControl control)
    {
      while (control != null)
      {
        var window = control as Window;
        if (window != null)
          return window;

        control = control.VisualParent;
      }

      return null;
    }
    #endregion
  }
}
