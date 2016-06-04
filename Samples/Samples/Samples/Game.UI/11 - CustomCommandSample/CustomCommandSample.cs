using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Samples.Game.UI
{
  [Sample(SampleCategory.GameUI,
    @"This sample shows how to implement custom input commands.",
    @"The ButtonHoldCommand tracks how long an input button has been pressed.
The ButtonTapCommand requires the player to rapidly tap a certain button.
The ButtonSequenceCommand detects whether a certain sequence of buttons is pressed.",
    11)]
  public class CustomCommandSample : Sample
  {
    private readonly ButtonHoldCommand _buttonHoldCommand;
    private readonly ButtonTapCommand _buttonTapCommand;
    private readonly ButtonSequenceCommand _buttonSequenceCommand;

    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _textFont;
    private readonly SpriteFont _buttonFont;

    private float _buttonHoldProgress;
    private float _buttonTapProgress;
    private float _buttonSequenceProgress;


    public CustomCommandSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add a DelegateGraphicsScreen as the first graphics screen to the graphics
      // service. This lets us do the rendering in the Render method of this class.
      var graphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render
      };
      GraphicsService.Screens.Insert(0, graphicsScreen);

      _spriteBatch = GraphicsService.GetSpriteBatch();

      // Load a few SpriteFonts for rendering.
      _textFont = UIContentManager.Load<SpriteFont>("UI Themes/WindowsPhone7/Segoe15");
      _buttonFont = ContentManager.Load<SpriteFont>("ButtonImages/xboxControllerSpriteFont");

      // Add custom commands to input service.
      _buttonHoldCommand = new ButtonHoldCommand(Buttons.A, 1.0f) { Name = "Hold A" };
      _buttonTapCommand = new ButtonTapCommand(Buttons.A, 0.2f, 1.0f) { Name = "Tap A" };
      _buttonSequenceCommand = new ButtonSequenceCommand(new [] { Buttons.A, Buttons.B, Buttons.A, Buttons.B }, 2.0f) { Name = "A-B-A-B" };
      InputService.Commands.Add(_buttonHoldCommand);
      InputService.Commands.Add(_buttonTapCommand);
      InputService.Commands.Add(_buttonSequenceCommand);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        InputService.Commands.Remove(_buttonHoldCommand);
        InputService.Commands.Remove(_buttonTapCommand);
        InputService.Commands.Remove(_buttonSequenceCommand);
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // Read the current values of the input commands.
      // value == 0 means that the required buttons haven't been pressed.
      // value > 0 means that the command is in progress.
      // value == 1 means that the command is complete.
      _buttonHoldProgress = _buttonHoldCommand.Value;
      _buttonTapProgress = _buttonTapCommand.Value;
      _buttonSequenceProgress = _buttonSequenceCommand.Value;
    }


    private void Render(RenderContext renderContext)
    {
      // Visualize the state of the input commands by drawing a simple progress bar.

      _spriteBatch.Begin();

      // "Hold (A)"
      DrawRightAlignedText(new Vector2(400, 100), _textFont, "Hold ", 1.0f, _buttonFont, "'", 0.5f);
      DrawProgressBar(new Rectangle(405, 100 - 16, 120, 32), _buttonHoldProgress);

      // "Rapidly tap (A)"
      DrawRightAlignedText(new Vector2(400, 140), _textFont, "Rapidly tap ", 1.0f, _buttonFont, "'", 0.5f);
      DrawProgressBar(new Rectangle(405, 140 - 16, 120, 32), _buttonTapProgress);

      // "Press sequence (A)(B)(A)(B)
      DrawRightAlignedText(new Vector2(400, 180), _textFont, "Press sequence ", 1.0f, _buttonFont, "')')", 0.5f);
      DrawProgressBar(new Rectangle(405, 180 - 16, 120, 32), _buttonSequenceProgress);

      _spriteBatch.End();
    }


    private void DrawRightAlignedText(Vector2 position, SpriteFont font0, string text0, float scale0, SpriteFont font1, string text1, float scale1)
    {
      Vector2 size0 = font0.MeasureString(text0) * scale0;
      Vector2 size1 = font1.MeasureString(text1) * scale1;

      Vector2 position0 = new Vector2(position.X - size0.X - size1.X, position.Y - size0.Y / 2);
      Vector2 position1 = new Vector2(position.X - size1.X, position.Y - size1.Y / 2);

      _spriteBatch.DrawString(font0, text0, position0, Color.White, 0, Vector2.Zero, new Vector2(scale0), SpriteEffects.None, 0);
      _spriteBatch.DrawString(font1, text1, position1, Color.White, 0, Vector2.Zero, new Vector2(scale1), SpriteEffects.None, 0);
    }


    private void DrawProgressBar(Rectangle bounds, float progress)
    {
      var whiteTexture = GraphicsService.GetDefaultTexture2DWhite();

      // Draw border.
      _spriteBatch.Draw(whiteTexture, bounds, null, Color.White);

      // Draw inner rectangle.
      bounds.X += 2;
      bounds.Y += 2;
      bounds.Width -= 4;
      bounds.Height -= 4;
      _spriteBatch.Draw(whiteTexture, bounds, null, Color.Black);

      // Draw progress indicator.
      if (progress > 0)
      {
        bounds.Width = (int)(bounds.Width * MathHelper.Clamp(progress, 0, 1));
        var color = (progress < 1) ? Color.Lime : Color.Yellow;
        _spriteBatch.Draw(whiteTexture, bounds, null, color);
      }
    }
  }
}
