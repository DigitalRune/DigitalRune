using System;
using System.Linq;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles.Effectors;
using NUnit.Framework;


namespace DigitalRune.Particles.Tests.Effectors
{
  [TestFixture]
  public class StartValueEffectorTest
  {
    //[Test]
    //public void InputOutputParameters()
    //{
    //  var e = new StartValueEffector<float>()
    //  {
    //    Parameter = "D",
    //  };

    //  Assert.IsTrue(e.InputParameters.Count() == 0);

    //  Assert.IsTrue(e.OutputParameters.Contains("D"));
    //}


    [Test]
    public void Uninitialize()
    {
      var ps = new ParticleSystem();
      var wp0 = new WeakReference(ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction));

      var e = new StartValueEffector<Vector3F> 
      { 
        ParticleSystem = ps,
        Parameter = ParticleParameterNames.Direction
      };

      e.RequeryParameters();
      e.Uninitialize();

      e.ParticleSystem = null;
      ps = null;

      GC.Collect();
      GC.WaitForFullGCComplete();

      Assert.IsFalse(wp0.IsAlive);
    }


    [Test]
    public void WrongParameters()
    {
      var ps = new ParticleSystem();
      ps.Parameters.AddUniform<float>("D");

      var e = new StartValueEffector<float> { ParticleSystem = ps };

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
      var direction = ps.Parameters.AddUniform<float>("D");

      var e = new StartValueEffector<float>
      {
        ParticleSystem = ps,
        Parameter = "D",
      };

      var dt = new TimeSpan(0, 0, 0, 1);

      e.RequeryParameters();
      e.Initialize();
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);
      
      Assert.AreEqual(0, direction.DefaultValue);

      e.Distribution = new ConstValueDistribution<float>(23);

      e.Initialize();
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(23, direction.DefaultValue);
    }


    [Test]
    public void Test1()
    {
      var ps = new ParticleSystem();
      var direction = ps.Parameters.AddVarying<float>("D");

      var e = new StartValueEffector<float>
      {
        ParticleSystem = ps,
        Parameter = "D",
      };

      var dt = new TimeSpan(0, 0, 0, 1);

      e.RequeryParameters();
      e.Initialize();
      e.InitializeParticles(10, 20, null);
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(0, direction.Values[11]);

      e.Distribution = new ConstValueDistribution<float>(23);

      e.Initialize();
      e.InitializeParticles(10, 20, null);
      e.BeginUpdate(dt);
      e.UpdateParticles(dt, 10, 20);
      e.EndUpdate(dt);

      Assert.AreEqual(23, direction.Values[11]);
    }
  }
}
