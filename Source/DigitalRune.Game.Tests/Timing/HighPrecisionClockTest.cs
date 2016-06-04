using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;


namespace DigitalRune.Game.Timing.Tests
{
// These tests do not run on the CruiseControl server - Thread.Sleep is to inaccurate to be useful :-(
  [TestFixture]
  public class HighPrecisionClockTest
  {
    [Test]
    public void InitialValues()
    {
      var clock = new HighPrecisionClock();
      Assert.IsFalse(clock.IsRunning);
      Assert.AreEqual(TimeSpan.Zero, clock.DeltaTime);
      Assert.AreEqual(TimeSpan.Zero, clock.GameTime);
      Assert.AreEqual(TimeSpan.Zero, clock.TotalTime);
    }


    [Test]
    public void IsRunning()
    {
      var clock = new HighPrecisionClock();
      Assert.IsFalse(clock.IsRunning);
      clock.Start();
      Assert.IsTrue(clock.IsRunning);
      clock.Start();
      Assert.IsTrue(clock.IsRunning);
      clock.Stop();
      Assert.IsFalse(clock.IsRunning);
      clock.Start();
      Assert.IsTrue(clock.IsRunning);
      clock.Stop();
      clock.Stop();
      Assert.IsFalse(clock.IsRunning);
    }


    [Test]
    public void Reset()
    {
      var clock = new HighPrecisionClock();
      clock.Start();
      clock.Update();
      Thread.Sleep(2);
      clock.Reset();

      Assert.IsFalse(clock.IsRunning);
      Assert.AreEqual(TimeSpan.Zero, clock.DeltaTime);
      Assert.AreEqual(TimeSpan.Zero, clock.GameTime);
      Assert.AreEqual(TimeSpan.Zero, clock.TotalTime);

      clock.Start();
      Thread.Sleep(2);
      clock.Update();
      clock.Stop();
      clock.Reset();

      Assert.IsFalse(clock.IsRunning);
      Assert.AreEqual(TimeSpan.Zero, clock.DeltaTime);
      Assert.AreEqual(TimeSpan.Zero, clock.GameTime);
      Assert.AreEqual(TimeSpan.Zero, clock.TotalTime);
    }


    [Test]
    public void ResetDeltaTime()
    {
      var clock = new HighPrecisionClock();
      
      clock.Start();
      Wait(new TimeSpan(100000));
      clock.Update();
      Assert.IsTrue(clock.DeltaTime.Ticks >= 100000);
      Assert.AreEqual(clock.GameTime, clock.TotalTime);
      clock.ResetDeltaTime();

      // DeltaTime is only changed at TimeChanged events.
      Assert.IsTrue(clock.DeltaTime.Ticks >= 100000);
      Assert.AreEqual(clock.GameTime, clock.TotalTime);

      clock.Update();
      Assert.IsTrue(clock.DeltaTime.Ticks < 100000);
      Assert.IsTrue(clock.GameTime < clock.TotalTime);

    }


    [Test]
    public void StartStop()
    {
      const long ticks = 100000;
      TimeSpan timeSpan = new TimeSpan(ticks);

      var clock = new HighPrecisionClock();

      Wait(timeSpan);
      clock.Update();

      Assert.IsFalse(clock.IsRunning);
      Assert.AreEqual(TimeSpan.Zero, clock.DeltaTime);
      Assert.AreEqual(TimeSpan.Zero, clock.GameTime);
      Assert.AreEqual(TimeSpan.Zero, clock.TotalTime);

      clock.Start();

      Stopwatch w = Stopwatch.StartNew();
      Wait(timeSpan);
      w.Stop();

      Assert.IsTrue(clock.IsRunning);
      Assert.AreEqual(TimeSpan.Zero, clock.DeltaTime);
      Assert.AreEqual(TimeSpan.Zero, clock.GameTime);
      Assert.AreEqual(TimeSpan.Zero, clock.TotalTime);

      clock.Update();

      Assert.IsTrue(clock.IsRunning);      
      Assert.IsTrue(clock.DeltaTime.Ticks > ticks && clock.DeltaTime.Ticks < 2 * ticks);
      Assert.IsTrue(clock.GameTime.Ticks > ticks && clock.GameTime.Ticks < 2 * ticks);
      Assert.IsTrue(clock.TotalTime.Ticks > ticks && clock.TotalTime.Ticks < 2 * ticks);

      clock.Stop();

      Wait(timeSpan);
      clock.Update();

      Assert.IsFalse(clock.IsRunning);
      Assert.IsTrue(clock.DeltaTime.Ticks > ticks && clock.DeltaTime.Ticks < 2 * ticks);
      Assert.IsTrue(clock.GameTime.Ticks > ticks && clock.GameTime.Ticks < 2 * ticks);
      Assert.IsTrue(clock.TotalTime.Ticks > ticks && clock.TotalTime.Ticks < 2 * ticks);

      clock.Start();
      Wait(timeSpan);
      clock.Update();
      Wait(timeSpan);
      Wait(timeSpan);
      clock.Update();

      Assert.IsTrue(clock.IsRunning);
      Assert.IsTrue(clock.DeltaTime.Ticks > 2 * ticks && clock.DeltaTime.Ticks < 3 * ticks);
      Assert.IsTrue(clock.GameTime.Ticks > 4 * ticks && clock.GameTime.Ticks < 5 * ticks);
      Assert.IsTrue(clock.TotalTime.Ticks > 4 * ticks && clock.TotalTime.Ticks < 5 * ticks);
    }


    [Test]
    public void TimeChanged()
    {
      const long ticks = 100000;
      TimeSpan timeSpan = new TimeSpan(ticks);

      var clock = new HighPrecisionClock();

      int numberOfEvents = 0;
      TimeSpan deltaTime = TimeSpan.Zero;
      TimeSpan gameTime = TimeSpan.Zero;
      TimeSpan totalTime = TimeSpan.Zero;

      clock.TimeChanged += (s, e) =>
      {
        numberOfEvents++;
        deltaTime = e.DeltaTime;
        gameTime = e.GameTime;
        totalTime = e.TotalTime;
      };

      Wait(timeSpan);
      clock.Update();

      Assert.AreEqual(0, numberOfEvents);

      clock.Start();
      Wait(timeSpan);
      clock.Update();
      Wait(timeSpan);
      clock.Update();

      Assert.IsTrue(clock.DeltaTime.Ticks > 1 * ticks && clock.DeltaTime.Ticks < 2 * ticks);
      Assert.IsTrue(clock.GameTime.Ticks > 2 * ticks && clock.GameTime.Ticks < 3 * ticks);
      Assert.IsTrue(clock.TotalTime.Ticks > 2 * ticks && clock.TotalTime.Ticks < 3 * ticks);

      clock.Stop();
      clock.Update();
    }
    

    private readonly Stopwatch _watch = new Stopwatch();
    private void Wait(TimeSpan time)
    {
      _watch.Start();
      while (_watch.Elapsed < time)
      {        
      }
      _watch.Reset();
    }

  }
}
