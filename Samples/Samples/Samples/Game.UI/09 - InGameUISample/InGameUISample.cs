#if !WP7 && !WP8
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Samples.Game.UI
{
  [Sample(SampleCategory.GameUI,
    @"This sample shows how to display a GraphicsScreen inside a UIControl and how to place an
interactive UI on 3D objects inside the 3D scene.",
    @"This sample creates two GraphicsScreens:
- One DeferredGraphicsScreen that renders a 3D scene.
- One DelegateGraphicsScreen that renders all UIScreens. InGameUISample.Render()
  is used as the render callback of this screen.

This sample also creates two UIScreens:
- The NormalUIScreen is drawn into the back buffer. It shows one window. The content
  of the window is an Image control that displays the output of the DeferredGraphicsScreen.
- The InGameUIScreen is drawn into an off-screen render target. It contains a single
  window with some sliders and a button. The off-screen render target is used as a
  texture of a TV 3D model and a ProjectorLight.

Rendering works like this:
When GraphicsManager.Render() is executed in the game loop, the DeferredGraphicsScreen
renders the 3D scene into an off-screen render target. The TV objects in the 3D scene
are textured with the render target that contains the InGameUIScreen of the last frame.
Then the DelegateGraphicsScreen is rendered and calls the Render() callback of this sample.
This Render() method gets the DeferredGraphicsScreen's render target from the render
context and sets it as the texture of the Image control. The InGameUIScreen is rendered
into another off-screen render target (used on the TV objects). Finally, the Render()
method renders the NormalUIScreen into the back buffer.

UI handling works like this:
At first, the mouse is shown and mouse centering is disabled. Only the NormalUIScreen
handles the input. When the Image control is clicked, the mouse is hidden, mouse
centering is enabled and the InGameUIScreen handles the input.
When mouse centering is enabled, the CameraObject reads the device input to move the
camera. The InGameUIScreen compute intersections between the reticle and the TV objects.
If the front side of a TV is hit, the 'mouse position' relative to the  InGameUIScreen
is computed and used by the controls of this UIScreen instead of the real mouse position.
When the user presses <Esc>, the mouse is released.",
    9)]
  public class InGameUISample : Sample
  {
    // Two GraphicsScreens.

    // Two UIScreens.
    private readonly NormalUIScreen _normalUIScreen;
    private readonly InGameUIScreen _inGameScreen;

    // The render target for the InGameUIScreen.
    private readonly RenderTarget2D _inGameUIScreenRenderTarget;


    public InGameUISample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add gravity and damping to the physics simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Create the DeferredGraphicsScreen.
      var graphicsScreen = new DeferredGraphicsScreen(Services);
      graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, graphicsScreen);

      Services.Register(typeof(DebugRenderer), null, graphicsScreen.DebugRenderer);
      Services.Register(typeof(IScene), null, graphicsScreen.Scene);

      // some 3D objects
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;
      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new StaticSkyObject(Services));
      GameObjectService.Objects.Add(new GroundObject(Services));
      for (int i = 0; i < 10; i++)
        GameObjectService.Objects.Add(new DynamicObject(Services, 3));

      // Create the UIScreen which is rendered into the back buffer.
      Theme theme = ContentManager.Load<Theme>("UI Themes/Aero/Theme");
      UIRenderer renderer = new UIRenderer(Game, theme);
      _normalUIScreen = new NormalUIScreen(renderer);
      UIService.Screens.Add(_normalUIScreen);

      // Handle the InputProcessed event of the Image control.
      _normalUIScreen.Image.InputProcessed += OnGameViewControlInputProcessed;

      // Create the DelegateGraphicsScreen. This graphics screen is on top of the
      // DeferredGraphicsScreen and instructs the graphics service to render the
      // previous screen into an off-screen render target.
      var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
        RenderPreviousScreensToTexture = true,
        SourceTextureFormat = new RenderTargetFormat(
          (int)_normalUIScreen.Image.Width,
          (int)_normalUIScreen.Image.Height,
          false,
          SurfaceFormat.Color,
          DepthFormat.Depth24),
      };
      GraphicsService.Screens.Insert(1, delegateGraphicsScreen);

      // Create the UIScreen that is rendered into a render target and mapped onto the 3D game objects.
      _inGameUIScreenRenderTarget = new RenderTarget2D(GraphicsService.GraphicsDevice, 600, 250, false, SurfaceFormat.Color, DepthFormat.None);
      _inGameScreen = new InGameUIScreen(Services, renderer)
      {
        InputEnabled = false,
        Width = _inGameUIScreenRenderTarget.Width,
        Height = _inGameUIScreenRenderTarget.Height,
      };
      UIService.Screens.Add(_inGameScreen);

      // We can use the off-screen render target anywhere in the 3D scene. Here, we 
      // use it to replace the normal "TestCard" texture of the TV objects and the ProjectorLights.
      foreach (var node in graphicsScreen.Scene.GetDescendants())
      {
        var meshNode = node as MeshNode;
        if (meshNode != null)
        {
          foreach (var material in meshNode.Mesh.Materials)
          {
            if (material.Name == "TestCard")
            {
              material["Material"].Set("EmissiveTexture", (Texture)_inGameUIScreenRenderTarget);
              material["Material"].Set("Exposure", 0.1f);
            }
          }
          continue;
        }

        var lightNode = node as LightNode;
        if (lightNode != null && lightNode.Light is ProjectorLight)
        {
          ((ProjectorLight)lightNode.Light).Texture = _inGameUIScreenRenderTarget;
        }
      }
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        UIService.Screens.Remove(_normalUIScreen);
        UIService.Screens.Remove(_inGameScreen);

        // Unload content.
        // We have modified the material of the TV mesh. These changes should not
        // affect other samples. Therefore, we unload the assets. The next sample
        // will reload them with default values.)
        ContentManager.Unload();
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // If the 3D scene has the input, check if <Esc> is pressed.
      if (_inGameScreen.InputEnabled && InputService.IsPressed(Keys.Escape, false))
      {
        InputService.IsKeyboardHandled = true;

        // From now on the NormalUIScreen should handle the input and the InGameUIScreen 
        // should ignore the input.
        _normalUIScreen.InputEnabled = true;
        _inGameScreen.InputEnabled = false;

        // Show the mouse cursor.
        SampleFramework.IsMouseVisible = true;

        // The mouse was made visible - set the mouse cursor position to center 
        // of the Image control - so that the user does not have to search for it.
        Mouse.SetPosition(
          (int)(_normalUIScreen.Image.ActualX + _normalUIScreen.Image.ActualWidth / 2),
          (int)(_normalUIScreen.Image.ActualY + _normalUIScreen.Image.ActualHeight / 2));
      }
    }


    private void Render(RenderContext context)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      // context.SourceTexture contains the result of the DeferredGraphicsScreen.
      // Use this texture in the Image control.
      _normalUIScreen.Image.Width = context.SourceTexture.Width;
      _normalUIScreen.Image.Height = context.SourceTexture.Height;
      _normalUIScreen.Image.Texture = context.SourceTexture;

      // Draw the InGameUIScreen into its off-screen render target.
      graphicsDevice.SetRenderTarget(_inGameUIScreenRenderTarget);
      graphicsDevice.Clear(Color.CornflowerBlue);
      _inGameScreen.Draw(context.DeltaTime);
      graphicsDevice.SetRenderTarget(context.RenderTarget);
      graphicsDevice.Viewport = context.Viewport;

      // Draw the NormalUIScreen.
      _normalUIScreen.Draw(context.DeltaTime);
    }


    // Called when the Image control has processed its input.
    private void OnGameViewControlInputProcessed(object sender, InputEventArgs eventArgs)
    {
      // Check if mouse is clicked over the Image control.
      if (!InputService.IsMouseOrTouchHandled
          && _normalUIScreen.Image.IsMouseDirectlyOver
          && InputService.IsPressed(MouseButtons.Left, false))
      {
        InputService.IsMouseOrTouchHandled = true;

        // Image control was clicked. From now on the NormalUIScreen should ignore input and the
        // InGameUIScreen should handle the input.
        _normalUIScreen.InputEnabled = false;
        _inGameScreen.InputEnabled = true;

        // Enable mouse centering and hide the mouse cursor.
        SampleFramework.IsMouseVisible = false;
      }
    }
  }
}
#endif