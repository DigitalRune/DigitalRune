using System;
using NUnit.Framework;


namespace DigitalRune.Game.Timing.Tests
{
  [TestFixture]
  public class ManualClockTest
  {
    [Test]
    public void InitialValues()
    {
      var clock = new ManualClock();
      Assert.IsFalse(clock.IsRunning);
      Assert.AreEqual(TimeSpan.Zero, clock.DeltaTime);
      Assert.AreEqual(TimeSpan.Zero, clock.GameTime);
      Assert.AreEqual(TimeSpan.Zero, clock.TotalTime);
    }


    [Test]
    public void IsRunning()
    {
      var clock = new ManualClock();
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
      var clock = new ManualClock();
      clock.Start();
      clock.Update(new TimeSpan(100000));
      clock.Reset();

      Assert.IsFalse(clock.IsRunning);
      Assert.AreEqual(TimeSpan.Zero, clock.DeltaTime);
      Assert.AreEqual(TimeSpan.Zero, clock.GameTime);
      Assert.AreEqual(TimeSpan.Zero, clock.TotalTime);

      clock.Start();
      clock.Update(new TimeSpan(100000));
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
      IGameClock clock = new ManualClock();
      Assert.Throws<NotSupportedException>(clock.ResetDeltaTime);
    }


    [Test]
    public void StartStop()
    {
      var clock = new ManualClock();
      var step = new TimeSpan(100000);

      clock.Update(step);

      Assert.IsFalse(clock.IsRunning);
      Assert.AreEqual(TimeSpan.Zero, clock.DeltaTime);
      Assert.AreEqual(TimeSpan.Zero, clock.GameTime);
      Assert.AreEqual(TimeSpan.Zero, clock.TotalTime);

      clock.Start();
      
      Assert.IsTrue(clock.IsRunning);
      Assert.AreEqual(TimeSpan.Zero, clock.DeltaTime);
      Assert.AreEqual(TimeSpan.Zero, clock.GameTime);
      Assert.AreEqual(TimeSpan.Zero, clock.TotalTime);

      clock.Update(step);

      Assert.IsTrue(clock.IsRunning);
      Assert.AreEqual(step.Ticks, clock.DeltaTime.Ticks);
      Assert.AreEqual(1 * step.Ticks, clock.GameTime.Ticks);
      Assert.AreEqual(1 * step.Ticks, clock.TotalTime.Ticks);

      clock.Stop();
      clock.Update(step);

      Assert.IsFalse(clock.IsRunning);
      Assert.AreEqual(step.Ticks, clock.DeltaTime.Ticks);
      Assert.AreEqual(1 * step.Ticks, clock.GameTime.Ticks);
      Assert.AreEqual(1 * step.Ticks, clock.TotalTime.Ticks);

      clock.Start();
      clock.Update(step);
      clock.Update(step);
      clock.Update(new TimeSpan(step.Ticks * 2));

      Assert.IsTrue(clock.IsRunning);
      Assert.AreEqual(2 * step.Ticks, clock.DeltaTime.Ticks);
      Assert.AreEqual(5 * step.Ticks, clock.GameTime.Ticks);
      Assert.AreEqual(5 * step.Ticks, clock.TotalTime.Ticks);
    }


    [Test]
    public void TimeChanged()
    {
      var clock = new ManualClock();
      var step = new TimeSpan(100000);

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

      clock.Update(step);
      
      Assert.AreEqual(0, numberOfEvents);

      clock.Start();
      clock.Update(step);
      clock.Update(step);

      Assert.AreEqual(step.Ticks, deltaTime.Ticks);
      Assert.AreEqual(2 * step.Ticks, gameTime.Ticks);
      Assert.AreEqual(2 * step.Ticks, totalTime.Ticks);

      clock.Stop();
      clock.Update(step);
    }
  }
}
