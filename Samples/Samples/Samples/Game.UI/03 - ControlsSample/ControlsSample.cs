using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace Samples.Game.UI
{
  [Sample(SampleCategory.GameUI,
    @"This sample shows how to use the DigitalRune Game UI controls.",
    @"This sample shows:
- All controls provided by the UI library.
- How to create a resizable window with scrollable content.
- How to use a render transform to scale and rotate a control.
- How to a modal dialog can be created in code or loaded from an XML file.
- How to open a console for debugging, print a message in the console and
  create a new console command.
- How to add a custom text block control that displays the frame rate.",
    3)]
  public class ControlsSample : Sample
  {
    // Important note: 
    // If we want the UIControls to handle gamepad input, we must set a logical player. 
    // This is done in GamePadComponent.cs

    private UIScreen _uiScreen;

    private int _themeNumber;
    private bool _changeTheme;


    public ControlsSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add a DelegateGraphicsScreen as the first graphics screen to the graphics
      // service. This lets us do the rendering in the Render method of this class.
      var graphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, graphicsScreen);

      CreateGui();
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


    private void CreateGui()
    {
      // Remove old screen.
      if (_uiScreen != null)
        UIService.Screens.Remove(_uiScreen);

      // Load a UI theme, which defines the appearance and default values of UI controls.
      string themeName;
      if (_themeNumber == 0)
        themeName = "UI Themes/BlendBlue/Theme";
      else
        themeName = "UI Themes/Aero/Theme";

      Theme theme = ContentManager.Load<Theme>(themeName);

      // Create a UI renderer, which uses the theme info to renderer UI controls.
      UIRenderer renderer = new UIRenderer(Game, theme);

      // Create a UIScreen and add it to the UI service. The screen is the root of the 
      // tree of UI controls. Each screen can have its own renderer.
      _uiScreen = new UIScreen("SampleUIScreen", renderer);
      UIService.Screens.Add(_uiScreen);

      // Add a text block that displays the frame rate.
      _uiScreen.Children.Add(new FpsTextBlock
      {
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Top,
        Margin = new Vector4F(40),
      });

      // Add a text label.
      var textBlock = new TextBlock
      {
        Text = "Press button to open window: ",
        Margin = new Vector4F(4)
      };

      // Add buttons that open samples.
      var button0 = new Button
      {
        Content = new TextBlock { Text = "Sample #1: Controls" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch,
      };
      button0.Click += (s, e) =>
      {
        var allControlsWindow = new AllControlsWindow(ContentManager, renderer);
        allControlsWindow.Show(_uiScreen);
      };

      var button1 = new Button
      {
        Content = new TextBlock { Text = "Sample #2: ScrollViewer" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      button1.Click += (s, e) =>
      {
        var resizableWindow = new ResizableWindow(ContentManager);
        resizableWindow.Show(_uiScreen);
      };

      var button2 = new Button
      {
        Content = new TextBlock { Text = "Sample #3: Transformations" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      button2.Click += (s, e) =>
      {
        var renderTransformWindow = new RenderTransformWindow();
        renderTransformWindow.Show(_uiScreen);
      };

      var button3 = new Button
      {
        Content = new TextBlock { Text = "Sample #4: Dialogs" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      button3.Click += (s, e) =>
      {
        var dialogDemoWindow = new DialogDemoWindow();
        dialogDemoWindow.Show(_uiScreen);
      };

      var button4 = new Button
      {
        Content = new TextBlock { Text = "Sample #5: Console" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      button4.Click += (s, e) =>
      {
        var consoleWindow = new ConsoleWindow();
        consoleWindow.Show(_uiScreen);
      };

      var button5 = new Button
      {
        Content = new TextBlock { Text = "Switch UI Theme" },
        Margin = new Vector4F(4),
        Padding = new Vector4F(6),
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      button5.Click += (s, e) =>
      {
        // Set a flag. In the next Update() we will load a new theme.
        // Note: We should not load the theme here because the UI is currently updated.
        _changeTheme = true;
      };

      var stackPanel = new StackPanel { Margin = new Vector4F(40) };
      stackPanel.Children.Add(textBlock);
      stackPanel.Children.Add(button0);
      stackPanel.Children.Add(button1);
      stackPanel.Children.Add(button2);
      stackPanel.Children.Add(button3);
      stackPanel.Children.Add(button4);
      stackPanel.Children.Add(button5);
      _uiScreen.Children.Add(stackPanel);

      // Optional: If we want to allow the user to use buttons in the screen with 
      // keyboard or game pad, we have to make it a focus scope. Normally, only 
      // windows are focus scopes.
      _uiScreen.IsFocusScope = true;
      _uiScreen.Focus();
    }


    public override void Update(GameTime gameTime)
    {
      if (_changeTheme)
      {
        // Load a new theme.
        _themeNumber = (_themeNumber + 1) % 4;
        CreateGui();
        _changeTheme = false;
      }
    }


    private void Render(RenderContext context)
    {
      // Draw the UIScreen. UIScreen.Draw() must be called manually. (That means, 
      // we can decide where and when it is called. For example, we could also
      // render the screen into an offscreen render target.)
      _uiScreen.Draw(context.DeltaTime);
    }
  }
}
