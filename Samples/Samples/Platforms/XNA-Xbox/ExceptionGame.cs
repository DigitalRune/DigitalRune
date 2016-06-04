using System;
using System.Reflection;
using System.Text;
using DigitalRune.Storages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Samples
{
  /// <summary>
  /// Represents an XNA game that is only used to display an error message.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Use this game class to display an error message if an exception has occurred in the main XNA
  /// game class. This class shows a Guide message box. If exception details (property
  /// <see cref="Exception"/>) are given, then the message box allows the user to view exception
  /// details or to exit the game. If <see cref="Exception"/> is empty, then the game ends when the
  /// message box is closed.
  /// </para>
  /// <para>
  /// This class is intended to display exceptions on the Xbox 360. On Windows it is recommended to
  /// show the exception text in a window, not in an XNA game.
  /// </para>
  /// </remarks>
  /// <example>
  /// The <see cref="ExceptionGame"/> is typically used like this:
  /// <code lang="csharp">
  /// <![CDATA[
  /// if (Debugger.IsAttached)
  /// {
  ///   // The debugger is attached. The debugger will display any exception messages.
  /// 
  ///   // Run the XNA game.
  ///   using (Game1 game = new Game1())
  ///    game.Run();
  /// }
  /// else
  /// {
  ///   // The debugger is NOT attached. The ExceptionGame will display any exception messages.
  ///   try
  ///   {
  ///     // Run the XNA game.
  ///     using (Game1 game = new Game1())
  ///       game.Run();
  ///   }
  ///   catch (Exception exception)
  ///   {
  ///     using (ExceptionGame exceptionGame = new ExceptionGame())
  ///     {
  ///       exceptionGame.ApplicationName = "ExceptionGameTest";
  ///       exceptionGame.Exception = exception;
  ///       exceptionGame.Run();
  ///     };
  ///   }
  /// }
  /// ]]>
  /// </code>
  /// </example>
  public class ExceptionGame : Microsoft.Xna.Framework.Game
  {
    // Notes:
    // - George W. Clingerman: "Tell me what's wrong!", http://www.xnadevelopment.com/tutorials/tellmewhatswrong/tellmewhatswrong.shtml
    //   In this example George W. Clingerman shows how to handle error reporting
    //   and feedback in XBLIG.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _spriteFont;
    private string _message;

    // true while the message box is visible.
    private bool _showMessage = true;

    // true if the user selected the "View error details" option in the message box.
    private bool _showException;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the application that threw the exception.
    /// </summary>
    /// <value>
    /// The name of the application that threw the exception.
    /// The default value is the full name of the executing assembly.
    /// </value>
    public string ApplicationName { get; set; }


    /// <summary>
    /// Gets or sets the exception.
    /// </summary>
    /// <value>The exception.</value>
    public Exception Exception { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionGame"/> class.
    /// </summary>
    public ExceptionGame()
    {
      ApplicationName = Assembly.GetExecutingAssembly().FullName;
      _graphics = new GraphicsDeviceManager(this)
      {
        PreferredBackBufferWidth = 1280,
        PreferredBackBufferHeight = 720
      };
      Components.Add(new GamerServicesComponent(this));
      IsMouseVisible = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called after the Game and GraphicsDevice are created, but before LoadContent. 
    /// </summary>
    protected override void Initialize()
    {
      // Recursively collect all stack traces and inner exceptions.
      StringBuilder stringBuilder = new StringBuilder();
      if (string.IsNullOrEmpty(ApplicationName))
      {
        stringBuilder.AppendLine("An unexpected error has occurred.");
      }
      else
      {
        stringBuilder.Append("An unexpected error has occurred in ");
        stringBuilder.Append(ApplicationName);
        stringBuilder.AppendLine(".");
      }
      stringBuilder.AppendLine("Press Back on a gamepad or Escape on the keyboard to exit...");
      stringBuilder.AppendLine();
      stringBuilder.AppendLine("Exception text:");
      Exception currentException = Exception;
      while (currentException != null)
      {
        stringBuilder.AppendLine(currentException.Message);
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(currentException.StackTrace);
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Inner exception:");

        // Continue with inner exception.
        currentException = currentException.InnerException;
      }
      stringBuilder.AppendLine("-");
      _message = stringBuilder.ToString();

      base.Initialize();
    }


    /// <summary>
    /// Called when graphics resources need to be loaded. Override this method to load any
    /// game-specific graphics resources. 
    /// </summary>
    protected override void LoadContent()
    {
      try
      {
        // Configure ContentManager to load the SpriteFont.
        var titleStorage = new TitleStorage("Content");
        var vfsStorage = new VfsStorage();
        vfsStorage.MountInfos.Add(new VfsMountInfo(titleStorage, null));
#if MONOGAME
        var assetsStorage = new ZipStorage(titleStorage, "Content.zip");
        vfsStorage.MountInfos.Add(new VfsMountInfo(assetsStorage, null));
        var drStorage = new ZipStorage(titleStorage, "DigitalRune.zip");
        vfsStorage.MountInfos.Add(new VfsMountInfo(drStorage, null));
#endif
        Content = new StorageContentManager(Services, vfsStorage);

        // Load SpriteFont.
        _spriteFont = Content.Load<SpriteFont>("SpriteFont1");
        _spriteBatch = new SpriteBatch(GraphicsDevice);
      }
      catch
      {
        // Failed to load sprite font.
      }
    }


    /// <summary>
    /// Called when the game has determined that game logic needs to be processed. This might
    /// include the management of the game state, the processing of user input, or the updating of
    /// simulation data. Override this method with game-specific logic.
    /// </summary>
    /// <param name="gameTime">Time passed since the last call to Update.</param>
    protected override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // Exit when Back or Escape are pressed.
      if (_showException
          && (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back)
              || GamePad.GetState(PlayerIndex.Two).IsButtonDown(Buttons.Back)
              || GamePad.GetState(PlayerIndex.Three).IsButtonDown(Buttons.Back)
              || GamePad.GetState(PlayerIndex.Four).IsButtonDown(Buttons.Back)
              || Keyboard.GetState().IsKeyDown(Keys.Escape)))
      {
        Exit();
      }

      try
      {
        if (_showMessage && !Guide.IsVisible)
        {
          // The message box has not been opened or closed yet.

          // If Exception info and a sprite font are available, then we offer the user an option
          // to view the exception details.
          string[] buttons;
          if (Exception != null)
            buttons = new[] { "Exit to Dashboard", "View Error Details" };
          else
            buttons = new[] { "Exit to Dashboard" };

          Guide.BeginShowMessageBox(
            PlayerIndex.One,
            "Unexpected Error",
            "The game had an unexpected error and had to shut down. We are sorry for the inconvenience.",
            buttons,
            0,
            MessageBoxIcon.Error,
            OnMessageBoxEnd,
            null);

          _showMessage = false;
        }
      }
      catch
      {
        // Opening the message box failed. Ignore exception and show error directly.
        _showMessage = false;
        _showException = true;
      }
    }


    /// <summary>
    /// Called when the Guide message box was closed.
    /// </summary>
    /// <param name="asyncResult">The async result.</param>
    private void OnMessageBoxEnd(IAsyncResult asyncResult)
    {
      int? button = Guide.EndShowMessageBox(asyncResult);

      if (button.HasValue && button.Value == 1)
        _showException = true;
      else
        Exit();
    }


    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.Black);

      if (_showException && _spriteBatch != null && _spriteFont != null)
      {
        Point upperLeft = GraphicsDevice.Viewport.TitleSafeArea.Location;

        _spriteBatch.Begin();
        _spriteBatch.DrawString(
          _spriteFont,
          _message,
          new Vector2(upperLeft.X, upperLeft.Y),
          Color.White);
        _spriteBatch.End();
      }

      base.Draw(gameTime);
    }
    #endregion
  }
}
