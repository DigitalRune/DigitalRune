using System;
using System.Linq;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles.Effectors;
using NUnit.Framework;


namespace DigitalRune.Particles.Tests.Effectors
{
  [TestFixture]
  public class AccelerationEffectorTest
  {
    [Test]
    public void DefaultValues()
    {
      var e = new LinearAccelerationEffector();
      Assert.AreEqual(ParticleParameterNames.Direction, e.DirectionParameter);
      Assert.AreEqual(ParticleParameterNames.LinearSpeed, e.SpeedParameter);
      Assert.AreEqual(ParticleParameterNames.LinearAcceleration, e.AccelerationParameter);
    }


    //[Test]
    //public void InputOutputParameters()
    //{
    //  var e = new LinearAccelerationEffector()
    //  {
    //    DirectionParameter = "D",
    //    SpeedParameter = "S",
    //    AccelerationParameter = "A",
    //  };

    //  Assert.IsTrue(e.InputParameters.Contains("D"));
    //  Assert.IsTrue(e.InputParameters.Contains("S"));
    //  Assert.IsTrue(e.InputParameters.Contains("A"));

    //  Assert.IsTrue(e.OutputParameters.Contains("D"));
    //  Assert.IsTrue(e.OutputParameters.Contains("S"));
    //}


    [Test]
    public void Clone()
    {
      var e = new LinearAccelerationEffector
      {
        DirectionParameter = "D",
        SpeedParameter = "S",
        AccelerationParameter = "A",
        Enabled = false,
      };

      var c = (LinearAccelerationEffector)e.Clone();

      Assert.AreEqual(e.DirectionParameter, c.DirectionParameter);
      Assert.AreEqual(e.SpeedParameter, c.SpeedParameter);
      Assert.AreEqual(e.AccelerationParameter, c.AccelerationParameter);
      Assert.AreEqual(e.Enabled, c.Enabled);
    }


    [Test]
    public void Uninitialize()
    {
      var ps = new ParticleSystem();
      var wp0 = new WeakReference(ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction));
      var wp1 = new WeakReference(ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed));
      var wp2 = new WeakReference(ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.LinearAcceleration));

      var e = new LinearAccelerationEffector { ParticleSystem = ps };

      e.RequeryParameters();
      e.Uninitialize();

      e.ParticleSystem = null;
      ps = null;

      GC.Collect();
      GC.WaitForFullGCComplete();

      Assert.IsFalse(wp0.IsAlive);
      Assert.IsFalse(wp1.IsAlive);
      Assert.IsFalse(wp2.IsAlive);
    }

    [Test]
    public void WrongParameters()
    {
      var ps = new ParticleSystem();
      ps.Parameters.AddUniform<Vector3F>("D");
      ps.Parameters.AddUniform<float>("L");
      ps.Parameters.AddUniform<Vector3F>("A");

      var e = new LinearAccelerationEffector { ParticleSystem = ps };

      var dt = new TimeSpan(0, 0, 0, 1);

      // There should be no exception.
      e.RequeryParameters();
      e.Initialize();
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      e.Uninitialize();
    }


    [Test]
    public void Test0()
    {
      var ps = new ParticleSystem();
      var direction = ps.Parameters.AddUniform<Vector3F>("D");
      var speed = ps.Parameters.AddUniform<float>("L");
      var acceleration = ps.Parameters.AddUniform<Vector3F>("A");

      direction.DefaultValue = new Vector3F(0, 1 , 0);
      speed.DefaultValue = 1;
      acceleration.DefaultValue = new Vector3F(0, -1, 0);

      var e = new LinearAccelerationEffector
      {
        ParticleSystem = ps,
        DirectionParameter = "D",
        SpeedParameter = "L",
        AccelerationParameter = "A",
      };
      
      e.RequeryParameters();
      e.Initialize();

      var dt = new TimeSpan(0, 0, 0, 1);
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);
      
      Assert.AreEqual(new Vector3F(0, 1, 0), direction.DefaultValue);
      Assert.AreEqual(0, speed.DefaultValue);

      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(new Vector3F(0, -1, 0), direction.DefaultValue);
      Assert.AreEqual(1, speed.DefaultValue);
    }


    [Test]
    public void Test1()
    {
      var ps = new ParticleSystem();
      var direction = ps.Parameters.AddVarying<Vector3F>("D");
      var speed = ps.Parameters.AddVarying<float>("L");
      var acceleration = ps.Parameters.AddUniform<Vector3F>("A");

      direction.Values[11] = new Vector3F(0, 1, 0);
      speed.Values[11] = 1;
      acceleration.DefaultValue = new Vector3F(0, -1, 0);

      var e = new LinearAccelerationEffector
      {
        ParticleSystem = ps,
        DirectionParameter = "D",
        SpeedParameter = "L",
        AccelerationParameter = "A",
      };

      e.RequeryParameters();
      e.Initialize();

      var dt = new TimeSpan(0, 0, 0, 1);
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(new Vector3F(0, 1, 0), direction.Values[11]);
      Assert.AreEqual(0, speed.Values[11]);

      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(new Vector3F(0, -1, 0), direction.Values[11]);
      Assert.AreEqual(1, speed.Values[11]);
    }


    [Test]
    public void Test2()
    {
      var ps = new ParticleSystem();
      var direction = ps.Parameters.AddVarying<Vector3F>("D");
      var speed = ps.Parameters.AddVarying<float>("L");
      var acceleration = ps.Parameters.AddVarying<Vector3F>("A");

      direction.Values[11] = new Vector3F(0, 1, 0);
      speed.Values[11] = 1;
      acceleration.Values[11] = new Vector3F(0, -1, 0);

      var e = new LinearAccelerationEffector
      {
        ParticleSystem = ps,
        DirectionParameter = "D",
        SpeedParameter = "L",
        AccelerationParameter = "A",
      };

      e.RequeryParameters();
      e.Initialize();

      var dt = new TimeSpan(0, 0, 0, 1);
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(new Vector3F(0, 1, 0), direction.Values[11]);
      Assert.AreEqual(0, speed.Values[11]);

      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(new Vector3F(0, -1, 0), direction.Values[11]);
      Assert.AreEqual(1, speed.Values[11]);
    }


    [Test]
    public void Test3()
    {
      var ps = new ParticleSystem();
      var direction = ps.Parameters.AddVarying<Vector3F>("D");
      var speed = ps.Parameters.AddUniform<float>("L");
      var acceleration = ps.Parameters.AddVarying<Vector3F>("A");

      direction.Values[11] = new Vector3F(0, 1, 0);
      speed.DefaultValue = 1;
      acceleration.DefaultValue = new Vector3F(0, -1, 0);

      var e = new LinearAccelerationEffector
      {
        ParticleSystem = ps,
        DirectionParameter = "D",
        SpeedParameter = "L",
        AccelerationParameter = "A",
      };

      e.RequeryParameters();
      e.Initialize();

      var dt = new TimeSpan(0, 0, 0, 1);
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      //Assert.AreEqual(new Vector3F(0, 1, 0), direction.Values[11]);
      //Assert.AreEqual(0, speed.DefaultValue);

      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      //Assert.AreEqual(new Vector3F(0, -1, 0), direction.Values[11]);
      //Assert.AreEqual(1, speed.DefaultValue);
    }
  }
}
