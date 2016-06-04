using System;
using System.Text;
using DigitalRune.Graphics;
using DigitalRune.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace InteropSample
{
  // A GraphicsScreen that draws some info and status texts.
  public class TextGraphicsScreen : GraphicsScreen
  {
    private readonly ContentManager _content;
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _spriteFont;
    private readonly StringBuilder _text;


    public TextGraphicsScreen(IGraphicsService graphicsService, ContentManager content)
      : base(graphicsService)
    {
      _content = content;
      _text = new StringBuilder();

      _spriteBatch = new SpriteBatch(GraphicsService.GraphicsDevice);
      _spriteFont = _content.Load<SpriteFont>("SpriteFont1");
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      _text.Clear();
      _text.Append("Press <1>, <2>, <3> or <4> to switch between\n"
                   + "Windowed, WinForm, WPF and FullScreen mode.");

      // Create a string with the pressed keys.
      Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
      _text.Append("\nPressed Keys: ");
      foreach (Keys key in pressedKeys)
        _text.Append(key + " ");

      // Create a string with a few mouse inputs.
      _text.Append("\nMouse Position: ");
      _text.AppendNumber(Mouse.GetState().X);
      _text.Append("/");
      _text.AppendNumber(Mouse.GetState().Y);
      _text.Append("\nLeft Button: ");
      _text.Append(Mouse.GetState().LeftButton == ButtonState.Pressed);
      _text.Append("\nMouse Wheel: ");
      _text.Append(Mouse.GetState().ScrollWheelValue);
    }


    protected override void OnRender(RenderContext context)
    {
      // Draw text
      _spriteBatch.Begin();
      _spriteBatch.DrawString(_spriteFont, _text, new Vector2(10, 10), Color.LightGreen, 0, new Vector2(), 1.0f, SpriteEffects.None, 0.5f);
      _spriteBatch.End();
    }
  }
}
