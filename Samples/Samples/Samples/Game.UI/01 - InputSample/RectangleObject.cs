using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Game.UI
{
  // Draws a colored rectangle. 
  // The rectangle can be moved with the left mouse button or the left thumbstick.
  // Another rectangle can be selected by clicking it with the left mouse button or
  // by pressing a gamepad shoulder button.
  // Pressing <Space> on keyboard or <A> on gamepad changes the color.
  public class RectangleObject : GameObject
  {
    private const int Size = 300;

    private readonly IInputService _inputService;
    private readonly IGameObjectService _gameObjectService;
    private readonly DebugRenderer _debugRenderer;

    private float _left;
    private float _top;
    private Color _color;

    // Data for dragging the object with the mouse.
    private bool _isDragging;
    private Vector2F _lastMousePosition;


    public RectangleObject(IServiceLocator services)
    {
      _inputService = services.GetInstance<IInputService>();
      _gameObjectService = services.GetInstance<IGameObjectService>();
      _debugRenderer = services.GetInstance<DebugRenderer>("DebugRenderer2D");

      _left = RandomHelper.Random.NextInteger(0, 1000);
      _top = RandomHelper.Random.NextInteger(0, 600);
      SetRandomColor();
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // End dragging if button is released.
      if (_inputService.IsUp(MouseButtons.Left))
        _isDragging = false;

      // Do not handle mouse input if the input was already handled by someone else.
      if (!_inputService.IsMouseOrTouchHandled)
      {
        bool mouseIsInside = IsInside(_inputService.MousePosition);
        if (_inputService.IsPressed(MouseButtons.Left, false) && mouseIsInside)
        {
          // Start dragging.
          _isDragging = true;

          // The rectangle was clicked. It must move into the foreground and should
          // be updated first. 
          // To update this object first, we must move it to the start of the
          // game object service collection.
          _gameObjectService.Objects.Remove(this);
          _gameObjectService.Objects.Insert(0, this);

          // Set flag to let other game logic know that the mouse input was already handled.
          _inputService.IsMouseOrTouchHandled = true;
        }
        else if (_isDragging)
        {
          // Drag rectangle.
          _left += _inputService.MousePosition.X - _lastMousePosition.X;
          _top += _inputService.MousePosition.Y - _lastMousePosition.Y;

          _inputService.IsMouseOrTouchHandled = true;
        }

        _lastMousePosition = _inputService.MousePosition;
      }

      // Do not handle keyboard input if the input was already handled by someone else.
      if (!_inputService.IsKeyboardHandled)
      {
        // Space click sets new random color.
        if (_inputService.IsPressed(Keys.Space, true))
        {
          SetRandomColor();

          // Set flag to let other game logic know that the keyboard input was already handled.
          _inputService.IsKeyboardHandled = true;
        }
      }

      // Do not handle gamepad input if the input was already handled by someone else.
      if (!_inputService.IsGamePadHandled(LogicalPlayerIndex.One))
      {
        bool handled = false;

        // Change color with <A>.
        if (_inputService.IsPressed(Buttons.A, true, LogicalPlayerIndex.One))
        {
          SetRandomColor();
          handled = true;
        }

        // Change update order with should buttons.
        if (_inputService.IsPressed(Buttons.LeftShoulder, true, LogicalPlayerIndex.One))
        {
          // Move this object to the end of the game objects collection.
          _gameObjectService.Objects.Remove(this);
          _gameObjectService.Objects.Add(this);
          handled = true;
        }
        else if (_inputService.IsPressed(Buttons.RightShoulder, true, LogicalPlayerIndex.One))
        {
          // Move the last RectangleObject to the start of the game objects collection.
          var lastRectangleObject = _gameObjectService.Objects.OfType<RectangleObject>().Last();
          _gameObjectService.Objects.Remove(lastRectangleObject);
          _gameObjectService.Objects.Insert(0, lastRectangleObject);
          handled = true;
        }

        // Move rectangle with thumbstick.
        Vector2 leftThumbStick = _inputService.GetGamePadState(LogicalPlayerIndex.One).ThumbSticks.Left;
        _left += leftThumbStick.X * (float)deltaTime.TotalSeconds * 500;
        _top -= leftThumbStick.Y * (float)deltaTime.TotalSeconds * 500;
        if (leftThumbStick != Vector2.Zero)
          handled = true;

        // Set flag to let other game logic know that the gamepad input was already handled.
        if (handled)
          _inputService.SetGamePadHandled(LogicalPlayerIndex.One, true);
      }

      // Draw the rectangle. We use the DebugRenderer.DrawBox() method of the SampleGraphicsScreen. 
      // The depth (z value) of the box depends on the update order. The object which is
      // update first should be rendered on top of other objects.
      float depth = -_gameObjectService.Objects.OfType<RectangleObject>().ToList().IndexOf(this);
      _debugRenderer.DrawBox(
        Size,
        Size,
        1,
        new Pose(new Vector3F(_left + Size / 2, _top + Size / 2, depth)),
        _color,
        false,
        false);
    }


    // Returns true if coordinates are within rectangle.
    private bool IsInside(Vector2F position)
    {
      return _left < position.X && position.X < _left + Size
             && _top < position.Y && position.Y < _top + Size;
    }


    private void SetRandomColor()
    {
      _color = new Color((Vector3)RandomHelper.Random.NextVector3F(0, 1));
    }
  }
}
