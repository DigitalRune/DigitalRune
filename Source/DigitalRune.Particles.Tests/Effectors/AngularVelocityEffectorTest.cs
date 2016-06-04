using System;
using System.Linq;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles.Effectors;
using NUnit.Framework;


namespace DigitalRune.Particles.Tests.Effectors
{
  [TestFixture]
  public class AngularVelocityEffectorTest
  {
    [Test]
    public void DefaultValues()
    {
      var e = new AngularVelocityEffector();
      Assert.AreEqual(ParticleParameterNames.Angle, e.AngleParameter);
      Assert.AreEqual(ParticleParameterNames.AngularSpeed, e.SpeedParameter);
    }

    
    [Test]
    public void Clone()
    {
      var e = new AngularVelocityEffector
      {
        AngleParameter = "A",
        SpeedParameter = "S",
        Enabled = false,
      };

      var c = (AngularVelocityEffector)e.Clone();

      Assert.AreEqual(e.AngleParameter, c.AngleParameter);
      Assert.AreEqual(e.SpeedParameter, c.SpeedParameter);
      Assert.AreEqual(e.Enabled, c.Enabled);
    }


    [Test]
    public void Uninitialize()
    {
      var ps = new ParticleSystem();
      var wp0 = new WeakReference(ps.Parameters.AddVarying<float>(ParticleParameterNames.Angle));
      var wp1 = new WeakReference(ps.Parameters.AddVarying<float>(ParticleParameterNames.AngularSpeed));

      var e = new AngularVelocityEffector { ParticleSystem = ps };

      e.RequeryParameters();
      e.Uninitialize();

      e.ParticleSystem = null;
      ps = null;

      GC.Collect();
      GC.WaitForFullGCComplete();

      Assert.IsFalse(wp0.IsAlive);
      Assert.IsFalse(wp1.IsAlive);
    }


    [Test]
    public void WrongParameters()
    {
      var ps = new ParticleSystem();
      ps.Parameters.AddUniform<float>("A");
      ps.Parameters.AddUniform<float>("S");

      var e = new AngularVelocityEffector { ParticleSystem = ps };

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
      var angle = ps.Parameters.AddUniform<float>("A");
      var speed = ps.Parameters.AddUniform<float>("S");

      angle.DefaultValue = 3;
      speed.DefaultValue = 2;

      var e = new AngularVelocityEffector
      {
        ParticleSystem = ps,
        AngleParameter = "A",
        SpeedParameter = "S",
      };
      
      e.RequeryParameters();
      e.Initialize();

      var dt = new TimeSpan(0, 0, 0, 1);
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);
      
      Assert.AreEqual(5, angle.DefaultValue);

      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(7 % ConstantsF.TwoPi, angle.DefaultValue);

      speed.DefaultValue = -17;

      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.IsTrue(Numeric.AreEqual(-10 + 2 * ConstantsF.TwoPi, angle.DefaultValue));
    }

    [Test]
    public void Test1()
    {
      var ps = new ParticleSystem();
      var angle = ps.Parameters.AddVarying<float>("A");
      var speed = ps.Parameters.AddUniform<float>("S");

      angle.Values[11] = 3;
      speed.DefaultValue = 2;

      var e = new AngularVelocityEffector
      {
        ParticleSystem = ps,
        AngleParameter = "A",
        SpeedParameter = "S",
      };

      e.RequeryParameters();
      e.Initialize();

      var dt = new TimeSpan(0, 0, 0, 1);
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(5, angle.Values[11]);

      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(7 % ConstantsF.TwoPi, angle.Values[11]);

      speed.DefaultValue = -17;

      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.IsTrue(Numeric.AreEqual(-10 + 2 * ConstantsF.TwoPi, angle.Values[11]));
    }


    [Test]
    public void Test2()
    {
      var ps = new ParticleSystem();
      var angle = ps.Parameters.AddVarying<float>("A");
      var speed = ps.Parameters.AddVarying<float>("S");

      angle.Values[11] = 3;
      speed.Values[11] = 2;

      var e = new AngularVelocityEffector
      {
        ParticleSystem = ps,
        AngleParameter = "A",
        SpeedParameter = "S",
      };

      e.RequeryParameters();
      e.Initialize();

      var dt = new TimeSpan(0, 0, 0, 1);
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(5, angle.Values[11]);

      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(7 % ConstantsF.TwoPi, angle.Values[11]);

      speed.Values[11] = -17;

      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.IsTrue(Numeric.AreEqual(-10 + 2 * ConstantsF.TwoPi, angle.Values[11]));
    }
  }
}
