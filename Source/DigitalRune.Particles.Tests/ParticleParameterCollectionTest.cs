using System;
using System.Linq;
using NUnit.Framework;


namespace DigitalRune.Particles.Tests
{
  [TestFixture]
  public class ParticleParameterCollectionTest
  {
    [Test]
    public void Constructor()
    {
      Assert.Throws<ArgumentNullException>(() => new ParticleParameterCollection(null));
    }

    [Test]
    public void Changed()
    {
      bool changed = false;

      var ppc = new ParticleParameterCollection(new ParticleSystem());
      ppc.Changed += (s, e) => changed = true;

      ppc.Clear();
      Assert.IsFalse(changed);

      ppc.AddVarying<float>("D");
      Assert.IsTrue(changed);

      changed = false;
      ppc.AddUniform<float>("F");
      Assert.IsTrue(changed);

      changed = false;
      ppc.AddUniform<float>("F");
      Assert.IsFalse(changed);

      changed = false;
      ppc.AddVarying<float>("F");
      Assert.IsTrue(changed);

      changed = false;
      ppc.AddUniform<float>("F");
      Assert.IsFalse(changed);

      changed = false;
      ppc.AddVarying<float>("F");
      Assert.IsFalse(changed);

      changed = false;
      ppc.Remove("D");
      Assert.IsTrue(changed);

      changed = false;
      ppc.Clear();
      Assert.IsTrue(changed);
    }


    [Test]
    public void Clear()
    {
      var ppc = new ParticleParameterCollection(new ParticleSystem());

      ppc.Clear();
      Assert.AreEqual(0, ppc.Count());

      ppc.AddVarying<float>("A");
      ppc.AddUniform<float>("B");

      ppc.Clear();
      Assert.AreEqual(0, ppc.Count());
    }


    [Test]
    public void Contains()
    {
      var ppc = new ParticleParameterCollection(new ParticleSystem());

      Assert.IsFalse(ppc.Contains("A"));

      ppc.AddVarying<float>("A");
      ppc.AddUniform<float>("B");

      Assert.IsTrue(ppc.Contains("A"));
      Assert.IsFalse(ppc.Contains("C"));      
    }


    [Test]
    public void Add()
    {
      var ppc = new ParticleParameterCollection(new ParticleSystem());

      Assert.Throws<ArgumentNullException>(() => ppc.AddUniform<float>(null));
      Assert.Throws<ArgumentException>(() => ppc.AddUniform<float>(""));
      Assert.Throws<ArgumentNullException>(() => ppc.AddVarying<float>(null));
      Assert.Throws<ArgumentException>(() => ppc.AddVarying<float>(""));

      var pu = ppc.AddUniform<float>("F");
      Assert.IsTrue(pu.Values == null);
      Assert.Throws<ParticleSystemException>(() => ppc.AddUniform<int>("F"));
      Assert.Throws<ParticleSystemException>(() => ppc.AddVarying<int>("F"));

      var pv = ppc.AddVarying<float>("V");
      Assert.IsTrue(pv.Values != null);
      Assert.Throws<ParticleSystemException>(() => ppc.AddUniform<int>("V"));
      Assert.Throws<ParticleSystemException>(() => ppc.AddVarying<int>("V"));

      // Test overriding.
      Assert.AreEqual(pu, ppc.AddUniform<float>("F"));
      Assert.AreEqual(pv, ppc.AddUniform<float>("V"));
      Assert.AreEqual(pv, ppc.AddVarying<float>("V"));

      var pvv = ppc.AddVarying<float>("F");
      Assert.AreNotEqual(pv, pvv);
    }


    [Test]
    public void Get()
    {
      var ppc = new ParticleParameterCollection(new ParticleSystem());
      var pu = ppc.AddUniform<float>("U");
      var pv = ppc.AddVarying<float>("V");

      Assert.AreEqual(null, ppc.Get<float>(null));
      Assert.AreEqual(null, ppc.Get<float>(""));
      Assert.AreEqual(pu, ppc.Get<float>("U"));
      Assert.AreEqual(pv, ppc.Get<float>("V"));
      Assert.Throws<ParticleSystemException>(() => ppc.Get<int>("U"));
    }


    [Test]
    public void TryGet()
    {
      var ppc = new ParticleParameterCollection(new ParticleSystem());
      var pu = ppc.AddUniform<float>("U");
      var pv = ppc.AddVarying<float>("V");

      Assert.AreEqual(null, ppc.GetUnchecked<float>(null));
      Assert.AreEqual(null, ppc.GetUnchecked<float>(""));
      Assert.AreEqual(pu, ppc.GetUnchecked<float>("U"));
      Assert.AreEqual(pv, ppc.GetUnchecked<float>("V"));
      Assert.AreEqual(null, ppc.GetUnchecked<int>("V"));
    }


    [Test]
    public void Remove()
    {
      var ppc = new ParticleParameterCollection(new ParticleSystem());
      var pu = ppc.AddUniform<float>("U");
      var pv = ppc.AddVarying<float>("V");

      Assert.AreEqual(false, ppc.Remove(null));
      Assert.AreEqual(false, ppc.Remove(""));
      Assert.AreEqual(true, ppc.Remove("U"));
      Assert.AreEqual(false, ppc.Remove("U"));
      Assert.AreEqual(true, ppc.Remove("V"));
      Assert.AreEqual(false, ppc.Remove("V"));
    }


    [Test]
    public void UpdateArrays()
    {
      var ppc = new ParticleParameterCollection(new ParticleSystem());
      var pu = ppc.AddUniform<float>("U");
      var pv = ppc.AddVarying<float>("V");

      ppc.ParticleSystem.MaxNumberOfParticles = 71;
      ppc.UpdateArrayLength();
      Assert.AreEqual(71, pv.Values.Length);
    }
  }
}
