///*

//using System;
//using NUnit.Framework;

//namespace DigitalRune.Game.Timing.Tests
//{
//  [TestFixture]
//  public class TickCountClockTest
//  {
//    [Test]
//    public void NormalRun()
//    {
//      long ticksBeforeStart = Environment.TickCount;
//      GameClock clock = new TickCountClock();
//      Assert.IsFalse(clock.IsRunning);

//      clock.Start();
//      Assert.IsTrue(clock.IsRunning);
      
//      long ticksAfterStart = Environment.TickCount;

//      Assert.AreEqual(TimeSpan.Zero, clock.Time);
//      Assert.AreEqual(TimeSpan.Zero, clock.DeltaTime);

//      System.Threading.Thread.Sleep(20);

//      long ticksBeforeAdvance = Environment.TickCount;
//      clock.Update();
//      long ticksAfterAdvance = Environment.TickCount;

//      Assert.AreEqual(clock.DeltaTime, clock.Time);
//      TimeSpan maxDeltaTime = TimeSpan.FromMilliseconds(ticksAfterAdvance - ticksBeforeStart);
//      TimeSpan minDeltaTime = TimeSpan.FromMilliseconds(ticksBeforeAdvance - ticksAfterStart);
//      Assert.GreaterOrEqual(maxDeltaTime, clock.DeltaTime);
//      Assert.LessOrEqual(minDeltaTime, clock.DeltaTime);

//      System.Threading.Thread.Sleep(20);

//      clock.Update();

//      Assert.Greater(clock.DeltaTime, TimeSpan.FromMilliseconds(10));
//      Assert.Less(clock.DeltaTime, TimeSpan.FromMilliseconds(40));
//      Assert.Greater(clock.Time, clock.DeltaTime);
//    }

//    [Test]
//    public void TestStartStop()
//    {
//      GameClock clock = new TickCountClock();
//      clock.Start();

//      Assert.AreEqual(TimeSpan.Zero, clock.DeltaTime);
//      Assert.IsTrue(clock.IsRunning);

//      System.Threading.Thread.Sleep(50);

//      clock.Update();
//      Assert.IsTrue(TimeSpan.FromMilliseconds(30) <= clock.DeltaTime && clock.DeltaTime <= TimeSpan.FromMilliseconds(70));

//      clock.Stop();
//      Assert.IsFalse(clock.IsRunning);

//      System.Threading.Thread.Sleep(50);

//      clock.Start();
//      Assert.IsTrue(clock.IsRunning);

//      System.Threading.Thread.Sleep(50);

//      clock.Update();
//      Assert.IsTrue(TimeSpan.FromMilliseconds(30) <= clock.DeltaTime && clock.DeltaTime <= TimeSpan.FromMilliseconds(70));
//    }
//  }
//}

//*/