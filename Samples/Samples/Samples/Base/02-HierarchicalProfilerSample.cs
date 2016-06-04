// The conditional compilation symbol "DIGITALRUNE_PROFILE" must be defined to 
// activate profiling.
#define DIGITALRUNE_PROFILE

using System;
using DigitalRune.Diagnostics;


namespace Samples.Base
{
  [Sample(SampleCategory.Base,
    @"This samples shows how to use the HierarchicalProfiler class to measure time in
a single-threaded application.",
    @"",
    2)]
  public class HierarchicalProfilerSample : BasicSample
  {
    // The profiler instance.
    private static readonly HierarchicalProfiler _profiler = new HierarchicalProfiler("MyProfiler");


    public HierarchicalProfilerSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.UseFixedWidthFont = true;

      // Start profiling.
      _profiler.Reset();

      // This loop simulates the main-loop of a game.
      for (int i = 0; i < 20; i++)
      {
        // NewFrame() must be called when a new frame begins (= start of game loop).
        _profiler.NewFrame();

        Update();
        Draw();
      }

      // Print the profiler data. We start at the root node and include up to 5 child levels.
      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      debugRenderer.DrawText("\n");
      debugRenderer.DrawText(_profiler.Dump(_profiler.Root, 5));
    }


    private static void Update()
    {
      _profiler.Start("Update");

      Physics();
      AI();
      AI();
      AI();

      // Simulate other work...
      Sleep(1);

      _profiler.Stop();
    }


    private static void Physics()
    {
      _profiler.Start("Physics");

      // Simulate work...
      Sleep(6);

      _profiler.Stop();
    }


    private static void AI()
    {
      _profiler.Start("AI");

      // Simulate work...
      Sleep(3);

      _profiler.Stop();
    }


    private static void Draw()
    {
      _profiler.Start("Draw");

      // Simulate work...
      Sleep(4);

      _profiler.Stop();
    }


    private static void Sleep(float timeInMilliseconds)
    {
#if NETFX_CORE
      System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(timeInMilliseconds)).Wait();
#else
      System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(timeInMilliseconds));
#endif
    }
  }
}
