using System;
using NUnit.Framework;


namespace DigitalRune.Game.States.Tests
{
  [TestFixture]
  public class TransitionTest
  {
    [Test]
    public void TestBasics()
    {
      Transition t = new Transition();
      t.SourceState = new State();

      // Not fired.
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);

      // Fired.
      t.Fire();
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), true);
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);

      // Fired.
      t.Fire(null, null);
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), true);
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);

      // FireAlways
      t.FireAlways = true;
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), true);

      // FireAlways with Guard
      t.Guard = () => false;
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);
      t.Guard = () => true;
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), true);

      // Fire with Guard
      t.FireAlways = false;
      t.Guard = () => false;
      t.Fire();
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);
      t.Guard = () => true;
      t.Fire();
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), true);
    }


    [Test]
    public void TestDelay()
    {
      Transition t = new Transition();
      StateMachine sm = new StateMachine();
      
      var s = new State();
      s.Transitions.Add(t);
      sm.States.Add(s);
      Assert.AreEqual(s, t.SourceState);

      t.Delay = TimeSpan.FromSeconds(-1);
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);

      t.Fire();
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), true);

      t.Delay = TimeSpan.FromSeconds(10);
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);
      t.Fire();
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);  // Fire is registered. deltaTime of this update does not count.
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);
      t.Fire();                                                   // A second fire is ignored - timer does not restart.
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(8)), true);   // 1 + 1 + 8 = 10 => Fire
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);

      // Test with other time steps.
      t.Delay = TimeSpan.FromSeconds(10);
      t.Fire();
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(7)), false);
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(7)), true);

      // Test again with a reset in the middle.
      t.Delay = TimeSpan.FromSeconds(10);
      t.Fire();
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(1)), false);
      t.SourceState.EnterState(null, new StateEventArgs());             // Reset. Transition should not fire anymore.
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(10)), false);
      Assert.AreEqual(t.Update(TimeSpan.FromSeconds(10)), false);
    }


    [Test]
    public void ChangeSourceStateOfTransition()
    {
      Transition t = new Transition();
      StateMachine sm = new StateMachine();

      var s0 = new State();
      var s1 = new State();
      sm.States.Add(s0);
      sm.States.Add(s1);

      s0.Transitions.Add(t);
      Assert.AreEqual(s0, t.SourceState);
      Assert.Contains(t, s0.Transitions);

      t.SourceState = s1;
      Assert.AreEqual(s1, t.SourceState);      
      Assert.Contains(t, s1.Transitions);
    }
  }
}
