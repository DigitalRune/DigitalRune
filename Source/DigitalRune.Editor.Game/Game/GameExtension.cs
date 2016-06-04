// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DigitalRune.Animation;
using DigitalRune.Collections;
using DigitalRune.Editor.Game.Properties;
using DigitalRune.Game.Timing;
using DigitalRune.Graphics;
using DigitalRune.Storages;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NLog;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Adds a game loop.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Services:</strong><br/>
    /// The extension adds the following services to the service container:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="IAnimationService"/></item>
    /// <item><see cref="ContentManager"/></item>
    /// <item><see cref="IGraphicsDeviceService"/></item>
    /// <item><see cref="IGraphicsService"/></item>
    /// </list>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public sealed class GameExtension : EditorExtension
    {
        // TODO: Make IGameService.
        // TODO: Implement full game loop with all services, game components and game objects.

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ResourceDictionary _resourceDictionary;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        private ContentManager _contentManager;
        private AnimationManager _animationManager;
        private GraphicsManager _graphicsManager;
        private HighPrecisionClock _clock;
        private IGameTimer _timer;
        private bool _idle;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Occurs each frame at the start of the game loop
        /// </summary>
        public event EventHandler<EventArgs> GameLoopNewFrame;


        /// <summary>
        /// Occurs before the game logic is updated.
        /// </summary>
        public event EventHandler<EventArgs> GameLogicUpdating;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            Editor.Services.Register(typeof(IMonoGameService), null, typeof(MonoGameService));
            AddGraphicsService();
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            AddDataTemplates();
            AddCommands();
            AddToolBars();
            AddGameLoop();
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            RemoveGameLoop();
            RemoveToolBars();
            RemoveCommands();
            RemoveDataTemplates();
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            RemoveGraphicsService();
            Editor.Services.Unregister(typeof(IMonoGameService));

            Settings.Default.Save();
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor.Game;component/Game/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_resourceDictionary);
        }


        private void RemoveDataTemplates()
        {
            EditorHelper.UnregisterResources(_resourceDictionary);
            _resourceDictionary = null;
        }


        private void AddCommands()
        {
            // Add items.
            CommandItems.Add(
                new DelegateCommandItem("ShowView3D", new DelegateCommand(ShowView3D))
                {
                    Category = "View",
                    Text = "Show 3D View",
                    ToolTip = "Show window with 3D view."
                });
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
        }


        private void AddToolBars()
        {
            _toolBarNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("Game"),
                    new MergeableNode<ICommandItem>(CommandItems["ShowView3D"])),
            };

            Editor.ToolBarNodeCollections.Add(_toolBarNodes);
        }


        private void RemoveToolBars()
        {
            Editor.MenuNodeCollections.Remove(_toolBarNodes);
            _toolBarNodes = null;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void AddGraphicsService()
        {
            // ----- Storage
            // Create a "virtual file system" for reading game assets.
            var titleStorage = new TitleStorage("Content");
            var assetsStorage = new ZipStorage(titleStorage, "DigitalRune.Editor.Game.zip");
            var digitalRuneStorage = new ZipStorage(titleStorage, "DigitalRune.zip");
            var vfsStorage = new VfsStorage();
            vfsStorage.MountInfos.Add(new VfsMountInfo(titleStorage, null));
            vfsStorage.MountInfos.Add(new VfsMountInfo(assetsStorage, null));
            vfsStorage.MountInfos.Add(new VfsMountInfo(digitalRuneStorage, null));

            // ----- Content
            _contentManager = new StorageContentManager(Editor.Services, vfsStorage);
            //_contentManager = new ContentManager(serviceContainer, "Content");
            Editor.Services.Register(typeof(ContentManager), null, _contentManager);

            // ----- Animations
            _animationManager = new AnimationManager();
            Editor.Services.Register(typeof(IAnimationService), null, _animationManager);

            // ----- Graphics
            // Create Direct3D 11 device.
            var presentationParameters = new PresentationParameters
            {
                BackBufferWidth = 1,
                BackBufferHeight = 1,
                DeviceWindowHandle = IntPtr.Zero
            };
            var graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, presentationParameters);
            Editor.Services.Register(typeof(IGraphicsDeviceService), null, new GraphicsDeviceManager(graphicsDevice));

            // Create and register the graphics manager.
            _graphicsManager = new GraphicsManager(graphicsDevice, _contentManager);
            Editor.Services.Register(typeof(IGraphicsService), null, _graphicsManager);
        }


        private void RemoveGraphicsService()
        {
            Editor.Services.Unregister(typeof(IGraphicsService));
            _graphicsManager.Dispose();

            Editor.Services.Unregister(typeof(IGraphicsDeviceService));
            _graphicsManager.GraphicsDevice.Dispose();
            _graphicsManager = null;

            Editor.Services.Unregister(typeof(ContentManager));
            _contentManager.Dispose();
            _contentManager = null;
        }


        private void AddGameLoop()
        {
            _clock = new HighPrecisionClock();
            _clock.Start();
            CompositionTarget.Rendering += OnCompositionTargetRendering;
            ComponentDispatcher.ThreadIdle += OnApplicationIdle;

            // The FixedStepTimer reads the clock and triggers the game loop at 60 Hz.
            _timer = new FixedStepTimer(_clock)
            {
                StepSize = new TimeSpan(166667), // ~60 Hz
                //StepSize = new TimeSpan(333333), // ~30 Hz
                AccumulateTimeSteps = false,
            };
            // The VariableStepTimer reads the clock and triggers the game loop as often
            // as possible.
            //_timer = new VariableStepTimer(_clock);
            _timer.TimeChanged += (s, e) => GameLoop(e.DeltaTime);
            _timer.Start();

            Editor.Services.Register(typeof(IGameTimer), null, _timer);
        }


        private void RemoveGameLoop()
        {
            Editor.Services.Unregister(typeof(IGameTimer));
            _timer.Stop();
            _timer = null;
            ComponentDispatcher.ThreadIdle -= OnApplicationIdle;
            CompositionTarget.Rendering -= OnCompositionTargetRendering;
            _clock.Stop();
            _clock = null;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }


        private void ShowView3D()
        {
            Editor.ActivateItem(new GameViewModel(Editor));
        }


        private void OnCompositionTargetRendering(object sender, EventArgs eventArgs)
        {
            _clock.Update();
            _idle = false;
        }


        private void OnApplicationIdle(object sender, EventArgs eventArgs)
        {
            _idle = true;
        }


        private void GameLoop(TimeSpan deltaTime)
        {
            //if (_idle)
            {
                // Update game services and render screens.

                GameLoopNewFrame?.Invoke(this, EventArgs.Empty);

                // Update animation service.
                _animationManager.Update(deltaTime);
                _animationManager.ApplyAnimations();

                GameLogicUpdating?.Invoke(this, EventArgs.Empty);

                // Update graphics service.
                _graphicsManager.Update(deltaTime);

                // If the game runs slowly, the timer will call this method several times. Only the
                // last call should actually draw to the screen.
                var fixedStepTimer = _timer as FixedStepTimer;
                if (fixedStepTimer != null && fixedStepTimer.PendingSteps > 0)
                    return;

                // Render the current graphics screens into the presentation targets.
                foreach (var presentationTarget in _graphicsManager.PresentationTargets)
                {
                    var gamePresentationTarget = presentationTarget as GamePresentationTarget;
                    if (gamePresentationTarget == null)
                        continue;

                    //if ("gamePresentationTarget needs to be rendered")    // TODO: Add support for views that are not rendered every frame.
                    {
                        var graphicsScreens = gamePresentationTarget.GraphicsScreens;
                        if (graphicsScreens != null)
                        {
                            // To reduce flickering: When the user is resizing the window, the graphics
                            // resources have to be recreated. We do not want to do this every frame
                            // because this is unnecessary work and creates flickering. Let's wait until
                            // the user is finished (= application is idle).
                            var size = gamePresentationTarget.RenderSize;
                            if (size != gamePresentationTarget.LastSize && !_idle)
                                continue;

                            gamePresentationTarget.IsDirty = !gamePresentationTarget.IsSynchronized;
                            gamePresentationTarget.LastSize = size;

                            try
                            {
                                _graphicsManager.Render(presentationTarget, graphicsScreens);
                            }
                            catch (Exception exception)
                            {
                                Logger.Error(exception, "Error while rendering graphics screen.");
                            }
                        }
                    }
                    //else
                    //{
                    //    // Update D3DImage (no rendering).
                    //    if (gamePresentationTarget.IsDirty          // The WPF front buffer may not be up-to-date.
                    //        && gamePresentationTarget.IsFrameReady  // The Direct3D 11 back buffer is up-to-date.
                    //        && gamePresentationTarget.IsVisible)
                    //    {
                    //        var d3dImage = (D3DImage)gamePresentationTarget.Source;
                    //        d3dImage.Lock();
                    //        d3dImage.AddDirtyRect(new Int32Rect(0, 0, d3dImage.PixelWidth, d3dImage.PixelHeight));
                    //        d3dImage.Unlock();

                    //        gamePresentationTarget.IsDirty = false;
                    //    }
                    //}
                }
            }
            //else
            //{
            //    // Update D3DImage (no rendering).
            //    foreach (var presentationTarget in _graphicsManager.PresentationTargets)
            //    {
            //        var gamePresentationTarget = presentationTarget as GamePresentationTarget;
            //        if (gamePresentationTarget != null
            //            && gamePresentationTarget.IsDirty       // The WPF front buffer may not be up-to-date.
            //            && gamePresentationTarget.IsFrameReady  // The Direct3D 11 back buffer is up-to-date.
            //            && gamePresentationTarget.IsVisible)
            //        {
            //            var d3dImage = (D3DImage)gamePresentationTarget.Source;
            //            d3dImage.Lock();
            //            d3dImage.AddDirtyRect(new Int32Rect(0, 0, d3dImage.PixelWidth, d3dImage.PixelHeight));
            //            d3dImage.Unlock();

            //            gamePresentationTarget.IsDirty = false;
            //        }
            //    }
            //}
        }
        #endregion
    }
}
