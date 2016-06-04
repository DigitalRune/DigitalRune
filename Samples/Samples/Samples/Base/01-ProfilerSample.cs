// The conditional compilation symbol "DIGITALRUNE_PROFILE" must be defined to 
// activate profiling.
#define DIGITALRUNE_PROFILE

using System;
using DigitalRune.Diagnostics;
using DigitalRune.Threading;


namespace Samples.Base
{
  [Sample(SampleCategory.Base,
    @"This samples shows how to use the Profiler class to measure time and collect other
statistics in a multi-threaded application.",
    @"",
    1)]
  public class ProfilerSample : BasicSample
  {
    public ProfilerSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.UseFixedWidthFont = true;

      Profiler.ClearAll();

      // Warmstart: We call Foo and the Parallel class so that all one-time initializations 
      // are done before we start measuring.
      Parallel.For(0, 100, i => Foo());

      // Measure time of a sequential for-loop.
      Profiler.Start("MainSequential");
      for (int i = 0; i < 100; i++)
        Foo();
      Profiler.Stop("MainSequential");

      // Measure time of a parallel for-loop.
      Profiler.Start("MainParallel");
      Parallel.For(0, 100, i => Foo());
      Profiler.Stop("MainParallel");

      // Format the output by defining a useful scale. We add descriptions, so that 
      // any other person looking at the output can interpret them more easily.
      Profiler.SetFormat("MainSequential", 1e3f, "[ms]");
      Profiler.SetFormat("MainParallel", 1e3f, "[ms]");
      Profiler.SetFormat("Foo", 1e6f, "[us]");  // Use "us" instead of "µs" because 'µ' is usually not in SpriteFont.
      Profiler.SetFormat("ValuesBelow10", 1.0f / 100.0f, "[%]");

      // Print the profiling results.
      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      debugRenderer.DrawText("\n");
      debugRenderer.DrawText(Profiler.DumpAll());

      Profiler.ClearAll();
    }


    public static void Foo()
    {
      Profiler.Start("Foo");

      // Generate a few random numbers.
      var random = new Random();
      int numberOfValuesBelow10 = 0;
      for (int i = 0; i < 10000; i++)
      {
        int x = random.Next(0, 100);
        if (x < 10)
          numberOfValuesBelow10++;
      }

      // Profilers can also collect other interesting numbers (not only time).
      Profiler.AddValue("ValuesBelow10", numberOfValuesBelow10);

      Profiler.Stop("Foo");
    }
  }
}
