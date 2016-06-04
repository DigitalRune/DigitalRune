using System;
using System.Diagnostics;
using DigitalRune.Animation;
using DigitalRune.Diagnostics;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles;
using DigitalRune.Physics;
using DigitalRune.ServiceLocation;
using DigitalRune.Storages;
using DigitalRune.Threading;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if XBOX
using Microsoft.Xna.Framework.GamerServices;
#endif


namespace Samples
{
  // The main game class, which creates and updates the game services (physics, 
  // graphics, animation, etc.). It also adds the MenuComponent which will 
  // load/switch the samples and end the application.
  public class SampleGame : Microsoft.Xna.Framework.Game
  {
    // The XNA GraphicsDeviceManager.
    private readonly GraphicsDeviceManager _graphicsDeviceManager;

    // The IoC service container providing access to all services.
    private ServiceContainer _services;

    // The SampleFramework manages loading of samples and switching between samples.
    private SampleFramework _sampleFramework;

    // Services of the game:
    private InputManager _inputManager;                   // Input
    private GraphicsManager _graphicsManager;             // Graphics
    private UIManager _uiManager;                         // GUI
    private AnimationManager _animationManager;           // Animation
    private ParticleSystemManager _particleSystemManager; // Particle simulation
    private Simulation _simulation;                       // Physics simulation
    private GameObjectManager _gameObjectManager;         // Game logic
    private HierarchicalProfiler _profiler;               // Profiler for game loop

    // Animation, physics simulation, and particle simulation may run in threads
    // parallel to input and graphics. For this we need some delegates and tasks.
    private Action _updateAnimation;
    private Action _updatePhysics;
    private Action _updateParticles;
    private Task _updateAnimationTask;
    private Task _updatePhysicsTask;
    private Task _updateParticlesTask;

    // The physics and particles simulation can be paused for debugging. (Only works 
    // in single-threaded game loop. This flag is ignored in multi-threaded game loop.)
    private bool _isSimulationPaused;

    // The size of the current time step.
    private TimeSpan _deltaTime;


    // Enables/Disables the multi-threaded game loop. If enabled, certain game 
    // services will be updated in parallel.
    public bool EnableParallelGameLoop { get; set; }


    public SampleGame()
    {
#if MONOGAME && DEBUG
      // Optional: Enable Direct3D debug layer to get additional debug information during development.
      //GraphicsAdapter.UseDebugDevice = true;
#endif

      _graphicsDeviceManager = new GraphicsDeviceManager(this)
      {
#if WINDOWS_PHONE || IOS
        PreferredBackBufferWidth = 800,
        PreferredBackBufferHeight = 480,
        IsFullScreen = true,   // Set fullscreen to hide the Windows Phone status bar.
        SupportedOrientations = DisplayOrientation.LandscapeLeft,
#elif XBOX
        // Following resolution is used in several AAA games on Xbox 360.
        PreferredBackBufferWidth = 1024,
        PreferredBackBufferHeight = 600,
#elif ANDROID
        // Using PreferredBackBufferWidth/Height does not work in Android. 
        // We would have to manually render into a low resolution render target and upscale the result.
        // This sample simply uses the default device resolution.
        IsFullScreen = true,
        SupportedOrientations = DisplayOrientation.LandscapeLeft,
#else
        PreferredBackBufferWidth = 1280,
        PreferredBackBufferHeight = 720,
#endif
        PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
        PreferMultiSampling = false,
        SynchronizeWithVerticalRetrace = true,
#if !WINDOWS && !WINDOWS_UWP && !XBOX
        // The DigitalRune builds for Windows, Universal Windows Apps and Xbox 360 support
        // HiDef effects. The other platforms support currently only Reach.
        GraphicsProfile = GraphicsProfile.Reach,
#endif
      };

      IsMouseVisible = false;
      IsFixedTimeStep = false;
    }


    // Initializes services and adds game components.
    protected override void Initialize()
    {
#if WINDOWS || WINDOWS_UWP || XBOX
      if (GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
      {
        throw new NotSupportedException(
          "The DigitalRune Samples and Content for Windows, Universal Windows Apps and Xbox 360 are " +
          "designed for the HiDef graphics profile. A graphics cards supporting DirectX 10.0 or better " +
          "is required.");
      }
#endif

      // ----- Service Container
      // The DigitalRune ServiceContainer is an "inversion of control" container.
      // All game services (such as input, graphics, physics, etc.) are registered
      // in this container. Other game components can access these services via lookup
      // in the service container.
      // The DigitalRune ServiceContainer replaces the XNA GameServiceContainer (see 
      // property Game.Services).

      // Note: The DigitalRune libraries do not require the use of the ServiceContainer
      // or any other IoC container. The ServiceContainer is only used in the sample
      // for convenience - but it is not mandatory.
      _services = new ServiceContainer();

      // The service container is either passed directly to the game components
      // or accessed through the global variable ServiceLocator.Current.
      // The following call makes the service container publicly available in 
      // ServiceLocator.Current.
      ServiceLocator.SetLocatorProvider(() => _services);

      // ----- Storage
      // For XNA the assets are stored in the following folders:
      //
      //   <gameLocation>/
      //     Content/
      //       DigitalRune/
      //         ... DigitalRune assets ...
      //       ... other assets ...
      //
      // For MonoGame the assets (*.xnb files) are stored in ZIP packages. The
      // sample assets are stored in "Content/Content.zip" and the DigitalRune
      // assets are stored in "Content/DigitalRune.zip".
      //
      //   <gameLocation>/
      //     Content/
      //       Content.zip
      //       DigitalRune.zip
      //
      // DigitalRune introduces the concept of "storages". Storages can be used
      // to access files on disk or files stored in packages (e.g. ZIP archives).
      // These storages can be mapped into a "virtual file system", which makes
      // it easier to write portable code. (The game logic can read the files
      // from the virtual file system and does not need to know the specifics
      // about the platform.)
      //
      // The virtual files system should look like this:
      //
      //   /                                     <-- root of virtual file system
      //       DigitalRune/
      //           ... DigitalRune assets ...
      //       ... other assets ...

      // The VfsStorage creates a virtual file system.
      var vfsStorage = new VfsStorage();

      // The TitleStorage reads files from the game's default storage location.
      // --> Create a TitleStorage that reads files from "<gameLocation>/Content".
      var titleStorage = new TitleStorage("Content");

#if MONOGAME
      // A ZipStorage can be used to access files inside a ZIP archive.
      // --> Mount the sample assets to the root of the virtual file system.
      var assetsStorage = new ZipStorage(titleStorage, "Content.zip");
      vfsStorage.MountInfos.Add(new VfsMountInfo(assetsStorage, null));

#if !ANDROID && !IOS && !LINUX && !MACOS
      // --> Mount the DigitalRune assets to the root of the virtual file system.
      var drStorage = new ZipStorage(titleStorage, "DigitalRune.zip");
      vfsStorage.MountInfos.Add(new VfsMountInfo(drStorage, null));
#endif
#endif

      // Finally, map the TitleStorage to the root of the virtual file system.
      // (The TitleStorage is added as the last mount point. The ZIP archives
      // have priority.)
      vfsStorage.MountInfos.Add(new VfsMountInfo(titleStorage, null));

      // Register the virtual file system as a service.
      _services.Register(typeof(IStorage), null, vfsStorage);

      // ----- Content Managers
      // The GraphicsDeviceManager needs to be registered in the service container.
      // (This is required by the XNA content managers.)
      _services.Register(typeof(IGraphicsDeviceService), null, _graphicsDeviceManager);
      _services.Register(typeof(GraphicsDeviceManager), null, _graphicsDeviceManager);

      // Register a default, shared content manager.
      // The new StorageContentManager can be used to read assets from the virtual
      // file system. (Replaces the content manager stored in Game.Content.)
      Content = new StorageContentManager(_services, vfsStorage);
      _services.Register(typeof(ContentManager), null, Content);

      // Create and register content manager that will be used to load the GUI.
#if !MONOGAME
      var uiContentManager = new ContentManager(_services, "Content");
#else
      var uiContentManager = new StorageContentManager(_services, assetsStorage);
#endif
      _services.Register(typeof(ContentManager), "UIContent", uiContentManager);

      // Create content manager that will be used exclusively by the graphics service
      // to load the pre-built effects and resources of DigitalRune.Graphics. (We
      // could use Game.Content, but it is recommended to separate the content. This 
      // allows to unload the content of the samples without unloading the other 
      // content.)
      var graphicsContentManager = new StorageContentManager(_services, vfsStorage);

      // ----- Initialize Services
      // Register the game class.
      _services.Register(typeof(Microsoft.Xna.Framework.Game), null, this);
      _services.Register(typeof(SampleGame), null, this);

#if XBOX
      // On Xbox, we use the XNA gamer services (e.g. for text input).
      Components.Add(new GamerServicesComponent(this));
#endif

      // Input
#if XBOX
      const bool useGamerServices = true;
#else
      const bool useGamerServices = false;
#endif
      _inputManager = new InputManager(useGamerServices);
      _services.Register(typeof(IInputService), null, _inputManager);

      // Graphics
      _graphicsManager = new GraphicsManager(GraphicsDevice, Window, graphicsContentManager);
      _services.Register(typeof(IGraphicsService), null, _graphicsManager);

      // GUI
      _uiManager = new UIManager(this, _inputManager);
      _services.Register(typeof(IUIService), null, _uiManager);

      // Animation
      _animationManager = new AnimationManager();
      _services.Register(typeof(IAnimationService), null, _animationManager);

      // Particle simulation
      _particleSystemManager = new ParticleSystemManager();
      _services.Register(typeof(IParticleSystemService), null, _particleSystemManager);

      // Physics simulation
      ResetPhysicsSimulation();

      // Game logic
      _gameObjectManager = new GameObjectManager();
      _services.Register(typeof(IGameObjectService), null, _gameObjectManager);

      // Profiler
      _profiler = new HierarchicalProfiler("Main");
      _services.Register(typeof(HierarchicalProfiler), "Main", _profiler);

      // Initialize delegates for running tasks in parallel.
      // (Creating delegates allocates memory, therefore we do this only once and
      // cache the delegates.)
      _updateAnimation = () => _animationManager.Update(_deltaTime);
      _updatePhysics = () => _simulation.Update(_deltaTime);
      _updateParticles = () => _particleSystemManager.Update(_deltaTime);

      // SampleFramework
      // The SampleFramework automatically discovers all samples using reflection, provides 
      // controls for switching samples and starts the initial sample.
#if KINECT
      var initialSample = typeof(Kinect.KinectSkeletonMappingSample);
#elif WINDOWS || WINDOWS_UWP
      var initialSample = typeof(Graphics.DeferredLightingSample);
#else
      var initialSample = typeof(Graphics.BasicEffectSample);
#endif
      _sampleFramework = new SampleFramework(this, initialSample);
      _services.Register(typeof(SampleFramework), null, _sampleFramework);

      base.Initialize();
    }


    public void ResetPhysicsSimulation()
    {
      _simulation = new Simulation();

      // Limit max. number of internal simulation steps to 2.
      // (Simulation.Settings.Timing.FixedTimeStep is 1/60 s per default. If the game is
      // running with 60 fps, Simulation.Update() internally makes on simulation step. If 
      // the game is running with 30 fps, Simulation.Update() makes two internal steps with 
      // 1/60s. If the game is running with less than 30 fps, the simulation will run in 
      // slow motion because more than 2 internal steps would have to be made but the 
      // limit is two. - This way the game will not freeze.)
      _simulation.Settings.Timing.MaxNumberOfSteps = 2;

      // When the simulation is updated with Simulation.Update() the collision detection is 
      // updated first and then the physics simulation computes new forces, velocities and 
      // positions. That means, that after Simulation.Update() the collision detection information
      // (Simulation.CollisionDomain) is not up-to-date! If we manually query the collision 
      // detection using Simulation.CollisionDomain, then we can set the SynchronizeCollisionDomain
      // flag. If this flag is set, the collision detection info is updated at the beginning 
      // and at the end of Simulation.Update().
      //Simulation.Settings.SynchronizeCollisionDomain = true;

      // The collision domain computes collision information between non-moving bodies only once 
      // and caches this information - in case someone wants to check if two static bodies touch.
      // Nevertheless, on less powerful systems, like the Xbox 360, it can still improve performance 
      // to filter collisions between static bodies. This can be done in a broad phase filter based 
      // on collision groups, or with a simple filter like this: 
      _simulation.CollisionDomain.BroadPhase.Filter = new DelegatePairFilter<CollisionObject>(
        pair =>
        {
          var bodyA = pair.First.GeometricObject as RigidBody;
          if (bodyA == null || bodyA.MotionType != MotionType.Static)
            return true;

          var bodyB = pair.Second.GeometricObject as RigidBody;
          if (bodyB == null || bodyB.MotionType != MotionType.Static)
            return true;

          return false;   // Do not compute collisions between two static bodies.
        });

      // Another way to filter collisions is to use the CollisionDetection.CollisionFilter. 
      // Filtering on this level is slower because the filter is applied after the broad phase and 
      // the broad phase filter. However, it is more flexible. It can be changed at runtime, whereas
      // a broad phase filter should not change after the simulation was initialized.
      var filter = (ICollisionFilter)_simulation.CollisionDomain.CollisionDetection.CollisionFilter;
      // We can disable collision for pairs of collision objects or for collision groups. Here,
      // we disable collisions between collision group 1 and 2. The ray for mouse picking will
      // use collision group 2 (see GrabObject.cs). Any objects that should not be pickable can use
      // collision group 1.
      filter.Set(1, 2, false);

      _services.Register(typeof(Simulation), null, _simulation);
    }


    // Updates the different sub-systems (input, physics, game logic, ...).
    protected override void Update(GameTime gameTime)
    {
      _deltaTime = gameTime.ElapsedGameTime;

      // Tell the profiler that a new frame has begun.
      _profiler.NewFrame();
      _profiler.Start("Update");

      // Update input manager. The input manager gets the device states and performs other work.
      // (Note: XNA requires that the input service is run on the main thread!)
      _profiler.Start("InputManager.Update             ");
      _inputManager.Update(_deltaTime);
      _profiler.Stop();

      if (EnableParallelGameLoop)
      {
        // In a parallel game loop animation, physics and particles are started at
        // the end of the Update method. The services are now running in parallel. 
        // --> Wait for services to finish.
        _updateAnimationTask.Wait();
        _updatePhysicsTask.Wait();
        _updateParticlesTask.Wait();

        // Now, nothing is running in parallel anymore and we can apply the animations.
        // (This means the animation values are written to the objects and properties
        // that are being animated.)
        _animationManager.ApplyAnimations();
      }
      else
      {
        // Update animation, physics, particles sequentially.

        // For debugging we can pause the physics and particle simulations with <P>,
        // and execute single simulation steps with <T>.
        if (_inputManager.IsPressed(Keys.P, true))
          _isSimulationPaused = !_isSimulationPaused;

        if (!_isSimulationPaused || _inputManager.IsPressed(Keys.T, true))
        {
          // Update physics simulation.
          _profiler.Start("Simulation.Update               ");
          _simulation.Update(_deltaTime);
          _profiler.Stop();

          // Update particles.
          _profiler.Start("ParticleSystemManager.Update    ");
          _particleSystemManager.Update(_deltaTime);
          _profiler.Stop();
        }

        // Update animations.
        // (The animation results are stored internally but not yet applied).
        _profiler.Start("AnimationManger.Update          ");
        _animationManager.Update(_deltaTime);
        _profiler.Stop();

        // Apply animations.
        // (The animation results are written to the objects and properties that 
        // are being animated. ApplyAnimations() must be called at a point where 
        // it is thread-safe to change the animated objects and properties.)
        _profiler.Start("AnimationManager.ApplyAnimations");
        _animationManager.ApplyAnimations();
        _profiler.Stop();
      }

      // Run any task completion callbacks that have been scheduled.
      _profiler.Start("Parallel.RunCallbacks           ");
      Parallel.RunCallbacks();
      _profiler.Stop();

      // When the menu is visible, update SampleFramework before the game logic.
      if (_sampleFramework.IsMenuVisible)
        _sampleFramework.Update();

      // Update XNA GameComponents.
      _profiler.Start("base.Update                     ");
      base.Update(gameTime);
      _profiler.Stop();

      // Update UI manager. The UI manager updates all registered UIScreens.
      _profiler.Start("UIManager.Update                ");
      _uiManager.Update(_deltaTime);
      _profiler.Stop();

      // Update DigitalRune GameObjects.
      _profiler.Start("GameObjectManager.Update        ");
      _gameObjectManager.Update(_deltaTime);
      _profiler.Stop();

      // When the menu is hidden, update SampleFramework after the game logic.
      if (!_sampleFramework.IsMenuVisible)
        _sampleFramework.Update();

      if (EnableParallelGameLoop)
      {
        // Start animation, physics and particle simulation. They will be executed 
        // parallel to the graphics rendering in Draw().
        _updateAnimationTask = Parallel.Start(_updateAnimation);
        _updatePhysicsTask = Parallel.Start(_updatePhysics);
        _updateParticlesTask = Parallel.Start(_updateParticles);
      }

      _profiler.Stop();
    }


    // Draws the game content.
    protected override void Draw(GameTime gameTime)
    {
      // Manually clear background. 
      // (This is not really necessary because the individual samples are 
      // responsible for rendering. However, if we skip this Clear() on Android 
      // then we can see trash in the back buffer when switching between samples.)
      GraphicsDevice.Clear(Color.Black);

      _profiler.Start("Draw");

      _sampleFramework.Draw(); // Does not draw anything - just increments the FPS counter.

      // Render all DrawableGameComponents registered in Components.
      _profiler.Start("base.Draw                       ");
      base.Draw(gameTime);
      _profiler.Stop();

      // Update the graphics (including graphics screens).
      // Important, if symbol EnableParallelGameLoop is true: Currently 
      // animation, physics and particles are running in parallel. Therefore, 
      // the GraphicsScreen.OnUpdate() methods must not influence the animation,
      // physics or particle state!
      _profiler.Start("GraphicsManager.Update          ");
      _graphicsManager.Update(gameTime.ElapsedGameTime);
      _profiler.Stop();

      // Render graphics screens to the back buffer.
      _profiler.Start("GraphicsManager.Render          ");
      _graphicsManager.Render(false);
      _profiler.Stop();

      _profiler.Stop();
    }
  }
}
