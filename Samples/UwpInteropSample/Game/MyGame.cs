using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System.Threading;
using DigitalRune.Game.Timing;
using DigitalRune.Graphics;
using DigitalRune.ServiceLocation;
using DigitalRune.Storages;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Task = System.Threading.Tasks.Task;


namespace UwpInteropSample
{
  // This class implements a simple game loop. The 3D graphics are rendered into the 
  // MyGamePresentationTargets.
  // The MyGame class renders only graphics, but other services (e.g. physics, animations,
  // game objects, etc.) could be added as well.
  // 
  // Thread-safety: To remove work from the UI thread, the game loop runs in a parallel thread.
  // Code which uses game services must use the Lock object to synchronize access.
  internal sealed class MyGame : IDisposable
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isDisposed;
    
    private readonly TitleStorage _titleStorage;
    private readonly ZipStorage _assetsStorage;
    private readonly ZipStorage _digitalRuneStorage;
    private readonly VfsStorage _vfsStorage;
    private readonly ServiceContainer _serviceContainer;
    private readonly HighPrecisionClock _clock;
    private readonly IGameTimer _timer;
    private readonly ContentManager _contentManager;
    private readonly GraphicsManager _graphicsManager;
    private readonly SharpDX.DXGI.Output _dxgiOutput;
    private readonly Task _gameLoopTask;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    public object Lock { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public MyGame()
    {
      Lock = new object();

      // ----- Service Container
      // MyGame uses a ServiceContainer, which is a simple service locator and
      // Inversion of Control (IoC) container. (The ServiceContainer can be
      // replaced by any other container that implements System.IServiceProvider.)
      _serviceContainer = new ServiceContainer();
      ServiceLocator.SetLocatorProvider(() => _serviceContainer);

      _serviceContainer.Register(typeof(MyGame), null, this);

      // ----- Storage
      // Create a "virtual file system" for reading game assets.
      _titleStorage = new TitleStorage("Content");
      _assetsStorage = new ZipStorage(_titleStorage, "Content.zip");
      _digitalRuneStorage = new ZipStorage(_titleStorage, "DigitalRune.zip");
      _vfsStorage = new VfsStorage();
      _vfsStorage.MountInfos.Add(new VfsMountInfo(_assetsStorage, null));
      _vfsStorage.MountInfos.Add(new VfsMountInfo(_digitalRuneStorage, null));

      // ----- Content
      _contentManager = new StorageContentManager(ServiceLocator.Current, _vfsStorage);
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

      // Get DXGIOutput to call WaitForVerticalBlank() in the game loop.
      using (var dxgiFactory = new SharpDX.DXGI.Factory1())
      using (var dxgiAdapter = dxgiFactory.GetAdapter1(0))
        _dxgiOutput = dxgiAdapter.GetOutput(0);

      // Create and register the graphics manager.
      _graphicsManager = new GraphicsManager(graphicsDevice, _contentManager);
      _serviceContainer.Register(typeof(IGraphicsService), null, _graphicsManager);

      // ----- Timing
      // The game loop runs in a parallel thread to keep the UI thread responsive.
      // To measure the time that has passed, we use a HighPrecisionClock.
      _clock = new HighPrecisionClock();
      _clock.Start();
      _gameLoopTask = ThreadPool.RunAsync(GameLoopTaskAction, WorkItemPriority.High, WorkItemOptions.TimeSliced)
                                .AsTask();

      // The FixedStepTimer reads the clock and triggers the game loop at 60 Hz.
      //_timer = new FixedStepTimer(_clock)
      //{
      //  StepSize = new TimeSpan(166667), // ~60 Hz
      //  AccumulateTimeSteps = false,
      //};

      // The VariableStepTimer reads the clock and triggers the game loop as often as possible.
      _timer = new VariableStepTimer(_clock);

      _timer.TimeChanged += (s, e) => GameLoop(e.DeltaTime);
      _timer.Start();

      CoreApplication.Suspending += OnCoreApplicationSuspending;

      // DirectX buffers only a limit amount of Present calls per frame which is controlled by 
      // the MaximumFrameLatency property. The default value is usually 3. If the application 
      // uses more SwapChainPresentationTargets we must increase this property.
      //var d3dDevice = (SharpDX.Direct3D11.Device)_graphicsManager.GraphicsDevice.Handle;
      //using (var dxgiDevice2 = d3dDevice.QueryInterface<SharpDX.DXGI.Device2>())
      //  dxgiDevice2.MaximumFrameLatency = numberOfSwapChainPanels;
    }


    public void Dispose()
    {
      if (_isDisposed)
        return;

      _isDisposed = true;
      
      CoreApplication.Suspending -= OnCoreApplicationSuspending;

      // Wait until game loop is finished.
      _gameLoopTask.Wait();

      _titleStorage.Dispose();
      _assetsStorage.Dispose();
      _digitalRuneStorage.Dispose();
      _vfsStorage.Dispose();
      _contentManager.Dispose();
      _serviceContainer.Dispose();
      _graphicsManager.Dispose();
      _dxgiOutput.Dispose();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // This method runs in a parallel task.
    private void GameLoopTaskAction(IAsyncAction action)
    {
      // Run until MyGame is disposed.
      while (!_isDisposed)
      {
        // Pulse the clock, which updates the timer. The timer will call GameLoop at the
        // desired frequency.
        lock (Lock)
          _clock.Update();

        // Halt the thread until the next vertical blank is reached.
        // This ensures the app isn't updating and rendering faster than the display can refresh,
        // which would unnecessarily spend extra CPU and GPU resources. This helps improve battery
        // life.
        _dxgiOutput.WaitForVerticalBlank();
      }
    }


    private void GameLoop(TimeSpan deltaTime)
    {
      // Update graphics service and graphics screens.
      _graphicsManager.Update(deltaTime);

      // If the game runs slowly, the timer will call this method several times. Only the last
      // call should actually draw to the screen. 
      // (E.g. if the desired frame rate is 60 Hz, then deltaTime is always 1/60s. If 3/60s have
      // passed since the last GameLoop call, this method is called 3 times with deltaTime = 1/60s.)
      var fixedStepTimer = _timer as FixedStepTimer;
      if (fixedStepTimer != null && fixedStepTimer.PendingSteps > 0)
        return;

      // Render the graphics screens into the presentation targets.
      foreach (var presentationTarget in _graphicsManager.PresentationTargets)
      {
        var myGamePresentationTarget = presentationTarget as MyGamePresentationTarget;
        if (myGamePresentationTarget != null)
          _graphicsManager.Render(presentationTarget, myGamePresentationTarget.GraphicsScreens);
      }
    }


    private void OnCoreApplicationSuspending(object sender, SuspendingEventArgs e)
    {
      lock (Lock)
      {
        // Call IDXGIDevice3::Trim. This hints to the driver that the app is entering an idle state
        // and that its memory can be used temporarily for other apps.
        var d3dDevice = (SharpDX.Direct3D11.Device)_graphicsManager.GraphicsDevice.Handle;
        using (var dxgiDevice3 = d3dDevice.QueryInterface<SharpDX.DXGI.Device3>())
          dxgiDevice3.Trim();
      }
    }
    #endregion
  }
}