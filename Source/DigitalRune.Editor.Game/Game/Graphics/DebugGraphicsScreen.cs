// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Linq;
using System.Text;
using DigitalRune.Diagnostics;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Text;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Draws debugging information.
    /// </summary>
    public class DebugGraphicsScreen : GraphicsScreen, IDisposable
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        // Colors used to highlight the safe areas:
        //  Invisible area > action safe area > title safe area
        private static readonly Color InvisibleAreaColor = new Color(255, 0, 0, 127);
        private static readonly Color ActionSafeAreaColor = new Color(255, 255, 0, 127);
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _isDisposed;
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _whiteTexture;

        // For frame rate texts:
        private readonly DebugRenderer _internalDebugRenderer;
        private int _numberOfUpdates;
        private int _numberOfDraws;
        private Stopwatch _stopwatch;
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private float _textWidth;
        private readonly GameExtension _gameExtension;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the debug renderer.
        /// </summary>
        /// <value>The debug renderer.</value>
        public DebugRenderer DebugRenderer { get; }


        /// <summary>
        /// Gets or sets the camera node.
        /// </summary>
        /// <value>The camera node. The default value is <see langword="null"/>.</value>
        public CameraNode CameraNode { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the title safe area should be visualized.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the title safe area should be visualized; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// If this flag is set, the action safe area is tinted orange. And the not action safe area
        /// is tinted red. The title safe area is not colorized.
        /// </remarks>
        public bool ShowTitleSafeArea { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugGraphicsScreen" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public DebugGraphicsScreen(IServiceLocator services)
              : base(services?.GetInstance<IGraphicsService>())
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            Coverage = GraphicsScreenCoverage.Partial;
            _spriteBatch = GraphicsService.GetSpriteBatch();
            _whiteTexture = GraphicsService.GetDefaultTexture2DWhite();

            var contentManager = services.GetInstance<ContentManager>();
            var spriteFont = contentManager.Load<SpriteFont>("DigitalRune.Editor.Game/Fonts/DejaVuSans");
            DebugRenderer = new DebugRenderer(GraphicsService, spriteFont);
            _internalDebugRenderer = new DebugRenderer(GraphicsService, spriteFont);

            // To count the update frame rate, we handle the GameLogicUpdating event.
            // (We cannot use GraphicsScreen.OnUpdate because it is only called at the same rate if
            // the graphics screen is registered in the graphics service. If it is not registered,
            // then OnUpdate and OnRender are always called together.)
            var editor = services.GetInstance<IEditorService>();
            _gameExtension = editor.Extensions.OfType<GameExtension>().FirstOrDefault();
            if (_gameExtension != null)
                _gameExtension.GameLogicUpdating += OnGameLogicUpdating;
        }


        /// <summary>
        /// Releases unmanaged resources before an instance of the <see cref="DebugGraphicsScreen"/>
        /// class is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This method releases unmanaged resources by calling the virtual
        /// <see cref="Dispose(bool)"/> method, passing in <see langword="false"/>.
        /// </remarks>
        ~DebugGraphicsScreen()
        {
            Dispose(false);
        }


        /// <summary>
        /// Releases all resources used by an instance of the <see cref="DebugGraphicsScreen"/> class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in
        /// <see langword="true"/>, and then suppresses finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="DebugGraphicsScreen"/> class
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "<DebugRenderer>k__BackingField")]
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    DebugRenderer.Dispose();
                    _internalDebugRenderer.Dispose();

                    if (_gameExtension != null)
                        _gameExtension.GameLogicUpdating -= OnGameLogicUpdating;
                }

                // Release unmanaged resources.

                _isDisposed = true;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnGameLogicUpdating(object sender, EventArgs eventArgs)
        {
            _numberOfUpdates++;
        }


        /// <inheritdoc/>
        protected override void OnUpdate(TimeSpan deltaTime)
        {
            if (_stopwatch == null)
                _stopwatch = Stopwatch.StartNew();
            
            // Show "Update FPS" and "Draw FPS" in upper right corner.
            const float UpdateInterval = 1; 
            if (_stopwatch.Elapsed.TotalSeconds > UpdateInterval)
            {
                {
                    _stringBuilder.Clear();
                    _stringBuilder.Append("Update: ");
                    float fps = (float)Math.Round(_numberOfUpdates / _stopwatch.Elapsed.TotalSeconds);
                    _stringBuilder.AppendNumber((int)fps);
                    _stringBuilder.Append(" fps, ");
                    _stringBuilder.AppendNumber(1 / fps * 1000, 2, AppendNumberOptions.None);
                    _stringBuilder.Append(" ms");
                }
                {
                    _stringBuilder.AppendLine();
                    _stringBuilder.Append("Draw: ");
                    float fps = (float)Math.Round(_numberOfDraws / _stopwatch.Elapsed.TotalSeconds);
                    _stringBuilder.AppendNumber((int)fps);
                    _stringBuilder.Append(" fps, ");
                    _stringBuilder.AppendNumber(1 / fps * 1000, 2, AppendNumberOptions.None);
                    _stringBuilder.Append(" ms");
                }

                _textWidth = _internalDebugRenderer.SpriteFont.MeasureString(_stringBuilder).X;

                _numberOfUpdates = 0;
                _numberOfDraws = 0;
                _stopwatch.Reset();
                _stopwatch.Start();
            }
        }


        /// <inheritdoc/>
        protected override void OnRender(RenderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _numberOfDraws++;

            var graphicsDevice = GraphicsService.GraphicsDevice;
            context.CameraNode = CameraNode;

            DebugRenderer.Render(context);
            DrawTitleSafeArea(graphicsDevice);

            // Draw FPS text.
            _internalDebugRenderer.Clear();
            _internalDebugRenderer.DrawText(
                _stringBuilder,
                new Vector2F((int)(context.Viewport.Width - _textWidth - 5), 0),
                Color.Yellow);
            _internalDebugRenderer.Render(context);

            context.CameraNode = null;
        }


        private void DrawTitleSafeArea(GraphicsDevice graphicsDevice)
        {
            if (!ShowTitleSafeArea)
                return;

            var viewport = graphicsDevice.Viewport;
            var width = viewport.Width;
            var height = viewport.Height;
            var dx = (int)(width * 0.05f);     // 5% of the width
            var dy = (int)(height * 0.05f);    // 5% of the height

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

            // Highlight the invisible area.
            _spriteBatch.Draw(_whiteTexture, new Rectangle(0, 0, dx, height), InvisibleAreaColor);
            _spriteBatch.Draw(_whiteTexture, new Rectangle(width - dx, 0, dx, height), InvisibleAreaColor);
            _spriteBatch.Draw(_whiteTexture, new Rectangle(dx, 0, width - 2 * dx, dy), InvisibleAreaColor);
            _spriteBatch.Draw(_whiteTexture, new Rectangle(dx, height - dy, width - 2 * dx, dy), InvisibleAreaColor);

            // Highlight action safe area (center 90% of screen).
            _spriteBatch.Draw(_whiteTexture, new Rectangle(dx, dy, dx, height - 2 * dy), ActionSafeAreaColor);
            _spriteBatch.Draw(_whiteTexture, new Rectangle(width - 2 * dx, dy, dx, height - 2 * dy), ActionSafeAreaColor);
            _spriteBatch.Draw(_whiteTexture, new Rectangle(2 * dx, dy, width - 4 * dx, dy), ActionSafeAreaColor);
            _spriteBatch.Draw(_whiteTexture, new Rectangle(2 * dx, height - 2 * dy, width - 4 * dx, dy), ActionSafeAreaColor);

            // The center 80% is title safe.
            // These limits (80%, 90%) are standards for TV broadcasting and console development.

            _spriteBatch.End();
        }
        #endregion
    }
}
