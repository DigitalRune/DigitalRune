using System.Collections.Generic;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Game.UI
{
  [Sample(SampleCategory.GameUI,
    @"This sample shows how to position a UIControl relative to a 3D object.",
    @"The ProgressBar control is displayed over 3D objects like a health bar.
The screen space position of the progress bar controls is computed from the 3D object positions.
A new game object property 'Z' is created for UIControls. This is used to attach depth value to all
controls. The controls are sorted by their z value before rendering.",
    12)]
  public class HealthBarSample : BasicSample
  {
    private readonly UIScreen _uiScreen;
    private readonly CameraObject _cameraObject;

    // A list of objects and their health bar.
    private readonly List<Pair<DynamicObject, ProgressBar>> _objects = new List<Pair<DynamicObject, ProgressBar>>();

    // ID of a new game object property for UIControls which stores the z (depth) value.
    private readonly int _zPropertyId;


    public HealthBarSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // The base class has created the graphics screen for the 3D objects.
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3F(0, 1, 3), 0, 0);
      _cameraObject = GameObjectService.Objects.OfType<CameraObject>().First();

      // We add another graphics screen on top which renders the GUI.
      var graphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(1, graphicsScreen);

      // Create a UIScreen.
      Theme theme = ContentManager.Load<Theme>("UI Themes/BlendBlue/Theme");
      UIRenderer renderer = new UIRenderer(Game, theme);
      _uiScreen = new UIScreen("HealthBarScreen", renderer)
      {
        Background = Color.Transparent,   // Background must not hide the 3D graphics screen.
      };
      UIService.Screens.Add(_uiScreen);

      // Standard force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Standard game objects.
      GameObjectService.Objects.Add(new GroundObject(Services));

      // Create a new game object property. This allows to attach a z value to all UIControls.
      _zPropertyId = UIControl.CreateProperty<float>(
        typeof(UIControl), 
        "Z", 
        GamePropertyCategories.Appearance, 
        "The layer depth. Objects with lower depth value are in front.", 
        0.0f, 
        UIPropertyOptions.AffectsRender);

      // Create 3D objects and a progress bar for each object.
      for (int i = 0; i < 10; i++)
      {
        var dynamicObject = new DynamicObject(Services, 1);
        GameObjectService.Objects.Add(dynamicObject);

        var progressBar = new ProgressBar
        {
          Value = RandomHelper.Random.NextFloat(0, 100),
          Maximum = 100,
          X = 100,
          Y = 100,
          Width = 100,
          Height = 20,
          Margin = new Vector4F(-50, -10, 0, 0),  // Use a margin to center the control.
        };
        _uiScreen.Children.Add(progressBar);

        _objects.Add(new Pair<DynamicObject, ProgressBar>(dynamicObject, progressBar));
      }
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Remove UIScreen from UI service.
        UIService.Screens.Remove(_uiScreen);

        // Rest, like graphics screens, is cleaned up by the base class.
      }

      base.Dispose(disposing);
    }


    // Render callback of the the graphics screen which renders the GUI.
    private void Render(RenderContext context)
    {
      var cameraNode = _cameraObject.CameraNode;
      Matrix44F viewProjection = cameraNode.Camera.Projection * cameraNode.View;
      var viewport = GraphicsService.GraphicsDevice.Viewport;

      // Update x/y screen position of progress bars.
      foreach (var pair in _objects)
      {
        var dynamicObject = pair.First;
        var rigidBody = dynamicObject.RigidBody;

        // Get a value which is proportional to the object radius.
        float radius = rigidBody.Shape.GetAabb().Extent.Length * 0.35f;

        // Position the progress bar above the object.
        Vector3F positionWorld = rigidBody.Pose.Position + new Vector3F(0, radius, 0);

        // Convert world space position to screen space.
        Vector3F positionScreen = viewport.Project(positionWorld, viewProjection);

        var progressBar = pair.Second;
        progressBar.X = positionScreen.X;
        progressBar.Y = positionScreen.Y;
        progressBar.SetValue(_zPropertyId, positionScreen.Z);

        // Hide progress bars that are too close or too far away.
        progressBar.IsVisible = positionScreen.Z >= 0 && positionScreen.Z <= 1;
      }

      // The default UI renderer renders UIScreen children from first to last. To get a correct
      // z order, we need to sort the UIScreen children by their z value.
      // Z values don't change often. Therefore, we can use a primitive sort algorithm.
      bool continueSorting = true;
      while (continueSorting)
      {
        continueSorting = false;
        for (int i = 0; i < _uiScreen.Children.Count - 1; i++)
        {
          if (_uiScreen.Children[i].GetValue<float>(_zPropertyId) < _uiScreen.Children[i + 1].GetValue<float>(_zPropertyId))
          {
            _uiScreen.Children.Move(i, i + 1);
            continueSorting = true;
          }
        }
      }

      _uiScreen.Draw(context.DeltaTime);
    }
  }
}
