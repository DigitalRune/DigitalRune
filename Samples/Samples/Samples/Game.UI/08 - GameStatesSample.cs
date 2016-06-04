using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Game.Input;
using DigitalRune.Game.States;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Game.UI
{
  [Sample(SampleCategory.GameUI,
    @"This sample shows to how create a simple game menu with a custom UI theme, a state machine 
and animations.",
    @"The primary purpose of this sample is to demonstrate the use of the StateMachine provided 
by DigitalRune Game. The user interface is created using DigitalRune Game UI. Transitions 
between screens are implemented using DigitalRune Animation.

The StateMachine is used to manage several game screens:
- Initially, when the game starts, a 'Loading' screen is shown.
- When all assets are loaded a 'Start' screen is shown.
- A 'Menu' and 'Sub Menu' screen represents the game's main menu.
- A 'Game' screen is rendered as a placeholder instead of any actual gameplay.

Note: The GameMenuSample and the GameStatesSample solve a similar problem. The GameMenuSample 
uses GameComponents to represents the game states. The GameStatesSample uses a single 
GameComponent with a StateMachine.",
    8)]
  public class GameStatesSample : Sample
  {
    // The state machine keeps track of the current state and manages transitions 
    // between states.
    private StateMachine _stateMachine;

    // The UI screen renders our controls, such as text labels, buttons, etc.
    private readonly UIScreen _uiScreen;


    public GameStatesSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add a DelegateGraphicsScreen as the first graphics screen to the graphics
      // service. This lets us do the rendering in the Render method of this class.
      var graphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, graphicsScreen);

      // Load a UI theme, which defines the appearance and default values of UI controls.
      Theme theme = ContentManager.Load<Theme>("UI Themes/GameStatesSample/Theme");

      // Create a UI renderer, which uses the theme info to renderer UI controls.
      UIRenderer renderer = new UIRenderer(Game, theme);

      // Create a UIScreen and add it to the UI service. The screen is the root of the 
      // tree of UI controls. Each screen can have its own renderer.
      _uiScreen = new UIScreen("SampleUIScreen", renderer)
      {
        // Make the screen transparent.
        Background = new Color(0, 0, 0, 0),
      };
      UIService.Screens.Add(_uiScreen);

      CreateStateMachine();
    }


    private void CreateStateMachine()
    {
      // Let's create a state machine that manages all states and transitions.
      _stateMachine = new StateMachine();

      // ----- First we need to define all possible states of the game component:

      // The initial state is the "Loading" state. In this state a "Loading..."
      // text is rendered and all required assets are loaded in the background.
      var loadingState = new State { Name = "Loading" };
      loadingState.Enter += OnEnterLoadingScreen;
      loadingState.Exit += OnExitLoadingScreen;

      // The second state is the "Start" state. Once loading is finished the text
      // "Press Start button" is shown until the users presses the Start button
      // on the gamepad.
      var startState = new State { Name = "Start" };
      startState.Enter += OnEnterStartScreen;
      startState.Update += OnUpdateStartScreen;
      startState.Exit += OnExitStartScreen;

      // The "Menu" state represents the main menu. It provides buttons to start 
      // the game, show sub menus, and exit the game.
      var menuState = new State { Name = "Menu" };
      menuState.Enter += OnEnterMenuScreen;
      menuState.Exit += OnExitMenuScreen;

      // The "SubMenu" is just a dummy menu containing a few buttons. It is shown
      // whenever a sub menu is selected in the main menu.
      var subMenuState = new State { Name = "SubMenu" };
      subMenuState.Enter += OnEnterSubMenuScreen;
      subMenuState.Update += OnUpdateSubMenuScreen;
      subMenuState.Exit += OnExitSubMenuScreen;

      // The "Game" state is a placeholder for the actual game content.
      var gameState = new State { Name = "Game" };
      gameState.Enter += OnEnterGameScreen;
      gameState.Update += OnUpdateGameScreen;
      gameState.Exit += OnExitGameScreen;

      // Register the states in the state machine.
      _stateMachine.States.Add(loadingState);
      _stateMachine.States.Add(startState);
      _stateMachine.States.Add(menuState);
      _stateMachine.States.Add(subMenuState);
      _stateMachine.States.Add(gameState);

      // Optional: Define the initially selected state.
      // (If not set explicitly then the first state in the list is used as the 
      // initial state.)
      _stateMachine.States.InitialState = loadingState;

      // ----- Next we can define the allowed transitions between states.

      // The "Loading" screen will transition to the "Start" screen once all assets
      // are loaded. The assets are loaded in the background. The background worker 
      // sets the flag _allAssetsLoaded when it has finished.
      // The transition should fire automatically. To achieve this we can set FireAlways 
      // to true and define a Guard. A Guard is a condition that needs to be fulfilled 
      // to enable the transition. This way the game component automatically switches 
      // from the "Loading" state to the "Start" state once the loading is complete.
      var loadToStartTransition = new Transition
      {
        Name = "LoadingToStart",
        TargetState = startState,
        FireAlways = true,                // Always trigger the transition, if the guard allows it.
        Guard = () => _allAssetsLoaded,   // Enable the transition when _allAssetsLoaded is true.
      };
      loadingState.Transitions.Add(loadToStartTransition);

      // The remaining transition need to be triggered manually.

      // The "Start" screen will transition to the "Menu" screen.
      var startToMenuTransition = new Transition
      {
        Name = "StartToMenu",
        TargetState = menuState,
      };
      startState.Transitions.Add(startToMenuTransition);

      // The "Menu" screen can transition to the "Game" screen or to the "SubMenu".
      var menuToGameTransition = new Transition
      {
        Name = "MenuToGame",
        TargetState = gameState,
      };
      var menuToSubMenuTransition = new Transition
      {
        Name = "MenuToSubMenu",
        TargetState = subMenuState,
      };
      menuState.Transitions.Add(menuToGameTransition);
      menuState.Transitions.Add(menuToSubMenuTransition);

      // The "Game" screen will transition back to the "Menu" screen.
      var gameToMenuTransition = new Transition
      {
        Name = "GameToMenu",
        TargetState = menuState,
      };
      gameState.Transitions.Add(gameToMenuTransition);

      // The "SubMenu" can transition to back to the "Menu" screen.
      var subMenuToMenuTransition = new Transition
      {
        Name = "SubMenuToMenu",
        TargetState = menuState,
      };
      subMenuState.Transitions.Add(subMenuToMenuTransition);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Remove UIScreen from UI service.
        UIService.Screens.Remove(_uiScreen);
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // Update the state machine. (Everything else is handled by the currently active state.)
      _stateMachine.Update(gameTime.ElapsedGameTime);
    }


    private void Render(RenderContext context)
    {
      // Clear background.
      context.GraphicsService.GraphicsDevice.Clear(new Color(50, 50, 50));

      // Draw the UI screen. 
      _uiScreen.Draw(context.DeltaTime);
    }


    #region ----- Loading State -----

    private TextBlock _loadingTextBlock;    // Shows the text "Loading...".
    private volatile bool _allAssetsLoaded; // Will be set to true, when the background thread is finished.


    /// <summary>
    /// Called when "Loading" state is entered.
    /// </summary>
    private void OnEnterLoadingScreen(object sender, StateEventArgs eventArgs)
    {
      // Show the text "Loading..." centered on the screen.
      _loadingTextBlock = new TextBlock
      {
        Name = "LoadingTextBlock",    // Control names are optional - but very helpful for debugging!
        Text = "Loading...",
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
      };
      _uiScreen.Children.Add(_loadingTextBlock);

      // Start loading assets in the background.
      Parallel.StartBackground(LoadAssets);
    }


    /// <summary>
    /// Loads all required assets.
    /// </summary>
    private void LoadAssets()
    {
      // To simulate a loading process we simply wait for 2 seconds.
#if NETFX_CORE
      System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(2)).Wait();
#else
      System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
#endif

      // Set flag to enable the transition from "Loading" state to "Start" state.
      _allAssetsLoaded = true;
    }


    /// <summary>
    /// Called when "Loading" state is exited.
    /// </summary>
    private void OnExitLoadingScreen(object sender, StateEventArgs eventArgs)
    {
      // Clean up.
      _uiScreen.Children.Remove(_loadingTextBlock);
      _loadingTextBlock = null;
    }
    #endregion


    #region ----- Start State -----

    private TextBlock _startTextBlock;                    // Shows the text "Press Start button".
    private bool _exitAnimationIsPlaying;                 // true if fade-out animation is playing.
    private AnimationController _exitAnimationController; // Controls the fade-out animation.


    /// <summary>
    /// Called when "Start" state is entered.
    /// </summary>
    private void OnEnterStartScreen(object sender, StateEventArgs eventArgs)
    {
      // Show the "Press Start button" text centered on the screen.
      _startTextBlock = new TextBlock
      {
        Name = "StartTextBlock",
        Text = "Press Start button",
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
      };
      _uiScreen.Children.Add(_startTextBlock);

      // The text should pulse to indicate that a user interaction is required.
      // To achieve this we can animate the opacity of the TextBlock.
      var opacityAnimation = new SingleFromToByAnimation
      {
        From = 1,                             // Animate from opaque (Opacity == 1)
        To = 0.25f,                           // to nearly transparent (Opacity == 0.25)
        Duration = TimeSpan.FromSeconds(0.5), // over a duration of 0.5 seconds.
        EasingFunction = new SineEase { Mode = EasingMode.EaseInOut }
      };

      // A SingleFromToByAnimation plays only once, but the animation should be 
      // played back-and-forth until the user presses a button.
      // We need wrap the SingleFromToByAnimation in an AnimationClip or TimelineClip.
      // Animation clips can be used to cut and loop other animations.
      var loopingOpacityAnimation = new AnimationClip<float>(opacityAnimation)
      {
        LoopBehavior = LoopBehavior.Oscillate,  // Play back-and-forth.
        Duration = TimeSpan.MaxValue            // Loop forever.
      };

      // We want to apply the animation to the "Opacity" property of the TextBlock.
      // All "game object properties" of a UIControl can be made "animatable".      
      // First, get a handle to the "Opacity" property.
      var opacityProperty = _startTextBlock.Properties.Get<float>(TextBlock.OpacityPropertyId);

      // Then cast the "Opacity" property to an IAnimatableProperty. 
      var animatableOpacityProperty = opacityProperty.AsAnimatable();

      // Start the pulse animation.
      var animationController = AnimationService.StartAnimation(loopingOpacityAnimation, animatableOpacityProperty);

      // Enable "automatic recycling". This step is optional. It ensures that the
      // associated resources are recycled when either the animation is stopped or
      // the target object (the TextBlock) is garbage collected.
      // (The associated resources will be reused by future animations, which will
      // reduce the number of required memory allocations at runtime.)
      animationController.AutoRecycle();
    }


    /// <summary>
    /// Called every frame when "Start" state is active.
    /// </summary>
    private void OnUpdateStartScreen(object sender, StateEventArgs eventArgs)
    {
      if (_exitAnimationIsPlaying)
        return;

      bool transitionToMenu = false;

      // Check if the user presses A or START on any connected gamepad.
      for (var controller = PlayerIndex.One; controller <= PlayerIndex.Four; controller++)
      {
        if (InputService.IsDown(Buttons.A, controller) || InputService.IsDown(Buttons.Start, controller))
        {
          // A or START was pressed. Assign this controller to the first "logical player".
          InputService.SetLogicalPlayer(LogicalPlayerIndex.One, controller);
          transitionToMenu = true;
        }
      }

      if (InputService.IsDown(MouseButtons.Left)
          || InputService.IsDown(Keys.Enter)
          || InputService.IsDown(Keys.Escape)
          || InputService.IsDown(Keys.Space))
      {
        // The users has pressed the left mouse button or a key on the keyboard.

        if (!InputService.GetLogicalPlayer(LogicalPlayerIndex.One).HasValue)
        {
          // No controller has been assigned to the first "logical player". Maybe 
          // there is no gamepad connected.
          // --> Just guess which controller is the primary player and continue.
          InputService.SetLogicalPlayer(LogicalPlayerIndex.One, PlayerIndex.One);
        }

        transitionToMenu = true;
      }

      if (transitionToMenu)
      {
        // Play a fade-out animation which changes the opacity from its current 
        // value to 0.
        var fadeOutAnimation = new SingleFromToByAnimation
        {
          To = 0,                                // Animate the opacity from the current value to 0
          Duration = TimeSpan.FromSeconds(0.5),  // over a duration of 0.5 seconds.
        };
        var opacityProperty = _startTextBlock.Properties.Get<float>(TextBlock.OpacityPropertyId).AsAnimatable();
        _exitAnimationController = AnimationService.StartAnimation(fadeOutAnimation, opacityProperty);

        // When the fade-out animation finished trigger the transition from the "Start" 
        // screen to the "Menu" screen.
        _exitAnimationController.Completed += (s, e) => _stateMachine.States.ActiveState.Transitions["StartToMenu"].Fire();

        _exitAnimationIsPlaying = true;
      }
    }


    /// <summary>
    /// Called when "Start" state is exited.
    /// </summary>
    private void OnExitStartScreen(object sender, StateEventArgs eventArgs)
    {
      // Clean up.
      _exitAnimationController.Stop();
      _exitAnimationController.Recycle();
      _exitAnimationIsPlaying = false;

      _uiScreen.Children.Remove(_startTextBlock);
      _startTextBlock = null;
    }
    #endregion


    #region ----- Menu State -----

    private Window _menuWindow;
    private AnimationController _menuExitAnimationController;


    /// <summary>
    /// Called when "Menu" state is entered.
    /// </summary>
    private void OnEnterMenuScreen(object sender, StateEventArgs eventArgs)
    {
      // Show a main menu consisting of several buttons.

      // The user should be able to select individual buttons by using the 
      // D-pad on the gamepad or the arrow keys. Therefore we need to create
      // a Window. A Window manages the currently selected ("focused") control 
      // and automatically handles focus movement.
      // In this example the Window is invisible (no chrome) and stretches across 
      // the entire screen.
      _menuWindow = new Window
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
      };
      _uiScreen.Children.Add(_menuWindow);

      // The content of the Window is a vertical StackPanel containing several buttons.
      var stackPanel = new StackPanel
      {
        Orientation = Orientation.Vertical,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new Vector4F(150, 0, 0, 200)
      };
      _menuWindow.Content = stackPanel;

      // The "Start" button starts the "Game" state.
      var startButton = new Button
      {
        Name = "StartButton",
        Content = new TextBlock { Text = "Start" },
        FocusWhenMouseOver = true,
      };
      startButton.Click += OnStartButtonClicked;

      // The buttons "Sub menu 1" and "Sub menu 2" show a dummy sub-menu.
      var subMenu1Button = new Button
      {
        Name = "SubMenu1Button",
        Content = new TextBlock { Text = "Sub-menu 1" },
        FocusWhenMouseOver = true,
      };
      subMenu1Button.Click += OnSubMenuButtonClicked;

      var subMenu2Button = new Button
      {
        Name = "SubMenu2Button",
        Content = new TextBlock { Text = "Sub-menu 2" },
        FocusWhenMouseOver = true,
      };
      subMenu2Button.Click += OnSubMenuButtonClicked;

      // The "Exit" button closes the application.
      var exitButton = new Button
      {
        Name = "ExitButton",
        Content = new TextBlock { Text = "Exit" },
        FocusWhenMouseOver = true,
      };
      exitButton.Click += OnExitButtonClicked;

      stackPanel.Children.Add(startButton);
      stackPanel.Children.Add(subMenu1Button);
      stackPanel.Children.Add(subMenu2Button);
      stackPanel.Children.Add(exitButton);

      // By default, the first button should be selected.
      startButton.Focus();

      // Slide the buttons in from the left (off screen) to make things more dynamic.
      AnimateFrom(stackPanel.Children, 0, new Vector2F(-300, 0));

      // The first time initialization of the GUI can take a short time. If we reset the elapsed 
      // time of the XNA game timer, the animation will start a lot smoother. 
      // (This works only if the XNA game uses a variable time step.)
      Game.ResetElapsedTime();
    }


    /// <summary>
    /// Called when "Start" button is clicked.
    /// </summary>
    private void OnStartButtonClicked(object sender, EventArgs eventArgs)
    {
      // Animate all buttons within the StackPanel to opacity 0 and offset (-300, 0).
      var stackPanel = (StackPanel)_menuWindow.Content;
      _menuExitAnimationController = AnimateTo(stackPanel.Children, 0, new Vector2F(-300, 0));

      // When the last animation finishes, trigger the "MenuToGame" transition.
      _menuExitAnimationController.Completed +=
        (s, e) => _stateMachine.States.ActiveState.Transitions["MenuToGame"].Fire();

      // Disable all buttons. The user should not be able to click a button while 
      // the fade-out animation is playing.
      DisableMenuItems();
    }


    /// <summary>
    /// Called when a "Sub Menu" button is clicked.
    /// </summary>
    private void OnSubMenuButtonClicked(object sender, EventArgs eventArgs)
    {
      // Animate all buttons within the StackPanel to opacity 0 and offset (-300, 0).
      var stackPanel = (StackPanel)_menuWindow.Content;
      _menuExitAnimationController = AnimateTo(stackPanel.Children, 0, new Vector2F(-300, 0));

      // When the last animation finishes, trigger the "MenuToSubMenu" transition.
      _menuExitAnimationController.Completed +=
        (s, e) => _stateMachine.States.ActiveState.Transitions["MenuToSubMenu"].Fire();

      // Disable all buttons. The user should not be able to click a button while 
      // the fade-out animation is playing.
      DisableMenuItems();
    }


    private void OnExitButtonClicked(object sender, EventArgs eventArgs)
    {
      // Animate all buttons within the StackPanel to opacity 0 and offset (-300, 0).
      var stackPanel = (StackPanel)_menuWindow.Content;
      _menuExitAnimationController = AnimateTo(stackPanel.Children, 0, new Vector2F(-300, 0));

      // When the last animation finishes, exit the game.
      _menuExitAnimationController.Completed += (s, e) =>
      {
        // Here, we would exit the game.
        //Game.Exit();
        // In this project we switch to the next sample instead.
        SampleFramework.LoadNextSample();
      };

      // Disable all buttons. The user should not be able to click a button while 
      // the fade-out animation is playing.
      DisableMenuItems();
    }


    private void DisableMenuItems()
    {
      var stackPanel = (StackPanel)_menuWindow.Content;
      foreach (var button in stackPanel.Children)
        button.IsEnabled = false;
    }


    /// <summary>
    /// Called when "Menu" state is exited.
    /// </summary>
    private void OnExitMenuScreen(object sender, StateEventArgs eventArgs)
    {
      // Clean up.
      _menuExitAnimationController.Stop();
      _menuExitAnimationController.Recycle();

      _uiScreen.Children.Remove(_menuWindow);
      _menuWindow = null;
    }
    #endregion


    #region ----- Sub Menu State -----

    private Window _subMenuWindow;
    private bool _subMenuExitAnimationIsPlaying;
    private AnimationController _subMenuExitAnimationController;


    /// <summary>
    /// Called when "SubMenu" state is entered.
    /// </summary>
    private void OnEnterSubMenuScreen(object sender, StateEventArgs eventArgs)
    {
      // Similar to OnEnterMenuScreen.
      _subMenuWindow = new Window
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
      };
      _uiScreen.Children.Add(_subMenuWindow);

      var stackPanel = new StackPanel
      {
        Orientation = Orientation.Vertical,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new Vector4F(150, 0, 0, 200)
      };
      _subMenuWindow.Content = stackPanel;

      var button1 = new Button
      {
        Name = "Item1Button",
        Content = new TextBlock { Text = "Item 1" },
        FocusWhenMouseOver = true,
      };
      var button2 = new Button
      {
        Name = "Item2Button",
        Content = new TextBlock { Text = "Item 2" },
        FocusWhenMouseOver = true,
      };
      var button3 = new Button
      {
        Name = "Item3Button",
        Content = new TextBlock { Text = "Item 3" },
        FocusWhenMouseOver = true,
      };
      var backButton = new Button
      {
        Name = "BackButton",
        Content = new TextBlock { Text = "Back" },
        FocusWhenMouseOver = true,
      };
      backButton.Click += OnBackButtonClicked;

      stackPanel.Children.Add(button1);
      stackPanel.Children.Add(button2);
      stackPanel.Children.Add(button3);
      stackPanel.Children.Add(backButton);

      button1.Focus();

      // Fade-in the buttons from the right.
      AnimateFrom(stackPanel.Children, 0, new Vector2F(300, 0));
    }


    /// <summary>
    /// Called every frame when "SubMenu" state is active.
    /// </summary>
    private void OnUpdateSubMenuScreen(object sender, StateEventArgs eventArgs)
    {
      if (_subMenuExitAnimationIsPlaying)
        return;

      // Exit sub menu if Back button, B button, or Escape key is pressed.
      if (InputService.IsPressed(Buttons.Back, false, LogicalPlayerIndex.One)
          || InputService.IsPressed(Buttons.B, false, LogicalPlayerIndex.One)
          || InputService.IsPressed(Keys.Escape, false))
      {
        InputService.IsKeyboardHandled = true;
        InputService.SetGamePadHandled(LogicalPlayerIndex.One, true);
        ExitSubMenuScreen();
      }
    }


    private void OnBackButtonClicked(object sender, EventArgs eventArgs)
    {
      ExitSubMenuScreen();
    }


    private void ExitSubMenuScreen()
    {
      // Animate all buttons within the StackPanel to opacity 0 and offset (300, 0).
      var stackPanel = (StackPanel)_subMenuWindow.Content;
      _subMenuExitAnimationController = AnimateTo(stackPanel.Children, 0, new Vector2F(300, 0));

      // When the last animation finishes, trigger the "MenuToSubMenu" transition.
      _subMenuExitAnimationController.Completed +=
        (s, e) => _stateMachine.States.ActiveState.Transitions["SubMenuToMenu"].Fire();

      _subMenuExitAnimationIsPlaying = true;

      // Disable all buttons. The user should not be able to click a button while 
      // the fade-out animation is playing.
      foreach (var button in stackPanel.Children)
        button.IsEnabled = false;
    }


    /// <summary>
    /// Called when "SubMenu" state is exited.
    /// </summary>
    private void OnExitSubMenuScreen(object sender, StateEventArgs eventArgs)
    {
      // Clean up.
      _subMenuExitAnimationController.Stop();
      _subMenuExitAnimationController.Recycle();
      _subMenuExitAnimationIsPlaying = false;

      _uiScreen.Children.Remove(_subMenuWindow);
      _subMenuWindow = null;
    }
    #endregion


    #region ----- Game State -----

    private TextBlock _gameTextBlock;

    /// <summary>
    /// Called when "Game" state is entered.
    /// </summary>
    private void OnEnterGameScreen(object sender, StateEventArgs eventArgs)
    {
      // Show a dummy text.
      _gameTextBlock = new TextBlock
      {
        Text = "Game is running. (Press Back button to return to menu.)",
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
      };
      _uiScreen.Children.Add(_gameTextBlock);
    }


    /// <summary>
    /// Called every frame when "Game" state is active.
    /// </summary>
    private void OnUpdateGameScreen(object sender, StateEventArgs eventArgs)
    {
      // Exit the "Game" state if Back button or Escape key is pressed.
      if (InputService.IsPressed(Buttons.Back, false, LogicalPlayerIndex.One)
          || InputService.IsPressed(Keys.Escape, false))
      {
        InputService.IsKeyboardHandled = true;
        InputService.SetGamePadHandled(LogicalPlayerIndex.One, true);
        _stateMachine.States.ActiveState.Transitions["GameToMenu"].Fire();
      }
    }


    /// <summary>
    /// Called when "Game" state is exited.
    /// </summary>
    private void OnExitGameScreen(object sender, StateEventArgs eventArgs)
    {
      // Clean up.
      _uiScreen.Children.Remove(_gameTextBlock);
      _gameTextBlock = null;
    }
    #endregion


    #region ----- Animation Helpers -----

    // The following code contains two helper methods to animate the opacity and offset
    // of a group of UI controls. The methods basically do the same, they animate the 
    // properties from/to a specific value. However the methods demonstrate two different 
    // approaches.
    //
    // The AnimateFrom method uses a more direct approach. It directly starts an 
    // animation for each UI control in list, thereby creating several independently 
    // running animations.
    //
    // The AnimateTo method uses a more declarative approach. All animations are 
    // defined and assigned to the target objects by setting the name of the UI control
    // in the TargetObject property. Then all animations are grouped together into
    // a single animation. When the resulting animation is started the animation system
    // creates the required animation instances and assigns the instances to the correct
    // objects and properties by matching the TargetObject and TargetProperty with the
    // name of the UI controls and their properties.
    //
    // Both methods achieve a similar result. The advantage of the first method is more
    // direct control. The advantage of the seconds method is that only a single animation
    // controller is required to control all animations at once.

    /// <summary>
    /// Animates the opacity and offset of a group of controls from the specified value to their 
    /// current value.
    /// </summary>
    /// <param name="controls">The UI controls to be animated.</param>
    /// <param name="opacity">The initial opacity.</param>
    /// <param name="offset">The initial offset.</param>
    private void AnimateFrom(IList<UIControl> controls, float opacity, Vector2F offset)
    {
      TimeSpan duration = TimeSpan.FromSeconds(0.8);

      // First, let's define the animation that is going to be applied to a control.
      // Animate the "Opacity" from the specified value to its current value.
      var opacityAnimation = new SingleFromToByAnimation
      {
        TargetProperty = "Opacity",
        From = opacity,
        Duration = duration,
        EasingFunction = new CubicEase { Mode = EasingMode.EaseOut },
      };

      // Animate the "RenderTranslation" property from the specified offset to its
      // its current value, which is usually (0, 0).
      var offsetAnimation = new Vector2FFromToByAnimation
      {
        TargetProperty = "RenderTranslation",
        From = offset,
        Duration = duration,
        EasingFunction = new CubicEase { Mode = EasingMode.EaseOut },
      };

      // Group the opacity and offset animation together using a TimelineGroup.
      var timelineGroup = new TimelineGroup();
      timelineGroup.Add(opacityAnimation);
      timelineGroup.Add(offsetAnimation);

      // Run the animation on each control using a negative delay to give the first controls
      // a slight head start.
      var numberOfControls = controls.Count;
      for (int i = 0; i < controls.Count; i++)
      {
        var clip = new TimelineClip(timelineGroup)
        {
          Delay = TimeSpan.FromSeconds(-0.04 * (numberOfControls - i)),
          FillBehavior = FillBehavior.Stop,   // Stop and remove the animation when it is done.
        };
        var animationController = AnimationService.StartAnimation(clip, controls[i]);

        animationController.UpdateAndApply();

        // Enable "auto-recycling" to ensure that the animation resources are recycled once
        // the animation stops or the target objects are garbage collected.
        animationController.AutoRecycle();
      }
    }


    /// <summary>
    /// Animates the opacity and offset of a group of controls from their current value to the 
    /// specified value.
    /// </summary>
    /// <param name="controls">The UI controls to be animated.</param>
    /// <param name="opacity">The opacity.</param>
    /// <param name="offset">The offset.</param>
    private AnimationController AnimateTo(IList<UIControl> controls, float opacity, Vector2F offset)
    {
      TimeSpan duration = TimeSpan.FromSeconds(0.6f);

      // First, let's define the animation that is going to be applied to a control.
      // Animate the "Opacity" from its current value to the specified value.
      var opacityAnimation = new SingleFromToByAnimation
      {
        TargetProperty = "Opacity",
        To = opacity,
        Duration = duration,
        EasingFunction = new CubicEase { Mode = EasingMode.EaseIn },
      };

      // Animate the "RenderTranslation" property from its current value, which is 
      // usually (0, 0), to the specified value.
      var offsetAnimation = new Vector2FFromToByAnimation
      {
        TargetProperty = "RenderTranslation",
        To = offset,
        Duration = duration,
        EasingFunction = new CubicEase { Mode = EasingMode.EaseIn },
      };

      // Group the opacity and offset animation together using a TimelineGroup.
      var timelineGroup = new TimelineGroup();
      timelineGroup.Add(opacityAnimation);
      timelineGroup.Add(offsetAnimation);

      // Now we duplicate this animation by creating new TimelineClips that wrap the TimelineGroup.
      // A TimelineClip is assigned to a target by setting the TargetObject property.
      var storyboard = new TimelineGroup();

      for (int i = 0; i < controls.Count; i++)
      {
        var clip = new TimelineClip(timelineGroup)
        {
          TargetObject = controls[i].Name,  // Assign the clip to the i-th control.
          Delay = TimeSpan.FromSeconds(0.04f * i),
          FillBehavior = FillBehavior.Hold, // Hold the last value of the animation when it            
        };                                  // because we don't want to opacity and offset to
        // jump back to their original value.
        storyboard.Add(clip);
      }

      // Now we apply the "storyboard" to the group of UI controls. The animation system
      // will automatically assign individual animations to the right objects and 
      // properties.
#if !XBOX && !WP7
      var animationController = AnimationService.StartAnimation(storyboard, controls);
#else
      var animationController = AnimationService.StartAnimation(storyboard, controls.Cast<IAnimatableObject>());
#endif

      animationController.UpdateAndApply();

      // The returned animation controller can be used to start, stop, pause, ... all
      // animations at once. (Note that we don't set AutoRecycle here, because we will 
      // explicitly stop and recycle the animations in the code above.)
      return animationController;
    }
    #endregion
  }
}
