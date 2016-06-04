using System;
using NUnit.Framework;


namespace DigitalRune.Game.States.Tests
{
  [TestFixture]
  public class StateMachineTest
  {
    private string _events;  // Events write into this string.

    [SetUp]
    public void SetUp()
    {
      _events = string.Empty;
    }


    [Test]
    public void EmptyStateMachine()
    {
      var sm = new StateMachine();

      sm.Update(TimeSpan.FromSeconds(1));
    }


    [Test]
    public void SimpleStateMachine()
    {
      var sm = new StateMachine();

      sm.States.Add(CreateState("0"));

      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0U0", _events);
    }


    [Test]
    public void SimpleStateMachine2()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      sm.States.Add(s0);

      var s1 = CreateState("1");
      sm.States.Add(s1);

      var t0 = new Transition
      {
        SourceState = s0, 
        TargetState = s1,         
      };
      t0.Action += (s, e) => _events = _events + "A0";

      var t1 = new Transition
      {
        SourceState = s1, 
        TargetState = s0, 
        FireAlways = true,         
      };
      t1.Action += (s, e) => _events = _events + "A1";

      // Start at 1.
      sm.States.InitialState = s1;

      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      t0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E1U1X1A1E0U0U0X0A0E1U1", _events);
    }


    [Test]
    public void DelayCycle()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      sm.States.Add(s0);

      var s1 = CreateState("1");
      sm.States.Add(s1);

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s1,
        FireAlways = true,
        Delay = TimeSpan.FromSeconds(3),        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      var t1 = new Transition
      {
        SourceState = s1,
        TargetState = s0,
        FireAlways = true,
        Delay = TimeSpan.FromSeconds(3),        
      };
      t1.Action += (s, e) => _events = _events + "A1";

      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0U0U0U0X0A0E1U1U1U1X1A1E0U0U0U0", _events);
    }


    [Test]
    public void TwoParallelTransitions()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      sm.States.Add(s0);

      var s1 = CreateState("1");
      sm.States.Add(s1);

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s1,
        FireAlways = true,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      var t1 = new Transition
      {
        SourceState = s0,
        TargetState = s1,
        FireAlways = true,        
      };
      t1.Action += (s, e) => _events = _events + "A1";

      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0U0X0A0E1U1", _events);
    }


    [Test]
    public void TwoParallelTransitionsFirstHasGuard()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      sm.States.Add(s0);

      var s1 = CreateState("1");
      sm.States.Add(s1);

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s1,
        FireAlways = true,
        Guard = () => false,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      var t1 = new Transition
      {
        SourceState = s0,
        TargetState = s1,
        FireAlways = true,                
      };
      t1.Action += (s, e) => _events = _events + "A1";

      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0U0X0A1E1U1", _events);
    }


    [Test]
    public void TwoParallelTransitionsManualFire()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      sm.States.Add(s0);

      var s1 = CreateState("1");
      sm.States.Add(s1);

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s1,
        Guard = () => false,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      var t1 = new Transition
      {
        SourceState = s0,
        TargetState = s1,        
      };
      t1.Action += (s, e) => _events = _events + "A1";

      sm.Update(TimeSpan.FromSeconds(1));
      t1.Fire();
      t0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0U0X0A1E1U1", _events);
    }


    [Test]
    public void CompositeStateTest()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      var s0States = new StateCollection();
      s0.ParallelSubStates.Add(s0States);
      sm.States.Add(s0);

      var s00 = CreateState("00");
      s0States.Add(s00);
      var s01 = CreateState("01");
      s0States.Add(s01);
      
      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s0,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      t0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0E00U00U0U00U0X00X0A0E0E00U00U0U00U0", _events);
    }


    [Test]
    public void CompositeStateTestWithInitialState()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      var s0States = new StateCollection();
      s0.ParallelSubStates.Add(s0States);
      sm.States.Add(s0);

      var s00 = CreateState("00");
      s0States.Add(s00);
      var s01 = CreateState("01");
      s0States.Add(s01);

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s0,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      s0States.InitialState = s01;

      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      t0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0E01U01U0U01U0X01X0A0E0E01U01U0U01U0", _events);
    }


    [Test]
    public void CompositeStateTestWithHistory()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      var s0States = new StateCollection();
      s0States.SaveHistory = true;
      s0.ParallelSubStates.Add(s0States);
      sm.States.Add(s0);

      var s00 = CreateState("00");
      s0States.Add(s00);
      var s01 = CreateState("01");
      s0States.Add(s01);

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s0,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      var t00 = new Transition
      {
        SourceState = s00,
        TargetState = s01,        
      };
      t00.Action += (s, e) => _events = _events + "A00";

      sm.Update(TimeSpan.FromSeconds(1));
      t00.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      t0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0E00U00U0X00A00E01U01U0X01X0A0E0E01U01U0U01U0", _events);
    }


    [Test]
    public void TwoCompositeStates()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      var s0States = new StateCollection();
      s0.ParallelSubStates.Add(s0States);
      sm.States.Add(s0);

      var s1 = CreateState("1");
      var s1States = new StateCollection();
      s1.ParallelSubStates.Add(s1States);
      sm.States.Add(s1);

      var s00 = CreateState("00");
      s0States.Add(s00);
      var s01 = CreateState("01");
      s0States.Add(s01);
      var s10 = CreateState("10");
      s1States.Add(s10);
      var s11 = CreateState("11");
      s1States.Add(s11);

      var t0001 = new Transition
      {
        SourceState = s00,
        TargetState = s01,        
      };
      t0001.Action += (s, e) => _events = _events + "A0001";

      var t0111 = new Transition
      {
        SourceState = s01,
        TargetState = s11,        
      };
      t0111.Action += (s, e) => _events = _events + "A0111";
      
      var t101 = new Transition
      {
        SourceState = s1,
        TargetState = s10,        
      };
      t101.Action += (s, e) => _events = _events + "A101";

      sm.Update(TimeSpan.FromSeconds(1));
      t0001.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      t0111.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      t101.Fire(null, null);
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0E00U00U0X00A0001E01U01U0X01X0A0111E1E11U11U1X11X1A101E1E10U10U1", _events);
    }


    [Test]
    public void CompositeState()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      var s0States = new StateCollection();
      s0.ParallelSubStates.Add(s0States);
      sm.States.Add(s0);

      var s00 = CreateState("00");
      s0States.Add(s00);

      var t = new Transition
      {
        SourceState = s00,
        TargetState = s0,        
      };
      t.Action += (s, e) => _events = _events + "A";

      sm.Update(TimeSpan.FromSeconds(1));
      t.Fire();
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0E00U00U0X00X0AE0E00U00U0", _events);
    }


    [Test]
    public void TestFireAlways()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      sm.States.Add(s0);

      var s1 = CreateState("1");
      sm.States.Add(s1);

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s1,
        FireAlways = true,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      var t1 = new Transition
      {
        SourceState = s1,
        TargetState = s0,
        FireAlways = true,        
      };
      t1.Action += (s, e) => _events = _events + "A1";

      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0U0X0A0E1U1X1A1E0U0", _events);
    }


    [Test]
    public void IgnoredHistory()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      sm.States.Add(s0);

      var s1 = CreateState("1");
      var s1States = new StateCollection();
      s1States.SaveHistory = true;
      s1.ParallelSubStates.Add(s1States);
      sm.States.Add(s1);

      var s10 = CreateState("10");
      s1States.Add(s10);
      var s11 = CreateState("11");
      s1States.Add(s11);
      var s12 = CreateState("12");
      s1States.Add(s12);

      s1States.InitialState = s11;

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s12,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      var t1211 = new Transition
      {
        SourceState = s12,
        TargetState = s11,        
      };
      t1211.Action += (s, e) => _events = _events + "A1211";

      var t1 = new Transition
      {
        SourceState = s1,
        TargetState = s0,        
      };
      t1.Action += (s, e) => _events = _events + "A1";

      var t010 = new Transition
      {
        SourceState = s0,
        TargetState = s10,        
      };
      t010.Action += (s, e) => _events = _events + "A010";

      sm.Update(TimeSpan.FromSeconds(1));
      t0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      t1211.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      t1.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      t010.Fire();
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0U0X0A0E1E12U12U1X12A1211E11U11U1X11X1A1E0U0X0A010E1E10U10U1", _events);
    }


    [Test]
    public void TestNoFinalState()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      var s0States = new StateCollection();
      s0.ParallelSubStates.Add(s0States);
      sm.States.Add(s0);

      var s1 = CreateState("1");
      sm.States.Add(s1);

      var s00 = CreateState("00");
      s0States.Add(s00);
      var s01 = CreateState("01");
      s0States.Add(s01);
      var s02 = CreateState("02");
      s0States.Add(s02);

      Assert.AreEqual(null, s0States.FinalState);

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s1,
        FireAlways = true,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0E00U00U0X00X0A0E1U1", _events);
    }


    [Test]
    public void TestWithFinalState()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      var s0States = new StateCollection();
      s0.ParallelSubStates.Add(s0States);
      sm.States.Add(s0);

      var s1 = CreateState("1");
      sm.States.Add(s1);

      var s00 = CreateState("00");
      s0States.Add(s00);
      var s01 = CreateState("01");
      s0States.Add(s01);
      var s02 = CreateState("02");
      s0States.Add(s02);

      // Set final state
      s0States.FinalState = s02;

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s1,
        FireAlways = true,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      var t0001 = new Transition
      {
        SourceState = s00,
        TargetState = s01,        
      };
      t0001.Action += (s, e) => _events = _events + "A0001";
      
      var t0102 = new Transition
      {
        SourceState = s01,
        TargetState = s02,        
      };
      t0102.Action += (s, e) => _events = _events + "A0102";

      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      t0001.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));
      t0102.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0E00U00U0U00U0X00A0001E01U01U0U01U0X01A0102E02U02U0X02X0A0E1U1", _events);
    }


    [Test]
    public void TestParallelStates()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      sm.States.Add(s0);

      var s0A = new StateCollection();
      s0.ParallelSubStates.Add(s0A);
      var s0B = new StateCollection();
      s0.ParallelSubStates.Add(s0B);
      var s0C = new StateCollection();
      s0.ParallelSubStates.Add(s0C);
      
      var s1 = CreateState("1");
      sm.States.Add(s1);

      var sa0 = CreateState("a0");
      s0A.Add(sa0);
      var sa1 = CreateState("a1");
      s0A.Add(sa1);

      var sb0 = CreateState("b0");
      s0B.Add(sb0);
      var sb1 = CreateState("b1");
      s0B.Add(sb1);
      var sb2 = CreateState("b2");
      s0B.Add(sb2);

      var sc0 = CreateState("c0");
      s0C.Add(sc0);
      var sc1 = CreateState("c1");
      s0C.Add(sc1);

      s0C.InitialState = sc1;

      // Set final state
      s0A.FinalState = sa1;
      s0B.FinalState = sb2;
      s0C.FinalState = null;

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s1,
        FireAlways = true,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      var ta0a1 = new Transition
      {
        SourceState = sa0,
        TargetState = sa1,
        FireAlways = true,        
      };
      ta0a1.Action += (s, e) => _events = _events + "Aa0a1";

      var tb0b1 = new Transition
      {
        SourceState = sb0,
        TargetState = sb1,
        FireAlways = true,        
      };
      tb0b1.Action += (s, e) => _events = _events + "Ab0b1";

      var tb1b2 = new Transition
      {
        SourceState = sb1,
        TargetState = sb2,
        FireAlways = true,        
      };
      tb1b2.Action += (s, e) => _events = _events + "Ab1b2";

      var tc0c1 = new Transition
      {
        SourceState = sc0,
        TargetState = sc1,
        FireAlways = true,        
      };
      tc0c1.Action += (s, e) => _events = _events + "Ac0c1";

      var tc1c0 = new Transition
      {
        SourceState = sc1,
        TargetState = sc0,        
      };
      tc1c0.Action += (s, e) => _events = _events + "Ac1c0";

      tc1c0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      tc1c0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      tc1c0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0Ea0Eb0Ec1Ua0Ub0Uc1U0Xa0Aa0a1Ea1Xb0Ab0b1Eb1Xc1Ac1c0Ec0Ua1Ub1Uc0U0Xb1Ab1b2Eb2Xc0Ac0c1Ec1Ua1Ub2Uc1U0Xa1Xb2Xc1X0A0E1U1", _events);
    }


    [Test]
    public void TestParallelStates2()
    {
      var sm = new StateMachine();

      var s0 = CreateState("0");
      sm.States.Add(s0);

      var s0A = new StateCollection();
      s0.ParallelSubStates.Add(s0A);
      var s0B = new StateCollection();
      s0.ParallelSubStates.Add(s0B);
      var s0C = new StateCollection();
      s0.ParallelSubStates.Add(s0C);

      var s1 = CreateState("1");
      sm.States.Add(s1);

      var sa0 = CreateState("a0");
      s0A.Add(sa0);
      var sa1 = CreateState("a1");
      s0A.Add(sa1);

      var sb0 = CreateState("b0");
      s0B.Add(sb0);
      var sb1 = CreateState("b1");
      s0B.Add(sb1);
      var sb2 = CreateState("b2");
      s0B.Add(sb2);

      var sc0 = CreateState("c0");
      s0C.Add(sc0);
      var sc1 = CreateState("c1");
      s0C.Add(sc1);

      s0C.InitialState = sc1;

      // Set final state
      s0A.FinalState = sa1;
      s0B.FinalState = sb2;
      s0C.FinalState = null;

      var t0 = new Transition
      {
        SourceState = s0,
        TargetState = s1,
        FireAlways = true,        
      };
      t0.Action += (s, e) => _events = _events + "A0";

      var ta0a1 = new Transition
      {
        SourceState = sa0,
        TargetState = sa1,
        FireAlways = true,        
      };
      ta0a1.Action += (s, e) => _events = _events + "Aa0a1";

      var tb0b1 = new Transition
      {
        SourceState = sb0,
        TargetState = sb1,
        FireAlways = true,        
      };
      tb0b1.Action += (s, e) => _events = _events + "Ab0b1";

      var tb1b2 = new Transition
      {
        SourceState = sb1,
        TargetState = sb2,
        FireAlways = true,        
      };
      tb1b2.Action += (s, e) => _events = _events + "Ab1b2";

      var tc0c1 = new Transition
      {
        SourceState = sc0,
        TargetState = sc1,
        FireAlways = true,        
      };
      tc0c1.Action += (s, e) => _events = _events + "Ac0c1";

      var tc1c0 = new Transition
      {
        SourceState = sc1,
        TargetState = sc0,        
      };
      tc1c0.Action += (s, e) => _events = _events + "Ac1c0";

      tc1c0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      tc1c0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      tc1c0.Fire();
      sm.Update(TimeSpan.FromSeconds(1));
      tc1c0.Fire(); // tc1c0 fires --> t0 must not fire.
      sm.Update(TimeSpan.FromSeconds(1));

      Assert.AreEqual("E0Ea0Eb0Ec1Ua0Ub0Uc1U0Xa0Aa0a1Ea1Xb0Ab0b1Eb1Xc1Ac1c0Ec0Ua1Ub1Uc0U0Xb1Ab1b2Eb2Xc0Ac0c1Ec1Ua1Ub2Uc1U0Xc1Ac1c0Ec0Ua1Ub2Uc0U0", _events);
    }


    //--------------------------------------------------------------
    #region Helper Methods & Event Handlers
    //--------------------------------------------------------------

    private State CreateState(string name)
    {
      var state = new State
      {
        Name = name,
      };
      state.Enter += OnEnter;
      state.Update += OnUpdate;
      state.Exit += OnExited;
      return state;
    }


    private void OnEnter(object sender, EventArgs eventArgs)
    {
      _events = _events + "E" + ((State)sender).Name;
    }


    private void OnUpdate(object sender, EventArgs eventArgs)
    {
      _events = _events + "U" + ((State)sender).Name;
    }


    private void OnExited(object sender, EventArgs eventArgs)
    {
      _events = _events + "X" + ((State)sender).Name;
    }
    #endregion
  }
}
