using System;
using System.Windows.Media;
using DigitalRune.Game.Timing;
using DigitalRune.Graphics;
using DigitalRune.ServiceLocation;
using DigitalRune.Storages;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace WpfInteropSample2
{
  // This class implements a simple game loop. The 3D graphics are rendered into the 
  // MyGamePresentationTargets.
  // The MyGame class renders only graphics, but other services (e.g. physics, animations,
  // game objects, etc.) could be added as well.
  internal class MyGame
  {
    private readonly ServiceContainer _serviceContainer;
    private readonly HighPrecisionClock _clock;
    private readonly IGameTimer _timer;
    private readonly ContentManager _contentManager;
    private readonly GraphicsManager _graphicsManager;


    public MyGame()
    {
      // ----- Service Container
      // The MyGame uses a ServiceContainer, which is a simple service locator 
      // and Inversion of Control (IoC) container. (The ServiceContainer can be 
      // replaced by any other container that implements System.IServiceProvider.)
      _serviceContainer = new ServiceContainer();
      ServiceLocator.SetLocatorProvider(() => _serviceContainer);

      // ----- Storage
      // Create a "virtual file system" for reading game assets.
      var titleStorage = new TitleStorage("Content");
      var assetsStorage = new ZipStorage(titleStorage, "Content.zip");
      var digitalRuneStorage = new ZipStorage(titleStorage, "DigitalRune.zip");
      var vfsStorage = new VfsStorage();
      vfsStorage.MountInfos.Add(new VfsMountInfo(titleStorage, null));
      vfsStorage.MountInfos.Add(new VfsMountInfo(assetsStorage, null));
      vfsStorage.MountInfos.Add(new VfsMountInfo(digitalRuneStorage, null));

      // ----- Content
      _contentManager = new StorageContentManager(ServiceLocator.Current, vfsStorage);
      _serviceContainer.Register(typeof(ContentManager), null, _contentManager);

      // ----- Graphics
      // Create Direct3D 11 device.
      var presentationParameters = new PresentationParameters
      {
        BackBufferWidth = 1,
        BackBufferHeight = 1,
        // Do not associate graphics device with any window.
        DeviceWindowHandle = IntPtr.Zero,
      };
      var graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, presentationParameters);

      // An IGraphicsDeviceService is required by the MonoGame/XNA content manager.
      _serviceContainer.Register(typeof(IGraphicsDeviceService), null, new DummyGraphicsDeviceManager(graphicsDevice));

      // Create and register the graphics manager.
      _graphicsManager = new GraphicsManager(graphicsDevice, _contentManager);
      _serviceContainer.Register(typeof(IGraphicsService), null, _graphicsManager);

      // ----- Timing
      // We can use the CompositionTarget.Rendering event to trigger our game loop.
      // The CompositionTarget.Rendering event is raised once per frame by WPF.

      // To measure the time that has passed, we use a HighPrecisionClock.
      _clock = new HighPrecisionClock();
      _clock.Start();
      CompositionTarget.Rendering += (s, e) => _clock.Update();

      // The FixedStepTimer reads the clock and triggers the game loop at 60 Hz.
      //_timer = new FixedStepTimer(_clock)
      //{
      //  StepSize = new TimeSpan(166667), // ~60 Hz
      //  AccumulateTimeSteps = false,
      //};
      // The VariableStepTimer reads the clock and triggers the game loop as often
      // as possible.
      _timer = new VariableStepTimer(_clock);
      _timer.TimeChanged += (s, e) => GameLoop(e.DeltaTime);
      _timer.Start();
    }


    private void GameLoop(TimeSpan deltaTime)
    {
      // Update graphics service and graphics screens.
      _graphicsManager.Update(deltaTime);

      // Render the current graphics screens into the presentation targets.
      foreach (var presentationTarget in _graphicsManager.PresentationTargets)
        _graphicsManager.Render(presentationTarget);
    }
  }
}