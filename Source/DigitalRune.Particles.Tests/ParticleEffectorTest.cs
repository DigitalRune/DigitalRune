using System;
using NUnit.Framework;


namespace DigitalRune.Particles.Tests
{
  [TestFixture]
  public class ParticleEffectorTest
  {
    class MyEffector : ParticleEffector
    {
      protected override void OnInitializeParticles(int startIndex, int count, object emitter)
      {
        for (int i = startIndex; i < startIndex + count; i++)
          ParticleSystem.Parameters.Get<int>("V").Values[i] = 77;
      }

      protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
      {
        for (int i = startIndex; i < startIndex + count; i++)
          ParticleSystem.Parameters.Get<int>("V").Values[i] = 99;
      }

      protected override ParticleEffector CreateInstanceCore()
      {
        return new MyEffector();
      }
    }


    [Test]
    public void InitializeWithoutWrapAround()
    {
      var ps = new ParticleSystem();
      var pv = ps.Parameters.AddVarying<int>("V");

      var e = new MyEffector { ParticleSystem = ps };

      e.InitializeParticles(10, 20, null);

      for (int i = 0; i < ps.MaxNumberOfParticles; i++)
      {
        if (i >= 10 && i < 30)
          Assert.AreEqual(77, pv.Values[i]);
        else
          Assert.AreEqual(0, pv.Values[i]);
      }
    }


    [Test]
    public void InitializeWithWrapAround()
    {
      var ps = new ParticleSystem();
      var pv = ps.Parameters.AddVarying<int>("V");

      var e = new MyEffector { ParticleSystem = ps };

      e.InitializeParticles(90, 20, null);

      for (int i = 0; i < ps.MaxNumberOfParticles; i++)
      {
        if (i < 10 || i >= 90)
          Assert.AreEqual(77, pv.Values[i]);
        else
          Assert.AreEqual(0, pv.Values[i]);
      }
    }


    [Test]
    public void UpdateWithoutWrapAround()
    {
      var ps = new ParticleSystem();
      var pv = ps.Parameters.AddVarying<int>("V");

      var e = new MyEffector { ParticleSystem = ps };

      e.UpdateParticles(new TimeSpan(1), 10, 20);

      for (int i = 0; i < ps.MaxNumberOfParticles; i++)
      {
        if (i >= 10 && i < 30)
          Assert.AreEqual(99, pv.Values[i]);
        else
          Assert.AreEqual(0, pv.Values[i]);
      }
    }


    [Test]
    public void UpdateWithWrapAround()
    {
      var ps = new ParticleSystem();
      var pv = ps.Parameters.AddVarying<int>("V");

      var e = new MyEffector { ParticleSystem = ps };

      e.UpdateParticles(new TimeSpan(1), 90, 20);

      for (int i = 0; i < ps.MaxNumberOfParticles; i++)
      {
        if (i < 10 || i >= 90)
          Assert.AreEqual(99, pv.Values[i]);
        else
          Assert.AreEqual(0, pv.Values[i]);
      }
    }
  }
}
