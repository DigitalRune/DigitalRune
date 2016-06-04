using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DigitalRune;
using DigitalRune.Diagnostics;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.ServiceLocation;
using DigitalRune.Text;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples
{
  /// <summary>
  /// Manages samples and provides a user interface for switching between samples.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="SampleFramework"/> automatically discovers samples using reflection. Samples
  /// need to be derived from <see cref="GameComponent"/> and marked with the
  /// <see cref="SampleAttribute"/>.
  /// </para>
  /// <para>
  /// <strong>Gamepad Detection:</strong><br/>
  /// The <see cref="SampleFramework"/> controls which gamepad is used for input. To use a gamepad,
  /// the user has to press the Start button on the gamepad. This gamepad is then assigned to a
  /// <see cref="LogicalPlayerIndex"/>. If the connection to the gamepad is lost, the
  /// <see cref="LogicalPlayerIndex"/> assignment is removed. An info text about the gamepad state
  /// is shown in the <see cref="UIScreen"/>.
  /// </para>
  /// <para>
  /// <strong>Mouse Visibility and Mouse Centering:</strong><br/>
  /// Samples can hide the mouse cursor and enable "mouse centering" by setting
  /// <see cref="IsMouseVisible"/> to <see langword="false"/>. "Mouse centering" means that the
  /// mouse position is reset to a fixed position each frame to avoid that the mouse movement is
  /// blocked by the screen borders.
  /// </para>
  /// <para>
  /// <strong>Menu Window:</strong><br/>
  /// Esc (or Back on gamepad) opens the Menu window, which contains a list of all samples.
  /// controls.
  /// </para>
  /// <para>
  /// <strong>Help Window:</strong><br/>
  /// F1 (or left stick on gamepad) opens the Help window, which shows the description of the sample
  /// and a summary of all controls.
  /// </para>
  /// <para>
  /// <strong>Profiler Window:</strong><br/>
  /// F3 opens the profiler window, which contains the profiling data. <strong>
  /// IMPORTANT:</strong> DigitalRune Base profiling classes are only active if the
  /// conditional compilation symbol DIGITALRUNE_PROFILE is defined.
  /// (See: Project Properties | Build | Conditional compilation symbols)
  /// </para>
  /// <para>
  /// <strong>Options Window:</strong><br/>
  /// F4 opens the Options window. Samples can add additional controls to the window.
  /// See <see cref="AddOptions(string,int)"/>.
  /// </para>
  /// </remarks>
  public class SampleFramework
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly SampleGame _game;
    private readonly Type _initialSample;
    private readonly ServiceContainer _services;
    private readonly IInputService _inputService;
    private readonly IGraphicsService _graphicsService;
    private readonly IGameObjectService _gameObjectService;

    // A collection of all automatically discovered samples.
    private List<Type> _samples;

    // The currently loaded sample.
    private Sample _sample;
    private int _sampleIndex = -1;

    // The sample that should be loaded.
    private int _nextSampleIndex = -1;

    // Profiling
    private int _numberOfUpdates;
    private int _numberOfDraws;
    private Stopwatch _stopwatch;

    // UI controls.
    private GuiGraphicsScreen _guiGraphicsScreen;
    private TextBlock _titleTextBlock;
    private StackPanel _infoPanel;
    private TextBlock _controllerTextBlock;
    private StackPanel _buttonsPanel;
    private StackPanel _fpsPanel;
    private TextBlock _updateFpsTextBlock;
    private TextBlock _drawFpsTextBlock;

    private Window _menuWindow;

    private Window _profilerWindow;
    private TextBlock _profilerTextBlock;

    private Window _optionsWindow;
    private TabControl _optionsTabControl;

    private Window _helpWindow;
    private TextBlock _helpTextBlock;

    // Misc
    private readonly StringBuilder _stringBuilder = new StringBuilder();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the menu (or another window) is open.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the menu is visible; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsMenuVisible
    {
      get
      {
        return _menuWindow.IsVisible
               || _profilerWindow.IsVisible
               || _optionsWindow.IsVisible
               || _helpWindow.IsVisible;
      }
    }


    /// <summary>
    /// Gets or sets a value indicating whether the mouse cursor is visible.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the mouse cursor is visible; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsMouseVisible
    {
      get { return _isMouseVisible; }
      set
      {
        _isMouseVisible = value;

        if (value)
        {
          // Make mouse cursor visible immediately.
          _game.IsMouseVisible = true;
          _inputService.EnableMouseCentering = false;
        }
      }
    }
    private bool _isMouseVisible;


    /// <summary>
    /// Gets or sets a value indicating whether user interface is visible or hidden
    /// (e.g. for screenshots).
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the GUI is visible; otherwise, <see langword="false" />.
    /// </value>
    public bool IsGuiVisible
    {
      get { return _guiGraphicsScreen.IsVisible; }
      set { _guiGraphicsScreen.IsVisible = value; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SampleFramework" /> class.
    /// </summary>
    /// <param name="game">The XNA game.</param>
    /// <param name="initialSample">The type of the first sample.</param>
    public SampleFramework(SampleGame game, Type initialSample)
    {
      _game = game;
      _initialSample = initialSample;

      // Get all required services.
      _services = (ServiceContainer)ServiceLocator.Current;
      _inputService = ServiceLocator.Current.GetInstance<IInputService>();
      _graphicsService = _services.GetInstance<IGraphicsService>();
      _gameObjectService = _services.GetInstance<IGameObjectService>();

      InitializeSamples();
      InitializeController();
      InitializeMouse();
      InitializeProfiler();
      InitializeGui();
    }


    private void InitializeSamples()
    {
      // Automatically find all samples using reflection. Samples are derived from 
      // GameComponent and have a SampleAttribute.
#if NETFX_CORE
      _samples = GetType().GetTypeInfo()
                          .Assembly
                          .DefinedTypes
                          .Where(ti => ti.IsSubclassOf(typeof(GameComponent))
                                      && SampleAttribute.GetSampleAttribute(ti.AsType()) != null)
                          .Select(ti => ti.AsType())
                          .ToList();
#else
      _samples = Assembly.GetCallingAssembly()
                         .GetTypes()
                         .Where(t => typeof(GameComponent).IsAssignableFrom(t)
                                     && SampleAttribute.GetSampleAttribute(t) != null)
                         .ToList();
#endif

      _samples.Sort(CompareSamples);

      if (_samples.Count == 0)
        throw new Exception("No samples found.");

      // Set _nextSampleIndex to immediately start a specific sample.
      _nextSampleIndex = _samples.IndexOf(_initialSample);
    }


    private static int CompareSamples(Type sampleA, Type sampleB)
    {
      var sampleAttributeA = SampleAttribute.GetSampleAttribute(sampleA);
      var sampleAttributeB = SampleAttribute.GetSampleAttribute(sampleB);

      // Sort by category...
      var categoryA = sampleAttributeA.Category;
      var categoryB = sampleAttributeB.Category;
      if (categoryA < categoryB)
        return -1;
      if (categoryA > categoryB)
        return +1;

      // ...then by sample order.
      int orderA = sampleAttributeA.Order;
      int orderB = sampleAttributeB.Order;
      return orderA - orderB;
    }


    private void InitializeController()
    {
#if WINDOWS_PHONE || ANDROID
      // Set logical player 1 to first gamepad. On WP the first gamepad is used
      // to check the Windows Phone's Back button.
      _inputService.SetLogicalPlayer(LogicalPlayerIndex.One, PlayerIndex.One);
#endif
    }


    private void InitializeMouse()
    {
      IsMouseVisible = false;

      // Set the mouse centering position to center of the game window.
      var presentationParameters = _game.GraphicsDevice.PresentationParameters;
      _inputService.Settings.MouseCenter = new Vector2F(
        presentationParameters.BackBufferWidth / 2.0f,
        presentationParameters.BackBufferHeight / 2.0f);
    }


    private void InitializeProfiler()
    {
      _stopwatch = Stopwatch.StartNew();

      // Add format/description for profiler values which are captured in Update().
      Profiler.SetFormat("NumBodies", 1, "The number of rigid bodies in the physics simulation.");
      Profiler.SetFormat("NumContacts", 1, "The number of contact constraints in the physics simulation.");
    }


    private void InitializeGui()
    {
      // Add the GuiGraphicsScreen to the graphics service.
      _guiGraphicsScreen = new GuiGraphicsScreen(_services);
      _graphicsService.Screens.Add(_guiGraphicsScreen);

      // ----- Title (top left)
      _titleTextBlock = new TextBlock
      {
        Font = "DejaVuSans",
        Foreground = Color.White,
        Text = "No sample selected",
        X = 10,
        Y = 10,
      };
      _guiGraphicsScreen.UIScreen.Children.Add(_titleTextBlock);

      // ----- Info (bottom left)
      _infoPanel = new StackPanel
      {
        Margin = new Vector4F(10),
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom,
      };
      _guiGraphicsScreen.UIScreen.Children.Add(_infoPanel);
      _controllerTextBlock = new TextBlock
      {
        Font = "DejaVuSans",
        Foreground = Color.White,
        Text = "Controller disconnected. Press <Start> to use controller.",
      };
      _infoPanel.Children.Add(_controllerTextBlock);

#if XBOX
      _buttonsPanel = new StackPanel(); // Not used.
      var infoTextBlock = new TextBlock
      {
        Font = "DejaVuSans",
        Foreground = Color.White,
        Text = "Press <Left Stick> to show/hide help.",
      };
      _infoPanel.Children.Add(infoTextBlock);
      infoTextBlock = new TextBlock
      {
        Font = "DejaVuSans",
        Foreground = Color.White,
        Text = "Press <Back> to show/hide menu.",
      };
      _infoPanel.Children.Add(infoTextBlock);
#else
      // ----- Buttons (bottom right)
      _buttonsPanel = new StackPanel
      {
        Background = new Color(0, 0, 0, 64),
        Padding = new Vector4F(10),
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Bottom,
      };
      _guiGraphicsScreen.UIScreen.Children.Add(_buttonsPanel);
      AddButton(_buttonsPanel, "Previous (PgUp)", LoadPreviousSample);
      AddButton(_buttonsPanel, "Next (PgDn)", LoadNextSample);
      AddButton(_buttonsPanel, "Menu (Esc)", ShowMenuWindow);
      AddButton(_buttonsPanel, "Help (F1)", ShowHelpWindow);
      AddButton(_buttonsPanel, "Profile (F3)", ShowProfilerWindow);
      AddButton(_buttonsPanel, "Options (F4)", ShowOptionsWindow);
#if !NETFX_CORE && !IOS
      AddButton(_buttonsPanel, "Exit (Alt-F4)", _game.Exit);
#endif
#endif

      // ----- FPS Counter (top right)
      _fpsPanel = new StackPanel
      {
        Margin = new Vector4F(10),
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Top,
      };
      _guiGraphicsScreen.UIScreen.Children.Add(_fpsPanel);
      _updateFpsTextBlock = new TextBlock
      {
        Font = "DejaVuSans",
        Foreground = Color.Yellow,
        HorizontalAlignment = HorizontalAlignment.Right,
        Text = "Update",
      };
      _fpsPanel.Children.Add(_updateFpsTextBlock);
      _drawFpsTextBlock = new TextBlock
      {
        Font = "DejaVuSans",
        Foreground = Color.Yellow,
        HorizontalAlignment = HorizontalAlignment.Right,
        Text = "Draw",
      };
      _fpsPanel.Children.Add(_drawFpsTextBlock);

      // Create windows. (Hidden at start.)
      CreateMenuWindow();
      CreateProfilerWindow();
      CreateOptionsWindow();
      CreateHelpWindow();
    }


    private void AddButton(StackPanel panel, string text, Action clickHandler)
    {
      var button = new Button
      {
        Content = new TextBlock { Text = text },
        HorizontalAlignment = HorizontalAlignment.Stretch,
        MinWidth = 100
      };

      if (panel.Children.Count > 0)
        button.Margin = (panel.Orientation == Orientation.Horizontal) ? new Vector4F(3, 0, 0, 0) : new Vector4F(0, 3, 0, 0);

#if WINDOWS_PHONE || ANDROID || IOS
      button.Padding = new Vector4F(15);
#endif

      if (clickHandler != null)
        button.Click += (s, e) => clickHandler();

      panel.Children.Add(button);
    }


    private void CreateMenuWindow()
    {
      if (_menuWindow != null)
        return;

      // Window
      //   StackPanel (vertical)
      //     StackPanel (horizontal)
      //       Buttons
      //     TextBlock 
      //     TabControl
      //       TabItem                      * per category
      //         ScrollViewer
      //           StackPanel (vertical)
      //             StackPanel (vertical)  * per sample
      //               Button
      //               TextBlock

      _menuWindow = new Window
      {
        Name = "MenuWindow",
        Title = "Sample Browser",
        X = 50,
        Y = 50,
        IsVisible = false,
        HideOnClose = true
      };
      _menuWindow.Closed += OnWindowClosed;

      var panel = new StackPanel
      {
        Margin = new Vector4F(10),
        Orientation = Orientation.Vertical,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch
      };
      _menuWindow.Content = panel;

      var buttonsPanel = new StackPanel
      {
        Orientation = Orientation.Horizontal
      };
      panel.Children.Add(buttonsPanel);

      AddButton(buttonsPanel, "Resume (Esc)", CloseWindows);
      AddButton(buttonsPanel, "Help (F1)", ShowHelpWindow);
      AddButton(buttonsPanel, "Profile (F3)", ShowProfilerWindow);
      AddButton(buttonsPanel, "Options (F4)", ShowOptionsWindow);
#if !NETFX_CORE && !IOS
      AddButton(buttonsPanel, "Exit (Alt-F4)", _game.Exit);
#endif

      var label = new TextBlock
      {
        Text = "Select Sample:",
        Margin = new Vector4F(2, 10, 0, 0)
      };
      panel.Children.Add(label);

      var tabControl = new TabControl
      {
        Margin = new Vector4F(0, 3, 0, 0),
        Width = 580,
#if WINDOWS_PHONE || ANDROID || IOS
        Height = 300
#else
        Height = 400
#endif
      };
      panel.Children.Add(tabControl);

      // Each tab shows a sample category (Base, Mathematics, Geometry, ...).
      var samplesByCategory = _samples.GroupBy(t => SampleAttribute.GetSampleAttribute(t).Category);
      int category = -1;
      foreach (var grouping in samplesByCategory)
      {
        category++;

        var tabItem = new TabItem
        {
          Content = new TextBlock { Text = grouping.Key.ToString() },
        };
        tabControl.Items.Add(tabItem);

        var scrollViewer = new ScrollViewer
        {
          HorizontalAlignment = HorizontalAlignment.Stretch,
          VerticalAlignment = VerticalAlignment.Stretch,
          HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        tabItem.TabPage = scrollViewer;

        var itemsPanel = new StackPanel
        {
          Orientation = Orientation.Vertical,
          Margin = new Vector4F(5),
          HorizontalAlignment = HorizontalAlignment.Stretch,
          VerticalAlignment = VerticalAlignment.Stretch
        };
        scrollViewer.Content = itemsPanel;

        foreach (Type sample in grouping)
        {
          var item = new StackPanel
          {
            Orientation = Orientation.Vertical
          };
          itemsPanel.Children.Add(item);

          string title = sample.Name;
          var button = new Button
          {
            Content = new TextBlock { Text = title },
            Width = 200,
#if WINDOWS_PHONE || ANDROID || IOS
            Padding = new Vector4F(10),
#else
            Padding = new Vector4F(0, 5, 0, 5),
#endif
          };
          int sampleIndex = _samples.IndexOf(sample);
          button.Click += (s, e) =>
                          {
                            _nextSampleIndex = sampleIndex;
                            CloseWindows();
                          };
          item.Children.Add(button);

          string summary = SampleAttribute.GetSampleAttribute(sample).Summary;
          summary = summary.Replace("\r\n", " ");
          var summaryTextBlock = new TextBlock
          {
            Margin = new Vector4F(0, 3, 0, 12),
            Text = summary,
            WrapText = true
          };
          item.Children.Add(summaryTextBlock);
        }
      }
    }


    private void CreateProfilerWindow()
    {
      if (_profilerWindow != null)
        return;

      // Window
      //   ScrollViewer
      //     TextView

      _profilerTextBlock = new TextBlock
      {
        // Text is set in UpdateHelpText().
        Font = "Console",
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
      };

      var scrollViewer = new ScrollViewer
      {
        Content = _profilerTextBlock,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
      };

      _profilerWindow = new Window
      {
        Name = "ProfilerWindow",
        Title = "Profile",
        Content = scrollViewer,
        X = 50,
        Y = 50,
        Width = 640,
        Height = 480,
        CanResize = true,
        HideOnClose = true,
        IsVisible = false,
      };
      _profilerWindow.Closed += OnWindowClosed;
    }


    private void CreateOptionsWindow()
    {
      if (_optionsWindow != null)
        return;

      // Add the Options window (v-sync, fixed/variable timing, parallel game loop).

      // Window
      //   TabControl

      _optionsWindow = new Window
      {
        Name = "OptionsWindow",
        Title = "Options",
        X = 50,
        Y = 50,
        Width = 400,
        MaxHeight = 640,
        HideOnClose = true,
        IsVisible = false,
      };
      _optionsWindow.Closed += OnWindowClosed;

      _optionsTabControl = new TabControl
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        Margin = new Vector4F(SampleHelper.Margin),
      };
      _optionsWindow.Content = _optionsTabControl;

      var panel = SampleHelper.AddTabItem(_optionsTabControl, "General");
      var graphicsDeviceManager = _services.GetInstance<GraphicsDeviceManager>();
      SampleHelper.AddCheckBox(
        panel,
        "Use fixed frame rate",
        _game.IsFixedTimeStep,
        value => _game.IsFixedTimeStep = value);

      SampleHelper.AddCheckBox(
        panel,
        "Enable V-Sync",
        graphicsDeviceManager.SynchronizeWithVerticalRetrace,
        value =>
        {
          graphicsDeviceManager.SynchronizeWithVerticalRetrace = value;
          graphicsDeviceManager.ApplyChanges();
        });

      SampleHelper.AddCheckBox(
        panel,
        "Enable parallel game loop",
        _game.EnableParallelGameLoop,
        value => _game.EnableParallelGameLoop = value);

      SampleHelper.AddButton(
        panel,
        "GC.Collect()",
        GC.Collect,
        "Force an immediate garbage collection.");
    }


    private void CreateHelpWindow()
    {
      if (_helpWindow != null)
        return;

      // Window
      //   ScrollViewer
      //     TextBlock

      _helpTextBlock = new TextBlock(); // Text is set in UpdateHelpText().

      var scrollViewer = new ScrollViewer
      {
        Content = _helpTextBlock,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
      };

      _helpWindow = new Window
      {
        Name = "HelpWindow",
        Title = "HELP",
        Content = scrollViewer,
        X = 50,
        Y = 50,
        Width = 640,
        Height = 480,
        CanResize = true,
        HideOnClose = true,
        IsVisible = false,
      };
      _helpWindow.Closed += OnWindowClosed;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Updates the sample framework. (Needs to be called in <see cref="Game.Update"/>.)
    /// </summary>
    public void Update()
    {
      _numberOfUpdates++;

      UpdateController();
      UpdateMouse();
      UpdateProfiler();

      if (_nextSampleIndex != _sampleIndex)
      {
        LoadSample(_nextSampleIndex);
        return;
      }

      if (!_inputService.IsKeyboardHandled)
      {
        if (_inputService.IsPressed(Keys.Escape, false))
        {
          _inputService.IsKeyboardHandled = true;
          if (IsMenuVisible)
            CloseWindows();
          else
            ShowMenuWindow();
        }
        else if (_inputService.IsPressed(Keys.F1, false))
        {
          _inputService.IsKeyboardHandled = true;
          if (_helpWindow.IsVisible)
            CloseWindows();
          else
            ShowHelpWindow();
        }
        else if (_inputService.IsPressed(Keys.F3, false))
        {
          _inputService.IsKeyboardHandled = true;
          if (_profilerWindow.IsVisible)
            CloseWindows();
          else
            ShowProfilerWindow();
        }
        else if (_inputService.IsPressed(Keys.F4, false))
        {
          _inputService.IsKeyboardHandled = true;
          if (_optionsWindow.IsVisible)
            CloseWindows();
          else
            ShowOptionsWindow();
        }
        else if (_inputService.IsPressed(Keys.F7, false))
        {
          _inputService.IsKeyboardHandled = true;
          IsGuiVisible = !IsGuiVisible;
        }
      }

      if (!_inputService.IsGamePadHandled(LogicalPlayerIndex.Any))
      {
        if (_inputService.IsPressed(Buttons.Back, false, LogicalPlayerIndex.One))
        {
          _inputService.SetGamePadHandled(LogicalPlayerIndex.Any, true);
          if (IsMenuVisible)
            CloseWindows();
          else
            ShowMenuWindow();
        }
        else if (_inputService.IsPressed(Buttons.LeftStick, false, LogicalPlayerIndex.Any))
        {
          _inputService.SetGamePadHandled(LogicalPlayerIndex.Any, true);
          if (_helpWindow.IsVisible)
            CloseWindows();
          else
            ShowHelpWindow();
        }
      }

#if !WINDOWS_PHONE && !ANDROID && !IOS
      if (!_inputService.IsKeyboardHandled
         // Ignore game pad handled flag otherwise some samples cannot be switched.
         // && !_inputService.IsGamePadHandled(LogicalPlayerIndex.Any)   
         )
      {
        // If <Keyboard End> or <LeftShoulder>+<RightShoulder> is pressed, the current sample restarts.
        if (_inputService.IsPressed(Keys.End, true) || _inputService.IsPressed(Buttons.LeftShoulder | Buttons.RightShoulder, true, LogicalPlayerIndex.One))
          RestartSample();

        // If <Keyboard PageUp> is pressed or <D-pad Left>, the previous sample is loaded.
        if (_inputService.IsPressed(Keys.PageUp, true) || _inputService.IsPressed(Buttons.DPadLeft, true, LogicalPlayerIndex.One))
          LoadPreviousSample();

        // If <Keyboard PageDown> is pressed or <D-pad Right>, the next sample is loaded.
        if (_inputService.IsPressed(Keys.PageDown, true) || _inputService.IsPressed(Buttons.DPadRight, true, LogicalPlayerIndex.One))
          LoadNextSample();
      }
#endif
    }


    /// <summary>
    /// Increments the FPS counter. (Needs to be called in the <see cref="Game.Draw"/>.)
    /// </summary>
    public void Draw()
    {
      _numberOfDraws++;
    }


    /// <summary>
    /// Shows the Menu window.
    /// </summary>
    private void ShowMenuWindow()
    {
      CloseWindows();
      _menuWindow.Show(_guiGraphicsScreen.UIScreen);
      _buttonsPanel.IsVisible = false;
      _guiGraphicsScreen.HideBackground = true;
    }


    /// <summary>
    /// Shows the Profiler window.
    /// </summary>
    public void ShowProfilerWindow()
    {
      DumpProfiler();
      CloseWindows();
      _profilerWindow.Show(_guiGraphicsScreen.UIScreen);
      _buttonsPanel.IsVisible = false;
      _guiGraphicsScreen.HideBackground = true;
    }


    /// <summary>
    /// Shows the Options window.
    /// </summary>
    public void ShowOptionsWindow()
    {
      ShowOptionsWindow(null);
    }


    /// <summary>
    /// Shows the Options window.
    /// </summary>
    /// <param name="tabItem">
    /// The name of the tab item to show. Can be <see langword="null"/> or empty.
    /// </param>
    public void ShowOptionsWindow(string tabItem)
    {
      CloseWindows();
      _optionsWindow.Show(_guiGraphicsScreen.UIScreen);
      var tab = _optionsTabControl.Items.FirstOrDefault(t => t.Name == tabItem);
      if (tab != null)
        _optionsTabControl.Select(tab);

      _buttonsPanel.IsVisible = true;
      _guiGraphicsScreen.HideBackground = false;
    }


    /// <summary>
    /// Shows the Help window.
    /// </summary>
    public void ShowHelpWindow()
    {
      CloseWindows();
      _helpWindow.Show(_guiGraphicsScreen.UIScreen);
      _buttonsPanel.IsVisible = false;
      _guiGraphicsScreen.HideBackground = true;
    }


    /// <summary>
    /// Closes all windows.
    /// </summary>
    public void CloseWindows()
    {
      _menuWindow.Close();
      _profilerWindow.Close();
      _optionsWindow.Close();
      _helpWindow.Close();

      _buttonsPanel.IsVisible = true;
      _guiGraphicsScreen.HideBackground = false;
    }


    private void OnWindowClosed(object sender, EventArgs eventArgs)
    {
      // A window was closed by clicking the Close button.
      // --> Call CloseWindows() which restores the other controls.
      CloseWindows();
    }


    /// <summary>
    /// Loads the previous sample.
    /// </summary>
    public void LoadPreviousSample()
    {
      if (_sampleIndex <= 0)
        _nextSampleIndex = _samples.Count - 1;
      else
        _nextSampleIndex = _sampleIndex - 1;
    }


    /// <summary>
    /// Loads the next sample.
    /// </summary>
    public void LoadNextSample()
    {
      if (_sampleIndex >= _samples.Count - 1)
        _nextSampleIndex = 0;
      else
        _nextSampleIndex = _sampleIndex + 1;
    }


    private void LoadSample(int sampleIndex)
    {
      EndSample();
      CloseWindows();

      var type = _samples[sampleIndex];

      // Use the service container to create an instance of the sample. The service
      // container will automatically provide the sample constructor with the necessary
      // arguments.
      try
      {
        _sample = (Sample)_services.CreateInstance(type);
      }
      catch (TargetInvocationException exception)
      {
        // Samples are created using reflection. If the sample constructor throws an exception,
        // we land here.

        // Throw inner exception, which is more useful.
        throw exception.InnerException;

        // To break at the actual code line which caused the exception, you can tell the Visual 
        // studio debugger to break at "first chance" exceptions:
        // Open the exception settings in Visual Studio using the menu
        //   Debug > Windows > Exception Settings (VS 2015)
        // or 
        //   Debug > Exceptions... (VS 2013 and older)
        // and check the check box next to "Common Language Runtime Exceptions".
        // If you run the game again, then the debugger should break where the
        // inner exception is created and not on this line.
      }

      _sampleIndex = sampleIndex;
      _nextSampleIndex = sampleIndex;
      _game.Components.Add(_sample);

      UpdateHelp();

      if (_optionsTabControl.Items.Count > 1)
        _optionsTabControl.SelectedIndex = _optionsTabControl.Items.Count - 1;
    }


    private void EndSample()
    {
      if (_sample != null)
      {
        _game.Components.Remove(_sample);
        _sample.Dispose();
        _sample = null;

        ResetOptions();

        Profiler.ClearAll();
        ResourcePool.ClearAll();
        GC.Collect();
      }
    }


    private void RestartSample()
    {
      LoadSample(_sampleIndex);
    }


    private void UpdateController()
    {
#if !WINDOWS_PHONE && !ANDROID && !IOS
      // Check if controller is connected.
      bool controllerIsConnected = _inputService.GetGamePadState(LogicalPlayerIndex.One).IsConnected;
      if (!controllerIsConnected)
      {
        // No controller assigned to LogicalPlayerIndex.One or controller was disconnected.
        // Reset the logical player assignment.
        _inputService.SetLogicalPlayer(LogicalPlayerIndex.One, null);

        // Check if the user presses <Start> on any connected gamepad and assign a new
        // controller to the first logical player.
        for (int i = 0; i < _inputService.MaxNumberOfPlayers; i++)
        {
          var controller = (PlayerIndex)i;
          if (_inputService.IsDown(Buttons.Start, controller))
          {
            _inputService.SetGamePadHandled(controller, true);
            _inputService.SetLogicalPlayer(LogicalPlayerIndex.One, controller);
            controllerIsConnected = true;
            break;
          }
        }
      }

      // Show/hide message text.
      _controllerTextBlock.IsVisible = !controllerIsConnected;
#endif
    }


    private void UpdateMouse()
    {
      // Update mouse visibility and mouse centering:
      bool isMouseVisible = IsMouseVisible || IsMenuVisible || !_game.IsActive;
      _game.IsMouseVisible = isMouseVisible;
      _inputService.EnableMouseCentering = !isMouseVisible;
    }


    private void UpdateProfiler()
    {
      // Show "Update FPS" and "Draw FPS" in upper right corner.
      // (The data is updated every ~0.5 s.)
      if (_stopwatch.Elapsed.TotalSeconds > 0.5)
      {
        {
          _stringBuilder.Clear();
          _stringBuilder.Append("Update: ");
          float fps = (float)Math.Round(_numberOfUpdates / _stopwatch.Elapsed.TotalSeconds);
          _stringBuilder.AppendNumber((int)fps);
          _stringBuilder.Append(" fps, ");
          _stringBuilder.AppendNumber(1 / fps * 1000, 2, AppendNumberOptions.None);
          _stringBuilder.Append(" ms");
          _updateFpsTextBlock.Text = _stringBuilder.ToString();
        }
        {
          _stringBuilder.Clear();
          _stringBuilder.Append("Draw: ");
          float fps = (float)Math.Round(_numberOfDraws / _stopwatch.Elapsed.TotalSeconds);
          _stringBuilder.AppendNumber((int)fps);
          _stringBuilder.Append(" fps, ");
          _stringBuilder.AppendNumber(1 / fps * 1000, 2, AppendNumberOptions.None);
          _stringBuilder.Append(" ms");
          _drawFpsTextBlock.Text = _stringBuilder.ToString();
        }

        _numberOfUpdates = 0;
        _numberOfDraws = 0;
        _stopwatch.Reset();
        _stopwatch.Start();
      }

      // Capture general interesting info.
      var simulation = _services.GetInstance<Simulation>();
      Profiler.AddValue("NumBodies", simulation.RigidBodies.Count);
      Profiler.AddValue("NumContacts", simulation.ContactConstraints.Count);
    }


    private void DumpProfiler()
    {
      _stringBuilder.Clear();
      _stringBuilder.AppendLine("PROFILE\n\n");
#if DIGITALRUNE_PROFILE
      // Dump profiler.
      _stringBuilder.AppendLine("-------------------------------------------------------------------------------");
      _stringBuilder.AppendLine("Profiler:");
      _stringBuilder.AppendLine("-------------------------------------------------------------------------------");
      _stringBuilder.AppendLine(Profiler.DumpAll());
      Profiler.ClearAll();

      // Dump all Hierarchical Profilers.
      var hierarchicalProfilers = _services.GetAllInstances<HierarchicalProfiler>();
      foreach (var hierarchicalProfiler in hierarchicalProfilers)
      {
        _stringBuilder.AppendLine("");
        _stringBuilder.AppendLine("-------------------------------------------------------------------------------");
        _stringBuilder.AppendLine("Hierarchical Profilers:");
        _stringBuilder.AppendLine("-------------------------------------------------------------------------------");
        _stringBuilder.AppendLine(hierarchicalProfiler.Dump(hierarchicalProfiler.Root, int.MaxValue));
        _stringBuilder.AppendLine("");
        hierarchicalProfiler.Reset();
      }
#else
      _stringBuilder.Append("Profiling is disabled. To enable profiling, define the conditional compilation symbol 'DIGITALRUNE_PROFILE' in the project.");
#endif

      _profilerTextBlock.Text = _stringBuilder.ToString();
    }


    private void ResetOptions()
    {
      for (int i = _optionsTabControl.Items.Count - 1; i >= 0; i--)
        if (_optionsTabControl.Items[i].Name != "General")
          _optionsTabControl.Items.RemoveAt(i);
    }


    /// <summary>
    /// Returns a panel for adding controls to the Options window.
    /// </summary>
    /// <param name="tabName">The name of the tab page.</param>
    /// <returns>
    /// The panel that hosts the UI controls in the Options window.
    /// Add the new controls to this panel!
    /// </returns>
    public Panel AddOptions(string tabName)
    {
      return AddOptions(tabName, -1);
    }


    /// <summary>
    /// Returns a panel for adding controls to the Options window.
    /// </summary>
    /// <param name="tabName">The name of the tab page.</param>
    /// <param name="tabIndex">The index of the tab page.</param>
    /// <returns>
    /// The panel that hosts the UI controls in the Options window.
    /// Add the new controls to this panel!
    /// </returns>
    public Panel AddOptions(string tabName, int tabIndex)
    {
      // Check if tab control already contains the specified tab.
      var optionsPanel = _optionsTabControl.Items
                                           .Where(tabItem => tabItem.Name == tabName)
                                           .Select(tabItem => (Panel)(((ScrollViewer)tabItem.TabPage).Content))
                                           .FirstOrDefault();
      if (optionsPanel == null)
        optionsPanel = SampleHelper.AddTabItem(_optionsTabControl, tabName, tabIndex);

      return optionsPanel;
    }


    private void UpdateHelp()
    {
      string name = null;
      SampleCategory category = SampleCategory.Unsorted;
      string summary = null;
      string description = null;
      if (_sample != null)
      {
        var sampleType = _sample.GetType();
        var sampleAttribute = SampleAttribute.GetSampleAttribute(sampleType);
        if (sampleAttribute != null)
        {
          name = sampleType.Name;
          category = sampleAttribute.Category;
          summary = sampleAttribute.Summary;
          description = sampleAttribute.Description;
        }
      }

      // Create help text for the sample.
      string helpText =
        "Sample: " + (string.IsNullOrEmpty(name) ? "No sample selected" : name) +
        "\nCategory: " + category +
        "\n" +
        "\nDescription:" +
        "\n--------------" +
        "\n" + (string.IsNullOrEmpty(summary) ? "-" : summary);

      if (!string.IsNullOrEmpty(description))
        helpText += "\n\n" + description;

      // General controls.
      helpText +=
        "\n\nControls:" +
        "\n-----------" +
        "\nGeneral" +
        "\n  Press <Esc> or <Back> to show menu." +
        "\n  Press <F1> or <Left Stick> to show help." +
        "\n  Press <F3> to show profiling data (and also reset the data)." +
        "\n  Press <F4> to show options." +
        "\n  Press <F7> to hide GUI." +
        "\n  Press <PageUp/Down> or <DPad Left/Right> to switch samples." +
        "\n  Press <Keyboard End> or <Left Shoulder>+<Right Shoulder> to restart sample." +
        "\n  Press <P> to pause/resume physics and particle simulation." +
        "\n  Press <T> to single step physics and particle simulation when simulation is paused.";

      // Controls of sample.
      if (_sample != null)
        foreach (var controlsAttribute in ControlsAttribute.GetControlsAttribute(_sample.GetType()))
          helpText += "\n\n" + controlsAttribute.Description;

      // Controls of game objects.
      foreach (var gameObject in _gameObjectService.Objects)
        foreach (var controlsAttribute in ControlsAttribute.GetControlsAttribute(gameObject.GetType()))
          helpText += "\n\n" + controlsAttribute.Description;

      _titleTextBlock.Text = (_sample != null) ? category + " - " + name : name;
      _helpTextBlock.Text = helpText;
    }
    #endregion
  }
}
