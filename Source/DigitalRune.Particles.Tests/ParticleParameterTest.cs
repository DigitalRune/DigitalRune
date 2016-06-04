using System;
using NUnit.Framework;


namespace DigitalRune.Particles.Tests
{
  [TestFixture]
  public class ParticleParameterTest
  {   
    [Test]
    public void UniformParticleParameter()
    {
      Assert.Throws<ArgumentNullException>(() => new UniformParticleParameter<float>(null));

      IParticleParameter<float> p = new UniformParticleParameter<float>("Size");
      Assert.AreEqual("Size", p.Name);
      Assert.IsNull(p.Values);

      p.DefaultValue = 10;
      Assert.AreEqual(10, p.DefaultValue);
    }


    [Test]
    public void VaryingParticleParameter()
    {
      Assert.Throws<ArgumentNullException>(() => new VaryingParticleParameter<float>(null, 100));
      Assert.Throws<ArgumentOutOfRangeException>(() => new VaryingParticleParameter<float>("Size", -10));

      IParticleParameter<float> p = new VaryingParticleParameter<float>("Size", 100);
      Assert.AreEqual("Size", p.Name);
      Assert.AreEqual(100, p.Values.Length);

      p.DefaultValue = 10;
      Assert.AreEqual(10, p.DefaultValue);
    }
  }
}
