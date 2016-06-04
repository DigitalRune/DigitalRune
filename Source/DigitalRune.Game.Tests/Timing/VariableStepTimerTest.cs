using System;
using NUnit.Framework;


namespace DigitalRune.Game.Timing.Tests
{
  [TestFixture]
  public class VariableStepTimerTest
  {
    bool idleEventOccured;
    bool timeChangedEventOccured;
    GameTimerEventArgs idleEventArgs;
    GameTimerEventArgs timeEventArgs;

    [SetUp]
    public void Setup()
    {
      idleEventOccured = false;
      timeChangedEventOccured = false;
      idleEventArgs = null;
      timeEventArgs = null;
    }

    [Test]
    public void NormalRun()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      VariableStepTimer timer = new VariableStepTimer(clock);
      timer.Idle += timer_Idle;
      timer.TimeChanged += timer_TimeChanged;

      // Clock is not running
      clock.Update(TimeSpan.FromMilliseconds(10));
      Assert.IsFalse(timer.IsRunning);
      CheckNoIdleEvent();
      CheckNoTimeChangedEvent();

      // Start/Stop ... not running
      timer.Start();
      timer.Stop();
      clock.Update(TimeSpan.FromMilliseconds(10));
      Assert.IsFalse(timer.IsRunning);
      CheckNoIdleEvent();
      CheckNoTimeChangedEvent();

      // Start
      timer.Start();
      clock.Update(TimeSpan.FromMilliseconds(10));
      Assert.IsTrue(timer.IsRunning);
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(10));
      Assert.AreEqual(TimeSpan.FromMilliseconds(20), timer.Time);
      Assert.AreEqual(TimeSpan.FromMilliseconds(10), timer.DeltaTime);
      Assert.AreEqual(TimeSpan.Zero, timer.IdleTime);
      Assert.AreEqual(TimeSpan.Zero, timer.LostTime);

      // Pause
      timer.Stop();
      clock.Update(TimeSpan.FromMilliseconds(10));
      Assert.IsFalse(timer.IsRunning);
      CheckNoIdleEvent();
      CheckNoTimeChangedEvent();
      Assert.AreEqual(TimeSpan.FromMilliseconds(20), timer.Time);

      // Resume
      timer.Start();
      clock.Update(TimeSpan.FromMilliseconds(10));
      Assert.IsTrue(timer.IsRunning);
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(30), TimeSpan.FromMilliseconds(10));
      Assert.AreEqual(TimeSpan.FromMilliseconds(30), timer.Time);
      Assert.AreEqual(TimeSpan.FromMilliseconds(10), timer.DeltaTime);
      Assert.AreEqual(TimeSpan.Zero, timer.IdleTime);
      Assert.AreEqual(TimeSpan.Zero, timer.LostTime);

      // Stop
      timer.Stop();
      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckNoTimeChangedEvent();
    }


    [Test]
    public void TimerReset()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      VariableStepTimer timer = new VariableStepTimer(clock);
      timer.Idle += timer_Idle;
      timer.TimeChanged += timer_TimeChanged;
      timer.Reset();
      timer.Start();

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));

      timer.Reset();
      Assert.IsFalse(timer.IsRunning);
      Assert.AreEqual(TimeSpan.Zero, timer.Time);
      Assert.AreEqual(TimeSpan.Zero, timer.DeltaTime);

      timer.Start();
      clock.Update(TimeSpan.FromMilliseconds(10));
      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(10));

      timer.Stop();
      Assert.AreEqual(TimeSpan.FromMilliseconds(20), timer.Time);
      Assert.AreEqual(TimeSpan.Zero, timer.DeltaTime);

      timer.Reset();
      Assert.AreEqual(TimeSpan.Zero, timer.Time);
      Assert.AreEqual(TimeSpan.Zero, timer.DeltaTime);
    }


    [Test]
    public void SwitchClocks()
    {
      ManualClock clock1 = new ManualClock();
      ManualClock clock2 = new ManualClock();
      clock1.Start();
      clock2.Start();

      IGameTimer timer = new VariableStepTimer(clock1);
      timer.TimeChanged += timer_TimeChanged;
      timer.Start();

      clock1.Update(TimeSpan.FromMilliseconds(10));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));

      timer.Clock = clock2;
      Assert.AreSame(clock2, timer.Clock);
      clock1.Update(TimeSpan.FromMilliseconds(10));
      CheckNoTimeChangedEvent();

      clock2.Update(TimeSpan.FromMilliseconds(20));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(30), TimeSpan.FromMilliseconds(20));

      timer.Clock = null;
      Assert.IsNull(timer.Clock);
      clock1.Update(TimeSpan.FromMilliseconds(10));
      clock2.Update(TimeSpan.FromMilliseconds(20));
      CheckNoTimeChangedEvent();
      Assert.AreEqual(TimeSpan.FromMilliseconds(30), timer.Time);
      Assert.AreEqual(TimeSpan.FromMilliseconds(20), timer.DeltaTime);
    }


    [Test]
    public void IdleTime()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      VariableStepTimer timer = new VariableStepTimer(clock);
      timer.Idle += timer_Idle;
      timer.TimeChanged += timer_TimeChanged;
      timer.MinDeltaTime = TimeSpan.FromMilliseconds(20);
      timer.Start();

      clock.Update(TimeSpan.FromMilliseconds(20));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(20));
      Assert.AreEqual(TimeSpan.Zero, timer.IdleTime);

      clock.Update(TimeSpan.FromMilliseconds(20));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(20));
      Assert.AreEqual(TimeSpan.Zero, timer.IdleTime);

      clock.Update(TimeSpan.FromMilliseconds(15));
      CheckIdleEvent(TimeSpan.FromMilliseconds(20 - 15));
      CheckNoTimeChangedEvent();
      Assert.AreEqual(TimeSpan.FromMilliseconds(20 - 15), timer.IdleTime);

      clock.Update(TimeSpan.FromMilliseconds(2));
      CheckIdleEvent(TimeSpan.FromMilliseconds(20 - (15 + 2)));
      CheckNoTimeChangedEvent();
      Assert.AreEqual(TimeSpan.FromMilliseconds(20 - (15 + 2)), timer.IdleTime);
    }


    [Test]
    public void LostTime()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      VariableStepTimer timer = new VariableStepTimer(clock);
      timer.TimeChanged += timer_TimeChanged;
      timer.MaxDeltaTime = TimeSpan.FromMilliseconds(50);
      timer.Start();

      clock.Update(TimeSpan.FromMilliseconds(20));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(20));
      Assert.AreEqual(TimeSpan.Zero, timer.LostTime);

      clock.Update(TimeSpan.FromMilliseconds(20));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(20));
      Assert.AreEqual(TimeSpan.Zero, timer.LostTime);

      clock.Update(TimeSpan.FromMilliseconds(50));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(90), TimeSpan.FromMilliseconds(50));
      Assert.AreEqual(TimeSpan.Zero, timer.LostTime);

      clock.Update(TimeSpan.FromMilliseconds(70));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(140), TimeSpan.FromMilliseconds(50));
      Assert.AreEqual(TimeSpan.FromMilliseconds(70 - 50), timer.LostTime);

      clock.Update(TimeSpan.FromMilliseconds(80));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(190), TimeSpan.FromMilliseconds(50));
      Assert.AreEqual(TimeSpan.FromMilliseconds(80 - 50), timer.LostTime);

      clock.Update(TimeSpan.FromMilliseconds(20));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(210), TimeSpan.FromMilliseconds(20));
      Assert.AreEqual(TimeSpan.Zero, timer.LostTime);
    }


    [Test]
    public void Scale()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      VariableStepTimer timer = new VariableStepTimer(clock);
      timer.TimeChanged += timer_TimeChanged;
      timer.Start();
      Assert.AreEqual(1.0, timer.Speed);

      clock.Update(TimeSpan.FromMilliseconds(20));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(20));

      clock.Update(TimeSpan.FromMilliseconds(20));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(20));

      timer.Speed = 0.5;
      Assert.AreEqual(0.5, timer.Speed);

      clock.Update(TimeSpan.FromMilliseconds(20));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(10));

      clock.Update(TimeSpan.FromMilliseconds(20));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(60), TimeSpan.FromMilliseconds(10));

      timer.Speed = 2.0;
      Assert.AreEqual(2.0, timer.Speed);

      clock.Update(TimeSpan.FromMilliseconds(20));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(40));

      timer.Speed = -3.0;
      Assert.AreEqual(-3.0, timer.Speed);

      clock.Update(TimeSpan.FromMilliseconds(20));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(-60));
      Assert.AreEqual(TimeSpan.FromMilliseconds(40), timer.Time);
      Assert.AreEqual(TimeSpan.FromMilliseconds(-60), timer.DeltaTime);
    }


    [Test]
    public void NegativeScale()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      VariableStepTimer timer = new VariableStepTimer(clock);
      timer.Idle += timer_Idle;
      timer.TimeChanged += timer_TimeChanged;
      timer.Speed = -2.0;
      timer.Start();
      Assert.AreEqual(-2.0, timer.Speed);

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(-20), TimeSpan.FromMilliseconds(-20));

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(-40), TimeSpan.FromMilliseconds(-20));

      timer.MinDeltaTime = TimeSpan.FromMilliseconds(20);
      timer.MaxDeltaTime = TimeSpan.FromMilliseconds(50);

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromSeconds(-0.06f), TimeSpan.FromMilliseconds(-20));

      clock.Update(TimeSpan.FromMilliseconds(9));
      CheckIdleEvent(TimeSpan.FromMilliseconds(20 - 18));
      CheckNoTimeChangedEvent();

      clock.Update(TimeSpan.FromTicks(5000)); // 0.5 ms
      CheckIdleEvent(TimeSpan.FromMilliseconds(20 - 19));
      CheckNoTimeChangedEvent();

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(-60 - 39), TimeSpan.FromMilliseconds(-39));

      clock.Update(TimeSpan.FromMilliseconds(30));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(-60 - 39 - 50), TimeSpan.FromMilliseconds(-50));
      Assert.AreEqual(TimeSpan.FromMilliseconds(10), timer.LostTime);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void InvalidMinDeltaTime()
    {
      VariableStepTimer timer = new VariableStepTimer(null);
      timer.MinDeltaTime = TimeSpan.FromMilliseconds(-10);
    }

    
    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void InvalidMaxDeltaTime()
    {
      VariableStepTimer timer = new VariableStepTimer(null);
      timer.MaxDeltaTime = TimeSpan.Zero;
    }
    

    //--------------------------------------------------------------
    #region Helpers for Events
    //--------------------------------------------------------------

    void timer_Idle(object sender, GameTimerEventArgs eventArgs)
    {
      idleEventOccured = true;
      idleEventArgs = eventArgs;
    }

    void timer_TimeChanged(object sender, GameTimerEventArgs eventArgs)
    {
      timeChangedEventOccured = true;
      timeEventArgs = eventArgs;
    }

    void CheckNoIdleEvent()
    {
      Assert.IsFalse(idleEventOccured);
    }

    void CheckNoTimeChangedEvent()
    {
      Assert.IsFalse(timeChangedEventOccured);
    }

    void CheckIdleEvent(TimeSpan expectedIdleTime)
    {
      Assert.IsTrue(idleEventOccured);
      Assert.AreEqual(expectedIdleTime, idleEventArgs.IdleTime);
      idleEventOccured = false;
    }

    void CheckTimeChangedEvent(TimeSpan expectedTime, TimeSpan expectedDeltaTime)
    {
      Assert.IsTrue(timeChangedEventOccured);
      Assert.AreEqual(expectedTime, timeEventArgs.Time);
      Assert.AreEqual(expectedDeltaTime, timeEventArgs.DeltaTime);
      timeChangedEventOccured = false;
    }
    #endregion
  }
}
