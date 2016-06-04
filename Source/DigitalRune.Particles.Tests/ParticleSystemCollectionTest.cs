using NUnit.Framework;


namespace DigitalRune.Particles.Tests
{
  [TestFixture]
  public class ParticleSystemCollectionTest
  {
    [Test]
    public void Test()
    {
      var m = new ParticleSystemManager();

      var parent = new ParticleSystem() { Service = m };
      var a = new ParticleSystem();
      var b = new ParticleSystem();
      var c = new ParticleSystem();

      var psc = new ParticleSystemCollection();
      parent.Children = psc;

      psc.Add(a);

      Assert.AreEqual(parent, a.Parent);
      Assert.AreEqual(m, a.Service);

      psc[0] = b;

      Assert.AreEqual(null, a.Parent);
      Assert.AreEqual(null, a.Service);
      Assert.AreEqual(parent, b.Parent);
      Assert.AreEqual(m, b.Service);

      psc.Add(a);

      psc.Remove(b);
      Assert.AreEqual(null, b.Parent);
      Assert.AreEqual(null, b.Service);

      psc.Clear();
      Assert.AreEqual(null, a.Parent);
      Assert.AreEqual(null, a.Service);
    }
  }
}
